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
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using static IOITQln.Common.Enums.AppEnums;
using DevExpress.XtraPrinting;

namespace IOITQln.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LandPriceController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("LandPrice", "LandPrice");
        private static string functionCodeNoc = "LAND_PRICE_MANAGEMENT";
        private static string functionCode167 = "LAND_PRICE";
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;

        public LandPriceController(ApiDbContext context, IMapper mapper, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
        }

        [HttpGet("GetByPage/{landPriceType}")]
        public IActionResult GetByPage(landPriceType landPriceType, [FromQuery] FilteredLandPricePagination paging, [FromQuery] string txtSearch)
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

            string functionCode = landPriceType == landPriceType.NOC ? functionCodeNoc : functionCode167;
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
                    IQueryable<LandPrice> data;

                    if(txtSearch != "" && txtSearch != null)
                    {
                        data = (from d in _context.Districts
                                join l in _context.LandPricies on d.Id equals l.District
                                where d.Status != EntityStatus.DELETED && l.Status != EntityStatus.DELETED
                                && d.Name.Contains(txtSearch) && l.LandPriceType == paging.LandPriceType
                                select l);
                    }
                    else
                    {
                        data = _context.LandPricies.Where(c => c.Status != AppEnums.EntityStatus.DELETED && c.LandPriceType == paging.LandPriceType);
                    }

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
                        List<LandPriceData> res = _mapper.Map<List<LandPriceData>>(data.ToList());
                        foreach (LandPriceData item in res)
                        {
                            Decree decree_type1 = _context.Decreies.Where(d => d.Id == item.DecreeType1Id).FirstOrDefault();
                            item.DecreeType1Name = decree_type1 != null ? decree_type1.Code : "";

                            Decree decree_type2 = _context.Decreies.Where(d => d.Id == item.DecreeType2Id).FirstOrDefault();
                            item.DecreeType2Name = decree_type2 != null ? decree_type2.Code : "";

                            Province province = _context.Provincies.Where(l => l.Id == item.Province).FirstOrDefault();
                            item.ProvinceName = province != null ? province.Name : "";

                            District district = _context.Districts.Where(l => l.Id == item.District).FirstOrDefault();
                            item.DistrictName = district != null ? district.Name : "";

                            //Lane lane2 = _context.Lanies.Where(l => l.Id == item.LaneEndId).FirstOrDefault();
                            //item.LaneEndName = lane2 != null ? lane2.Name : "";
                            item.landPriceItems = _context.LandPriceItems.Where(l => l.LandPriceId == item.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
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

        // GET: api/LandPrice/1
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

            try
            {
                LandPrice data = await _context.LandPricies.FindAsync(id);

                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                    return Ok(def);
                }

                string functionCode = data.LandPriceType == landPriceType.NOC ? functionCodeNoc : functionCode167;
                if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
                {
                    def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
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

        // POST: api/LandPrice
        [HttpPost]
        public async Task<IActionResult> Post(landPriceType filterLandPrice ,LandPriceData input)
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

            string functionCode = filterLandPrice == landPriceType.NOC ? functionCodeNoc : functionCode167;
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.CREATE))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_CREATE_MESSAGE);
                return Ok(def);
            }

            try
            {
                input = (LandPriceData)UtilsService.TrimStringPropertyTypeObject(input);
                input.LandPriceType = filterLandPrice;
                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.CreatedById = userId;
                    input.CreatedBy = fullName;
                    _context.LandPricies.Add(input);

                    try
                    {
                        await _context.SaveChangesAsync();

                        //Pri
                        if (input.landPriceItems != null)
                        {
                            foreach (var landPriceItem in input.landPriceItems)
                            {
                                landPriceItem.LandPriceId = input.Id;
                                landPriceItem.CreatedBy = fullName;
                                landPriceItem.CreatedById = userId;

                                _context.LandPriceItems.Add(landPriceItem);
                            }
                        }

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Thêm mới Bảng giá đất", "LandPrice", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
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
                        if (LandPriceExists(input.Id))
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

        // PUT: api/LandPrice/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] LandPriceData input)
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

            try
            {
                input = (LandPriceData)UtilsService.TrimStringPropertyTypeObject(input);

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

                LandPrice data = await _context.LandPricies.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_UPDATE_MESSAGE);
                    return Ok(def);
                }

                string functionCode = data.LandPriceType == landPriceType.NOC ? functionCodeNoc : functionCode167;
                if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.UPDATE))
                {
                    def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_UPDATE_MESSAGE);
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

                        //PriceListItem
                        List<LandPriceItem> landPriceItems = _context.LandPriceItems.Where(l => l.LandPriceId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).AsNoTracking().ToList();
                        if (input.landPriceItems != null)
                        {
                            foreach (var landPriceItem in input.landPriceItems)
                            {
                                LandPriceItem landPriceItemExist = landPriceItems.Where(l => l.Id == landPriceItem.Id).FirstOrDefault();
                                if (landPriceItemExist == null)
                                {
                                    landPriceItem.LandPriceId = input.Id;
                                    landPriceItem.CreatedBy = fullName;
                                    landPriceItem.CreatedById = userId;

                                    _context.LandPriceItems.Add(landPriceItem);
                                }
                                else
                                {
                                    landPriceItem.LandPriceId = input.Id;
                                    landPriceItem.CreatedBy = landPriceItemExist.CreatedBy;
                                    landPriceItem.CreatedById = landPriceItemExist.CreatedById;
                                    landPriceItem.UpdatedBy = fullName;
                                    landPriceItem.UpdatedById = userId;

                                    _context.Update(landPriceItem);

                                    landPriceItems.Remove(landPriceItemExist);
                                }
                            }
                        }

                        if (landPriceItems.Count > 0)
                        {
                            landPriceItems.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = AppEnums.EntityStatus.DELETED;
                            });
                            _context.UpdateRange(landPriceItems);
                        }

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Sửa thông tin Bảng giá đất", "LandPrice", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, userId, fullName);
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
                        if (!LandPriceExists(data.Id))
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

        // DELETE: api/LandPrice/1
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

            try
            {
                LandPrice data = await _context.LandPricies.FindAsync(id);
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_DELETE_MESSAGE);
                    return Ok(def);
                }

                string functionCode = data.LandPriceType == landPriceType.NOC ? functionCodeNoc : functionCode167;
                if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.DELETED))
                {
                    def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_DELETE_MESSAGE);
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
                        LogActionModel logActionModel = new LogActionModel("Xóa thông tin Bảng giá đất", "LandPrice", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.DELETED, userId, fullName);
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
                        if (!LandPriceExists(data.Id))
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

        //GET: get LandPriceItem by DecreeType1Id and District
        [HttpGet("getLandPriceItems/{decreeType1Id}/{district}")]
        public async Task<IActionResult> GetLandPriceItems(int decreeType1Id, int district, [FromQuery] string txtSg = "")
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
            if (!CheckRole.CheckRoleByCode(access_key, functionCodeNoc, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }

            try
            {
                List<LandPriceItem> data = (from l in _context.LandPricies
                                            join li in _context.LandPriceItems on l.Id equals li.LandPriceId
                                            where l.Status != AppEnums.EntityStatus.DELETED
                                            && li.Status != AppEnums.EntityStatus.DELETED
                                            && l.DecreeType1Id == decreeType1Id && l.District == district
                                            select new LandPriceItem
                                            {
                                                Id = li.Id,
                                                LaneName = li.LaneName + (li.LaneStartName != null && li.LaneStartName != "" ? (" từ đoạn " + li.LaneStartName) : "") + (li.LaneEndName != null && li.LaneEndName != "" ? (" đến đoạn " + li.LaneEndName) : "") + (li.Des != null && li.Des != "" ? (", " + li.Des) : ""),
                                                Value = li.Value,
                                                Ward = li.LaneName.CompareTo(txtSg)
                                            }).OrderByDescending(x => x.Ward).ToList();

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.metadata = data.Count;
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

        //GET: get LandPriceItem by Multi DecreeType1Id and District
        [HttpPost("getLandPriceItemsMultiDecreeType1Id/{district}/{landPriceType}")]
        public async Task<IActionResult> getLandPriceItemsMultiDecreeType1Id(int district, landPriceType landPriceType, [FromBody] List<int> list_decree, [FromQuery] string txtSg = "")
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
            if (!CheckRole.CheckRoleByCode(access_key, functionCodeNoc, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }

            try
            {
                List<LandPriceItemDecreeData> data = (from l in _context.LandPricies
                                            join li in _context.LandPriceItems on l.Id equals li.LandPriceId
                                            where l.Status != AppEnums.EntityStatus.DELETED
                                            && li.Status != AppEnums.EntityStatus.DELETED
                                            && l.LandPriceType == landPriceType
                                            && list_decree.Contains(l.DecreeType1Id) && l.District == district
                                            select new LandPriceItemDecreeData
                                            {
                                                Id = li.Id,
                                                LaneName = li.LaneName + (li.LaneStartName != null && li.LaneStartName != "" ? (" từ đoạn " + li.LaneStartName) : "") + (li.LaneEndName != null && li.LaneEndName != "" ? (" đến đoạn " + li.LaneEndName) : "") + (li.Des != null && li.Des != "" ? (", " + li.Des) : ""),
                                                Value = li.Value,
                                                Ward = li.LaneName.CompareTo(txtSg),
                                                DecreeType1Id = l.DecreeType1Id
                                            }).OrderByDescending(x => x.Ward).ToList();

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.metadata = data.Count;
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

        #region import phiếu thu từ excel
        [HttpPost]
        [Route("ImportDataExcel/{landPriceType}")]
        public IActionResult ImportDataExcel(landPriceType landPriceType)
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

            string functionCode = landPriceType == landPriceType.NOC ? functionCodeNoc : functionCode167;
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
                importHistory.Type = landPriceType == landPriceType.NOC ? ImportHistoryType.Noc_Landprice : AppEnums.ImportHistoryType.Md167Landprice;

                List<LandPriceItemData> data = new List<LandPriceItemData>();

                List<Decree> decreeData = _context.Decreies.Where(e => e.TypeDecree == TypeDecree.THONG_TU_VAN_BAN && e.Status != EntityStatus.DELETED).ToList();
                List<District> districtData = _context.Districts.Where(e => e.Status != EntityStatus.DELETED).ToList();
                List<Province> provinceData = _context.Provincies.Where(e => e.Status != EntityStatus.DELETED).ToList();
                List<LandPrice> landPriceData = _context.LandPricies.Where(e => e.Status != EntityStatus.DELETED).ToList();

                foreach (var file in httpRequest)
                {
                    var postedFile = httpRequest[i];
                    if(postedFile != null && postedFile.Length > 0)
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
                                    data = importData(ms, 0, 2, decreeData, districtData, provinceData);
                                }
                            }
                        }
                    }
                    i++;
                }

                List<LandPriceItemData> dataValid = data.Where(e => e.Valid).ToList();
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        //group data theo Nghị định - Văn bản pl - căn cứ - Quận huyện - Tỉnh thành
                        var groupDataValid = dataValid.GroupBy(x => new { x.DecreeType1Id, x.DecreeType2Id, x.DeslandPriceNoneUnicode, x.DistrictId, x.ProviceId }).ToList();
                        foreach( var groupDataValidItem in groupDataValid)
                        {
                            var key = groupDataValidItem.Key;

                            //Kiểm tra bảng LandPrice có tồn tại item tương ứng
                            LandPrice landPrice = landPriceData.AsEnumerable().Where(e => e.LandPriceType == landPriceType && e.DecreeType1Id == key.DecreeType1Id && e.DecreeType2Id == key.DecreeType2Id && UtilsService.NonUnicode(e.Des) == key.DeslandPriceNoneUnicode && e.District == key.DistrictId && e.Province == key.ProviceId).FirstOrDefault();
                            if(landPrice == null)
                            {
                                landPrice = new LandPrice();
                                landPrice.LandPriceType = landPriceType;
                                landPrice.DecreeType1Id = key.DecreeType1Id;
                                landPrice.DecreeType2Id = key.DecreeType2Id;
                                landPrice.Des = groupDataValidItem.FirstOrDefault().DeslandPrice;
                                landPrice.District = key.DistrictId;
                                landPrice.Province = key.ProviceId;
                                landPrice.CreatedById = userId;
                                landPrice.CreatedBy = $"{fullName} (Excel)";

                                _context.LandPricies.Add(landPrice);
                                _context.SaveChanges();
                            }

                            //Thêm LandPriceItem
                            List<LandPriceItem> landPriceItems = new List<LandPriceItem>();
                            groupDataValidItem.ToList().ForEach(x => {
                                LandPriceItem landPriceItem = new LandPriceItem();
                                landPriceItem.LandPriceId = landPrice.Id;
                                landPriceItem.LaneName = x.LaneName;
                                landPriceItem.LaneStartName = x.LaneStartName;
                                landPriceItem.LaneEndName = x.LaneEndName;
                                landPriceItem.Value = x.Value;
                                landPriceItem.Des = x.Des;
                                landPriceItem.CreatedById = userId;
                                landPriceItem.CreatedBy = $"{fullName} (Excel)";

                                landPriceItems.Add(landPriceItem);
                            });

                            _context.LandPriceItems.AddRange(landPriceItems);
                            _context.SaveChanges();
                        }

                        importHistory.Data = data.Cast<dynamic>().ToList();
                        importHistory.CreatedById = userId;
                        importHistory.CreatedBy = fullName;

                        _context.ImportHistories.Add(importHistory);
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

        public static List<LandPriceItemData> importData(MemoryStream ms, int sheetnumber, int rowStart, List<Decree> decreeType2Data, List<District> districtData, List<Province> provinceData)
        {
            XSSFWorkbook workbook = new XSSFWorkbook(ms);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);

            List<LandPriceItemData> res = new List<LandPriceItemData>();
            for (int row = rowStart; row <= sheet.LastRowNum; row++)
            {
                if (sheet.GetRow(row) != null)
                {
                    //Đọc dữ liệu từng cell
                    LandPriceItemData inputDetail = new LandPriceItemData();
                    inputDetail.Valid = true;
                    inputDetail.ErrMsg = "";

                    for (int i = 0; i < 11; i++)
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
                                        inputDetail.DecreeType1Name = str;
                                        string type_decree = UtilsService.NonUnicode(str);
                                        if (type_decree == "99-2015-nd-cp")
                                        {
                                            inputDetail.DecreeType1Id = (int)DecreeEnum.ND_CP_99_2015;
                                        }
                                        else if (type_decree == "34-2013-nd-cp")
                                        {
                                            inputDetail.DecreeType1Id = (int)DecreeEnum.ND_CP_34_2013;
                                        }
                                        else if (type_decree == "61-nd-cp")
                                        {
                                            inputDetail.DecreeType1Id = (int)DecreeEnum.ND_CP_61;
                                        }
                                        else
                                        {
                                            inputDetail.Valid = false;
                                            inputDetail.ErrMsg += "Cột Nghị định không hợp lệ\n";
                                        }
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột nghị định chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột nghị định\n";
                                }
                            }
                            else if (i == 2)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.DecreeType2Name = str;

                                        //Kiểm tra dữ liệu pháp luật liên quan có hợp lệ
                                        string strNoneUnicode = UtilsService.NonUnicode(str);
                                        Decree decree = decreeType2Data.AsEnumerable().Where(e => UtilsService.NonUnicode(e.Code) == strNoneUnicode).FirstOrDefault();
                                        if(decree != null)
                                        {
                                            inputDetail.DecreeType2Id = decree.Id;
                                        }
                                        else
                                        {
                                            inputDetail.Valid = false;
                                            inputDetail.ErrMsg += "Cột Số văn bản pháp luật liên quan không hợp lệ\n";
                                        }
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột Số văn bản pháp luật liên quan chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Số văn bản pháp luật liên quan\n";
                                }
                            }
                            else if (i == 3)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.DeslandPrice = str;
                                        inputDetail.DeslandPriceNoneUnicode = UtilsService.NonUnicode(str);
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột căn cứ chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột căn cứ\n";
                                }
                            }
                            else if (i == 4)
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
                                        inputDetail.ErrMsg += "Cột Quận (huyện) chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Quận (huyện)\n";
                                }
                            }
                            else if (i == 5)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.ProviceName = str;
                                        string strNoneUnicode = UtilsService.NonUnicode(str);
                                        Province province = provinceData.AsEnumerable().Where(e => UtilsService.NonUnicode(e.Name).Contains(strNoneUnicode)).FirstOrDefault();
                                        if(province != null)
                                        {
                                            inputDetail.ProviceId = province.Id;

                                            //kiểm tra thông tin quận có hợp lệ
                                            string strDistrictNameNoneUnicode = UtilsService.NonUnicode(inputDetail.DistrictName);
                                            District district = districtData.AsEnumerable().Where(e => e.ProvinceId == province.Id && UtilsService.NonUnicode(e.Name).Contains(strDistrictNameNoneUnicode)).FirstOrDefault();
                                            if (district != null)
                                            {
                                                inputDetail.DistrictId = district.Id;
                                            }
                                            else
                                            {
                                                inputDetail.Valid = false;
                                                inputDetail.ErrMsg += "Cột thông tin Quận/huyện không hợp lệ\n";
                                            }
                                        }
                                        else
                                        {
                                            inputDetail.Valid = false;
                                            inputDetail.ErrMsg += "Cột Tỉnh/thành phố chưa có dữ liệu\n";
                                        }

                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột Tỉnh (thành phố) chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Tỉnh (thành phố)\n";
                                }
                            }
                            else if (i == 6)
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
                                        inputDetail.ErrMsg += "Cột tên đường chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột tên đường\n";
                                }
                            }
                            else if (i == 7)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.LaneStartName = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột đoạn đường từ chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột đoạn đường từ\n";
                                }
                            }
                            else if (i == 8)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.LaneEndName = str;
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột đoạn đường đến\n";
                                }
                            }
                            else if (i == 9)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.Value = double.Parse(str);
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột giá chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột giá\n";
                                }
                            }
                            else if( i== 10)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.Des = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột căn cứ theo chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột căn cứ theo \n";
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
        private bool LandPriceExists(int id)
        {
            return _context.LandPricies.Count(e => e.Id == id) > 0;
        }

        private string GetLandPriceMd167(int id)
        {
            var lane =  _context.Lanies.Find(id);
            if (lane != null)
            {
                LandPriceItem dataItem = new LandPriceItem();
                string strNoneUnicode = UtilsService.NonUnicode(lane.Name);
                var data = _context.LandPricies.Where(c => c.Status != AppEnums.EntityStatus.DELETED && c.LandPriceType == landPriceType.MD167).ToList();
                foreach (var item in data)
                {
                    dataItem = new LandPriceItem();
                    dataItem = _context.LandPriceItems.Where(x=>x.LandPriceId == item.Id && UtilsService.NonUnicode(x.LaneName)== strNoneUnicode).FirstOrDefault();
                    if (dataItem != null)
                    {
                        break;
                    }
                }
                if(dataItem == null)
                {

                }
            }
            return "";
        }
    }
}
