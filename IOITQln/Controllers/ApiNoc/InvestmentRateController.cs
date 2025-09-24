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
using static IOITQln.Common.Enums.AppEnums;
using Microsoft.Net.Http.Headers;

namespace IOITQln.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InvestmentRateController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("InvestmentRate", "InvestmentRate");
        private static string functionCode = "INVESTMENT_RATE_MANAGEMENT";
        private readonly ApiDbContext _context;
        private IMapper _mapper;

        public InvestmentRateController(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
                    IQueryable<InvestmentRate> data = _context.InvestmentRaties.Where(c => c.Status != AppEnums.EntityStatus.DELETED);
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
                        List<InvestmentRateData> res = _mapper.Map<List<InvestmentRateData>>(data.ToList());
                        foreach (InvestmentRateData item in res)
                        {
                            Decree decree_type1 = _context.Decreies.Where(d => d.Id == item.DecreeType1Id).FirstOrDefault();
                            item.DecreeType1Name = decree_type1 != null ? decree_type1.Code : "";

                            Decree decree_type2 = _context.Decreies.Where(d => d.Id == item.DecreeType2Id).FirstOrDefault();
                            item.DecreeType2Name = decree_type2 != null ? decree_type2.Code : "";

                            item.investmentRateItems = _context.InvestmentRateItems.Where(i => i.InvestmentRateId == item.Id && i.Status != AppEnums.EntityStatus.DELETED).ToList();
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

        // GET: api/InvestmentRate/1
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
                InvestmentRate data = await _context.InvestmentRaties.FindAsync(id);

                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                    return Ok(def);
                }

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = data;
                return Ok(def);
            }
            catch(Exception ex)
            {
                log.Error("GetById Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        // POST: api/InvestmentRate
        [HttpPost]
        public async Task<IActionResult> Post(InvestmentRateData input)
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
                input = (InvestmentRateData)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                //if (input.Code == null || input.Code == "")
                //{
                //    def.meta = new Meta(400, "Mã không được để trống!");
                //    return Ok(def);
                //}

                //if (input.Name == null || input.Name == "")
                //{
                //    def.meta = new Meta(400, "Tên không được để trống!");
                //    return Ok(def);
                //}

                //InvestmentRate codeExist = _context.InvestmentRaties.Where(f => f.Code == input.Code && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                //if (codeExist != null)
                //{
                //    def.meta = new Meta(211, "Mã đã tồn tại!");
                //    return Ok(def);
                //}

                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.CreatedById = userId;
                    input.CreatedBy = fullName;
                    _context.InvestmentRaties.Add(input);

                    try
                    {
                        await _context.SaveChangesAsync();

                        //InvestmentRateItem
                        if (input.investmentRateItems != null)
                        {
                            foreach (var investmentRateItem in input.investmentRateItems)
                            {
                                investmentRateItem.InvestmentRateId = input.Id;
                                investmentRateItem.CreatedBy = fullName;
                                investmentRateItem.CreatedById = userId;

                                _context.InvestmentRateItems.Add(investmentRateItem);
                            }
                        }

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Thêm mới suất vốn đầu tư ", "InvestmentRate", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
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
                        if (InvestmentRateExists(input.Id))
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

        // PUT: api/InvestmentRate/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] InvestmentRateData input)
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
                input = (InvestmentRateData)UtilsService.TrimStringPropertyTypeObject(input);

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

                //if (input.Code == null || input.Code == "")
                //{
                //    def.meta = new Meta(400, "Mã không được để trống!");
                //    return Ok(def);
                //}

                //if (input.Name == null || input.Name == "")
                //{
                //    def.meta = new Meta(400, "Tên không được để trống!");
                //    return Ok(def);
                //}

                InvestmentRate data = await _context.InvestmentRaties.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
                if(data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_UPDATE_MESSAGE);
                    return Ok(def);
                }

                //InvestmentRate codeExist = _context.InvestmentRaties.Where(f => f.Code == input.Code && f.Status != AppEnums.EntityStatus.DELETED && f.Id != id).FirstOrDefault();
                //if (codeExist != null)
                //{
                //    def.meta = new Meta(211, "Mã đã tồn tại!");
                //    return Ok(def);
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

                        //useValueCoefficientITem
                        List<InvestmentRateItem> investmentRateItems = _context.InvestmentRateItems.Where(l => l.InvestmentRateId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).AsNoTracking().ToList();
                        if (input.investmentRateItems != null)
                        {
                            foreach (var investmentRateItem in input.investmentRateItems)
                            {
                                InvestmentRateItem investmentRateItemExist = investmentRateItems.Where(l => l.Id == investmentRateItem.Id).FirstOrDefault();
                                if (investmentRateItemExist == null)
                                {
                                    investmentRateItem.InvestmentRateId = input.Id;
                                    investmentRateItem.CreatedBy = fullName;
                                    investmentRateItem.CreatedById = userId;

                                    _context.InvestmentRateItems.Add(investmentRateItem);
                                }
                                else
                                {
                                    investmentRateItem.InvestmentRateId = input.Id;
                                    investmentRateItem.CreatedAt = investmentRateItemExist.CreatedAt;
                                    investmentRateItem.CreatedBy = investmentRateItemExist.CreatedBy;
                                    investmentRateItem.CreatedById = investmentRateItemExist.CreatedById;
                                    investmentRateItem.UpdatedBy = fullName;
                                    investmentRateItem.UpdatedById = userId;

                                    _context.Update(investmentRateItem);

                                    investmentRateItems.Remove(investmentRateItemExist);
                                }
                            }
                        }

                        if (investmentRateItems.Count > 0)
                        {
                            investmentRateItems.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = AppEnums.EntityStatus.DELETED;
                            });
                            _context.UpdateRange(investmentRateItems);
                        }

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Sửa suất vốn đầu tư ", "InvestmentRate", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, userId, fullName);
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
                        if (!InvestmentRateExists(data.Id))
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

        // DELETE: api/InvestmentRate/1
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
                InvestmentRate data = await _context.InvestmentRaties.FindAsync(id);
                if(data == null)
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

                    List<InvestmentRateItem> investmentRateItems = _context.InvestmentRateItems.Where(c => c.InvestmentRateId == data.Id && c.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if (investmentRateItems.Count > 0)
                    {
                        investmentRateItems.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(investmentRateItems);
                    }

                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Xóa suất vốn đầu tư ", "InvestmentRate", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.DELETED, userId, fullName);
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
                        if (!InvestmentRateExists(data.Id))
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

        [HttpGet("getInvestmentRateItems/{typeReportApply}")]
        public async Task<IActionResult> getInvestmentRateItems(TypeReportApply typeReportApply)
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
                List<InvestmentRateItemData> data = await (from pl in _context.InvestmentRaties
                                                join pli in _context.InvestmentRateItems on pl.Id equals pli.InvestmentRateId
                                                where pl.Status != AppEnums.EntityStatus.DELETED
                                                && pli.Status != AppEnums.EntityStatus.DELETED
                                                && pl.TypeReportApply == typeReportApply
                                                select new InvestmentRateItemData
                                                {
                                                    Id = pli.Id,
                                                    InvestmentRateId = pli.Id,
                                                    DecreeType1Id = pl.DecreeType1Id,
                                                    TypeReportApply = pl.TypeReportApply,
                                                    Des = pl.Des,
                                                    LineInfo = pli.LineInfo,
                                                    DetailInfo = pli.DetailInfo,
                                                    Value = pli.Value,
                                                    Value1 = pli.Value1,
                                                    Value2 = pli.Value2
                                                }).ToListAsync();

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.metadata = data.Count;
                def.data = data;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("getPriceListItems Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        private bool InvestmentRateExists(int id)
        {
            return _context.InvestmentRaties.Count(e => e.Id == id) > 0;
        }
    }
}
