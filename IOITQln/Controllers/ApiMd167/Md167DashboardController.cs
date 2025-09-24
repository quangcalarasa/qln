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
using Microsoft.Net.Http.Headers;
using IOITQln.Controllers.ApiMd167;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Controllers.ApiInv
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class Md167DashboardController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("Md167Dashboard", "Md167Dashboard");
        private static string functionCode = "MD167_DASHBOARD";
        private readonly ApiDbContext _context;
        private IMapper _mapper;

        public Md167DashboardController(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        //API tổng cơ sở nhà đất
        [HttpGet("GetHouseBase")]
        public async Task<IActionResult> GetHouseBase()
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
                Md167HouseBaseDashboardData res = new Md167HouseBaseDashboardData();

                List<Md167StateOfUse> md167StateOfUses = _context.Md167StateOfUses.Where(e => e.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<Md167House> houseData = _context.Md167Houses.Where(e => e.TypeHouse != Md167House.Type_House.Kios && e.Status != AppEnums.EntityStatus.DELETED).ToList();

                res.TotalValue += houseData.Count;
                res.NotRentValue += (from h in houseData
                                     join m in md167StateOfUses on h.StatusOfUse equals m.Id
                                     where m.Code == "CHUA_CHO_THUE"
                                     select h).Count();

                res.RentValue += (from h in houseData
                                     join m in md167StateOfUses on h.StatusOfUse equals m.Id
                                     where m.Code == "DANG_CHO_THUE"
                                     select h).Count();

                List<Md167House> kiosData = _context.Md167Houses.Where(e => e.Md167HouseId != null && e.TypeHouse == Md167House.Type_House.Kios && e.Status != AppEnums.EntityStatus.DELETED).ToList();
                res.TotalValue += kiosData.Count;
                res.NotRentValue += kiosData.Where(e => e.InfoValue?.KiosStatus == Md167House.Kios_Status.CHUA_CHO_THUE).Count();
                res.RentValue += kiosData.Where(e => e.InfoValue?.KiosStatus == Md167House.Kios_Status.DANG_CHO_THUE).Count();

                res.OtherValue = res.TotalValue - res.NotRentValue - res.RentValue;

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = res;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("GetHouseBase Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        //API cơ sở nhà đất mới tiếp nhận
        [HttpGet("GetNewHouseBase/{year}/{month}")]
        public async Task<IActionResult> GetNewHouseBase(int year, int month)
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
                List<Md167NewHouseBaseDashboardData> res = new List<Md167NewHouseBaseDashboardData>();

                List<Md167House> houseData = new List<Md167House>();

                if(month != 0 )
                {
                    //houseData = _context.Md167Houses.Where(e => e.CreatedAt.Month == month && e.CreatedAt.Year == year && e.TypeHouse != Md167House.Type_House.Kios && e.Status != AppEnums.EntityStatus.DELETED).ToList();
                    houseData = (from c in _context.Md167Contracts
                                 join h in _context.Md167Houses on c.HouseId equals h.Id
                                 where h.Status != AppEnums.EntityStatus.DELETED
                                     && c.Status != AppEnums.EntityStatus.DELETED
                                     && c.DateSign.Month == month && c.DateSign.Year == year
                                 select h).ToList();
                }
                else
                {
                    //houseData = _context.Md167Houses.Where(e => e.CreatedAt.Year == year && e.TypeHouse != Md167House.Type_House.Kios && e.Status != AppEnums.EntityStatus.DELETED).ToList();
                    houseData = (from c in _context.Md167Contracts
                                 join h in _context.Md167Houses on c.HouseId equals h.Id
                                 where h.Status != AppEnums.EntityStatus.DELETED
                                     && c.Status != AppEnums.EntityStatus.DELETED
                                     && c.DateSign.Year == year
                                 select h).ToList();
                }

                var groupHouseData = houseData.GroupBy(x => x.DistrictId).ToList();
                List<District> districts = _context.Districts.Where(e => e.Status != AppEnums.EntityStatus.DELETED).ToList();
                groupHouseData.ForEach(item => {
                    if(item.Key != null)
                    {
                        District district = districts.Where(e => e.Id == item.Key).FirstOrDefault();
                        if(district != null)
                        {
                            Md167NewHouseBaseDashboardData data = new Md167NewHouseBaseDashboardData();
                            data.DistrictName = district.Name;
                            data.TypeHouse1 = item.Where(e => e.TypeHouse == Md167House.Type_House.House).Count();
                            data.TypeHouse2 = item.Where(e => e.TypeHouse == Md167House.Type_House.Apartment).Count();

                            res.Add(data);
                        }
                    }
                });

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = res;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("GetHouseBase Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        //API thống kê doanh thu
        [HttpGet("GetRevenue/{year}/{month}")]
        public async Task<IActionResult> GetRevenue(int year, int month)
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
                List<Md167Contract> md167Contracts;
                if(month != 0 )
                {
                    //md167Contracts = _context.Md167Contracts.Where(m => m.DateSign.Month == month && m.DateSign.Year == year && m.ContractStatus == AppEnums.ContractStatus167.CON_HIEU_LUC && m.Status != AppEnums.EntityStatus.DELETED).ToList();
                    md167Contracts = (from c in _context.Md167Contracts
                                      join h in _context.Md167Houses on c.HouseId equals h.Id
                                      where c.Status != AppEnums.EntityStatus.DELETED
                                        && h.Status != AppEnums.EntityStatus.DELETED
                                        && c.ContractStatus == AppEnums.ContractStatus167.CON_HIEU_LUC
                                        && c.DateSign.Month == month && c.DateSign.Year == year
                                      select c).ToList();
                }
                else
                {
                    //md167Contracts = _context.Md167Contracts.Where(m => m.DateSign.Year == year && m.ContractStatus == AppEnums.ContractStatus167.CON_HIEU_LUC && m.Status != AppEnums.EntityStatus.DELETED).ToList();
                    md167Contracts = (from c in _context.Md167Contracts
                                      join h in _context.Md167Houses on c.HouseId equals h.Id
                                      where c.Status != AppEnums.EntityStatus.DELETED
                                        && h.Status != AppEnums.EntityStatus.DELETED
                                        && c.ContractStatus == AppEnums.ContractStatus167.CON_HIEU_LUC
                                        && c.DateSign.Year == year
                                      select c).ToList();
                }

                Md167RevenueDashboardData md167RevenueDashboardData = new Md167RevenueDashboardData();

                List<Md167ProfitValue> profitValues = _context.Md167ProfitValues.Where(p => p.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<Md167PricePerMonth> pricePerMonthsAllData = (from m in md167Contracts
                                                           join ppm in _context.Md167PricePerMonths on m.Id equals ppm.Md167ContractId
                                                           where m.Status != AppEnums.EntityStatus.DELETED
                                                            && ppm.Status != AppEnums.EntityStatus.DELETED
                                                           select ppm).ToList();

                List<Md167Receipt> md167ReceiptsAllData = (from m in md167Contracts
                                                           join mr in _context.Md167Receipts on m.Id equals mr.Md167ContractId
                                                           where m.Status != AppEnums.EntityStatus.DELETED
                                                            && mr.Status != AppEnums.EntityStatus.DELETED
                                                           select mr).ToList();

                foreach (Md167Contract md167Contract in md167Contracts)
                {
                    List<Md167PricePerMonth> pricePerMonths = pricePerMonthsAllData.Where(p => p.Md167ContractId == md167Contract.Id).OrderBy(x => x.UpdatedAt).ToList();
                    List<Md167Receipt> md167Receipts = md167ReceiptsAllData.Where(m => m.Md167ContractId == md167Contract.Id && m.PaidDeposit != true && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();

                    List<GroupMd167DebtData> groupMd167DebtData = null;
                    if(md167Contract.Type == Contract167Type.MAIN)
                    {
                        //Ds phiếu thu thanh toán tiền thế chân
                        List<Md167Receipt> depositMd167Receipts = md167ReceiptsAllData.Where(m => m.Md167ContractId == md167Contract.Id && m.PaidDeposit == true && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();

                        //Tìm hợp đồng liên quan để lấy tiền thế chân
                        Md167Contract dataRelated = await _context.Md167Contracts.Where(x => x.DelegateId == md167Contract.DelegateId && x.HouseId == md167Contract.HouseId && x.RefundPaidDeposit != true && x.Status != EntityStatus.DELETED).FirstOrDefaultAsync();
                        if (dataRelated != null)
                        {
                            var pricePerMonth = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == dataRelated.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).FirstOrDefault();
                            groupMd167DebtData = Md167ContractController.GetDataDebtFunc(pricePerMonths, profitValues, md167Receipts, md167Contract, pricePerMonth?.TotalPrice * 3, depositMd167Receipts);
                        }
                        else
                        {
                            var pricePerMonth = pricePerMonths.Count > 0 ? pricePerMonths[pricePerMonths.Count - 1] : null;
                            groupMd167DebtData = Md167ContractController.GetDataDebtFunc(pricePerMonths, profitValues, md167Receipts, md167Contract, pricePerMonth?.TotalPrice * 3, depositMd167Receipts);
                        }
                    }
                    else
                    {
                        Md167Contract parentData = await _context.Md167Contracts.FindAsync(md167Contract.ParentId);
                        if (parentData == null)
                        {
                            def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                            return Ok(def);
                        }

                        var parentPricePerMonths = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == parentData.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                        List<Md167Receipt> parentMd167Receipts = _context.Md167Receipts.Where(m => m.Md167ContractId == parentData.Id && m.PaidDeposit != true && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();
                        List<Md167Receipt> depositMd167Receipts = md167ReceiptsAllData.Where(m => m.Md167ContractId == md167Contract.Id && m.PaidDeposit == true && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();

                        groupMd167DebtData = Md167ContractController.GetDataExtraDebtFunc(pricePerMonths, profitValues, md167Receipts, md167Contract, parentPricePerMonths, parentMd167Receipts, parentData, depositMd167Receipts);
                    }

                    

                    var endMd167DebtData = groupMd167DebtData.LastOrDefault();
                    var sumRow = endMd167DebtData.dataGroup.Where(e => e.TypeRow == AppEnums.Md167DebtTypeRow.DONG_TONG).FirstOrDefault();

                    md167RevenueDashboardData.AmountToBePaid += sumRow.AmountToBePaid ?? 0;
                    md167RevenueDashboardData.AmountPaid += sumRow.AmountPaid ?? 0;
                    md167RevenueDashboardData.AmountDiff += sumRow.AmountDiff ?? 0;
                }

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = md167RevenueDashboardData;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("GetHouseBase Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        //API thống kê thuế đất
        [HttpGet("GetTaxInfo")]
        public async Task<IActionResult> GetTaxInfo()
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
                int year = DateTime.Now.Year;

                List<Md167TaxDashboardData> res = new List<Md167TaxDashboardData>();
                List<Md167House> houseData = _context.Md167Houses.Where(e => e.ReceptionDate.Year <= year && e.TypeHouse != Md167House.Type_House.Kios && e.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<Md167House> kiosData = _context.Md167Houses.Where(e => e.TypeHouse == Md167House.Type_House.Kios && e.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<Md167ManagePayment> payments = _context.Md167ManagePayments.Where(e => e.Status != AppEnums.EntityStatus.DELETED).ToList();

                decimal amountPaid = 0;
                for(int i = 5; i >= 0; i--)
                {
                    Md167TaxDashboardData md167TaxDashboardData = new Md167TaxDashboardData();
                    md167TaxDashboardData.Year = year - i;

                    List<Md167House> houses = houseData.Where(e => e.ReceptionDate.Year <= md167TaxDashboardData.Year && e.TypeHouse == Md167House.Type_House.House).ToList();
                    md167TaxDashboardData.AmountToBePaid += houses.Sum(x => x.InfoValue?.TaxNN ?? 0);

                    List<Md167House> kios = (from h in houseData
                                             join k in kiosData on h.Id equals k.Md167HouseId
                                             where h.TypeHouse == Md167House.Type_House.Apartment
                                                && k.ReceptionDate.Year <= md167TaxDashboardData.Year
                                             select k).ToList();

                    md167TaxDashboardData.AmountToBePaid += kios.Sum(x => x.InfoValue?.TaxNN ?? 0);
                    if(i == 5)
                    {
                        amountPaid = payments.Where(e => e.Year <= md167TaxDashboardData.Year).Sum(x => x.Payment);
                    }
                    else
                    {
                        md167TaxDashboardData.AmountToBePaid = md167TaxDashboardData.AmountToBePaid - amountPaid;
                        md167TaxDashboardData.AmountPaid = payments.Where(e => e.Year == md167TaxDashboardData.Year).Sum(x => x.Payment);
                        md167TaxDashboardData.AmountDiff = md167TaxDashboardData.AmountToBePaid - md167TaxDashboardData.AmountPaid;
                        amountPaid += md167TaxDashboardData.AmountPaid ?? 0;

                        res.Add(md167TaxDashboardData);
                    }
                }



                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = res;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("GetTaxInfo Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }
    }
}
