using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;


namespace IOITQln.Entities
{
    public class ChildDefaultCoefficient : AbstractEntity<int>
    {
        public int RentFileBctId { get; set; }
        public DateTime DoApply { get; set; }
        public TypeTable CoefficientId { get; set; }
        public double Value { get; set; }
        public int UnitPriceId { get; set; }
        public int defaultCoefficientsId { get; set; }
        public byte Type { get; set; } //1:cong no,2:try thu,3:nguoi su dung
    }
}
