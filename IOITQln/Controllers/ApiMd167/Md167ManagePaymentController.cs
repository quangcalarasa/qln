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
using Microsoft.AspNetCore.Hosting;
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
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System.IO;
using static IOITQln.Entities.Md167House;
using System.Globalization;
using static IOITQln.Common.Enums.AppEnums;
using IOITQln.Controllers.ApiMd167;
using DevExpress.Utils.DirectXPaint;
using Microsoft.Net.Http.Headers;
using DevExpress.ClipboardSource.SpreadsheetML;

namespace IOITQln.Controllers.ApiMd167
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class Md167ManagePaymentController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("Md167ManagePayment", "Md167ManagePayment");
        private static string functionCode = "MANAGE_PAYMENT";
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        private IHostingEnvironment _hostingEnvironment;

        public Md167ManagePaymentController(ApiDbContext context, IMapper mapper, IHostingEnvironment hostingEnvironment)
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
                    IQueryable<Md167ManagePayment> data = _context.Md167ManagePayments.Where(c => c.Status != AppEnums.EntityStatus.DELETED);
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
                        List<Md167ManagePaymentData> res = _mapper.Map<List<Md167ManagePaymentData>>(data.ToList());
                        foreach (Md167ManagePaymentData item in res)
                        {
                            List<HousePayment> housePayments = _context.HousePayments.Where(l => l.Md167PaymentId == item.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                            List<HousePaymentData> map_housePaymentDatas = _mapper.Map<List<HousePaymentData>>(housePayments);
                            foreach (HousePaymentData map_housePaymentData in map_housePaymentDatas)
                            {
                                
                                Md167House md167House = _context.Md167Houses.Where(f => f.Id == map_housePaymentData.HouseId && f.TypeHouse != Type_House.Apartment && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                if (md167House == null)
                                {
                                    map_housePaymentData.HouseCode = "";
                                    map_housePaymentData.HouseName = "";
                                    map_housePaymentData.ProviceName = "";
                                    //map_housePaymentData.DistrictName = "";
                                    //map_housePaymentData.WardName = "";
                                    //map_housePaymentData.LaneName = "";
                                    map_housePaymentData.TaxNN = 0;
                                }
                                else
                                {
                                    map_housePaymentData.HouseCode = md167House != null ? md167House.Code : "";
                                    map_housePaymentData.HouseName = md167House != null ? md167House.HouseNumber : "";
                                    //map_housePaymentData.ProviceName = provincies != null ? provincies.Name : "";
                                    map_housePaymentData.TaxNN = md167House.InfoValue.TaxNN;
                                }
                            }
                            item.housePayments = map_housePaymentDatas;
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
                Md167ManagePayment data = await _context.Md167ManagePayments.FindAsync(id);

                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                    return Ok(def);
                }
                Md167ManagePaymentData res = _mapper.Map<Md167ManagePaymentData>(data);


                List<HousePayment> housePayments = _context.HousePayments.Where(l => l.Md167PaymentId == res.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<HousePaymentData> map_housePaymentDatas = _mapper.Map<List<HousePaymentData>>(housePayments);
                foreach (HousePaymentData map_housePaymentData in map_housePaymentDatas)
                {
                    Md167House md167House = _context.Md167Houses.Where(f => f.Id == map_housePaymentData.HouseId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    if (md167House == null)
                    {
                        map_housePaymentData.HouseCode = "";
                        map_housePaymentData.HouseName = "";
                        //map_housePaymentData.ProviceName = "";
                        //map_housePaymentData.DistrictName = "";
                        //map_housePaymentData.WardName = "";
                        //map_housePaymentData.LaneName = "";
                        map_housePaymentData.TaxNN = 0;
                    }
                    else 
                    {
                        map_housePaymentData.HouseCode = md167House != null ? md167House.Code : "";
                        map_housePaymentData.HouseName = md167House != null ? md167House.HouseNumber : "";
                        //map_housePaymentData.ProviceName = provincies != null ? provincies.Name : "";
                        map_housePaymentData.TaxNN = md167House.InfoValue.TaxNN;
                    }
                }
                res.housePayments = map_housePaymentDatas;

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
        [HttpGet("GetHouseAndKios")]
        public IActionResult GetByPage()
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
                IQueryable<Province> provincies = _context.Provincies.Where(x => x.Status != EntityStatus.DELETED);
                IQueryable<District> districts = _context.Districts.Where(x => x.Status != EntityStatus.DELETED);
                IQueryable<Ward> wards = _context.Wards.Where(x => x.Status != EntityStatus.DELETED);
                IQueryable<Lane> lanies = _context.Lanies.Where(x => x.Status != EntityStatus.DELETED);
                List<Md167House> md167Houses = _context.Md167Houses.Where(f => f.TypeHouse != Type_House.Apartment && f.IsPayTax == true && f.Status != EntityStatus.DELETED).ToList();
                List<KiosAndHouseResData> res = new List<KiosAndHouseResData>();
                foreach(var house in md167Houses)
                {
                    var childHouse = _context.Md167Houses.Find(house.Id);
                    if (childHouse != null)
                    {
                        KiosAndHouseResData newItem = new KiosAndHouseResData
                        {
                            DistrictId = childHouse.DistrictId,
                            WardId = childHouse.WardId,
                            LaneId = childHouse.LaneId,
                            ProviceName = provincies.Where(f => f.Id == childHouse.ProvinceId).Select(f => f.Name).FirstOrDefault(),
                            DistrictName = districts.Where(f => f.Id == childHouse.DistrictId).Select(f => f.Name).FirstOrDefault(),
                            WardName = wards.Where(f => f.Id == childHouse.WardId).Select(f => f.Name).FirstOrDefault(),
                            LaneName = lanies.Where(f => f.Id == childHouse.LaneId).Select(f => f.Name).FirstOrDefault(),
                            HouseCode = childHouse.Code,
                            HouseName = childHouse.HouseNumber,
                            IsPayTax = childHouse.IsPayTax,
                            TaxNN = childHouse.InfoValue.TaxNN,
                            HouseId = childHouse.Id,
                            TypeHouse = childHouse.TypeHouse,
                            Md167HouseId = childHouse.Md167HouseId,
                            
                        };

                        if (childHouse.TypeHouse == Type_House.House)
                        {
                            newItem.ProviceName = provincies.FirstOrDefault(e => e.Id == childHouse.ProvinceId)?.Name;
                            newItem.LaneName = lanies.FirstOrDefault(e => e.Id == childHouse.LaneId)?.Name;
                            newItem.WardName = wards.FirstOrDefault(e => e.Id == childHouse.WardId)?.Name;
                            newItem.DistrictName = districts.FirstOrDefault(e => e.Id == childHouse.DistrictId)?.Name;
                            newItem.fullAddress = $"{newItem.HouseName} - {newItem.WardName}/{newItem.DistrictName}/{newItem.ProviceName}";
                        }
                        else
                        {
                            var childHouseParent = _context.Md167Houses.Find(childHouse.Md167HouseId);
                            if (childHouseParent != null)
                            {
                                newItem.HouseName = childHouseParent.HouseNumber;
                                newItem.isKios = true;
                                newItem.ProviceName = provincies.Where(f => f.Id == childHouseParent.ProvinceId).Select(f => f.Name).FirstOrDefault();
                                newItem.DistrictName = districts.Where(f => f.Id == childHouseParent.DistrictId).Select(f => f.Name).FirstOrDefault();
                                newItem.WardName = wards.Where(f => f.Id == childHouseParent.WardId).Select(f => f.Name).FirstOrDefault();
                                newItem.LaneName = lanies.Where(f => f.Id == childHouseParent.LaneId).Select(f => f.Name).FirstOrDefault();
                                newItem.fullAddress = $"{newItem.HouseName} - {newItem.WardName}/{newItem.DistrictName}/{newItem.ProviceName}";
                            }
                        }
                        res.Add(newItem);
                    }
                }
                def.data = res;
                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("GetByPage Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Md167ManagePaymentData input)
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
                input = (Md167ManagePaymentData)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                if (input.Code == null || input.Code == "")
                {
                    def.meta = new Meta(400, "Mã phiếu chi không được để trống!");
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.CreatedById = userId;
                    input.CreatedBy = fullName;
                    _context.Md167ManagePayments.Add(input);

                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm danh sách căn nhà/kios thuộc phiếu chi
                        if (input.housePayments != null)
                        {
                            foreach (var housePayment in input.housePayments)
                            {
                                housePayment.Md167PaymentId = input.Id;
                                housePayment.CreatedBy = fullName;
                                housePayment.CreatedById = userId;

                                _context.HousePayments.Add(housePayment);
                            }
                            await _context.SaveChangesAsync();
                        }

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Thêm mới Phiếu chi " + input.Code, "Md167Payment", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (input.Id > 0)
                        {
                            transaction.Commit();
                            def.meta = new Meta(200, ApiConstants.MessageResource.ADD_SUCCESS);
                            def.data = input;
                            return Ok(def);
                        }
                        else
                        {
                            transaction.Rollback();
                            def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                            return Ok(def);
                        }
                    }
                    catch (DbUpdateException e)
                    {
                        log.Error("DbUpdateException:" + e);
                        transaction.Rollback();
                        if (Md167ManagePaymentExists(input.Id))
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Md167ManagePaymentData input)
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
                input = (Md167ManagePaymentData)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                Md167ManagePayment data = await _context.Md167ManagePayments.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_UPDATE_MESSAGE);
                    return Ok(def);
                }

                if (input.Code == null || input.Code == "")
                {
                    def.meta = new Meta(400, "Mã phiếu chi không được để trống!");
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

                        List<HousePayment> housePayment = _context.HousePayments.Where(l => l.Md167PaymentId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                        if (input.housePayments != null)
                        {
                            foreach (var housePayments in input.housePayments)
                            {
                                HousePayment housePaymentExist = housePayment.Where(l => l.Id == housePayments.Id).FirstOrDefault();
                                if (housePaymentExist == null)
                                {
                                    housePayments.Md167PaymentId = input.Id;
                                    housePayments.CreatedBy = fullName;
                                    housePayments.CreatedById = userId;

                                    _context.HousePayments.Add(housePayments);
                                }
                                else
                                {
                                    housePayments.CreatedAt = housePaymentExist.CreatedAt;
                                    housePayments.CreatedBy = housePaymentExist.CreatedBy;
                                    housePayments.CreatedById = housePaymentExist.CreatedById;
                                    housePayments.Md167PaymentId = input.Id;
                                    housePayments.CreatedBy = fullName;
                                    housePayments.CreatedById = userId;

                                    _context.Update(housePayments);

                                    housePayment.Remove(housePaymentExist);
                                }
                            }
                        }
                        if (housePayment.Count > 0)
                            housePayment.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = AppEnums.EntityStatus.DELETED;
                            });
                        _context.UpdateRange(housePayment);

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Sửa phiếu chi  " + input.Code, "Md167Payment", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, userId, fullName);
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
                        if (!Md167ManagePaymentExists(data.Id))
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
                Md167ManagePayment data = await _context.Md167ManagePayments.FindAsync(id);
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

                    List<HousePayment> valuations = _context.HousePayments.Where(l => l.Md167PaymentId == data.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if (valuations.Count > 0)
                    {
                        valuations.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(valuations);
                    }

                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Xóa phiếu chi " + data.Code, "Md167Payment", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.DELETED, userId, fullName);
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
                        if (!Md167ManagePaymentExists(data.Id))
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

        private bool Md167ManagePaymentExists(int id)
        {
            return _context.Md167ManagePayments.Count(e => e.Id == id) > 0;
        }
    }
}
