using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class ExtraNewsArticleList : AbstractEntity<int>
    {
        public string Code { get; set; } //Mã danh mục
        public string TypeNews { get; set; } //Loại danh mục tin tức 
        public string Note { get; set; } //Ghi chú ngắn 
        public string FileImg { get; set; } //Ảnh
    }
}
