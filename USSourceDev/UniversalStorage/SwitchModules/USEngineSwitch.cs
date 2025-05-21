using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace UniversalStorage2.SwitchModules
{
    public class USEngineSwitch : PartModule, IPartCostModifier, IPartMassModifier
    {
        [KSPField]
        public string SwitchID = string.Empty;
        [KSPField]
        public bool availableInFlight = false;
        [KSPField]
        public bool availableInEditor = false;
        [KSPField]
        public bool ShowInfo = true;
        //[KSPField(isPersistant = true)]
        // public bool hasLaunched = false;
        [KSPField(isPersistant = true)]
        public bool configLoaded = false;
        [KSPField]
        public bool DebugMode = false;
        [KSPField]
        public string DisplayCostName = "#autoLOC_US_EngineCost";
        [KSPField]
        public string ModuleDisplayName = "#autoLOC_US_EngineSwitch";

        [KSPField(isPersistant = true)]
        public int selectedEngine = -1;

        private int[] _SwitchIndices;

        private float meshCost = 0;
        private float meshMass = 0;


        private bool initialized = false;

        UIPartActionWindow tweakableUI;

        private USdebugMessages debug;

        private EventData<int, int, bool, Part> onUSFuelSwitch;
        private EventData<int, Part, USFuelSwitch> onFuelRequestMass;
        private EventData<int, Part, USFuelSwitch> onFuelRequestCost;


        List<string> engineList = new List<string>();

        public override void OnStart(PartModule.StartState state)
        {
            if (String.IsNullOrEmpty(SwitchID))
                return;

            _SwitchIndices = USTools.parseIntegers(SwitchID).ToArray();

            onUSFuelSwitch = GameEvents.FindEvent<EventData<int, int, bool, Part>>("onUSFuelSwitch");

            if (onUSFuelSwitch != null)
                onUSFuelSwitch.Add(OnFuelSwitch);

            initializeData();

            SelectEngine(selectedEngine);
        }

        void setupEngineList()
        {
            if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight)
            {
                int i = 0;
                for (int count = base.part.Modules.Count; i < count; i++)
                {
                    PartModule partModule = base.part.Modules[i];
                    if (partModule is ModuleEnginesFX)
                    {
                        var tempEngine = (ModuleEnginesFX)partModule;
                        tempEngine.enabled = tempEngine.moduleIsEnabled = false;
                        engineList.Add(tempEngine.engineID);
                        continue;
                    }
                    if (partModule is ModuleEngines)
                    {
                        var tempEngine = (ModuleEngines)partModule;
                        tempEngine.enabled = tempEngine.moduleIsEnabled = false;
                        engineList.Add(tempEngine.engineID);
                    }
                }
            }
        }
        public override void OnAwake()
        {
            if (configLoaded)
                initializeData();
        }

        private void OnDestroy()
        {
            if (onUSFuelSwitch != null)
                onUSFuelSwitch.Remove(OnFuelSwitch);
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            if (!configLoaded)
                initializeData();

            configLoaded = true;
        }

        public override string GetInfo()
        {
            if (ShowInfo)
            {
                StringBuilder info = StringBuilderCache.Acquire();
                info.AppendLine(Localizer.Format("#autoLOC_US_EngineVariants"));

                for (int i = 0; i < engineList.Count; i++)
                    info.AppendLine(engineList[i].Replace(",", ", "));

                return info.ToStringAndRelease();
            }
            else
                return base.GetInfo();
        }

        public override string GetModuleDisplayName()
        {
            return Localizer.Format(ModuleDisplayName);
        }

        private void initializeData()
        {
            if (!initialized)
            {
                debug = new USdebugMessages(DebugMode, "USEngineSwitch");

                setupEngineList();


                // if (HighLogic.LoadedSceneIsFlight)
                //     hasLaunched = true;

                initialized = true;
            }
        }


        private void OnFuelSwitch(int index, int selection, bool modeOne, Part p)
        {
            if (p != part)
                return;

            selectedEngine = selection;
            SelectEngine(selectedEngine);
        }
        void SelectEngine(int selectedEngine)
        {
            if (selectedEngine < 0)
            {
                return;
            }
            int index = 0;
            for (int i1 = 0; i1 < base.part.Modules.Count; i1++)
            {
                PartModule partModule = base.part.Modules[i1];

                if (partModule is ModuleEnginesFX)
                {
                    var tempEngine = (ModuleEnginesFX)partModule;
                    if (HighLogic.LoadedSceneIsFlight)
                        tempEngine.Shutdown();
                    tempEngine.enabled = tempEngine.moduleIsEnabled = tempEngine.includeinDVCalcs = tempEngine.isEnabled = false;
                    if (index == selectedEngine)
                    {
                        if (HighLogic.LoadedSceneIsFlight)
                            tempEngine.enabled = tempEngine.moduleIsEnabled = tempEngine.includeinDVCalcs = tempEngine.isEnabled = true;

                    }
                    index++;

                }
                else
                {
                    if (partModule is ModuleEngines)
                    {
                        var tempEngine = (ModuleEngines)partModule;
                        if (HighLogic.LoadedSceneIsFlight)
                            tempEngine.Shutdown();
                        tempEngine.enabled = tempEngine.moduleIsEnabled = tempEngine.includeinDVCalcs = tempEngine.isEnabled = false;
                        if (index == selectedEngine)
                        {
                            if (HighLogic.LoadedSceneIsFlight)
                                tempEngine.enabled = tempEngine.moduleIsEnabled = tempEngine.includeinDVCalcs = tempEngine.isEnabled = true;
                        }
                        index++;
                    }
                }
            }
        }

        public void setMeshCost(float cost)
        {
            meshCost = cost;
        }

        public void setMeshMass(float mass)
        {
            meshMass = mass;
        }

        private float updateCost()
        {
            float cost = 0;


            float newCost = meshCost;

            return newCost;
        }

        public float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
        {
            return updateCost();
        }

        public ModifierChangeWhen GetModuleCostChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
        }

        public float GetModuleMass(float defaultMass, ModifierStagingSituation sit)
        {
            return 0;
        }

        public ModifierChangeWhen GetModuleMassChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
        }
    }
}
