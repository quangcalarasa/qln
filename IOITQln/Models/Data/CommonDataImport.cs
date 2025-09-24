using IOITQln.Entities;

namespace IOITQln.Models.Data
{
    public class CommonDataImport
    {
    }

    public class DepartmentDataImport : Department
    {
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class PositionDataImport : Position
    {
        public string DepartmentName { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }

    public class LaneDataImport : Lane
    {
        public string WardName { get; set; }
        public string DistrictName { get; set; }
        public string ProvinceName { get; set; }
        public int? Index { get; set; }//số thứ tự
        public bool Valid { get; set; } //kiểm tra xem có hợp lệ không
        public string ErrMsg { get; set; }//Thông báo khi không hợp lệ
    }
}
