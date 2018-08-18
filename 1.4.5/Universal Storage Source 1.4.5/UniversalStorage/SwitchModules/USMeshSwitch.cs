
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniversalStorage
{
    public class USMeshSwitch : PartModule
    {
        [KSPField]
        public string SwitchID = string.Empty;
        [KSPField]
        public string MeshTransforms = string.Empty;
        [KSPField]
        public bool AffectColliders = true;
        [KSPField(isPersistant = true)]
        public int CurrentSelection = 0;
        [KSPField]
        public bool DebugMode = false;
        [KSPField]
        public bool DeleteUnused = true;

        private int[] _SwitchIndices;
        private List<List<Transform>> _Transforms;
        private List<List<string>> _TransformNames;
        private EventData<int, int, Part> onUSSwitch;
        private EventData<int, int, AvailablePart, Transform> onUSEditorIconSwitch;

        private USdebugMessages debug;

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            LoadMeshData();

            if (!HighLogic.LoadedSceneIsFlight && !HighLogic.LoadedSceneIsEditor)
            {
                LoadMeshNameData();

                onUSEditorIconSwitch = GameEvents.FindEvent<EventData<int, int, AvailablePart, Transform>>("onUSEditorIconSwitch");

                if (onUSEditorIconSwitch != null)
                    onUSEditorIconSwitch.Add(onEditorIconSwitch);
            }
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            debug = new USdebugMessages(DebugMode, "USMeshSwitch");

            if (state == StartState.Editor)
                LoadMeshData();

            onUSSwitch = GameEvents.FindEvent<EventData<int, int, Part>>("onUSSwitch");

            if (onUSSwitch != null)
                onUSSwitch.Add(onSwitch);

            UpdateMesh();

            if (!HighLogic.LoadedSceneIsFlight || !DeleteUnused)
                return;

            if (DebugMode)
                debug.debugMessage("Deleting unused meshes...");

            for (int i = _Transforms.Count - 1; i >= 0; i--)
            {
                if (i == CurrentSelection)
                    continue;

                for (int j = _Transforms[i].Count - 1; j >= 0; j--)
                {
                    if (DebugMode)
                        debug.debugMessage(string.Format("Delete: {0}", _Transforms[i][j].name));

                    _Transforms[i][j].gameObject.SetActive(false);

                    Destroy(_Transforms[i][j].gameObject);
                }
            }
        }

        private void LoadMeshData()
        {
            if (String.IsNullOrEmpty(SwitchID))
                return;

            _SwitchIndices = USTools.parseIntegers(SwitchID).ToArray();

            if (String.IsNullOrEmpty(MeshTransforms))
                return;

            _Transforms = USTools.parseObjectNames(MeshTransforms, part);
        }

        private void LoadMeshNameData()
        {
            _TransformNames = new List<List<string>>();

            for (int i = 0; i < _Transforms.Count; i++)
            {
                if (DebugMode)
                    debug.debugMessage(string.Format("Mesh Group: {0}", i));

                _TransformNames.Add(new List<string>());

                for (int j = 0; j < _Transforms[i].Count; j++)
                {
                    if (DebugMode)
                        debug.debugMessage(string.Format("Mesh Transform: {0}", _Transforms[i][j].name));

                    _TransformNames[i].Add(_Transforms[i][j].name);
                }
            }
        }

        private void OnDestroy()
        {
            if (onUSSwitch != null)
                onUSSwitch.Remove(onSwitch);

            if (onUSEditorIconSwitch != null)
                onUSEditorIconSwitch.Remove(onEditorIconSwitch);
        }
        
        private void onSwitch(int index, int selection, Part p)
        {
            if (p != part)
                return;

            if (DebugMode)
            {
                debug.debugMessage(string.Format("Switch Received - Index: {0} - Selection: {1} - Part: {2} - Module ID: {3}"
                  , index, selection, p.partInfo.name, SwitchID));
            }

            for (int i = _SwitchIndices.Length - 1; i >= 0; i--)
            {
                if (_SwitchIndices[i] == index)
                {
                    if (DebugMode)
                        debug.debugMessage("Mesh switch activated");

                    CurrentSelection = selection;

                    UpdateMesh();

                    break;
                }
            }
        }

        private void onEditorIconSwitch(int index, int selection, AvailablePart partInfo, Transform icon)
        {
            //USdebugMessages.USStaticLog("Editor Icon Switch Event Fired: {0}", partInfo.title);

            if (partInfo != part.partInfo)
                return;

            for (int i = _SwitchIndices.Length - 1; i >= 0; i--)
            {
                if (_SwitchIndices[i] == index)
                {
                    //USdebugMessages.USStaticLog("Editor Mesh switch activated: {0}", selection);

                    UpdateEditorMesh(selection, icon);

                    break;
                }
            }
        }

        private void UpdateEditorMesh(int selection, Transform icon)
        {
            var children = icon.GetComponentsInChildren<Transform>(true);

            for (int i = _TransformNames.Count - 1; i >= 0; i--)
            {
                for (int j = _TransformNames[i].Count - 1; j >= 0; j--)
                {
                    for (int k = children.Length - 1; k >= 0; k--)
                    {
                        //USdebugMessages.USStaticLog("Searching for icon transform: {0}", _TransformNames[i][j]);

                        if (children[k].name == _TransformNames[i][j])
                        {
                            //USdebugMessages.USStaticLog("Disabling icon transform: {0}", children[k].name);

                            children[k].gameObject.SetActive(false);
                        }
                    }
                }
            }

            for (int i = 0; i < _TransformNames[selection].Count; i++)
            {
                for (int j = children.Length - 1; j >= 0; j--)
                {
                    if (children[j].name == _TransformNames[selection][i])
                    {
                        //USdebugMessages.USStaticLog("Enabling icon transform: {0}", children[j].name);

                        children[j].gameObject.SetActive(true);
                    }
                }
            }
        }

        private void UpdateMesh()
        {
            if (DebugMode)
                debug.debugMessage(string.Format("Updating Mesh - Selection: {0} - Count: {1}", CurrentSelection, _Transforms.Count));

            if (_Transforms == null || _Transforms.Count <= CurrentSelection)
                return;

            if (DebugMode)
                debug.debugMessage("Turning off meshes");

            for (int i = _Transforms.Count - 1; i >= 0; i--)
            {
                for (int j = _Transforms[i].Count - 1; j >= 0; j--)
                {
                    _Transforms[i][j].gameObject.SetActive(false);

                    if (AffectColliders)
                    {
                        Collider coll = _Transforms[i][j].gameObject.GetComponent<Collider>();

                        if (coll != null)
                            coll.enabled = false;
                    }
                }
            }

            for (int i = 0; i < _Transforms[CurrentSelection].Count; i++)
            {
                if (DebugMode)
                    debug.debugMessage("Activating selected meshes");

                _Transforms[CurrentSelection][i].gameObject.SetActive(true);

                if (AffectColliders)
                {
                    Collider coll = _Transforms[CurrentSelection][i].gameObject.GetComponent<Collider>();

                    if (coll != null)
                        coll.enabled = true;
                }
            }
        }
    }

}
