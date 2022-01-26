
using UnityEngine;

namespace UniversalStorage
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class USEvents : MonoBehaviour
    {
        public static EventData<int, int, AvailablePart, Transform> onUSEditorIconSwitch;
        public static EventData<AvailablePart> onUSEditorPrimaryVariantSwitched;
        public static EventData<AvailablePart> onUSEditorSecondaryVariantSwitched;
        public static EventData<int, int, Part> onUSSwitch;
        public static EventData<int, int, bool, Part> onUSFuelSwitch;
        public static EventData<int, Part, USFuelSwitch> onFuelRequestCost;
        public static EventData<int, Part, USFuelSwitch> onFuelRequestMass;

        private void Awake()
        {
            onUSEditorIconSwitch = new EventData<int, int, AvailablePart, Transform>("onUSEditorIconSwitch");
            onUSEditorPrimaryVariantSwitched = new EventData<AvailablePart>("onUSEditorPrimaryVariantSwitched");
            onUSEditorSecondaryVariantSwitched = new EventData<AvailablePart>("onUSEditorSecondaryVariantSwitched");
            onUSSwitch = new EventData<int, int, Part>("onUSSwitch");
            onUSFuelSwitch = new EventData<int, int, bool, Part>("onUSFuelSwitch");
            onFuelRequestCost = new EventData<int, Part, USFuelSwitch>("onFuelRequestCost");
            onFuelRequestMass = new EventData<int, Part, USFuelSwitch>("onFuelRequestMass");

            Destroy(gameObject);
        }
    }
}
