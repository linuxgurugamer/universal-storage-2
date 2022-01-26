
using System.Collections;
using UnityEngine;
using KSP.Localization;

namespace UniversalStorage2
{
    public class USDecouple : ModuleDecouple, IStageSeparator
    {
        [KSPField]
        public string DecoupleAnimationName;
        [KSPField]
        public float DecoupleTime = -1;
        [KSPField]
        public bool DecoupleEVA = true;
        [KSPField]
        public float AnimationSpeed = 1;
        [KSPField]
        public bool debrisAfterDecouple = true;
        [KSPField]
        public string nameSuffix = "";


        private BaseEvent decoupleEvent;
        private BaseAction decoupleAction;

        private Animation[] _decoupleAnimation;

        public override void OnAwake()
        {
            base.OnAwake();

            decoupleEvent = Events["Decouple"];
            decoupleAction = Actions["DecoupleAction"];
            decoupleEvent.active = false;
            decoupleAction.active = false;
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            decoupleEvent.guiActiveUnfocused = DecoupleEVA;

            decoupleEvent.active = !isDecoupled;
            decoupleAction.active = !isDecoupled;
        }

        public override void OnStartFinished(StartState state)
        {
            base.OnStartFinished(state);

            if (!string.IsNullOrEmpty(DecoupleAnimationName))
                _decoupleAnimation = part.FindModelAnimators(DecoupleAnimationName);
        }

        public override void OnActive()
        {
            if (staged)
                Decouple();
        }

        new public void DecoupleAction(KSPActionParam param)
        {
            Decouple();
        }

        new public void Decouple()
        {
            decoupleEvent.active = false;
            decoupleAction.active = false;

            if (DecoupleTime < 0 || _decoupleAnimation == null || _decoupleAnimation.Length <= 0)
                OnDecouple();
            else
                StartCoroutine(WaitForDecouple());
        }

        private IEnumerator WaitForDecouple()
        {
            float time = 0;

            for (int i = _decoupleAnimation.Length - 1; i >= 0; i--)
            {
                Animation anim = _decoupleAnimation[i];

                if (anim == null)
                    continue;

                if (anim.gameObject.activeInHierarchy)
                {
                    Animate(anim, AnimationSpeed);

                    time = anim[DecoupleAnimationName].length * DecoupleTime;
                }
            }

            if (time > 0)
                yield return new WaitForSeconds(time);

#if true 
            var mj = this.part.FindModuleImplementing<ModuleJettison>();
            if (mj)
            {
                mj.decoupleEnabled = true;
                Transform transform = base.part.FindModelTransform(name);
                if (transform != null && transform.gameObject.activeSelf)
                {
                    mj.activejettisonName = name;
                    mj.jettisonTransform = transform;
                }

                AttachNode attachNode = base.part.FindAttachNode(mj.bottomNodeName);
                if (attachNode != null)
                {
                    mj.jettisonTransform.parent = attachNode.attachedPart.gameObject.transform;
                    Debug.Log("USDecouple, mj.jettisonTransform.parent is set");
                }
                mj.Jettison();
            }
#endif
            OnDecouple();
            Debug.Log("USDecouple, after OnDecouple");

            if (!debrisAfterDecouple)
            {
                this.vessel.vesselType = VesselType.Probe;
                string str = this.vessel.vesselName;
                int idx1 = str.LastIndexOf("Debris");
                if (idx1 < 0)
                {
                    int idx2 = str.LastIndexOf(Localizer.Format("#autoLOC_900676"));
                    if (idx2 < 0)
                    {
                        int idx3 = str.LastIndexOf(Localizer.Format("#autoLOC_6100044"));
                        if (idx3 >= 0)
                        {
                            str = str.Substring(0, idx3) + " " + nameSuffix;
                        }
                    }
                    else
                    {
                        str = str.Substring(0, idx2) + " " + nameSuffix;
                    }
                }
                else
                {
                    str = str.Substring(0, idx1) + " " + nameSuffix;
                }
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
