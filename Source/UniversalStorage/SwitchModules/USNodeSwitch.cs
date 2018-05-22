
using System;
using System.Collections.Generic;

namespace UniversalStorage
{
    public class USNodeSwitch : PartModule
    {
        [KSPField]
        public string SwitchID = string.Empty;
        [KSPField]
        public string AttachNodes = string.Empty;
        [KSPField(isPersistant = true)]
        public int CurrentSelection = 0;
        [KSPField]
        public bool DebugMode = false;

        private int[] _SwitchIndices;
        private List<List<ModuleStructuralNode>> _Nodes;
        private EventData<int, int, Part> onUSSwitch;

        private USdebugMessages debug;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (String.IsNullOrEmpty(SwitchID))
                return;

            debug = new USdebugMessages(DebugMode, "USNodeSwitch");

            _SwitchIndices = USTools.parseIntegers(SwitchID).ToArray();

            if (String.IsNullOrEmpty(AttachNodes))
                return;

            _Nodes = USTools.parseStructuralNodes(AttachNodes, part);
            
            onUSSwitch = GameEvents.FindEvent<EventData<int, int, Part>>("onUSSwitch");

            if (onUSSwitch != null)
                onUSSwitch.Add(onSwitch);

            UpdateAttachNodes();
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
                    CurrentSelection = selection;

                    UpdateAttachNodes();

                    break;
                }
            }
        }

        private void UpdateAttachNodes()
        {
            if (_Nodes == null || _Nodes.Count <= CurrentSelection)
                return;

            debug.debugMessage("Disabling Nodes");

            for (int i = _Nodes.Count - 1; i >= 0; i--)
            {
                for (int j = _Nodes[i].Count - 1; j >= 0; j--)
                {
                    _Nodes[i][j].SetNodeState(false);
                }
            }

            debug.debugMessage(string.Format("Activating Node Group: {0}", CurrentSelection));

            for (int i = 0; i < _Nodes[CurrentSelection].Count; i++)
            {
                _Nodes[CurrentSelection][i].SetNodeState(true);
            }
        }
    }
}
