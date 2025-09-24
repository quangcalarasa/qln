using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class MemberRentFile : AbstractEntity<int>
    {
        public Guid RentFileId { get; set; } //Id hồ sơ cho thuê
        public string Name { get; set; } //tên TV
        public string Relationship { get; set; } //Mối qhe với chủ hộ
        public string Note { get; set; } //Ghi chú
        public bool  Check { get; set; } 
    }
}
