
using UnityEngine;

namespace UniversalStorage
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class USEvents : MonoBehaviour
    {
        public static EventData<int, int, Part> onUSSwitch;
        public static EventData<int, int, bool, Part> onUSFuelSwitch;
        public static EventData<int, Part, USFuelSwitch> onFuelRequestCost;
        public static EventData<int, Part, USFuelSwitch> onFuelRequestMass;

        private void Awake()
        {
            onUSSwitch = new EventData<int, int, Part>("onUSSwitch");
            onUSFuelSwitch = new EventData<int, int, bool, Part>("onUSFuelSwitch");
            onFuelRequestCost = new EventData<int, Part, USFuelSwitch>("onFuelRequestCost");
            onFuelRequestMass = new EventData<int, Part, USFuelSwitch>("onFuelRequestMass");

            Destroy(gameObject);
        }
    }
}
