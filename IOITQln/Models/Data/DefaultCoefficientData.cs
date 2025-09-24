using DevExpress.Office.Utils;
using IOITQln.Entities;
using System.Collections.Generic;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Models.Data
{
    public class DefaultCoefficientData : DefaultCoefficient
    {
        public string UnitPriceName { get; set; }
        public List<GroupData> groupDataDf { get; set; }
        public string Coefficient_Name { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ

    }

    public class GroupData
    {
        public TypeTable CoefficientId { get; set; }
        public List<DefaultCoefficient> defaultCoefficients { get; set; }
        public List<ChildDefaultCoefficient> childDefaultCoefficients { get; set; }
    }

}
