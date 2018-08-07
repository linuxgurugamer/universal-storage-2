using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;

namespace UniversalStorage
{
	public class USMeshSwitchOld : PartModule //, IPartCostModifier, IPartMassModifier
	{
	//	[KSPField]
	//	public int moduleID = 0;
	//	[KSPField]
	//	public string buttonName = "Next part variant";
	//	[KSPField]
	//	public string previousButtonName = "Prev part variant";
	//	[KSPField]
	//	public string objectDisplayNames = string.Empty;
	//	[KSPField]
	//	public bool showPreviousButton = true;
	//	[KSPField]
	//	public bool useFuelSwitchModeOne = false;
	//	[KSPField]
	//	public bool useFuelSwitchModeTwo = false;
	//	[KSPField]
	//	public string fuelTankSetups = "0";
	//	[KSPField]
	//	public string objects = string.Empty;
	//	[KSPField]
	//	public string attachNodes = string.Empty;
	//	[KSPField]
	//	public string dragCubes = string.Empty;
	//	[KSPField]
	//	public string addedMass = string.Empty;
	//	[KSPField]
	//	public string addedCost = string.Empty;
	//	[KSPField]
	//	public float basePartMass = 0;
	//	[KSPField]
	//	public float basePartCost = 0;
	//	[KSPField]
	//	public bool updateSymmetry = true;
	//	[KSPField]
	//	public bool affectColliders = true;
	//	[KSPField]
	//	public bool showInfo = true;
	//	[KSPField]
	//	public bool debugMode = true;
	//	[KSPField]
	//	public bool displayCurrentModeCost = false;
	//	[KSPField]
	//	public bool displayCurrentModeMass = false;
	//	[KSPField(isPersistant = true)]
	//	public int selectedObject = 0;
	//	[KSPField(guiActiveEditor = true, guiName = "Current Variant")]
	//	public string currentObjectName = string.Empty;
	//	[KSPField(guiActive = false, guiActiveEditor = false, guiName = "Part cost")]
	//	public float addedCostValue = 0f;
	//	[KSPField(guiActive = false, guiActiveEditor = true, guiName = "Part Mass")]
	//	public float dryMassInfo = 0f;

	//	private List<USMeshObject> meshObjects = new List<USMeshObject>();
	//	private USMeshObject selectedMeshObject;

	//	private bool updateMass;
	//	private bool updateCost;

	//	private USFuelSwitch fuelSwitch;
	//	private USdebugMessages debug;

	//	private bool initialized = false;

	//	public bool HasDragCubes
	//	{
	//		get { return selectedMeshObject != null && selectedMeshObject.objectDragCubes.Count > 0; }
	//	}

	//	public override void OnStart(PartModule.StartState state)
	//	{
	//		initializeData();

	//		switchToObject(selectedObject, false);

	//		Events["nextObjectEvent"].guiName = buttonName;
	//		Events["previousObjectEvent"].guiName = previousButtonName;

	//		if (!showPreviousButton)
	//			Events["previousObjectEvent"].guiActiveEditor = false;

	//	}

	//	public void initializeData()
	//	{
	//		if (!initialized)
	//		{
	//			debug = new USdebugMessages(debugMode, "USMeshSwitch");

	//			// you can't have fuel switching without symmetry, it breaks the editor GUI.
	//			if (useFuelSwitchModeOne || useFuelSwitchModeTwo)
	//				updateSymmetry = true;

	//			if (string.IsNullOrEmpty(objects))
	//			{
	//				initialized = true;
	//				return;
	//			}

	//			string[] objectBatchNames = objects.Split(';');

	//			if (objectBatchNames.Length < 1)
	//			{
	//				debug.debugMessage("USMeshSwitch: Found no object names in the object list");
	//				initialized = true;
	//				return;
	//			}
	//			else
	//			{
	//				meshObjects.Clear();

	//				string[] displayNames = new string[0];

	//				if (!string.IsNullOrEmpty(objectDisplayNames))
	//					displayNames = objectDisplayNames.Split(';');

	//				string[] attachNodeRoots = new string[0];

	//				if (!string.IsNullOrEmpty(attachNodes))
	//					attachNodeRoots = attachNodes.Split(';');

	//				string[] fuelTankModes = new string[0];

	//				if (!string.IsNullOrEmpty(fuelTankSetups))
	//					fuelTankModes = fuelTankSetups.Split(';');

	//				string[] dragCubeNames = new string[0];

	//				if (!string.IsNullOrEmpty(dragCubes))
	//					dragCubeNames = dragCubes.Split(';');

	//				string[] addedMasses = new string[0];

	//				if (!string.IsNullOrEmpty(addedMass))
	//					addedMasses = addedMass.Split(';');

	//				string[] addedCosts = new string[0];

	//				if (!string.IsNullOrEmpty(addedCost))
	//					addedCosts = addedCost.Split(';');

	//				for (int i = 0; i < objectBatchNames.Length; i++)
	//				{
	//					List<Transform> objectTransforms = parseObjectNames(objectBatchNames[i]);

	//					string displayName = "Unnamed";

	//					if (displayNames.Length > i)
	//						displayName = displayNames[i];

	//					List<ModuleStructuralNode> structuralNodes = new List<ModuleStructuralNode>();

	//					if (attachNodeRoots.Length > i)
	//						structuralNodes = parseStructuralNodes(attachNodeRoots[i]);

	//					int fuelNumber = 0;

	//					if (fuelTankModes.Length > i)
	//						fuelNumber = parseFuelIntegers(fuelTankModes[i]);

	//					float cost = 0;

	//					if (addedCosts.Length > i)
	//						cost = parseFloat(addedCosts[i]);

	//					float mass = 0;

	//					if (addedMasses.Length > i)
	//						mass = parseFloat(addedMasses[i]);

	//					List<string> dragCubeList = new List<string>();

	//					if (dragCubeNames.Length > i)
	//						dragCubeList = parseDragCubes(dragCubeNames[i]);

	//					meshObjects.Add(new USMeshObject()
	//						{
	//							objectIndex = i,
	//							objectName = displayName,
	//							objectFuelNumber = fuelNumber,
	//							objectAddedCost = cost,
	//							objectAddedMass = mass,
	//							objectAttachNodes = structuralNodes,
	//							objectTransforms = objectTransforms,
	//							objectDragCubes = dragCubeList
	//						});
	//				}
	//			}

	//			if (useFuelSwitchModeOne || useFuelSwitchModeTwo)
	//			{
	//				fuelSwitch = part.GetComponent<USFuelSwitch>();

	//				if (fuelSwitch == null)
	//				{
	//					useFuelSwitchModeOne = false;
	//					useFuelSwitchModeTwo = false;
	//					updateMass = true;
	//					updateCost = true;
	//					debug.debugMessage("no USFuelSwitch module found, despite useFuelSwitchModule being true");
	//				}
	//				else
	//				{
	//					Fields["addedCostValue"].guiActiveEditor = false;
	//					Fields["dryMassInfo"].guiActiveEditor = false;
	//					updateMass = false;
	//					updateCost = false;
	//				}
	//			}
	//			else
	//			{
	//				updateMass = true;
	//				updateCost = true;

	//				if (HighLogic.CurrentGame == null || HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
	//					Fields["addedCostValue"].guiActiveEditor = displayCurrentModeCost;

	//				Fields["dryMassInfo"].guiActiveEditor = displayCurrentModeMass;
	//			}

	//			initialized = true;
	//		}
	//	}

	//	public override string GetInfo()
	//	{
	//		if (showInfo)
	//		{
	//			List<string> variantList;

	//			if (!string.IsNullOrEmpty(objectDisplayNames))
	//				variantList = USTools.parseNames(objectDisplayNames);
	//			else
	//				variantList = USTools.parseNames(objects);

	//			StringBuilder info = StringBuilderCache.Acquire();
	//			info.AppendLine("Part variants available:");

	//			for (int i = 0; i < variantList.Count; i++)
	//				info.AppendLine(variantList[i]);

	//			return info.ToStringAndRelease();
	//		}
	//		else
	//			return base.GetInfo();
	//	}

	//	[KSPEvent(guiActive = false, guiActiveEditor = true, guiActiveUnfocused = false, guiName = "Next part variant")]
	//	public void nextObjectEvent()
	//	{
	//		selectedObject++;

	//		if (selectedObject >= meshObjects.Count)
	//			selectedObject = 0;

	//		switchToObject(selectedObject, true);
	//	}

	//	[KSPEvent(guiActive = false, guiActiveEditor = true, guiActiveUnfocused = false, guiName = "Prev part variant")]
	//	public void previousObjectEvent()
	//	{
	//		selectedObject--;

	//		if (selectedObject < 0)
	//			selectedObject = meshObjects.Count - 1;

	//		switchToObject(selectedObject, true);
	//	}

	//	private void switchToObject(int objectNumber, bool calledByPlayer)
	//	{
	//		setObject(objectNumber, calledByPlayer);

	//		if (updateSymmetry)
	//		{
	//			for (int i = 0; i < part.symmetryCounterparts.Count; i++)
	//			{
	//				USMeshSwitchOld[] symSwitch = part.symmetryCounterparts[i].GetComponents<USMeshSwitchOld>();

	//				for (int j = 0; j < symSwitch.Length; j++)
	//				{
	//					if (symSwitch[j].moduleID == moduleID)
	//					{
	//						symSwitch[j].selectedObject = selectedObject;
	//						symSwitch[j].setObject(objectNumber, calledByPlayer);
	//					}
	//				}
	//			}
	//		}
	//	}

	//	private void setObject(int objectNumber, bool calledByPlayer)
	//	{
	//		initializeData();

	//		for (int i = 0; i < meshObjects.Count; i++)
	//		{
	//			for (int j = 0; j < meshObjects[i].objectTransforms.Count; j++)
	//			{
	//				debug.debugMessage("Setting object enabled");
	//				meshObjects[i].objectTransforms[j].gameObject.SetActive(false);

	//				if (affectColliders)
	//				{
	//					debug.debugMessage("setting collider states");
	//					if (meshObjects[i].objectTransforms[j].gameObject.GetComponent<Collider>() != null)
	//						meshObjects[i].objectTransforms[j].gameObject.GetComponent<Collider>().enabled = false;
	//				}
	//			}

	//			for (int j = 0; j < meshObjects[i].objectAttachNodes.Count; j++)
	//				meshObjects[i].objectAttachNodes[j].SetNodeState(false);

	//			if (meshObjects[objectNumber].objectDragCubes.Count > 0)
	//			{
	//				for (int j = 0; j < part.DragCubes.Cubes.Count; j++)
	//					part.DragCubes.SetCubeWeight(part.DragCubes.Cubes[j].Name, 0);
	//			}
	//		}

	//		selectedMeshObject = meshObjects[objectNumber];

	//		// enable the selected one last because there might be several entries with the same object, and we don't want to disable it after it's been enabled.
	//		for (int i = 0; i < selectedMeshObject.objectTransforms.Count; i++)
	//		{
	//			selectedMeshObject.objectTransforms[i].gameObject.SetActive(true);

	//			if (affectColliders)
	//			{
	//				if (selectedMeshObject.objectTransforms[i].gameObject.GetComponent<Collider>() != null)
	//				{
	//					debug.debugMessage("Setting collider true on new active object");
	//					selectedMeshObject.objectTransforms[i].gameObject.GetComponent<Collider>().enabled = true;
	//				}
	//			}
	//		}

	//		for (int i = 0; i < selectedMeshObject.objectAttachNodes.Count; i++)
	//			selectedMeshObject.objectAttachNodes[i].SetNodeState(true);

	//		if (selectedMeshObject.objectDragCubes.Count > 0 && !string.IsNullOrEmpty(selectedMeshObject.objectDragCubes[0]))
	//			part.DragCubes.SetCubeWeight(selectedMeshObject.objectDragCubes[0], 1);

	//		for (int i = 0; i < part.DragCubes.Cubes.Count; i++)
	//			debug.debugMessage("current drag cube state " + part.DragCubes.Cubes[i].Weight);

	//		//if (useFuelSwitchModeOne)
	//		//{
	//		//	debug.debugMessage("calling on USFuelSwitch tank mode one setup " + objectNumber);

	//		//	if (objectNumber < meshObjects.Count)
	//		//		fuelSwitch.selectTankSetup(selectedMeshObject.objectFuelNumber, calledByPlayer, selectedMeshObject.objectAddedCost, selectedMeshObject.objectAddedMass);
	//		//	else
	//		//		debug.debugMessage("no such fuel tank setup");
	//		//} 
	//		//else if (useFuelSwitchModeTwo)
	//		//{
	//		//	debug.debugMessage("calling on USFuelSwitch tank mode two setup " + objectNumber);

	//		//	if (objectNumber < meshObjects.Count)
	//		//		fuelSwitch.selectModeSetup(selectedMeshObject.objectFuelNumber, calledByPlayer, selectedMeshObject.objectAddedCost, selectedMeshObject.objectAddedMass);
	//		//	else
	//		//		debug.debugMessage("no such fuel tank setup");
	//		//}

	//		GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);

	//		currentObjectName = selectedMeshObject.objectName;
	//	}

	//	private List<Transform> parseObjectNames(string batch)
	//	{
	//		List<Transform> objects = new List<Transform>();

	//		string[] objectNames = batch.Split(',');

	//		for (int i = 0; i < objectNames.Length; i++)
	//		{
	//			Transform newTransform = part.FindModelTransform(objectNames[i].Trim(' '));

	//			if (newTransform != null)
	//			{
	//				objects.Add(newTransform);
	//				debug.debugMessage("USMeshSwitch: added object to list: " + objectNames[i]);
	//			}
	//			else
	//				debug.debugMessage("USMeshSwitch: could not find object " + objectNames[i]);
	//		}

	//		return objects;
	//	}

	//	private List<ModuleStructuralNode> parseStructuralNodes(string batch)
	//	{
	//		List<ModuleStructuralNode> nodes = new List<ModuleStructuralNode>();

	//		string[] objectNames = batch.Split(',');

	//		var modNodes = part.FindModulesImplementing<ModuleStructuralNode>();

	//		for (int i = 0; i < objectNames.Length; i++)
	//		{
	//			for (int j = modNodes.Count - 1; j >= 0; j--)
	//			{
	//				ModuleStructuralNode node = modNodes[j];

	//				if (node == null)
	//					continue;

	//				if (node.rootObject != objectNames[i])
	//					continue;

	//				node.visibilityState = false;

	//				nodes.Add(node);
	//				debug.debugMessage("USMeshSwitch: added structural node to list: " + objectNames[i]);
	//				break;
	//			}
	//		}

	//		return nodes;
	//	}

	//	private List<string> parseDragCubes(string batch)
	//	{
	//		List<string> cubes = new List<string>();

	//		string[] cubeNames = batch.Split(',');

	//		for (int i = 0; i < cubeNames.Length; i++)
	//		{
	//			string cube = cubeNames[i];

	//			for (int j = part.DragCubes.Cubes.Count - 1; j >= 0 ; j--)
	//			{
	//				DragCube d = part.DragCubes.Cubes[j];
					
	//				if (d.Name != cube)
	//					continue;

	//				cubes.Add(cube);
	//				debug.debugMessage("USMeshSwitch: added drag cube to list: " + cube);
	//				break;
	//			}
	//		}

	//		return cubes;
	//	}

	//	private int parseFuelIntegers(string index)
	//	{
	//		int fuelNumber = 0;

	//		if (!int.TryParse(index, out fuelNumber))
	//			return 0;

	//		return fuelNumber;
	//	}

	//	private float parseFloat(string number)
	//	{
	//		float f = 0;

	//		if (!float.TryParse(number, out f))
	//			return 0;

	//		return f;
	//	}

	//	private float UpdateCost()
	//	{
	//		float cost = 0;

	//		if (selectedMeshObject != null)
	//			cost = selectedMeshObject.objectAddedCost;

	//		addedCostValue = basePartCost + cost;

	//		return cost;
	//	}

	//	private float UpdateWeight(Part currentPart)
	//	{
	//		float mass = 0;

	//		if (selectedMeshObject != null)
	//			mass = selectedMeshObject.objectAddedMass;

	//		dryMassInfo = basePartMass + mass;

	//		return mass;
	//	}

	//	public float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
	//	{
	//		if (!updateCost)
	//			return 0;

	//		return UpdateCost();
	//	}

	//	public ModifierChangeWhen GetModuleCostChangeWhen()
	//	{
	//		return ModifierChangeWhen.CONSTANTLY;
	//	}

	//	public float GetModuleMass(float defaultMass, ModifierStagingSituation sit)
	//	{
	//		if (!updateMass)
	//			return 0;

	//		return UpdateWeight(part);
	//	}

	//	public ModifierChangeWhen GetModuleMassChangeWhen()
	//	{
	//		return ModifierChangeWhen.CONSTANTLY;
	//	}

	}
}
