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
using DevExpress.XtraRichEdit.Import.Html;
using DevExpress.XtraRichEdit.Model;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Text;
using Newtonsoft.Json.Linq;
using IOITQln.Models.Input;
using DevExpress.CodeParser;
using DevExpress.ClipboardSource.SpreadsheetML;

namespace IOITQln.Controllers.ApiExtra
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ExtraDebtNotificationController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("notification", "notification");
        private static string functionCode = "TBCN";
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        private readonly IConfiguration _configuration;

        public ExtraDebtNotificationController(ApiDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
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
                    IQueryable<ExtraDebtNotification> data = _context.ExtraDebtNotifications.Where(c => c.Status != AppEnums.EntityStatus.DELETED);
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
                        def.data = data.ToList();
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
                ExtraDebtNotification data = await _context.ExtraDebtNotifications.FindAsync(id);

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
        public async Task<IActionResult> Post(ExtraDebtNotification input)
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
                input = (ExtraDebtNotification)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                if (input.DebtType == null)
                {
                    def.meta = new Meta(400, "Mã chức năng không được để trống!");
                    return Ok(def);
                }

                if (input.Title == null)
                {
                    def.meta = new Meta(400, "Căn nhà không được để trống!");
                    return Ok(def);
                }

                if (input.Content == null)
                {
                    def.meta = new Meta(400, "Căn hộ không được để trống!");
                    return Ok(def);
                }

                if (input.GroupNotification == null)
                {
                    def.meta = new Meta(400, "địa chỉ không được để trống!");
                    return Ok(def);
                }

                if (input.ListNotification == null)
                {
                    def.meta = new Meta(400, "Tên chủ hộ không được để trống!");
                    return Ok(def);
                }


                if (input.ToDate == null)
                {
                    def.meta = new Meta(400, "Thời gian yêu cầu hỗ trợ không được để trống!");
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.CreatedById = userId;
                    input.CreatedBy = fullName;
                    _context.ExtraDebtNotifications.Add(input);

                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Thêm mới yêu cầu hỗ trợ: " + input.Title, "ExtraDebtNotification", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
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
                        if (ExtraDebtNotificationExists(input.Id))
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
        public async Task<IActionResult> Put(int id, [FromBody] ExtraDebtNotification input)
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
                input = (ExtraDebtNotification)UtilsService.TrimStringPropertyTypeObject(input);

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

                if (input.DebtType == null)
                {
                    def.meta = new Meta(400, "Mã chức năng không được để trống!");
                    return Ok(def);
                }

                if (input.Title == null)
                {
                    def.meta = new Meta(400, "Căn nhà không được để trống!");
                    return Ok(def);
                }

                if (input.Content == null)
                {
                    def.meta = new Meta(400, "Căn hộ không được để trống!");
                    return Ok(def);
                }

                if (input.GroupNotification == null)
                {
                    def.meta = new Meta(400, "địa chỉ không được để trống!");
                    return Ok(def);
                }

                if (input.ListNotification == null)
                {
                    def.meta = new Meta(400, "Tên chủ hộ không được để trống!");
                    return Ok(def);
                }


                if (input.ToDate == null)
                {
                    def.meta = new Meta(400, "Thời gian yêu cầu hỗ trợ không được để trống!");
                    return Ok(def);
                }


                if (input.ToDate == null)
                {
                    def.meta = new Meta(400, "Thời gian yêu cầu hỗ trợ không được để trống!");
                    return Ok(def);
                }

                ExtraDebtNotification data = await _context.ExtraDebtNotifications.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
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

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Sửa yêu cầu hỗ trợ: " + input.Title, "ExtraDebtNotification", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, userId, fullName);
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
                        if (!ExtraDebtNotificationExists(data.Id))
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
                ExtraDebtNotification data = await _context.ExtraDebtNotifications.FindAsync(id);
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

                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Xóa Yêu cầu hỗ trợ: " + data.Title, "ExtraDebtNotification", data.Id, HttpContext.Connection.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.DELETED, userId, fullName);
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
                        if (!ExtraDebtNotificationExists(data.Id))
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

        private bool ExtraDebtNotificationExists(int id)
        {
            return _context.ExtraDebtNotifications.Count(e => e.Id == id) > 0;
        }

        [Authorize]
        [Route("changeStatus")]
        [HttpPut]
        public async Task<ActionResult> ChangeStatusUser([FromBody] ExtraDebtNotficationUpdateStatus req)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token tokenEntity = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (tokenEntity == null)
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
                bool success = false;
                string actionName = req.stt == AppEnums.EntityStatus.NA ? "Mở khóa " : (req.stt == AppEnums.EntityStatus.DELETED ? "Khóa " : "Đổi trạng thái");
                using (var transaction = _context.Database.BeginTransaction())
                {
                    foreach (var id in req.id)
                    {
                    ExtraDebtNotification data = await _context.ExtraDebtNotifications.FindAsync(id);
                    if (data == null)
                    {
                        def.meta = new Meta(404, "Không tìm thấy Thông báo công nợ!");
                        return Ok(def);
                    }
                    
                        data.UpdatedAt = DateTime.Now;
                        data.UpdatedById = userId;
                        data.UpdatedBy = fullName;
                        data.Status = req.stt;

                        _context.Update(data);

                        try
                        {
                            //thêm LogAction
                            
                            LogActionModel logActionModel = new LogActionModel(actionName + "Thông báo công nợ " + data.Title, "User", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.UPDATE, userId, fullName);
                            LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                            _context.Add(logAction);

                            await _context.SaveChangesAsync();
                            

                            if (req.stt == AppEnums.EntityStatus.NA || req.stt == AppEnums.EntityStatus.DELETED)
                            {
                                success = true;
                            }
                            else
                            {
                                success = true;
                            }

                            
                        }
                        catch (DbUpdateException e)
                        {
                            transaction.Rollback();
                            log.Error("DbUpdateException:" + e);
                            if (!ExtraDebtNotificationExists(data.Id))
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

                if (success)
                {
                    transaction.Commit();
                    def.meta = new Meta(200, actionName + " cho Thông báo công nợ thành công!");
                    def.data = null;
                    return Ok(def);
                }
                else
                {
                    transaction.Rollback();
                    def.meta = new Meta(500, actionName + "cho Thông báo công nợ không thành công!");
                    def.data = null;
                    return Ok(def);
                }
                }
            }

            catch (Exception e)
            {
                log.Error("Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        private static async Task<string> GetTokenAdminCliKeyCloak(string keycloakUrl, string realm, string keycloakClientId, string keycloakClientSecret)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // Tạo yêu cầu để xác thực token
                    var request = new HttpRequestMessage(HttpMethod.Post, keycloakUrl + "/auth/realms" + realm + "/protocol/openid-connect/token");
                    request.Content = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", keycloakClientId),
                    new KeyValuePair<string, string>("client_secret", keycloakClientSecret)
                });

                    // Gửi yêu cầu và lấy kết quả
                    var response = await client.SendAsync(request);
                    log.Error("GetTokenAdminCliKeyCloak Response:" + JsonConvert.SerializeObject(response));

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var data = JObject.Parse(content).ToObject<dynamic>();

                        return data.access_token;
                    }
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                log.Error("GetTokenAdminCliKeyCloak Function:" + ex);
                log.Error("Msg:" + ex.Message);
                return null;
            }

        }

        private static async Task<string> GetAccountIdKeyCloak(string keycloakUrl, string realm, string tokenAdminCli, string username)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAdminCli);

                // Gửi yêu cầu và lấy kết quả
                var response = await client.GetAsync(keycloakUrl + "/auth/admin/realms" + realm + "/users");
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<List<dynamic>>(content);

                    var user = data.Where(e => e.username == username.ToLower()).FirstOrDefault();
                    if (user != null)
                        return user.id;
                    else
                        return null;
                }
                else
                    return null;
            }
        }


    }
}
