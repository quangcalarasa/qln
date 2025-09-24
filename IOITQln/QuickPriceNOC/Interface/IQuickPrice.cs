using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOITQln.QuickPriceNOC.Interface
{
    public interface IQuickPrice
    {
        Task QuickPrice(QuickPriceReq  req, IServiceScopeFactory serviceScopeFactory);
    }
    public class QuickPriceReq
    {
        public DateTime DoApply { get; set; } //Ngày áp dụng
        public decimal Value { get; set; } //Giá trị thay đổi
        public string TypeValue { get; set; }  //Loại giá trị thay đổi
        public int Type { get; set; } //Kiểu giá trị thay đổi 1-VAT,2-LCB,3-Giá chuẩn,4-thời điểm bố trí
        public int QuickMathHistoryId { get; set; } //Id 
    }


}
