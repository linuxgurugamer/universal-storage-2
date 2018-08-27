using System;
using System.Collections.Generic;

namespace UniversalStorage2.StockVariants
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
    public class UI_USVariantSelector : UI_Control
    {
        public List<USVariantInfo> Variants = new List<USVariantInfo>();
    }
}
