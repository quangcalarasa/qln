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
using static DevExpress.XtraEditors.Filtering.DataItemsExtension;

namespace IOITQln.Controllers.ApiNoc
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ConversionController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("conversion", "conversion");
        private readonly ApiDbContext _context;
        string functionCode = "CONVERSION";
        private IMapper _mapper;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;
        public ConversionController(ApiDbContext context, IMapper mapper, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
        }


        //GET: api/conversion
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
                    IQueryable<Conversion> data = _context.Conversions.Where(c => c.Status != AppEnums.EntityStatus.DELETED);
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
                        def.data = data;
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

        // GET: api/conversion/Id
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
                Conversion data = await _context.Conversions.FindAsync(id);

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

        // POST: api/conversion
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Conversion input)
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
                input = (Conversion)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }
                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.CreatedById = userId;
                    input.CreatedBy = fullName;
                    _context.Conversions.Add(input);
                    try
                    {
                        await _context.SaveChangesAsync();
                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Thêm mới danh sách hệ số quy đổi: " + input.Id, "Conversion", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
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
                        if (ConversionExists(input.Id))
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

        // PUT: api/Conversion/Put
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Conversion input)
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
                input = (Conversion)UtilsService.TrimStringPropertyTypeObject(input);

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


                Conversion data = await _context.Conversions.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
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
                        LogActionModel logActionModel = new LogActionModel("Sửa danh sách Hệ số quy đổi: " + input.Id, "Conversion", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, userId, fullName);
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
                        if (!ConversionExists(data.Id))
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
                Conversion data = await _context.Conversions.FindAsync(id);
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
                        LogActionModel logActionModel = new LogActionModel("Xóa danh sách Hệ số quy đổi: " + data.Id, "Conversion", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.DELETED, userId, fullName);
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
                        if (!ConversionExists(data.Id))
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
        private bool ConversionExists(int id)
        {
            return _context.AnnualInstallments.Count(e => e.Id == id) > 0;
        }

        #region export hệ số quy đổi
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

            List<Conversion> conversions = _context.Conversions.Where(p => p.Status != AppEnums.EntityStatus.DELETED).ToList();
            List<ConversionData> mapper_data = _mapper.Map<List<ConversionData>>(conversions);
            foreach (var map in mapper_data)
            {
                string typeQD = UtilsService.NonUnicode(map.type_Qd);
                if (typeQD == "qd-1753")
                {
                    map.TypeQD = TypeQD.QD_1753;
                }
                else if (typeQD == "qd-09")
                {
                    map.TypeQD = TypeQD.QD_09;
                }
                else if (typeQD == "qd-22-2")
                {
                    map.TypeQD = TypeQD.QD_22;
                }

                string coefficientName = UtilsService.NonUnicode(map.Coefficient_Name);
                if (typeQD == "bang-he-so-tinh-thanh")
                {
                    map.CoefficientName = TypeTable.HSTT;
                }
                else if (typeQD == "bang-he-so-tang-cao")
                {
                    map.CoefficientName = TypeTable.HSTC;
                }
                else if (typeQD == "bang-he-so-k5")
                {
                    map.CoefficientName = TypeTable.K5;
                }
                else if (typeQD == "bang-he-so-giam-gia")
                {
                    map.CoefficientName = TypeTable.HSGG;
                }
                else if (typeQD == "bang-he-so-khu-vuc")
                {
                    map.CoefficientName = TypeTable.HSKV;
                }
                else if (typeQD == "bang-he-so-ki-thuat")
                {
                    map.CoefficientName = TypeTable.HTKT;
                }
                else if (typeQD == "bang-he-so-gia-thue")
                {
                    map.CoefficientName = TypeTable.HSGT;
                }
            }

            // khởi tạo wb rỗng
            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"NOCexcel/hesoquydoi.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);

            MemoryStream ms = WriteDataToExcel(templatePath, 0, mapper_data);
            byte[] byteArrayContent = ms.ToArray();
            return new FileContentResult(byteArrayContent, "application/octet-stream");
        }

        private static MemoryStream WriteDataToExcel(string templatePath, int sheetnumber, List<ConversionData> data)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowStart = 1;

            if (sheet != null)
            {
                int datacol = 5;
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
                                    row.GetCell(i).SetCellValue(item.type_Qd);
                                }
                                else if (i == 2)
                                {
                                    row.GetCell(i).SetCellValue(item.Coefficient_Name);
                                }
                                else if (i == 3)
                                {
                                    row.GetCell(i).SetCellValue(item.Code);
                                }
                                else if (i == 4)
                                {
                                    row.GetCell(i).SetCellValue(item.Note);
                                }
                            }
                            rowStart++;
                            k++;
                        }
                        catch (Exception ex)
                        {
                            log.Error("ExportDataExcel:" + ex);
                            item.ErrMsg += "Lỗi dữ liệu";
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

        #region import hệ số quy đổi
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
                importHistory.Type = AppEnums.ImportHistoryType.NocConversion;

                List<ConversionData> data = new List<ConversionData>();

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

                List<ConversionData> dataValid = new List<ConversionData>();
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var dataItem in data)
                        {
                            if (dataItem.Valid == true)
                            {
                                string typeQD = UtilsService.NonUnicode(dataItem.type_Qd);
                                if (typeQD == "qd_1753")
                                {
                                    dataItem.TypeQD = TypeQD.QD_1753;
                                }
                                else if (typeQD == "qd_09")
                                {
                                    dataItem.TypeQD = TypeQD.QD_09;
                                }
                                else if (typeQD == "qd_22")
                                {
                                    dataItem.TypeQD = TypeQD.QD_22;
                                }
                                else
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Cột Kỳ Thông tư/Văn bản/Quyết định không hợp lệ\n";
                                    continue;
                                }

                                string coefficientName = UtilsService.NonUnicode(dataItem.Coefficient_Name);
                                if (coefficientName == "bang-he-so-tinh-thanh")
                                {
                                    dataItem.CoefficientName = TypeTable.HSTT;
                                }
                                else if (coefficientName == "bang-he-so-tang-cao")
                                {
                                    dataItem.CoefficientName = TypeTable.HSTC;
                                }
                                else if (coefficientName == "bang-he-so-k5")
                                {
                                    dataItem.CoefficientName = TypeTable.K5;
                                }
                                else if (coefficientName == "bang-he-so-giam-gia")
                                {
                                    dataItem.CoefficientName = TypeTable.HSGG;
                                }
                                else if (coefficientName == "bang-he-so-khu-vuc")
                                {
                                    dataItem.CoefficientName = TypeTable.HSKV;
                                }
                                else if (coefficientName == "bang-he-so-ki-thuat")
                                {
                                    dataItem.CoefficientName = TypeTable.HTKT;
                                }
                                else if (coefficientName == "bang-he-so-gia-thue")
                                {
                                    dataItem.CoefficientName = TypeTable.HSGT;
                                }
                                else
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Cột Kỳ Tên hệ số định không hợp lệ\n";
                                    continue;
                                }

                                dataItem.CreatedById = -1;
                                dataValid.Add(dataItem);
                            }
                        }
                        importHistory.Data = data.Cast<dynamic>().ToList();
                        importHistory.CreatedById = userId;
                        importHistory.CreatedBy = fullName;

                        _context.ImportHistories.Add(importHistory);
                        _context.Conversions.AddRange(dataValid);

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

        public static List<ConversionData> importData(MemoryStream ms, int sheetnumber, int rowStart)
        {
            XSSFWorkbook workbook = new XSSFWorkbook(ms);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);

            List<ConversionData> res = new List<ConversionData>();
            for (int row = rowStart; row <= sheet.LastRowNum; row++)
            {
                if (sheet.GetRow(row) != null)
                {
                    ConversionData inputDetail = new ConversionData();
                    inputDetail.Valid = true;
                    inputDetail.ErrMsg = "";

                    for (int i = 0; i < 4; i++)
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
                                        inputDetail.Index = int.Parse(str);
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Số thứ tự\n";
                                }
                            }

                            else if (i == 1)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.type_Qd = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột Thông tư/Văn bản/Quyết định chưa có dữ liệu";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Thông tư/Văn bản/Quyết định\n";
                                }
                            }

                            else if (i == 2)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.Coefficient_Name = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột Tên hệ số chưa có dữ liệu";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Tên hệ số\n";
                                }
                            }

                            else if (i == 3)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.Code = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột Tên kí hiệu chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Tên kí hiệu\n";
                                }
                            }
                            else if (i == 4)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.Note = str;
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Ghi chú\n";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            inputDetail.Valid = false;
                            inputDetail.ErrMsg += "Lỗi dữ liệu\n";
                            log.Error("Exception:" + ex);
                        }
                    }
                    res.Add(inputDetail);
                }
            }
            return res;
        }

        #endregion
    }
}
