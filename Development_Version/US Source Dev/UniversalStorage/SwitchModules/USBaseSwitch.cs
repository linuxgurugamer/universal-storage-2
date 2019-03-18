using System;

namespace UniversalStorage2
{
    public class USBaseSwitch : PartModule
    {
        [KSPField]
        public string SwitchID = string.Empty;

        protected int[] _SwitchIndices;
        protected bool _switcher;

        private EventData<int, int, Part> onUSSwitch;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (String.IsNullOrEmpty(SwitchID))
            {
                _switcher = false;
                return;
            }

            _switcher = true;

            _SwitchIndices = USTools.parseIntegers(SwitchID).ToArray();

            onUSSwitch = GameEvents.FindEvent<EventData<int, int, Part>>("onUSSwitch");

            if (onUSSwitch != null)
                onUSSwitch.Add(onSwitch);
        }

        private void OnDestroy()
        {
            if (onUSSwitch != null)
                onUSSwitch.Remove(onSwitch);
        }

        protected virtual void onSwitch(int index, int selection, Part p) { }

    }
}
