using System;
using System.Linq;
using UnityEngine;
using KSP.Localization;

namespace UniversalStorage2
{
	public class USAnimateGeneric : PartModule, IScalarModule
	{
		[KSPField(guiActive = true, guiActiveEditor = true)]
		public string status = "Locked";
        [KSPField]
        public string statusTitle = "Status";
        [KSPField]
		public string primaryAnimationName;
		[KSPField]
		public string secondaryAnimationName;
		[KSPField]
		public string primaryStartEventGUIName = "Deploy Primary Bays";
		[KSPField]
		public string primaryEndEventGUIName = "Retract Primary Bays";
		[KSPField]
		public string primaryToggleActionName = "Toggle Primary Bays";
        [KSPField]
        public string lockPrimaryDoorName = "Lock Primary Bays";
        [KSPField]
        public string unlockPrimaryDoorName = "Unlock Primary Bays";
        [KSPField]
		public string secondaryStartEventGUIName = "Deploy Secondary Bays";
		[KSPField]
		public string secondaryEndEventGUIName = "Retract Secondary Bays";
		[KSPField]
		public string secondaryToggleActionName = "Toggle Secondary Bays";
        [KSPField]
        public string lockSecondaryDoorName = "Lock Secondary Bays";
        [KSPField]
        public string unlockSecondaryDoorName = "Unlock Secondary Bays";
        [KSPField]
		public string combinedStartEventGUIName = "Deploy All Bays";
		[KSPField]
		public string combinedEndEventGUIName = "Retract All Bays";
		[KSPField]
		public string combinedToggleActionName = "Toggle All Bays";
        [KSPField(isPersistant = true)]
		public bool primaryDeployed = false;
		[KSPField(isPersistant = true)]
		public bool secondaryDeployed = false;
		[KSPField(isPersistant = true)]
		public bool combinedDeployed = false;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true), UI_FloatRange(minValue = 0, maxValue = 100, stepIncrement = 1)]
        public float primaryDeployLimit = 100;
        [KSPField]
        public string primaryDeployLimitName = "Primary Bay Deploy Limit";
        [KSPField]
        public bool allowPrimaryDeployLimit = true;
        [KSPField]
        public bool primaryDeployLimitInFlight = false;
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true), UI_FloatRange(minValue = 0, maxValue = 100, stepIncrement = 1)]
        public float secondaryDeployLimit = 100;
        [KSPField]
        public string secondaryDeployLimitName = "Secondary Bay Deploy Limit";
        [KSPField]
        public bool allowSecondaryDeployLimit = true;
        [KSPField]
        public bool secondaryDeployLimitInFlight = false;
        [KSPField]
		public float customAnimationSpeed = 1f;
		[KSPField]
		public float scalarAnimationSpeed = 1f;
		[KSPField]
		public bool primaryAvailableInEVA = true;
		[KSPField]
		public bool primaryAvailableInVessel = true;
		[KSPField]
		public bool primaryAvailableInEditor = true;
		[KSPField]
		public bool primaryActionAvailable = true;
		[KSPField]
		public bool secondaryAvailableInEVA = false;
		[KSPField]
		public bool secondaryAvailableInVessel = false;
		[KSPField]
		public bool secondaryAvailableInEditor = false;
		[KSPField]
		public bool secondaryActionAvailable = false;
		[KSPField]
		public bool combinedAvailableInEVA = false;
		[KSPField]
		public bool combinedAvailableInVessel = false;
		[KSPField]
		public bool combinedAvailableInEditor = false;
		[KSPField]
		public bool combinedActionAvailable = false;
        [KSPField]
        public bool allowDoorLock = true;
        [KSPField(isPersistant = true)]
        public bool lockPrimaryDoors = false;
        [KSPField(isPersistant = true)]
        public bool lockSecondaryDoors = false;
        [KSPField]
        public bool jettisonAvailable = false;
        [KSPField]
        public bool jettisonDeployedOnly = false;
        [KSPField]
        public bool jettisonStowedOnly = false;
        [KSPField]
        public string jettisonIndices = String.Empty;
        [KSPField]
        public int dragModuleIndex = -1;
        [KSPField]
		public float EVArange = 5f;
		[KSPField]
		public bool oneShot = false;
        [KSPField]
        public string SwitchID = string.Empty;
        [KSPField]
        public string AnimationControlState = string.Empty;
        [KSPField]
        public bool ToggleDoorRadiators = false;
        [KSPField]
        public bool UseDoorObstructions = false;
        [KSPField]
        public string DoorObstructionTrigger = "DoorTrigger";
        [KSPField]
        public string PrimaryDoorObstructionSource = "RaySource";
        [KSPField]
        public string SecondaryDoorObstructionSource = "SecondaryRaySource";
        [KSPField]
        public string PrimaryDoorObstructionLength = string.Empty;
        [KSPField]
        public string SecondaryDoorObstructionLength = string.Empty;
        [KSPField]
        public bool ObstructionDebugLines = false;
        [KSPField]
        public bool SolarPanelToggle = false;
        [KSPField(isPersistant = true)]
        public int CurrentSelection = -1;
        //[KSPField(isPersistant = true)]
		public ModuleAnimateGeneric.animationStates primaryAnimationState;
		//[KSPField(isPersistant = true)]
		public ModuleAnimateGeneric.animationStates secondaryAnimationState;
        [KSPField]
        public bool DebugMode = false;

        private string _id = "USAnimator";
		private float _animSpeed = 1;
		private float _primaryAnimTime;
		private float _secondaryAnimTime;

		private BaseEvent tglEventPrimary;
		private BaseAction tglActionPrimary;

		private BaseEvent tglEventSecondary;
		private BaseAction tglActionSecondary;

		private BaseEvent tglEventCombined;
		private BaseAction tglActionCombined;

        private BaseEvent lockEventPrimary;
        private BaseEvent lockEventSecondary;

        private BaseEvent jettEvent;
        private BaseAction jettAction;

        private BaseField primaryLimit;
        private BaseField secondaryLimit;

        private bool _primaryDeployReached;
        private bool _secondaryDeployReached;

        private Animation[] _animsPrimary;
		private Animation[] _animsSecondary;
        
		private EventData<float> onStop;
		private EventData<float, float> onMove;
        
		private USdebugMessages debug;

        private Shader _DebugLineShader;

        private USJettisonSwitch[] jettisonModules;
        private USDragSwitch dragModule;
        private USRadiatorSwitch radiatorModule;
        private USSolarSwitch solarModule;
        
        private int[] _SwitchIndices;

        private int[] _ControlStates;
        
        private double[] _PrimaryObstructionLengths;
        private double[] _SecondaryObstructionLengths;

        private float _primaryObstructionLength;
        private float _secondaryObstructionLength;

        private Transform[] _PrimaryObstructionSources;
        private Transform[] _SecondaryObstructionSources;
        
        private EventData<int, int, Part> onUSSwitch;

        private string _localizedPrimaryStartString = "Deploy Primary Bays";
        private string _localizedPrimaryEndString = "Retract Primary Bays";
        private string _localizedPrimaryActionString = "Toggle Primary Bays";
        private string _localizedPrimaryLockString = "Lock Primary Bays";
        private string _localizedPrimaryUnlockString = "Unlock Primary Bays";
        private string _localizedPrimaryLimit = "Primary Bay Deploy Limit";
        private string _localizedSecondaryStartString = "Deploy Secondary Bays";
        private string _localizedSecondaryEndString = "Retract Secondary Bays";
        private string _localizedSecondaryActionString = "Toggle Secondary Bays";
        private string _localizedSecondaryLockString = "Lock Secondary Bays";
        private string _localizedSecondaryUnlockString = "Unlock Secondary Bays";
        private string _localizedSecondaryLimit = "Secondary Bay Deploy Limit";
        private string _localizedCombinedStartString = "Deploy All Bays";
        private string _localizedCombinedEndString = "Retract All Bays";
        private string _localizedCombinedToggleString = "Toggle All Bays";
        
        private string _localizedStatusString = "Status";

        private string _localizedMoving = "Moving...";
        private string _localizedLocked = "Locked";
        private string _localizedFixed = "Fixed";
        private string _localizedClamp = "Clamped";

        public override void OnAwake()
		{
			base.OnAwake();

            _localizedPrimaryStartString = Localizer.Format(primaryStartEventGUIName);
            _localizedPrimaryEndString = Localizer.Format(primaryEndEventGUIName);
            _localizedPrimaryActionString = Localizer.Format(primaryToggleActionName);
            _localizedPrimaryLockString = Localizer.Format(lockPrimaryDoorName);
            _localizedPrimaryUnlockString = Localizer.Format(unlockPrimaryDoorName);
            _localizedPrimaryLimit = Localizer.Format(primaryDeployLimitName);
            _localizedSecondaryStartString = Localizer.Format(secondaryStartEventGUIName);
            _localizedSecondaryEndString = Localizer.Format(secondaryEndEventGUIName);
            _localizedSecondaryActionString = Localizer.Format(secondaryToggleActionName);
            _localizedSecondaryLockString = Localizer.Format(lockSecondaryDoorName);
            _localizedSecondaryUnlockString = Localizer.Format(unlockSecondaryDoorName);
            _localizedSecondaryLimit = Localizer.Format(secondaryDeployLimitName);
            _localizedCombinedStartString = Localizer.Format(combinedStartEventGUIName);
            _localizedCombinedEndString = Localizer.Format(combinedEndEventGUIName);
            _localizedCombinedToggleString = Localizer.Format(combinedToggleActionName);

            _localizedStatusString = Localizer.Format(statusTitle);

            _localizedMoving = Localizer.Format("#autoLOC_215506");
            _localizedFixed = Localizer.Format("#autoLOC_215714");
            _localizedLocked = Localizer.Format("#autoLOC_215719");
            _localizedClamp = Localizer.Format("#autoLOC_215760");

            tglEventPrimary = Events["toggleEventPrimary"];
			tglActionPrimary = Actions["toggleActionPrimary"];
			tglEventPrimary.active = false;
			tglActionPrimary.active = false;

			tglEventSecondary = Events["toggleEventSecondary"];
			tglActionSecondary = Actions["toggleActionSecondary"];
			tglEventSecondary.active = false;
			tglActionSecondary.active = false;

			tglEventCombined = Events["toggleEventCombined"];
			tglActionCombined = Actions["toggleActionCombined"];
			tglEventCombined.active = false;
			tglActionCombined.active = false;

            lockEventPrimary = Events["lockPrimaryDoorsEvent"];
            lockEventPrimary.active = allowDoorLock;

            lockEventSecondary = Events["lockSecondaryDoorsEvent"];
            lockEventSecondary.active = allowDoorLock;
            
            jettEvent = Events["jettisonEvent"];
            jettAction = Actions["jettisonAction"];
            jettEvent.active = false;
            jettAction.active = false;

            primaryLimit = Fields["primaryDeployLimit"];
            primaryLimit.guiName = _localizedPrimaryLimit;

            secondaryLimit = Fields["secondaryDeployLimit"];
            secondaryLimit.guiName = _localizedSecondaryLimit;
            
            if (HighLogic.LoadedSceneIsEditor)
            {
                (primaryLimit.uiControlEditor as UI_FloatRange).onFieldChanged = OnPrimaryDeployChange;
                (secondaryLimit.uiControlEditor as UI_FloatRange).onFieldChanged = OnSecondaryDeployChange;

            }

            Fields["status"].guiName = _localizedStatusString;

			onStop = new EventData<float>(string.Format("{0}_{1}_onStop", part.partName, part.flightID));
			onMove = new EventData<float, float>(string.Format("{0}_{1}_onMove", part.partName, part.flightID));
        }
        
        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            debug = new USdebugMessages(DebugMode, "USAnimateGeneric");

            _DebugLineShader = Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply");

            if (!String.IsNullOrEmpty(SwitchID))
            {
                _SwitchIndices = USTools.parseIntegers(SwitchID).ToArray();

                onUSSwitch = GameEvents.FindEvent<EventData<int, int, Part>>("onUSSwitch");

                if (onUSSwitch != null)
                    onUSSwitch.Add(onSwitch);
            }

            if (!String.IsNullOrEmpty(AnimationControlState))
                _ControlStates = USTools.parseIntegers(AnimationControlState).ToArray();
            
            if (jettisonAvailable && !string.IsNullOrEmpty(jettisonIndices))
            {
                AssignJettisonModules();

                jettEvent.active = true;
                jettAction.active = true;

                jettEvent.guiActive = !jettisonDeployedOnly && !jettisonStowedOnly;
            }

            if (SolarPanelToggle)
                solarModule = part.FindModuleImplementing<USSolarSwitch>();

            _animSpeed = customAnimationSpeed;
        }

        public override void OnStartFinished(StartState state)
        {
            base.OnStartFinished(state);

            if (!string.IsNullOrEmpty(primaryAnimationName))
                _animsPrimary = part.FindModelAnimators(primaryAnimationName);

            if (!string.IsNullOrEmpty(secondaryAnimationName))
                _animsSecondary = part.FindModelAnimators(secondaryAnimationName);

            if (_animsPrimary != null && DebugMode)
                debug.debugMessage("[US_Anim] Primary Animations found: " + _animsPrimary.Length);

            if (_animsSecondary != null && DebugMode)
                debug.debugMessage("[US_Anim] Secondary Animations found: " + _animsPrimary.Length);
            
            if (_animsPrimary != null && _animsPrimary.Length > 0)
            {
                tglEventPrimary.guiActiveUnfocused = primaryAvailableInEVA;
                tglEventPrimary.guiActive = primaryAvailableInVessel;
                tglEventPrimary.guiActiveEditor = primaryAvailableInEditor;
                tglEventPrimary.unfocusedRange = EVArange;
                tglEventPrimary.active = !(allowDoorLock && lockPrimaryDoors);
                tglActionPrimary.active = primaryActionAvailable && !(allowDoorLock && lockPrimaryDoors);

                lockEventPrimary.guiName = lockPrimaryDoors ? _localizedPrimaryUnlockString : _localizedPrimaryLockString;

                if (primaryDeployed)
                {
                    tglActionPrimary.guiName = _localizedPrimaryEndString;
                    tglEventPrimary.guiName = _localizedPrimaryEndString;
                    _primaryAnimTime = primaryDeployLimit;

                    if (oneShot)
                        primaryAnimationState = ModuleAnimateGeneric.animationStates.FIXED;
                }
                else
                {
                    tglActionPrimary.guiName = _localizedPrimaryStartString;
                    tglEventPrimary.guiName = _localizedPrimaryStartString;
                    _primaryAnimTime = 0;
                }

                if (!string.IsNullOrEmpty(primaryToggleActionName))
                    tglActionPrimary.guiName = _localizedPrimaryActionString;

                for (int i = _animsPrimary.Length - 1; i >= 0; i--)
                {
                    Animation anim = _animsPrimary[i];

                    if (anim == null)
                        continue;

                    if (DebugMode)
                        debug.debugMessage(string.Format("Primary Animation Clips: {0} - Name: {1}", anim.GetClipCount(), anim.clip.name));

                    anim.playAutomatically = false;
                    anim.cullingType = AnimationCullingType.BasedOnRenderers;
                    anim[primaryAnimationName].wrapMode = WrapMode.Once;

                    if (!anim.gameObject.activeInHierarchy)
                        continue;
                    
                    anim[primaryAnimationName].normalizedTime = _primaryAnimTime;
                    anim[primaryAnimationName].enabled = true;
                    anim[primaryAnimationName].speed = 0;
                    anim.Blend(primaryAnimationName);
                }

                if (dragModuleIndex >= 0)
                {
                    if (part.Modules.Count > dragModuleIndex)
                    {
                        if (part.Modules[dragModuleIndex] is USDragSwitch)
                            dragModule = part.Modules[dragModuleIndex] as USDragSwitch;
                    }

                    SetDragCubes(1 - _primaryAnimTime);
                }

                if (ToggleDoorRadiators)
                {
                    radiatorModule = part.FindModuleImplementing<USRadiatorSwitch>();

                    if (primaryDeployed && radiatorModule != null)
                        radiatorModule.Enable();
                }
            }
            else
            {
                tglEventPrimary.guiActive = false;
                tglEventPrimary.guiActiveEditor = false;
                tglEventPrimary.guiActiveUnfocused = false;
                tglEventPrimary.active = false;
                tglActionPrimary.guiName = "Toggle Secondary (Disabled)";
                tglActionPrimary.active = false;
                lockEventPrimary.active = false;
            }

            if (_animsSecondary != null && _animsSecondary.Length > 0)
            {
                tglEventSecondary.guiActiveUnfocused = secondaryAvailableInEVA;
                tglEventSecondary.guiActive = secondaryAvailableInVessel;
                tglEventSecondary.guiActiveEditor = secondaryAvailableInEditor;
                tglEventSecondary.unfocusedRange = EVArange;
                tglEventSecondary.active = !(allowDoorLock && lockSecondaryDoors);
                tglActionSecondary.active = secondaryActionAvailable && !(allowDoorLock && lockSecondaryDoors);

                lockEventSecondary.guiName = lockSecondaryDoors ? _localizedSecondaryUnlockString : _localizedSecondaryLockString;

                if (secondaryDeployed)
                {
                    tglActionSecondary.guiName = _localizedSecondaryEndString;
                    tglEventSecondary.guiName = _localizedSecondaryEndString;
                    _secondaryAnimTime = secondaryDeployLimit;

                    if (oneShot)
                        secondaryAnimationState = ModuleAnimateGeneric.animationStates.FIXED;
                }
                else
                {
                    tglActionSecondary.guiName = _localizedSecondaryStartString;
                    tglEventSecondary.guiName = _localizedSecondaryStartString;
                    _secondaryAnimTime = 0;
                }

                if (!string.IsNullOrEmpty(secondaryToggleActionName))
                    tglActionSecondary.guiName = _localizedSecondaryActionString;

                for (int i = _animsSecondary.Length - 1; i >= 0; i--)
                {
                    Animation anim = _animsSecondary[i];

                    if (anim == null)
                        continue;

                    if (DebugMode)
                        debug.debugMessage(string.Format("Secondary Animation Clips: {0} - Name: {1}", anim.GetClipCount(), anim.clip.name));

                    anim.playAutomatically = false;
                    anim.cullingType = AnimationCullingType.BasedOnRenderers;
                    anim[secondaryAnimationName].wrapMode = WrapMode.Once;

                    if (!anim.gameObject.activeInHierarchy)
                        continue;

                    anim[secondaryAnimationName].normalizedTime = _secondaryAnimTime;
                    anim[secondaryAnimationName].enabled = true;
                    anim[secondaryAnimationName].speed = 0;
                    anim.Blend(secondaryAnimationName);
                }
            }
            else
            {
                tglEventSecondary.guiActive = false;
                tglEventSecondary.guiActiveEditor = false;
                tglEventSecondary.guiActiveUnfocused = false;
                tglEventSecondary.active = false;
                tglActionSecondary.guiName = "Toggle Secondary (Disabled)";
                tglActionSecondary.active = false;
                lockEventSecondary.active = false;
            }

            if ((_animsPrimary != null && _animsPrimary.Length > 0) && (_animsSecondary != null && _animsSecondary.Length > 0))
            {
                tglEventCombined.active = !(allowDoorLock && (lockSecondaryDoors || lockPrimaryDoors));
                tglEventCombined.guiActive = combinedAvailableInVessel;
                tglEventCombined.guiActiveEditor = combinedAvailableInEditor;
                tglEventCombined.guiActiveUnfocused = combinedAvailableInEVA;
                tglEventCombined.unfocusedRange = EVArange;
                tglActionCombined.active = combinedActionAvailable && !(allowDoorLock && (lockSecondaryDoors || lockPrimaryDoors));

                if (combinedDeployed || (primaryDeployed && secondaryDeployed))
                {
                    tglActionCombined.guiName = _localizedCombinedEndString;
                    tglEventCombined.guiName = _localizedCombinedEndString;
                }
                else
                {
                    tglActionCombined.guiName = _localizedCombinedStartString;
                    tglEventCombined.guiName = _localizedCombinedStartString;
                }

                if (!string.IsNullOrEmpty(combinedToggleActionName))
                    tglActionCombined.guiName = _localizedCombinedToggleString;
            }
            else
            {
                tglEventCombined.guiActive = false;
                tglEventCombined.guiActiveEditor = false;
                tglEventCombined.guiActiveUnfocused = false;
                tglEventCombined.active = false;
                tglActionCombined.active = false;
                tglActionCombined.guiName = "Toggle All (Disabled)";
            }
            
            primaryLimit.guiActiveEditor = allowPrimaryDeployLimit && !(allowDoorLock && lockPrimaryDoors) && _animsPrimary != null;
            primaryLimit.guiActive = allowSecondaryDeployLimit && !(allowDoorLock && lockPrimaryDoors) && primaryDeployLimitInFlight && _animsPrimary != null;
            
            secondaryLimit.guiActiveEditor = !(allowDoorLock && lockSecondaryDoors) && _animsSecondary != null;
            secondaryLimit.guiActive = !(allowDoorLock && lockSecondaryDoors) && secondaryDeployLimitInFlight && _animsSecondary != null;

            if (CurrentSelection >= 0)
            {
                UpdateEventStates();
            }

            if (!UseDoorObstructions)
                return;

            if (DebugMode)
                debug.debugMessage("Searching For Door Obstructors...");

            if (!String.IsNullOrEmpty(PrimaryDoorObstructionLength))
            {
                _PrimaryObstructionLengths = USTools.parseDoubles(PrimaryDoorObstructionLength).ToArray();

                if (_PrimaryObstructionLengths.Length == 1 || _SwitchIndices == null || _SwitchIndices.Length == 0 || CurrentSelection < 0)
                    _primaryObstructionLength = (float)_PrimaryObstructionLengths[0];
            }

            if (!String.IsNullOrEmpty(SecondaryDoorObstructionLength))
            {
                _SecondaryObstructionLengths = USTools.parseDoubles(SecondaryDoorObstructionLength).ToArray();

                if (_SecondaryObstructionLengths.Length == 1 || _SwitchIndices == null || _SwitchIndices.Length == 0 || CurrentSelection < 0)
                    _secondaryObstructionLength = (float)_SecondaryObstructionLengths[0];
            }

            if (!String.IsNullOrEmpty(PrimaryDoorObstructionSource))
                _PrimaryObstructionSources = part.FindModelTransforms(PrimaryDoorObstructionSource);

            if (!String.IsNullOrEmpty(SecondaryDoorObstructionSource))
                _SecondaryObstructionSources = part.FindModelTransforms(SecondaryDoorObstructionSource);
            
            if (CurrentSelection >= 0)
                UpdateCollisionLength();
        }
        
        private void AssignJettisonModules()
        {
            int[] indices = USTools.parseIntegers(jettisonIndices).ToArray();

            jettisonModules = new USJettisonSwitch[indices.Length];

            for (int i = 0; i < indices.Length; i++)
            {
                int index = indices[i];

                if (part.Modules.Count <= index)
                    continue;

                if (part.Modules[index] is USJettisonSwitch)
                    jettisonModules[i] = part.Modules[index] as USJettisonSwitch;
            }
        }
        
		private void FixedUpdate()
		{
			if (!HighLogic.LoadedSceneIsFlight && !HighLogic.LoadedSceneIsEditor)
				return;

			if (primaryAnimationState == ModuleAnimateGeneric.animationStates.MOVING)
			{
				if (_animsPrimary == null)
				{
					primaryAnimationState = ModuleAnimateGeneric.animationStates.LOCKED;
				}
				else
				{
					for (int i = _animsPrimary.Length - 1; i >= 0; i--)
					{
						Animation anim = _animsPrimary[i];

						if (anim == null)
							continue;

						if (anim.gameObject.activeInHierarchy)
						{
                            if (!anim.IsPlaying(primaryAnimationName))
                            {
                                if (DebugMode)
                                    debug.debugMessage(string.Format("Forcing Animation Stop: {0} - Time: {1:F2}", primaryAnimationName, anim[primaryAnimationName].normalizedTime));

                                if (oneShot)
                                {
                                    primaryAnimationState = ModuleAnimateGeneric.animationStates.FIXED;
                                }
                                else
                                {
                                    primaryAnimationState = ModuleAnimateGeneric.animationStates.LOCKED;
                                }

                                if (primaryDeployed)
                                    _primaryAnimTime = primaryDeployLimit * 0.01f;
                                else
                                    _primaryAnimTime = 0;

                                primaryLimit.guiActiveEditor = allowPrimaryDeployLimit;
                                primaryLimit.guiActive = primaryDeployLimitInFlight;

                                anim[primaryAnimationName].normalizedTime = _primaryAnimTime;
                                SetDragCubes(1 - _primaryAnimTime);
                                anim.Stop(primaryAnimationName);
                                onStop.Fire(_primaryAnimTime);

                                if (ToggleDoorRadiators && radiatorModule != null)
                                {
                                    if (_primaryAnimTime <= 0)
                                        radiatorModule.Disable();
                                    else
                                        radiatorModule.Enable();
                                }

                                if (SolarPanelToggle && solarModule != null)
                                    solarModule.DeploySolarPanels(_primaryAnimTime > 0);
                            }
                            else
                            {
                                //debug.debugMessage(string.Format("Animation {0} playing - Time: {1:F2}", primaryAnimationName, anim[primaryAnimationName].normalizedTime));
                                _primaryAnimTime = anim[primaryAnimationName].normalizedTime;
                                primaryAnimationState = ModuleAnimateGeneric.animationStates.MOVING;
                                SetDragCubes(1 - _primaryAnimTime);

                                if (anim[primaryAnimationName].speed > 0)
                                {
                                    if (_primaryAnimTime >= primaryDeployLimit * 0.01f)
                                    {
                                        _primaryAnimTime = primaryDeployLimit * 0.01f;

                                        for (int j = _animsPrimary.Length - 1; j >= 0; j--)
                                        {
                                            if (j == i)
                                                continue;

                                            Animation otherAnim = _animsPrimary[j];

                                            if (otherAnim == null)
                                                continue;

                                            otherAnim[primaryAnimationName].normalizedTime = _primaryAnimTime;
                                            otherAnim[primaryAnimationName].speed = 0;
                                            otherAnim.Stop(primaryAnimationName);
                                        }

                                        anim[primaryAnimationName].normalizedTime = _primaryAnimTime;
                                        anim[primaryAnimationName].speed = 0;
                                        anim.Stop(primaryAnimationName);
                                    }
                                }
                            }
						}
					}

                    _primaryDeployReached = false;
				}
			}

			if (secondaryAnimationState == ModuleAnimateGeneric.animationStates.MOVING)
			{
				if (_animsSecondary == null)
				{
					secondaryAnimationState = ModuleAnimateGeneric.animationStates.LOCKED;
				}
				else
				{
					for (int i = _animsSecondary.Length - 1; i >= 0; i--)
					{
						Animation anim = _animsSecondary[i];

						if (anim == null)
							continue;

						if (anim.gameObject.activeInHierarchy)
						{
							if (!anim.IsPlaying(secondaryAnimationName))
							{
                                if (DebugMode)
                                    debug.debugMessage(string.Format("Forcing Animation Stop: {0} - Time: {1:F2}", secondaryAnimationName, anim[secondaryAnimationName].normalizedTime));

                                if (oneShot)
								{
									secondaryAnimationState = ModuleAnimateGeneric.animationStates.FIXED;
								}
								else
								{
									secondaryAnimationState = ModuleAnimateGeneric.animationStates.LOCKED;
								}

								if (secondaryDeployed)
									_secondaryAnimTime = secondaryDeployLimit * 0.01f;
								else
									_secondaryAnimTime = 0;

                                secondaryLimit.guiActiveEditor = allowSecondaryDeployLimit;
                                secondaryLimit.guiActive = secondaryDeployLimitInFlight;

                                anim[secondaryAnimationName].normalizedTime = _secondaryAnimTime;
								anim.Stop(secondaryAnimationName);
								onStop.Fire(_secondaryAnimTime);
                            }
							else
							{
								//debug.debugMessage(string.Format("Animation {0} playing - Time: {1:F2}", secondaryAnimationName, anim[secondaryAnimationName].normalizedTime));
								_secondaryAnimTime = anim[secondaryAnimationName].normalizedTime;
								secondaryAnimationState = ModuleAnimateGeneric.animationStates.MOVING;

                                if (anim[secondaryAnimationName].speed > 0)
                                {
                                    if (_secondaryAnimTime >= secondaryDeployLimit * 0.01f)
                                    {
                                        _secondaryAnimTime = secondaryDeployLimit * 0.01f;

                                        for (int j = _animsSecondary.Length - 1; j >= 0; j--)
                                        {
                                            if (j == i)
                                                continue;

                                            Animation otherAnim = _animsSecondary[j];

                                            if (otherAnim == null)
                                                continue;

                                            otherAnim[secondaryAnimationName].normalizedTime = _secondaryAnimTime;
                                            otherAnim[secondaryAnimationName].speed = 0;
                                            otherAnim.Stop(secondaryAnimationName);
                                        }

                                        anim[secondaryAnimationName].normalizedTime = _secondaryAnimTime;
                                        anim[secondaryAnimationName].speed = 0;
                                        anim.Stop(secondaryAnimationName);
                                    }
                                }
                            }
						}
					}
				}
			}

            if (primaryAnimationState == ModuleAnimateGeneric.animationStates.CLAMPED || secondaryAnimationState == ModuleAnimateGeneric.animationStates.CLAMPED)
            {
                status = _localizedClamp;
            }
            else
            {
                if (primaryAnimationState == ModuleAnimateGeneric.animationStates.MOVING || secondaryAnimationState == ModuleAnimateGeneric.animationStates.MOVING)
                {
                    status = _localizedMoving;
                }
                else if (primaryAnimationState == ModuleAnimateGeneric.animationStates.LOCKED || secondaryAnimationState == ModuleAnimateGeneric.animationStates.LOCKED)
                {
                    status = _localizedLocked;
                }
                else if (primaryAnimationState == ModuleAnimateGeneric.animationStates.FIXED || secondaryAnimationState == ModuleAnimateGeneric.animationStates.FIXED)
                {
                    status = _localizedFixed;
                }
            }
		}

        private void OnPrimaryDeployChange(BaseField field, object obj)
        {
            if (HighLogic.LoadedSceneIsFlight)
                return;

            float deploy = primaryDeployLimit * 0.01f;

            if (_animsPrimary == null)
            {
                primaryAnimationState = ModuleAnimateGeneric.animationStates.LOCKED;
            }
            else
            {
                for (int i = _animsPrimary.Length - 1; i >= 0; i--)
                {
                    Animation anim = _animsPrimary[i];

                    if (anim == null)
                        continue;

                    anim[primaryAnimationName].normalizedTime = deploy;
                    _primaryAnimTime = deploy;
                    anim[primaryAnimationName].speed = 0;
                    anim.Blend(primaryAnimationName);
                }

                primaryAnimationState = ModuleAnimateGeneric.animationStates.CLAMPED;

                SetDragCubes(1 - _primaryAnimTime);

                if (primaryDeployLimit == 0)
                {
                    primaryAnimationState = ModuleAnimateGeneric.animationStates.LOCKED;
                    status = _localizedLocked;

                    primaryDeployed = false;

                    tglActionPrimary.guiName = _localizedPrimaryStartString;
                    tglEventPrimary.guiName = _localizedPrimaryStartString;

                    if (!string.IsNullOrEmpty(primaryToggleActionName))
                        tglActionPrimary.guiName = _localizedPrimaryActionString;

                    if (!secondaryDeployed)
                    {
                        combinedDeployed = false;

                        tglActionCombined.guiName = _localizedCombinedStartString;
                        tglEventCombined.guiName = _localizedCombinedStartString;

                        if (!string.IsNullOrEmpty(combinedToggleActionName))
                            tglActionCombined.guiName = _localizedCombinedToggleString;
                    }
                }
                else
                {
                    primaryDeployed = true;

                    tglActionPrimary.guiName = _localizedPrimaryEndString;
                    tglEventPrimary.guiName = _localizedPrimaryEndString;

                    if (!string.IsNullOrEmpty(primaryToggleActionName))
                        tglActionPrimary.guiName = _localizedPrimaryActionString;

                    if (secondaryDeployed)
                    {
                        combinedDeployed = true;

                        tglActionCombined.guiName = _localizedCombinedEndString;
                        tglEventCombined.guiName = _localizedCombinedEndString;

                        if (!string.IsNullOrEmpty(combinedToggleActionName))
                            tglActionCombined.guiName = _localizedCombinedToggleString;
                    }
                }
            }
        }

        private void OnSecondaryDeployChange(BaseField field, object obj)
        {
            if (HighLogic.LoadedSceneIsFlight)
                return;

            float deploy = secondaryDeployLimit * 0.01f;

            if (_animsSecondary == null)
            {
                secondaryAnimationState = ModuleAnimateGeneric.animationStates.LOCKED;
            }
            else
            {
                for (int i = _animsSecondary.Length - 1; i >= 0; i--)
                {
                    Animation anim = _animsSecondary[i];

                    if (anim == null)
                        continue;

                    anim[secondaryAnimationName].normalizedTime = deploy;
                    _secondaryAnimTime = deploy;
                    anim[secondaryAnimationName].speed = 0;
                    anim.Blend(secondaryAnimationName);
                }

                secondaryAnimationState = ModuleAnimateGeneric.animationStates.CLAMPED;

                SetDragCubes(1 - _secondaryAnimTime);

                if (primaryDeployLimit == 0)
                {
                    secondaryAnimationState = ModuleAnimateGeneric.animationStates.LOCKED;
                    status = _localizedLocked;

                    secondaryDeployed = false;

                    tglActionSecondary.guiName = _localizedSecondaryStartString;
                    tglEventSecondary.guiName = _localizedSecondaryStartString;

                    if (!string.IsNullOrEmpty(secondaryToggleActionName))
                        tglActionSecondary.guiName = _localizedSecondaryActionString;

                    if (!primaryDeployed)
                    {
                        combinedDeployed = false;

                        tglActionCombined.guiName = _localizedCombinedStartString;
                        tglEventCombined.guiName = _localizedCombinedStartString;

                        if (!string.IsNullOrEmpty(combinedToggleActionName))
                            tglActionCombined.guiName = _localizedCombinedToggleString;
                    }
                }
                else
                {
                    secondaryDeployed = true;

                    tglActionSecondary.guiName = _localizedSecondaryEndString;
                    tglEventSecondary.guiName = _localizedSecondaryEndString;

                    if (!string.IsNullOrEmpty(secondaryToggleActionName))
                        tglActionSecondary.guiName = _localizedPrimaryActionString;

                    if (primaryDeployed)
                    {
                        combinedDeployed = true;

                        tglActionCombined.guiName = _localizedCombinedEndString;
                        tglEventCombined.guiName = _localizedCombinedEndString;

                        if (!string.IsNullOrEmpty(combinedToggleActionName))
                            tglActionCombined.guiName = _localizedCombinedToggleString;
                    }
                }
            }
        }

        private void onSwitch(int index, int selection, Part p)
        {
            if (p != part)
                return;

            if (_SwitchIndices == null || _SwitchIndices.Length <= 0)
            {
                if (String.IsNullOrEmpty(SwitchID))
                    return;

                _SwitchIndices = USTools.parseIntegers(SwitchID).ToArray();
            }

            for (int i = _SwitchIndices.Length - 1; i >= 0; i--)
            {
                if (_SwitchIndices[i] == index)
                {
                    if (DebugMode)
                        debug.debugMessage(string.Format("On Switch - Index: {0}", selection));

                    CurrentSelection = selection;

                    UpdateEventStates();

                    UpdateCollisionLength();
                    
                    break;
                }
            }
        }

        private void UpdateEventStates()
        {
            if (_ControlStates == null || _ControlStates.Length <= CurrentSelection)
                return;

            switch(_ControlStates[CurrentSelection])
            {
                case 0:
                    tglEventPrimary.active = false;
                    tglActionPrimary.active = false;
                    tglEventSecondary.active = false;
                    tglActionSecondary.active = false;
                    tglEventCombined.active = false;
                    tglActionCombined.active = false;
                    lockEventPrimary.active = false;
                    lockEventSecondary.active = false;
                    break;
                case 1:
                    tglEventPrimary.active = !(allowDoorLock && lockPrimaryDoors) && (primaryAvailableInVessel || primaryAvailableInEVA || primaryAvailableInEditor);
                    tglActionPrimary.active = !(allowDoorLock && lockPrimaryDoors) && primaryActionAvailable;
                    tglEventSecondary.active = false;
                    tglActionSecondary.active = false;
                    tglEventCombined.active = false;
                    tglActionCombined.active = false;
                    lockEventPrimary.active = allowDoorLock;
                    lockEventSecondary.active = false;
                    break;
                case 2:
                    tglEventPrimary.active = false;
                    tglActionPrimary.active = false;
                    tglEventSecondary.active = !(allowDoorLock && lockSecondaryDoors) && (secondaryAvailableInVessel || secondaryAvailableInEVA|| secondaryAvailableInEditor);
                    tglActionSecondary.active = !(allowDoorLock && lockSecondaryDoors) && secondaryActionAvailable;
                    tglEventCombined.active = false;
                    tglActionCombined.active = false;
                    lockEventPrimary.active = false;
                    lockEventSecondary.active = allowDoorLock;
                    break;
                case 3:
                    tglEventPrimary.active = !(allowDoorLock && lockPrimaryDoors) && (primaryAvailableInVessel || primaryAvailableInEVA || primaryAvailableInEditor);
                    tglActionPrimary.active = !(allowDoorLock && lockPrimaryDoors) && primaryActionAvailable;
                    tglEventSecondary.active = !(allowDoorLock && lockSecondaryDoors) && (secondaryAvailableInVessel || secondaryAvailableInEVA || secondaryAvailableInEditor);
                    tglActionSecondary.active = !(allowDoorLock && lockSecondaryDoors) && secondaryActionAvailable;
                    tglEventCombined.active = !(allowDoorLock && (lockPrimaryDoors || lockSecondaryDoors)) && (combinedAvailableInVessel || combinedAvailableInEVA || combinedAvailableInEditor);
                    tglActionCombined.active = !(allowDoorLock && (lockPrimaryDoors || lockSecondaryDoors)) && combinedActionAvailable;
                    lockEventPrimary.active = allowDoorLock;
                    lockEventSecondary.active = allowDoorLock;
                    break;
            }
        }

        private void UpdateCollisionLength()
        {
            if (!UseDoorObstructions)
                return;

            if (_PrimaryObstructionLengths != null && _PrimaryObstructionLengths.Length > CurrentSelection)
                _primaryObstructionLength = (float)_PrimaryObstructionLengths[CurrentSelection];

            if (_SecondaryObstructionLengths != null && _SecondaryObstructionLengths.Length > CurrentSelection)
                _secondaryObstructionLength = (float)_SecondaryObstructionLengths[CurrentSelection];
            
            if (_PrimaryObstructionSources != null && ObstructionDebugLines)
                DrawCollisionLines(_PrimaryObstructionSources, _primaryObstructionLength, Color.blue);

            if (_SecondaryObstructionSources != null && ObstructionDebugLines)
                DrawCollisionLines(_SecondaryObstructionSources, _secondaryObstructionLength, Color.red);
        }
        
        [KSPAction("Jettison Doors")]
        public void jettisonAction(KSPActionParam param)
        {
            jettisonEvent();
        }

        [KSPEvent(name = "jettisonEvent", guiName = "Jettison Doors", guiActive = false, guiActiveUnfocused = false, unfocusedRange = 5f, guiActiveEditor = false)]
        public void jettisonEvent()
        {
            for (int i = jettisonModules.Length - 1; i >= 0; i--)
            {
                jettisonModules[i].OnJettison();
            }

            jettEvent.active = false;
            jettAction.active = false;
        }

        [KSPEvent(name = "lockPrimaryDoorsEvent", guiName = "Lock Primary Bays", guiActive = false, guiActiveUnfocused = false, guiActiveEditor = true, active = false)]
        public void lockPrimaryDoorsEvent()
        {
            if (!allowDoorLock)
                return;

            lockPrimaryDoors = !lockPrimaryDoors;

            primaryLimit.guiActiveEditor = !lockPrimaryDoors;

            lockEventPrimary.guiName = lockPrimaryDoors ? _localizedPrimaryUnlockString : _localizedPrimaryLockString;

            tglEventPrimary.active = !lockPrimaryDoors && (primaryAvailableInVessel || primaryAvailableInEVA || primaryAvailableInEditor);

            tglActionPrimary.active = !lockPrimaryDoors && primaryActionAvailable;

            tglEventCombined.active = !lockPrimaryDoors && !lockSecondaryDoors && (combinedAvailableInVessel || combinedAvailableInEVA || combinedAvailableInEditor);

            tglActionCombined.active = !lockPrimaryDoors && !lockSecondaryDoors && combinedActionAvailable;
        }

        [KSPEvent(name = "lockSecondaryDoorsEvent", guiName = "Lock Secondary Bays", guiActive = false, guiActiveUnfocused = false, guiActiveEditor = true, active = false)]
        public void lockSecondaryDoorsEvent()
        {
            if (!allowDoorLock)
                return;

            lockSecondaryDoors = !lockSecondaryDoors;

            secondaryLimit.guiActiveEditor = !lockSecondaryDoors;

            lockEventSecondary.guiName = lockSecondaryDoors ? _localizedSecondaryUnlockString : _localizedSecondaryLockString;

            tglEventSecondary.active = !lockSecondaryDoors && (secondaryAvailableInVessel || secondaryAvailableInEVA || secondaryAvailableInEditor);

            tglActionSecondary.active = !lockSecondaryDoors && secondaryActionAvailable;

            tglEventCombined.active = !lockPrimaryDoors && !lockSecondaryDoors && (combinedAvailableInVessel || combinedAvailableInEVA || combinedAvailableInEditor);

            tglActionCombined.active = !lockPrimaryDoors && !lockSecondaryDoors && combinedActionAvailable;
        }

        [KSPAction("Toggle All Bays")]
		public void toggleActionCombined(KSPActionParam param)
		{
			if (combinedAvailableInVessel)
				toggleEventCombined();
		}

		[KSPEvent(name = "toggleEventCombined", guiName = "Deploy All Bays", guiActive = false, guiActiveUnfocused = false, unfocusedRange = 5f, guiActiveEditor = false)]
		public void toggleEventCombined()
		{
			combinedDeployed = !combinedDeployed;

			if (combinedDeployed)
			{
				if (!primaryDeployed)
				{
					primaryDeployed = true;
					tglActionPrimary.guiName = _localizedPrimaryEndString;
					tglEventPrimary.guiName = _localizedPrimaryEndString;

                    if (!string.IsNullOrEmpty(primaryToggleActionName))
						tglActionPrimary.guiName = _localizedPrimaryActionString;

                    primaryLimit.guiActiveEditor = false;
                    primaryLimit.guiActive = false;

                    if (_animsPrimary != null)
					{
						for (int i = _animsPrimary.Length - 1; i >= 0; i--)
							animate(_animsPrimary[i], primaryAnimationName, _animSpeed, 0);
					}

					primaryAnimationState = ModuleAnimateGeneric.animationStates.MOVING;
				}

				if (!secondaryDeployed)
				{
					secondaryDeployed = true;
					tglActionSecondary.guiName = _localizedSecondaryEndString;
					tglEventSecondary.guiName = _localizedSecondaryEndString;

					if (!string.IsNullOrEmpty(secondaryToggleActionName))
						tglActionSecondary.guiName = _localizedSecondaryActionString;

                    secondaryLimit.guiActiveEditor = false;
                    secondaryLimit.guiActive = false;

                    if (_animsSecondary != null)
					{
						for (int i = _animsSecondary.Length - 1; i >= 0; i--)
							animate(_animsSecondary[i], secondaryAnimationName, _animSpeed, 0);
					}

					secondaryAnimationState = ModuleAnimateGeneric.animationStates.MOVING;
				}

				onMove.Fire(0, 1);

				tglActionCombined.guiName = _localizedCombinedEndString;
				tglEventCombined.guiName = _localizedCombinedEndString;

				if (!string.IsNullOrEmpty(combinedToggleActionName))
					tglActionCombined.guiName = _localizedCombinedToggleString;
			}
			else
			{
				if (primaryDeployed && !oneShot || primaryAnimationState == ModuleAnimateGeneric.animationStates.FIXED)
                {
                    bool obstructed = false;

                    if (UseDoorObstructions)
                    {
                        if (PrimaryCollisionCheck())
                        {
                            primaryAnimationState = ModuleAnimateGeneric.animationStates.LOCKED;
                            obstructed = true;
                        }
                    }

                    if (!obstructed)
                    {
                        primaryDeployed = false;
                        tglActionPrimary.guiName = _localizedPrimaryStartString;
                        tglEventPrimary.guiName = _localizedPrimaryStartString;

                        if (!string.IsNullOrEmpty(primaryToggleActionName))
                            tglActionPrimary.guiName = _localizedPrimaryActionString;

                        primaryLimit.guiActiveEditor = false;
                        primaryLimit.guiActive = false;

                        if (_animsPrimary != null)
                        {
                            for (int i = _animsPrimary.Length - 1; i >= 0; i--)
                                animate(_animsPrimary[i], primaryAnimationName, _animSpeed * -1f, primaryDeployLimit * 0.01f);
                        }

                        primaryAnimationState = ModuleAnimateGeneric.animationStates.MOVING;
                    }
				}

				if (secondaryDeployed && !oneShot || secondaryAnimationState == ModuleAnimateGeneric.animationStates.FIXED)
                {
                    bool obstructed = false;

                    if (UseDoorObstructions)
                    {
                        if (SecondaryCollisionCheck())
                        {
                            secondaryAnimationState = ModuleAnimateGeneric.animationStates.LOCKED;
                            obstructed = true;
                        }
                    }

                    if (!obstructed)
                    {
                        secondaryDeployed = false;
                        tglActionSecondary.guiName = _localizedSecondaryStartString;
                        tglEventSecondary.guiName = _localizedSecondaryStartString;

                        if (!string.IsNullOrEmpty(secondaryToggleActionName))
                            tglActionSecondary.guiName = _localizedSecondaryActionString;

                        secondaryLimit.guiActiveEditor = false;
                        secondaryLimit.guiActive = false;

                        if (_animsSecondary != null)
                        {
                            for (int i = _animsSecondary.Length - 1; i >= 0; i--)
                                animate(_animsSecondary[i], secondaryAnimationName, _animSpeed * -1f, secondaryDeployLimit * 0.01f);
                        }

                        secondaryAnimationState = ModuleAnimateGeneric.animationStates.MOVING;
                    }
				}

				onMove.Fire(1, 0);

				tglActionCombined.guiName = _localizedCombinedStartString;
				tglEventCombined.guiName = _localizedCombinedStartString;

				if (!string.IsNullOrEmpty(combinedToggleActionName))
					tglActionCombined.guiName = _localizedCombinedToggleString;
			}
		}

		[KSPAction("Toggle Primary Bays")]
		public void toggleActionPrimary(KSPActionParam param)
		{
			if (primaryAvailableInVessel)
				toggleEventPrimary();
		}

		[KSPEvent(name = "toggleEventPrimary", guiName = "Deploy Primary Bays", guiActive = false, guiActiveUnfocused = false, unfocusedRange = 5f, guiActiveEditor = false)]
		public void toggleEventPrimary()
		{
			if (oneShot && primaryAnimationState == ModuleAnimateGeneric.animationStates.FIXED)
				return;

			if (_animsPrimary == null || _animsPrimary.Length <= 0)
			{
				tglEventPrimary.active = false;
				tglActionPrimary.active = false;
				return;
			}

			primaryAnimationState = ModuleAnimateGeneric.animationStates.MOVING;

			if (primaryDeployed)
            {
                if (UseDoorObstructions)
                {
                    if (PrimaryCollisionCheck())
                    {
                        primaryAnimationState = ModuleAnimateGeneric.animationStates.LOCKED;

                        if (!secondaryDeployed)
                        {
                            combinedDeployed = false;

                            tglActionCombined.guiName = _localizedCombinedStartString;
                            tglEventCombined.guiName = _localizedCombinedStartString;

                            if (!string.IsNullOrEmpty(combinedToggleActionName))
                                tglActionCombined.guiName = _localizedCombinedToggleString;
                        }
                        else
                        {
                            combinedDeployed = true;

                            tglActionCombined.guiName = _localizedCombinedEndString;
                            tglEventCombined.guiName = _localizedCombinedEndString;

                            if (!string.IsNullOrEmpty(combinedToggleActionName))
                                tglActionCombined.guiName = _localizedCombinedToggleString;
                        }

                        return;
                    }
                }

                primaryDeployed = false;
				tglActionPrimary.guiName = _localizedPrimaryStartString;
				tglEventPrimary.guiName = _localizedPrimaryStartString;

				if (!string.IsNullOrEmpty(primaryToggleActionName))
					tglActionPrimary.guiName = _localizedPrimaryActionString;

                primaryLimit.guiActiveEditor = false;
                primaryLimit.guiActive = false;

                if (_animsPrimary != null)
				{
					for (int i = _animsPrimary.Length - 1; i >= 0; i--)
						animate(_animsPrimary[i], primaryAnimationName, _animSpeed * -1f, primaryDeployLimit * 0.01f);

					onMove.Fire(1, 0);
				}

				if (!secondaryDeployed)
				{
					combinedDeployed = false;

					tglActionCombined.guiName = _localizedCombinedStartString;
					tglEventCombined.guiName = _localizedCombinedStartString;

					if (!string.IsNullOrEmpty(combinedToggleActionName))
						tglActionCombined.guiName = _localizedCombinedToggleString;
				}
			}
			else
			{
				primaryDeployed = true;
				tglActionPrimary.guiName = _localizedPrimaryEndString;
				tglEventPrimary.guiName = _localizedPrimaryEndString;

				if (!string.IsNullOrEmpty(primaryToggleActionName))
					tglActionPrimary.guiName = _localizedPrimaryActionString;

                primaryLimit.guiActiveEditor = false;
                primaryLimit.guiActive = false;

                if (_animsPrimary != null)
				{
					for (int i = _animsPrimary.Length - 1; i >= 0; i--)
						animate(_animsPrimary[i], primaryAnimationName, _animSpeed, 0);

					onMove.Fire(0, 1);
				}

				if (secondaryDeployed)
				{
					combinedDeployed = true;

					tglActionCombined.guiName = _localizedCombinedEndString;
					tglEventCombined.guiName = _localizedCombinedEndString;

					if (!string.IsNullOrEmpty(combinedToggleActionName))
						tglActionCombined.guiName = _localizedCombinedToggleString;
				}
			}
		}

		[KSPAction("Toggle Secondary Bays")]
		public void toggleActionSecondary(KSPActionParam param)
		{
			if (secondaryAvailableInVessel)
				toggleEventSecondary();
		}

		[KSPEvent(name = "toggleEventSecondary", guiName = "Deploy Secondary Bays", guiActive = false, guiActiveUnfocused = false, unfocusedRange = 5f, guiActiveEditor = false)]
		public void toggleEventSecondary()
		{
			if (oneShot && secondaryAnimationState == ModuleAnimateGeneric.animationStates.FIXED)
				return;

			if (_animsSecondary == null || _animsSecondary.Length <= 0)
			{
				tglEventSecondary.active = false;
				tglActionSecondary.active = false;
				return;
			}

			secondaryAnimationState = ModuleAnimateGeneric.animationStates.MOVING;

			if (secondaryDeployed)
			{
                if (UseDoorObstructions)
                {
                    if (SecondaryCollisionCheck())
                    {
                        secondaryAnimationState = ModuleAnimateGeneric.animationStates.LOCKED;
                        
                        if (!primaryDeployed)
                        {
                            combinedDeployed = false;

                            tglActionCombined.guiName = _localizedCombinedStartString;
                            tglEventCombined.guiName = _localizedCombinedStartString;

                            if (!string.IsNullOrEmpty(combinedToggleActionName))
                                tglActionCombined.guiName = _localizedCombinedToggleString;
                        }
                        else
                        {
                            combinedDeployed = true;

                            tglActionCombined.guiName = _localizedCombinedEndString;
                            tglEventCombined.guiName = _localizedCombinedEndString;

                            if (!string.IsNullOrEmpty(combinedToggleActionName))
                                tglActionCombined.guiName = _localizedCombinedToggleString;
                        }

                        return;
                    }
                }

				secondaryDeployed = false;
				tglActionSecondary.guiName = _localizedSecondaryStartString;
				tglEventSecondary.guiName = _localizedSecondaryStartString;

				if (!string.IsNullOrEmpty(secondaryToggleActionName))
					tglActionSecondary.guiName = _localizedSecondaryActionString;

                secondaryLimit.guiActiveEditor = false;
                secondaryLimit.guiActive = false;

                if (_animsSecondary != null)
				{
					for (int i = _animsSecondary.Length - 1; i >= 0; i--)
						animate(_animsSecondary[i], secondaryAnimationName, _animSpeed * -1f, secondaryDeployLimit * 0.01f);

					onMove.Fire(1, 0);
				}

				if (!primaryDeployed)
				{
					combinedDeployed = false;

					tglActionCombined.guiName = _localizedCombinedStartString;
					tglEventCombined.guiName = _localizedCombinedStartString;

					if (!string.IsNullOrEmpty(combinedToggleActionName))
						tglActionCombined.guiName = _localizedCombinedToggleString;
				}
			}
			else
			{
				secondaryDeployed = true;
				tglActionSecondary.guiName = _localizedSecondaryEndString;
				tglEventSecondary.guiName = _localizedSecondaryEndString;

				if (!string.IsNullOrEmpty(secondaryToggleActionName))
					tglActionSecondary.guiName = _localizedSecondaryActionString;

                secondaryLimit.guiActiveEditor = false;
                secondaryLimit.guiActive = false;

                if (_animsSecondary != null)
				{
					for (int i = _animsSecondary.Length - 1; i >= 0; i--)
						animate(_animsSecondary[i], secondaryAnimationName, _animSpeed, 0);

					onMove.Fire(0, 1);
				}

				if (primaryDeployed)
				{
					combinedDeployed = true;

					tglActionCombined.guiName = _localizedCombinedEndString;
					tglEventCombined.guiName = _localizedCombinedEndString;

					if (!string.IsNullOrEmpty(combinedToggleActionName))
						tglActionCombined.guiName = _localizedCombinedToggleString;
				}
			}
		}

        private bool PrimaryCollisionCheck()
        {
            if (_PrimaryObstructionSources == null)
                return false;

            if (ObstructionDebugLines)
                DrawCollisionLines(_PrimaryObstructionSources, _primaryObstructionLength, Color.blue);

            if (DebugMode)
                debug.debugMessage("Testing for primary door collisions before closing...");

            for (int i = _PrimaryObstructionSources.Length - 1; i >= 0; i--)
            {
                Transform source = _PrimaryObstructionSources[i];

                Ray r = new Ray(source.position, source.forward);
                RaycastHit hit = new RaycastHit();

                if (Physics.Raycast(r, out hit, _primaryObstructionLength, 1 << 0))
                {
                    if (hit.collider != null)
                    {
                        if (hit.collider.attachedRigidbody != null)
                        {
                            Part p = Part.FromGO(hit.transform.gameObject);

                            if (p != null)
                            {
                                ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_US_PrimaryBayObstruction", p.partInfo.title, part.partInfo.title), 10f, ScreenMessageStyle.UPPER_CENTER);

                                if (DebugMode)
                                    debug.debugMessage(string.Format("Primary door obstruction detected; stopping animation: {0}", p.partName));

                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool SecondaryCollisionCheck()
        {
            if (_SecondaryObstructionSources == null)
                return false;

            if (ObstructionDebugLines)
                DrawCollisionLines(_SecondaryObstructionSources, _secondaryObstructionLength, Color.red);

            if (DebugMode)
                debug.debugMessage("Testing for secondary door collisions before closing...");

            for (int i = _SecondaryObstructionSources.Length - 1; i >= 0; i--)
            {
                Transform source = _SecondaryObstructionSources[i];

                Ray r = new Ray(source.position, source.forward);
                RaycastHit hit = new RaycastHit();

                if (Physics.Raycast(r, out hit, _secondaryObstructionLength, 1 << 0))
                {
                    if (hit.collider != null)
                    {
                        if (hit.collider.attachedRigidbody != null)
                        {
                            Part p = Part.FromGO(hit.transform.gameObject);

                            if (p != null)
                            {
                                ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_US_SecondaryBayObstruction", p.partInfo.title, part.partInfo.title), 10f, ScreenMessageStyle.UPPER_CENTER);

                                if (DebugMode)
                                    debug.debugMessage(string.Format("Secondary door obstruction detected; stopping animation: {0}", p.partName));

                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private void DrawCollisionLines(Transform[] sources, float length, Color c)
        {
            if (sources == null)
                return;

            if (DebugMode)
                debug.debugMessage("drawing door collisions test lines");

            for (int i = sources.Length - 1; i >= 0; i--)
            {
                GameObject line = new GameObject("Debug Line");
                line.transform.position = sources[i].position;
                LineRenderer lr = line.AddComponent<LineRenderer>();
                lr.material = new Material(_DebugLineShader);
                lr.startColor = c;
                lr.endColor = c * 0.3f;
                lr.startWidth = 0.03f;
                lr.endWidth = 0.01f;

                Vector3 end = sources[i].position + (sources[i].forward * length);

                //debug.debugMessage(string.Format("Debug Line: {0} Start: {1:N3} - End: {2:N3}"
                //    , sources[i].name, sources[i].position, end));

                lr.SetPosition(0, sources[i].position);
                lr.SetPosition(1, end);
                Destroy(line, 2f);
            }
        }

		private void animate(Animation anim, string animationName, float speed, float time)
		{
			if (anim == null)
				return;

			if (anim[animationName] == null)
				return;

            if (DebugMode)
                debug.debugMessage(string.Format("Animation Play: Speed {0:F2} - Time: {1:F2}", speed, time));

			anim[animationName].speed = speed;

			if (!anim.IsPlaying(animationName))
			{
				anim[animationName].enabled = true;

                if (DebugMode)
                    debug.debugMessage(string.Format("Starting Animation: {0} - Time: {1:F2}", animationName, time));

                anim[animationName].normalizedTime = time;
				anim.Blend(animationName);
			}
		}
        
        private void SetDragCubes(float a)
        {
            if (dragModule == null)
                return;

            StartCoroutine(dragModule.UpdateDragCubes(a));
        }

		public EventData<float, float> OnMoving
		{
			get { return onMove; }
		}

		public EventData<float> OnStop
		{
			get { return onStop; }
		}

		public bool CanMove
		{
			get 
			{ 
				return (_animsPrimary != null && _animsPrimary.Length > 0) ||
				(_animsSecondary != null && _animsSecondary.Length > 0); 
			}
		}

		public float GetScalar
		{
			get
			{
                //debug.debugMessage(string.Format("Get Scalar called: Primary: {0:N2} - Secondary: {1:N2}", _primaryAnimTime, _secondaryAnimTime));

                if (primaryAnimationState == ModuleAnimateGeneric.animationStates.MOVING)
                    return _primaryAnimTime;

                if (secondaryAnimationState == ModuleAnimateGeneric.animationStates.MOVING)
                    return _secondaryAnimTime;

				if (_primaryAnimTime >= primaryDeployLimit || _secondaryAnimTime >= secondaryDeployLimit)
					return 1;

				if (_primaryAnimTime > 0)
					return _primaryAnimTime;

				if (_secondaryAnimTime > 0)
					return _secondaryAnimTime;

				return 0;
			}
		}

		public string ScalarModuleID
		{
			get { return _id; }
		}

		public bool IsMoving()
		{
			//debug.debugMessage(string.Format("Is Moving called: Primary: {0} - Secondary: {1}", primaryAnimationState, secondaryAnimationState));

			return primaryAnimationState == ModuleAnimateGeneric.animationStates.MOVING || secondaryAnimationState == ModuleAnimateGeneric.animationStates.MOVING;
		}

		public void SetScalar(float t)
		{
			bool primary = true;
			bool secondary = true;

			if (_animsPrimary == null || _animsPrimary.Length <= 0)
			{
				primary = false;
			}

			if (_animsSecondary == null || _animsSecondary.Length <= 0)
			{
				secondary = false;
			}

			if (oneShot && primaryAnimationState == ModuleAnimateGeneric.animationStates.FIXED)
			{
				primary = false;
			}

			if (oneShot && secondaryAnimationState == ModuleAnimateGeneric.animationStates.FIXED)
			{
				secondary = false;
			}

			if (!primary && !secondary)
			{
				_secondaryAnimTime = t;
				_primaryAnimTime = t;
				return;
			}

			float time = 0;

			if (primary && secondary)
			{
				time = _primaryAnimTime;
			}
			else if (primary)
			{
				time = _primaryAnimTime;
			}
			else if (secondary)
			{
				time = _secondaryAnimTime;
			}

			t = Mathf.MoveTowards(time, t, scalarAnimationSpeed * Time.deltaTime);

			if (primary)
			{
				primaryDeployed = t > 0;

                primaryLimit.guiActiveEditor = false;
                primaryLimit.guiActive = false;

                for (int i = _animsPrimary.Length - 1; i >= 0; i--)
				{
					Animation anim = _animsPrimary[i];

					if (anim == null)
						continue;

					if (anim.isPlaying)
					{
						anim.Stop(primaryAnimationName);
						onStop.Fire(anim[primaryAnimationName].normalizedTime);
					}

					anim[primaryAnimationName].speed = 0;
					anim[primaryAnimationName].enabled = true;

					anim[primaryAnimationName].normalizedTime = t;
				}
			}

			if (secondary)
			{
				secondaryDeployed = t > 0;

                secondaryLimit.guiActiveEditor = false;
                secondaryLimit.guiActive = false;

                for (int i = _animsSecondary.Length - 1; i >= 0; i--)
				{
					Animation anim = _animsSecondary[i];

					if (anim == null)
						continue;

					if (anim.isPlaying)
					{
						anim.Stop(secondaryAnimationName);
						onStop.Fire(anim[secondaryAnimationName].normalizedTime);
					}

					anim[secondaryAnimationName].speed = 0;
					anim[secondaryAnimationName].enabled = true;

					anim[secondaryAnimationName].normalizedTime = t;
				}
			}

			_primaryAnimTime = t;
			_secondaryAnimTime = t;

			if (Mathf.Abs(t) >= 0.99f)
			{
				if (primary)
				{
					primaryDeployed = true;
					if (oneShot)
					{
						primaryAnimationState = ModuleAnimateGeneric.animationStates.FIXED;
						tglEventPrimary.active = false;
					}
					else
					{
						primaryAnimationState = ModuleAnimateGeneric.animationStates.LOCKED;
						tglEventPrimary.guiName = _localizedPrimaryEndString;
						tglEventPrimary.guiActive = primaryAvailableInVessel;
					}

                    primaryLimit.guiActiveEditor = allowPrimaryDeployLimit;
                    primaryLimit.guiActive = primaryDeployLimitInFlight;
                }

				if (secondary)
				{
					secondaryDeployed = true;
					if (oneShot)
					{
						secondaryAnimationState = ModuleAnimateGeneric.animationStates.FIXED;
						tglEventSecondary.active = false;
					}
					else
					{
						secondaryAnimationState = ModuleAnimateGeneric.animationStates.LOCKED;
						tglEventSecondary.guiName = _localizedSecondaryEndString;
						tglEventSecondary.guiActive = secondaryAvailableInVessel;
					}

                    secondaryLimit.guiActiveEditor = allowSecondaryDeployLimit;
                    secondaryLimit.guiActive = secondaryDeployLimitInFlight;
                }
			}
			else if (Mathf.Abs(t) < 0.01f)
			{
				if (primary)
				{
					primaryAnimationState = ModuleAnimateGeneric.animationStates.LOCKED;
					primaryDeployed = false;
					tglEventPrimary.guiName = _localizedPrimaryStartString;
					tglEventPrimary.guiActive = primaryAvailableInVessel;

                    primaryLimit.guiActiveEditor = allowPrimaryDeployLimit;
                    primaryLimit.guiActive = primaryDeployLimitInFlight;
                }

				if (secondary)
				{
					secondaryAnimationState = ModuleAnimateGeneric.animationStates.LOCKED;
					secondaryDeployed = false;
					tglEventSecondary.guiName = _localizedSecondaryStartString;
					tglEventSecondary.guiActive = secondaryAvailableInVessel;

                    secondaryLimit.guiActiveEditor = allowSecondaryDeployLimit;
                    secondaryLimit.guiActive = secondaryDeployLimitInFlight;
                }
			}
			else
			{
				if (primary)
				{
					primaryAnimationState = ModuleAnimateGeneric.animationStates.MOVING;
					tglEventPrimary.guiActive = false;
				}

				if (secondary)
				{
					secondaryAnimationState = ModuleAnimateGeneric.animationStates.MOVING;
					tglEventSecondary.guiActive = false;
				}
			}

			if (primaryAnimationState == ModuleAnimateGeneric.animationStates.MOVING || secondaryAnimationState == ModuleAnimateGeneric.animationStates.MOVING)
			{
				status = _localizedMoving;
			}
			else if (primaryAnimationState == ModuleAnimateGeneric.animationStates.LOCKED || secondaryAnimationState == ModuleAnimateGeneric.animationStates.LOCKED)
			{
				status = _localizedLocked;
			}
			else if (primaryAnimationState == ModuleAnimateGeneric.animationStates.FIXED || secondaryAnimationState == ModuleAnimateGeneric.animationStates.FIXED)
			{
				status = _localizedFixed;
			}
		}

		public void SetUIRead(bool state)
		{

		}

		public void SetUIWrite(bool state)
		{

		}
	}
}
