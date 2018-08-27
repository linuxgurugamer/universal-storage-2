using UnityEngine.EventSystems;

namespace UniversalStorage2.StockVariants
{
    public class UI_USPartActionVariantButton : UIPartActionVariantButton, IPointerEnterHandler, IPointerExitHandler
    {
        private int _index;
        private UI_USPartActionVariantSelector _usSelector;

        public void USSetup(UI_USPartActionVariantSelector selector, int index, string primaryColor, string secondaryColor)
        {
            base.Setup(selector, index, primaryColor, secondaryColor);

            _index = index;
            _usSelector = selector;

            buttonMain.onClick.RemoveAllListeners();

            buttonMain.onClick.AddListener(new UnityEngine.Events.UnityAction(ButtonPressed));

            //USdebugMessages.USStaticLog("<color={0}>Variant selector</color> <color={1}>button added</color>: {2}", primaryColor, secondaryColor, index);
        }

        public void ButtonPressed()
        {
            _usSelector.ButtonPressed(_index);
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData data)
        {
            _usSelector.SetNameText(_index);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData data)
        {
            _usSelector.ResetNameText();
        }
    }
}
