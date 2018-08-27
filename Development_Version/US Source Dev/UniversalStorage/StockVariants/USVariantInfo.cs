
namespace UniversalStorage2.StockVariants
{
    public struct USVariantInfo
    {
        private string _variantType;
        private string _displayName;
        private string _primaryColor;
        private string _secondaryColor;

        public string VariantType
        {
            get { return _variantType; }
        }

        public string DisplayName
        {
            get { return _displayName; }
        }

        public string PrimaryColor
        {
            get { return _primaryColor; }
        }

        public string SecondaryColor
        {
            get { return _secondaryColor; }
        }

        public USVariantInfo(string typeName, string name, string primary, string secondary)
        {
            _variantType = typeName;
            _displayName = name;
            _primaryColor = primary;
            _secondaryColor = secondary;
        }
    }
}
