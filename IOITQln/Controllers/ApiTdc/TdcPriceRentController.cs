using IOITQln.Common.Constants;
using IOITQln.Common.Enums;
using IOITQln.Common.Services;
using IOITQln.Common.ViewModels.Common;
using IOITQln.Entities;
using IOITQln.Models.Data;
using IOITQln.Models.Dto;
using IOITQln.Persistence;
using AutoMapper;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using static IOITQln.Common.Enums.AppEnums;
using Microsoft.AspNetCore.Hosting;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using NPOI.SS.Util;
using System.Data;
using Microsoft.Net.Http.Headers;

namespace IOITQln.Controllers.ApiTdc
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TdcPriceRentController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("TdcPriceRent", "TdcPriceRent");
        private static string functionCode = "PriceRent_TDC";
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        private IHostingEnvironment _hostingEnvironment;

        public TdcPriceRentController(ApiDbContext context, IMapper mapper, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
        }
        [HttpGet("GetByPage")]
        public IActionResult GetByPage([FromQuery] FilteredPagination paging)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            DefaultResponse def = new DefaultResponse();

            var identity = (ClaimsIdentity)User.Identity;
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }
            try
            {
                if (paging != null)
                {
                    def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                    IQueryable<TdcPriceRent> data = _context.TdcPriceRents.Where(c => c.Status != AppEnums.EntityStatus.DELETED);
                    if (paging.query != null)
                    {
                        paging.query = HttpUtility.UrlDecode(paging.query);
                    }
                    data = data.Where(paging.query);
                    def.metadata = data.Count();

                    if (paging.page_size > 0)
                    {
                        if (paging.order_by != null)
                        {
                            data = data.OrderBy(paging.order_by).Skip((paging.page - 1) * paging.page_size).Take(paging.page_size);
                        }
                        else
                        {
                            data = data.OrderBy("Id desc").Skip((paging.page - 1) * paging.page_size).Take(paging.page_size);
                        }
                    }
                    else
                    {
                        if (paging.order_by != null)
                        {
                            data = data.OrderBy(paging.order_by);
                        }
                        else
                        {
                            data = data.OrderBy("Id desc");
                        }
                    }

                    if (paging.select != null && paging.select != "")
                    {
                        paging.select = "new(" + paging.select + ")";
                        paging.select = HttpUtility.UrlDecode(paging.select);
                        def.data = data.Select(paging.select);
                    }
                    else
                    {
                        List<TdcPriceRentData> res = _mapper.Map<List<TdcPriceRentData>>(data.ToList());
                        foreach (TdcPriceRentData item in res)
                        {
                            item.TdcCustomerName = _context.TdcCustomers.Where(f => f.Id == item.TdcCustomerId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.FullName).FirstOrDefault();

                            item.TdcProjectName = _context.TDCProjects.Where(f => f.Id == item.TdcProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            item.TdcLandName = _context.Lands.Where(f => f.Id == item.LandId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            item.TdcBlockHouseName = _context.BlockHouses.Where(f => f.Id == item.BlockHouseId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            item.TdcFloorName = _context.FloorTdcs.Where(f => f.Id == item.FloorTdcId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            ApartmentTdc apartmentTdc = _context.ApartmentTdcs.Where(f => f.Id == item.TdcApartmentId && f.Status != AppEnums.EntityStatus.DELETED)
                                .Select(s => new ApartmentTdc()
                                {
                                    Name = s.Name,
                                    Corner = s.Corner
                                })
                                .FirstOrDefault();
                            item.TdcApartmentName = apartmentTdc != null ? apartmentTdc.Name : "";
                            item.Corner = apartmentTdc != null ? apartmentTdc.Corner : false;

                            //Giá Bán Cấu Thành Chính Thức
                            List<TdcPriceRentOfficial> tdcPriceRentOfficial = _context.TdcPriceRentOfficials.Where(l => l.TdcPriceRentId == item.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                            List<TdcPriceRentOfficialData> map_tdcPriceRentOfficials = _mapper.Map<List<TdcPriceRentOfficialData>>(tdcPriceRentOfficial.ToList());
                            foreach (TdcPriceRentOfficialData map_tdcPriceRentOfficial in map_tdcPriceRentOfficials)
                            {
                                map_tdcPriceRentOfficial.IngrePriceName = _context.IngredientsPrices.Where(l => l.Id == map_tdcPriceRentOfficial.IngredientsPriceId && l.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                            }
                            item.tdcPriceRentOfficials = map_tdcPriceRentOfficials.ToList();

                            // Giá Bán Cấu Thành Tạm Thời
                            List<TdcPriceRentTemporary> tdcPriceRentTemporarie = _context.TdcPriceRentTemporaries.Where(l => l.TdcPriceRentId == item.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                            List<TdcPriceRentTemporaryData> map_tdcPriceRentTemporaries = _mapper.Map<List<TdcPriceRentTemporaryData>>(tdcPriceRentTemporarie.ToList());
                            foreach (TdcPriceRentTemporaryData map_tdcPriceRentTemporarie in map_tdcPriceRentTemporaries)
                            {
                                map_tdcPriceRentTemporarie.IngrePriceName = _context.IngredientsPrices.Where(l => l.Id == map_tdcPriceRentTemporarie.IngredientsPriceId && l.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                            }
                            item.tdcPriceRentTemporaries = map_tdcPriceRentTemporaries.ToList();




                            //Thuế Phí Nông Nghiệp
                            item.tdcPriceRentTaxes = _context.TdcPriceRentTaxs.Where(l => l.TdcPriceRentId == item.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();

                        }
                        def.data = res;
                    }
                    return Ok(def);
                }
                else
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }
            }
            catch (Exception ex)
            {
                log.Error("GetByPage Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }


        [HttpGet("GetExcelTable/{tdcPriceRentId}")]
        public async Task<IActionResult> GetExcelTable(int tdcPriceRentId, [FromQuery] DateTime dateTime)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            DefaultResponse def = new DefaultResponse();
            //check role
            var identity = (ClaimsIdentity)User.Identity;
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }
            try
            {
                //Tìm hồ sơ cho thuê
                TdcPriceRent tdcPriceRent = _context.TdcPriceRents.Where(l => l.Id == tdcPriceRentId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                if (tdcPriceRent == null)
                {
                    def.meta = new Meta(404, "Không tìm thấy hồ sơ tương ứng!");
                    return Ok(def);
                }
                //Check Xem có file Import hay không
                bool checkImp = false;
                TdcPriceRentMetaData dataLast = null;
                TdcPriceRentMetaData dataFist = null;
                int? PaymentTimesLast = 0;
                decimal? unitPay = 0;
                DateTime? DateExpectedLast = null;
                DateTime? DateExpectedFist = null;
                DateTime? DatePrescribedLast = null;
                int countYear = 0;

                decimal? NCUnitPay = 0;
                decimal? NCPriceEarnings = 0;
                decimal? NCPricePaymentPeriod = 0;
                decimal? NCPay = 0;
                decimal? NCPaid = 0;
                decimal? Paid = 0;

                List<TdcPriceRentExcelMeta> lst = _context.TdcPriceRentExcelMetas.Where(f => f.TdcPriceRentId == tdcPriceRentId && f.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<TdcPriceRentMetaData> metaDataImport_mapper = _mapper.Map<List<TdcPriceRentMetaData>>(lst);

                if (lst.Count > 0)
                {
                    checkImp = true;

                    foreach (var itemImport in metaDataImport_mapper)
                    {
                        List<TdcPriceRentExcelData> tdcPriceRentMetaDatas = _context.TdcPriceRentExcelDatas.Where(l => l.TdcPriceRentExcelMetaId == itemImport.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.index).ToList();
                        itemImport.tdcPriceRentExcelDatas = tdcPriceRentMetaDatas;

                        NCUnitPay = NCUnitPay + itemImport.tdcPriceRentExcelDatas.Sum(x => x.UnitPay);
                        NCPriceEarnings = NCPriceEarnings + itemImport.tdcPriceRentExcelDatas.Sum(x => x.PriceEarnings);
                        NCPricePaymentPeriod = NCPricePaymentPeriod + itemImport.tdcPriceRentExcelDatas.Sum(x => x.PricePaymentPeriod);
                        NCPay = NCPricePaymentPeriod;
                        Paid = Paid + itemImport.tdcPriceRentExcelDatas.Sum(x => x.Paid);
                    }

                    dataLast = metaDataImport_mapper[metaDataImport_mapper.Count() - 1];
                    dataFist = metaDataImport_mapper[0];
                    PaymentTimesLast = int.Parse(dataLast.tdcPriceRentExcelDatas[dataLast.tdcPriceRentExcelDatas.Count() - 1].PaymentTimes);
                    DateExpectedLast = dataLast.tdcPriceRentExcelDatas[dataLast.tdcPriceRentExcelDatas.Count() - 1].ExpectedPaymentDate;
                    DateExpectedFist = dataFist.tdcPriceRentExcelDatas[0].ExpectedPaymentDate;
                    DatePrescribedLast = dataLast.tdcPriceRentExcelDatas[dataLast.tdcPriceRentExcelDatas.Count() - 1].PaymentDatePrescribed;
                    countYear = (int)dataFist.tdcPriceRentExcelDatas[0].CountYear;
                    Paid = Paid - dataFist.tdcPriceRentExcelDatas[0].Paid;

                }
                List<TdcPriceRentExcel> data = new List<TdcPriceRentExcel>(tdcPriceRent.MonthRent + 2);
                List<TdcPriceRentExcel> dataImport = new List<TdcPriceRentExcel>();
                List<TdcPriceRentPay> tdcPriceRentPays = _context.TdcPriceRentPays.Where(l => l.TdcPriceRentId == tdcPriceRent.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<ProfitValue> profitValues = _context.ProfitValues.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderBy(p => p.DoApply).ToList();

                decimal? noCu = 0;
                int k = 0;
                int kImp = 0;

                TypePayQD check;
                DateTime date1 = new DateTime(2021, 06, 10);

                int paytimeIdImport = 0;
                if (tdcPriceRentPays.Count() != 0)
                {
                    paytimeIdImport = (int)(tdcPriceRentPays[tdcPriceRentPays.Count() - 1].Id + 999.99);
                }
                else paytimeIdImport = paytimeIdImport;

                if (checkImp)
                {
                    foreach (var itemImport in metaDataImport_mapper)
                    {
                        paytimeIdImport++;

                        foreach (var item in itemImport.tdcPriceRentExcelDatas)
                        {

                            TdcPriceRentExcel itemDataImport = new TdcPriceRentExcel();

                            itemDataImport = new TdcPriceRentExcel(paytimeIdImport, true, item.PaymentTimes, item.PaymentDatePrescribed, item.PaymentDatePrescribed1, 0, item.ExpectedPaymentDate, item.Note, item.DailyInterest, (double?)item.DailyInterestRate, item.UnitPay, item.PriceEarnings, item.PricePaymentPeriod, item.Pay, item.Paid, item.PriceDifference, 5, false, true);
                            if (item.PaymentTimes == null)
                            {
                                itemDataImport = new TdcPriceRentExcel(paytimeIdImport, true, "Nợ Cũ", item.PaymentDatePrescribed, item.PaymentDatePrescribed1, 0, item.ExpectedPaymentDate, item.Note, item.DailyInterest, (double?)item.DailyInterestRate, item.UnitPay, item.PriceEarnings, item.PricePaymentPeriod, item.Pay, item.Paid, item.PriceDifference, 4, false, true);
                            }
                            if (item.PricePaymentPeriod == null && item.ExpectedPaymentDate == null)
                            {
                                itemDataImport = new TdcPriceRentExcel(paytimeIdImport, true, item.PaymentTimes, item.PaymentDatePrescribed, item.PaymentDatePrescribed1, 0, item.ExpectedPaymentDate, item.Note, item.DailyInterest, (double?)item.DailyInterestRate, item.UnitPay, item.PriceEarnings, item.PricePaymentPeriod, item.Pay, item.Paid, item.PriceDifference, 2, true, true);
                            }

                            data.Add(itemDataImport);

                            dataImport.Add(itemDataImport);
                        }
                    }

                    TdcPriceRentExcel fist = new TdcPriceRentExcel(-paytimeIdImport, true, "Chốt đến" + " " + DateExpectedLast?.ToString("dd/MM/yyyy"), null, null, 1, null, null, null, null, NCUnitPay, NCPriceEarnings, NCPricePaymentPeriod, NCPricePaymentPeriod, Paid, null, 1, true, false);
                    data.Add(fist);

                    TdcPriceRentExcel second = new TdcPriceRentExcel();
                    second.PaymentTimes = "Nợ cũ sau thanh lý";
                    second.Year = 2;
                    second.TypeRow = 1;
                    second.Status = true;
                    second.Check = false;
                    //Lấy ngày sau thanh lý HĐ
                    if (tdcPriceRentPays.Count > 0)
                    {
                        second.PaymentDatePrescribed1 = tdcPriceRentPays[0].PaymentDate;
                        // Lấy DateTime bên trong kiểu Nullable DateTime
                        DateTime start = DateExpectedLast.Value;
                        DateTime end = second.PaymentDatePrescribed1.Value;
                        // Lấy TimeSpan giữa hai ngày
                        TimeSpan span = end.Subtract(start);
                        // Lấy số ngày chênh lệch
                        second.DailyInterest = (int)span.TotalDays;
                        if (second.DailyInterest < 0) second.DailyInterest = 0;
                    }
                    else
                    {
                        second.DailyInterest = 0;
                    }
                    //Lấy Lãi Suất
                    foreach (var profitValue in profitValues)
                    {
                        if (second.PaymentDatePrescribed1 < profitValue.DoApply) break;
                        second.DailyInterestRate = profitValue.Value;
                    }
                    //Thời gian tính lãi theo ngày

                    foreach (var itemImport in metaDataImport_mapper)
                    {
                        List<TdcPriceRentExcelData> tdcPriceRentMetaDatas = _context.TdcPriceRentExcelDatas.Where(l => l.TdcPriceRentExcelMetaId == itemImport.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.index).ToList();
                        itemImport.tdcPriceRentExcelDatas = tdcPriceRentMetaDatas;

                        foreach (var i in itemImport.tdcPriceRentExcelDatas)
                        {
                            if (i.ExpectedPaymentDate == DateExpectedLast && i.UnitPay != null)
                            {
                                unitPay = unitPay + i.UnitPay;
                                second.UnitPay = unitPay;
                            }
                        }
                    }
                    second.PriceEarnings = Math.Round(((decimal)second.DailyInterest * (decimal)second.DailyInterestRate * (decimal)second.UnitPay) / 100);
                    second.PricePaymentPeriod = second.PriceEarnings;
                    data.Add(second);
                }
                else
                {
                    TdcPriceRentExcel fist = new TdcPriceRentExcel(0101, true, "Tiền thế chân", null, null, 2, tdcPriceRent.DateTTC, null, null, null, null, null, null, null, tdcPriceRent.PriceTC, null, 1, true, false);
                    data.Add(fist);

                    TdcPriceRentExcel second = new TdcPriceRentExcel(0100, true, "Năm thứ 1", null, null, 2, null, null, null, null, null, null, null, null, null, null, 1, true, false);
                    data.Add(second);
                }

                if (checkImp == false)
                {
                    TdcPriceRentPay firstPay = new TdcPriceRentPay();
                    firstPay.PayCount = 1;
                    firstPay.PaymentDate = tdcPriceRent.DateTTC;
                    firstPay.AmountPaid = Math.Round(tdcPriceRent.TotalPriceTT.GetValueOrDefault() / tdcPriceRent.MonthRent);
                    if (firstPay.PaymentDate > tdcPriceRent.DecisionDateCT) firstPay.AmountPaid = Math.Round(tdcPriceRent.TotalPriceCT / tdcPriceRent.MonthRent);
                    tdcPriceRentPays.Insert(0, firstPay);
                }

                decimal UnitPayTT = Math.Round(tdcPriceRent.TotalPriceTT.GetValueOrDefault() / tdcPriceRent.MonthRent);
                decimal UnitPayCT = Math.Round(tdcPriceRent.TotalPriceCT / tdcPriceRent.MonthRent);

                decimal? TotalUnitPay = 0;
                decimal? TotalPriceEarnings = 0;
                decimal? TotalPricePaymentPeriod = 0;
                decimal? TotalPay = 0;

                DateTime date = new DateTime(2019, 12, 31);
                int PayCount = 0;
                int payStatus = 0;
                int monthRent = 0;

                int j = 0;//để check nợ cũ

                if (checkImp == true)
                {
                    int monthDiff = ((DatePrescribedLast.Value.Year - DateExpectedFist.Value.Year) * 12) + (DatePrescribedLast.Value.Month - DateExpectedFist.Value.Month) + 1;
                    monthRent = tdcPriceRent.MonthRent - monthDiff;
                    j = dataImport.Count + 1; // nếu có nợ cũ thì gắn bằng số kì của nợ cũ
                }
                else
                {
                    monthRent = tdcPriceRent.MonthRent;
                    j = 1;//Ko có gắn bằng 1
                }

                for (int i = 0; i < monthRent; i++)
                {
                    check = AppEnums.TypePayQD.DUNG_HAN;
                    TdcPriceRentExcel tdcPriceRentExcel = new TdcPriceRentExcel();
                    tdcPriceRentExcel.Status = false;

                    if (data[j] != null)
                    {
                        if (data[j].PriceDifference > 0)
                        {
                            check = AppEnums.TypePayQD.NO_CU;
                            noCu = data[j].PriceDifference;

                        }
                        if (data[j].PriceDifference < 0)
                        {
                            // đóng dư tiền
                            check = AppEnums.TypePayQD.DONG_DU;
                        }
                        if (data[j].PriceDifference == null) data[j].PriceDifference = 0;
                    }

                    if (k < tdcPriceRentPays.Count())
                    {
                        //Id lần trả
                        tdcPriceRentExcel.PayTimeId = tdcPriceRentPays[k].Id;
                        //Ngày thanh toán thực tế
                        tdcPriceRentExcel.ExpectedPaymentDate = tdcPriceRentPays[k].PaymentDate;
                        tdcPriceRentExcel.PricePublic = tdcPriceRentPays[k].PricePublic;

                        if (PayCount == 0 && tdcPriceRentPays.Count != 0)
                        {
                            PayCount = tdcPriceRentPays[k].PayCount;
                        }

                        //Số tiền đã thanh toán
                        if (payStatus == 0)
                        {
                            tdcPriceRentExcel.Paid = tdcPriceRentPays[k].AmountPaid;
                            NCPaid += tdcPriceRentExcel.Paid;
                        }
                        else
                        {
                            tdcPriceRentExcel.Paid = 0;
                        }
                        tdcPriceRentExcel.Status = true;
                    }
                    else
                    {
                        tdcPriceRentExcel.Status = false;
                    }

                    j++;
                    //Số lần 
                    if (checkImp == true)
                    {
                        tdcPriceRentExcel.PaymentTimes = ((PaymentTimesLast + 1 + i) % 12).ToString();
                        if (tdcPriceRentExcel.PaymentTimes == "0") tdcPriceRentExcel.PaymentTimes = "12";
                    }
                    else
                    {
                        tdcPriceRentExcel.PaymentTimes = ((i + 1) % 12).ToString();
                        if (tdcPriceRentExcel.PaymentTimes == "0") tdcPriceRentExcel.PaymentTimes = "12";
                    }
                    tdcPriceRentExcel.TypeRow = 3;

                    //Lấy ngày thanh toán theo quy định
                    if (checkImp == true)
                    {
                        tdcPriceRentExcel.PaymentDatePrescribed = DatePrescribedLast.Value.AddMonths(i + 1);
                    }
                    else
                    {
                        TdcPriceRent firstPayDate = _context.TdcPriceRents.Where(l => l.Id == tdcPriceRent.Id).FirstOrDefault();
                        tdcPriceRentExcel.PaymentDatePrescribed = firstPayDate.DateTTC.AddMonths(i);
                    }

                    //Thời gian tính lãi theo ngày
                    if (tdcPriceRentExcel.ExpectedPaymentDate.HasValue && tdcPriceRentExcel.ExpectedPaymentDate.HasValue)
                    {
                        // Lấy DateTime bên trong kiểu Nullable DateTime
                        DateTime start = tdcPriceRentExcel.PaymentDatePrescribed.Value;
                        DateTime end = tdcPriceRentExcel.ExpectedPaymentDate.Value;
                        // Lấy TimeSpan giữa hai ngày
                        TimeSpan span = end.Subtract(start);
                        // Lấy số ngày chênh lệch
                        tdcPriceRentExcel.DailyInterest = (int)span.TotalDays;
                        if (tdcPriceRentExcel.DailyInterest < 0) tdcPriceRentExcel.DailyInterest = 0;
                    }
                    else
                    {
                        tdcPriceRentExcel.DailyInterest = 0;
                    }

                    //Lãi suất tính theo ngày
                    foreach (var profitValue in profitValues)
                    {
                        if (tdcPriceRentExcel.PaymentDatePrescribed < profitValue.DoApply) break;
                        tdcPriceRentExcel.DailyInterestRate = profitValue.Value;
                    }

                    //Số tiền phải trả từng tháng 
                    tdcPriceRentExcel.UnitPay = UnitPayTT;
                    if (tdcPriceRentExcel.PaymentDatePrescribed > tdcPriceRent.DecisionDateCT) tdcPriceRentExcel.UnitPay = UnitPayCT;

                    TotalUnitPay += tdcPriceRentExcel.UnitPay;

                    //Số tiền lãi phát sinh do chậm thanh toán                   
                    tdcPriceRentExcel.PriceEarnings = Math.Round(((decimal)tdcPriceRentExcel.DailyInterest * (decimal)tdcPriceRentExcel.DailyInterestRate * (decimal)tdcPriceRentExcel.UnitPay) / 100);

                    if (tdcPriceRentExcel.PriceEarnings != null)
                    {
                        TotalPriceEarnings += tdcPriceRentExcel.PriceEarnings;
                    }

                    //Số tiền đến kỳ phải thanh toán
                    tdcPriceRentExcel.PricePaymentPeriod = (tdcPriceRentExcel.PriceEarnings + tdcPriceRentExcel.UnitPay);

                    if (tdcPriceRentExcel.PricePaymentPeriod != null)
                    {
                        TotalPricePaymentPeriod += tdcPriceRentExcel.PricePaymentPeriod;
                    }

                    //Số tiền phải thanh toán
                    if (data[j - 1] != null)
                    {
                        tdcPriceRentExcel.Pay = tdcPriceRentExcel.PricePaymentPeriod + data[j - 1].PriceDifference;
                    }
                    else
                    {
                        tdcPriceRentExcel.Pay = tdcPriceRentExcel.PricePaymentPeriod;
                    }
                    if (tdcPriceRentExcel.Pay.HasValue)
                    {
                        TotalPay += tdcPriceRentExcel.Pay;
                    }

                    //Số tiền đã thanh toán
                    if (k < tdcPriceRentPays.Count())
                        tdcPriceRentExcel.Paid = tdcPriceRentPays[k].AmountPaid;
                    if (check == AppEnums.TypePayQD.DONG_DU)
                    {
                        if (data[j - 1].PriceDifference < 0 && data[j - 1].PayTimeId != tdcPriceRentExcel.PayTimeId)
                        {
                            tdcPriceRentExcel.Pay = tdcPriceRentExcel.PricePaymentPeriod + data[j - 1].PriceDifference;
                            if (k < tdcPriceRentPays.Count())
                                tdcPriceRentExcel.Paid = tdcPriceRentPays[k].AmountPaid;
                        }
                        else
                        {
                            tdcPriceRentExcel.Pay = tdcPriceRentExcel.PricePaymentPeriod;
                            tdcPriceRentExcel.Paid = -data[j - 1].PriceDifference;
                        }
                        if (tdcPriceRentExcel.Pay.HasValue)
                        {
                            TotalPay += tdcPriceRentExcel.Pay;
                        }
                    }

                    //Chênh lệch
                    tdcPriceRentExcel.PriceDifference = tdcPriceRentExcel.Pay - tdcPriceRentExcel.Paid;

                    if (tdcPriceRentExcel.PriceDifference.HasValue)
                    {
                        tdcPriceRentExcel.Status = true;
                    }
                    else
                    {
                        int day = data.First().ExpectedPaymentDate.Value.Day;
                        int dayNow = dateTime.Day;

                        DateTime newDate = dateTime.AddDays(day);
                        tdcPriceRentExcel.ExpectedPaymentDate = newDate.AddDays(-dayNow);

                        if(tdcPriceRentExcel.ExpectedPaymentDate < dateTime)
                        {
                            tdcPriceRentExcel.ExpectedPaymentDate=  tdcPriceRentExcel.ExpectedPaymentDate.Value.AddMonths(1);
                        }

                        DateTime start = tdcPriceRentExcel.PaymentDatePrescribed.Value;
                        DateTime end = tdcPriceRentExcel.ExpectedPaymentDate.Value;
                        // Lấy TimeSpan giữa hai ngày
                        TimeSpan span = end.Subtract(start);
                        // Lấy số ngày chênh lệch
                        tdcPriceRentExcel.DailyInterest = (int)span.TotalDays;

                        if (tdcPriceRentExcel.DailyInterest < 0) tdcPriceRentExcel.DailyInterest = 0;

                        //Số tiền lãi phát sinh do chậm thanh toán 
                        tdcPriceRentExcel.PriceEarnings = Math.Round(((decimal)tdcPriceRentExcel.DailyInterest * (decimal)tdcPriceRentExcel.DailyInterestRate * (decimal)tdcPriceRentExcel.UnitPay) / 100);
                        if (tdcPriceRentExcel.PriceEarnings != null)
                        {
                            TotalPriceEarnings += tdcPriceRentExcel.PriceEarnings;
                        }

                        //Số tiền đến kỳ phải thanh toán
                        tdcPriceRentExcel.PricePaymentPeriod = (tdcPriceRentExcel.PriceEarnings + tdcPriceRentExcel.UnitPay);

                        if (tdcPriceRentExcel.PricePaymentPeriod != null)
                        {
                            TotalPricePaymentPeriod += tdcPriceRentExcel.PricePaymentPeriod;
                        }

                        if (data[j - 1] != null)
                        {
                            tdcPriceRentExcel.Pay = tdcPriceRentExcel.PricePaymentPeriod + data[j - 1].PriceDifference;
                        }
                        else
                        {
                            tdcPriceRentExcel.Pay = tdcPriceRentExcel.PricePaymentPeriod;
                        }

                        if (tdcPriceRentExcel.Pay.HasValue)
                        {
                            TotalPay += tdcPriceRentExcel.Pay;
                        }
                    }
                    data.Add(tdcPriceRentExcel);

                    if (check == AppEnums.TypePayQD.NO_CU)
                    {

                        //Thêm  nợ cũ
                        TdcPriceRentExcel tdcPriceRentExcelNoCu = new TdcPriceRentExcel();
                        j++;
                        if (k < tdcPriceRentPays.Count())
                        {
                            tdcPriceRentExcelNoCu.PayTimeId = tdcPriceRentPays[k].Id;
                            PayCount--;
                        }

                        tdcPriceRentExcelNoCu.TypeRow = 4;

                        //tdcPriceRentExcelNoCu.Status = false;

                        tdcPriceRentExcelNoCu.PaymentTimes = "Nợ cũ";

                        tdcPriceRentExcelNoCu.RowStatus = AppEnums.TypePayQD.NO_CU;

                        if (k < tdcPriceRentPays.Count())
                            tdcPriceRentExcelNoCu.ExpectedPaymentDate = tdcPriceRentPays[k].PaymentDate;

                        //Thời gian tính lãi theo ngày
                        if (k < tdcPriceRentPays.Count())
                        {
                            DateTime start = tdcPriceRentExcelNoCu.ExpectedPaymentDate.Value;
                            DateTime end = data[j - 2].ExpectedPaymentDate.Value;
                            TimeSpan span = start.Subtract(end);
                            tdcPriceRentExcelNoCu.DailyInterest = (int)span.TotalDays;
                            if (tdcPriceRentExcelNoCu.DailyInterest < 0) tdcPriceRentExcelNoCu.DailyInterest = 0;
                        }

                        //Lãi suất
                        tdcPriceRentExcelNoCu.DailyInterestRate = tdcPriceRentExcel.DailyInterestRate;

                        //Số tiền lãi phát sinh do chậm thanh toán 
                        if (k < tdcPriceRentPays.Count())
                        {
                            tdcPriceRentExcelNoCu.PriceEarnings = Math.Round(((decimal)tdcPriceRentExcelNoCu.DailyInterest * (decimal)tdcPriceRentExcelNoCu.DailyInterestRate * (decimal)noCu) / 100);
                            TotalPriceEarnings += tdcPriceRentExcelNoCu.PriceEarnings;
                        }

                        //Số tiền đến kỳ phải thanh toán
                        tdcPriceRentExcelNoCu.PricePaymentPeriod = tdcPriceRentExcelNoCu.PriceEarnings;

                        TotalPricePaymentPeriod += tdcPriceRentExcelNoCu.PricePaymentPeriod;

                        if (data[j - 1].PriceDifference > 0)
                        {
                            tdcPriceRentExcelNoCu.Pay = tdcPriceRentExcelNoCu.PricePaymentPeriod + data[j - 1].PriceDifference;
                            tdcPriceRentExcelNoCu.Paid = 0;
                            tdcPriceRentExcelNoCu.PriceDifference = tdcPriceRentExcelNoCu.Pay;
                        }
                        else
                        {
                            tdcPriceRentExcelNoCu.Pay = tdcPriceRentExcelNoCu.PricePaymentPeriod;
                            tdcPriceRentExcelNoCu.Paid = tdcPriceRentExcelNoCu.Pay;
                            tdcPriceRentExcelNoCu.PriceDifference = tdcPriceRentExcelNoCu.Pay + data[j - 1].PriceDifference;
                        }

                        if (tdcPriceRentExcelNoCu.Pay.HasValue)
                        {
                            TotalPay += tdcPriceRentExcelNoCu.Pay;
                        }

                        if (tdcPriceRentExcelNoCu.ExpectedPaymentDate == null)
                        {
                            int day = data.First().ExpectedPaymentDate.Value.Day;
                            int dayNow = dateTime.Day;

                            DateTime newDate = dateTime.AddDays(day);
                            tdcPriceRentExcelNoCu.ExpectedPaymentDate = newDate.AddDays(-dayNow);

                            if (tdcPriceRentExcelNoCu.ExpectedPaymentDate < dateTime)
                            {
                                tdcPriceRentExcelNoCu.ExpectedPaymentDate = tdcPriceRentExcelNoCu.ExpectedPaymentDate.Value.AddMonths(1);
                            }

                            DateTime start = tdcPriceRentExcelNoCu.ExpectedPaymentDate.Value;
                            DateTime end = data[j - 2].ExpectedPaymentDate.Value;
                            TimeSpan span = start.Subtract(end);
                            tdcPriceRentExcelNoCu.DailyInterest = (int)span.TotalDays;
                            if (tdcPriceRentExcelNoCu.DailyInterest < 0) tdcPriceRentExcelNoCu.DailyInterest = 0;


                            //Lãi suất
                            tdcPriceRentExcelNoCu.DailyInterestRate = tdcPriceRentExcel.DailyInterestRate;

                            //Số tiền lãi phát sinh do chậm thanh toán 

                            tdcPriceRentExcelNoCu.PriceEarnings = Math.Round(((decimal)tdcPriceRentExcelNoCu.DailyInterest * (decimal)tdcPriceRentExcelNoCu.DailyInterestRate * (decimal)noCu) / 100);
                            TotalPriceEarnings += tdcPriceRentExcelNoCu.PriceEarnings;


                            //Số tiền đến kỳ phải thanh toán
                            tdcPriceRentExcelNoCu.PricePaymentPeriod = tdcPriceRentExcelNoCu.PriceEarnings;

                            TotalPricePaymentPeriod += tdcPriceRentExcelNoCu.PricePaymentPeriod;

                            tdcPriceRentExcelNoCu.Pay = tdcPriceRentExcelNoCu.PricePaymentPeriod;


                            if (tdcPriceRentExcelNoCu.Pay.HasValue)
                            {
                                TotalPay += tdcPriceRentExcelNoCu.Pay;
                            }
                        }

                        data.Add(tdcPriceRentExcelNoCu);
                    }

                    decimal checkyear;
                    if (checkImp == true)
                    {
                        checkyear = (PaymentTimesLast.Value + i + 1) % 12;
                    }
                    else
                    {
                        checkyear = (i + 1) % 12;
                    }

                    if (checkyear == 0)
                    {
                        j++;
                        TdcPriceRentExcel Year = new TdcPriceRentExcel();
                        int year = 0;

                        if (checkImp == true)
                        {
                            year = countYear + ((i + 1) / 12) + 1;
                        }
                        else
                        {
                            year = ((i + 1) / 12) + 1;
                        }

                        Year.PaymentTimes = "Năm thứ " + year;
                        if (k < tdcPriceRentPays.Count())
                        {
                            Year.PayTimeId = tdcPriceRentPays[k].Id;
                            PayCount--;
                        }
                        Year.TypeRow = 2;
                        Year.Year = 2;
                        Year.PriceDifference = data[j - 1].PriceDifference;
                        Year.ExpectedPaymentDate = data[j - 1].ExpectedPaymentDate;
                        Year.Check = true;
                        if (data[j - 1].Status == true)
                        {
                            Year.Status = true;
                        }
                        else Year.Status = false;
                        data.Add(Year);
                    }
                    if (k == 0 && PayCount == 1) PayCount = 0;
                    if (PayCount == 0 && tdcPriceRentPays.Count != 0)
                    {
                        k++;

                    }
                    else
                    {
                        PayCount--;
                        if (PayCount == 0) k = k + 1;
                    }

                }
                //Thế Chân
                TdcPriceRentExcel tdcPriceRentExcelTCT = new TdcPriceRentExcel();
                tdcPriceRentExcelTCT.PaymentTimes = "Tiền Thế Chân Tăng";
                tdcPriceRentExcelTCT.Year = 2;
                tdcPriceRentExcelTCT.TypeRow = 1;
                decimal TCTT = 12 * (tdcPriceRent.TotalPriceTT.GetValueOrDefault() / tdcPriceRent.MonthRent);
                decimal TCCT = 12 * (tdcPriceRent.TotalPriceCT / tdcPriceRent.MonthRent);
                tdcPriceRentExcelTCT.Pay = TCCT - TCTT;
                tdcPriceRentExcelTCT.Paid = null;
                tdcPriceRentExcelTCT.Status = true;
                tdcPriceRentExcelTCT.Check = true;
                data.Add(tdcPriceRentExcelTCT);

                //Truy Thu
                TdcPriceRentExcel tdcPriceRentExcelLast = new TdcPriceRentExcel();
                tdcPriceRentExcelLast.Year = 1;
                tdcPriceRentExcelLast.TypeRow = 1;
                tdcPriceRentExcelLast.UnitPay = (tdcPriceRent.TotalPriceCT / tdcPriceRent.MonthRent) - (tdcPriceRent.TotalPriceTT.GetValueOrDefault() / tdcPriceRent.MonthRent);
                //Tháng Chênh Lệch
                int monthsDiff = ((tdcPriceRent.DecisionDateCT.Year - tdcPriceRent.DateTTC.Year) * 12) + (tdcPriceRent.DecisionDateCT.Month - tdcPriceRent.DateTTC.Month);
                tdcPriceRentExcelLast.PaymentTimes = "(" + monthsDiff + "tháng )" + " " + "Truy Thu từ " + tdcPriceRent.DateTTC.ToString("dd/MM/yyyy") + "  " + "đến" + "  " + tdcPriceRent.DecisionDateCT.ToString("dd/MM/yyyy");
                tdcPriceRentExcelLast.PricePaymentPeriod = tdcPriceRentExcelLast.UnitPay * monthsDiff;
                tdcPriceRentExcelLast.Pay = tdcPriceRentExcelLast.PricePaymentPeriod;
                tdcPriceRentExcelLast.Paid = null;
                tdcPriceRentExcelLast.Status = true;
                tdcPriceRentExcelLast.Check = true;
                data.Add(tdcPriceRentExcelLast);

                //Tổng
                TdcPriceRentExcel tdcPriceRentExcelTotal = new TdcPriceRentExcel();
                tdcPriceRentExcelTotal.PaymentTimes = "Tổng";
                tdcPriceRentExcelTotal.Year = 2;
                tdcPriceRentExcelTotal.TypeRow = 1;
                tdcPriceRentExcelTotal.UnitPay = NCUnitPay + TotalUnitPay;
                tdcPriceRentExcelTotal.PriceEarnings = NCPriceEarnings + TotalPriceEarnings;
                tdcPriceRentExcelTotal.PricePaymentPeriod = tdcPriceRentExcelTotal.PriceEarnings + tdcPriceRentExcelTotal.UnitPay + tdcPriceRentExcelLast.PricePaymentPeriod;
                tdcPriceRentExcelTotal.Pay = NCPricePaymentPeriod + TotalPay + tdcPriceRentExcelLast.Pay + tdcPriceRentExcelTCT.Pay;
                if (checkImp == true)
                {
                    tdcPriceRentExcelTotal.Paid = tdcPriceRentExcelTotal.Pay - Paid;
                }
                else
                {
                    tdcPriceRentExcelTotal.Paid = tdcPriceRentExcelTotal.Pay - NCPaid;
                }
                tdcPriceRentExcelTotal.Status = true;
                tdcPriceRentExcelTotal.Check = true;
                data.Add(tdcPriceRentExcelTotal);

                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i].TypeRow == 2)
                    {
                        TdcPriceRentExcel prev = null;
                        TdcPriceRentExcel next = null;

                        if (i - 1 > -1) prev = data[i - 1];
                        if (i + 1 < data.Count) next = data[i + 1];

                        if (prev != null && next != null)
                        {
                            if (prev.PayTimeId == next.PayTimeId) data[i].PayTimeId = next.PayTimeId;
                        }
                    }
                }

                var groupDataExcel = data.GroupBy(f => f.PayTimeId != null ? (object)f.PayTimeId : new object(), key => key).Select(f => new TdcPriceRentExcelGroupByPayTimeId
                {
                    PayTimeId = f.Key,
                    tdcPriceRentExcels = f.ToList()
                });

                List<TdcPriceRentExcelGroupByPayTimeId> res = new List<TdcPriceRentExcelGroupByPayTimeId>();
                foreach (var item in groupDataExcel.ToList())
                {
                    if (item.tdcPriceRentExcels.Where(x => x.TypeRow == 3).FirstOrDefault() != null)
                    {
                        item.Pay = item.tdcPriceRentExcels.Sum(x => x.TypeRow == 3 ? x.Pay : (x.TypeRow == 4 ? x.PriceEarnings : 0));
                        item.Paid = item.tdcPriceRentExcels.First().Paid;
                        item.PriceDifference = item.tdcPriceRentExcels[item.tdcPriceRentExcels.Count - 1].PriceDifference;
                        item.Status = item.tdcPriceRentExcels.First().Status;
                        item.PricePublic = item.tdcPriceRentExcels.First().PricePublic;
                        item.Check = item.tdcPriceRentExcels.First().Check;

                    }
                    else if (item.tdcPriceRentExcels.Where(x => x.TypeRow == 1).FirstOrDefault() != null)
                    {
                        item.Pay = item.tdcPriceRentExcels.Sum(x => x.Pay);
                        item.Paid = item.tdcPriceRentExcels.Sum(x => x.Paid);
                        item.Status = item.Status = true;
                        item.Check = item.Check = true;
                    }
                    else if (item.tdcPriceRentExcels.Where(x => x.TypeRow == 2).FirstOrDefault() != null)
                    {
                        item.Status = item.tdcPriceRentExcels.First().Status;
                        item.Check = item.tdcPriceRentExcels.First().Check;

                        item.Pay = item.tdcPriceRentExcels.First().Pay;
                        item.Paid = item.tdcPriceRentExcels.First().Paid;
                        item.PriceDifference = item.tdcPriceRentExcels.Sum(x => x.PriceDifference);

                    }
                    else if (item.tdcPriceRentExcels.Where(x => x.TypeRow == 5).FirstOrDefault() != null)
                    {
                        item.Pay = item.tdcPriceRentExcels.First().Pay;
                        item.Paid = item.tdcPriceRentExcels.First().Paid;
                        item.PriceDifference = item.tdcPriceRentExcels.Sum(x => x.PriceDifference);
                        item.Status = item.tdcPriceRentExcels.First().Status;
                    }
                    else if (item.tdcPriceRentExcels.Where(x => x.TypeRow == 4 && x.PayTimeId == null).FirstOrDefault() != null)
                    {
                        item.Status = false;
                        item.Pay = item.tdcPriceRentExcels.First().Pay;
                    }

                    res.Add(item);
                }

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = res;

                return Ok(def);

            }
            catch (Exception ex)
            {
                log.Error("GetExcelTable Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);

                return Ok(def);
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            DefaultResponse def = new DefaultResponse();
            //check role
            var identity = (ClaimsIdentity)User.Identity;
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }
            try
            {
                TdcPriceRent data = await _context.TdcPriceRents.FindAsync(id);
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                    return Ok(def);
                }


                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = data;
                return Ok(def);
            }


            catch (Exception ex)
            {
                log.Error("GetById Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(TdcPriceRentData input)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            DefaultResponse def = new DefaultResponse();

            var identity = (ClaimsIdentity)User.Identity;
            int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id").Select(c => c.Value).SingleOrDefault());
            string fullName = identity.Claims.Where(c => c.Type == "FullName").Select(c => c.Value).SingleOrDefault() ?? string.Empty;
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.CREATE))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_CREATE_MESSAGE);
                return Ok(def);
            }
            try
            {
                input = (TdcPriceRentData)UtilsService.TrimStringPropertyTypeObject(input);
                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }
                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.CreatedById = userId;
                    input.CreatedBy = fullName;
                    input.Check = false;

                    _context.TdcPriceRents.Add(input);

                    try
                    {

                        await _context.SaveChangesAsync();
                        string code = "";
                        int newid = input.Id;
                        if (newid < 10) code = "2003DC000" + newid + "-PLHĐ";
                        if (newid >= 10) code = "2003DC00" + newid + "-PLHĐ";
                        if (newid >= 100) code = "2003DC0" + newid + "-PLHĐ";
                        if (newid >= 1000) code = "2003DC" + newid + "-PLHĐ";

                        input.Code = code;

                        TdcPriceRent tdcPriceRent = _context.TdcPriceRents.Find(newid);
                        tdcPriceRent.Code = code;
                        _context.Update(tdcPriceRent);
                        if (input.tdcPriceRentOfficials != null)
                        {
                            foreach (var tdcPriceOfficial in input.tdcPriceRentOfficials)
                            {
                                tdcPriceOfficial.TdcPriceRentId = input.Id;
                                tdcPriceOfficial.CreatedBy = fullName;
                                tdcPriceOfficial.CreatedById = userId;

                                _context.TdcPriceRentOfficials.Add(tdcPriceOfficial);
                            }
                            await _context.SaveChangesAsync();
                        }
                        if (input.tdcPriceRentTemporaries != null)
                        {
                            foreach (var tdcPriceRentTemporarie in input.tdcPriceRentTemporaries)
                            {
                                tdcPriceRentTemporarie.TdcPriceRentId = input.Id;
                                tdcPriceRentTemporarie.CreatedById = userId;
                                tdcPriceRentTemporarie.CreatedBy = fullName;

                                _context.TdcPriceRentTemporaries.Add(tdcPriceRentTemporarie);
                            }
                            await _context.SaveChangesAsync();
                        }
                        if (input.tdcPriceRentTaxes != null)
                        {
                            foreach (var tdcPriceRentTaxe in input.tdcPriceRentTaxes)
                            {
                                tdcPriceRentTaxe.TdcPriceRentId = input.Id;
                                tdcPriceRentTaxe.CreatedById = userId;
                                tdcPriceRentTaxe.CreatedBy = fullName;

                                _context.TdcPriceRentTaxs.Add(tdcPriceRentTaxe);
                            }
                            await _context.SaveChangesAsync();
                        }

                        LogActionModel logActionModel = new LogActionModel("Thêm mới hồ sơ" + input.Code, "TdcPriceRent", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (input.Id > 0) transaction.Commit();
                        else transaction.Rollback();

                        def.meta = new Meta(200, ApiConstants.MessageResource.ADD_SUCCESS);
                        def.data = input;
                        return Ok(def);
                    }
                    catch (DbUpdateException e)
                    {
                        log.Error("DbUpdateException:" + e);
                        transaction.Rollback();
                        if (TdcPriceRentExists(input.Id))
                        {
                            def.meta = new Meta(212, "Id đã tồn tại trên hệ thống!!!");
                            return Ok(def);
                        }
                        else
                        {
                            def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                            return Ok(def);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                log.Error("Post Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] TdcPriceRentData input)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            DefaultResponse def = new DefaultResponse();

            var identity = (ClaimsIdentity)User.Identity;
            int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id").Select(c => c.Value).SingleOrDefault());
            string fullName = identity.Claims.Where(c => c.Type == "FullName").Select(c => c.Value).SingleOrDefault() ?? string.Empty;
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.UPDATE))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_UPDATE_MESSAGE);
                return Ok(def);
            }
            try
            {
                input = (TdcPriceRentData)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }
                if (id != input.Id)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }
                TdcPriceRent data = await _context.TdcPriceRents.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_UPDATE_MESSAGE);
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.UpdatedAt = DateTime.Now;
                    input.UpdatedById = userId;
                    input.UpdatedBy = fullName;
                    input.CreatedAt = data.CreatedAt;
                    input.CreatedBy = data.CreatedBy;
                    input.CreatedById = data.CreatedById;
                    input.Status = data.Status;

                    _context.Update(input);
                    try
                    {
                        string code = "";
                        int newid = input.Id;
                        if (newid < 10) code = "2003DC000" + newid + "-PLHĐ";
                        if (newid >= 10) code = "2003DC00" + newid + "-PLHĐ";
                        if (newid >= 100) code = "2003DC0" + newid + "-PLHĐ";
                        if (newid >= 1000) code = "2003DC" + newid + "-PLHĐ";

                        input.Code = code;

                        TdcPriceRent tdcPriceRent = _context.TdcPriceRents.Find(newid);
                        tdcPriceRent.Code = code;
                        _context.Update(tdcPriceRent);

                        await _context.SaveChangesAsync();

                        List<TdcPriceRentTax> lstTdcPriceRentTaxAdd = new List<TdcPriceRentTax>();
                        List<TdcPriceRentTax> lstTdcPriceRentTaxUpdate = new List<TdcPriceRentTax>();

                        List<TdcPriceRentTax> TdcPriceRentTaxes = _context.TdcPriceRentTaxs.AsNoTracking().Where(l => l.TdcPriceRentId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();

                        if (input.tdcPriceRentTaxes != null)
                        {
                            foreach (var tdcPriceRentTax in input.tdcPriceRentTaxes)
                            {
                                TdcPriceRentTax tdcPriceRentTaxExist = TdcPriceRentTaxes.Where(l => l.Id == tdcPriceRentTax.Id).FirstOrDefault();
                                if (tdcPriceRentTaxExist == null)
                                {
                                    tdcPriceRentTax.TdcPriceRentId = input.Id;
                                    tdcPriceRentTax.CreatedBy = fullName;
                                    tdcPriceRentTax.CreatedById = userId;

                                    lstTdcPriceRentTaxAdd.Add(tdcPriceRentTax);
                                }
                                else
                                {
                                    tdcPriceRentTax.CreatedAt = tdcPriceRentTax.CreatedAt;
                                    tdcPriceRentTax.CreatedBy = tdcPriceRentTax.CreatedBy;
                                    tdcPriceRentTax.CreatedById = tdcPriceRentTax.UpdatedById;
                                    tdcPriceRentTax.TdcPriceRentId = input.Id;
                                    tdcPriceRentTax.UpdatedById = userId;
                                    tdcPriceRentTax.UpdatedBy = fullName;

                                    lstTdcPriceRentTaxUpdate.Add(tdcPriceRentTax);
                                    TdcPriceRentTaxes.Remove(tdcPriceRentTaxExist);
                                }
                            }
                        }
                        foreach (var itemTdcPriceRentTax in TdcPriceRentTaxes)
                        {
                            itemTdcPriceRentTax.UpdatedAt = DateTime.Now;
                            itemTdcPriceRentTax.UpdatedById = userId;
                            itemTdcPriceRentTax.UpdatedBy = fullName;
                            itemTdcPriceRentTax.Status = AppEnums.EntityStatus.DELETED;

                            lstTdcPriceRentTaxUpdate.Add(itemTdcPriceRentTax);
                        }
                        _context.TdcPriceRentTaxs.UpdateRange(lstTdcPriceRentTaxUpdate);
                        _context.TdcPriceRentTaxs.AddRange(lstTdcPriceRentTaxAdd);
                        //Thành Phần Giá Bán Cấu Thành Tạm Thời
                        List<TdcPriceRentTemporary> lstTdcPriceRentTemporaryAdd = new List<TdcPriceRentTemporary>();
                        List<TdcPriceRentTemporary> lstlTdcPriceRentTemporaryUpdate = new List<TdcPriceRentTemporary>();

                        List<TdcPriceRentTemporary> TdcPriceRentTemporarys = _context.TdcPriceRentTemporaries.AsNoTracking().Where(l => l.TdcPriceRentId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();

                        if (input.tdcPriceRentTemporaries != null)
                        {
                            foreach (var tdcPriceRentTemporarie in input.tdcPriceRentTemporaries)
                            {
                                TdcPriceRentTemporary tdcPriceRentTemporaryExist = TdcPriceRentTemporarys.Where(l => l.Id == tdcPriceRentTemporarie.Id).FirstOrDefault();
                                if (tdcPriceRentTemporaryExist == null)
                                {
                                    tdcPriceRentTemporarie.TdcPriceRentId = input.Id;
                                    tdcPriceRentTemporarie.CreatedBy = fullName;
                                    tdcPriceRentTemporarie.CreatedById = userId;

                                    lstTdcPriceRentTemporaryAdd.Add(tdcPriceRentTemporarie);
                                }
                                else
                                {
                                    tdcPriceRentTemporarie.CreatedAt = tdcPriceRentTemporarie.CreatedAt;
                                    tdcPriceRentTemporarie.CreatedBy = tdcPriceRentTemporarie.CreatedBy;
                                    tdcPriceRentTemporarie.CreatedById = tdcPriceRentTemporarie.UpdatedById;
                                    tdcPriceRentTemporarie.TdcPriceRentId = input.Id;
                                    tdcPriceRentTemporarie.UpdatedById = userId;
                                    tdcPriceRentTemporarie.UpdatedBy = fullName;

                                    lstlTdcPriceRentTemporaryUpdate.Add(tdcPriceRentTemporarie);
                                    TdcPriceRentTemporarys.Remove(tdcPriceRentTemporaryExist);
                                }
                            }
                        }
                        foreach (var itemTdcPriceRentTemporarie in TdcPriceRentTemporarys)
                        {
                            itemTdcPriceRentTemporarie.UpdatedAt = DateTime.Now;
                            itemTdcPriceRentTemporarie.UpdatedById = userId;
                            itemTdcPriceRentTemporarie.UpdatedBy = fullName;
                            itemTdcPriceRentTemporarie.Status = AppEnums.EntityStatus.DELETED;

                            lstlTdcPriceRentTemporaryUpdate.Add(itemTdcPriceRentTemporarie);
                        }
                        _context.TdcPriceRentTemporaries.UpdateRange(lstlTdcPriceRentTemporaryUpdate);
                        _context.TdcPriceRentTemporaries.AddRange(lstTdcPriceRentTemporaryAdd);

                        //Thành Phần Giá Bán Cấu Thành Chính Thức
                        //List<TdcPriceRentOfficial> lstTdcPriceRentOfficialAdd = new List<TdcPriceRentOfficial>();
                        //List<TdcPriceRentOfficial> lstTdcPriceRentOfficialUpdate = new List<TdcPriceRentOfficial>();

                        //List<TdcPriceRentOfficial> TdcPriceRentOfficials = _context.TdcPriceRentOfficials.AsNoTracking().Where(l => l.TdcPriceRentId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();

                        //if (input.tdcPriceRentOfficials != null)
                        //{
                        //    foreach (var tdcPriceRentOfficial in input.tdcPriceRentOfficials)
                        //    {
                        //        TdcPriceRentOfficial tdcPriceRentOfficialExist = TdcPriceRentOfficials.Where(l => l.Id == tdcPriceRentOfficial.Id).FirstOrDefault();
                        //        if (tdcPriceRentOfficialExist == null)
                        //        {
                        //            tdcPriceRentOfficial.TdcPriceRentId = input.Id;
                        //            tdcPriceRentOfficial.CreatedBy = fullName;
                        //            tdcPriceRentOfficial.CreatedById = userId;

                        //            lstTdcPriceRentOfficialAdd.Add(tdcPriceRentOfficial);
                        //        }
                        //        else
                        //        {
                        //            tdcPriceRentOfficial.CreatedAt = tdcPriceRentOfficial.CreatedAt;
                        //            tdcPriceRentOfficial.CreatedBy = tdcPriceRentOfficial.CreatedBy;
                        //            tdcPriceRentOfficial.CreatedById = tdcPriceRentOfficial.UpdatedById;
                        //            tdcPriceRentOfficial.TdcPriceRentId = input.Id;
                        //            tdcPriceRentOfficial.UpdatedById = userId;
                        //            tdcPriceRentOfficial.UpdatedBy = fullName;

                        //            lstTdcPriceRentOfficialUpdate.Add(tdcPriceRentOfficial);
                        //            TdcPriceRentOfficials.Remove(tdcPriceRentOfficialExist);
                        //        }
                        //    }
                        //}
                        //foreach (var itemTdcPriceRentOfficial in TdcPriceRentOfficials)
                        //{
                        //    itemTdcPriceRentOfficial.UpdatedAt = DateTime.Now;
                        //    itemTdcPriceRentOfficial.UpdatedById = userId;
                        //    itemTdcPriceRentOfficial.UpdatedBy = fullName;
                        //    itemTdcPriceRentOfficial.Status = AppEnums.EntityStatus.DELETED;

                        //    lstTdcPriceRentOfficialUpdate.Add(itemTdcPriceRentOfficial);
                        //}
                        //_context.TdcPriceRentOfficials.UpdateRange(lstTdcPriceRentOfficialUpdate);
                        //_context.TdcPriceRentOfficials.AddRange(lstTdcPriceRentOfficialAdd);


                        List<TdcPriceRentOfficial> tdcPriceRentOfficial = _context.TdcPriceRentOfficials.Where(l => l.TdcPriceRentId == id && l.Status != AppEnums.EntityStatus.DELETED).AsNoTracking().ToList();
                        List<TdcPriceRentOfficialData> map_tdcPriceRentOfficials = _mapper.Map<List<TdcPriceRentOfficialData>>(tdcPriceRentOfficial.ToList());
                        foreach (TdcPriceRentOfficialData map_tdcPriceRentOfficial in map_tdcPriceRentOfficials)
                        {
                            map_tdcPriceRentOfficial.IngrePriceName = _context.IngredientsPrices.Where(l => l.Id == map_tdcPriceRentOfficial.IngredientsPriceId && l.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                        }
                        var lst = map_tdcPriceRentOfficials.ToList();

                        if (input.tdcPriceRentOfficials != null)
                        {
                            bool check = Check(lst, input.tdcPriceRentOfficials);

                            if(check == false)
                            {
                                lst.ForEach(x => {
                                    x.ChangeTimes = lst[0].Id;
                                    x.Status = AppEnums.EntityStatus.DELETED;
                                });
                                _context.UpdateRange(lst);
                                await _context.SaveChangesAsync();
                                foreach (var item in input.tdcPriceRentOfficials)
                                {
                                    
                                    item.Id = 0;
                                    item.TdcPriceRentId = input.Id;
                                    _context.TdcPriceRentOfficials.Add(item);
                                }
                                await _context.SaveChangesAsync();
                            }

                        }

                        LogActionModel logActionModel = new LogActionModel("Chỉnh sửa thông tin Hồ Sơ " + input.Code, "TdcPriceRent", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (input.Id > 0) transaction.Commit();
                        else transaction.Rollback();

                        def.meta = new Meta(200, ApiConstants.MessageResource.UPDATE_SUCCESS);
                        def.data = data;
                        return Ok(def);
                    }

                    catch (DbUpdateException e)
                    {
                        transaction.Rollback();
                        log.Error("DbUpdateException" + e);
                        if (!TdcPriceRentExists(data.Id))
                        {
                            def.meta = new Meta(212, "Id Không Tồn Tại Trên Hệ Thống!!!");
                            return Ok(def);
                        }
                        else
                        {
                            def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                            return Ok(def);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            DefaultResponse def = new DefaultResponse();

            var identity = (ClaimsIdentity)User.Identity;
            int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id").Select(c => c.Value).SingleOrDefault());
            string fullName = identity.Claims.Where(c => c.Type == "FullName").Select(c => c.Value).SingleOrDefault() ?? string.Empty;
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.DELETED))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_DELETE_MESSAGE);
                return Ok(def);
            }
            try
            {
                TdcPriceRent data = await _context.TdcPriceRents.FindAsync(id);
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_DELETE_MESSAGE);
                    return Ok(def);

                }
                using (var transaction = _context.Database.BeginTransaction())
                {
                    data.UpdatedAt = DateTime.Now;
                    data.UpdatedBy = fullName;
                    data.UpdatedById = userId;
                    data.Status = AppEnums.EntityStatus.DELETED;
                    _context.Update(data);

                    try
                    {
                        await _context.SaveChangesAsync();
                        LogActionModel logActionModel = new LogActionModel("Xóa Hồ Sơ" + data.Code, "TdcPriceRent", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.DELETED, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (data.Id > 0) transaction.Commit();
                        else transaction.Rollback();

                        def.meta = new Meta(200, ApiConstants.MessageResource.DELETE_SUCCESS);
                        def.data = data;
                        return Ok(def);

                    }
                    catch (DbUpdateException e)
                    {
                        transaction.Rollback();
                        log.Error("DbUpdateException:" + e);
                        if (!TdcPriceRentExists(data.Id))
                        {
                            def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_DELETE_MESSAGE);
                            return Ok(def);
                        }
                        else
                        {
                            def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                            return Ok(def);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [HttpPost("ImportExcel/{Id}")]
        public async Task<IActionResult> ImportExcel(int id, [FromBody] List<TdcPriceRentMetaData> input)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            DefaultResponse def = new DefaultResponse();

            var identity = (ClaimsIdentity)User.Identity;
            int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id").Select(c => c.Value).SingleOrDefault());
            string fullName = identity.Claims.Where(c => c.Type == "FullName").Select(c => c.Value).SingleOrDefault() ?? string.Empty;
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.CREATE))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_CREATE_MESSAGE);
                return Ok(def);
            }
            try
            {
                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        List<TdcPriceRentExcelMeta> lst = _context.TdcPriceRentExcelMetas.Where(x => x.TdcPriceRentId == id && x.Status != AppEnums.EntityStatus.DELETED).ToList();
                        lst.ForEach(x => x.Status = AppEnums.EntityStatus.DELETED);
                        _context.UpdateRange(lst);
                        await _context.SaveChangesAsync();

                        foreach (var item in input)
                        {
                            item.TdcPriceRentId = id;
                            item.CreatedById = userId;
                            item.CreatedBy = fullName;
                            _context.TdcPriceRentExcelMetas.Add(item);

                            await _context.SaveChangesAsync();

                            //installmentPriceTableTdcs
                            if (item.tdcPriceRentExcelDatas != null)
                            {
                                foreach (var tdcPriceRentExcelData in item.tdcPriceRentExcelDatas)
                                {
                                    tdcPriceRentExcelData.TdcPriceRentExcelMetaId = item.Id;
                                    tdcPriceRentExcelData.CreatedBy = fullName;
                                    tdcPriceRentExcelData.CreatedById = userId;

                                    _context.TdcPriceRentExcelDatas.Add(tdcPriceRentExcelData);
                                }
                            }
                            //thêm LogAction
                            LogActionModel logActionModel = new LogActionModel("Thêm data file excel import", "tdcPriceExcel", item.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
                            LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                            _context.Add(logAction);
                            await _context.SaveChangesAsync();
                        }

                        transaction.Commit();
                        def.meta = new Meta(200, ApiConstants.MessageResource.ADD_SUCCESS);
                        def.data = input;
                    }
                    catch (DbUpdateException e)
                    {
                        log.Error("DbUpdateException:" + e);
                        transaction.Rollback();
                        def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                    }

                    return Ok(def);
                }
            }
            catch (Exception e)
            {
                log.Error("Post Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [HttpPost("ExportExcel/{Id}")]
        public async Task<IActionResult> ExportExcel(int id, [FromBody] List<TdcPriceRentExcelGroupByPayTimeId> input)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            DefaultResponse def = new DefaultResponse();

            //check role
            var identity = (ClaimsIdentity)User.Identity;
            int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id").Select(c => c.Value).SingleOrDefault());
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();

            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.CREATE))
            {
                return new FileContentResult(new byte[0], "application/octet-stream");
            }

            //Lấy ra hố sơ bán trả góp

            TdcPriceRent dataTdc = _context.TdcPriceRents.Where(x => x.Id == id && x.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

            if (dataTdc == null)
            {
                def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                return Ok(def);
            }
            TdcPriceRentData mapper_dataTdc = _mapper.Map<TdcPriceRentData>(dataTdc);

            mapper_dataTdc.TdcCustomerName = _context.TdcCustomers.Where(f => f.Id == mapper_dataTdc.TdcCustomerId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.FullName).FirstOrDefault();

            mapper_dataTdc.TdcProjectName = _context.TDCProjects.Where(f => f.Id == mapper_dataTdc.TdcProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

            mapper_dataTdc.TdcLandName = _context.Lands.Where(f => f.Id == mapper_dataTdc.LandId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

            mapper_dataTdc.TdcBlockHouseName = _context.BlockHouses.Where(f => f.Id == mapper_dataTdc.BlockHouseId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

            mapper_dataTdc.TdcFloorName = _context.FloorTdcs.Where(f => f.Id == mapper_dataTdc.FloorTdcId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

            mapper_dataTdc.TdcApartmentName = _context.ApartmentTdcs.Where(f => f.Id == mapper_dataTdc.TdcApartmentId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

            // khởi tạo wb rỗng
            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"templates/export.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);

            MemoryStream ms = WriteDataToExcel(templatePath, 0, input, mapper_dataTdc);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", "BangChietTinh_");
        }

        private static MemoryStream WriteDataToExcel(string templatePath, int sheetnumber, List<TdcPriceRentExcelGroupByPayTimeId> data, TdcPriceRentData dataTdc)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);

            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowStart = 10;

            if (sheet != null)
            {
                int datacol = 14;
                try
                {
                    //Ghi Tên Khách Hàng Vào Ô D3
                    IRow rowD3 = sheet.GetRow(2);
                    ICell cellD3 = rowD3.GetCell(3);
                    cellD3.SetCellValue(dataTdc.TdcCustomerName);

                    //Ghi Căn Số Vào Ô D4
                    IRow rowD4 = sheet.GetRow(3);
                    ICell cellD4 = rowD4.GetCell(3);
                    cellD4.SetCellValue(dataTdc.TdcApartmentName);

                    //Ghi Khối vào ô B5
                    IRow rowB5 = sheet.GetRow(4);
                    ICell cellB5 = rowB5.GetCell(1);
                    cellB5.SetCellValue("Khối nhà: " + dataTdc.TdcBlockHouseName);

                    //Ghi Lô vào ô D5
                    IRow rowD5 = sheet.GetRow(4);
                    ICell cellD5 = rowD5.GetCell(3);
                    cellD5.SetCellValue("Lô:" + dataTdc.TdcLandName);

                    //Ghi tên dự án vào ô E5
                    IRow rowE5 = sheet.GetRow(4);
                    ICell cellE5 = rowE5.GetCell(4);
                    cellE5.SetCellValue("Chung cư :" + dataTdc.TdcProjectName);

                    //Ghi số tiền thế chân vào ô H5
                    IRow rowH5 = sheet.GetRow(4);
                    ICell cellH5 = rowH5.GetCell(7);
                    cellH5.SetCellValue((double)dataTdc.PriceTC);

                    //Ghi sô tiền phải nộp hàng tháng vào ô H7
                    IRow rowH7 = sheet.GetRow(6);
                    ICell cellH7 = rowH7.GetCell(7);
                    cellH7.SetCellValue((double)dataTdc.PriceMonth);

                    //Ghi Ngày nộp tiền TC vào F8
                    IRow rowF8 = sheet.GetRow(7);
                    ICell cellF8 = rowF8.GetCell(5);
                    cellF8.SetCellValue(dataTdc.DateTDC);

                    //Ghi Ngày nộp tiền TC vào F5
                    IRow rowF5 = sheet.GetRow(5);
                    ICell cellF5 = rowF5.GetCell(5);
                    cellF5.SetCellValue(dataTdc.DateTTC);


                    List<ICellStyle> rowStyle = new List<ICellStyle>();

                    // Lấy Style excel
                    for (int i = 0; i < datacol; i++)
                    {
                        rowStyle.Add(sheet.GetRow(rowStart).GetCell(i).CellStyle);
                    }

                    //Thêm row
                    int k = 0;
                    foreach (var item in data)
                    {
                        int firstRow = rowStart;
                        foreach (var childItem in item.tdcPriceRentExcels)
                        {
                            XSSFRow row = (XSSFRow)sheet.CreateRow(rowStart);
                            for (int i = 0; i < datacol; i++)
                            {
                                row.CreateCell(i).CellStyle = rowStyle[i];
                                if (i == 0 && childItem.PaymentTimes != null)
                                {
                                    row.GetCell(i).SetCellValue(childItem.PaymentTimes);
                                    if (childItem.PaymentTimes == "Nợ Cũ")
                                    {
                                        row.GetCell(i).CellStyle.Alignment = HorizontalAlignment.General;
                                        row.GetCell(i).SetCellValue("");
                                    }
                                    if (childItem.RowStatus == AppEnums.TypePayQD.NO_CU)
                                    {
                                        row.GetCell(i).CellStyle.Alignment = HorizontalAlignment.General;
                                        row.GetCell(i).SetCellValue("");
                                    }
                                }
                                else if (i == 1)
                                {
                                    if (childItem.PaymentDatePrescribed.HasValue)
                                    {
                                        row.GetCell(i).SetCellValue((DateTime)childItem.PaymentDatePrescribed);
                                    }
                                    if (childItem.RowStatus == AppEnums.TypePayQD.NO_CU)
                                    {
                                        row.GetCell(i).CellStyle.Alignment = HorizontalAlignment.General;
                                        row.GetCell(i).SetCellValue("Nợ Cũ");
                                    }
                                    if (childItem.ExpectedPaymentDate.HasValue && childItem.PaymentDatePrescribed == null && childItem.PricePaymentPeriod.HasValue)
                                    {
                                        row.GetCell(i).CellStyle.Alignment = HorizontalAlignment.General;
                                        row.GetCell(i).SetCellValue("Nợ Cũ");
                                    }
                                }
                                else if (i == 2 && childItem.PaymentDatePrescribed1.HasValue)
                                {
                                    row.GetCell(i).SetCellValue((DateTime)childItem.PaymentDatePrescribed1);
                                }
                                else if (i == 3 && childItem.ExpectedPaymentDate.HasValue)
                                {
                                    row.GetCell(i).SetCellValue((DateTime)childItem.ExpectedPaymentDate);
                                    if (childItem.TypeRow == 2)
                                    {
                                        row.GetCell(i).CellStyle.Alignment = HorizontalAlignment.General;
                                        row.GetCell(i).SetCellValue("");
                                    }
                                }
                                else if (i == 4 && childItem.DailyInterest.HasValue)
                                {
                                    row.GetCell(i).SetCellValue((double)childItem.DailyInterest);
                                }
                                else if (i == 5 && childItem.DailyInterestRate.HasValue)
                                {
                                    row.GetCell(i).SetCellValue((double)childItem.DailyInterestRate);
                                }
                                else if (i == 6 && childItem.UnitPay.HasValue)
                                {
                                    row.GetCell(i).SetCellValue((double)childItem.UnitPay);
                                }
                                else if (i == 7 && childItem.PriceEarnings.HasValue)
                                {
                                    row.GetCell(i).SetCellValue((double)childItem.PriceEarnings);
                                }
                                else if (i == 8 && childItem.PricePaymentPeriod.HasValue)
                                {
                                    row.GetCell(i).SetCellValue((double)childItem.PricePaymentPeriod);
                                }
                                else if (i == 9)
                                {
                                    if (childItem.Pay == null || childItem.Pay == 0)
                                    {
                                        row.GetCell(i).SetCellValue("");
                                    }
                                    else
                                    {
                                        row.GetCell(i).SetCellValue((double)childItem.Pay);
                                    }
                                }
                                else if (i == 10)
                                {
                                    if (childItem.Paid == null || childItem.Paid == 0)
                                    {
                                        row.GetCell(i).SetCellValue("");
                                    }
                                    else
                                    {
                                        row.GetCell(i).SetCellValue((double)childItem.Paid);
                                    }
                                }
                                else if (i == 11)
                                {
                                    if (childItem.PriceDifference == null || childItem.PriceDifference == 0)
                                    {
                                        row.GetCell(i).SetCellValue("");
                                    }
                                    else
                                    {
                                        row.GetCell(i).SetCellValue((double)childItem.PriceDifference);
                                    }
                                }
                                else if (i == 12)
                                {
                                    row.GetCell(i).SetCellValue(childItem.Note);
                                }
                                else if (i == 13)
                                {
                                    if (childItem.PricePublic == null || childItem.PricePublic == false)
                                    {
                                        row.GetCell(i).SetCellValue("");
                                    }
                                    else
                                    {
                                        row.GetCell(i).SetCellValue("Check");
                                    }
                                }
                            }
                            rowStart++;
                            k++;
                        }
                        int lastRow = rowStart - 1;
                        if (lastRow > firstRow)
                        {

                            CellRangeAddress mergedRegionPay = new CellRangeAddress(firstRow, lastRow, 9, 9);
                            sheet.AddMergedRegion(mergedRegionPay);

                            CellRangeAddress mergedRegionPaid = new CellRangeAddress(firstRow, lastRow, 10, 10);
                            sheet.AddMergedRegion(mergedRegionPaid);

                            CellRangeAddress mergedRegionDiff = new CellRangeAddress(firstRow, lastRow, 11, 11);
                            sheet.AddMergedRegion(mergedRegionDiff);

                            CellRangeAddress mergedRegionNote = new CellRangeAddress(firstRow, lastRow, 12, 12);
                            sheet.AddMergedRegion(mergedRegionNote);

                            CellRangeAddress mergedRegionPayPublic = new CellRangeAddress(firstRow, lastRow, 13, 13);
                            sheet.AddMergedRegion(mergedRegionPayPublic);

                        }
                        IRow rowMerge = sheet.GetRow(firstRow);
                        if (item.Pay.HasValue)
                        {
                            ICell cellPay = rowMerge.GetCell(9);
                            cellPay.SetCellValue((double)item.Pay);
                        }
                        if (item.Paid.HasValue)
                        {
                            ICell cellPaid = rowMerge.GetCell(10);
                            cellPaid.SetCellValue((double)item.Paid);
                        }
                        if (item.PriceDifference.HasValue)
                        {
                            ICell cellDiff = rowMerge.GetCell(11);
                            cellDiff.SetCellValue((double)item.PriceDifference);
                        }
                    }
                    if (data.Count > 0)
                    {
                        // Bằng số dòng file template
                        int rowFooter = 10;
                        foreach (var i in data)
                        {
                            //Cộng số bản ghi đã ghi vào excel
                            rowFooter += i.tdcPriceRentExcels.Count;
                        }
                        int rowFooter2 = rowFooter + 2;
                        int rowFooter3 = rowFooter + 3;
                        int rowFooter4 = rowFooter + 4;

                        ISheet sheet1 = workbook.GetSheetAt(sheetnumber);
                        ISheet sheet2 = workbook.GetSheetAt(sheetnumber);
                        ISheet sheet3 = workbook.GetSheetAt(sheetnumber);
                        ISheet sheet4 = workbook.GetSheetAt(sheetnumber);
                        ISheet sheet5 = workbook.GetSheetAt(sheetnumber);

                        //Set Style 
                        ICellStyle cellStyle1 = workbook.CreateCellStyle();
                        IFont font = workbook.CreateFont();
                        font.Boldweight = (short)FontBoldWeight.Bold;
                        font.Color = IndexedColors.Black.Index;

                        cellStyle1.SetFont(font);
                        cellStyle1.Alignment = HorizontalAlignment.Center;

                        for (int i = 0; i < datacol; i++)
                        {
                            if (rowFooter == rowFooter2)
                            {
                                XSSFRow rowsFooter4 = (XSSFRow)sheet1.CreateRow(rowFooter);
                                ICell cell5 = rowsFooter4.CreateCell(10);
                                cell5.SetCellValue("TP. Hồ Chí Minh, Ngày " + "       " + " tháng " + "      " + " năm 20");
                                cell5.CellStyle = cellStyle1;
                            }
                            if (rowFooter == rowFooter3)
                            {
                                XSSFRow rowsFooter2 = (XSSFRow)sheet2.CreateRow(rowFooter);
                                ICell cell2 = rowsFooter2.CreateCell(7);
                                cell2.SetCellValue("PHÒNG QUẢN LÝ NHÀ Ở");
                                cell2.CellStyle = cellStyle1;

                                ICell cell6 = rowsFooter2.CreateCell(10);
                                cell6.SetCellValue("PHÓ GIÁM ĐỐC");
                                cell6.CellStyle = cellStyle1;
                            }
                            if (rowFooter == rowFooter4)
                            {
                                XSSFRow rowsFooter = (XSSFRow)sheet4.CreateRow(rowFooter);
                                ICell cell = rowsFooter.CreateCell(1);
                                cell.SetCellValue("NGƯỜI LẬP");
                                cell.CellStyle = cellStyle1;

                                ICell cell3 = rowsFooter.CreateCell(7);
                                cell3.SetCellValue("TRƯỞNG PHÒNG");
                                cell3.CellStyle = cellStyle1;
                            }
                            rowFooter++;
                        }
                        CellRangeAddress mergedRegionSheet1 = new CellRangeAddress(rowFooter2, rowFooter2, 10, 12);
                        sheet1.AddMergedRegion(mergedRegionSheet1);

                        CellRangeAddress mergedRegionSheet2 = new CellRangeAddress(rowFooter3, rowFooter3, 7, 8);
                        sheet2.AddMergedRegion(mergedRegionSheet2);

                        CellRangeAddress mergedRegionSheet3 = new CellRangeAddress(rowFooter3, rowFooter3, 10, 12);
                        sheet3.AddMergedRegion(mergedRegionSheet3);

                        CellRangeAddress mergedRegionSheet4 = new CellRangeAddress(rowFooter4, rowFooter4, 1, 3);
                        sheet4.AddMergedRegion(mergedRegionSheet4);

                        CellRangeAddress mergedRegionSheet5 = new CellRangeAddress(rowFooter4, rowFooter4, 7, 8);
                        sheet5.AddMergedRegion(mergedRegionSheet5);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Put Exception:" + ex);
                }
            }
            sheet.ForceFormulaRecalculation = true;

            MemoryStream ms = new MemoryStream();

            workbook.Write(ms);

            return ms;

        }

        [HttpPost("GetReportTable")]
        public async Task<IActionResult> GetReportTable([FromBody] List<TdcPriceRent> input)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            DefaultResponse def = new DefaultResponse();
            //check role
            var identity = (ClaimsIdentity)User.Identity;
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }
            try
            {
                List<TdcPriceRentReport> resReport = new List<TdcPriceRentReport>();

                //Tìm hồ sơ cho thuê
                foreach (var items in input)
                {
                    TdcPriceRentReport dataReport = new TdcPriceRentReport();

                    TdcPriceRent tdcPriceRent = _context.TdcPriceRents.Where(l => l.Id == items.Id && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                    if (tdcPriceRent == null)
                    {
                        def.meta = new Meta(404, "Không tìm thấy hồ sơ tương ứng!");
                        return Ok(def);
                    }
                    bool checkImp = false;
                    TdcPriceRentMetaData dataLast = null;
                    TdcPriceRentMetaData dataFist = null;
                    int? PaymentTimesLast = 0;
                    decimal? unitPay = 0;
                    DateTime? DateExpectedLast = null;
                    DateTime? DateExpectedFist = null;
                    DateTime? DatePrescribedLast = null;
                    int countYear = 0;

                    decimal? NCUnitPay = 0;
                    decimal? NCPriceEarnings = 0;
                    decimal? NCPricePaymentPeriod = 0;
                    decimal? NCPay = 0;
                    decimal? NCPaid = 0;
                    decimal? Paid = 0;

                    List<TdcPriceRentExcelMeta> lst = _context.TdcPriceRentExcelMetas.Where(f => f.TdcPriceRentId == items.Id && f.Status != AppEnums.EntityStatus.DELETED).ToList();
                    List<TdcPriceRentMetaData> metaDataImport_mapper = _mapper.Map<List<TdcPriceRentMetaData>>(lst);

                    if (lst.Count > 0)
                    {
                        checkImp = true;

                        foreach (var itemImport in metaDataImport_mapper)
                        {
                            List<TdcPriceRentExcelData> tdcPriceRentMetaDatas = _context.TdcPriceRentExcelDatas.Where(l => l.TdcPriceRentExcelMetaId == itemImport.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.index).ToList();
                            itemImport.tdcPriceRentExcelDatas = tdcPriceRentMetaDatas;

                            NCUnitPay = NCUnitPay + itemImport.tdcPriceRentExcelDatas.Sum(x => x.UnitPay);
                            NCPriceEarnings = NCPriceEarnings + itemImport.tdcPriceRentExcelDatas.Sum(x => x.PriceEarnings);
                            NCPricePaymentPeriod = NCPricePaymentPeriod + itemImport.tdcPriceRentExcelDatas.Sum(x => x.PricePaymentPeriod);
                            NCPay = NCPricePaymentPeriod;
                            NCPaid = Paid = Paid + itemImport.tdcPriceRentExcelDatas.Sum(x => x.Paid);
                        }

                        dataLast = metaDataImport_mapper[metaDataImport_mapper.Count() - 1];
                        dataFist = metaDataImport_mapper[0];
                        PaymentTimesLast = int.Parse(dataLast.tdcPriceRentExcelDatas[dataLast.tdcPriceRentExcelDatas.Count() - 1].PaymentTimes);
                        DateExpectedLast = dataLast.tdcPriceRentExcelDatas[dataLast.tdcPriceRentExcelDatas.Count() - 1].ExpectedPaymentDate;
                        DateExpectedFist = dataFist.tdcPriceRentExcelDatas[0].ExpectedPaymentDate;
                        DatePrescribedLast = dataLast.tdcPriceRentExcelDatas[dataLast.tdcPriceRentExcelDatas.Count() - 1].PaymentDatePrescribed;
                        countYear = (int)dataFist.tdcPriceRentExcelDatas[0].CountYear;
                        Paid = Paid - dataFist.tdcPriceRentExcelDatas[0].Paid;

                    }
                    List<TdcPriceRentExcel> data = new List<TdcPriceRentExcel>(tdcPriceRent.MonthRent + 2);
                    List<TdcPriceRentExcel> dataImport = new List<TdcPriceRentExcel>();
                    List<TdcPriceRentPay> tdcPriceRentPays = _context.TdcPriceRentPays.Where(l => l.TdcPriceRentId == tdcPriceRent.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    List<ProfitValue> profitValues = _context.ProfitValues.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderBy(p => p.DoApply).ToList();

                    decimal? noCu = 0;
                    int k = 0;
                    int kImp = 0;

                    TypePayQD check;
                    DateTime date1 = new DateTime(2021, 06, 10);

                    int paytimeIdImport = 0;
                    if (tdcPriceRentPays.Count() != 0)
                    {
                        paytimeIdImport = (int)(tdcPriceRentPays[tdcPriceRentPays.Count() - 1].Id + 999.99);
                    }
                    else paytimeIdImport = paytimeIdImport;

                    if (checkImp)
                    {
                        foreach (var itemImport in metaDataImport_mapper)
                        {
                            paytimeIdImport++;

                            foreach (var item in itemImport.tdcPriceRentExcelDatas)
                            {

                                TdcPriceRentExcel itemDataImport = new TdcPriceRentExcel();

                                itemDataImport = new TdcPriceRentExcel(paytimeIdImport, true, item.PaymentTimes, item.PaymentDatePrescribed, item.PaymentDatePrescribed1, 0, item.ExpectedPaymentDate, item.Note, item.DailyInterest, (double?)item.DailyInterestRate, item.UnitPay, item.PriceEarnings, item.PricePaymentPeriod, item.Pay, item.Paid, item.PriceDifference, 5, false, true);
                                if (item.PaymentTimes == null)
                                {
                                    itemDataImport = new TdcPriceRentExcel(paytimeIdImport, true, "Nợ Cũ", item.PaymentDatePrescribed, item.PaymentDatePrescribed1, 0, item.ExpectedPaymentDate, item.Note, item.DailyInterest, (double?)item.DailyInterestRate, item.UnitPay, item.PriceEarnings, item.PricePaymentPeriod, item.Pay, item.Paid, item.PriceDifference, 4, false, true);
                                }
                                if (item.PricePaymentPeriod == null)
                                {
                                    itemDataImport = new TdcPriceRentExcel(paytimeIdImport, true, item.PaymentTimes, item.PaymentDatePrescribed, item.PaymentDatePrescribed1, 0, item.ExpectedPaymentDate, item.Note, item.DailyInterest, (double?)item.DailyInterestRate, item.UnitPay, item.PriceEarnings, item.PricePaymentPeriod, item.Pay, item.Paid, item.PriceDifference, 2, true, true);
                                }

                                data.Add(itemDataImport);

                                dataImport.Add(itemDataImport);
                            }
                        }

                        TdcPriceRentExcel fist = new TdcPriceRentExcel(-paytimeIdImport, true, "Chốt đến" + " " + DateExpectedLast?.ToString("dd/MM/yyyy"), null, null, 1, null, null, null, null, NCUnitPay, NCPriceEarnings, NCPricePaymentPeriod, NCPricePaymentPeriod, Paid, null, 1, true, false);
                        data.Add(fist);

                        TdcPriceRentExcel second = new TdcPriceRentExcel();
                        second.PaymentTimes = "Nợ cũ sau thanh lý";
                        second.Year = 2;
                        second.TypeRow = 1;
                        second.Status = true;
                        second.Check = false;

                        //Lấy ngày sau thanh lý HĐ
                        if (tdcPriceRentPays.Count > 0)
                        {
                            second.PaymentDatePrescribed1 = tdcPriceRentPays[0].PaymentDate;
                            // Lấy DateTime bên trong kiểu Nullable DateTime
                            DateTime start = DateExpectedLast.Value;
                            DateTime end = second.PaymentDatePrescribed1.Value;
                            // Lấy TimeSpan giữa hai ngày
                            TimeSpan span = end.Subtract(start);
                            // Lấy số ngày chênh lệch
                            second.DailyInterest = (int)span.TotalDays;
                            if (second.DailyInterest < 0) second.DailyInterest = 0;
                        }
                        else
                        {
                            second.DailyInterest = 0;
                        }
                        //Lấy Lãi Suất
                        foreach (var profitValue in profitValues)
                        {
                            if (second.PaymentDatePrescribed1 < profitValue.DoApply) break;
                            second.DailyInterestRate = profitValue.Value;
                        }

                        //Thời gian tính lãi theo ngày
                        foreach (var itemImport in metaDataImport_mapper)
                        {
                            List<TdcPriceRentExcelData> tdcPriceRentMetaDatas = _context.TdcPriceRentExcelDatas.Where(l => l.TdcPriceRentExcelMetaId == itemImport.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.index).ToList();
                            itemImport.tdcPriceRentExcelDatas = tdcPriceRentMetaDatas;

                            foreach (var i in itemImport.tdcPriceRentExcelDatas)
                            {
                                if (i.ExpectedPaymentDate == DateExpectedLast && i.UnitPay != null)
                                {
                                    unitPay = unitPay + i.UnitPay;
                                    second.UnitPay = unitPay;
                                }
                            }
                        }
                        second.PriceEarnings = Math.Round(((decimal)second.DailyInterest * (decimal)second.DailyInterestRate * (decimal)second.UnitPay) / 100);
                        second.PricePaymentPeriod = second.PriceEarnings;
                        data.Add(second);
                    }
                    else
                    {
                        TdcPriceRentExcel fist = new TdcPriceRentExcel(0101, true, "Tiền thế chân", null, null, 2, tdcPriceRent.DateTTC, null, null, null, null, null, null, null, tdcPriceRent.PriceTC, null, 1, true, false);
                        data.Add(fist);

                        TdcPriceRentExcel second = new TdcPriceRentExcel(0100, true, "Năm thứ 1", null, null, 2, null, null, null, null, null, null, null, null, null, null, 1, true, false);
                        data.Add(second);
                    }

                    if (checkImp == false)
                    {
                        TdcPriceRentPay firstPay = new TdcPriceRentPay();
                        firstPay.PayCount = 1;
                        firstPay.PaymentDate = tdcPriceRent.DateTTC;
                        firstPay.AmountPaid = tdcPriceRent.TotalPriceTT.GetValueOrDefault() / tdcPriceRent.MonthRent;
                        if (firstPay.PaymentDate > tdcPriceRent.DecisionDateCT) firstPay.AmountPaid = tdcPriceRent.TotalPriceCT / tdcPriceRent.MonthRent;
                        tdcPriceRentPays.Insert(0, firstPay);
                    }

                    decimal UnitPayTT = Math.Round(tdcPriceRent.TotalPriceTT.GetValueOrDefault() / tdcPriceRent.MonthRent);
                    decimal UnitPayCT = Math.Round(tdcPriceRent.TotalPriceCT / tdcPriceRent.MonthRent);

                    decimal? TotalUnitPay = 0;
                    decimal? TotalPriceEarnings = 0;
                    decimal? TotalPricePaymentPeriod = 0;
                    decimal? TotalPay = 0;

                    DateTime date = new DateTime(2019, 12, 31);
                    int PayCount = 0;
                    int payStatus = 0;
                    int monthRent = 0;

                    int j = 0;//để check nợ cũ

                    if (checkImp == true)
                    {
                        int monthDiff = ((DatePrescribedLast.Value.Year - DateExpectedFist.Value.Year) * 12) + (DatePrescribedLast.Value.Month - DateExpectedFist.Value.Month) + 1;
                        monthRent = tdcPriceRent.MonthRent - monthDiff;
                        j = dataImport.Count + 1; // nếu có nợ cũ thì gắn bằng số kì của nợ cũ
                    }
                    else
                    {
                        monthRent = tdcPriceRent.MonthRent;
                        j = 1;//Ko có gắn bằng 1
                    }

                    for (int i = 0; i < monthRent; i++)
                    {
                        check = AppEnums.TypePayQD.DUNG_HAN;
                        TdcPriceRentExcel tdcPriceRentExcel = new TdcPriceRentExcel();
                        tdcPriceRentExcel.Status = false;

                        if (data[j] != null)
                        {
                            if (data[j].PriceDifference > 0)
                            {
                                check = AppEnums.TypePayQD.NO_CU;
                                noCu = data[j].PriceDifference;

                            }
                            if (data[j].PriceDifference < 0)
                            {
                                // đóng dư tiền
                                check = AppEnums.TypePayQD.DONG_DU;
                            }
                            if (data[j].PriceDifference == null) data[j].PriceDifference = 0;
                        }

                        if (k < tdcPriceRentPays.Count())
                        {
                            //Id lần trả
                            tdcPriceRentExcel.PayTimeId = tdcPriceRentPays[k].Id;
                            //Ngày thanh toán thực tế
                            tdcPriceRentExcel.ExpectedPaymentDate = tdcPriceRentPays[k].PaymentDate;
                            tdcPriceRentExcel.PricePublic = tdcPriceRentPays[k].PricePublic;
                            if (PayCount == 0 && tdcPriceRentPays.Count != 0)
                            {
                                PayCount = tdcPriceRentPays[k].PayCount;
                            }

                            //Số tiền đã thanh toán
                            if (payStatus == 0)
                            {
                                tdcPriceRentExcel.Paid = tdcPriceRentPays[k].AmountPaid;
                            }
                            else
                            {
                                tdcPriceRentExcel.Paid = 0;
                            }
                            tdcPriceRentExcel.Status = true;
                        }
                        else
                        {
                            tdcPriceRentExcel.Status = false;
                        }

                        j++;
                        //Số lần 
                        if (checkImp == true)
                        {
                            tdcPriceRentExcel.PaymentTimes = ((PaymentTimesLast + 1 + i) % 12).ToString();
                            if (tdcPriceRentExcel.PaymentTimes == "0") tdcPriceRentExcel.PaymentTimes = "12";
                        }
                        else
                        {
                            tdcPriceRentExcel.PaymentTimes = ((i + 1) % 12).ToString();
                            if (tdcPriceRentExcel.PaymentTimes == "0") tdcPriceRentExcel.PaymentTimes = "12";
                        }
                        tdcPriceRentExcel.TypeRow = 3;

                        //Lấy ngày thanh toán theo quy định
                        if (checkImp == true)
                        {
                            tdcPriceRentExcel.PaymentDatePrescribed = DatePrescribedLast.Value.AddMonths(i + 1);
                        }
                        else
                        {
                            TdcPriceRent firstPayDate = _context.TdcPriceRents.Where(l => l.Id == tdcPriceRent.Id).FirstOrDefault();
                            tdcPriceRentExcel.PaymentDatePrescribed = firstPayDate.DateTTC.AddMonths(i);
                        }

                        //Thời gian tính lãi theo ngày
                        if (tdcPriceRentExcel.ExpectedPaymentDate.HasValue && tdcPriceRentExcel.ExpectedPaymentDate.HasValue)
                        {
                            // Lấy DateTime bên trong kiểu Nullable DateTime
                            DateTime start = tdcPriceRentExcel.PaymentDatePrescribed.Value;
                            DateTime end = tdcPriceRentExcel.ExpectedPaymentDate.Value;
                            // Lấy TimeSpan giữa hai ngày
                            TimeSpan span = end.Subtract(start);
                            // Lấy số ngày chênh lệch
                            tdcPriceRentExcel.DailyInterest = (int)span.TotalDays;
                            if (tdcPriceRentExcel.DailyInterest < 0) tdcPriceRentExcel.DailyInterest = 0;
                        }
                        else
                        {
                            tdcPriceRentExcel.DailyInterest = 0;
                        }

                        //Lãi suất tính theo ngày
                        foreach (var profitValue in profitValues)
                        {
                            if (tdcPriceRentExcel.PaymentDatePrescribed < profitValue.DoApply) break;
                            tdcPriceRentExcel.DailyInterestRate = profitValue.Value;
                        }

                        //Số tiền phải trả từng tháng 
                        tdcPriceRentExcel.UnitPay = UnitPayTT;
                        if (tdcPriceRentExcel.PaymentDatePrescribed > tdcPriceRent.DecisionDateCT) tdcPriceRentExcel.UnitPay = UnitPayCT;

                        TotalUnitPay += tdcPriceRentExcel.UnitPay;

                        //Số tiền lãi phát sinh do chậm thanh toán                   
                        if (k < tdcPriceRentPays.Count())
                            tdcPriceRentExcel.PriceEarnings = Math.Round(((decimal)tdcPriceRentExcel.DailyInterest * (decimal)tdcPriceRentExcel.DailyInterestRate * (decimal)tdcPriceRentExcel.UnitPay) / 100);

                        if (tdcPriceRentExcel.PriceEarnings != null)
                        {
                            TotalPriceEarnings += tdcPriceRentExcel.PriceEarnings;
                        }

                        //Số tiền đến kỳ phải thanh toán
                        tdcPriceRentExcel.PricePaymentPeriod = (tdcPriceRentExcel.PriceEarnings + tdcPriceRentExcel.UnitPay);

                        if (tdcPriceRentExcel.PricePaymentPeriod != null)
                        {
                            TotalPricePaymentPeriod += tdcPriceRentExcel.PricePaymentPeriod;
                        }

                        //Số tiền phải thanh toán
                        if (data[j - 1] != null)
                        {
                            tdcPriceRentExcel.Pay = tdcPriceRentExcel.PricePaymentPeriod + data[j - 1].PriceDifference;
                        }
                        else
                        {
                            tdcPriceRentExcel.Pay = tdcPriceRentExcel.PricePaymentPeriod;
                        }
                        if (tdcPriceRentExcel.Pay.HasValue)
                        {
                            TotalPay += tdcPriceRentExcel.Pay;
                        }

                        //Số tiền đã thanh toán
                        if (k < tdcPriceRentPays.Count())
                            tdcPriceRentExcel.Paid = tdcPriceRentPays[k].AmountPaid;
                        if (check == AppEnums.TypePayQD.DONG_DU)
                        {
                            if (data[j - 1].PriceDifference < 0 && data[j - 1].PayTimeId != tdcPriceRentExcel.PayTimeId)
                            {
                                tdcPriceRentExcel.Pay = tdcPriceRentExcel.PricePaymentPeriod + data[j - 1].PriceDifference;
                                if (k < tdcPriceRentPays.Count())
                                    tdcPriceRentExcel.Paid = tdcPriceRentPays[k].AmountPaid;
                            }
                            else
                            {
                                tdcPriceRentExcel.Pay = tdcPriceRentExcel.PricePaymentPeriod;
                                tdcPriceRentExcel.Paid = -data[j - 1].PriceDifference;
                            }

                            if (tdcPriceRentExcel.Pay.HasValue)
                            {
                                TotalPay += tdcPriceRentExcel.Pay;
                            }
                        }

                        //Chênh lệch
                        tdcPriceRentExcel.PriceDifference = tdcPriceRentExcel.Pay - tdcPriceRentExcel.Paid;

                        if (tdcPriceRentExcel.PriceDifference.HasValue)
                        {
                            tdcPriceRentExcel.Status = true;
                        }
                        else
                        {
                            DateTime start = tdcPriceRentExcel.PaymentDatePrescribed.Value;
                            if (tdcPriceRentExcel.ExpectedPaymentDate.HasValue)
                            {
                                DateTime end = tdcPriceRentExcel.ExpectedPaymentDate.Value;
                                TimeSpan span = end.Subtract(start);
                                tdcPriceRentExcel.DailyInterest = (int)span.TotalDays;
                            }
                            // Lấy TimeSpan giữa hai ngày
                            // Lấy số ngày chênh lệch

                            if (tdcPriceRentExcel.DailyInterest < 0) tdcPriceRentExcel.DailyInterest = 0;

                            //Số tiền lãi phát sinh do chậm thanh toán 
                            tdcPriceRentExcel.PriceEarnings = Math.Round(((decimal)tdcPriceRentExcel.DailyInterest * (decimal)tdcPriceRentExcel.DailyInterestRate * (decimal)tdcPriceRentExcel.UnitPay) / 100);
                            if (tdcPriceRentExcel.PriceEarnings != null)
                            {
                                TotalPriceEarnings += tdcPriceRentExcel.PriceEarnings;
                            }

                            //Số tiền đến kỳ phải thanh toán
                            tdcPriceRentExcel.PricePaymentPeriod = (tdcPriceRentExcel.PriceEarnings + tdcPriceRentExcel.UnitPay);

                            if (tdcPriceRentExcel.PricePaymentPeriod != null)
                            {
                                TotalPricePaymentPeriod += tdcPriceRentExcel.PricePaymentPeriod;
                            }

                            if (data[j - 1] != null)
                            {
                                tdcPriceRentExcel.Pay = tdcPriceRentExcel.PricePaymentPeriod + data[j - 1].PriceDifference;
                            }
                            else
                            {
                                tdcPriceRentExcel.Pay = tdcPriceRentExcel.PricePaymentPeriod;
                            }

                            if (tdcPriceRentExcel.Pay.HasValue)
                            {
                                TotalPay += tdcPriceRentExcel.Pay;
                            }
                        }
                        data.Add(tdcPriceRentExcel);

                        if (check == AppEnums.TypePayQD.NO_CU)
                        {
                            //Thêm  nợ cũ
                            TdcPriceRentExcel tdcPriceRentExcelNoCu = new TdcPriceRentExcel();
                            j++;
                            if (k < tdcPriceRentPays.Count())
                            {
                                tdcPriceRentExcelNoCu.PayTimeId = tdcPriceRentPays[k].Id;
                                PayCount--;
                            }

                            tdcPriceRentExcelNoCu.TypeRow = 4;

                            tdcPriceRentExcelNoCu.Status = true;

                            tdcPriceRentExcelNoCu.PaymentTimes = "Nợ cũ";

                            tdcPriceRentExcelNoCu.RowStatus = AppEnums.TypePayQD.NO_CU;

                            if (k < tdcPriceRentPays.Count())
                                tdcPriceRentExcelNoCu.ExpectedPaymentDate = tdcPriceRentPays[k].PaymentDate;

                            //Thời gian tính lãi theo ngày
                            if (k < tdcPriceRentPays.Count())
                            {
                                DateTime start = tdcPriceRentExcelNoCu.ExpectedPaymentDate.Value;
                                DateTime end = data[j - 2].ExpectedPaymentDate.Value;
                                TimeSpan span = start.Subtract(end);
                                tdcPriceRentExcelNoCu.DailyInterest = (int)span.TotalDays;
                                if (tdcPriceRentExcelNoCu.DailyInterest < 0) tdcPriceRentExcelNoCu.DailyInterest = 0;
                            }

                            //Lãi suất
                            tdcPriceRentExcelNoCu.DailyInterestRate = tdcPriceRentExcel.DailyInterestRate;

                            //Số tiền lãi phát sinh do chậm thanh toán 
                            if (k < tdcPriceRentPays.Count())
                            {
                                tdcPriceRentExcelNoCu.PriceEarnings = Math.Round(((decimal)tdcPriceRentExcelNoCu.DailyInterest * (decimal)tdcPriceRentExcelNoCu.DailyInterestRate * (decimal)noCu) / 100);
                                TotalPriceEarnings += tdcPriceRentExcelNoCu.PriceEarnings;
                            }

                            //Số tiền đến kỳ phải thanh toán
                            tdcPriceRentExcelNoCu.PricePaymentPeriod = tdcPriceRentExcelNoCu.PriceEarnings;

                            TotalPricePaymentPeriod += tdcPriceRentExcelNoCu.PricePaymentPeriod;

                            if (data[j - 1].PriceDifference > 0)
                            {
                                tdcPriceRentExcelNoCu.Pay = tdcPriceRentExcelNoCu.PricePaymentPeriod + data[j - 1].PriceDifference;
                                tdcPriceRentExcelNoCu.Paid = 0;
                                tdcPriceRentExcelNoCu.PriceDifference = tdcPriceRentExcelNoCu.Pay;
                            }
                            else
                            {
                                tdcPriceRentExcelNoCu.Pay = tdcPriceRentExcelNoCu.PricePaymentPeriod;
                                tdcPriceRentExcelNoCu.Paid = tdcPriceRentExcelNoCu.Pay;
                                tdcPriceRentExcelNoCu.PriceDifference = tdcPriceRentExcelNoCu.Pay + data[j - 1].PriceDifference;
                            }

                            if (tdcPriceRentExcelNoCu.Pay.HasValue)
                            {
                                TotalPay += tdcPriceRentExcelNoCu.Pay;
                            }

                            if (k < tdcPriceRentPays.Count())
                                tdcPriceRentExcelNoCu.PayTimeId = tdcPriceRentPays[k].Id;
                            data.Add(tdcPriceRentExcelNoCu);
                        }

                        decimal checkyear;
                        if (checkImp == true)
                        {
                            checkyear = (PaymentTimesLast.Value + i + 1) % 12;
                        }
                        else
                        {
                            checkyear = (i + 1) % 12;
                        }

                        if (checkyear == 0)
                        {
                            j++;
                            TdcPriceRentExcel Year = new TdcPriceRentExcel();
                            int year = 0;

                            if (checkImp == true)
                            {
                                year = countYear + ((i + 1) / 12) + 1;
                            }
                            else
                            {
                                year = ((i + 1) / 12) + 1;
                            }

                            Year.PaymentTimes = "Năm thứ " + year;
                            Year.TypeRow = 2;
                            Year.Year = 2;
                            Year.PriceDifference = data[j - 1].PriceDifference;
                            Year.ExpectedPaymentDate = data[j - 1].ExpectedPaymentDate;
                            Year.PayTimeId = data[j - 1].PayTimeId;
                            Year.Check = true;
                            if (data[j - 1].Status == true)
                            {
                                Year.Status = true;
                            }
                            else Year.Status = false;
                            data.Add(Year);
                        }

                        if (k == 0 && PayCount == 1) PayCount = 0;
                        if (PayCount == 0 && tdcPriceRentPays.Count != 0)
                        {
                            k++;
                        }
                        else
                        {
                            PayCount--;
                            if (PayCount == 0) k = k + 1;
                        }

                    }
                    //Thế Chân
                    TdcPriceRentExcel tdcPriceRentExcelTCT = new TdcPriceRentExcel();
                    tdcPriceRentExcelTCT.PaymentTimes = "Tiền Thế Chân Tăng";
                    tdcPriceRentExcelTCT.Year = 2;
                    tdcPriceRentExcelTCT.TypeRow = 1;
                    decimal TCTT = 12 * (tdcPriceRent.TotalPriceTT.GetValueOrDefault() / tdcPriceRent.MonthRent);
                    decimal TCCT = 12 * (tdcPriceRent.TotalPriceCT / tdcPriceRent.MonthRent);
                    tdcPriceRentExcelTCT.Pay = TCCT - TCTT;
                    tdcPriceRentExcelTCT.Paid = null;
                    tdcPriceRentExcelTCT.Status = true;
                    tdcPriceRentExcelTCT.Check = true;
                    data.Add(tdcPriceRentExcelTCT);

                    //Truy Thu
                    TdcPriceRentExcel tdcPriceRentExcelLast = new TdcPriceRentExcel();
                    tdcPriceRentExcelLast.Year = 1;
                    tdcPriceRentExcelLast.TypeRow = 1;
                    tdcPriceRentExcelLast.UnitPay = (tdcPriceRent.TotalPriceCT / tdcPriceRent.MonthRent) - (tdcPriceRent.TotalPriceTT.GetValueOrDefault() / tdcPriceRent.MonthRent);
                    //Tháng Chênh Lệch
                    int monthsDiff = ((tdcPriceRent.DecisionDateCT.Year - tdcPriceRent.DateTTC.Year) * 12) + (tdcPriceRent.DecisionDateCT.Month - tdcPriceRent.DateTTC.Month);
                    tdcPriceRentExcelLast.PaymentTimes = "(" + monthsDiff + "tháng )" + " " + "Truy Thu từ " + tdcPriceRent.DateTTC.ToString("dd/MM/yyyy") + "  " + "đến" + "  " + tdcPriceRent.DecisionDateCT.ToString("dd/MM/yyyy");
                    tdcPriceRentExcelLast.PricePaymentPeriod = tdcPriceRentExcelLast.UnitPay * monthsDiff;
                    tdcPriceRentExcelLast.Pay = tdcPriceRentExcelLast.PricePaymentPeriod;
                    tdcPriceRentExcelLast.Paid = null;
                    tdcPriceRentExcelLast.Status = true;
                    tdcPriceRentExcelLast.Check = true;
                    data.Add(tdcPriceRentExcelLast);

                    //Tổng
                    TdcPriceRentExcel tdcPriceRentExcelTotal = new TdcPriceRentExcel();
                    tdcPriceRentExcelTotal.PaymentTimes = "Tổng";
                    tdcPriceRentExcelTotal.Year = 2;
                    tdcPriceRentExcelTotal.TypeRow = 1;
                    tdcPriceRentExcelTotal.UnitPay = NCUnitPay + TotalUnitPay;
                    tdcPriceRentExcelTotal.PriceEarnings = NCPriceEarnings + TotalPriceEarnings;
                    tdcPriceRentExcelTotal.PricePaymentPeriod = NCPricePaymentPeriod + TotalPricePaymentPeriod + tdcPriceRentExcelLast.PricePaymentPeriod;
                    tdcPriceRentExcelTotal.Pay = NCPricePaymentPeriod + TotalPay + tdcPriceRentExcelLast.Pay + tdcPriceRentExcelTCT.Pay;
                    tdcPriceRentExcelTotal.Paid = tdcPriceRentExcelTotal.Pay - NCPaid;
                    tdcPriceRentExcelTotal.Status = true;
                    tdcPriceRentExcelTotal.Check = true;
                    data.Add(tdcPriceRentExcelTotal);

                    for (int i = 0; i < data.Count; i++)
                    {
                        if (data[i].TypeRow == 2)
                        {
                            TdcPriceRentExcel prev = null;
                            TdcPriceRentExcel next = null;

                            if (i - 1 > -1) prev = data[i - 1];
                            if (i + 1 < data.Count) next = data[i + 1];

                            if (prev != null && next != null)
                            {
                                if (prev.PayTimeId == next.PayTimeId) data[i].PayTimeId = next.PayTimeId;
                            }
                        }
                    }

                    var groupDataExcel = data.GroupBy(f => f.PayTimeId != null ? (object)f.PayTimeId : new object(), key => key).Select(f => new TdcPriceRentExcelGroupByPayTimeId
                    {
                        PayTimeId = f.Key,
                        tdcPriceRentExcels = f.ToList()
                    });

                    List<TdcPriceRentExcelGroupByPayTimeId> res = new List<TdcPriceRentExcelGroupByPayTimeId>();
                    foreach (var item in groupDataExcel.ToList())
                    {
                        if (item.tdcPriceRentExcels.Where(x => x.TypeRow == 3).FirstOrDefault() != null)
                        {
                            item.Pay = item.tdcPriceRentExcels.Sum(x => x.TypeRow == 3 ? x.Pay : (x.TypeRow == 4 ? x.PriceEarnings : 0));
                            item.Paid = item.tdcPriceRentExcels.First().Paid;
                            item.PriceDifference = item.tdcPriceRentExcels[item.tdcPriceRentExcels.Count - 1].PriceDifference;
                            item.Status = item.tdcPriceRentExcels.First().Status;
                            item.PricePublic = item.tdcPriceRentExcels.First().PricePublic;
                            item.Check = item.tdcPriceRentExcels.First().Check;

                        }
                        else if (item.tdcPriceRentExcels.Where(x => x.TypeRow == 1).FirstOrDefault() != null)
                        {
                            item.Pay = item.tdcPriceRentExcels.Sum(x => x.Pay);
                            item.Paid = item.tdcPriceRentExcels.Sum(x => x.Paid);
                            item.Status = item.Status = true;
                            item.Check = item.Check = true;
                        }
                        else if (item.tdcPriceRentExcels.Where(x => x.TypeRow == 2).FirstOrDefault() != null)
                        {
                            item.Status = item.tdcPriceRentExcels.First().Status;
                            item.Check = item.tdcPriceRentExcels.First().Check;

                        }
                        else if (item.tdcPriceRentExcels.Where(x => x.TypeRow == 5).FirstOrDefault() != null)
                        {
                            item.Pay = item.tdcPriceRentExcels.First().Pay;
                            item.Paid = item.tdcPriceRentExcels.First().Paid;
                            item.PriceDifference = item.tdcPriceRentExcels.Sum(x => x.PriceDifference);
                            item.Status = item.tdcPriceRentExcels.First().Status;
                        }

                        res.Add(item);
                    }

                    TdcPriceRentData mapper_dataTdc = _mapper.Map<TdcPriceRentData>(tdcPriceRent);



                    // Tạo list để khi add vào ko bị null
                    dataReport.temporaryDatas = new List<IngreData>();

                    dataReport.officialDatas = new List<IngreData>();

                    dataReport.priceAndTaxes = new List<PriceAndTax>();

                    dataReport.priceAndTaxTTs = new List<PriceAndTaxTT>();

                    dataReport.excels = new List<Excel>();

                    dataReport.taxes = new List<Tax>();

                    //Chỗ Khai báo biến dùng chung
                    decimal TotalTT = 0;
                    decimal TotalCT = 0;

                    decimal TotalPatTt = 0; // Do not show tạm thời;
                    decimal TotalPatCt = 0; // Do not show chính thức;

                    decimal sumTotalPercent = 0; //Tổng tiền của các % trong kì

                    List<TDCProjectIngrePrice> tDCProjectIngrePrices = _context.TDCProjectIngrePrices.Where(x => x.TDCProjectId == tdcPriceRent.TdcProjectId && x.Status != AppEnums.EntityStatus.DELETED).ToList();

                    //lấy value TP giá bán cấu thành tạm thời
                    List<TdcPriceRentTemporary> tdcPriceRentTemporaries = _context.TdcPriceRentTemporaries.Where(l => l.TdcPriceRentId == tdcPriceRent.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    List<TdcPriceRentTemporaryData> map_temporary = _mapper.Map<List<TdcPriceRentTemporaryData>>(tdcPriceRentTemporaries.ToList());
                    foreach (TdcPriceRentTemporaryData i in map_temporary)
                    {

                        var itemTemp = new IngreData();

                        itemTemp.Id = i.IngredientsPriceId;
                        itemTemp.Name = _context.IngredientsPrices.Where(f => f.Id == i.IngredientsPriceId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                        itemTemp.Area = i.Area;
                        itemTemp.UnitPrice = i.Price;
                        itemTemp.Price = i.Total;
                        itemTemp.Value = tDCProjectIngrePrices.Where(x => x.IngredientsPriceId == i.IngredientsPriceId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Value).FirstOrDefault();

                        dataReport.temporaryDatas.Add(itemTemp);
                        if(itemTemp.Value != 0)
                        {
                            TotalTT += (itemTemp.Price / (decimal)itemTemp.Value);
                        }
                    }
                    mapper_dataTdc.tdcPriceRentTemporaries = map_temporary;

                    //lấy value TP giá bán cấu thành chính thức
                    List<TdcPriceRentOfficial> tdcPriceRentOfficials = _context.TdcPriceRentOfficials.Where(l => l.TdcPriceRentId == tdcPriceRent.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    List<TdcPriceRentOfficialData> map_official = _mapper.Map<List<TdcPriceRentOfficialData>>(tdcPriceRentOfficials.ToList());
                    foreach (TdcPriceRentOfficialData i in map_official)
                    {
                        // tạo 1 biến = 1 list rỗng
                        var itemOffi = new IngreData();

                        itemOffi.Id = i.IngredientsPriceId;
                        itemOffi.Name = _context.IngredientsPrices.Where(f => f.Id == i.IngredientsPriceId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                        itemOffi.Area = i.Area;
                        itemOffi.UnitPrice = i.Price;
                        itemOffi.Price = i.Total;
                        itemOffi.Value = tDCProjectIngrePrices.Where(x => x.IngredientsPriceId == i.IngredientsPriceId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Value).FirstOrDefault();

                        dataReport.officialDatas.Add(itemOffi);

                        if(itemOffi.Value != 0)
                        {
                            TotalCT += (itemOffi.Price / (decimal)itemOffi.Value);
                        }
                    }
                    mapper_dataTdc.tdcPriceRentOfficials = map_official;

                    List<TDCProjectPriceAndTax> tDCProjectPriceAndTaxs = _context.TDCProjectPriceAndTaxs.Where(l => l.TDCProjectId == tdcPriceRent.TdcProjectId && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    List<IngreData> ingreDataDetails = new List<IngreData>();
                    List<TDCProjectPriceAndTaxData> map_TDCProjectPriceAndTaxs = _mapper.Map<List<TDCProjectPriceAndTaxData>>(tDCProjectPriceAndTaxs);
                    var totalPriceTTValue = tdcPriceRent.TotalPriceTT.GetValueOrDefault();
                    foreach (TDCProjectPriceAndTaxData map_TDCProjectPriceAndTax in map_TDCProjectPriceAndTaxs)
                    {
                        List<TDCProjectPriceAndTaxDetails> de = _context.TDCProjectPriceAndTaxDetailss.Where(f => f.PriceAndTaxId == map_TDCProjectPriceAndTax.Id && f.Status != AppEnums.EntityStatus.DELETED).ToList();
                        List<TDCProjectPriceAndTaxDetailData> detail = _mapper.Map<List<TDCProjectPriceAndTaxDetailData>>(de);
                        OriginalPriceAndTax pat = _context.OriginalPriceAndTaxs.Where(f => f.Id == map_TDCProjectPriceAndTax.PriceAndTaxId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                        if (de == null) map_TDCProjectPriceAndTax.PATName = "";
                        else map_TDCProjectPriceAndTax.PATName = pat.Name;

                        var itemPriceATaxTT = new PriceAndTaxTT();
                        itemPriceATaxTT.Name = pat.Name;
                        itemPriceATaxTT.Value = map_TDCProjectPriceAndTax.Value;
                        itemPriceATaxTT.Location = map_TDCProjectPriceAndTax.Location;
                        itemPriceATaxTT.Total = 0;
                        itemPriceATaxTT.temporaryDatas = new List<IngreData>();
                        TotalPatTt = 0;

                        foreach (TDCProjectPriceAndTaxDetailData i in detail)
                        {
                            IngreData newItem = new IngreData();
                            IngredientsPrice result = _context.IngredientsPrices.Where(f => f.Id == i.IngredientsPriceId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            newItem = dataReport.temporaryDatas.Where(x => x.Id == i.IngredientsPriceId).FirstOrDefault();
                            if (de == null) i.IngrePriceName = "";
                            else i.IngrePriceName = result.Name;
                            itemPriceATaxTT.temporaryDatas.Add(newItem);

                            if(newItem.Value != 0)
                            {
                                TotalPatTt += (newItem.Price / (decimal)newItem.Value);
                            }
                        }

                        if (itemPriceATaxTT.Name == "VAT")
                        {
                            if (totalPriceTTValue == 0)
                            {
                                itemPriceATaxTT.Total = 0;
                            }
                            else
                            {
                                itemPriceATaxTT.Total = Math.Round(((TotalTT / totalPriceTTValue) * 100) * (decimal)itemPriceATaxTT.Value / 100);
                            }
                        }
                        else
                        {
                            if (totalPriceTTValue == 0)
                            {
                                itemPriceATaxTT.Total = 0;
                            }
                            else
                            {
                                itemPriceATaxTT.Total = Math.Round((TotalPatTt * (decimal)itemPriceATaxTT.Value) / totalPriceTTValue);
                            }
                            
                        }
                        dataReport.priceAndTaxTTs.Add(itemPriceATaxTT);

                        var itemPriceATax = new PriceAndTax();

                        itemPriceATax.Name = pat.Name;
                        itemPriceATax.Value = map_TDCProjectPriceAndTax.Value;
                        itemPriceATax.Location = map_TDCProjectPriceAndTax.Location;
                        itemPriceATax.datas = new List<IngreData>();
                        TotalPatCt = 0;

                        foreach (TDCProjectPriceAndTaxDetailData i in detail)
                        {
                            IngreData newItem = new IngreData();
                            IngredientsPrice result = _context.IngredientsPrices.Where(f => f.Id == i.IngredientsPriceId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            newItem = dataReport.officialDatas.Where(x => x.Id == i.IngredientsPriceId).FirstOrDefault();
                            if (de == null) i.IngrePriceName = "";
                            else i.IngrePriceName = result.Name;
                            itemPriceATax.datas.Add(newItem);
                            if (newItem.Value != 0)
                            {
                                TotalPatCt += (newItem.Price / (decimal)newItem.Value);
                            }
                        }

                        if (itemPriceATax.Name == "VAT")
                        {
                            itemPriceATax.Total = Math.Round(((TotalCT / tdcPriceRent.TotalPriceCT) * 100) * (decimal)itemPriceATax.Value / 100);
                        }
                        else
                        {
                            itemPriceATax.Total = Math.Round((TotalPatCt * (decimal)itemPriceATax.Value) / tdcPriceRent.TotalPriceCT);
                        }
                        map_TDCProjectPriceAndTax.TDCProjectPriceAndTaxDetails = detail;
                        dataReport.priceAndTaxes.Add(itemPriceATax);
                    }

                    //Thuế phí nông nghiệp
                    List<TdcPriceRentTax> tdcPriceRentTaxes = _context.TdcPriceRentTaxs.Where(l => l.TdcPriceRentId == tdcPriceRent.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(x => x.Id).ToList();
                    foreach (var t in tdcPriceRentTaxes)
                    {
                        var itemTax = new Tax();
                        itemTax.Year = t.Year;
                        itemTax.Price = t.Price;

                        dataReport.taxes.Add(itemTax);
                    }

                    dataReport.Code = tdcPriceRent.Code;

                    dataReport.ContractDate = tdcPriceRent.Date;

                    dataReport.CustomerName = _context.TdcCustomers.Where(f => f.Id == mapper_dataTdc.TdcCustomerId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.FullName).FirstOrDefault();

                    dataReport.ProjectName = _context.TDCProjects.Where(f => f.Id == mapper_dataTdc.TdcProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                    dataReport.LandName = _context.Lands.Where(f => f.Id == mapper_dataTdc.LandId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                    dataReport.BlockHouseName = _context.BlockHouses.Where(f => f.Id == mapper_dataTdc.BlockHouseId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                    dataReport.FloorName = _context.FloorTdcs.Where(f => f.Id == mapper_dataTdc.FloorTdcId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                    dataReport.ApartmentName = _context.ApartmentTdcs.Where(f => f.Id == mapper_dataTdc.TdcApartmentId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                    dataReport.Floor = tdcPriceRent.Floor1;

                    dataReport.Corner = _context.ApartmentTdcs.Where(f => f.Id == mapper_dataTdc.TdcApartmentId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Corner).FirstOrDefault();

                    dataReport.TotalAreaTT = tdcPriceRent.TotalAreaTT;

                    dataReport.TotalPirceTT = tdcPriceRent.TotalPriceTT;

                    dataReport.DecisionNumberTT = tdcPriceRent.DecisionNumberTT;

                    dataReport.DecisionDateTT = tdcPriceRent.DecisionDateTT;

                    dataReport.TotalAreaCT = tdcPriceRent.TotalAreaCT;

                    dataReport.TotalPirceCT = tdcPriceRent.TotalPriceCT;

                    dataReport.DecisionNumberCT = tdcPriceRent.DecisionNumberCT;

                    dataReport.DecisionDateCT = tdcPriceRent.DecisionDateCT;

                    dataReport.FistPayment = tdcPriceRent.DateTTC;
                    if (totalPriceTTValue == 0)
                    {
                        dataReport.PercentOriginTT = 0;
                    }
                    else
                    {
                        dataReport.PercentOriginTT = Math.Round((TotalTT / totalPriceTTValue) * 100);
                    }

                    dataReport.PercentOriginCT = Math.Round((TotalCT / tdcPriceRent.TotalPriceCT) * 100);

                    foreach (var y in res)
                    {
                        if (y.Check == false && y.Status == true)
                        {
                            var itemExcel = new Excel();

                            foreach (var childItem in y.tdcPriceRentExcels)
                            {
                                if (childItem.TypeRow != 1 && childItem.TypeRow != 2)
                                {
                                    itemExcel.index += childItem.PaymentTimes + ",";
                                    itemExcel.PaymentDate = y.tdcPriceRentExcels.First().PaymentDatePrescribed;
                                    itemExcel.PayDate = y.tdcPriceRentExcels.First().ExpectedPaymentDate;

                                    itemExcel.AmountPayable = y.tdcPriceRentExcels.First().UnitPay;
                                    if (childItem.PriceEarnings.HasValue)
                                    {
                                        if (childItem.TypeRow == 4)
                                        {
                                            itemExcel.PricePunish += (decimal)childItem.PriceEarnings;
                                        }
                                        else itemExcel.Interest += (decimal)childItem.PriceEarnings;
                                    }
                                    itemExcel.Overpayment = y.PriceDifference;

                                    itemExcel.excelChilds = new List<childExcel>();

                                    if (childItem.PaymentDatePrescribed < tdcPriceRent.DecisionDateCT)
                                    {
                                        if (itemExcel.AmountPayable == null) itemExcel.AmountPayable = 0;

                                        itemExcel.PrinCipal = (itemExcel.AmountPayable + itemExcel.Interest + itemExcel.PricePunish) * (dataReport.PercentOriginTT / 100);

                                        foreach (var x in dataReport.priceAndTaxTTs)
                                        {
                                            var childItemExcel = new childExcel();
                                            childItemExcel.Name = x.Name;
                                            if (x.Name != "VAT")
                                            {
                                                childItemExcel.TotalValue = (decimal)((itemExcel.AmountPayable + itemExcel.Interest + itemExcel.PricePunish) * (x.Total / 100));
                                            }
                                            itemExcel.excelChilds.Add(childItemExcel);
                                            sumTotalPercent = itemExcel.excelChilds.Sum(x => x.TotalValue);
                                        }
                                    }
                                    else
                                    {
                                        if (itemExcel.AmountPayable == null) itemExcel.AmountPayable = 0;

                                        itemExcel.PrinCipal = (itemExcel.AmountPayable + itemExcel.Interest + itemExcel.PricePunish) * (dataReport.PercentOriginTT / 100);

                                        foreach (var x in dataReport.priceAndTaxes)
                                        {
                                            var childItemExcel = new childExcel();
                                            childItemExcel.Name = x.Name;
                                            if (x.Name != "VAT")
                                            {
                                                childItemExcel.TotalValue = (decimal)((itemExcel.AmountPayable + itemExcel.Interest + itemExcel.PricePunish) * (x.Total / 100));
                                            }
                                            itemExcel.excelChilds.Add(childItemExcel);
                                            sumTotalPercent = itemExcel.excelChilds.Sum(x => x.TotalValue);
                                        }
                                    }
                                }
                            }
                            itemExcel.VAT = (itemExcel.AmountPayable + itemExcel.Interest + itemExcel.PricePunish) - (itemExcel.PrinCipal + sumTotalPercent);
                            dataReport.excels.Add(itemExcel);
                        }
                    }
                    resReport.Add(dataReport);
                }
                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = resReport;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("GetExcelTable Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);

                return Ok(def);
            }
        }
        private bool TdcPriceRentExists(int id)
        {
            return _context.TdcPriceRents.Count(e => e.Id == id) > 0;
        }

        [HttpPost("ExportReport")]
        public async Task<IActionResult> ExportReport([FromBody] List<TdcPriceRentReport> input)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            //check role
            var identity = (ClaimsIdentity)User.Identity;
            int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id").Select(c => c.Value).SingleOrDefault());
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            DefaultResponse def = new DefaultResponse();
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.CREATE))
            {
                return new FileContentResult(new byte[0], "application/octet-stream");
            }
            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"templates/nostyle.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);
            string nameExcel = "BangBaoCao_.xls";

            MemoryStream ms = WriteDataToExcel(templatePath, 0, input);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", nameExcel);
        }
        private static MemoryStream WriteDataToExcel(string templatePath, int sheetnumber, List<TdcPriceRentReport> data)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowHeader = 0;
            int rowHeader1 = 1;
            int rowStart = 2;

            if (sheet != null)
            {
                try
                {
                    int datacol = 0;
                    int childExcelCount = 0;
                    int taxNum = 0;
                    foreach (var item in data)
                    {
                        foreach (var e in item.excels)
                        {
                            childExcelCount += (e.excelChilds != null ? e.excelChilds.Count : 0);
                        }
                        if (item.taxes.Count == 0)
                        {
                            taxNum = 2;
                        }
                        else
                        {
                            taxNum = (item.taxes != null ? item.taxes.Count : 0) * 2;
                        }
                        datacol = 18 + ((item.temporaryDatas != null ? item.temporaryDatas.Count : 0) * 3) + ((item.officialDatas != null ? item.officialDatas.Count : 0) * 3) + ((item.excels != null ? item.excels.Count : 0) * 9 + (childExcelCount - 1)) + taxNum;

                        var style = workbook.CreateCellStyle();
                        style.BorderBottom = BorderStyle.Thin;
                        style.BorderLeft = BorderStyle.Thin;
                        style.BorderRight = BorderStyle.Thin;
                        style.BorderTop = BorderStyle.Thin;

                        ICellStyle cellStyle = workbook.CreateCellStyle();
                        cellStyle.Alignment = HorizontalAlignment.Center;
                        cellStyle.BorderBottom = BorderStyle.Thin;
                        cellStyle.BorderLeft = BorderStyle.Thin;
                        cellStyle.BorderRight = BorderStyle.Thin;
                        cellStyle.BorderTop = BorderStyle.Thin;

                        IRow rowHeaders = sheet.CreateRow(rowHeader);
                        IRow rowHeaders1 = sheet.CreateRow(rowHeader1);

                        for (int i = 0; i < datacol; i++)
                        {
                            sheet.SetColumnWidth(i, (int)(15.5 * 256));
                        }

                        ICell cellHeader1 = rowHeaders.CreateCell(0);
                        cellHeader1.SetCellValue("Chủ căn hộ");
                        cellHeader1.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader1 = new CellRangeAddress(rowHeader, rowHeader, 0, 2);
                        sheet.AddMergedRegion(mergedRegioncellHeader1);
                        RegionUtil.SetBorderBottom(1, mergedRegioncellHeader1, sheet);//Bottom border  
                        RegionUtil.SetBorderTop(1, mergedRegioncellHeader1, sheet);//Bottom border  
                        RegionUtil.SetBorderLeft(1, mergedRegioncellHeader1, sheet);//Bottom border  
                        RegionUtil.SetBorderRight(1, mergedRegioncellHeader1, sheet);//Bottom border 

                        ICell cellHeader2 = rowHeaders.CreateCell(3);
                        cellHeader2.SetCellValue("thông tin căn hộ");
                        cellHeader2.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader2 = new CellRangeAddress(rowHeader, rowHeader, 3, 8);
                        sheet.AddMergedRegion(mergedRegioncellHeader2);
                        RegionUtil.SetBorderBottom(1, mergedRegioncellHeader2, sheet);
                        RegionUtil.SetBorderTop(1, mergedRegioncellHeader2, sheet);
                        RegionUtil.SetBorderLeft(1, mergedRegioncellHeader2, sheet);
                        RegionUtil.SetBorderRight(1, mergedRegioncellHeader2, sheet);

                        int temporaryDataCount = 9;

                        foreach (var i in item.temporaryDatas)
                        {
                            ICell cellHeader3 = rowHeaders.CreateCell(temporaryDataCount);
                            cellHeader3.SetCellValue(i.Name);
                            cellHeader3.CellStyle = cellStyle;
                            CellRangeAddress mergedRegioncellHeader3 = new CellRangeAddress(rowHeader, rowHeader, temporaryDataCount, temporaryDataCount + 2);
                            sheet.AddMergedRegion(mergedRegioncellHeader3);
                            RegionUtil.SetBorderBottom(1, mergedRegioncellHeader3, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeader3, sheet);
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeader3, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeader3, sheet);

                            temporaryDataCount = temporaryDataCount + 3;
                        }

                        ICell cellHeader4 = rowHeaders.CreateCell(temporaryDataCount);
                        cellHeader4.SetCellValue("Giá bán(tạm)");
                        cellHeader4.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader4 = new CellRangeAddress(rowHeader, rowHeader, temporaryDataCount, temporaryDataCount + 1);
                        sheet.AddMergedRegion(mergedRegioncellHeader4);
                        RegionUtil.SetBorderBottom(1, mergedRegioncellHeader4, sheet);
                        RegionUtil.SetBorderTop(1, mergedRegioncellHeader4, sheet);
                        RegionUtil.SetBorderLeft(1, mergedRegioncellHeader4, sheet);
                        RegionUtil.SetBorderRight(1, mergedRegioncellHeader4, sheet);


                        ICell cellHeader5 = rowHeaders.CreateCell(temporaryDataCount + 2);
                        cellHeader5.SetCellValue("QĐ bố trí tạm(tạm)");
                        cellHeader5.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader5 = new CellRangeAddress(rowHeader, rowHeader, temporaryDataCount + 2, temporaryDataCount + 4);
                        sheet.AddMergedRegion(mergedRegioncellHeader5);
                        RegionUtil.SetBorderBottom(1, mergedRegioncellHeader5, sheet);
                        RegionUtil.SetBorderTop(1, mergedRegioncellHeader5, sheet);
                        RegionUtil.SetBorderLeft(1, mergedRegioncellHeader5, sheet);
                        RegionUtil.SetBorderRight(1, mergedRegioncellHeader5, sheet);

                        int officialDataCount = temporaryDataCount + 5;

                        foreach (var q in item.officialDatas)
                        {
                            ICell cellHeader6 = rowHeaders.CreateCell(officialDataCount);
                            cellHeader6.SetCellValue(q.Name);
                            cellHeader6.CellStyle = cellStyle;
                            CellRangeAddress mergedRegioncellHeader6 = new CellRangeAddress(rowHeader, rowHeader, officialDataCount, officialDataCount + 2);
                            sheet.AddMergedRegion(mergedRegioncellHeader6);
                            RegionUtil.SetBorderBottom(1, mergedRegioncellHeader6, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeader6, sheet);
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeader6, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeader6, sheet);
                            officialDataCount = officialDataCount + 3;
                        }

                        ICell cellHeader7 = rowHeaders.CreateCell(officialDataCount);
                        cellHeader7.SetCellValue("Giá bán(Chính thức)");
                        cellHeader7.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader7 = new CellRangeAddress(rowHeader, rowHeader, officialDataCount, officialDataCount + 1);
                        sheet.AddMergedRegion(mergedRegioncellHeader7);
                        RegionUtil.SetBorderBottom(1, mergedRegioncellHeader7, sheet);
                        RegionUtil.SetBorderTop(1, mergedRegioncellHeader7, sheet);
                        RegionUtil.SetBorderLeft(1, mergedRegioncellHeader7, sheet);
                        RegionUtil.SetBorderRight(1, mergedRegioncellHeader7, sheet);

                        ICell cellHeader8 = rowHeaders.CreateCell(officialDataCount + 2);
                        cellHeader8.SetCellValue("QĐ bố trí tạm(Chính thức)");
                        cellHeader8.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader8 = new CellRangeAddress(rowHeader, rowHeader, officialDataCount + 2, officialDataCount + 3);
                        sheet.AddMergedRegion(mergedRegioncellHeader8);
                        RegionUtil.SetBorderBottom(1, mergedRegioncellHeader8, sheet);
                        RegionUtil.SetBorderTop(1, mergedRegioncellHeader8, sheet);
                        RegionUtil.SetBorderLeft(1, mergedRegioncellHeader8, sheet);
                        RegionUtil.SetBorderRight(1, mergedRegioncellHeader8, sheet);

                        int excelCount = officialDataCount + 4;
                        foreach (var e in item.excels)
                        {
                            ICell cellHeader = rowHeaders.CreateCell(excelCount);
                            cellHeader.SetCellValue("Kỳ");
                            cellHeader.CellStyle = cellStyle;
                            CellRangeAddress mergedRegioncellHeader = new CellRangeAddress(rowHeader, rowHeader1, excelCount, excelCount);
                            sheet.AddMergedRegion(mergedRegioncellHeader);
                            RegionUtil.SetBorderBottom(1, mergedRegioncellHeader, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeader, sheet);
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeader, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeader, sheet);
                            excelCount++;

                            ICell cellHeader9 = rowHeaders.CreateCell(excelCount);
                            cellHeader9.SetCellValue("Ngày phải nộp tiền");
                            cellHeader9.CellStyle = cellStyle;
                            CellRangeAddress mergedRegioncellHeader9 = new CellRangeAddress(rowHeader, rowHeader1, excelCount, excelCount);
                            sheet.AddMergedRegion(mergedRegioncellHeader9);
                            RegionUtil.SetBorderBottom(1, mergedRegioncellHeader9, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeader9, sheet);
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeader9, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeader9, sheet);
                            excelCount++;

                            ICell cellHeader10 = rowHeaders.CreateCell(excelCount);
                            cellHeader10.SetCellValue("Ngày nộp tiền");
                            cellHeader10.CellStyle = cellStyle;
                            CellRangeAddress mergedRegioncellHeader10 = new CellRangeAddress(rowHeader, rowHeader1, excelCount, excelCount);
                            sheet.AddMergedRegion(mergedRegioncellHeader10);
                            RegionUtil.SetBorderBottom(1, mergedRegioncellHeader10, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeader10, sheet);
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeader10, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeader10, sheet);
                            excelCount++;

                            ICell cellHeader11 = rowHeaders.CreateCell(excelCount);
                            cellHeader11.SetCellValue("Số tiền phải nộp");
                            cellHeader11.CellStyle = cellStyle;
                            CellRangeAddress mergedRegioncellHeader11 = new CellRangeAddress(rowHeader, rowHeader1, excelCount, excelCount);
                            sheet.AddMergedRegion(mergedRegioncellHeader11);
                            RegionUtil.SetBorderBottom(1, mergedRegioncellHeader11, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeader11, sheet);
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeader11, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeader11, sheet);
                            excelCount++;

                            ICell cellHeader12 = rowHeaders.CreateCell(excelCount);
                            cellHeader12.SetCellValue("tiền lãi");
                            cellHeader12.CellStyle = cellStyle;
                            CellRangeAddress mergedRegioncellHeader12 = new CellRangeAddress(rowHeader, rowHeader1, excelCount, excelCount);
                            sheet.AddMergedRegion(mergedRegioncellHeader12);
                            RegionUtil.SetBorderBottom(1, mergedRegioncellHeader12, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeader12, sheet);
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeader12, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeader12, sheet);
                            excelCount++;

                            ICell cellHeader13 = rowHeaders.CreateCell(excelCount);
                            cellHeader13.SetCellValue("tiền phạt");
                            cellHeader13.CellStyle = cellStyle;
                            CellRangeAddress mergedRegioncellHeader13 = new CellRangeAddress(rowHeader, rowHeader1, excelCount, excelCount);
                            sheet.AddMergedRegion(mergedRegioncellHeader13);
                            RegionUtil.SetBorderBottom(1, mergedRegioncellHeader13, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeader13, sheet);
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeader13, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeader13, sheet);
                            excelCount++;

                            ICell cellHeader14 = rowHeaders.CreateCell(excelCount);
                            cellHeader14.SetCellValue("tiền gốc");
                            cellHeader14.CellStyle = cellStyle;
                            CellRangeAddress mergedRegioncellHeader14 = new CellRangeAddress(rowHeader, rowHeader1, excelCount, excelCount);
                            sheet.AddMergedRegion(mergedRegioncellHeader14);
                            RegionUtil.SetBorderBottom(1, mergedRegioncellHeader14, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeader14, sheet);
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeader14, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeader14, sheet);
                            excelCount++;

                            ICell cellHeaderVAT = rowHeaders.CreateCell(excelCount);
                            cellHeaderVAT.SetCellValue("VAT");
                            cellHeaderVAT.CellStyle = cellStyle;
                            CellRangeAddress mergedRegioncellHeaderVAT = new CellRangeAddress(rowHeader, rowHeader1, excelCount, excelCount);
                            sheet.AddMergedRegion(mergedRegioncellHeaderVAT);
                            RegionUtil.SetBorderBottom(1, mergedRegioncellHeaderVAT, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeaderVAT, sheet);
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeaderVAT, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeaderVAT, sheet);
                            excelCount++;

                            int excelChildCount = excelCount;
                            foreach (var r in item.priceAndTaxes)
                            {
                                if (r.Name != "VAT")
                                {
                                    ICell cellHeader15 = rowHeaders.CreateCell(excelChildCount);
                                    cellHeader15.SetCellValue(r.Name);
                                    cellHeader15.CellStyle = cellStyle;
                                    CellRangeAddress mergedRegioncellHeader15 = new CellRangeAddress(rowHeader, rowHeader1, excelChildCount, excelChildCount);
                                    sheet.AddMergedRegion(mergedRegioncellHeader15);
                                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader15, sheet);
                                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader15, sheet);
                                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader15, sheet);
                                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader15, sheet);
                                    excelChildCount = excelChildCount + 1;
                                }
                            }
                            excelCount = excelChildCount;

                            ICell cellHeader16 = rowHeaders.CreateCell(excelCount);
                            cellHeader16.SetCellValue("Nộp dư trong kì");
                            cellHeader16.CellStyle = cellStyle;
                            CellRangeAddress mergedRegioncellHeader16 = new CellRangeAddress(rowHeader, rowHeader1, excelCount, excelCount);
                            sheet.AddMergedRegion(mergedRegioncellHeader16);
                            RegionUtil.SetBorderBottom(1, mergedRegioncellHeader16, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeader16, sheet);
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeader16, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeader16, sheet);
                            excelCount++;
                        }
                        int taxCountStart = excelCount;
                        int taxCountEnd = ((item.taxes != null ? item.taxes.Count : 0) * 2) + taxCountStart;

                        ICell cellHeader17 = rowHeaders.CreateCell(taxCountStart);
                        cellHeader17.SetCellValue("Thuế phí nông nghiệp");
                        cellHeader17.CellStyle = cellStyle;
                        if (item.taxes.Count == 0)
                        {
                            CellRangeAddress mergedRegioncellHeader17 = new CellRangeAddress(rowHeader, rowHeader, taxCountStart, taxCountStart + 1);
                            sheet.AddMergedRegion(mergedRegioncellHeader17);
                            RegionUtil.SetBorderBottom(1, mergedRegioncellHeader17, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeader17, sheet);
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeader17, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeader17, sheet);
                        }
                        else
                        {
                            CellRangeAddress mergedRegioncellHeader17 = new CellRangeAddress(rowHeader, rowHeader, taxCountStart, taxCountEnd - 1);
                            sheet.AddMergedRegion(mergedRegioncellHeader17);
                            RegionUtil.SetBorderBottom(1, mergedRegioncellHeader17, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeader17, sheet);
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeader17, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeader17, sheet);
                        }

                        ICell cellHeaders1 = rowHeaders1.CreateCell(0);
                        cellHeaders1.SetCellValue("Số HĐ");
                        cellHeaders1.CellStyle = cellStyle;

                        ICell cellHeaders2 = rowHeaders1.CreateCell(1);
                        cellHeaders2.SetCellValue("ngày kí HĐ");
                        cellHeaders2.CellStyle = cellStyle;

                        ICell cellHeaders3 = rowHeaders1.CreateCell(2);
                        cellHeaders3.SetCellValue("Họ Tên");
                        cellHeaders3.CellStyle = cellStyle;

                        ICell cellHeaders4 = rowHeaders1.CreateCell(3);
                        cellHeaders4.SetCellValue("Căn Hộ");
                        cellHeaders4.CellStyle = cellStyle;

                        ICell cellHeaders5 = rowHeaders1.CreateCell(4);
                        cellHeaders5.SetCellValue("Lô góc");
                        cellHeaders5.CellStyle = cellStyle;

                        ICell cellHeaders6 = rowHeaders1.CreateCell(5);
                        cellHeaders6.SetCellValue("Khối");
                        cellHeaders6.CellStyle = cellStyle;

                        ICell cellHeaders7 = rowHeaders1.CreateCell(6);
                        cellHeaders7.SetCellValue("Lô");
                        cellHeaders7.CellStyle = cellStyle;

                        ICell cellHeaders8 = rowHeaders1.CreateCell(7);
                        cellHeaders8.SetCellValue("Lầu");
                        cellHeaders8.CellStyle = cellStyle;

                        ICell cellHeaders9 = rowHeaders1.CreateCell(8);
                        cellHeaders9.SetCellValue("Tầng");
                        cellHeaders9.CellStyle = cellStyle;

                        int childTempCount = 9;
                        foreach (var z in item.temporaryDatas)
                        {
                            ICell cellHeaders10 = rowHeaders1.CreateCell(childTempCount);
                            cellHeaders10.SetCellValue("Diện tích");
                            cellHeaders10.CellStyle = cellStyle;
                            childTempCount++;

                            ICell cellHeaders11 = rowHeaders1.CreateCell(childTempCount);
                            cellHeaders11.SetCellValue("Đơn Giá");
                            cellHeaders11.CellStyle = cellStyle;
                            childTempCount++;

                            ICell cellHeaders12 = rowHeaders1.CreateCell(childTempCount);
                            cellHeaders12.SetCellValue("Thành tiền");
                            cellHeaders12.CellStyle = cellStyle;
                            childTempCount++;
                        }
                        ICell cellHeaders13 = rowHeaders1.CreateCell(childTempCount);
                        cellHeaders13.SetCellValue("Tổng DT");
                        cellHeaders13.CellStyle = cellStyle;

                        ICell cellHeaders14 = rowHeaders1.CreateCell(childTempCount + 1);
                        cellHeaders14.SetCellValue("Thành tiền");
                        cellHeaders14.CellStyle = cellStyle;

                        int childOffi = childTempCount + 2;

                        ICell cellHeaders15 = rowHeaders1.CreateCell(childOffi);
                        cellHeaders15.SetCellValue("Số QĐ");
                        cellHeaders15.CellStyle = cellStyle;

                        ICell cellHeaders16 = rowHeaders1.CreateCell(childOffi + 1);
                        cellHeaders16.SetCellValue("Ngày QĐ");
                        cellHeaders16.CellStyle = cellStyle;

                        ICell cellHeaders17 = rowHeaders1.CreateCell(childOffi + 2);
                        cellHeaders17.SetCellValue("Thanh toán lần đầu");
                        cellHeaders17.CellStyle = cellStyle;

                        childOffi = childOffi + 3;

                        foreach (var x in item.officialDatas)
                        {
                            ICell cellHeaders18 = rowHeaders1.CreateCell(childOffi);
                            cellHeaders18.SetCellValue("Diện tích");
                            cellHeaders18.CellStyle = cellStyle;
                            childOffi++;

                            ICell cellHeaders19 = rowHeaders1.CreateCell(childOffi);
                            cellHeaders19.SetCellValue("Đơn Giá");
                            cellHeaders19.CellStyle = cellStyle;
                            childOffi++;

                            ICell cellHeaders20 = rowHeaders1.CreateCell(childOffi);
                            cellHeaders20.SetCellValue("Thành tiền");
                            cellHeaders20.CellStyle = cellStyle;
                            childOffi++;
                        }
                        ICell cellHeaders21 = rowHeaders1.CreateCell(childOffi);
                        cellHeaders21.SetCellValue("Tổng DT");
                        cellHeaders21.CellStyle = cellStyle;

                        ICell cellHeaders22 = rowHeaders1.CreateCell(childOffi + 1);
                        cellHeaders22.SetCellValue("Thành tiền");
                        cellHeaders22.CellStyle = cellStyle;

                        ICell cellHeaders23 = rowHeaders1.CreateCell(childOffi + 2);
                        cellHeaders23.SetCellValue("Số QĐ");
                        cellHeaders23.CellStyle = cellStyle;

                        ICell cellHeaders24 = rowHeaders1.CreateCell(childOffi + 3);
                        cellHeaders24.SetCellValue("Ngày QĐ");
                        cellHeaders24.CellStyle = cellStyle;

                        if (item.taxes.Count == 0)
                        {
                            ICell cellHeaders25 = rowHeaders1.CreateCell(taxCountStart);
                            cellHeaders25.SetCellValue("Năm");
                            cellHeaders25.CellStyle = cellStyle;

                            ICell cellHeaders26 = rowHeaders1.CreateCell(taxCountStart + 1);
                            cellHeaders26.SetCellValue("Số tiền");
                            cellHeaders26.CellStyle = cellStyle;
                        }
                        else
                        {
                            foreach (var taxes in item.taxes)
                            {
                                ICell cellHeaders25 = rowHeaders1.CreateCell(taxCountStart);
                                cellHeaders25.SetCellValue("Năm");
                                cellHeaders25.CellStyle = cellStyle;
                                taxCountStart++;

                                ICell cellHeaders26 = rowHeaders1.CreateCell(taxCountStart);
                                cellHeaders26.SetCellValue("Số tiền");
                                cellHeaders26.CellStyle = cellStyle;
                                taxCountStart++;
                            }
                        }

                        int temporaryDatas = 8;
                        int temporaryDatasEnd = 8;
                        int priceAndTaxTTs = -1;
                        int officialDatas = -2;
                        int priceAndTaxes = -3;
                        int tax = -4;

                        IRow row = sheet.CreateRow(rowStart);
                        ICellStyle cellStyleDate = workbook.CreateCellStyle();
                        cellStyleDate.DataFormat = workbook.CreateDataFormat().GetFormat("dd/mm/yyyy");
                        cellStyleDate.BorderBottom = BorderStyle.Thin;
                        cellStyleDate.BorderLeft = BorderStyle.Thin;
                        cellStyleDate.BorderRight = BorderStyle.Thin;
                        cellStyleDate.BorderTop = BorderStyle.Thin;

                        ICellStyle cellStyleMoney = workbook.CreateCellStyle();
                        var dataFormat = workbook.CreateDataFormat();
                        cellStyleMoney.DataFormat = dataFormat.GetFormat("#,##0");
                        cellStyleMoney.BorderBottom = BorderStyle.Thin;
                        cellStyleMoney.BorderLeft = BorderStyle.Thin;
                        cellStyleMoney.BorderRight = BorderStyle.Thin;
                        cellStyleMoney.BorderTop = BorderStyle.Thin;

                        for (int i = 0; i < datacol; i++)
                        {
                            ICell cell = row.CreateCell(i);

                            if (i == 0)
                            {
                                cell.SetCellValue(item.Code);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == 1)
                            {
                                cell.SetCellValue((DateTime)item.ContractDate);
                                cell.CellStyle = cellStyleDate;
                            }
                            else if (i == 2)
                            {
                                cell.SetCellValue(item.CustomerName);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == 3)
                            {
                                cell.SetCellValue(item.ApartmentName);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == 4)
                            {
                                if (item.Corner == null || item.Corner == false)
                                {
                                    cell.SetCellValue("");
                                    cell.CellStyle = cellStyle;
                                }
                                else
                                {
                                    cell.SetCellValue("Check");
                                    cell.CellStyle = cellStyle;
                                }
                            }
                            else if (i == 5)
                            {
                                cell.SetCellValue(item.BlockHouseName);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == 6)
                            {
                                cell.SetCellValue(item.LandName);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == 7)
                            {
                                cell.SetCellValue(item.Floor);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == 8)
                            {
                                cell.SetCellValue(item.FloorName);
                                cell.CellStyle = cellStyle;
                                temporaryDatas++;
                            }
                            else if (i == temporaryDatas)
                            {
                                foreach (var childItem in item.temporaryDatas)
                                {
                                    ICell cellArea = row.CreateCell(i);
                                    cellArea.SetCellValue((double)childItem.Area);
                                    cellArea.CellStyle = cellStyle;
                                    i++;

                                    ICell cellUnitPrice = row.CreateCell(i);
                                    cellUnitPrice.SetCellValue((double)childItem.UnitPrice);
                                    cellUnitPrice.CellStyle = cellStyleMoney;
                                    i++;

                                    ICell cellPrice = row.CreateCell(i);
                                    cellPrice.SetCellValue((double)childItem.Price);
                                    cellPrice.CellStyle = cellStyleMoney;
                                    i++;
                                }
                                temporaryDatasEnd = i;
                                i--;
                            }
                            else if (i == temporaryDatasEnd)
                            {
                                if (item.TotalAreaTT.HasValue)
                                {
                                    cell.SetCellValue((double)item.TotalAreaTT.Value);
                                }
                                else
                                {
                                    cell.SetCellValue(string.Empty);
                                }
                                cell.CellStyle = cellStyle;

                            }
                            else if (i == temporaryDatasEnd + 1)
                            {
                                if (item.TotalPirceTT.HasValue)
                                {
                                    cell.SetCellValue((double)item.TotalPirceTT.Value);
                                }
                                else
                                {
                                    cell.SetCellValue(string.Empty);
                                }
                                cell.CellStyle = cellStyleMoney;
                                priceAndTaxTTs = i;
                            }
                            else if (i == priceAndTaxTTs + 1)
                            {
                                cell.SetCellValue(item.DecisionNumberTT);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == priceAndTaxTTs + 2)
                            {
                                if (item.DecisionDateTT.HasValue)
                                {
                                    cell.SetCellValue(item.DecisionDateTT.Value);
                                }
                                else
                                {
                                    cell.SetCellValue(string.Empty);
                                }
                                cell.CellStyle = cellStyleDate;
                            }
                            else if (i == priceAndTaxTTs + 3)
                            {
                                cell.SetCellValue((DateTime)item.FistPayment);
                                cell.CellStyle = cellStyleDate;

                                foreach (var childItem2 in item.officialDatas)
                                {
                                    i++;
                                    ICell cellArea = row.CreateCell(i);
                                    cellArea.SetCellValue((double)childItem2.Area);
                                    cellArea.CellStyle = cellStyle;

                                    i++;
                                    ICell cellUnitPrice = row.CreateCell(i);
                                    cellUnitPrice.SetCellValue((double)childItem2.UnitPrice);
                                    cellUnitPrice.CellStyle = cellStyleMoney;

                                    i++;
                                    ICell cellPrice = row.CreateCell(i);
                                    cellPrice.SetCellValue((double)childItem2.Price);
                                    cellPrice.CellStyle = cellStyleMoney;
                                }
                                officialDatas = i;
                            }
                            else if (i == officialDatas + 1)
                            {
                                cell.SetCellValue((double)item.TotalAreaCT);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == officialDatas + 2)
                            {
                                cell.SetCellValue((double)item.TotalPirceCT);
                                cell.CellStyle = cellStyleMoney;
                                priceAndTaxes = i;

                            }
                            else if (i == priceAndTaxes + 1)
                            {
                                cell.SetCellValue(item.DecisionNumberCT);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == priceAndTaxes + 2)
                            {
                                cell.SetCellValue((DateTime)item.DecisionDateCT);
                                cell.CellStyle = cellStyleDate;
                            }
                            else if (i == priceAndTaxes + 3)
                            {
                                foreach (var itemExcel in item.excels)
                                {
                                    ICell cellIndex = row.CreateCell(i);
                                    cellIndex.SetCellValue(itemExcel.index);
                                    cellIndex.CellStyle = cellStyle;
                                    i++;

                                    if (itemExcel.PaymentDate == null)
                                    {
                                        ICell cellPaymentDate = row.CreateCell(i);
                                        cellPaymentDate.SetCellValue("");
                                        cellPaymentDate.CellStyle = cellStyle;
                                        i++;
                                    }
                                    else
                                    {
                                        ICell cellPaymentDate = row.CreateCell(i);
                                        cellPaymentDate.SetCellValue((DateTime)itemExcel.PaymentDate);
                                        cellPaymentDate.CellStyle = cellStyleDate;
                                        i++;
                                    }

                                    if (itemExcel.PayDate == null)
                                    {
                                        ICell cellPayDate = row.CreateCell(i);
                                        cellPayDate.SetCellValue("");
                                        cellPayDate.CellStyle = cellStyle;
                                        i++;
                                    }
                                    else
                                    {
                                        ICell cellPayDate = row.CreateCell(i);
                                        cellPayDate.SetCellValue((DateTime)itemExcel.PayDate);
                                        cellPayDate.CellStyle = cellStyleDate;
                                        i++;
                                    }
                                    ICell cellAmountPayable = row.CreateCell(i);
                                    cellAmountPayable.SetCellValue((double)itemExcel.AmountPayable);
                                    cellAmountPayable.CellStyle = cellStyleMoney;
                                    i++;

                                    ICell cellAmountInterest = row.CreateCell(i);
                                    cellAmountInterest.SetCellValue((double)itemExcel.Interest);
                                    cellAmountInterest.CellStyle = cellStyleMoney;
                                    i++;

                                    ICell cellPricePunish = row.CreateCell(i);
                                    cellPricePunish.SetCellValue((double)itemExcel.PricePunish);
                                    cellPricePunish.CellStyle = cellStyleMoney;
                                    i++;

                                    ICell cellPrinCipal = row.CreateCell(i);
                                    cellPrinCipal.SetCellValue((double)itemExcel.PrinCipal);
                                    cellPrinCipal.CellStyle = cellStyleMoney;
                                    i++;

                                    ICell cellVAT = row.CreateCell(i);
                                    cellVAT.SetCellValue((double)itemExcel.VAT);
                                    cellVAT.CellStyle = cellStyleMoney;
                                    i++;

                                    foreach (var childItemExcel in itemExcel.excelChilds)
                                    {
                                        if (childItemExcel.Name != "VAT")
                                        {
                                            ICell cellTotalValue = row.CreateCell(i);
                                            cellTotalValue.SetCellValue((double)childItemExcel.TotalValue);
                                            cellTotalValue.CellStyle = cellStyleMoney;
                                            i++;
                                        }
                                    }
                                    if (itemExcel.Overpayment == null) itemExcel.Overpayment = 0;
                                    ICell cellOverpayment = row.CreateCell(i);
                                    cellOverpayment.SetCellValue((double)itemExcel.Overpayment);
                                    cellOverpayment.CellStyle = cellStyleMoney;
                                    i++;
                                }
                                tax = i;
                                i--;
                            }
                            else if (i == tax)
                            {
                                if (item.taxes.Count == 0)
                                {
                                    ICell cellYear = row.CreateCell(i);
                                    cellYear.SetCellValue("");
                                    cellYear.CellStyle = cellStyle;
                                    i++;

                                    ICell cellPrice = row.CreateCell(i);
                                    cellPrice.SetCellValue("");
                                    cellPrice.CellStyle = cellStyleMoney;
                                }
                                else
                                {
                                    foreach (var itemTax in item.taxes)
                                    {

                                        if (itemTax.Year == null)
                                        {
                                            ICell cellYear = row.CreateCell(i);
                                            cellYear.SetCellValue("");
                                            cellYear.CellStyle = cellStyle;
                                            i++;
                                        }
                                        else
                                        {
                                            ICell cellYear = row.CreateCell(i);
                                            cellYear.SetCellValue(itemTax.Year);
                                            cellYear.CellStyle = cellStyle;
                                            i++;
                                        }

                                        if (itemTax.Price == null)
                                        {
                                            ICell cellPrice = row.CreateCell(i);
                                            cellPrice.SetCellValue("");
                                            cellPrice.CellStyle = cellStyle;
                                            i++;
                                        }
                                        else
                                        {
                                            ICell cellPrice = row.CreateCell(i);
                                            cellPrice.SetCellValue((double)itemTax.Price);
                                            cellPrice.CellStyle = cellStyleMoney;
                                            i++;
                                        }
                                    }
                                }
                            }
                        }
                        rowStart = rowStart + 3;
                        rowHeader = rowHeader + 3;
                        rowHeader1 = rowHeader1 + 3;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Put Exception:" + ex);
                }
            }
            sheet.ForceFormulaRecalculation = true;
            MemoryStream ms = new MemoryStream();
            workbook.Write(ms);

            return ms;
        }

        [HttpGet("getLogTdcPriceRentOff/{Id}")]
        public async Task<IActionResult> getLogTdcPriceRentOff(int id)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            DefaultResponse def = new DefaultResponse();

            var identity = (ClaimsIdentity)User.Identity;
            int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id").Select(c => c.Value).SingleOrDefault());
            string fullName = identity.Claims.Where(c => c.Type == "FullName").Select(c => c.Value).SingleOrDefault() ?? string.Empty;
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.DELETED))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_DELETE_MESSAGE);
                return Ok(def);
            }
            try
            {
                TdcPriceRent lst = _context.TdcPriceRents.Where(l => l.Id == id && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                List<TdcPriceRentOfficial> tdcPriceRentOfficial = _context.TdcPriceRentOfficials.Where(l => l.TdcPriceRentId == lst.Id && l.Status == AppEnums.EntityStatus.DELETED).ToList();
                List<TdcPriceRentOfficialData> map_tdcPriceRentOfficials = _mapper.Map<List<TdcPriceRentOfficialData>>(tdcPriceRentOfficial.ToList());
                foreach (TdcPriceRentOfficialData map_tdcPriceRentOfficial in map_tdcPriceRentOfficials)
                {
                    map_tdcPriceRentOfficial.IngrePriceName = _context.IngredientsPrices.Where(l => l.Id == map_tdcPriceRentOfficial.IngredientsPriceId && l.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                }
                var item = map_tdcPriceRentOfficials.ToList();

                var groupData = item.GroupBy(x =>
                    x.ChangeTimes).Select(e => new GrDataOff<TdcPriceRentOfficialData>
                {
                    UpdateTime = e.First().UpdatedAt,
                    grData = e.ToList(),
                }).ToList();

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = groupData.OrderByDescending(x => x.UpdateTime);
                return Ok(def);
            }
            catch(Exception e)
            {
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }
        private bool Check(List<TdcPriceRentOfficialData> list1, List<TdcPriceRentOfficialData> list2)
        {
            var difference = list1.Except(list2).ToList();
            return difference.Count == 0;
        }

    }
}
