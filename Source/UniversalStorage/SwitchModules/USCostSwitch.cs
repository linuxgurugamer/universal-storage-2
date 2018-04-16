
using System;

namespace UniversalStorage
{
    public class USCostSwitch : PartModule, IPartCostModifier
    {
        [KSPField]
        public string SwitchID = string.Empty;
        [KSPField]
        public string AddedCost = string.Empty;
        [KSPField]
        public bool DisplayCurrentModeCost = false;
        [KSPField(guiActive = false, guiActiveEditor = false, guiName = "Part cost")]
        public float AddedCostValue = 0f;
        [KSPField(isPersistant = true)]
        public int CurrentSelection = 0;

        private int[] _SwitchIndices;
        private double[] _Costs;
        private EventData<int, int, Part> onUSSwitch;
        private EventData<int, Part, USFuelSwitch> onFuelRequestCost;
        private bool _updateCost = true;

        public override void OnAwake()
        {
            base.OnAwake();

            onUSSwitch = GameEvents.FindEvent<EventData<int, int, Part>>("onUSSwitch");
            onFuelRequestCost = GameEvents.FindEvent<EventData<int, Part, USFuelSwitch>>("onFuelRequestCost");

            if (onUSSwitch != null)
                onUSSwitch.Add(onSwitch);

            if (onFuelRequestCost != null)
                onFuelRequestCost.Add(onFuelSwitchRequest);
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (String.IsNullOrEmpty(SwitchID))
                return;

            _SwitchIndices = USTools.parseIntegers(SwitchID).ToArray();

            if (String.IsNullOrEmpty(AddedCost))
                return;

            _Costs = USTools.parseDoubles(AddedCost).ToArray();

            Fields["AddedCostValue"].guiActiveEditor = DisplayCurrentModeCost;
        }

        private void OnDestroy()
        {
            if (onUSSwitch != null)
                onUSSwitch.Remove(onSwitch);

            if (onFuelRequestCost != null)
                onFuelRequestCost.Remove(onFuelSwitchRequest);
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
            _updateCost = false;

            if (p != part)
                return;

            if (fuel == null || fuel.part != p)
                return;

            float cost = 0;

            if (_Costs == null || _Costs.Length <= 0)
            {
                if (String.IsNullOrEmpty(AddedCost))
                    return;

                _Costs = USTools.parseDoubles(AddedCost).ToArray();
            }

            if (_Costs.Length >= CurrentSelection)
                cost = (float)_Costs[CurrentSelection];

            fuel.setMeshCost(cost);            
        }

        private float UpdateCost()
        {
            float cost = 0;

            if (_Costs != null && _Costs.Length >= CurrentSelection)
                cost = (float)_Costs[CurrentSelection];
            
            AddedCostValue = part.partInfo.cost + cost;

            return cost;
        }

        public float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
        {
            if (!_updateCost)
                return 0;

            return UpdateCost();
        }

        public ModifierChangeWhen GetModuleCostChangeWhen()
        {
            return ModifierChangeWhen.CONSTANTLY;
        }
    }
}
