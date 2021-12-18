
using System;
using System.Collections.Generic;
using KSP.Localization;

namespace UniversalStorage2
{
    public class USCostSwitch : USBaseSwitch, IPartCostModifier
    {
        [KSPField]
        public string AddedCost = string.Empty;
        [KSPField]
        public bool DisplayCurrentModeCost = false;
        [KSPField]
        public string DisplayCostName = "Part Cost";
        [KSPField(guiActive = false, guiActiveEditor = false, guiName = "Part Cost")]
        public float AddedCostValue = 0f;
        [KSPField(isPersistant = true)]
        public int CurrentSelection = 0;
        
        private double[] _Costs;
        private EventData<int, Part, USFuelSwitch> onFuelRequestCost;
        private bool _updateCost = true;

        private List<IPartCostModifier> _partCostModifiers = new List<IPartCostModifier>();

        private string _localizedDryCostString = "Part Cost";

        public override void OnAwake()
        {
            base.OnAwake();
            
            onFuelRequestCost = GameEvents.FindEvent<EventData<int, Part, USFuelSwitch>>("onFuelRequestCost");
            
            if (onFuelRequestCost != null)
                onFuelRequestCost.Add(onFuelSwitchRequest);

            _localizedDryCostString = Localizer.Format(DisplayCostName);

            Fields["AddedCostValue"].guiName = _localizedDryCostString;
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            
            if (String.IsNullOrEmpty(AddedCost))
                return;

            _partCostModifiers = part.FindModulesImplementing<IPartCostModifier>();

            for (int i = _partCostModifiers.Count - 1; i >= 0; i--)
            {
                if (_partCostModifiers[i] == (IPartCostModifier)this)
                {
                    _partCostModifiers.RemoveAt(i);
                    break;
                }
            }

            _Costs = USTools.parseDoubles(AddedCost).ToArray();

            Fields["AddedCostValue"].guiActiveEditor = DisplayCurrentModeCost;
        }

        private void OnDestroy()
        {
            if (onFuelRequestCost != null)
                onFuelRequestCost.Remove(onFuelSwitchRequest);
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

            if (_Costs.Length > CurrentSelection)
                cost = (float)_Costs[CurrentSelection];

            fuel.setMeshCost(cost);            
        }

        private float UpdateCost()
        {
            float cost = 0;

            if (_Costs != null && _Costs.Length >= CurrentSelection)
                cost = (float)_Costs[CurrentSelection];

            float otherCosts = 0;

            for (int i = _partCostModifiers.Count - 1; i >= 0; i--)
            {
                otherCosts += _partCostModifiers[i].GetModuleCost(0, ModifierStagingSituation.CURRENT);
            }
            
            AddedCostValue = part.partInfo.cost + cost + otherCosts;

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
