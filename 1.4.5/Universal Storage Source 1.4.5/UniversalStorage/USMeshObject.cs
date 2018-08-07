using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniversalStorage
{
	public class USMeshObject
	{
		public int objectIndex;
		public string objectName;
		public int objectFuelNumber;
		public float objectAddedMass;
		public float objectAddedCost;
		public List<ModuleStructuralNode> objectAttachNodes;
		public List<Transform> objectTransforms;
		public List<string> objectDragCubes;
	}
}
