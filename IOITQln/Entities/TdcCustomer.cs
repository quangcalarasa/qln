using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class TdcCustomer : AbstractEntity<int>
    {
        
        public string Code { get; set; }
        public string FullName { get; set; }
        public DateTime Dob { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string AddressTT { get; set; }
        public string AddressLH { get; set; }
        public int? LaneTT { get; set; }
        public int WardTT { get; set; }
        public int DistrictTT { get; set; }
        public int ProvinceTT { get; set; }
        public int? LaneLH { get; set; }
        public int WardLH { get; set; }
        public int DistrictLH { get; set; }
        public int ProvinceLH { get; set; }
        public string Note { get; set; }
        public string CCCD { get; set; }
        public int TdcProjectId { get; set; }
        public int LandId { get; set; }
        public int BlockHouseId { get; set; }
        public int FloorTdcId { get; set; }
        public int TdcApartmentId { get; set; }
        public string Floor1 { get; set; }

    }
}
