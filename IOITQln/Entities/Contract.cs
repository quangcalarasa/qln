using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class Contract : AbstractEntity<int>
    {
        public string Code { get; set; }        //CCCD
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public int SupportType { get; set; }
        public int State { get; set; } //Trạng thái
        public string Content { get; set; }      //Ngày cấp
        public string File { get; set; }   //Nơi cấp
    }
}
