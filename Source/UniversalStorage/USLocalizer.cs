using UnityEngine;
using KSP.Localization;
using UniversalStorage.Unity;

namespace UniversalStorage
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class USLocalizer : MonoBehaviour
    {
        private void Awake()
        {
            KSPediaLocalizer.onLocalize.AddListener(OnLocalize);
        }

        private void OnDestroy()
        {
            KSPediaLocalizer.onLocalize.RemoveListener(OnLocalize);
        }

        private void OnLocalize(KSPediaLocalizer localizer, string tag)
        {
            USdebugMessages.USStaticLog("Localize Request Received: {0}", tag);

            localizer.UpdateText(Localizer.Format(tag));
        }
    }
}
