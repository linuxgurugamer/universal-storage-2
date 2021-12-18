
using System;
using System.Collections.Generic;
using System.Collections;

namespace UniversalStorage2
{
    public class USDragSwitch : USBaseSwitch
    {
        [KSPField]
        public string DragCubes = string.Empty;
        [KSPField(isPersistant = true)]
        public int CurrentSelection = 0;
        [KSPField]
        public bool DebugMode = false;
        
        private List<List<string>> _DragCubes;
        private bool _Loaded;

        private USdebugMessages debug;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            debug = new USdebugMessages(DebugMode, "USDragSwitch");
            
            if (String.IsNullOrEmpty(DragCubes))
                return;

            _DragCubes = USTools.parseDragCubes(DragCubes, part);
            
            UpdateDragCube();

            _Loaded = true;
        }
        
        protected override void onSwitch(int index, int selection, Part p)
        {
            if (p != part)
                return;

            for (int i = _SwitchIndices.Length - 1; i >= 0; i--)
            {
                if (_SwitchIndices[i] == index)
                {
                    CurrentSelection = selection;

                    UpdateDragCube();

                    break;
                }
            }
        }

        private void UpdateDragCube()
        {
            if (_DragCubes != null
                        && _DragCubes.Count > CurrentSelection
                        && _DragCubes[CurrentSelection].Count > 0
                        && !String.IsNullOrEmpty(_DragCubes[CurrentSelection][0]))
            {
                part.DragCubes.SetCubeWeight(_DragCubes[CurrentSelection][0], 1);
            }
        }

        public IEnumerator UpdateDragCubes(float value)
        {
            while (!_Loaded)
                yield return null;

            if (DebugMode)
                debug.debugMessage(string.Format("Updating Drag Cube from animation: {0:N3}", value));

            if (_DragCubes != null
                            && _DragCubes.Count > CurrentSelection
                            && _DragCubes[CurrentSelection].Count > 0
                            && !String.IsNullOrEmpty(_DragCubes[CurrentSelection][0])
                            && !String.IsNullOrEmpty(_DragCubes[CurrentSelection][1]))
            {
                part.DragCubes.SetCubeWeight(_DragCubes[CurrentSelection][0], value);
                part.DragCubes.SetCubeWeight(_DragCubes[CurrentSelection][1], 1 - value);
            }
        }
    }
}
