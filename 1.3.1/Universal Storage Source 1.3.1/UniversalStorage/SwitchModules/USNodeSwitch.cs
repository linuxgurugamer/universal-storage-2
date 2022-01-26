
using System;
using System.Collections.Generic;

namespace UniversalStorage2
{
    public class USNodeSwitch : USBaseSwitch
    {
        [KSPField]
        public string AttachNodes = string.Empty;
        [KSPField]
        public bool ShiftNodes = false;
        [KSPField]
        public string ShiftedNodeNames = string.Empty;
        [KSPField(isPersistant = true)]
        public int CurrentSelection = 0;
        [KSPField]
        public bool DebugMode = false;
        
        private List<List<ModuleStructuralNode>> _Nodes;
        
        private USdebugMessages debug;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            
            debug = new USdebugMessages(DebugMode, "USNodeSwitch");
            
            if (String.IsNullOrEmpty(AttachNodes))
                return;

            _Nodes = USTools.parseStructuralNodes(AttachNodes, part);
            
            UpdateAttachNodes();
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
                    int oldSelection = CurrentSelection;

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

            if (DebugMode)
                debug.debugMessage("Disabling Nodes");

            for (int i = _Nodes.Count - 1; i >= 0; i--)
            {
                for (int j = _Nodes[i].Count - 1; j >= 0; j--)
                {
                    _Nodes[i][j].SetNodeState(false);
                }
            }

            if (DebugMode)
                debug.debugMessage(string.Format("Activating Node Group: {0}", CurrentSelection));

            for (int i = 0; i < _Nodes[CurrentSelection].Count; i++)
            {
                _Nodes[CurrentSelection][i].SetNodeState(true);
            }
        }        
    }
}
