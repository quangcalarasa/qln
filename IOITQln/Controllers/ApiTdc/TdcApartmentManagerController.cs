using AutoMapper;
using DevExpress.ClipboardSource.SpreadsheetML;
using DevExpress.Utils.Extensions;
using DevExpress.XtraPrinting;
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
using Microsoft.EntityFrameworkCore.Internal;
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
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace IOITQln.Controllers.ApiTdc
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TdcApartmentManagerController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("report_Apartment", "report_Apartment");
        private static string functionCode = "report1_TDC_Apartment";
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        private IHostingEnvironment _hostingEnvironment;

        public TdcApartmentManagerController(ApiDbContext context, IMapper mapper, IHostingEnvironment hostingEnvironment)
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
                    IQueryable<TdcApartmentManager> data = _context.TdcApartmentManagers.Where(c => c.Status != AppEnums.EntityStatus.DELETED);
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
                        List<TdcApartmentManagerData> res = _mapper.Map<List<TdcApartmentManagerData>>(data.ToList());
                        foreach (var item in res)
                        {
                            item.TdcProjectName = _context.TDCProjects.Where(f => f.Id == item.TdcProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            item.TdcLandName = _context.Lands.Where(f => f.Id == item.LandId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            item.TdcBlockHouseName = _context.BlockHouses.Where(f => f.Id == item.BlockHouseId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            item.TdcFloorName = _context.FloorTdcs.Where(f => f.Id == item.FloorTdcId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            item.TdcApartmentName = _context.ApartmentTdcs.Where(f => f.Id == item.ApartmentTdcId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            item.DistrictProjectName = _context.Districts.Where(f => f.Id == item.DistrictProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            item.TypeDecisionName = _context.TypeAttributeItems.Where(f => f.Id == item.TypeDecisionId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            item.TypeLegalName = _context.TypeAttributeItems.Where(f => f.Id == item.TypeLegalId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            List<DistrictAllocasionApartment> districtAllocasionApartments = _context.DistrictAllocasionApartments.Where(l => l.TdcApartmentManagerId == item.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                            List<DistrictAllocasionApartmentData> map_districtAllocasionApartmentDatas = _mapper.Map<List<DistrictAllocasionApartmentData>>(districtAllocasionApartments);
                            foreach (DistrictAllocasionApartmentData map_districtAllocasionApartmentData in map_districtAllocasionApartmentDatas)
                            {
                                District district = _context.Districts.Where(f => f.Id == map_districtAllocasionApartmentData.DistrictId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                if (district == null) map_districtAllocasionApartmentData.DistrictName = "";
                                else map_districtAllocasionApartmentData.DistrictName = district != null ? district.Name : "";
                            }
                            item.districtAllocasionApartment = map_districtAllocasionApartmentDatas;
                            item.districtAllocasionApartmentName = string.Join(" , ", item.districtAllocasionApartment.Select(s => s.DistrictName));
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
            var identity = (ClaimsIdentity)User.Identity;
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }

            try
            {
                TdcApartmentManager data = await _context.TdcApartmentManagers.FindAsync(id);

                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                    return Ok(def);
                }

                TdcApartmentManagerData res = _mapper.Map<TdcApartmentManagerData>(data);

                List<DistrictAllocasionApartment> districtAllocasionApartments = _context.DistrictAllocasionApartments.Where(l => l.TdcApartmentManagerId == res.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<DistrictAllocasionApartmentData> map_districtAllocasionApartmentDatas = _mapper.Map<List<DistrictAllocasionApartmentData>>(districtAllocasionApartments);
                foreach (DistrictAllocasionApartmentData map_districtAllocasionApartmentData in map_districtAllocasionApartmentDatas)
                {
                    District district = _context.Districts.Where(f => f.Id == map_districtAllocasionApartmentData.TdcApartmentManagerId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    if (district == null) map_districtAllocasionApartmentData.DistrictName = "";
                    else map_districtAllocasionApartmentData.DistrictName = district != null ? district.Name : "";
                }
                res.districtAllocasionApartment = map_districtAllocasionApartmentDatas;
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
        public async Task<IActionResult> Post(TdcApartmentManagerData input)
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
                input = (TdcApartmentManagerData)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                var dataProject = _context.TDCProjects.Where(x => x.Id == input.TdcProjectId && x.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                if (dataProject == null)
                {
                    def.meta = new Meta(400, "Không tồn tại dự án");
                    return Ok(def);
                }

                var dataLand = _context.Lands.Where(x => x.Id == input.LandId && x.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                if (dataLand == null)
                {
                    def.meta = new Meta(400, "Không tồn tại Lô");
                    return Ok(def);
                }

                var dataBlockHouse = _context.BlockHouses.Where(x => x.Id == input.BlockHouseId && x.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                if (dataBlockHouse == null)
                {
                    def.meta = new Meta(400, "Không tồn tại Khối");
                    return Ok(def);
                }

                var dataFloorTdc = _context.FloorTdcs.Where(x => x.Id == input.FloorTdcId && x.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                if (dataFloorTdc == null)
                {
                    def.meta = new Meta(400, "Không tồn tại Tầng");
                    return Ok(def);
                }

                var dataApartmentTdc = _context.ApartmentTdcs.Where(x => x.Id == input.ApartmentTdcId && x.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                if (dataApartmentTdc == null)
                {
                    def.meta = new Meta(400, "Không tồn tại căn hộ");
                    return Ok(def);
                }

                //TdcApartmentManager tdcApartmentManagerExist = _context.TdcApartmentManagers.Where(f => f.Identifier == input.Identifier && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                //{
                //    if(tdcApartmentManagerExist != null)
                //    {
                //        def.meta = new Meta(211, "Đã tồn tại mã định danh Dự án " + input.Identifier + "!");
                //        return Ok(def);
                //    }
                //}

                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.CreatedById = userId;
                    input.CreatedBy = fullName;
                    _context.TdcApartmentManagers.Add(input);

                    try
                    {
                        await _context.SaveChangesAsync();
                        //thêm mới danh sách quận/huyện và số lượng thực tế theo phân bổ (căn hộ)
                        if (input.districtAllocasionApartment == null)
                        {
                            foreach (var districtAllocasionApartments in input.districtAllocasionApartment)
                            {
                                districtAllocasionApartments.TdcApartmentManagerId = input.Id;
                                districtAllocasionApartments.CreatedBy = fullName;
                                districtAllocasionApartments.CreatedById = userId;

                                _context.DistrictAllocasionApartments.Add(districtAllocasionApartments);
                            }
                            await _context.SaveChangesAsync();
                        }

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Thêm mới biểu mẫu căn hộ ", "TdcApartmentManager", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
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
                        if (TdcApartmentManagerExists(input.Id))
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
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] TdcApartmentManagerData input)
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
                input = (TdcApartmentManagerData)UtilsService.TrimStringPropertyTypeObject(input);

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

                TdcApartmentManager data = await _context.TdcApartmentManagers.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_UPDATE_MESSAGE);
                    return Ok(def);
                }

                //TdcApartmentManager tdcApartmentManagerExist = _context.TdcApartmentManagers.Where(f => f.Identifier == input.Identifier && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                //{
                //    if (tdcApartmentManagerExist != null)
                //    {
                //        def.meta = new Meta(211, "Đã tồn tại mã định danh Dự án " + input.Identifier + "!");
                //        return Ok(def);
                //    }
                //}

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
                        List<DistrictAllocasionApartment> districtAllocasionApartment = _context.DistrictAllocasionApartments.Where(l => l.TdcApartmentManagerId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                        if (input.districtAllocasionApartment != null)
                        {
                            foreach (var districtAllocasionApartments in input.districtAllocasionApartment)
                            {
                                DistrictAllocasionApartment districtAllocasionApartmentExist = districtAllocasionApartment.Where(l => l.Id == districtAllocasionApartments.Id).FirstOrDefault();
                                if (districtAllocasionApartmentExist == null)
                                {
                                    districtAllocasionApartments.TdcApartmentManagerId = input.Id;
                                    districtAllocasionApartments.CreatedBy = fullName;
                                    districtAllocasionApartments.CreatedById = userId;

                                    _context.DistrictAllocasionApartments.Add(districtAllocasionApartments);
                                }
                                else
                                {
                                    districtAllocasionApartments.CreatedAt = districtAllocasionApartmentExist.CreatedAt;
                                    districtAllocasionApartments.CreatedBy = districtAllocasionApartmentExist.CreatedBy;
                                    districtAllocasionApartments.CreatedById = districtAllocasionApartmentExist.CreatedById;
                                    districtAllocasionApartments.TdcApartmentManagerId = input.Id;
                                    districtAllocasionApartments.UpdatedBy = fullName;
                                    districtAllocasionApartments.UpdatedById = userId;

                                    _context.Update(districtAllocasionApartments);

                                    districtAllocasionApartment.Remove(districtAllocasionApartmentExist);
                                }
                            }
                        }
                        if (districtAllocasionApartment.Count > 0)
                            districtAllocasionApartment.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = AppEnums.EntityStatus.DELETED;
                            });
                        _context.UpdateRange(districtAllocasionApartment);

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Sửa biểu mẫu căn hộ ", "TdcApartmentManager", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, userId, fullName);
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
                        if (!TdcApartmentManagerExists(data.Id))
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
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

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
                TdcApartmentManager data = await _context.TdcApartmentManagers.FindAsync(id);
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

                    List<DistrictAllocasionApartment> districtAllocasionApartment = _context.DistrictAllocasionApartments.Where(l => l.TdcApartmentManagerId == data.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if (districtAllocasionApartment.Count > 0)
                    {
                        districtAllocasionApartment.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(districtAllocasionApartment);
                    }

                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Xóa báo cáo căn hộ ", "TdcApartmentManager", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.DELETED, userId, fullName);
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
                        if (!TdcApartmentManagerExists(data.Id))
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

        private bool TdcApartmentManagerExists(int id)
        {
            return _context.Templaties.Count(e => e.Id == id) > 0;
        }

        [HttpPost("ExcelRP2")]
        public async Task<IActionResult> ExcelRP2(int TypeLegalId)
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

            List<TdcApartmentManager> tdcApartmentManagersRP2 = _context.TdcApartmentManagers.Where(x => x.TypeLegalId == TypeLegalId && x.Status != AppEnums.EntityStatus.DELETED).ToList();
            List<TdcApartmentManagerData> mapper_ApartmentRP2 = _mapper.Map<List<TdcApartmentManagerData>>(tdcApartmentManagersRP2);
            foreach (var map in mapper_ApartmentRP2)
            {
                map.TdcProjectName = _context.TDCProjects.Where(f => f.Id == map.TdcProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                map.TypeLegalName = _context.TypeAttributeItems.Where(f => f.Id == map.TypeLegalId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                map.TypeDecisionName = _context.TypeAttributeItems.Where(f => f.Id == map.TypeDecisionId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                map.DistrictProjectName = _context.Districts.Where(f => f.Id == map.DistrictProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
            }

            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"ReportTDC/RP-2-Apartment.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);
            string nameExcel = "BangBaoCao_.xls";

            MemoryStream ms = WriteDataToExcel2(templatePath, 0, mapper_ApartmentRP2);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", nameExcel);

        }
        private static MemoryStream WriteDataToExcel2(string templatePath, int sheetnumber, List<TdcApartmentManagerData> data)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();

            int rowTitle = 6;
            int rowStart = 10;
            int rowStart1 = 11;

            if (sheet != null)
            {
                try
                {
                    int k = 0;
                    int datacol = 0;

                    datacol = 8;

                    var style = workbook.CreateCellStyle();
                    style.BorderBottom = BorderStyle.Thin;
                    style.BorderLeft = BorderStyle.Thin;
                    style.BorderRight = BorderStyle.Thin;
                    style.BorderTop = BorderStyle.Thin;

                    ICellStyle cellStyle = workbook.CreateCellStyle();
                    IFont font = workbook.CreateFont();
                    font.FontName = "Times New Roman";
                    cellStyle.SetFont(font);
                    cellStyle.Alignment = HorizontalAlignment.Center;
                    cellStyle.BorderBottom = BorderStyle.Thin;
                    cellStyle.BorderLeft = BorderStyle.Thin;
                    cellStyle.BorderRight = BorderStyle.Thin;
                    cellStyle.BorderTop = BorderStyle.Thin;

                    IRow rowtitle = sheet.CreateRow(rowTitle);

                    int totalCountDecision = 0;
                    int totalCountReceived = 0;
                    int totalCountReceivedYet = 0;
                    int totalCountNotReceived = 0;

                    var groupbyDecision = data.GroupBy(x => x.TypeLegalName).ToList();

                    var currentDate = DateTime.Now;
                    var firstTypeDecision = data.First().TypeLegalName;

                    ICell cellTitle = rowtitle.CreateCell(2);
                    cellTitle.SetCellValue($"DANH SÁCH TIẾP NHẬN QUỸ NHÀ Ở, ĐẤT Ở PHỤC VỤ TÁI ĐỊNH CƯ THEO {firstTypeDecision} CỦA ỦY BAN NHÂN DÂN THÀNH PHỐ HỒ CHÍ MINH\r\n(Số liệu tính đến ngày {currentDate.ToString("dd/MM/yyyy")})");
                    cellTitle.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader2 = new CellRangeAddress(rowTitle, rowTitle, 2, 9);
                    RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader2, sheet);
                    RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader2, sheet);
                    RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader2, sheet);
                    RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader2, sheet);
                    sheet.AddMergedRegion(mergedRegioncellHeader2);

                    foreach (var itemDecison in groupbyDecision)
                    {
                        var groupbyDistrictProject = itemDecison.GroupBy(x => x.DistrictProjectName).ToList();
                        int districtcount = 1;

                        foreach (var itemDistrict in groupbyDistrictProject)
                        {
                            IRow row = sheet.CreateRow(rowStart);
                            IRow row1 = sheet.CreateRow(rowStart1);

                            ICellStyle cellStyleDate = workbook.CreateCellStyle();
                            IFont fontDate = workbook.CreateFont();
                            fontDate.FontName = "Times New Roman";
                            cellStyleDate.SetFont(fontDate);
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

                            var receptionTimes = itemDistrict.GroupBy(x => x.ReceptionTime).Select(f => new
                            {
                                ReceptionType = f.Key,
                                Count = f.Count()
                            });

                            int countReceived = 0;
                            int countReceivedYet = 0;
                            int countNotReceived = 0;

                            string districtProjectName = itemDistrict.Key;
                            int countDecison = itemDistrict.Count();

                            foreach (var receptionTime in receptionTimes)
                            {
                                if (receptionTime.ReceptionType.ToString() == "Received")
                                {
                                    countReceived = receptionTime.Count;
                                }
                                else if (receptionTime.ReceptionType.ToString() == "ReceivedYet")
                                {
                                    countReceivedYet = receptionTime.Count;
                                }
                                else if (receptionTime.ReceptionType.ToString() == "NotReceived")
                                {
                                    countNotReceived = receptionTime.Count;
                                }
                            }

                            totalCountDecision += countDecison;
                            totalCountReceived += countReceived;
                            totalCountReceivedYet += countReceivedYet;
                            totalCountNotReceived += countNotReceived;


                            ICell cell = row1.CreateCell(2);
                            cell.SetCellValue(districtcount++);
                            cell.CellStyle = cellStyle;

                            ICell cell1 = row1.CreateCell(3);
                            cell1.SetCellValue(districtProjectName);
                            cell1.CellStyle = cellStyle;

                            ICell cell2 = row1.CreateCell(4);
                            cell2.SetCellValue("");
                            cell2.CellStyle = cellStyle;

                            ICell cell3 = row1.CreateCell(5);
                            cell3.SetCellValue(countDecison);
                            cell3.CellStyle = cellStyle;

                            ICell cell4 = row1.CreateCell(6);
                            cell4.SetCellValue(countReceived);
                            cell4.CellStyle = cellStyle;

                            ICell cell5 = row1.CreateCell(7);
                            cell5.SetCellValue(countReceivedYet);
                            cell5.CellStyle = cellStyle;

                            ICell cell6 = row1.CreateCell(8);
                            cell6.SetCellValue(countNotReceived);
                            cell6.CellStyle = cellStyle;

                            ICell cell7 = row1.CreateCell(9);
                            cell7.SetCellValue("");
                            cell7.CellStyle = cellStyle;

                            rowStart1++;
                            k = 0;

                            var groupbyProjects = itemDistrict.GroupBy(x => x.TdcProjectName);

                            foreach (var project in groupbyProjects)
                            {

                                IRow row2 = sheet.CreateRow(rowStart1);
                                var ReceptionTimes = project.GroupBy(x => x.ReceptionTime).Select(f => new
                                {
                                    ReceptionType = f.Key,
                                    Count = f.Count()
                                });

                                int countReceived1 = 0;
                                int countReceivedYet1 = 0;
                                int countNotReceived1 = 0;

                                string tdcProjectName = project.Key;
                                int countDecison1 = project.Count();

                                foreach (var recepTime in ReceptionTimes)
                                {
                                    if (recepTime.ReceptionType.ToString() == "Received")
                                    {
                                        countReceived1 = recepTime.Count;
                                    }
                                    else if (recepTime.ReceptionType.ToString() == "ReceivedYet")
                                    {
                                        countReceivedYet1 = recepTime.Count;
                                    }
                                    else if (recepTime.ReceptionType.ToString() == "NotReceived")
                                    {
                                        countNotReceived1 = recepTime.Count;
                                    }
                                }

                                ICell cellprojectnull1 = row2.CreateCell(2);
                                cellprojectnull1.SetCellValue(" ");
                                cellprojectnull1.CellStyle = cellStyle;

                                ICell cellprojectnull2 = row2.CreateCell(3);
                                cellprojectnull2.SetCellValue(" ");
                                cellprojectnull2.CellStyle = cellStyle;

                                ICell cellproject = row2.CreateCell(4);
                                cellproject.SetCellValue(tdcProjectName);
                                cellproject.CellStyle = cellStyle;

                                ICell cellproject1 = row2.CreateCell(5);
                                cellproject1.SetCellValue(countDecison1);
                                cellproject1.CellStyle = cellStyle;

                                ICell cellproject2 = row2.CreateCell(6);
                                cellproject2.SetCellValue(countReceived1);
                                cellproject2.CellStyle = cellStyle;

                                ICell cellproject3 = row2.CreateCell(7);
                                cellproject3.SetCellValue(countReceivedYet1);
                                cellproject3.CellStyle = cellStyle;

                                ICell cellproject4 = row2.CreateCell(8);
                                cellproject4.SetCellValue(countNotReceived1);
                                cellproject4.CellStyle = cellStyle;

                                ICell cellprojectnull3 = row2.CreateCell(9);
                                cellprojectnull3.SetCellValue(" ");
                                cellprojectnull3.CellStyle = cellStyle;
                                rowStart1++;

                            }

                        }

                    }

                    IRow rowTotal = sheet.CreateRow(rowStart);

                    ICell cellTotal5 = rowTotal.CreateCell(2);
                    cellTotal5.SetCellValue("TỔNG CỘNG");
                    cellTotal5.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader1 = new CellRangeAddress(rowStart, rowStart, 2, 4);
                    RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                    RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                    RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                    RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                    sheet.AddMergedRegion(mergedRegioncellHeader1);

                    ICell cellTotal1 = rowTotal.CreateCell(5);
                    cellTotal1.SetCellValue(totalCountDecision);
                    cellTotal1.CellStyle = cellStyle;

                    ICell cellTotal2 = rowTotal.CreateCell(6);
                    cellTotal2.SetCellValue(totalCountReceived);
                    cellTotal2.CellStyle = cellStyle;

                    ICell cellTotal3 = rowTotal.CreateCell(7);
                    cellTotal3.SetCellValue(totalCountReceivedYet);
                    cellTotal3.CellStyle = cellStyle;

                    ICell cellTotal4 = rowTotal.CreateCell(8);
                    cellTotal4.SetCellValue(totalCountNotReceived);
                    cellTotal4.CellStyle = cellStyle;

                    ICell cellTotal6 = rowTotal.CreateCell(9);
                    cellTotal6.SetCellValue("");
                    cellTotal6.CellStyle = cellStyle;
                }
                catch (Exception ex)
                {
                    log.Error("Put Exception:" + ex);
                }
            }
            sheet.ForceFormulaRecalculation = true;
            MemoryStream mss = new MemoryStream();
            workbook.Write(mss);

            return mss;
        }

        [HttpPost("ExcelRP3")]
        public async Task<IActionResult> ExcelRP3()
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

            List<TdcApartmentManager> tdcApartmentManagersRP3 = _context.TdcApartmentManagers.Where(x => x.Status != AppEnums.EntityStatus.DELETED).ToList();
            List<TdcApartmentManagerData> mapper_ApartmentRP3 = _mapper.Map<List<TdcApartmentManagerData>>(tdcApartmentManagersRP3);
            foreach (var map in mapper_ApartmentRP3)
            {
                map.TypeLegalName = _context.TypeAttributeItems.Where(f => f.Id == map.TypeLegalId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
            }

            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"ReportTDC/Apartment-RP3.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);
            string nameExcel = "BangBaoCao_.xls";

            MemoryStream ms = WriteDataToExcel3(templatePath, 0, mapper_ApartmentRP3);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", nameExcel);

        }
        private static MemoryStream WriteDataToExcel3(string templatePath, int sheetnumber, List<TdcApartmentManagerData> data)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();

            int rowStart = 8;

            int totalTypeDecision = 0;
            int totalProjects = 0;
            int totalReceived = 0;
            int totalReceivedYet = 0;
            int totalNotReceived = 0;
            int totalhandoveryear = 0;
            int totalhandyearnull = 0;

            if (sheet != null)
            {
                try
                {
                    int k = 0;
                    int datacol = 0;

                    datacol = 9;

                    var style = workbook.CreateCellStyle();
                    style.BorderBottom = BorderStyle.Thin;
                    style.BorderLeft = BorderStyle.Thin;
                    style.BorderRight = BorderStyle.Thin;
                    style.BorderTop = BorderStyle.Thin;

                    ICellStyle cellStyle = workbook.CreateCellStyle();
                    IFont font = workbook.CreateFont();
                    font.FontName = "Times New Roman";
                    cellStyle.SetFont(font);
                    cellStyle.Alignment = HorizontalAlignment.Center;
                    cellStyle.BorderBottom = BorderStyle.Thin;
                    cellStyle.BorderLeft = BorderStyle.Thin;
                    cellStyle.BorderRight = BorderStyle.Thin;
                    cellStyle.BorderTop = BorderStyle.Thin;


                    var groupbyDecision = data.GroupBy(x => x.TypeLegalName);
                    int typedecisioncount = 1;
                    foreach (var itemDecison in groupbyDecision)
                    {
                        IRow row = sheet.CreateRow(rowStart);
                        var receptionTimes = itemDecison.GroupBy(x => x.ReceptionTime).Select(f => new
                        {
                            ReceptionType = f.Key,
                            Count = f.Count()
                        });
                        string typeDecisionName = itemDecison.Key;
                        int totalProjectCount = itemDecison.Count();
                        int countReceived = 0;
                        int countReceivedYet = 0;
                        int countNotReceived = 0;
                        int handoveryear = 0;
                        int handyearnull = 0;




                        var grouphandover = itemDecison.GroupBy(x => x.HandOverYear);

                        foreach (var overyear in grouphandover)
                        {
                            if (overyear.Key != null)
                            {
                                handoveryear += overyear.Count();
                            }
                            else
                            {
                                handyearnull += overyear.Count();
                            }
                        }


                        foreach (var receptionTime in receptionTimes)
                        {
                            if (receptionTime.ReceptionType.ToString() == "Received")
                            {
                                countReceived = receptionTime.Count;
                            }
                            else if (receptionTime.ReceptionType.ToString() == "ReceivedYet")
                            {
                                countReceivedYet = receptionTime.Count;
                            }
                            else
                            {
                                countNotReceived = receptionTime.Count;
                            }
                        }

                        totalTypeDecision++;
                        totalProjects += totalProjectCount;
                        totalReceived += countReceived;
                        totalReceivedYet += countReceivedYet;
                        totalNotReceived += countNotReceived;
                        totalhandoveryear += handoveryear;
                        totalhandyearnull += handyearnull;

                        ICell cell = row.CreateCell(0);
                        cell.SetCellValue(typedecisioncount++);
                        cell.CellStyle = cellStyle;

                        ICell cell1 = row.CreateCell(1);
                        cell1.SetCellValue(typeDecisionName);
                        cell1.CellStyle = cellStyle;

                        ICell cell2 = row.CreateCell(2);
                        cell2.SetCellValue(totalProjectCount);
                        cell2.CellStyle = cellStyle;

                        ICell cell3 = row.CreateCell(3);
                        cell3.SetCellValue(countReceived);
                        cell3.CellStyle = cellStyle;

                        ICell cell4 = row.CreateCell(4);
                        cell4.SetCellValue(countReceivedYet);
                        cell4.CellStyle = cellStyle;

                        ICell cell5 = row.CreateCell(5);
                        cell5.SetCellValue(countNotReceived);
                        cell5.CellStyle = cellStyle;

                        ICell cell6 = row.CreateCell(6);
                        cell6.SetCellValue(handoveryear);
                        cell6.CellStyle = cellStyle;

                        ICell cell7 = row.CreateCell(7);
                        cell7.SetCellValue(handyearnull);
                        cell7.CellStyle = cellStyle;

                        ICell cell8 = row.CreateCell(8);
                        cell8.SetCellValue("");
                        cell8.CellStyle = cellStyle;

                        rowStart++;
                    }

                    IRow totalRow = sheet.CreateRow(rowStart);

                    ICell celltotal = totalRow.CreateCell(0);
                    celltotal.SetCellValue("TỔNG CỘNG");
                    celltotal.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader1 = new CellRangeAddress(rowStart, rowStart, 0, 1);
                    RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                    RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                    RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                    RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                    sheet.AddMergedRegion(mergedRegioncellHeader1);

                    ICell celltotal1 = totalRow.CreateCell(2);
                    celltotal1.SetCellValue(totalProjects);
                    celltotal1.CellStyle = cellStyle;

                    ICell celltotal2 = totalRow.CreateCell(3);
                    celltotal2.SetCellValue(totalReceived);
                    celltotal2.CellStyle = cellStyle;

                    ICell celltotal3 = totalRow.CreateCell(4);
                    celltotal3.SetCellValue(totalReceivedYet);
                    celltotal3.CellStyle = cellStyle;

                    ICell celltotal4 = totalRow.CreateCell(5);
                    celltotal4.SetCellValue(totalNotReceived);
                    celltotal4.CellStyle = cellStyle;

                    ICell celltotal5 = totalRow.CreateCell(6);
                    celltotal5.SetCellValue(totalhandoveryear);
                    celltotal5.CellStyle = cellStyle;

                    ICell celltotal6 = totalRow.CreateCell(7);
                    celltotal6.SetCellValue(totalhandyearnull);
                    celltotal6.CellStyle = cellStyle;

                    ICell celltotal7 = totalRow.CreateCell(8);
                    celltotal7.SetCellValue("");
                    celltotal7.CellStyle = cellStyle;
                }
                catch (Exception ex)
                {
                    log.Error("Put Exception:" + ex);
                }
            }
            sheet.ForceFormulaRecalculation = true;
            MemoryStream ms1 = new MemoryStream();
            workbook.Write(ms1);

            return ms1;
        }

        [HttpPost("ExportReport1TdcApartment")]
        public async Task<IActionResult> ExportReport1TdcApartment()
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

            try
            {
                List<TdcApartmentManager> tdcApartmentManagers = _context.TdcApartmentManagers.Where(x => x.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<TdcApartmentManagerData> mapper_ApartmentRP = _mapper.Map<List<TdcApartmentManagerData>>(tdcApartmentManagers.ToList());
                foreach (TdcApartmentManagerData map in mapper_ApartmentRP)
                {
                    map.TdcProjectName = _context.TDCProjects.Where(f => f.Id == map.TdcProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                    map.TdcLandName = _context.Lands.Where(f => f.Id == map.LandId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                    map.TdcBlockHouseName = _context.BlockHouses.Where(f => f.Id == map.BlockHouseId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                    map.TdcFloorName = _context.FloorTdcs.Where(f => f.Id == map.FloorTdcId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                    map.TdcApartmentName = _context.ApartmentTdcs.Where(f => f.Id == map.ApartmentTdcId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                    map.TypeDecisionName = _context.TypeAttributeItems.Where(f => f.Id == map.TypeDecisionId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                    map.TypeLegalName = _context.TypeAttributeItems.Where(f => f.Id == map.TypeLegalId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                    map.DistrictProjectName = _context.Districts.Where(f => f.Id == map.DistrictProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                    List<DistrictAllocasionApartment> districtAllocasionApartments = _context.DistrictAllocasionApartments.Where(l => l.TdcApartmentManagerId == map.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    List<DistrictAllocasionApartmentData> map_districtAllocasionApartmentDatas = _mapper.Map<List<DistrictAllocasionApartmentData>>(districtAllocasionApartments);
                    foreach (DistrictAllocasionApartmentData map_districtAllocasionApartmentData in map_districtAllocasionApartmentDatas)
                    {
                        map_districtAllocasionApartmentData.DistrictName = _context.Districts.Where(l => l.Id == map_districtAllocasionApartmentData.DistrictId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Name).FirstOrDefault();
                    }
                    map.districtAllocasionApartment = map_districtAllocasionApartmentDatas;
                }

                var groupData = mapper_ApartmentRP.GroupBy(p => p.DistrictProjectId).Select(e => new GrByDistrictProjectId
                {
                    DistrictProjectId = e.Key,
                    DistrictName = _context.Districts.Where(l => l.Id == e.Key && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Name).FirstOrDefault(),
                    GrByTdcProjectIds = e.ToList().GroupBy(p => p.TdcProjectId).Select(e => new GrByTdcProjectId
                    {
                        TdcProjectId = (int)e.Key,
                        TdcProjectName = _context.TDCProjects.Where(l => l.Id == e.Key && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Name).FirstOrDefault(),
                        GrDataTdcApartmentManagerDatas = e.ToList()
                    }).ToList()
                }).ToList();

                XSSFWorkbook wb = new XSSFWorkbook();
                // Tạo ra 1 sheet
                ISheet sheet = wb.CreateSheet();

                string template = @"ReportTDC/TdcApartmentNew.xlsx";
                string webRootPath = _hostingEnvironment.WebRootPath;
                string templatePath = Path.Combine(webRootPath, template);
                string nameExcel = "BangBaoCao_.xls";

                MemoryStream ms = WriteDataToExcelExportReport1(templatePath, 0, groupData);
                byte[] byteArrayContent = ms.ToArray();
                return File(ms.ToArray(), "application/vnd.ms-excel", nameExcel);
            }
            catch (Exception ex)
            {
                log.Error("ExportReport1 Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        private static MemoryStream WriteDataToExcelExportReport1(string templatePath, int sheetnumber, List<GrByDistrictProjectId> data)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();

            int rowStart = 8;
            if (sheet != null)
            {
                try
                {
                    int datacol = 26;

                    var style = workbook.CreateCellStyle();
                    style.BorderBottom = BorderStyle.Thin;
                    style.BorderLeft = BorderStyle.Thin;
                    style.BorderRight = BorderStyle.Thin;
                    style.BorderTop = BorderStyle.Thin;

                    ICellStyle cellStyle = workbook.CreateCellStyle();
                    IFont font = workbook.CreateFont();
                    font.FontName = "Times New Roman";
                    cellStyle.SetFont(font);
                    cellStyle.Alignment = HorizontalAlignment.Center;
                    cellStyle.BorderBottom = BorderStyle.Thin;
                    cellStyle.BorderLeft = BorderStyle.Thin;
                    cellStyle.BorderRight = BorderStyle.Thin;
                    cellStyle.BorderTop = BorderStyle.Thin;

                    int districtcount = 1;

                    DateTime date = DateTime.Now;

                    IRow rowD3 = sheet.GetRow(3);
                    ICell cellD3 = rowD3.GetCell(23);
                    cellD3.SetCellValue("Thành phố Hồ Chí Minh ,ngày   " +  date.Day     + "  tháng    " +    date.Month    + "  năm    " +    date.Year) ;


                    foreach (var districtGroup in data)
                    {
                        int countTdcProjectName = 0;

                        int countQantity = 0; //Số lượng phân bổ theo quy định

                        int countReceived = 0; //Số lượng đã tiếp nhận

                        int countReceivedYet = 0;//Số lượng chưa tiếp nhận

                        int countNotReceived = 0;//Số lượng không tiếp nhận

                        int countHandoverPublic = 0; //Số lượng DVCI bàn giao

                        int countHandoverCenter = 0; //Số lượng trung tâm giao

                        IRow row = sheet.CreateRow(rowStart);
                        ICellStyle cellStyleDate = workbook.CreateCellStyle();
                        IFont fontDate = workbook.CreateFont();
                        fontDate.FontName = "Times New Roman";
                        cellStyleDate.SetFont(fontDate);
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

                        string districtProjectName = "" + districtcount++ + ". " + districtGroup.DistrictName;

                        foreach (var item in districtGroup.GrByTdcProjectIds)
                        {
                            foreach(var z in item.GrDataTdcApartmentManagerDatas)
                            {
                                countTdcProjectName++;
                            }
                            countQantity = item.GrDataTdcApartmentManagerDatas.Sum(x => x.Qantity);

                            countReceived += item.GrDataTdcApartmentManagerDatas.Count(x => x.ReceptionTime == AppEnums.TypeReception.Received);

                            countReceivedYet += item.GrDataTdcApartmentManagerDatas.Count(x => x.ReceptionTime == AppEnums.TypeReception.ReceivedYet);

                            countNotReceived += item.GrDataTdcApartmentManagerDatas.Count(x => x.ReceptionTime == AppEnums.TypeReception.NotReceived);

                            countHandoverCenter += item.GrDataTdcApartmentManagerDatas.Count(x => x.HandoverCenter == true);

                            countHandoverPublic += item.GrDataTdcApartmentManagerDatas.Count(x => x.HandoverPublic == true);
                        }

                        ICell cell = row.CreateCell(0);
                        cell.SetCellValue(districtProjectName); //Tên quận 
                        cell.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader19 = new CellRangeAddress(rowStart, rowStart, 0, 1);
                        RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader19, sheet);
                        RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader19, sheet);
                        RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader19, sheet);
                        RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader19, sheet);
                        sheet.AddMergedRegion(mergedRegioncellHeader19);

                

                        ICell cell1 = row.CreateCell(2);
                        cell1.SetCellValue(countTdcProjectName);//Số lượng dự án 
                        cell1.CellStyle = cellStyle;

                        ICell cell2 = row.CreateCell(10);
                        cell2.SetCellValue(countReceived); //số lượng đã tiếp nhận
                        cell2.CellStyle = cellStyle;

                        ICell cell3 = row.CreateCell(11);
                        cell3.SetCellValue(countReceivedYet); //Số lượng chưa tiếp nhận
                        cell3.CellStyle = cellStyle;

                        ICell cell12 = row.CreateCell(12);
                        cell12.SetCellValue(""); 
                        cell12.CellStyle = cellStyle;

                        ICell cell13 = row.CreateCell(13);
                        cell13.SetCellValue(""); 
                        cell13.CellStyle = cellStyle;

                        ICell cell14 = row.CreateCell(14);
                        cell14.SetCellValue(countNotReceived); //Số lượng không tiếp nhận
                        cell14.CellStyle = cellStyle;

                        ICell cell15 = row.CreateCell(15);
                        cell15.SetCellValue(""); 
                        cell15.CellStyle = cellStyle;

                        ICell cell16 = row.CreateCell(16);
                        cell16.SetCellValue(""); 
                        cell16.CellStyle = cellStyle;

                        ICell cell17 = row.CreateCell(17);
                        cell17.SetCellValue("");
                        cell17.CellStyle = cellStyle;

                        ICell cell7 = row.CreateCell(18);
                        cell7.SetCellValue(countQantity);
                        cell7.CellStyle = cellStyle;

                        ICell cell19 = row.CreateCell(19);
                        cell19.SetCellValue("");
                        cell19.CellStyle = cellStyle;

                        ICell cell23 = row.CreateCell(20);
                        cell23.SetCellValue(countReceivedYet); 
                        cell23.CellStyle = cellStyle;

                        ICell cell33 = row.CreateCell(21);
                        cell33.SetCellValue(""); 
                        cell33.CellStyle = cellStyle;

                        ICell cell32 = row.CreateCell(22);
                        cell32.SetCellValue("");
                        cell32.CellStyle = cellStyle;


                        ICell cell8 = row.CreateCell(23);
                        cell8.SetCellValue(countHandoverPublic);
                        cell8.CellStyle = cellStyle;

                        ICell cell9 = row.CreateCell(24);
                        cell9.SetCellValue(countHandoverCenter);
                        cell9.CellStyle = cellStyle;

                        ICell cell300 = row.CreateCell(25);
                        cell300.SetCellValue(""); 
                        cell300.CellStyle = cellStyle;

                        rowStart++;
                        int k = 0;

                        foreach (var item in districtGroup.GrByTdcProjectIds)
                        {

                            foreach (var items in item.GrDataTdcApartmentManagerDatas)
                            {
                                IRow row1 = sheet.CreateRow(rowStart);
                                for (int i = 0; i < datacol; i++)
                                {
                                    ICell cellitem = row1.CreateCell(i);
                                    if(i == 0)
                                    {
                                        cellitem.SetCellValue(k + 1);
                                        cellitem.CellStyle = cellStyle;
                                    }
                                    if (i == 1)
                                    {
                                        cellitem.SetCellValue(string.Empty);
                                        cellitem.CellStyle = cellStyle;
                                    }
                                    else if (i == 2)
                                    {
                                        cellitem.SetCellValue(items.TdcProjectName);
                                        cellitem.CellStyle = cellStyle;
                                    }
                                    else if (i == 3)
                                    {
                                        cellitem.SetCellValue(items.TdcApartmentName);
                                        cellitem.CellStyle = cellStyle;
                                    }
                                    else if (i == 4)
                                    {
                                        cellitem.SetCellValue(items.TdcFloorName);
                                        cellitem.CellStyle = cellStyle;
                                    }
                                    else if (i == 5)
                                    {
                                        cellitem.SetCellValue(items.TdcBlockHouseName);
                                        cellitem.CellStyle = cellStyle;
                                    }
                                    else if (i == 6)
                                    {
                                        cellitem.SetCellValue(items.TdcLandName); 
                                        cellitem.CellStyle = cellStyle;
                                    }
                                    else if (i == 7)
                                    {
                                        cellitem.SetCellValue((double)items.TdcApartmentArea);
                                        cellitem.CellStyle = cellStyle;
                                    }
                                    else if (i == 8)
                                    {
                                        cellitem.SetCellValue(items.TdcApartmentCountRoom);
                                        cellitem.CellStyle = cellStyle;
                                    }
                                    else if (i == 9)
                                    {
                                        cellitem.SetCellValue(items.TypeLegalName);
                                        cellitem.CellStyle = cellStyle;
                                    }
                                    else if (i == 10)
                                    {
                                        if (items.ReceptionTime == AppEnums.TypeReception.Received && items.ReceptionDate.HasValue)
                                        {
                                            cellitem.SetCellValue(items.ReceptionDate.Value);
                                            cellitem.CellStyle = cellStyleDate;
                                        }
                                        else
                                        {
                                            cellitem.SetCellValue(string.Empty);
                                            cellitem.CellStyle = cellStyle;
                                        }
                                    }
                                    else if (i == 11)
                                    {
                                        if (items.ReceptionTime == AppEnums.TypeReception.ReceivedYet && items.ReceptionDate.HasValue)
                                        {
                                            cellitem.SetCellValue(items.ReceptionDate.Value);
                                            cellitem.CellStyle = cellStyleDate;
                                        }
                                        else
                                        {
                                            cellitem.SetCellValue(string.Empty);
                                            cellitem.CellStyle = cellStyle;
                                        }
                                    }
                                    else if (i == 12)
                                    {
                                        cellitem.SetCellValue(items.ReasonReceivedYet);
                                        cellitem.CellStyle = cellStyle;
                                    }
                                    else if (i == 13)
                                    {
                                        cellitem.SetCellValue(items.Reminded);
                                        cellitem.CellStyle = cellStyle;
                                    }
                                    else if (i == 14)
                                    {
                                        if (items.ReceptionTime == AppEnums.TypeReception.NotReceived)
                                        {
                                            if (items.ReceptionDate.HasValue)
                                            {
                                                cellitem.SetCellValue(items.ReceptionDate.Value);
                                                cellitem.CellStyle = cellStyleDate;
                                            }
                                            else
                                            {
                                                cellitem.SetCellValue(string.Empty);
                                                cellitem.CellStyle = cellStyle;
                                            }
                                        }
                                        else
                                        {
                                            cellitem.SetCellValue(string.Empty);
                                            cellitem.CellStyle = cellStyle;
                                        }
                                    }
                                    else if (i == 15)
                                    {
                                        cellitem.SetCellValue(items.ReasonNotReceived);
                                        cellitem.CellStyle = cellStyle;
                                    }
                                    else if (i == 16)
                                    {
                                        cellitem.SetCellValue(items.TypeDecisionName);
                                        cellitem.CellStyle = cellStyle;
                                    }
                                    else if (i == 17)
                                    {
                                        string str = "";
                                        items.districtAllocasionApartment.RemoveAll(item =>
                                        {
                                            return items.districtAllocasionApartment.Count(x => x.DistrictName == item.DistrictName) > 1;
                                        });
                                        foreach (var q in items.districtAllocasionApartment)
                                        {
                                            str += q.DistrictName == "" ? $"" : $" , {q.DistrictName}";
                                        }

                                        cellitem.SetCellValue(str);
                                        cellitem.CellStyle = cellStyle;
                                    }
                                    else if (i == 18)
                                    {
                                        int count = items.districtAllocasionApartment.Sum(x => x.ActualNumber);
                                        cellitem.SetCellValue(count);
                                        cellitem.CellStyle = cellStyle;
                                    }
                                    else if (i == 19)
                                    {
                                        if (items.OverYear == AppEnums.TypeOverYear.Installment)
                                        {
                                            cellitem.SetCellValue(items.HandOverYear);
                                            cellitem.CellStyle = cellStyleDate;
                                        }
                                        else
                                        {
                                            cellitem.SetCellValue(string.Empty);
                                            cellitem.CellStyle = cellStyle;
                                        }
                                    }
                                    else if (i == 20)
                                    {
                                        if (items.OverYear == AppEnums.TypeOverYear.TemporaryResidence)
                                        {
                                            cellitem.SetCellValue(items.HandOverYear);
                                            cellitem.CellStyle = cellStyleDate;
                                        }
                                        else
                                        {
                                            cellitem.SetCellValue(string.Empty);
                                            cellitem.CellStyle = cellStyle;
                                        }
                                    }
                                    else if (i == 21)
                                    {
                                        if (items.OverYear == AppEnums.TypeOverYear.Rent)
                                        {
                                            cellitem.SetCellValue(items.HandOverYear);
                                            cellitem.CellStyle = cellStyleDate;
                                        }
                                        else
                                        {
                                            cellitem.SetCellValue(string.Empty);
                                            cellitem.CellStyle = cellStyle;
                                        }
                                    }
                                    else if (i == 22)
                                    {
                                        if (items.OverYear == AppEnums.TypeOverYear.OneSell)
                                        {
                                            cellitem.SetCellValue(items.HandOverYear);
                                            cellitem.CellStyle = cellStyleDate;
                                        }
                                        else
                                        {
                                            cellitem.SetCellValue(string.Empty);
                                            cellitem.CellStyle = cellStyle;
                                        }
                                    }
                                    else if (i == 23)
                                    {
                                        if(items.HandoverPublic == true)
                                        {
                                            if(items.NoteHandoverPublic != null)
                                            {
                                                cellitem.SetCellValue(items.NoteHandoverPublic);
                                                cellitem.CellStyle = cellStyle;
                                            }
                                            else
                                            {
                                                cellitem.SetCellValue("X");
                                                cellitem.CellStyle = cellStyle;
                                            }
                                        }
                                        else
                                        {
                                            cellitem.SetCellValue(string.Empty);
                                            cellitem.CellStyle = cellStyle;
                                        }
                                    }
                                    else if (i == 24)
                                    {
                                        if (items.HandoverCenter == true)
                                        {
                                            if (items.NoteHandoverCenter != null)
                                            {
                                                cellitem.SetCellValue(items.NoteHandoverCenter);
                                                cellitem.CellStyle = cellStyle;
                                            }
                                            else
                                            {
                                                cellitem.SetCellValue("X");
                                                cellitem.CellStyle = cellStyle;
                                            }
                                        }
                                        else
                                        {
                                            cellitem.SetCellValue(string.Empty);
                                            cellitem.CellStyle = cellStyle;
                                        }
                                    }
                                    else if (i == 25)
                                    {
                                        cellitem.SetCellValue(string.Empty);
                                        cellitem.CellStyle = cellStyle;
                                    }
                                }
                                rowStart++;
                                k++;
                            }
                        }
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

        [HttpPost("ExcelRP4")]
        public async Task<IActionResult> ExcelRP4()
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

            List<TdcApartmentManager> tdcApartmentManagersRP4 = _context.TdcApartmentManagers.Where(x => x.Status != AppEnums.EntityStatus.DELETED).ToList();
            List<TdcApartmentManagerData> mapper_ApartmentRP4 = _mapper.Map<List<TdcApartmentManagerData>>(tdcApartmentManagersRP4);
            foreach (var map in mapper_ApartmentRP4)
            {
                map.TdcProjectName = _context.TDCProjects.Where(f => f.Id == map.TdcProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                map.TdcLandName = _context.Lands.Where(f => f.Id == map.LandId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                map.TdcBlockHouseName = _context.BlockHouses.Where(f => f.Id == map.BlockHouseId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                map.TdcFloorName = _context.FloorTdcs.Where(f => f.Id == map.FloorTdcId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                map.TdcApartmentName = _context.ApartmentTdcs.Where(f => f.Id == map.ApartmentTdcId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                map.TypeDecisionName = _context.TypeAttributeItems.Where(f => f.Id == map.TypeDecisionId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                map.TypeLegalName = _context.TypeAttributeItems.Where(f => f.Id == map.TypeLegalId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                map.DistrictProjectName = _context.Districts.Where(f => f.Id == map.DistrictProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                List<DistrictAllocasionApartment> districtAllocasionApartments = _context.DistrictAllocasionApartments.Where(l => l.TdcApartmentManagerId == map.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<DistrictAllocasionApartmentData> map_districtAllocasionApartmentDatas = _mapper.Map<List<DistrictAllocasionApartmentData>>(districtAllocasionApartments);
                foreach (DistrictAllocasionApartmentData map_districtAllocasionApartmentData in map_districtAllocasionApartmentDatas)
                {
                    map_districtAllocasionApartmentData.DistrictName = _context.Districts.Where(l => l.Id == map_districtAllocasionApartmentData.DistrictId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Name).FirstOrDefault();
                }
                map.districtAllocasionApartment = map_districtAllocasionApartmentDatas;
            }



            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"ReportTDC/RP4-Apartment.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);
            string nameExcel = "BangBaoCao_.xls";

            MemoryStream ms = WriteDataToExcel4(templatePath, 0, mapper_ApartmentRP4);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", nameExcel);

        }
        private static MemoryStream WriteDataToExcel4(string templatePath, int sheetnumber, List<TdcApartmentManagerData> data)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();

            int rowSTT = 7;
            int rowHeader1 = 8;
            int rowHeader2 = 9;
            int rowHeaderNB = 10;
            int rowHeader3 = 11;
            int rowStart = 12;

            if (sheet != null)
            {
                try
                {
                    int k = 0;
                    int datacol = 100;

                    int totalCountDecision = 0;
                    int totalCountReceived = 0;
                    int totalCountReceivedYet = 0;
                    int totalCountNotReceived = 0;

                    int datacount = 1;

                    var style = workbook.CreateCellStyle();
                    style.BorderBottom = BorderStyle.Thin;
                    style.BorderLeft = BorderStyle.Thin;
                    style.BorderRight = BorderStyle.Thin;
                    style.BorderTop = BorderStyle.Thin;

                    ICellStyle cellStyle = workbook.CreateCellStyle();
                    IFont font = workbook.CreateFont();
                    font.FontName = "Times New Roman";
                    cellStyle.SetFont(font);
                    cellStyle.Alignment = HorizontalAlignment.Center;
                    cellStyle.BorderBottom = BorderStyle.Thin;
                    cellStyle.BorderLeft = BorderStyle.Thin;
                    cellStyle.BorderRight = BorderStyle.Thin;
                    cellStyle.BorderTop = BorderStyle.Thin;

                    IRow rowStt = sheet.CreateRow(rowSTT);
                    IRow rowHeaders1 = sheet.CreateRow(rowHeader1);
                    IRow rowHeaders2 = sheet.CreateRow(rowHeader2);
                    IRow rowHeadersNB = sheet.CreateRow(rowHeaderNB);

                    ICell cellHeader1 = rowStt.CreateCell(0);
                    cellHeader1.SetCellValue("STT");
                    cellHeader1.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader1 = new CellRangeAddress(rowSTT, rowHeader2, 0, 0);
                    RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                    RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                    RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                    RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                    sheet.AddMergedRegion(mergedRegioncellHeader1);

                    ICell cellHeader2 = rowHeaders1.CreateCell(1);
                    cellHeader2.SetCellValue("Đơn vị bàn giao");
                    cellHeader2.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader2 = new CellRangeAddress(rowHeader1, rowHeader2, 1, 1);
                    RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader2, sheet);
                    RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader2, sheet);
                    RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader2, sheet);
                    RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader2, sheet);
                    sheet.AddMergedRegion(mergedRegioncellHeader2);

                    ICell cellHeader3 = rowHeaders1.CreateCell(2);
                    cellHeader3.SetCellValue("Tên Dự án, địa chỉ dự án");
                    cellHeader3.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader3 = new CellRangeAddress(rowHeader1, rowHeader2, 2, 2);
                    RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader3, sheet);
                    RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader3, sheet);
                    RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader3, sheet);
                    RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader3, sheet);
                    sheet.AddMergedRegion(mergedRegioncellHeader3);

                    ICell cellHeader4 = rowHeaders1.CreateCell(3);
                    cellHeader4.SetCellValue("Số liệu tiếp nhận nền đất");
                    cellHeader4.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader4 = new CellRangeAddress(rowHeader1, rowHeader1, 3, 6);
                    RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader4, sheet);
                    RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader4, sheet);
                    RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader4, sheet);
                    RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader4, sheet);
                    sheet.AddMergedRegion(mergedRegioncellHeader4);

                    ICell cellHeader5 = rowHeaders2.CreateCell(3);
                    cellHeader5.SetCellValue("Theo QĐ");
                    cellHeader5.CellStyle = cellStyle;

                    ICell cellHeader6 = rowHeaders2.CreateCell(4);
                    cellHeader6.SetCellValue("Đã nhận");
                    cellHeader6.CellStyle = cellStyle;

                    ICell cellHeader7 = rowHeaders2.CreateCell(5);
                    cellHeader7.SetCellValue("Chưa nhận");
                    cellHeader7.CellStyle = cellStyle;

                    ICell cellHeader8 = rowHeaders2.CreateCell(6);
                    cellHeader8.SetCellValue("Không nhận");
                    cellHeader8.CellStyle = cellStyle;

                    ICell cellname6 = rowHeaders1.CreateCell(7);
                    cellname6.SetCellValue("Bàn giao cho hộ dân");
                    cellname6.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader21 = new CellRangeAddress(rowHeader1, rowHeader1, 7, 9);
                    RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader21, sheet);
                    RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader21, sheet);
                    RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader21, sheet);
                    RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader21, sheet);
                    sheet.AddMergedRegion(mergedRegioncellHeader21);

                    ICell cellname7 = rowHeaders1.CreateCell(10);
                    cellname7.SetCellValue("Bàn giao cho ban bồi thường");
                    cellname7.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader22 = new CellRangeAddress(rowHeader1, rowHeader1, 10, 12);
                    RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader22, sheet);
                    RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader22, sheet);
                    RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader22, sheet);
                    RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader22, sheet);
                    sheet.AddMergedRegion(mergedRegioncellHeader22);

                    ICell cellname8 = rowHeaders2.CreateCell(7);
                    cellname8.SetCellValue("DVCI bàn giao");
                    cellname8.CellStyle = cellStyle;

                    ICell cellname9 = rowHeaders2.CreateCell(8);
                    cellname9.SetCellValue("Trung tâm giao");
                    cellname9.CellStyle = cellStyle;

                    ICell cellname10 = rowHeaders2.CreateCell(9);
                    cellname10.SetCellValue("Bàn giao khác");
                    cellname10.CellStyle = cellStyle;

                    ICell cellname11 = rowHeaders2.CreateCell(10);
                    cellname11.SetCellValue("DVCI bàn giao");
                    cellname11.CellStyle = cellStyle;

                    ICell cellname12 = rowHeaders2.CreateCell(11);
                    cellname12.SetCellValue("Trung tâm giao");
                    cellname12.CellStyle = cellStyle;

                    ICell cellname13 = rowHeaders2.CreateCell(12);
                    cellname13.SetCellValue("Bàn giao khác");
                    cellname13.CellStyle = cellStyle;


                    int nameDataCount = 13;

                    var grouped = data.GroupBy(x => new { x.TypeLegalId, x.TypeDecisionId }).Select(g => g.First());

                    foreach (var item in grouped)
                    {
                        ICell cellnamecount = rowStt.CreateCell(nameDataCount);
                        cellnamecount.SetCellValue("Quyết định tiếp nhận và phân bổ số: " + datacount++);
                        cellnamecount.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeadercount = new CellRangeAddress(rowSTT, rowSTT, nameDataCount, nameDataCount + 5);
                        RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeadercount, sheet);
                        RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeadercount, sheet);
                        RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeadercount, sheet);
                        RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeadercount, sheet);
                        sheet.AddMergedRegion(mergedRegioncellHeadercount);

                        ICell cellname = rowHeaders1.CreateCell(nameDataCount);
                        cellname.SetCellValue(item.TypeLegalName + " (Tiếp nhận)");
                        cellname.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader5 = new CellRangeAddress(rowHeader1, rowHeader1, nameDataCount, nameDataCount + 2);
                        RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader5, sheet);
                        RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader5, sheet);
                        RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader5, sheet);
                        RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader5, sheet);
                        sheet.AddMergedRegion(mergedRegioncellHeader5);

                        ICell cellname1 = rowHeaders2.CreateCell(nameDataCount);
                        cellname1.SetCellValue("Quận/Huyện");
                        cellname1.CellStyle = cellStyle;

                        ICell cellname2 = rowHeaders2.CreateCell(nameDataCount + 1);
                        CellRangeAddress mergedRegioncellHeader8 = new CellRangeAddress(rowHeader2, rowHeader2, nameDataCount + 1, nameDataCount + 2);
                        RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader8, sheet);
                        RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader8, sheet);
                        RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader8, sheet);
                        RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader8, sheet);
                        sheet.AddMergedRegion(mergedRegioncellHeader8);
                        cellname2.SetCellValue("Số lượng");
                        cellname2.CellStyle = cellStyle;

                        ICell cellnameNB3 = rowHeadersNB.CreateCell(nameDataCount + 1);
                        cellnameNB3.SetCellValue("Số lượng phân bổ theo tiếp nhận");
                        cellnameNB3.CellStyle = cellStyle;

                        ICell cellnameNB4 = rowHeadersNB.CreateCell(nameDataCount + 2);
                        cellnameNB4.SetCellValue("Số lượng theo thực tế");
                        cellnameNB4.CellStyle = cellStyle;

                        ICell cellname3 = rowHeaders1.CreateCell(nameDataCount + 3);
                        cellname3.SetCellValue(item.TypeDecisionName + " (Phân bổ)");
                        cellname3.CellStyle = cellStyle;
                        CellRangeAddress mergedRegioncellHeader6 = new CellRangeAddress(rowHeader1, rowHeader1, nameDataCount + 3, nameDataCount + 5);
                        RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader6, sheet);
                        RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader6, sheet);
                        RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader6, sheet);
                        RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader6, sheet);
                        sheet.AddMergedRegion(mergedRegioncellHeader6);

                        ICell cellname4 = rowHeaders2.CreateCell(nameDataCount + 3);
                        cellname4.SetCellValue("Quận/Huyện");
                        cellname4.CellStyle = cellStyle;

                        ICell cellname5 = rowHeaders2.CreateCell(nameDataCount + 4);
                        CellRangeAddress mergedRegioncellHeader7 = new CellRangeAddress(rowHeader2, rowHeader2, nameDataCount + 4, nameDataCount + 5);
                        RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader7, sheet);
                        RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader7, sheet);
                        RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader7, sheet);
                        RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader7, sheet);
                        sheet.AddMergedRegion(mergedRegioncellHeader7);
                        cellname5.SetCellValue("Số lượng");
                        cellname5.CellStyle = cellStyle;

                        ICell cellnameNB1 = rowHeadersNB.CreateCell(nameDataCount + 4);
                        cellnameNB1.SetCellValue("Số lượng phân bổ về TT");
                        cellnameNB1.CellStyle = cellStyle;

                        ICell cellnameNB2 = rowHeadersNB.CreateCell(nameDataCount + 5);
                        cellnameNB2.SetCellValue("Số lượng theo thực tế quản lý");
                        cellnameNB2.CellStyle = cellStyle;

                        nameDataCount = nameDataCount + 6;
                    }

                    

                    ICell cellname14 = rowHeaders1.CreateCell(nameDataCount );
                    cellname14.SetCellValue("Ghi chú");
                    cellname14.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader9 = new CellRangeAddress(rowHeader1, rowHeader2, nameDataCount , nameDataCount );
                    RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader9, sheet);
                    RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader9, sheet);
                    RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader9, sheet);
                    RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader9, sheet);
                    sheet.AddMergedRegion(mergedRegioncellHeader9);

                    var dataGrouped = data.GroupBy(x => x.DistrictProjectName).Distinct().ToList();
                    int districtcount = 1;

                    foreach (var districtproject in dataGrouped)
                    {
                        IRow row = sheet.CreateRow(rowStart);

                        ICellStyle cellStyleDate = workbook.CreateCellStyle();
                        IFont fontDate = workbook.CreateFont();
                        fontDate.FontName = "Times New Roman";
                        cellStyleDate.SetFont(fontDate);
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


                        int countReceived = 0;
                        int countReceivedYet = 0;
                        int countNotReceived = 0;

                        string districtProjectName = districtproject.Key;
                        int countDecison = districtproject.Count();

                        totalCountDecision += countDecison;


                        countReceived += districtproject.Count(x => x.ReceptionTime == AppEnums.TypeReception.Received);

                        countReceivedYet += districtproject.Count(x => x.ReceptionTime == AppEnums.TypeReception.ReceivedYet);

                        countNotReceived += districtproject.Count(x => x.ReceptionTime == AppEnums.TypeReception.NotReceived);

                        totalCountReceived += countReceived;
                        totalCountReceivedYet += countReceivedYet;
                        totalCountNotReceived += countNotReceived;

                        ICell cell = row.CreateCell(0);
                        cell.SetCellValue(districtcount++);
                        cell.CellStyle = cellStyle;

                        ICell cell1 = row.CreateCell(1);
                        cell1.SetCellValue(districtProjectName);
                        cell1.CellStyle = cellStyle;

                        ICell cell2 = row.CreateCell(2);
                        cell2.SetCellValue("");
                        cell2.CellStyle = cellStyle;

                        ICell cell3 = row.CreateCell(3);
                        cell3.SetCellValue(countDecison);
                        cell3.CellStyle = cellStyle;

                        ICell cell4 = row.CreateCell(4);
                        cell4.SetCellValue(countReceived);
                        cell4.CellStyle = cellStyle;

                        ICell cell5 = row.CreateCell(5);
                        cell5.SetCellValue(countReceivedYet);
                        cell5.CellStyle = cellStyle;

                        ICell cell6 = row.CreateCell(6);
                        cell6.SetCellValue(countNotReceived);
                        cell6.CellStyle = cellStyle;

                        rowStart++;

                        var grouptdcproject = districtproject.GroupBy(x => x.TdcProjectName);

                        foreach (var project in grouptdcproject)
                        {
                            IRow row1 = sheet.CreateRow(rowStart);

                            var ReceptionTimes = project.GroupBy(x => x.ReceptionTime).Select(f => new
                            {
                                ReceptionType = f.Key,
                                Count = f.Count()
                            });

                            int countdecison = project.Count();

                            int countReceived1 = 0;
                            int countReceivedYet1 = 0;
                            int countNotReceived1 = 0;

                            string tdcProjectName = project.Key;

                            var householdData = data.Where(x => x.HandOver == AppEnums.TypeHandover.Household);
                            var indemnifyData = data.Where(x => x.HandOver == AppEnums.TypeHandover.Indemnify);

                            var householhandoverCenterCount = householdData.Where(x => x.TdcProjectName == project.Key).Count(x => x.HandoverCenter == true);
                            var householhandoverPublicCount = householdData.Where(x => x.TdcProjectName == project.Key).Count(x => x.HandoverPublic == true);
                            var householhandoverOtherCount = householdData.Where(x => x.TdcProjectName == project.Key).Count(x => x.HandoverOther == true);

                            var indemnifyhandoverCenterCount = indemnifyData.Where(x => x.TdcProjectName == project.Key).Count(x => x.HandoverCenter == true);
                            var indemnifyhandoverPublicCount = indemnifyData.Where(x => x.TdcProjectName == project.Key).Count(x => x.HandoverCenter == true);
                            var indemnifyhandoverOtherCount = indemnifyData.Where(x => x.TdcProjectName == project.Key).Count(x => x.HandoverCenter == true);

                            foreach (var recepTime in ReceptionTimes)
                            {
                                if (recepTime.ReceptionType.ToString() == "Received")
                                {
                                    countReceived1 = recepTime.Count;
                                }
                                else if (recepTime.ReceptionType.ToString() == "ReceivedYet")
                                {
                                    countReceivedYet1 = recepTime.Count;
                                }
                                else if (recepTime.ReceptionType.ToString() == "NotReceived")
                                {
                                    countNotReceived1 = recepTime.Count;
                                }
                            }

                            ICell cellprojectnull1 = row1.CreateCell(0);
                            cellprojectnull1.SetCellValue(" ");
                            cellprojectnull1.CellStyle = cellStyle;

                            ICell cellprojectnull2 = row1.CreateCell(1);
                            cellprojectnull2.SetCellValue(" ");
                            cellprojectnull2.CellStyle = cellStyle;

                            ICell cellproject = row1.CreateCell(2);
                            cellproject.SetCellValue(tdcProjectName);
                            cellproject.CellStyle = cellStyle;

                            ICell cellproject1 = row1.CreateCell(3);
                            cellproject1.SetCellValue(countdecison);
                            cellproject1.CellStyle = cellStyle;

                            ICell cellproject2 = row1.CreateCell(4);
                            cellproject2.SetCellValue(countReceived1);
                            cellproject2.CellStyle = cellStyle;

                            ICell cellproject3 = row1.CreateCell(5);
                            cellproject3.SetCellValue(countReceivedYet1);
                            cellproject3.CellStyle = cellStyle;

                            ICell cellproject4 = row1.CreateCell(6);
                            cellproject4.SetCellValue(countNotReceived1);
                            cellproject4.CellStyle = cellStyle;

                            ICell cellproject5 = row1.CreateCell(7);
                            cellproject5.SetCellValue(householhandoverPublicCount);
                            cellproject5.CellStyle = cellStyle;

                            ICell cellproject6 = row1.CreateCell(8);
                            cellproject6.SetCellValue(householhandoverCenterCount);
                            cellproject6.CellStyle = cellStyle;

                            ICell cellproject7 = row1.CreateCell(9);
                            cellproject7.SetCellValue(householhandoverOtherCount);
                            cellproject7.CellStyle = cellStyle;

                            ICell cellproject8 = row1.CreateCell(10);
                            cellproject8.SetCellValue(indemnifyhandoverPublicCount);
                            cellproject8.CellStyle = cellStyle;

                            ICell cellproject9 = row1.CreateCell(11);
                            cellproject9.SetCellValue(indemnifyhandoverCenterCount);
                            cellproject9.CellStyle = cellStyle;

                            ICell cellproject10 = row1.CreateCell(12);
                            cellproject10.SetCellValue(indemnifyhandoverOtherCount);
                            cellproject10.CellStyle = cellStyle;

                            rowStart++;
                            nameDataCount = 13;

                            foreach (var item in project)
                            {
                                int qantities = item.Qantity;
                                int receivenumbers = item.ReceiveNumber;
                                string districtprojects = item.DistrictProjectName;

                                var fillterDecisionName = item.districtAllocasionApartment.Where(x => x.TdcApartmentManagerId == item.Id).ToList();


                                foreach (var childitem in fillterDecisionName)
                                {
                                    var dataItem = grouped.Where(d => d.TypeLegalId == item.TypeLegalId && d.TypeDecisionId == item.TypeDecisionId).FirstOrDefault();
                                    int indexDataItem = grouped.IndexOf(dataItem);
                                    int position = nameDataCount + (indexDataItem * 6);
                                    int sumactual = fillterDecisionName.Sum(x => x.ActualNumber);
                                    if (indexDataItem >= 0)
                                    {
                                        IRow row2 = sheet.CreateRow(rowStart);

                                        ICell cellnameItem1 = row2.CreateCell(position);
                                        cellnameItem1.SetCellValue(districtprojects);
                                        cellnameItem1.CellStyle = cellStyle;

                                        ICell cellnameItem2 = row2.CreateCell(position + 1);
                                        cellnameItem2.SetCellValue(receivenumbers);
                                        cellnameItem2.CellStyle = cellStyle;

                                        ICell cellnameItem3 = row2.CreateCell(position + 2);
                                        cellnameItem3.SetCellValue(sumactual);
                                        cellnameItem3.CellStyle = cellStyle;

                                        ICell cellnameItem4 = row2.CreateCell(position + 3);
                                        cellnameItem4.SetCellValue(childitem.DistrictName);
                                        cellnameItem4.CellStyle = cellStyle;

                                        ICell cellnameItem5 = row2.CreateCell(position + 4);
                                        cellnameItem5.SetCellValue(qantities);
                                        cellnameItem5.CellStyle = cellStyle;

                                        ICell cellnameItem6 = row2.CreateCell(position + 5);
                                        cellnameItem6.SetCellValue(childitem.ActualNumber);
                                        cellnameItem6.CellStyle = cellStyle;

                                        //nameDataCount = nameDataCount + 6;

                                        rowStart++;
                                    }
                                }
                            }

                        }

                    }

                    IRow rowTotal = sheet.CreateRow(rowHeader3);

                    ICell cellTotal = rowTotal.CreateCell(0);
                    cellTotal.SetCellValue("TỔNG CỘNG");
                    cellTotal.CellStyle = cellStyle;

                    CellRangeAddress mergedRegioncellHeader10 = new CellRangeAddress(rowHeader3, rowHeader3, 0, 2);
                    RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader10, sheet);
                    RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader10, sheet);
                    RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader10, sheet);
                    RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader10, sheet);
                    sheet.AddMergedRegion(mergedRegioncellHeader10);

                    ICell cellTotal1 = rowTotal.CreateCell(3);
                    cellTotal1.SetCellValue(totalCountDecision);
                    cellTotal1.CellStyle = cellStyle;

                    ICell cellTotal2 = rowTotal.CreateCell(4);
                    cellTotal2.SetCellValue(totalCountReceived);
                    cellTotal2.CellStyle = cellStyle;

                    ICell cellTotal3 = rowTotal.CreateCell(5);
                    cellTotal3.SetCellValue(totalCountReceivedYet);
                    cellTotal3.CellStyle = cellStyle;

                    ICell cellTotal4 = rowTotal.CreateCell(6);
                    cellTotal4.SetCellValue(totalCountNotReceived);
                    cellTotal4.CellStyle = cellStyle;

                }
                catch (Exception ex)
                {
                    log.Error("Put Exception:" + ex);
                }
            }
            sheet.ForceFormulaRecalculation = true;
            MemoryStream ms2 = new MemoryStream();
            workbook.Write(ms2);

            return ms2;
        }
    }
}
