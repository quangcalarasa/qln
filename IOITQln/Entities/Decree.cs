using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class Decree : AbstractEntity<int>           //Nghị định, thông tư, văn bản
    {
        public TypeDecree TypeDecree { get; set; }
        public string Code { get; set; }
        public DateTime? DoPub { get; set; }        //Ngày ra quyết định
        public string DecisionUnit { get; set; }    //Đơn vị quyết định
        public string Note { get; set; }
        public bool? ApplyCalculateRentalPrice { get; set; }
    }
}
