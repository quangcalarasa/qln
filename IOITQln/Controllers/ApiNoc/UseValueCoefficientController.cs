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
    public class UseValueCoefficientController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("UseValueCoefficient", "UseValueCoefficient");
        private static string functionCode = "USE_VALUE_COEFFICIENT_MANAGEMENT";
        private readonly ApiDbContext _context;
        private IMapper _mapper;

        public UseValueCoefficientController(ApiDbContext context, IMapper mapper)
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
                    IQueryable<UseValueCoefficient> data = _context.UseValueCoefficients.Where(c => c.Status != AppEnums.EntityStatus.DELETED);
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
                        List<UseValueCoefficientData> res = _mapper.Map<List<UseValueCoefficientData>>(data.ToList());
                        foreach (UseValueCoefficientData item in res)
                        {
                            Decree decree_type1 = _context.Decreies.Where(d => d.Id == item.DecreeType1Id).FirstOrDefault();
                            item.DecreeType1Name = decree_type1 != null ? decree_type1.Code : "";

                            Decree decree_type2 = _context.Decreies.Where(d => d.Id == item.DecreeType2Id).FirstOrDefault();
                            item.DecreeType2Name = decree_type2 != null ? decree_type2.Code : "";

                            item.useValueCoefficientItems = _context.UseValueCoefficientItems.Where(c => c.UseValueCoefficientId == item.Id && c.Status != AppEnums.EntityStatus.DELETED).ToList();
                            item.useValueCoefficientTypeReportApplies = _context.UseValueCoefficientTypeReportApplies.Where(c => c.UseValueCoefficientId == item.Id && c.Status != AppEnums.EntityStatus.DELETED).ToList();
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

        // GET: api/UseValueCoefficient/1
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
                UseValueCoefficient data = await _context.UseValueCoefficients.FindAsync(id);

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

        // POST: api/UseValueCoefficient
        [HttpPost]
        public async Task<IActionResult> Post(UseValueCoefficientData input)
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
                input = (UseValueCoefficientData)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.CreatedById = userId;
                    input.CreatedBy = fullName;
                    _context.UseValueCoefficients.Add(input);

                    try
                    {
                        await _context.SaveChangesAsync();

                        //UseValueCoefficientItem
                        if (input.useValueCoefficientItems != null)
                        {
                            foreach (var useValueCoefficientItem in input.useValueCoefficientItems)
                            {
                                useValueCoefficientItem.UseValueCoefficientId = input.Id;
                                useValueCoefficientItem.CreatedBy = fullName;
                                useValueCoefficientItem.CreatedById = userId;

                                _context.UseValueCoefficientItems.Add(useValueCoefficientItem);
                            }
                        }

                        //UseValueCoefficientTypeReportApply
                        if (input.useValueCoefficientTypeReportApplies != null)
                        {
                            foreach (var useValueCoefficientTypeReportApply in input.useValueCoefficientTypeReportApplies)
                            {
                                useValueCoefficientTypeReportApply.UseValueCoefficientId = input.Id;
                                useValueCoefficientTypeReportApply.CreatedBy = fullName;
                                useValueCoefficientTypeReportApply.CreatedById = userId;

                                _context.UseValueCoefficientTypeReportApplies.Add(useValueCoefficientTypeReportApply);
                            }
                        }

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Thêm mới Hệ số điều chỉnh giá trị sử dụng", "UseValueCoefficient", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
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
                        if (useValueCoefficientExists(input.Id))
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

        // PUT: api/useValueCoefficient/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] UseValueCoefficientData input)
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
                input = (UseValueCoefficientData)UtilsService.TrimStringPropertyTypeObject(input);

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

                UseValueCoefficient data = await _context.UseValueCoefficients.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
                if(data == null)
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

                        //useValueCoefficientITem
                        List<UseValueCoefficientItem> useValueCoefficientItems = _context.UseValueCoefficientItems.Where(l => l.UseValueCoefficientId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).AsNoTracking().ToList();
                        if (input.useValueCoefficientItems != null)
                        {
                            foreach (var useValueCoefficientItem in input.useValueCoefficientItems)
                            {
                                UseValueCoefficientItem useValueCoefficientItemExist = useValueCoefficientItems.Where(l => l.Id == useValueCoefficientItem.Id).FirstOrDefault();
                                if (useValueCoefficientItemExist == null)
                                {
                                    useValueCoefficientItem.UseValueCoefficientId = input.Id;
                                    useValueCoefficientItem.CreatedBy = fullName;
                                    useValueCoefficientItem.CreatedById = userId;

                                    _context.UseValueCoefficientItems.Add(useValueCoefficientItem);
                                }
                                else
                                {
                                    useValueCoefficientItem.UseValueCoefficientId = input.Id;
                                    useValueCoefficientItem.CreatedBy = useValueCoefficientItemExist.CreatedBy;
                                    useValueCoefficientItem.CreatedById = useValueCoefficientItemExist.CreatedById;
                                    useValueCoefficientItem.UpdatedBy = fullName;
                                    useValueCoefficientItem.UpdatedById = userId;

                                    _context.Update(useValueCoefficientItem);

                                    useValueCoefficientItems.Remove(useValueCoefficientItemExist);
                                }
                            }
                        }

                        if (useValueCoefficientItems.Count > 0)
                        {
                            useValueCoefficientItems.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = AppEnums.EntityStatus.DELETED;
                            });
                            _context.UpdateRange(useValueCoefficientItems);
                        }

                        //UseValueCoefficientTypeReportApply
                        List<UseValueCoefficientTypeReportApply> useValueCoefficientTypeReportApplies = _context.UseValueCoefficientTypeReportApplies.Where(l => l.UseValueCoefficientId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).AsNoTracking().ToList();
                        if (input.useValueCoefficientTypeReportApplies != null)
                        {
                            foreach (var useValueCoefficientTypeReportApply in input.useValueCoefficientTypeReportApplies)
                            {
                                UseValueCoefficientTypeReportApply useValueCoefficientTypeReportApplyExist = useValueCoefficientTypeReportApplies.Where(l => l.TypeReportApply == useValueCoefficientTypeReportApply.TypeReportApply).FirstOrDefault();
                                if (useValueCoefficientTypeReportApplyExist == null)
                                {
                                    useValueCoefficientTypeReportApply.UseValueCoefficientId = input.Id;
                                    useValueCoefficientTypeReportApply.CreatedBy = fullName;
                                    useValueCoefficientTypeReportApply.CreatedById = userId;

                                    _context.UseValueCoefficientTypeReportApplies.Add(useValueCoefficientTypeReportApply);
                                }
                                else
                                {
                                    useValueCoefficientTypeReportApply.UseValueCoefficientId = input.Id;
                                    useValueCoefficientTypeReportApply.CreatedBy = useValueCoefficientTypeReportApplyExist.CreatedBy;
                                    useValueCoefficientTypeReportApply.CreatedById = useValueCoefficientTypeReportApplyExist.CreatedById;
                                    useValueCoefficientTypeReportApply.UpdatedBy = fullName;
                                    useValueCoefficientTypeReportApply.UpdatedById = userId;

                                    _context.Update(useValueCoefficientTypeReportApply);

                                    useValueCoefficientTypeReportApplies.Remove(useValueCoefficientTypeReportApplyExist);
                                }
                            }
                        }

                        if (useValueCoefficientTypeReportApplies.Count > 0)
                        {
                            useValueCoefficientTypeReportApplies.ForEach(item => {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = AppEnums.EntityStatus.DELETED;
                            });
                            _context.UpdateRange(useValueCoefficientTypeReportApplies);
                        }

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Sửa Hệ số điều chỉnh giá trị sử dụng ", "UseValueCoefficient", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, userId, fullName);
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
                        if (!useValueCoefficientExists(data.Id))
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

        // DELETE: api/useValueCoefficient/1
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
                UseValueCoefficient data = await _context.UseValueCoefficients.FindAsync(id);
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

                    List<UseValueCoefficientItem> useValueCoefficientItems = _context.UseValueCoefficientItems.Where(c => c.UseValueCoefficientId == data.Id && c.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if (useValueCoefficientItems.Count > 0)
                    {
                        useValueCoefficientItems.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(useValueCoefficientItems);
                    }

                    List<UseValueCoefficientTypeReportApply> useValueCoefficientTypeReportApplies = _context.UseValueCoefficientTypeReportApplies.Where(c => c.UseValueCoefficientId == data.Id && c.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if (useValueCoefficientTypeReportApplies.Count > 0)
                    {
                        useValueCoefficientTypeReportApplies.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(useValueCoefficientTypeReportApplies);
                    }

                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Xóa Hệ số điều chỉnh giá trị sử dụng", "useValueCoefficient", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.DELETED, userId, fullName);
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
                        if (!useValueCoefficientExists(data.Id))
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

        //GET: get getUseValueCoefficientItems by DecreeType1Id and District
        [HttpGet("getUseValueCoefficientItems/{typeReportApply}")]
        public async Task<IActionResult> getUseValueCoefficientItems(TypeReportApply typeReportApply)
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
                List<UseValueCoefficientItemData> data = (from uvct in _context.UseValueCoefficientTypeReportApplies
                                            join l in _context.UseValueCoefficients on uvct.UseValueCoefficientId equals l.Id
                                            join li in _context.UseValueCoefficientItems on l.Id equals li.UseValueCoefficientId
                                            where l.Status != AppEnums.EntityStatus.DELETED
                                            && li.Status != AppEnums.EntityStatus.DELETED
                                            && uvct.Status != AppEnums.EntityStatus.DELETED
                                            && uvct.TypeReportApply == typeReportApply
                                            select new UseValueCoefficientItemData { 
                                                Id = li.Id,
                                                UseValueCoefficientId = li.UseValueCoefficientId,
                                                FloorId = li.FloorId,
                                                IsMezzanine = li.IsMezzanine,
                                                Value = li.Value,
                                                DecreeType1Id = l.DecreeType1Id,
                                                TypeReportApply = uvct.TypeReportApply
                                            }).ToList();

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

        private bool useValueCoefficientExists(int id)
        {
            return _context.UseValueCoefficients.Count(e => e.Id == id) > 0;
        }
    }
}
