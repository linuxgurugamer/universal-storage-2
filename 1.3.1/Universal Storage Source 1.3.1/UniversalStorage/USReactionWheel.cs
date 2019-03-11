
using UnityEngine;

namespace UniversalStorage2
{
    public class USReactionWheel : PartModule
    {
        [KSPField(guiName = "Max Rotation", isPersistant = true, guiActive = true, guiFormat = "N2"), UI_FloatRange(minValue = 0f, maxValue = 1f, stepIncrement = 0.01f, scene = UI_Scene.All, affectSymCounterparts = UI_Scene.All)]
        public float MaxRotation = 5;
        [KSPField(guiName = "Wheel Speed", isPersistant = true, guiActive = true, guiFormat = "0"), UI_FloatRange(minValue = 0f, maxValue = 50f, stepIncrement = 0.1f, scene = UI_Scene.All, affectSymCounterparts = UI_Scene.All)]
        public float WheelSpeed = 10;
        [KSPField(guiName = "Wheel Acceleration", isPersistant = true, guiActive = true, guiFormat = "N1"), UI_FloatRange(minValue = 0f, maxValue = 10f, stepIncrement = 0.1f, scene = UI_Scene.All, affectSymCounterparts = UI_Scene.All)]
        public float WheelAcceleration = 1;
        [KSPField]
        public string WheelTransformName;
        [KSPField]
        public int WheelOrientation = 1;
        [KSPField]
        public bool DebugMode = false;

        private ModuleReactionWheel _reactionWheel;

        private Transform[] _wheelTransforms;

        private float _targetSpeed;
        private float _currentSpeed;
        private Vector3 _rotationAxis;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            
            Fields["MaxRotation"].guiActive = DebugMode;
            Fields["MaxRotation"].guiActiveEditor= DebugMode;
            Fields["WheelSpeed"].guiActive = DebugMode;
            Fields["WheelSpeed"].guiActiveEditor = DebugMode;
            Fields["WheelAcceleration"].guiActive = DebugMode;
            Fields["WheelAcceleration"].guiActiveEditor = DebugMode;

            switch (WheelOrientation)
            {
                case 1:
                    _rotationAxis = Vector3.up;
                    break;
                case 2:
                    _rotationAxis = Vector3.right;
                    break;
                case 3:
                    _rotationAxis = Vector3.forward;
                    break;
                case -1:
                    _rotationAxis = Vector3.up * -1;
                    break;
                case -2:
                    _rotationAxis = Vector3.right * -1;
                    break;
                case -3:
                    _rotationAxis = Vector3.forward * -1;
                    break;
            }
        }

        public override void OnStartFinished(StartState state)
        {
            base.OnStartFinished(state);

            _wheelTransforms = part.FindModelTransforms(WheelTransformName);

            _reactionWheel = part.FindModuleImplementing<ModuleReactionWheel>();
        }

        private void Update()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            if (_reactionWheel == null || _wheelTransforms == null || _wheelTransforms.Length <= 0)
                return;

            _targetSpeed = Mathf.Clamp(_reactionWheel.inputSum, 0, MaxRotation) * WheelSpeed;

            _currentSpeed = Mathf.Lerp(_currentSpeed, _targetSpeed, TimeWarp.deltaTime * WheelAcceleration);

            for (int i = _wheelTransforms.Length - 1; i >= 0; i--)
            {
                if (_wheelTransforms[i] == null || _wheelTransforms[i].gameObject == null)
                    continue;

                if (_wheelTransforms[i].gameObject.activeInHierarchy)
                    _wheelTransforms[i].Rotate(_rotationAxis, _currentSpeed);
            }
        }
    }
}
