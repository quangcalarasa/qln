using IOITQln.Common.Bases;
using System;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Entities
{
    public class ExtraNewsArticle : AbstractEntity<int>
    {
        public string ArticleTitle { get; set; } //Tiêu đề bài viết
        public int ExtraNewsArticleListId { get; set; } //Loại danh mục tin tức 
        public string ShortNote { get; set; } //Ghi chú ngắn 
        public string Files { get; set; } //Tên file 
        public string Content { get; set; } //Nội dung bài viết
        public DateTime DateUpdate { get; set; } //Ngày cập nhập
    }
}
