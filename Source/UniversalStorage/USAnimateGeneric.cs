using System;
using System.Collections;
using UnityEngine;

namespace UniversalStorage
{
	public class USAnimateGeneric : PartModule, IScalarModule
	{
		[KSPField(guiActive = true, guiActiveEditor = true)]
		public string status = "Locked";
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

		private Animation[] _animsPrimary;
		private Animation[] _animsSecondary;
        
		private EventData<float> onStop;
		private EventData<float, float> onMove;
        
		private USdebugMessages debug;

        private Shader _DebugLineShader;

        private USJettisonSwitch[] jettisonModules;
        private USDragSwitch dragModule;
        
        private int[] _SwitchIndices;

        private int[] _ControlStates;
        
        private double[] _PrimaryObstructionLengths;
        private double[] _SecondaryObstructionLengths;
        private Transform[] _PrimaryObstructionSources;
        private Transform[] _SecondaryObstructionSources;

        private float _PrimaryObstructionLength;
        private float _SecondaryObstructionLength;
        
        private EventData<int, int, Part> onUSSwitch;

        public override void OnAwake()
		{
			base.OnAwake();

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

			onStop = new EventData<float>(string.Format("{0}_{1}_onStop", part.partName, part.flightID));
			onMove = new EventData<float, float>(string.Format("{0}_{1}_onMove", part.partName, part.flightID));
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            debug = new USdebugMessages(DebugMode, "USAnimateGeneric");

            _DebugLineShader = Shader.Find("Particles/Alpha Blended Premultiply");

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

            _animSpeed = customAnimationSpeed;
        }

        public override void OnStartFinished(StartState state)
        {
            base.OnStartFinished(state);

            _animsPrimary = part.FindModelAnimators(primaryAnimationName);
            _animsSecondary = part.FindModelAnimators(secondaryAnimationName);

            if (_animsPrimary != null)
                debug.debugMessage("[US_Anim] Primary Animations found: " + _animsPrimary.Length);

            if (_animsSecondary != null)
                debug.debugMessage("[US_Anim] Secondary Animations found: " + _animsPrimary.Length);
            
            if (_animsPrimary != null && _animsPrimary.Length > 0)
            {
                tglEventPrimary.guiActiveUnfocused = primaryAvailableInEVA;
                tglEventPrimary.guiActive = primaryAvailableInVessel;
                tglEventPrimary.guiActiveEditor = primaryAvailableInEditor;
                tglEventPrimary.unfocusedRange = EVArange;
                tglEventPrimary.active = !(allowDoorLock && lockPrimaryDoors);
                tglActionPrimary.active = primaryActionAvailable && !(allowDoorLock && lockPrimaryDoors);

                lockEventPrimary.guiName = lockPrimaryDoors ? unlockPrimaryDoorName : lockPrimaryDoorName;

                if (primaryDeployed)
                {
                    tglActionPrimary.guiName = primaryEndEventGUIName;
                    tglEventPrimary.guiName = primaryEndEventGUIName;
                    _primaryAnimTime = 1;

                    if (oneShot)
                        primaryAnimationState = ModuleAnimateGeneric.animationStates.FIXED;
                }
                else
                {
                    tglActionPrimary.guiName = primaryStartEventGUIName;
                    tglEventPrimary.guiName = primaryStartEventGUIName;
                    _primaryAnimTime = 0;
                }

                if (!string.IsNullOrEmpty(primaryToggleActionName))
                    tglActionPrimary.guiName = primaryToggleActionName;

                for (int i = _animsPrimary.Length - 1; i >= 0; i--)
                {
                    Animation anim = _animsPrimary[i];

                    if (anim == null)
                        continue;

                    debug.debugMessage(string.Format("Primary Animation Clips: {0} - Name: {1}", anim.GetClipCount(), anim.clip.name));

                    anim.playAutomatically = false;
                    anim.cullingType = AnimationCullingType.BasedOnRenderers;
                    anim[primaryAnimationName].wrapMode = WrapMode.Once;

                    if (!anim.gameObject.activeInHierarchy)
                        continue;

                    //animate(anim, primaryAnimationName, 0, _primaryAnimTime);

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

                lockEventSecondary.guiName = lockSecondaryDoors ? unlockSecondaryDoorName : lockSecondaryDoorName;

                if (secondaryDeployed)
                {
                    tglActionSecondary.guiName = secondaryEndEventGUIName;
                    tglEventSecondary.guiName = secondaryEndEventGUIName;
                    _secondaryAnimTime = 1;

                    if (oneShot)
                        secondaryAnimationState = ModuleAnimateGeneric.animationStates.FIXED;
                }
                else
                {
                    tglActionSecondary.guiName = secondaryStartEventGUIName;
                    tglEventSecondary.guiName = secondaryStartEventGUIName;
                    _secondaryAnimTime = 0;
                }

                if (!string.IsNullOrEmpty(secondaryToggleActionName))
                    tglActionSecondary.guiName = secondaryToggleActionName;

                for (int i = _animsSecondary.Length - 1; i >= 0; i--)
                {
                    Animation anim = _animsSecondary[i];

                    if (anim == null)
                        continue;

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
                    tglActionCombined.guiName = combinedEndEventGUIName;
                    tglEventCombined.guiName = combinedEndEventGUIName;
                }
                else
                {
                    tglActionCombined.guiName = combinedStartEventGUIName;
                    tglEventCombined.guiName = combinedStartEventGUIName;
                }

                if (!string.IsNullOrEmpty(combinedToggleActionName))
                    tglActionCombined.guiName = combinedToggleActionName;
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
            
            if (CurrentSelection >= 0)
            {
                UpdateEventStates();
            }

            if (!UseDoorObstructions)
                return;

            debug.debugMessage("Searching For Door Obstructors...");

            if (!String.IsNullOrEmpty(PrimaryDoorObstructionLength))
                _PrimaryObstructionLengths = USTools.parseDoubles(PrimaryDoorObstructionLength).ToArray();

            if (!String.IsNullOrEmpty(SecondaryDoorObstructionLength))
                _SecondaryObstructionLengths = USTools.parseDoubles(SecondaryDoorObstructionLength).ToArray();

            if (!String.IsNullOrEmpty(PrimaryDoorObstructionSource))
                _PrimaryObstructionSources = part.FindModelTransforms(PrimaryDoorObstructionSource);

            if (!String.IsNullOrEmpty(SecondaryDoorObstructionSource))
                _SecondaryObstructionSources = part.FindModelTransforms(SecondaryDoorObstructionSource);
            
            if (CurrentSelection > 0)
                UpdateCollisionLength();

            //var triggers = part.FindModelTransforms(DoorObstructionTrigger);

            //for (int i = triggers.Length - 1; i >= 0; i--)
            //{
            //    GameObject obj = triggers[i].gameObject;

            //    var trigger = triggers[i].gameObject.AddComponent<USDoorTrigger>();

            //    trigger.Init(this);
            //}
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

		//private void LateUpdate()
		//{
		//	if (!_primaryAnimStarted && !_secondaryAnimStarted)
		//		return;

		//	if (_primaryAnimStarted)
		//	{
		//		for (int i = _animsPrimary.Length - 1; i >= 0; i--)
		//		{
		//			Animation anim = _animsPrimary[i];

		//			if (anim == null)
		//				continue;
		//			debug.debugMessage(string.Format("Primary Animation stopped"));
		//			anim[primaryAnimationName].enabled = false;
		//			anim.Stop(primaryAnimationName);
		//		}

		//		_primaryAnimStarted = false;
		//	}

		//	if (_secondaryAnimStarted)
		//	{
		//		for (int i = _animsSecondary.Length - 1; i >= 0; i--)
		//		{
		//			Animation anim = _animsSecondary[i];

		//			if (anim == null)
		//				continue;
		//			debug.debugMessage(string.Format("Secondary Animation stopped"));
		//			anim[secondaryAnimationName].enabled = false;
		//			anim.Stop(secondaryAnimationName);
		//		}

		//		_secondaryAnimStarted = false;
		//	}
		//}

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
									_primaryAnimTime = 1;
								else
									_primaryAnimTime = 0;

								anim[primaryAnimationName].normalizedTime = _primaryAnimTime;
								SetDragCubes(1 - _primaryAnimTime);
								anim.Stop(primaryAnimationName);
								onStop.Fire(_primaryAnimTime);
							}
							else
							{
								//debug.debugMessage(string.Format("Animation {0} playing - Time: {1:F2}", primaryAnimationName, anim[primaryAnimationName].normalizedTime));
								_primaryAnimTime = anim[primaryAnimationName].normalizedTime;
								primaryAnimationState = ModuleAnimateGeneric.animationStates.MOVING;
								SetDragCubes(1 - _primaryAnimTime);
							}
						}
					}
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
									_secondaryAnimTime = 1;
								else
									_secondaryAnimTime = 0;

								anim[secondaryAnimationName].normalizedTime = _secondaryAnimTime;
								anim.Stop(secondaryAnimationName);
								onStop.Fire(_secondaryAnimTime);
                            }
							else
							{
								//debug.debugMessage(string.Format("Animation {0} playing - Time: {1:F2}", secondaryAnimationName, anim[secondaryAnimationName].normalizedTime));
								_secondaryAnimTime = anim[secondaryAnimationName].normalizedTime;
								secondaryAnimationState = ModuleAnimateGeneric.animationStates.MOVING;
							}
						}
					}
				}
			}

			if (primaryAnimationState == ModuleAnimateGeneric.animationStates.MOVING || secondaryAnimationState == ModuleAnimateGeneric.animationStates.MOVING)
			{
				status = "Moving...";
			}
			else if (primaryAnimationState == ModuleAnimateGeneric.animationStates.LOCKED || secondaryAnimationState == ModuleAnimateGeneric.animationStates.LOCKED)
			{
				status = "Locked";
			}
			else if (primaryAnimationState == ModuleAnimateGeneric.animationStates.FIXED || secondaryAnimationState == ModuleAnimateGeneric.animationStates.FIXED)
			{
				status = "Fixed";
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
                _PrimaryObstructionLength = (float)_PrimaryObstructionLengths[CurrentSelection];

            if (_SecondaryObstructionLengths != null && _SecondaryObstructionLengths.Length > CurrentSelection)
                _SecondaryObstructionLength = (float)_SecondaryObstructionLengths[CurrentSelection];

            if (_PrimaryObstructionSources != null)
                DrawCollisionLines(_PrimaryObstructionSources, _PrimaryObstructionLength, Color.blue);

            if (_SecondaryObstructionSources != null)
                DrawCollisionLines(_SecondaryObstructionSources, _SecondaryObstructionLength, Color.red);
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

            lockEventPrimary.guiName = lockPrimaryDoors ? unlockPrimaryDoorName : lockPrimaryDoorName;

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

            lockEventSecondary.guiName = lockSecondaryDoors ? unlockSecondaryDoorName : lockSecondaryDoorName;

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
					tglActionPrimary.guiName = primaryEndEventGUIName;
					tglEventPrimary.guiName = primaryEndEventGUIName;

					if (!string.IsNullOrEmpty(primaryToggleActionName))
						tglActionPrimary.guiName = primaryToggleActionName;

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
					tglActionSecondary.guiName = secondaryEndEventGUIName;
					tglEventSecondary.guiName = secondaryEndEventGUIName;

					if (!string.IsNullOrEmpty(secondaryToggleActionName))
						tglActionSecondary.guiName = secondaryToggleActionName;

					if (_animsSecondary != null)
					{
						for (int i = _animsSecondary.Length - 1; i >= 0; i--)
							animate(_animsSecondary[i], secondaryAnimationName, _animSpeed, 0);
					}

					secondaryAnimationState = ModuleAnimateGeneric.animationStates.MOVING;
				}

				onMove.Fire(0, 1);

				tglActionCombined.guiName = combinedEndEventGUIName;
				tglEventCombined.guiName = combinedEndEventGUIName;

				if (!string.IsNullOrEmpty(combinedToggleActionName))
					tglActionCombined.guiName = combinedToggleActionName;
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
                        tglActionPrimary.guiName = primaryStartEventGUIName;
                        tglEventPrimary.guiName = primaryStartEventGUIName;

                        if (!string.IsNullOrEmpty(primaryToggleActionName))
                            tglActionPrimary.guiName = primaryToggleActionName;

                        if (_animsPrimary != null)
                        {
                            for (int i = _animsPrimary.Length - 1; i >= 0; i--)
                                animate(_animsPrimary[i], primaryAnimationName, _animSpeed * -1f, 1);
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
                        tglActionSecondary.guiName = secondaryStartEventGUIName;
                        tglEventSecondary.guiName = secondaryStartEventGUIName;

                        if (!string.IsNullOrEmpty(secondaryToggleActionName))
                            tglActionSecondary.guiName = secondaryToggleActionName;

                        if (_animsSecondary != null)
                        {
                            for (int i = _animsSecondary.Length - 1; i >= 0; i--)
                                animate(_animsSecondary[i], secondaryAnimationName, _animSpeed * -1f, 1);
                        }

                        secondaryAnimationState = ModuleAnimateGeneric.animationStates.MOVING;
                    }
				}

				onMove.Fire(1, 0);

				tglActionCombined.guiName = combinedStartEventGUIName;
				tglEventCombined.guiName = combinedStartEventGUIName;

				if (!string.IsNullOrEmpty(combinedToggleActionName))
					tglActionCombined.guiName = combinedToggleActionName;
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
                        return;
                    }
                }

                primaryDeployed = false;
				tglActionPrimary.guiName = primaryStartEventGUIName;
				tglEventPrimary.guiName = primaryStartEventGUIName;

				if (!string.IsNullOrEmpty(primaryToggleActionName))
					tglActionPrimary.guiName = primaryToggleActionName;

				if (_animsPrimary != null)
				{
					for (int i = _animsPrimary.Length - 1; i >= 0; i--)
						animate(_animsPrimary[i], primaryAnimationName, _animSpeed * -1f, 1);

					onMove.Fire(1, 0);
				}

				if (!secondaryDeployed)
				{
					combinedDeployed = false;

					tglActionCombined.guiName = combinedStartEventGUIName;
					tglEventCombined.guiName = combinedStartEventGUIName;

					if (!string.IsNullOrEmpty(combinedToggleActionName))
						tglActionCombined.guiName = combinedToggleActionName;
				}
			}
			else
			{
				primaryDeployed = true;
				tglActionPrimary.guiName = primaryEndEventGUIName;
				tglEventPrimary.guiName = primaryEndEventGUIName;

				if (!string.IsNullOrEmpty(primaryToggleActionName))
					tglActionPrimary.guiName = primaryToggleActionName;

				if (_animsPrimary != null)
				{
					for (int i = _animsPrimary.Length - 1; i >= 0; i--)
						animate(_animsPrimary[i], primaryAnimationName, _animSpeed, 0);

					onMove.Fire(0, 1);
				}

				if (secondaryDeployed)
				{
					combinedDeployed = true;

					tglActionCombined.guiName = combinedEndEventGUIName;
					tglEventCombined.guiName = combinedEndEventGUIName;

					if (!string.IsNullOrEmpty(combinedToggleActionName))
						tglActionCombined.guiName = combinedToggleActionName;
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
                        return;
                    }
                }

				secondaryDeployed = false;
				tglActionSecondary.guiName = secondaryStartEventGUIName;
				tglEventSecondary.guiName = secondaryStartEventGUIName;

				if (!string.IsNullOrEmpty(secondaryToggleActionName))
					tglActionSecondary.guiName = secondaryToggleActionName;

				if (_animsSecondary != null)
				{
					for (int i = _animsSecondary.Length - 1; i >= 0; i--)
						animate(_animsSecondary[i], secondaryAnimationName, _animSpeed * -1f, 1);

					onMove.Fire(1, 0);
				}

				if (!primaryDeployed)
				{
					combinedDeployed = false;

					tglActionCombined.guiName = combinedStartEventGUIName;
					tglEventCombined.guiName = combinedStartEventGUIName;

					if (!string.IsNullOrEmpty(combinedToggleActionName))
						tglActionCombined.guiName = combinedToggleActionName;
				}
			}
			else
			{
				secondaryDeployed = true;
				tglActionSecondary.guiName = secondaryEndEventGUIName;
				tglEventSecondary.guiName = secondaryEndEventGUIName;

				if (!string.IsNullOrEmpty(secondaryToggleActionName))
					tglActionSecondary.guiName = secondaryToggleActionName;

				if (_animsSecondary != null)
				{
					for (int i = _animsSecondary.Length - 1; i >= 0; i--)
						animate(_animsSecondary[i], secondaryAnimationName, _animSpeed, 0);

					onMove.Fire(0, 1);
				}

				if (primaryDeployed)
				{
					combinedDeployed = true;

					tglActionCombined.guiName = combinedEndEventGUIName;
					tglEventCombined.guiName = combinedEndEventGUIName;

					if (!string.IsNullOrEmpty(combinedToggleActionName))
						tglActionCombined.guiName = combinedToggleActionName;
				}
			}
		}

        private bool PrimaryCollisionCheck()
        {
            if (_PrimaryObstructionSources == null)
                return false;

            if (ObstructionDebugLines)
                DrawCollisionLines(_PrimaryObstructionSources, _PrimaryObstructionLength, Color.blue);

            debug.debugMessage("Testing for primary door collisions before closing...");

            for (int i = _PrimaryObstructionSources.Length - 1; i >= 0; i--)
            {
                Transform source = _PrimaryObstructionSources[i];

                Ray r = new Ray(source.position, source.forward);
                RaycastHit hit = new RaycastHit();

                if (Physics.Raycast(r, out hit, _PrimaryObstructionLength, 1 << 0))
                {
                    if (hit.collider != null)
                    {
                        if (hit.collider.attachedRigidbody != null)
                        {
                            Part p = Part.FromGO(hit.transform.gameObject);

                            if (p != null)
                            {
                                ScreenMessages.PostScreenMessage("Obstruction detected in primary bay doors. Please clear objects before closing.", 4f, ScreenMessageStyle.UPPER_CENTER);
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
                DrawCollisionLines(_SecondaryObstructionSources, _SecondaryObstructionLength, Color.red);

            debug.debugMessage("Testing for secondary door collisions before closing...");

            for (int i = _SecondaryObstructionSources.Length - 1; i >= 0; i--)
            {
                Transform source = _SecondaryObstructionSources[i];

                Ray r = new Ray(source.position, source.forward);
                RaycastHit hit = new RaycastHit();

                if (Physics.Raycast(r, out hit, _SecondaryObstructionLength, 1 << 0))
                {
                    if (hit.collider != null)
                    {
                        if (hit.collider.attachedRigidbody != null)
                        {
                            Part p = Part.FromGO(hit.transform.gameObject);

                            if (p != null)
                            {
                                ScreenMessages.PostScreenMessage("Obstruction detected in secondary bay doors. Please clear objects before closing.", 4f, ScreenMessageStyle.UPPER_CENTER);
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

			debug.debugMessage(string.Format("Animation Play: Speed {0:F2} - Time: {1:F2}", speed, time));

			anim[animationName].speed = speed;

			if (!anim.IsPlaying(animationName))
			{
				anim[animationName].enabled = true;
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

				if (_primaryAnimTime >= 1 || _secondaryAnimTime >= 1)
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
						tglEventPrimary.guiName = primaryEndEventGUIName;
						tglEventPrimary.guiActive = primaryAvailableInVessel;
					}
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
						tglEventSecondary.guiName = secondaryEndEventGUIName;
						tglEventSecondary.guiActive = secondaryAvailableInVessel;
					}
				}
			}
			else if (Mathf.Abs(t) < 0.01f)
			{
				if (primary)
				{
					primaryAnimationState = ModuleAnimateGeneric.animationStates.LOCKED;
					primaryDeployed = false;
					tglEventPrimary.guiName = primaryStartEventGUIName;
					tglEventPrimary.guiActive = primaryAvailableInVessel;
				}

				if (secondary)
				{
					secondaryAnimationState = ModuleAnimateGeneric.animationStates.LOCKED;
					secondaryDeployed = false;
					tglEventSecondary.guiName = secondaryStartEventGUIName;
					tglEventSecondary.guiActive = secondaryAvailableInVessel;
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
				status = "Moving...";
			}
			else if (primaryAnimationState == ModuleAnimateGeneric.animationStates.LOCKED || secondaryAnimationState == ModuleAnimateGeneric.animationStates.LOCKED)
			{
				status = "Locked";
			}
			else if (primaryAnimationState == ModuleAnimateGeneric.animationStates.FIXED || secondaryAnimationState == ModuleAnimateGeneric.animationStates.FIXED)
			{
				status = "Fixed";
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
