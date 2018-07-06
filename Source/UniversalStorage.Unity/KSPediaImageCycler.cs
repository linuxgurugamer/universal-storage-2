
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UniversalStorage.Unity
{
    [RequireComponent(typeof(Image))]
    public class KSPediaImageCycler : MonoBehaviour
    {
        [SerializeField]
        private Sprite[] m_Images = null;
        [SerializeField]
        private float m_Delay = 3;

        private Image _image;
        private Coroutine _cycler;
        private int _counter;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            if (_cycler != null)
                StopCoroutine(_cycler);

            _cycler = StartCoroutine(CycleImages());
        }

        private void OnDisable()
        {
            if (_cycler != null)
                StopCoroutine(_cycler);

            _cycler = null;
        }

        private IEnumerator CycleImages()
        {
            WaitForSeconds wait = new WaitForSeconds(m_Delay);

            while (gameObject.activeInHierarchy)
            {
                yield return wait;

                _counter++;

                if (_counter >= m_Images.Length)
                    _counter = 0;

                _image.sprite = m_Images[_counter];
            }

            _cycler = null;
        }
    }
}
