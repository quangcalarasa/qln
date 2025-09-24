using AutoMapper;
using DevExpress.ClipboardSource.SpreadsheetML;
using DevExpress.XtraRichEdit.Import.Html;
using IOITQln.Common.Constants;
using IOITQln.Common.Enums;
using IOITQln.Common.Services;
using IOITQln.Common.ViewModels.Common;
using IOITQln.Entities;
using IOITQln.Migrations;
using IOITQln.Models.Data;
using IOITQln.Models.Dto;
using IOITQln.Persistence;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Web;
using static IOITQln.Common.Enums.AppEnums;


namespace IOITQln.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TDCProjectController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("project", "project");
        private static string functionCode = "TDC_PROJECT";
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        private IHostingEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;

        public TDCProjectController(ApiDbContext context, IMapper mapper, IHostingEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
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
                    IQueryable<TDCProject> data = _context.TDCProjects.Where(c => c.Status != AppEnums.EntityStatus.DELETED);
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

                        List<TDCProjectData> res = _mapper.Map<List<TDCProjectData>>(data.ToList());
                        foreach (TDCProjectData item in res)
                        {
                            item.FullAddress = item.BuildingName + item.HouseNumber;

                            Lane lane = _context.Lanies.Where(p => p.Id == item.Lane).FirstOrDefault();
                            item.FullAddress = item.FullAddress != null && item.FullAddress != "" ? (lane != null ? String.Join(", ", item.FullAddress, lane.Name) : item.FullAddress) : (lane != null ? lane.Name : item.FullAddress);
                            
                            Ward ward = _context.Wards.Where(p => p.Id == item.Ward).FirstOrDefault();
                            item.FullAddress = item.FullAddress != null && item.FullAddress != "" ? (ward != null ? String.Join(", ", item.FullAddress, ward.Name) : item.FullAddress) : (ward != null ? ward.Name : item.FullAddress);

                            District district = _context.Districts.Where(p => p.Id == item.District).FirstOrDefault();
                            item.FullAddress = item.FullAddress != null && item.FullAddress != "" ? (district != null ? String.Join(", ", item.FullAddress, district.Name) : item.FullAddress) : (district != null ? district.Name : item.FullAddress);

                            Province province = _context.Provincies.Where(p => p.Id == item.Province).FirstOrDefault();
                            item.FullAddress = item.FullAddress != null && item.FullAddress != "" ? (province != null ? String.Join(", ", item.FullAddress, province.Name) : item.FullAddress) : (province != null ? province.Name : item.FullAddress);

                            List<TDCProjectPriceAndTax> tDCProjectPriceAndTaxs = _context.TDCProjectPriceAndTaxs.Where(l => l.TDCProjectId == item.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                            List<TDCProjectPriceAndTaxData> map_TDCProjectPriceAndTaxs = _mapper.Map<List<TDCProjectPriceAndTaxData>>(tDCProjectPriceAndTaxs);
                            foreach (TDCProjectPriceAndTaxData map_TDCProjectPriceAndTax in map_TDCProjectPriceAndTaxs)
                            {
                                List<TDCProjectPriceAndTaxDetails> de = _context.TDCProjectPriceAndTaxDetailss.Where(f => f.PriceAndTaxId == map_TDCProjectPriceAndTax.Id && f.Status != AppEnums.EntityStatus.DELETED).ToList();
                                List<TDCProjectPriceAndTaxDetailData> detail = _mapper.Map<List<TDCProjectPriceAndTaxDetailData>>(de);
                                foreach (TDCProjectPriceAndTaxDetailData i in detail)
                                {
                                    IngredientsPrice result = _context.IngredientsPrices.Where(f => f.Id == i.IngredientsPriceId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                    if (result == null)
                                    {
                                        i.IngrePriceName = "";
                                    }
                                    else
                                    {
                                        i.IngrePriceName = (de == null) ? "" : result.Name;
                                    }
                                }
                                map_TDCProjectPriceAndTax.TDCProjectPriceAndTaxDetails = detail;
                                OriginalPriceAndTax pat = _context.OriginalPriceAndTaxs.Where(f => f.Id == map_TDCProjectPriceAndTax.PriceAndTaxId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                if (de == null) map_TDCProjectPriceAndTax.PATName = "";
                                else map_TDCProjectPriceAndTax.PATName = pat != null ? pat.Name : "";
                            }
                            item.tDCProjectPriceAndTaxes = map_TDCProjectPriceAndTaxs;
                            List<TDCProjectIngrePrice> tDCProjectIngrePrices = _context.TDCProjectIngrePrices.Where(l => l.TDCProjectId == item.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                            List<TDCProjectIngrePriceData> map_TDCProjectIngrePriceDatas = _mapper.Map<List<TDCProjectIngrePriceData>>(tDCProjectIngrePrices);
                            foreach (TDCProjectIngrePriceData map_TDCProjectIngrePriceData in map_TDCProjectIngrePriceDatas)
                            {
                                IngredientsPrice de = _context.IngredientsPrices.Where(f => f.Id == map_TDCProjectIngrePriceData.IngredientsPriceId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                if (de == null) map_TDCProjectIngrePriceData.IngrePriceName = "";
                                else map_TDCProjectIngrePriceData.IngrePriceName = de != null ? de.Name : "";
                            }
                            item.tDCProjectIngrePrices = map_TDCProjectIngrePriceDatas;
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

        //[HttpGet("ProjectByDistrict")]
        //public IActionResult ProjectByDistrict([FromQuery] FilteredPagination paging)
        //{
        //    string accessToken = Request.Headers[HeaderNames.Authorization];
        //    Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
        //    if (token == null)
        //    {
        //        return Unauthorized();
        //    }

        //    DefaultResponse def = new DefaultResponse();

        //    var identity = (ClaimsIdentity)User.Identity;
        //    string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
        //    if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
        //    {
        //        def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
        //        return Ok(def);
        //    }

        //    try
        //    {
        //        if (paging != null)
        //        {
        //            def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
        //            IQueryable<TDCProject> data = _context.TDCProjects.Where(c => c.Status != AppEnums.EntityStatus.DELETED);
        //            if (paging.query != null)
        //            {
        //                paging.query = HttpUtility.UrlDecode(paging.query);
        //            }

        //            data = data.Where(paging.query);
        //            def.metadata = data.Count();

        //            if (paging.page_size > 0)
        //            {
        //                if (paging.order_by != null)
        //                {
        //                    data = data.OrderBy(paging.order_by).Skip((paging.page - 1) * paging.page_size).Take(paging.page_size);
        //                }
        //                else
        //                {
        //                    data = data.OrderBy("Id desc").Skip((paging.page - 1) * paging.page_size).Take(paging.page_size);
        //                }
        //            }
        //            else
        //            {
        //                if (paging.order_by != null)
        //                {
        //                    data = data.OrderBy(paging.order_by);
        //                }
        //                else
        //                {
        //                    data = data.OrderBy("Id desc");
        //                }
        //            }

        //            if (paging.select != null && paging.select != "")
        //            {
        //                paging.select = "new(" + paging.select + ")";
        //                paging.select = HttpUtility.UrlDecode(paging.select);
        //                def.data = data.Select(paging.select);
        //            }
        //            else
        //            {

        //                List<TDCProjectData> res = _mapper.Map<List<TDCProjectData>>(data.ToList());
        //                foreach (TDCProjectData item in res)
        //                {
        //                    List<TDCProject> tDCProjects = _context.TDCProjects.Where(p => p.District == item.District && p.Status != AppEnums.EntityStatus.DELETED).ToList();
        //                    item.tDCProjectByDistricts = _mapper.Map<List<TDCProjectData>>(tDCProjects);
        //                }

        //                def.data = res;

        //            }

        //            return Ok(def);
        //        }
        //        else
        //        {
        //            def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
        //            return Ok(def);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error("GetByPage Error:" + ex);
        //        def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
        //        return Ok(def);
        //    }
        //}

        // GET: api/TDCProject/1
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
                TDCProject data = await _context.TDCProjects.FindAsync(id);
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                    return Ok(def);
                }

                TDCProjectData res = _mapper.Map<TDCProjectData>(data);

                res.FullAddress = res.BuildingName + ", " + res.HouseNumber;

                Lane lane = _context.Lanies.Where(p => p.Id == res.Lane).FirstOrDefault();
                res.FullAddress = res.FullAddress != null && res.FullAddress != "" ? (lane != null ? String.Join(", ", res.FullAddress, lane.Name) : res.FullAddress) : (lane != null ? lane.Name : res.FullAddress);

                Ward ward = _context.Wards.Where(p => p.Id == res.Ward).FirstOrDefault();
                res.FullAddress = res.FullAddress != null && res.FullAddress != "" ? (ward != null ? String.Join(", ", res.FullAddress, ward.Name) : res.FullAddress) : (ward != null ? ward.Name : res.FullAddress);

                District district = _context.Districts.Where(p => p.Id == res.District).FirstOrDefault();
                res.FullAddress = res.FullAddress != null && res.FullAddress != "" ? (district != null ? String.Join(", ", res.FullAddress, district.Name) : res.FullAddress) : (district != null ? district.Name : res.FullAddress);

                Province province = _context.Provincies.Where(p => p.Id == res.Province).FirstOrDefault();
                res.FullAddress = res.FullAddress != null && res.FullAddress != "" ? (province != null ? String.Join(", ", res.FullAddress, province.Name) : res.FullAddress) : (province != null ? province.Name : res.FullAddress);

                List<TDCProjectPriceAndTax> tDCProjectPriceAndTaxs = _context.TDCProjectPriceAndTaxs.Where(l => l.TDCProjectId == res.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<TDCProjectPriceAndTaxData> map_TDCProjectPriceAndTaxs = _mapper.Map<List<TDCProjectPriceAndTaxData>>(tDCProjectPriceAndTaxs);
                foreach (TDCProjectPriceAndTaxData map_TDCProjectPriceAndTax in map_TDCProjectPriceAndTaxs)
                {
                    List<TDCProjectPriceAndTaxDetails> de = _context.TDCProjectPriceAndTaxDetailss.Where(f => f.PriceAndTaxId == map_TDCProjectPriceAndTax.Id && f.Status != AppEnums.EntityStatus.DELETED).ToList();
                    List<TDCProjectPriceAndTaxDetailData> detail = _mapper.Map<List<TDCProjectPriceAndTaxDetailData>>(de);
                    foreach (TDCProjectPriceAndTaxDetailData i in detail)
                    {
                        IngredientsPrice result = _context.IngredientsPrices.Where(f => f.Id == i.IngredientsPriceId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                        if (de == null) i.IngrePriceName = "";
                        else i.IngrePriceName = result.Name;
                    }
                    map_TDCProjectPriceAndTax.TDCProjectPriceAndTaxDetails = detail;
                    OriginalPriceAndTax pat = _context.OriginalPriceAndTaxs.Where(f => f.Id == map_TDCProjectPriceAndTax.PriceAndTaxId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    if (de == null) map_TDCProjectPriceAndTax.PATName = "";
                    else map_TDCProjectPriceAndTax.PATName = pat.Name;
                }
                res.tDCProjectPriceAndTaxes = map_TDCProjectPriceAndTaxs;
                List<TDCProjectIngrePrice> tDCProjectIngrePrices = _context.TDCProjectIngrePrices.Where(l => l.TDCProjectId == res.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<TDCProjectIngrePriceData> map_TDCProjectIngrePriceDatas = _mapper.Map<List<TDCProjectIngrePriceData>>(tDCProjectIngrePrices);
                foreach (TDCProjectIngrePriceData map_TDCProjectIngrePriceData in map_TDCProjectIngrePriceDatas)
                {
                    IngredientsPrice de = _context.IngredientsPrices.Where(f => f.Id == map_TDCProjectIngrePriceData.IngredientsPriceId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    if (de == null) map_TDCProjectIngrePriceData.IngrePriceName = "";
                    else map_TDCProjectIngrePriceData.IngrePriceName = de.Name;
                }
                res.tDCProjectIngrePrices = map_TDCProjectIngrePriceDatas;
                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
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

        // POST: api/TDCProject
        [HttpPost]
        public async Task<IActionResult> Post(TDCProjectData input)
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
                input = (TDCProjectData)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                if (input.BuildingName == null || input.BuildingName == "")
                {
                    def.meta = new Meta(400, "Tòa nhà không được để trống!");
                    return Ok(def);
                }

                if (input.HouseNumber == null || input.HouseNumber == "")
                {
                    def.meta = new Meta(400, "Số căn nhà không được để trống!");
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.CreatedById = userId;
                    input.CreatedBy = fullName;
                    _context.TDCProjects.Add(input);

                    try
                    {
                        await _context.SaveChangesAsync();
                        //add tDCProjectIngrePrices
                        if (input.tDCProjectIngrePrices != null)
                        {
                            foreach (var tDCProjectIngrePrice in input.tDCProjectIngrePrices)
                            {
                                tDCProjectIngrePrice.TDCProjectId = input.Id;
                                tDCProjectIngrePrice.CreatedBy = fullName;
                                tDCProjectIngrePrice.CreatedById = userId;

                                _context.TDCProjectIngrePrices.Add(tDCProjectIngrePrice);
                            }
                            await _context.SaveChangesAsync();
                        }
                        //add tDCProjectPriceAndTaxes
                        if (input.tDCProjectPriceAndTaxes != null)
                        {
                            foreach (var tDCProjectPriceAndTaxe in input.tDCProjectPriceAndTaxes)
                            {
                                tDCProjectPriceAndTaxe.TDCProjectId = input.Id;
                                tDCProjectPriceAndTaxe.CreatedBy = fullName;
                                tDCProjectPriceAndTaxe.CreatedById = userId;
                                _context.TDCProjectPriceAndTaxs.Add(tDCProjectPriceAndTaxe);
                                await _context.SaveChangesAsync();
                                if (tDCProjectPriceAndTaxe.TDCProjectPriceAndTaxDetails != null)
                                {
                                    foreach (var tDCProjectPriceAndTaxDetails in tDCProjectPriceAndTaxe.TDCProjectPriceAndTaxDetails)
                                    {
                                        tDCProjectPriceAndTaxDetails.PriceAndTaxId = tDCProjectPriceAndTaxe.Id;
                                        tDCProjectPriceAndTaxDetails.CreatedBy = fullName;
                                        tDCProjectPriceAndTaxDetails.CreatedById = userId;

                                        _context.TDCProjectPriceAndTaxDetailss.Add(tDCProjectPriceAndTaxDetails);
                                    }
                                }
                            }
                            await _context.SaveChangesAsync();
                        }
                        int newid = input.Id;
                        string code = "";
                        if (newid < 10) code = "DA000" + newid;
                        if (newid >= 10) code = "DA00" + newid;
                        if (newid >= 100) code = "DA0" + newid;
                        if (newid >= 1000) code = "DA" + newid;
                        input.Code = code;
                        _context.Update(input);
                        await _context.SaveChangesAsync();
                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Thêm mới dự án tái định cư " + input.Name, "TDCProject", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
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
                        if (TDCProjectExists(input.Id))
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

        // PUT: api/TDCProjcet/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] TDCProjectData input)
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
                input = (TDCProjectData)UtilsService.TrimStringPropertyTypeObject(input);

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
                if (input.HouseNumber == null || input.HouseNumber == "")
                {
                    def.meta = new Meta(400, "Số căn nhà không được để trống!");
                    return Ok(def);
                }
                if (input.BuildingName == null || input.BuildingName == "")
                {
                    def.meta = new Meta(400, "Tòa nhà không được để trống!");
                    return Ok(def);
                }

                TDCProject data = await _context.TDCProjects.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
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
                        //PriceAndTax
                        List<TDCProjectPriceAndTax> tDCProjectPriceAndTaxs = _context.TDCProjectPriceAndTaxs.Where(l => l.TDCProjectId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).AsNoTracking().ToList();
                        List<TDCProjectPriceAndTaxData> map_TDCProjectPriceAndTaxs = _mapper.Map<List<TDCProjectPriceAndTaxData>>(tDCProjectPriceAndTaxs);

                        if (input.tDCProjectPriceAndTaxes != null)
                        {
                            foreach (TDCProjectPriceAndTaxData item in input.tDCProjectPriceAndTaxes)
                            {
                                TDCProjectPriceAndTaxData TDCProjectPriceAndTaxpExist = map_TDCProjectPriceAndTaxs.Where(l => l.Id == item.Id).FirstOrDefault();
                                if (TDCProjectPriceAndTaxpExist == null)
                                {
                                    item.TDCProjectId = input.Id;
                                    item.CreatedBy = fullName;
                                    item.CreatedById = userId;

                                    _context.TDCProjectPriceAndTaxs.Add(item);
                                    await _context.SaveChangesAsync();

                                    foreach (var itemDetails in item.TDCProjectPriceAndTaxDetails)
                                    {
                                        itemDetails.Id = 0;
                                        itemDetails.PriceAndTaxId = item.Id;
                                        itemDetails.CreatedBy = fullName;
                                        itemDetails.CreatedById = userId;

                                        _context.TDCProjectPriceAndTaxDetailss.Add(itemDetails);
                                    }

                                }
                                else
                                {
                                    item.CreatedAt = TDCProjectPriceAndTaxpExist.CreatedAt;
                                    item.CreatedBy = TDCProjectPriceAndTaxpExist.CreatedBy;
                                    item.CreatedById = TDCProjectPriceAndTaxpExist.CreatedById;
                                    item.TDCProjectId = input.Id;
                                    item.UpdatedBy = fullName;
                                    item.UpdatedById = userId;

                                    _context.Update(item);

                                    List<TDCProjectPriceAndTaxDetails> listItemDetail = _context.TDCProjectPriceAndTaxDetailss.Where(x => x.PriceAndTaxId == item.Id && x.Status == AppEnums.EntityStatus.DELETED).ToList();

                                    foreach (var itemDetails in item.TDCProjectPriceAndTaxDetails)
                                    {
                                        var isExist = listItemDetail.Where(x => x.PriceAndTaxId == item.Id && x.Status == AppEnums.EntityStatus.DELETED && x.IngredientsPriceId == itemDetails.IngredientsPriceId).FirstOrDefault();

                                        if (isExist == null)
                                        {

                                            itemDetails.PriceAndTaxId = item.Id;
                                            itemDetails.CreatedBy = fullName;
                                            itemDetails.CreatedById = userId;

                                            _context.TDCProjectPriceAndTaxDetailss.Add(itemDetails);
                                        }
                                        else
                                        {
                                            listItemDetail.Remove(isExist);
                                        }


                                        _context.TDCProjectPriceAndTaxDetailss.Add(itemDetails);
                                    }
                                    _context.TDCProjectPriceAndTaxDetailss.RemoveRange(listItemDetail);
                                    map_TDCProjectPriceAndTaxs.Remove(TDCProjectPriceAndTaxpExist);
                                }
                            }
                        }

                        if (map_TDCProjectPriceAndTaxs.Count > 0)
                        {
                            map_TDCProjectPriceAndTaxs.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = AppEnums.EntityStatus.DELETED;
                            });
                            _context.UpdateRange(map_TDCProjectPriceAndTaxs);
                        }

                        //IngredientsPrice
                        List<TDCProjectIngrePrice> tDCProjectIngrePrices = _context.TDCProjectIngrePrices.Where(l => l.TDCProjectId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                        if (input.tDCProjectIngrePrices != null)
                        {
                            foreach (var tDCProjectIngrePrice in input.tDCProjectIngrePrices)
                            {
                                TDCProjectIngrePrice tDCProjectIngrePriceExist = tDCProjectIngrePrices.Where(l => l.Id == tDCProjectIngrePrice.Id).FirstOrDefault();
                                if (tDCProjectIngrePriceExist == null)
                                {
                                    tDCProjectIngrePrice.TDCProjectId = input.Id;
                                    tDCProjectIngrePrice.CreatedBy = fullName;
                                    tDCProjectIngrePrice.CreatedById = userId;

                                    _context.TDCProjectIngrePrices.Add(tDCProjectIngrePrice);
                                }
                                else
                                {
                                    tDCProjectIngrePrice.CreatedAt = tDCProjectIngrePriceExist.CreatedAt;
                                    tDCProjectIngrePrice.CreatedBy = tDCProjectIngrePriceExist.CreatedBy;
                                    tDCProjectIngrePrice.CreatedById = tDCProjectIngrePriceExist.CreatedById;
                                    tDCProjectIngrePrice.TDCProjectId = input.Id;
                                    tDCProjectIngrePrice.UpdatedBy = fullName;
                                    tDCProjectIngrePrice.UpdatedById = userId;

                                    _context.Update(tDCProjectIngrePrice);

                                    tDCProjectIngrePrices.Remove(tDCProjectIngrePriceExist);
                                }
                            }
                        }
                        if (tDCProjectIngrePrices.Count > 0)
                            tDCProjectIngrePrices.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(tDCProjectIngrePrices);


                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Sửa dự án tái định cư  " + input.Name, "TDCProject", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, userId, fullName);
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
                        if (!TDCProjectExists(data.Id))
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

        // DELETE: api/TDCProject/1
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
                TDCProject data = await _context.TDCProjects.FindAsync(id);
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

                    //Xóa TDCProjectPAT
                    List<TDCProjectPriceAndTax> tDCProjectPriceAndTaxs = _context.TDCProjectPriceAndTaxs.Where(l => l.TDCProjectId == data.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if (tDCProjectPriceAndTaxs.Count > 0)
                    {
                        tDCProjectPriceAndTaxs.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                            // xóa TDCProjectPriceAndTaxDeatail
                            List<TDCProjectPriceAndTaxDetails> Details = _context.TDCProjectPriceAndTaxDetailss.Where(x => x.PriceAndTaxId == item.Id && x.Status != AppEnums.EntityStatus.DELETED).ToList();
                            if (Details.Count > 0)
                            {
                                Details.ForEach(itemd =>
                                {
                                    itemd.UpdatedAt = DateTime.Now;
                                    itemd.UpdatedById = userId;
                                    itemd.UpdatedBy = fullName;
                                    itemd.Status = AppEnums.EntityStatus.DELETED;
                                });
                                _context.UpdateRange(Details);
                            }
                        });
                        _context.UpdateRange(tDCProjectPriceAndTaxs);
                    }

                    //Xóa TDCProjectIP
                    List<TDCProjectIngrePrice> tDCProjectIngrePrices = _context.TDCProjectIngrePrices.Where(l => l.TDCProjectId == data.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if (tDCProjectIngrePrices.Count > 0)
                    {
                        tDCProjectIngrePrices.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(tDCProjectIngrePrices);
                    }
                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Xóa dự án tái định cư " + data.Name, "TDCProject", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.DELETED, userId, fullName);
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
                        if (!TDCProjectExists(data.Id))
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

        private bool TDCProjectExists(int id)
        {
            return _context.TDCProjects.Count(e => e.Id == id) > 0;
        }

        [HttpPost("ExportExcel")]
        public async Task<IActionResult> ExportExcel()
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            var identity = (ClaimsIdentity)User.Identity;
            int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id").Select(c => c.Value).SingleOrDefault());
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();

            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.EXPORT))
            {
                return new FileContentResult(new byte[0], "application/octet-stream");
            }

            List<TDCProject> tDCProjects = _context.TDCProjects.Where(f => f.Status != AppEnums.EntityStatus.DELETED).ToList();
            List<TDCProjectData> mapper_tdc = _mapper.Map<List<TDCProjectData>>(tDCProjects);
            foreach (var map in mapper_tdc)
            {
                map.FullAddress = map.BuildingName + ", " + map.HouseNumber;

                Lane lane = _context.Lanies.Where(p => p.Id == map.Lane).FirstOrDefault();
                map.FullAddress = map.FullAddress != null && map.FullAddress != "" ? (lane != null ? String.Join(", ", map.FullAddress, lane.Name) : map.FullAddress) : (lane != null ? lane.Name : map.FullAddress);

                Ward ward = _context.Wards.Where(p => p.Id == map.Ward).FirstOrDefault();
                map.FullAddress = map.FullAddress != null && map.FullAddress != "" ? (ward != null ? String.Join(", ", map.FullAddress, ward.Name) : map.FullAddress) : (ward != null ? ward.Name : map.FullAddress);

                District district = _context.Districts.Where(p => p.Id == map.District).FirstOrDefault();
                map.FullAddress = map.FullAddress != null && map.FullAddress != "" ? (district != null ? String.Join(", ", map.FullAddress, district.Name) : map.FullAddress) : (district != null ? district.Name : map.FullAddress);

                Province province = _context.Provincies.Where(p => p.Id == map.Province).FirstOrDefault();
                map.FullAddress = map.FullAddress != null && map.FullAddress != "" ? (province != null ? String.Join(", ", map.FullAddress, province.Name) : map.FullAddress) : (province != null ? province.Name : map.FullAddress);

                List<TDCProjectPriceAndTax> tDCProjectPriceAndTaxs = _context.TDCProjectPriceAndTaxs.Where(l => l.TDCProjectId == map.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<TDCProjectPriceAndTaxData> map_TDCProjectPriceAndTaxs = _mapper.Map<List<TDCProjectPriceAndTaxData>>(tDCProjectPriceAndTaxs);
                foreach (TDCProjectPriceAndTaxData map_TDCProjectPriceAndTax in map_TDCProjectPriceAndTaxs)
                {
                    List<TDCProjectPriceAndTaxDetails> de = _context.TDCProjectPriceAndTaxDetailss.Where(f => f.PriceAndTaxId == map_TDCProjectPriceAndTax.Id && f.Status != AppEnums.EntityStatus.DELETED).ToList();
                    List<TDCProjectPriceAndTaxDetailData> detail = _mapper.Map<List<TDCProjectPriceAndTaxDetailData>>(de);
                    foreach (TDCProjectPriceAndTaxDetailData i in detail)
                    {
                        IngredientsPrice result = _context.IngredientsPrices.Where(f => f.Id == i.IngredientsPriceId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                        if (result == null)
                        {
                            i.IngrePriceName = "";
                        }
                        else
                        {
                            i.IngrePriceName = (de == null) ? "" : result.Name;
                        }
                    }
                    map_TDCProjectPriceAndTax.TDCProjectPriceAndTaxDetails = detail;
                    OriginalPriceAndTax pat = _context.OriginalPriceAndTaxs.Where(f => f.Id == map_TDCProjectPriceAndTax.PriceAndTaxId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    if (de == null) map_TDCProjectPriceAndTax.PATName = "";
                    else map_TDCProjectPriceAndTax.PATName = pat != null ? pat.Name : "";
                }
                map.tDCProjectPriceAndTaxes = map_TDCProjectPriceAndTaxs;

                List<TDCProjectIngrePrice> tDCProjectIngrePrices = _context.TDCProjectIngrePrices.Where(l => l.TDCProjectId == map.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<TDCProjectIngrePriceData> map_TDCProjectIngrePriceDatas = _mapper.Map<List<TDCProjectIngrePriceData>>(tDCProjectIngrePrices);
                foreach (TDCProjectIngrePriceData map_TDCProjectIngrePriceData in map_TDCProjectIngrePriceDatas)
                {
                    IngredientsPrice de = _context.IngredientsPrices.Where(f => f.Id == map_TDCProjectIngrePriceData.IngredientsPriceId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    if (de == null) map_TDCProjectIngrePriceData.IngrePriceName = "";
                    else map_TDCProjectIngrePriceData.IngrePriceName = de != null ? de.Name : "";
                }
                map.tDCProjectIngrePrices = map_TDCProjectIngrePriceDatas;

            }

            // khởi tạo wb rỗng
            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"templates/tdcProject.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);

            MemoryStream ms = WriteDataToExcel(templatePath, 0, mapper_tdc);
            byte[] byteArrayContent = ms.ToArray();
            return new FileContentResult(byteArrayContent, "application/octet-stream");
        }

        private static MemoryStream WriteDataToExcel(string templatePath, int sheetnumber, List<TDCProjectData> data)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowStart = 2;
            int rowPriceTax = 0;

            if (sheet != null)
            {
                int datacol = 16;
                int dataingre = 0;
                int datapricetax = 0;
                try
                {
                    List<ICellStyle> rowStyle = new List<ICellStyle>();
                    for (int i = 0; i < datacol; i++)
                    {
                        rowStyle.Add(sheet.GetRow(0).GetCell(i).CellStyle);
                    }

                    

                    int k = 0;
                    foreach (var item in data)
                    {
                        k++;
                        XSSFRow row = (XSSFRow)sheet.CreateRow(rowStart);
                        for (int i = 0; i < datacol; i++)
                        {
                            row.CreateCell(i).CellStyle = rowStyle[i];

                            if (i == 0)
                            {
                                row.GetCell(i).SetCellValue(k);
                            }
                            else if (i == 1)
                            {
                                row.GetCell(i).SetCellValue(item.Code);
                            }
                            else if (i == 2)
                            {
                                row.GetCell(i).SetCellValue(item.Name);
                            }
                            else if (i == 3)
                            {
                                row.GetCell(i).SetCellValue(item.LandCount);
                            }
                            else if (i == 4)
                            {
                                row.GetCell(i).SetCellValue((double)item.LateRate);
                            }
                            else if (i == 5)
                            {
                                row.GetCell(i).SetCellValue((double)item.DebtRate);
                            }
                            else if (i == 6)
                            {
                                row.GetCell(i).SetCellValue(item.TotalAreas);
                            }
                            else if (i == 7)
                            {
                                row.GetCell(i).SetCellValue(item.TotalApartment);
                            }
                            else if (i == 8)
                            {
                                row.GetCell(i).SetCellValue(item.TotalPlatform);
                            }
                            else if (i == 9)
                            {
                                row.GetCell(i).SetCellValue(item.TotalFloorAreas);
                            }
                            else if (i == 10)
                            {
                                row.GetCell(i).SetCellValue(item.TotalUseAreas);
                            }
                            else if (i == 11)
                            {
                                row.GetCell(i).SetCellValue(item.TotalBuildAreas);
                            }
                            else if (i == 12)
                            {
                                row.GetCell(i).SetCellValue(item.HouseNumber);
                            }
                            else if (i == 13)
                            {
                                row.GetCell(i).SetCellValue(item.BuildingName);
                            }
                            else if (i == 14)
                            {
                                row.GetCell(i).SetCellValue(item.FullAddress);
                            }
                            else if (i == 15)
                            {
                                row.GetCell(i).SetCellValue(item.Note);
                            }


                        }
                        rowStart++;

                        ICellStyle style = workbook.CreateCellStyle();
                        style.FillForegroundColor = HSSFColor.LightGreen.Index;
                        style.FillPattern = FillPattern.SolidForeground;
                        style.BorderBottom = BorderStyle.Thin;
                        style.BorderTop = BorderStyle.Thin;
                        style.BorderLeft = BorderStyle.Thin;
                        style.BorderRight = BorderStyle.Thin;

                        ICellStyle styleValue = workbook.CreateCellStyle();
                        styleValue.BorderBottom = BorderStyle.Thin;
                        styleValue.BorderTop = BorderStyle.Thin;
                        styleValue.BorderLeft = BorderStyle.Thin;
                        styleValue.BorderRight = BorderStyle.Thin;

                        

                        IRow rowIngres1 = sheet.CreateRow(rowStart);

                        ICell cellIngre = rowIngres1.CreateCell(1);
                        cellIngre.SetCellValue("Thành phần bán giá cấu thành");
                        cellIngre.CellStyle = style;
                        CellRangeAddress mergedRegioncellHeader1 = new CellRangeAddress(rowStart, rowStart, 1, 2);
                        RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                        RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                        RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                        RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                        sheet.AddMergedRegion(mergedRegioncellHeader1);

                        int colIngre = 3;
                        foreach (var j in item.tDCProjectIngrePrices)
                        {
                            ICell cellIngre1 = rowIngres1.CreateCell(colIngre);
                            cellIngre1.SetCellValue(j.IngrePriceName);
                            cellIngre1.CellStyle = styleValue;
                            colIngre++;
                        }

                        rowStart++;

                        IRow rowingres2 = sheet.CreateRow(rowStart);

                        ICell cellingrevalue = rowingres2.CreateCell(1);
                        cellingrevalue.SetCellValue("Hệ số");
                        cellingrevalue.CellStyle = style;
                        CellRangeAddress mergedregioncellheader2 = new CellRangeAddress(rowStart, rowStart, 1, 2);
                        RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedregioncellheader2, sheet);
                        RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedregioncellheader2, sheet);
                        RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedregioncellheader2, sheet);
                        RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedregioncellheader2, sheet);
                        sheet.AddMergedRegion(mergedregioncellheader2);

                        int valueIngre = 3;
                        foreach (var z in item.tDCProjectIngrePrices)
                        {
                            ICell cellingre2 = rowingres2.CreateCell(valueIngre);
                            cellingre2.SetCellValue(z.Value);
                            cellingre2.CellStyle = styleValue;
                            valueIngre++;
                        }

                        rowStart++;

                        IRow rowPriceTaxs3 = sheet.CreateRow(rowStart);

                        ICell cellPriceTax = rowPriceTaxs3.CreateCell(1);
                        cellPriceTax.SetCellValue("Thành phần Giá gốc thuế phí");
                        cellPriceTax.CellStyle = style;
                        CellRangeAddress mergedRegioncellHeader3 = new CellRangeAddress(rowStart, rowStart, 1, 2);
                        RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader3, sheet);
                        RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader3, sheet);
                        RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader3, sheet);
                        RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader3, sheet);
                        sheet.AddMergedRegion(mergedRegioncellHeader3);

                        int codPriceTax = 3;
                        foreach (var j in item.tDCProjectPriceAndTaxes)
                        {
                            ICell cellPriceTax3 = rowPriceTaxs3.CreateCell(codPriceTax);
                            cellPriceTax3.SetCellValue(j.PATName);
                            cellPriceTax3.CellStyle = styleValue;
                            codPriceTax++;
                        }

                        rowStart++;

                        IRow rowPriceTaxs4 = sheet.CreateRow(rowStart);

                        ICell cellPriceTaxValue = rowPriceTaxs4.CreateCell(1);
                        cellPriceTaxValue.SetCellValue("Hệ số");
                        cellPriceTaxValue.CellStyle = style;
                        CellRangeAddress mergedRegioncellHeader4 = new CellRangeAddress(rowStart, rowStart, 1, 2);
                        RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader4, sheet);
                        RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader4, sheet);
                        RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader4, sheet);
                        RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader4, sheet);
                        sheet.AddMergedRegion(mergedRegioncellHeader4);

                        int codPriceTaxValue = 3;
                        foreach (var j in item.tDCProjectPriceAndTaxes)
                        {
                            ICell cellPriceTax4 = rowPriceTaxs4.CreateCell(codPriceTaxValue);
                            cellPriceTax4.SetCellValue(j.Value);
                            cellPriceTax4.CellStyle = styleValue;
                            codPriceTaxValue++;
                        }

                        rowStart++;
                    }
                }
                catch (Exception ex)
                {
                }
            }

            sheet.ForceFormulaRecalculation = true;

            MemoryStream ms = new MemoryStream();

            workbook.Write(ms);

            return ms;
        }

        #region import hệ số lương
        [HttpPost]
        [Route("ImportDataExcel")]
        public IActionResult ImportDataExcel()
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
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.IMPORT))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_ACTION_MESSAGE);
                return Ok(def);
            }
            try
            {
                int i = 0;
                var httpRequest = Request.Form.Files;
                ImportHistory importHistory = new ImportHistory();
                importHistory.Type = AppEnums.ImportHistoryType.TdcProject;

                List<TDCProjectDataImport> data = new List<TDCProjectDataImport>();

                foreach (var file in httpRequest)
                {
                    var postedFile = httpRequest[i];
                    if (postedFile != null && postedFile.Length > 0)
                    {
                        IList<string> AllowedDocuments = new List<string> { ".xls", ".xlsx" };
                        int MaxContentLength = 1024 * 1024 * 32; //Size = 32 MB

                        var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                        var name = postedFile.FileName.Substring(0, postedFile.FileName.LastIndexOf('.'));
                        var extension = ext.ToLower();

                        bool checkFile = true;
                        if (AllowedDocuments.Contains(extension))
                        {
                            checkFile = false;
                        }

                        if (checkFile)
                        {
                            var message = string.Format("Vui lòng upload đúng định dạng file excel!");
                            def.meta = new Meta(600, message);
                            return Ok(def);
                        }

                        if (postedFile.Length > MaxContentLength)
                        {
                            var message = string.Format("Vui lòng Up file có dung lượng nhỏ hơn 32 MB!");
                            def.meta = new Meta(600, message);
                            return Ok(def);
                        }
                        else
                        {
                            string folderName = _configuration["AppSettings:BaseUrlImportHistory"]; ;
                            string webRootPath = _hostingEnvironment.WebRootPath;
                            string newPath = Path.Combine(webRootPath, folderName);
                            if (!Directory.Exists(newPath))
                            {
                                Directory.CreateDirectory(newPath);
                            }

                            string fileNameCheck = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.ToString().Trim('"');
                            if (UtilsService.ConvertUrlpath(fileNameCheck).Contains("../") || UtilsService.ConvertUrlpath(fileNameCheck).Contains("..\\") || fileNameCheck.IndexOfAny(Path.GetInvalidPathChars()) > -1)
                            {
                                var vMessage = "Tên file không hợp lệ!";

                                def.meta = new Meta(202, vMessage);
                                return Ok(def);
                            }

                            DateTime now = DateTime.Now;
                            string fileName = name + "_" + now.ToString("yyyyMMddHHmmssfff") + extension;
                            string fullPath = Path.Combine(newPath, fileName);
                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                postedFile.CopyTo(stream);
                            }

                            importHistory.FileUrl = fileName;

                            byte[] fileData = null;
                            using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                            {
                                fileData = binaryReader.ReadBytes((int)file.Length);
                                using (MemoryStream ms = new MemoryStream(fileData))
                                {
                                    data = importData(ms, 0, 2);
                                }
                            }
                        }
                    }
                    i++;
                }



                List<TDCProjectDataImport> dataValid = new List<TDCProjectDataImport>();

                List<Province> provinces = _context.Provincies.Where(e => e.Status != EntityStatus.DELETED).ToList();
                List<District> districts = _context.Districts.Where(e => e.Status != EntityStatus.DELETED).ToList();
                List<Ward> wards = _context.Wards.Where(e => e.Status != EntityStatus.DELETED).ToList();
                List<Lane> lanes = _context.Lanies.Where(e => e.Status != EntityStatus.DELETED).ToList();

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var dataItem in data)
                        {
                            if (dataItem.Valid == true)
                            {
                                //Kiểm tra Tỉnh TP
                                string provinceNameNoneUnicode = UtilsService.NonUnicode(dataItem.ProvinceName);
                                Province province = provinces.AsEnumerable().Where(e => UtilsService.NonUnicode(e.Name).Contains(provinceNameNoneUnicode)).FirstOrDefault();
                                if (province == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy tỉnh (tp)\n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.Province = province.Id;
                                }

                                //Kiểm tra Quận huyện
                                string districtNameNoneUnicode = UtilsService.NonUnicode(dataItem.DistrictName);
                                District district = districts.AsEnumerable().Where(e => e.ProvinceId == dataItem.Province && UtilsService.NonUnicode(e.Name).Contains(districtNameNoneUnicode)).FirstOrDefault();
                                if (district == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy quận (huyện)\n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.District = district.Id;
                                }

                                //Kiểm tra phường xã
                                string wardNameNoneUnicode = UtilsService.NonUnicode(dataItem.WardName);
                                Ward ward = wards.AsEnumerable().Where(e => e.DistrictId == dataItem.District && UtilsService.NonUnicode(e.Name).Contains(wardNameNoneUnicode)).FirstOrDefault();
                                if (ward == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy phường (xã)\n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.Ward = ward.Id;
                                }

                                //Kiểm tra duong
                                string laneNameNoneUnicode = UtilsService.NonUnicode(dataItem.LaneName);
                                Lane lane = lanes.AsEnumerable().Where(e => e.Ward == dataItem.Ward && UtilsService.NonUnicode(e.Name).Contains(laneNameNoneUnicode)).FirstOrDefault();
                                if (lane == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy đường\n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.Lane = lane.Id;
                                }

                                dataItem.CreatedById = -1;
                                dataItem.CreatedBy = fullName;
                                dataItem.Code = "DA";
                                dataItem.Code = CodeIndentity.CodeInd("DA", _context.TDCProjects.Count(), 4);
                                dataValid.Add(dataItem);
                            }
                        }
                        importHistory.Data = data.Cast<dynamic>().ToList();
                        importHistory.CreatedById = userId;
                        importHistory.CreatedBy = fullName;

                        _context.ImportHistories.Add(importHistory);
                        _context.TDCProjects.AddRange(dataValid);

                        _context.SaveChanges();

                        transaction.Commit();
                        def.meta = new Meta(200, ApiConstants.MessageResource.DELETE_SUCCESS);
                        def.metadata = data.Count;
                        def.data = data;
                    }
                    catch (Exception ex)
                    {
                        log.Error("ImportDataExcel:" + ex);
                        transaction.Rollback();
                        def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                    }
                }
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("ImportDataExcel:" + ex);
                def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                return Ok(def);
            }
        }

        public static List<TDCProjectDataImport> importData(MemoryStream ms, int sheetnumber, int rowStart)
        {
            XSSFWorkbook workbook = new XSSFWorkbook(ms);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);

            List<TDCProjectDataImport> res = new List<TDCProjectDataImport>();
            for (int row = rowStart; row <= sheet.LastRowNum; row++)
            {
                if (sheet.GetRow(row) != null)
                {
                    TDCProjectDataImport inputDetail = new TDCProjectDataImport();
                    inputDetail.Valid = true;
                    inputDetail.ErrMsg = "";

                    for (int i = 0; i < 16; i++)
                    {
                        try
                        {
                            var cell = sheet.GetRow(row).GetCell(i, MissingCellPolicy.RETURN_NULL_AND_BLANK);

                            //Lấy giá trị trong cell
                            string str = UtilsService.getCellValue(cell);
                            if (i == 0)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.Index = int.Parse(str);
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Số thứ tự\n";
                                }
                            }

                            else if (i == 1)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.Name = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột tên dự án chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột tên lô\n";
                                }
                            }

                            else if (i == 2)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.LateRate = decimal.Parse(str);
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột hệ số lãi phạt trễ\n";
                                }
                            }

                            else if (i == 3)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.DebtRate = decimal.Parse(str);
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột lãi nợ\n";
                                }
                            }

                            else if (i == 4)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TotalAreas = double.Parse(str);
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột tổng diện tích chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột tổng diện tích\n";
                                }
                            }

                            else if (i == 5)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TotalApartment = int.Parse(str);
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột tổng số căn hộ chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột tổng số căn hộ\n";
                                }
                            }

                            else if (i == 6)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TotalPlatform = int.Parse(str);
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột tổng số nền đất chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột tổng số nền đất\n";
                                }
                            }

                            else if (i == 7)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TotalFloorAreas = double.Parse(str);
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột tổng diện tích sàn chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột tổng diện tích sàn\n";
                                }
                            }

                            else if (i == 8)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TotalUseAreas = double.Parse(str);
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột tổng diện tích sử dụng chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột tổng diện tích sử dụng\n";
                                }
                            }

                            else if (i == 9)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TotalBuildAreas = double.Parse(str);
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột tổng diện tích xây dựng chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột tổng diện tích xây dựng\n";
                                }
                            }

                            else if (i == 10)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.HouseNumber = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột căn nhà số chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột căn nhà số\n";
                                }
                            }

                            else if (i == 11)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.BuildingName = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột tòa chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột tòa\n";
                                }
                            }

                            else if (i == 12)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.ProvinceName = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột tỉnh/thành phố không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột tỉnh/thành phố\n";
                                }
                            }
                            else if (i == 13)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.DistrictName = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột quận/huyện không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột quận/huyện\n";
                                }
                            }
                            else if (i == 14)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.WardName = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột xã/phường không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột xã/phường\n";
                                }
                            }
                            else if (i == 15)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.LaneName = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột đường không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột đường\n";
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            inputDetail.Valid = false;
                            inputDetail.ErrMsg += "Lỗi dữ liệu\n";
                            log.Error("Exception:" + ex);
                        }
                    }
                    res.Add(inputDetail);
                }
            }
            return res;
        }

        #endregion

        //[HttpPost("ImportExcel")]
        //public async Task<IActionResult> ImportExcel([FromBody] List<TDCProjectData> input)
        //{
        //    string accessToken = Request.Headers[HeaderNames.Authorization];
        //    Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
        //    if (token == null)
        //    {
        //        return Unauthorized();
        //    }

        //    DefaultResponse def = new DefaultResponse();
        //    var identity = (ClaimsIdentity)User.Identity;
        //    int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id").Select(c => c.Value).SingleOrDefault());
        //    string fullName = identity.Claims.Where(c => c.Type == "FullName").Select(c => c.Value).SingleOrDefault() ?? string.Empty;
        //    string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
        //    if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.CREATE))
        //    {
        //        def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_CREATE_MESSAGE);
        //        return Ok(def);
        //    }
        //    List<string> messages = new List<string>();

        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
        //            return Ok(def);
        //        }
        //        using (var transaction = _context.Database.BeginTransaction())
        //        {
        //            try 
        //            {
        //                List<District> districts = _context.Districts.Where(d => d.Status != AppEnums.EntityStatus.DELETED).ToList();
        //                List<Province> provinces = _context.Provincies.Where(d => d.Status != AppEnums.EntityStatus.DELETED).ToList();
        //                List<Ward> wards = _context.Wards.Where(d => d.Status != AppEnums.EntityStatus.DELETED).ToList();
        //                List<Lane> lanies = _context.Lanies.Where(d => d.Status != AppEnums.EntityStatus.DELETED).ToList();
        //                List<TDCProjectIngrePrice> tdcprojectPricies = _context.TDCProjectIngrePrices.Where(d => d.Status != AppEnums.EntityStatus.DELETED).ToList();

        //                foreach (var item in input)
        //                {
        //                    var existCode = _context.TDCProjects.FirstOrDefault(x => x.Code == item.Code);
        //                    if (existCode != null)
        //                    {
        //                        messages.Add(item.Code + " không hợp lệ");
        //                        continue;
        //                    }

        //                    int provinceId = provinces.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(item.ProvinceName)).Select(x => x.Id).FirstOrDefault();
        //                    if (provinceId > 0)
        //                    {
        //                        item.Province = provinceId;
        //                    }
        //                    if (provinceId == 0)   
        //                    {
        //                        def.meta = new Meta(400, "Tỉnh/thành phố không tồn tại");
        //                        return Ok(def);
        //                    }

        //                    var districtId = districts.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(item.DistrictName)).Select(x => x.Id).FirstOrDefault();

        //                    if (districtId > 0)
        //                    {
        //                        item.District = districtId;
        //                    }
        //                    if (districtId == 0)
        //                    {
        //                        def.meta = new Meta(400, "Quận/huyện không tồn tại");
        //                        return Ok(def);
        //                    }

        //                    int wardId = wards.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(item.WardName)).Select(x => x.Id).FirstOrDefault();
        //                    if (wardId > 0)
        //                    { 
        //                        item.Ward = wardId;
        //                    }
        //                    if (wardId == 0)
        //                    {
        //                        def.meta = new Meta(400, "Phường/Xã không tồn tại");
        //                        return Ok(def);
        //                    }

        //                    int laneId = lanies.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(item.LaneName)).Select(x => x.Id).FirstOrDefault();
        //                    if (laneId > 0)
        //                    {
        //                        item.Lane = laneId;
        //                    }
        //                    if (laneId == 0)
        //                    {
        //                        def.meta = new Meta(400, "Đường không tồn tại");
        //                        return Ok(def);
        //                    }

        //                    item.CreatedBy = fullName;
        //                    item.CreatedById = userId;
        //                    _context.TDCProjects.Add(item);
        //                    await _context.SaveChangesAsync();

        //                    if (item.tDCProjectIngrePrices != null)
        //                    {
        //                        foreach (var tDCProjectIngrePrice in item.tDCProjectIngrePrices)
        //                        {
        //                            int IngrePriceId = _context.IngredientsPrices.Where(x => x.Name.Trim().Contains(tDCProjectIngrePrice.IngrePriceName)).Select(x => x.Id).FirstOrDefault();
        //                            if(IngrePriceId > 0)
        //                            {
        //                                tDCProjectIngrePrice.IngredientsPriceId = IngrePriceId;
        //                            }

        //                            tDCProjectIngrePrice.TDCProjectId = item.Id;
        //                            tDCProjectIngrePrice.CreatedBy = fullName;
        //                            tDCProjectIngrePrice.CreatedById = userId;

        //                            _context.TDCProjectIngrePrices.Add(tDCProjectIngrePrice);
        //                        }
        //                        await _context.SaveChangesAsync();
        //                    }
        //                    //add tDCProjectPriceAndTaxes
        //                    if (item.tDCProjectPriceAndTaxes != null)
        //                    {
        //                        foreach (var tDCProjectPriceAndTaxe in item.tDCProjectPriceAndTaxes)
        //                        {
        //                            tDCProjectPriceAndTaxe.TDCProjectId = item.Id;
        //                            tDCProjectPriceAndTaxe.CreatedBy = fullName;
        //                            tDCProjectPriceAndTaxe.CreatedById = userId;
        //                            _context.TDCProjectPriceAndTaxs.Add(tDCProjectPriceAndTaxe);
        //                            await _context.SaveChangesAsync();
        //                            if (tDCProjectPriceAndTaxe.TDCProjectPriceAndTaxDetails != null)
        //                            {
        //                                foreach (var tDCProjectPriceAndTaxDetails in tDCProjectPriceAndTaxe.TDCProjectPriceAndTaxDetails)
        //                                {
        //                                    int PriceAndTaxId = _context.OriginalPriceAndTaxs.Where(x => x.Name.Trim().Contains(tDCProjectPriceAndTaxDetails.IngrePriceName)).Select(x => x.Id).FirstOrDefault();
        //                                    if(PriceAndTaxId > 0)
        //                                    {
        //                                        tDCProjectPriceAndTaxDetails.PriceAndTaxId = PriceAndTaxId;
        //                                    }

        //                                    tDCProjectPriceAndTaxDetails.PriceAndTaxId = tDCProjectPriceAndTaxe.Id;
        //                                    tDCProjectPriceAndTaxDetails.CreatedBy = fullName;
        //                                    tDCProjectPriceAndTaxDetails.CreatedById = userId;

        //                                    _context.TDCProjectPriceAndTaxDetailss.Add(tDCProjectPriceAndTaxDetails);
        //                                }
        //                            }
        //                        }
        //                        await _context.SaveChangesAsync();
        //                    }

        //                    LogActionModel logActionModel = new LogActionModel("Thêm mới dự án: " + item.Code, "TDCProject", item.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
        //                    LogAction logAction = _mapper.Map<LogAction>(logActionModel);
        //                    _context.Add(logAction);
        //                    await _context.SaveChangesAsync();

        //                    def.meta = new Meta(200, ApiConstants.MessageResource.ADD_SUCCESS);
        //                    def.data = item;
        //                }
        //                transaction.Commit();
        //                return Ok(def);
        //            }
        //            catch (DbUpdateException e)
        //            {
        //                log.Error("DbUpdateException:" + e);
        //                transaction.Rollback();
        //                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
        //                return Ok(def);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        log.Error("Post Exception:" + e);
        //        def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
        //        return Ok(def);
        //    }
        //}
    }
}
