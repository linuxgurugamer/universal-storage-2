
using System;
using System.Collections;

namespace UniversalStorage
{
    public class USMassSwitch : PartModule, IPartMassModifier
    {
        [KSPField]
        public string SwitchID = string.Empty;
        [KSPField]
        public string AddedMass = string.Empty;
        [KSPField]
        public bool DisplayCurrentModeMass = false;
        [KSPField(guiActive = false, guiActiveEditor = true, guiName = "Part Mass")]
        public float DryMassInfo = 0f;
        [KSPField(isPersistant = true)]
        public int CurrentSelection = 0;

        private int[] _SwitchIndices;
        private double[] _Masses;
        private EventData<int, int, Part> onUSSwitch;
        private EventData<int, Part, USFuelSwitch> onFuelRequestMass;
        private bool _updateMass = true;

        public override void OnAwake()
        {
            base.OnAwake();

            onUSSwitch = GameEvents.FindEvent<EventData<int, int, Part>>("onUSSwitch");
            onFuelRequestMass = GameEvents.FindEvent<EventData<int, Part, USFuelSwitch>>("onFuelRequestMass");

            if (onUSSwitch != null)
                onUSSwitch.Add(onSwitch);

            if (onFuelRequestMass != null)
                onFuelRequestMass.Add(onFuelSwitchRequest);
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (String.IsNullOrEmpty(SwitchID))
                return;

            _SwitchIndices = USTools.parseIntegers(SwitchID).ToArray();

            if (String.IsNullOrEmpty(AddedMass))
                return;

            _Masses = USTools.parseDoubles(AddedMass).ToArray();
            
            Fields["DryMassInfo"].guiActiveEditor = DisplayCurrentModeMass;            
        }

        private void OnDestroy()
        {
            if (onUSSwitch != null)
                onUSSwitch.Remove(onSwitch);

            if (onFuelRequestMass != null)
                onFuelRequestMass.Remove(onFuelSwitchRequest);
        }

        private void onSwitch(int index, int selection, Part p)
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

            if (_Masses.Length >= CurrentSelection)
                mass = (float)_Masses[CurrentSelection];

            fuel.setMeshMass(mass);
        }

        private float UpdateWeight(Part currentPart)
        {
            float mass = 0;

            if (_Masses != null && _Masses.Length >= CurrentSelection)
                mass = (float)_Masses[CurrentSelection];

            DryMassInfo = currentPart.partInfo.partPrefab.mass + mass;

            return mass;
        }
        
        public float GetModuleMass(float defaultMass, ModifierStagingSituation sit)
        {
            if (!_updateMass)
                return 0;

            return UpdateWeight(part);
        }

        public ModifierChangeWhen GetModuleMassChangeWhen()
        {
            return ModifierChangeWhen.CONSTANTLY;
        }
    }
}
