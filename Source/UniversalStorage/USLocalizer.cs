
using System.Collections;
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
            StartCoroutine(WaitForLocalize(localizer, tag));
        }

        private IEnumerator WaitForLocalize(KSPediaLocalizer localizer, string tag)
        {
            yield return new WaitForEndOfFrame();

            localizer.UpdateText(Localizer.Format(tag));
        }
    }
}
