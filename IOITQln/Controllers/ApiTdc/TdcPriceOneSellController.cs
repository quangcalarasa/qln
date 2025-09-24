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
using IOITQln.Migrations;
using OfficeOpenXml.Style;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Data;
using NPOI.SS.Util;
using System.Reflection.PortableExecutable;
using NPOI.SS.Formula.Functions;
using DevExpress.Data.Helpers;
using NPOI.POIFS.Properties;
using Microsoft.Net.Http.Headers;

namespace IOITQln.Controllers.ApiTdc
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TdcPriceOneSellController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("price-one-time-tdc", "price-one-time-tdc");
        private static string functionCode = "PRICEONETIME_TDC";
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        private IHostingEnvironment _hostingEnvironment;

        public TdcPriceOneSellController(ApiDbContext context, IMapper mapper, IHostingEnvironment hostingEnvironment)
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
                    IQueryable<TdcPriceOneSell> data = _context.TdcPriceOneSells.Where(c => c.Status != AppEnums.EntityStatus.DELETED);
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
                        List<TdcPriceOneSellData> res = _mapper.Map<List<TdcPriceOneSellData>>(data.ToList());
                        foreach (TdcPriceOneSellData item in res)
                        {
                            item.TdcCustomerName = _context.TdcCustomers.Where(f => f.Id == item.TdcCustomerId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.FullName).FirstOrDefault();

                            item.TdcProjectName = _context.TDCProjects.Where(f => f.Id == item.TdcProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            item.TdcLandName = _context.Lands.Where(f => f.Id == item.LandId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            item.TdcBlockHouseName = _context.BlockHouses.Where(f => f.Id == item.BlockHouseId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            item.TdcFloorName = _context.FloorTdcs.Where(f => f.Id == item.FloorTdcId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            item.TdcApartmentName = _context.ApartmentTdcs.Where(f => f.Id == item.TdcApartmentId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            item.TdcPlatformName = _context.ApartmentTdcs.Where(f => f.Id == item.PlatformId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            //Lay du an
                            //TDCProject tDCProject = _context.TDCProjects.Where(f => f.Id == item.TdcProjectId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            List<TDCProjectIngrePrice> tDCProjectIngrePrices = _context.TDCProjectIngrePrices.Where(x => x.TDCProjectId == item.TdcProjectId && x.Status != AppEnums.EntityStatus.DELETED).ToList();
                            //Giá Bán Cấu Thành Chính Thức
                            List<TdcPriceOneSellOfficial> tdcPriceOneSellOfficial = _context.TdcPriceOneSellOfficials.Where(l => l.TdcPriceOneSellId == item.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                            List<TdcPriceOneSellOfficialData> map_tdcPriceOneSellOfficials = _mapper.Map<List<TdcPriceOneSellOfficialData>>(tdcPriceOneSellOfficial.ToList());
                            foreach (TdcPriceOneSellOfficialData map_tdcPriceOneSellOfficial in map_tdcPriceOneSellOfficials)
                            {
                                map_tdcPriceOneSellOfficial.IngrePriceName = _context.IngredientsPrices.Where(l => l.Id == map_tdcPriceOneSellOfficial.IngredientsPriceId && l.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                                map_tdcPriceOneSellOfficial.Value = tDCProjectIngrePrices.Where(x => x.IngredientsPriceId == map_tdcPriceOneSellOfficial.IngredientsPriceId).Select(x => x.Value).FirstOrDefault();
                            }
                            item.tdcPriceOneSellOfficials = map_tdcPriceOneSellOfficials.ToList();

                            // Giá Bán Cấu Thành Tạm Thời
                            List<TdcPriceOneSellTemporary> tdcPriceOneSellTemporarie = _context.TdcPriceOneSellTemporaries.Where(l => l.TdcPriceOneSellId == item.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                            List<TdcPriceOneSellTemporaryData> map_tdcPriceOneSellTemporaries = _mapper.Map<List<TdcPriceOneSellTemporaryData>>(tdcPriceOneSellTemporarie.ToList());
                            foreach (TdcPriceOneSellTemporaryData map_tdcPriceOneSellTemporarie in map_tdcPriceOneSellTemporaries)
                            {
                                map_tdcPriceOneSellTemporarie.IngrePriceName = _context.IngredientsPrices.Where(l => l.Id == map_tdcPriceOneSellTemporarie.IngredientsPriceId && l.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                                map_tdcPriceOneSellTemporarie.Value = tDCProjectIngrePrices.Where(x => x.IngredientsPriceId == map_tdcPriceOneSellTemporarie.IngredientsPriceId).Select(x => x.Value).FirstOrDefault();
                            }
                            item.tdcPriceOneSellTemporaries = map_tdcPriceOneSellTemporaries.ToList();

                            List<TdcPriceOneSellTax> TdcPriceOneSellTaxes = _context.TdcPriceOneSellTaxes.Where(l => l.TdcPriceOneSellId == item.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                            item.tdcPriceOneSellTaxes = TdcPriceOneSellTaxes;

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
                TdcPriceOneSell data = await _context.TdcPriceOneSells.FindAsync(id);
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
        public async Task<IActionResult> Post(TdcPriceOneSellData input)
        {
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
                input = (TdcPriceOneSellData)UtilsService.TrimStringPropertyTypeObject(input);
                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }
                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.CreatedById = userId;
                    input.CreatedBy = fullName;
                    _context.TdcPriceOneSells.Add(input);

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


                        if (input.tdcPriceOneSellOfficials != null)
                        {
                            foreach (var tdcPriceOfficial in input.tdcPriceOneSellOfficials)
                            {
                                tdcPriceOfficial.TdcPriceOneSellId = input.Id;
                                tdcPriceOfficial.CreatedBy = fullName;
                                tdcPriceOfficial.CreatedById = userId;

                                _context.TdcPriceOneSellOfficials.Add(tdcPriceOfficial);
                            }
                        }

                        if (input.tdcPriceOneSellTemporaries != null)
                        {
                            foreach (var tdcPriceOneSellTemporarie in input.tdcPriceOneSellTemporaries)
                            {
                                tdcPriceOneSellTemporarie.TdcPriceOneSellId = input.Id;
                                tdcPriceOneSellTemporarie.CreatedById = userId;
                                tdcPriceOneSellTemporarie.CreatedBy = fullName;

                                _context.TdcPriceOneSellTemporaries.Add(tdcPriceOneSellTemporarie);
                            }
                        }

                        if (input.tdcPriceOneSellTaxes != null)
                        {
                            foreach (var tdcPriceOneSellTaxe in input.tdcPriceOneSellTaxes)
                            {
                                tdcPriceOneSellTaxe.TdcPriceOneSellId = input.Id;
                                tdcPriceOneSellTaxe.CreatedById = userId;
                                tdcPriceOneSellTaxe.CreatedBy = fullName;

                                _context.TdcPriceOneSellTaxes.Add(tdcPriceOneSellTaxe);
                            }
                        }

                        LogActionModel logActionModel = new LogActionModel("Thêm mới hồ sơ", "TdcPriceOneSell", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
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
                        if (TdcPriceOneSellExists(input.Id))
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
        public async Task<IActionResult> Put(int id, [FromBody] TdcPriceOneSellData input)
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
                input = (TdcPriceOneSellData)UtilsService.TrimStringPropertyTypeObject(input);

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
                TdcPriceOneSell data = await _context.TdcPriceOneSells.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
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

                        TdcPriceOneSell tdcPriceOneSell = _context.TdcPriceOneSells.Find(newid);
                        tdcPriceOneSell.Code = code;
                        _context.Update(tdcPriceOneSell);

                        await _context.SaveChangesAsync();

                        List<TdcPriceOneSellTax> lstTdcPriceOneSellTaxAdd = new List<TdcPriceOneSellTax>();
                        List<TdcPriceOneSellTax> lstTdcPriceOneSellTaxUpdate = new List<TdcPriceOneSellTax>();

                        List<TdcPriceOneSellTax> TdcPriceOneSellTaxes = _context.TdcPriceOneSellTaxes.AsNoTracking().Where(l => l.TdcPriceOneSellId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();

                        if (input.tdcPriceOneSellTaxes != null)
                        {
                            foreach (var tdcPriceOneSellTax in input.tdcPriceOneSellTaxes)
                            {
                                TdcPriceOneSellTax tdcPriceOneSellTaxExist = TdcPriceOneSellTaxes.Where(l => l.Id == tdcPriceOneSellTax.Id).FirstOrDefault();
                                if (tdcPriceOneSellTaxExist == null)
                                {
                                    tdcPriceOneSellTax.TdcPriceOneSellId = input.Id;
                                    tdcPriceOneSellTax.CreatedBy = fullName;
                                    tdcPriceOneSellTax.CreatedById = userId;

                                    lstTdcPriceOneSellTaxAdd.Add(tdcPriceOneSellTax);
                                }
                                else
                                {
                                    tdcPriceOneSellTax.CreatedAt = tdcPriceOneSellTax.CreatedAt;
                                    tdcPriceOneSellTax.CreatedBy = tdcPriceOneSellTax.CreatedBy;
                                    tdcPriceOneSellTax.CreatedById = tdcPriceOneSellTax.UpdatedById;
                                    tdcPriceOneSellTax.TdcPriceOneSellId = input.Id;
                                    tdcPriceOneSellTax.UpdatedById = userId;
                                    tdcPriceOneSellTax.UpdatedBy = fullName;

                                    lstTdcPriceOneSellTaxUpdate.Add(tdcPriceOneSellTax);
                                    TdcPriceOneSellTaxes.Remove(tdcPriceOneSellTaxExist);
                                }
                            }
                        }
                        foreach (var itemTdcPriceOneSellTax in TdcPriceOneSellTaxes)
                        {
                            itemTdcPriceOneSellTax.UpdatedAt = DateTime.Now;
                            itemTdcPriceOneSellTax.UpdatedById = userId;
                            itemTdcPriceOneSellTax.UpdatedBy = fullName;
                            itemTdcPriceOneSellTax.Status = AppEnums.EntityStatus.DELETED;

                            lstTdcPriceOneSellTaxUpdate.Add(itemTdcPriceOneSellTax);
                        }
                        _context.TdcPriceOneSellTaxes.UpdateRange(lstTdcPriceOneSellTaxUpdate);
                        _context.TdcPriceOneSellTaxes.AddRange(lstTdcPriceOneSellTaxAdd);
                        //Thành Phần Giá Bán Cấu Thành Tạm Thời
                        List<TdcPriceOneSellTemporary> lstTdcPriceOneSellTemporaryAdd = new List<TdcPriceOneSellTemporary>();
                        List<TdcPriceOneSellTemporary> lstlTdcPriceOneSellTemporaryUpdate = new List<TdcPriceOneSellTemporary>();

                        List<TdcPriceOneSellTemporary> TdcPriceOneSellTemporarys = _context.TdcPriceOneSellTemporaries.AsNoTracking().Where(l => l.TdcPriceOneSellId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();

                        if (input.tdcPriceOneSellTemporaries != null)
                        {
                            foreach (var tdcPriceOneSellTemporarie in input.tdcPriceOneSellTemporaries)
                            {
                                TdcPriceOneSellTemporary tdcPriceOneSellTemporaryExist = TdcPriceOneSellTemporarys.Where(l => l.Id == tdcPriceOneSellTemporarie.Id).FirstOrDefault();
                                if (tdcPriceOneSellTemporaryExist == null)
                                {
                                    tdcPriceOneSellTemporarie.TdcPriceOneSellId = input.Id;
                                    tdcPriceOneSellTemporarie.CreatedBy = fullName;
                                    tdcPriceOneSellTemporarie.CreatedById = userId;

                                    lstTdcPriceOneSellTemporaryAdd.Add(tdcPriceOneSellTemporarie);
                                }
                                else
                                {
                                    tdcPriceOneSellTemporarie.CreatedAt = tdcPriceOneSellTemporarie.CreatedAt;
                                    tdcPriceOneSellTemporarie.CreatedBy = tdcPriceOneSellTemporarie.CreatedBy;
                                    tdcPriceOneSellTemporarie.CreatedById = tdcPriceOneSellTemporarie.UpdatedById;
                                    tdcPriceOneSellTemporarie.TdcPriceOneSellId = input.Id;
                                    tdcPriceOneSellTemporarie.UpdatedById = userId;
                                    tdcPriceOneSellTemporarie.UpdatedBy = fullName;

                                    lstlTdcPriceOneSellTemporaryUpdate.Add(tdcPriceOneSellTemporarie);
                                    TdcPriceOneSellTemporarys.Remove(tdcPriceOneSellTemporaryExist);
                                }
                            }
                        }
                        foreach (var itemTdcPriceOneSellTemporarie in TdcPriceOneSellTemporarys)
                        {
                            itemTdcPriceOneSellTemporarie.UpdatedAt = DateTime.Now;
                            itemTdcPriceOneSellTemporarie.UpdatedById = userId;
                            itemTdcPriceOneSellTemporarie.UpdatedBy = fullName;
                            itemTdcPriceOneSellTemporarie.Status = AppEnums.EntityStatus.DELETED;

                            lstlTdcPriceOneSellTemporaryUpdate.Add(itemTdcPriceOneSellTemporarie);
                        }
                        _context.TdcPriceOneSellTemporaries.UpdateRange(lstlTdcPriceOneSellTemporaryUpdate);
                        _context.TdcPriceOneSellTemporaries.AddRange(lstTdcPriceOneSellTemporaryAdd);

                        //Thành Phần Giá Bán Cấu Thành Chính Thức
                        //List<TdcPriceOneSellOfficial> lstTdcPriceOneSellOfficialAdd = new List<TdcPriceOneSellOfficial>();
                        //List<TdcPriceOneSellOfficial> lstTdcPriceOneSellOfficialUpdate = new List<TdcPriceOneSellOfficial>();

                        //List<TdcPriceOneSellOfficial> TdcPriceOneSellOfficials = _context.TdcPriceOneSellOfficials.AsNoTracking().Where(l => l.TdcPriceOneSellId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();

                        //if (input.tdcPriceOneSellOfficials != null)
                        //{
                        //    foreach (var tdcPriceOneSellOfficial in input.tdcPriceOneSellOfficials)
                        //    {
                        //        TdcPriceOneSellOfficial tdcPriceOneSellOfficialExist = TdcPriceOneSellOfficials.Where(l => l.Id == tdcPriceOneSellOfficial.Id).FirstOrDefault();
                        //        if (tdcPriceOneSellOfficialExist == null)
                        //        {
                        //            tdcPriceOneSellOfficial.TdcPriceOneSellId = input.Id;
                        //            tdcPriceOneSellOfficial.CreatedBy = fullName;
                        //            tdcPriceOneSellOfficial.CreatedById = userId;

                        //            lstTdcPriceOneSellOfficialAdd.Add(tdcPriceOneSellOfficial);
                        //        }
                        //        else
                        //        {
                        //            tdcPriceOneSellOfficial.CreatedAt = tdcPriceOneSellOfficial.CreatedAt;
                        //            tdcPriceOneSellOfficial.CreatedBy = tdcPriceOneSellOfficial.CreatedBy;
                        //            tdcPriceOneSellOfficial.CreatedById = tdcPriceOneSellOfficial.UpdatedById;
                        //            tdcPriceOneSellOfficial.TdcPriceOneSellId = input.Id;
                        //            tdcPriceOneSellOfficial.UpdatedById = userId;
                        //            tdcPriceOneSellOfficial.UpdatedBy = fullName;

                        //            lstTdcPriceOneSellOfficialUpdate.Add(tdcPriceOneSellOfficial);
                        //            TdcPriceOneSellOfficials.Remove(tdcPriceOneSellOfficialExist);
                        //        }
                        //    }
                        //}
                        //foreach (var itemTdcPriceOneSellOfficial in TdcPriceOneSellOfficials)
                        //{
                        //    itemTdcPriceOneSellOfficial.UpdatedAt = DateTime.Now;
                        //    itemTdcPriceOneSellOfficial.UpdatedById = userId;
                        //    itemTdcPriceOneSellOfficial.UpdatedBy = fullName;
                        //    itemTdcPriceOneSellOfficial.Status = AppEnums.EntityStatus.DELETED;

                        //    lstTdcPriceOneSellOfficialUpdate.Add(itemTdcPriceOneSellOfficial);
                        //}
                        //_context.TdcPriceOneSellOfficials.UpdateRange(lstTdcPriceOneSellOfficialUpdate);
                        //_context.TdcPriceOneSellOfficials.AddRange(lstTdcPriceOneSellOfficialAdd);

                        List<TdcPriceOneSellOfficial> tdcPriceOneSellOfficial = _context.TdcPriceOneSellOfficials.Where(l => l.TdcPriceOneSellId == id && l.Status != AppEnums.EntityStatus.DELETED).AsNoTracking().ToList();
                        List<TdcPriceOneSellOfficialData> map_tdcPriceOneSellOfficials = _mapper.Map<List<TdcPriceOneSellOfficialData>>(tdcPriceOneSellOfficial.ToList());
                        foreach (TdcPriceOneSellOfficialData map_tdcPriceOneSellOfficial in map_tdcPriceOneSellOfficials)
                        {
                            map_tdcPriceOneSellOfficial.IngrePriceName = _context.IngredientsPrices.Where(l => l.Id == map_tdcPriceOneSellOfficial.IngredientsPriceId && l.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                        }
                        var lst = map_tdcPriceOneSellOfficials.ToList();

                        if (input.tdcPriceOneSellOfficials != null)
                        {
                            bool check = Check(lst, input.tdcPriceOneSellOfficials);

                            if (check == false)
                            {
                                lst.ForEach(x => {
                                    x.ChangeTimes = lst[0].Id;
                                    x.Status = AppEnums.EntityStatus.DELETED;
                                });
                                _context.UpdateRange(lst);
                                await _context.SaveChangesAsync();
                                foreach (var item in input.tdcPriceOneSellOfficials)
                                {

                                    item.Id = 0;
                                    item.TdcPriceOneSellId = input.Id;
                                    _context.TdcPriceOneSellOfficials.Add(item);
                                }
                                await _context.SaveChangesAsync();
                            }

                        }

                        LogActionModel logActionModel = new LogActionModel("Chỉnh sửa thông tin Hồ Sơ " + input.Code, "TdcPriceOneSell", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, userId, fullName);
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
                        if (!TdcPriceOneSellExists(data.Id))
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
                TdcPriceOneSell data = await _context.TdcPriceOneSells.FindAsync(id);
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
                        LogActionModel logActionModel = new LogActionModel("Xóa Hồ Sơ" + data.Code, "TdcPriceOneSell", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.DELETED, userId, fullName);
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
                        if (!TdcPriceOneSellExists(data.Id))
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

        [HttpPost("GetExTable")]
        public async Task<IActionResult> GetReportTable([FromBody] List<TdcPriceOneSell> input)
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
                List<TdcPriceOneSellEx> resEx = new List<TdcPriceOneSellEx>();
                foreach (var items in input)
                {
                    TdcPriceOneSellEx dataEx = new TdcPriceOneSellEx();

                    TdcPriceOneSell tdcPriceOneSell = _context.TdcPriceOneSells.Where(l => l.Id == items.Id && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                    if (tdcPriceOneSell == null)
                    {
                        def.meta = new Meta(404, "Không tìm thấy hồ sơ tương ứng!");
                        return Ok(def);
                    }

                    TdcPriceOneSellData mapper_dataTdcOne = _mapper.Map<TdcPriceOneSellData>(tdcPriceOneSell);



                    dataEx.oneselltaxes = new List<Oenselltax>();

                    dataEx.temporaryDatas = new List<OneSellIngreData>();

                    dataEx.officialDatas = new List<OneSellIngreData>();

                    dataEx.differenceDatas = new List<OneSellIngreData>();

                    dataEx.priceTaxes = new List<OneSellPriceAndTax>();

                    dataEx.priceTaxTTs = new List<OneSellPriceAndTaxTT>();

                    dataEx.priceTaxCLs = new List<OneSellPriceAndTaxCL>();

                    decimal TotalTT = 0;
                    decimal TotalCT = 0;
                    decimal TotalCL = 0;
                    decimal TotalPatTt = 0; // Do not show tam thoi;
                    decimal TotalPatCt = 0; // Do not show chinh thuc
                    decimal TotalPatCl = 0; // Do not show chech lech

                    List<TDCProjectIngrePrice> tDCProjectIngrePrices = _context.TDCProjectIngrePrices.Where(x => x.TDCProjectId == tdcPriceOneSell.TdcProjectId && x.Status != AppEnums.EntityStatus.DELETED).ToList();

                    List<TdcPriceOneSellTemporary> tdcPriceOneSellTemporaries = _context.TdcPriceOneSellTemporaries.Where(l => l.TdcPriceOneSellId == tdcPriceOneSell.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    List<TdcPriceOneSellTemporaryData> map_temporary = _mapper.Map<List<TdcPriceOneSellTemporaryData>>(tdcPriceOneSellTemporaries.ToList());
                    foreach (TdcPriceOneSellTemporaryData i in map_temporary)
                    {
                        i.IngrePriceName = _context.IngredientsPrices.Where(f => f.Id == i.IngredientsPriceId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                        i.Value = tDCProjectIngrePrices.Where(x => x.IngredientsPriceId == i.IngredientsPriceId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Value).FirstOrDefault();

                        var itemTemp = new OneSellIngreData();

                        itemTemp.Id = i.IngredientsPriceId;
                        itemTemp.Name = i.IngrePriceName;
                        itemTemp.Area = i.Area;
                        itemTemp.UnitPrice = i.Price;
                        itemTemp.Price = i.Total;
                        itemTemp.Value = i.Value;

                        dataEx.temporaryDatas.Add(itemTemp);
                        if(i.Value!= 0)
                        {
                            TotalTT += Math.Round((i.Total / (decimal)i.Value));
                        }
                    }
                    mapper_dataTdcOne.tdcPriceOneSellTemporaries = map_temporary;

                    List<TdcPriceOneSellOfficial> tdcPriceOneSellOfficials = _context.TdcPriceOneSellOfficials.Where(l => l.TdcPriceOneSellId == tdcPriceOneSell.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    List<TdcPriceOneSellOfficialData> map_official = _mapper.Map<List<TdcPriceOneSellOfficialData>>(tdcPriceOneSellOfficials.ToList());
                    foreach (TdcPriceOneSellOfficialData i in map_official)
                    {
                        i.IngrePriceName = _context.IngredientsPrices.Where(f => f.Id == i.IngredientsPriceId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                        i.Value = tDCProjectIngrePrices.Where(x => x.IngredientsPriceId == i.IngredientsPriceId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Value).FirstOrDefault();

                        var itemOffi = new OneSellIngreData();

                        itemOffi.Id = i.IngredientsPriceId;
                        itemOffi.Name = i.IngrePriceName;
                        itemOffi.Area = i.Area;
                        itemOffi.UnitPrice = i.Price;
                        itemOffi.Price = i.Total;
                        itemOffi.Value = i.Value;

                        dataEx.officialDatas.Add(itemOffi);
                        if (i.Value != 0)
                        {
                            TotalCT += Math.Round((i.Total / (decimal)i.Value));
                        }
                    }
                    mapper_dataTdcOne.tdcPriceOneSellOfficials = map_official;

                    //Thuế phí nông nghiệp
                    List<TdcPriceOneSellTax> tdcPriceOneSellTaxes = _context.TdcPriceOneSellTaxes.Where(l => l.TdcPriceOneSellId == tdcPriceOneSell.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    foreach (TdcPriceOneSellTax i in tdcPriceOneSellTaxes)
                    {
                        var itemTax = new Oenselltax();
                        itemTax.Year = i.Year;
                        itemTax.Total = i.Total;

                        dataEx.oneselltaxes.Add(itemTax);
                    }

                    List<TDCProjectPriceAndTax> tDCProjectPriceAndTaxs = _context.TDCProjectPriceAndTaxs.Where(l => l.TDCProjectId == tdcPriceOneSell.TdcProjectId && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    List<OneSellIngreData> ingreDataDetails = new List<OneSellIngreData>();
                    List<TDCProjectPriceAndTaxData> map_TDCProjectPriceAndTaxs = _mapper.Map<List<TDCProjectPriceAndTaxData>>(tDCProjectPriceAndTaxs);
                    foreach (TDCProjectPriceAndTaxData map_TDCProjectPriceAndTax in map_TDCProjectPriceAndTaxs)
                    {
                        List<TDCProjectPriceAndTaxDetails> de = _context.TDCProjectPriceAndTaxDetailss.Where(f => f.PriceAndTaxId == map_TDCProjectPriceAndTax.Id && f.Status != AppEnums.EntityStatus.DELETED).ToList();
                        List<TDCProjectPriceAndTaxDetailData> detail = _mapper.Map<List<TDCProjectPriceAndTaxDetailData>>(de);
                        OriginalPriceAndTax pat = _context.OriginalPriceAndTaxs.Where(f => f.Id == map_TDCProjectPriceAndTax.PriceAndTaxId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                        if (de == null) map_TDCProjectPriceAndTax.PATName = "";
                        else map_TDCProjectPriceAndTax.PATName = pat.Name;

                        //Tính giá tạm thời
                        var itemPriceATaxTT = new OneSellPriceAndTaxTT();
                        itemPriceATaxTT.Name = pat.Name;
                        itemPriceATaxTT.Value = map_TDCProjectPriceAndTax.Value;
                        itemPriceATaxTT.Location = map_TDCProjectPriceAndTax.Location;
                        itemPriceATaxTT.TotalTt = 0;
                        itemPriceATaxTT.temporaryDatas = new List<OneSellIngreData>();
                        TotalPatTt = 0;
                        foreach (TDCProjectPriceAndTaxDetailData i in detail)
                        {
                            OneSellIngreData newItem = new OneSellIngreData();
                            IngredientsPrice result = _context.IngredientsPrices.Where(f => f.Id == i.IngredientsPriceId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            newItem = dataEx.temporaryDatas.Where(x => x.Id == i.IngredientsPriceId).FirstOrDefault();

                            if (de == null) i.IngrePriceName = "";
                            else i.IngrePriceName = result.Name;
                            itemPriceATaxTT.temporaryDatas.Add(newItem);
                            if (newItem.Value != 0)
                            {
                                TotalPatTt += Math.Round((newItem.Price / (decimal)newItem.Value));
                            }
                        }

                        if (itemPriceATaxTT.Name == "VAT")
                        {
                            if (tdcPriceOneSell.TotalPriceTT == 0)
                            {
                                itemPriceATaxTT.TotalTt = 0;
                            }
                            else
                            {
                                itemPriceATaxTT.TotalTt = Math.Round(((TotalTT / tdcPriceOneSell.TotalPriceTT) * 100) * (decimal)itemPriceATaxTT.Value / 100);
                            }
                        }
                        else
                        {
                            if (tdcPriceOneSell.TotalPriceTT == 0)
                            {
                                itemPriceATaxTT.TotalTt = 0;
                            }
                            else
                            {
                                itemPriceATaxTT.TotalTt = Math.Round((TotalPatTt * (decimal)itemPriceATaxTT.Value) / tdcPriceOneSell.TotalPriceTT);
                            }
                        }
                        if (itemPriceATaxTT.Name != "VAT")
                        {
                            if (tdcPriceOneSell.TotalPriceTT == 0)
                            {
                                itemPriceATaxTT.Price = 0;
                            }
                            else
                            {
                                itemPriceATaxTT.Price = Math.Round(((TotalPatTt * (decimal)itemPriceATaxTT.Value) / tdcPriceOneSell.TotalPriceTT) * tdcPriceOneSell.TotalPriceTT) / 100;
                            }

                        }
                        map_TDCProjectPriceAndTax.TDCProjectPriceAndTaxDetails = detail;
                        dataEx.priceTaxTTs.Add(itemPriceATaxTT);

                        //Tính giá chính thức 
                        var itemPriceATax = new OneSellPriceAndTax();
                        itemPriceATax.Name = pat.Name;
                        itemPriceATax.Value = map_TDCProjectPriceAndTax.Value;
                        itemPriceATax.Location = map_TDCProjectPriceAndTax.Location;
                        itemPriceATax.TotalCt = 0;
                        itemPriceATax.Price = 0;
                        itemPriceATax.datas = new List<OneSellIngreData>();
                        TotalPatCt = 0;
                        foreach (TDCProjectPriceAndTaxDetailData i in detail)
                        {
                            OneSellIngreData newItem = new OneSellIngreData();
                            IngredientsPrice result = _context.IngredientsPrices.Where(f => f.Id == i.IngredientsPriceId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            newItem = dataEx.officialDatas.Where(x => x.Id == i.IngredientsPriceId).FirstOrDefault();

                            if (de == null) i.IngrePriceName = "";
                            else i.IngrePriceName = result.Name;
                            itemPriceATax.datas.Add(newItem);
                            if (newItem.Value != 0)
                            {
                                TotalPatCt += Math.Round((newItem.Price / (decimal)newItem.Value));
                            }
                        }

                        if (itemPriceATax.Name == "VAT")
                        {
                            itemPriceATax.TotalCt = Math.Round(((TotalCT / tdcPriceOneSell.TotalPriceCT) * 100) * (decimal)itemPriceATax.Value / 100);
                        }
                        else
                        {
                            itemPriceATax.TotalCt = Math.Round((TotalPatCt * (decimal)itemPriceATax.Value) / tdcPriceOneSell.TotalPriceCT);
                        }

                        if (itemPriceATax.Name != "VAT")
                        {
                            itemPriceATax.Price = Math.Round((((TotalPatCt * (decimal)itemPriceATax.Value) / tdcPriceOneSell.TotalPriceCT) * tdcPriceOneSell.TotalPriceCT) / 100);
                        }
                        map_TDCProjectPriceAndTax.TDCProjectPriceAndTaxDetails = detail;
                        dataEx.priceTaxes.Add(itemPriceATax);

                        //Tính giá chênh lệch
                        var itemPriceATaxCL = new OneSellPriceAndTaxCL();
                        itemPriceATaxCL.Name = pat.Name;
                        itemPriceATaxCL.Value = map_TDCProjectPriceAndTax.Value;
                        itemPriceATaxCL.TotalCl = 0;
                        itemPriceATaxCL.Price = 0;
                        TotalPatCl = 0;

                        itemPriceATaxCL.differenceDatas = new List<OneSellIngreData>();

                        foreach (TDCProjectPriceAndTaxDetailData i in detail)
                        {
                            OneSellIngreData newItem = dataEx.officialDatas.FirstOrDefault(x => x.Id == i.IngredientsPriceId);
                            //IngredientsPrice result = _context.IngredientsPrices.Where(f => f.Id == i.IngredientsPriceId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            if (newItem != null)
                            {
                                itemPriceATaxCL.differenceDatas.Add(newItem);

                                TotalPatCl += Math.Round(TotalPatCt - TotalPatTt);
                            }
                        }
                        if (itemPriceATaxCL.Name == "VAT")
                        {
                            itemPriceATaxCL.TotalCl = Math.Round(itemPriceATax.TotalCt - itemPriceATaxTT.TotalTt);
                        }
                        if (itemPriceATaxCL.Name != "VAT")
                        {
                            itemPriceATaxCL.Price = Math.Round(itemPriceATax.Price - itemPriceATaxTT.Price);
                        }
                        dataEx.priceTaxCLs.Add(itemPriceATaxCL);

                    }

                    dataEx.Code = tdcPriceOneSell.Code;

                    dataEx.ContractDate = tdcPriceOneSell.Date;

                    dataEx.CustomerName = _context.TdcCustomers.Where(f => f.Id == mapper_dataTdcOne.TdcCustomerId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.FullName).FirstOrDefault();

                    dataEx.ProjectName = _context.TDCProjects.Where(f => f.Id == mapper_dataTdcOne.TdcProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                    dataEx.LandName = _context.Lands.Where(f => f.Id == mapper_dataTdcOne.LandId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                    dataEx.BlockHouseName = _context.BlockHouses.Where(f => f.Id == mapper_dataTdcOne.BlockHouseId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                    dataEx.FloorName = _context.FloorTdcs.Where(f => f.Id == mapper_dataTdcOne.FloorTdcId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                    dataEx.ApartmentName = _context.ApartmentTdcs.Where(f => f.Id == mapper_dataTdcOne.TdcApartmentId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                    dataEx.PlatformName = _context.PlatformTdcs.Where(f => f.Id == mapper_dataTdcOne.PlatformId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                    dataEx.Floor1 = tdcPriceOneSell.Floor1;

                    dataEx.Corner = _context.ApartmentTdcs.Where(f => f.Id == mapper_dataTdcOne.TdcApartmentId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Corner).FirstOrDefault();

                    dataEx.TotalAreaTT = tdcPriceOneSell.TotalAreaTT;

                    dataEx.TotalPirceTT = tdcPriceOneSell.TotalPriceTT;

                    dataEx.DecisionNumberTT = tdcPriceOneSell.DecisionNumberTT;

                    dataEx.DecisionDateTT = tdcPriceOneSell.DecisionDateTT;

                    dataEx.TotalAreaCT = tdcPriceOneSell.TotalAreaCT;

                    dataEx.TotalPirceCT = tdcPriceOneSell.TotalPriceCT; // số tiền phải nộp chính thức

                    dataEx.TotalPirceTT =tdcPriceOneSell.TotalPriceTT; // số tiền phải nộp tạm thời

                    dataEx.TotalPirceCL = dataEx.TotalPirceCT - dataEx.TotalPirceTT;// số tiền phải nộp chênh lệch

                    dataEx.DecisionNumberCT = tdcPriceOneSell.DecisionNumberCT;

                    dataEx.DecisionDateCT = tdcPriceOneSell.DecisionDateCT;

                    dataEx.FistPayment = tdcPriceOneSell.Date;

                    dataEx.PersonalTax = tdcPriceOneSell.PersonalTax;

                    dataEx.RegistrationTax = tdcPriceOneSell.RegistrationTax;

                    dataEx.AmountPayment = tdcPriceOneSell.TotalPriceCT;

                    dataEx.AmountPaymentTT = tdcPriceOneSell.TotalPriceTT;

                    dataEx.PaymentPublic = tdcPriceOneSell.PaymentPublic;

                    dataEx.PaymentCenter = dataEx.TotalPirceCT - dataEx.PaymentPublic;

                    //tính tiền gốc tạm thời
                    if (tdcPriceOneSell.TotalPriceTT == 0)
                    {
                        dataEx.MoneyPrincipalTT = 0;
                    }
                    else
                    {
                        dataEx.MoneyPrincipalTT = Math.Round((TotalTT / tdcPriceOneSell.TotalPriceTT) * 100);
                    }

                    //tính tiền gốc chính thức
                    dataEx.MoneyPrincipalCT = Math.Round((TotalCT / tdcPriceOneSell.TotalPriceCT) * 100);

                    //tính tiền gốc chênh lệch
                    dataEx.MoneyPrincipalCL = Math.Round(dataEx.MoneyPrincipalCT - dataEx.MoneyPrincipalTT);

                    //tiền gốc đã nộp chính thức
                    dataEx.PrincipalPaid = Math.Round((dataEx.AmountPayment * dataEx.MoneyPrincipalCT) / 100);

                    //tiền gốc đã nộp tạm thời
                    dataEx.PrincipalPaidTT = Math.Round((dataEx.AmountPaymentTT * dataEx.MoneyPrincipalTT) / 100);

                    //tiền gốc đã nộp chênh lệch
                    dataEx.PrincipalPaidCL = Math.Round(dataEx.PrincipalPaid - dataEx.PrincipalPaidTT);

                    decimal Sumpersent = 0;

                    foreach (var q in dataEx.priceTaxes)
                    {

                        Sumpersent = Math.Round(dataEx.priceTaxes.Sum(x => x.Price));
                    }

                    decimal SumpersentTT = 0;

                    foreach (var q in dataEx.priceTaxTTs)
                    {

                        SumpersentTT = Math.Round(dataEx.priceTaxTTs.Sum(x => x.Price));
                    }


                    dataEx.Vat = Math.Round(dataEx.AmountPayment - (dataEx.PrincipalPaid + Sumpersent));
                    dataEx.VatTT = Math.Round(dataEx.AmountPaymentTT - (dataEx.PrincipalPaidTT + SumpersentTT));
                    dataEx.VatCL = Math.Round(dataEx.Vat - dataEx.VatTT);


                    resEx.Add(dataEx);
                }
                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = resEx;

                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("GetExcelTable Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);

                return Ok(def);
            }
        }

        [HttpPost("ExportExcel")]
        public async Task<IActionResult> ExportExcel([FromBody] List<TdcPriceOneSellEx> input)
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

        private static MemoryStream WriteDataToExcel(string templatePath, int sheetnumber, List<TdcPriceOneSellEx> data)
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
                    foreach (var item in data)
                    {
                        datacol = 29 + ((item.temporaryDatas != null ? item.temporaryDatas.Count : 0) * 3) + ((item.officialDatas != null ? item.officialDatas.Count : 0) * 3) + ((item.priceTaxTTs != null ? item.priceTaxTTs.Count : 0)) + ((item.priceTaxes != null ? item.priceTaxes.Count : 0)) + ((item.priceTaxCLs != null ? item.priceTaxCLs.Count : 0)) + ((item.oneselltaxes != null ? item.oneselltaxes.Count : 0) * 2);

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

                        ICell cellHeader2 = rowHeaders.CreateCell(3);
                        cellHeader2.SetCellValue("thông tin căn hộ");
                        cellHeader2.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader2 = new CellRangeAddress(rowHeader, rowHeader, 3, 8);
                        sheet.AddMergedRegion(mergedRegioncellHeader2);

                        int temporaryDataCount = 9;

                        foreach (var i in item.temporaryDatas)
                        {
                            ICell cellHeader3 = rowHeaders.CreateCell(temporaryDataCount);
                            cellHeader3.SetCellValue(i.Name);
                            cellHeader3.CellStyle = cellStyle;
                            CellRangeAddress mergedRegioncellHeader3 = new CellRangeAddress(rowHeader, rowHeader, temporaryDataCount, temporaryDataCount + 2);
                            sheet.AddMergedRegion(mergedRegioncellHeader3);

                            temporaryDataCount = temporaryDataCount + 3;
                        }

                        ICell cellHeader4 = rowHeaders.CreateCell(temporaryDataCount);
                        cellHeader4.SetCellValue("Giá bán(tạm)");
                        cellHeader4.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader4 = new CellRangeAddress(rowHeader, rowHeader, temporaryDataCount, temporaryDataCount + 1);
                        sheet.AddMergedRegion(mergedRegioncellHeader4);


                        ICell cellHeader5 = rowHeaders.CreateCell(temporaryDataCount + 2);
                        cellHeader5.SetCellValue("QĐ bố trí tạm(tạm)");
                        cellHeader5.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader5 = new CellRangeAddress(rowHeader, rowHeader, temporaryDataCount + 2, temporaryDataCount + 4);
                        sheet.AddMergedRegion(mergedRegioncellHeader5);

                        int officialDataCount = temporaryDataCount + 5;

                        foreach (var x in item.officialDatas)
                        {
                            ICell cellHeader6 = rowHeaders.CreateCell(officialDataCount);
                            cellHeader6.SetCellValue(x.Name);
                            cellHeader6.CellStyle = cellStyle;
                            CellRangeAddress mergedRegioncellHeader6 = new CellRangeAddress(rowHeader, rowHeader, officialDataCount, officialDataCount + 2);
                            sheet.AddMergedRegion(mergedRegioncellHeader6);

                            officialDataCount = officialDataCount + 3;
                        }

                        ICell cellHeader7 = rowHeaders.CreateCell(officialDataCount);
                        cellHeader7.SetCellValue("Giá bán(chính thức)");
                        cellHeader7.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader7 = new CellRangeAddress(rowHeader, rowHeader, officialDataCount, officialDataCount + 1);
                        sheet.AddMergedRegion(mergedRegioncellHeader7);

                        ICell cellHeader8 = rowHeaders.CreateCell(officialDataCount + 2);
                        cellHeader8.SetCellValue("QĐ bố trí tạm(Chính thức)");
                        cellHeader8.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader8 = new CellRangeAddress(rowHeader, rowHeader, officialDataCount + 2, officialDataCount + 3);
                        sheet.AddMergedRegion(mergedRegioncellHeader8);

                        ICell cellHeader9 = rowHeaders.CreateCell(officialDataCount + 4);
                        cellHeader9.SetCellValue("Tiền nộp về");
                        cellHeader9.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader9 = new CellRangeAddress(rowHeader, rowHeader, officialDataCount + 4, officialDataCount + 5);
                        sheet.AddMergedRegion(mergedRegioncellHeader9);


                        int countTT = officialDataCount + 6;
                        int countTT1 = item.priceTaxTTs.Count + 1 + countTT;

                        ICell cellHeader10 = rowHeaders.CreateCell(countTT);
                        cellHeader10.SetCellValue("Số tiền đã nộp (tạm thời)");
                        cellHeader10.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader10 = new CellRangeAddress(rowHeader, rowHeader, countTT, countTT1);
                        sheet.AddMergedRegion(mergedRegioncellHeader10);

                        int countCT = countTT1 + 1;
                        int countCT1 = item.priceTaxes.Count + 1 + countCT;

                        ICell cellHeader1CT = rowHeaders.CreateCell(countCT);
                        cellHeader1CT.SetCellValue("Số tiền đã nộp (chính thức)");
                        cellHeader1CT.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader1CT = new CellRangeAddress(rowHeader, rowHeader, countCT, countCT1);
                        sheet.AddMergedRegion(mergedRegioncellHeader1CT);

                        int countCL = countCT1 + 1;
                        int countCL1 = item.priceTaxCLs.Count + 1 + countCL;

                        ICell cellHeader1CL = rowHeaders.CreateCell(countCL);
                        cellHeader1CL.SetCellValue("Số tiền đã nộp (chênh lệch)");
                        cellHeader1CL.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader1CL = new CellRangeAddress(rowHeader, rowHeader, countCL, countCL1);
                        sheet.AddMergedRegion(mergedRegioncellHeader1CL);

                        ICell cellHeader11 = rowHeaders.CreateCell(countCL1 + 1);
                        cellHeader11.SetCellValue("Thuế");
                        cellHeader11.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader11 = new CellRangeAddress(rowHeader, rowHeader, countCL1 + 1, countCL1 + 2);
                        sheet.AddMergedRegion(mergedRegioncellHeader11);

                        int counttax = countCL1 + 3;
                        int counttax1 = item.oneselltaxes.Count * 2 + counttax;

                        ICell cellHeader12 = rowHeaders.CreateCell(counttax);
                        cellHeader12.SetCellValue("Thuế phí nông nghiệp");
                        cellHeader12.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader12 = new CellRangeAddress(rowHeader, rowHeader, counttax, counttax1 - 1);
                        sheet.AddMergedRegion(mergedRegioncellHeader12);


                        int priceTaxCount = officialDataCount + 4;

                        //Hàng tiêu đề thứ 2
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
                        foreach (var i in item.temporaryDatas)
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

                        ICell cellHeaders25 = rowHeaders1.CreateCell(childOffi + 4);
                        cellHeaders25.SetCellValue("TIền nộp về trung tâm");
                        cellHeaders25.CellStyle = cellStyle;

                        ICell cellHeaders26 = rowHeaders1.CreateCell(childOffi + 5);
                        cellHeaders26.SetCellValue("TIền nộp về công ích");
                        cellHeaders26.CellStyle = cellStyle;

                        ICell cellHeaders27 = rowHeaders1.CreateCell(childOffi + 6);
                        cellHeaders27.SetCellValue("Số tiền phải nộp (tạm thời)");
                        cellHeaders27.CellStyle = cellStyle;

                        ICell cellHeaders28 = rowHeaders1.CreateCell(childOffi + 7);
                        cellHeaders28.SetCellValue("TIền gốc (tạm thời)");
                        cellHeaders28.CellStyle = cellStyle;

                        ICell cellHeaders29 = rowHeaders1.CreateCell(childOffi + 8);
                        cellHeaders29.SetCellValue("VAT");
                        cellHeaders29.CellStyle = cellStyle;

                        int tempItem = childOffi + 9;

                        foreach (var x in item.priceTaxTTs)
                        {
                            if (x.Name != "VAT")
                            {
                                ICell cellHeader30 = rowHeaders1.CreateCell(tempItem);
                                cellHeader30.SetCellValue(x.Name);
                                cellHeader30.CellStyle = cellStyle;
                                tempItem++;
                            }
                        }

                        ICell cellHeaders31 = rowHeaders1.CreateCell(tempItem);
                        cellHeaders31.SetCellValue("Số tiền phải nộp (chính thức)");
                        cellHeaders31.CellStyle = cellStyle;

                        ICell cellHeaders32 = rowHeaders1.CreateCell(tempItem + 1);
                        cellHeaders32.SetCellValue("TIền gốc (chính thức)");
                        cellHeaders32.CellStyle = cellStyle;

                        ICell cellHeaders33 = rowHeaders1.CreateCell(tempItem + 2);
                        cellHeaders33.SetCellValue("VAT");
                        cellHeaders33.CellStyle = cellStyle;

                        int offiItem = tempItem + 3;

                        foreach (var x in item.priceTaxes)
                        {
                            if (x.Name != "VAT")
                            {
                                ICell cellHeader34 = rowHeaders1.CreateCell(offiItem);
                                cellHeader34.SetCellValue(x.Name);
                                cellHeader34.CellStyle = cellStyle;
                                offiItem++;
                            }
                        }

                        int differItem = offiItem ;

                        ICell cellHeaders35 = rowHeaders1.CreateCell(differItem);
                        cellHeaders35.SetCellValue("Số tiền phải nộp (chênh lệch)");
                        cellHeaders35.CellStyle = cellStyle;

                        ICell cellHeaders36 = rowHeaders1.CreateCell(differItem + 1);
                        cellHeaders36.SetCellValue("TIền gốc (chênh lệch)");
                        cellHeaders36.CellStyle = cellStyle;

                        ICell cellHeaders37 = rowHeaders1.CreateCell(differItem + 2);
                        cellHeaders37.SetCellValue("VAT");
                        cellHeaders37.CellStyle = cellStyle;

                        int childItem = differItem + 3;

                        foreach (var x in item.priceTaxCLs)
                        {
                            if (x.Name != "VAT")
                            {
                                ICell cellHeader38 = rowHeaders1.CreateCell(childItem);
                                cellHeader38.SetCellValue(x.Name);
                                cellHeader38.CellStyle = cellStyle;
                                childItem++;
                            }
                        }

                        ICell cellHeaders39 = rowHeaders1.CreateCell(childItem);
                        cellHeaders39.SetCellValue("Thuế thu nhập cá nhân");
                        cellHeaders39.CellStyle = cellStyle;

                        ICell cellHeaders40 = rowHeaders1.CreateCell(childItem + 1);
                        cellHeaders40.SetCellValue("Thuế trước bạ");
                        cellHeaders40.CellStyle = cellStyle;

                        int taxItem = childItem + 2;

                        foreach (var x in item.oneselltaxes)
                        {
                            ICell cellHeaders41 = rowHeaders1.CreateCell(taxItem);
                            cellHeaders41.SetCellValue("Năm");
                            cellHeaders41.CellStyle = cellStyle;
                            taxItem++;

                            ICell cellHeaders42 = rowHeaders1.CreateCell(taxItem);
                            cellHeaders42.SetCellValue("Giá tiền");
                            cellHeaders42.CellStyle = cellStyle;
                            taxItem++;
                        }


                        int temporaryDatas = 8;
                        int temporaryDatasEnd = 8;
                        int priceAndTaxTTs = -10;
                        int officialDatas = -12;
                        int priceAndTaxesTT =-13;
                        int priceAndTaxes = -14;
                        int priceAndTaxesCL = -15;
                        int tax = -16;
                        int taxsItem = 19;

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

                        ////hàng ghi giá trị
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
                                cell.SetCellValue(item.Floor1);
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
                                foreach (var childTT in item.temporaryDatas)
                                {
                                    ICell cellArea = row.CreateCell(i);
                                    cellArea.SetCellValue((double)childTT.Area);
                                    cellArea.CellStyle = cellStyle;
                                    i++;

                                    ICell cellUnitPrice = row.CreateCell(i);
                                    cellUnitPrice.SetCellValue(((double)childTT.UnitPrice));
                                    cellUnitPrice.CellStyle = cellStyleMoney;
                                    i++;

                                    ICell cellPrice = row.CreateCell(i);
                                    cellPrice.SetCellValue(((double)childTT.Price));
                                    cellPrice.CellStyle = cellStyleMoney;
                                    i++;
                                }
                                temporaryDatasEnd = i;
                                i--;
                            }
                            else if (i == temporaryDatasEnd)
                            {
                                cell.SetCellValue((double)item.TotalAreaTT);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == temporaryDatasEnd + 1)
                            {
                                cell.SetCellValue((double)item.TotalPirceTT);
                                cell.CellStyle = cellStyleMoney;
                                priceAndTaxTTs = i;
                            }
                            else if (i == priceAndTaxTTs + 1)
                            {
								//Qfix 
                                cell.SetCellValue(item.DecisionNumberTT);
								//cell.SetCellValue((double)item.DecisionNumberTT);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == priceAndTaxTTs + 2)
                            {
                                cell.SetCellValue((DateTime)item.DecisionDateTT);
                                cell.CellStyle = cellStyleDate;
                            }
                            else if (i == priceAndTaxTTs + 3)
                            {
                                cell.SetCellValue((DateTime)item.FistPayment);
                                cell.CellStyle = cellStyleDate;

                                foreach (var childCT in item.officialDatas)
                                {
                                    i++;
                                    ICell cellArea = row.CreateCell(i);
                                    cellArea.SetCellValue((double)childCT.Area);
                                    cellArea.CellStyle = cellStyle;

                                    i++;
                                    ICell cellUnitPrice = row.CreateCell(i);
                                    cellUnitPrice.SetCellValue(((double)childCT.UnitPrice));
                                    cellUnitPrice.CellStyle = cellStyleMoney;

                                    i++;
                                    ICell cellPrice = row.CreateCell(i);
                                    cellPrice.SetCellValue(((double)childCT.Price));
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
                                priceAndTaxesTT = i;
                            }
                            else if (i == priceAndTaxesTT + 1)
                            {
                                cell.SetCellValue((double)item.DecisionNumberCT);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == priceAndTaxesTT + 2)
                            {
                                cell.SetCellValue((DateTime)item.DecisionDateCT);
                                cell.CellStyle = cellStyleDate;
                            }
                            else if (i == priceAndTaxesTT + 3)
                            {
                                cell.SetCellValue((double)item.PaymentCenter);
                                cell.CellStyle = cellStyleMoney;
                            }
                            else if (i == priceAndTaxesTT + 4)
                            {
                                cell.SetCellValue((double)item.PaymentPublic);
                                cell.CellStyle = cellStyleMoney;
                            }
                            else if (i == priceAndTaxesTT + 5)
                            {
                                cell.SetCellValue((double)item.AmountPaymentTT);
                                cell.CellStyle = cellStyleMoney;
                            }
                            else if (i == priceAndTaxesTT + 6)
                            {
                                cell.SetCellValue((double)item.PrincipalPaidTT);
                                cell.CellStyle = cellStyleMoney;
                            }
                            else if (i == priceAndTaxesTT + 7)
                            {
                                cell.SetCellValue((double)item.VatTT);
                                cell.CellStyle = cellStyleMoney;
                                foreach (var itemchild in item.priceTaxTTs)
                                {
                                    if (itemchild.Name != "VAT")
                                    {
                                        i++;
                                        ICell cellPrices = row.CreateCell(i);
                                        cellPrices.SetCellValue((double)itemchild.Price);
                                        cellPrices.CellStyle = cellStyleMoney;
                                    }
                                }
                                priceAndTaxes = i;
                            }
                            else if (i == priceAndTaxes + 1)
                            {
                                cell.SetCellValue((double)item.AmountPayment);
                                cell.CellStyle = cellStyleMoney;
                            }
                            else if (i == priceAndTaxes + 2)
                            {
                                cell.SetCellValue((double)item.PrincipalPaid);
                                cell.CellStyle = cellStyleMoney;
                            }
                            else if (i == priceAndTaxes + 3)
                            {
                                cell.SetCellValue((double)item.Vat);
                                cell.CellStyle = cellStyleMoney;
                                foreach (var itemchild in item.priceTaxes)
                                {
                                    if (itemchild.Name != "VAT")
                                    {
                                        i++;
                                        ICell cellPrices = row.CreateCell(i);
                                        cellPrices.SetCellValue((double)itemchild.Price);
                                        cellPrices.CellStyle = cellStyleMoney;
                                    }
                                }
                                priceAndTaxesCL = i;
                            }
                            else if (i == priceAndTaxesCL + 1)
                            {
                                cell.SetCellValue((double)item.AmountPaymentCL);
                                cell.CellStyle = cellStyleMoney;
                            }
                            else if (i == priceAndTaxesCL + 2)
                            {
                                cell.SetCellValue((double)item.PrincipalPaidCL);
                                cell.CellStyle = cellStyleMoney;
                            }
                            else if (i == priceAndTaxesCL + 3)
                            {
                                cell.SetCellValue((double)item.VatCL);
                                cell.CellStyle = cellStyleMoney;
                                foreach (var itemchild in item.priceTaxCLs)
                                {
                                    if (itemchild.Name != "VAT")
                                    {
                                        i++;
                                        ICell cellPrices = row.CreateCell(i);
                                        cellPrices.SetCellValue((double)itemchild.Price);
                                        cellPrices.CellStyle = cellStyleMoney;
                                    }
                                }
                                tax = i;
                            }

                            else if (i == tax + 1)
                            {
                                cell.SetCellValue((double)item.PersonalTax);
                                cell.CellStyle = cellStyleMoney;
                            }
                            else if (i == tax + 2)
                            {
                                cell.SetCellValue((double)item.RegistrationTax);
                                cell.CellStyle = cellStyleMoney;
                                taxsItem = i;
                            }
                            else if (i == taxsItem + 1)
                            {
                                foreach (var itemTax in item.oneselltaxes)
                                {

                                    ICell cellYear = row.CreateCell(i);
                                    cellYear.SetCellValue(itemTax.Year);
                                    cellYear.CellStyle = cellStyle;
                                    i++;


                                    ICell cellTotal = row.CreateCell(i);
                                    cellTotal.SetCellValue(((double)itemTax.Total));
                                    cellTotal.CellStyle = cellStyleMoney;
                                    i++;
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

        [HttpGet("getLogTdcOneSellOff/{Id}")]
        public async Task<IActionResult> getLogTdcOneSellOff(int id)
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
                TdcPriceOneSell lst = _context.TdcPriceOneSells.Where(l => l.Id == id && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                List<TdcPriceOneSellOfficial> tdcPriceOneSellOfficial = _context.TdcPriceOneSellOfficials.Where(l => l.TdcPriceOneSellId == lst.Id && l.Status == AppEnums.EntityStatus.DELETED).ToList();
                List<TdcPriceOneSellOfficialData> map_tdcPriceOneSellOfficials = _mapper.Map<List<TdcPriceOneSellOfficialData>>(tdcPriceOneSellOfficial.ToList());
                foreach (TdcPriceOneSellOfficialData map_tdcPriceOneSellOfficial in map_tdcPriceOneSellOfficials)
                {
                    map_tdcPriceOneSellOfficial.IngrePriceName = _context.IngredientsPrices.Where(l => l.Id == map_tdcPriceOneSellOfficial.IngredientsPriceId && l.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                }
                var item = map_tdcPriceOneSellOfficials.ToList();

                var groupData = item.GroupBy(x => x.TdcPriceOneSellId).Select(e => new GrOneSellDataOff
                {
                    TdcPriceOneSellId = e.Key,
                    grData = e.ToList(),
                    UpdateTime = e.First().UpdatedAt
                }).ToList();

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = groupData;
                return Ok(def);
            }
            catch (Exception e)
            {
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        private bool TdcPriceOneSellExists(int id)
        {
            return _context.TdcPriceOneSells.Count(e => e.Id == id) > 0;
        }

        private bool Check(List<TdcPriceOneSellOfficialData> list1, List<TdcPriceOneSellOfficialData> list2)
        {
            var difference = list1.Except(list2).ToList();
            return difference.Count == 0;
        }

    }
}
