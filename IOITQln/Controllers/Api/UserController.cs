using IOITQln.Common.Constants;
using IOITQln.Common.Enums;
using IOITQln.Common.Services;
using IOITQln.Common.ViewModels.Common;
using IOITQln.Models.Dto;
using IOITQln.Persistence;
using AutoMapper;
using log4net;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOITQln.Entities;
using IOITQln.Models.Data;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Web;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using Microsoft.Net.Http.Headers;
using IOITQln.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using System.IO;
using IOITQln.Common.Hub;
using Microsoft.AspNetCore.SignalR;
using static IOITQln.Common.Enums.AppEnums;
using IOITQln.Common.Attribute;
using Microsoft.AspNetCore.Identity;

namespace IOITQln.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("user", "user");
        private const string functionCode = "USER_MANAGEMENT";
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IFirebaseService _firebaseService;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;

        public UserController(ApiDbContext context, IMapper mapper, IConfiguration configuration, IFirebaseService firebaseService, IWebHostEnvironment hostingEnvironment, IHubContext<BroadcastHub, IHubClient> hubContext)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _firebaseService = firebaseService;
            _hostingEnvironment = hostingEnvironment;
            _hubContext = hubContext;
        }

        #region user management
        [Authorize]
        [HttpGet("GetByPage")]
        [TypeFilter(typeof(AuthorizationAttribute), Arguments = new object[] { AppEnums.Action.VIEW, functionCode })]
        public IActionResult GetByPage([FromQuery] FilteredPagination paging, [FromQuery] int? roleId)
        {
            DefaultResponse def = new DefaultResponse();

            try
            {
                if (paging != null)
                {
                    def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                    IQueryable<User> data;
                    if(roleId != null)
                    {
                        data = (from u in _context.Users
                                join ur in _context.UserRoles on u.Id equals ur.UserId
                                where u.Status != AppEnums.EntityStatus.DELETED
                                    && ur.Status != AppEnums.EntityStatus.DELETED
                                    && ur.RoleId == roleId
                                select u).Distinct().AsQueryable();
                    }
                    else
                    {
                        data = _context.Users.Where(c => c.Status != AppEnums.EntityStatus.DELETED);
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
                        List<UserData> res = _mapper.Map<List<UserData>>(data.ToList());
                        foreach (UserData item in res)
                        {
                            item.userRoles = _context.UserRoles.Where(e => e.UserId == item.Id && e.Status != AppEnums.EntityStatus.DELETED).ToList();
                            item.wardManagements = _context.WardManagements.Where(e => e.UserId == item.Id && e.Status != AppEnums.EntityStatus.DELETED).ToList();
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

        // GET: api/User/1
        [Authorize]
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
                User data = await _context.Users.FindAsync(id);

                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                    return Ok(def);
                }

                UserData res = _mapper.Map<UserData>(data);
                res.userRoles = _context.UserRoles.Where(e => e.UserId == res.Id && e.Status != AppEnums.EntityStatus.DELETED).ToList();

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = res;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("GetByPage Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [Authorize]
        [HttpGet("Status/{stt}")]
        public async Task<IActionResult> GetByStatus(byte stt)
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
                var data = _context.Users.Where(u => u.Status == (AppEnums.EntityStatus)stt).ToList();
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
                log.Error("GetByPage Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        // POST: api/User
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Post(UserData input)
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
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.CREATE))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_CREATE_MESSAGE);
                return Ok(def);
            }

            try
            {
                input = (UserData)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                if (input.UserName == null || input.UserName == "")
                {
                    def.meta = new Meta(400, "Tên tài khoản không được để trống!");
                    return Ok(def);
                }

                if (input.FullName == null || input.FullName == "")
                {
                    def.meta = new Meta(400, "Họ tên không được để trống!");
                    return Ok(def);
                }

                if (input.userRoles == null)
                {
                    def.meta = new Meta(400, "Danh sách nhóm quyền đang trống!");
                    return Ok(def);
                }

                if (input.userRoles.Count == 0)
                {
                    def.meta = new Meta(400, "Danh sách nhóm quyền đang trống!");
                    return Ok(def);
                }

                User usernameExist = _context.Users.Where(f => f.UserName.ToUpper() == input.UserName.ToUpper() && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                if (usernameExist != null)
                {
                    def.meta = new Meta(211, "Tài khoản đã tồn tại!");
                    return Ok(def);
                }

                User emailExist = _context.Users.Where(f => f.Email == input.Email && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                if (emailExist != null)
                {
                    def.meta = new Meta(211, "Email đã tồn tại!");
                    return Ok(def);
                }

                //Check Password valid
                if(!UtilsService.CheckPasswordValidParttern(input.Password))
                {
                    def.meta = new Meta(400, UtilsService.errMsgPartternPassword);
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.KeyLock = UtilsService.RandomString(20);
                    input.RegEmail = UtilsService.RandomString(8);

                    //Sửa Password hệ thống tự sinh
                    input.PasswordKeyCloak = input.Password;
                    input.Password = input.KeyLock + input.RegEmail;

                    //Encrypt PasswordKeyCloak
                    input.PasswordKeyCloak = Security.Encrypt(input.KeyLock, input.PasswordKeyCloak);

                    input.CreatedById = userId;
                    input.CreatedBy = fullName;
                    _context.Users.Add(input);

                    try
                    {
                        await _context.SaveChangesAsync();

                        //update password
                        input.Password = UtilsService.GetMD5Hash(input.KeyLock.Trim() + input.RegEmail.Trim() + input.Id + input.Password.Trim());
                        _context.Update(input);

                        if (input.userRoles != null)
                        {
                            foreach (UserRole userRole in input.userRoles)
                            {
                                userRole.UserId = input.Id;
                                userRole.CreatedBy = fullName;
                                userRole.CreatedById = userId;

                                _context.UserRoles.Add(userRole);
                            }
                        }

                        if (input.wardManagements != null)
                        {
                            foreach (WardManagement wardManagement in input.wardManagements)
                            {
                                wardManagement.UserId = input.Id;
                                wardManagement.CreatedBy = fullName;
                                wardManagement.CreatedById = userId;

                                _context.WardManagements.Add(wardManagement);
                            }
                        }

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Thêm mới tài khoản " + input.FullName, "User", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);


                        if (input.Id > 0)
                        {
                            input.AddedOnKeyCloak = false;
                            // POST tài khoản lên KeyCloak
                            try
                            {
                                string keycloakUrl = _configuration["KeycloakSettings:endpoint"];
                                string keycloakClientId = _configuration["KeycloakSettings:client_id"];
                                string keycloakClientSecret = _configuration["KeycloakSettings:client_secret"];
                                string realm = _configuration["KeycloakSettings:realm"];

                                var token = await GetTokenAdminCliKeyCloak(keycloakUrl, realm, keycloakClientId, keycloakClientSecret);

                                if(token != null)
                                {
                                    using (var client = new HttpClient())
                                    {
                                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                        UserKeyCloak userKeyCloak = new UserKeyCloak();
                                        userKeyCloak.firstName = input.FullName;
                                        userKeyCloak.lastName = "";
                                        userKeyCloak.email = input.Email;
                                        userKeyCloak.enabled = true;
                                        userKeyCloak.username = input.UserName;
                                        userKeyCloak.credentials = new List<Credential>();

                                        Credential credential = new Credential();
                                        credential.type = "password";
                                        credential.value = Security.Decrypt(input.KeyLock, input.PasswordKeyCloak);

                                        userKeyCloak.credentials.Add(credential);

                                        var stringPayload = JsonConvert.SerializeObject(userKeyCloak);
                                        var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                                        var response = await client.PostAsync(keycloakUrl + "/auth/admin/realms" + realm + "/users", httpContent);

                                        if(response.StatusCode == System.Net.HttpStatusCode.Created)
                                        {
                                            input.AddedOnKeyCloak = true;
                                        }
                                        else if(response.StatusCode == System.Net.HttpStatusCode.Conflict)
                                        {
                                            input.AddedOnKeyCloak = true;

                                            //Cập nhật lại Password
                                            //lấy id keycloak
                                            string idKeycloak = await GetAccountIdKeyCloak(keycloakUrl, realm, token, input.UserName);
                                            if (idKeycloak != null)
                                            {
                                                using (var second_client = new HttpClient())
                                                {
                                                    second_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                                    second_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                                    Credential second_credential = new Credential();
                                                    second_credential.type = "password";
                                                    second_credential.temporary = false;
                                                    second_credential.value = credential.value;

                                                    var second_StringPayload = JsonConvert.SerializeObject(second_credential);
                                                    var second_HttpContent = new StringContent(second_StringPayload, Encoding.UTF8, "application/json");

                                                    await second_client.PutAsync(keycloakUrl + "/auth/admin/realms" + realm + "/users/" + idKeycloak + "/reset-password", second_HttpContent);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch(Exception ex)
                            {
                                log.Error("AddedOnKeyCloak:" + ex);
                            }

                            _context.Update(input);
                            await _context.SaveChangesAsync();

                            transaction.Commit();
                        }
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
                        if (UserExists(input.Id))
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
                log.Error("Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        // PUT: api/User/1
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] UserData input)
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
                input = (UserData)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                if (input.UserName == null || input.UserName == "")
                {
                    def.meta = new Meta(400, "Tên tài khoản không được để trống!");
                    return Ok(def);
                }

                if (input.FullName == null || input.FullName == "")
                {
                    def.meta = new Meta(400, "Họ tên không được để trống!");
                    return Ok(def);
                }

                if (input.userRoles == null)
                {
                    def.meta = new Meta(400, "Danh sách chức năng đang trống!");
                    return Ok(def);
                }

                if (input.userRoles.Count == 0)
                {
                    def.meta = new Meta(400, "Danh sách chức năng đang trống!");
                    return Ok(def);
                }

                if (id != input.Id)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                User data = await _context.Users.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_UPDATE_MESSAGE);
                    return Ok(def);
                }

                User usernameExist = _context.Users.Where(f => f.UserName.ToUpper() == input.UserName.ToUpper() && f.Status != AppEnums.EntityStatus.DELETED && f.Id != id).FirstOrDefault();
                if (usernameExist != null)
                {
                    def.meta = new Meta(211, "Tài khoản đã tồn tại!");
                    return Ok(def);
                }

                User emailExist = _context.Users.Where(f => f.Email == input.Email && f.Status != AppEnums.EntityStatus.DELETED && f.Id != id).FirstOrDefault();
                if (emailExist != null)
                {
                    def.meta = new Meta(211, "Email đã tồn tại!");
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
                    input.Password = data.Password;
                    input.PasswordKeyCloak = data.PasswordKeyCloak;
                    input.RegEmail = data.RegEmail;
                    input.KeyLock = data.KeyLock;
                    input.Status = data.Status;

                    _context.Update(input);

                    try
                    {
                        List<UserRole> userRoles = _context.UserRoles.Where(e => e.UserId == input.Id && e.Status != AppEnums.EntityStatus.DELETED).ToList();
                        if (input.userRoles != null)
                        {
                            foreach (UserRole userRole in input.userRoles)
                            {
                                UserRole userRoleExist = userRoles.Where(e => e.RoleId == userRole.RoleId).FirstOrDefault();
                                if (userRoleExist == null)
                                {
                                    userRole.UserId = input.Id;
                                    userRole.CreatedBy = fullName;
                                    userRole.CreatedById = userId;
                                    _context.UserRoles.Add(userRole);
                                }
                                else
                                {
                                    userRoles.Remove(userRoleExist);
                                }
                            }
                        }

                        userRoles.ForEach(item => {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(userRoles);

                        List<WardManagement> wardManagements = _context.WardManagements.Where(e => e.UserId == input.Id && e.Status != AppEnums.EntityStatus.DELETED).ToList();
                        if (input.wardManagements != null)
                        {
                            foreach (WardManagement wardManagement in input.wardManagements)
                            {
                                WardManagement wardManagementExist = wardManagements.Where(e => e.WardId == wardManagement.WardId).FirstOrDefault();
                                if (wardManagementExist == null)
                                {
                                    wardManagement.UserId = input.Id;
                                    wardManagement.CreatedBy = fullName;
                                    wardManagement.CreatedById = userId;
                                    _context.WardManagements.Add(wardManagement);
                                }
                                else
                                {
                                    wardManagements.Remove(wardManagementExist);
                                }
                            }
                        }

                        wardManagements.ForEach(item => {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(wardManagements);

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Sửa tài khoản " + input.FullName, "User", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, userId, fullName);
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
                        if (!UserExists(data.Id))
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
                log.Error("Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        // DELETE: api/User/1
        [Authorize]
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
                User data = await _context.Users.FindAsync(id);
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

                    List<UserRole> ur = _context.UserRoles.Where(e => e.UserId == data.Id && e.Status != AppEnums.EntityStatus.DELETED).ToList();
                    ur.ForEach(item => {
                        item.UpdatedAt = DateTime.Now;
                        item.UpdatedById = userId;
                        item.UpdatedBy = fullName;
                        item.Status = AppEnums.EntityStatus.DELETED;
                    });
                    _context.UpdateRange(ur);

                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Xóa tài khoản " + data.FullName, "User", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.DELETED, userId, fullName);
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
                        if (!UserExists(data.Id))
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

        private bool UserExists(int id)
        {
            return _context.Users.Count(e => e.Id == id) > 0;
        }

        //Cấp lại mật khẩu cho tài khoản
        [Authorize]
        [HttpPut("changePassUser/{id}")]
        public async Task<ActionResult> ChangePassUser(int id, [FromBody] ChangePassUserModel input)
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
                input = (ChangePassUserModel)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                if (input.NewPassword == null || input.NewPassword == "")
                {
                    def.meta = new Meta(400, "Dữ liệu không hợp lệ!");
                    return Ok(def);
                }

                User data = await _context.Users.FindAsync(id);
                if (data == null)
                {
                    def.meta = new Meta(404, "Không tìm thấy tài khoản người dùng!");
                    return Ok(def);
                }

                //Check Password valid
                if (!UtilsService.CheckPasswordValidParttern(input.NewPassword))
                {
                    def.meta = new Meta(400, UtilsService.errMsgPartternPassword);
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    string newPassword = data.KeyLock.Trim() + data.RegEmail.Trim() + data.Id + input.NewPassword.Trim();
                    newPassword = UtilsService.GetMD5Hash(newPassword);

                    data.Password = newPassword;
                    data.UpdatedAt = DateTime.Now;
                    data.UpdatedById = userId;
                    data.UpdatedBy = fullName;

                    _context.Update(data);

                    try
                    {
                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Cấp lại mật khẩu cho tài khoản " + data.FullName, "User", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.UPDATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);

                        await _context.SaveChangesAsync();

                        if (data.Id > 0)
                            transaction.Commit();
                        else
                            transaction.Rollback();

                        def.meta = new Meta(200, "Cấp lại mật khẩu thành công!");
                        def.data = data;
                        return Ok(def);
                    }
                    catch (DbUpdateException e)
                    {
                        transaction.Rollback();
                        log.Error("DbUpdateException:" + e);
                        if (!UserExists(data.Id))
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
                log.Error("Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        //Cấp lại mật khẩu cho tài khoản - đẩy thông tin lên Keycloak
        [Authorize]
        [HttpPut("changePassUserApplyKeycloak/{id}")]
        public async Task<ActionResult> ChangePassUserApplyKeycloak(int id, [FromBody] ChangePassUserModel input)
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
                input = (ChangePassUserModel)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                if (input.NewPassword == null || input.NewPassword == "")
                {
                    def.meta = new Meta(400, "Dữ liệu không hợp lệ!");
                    return Ok(def);
                }

                User data = await _context.Users.FindAsync(id);
                if (data == null)
                {
                    def.meta = new Meta(404, "Không tìm thấy tài khoản người dùng!");
                    return Ok(def);
                }

                //Check Password valid
                if (!UtilsService.CheckPasswordValidParttern(input.NewPassword))
                {
                    def.meta = new Meta(400, UtilsService.errMsgPartternPassword);
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    if(data.KeyLock == null)
                    {
                        data.KeyLock = UtilsService.RandomString(20);
                        data.RegEmail = data.RegEmail ?? UtilsService.RandomString(20);

                        data.Password = data.KeyLock + data.RegEmail;
                    }

                    data.PasswordKeyCloak = Security.Encrypt(data.KeyLock, input.NewPassword);
                    data.UpdatedAt = DateTime.Now;
                    data.UpdatedById = userId;
                    data.UpdatedBy = fullName;

                    _context.Update(data);

                    try
                    {
                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Cấp lại mật khẩu cho tài khoản " + data.FullName, "User", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.UPDATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);

                        await _context.SaveChangesAsync();

                        bool success = false;
                        //Đẩy thông tin mật khẩu thay đổi lên Keycloak
                        try
                        {
                            string keycloakUrl = _configuration["KeycloakSettings:endpoint"];
                            string keycloakClientId = _configuration["KeycloakSettings:client_id"];
                            string keycloakClientSecret = _configuration["KeycloakSettings:client_secret"];
                            string realm = _configuration["KeycloakSettings:realm"];

                            var token = await GetTokenAdminCliKeyCloak(keycloakUrl, realm, keycloakClientId, keycloakClientSecret);
                            if (token != null)
                            {
                                //lấy id keycloak
                                string idKeycloak = await GetAccountIdKeyCloak(keycloakUrl, realm, token, data.UserName);
                                if(idKeycloak != null)
                                {
                                    using (var client = new HttpClient())
                                    {
                                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                        Credential credential = new Credential();
                                        credential.type = "password";
                                        credential.temporary = false;
                                        credential.value = input.NewPassword;

                                        var stringPayload = JsonConvert.SerializeObject(credential);
                                        var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                                        var response = await client.PutAsync(keycloakUrl + "/auth/admin/realms" + realm + "/users/" + idKeycloak + "/reset-password", httpContent);

                                        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                                        {
                                            success = true;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error("Push Info Keycloak Faild:" + ex);
                        }

                        if (success)
                        {
                            transaction.Commit();

                            //Gửi thông báo cho client để logout
                            SignalRNotify signalRNotify = new SignalRNotify("Đăng xuất", "Tài khoản vừa được cấp lại mật khẩu. Cần đăng nhập lại!", TypeSignalRNotify.LOG_OUT);
                            await _hubContext.Clients.Group(id.ToString()).BroadcastMessage(signalRNotify);

                            def.meta = new Meta(200, "Cấp lại mật khẩu thành công!");
                            def.data = data;
                            return Ok(def);
                        }
                        else
                        {
                            transaction.Rollback();
                            def.meta = new Meta(500, "Đẩy thông tin Keycloak thất bại. Cấp lại mật khẩu không thành công!");
                            def.data = data;
                            return Ok(def);
                        }
                    }
                    catch (DbUpdateException e)
                    {
                        transaction.Rollback();
                        log.Error("DbUpdateException:" + e);
                        if (!UserExists(data.Id))
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
                log.Error("Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [Authorize]
        [Route("changeStatusUser/{id}/{stt}")]
        [HttpPut]
        public async Task<ActionResult> ChangeStatusUser(int id, byte stt)
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
                User data = await _context.Users.FindAsync(id);
                if (data == null)
                {
                    def.meta = new Meta(404, "Không tìm thấy tài khoản người dùng!");
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    data.UpdatedAt = DateTime.Now;
                    data.UpdatedById = userId;
                    data.UpdatedBy = fullName;
                    data.Status = (AppEnums.EntityStatus)stt;

                    _context.Update(data);

                    try
                    {
                        //thêm LogAction
                        string actionName = (AppEnums.EntityStatus)stt == AppEnums.EntityStatus.NORMAL ? "Mở khóa " : ((AppEnums.EntityStatus)stt == AppEnums.EntityStatus.DELETED ? "Khóa " : "Đổi trạng thái cho ");
                        LogActionModel logActionModel = new LogActionModel(actionName + "tài khoản " + data.FullName, "User", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.UPDATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);

                        await _context.SaveChangesAsync();

                        bool success = false;
                        //Đẩy thông tin mật khẩu thay đổi lên Keycloak
                        if((AppEnums.EntityStatus)stt == AppEnums.EntityStatus.NORMAL || (AppEnums.EntityStatus)stt == AppEnums.EntityStatus.DELETED)
                        {
                            try
                            {
                                string keycloakUrl = _configuration["KeycloakSettings:endpoint"];
                                string keycloakClientId = _configuration["KeycloakSettings:client_id"];
                                string keycloakClientSecret = _configuration["KeycloakSettings:client_secret"];
                                string realm = _configuration["KeycloakSettings:realm"];

                                var token = await GetTokenAdminCliKeyCloak(keycloakUrl, realm, keycloakClientId, keycloakClientSecret);
                                if (token != null)
                                {
                                    //lấy id keycloak
                                    string idKeycloak = await GetAccountIdKeyCloak(keycloakUrl, realm, token, data.UserName);
                                    if (idKeycloak != null)
                                    {
                                        using (var client = new HttpClient())
                                        {
                                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                            dynamic body = new System.Dynamic.ExpandoObject();
                                            body.enabled = (AppEnums.EntityStatus)stt == AppEnums.EntityStatus.NORMAL ? true : false;

                                            var stringPayload = JsonConvert.SerializeObject(body);
                                            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                                            var response = await client.PutAsync(keycloakUrl + "/auth/admin/realms" + realm + "/users/" + idKeycloak, httpContent);

                                            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                                            {
                                                success = true;
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error("Push Info Keycloak Faild:" + ex);
                            }

                        }
                        else
                        {
                            success = true;
                        }

                        if (success)
                        {
                            transaction.Commit();
                            def.meta = new Meta(200, actionName +" cho tài khoản thành công!");
                            def.data = data;
                            return Ok(def);
                        }
                        else
                        {
                            transaction.Rollback();
                            def.meta = new Meta(500, "Đẩy thông tin Keycloak thất bại. " + actionName + " cho tài khoản không thành công!");
                            def.data = data;
                            return Ok(def);
                        }
                    }
                    catch (DbUpdateException e)
                    {
                        transaction.Rollback();
                        log.Error("DbUpdateException:" + e);
                        if (!UserExists(data.Id))
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
                log.Error("Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        //API đồng bộ tài khoản lên keycloak server
        [Authorize]
        [HttpPost("SyncUserInfoToKeycloak")]
        public async Task<IActionResult> SyncUserInfoToKeycloak()
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
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.CREATE))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_ACTION_MESSAGE);
                return Ok(def);
            }

            try
            {

                List<User> users = _context.Users.Where(e => e.Status != AppEnums.EntityStatus.DELETED && e.KeyLock != null && e.PasswordKeyCloak != null).ToList();

                string keycloakUrl = _configuration["KeycloakSettings:endpoint"];
                string keycloakClientId = _configuration["KeycloakSettings:client_id"];
                string keycloakClientSecret = _configuration["KeycloakSettings:client_secret"];
                string realm = _configuration["KeycloakSettings:realm"];

                var token = await GetTokenAdminCliKeyCloak(keycloakUrl, realm, keycloakClientId, keycloakClientSecret);

                if (token != null)
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        foreach(var user in users)
                        {
                            UserKeyCloak userKeyCloak = new UserKeyCloak();
                            userKeyCloak.firstName = user.FullName;
                            userKeyCloak.lastName = "";
                            userKeyCloak.email = user.Email;
                            userKeyCloak.enabled = true;
                            userKeyCloak.username = user.UserName;
                            userKeyCloak.credentials = new List<Credential>();

                            Credential credential = new Credential();
                            credential.type = "password";
                            credential.value = Security.Decrypt(user.KeyLock, user.PasswordKeyCloak);

                            userKeyCloak.credentials.Add(credential);

                            var stringPayload = JsonConvert.SerializeObject(userKeyCloak);
                            var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                            var response = await client.PostAsync(keycloakUrl + "/auth/admin/realms" + realm + "/users", httpContent);

                            log.Error("Res Keycloak Create User: " + response.StatusCode);

                            await Task.Delay(1000);
                        }

                        def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                        def.data = null;
                        return Ok(def);
                    }
                }
                else
                {
                    def.meta = new Meta(500, "GetTokenAdminCliKeyCloak Not Success!");
                    def.data = null;
                    return Ok(def);
                }
            }
            catch (Exception e)
            {
                log.Error("Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [Authorize]
        [HttpPost("Fcm")]
        public async Task<IActionResult> Fcm([FromBody] string tokenKey)
        {
            DefaultResponse def = new DefaultResponse();

            var identity = (ClaimsIdentity)User.Identity;
            int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id").Select(c => c.Value).SingleOrDefault());
            string fullName = identity.Claims.Where(c => c.Type == "FullName").Select(c => c.Value).SingleOrDefault() ?? string.Empty;
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.CREATE))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_ACTION_MESSAGE);
                return Ok(def);
            }

            try
            {
                int badge = 0;
                int badgeTotal = 0;
                string name = "Name Test";
                string contents = "Content Test";
                bool notificationSound = false;
                string returnUrl = "";
                string domain = "";

                await _firebaseService.sendNotification(tokenKey, userId, badge, badgeTotal, name, contents, notificationSound, returnUrl, domain);

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = null;
                return Ok(def);

            }
            catch (Exception e)
            {
                log.Error("Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }
        #endregion

        #region actions by user
        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginModel loginModel)
        {
            DefaultResponse def = new DefaultResponse();
            try
            {
                if (loginModel != null)
                {
                    if (loginModel.username != null && loginModel.password != null && loginModel.username != "" && loginModel.password != "")
                    {
                        string username = loginModel.username.Trim();
                        Entities.User user = _context.Users.Where(e => e.UserName.Equals(username) && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                        if (user != null)
                        {
                            string password = user.KeyLock.Trim() + user.RegEmail.Trim() + user.Id + loginModel.password.Trim();
                            password = UtilsService.GetMD5Hash(password);
                            Entities.User userLogin = _context.Users.Where(e => e.UserName.Equals(username) && e.Password == password && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            if (userLogin != null)
                            {
                                UserLoginData userLoginData = _mapper.Map<UserLoginData>(userLogin);

                                if (userLoginData.Status == AppEnums.EntityStatus.LOCK)
                                {
                                    def.meta = new Meta(223, "Tài khoản bị khóa!");
                                    return Ok(def);
                                }

                                Role roleMax = _context.Roles.Where(r => r.Id == userLogin.RoleMax && r.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                userLoginData.RoleCode = roleMax != null ? roleMax.Code : "";

                                int userId = userLoginData.Id;
                                List<MenuData> listFunctionRole = new List<MenuData>();
                                List<UserRole> userRole = _context.UserRoles.Where(e => e.UserId == userId && e.Status == AppEnums.EntityStatus.NORMAL).ToList();

                                foreach (var item in userRole)
                                {
                                    var listFRR = (from fr in _context.FunctionRoles
                                                   join f in _context.Functions on fr.FunctionId equals f.Id
                                                   where fr.TargetId == item.RoleId
                                                      && fr.Type == (int)AppEnums.TypeFunction.FUNCTION_ROLE
                                                      && fr.Status == AppEnums.EntityStatus.NORMAL
                                                      && f.Status == AppEnums.EntityStatus.NORMAL
                                                   select new
                                                   {
                                                       fr.Id,
                                                       fr.FunctionId,
                                                       fr.Type,
                                                       fr.Status,
                                                       fr.ActiveKey,
                                                       f.Location,
                                                       f.Code,
                                                       f.Name,
                                                       f.Url,
                                                       f.Icon,
                                                       f.FunctionParentId,
                                                       f.IsSpecialFunc,
                                                       f.SubSystem
                                                   }).OrderBy(e => e.Location).ToList();

                                    foreach (var itemFR in listFRR)
                                    {
                                        var fr = listFunctionRole.Where(e => e.MenuId == itemFR.FunctionId).ToList();
                                        if (fr.Count > 0)
                                        {
                                            string key1 = fr.FirstOrDefault().ActiveKey;
                                            if (fr.FirstOrDefault().ActiveKey != itemFR.ActiveKey)
                                            {
                                                key1 = plusActiveKey(fr.FirstOrDefault().ActiveKey, itemFR.ActiveKey);
                                            }
                                            fr.FirstOrDefault().ActiveKey = key1;
                                        }
                                        else
                                        {
                                            Function function = _context.Functions.Where(e => e.Id == itemFR.FunctionId).FirstOrDefault();
                                            if (function != null)
                                            {
                                                MenuData menu = new MenuData();
                                                menu.MenuId = itemFR.FunctionId;
                                                menu.Code = function.Code;
                                                menu.Name = function.Name;
                                                menu.Url = function.Url;
                                                menu.Icon = function.Icon;
                                                menu.MenuParent = (int)function.FunctionParentId;
                                                menu.ActiveKey = itemFR.ActiveKey;
                                                menu.IsSpecialFunc = function.IsSpecialFunc;
                                                menu.SubSystem = function.SubSystem;
                                                listFunctionRole.Add(menu);
                                            }
                                        }
                                    }
                                }

                                string access_key = "";
                                int count = listFunctionRole.Count;
                                if (count > 0)
                                {
                                    for (int i = 0; i < count - 1; i++)
                                    {
                                        if (listFunctionRole[i].ActiveKey != "000000000")
                                        {
                                            access_key += listFunctionRole[i].Code + ":" + listFunctionRole[i].ActiveKey + "-";
                                        }
                                    }

                                    access_key = access_key + listFunctionRole[count - 1].Code + ":" + listFunctionRole[count - 1].ActiveKey;
                                }

                                userLoginData.AccessKey = access_key;
                                userLoginData.listMenus = CreateMenu(listFunctionRole, 0);
                                var claims = new List<Claim>
                                {
                                    new Claim(JwtRegisteredClaimNames.Email, userLogin.Email ?? ""),
                                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                    new Claim(ClaimTypes.NameIdentifier, userLogin.Id.ToString()),
                                    new Claim(ClaimTypes.Name, userLogin.FullName),
                                    new Claim("Id", userLogin.Id.ToString()),
                                    new Claim("FullName", userLogin.FullName != null ? userLogin.FullName.ToString() : ""),
                                    new Claim("Phone", userLogin.Phone != null ? userLogin.Phone.ToString() : ""),
                                    new Claim("Avatar", userLogin.Avatar != null ? userLogin.Avatar.ToString() : ""),
                                    new Claim("RoleMax", userLogin.RoleMax != null ? userLogin.RoleMax.ToString() : ""),
                                    new Claim("RoleLevel", userLogin.RoleLevel != null ? userLogin.RoleLevel.ToString() : ""),
                                    new Claim("AccessKey", access_key != null ? access_key : ""),
                                    new Claim("ModuleSystem", userLogin.ModuleSystem != null ? userLogin.ModuleSystem.ToString() : "-1")
                                };

                                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AppSettings:JwtKey"]));
                                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                                var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["AppSettings:JwtExpireDays"]));

                                var token = new JwtSecurityToken(
                                    _configuration["AppSettings:JwtIssuer"],
                                    _configuration["AppSettings:JwtIssuer"],
                                    claims,
                                    expires: expires,
                                    signingCredentials: creds
                                );

                                userLoginData.AccessToken = new JwtSecurityTokenHandler().WriteToken(token);
                                userLoginData.BaseUrlImg = _configuration["AppSettings:BaseUrlImg"];
                                userLoginData.BaseUrlFile = "hidden";

                                userLoginData.Password = null;
                                userLoginData.KeyLock = null;
                                userLoginData.RegEmail = null;

                                // save time login
                                userLogin.LastLogin = DateTime.Now;
                                _context.Update(userLogin);

                                //Save AccessToken
                                Token tokenEntity = new Token();
                                tokenEntity.AccessToken = "Bearer " + userLoginData.AccessToken;
                                _context.Tokens.Add(tokenEntity);

                                _context.SaveChanges();
                                def.data = userLoginData;
                                def.meta = new Meta(200, "Đăng nhập thành công!");
                                return Ok(def);
                            }
                            else
                            {
                                def.meta = new Meta(404, "Tài khoản hoặc mật khẩu không chính xác!");
                                return Ok(def);
                            }
                        }
                        else
                        {
                            def.meta = new Meta(404, "Tài khoản hoặc mật khẩu không chính xác!");
                            return Ok(def);
                        }
                    }
                    else
                    {
                        def.meta = new Meta(400, ApiConstants.MessageResource.ERROR_400_MESSAGE);
                        return Ok(def);
                    }
                }
                else
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.ERROR_400_MESSAGE);
                    return Ok(def);
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [HttpPost("loginViaSso")]
        public async Task<ActionResult> LoginViaSso([FromBody] LoginSsoModel loginModel)
        {
            DefaultResponse def = new DefaultResponse();
            try
            {
                if (loginModel != null)
                {
                    if (loginModel.username != null && loginModel.token != null && loginModel.username != "" && loginModel.token != "")
                    {
                        //Xác thực tài khoản trên keycloak
                        string keycloakUrl = _configuration["KeycloakSettings:endpoint"];
                        string keycloakClientId = _configuration["KeycloakSettings:client_id"];
                        string keycloakClientSecret = _configuration["KeycloakSettings:client_secret"];
                        string realm = _configuration["KeycloakSettings:realm"];

                        // Xác thực token
                        var authenticated = await ValidateTokenAsync(keycloakUrl, realm, keycloakClientId, keycloakClientSecret, loginModel.token);
                        if (authenticated.active)
                        {
                            string username = loginModel.username.Trim();

                            if (authenticated.preferred_username.Trim() == username)
                            {
                                Entities.User userLogin = _context.Users.Where(e => e.UserName.Equals(username) && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                if (userLogin != null)
                                {
                                    UserLoginData userLoginData = _mapper.Map<UserLoginData>(userLogin);

                                    if (userLoginData.Status == AppEnums.EntityStatus.LOCK)
                                    {
                                        def.meta = new Meta(223, "Tài khoản bị khóa!");
                                        return Ok(def);
                                    }

                                    Role roleMax = _context.Roles.Where(r => r.Id == userLogin.RoleMax && r.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                    userLoginData.RoleCode = roleMax != null ? roleMax.Code : "";

                                    int userId = userLoginData.Id;
                                    List<MenuData> listFunctionRole = new List<MenuData>();
                                    List<UserRole> userRole = _context.UserRoles.Where(e => e.UserId == userId && e.Status == AppEnums.EntityStatus.NORMAL).ToList();

                                    foreach (var item in userRole)
                                    {
                                        var listFRR = (from fr in _context.FunctionRoles
                                                       join f in _context.Functions on fr.FunctionId equals f.Id
                                                       where fr.TargetId == item.RoleId
                                                          && fr.Type == (int)AppEnums.TypeFunction.FUNCTION_ROLE
                                                          && fr.Status == AppEnums.EntityStatus.NORMAL
                                                          && f.Status == AppEnums.EntityStatus.NORMAL
                                                       select new
                                                       {
                                                           fr.Id,
                                                           fr.FunctionId,
                                                           fr.Type,
                                                           fr.Status,
                                                           fr.ActiveKey,
                                                           f.Location,
                                                           f.Code,
                                                           f.Name,
                                                           f.Url,
                                                           f.Icon,
                                                           f.FunctionParentId,
                                                           f.IsSpecialFunc,
                                                           f.SubSystem
                                                       }).OrderBy(e => e.Location).ToList();

                                        foreach (var itemFR in listFRR)
                                        {
                                            var fr = listFunctionRole.Where(e => e.MenuId == itemFR.FunctionId).ToList();
                                            if (fr.Count > 0)
                                            {
                                                string key1 = fr.FirstOrDefault().ActiveKey;
                                                if (fr.FirstOrDefault().ActiveKey != itemFR.ActiveKey)
                                                {
                                                    key1 = plusActiveKey(fr.FirstOrDefault().ActiveKey, itemFR.ActiveKey);
                                                }
                                                fr.FirstOrDefault().ActiveKey = key1;
                                            }
                                            else
                                            {
                                                Function function = _context.Functions.Where(e => e.Id == itemFR.FunctionId).FirstOrDefault();
                                                if (function != null)
                                                {
                                                    MenuData menu = new MenuData();
                                                    menu.MenuId = itemFR.FunctionId;
                                                    menu.Code = function.Code;
                                                    menu.Name = function.Name;
                                                    menu.Url = function.Url;
                                                    menu.Icon = function.Icon;
                                                    menu.MenuParent = (int)function.FunctionParentId;
                                                    menu.ActiveKey = itemFR.ActiveKey;
                                                    menu.IsSpecialFunc = function.IsSpecialFunc;
                                                    menu.SubSystem = function.SubSystem;
                                                    listFunctionRole.Add(menu);
                                                }
                                            }
                                        }
                                    }

                                    string access_key = "";
                                    int count = listFunctionRole.Count;
                                    if (count > 0)
                                    {
                                        for (int i = 0; i < count - 1; i++)
                                        {
                                            if (listFunctionRole[i].ActiveKey != "000000000")
                                            {
                                                access_key += listFunctionRole[i].Code + ":" + listFunctionRole[i].ActiveKey + "-";
                                            }
                                        }

                                        access_key = access_key + listFunctionRole[count - 1].Code + ":" + listFunctionRole[count - 1].ActiveKey;
                                    }

                                    userLoginData.AccessKey = access_key;
                                    userLoginData.listMenus = CreateMenu(listFunctionRole, 0);
                                    var claims = new List<Claim>
                                {
                                    new Claim(JwtRegisteredClaimNames.Email, userLogin.Email ?? ""),
                                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                    new Claim(ClaimTypes.NameIdentifier, userLogin.Id.ToString()),
                                    new Claim(ClaimTypes.Name, userLogin.FullName),
                                    new Claim("Id", userLogin.Id.ToString()),
                                    new Claim("FullName", userLogin.FullName != null ? userLogin.FullName.ToString() : ""),
                                    new Claim("Phone", userLogin.Phone != null ? userLogin.Phone.ToString() : ""),
                                    new Claim("Avatar", userLogin.Avatar != null ? userLogin.Avatar.ToString() : ""),
                                    new Claim("RoleMax", userLogin.RoleMax != null ? userLogin.RoleMax.ToString() : ""),
                                    new Claim("RoleLevel", userLogin.RoleLevel != null ? userLogin.RoleLevel.ToString() : ""),
                                    new Claim("AccessKey", access_key != null ? access_key : ""),
                                    new Claim("ModuleSystem", userLogin.ModuleSystem != null ? ((int)userLogin.ModuleSystem).ToString() : "-1")
                                };

                                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AppSettings:JwtKey"]));
                                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                                    var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["AppSettings:JwtExpireDays"]));

                                    var token = new JwtSecurityToken(
                                        _configuration["AppSettings:JwtIssuer"],
                                        _configuration["AppSettings:JwtIssuer"],
                                        claims,
                                        expires: expires,
                                        signingCredentials: creds
                                    );

                                    userLoginData.AccessToken = new JwtSecurityTokenHandler().WriteToken(token);
                                    userLoginData.BaseUrlImg = _configuration["AppSettings:BaseUrlImg"];
                                    userLoginData.BaseUrlFile = "hidden";

                                    userLoginData.Password = null;
                                    userLoginData.KeyLock = null;
                                    userLoginData.RegEmail = null;

                                    //Save time login
                                    userLogin.LastLogin = DateTime.Now;
                                    _context.Update(userLogin);

                                    //Save AccessToken
                                    Token tokenEntity = new Token();
                                    tokenEntity.AccessToken = "Bearer " + userLoginData.AccessToken;
                                    _context.Tokens.Add(tokenEntity);

                                    _context.SaveChanges();
                                    def.data = userLoginData;
                                    def.meta = new Meta(200, "Đăng nhập thành công!");
                                    return Ok(def);
                                }
                                else
                                {
                                    def.meta = new Meta(404, "Tài khoản hoặc mật khẩu không chính xác!");
                                    return Ok(def);
                                }

                            }
                            else
                            {
                                def.meta = new Meta(400, "Đăng nhập không thành công. Thông tin tài khoản không hợp lệ!");
                                return Ok(def);
                            }
                        }
                        else
                        {
                            def.meta = new Meta(400, "Đăng nhập không thành công. Thông tin tài khoản không hợp lệ!");
                            return Ok(def);
                        }
                    }
                    else
                    {
                        def.meta = new Meta(400, ApiConstants.MessageResource.ERROR_400_MESSAGE);
                        return Ok(def);
                    }
                }
                else
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.ERROR_400_MESSAGE);
                    return Ok(def);
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        private static async Task<IntrospectionResponse> ValidateTokenAsync(string keycloakUrl, string realm, string keycloakClientId, string keycloakClientSecret, string accessToken)
        {
            using (var client = new HttpClient())
            {
                // Tạo yêu cầu để xác thực token
                var request = new HttpRequestMessage(HttpMethod.Post, keycloakUrl + "/auth/realms" + realm + "/protocol/openid-connect/token/introspect");
                request.Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("token", accessToken),
                    new KeyValuePair<string, string>("client_id", keycloakClientId),
                    new KeyValuePair<string, string>("client_secret", keycloakClientSecret)
                });

                // Gửi yêu cầu và lấy kết quả
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                // Phân tích kết quả
                var introspectionResponse = JObject.Parse(content).ToObject<IntrospectionResponse>();

                return introspectionResponse;
            }
        }


        private string plusActiveKey(string key1, string key2)
        {
            string str = "";
            char[] str1 = key1.ToCharArray();
            char[] str2 = key2.ToCharArray();
            for (int i = 0; i < str1.Length; i++)
            {
                int k = int.Parse(str1[i].ToString()) + int.Parse(str2[i].ToString());
                if (k > 1) k = 1;
                str += k;
            }
            return str;
        }

        private List<MenuData> CreateMenu(List<MenuData> list, int k)
        {
            var listMenu = list.Where(e => e.MenuParent == k).ToList();
            if (listMenu.Count > 0)
            {
                List<MenuData> menus = new List<MenuData>();
                foreach (var item in listMenu)
                {
                    char[] str = item.ActiveKey.ToCharArray();
                    if (int.Parse(str[8].ToString()) == 1)
                    {
                        MenuData menu = new MenuData();
                        menu.MenuId = item.MenuId;
                        menu.Code = item.Code;
                        menu.Name = item.Name;
                        menu.Url = item.Url;
                        menu.Icon = item.Icon;
                        menu.IsSpecialFunc = item.IsSpecialFunc;
                        menu.SubSystem = item.SubSystem;
                        menu.MenuParent = item.MenuParent;
                        menu.ActiveKey = item.ActiveKey;
                        menu.listMenus = CreateMenu(list, item.MenuId);
                        menus.Add(menu);
                    }
                }
                return menus;
            }
            return new List<MenuData>();
        }

        //Người dùng thay đổi mật khẩu
        [Authorize]
        [HttpPut("changePass/{id}")]
        public async Task<ActionResult> ChangePass(int id, [FromBody] ChangePassUserModel input)
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

            if(id != userId)
            {
                def.meta = new Meta(222, "Id tài khoản không hợp lệ!");
                return Ok(def);
            }

            try
            {
                input = (ChangePassUserModel)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                if (input.CurrentPassword == null || input.NewPassword == null || input.CurrentPassword == "" || input.NewPassword == "")
                {
                    def.meta = new Meta(400, "Dữ liệu không hợp lệ!");
                    return Ok(def);
                }

                //Check Password valid
                if (!UtilsService.CheckPasswordValidParttern(input.NewPassword))
                {
                    def.meta = new Meta(400, UtilsService.errMsgPartternPassword);
                    return Ok(def);
                }

                User data = await _context.Users.FindAsync(id);
                if (data == null)
                {
                    def.meta = new Meta(404, "Không tìm thấy tài khoản người dùng!");
                    return Ok(def);
                }

                //check password old
                string password = data.KeyLock.Trim() + data.RegEmail.Trim() + data.Id + input.CurrentPassword.Trim();
                password = UtilsService.GetMD5Hash(password);
                if (data.Password.Trim() != password)
                {
                    def.meta = new Meta(213, "Mật khẩu cũ không chính xác!");
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    string newPassword = data.KeyLock.Trim() + data.RegEmail.Trim() + data.Id + input.NewPassword.Trim();
                    newPassword = UtilsService.GetMD5Hash(newPassword);

                    data.Password = newPassword;
                    data.UpdatedAt = DateTime.Now;
                    data.UpdatedById = userId;
                    data.UpdatedBy = fullName;

                    _context.Update(data);

                    try
                    {
                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Tài khoản " + data.FullName + " đổi mật khẩu", "User", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.UPDATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);

                        await _context.SaveChangesAsync();

                        if (data.Id > 0)
                            transaction.Commit();
                        else
                            transaction.Rollback();

                        def.meta = new Meta(200, "Thay đổi mật khẩu thành công!");
                        def.data = data;
                        return Ok(def);
                    }
                    catch (DbUpdateException e)
                    {
                        transaction.Rollback();
                        log.Error("DbUpdateException:" + e);
                        if (!UserExists(data.Id))
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
                log.Error("Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        //Người dùng thay đổi mật khẩu - đẩy thông tin lên Keycloak
        [Authorize]
        [HttpPut("changePassApplyKeycloak/{id}")]
        public async Task<ActionResult> ChangePassApplyKeycloak(int id, [FromBody] ChangePassUserModel input)
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

            if (id != userId)
            {
                def.meta = new Meta(222, "Id tài khoản không hợp lệ!");
                return Ok(def);
            }

            try
            {
                input = (ChangePassUserModel)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                if (input.CurrentPassword == null || input.NewPassword == null || input.CurrentPassword == "" || input.NewPassword == "")
                {
                    def.meta = new Meta(400, "Dữ liệu không hợp lệ!");
                    return Ok(def);
                }

                User data = await _context.Users.FindAsync(id);
                if (data == null)
                {
                    def.meta = new Meta(404, "Không tìm thấy tài khoản người dùng!");
                    return Ok(def);
                }

                //Check Password valid
                if (!UtilsService.CheckPasswordValidParttern(input.NewPassword))
                {
                    def.meta = new Meta(400, UtilsService.errMsgPartternPassword);
                    return Ok(def);
                }

                //kiểm tra mật khẩu hiện tại có chính xác không
                string keycloakUrl = _configuration["KeycloakSettings:endpoint"];
                string keycloakClientId = _configuration["KeycloakSettings:client_id"];
                string keycloakClientSecret = _configuration["KeycloakSettings:client_secret"];
                string realm = _configuration["KeycloakSettings:realm"];

                var tokenLogin = await GetTokenUserKeyCloak(keycloakUrl, realm, keycloakClientId, keycloakClientSecret, data.UserName, input.CurrentPassword);
                if(tokenLogin == null)
                {
                    def.meta = new Meta(404, "Mật khẩu hiện tại không chính xác!");
                    return Ok(def);
                }    

                using (var transaction = _context.Database.BeginTransaction())
                {
                    if (data.KeyLock == null)
                    {
                        data.KeyLock = UtilsService.RandomString(20);
                        data.RegEmail = data.RegEmail ?? UtilsService.RandomString(20);

                        data.Password = data.KeyLock + data.RegEmail;
                    }

                    data.PasswordKeyCloak = Security.Encrypt(data.KeyLock, input.NewPassword);
                    data.UpdatedAt = DateTime.Now;
                    data.UpdatedById = userId;
                    data.UpdatedBy = fullName;

                    _context.Update(data);

                    try
                    {
                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Tài khoản " + data.FullName + " đổi mật khẩu", "User", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.UPDATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);

                        await _context.SaveChangesAsync();

                        bool success = false;
                        //Đẩy thông tin mật khẩu thay đổi lên Keycloak
                        try
                        {
                            var token = await GetTokenAdminCliKeyCloak(keycloakUrl, realm, keycloakClientId, keycloakClientSecret);
                            if (token != null)
                            {
                                //lấy id keycloak
                                string idKeycloak = await GetAccountIdKeyCloak(keycloakUrl, realm, token, data.UserName);
                                if (idKeycloak != null)
                                {
                                    using (var client = new HttpClient())
                                    {
                                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                        Credential credential = new Credential();
                                        credential.type = "password";
                                        credential.temporary = false;
                                        credential.value = input.NewPassword;

                                        var stringPayload = JsonConvert.SerializeObject(credential);
                                        var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                                        var response = await client.PutAsync(keycloakUrl + "/auth/admin/realms" + realm + "/users/" + idKeycloak + "/reset-password", httpContent);

                                        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                                        {
                                            success = true;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error("Push Info Keycloak Faild:" + ex);
                        }

                        if (success)
                        {
                            transaction.Commit();
                            def.meta = new Meta(200, "Đổi mật khẩu thành công!");
                            def.data = data;
                            return Ok(def);
                        }
                        else
                        {
                            transaction.Rollback();
                            def.meta = new Meta(500, "Đẩy thông tin Keycloak thất bại. Đổi mật khẩu không thành công!");
                            def.data = data;
                            return Ok(def);
                        }
                    }
                    catch (DbUpdateException e)
                    {
                        transaction.Rollback();
                        log.Error("DbUpdateException:" + e);
                        if (!UserExists(data.Id))
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
                log.Error("Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [Authorize]
        [HttpGet("getInfo/{id}")]
        public async Task<IActionResult> GetInfo(int id)
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
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            if (userId != id)
            {
                def.meta = new Meta(222, "Không có quyền lấy thông tin!");
                return Ok(def);
            }

            try
            {
                User data = await _context.Users.FindAsync(id);

                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                    return Ok(def);
                }

                data.RegEmail = null;
                data.KeyLock = null;
                data.Password = null;

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = data;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("GetByPage Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [Authorize]
        [HttpPut("changeInfoUser/{id}")]
        public async Task<ActionResult> ChangeInfoUser(int id, [FromBody] User input)
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

            if (id != userId)
            {
                def.meta = new Meta(222, "Id tài khoản không hợp lệ!");
                return Ok(def);
            }

            try
            {
                input = (User)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                User data = await _context.Users.FindAsync(id);
                if (data == null)
                {
                    def.meta = new Meta(404, "Không tìm thấy tài khoản người dùng!");
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    data.FullName = input.FullName;
                    data.Dob = input.Dob;
                    data.Phone = input.Phone;
                    data.Email = input.Email;
                    data.Address = input.Address;
                    data.Avatar = input.Avatar;
                    data.UpdatedAt = DateTime.Now;
                    data.UpdatedById = userId;
                    data.UpdatedBy = fullName;

                    _context.Update(data);

                    try
                    {
                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Tài khoản " + data.FullName + " thay đổi thông tin", "User", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.UPDATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);

                        await _context.SaveChangesAsync();

                        if (data.Id > 0)
                            transaction.Commit();
                        else
                            transaction.Rollback();

                        def.meta = new Meta(200, "Thay đổi thông tin tài khoản thành công!");
                        def.data = data;
                        return Ok(def);
                    }
                    catch (DbUpdateException e)
                    {
                        transaction.Rollback();
                        log.Error("DbUpdateException:" + e);
                        if (!UserExists(data.Id))
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
                log.Error("Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> logout()
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Token token = await (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefaultAsync();
            if (token != null)
            {
                _context.Tokens.Remove(token);
                _context.SaveChanges();
            }

            return NoContent();
        }

        #endregion

        //get token admin_cli keycloak
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
            catch(Exception ex)
            {
                log.Error("GetTokenAdminCliKeyCloak Function:" + ex);
                log.Error("Msg:" + ex.Message);
                return null;
            }

        }

        //get token user
        private static async Task<string> GetTokenUserKeyCloak(string keycloakUrl, string realm, string keycloakClientId, string keycloakClientSecret, string userName, string password)
        {
            using (var client = new HttpClient())
            {
                // Tạo yêu cầu để xác thực token
                var request = new HttpRequestMessage(HttpMethod.Post, keycloakUrl + "/auth/realms" + realm + "/protocol/openid-connect/token");
                request.Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("client_id", keycloakClientId),
                    new KeyValuePair<string, string>("client_secret", keycloakClientSecret),
                    new KeyValuePair<string, string>("username", userName),
                    new KeyValuePair<string, string>("password", password)
                });

                // Gửi yêu cầu và lấy kết quả
                var response = await client.SendAsync(request);
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

        //get Id account Keycloak
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

        #region export excel
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

            List<User> users = _context.Users.Where(p => p.Status != AppEnums.EntityStatus.DELETED).ToList();
            List<UserDataExport> data = _mapper.Map<List<UserDataExport>>(users.ToList());
            foreach (UserDataExport item in data)
            {
                List<UserRole> userRoles = _context.UserRoles.Where(e => e.UserId == item.Id && e.Status != AppEnums.EntityStatus.DELETED).ToList();
                userRoles.ForEach(userRole => {
                    Role role = _context.Roles.Where(r => r.Id == userRole.RoleId && r.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    if(role != null)
                    {
                        item.Role = item.Role == null || item.Role == "" ? role.Name : item.Role + ", " + role.Name;
                    }
                });

            }

            // khởi tạo wb rỗng
            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"templates/tai_khoan_nguoi_dung.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);

            MemoryStream ms = WriteDataToExcel(templatePath, 0, data);
            byte[] byteArrayContent = ms.ToArray();
            return new FileContentResult(byteArrayContent, "application/octet-stream");
        }

        private static MemoryStream WriteDataToExcel(string templatePath, int sheetnumber, List<UserDataExport> data)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            int rowStart = 1;

            if (sheet != null)
            {
                int datacol = 8;
                try
                {
                    //style body
                    List<ICellStyle> rowStyle = new List<ICellStyle>();
                    for (int i = 0; i < datacol; i++)
                    {
                        rowStyle.Add(sheet.GetRow(rowStart).GetCell(i).CellStyle);
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
                                    row.GetCell(i).SetCellValue(item.FullName);
                                }
                                else if (i == 2)
                                {
                                    row.GetCell(i).SetCellValue(item.UserName);
                                }
                                else if (i == 3)
                                {
                                    row.GetCell(i).SetCellValue(item.Phone);
                                }
                                else if (i == 4)
                                {
                                    row.GetCell(i).SetCellValue(item.Email);
                                }
                                else if (i == 5)
                                {
                                    row.GetCell(i).SetCellValue(item.Dob != null ? item.Dob.Value.ToString("dd/MM/yyyy") : "");
                                }
                                else if (i == 6)
                                {
                                    row.GetCell(i).SetCellValue(item.Address);
                                }
                                else if (i == 7)
                                {
                                    row.GetCell(i).SetCellValue(item.Role);
                                }
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

        #region SignalR Notify
        [HttpPost("SignalRNotify/{userId}")]
        public async Task<IActionResult> SignalRNotify(string userId)
        {
            DefaultResponse def = new DefaultResponse();

            try
            {
                SignalRNotify signalRNotify = new SignalRNotify("Lorem", "Input type submit!!!", TypeSignalRNotify.NOTIFY);
                await _hubContext.Clients.Group(userId).BroadcastMessage(signalRNotify);

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = null;
                return Ok(def);

            }
            catch (Exception e)
            {
                log.Error("Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }
        #endregion

        #region TEST FTS
        [Authorize]
        [HttpGet("TestFts")]
        public async Task<IActionResult> TestFts([FromQuery] string txt)
        {
            DefaultResponse def = new DefaultResponse();

            try
            {
                var arrTxt = System.Text.RegularExpressions.Regex.Split(txt.Trim(), @"\s+").Select(x => x.Trim());
                var txtQuery = String.Join(" AND ", arrTxt);

                List<User> data = await _context.Users.Where(e => EF.Functions.Contains(e.FullName, txtQuery)).ToListAsync();

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = data;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("TestFts Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }
        #endregion
    }
}
