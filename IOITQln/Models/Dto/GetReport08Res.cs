using System;
using static IOITQln.Entities.Md167House;

namespace IOITQln.Models.Dto
{
    public class GetReport08Res
    {
        public string TransferUnit { get; set; }
        public string CustomertName { get; set; }
        public string HouseCode { get; set; }
        public int District { get; set; } // Quận.
        public string DistrictName { get; set; } // Quận.
        public int Ward { get; set; } // Xã.
        public string WardName { get; set; } // Xã.
        public string LaneName { get; set; } // Đường.
        public int Lane { get; set; } // Đường.
        public decimal? AreaLand { get; set; }
        public decimal? AreaHouse { get; set; }
        public int? FloorCount { get; set; }
        public string Md167ContractCode { get; set; }
        public string TextureScale { get; set; }
        public string Md167ContractDate { get; set; }
        public decimal? PriceOM { get; set; } //số tiền trên tháng
        public  string LeaseTerm  { get; set; }// thời hạn thuê
        public string LeaseCode { get; set; } // Số Quyết thuê đất
        public string LeaseDate { get; set; } // Ngày Quyết định Thuê đất
        public string StatusOfUseName { get; set; }// Hiện trạng sử dụng đất
        public string ContractCode { get; set; } // Số Hợp đồng
        public string ContractDate { get; set; } // Ngày Hợp đồng
        public string LeaseCertCode { get; set; } // Số giấy chứng nhận thuê đất
        public string LeaseCertDate { get; set; } // Ngày tạo giấy chứng nhận thuê đất
        public string DocumentCode { get; set; } //Số văn bản về phương án sắp xếp của UBNDTP
        public string DocumentDate { get; set; } // Ngày văn bản về phương án sắp xếp của UBNDTP
        public string PlanContent { get; set; }//Nội dung phương án được phê duyể cyar UBND TP 
        public string Note { get; set; }

    }
}
