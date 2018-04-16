using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UniversalStorage
{
	public class USFuelSwitch : PartModule, IPartCostModifier, IPartMassModifier
    {
        [KSPField]
        public string SwitchID = string.Empty;
        [KSPField]
		public string resourceNames = "ElectricCharge;ElectricCharge|LiquidFuel,Oxidizer;LiquidFuel,Oxidizer|MonoPropellant;MonoPropellant|Structural;Structural";
		[KSPField]
		public string resourceAmounts = "100;100|75,25;75,25|200;200|0;0";
		[KSPField]
		public string initialResourceAmounts = "100;100|75,25;75,25|200;200|0;0";
		[KSPField]
		public string tankMass = "0;0|0;0|0;0|0;0";
		[KSPField]
		public string tankCost = "0;0|0;0|0;0|0;0";
		[KSPField]
		public bool displayCurrentTankCost = true;
        [KSPField]
        public bool displayCurrentTankDryMass = true;
        [KSPField]
		public bool availableInFlight = false;
		[KSPField]
		public bool availableInEditor = false;
		[KSPField]
		public bool showInfo = true;
		[KSPField(isPersistant = true)]
		public int selectedTankModeOne = -1;
		[KSPField(isPersistant = true)]
		public int selectedTankModeTwo = -1;
		[KSPField(isPersistant = true)]
		public bool hasLaunched = false;
		[KSPField(isPersistant = true)]
		public bool configLoaded = false;
		[KSPField]
		public bool DebugMode = false;
		[KSPField(guiActive = false, guiActiveEditor = true, guiName = "Dry cost")]
		public float addedCost = 0f;
		[KSPField(guiActive = false, guiActiveEditor = true, guiName = "Dry mass")]
		public float dryMassInfo = 0f;

		private List<List<List<USResource>>> tankList;

        private int[] _SwitchIndices;

        private float meshCost = 0;
		private float meshMass = 0;

		private List<List<double>> weightList;
		private List<List<double>> tankCostList;

		private bool initialized = false;

		UIPartActionWindow tweakableUI;

		private USdebugMessages debug;
        
        private EventData<int, int, bool, Part> onUSFuelSwitch;
        private EventData<int, Part, USFuelSwitch> onFuelRequestMass;
        private EventData<int, Part, USFuelSwitch> onFuelRequestCost;

        public override void OnStart(PartModule.StartState state)
        {
            if (String.IsNullOrEmpty(SwitchID))
                return;

            _SwitchIndices = USTools.parseIntegers(SwitchID).ToArray();

            onFuelRequestCost = GameEvents.FindEvent<EventData<int, Part, USFuelSwitch>>("onFuelRequestCost");
            onFuelRequestMass = GameEvents.FindEvent<EventData<int, Part, USFuelSwitch>>("onFuelRequestMass");
            onUSFuelSwitch = GameEvents.FindEvent<EventData<int, int, bool, Part>>("onUSFuelSwitch");

            if (onUSFuelSwitch != null)
                onUSFuelSwitch.Add(OnFuelSwitch);
            
            initializeData();

			if (selectedTankModeOne == -1 || selectedTankModeTwo == -1)
			{
				selectedTankModeOne = 0;
				selectedTankModeTwo = 0;
				assignResourcesToPart(false);
			}

            onFuelRequestMass.Fire(_SwitchIndices[0], part, this);
            onFuelRequestCost.Fire(_SwitchIndices[0], part, this);
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
			if (showInfo)
			{
				List<string> resourceList = USTools.parseNames(resourceNames);

				StringBuilder info = StringBuilderCache.Acquire();
				info.AppendLine("Fuel tank setups available:");

				for (int i = 0; i < resourceList.Count; i++)
					info.AppendLine(resourceList[i].Replace(",", ", "));

				return info.ToStringAndRelease();
			}
			else
				return base.GetInfo();
		}

        private void initializeData()
        {
            if (!initialized)
            {
                debug = new USdebugMessages(DebugMode, "USFuelSwitch");

                setupTankList();

                weightList = new List<List<double>>();

                string[] weights = tankMass.Split('|');

                for (int i = 0; i < weights.Length; i++)
                {
                    weightList.Add(USTools.parseDoubles(weights[i]));
                }

                tankCostList = new List<List<double>>();

                string[] costs = tankCost.Split('|');

                for (int i = 0; i < costs.Length; i++)
                {
                    tankCostList.Add(USTools.parseDoubles(costs[i]));
                }

                if (HighLogic.LoadedSceneIsFlight)
                    hasLaunched = true;

                Events["nextTankSetupEvent"].guiActive = availableInFlight;
                Events["nextTankSetupEvent"].guiActiveEditor = availableInEditor;
                Events["previousTankSetupEvent"].guiActive = availableInFlight;
                Events["previousTankSetupEvent"].guiActiveEditor = availableInEditor;
                Events["nextModeEvent"].guiActive = availableInFlight;
                Events["nextModeEvent"].guiActiveEditor = availableInEditor;
                Events["previousModeEvent"].guiActive = availableInFlight;
                Events["previousModeEvent"].guiActiveEditor = availableInEditor;

                if (HighLogic.CurrentGame == null || HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                    Fields["addedCost"].guiActiveEditor = displayCurrentTankCost;

                Fields["dryMassInfo"].guiActiveEditor = displayCurrentTankDryMass;

                initialized = true;
            }
        }

		[KSPEvent(guiActive=true, guiActiveEditor = true, guiName = "Next tank mode setup")]
		public void nextModeEvent()
		{
			selectedTankModeTwo++;

			if (selectedTankModeTwo >= tankList.Count)
				selectedTankModeTwo = 0;

			assignResourcesToPart(true);
		}

		[KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Previous tank mode setup")]
		public void previousModeEvent()
		{
			selectedTankModeTwo--;

			if (selectedTankModeTwo < 0)
				selectedTankModeTwo = tankList.Count - 1;

			assignResourcesToPart(true);
		}

		[KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Next tank setup")]
		public void nextTankSetupEvent()
		{
			selectedTankModeOne++;

			if (selectedTankModeOne >= tankList[selectedTankModeTwo].Count)
				selectedTankModeOne = 0;

			assignResourcesToPart(true);
		}

		[KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Previous tank setup")]
		public void previousTankSetupEvent()
		{
			selectedTankModeOne--;

			if (selectedTankModeOne < 0)
				selectedTankModeOne = tankList[selectedTankModeTwo].Count - 1;

			assignResourcesToPart(true);
		}

        private void OnFuelSwitch(int index, int selection, bool modeOne, Part p)
        {
            if (p != part)
                return;

            for (int i = _SwitchIndices.Length - 1; i >= 0; i--)
            {
                if (_SwitchIndices[i] == index)
                {
                    if (modeOne)
                    {
                        selectedTankModeOne = selection;

                        if (onFuelRequestCost != null)
                            onFuelRequestCost.Fire(index, part, this);

                        if (onFuelRequestMass != null)
                            onFuelRequestMass.Fire(index, part, this);

                        assignResourcesToPart(true);

                        //selectTankSetup(selectedTankModeOne, true);
                    }
                    else
                    {
                        selectedTankModeTwo = selection;

                        if (onFuelRequestCost != null)
                            onFuelRequestCost.Fire(index, part, this);

                        if (onFuelRequestMass != null)
                            onFuelRequestMass.Fire(index, part, this);

                        assignResourcesToPart(true);

                        //selectModeSetup(selectedTankModeTwo, true);
                    }

                    break;
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
        
		//public void selectModeSetup(int i, bool calledByPlayer, float cost, float mass)
		//{
		//	initializeData();

		//	if (selectedTankModeTwo != i)
		//	{
		//		debug.debugMessage("Update tank mode: " + i);

		//		meshModeTwoCost = cost;
		//		meshModeTwoMass = mass;
		//		selectedTankModeTwo = i;
		//		assignResourcesToPart(calledByPlayer);
		//	}
		//}

		//public void selectTankSetup(int i, bool calledByPlayer, float cost, float mass)
		//{
		//	initializeData();

		//	if (selectedTankModeOne != i)
		//	{
		//		debug.debugMessage("Update tank setup: " + i);

		//		meshModeOneCost = cost;
		//		meshModeOneMass = mass;
		//		selectedTankModeOne = i;
		//		assignResourcesToPart(calledByPlayer);
		//	}
		//}

		private void assignResourcesToPart(bool calledByPlayer)
		{
			// destroying a resource messes up the gui in editor, but not in flight.
			setupTankInPart(part, calledByPlayer);

			if (HighLogic.LoadedSceneIsEditor)
			{
				for (int s = 0; s < part.symmetryCounterparts.Count; s++)
				{
					setupTankInPart(part.symmetryCounterparts[s], calledByPlayer);

					USFuelSwitch symSwitch = part.symmetryCounterparts[s].GetComponent<USFuelSwitch>();

					if (symSwitch != null)
					{
						symSwitch.selectedTankModeOne = selectedTankModeOne;
						symSwitch.selectedTankModeTwo = selectedTankModeTwo;
					}
				}
			}

			if (tweakableUI == null)
				tweakableUI = USTools.FindActionWindow(part);

			if (tweakableUI != null)
				tweakableUI.displayDirty = true;
			else
                debug.debugMessage("no UI to refresh");
		}

		private void setupTankInPart(Part currentPart, bool calledByPlayer)
		{
			currentPart.Resources.dict = new DictionaryValueList<int, PartResource>();
			PartResource[] partResources = currentPart.GetComponents<PartResource>();

			for (int i = 0; i < tankList.Count; i++)
			{
				debug.debugMessage(string.Format("Tank Mode: {0} - Selection: {1}", i, selectedTankModeTwo));
				if (selectedTankModeTwo == i)
				{
					for (int j = 0; j < tankList[i].Count; j++)
					{
						debug.debugMessage(string.Format("Tank: {0} - Selection: {1}", j, selectedTankModeOne));
						if (selectedTankModeOne == j)
						{
							for (int k = 0; k < tankList[i][j].Count; k++)
							{
								USResource res = tankList[i][j][k];
								if (res.name != "Structural")
								{
									ConfigNode newResourceNode = new ConfigNode("RESOURCE");
									newResourceNode.AddValue("name", res.name);
									newResourceNode.AddValue("maxAmount", res.maxAmount);

									if (calledByPlayer && !HighLogic.LoadedSceneIsEditor)
										newResourceNode.AddValue("amount", 0.0f);
									else
										newResourceNode.AddValue("amount", res.amount);

									debug.debugMessage(string.Format("Switch to new resource: {0} - Amount: {1:N2} - Max: {2:N2}", res.name, res.amount, res.maxAmount));
									currentPart.AddResource(newResourceNode);
								}
							}
						}
					}
				}
			}

			updateWeight(currentPart);
			updateCost();
		}

		private void setupTankList()
		{
			tankList = new List<List<List<USResource>>>();

			List<List<List<double>>> resourceList = new List<List<List<double>>>();
			List<List<List<double>>> initialResourceList = new List<List<List<double>>>();

			string[] resourceModeArray = resourceAmounts.Split('|');
			string[] initialResourceModeArray = initialResourceAmounts.Split('|');

			if (string.IsNullOrEmpty(initialResourceAmounts) || initialResourceModeArray.Length != resourceModeArray.Length)
				initialResourceModeArray = resourceModeArray;

			for (int i = 0; i < resourceModeArray.Length; i++)
			{
				string[] resourceTankArray = resourceModeArray[i].Split(';');
				string[] initialResourceTankArray = initialResourceModeArray[i].Split(';');

				if (string.IsNullOrEmpty(initialResourceModeArray[i]) || initialResourceTankArray.Length != resourceTankArray.Length)
					initialResourceTankArray = resourceTankArray;

				List<List<double>> resourceTankAmounts = new List<List<double>>();
				List<List<double>> initResourceTankAmounts = new List<List<double>>();

				for (int j = 0; j < resourceTankArray.Length; j++)
				{
					string[] resourceAmountArray = resourceTankArray[j].Trim().Split(',');
					string[] initialResourceAmountArray = initialResourceTankArray[j].Trim().Split(',');

					if (string.IsNullOrEmpty(initialResourceTankArray[j]) || initialResourceAmountArray.Length != resourceAmountArray.Length)
						initialResourceAmountArray = resourceAmountArray;

					List<double> resList = new List<double>();
					List<double> initList = new List<double>();

					for (int k = 0; k < resourceAmountArray.Length; k++)
					{
						double res = 0;
						double init = 0;

						double.TryParse(resourceAmountArray[k].Trim(), out res);
						resList.Add(res);

						double.TryParse(initialResourceAmountArray[k].Trim(), out init);
						initList.Add(init);
					}

					resourceTankAmounts.Add(resList);
					initResourceTankAmounts.Add(initList);
				}

				resourceList.Add(resourceTankAmounts);
				initialResourceList.Add(initResourceTankAmounts);
			}
			
			// Then find the kinds of resources each tank holds, and fill them with the amounts found previously, or the amount they held last (values kept in save persistence/craft)
			string[] modeArray = resourceNames.Split('|');

			for (int i = 0; i < modeArray.Length; i++)
			{
				List<List<USResource>> newTankModes = new List<List<USResource>>();

				string mode = modeArray[i];

				string[] tankArray = mode.Split(';');

				for (int j = 0; j < tankArray.Length; j++)
				{
					List<USResource> newResources = new List<USResource>();

					string[] resourceArray = tankArray[j].Split(',');

					for (int k = 0; k < resourceArray.Length; k++)
					{
						string res = resourceArray[k].Trim(' ');

						USResource newResource = new USResource()
						{
							name = res
						};

						if (resourceList != null && i < resourceList.Count)
						{
							if (resourceList[i] != null && j < resourceList[i].Count)
							{
								if (resourceList[i][j] != null && k < resourceList[i][j].Count)
								{
									newResource.maxAmount = resourceList[i][j][k];
								}
							}
						}

						if (initialResourceList != null && i < initialResourceList.Count)
						{
							if (initialResourceList[i] != null && j < initialResourceList[i].Count)
							{
								if (initialResourceList[i][j] != null && k < initialResourceList[i][j].Count)
								{
									newResource.amount = initialResourceList[i][j][k];
								}
							}
						}

						newResources.Add(newResource);
					}

					newTankModes.Add(newResources);
				}

				tankList.Add(newTankModes);
			}
		}

		private float updateCost()
		{
			float cost = 0;

			if (selectedTankModeTwo >= 0 && selectedTankModeTwo < tankCostList.Count)
			{
				if (selectedTankModeOne >= 0 && selectedTankModeOne < tankCostList[selectedTankModeTwo].Count)
				{
					cost = (float)tankCostList[selectedTankModeTwo][selectedTankModeOne];
				}
			}

			float newCost = cost + meshCost;

			addedCost = getDryCost(part.partInfo.cost + newCost);

			return newCost;
		}

		private float getDryCost(float fullCost)
		{
			float cost = fullCost;

			for (int i = part.Resources.Count - 1; i >= 0; i--)
			{
				PartResource res = part.Resources[i];
				PartResourceDefinition def = res.info;

				cost -= def.unitCost * (float)res.maxAmount;
			}

			return cost;
		}

		private float updateWeight(Part currentPart)
		{
			float mass = 0;

			if (selectedTankModeTwo >= 0 && selectedTankModeTwo < weightList.Count)
			{
				if (selectedTankModeOne >= 0 && selectedTankModeOne < weightList[selectedTankModeTwo].Count)
				{
					mass = (float)weightList[selectedTankModeTwo][selectedTankModeOne];
				}
			}

			float newMass = mass + meshMass;

			dryMassInfo = currentPart.partInfo.partPrefab.mass + newMass;

			return newMass;
		}

		public float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
		{
			return updateCost();
		}

		public ModifierChangeWhen GetModuleCostChangeWhen()
		{
			return ModifierChangeWhen.CONSTANTLY;
		}

		public float GetModuleMass(float defaultMass, ModifierStagingSituation sit)
		{
			return updateWeight(part);
		}

		public ModifierChangeWhen GetModuleMassChangeWhen()
		{
			return ModifierChangeWhen.CONSTANTLY;
		}
	}
}
