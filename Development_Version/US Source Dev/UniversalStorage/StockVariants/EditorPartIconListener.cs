
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using KSP.UI.Screens;

namespace UniversalStorage2.StockVariants
{
    public class EditorPartIconListener : MonoBehaviour
    {
        private EditorPartIcon icon;

        private bool _activeSwitcher;

        private Transform _partIconTransform;
        private AvailablePart _partInfo;

        private EventData<AvailablePart> onPrimaryVariantSwitched = GameEvents.FindEvent
            <EventData<AvailablePart>>("onUSEditorPrimaryVariantSwitched");

        private EventData<AvailablePart> onSecondaryVariantSwitched = GameEvents.FindEvent
            <EventData<AvailablePart>>("onUSEditorSecondaryVariantSwitched");

        private void Start()
        {
            if (!HighLogic.LoadedSceneIsEditor)
                return;

            icon = GetComponentInParent<EditorPartIcon>();

            PartIconSpawn();

            //USVariantController.OnEditorPartIconSpawn.Invoke(icon);
        }

        private void OnDestroy()
        {
            if (onSecondaryVariantSwitched != null)
                onSecondaryVariantSwitched.Remove(OnSecondaryVariantSwitch);

            if (onPrimaryVariantSwitched != null)
                onPrimaryVariantSwitched.Remove(OnPrimaryVariantSwitch);
        }

        private void PartIconSpawn()
        {
            //USdebugMessages.USStaticLog("Parsing Editor Icon...");

            if (icon == null)
                return;

            if (!icon.isPart)
                return;

            if (icon.partInfo.partPrefab == null)
                return;

            if (icon.partInfo == null)
                return;

            bool flag = false;

            for (int i = icon.partInfo.moduleInfos.Count - 1; i >= 0; i--)
            {
                if (icon.partInfo.moduleInfos[i].moduleName == "USSwitch Control")
                {
                    //USdebugMessages.USStaticLog("USSwitchControl Module Found");

                    flag = true;
                    break;
                }
            }

            if (!flag)
                return;

            var switches = icon.partInfo.partPrefab.Modules.GetModules<USSwitchControl>();

            //USdebugMessages.USStaticLog("Parsing Switch Control Modules: {0}", switches.Count);

            flag = false;

            bool secondary = false;

            for (int i = 0; i < switches.Count; i++)
            {
                if (switches[i].HasSwitches())
                {
                    if (i == 0)
                    {
                        flag = true;

                        //USdebugMessages.USStaticLog("Primary Switch Control Found");

                        USVariantController.Instance.AddSwitchControl(icon.partInfo, switches[i], true);
                    }
                    else if (i == 1)
                    {
                        secondary = true;

                        //USdebugMessages.USStaticLog("Secondary Switch Control Found");

                        USVariantController.Instance.AddSwitchControl(icon.partInfo, switches[i], false);
                    }
                }
            }

            if (!flag)
                return;

            _partInfo = icon.partInfo;

            //USdebugMessages.USStaticLog("Activating US Switch icon buttons");

            icon.btnSwapTexture.gameObject.SetActive(true);

            icon.btnSwapTexture.onClick.RemoveAllListeners();

            //USdebugMessages.USStaticLog("Editor icon: {0}", icon.partInfo.iconPrefab.name);

            string clone = _partInfo.iconPrefab.name + "(Clone)";
            
            var children = icon.GetComponentsInChildren<Transform>(true);

            for (int i = children.Length - 1; i >= 0; i--)
            {
                if (children[i].name == clone)
                {
                    // USdebugMessages.USStaticLog("Found Editor icon: {0}", children[i].name);

                    _partIconTransform = children[i];

                    break;
                }
            }

            if (_partIconTransform == null)
                return;

            if (secondary)
            {
                Button secondaryButton = Instantiate(icon.btnSwapTexture, icon.btnSwapTexture.transform.parent, false);

                RectTransform secondRect = secondaryButton.GetComponent<RectTransform>();

                secondRect.anchorMin = new Vector2(1, 0);
                secondRect.anchorMax = new Vector2(1, 0);

                secondRect.anchoredPosition = new Vector2(-4, secondRect.anchoredPosition.y);

                secondaryButton.onClick.AddListener(delegate { ToggleSecondaryVariant(_partInfo); });

                if (switches != null && switches.Count > 1)
                    switches[1].EditorToggleVariant(_partInfo, _partIconTransform, false);

                if (onSecondaryVariantSwitched != null)
                    onSecondaryVariantSwitched.Add(OnSecondaryVariantSwitch);
            }

            icon.btnSwapTexture.onClick.AddListener(delegate { TogglePrimaryVariant(_partInfo); });

            if (switches != null && switches.Count > 0)
                switches[0].EditorToggleVariant(_partInfo, _partIconTransform, false);

            //USdebugMessages.USStaticLog("Original Icon: {7}: Scale: {0:F4} - Rotation: {1:F4} - Position: {2:F4}\nChild: {6}: Scale: {3:F4} - Rotation: {4:F4} - Position: {5:F4}"
            //    , _partIconTransform.localScale, _partIconTransform.localRotation, _partIconTransform.localPosition
            //    , _partIconTransform.GetChild(0).localScale, _partIconTransform.GetChild(0).localRotation, _partIconTransform.GetChild(0).localPosition
            //    , _partIconTransform.GetChild(0).name, _partIconTransform.name);

            UpdateIconScale();

            if (onPrimaryVariantSwitched != null)
                onPrimaryVariantSwitched.Add(OnPrimaryVariantSwitch);
        }

        private void UpdateIconScale()
        {
            if (_partIconTransform == null)
                return;

            if (_partIconTransform.childCount < 1)
                return;
            
            Transform scaler = _partIconTransform.GetChild(0);

            Bounds bounder = USTools.GetActivePartBounds(scaler.gameObject);

            float scale = USTools.GetBoundsScale(bounder);

            //USdebugMessages.USStaticLog("New bounds scale: {0}\nIcon Scale: {1}\nBounds Center: {2}\nBounds X: {3} Y: {4} Z: {5}"
            //    , scale.ToString("F4"), _partInfo.iconScale.ToString("F4"), bounder.center.ToString("F3")
            //    , bounder.size.x.ToString("F3"), bounder.size.y.ToString("F3"), bounder.size.y.ToString("F3"));

            scaler.localScale = Vector3.one * scale;
            scaler.localPosition = new Vector3(scaler.localPosition.x, (bounder.center * scale * -1f).y, scaler.localPosition.z);

            //USdebugMessages.USStaticLog("New Icon: {7}: Scale: {0:F4} - Rotation: {1:F4} - Position: {2:F4}\nChild: {6}: Scale: {3:F4} - Rotation: {4:F4} - Position: {5:F4}"
            //    , _partIconTransform.localScale, _partIconTransform.localRotation, _partIconTransform.localPosition
            //    , scaler.localScale, scaler.localRotation, scaler.localPosition, scaler.name, _partIconTransform.name);
        }
        
        private void TogglePrimaryVariant(AvailablePart partInfo)
        {
            USSwitchControl switchControl = USVariantController.Instance.GetSwitchControl(partInfo, true);

            if (switchControl == null)
                return;

            if (_partInfo == null || partInfo != _partInfo)
                return;

            if (_partIconTransform == null)
                return;

            _activeSwitcher = true;

            //USdebugMessages.USStaticLog("Toggle primary variant event for icon: {0}", partInfo.title);

            switchControl.EditorToggleVariant(partInfo, _partIconTransform, true);

            if (onPrimaryVariantSwitched != null)
                onPrimaryVariantSwitched.Fire(partInfo);

            _activeSwitcher = false;

            UpdateIconScale();

            //USdebugMessages.USStaticLog("Select variant from Editor Icon: {0} - Name: {1}", partInfo.title, partIcon.name);
        }

        private void ToggleSecondaryVariant(AvailablePart partInfo)
        {
            USSwitchControl switchControl = USVariantController.Instance.GetSwitchControl(partInfo, false);

            if (switchControl == null)
                return;

            if (_partInfo == null || partInfo != _partInfo)
                return;

            if (_partIconTransform == null)
                return;

            _activeSwitcher = true;

            //USdebugMessages.USStaticLog("Toggle secondary variant event for icon: {0}", partInfo.title);

            switchControl.EditorToggleVariant(partInfo, _partIconTransform, true);

            if (onSecondaryVariantSwitched != null)
                onSecondaryVariantSwitched.Fire(partInfo);

            _activeSwitcher = false;

            UpdateIconScale();

            //USdebugMessages.USStaticLog("Select variant from secondary Editor Icon: {0} - Name: {1}", partInfo.title, partIcon.name);
        }

        private void OnPrimaryVariantSwitch(AvailablePart partInfo)
        {
            if (_activeSwitcher)
                return;

            if (_partInfo == null || partInfo != _partInfo)
                return;

            if (_partIconTransform == null)
                return;

            USSwitchControl switchControl = USVariantController.Instance.GetSwitchControl(partInfo, true);

            if (switchControl == null)
                return;

            //USdebugMessages.USStaticLog("Fire primary variant event for icon: {0}", partInfo.title);

            switchControl.EditorToggleVariant(partInfo, _partIconTransform, false);

            UpdateIconScale();
        }

        private void OnSecondaryVariantSwitch(AvailablePart partInfo)
        {
            if (_activeSwitcher)
                return;

            if (_partInfo == null || partInfo != _partInfo)
                return;

            if (_partIconTransform == null)
                return;

            USSwitchControl switchControl = USVariantController.Instance.GetSwitchControl(partInfo, false);

            if (switchControl == null)
                return;

            //USdebugMessages.USStaticLog("Fire secondary variant event for icon: {0}", partInfo.title);

            switchControl.EditorToggleVariant(partInfo, _partIconTransform, false);

            UpdateIconScale();
        }
    }
}
