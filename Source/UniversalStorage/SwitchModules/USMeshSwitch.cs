
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
        private EventData<int, int, Part> onUSSwitch;

        private USdebugMessages debug;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (String.IsNullOrEmpty(SwitchID))
                return;

            debug = new USdebugMessages(DebugMode, "USMeshSwitch");

            _SwitchIndices = USTools.parseIntegers(SwitchID).ToArray();

            if (String.IsNullOrEmpty(MeshTransforms))
                return;

            _Transforms = USTools.parseObjectNames(MeshTransforms, part);

            for (int i = 0; i < _Transforms.Count; i++)
            {
                debug.debugMessage(string.Format("Mesh Group: {0}", i));

                for (int j = 0; j < _Transforms[i].Count; j++)
                {
                    debug.debugMessage(string.Format("Mesh Transform: {0}", _Transforms[i][j].name));
                }
            }

            onUSSwitch = GameEvents.FindEvent<EventData<int, int, Part>>("onUSSwitch");

            if (onUSSwitch != null)
                onUSSwitch.Add(onSwitch);

            UpdateMesh();

            if (!HighLogic.LoadedSceneIsFlight || !DeleteUnused)
                return;

            debug.debugMessage("Deleting unused meshes...");

            for (int i = _Transforms.Count - 1; i >= 0; i--)
            {
                if (i == CurrentSelection)
                    continue;

                for (int j = _Transforms[i].Count - 1; j >= 0; j--)
                {
                    debug.debugMessage(string.Format("Delete: {0}", _Transforms[i][j].name));

                    _Transforms[i][j].gameObject.SetActive(false);

                    Destroy(_Transforms[i][j].gameObject);
                }
            }
        }

        private void OnDestroy()
        {
            if (onUSSwitch != null)
                onUSSwitch.Remove(onSwitch);
        }

        private void onSwitch(int index, int selection, Part p)
        {
            if (p != part)
                return;

            debug.debugMessage(string.Format("Switch Received - Index: {0} - Selection: {1} - Part: {2} - Module ID: {3}"
                , index, selection, p.partInfo.name, SwitchID));

            for (int i = _SwitchIndices.Length - 1; i >= 0; i--)
            {
                if (_SwitchIndices[i] == index)
                {
                    debug.debugMessage("Mesh switch activated");
                    CurrentSelection = selection;

                    UpdateMesh();

                    break;
                }
            }
        }

        private void UpdateMesh()
        {
            debug.debugMessage(string.Format("Updating Mesh - Selection: {0} - Count: {1}", CurrentSelection, _Transforms.Count));
            if (_Transforms == null || _Transforms.Count < CurrentSelection + 1)
                return;
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
