using IOITQln.Entities;
using System.Collections.Generic;

namespace IOITQln.Models.Data
{
    public class TypeAttributeData : TypeAttribute
    {
        public List<TypeAttributeItem> typeAttributeItems { get; set; }
    }
}
