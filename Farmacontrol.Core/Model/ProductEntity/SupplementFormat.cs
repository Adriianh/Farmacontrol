using System.ComponentModel;

namespace Farmacontrol.Model.ProductEntity
{
    public enum SupplementFormat
    {
        [Description("Cápsula")]
        Capsule,
        
        [Description("Tableta")]
        Tablet,
        
        [Description("Polvo")]
        Powder,
        
        [Description("Líquido")]
        Liquid,
        
        [Description("Gomita")]
        Gummy,
        
        [Description("Barra")]
        Bar,
        
        [Description("Otro")]
        Other
    }
}
