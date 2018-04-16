using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;

namespace UniversalStorage
{
	public static class USTools
	{
		public static List<int> parseIntegers(string stringOfInts, char sep = ';')
		{
			List<int> newIntList = new List<int>();
			string[] valueArray = stringOfInts.Split(sep);
			for (int i = 0; i < valueArray.Length; i++)
			{
				int newValue = 0;

                if (int.TryParse(valueArray[i], out newValue))
                {
                    newIntList.Add(newValue);
                }
                else
                {
                    USdebugMessages.USStaticLog("Error parsing: Invalid integer - {0}", valueArray[i]);
                }
			}
			return newIntList;
		}

		public static List<double> parseDoubles(string stringOfDoubles, char sep = ';')
		{
			List<double> list = new List<double>();
			string[] array = stringOfDoubles.Trim().Split(sep);
			for (int i = 0; i < array.Length; i++)
			{
				double item = 0f;
				if (double.TryParse(array[i].Trim(), out item))
				{
					list.Add(item);
				}
				else
				{
                    USdebugMessages.USStaticLog("Error parsing: Invalid float - {0}", array[i]);
				}
			}
			return list;
		}

		public static List<string> parseNames(string names)
		{
			return parseNames(names, false, true, string.Empty);
		}

		public static List<string> parseNames(string names, bool replaceBackslashErrors)
		{
			return parseNames(names, replaceBackslashErrors, true, string.Empty);
		}

		public static List<string> parseNames(string names, bool replaceBackslashErrors, bool trimWhiteSpace, string prefix)
		{
			List<string> source = names.Split(';').ToList<string>();
			for (int i = source.Count - 1; i >= 0; i--)
			{
				if (source[i] == string.Empty)
				{
					source.RemoveAt(i);
				}
			}
			if (trimWhiteSpace)
			{
				for (int i = 0; i < source.Count; i++)
				{
					source[i] = source[i].Trim(' ');
				}
			}
			if (prefix != string.Empty)
			{
				for (int i = 0; i < source.Count; i++)
				{
					source[i] = prefix + source[i];
				}
			}
			if (replaceBackslashErrors)
			{
				for (int i = 0; i < source.Count; i++)
				{
					source[i] = source[i].Replace('\\', '/');
				}
			}
			return source.ToList<string>();
		}

        public static List<List<Transform>> parseObjectNames(string batch, Part part)
        {
            List<List<Transform>> transformBatches = new List<List<Transform>>();

            string[] batches = batch.Split(';');

            for (int i = 0; i < batches.Length; i++)
            {
                List<Transform> transforms = new List<Transform>();

                string[] objectNames = batches[i].Split(',');

                for (int j = 0; j < objectNames.Length; j++)
                {
                    Transform newTransform = part.FindModelTransform(objectNames[j].Trim(' '));

                    if (newTransform != null)
                        transforms.Add(newTransform);
                }

                transformBatches.Add(transforms);
            }

            return transformBatches;
        }

        public static List<Transform> parseTransformNames(string batch, Part part)
        {
            List<Transform> transforms = new List<Transform>();

            string[] objectNames = batch.Split(';');

            for (int i = 0; i < objectNames.Length; i++)
            {
                Transform newTransform = part.FindModelTransform(objectNames[i].Trim(' '));

                if (newTransform != null)
                {
                    transforms.Add(newTransform);
                }
            }

            return transforms;
        }

        public static List<List<string>> parseDragCubes(string batch, Part part)
        {
            List<List<string>> cubeBatches = new List<List<string>>();

            string[] batches = batch.Split(';');

            for (int i = 0; i < batches.Length; i++)
            {
                List<string> cubes = new List<string>();

                string[] cubeNames = batches[i].Split(',');

                for (int j = 0; j < cubeNames.Length; j++)
                {
                    string cube = cubeNames[j];

                    for (int k = part.DragCubes.Cubes.Count - 1; k >= 0; k--)
                    {
                        DragCube d = part.DragCubes.Cubes[k];

                        if (d.Name != cube)
                            continue;

                        cubes.Add(cube);
                        break;
                    }
                }

                cubeBatches.Add(cubes);
            }

            return cubeBatches;
        }

        public static List<List<ModuleStructuralNode>> parseStructuralNodes(string batch, Part part)
        {
            List<List<ModuleStructuralNode>> nodeBatches = new List<List<ModuleStructuralNode>>();

            string[] batches = batch.Split(';');

            var modNodes = part.FindModulesImplementing<ModuleStructuralNode>();

            for (int i = 0; i < batches.Length; i++)
            {
                List<ModuleStructuralNode> nodes = new List<ModuleStructuralNode>();

                string[] nodeNames = batches[i].Split(',');

                for (int j = 0; j < nodeNames.Length; j++)
                {
                    for (int k = modNodes.Count - 1; k >= 0; k--)
                    {
                        ModuleStructuralNode node = modNodes[k];

                        if (node == null)
                            continue;

                        if (node.rootObject != nodeNames[j])
                            continue;

                        node.visibilityState = false;

                        nodes.Add(node);
                        break;
                    }

                    nodeBatches.Add(nodes);
                }
            }

            return nodeBatches;
        }

        // Code from https://github.com/Swamp-Ig/KSPAPIExtensions/blob/master/Source/Utils/KSPUtils.cs#L62
        private static FieldInfo windowListField;

		/// <summary>
		/// Find the UIPartActionWindow for a part. Usually this is useful just to mark it as dirty.
		/// </summary>
		public static UIPartActionWindow FindActionWindow(this Part part)
		{
			if (part == null)
				return null;

			// We need to do quite a bit of piss-farting about with reflection to 
			// dig the thing out. We could just use Object.Find, but that requires hitting a heap more objects.
			UIPartActionController controller = UIPartActionController.Instance;
			if (controller == null)
				return null;

			if (windowListField == null)
			{
				Type cntrType = typeof(UIPartActionController);
				foreach (FieldInfo info in cntrType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
				{
					if (info.FieldType == typeof(List<UIPartActionWindow>))
					{
						windowListField = info;
						goto foundField;
					}
				}
				Debug.LogWarning("*PartUtils* Unable to find UIPartActionWindow list");
				return null;
			}
		foundField:

			List<UIPartActionWindow> uiPartActionWindows = (List<UIPartActionWindow>)windowListField.GetValue(controller);
			if (uiPartActionWindows == null)
				return null;

			return uiPartActionWindows.FirstOrDefault(window => window != null && window.part == part);
		}
	}
}
