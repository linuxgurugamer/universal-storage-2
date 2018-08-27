using System;
using KSP.Localization;

namespace UniversalStorage2
{
    public class USRadiatorSwitch : ModuleActiveRadiator
    {
        [KSPField]
        public string SwitchID = string.Empty;
        [KSPField]
        public string RadiatorAvailable = string.Empty;
        [KSPField]
        public string RadiatorPower = string.Empty;
        [KSPField]
        public string RadiatorOvercool = string.Empty;
        [KSPField]
        public string RadiatorEnergy = string.Empty;
        [KSPField]
        public bool ManualRadiatorToggle = false;
        [KSPField(isPersistant = true)]
        public bool ActiveCooling = true;
        [KSPField(isPersistant = true)]
        public int CurrentSelection = 0;

        private int[] _SwitchIndices;
        private int[] _RadiatorAvailables;
        private float[] _RadiatorPowers;
        private float[] _RadiatorOvercools;
        private float[] _RadiatorEnergies;
        
        private bool _Enabled;
        private bool _Unavailable = false;
        
        private BaseField _BaseStatusField;

        private BaseEvent _ToggleRadiatorEvent;

        private EventData<int, int, Part> onUSSwitch;

        public override void Start()
        {

        }

        public override void OnStart(StartState state)
        {
            _BaseStatusField = Fields["status"];

            _ToggleRadiatorEvent = Events["ToggleRadiator"];

            if (String.IsNullOrEmpty(RadiatorPower))
            {
                base.OnStart(state);
                base.Start();

                Actions["ToggleRadiatorAction"].active = false;
                Actions["ActivateAction"].active = false;
                Actions["ShutdownAction"].active = false;
                Events["Activate"].active = false;
                Events["Shutdown"].active = false;

                return;
            }

            _RadiatorPowers = USTools.parseSingles(RadiatorPower).ToArray();

            if (!String.IsNullOrEmpty(RadiatorAvailable))
                _RadiatorAvailables = USTools.parseIntegers(RadiatorAvailable).ToArray();

            if (!String.IsNullOrEmpty(RadiatorOvercool))
                _RadiatorOvercools = USTools.parseSingles(RadiatorOvercool).ToArray();

            if (!String.IsNullOrEmpty(RadiatorEnergy))
                _RadiatorEnergies = USTools.parseSingles(RadiatorEnergy).ToArray();
            
            if (!String.IsNullOrEmpty(SwitchID))
            {
                _SwitchIndices = USTools.parseIntegers(SwitchID).ToArray();

                onUSSwitch = GameEvents.FindEvent<EventData<int, int, Part>>("onUSSwitch");

                if (onUSSwitch != null)
                    onUSSwitch.Add(onSwitch);

                UpdateRadiator();
            }

            base.OnStart(state);
            base.Start();

            Actions["ToggleRadiatorAction"].active = false;
            Actions["ActivateAction"].active = false;
            Actions["ShutdownAction"].active = false;
            Events["Activate"].active = false;
            Events["Shutdown"].active = false;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (onUSSwitch != null)
                onUSSwitch.Remove(onSwitch);
        }

        public void Enable()
        {
            if (_Unavailable)
                return;

            _ToggleRadiatorEvent.active = ManualRadiatorToggle;

            _BaseStatusField.guiActive = true;

            _Enabled = true;
            IsCooling = ActiveCooling;            
        }

        public void Disable()
        {
            _ToggleRadiatorEvent.active = false;

            _BaseStatusField.guiActive = false;

            _Enabled = false;

            IsCooling = false;
        }

        [KSPEvent(guiName = "#autoLOC_6001416", active = false, guiActive = true, guiActiveEditor = true, guiActiveUnfocused = true, unfocusedRange = 4f, externalToEVAOnly = true)]
        public void ToggleRadiator()
        {
            if (!_Enabled || _Unavailable)
            {
                IsCooling = false;
                ActiveCooling = false;
                return;
            }

            IsCooling = !IsCooling;

            ActiveCooling = IsCooling;
        }
        
        public override bool IsSibling(Part targetPart)
        {
            if (part.parent != null)
            {
                if (targetPart = part.parent)
                    return true;
                else if (part.parent.parent != null && targetPart == part.parent.parent)
                    return true;
                else if (targetPart.parent != null && targetPart.parent == part.parent)
                    return true;
            }

            if (targetPart.parent != null)
            {
                if (targetPart.parent == part)
                    return true;
                else if (targetPart.parent.parent != null && targetPart.parent.parent == part)
                    return true;
            }

            return false;
        }
        
        public override void Activate()
        {
            IsCooling = true;
        }

        public override void Shutdown()
        {
            IsCooling = false;
        }

        public override void ToggleRadiatorAction(KSPActionParam param)
        {
            IsCooling = !IsCooling;
        }

        public void ToggleRadiator(bool isOn)
        {
            IsCooling = isOn;
        }

        protected override void UpdateStatus()
        {

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

                    UpdateRadiator();

                    break;
                }
            }
        }

        private void UpdateRadiator()
        {
            if (_RadiatorPowers != null && _RadiatorPowers.Length > CurrentSelection)
                maxEnergyTransfer = _RadiatorPowers[CurrentSelection];

            if (_RadiatorOvercools != null && _RadiatorOvercools.Length > CurrentSelection)
                overcoolFactor = _RadiatorOvercools[CurrentSelection];

            if (_RadiatorAvailables != null && _RadiatorAvailables.Length > CurrentSelection)
            {
                int i = _RadiatorAvailables[CurrentSelection];

                _Unavailable = i == 0;

                if (_Unavailable)
                {
                    IsCooling = false;
                    _ToggleRadiatorEvent.active = false;
                    _BaseStatusField.guiActive = false;
                }
                else
                {
                    _ToggleRadiatorEvent.active = ManualRadiatorToggle;
                    _BaseStatusField.guiActive = _Enabled;
                }
            }

            if (resHandler != null)
            {
                if (resHandler.inputResources != null && resHandler.inputResources.Count > 0)
                {
                    if (_RadiatorEnergies != null && _RadiatorEnergies.Length > CurrentSelection)
                    {
                        resHandler.inputResources[0].rate = _RadiatorEnergies[CurrentSelection];
                    }
                }
            }

            base.Start();

            Actions["ToggleRadiatorAction"].active = false;
            Actions["ActivateAction"].active = false;
            Actions["ShutdownAction"].active = false;
            Events["Activate"].active = false;
            Events["Shutdown"].active = false;
        }
    }
}
