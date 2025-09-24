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
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net.Http;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;
using System.Text;
using System.Net;
using static IOITQln.Common.Enums.AppEnums;
using Microsoft.Net.Http.Headers;

namespace IOITQln.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProcessProfileCeController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("ProcessProfileCe", "ProcessProfileCe");
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        private readonly IConfiguration _configuration;

        public ProcessProfileCeController(ApiDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        [Authorize]
        [HttpGet("GetByPage")]
        public IActionResult GetByPage([FromQuery] FilteredPagination paging)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            DefaultResponse def = new DefaultResponse();

            var identity = (ClaimsIdentity)User.Identity;
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            //if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            //{
            //    def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
            //    return Ok(def);
            //}

            try
            {
                if (paging != null)
                {
                    def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                    IQueryable<ProcessProfileCe> data = _context.ProcessProfileCes.Where(c => c.Status != AppEnums.EntityStatus.DELETED);
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

        // GET: api/ProcessProfileCe/1
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            DefaultResponse def = new DefaultResponse();
            //check role
            var identity = (ClaimsIdentity)User.Identity;
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            //if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            //{
            //    def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
            //    return Ok(def);
            //}

            try
            {
                ProcessProfileCe data = await _context.ProcessProfileCes.FindAsync(id);

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

        // POST: api/ProcessProfileCe
        [HttpPost]
        public async Task<IActionResult> Post(ProcessProfileCeData input)
        {
            DefaultResponse def = new DefaultResponse();

            try
            {
                input = (ProcessProfileCeData)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                if (input.Code == null || input.Code == "" || input.IdServiceRecord == null || input.IdServiceRecord == "" || input.CodeIdentify == null || input.CodeIdentify == "" || input.SecretKeyCe == null || input.SecretKeyCe == "")
                {
                    def.meta = new Meta(400, "Thiếu dữ liệu!");
                    return Ok(def);
                }

                //check secret key valid
                string secretKeyCe = _configuration["SecretKeyCe"];
                secretKeyCe = UtilsService.GetMD5Hash(secretKeyCe) + input.IdServiceRecord;
                secretKeyCe = UtilsService.GetMD5Hash(secretKeyCe);

                if (secretKeyCe != input.SecretKeyCe)
                {
                    def.meta = new Meta(400, "SecretKey không hợp lệ!");
                    return Ok(def);
                }

                ProcessProfileCe idServiceRecordExist = await _context.ProcessProfileCes.AsNoTracking().Where(e => e.IdServiceRecord == input.IdServiceRecord && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
                if (idServiceRecordExist != null)
                {
                    def.meta = new Meta(212, "IdServiceRecord đã tồn tại!");
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.CreatedById = 0;
                    input.CreatedBy = "Ce Api";
                    _context.ProcessProfileCes.Add(input);

                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Thêm mới mã hồ sơ CE", "ProcessProfileCe", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, 0, "Ce Api");
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
                        if (ProcessProfileCeExists(input.Id))
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

        // PUT: api/ProcessProfileCe
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] ProcessProfileCeData input)
        {
            DefaultResponse def = new DefaultResponse();

            try
            {
                input = (ProcessProfileCeData)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                if (input.Code == null || input.Code == "" || input.IdServiceRecord == null || input.IdServiceRecord == "" || input.CodeIdentify == null || input.CodeIdentify == "" || input.SecretKeyCe == null || input.SecretKeyCe == "")
                {
                    def.meta = new Meta(400, "Thiếu dữ liệu!");
                    return Ok(def);
                }

                //check secret key valid
                string secretKeyCe = _configuration["SecretKeyCe"];
                secretKeyCe = UtilsService.GetMD5Hash(secretKeyCe) + input.IdServiceRecord;
                secretKeyCe = UtilsService.GetMD5Hash(secretKeyCe);

                if (secretKeyCe != input.SecretKeyCe)
                {
                    def.meta = new Meta(400, "SecretKey không hợp lệ!");
                    return Ok(def);
                }

                ProcessProfileCe data = await _context.ProcessProfileCes.AsNoTracking().Where(e => e.IdServiceRecord == input.IdServiceRecord && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_UPDATE_MESSAGE);
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.Id = data.Id;
                    input.UpdatedAt = DateTime.Now;
                    input.UpdatedById = 0;
                    input.UpdatedBy = "Ce Api";
                    input.CreatedAt = data.CreatedAt;
                    input.CreatedBy = data.CreatedBy;
                    input.CreatedById = data.CreatedById;
                    input.Status = data.Status;
                    _context.Update(input);

                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Sửa thông tin mã hồ sơ CE", "ProcessProfileCe", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, 0, "Ce Api");
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
                        if (!ProcessProfileCeExists(data.Id))
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

        // DELETE: api/ProcessProfileCe/1
        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] ProcessProfileCeData input)
        {
            DefaultResponse def = new DefaultResponse();

            input = (ProcessProfileCeData)UtilsService.TrimStringPropertyTypeObject(input);

            if (!ModelState.IsValid)
            {
                def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                return Ok(def);
            }

            if (input.IdServiceRecord == null || input.IdServiceRecord == "" || input.SecretKeyCe == null || input.SecretKeyCe == "")
            {
                def.meta = new Meta(400, "Thiếu dữ liệu!");
                return Ok(def);
            }

            //check secret key valid
            string secretKeyCe = _configuration["SecretKeyCe"];
            secretKeyCe = UtilsService.GetMD5Hash(secretKeyCe) + input.IdServiceRecord;
            secretKeyCe = UtilsService.GetMD5Hash(secretKeyCe);

            if (secretKeyCe != input.SecretKeyCe)
            {
                def.meta = new Meta(400, "SecretKey không hợp lệ!");
                return Ok(def);
            }

            try
            {
                ProcessProfileCe data = await _context.ProcessProfileCes.Where(e => e.IdServiceRecord == input.IdServiceRecord).FirstOrDefaultAsync();
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_DELETE_MESSAGE);
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    data.UpdatedAt = DateTime.Now;
                    data.UpdatedById = 0;
                    data.UpdatedBy = "Ce api";
                    data.Status = AppEnums.EntityStatus.DELETED;
                    _context.Update(data);

                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Xóa thông tin hồ sơ CE ", "ProcessProfileCes", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.DELETED, 0, "Ce Api");
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
                        if (!ProcessProfileCeExists(data.Id))
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

        //API get danh sách Mã định danh
        [HttpPost("GetListCode")]
        public async Task<IActionResult> GetListCode()
        {
            try
            {
                var blocks = _context.Blocks.Where(b => b.TypeReportApply == AppEnums.TypeReportApply.NHA_RIENG_LE && b.Status != AppEnums.EntityStatus.DELETED).Select(e => new
                {
                    Id = e.Code,
                    Name = e.Code
                }).Distinct().ToList();

                var apartments = _context.Apartments.Where(a => a.Status != AppEnums.EntityStatus.DELETED).Select(e => new
                {
                    Id = e.Code,
                    Name = e.Code
                }).Distinct().ToList();

                var data = blocks.Concat(apartments);

                dynamic res = new System.Dynamic.ExpandoObject();
                res.Status = 1;
                res.Messages = new List<dynamic>();
                res.Data = new System.Dynamic.ExpandoObject();
                res.Data.Categories = data;

                return Ok(res);

            }
            catch
            {
                dynamic res = new System.Dynamic.ExpandoObject();
                res.Status = 0;
                res.Data = new System.Dynamic.ExpandoObject();

                return Ok(res);
            }
        }

        //API replace thông tin file từ link file tải lên
        [HttpPost("DownloadFile")]
        public async Task<IActionResult> DownloadFile([FromBody] ProcessProfileCeData input)
        {
            DefaultResponse def = new DefaultResponse();

            input = (ProcessProfileCeData)UtilsService.TrimStringPropertyTypeObject(input);

            if (!ModelState.IsValid)
            {
                def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                return Ok(def);
            }

            //if (input.IdServiceRecord == null || input.IdServiceRecord == "" || input.SecretKeyCe == null || input.SecretKeyCe == "" || input.LinkTemplate == null || input.LinkTemplate == "")
            //{
            //    def.meta = new Meta(400, "Thiếu dữ liệu!");
            //    return Ok(def);
            //}

            //check secret key valid
            string secretKeyCe = _configuration["SecretKeyCe"];
            secretKeyCe = UtilsService.GetMD5Hash(secretKeyCe) + input.IdServiceRecord;
            secretKeyCe = UtilsService.GetMD5Hash(secretKeyCe);

            if (secretKeyCe != input.SecretKeyCe)
            {
                def.meta = new Meta(400, "SecretKey không hợp lệ!");
                return Ok(def);
            }

            try
            {
                //Download file, replace thông tin, tạo file đã thay đổi thông tin
                string path = "";

                try
                {
                    path = "template" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";
                    RichEditDocumentServer wordProcessor = new RichEditDocumentServer();
                    //wordProcessor.LoadDocumentTemplate(input.LinkTemplate, DocumentFormat.OpenXml);
                    WebRequest request = WebRequest.Create(input.LinkTemplate);
                    using (var response = request.GetResponse())
                    {
                        using (var ms = response.GetResponseStream())
                        {
                            wordProcessor.LoadDocument(ms);
                        }
                    }

                    var document = wordProcessor.Document;
                    document.BeginUpdate();

                    #region Replace thông tin trên file

                    //Kiểm tra xem nhà ở cũ - Tính giá bán có biên bản nào tương ứng thì replace thông tin
                    Pricing pricing = _context.Pricings.Where(e => e.ProcessProfileCeCode == input.Code && e.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(x => x.CreatedAt).FirstOrDefault();
                    if (pricing != null)
                    {
                        Block block = _context.Blocks.Where(b => b.Id == pricing.BlockId && b.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                        Apartment apartment = _context.Apartments.Where(a => a.Id == pricing.ApartmentId && a.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                        if (block != null)
                        {
                            DateTime dateTime = DateTime.Now;
                            document.ReplaceAll("{ngay}", dateTime.Day < 10 ? "0" + dateTime.Day.ToString() : dateTime.Day.ToString(), SearchOptions.None);
                            document.ReplaceAll("{thang}", dateTime.Month < 10 ? "0" + dateTime.Month.ToString() : dateTime.Month.ToString(), SearchOptions.None);
                            document.ReplaceAll("{nam}", dateTime.Year.ToString(), SearchOptions.None);

                            string canho = "";
                            if (pricing.TypeReportApply == AppEnums.TypeReportApply.NHA_RIENG_LE || (pricing.TypeReportApply == AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_LIEN_KE && block.TypeReportApply == AppEnums.TypeReportApply.NHA_RIENG_LE))
                            {
                                canho = block.Address;
                            }
                            else
                            {
                                canho = apartment.Address;
                            }
                            document.ReplaceAll("{diachicanho}", canho, SearchOptions.None);

                            Lane lane = _context.Lanies.Where(l => l.Id == block.Lane && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            document.ReplaceAll("{duong}", lane != null ? lane.Name : "", SearchOptions.None);

                            Ward ward = _context.Wards.Where(l => l.Id == block.Ward && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            document.ReplaceAll("{phuongxa}", ward != null ? ward.Name : "", SearchOptions.None);

                            District district = _context.Districts.Where(l => l.Id == block.District && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            document.ReplaceAll("{quanhuyen}", district != null ? district.Name : "", SearchOptions.None);

                            Customer customer = (from c in _context.Customers
                                                 join pc in _context.PricingCustomers on c.Id equals pc.CustomerId
                                                 where c.Status != EntityStatus.DELETED
                                                 && pc.Status != EntityStatus.DELETED
                                                 && pc.PricingId == pricing.Id
                                                 select c).FirstOrDefault();

                            document.ReplaceAll("{ongba}", customer != null ? customer.FullName : "", SearchOptions.None);
                            document.ReplaceAll("{diachi}", customer != null ? (customer.Address ?? "") : "", SearchOptions.None);
                            document.ReplaceAll("{ngaysinh}", customer != null ? (customer.Dob != null ? customer.Dob.Value.ToString("dd/MM/yyyy") : "") : "", SearchOptions.None);
                            document.ReplaceAll("{cancuoc}", customer != null ? (customer.Code ?? "") : "", SearchOptions.None);

                            var doc = customer != null ? customer.Doc : null;
                            document.ReplaceAll("{ngaycap}", doc != null ? doc.Value.Day.ToString() : "", SearchOptions.None);
                            document.ReplaceAll("{thangcap}", doc != null ? doc.Value.Month.ToString() : "", SearchOptions.None);
                            document.ReplaceAll("{namcap}", doc != null ? doc.Value.Year.ToString() : "", SearchOptions.None);
                            document.ReplaceAll("{noicap}", customer != null ? (customer.PlaceCode ?? "") : "", SearchOptions.None);

                            //Loại nhà
                            TypeBlock typeBlock = _context.TypeBlocks.Where(t => t.Id == block.TypeBlockId && t.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            document.ReplaceAll("{loainha}", typeBlock != null ? typeBlock.Name : "", SearchOptions.None);
                        }
                    }
                    //Tính giá thuê nhà ở cũ
                    else
                    {
                        RentFile rentFile = _context.RentFiles.Where(r => r.ProcessProfileCeCode == input.Code && r.Status != EntityStatus.DELETED).OrderByDescending(x => x.CreatedAt).FirstOrDefault();
                        if (rentFile != null)
                        {
                            DateTime dateTime = DateTime.Now;
                            document.ReplaceAll("{ngay}", dateTime.Day < 10 ? "0" + dateTime.Day.ToString() : dateTime.Day.ToString(), SearchOptions.None);
                            document.ReplaceAll("{thang}", dateTime.Month < 10 ? "0" + dateTime.Month.ToString() : dateTime.Month.ToString(), SearchOptions.None);
                            document.ReplaceAll("{nam}", dateTime.Year.ToString(), SearchOptions.None);
                            document.ReplaceAll("{ngaythangnam}", dateTime.ToString("dd/MM/yyyy"), SearchOptions.None);

                            Block block = _context.Blocks.Where(b => b.Id == rentFile.RentBlockId && b.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            Apartment apartment = _context.Apartments.Where(a => a.Id == rentFile.RentApartmentId && a.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                            string canho = "";
                            if (rentFile.TypeReportApply == TypeReportApply.NHA_RIENG_LE)
                            {
                                canho = block.Address;
                            }
                            else
                            {
                                canho = apartment.Address;
                            }

                            document.ReplaceAll("{diachicanho}", canho, SearchOptions.None);

                            Lane lane = _context.Lanies.Where(l => l.Id == block.Lane && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            document.ReplaceAll("{duong}", lane != null ? lane.Name : "", SearchOptions.None);

                            Ward ward = _context.Wards.Where(l => l.Id == block.Ward && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            document.ReplaceAll("{phuongxa}", ward != null ? ward.Name : "", SearchOptions.None);

                            District district = _context.Districts.Where(l => l.Id == block.District && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            document.ReplaceAll("{quanhuyen}", district != null ? district.Name : "", SearchOptions.None);

                            Customer customer = _context.Customers.Where(c => c.Id == rentFile.CustomerId).FirstOrDefault();

                            document.ReplaceAll("{ongba}", customer != null ? customer.FullName : "", SearchOptions.None);
                            document.ReplaceAll("{diachi}", rentFile != null ? (rentFile.AddressKH ?? "") : "", SearchOptions.None);
                            document.ReplaceAll("{ngaysinh}", rentFile != null ? (rentFile.Dob != null ? rentFile.Dob.Value.ToString("dd/MM/yyyy") : "") : "", SearchOptions.None);
                            document.ReplaceAll("{cancuoc}", rentFile != null ? (rentFile.CodeKH ?? "") : "", SearchOptions.None);
                        }
                    }

                    #endregion

                    document.EndUpdate();
                    wordProcessor.SaveDocument(path, DocumentFormat.OpenXml);
                }
                catch (Exception ex)
                {
                    def.meta = new Meta(215, "Download file mẫu không thành công!");
                    return Ok(def);
                }

                if (path != null && path != "")
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                        string fileName = "file" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";

                        using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read))
                        {
                            file.CopyTo(ms);
                        }

                        try
                        {
                            System.IO.File.Delete(path);
                        }
                        catch { }

                        return File(ms.ToArray(), "application/octet-stream", fileName);
                    }
                }
                else
                {
                    def.meta = new Meta(215, "Data file null!");
                    return Ok(def);
                }
            }
            catch (Exception ex)
            {
                log.Error("DownloadFile:" + ex.Message);
                throw;
            }
        }

        private bool ProcessProfileCeExists(long id)
        {
            return _context.ProcessProfileCes.Count(e => e.Id == id) > 0;
        }
    }
}
