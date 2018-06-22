using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace UniversalStorage
{
    public class USSwitchControl : PartModule
    {
        [KSPField]
        public int SwitchID = -1;
        [KSPField]
        public string ButtonName = "Next part variant";
        [KSPField]
        public string PreviousButtonName = "Prev part variant";
        [KSPField]
        public string ObjectNames = string.Empty;
        [KSPField]
        public bool ShowPreviousButton = true;
        [KSPField]
        public bool ShowInfo = true;
        [KSPField]
        public bool UpdateSymmetry = true;
        [KSPField]
        public bool FuelSwitchModeOne = false;
        [KSPField]
        public bool FuelSwitchModeTwo = false;
        [KSPField]
        public bool DebugDragCube = true;
        [KSPField(isPersistant = true)]
        public int CurrentSelection = 0;
        [KSPField(guiActiveEditor = true, guiName = "Current Variant")]
        public string CurrentObjectName = string.Empty;

        private string[] SwitchNames;
        private EventData<int, int, Part> onUSSwitch;
        private EventData<int, int, bool, Part> onUSFuelSwitch;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            Events["nextObjectEvent"].guiName = ButtonName;
            Events["previousObjectEvent"].guiName = PreviousButtonName;

            Events["RenderDragCube"].active = DebugDragCube;

            if (!ShowPreviousButton)
                Events["previousObjectEvent"].guiActiveEditor = false;
            
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

            if (!String.IsNullOrEmpty(ObjectNames))
                SwitchNames = ObjectNames.Split(';');

            onUSSwitch = GameEvents.FindEvent<EventData<int, int, Part>>("onUSSwitch");

            onUSFuelSwitch = GameEvents.FindEvent<EventData<int, int, bool, Part>>("onUSFuelSwitch");

            if (SwitchNames == null || SwitchNames.Length == 0)
                return;

            if (CurrentSelection >= SwitchNames.Length)
                CurrentSelection = SwitchNames.Length - 1;

            if (CurrentSelection < 0)
                CurrentSelection = 0;

            CurrentObjectName = SwitchNames[CurrentSelection];
        }

        public override string GetInfo()
        {
            if (ShowInfo && !String.IsNullOrEmpty(ObjectNames))
            {
                string[] variantList = USTools.parseNames(ObjectNames, false, true, String.Empty).ToArray();

                StringBuilder info = StringBuilderCache.Acquire();
                info.AppendLine("Part variants available:");

                for (int i = 0; i < variantList.Length; i++)
                {
                    info.AppendLine(variantList[i]);
                }

                return info.ToStringAndRelease();
            }
            else
                return base.GetInfo();
        }

        [KSPEvent(guiActive = false, guiActiveEditor = true, guiActiveUnfocused = false, guiName = "Next part variant")]
        public void nextObjectEvent()
        {
            CurrentSelection++;

            if (CurrentSelection >= SwitchNames.Length)
                CurrentSelection = 0;

            SwitchTo(true);
        }

        [KSPEvent(guiActive = false, guiActiveEditor = true, guiActiveUnfocused = false, guiName = "Prev part variant")]
        public void previousObjectEvent()
        {
            CurrentSelection--;

            if (CurrentSelection < 0)
                CurrentSelection = SwitchNames.Length - 1;

            SwitchTo(true);
        }

        private void SwitchTo(bool player)
        {
            if (SwitchNames == null || SwitchNames.Length == 0)
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

            CurrentObjectName = SwitchNames[CurrentSelection];

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
                sb.Append(", ");
            }

            USdebugMessages.USStaticLog(sb.ToStringAndRelease());
        }
    }
}
