using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using KSP.Localization;
using UniversalStorage2.StockVariants;

namespace UniversalStorage2
{
    public class USSwitchControl : PartModule
    {
        private const string ICON_TAG = "Icon_Hidden";
        private const string UNTAGGED = "Untagged";

        [KSPField]
        public int SwitchID = -1;
        [KSPField]
        public string ObjectNames = string.Empty;
        [KSPField]
        public string VariantColors = string.Empty;
        [KSPField]
        public bool ShowInfo = true;
        [KSPField]
        public bool UpdateSymmetry = true;
        [KSPField]
        public bool FuelSwitchModeOne = false;
        [KSPField]
        public bool FuelSwitchModeTwo = false;
        [KSPField]
        public bool DebugDragCube = false;
        [KSPField(isPersistant = true)]
        public int CurrentSelection = 0;
        [KSPField]
        public string CurrentVariantTitle = "Current Variant";
        [KSPField]
        public string ModuleDisplayName = "Switch Control";

        [KSPField, UI_USVariantSelector(affectSymCounterparts = UI_Scene.Editor, controlEnabled = true, scene = UI_Scene.Editor)]
        private int VariantSelection = 0;
        
        private List<USVariantInfo> _variantInfoList = new List<USVariantInfo>();
        
        private string[] SwitchNames;
        private string[] _localizedSwitchNames;
        private string[] SwitchColors;

        private EventData<int, int, Part> onUSSwitch;
        private EventData<int, int, bool, Part> onUSFuelSwitch;

        private EventData<int, int, AvailablePart, Transform> onUSEditorIconSwitch;
        
        private string _localizedCurrentVariantString = "Current Variant";

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            //USdebugMessages.USStaticLog("Load US Switch Control Module");
            
            LoadSwitchData();

            if (!HighLogic.LoadedSceneIsFlight && !HighLogic.LoadedSceneIsEditor)
            {
                //USdebugMessages.USStaticLog("Find Editor Icon Switch Event");
                onUSEditorIconSwitch = GameEvents.FindEvent<EventData<int, int, AvailablePart, Transform>>("onUSEditorIconSwitch");
            }
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            
            _localizedCurrentVariantString = Localizer.Format(CurrentVariantTitle);
            
            Events["RenderDragCube"].active = DebugDragCube;
            
            if (SwitchID < 0)
            {
                for (int i = part.Modules.Count - 1; i >= 0; i--)
                {
                    if (part.Modules[i] == this)
                    {
                        SwitchID = i;
                        break;
                    }
                }
            }

            if (FuelSwitchModeOne || FuelSwitchModeTwo)
                UpdateSymmetry = true;
            
            onUSSwitch = GameEvents.FindEvent<EventData<int, int, Part>>("onUSSwitch");

            onUSFuelSwitch = GameEvents.FindEvent<EventData<int, int, bool, Part>>("onUSFuelSwitch");
            
            if (state == StartState.Editor)
            {
                LoadSwitchData();

                _variantInfoList.Clear();

                for (int i = 0; i < SwitchNames.Length; i++)
                {
                    _localizedSwitchNames[i] = Localizer.Format(SwitchNames[i]);

                    string primary = "#3a562a";
                    string secondary = "#999999";

                    if (SwitchColors != null && i < SwitchColors.Length)
                    {
                        string[] colors = SwitchColors[i].Split(',');

                        if (colors.Length == 2)
                        {
                            primary = colors[0];
                            secondary = colors[1];
                        }
                    }

                    _variantInfoList.Add(new USVariantInfo(_localizedCurrentVariantString, _localizedSwitchNames[i], primary, secondary));
                }

                UI_USVariantSelector variantSelector = Fields["VariantSelection"].uiControlEditor as UI_USVariantSelector;

                variantSelector.onFieldChanged = OnSwitch;
                variantSelector.onSymmetryFieldChanged = OnSymmetrySwitch;

                variantSelector.Variants = _variantInfoList;
            }
        }
        
        private void LoadSwitchData()
        {
            if (!String.IsNullOrEmpty(ObjectNames))
                SwitchNames = ObjectNames.Split(';');

            if (!String.IsNullOrEmpty(VariantColors))
                SwitchColors = VariantColors.Split(';');

            if (SwitchNames == null || SwitchNames.Length == 0)
                return;

            _localizedSwitchNames = new string[SwitchNames.Length];

            if (CurrentSelection >= _localizedSwitchNames.Length)
                CurrentSelection = _localizedSwitchNames.Length - 1;

            if (CurrentSelection < 0)
                CurrentSelection = 0;

            VariantSelection = CurrentSelection;
        }
        
        public override void OnStartFinished(StartState state)
        {
            base.OnStartFinished(state);

            if (state == StartState.Editor)
                StartCoroutine(WaitForStart());
        }

        private IEnumerator WaitForStart()
        {
            yield return new WaitForEndOfFrame();

            Switch();
        }

        public bool HasSwitches()
        {
            return SwitchNames != null && SwitchNames.Length > 0;
        }

        public override void OnIconCreate()
        {
            StripTag<MeshRenderer>(part.gameObject, ICON_TAG);
            StripTag<MeshFilter>(part.gameObject, ICON_TAG);
            StripTag<SkinnedMeshRenderer>(part.gameObject, ICON_TAG);
        }

        private void StripTag<T>(GameObject obj, string tag) where T : Component
        {
            var components = obj.GetComponentsInChildren<T>();

            for (int i = components.Length - 1; i >= 0; i--)
            {
                if (components[i].gameObject.tag == tag)
                {
                    components[i].gameObject.tag = UNTAGGED;
                }
            }
        }

        public override string GetInfo()
        {
            if (ShowInfo && !String.IsNullOrEmpty(ObjectNames))
            {
                string[] variantList = USTools.parseNames(ObjectNames, false, true, String.Empty).ToArray();

                StringBuilder info = StringBuilderCache.Acquire();
                info.AppendLine(Localizer.Format("#autoLOC_US_PartVariants"));

                for (int i = 0; i < variantList.Length; i++)
                {
                    info.AppendLine(Localizer.Format(variantList[i]));
                }

                return info.ToStringAndRelease();
            }
            else
                return base.GetInfo();
        }

        public override string GetModuleDisplayName()
        {
            return Localizer.Format(ModuleDisplayName);
        }
        
        public void EditorToggleVariant(AvailablePart partInfo, Transform partIcon, bool iterate)
        {
            //USdebugMessages.USStaticLog("Fire editor icon toggle: {0} - Iterate: {1}", partInfo.title, iterate);

            if (iterate)
                CurrentSelection++;

            if (CurrentSelection >= _localizedSwitchNames.Length)
                CurrentSelection = 0;

            if (CurrentSelection < 0)
                CurrentSelection = _localizedSwitchNames.Length - 1;

            if (onUSEditorIconSwitch != null)
            {
                //USdebugMessages.USStaticLog("Fire editor switch");
                onUSEditorIconSwitch.Fire(SwitchID, CurrentSelection, partInfo, partIcon);
            }
        }

        private void OnSwitch(BaseField field, object obj)
        {
            CurrentSelection = VariantSelection;

            SwitchTo(true);
        }

        private void OnSymmetrySwitch(BaseField field, object obj)
        {
            //CurrentSelection = VariantSelection;

            //SwitchTo(true);
        }

        private void SwitchTo(bool player)
        {
            if (_localizedSwitchNames == null || _localizedSwitchNames.Length == 0)
                return;

            Switch();

            if (UpdateSymmetry && player)
            {
                for (int i = 0; i < part.symmetryCounterparts.Count; i++)
                {
                    USSwitchControl[] symSwitch = part.symmetryCounterparts[i].GetComponents<USSwitchControl>();

                    for (int j = 0; j < symSwitch.Length; j++)
                    {
                        if (symSwitch[j].SwitchID == SwitchID)
                        {
                            symSwitch[j].CurrentSelection = CurrentSelection;
                            symSwitch[j].Switch();
                        }
                    }
                }
            }
        }

        private void Switch()
        {
            onUSSwitch.Fire(SwitchID, CurrentSelection, part);

            if (FuelSwitchModeOne)
                onUSFuelSwitch.Fire(SwitchID, CurrentSelection, true, part);
            else if (FuelSwitchModeTwo)
                onUSFuelSwitch.Fire(SwitchID, CurrentSelection, false, part);
            
            GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
        }

        [KSPEvent(name = "RenderDragCube", guiName = "Render Drag Cube", active = false, guiActive = true, guiActiveEditor = true)]
        public void RenderDragCube()
        {
            DragCube cube = DragCubeSystem.Instance.RenderProceduralDragCube(part);

            PrintCube(cube.Area, cube.Drag, cube.Depth, cube.Center, cube.Size);
        }

        private void PrintCube(float[] area, float[] drag, float[] depth, Vector3 center, Vector3 bounds)
        {
            List<string> cubeStrings = new List<string>();

            cubeStrings.Add(CurrentSelection.ToString());

            for (int i = 0; i < 6; i++)
            {
                cubeStrings.Add(string.Format("{0:F4},{1:F4},{2:F4}", area[i], drag[i], depth[i]));
            }

            cubeStrings.Add(string.Format("{0:F4},{1:F4},{2:F4}", center.x, center.y, center.z));
            
            cubeStrings.Add(string.Format("{0:F4},{1:F4},{2:F4}", bounds.x, bounds.y, bounds.z));

            StringBuilder sb = StringBuilderCache.Acquire(256);

            for (int i = 0; i < 9; i++)
            {
                sb.Append(cubeStrings[i]);
                if (i < 8)
                    sb.Append(", ");
            }

            USdebugMessages.USStaticLog(sb.ToStringAndRelease());
        }
    }
}
