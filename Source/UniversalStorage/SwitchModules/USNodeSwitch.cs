
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
        [KSPField]
        public bool ShiftNodes = false;
        [KSPField]
        public string ShiftedNodeNames = string.Empty;
        [KSPField(isPersistant = true)]
        public int CurrentSelection = 0;
        [KSPField]
        public bool DebugMode = false;

        private int[] _SwitchIndices;
        private List<List<ModuleStructuralNode>> _Nodes;
        private EventData<int, int, Part> onUSSwitch;

        private AttachNode[] _ShiftedNodes;

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

            //if (HighLogic.LoadedSceneIsEditor)
            //{
            //    if (ShiftNodes && !string.IsNullOrEmpty(ShiftedNodeNames))
            //    {
            //        _ShiftedNodes = USTools.parseAttachNodes(ShiftedNodeNames, part).ToArray();

            //        if (DebugMode)
            //            debug.debugMessage("Shift Nodes Parsed: " + _ShiftedNodes.Length);
            //    }
            //}
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

                    //if (HighLogic.LoadedSceneIsEditor && ShiftNodes)
                    //    UpdateShiftNodes(oldSelection);

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

        //private void UpdateShiftNodes(int old)
        //{
        //    if (_ShiftedNodes == null)
        //        return;

        //    if (old >= _ShiftedNodes.Length || CurrentSelection >= _ShiftedNodes.Length)
        //        return;

        //    if (DebugMode)
        //        debug.debugMessage(string.Format("Updating Attach Position - Old: {0} - New: {1}", _ShiftedNodes[old].id, _ShiftedNodes[CurrentSelection].id));

        //    AttachNode previousNode = _ShiftedNodes[old];

        //    AttachNode newNode = _ShiftedNodes[CurrentSelection];

        //    if (DebugMode)
        //    {
        //        debug.debugMessage(
        //          string.Format(
        //              "Current Node:\nPosition: {0} - Original Position: {1}\nNew Node:\nPosition: {2} - Original Position: {3}"
        //              , previousNode.position, previousNode.originalPosition, newNode.position, newNode.originalPosition
        //              ));
        //    }

        //    if (DebugMode)
        //    {
        //        debug.debugMessage(
        //          string.Format("Current Node Owner: {0} - Potential Parent: {1}\nAttached Part Root: {2} - Attached Part: {3} ; Attached Part Self: {4}"
        //          , previousNode.owner == null ? "Null" : previousNode.owner.partInfo.name
        //          , previousNode.owner == null ? "Null" : previousNode.owner.potentialParent.partInfo.name
        //          , previousNode.attachedPart == null ? "Null" : (previousNode.attachedPart == EditorLogic.RootPart).ToString()
        //          , previousNode.attachedPart == null ? "Null" : previousNode.attachedPart.partInfo.name
        //          , previousNode.attachedPart == null ? "Null" : (previousNode.attachedPart == part).ToString()
        //          ));
        //    }

        //    ModulePartVariants.UpdatePartPosition(previousNode, newNode);

        //    newNode.attachedPart = previousNode.attachedPart;

        //    previousNode.attachedPart = null;
        //}
    }
}
