using System;
using System.Collections;
using UnityEngine;

namespace UniversalStorage
{
    public class USJettisonSwitch : PartModule
    {
        [KSPField]
        public string SwitchID = string.Empty;
        [KSPField]
        public string JettisonTransforms = string.Empty;
        [KSPField]
        public int JettisonModuleIndex = -1;
        [KSPField]
        public bool ShowJettisonUI = false;
        [KSPField(isPersistant = true)]
        public bool Jettisoned;
        [KSPField]
        public bool DebugMode = false;

        private int[] _SwitchIndices;
        private Transform[] _JettisonTransforms;
        private EventData<int, int, Part> onUSSwitch;

        private Transform _jettisonTransform;
        private ModuleJettison _jettisonModule;

        private USdebugMessages debug;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (JettisonModuleIndex < 0)
                return;

            if (part.Modules.Count <= JettisonModuleIndex)
                return;

            if (String.IsNullOrEmpty(SwitchID))
                return;

            _SwitchIndices = USTools.parseIntegers(SwitchID).ToArray();

            if (String.IsNullOrEmpty(JettisonTransforms))
                return;

            _JettisonTransforms = USTools.parseTransformNames(JettisonTransforms, part).ToArray();

            debug = new USdebugMessages(DebugMode, "USJettisonSwitch");

            if (part.Modules[JettisonModuleIndex] is ModuleJettison)
                _jettisonModule = part.Modules[JettisonModuleIndex] as ModuleJettison;
            
            onUSSwitch = GameEvents.FindEvent<EventData<int, int, Part>>("onUSSwitch");

            if (onUSSwitch != null)
                onUSSwitch.Add(onSwitch);
            
            Events["OnJettison"].active = ShowJettisonUI && !Jettisoned;

            if (Jettisoned)
                DeactivateTransforms();
            else
                StartCoroutine(SetJettisonTransform());
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

            for (int i = _SwitchIndices.Length - 1; i >= 0; i--)
            {
                if (_SwitchIndices[i] == index)
                {
                    StartCoroutine(SetJettisonTransform());
                    break;
                }
            }
        }

        [KSPEvent(guiName = "Jettison Doors", guiActive = false, guiActiveUnfocused = false, guiActiveEditor = false)]
        public void OnJettison()
        {
            if (_jettisonModule == null)
                return;

            if (_jettisonTransform != null)
                debug.debugMessage(string.Format("Jettison transform: {0}", _jettisonTransform.name));

            _jettisonModule.JettisonAction(null);

            Jettisoned = true;

            Events["OnJettison"].active = false;
        }

        private void DeactivateTransforms()
        {
            for (int i = _JettisonTransforms.Length - 1; i >= 0; i--)
            {
                _JettisonTransforms[i].gameObject.SetActive(false);                
            }
        }

        private IEnumerator SetJettisonTransform()
        {
            int timer = 0;

            while (timer < 10)
            {
                timer++;
                yield return null;
            }

            debug.debugMessage("Setting Jettison Transform...");

            for (int i = _JettisonTransforms.Length - 1; i >= 0; i--)
            {
                Transform t = _JettisonTransforms[i];

                debug.debugMessage(String.Format("Transform check: {0}", t.name));

                if (!t.gameObject.activeInHierarchy)
                    continue;

                _jettisonTransform = t;

                debug.debugMessage(String.Format("Transform active: {0}", t.name));

                if (_jettisonModule != null)
                    _jettisonModule.jettisonTransform = t;
            }
        }
                
    }
}
