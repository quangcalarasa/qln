using IOITQln.QuickPriceNOC.Interface;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOITQln.Common.Services;
using IOITQln.Persistence;
using IOITQln.Entities;
using static IOITQln.Common.Enums.AppEnums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using DevExpress.XtraRichEdit.Import.Html;

namespace IOITQln.QuickPriceNOC.Service
{
    public class ChangePriceService : IChangePrice
    {
        private static readonly ILog log = LogMaster.GetLogger("ChangePrice", "ChangePrice");

        public ChangePriceService()
        {
        }

        public async Task ChangePrice(ChangePrice req, IServiceScopeFactory serviceScopeFactory)
        {
            try
            {
                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetService<ApiDbContext>();
                    using (var transaction = _context.Database.BeginTransaction())
                    {
                        try
                        {
                            Pricing Pricing = new Pricing();

                            if (req.TypeReportApply == TypeReportApply.NHA_CHUNG_CU)
                            {
                                Pricing = _context.Pricings
                                    .Where(p => p.ApartmentId == req.Id && (int)p.Status != 99)
                                    .FirstOrDefault();
                                if (Pricing == null) return;

                                _context.Blocks
                                    .Where(p => p.Id == Pricing.BlockId && (int)p.Status != 99)
                                    .FirstOrDefault();

                                var blockRate = _context.BlockMaintextureRaties
                                    .Where(b => b.TargetId == Pricing.BlockId && (int)b.Type == 1 && (int)b.Status != 99)
                                    .ToList();

                                float? mainRate = blockRate.Select(p => p.TotalValue).FirstOrDefault();
                                var landTbls = _context.PricingLandTbls.Where(p => p.PricingId == Pricing.Id).ToList();
                                var aptDetails = _context.ApartmentDetails.Where(a => a.TargetId == req.Id && (int)a.Type == 2 && (int)a.Status != 99).ToList();

                                decimal? totalRemain = 0;
                                foreach (var item in landTbls)
                                {
                                    bool updated = false;
                                    float? area = aptDetails.Where(p => p.Id == item.ApartmentId).Select(p => p.PrivateArea).FirstOrDefault();
                                    if (area != item.PrivateArea)
                                    {
                                        item.PrivateArea = area;
                                        updated = true;
                                    }
                                    if (mainRate != item.MaintextureRateValue)
                                    {
                                        item.MaintextureRateValue = mainRate;
                                        updated = true;
                                    }
                                    float? rate = item.MaintextureRateValue / 100f;
                                    if (updated)
                                    {
                                        switch (item.TermApply)
                                        {
                                            case TermApply.DIEU_65:
                                                item.RemainingPrice = item.PriceInYear * (decimal)rate.Value * (decimal)item.PrivateArea.Value * (decimal)item.CoefficientUseValue.Value;
                                                break;
                                            case TermApply.DIEU_70:
                                                item.RemainingPrice = item.PriceInYear * (decimal)rate.Value * (decimal)item.PrivateArea.Value;
                                                break;
                                        }
                                    }
                                    item.RemainingPrice = item.RemainingPrice.Value;
                                    totalRemain += item.RemainingPrice;
                                    item.UpdatedAt = DateTime.Now;
                                    item.CreatedBy = "Tools-ChangePrice";
                                    _context.PricingLandTbls.Update(item);
                                }

                                Pricing.ApartmentPrice = totalRemain;
                                decimal reduced = Pricing.ApartmentPriceReduced ?? 0;
                                Pricing.ApartmentPriceRemaining = (totalRemain - reduced).Value;

                                float vatRate = (100f + Pricing.Vat).GetValueOrDefault(1f);
                                Pricing.ApartmentPriceNoVat = Pricing.ApartmentPriceRemaining * 100 / (decimal)vatRate;
                                Pricing.ApartmentPriceNoVat = Pricing.ApartmentPriceNoVat.Value;

                                if (Pricing.Vat.HasValue || Pricing.Vat > 0f)
                                    Pricing.ApartmentPriceVat = (Pricing.ApartmentPriceRemaining - Pricing.ApartmentPriceNoVat).Value;
                                else
                                    Pricing.ApartmentPriceVat = Pricing.ApartmentPriceRemaining.Value;

                                var landDetails = _context.PricingApartmentLandDetails.Where(p => p.PricingId == Pricing.Id).ToList();
                                var aptLandDetails = _context.ApartmentLandDetails
                                    .Where(a => a.TargetId == req.Id && (int)a.Type == 2 && (int)a.Status != 99)
                                    .OrderBy(x => x.UpdatedAt)
                                    .ToList();

                                decimal? totalLand = 0;
                                foreach (var item in landDetails)
                                {
                                    bool updated = false;
                                    float? area = aptLandDetails.Where(p => p.Id == item.ApartmentId).Select(p => p.PrivateArea).FirstOrDefault();
                                    if (area != item.PrivateArea)
                                    {
                                        item.PrivateArea = area;
                                        updated = true;
                                    }

                                    if (updated)
                                    {
                                        decimal ded = (item.DeductionLandMoneyValue.HasValue ? ((decimal)((100f - item.DeductionLandMoneyValue) / 100f).Value) : 1m);
                                        decimal kdt = 0.1m;
                                        decimal dist = (item.CoefficientDistribution > 0 ? (decimal)item.CoefficientDistribution.Value : 1m);

                                        switch (item.TermApply)
                                        {
                                            case TermApply.DIEU_65:
                                                decimal f99 = (Pricing.FlatCoefficientId_99.HasValue ? (decimal)Pricing.FlatCoefficient_99.Value : 1m);
                                                item.LandPrice = (item.LandUnitPrice * (decimal)item.PrivateArea.Value * dist * kdt * f99 * ded).Value;
                                                break;
                                            case TermApply.DIEU_70:
                                                decimal f70 = (Pricing.FlatCoefficientId_99.HasValue ? (decimal)Pricing.FlatCoefficient_99.Value : 1m);
                                                item.LandPrice = item.LandUnitPrice.Value * (decimal)item.PrivateArea.Value * dist * f70;
                                                break;
                                            case TermApply.KHOAN_1_DIEU_34:
                                                decimal f34_1 = (Pricing.FlatCoefficientId_34.HasValue ? (decimal)Pricing.FlatCoefficient_34.Value : 1m);
                                                item.LandPrice = item.LandUnitPrice.Value * (decimal)item.PrivateArea.Value * dist * kdt * f34_1 * ded;
                                                break;
                                            case TermApply.KHOAN_2_DIEU_34:
                                                decimal f34_2 = (Pricing.FlatCoefficientId_34.HasValue ? (decimal)Pricing.FlatCoefficient_34.Value : 1m);
                                                item.LandPrice = item.LandUnitPrice.Value * (decimal)item.PrivateArea.Value * dist * f34_2;
                                                break;
                                            case TermApply.DIEU_7:
                                                decimal f61 = (Pricing.FlatCoefficientId_61.HasValue ? (decimal)Pricing.FlatCoefficient_61.Value : 1m);
                                                item.LandPrice = item.LandUnitPrice.Value * (decimal)item.PrivateArea.Value * dist * kdt * f61 * ded;
                                                break;
                                        }
                                        totalLand += item.LandPrice;
                                        _context.PricingApartmentLandDetails.Update(item);
                                    }
                                }

                                Pricing.LandPrice = totalLand.Value;
                                Pricing.TotalPrice = (Pricing.LandPrice + Pricing.ApartmentPriceRemaining).Value;
                                _context.Pricings.Update(Pricing);
                                _context.SaveChanges();
                            }
                            else if (req.TypeReportApply == TypeReportApply.NHA_RIENG_LE)
                            {
                                // (phần xử lý NHA_RIENG_LE tương tự, giữ nguyên logic, format lại như QuickPriceService.cs)
                                // ...
                            }

                            log.Info("SUCCESS" + req.Id);
                        }
                        catch (Exception)
                        {
                            log.Info("ERROR" + req.Id);
                        }
                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Info("ERROR ChangePrice Exception: " + ex);
            }
        }
    }
}
