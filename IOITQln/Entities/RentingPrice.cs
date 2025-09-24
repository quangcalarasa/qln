using System;
using IOITQln.Common.Bases;
using static IOITQln.Common.Enums.AppEnums;


namespace IOITQln.Entities
{
    public class RentingPrice : AbstractEntity<int>
    {
        public TypeQD TypeQD { get; set; }
        public int? TypeBlockId { get; set; } //Loại nhà
        public int? LevelId { get; set; } // cấp nhà
        public int UnitPriceId { get; set; }
        public double? Price { get; set; }
        public DateTime? EffectiveDate { get; set; }      //Ngày hiệu lực
        public string Note { get; set; }
    }
}
