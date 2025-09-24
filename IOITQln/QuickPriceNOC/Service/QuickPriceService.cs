using IOITQln.QuickPriceNOC.Interface;
using FcmSharp;
using FcmSharp.Requests;
using FcmSharp.Settings;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IOITQln.Common.Services;
using IOITQln.Persistence;
using IOITQln.Entities;
using NPOI.HPSF;
using static IOITQln.Common.Enums.AppEnums;
using NPOI.OpenXmlFormats.Shared;
using DevExpress.CodeParser;
using Microsoft.Data.SqlClient;
using DevExpress.Xpo.DB.Helpers;
using static DevExpress.XtraEditors.Filtering.DataItemsExtension;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using DevExpress.XtraRichEdit.Import.Html;

namespace IOITQln.QuickPriceNOC.Service
{
    public class QuickPriceService : IQuickPrice
    {
        private static readonly ILog log = LogMaster.GetLogger("QuickPriceService", "QuickPriceService");
        public QuickPriceService()
        {
        }

        public async Task QuickPrice(QuickPriceReq req, IServiceScopeFactory serviceScopeFactory)
        {
            try
            {
                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetService<ApiDbContext>();
                    using (var transaction = _context.Database.BeginTransaction())
                    {
                        //Guid id = new Guid("9747AB2F-A08C-4A59-8759-2D651EB9D336");
                        var dataRentFile = _context.RentFiles.Where(l => l.Status != Common.Enums.AppEnums.EntityStatus.DELETED && (l.DateHD.AddMonths(l.Month) > req.DoApply && l.DateHD < req.DoApply) || (l.DateHD > req.DoApply)).ToList();
                        //var dataRentFile = _context.RentFiles.Where(l => l.Status != Common.Enums.AppEnums.EntityStatus.DELETED && l.Id == id).ToList();

                        var dataRentFileTable = _context.RentBctTables.Where(l => l.Status != Common.Enums.AppEnums.EntityStatus.DELETED).ToList();

                        var dataDebtsTable = _context.DebtsTables.AsNoTracking().Where(l => l.Status != Common.Enums.AppEnums.EntityStatus.DELETED).ToList();
                        var dataDebts = _context.debts.Where(l => l.Status != Common.Enums.AppEnums.EntityStatus.DELETED).ToList();

                        foreach (var item in dataRentFile)
                        {
                            List<RentBctTable> res = new List<RentBctTable>();

                            var dataQ = dataRentFileTable.Where(l => l.RentFileId == item.Id).OrderBy(l => l.Index).GroupBy(p => p.Type).Select(e => new
                            {
                                key = e.Key,
                                date = e.ToList().First().DateStart,
                                data = e.ToList(),
                            }).ToList();

                            if (dataQ.Count > 0)
                            {
                                var rq = dataQ.Last().data;
                                List<RentBctTable> listUpdate = new List<RentBctTable>();
                                List<DebtsTable> lstUpdateDebts = new List<DebtsTable>();
                                var datatemp = new RentBctTable();
                                var dataTempDebts = new Debts();
                                var dataTempDebtsTable = new DebtsTable();
                                dataQ.Last().data.ForEach(itemC =>
                                {
                                    datatemp = itemC;
                                    var x = itemC.DateStart;
                                    var y = dataQ.First().date.Value.AddMonths(item.Month);

                                    if (itemC.DateStart < req.DoApply && req.DoApply < y)
                                    {
                                     
                                        datatemp.Id = itemC.Id;
                                        datatemp.DateEnd = req.DoApply.AddDays(-1);
                                        datatemp.UpdatedAt = DateTime.Now;
                                        listUpdate.Add(datatemp);
                                    }
                                });

                                _context.UpdateRange(listUpdate);
                                _context.SaveChanges();

                                decimal sum = 0;
                                int Index = 0;
                                int Type = 0;
                                for (int r = 0; r < dataQ.Last().data.Count - 1; r++)
                                {
                                    decimal oldValue = 0;
                                    RentBctTable data = dataQ.Last().data[r];
                                    data.DateStart = req.DoApply;
                                    var montold = dataQ.First().date.Value;
                                    var month = 60 - UtilsService.MothDiff(montold, req.DoApply);
                                    int ExcessDay = UtilsService.ExcessDay(montold, montold.AddMonths(item.Month), month);
                                    if (ExcessDay < 0)
                                    {
                                        ExcessDay = 30 + ExcessDay;
                                        month--;
                                    }
                                    data.DateEnd = data.DateStart.Value.AddMonths(month);
                                    data.DateEnd = data.DateEnd.Value.AddDays(ExcessDay);
                                    data.Index = dataQ.Last().data.Last().Index + r + 1;
                                    data.Type = dataQ.Last().data.Last().Type + r + 1;
                                    Guid itData = Guid.NewGuid();
                                    data.Id = itData;
                                    var OldKlcb = dataQ.Last().data[r].Ktlcb;
                                    var OldVAT = dataQ.Last().data[r].VAT != null ? dataQ.Last().data[r].VAT : 0;
                                    var OldStandardPrice = dataQ.Last().data[r].StandardPrice != null ? dataQ.Last().data[r].StandardPrice : 0;
                                    var OldKtdbt = dataQ.Last().data[r].Ktdbt != null ? dataQ.Last().data[r].Ktdbt : 0;
                                    switch (req.Type)
                                    {
                                        case 1:
                                            data.VAT = (double?)req.Value;
                                            if (OldVAT == 0)
                                            {
                                                data.PriceRent = dataQ.Last().data[r].PriceRent * req.Value / 1;
                                            }
                                            else
                                            {
                                                data.PriceRent = dataQ.Last().data[r].PriceRent * req.Value / (decimal)OldVAT;
                                            }
                                            break;
                                        case 2:
                                            data.Ktlcb = req.Value;
                                            if (OldKlcb == 0)
                                            {
                                                data.PriceRent = dataQ.Last().data[r].PriceRent * req.Value / 1;
                                            }
                                            else
                                            {
                                                data.PriceRent = dataQ.Last().data[r].PriceRent * req.Value / OldKlcb;
                                            }
                                            break;
                                        case 3:
                                            data.StandardPrice = req.Value;
                                            if (OldStandardPrice == 0)
                                            {
                                                data.PriceRent = dataQ.Last().data[r].PriceRent * req.Value / 1;
                                            }
                                            else
                                            {
                                                data.PriceRent = dataQ.Last().data[r].PriceRent * req.Value / OldStandardPrice;
                                            }
                                            break;
                                        case 4:
                                            data.Ktdbt = req.Value;
                                            if (OldKtdbt == 0)
                                            {
                                                data.PriceRent = dataQ.Last().data[r].PriceRent * req.Value / 1;
                                            }
                                            else
                                            {
                                                data.PriceRent = dataQ.Last().data[r].PriceRent * req.Value / OldKtdbt;
                                            }
                                            break;
                                    }
                                    var discountCoff = data.DiscountCoff != null ? data.DiscountCoff : 0;
                                    data.PriceAfterDiscount = data.PriceRent - discountCoff;

                                    sum += (decimal)data.PriceAfterDiscount;
                                    Index = data.Index;
                                    Type = (int)data.Type;
                                    res.Add(data);
                                    decimal totalPrice = 0;
                                    totalPrice = dataDebtsTable.Where(l => l.RentFileId == item.Id && l.DateStart < req.DoApply).Sum(p => p.Price); //Tính tổng tiền các kì trước ngày thay đổi

                                    //Cập nhập lại số tiền các kì sau ngày thay đổi
                                    var dataChangeDebts = dataDebtsTable.Where(l => l.RentFileId == item.Id && req.DoApply < l.DateStart).ToList();
                                    dataChangeDebts.ForEach(d =>
                                    {
                                        d.Price = (decimal)data.PriceAfterDiscount;
                                        d.PriceDiff = (decimal)data.PriceAfterDiscount;
                                        double? v = (1 + data.VAT / 100);
                                        d.VATPrice = (double)((d.Price / (decimal)v) * (decimal)(d.VAT / 100));
                                        lstUpdateDebts.Add(d);
                                    });
                                    _context.DebtsTables.UpdateRange(lstUpdateDebts);


                                    //Update lại tổng tiền công nợ và tiền nợ 
                                    var Debts = dataDebts.Where(l => l.RentFileId == item.Id).FirstOrDefault();
                                    if (Debts != null)
                                    {
                                        decimal newTotal = 0;
                                        newTotal = totalPrice + lstUpdateDebts.Sum(p => p.Price);

                                        Debts.Total = totalPrice + newTotal;
                                        if (Debts.Paid == null) Debts.Paid = 0;
                                        Debts.Diff = totalPrice + newTotal - Debts.Paid;

                                        _context.debts.UpdateRange(Debts);
                                    }
                                }
                                RentBctTable dataTotal = new RentBctTable();
                                Guid idTotal = Guid.NewGuid();
                                dataTotal.Id = idTotal;
                                dataTotal.AreaName = "Tổng";
                                dataTotal.PriceAfterDiscount = sum;

                                dataTotal.Index = Index + 1;
                                dataTotal.Type = Type;
                                dataTotal.RentFileId = item.Id;

                                res.Add(dataTotal);
                                _context.RentBctTables.AddRange(res);
                                _context.SaveChanges();
                                try
                                {
                                    QuickMathLog qml = new QuickMathLog();
                                    qml.CodeHD = item.Code;
                                    qml.StatusProcess = 2;
                                    qml.QuickMathHistoryId = req.QuickMathHistoryId;
                                    _context.QuickMathLogs.Add(qml);
                                    _context.SaveChanges();
                                    log.Info("SUCCESS" + item.Id);
                                }
                                catch (Exception e)
                                {
                                    QuickMathLog qml = new QuickMathLog();
                                    qml.CodeHD = item.Code;
                                    qml.StatusProcess = 1;
                                    qml.QuickMathHistoryId = req.QuickMathHistoryId;
                                    _context.QuickMathLogs.Add(qml);
                                    _context.SaveChanges();
                                    transaction.Rollback();
                                    log.Info("ERROR" + item.Id);
                                }
                            }
                        }
                        transaction.Commit();
                    }

                    QuickMathHistory updateQM = _context.QuickMathHistories.Find(req.QuickMathHistoryId);
                    updateQM.StatusProcess = 2; // là hoàn thành
                    _context.QuickMathHistories.Update(updateQM);
                    _context.SaveChanges();

                }
            }
            catch (Exception ex)
            {
                log.Info("QuickPrice Exception: " + ex);
            }
        }
    }
}
