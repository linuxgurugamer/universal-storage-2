using System;

namespace UniversalStorage2
{
    public class USSolarSwitch : USBaseSwitch
    {
        [KSPField]
        public string SolarChargeRate = string.Empty;
        [KSPField(isPersistant = true)]
        public int CurrentSelection = 0;
        [KSPField]
        public bool DebugMode = false;

        private ModuleDeployableSolarPanel solarModule;

        private USdebugMessages debug;
        private float[] _ChargeRates;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            debug = new USdebugMessages(DebugMode, "USSolarSwitch");
            
            if (String.IsNullOrEmpty(SolarChargeRate))
                return;

            _ChargeRates = USTools.parseSingles(SolarChargeRate).ToArray();

            solarModule = part.FindModuleImplementing<ModuleDeployableSolarPanel>();

            UpdateSolarPanels();
        }

        protected override void onSwitch(int index, int selection, Part p)
        {
            if (p != part)
                return;

            for (int i = _SwitchIndices.Length - 1; i >= 0; i--)
            {
                if (_SwitchIndices[i] == index)
                {
                    CurrentSelection = selection;

                    UpdateSolarPanels();

                    break;
                }
            }
        }

        private void UpdateSolarPanels()
        {
            if (solarModule == null)
                return;

            if (solarModule.resHandler != null)
            {
                if (solarModule.resHandler.outputResources != null && solarModule.resHandler.outputResources.Count > 0)
                {
                    if (_ChargeRates != null && _ChargeRates.Length > CurrentSelection)
                    {
                        solarModule.resHandler.outputResources[0].rate = _ChargeRates[CurrentSelection];
                    }
                }
            }
        }
    }
}
