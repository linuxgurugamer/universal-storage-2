
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniversalStorage2
{
    public class USMeshSwitch : USBaseSwitch
    {
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
        
        private List<List<Transform>> _Transforms;

        private USdebugMessages debug;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            
            debug = new USdebugMessages(DebugMode, "USMeshSwitch");
            
            if (String.IsNullOrEmpty(MeshTransforms))
                return;

            _Transforms = USTools.parseObjectNames(MeshTransforms, part);

            for (int i = 0; i < _Transforms.Count; i++)
            {
                if (DebugMode)
                    debug.debugMessage(string.Format("Mesh Group: {0}", i));

                if (DebugMode)
                {
                    for (int j = 0; j < _Transforms[i].Count; j++)
                    {
                        debug.debugMessage(string.Format("Mesh Transform: {0}", _Transforms[i][j].name));
                    }
                }
            }
            
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

        protected override void onSwitch(int index, int selection, Part p)
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
