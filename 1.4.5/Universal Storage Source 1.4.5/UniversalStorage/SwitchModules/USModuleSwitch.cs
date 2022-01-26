using System;
using System.Collections.Generic;

namespace UniversalStorage
{
    public class USModuleSwitch : USBaseSwitch
    {
        [KSPField]
        public string TargetModule = string.Empty;
        [KSPField]
        public string TargetFields = string.Empty;
        [KSPField]
        public string TargetValues = string.Empty;
        [KSPField(isPersistant = true)]
        public int CurrentSelection = 0;
        [KSPField]
        public bool DebugMode = false;
        
        private string[] _Fields;
        private List<List<string>> _Values;
        private PartModule _TargetModule;
        private USdebugMessages debug;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            
            debug = new USdebugMessages(DebugMode, "USModuleSwitch");
            
            if (!string.IsNullOrEmpty(TargetModule))
                _TargetModule = part.Modules[TargetModule];

            if (!string.IsNullOrEmpty(TargetFields))
                _Fields = USTools.parseNames(TargetFields, '|').ToArray();

            if (!string.IsNullOrEmpty(TargetValues))
                _Values = USTools.parseDoubleStrings(TargetValues);
        }

        public override void OnStartFinished(StartState state)
        {
            base.OnStartFinished(state);

            if (_TargetModule == null)
                return;
            
            if (DebugMode)
                debug.debugMessage("US Module Switch Initialized");

            UpdateModule();            
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

                    UpdateModule();

                    break;
                }
            }
        }

        private void UpdateModule()
        {
            if (_TargetModule == null)
                return;
            
            for (int i = _Fields.Length - 1; i >= 0; i--)
            {
                if (_TargetModule.Fields[_Fields[i]] == null)
                    continue;

                if (_Values.Count > i && _Values[i].Count > CurrentSelection)
                {
                    var field = _TargetModule.Fields[_Fields[i]];
                    
                    field.Read(_Values[i][CurrentSelection], _TargetModule);
                    
                    if (DebugMode)
                    {
                        debug.debugMessage(string.Format("Updating Fields For Target Module: {0}\nTarget Field: {1} - Value: {2}"
                          , _TargetModule.ClassName, _Fields[i], _Values[i][CurrentSelection]));
                    }
                }
            }
        }
    }
}
