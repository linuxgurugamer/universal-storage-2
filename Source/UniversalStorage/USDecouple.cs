
using System.Collections;
using UnityEngine;

namespace UniversalStorage
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

            OnDecouple();
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
