using IOITQln.Entities;
using System.Collections.Generic;

namespace IOITQln.Models.Data
{
    public class TdcCustomerData : TdcCustomer
    {
        public string fullAddressTT { get; set; }
        public string WardNameTT { get; set; }
        public string DistrictNameTT { get; set; }
        public string ProvinceNameTT { get; set; }
        public string fullAddressLH { get; set; }
        public string WardNameLH { get; set; }
        public string DistrictNameLH { get; set; }
        public string ProvinceNameLH { get; set; }
        public string LaneTt { get; set; }
        public string LaneLh { get; set; }
        public string TdcProjectName { get; set; }
        public string TdcLandName { get; set; }
        public string TdcBlockHouseName { get; set; }
        public string TdcFloorName { get; set; }
        public string TdcApartmentName { get; set; }
        public bool? Corner { get; set; }

        public List<TdcAuthCustomerDetailData> tdcAuthCustomerDetailDatas { get; set; }
        public List<TdcCustomerFile> tdcCustomerFiles { get; set; }

        public List<TdcMenberCustomerData> tdcMenberCustomerDatas { get; set; }
    }

    public class TdcAuthCustomerDetailData : TdcAuthCustomerDetail
    {
        public string fullAddressTt { get; set; }
        public string WardNameTt { get; set; }
        public string DistrictNameTt { get; set; }
        public string ProvinceNameTt { get; set; }
        public string fullAddressLh { get; set; }
        public string WardNameLh { get; set; }
        public string DistrictNameLh { get; set; }
        public string ProvinceNameLh { get; set; }
        public string LaneTT { get; set; }
        public string LaneLH { get; set; }
      
    }

    public class TdcMenberCustomerData : TdcMemberCustomer
    {
        public string fullAddressTt { get; set; }
        public string WardNameTt { get; set; }
        public string DistrictNameTt { get; set; }
        public string ProvinceNameTt { get; set; }
        public string fullAddressLh { get; set; }
        public string WardNameLh { get; set; }
        public string DistrictNameLh { get; set; }
        public string ProvinceNameLh { get; set; }
        public string LaneTT { get; set; }
        public string LaneLH { get; set; }
    }
}
