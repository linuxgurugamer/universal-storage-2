
using System.Collections.Generic;
using KSP.Localization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UniversalStorage.StockVariants
{
    [UI_USVariantSelector]
    public class UI_USPartActionVariantSelector : UIPartActionVariantSelector
    {
        private int _currentSelection;
        private UI_USVariantSelector _variantSelector;

        private TextMeshProUGUI _variantLabel;

        private List<UI_USPartActionVariantButton> _variantButtons = new List<UI_USPartActionVariantButton>();

        public override void Setup(UIPartActionWindow window, Part part, PartModule partModule, UI_Scene scene, UI_Control control, BaseField field)
        {
            //base.Setup(window, part, partModule, scene, control, field);

            base.SetupItem(window, part, partModule, scene, control);

            this.field = field;

            buttonNext.onClick.AddListener(new UnityEngine.Events.UnityAction(NextButton));
            buttonPrevious.onClick.AddListener(new UnityEngine.Events.UnityAction(PrevButton));
            
            _currentSelection = field.GetValue<int>(field.host);

            _variantSelector = (UI_USVariantSelector)control;

            _variantLabel = GetComponentsInChildren<TextMeshProUGUI>()[0];

            //USdebugMessages.USStaticLog("Setting up US field control...");

            if (_variantSelector.Variants != null && _variantSelector.Variants.Count > 0)
                AddVariants();
        }

        private void AddVariants()
        {
            for (int i = 0; i < _variantSelector.Variants.Count; i++)
            {
                USVariantInfo info = _variantSelector.Variants[i];

                AddVariant(i, info.PrimaryColor, info.SecondaryColor);
            }

            _variantLabel.text = _variantSelector.Variants[0].VariantType + ":";

            _variantLabel.enableAutoSizing = false;
            _variantLabel.fontSize = 11.15f;

            float addedWidth = _variantLabel.preferredWidth - 40 + 4;

            if (addedWidth < 0)
                addedWidth = 0;
            
            _variantLabel.rectTransform.sizeDelta = new Vector2(_variantLabel.rectTransform.sizeDelta.x + addedWidth, _variantLabel.rectTransform.sizeDelta.y);
            _variantLabel.rectTransform.anchoredPosition = new Vector2(_variantLabel.rectTransform.anchoredPosition.x + (addedWidth / 2), _variantLabel.rectTransform.anchoredPosition.y);

            variantName.rectTransform.sizeDelta = new Vector2(variantName.rectTransform.sizeDelta.x - addedWidth, variantName.rectTransform.sizeDelta.y);
            variantName.rectTransform.anchoredPosition = new Vector2(variantName.rectTransform.anchoredPosition.x + (addedWidth / 2), variantName.rectTransform.anchoredPosition.y);

            _variantButtons[_currentSelection].Select();

            SetText(_variantSelector.Variants[_currentSelection].DisplayName);
        }

        private void AddVariant(int index, string primaryColor, string secondaryColor)
        {
            GameObject button = Instantiate(prefabVariantButton, scrollMain.content);

            UIPartActionVariantButton stockButton = button.GetComponent<UIPartActionVariantButton>();
            
            Image invalid = stockButton.imageInvalid;
            Image primary = stockButton.imagePrimaryColor;
            Image secondary = stockButton.imageSecomdaryColor;
            Image selected = stockButton.imageSelected;
            Button main = stockButton.buttonMain;

            DestroyImmediate(stockButton);

            UI_USPartActionVariantButton usButton = button.AddComponent<UI_USPartActionVariantButton>();

            usButton.imageInvalid = invalid;
            usButton.imagePrimaryColor = primary;
            usButton.imageSecomdaryColor = secondary;
            usButton.imageSelected = selected;
            usButton.buttonMain = main;
            
            usButton.USSetup(this, index, primaryColor, secondaryColor);
            
            _variantButtons.Add(usButton);
        }

        public override void UpdateItem()
        {
            transform.SetAsLastSibling();
        }

        public void NextButton()
        {
            SetVariant(_currentSelection + 1);
        }

        public void PrevButton()
        {
            SetVariant(_currentSelection - 1);
        }

        public void ButtonPressed(int selection)
        {
            SetVariant(selection);
        }

        public void SetNameText(int selection)
        {
            if (selection == _currentSelection)
                return;
            
            SetText(string.Format("<color=yellow><b>{0}</b></color>", _variantSelector.Variants[selection].DisplayName));
        }

        public void ResetNameText()
        {
            SetText(_variantSelector.Variants[_currentSelection].DisplayName);
        }

        private void SetText(string text)
        {
            variantName.text = text;
        }

        private void SetVariant(int index)
        {
            if (_variantSelector.Variants == null)
                return;

            if (index >= _variantSelector.Variants.Count)
                index = 0;
            else if (index < 0)
                index = _variantSelector.Variants.Count - 1;

            //USdebugMessages.USStaticLog("Select variant from UI: {0}", index);

            if (_variantButtons.Count > index && _variantButtons.Count > _currentSelection)
            {
                _variantButtons[index].Select();
                _variantButtons[_currentSelection].Reset();
            }

            SetText(_variantSelector.Variants[index].DisplayName);
            _currentSelection = index;

            SetFieldValue(_currentSelection);
        }

    }
}
