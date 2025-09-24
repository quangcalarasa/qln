using IOITQln.Entities;
using System.Collections.Generic;

namespace IOITQln.Models.Data
{
    public class RentFlieData : RentFile
    {
        public string Address { get; set; } //Dc căn hộ
        public List<MemberRentFile> memberRentFiles { get; set; }
        public List<RentFile> ListAddendums { get; set; }
        public EditHistory editHistory { get; set; }
    }

    public class groupDataByCode
    {
        public string Code { get; set; }
        public List<RentFile> GroupByCode { get; set; }
    }
}
