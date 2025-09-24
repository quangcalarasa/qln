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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using static IOITQln.Common.Enums.AppEnums;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;

namespace IOITQln.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BlockController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("block", "block");
        private static string functionCode = "BLOCK_MANAGEMENT";
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;

        public BlockController(ApiDbContext context, IMapper mapper, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
        }

        [HttpGet("GetByPage")]
        public IActionResult GetByPage([FromQuery] FilteredPagination paging, [FromQuery] DecreeEnum? decree)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            ExtraDefaultResponse def = new ExtraDefaultResponse();

            var identity = (ClaimsIdentity)User.Identity;
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id").Select(c => c.Value).SingleOrDefault());
            int moduleSystem = int.Parse(identity.Claims.Where(c => c.Type == "ModuleSystem").Select(c => c.Value).SingleOrDefault());

            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }

            try
            {
                TypeApartmentEntity? typeApartmentEntity = null;
                TypeBlockEntity? typeBlockEntity = null;
                if(paging.query.Contains("TypeBlockEntity=2") || paging.query.Contains("TypeBlockEntity = 2") || paging.query.Contains("TypeBlockEntity= 2") || paging.query.Contains("TypeBlockEntity =2"))
                {
                    typeBlockEntity = TypeBlockEntity.BLOCK_RENT;
                    typeApartmentEntity = TypeApartmentEntity.APARTMENT_RENT;

                }
                else if(paging.query.Contains("TypeBlockEntity=1") || paging.query.Contains("TypeBlockEntity = 1") || paging.query.Contains("TypeBlockEntity= 1") || paging.query.Contains("TypeBlockEntity =1"))
                {
                    typeBlockEntity = TypeBlockEntity.BLOCK_NORMAL;
                    typeApartmentEntity = TypeApartmentEntity.APARTMENT_NORMAL;
                }

                if (paging != null)
                {
                    def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                    IQueryable<Block> data;

                    if(decree == null)
                    {
                        if(moduleSystem == (int)ModuleSystem.NOC)
                        {
                            data = (from b in _context.Blocks
                                    join wm in _context.WardManagements on b.Ward equals wm.WardId
                                    where b.Status != EntityStatus.DELETED
                                        && wm.Status != EntityStatus.DELETED
                                        && wm.UserId == userId
                                        && ((b.TypeBlockEntity == typeBlockEntity && typeBlockEntity != null) || (1==1 && typeBlockEntity == null))
                                    select b).Distinct().AsQueryable();
                        }
                        else
                        {
                            data = _context.Blocks.Where(c => ((c.TypeBlockEntity == typeBlockEntity && typeBlockEntity != null) || (1 == 1 && typeBlockEntity == null)) && c.Status != AppEnums.EntityStatus.DELETED).AsQueryable();
                        }
                    }
                    else
                    {
                        if (moduleSystem == (int)ModuleSystem.NOC)
                        {
                            data = (from wm in _context.WardManagements
                                    join b in _context.Blocks on wm.WardId equals b.Ward
                                    join dm in _context.DecreeMaps on b.Id equals dm.TargetId
                                    where b.Status != EntityStatus.DELETED
                                        && dm.Status != EntityStatus.DELETED
                                        && dm.DecreeType1Id == decree
                                        && wm.Status != EntityStatus.DELETED
                                        && wm.UserId == userId
                                        && ((b.TypeBlockEntity == typeBlockEntity && typeBlockEntity != null) || (1 == 1 && typeBlockEntity == null))
                                    select b).AsQueryable();
                        }
                        else
                        {
                            data = (from b in _context.Blocks
                                    join dm in _context.DecreeMaps on b.Id equals dm.TargetId
                                    where b.Status != EntityStatus.DELETED
                                        && dm.Status != EntityStatus.DELETED
                                        && dm.DecreeType1Id == decree
                                        && ((b.TypeBlockEntity == typeBlockEntity && typeBlockEntity != null) || (1 == 1 && typeBlockEntity == null))
                                    select b).AsQueryable();
                        }
                    }

                    if (paging.query != null)
                    {
                        paging.query = HttpUtility.UrlDecode(paging.query);
                    }
                   

                    data = data.Where(paging.query);
                    def.metadata = data.Count();

                    int totalApartment = (from b in data
                                          join a in _context.Apartments on b.Id equals a.BlockId
                                          where a.Status != EntityStatus.DELETED
                                            && ((a.TypeApartmentEntity == typeApartmentEntity && typeApartmentEntity != null) || (1 == 1 && typeApartmentEntity == null))
                                          select a.Id).Distinct().Count();

                    def.extradata = totalApartment;

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
                        var dataSelect = data.Select(paging.select);
                        List<dynamic> dataDef = new List<dynamic>();

                        List<Lane> lanies = _context.Lanies.Where(e => e.Status != EntityStatus.DELETED).ToList();
                        List<Ward> wards = _context.Wards.Where(e => e.Status != EntityStatus.DELETED).ToList();
                        List<District> districts = _context.Districts.Where(e => e.Status != EntityStatus.DELETED).ToList();
                        List<Province> provincies = _context.Provincies.Where(e => e.Status != EntityStatus.DELETED).ToList();


                        foreach (var item in dataSelect.ToDynamicList())
                        {
                            if (paging.select.Contains("Lane") && paging.select.Contains("Ward") && paging.select.Contains("District") && paging.select.Contains("Province") && paging.select.Contains("Name"))
                            {
                                item.Name = item.Address;

                                int laneId = item.Lane;
                                Lane lane = lanies.Where(l => l.Id == laneId).FirstOrDefault();
                                item.Name = item.Name != null && item.Name != "" ? (lane != null ? String.Join(", ", item.Name, lane.Name) : item.Name) : (lane != null ? lane.Name : item.Name);

                                int wardId = item.Ward;
                                Ward ward = wards.Where(p => p.Id == wardId).FirstOrDefault();
                                item.Name = item.Name != null && item.Name != "" ? (ward != null ? String.Join(", ", item.Name, ward.Name) : item.Name) : (ward != null ? ward.Name : item.Name);

                                int districtId = item.District;
                                District district = districts.Where(p => p.Id == districtId).FirstOrDefault();
                                item.Name = item.Name != null && item.Name != "" ? (district != null ? String.Join(", ", item.Name, district.Name) : item.Name) : (district != null ? district.Name : item.Name);

                                int provinceId = item.Province;
                                Province province = provincies.Where(p => p.Id == provinceId).FirstOrDefault();
                                item.Name = item.Name != null && item.Name != "" ? (province != null ? String.Join(", ", item.Name, province.Name) : item.Name) : (province != null ? province.Name : item.Name);
                            }

                            dataDef.Add(item);
                        }

                        def.data = dataDef;
                        //def.data = data.Select(paging.select);
                    }
                    else
                    {
                        List<BlockData> res = _mapper.Map<List<BlockData>>(data.ToList());
                        foreach (BlockData item in res)
                        {
                            item.FullAddress = item.Address;

                            Lane lane = _context.Lanies.Where(l => l.Id == item.Lane).FirstOrDefault();
                            item.FullAddress = item.FullAddress != null && item.FullAddress != "" ? (lane != null ? String.Join(", ", item.FullAddress, lane.Name) : item.FullAddress) : (lane != null ? lane.Name : item.FullAddress);

                            Ward ward = _context.Wards.Where(p => p.Id == item.Ward).FirstOrDefault();
                            item.FullAddress = item.FullAddress != null && item.FullAddress != "" ? (ward != null ? String.Join(", ", item.FullAddress, ward.Name) : item.FullAddress) : (ward != null ? ward.Name : item.FullAddress);

                            District district = _context.Districts.Where(p => p.Id == item.District).FirstOrDefault();
                            item.FullAddress = item.FullAddress != null && item.FullAddress != "" ? (district != null ? String.Join(", ", item.FullAddress, district.Name) : item.FullAddress) : (district != null ? district.Name : item.FullAddress);

                            Province province = _context.Provincies.Where(p => p.Id == item.Province).FirstOrDefault();
                            item.FullAddress = item.FullAddress != null && item.FullAddress != "" ? (province != null ? String.Join(", ", item.FullAddress, province.Name) : item.FullAddress) : (province != null ? province.Name : item.FullAddress);

                            item.levelBlockMaps = _context.LevelBlockMaps.Where(l => l.BlockId == item.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                            item.decreeMaps = _context.DecreeMaps.Where(l => l.TargetId == item.Id && l.Type == AppEnums.TypeDecreeMapping.BLOCK && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                            item.blockMaintextureRaties = _context.BlockMaintextureRaties.Where(b => b.TargetId == item.Id && b.Type == AppEnums.TypeBlockMaintextureRate.BLOCK && b.Status != AppEnums.EntityStatus.DELETED).ToList();

                            List<BlockDetail> blockDetails = _context.BlockDetails.Where(l => l.BlockId == item.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                            List<BlockDetailData> map_BlockDetails = _mapper.Map<List<BlockDetailData>>(blockDetails);
                            foreach(BlockDetailData map_BlockDetail in map_BlockDetails)
                            {
                                Floor floor = _context.Floors.Where(f => f.Id == map_BlockDetail.FloorId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                map_BlockDetail.FloorName = floor != null ? floor.Name : "";
                                map_BlockDetail.FloorCode = floor != null ? floor.Code : 0;

                                Area area = _context.Areas.Where(f => f.Id == map_BlockDetail.AreaId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                map_BlockDetail.AreaName = area != null ? area.Name : "";
                                map_BlockDetail.IsMezzanine = area != null ? area.IsMezzanine : null;
                            }

                            item.blockDetails = map_BlockDetails.OrderBy(x => x.FloorCode).ThenBy(x => x.IsMezzanine).ToList();
                            item.apartmentDetails = _context.ApartmentDetails.Where(a => a.TargetId == item.Id && a.Type == TypeApartmentDetail.BLOCK && a.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                            item.apartmentLandDetails = _context.ApartmentLandDetails.Where(a => a.TargetId == item.Id && a.Type == TypeApartmentLandDetail.BLOCK && a.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();

                            //Lấy thông tin diện tích đất liền kề đã bán
                            if (item.TypeReportApply == TypeReportApply.NHA_RIENG_LE && item.TypeBlockEntity == TypeBlockEntity.BLOCK_NORMAL)
                            {
                                Pricing pricing = _context.Pricings.Where(e => e.BlockId == item.Id && e.TypeReportApply == TypeReportApply.BAN_PHAN_DIEN_TICH_LIEN_KE && e.Status != EntityStatus.DELETED).OrderByDescending(x => x.DateCreate).FirstOrDefault();

                                if (pricing != null)
                                {
                                    item.SellLandArea = pricing.SellLandArea;
                                    item.pricingApartmentLandDetails = _context.PricingApartmentLandDetails.Where(a => a.PricingId == pricing.Id && a.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                                }
                            }

                            //Đếm số căn hộ
                            int totalApartmentByBlock = _context.Apartments.Where(e => e.BlockId == item.Id && e.Status != EntityStatus.DELETED).Count();
                            if(totalApartmentByBlock > 0)
                            {
                                Apartment apartment = _context.Apartments.Where(e => e.BlockId == item.Id && ((e.TypeApartmentEntity == typeApartmentEntity && typeApartmentEntity != null) || (1 == 1 && typeApartmentEntity == null)) && e.Status != EntityStatus.DELETED).OrderBy(x => x.Id).FirstOrDefault();
                                if(apartment != null)
                                {
                                    item.ApartmentInfo = $"{apartment.Code}({totalApartmentByBlock})";
                                }
                            }
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
            catch(Exception ex)
            {
                log.Error("GetByPage Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        // GET: api/Block/1
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
                Block data = await _context.Blocks.FindAsync(id);
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                    return Ok(def);
                }

                BlockData res = _mapper.Map<BlockData>(data);

                res.FullAddress = res.Address;

                Ward ward = _context.Wards.Where(p => p.Id == res.Ward).FirstOrDefault();
                res.FullAddress = res.FullAddress != null && res.FullAddress != "" ? (ward != null ? String.Join(", ", res.FullAddress, ward.Name) : res.FullAddress) : (ward != null ? ward.Name : res.FullAddress);

                District district = _context.Districts.Where(p => p.Id == res.District).FirstOrDefault();
                res.FullAddress = res.FullAddress != null && res.FullAddress != "" ? (district != null ? String.Join(", ", res.FullAddress, district.Name) : res.FullAddress) : (district != null ? district.Name : res.FullAddress);

                Province province = _context.Provincies.Where(p => p.Id == res.Province).FirstOrDefault();
                res.FullAddress = res.FullAddress != null && res.FullAddress != "" ? (province != null ? String.Join(", ", res.FullAddress, province.Name) : res.FullAddress) : (province != null ? province.Name : res.FullAddress);

                res.levelBlockMaps = _context.LevelBlockMaps.Where(l => l.BlockId == res.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                res.decreeMaps = _context.DecreeMaps.Where(l => l.TargetId == res.Id && l.Type == AppEnums.TypeDecreeMapping.BLOCK && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                res.blockMaintextureRaties = _context.BlockMaintextureRaties.Where(b => b.TargetId == res.Id && b.Type == AppEnums.TypeBlockMaintextureRate.BLOCK && b.Status != AppEnums.EntityStatus.DELETED).ToList();

                List<BlockDetail> blockDetails = _context.BlockDetails.Where(l => l.BlockId == res.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<BlockDetailData> map_BlockDetails = _mapper.Map<List<BlockDetailData>>(blockDetails);
                foreach (BlockDetailData map_BlockDetail in map_BlockDetails)
                {
                    Floor floor = _context.Floors.Where(f => f.Id == map_BlockDetail.FloorId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    map_BlockDetail.FloorName = floor != null ? floor.Name : "";
                    map_BlockDetail.FloorCode = floor != null ? floor.Code : 0;

                    Area area = _context.Areas.Where(f => f.Id == map_BlockDetail.AreaId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    map_BlockDetail.AreaName = area != null ? area.Name : "";
                    map_BlockDetail.IsMezzanine = area != null ? area.IsMezzanine : null;
                }
                res.blockDetails = map_BlockDetails.OrderBy(x => x.FloorCode).ThenBy(x => x.IsMezzanine).ToList();
                res.apartmentLandDetails = _context.ApartmentLandDetails.Where(a => a.TargetId == res.Id && a.Type == TypeApartmentLandDetail.BLOCK && a.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                List<ApartmentDetail> apartmentDetails = _context.ApartmentDetails.Where(a => a.TargetId == res.Id && a.Type == TypeApartmentDetail.BLOCK && a.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                res.apartmentDetails = apartmentDetails.OrderBy(x => x.UpdatedAt).ToList();

                List<ApartmentDetailData> map_ApartmentDetails = _mapper.Map<List<ApartmentDetailData>>(apartmentDetails);
                foreach (ApartmentDetailData map_ApartmentDetail in map_ApartmentDetails)
                {
                    Floor floor = _context.Floors.Where(f => f.Id == map_ApartmentDetail.FloorId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    map_ApartmentDetail.FloorName = floor != null ? floor.Name : "";
                    map_ApartmentDetail.FloorCode = floor != null ? floor.Code : 0;

                    Area area = _context.Areas.Where(f => f.Id == map_ApartmentDetail.AreaId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    map_ApartmentDetail.AreaName = area != null ? area.Name : "";
                    map_ApartmentDetail.IsMezzanine = area != null ? area.IsMezzanine : null;
                }

                res.apartmentDetailData = map_ApartmentDetails.OrderBy(x => x.Level).ThenBy(x => x.FloorCode).ThenBy(x => x.IsMezzanine).ToList();

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = res;
                return Ok(def);
            }
            catch(Exception ex)
            {
                log.Error("GetById Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        // POST: api/Block
        [HttpPost]
        public async Task<IActionResult> Post(BlockData input)
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
                input = (BlockData)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                if(input.TypeReportApply == TypeReportApply.NHA_RIENG_LE)
                {
                    if (input.Code == null || input.Code == "")
                    {
                        def.meta = new Meta(400, "Mã không được để trống!");
                        return Ok(def);
                    }

                    //Kiểm tra Mã định danh
                    Block blockCodeExist = _context.Blocks.Where(f => f.Code == input.Code && f.TypeBlockEntity == input.TypeBlockEntity && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    if (blockCodeExist != null)
                    {
                        def.meta = new Meta(211, "Đã tồn tại căn nhà có mã định danh " + input.Code + "!");
                        return Ok(def);
                    }

                    Apartment apartmentCodeExist = _context.Apartments.Where(c => c.Code == input.Code && c.Status != EntityStatus.DELETED).FirstOrDefault();
                    if (apartmentCodeExist != null)
                    {
                        def.meta = new Meta(211, "Đã tồn tại căn hộ có mã định danh " + input.Code + "!");
                        return Ok(def);
                    }
                }

                if (input.Address == null || input.Address == "")
                {
                    def.meta = new Meta(400, "Số căn nhà không được để trống!");
                    return Ok(def);
                }

                if(input.Attactment != null && input .Attactment != "")
                {
                    input.UserIdCreateAttactment = userId;
                }
                

                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.CreatedById = userId;
                    input.CreatedBy = fullName;
                    _context.Blocks.Add(input);

                    try
                    {
                        await _context.SaveChangesAsync();

                        if(input.levelBlockMaps != null)
                        {
                            foreach(var levelBlockMap in input.levelBlockMaps)
                            {
                                levelBlockMap.BlockId = input.Id;
                                levelBlockMap.CreatedBy = fullName;
                                levelBlockMap.CreatedById = userId;

                                _context.LevelBlockMaps.Add(levelBlockMap);
                            }
                        }

                        if (input.blockDetails != null)
                        {
                            foreach (var blockDetail in input.blockDetails)
                            {
                                blockDetail.BlockId = input.Id;
                                blockDetail.CreatedBy = fullName;
                                blockDetail.CreatedById = userId;

                                _context.BlockDetails.Add(blockDetail);
                            }
                        }

                        if (input.blockMaintextureRaties != null)
                        {
                            foreach (var blockMaintextureRate in input.blockMaintextureRaties)
                            {
                                blockMaintextureRate.Type = AppEnums.TypeBlockMaintextureRate.BLOCK;
                                blockMaintextureRate.TargetId = input.Id;
                                blockMaintextureRate.CreatedBy = fullName;
                                blockMaintextureRate.CreatedById = userId;

                                _context.BlockMaintextureRaties.Add(blockMaintextureRate);
                            }
                        }

                        if (input.decreeMaps != null)
                        {
                            foreach (var decreeMap in input.decreeMaps)
                            {
                                decreeMap.Type = AppEnums.TypeDecreeMapping.BLOCK;
                                decreeMap.TargetId = input.Id;
                                decreeMap.CreatedBy = fullName;
                                decreeMap.CreatedById = userId;

                                _context.DecreeMaps.Add(decreeMap);
                            }
                        }

                        //ApartmentDetail
                        if (input.apartmentDetails != null)
                        {
                            foreach (var apartmentDetail in input.apartmentDetails)
                            {
                                apartmentDetail.TargetId = input.Id;
                                apartmentDetail.CreatedBy = fullName;
                                apartmentDetail.CreatedById = userId;
                                apartmentDetail.Type = TypeApartmentDetail.BLOCK;

                                _context.ApartmentDetails.Add(apartmentDetail);

                            }
                        }

                        //ApartmentLandDetail
                        if (input.apartmentLandDetails != null)
                        {
                            foreach (var apartmentLandDetail in input.apartmentLandDetails)
                            {
                                apartmentLandDetail.TargetId = input.Id;
                                apartmentLandDetail.CreatedBy = fullName;
                                apartmentLandDetail.CreatedById = userId;
                                apartmentLandDetail.Type = TypeApartmentLandDetail.BLOCK;

                                _context.ApartmentLandDetails.Add(apartmentLandDetail);
                            }
                        }
                        
                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Thêm mới căn nhà " + input.Name, "Block", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
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
                        if (BlockExists(input.Id))
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

        // PUT: api/Block/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] BlockData input)
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
                input = (BlockData)UtilsService.TrimStringPropertyTypeObject(input);

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

                if (input.TypeReportApply == TypeReportApply.NHA_RIENG_LE)
                {
                    if (input.Code == null || input.Code == "")
                    {
                        def.meta = new Meta(400, "Mã không được để trống!");
                        return Ok(def);
                    }

                    //Kiểm tra Mã định danh
                    Block blockCodeExist = _context.Blocks.Where(f => f.Id != input.Id && f.Code == input.Code && f.TypeBlockEntity == input.TypeBlockEntity && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    if (blockCodeExist != null)
                    {
                        def.meta = new Meta(211, "Đã tồn tại căn nhà có mã định danh " + input.Code + "!");
                        return Ok(def);
                    }

                    Apartment apartmentCodeExist = _context.Apartments.Where(c => c.Code == input.Code && c.Status != EntityStatus.DELETED).FirstOrDefault();
                    if (apartmentCodeExist != null)
                    {
                        def.meta = new Meta(211, "Đã tồn tại căn hộ có mã định danh " + input.Code + "!");
                        return Ok(def);
                    }
                }

                if (input.Address == null || input.Address == "")
                {
                    def.meta = new Meta(400, "Số căn nhà không được để trống!");
                    return Ok(def);
                }

                Block data = await _context.Blocks.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
                if(data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_UPDATE_MESSAGE);
                    return Ok(def);
                }

                //Block codeExist = _context.Blocks.Where(f => f.Code == input.Code && f.Status != AppEnums.EntityStatus.DELETED && f.Id != id).FirstOrDefault();
                //if (codeExist != null)
                //{
                //    def.meta = new Meta(211, "Mã đã tồn tại!");
                //    return Ok(def);
                //}

                if(data.Attactment == null && input.Attactment != null && input.Attactment != "")
                {
                    input.UserIdCreateAttactment = userId;
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

                        //LevelBlockMap
                        List<LevelBlockMap> levelBlockMaps = _context.LevelBlockMaps.Where(l => l.BlockId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).AsNoTracking().ToList();
                        if (input.levelBlockMaps != null)
                        {
                            foreach (var levelBlockMap in input.levelBlockMaps)
                            {
                                LevelBlockMap levelBlockMapExist = levelBlockMaps.Where(l => l.LevelId == levelBlockMap.LevelId).FirstOrDefault();
                                if (levelBlockMapExist == null)
                                {
                                    levelBlockMap.BlockId = input.Id;
                                    levelBlockMap.CreatedBy = fullName;
                                    levelBlockMap.CreatedById = userId;

                                    _context.LevelBlockMaps.Add(levelBlockMap);
                                }
                                else
                                {
                                    levelBlockMaps.Remove(levelBlockMapExist);
                                }
                            }
                        }

                        if (levelBlockMaps.Count > 0)
                        {
                            levelBlockMaps.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = AppEnums.EntityStatus.DELETED;
                            });
                            _context.UpdateRange(levelBlockMaps);
                        }

                        //BlockDetail
                        List<BlockDetail> blockDetails = _context.BlockDetails.Where(l => l.BlockId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).AsNoTracking().ToList();
                        if (input.blockDetails != null)
                        {
                            foreach (var blockDetail in input.blockDetails)
                            {
                                BlockDetail blockDetailExist = blockDetails.Where(l => l.Id == blockDetail.Id).FirstOrDefault();
                                if (blockDetailExist == null)
                                {
                                    blockDetail.BlockId = input.Id;
                                    blockDetail.CreatedBy = fullName;
                                    blockDetail.CreatedById = userId;

                                    _context.BlockDetails.Add(blockDetail);
                                }
                                else
                                {
                                    blockDetail.CreatedAt = blockDetailExist.CreatedAt;
                                    blockDetail.CreatedBy = blockDetailExist.CreatedBy;
                                    blockDetail.CreatedById = blockDetailExist.CreatedById;
                                    blockDetail.BlockId = input.Id;
                                    blockDetail.UpdatedBy = fullName;
                                    blockDetail.UpdatedById = userId;

                                    _context.Update(blockDetail);

                                    blockDetails.Remove(blockDetailExist);
                                }
                            }
                        }

                        blockDetails.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(blockDetails);

                        //BlockMaintextureRate
                        List<BlockMaintextureRate> blockMaintextureRaties = _context.BlockMaintextureRaties.Where(l => l.TargetId == input.Id && l.Type == AppEnums.TypeBlockMaintextureRate.BLOCK && l.Status != AppEnums.EntityStatus.DELETED).AsNoTracking().ToList();
                        if (input.blockMaintextureRaties != null)
                        {
                            foreach (var blockMaintextureRate in input.blockMaintextureRaties)
                            {
                                BlockMaintextureRate blockMaintextureRateExist = blockMaintextureRaties.Where(l => l.Id == blockMaintextureRate.Id).FirstOrDefault();
                                if (blockMaintextureRateExist == null)
                                {
                                    blockMaintextureRate.Type = AppEnums.TypeBlockMaintextureRate.BLOCK;
                                    blockMaintextureRate.TargetId = input.Id;
                                    blockMaintextureRate.CreatedBy = fullName;
                                    blockMaintextureRate.CreatedById = userId;

                                    _context.BlockMaintextureRaties.Add(blockMaintextureRate);
                                }
                                else
                                {
                                    blockMaintextureRate.Type = AppEnums.TypeBlockMaintextureRate.BLOCK;
                                    blockMaintextureRate.CreatedAt = blockMaintextureRateExist.CreatedAt;
                                    blockMaintextureRate.CreatedBy = blockMaintextureRateExist.CreatedBy;
                                    blockMaintextureRate.CreatedById = blockMaintextureRateExist.CreatedById;
                                    blockMaintextureRate.TargetId = input.Id;
                                    blockMaintextureRate.UpdatedBy = fullName;
                                    blockMaintextureRate.UpdatedById = userId;

                                    _context.Update(blockMaintextureRate);

                                    blockMaintextureRaties.Remove(blockMaintextureRateExist);
                                }
                            }
                        }

                        if (blockMaintextureRaties.Count > 0)
                        {
                            blockMaintextureRaties.ForEach(item => {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = AppEnums.EntityStatus.DELETED;
                            });
                            _context.UpdateRange(blockMaintextureRaties);
                        }

                        //DecreeMap
                        List<DecreeMap> decreeMaps = _context.DecreeMaps.Where(l => l.TargetId == input.Id && l.Type == AppEnums.TypeDecreeMapping.BLOCK && l.Status != AppEnums.EntityStatus.DELETED).AsNoTracking().ToList();
                        if (input.decreeMaps != null)
                        {
                            foreach (var decreeMap in input.decreeMaps)
                            {
                                DecreeMap decreeMapExist = decreeMaps.Where(l => l.DecreeType1Id == decreeMap.DecreeType1Id).FirstOrDefault();
                                if (decreeMapExist == null)
                                {
                                    decreeMap.TargetId = input.Id;
                                    decreeMap.Type = AppEnums.TypeDecreeMapping.BLOCK;
                                    decreeMap.CreatedBy = fullName;
                                    decreeMap.CreatedById = userId;

                                    _context.DecreeMaps.Add(decreeMap);
                                }
                                else
                                {
                                    decreeMaps.Remove(decreeMapExist);
                                }
                            }
                        }

                        if (decreeMaps.Count > 0)
                        {
                            decreeMaps.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = AppEnums.EntityStatus.DELETED;
                            });
                            _context.UpdateRange(decreeMaps);
                        }

                        //ApartmentDetail
                        List<ApartmentDetail> apartmentDetails = _context.ApartmentDetails.Where(l => l.TargetId == input.Id && l.Type == TypeApartmentDetail.BLOCK && l.Status != AppEnums.EntityStatus.DELETED).AsNoTracking().ToList();
                        if (input.apartmentDetails != null)
                        {
                            foreach (var apartmentDetail in input.apartmentDetails)
                            {
                                ApartmentDetail apartmentDetailExist = apartmentDetails.Where(l => l.Id == apartmentDetail.Id).FirstOrDefault();
                                if (apartmentDetailExist == null)
                                {
                                    apartmentDetail.TargetId = input.Id;
                                    apartmentDetail.CreatedBy = fullName;
                                    apartmentDetail.CreatedById = userId;
                                    apartmentDetail.UpdatedAt = DateTime.Now;
                                    apartmentDetail.Type = TypeApartmentDetail.BLOCK;

                                    _context.ApartmentDetails.Add(apartmentDetail);
                                }
                                else
                                {
                                    apartmentDetail.TargetId = input.Id;
                                    apartmentDetail.CreatedBy = apartmentDetailExist.CreatedBy;
                                    apartmentDetail.CreatedById = apartmentDetailExist.CreatedById;
                                    apartmentDetail.UpdatedBy = fullName;
                                    apartmentDetail.UpdatedById = userId;
                                    apartmentDetail.UpdatedAt = DateTime.Now;
                                    apartmentDetail.Type = TypeApartmentDetail.BLOCK;

                                    _context.Update(apartmentDetail);

                                    apartmentDetails.Remove(apartmentDetailExist);
                                }
                            }
                        }

                        if (apartmentDetails.Count > 0)
                        {
                            apartmentDetails.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = AppEnums.EntityStatus.DELETED;
                            });
                            _context.UpdateRange(apartmentDetails);
                        }

                        //ApartmentLandDetail
                        List<ApartmentLandDetail> apartmentLandDetails = _context.ApartmentLandDetails.Where(l => l.TargetId == input.Id && l.Type == TypeApartmentLandDetail.BLOCK && l.Status != AppEnums.EntityStatus.DELETED).AsNoTracking().ToList();
                        if (input.apartmentLandDetails != null)
                        {
                            foreach (var apartmentLandDetail in input.apartmentLandDetails)
                            {
                                ApartmentLandDetail apartmentLandDetailExist = apartmentLandDetails.Where(l => l.Id == apartmentLandDetail.Id).FirstOrDefault();
                                if (apartmentLandDetailExist == null)
                                {
                                    apartmentLandDetail.TargetId = input.Id;
                                    apartmentLandDetail.CreatedBy = fullName;
                                    apartmentLandDetail.CreatedById = userId;
                                    apartmentLandDetail.UpdatedAt = DateTime.Now;
                                    apartmentLandDetail.Type = TypeApartmentLandDetail.BLOCK;

                                    _context.ApartmentLandDetails.Add(apartmentLandDetail);
                                }
                                else
                                {
                                    apartmentLandDetail.TargetId = input.Id;
                                    apartmentLandDetail.CreatedBy = apartmentLandDetailExist.CreatedBy;
                                    apartmentLandDetail.CreatedById = apartmentLandDetailExist.CreatedById;
                                    apartmentLandDetail.UpdatedBy = fullName;
                                    apartmentLandDetail.UpdatedById = userId;
                                    apartmentLandDetail.UpdatedAt = DateTime.Now;
                                    apartmentLandDetail.Type = TypeApartmentLandDetail.BLOCK;

                                    _context.Update(apartmentLandDetail);

                                    apartmentLandDetails.Remove(apartmentLandDetailExist);
                                }
                            }
                        }

                        if (apartmentLandDetails.Count > 0)
                        {
                            apartmentLandDetails.ForEach(item => {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = AppEnums.EntityStatus.DELETED;
                            });
                            _context.UpdateRange(apartmentLandDetails);
                        }

                        if (input.editHistory != null)
                        {
                            input.editHistory.TargetId = id;
                            input.editHistory.CreatedById = userId;
                            input.editHistory.CreatedBy = fullName;
                            input.editHistory.TypeEditHistory = TypeEditHistory.BLOCK;

                            _context.EditHistories.Add(input.editHistory);
                        }

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Sửa căn nhà " + input.Name, "Block", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, userId, fullName);
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
                        if (!BlockExists(data.Id))
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

        // DELETE: api/Block/1
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
                Block data = await _context.Blocks.FindAsync(id);
                if(data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_DELETE_MESSAGE);
                    return Ok(def);
                }

                //Kiểm tra dữ liệu liên quan có tồn tại thì không thể xóa
                //Căn hộ
                Apartment apartment = _context.Apartments.Where(a => a.BlockId == id && a.Status != EntityStatus.DELETED).FirstOrDefault();
                if(apartment != null)
                {
                    def.meta = new Meta(222, "Dữ liệu căn hộ liên quan còn tồn tại. Không thể xóa căn nhà này!");
                    return Ok(def);
                }

                //Biên bản
                Pricing pricing = _context.Pricings.Where(c => c.BlockId == id && c.Status != EntityStatus.DELETED).FirstOrDefault();
                if(pricing != null)
                {
                    def.meta = new Meta(222, "Dữ liệu biên bản tính giá liên quan còn tồn tại. Không thể xóa căn nhà này!");
                    return Ok(def);
                }

                //Kiểm tra dữ liệu liên quan đến căn nhà khác

                using (var transaction = _context.Database.BeginTransaction())
                {
                    data.UpdatedAt = DateTime.Now;
                    data.UpdatedById = userId;
                    data.UpdatedBy = fullName;
                    data.Status = AppEnums.EntityStatus.DELETED;
                    _context.Update(data);

                    //Xóa LevelBlockMap
                    List<LevelBlockMap> levelBlockMaps = _context.LevelBlockMaps.Where(l => l.BlockId == data.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if(levelBlockMaps.Count > 0)
                    {
                        levelBlockMaps.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(levelBlockMaps);
                    }

                    //Xóa BlockDetail
                    List<BlockDetail> blockDetails = _context.BlockDetails.Where(l => l.BlockId == data.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if (blockDetails.Count > 0)
                    {
                        blockDetails.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(blockDetails);
                    }

                    //Xóa BlockMaintextureRate
                    List<BlockMaintextureRate> blockMaintextureRaties = _context.BlockMaintextureRaties.Where(l => l.TargetId == data.Id && l.Type == AppEnums.TypeBlockMaintextureRate.BLOCK && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if (blockMaintextureRaties.Count > 0)
                    {
                        blockMaintextureRaties.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(blockMaintextureRaties);
                    }

                    //Xóa DecreeMap
                    List<DecreeMap> decreeMaps = _context.DecreeMaps.Where(l => l.TargetId == data.Id && l.Type == AppEnums.TypeDecreeMapping.BLOCK && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if (decreeMaps.Count > 0)
                    {
                        decreeMaps.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(decreeMaps);
                    }

                    //Xóa ApartmentDetails: TH Nhà riêng lẻ
                    List<ApartmentDetail> apartmentDetails = _context.ApartmentDetails.Where(a => a.TargetId == id && a.Type == TypeApartmentDetail.BLOCK && a.Status != EntityStatus.DELETED).ToList();
                    if (apartmentDetails.Count > 0)
                    {
                        apartmentDetails.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(apartmentDetails);
                    }

                    //Xóa ApartmentLandDetail: TH Nhà riêng lẻ
                    List<ApartmentLandDetail> apartmentLandDetails = _context.ApartmentLandDetails.Where(a => a.TargetId == id && a.Type == TypeApartmentLandDetail.BLOCK && a.Status != EntityStatus.DELETED).ToList();
                    if (apartmentLandDetails.Count > 0)
                    {
                        apartmentLandDetails.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(apartmentLandDetails);
                    }

                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Xóa căn nhà " + data.Name, "Block", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.DELETED, userId, fullName);
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
                        if (!BlockExists(data.Id))
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

        //API kiểm tra thông tin tình trạng Mã định danh
        [HttpPost("checkCodeStatus")]
        public async Task<IActionResult> CheckCodeStatus(dynamic input)
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
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }

            try
            {
                string code = input.Code;
                dynamic output = new System.Dynamic.ExpandoObject();
                output.CodeStatus = CodeStatus.CHUA_TON_TAI;

                //Kiểm tra mã định danh đã có trên hệ thống thì lấy ra Id của căn nhà để chỉnh sửa
                Block block_normal = _context.Blocks.Where(a => a.Code == code && a.TypeBlockEntity == TypeBlockEntity.BLOCK_NORMAL && a.Status != EntityStatus.DELETED).FirstOrDefault();
                if(block_normal != null)
                {
                    output.CodeStatus = CodeStatus.DA_CAP_NHAT;
                    output.Data = block_normal;
                }
                else
                {
                    Block block_rent = _context.Blocks.Where(a => a.Code == code && a.TypeBlockEntity == TypeBlockEntity.BLOCK_RENT && a.Status != EntityStatus.DELETED).FirstOrDefault();
                    if(block_rent != null)
                    {
                        output.CodeStatus = CodeStatus.DA_TON_TAI;
                        output.Data = block_rent;
                    }
                    else
                    {
                        //Kiểm tra xem căn hộ thuộc căn nhà
                        Apartment apartment_normal = _context.Apartments.Where(a => a.Code == code && a.TypeApartmentEntity == TypeApartmentEntity.APARTMENT_NORMAL && a.Status != EntityStatus.DELETED).FirstOrDefault();
                        if(apartment_normal != null)
                        {
                            Block apartment_block = _context.Blocks.Where(a => a.Id == apartment_normal.BlockId && a.Status != EntityStatus.DELETED).FirstOrDefault();
                            if(apartment_block != null)
                            {
                                output.CodeStatus = CodeStatus.DA_CAP_NHAT;
                                output.Data = apartment_block;
                            }
                        }
                        else
                        {
                            Apartment apartment_rent = _context.Apartments.Where(a => a.Code == code && a.TypeApartmentEntity == TypeApartmentEntity.APARTMENT_RENT && a.Status != EntityStatus.DELETED).FirstOrDefault();
                            if(apartment_rent != null)
                            {
                                Block apartment_block = _context.Blocks.Where(a => a.Id == apartment_rent.BlockId && a.Status != EntityStatus.DELETED).FirstOrDefault();
                                if (apartment_block != null)
                                {
                                    output.CodeStatus = CodeStatus.DA_TON_TAI;
                                    output.Data = apartment_block;
                                }
                            }
                        }
                    }
                }

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = output;
                return Ok(def);
            }
            catch (Exception e)
            {
                log.Error("CheckCodeStatus Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        //API thử đọc file excel
        //[HttpPost]
        //[Route("ImportDataExcel")]
        //public IActionResult ImportDataExcel()
        //{
        //    string accessToken = Request.Headers[HeaderNames.Authorization];
        //    Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
        //    if (token == null)
        //    {
        //        return Unauthorized();
        //    }

        //    DefaultResponse def = new DefaultResponse();
        //    try
        //    {
        //        int i = 0;
        //        var httpRequest = Request.Form.Files;
        //        //decimal totalValue = 0;
        //        foreach (var file in httpRequest)
        //        {
        //            var postedFile = httpRequest[i];
        //            if (postedFile != null && postedFile.Length > 0)
        //            {

        //                int MaxContentLength = 1024 * 1024 * 32; //Size = 32 MB  

        //                var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
        //                var name = postedFile.FileName.Substring(0, postedFile.FileName.LastIndexOf('.'));
        //                var extension = ext.ToLower();

        //                if (postedFile.Length > MaxContentLength)
        //                {
        //                    var message = string.Format("Vui lòng Up file có dung lượng nhỏ hơn 32 MB!");
        //                    def.meta = new Meta(600, message);
        //                    return Ok(def);
        //                }
        //                else
        //                {
        //                    byte[] fileData = null;
        //                    using (var binaryReader = new BinaryReader(file.OpenReadStream()))
        //                    {
        //                        fileData = binaryReader.ReadBytes((int)file.Length);
        //                        using (MemoryStream ms = new MemoryStream(fileData))
        //                        {
        //                           importData(ms, 0, 2);
        //                        }
        //                    }
        //                }
        //            }
        //            i++;
        //        }

        //        def.meta = new Meta(200, "Success");
        //        def.data = null;
        //        def.metadata = null;
        //        return Ok(def);
        //    }
        //    catch (Exception ex)
        //    {
        //        def.meta = new Meta(400, "Bad request");
        //        return Ok(def);
        //    }
        //}

        //API get ds tòa nhà bán theo Nghị định
        //[HttpPost("getBlockByDecree")]
        //public async Task<IActionResult> GetBlockByDecree([FromBody] List<DecreeEnum> decrees)
        //{
        //    string accessToken = Request.Headers[HeaderNames.Authorization];
        //    Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
        //    if (token == null)
        //    {
        //        return Unauthorized();
        //    }

        //    DefaultResponse def = new DefaultResponse();
        //    //check role
        //    var identity = (ClaimsIdentity)User.Identity;
        //    string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
        //    if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
        //    {
        //        def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
        //        return Ok(def);
        //    }

        //    try
        //    {
        //        List<BlockDataSm> data = await (from dm in _context.DecreeMaps
        //                                        join b in _context.Blocks on dm.TargetId equals b.Id
        //                                        where dm.Status != EntityStatus.DELETED
        //                                        && b.Status != EntityStatus.DELETED
        //                                        && dm.Type == TypeDecreeMapping.BLOCK
        //                                        && b.TypeReportApply != TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG
        //                                        && b.TypeReportApply != TypeReportApply.NHA_RIENG_LE
        //                                        && decrees.Contains(dm.DecreeType1Id)
        //                                        select new BlockDataSm { 
        //                                            Id = b.Id,
        //                                            Address = b.Address + "(" + (b.TypeReportApply == TypeReportApply.NHA_HO_CHUNG ? "Nhà hộ chung" : (b.TypeReportApply == TypeReportApply.NHA_CHUNG_CU ? "Nhà chung cư" : "Nhà riêng lẻ" )) + ")"
        //                                        }).Distinct().ToListAsync();

        //        def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
        //        def.data = data;
        //        return Ok(def);
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error("GetBlockByDecree Error:" + ex);
        //        def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
        //        return Ok(def);
        //    }
        //}

        //public static void importData(MemoryStream ms, int sheetnumber, int rowStart)
        //{
        //    XSSFWorkbook workbook = new XSSFWorkbook(ms);
        //    ISheet sheet = workbook.GetSheetAt(sheetnumber);
        //    IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
        //    int k = 1;
        //    for (int rowIndex = rowStart; rowIndex <= sheet.LastRowNum; rowIndex++)
        //    {
        //        var row = sheet.GetRow(rowIndex);
                
        //    }
        //}


        private bool BlockExists(int id)
        {
            return _context.Blocks.Count(e => e.Id == id) > 0;
        }

        [HttpGet("GetTypeReport2")]
        public IActionResult GroupedData()
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
                List<Block> data = _context.Blocks.Where(l =>l.TypeReportApply == AppEnums.TypeReportApply.NHA_RIENG_LE && l.Status != AppEnums.EntityStatus.DELETED).ToList();
               
                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = data;
                return Ok(def);
            }
            catch
            {
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }


        #region export căn nhà thuê
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
            List<Block> rentingPrices = _context.Blocks.Where(p => p.Status != AppEnums.EntityStatus.DELETED).ToList();
            List<BlockData> mapper_data = _mapper.Map<List<BlockData>>(rentingPrices);
            foreach (var map in mapper_data)
            {
                map.TypeBlockName = _context.TypeBlocks.Where(f => f.Id == map.TypeBlockId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                map.ProvinceName = _context.Provincies.Where(f => f.Id == map.Province && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                map.DistrictName = _context.Districts.Where(f => f.Id == map.District && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                map.WardName = _context.Wards.Where(f => f.Id == map.Ward && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                map.LaneName = _context.Lands.Where(f => f.Id == map.Lane && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                map.CustomerName = _context.Customers.Where(f => f.Id == map.CustomerId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.FullName).FirstOrDefault();

                map.levelBlockMaps = _context.LevelBlockMaps.Where(l => l.BlockId == map.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();

                string usageStatus = UtilsService.NonUnicode(map.usage_Status);
                if (map.UsageStatus == UsageStatus.DANG_CHO_THUE)
                {
                    usageStatus = "Đang cho thuê";
                }
                else if (map.UsageStatus == UsageStatus.NHA_TRONG)
                {
                    usageStatus = "Nhà trống";
                }
                else if (map.UsageStatus == UsageStatus.TRANH_CHAP)
                {
                    usageStatus = "Bị chiếm dụng";
                }
                else if (map.UsageStatus == UsageStatus.CAC_TRUONG_HOP_KHAC)
                {
                    usageStatus = "Các trường hợp khác";
                }

                string type_house = UtilsService.NonUnicode(map.TypeHouseName);
                if (map.TypeHouse == TypeHouse.Eligible_Sell)
                {
                    type_house = "Đủ điều kiện";
                }
                else if (map.TypeHouse == TypeHouse.Not_Eligible_Sell)
                {
                    type_house = "Chưa đủ điều kiện";
                }
                else if (map.TypeHouse == TypeHouse.Not_Sell)
                {
                    type_house = "Không đủ điều kiện";
                }

                string rp_apply = UtilsService.NonUnicode(map.type_rpApply);
                if(map.TypeReportApply == TypeReportApply.NHA_HO_CHUNG)
                {
                    rp_apply = "Nhà hộ chung";
                }
                else if(map.TypeReportApply == TypeReportApply.NHA_RIENG_LE)
                {
                    rp_apply = "Nhà riêng lẻ";
                }
                else if(map.TypeReportApply == TypeReportApply.NHA_CHUNG_CU)
                {
                    rp_apply = "Nhà chung cư";
                }
            }
            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"NOCexcel/can-nha-thue.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);

            MemoryStream ms = WriteDataToExcel(templatePath, 0, mapper_data);
            byte[] byteArrayContent = ms.ToArray();
            return new FileContentResult(byteArrayContent, "application/octet-stream");
        }

        private static MemoryStream WriteDataToExcel(string templatePath, int sheetnumber, List<BlockData> data)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowStart = 2;

            if (sheet != null)
            {
                int datacol = 27;
                try
                {
                    //style body
                    List<ICellStyle> rowStyle = new List<ICellStyle>();
                    for (int i = 0; i < datacol; i++)
                    {
                        rowStyle.Add(sheet.GetRow(0).GetCell(i).CellStyle);
                    }
                    //Thêm row
                    int k = 0;
                    foreach (var item in data)
                    {
                        try
                        {
                            XSSFRow row = (XSSFRow)sheet.CreateRow(rowStart);
                            for (int i = 0; i < datacol; i++)
                            {
                                row.CreateCell(i).CellStyle = rowStyle[i];

                                if (i == 0)
                                {
                                    row.GetCell(i).SetCellValue(k + 1);
                                }
                                else if (i == 1)
                                {
                                    row.GetCell(i).SetCellValue(item.usage_Status);
                                }
                                else if (i == 2)
                                {
                                    row.GetCell(i).SetCellValue(item.TypeBlockName);
                                }
                                else if (i == 3)
                                {
                                    row.GetCell(i).SetCellValue(item.TypeBlockName);
                                }
                                else if (i == 4)
                                {
                                    row.GetCell(i).SetCellValue(item.ProvinceName);
                                }
                                else if (i == 5)
                                {
                                    row.GetCell(i).SetCellValue(item.DistrictName);
                                }
                                else if (i == 6)
                                {
                                    row.GetCell(i).SetCellValue(item.WardName);
                                }
                                else if (i == 7)
                                {
                                    row.GetCell(i).SetCellValue(item.LaneName);
                                }
                                else if (i == 8)
                                {
                                    row.GetCell(i).SetCellValue(item.Address);
                                }
                                else if (i == 9)
                                {
                                    row.GetCell(i).SetCellValue(item.Floor);
                                }
                                else if (i == 10)
                                {
                                    row.GetCell(i).SetCellValue(item.DateRecord.ToString("dd/MM/yyyy"));
                                }
                                else if (i == 11)
                                {
                                    row.GetCell(i).SetCellValue(item.TypeHouseName);
                                }
                                else if (i == 12)
                                {
                                    row.GetCell(i).SetCellValue(item.DateApply.ToString("dd/MM/yyyy"));
                                }
                                else if (i == 13)
                                {
                                    row.GetCell(i).SetCellValue(item.Code);
                                }
                                else if (i == 14)
                                {
                                    row.GetCell(i).SetCellValue((double)item.CampusArea);
                                }
                                else if (i == 15)
                                {
                                    row.GetCell(i).SetCellValue(item.ConstructionAreaValue);
                                }
                                else if (i == 16)
                                {
                                    row.GetCell(i).SetCellValue(item.UseAreaValue);
                                }
                                else if (i == 17)
                                {
                                    row.GetCell(i).SetCellValue(item.UseAreaNote);
                                }
                                else if (i == 18)
                                {
                                    if (item.EstablishStateOwnership == true)
                                    {
                                        row.GetCell(i).SetCellValue("TRUE");
                                    }
                                    else
                                    {
                                        row.GetCell(i).SetCellValue("FALSE");
                                    }
                                }
                                else if (i == 19)
                                {
                                    if (item.Dispute == true)
                                    {
                                        row.GetCell(i).SetCellValue("TRUE");
                                    }
                                    else
                                    {
                                        row.GetCell(i).SetCellValue("FALSE");
                                    }
                                }
                                else if (i == 20)
                                {
                                    if (item.TakeOver == true)
                                    {
                                        row.GetCell(i).SetCellValue("TRUE");
                                    }
                                    else
                                    {
                                        row.GetCell(i).SetCellValue("FALSE");
                                    }
                                }
                                else if (i == 21)
                                {
                                    if (item.Blueprint == true)
                                    {
                                        row.GetCell(i).SetCellValue("TRUE");
                                    }
                                    else
                                    {
                                        row.GetCell(i).SetCellValue("FALSE");
                                    }
                                }
                                else if (i == 22)
                                {
                                    row.GetCell(i).SetCellValue(item.CustomerName);
                                }
                                else if (i == 23)
                                {
                                    row.GetCell(i).SetCellValue(item.usage_Status);
                                }
                                //else if (i == 3)
                                //{
                                //    row.GetCell(i).SetCellValue(item.TypeBlockName);
                                //}
                                //else if (i == 4)
                                //{
                                //    row.GetCell(i).SetCellValue((int)item.LevelId);
                                //}
                                //else if (i == 5)
                                //{
                                //    row.GetCell(i).SetCellValue((double)item.Price);
                                //}
                                //else if (i == 6)
                                //{
                                //    row.GetCell(i).SetCellValue(item.UnitPriceName);
                                //}
                                //else if (i == 7)
                                //{
                                //    row.GetCell(i).SetCellValue(item.Note);
                                //}

                            }
                            rowStart++;
                            k++;
                        }
                        catch (Exception ex)
                        {
                            log.Error("ExportDataExcel:" + ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("ExportDataExcel:" + ex);
                }
            }

            sheet.ForceFormulaRecalculation = true;

            MemoryStream ms = new MemoryStream();

            workbook.Write(ms);

            return ms;
        }

        #endregion

        //#region import căn nhà
        //[HttpPost]
        //[Route("ImportDataExcel")]
        //public IActionResult ImportDataExcel()
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
        //    if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.IMPORT))
        //    {
        //        def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_ACTION_MESSAGE);
        //        return Ok(def);
        //    }
        //    try
        //    {
        //        int i = 0;
        //        var httpRequest = Request.Form.Files;
        //        ImportHistory importHistory = new ImportHistory();
        //        importHistory.Type = AppEnums.ImportHistoryType.NocBlock;

        //        List<BlockData> data = new List<BlockData>();

        //        foreach (var file in httpRequest)
        //        {
        //            var postedFile = httpRequest[i];
        //            if (postedFile != null && postedFile.Length > 0)
        //            {
        //                IList<string> AllowedDocuments = new List<string> { ".xls", ".xlsx" };
        //                int MaxContentLength = 1024 * 1024 * 32; //Size = 32 MB

        //                var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
        //                var name = postedFile.FileName.Substring(0, postedFile.FileName.LastIndexOf('.'));
        //                var extension = ext.ToLower();

        //                bool checkFile = true;
        //                if (AllowedDocuments.Contains(extension))
        //                {
        //                    checkFile = false;
        //                }

        //                if (checkFile)
        //                {
        //                    var message = string.Format("Vui lòng upload đúng định dạng file excel!");
        //                    def.meta = new Meta(600, message);
        //                    return Ok(def);
        //                }

        //                if (postedFile.Length > MaxContentLength)
        //                {
        //                    var message = string.Format("Vui lòng Up file có dung lượng nhỏ hơn 32 MB!");
        //                    def.meta = new Meta(600, message);
        //                    return Ok(def);
        //                }
        //                else
        //                {
        //                    string folderName = _configuration["AppSettings:BaseUrlImportHistory"]; ;
        //                    string webRootPath = _hostingEnvironment.WebRootPath;
        //                    string newPath = Path.Combine(webRootPath, folderName);
        //                    if (!Directory.Exists(newPath))
        //                    {
        //                        Directory.CreateDirectory(newPath);
        //                    }

        //                    string fileNameCheck = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.ToString().Trim('"');
        //                    if (UtilsService.ConvertUrlpath(fileNameCheck).Contains("../") || UtilsService.ConvertUrlpath(fileNameCheck).Contains("..\\") || fileNameCheck.IndexOfAny(Path.GetInvalidPathChars()) > -1)
        //                    {
        //                        var vMessage = "Tên file không hợp lệ!";

        //                        def.meta = new Meta(202, vMessage);
        //                        return Ok(def);
        //                    }

        //                    DateTime now = DateTime.Now;
        //                    string fileName = name + "_" + now.ToString("yyyyMMddHHmmssfff") + extension;
        //                    string fullPath = Path.Combine(newPath, fileName);
        //                    using (var stream = new FileStream(fullPath, FileMode.Create))
        //                    {
        //                        postedFile.CopyTo(stream);
        //                    }

        //                    importHistory.FileUrl = fileName;

        //                    byte[] fileData = null;
        //                    using (var binaryReader = new BinaryReader(file.OpenReadStream()))
        //                    {
        //                        fileData = binaryReader.ReadBytes((int)file.Length);
        //                        using (MemoryStream ms = new MemoryStream(fileData))
        //                        {
        //                            data = importData(ms, 0, 3);
        //                        }
        //                    }
        //                }
        //            }
        //            i++;
        //        }

        //        List<BlockData> dataValid = new List<BlockData>();
        //        using (var transaction = _context.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                foreach (var dataItem in data)
        //                {
        //                    if (dataItem.Valid == true)
        //                    {



        //                        dataItem.CreatedById = -1;
        //                        dataValid.Add(dataItem);
        //                    }
        //                }
        //                importHistory.Data = data.Cast<dynamic>().ToList();
        //                importHistory.CreatedById = userId;
        //                importHistory.CreatedBy = fullName;

        //                _context.ImportHistories.Add(importHistory);
        //                _context.Blocks.AddRange(dataValid);

        //                _context.SaveChanges();

        //                transaction.Commit();
        //                def.meta = new Meta(200, ApiConstants.MessageResource.DELETE_SUCCESS);
        //                def.metadata = data.Count;
        //                def.data = data;
        //            }
        //            catch (Exception ex)
        //            {
        //                log.Error("ImportDataExcel:" + ex);
        //                transaction.Rollback();
        //                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
        //            }
        //        }
        //        return Ok(def);
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error("ImportDataExcel:" + ex);
        //        def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
        //        return Ok(def);
        //    }
        //}

        //public static List<BlockData> importData(MemoryStream ms, int sheetnumber, int rowStart)
        //{
        //    XSSFWorkbook workbook = new XSSFWorkbook(ms);
        //    ISheet sheet = workbook.GetSheetAt(sheetnumber);

        //    List<BlockData> res = new List<BlockData>();
        //    for (int row = rowStart; row <= sheet.LastRowNum; row++)
        //    {
        //        if (sheet.GetRow(row) != null)
        //        {
        //            BlockData inputDetail = new BlockData();
        //            inputDetail.Valid = true;
        //            inputDetail.ErrMsg = "";

        //            for (int i = 0; i < 39; i++)
        //            {
        //                try
        //                {
        //                    var cell = sheet.GetRow(row).GetCell(i, MissingCellPolicy.RETURN_NULL_AND_BLANK);

        //                    //Lấy giá trị trong cell
        //                    string str = UtilsService.getCellValue(cell);
        //                    if (i == 0)
        //                    {
        //                        try
        //                        {
        //                            if (str != "")
        //                            {
        //                                inputDetail.Index = int.Parse(str);
        //                            }
        //                        }
        //                        catch
        //                        {
        //                            inputDetail.Valid = false;
        //                            inputDetail.ErrMsg += "Lỗi cột Số thứ tự\n";
        //                        }
        //                    }
        //                    else if (i == 1)
        //                    {
        //                        try
        //                        {
        //                            if (str != "")
        //                            {
        //                                inputDetail.Code = str;
        //                            }
        //                            else
        //                            {
        //                                inputDetail.Valid = false;
        //                                inputDetail.ErrMsg += "Cột mã định danh chưa có dữ liệu";
        //                            }
        //                        }
        //                        catch
        //                        {
        //                            inputDetail.Valid = false;
        //                            inputDetail.ErrMsg += "Lỗi cột mã định danh\n";
        //                        }
        //                    }
        //                    else if (i == 1)
        //                    {
        //                        try
        //                        {
        //                            if (str != "")
        //                            {
        //                                inputDetail.Code = str;
        //                            }
        //                            else
        //                            {
        //                                inputDetail.Valid = false;
        //                                inputDetail.ErrMsg += "Cột mã định danh chưa có dữ liệu";
        //                            }
        //                        }
        //                        catch
        //                        {
        //                            inputDetail.Valid = false;
        //                            inputDetail.ErrMsg += "Lỗi cột mã định danh\n";
        //                        }
        //                    }
        //                    else if (i == 3)
        //                    {
        //                        try
        //                        {
        //                            if (str != "")
        //                            {
        //                                inputDetail.Code = str;
        //                            }
        //                            else
        //                            {
        //                                inputDetail.Valid = false;
        //                                inputDetail.ErrMsg += "Cột cấp nhà chưa có dữ liệu";
        //                            }
        //                        }
        //                        catch
        //                        {
        //                            inputDetail.Valid = false;
        //                            inputDetail.ErrMsg += "Lỗi cấp nhà\n";
        //                        }
        //                    }
        //                    else if (i == 4)
        //                    {
        //                        try
        //                        {
        //                            if (str != "")
        //                            {
        //                                inputDetail.FloorBlockMap = str;
        //                            }
        //                            else
        //                            {
        //                                inputDetail.Valid = false;
        //                                inputDetail.ErrMsg += "Cột Số tầng chưa có dữ liệu";
        //                            }
        //                        }
        //                        catch
        //                        {
        //                            inputDetail.Valid = false;
        //                            inputDetail.ErrMsg += "Lỗi cột Cột Số tầng\n";
        //                        }
        //                    }

        //                }
        //                catch (Exception ex)
        //                {
        //                    inputDetail.Valid = false;
        //                    inputDetail.ErrMsg += "Lỗi dữ liệu\n";
        //                    log.Error("Exception:" + ex);
        //                }
        //            }
        //            res.Add(inputDetail);
        //        }
        //    }
        //    return res;
        //}
        //#endregion
    }
}
