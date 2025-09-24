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
using Microsoft.IdentityModel.Logging;
using static IOITQln.Models.Data.TdcInstallmentPriceTable;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using NPOI.HSSF.UserModel;
using NPOI.SS.Util;
using NPOI.POIFS.Crypt.Dsig;
using NPOI.SS.Formula.Functions;
using System.Reflection.PortableExecutable;
using DevExpress.ClipboardSource.SpreadsheetML;
using DevExpress.SpreadsheetSource.Implementation;
using DevExpress.Office.Utils;
using static DevExpress.Data.Helpers.ExpressiveSortInfo;
using DevExpress.XtraExport.Xls;
using IOITQln.Migrations;
using System.Drawing;
using DevExpress.XtraLayout.Filtering.Templates;
using NPOI.OpenXml4Net.Util;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Net.Http.Headers;
using DevExpress.XtraRichEdit.Layout;

namespace IOITQln.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TDCInstallmentPriceController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("tdcInstallmentPrice", "tdcInstallmentPrice");
        private static string functionCode = "TDC_INSTALLMENT_PRICE";
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        private IHostingEnvironment _hostingEnvironment;

        public TDCInstallmentPriceController(ApiDbContext context, IMapper mapper, IHostingEnvironment hostingEnvironment)
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
                    IQueryable<TDCInstallmentPrice> data = _context.TDCInstallmentPrices.Where(c => c.Status != AppEnums.EntityStatus.DELETED);
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

                        List<TDCInstallmentPriceData> res = _mapper.Map<List<TDCInstallmentPriceData>>(data.ToList());
                        foreach (TDCInstallmentPriceData item in res)
                        {
                            item.CustomerName = _context.TdcCustomers.Where(f => f.Id == item.TdcCustomerId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.FullName).FirstOrDefault();

                            item.TdcProjectName = _context.TDCProjects.Where(f => f.Id == item.TdcProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            item.TdcLandName = _context.Lands.Where(f => f.Id == item.LandId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            item.TdcBlockHouseName = _context.BlockHouses.Where(f => f.Id == item.BlockHouseId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            item.TdcFloorName = _context.FloorTdcs.Where(f => f.Id == item.FloorTdcId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            item.TdcApartmentName = _context.ApartmentTdcs.Where(f => f.Id == item.TdcApartmentId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            List<TDCInstallmentOfficialDetail> tDCInstallmentOfficialDetails = _context.TDCInstallmentOfficialDetails.Where(l => l.TDCInstallmentPriceId == item.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                            List<TDCInstallmentOfficialDetailData> map_offical = _mapper.Map<List<TDCInstallmentOfficialDetailData>>(tDCInstallmentOfficialDetails.ToList());
                            foreach (TDCInstallmentOfficialDetailData i in map_offical)
                            {
                                i.IngrePriceName = _context.IngredientsPrices.Where(f => f.Id == i.IngredientsPriceId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                            }
                            item.tDCInstallmentOfficialDetails = map_offical;

                            List<TDCInstallmentTemporaryDetail> tDCInstallmentTemporaryDetails = _context.TDCInstallmentTemporaryDetails.Where(l => l.TDCInstallmentPriceId == item.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                            List<TDCInstallmentTemporaryDetailData> map_temporary = _mapper.Map<List<TDCInstallmentTemporaryDetailData>>(tDCInstallmentTemporaryDetails.ToList());
                            foreach (TDCInstallmentTemporaryDetailData i in map_temporary)
                            {
                                i.IngrePriceName = _context.IngredientsPrices.Where(f => f.Id == i.IngredientsPriceId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                            }
                            item.tDCInstallmentTemporaryDetails = map_temporary;

                            List<TDCInstallmentPriceAndTax> tDCInstallmentPriceAndTaxs = _context.TDCInstallmentPriceAndTaxs.Where(l => l.TDCInstallmentPriceId == item.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                            item.tDCInstallmentPriceAndTaxs = tDCInstallmentPriceAndTaxs;
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
        //GET: api/TDCInstallmentPrice/1
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
                TDCInstallmentPrice data = await _context.TDCInstallmentPrices.FindAsync(id);

                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                    return Ok(def);
                }

                TDCInstallmentPriceData res = _mapper.Map<TDCInstallmentPriceData>(data);

                List<TDCInstallmentOfficialDetail> tDCInstallmentOfficialDetails = _context.TDCInstallmentOfficialDetails.Where(l => l.TDCInstallmentPriceId == res.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<TDCInstallmentOfficialDetailData> map_offical = _mapper.Map<List<TDCInstallmentOfficialDetailData>>(tDCInstallmentOfficialDetails.ToList());
                foreach (TDCInstallmentOfficialDetailData i in map_offical)
                {
                    i.IngrePriceName = _context.IngredientsPrices.Where(f => f.Id == i.IngredientsPriceId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                }
                res.tDCInstallmentOfficialDetails = map_offical;

                List<TDCInstallmentTemporaryDetail> tDCInstallmentTemporaryDetails = _context.TDCInstallmentTemporaryDetails.Where(l => l.TDCInstallmentPriceId == res.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<TDCInstallmentTemporaryDetailData> map_temporary = _mapper.Map<List<TDCInstallmentTemporaryDetailData>>(tDCInstallmentTemporaryDetails.ToList());
                foreach (TDCInstallmentTemporaryDetailData i in map_temporary)
                {
                    i.IngrePriceName = _context.IngredientsPrices.Where(f => f.Id == i.IngredientsPriceId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                }
                res.tDCInstallmentTemporaryDetails = map_temporary;

                List<TDCInstallmentPriceAndTax> tDCInstallmentPriceAndTaxs = _context.TDCInstallmentPriceAndTaxs.Where(l => l.TDCInstallmentPriceId == res.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                res.tDCInstallmentPriceAndTaxs = tDCInstallmentPriceAndTaxs;

                def.data = res;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("GetById Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        // POST: api/TDCInstallmentPrice
        [HttpPost]
        public async Task<IActionResult> Post(TDCInstallmentPriceData input)
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
                input = (TDCInstallmentPriceData)UtilsService.TrimStringPropertyTypeObject(input);

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
                    _context.TDCInstallmentPrices.Add(input);

                    try
                    {
                        await _context.SaveChangesAsync();

                        //TDCInstallmentOfficialDetail
                        if (input.tDCInstallmentOfficialDetails != null)
                        {
                            foreach (var tDCInstallmentOfficialDetails in input.tDCInstallmentOfficialDetails)
                            {
                                tDCInstallmentOfficialDetails.TDCInstallmentPriceId = input.Id;
                                tDCInstallmentOfficialDetails.CreatedBy = fullName;
                                tDCInstallmentOfficialDetails.CreatedById = userId;

                                _context.TDCInstallmentOfficialDetails.Add(tDCInstallmentOfficialDetails);
                            }
                        }

                        //TDCInstallmentPriceAndTax
                        if (input.tDCInstallmentPriceAndTaxs != null)
                        {
                            foreach (var tDCInstallmentPriceAndTaxs in input.tDCInstallmentPriceAndTaxs)
                            {
                                tDCInstallmentPriceAndTaxs.TDCInstallmentPriceId = input.Id;
                                tDCInstallmentPriceAndTaxs.CreatedBy = fullName;
                                tDCInstallmentPriceAndTaxs.CreatedById = userId;

                                _context.TDCInstallmentPriceAndTaxs.Add(tDCInstallmentPriceAndTaxs);
                            }
                        }

                        //TDCInstallmentTemporaryDetail
                        if (input.tDCInstallmentTemporaryDetails != null)
                        {
                            foreach (var tDCInstallmentTemporaryDetails in input.tDCInstallmentTemporaryDetails)
                            {
                                tDCInstallmentTemporaryDetails.TDCInstallmentPriceId = input.Id;
                                tDCInstallmentTemporaryDetails.CreatedBy = fullName;
                                tDCInstallmentTemporaryDetails.CreatedById = userId;

                                _context.TDCInstallmentTemporaryDetails.Add(tDCInstallmentTemporaryDetails);
                            }
                        }

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Thêm mới Hợp đồng bán trả góp", "TDCInstallmentPrice", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (input.Id > 0)
                            transaction.Commit();
                        else
                            transaction.Rollback();

                        def.meta = new Meta(200, ApiConstants.MessageResource.ADD_SUCCESS);
                        def.data = input;
                        return Ok(def);
                    }
                    catch (DbUpdateException e)
                    {
                        log.Error("DbUpdateException:" + e);
                        transaction.Rollback();
                        if (TDCInstallmentPriceExists(input.Id))
                        {
                            def.meta = new Meta(212, "Đã tồn tại Id trên hệ thống!");
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
        private bool Check(List<TDCInstallmentOfficialDetailData> list1, List<TDCInstallmentOfficialDetail> list2)
        {
            var difference = list1.Except(list2).ToList();
            return difference.Count == 0;
        }

        [HttpGet("getLogTdcIPOff/{Id}")]
        public async Task<IActionResult> getLogTdcIPOff(int id)
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
                TDCInstallmentPrice lst = _context.TDCInstallmentPrices.Where(l => l.Id == id && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                List<TDCInstallmentOfficialDetail> tDCInstallmentOfficialDetail = _context.TDCInstallmentOfficialDetails.Where(l => l.TDCInstallmentPriceId == lst.Id && l.Status == AppEnums.EntityStatus.DELETED).ToList();
                List<TDCInstallmentOfficialDetailData> map_tDCInstallmentOfficialDetails = _mapper.Map<List<TDCInstallmentOfficialDetailData>>(tDCInstallmentOfficialDetail.ToList());
                foreach (var map_tDCInstallmentOfficialDetail in map_tDCInstallmentOfficialDetails)
                {
                    map_tDCInstallmentOfficialDetail.IngrePriceName = _context.IngredientsPrices.Where(l => l.Id == map_tDCInstallmentOfficialDetail.IngredientsPriceId && l.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                }
                var item = map_tDCInstallmentOfficialDetails.ToList();

                var groupData = item.GroupBy(x =>
                   x.ChangeTimes).Select(e => new GrDataOff<TDCInstallmentOfficialDetailData>
                   {
                       UpdateTime = e.First().UpdatedAt,
                       grData = e.ToList(),
                   }).ToList();

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = groupData.OrderByDescending(x => x.UpdateTime);
                return Ok(def);
            }
            catch (Exception e)
            {
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }
        // PUT: api/TDCInstallmentPrice/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] TDCInstallmentPriceData input)
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
                input = (TDCInstallmentPriceData)UtilsService.TrimStringPropertyTypeObject(input);

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

                TDCInstallmentPrice data = await _context.TDCInstallmentPrices.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
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
                        await _context.SaveChangesAsync();

                        //TDCInstallmentOfficialDetail
                        List<TDCInstallmentOfficialDetail> tDCInstallmentOfficialDetails = _context.TDCInstallmentOfficialDetails.Where(l => l.TDCInstallmentPriceId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).AsNoTracking().ToList();

                        if (input.tDCInstallmentOfficialDetails != null)
                        {
                            bool check = Check(input.tDCInstallmentOfficialDetails, tDCInstallmentOfficialDetails);

                            if (check == false)
                            {
                                tDCInstallmentOfficialDetails.ForEach(x =>
                                {
                                    x.ChangeTimes = tDCInstallmentOfficialDetails[0].Id;
                                    x.Status = AppEnums.EntityStatus.DELETED;
                                });
                                _context.UpdateRange(tDCInstallmentOfficialDetails);
                                await _context.SaveChangesAsync();
                                foreach (var item in input.tDCInstallmentOfficialDetails)
                                {
                                    item.Id = 0;
                                    item.TDCInstallmentPriceId = input.Id;
                                    _context.TDCInstallmentOfficialDetails.Add(item);
                                }
                                await _context.SaveChangesAsync();
                            }

                        }
                        //if (input.tDCInstallmentOfficialDetails != null)
                        //{
                        //    foreach (var tDCInstallmentOfficialDetail in input.tDCInstallmentOfficialDetails)
                        //    {
                        //        TDCInstallmentOfficialDetail tDCInstallmentOfficialDetailExist = tDCInstallmentOfficialDetails.Where(l => l.Id == tDCInstallmentOfficialDetail.Id).FirstOrDefault();
                        //        if (tDCInstallmentOfficialDetailExist == null)
                        //        {
                        //            tDCInstallmentOfficialDetail.TDCInstallmentPriceId = input.Id;
                        //            tDCInstallmentOfficialDetail.CreatedBy = fullName;
                        //            tDCInstallmentOfficialDetail.CreatedById = userId;

                        //            _context.TDCInstallmentOfficialDetails.Add(tDCInstallmentOfficialDetail);
                        //        }
                        //        else
                        //        {
                        //            tDCInstallmentOfficialDetail.TDCInstallmentPriceId = input.Id;
                        //            tDCInstallmentOfficialDetail.CreatedBy = tDCInstallmentOfficialDetailExist.CreatedBy;
                        //            tDCInstallmentOfficialDetail.CreatedById = tDCInstallmentOfficialDetailExist.CreatedById;
                        //            tDCInstallmentOfficialDetail.UpdatedBy = fullName;
                        //            tDCInstallmentOfficialDetail.UpdatedById = userId;

                        //            _context.Update(tDCInstallmentOfficialDetail);

                        //            tDCInstallmentOfficialDetails.Remove(tDCInstallmentOfficialDetailExist);
                        //        }
                        //    }
                        //}

                        //if (tDCInstallmentOfficialDetails.Count > 0)
                        //{
                        //    tDCInstallmentOfficialDetails.ForEach(item =>
                        //    {
                        //        item.UpdatedAt = DateTime.Now;
                        //        item.UpdatedById = userId;
                        //        item.UpdatedBy = fullName;
                        //        item.Status = AppEnums.EntityStatus.DELETED;
                        //    });
                        //    _context.UpdateRange(tDCInstallmentOfficialDetails);
                        //}
                        //TDCInstallmentPriceAndTax
                        List<TDCInstallmentPriceAndTax> tDCInstallmentPriceAndTaxs = _context.TDCInstallmentPriceAndTaxs.Where(l => l.TDCInstallmentPriceId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).AsNoTracking().ToList();
                        if (input.tDCInstallmentPriceAndTaxs != null)
                        {
                            foreach (var tDCInstallmentPriceAndTax in input.tDCInstallmentPriceAndTaxs)
                            {
                                TDCInstallmentPriceAndTax tDCInstallmentPriceAndTaxExist = tDCInstallmentPriceAndTaxs.Where(l => l.Id == tDCInstallmentPriceAndTax.Id).FirstOrDefault();
                                if (tDCInstallmentPriceAndTaxExist == null)
                                {
                                    tDCInstallmentPriceAndTax.TDCInstallmentPriceId = input.Id;
                                    tDCInstallmentPriceAndTax.CreatedBy = fullName;
                                    tDCInstallmentPriceAndTax.CreatedById = userId;

                                    _context.TDCInstallmentPriceAndTaxs.Add(tDCInstallmentPriceAndTax);
                                }
                                else
                                {
                                    tDCInstallmentPriceAndTax.TDCInstallmentPriceId = input.Id;
                                    tDCInstallmentPriceAndTax.CreatedBy = tDCInstallmentPriceAndTaxExist.CreatedBy;
                                    tDCInstallmentPriceAndTax.CreatedById = tDCInstallmentPriceAndTaxExist.CreatedById;
                                    tDCInstallmentPriceAndTax.UpdatedBy = fullName;
                                    tDCInstallmentPriceAndTax.UpdatedById = userId;

                                    _context.Update(tDCInstallmentPriceAndTax);

                                    tDCInstallmentPriceAndTaxs.Remove(tDCInstallmentPriceAndTaxExist);
                                }
                            }
                        }

                        if (tDCInstallmentPriceAndTaxs.Count > 0)
                        {
                            tDCInstallmentPriceAndTaxs.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = AppEnums.EntityStatus.DELETED;
                            });
                            _context.UpdateRange(tDCInstallmentPriceAndTaxs);
                        }

                        //TDCInstallmentTemporaryDetail
                        List<TDCInstallmentTemporaryDetail> tDCInstallmentTemporaryDetails = _context.TDCInstallmentTemporaryDetails.Where(l => l.TDCInstallmentPriceId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).AsNoTracking().ToList();
                        if (input.tDCInstallmentTemporaryDetails != null)
                        {
                            foreach (TDCInstallmentTemporaryDetail tDCInstallmentTemporaryDetail in input.tDCInstallmentTemporaryDetails)
                            {
                                TDCInstallmentTemporaryDetail tDCInstallmentTemporaryDetailExist = tDCInstallmentTemporaryDetails.Where(l => l.Id == tDCInstallmentTemporaryDetail.Id).FirstOrDefault();
                                if (tDCInstallmentTemporaryDetailExist == null)
                                {
                                    tDCInstallmentTemporaryDetail.TDCInstallmentPriceId = input.Id;
                                    tDCInstallmentTemporaryDetail.CreatedBy = fullName;
                                    tDCInstallmentTemporaryDetail.CreatedById = userId;

                                    _context.TDCInstallmentTemporaryDetails.Add(tDCInstallmentTemporaryDetail);
                                }
                                else
                                {
                                    tDCInstallmentTemporaryDetail.TDCInstallmentPriceId = input.Id;
                                    tDCInstallmentTemporaryDetail.CreatedBy = tDCInstallmentTemporaryDetailExist.CreatedBy;
                                    tDCInstallmentTemporaryDetail.CreatedById = tDCInstallmentTemporaryDetailExist.CreatedById;
                                    tDCInstallmentTemporaryDetail.UpdatedBy = fullName;
                                    tDCInstallmentTemporaryDetail.UpdatedById = userId;


                                    _context.Update(tDCInstallmentTemporaryDetail);

                                    tDCInstallmentTemporaryDetails.Remove(tDCInstallmentTemporaryDetailExist);
                                }
                            }
                        }

                        if (tDCInstallmentTemporaryDetails.Count > 0)
                        {
                            tDCInstallmentTemporaryDetails.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = AppEnums.EntityStatus.DELETED;
                            });
                            _context.UpdateRange(tDCInstallmentTemporaryDetails);
                        }

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Sửa hồ sơ giá bán trả góp", "TDCInstallmentPrice", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (input.Id > 0)
                            transaction.Commit();
                        else
                            transaction.Rollback();

                        def.meta = new Meta(200, ApiConstants.MessageResource.UPDATE_SUCCESS);
                        def.data = data;
                        return Ok(def);
                    }
                    catch (DbUpdateException e)
                    {
                        transaction.Rollback();
                        log.Error("DbUpdateException:" + e);
                        if (!TDCInstallmentPriceExists(data.Id))
                        {
                            def.meta = new Meta(212, "Không tồn tại Id trên hệ thống!");
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
                log.Error("Put Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        // DELETE: api/TDCInstallmentPrice/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
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
                TDCInstallmentPrice data = await _context.TDCInstallmentPrices.FindAsync(id);
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_DELETE_MESSAGE);
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    data.UpdatedAt = DateTime.Now;
                    data.UpdatedById = userId;
                    data.UpdatedBy = fullName;
                    data.Status = AppEnums.EntityStatus.DELETED;
                    _context.Update(data);

                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Xóa thông tin hệ số phân bổ các tầng", "DistributionFloorCoefficient", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.DELETED, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (data.Id > 0)
                            transaction.Commit();
                        else
                            transaction.Rollback();

                        def.meta = new Meta(200, ApiConstants.MessageResource.DELETE_SUCCESS);
                        def.data = data;
                        return Ok(def);
                    }
                    catch (DbUpdateException e)
                    {
                        transaction.Rollback();
                        log.Error("DbUpdateException:" + e);
                        if (!TDCInstallmentPriceExists(data.Id))
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

        private bool TDCInstallmentPriceExists(int id)
        {
            return _context.TDCInstallmentPrices.Count(e => e.Id == id) > 0;
        }

        [HttpPost("ImportExcel/{Id}")]
        public async Task<IActionResult> ImportExcel(int id, [FromBody] List<InstallmentPriceTableMetaTdcData> input)
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
                        List<InstallmentPriceTableMetaTdc> lst = _context.InstallmentPriceTableMetaTdcs.Where(x => x.TdcIntallmentPriceId == id && x.Status != AppEnums.EntityStatus.DELETED).ToList();
                        lst.ForEach(x => x.Status = AppEnums.EntityStatus.DELETED);
                        _context.UpdateRange(lst);
                        await _context.SaveChangesAsync();

                        foreach (var item in input)
                        {
                            item.TdcIntallmentPriceId = id;
                            item.CreatedById = userId;
                            item.CreatedBy = fullName;
                            _context.InstallmentPriceTableMetaTdcs.Add(item);

                            await _context.SaveChangesAsync();

                            //installmentPriceTableTdcs
                            if (item.installmentPriceTableTdcs != null)
                            {
                                foreach (var installmentPriceTableTdc in item.installmentPriceTableTdcs)
                                {
                                    installmentPriceTableTdc.InstallmentPriceTableMetaTdcId = item.Id;
                                    installmentPriceTableTdc.CreatedBy = fullName;
                                    installmentPriceTableTdc.CreatedById = userId;

                                    _context.InstallmentPriceTableTdcs.Add(installmentPriceTableTdc);
                                }
                            }

                            //thêm LogAction
                            LogActionModel logActionModel = new LogActionModel("Thêm data file excel import", "TDCInstallmentPrice", item.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
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

        // Xuất file dữ liệu từ database lên excel
        [HttpPost("ExportExcel/{Id}")]
        public async Task<IActionResult> ExportExcel(int id, [FromBody] List<TdcInstallmentPriceGroupByPayTimeId> input)
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

            //Lấy ra hồ sơ bán trả góp
            TDCInstallmentPrice dataTdc = _context.TDCInstallmentPrices.Where(x => x.Id == id && x.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
            TDCProject project = _context.TDCProjects.Where(x => x.Id == dataTdc.TdcProjectId && x.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
            if (dataTdc == null || project == null)
            {
                def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                return Ok(def);
            }
            TDCInstallmentPriceData mapper_dataTdc = _mapper.Map<TDCInstallmentPriceData>(dataTdc);
            mapper_dataTdc.CustomerName = _context.TdcCustomers.Where(f => f.Id == mapper_dataTdc.TdcCustomerId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.FullName).FirstOrDefault();

            mapper_dataTdc.TdcProjectName = _context.TDCProjects.Where(f => f.Id == mapper_dataTdc.TdcProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

            mapper_dataTdc.TdcLandName = _context.Lands.Where(f => f.Id == mapper_dataTdc.LandId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

            mapper_dataTdc.TdcBlockHouseName = _context.BlockHouses.Where(f => f.Id == mapper_dataTdc.BlockHouseId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

            mapper_dataTdc.TdcFloorName = _context.FloorTdcs.Where(f => f.Id == mapper_dataTdc.FloorTdcId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

            mapper_dataTdc.TdcApartmentName = _context.ApartmentTdcs.Where(f => f.Id == mapper_dataTdc.TdcApartmentId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

            // khởi tạo wb rỗng
            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"templates/BangChietTinhHoSoBanTraGop.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);
            string nameExcel = "BangChietTinh_" + mapper_dataTdc.DecreeNumber + ".xls";

            MemoryStream ms = WriteDataToExcel(templatePath, 0, input, mapper_dataTdc, project);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", nameExcel);
        }

        private static MemoryStream WriteDataToExcel(string templatePath, int sheetnumber, List<TdcInstallmentPriceGroupByPayTimeId> data, TDCInstallmentPriceData dataTdc, TDCProject dataProject)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowStart = 15;

            if (sheet != null)
            {
                int datacol = 18;
                try
                {
                    // Đặt giá trị của dataTdc.YearPay vào ô H9
                    IRow rowH9 = sheet.GetRow(8);
                    ICell cellH9 = rowH9.GetCell(7);
                    cellH9.SetCellValue((double)dataTdc.YearPay);

                    // Ghi giá trị dataTdc.CustomerName vào ô B4
                    IRow rowB4 = sheet.GetRow(3);
                    ICell cellB4 = rowB4.GetCell(1);
                    cellB4.SetCellValue(dataTdc.CustomerName);

                    // Ghi giá trị dataTdc.HouseNumber vào ô B5
                    IRow rowB5 = sheet.GetRow(4);
                    ICell cellB5 = rowB5.GetCell(1);
                    cellB5.SetCellValue(dataProject.HouseNumber);

                    // Ghi giá trị dataTdc.FirstPayDate vào ô H8
                    IRow rowH8 = sheet.GetRow(7);
                    ICell cellH8 = rowH8.GetCell(7);
                    cellH8.SetCellValue(dataTdc.FirstPayDate);

                    // Ghi giá trị dataTdc.TdcBlockHouseName vào ô B6
                    IRow rowB6 = sheet.GetRow(5);
                    ICell cellB6 = rowB6.GetCell(1);
                    cellB6.SetCellValue(dataTdc.TdcBlockHouseName);

                    // Ghi giá trị dataTdc.ContractNumber vào ô K4
                    IRow rowK4 = sheet.GetRow(3);
                    ICell cellK4 = rowK4.GetCell(10);
                    cellK4.SetCellValue(dataTdc.ContractNumber);

                    // Ghi giá trị dataTdc.DateNumber vào ô K5
                    IRow rowK5 = sheet.GetRow(4);
                    ICell cellK5 = rowK5.GetCell(10);
                    cellK5.SetCellValue(dataTdc.DateNumber);

                    // Ghi giá trị dataTdc.OldContractValue vào ô K6
                    IRow rowK6 = sheet.GetRow(5);
                    ICell cellK6 = rowK6.GetCell(10);
                    cellK6.SetCellValue((double)dataTdc.OldContractValue);

                    // Ghi giá trị dataTdc.NewContractValue vào ô K7
                    IRow rowK7 = sheet.GetRow(6);
                    ICell cellK7 = rowK7.GetCell(10);
                    cellK7.SetCellValue((double)dataTdc.NewContractValue);

                    // Ghi giá trị dataTdc.FirstPay vào ô K8
                    IRow rowK8 = sheet.GetRow(7);
                    ICell cellK8 = rowK8.GetCell(10);
                    cellK8.SetCellValue((double)dataTdc.FirstPay);

                    // Ghi giá trị 'dataTdc.Floor1(dataTdc.TdcFloorName)' vào ô G5
                    IRow rowG5 = sheet.GetRow(4);
                    ICell cellG5 = rowG5.GetCell(6);
                    cellG5.SetCellValue(dataTdc.Floor1 + " (" + dataTdc.TdcFloorName + ")");

                    // Ghi giá trị dataTdc.TdcLandName vào ô G6
                    IRow rowG6 = sheet.GetRow(5);
                    ICell cellG6 = rowG6.GetCell(6);
                    cellG6.SetCellValue(dataTdc.TdcLandName);

                    // Ghi giá trị dataTdc.TemporaryDecreeDate vào ô H10
                    IRow rowH10 = sheet.GetRow(9);
                    ICell cellH10 = rowH10.GetCell(7);
                    if (dataTdc.TemporaryDecreeDate.HasValue)
                    {
                        cellH10.SetCellValue(dataTdc.TemporaryDecreeDate.Value);
                    }
                    else
                    {
                        cellH10.SetCellValue(string.Empty);
                    }

                    // Ghi giá trị dataTdc.DecreeDate vào ô H11
                    IRow rowH11 = sheet.GetRow(10);
                    ICell cellH11 = rowH11.GetCell(7);
                    cellH11.SetCellValue(dataTdc.DecreeDate);
                    //style body
                    List<ICellStyle> rowStyle = new List<ICellStyle>();
                    for (int i = 0; i < datacol; i++)
                    {
                        rowStyle.Add(sheet.GetRow(rowStart).GetCell(i).CellStyle);
                    }

                    //Thêm row
                    foreach (var item in data)
                    {
                        int firstRow = rowStart;
                        foreach (var childItem in item.tdcInstallmentPriceTables)
                        {
                            XSSFRow row = (XSSFRow)sheet.CreateRow(rowStart);
                            for (int i = 0; i < datacol; i++)
                            {
                                row.CreateCell(i).CellStyle = rowStyle[i];
                                if (i == 0 && childItem.PaymentTimes.HasValue)
                                {
                                    row.GetCell(i).SetCellValue((double)childItem.PaymentTimes);
                                }
                                else if (i == 1)
                                {

                                    if (childItem.PayDateDefault.HasValue)
                                    {
                                        row.GetCell(i).SetCellValue((DateTime)childItem.PayDateDefault);
                                    }
                                    if (childItem.RowStatus == AppEnums.TypePayQD.NO_CU)
                                    {
                                        row.GetCell(i).CellStyle.Alignment = HorizontalAlignment.General;
                                        row.GetCell(i).SetCellValue("Nợ Cũ");
                                    }
                                    if (childItem.RowStatus == AppEnums.TypePayQD.TRE_HAN)
                                    {
                                        row.GetCell(i).CellStyle.Alignment = HorizontalAlignment.General;
                                        row.GetCell(i).SetCellValue("Phạt trễ hạn");
                                    }
                                    if (childItem.RowStatus == AppEnums.TypePayQD.TONG)
                                    {
                                        row.GetCell(i).CellStyle.Alignment = HorizontalAlignment.General;
                                        row.GetCell(i).SetCellValue("Tổng cộng");
                                    }
                                    if (childItem.RowStatus == AppEnums.TypePayQD.DONG_DU)
                                    {
                                        row.GetCell(i).CellStyle.Alignment = HorizontalAlignment.General;
                                        row.GetCell(i).SetCellValue("SỐ TIỀN ĐÓNG DƯ CỦA LẦN TRƯỚC");
                                    }
                                }
                                else if (i == 2 && childItem.PayDateBefore.HasValue)
                                {
                                    row.GetCell(i).SetCellValue((DateTime)childItem.PayDateBefore);
                                }
                                else if (i == 3 && childItem.PayDateGuess.HasValue)
                                {
                                    row.GetCell(i).SetCellValue((DateTime)childItem.PayDateGuess);
                                }
                                else if (i == 4 && childItem.PayDateReal.HasValue)
                                {
                                    if (childItem.DataStatus == 1)
                                        row.GetCell(i).SetCellValue((DateTime)childItem.PayDateReal);
                                }
                                else if (i == 5 && childItem.MonthInterest.HasValue)
                                {
                                    row.GetCell(i).SetCellValue((double)childItem.MonthInterest);
                                }
                                else if (i == 6 && childItem.DailyInterest.HasValue)
                                {
                                    row.GetCell(i).SetCellValue((double)childItem.DailyInterest);
                                }
                                else if (i == 7 && childItem.MonthInterestRate.HasValue)
                                {
                                    row.GetCell(i).SetCellValue((double)childItem.MonthInterestRate);
                                }
                                else if (i == 8 && childItem.DailyInterestRate.HasValue)
                                {
                                    row.GetCell(i).SetCellValue((double)childItem.DailyInterestRate);
                                }
                                else if (i == 9 && childItem.TotalPay.HasValue)
                                {
                                    row.GetCell(i).SetCellValue((double)childItem.TotalPay);
                                }
                                else if (i == 10 && childItem.PayAnnual.HasValue)
                                {
                                    row.GetCell(i).SetCellValue((double)childItem.PayAnnual);
                                }
                                else if (i == 11 && childItem.TotalInterest.HasValue)
                                {
                                    row.GetCell(i).SetCellValue((double)childItem.TotalInterest);
                                }
                                else if (i == 12 && childItem.TotalPayAnnual.HasValue)
                                {
                                    row.GetCell(i).SetCellValue((double)childItem.TotalPayAnnual);
                                }

                                else if (i == 14 && childItem.Paid.HasValue)
                                {
                                    row.GetCell(i).SetCellValue((double)childItem.Paid);
                                }
                                else if (i == 15 && childItem.PriceDifference.HasValue)
                                {
                                    row.GetCell(i).SetCellValue((double)childItem.PriceDifference);
                                }
                                else if (i == 16 && childItem.Note != null)
                                {
                                    row.GetCell(i).SetCellValue(childItem.Note);
                                }
                                else if (i == 17)
                                {
                                    if (childItem.publicPay == null || childItem.publicPay == false)
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

                        }
                        int lastRow = rowStart - 1;
                        if (lastRow > firstRow)
                        {

                            CellRangeAddress mergedRegionPay = new CellRangeAddress(firstRow, lastRow, 13, 13);
                            sheet.AddMergedRegion(mergedRegionPay);

                            CellRangeAddress mergedRegionPaid = new CellRangeAddress(firstRow, lastRow, 14, 14);
                            sheet.AddMergedRegion(mergedRegionPaid);

                            CellRangeAddress mergedRegionDiff = new CellRangeAddress(firstRow, lastRow, 15, 15);
                            sheet.AddMergedRegion(mergedRegionDiff);

                            CellRangeAddress mergedRegionPricePublic = new CellRangeAddress(firstRow, lastRow, 17, 17);
                            sheet.AddMergedRegion(mergedRegionPricePublic);

                        }
                        IRow rowMerge = sheet.GetRow(firstRow);
                        if (item.Pay.HasValue)
                        {
                            ICell cellPay = rowMerge.GetCell(13);
                            cellPay.SetCellValue((double)item.Pay);
                        }
                        if (item.Paid.HasValue)
                        {
                            ICell cellPaid = rowMerge.GetCell(14);
                            cellPaid.SetCellValue((double)item.Paid);
                        }
                        if (item.PriceDifference.HasValue)
                        {
                            ICell cellDiff = rowMerge.GetCell(15);
                            cellDiff.SetCellValue((double)item.PriceDifference);
                        }
                    }
                    rowStart += 2;
                    ICellStyle style1 = workbook.CreateCellStyle();
                    style1.Alignment = HorizontalAlignment.Center;
                    style1.VerticalAlignment = VerticalAlignment.Center;
                    IFont font = workbook.CreateFont();
                    font.Boldweight = (short)FontBoldWeight.Bold;
                    font.FontName = "Times New Roman";
                    style1.SetFont(font);
                    style1.WrapText = true;
                    ICellStyle style2 = workbook.CreateCellStyle();
                    style2.VerticalAlignment = VerticalAlignment.Center;
                    IFont font2 = workbook.CreateFont();
                    font2.FontName = "Times New Roman";
                    style2.SetFont(font2);
                    ICellStyle style3 = workbook.CreateCellStyle();
                    style3.Alignment = HorizontalAlignment.Center;
                    style3.VerticalAlignment = VerticalAlignment.Center;
                    IFont font3 = workbook.CreateFont();
                    font3.FontName = "Times New Roman";
                    style2.SetFont(font3);

                    XSSFRow row1 = (XSSFRow)sheet.CreateRow(rowStart);
                    row1.CreateCell(1).CellStyle = style2;
                    row1.CreateCell(2).CellStyle = style2;
                    rowStart++;

                    XSSFRow row2 = (XSSFRow)sheet.CreateRow(rowStart);
                    row2.CreateCell(1).CellStyle = style2;
                    row2.CreateCell(2).CellStyle = style2;
                    rowStart++;

                    XSSFRow row3 = (XSSFRow)sheet.CreateRow(rowStart);
                    row3.CreateCell(1).CellStyle = style2;
                    row3.CreateCell(2).CellStyle = style2;
                    rowStart++;

                    XSSFRow row4 = (XSSFRow)sheet.CreateRow(rowStart);
                    row4.CreateCell(1).CellStyle = style2;
                    row4.CreateCell(2).CellStyle = style2;
                    rowStart++;

                    XSSFRow row5 = (XSSFRow)sheet.CreateRow(rowStart);
                    row5.CreateCell(1).CellStyle = style2;
                    row5.CreateCell(2).CellStyle = style2;
                    rowStart++;

                    XSSFRow row6 = (XSSFRow)sheet.CreateRow(rowStart);
                    row6.CreateCell(13).CellStyle = style3;
                    rowStart++;

                    XSSFRow row7 = (XSSFRow)sheet.CreateRow(rowStart);
                    row7.CreateCell(7).CellStyle = style1;
                    row7.CreateCell(13).CellStyle = style1;
                    rowStart++;

                    XSSFRow row8 = (XSSFRow)sheet.CreateRow(rowStart);
                    row8.CreateCell(2).CellStyle = style1;
                    row8.CreateCell(8).CellStyle = style1;
                    row8.CreateCell(13).CellStyle = style1;

                    row1.GetCell(1).SetCellValue("(4)");
                    row1.GetCell(2).SetCellValue("Số tháng khách hàng phải chịu lãi suất tiền gửi tiết kiệm không kỳ hạn của Ngân hàng Nhà nước (NHNN)");


                    row2.GetCell(1).SetCellValue("(5)");
                    row2.GetCell(2).SetCellValue("Dùng để tính đối với khách hàng thanh toán không đúng ngày quy định, số ngày phát sinh chưa đủ 1 tháng (có thể từ 1 đến 30 ngày)");


                    row3.GetCell(1).SetCellValue("(6),(7)");
                    row3.GetCell(2).SetCellValue("Đến kỳ thanh toán lãi suất sẽ được sử dụng theo số liệu công bố của NHNN tại thời điểm thanh toán theo tháng và ngày");


                    row4.GetCell(1).SetCellValue("(10)");
                    row4.GetCell(2).SetCellValue("Số tiền lãi phát sinh tính đến thời điểm thanh toán thực tế của khách hàng theo công thức sau:");


                    row5.GetCell(1).SetCellValue("(12)");
                    row5.GetCell(2).SetCellValue("Tổng số tiền khách hàng phải thanh toán thực tế hàng năm gồm nợ gốc, lãi suất phát sinh và nợ cũ (nếu có)");

                    row6.GetCell(13).SetCellValue("TP. Hồ Chí Minh, Ngày        tháng       năm 2023");

                    row7.GetCell(7).SetCellValue("PHÒNG QUẢN LÝ NHÀ, ĐẤT TÁI ĐỊNH CƯ");
                    row7.GetCell(13).SetCellValue("KT GIÁM ĐÓC");


                    row8.GetCell(2).SetCellValue("NGƯỜI LẬP");
                    row8.GetCell(8).SetCellValue("PHÓ TRƯỞNG PHÒNG");
                    row8.GetCell(13).SetCellValue("PHÓ GIÁM ĐỐC");

                    CellRangeAddress mergeNQ6 = new CellRangeAddress(rowStart - 2, rowStart - 2, 13, 16);
                    CellRangeAddress mergeNQ7 = new CellRangeAddress(rowStart - 1, rowStart - 1, 13, 16);
                    CellRangeAddress mergeCE7 = new CellRangeAddress(rowStart, rowStart, 2, 4);
                    CellRangeAddress mergeNQ8 = new CellRangeAddress(rowStart, rowStart, 13, 16);
                    CellRangeAddress mergeHK7 = new CellRangeAddress(rowStart - 1, rowStart - 1, 7, 10);
                    CellRangeAddress mergeJK8 = new CellRangeAddress(rowStart, rowStart, 8, 10);
                    UtilsService.MergeCellRanges(sheet, mergeNQ6, mergeNQ7, mergeCE7, mergeNQ8, mergeHK7, mergeJK8);
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

        //Mẫu báo cáo số 1
        [HttpPost("GetReportTable/{Id}")]
        public async Task<IActionResult> GetReportTable(int id, [FromBody] List<TdcInstallmentPriceGroupByPayTimeId> input)
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
                TDCInstallmentPrice tDCInstallmentPrice = _context.TDCInstallmentPrices.Where(l => l.Id == id && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                if (tDCInstallmentPrice == null)
                {
                    def.meta = new Meta(404, "Không tìm thấy hồ sơ tương ứng!");
                    return Ok(def);
                }
                var tempTotalPriceMainValue = tDCInstallmentPrice.TemporaryTotalPrice.GetValueOrDefault();
                //Khai bao bien
                decimal TotalCt = 0;
                decimal TotalTt = 0;

                decimal TotalPatTt = 0;
                decimal TotalPatCt = 0;

                decimal IngredientPriceTt = 0;
                decimal IngredientPriceCt = 0;

                decimal sumOriginTt = 0;
                decimal sumOriginCt = 0;

                decimal sumTotalIngreTt = 0;
                decimal sumTotalIngreCt = 0;

                decimal sumTotalPercent = 0;

                //
                List<TdcInstallmentPriceReport> res = new List<TdcInstallmentPriceReport>();
                TdcInstallmentPriceReport data = new TdcInstallmentPriceReport();
                data.reportIngreTemporarys = new List<ReportIngre>();
                data.reportPatTemporarys = new List<ReportPat>();
                data.reportPats = new List<ReportPat>();
                data.reportIngres = new List<ReportIngre>();
                TDCInstallmentPriceData mapper_data = _mapper.Map<TDCInstallmentPriceData>(tDCInstallmentPrice);
                data.payDetails = new List<PayDetail>();
                data.taxes = new List<TaxEx>();

                List<TDCProjectIngrePrice> tDCProjectIngrePrices = _context.TDCProjectIngrePrices.Where(x => x.TDCProjectId == tDCInstallmentPrice.TdcProjectId && x.Status != AppEnums.EntityStatus.DELETED).ToList();

                //lấy value TP giá bán cấu thành tạm thời
                List<TDCInstallmentTemporaryDetail> tDCInstallmentTemporaryDetails = _context.TDCInstallmentTemporaryDetails.Where(l => l.TDCInstallmentPriceId == tDCInstallmentPrice.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                foreach (TDCInstallmentTemporaryDetail i in tDCInstallmentTemporaryDetails)
                {
                    var newItem = new ReportIngre();

                    newItem.Id = i.IngredientsPriceId;
                    newItem.IngrePriceName = _context.IngredientsPrices.Where(f => f.Id == i.IngredientsPriceId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                    newItem.Area = i.Area;
                    newItem.UnitPrice = i.UnitPrice;
                    newItem.Price = i.Price;
                    newItem.Value = tDCProjectIngrePrices.Where(x => x.IngredientsPriceId == i.IngredientsPriceId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Value).FirstOrDefault();

                    data.reportIngreTemporarys.Add(newItem);
                    if(newItem.Value != 0)
                    {
                        TotalTt += (newItem.Price / (decimal)newItem.Value);
                    }
                }

                //lấy value TP giá bán cấu thành chính thức
                List<TDCInstallmentOfficialDetail> tDCInstallmentOfficialDetails = _context.TDCInstallmentOfficialDetails.Where(l => l.TDCInstallmentPriceId == tDCInstallmentPrice.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                foreach (TDCInstallmentOfficialDetail i in tDCInstallmentOfficialDetails)
                {
                    var newItem = new ReportIngre();

                    newItem.Id = i.IngredientsPriceId;
                    newItem.IngrePriceName = _context.IngredientsPrices.Where(f => f.Id == i.IngredientsPriceId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault(); ;
                    newItem.Area = i.Area;
                    newItem.UnitPrice = i.UnitPrice;
                    newItem.Price = i.Price;
                    newItem.Value = tDCProjectIngrePrices.Where(x => x.IngredientsPriceId == i.IngredientsPriceId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Value).FirstOrDefault();

                    data.reportIngres.Add(newItem);
                    if (newItem.Value != 0)
                    {
                        TotalCt += (newItem.Price / (decimal)newItem.Value);
                    }
                }

                List<TDCProjectPriceAndTax> tDCProjectPriceAndTaxs = _context.TDCProjectPriceAndTaxs.Where(l => l.TDCProjectId == tDCInstallmentPrice.TdcProjectId && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<ReportIngre> ingreDataDetails = new List<ReportIngre>();
                List<TDCProjectPriceAndTaxData> map_TDCProjectPriceAndTaxs = _mapper.Map<List<TDCProjectPriceAndTaxData>>(tDCProjectPriceAndTaxs);
                foreach (TDCProjectPriceAndTaxData map_TDCProjectPriceAndTax in map_TDCProjectPriceAndTaxs)
                {
                    List<TDCProjectPriceAndTaxDetails> de = _context.TDCProjectPriceAndTaxDetailss.Where(f => f.PriceAndTaxId == map_TDCProjectPriceAndTax.Id && f.Status != AppEnums.EntityStatus.DELETED).ToList();
                    List<TDCProjectPriceAndTaxDetailData> detail = _mapper.Map<List<TDCProjectPriceAndTaxDetailData>>(de);
                    OriginalPriceAndTax pat = _context.OriginalPriceAndTaxs.Where(f => f.Id == map_TDCProjectPriceAndTax.PriceAndTaxId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    if (de == null) map_TDCProjectPriceAndTax.PATName = "";
                    else map_TDCProjectPriceAndTax.PATName = pat.Name;

                    var newItemPATTemporary = new ReportPat();
                    newItemPATTemporary.PATName = pat.Name;
                    newItemPATTemporary.Value = map_TDCProjectPriceAndTax.Value;
                    newItemPATTemporary.Location = map_TDCProjectPriceAndTax.Location;
                    newItemPATTemporary.PercentTemporary = 0;
                    newItemPATTemporary.PriceTemporary = 0;
                    newItemPATTemporary.Percent = 0;
                    newItemPATTemporary.Price = 0;
                    newItemPATTemporary.reportIngres = new List<ReportIngre>();
                    TotalPatTt = 0;
                    sumOriginTt = 0;
                    sumTotalIngreTt = 0;
                    foreach (TDCProjectPriceAndTaxDetailData i in detail)
                    {
                        ReportIngre newItem = new ReportIngre();
                        newItem = data.reportIngreTemporarys.Where(x => x.Id == i.IngredientsPriceId).FirstOrDefault();

                        newItemPATTemporary.reportIngres.Add(newItem);
                        if(newItem.Value != 0)
                        {
                            TotalPatTt += (newItem.Price / (decimal)newItem.Value);
                        }
                        sumOriginTt += TotalPatTt;
                        if (newItemPATTemporary.Value != 0)
                        {
                            IngredientPriceTt = ((TotalPatTt * (decimal)newItemPATTemporary.Value) / 100);
                        }
                        sumTotalIngreTt += IngredientPriceTt;
                    }

                    if (newItemPATTemporary.PATName == "VAT")
                    {
                        if(tempTotalPriceMainValue == 0 || newItemPATTemporary.Value == 0)
                        {
                            newItemPATTemporary.Percent = 0;
                        }
                        else
                        {
                            newItemPATTemporary.Percent = Math.Round(((TotalTt / tempTotalPriceMainValue) * 100) / (decimal)newItemPATTemporary.Value);
                        }
                    }
                    else
                    {
                        if(tempTotalPriceMainValue == 0)
                        {
                            newItemPATTemporary.Percent = 0;
                        }
                        else
                        {
                            newItemPATTemporary.Percent = Math.Round(TotalPatTt * (decimal)newItemPATTemporary.Value / tempTotalPriceMainValue);
                        }
                    }
                    data.reportPatTemporarys.Add(newItemPATTemporary);

                    var newItemPAT = new ReportPat();
                    newItemPAT.PATName = pat.Name;
                    newItemPAT.Value = map_TDCProjectPriceAndTax.Value;
                    newItemPAT.Location = map_TDCProjectPriceAndTax.Location;
                    newItemPAT.PercentTemporary = 0;
                    newItemPAT.PriceTemporary = 0;
                    newItemPAT.Percent = 0;
                    newItemPAT.Price = 0;
                    newItemPAT.reportIngres = new List<ReportIngre>();
                    TotalPatCt = 0;
                    sumOriginCt = 0;
                    sumTotalIngreCt = 0;
                    foreach (TDCProjectPriceAndTaxDetailData i in detail)
                    {
                        ReportIngre newItem = new ReportIngre();
                        newItem = data.reportIngres.Where(x => x.Id == i.IngredientsPriceId).FirstOrDefault();
                        newItemPAT.reportIngres.Add(newItem);
                        if(newItem.Value != 0)
                        {
                            TotalPatCt += (newItem.Price / (decimal)newItem.Value);
                        }
                        sumOriginCt += TotalPatCt;
                        if(newItemPAT.Value != 0)
                        {
                            IngredientPriceCt = ((TotalPatCt * (decimal)newItemPAT.Value) / 100);
                        }
                        sumTotalIngreCt += IngredientPriceCt;
                    }
                    if (newItemPAT.PATName == "VAT")
                    {
                        if (tDCInstallmentPrice.TotalPrice ==0 || newItemPAT.Value ==0)
                        {
                            newItemPAT.Percent = 0;
                        }
                        else
                        {
                            newItemPAT.Percent = Math.Round(((TotalCt / tDCInstallmentPrice.TotalPrice) * 100) / (decimal)newItemPAT.Value);
                        }
                    }
                    else
                    {
                        if (tDCInstallmentPrice.TotalPrice == 0 || newItemPAT.Value ==0)
                        {
                            newItemPAT.Percent = 0;
                        }
                        else
                        {
                            newItemPAT.Percent = Math.Round(TotalPatCt * (decimal)newItemPAT.Value / tDCInstallmentPrice.TotalPrice);
                        }
                    }

                    data.reportPats.Add(newItemPAT);
                }

                //Thuế phí nông nghiệp
                List<TDCInstallmentPriceAndTax> tdcPriceRentTaxes = _context.TDCInstallmentPriceAndTaxs.Where(l => l.TDCInstallmentPriceId == tDCInstallmentPrice.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(x => x.Id).ToList();
                foreach (var t in tdcPriceRentTaxes)
                {
                    var itemTax = new TaxEx();
                    itemTax.Year = t.Year;
                    itemTax.Price = t.Value;

                    data.taxes.Add(itemTax);
                }

                data.ContractNumber = tDCInstallmentPrice.ContractNumber;
                data.DateNumber = tDCInstallmentPrice.DateNumber;
                data.CustomerName = _context.TdcCustomers.Where(x => x.Id == tDCInstallmentPrice.TdcCustomerId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.FullName).FirstOrDefault();
                data.TdcApartmentName = _context.ApartmentTdcs.Where(x => x.Id == tDCInstallmentPrice.TdcApartmentId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault();
                data.TdcBlockHouseName = _context.BlockHouses.Where(x => x.Id == tDCInstallmentPrice.BlockHouseId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault();
                data.TdcLandName = _context.Lands.Where(x => x.Id == tDCInstallmentPrice.LandId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault();
                data.TdcFloorName = _context.FloorTdcs.Where(x => x.Id == tDCInstallmentPrice.FloorTdcId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault();
                data.Corner = tDCInstallmentPrice.Corner;
                data.Floor1 = tDCInstallmentPrice.Floor1;
                data.TemporaryTotalArea = tDCInstallmentPrice.TemporaryTotalArea;
                data.TemporaryTotalPrice = tDCInstallmentPrice.TemporaryTotalPrice;
                data.TemporaryDecreeNumber = tDCInstallmentPrice.TemporaryDecreeNumber;
                data.TemporaryDecreeDate = tDCInstallmentPrice.TemporaryDecreeDate;
                data.TotalArea = tDCInstallmentPrice.TotalArea;
                data.TotalPrice = tDCInstallmentPrice.TotalPrice;
                data.DecreeNumber = tDCInstallmentPrice.DecreeNumber;
                data.DecreeDate = tDCInstallmentPrice.DecreeDate;
                data.FirstPayDate = tDCInstallmentPrice.FirstPayDate;

                if(tempTotalPriceMainValue == 0)
                {
                    data.OriginalPrecentTemprorary = 0;
                }
                else
                {
                    data.OriginalPrecentTemprorary = Math.Round((TotalTt / tempTotalPriceMainValue) * 100);
                }


                if(tDCInstallmentPrice.TotalPrice == 0)
                {
                    data.OriginalPrecent = 0;
                }
                else
                {
                    data.OriginalPrecent = Math.Round((TotalCt / tDCInstallmentPrice.TotalPrice) * 100);
                }

                List<TdcInstallmentPriceGroupByPayTimeId> listA = new List<TdcInstallmentPriceGroupByPayTimeId>();

                for (int z = 0; z < input.Count; z++)
                {
                    if (input[z].Paid != 0)
                    {
                        if (input[z].Pay != 0 && input[z].tdcInstallmentPriceTables[0].RowStatus != AppEnums.TypePayQD.NO_CU && input[z].tdcInstallmentPriceTables[0].RowStatus != AppEnums.TypePayQD.TONG)
                        {
                            listA.Add(input[z]);
                        }
                    }
                }
                for (int i = 0; i < listA.Count; i++)
                {
                    var itemExcel = new PayDetail();

                    foreach (var childItem in listA[i].tdcInstallmentPriceTables)  ///Cần fix lại nợ cũ
                    {
                        if (listA[i].tdcInstallmentPriceTables.First().PaymentTimes != null)
                        {
                            itemExcel.index = (int)(listA[i].tdcInstallmentPriceTables.First().PaymentTimes);
                        }

                        itemExcel.PayDateDefault = listA[i].tdcInstallmentPriceTables.First().PayDateDefault;

                        itemExcel.PayDateReal = listA[i].tdcInstallmentPriceTables.Last().PayDateReal;

                        if (itemExcel.PayAnnual != null && childItem.PayAnnual != null)
                        {
                            if (i > 0)
                            {
                                itemExcel.PayAnnual = (decimal)(listA[i].tdcInstallmentPriceTables.Sum(x => x.PayAnnual) + listA[i - 1].PriceDifference); //FIX 
                            }
                            else
                            {
                                itemExcel.PayAnnual = (decimal)childItem.PayAnnual;
                            }
                        }

                        itemExcel.TotalInterest += childItem.TotalInterest;

                        itemExcel.OverPay = (decimal)listA[i].PriceDifference;

                        itemExcel.payDetailChilds = new List<PayDetailChild>();

                        if (itemExcel.PayAnnual == null) itemExcel.PayAnnual = 0;
                        if (itemExcel.TotalInterest == null) itemExcel.TotalInterest = 0;
                        if (itemExcel.Fines == null) itemExcel.Fines = 0;

                        if (childItem.PayDateDefault < tDCInstallmentPrice.DecreeDate)
                        {
                            itemExcel.PrinCipal = (decimal)((itemExcel.PayAnnual + itemExcel.TotalInterest + itemExcel.Fines) * (data.OriginalPrecentTemprorary / 100));

                            foreach (var x in data.reportPatTemporarys)
                            {
                                var childItemExcel = new PayDetailChild();
                                childItemExcel.Name = x.PATName;
                                if (x.PATName != "VAT")
                                {
                                    childItemExcel.TotalValue = (decimal)((itemExcel.PayAnnual + itemExcel.TotalInterest + itemExcel.Fines) * (x.Percent / 100));
                                }

                                itemExcel.payDetailChilds.Add(childItemExcel);
                                sumTotalPercent = itemExcel.payDetailChilds.Sum(x => x.TotalValue);
                            }
                        }
                        else
                        {
                            itemExcel.PrinCipal = (decimal)((itemExcel.PayAnnual + itemExcel.TotalInterest + itemExcel.Fines) * (data.OriginalPrecent / 100));

                            foreach (var x in data.reportPats)
                            {
                                var childItemExcel = new PayDetailChild();
                                childItemExcel.Name = x.PATName;

                                if (x.PATName != "VAT")
                                {
                                    childItemExcel.TotalValue = (decimal)((itemExcel.PayAnnual + itemExcel.TotalInterest + itemExcel.Fines) * (x.Percent / 100));
                                }

                                itemExcel.payDetailChilds.Add(childItemExcel);
                                sumTotalPercent = itemExcel.payDetailChilds.Sum(x => x.TotalValue);
                            }
                        }
                    }
                    itemExcel.VAT = (decimal)((itemExcel.PayAnnual + itemExcel.TotalInterest + itemExcel.Fines) - (itemExcel.PrinCipal + sumTotalPercent));
                    data.payDetails.Add(itemExcel);
                }
                res.Add(data);

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

        [HttpPost("ExportReport")]
        public async Task<IActionResult> ExportReport([FromBody] List<TdcInstallmentPriceReport> input)
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
            string nameExcel = "BaoCao_.xls";

            MemoryStream ms = WriteDataToExcel(templatePath, 0, input);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", nameExcel);
        }
        private static MemoryStream WriteDataToExcel(string templatePath, int sheetnumber, List<TdcInstallmentPriceReport> data)
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
                    foreach (var item in data)
                    {
                        foreach (var e in item.payDetails)
                        {
                            childExcelCount += (e.payDetailChilds != null ? e.payDetailChilds.Count : 0);
                        }
                        datacol = 18 + ((item.reportIngreTemporarys != null ? item.reportIngreTemporarys.Count : 0) * 3) + ((item.reportIngres != null ? item.reportIngres.Count : 0) * 3) + ((item.payDetails != null ? item.payDetails.Count : 0) * 9 + (childExcelCount - 1) + ((item.taxes != null ? item.taxes.Count : 0) * 2));

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
                            sheet.SetColumnWidth(i, (int)(13.5 * 256));
                        }

                        ICell cellHeader1 = rowHeaders.CreateCell(0);
                        cellHeader1.SetCellValue("Chủ căn hộ");
                        cellHeader1.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader1 = new CellRangeAddress(rowHeader, rowHeader, 0, 2);
                        sheet.AddMergedRegion(mergedRegioncellHeader1);
                        RegionUtil.SetBorderBottom(1, mergedRegioncellHeader1, sheet);
                        RegionUtil.SetBorderLeft(1, mergedRegioncellHeader1, sheet);
                        RegionUtil.SetBorderRight(1, mergedRegioncellHeader1, sheet);
                        RegionUtil.SetBorderTop(1, mergedRegioncellHeader1, sheet);

                        ICell cellHeader2 = rowHeaders.CreateCell(3);
                        cellHeader2.SetCellValue("thông tin căn hộ");
                        cellHeader2.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader2 = new CellRangeAddress(rowHeader, rowHeader, 3, 8);
                        sheet.AddMergedRegion(mergedRegioncellHeader2);
                        RegionUtil.SetBorderBottom(1, mergedRegioncellHeader2, sheet);
                        RegionUtil.SetBorderLeft(1, mergedRegioncellHeader2, sheet);
                        RegionUtil.SetBorderRight(1, mergedRegioncellHeader2, sheet);
                        RegionUtil.SetBorderTop(1, mergedRegioncellHeader2, sheet);

                        int temporaryDataCount = 9;

                        foreach (var i in item.reportIngreTemporarys)
                        {
                            ICell cellHeader3 = rowHeaders.CreateCell(temporaryDataCount);
                            cellHeader3.SetCellValue(i.IngrePriceName);
                            cellHeader3.CellStyle = cellStyle;
                            CellRangeAddress mergedRegioncellHeader3 = new CellRangeAddress(rowHeader, rowHeader, temporaryDataCount, temporaryDataCount + 2);
                            sheet.AddMergedRegion(mergedRegioncellHeader3);
                            RegionUtil.SetBorderBottom(1, mergedRegioncellHeader3, sheet);
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeader3, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeader3, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeader3, sheet);

                            temporaryDataCount = temporaryDataCount + 3;
                        }

                        ICell cellHeader4 = rowHeaders.CreateCell(temporaryDataCount);
                        cellHeader4.SetCellValue("Giá bán(tạm)");
                        cellHeader4.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader4 = new CellRangeAddress(rowHeader, rowHeader, temporaryDataCount, temporaryDataCount + 1);
                        sheet.AddMergedRegion(mergedRegioncellHeader4);
                        RegionUtil.SetBorderBottom(1, mergedRegioncellHeader4, sheet);
                        RegionUtil.SetBorderLeft(1, mergedRegioncellHeader4, sheet);
                        RegionUtil.SetBorderRight(1, mergedRegioncellHeader4, sheet);
                        RegionUtil.SetBorderTop(1, mergedRegioncellHeader4, sheet);


                        ICell cellHeader5 = rowHeaders.CreateCell(temporaryDataCount + 2);
                        cellHeader5.SetCellValue("QĐ bố trí tạm(tạm)");
                        cellHeader5.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader5 = new CellRangeAddress(rowHeader, rowHeader, temporaryDataCount + 2, temporaryDataCount + 4);
                        sheet.AddMergedRegion(mergedRegioncellHeader5);
                        RegionUtil.SetBorderBottom(1, mergedRegioncellHeader5, sheet);
                        RegionUtil.SetBorderLeft(1, mergedRegioncellHeader5, sheet);
                        RegionUtil.SetBorderRight(1, mergedRegioncellHeader5, sheet);
                        RegionUtil.SetBorderTop(1, mergedRegioncellHeader5, sheet);

                        int officialDataCount = temporaryDataCount + 5;

                        foreach (var q in item.reportIngres)
                        {
                            ICell cellHeader6 = rowHeaders.CreateCell(officialDataCount);
                            cellHeader6.SetCellValue(q.IngrePriceName);
                            cellHeader6.CellStyle = cellStyle;
                            CellRangeAddress mergedRegioncellHeader6 = new CellRangeAddress(rowHeader, rowHeader, officialDataCount, officialDataCount + 2);
                            sheet.AddMergedRegion(mergedRegioncellHeader6);
                            RegionUtil.SetBorderBottom(1, mergedRegioncellHeader6, sheet);
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeader6, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeader6, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeader6, sheet);

                            officialDataCount = officialDataCount + 3;
                        }

                        ICell cellHeader7 = rowHeaders.CreateCell(officialDataCount);
                        cellHeader7.SetCellValue("Giá bán(Chính thức)");
                        cellHeader7.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader7 = new CellRangeAddress(rowHeader, rowHeader, officialDataCount, officialDataCount + 1);
                        sheet.AddMergedRegion(mergedRegioncellHeader7);
                        RegionUtil.SetBorderBottom(1, mergedRegioncellHeader7, sheet);
                        RegionUtil.SetBorderLeft(1, mergedRegioncellHeader7, sheet);
                        RegionUtil.SetBorderRight(1, mergedRegioncellHeader7, sheet);
                        RegionUtil.SetBorderTop(1, mergedRegioncellHeader7, sheet);

                        ICell cellHeader8 = rowHeaders.CreateCell(officialDataCount + 2);
                        cellHeader8.SetCellValue("QĐ bố trí tạm(Chính thức)");
                        cellHeader8.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader8 = new CellRangeAddress(rowHeader, rowHeader, officialDataCount + 2, officialDataCount + 3);
                        sheet.AddMergedRegion(mergedRegioncellHeader8);
                        RegionUtil.SetBorderBottom(1, mergedRegioncellHeader8, sheet);
                        RegionUtil.SetBorderLeft(1, mergedRegioncellHeader8, sheet);
                        RegionUtil.SetBorderRight(1, mergedRegioncellHeader8, sheet);
                        RegionUtil.SetBorderTop(1, mergedRegioncellHeader8, sheet);

                        int excelCount = officialDataCount + 4;
                        foreach (var e in item.payDetails)
                        {
                            ICell cellHeader = rowHeaders.CreateCell(excelCount);
                            cellHeader.SetCellValue("Kỳ");
                            cellHeader.CellStyle = cellStyle;
                            CellRangeAddress mergedRegioncellHeader = new CellRangeAddress(rowHeader, rowHeader1, excelCount, excelCount);
                            sheet.AddMergedRegion(mergedRegioncellHeader);
                            RegionUtil.SetBorderBottom(1, mergedRegioncellHeader, sheet);
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeader, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeader, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeader, sheet);
                            excelCount++;

                            ICell cellHeader9 = rowHeaders.CreateCell(excelCount);
                            cellHeader9.SetCellValue("Ngày phải nộp tiền");
                            cellHeader9.CellStyle = cellStyle;
                            CellRangeAddress mergedRegioncellHeader9 = new CellRangeAddress(rowHeader, rowHeader1, excelCount, excelCount);
                            sheet.AddMergedRegion(mergedRegioncellHeader9);
                            RegionUtil.SetBorderBottom(1, mergedRegioncellHeader9, sheet);
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeader9, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeader9, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeader9, sheet);
                            excelCount++;

                            ICell cellHeader10 = rowHeaders.CreateCell(excelCount);
                            cellHeader10.SetCellValue("Ngày nộp tiền");
                            cellHeader10.CellStyle = cellStyle;
                            CellRangeAddress mergedRegioncellHeader10 = new CellRangeAddress(rowHeader, rowHeader1, excelCount, excelCount);
                            sheet.AddMergedRegion(mergedRegioncellHeader10);
                            RegionUtil.SetBorderBottom(1, mergedRegioncellHeader10, sheet);
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeader10, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeader10, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeader10, sheet);
                            excelCount++;

                            ICell cellHeader11 = rowHeaders.CreateCell(excelCount);
                            cellHeader11.SetCellValue("Số tiền phải nộp");
                            cellHeader11.CellStyle = cellStyle;
                            CellRangeAddress mergedRegioncellHeader11 = new CellRangeAddress(rowHeader, rowHeader1, excelCount, excelCount);
                            sheet.AddMergedRegion(mergedRegioncellHeader11);
                            RegionUtil.SetBorderBottom(1, mergedRegioncellHeader11, sheet);
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeader11, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeader11, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeader11, sheet);
                            excelCount++;

                            ICell cellHeader12 = rowHeaders.CreateCell(excelCount);
                            cellHeader12.SetCellValue("tiền lãi");
                            cellHeader12.CellStyle = cellStyle;
                            CellRangeAddress mergedRegioncellHeader12 = new CellRangeAddress(rowHeader, rowHeader1, excelCount, excelCount);
                            sheet.AddMergedRegion(mergedRegioncellHeader12);
                            RegionUtil.SetBorderBottom(1, mergedRegioncellHeader12, sheet);
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeader12, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeader12, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeader12, sheet);
                            excelCount++;

                            ICell cellHeader13 = rowHeaders.CreateCell(excelCount);
                            cellHeader13.SetCellValue("tiền phạt");
                            cellHeader13.CellStyle = cellStyle;
                            CellRangeAddress mergedRegioncellHeader13 = new CellRangeAddress(rowHeader, rowHeader1, excelCount, excelCount);
                            sheet.AddMergedRegion(mergedRegioncellHeader13);
                            RegionUtil.SetBorderBottom(1, mergedRegioncellHeader13, sheet);
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeader13, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeader13, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeader13, sheet);
                            excelCount++;

                            ICell cellHeader14 = rowHeaders.CreateCell(excelCount);
                            cellHeader14.SetCellValue("tiền gốc");
                            cellHeader14.CellStyle = cellStyle;
                            CellRangeAddress mergedRegioncellHeader14 = new CellRangeAddress(rowHeader, rowHeader1, excelCount, excelCount);
                            sheet.AddMergedRegion(mergedRegioncellHeader14);
                            RegionUtil.SetBorderBottom(1, mergedRegioncellHeader14, sheet);
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeader14, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeader14, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeader14, sheet);
                            excelCount++;

                            ICell cellHeaderVAT = rowHeaders.CreateCell(excelCount);
                            cellHeaderVAT.SetCellValue("VAT");
                            cellHeaderVAT.CellStyle = cellStyle;
                            CellRangeAddress mergedRegioncellHeaderVAT = new CellRangeAddress(rowHeader, rowHeader1, excelCount, excelCount);
                            sheet.AddMergedRegion(mergedRegioncellHeaderVAT);
                            RegionUtil.SetBorderBottom(1, mergedRegioncellHeaderVAT, sheet);
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeaderVAT, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeaderVAT, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeaderVAT, sheet);
                            excelCount++;

                            int excelChildCount = excelCount;
                            foreach (var r in item.reportPats)
                            {
                                if (r.PATName != "VAT")
                                {
                                    ICell cellHeader15 = rowHeaders.CreateCell(excelChildCount);
                                    cellHeader15.SetCellValue(r.PATName);
                                    cellHeader15.CellStyle = cellStyle;
                                    CellRangeAddress mergedRegioncellHeader15 = new CellRangeAddress(rowHeader, rowHeader1, excelChildCount, excelChildCount);
                                    sheet.AddMergedRegion(mergedRegioncellHeader15);
                                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader15, sheet);
                                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader15, sheet);
                                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader15, sheet);
                                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader15, sheet);
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
                            RegionUtil.SetBorderLeft(1, mergedRegioncellHeader16, sheet);
                            RegionUtil.SetBorderRight(1, mergedRegioncellHeader16, sheet);
                            RegionUtil.SetBorderTop(1, mergedRegioncellHeader16, sheet);
                            excelCount++;
                        }

                        int taxCountStart = excelCount;
                        int taxCountEnd = 0;
                        foreach (var tax1 in item.taxes)
                        {
                            taxCountEnd = (item.taxes != null ? item.taxes.Count : 0) * 2;
                        }
                        taxCountEnd = taxCountEnd + taxCountStart;


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
                        foreach (var z in item.reportIngreTemporarys)
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

                        foreach (var x in item.reportIngreTemporarys)
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
                                cell.SetCellValue(item.ContractNumber);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == 1)
                            {
                                cell.SetCellValue((DateTime)item.DateNumber);
                                cell.CellStyle = cellStyleDate;
                            }
                            else if (i == 2)
                            {
                                cell.SetCellValue(item.CustomerName);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == 3)
                            {
                                cell.SetCellValue(item.TdcApartmentName);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == 4)
                            {
                                if (item.Corner == null)
                                {
                                    cell.SetCellValue("");
                                    cell.CellStyle = cellStyle;
                                }
                                else
                                {
                                    cell.SetCellValue((bool)item.Corner);
                                    cell.CellStyle = cellStyle;
                                }
                            }
                            else if (i == 5)
                            {
                                cell.SetCellValue(item.TdcBlockHouseName);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == 6)
                            {
                                cell.SetCellValue(item.TdcLandName);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == 7)
                            {
                                cell.SetCellValue(item.Floor1);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == 8)
                            {
                                cell.SetCellValue(item.TdcFloorName);
                                cell.CellStyle = cellStyle;
                                temporaryDatas++;
                            }
                            else if (i == temporaryDatas)
                            {
                                foreach (var childItem in item.reportIngreTemporarys)
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
                                if (item.TemporaryTotalArea.HasValue)
                                {
                                    cell.SetCellValue((double)item.TemporaryTotalArea.Value);
                                }
                                else
                                {
                                    cell.SetCellValue(string.Empty);
                                }
                                cell.CellStyle = cellStyle;

                            }
                            else if (i == temporaryDatasEnd + 1)
                            {
                                if (item.TemporaryTotalPrice.HasValue)
                                {
                                    cell.SetCellValue((double)item.TemporaryTotalPrice.Value);
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
                                cell.SetCellValue(item.TemporaryDecreeNumber);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == priceAndTaxTTs + 2)
                            {
                                if (item.TemporaryDecreeDate.HasValue)
                                {
                                    cell.SetCellValue(item.TemporaryDecreeDate.Value);
                                }
                                else
                                {
                                    cell.SetCellValue(string.Empty);
                                }
                                cell.CellStyle = cellStyleDate;
                            }
                            else if (i == priceAndTaxTTs + 3)
                            {
                                cell.SetCellValue((DateTime)item.FirstPayDate);
                                cell.CellStyle = cellStyleDate;

                                foreach (var childItem2 in item.reportIngres)
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
                                cell.SetCellValue((double)item.TotalArea);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == officialDatas + 2)
                            {
                                cell.SetCellValue((double)item.TotalPrice);
                                cell.CellStyle = cellStyleMoney;
                                priceAndTaxes = i;

                            }
                            else if (i == priceAndTaxes + 1)
                            {
                                cell.SetCellValue(item.DecreeNumber);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == priceAndTaxes + 2)
                            {
                                cell.SetCellValue((DateTime)item.DecreeDate);
                                cell.CellStyle = cellStyleDate;
                            }
                            else if (i == priceAndTaxes + 3)
                            {
                                foreach (var itemExcel in item.payDetails)
                                {
                                    ICell cellIndex = row.CreateCell(i);
                                    cellIndex.SetCellValue(itemExcel.index);
                                    cellIndex.CellStyle = cellStyle;
                                    i++;

                                    if (itemExcel.PayDateDefault == null)
                                    {
                                        ICell cellPaymentDate = row.CreateCell(i);
                                        cellPaymentDate.SetCellValue("");
                                        cellPaymentDate.CellStyle = cellStyle;
                                        i++;
                                    }
                                    else
                                    {
                                        ICell cellPaymentDate = row.CreateCell(i);
                                        cellPaymentDate.SetCellValue((DateTime)itemExcel.PayDateDefault);
                                        cellPaymentDate.CellStyle = cellStyleDate;
                                        i++;
                                    }

                                    if (itemExcel.PayDateReal == null)
                                    {
                                        ICell cellPayDate = row.CreateCell(i);
                                        cellPayDate.SetCellValue("");
                                        cellPayDate.CellStyle = cellStyle;
                                        i++;
                                    }
                                    else
                                    {
                                        ICell cellPayDate = row.CreateCell(i);
                                        cellPayDate.SetCellValue((DateTime)itemExcel.PayDateReal);
                                        cellPayDate.CellStyle = cellStyleDate;
                                        i++;
                                    }
                                    ICell cellAmountPayable = row.CreateCell(i);
                                    cellAmountPayable.SetCellValue((double)itemExcel.PayAnnual);
                                    cellAmountPayable.CellStyle = cellStyleMoney;
                                    i++;

                                    ICell cellAmountInterest = row.CreateCell(i);
                                    cellAmountInterest.SetCellValue((double)itemExcel.TotalInterest);
                                    cellAmountInterest.CellStyle = cellStyleMoney;
                                    i++;

                                    ICell cellPricePunish = row.CreateCell(i);
                                    cellPricePunish.SetCellValue((double)itemExcel.Fines);
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

                                    foreach (var childItemExcel in itemExcel.payDetailChilds)
                                    {
                                        if (childItemExcel.Name != "VAT")
                                        {
                                            ICell cellTotalValue = row.CreateCell(i);
                                            cellTotalValue.SetCellValue((double)childItemExcel.TotalValue);
                                            cellTotalValue.CellStyle = cellStyleMoney;
                                            i++;
                                        }
                                    }
                                    if (itemExcel.OverPay == null) itemExcel.OverPay = 0;
                                    ICell cellOverpayment = row.CreateCell(i);
                                    cellOverpayment.SetCellValue((double)itemExcel.OverPay);
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

        //Mẫu báo cáo số 3
        [HttpPost("GetReport/{Id}")]
        public async Task<IActionResult> GetReport(int id, [FromBody] List<TdcInstallmentPriceGroupByPayTimeId> input)
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
                TDCInstallmentPrice tDCInstallmentPrice = _context.TDCInstallmentPrices.Where(l => l.Id == id && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                var tempTotalPriceMainValue = tDCInstallmentPrice.TemporaryTotalPrice.GetValueOrDefault();
                if (tDCInstallmentPrice == null)
                {
                    def.meta = new Meta(404, "Không tìm thấy hồ sơ tương ứng!");
                    return Ok(def);
                }

                //Khai báo biến 
                List<Report> res = new List<Report>();

                decimal PrincipalReceived = 0; //Số tiền gốc đã thu
                decimal InterestRecevied = 0; //Số tiền lãi đã thu

                decimal sumValuePrincipa = 0; //Sum value gốc
                decimal sumValueInterest = 0; //Sum value lãi

                decimal TotalCt = 0; //tổng giá bán cấu thành chính thức
                decimal TotalTt = 0;//tổng giá bán cấu thành tạm thời

                decimal InterestReceviedTotal = 0; //Tổng số tiền gốc đã thu
                decimal PrincipalReceivedToal = 0; //Tổng số tiền lãi đã thu

                decimal PrincipalBfTaxTotal = 0;//Tổng số tiền gốc trước thuế
                decimal InterestBfTaxTotal = 0;//Tổng số tiền lãi trước thuế
                decimal PrincipalTotal = 0;//Tổng số tiền giá gốc căn hộ nộp ngân sách

                decimal VATPrincipalTotal = 0;  //tổng VAT gốc đã thu
                decimal VATInterestTotal = 0;  //Tổng VAT lãi đã thu
                decimal VAT = 0;   //Tổng VAT các dòng

                //Tính Count để lấy dòng cuối sau khi import
                decimal? PayAnnual = 0;// số tiền gốc của kì tổng cuối cùng file import
                decimal? TotalInterest = 0; //Lãi phát sinh tính theo tháng và ngày cuối cùng của số tiền đóng dư cuối import
                decimal? PayAnnualPre = 0; // số tiền gốc trước của kì tổng cuối cùng file import

                decimal ValuePrincipal = 0;//Sum từng cột value ở tiền gốc đã thu
                decimal ValueInterest = 0;//Sum từng cột value ở tiền lãi đã thu
                decimal ValueTotal = 0;//Sum từng cột value ở tổng tiền đã thu

                decimal PrincipalReceivedPublic = 0; //Số tiền gốc phải trả hàng năm công ích
                decimal InterestReceviedPublic = 0;//Lãi phát sinh tính theo tháng và ngày công ích

                List<TDCProjectIngrePrice> tDCProjectIngrePrices = _context.TDCProjectIngrePrices.Where(x => x.TDCProjectId == tDCInstallmentPrice.TdcProjectId && x.Status != AppEnums.EntityStatus.DELETED).ToList();

                //lấy value TP giá bán cấu thành tạm thời
                List<TDCInstallmentTemporaryDetail> tDCInstallmentTemporaryDetails = _context.TDCInstallmentTemporaryDetails.Where(l => l.TDCInstallmentPriceId == tDCInstallmentPrice.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                foreach (TDCInstallmentTemporaryDetail temporaryDetail in tDCInstallmentTemporaryDetails)
                {
                    double value = tDCProjectIngrePrices.Where(x => x.IngredientsPriceId == temporaryDetail.IngredientsPriceId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Value).FirstOrDefault();

                    TotalTt += (temporaryDetail.Price / (decimal)value);
                }

                //lấy value TP giá bán cấu thành chính thức
                List<TDCInstallmentOfficialDetail> tDCInstallmentOfficialDetails = _context.TDCInstallmentOfficialDetails.Where(l => l.TDCInstallmentPriceId == tDCInstallmentPrice.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                foreach (TDCInstallmentOfficialDetail officialDetail in tDCInstallmentOfficialDetails)
                {
                    double value = tDCProjectIngrePrices.Where(x => x.IngredientsPriceId == officialDetail.IngredientsPriceId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Value).FirstOrDefault();

                    TotalCt += (officialDetail.Price / (decimal)value);
                }
                for (int i = 0; i < input.Count; i++)
                {
                    int Count = -1;
                    for (int j = 0; j < input[i].tdcInstallmentPriceTables.Count; j++)
                    {
                        if (input[i].tdcInstallmentPriceTables[j].IsImport == true)
                        {
                            Count = j;
                        }
                        else
                        {
                            Count = -10;
                        }
                    }
                    if (i >= 3 && Count == 0)
                    {
                        PayAnnual = input[i - 2].tdcInstallmentPriceTables[Count].PayAnnual;
                        TotalInterest = input[i - 2].tdcInstallmentPriceTables[Count].TotalInterest;
                        PayAnnualPre = input[i - 3].tdcInstallmentPriceTables[Count].PayAnnual;
                    }
                    if (Count == -10) break;
                }

                var dataPublic = new Report();
                foreach (var z in input)
                {
                    //if (z.publicPay == true)
                    //{
                    foreach (var itemz in z.tdcInstallmentPriceTables)
                    {
                        if (itemz.IsImport == true || itemz.publicPay == true)
                        {
                            if (itemz.publicPay == true)
                            {
                                PrincipalReceivedPublic += (decimal)itemz.PayAnnual;
                                InterestReceviedPublic += (decimal)itemz.TotalInterest;
                            }

                            if (z.tdcInstallmentPriceTables.First().PaymentTimes.HasValue)
                            {
                                dataPublic.index = (int)(z.tdcInstallmentPriceTables.First().PaymentTimes);
                            }
                            dataPublic.PayDateReal = z.tdcInstallmentPriceTables.Last().PayDateReal;
                            dataPublic.ContractNumber = tDCInstallmentPrice.ContractNumber;
                            dataPublic.ContractDate = tDCInstallmentPrice.DateNumber;
                            dataPublic.Name = _context.TdcCustomers.Where(x => x.Id == tDCInstallmentPrice.TdcCustomerId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.FullName).FirstOrDefault();
                            dataPublic.TdcApartmentName = _context.ApartmentTdcs.Where(x => x.Id == tDCInstallmentPrice.TdcApartmentId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault();
                            dataPublic.TdcBlockHouseName = _context.BlockHouses.Where(x => x.Id == tDCInstallmentPrice.BlockHouseId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault();
                            dataPublic.TdcLandName = _context.Lands.Where(x => x.Id == tDCInstallmentPrice.LandId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault();

                            dataPublic.PrincipalReceived = (decimal)(PayAnnual + PrincipalReceivedPublic - PayAnnualPre);
                            dataPublic.InterestRecevied = (decimal)(InterestReceviedPublic + TotalInterest);
                            dataPublic.CheckPublicPay = true;
                        }

                    }
                    //}
                }
                res.Add(dataPublic);

                var dataPublicTotal = new Report();
                dataPublicTotal.Name = "Tổng";
                dataPublicTotal.PrincipalReceived = dataPublic.PrincipalReceived;
                dataPublicTotal.InterestRecevied = dataPublic.InterestRecevied;
                dataPublicTotal.CheckPublicPay = true;
                dataPublicTotal.CheckTotal = 99;
                res.Add(dataPublicTotal);

                var DataPublicTotal = new Report();
                DataPublicTotal.Name = "Tổng";
                DataPublicTotal.PrincipalReceived = dataPublicTotal.PrincipalReceived + dataPublicTotal.InterestRecevied;
                DataPublicTotal.CheckPublicPay = true;
                DataPublicTotal.CheckTotal = 1;
                res.Add(DataPublicTotal);

                var dataTotal = new Report();//Tổng Gốc

                TDCInstallmentPriceData mapper_data = _mapper.Map<TDCInstallmentPriceData>(tDCInstallmentPrice);
                List<TDCProjectPriceAndTax> tDCProjectPriceAndTaxs = _context.TDCProjectPriceAndTaxs.Where(l => l.TDCProjectId == tDCInstallmentPrice.TdcProjectId && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<ReportIngre> ingreDataDetails = new List<ReportIngre>();
                List<TDCProjectPriceAndTaxData> map_TDCProjectPriceAndTaxs = _mapper.Map<List<TDCProjectPriceAndTaxData>>(tDCProjectPriceAndTaxs);

                var DataTotal = new Report();//Tổng
                var Total = new Report(); //Tổng Thu

                for (int i = 0; i < input.Count; i++)
                {
                    if (i == 0 && input[i].tdcInstallmentPriceTables[0].IsImport == false) /// In kì 1 ko có import
                    {
                        Report data = new Report();

                        data.Pat = new List<ReportPat>();
                        data.valuePrincipal = new List<ValuePat>();
                        data.valueBeforeTax = new List<ValuePat>();
                        data.values = new List<ValuePat>();
                        dataTotal.valueTotal = new List<ValueTotal>();
                        DataTotal.valueTotal = new List<ValueTotal>();
                        Total.valueTotal = new List<ValueTotal>();

                        foreach (TDCProjectPriceAndTaxData map_TDCProjectPriceAndTax in map_TDCProjectPriceAndTaxs)
                        {
                            List<TDCProjectPriceAndTaxDetails> de = _context.TDCProjectPriceAndTaxDetailss.Where(f => f.PriceAndTaxId == map_TDCProjectPriceAndTax.Id && f.Status != AppEnums.EntityStatus.DELETED).ToList();
                            List<TDCProjectPriceAndTaxDetailData> detail = _mapper.Map<List<TDCProjectPriceAndTaxDetailData>>(de);
                            OriginalPriceAndTax pat = _context.OriginalPriceAndTaxs.Where(f => f.Id == map_TDCProjectPriceAndTax.PriceAndTaxId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            if (de == null) map_TDCProjectPriceAndTax.PATName = "";
                            else map_TDCProjectPriceAndTax.PATName = pat.Name;

                            var PAT = new ReportPat();
                            PAT.PATName = pat.Name;
                            PAT.Value = map_TDCProjectPriceAndTax.Value;
                            PAT.Location = map_TDCProjectPriceAndTax.Location;

                            data.Pat.Add(PAT);
                        }
                        foreach (var childItem in input[i].tdcInstallmentPriceTables)
                        {
                            data.index = (int)(input[i].tdcInstallmentPriceTables.First().PaymentTimes);
                            data.PayDateReal = input[i].tdcInstallmentPriceTables.Last().PayDateReal;
                            data.ContractNumber = tDCInstallmentPrice.ContractNumber;
                            data.ContractDate = tDCInstallmentPrice.DateNumber;
                            data.Name = _context.TdcCustomers.Where(x => x.Id == tDCInstallmentPrice.TdcCustomerId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.FullName).FirstOrDefault();
                            data.TdcApartmentName = _context.ApartmentTdcs.Where(x => x.Id == tDCInstallmentPrice.TdcApartmentId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault();
                            data.TdcBlockHouseName = _context.BlockHouses.Where(x => x.Id == tDCInstallmentPrice.BlockHouseId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault();
                            data.TdcLandName = _context.Lands.Where(x => x.Id == tDCInstallmentPrice.LandId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault();
                            data.PrincipalReceived = (decimal)childItem.PayAnnual; ;
                            if (childItem.TotalInterest == null) childItem.TotalInterest = 0;
                            data.InterestRecevied = (decimal)childItem.TotalInterest;

                            if (childItem.PayDateDefault < tDCInstallmentPrice.DecreeDate)
                            {
                                decimal n = tempTotalPriceMainValue == 0 ? 0 : Math.Round((TotalTt / tempTotalPriceMainValue) * 100);
                                data.PrincipalBfTax = Math.Round(data.PrincipalReceived * (n / 100));
                                data.InterestBfTax = Math.Round(data.InterestRecevied * (n / 100));
                            }
                            else
                            {
                                decimal n = Math.Round((TotalCt / tDCInstallmentPrice.TotalPrice) * 100);
                                data.PrincipalBfTax = Math.Round(data.PrincipalReceived * (n / 100));
                                data.InterestBfTax = Math.Round(data.InterestRecevied * (n / 100));
                            }
                            data.Principal = data.PrincipalBfTax + data.InterestBfTax;
                        }
                        foreach (var ItemPat in data.Pat)
                        {
                            var itemValuePrincipal = new ValuePat();
                            itemValuePrincipal.Name = ItemPat.PATName;
                            if (ItemPat.PATName != "VAT")
                            {
                                itemValuePrincipal.Value = Math.Round(data.PrincipalReceived * ((decimal)ItemPat.Value / 100));
                            }
                            else
                            {
                                itemValuePrincipal.Value = 0;
                            }
                            data.valuePrincipal.Add(itemValuePrincipal);
                            sumValuePrincipa = data.valuePrincipal.Sum(x => x.Value);

                            var itemValueInterest = new ValuePat();
                            itemValueInterest.Name = ItemPat.PATName;
                            if (ItemPat.PATName != "VAT")
                            {
                                itemValueInterest.Value = Math.Round(data.InterestRecevied * ((decimal)ItemPat.Value / 100));
                            }
                            else
                            {
                                itemValuePrincipal.Value = 0;
                            }
                            data.valueBeforeTax.Add(itemValueInterest);
                            sumValueInterest = data.valueBeforeTax.Sum(x => x.Value);

                            var itemValue = new ValuePat();
                            itemValue.Name = ItemPat.PATName;
                            itemValue.Value = itemValuePrincipal.Value + itemValueInterest.Value;
                            data.values.Add(itemValue);

                            var itemTotal = new ValueTotal();
                            itemTotal.Name = ItemPat.PATName;
                            itemTotal.ValuePrincipal += itemValuePrincipal.Value;//Sum từng cột value ở tiền gốc đã tu
                            itemTotal.ValueInterest += itemValueInterest.Value;//Sum từng cột value ở tiền lãi đã tu
                            itemTotal.Value += itemValue.Value;//Sum từng cột value ở tổng tiền đã tu

                            dataTotal.valueTotal.Add(itemTotal);
                            DataTotal.valueTotal.Add(itemTotal);
                            Total.valueTotal.Add(itemTotal);
                        }
                        data.VATPrincipal = data.PrincipalReceived - (data.PrincipalBfTax + sumValuePrincipa);
                        data.VATInterest = data.InterestRecevied - (data.InterestBfTax + sumValueInterest);
                        data.VATTotal = data.VATPrincipal + data.VATInterest;

                        PrincipalBfTaxTotal += data.PrincipalBfTax;//Tính tổng gốc trước thuế
                        InterestBfTaxTotal += data.InterestBfTax;//Tính tổng lãi trước thuế
                        PrincipalTotal += data.Principal;//Tính  tổng giá  gốc căn hộ nộp ngân sách

                        VATPrincipalTotal += data.VATPrincipal;//Tính tổng VAT gốc
                        VATInterestTotal += data.VATInterest;//Tính tổng VAT lãi
                        VAT += data.VATTotal;//Tính tổng VAT tiền đã thu

                        res.Add(data);
                    }

                    if (i > 0)
                    {
                        if (input[i].publicPay == false && input[i - 1].tdcInstallmentPriceTables[0].IsImport == true && input[i + 1].tdcInstallmentPriceTables[0].IsImport != true && input[i].Pay != null)
                        {
                            if (input[i + 1].tdcInstallmentPriceTables[0].PayAnnual == null) input[i + 1].tdcInstallmentPriceTables[0].PayAnnual = 0;
                            if (input[i + 1].tdcInstallmentPriceTables[0].TotalInterest == null) input[i + 1].tdcInstallmentPriceTables[0].TotalInterest = 0;

                            PrincipalReceived = (decimal)input[i + 1].tdcInstallmentPriceTables[0].PayAnnual;
                            InterestRecevied = (decimal)input[i + 1].tdcInstallmentPriceTables[0].TotalInterest;
                            PrincipalReceivedToal += PrincipalReceived;
                            InterestReceviedTotal += InterestRecevied;
                        }
                        if (input[i].publicPay == false && input[i - 1].tdcInstallmentPriceTables[0].IsImport == false && input[i].Pay != null)//Check không phải import không phải công ích và không phải kì tổng
                        {
                            if (i > 3) /// fix dòng nợ cũ
                            {
                                if (input[i - 2].tdcInstallmentPriceTables[0].RowStatus == AppEnums.TypePayQD.NO_CU)
                                {

                                    if (input[i - 3].tdcInstallmentPriceTables[0].PayAnnual == null) input[i - 3].tdcInstallmentPriceTables[0].PayAnnual = 0;
                                    if (input[i - 3].tdcInstallmentPriceTables[0].TotalInterest == null) input[i - 3].tdcInstallmentPriceTables[0].TotalInterest = 0;

                                    PrincipalReceived = (decimal)input[i - 3].tdcInstallmentPriceTables[0].PayAnnual;
                                    InterestRecevied = (decimal)input[i - 3].tdcInstallmentPriceTables[0].TotalInterest;
                                }
                                else
                                {
                                    if (input[i - 1].tdcInstallmentPriceTables[0].PayAnnual == null) input[i - 1].tdcInstallmentPriceTables[0].PayAnnual = 0;
                                    if (input[i - 1].tdcInstallmentPriceTables[0].TotalInterest == null) input[i - 1].tdcInstallmentPriceTables[0].TotalInterest = 0;

                                    PrincipalReceived = (decimal)input[i - 1].tdcInstallmentPriceTables[0].PayAnnual;
                                    InterestRecevied = (decimal)input[i - 1].tdcInstallmentPriceTables[0].TotalInterest;
                                }
                            }
                            else
                            {
                                if (input[i - 1].tdcInstallmentPriceTables[0].PayAnnual == null) input[i - 1].tdcInstallmentPriceTables[0].PayAnnual = 0;
                                if (input[i - 1].tdcInstallmentPriceTables[0].TotalInterest == null) input[i - 1].tdcInstallmentPriceTables[0].TotalInterest = 0;

                                PrincipalReceived = (decimal)input[i - 1].tdcInstallmentPriceTables[0].PayAnnual;
                                InterestRecevied = (decimal)input[i - 1].tdcInstallmentPriceTables[0].TotalInterest;
                            }
                            PrincipalReceivedToal += PrincipalReceived;
                            InterestReceviedTotal += InterestRecevied;
                        }
                    }

                    if (input[i].Pay != 0 && input[i].publicPay == false && input[i].Pay != null)
                    {
                        Report data = new Report();

                        data.Pat = new List<ReportPat>();
                        data.valuePrincipal = new List<ValuePat>();
                        data.valueBeforeTax = new List<ValuePat>();
                        data.values = new List<ValuePat>();
                        dataTotal.valueTotal = new List<ValueTotal>();
                        DataTotal.valueTotal = new List<ValueTotal>();
                        Total.valueTotal = new List<ValueTotal>();

                        foreach (TDCProjectPriceAndTaxData map_TDCProjectPriceAndTax in map_TDCProjectPriceAndTaxs)
                        {
                            List<TDCProjectPriceAndTaxDetails> de = _context.TDCProjectPriceAndTaxDetailss.Where(f => f.PriceAndTaxId == map_TDCProjectPriceAndTax.Id && f.Status != AppEnums.EntityStatus.DELETED).ToList();
                            List<TDCProjectPriceAndTaxDetailData> detail = _mapper.Map<List<TDCProjectPriceAndTaxDetailData>>(de);
                            OriginalPriceAndTax pat = _context.OriginalPriceAndTaxs.Where(f => f.Id == map_TDCProjectPriceAndTax.PriceAndTaxId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            if (de == null) map_TDCProjectPriceAndTax.PATName = "";
                            else map_TDCProjectPriceAndTax.PATName = pat.Name;

                            var PAT = new ReportPat();
                            PAT.PATName = pat.Name;
                            PAT.Value = map_TDCProjectPriceAndTax.Value;
                            PAT.Location = map_TDCProjectPriceAndTax.Location;

                            data.Pat.Add(PAT);
                        }
                        foreach (var childItem in input[i].tdcInstallmentPriceTables)
                        {
                            data.index = (int)(input[i].tdcInstallmentPriceTables.First().PaymentTimes);
                            data.PayDateReal = input[i].tdcInstallmentPriceTables.Last().PayDateReal;
                            data.ContractNumber = tDCInstallmentPrice.ContractNumber;
                            data.ContractDate = tDCInstallmentPrice.DateNumber;
                            data.Name = _context.TdcCustomers.Where(x => x.Id == tDCInstallmentPrice.TdcCustomerId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.FullName).FirstOrDefault();
                            data.TdcApartmentName = _context.ApartmentTdcs.Where(x => x.Id == tDCInstallmentPrice.TdcApartmentId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault();
                            data.TdcBlockHouseName = _context.BlockHouses.Where(x => x.Id == tDCInstallmentPrice.BlockHouseId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault();
                            data.TdcLandName = _context.Lands.Where(x => x.Id == tDCInstallmentPrice.LandId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault();
                            data.PrincipalReceived = PrincipalReceived;
                            data.InterestRecevied = InterestRecevied;

                            if (childItem.PayDateDefault < tDCInstallmentPrice.DecreeDate)
                            {
                                decimal n = tempTotalPriceMainValue == 0 ? 0 : Math.Round((TotalTt / tempTotalPriceMainValue) * 100);
                                data.PrincipalBfTax = Math.Round(data.PrincipalReceived * (n / 100));
                                data.InterestBfTax = Math.Round(data.InterestRecevied * (n / 100));
                            }
                            else
                            {
                                decimal n = Math.Round((TotalCt / tDCInstallmentPrice.TotalPrice) * 100);
                                data.PrincipalBfTax = Math.Round(data.PrincipalReceived * (n / 100));
                                data.InterestBfTax = Math.Round(data.InterestRecevied * (n / 100));
                            }
                            data.Principal = data.PrincipalBfTax + data.InterestBfTax;
                        }
                        foreach (var ItemPat in data.Pat)
                        {
                            var itemValuePrincipal = new ValuePat();
                            itemValuePrincipal.Name = ItemPat.PATName;
                            if (ItemPat.PATName != "VAT")
                            {
                                itemValuePrincipal.Value = Math.Round(data.PrincipalReceived * ((decimal)ItemPat.Value / 100));
                            }
                            else
                            {
                                itemValuePrincipal.Value = 0;
                            }
                            data.valuePrincipal.Add(itemValuePrincipal);
                            sumValuePrincipa = data.valuePrincipal.Sum(x => x.Value);

                            var itemValueInterest = new ValuePat();
                            itemValueInterest.Name = ItemPat.PATName;
                            if (ItemPat.PATName != "VAT")
                            {
                                itemValueInterest.Value = Math.Round(data.InterestRecevied * ((decimal)ItemPat.Value / 100));
                            }
                            else
                            {
                                itemValuePrincipal.Value = 0;
                            }
                            data.valueBeforeTax.Add(itemValueInterest);
                            sumValueInterest = data.valueBeforeTax.Sum(x => x.Value);

                            var itemValue = new ValuePat();
                            itemValue.Name = ItemPat.PATName;
                            itemValue.Value = itemValuePrincipal.Value + itemValueInterest.Value;
                            data.values.Add(itemValue);

                            var itemTotal = new ValueTotal();
                            itemTotal.Name = ItemPat.PATName;
                            itemTotal.ValuePrincipal += itemValuePrincipal.Value;//Sum từng cột value ở tiền gốc đã tu
                            itemTotal.ValueInterest += itemValueInterest.Value;//Sum từng cột value ở tiền lãi đã tu
                            itemTotal.Value += itemValue.Value;//Sum từng cột value ở tổng tiền đã tu

                            dataTotal.valueTotal.Add(itemTotal);
                            DataTotal.valueTotal.Add(itemTotal);
                            Total.valueTotal.Add(itemTotal);
                        }
                        data.VATPrincipal = data.PrincipalReceived - (data.PrincipalBfTax + sumValuePrincipa);
                        data.VATInterest = data.InterestRecevied - (data.InterestBfTax + sumValueInterest);
                        data.VATTotal = data.VATPrincipal + data.VATInterest;

                        PrincipalBfTaxTotal += data.PrincipalBfTax;//Tính tổng gốc trước thuế
                        InterestBfTaxTotal += data.InterestBfTax;//Tính tổng lãi trước thuế
                        PrincipalTotal += data.Principal;//Tính  tổng giá  gốc căn hộ nộp ngân sách

                        VATPrincipalTotal += data.VATPrincipal;//Tính tổng VAT gốc
                        VATInterestTotal += data.VATInterest;//Tính tổng VAT lãi
                        VAT += data.VATTotal;//Tính tổng VAT tiền đã thu

                        res.Add(data);
                    }
                }
                dataTotal.Name = "Tổng";
                dataTotal.PrincipalReceived = PrincipalReceivedToal;
                dataTotal.InterestRecevied = InterestReceviedTotal;
                dataTotal.VATPrincipal = VATPrincipalTotal;
                dataTotal.VATInterest = VATInterestTotal;
                dataTotal.VATTotal = VAT;
                dataTotal.PrincipalBfTax = PrincipalBfTaxTotal;
                dataTotal.InterestBfTax = InterestBfTaxTotal;
                dataTotal.Principal = PrincipalTotal;
                dataTotal.CheckTotal = 99;
                res.Add(dataTotal);

                DataTotal.Name = "Tổng";
                DataTotal.PrincipalReceived = dataTotal.PrincipalReceived + dataTotal.InterestRecevied;
                DataTotal.VATPrincipal = VATPrincipalTotal;
                DataTotal.VATInterest = VATInterestTotal;
                DataTotal.VATTotal = VAT;
                DataTotal.CheckTotal = 1;
                DataTotal.PrincipalBfTax = PrincipalBfTaxTotal;
                DataTotal.InterestBfTax = InterestBfTaxTotal;
                DataTotal.Principal = PrincipalTotal;
                res.Add(DataTotal);


                Total.ContractNumber = "Tổng Cộng";
                Total.PrincipalReceived = DataTotal.PrincipalReceived + DataPublicTotal.PrincipalReceived;
                Total.VATPrincipal = VATPrincipalTotal;
                Total.VATInterest = VATInterestTotal;
                Total.VATTotal = VAT;
                Total.PrincipalBfTax = PrincipalBfTaxTotal;
                Total.InterestBfTax = InterestBfTaxTotal;
                Total.Principal = PrincipalTotal;
                Total.CheckTotal = 2;
                res.Add(Total);


                //Dòng đầu
                var fisData = new Report();
                fisData.Pat = new List<ReportPat>();
                fisData.valuePrincipal = new List<ValuePat>();
                fisData.valueBeforeTax = new List<ValuePat>();
                fisData.values = new List<ValuePat>();

                //Dòng cuối
                var lastData = new Report();
                lastData.Pat = new List<ReportPat>();
                lastData.valuePrincipal = new List<ValuePat>();
                lastData.valueBeforeTax = new List<ValuePat>();
                lastData.values = new List<ValuePat>();

                fisData.ContractNumber = tDCInstallmentPrice.ContractNumber;
                fisData.ContractDate = tDCInstallmentPrice.DateNumber;
                fisData.Name = _context.TdcCustomers.Where(x => x.Id == tDCInstallmentPrice.TdcCustomerId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.FullName).FirstOrDefault();
                fisData.TdcApartmentName = _context.ApartmentTdcs.Where(x => x.Id == tDCInstallmentPrice.TdcApartmentId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault();
                fisData.TdcBlockHouseName = _context.BlockHouses.Where(x => x.Id == tDCInstallmentPrice.BlockHouseId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault();
                fisData.TdcLandName = _context.Lands.Where(x => x.Id == tDCInstallmentPrice.LandId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault();
                fisData.PrincipalReceived = dataTotal.PrincipalReceived + dataPublicTotal.PrincipalReceived;
                fisData.InterestRecevied = dataTotal.InterestRecevied + dataPublicTotal.InterestRecevied;
                fisData.CheckPublicPay = true;

                foreach (TDCProjectPriceAndTaxData map_TDCProjectPriceAndTax in map_TDCProjectPriceAndTaxs)
                {
                    List<TDCProjectPriceAndTaxDetails> de = _context.TDCProjectPriceAndTaxDetailss.Where(f => f.PriceAndTaxId == map_TDCProjectPriceAndTax.Id && f.Status != AppEnums.EntityStatus.DELETED).ToList();
                    List<TDCProjectPriceAndTaxDetailData> detail = _mapper.Map<List<TDCProjectPriceAndTaxDetailData>>(de);
                    OriginalPriceAndTax pat = _context.OriginalPriceAndTaxs.Where(f => f.Id == map_TDCProjectPriceAndTax.PriceAndTaxId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    if (de == null) map_TDCProjectPriceAndTax.PATName = "";
                    else map_TDCProjectPriceAndTax.PATName = pat.Name;

                    var PAT = new ReportPat();
                    PAT.PATName = pat.Name;
                    PAT.Value = map_TDCProjectPriceAndTax.Value;
                    PAT.Location = map_TDCProjectPriceAndTax.Location;

                    fisData.Pat.Add(PAT);
                    lastData.Pat.Add(PAT);
                }
                foreach (var ItemPat in fisData.Pat)
                {
                    var itemValuePrincipal = new ValuePat();
                    itemValuePrincipal.Name = ItemPat.PATName;

                    if (ItemPat.PATName != "VAT")
                    {
                        itemValuePrincipal.Value = Math.Round(fisData.PrincipalReceived * ((decimal)ItemPat.Value / 100));
                    }
                    else
                    {
                        itemValuePrincipal.Value = 0;
                    }
                    fisData.valuePrincipal.Add(itemValuePrincipal);
                    sumValuePrincipa = fisData.valuePrincipal.Sum(x => x.Value);

                    var itemValueInterest = new ValuePat();
                    itemValueInterest.Name = ItemPat.PATName;
                    if (ItemPat.PATName != "VAT")
                    {
                        itemValueInterest.Value = Math.Round(fisData.InterestRecevied * ((decimal)ItemPat.Value / 100));
                    }
                    else
                    {
                        itemValuePrincipal.Value = 0;
                    }
                    fisData.valueBeforeTax.Add(itemValueInterest);
                    sumValueInterest = fisData.valueBeforeTax.Sum(x => x.Value);

                    var itemValue = new ValuePat();
                    itemValue.Name = ItemPat.PATName;
                    itemValue.Value = itemValuePrincipal.Value + itemValueInterest.Value;
                    fisData.values.Add(itemValue);
                }
                decimal t = Math.Round((TotalCt / tDCInstallmentPrice.TotalPrice) * 100);

                fisData.PrincipalBfTax = Math.Round(fisData.PrincipalReceived * (t / 100));
                fisData.InterestBfTax = Math.Round(fisData.InterestRecevied * (t / 100));
                fisData.Principal = fisData.PrincipalBfTax + fisData.InterestBfTax;
                fisData.OriginalPrice = tDCInstallmentPrice.TotalPrice;
                fisData.BeforeTax = TotalCt;

                fisData.VATPrincipal = fisData.PrincipalReceived - (fisData.PrincipalBfTax + sumValuePrincipa);
                fisData.VATInterest = fisData.InterestRecevied - (fisData.InterestBfTax + sumValueInterest);
                fisData.VATTotal = fisData.VATPrincipal + fisData.VATInterest;
                fisData.Note = "Dòng đầu tiên";
                res.Insert(0, fisData);

                //Dòng cuối cùng

                lastData.ContractNumber = "Số tiền còn lại dự kiến thu";
                lastData.PrincipalReceived = fisData.OriginalPrice - dataTotal.PrincipalReceived - dataPublicTotal.PrincipalReceived;
                lastData.CheckTotal = 3;

                foreach (var ItemPat in fisData.Pat)
                {
                    var itemValuePrincipal = new ValuePat();
                    if (ItemPat.PATName != "VAT")
                    {
                        itemValuePrincipal.Value = Math.Round(lastData.PrincipalReceived * ((decimal)ItemPat.Value / 100));
                    }
                    else
                    {
                        itemValuePrincipal.Value = 0;
                    }
                    lastData.valuePrincipal.Add(itemValuePrincipal);

                    var itemValueInterest = new ValuePat();
                    if (ItemPat.PATName != "VAT")
                    {
                        itemValueInterest.Value = Math.Round(lastData.InterestRecevied * ((decimal)ItemPat.Value / 100));
                    }
                    else
                    {
                        itemValuePrincipal.Value = 0;
                    }
                    lastData.valueBeforeTax.Add(itemValueInterest);

                    var itemValue = new ValuePat();
                    itemValue.Name = ItemPat.PATName;
                    itemValue.Value = itemValuePrincipal.Value + itemValueInterest.Value;
                    lastData.values.Add(itemValue);
                }

                res.Add(lastData);
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

        [HttpPost("Export")]
        public async Task<IActionResult> Export([FromBody] List<Report> input)
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
            //Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"templates/Mau3BanTraGop.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);
            string nameExcel = "BaoCao_.xls";

            MemoryStream ms = WriteDataToExcel(templatePath, 0, input);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", nameExcel);
        }

        private static MemoryStream WriteDataToExcel(string templatePath, int sheetnumber, List<Report> data)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowHeader = 8;
            int rowHeader1 = 10;
            int rowStart = 11;

            if (sheet != null)
            {
                try
                {
                    int datacol = 0;
                    int CountMax = 0;

                    CountMax = data[0].valueBeforeTax.Count;

                    datacol = 17 + CountMax * 3;
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

                    ICellStyle cellStyleDate = workbook.CreateCellStyle();
                    cellStyleDate.DataFormat = workbook.CreateDataFormat().GetFormat("dd/mm/yyyy");
                    cellStyleDate.BorderBottom = BorderStyle.Thin;
                    cellStyleDate.BorderLeft = BorderStyle.Thin;
                    cellStyleDate.BorderRight = BorderStyle.Thin;
                    cellStyleDate.BorderTop = BorderStyle.Thin;

                    ICellStyle cellStyleDatePl = workbook.CreateCellStyle();
                    cellStyleDatePl.DataFormat = workbook.CreateDataFormat().GetFormat("dd/mm/yyyy");
                    cellStyleDatePl.BorderBottom = BorderStyle.Thin;
                    cellStyleDatePl.BorderLeft = BorderStyle.Thin;
                    cellStyleDatePl.BorderRight = BorderStyle.Thin;
                    cellStyleDatePl.BorderTop = BorderStyle.Thin;
                    cellStyleDatePl.FillForegroundColor = IndexedColors.Aqua.Index;
                    cellStyleDatePl.FillPattern = FillPattern.SolidForeground;

                    ICellStyle cellStyleMoney = workbook.CreateCellStyle();
                    var dataFormat = workbook.CreateDataFormat();
                    cellStyleMoney.DataFormat = dataFormat.GetFormat("#,##0");
                    cellStyleMoney.BorderBottom = BorderStyle.Thin;
                    cellStyleMoney.BorderLeft = BorderStyle.Thin;
                    cellStyleMoney.BorderRight = BorderStyle.Thin;
                    cellStyleMoney.BorderTop = BorderStyle.Thin;

                    ICellStyle cellStyleMoneyPl = workbook.CreateCellStyle();
                    var dataFormatPl = workbook.CreateDataFormat();
                    cellStyleMoneyPl.DataFormat = dataFormat.GetFormat("#,##0");
                    cellStyleMoneyPl.BorderBottom = BorderStyle.Thin;
                    cellStyleMoneyPl.BorderLeft = BorderStyle.Thin;
                    cellStyleMoneyPl.BorderRight = BorderStyle.Thin;
                    cellStyleMoneyPl.BorderTop = BorderStyle.Thin;
                    cellStyleMoneyPl.FillForegroundColor = IndexedColors.Aqua.Index;
                    cellStyleMoneyPl.FillPattern = FillPattern.SolidForeground;

                    ICellStyle cellStylePl = workbook.CreateCellStyle();
                    cellStylePl.Alignment = HorizontalAlignment.Center;
                    cellStylePl.BorderBottom = BorderStyle.Thin;
                    cellStylePl.BorderLeft = BorderStyle.Thin;
                    cellStylePl.BorderRight = BorderStyle.Thin;
                    cellStylePl.BorderTop = BorderStyle.Thin;
                    cellStylePl.FillForegroundColor = IndexedColors.Aqua.Index;
                    cellStylePl.FillPattern = FillPattern.SolidForeground;

                    IRow rowHeaders = sheet.CreateRow(rowHeader);
                    IRow rowHeaderPrev = sheet.CreateRow(rowHeader + 1);//Dòng trước rowHeaders
                    IRow rowHeaders1 = sheet.CreateRow(rowHeader1);

                    for (int i = 0; i < datacol; i++)
                    {
                        sheet.SetColumnWidth(i, (int)(19.45 * 256));

                    }

                    ICell cellHeader1 = rowHeaders.CreateCell(0);
                    cellHeader1.SetCellValue("Lần Trả");
                    CellRangeAddress mergedRegioncellHeader1 = new CellRangeAddress(rowHeader, rowHeader1, 0, 0);
                    sheet.AddMergedRegion(mergedRegioncellHeader1);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader1, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader1, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader1, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader1, sheet);
                    cellHeader1.CellStyle = cellStyle;

                    ICell cellHeader2 = rowHeaders.CreateCell(1);
                    cellHeader2.SetCellValue("Ngày thanh toán thực tế");
                    CellRangeAddress mergedRegioncellHeader2 = new CellRangeAddress(rowHeader, rowHeader1, 1, 1);
                    sheet.AddMergedRegion(mergedRegioncellHeader2);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader2, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader2, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader2, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader2, sheet);
                    cellHeader2.CellStyle = cellStyle;

                    ICell cellHeader3 = rowHeaders.CreateCell(2);
                    cellHeader3.SetCellValue("Số hợp đồng");
                    CellRangeAddress mergedRegioncellHeader3 = new CellRangeAddress(rowHeader, rowHeader1, 2, 2);
                    sheet.AddMergedRegion(mergedRegioncellHeader3);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader3, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader3, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader3, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader3, sheet);
                    cellHeader3.CellStyle = cellStyle;

                    ICell cellHeader4 = rowHeaders.CreateCell(3);
                    cellHeader4.SetCellValue("Ngày ký hợp đồng");
                    CellRangeAddress mergedRegioncellHeader4 = new CellRangeAddress(rowHeader, rowHeader1, 3, 3);
                    sheet.AddMergedRegion(mergedRegioncellHeader4);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader4, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader4, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader4, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader4, sheet);
                    cellHeader4.CellStyle = cellStyle;

                    ICell cellHeader5 = rowHeaders.CreateCell(4);
                    cellHeader5.SetCellValue("Họ và tên");
                    CellRangeAddress mergedRegioncellHeader5 = new CellRangeAddress(rowHeader, rowHeader1, 4, 4);
                    sheet.AddMergedRegion(mergedRegioncellHeader5);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader5, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader5, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader5, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader5, sheet);
                    cellHeader5.CellStyle = cellStyle;

                    ICell cellHeader6 = rowHeaders.CreateCell(5);
                    cellHeader6.SetCellValue("Căn hộ");
                    CellRangeAddress mergedRegioncellHeader6 = new CellRangeAddress(rowHeader, rowHeader + 1, 5, 7);
                    sheet.AddMergedRegion(mergedRegioncellHeader6);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader6, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader6, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader6, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader6, sheet);
                    cellHeader6.CellStyle = cellStyle;

                    ICell cellHeader7 = rowHeaders.CreateCell(8);
                    cellHeader7.SetCellValue("Giá căn hộ");
                    CellRangeAddress mergedRegioncellHeader7 = new CellRangeAddress(rowHeader, rowHeader + 1, 8, 9);
                    sheet.AddMergedRegion(mergedRegioncellHeader7);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader7, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader7, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader7, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader7, sheet);
                    cellHeader7.CellStyle = cellStyle;

                    ICell cellHeader8 = rowHeaders.CreateCell(10);
                    cellHeader8.SetCellValue("Nội Dung");
                    CellRangeAddress mergedRegioncellHeader8 = new CellRangeAddress(rowHeader, rowHeader1, 10, 10);
                    sheet.AddMergedRegion(mergedRegioncellHeader8);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader8, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader8, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader8, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader8, sheet);
                    cellHeader8.CellStyle = cellStyle;

                    ICell cellHeader9 = rowHeaders.CreateCell(11);
                    cellHeader9.SetCellValue("Tổng số tiền đã thu");
                    CellRangeAddress mergedRegioncellHeader9 = new CellRangeAddress(rowHeader, rowHeader + 1, 11, 12);
                    sheet.AddMergedRegion(mergedRegioncellHeader9);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader9, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader9, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader9, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader9, sheet);

                    cellHeader9.CellStyle = cellStyle;

                    int valuePrincipalCount = 1 + CountMax;

                    ICell cellHeader10 = rowHeaders.CreateCell(13);
                    cellHeader10.SetCellValue("Diễn giải số tiền gốc đã thu");
                    CellRangeAddress mergedRegioncellHeader10 = new CellRangeAddress(rowHeader, rowHeader, 13, 13 + valuePrincipalCount - 1);
                    sheet.AddMergedRegion(mergedRegioncellHeader10);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader10, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader10, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader10, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader10, sheet);
                    cellHeader10.CellStyle = cellStyle;


                    int valueBeforeTaxCountStart = 13 + valuePrincipalCount;//13 cột phía trước 
                    int valueBeforeTaxCountEnd = valueBeforeTaxCountStart + CountMax;

                    ICell cellHeader11 = rowHeaders.CreateCell(valueBeforeTaxCountStart);
                    cellHeader11.SetCellValue("Diễn giải số tiền lãi đã thu");
                    CellRangeAddress mergedRegioncellHeader11 = new CellRangeAddress(rowHeader, rowHeader, valueBeforeTaxCountStart, valueBeforeTaxCountEnd);
                    sheet.AddMergedRegion(mergedRegioncellHeader11);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader11, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader11, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader11, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader11, sheet);
                    cellHeader11.CellStyle = cellStyle;

                    int valueTotalCountStart = valueBeforeTaxCountEnd + 1;
                    int valueTotalCountEnd = valueTotalCountStart + CountMax;

                    ICell cellHeader12 = rowHeaders.CreateCell(valueTotalCountStart);
                    cellHeader12.SetCellValue("Tổng số tiền");
                    CellRangeAddress mergedRegioncellHeader12 = new CellRangeAddress(rowHeader, rowHeader, valueTotalCountStart, valueTotalCountEnd);
                    sheet.AddMergedRegion(mergedRegioncellHeader12);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader12, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader12, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader12, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader12, sheet);
                    cellHeader12.CellStyle = cellStyle;

                    ICell cellHeader13 = rowHeaders.CreateCell(valueTotalCountEnd + 1);
                    cellHeader13.SetCellValue("Ghi chú");
                    CellRangeAddress mergedRegioncellHeader13 = new CellRangeAddress(rowHeader, rowHeader1, valueTotalCountEnd + 1, valueTotalCountEnd + 1);
                    sheet.AddMergedRegion(mergedRegioncellHeader13);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader13, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader13, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader13, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader13, sheet);
                    cellHeader13.CellStyle = cellStyle;

                    ICell cellHeaders1 = rowHeaders1.CreateCell(5);
                    cellHeaders1.SetCellValue("Số nhà");
                    cellHeaders1.CellStyle = cellStyle;

                    ICell cellHeaders2 = rowHeaders1.CreateCell(6);
                    cellHeaders2.SetCellValue("Khối");
                    cellHeaders2.CellStyle = cellStyle;

                    ICell cellHeaders3 = rowHeaders1.CreateCell(7);
                    cellHeaders3.SetCellValue("Lô");
                    cellHeaders3.CellStyle = cellStyle;

                    ICell cellHeaders4 = rowHeaders1.CreateCell(8);
                    cellHeaders4.SetCellValue("Giá gốc căn hộ");
                    cellHeaders4.CellStyle = cellStyle;

                    ICell cellHeaders5 = rowHeaders1.CreateCell(9);
                    cellHeaders5.SetCellValue("Giá căn hộ trước thuế");
                    cellHeaders5.CellStyle = cellStyle;

                    ICell cellHeaders6 = rowHeaders1.CreateCell(11);
                    cellHeaders6.SetCellValue("Số tiền gốc đã thu");
                    cellHeaders6.CellStyle = cellStyle;

                    ICell cellHeaders7 = rowHeaders1.CreateCell(12);
                    cellHeaders7.SetCellValue("Số tiền lãi đã thu");
                    cellHeaders7.CellStyle = cellStyle;

                    ICell cellHeaders8 = rowHeaderPrev.CreateCell(13);
                    cellHeaders8.SetCellValue("Số tiền gốc trước thuế");
                    cellHeaders8.CellStyle = cellStyle;

                    int valuePrincipalStart = 13;
                    foreach (var itemPat in data[0].Pat)
                    {
                        if (itemPat.PATName != "VAT")
                        {
                            valuePrincipalStart++;
                            ICell cellHeaders = rowHeaderPrev.CreateCell(valuePrincipalStart);
                            cellHeaders.SetCellValue(itemPat.PATName);
                            cellHeaders.CellStyle = cellStyle;

                            ICell cellHeadersPrev = rowHeaders1.CreateCell(valuePrincipalStart);
                            cellHeadersPrev.SetCellValue(itemPat.Value + "%");
                            cellHeadersPrev.CellStyle = cellStyle;
                        }
                    }

                    ICell cellHeaders9 = rowHeaderPrev.CreateCell(valuePrincipalStart + 1);
                    cellHeaders9.SetCellValue("Thuế VAT");
                    cellHeaders9.CellStyle = cellStyle;
                    foreach (var itemPat in data[0].Pat)
                    {
                        if (itemPat.PATName == "VAT")
                        {
                            ICell cellHeadersPrev = rowHeaders1.CreateCell(valuePrincipalStart + 1);
                            cellHeadersPrev.SetCellValue(itemPat.Value + "%");
                            cellHeadersPrev.CellStyle = cellStyle;
                        }
                    }

                    int valueBeforeTaxStart = valuePrincipalStart + 2;

                    ICell cellHeaders10 = rowHeaderPrev.CreateCell(valueBeforeTaxStart);
                    cellHeaders10.SetCellValue("Số tiền lãi trước thuế");
                    cellHeaders10.CellStyle = cellStyle;


                    foreach (var itemPat in data[0].Pat)
                    {
                        if (itemPat.PATName != "VAT")
                        {
                            valueBeforeTaxStart++;
                            ICell cellHeaders = rowHeaderPrev.CreateCell(valueBeforeTaxStart);
                            cellHeaders.SetCellValue(itemPat.PATName);
                            cellHeaders.CellStyle = cellStyle;

                            ICell cellHeadersPrev = rowHeaders1.CreateCell(valueBeforeTaxStart);
                            cellHeadersPrev.SetCellValue(itemPat.Value + "%");
                            cellHeadersPrev.CellStyle = cellStyle;
                        }
                    }

                    ICell cellHeaders11 = rowHeaderPrev.CreateCell(valueBeforeTaxStart + 1);
                    cellHeaders11.SetCellValue("Thuế VAT");
                    cellHeaders11.CellStyle = cellStyle;
                    foreach (var itemPat in data[0].Pat)
                    {
                        if (itemPat.PATName == "VAT")
                        {
                            ICell cellHeadersPrev = rowHeaders1.CreateCell(valueBeforeTaxStart + 1);
                            cellHeadersPrev.SetCellValue(itemPat.Value + "%");
                            cellHeadersPrev.CellStyle = cellStyle;
                        }
                    }

                    int valueStart = valueBeforeTaxStart + 2;

                    ICell cellHeaders12 = rowHeaderPrev.CreateCell(valueStart);
                    cellHeaders12.SetCellValue("Giá gốc căn hộ nộp ngân sách");
                    cellHeaders12.CellStyle = cellStyle;

                    foreach (var itemPat in data[0].Pat)
                    {
                        if (itemPat.PATName != "VAT")
                        {
                            valueStart++;
                            ICell cellHeaders = rowHeaderPrev.CreateCell(valueStart);
                            cellHeaders.SetCellValue(itemPat.PATName);
                            cellHeaders.CellStyle = cellStyle;

                            ICell cellHeadersPrev = rowHeaders1.CreateCell(valueStart);
                            cellHeadersPrev.SetCellValue(itemPat.Value + "%");
                            cellHeadersPrev.CellStyle = cellStyle;
                        }
                    }
                    ICell cellHeaders13 = rowHeaderPrev.CreateCell(valueStart + 1);
                    cellHeaders13.SetCellValue("Thuế VAT");
                    cellHeaders13.CellStyle = cellStyle;
                    foreach (var itemPat in data[0].Pat)
                    {
                        if (itemPat.PATName == "VAT")
                        {
                            ICell cellHeadersPrev = rowHeaders1.CreateCell(valueStart + 1);
                            cellHeadersPrev.SetCellValue(itemPat.Value + "%");
                            cellHeadersPrev.CellStyle = cellStyle;
                        }
                    }

                    foreach (var item in data)
                    {
                        valuePrincipalStart = -99;
                        int valuePrincipalEnd = -99;
                        valueBeforeTaxStart = -99;
                        int valueBeforeTaxEnd = -99;
                        valueStart = -99;
                        int valueEnd = -99;

                        IRow row = sheet.CreateRow(rowStart);



                        for (int i = 0; i < datacol; i++)
                        {
                            ICell cell = row.CreateCell(i);

                            if (i == 0)
                            {
                                if (item.index == null)
                                {
                                    cell.SetCellValue("");
                                    if (item.CheckPublicPay == true)
                                    {
                                        cell.CellStyle = cellStylePl;
                                    }
                                    else
                                    {
                                        cell.CellStyle = cellStyle;
                                    }
                                }
                                else
                                {
                                    cell.SetCellValue((double)item.index);
                                    if (item.CheckPublicPay == true)
                                    {
                                        cell.CellStyle = cellStylePl;
                                    }
                                    else
                                    {
                                        cell.CellStyle = cellStyle;
                                    }
                                }
                            }
                            else if (i == 1)
                            {
                                if (item.PayDateReal == null)
                                {
                                    cell.SetCellValue("");
                                    if (item.CheckPublicPay == true)
                                    {
                                        cell.CellStyle = cellStyleDatePl;
                                    }
                                    else
                                    {
                                        cell.CellStyle = cellStyleDate;
                                    }
                                }
                                else
                                {
                                    cell.SetCellValue((DateTime)item.PayDateReal);
                                    if (item.CheckPublicPay == true)
                                    {
                                        cell.CellStyle = cellStyleDatePl;
                                    }
                                    else
                                    {
                                        cell.CellStyle = cellStyleDate;
                                    }
                                }

                            }
                            else if (i == 2)
                            {
                                if (item.ContractNumber == null)
                                {
                                    cell.SetCellValue("");
                                    if (item.CheckPublicPay == true)
                                    {
                                        cell.CellStyle = cellStylePl;
                                    }
                                    else
                                    {
                                        cell.CellStyle = cellStyle;
                                    }
                                }
                                else
                                {
                                    if (item.CheckTotal == 2)
                                    {
                                        ICell cellName = row.CreateCell(0);
                                        cellName.SetCellValue("Tổng Cộng");
                                        CellRangeAddress mergedRegioncellName = new CellRangeAddress(rowStart, rowStart, 0, 4);
                                        sheet.AddMergedRegion(mergedRegioncellName);
                                        RegionUtil.SetBorderBottom(1, mergedRegioncellName, sheet);
                                        RegionUtil.SetBorderLeft(1, mergedRegioncellName, sheet);
                                        RegionUtil.SetBorderRight(1, mergedRegioncellName, sheet);
                                        RegionUtil.SetBorderTop(1, mergedRegioncellName, sheet);
                                        if (item.CheckPublicPay == true)
                                        {
                                            cellName.CellStyle = cellStylePl;
                                        }
                                        else
                                        {
                                            cellName.CellStyle = cellStyle;
                                        }
                                    }
                                    else if (item.CheckTotal == 3)
                                    {
                                        ICell cellName = row.CreateCell(0);
                                        cellName.SetCellValue("Số tiền còn lại dự kiến thu");
                                        CellRangeAddress mergedRegioncellName = new CellRangeAddress(rowStart, rowStart, 0, 4);
                                        sheet.AddMergedRegion(mergedRegioncellName);
                                        RegionUtil.SetBorderBottom(1, mergedRegioncellName, sheet);
                                        RegionUtil.SetBorderLeft(1, mergedRegioncellName, sheet);
                                        RegionUtil.SetBorderRight(1, mergedRegioncellName, sheet);
                                        RegionUtil.SetBorderTop(1, mergedRegioncellName, sheet);
                                        if (item.CheckPublicPay == true)
                                        {
                                            cellName.CellStyle = cellStylePl;
                                        }
                                        else
                                        {
                                            cellName.CellStyle = cellStyle;
                                        }
                                    }
                                    else
                                    {
                                        cell.SetCellValue(item.ContractNumber);
                                        if (item.CheckPublicPay == true)
                                        {
                                            cell.CellStyle = cellStylePl;
                                        }
                                        else
                                        {
                                            cell.CellStyle = cellStyle;
                                        }
                                    }
                                }

                            }
                            else if (i == 3)
                            {
                                if (item.ContractDate == null)
                                {
                                    cell.SetCellValue("");
                                    if (item.CheckPublicPay == true)
                                    {
                                        cell.CellStyle = cellStylePl;
                                    }
                                    else
                                    {
                                        cell.CellStyle = cellStyle;
                                    }
                                }
                                else
                                {
                                    cell.SetCellValue((DateTime)item.ContractDate);
                                    if (item.CheckPublicPay == true)
                                    {
                                        cell.CellStyle = cellStyleDatePl;
                                    }
                                    else
                                    {
                                        cell.CellStyle = cellStyleDate;
                                    }
                                }


                            }
                            else if (i == 4)
                            {
                                if (item.Name == null)
                                {
                                    cell.SetCellValue("");
                                    if (item.CheckPublicPay == true)
                                    {
                                        cell.CellStyle = cellStylePl;
                                    }
                                    else
                                    {
                                        cell.CellStyle = cellStyle;
                                    }
                                }
                                else
                                {
                                    cell.SetCellValue(item.Name);
                                    if (item.CheckTotal == 99)
                                    {
                                        CellRangeAddress mergedRegioncellName = new CellRangeAddress(rowStart, rowStart + 1, i, i);
                                        sheet.AddMergedRegion(mergedRegioncellName);
                                        RegionUtil.SetBorderBottom(1, mergedRegioncellName, sheet);
                                        RegionUtil.SetBorderLeft(1, mergedRegioncellName, sheet);
                                        RegionUtil.SetBorderRight(1, mergedRegioncellName, sheet);
                                        RegionUtil.SetBorderTop(1, mergedRegioncellName, sheet);
                                    }
                                    if (item.CheckPublicPay == true)
                                    {
                                        cell.CellStyle = cellStylePl;
                                    }
                                    else
                                    {
                                        cell.CellStyle = cellStyle;
                                    }
                                }

                            }
                            else if (i == 5)
                            {
                                if (item.TdcApartmentName == null)
                                {
                                    cell.SetCellValue("");
                                    if (item.CheckPublicPay == true)
                                    {
                                        cell.CellStyle = cellStylePl;
                                    }
                                    else
                                    {
                                        cell.CellStyle = cellStyle;
                                    }
                                }
                                else
                                {
                                    cell.SetCellValue(item.TdcApartmentName);
                                    if (item.CheckPublicPay == true)
                                    {
                                        cell.CellStyle = cellStylePl;
                                    }
                                    else
                                    {
                                        cell.CellStyle = cellStyle;
                                    }
                                }
                            }
                            else if (i == 6)
                            {
                                if (item.TdcBlockHouseName == null)
                                {
                                    cell.SetCellValue("");
                                    if (item.CheckPublicPay == true)
                                    {
                                        cell.CellStyle = cellStylePl;
                                    }
                                    else
                                    {
                                        cell.CellStyle = cellStyle;
                                    }
                                }
                                else
                                {
                                    cell.SetCellValue(item.TdcBlockHouseName);
                                    if (item.CheckPublicPay == true)
                                    {
                                        cell.CellStyle = cellStylePl;
                                    }
                                    else
                                    {
                                        cell.CellStyle = cellStyle;
                                    }
                                }
                            }
                            else if (i == 7)
                            {
                                if (item.TdcLandName == null)
                                {
                                    cell.SetCellValue("");
                                    if (item.CheckPublicPay == true)
                                    {
                                        cell.CellStyle = cellStylePl;
                                    }
                                    else
                                    {
                                        cell.CellStyle = cellStyle;
                                    }
                                }
                                else
                                {
                                    cell.SetCellValue(item.TdcLandName);
                                    if (item.CheckPublicPay == true)
                                    {
                                        cell.CellStyle = cellStylePl;
                                    }
                                    else
                                    {
                                        cell.CellStyle = cellStyle;
                                    }
                                }
                            }
                            else if (i == 8)
                            {
                                if (item.OriginalPrice == 0)
                                {
                                    cell.SetCellValue("");
                                    if (item.CheckPublicPay == true)
                                    {
                                        cell.CellStyle = cellStyleMoneyPl;
                                    }
                                    else
                                    {
                                        cell.CellStyle = cellStyleMoney;
                                    }
                                }
                                else
                                {
                                    cell.SetCellValue((double)item.OriginalPrice);
                                    if (item.CheckPublicPay == true)
                                    {
                                        cell.CellStyle = cellStyleMoneyPl;
                                    }
                                    else
                                    {
                                        cell.CellStyle = cellStyleMoney;
                                    }
                                }
                            }
                            else if (i == 9)
                            {
                                if (item.BeforeTax == 0)
                                {
                                    cell.SetCellValue("");
                                    if (item.CheckPublicPay == true)
                                    {
                                        cell.CellStyle = cellStyleMoneyPl;
                                    }
                                    else
                                    {
                                        cell.CellStyle = cellStyleMoney;
                                    }
                                }
                                else
                                {
                                    cell.SetCellValue((double)item.BeforeTax);
                                    if (item.CheckPublicPay == true)
                                    {
                                        cell.CellStyle = cellStyleMoneyPl;
                                    }
                                    else
                                    {
                                        cell.CellStyle = cellStyleMoney;
                                    }
                                }
                            }
                            else if (i == 10)
                            {
                                cell.SetCellValue("");
                                if (item.CheckPublicPay == true)
                                {
                                    cell.CellStyle = cellStylePl;
                                }
                                else
                                {
                                    cell.CellStyle = cellStyle;
                                }
                            }
                            else if (i == 11)
                            {
                                if (item.PrincipalReceived == 0)
                                {
                                    cell.SetCellValue("");
                                    if (item.CheckPublicPay == true)
                                    {
                                        cell.CellStyle = cellStyleMoneyPl;
                                    }
                                    else
                                    {
                                        cell.CellStyle = cellStyleMoney;
                                    }
                                }
                                else
                                {
                                    cell.SetCellValue((double)item.PrincipalReceived);
                                    if (item.CheckPublicPay == true)
                                    {
                                        cell.CellStyle = cellStyleMoneyPl;
                                    }
                                    else
                                    {
                                        cell.CellStyle = cellStyleMoney;
                                    }
                                }
                            }
                            else if (i == 12)
                            {
                                if (item.InterestRecevied == 0 && 0 < item.CheckTotal && item.CheckTotal < 3)
                                {
                                    CellRangeAddress mergedRegioncellInterestRecevied = new CellRangeAddress(rowStart, rowStart, i - 1, i);
                                    sheet.AddMergedRegion(mergedRegioncellInterestRecevied);
                                    RegionUtil.SetBorderBottom(1, mergedRegioncellInterestRecevied, sheet);
                                    RegionUtil.SetBorderLeft(1, mergedRegioncellInterestRecevied, sheet);
                                    RegionUtil.SetBorderRight(1, mergedRegioncellInterestRecevied, sheet);
                                    RegionUtil.SetBorderTop(1, mergedRegioncellInterestRecevied, sheet);
                                }
                                cell.SetCellValue((double)item.InterestRecevied);
                                if (item.CheckPublicPay == true)
                                {
                                    cell.CellStyle = cellStyleMoneyPl;
                                }
                                else
                                {
                                    cell.CellStyle = cellStyleMoney;
                                }
                            }
                            ////
                            else if (i == 13)
                            {
                                cell.SetCellValue((double)item.PrincipalBfTax);
                                if (item.CheckPublicPay == true)
                                {
                                    cell.CellStyle = cellStyleMoneyPl;
                                }
                                else
                                {
                                    cell.CellStyle = cellStyleMoney;
                                }
                                valuePrincipalStart = i + 1;
                            }
                            else if (i == valuePrincipalStart)
                            {
                                if (item.valueTotal != null)
                                {
                                    foreach (var childItem in item.valueTotal)
                                    {
                                        if (childItem.Name != "VAT")
                                        {
                                            ICell cellValue = row.CreateCell(i);
                                            cellValue.SetCellValue((double)childItem.ValuePrincipal);
                                            if (item.CheckPublicPay == true)
                                            {
                                                cellValue.CellStyle = cellStyleMoneyPl;
                                            }
                                            else
                                            {
                                                cellValue.CellStyle = cellStyleMoney;
                                            }
                                            i++;
                                        }
                                    }
                                }
                                else
                                {
                                    if (item.valuePrincipal == null)
                                    {
                                        for (int z = 0; z < CountMax - 1; z++)
                                        {
                                            ICell cellValue = row.CreateCell(i);
                                            cellValue.SetCellValue("");
                                            if (item.CheckPublicPay == true)
                                            {
                                                cellValue.CellStyle = cellStylePl;
                                            }
                                            else
                                            {
                                                cellValue.CellStyle = cellStyle;
                                            }
                                            i++;
                                        }
                                    }
                                    else
                                    {
                                        foreach (var childItem in item.valuePrincipal)
                                        {
                                            if (childItem.Name != "VAT")
                                            {
                                                ICell cellValue = row.CreateCell(i);
                                                cellValue.SetCellValue((double)childItem.Value);
                                                if (item.CheckPublicPay == true)
                                                {
                                                    cellValue.CellStyle = cellStyleMoneyPl;
                                                }
                                                else
                                                {
                                                    cellValue.CellStyle = cellStyleMoney;
                                                }
                                                i++;
                                            }
                                        }
                                    }
                                }
                                valuePrincipalEnd = i;
                                i--;
                            }
                            else if (i == valuePrincipalEnd)
                            {
                                cell.SetCellValue((double)item.VATPrincipal);
                                if (item.CheckPublicPay == true)
                                {
                                    cell.CellStyle = cellStyleMoneyPl;
                                }
                                else
                                {
                                    cell.CellStyle = cellStyleMoney;
                                }
                                valueBeforeTaxStart = i + 1;
                            }
                            /////

                            else if (i == valueBeforeTaxStart)
                            {
                                cell.SetCellValue((double)item.InterestBfTax);
                                if (item.CheckPublicPay == true)
                                {
                                    cell.CellStyle = cellStyleMoneyPl;
                                }
                                else
                                {
                                    cell.CellStyle = cellStyleMoney;
                                }
                            }
                            else if (i == valueBeforeTaxStart + 1)
                            {
                                if (item.valueTotal != null)
                                {
                                    foreach (var childItem in item.valueTotal)
                                    {
                                        if (childItem.Name != "VAT")
                                        {
                                            ICell cellValue = row.CreateCell(i);
                                            cellValue.SetCellValue((double)childItem.ValueInterest);
                                            if (item.CheckPublicPay == true)
                                            {
                                                cellValue.CellStyle = cellStyleMoneyPl;
                                            }
                                            else
                                            {
                                                cellValue.CellStyle = cellStyleMoney;
                                            }
                                            i++;
                                        }
                                    }
                                }
                                else
                                {
                                    if (item.valueBeforeTax == null)
                                    {
                                        for (int z = 0; z < CountMax - 1; z++)
                                        {
                                            ICell cellValue = row.CreateCell(i);
                                            cellValue.SetCellValue("");
                                            if (item.CheckPublicPay == true)
                                            {
                                                cellValue.CellStyle = cellStylePl;
                                            }
                                            else
                                            {
                                                cellValue.CellStyle = cellStyle;
                                            }
                                            i++;
                                        }
                                    }
                                    else
                                    {
                                        foreach (var childItem in item.valueBeforeTax)
                                        {
                                            if (childItem.Name != "VAT")
                                            {
                                                ICell cellValue = row.CreateCell(i);
                                                cellValue.SetCellValue((double)childItem.Value);
                                                if (item.CheckPublicPay == true)
                                                {
                                                    cellValue.CellStyle = cellStyleMoneyPl;
                                                }
                                                else
                                                {
                                                    cellValue.CellStyle = cellStyleMoney;
                                                }
                                                i++;
                                            }
                                        }
                                    }
                                }
                                valueBeforeTaxEnd = i;
                                i--;
                            }
                            else if (i == valueBeforeTaxEnd)
                            {
                                cell.SetCellValue((double)item.VATInterest);
                                if (item.CheckPublicPay == true)
                                {
                                    cell.CellStyle = cellStyleMoneyPl;
                                }
                                else
                                {
                                    cell.CellStyle = cellStyleMoney;
                                }
                                valueStart = i + 1;
                            }
                            ////
                            else if (i == valueStart)
                            {
                                cell.SetCellValue((double)item.Principal);
                                if (item.CheckPublicPay == true)
                                {
                                    cell.CellStyle = cellStyleMoneyPl;
                                }
                                else
                                {
                                    cell.CellStyle = cellStyleMoney;
                                }
                            }
                            else if (i == valueStart + 1)
                            {
                                if (item.valueTotal != null)
                                {
                                    foreach (var childItem in item.valueTotal)
                                    {
                                        if (childItem.Name != "VAT")
                                        {
                                            ICell cellValue = row.CreateCell(i);
                                            cellValue.SetCellValue((double)childItem.Value);
                                            if (item.CheckPublicPay == true)
                                            {
                                                cellValue.CellStyle = cellStyleMoneyPl;
                                            }
                                            else
                                            {
                                                cellValue.CellStyle = cellStyleMoney;
                                            }
                                            i++;
                                        }
                                    }
                                }
                                else
                                {
                                    if (item.values == null)
                                    {
                                        for (int z = 0; z < CountMax - 1; z++)
                                        {
                                            ICell cellValue = row.CreateCell(i);
                                            cellValue.SetCellValue("");
                                            if (item.CheckPublicPay == true)
                                            {
                                                cellValue.CellStyle = cellStylePl;
                                            }
                                            else
                                            {
                                                cellValue.CellStyle = cellStyle;
                                            }
                                            i++;
                                        }
                                    }
                                    else
                                    {
                                        foreach (var childItem in item.values)
                                        {
                                            if (childItem.Name != "VAT")
                                            {
                                                ICell cellValue = row.CreateCell(i);
                                                cellValue.SetCellValue((double)childItem.Value);
                                                if (item.CheckPublicPay == true)
                                                {
                                                    cellValue.CellStyle = cellStyleMoneyPl;
                                                }
                                                else
                                                {
                                                    cellValue.CellStyle = cellStyleMoney;
                                                }
                                                i++;
                                            }
                                        }
                                    }
                                }
                                valueEnd = i;
                                i--;
                            }
                            else if (i == valueEnd)
                            {
                                cell.SetCellValue((double)item.VATTotal);
                                if (item.CheckPublicPay == true)
                                {
                                    cell.CellStyle = cellStyleMoneyPl;
                                }
                                else
                                {
                                    cell.CellStyle = cellStyleMoney;
                                }
                            }
                            ////
                            else if (i == valueEnd + 1)
                            {
                                cell.SetCellValue(item.Note);
                                if (item.CheckPublicPay == true)
                                {
                                    cell.CellStyle = cellStylePl;
                                }
                                else
                                {
                                    cell.CellStyle = cellStyle;
                                }
                            }
                        }
                        rowStart = rowStart + 1;
                    }

                    int rowFooter2 = rowStart + 1;
                    int rowFooter3 = rowStart + 2;
                    rowStart = rowStart + 1;

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

                    if (rowStart == rowFooter2)
                    {
                        XSSFRow rowsFooter4 = (XSSFRow)sheet1.CreateRow(rowStart);
                        ICell cell5 = rowsFooter4.CreateCell(16);
                        cell5.SetCellValue("TP. Hồ Chí Minh, Ngày " + "       " + " tháng " + "      " + " năm");
                        cell5.CellStyle = cellStyle1;

                        ICell cell4 = rowsFooter4.CreateCell(21);
                        cell4.SetCellValue("TỔNG HỢP TỪNG HẠNG MỤC CHI PHÍ");
                        cell4.CellStyle = cellStyle1;
                        rowStart++;
                    }

                    if (rowStart == rowFooter3)
                    {
                        XSSFRow rowsFooter = (XSSFRow)sheet4.CreateRow(rowStart);
                        ICell cell = rowsFooter.CreateCell(7);
                        cell.SetCellValue("NGƯỜI LẬP");
                        cell.CellStyle = cellStyle1;

                        ICell cell3 = rowsFooter.CreateCell(16);
                        cell3.SetCellValue("PHÓ TRƯỞNG PHÒNG");
                        cell3.CellStyle = cellStyle1;

                        ICell cell2 = rowsFooter.CreateCell(21);
                        cell2.SetCellValue("(ĐÃ THANH TOÁN CHO TRUNG TÂM)");
                        cell2.CellStyle = cellStyle1;
                        rowStart++;
                    }

                    for (int i = 0; i < 7; i++)
                    {
                        XSSFRow rowFooter = (XSSFRow)sheet.CreateRow(rowStart);
                        for (int j = 20; j < 24; j++)
                        {
                            ICell cell = rowFooter.CreateCell(j);
                            if (i == 0)
                            {
                                if (j == 20)
                                {
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("Loại giá");
                                }
                                else if (j == 21)
                                {
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("Mức");
                                }
                                else if (j == 22)
                                {
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("Tổng");
                                }
                                else if (j == 23)
                                {
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("Ghi chú");
                                }
                            }
                            if (i == 1)
                            {
                                if (j == 20)
                                {
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("Giá bán căn hộ");
                                }
                                else if (j == 21)
                                {
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                                else if (j == 22)
                                {
                                    cell.CellStyle = cellStyleMoney;
                                    cell.SetCellValue((double)data[data.Count - 2].PrincipalReceived);
                                }
                                else if (j == 23)
                                {
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                            }
                            if (i == 2)
                            {
                                if (j == 20)
                                {
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("Giá gốc căn hộ");
                                }
                                else if (j == 21)
                                {
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");

                                }
                                else if (j == 22)
                                {
                                    cell.CellStyle = cellStyleMoney;
                                    cell.SetCellValue((double)data[data.Count - 2].Principal);
                                }
                                else if (j == 23)
                                {
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                            }
                            if (i == 3)
                            {
                                if (j == 20)
                                {
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("Thuế VAT ");
                                }
                                else if (j == 21)
                                {
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                                else if (j == 22)
                                {
                                    foreach (var q in data[data.Count - 2].valueTotal)
                                    {
                                        if (q.Name.Contains("VAT"))
                                        {
                                            cell.CellStyle = cellStyleMoney;
                                            cell.SetCellValue((double)data[data.Count - 2].VATTotal);
                                            break;
                                        }
                                    }
                                }
                                else if (j == 23)
                                {
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                            }
                            if (i == 4)
                            {
                                if (j == 20)
                                {
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("Chi phí quản lý ");
                                }
                                else if (j == 21)
                                {
                                    foreach (var q in data[data.Count - 2].valueTotal)
                                    {
                                        if (q.Name.Contains("phí quản lý"))
                                        {
                                            cell.CellStyle = cellStyle;
                                            cell.SetCellValue(q.Name);
                                            break;
                                        }
                                    }
                                }
                                else if (j == 22)
                                {
                                    foreach (var q in data[data.Count - 2].valueTotal)
                                    {
                                        if (q.Name.Contains("phí quản lý"))
                                        {
                                            cell.CellStyle = cellStyleMoney;
                                            cell.SetCellValue((double)q.Value);
                                            break;
                                        }
                                    }
                                }
                                else if (j == 23)
                                {
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                            }
                            if (i == 5)
                            {
                                if (j == 20)
                                {
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("Chi phí bảo trì ");
                                }
                                else if (j == 21)
                                {
                                    foreach (var q in data[data.Count - 2].valueTotal)
                                    {
                                        if (q.Name.Contains("3% phí bảo trì"))
                                        {
                                            cell.CellStyle = cellStyleMoney;
                                            cell.SetCellValue(q.Name);
                                            break;
                                        }
                                    }
                                }
                                else if (j == 22)
                                {
                                    foreach (var q in data[data.Count - 2].valueTotal)
                                    {
                                        if (q.Name.Contains("3% phí bảo trì"))
                                        {
                                            cell.CellStyle = cellStyleMoney;
                                            cell.SetCellValue((double)q.Value);
                                            break;
                                        }
                                    }
                                }
                                else if (j == 23)
                                {
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                            }
                            if (i == 6)
                            {
                                if (j == 20)
                                {
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("Chi phí bảo trì ");
                                }
                                else if (j == 21)
                                {
                                    foreach (var q in data[data.Count - 2].valueTotal)
                                    {
                                        if (q.Name.Contains("2% phí bảo trì"))
                                        {
                                            cell.CellStyle = cellStyleMoney;
                                            cell.SetCellValue(q.Name);
                                            break;
                                        }
                                    }
                                }
                                else if (j == 22)
                                {
                                    foreach (var q in data[data.Count - 2].valueTotal)
                                    {
                                        if (q.Name.Contains("2% phí bảo trì"))
                                        {
                                            cell.CellStyle = cellStyleMoney;
                                            cell.SetCellValue((double)q.Value);
                                            break;
                                        }
                                    }
                                }
                                else if (j == 23)
                                {
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                            }
                        }
                        rowStart++;
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

        [HttpPost("GetReportNo2/{Id}")]
        public async Task<IActionResult> GetReportNo2(int id, [FromBody] List<ValueAndTotal> input)
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

                //Lấy ra hồ sơ bán trả góp
                TDCInstallmentPrice tDCInstallmentPrice = _context.TDCInstallmentPrices.Where(l => l.Id == id && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                if (tDCInstallmentPrice == null)
                {
                    def.meta = new Meta(404, "Không tìm thấy hồ sơ tương ứng!");
                    return Ok(def);
                }
                //Khai bao bien
                decimal TotalCt = 0;
                decimal TotalTt = 0;

                decimal TotalSellCt = 0;
                decimal TotalSellTt = 0;

                decimal TotalPatTt = 0;
                decimal TotalPatCt = 0;

                List<ValueAndTotal> res = new List<ValueAndTotal>();
                ValueAndTotal data = new ValueAndTotal();
                data.IngreReportTemporarys = new List<ReportIngre>();
                data.PatReportTemporary = new List<ReportPat>();
                data.PatReport = new List<ReportPat>();
                data.IngreReport = new List<ReportIngre>();
                TDCInstallmentPriceData mapper_data = _mapper.Map<TDCInstallmentPriceData>(tDCInstallmentPrice);

                List<TDCProjectIngrePrice> tDCProjectIngrePrices = _context.TDCProjectIngrePrices.Where(x => x.TDCProjectId == tDCInstallmentPrice.TdcProjectId && x.Status != AppEnums.EntityStatus.DELETED).ToList();

                //lấy value TP giá bán cấu thành tạm thời
                List<TDCInstallmentTemporaryDetail> tDCInstallmentTemporaryDetails = _context.TDCInstallmentTemporaryDetails.Where(l => l.TDCInstallmentPriceId == tDCInstallmentPrice.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                foreach (TDCInstallmentTemporaryDetail i in tDCInstallmentTemporaryDetails)
                {
                    var Itemnew = new ReportIngre();

                    Itemnew.Id = i.IngredientsPriceId;
                    Itemnew.IngrePriceName = _context.IngredientsPrices.Where(f => f.Id == i.IngredientsPriceId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                    Itemnew.Area = i.Area;
                    Itemnew.UnitPrice = i.UnitPrice;
                    Itemnew.Price = i.Price;
                    Itemnew.Value = tDCProjectIngrePrices.Where(x => x.IngredientsPriceId == i.IngredientsPriceId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Value).FirstOrDefault();

                    data.IngreReportTemporarys.Add(Itemnew);
                    TotalSellTt += Itemnew.Price;
                    TotalTt += (Itemnew.Price / (decimal)Itemnew.Value);


                }

                //lấy value TP giá bán cấu thành chính thức
                List<TDCInstallmentOfficialDetail> tDCInstallmentOfficialDetails = _context.TDCInstallmentOfficialDetails.Where(l => l.TDCInstallmentPriceId == tDCInstallmentPrice.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(p => p.IngredientsPriceId).ToList();
                foreach (TDCInstallmentOfficialDetail i in tDCInstallmentOfficialDetails)
                {
                    var Itemnew = new ReportIngre();

                    Itemnew.Id = i.IngredientsPriceId;
                    Itemnew.IngrePriceName = _context.IngredientsPrices.Where(f => f.Id == i.IngredientsPriceId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault(); ;
                    Itemnew.Area = i.Area;
                    Itemnew.UnitPrice = i.UnitPrice;
                    Itemnew.Price = i.Price;
                    Itemnew.Value = tDCProjectIngrePrices.Where(x => x.IngredientsPriceId == i.IngredientsPriceId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Value).FirstOrDefault();

                    data.IngreReport.Add(Itemnew);
                    TotalSellCt += Itemnew.Price;
                    TotalCt += (Itemnew.Price / (decimal)Itemnew.Value);


                }

                List<TDCProjectPriceAndTax> tDCProjectPriceAndTaxs = _context.TDCProjectPriceAndTaxs.Where(l => l.TDCProjectId == tDCInstallmentPrice.TdcProjectId && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<ReportIngre> ingreDataDetails = new List<ReportIngre>();
                List<TDCProjectPriceAndTaxData> map_TDCProjectPriceAndTaxs = _mapper.Map<List<TDCProjectPriceAndTaxData>>(tDCProjectPriceAndTaxs);
                var tempTotalPriceValueReport = tDCInstallmentPrice.TemporaryTotalPrice.GetValueOrDefault();
                foreach (TDCProjectPriceAndTaxData map_TDCProjectPriceAndTax in map_TDCProjectPriceAndTaxs)
                {
                    List<TDCProjectPriceAndTaxDetails> de = _context.TDCProjectPriceAndTaxDetailss.Where(f => f.PriceAndTaxId == map_TDCProjectPriceAndTax.Id && f.Status != AppEnums.EntityStatus.DELETED).ToList();
                    List<TDCProjectPriceAndTaxDetailData> detail = _mapper.Map<List<TDCProjectPriceAndTaxDetailData>>(de);
                    OriginalPriceAndTax pat = _context.OriginalPriceAndTaxs.Where(f => f.Id == map_TDCProjectPriceAndTax.PriceAndTaxId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    if (de == null) map_TDCProjectPriceAndTax.PATName = "";
                    else map_TDCProjectPriceAndTax.PATName = pat.Name;

                    var ItemnewPATTemporary = new ReportPat();
                    ItemnewPATTemporary.PATName = pat.Name;
                    ItemnewPATTemporary.Value = map_TDCProjectPriceAndTax.Value;
                    ItemnewPATTemporary.Location = map_TDCProjectPriceAndTax.Location;
                    ItemnewPATTemporary.PercentTemporary = 0;
                    ItemnewPATTemporary.PriceTemporary = 0;
                    ItemnewPATTemporary.Percent = 0;
                    ItemnewPATTemporary.Price = 0;
                    ItemnewPATTemporary.IngreReport = new List<ReportIngre>();
                    TotalPatTt = 0;
                    foreach (TDCProjectPriceAndTaxDetailData i in detail)
                    {
                        ReportIngre Itemnew = new ReportIngre();
                        Itemnew = data.IngreReportTemporarys.Where(x => x.Id == i.IngredientsPriceId).FirstOrDefault();
                        ItemnewPATTemporary.IngreReport.Add(Itemnew);
                        TotalPatTt = (Itemnew.Price / (decimal)Itemnew.Value);
                    }
                    if (ItemnewPATTemporary.PATName == "VAT")
                    {
                        ItemnewPATTemporary.Percent = Math.Round(((TotalTt / tempTotalPriceValueReport) * 100) / (decimal)ItemnewPATTemporary.Value, 2);
                    }
                    else
                    {
                        ItemnewPATTemporary.Percent = tempTotalPriceValueReport == 0 ? 0 : Math.Round((TotalPatTt * (decimal)ItemnewPATTemporary.Value) / tempTotalPriceValueReport, 2);
                    }
                    data.PatReportTemporary.Add(ItemnewPATTemporary);

                    var ItemnewPAT = new ReportPat();
                    ItemnewPAT.PATName = pat.Name;
                    ItemnewPAT.Value = map_TDCProjectPriceAndTax.Value;
                    ItemnewPAT.Location = map_TDCProjectPriceAndTax.Location;
                    ItemnewPAT.PercentTemporary = 0;
                    ItemnewPAT.PriceTemporary = 0;
                    ItemnewPAT.Percent = 0;
                    ItemnewPAT.Price = 0;
                    ItemnewPAT.IngreReport = new List<ReportIngre>();
                    TotalPatCt = 0;
                    foreach (TDCProjectPriceAndTaxDetailData i in detail)
                    {
                        ReportIngre Itemnew = new ReportIngre();
                        Itemnew = data.IngreReport.Where(x => x.Id == i.IngredientsPriceId).FirstOrDefault();
                        ItemnewPAT.IngreReport.Add(Itemnew);
                        TotalPatCt = (Itemnew.Price / (decimal)Itemnew.Value);

                    }
                    if (ItemnewPAT.PATName == "VAT")
                    {
                        ItemnewPAT.Percent = Math.Round(((TotalCt / tDCInstallmentPrice.TotalPrice) * 100) / (decimal)ItemnewPAT.Value, 2);
                    }
                    else
                    {
                        ItemnewPAT.Percent = Math.Round((TotalPatCt * (decimal)ItemnewPAT.Value) / tDCInstallmentPrice.TotalPrice, 2);
                    }
                    data.PatReport.Add(ItemnewPAT);
                }

                data.TotalSellCt = TotalSellCt; // tổng bán ct
                data.TotalCt = TotalCt;// tổng gốc ct
                data.TotalSellTt = TotalSellTt; // tổng bán tt
                data.TotalTt = TotalTt;// tổng gốc tt                
                data.PercentSell = Math.Round(((double)data.TotalSellCt / (double)TotalSellCt) * 100, 2);//100% giá bán
                data.PercentOrigin = Math.Round((double)data.TotalCt / (double)data.TotalSellCt * 100, 2);//% giá gốc
                data.PercentIngre = Math.Round((double)data.sumPatCt / (double)data.TotalSellCt * 100, 2);//% giá thành phần gốc thuế phí
                data.LandName = _context.Lands.Where(f => f.Id == mapper_data.LandId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                data.DecreeNumber = tDCInstallmentPrice.DecreeNumber == null ? "" : tDCInstallmentPrice.DecreeNumber;
                data.DecreeDate = tDCInstallmentPrice.DecreeDate;

                res.Add(data);

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

        [HttpPost("ExportReportNo2")]
        public async Task<IActionResult> ExportReportNo2([FromBody] List<ValueAndTotal> input)
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
            //Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"templates/BaoCao-2.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);
            string nameExcel = "BaoCao2_.xls";

            MemoryStream ms = WriteDataToExcel1(templatePath, 0, input);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", nameExcel);
        }
        private static MemoryStream WriteDataToExcel1(string templatePath, int sheetnumber, List<ValueAndTotal> data)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowTitle = 2;
            int rowHeader = 3;
            int rowHeader1 = 4;
            int rowHeader2 = 5;
            int rowHeader3 = 6;
            int rowHeader4 = 7;
            int rowStart = 8;

            if (sheet != null)
            {
                int datacol = 0;
                int countMax = 0;

                countMax = data[0].IngreReport.Count;
                foreach (var item in data)
                {
                    datacol = 4 + ((item.IngreReport != null ? item.IngreReport.Count : 0));
                    var style = workbook.CreateCellStyle();
                    style.BorderBottom = BorderStyle.Thin;
                    style.BorderLeft = BorderStyle.Thin;
                    style.BorderRight = BorderStyle.Thin;
                    style.BorderTop = BorderStyle.Thin;

                    IFont font = workbook.CreateFont();
                    font.FontHeightInPoints = 14; // Đặt kích thước chữ là 16
                    font.FontName = "Times New Roman";

                    ICellStyle cellStyle = workbook.CreateCellStyle();
                    cellStyle.Alignment = HorizontalAlignment.Center;
                    cellStyle.BorderBottom = BorderStyle.Thin;
                    cellStyle.BorderLeft = BorderStyle.Thin;
                    cellStyle.BorderRight = BorderStyle.Thin;
                    cellStyle.BorderTop = BorderStyle.Thin;
                    cellStyle.SetFont(font);

                    ICellStyle cellStyleMoney = workbook.CreateCellStyle();
                    var dataFormat = workbook.CreateDataFormat();
                    cellStyleMoney.DataFormat = dataFormat.GetFormat("#,##0");
                    cellStyleMoney.BorderBottom = BorderStyle.Thin;
                    cellStyleMoney.BorderLeft = BorderStyle.Thin;
                    cellStyleMoney.BorderRight = BorderStyle.Thin;
                    cellStyleMoney.BorderTop = BorderStyle.Thin;
                    cellStyleMoney.SetFont(font);

                    IRow rowHeaders = sheet.CreateRow(rowHeader);
                    IRow rowHeaderPrev = sheet.CreateRow(rowHeader + 1);//Dòng trước rowHeaders
                    IRow rowHeaders1 = sheet.CreateRow(rowHeader1);
                    IRow rowHeaders2 = sheet.CreateRow(rowHeader2);
                    IRow rowHeaders3 = sheet.CreateRow(rowHeader3);
                    IRow rowHeaders4 = sheet.CreateRow(rowHeader4);
                    IRow rowTitles = sheet.CreateRow(rowTitle);

                    string formattedDate = item.DecreeDate.ToString("dd/MM/yyyy");

                    for (int i = 0; i < datacol; i++)
                    {
                        sheet.SetColumnWidth(i, (int)(25 * 256));

                    }

                    ICell cellTitle = rowTitles.CreateCell(2);
                    cellTitle.SetCellValue("Giá trị theo Quyết định số: " + item.DecreeNumber + " Ngày " + formattedDate);
                    cellTitle.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader1 = new CellRangeAddress(rowTitle, rowTitle, 2, 7);
                    sheet.AddMergedRegion(mergedRegioncellHeader1);

                    ICell cellHeader1 = rowHeaders1.CreateCell(1);
                    cellHeader1.SetCellValue("Loại giá");
                    cellHeader1.CellStyle = cellStyle;

                    ICell cellHeader2 = rowHeaders1.CreateCell(2);
                    cellHeader2.SetCellValue("Mức");
                    cellHeader2.CellStyle = cellStyle;

                    int nameValueCount = 3;
                    foreach (var i in item.IngreReport)
                    {
                        ICell cellHeader3 = rowHeaders1.CreateCell(nameValueCount);
                        cellHeader3.SetCellValue(i.IngrePriceName);
                        cellHeader3.CellStyle = cellStyle;
                        nameValueCount++;
                    }

                    ICell cellHeader4 = rowHeaders1.CreateCell(nameValueCount);
                    cellHeader4.SetCellValue("Tổng");
                    cellHeader4.CellStyle = cellStyle;

                    ICell cellHeader5 = rowHeaders1.CreateCell(nameValueCount + 1);
                    cellHeader5.SetCellValue("Tỉ lệ");
                    cellHeader5.CellStyle = cellStyle;

                    ICell cellHeaders1 = rowHeaders2.CreateCell(1);
                    cellHeaders1.SetCellValue("Tỉ lệ cho " + item.LandName);
                    cellHeaders1.CellStyle = cellStyle;
                    sheet.SetColumnWidth(1, (int)(35 * 256));


                    ICell cellHeaders2 = rowHeaders2.CreateCell(2);
                    cellHeaders2.SetCellValue("");
                    cellHeaders2.CellStyle = cellStyle;

                    int ValueCount = 3;
                    foreach (var i in item.IngreReport)
                    {
                        ICell cellHeaders3 = rowHeaders2.CreateCell(ValueCount);
                        cellHeaders3.SetCellValue(i.Value);
                        cellHeaders3.CellStyle = cellStyle;
                        ValueCount++;
                    }


                    ICell cellHeaders4 = rowHeaders2.CreateCell(ValueCount);
                    cellHeaders4.SetCellValue("");
                    cellHeaders4.CellStyle = cellStyle;

                    ICell cellHeaders5 = rowHeaders2.CreateCell(ValueCount + 1);
                    cellHeaders5.SetCellValue("");
                    cellHeaders5.CellStyle = cellStyle;

                    ICell cellHead1 = rowHeaders3.CreateCell(1);
                    cellHead1.SetCellValue("Giá bán căn hộ");
                    cellHead1.CellStyle = cellStyle;

                    ICell cellHead2 = rowHeaders3.CreateCell(2);
                    cellHead2.SetCellValue("");
                    cellHead2.CellStyle = cellStyle;

                    int PriceCount = 3;
                    foreach (var i in item.IngreReport)
                    {
                        ICell cellHead3 = rowHeaders3.CreateCell(PriceCount);
                        cellHead3.SetCellValue((double)i.Price);
                        cellHead3.CellStyle = cellStyleMoney;
                        PriceCount++;
                    }

                    ICell cellHead4 = rowHeaders3.CreateCell(PriceCount);
                    cellHead4.SetCellValue((double)item.TotalSellCt);
                    cellHead4.CellStyle = cellStyleMoney;

                    ICell cellHead5 = rowHeaders3.CreateCell(PriceCount + 1);
                    cellHead5.SetCellValue((double)item.PercentSell + "%");
                    cellHead5.CellStyle = cellStyle;

                    ICell cellOrigin1 = rowHeaders4.CreateCell(1);
                    cellOrigin1.SetCellValue("Giá gốc căn hộ");
                    cellOrigin1.CellStyle = cellStyle;

                    ICell cellOrigin2 = rowHeaders4.CreateCell(2);
                    cellOrigin2.SetCellValue("");
                    cellOrigin2.CellStyle = cellStyle;

                    int OriginCount = 3;
                    foreach (var i in item.IngreReport)
                    {
                        ICell cellOrigin3 = rowHeaders4.CreateCell(OriginCount);
                        cellOrigin3.SetCellValue(((double)i.Price / i.Value));
                        cellOrigin3.CellStyle = cellStyleMoney;
                        OriginCount++;
                    }

                    ICell cellOrigin4 = rowHeaders4.CreateCell(OriginCount);
                    cellOrigin4.SetCellValue((double)item.TotalCt);
                    cellOrigin4.CellStyle = cellStyleMoney;

                    ICell cellOrigin5 = rowHeaders4.CreateCell(OriginCount + 1);
                    cellOrigin5.SetCellValue((double)item.PercentOrigin + "%");
                    cellOrigin5.CellStyle = cellStyle;

                    foreach (var i in item.PatReport)
                    {
                        double total = 0;

                        IRow rowStarts = sheet.CreateRow(rowStart);

                        ICell cellComponent1 = rowStarts.CreateCell(1);
                        cellComponent1.SetCellValue(i.PATName);
                        cellComponent1.CellStyle = cellStyle;

                        ICell cellComponent2 = rowStarts.CreateCell(2);
                        cellComponent2.SetCellValue(i.Value + "%");
                        cellComponent2.CellStyle = cellStyle;

                        for (int z = 0; z < item.IngreReport.Count; z++)
                        {
                            int patcount = 3 + z;
                            foreach (var childItem in i.IngreReport)
                            {
                                if (item.IngreReport[z].IngrePriceName == childItem.IngrePriceName)
                                {
                                    ICell cellComponent3 = rowStarts.CreateCell(patcount);
                                    cellComponent3.SetCellValue(Math.Round((((double)childItem.Price / childItem.Value) * i.Value) / 100));
                                    cellComponent3.CellStyle = cellStyleMoney;
                                    total += Math.Round((((double)childItem.Price / childItem.Value) * i.Value) / 100);
                                    break;
                                }
                                else
                                {
                                    ICell cellComponent3 = rowStarts.CreateCell(patcount);
                                    cellComponent3.SetCellValue("");
                                    cellComponent3.CellStyle = cellStyle;
                                }
                            }
                        }

                        ICell cellComponent4 = rowStarts.CreateCell(countMax + 3);
                        cellComponent4.SetCellValue(total);
                        cellComponent4.CellStyle = cellStyleMoney;

                        ICell cellComponent5 = rowStarts.CreateCell(countMax + 4);
                        cellComponent5.SetCellValue(Math.Round((total / (double)item.TotalSellCt) * 100, 2) + "%");
                        cellComponent5.CellStyle = cellStyle;
                        rowStart++;

                    }
                }

            }
            sheet.ForceFormulaRecalculation = true;
            MemoryStream ms = new MemoryStream();

            workbook.Write(ms);

            return ms;
        }

        #region BACK_UP_WORKSHEET
        //Bảng chiết tính
        [HttpGet("GetWorkSheet/{Id}")]
        public async Task<IActionResult> GetWorkSheet(int Id, DateTime dateTime, bool isPay, bool? payOff, int? payCount)
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
                TDCInstallmentPrice data = _context.TDCInstallmentPrices.Where(t => t.Id == Id && t.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                if (data == null)
                {
                    def.meta = new Meta(404, "Không tìm thấy hồ sơ tương ứng!");
                    return Ok(def);
                }
                bool decreeChange = false;
                bool firstImport = false;
                //Tìm file  import  excel
                int annualImport = 2;
                bool importCheck = false;
                bool totalDebtAdd = false;
                InstallmentPriceTableMetaTdcData debtItemImport = null;
                InstallmentPriceTableMetaTdcData paidItemImport = null;
                List<InstallmentPriceTableMetaTdc> metaDataImport = _context.InstallmentPriceTableMetaTdcs.Where(x => x.Status != AppEnums.EntityStatus.DELETED && x.TdcIntallmentPriceId == Id).ToList();
                List<InstallmentPriceTableMetaTdcData> metaDataImport_mapper = _mapper.Map<List<InstallmentPriceTableMetaTdcData>>(metaDataImport);
                if (metaDataImport.Count() > 0)
                {
                    importCheck = true;
                    foreach (var itemImport in metaDataImport_mapper)
                    {
                        List<InstallmentPriceTableTdc> installmentPriceTableTdcs = _context.InstallmentPriceTableTdcs.Where(l => l.InstallmentPriceTableMetaTdcId == itemImport.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.Location).ToList();
                        itemImport.installmentPriceTableTdcs = installmentPriceTableTdcs;
                        if (itemImport.DataRow == 4)
                        {
                            debtItemImport = itemImport;
                        }
                        if (itemImport.DataRow == 7)
                        {
                            paidItemImport = itemImport;
                        }
                    }
                    if (debtItemImport.installmentPriceTableTdcs[0].PayDateGuess.HasValue)
                        if (debtItemImport.installmentPriceTableTdcs[0].PayDateGuess > data.DecreeDate)
                            decreeChange = true;
                    if (paidItemImport.installmentPriceTableTdcs[0].PayDateGuess.HasValue)
                        if (paidItemImport.installmentPriceTableTdcs[0].PayDateGuess > data.DecreeDate)
                            decreeChange = true;
                }
                decimal lateRate = _context.TDCProjects.Where(x => x.Id == data.TdcProjectId).Select(x => x.LateRate).FirstOrDefault();
                decimal debtRate = _context.TDCProjects.Where(x => x.Id == data.TdcProjectId).Select(x => x.DebtRate).FirstOrDefault();
                decimal lateTotalPay = 0;
                List<TDCInstallmentPricePay> payData = _context.TDCInstallmentPricePays.Where(t => t.TdcInstallmentPriceId == data.Id && t.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.Date).ToList();

                int lastPayOff = 0;
                bool isPayOff = false;


                int Times = payData.Sum(x => x.PayCount) + 2;
                List<AnnualInstallment> installments = _context.AnnualInstallments.Where(x => x.Status != AppEnums.EntityStatus.DELETED).ToList();
                int paytimeIdImport = 0;
                if (payData.Count() != 0)
                {
                    paytimeIdImport = payData[payData.Count() - 1].Id + 500;
                }
                else paytimeIdImport = paytimeIdImport;
                //Biến chạy của bảng trả tiền
                int k = 0;
                decimal firstValueDecreeChange = 0;
                decimal? interest_dataTotal = 0;
                decimal? totalItemPay = 0;

                TdcInstallmentPriceTable firstData = new TdcInstallmentPriceTable(1, null, 0, 1, data.FirstPayDate, null, null, data.FirstPayDate, null, null, null, null, data.OldContractValue, data.FirstPay, null, data.FirstPay, data.FirstPay, data.FirstPay, 0, null);
                List<TdcInstallmentPriceTable> res = new List<TdcInstallmentPriceTable>();
                res.Add(firstData);
                TdcInstallmentPriceTable changeItem = new TdcInstallmentPriceTable();
                //TdcInstallmentPriceTable dataBefore = new TdcInstallmentPriceTable();
                TdcInstallmentPriceTable totalItem = new TdcInstallmentPriceTable();
                decimal? Pricediff = totalItem.PriceDifference = 0;
                TdcInstallmentPriceTable debtItem = new TdcInstallmentPriceTable();
                DateTime? payDateReal_dataBefore = firstData.PayDateReal;
                decimal? totalPay_dataBefore = firstData.TotalPay;
                decimal? payAnnual_dataBefore = firstData.PayAnnual;
                decimal TotalDifference = 0;
                int payTimeId = -1;
                DateTime? firstPayGuess = firstData.PayDateGuess;
                if (importCheck)
                {
                    res.RemoveAt(0);
                    foreach (var itemImport in metaDataImport_mapper)
                    {
                        paytimeIdImport++;

                        foreach (var item in itemImport.installmentPriceTableTdcs)
                        {
                            var itemDataImport = new TdcInstallmentPriceTable(1, item.RowStatus, paytimeIdImport, item.PaymentTimes, item.PayDateDefault, item.PayDateBefore, item.PayDateGuess, item.PayDateReal, item.MonthInterest, item.DailyInterest, item.MonthInterestRate, item.DailyInterestRate, item.TotalPay, item.PayAnnual, item.TotalInterest, item.TotalPayAnnual, item.Pay, item.Paid, item.PriceDifference, item.Note);
                            itemDataImport.DataStatus = 1;
                            itemDataImport.IsImport = true;
                            res.Add(itemDataImport);
                            if (item.PaymentTimes != null) annualImport = (int)item.PaymentTimes;
                        }
                    }
                    if (debtItemImport != null)
                    {
                        foreach (var debtI in debtItemImport.installmentPriceTableTdcs)
                        {
                            firstData = new TdcInstallmentPriceTable(1, debtI.RowStatus, paytimeIdImport, debtI.PaymentTimes, debtI.PayDateDefault, debtI.PayDateBefore, debtI.PayDateGuess, debtI.PayDateReal, debtI.MonthInterest, debtI.DailyInterest, debtI.MonthInterestRate, debtI.DailyInterestRate, debtI.TotalPay, 0, debtI.TotalInterest, 0, debtI.Pay, debtI.Paid, debtI.PriceDifference, debtI.Note);
                            firstData.PayTimeId = payTimeId;
                            //Lãi phát sinh theo tháng va ngày
                            if (firstData.MonthInterest.HasValue && firstData.MonthInterestRate.HasValue)
                                firstData.TotalInterest = ((firstData.DailyInterest * firstData.DailyInterestRate + firstData.MonthInterest * firstData.MonthInterestRate) * firstData.TotalPay) / 100;
                            else
                                firstData.TotalInterest = firstData.DailyInterest * firstData.DailyInterestRate * firstData.TotalPay / 100;
                            interest_dataTotal += firstData.TotalInterest;
                            //Số tiền đến kì phải thanh toán
                            firstData.TotalPayAnnual = firstData.TotalInterest + firstData.PayAnnual;
                            firstData.Pay = firstData.TotalPayAnnual + paidItemImport.PriceDifference;
                            firstData.Paid = 0;
                            firstData.PriceDifference = firstData.Pay - firstData.Paid;
                            if (payData.Count > 0)
                            {
                                firstData.DataStatus = 1;
                                firstData.publicPay = payData[0].PublicPay;
                            }
                            else
                                firstData.DataStatus = 2;
                            res.Add(firstData);
                            if (firstData.PriceDifference.HasValue)
                                TotalDifference = firstData.PriceDifference.Value;
                        }
                        payDateReal_dataBefore = debtItemImport.installmentPriceTableTdcs[0].PayDateReal;
                        totalPay_dataBefore = debtItemImport.installmentPriceTableTdcs[0].TotalPay;
                        payAnnual_dataBefore = debtItemImport.installmentPriceTableTdcs[0].PayAnnual;
                        firstPayGuess = debtItemImport.installmentPriceTableTdcs[0].PayDateGuess;
                        payTimeId++;
                    }
                    else
                    {
                        totalItem.PriceDifference = paidItemImport.PriceDifference;
                    }
                }

                //các biến lưu dữ liệu dòng cộng tổng
                decimal TotalPay = 0;
                int payStatus = 0;
                int isPaid = 1;
                int debtValue = 0;
                //variable
                decimal? payAnnual_dataTotal = 0;
                int timesAgain = 0;

                if (payData.Count() != 0)
                {
                    timesAgain = payData[0].PayCount - 1;
                    isPaid = 0;
                    if (debtItemImport != null)
                    {
                        payTimeId--;
                    }
                }

                if (payOff != null && isPay == true)
                {
                    TDCInstallmentPricePay fakePayData = new TDCInstallmentPricePay();
                    fakePayData.Date = dateTime;
                    fakePayData.IsPayOff = true;
                    fakePayData.Value = 0;
                    fakePayData.PublicPay = false;
                    fakePayData.PayCount = payCount.Value;
                    fakePayData.PayTime = payData[payData.Count() - 1].PayTime + 1;
                    payData.Add(fakePayData);
                }
                if (payOff != null || data.isPayOff)
                {
                    isPayOff = true;
                    lastPayOff = annualImport + payData.Sum(x => x.PayCount) - 1;
                }
                bool remmovePaytimmes = false;
                if (debtItemImport != null)
                {
                    remmovePaytimmes = true;
                }
                for (int i = -1 + annualImport; i <= data.YearPay; i++)
                {

                    TdcInstallmentPriceTable item = new TdcInstallmentPriceTable();
                    if (Pricediff > 1 && debtValue != k)
                    {
                        debtItem = new TdcInstallmentPriceTable();
                        debtItem.PayTimeId = payTimeId;
                        debtItem.PayDateDefault = data.FirstPayDate.AddYears(i);
                        debtItem.PayDateBefore = payDateReal_dataBefore;
                        debtItem.RowStatus = AppEnums.TypePayQD.NO_CU;
                        debtItem.DataStatus = 2;
                        if (k < payData.Count())
                        {
                            debtItem.PayDateReal = payData[k].Date;
                            debtItem.Paid = payData[k].Value;
                            debtItem.DataStatus = 1;
                        }
                        else
                        {
                            debtItem.PayDateReal = dateTime;
                        }
                        //Thơi gian tính lãi theo ngày
                        // Lấy DateTime bên trong kiểu Nullable DateTime
                        if (debtItem.PayDateReal.HasValue && debtItem.PayDateBefore.HasValue)
                        {
                            DateTime start = debtItem.PayDateBefore.Value;
                            DateTime end = debtItem.PayDateReal.Value;
                            // Lấy TimeSpan giữa hai ngày
                            TimeSpan span = end.Subtract(start);
                            // Lấy số ngày chênh lệch
                            debtItem.DailyInterest = (int)span.TotalDays;
                        }
                        else debtItem.DailyInterest = 0;
                        debtItem.DailyInterestRate = debtRate;
                        debtItem.TotalPay = Pricediff;
                        debtItem.PayAnnual = debtItem.TotalPay;
                        debtItem.TotalInterest = debtItem.DailyInterest * debtItem.DailyInterestRate;
                        debtItem.TotalPayAnnual = debtItem.PayAnnual + debtItem.TotalInterest;
                        interest_dataTotal += debtItem.TotalInterest;
                        if (debtItem.TotalPayAnnual.HasValue)
                            TotalPay += debtItem.TotalPayAnnual.Value;

                        debtItem.Pay = debtItem.TotalPayAnnual;
                        totalItemPay += debtItem.Pay;
                        payAnnual_dataTotal += debtItem.PayAnnual;
                        debtItem.PriceDifference = debtItem.Pay - debtItem.Paid;
                        if (debtItem.PriceDifference.HasValue)
                            TotalDifference += debtItem.PriceDifference.Value;
                        debtItem.Note = "Nợ cũ";
                        debtItem.PayDateGuess = debtItem.PayDateReal;
                        res.Add(debtItem);
                        debtValue = k;

                        if (timesAgain == 0 && payData.Count != 0)
                        {
                            totalDebtAdd = true;
                            payTimeId--;
                            totalItem = new TdcInstallmentPriceTable();
                            totalItem.PayDateDefault = debtItem.PayDateDefault;
                            totalItem.PayDateGuess = debtItem.PayDateGuess;
                            totalItem.PayDateReal = debtItem.PayDateReal;
                            if (k < payData.Count())
                            {
                                totalItem.Paid = payData[k].Value;
                            }

                            totalItem.TotalPayAnnual = TotalPay;
                            totalItem.PriceDifference = TotalDifference;
                            totalItem.PayTimeId = payTimeId;
                            totalItem.RowStatus = AppEnums.TypePayQD.TONG;
                            totalItem.TotalInterest = interest_dataTotal;
                            totalItem.PayAnnual = payAnnual_dataTotal;
                            totalItem.DataStatus = 1;
                            Pricediff = totalItem.PriceDifference;
                            //Pricediff = totalItemPay - totalItem.Paid;
                            res.Add(totalItem);
                            payTimeId--;
                            //Tăng biến chạy của lần trả lên 1
                            k++;
                            payAnnual_dataTotal = 0;
                            TotalPay = 0;
                            TotalDifference = 0;
                            interest_dataTotal = 0;
                            totalItemPay = 0;
                            if (k < payData.Count())
                            {
                                timesAgain = payData[k].PayCount - 1;
                                payStatus = 0;
                                isPaid = 0;
                            }
                            else
                            {
                                timesAgain--;
                                payStatus = 1;
                                isPaid = 1;
                            }
                        }
                        else
                        {
                            timesAgain--;
                            payStatus = 1;

                        }
                    }
                    if (totalDebtAdd == false)
                    {
                        //trang thai
                        item.RowStatus = AppEnums.TypePayQD.DUNG_HAN;
                        lateTotalPay = 0;
                        item.TypeRow = isPaid;
                        //kiểm tra xem còn dữ liệu trong payData hay không
                        if (k < payData.Count())
                        {
                            //Ngày thanh toán thực tế
                            item.PayDateReal = payData[k].Date;
                            item.publicPay = payData[k].PublicPay;
                            //Số tiền đã thanh toán
                            if (payStatus == 0)
                            {
                                item.Paid = payData[k].Value;

                            }
                            else
                            {
                                item.Paid = 0;
                            }
                            item.DataStatus = 1;
                        }
                        else
                        {
                            item.PayDateReal = dateTime;
                            item.DataStatus = 2;
                            item.Paid = 0;
                            payTimeId--;
                        }
                        item.PayTimeId = payTimeId;
                        //Lần trả

                        item.PaymentTimes = i + 1;

                        //Ngày thanh toán theo quy định
                        item.PayDateDefault = data.FirstPayDate.AddYears(i);

                        //Ngày thanh toán kì trc
                        //item.PayDateBefore = payDateReal_dataBefore;

                        //if (item.TypeRow == 1)
                        //{
                        item.PayDateBefore = data.FirstPayDate.AddYears(i - 1);
                        //}
                        if (importCheck && firstImport == false)
                        {
                            item.PayDateBefore = firstPayGuess;
                            item.TotalPay = paidItemImport.installmentPriceTableTdcs[0].TotalPay - paidItemImport.installmentPriceTableTdcs[0].PayAnnual;
                            firstImport = true;
                        }
                        else
                            item.TotalPay = totalPay_dataBefore - payAnnual_dataBefore;

                        //Lấy ra khoảng lãi suất áp dụng cho đợt thanh toán
                        AnnualInstallment LsNN = installments.Where(x => x.DoApply < item.PayDateBefore).OrderByDescending(x => x.DoApply).FirstOrDefault();
                        if (LsNN == null)
                        {
                            LsNN = new AnnualInstallment();
                            if (item.PayDateBefore == null) item.PayDateBefore = new DateTime(1900, 3, 9);
                            LsNN.DoApply = (DateTime)item.PayDateBefore;
                            LsNN.Value = 0;
                        }
                        else
                        {

                            LsNN.DoApply = (DateTime)item.PayDateBefore;
                        }
                        List<AnnualInstallment> lstInstallment = installments.Where(x => x.DoApply > item.PayDateBefore && x.DoApply < item.PayDateDefault).OrderBy(x => x.DoApply).ToList();

                        //Ngày thanh toán kỳ này theo dự kiến
                        if (lstInstallment.Count() == 0)
                            item.PayDateGuess = item.PayDateDefault;
                        else item.PayDateGuess = lstInstallment[0].DoApply;

                        if (item.PayDateBefore.HasValue && item.PayDateDefault.HasValue)
                            if (checkDecreeChange(data.DecreeDate, item.PayDateBefore.Value.AddYears(-1), item.PayDateDefault.Value.AddYears(-1)))
                                item.TotalPay += data.DifferenceValue - firstValueDecreeChange;
                        //Lãi suất tạm tính theo tháng
                        item.MonthInterestRate = (decimal)LsNN.Value / 12;
                        //Lãi suất tạm tính theo  ngày
                        item.DailyInterestRate = item.MonthInterestRate / 30;
                        //Số tiền  gốc phải trả hàng năm
                        item.PayAnnual = item.TotalPay / (data.YearPay + 2 - item.PaymentTimes);
                        if (isPayOff && item.PaymentTimes == lastPayOff)
                        {
                            item.PayAnnual = item.TotalPay;
                        }
                        if (checkDecreeChange(data.DecreeDate, item.PayDateBefore.Value, item.PayDateDefault.Value))
                        {
                            item.PayAnnual = 0;
                        }
                        if (remmovePaytimmes == true)
                        {
                            item.PaymentTimes = null;
                            remmovePaytimmes = false;
                        }
                        if (item.PayDateBefore.HasValue && item.PayDateGuess.HasValue)
                        {
                            if (decreeChange == false && checkDecreeChange(data.DecreeDate, item.PayDateBefore.Value, item.PayDateGuess.Value))
                            {
                                decreeChange = true;
                                changeItem = new TdcInstallmentPriceTable();
                                changeItem.RowStatus = item.RowStatus;
                                changeItem.PayTimeId = item.PayTimeId;
                                changeItem.TypeRow = item.TypeRow;
                                changeItem.DataStatus = item.DataStatus;
                                changeItem.PayDateDefault = item.PayDateDefault;
                                changeItem.PayDateBefore = data.DecreeDate;
                                changeItem.PayDateGuess = item.PayDateGuess;
                                item.PayDateGuess = data.DecreeDate;
                                changeItem.PayDateReal = item.PayDateReal;
                                //Thời gian tính lãi theo thàng
                                if (changeItem.PayDateGuess.HasValue && changeItem.PayDateBefore.HasValue)
                                {
                                    changeItem.MonthInterest = (changeItem.PayDateGuess.Value.Year - changeItem.PayDateBefore.Value.Year) * 12 + (changeItem.PayDateGuess.Value.Month - changeItem.PayDateBefore.Value.Month);
                                }
                                //Thơi gian tính lãi theo ngày
                                if (changeItem.PayDateGuess.HasValue && changeItem.PayDateBefore.HasValue)
                                {
                                    // Lấy DateTime bên trong kiểu Nullable DateTime
                                    DateTime start = changeItem.PayDateBefore.Value.AddMonths(changeItem.MonthInterest.Value);
                                    DateTime end = changeItem.PayDateGuess.Value;
                                    // Lấy TimeSpan giữa hai ngày
                                    TimeSpan span = end.Subtract(start);
                                    // Lấy số ngày chênh lệch
                                    changeItem.DailyInterest = (int)span.TotalDays;
                                }
                                if (changeItem.DailyInterest < 0)
                                {
                                    changeItem.DailyInterest = 30 + changeItem.DailyInterest;
                                    changeItem.MonthInterest--;
                                }
                                changeItem.DailyInterestRate = item.DailyInterestRate;
                                changeItem.MonthInterestRate = item.MonthInterestRate;
                                changeItem.TotalPay = item.TotalPay + data.DifferenceValue;
                                //Số tiền  gốc phải trả hàng năm
                                changeItem.PayAnnual = changeItem.TotalPay / (data.YearPay + 2 - item.PaymentTimes);
                                payAnnual_dataTotal += changeItem.PayAnnual;
                                //Lãi phát sinh theo tháng va ngày
                                changeItem.TotalInterest = ((changeItem.DailyInterest * changeItem.DailyInterestRate + changeItem.MonthInterest * changeItem.MonthInterestRate) * changeItem.TotalPay) / 100;
                                interest_dataTotal += changeItem.TotalInterest;
                                //Số tiền đến kì phải thanh toán
                                changeItem.TotalPayAnnual = changeItem.TotalInterest + changeItem.PayAnnual;
                                if (changeItem.TotalPayAnnual.HasValue)
                                {
                                    //Tổng số tiền phải thanh toán
                                    lateTotalPay += changeItem.TotalPayAnnual.Value;
                                    TotalPay += changeItem.TotalPayAnnual.Value;
                                }
                                //Tổng số tiền phải thanh toán
                                //if (Pricediff.HasValue)
                                //    changeItem.Pay = Pricediff + changeItem.TotalPayAnnual;
                                //else
                                changeItem.Pay = changeItem.TotalPayAnnual;
                                totalItemPay += changeItem.Pay;
                                changeItem.Paid = 0;
                                //Chênh lệch
                                changeItem.PriceDifference = changeItem.Pay - changeItem.Paid;
                                if (changeItem.PriceDifference.HasValue)
                                    TotalDifference += changeItem.PriceDifference.Value;
                                changeItem.Note = "Ngày thay đổi quyết định";
                            }
                        }

                        //Thời gian tính lãi theo thàng
                        if (item.PayDateGuess.HasValue && item.PayDateBefore.HasValue)
                        {
                            item.MonthInterest = (item.PayDateGuess.Value.Year - item.PayDateBefore.Value.Year) * 12 + (item.PayDateGuess.Value.Month - item.PayDateBefore.Value.Month);
                        }
                        //Thơi gian tính lãi theo ngày
                        if (item.PayDateGuess.HasValue && item.PayDateBefore.HasValue && item.PayDateReal.HasValue)
                        {
                            // Lấy DateTime bên trong kiểu Nullable DateTime
                            DateTime start = item.PayDateBefore.Value.AddMonths(item.MonthInterest.Value);
                            DateTime end;
                            //if (item.PayDateReal.Value < item.PayDateGuess.Value && item.TypeRow == 0)
                            //    end = item.PayDateReal.Value;
                            //else
                            //{
                            end = item.PayDateGuess.Value;
                            //}
                            // Lấy TimeSpan giữa hai ngày
                            TimeSpan span = end.Subtract(start);
                            // Lấy số ngày chênh lệch
                            item.DailyInterest = (int)span.TotalDays;
                        }
                        if (item.DailyInterest < 0)
                        {
                            item.DailyInterest = 30 + item.DailyInterest;
                            item.MonthInterest--;
                        }

                        payAnnual_dataTotal += item.PayAnnual;
                        //Lãi phát sinh theo tháng va ngày
                        item.TotalInterest = ((item.DailyInterest * item.DailyInterestRate + item.MonthInterest * item.MonthInterestRate) * item.TotalPay) / 100;
                        interest_dataTotal += item.TotalInterest;
                        //Số tiền đến kì phải thanh toán
                        item.TotalPayAnnual = item.TotalInterest + item.PayAnnual;
                        if (item.TotalPayAnnual.HasValue)
                        {
                            //Tổng số tiền phải thanh toán
                            lateTotalPay += item.TotalPayAnnual.Value;
                            TotalPay += item.TotalPayAnnual.Value;
                        }
                        //Tổng số tiền phải thanh toán
                        if (totalItem.PriceDifference.HasValue && item.PaymentTimes < annualImport + payData.Count() + 1)
                        {
                            item.Pay = totalItem.PriceDifference + item.TotalPayAnnual;

                        }
                        else
                        {
                            item.Pay = item.TotalPayAnnual;
                        }
                        totalItemPay += item.Pay;

                        //Chênh lệch
                        item.PriceDifference = item.Pay - item.Paid;
                        if (item.PriceDifference.HasValue)
                            TotalDifference += item.PriceDifference.Value;
                        //if (item.PaymentTimes == Times)
                        //{
                        //    item.PayDateReal = dateTime;
                        //    payStatus = -1;
                        //}
                        //Add thêm dòng vào bảng
                        res.Add(item);
                        if (decreeChange == true)
                        {
                            if (changeItem.PayAnnual.HasValue)
                                firstValueDecreeChange = changeItem.PayAnnual.Value;
                            res.Add(changeItem);
                            decreeChange = false;
                        }
                        //dataBefore = item;

                        payDateReal_dataBefore = item.PayDateReal;
                        totalPay_dataBefore = item.TotalPay;
                        payAnnual_dataBefore = item.PayAnnual;

                        TdcInstallmentPriceTable itemplus = new TdcInstallmentPriceTable();
                        List<ProfitValue> profitValues = _context.ProfitValues.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderBy(p => p.DoApply).ToList();


                        //trường hợp có lãi suất thay đổi
                        if (lstInstallment.Count > 0)
                        {
                            for (int l = 0; l < lstInstallment.Count(); l++)
                            {
                                itemplus = new TdcInstallmentPriceTable();
                                itemplus.RowStatus = AppEnums.TypePayQD.DUNG_HAN;
                                itemplus.PayDateDefault = item.PayDateDefault;
                                itemplus.PayDateBefore = lstInstallment[l].DoApply;
                                if (l < lstInstallment.Count() - 1)
                                    itemplus.PayDateGuess = lstInstallment[l + 1].DoApply;
                                else itemplus.PayDateGuess = item.PayDateDefault;
                                itemplus.PayDateReal = item.PayDateReal;
                                //Lãi suất tạm tính theo tháng
                                itemplus.MonthInterestRate = (decimal)lstInstallment[l].Value / 12;
                                //Lãi suất tạm tính theo  ngày
                                itemplus.DailyInterestRate = itemplus.MonthInterestRate / 30;
                                //Số tiền gốc phải tính lãi 
                                itemplus.TotalPay = item.TotalPay;
                                if (item.PayDateBefore.HasValue && itemplus.PayDateBefore.HasValue && checkDecreeChange(data.DecreeDate, item.PayDateBefore.Value, itemplus.PayDateBefore.Value))
                                    itemplus.TotalPay = item.TotalPay + data.DifferenceValue;
                                itemplus.PayTimeId = item.PayTimeId;
                                if (k < payData.Count())
                                {
                                    itemplus.DataStatus = 1;
                                }
                                else
                                {
                                    itemplus.DataStatus = 2;
                                }
                                if (itemplus.PayDateBefore.HasValue && itemplus.PayDateGuess.HasValue)
                                {
                                    if (decreeChange == false && checkDecreeChange(data.DecreeDate, itemplus.PayDateBefore.Value, itemplus.PayDateGuess.Value))
                                    {
                                        decreeChange = true;
                                        changeItem = new TdcInstallmentPriceTable();
                                        changeItem.RowStatus = itemplus.RowStatus;
                                        changeItem.PayTimeId = itemplus.PayTimeId;
                                        changeItem.DataStatus = itemplus.DataStatus;
                                        changeItem.PayDateDefault = itemplus.PayDateDefault;
                                        changeItem.PayDateBefore = data.DecreeDate;
                                        changeItem.PayDateGuess = itemplus.PayDateGuess;
                                        itemplus.PayDateGuess = data.DecreeDate;
                                        changeItem.PayDateReal = itemplus.PayDateReal;
                                        //Thời gian tính lãi theo thàng
                                        if (changeItem.PayDateGuess.HasValue && changeItem.PayDateBefore.HasValue)
                                        {
                                            changeItem.MonthInterest = (changeItem.PayDateGuess.Value.Year - changeItem.PayDateBefore.Value.Year) * 12 + (changeItem.PayDateGuess.Value.Month - changeItem.PayDateBefore.Value.Month);
                                        }
                                        //Thơi gian tính lãi theo ngày
                                        if (changeItem.PayDateGuess.HasValue && changeItem.PayDateBefore.HasValue && changeItem.PayDateReal.HasValue)
                                        {
                                            // Lấy DateTime bên trong kiểu Nullable DateTime
                                            DateTime start = changeItem.PayDateBefore.Value.AddMonths(changeItem.MonthInterest.Value);
                                            DateTime end;
                                            if (changeItem.PayDateReal.Value < changeItem.PayDateGuess.Value)
                                                end = changeItem.PayDateReal.Value;
                                            else
                                            {
                                                end = changeItem.PayDateGuess.Value;
                                            }
                                            // Lấy TimeSpan giữa hai ngày
                                            TimeSpan span = end.Subtract(start);
                                            // Lấy số ngày chênh lệch
                                            changeItem.DailyInterest = (int)span.TotalDays;
                                        }
                                        if (changeItem.DailyInterest < 0)
                                        {
                                            changeItem.DailyInterest = 30 + changeItem.DailyInterest;
                                            changeItem.MonthInterest--;
                                        }
                                        changeItem.DailyInterestRate = itemplus.DailyInterestRate;
                                        changeItem.MonthInterestRate = itemplus.MonthInterestRate;
                                        changeItem.TotalPay = itemplus.TotalPay + data.DifferenceValue;
                                        //Số tiền  gốc phải trả hàng năm
                                        changeItem.PayAnnual = changeItem.TotalPay / (data.YearPay + 2 - item.PaymentTimes);
                                        payAnnual_dataTotal += changeItem.PayAnnual;
                                        //Lãi phát sinh theo tháng va ngày
                                        changeItem.TotalInterest = ((changeItem.DailyInterest * changeItem.DailyInterestRate + changeItem.MonthInterest * changeItem.MonthInterestRate) * changeItem.TotalPay) / 100;
                                        interest_dataTotal += changeItem.TotalInterest;
                                        //Số tiền đến kì phải thanh toán
                                        changeItem.TotalPayAnnual = changeItem.TotalInterest + changeItem.PayAnnual;
                                        if (changeItem.TotalPayAnnual.HasValue)
                                        {
                                            //Tổng số tiền phải thanh toán
                                            lateTotalPay += changeItem.TotalPayAnnual.Value;
                                            TotalPay += changeItem.TotalPayAnnual.Value;
                                        }
                                        //Tổng số tiền phải thanh toán
                                        //if (totalItem.PriceDifference.HasValue)
                                        //    changeItem.Pay = totalItem.PriceDifference + changeItem.TotalPayAnnual;
                                        //else
                                        changeItem.Pay = changeItem.TotalPayAnnual;
                                        totalItemPay += changeItem.Pay;
                                        changeItem.Paid = 0;
                                        //Chênh lệch
                                        changeItem.PriceDifference = changeItem.Pay - changeItem.Paid;
                                        TotalDifference += changeItem.PriceDifference.HasValue ? changeItem.PriceDifference.Value : 0;

                                    }
                                }
                                //Thời gian tính lãi theo tháng
                                if (itemplus.PayDateGuess.HasValue && itemplus.PayDateBefore.HasValue)
                                {
                                    itemplus.MonthInterest = (itemplus.PayDateGuess.Value.Year - itemplus.PayDateBefore.Value.Year) * 12 + (itemplus.PayDateGuess.Value.Month - itemplus.PayDateBefore.Value.Month);
                                }
                                //Thơi gian tính lãi theo ngày
                                if (itemplus.PayDateGuess.HasValue && itemplus.PayDateBefore.HasValue)
                                {
                                    // Lấy DateTime bên trong kiểu Nullable DateTime
                                    DateTime start = itemplus.PayDateBefore.Value.AddMonths(itemplus.MonthInterest.Value);
                                    DateTime end = itemplus.PayDateGuess.Value;
                                    // Lấy TimeSpan giữa hai ngày
                                    TimeSpan span = end.Subtract(start);
                                    // Lấy số ngày chênh lệch
                                    itemplus.DailyInterest = (int)span.TotalDays;
                                    if (itemplus.DailyInterest < 0)
                                    {
                                        itemplus.DailyInterest = 30 + itemplus.DailyInterest;
                                        itemplus.MonthInterest--;
                                    }
                                }

                                //số tiền trả định kì
                                itemplus.PayAnnual = 0;
                                //lãi phát sinh theo ngày và tháng
                                itemplus.TotalInterest = ((itemplus.DailyInterest * itemplus.DailyInterestRate + itemplus.MonthInterest * itemplus.MonthInterestRate) * itemplus.TotalPay) / 100;
                                interest_dataTotal += itemplus.TotalInterest;

                                //Số tiền đến kì phải thanh toán
                                itemplus.Pay = itemplus.TotalPayAnnual = itemplus.TotalInterest;
                                totalItemPay += itemplus.Pay;
                                //Tổng số tiền phải thanh toán
                                if (itemplus.TotalPayAnnual.HasValue)
                                {
                                    lateTotalPay += itemplus.TotalPayAnnual.Value;
                                    TotalPay += itemplus.TotalPayAnnual.Value;
                                }
                                itemplus.Paid = 0;
                                itemplus.PriceDifference = itemplus.Pay - itemplus.Paid;
                                if (itemplus.PriceDifference.HasValue)
                                    TotalDifference += itemplus.PriceDifference.Value;
                                itemplus.Note = "Thay đổi lãi suất";
                                res.Add(itemplus);
                                if (decreeChange == true)
                                {
                                    firstValueDecreeChange = changeItem.PayAnnual.HasValue ? changeItem.PayAnnual.Value : 0;
                                    res.Add(changeItem);
                                    decreeChange = false;
                                }

                            }
                        }
                        //trường hợp đóng trễ hạn
                        if (item.PayDateReal > item.PayDateDefault)
                        {
                            TdcInstallmentPriceTable lateItem = new TdcInstallmentPriceTable();

                            lateItem.RowStatus = AppEnums.TypePayQD.TRE_HAN;
                            lateItem.PayDateBefore = item.PayDateDefault;

                            lateItem.PayDateGuess = item.PayDateGuess.Value.AddYears(1);
                            lateItem.PayDateReal = item.PayDateReal;
                            //Thơi gian tính lãi theo ngày
                            // Lấy DateTime bên trong kiểu Nullable DateTime
                            DateTime start = lateItem.PayDateBefore.Value;
                            DateTime end = lateItem.PayDateReal.Value;
                            // Lấy TimeSpan giữa hai ngày
                            TimeSpan span = end.Subtract(start);
                            // Lấy số ngày chênh lệch
                            lateItem.DailyInterest = (int)span.TotalDays;
                            lateItem.DailyInterestRate = lateRate;
                            //số tiền gốc phải tính lãi
                            lateItem.TotalPay = lateTotalPay;
                            lateItem.PayAnnual = 0;
                            lateItem.TotalInterest = lateItem.DailyInterest * lateItem.TotalPay * lateItem.DailyInterestRate / 100;
                            interest_dataTotal += lateItem.TotalInterest;
                            //Số tiền đến kì phải thanh toán
                            lateItem.TotalPayAnnual = lateItem.TotalInterest;
                            TotalPay += lateItem.TotalInterest.Value;
                            lateItem.Pay = lateItem.TotalPayAnnual;
                            totalItemPay += lateItem.Pay;
                            lateItem.Paid = 0;
                            lateItem.PriceDifference = lateItem.Pay - lateItem.Paid;
                            TotalDifference += lateItem.PriceDifference.Value;
                            lateItem.PayTimeId = item.PayTimeId;
                            if (k < payData.Count())
                            {
                                lateItem.DataStatus = 1;
                            }
                            else
                            {
                                lateItem.DataStatus = 2;
                            }
                            lateItem.Note = "Phạt trễ hạn";

                            res.Add(lateItem);
                        }
                        if (isPayOff && item.PaymentTimes == lastPayOff)
                        {
                            break;
                        }
                        if (timesAgain == 0 && payData.Count != 0)
                        {
                            payTimeId--;
                            totalItem = new TdcInstallmentPriceTable();
                            totalItem.PayDateDefault = item.PayDateDefault;
                            totalItem.PayDateGuess = item.PayDateGuess;
                            totalItem.PayDateReal = item.PayDateReal;
                            if (k < payData.Count())
                            {
                                totalItem.Paid = payData[k].Value;
                            }

                            totalItem.TotalPayAnnual = TotalPay;
                            totalItem.PriceDifference = TotalDifference;

                            totalItem.PayTimeId = payTimeId;
                            totalItem.RowStatus = AppEnums.TypePayQD.TONG;
                            totalItem.TotalInterest = interest_dataTotal;
                            totalItem.PayAnnual = payAnnual_dataTotal;
                            totalItem.DataStatus = 1;
                            Pricediff = totalItem.PriceDifference;
                            //Pricediff = totalItemPay - totalItem.Paid;
                            res.Add(totalItem);
                            payTimeId--;
                            //Tăng biến chạy của lần trả lên 1
                            k++;
                            payAnnual_dataTotal = 0;
                            TotalPay = 0;
                            TotalDifference = 0;
                            totalItemPay = 0;
                            interest_dataTotal = 0;
                            if (k < payData.Count())
                            {
                                timesAgain = payData[k].PayCount - 1;
                                payStatus = 0;
                                isPaid = 0;
                            }
                            else
                            {
                                timesAgain--;
                                payStatus = 1;
                                isPaid = 1;
                            }
                        }
                        else
                        {
                            timesAgain--;
                            payStatus = 1;

                        }
                    }
                    else
                    {
                        totalDebtAdd = false;
                        i--;
                    }

                }

                var groupDataExcel = res.GroupBy(f => f.PayTimeId != null ? (object)f.PayTimeId : new object(), key => key).Select(f => new TdcInstallmentPriceGroupByPayTimeId
                {
                    PayTimeId = f.Key,
                    tdcInstallmentPriceTables = f.ToList()
                });
                List<TdcInstallmentPriceGroupByPayTimeId> resItem = new List<TdcInstallmentPriceGroupByPayTimeId>();
                foreach (var item in groupDataExcel.ToList())
                {
                    item.Pay = item.tdcInstallmentPriceTables.Sum(x => x.Pay);
                    item.Paid = item.tdcInstallmentPriceTables.Sum(x => x.Paid);
                    item.PriceDifference = item.tdcInstallmentPriceTables.Sum(x => x.PriceDifference);
                    item.DataStatus = item.tdcInstallmentPriceTables[0].DataStatus;
                    item.publicPay = item.tdcInstallmentPriceTables[0].publicPay;
                    resItem.Add(item);
                }

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = resItem;
                return Ok(def);

            }
            catch (Exception ex)
            {
                log.Error("GetWorkSheet Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        private bool checkDecreeChange(DateTime checkValue, DateTime start, DateTime end)
        {
            if (checkValue > start && checkValue < end)
                return true;
            return false;
        }
        #endregion


    }
}

