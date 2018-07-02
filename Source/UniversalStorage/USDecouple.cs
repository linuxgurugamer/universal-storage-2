
using UnityEngine;
using KSP.Localization;

namespace UniversalStorage
{
    public class USDecouple : PartModule
    {
        [KSPField]
        public string DecoupleAnimationName;
        [KSPField(guiName = "Decouple Animation Time", isPersistant = true, guiActive = true, guiFormat = "N2"), UI_FloatRange(minValue = 0f, maxValue = 1f, stepIncrement = 0.01f, scene = UI_Scene.All, affectSymCounterparts = UI_Scene.All)]
        public float DecoupleTime = -1;
        [KSPField]
        public bool DecoupleEVA = true;
        [KSPField]
        public string DecoupleEventName = "Decouple";
        [KSPField]
        public string DecoupleActionName = "Decouple";
        [KSPField]
        public float AnimationSpeed = 1;
        [KSPField(isPersistant = true)]
        public bool Staged = true;
        [KSPField]
        public bool StageOption = true;
        [KSPField(isPersistant = true)]
        public bool Decoupled = false;
        [KSPField]
        public bool DebugMode = false;

        private BaseEvent decoupleEvent;
        private BaseEvent stagingEvent;
        private BaseAction decoupleAction;

        private ModuleDecouple _decoupler;
        private Animation[] _decoupleAnimation;

        private bool _decouplePlaying;

        private USdebugMessages debug;

        private string _localizedDecoupleEventName = "Decouple";
        private string _localizedDecoupleActionName = "Decouple";

        public override void OnAwake()
        {
            base.OnAwake();

            decoupleEvent = Events["DecoupleEvent"];
            stagingEvent = Events["ToggleUSStaging"];
            decoupleAction = Actions["DecoupleAction"];
            decoupleEvent.active = false;
            decoupleAction.active = false;
            stagingEvent.active = false;

            _localizedDecoupleEventName = Localizer.Format(DecoupleEventName);
            _localizedDecoupleActionName = Localizer.Format(DecoupleActionName);
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            debug = new USdebugMessages(DebugMode, "USDecouple");
            
            if (GameSettings.ADVANCED_TWEAKABLES)
                stagingEvent.active = false;
            else
                stagingEvent.active = StageOption;

            stagingEnabled = StageOption && Staged;

            decoupleEvent.guiActiveUnfocused = DecoupleEVA;
            decoupleEvent.guiName = _localizedDecoupleEventName;
            decoupleEvent.active = !Decoupled;
            
            decoupleAction.guiName = _localizedDecoupleActionName;
            decoupleAction.active = !Decoupled;

            Fields["DecoupleTime"].guiActive = DebugMode;
            Fields["DecoupleTime"].guiActiveEditor = DebugMode;
        }

        public override void OnStartFinished(StartState state)
        {
            base.OnStartFinished(state);

            _decoupler = part.FindModuleImplementing<ModuleDecouple>();

            if (_decoupler != null)
            {
                _decoupler.stagingEnabled = false;
                _decoupler.staged = false;
                _decoupler.Events["Decouple"].active = false;
                _decoupler.Actions["DecoupleAction"].active = false;
            }

            _decoupleAnimation = part.FindModelAnimators(DecoupleAnimationName);
            
            part.UpdateStageability(true, true);            
        }

        private void FixedUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            if (!_decouplePlaying)
                return;

            float time = 0;

            for (int i = _decoupleAnimation.Length - 1; i >= 0; i--)
            {
                Animation anim = _decoupleAnimation[i];

                if (anim == null)
                    continue;

                if (anim.gameObject.activeInHierarchy)
                {
                    if (anim.IsPlaying(DecoupleAnimationName))
                    {
                        time = anim[DecoupleAnimationName].normalizedTime;
                    }
                }
            }

            if (time >= DecoupleTime)
            {
                _decouplePlaying = false;

                if (_decoupler == null)
                    return;

                _decoupler.Decouple();

                debug.debugMessage("Firing stock Module Decouple");
            }
        }

        public override string GetStagingDisableText()
        {
            return Localizer.Format("#autoLOC_240329");
        }

        public override string GetStagingEnableText()
        {
            return Localizer.Format("#autoLOC_240328");
        }

        public override void OnActive()
        {
            if (StageOption && Staged)
                DecoupleEvent();
        }

        public override bool IsStageable()
        {
            return StageOption;
        }

        public override bool StagingEnabled()
        {
            return base.StagingEnabled() && StageOption;
        }

        public override bool StagingToggleEnabledEditor()
        {
            return StageOption;
        }

        public override bool StagingToggleEnabledFlight()
        {
            return base.StagingToggleEnabledFlight();
        }

        [KSPEvent(name = "ToggleUSStaging", guiName = "Toggle Staging", guiActive = false, guiActiveEditor = true, active = false)]
        public void ToggleUSStaging()
        {
            if (_decoupler != null)
            {
                _decoupler.stagingEnabled = false;
                _decoupler.staged = false;
            }

            if (!StageOption)
                return;
            
            stagingEnabled = !stagingEnabled;
            Staged = stagingEnabled;

            stagingEvent.guiName = stagingEnabled ? GetStagingDisableText() : GetStagingEnableText();

            part.UpdateStageability(true, true);
        }

        [KSPEvent(name = "DecoupleEvent", guiName = "Decouple", guiActive = true, guiActiveUnfocused = false, unfocusedRange = 5f, guiActiveEditor = false, active = false)]
        public void DecoupleEvent()
        {
            decoupleEvent.active = false;
            decoupleAction.active = false;

            if (Decoupled)
                return;

            Decoupled = true;

            _decouplePlaying = true;

            for (int i = _decoupleAnimation.Length - 1; i >= 0; i--)
            {
                Animate(_decoupleAnimation[i], AnimationSpeed);
            }
        }

        [KSPAction("Decouple")]
        public void DecoupleAction(KSPActionParam param)
        {
            decoupleEvent.active = false;
            decoupleAction.active = false;

            if (Decoupled)
                return;

            Decoupled = true;

            for (int i = _decoupleAnimation.Length - 1; i >= 0; i--)
            {
                Animate(_decoupleAnimation[i], AnimationSpeed);
            }
        }

        private void Animate(Animation anim, float speed)
        {
            if (anim == null)
                return;

            anim[DecoupleAnimationName].speed = speed;

            if (!anim.IsPlaying(DecoupleAnimationName))
            {
                anim[DecoupleAnimationName].enabled = true;
                anim[DecoupleAnimationName].normalizedTime = 0;
                anim.Blend(DecoupleAnimationName);
            }
        }
    }
}
