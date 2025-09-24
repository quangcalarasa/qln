using IOITQln.Common.Bases;
using System;

namespace IOITQln.Entities
{
    public class TdcPriceRentOfficial : AbstractEntity<int>
    {
        public int TdcPriceRentId { get; set; }
        public int IngredientsPriceId { get; set; }
        public decimal Area { get; set; } // Diện Tích
        public decimal Price { get; set; } //Dơn Giá
        public decimal Total { get; set; } //Thành Tiền
        public int ChangeTimes { get; set; } //Lần sửa

    }
}
