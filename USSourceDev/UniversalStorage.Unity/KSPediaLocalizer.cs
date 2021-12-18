
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace UniversalStorage2.Unity
{
    public class LocalizeKSPedia : UnityEvent<KSPediaLocalizer, string> { }

    [RequireComponent(typeof(Text))]
    public class KSPediaLocalizer : MonoBehaviour
    {
        public static LocalizeKSPedia onLocalize = new LocalizeKSPedia();

        [SerializeField]
        private string m_LocalizationTag = "";

        private Text _text;
        
        private void Awake()
        {
            _text = GetComponent<Text>();
        }

        private void OnEnable()
        {
            onLocalize.Invoke(this, m_LocalizationTag);
        }

        public void UpdateText(string text)
        {
            if (_text != null)
                _text.text = text;
        }
    }
}
