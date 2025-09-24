using DevExpress.Internal.WinApi.Windows.UI.Notifications;
using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class ChilDfTable : AbstractEntity<int>
    {
        public Guid RentBctTableId { get; set; }
        public int CoefficientId { get; set; }
        public decimal? Value { get; set; }  
        public string Code { get; set; }
        public int Index { get; set; }  
    }
}
