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
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PricingController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("Pricing", "Pricing");
        private static string functionCode = "PRICING";
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;

        public PricingController(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Pricing/GetByPage?query=...&order_by=...&page=1&page_size=20&select=...
        [HttpGet("GetByPage")]
        public IActionResult GetByPage([FromQuery] FilteredPagination paging)
        {
            // Xác thực AccessToken
            string accessToken = Request.Headers[HeaderNames.Authorization];
            var token = _context.Tokens.FirstOrDefault(t => Convert.ToString(t.AccessToken) == accessToken);
            if (token == null) return Unauthorized();

            var def = new DefaultResponse();

            // Quyền
            var identity = (ClaimsIdentity)User.Identity;
            string access_key = identity.Claims.FirstOrDefault(c => c.Type == "AccessKey")?.Value;
            int userId = int.Parse(identity.Claims.FirstOrDefault(c => c.Type == "Id")?.Value ?? "0");
            int moduleSystem = int.Parse(identity.Claims.FirstOrDefault(c => c.Type == "ModuleSystem")?.Value ?? "0");

            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }

            try
            {
                if (paging == null)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);

                IQueryable<Pricing> data;
                if (moduleSystem == (int)ModuleSystem.NOC)
                {
                    data =
                        from b in _context.Blocks
                        join p in _context.Pricings on b.Id equals p.BlockId
                        join wm in _context.WardManagements on b.Ward equals wm.WardId
                        where b.Status != EntityStatus.DELETED
                              && p.Status != EntityStatus.DELETED
                              && wm.Status != EntityStatus.DELETED
                              && wm.UserId == userId
                        select p;
                }
                else
                {
                    data = _context.Pricings.Where(p => p.Status != EntityStatus.DELETED);
                }

                if (!string.IsNullOrWhiteSpace(paging.query))
                {
                    paging.query = HttpUtility.UrlDecode(paging.query);
                    data = data.Where(paging.query);
                }

                def.metadata = data.Count();

                // Sắp xếp + phân trang
                if (paging.page_size > 0)
                {
                    data = !string.IsNullOrWhiteSpace(paging.order_by)
                        ? data.OrderBy(paging.order_by)
                        : data.OrderBy("Id desc");

                    data = data.Skip((paging.page - 1) * paging.page_size)
                               .Take(paging.page_size);
                }
                else
                {
                    data = !string.IsNullOrWhiteSpace(paging.order_by)
                        ? data.OrderBy(paging.order_by)
                        : data.OrderBy("Id desc");
                }

                if (!string.IsNullOrWhiteSpace(paging.select))
                {
                    paging.select = "new(" + paging.select + ")";
                    paging.select = HttpUtility.UrlDecode(paging.select);
                    def.data = data.Select(paging.select);
                }
                else
                {
                    // Map sang PricingData + load các bảng con
                    var list = _mapper.Map<List<PricingData>>(data.ToList());
                    foreach (var item in list)
                    {
                        item.block = _context.Blocks.FirstOrDefault(b => b.Id == item.BlockId && b.Status != EntityStatus.DELETED);
                        item.apartment = _context.Apartments.FirstOrDefault(a => a.Id == item.ApartmentId && a.Status != EntityStatus.DELETED);
                        if (item.block != null)
                        {
                            item.decree = _context.Decreies.FirstOrDefault(d => d.Id == item.block.DecreeType1Id && d.Status != EntityStatus.DELETED);
                        }

                        item.constructionPricies = _context.PricingConstructionPricies
                            .Where(x => x.PricingId == item.Id && x.Status != EntityStatus.DELETED).ToList();

                        item.customers = _context.PricingCustomers
                            .Where(x => x.PricingId == item.Id && x.Status != EntityStatus.DELETED).ToList();

                        item.landPricingTbl = _context.PricingLandTbls
                            .Where(x => x.PricingId == item.Id && x.Status != EntityStatus.DELETED).ToList();

                        item.pricingOfficers = _context.PricingOfficers
                            .Where(x => x.PricingId == item.Id && x.Status != EntityStatus.DELETED).ToList();

                        item.reducedPerson = _context.PricingReducedPeople
                            .Where(x => x.PricingId == item.Id && x.Status != EntityStatus.DELETED).ToList();

                        item.pricingApartmentLandDetails = _context.PricingApartmentLandDetails
                            .Where(x => x.PricingId == item.Id && x.Status != EntityStatus.DELETED)
                            .OrderBy(x => x.UpdatedAt).ToList();

                        item.pricingReplaceds = _context.PricingReplaceds
                            .Where(x => x.PricingId == item.Id && x.Status != EntityStatus.DELETED).ToList();
                    }

                    def.data = list;
                }

                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("GetByPage Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        // GET: api/Pricing/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            var token = _context.Tokens.FirstOrDefault(t => Convert.ToString(t.AccessToken) == accessToken);
            if (token == null) return Unauthorized();

            var def = new DefaultResponse();

            var identity = (ClaimsIdentity)User.Identity;
            string access_key = identity.Claims.FirstOrDefault(c => c.Type == "AccessKey")?.Value;
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }

            try
            {
                var data = await _context.Pricings.FindAsync(id);
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
                var defErr = new DefaultResponse { meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE) };
                return Ok(defErr);
            }
        }

        // POST: api/Pricing
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PricingData input)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            var token = _context.Tokens.FirstOrDefault(t => Convert.ToString(t.AccessToken) == accessToken);
            if (token == null) return Unauthorized();

            var def = new DefaultResponse();

            var identity = (ClaimsIdentity)User.Identity;
            int userId = int.Parse(identity.Claims.FirstOrDefault(c => c.Type == "Id")?.Value ?? "0");
            string fullName = identity.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? string.Empty;
            string access_key = identity.Claims.FirstOrDefault(c => c.Type == "AccessKey")?.Value;

            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.CREATE))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_CREATE_MESSAGE);
                return Ok(def);
            }

            try
            {
                input = (PricingData)UtilsService.TrimStringPropertyTypeObject(input);
                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                var block = _context.Blocks.FirstOrDefault(b => b.Id == input.BlockId && b.Status != EntityStatus.DELETED);
                if (block == null)
                {
                    def.meta = new Meta(400, "Không có thông tin Căn nhà!");
                    return Ok(def);
                }

                Apartment apartment = null;
                if (block.TypeReportApply != TypeReportApply.NHA_RIENG_LE)
                {
                    apartment = _context.Apartments.FirstOrDefault(a => a.Id == input.ApartmentId && a.Status != EntityStatus.DELETED);
                    if (apartment == null)
                    {
                        def.meta = new Meta(400, "Không có thông tin Căn hộ!");
                        return Ok(def);
                    }
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.CreatedById = userId;
                    input.CreatedBy = fullName;
                    _context.Pricings.Add(input);

                    try
                    {
                        await _context.SaveChangesAsync();

                        // Children — thêm mới
                        if (input.constructionPricies != null)
                        {
                            foreach (var it in input.constructionPricies)
                            {
                                it.Id = 0;
                                it.PricingId = input.Id;
                                it.CreatedBy = fullName;
                                it.CreatedById = userId;
                                _context.PricingConstructionPricies.Add(it);
                            }
                        }

                        if (input.customers != null)
                        {
                            foreach (var it in input.customers)
                            {
                                it.Id = 0;
                                it.PricingId = input.Id;
                                it.CreatedBy = fullName;
                                it.CreatedById = userId;
                                _context.PricingCustomers.Add(it);
                            }
                        }

                        if (input.landPricingTbl != null)
                        {
                            foreach (var it in input.landPricingTbl)
                            {
                                it.Id = 0;
                                it.PricingId = input.Id;
                                it.CreatedBy = fullName;
                                it.CreatedById = userId;
                                _context.PricingLandTbls.Add(it);
                            }
                        }

                        if (input.pricingOfficers != null)
                        {
                            foreach (var it in input.pricingOfficers)
                            {
                                it.Id = 0;
                                it.PricingId = input.Id;
                                it.CreatedBy = fullName;
                                it.CreatedById = userId;
                                _context.PricingOfficers.Add(it);
                            }
                        }

                        if (input.reducedPerson != null)
                        {
                            foreach (var it in input.reducedPerson)
                            {
                                it.Id = 0;
                                it.PricingId = input.Id;
                                it.CreatedBy = fullName;
                                it.CreatedById = userId;
                                _context.PricingReducedPeople.Add(it);
                            }
                        }

                        if (input.pricingApartmentLandDetails != null)
                        {
                            foreach (var it in input.pricingApartmentLandDetails)
                            {
                                it.PricingId = input.Id;
                                it.CreatedBy = fullName;
                                it.CreatedById = userId;
                                _context.PricingApartmentLandDetails.Add(it);
                            }
                        }

                        if (input.pricingReplaceds != null)
                        {
                            foreach (var it in input.pricingReplaceds)
                            {
                                it.PricingId = input.Id;
                                it.CreatedBy = fullName;
                                it.CreatedById = userId;
                                _context.PricingReplaceds.Add(it);
                            }
                        }

                        // Log
                        var logActionModel = new LogActionModel(
                            "Thêm mới biên bản tính giá cho căn hộ " + (apartment != null ? apartment.Code : block.Code),
                            "Pricing",
                            input.Id,
                            HttpContext.Connection.RemoteIpAddress?.ToString(),
                            JsonConvert.SerializeObject(input),
                            (int)AppEnums.Action.CREATE,
                            userId,
                            fullName
                        );
                        var logAction = _mapper.Map<LogAction>(logActionModel);
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

                        if (PricingExists(input.Id))
                            def.meta = new Meta(212, "Đã tồn tại Id trên hệ thống!");
                        else
                            def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);

                        return Ok(def);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Post Exception:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        // PUT: api/Pricing/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] PricingData input)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            var token = _context.Tokens.FirstOrDefault(t => Convert.ToString(t.AccessToken) == accessToken);
            if (token == null) return Unauthorized();

            var def = new DefaultResponse();

            var identity = (ClaimsIdentity)User.Identity;
            int userId = int.Parse(identity.Claims.FirstOrDefault(c => c.Type == "Id")?.Value ?? "0");
            string fullName = identity.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? string.Empty;
            string access_key = identity.Claims.FirstOrDefault(c => c.Type == "AccessKey")?.Value;

            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.UPDATE))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_UPDATE_MESSAGE);
                return Ok(def);
            }

            try
            {
                input = (PricingData)UtilsService.TrimStringPropertyTypeObject(input);
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

                var block = _context.Blocks.FirstOrDefault(b => b.Id == input.BlockId && b.Status != EntityStatus.DELETED);
                if (block == null)
                {
                    def.meta = new Meta(400, "Không có thông tin Căn nhà!");
                    return Ok(def);
                }

                Apartment apartment = null;
                if (block.TypeReportApply != TypeReportApply.NHA_RIENG_LE)
                {
                    apartment = _context.Apartments.FirstOrDefault(a => a.Id == input.ApartmentId && a.Status != EntityStatus.DELETED);
                    if (apartment == null)
                    {
                        def.meta = new Meta(400, "Không có thông tin Căn hộ!");
                        return Ok(def);
                    }
                }

                var data = await _context.Pricings
                    .AsNoTracking()
                    .Where(e => e.Id == id && e.Status != EntityStatus.DELETED)
                    .FirstOrDefaultAsync();

                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_UPDATE_MESSAGE);
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    // Giữ nguyên Created*, Status; cập nhật Updated*
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

                        // ========== Đồng bộ bảng con (Add/Update/Soft-Delete) ==========
                        // ConstructionPricies
                        var oldConstruction = _context.PricingConstructionPricies
                            .Where(l => l.PricingId == input.Id && l.Status != EntityStatus.DELETED)
                            .AsNoTracking().ToList();

                        if (input.constructionPricies != null)
                        {
                            foreach (var it in input.constructionPricies)
                            {
                                var exist = oldConstruction.FirstOrDefault(x => x.Id == it.Id);
                                if (exist == null)
                                {
                                    it.PricingId = input.Id;
                                    it.CreatedBy = fullName;
                                    it.CreatedById = userId;
                                    _context.PricingConstructionPricies.Add(it);
                                }
                                else
                                {
                                    it.PricingId = input.Id;
                                    it.CreatedAt = exist.CreatedAt;
                                    it.CreatedBy = exist.CreatedBy;
                                    it.CreatedById = exist.CreatedById;
                                    it.UpdatedBy = fullName;
                                    it.UpdatedById = userId;
                                    _context.Update(it);
                                    oldConstruction.Remove(exist);
                                }
                            }
                        }
                        if (oldConstruction.Count > 0)
                        {
                            oldConstruction.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = EntityStatus.DELETED;
                            });
                            _context.UpdateRange(oldConstruction);
                        }

                        // Customers
                        var oldCustomers = _context.PricingCustomers
                            .Where(l => l.PricingId == input.Id && l.Status != EntityStatus.DELETED)
                            .AsNoTracking().ToList();

                        if (input.customers != null)
                        {
                            foreach (var it in input.customers)
                            {
                                var exist = oldCustomers.FirstOrDefault(x => x.Id == it.Id);
                                if (exist == null)
                                {
                                    it.PricingId = input.Id;
                                    it.CreatedBy = fullName;
                                    it.CreatedById = userId;
                                    _context.PricingCustomers.Add(it);
                                }
                                else
                                {
                                    it.CreatedAt = exist.CreatedAt;
                                    it.CreatedBy = exist.CreatedBy;
                                    it.CreatedById = exist.CreatedById;
                                    it.PricingId = input.Id;
                                    it.UpdatedBy = fullName;
                                    it.UpdatedById = userId;
                                    _context.Update(it);
                                    oldCustomers.Remove(exist);
                                }
                            }
                        }
                        if (oldCustomers.Count > 0)
                        {
                            oldCustomers.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = EntityStatus.DELETED;
                            });
                            _context.UpdateRange(oldCustomers);
                        }

                        // LandPricingTbl
                        var oldLand = _context.PricingLandTbls
                            .Where(l => l.PricingId == input.Id && l.Status != EntityStatus.DELETED)
                            .AsNoTracking().ToList();

                        if (input.landPricingTbl != null)
                        {
                            foreach (var it in input.landPricingTbl)
                            {
                                var exist = oldLand.FirstOrDefault(x => x.Id == it.Id);
                                if (exist == null)
                                {
                                    it.PricingId = input.Id;
                                    it.CreatedBy = fullName;
                                    it.CreatedById = userId;
                                    _context.PricingLandTbls.Add(it);
                                }
                                else
                                {
                                    it.CreatedAt = exist.CreatedAt;
                                    it.CreatedBy = exist.CreatedBy;
                                    it.CreatedById = exist.CreatedById;
                                    it.PricingId = input.Id;
                                    it.UpdatedBy = fullName;
                                    it.UpdatedById = userId;
                                    _context.Update(it);
                                    oldLand.Remove(exist);
                                }
                            }
                        }
                        if (oldLand.Count > 0)
                        {
                            oldLand.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = EntityStatus.DELETED;
                            });
                            _context.UpdateRange(oldLand);
                        }

                        // PricingOfficers
                        var oldOfficers = _context.PricingOfficers
                            .Where(l => l.PricingId == input.Id && l.Status != EntityStatus.DELETED)
                            .AsNoTracking().ToList();

                        if (input.pricingOfficers != null)
                        {
                            foreach (var it in input.pricingOfficers)
                            {
                                var exist = oldOfficers.FirstOrDefault(x => x.Id == it.Id);
                                if (exist == null)
                                {
                                    it.PricingId = input.Id;
                                    it.CreatedBy = fullName;
                                    it.CreatedById = userId;
                                    _context.PricingOfficers.Add(it);
                                }
                                else
                                {
                                    it.CreatedAt = exist.CreatedAt;
                                    it.CreatedBy = exist.CreatedBy;
                                    it.CreatedById = exist.CreatedById;
                                    it.PricingId = input.Id;
                                    it.UpdatedBy = fullName;
                                    it.UpdatedById = userId;
                                    _context.Update(it);
                                    oldOfficers.Remove(exist);
                                }
                            }
                        }
                        if (oldOfficers.Count > 0)
                        {
                            oldOfficers.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = EntityStatus.DELETED;
                            });
                            _context.UpdateRange(oldOfficers);
                        }

                        // ReducedPerson
                        var oldReduced = _context.PricingReducedPeople
                            .Where(l => l.PricingId == input.Id && l.Status != EntityStatus.DELETED)
                            .AsNoTracking().ToList();

                        if (input.reducedPerson != null)
                        {
                            foreach (var it in input.reducedPerson)
                            {
                                var exist = oldReduced.FirstOrDefault(x => x.Id == it.Id);
                                if (exist == null)
                                {
                                    it.PricingId = input.Id;
                                    it.CreatedBy = fullName;
                                    it.CreatedById = userId;
                                    _context.PricingReducedPeople.Add(it);
                                }
                                else
                                {
                                    it.CreatedAt = exist.CreatedAt;
                                    it.CreatedBy = exist.CreatedBy;
                                    it.CreatedById = exist.CreatedById;
                                    it.PricingId = input.Id;
                                    it.UpdatedBy = fullName;
                                    it.UpdatedById = userId;
                                    _context.Update(it);
                                    oldReduced.Remove(exist);
                                }
                            }
                        }
                        if (oldReduced.Count > 0)
                        {
                            oldReduced.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = EntityStatus.DELETED;
                            });
                            _context.UpdateRange(oldReduced);
                        }

                        // PricingApartmentLandDetails
                        var oldAptLand = _context.PricingApartmentLandDetails
                            .Where(l => l.PricingId == input.Id && l.Status != EntityStatus.DELETED)
                            .AsNoTracking().ToList();

                        if (input.pricingApartmentLandDetails != null)
                        {
                            foreach (var it in input.pricingApartmentLandDetails)
                            {
                                var exist = oldAptLand.FirstOrDefault(x => x.Id == it.Id);
                                if (exist == null)
                                {
                                    it.PricingId = input.Id;            // chú ý: PricingId, không phải ApartmentId
                                    it.CreatedBy = fullName;
                                    it.CreatedById = userId;
                                    it.UpdatedAt = DateTime.Now;
                                    _context.PricingApartmentLandDetails.Add(it);
                                }
                                else
                                {
                                    it.PricingId = input.Id;
                                    it.CreatedBy = exist.CreatedBy;
                                    it.CreatedById = exist.CreatedById;
                                    it.UpdatedBy = fullName;
                                    it.UpdatedById = userId;
                                    it.UpdatedAt = DateTime.Now;
                                    _context.Update(it);
                                    oldAptLand.Remove(exist);
                                }
                            }
                        }
                        if (oldAptLand.Count > 0)
                        {
                            oldAptLand.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = EntityStatus.DELETED;
                            });
                            _context.UpdateRange(oldAptLand);
                        }

                        // PricingReplaceds
                        var oldReplaceds = _context.PricingReplaceds
                            .Where(l => l.PricingId == input.Id && l.Status != EntityStatus.DELETED)
                            .AsNoTracking().ToList();

                        if (input.pricingReplaceds != null)
                        {
                            foreach (var it in input.pricingReplaceds)
                            {
                                // Repo gốc match theo PricingReplacedId (khóa phụ khác Id)
                                var exist = oldReplaceds.FirstOrDefault(x => x.PricingReplacedId == it.PricingReplacedId);
                                if (exist == null)
                                {
                                    it.PricingId = input.Id;
                                    it.CreatedBy = fullName;
                                    it.CreatedById = userId;
                                    _context.PricingReplaceds.Add(it);
                                }
                                else
                                {
                                    it.CreatedAt = exist.CreatedAt;
                                    it.CreatedBy = exist.CreatedBy;
                                    it.CreatedById = exist.CreatedById;
                                    it.PricingId = input.Id;
                                    it.UpdatedBy = fullName;
                                    it.UpdatedById = userId;
                                    _context.Update(it);
                                    oldReplaceds.Remove(exist);
                                }
                            }
                        }
                        if (oldReplaceds.Count > 0)
                        {
                            oldReplaceds.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = EntityStatus.DELETED;
                            });
                            _context.UpdateRange(oldReplaceds);
                        }

                        // ================================================================

                        // Log update
                        var logActionModel = new LogActionModel(
                            "Cập nhật biên bản tính giá" + (apartment != null ? $" cho căn hộ {apartment.Code}" : $" cho nhà {block.Code}"),
                            "Pricing",
                            input.Id,
                            HttpContext.Connection.RemoteIpAddress?.ToString(),
                            JsonConvert.SerializeObject(input),
                            (int)AppEnums.Action.UPDATE,
                            userId,
                            fullName
                        );
                        var logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);

                        await _context.SaveChangesAsync();

                        transaction.Commit();

                        def.meta = new Meta(200, ApiConstants.MessageResource.UPDATE_SUCCESS);
                        def.data = input;
                        return Ok(def);
                    }
                    catch (DbUpdateException e)
                    {
                        log.Error("DbUpdateException:" + e);
                        transaction.Rollback();
                        def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                        return Ok(def);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Put Exception:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        // DELETE (soft): api/Pricing/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            var token = _context.Tokens.FirstOrDefault(t => Convert.ToString(t.AccessToken) == accessToken);
            if (token == null) return Unauthorized();

            var def = new DefaultResponse();

            var identity = (ClaimsIdentity)User.Identity;
            int userId = int.Parse(identity.Claims.FirstOrDefault(c => c.Type == "Id")?.Value ?? "0");
            string fullName = identity.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? string.Empty;
            string access_key = identity.Claims.FirstOrDefault(c => c.Type == "AccessKey")?.Value;

            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.DELETED))
            {
                def.meta = new Meta(222, "Bạn không có quyền xoá mục này!");
                return Ok(def);
            }

            try
            {
                var data = await _context.Pricings.FindAsync(id);
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_DELETE_MESSAGE);
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // Soft-delete chính
                        data.UpdatedAt = DateTime.Now;
                        data.UpdatedById = userId;
                        data.UpdatedBy = fullName;
                        data.Status = EntityStatus.DELETED;
                        _context.Update(data);

                        // Soft-delete các bảng con còn active
                        var construction = _context.PricingConstructionPricies
                            .Where(l => l.PricingId == data.Id && l.Status != EntityStatus.DELETED).ToList();
                        if (construction.Count > 0)
                        {
                            construction.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = EntityStatus.DELETED;
                            });
                            _context.UpdateRange(construction);
                        }

                        var customers = _context.PricingCustomers
                            .Where(l => l.PricingId == data.Id && l.Status != EntityStatus.DELETED).ToList();
                        if (customers.Count > 0)
                        {
                            customers.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = EntityStatus.DELETED;
                            });
                            _context.UpdateRange(customers);
                        }

                        var landTbl = _context.PricingLandTbls
                            .Where(l => l.PricingId == data.Id && l.Status != EntityStatus.DELETED).ToList();
                        if (landTbl.Count > 0)
                        {
                            landTbl.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = EntityStatus.DELETED;
                            });
                            _context.UpdateRange(landTbl);
                        }

                        var officers = _context.PricingOfficers
                            .Where(l => l.PricingId == data.Id && l.Status != EntityStatus.DELETED).ToList();
                        if (officers.Count > 0)
                        {
                            officers.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = EntityStatus.DELETED;
                            });
                            _context.UpdateRange(officers);
                        }

                        var reduced = _context.PricingReducedPeople
                            .Where(l => l.PricingId == data.Id && l.Status != EntityStatus.DELETED).ToList();
                        if (reduced.Count > 0)
                        {
                            reduced.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = EntityStatus.DELETED;
                            });
                            _context.UpdateRange(reduced);
                        }

                        var aptLand = _context.PricingApartmentLandDetails
                            .Where(p => p.PricingId == data.Id && p.Status != EntityStatus.DELETED).ToList();
                        if (aptLand.Count > 0)
                        {
                            aptLand.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = EntityStatus.DELETED;
                            });
                            _context.UpdateRange(aptLand);
                        }

                        var replaceds = _context.PricingReplaceds
                            .Where(p => p.PricingId == data.Id && p.Status != EntityStatus.DELETED).ToList();
                        if (replaceds.Count > 0)
                        {
                            replaceds.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = EntityStatus.DELETED;
                            });
                            _context.UpdateRange(replaceds);
                        }

                        await _context.SaveChangesAsync();

                        // Ghi log
                        var logActionModel = new LogActionModel(
                            "Xóa biên bản tính giá",
                            "Pricing",
                            data.Id,
                            HttpContext.Connection.RemoteIpAddress?.ToString(),
                            JsonConvert.SerializeObject(data),
                            (int)AppEnums.Action.DELETED,
                            userId,
                            fullName
                        );
                        var logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (data.Id > 0) transaction.Commit();
                        else transaction.Rollback();

                        def.meta = new Meta(200, "Xóa bản ghi thành công!");
                        def.data = data;
                        return Ok(def);
                    }
                    catch (DbUpdateException e)
                    {
                        transaction.Rollback();
                        log.Error("DbUpdateException:" + e);

                        if (!PricingExists(data.Id))
                            def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_DELETE_MESSAGE);
                        else
                            def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);

                        return Ok(def);
                    }
                }
            }
            catch (Exception)
            {
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        private bool PricingExists(long id) =>
            _context.Pricings.Any(e => e.Id == id);
            // GET: api/Pricing/SearchByCode?code=...
[HttpGet("SearchByCode")]
public IActionResult SearchByCode([FromQuery] string code)
{
    // 1) Xác thực AccessToken
    string accessToken = Request.Headers[HeaderNames.Authorization];
    var token = _context.Tokens.FirstOrDefault(t => Convert.ToString(t.AccessToken) == accessToken);
    if (token == null) return Unauthorized();

    var def = new DefaultResponse();

    // 2) Quyền
    var identity = (ClaimsIdentity)User.Identity;
    string access_key = identity.Claims.FirstOrDefault(c => c.Type == "AccessKey")?.Value;
    int userId = int.Parse(identity.Claims.FirstOrDefault(c => c.Type == "Id")?.Value ?? "0");
    int moduleSystem = int.Parse(identity.Claims.FirstOrDefault(c => c.Type == "ModuleSystem")?.Value ?? "0");

    if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
    {
        def.meta = new Meta(222, "Bạn không có quyền xem dữ liệu tới mục này!");
        return Ok(def);
    }

    try
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            def.meta = new Meta(400, "Thiếu tham số 'code'.");
            return Ok(def);
        }

        code = code.Trim().ToLower();

        // 3) Nguồn dữ liệu (tự động bó theo NOC nếu cần)
        IQueryable<Pricing> q;
        if (moduleSystem == (int)AppEnums.ModuleSystem.NOC)
        {
            q = from b in _context.Blocks
                join p in _context.Pricings on b.Id equals p.BlockId
                join wm in _context.WardManagements on b.Ward equals wm.WardId
                where b.Status != AppEnums.EntityStatus.DELETED
                      && p.Status != AppEnums.EntityStatus.DELETED
                      && wm.Status != AppEnums.EntityStatus.DELETED
                      && wm.UserId == userId
                select p;
        }
        else
        {
            q = _context.Pricings.Where(p => p.Status != AppEnums.EntityStatus.DELETED);
        }

        // 4) Join để lọc theo Code của Block/Apartment
        var resultQuery =
            from p in q
            join b in _context.Blocks on p.BlockId equals b.Id
            join a in _context.Apartments on p.ApartmentId equals a.Id into aptLeft
            from a in aptLeft.DefaultIfEmpty()
            where b.Status != AppEnums.EntityStatus.DELETED
                  && (a == null || a.Status != AppEnums.EntityStatus.DELETED)
                  && (
                       (!string.IsNullOrEmpty(b.Code) && b.Code.ToLower().Contains(code)) ||
                       (a != null && !string.IsNullOrEmpty(a.Code) && a.Code.ToLower().Contains(code))
                     )
            orderby p.Id descending
            select new { Pricing = p, Block = b, Apartment = a };

        var rows = resultQuery.Take(50).ToList(); // chặn kết quả quá lớn

        // 5) Map sang DTO giống GetByPage để client dùng đồng nhất
        var data = new List<PricingData>();
        foreach (var r in rows)
        {
            var item = _mapper.Map<PricingData>(r.Pricing);
            item.block = r.Block;
            item.apartment = r.Apartment;

            // nạp gọn các bảng con nếu cần hiển thị
            item.constructionPricies = _context.PricingConstructionPricies
                .Where(x => x.PricingId == item.Id && x.Status != AppEnums.EntityStatus.DELETED).ToList();
            item.customers = _context.PricingCustomers
                .Where(x => x.PricingId == item.Id && x.Status != AppEnums.EntityStatus.DELETED).ToList();
            item.landPricingTbl = _context.PricingLandTbls
                .Where(x => x.PricingId == item.Id && x.Status != AppEnums.EntityStatus.DELETED).ToList();
            item.pricingOfficers = _context.PricingOfficers
                .Where(x => x.PricingId == item.Id && x.Status != AppEnums.EntityStatus.DELETED).ToList();
            item.reducedPerson = _context.PricingReducedPeople
                .Where(x => x.PricingId == item.Id && x.Status != AppEnums.EntityStatus.DELETED).ToList();
            item.pricingApartmentLandDetails = _context.PricingApartmentLandDetails
                .Where(x => x.PricingId == item.Id && x.Status != AppEnums.EntityStatus.DELETED)
                .OrderBy(x => x.UpdatedAt).ToList();
            item.pricingReplaceds = _context.PricingReplaceds
                .Where(x => x.PricingId == item.Id && x.Status != AppEnums.EntityStatus.DELETED).ToList();

            data.Add(item);
        }

        def.meta = new Meta(200, "Thành công");
        def.metadata = data.Count;
        def.data = data;
        return Ok(def);
    }
    catch (Exception ex)
    {
        log.Error("SearchByCode Error:" + ex);
        def.meta = new Meta(500, "Có lỗi xảy ra khi tìm kiếm.");
        return Ok(def);
    }
}

    }
    
}
