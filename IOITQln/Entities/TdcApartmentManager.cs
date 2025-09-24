using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class TdcApartmentManager: AbstractEntity<int>
    {
        //public string Identifier { get; set; }//mã định danh dự án
        public int? BlockHouseId { get; set; } //mã khối
        public int? LandId { get; set; }//mã lô
        public int? FloorTdcId { get; set; }//mã tầng
        public int? TdcProjectId { get; set; }//mã dự án ->> group 2
        public int? ApartmentTdcId { get; set; }// mã căn
        public int DistrictProjectId { get; set; }//Mã quận/huyện dự án ->> Số thứ tự group 1
        public int TypeDecisionId { get; set; }//mã quyết định
        public int TypeLegalId { get; set; }//mã pháp lý tiếp nhận
        public int TdcApartmentCountRoom { get; set; }// Sl phòng ngủ
        public double TdcApartmentArea { get; set; }// Diện tích căn hộ
        public int Qantity { get; set; }// Số lượng phân bổ theo quy định
        public int ReceiveNumber { get; set; }// Số lượng tiếp nhận theo quy định
        public DateTime? ReceptionDate { get; set; }// Thời gian tiếp nhận
        public TypeReception? ReceptionTime { get; set; }//Chọn tiếp nhận
        public TypeHandover? HandOver { get; set; } // Chọn bàn giao
        public string ReasonReceivedYet { get; set; }// lý do chưa tiếp nhận
        public string Reminded { get; set; }// đã nhắc nhở
        public string ReasonNotReceived { get; set; }//lý do không tiếp nhận
        public string HandOverYear { get; set; }//Năm bàn giao
        public TypeOverYear? OverYear { get; set; }// chọn theo năm bàn giao
        public bool? HandoverPublic { get; set; }//DVCI bàn giao
        public string NoteHandoverPublic { get; set; }//ghi chú theo DVCI bàn giao
        public bool? HandoverCenter { get; set; }//Trung tâm giao
        public string NoteHandoverCenter { get; set; }//ghi chú theo Trung tâm giao
        public bool? HandoverOther { get; set; }// Bàn giao khác
        public string NoteHandoverOther { get; set; }//ghi chú theo bàn giao khác
        public string Note { get; set; }//Ghi chú
    }
}
