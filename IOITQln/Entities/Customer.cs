using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class Customer : AbstractEntity<int>
    {
        public string Code { get; set; }        //CCCD
        public string FullName { get; set; }
        public DateTime? Dob { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Avatar { get; set; }
        public TypeSex? Sex { get; set; }
        public DateTime? Doc { get; set; }      //Ngày cấp
        public string PlaceCode { get; set; }   //Nơi cấp
    }
}
