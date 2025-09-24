using IOITQln.Common.Enums;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace IOITQln.QuickPriceNOC.Interface
{
    public interface IChangePrice
    {
        Task ChangePrice(ChangePrice req, IServiceScopeFactory serviceScopeFactory);
    }
    public class ChangePrice
    {
        public int Id
        {
            get;
            set;
        }

        public AppEnums.TypeReportApply TypeReportApply
        {
            get;
            set;
        }

        public decimal? newPrivateArea
        {
            get;
            set;
        }

        public decimal newMaintextureRateValue
        {
            get;
            set;
        }
    }
}
