
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KSP.UI;
using KSP.UI.Screens;
using KSP.UI.Screens.Editor;
using TMPro;

namespace UniversalStorage2.StockVariants
{
    [KSPAddon(KSPAddon.Startup.FlightEditorAndKSC, false)]
    public class USVariantController : MonoBehaviour
    {
        private static bool _iconPrefabProcessed;
        private static bool _rdPrefabProcessed;
        //private static bool _tooltipPrefabProcessed;

        private bool _activeSwitcher;
        private Transform _currentTooltipTransform;
        private AvailablePart _currentPartInfo;

        private Dictionary<AvailablePart, USSwitchControl> _primarySwitches = new Dictionary<AvailablePart, USSwitchControl>();
        private Dictionary<AvailablePart, USSwitchControl> _secondarySwitches = new Dictionary<AvailablePart, USSwitchControl>();

        private EventData<AvailablePart> onPrimaryVariantSwitched = GameEvents.FindEvent
            <EventData<AvailablePart>>("onUSEditorPrimaryVariantSwitched");

        private EventData<AvailablePart> onSecondaryVariantSwitched = GameEvents.FindEvent
            <EventData<AvailablePart>>("onUSEditorSecondaryVariantSwitched");

        private static USVariantController _instance;

        public static USVariantController Instance
        {
            get { return _instance; }
        }

        private void Awake()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                Destroy(gameObject);
                return;
            }

            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            if (!_iconPrefabProcessed && HighLogic.LoadedSceneIsEditor)
                ProcessEditorIconPrefab();

            if (!_rdPrefabProcessed && HighLogic.LoadedScene == GameScenes.SPACECENTER)
                GameEvents.onGUIRnDComplexSpawn.Add(RnDSpawn);

            //if (!_tooltipPrefabProcessed)
            //    ProcessEditorTooltipPrefab();

            if (HighLogic.LoadedSceneIsEditor)
                ProcessUIPrefab();

            GameEvents.onTooltipSpawned.Add(OnTooltipSpawned);
            GameEvents.onTooltipDespawned.Add(OnTooltipDespawn);
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;

            GameEvents.onTooltipSpawned.Remove(OnTooltipSpawned);
            GameEvents.onTooltipDespawned.Remove(OnTooltipDespawn);
            GameEvents.onGUIRnDComplexSpawn.Remove(RnDSpawn);
        }

        private void RnDSpawn()
        {
            if (!_rdPrefabProcessed && HighLogic.LoadedScene == GameScenes.SPACECENTER)
                ProcessRDrIconPrefab();
        }

        private void ProcessEditorIconPrefab()
        {
            USdebugMessages.USStaticLog("Processing Editor Icon Prefab...");

            StartCoroutine(WaitForEditorIconPrefab());
        }

        private IEnumerator WaitForEditorIconPrefab()
        {
            yield return new WaitForEndOfFrame();

            while (EditorPartList.Instance == null)
                yield return null;

            //USdebugMessages.USStaticLog("Editor Part List Ready...");

            EditorPartList.Instance.partPrefab.gameObject.AddOrGetComponent<EditorPartIconListener>();

            _iconPrefabProcessed = true;
        }

        private void ProcessRDrIconPrefab()
        {
            USdebugMessages.USStaticLog("Processing R&D Icon Prefab...");

            StartCoroutine(WaitForRDIconPrefab());
        }

        private IEnumerator WaitForRDIconPrefab()
        {
            yield return new WaitForEndOfFrame();

            while (RDController.Instance == null)
                yield return null;

            USdebugMessages.USStaticLog("R&D Part List Ready...");

            RDController.Instance.partList.partListItem.GetComponentInChildren<RDPartListItem>().gameObject.AddOrGetComponent<EditorPartIconListener>();

            _rdPrefabProcessed = true;
        }

        private void ProcessUIPrefab()
        {
            USdebugMessages.USStaticLog("Processing UI Part Action Controller Spawner...");

            StartCoroutine(WaitForUIPrefab());
        }

        private IEnumerator WaitForUIPrefab()
        {
            UIPartActionControllerSpawner spawner = FindObjectOfType<UIPartActionControllerSpawner>();

            while (spawner == null)
            {
                yield return null;

                spawner = FindObjectOfType<UIPartActionControllerSpawner>();
            }

            ProcessUIPrefabList(spawner.controllerPrefab);
        }
        
        private void ProcessUIPrefabList(UIPartActionController controller)
        {
            for (int i = controller.fieldPrefabs.Count - 1; i >= 0; i--)
            {
                UIPartActionFieldItem item = controller.fieldPrefabs[i];

                if (item is UIPartActionVariantSelector)
                {
                    UIPartActionVariantSelector variant = Instantiate(item) as UIPartActionVariantSelector;

                    Button next = variant.buttonNext;
                    Button prev = variant.buttonPrevious;

                    GameObject buttonPrefab = variant.prefabVariantButton;

                    ScrollRect scroll = variant.scrollMain;

                    TextMeshProUGUI text = variant.variantName;

                    GameObject obj = variant.gameObject;

                    DestroyImmediate(variant);

                    UI_USPartActionVariantSelector usSelector = obj.AddComponent<UI_USPartActionVariantSelector>();

                    usSelector.buttonNext = next;
                    usSelector.buttonPrevious = prev;

                    usSelector.prefabVariantButton = buttonPrefab;

                    usSelector.scrollMain = scroll;

                    usSelector.variantName = text;

                    controller.fieldPrefabs.Add(usSelector);

                    USdebugMessages.USStaticLog("UI Part Action Controller prefab list processed");

                    break;
                }
            }
        }

        private void OnTooltipSpawned(ITooltipController controller, Tooltip tooltip)
        {
            if (!(tooltip is PartListTooltip))
                return;
            
            if (!(controller is PartListTooltipController))
                return;
            
            PartListTooltipController partListController = controller as PartListTooltipController;

            if (partListController == null)
                return;
            
            PartListTooltip partListTooltip = tooltip as PartListTooltip;

            if (partListTooltip == null)
                return;
            
            EditorPartIcon baseIcon = partListController.GetComponent<EditorPartIcon>();

            if (baseIcon == null)
                return;
            
            AvailablePart partInfo = baseIcon.partInfo;

            USSwitchControl primarySwitch = null;

            if (_primarySwitches.ContainsKey(partInfo))
                primarySwitch = _primarySwitches[partInfo];
            else
                return;
            
            USSwitchControl secondarySwitch = null;

            if (_secondarySwitches.ContainsKey(partInfo))
                secondarySwitch = _secondarySwitches[partInfo];

            _currentTooltipTransform = partListTooltip.icon.transform;

            _currentPartInfo = partInfo;

            partListTooltip.buttonToggleVariant.gameObject.SetActive(true);
            partListTooltip.buttonToggleVariant.onClick.RemoveAllListeners();

            if (secondarySwitch != null)
            {
                Button secondaryButton = Instantiate(partListTooltip.buttonToggleVariant
                    , partListTooltip.buttonToggleVariant.transform.parent, false);

                RectTransform secondRect = secondaryButton.GetComponent<RectTransform>();

                secondRect.anchorMin = new Vector2(0, 1);
                secondRect.anchorMax = new Vector2(0, 1);

                secondRect.anchoredPosition = new Vector2(secondRect.anchoredPosition.x, -10);

                secondaryButton.onClick.AddListener(
                    delegate
                    {
                        ToggleTooltipSecondaryVariant(_currentPartInfo);
                    });

                secondarySwitch.EditorToggleVariant(_currentPartInfo, _currentTooltipTransform, false);

                if (onSecondaryVariantSwitched != null)
                    onSecondaryVariantSwitched.Add(OnSecondaryVariantSwitch);
            }

            if (primarySwitch != null)
            {
                partListTooltip.buttonToggleVariant.onClick.AddListener(
                    delegate 
                    {
                        ToggleTooltipPrimaryVariant(_currentPartInfo);
                    });

                primarySwitch.EditorToggleVariant(_currentPartInfo, _currentTooltipTransform, false);

                if (onPrimaryVariantSwitched != null)
                    onPrimaryVariantSwitched.Add(OnPrimaryVariantSwitch);
            }
        }

        private void OnTooltipDespawn(Tooltip tooltip)
        {
            _currentTooltipTransform = null;
            _currentPartInfo = null;

            if (onPrimaryVariantSwitched != null)
                onPrimaryVariantSwitched.Remove(OnPrimaryVariantSwitch);

            if (onSecondaryVariantSwitched != null)
                onSecondaryVariantSwitched.Remove(OnSecondaryVariantSwitch);
        }

        private void ToggleTooltipPrimaryVariant(AvailablePart partInfo)
        {
            if (!_primarySwitches.ContainsKey(partInfo))
                return;

            if (_currentPartInfo == null || partInfo != _currentPartInfo)
                return;

            if (_currentTooltipTransform == null)
                return;

            _activeSwitcher = true;

            //USdebugMessages.USStaticLog("Toggle primary variant event for tooltip: {0}", partInfo.title);

            _primarySwitches[partInfo].EditorToggleVariant(partInfo, _currentTooltipTransform, true);

            if (onPrimaryVariantSwitched != null)
                onPrimaryVariantSwitched.Fire(partInfo);

            _activeSwitcher = false;
        }

        private void ToggleTooltipSecondaryVariant(AvailablePart partInfo)
        {
            if (!_secondarySwitches.ContainsKey(partInfo))
                return;

            if (_currentPartInfo == null || partInfo != _currentPartInfo)
                return;

            if (_currentTooltipTransform == null)
                return;

            _activeSwitcher = true;

            //USdebugMessages.USStaticLog("Toggle secondary variant event for tooltip: {0}", partInfo.title);

            _secondarySwitches[partInfo].EditorToggleVariant(partInfo, _currentTooltipTransform, true);

            if (onSecondaryVariantSwitched != null)
                onSecondaryVariantSwitched.Fire(partInfo);

            _activeSwitcher = false;
        }

        private void OnPrimaryVariantSwitch(AvailablePart partInfo)
        {
            if (_activeSwitcher)
                return;

            if (_currentPartInfo == null || partInfo != _currentPartInfo)
                return;

            if (!_primarySwitches.ContainsKey(partInfo))
                return;

            if (_currentTooltipTransform == null)
                return;

            //USdebugMessages.USStaticLog("Fire primary variant event for tooltip: {0}", partInfo.title);

            _primarySwitches[partInfo].EditorToggleVariant(partInfo, _currentTooltipTransform, false);
        }

        private void OnSecondaryVariantSwitch(AvailablePart partInfo)
        {
            if (_activeSwitcher)
                return;

            if (_currentPartInfo == null || partInfo != _currentPartInfo)
                return;

            if (!_secondarySwitches.ContainsKey(partInfo))
                return;

            if (_currentTooltipTransform == null)
                return;

            //USdebugMessages.USStaticLog("Fire secondary variant event for tooltip: {0}", partInfo.title);

            _secondarySwitches[partInfo].EditorToggleVariant(partInfo, _currentTooltipTransform, false);
        }

        public void AddSwitchControl(AvailablePart partInfo, USSwitchControl switchControl, bool primary)
        {
            if (primary)
            {
                if (!_primarySwitches.ContainsKey(partInfo))
                    _primarySwitches.Add(partInfo, switchControl);
            }
            else
            {
                if (!_secondarySwitches.ContainsKey(partInfo))
                    _secondarySwitches.Add(partInfo, switchControl);
            }
        }

        public USSwitchControl GetSwitchControl(AvailablePart partInfo, bool primary)
        {
            if (primary)
            {
                if (_primarySwitches.ContainsKey(partInfo))
                    return _primarySwitches[partInfo];
            }
            else
            {
                if (_secondarySwitches.ContainsKey(partInfo))
                    return _secondarySwitches[partInfo];
            }

            return null;
        }

    }
}
