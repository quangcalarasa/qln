using  IOITQln.Common.Constants;
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
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using Microsoft.AspNetCore.Http;
using IOITQln.QuickPriceNOC.Interface;
using System.Data;
using IOITQln.Common.Interfaces;

namespace IOITQln.Controllers.ApiNoc
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RentFileController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("rentFile", "rentFile");
        private static string functionCode = "RENT_FILE";
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;
        private IDapper _dapper;

        public RentFileController(ApiDbContext context, IMapper mapper, IWebHostEnvironment hostingEnvironment, IConfiguration configuration, IDapper dapper)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
            _dapper = dapper;
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
            int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id").Select(c => c.Value).SingleOrDefault());
            int moduleSystem = int.Parse(identity.Claims.Where(c => c.Type == "ModuleSystem").Select(c => c.Value).SingleOrDefault());

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
                    IQueryable<RentFile> data;

                    if (moduleSystem == (int)ModuleSystem.NOC)
                    {
                        data = (from r in _context.RentFiles
                                join wm in _context.WardManagements on r.WardId equals wm.WardId
                                where r.Status != EntityStatus.DELETED
                                    && wm.Status != EntityStatus.DELETED
                                    && wm.UserId == userId
                                select r).Distinct().AsQueryable();
                    }
                    else
                    {
                        data = _context.RentFiles.Where(c => c.Status != AppEnums.EntityStatus.DELETED);
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
                        List<RentFlieData> res = _mapper.Map<List<RentFlieData>>(data.ToList());
                        foreach (RentFlieData item in res)
                        {
                            item.Address = _context.Blocks.Where(f => f.Id == item.RentBlockId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Address).FirstOrDefault();

                            item.memberRentFiles = _context.MemberRentFiles.Where(f => f.RentFileId == item.Id && f.Status != AppEnums.EntityStatus.DELETED).ToList();

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
        public async Task<IActionResult> GetById(Guid id)
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
                RentFile data =  _context.RentFiles.Where(p => p.Id == id && p.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                    return Ok(def);
                }
                RentFlieData res = _mapper.Map<RentFlieData>(data);

                res.CustomerName = _context.Customers.Where(f => f.Id == res.CustomerId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.FullName).FirstOrDefault();

                res.Address = _context.Blocks.Where(f => f.Id == res.RentBlockId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Address).FirstOrDefault();

                res.memberRentFiles = _context.MemberRentFiles.Where(f => f.RentFileId == res.Id && f.Status != AppEnums.EntityStatus.DELETED).ToList();

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
        public async Task<IActionResult> Post([FromBody] RentFlieData input)
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
                input = (RentFlieData)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }
                if (input.Type == 1)
                {
                    RentFile RentfileCodeExits = _context.RentFiles.Where(l => l.CodeHS == input.CodeHS && l.Type == 1 & l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    if (RentfileCodeExits != null)
                    {
                        def.meta = new Meta(400, "Đã tồn tại mã hồ sơ" + input.CodeHS + "!!!!");
                        return Ok(def);
                    }
                }
                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.Id = Guid.NewGuid();
                    input.CreatedById = userId;
                    input.CreatedBy = fullName;
                    input.UpdatedById = userId;
                    input.UpdatedBy = fullName;
                    _context.RentFiles.Add(input);
                    try
                    {
                        await _context.SaveChangesAsync();

                        if (input.memberRentFiles != null)
                        {
                            foreach (var memberRentFile in input.memberRentFiles)
                            {
                                memberRentFile.RentFileId = input.Id;
                                memberRentFile.CreatedById = userId;
                                memberRentFile.CreatedBy = fullName;

                                _context.MemberRentFiles.Add(memberRentFile);
                            }
                            await _context.SaveChangesAsync();
                        }
                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Thêm mới hồ sơ cho thuê: " + input.Id, "renFile", 0, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (input.Id != null)
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
                        if (RentFileExists(input.Id))
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
        public async Task<IActionResult> Put(Guid id, [FromBody] RentFlieData input)
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
                input = (RentFlieData)UtilsService.TrimStringPropertyTypeObject(input);

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

                RentFile data = await _context.RentFiles.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
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

                        List<MemberRentFile> lstMemberRentFileAdd = new List<MemberRentFile>();
                        List<MemberRentFile> lstMemberRentFilelUpdate = new List<MemberRentFile>();

                        List<MemberRentFile> memberRents = _context.MemberRentFiles.AsNoTracking().Where(l => l.RentFileId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();

                        if (input.memberRentFiles != null)
                        {
                            foreach (var memberRentFiles in input.memberRentFiles)
                            {
                                MemberRentFile memberRentFileExist = memberRents.Where(l => l.Id == memberRentFiles.Id).FirstOrDefault();
                                if (memberRentFileExist == null)
                                {
                                    memberRentFiles.RentFileId = input.Id;
                                    memberRentFiles.CreatedBy = fullName;
                                    memberRentFiles.CreatedById = userId;

                                    lstMemberRentFileAdd.Add(memberRentFiles);
                                }
                                else
                                {
                                    memberRentFiles.CreatedAt = memberRentFiles.CreatedAt;
                                    memberRentFiles.CreatedBy = memberRentFiles.CreatedBy;
                                    memberRentFiles.CreatedById = memberRentFiles.UpdatedById;
                                    memberRentFiles.RentFileId = input.Id;
                                    memberRentFiles.UpdatedById = userId;
                                    memberRentFiles.UpdatedBy = fullName;

                                    lstMemberRentFilelUpdate.Add(memberRentFiles);
                                    memberRents.Remove(memberRentFileExist);
                                }
                            }
                        }
                        foreach (var itemmemberRentsFile in memberRents)
                        {
                            itemmemberRentsFile.UpdatedAt = DateTime.Now;
                            itemmemberRentsFile.UpdatedById = userId;
                            itemmemberRentsFile.UpdatedBy = fullName;
                            itemmemberRentsFile.Status = AppEnums.EntityStatus.DELETED;

                            lstMemberRentFilelUpdate.Add(itemmemberRentsFile);
                        }
                        _context.MemberRentFiles.UpdateRange(lstMemberRentFilelUpdate);
                        _context.MemberRentFiles.AddRange(lstMemberRentFileAdd);

                        if (input.editHistory != null)
                        {
                            input.editHistory.RentFileId = id;
                            input.editHistory.CreatedById = userId;
                            input.editHistory.CreatedBy = fullName;
                            input.editHistory.TypeEditHistory = TypeEditHistory.RENT_CONTRACT;

                            _context.EditHistories.Add(input.editHistory);
                        }

                        ////thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Sửa hồ sơ cho thuê: " + input.Id, "renFile", 0, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (input.Id != null)
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
                        if (!RentFileExists(data.Id))
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
        public async Task<IActionResult> Delete(Guid id)
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
                RentFile data = await _context.RentFiles.FindAsync(id);
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

                    List<RentFileBCT> rentFileBCTs = _context.RentFileBCTs.Where(l => l.RentFileId == id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if (rentFileBCTs.Count > 0)
                    {
                        rentFileBCTs.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(rentFileBCTs);
                    }
                    List<MemberRentFile> memberRentFiles = _context.MemberRentFiles.Where(l => l.RentFileId == id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if (memberRentFiles.Count > 0)
                    {
                        memberRentFiles.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(memberRentFiles);
                    }
                    List<DebtsTable> debtsTables = _context.DebtsTables.Where(l => l.RentFileId == id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if (debtsTables != null)
                    {
                        debtsTables.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(debtsTables);
                    }
                    List<Debts> debts = _context.debts.Where(l => l.RentFileId == id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if (debts != null)
                    {
                        debts.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(debts);
                    }
                    try
                    {
                        await _context.SaveChangesAsync();

                        ////thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Xóa hồ sơ cho thuê: " + data.Id, "rentFile", 0, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.DELETED, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (data.Id != null)
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
                        if (!RentFileExists(data.Id))
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
        private bool RentFileExists(Guid id)
        {
            return _context.RentFiles.Count(e => e.Id == id) > 0;
        }

        [HttpGet("GroupDataRentFile/{Code}")]
        public IActionResult GroupDataRentFile(string Code)
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
                List<RentFile> data = _context.RentFiles.Where(l => l.Status != AppEnums.EntityStatus.DELETED && l.Code == Code).ToList();
                var groupData = data.GroupBy(x => x.Code).Select(e => new groupDataByCode
                {
                    Code = e.Key,
                    GroupByCode = e.ToList()
                });

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = groupData;
                return Ok(def);
            }
            catch
            {
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        #region import excel phiếu thu

        [HttpPost]
        [Route("ImportReceiptDataExcelOld")]
        public IActionResult ImportDataExcelOld()
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
                importHistory.Type = AppEnums.ImportHistoryType.Noc_Contract_Rent_Receipt;

                List<PromissoryDataImport> data = new List<PromissoryDataImport>();

                //Lấy dữ liệu từ file excel và lưu lại file
                foreach (var file in httpRequest)
                {
                    var postedFile = httpRequest[i];
                    if (postedFile != null && postedFile.Length > 0)
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
                            string folderName = _configuration["AppSettings:BaseUrlImportHistory"];
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
								//Qfix 
                            // Đọc trực tiếp từ file đã lưu để tránh cấp phát mảng byte lớn trong bộ nhớ
                            using (var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                            using (var ms = new MemoryStream())
                            {
                                fs.CopyTo(ms);
                                ms.Position = 0;
                                data = importData(ms, 0, 1);
                            }
                        }
                    }
                    i++;
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        List<RentFile> dataRentFile = _context.RentFiles.Where(e => e.Status != EntityStatus.DELETED).ToList();
                        List<NocReceipt> promissorys = _context.NocReceipts.Where(e => e.Status != EntityStatus.DELETED).ToList();
                        List<Debts> debts = _context.debts.Where(e => e.Status != EntityStatus.DELETED).ToList();
                        List<DebtsTable> dataDebtsTables = _context.DebtsTables.Where(e => e.Status != EntityStatus.DELETED).ToList();

                        data.ForEach(item =>
                        {
                            if (item.Valid)
                            {
                                //Kiểm tra xem Mã định danh có căn hộ tương ứng và có hợp đồng thuê nhà cho căn hộ này không
                                RentFile rentFile = dataRentFile.Where(e => e.CodeCN == item.Code || e.CodeCH == item.Code).FirstOrDefault();
                                if (rentFile == null)
                                {
                                    item.Valid = false;
                                    item.ErrMsg += "Không tìm thấy hợp đồng tương ứng \n";
                                }
                                else
                                {
                                    //Kiểm tra phiếu thu đã tồn tại chưa
                                    NocReceipt promissory = promissorys.Where(e => e.Number == item.Number).FirstOrDefault();
                                    if (promissory != null)
                                    {
                                        item.Valid = false;
                                        item.ErrMsg += "Phiếu thu đã tồn tại \n";
                                    }
                                    else
                                    {
                                        NocReceipt newPromissory = item;
                                        newPromissory.Action = 1;
                                        newPromissory.CreatedById = userId;
                                        newPromissory.CreatedBy = fullName + "(Excel)";
                                        newPromissory.Executor = fullName;
                                        newPromissory.IsImportExcel = true;

                                        _context.NocReceipts.Add(newPromissory);
                                        _context.SaveChanges();

                                        //Update vào số dư treo
                                        Debts debt = debts.Where(e => e.Code == newPromissory.Code).FirstOrDefault();
                                        if (debt != null)
                                        {
                                            //Ghi dấu check đã nộp cho những lần nào trong bảng công nợ theo thứ tự ưu tiên ngày ký gần nhất và phụ lục => hợp đồng => truy thu
                                            List<DebtsTable> debtsTables = dataDebtsTables.Where(e => e.Code == item.Code).ToList();
                                            List<RentFile> rentFiles = dataRentFile.Where(e => e.CodeCN == item.Code || e.CodeCH == item.Code).ToList();
                                            decimal surplusBalance = (debt.SurplusBalance ?? 0) + newPromissory.Price;

                                            List<MonthYearPromissory> monthYearPromissory = new List<MonthYearPromissory>();
                                            debt.SurplusBalance = updateDebtTable(debtsTables, rentFiles, monthYearPromissory, surplusBalance, newPromissory.Id, fullName, item.Date);
                                            debt.Paid = (debt.Paid ?? 0) + (surplusBalance - debt.SurplusBalance);
                                            debt.Diff = debt.Total - debt.Paid;

                                            if (monthYearPromissory.Count > 0)
                                            {
                                                var groupData = monthYearPromissory.GroupBy(x => x.Year).OrderBy(x => x.Key).ToList();
                                                groupData.ForEach(groupDataItem =>
                                                {
                                                    var exist = groupDataItem.Where(x => x.Month == 12).FirstOrDefault();
                                                    if (exist != null)
                                                    {
                                                        newPromissory.Content = newPromissory.Content == null ? $"Thanh toán hoàn tất phần nợ tiền thuê nhà năm {groupDataItem.Key}" : newPromissory.Content + $" và Thanh toán hoàn tất phần nợ tiền thuê nhà năm {groupDataItem.Key}";
                                                    }
                                                    else
                                                    {
                                                        newPromissory.Content = newPromissory.Content == null ? $"Thanh toán 1 phần nợ tiền thuê nhà năm {groupDataItem.Key}" : newPromissory.Content + $" và Thanh toán 1 phần nợ tiền thuê nhà năm {groupDataItem.Key}";
                                                    }
                                                });

                                                _context.Update(newPromissory);
                                            }

                                            _context.Update(debt);
                                            _context.UpdateRange(debtsTables);
                                            _context.SaveChanges();
                                        }
                                    }
                                }
                            }
                        });


                        // Lưu toàn bộ dữ liệu import (bao gồm cả bản ghi lỗi) vào lịch sử
                        importHistory.Data = data.Cast<dynamic>().ToList();

                        // Ghi log các bản ghi không import được ra file để tra soát
                        try
                        {
                            var failed = data.Where(x => x.Valid == false).ToList();
                            log.Error("Data failed: " + JsonConvert.SerializeObject(failed));
                            if (failed != null && failed.Count > 0)
                            {
                                string folderName = _configuration["AppSettings:BaseUrlImportHistory"];
                                string webRootPath = _hostingEnvironment.WebRootPath;
                                string newPath = Path.Combine(webRootPath, folderName);
                                if (!Directory.Exists(newPath))
                                {
                                    Directory.CreateDirectory(newPath);
                                }

                                string errorsFileName = $"{Path.GetFileNameWithoutExtension(importHistory.FileUrl)}_errors_{DateTime.Now:yyyyMMddHHmmssfff}.csv";
                                string errorsFullPath = Path.Combine(newPath, errorsFileName);

                                using (var writer = new StreamWriter(errorsFullPath, false, System.Text.Encoding.UTF8))
                                {
                                    // Ghi header cơ bản; dùng reflection để ghi tất cả cột sẵn có của model + ErrMsg
                                    var props = typeof(PromissoryDataImport).GetProperties();
                                    writer.WriteLine(string.Join(",", props.Select(p => p.Name)));

                                    foreach (var item in failed)
                                    {
                                        var values = props.Select(p =>
                                        {
                                            var val = p.GetValue(item);
                                            if (val == null) return string.Empty;
                                            var s = val.ToString();
                                            // Escape dấu phẩy và xuống dòng trong CSV
                                            if (s.Contains('"') || s.Contains(',') || s.Contains('\n') || s.Contains('\r'))
                                            {
                                                s = '"' + s.Replace("\"", "\"\"") + '"';
                                            }
                                            return s;
                                        });
                                        writer.WriteLine(string.Join(",", values));
                                    }
                                }

                                // Trả thông tin file log lỗi về cho client
                                if (def.data == null)
                                {
                                    def.data = new { FailedCount = failed.Count, FailedLog = Path.Combine(folderName, errorsFileName) };
                                }
                                else
                                {
                                    def.data = new { def.data, FailedCount = failed.Count, FailedLog = Path.Combine(folderName, errorsFileName) };
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error("WriteFailedImportLog:" + ex);
                        }
                        importHistory.CreatedById = userId;
                        importHistory.CreatedBy = fullName;

                        _context.ImportHistories.Add(importHistory);
                        _context.SaveChanges();

                        transaction.Commit();
                        def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
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

        [HttpPost]
        [Route("ImportReceiptDataExcel")]
        public IActionResult ImportDataExcel()
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
                importHistory.Type = AppEnums.ImportHistoryType.Noc_Contract_Rent_Receipt;

                List<PromissoryDataImport> data = new List<PromissoryDataImport>();

                //Lấy dữ liệu từ file excel và lưu lại file
                foreach (var file in httpRequest)
                {
                    var postedFile = httpRequest[i];
                    if (postedFile != null && postedFile.Length > 0)
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
                            string folderName = _configuration["AppSettings:BaseUrlImportHistory"];
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
                                    data = importData(ms, 0, 1);
                                }
                            }
                        }
                    }
                    i++;
                }

                try
                {
                    //List<RentFile> dataRentFile = _context.RentFiles.Where(e => e.Status != EntityStatus.DELETED).ToList();
                    //List<NocReceipt> promissorys = _context.NocReceipts.Where(e => e.Status != EntityStatus.DELETED).ToList();
                    //List<Debts> debts = _context.debts.Where(e => e.Status != EntityStatus.DELETED).ToList();
                    //List<DebtsTable> dataDebtsTables = _context.DebtsTables.Where(e => e.Status != EntityStatus.DELETED).ToList();

                    List<RentFile> dataRentFile = _dapper.GetAll<RentFile>($"SELECT * FROM RentFile WHERE Status != {(int)EntityStatus.DELETED}", null, commandType: CommandType.Text);
                    List<NocReceipt> promissorys = _dapper.GetAll<NocReceipt>($"SELECT * FROM NocReceipt WHERE Status != {(int)EntityStatus.DELETED}", null, commandType: CommandType.Text);
                    List<Debts> debts = _dapper.GetAll<Debts>($"SELECT * FROM Debts WHERE Status != {(int)EntityStatus.DELETED}", null, commandType: CommandType.Text);
                    List<DebtsTable> dataDebtsTables = _dapper.GetAll<DebtsTable>($"SELECT * FROM DebtsTable WHERE Status != {(int)EntityStatus.DELETED}", null, commandType: CommandType.Text);

                    // Tối ưu tra cứu bằng cấu trúc dữ liệu phù hợp
                    var promissoryNumberSet = new HashSet<string>(promissorys.Select(p => p.Number));
                    var debtsByCode = debts.GroupBy(d => d.Code).ToDictionary(g => g.Key, g => g.First());
                    var debtsTablesByCode = dataDebtsTables.GroupBy(d => d.Code).ToDictionary(g => g.Key, g => g.ToList());
                    // Mã định danh có thể nằm ở CodeCN hoặc CodeCH → lập map từ code → danh sách RentFile
                    var rentFilesByCode = new Dictionary<string, List<RentFile>>();
                    foreach (var rf in dataRentFile)
                    {
                        if (!string.IsNullOrWhiteSpace(rf.CodeCN))
                        {
                            if (!rentFilesByCode.TryGetValue(rf.CodeCN, out var list)) { list = new List<RentFile>(); rentFilesByCode[rf.CodeCN] = list; }
                            list.Add(rf);
                        }
                        if (!string.IsNullOrWhiteSpace(rf.CodeCH))
                        {
                            if (!rentFilesByCode.TryGetValue(rf.CodeCH, out var list2)) { list2 = new List<RentFile>(); rentFilesByCode[rf.CodeCH] = list2; }
                            list2.Add(rf);
                        }
                    }

                    List<NocReceipt> newPromissorys = new List<NocReceipt>();

                    string timespan = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                    data.ForEach(item =>
                    {
                        if (item.Valid)
                        {
                            //Kiểm tra xem Mã định danh có căn hộ tương ứng và có hợp đồng thuê nhà cho căn hộ này không
                            RentFile rentFile = null;
                            if (!string.IsNullOrWhiteSpace(item.Code) && rentFilesByCode.TryGetValue(item.Code, out var rfList))
                            {
                                rentFile = rfList.FirstOrDefault();
                            }
                            if (rentFile == null)
                            {
                                item.Valid = false;
                                item.ErrMsg += "Không tìm thấy hợp đồng tương ứng \n";
                            }
                            else
                            {
                                //Kiểm tra phiếu thu đã tồn tại chưa
                                if (promissoryNumberSet.Contains(item.Number))
                                {
                                    item.Valid = false;
                                    item.ErrMsg += "Phiếu thu đã tồn tại \n";
                                }
                                else
                                {
                                    NocReceipt newPromissory = item;
                                    newPromissory.Action = 1;
                                    newPromissory.CreatedById = userId;
                                    newPromissory.CreatedBy = fullName + "(Excel)";
                                    newPromissory.Executor = fullName;
                                    newPromissory.IsImportExcel = true;
                                    newPromissory.Id = Guid.NewGuid();

                                    //_context.NocReceipts.Add(newPromissory);
                                    //_context.SaveChanges();

                                    //Update vào số dư treo
                                    Debts debt = null;
                                    if (!string.IsNullOrWhiteSpace(newPromissory.Code))
                                        debtsByCode.TryGetValue(newPromissory.Code, out debt);
                                    if (debt != null)
                                    {
                                        //Ghi dấu check đã nộp cho những lần nào trong bảng công nợ theo thứ tự ưu tiên ngày ký gần nhất và phụ lục => hợp đồng => truy thu
                                        List<DebtsTable> debtsTables = debtsTablesByCode.TryGetValue(item.Code, out var dtList) ? dtList : new List<DebtsTable>();
                                        List<RentFile> rentFiles = rentFilesByCode.TryGetValue(item.Code, out var rfList2) ? rfList2 : new List<RentFile>();
                                        decimal surplusBalance = (debt.SurplusBalance ?? 0) + newPromissory.Price;

                                        List<MonthYearPromissory> monthYearPromissory = new List<MonthYearPromissory>();
                                        debt.SurplusBalance = updateDebtTable(debtsTables, rentFiles, monthYearPromissory, surplusBalance, newPromissory.Id, fullName, item.Date);
                                        debt.Paid = (debt.Paid ?? 0) + (surplusBalance - debt.SurplusBalance);
                                        debt.Diff = debt.Total - debt.Paid;

                                        if (monthYearPromissory.Count > 0)
                                        {
                                            var groupData = monthYearPromissory.GroupBy(x => x.Year).OrderBy(x => x.Key).ToList();
                                            groupData.ForEach(groupDataItem =>
                                            {
                                                var exist = groupDataItem.Where(x => x.Month == 12).FirstOrDefault();
                                                if (exist != null)
                                                {
                                                    newPromissory.Content = newPromissory.Content == null ? $"Thanh toán hoàn tất phần nợ tiền thuê nhà năm {groupDataItem.Key}" : newPromissory.Content + $" và Thanh toán hoàn tất phần nợ tiền thuê nhà năm {groupDataItem.Key}";
                                                }
                                                else
                                                {
                                                    newPromissory.Content = newPromissory.Content == null ? $"Thanh toán 1 phần nợ tiền thuê nhà năm {groupDataItem.Key}" : newPromissory.Content + $" và Thanh toán 1 phần nợ tiền thuê nhà năm {groupDataItem.Key}";
                                                }
                                            });

                                            //_context.Update(newPromissory);
                                        }


                                        //_context.Update(debt);
                                        debt.UpdatedBy = timespan;

                                        Parallel.ForEach(debtsTables, debtsTable => {
                                            debtsTable.UpdatedBy = timespan;
                                        });

                                        //_context.UpdateRange(debtsTables);
                                        //_context.SaveChanges();
                                    }

                                    newPromissorys.Add(newPromissory);
                                }
                            }
                        }
                    });
                    // Lưu toàn bộ dữ liệu import (bao gồm cả bản ghi lỗi) vào lịch sử
                        importHistory.Data = data.Cast<dynamic>().ToList();

                        // Ghi log các bản ghi không import được ra file để tra soát
                        try
                        {
                            var failed = data.Where(x => x.Valid == false).ToList();
                            log.Error("Data failed: " + JsonConvert.SerializeObject(failed));
                            if (failed != null && failed.Count > 0)
                            {
                                string folderName = _configuration["AppSettings:BaseUrlImportHistory"];
                                string webRootPath = _hostingEnvironment.WebRootPath;
                                string newPath = Path.Combine(webRootPath, folderName);
                                if (!Directory.Exists(newPath))
                                {
                                    Directory.CreateDirectory(newPath);
                                }

                                string errorsFileName = $"{Path.GetFileNameWithoutExtension(importHistory.FileUrl)}_errors_{DateTime.Now:yyyyMMddHHmmssfff}.csv";
                                string errorsFullPath = Path.Combine(newPath, errorsFileName);

                                using (var writer = new StreamWriter(errorsFullPath, false, System.Text.Encoding.UTF8))
                                {
                                    // Ghi header cơ bản; dùng reflection để ghi tất cả cột sẵn có của model + ErrMsg
                                    var props = typeof(PromissoryDataImport).GetProperties();
                                    writer.WriteLine(string.Join(",", props.Select(p => p.Name)));

                                    foreach (var item in failed)
                                    {
                                        var values = props.Select(p =>
                                        {
                                            var val = p.GetValue(item);
                                            if (val == null) return string.Empty;
                                            var s = val.ToString();
                                            // Escape dấu phẩy và xuống dòng trong CSV
                                            if (s.Contains('"') || s.Contains(',') || s.Contains('\n') || s.Contains('\r'))
                                            {
                                                s = '"' + s.Replace("\"", "\"\"") + '"';
                                            }
                                            return s;
                                        });
                                        writer.WriteLine(string.Join(",", values));
                                    }
                                }

                                // Trả thông tin file log lỗi về cho client
                                if (def.data == null)
                                {
                                    def.data = new { FailedCount = failed.Count, FailedLog = Path.Combine(folderName, errorsFileName) };
                                }
                                else
                                {
                                    def.data = new { def.data, FailedCount = failed.Count, FailedLog = Path.Combine(folderName, errorsFileName) };
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error("WriteFailedImportLog:" + ex);
                        }

                    //_context.NocReceipts.AddRange(newPromissorys);

                    var debtsUpdateData = debts.Where(e => e.UpdatedBy == timespan).ToList();
                    //_context.debts.UpdateRange(debtsUpdateData);

                    var dataUpdateDebtsTables = dataDebtsTables.Where(e => e.UpdatedBy == timespan).ToList();
                    //_context.DebtsTables.UpdateRange(dataUpdateDebtsTables);

                    importHistory.Data = data.Cast<dynamic>().ToList();
                    importHistory.CreatedById = userId;
                    importHistory.CreatedBy = fullName;

                    //_context.ImportHistories.Add(importHistory);
                    //_context.SaveChanges();

                    //Call dapper
                    bool resSqlCmd = _dapper.ImportNocReceipt(newPromissorys, debtsUpdateData, dataUpdateDebtsTables, importHistory);

                    if(resSqlCmd == true)
                    {
                        // Ghi log các bản ghi đã import thành công
                        try
                        {
                            var successful = data.Where(x => x.Valid != false).ToList();
                            log.Info($"ImportDataExcel - Successfully imported {successful.Count} records");
                            
                            if (successful != null && successful.Count > 0)
                            {
                                // Log chi tiết từng bản ghi thành công
                                foreach (var record in successful)
                                {
                                    log.Info($"ImportDataExcel - Successfully imported record: Code={record.Code}, Number={record.Number}, Price={record.Price}, Date={record.Date}, CreatedBy={fullName}");
                                }
                                
                                // Tạo file log cho các bản ghi thành công (tùy chọn)
                                string folderName = _configuration["AppSettings:BaseUrlImportHistory"];
                                string webRootPath = _hostingEnvironment.WebRootPath;
                                string newPath = Path.Combine(webRootPath, folderName);
                                if (!Directory.Exists(newPath))
                                {
                                    Directory.CreateDirectory(newPath);
                                }

                                string successFileName = $"{Path.GetFileNameWithoutExtension(importHistory.FileUrl)}_success_{DateTime.Now:yyyyMMddHHmmssfff}.csv";
                                string successFullPath = Path.Combine(newPath, successFileName);

                                using (var writer = new StreamWriter(successFullPath, false, System.Text.Encoding.UTF8))
                                {
                                    // Ghi header cho file success
                                    var props = typeof(PromissoryDataImport).GetProperties();
                                    writer.WriteLine(string.Join(",", props.Select(p => p.Name)));

                                    foreach (var item in successful)
                                    {
                                        var values = props.Select(p =>
                                        {
                                            var val = p.GetValue(item);
                                            if (val == null) return string.Empty;
                                            var s = val.ToString();
                                            // Escape dấu phẩy và xuống dòng trong CSV
                                            if (s.Contains('"') || s.Contains(',') || s.Contains('\n') || s.Contains('\r'))
                                            {
                                                s = '"' + s.Replace("\"", "\"\"") + '"';
                                            }
                                            return s;
                                        });
                                        writer.WriteLine(string.Join(",", values));
                                    }
                                }
                                
                                log.Info($"ImportDataExcel - Success log file created: {successFileName}");
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error("WriteSuccessImportLog:" + ex);
                        }

                        def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                        def.metadata = data.Count;
                        def.data = data;
                    }
                    else
                    {
                        def.meta = new Meta(500, "SQL command call via Dapper Failure!");
                    }
                }
                catch (Exception ex)
                {
                    log.Error("ImportDataExcelNew:" + ex);
                    log.Error("ImportDataExcelNew:" + ex.Message);

                    def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
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

        public static decimal updateDebtTable(List<DebtsTable> debtsTables, List<RentFile> rentFiles, List<MonthYearPromissory> monthYearPromissories, decimal amount, Guid PromissoryId, string fullName, DateTime date)
        {
            //Tìm kiếm theo phụ lục có ngày ký gần nhất
            var extra = rentFiles.Where(e => e.Type == 2).OrderByDescending(x => x.DateHD).FirstOrDefault();
            //var extra = (from d in debtsTables
            //             join rf in rentFiles on d.RentFileId equals rf.Id
            //             where rf.Type == 2 && d.Check != true && d.Type != 2
            //             select d).OrderByDescending(x => x.DateStart).FirstOrDefault();

            DebtsTable extraDebtsTable = null;
            if (extra != null) extraDebtsTable = debtsTables.Where(e => e.RentFileId == extra.Id && e.Check != true && e.Type != 2).OrderBy(e => e.DateStart).FirstOrDefault();

            if (extraDebtsTable != null)
            {
                if (amount >= extraDebtsTable.PriceDiff)
                {
                    amount = amount - extraDebtsTable.PriceDiff;

                    debtsTables.Find(d => d == extraDebtsTable).Check = true;
                    debtsTables.Find(d => d == extraDebtsTable).NocReceiptId = PromissoryId;
                    debtsTables.Find(d => d == extraDebtsTable).Executor = fullName;
                    debtsTables.Find(d => d == extraDebtsTable).PriceDiff = 0;
                    debtsTables.Find(d => d == extraDebtsTable).NearestActivities = "Nộp tiền (Từ phiếu thu ImportExcel)";
                    debtsTables.Find(d => d == extraDebtsTable).Date = date;
                    debtsTables.Find(d => d == extraDebtsTable).UpdatedAt = DateTime.Now;

                    monthYearPromissories.Add(new MonthYearPromissory
                    {
                        Month = extraDebtsTable.DateStart.Month,
                        Year = extraDebtsTable.DateStart.Year
                    });

                    if (amount > 0) amount = updateDebtTable(debtsTables, rentFiles, monthYearPromissories, amount, PromissoryId, fullName, date);
                }
            }
            else
            {
                //Tìm kiếm theo hợp đồng có ngày ký gần nhất
                var main = rentFiles.Where(e => e.Type == 1).OrderByDescending(x => x.DateHD).FirstOrDefault();

                //var main = (from d in debtsTables
                //             join rf in rentFiles on d.RentFileId equals rf.Id
                //             where rf.Type == 1 && d.Check != true && d.Type != 2
                //            select d).OrderByDescending(x => x.DateStart).FirstOrDefault();

                DebtsTable mainDebtsTable = null;
                if (main != null) mainDebtsTable = debtsTables.Where(e => e.RentFileId == main.Id && e.Check != true && e.Type != 2).OrderBy(e => e.DateStart).FirstOrDefault();

                if (mainDebtsTable != null)
                {
                    if (amount >= mainDebtsTable.PriceDiff)
                    {
                        amount = amount - mainDebtsTable.PriceDiff;

                        debtsTables.Find(d => d == mainDebtsTable).Check = true;
                        debtsTables.Find(d => d == mainDebtsTable).NocReceiptId = PromissoryId;
                        debtsTables.Find(d => d == mainDebtsTable).Executor = fullName;
                        debtsTables.Find(d => d == mainDebtsTable).PriceDiff = 0;
                        debtsTables.Find(d => d == mainDebtsTable).NearestActivities = "Nộp tiền (Từ phiếu thu ImportExcel)";
                        debtsTables.Find(d => d == mainDebtsTable).Date = date;
                        debtsTables.Find(d => d == mainDebtsTable).UpdatedAt = DateTime.Now;

                        monthYearPromissories.Add(new MonthYearPromissory
                        {
                            Month = mainDebtsTable.DateStart.Month,
                            Year = mainDebtsTable.DateStart.Year
                        });

                        if (amount > 0) amount = updateDebtTable(debtsTables, rentFiles, monthYearPromissories, amount, PromissoryId, fullName, date);
                    }
                }
                else
                {
                    //Tìm kiếm theo truy thu
                    var dff = debtsTables.Where(e => e.Check != true && e.Type != 2).OrderBy(e => e.DateStart).FirstOrDefault();

                    if(dff == null)
                    {
                        dff = debtsTables.Where(e => e.Check != true && e.Type == 2).OrderBy(e => e.DateStart).FirstOrDefault();
                    }

                    if (dff != null)
                    {
                        if (amount >= dff.PriceDiff)
                        {
                            amount = amount - dff.PriceDiff;

                            debtsTables.Find(d => d == dff).Check = true;
                            debtsTables.Find(d => d == dff).NocReceiptId = PromissoryId;
                            debtsTables.Find(d => d == dff).Executor = fullName;
                            debtsTables.Find(d => d == dff).PriceDiff = 0;
                            debtsTables.Find(d => d == dff).NearestActivities = "Nộp tiền (Từ phiếu thu ImportExcel)";
                            debtsTables.Find(d => d == dff).Date = date;
                            debtsTables.Find(d => d == dff).UpdatedAt = DateTime.Now;
                            monthYearPromissories.Add(new MonthYearPromissory
                            {
                                Month = dff.DateStart.Month,
                                Year = dff.DateStart.Year
                            });

                            if (amount > 0) amount = updateDebtTable(debtsTables, rentFiles, monthYearPromissories, amount, PromissoryId, fullName, date);
                        }
                    }
                }
            }
            return amount;
        }

        public static List<PromissoryDataImport> importData(MemoryStream ms, int sheetnumber, int rowStart)
        {
            XSSFWorkbook workbook = new XSSFWorkbook(ms);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);

            int expected = Math.Max(0, sheet.LastRowNum - rowStart + 1);
            List<PromissoryDataImport> res = new List<PromissoryDataImport>(expected);

            for (int row = rowStart; row <= sheet.LastRowNum; row++)
            {
                var rowRef = sheet.GetRow(row);
                if (rowRef == null) continue;

                string c0 = UtilsService.getCellValue(rowRef.GetCell(0, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                string c1 = UtilsService.getCellValue(rowRef.GetCell(1, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                string c2 = UtilsService.getCellValue(rowRef.GetCell(2, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                string c3 = UtilsService.getCellValue(rowRef.GetCell(3, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                string c4 = UtilsService.getCellValue(rowRef.GetCell(4, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                string c5 = UtilsService.getCellValue(rowRef.GetCell(5, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                string c6 = UtilsService.getCellValue(rowRef.GetCell(6, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                string c7 = UtilsService.getCellValue(rowRef.GetCell(7, MissingCellPolicy.RETURN_NULL_AND_BLANK));

                // Bỏ qua dòng hoàn toàn trống
                if (string.IsNullOrWhiteSpace(c0) && string.IsNullOrWhiteSpace(c1) && string.IsNullOrWhiteSpace(c2)
                    && string.IsNullOrWhiteSpace(c3) && string.IsNullOrWhiteSpace(c4) && string.IsNullOrWhiteSpace(c5)
                    && string.IsNullOrWhiteSpace(c6) && string.IsNullOrWhiteSpace(c7))
                {
                    continue;
                }

                var input = new PromissoryDataImport
                {
                    Valid = true,
                    ErrMsg = string.Empty
                };

                // 0: Số thứ tự (Number)
                if (!string.IsNullOrEmpty(c0))
                {
                    input.Number = c0.Trim();
                }

                // 1: Mã định danh (Code) - bắt buộc
                if (!string.IsNullOrEmpty(c1))
                {
                    input.Code = c1.Trim();
                }
                else
                {
                    input.Valid = false;
                    input.ErrMsg += "Cột Mã định danh chưa có dữ liệu\n";
                }

                // 2: Ngày thu (Date) - OADate
                if (!string.IsNullOrEmpty(c2))
                {
                    if (double.TryParse(c2, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var oa))
                    {
                        var dt = DateTime.FromOADate(oa);
                        if (dt.Year >= 1900) input.Date = dt; else { input.Valid = false; input.ErrMsg += "Ngày thu không hợp lệ\n"; }
                    }
                    else
                    {
                        input.Valid = false; input.ErrMsg += "Lỗi cột thông tin Ngày thu\n";
                    }
                }
                else { input.Valid = false; input.ErrMsg += "Cột Ngày thu chưa có dữ liệu\n"; }

                // 3: Nội dung thu (Note) - bắt buộc
                if (!string.IsNullOrEmpty(c3)) input.Note = c3.Trim(); else { input.Valid = false; input.ErrMsg += "Cột Nội dung thu chưa có dữ liệu\n"; }

                // 4: Số tiền (Price) - bắt buộc
                if (!string.IsNullOrEmpty(c4))
                {
                    if (decimal.TryParse(c4, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var price))
                        input.Price = price;
                    else { input.Valid = false; input.ErrMsg += "Lỗi cột Số tiền\n"; }
                }
                else { input.Valid = false; input.ErrMsg += "Cột Số tiền chưa có dữ liệu\n"; }

                // 5: Số phiếu chuyển (NumberOfTransfer) - bắt buộc
                if (!string.IsNullOrEmpty(c5)) input.NumberOfTransfer = c5.Trim(); else { input.Valid = false; input.ErrMsg += "Cột Số phiếu chuyển chưa có dữ liệu\n"; }

                // 6: Ngày phiếu chuyển (DateOfTransfer) - OADate
                if (!string.IsNullOrEmpty(c6))
                {
                    if (double.TryParse(c6, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var oa2))
                    {
                        var dt2 = DateTime.FromOADate(oa2);
                        if (dt2.Year >= 1900) input.DateOfTransfer = dt2; else { input.Valid = false; input.ErrMsg += "Ngày phiếu chuyển không hợp lệ\n"; }
                    }
                    else { input.Valid = false; input.ErrMsg += "Lỗi cột thông tin Ngày phiếu chuyển\n"; }
                }
                else { input.Valid = false; input.ErrMsg += "Cột Ngày phiếu chuyển chưa có dữ liệu\n"; }

                // 7: Mã xuất hóa đơn (InvoiceCode) - bắt buộc
                if (!string.IsNullOrEmpty(c7)) input.InvoiceCode = c7.Trim(); else { input.Valid = false; input.ErrMsg += "Cột Mã xuất hóa đơn chưa có dữ liệu\n"; }

                res.Add(input);
            }

            return res;
        }

        #endregion


        #region import excel danh sách hợp đồng thuê

        [HttpPost]
        [Route("ImportContractDataExcel")]
        public IActionResult ImportContractDataExcel()
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
                importHistory.Type = AppEnums.ImportHistoryType.Noc_Contract_Rent;

                List<ContractRentDataImport> data = new List<ContractRentDataImport>();

                //Lấy dữ liệu từ file excel và lưu lại file
                foreach (var file in httpRequest)
                {
                    var postedFile = httpRequest[i];
                    if (postedFile != null && postedFile.Length > 0)
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
                            string folderName = _configuration["AppSettings:BaseUrlImportHistory"];
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
                                    data = importContractRentData(ms, 0, 1);
                                }
                            }
                        }
                    }
                    i++;
                }

                List<TypeAttributeItem> contractStatusData = (from t in _context.TypeAttributes
                                                              join ti in _context.TypeAttributeItems on t.Id equals ti.TypeAttributeId
                                                              where t.Status != EntityStatus.DELETED && ti.Status != EntityStatus.DELETED
                                                              && t.Code == "CONTRACT_RENT_NOC_STATUS"
                                                              select ti).ToList();

                List<ContractRentDataImport> dataValid = new List<ContractRentDataImport>();
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        data.ForEach(item =>
                        {
                            if (item.Valid)
                            {
                                //Kiểm tra Số hợp đồng có tồn tại không
                                RentFile rentFileCodeExist = _context.RentFiles.Where(e => e.Code == item.NoContract && e.Status != EntityStatus.DELETED).FirstOrDefault();
                                if (rentFileCodeExist != null)
                                {
                                    item.Valid = false;
                                    item.ErrMsg += "Số hợp đồng đã tồn tại\n";
                                }

                                //Kiểm tra Mã định danh có tồn tại không
                                RentFile rentFileCodehsExist = _context.RentFiles.Where(e => e.CodeHS == item.CodeContract && e.Status != EntityStatus.DELETED).FirstOrDefault();
                                if (rentFileCodehsExist != null)
                                {
                                    item.Valid = false;
                                    item.ErrMsg += "Mã hồ sơ đã tồn tại\n";
                                }

                                //Kiểm tra trạng thái hợp đồng có hợp lệ không
                                string contractStatusNoneUnicode = UtilsService.NonUnicode(item.ContractStatus);
                                TypeAttributeItem typeAttributeItem = contractStatusData.AsEnumerable().Where(e => UtilsService.NonUnicode(e.Name) == contractStatusNoneUnicode).FirstOrDefault();
                                if (typeAttributeItem == null)
                                {
                                    item.Valid = false;
                                    item.ErrMsg += "Không tìm thấy trạng thái hợp đồng tương ứng\n";
                                }

                                //Kiểm tra thông tin CCCD/CMND nếu chưa tồn tại thì thêm mới
                                Customer customer = _context.Customers.Where(e => e.Code == item.IdentityCode && e.Status != EntityStatus.DELETED).FirstOrDefault();
                                if (customer == null)
                                {
                                    customer = new Customer();
                                    customer.FullName = item.CustomerName;
                                    customer.Code = item.IdentityCode;
                                    customer.Dob = item.Dob;
                                    customer.Phone = item.Phone;
                                    customer.Address = item.Address;
                                    customer.CreatedById = userId;
                                    customer.CreatedBy = $"{fullName} (Excel)";

                                    _context.Customers.Add(customer);
                                    _context.SaveChanges();
                                }

                                //Kiểm loại biên bản áp dụng và Mã định danh căn nhà/căn hộ
                                string traNoneUnicode = UtilsService.NonUnicode(item.TypeReportApplyStr);
                                TypeReportApply? typeReportApply = null;
                                if (traNoneUnicode == "nha-ho-rieng" || traNoneUnicode == "nha-rieng-le")
                                {
                                    typeReportApply = TypeReportApply.NHA_RIENG_LE;
                                }
                                else if (traNoneUnicode == "nha-ho-chung")
                                {
                                    typeReportApply = TypeReportApply.NHA_HO_CHUNG;
                                }
                                else if (traNoneUnicode == "nha-chung-cu")
                                {
                                    typeReportApply = TypeReportApply.NHA_CHUNG_CU;
                                }
                                else
                                {
                                    item.Valid = false;
                                    item.ErrMsg += "Cột thông tin Loại biên bản áp dụng không hợp lệ\n";
                                }

                                if (typeReportApply != null && item.Valid)
                                {
                                    RentFile rentFile = new RentFile();
                                    rentFile.CodeKH = customer.Code;
                                    rentFile.CustomerId = customer.Id;
                                    rentFile.Phone = customer.Phone;
                                    rentFile.Dob = customer.Dob;
                                    rentFile.AddressKH = customer.Address;

                                    if (typeReportApply == TypeReportApply.NHA_RIENG_LE)
                                    {
                                        Block block = _context.Blocks.Where(e => e.TypeReportApply == typeReportApply && e.Code == item.Code && e.TypeBlockEntity == TypeBlockEntity.BLOCK_RENT).FirstOrDefault();
                                        if (block == null)
                                        {
                                            item.Valid = false;
                                            item.ErrMsg += "Không tìm thấy Căn nhà với Mã định danh tương ứng\n";
                                        }
                                        else
                                        {
                                            rentFile.CodeCH = block.Code;
                                            rentFile.RentBlockId = block.Id;
                                            rentFile.TypeBlockId = block.TypeBlockId;
                                        }
                                    }
                                    else
                                    {
                                        Apartment apartment = _context.Apartments.Where(a => a.Code == item.Code && a.TypeApartmentEntity == TypeApartmentEntity.APARTMENT_RENT && a.Status != EntityStatus.DELETED).FirstOrDefault();
                                        if (apartment == null)
                                        {
                                            item.Valid = false;
                                            item.ErrMsg += "Không tìm thấy Căn hộ với Mã định danh tương ứng\n";
                                        }
                                        else
                                        {
                                            //Tìm căn nhà
                                            Block blockParent = _context.Blocks.Find(apartment.BlockId);
                                            if (blockParent == null)
                                            {
                                                item.Valid = false;
                                                item.ErrMsg += "Không tìm thấy Căn nhà của căn hộ\n";
                                            }
                                            else
                                            {
                                                rentFile.CodeCH = apartment.Code;
                                                rentFile.RentBlockId = blockParent.Id;
                                                rentFile.TypeBlockId = blockParent.TypeBlockId;
                                                rentFile.RentApartmentId = apartment.Id;
                                            }
                                        }
                                    }

                                    if (item.Valid)
                                    {
                                        rentFile.TypeReportApply = (TypeReportApply)typeReportApply;
                                        rentFile.Type = 1;
                                        rentFile.Month = 60;
                                        rentFile.Code = item.NoContract;
                                        rentFile.CodeHS = item.CodeContract;
                                        rentFile.DateHD = (DateTime)item.DateSign;
                                        rentFile.FileStatus = typeAttributeItem.Id;
                                        rentFile.CodeKH = item.IdentityCode;
                                        rentFile.CreatedById = userId;
                                        rentFile.CreatedBy = $"{fullName} (Excel)";
                                        rentFile.UpdatedById = userId;
                                        rentFile.UpdatedBy = $"{fullName} (Excel)";

                                        _context.RentFiles.Add(rentFile);
                                    }
                                }
                            }
                        });


                        importHistory.Data = data.Cast<dynamic>().ToList();
                        importHistory.CreatedById = userId;
                        importHistory.CreatedBy = fullName;

                        _context.ImportHistories.Add(importHistory);
                        _context.SaveChanges();

                        transaction.Commit();
                        def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                        def.metadata = data.Count;
                        def.data = data;
                    }
                    catch (Exception ex)
                    {
                        log.Error("ImportContractDataExcel:" + ex);
                        transaction.Rollback();
                        def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                    }
                }

                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("ImportContractDataExcel:" + ex);
                def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                return Ok(def);
            }
        }

        public static List<ContractRentDataImport> importContractRentData(MemoryStream ms, int sheetnumber, int rowStart)
        {
            XSSFWorkbook workbook = new XSSFWorkbook(ms);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);

            List<ContractRentDataImport> res = new List<ContractRentDataImport>();
            for (int row = rowStart; row <= sheet.LastRowNum; row++)
            {
                if (sheet.GetRow(row) != null)
                {
                    //Đọc dữ liệu từ từng cell
                    ContractRentDataImport input1Detai = new ContractRentDataImport();
                    input1Detai.Valid = true;
                    input1Detai.ErrMsg = "";

                    for (int i = 0; i < 12; i++)
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
                                        input1Detai.Index = int.Parse(str);
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Số thứ tự\n";
                                }
                            }
                            else if (i == 1)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.NoContract = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Số hợp đồng chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Số hợp đồng\n";
                                }
                            }
                            else if (i == 2)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.CodeContract = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Mã định danh chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Mã định danh\n";
                                }
                            }
                            else if (i == 3)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.DateSign = DateTime.FromOADate(Double.Parse(str));
                                        if (input1Detai.DateSign.Value.Year < 1900)
                                        {
                                            input1Detai.Valid = false;
                                            input1Detai.ErrMsg += "Ngày ký không hợp lệ\n";
                                        }
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Ngày ký chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột thông tin Ngày ký\n";
                                }
                            }
                            else if (i == 4)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.ContractStatus = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Trạng thái hợp đồng chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Trạng thái hợp đồng\n";
                                }
                            }
                            else if (i == 5)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.CustomerName = str;

                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Người đứng tên chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Người đứng tên\n";
                                }
                            }
                            else if (i == 6)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.IdentityCode = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột CMND/CCCD chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột CMND/CCCD\n";
                                }
                            }
                            else if (i == 7)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.Dob = DateTime.FromOADate(Double.Parse(str));
                                        if (input1Detai.Dob.Value.Year < 1900)
                                        {
                                            input1Detai.Valid = false;
                                            input1Detai.ErrMsg += "Ngày sinh không hợp lệ\n";
                                        }
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột thông tin Ngày sinh\n";
                                }
                            }
                            else if (i == 8)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.Phone = str;
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Số điện thoại\n";
                                }
                            }
                            else if (i == 9)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.Address = str;
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Địa chỉ\n";
                                }
                            }
                            else if (i == 10)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.TypeReportApplyStr = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Loại biên bản áp dụng chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Loại biên bản áp dụng\n";
                                }
                            }
                            else if (i == 11)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.Code = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Mã định danh căn nhà/căn hộ chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Mã định danh căn nhà/căn hộ\n";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            input1Detai.Valid = false;
                            input1Detai.ErrMsg += "Lỗi dữ liệu\n";
                            log.Error("Exception:" + ex);
                        }
                    }

                    res.Add(input1Detai);
                }
            }

            return res;
        }

        #endregion

        #region xuất phiếu thu hợp đồng thuê
        [HttpPost("GetPromissoryReport")]
        public async Task<IActionResult> GetPromissoryReport([FromBody] PromissoryReportReq input)
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
                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                try
                {
                    List<PromissoryReportRes> res = (from p in _context.NocReceipts
                                                     join d in _context.DebtsTables on p.Id equals d.NocReceiptId
                                                     where p.IsImportExcel == true
                                                     && p.Date >= input.FromDate && p.Date <= input.ToDate
                                                     && p.Status != EntityStatus.DELETED
                                                     && d.Status != EntityStatus.DELETED
                                                     && d.Check == true && d.CheckPayDepartment != true
                                                     select new PromissoryReportRes
                                                     {
                                                         Id = p.Id,
                                                         Code = p.Code,
                                                         Date = p.Date,
                                                         Executor = p.Executor,
                                                         Price = p.Price,
                                                         Action = p.Action,
                                                         NumberOfTransfer = p.NumberOfTransfer,
                                                         InvoiceCode = p.InvoiceCode,
                                                         DateOfTransfer = p.DateOfTransfer,
                                                         Note = p.Note,
                                                         IsImportExcel = p.IsImportExcel,
                                                         RentFileId = d.RentFileId,
                                                         ContentsVat = p.Content
                                                     }).Distinct().ToList();

                    foreach (var item in res)
                    {
                        RentFile rentFile = _context.RentFiles.Find(item.RentFileId);
                        if (rentFile != null)
                        {
                            Customer customer = _context.Customers.Find(rentFile.CustomerId);
                            if (customer != null)
                            {
                                item.CustomerName = customer.FullName;
                            }

                            item.ContractNo = rentFile.Code;

                            if (rentFile.RentApartmentId == 0)
                            {
                                Block block = _context.Blocks.Find(rentFile.RentBlockId);
                                if (block != null)
                                {
                                    item.Address = block.Address;
                                    Ward ward = _context.Wards.Find(block.Ward);
                                    District district = _context.Districts.Find(block.District);

                                    if (ward != null) item.Address = item.Address + "," + ward.Name;
                                    if (district != null) item.Address = item.Address + "," + district.Name;
                                }

                            }
                            else
                            {
                                Apartment apartment = _context.Apartments.Find(rentFile.RentApartmentId);
                                if (apartment != null)
                                {
                                    item.Address = apartment.Address;
                                }

                                Block block = _context.Blocks.Find(rentFile.RentBlockId);
                                if (block != null)
                                {
                                    item.Address = item.Address + "," + block.Address;
                                    Ward ward = _context.Wards.Find(block.Ward);
                                    District district = _context.Districts.Find(block.District);

                                    if (ward != null) item.Address = item.Address + "," + ward.Name;
                                    if (district != null) item.Address = item.Address + "," + district.Name;
                                }
                            }
                        }

                        if (input.Vat != null)
                        {
                            item.AmountNoTax = Math.Round(item.Price / (100 + (decimal)input.Vat) * 100);
                            item.AmountTax = item.Price - item.AmountNoTax;
                        }
                        else
                        {
                            item.AmountNoTax = item.Price;
                            item.AmountTax = 0;
                        }
                    }

                    def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                    def.data = res;
                    return Ok(def);
                }
                catch (DbUpdateException e)
                {
                    log.Error("DbUpdateException:" + e);
                    def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                    return Ok(def);
                }
            }
            catch (Exception e)
            {
                log.Error("GetPromissoryReport Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [HttpPost("UpdatePromissoryReport")]
        public async Task<IActionResult> UpdatePromissoryReport([FromBody] List<NocReceipt> input)
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
                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        input.ForEach(item =>
                        {
                            List<DebtsTable> debtsTbl = _context.DebtsTables.Where(e => e.NocReceiptId == item.Id && e.Status != EntityStatus.DELETED && e.CheckPayDepartment != true).ToList();
                            debtsTbl.ForEach(x => x.CheckPayDepartment = true);

                            _context.UpdateRange(debtsTbl);
                        });

                        _context.SaveChanges();
                        transaction.Commit();

                        def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                        def.data = null;
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
            catch (Exception e)
            {
                log.Error("GetPromissoryReport Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [HttpPost("ExportPromissoryReport")]
        public async Task<IActionResult> ExportPromissoryReport([FromBody] List<PromissoryReportRes> data)
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

            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.EXPORT))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                // khởi tạo wb rỗng
                XSSFWorkbook wb = new XSSFWorkbook();
                // Tạo ra 1 sheet
                ISheet sheet = wb.CreateSheet();

                string template = @"NOCexcel/hoa_don_tien_thue_nha_cu.xlsx";
                string webRootPath = _hostingEnvironment.WebRootPath;
                string templatePath = Path.Combine(webRootPath, template);

                MemoryStream ms = writeDataToExcel(templatePath, 0, data);
                byte[] byteArrayContent = ms.ToArray();
                return new FileContentResult(byteArrayContent, "application/octet-stream");
            }
            catch (Exception e)
            {
                log.Error("GetPromissoryReport Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        private static MemoryStream writeDataToExcel(string templatePath, int sheetnumber, List<PromissoryReportRes> data)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            int rowStart = 4;

            if (sheet != null)
            {
                int datacol = 12;
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
                    double price = 0;
                    double amountNoTax = 0;
                    double amountTax = 0;

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
                                    row.GetCell(i).SetCellValue(item.Code);
                                }
                                else if (i == 2)
                                {
                                    row.GetCell(i).SetCellValue(item.Date != null ? item.Date.ToString("dd/MM/yyyy") : "");
                                }
                                else if (i == 3)
                                {
                                    row.GetCell(i).SetCellValue(item.Note);
                                }
                                else if (i == 4)
                                {
                                    row.GetCell(i).SetCellValue(item.CustomerName);
                                }
                                else if (i == 5)
                                {
                                    row.GetCell(i).SetCellValue(item.Address);
                                }
                                else if (i == 6)
                                {
                                    row.GetCell(i).SetCellValue(item.ContractNo);
                                }
                                else if (i == 7)
                                {
                                    row.GetCell(i).SetCellValue(item.InvoiceCode);
                                }
                                else if (i == 8)
                                {
                                    row.GetCell(i).SetCellValue(item.ContentsVat);
                                }
                                else if (i == 9)
                                {
                                    row.GetCell(i).SetCellValue((double)item.Price);
                                    price += (double)item.Price;
                                }
                                else if (i == 10)
                                {
                                    if (item.AmountNoTax != null)
                                    {
                                        row.GetCell(i).SetCellValue((double)item.AmountNoTax);
                                        amountNoTax += (double)item.AmountNoTax;
                                    }
                                }
                                else if (i == 11)
                                {
                                    if (item.AmountTax != null)
                                    {
                                        row.GetCell(i).SetCellValue((double)item.AmountTax);
                                        amountTax += (double)item.AmountTax;
                                    }
                                }
                            }

                            rowStart++;
                            k++;
                        }
                        catch (Exception ex)
                        {
                            log.Error("WriteDataToExcel:" + ex);
                        }
                    }

                    XSSFRow rowSum = (XSSFRow)sheet.CreateRow(rowStart);
                    for (int i = 0; i < datacol; i++)
                    {
                        rowSum.CreateCell(i).CellStyle = rowStyle[i];
                    }

                    CellRangeAddress cellRangeAddress = new CellRangeAddress(rowStart, rowStart, 0, 8);
                    sheet.AddMergedRegion(cellRangeAddress);
                    rowSum.GetCell(0).SetCellValue("TỔNG CỘNG");
                    rowSum.GetCell(9).SetCellValue(price);
                    rowSum.GetCell(10).SetCellValue(amountNoTax);
                    rowSum.GetCell(11).SetCellValue(amountTax);
                }
                catch (Exception ex)
                {
                    log.Error("WriteDataToExcel:" + ex);
                }
            }

            sheet.ForceFormulaRecalculation = true;

            MemoryStream ms = new MemoryStream();

            workbook.Write(ms);

            return ms;
        }
        #endregion


        #region import excel danh sách hợp đồng thuê mẫu 2

        [HttpPost]
        [Route("ImportContractDataExcelType2")]
        public IActionResult ImportContractDataExcelType2()
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
                importHistory.Type = AppEnums.ImportHistoryType.Noc_Contract_Rent;

                List<ContractRentDataImportType2> data = new List<ContractRentDataImportType2>();

                //Lấy dữ liệu từ file excel và lưu lại file
                foreach (var file in httpRequest)
                {
                    var postedFile = httpRequest[i];
                    if (postedFile != null && postedFile.Length > 0)
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
                            string folderName = _configuration["AppSettings:BaseUrlImportHistory"];
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
                            List<SimpleLaneData> laneData = _context.Lanies.Where(e => e.Province == 2 && e.Status != EntityStatus.DELETED).Select(e => new SimpleLaneData { Id = e.Id, Name = e.Name, Ward = e.Ward }).ToList();
                            List<SimpleData> wardData = _context.Wards.Where(e => e.ProvinceId == 2 && e.Status != EntityStatus.DELETED).Select(e => new SimpleData { Id = e.Id, Name = e.Name }).ToList();
                            List<District> districtData = _context.Districts.Where(e => e.ProvinceId == 2 && e.Status != EntityStatus.DELETED).ToList();
                            List<SimpleData> typeBlockData = _context.TypeBlocks.Where(e => e.Status != EntityStatus.DELETED).Select(e => new SimpleData { Id = e.Id, Name = e.Name }).ToList();
                            List<SimpleBlockData> blockData = _context.Blocks.Where(e => e.Status != EntityStatus.DELETED).Select(e => new SimpleBlockData { Id = e.Id, Code = e.Code, TypeBlockEntity = e.TypeBlockEntity }).ToList();
                            List<SimpleApartmentData> apartmentData = _context.Apartments.Where(e => e.Status != EntityStatus.DELETED).Select(e => new SimpleApartmentData { Id = e.Id, Code = e.Code, TypeApartmentEntity = e.TypeApartmentEntity }).ToList();
                            List<Customer> customerData = _context.Customers.Where(c => c.Status != EntityStatus.DELETED).ToList();
                            using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                            {
                                fileData = binaryReader.ReadBytes((int)file.Length);
                                using (MemoryStream ms = new MemoryStream(fileData))
                                {
                                    data = importContractRentDataType2(ms, 0, 1, laneData, wardData, districtData, typeBlockData, blockData, apartmentData, customerData);
                                }
                            }
                        }
                    }
                    i++;
                }
                List<TypeAttributeItem> contractStatusData = (from t in _context.TypeAttributes
                                                              join ti in _context.TypeAttributeItems on t.Id equals ti.TypeAttributeId
                                                              where t.Status != EntityStatus.DELETED && ti.Status != EntityStatus.DELETED
                                                              && t.Code == "CONTRACT_RENT_NOC_STATUS"
                                                              select ti).ToList();

                List<ContractRentDataImportType2> dataValid = new List<ContractRentDataImportType2>();
                using (var transaction = _context.Database.BeginTransaction())
                {
                    var countEr = 0;
                    try
                    {
                        var laneData = _context.Lanies.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                        var wardData = _context.Wards.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                        var districtData = _context.Districts.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                        var provinceData = _context.Provincies.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                        var blockData = _context.Blocks.Where(e => e.Status != EntityStatus.DELETED).ToList();
                        var apartmentData = _context.Apartments.Where(e => e.Status != EntityStatus.DELETED).ToList();
                        var customerData = _context.Customers.Where(c => c.Status != EntityStatus.DELETED).ToList();
                        var ApartmentDetailData = _context.ApartmentDetails.Where(c => c.Status != EntityStatus.DELETED).ToList();
                        var BlockDetailData = _context.BlockDetails.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                        var Floor = _context.Floors.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                        var Area = _context.Areas.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();

                        var vat = _context.Vats.Where(v => v.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(v => v.DoApply)
                                .Select(v => new ImportDto
                                {
                                    DoApply = v.DoApply,
                                    Value = v.Value
                                })
                                .ToList();

                        var salaryCoefficients = _context.Salaries.Where(v => v.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(v => v.DoApply)
                             .Select(v => new ImportDto
                             {
                                 DoApply = v.DoApply,
                                 Value = v.Value
                             })
                             .ToList();

                        var coefficients = _context.Coefficients.Where(v => v.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(v => v.DoApply)
                       .Select(v => new ImportDto
                       {
                           DoApply = v.DoApply,
                           Value = v.Value
                       })
                       .ToList();

                        var rentingPricesData = _context.RentingPricies.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                        var countlv = 0;

                        List<RentFile> resRentFiles = new List<RentFile>(); 
                        List<DebtsTable> resDebtsTables = new List<DebtsTable>();
                        List<RentBctTable> resRentBctTables = new List<RentBctTable>();
                        List<Debts> resDebts = new List<Debts>();
                        data.ForEach(item =>
                        {
                            try
                            {
                                countlv++;
                                int? customerId = null;
                                RentFile rentFile = new RentFile();
                                Debts DtDebts = new Debts();
                                List<BlockDetailData> blockDetailsData = new List<BlockDetailData>();
                                List<ApartmentDetailData> apartmentDetailDatas = new List<ApartmentDetailData>();
                                Block block = blockData.Where(e => e.Code == item.Code && e.TypeBlockEntity == TypeBlockEntity.BLOCK_RENT && e.Status != EntityStatus.DELETED).FirstOrDefault();
                                Apartment apartment = apartmentData.Where(a => a.Code == item.Code && a.TypeApartmentEntity == TypeApartmentEntity.APARTMENT_RENT && a.Status != EntityStatus.DELETED).FirstOrDefault();

                                if (block != null)
                                {

                                    BlockData res = _mapper.Map<BlockData>(block);

                                    res.FullAddress = res.Address;

                                    var ward = wardData.Where(p => p.Id == res.Ward).FirstOrDefault();
                                    res.FullAddress = res.FullAddress != null && res.FullAddress != "" ? (ward != null ? String.Join(", ", res.FullAddress, ward.Name) : res.FullAddress) : (ward != null ? ward.Name : res.FullAddress);

                                    var district = districtData.Where(p => p.Id == res.District).FirstOrDefault();
                                    res.FullAddress = res.FullAddress != null && res.FullAddress != "" ? (district != null ? String.Join(", ", res.FullAddress, district.Name) : res.FullAddress) : (district != null ? district.Name : res.FullAddress);

                                    var province = provinceData.Where(p => p.Id == res.Province).FirstOrDefault();
                                    res.FullAddress = res.FullAddress != null && res.FullAddress != "" ? (province != null ? String.Join(", ", res.FullAddress, province.Name) : res.FullAddress) : (province != null ? province.Name : res.FullAddress);

                                    List<BlockDetail> blockDetails = BlockDetailData.Where(l => l.BlockId == rentFile.RentBlockId).ToList();
                                    List<BlockDetailData> map_BlockDetails = _mapper.Map<List<BlockDetailData>>(blockDetails);
                                    foreach (BlockDetailData map_BlockDetail in map_BlockDetails)
                                    {
                                        Entities.Floor floor = Floor.Where(f => f.Id == map_BlockDetail.FloorId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                        map_BlockDetail.FloorName = floor != null ? floor.Name : "";
                                        map_BlockDetail.FloorCode = floor != null ? floor.Code : 0;

                                        Area area = Area.Where(f => f.Id == map_BlockDetail.AreaId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                        map_BlockDetail.AreaName = area != null ? area.Name : "";
                                        map_BlockDetail.IsMezzanine = area != null ? area.IsMezzanine : null;
                                    }
                                    res.blockDetails = map_BlockDetails.OrderBy(x => x.FloorCode).ThenBy(x => x.IsMezzanine).ToList();

                                    rentFile.CodeCN = block.Code;
                                    rentFile.RentBlockId = block.Id;
                                    rentFile.TypeBlockId = block.TypeBlockId;
                                    rentFile.UseAreaValueCN = block.UseAreaValue;
                                    rentFile.fullAddressCN = res.FullAddress;
                                    rentFile.TypeBlockId = res.TypeBlockId;
                                    rentFile.TypeHouse = res.TypeHouse;
                                    rentFile.TypeReportApply = res.TypeReportApply;
                                    rentFile.CampusArea = res.CampusArea;
                                    rentFile.ConstructionAreaValue = res.ConstructionAreaValue;
                                    rentFile.DistrictId = res.District;
                                    rentFile.WardId = res.Ward;
                                    rentFile.LaneId = (int)res.Lane;

                                    customerId = block.CustomerId;
                                    blockDetailsData = res.blockDetails;
                                }
                                //=========================================================================================================================================================//
                                if (apartment == null && block == null)
                                {
                                    item.Valid = false;
                                    item.ErrMsg += "Không tìm thấy Căn nhà(Căn hộ) với Mã định danh tương ứng\n";
                                }
                                if (apartment != null)
                                {
                                    //Tìm căn nhà
                                    
                                    Block blockParent = _context.Blocks.Find(apartment.BlockId);
                                    if (blockParent == null)
                                    {
                                        item.Valid = false;
                                        item.ErrMsg += "Không tìm thấy Căn nhà thuộc căn hộ\n";
                                    }
                                    else
                                    {
                                        BlockData res = _mapper.Map<BlockData>(blockParent);

                                        res.FullAddress = res.Address;

                                        Ward ward = wardData.Where(p => p.Id == res.Ward).FirstOrDefault();
                                        res.FullAddress = res.FullAddress != null && res.FullAddress != "" ? (ward != null ? String.Join(", ", res.FullAddress, ward.Name) : res.FullAddress) : (ward != null ? ward.Name : res.FullAddress);

                                        District district = districtData.Where(p => p.Id == res.District).FirstOrDefault();
                                        res.FullAddress = res.FullAddress != null && res.FullAddress != "" ? (district != null ? String.Join(", ", res.FullAddress, district.Name) : res.FullAddress) : (district != null ? district.Name : res.FullAddress);

                                        Province province = provinceData.Where(p => p.Id == res.Province).FirstOrDefault();
                                        res.FullAddress = res.FullAddress != null && res.FullAddress != "" ? (province != null ? String.Join(", ", res.FullAddress, province.Name) : res.FullAddress) : (province != null ? province.Name : res.FullAddress);


                                        ApartmentData map_apartments = _mapper.Map<ApartmentData>(apartment);
                                        List<ApartmentDetail> apartmentDetails = ApartmentDetailData.Where(a => a.TargetId == apartment.Id && a.Type == TypeApartmentDetail.APARTMENT && a.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                                        List<ApartmentDetailData> apartmentDetailData = _mapper.Map<List<ApartmentDetailData>>(apartmentDetails);
                                        foreach (ApartmentDetailData map_apartmentDetail in apartmentDetailData)
                                        {
                                            Area area = Area.Where(f => f.Id == map_apartmentDetail.AreaId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                            map_apartmentDetail.AreaName = area != null ? area.Name : "";
                                            map_apartmentDetail.IsMezzanine = area != null ? area.IsMezzanine : null;

                                            Entities.Floor floor = Floor.Where(f => f.Id == map_apartmentDetail.FloorId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                            map_apartmentDetail.FloorName = floor != null ? floor.Name : "";
                                            map_apartmentDetail.FloorCode = floor != null ? floor.Code : 0;
                                        }
                                        map_apartments.apartmentDetailData = apartmentDetailData.ToList();

                                        rentFile.CodeCH = apartment.Code.Length > 0 ? apartment.Code : " ";
                                        customerId = apartment.CustomerId;
                                        rentFile.RentApartmentId = apartment.Id;
                                        if (apartment.UseAreaValue != null) rentFile.UseAreaValueCH = (float)apartment.UseAreaValue;
                                        if (apartment.Address != null && res.FullAddress != null) rentFile.fullAddressCH = apartment.Address + "/" + res.FullAddress;
                                        apartmentDetailDatas = map_apartments.apartmentDetailData;

                                        rentFile.CodeCN = blockParent.Code;
                                        rentFile.RentBlockId = blockParent.Id;
                                        rentFile.TypeBlockId = blockParent.TypeBlockId;
                                        rentFile.UseAreaValueCN = blockParent.UseAreaValue;
                                        if (res.FullAddress != null) rentFile.fullAddressCN = res.FullAddress;
                                        rentFile.TypeHouse = blockParent.TypeHouse;
                                        rentFile.TypeReportApply = blockParent.TypeReportApply;
                                        if (blockParent.CampusArea != null) rentFile.CampusArea = blockParent.CampusArea;
                                        rentFile.ConstructionAreaValue = blockParent.ConstructionAreaValue;
                                        rentFile.DistrictId = blockParent.District;
                                        rentFile.WardId = blockParent.Ward;
                                        rentFile.LaneId = (int)blockParent.Lane;
                                    }
                                }

                                if (item.Valid != false)
                                {
                                    //Kiểm tra thông tin CCCD / CMND nếu chưa tồn tại thì thêm mới
                                    Customer customer = customerData.Where(e => e.Id == customerId && e.Status != EntityStatus.DELETED).FirstOrDefault();
                                    if (customer != null)
                                    {
                                        rentFile.CodeKH = customer.Code;
                                        rentFile.CustomerId = customer.Id;
                                        rentFile.CustomerName = customer.FullName;
                                        rentFile.Phone = customer.Phone;
                                        rentFile.Dob = customer.Dob;
                                        rentFile.AddressKH = customer.Address;

                                        DtDebts.CustomerName = customer.FullName;
                                        DtDebts.Phone = customer.Phone;
                                    }
                                    rentFile.Id = Guid.NewGuid();
                                    rentFile.Code = item.CodeHD;
                                    rentFile.Type = 1;
                                    rentFile.Month = 60;
                                    rentFile.DateHD = item.DateAssign;
                                    rentFile.CodeKH = item.CCCD;
                                    rentFile.CreatedById = userId;
                                    rentFile.CreatedBy = $"{fullName} (Excel)";
                                    rentFile.CreatedAt = DateTime.Now;
                                    resRentFiles.Add(rentFile);

                                    var DebtsEx = _context.DebtsTables.Where(l => l.Code == item.Code && l.RentFileId == rentFile.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                                    if (DebtsEx.Count == 0)
                                    {
                                        if (item.StillDbest.HasValue)
                                        {
                                            DebtsTable deb = new DebtsTable();
                                            deb.Code = item.Code;
                                            deb.Index = 0;
                                            deb.RentFileId = rentFile.Id;
                                            deb.Price = (decimal)item.StillDbest;
                                            deb.Type = 1;
                                            deb.PriceDiff = (decimal)item.StillDbest;
                                            deb.DateStart = item.DateAssign;
                                            deb.DateEnd = item.DateAssign;
                                            deb.SXN = item.ConfirmationNumber;
                                            resDebtsTables.Add(deb);
                                        }

                                        for (int i = 0; i < rentFile.Month; i++)
                                        {
                                            DebtsTable deb = new DebtsTable();
                                            deb.Code = item.Code;
                                            deb.Index = i + 1;
                                            deb.DateStart = item.DateAssign.AddMonths(i);
                                            deb.DateEnd = item.DateAssign.AddMonths(i + 1).AddDays(-1);
                                            deb.RentFileId = rentFile.Id;
                                            deb.Price = (decimal)item.PriceRent;
                                            deb.Type = 1;
                                            deb.PriceDiff = (decimal)item.PriceRent;
                                            deb.SXN = null;
                                            resDebtsTables.Add(deb);
                                        }
                                        var x  = item.StillDbest != null ? item.StillDbest : 0;

                                        DtDebts.Code = rentFile.CodeCN != null ? rentFile.CodeCN : rentFile.CodeCH;
                                        DtDebts.SurplusBalance = 0;
                                        DtDebts.Diff = (item.PriceRent * rentFile.Month) + x;
                                        DtDebts.Total = (item.PriceRent * rentFile.Month) + x;
                                        DtDebts.Paid = 0;
                                        DtDebts.DistrictId = rentFile.DistrictId;
                                        DtDebts.TypeBlockId = rentFile.TypeBlockId;
                                        DtDebts.UsageStatus = rentFile.UsageStatus;
                                        DtDebts.RentFileId = rentFile.Id;
                                        DtDebts.CreatedBy = rentFile.CreatedBy;
                                        resDebts.Add(DtDebts);
                                    }
                                    else
                                    {
                                        item.ErrMsg += "Công nợ của hồ sơ đã tồn tại\n";
                                    }
                                    int GroupId = 0;
                                    int idx = 0;

                                    DateTime dateStart1753 = new DateTime(1992, 11, 1);
                                    DateTime dateEnd1753 = new DateTime(2010, 2, 13);

                                    DateTime dateStart09 = new DateTime(2010, 2, 14);
                                    DateTime dateEnd09 = new DateTime(2018, 7, 11);

                                    DateTime dateStart22 = new DateTime(2018, 7, 12);

                                    TypeQD type = AppEnums.TypeQD.QD_22;

                                    if (dateStart1753 < rentFile.DateHD && rentFile.DateHD < dateEnd1753) type = AppEnums.TypeQD.QD_1753;

                                    if (dateStart09 < rentFile.DateHD && rentFile.DateHD < dateEnd09) type = AppEnums.TypeQD.QD_09;

                                    if (dateStart22 < rentFile.DateHD) type = AppEnums.TypeQD.QD_22;
                                    var rentingPrices = rentingPricesData.Where(v => v.Status != AppEnums.EntityStatus.DELETED && v.TypeQD == type && v.TypeBlockId == rentFile.TypeBlockId)
                                       .Select(v => new rentingPrice
                                       {
                                           LevelId = v.LevelId,
                                           Price = v.Price
                                       })
                                       .ToList();

                                    var dataRentBctTable = importRentBCT(blockDetailsData, apartmentDetailDatas, item, rentFile, fullName, vat, salaryCoefficients, coefficients, rentingPrices);
                                    resRentBctTables.AddRange(dataRentBctTable);
                                }
                            }
                            catch (Exception ex2)
                            {
                                countEr++;
                                log.Error("Import:" + ex2 + "Dòng :" + countlv);
                            }
                        });
                        _context.DebtsTables.AddRange(resDebtsTables);
                        _context.RentFiles.AddRange(resRentFiles);
                        _context.RentBctTables.AddRange(resRentBctTables);
                        _context.debts.AddRange(resDebts);

                        _context.SaveChanges();


                        importHistory.Data = data.Cast<dynamic>().ToList();
                        importHistory.CreatedById = userId;
                        importHistory.CreatedBy = fullName;

                        _context.ImportHistories.Add(importHistory);
                        _context.SaveChanges();

                        transaction.Commit();
                        def.meta = new Meta(200, "Import được" + countlv + ", Lỗi :" + countEr);
                        def.metadata = data.Count;
                        def.data = data;
                    }
                    catch (Exception ex)
                    {
                        log.Error("ImportContractDataExcel:" + ex + "Lỗi :" + countEr);
                        transaction.Rollback();
                        def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                    }
                }

                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("ImportContractDataExcel:" + ex);
                def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                return Ok(def);
            }
        }
        public static List<ContractRentDataImportType2> importContractRentDataType2(MemoryStream ms, int sheetnumber, int rowStart, List<SimpleLaneData> laneData, List<SimpleData> wardData, List<District> districtData, List<SimpleData> typeBlockData, List<SimpleBlockData> blockData, List<SimpleApartmentData> apartmentData, List<Customer> customerData)
        {
            XSSFWorkbook workbook = new XSSFWorkbook(ms);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);

            List<ContractRentDataImportType2> res = new List<ContractRentDataImportType2>();
            for (int row = rowStart; row <= sheet.LastRowNum; row++)
            {
                int rowclom = row;
                var x = sheet.GetRow(row);
                if (sheet.GetRow(row) != null)
                {
                    //Đọc dữ liệu từ từng cell
                    ContractRentDataImportType2 input1Detai = new ContractRentDataImportType2();
                    input1Detai.Valid = true;
                    input1Detai.ErrMsg = "";

                    for (int i = 0; i < 15; i++)
                    {
                        try
                        {
                            var cell = sheet.GetRow(row).GetCell(i, MissingCellPolicy.RETURN_NULL_AND_BLANK);

                            //Lấy giá trị trong cell
                            string str = UtilsService.getCellValue(cell);
                            string format = "dd/MM/yyyy";
                            if (i == 0)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.Index = int.Parse(str);
                                    }
                                }
                                catch
                                {
                                    input1Detai.ErrMsg += "Lỗi cột Số thứ tự \n";
                                }
                            }
                            else if (i == 1)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.CodeHD = str;
                                    }
                                }
                                catch
                                {
                                    input1Detai.ErrMsg += "Lỗi cột số hợp đồng\n";
                                }
                            }
                            else if (i == 2)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        double number = double.Parse(str);
                                        input1Detai.DateAssign = DateTime.FromOADate(number);
                                        if (input1Detai.DateAssign.Year < 1900)
                                        {
                                            input1Detai.Valid = false;
                                            input1Detai.ErrMsg += "Ngày ký không hợp lệ\n";
                                        }
                                    }
                                }
                                catch
                                {
                                    input1Detai.ErrMsg += "Lỗi cột Ngày ký\n";
                                }
                            }
                            else if (i == 3)
                            {
                               
                                    if (str != "")
                                    {
                                        input1Detai.CustomerName = str;
                                    }
                            }
                            else if (i == 4)
                            {

                                if (str != "")
                                {
                                    input1Detai.CCCD = str;
                                }
                            }
                            else if (i == 5)
                            {

                                if (str != "")
                                {
                                    input1Detai.Address = str;
                                }
                            }
                            else if (i == 6)
                            {

                                if (str != "")
                                {
                                    input1Detai.Lane = str;
                                }
                            }
                            else if (i == 7)
                            {

                                if (str != "")
                                {
                                    input1Detai.Ward = str;
                                }
                            }
                            else if (i == 8)
                            {

                                if (str != "")
                                {
                                    input1Detai.Distric = str;
                                }
                            }
                            else if (i == 9)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.Code = str;
                                    }
                                    else
                                    {
                                        input1Detai.ErrMsg += "Cột mã định danh không hợp lệ \n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.ErrMsg += "Lỗi mã định danh\n";
                                }
                            }
                            else if (i == 10)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.PriceRent = decimal.Parse(str);
                                    }
                                    else
                                    {
                                        input1Detai.ErrMsg += "Cột Giá thuê/tháng (đã bao gồm VAT) chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.ErrMsg += "Lỗi cột Giá thuê/tháng (đã bao gồm VAT)\n";
                                }
                            }
                            else if (i == 11)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        double number = double.Parse(str);
                                        input1Detai.PlacementTime = DateTime.FromOADate(number);
                                    }
                                }
                                catch
                                {
                                    input1Detai.ErrMsg += "Thời điểm bố trí\n";
                                }
                            }
                            else if (i == 12)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.ConfirmationNumber = str;
                                    }
                                    else
                                    {
                                        input1Detai.ErrMsg += "Cột Số xác nhận chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.ErrMsg += "Lỗi cột Số xác nhận\n";
                                }
                            }
                            else if (i == 13)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.StillDbest = decimal.Parse(str);
                                    }
                                }
                                catch
                                {
                                    input1Detai.ErrMsg += "Lỗi cột Còn nợ (chưa Vat) Từ quá khứ đến 2023\n";
                                }
                            }
                            else if (i == 14)
                            {
                                try
                                {
                                    input1Detai.Note = str;
                                }
                                catch
                                {
                                    input1Detai.ErrMsg += "Lỗi cột ghi chú\n";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            input1Detai.Valid = false;
                            input1Detai.ErrMsg += "Lỗi dữ liệu\n";
                            log.Error("Exception:" + ex);
                        }
                    }

                    res.Add(input1Detai);
                }
            }

            return res;
        }
        #endregion
        public static List<RentBctTable> importRentBCT(List<BlockDetailData> blockDetailsData, List<ApartmentDetailData> apartmentDetailDatas, ContractRentDataImportType2 input, RentFile rentFile, string fullName, List<ImportDto> vats, List<ImportDto> salaryCoefficients, List<ImportDto> coefficients, List<rentingPrice> rentingPrices)
        {
            List<RentBctTable> res = new List<RentBctTable>();

            if (blockDetailsData.Count > 0)
            {

                int Index = 0;
                blockDetailsData.ForEach(item =>
                {
                    Index++;
                    RentBctTable itemResBctTable = new RentBctTable();
                    if (rentFile.DateHD != null)
                    {
                        itemResBctTable.DateStart = rentFile.DateHD;
                        itemResBctTable.DateEnd = rentFile.DateHD.AddMonths(rentFile.Month);
                        foreach (var itemLv in rentingPrices) //Lấy giá chuẩn
                        {
                            if (itemLv.LevelId == item.Level)
                            {
                                itemResBctTable.StandardPrice = (decimal)itemLv.Price;
                                break;
                            }
                        }

                        foreach (var vat in vats)
                        {
                            if (itemResBctTable.DateStart >= vat.DoApply) // lấy giá trị của VAT theo dateStart
                            {
                                itemResBctTable.VAT = vat.Value;
                                break;
                            }
                        }

                        foreach (var salaryCoefficient in salaryCoefficients) // lấy giá trị của lương cơ bản theo ngày áp dụng
                        {
                            if (itemResBctTable.DateStart >= salaryCoefficient.DoApply)
                            {
                                itemResBctTable.Ktlcb = (decimal)salaryCoefficient.Value;
                                break;
                            }
                        }

                        foreach (var coefficient in coefficients) // lấy giá trị của hệ số thời điểm bố trí theo ngày áp dụng
                        {
                            if (itemResBctTable.DateCoefficient >= coefficient.DoApply)
                            {
                                itemResBctTable.Ktdbt = (decimal)coefficient.Value;
                                break;
                            }
                        }
                    }
                    else
                    {
                        itemResBctTable.StandardPrice = 0;
                        itemResBctTable.Ktdbt = 0;
                        itemResBctTable.Ktlcb = 0;
                        itemResBctTable.VAT = 0;
                    }

                    itemResBctTable.Index = Index;
                    itemResBctTable.Type = 1;
                    itemResBctTable.Id = Guid.NewGuid();
                    itemResBctTable.AreaName = item.AreaName != null ? item.AreaName : "";
                    itemResBctTable.Level = item.Level.HasValue ? item.Level : null;
                    itemResBctTable.PrivateArea = item.PrivateArea.HasValue ? item.PrivateArea : null;
                    itemResBctTable.DateCoefficient = input.PlacementTime.HasValue ? input.PlacementTime : null;
                    itemResBctTable.TotalK = 1;
                    itemResBctTable.PriceRent = input.PriceRent.HasValue ? input.PriceRent : null;
                    itemResBctTable.Unit = "VND";
                    if (itemResBctTable.PriceRent != null && itemResBctTable.PrivateArea != null)
                    {
                        itemResBctTable.PriceRent1m2 = Math.Round((decimal)(itemResBctTable.PriceRent / (decimal)itemResBctTable.PrivateArea));
                    }
                    itemResBctTable.RentFileId = rentFile.Id;
                    itemResBctTable.DiscountCoff = 0;
                    itemResBctTable.PriceAfterDiscount = itemResBctTable.PriceRent - itemResBctTable.DiscountCoff;
                    itemResBctTable.CreatedAt = DateTime.Now;
                    itemResBctTable.CreatedBy = fullName;
                    res.Add(itemResBctTable);
                });
                RentBctTable itemResBctTable = new RentBctTable();
                itemResBctTable.Index = Index++;
                itemResBctTable.Type = 1;
                itemResBctTable.Id = Guid.NewGuid();
                itemResBctTable.AreaName = "Tổng";
                itemResBctTable.Unit = "VND";
                itemResBctTable.RentFileId = rentFile.Id;
                itemResBctTable.PriceAfterDiscount = res.Sum(p => p.PriceAfterDiscount);
                itemResBctTable.CreatedAt = DateTime.Now;
                itemResBctTable.CreatedBy = fullName;
                res.Add(itemResBctTable);
            }
            else if (apartmentDetailDatas.Count > 0)
            {
                int Index = 0;
                apartmentDetailDatas.ForEach(item =>
                {
                    Index++;
                    RentBctTable itemResBctTable = new RentBctTable();
                    if (rentFile.DateHD != null)
                    {
                        itemResBctTable.DateStart = rentFile.DateHD;
                        itemResBctTable.DateEnd = rentFile.DateHD.AddMonths(rentFile.Month);

                        foreach (var itemLv in rentingPrices) //Lấy giá chuẩn
                        {
                            if (itemLv.LevelId == item.Level)
                            {
                                itemResBctTable.StandardPrice = (decimal)itemLv.Price;
                                break;
                            }
                        }

                        foreach (var vat in vats)
                        {
                            if (itemResBctTable.DateStart >= vat.DoApply) // lấy giá trị của VAT theo dateStart
                            {
                                itemResBctTable.VAT = vat.Value;
                                break;
                            }
                        }

                        foreach (var salaryCoefficient in salaryCoefficients) // lấy giá trị của lương cơ bản theo ngày áp dụng
                        {
                            if (itemResBctTable.DateStart >= salaryCoefficient.DoApply)
                            {
                                itemResBctTable.Ktlcb = (decimal)salaryCoefficient.Value;
                                break;
                            }
                        }

                        foreach (var coefficient in coefficients) // lấy giá trị của hệ số thời điểm bố trí theo ngày áp dụng
                        {
                            if (itemResBctTable.DateCoefficient >= coefficient.DoApply)
                            {
                                itemResBctTable.Ktdbt = (decimal)coefficient.Value;
                                break;
                            }
                        }
                    }
                    else
                    {
                        itemResBctTable.StandardPrice = 0;
                        itemResBctTable.Ktdbt = 0;
                        itemResBctTable.Ktlcb = 0;
                        itemResBctTable.VAT = 0;
                    }

                    itemResBctTable.Index = Index;
                    itemResBctTable.Type = 1;
                    itemResBctTable.Id = Guid.NewGuid();
                    itemResBctTable.AreaName = item.AreaName != null ? item.AreaName : "";
                    itemResBctTable.Level = item.Level.HasValue ? item.Level : null;
                    itemResBctTable.PrivateArea = item.PrivateArea.HasValue ? item.PrivateArea : null;
                    itemResBctTable.DateCoefficient = input.PlacementTime.HasValue ? input.PlacementTime : null;
                    itemResBctTable.TotalK = 1;
                    itemResBctTable.PriceRent = input.PriceRent.HasValue ? input.PriceRent : null;
                    itemResBctTable.Unit = "VND";
                    if (itemResBctTable.PriceRent != null && itemResBctTable.PrivateArea != null)
                    {
                        itemResBctTable.PriceRent1m2 = Math.Round((decimal)(itemResBctTable.PriceRent / (decimal)itemResBctTable.PrivateArea));
                    }
                    itemResBctTable.RentFileId = rentFile.Id;
                    itemResBctTable.DiscountCoff = 0;
                    itemResBctTable.PriceAfterDiscount = itemResBctTable.PriceRent - itemResBctTable.DiscountCoff;
                    itemResBctTable.CreatedAt = DateTime.Now;
                    itemResBctTable.CreatedBy = fullName;
                    res.Add(itemResBctTable);
                });
                RentBctTable itemResBctTable = new RentBctTable();
                itemResBctTable.Index = Index++;
                itemResBctTable.Type = 1;
                itemResBctTable.Id = Guid.NewGuid();
                itemResBctTable.AreaName = "Tổng";
                itemResBctTable.Unit = "VND";
                itemResBctTable.RentFileId = rentFile.Id;
                itemResBctTable.PriceAfterDiscount = res.Sum(p => p.PriceAfterDiscount);
                itemResBctTable.CreatedAt = DateTime.Now;
                itemResBctTable.CreatedBy = fullName;
                res.Add(itemResBctTable);
            }
            else if (blockDetailsData.Count == 0 && apartmentDetailDatas.Count == 0)
            {
                RentBctTable itemResBctTable = new RentBctTable();
                if (rentFile.DateHD != null)
                {
                    itemResBctTable.DateStart = rentFile.DateHD;
                    itemResBctTable.DateEnd = rentFile.DateHD.AddMonths(60);
                    foreach (var vat in vats)
                    {
                        if (itemResBctTable.DateStart >= vat.DoApply) // lấy giá trị của VAT theo dateStart
                        {
                            itemResBctTable.VAT = vat.Value;
                            break;
                        }
                    }

                    foreach (var salaryCoefficient in salaryCoefficients) // lấy giá trị của lương cơ bản theo ngày áp dụng
                    {
                        if (itemResBctTable.DateStart >= salaryCoefficient.DoApply)
                        {
                            itemResBctTable.Ktlcb = (decimal)salaryCoefficient.Value;
                            break;
                        }
                    }

                    foreach (var coefficient in coefficients) // lấy giá trị của hệ số thời điểm bố trí theo ngày áp dụng
                    {
                        if (itemResBctTable.DateCoefficient >= coefficient.DoApply)
                        {
                            itemResBctTable.Ktdbt = (decimal)coefficient.Value;
                            break;
                        }
                    }
                }
                else
                {
                    itemResBctTable.StandardPrice = 0;
                    itemResBctTable.Ktdbt = 0;
                    itemResBctTable.Ktlcb = 0;
                    itemResBctTable.VAT = 0;
                }
                itemResBctTable.Index = 1;
                itemResBctTable.Type = 1;
                itemResBctTable.Id = Guid.NewGuid();
                itemResBctTable.DateCoefficient = input.PlacementTime.HasValue ? input.PlacementTime : null;
                itemResBctTable.TotalK = 1;
                itemResBctTable.PriceRent = input.PriceRent.HasValue ? input.PriceRent : null;
                itemResBctTable.Unit = "VND";
                itemResBctTable.RentFileId = rentFile.Id;
                itemResBctTable.DiscountCoff = 0;
                itemResBctTable.PriceAfterDiscount = itemResBctTable.PriceRent - itemResBctTable.DiscountCoff;
                itemResBctTable.CreatedAt = DateTime.Now;
                itemResBctTable.CreatedBy = fullName;
                res.Add(itemResBctTable);

                ///Add Dòng tổng
                RentBctTable itemResBctTableTotal = new RentBctTable();
                itemResBctTableTotal.Index = 2;
                itemResBctTableTotal.Type = 1;
                itemResBctTableTotal.Id = Guid.NewGuid();
                itemResBctTableTotal.AreaName = "Tổng";
                itemResBctTableTotal.Unit = "VND";
                itemResBctTableTotal.RentFileId = rentFile.Id;
                itemResBctTableTotal.PriceAfterDiscount = res.Sum(p => p.PriceAfterDiscount);
                itemResBctTableTotal.CreatedAt = DateTime.Now;
                itemResBctTableTotal.CreatedBy = fullName;
                res.Add(itemResBctTableTotal);
            }
            return res;
        }
    }
}
