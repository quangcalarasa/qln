using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class TdcMemberCustomer :  AbstractEntity<int>
    {
        public int TdcCustomerId { get; set; }
        public string FullName { get; set; }
        public string CCCD { get; set; }
        public DateTime Dob { get; set; }
        public string Phone { get; set; }
        public string AddressTt { get; set; }
        public string AddressLh { get; set; }
        public int? LaneTt { get; set; }
        public int WardTt { get; set; }
        public int DistrictTt { get; set; }
        public int ProvinceTt { get; set; }
        public int? LaneLh { get; set; }
        public int WardLh { get; set; }
        public int DistrictLh { get; set; }
        public int ProvinceLh { get; set; }
        public string Note { get; set; }
        public string Email { get; set; }
    }
}
