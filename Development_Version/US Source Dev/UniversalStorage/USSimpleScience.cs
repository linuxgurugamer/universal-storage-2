
using System.Linq;
using UnityEngine;
using KSP.Localization;

namespace UniversalStorage2
{
    public class USSimpleScience : ModuleScienceExperiment, IScienceDataContainer
    {
        [KSPField]
        public string deployAnimationName = null;
        [KSPField]
        public string endEventGUIName = "Retract";
        [KSPField]
        public string startEventGUIName = "Deploy";
        [KSPField]
        public string toggleEventGUIName = "Toggle";
        [KSPField]
        public bool deployAvailableInEVA = true;
        [KSPField]
        public bool deployAvailableInVessel = true;
        [KSPField]
        public bool deployAvailableInEditor = true;
        [KSPField]
        public bool deployTriggersExperiment = true;
        [KSPField]
        public bool dualExerimentPart = false;
        [KSPField]
        public float animSpeed = 1f;

        [KSPField(isPersistant = true)]
        public bool IsDeployed;

        private Animation _deployAnim;

        private BaseEvent _tglEvent;
        private BaseAction _tglAction;

        private BaseEvent _baseDeployExperiment;
        private BaseEvent _baseDeployExperimentExternal;
        private BaseAction _baseDeployAction;

        private string _localizedDeployString = "Deploy";
        private string _localizedRetractString = "Retract";
        private string _localizedToggleActionString = "Toggle";

        private USSimpleScience _otherUSScienceModule;

        public override void OnAwake()
        {
            base.OnAwake();

            _localizedDeployString = Localizer.Format(startEventGUIName);
            _localizedRetractString = Localizer.Format(endEventGUIName);
            _localizedToggleActionString = Localizer.Format(toggleEventGUIName);

            _tglEvent = Events["ToggleEvent"];
            _tglAction = Actions["ToggleAction"];

            _baseDeployExperiment = Events["DeployExperiment"];
            _baseDeployExperimentExternal = Events["DeployExperimentExternal"];
            _baseDeployAction = Actions["DeployAction"];
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (dualExerimentPart)
            {
                var simpleScience = part.FindModulesImplementing<USSimpleScience>();

                for (int i = simpleScience.Count - 1; i >= 0; i--)
                {
                    if (simpleScience[i] != this)
                    {
                        _otherUSScienceModule = simpleScience[i];
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(deployAnimationName))
            {
                _deployAnim = part.FindModelAnimators(deployAnimationName).FirstOrDefault();

                if (_deployAnim != null)
                    _deployAnim.playAutomatically = false;
            }

            _tglEvent.guiActiveUnfocused = deployAvailableInEVA;
            _tglEvent.guiActive = deployAvailableInVessel;
            _tglEvent.guiActiveEditor = deployAvailableInEditor;
            _tglEvent.unfocusedRange = interactionRange;
            _tglEvent.active = true;
            _tglAction.active = true;
            _tglAction.guiName = _localizedToggleActionString;

            if (IsDeployed)
            {
                _tglEvent.guiName = _localizedRetractString;

                _baseDeployExperiment.active = true;
                _baseDeployExperimentExternal.active = true;
                //_baseDeployAction.active = true;

                if (_deployAnim != null)
                {
                    Animate(1, 1, WrapMode.Default, deployAnimationName, _deployAnim);
                }
            }
            else
            {
                _tglEvent.guiName = _localizedDeployString;

                _baseDeployExperiment.active = false;
                _baseDeployExperimentExternal.active = false;
                //_baseDeployAction.active = false;

                if (_deployAnim != null)
                {
                    Animate(-1, 0, WrapMode.Default, deployAnimationName, _deployAnim);
                }
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            _baseDeployExperiment.active = IsDeployed;
            _baseDeployExperimentExternal.active = IsDeployed;
        }

        private void Animate(float speed, float time, WrapMode wrap, string name, Animation a)
        {
            if (a != null)
            {
                a[name].speed = speed;

                if (!a.IsPlaying(name))
                {
                    a[name].wrapMode = wrap;
                    a[name].normalizedTime = time;
                    a.Blend(name, 1);
                }
            }
        }

        [KSPEvent(guiActive = true, guiName = "Deploy", active = true)]
        private void ToggleEvent()
        {
            if (IsDeployed)
            {
                Animate(-1 * animSpeed, 1, WrapMode.Default, deployAnimationName, _deployAnim);
                IsDeployed = false;

                _baseDeployExperiment.active = false;
                _baseDeployExperimentExternal.active = false;
                //_baseDeployAction.active = false;

                _tglEvent.guiName = _localizedDeployString;
            }
            else
            {
                Animate(1 * animSpeed, 0, WrapMode.Default, deployAnimationName, _deployAnim);
                IsDeployed = true;

                _baseDeployExperiment.active = true;
                _baseDeployExperimentExternal.active = true;
                //_baseDeployAction.active = true;

                _tglEvent.guiName = _localizedRetractString;

                if (deployTriggersExperiment)
                    base.DeployExperiment();
            }
        }

        [KSPAction("Toggle")]
        private void ToggleAction(KSPActionParam param)
        {
            if (deployAvailableInVessel)
                ToggleEvent();
        }

        new public void DeployAction(KSPActionParam param)
        {
            if (!IsDeployed)
            {
                Animate(1 * animSpeed, 0, WrapMode.Default, deployAnimationName, _deployAnim);
                IsDeployed = true;

                _baseDeployExperiment.active = true;
                _baseDeployExperimentExternal.active = true;

                _tglEvent.guiName = _localizedRetractString;
            }

            base.DeployAction(param);
        }

        void IScienceDataContainer.ReturnData(ScienceData data)
        {
            ReturnData(data);
        }

        new private void ReturnData(ScienceData data)
        {
            if (data == null || experiment == null)
                return;

            if (data.subjectID.StartsWith(experiment.id))
            {
                base.ReturnData(data);
            }
            else if (dualExerimentPart)
            {
                if (_otherUSScienceModule != null && _otherUSScienceModule.experiment != null)
                {
                    if (data.subjectID.StartsWith(_otherUSScienceModule.experiment.id))
                    {
                        _otherUSScienceModule.ReturnData(data);
                    }
                }
            }
        }

    }
}
