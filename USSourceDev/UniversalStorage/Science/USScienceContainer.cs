using System.Collections;
using UnityEngine;

namespace UniversalStorage2
{
    public class USScienceContainer : ModuleScienceContainer
    {
        [KSPField]
        public string deployAnimationName = null;
        [KSPField]
        public string doorAnimationName = null;
        [KSPField]
        public string paperfeedAnimationName = null;
        [KSPField]
        public string startEventGUIName = "#autoLOC_502050"; // Deploy
        [KSPField]
        public string endEventGUIName = "#autoLOC_7003224"; // Retract
        [KSPField]
        public bool deployAvailableInEVA = true;
        [KSPField]
        public bool deployAvailableInVessel = true;
        [KSPField]
        public bool deployAvailableInEditor = true;
        [KSPField]
        public float interactionRange = 3;
        [KSPField]
        public bool autoRetract = false;
        [KSPField]
        public float maxAnimationLength = 0;

        [KSPField]
        public float animSpeed = 1f;

        [KSPField(isPersistant = true)]
        public bool IsDeployed;

        internal class AnimInfo
        {
            internal string animationName;
            internal Animation anim;
            internal float animLen;
        }

        AnimInfo[] animations = new AnimInfo[3];
        int activeAnim = 0;

        //public void Start()
        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight)
            {
                UpdateActions();
                animations[0] = SetUpAnimation(deployAnimationName, part);
                animations[1] = SetUpAnimation(doorAnimationName, part);
                animations[2] = SetUpAnimation(paperfeedAnimationName, part);
                Events["DeployKeyboardGUIName"].guiName = startEventGUIName;
                Events["RetractKeyboardGUIName"].guiName = endEventGUIName;
                Events["DeployKeyboardGUIName"].unfocusedRange = interactionRange;
                Events["RetractKeyboardGUIName"].unfocusedRange = interactionRange;


            }

        }

        void UpdateActions()
        {
            Events["CollectAllEvent"].active =
                Events["CollectAllEvent"].guiActive =
                Events["CollectAllEvent"].guiActiveUnfocused = fullyDeployed;

            Events["CollectDataExternalEvent"].active =
                Events["CollectDataExternalEvent"].guiActive =
                Events["CollectDataExternalEvent"].guiActiveUnfocused = IsDeployed;

            Events["DeployKeyboardGUIName"].active =
                Events["DeployKeyboardGUIName"].guiActive =
                Events["DeployKeyboardGUIName"].guiActiveUnfocused = !IsDeployed;

            Events["RetractKeyboardGUIName"].active =
                Events["RetractKeyboardGUIName"].guiActive =
            Events["RetractKeyboardGUIName"].guiActiveUnfocused = IsDeployed;
        }

        //[KSPEvent(active = true, guiActive = false, guiActiveUnfocused = true, guiName = "#autoLOC_6001808")]
        [KSPEvent(active = true, guiActive = true, guiActiveUnfocused = true, guiName = "Deploy Keyboard")]
        public void DeployKeyboardGUIName()
        {
            IsDeployed = true;
            UpdateActions();
            activeAnim = 0;
            animations[activeAnim].anim[animations[activeAnim].animationName].speed = 1;
            animations[activeAnim].anim.Play();
            StartCoroutine(SlowUpdate());
        }

        //[KSPEvent(active = true, guiActive = false, guiActiveUnfocused = true, guiName = "#autoLOC_6001808")]
        [KSPEvent(active = true, guiActive = true, guiActiveUnfocused = true, guiName = "Retract Keyboard")]
        public void RetractKeyboardGUIName()
        {
            IsDeployed = false;
            UpdateActions();

            // Don't bother reversing the paper feed
            if (activeAnim == animations.Length - 1)
                activeAnim--;
            animations[activeAnim].anim[animations[activeAnim].animationName].speed = -1;
            animations[activeAnim].anim.Play();
            StartCoroutine(SlowUpdate());
        }

        internal AnimInfo SetUpAnimation(string animationName, Part part)  //Thanks Majiir!
        {
            AnimInfo animInfo = new AnimInfo();

            var ma = part.FindModelAnimators(animationName);
            if (ma != null && ma.Length > 0)
                animInfo.anim = ma[0];
            animInfo.animationName = animationName;
            animInfo.anim.wrapMode = WrapMode.ClampForever;
            animInfo.animLen = animInfo.anim[animationName].length;
            return animInfo;
        }


        bool fullyDeployed = false;
        IEnumerator SlowUpdate()
        {
            float start = Time.time;

            while (true)
            {
                if (IsDeployed)
                {
                    if (animations[activeAnim].anim.isPlaying && animations[activeAnim].anim[animations[activeAnim].animationName].time > animations[activeAnim].animLen)
                    {
                        animations[activeAnim].anim.Stop();
                        animations[activeAnim].anim[animations[activeAnim].animationName].time = animations[activeAnim].animLen;
                        if (activeAnim < animations.Length - 1)
                        {
                            activeAnim++;
                            animations[activeAnim].anim[animations[activeAnim].animationName].speed = 1;
                            animations[activeAnim].anim.Play();
                        }
                        else
                        {
                            fullyDeployed = true;
                            UpdateActions();
                            break;
                        }
                    }
                    else
                    {
                        if (activeAnim == animations.Length)
                        {
                            fullyDeployed = true;
                            UpdateActions();
                            break;
                        }
                    }
                    if (maxAnimationLength > 0 && Time.time - start >= maxAnimationLength)
                    {
                        fullyDeployed = true;
                        UpdateActions();
                    }
                }
                else
                {
                    fullyDeployed = false;
                    UpdateActions();
                    if (animations[activeAnim].anim.isPlaying && animations[activeAnim].anim[animations[activeAnim].animationName].time <= 0)
                    {
                        animations[activeAnim].anim.Stop();
                        animations[activeAnim].anim[animations[activeAnim].animationName].time = 0;
                        if (activeAnim > 0)
                        {
                            activeAnim--;
                            animations[activeAnim].anim[animations[activeAnim].animationName].speed = -1;
                            animations[activeAnim].anim.Play();
                        }
                        else
                            break;
                    }
                }
                yield return new WaitForSeconds(0.1f);
            }
            yield return null;
        }
    }
}
