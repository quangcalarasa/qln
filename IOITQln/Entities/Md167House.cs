

// IOITQln.Entities.Md167House
using System;
using System.ComponentModel.DataAnnotations.Schema;
using IOITQln.Common.Bases;
using IOITQln.Common.Enums;
using IOITQln.Entities;
using Newtonsoft.Json;
namespace IOITQln.Entities
{
    public class Md167House : AbstractEntity<int>
    {
        public class HouseInfo
        {
            public decimal? HouAreaLand
            {
                get;
                set;
            }

            public decimal? TaxNN
            {
                get;
                set;
            }

            public decimal? UseFloorPb
            {
                get;
                set;
            }

            public decimal? UseFloorPr
            {
                get;
                set;
            }

            public decimal? UseLandPb
            {
                get;
                set;
            }

            public decimal? UseLandPr
            {
                get;
                set;
            }

            public decimal? AreBuildPb
            {
                get;
                set;
            }

            public decimal? AreBuildPr
            {
                get;
                set;
            }

            public decimal? AreaLandInSafe
            {
                get;
                set;
            }

            public decimal? AreaLandInBankSafe
            {
                get;
                set;
            }

            public decimal? AreaHouseInSafe
            {
                get;
                set;
            }

            public decimal? AreaHouseInBankSafe
            {
                get;
                set;
            }

            public decimal? AreaFloorBuild
            {
                get;
                set;
            }

            public decimal? AreaTunnel
            {
                get;
                set;
            }

            public int? ApaFloorCount
            {
                get;
                set;
            }

            public decimal? ApaTax
            {
                get;
                set;
            }

            public bool? ApaIsBasement
            {
                get;
                set;
            }

            public decimal? ApaValue
            {
                get;
                set;
            }

            public Kios_Status? KiosStatus
            {
                get;
                set;
            }
        }

        public enum Type_House
        {
            Kios = 3,
            House = 1,
            Apartment = 2
        }

        public enum Kios_Status
        {
            DANG_CHO_THUE = 1,
            CHUA_CHO_THUE
        }

        public enum Status_Of_Use
        {
            DANG_CHO_THUE = 1,
            CHUA_CHO_THUE
        }

        public string Code
        {
            get;
            set;
        }

        public string DocumentNb
        {
            get;
            set;
        }

        public string HouseNumber
        {
            get;
            set;
        }

        public int HouseTypeId
        {
            get;
            set;
        }

        public int ProvinceId
        {
            get;
            set;
        }

        public int DistrictId
        {
            get;
            set;
        }

        public int WardId
        {
            get;
            set;
        }

        public int LaneId
        {
            get;
            set;
        }

        public string MapNumber
        {
            get;
            set;
        }

        public string ParcelNumber
        {
            get;
            set;
        }

        public int? LandTaxRateId
        {
            get;
            set;
        }

        public decimal LandTaxRate
        {
            get;
            set;
        }

        public string PlanningInfor
        {
            get;
            set;
        }

        public int LandId
        {
            get;
            set;
        }

        public int Md167TransferUnitId
        {
            get;
            set;
        }

        public DateTime ReceptionDate
        {
            get;
            set;
        }

        public int Location
        {
            get;
            set;
        }

        public bool IsPayTax
        {
            get;
            set;
        }

        public AppEnums.DecreeEnum decree
        {
            get;
            set;
        }

        public string LocationCoefficient
        {
            get;
            set;
        }

        public decimal LocationCoefficientValue
        {
            get;
            set;
        }

        public string UnitPrice
        {
            get;
            set;
        }

        public decimal UnitPriceValue
        {
            get;
            set;
        }

        public decimal LandPrice
        {
            get;
            set;
        }

        public string SHNNCode
        {
            get;
            set;
        }

        public DateTime SHNNDate
        {
            get;
            set;
        }

        public string ContractCode
        {
            get;
            set;
        }

        public DateTime ContractDate
        {
            get;
            set;
        }

        public string LeaseCode
        {
            get;
            set;
        }

        public DateTime LeaseDate
        {
            get;
            set;
        }

        public string LeaseCertCode
        {
            get;
            set;
        }

        public DateTime LeaseCertDate
        {
            get;
            set;
        }

        public int? Md167HouseId
        {
            get;
            set;
        }

        public int PurposeUsing
        {
            get;
            set;
        }

        public string DocumentCode
        {
            get;
            set;
        }

        public DateTime DocumentDate
        {
            get;
            set;
        }

        public int PlanContent
        {
            get;
            set;
        }

        public decimal OriginPrice
        {
            get;
            set;
        }

        public decimal ValueLand
        {
            get;
            set;
        }

        public string TextureScale
        {
            get;
            set;
        }

        public Type_House TypeHouse
        {
            get;
            set;
        }

        public int StatusOfUse
        {
            get;
            set;
        }

        public int AreaValueId
        {
            get;
            set;
        }

        public int? LandPriceItemId
        {
            get;
            set;
        }

        public int Md167ManagePaymentId
        {
            get;
            set;
        }

        public string Note
        {
            get;
            set;
        }

        public string Info
        {
            get
            {
                if (InfoValue == null)
                {
                    InfoValue = new HouseInfo();
                }
                return JsonConvert.SerializeObject((object)InfoValue);
            }
            set
            {
                InfoValue = JsonConvert.DeserializeObject<HouseInfo>(value) ?? new HouseInfo();
            }
        }

        [NotMapped]
        public HouseInfo InfoValue
        {
            get;
            set;
        } = new HouseInfo();

    }

}

