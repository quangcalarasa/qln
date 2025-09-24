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
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace IOITQln.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class Md167ReceiptController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("Md167Receipt", "Md167Receipt");
        private static string functionCode = "Md167_CONTRACT_MANAGEMENT";
        private readonly ApiDbContext _context;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;
        private IMapper _mapper;

        public Md167ReceiptController(ApiDbContext context, IMapper mapper, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
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
                    IQueryable<Md167Receipt> data = _context.Md167Receipts.Where(c => c.Status != AppEnums.EntityStatus.DELETED);
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
            catch(Exception ex)
            {
                log.Error("GetByPage Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        // GET: api/Md167Receipt/1
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
                Md167Receipt data = await _context.Md167Receipts.FindAsync(id);

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

        // POST: api/Md167Receipt
        [HttpPost]
        public async Task<IActionResult> Post(Md167Receipt input)
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
                input = (Md167Receipt)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.CreatedById = userId;
                    input.CreatedBy = fullName;
                    _context.Md167Receipts.Add(input);

                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Thêm mới phiếu thu ", "Md167Receipt", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        //Kiểm tra đóng đủ tiền thế chân thì đổi trạng thái tiền thế chân của hợp đồng
                        if(input.PaidDeposit == true)
                        {
                            Md167Contract md167Contract = _context.Md167Contracts.Where(m => m.Id == input.Md167ContractId && m.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            if(md167Contract != null && md167Contract.PaidDeposit != true)
                            {
                                //Lấy ra só tiền thế chân
                                var pricePerMonth = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == input.Md167ContractId && l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(x => x.UpdatedAt).FirstOrDefault();
                                if (pricePerMonth != null)
                                {
                                    //Tổng số tiền thế chân đã nộp
                                    var totalAmount = _context.Md167Receipts.Where(m => m.Md167ContractId == input.Md167ContractId && m.PaidDeposit == true && m.Status != AppEnums.EntityStatus.DELETED).Sum(x => x.Amount);
                                    if(totalAmount + input.Amount >= pricePerMonth.TotalPrice * 3)
                                    {
                                        md167Contract.PaidDeposit = true;
                                        _context.Update(md167Contract);
                                        await _context.SaveChangesAsync();
                                    }
                                }
                                else
                                {
                                    md167Contract.PaidDeposit = true;
                                    _context.Update(md167Contract);
                                    await _context.SaveChangesAsync();
                                }
                            }
                            else
                            {
                                transaction.Rollback();
                                def.meta = new Meta(404, md167Contract == null ? "Không tìm thấy hợp đồng!" : "Hợp đồng đã được thanh toán đủ tiền thế chân!");
                                def.data = input;
                                return Ok(def);
                            }
                        }

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
                            def.data = input;
                            return Ok(def);
                        }
                    }
                    catch (DbUpdateException e)
                    {
                        log.Error("DbUpdateException:" + e);
                        transaction.Rollback();
                        if (Md167ReceiptExists(input.Id))
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

        [HttpPut("UpdateList/{id}")]
        public async Task<IActionResult> UpdateList(List<Md167Receipt> input, int id)
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
                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                if(input == null)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    List<Md167Receipt> md167Receipts = _context.Md167Receipts.Where(m => m.Md167ContractId == id && m.Status != AppEnums.EntityStatus.DELETED).AsNoTracking().ToList();

                    if (input.Count > 0)
                    {
                        foreach(var md167Receipt in input)
                        {
                            Md167Receipt md167ReceiptExist = md167Receipts.Where(m => m.Id == md167Receipt.Id).FirstOrDefault();
                            if(md167ReceiptExist == null)
                            {
                                md167Receipt.Md167ContractId = id;
                                md167Receipt.CreatedBy = fullName;
                                md167Receipt.CreatedById = userId;

                                _context.Md167Receipts.Add(md167Receipt);
                            }
                            else
                            {
                                md167Receipt.CreatedAt = md167ReceiptExist.CreatedAt;
                                md167Receipt.CreatedBy = md167ReceiptExist.CreatedBy;
                                md167Receipt.CreatedById = md167ReceiptExist.CreatedById;
                                md167Receipt.Md167ContractId = id;
                                md167Receipt.UpdatedBy = fullName;
                                md167Receipt.UpdatedById = userId;
                                md167Receipt.UpdatedAt = DateTime.Now;

                                _context.Update(md167Receipt);

                                md167Receipts.Remove(md167ReceiptExist);
                            }
                        }
                    }

                    md167Receipts.ForEach(item =>
                    {
                        item.UpdatedAt = DateTime.Now;
                        item.UpdatedById = userId;
                        item.UpdatedBy = fullName;
                        item.Status = AppEnums.EntityStatus.DELETED;
                    });
                    _context.UpdateRange(md167Receipts);

                    try
                    {
                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Thêm nhiều phiếu thu ", "Md167Receipt", 0, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        transaction.Commit();
                        def.meta = new Meta(200, ApiConstants.MessageResource.ADD_SUCCESS);
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
            catch (Exception e)
            {
                log.Error("Post Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        // PUT: api/Md167Receipt/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Md167Receipt input)
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
                input = (Md167Receipt)UtilsService.TrimStringPropertyTypeObject(input);

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

                Md167Receipt data = await _context.Md167Receipts.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
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

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Sửa phiếu thu ", "Md167Receipt", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        //Kiểm tra đóng đủ tiền thế chân thì đổi trạng thái tiền thế chân của hợp đồng
                        Md167Contract md167Contract = _context.Md167Contracts.Where(m => m.Id == input.Md167ContractId && m.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                        var pricePerMonth = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == input.Md167ContractId && l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(x => x.UpdatedAt).FirstOrDefault();
                        var totalAmount = _context.Md167Receipts.Where(m => m.Md167ContractId == input.Md167ContractId && m.PaidDeposit == true && m.Id != id && m.Status != AppEnums.EntityStatus.DELETED).Sum(x => x.Amount);

                        if(md167Contract != null)
                        {
                            if(totalAmount + input.Amount >= pricePerMonth.TotalPrice * 3)
                            {
                                md167Contract.PaidDeposit = true;
                                _context.Update(md167Contract);
                                await _context.SaveChangesAsync();
                            }
                            else
                            {
                                md167Contract.PaidDeposit = false;
                                _context.Update(md167Contract);
                                await _context.SaveChangesAsync();
                            }
                        }
                        else
                        {
                            transaction.Rollback();
                            def.meta = new Meta(404,"Không tìm thấy hợp đồng!");
                            def.data = input;
                            return Ok(def);
                        }

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
                            def.data = input;
                            return Ok(def);
                        }

                        def.meta = new Meta(200, ApiConstants.MessageResource.UPDATE_SUCCESS);
                        def.data = data;
                        return Ok(def);
                    }
                    catch (DbUpdateException e)
                    {
                        transaction.Rollback();
                        log.Error("DbUpdateException:" + e);
                        if (!Md167ReceiptExists(data.Id))
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

        // DELETE: api/Md167Receipt/1
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
                //QFix:Fix bug k xóa được phiếu thu
                //Md167Receipt data = await _context.Md167Receipts.FindAsync(id);
                Md167Receipt data = await _context.Md167Receipts.FindAsync(long.Parse(id.ToString()));
                if(data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_DELETE_MESSAGE);
                    return Ok(def);
                }

                //Kiểm tra dữ liệu liên quan có tồn tại thì không thể xóa
                //Chi tiết tầng
                Area area = _context.Areas.Where(a => a.FloorId == id && a.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                if(area != null)
                {
                    def.meta = new Meta(222, "Dữ liệu liên quan còn tồn tại (Area). Không thể xóa bản ghi này!");
                    return Ok(def);
                }

                //Dữ liệu bảng UseValueCoefficientItem
                UseValueCoefficientItem useValueCoefficientItem = _context.UseValueCoefficientItems.Where(u => u.FloorId == id && u.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                if (useValueCoefficientItem != null)
                {
                    def.meta = new Meta(222, "Dữ liệu liên quan còn tồn tại (UseValueCoefficientItem). Không thể xóa bản ghi này!");
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
                        LogActionModel logActionModel = new LogActionModel("Xóa phiếu thu ", "Md167Receipt", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.DELETED, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if(data.PaidDeposit == true)
                        {
                            //Kiểm tra đóng đủ tiền thế chân thì đổi trạng thái tiền thế chân của hợp đồng
                            Md167Contract md167Contract = _context.Md167Contracts.Where(m => m.Id == data.Md167ContractId && m.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            var pricePerMonth = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == data.Md167ContractId && l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(x => x.UpdatedAt).FirstOrDefault();
                            var totalAmount = _context.Md167Receipts.Where(m => m.Md167ContractId == data.Md167ContractId && m.PaidDeposit == true && m.Id != id && m.Status != AppEnums.EntityStatus.DELETED).Sum(x => x.Amount);

                            if (md167Contract != null)
                            {
                                if (totalAmount >= pricePerMonth.TotalPrice * 3)
                                {
                                    md167Contract.PaidDeposit = true;
                                    _context.Update(md167Contract);
                                    await _context.SaveChangesAsync();
                                }
                                else
                                {
                                    md167Contract.PaidDeposit = false;
                                    _context.Update(md167Contract);
                                    await _context.SaveChangesAsync();
                                }
                            }
                            else
                            {
                                transaction.Rollback();
                                def.meta = new Meta(404, "Không tìm thấy hợp đồng!");
                                def.data = null;
                                return Ok(def);
                            }
                        }

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
                        if (!Md167ReceiptExists(data.Id))
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

        #region import phiếu thu từ excel
        [HttpPost]
        [Route("ImportDataExcel")]
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
                importHistory.Type = AppEnums.ImportHistoryType.Md167Receipt;

                List<Md167ReceiptData> data = new List<Md167ReceiptData>();

                //Lấy dữ liệu từ file excel và lưu lại file
                foreach (var file in httpRequest)
                {
                    var postedFile = httpRequest[i];
                    if (postedFile != null && postedFile.Length > 0)
                    {
                        IList<string> AllowedDocuments = new List<string> {".xls", ".xlsx" };
                        int MaxContentLength = 1024 * 1024 * 32; //Size = 32 MB  

                        var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                        var name = postedFile.FileName.Substring(0, postedFile.FileName.LastIndexOf('.'));
                        var extension = ext.ToLower();

                        bool checkFile = true;
                        if (AllowedDocuments.Contains(extension))
                        {
                            checkFile = false;
                        }

                        if(checkFile)
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
                            string folderName = _configuration["AppSettings:BaseUrlImportHistory"]; ;
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

                List<Md167ReceiptData> dataValid = new List<Md167ReceiptData>();
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var dataItem in data)
                        {
                            if (dataItem.Valid == true)
                            {
                                //Check Mã hợp đồng/phụ lục
                                Md167Contract md167Contract = _context.Md167Contracts.Where(m => m.Code == dataItem.ContractCode && m.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                if (md167Contract == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy Hợp đồng/phụ lục hợp đồng";
                                }
                                else
                                {
                                    //Kiểm tra xem Mã hóa đơn có tồn tại chưa
                                    Md167Receipt codeExist = _context.Md167Receipts.Where(m => m.ReceiptCode == dataItem.ReceiptCode && m.Md167ContractId == md167Contract.Id && m.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                    if (codeExist == null)
                                    {
                                        dataItem.Md167ContractId = md167Contract.Id;
                                        dataItem.CreatedById = -1;
                                        dataValid.Add(dataItem);
                                    }
                                    else
                                    {
                                        dataItem.Valid = false;
                                        dataItem.ErrMsg += "Mã hóa đơn đã tồn tại";
                                    }
                                }
                            }
                        }

                        importHistory.Data = data.Cast<dynamic>().ToList();
                        importHistory.CreatedById = userId;
                        importHistory.CreatedBy = fullName;

                        _context.ImportHistories.Add(importHistory);
                        _context.Md167Receipts.AddRange(dataValid);

                        _context.SaveChanges();

                        transaction.Commit();
                        def.meta = new Meta(200, ApiConstants.MessageResource.DELETE_SUCCESS);
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

        public static List<Md167ReceiptData> importData(MemoryStream ms, int sheetnumber, int rowStart)
        {
            XSSFWorkbook workbook = new XSSFWorkbook(ms);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);

            List<Md167ReceiptData> res = new List<Md167ReceiptData>();
            for (int row = rowStart; row <= sheet.LastRowNum; row++)
            {
                if (sheet.GetRow(row) != null)
                {
                    //Đọc dữ liệu từ từng cell
                    Md167ReceiptData input1Detai = new Md167ReceiptData();
                    input1Detai.Valid = true;
                    input1Detai.ErrMsg = "";

                    for (int i = 0; i < 6; i++)
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
                                    if(str != "")
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
                                        input1Detai.ReceiptCode = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Mã hóa đơn chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Mã hóa đơn\n";
                                }
                            }
                            else if (i == 2)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.DateOfPayment = DateTime.FromOADate(Double.Parse(str));
                                        if (input1Detai.DateOfPayment.Value.Year < 1900)
                                        {
                                            input1Detai.DateOfPayment = null;
                                            input1Detai.Valid = false;
                                            input1Detai.ErrMsg += "Ngày nộp tiền không hợp lệ\n";
                                        }
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Ngày nộp tiền chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Ngày nộp tiền\n";
                                }
                            }
                            else if (i == 3)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.DateOfReceipt = DateTime.FromOADate(Double.Parse(str));
                                        if (input1Detai.DateOfReceipt.Value.Year < 1900)
                                        {
                                            input1Detai.DateOfReceipt = null;
                                            input1Detai.Valid = false;
                                            input1Detai.ErrMsg += "Ngày xuất hóa đơn không hợp lệ\n";
                                        }
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Ngày xuất hóa đơn chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Ngày xuất hóa đơn\n";
                                }
                            }
                            else if (i == 4)
                            {
                                try
                                {
                                    if(str != "")
                                    {
                                        input1Detai.Amount = decimal.Parse(str);
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Số tiền thanh toán chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Số tiền thanh toán\n";
                                }
                            }
                            else if (i == 5)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.ContractCode = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Số hợp đồng/Phụ lục chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Số hợp đồng/Phụ lục\n";
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

        private bool Md167ReceiptExists(long id)
        {
            return _context.Floors.Count(e => e.Id == id) > 0;
        }
    }
}
