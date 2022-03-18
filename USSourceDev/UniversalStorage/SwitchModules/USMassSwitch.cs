
using System;
using KSP.Localization;

namespace UniversalStorage2
{
    public class USMassSwitch : USBaseSwitch, IPartMassModifier
    {
        [KSPField]
        public string AddedMass = string.Empty;
        [KSPField]
        public bool DisplayCurrentModeMass = false;
        [KSPField]
        public string DisplayMassName = "#autoLOC_US_PartMass"; // Part Mass
        [KSPField(guiActive = false, guiActiveEditor = true, guiName = "#autoLOC_US_PartMass")]
        public float DryMassInfo = 0f;
        [KSPField(isPersistant = true)]
        public int CurrentSelection = 0;
        
        private double[] _Masses;
        private EventData<int, Part, USFuelSwitch> onFuelRequestMass;
        private bool _updateMass = true;

        private string _localizedDryMassString = "Part Mass";
        private USSolarSwitch solarSwitchModule = null;

        public override void OnAwake()
        {
            base.OnAwake();
            
            onFuelRequestMass = GameEvents.FindEvent<EventData<int, Part, USFuelSwitch>>("onFuelRequestMass");
            
            if (onFuelRequestMass != null)
                onFuelRequestMass.Add(onFuelSwitchRequest);

            _localizedDryMassString = Localizer.Format(DisplayMassName);

            Fields["DryMassInfo"].guiName = _localizedDryMassString;
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            
            if (String.IsNullOrEmpty(AddedMass))
                return;

            _Masses = USTools.parseDoubles(AddedMass).ToArray();

            Fields["DryMassInfo"].guiActiveEditor = DisplayCurrentModeMass;
            solarSwitchModule = part.FindModuleImplementing<USSolarSwitch>();
        }

        private void OnDestroy()
        {
            if (onFuelRequestMass != null)
                onFuelRequestMass.Remove(onFuelSwitchRequest);
        }

        protected override void onSwitch(int index, int selection, Part p)
        {
            if (p != part)
                return;

            if (_SwitchIndices == null || _SwitchIndices.Length <= 0)
            {
                if (String.IsNullOrEmpty(SwitchID))
                    return;

                _SwitchIndices = USTools.parseIntegers(SwitchID).ToArray();
            }

            for (int i = _SwitchIndices.Length - 1; i >= 0; i--)
            {
                if (_SwitchIndices[i] == index)
                {
                    CurrentSelection = selection;

                    break;
                }
            }
        }

        private void onFuelSwitchRequest(int index, Part p, USFuelSwitch fuel)
        {
            _updateMass = false;

            if (p != part)
                return;

            if (fuel == null || fuel.part != p)
                return;

            float mass = 0;

            if (_Masses == null || _Masses.Length <= 0)
            {
                if (String.IsNullOrEmpty(AddedMass))
                    return;

                _Masses = USTools.parseDoubles(AddedMass).ToArray();
            }

            if (_Masses.Length > CurrentSelection)
                mass = (float)_Masses[CurrentSelection];

            fuel.setMeshMass(mass);

            DryMassInfo = DryMassInfo = part.partInfo.cost + mass;
            UpdateSolarMass();

        }

        private float UpdateWeight(Part currentPart)
        {
            float mass = 0;

            if (_Masses != null && _Masses.Length >= CurrentSelection)
                mass = (float)_Masses[CurrentSelection];

            DryMassInfo = currentPart.partInfo.partPrefab.mass + mass;

            return mass;
        }

        private void UpdateSolarMass()
        {
            if (solarSwitchModule != null)
            {
                DryMassInfo += solarSwitchModule.GetModuleMass(0, ModifierStagingSituation.CURRENT);
                USdebugMessages.USStaticLog("USCostSwitch.GetModuleMass, solarSwitchModuleCost: " + solarSwitchModule.GetModuleCost(0, ModifierStagingSituation.CURRENT));
            }
        }

        public float GetModuleMass(float defaultMass, ModifierStagingSituation sit)
        {
            if (!_updateMass)
                return 0;

            var m = UpdateWeight(part);
            UpdateSolarMass();
            return m;
        }

        public ModifierChangeWhen GetModuleMassChangeWhen()
        {
            return ModifierChangeWhen.CONSTANTLY;
        }
    }
}
