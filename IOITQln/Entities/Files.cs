using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class Files  : AbstractEntity<int>
    {
        public FileStatus TypeFile { get; set; }  //Trạng thái
        public TypeReportApply TypeReportApply { get; set; } //Loại biên bản áp dụng
        public int TypeBlockId { get; set; } //loại nhà
        public string  CodeFile { get; set; } //Mã hồ sơ
        public DateTime Date { get; set; } //Ngày tạo
        public int BlockId { get; set; } //Id căn nhà
        public string fullAddress { get; set; } //Dc căn nhà
        public float UseAreaValue { get; set; } //Tổng dt sử dụng
        public string Note { get; set; } //Diễn giải
    }
}
