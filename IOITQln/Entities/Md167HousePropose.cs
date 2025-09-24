using DevExpress.ReportServer.ServiceModel.DataContracts;
using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class Md167HousePropose : AbstractEntity<int>
    {
        public int Md167HouseId { get; set; }
        public DateTime Date { get; set; }
        public string ProposeOption { get; set; }
        public string BrowseStatus { get; set; }
        public string AuthLetter { get; set; }
        public DateTime BrowseDate { get; set; }
        public string Note { get; set; }
    }
}
