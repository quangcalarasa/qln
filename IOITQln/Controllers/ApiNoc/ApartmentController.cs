using AutoMapper;
using IOITQln.Common.Constants;
using IOITQln.Common.Enums;
using IOITQln.Common.Services;
using IOITQln.Common.ViewModels.Common;
using IOITQln.Entities;
using IOITQln.Models.Data;
using IOITQln.Models.Dto;
using IOITQln.Persistence;
using IOITQln.QuickPriceNOC.Service;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using static IOITQln.Common.Enums.AppEnums;
using IOITQln.QuickPriceNOC.Interface;
namespace IOITQln.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ApartmentController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("apartment", "apartment");

        private static string functionCode = "APARTMENT_MANAGEMENT";

        private readonly ApiDbContext _context;

        private IMapper _mapper;

        private readonly IChangePrice _changePrice;

        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ApartmentController(ApiDbContext context, IMapper mapper, IChangePrice changePrice, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _mapper = mapper;
            _serviceScopeFactory = serviceScopeFactory;
            _changePrice = changePrice;
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

            DefaultResponse def = new DefaultResponse();

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

                if (paging.query.Contains("TypeApartmentEntity=2") || paging.query.Contains("TypeApartmentEntity = 2") || paging.query.Contains("TypeApartmentEntity= 2") || paging.query.Contains("TypeBlockEntity =2"))
                {
                    typeApartmentEntity = TypeApartmentEntity.APARTMENT_RENT;
                    typeBlockEntity = TypeBlockEntity.BLOCK_RENT;
                }
                else if (paging.query.Contains("TypeApartmentEntity=1") || paging.query.Contains("TypeApartmentEntity = 1") || paging.query.Contains("TypeApartmentEntity= 1") || paging.query.Contains("TypeBlockEntity =1"))
                {
                    typeApartmentEntity = TypeApartmentEntity.APARTMENT_NORMAL;
                    typeBlockEntity = TypeBlockEntity.BLOCK_NORMAL;
                }


                if (paging != null)
                {
                    def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                    IQueryable<ApartmentData> data;

                    if (decree == null)
                    {
                        if (moduleSystem == (int)ModuleSystem.NOC)
                        {
                            data = (from wm in _context.WardManagements
                                    join b in _context.Blocks on wm.WardId equals b.Ward
                                    join a in _context.Apartments on b.Id equals a.BlockId
                                    where wm.Status != EntityStatus.DELETED
                                        && b.Status != EntityStatus.DELETED
                                        && a.Status != EntityStatus.DELETED
                                        && wm.UserId == userId
                                        && ((b.TypeBlockEntity == typeBlockEntity && typeBlockEntity != null) || (1 == 1 && typeBlockEntity == null))
                                        && ((a.TypeApartmentEntity == typeApartmentEntity && typeApartmentEntity != null) || (1 == 1 && typeApartmentEntity == null))
                                    select new ApartmentData
                                    {
                                        Id = a.Id,
                                        CreatedAt = a.CreatedAt,
                                        CreatedBy = a.CreatedBy,
                                        CreatedById = a.CreatedById,
                                        UpdatedAt = a.UpdatedAt,
                                        UpdatedBy = a.UpdatedBy,
                                        UpdatedById = a.UpdatedById,
                                        TypeReportApply = a.TypeReportApply,
                                        BlockId = a.BlockId,
                                        Code = a.Code,
                                        Address = a.Address,
                                        ConstructionAreaValue = a.ConstructionAreaValue,
                                        ConstructionAreaValue1 = a.ConstructionAreaValue1,
                                        ConstructionAreaValue2 = a.ConstructionAreaValue2,
                                        ConstructionAreaValue3 = a.ConstructionAreaValue3,
                                        UseAreaValue = a.UseAreaValue,
                                        UseAreaValue1 = a.UseAreaValue1,
                                        UseAreaValue2 = a.UseAreaValue2,
                                        LandscapeAreaValue = a.LandscapeAreaValue,
                                        LandscapeAreaValue1 = a.LandscapeAreaValue1,
                                        LandscapeAreaValue2 = a.LandscapeAreaValue2,
                                        LandscapeAreaValue3 = a.LandscapeAreaValue3,
                                        InLimit40Percent = a.InLimit40Percent,
                                        OutLimit100Percent = a.OutLimit100Percent,
                                        TypeApartmentEntity = a.TypeApartmentEntity,
                                        CampusArea = a.CampusArea,
                                        EstablishStateOwnership = a.EstablishStateOwnership,
                                        Dispute = a.Dispute,
                                        Blueprint = a.Blueprint,
                                        CustomerId = a.CustomerId,
                                        UsageStatus = a.UsageStatus,
                                        UsageStatusNote = a.UsageStatusNote,
                                        UseAreaNote1 = a.UseAreaNote1,
                                        ApprovedForConstructionOnTheApartmentYard = a.ApprovedForConstructionOnTheApartmentYard,
                                        ApprovedForConstructionOnTheApartmentYardLandscape = a.ApprovedForConstructionOnTheApartmentYardLandscape,
                                        ConstructionAreaNote = a.ConstructionAreaNote,
                                        UseAreaNote = a.UseAreaNote,
                                        ParentTypeReportApply = a.ParentTypeReportApply,
                                        Floor = a.Floor,
                                        DateRecord = a.DateRecord,
                                        TakeOver = a.TakeOver,
                                        TypeHouse = a.TypeHouse,
                                        DateApply = a.DateApply,
                                        No = a.No,
                                        CodeEstablishStateOwnership = a.CodeEstablishStateOwnership,
                                        DateEstablishStateOwnership = a.DateEstablishStateOwnership,
                                        NameBlueprint = a.NameBlueprint,
                                        DateBlueprint = a.DateBlueprint,
                                        BlockAddress = b.Address,
                                        Lane = b.Lane,
                                        Ward = b.Ward,
                                        District = b.District
                                    }).AsQueryable();
                        }
                        else
                        {
                            //data = _context.Apartments.Where(c => c.Status != AppEnums.EntityStatus.DELETED).AsQueryable();
                            data = (from b in _context.Blocks
                                    join a in _context.Apartments on b.Id equals a.BlockId
                                    where b.Status != EntityStatus.DELETED
                                        && a.Status != EntityStatus.DELETED
                                        && ((b.TypeBlockEntity == typeBlockEntity && typeBlockEntity != null) || (1 == 1 && typeBlockEntity == null))
                                        && ((a.TypeApartmentEntity == typeApartmentEntity && typeApartmentEntity != null) || (1 == 1 && typeApartmentEntity == null))
                                    select new ApartmentData
                                    {
                                        Id = a.Id,
                                        CreatedAt = a.CreatedAt,
                                        CreatedBy = a.CreatedBy,
                                        CreatedById = a.CreatedById,
                                        UpdatedAt = a.UpdatedAt,
                                        UpdatedBy = a.UpdatedBy,
                                        UpdatedById = a.UpdatedById,
                                        TypeReportApply = a.TypeReportApply,
                                        BlockId = a.BlockId,
                                        Code = a.Code,
                                        Address = a.Address,
                                        ConstructionAreaValue = a.ConstructionAreaValue,
                                        ConstructionAreaValue1 = a.ConstructionAreaValue1,
                                        ConstructionAreaValue2 = a.ConstructionAreaValue2,
                                        ConstructionAreaValue3 = a.ConstructionAreaValue3,
                                        UseAreaValue = a.UseAreaValue,
                                        UseAreaValue1 = a.UseAreaValue1,
                                        UseAreaValue2 = a.UseAreaValue2,
                                        LandscapeAreaValue = a.LandscapeAreaValue,
                                        LandscapeAreaValue1 = a.LandscapeAreaValue1,
                                        LandscapeAreaValue2 = a.LandscapeAreaValue2,
                                        LandscapeAreaValue3 = a.LandscapeAreaValue3,
                                        InLimit40Percent = a.InLimit40Percent,
                                        OutLimit100Percent = a.OutLimit100Percent,
                                        TypeApartmentEntity = a.TypeApartmentEntity,
                                        CampusArea = a.CampusArea,
                                        EstablishStateOwnership = a.EstablishStateOwnership,
                                        Dispute = a.Dispute,
                                        Blueprint = a.Blueprint,
                                        CustomerId = a.CustomerId,
                                        UsageStatus = a.UsageStatus,
                                        UsageStatusNote = a.UsageStatusNote,
                                        UseAreaNote1 = a.UseAreaNote1,
                                        ApprovedForConstructionOnTheApartmentYard = a.ApprovedForConstructionOnTheApartmentYard,
                                        ApprovedForConstructionOnTheApartmentYardLandscape = a.ApprovedForConstructionOnTheApartmentYardLandscape,
                                        ConstructionAreaNote = a.ConstructionAreaNote,
                                        UseAreaNote = a.UseAreaNote,
                                        ParentTypeReportApply = a.ParentTypeReportApply,
                                        Floor = a.Floor,
                                        DateRecord = a.DateRecord,
                                        TakeOver = a.TakeOver,
                                        TypeHouse = a.TypeHouse,
                                        DateApply = a.DateApply,
                                        No = a.No,
                                        CodeEstablishStateOwnership = a.CodeEstablishStateOwnership,
                                        DateEstablishStateOwnership = a.DateEstablishStateOwnership,
                                        NameBlueprint = a.NameBlueprint,
                                        DateBlueprint = a.DateBlueprint,
                                        BlockAddress = b.Address,
                                        Lane = b.Lane,
                                        Ward = b.Ward,
                                        District = b.District
                                    }).AsQueryable();
                        }
                    }
                    else
                    {
                        if (moduleSystem == (int)ModuleSystem.NOC)
                        {
                            data = (from dm in _context.DecreeMaps
                                    join b in _context.Blocks on dm.TargetId equals b.Id
                                    join wm in _context.WardManagements on b.Ward equals wm.WardId
                                    join a in _context.Apartments on b.Id equals a.BlockId
                                    where b.Status != EntityStatus.DELETED
                                        && dm.Status != EntityStatus.DELETED
                                        && dm.DecreeType1Id == decree
                                        && a.Status != EntityStatus.DELETED
                                        && wm.Status != EntityStatus.DELETED
                                        && wm.UserId == userId
                                        && ((b.TypeBlockEntity == typeBlockEntity && typeBlockEntity != null) || (1 == 1 && typeBlockEntity == null))
                                        && ((a.TypeApartmentEntity == typeApartmentEntity && typeApartmentEntity != null) || (1 == 1 && typeApartmentEntity == null))
                                    select new ApartmentData
                                    {
                                        Id = a.Id,
                                        CreatedAt = a.CreatedAt,
                                        CreatedBy = a.CreatedBy,
                                        CreatedById = a.CreatedById,
                                        UpdatedAt = a.UpdatedAt,
                                        UpdatedBy = a.UpdatedBy,
                                        UpdatedById = a.UpdatedById,
                                        TypeReportApply = a.TypeReportApply,
                                        BlockId = a.BlockId,
                                        Code = a.Code,
                                        Address = a.Address,
                                        ConstructionAreaValue = a.ConstructionAreaValue,
                                        ConstructionAreaValue1 = a.ConstructionAreaValue1,
                                        ConstructionAreaValue2 = a.ConstructionAreaValue2,
                                        ConstructionAreaValue3 = a.ConstructionAreaValue3,
                                        UseAreaValue = a.UseAreaValue,
                                        UseAreaValue1 = a.UseAreaValue1,
                                        UseAreaValue2 = a.UseAreaValue2,
                                        LandscapeAreaValue = a.LandscapeAreaValue,
                                        LandscapeAreaValue1 = a.LandscapeAreaValue1,
                                        LandscapeAreaValue2 = a.LandscapeAreaValue2,
                                        LandscapeAreaValue3 = a.LandscapeAreaValue3,
                                        InLimit40Percent = a.InLimit40Percent,
                                        OutLimit100Percent = a.OutLimit100Percent,
                                        TypeApartmentEntity = a.TypeApartmentEntity,
                                        CampusArea = a.CampusArea,
                                        EstablishStateOwnership = a.EstablishStateOwnership,
                                        Dispute = a.Dispute,
                                        Blueprint = a.Blueprint,
                                        CustomerId = a.CustomerId,
                                        UsageStatus = a.UsageStatus,
                                        UsageStatusNote = a.UsageStatusNote,
                                        UseAreaNote1 = a.UseAreaNote1,
                                        ApprovedForConstructionOnTheApartmentYard = a.ApprovedForConstructionOnTheApartmentYard,
                                        ApprovedForConstructionOnTheApartmentYardLandscape = a.ApprovedForConstructionOnTheApartmentYardLandscape,
                                        ConstructionAreaNote = a.ConstructionAreaNote,
                                        UseAreaNote = a.UseAreaNote,
                                        ParentTypeReportApply = a.ParentTypeReportApply,
                                        Floor = a.Floor,
                                        DateRecord = a.DateRecord,
                                        TakeOver = a.TakeOver,
                                        TypeHouse = a.TypeHouse,
                                        DateApply = a.DateApply,
                                        No = a.No,
                                        CodeEstablishStateOwnership = a.CodeEstablishStateOwnership,
                                        DateEstablishStateOwnership = a.DateEstablishStateOwnership,
                                        NameBlueprint = a.NameBlueprint,
                                        DateBlueprint = a.DateBlueprint,
                                        BlockAddress = b.Address,
                                        Lane = b.Lane,
                                        Ward = b.Ward,
                                        District = b.District
                                    }).AsQueryable();
                        }
                        else
                        {
                            data = (from dm in _context.DecreeMaps
                                    join b in _context.Blocks on dm.TargetId equals b.Id
                                    join a in _context.Apartments on b.Id equals a.BlockId
                                    where b.Status != EntityStatus.DELETED
                                        && dm.Status != EntityStatus.DELETED
                                        && dm.DecreeType1Id == decree
                                        && a.Status != EntityStatus.DELETED
                                        && ((b.TypeBlockEntity == typeBlockEntity && typeBlockEntity != null) || (1 == 1 && typeBlockEntity == null))
                                        && ((a.TypeApartmentEntity == typeApartmentEntity && typeApartmentEntity != null) || (1 == 1 && typeApartmentEntity == null))
                                    select new ApartmentData
                                    {
                                        Id = a.Id,
                                        CreatedAt = a.CreatedAt,
                                        CreatedBy = a.CreatedBy,
                                        CreatedById = a.CreatedById,
                                        UpdatedAt = a.UpdatedAt,
                                        UpdatedBy = a.UpdatedBy,
                                        UpdatedById = a.UpdatedById,
                                        TypeReportApply = a.TypeReportApply,
                                        BlockId = a.BlockId,
                                        Code = a.Code,
                                        Address = a.Address,
                                        ConstructionAreaValue = a.ConstructionAreaValue,
                                        ConstructionAreaValue1 = a.ConstructionAreaValue1,
                                        ConstructionAreaValue2 = a.ConstructionAreaValue2,
                                        ConstructionAreaValue3 = a.ConstructionAreaValue3,
                                        UseAreaValue = a.UseAreaValue,
                                        UseAreaValue1 = a.UseAreaValue1,
                                        UseAreaValue2 = a.UseAreaValue2,
                                        LandscapeAreaValue = a.LandscapeAreaValue,
                                        LandscapeAreaValue1 = a.LandscapeAreaValue1,
                                        LandscapeAreaValue2 = a.LandscapeAreaValue2,
                                        LandscapeAreaValue3 = a.LandscapeAreaValue3,
                                        InLimit40Percent = a.InLimit40Percent,
                                        OutLimit100Percent = a.OutLimit100Percent,
                                        TypeApartmentEntity = a.TypeApartmentEntity,
                                        CampusArea = a.CampusArea,
                                        EstablishStateOwnership = a.EstablishStateOwnership,
                                        Dispute = a.Dispute,
                                        Blueprint = a.Blueprint,
                                        CustomerId = a.CustomerId,
                                        UsageStatus = a.UsageStatus,
                                        UsageStatusNote = a.UsageStatusNote,
                                        UseAreaNote1 = a.UseAreaNote1,
                                        ApprovedForConstructionOnTheApartmentYard = a.ApprovedForConstructionOnTheApartmentYard,
                                        ApprovedForConstructionOnTheApartmentYardLandscape = a.ApprovedForConstructionOnTheApartmentYardLandscape,
                                        ConstructionAreaNote = a.ConstructionAreaNote,
                                        UseAreaNote = a.UseAreaNote,
                                        ParentTypeReportApply = a.ParentTypeReportApply,
                                        Floor = a.Floor,
                                        DateRecord = a.DateRecord,
                                        TakeOver = a.TakeOver,
                                        TypeHouse = a.TypeHouse,
                                        DateApply = a.DateApply,
                                        No = a.No,
                                        CodeEstablishStateOwnership = a.CodeEstablishStateOwnership,
                                        DateEstablishStateOwnership = a.DateEstablishStateOwnership,
                                        NameBlueprint = a.NameBlueprint,
                                        DateBlueprint = a.DateBlueprint,
                                        BlockAddress = b.Address,
                                        Lane = b.Lane,
                                        Ward = b.Ward,
                                        District = b.District
                                    }).AsQueryable();
                        }
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
                        //List<ApartmentData> res = _mapper.Map<List<ApartmentData>>(data.ToList());
                        List<ApartmentData> res = data.ToList();
                        foreach (ApartmentData item in res)
                        {
                            Block block = _context.Blocks.Where(b => b.Id == item.BlockId).FirstOrDefault();
                            if (block != null)
                            {
                                item.BlockName = block.Address;

                                //Lấy thông tin diện tích đất liền kề đã bán
                                if (block.TypeReportApply == TypeReportApply.NHA_HO_CHUNG && block.TypeBlockEntity == TypeBlockEntity.BLOCK_NORMAL)
                                {
                                    Pricing pricing = _context.Pricings.Where(e => e.BlockId == block.Id && e.ApartmentId == item.Id && e.TypeReportApply == TypeReportApply.BAN_PHAN_DIEN_TICH_LIEN_KE && e.Status != EntityStatus.DELETED).OrderByDescending(x => x.DateCreate).FirstOrDefault();

                                    if (pricing != null)
                                    {
                                        item.SellLandArea = pricing.SellLandArea;
                                        item.pricingApartmentLandDetails = _context.PricingApartmentLandDetails.Where(a => a.PricingId == pricing.Id && a.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                                    }
                                }
                            }

                            item.apartmentDetails = _context.ApartmentDetails.Where(a => a.TargetId == item.Id && a.Type == TypeApartmentDetail.APARTMENT && a.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                            item.apartmentLandDetails = _context.ApartmentLandDetails.Where(a => a.TargetId == item.Id && a.Type == TypeApartmentLandDetail.APARTMENT && a.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                            item.blockMaintextureRaties = _context.BlockMaintextureRaties.Where(b => b.TargetId == item.Id && b.Type == AppEnums.TypeBlockMaintextureRate.APARTMENT && b.Status != AppEnums.EntityStatus.DELETED).ToList();

                            if (item.TypeApartmentEntity == TypeApartmentEntity.APARTMENT_RENT && item.CustomerId != null)
                            {
                                item.CustomerName = _context.Customers.Find(item.CustomerId)?.FullName;
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
            catch (Exception ex)
            {
                log.Error("GetByPage Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        // GET: api/Apartment/1
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
                Apartment data = await _context.Apartments.FindAsync(id);

                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                    return Ok(def);
                }

                ApartmentData res = _mapper.Map<ApartmentData>(data);
                Block block = _context.Blocks.Where(b => b.Id == res.BlockId).FirstOrDefault();
                if (block != null)
                {
                    res.BlockName = block.Address;
                }

                res.blockMaintextureRaties = _context.BlockMaintextureRaties.Where(b => b.TargetId == res.Id && b.Type == AppEnums.TypeBlockMaintextureRate.APARTMENT && b.Status != AppEnums.EntityStatus.DELETED).ToList();
                res.apartmentLandDetails = _context.ApartmentLandDetails.Where(a => a.TargetId == res.Id && a.Type == TypeApartmentLandDetail.APARTMENT && a.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                List<ApartmentDetail> apartmentDetails = _context.ApartmentDetails.Where(a => a.TargetId == res.Id && a.Type == TypeApartmentDetail.APARTMENT && a.Status != AppEnums.EntityStatus.DELETED).ToList();
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
            catch (Exception ex)
            {
                log.Error("GetById Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        // POST: api/Apartment
        [HttpPost]
        public async Task<IActionResult> Post(ApartmentData input)
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
                input = (ApartmentData)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                if (input.Code == null || input.Code == "")
                {
                    def.meta = new Meta(400, "Mã không được để trống!");
                    return Ok(def);
                }

                //Kiểm tra mã định danh tồn tại
                Block blockCodeExist = _context.Blocks.Where(f => f.Code == input.Code && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                if (blockCodeExist != null)
                {
                    def.meta = new Meta(211, "Đã tồn tại căn nhà có mã định danh " + input.Code + "!");
                    return Ok(def);
                }

                Apartment apartmentCodeExist = _context.Apartments.Where(c => c.TypeApartmentEntity == input.TypeApartmentEntity && c.Code == input.Code && c.Status != EntityStatus.DELETED).FirstOrDefault();
                if (apartmentCodeExist != null)
                {
                    def.meta = new Meta(211, "Đã tồn tại căn hộ có mã định danh " + input.Code + "!");
                    return Ok(def);
                }

                //if (input.Name == null || input.Name == "")
                //{
                //    def.meta = new Meta(400, "Tên không được để trống!");
                //    return Ok(def);
                //}

                Block block = _context.Blocks.Where(f => f.Id == input.BlockId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                if (block == null)
                {
                    def.meta = new Meta(404, "Không tìm thấy căn nhà!");
                    return Ok(def);
                }

                Apartment codeExist = _context.Apartments.Where(f => f.Code == input.Code && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                if (codeExist != null)
                {
                    def.meta = new Meta(211, "Mã đã tồn tại!");
                    return Ok(def);
                }


                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.TypeReportApply = block.TypeReportApply;
                    input.CreatedById = userId;
                    input.CreatedBy = fullName;
                    _context.Apartments.Add(input);

                    try
                    {
                        await _context.SaveChangesAsync();

                        //ApartmentDetail
                        if (input.apartmentDetails != null)
                        {
                            foreach (var apartmentDetail in input.apartmentDetails)
                            {
                                apartmentDetail.TargetId = input.Id;
                                apartmentDetail.CreatedBy = fullName;
                                apartmentDetail.CreatedById = userId;
                                apartmentDetail.Type = TypeApartmentDetail.APARTMENT;

                                _context.ApartmentDetails.Add(apartmentDetail);

                            }
                        }

                        if (input.blockMaintextureRaties != null)
                        {
                            foreach (var blockMaintextureRate in input.blockMaintextureRaties)
                            {
                                blockMaintextureRate.Type = AppEnums.TypeBlockMaintextureRate.APARTMENT;
                                blockMaintextureRate.TargetId = input.Id;
                                blockMaintextureRate.CreatedBy = fullName;
                                blockMaintextureRate.CreatedById = userId;

                                _context.BlockMaintextureRaties.Add(blockMaintextureRate);
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
                                apartmentLandDetail.Type = TypeApartmentLandDetail.APARTMENT;

                                _context.ApartmentLandDetails.Add(apartmentLandDetail);
                            }
                        }

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Thêm mới căn hộ " + input.Code, "Apartment", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
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
                        if (ApartmentExists(input.Id))
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

        // PUT: api/Apartment/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ApartmentData input)
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
                input = (ApartmentData)UtilsService.TrimStringPropertyTypeObject(input);

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

                if (input.Code == null || input.Code == "")
                {
                    def.meta = new Meta(400, "Mã không được để trống!");
                    return Ok(def);
                }

                //Kiểm tra mã định danh tồn tại
                Block blockCodeExist = _context.Blocks.Where(f => f.Code == input.Code && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                if (blockCodeExist != null)
                {
                    def.meta = new Meta(211, "Đã tồn tại căn nhà có mã định danh " + input.Code + "!");
                    return Ok(def);
                }

                Apartment apartmentCodeExist = _context.Apartments.Where(c => c.Id != input.Id && c.TypeApartmentEntity == input.TypeApartmentEntity && c.Code == input.Code && c.Status != EntityStatus.DELETED).FirstOrDefault();
                if (apartmentCodeExist != null)
                {
                    def.meta = new Meta(211, "Đã tồn tại căn hộ có mã định danh " + input.Code + "!");
                    return Ok(def);
                }

                //if (input.Name == null || input.Name == "")
                //{
                //    def.meta = new Meta(400, "Tên không được để trống!");
                //    return Ok(def);
                //}

                Block block = _context.Blocks.Where(f => f.Id == input.BlockId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                if (block == null)
                {
                    def.meta = new Meta(404, "Không tìm thấy căn nhà!");
                    return Ok(def);
                }

                Apartment data = await _context.Apartments.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_UPDATE_MESSAGE);
                    return Ok(def);
                }

                Apartment codeExist = _context.Apartments.Where(f => f.Code == input.Code && f.TypeApartmentEntity == input.TypeApartmentEntity && f.Status != AppEnums.EntityStatus.DELETED && f.Id != id).FirstOrDefault();
                if (codeExist != null)
                {
                    def.meta = new Meta(211, "Mã đã tồn tại!");
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.TypeReportApply = block.TypeReportApply;
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

                        //ApartmentDetail
                        List<ApartmentDetail> apartmentDetails = _context.ApartmentDetails.Where(l => l.TargetId == input.Id && l.Type == TypeApartmentDetail.APARTMENT && l.Status != AppEnums.EntityStatus.DELETED).AsNoTracking().ToList();
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
                                    apartmentDetail.Type = TypeApartmentDetail.APARTMENT;

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
                                    apartmentDetail.Type = TypeApartmentDetail.APARTMENT;

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

                        //BlockMaintextureRate
                        List<BlockMaintextureRate> blockMaintextureRaties = _context.BlockMaintextureRaties.Where(l => l.TargetId == input.Id && l.Type == AppEnums.TypeBlockMaintextureRate.APARTMENT && l.Status != AppEnums.EntityStatus.DELETED).AsNoTracking().ToList();
                        if (input.blockMaintextureRaties != null)
                        {
                            foreach (var blockMaintextureRate in input.blockMaintextureRaties)
                            {
                                BlockMaintextureRate blockMaintextureRateExist = blockMaintextureRaties.Where(l => l.Id == blockMaintextureRate.Id).FirstOrDefault();
                                if (blockMaintextureRateExist == null)
                                {
                                    blockMaintextureRate.Type = AppEnums.TypeBlockMaintextureRate.APARTMENT;
                                    blockMaintextureRate.TargetId = input.Id;
                                    blockMaintextureRate.CreatedBy = fullName;
                                    blockMaintextureRate.CreatedById = userId;

                                    _context.BlockMaintextureRaties.Add(blockMaintextureRate);
                                }
                                else
                                {
                                    blockMaintextureRate.Type = AppEnums.TypeBlockMaintextureRate.APARTMENT;
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

                        //ApartmentLandDetail
                        List<ApartmentLandDetail> apartmentLandDetails = _context.ApartmentLandDetails.Where(l => l.TargetId == input.Id && l.Type == TypeApartmentLandDetail.APARTMENT && l.Status != AppEnums.EntityStatus.DELETED).AsNoTracking().ToList();
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
                                    apartmentLandDetail.Type = TypeApartmentLandDetail.APARTMENT;

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
                                    apartmentLandDetail.Type = TypeApartmentLandDetail.APARTMENT;

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
                            input.editHistory.TypeEditHistory = TypeEditHistory.APARTMENT;

                            _context.EditHistories.Add(input.editHistory);
                        }

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Sửa căn hộ " + input.Code, "Apartment", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (input.Id > 0)
                        {
                            transaction.Commit();
                            ChangePrice req = new ChangePrice();
                            req.Id = input.Id;
                            req.TypeReportApply = input.TypeReportApply;
                            Task.Run(() => _changePrice.ChangePrice(req, _serviceScopeFactory));

                        }

                        else
                        {
                            transaction.Rollback();
                        }

                        def.meta = new Meta(200, ApiConstants.MessageResource.UPDATE_SUCCESS);
                        def.data = data;
                        return Ok(def);
                    }
                    catch (DbUpdateException e)
                    {
                        transaction.Rollback();
                        log.Error("DbUpdateException:" + e);
                        if (!ApartmentExists(data.Id))
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

        // DELETE: api/Apartment/1
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
                Apartment data = await _context.Apartments.FindAsync(id);
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_DELETE_MESSAGE);
                    return Ok(def);
                }

                //Kiểm tra dữ liệu liên quan có tồn tại thì không thể xóa
                //Biên bản
                Pricing pricing = _context.Pricings.Where(c => c.ApartmentId == id && c.Status != EntityStatus.DELETED).FirstOrDefault();
                if (pricing != null)
                {
                    def.meta = new Meta(222, "Dữ liệu biên bản tính giá liên quan còn tồn tại. Không thể xóa căn nhà này!");
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    data.UpdatedAt = DateTime.Now;
                    data.UpdatedById = userId;
                    data.UpdatedBy = fullName;
                    data.Status = AppEnums.EntityStatus.DELETED;
                    _context.Update(data);

                    //Xóa ApartmentDetails
                    List<ApartmentDetail> apartmentDetails = _context.ApartmentDetails.Where(a => a.TargetId == id && a.Type == TypeApartmentDetail.APARTMENT && a.Status != EntityStatus.DELETED).ToList();
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

                    //Xóa ApartmentLandDetail
                    List<ApartmentLandDetail> apartmentLandDetails = _context.ApartmentLandDetails.Where(a => a.TargetId == id && a.Type == TypeApartmentLandDetail.APARTMENT && a.Status != EntityStatus.DELETED).ToList();
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

                    //Xóa BlockMaintextureRate
                    List<BlockMaintextureRate> blockMaintextureRaties = _context.BlockMaintextureRaties.Where(l => l.TargetId == data.Id && l.Type == AppEnums.TypeBlockMaintextureRate.APARTMENT && l.Status != AppEnums.EntityStatus.DELETED).ToList();
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

                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Xóa căn hộ " + data.Code, "Apartment", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.DELETED, userId, fullName);
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
                        if (!ApartmentExists(data.Id))
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

                Apartment apartment_normal = _context.Apartments.Where(a => a.Code == code && a.TypeApartmentEntity == TypeApartmentEntity.APARTMENT_NORMAL && a.Status != EntityStatus.DELETED).FirstOrDefault();
                if (apartment_normal != null)
                {
                    output.CodeStatus = CodeStatus.DA_CAP_NHAT;
                    output.Data = apartment_normal;
                }
                else
                {
                    Apartment apartment_rent = _context.Apartments.Where(a => a.Code == code && a.TypeApartmentEntity == TypeApartmentEntity.APARTMENT_RENT && a.Status != EntityStatus.DELETED).FirstOrDefault();
                    if (apartment_rent != null)
                    {
                        Block block = _context.Blocks.Where(b => b.Id == apartment_rent.BlockId && b.Status != EntityStatus.DELETED).FirstOrDefault();
                        if (block != null)
                        {
                            output.CodeStatus = CodeStatus.DA_TON_TAI;

                            BlockData res = _mapper.Map<BlockData>(block);

                            res.FullAddress = res.Address;

                            Ward ward = _context.Wards.Where(p => p.Id == res.Ward).FirstOrDefault();
                            res.FullAddress = res.FullAddress != null && res.FullAddress != "" ? (ward != null ? String.Join(", ", res.FullAddress, ward.Name) : res.FullAddress) : (ward != null ? ward.Name : res.FullAddress);

                            District district = _context.Districts.Where(p => p.Id == res.District).FirstOrDefault();
                            res.FullAddress = res.FullAddress != null && res.FullAddress != "" ? (district != null ? String.Join(", ", res.FullAddress, district.Name) : res.FullAddress) : (district != null ? district.Name : res.FullAddress);

                            Province province = _context.Provincies.Where(p => p.Id == res.Province).FirstOrDefault();
                            res.FullAddress = res.FullAddress != null && res.FullAddress != "" ? (province != null ? String.Join(", ", res.FullAddress, province.Name) : res.FullAddress) : (province != null ? province.Name : res.FullAddress);

                            output.Data = res;
                            output.DataExtra = apartment_rent;
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

        private bool ApartmentExists(int id)
        {
            return _context.Apartments.Count(e => e.Id == id) > 0;
        }
    }
}
