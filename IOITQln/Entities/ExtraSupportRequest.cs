using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class ExtraSupportRequest : AbstractEntity<int>
    {
        public string Code { get; set; } // ma dinh danh
        public string House { get; set; } // căn nhà
        public string Apartment { get; set; } // căn hộ
        public string RequirePerson { get; set; } // người yêu cầu hỗ trợ
        public string TypeSupport { get; set; } //loại yêu cầu hỗ trợ
        public TypePersonSupport TypePersonSupportName { get; set; } //người tiếp nhận yêu cầu hỗ trợ
        public TypeSupportReq TypeSupportReq { get; set; } //Trạng thái yêu cầu hỗ trợ
        public string Title { get; set; } // tiêu đề
        public string Content { get; set; } // nội dung
        public DateTime ToDate { get; set; } //Ngày yêu cầu
    }
}
