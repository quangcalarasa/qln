using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class TdcPlatformManager : AbstractEntity<int>
    {
        //public string Identifier { get; set; } //mã định danh dự án danh sách nền
        public int? LandId { get; set; }//mã lô
        public int? TdcProjectId { get; set; }//mã dự án
        public int? PlatformTdcId { get; set; }// mã căn
        public int DistrictProjectId { get; set; }//Mã quận/huyện dự án
        public int TypeDecisionId { get; set; }//mã quyết định
        public int TypeLegalId { get; set; }//mã pháp lý tiếp nhận
        public double TdcLength { get; set; }// Chiều dài nền đất
        public double TdcWidth { get; set; }//Chiều rộng nền đất
        public decimal TdcPlatformArea { get; set; }// Diện tích nền đất
        public int PlatCount { get; set; }// Số nền đất
        public int Qantity { get; set; }// Số lượng phân bổ theo quy định
        public int ReceiveNumber { get; set; }// Số lượng tiếp nhận theo quy định
        public DateTime ReceptionDate { get; set; }// Thời gian tiếp nhận
        public TypeReception? ReceptionTime { get; set; }//Chọn tiếp nhận
        public TypeHandover? HandOver { get; set; } // Chọn bàn giao
        public string ReasonReceivedYet { get; set; }// lý do chưa tiếp nhận
        public string Reminded { get; set; }// đã nhắc nhở
        public string ReasonNotReceived { get; set; }//lý do không tiếp nhận
        public string HandOverYear { get; set; }//Năm bàn giao
        public bool? HandoverPublic { get; set; }//DVCI bàn giao
        public string NoteHandoverPublic { get; set; }//ghi chú theo DVCI bàn giao
        public bool? HandoverCenter { get; set; }//Trung tâm giao
        public string NoteHandoverCenter { get; set; }//ghi chú theo Trung tâm giao
        public bool? HandoverOther { get; set; }//Bàn giao khác
        public string NoteHandoverOther { get; set; }//ghi chú theo bàn giao khác
        public string Note { get; set; }//Ghi chú
    }
}
