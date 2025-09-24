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
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LandPriceCorrectionCoefficientController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("LandPriceCorrectionCoefficient", "LandPriceCorrectionCoefficient");
        private static string functionCode = "LAND_PRICE_CORRECTION_COEFFICIENT_MANAGEMENT";
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;

        public LandPriceCorrectionCoefficientController(ApiDbContext context, IMapper mapper, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
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
                    IQueryable<LandPriceCorrectionCoefficient> data = _context.LandPriceCorrectionCoefficients.Where(c => c.Status != AppEnums.EntityStatus.DELETED);
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
                        List<LandPriceCorrectionCoefficientData> res = _mapper.Map<List<LandPriceCorrectionCoefficientData>>(data.ToList());
                        foreach (LandPriceCorrectionCoefficientData item in res)
                        {
                            Decree decree_type1 = _context.Decreies.Where(d => d.Id == item.DecreeType1Id).FirstOrDefault();
                            item.DecreeType1Name = decree_type1 != null ? decree_type1.Code : "";

                            Decree decree_type2 = _context.Decreies.Where(d => d.Id == item.DecreeType2Id).FirstOrDefault();
                            item.DecreeType2Name = decree_type2 != null ? decree_type2.Code : "";
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

        // GET: api/LandPriceCorrectionCoefficient/1
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
                LandPriceCorrectionCoefficient data = await _context.LandPriceCorrectionCoefficients.FindAsync(id);

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

        // POST: api/LandPriceCorrectionCoefficient
        [HttpPost]
        public async Task<IActionResult> Post(LandPriceCorrectionCoefficient input)
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
                input = (LandPriceCorrectionCoefficient)UtilsService.TrimStringPropertyTypeObject(input);

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

                if (input.Name == null || input.Name == "")
                {
                    def.meta = new Meta(400, "Tên không được để trống!");
                    return Ok(def);
                }

                //LandPriceCorrectionCoefficient codeExist = _context.LandPriceCorrectionCoefficients.Where(f => f.Code == input.Code && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                //if (codeExist != null)
                //{
                //    def.meta = new Meta(211, "Mã đã tồn tại!");
                //    return Ok(def);
                //}

                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.CreatedById = userId;
                    input.CreatedBy = fullName;
                    _context.LandPriceCorrectionCoefficients.Add(input);

                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Thêm mới hệ số K điều chỉnh giá đất " + input.Name, "LandPriceCorrectionCoefficient", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
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
                        if (LandPriceCorrectionCoefficientExists(input.Id))
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

        // PUT: api/LandPriceCorrectionCoefficient/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] LandPriceCorrectionCoefficient input)
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
                input = (LandPriceCorrectionCoefficient)UtilsService.TrimStringPropertyTypeObject(input);

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

                if (input.Name == null || input.Name == "")
                {
                    def.meta = new Meta(400, "Tên không được để trống!");
                    return Ok(def);
                }

                LandPriceCorrectionCoefficient data = await _context.LandPriceCorrectionCoefficients.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
                if(data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_UPDATE_MESSAGE);
                    return Ok(def);
                }

                //LandPriceCorrectionCoefficient codeExist = _context.LandPriceCorrectionCoefficients.Where(f => f.Code == input.Code && f.Status != AppEnums.EntityStatus.DELETED && f.Id != id).FirstOrDefault();
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

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Sửa thông tin hệ số K điều chỉnh giá đất " + input.Name, "LandPriceCorrectionCoefficient", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, userId, fullName);
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
                        if (!LandPriceCorrectionCoefficientExists(data.Id))
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

        // DELETE: api/LandPriceCorrectionCoefficient/1
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
                LandPriceCorrectionCoefficient data = await _context.LandPriceCorrectionCoefficients.FindAsync(id);
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

                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Xóa thông tin hệ số K điều chỉnh giá đất " + data.Name, "LandPriceCorrectionCoefficient", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.DELETED, userId, fullName);
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
                        if (!LandPriceCorrectionCoefficientExists(data.Id))
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

        private bool LandPriceCorrectionCoefficientExists(int id)
        {
            return _context.LandPriceCorrectionCoefficients.Count(e => e.Id == id) > 0;
        }

        #region import excel

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
                importHistory.Type = AppEnums.ImportHistoryType.Noc_K_LandPrice_Correction_Coefficient;

                List<LandPriceCorrectionCoefficientDataImport> data = new List<LandPriceCorrectionCoefficientDataImport>();

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

                List<LandPriceCorrectionCoefficientDataImport> dataValid = new List<LandPriceCorrectionCoefficientDataImport>();
                List<Decree> decrees = _context.Decreies.Where(p => p.TypeDecree == TypeDecree.THONG_TU_VAN_BAN && p.Status != AppEnums.EntityStatus.DELETED).ToList();

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        data.ForEach(item => {
                            if (item.Valid == true)
                            {
                                Decree decree = decrees.AsEnumerable().Where(e => UtilsService.NonUnicode(e.Code) == item.DecreeType2Name).FirstOrDefault();
                                if (decree != null)
                                {
                                    item.DecreeType2Id = decree.Id;
                                    dataValid.Add(item);
                                }
                                else
                                {
                                    item.Valid = false;
                                    item.ErrMsg += "Cột Văn bản pháp luật liên quan không hợp lệ\n";
                                }
                            }
                        });

                        importHistory.Data = data.Cast<dynamic>().ToList();
                        importHistory.CreatedById = userId;
                        importHistory.CreatedBy = fullName;

                        _context.ImportHistories.Add(importHistory);
                        _context.LandPriceCorrectionCoefficients.AddRange(dataValid);

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

        public static List<LandPriceCorrectionCoefficientDataImport> importData(MemoryStream ms, int sheetnumber, int rowStart)
        {
            XSSFWorkbook workbook = new XSSFWorkbook(ms);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);

            List<LandPriceCorrectionCoefficientDataImport> res = new List<LandPriceCorrectionCoefficientDataImport>();
            for (int row = rowStart; row <= sheet.LastRowNum; row++)
            {
                if (sheet.GetRow(row) != null)
                {
                    //Đọc dữ liệu từ từng cell
                    LandPriceCorrectionCoefficientDataImport input1Detai = new LandPriceCorrectionCoefficientDataImport();
                    input1Detai.Valid = true;
                    input1Detai.ErrMsg = "";

                    for (int i = 0; i < 7; i++)
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
                                        string type_decree = UtilsService.NonUnicode(str);
                                        if (type_decree == "99-2015-nd-cp")
                                        {
                                            input1Detai.DecreeType1Id = (int)DecreeEnum.ND_CP_99_2015;
                                        }
                                        else if (type_decree == "34-2013-nd-cp")
                                        {
                                            input1Detai.DecreeType1Id = (int)DecreeEnum.ND_CP_34_2013;
                                        }
                                        else if (type_decree == "61-nd-cp")
                                        {
                                            input1Detai.DecreeType1Id = (int)DecreeEnum.ND_CP_61;
                                        }
                                        else
                                        {
                                            input1Detai.Valid = false;
                                            input1Detai.ErrMsg += "Cột Nghị định không hợp lệ\n";
                                        }
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Nghị định chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Nghị định\n";
                                }
                            }
                            else if (i == 2)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.DecreeType2Name = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Văn bản pháp luật liên quan chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Văn bản pháp luật liên quan\n";
                                }
                            }
                            else if (i == 3)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.Name = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Hệ số K điều chỉnh giá đất chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Hệ số K điều chỉnh giá đất\n";
                                }
                            }
                            else if (i == 4)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.FacadeWidth = float.Parse(str);
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Chiều rộng mặt tiền chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Chiều rộng mặt tiền\n";
                                }
                            }
                            else if (i == 5)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.Value = float.Parse(str);
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Giá trị chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Giá trị\n";
                                }
                            }
                            else if (i == 6)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.Note = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Diễn giải chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Diễn giải\n";
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

            List<LandPriceCorrectionCoefficient> data = _context.LandPriceCorrectionCoefficients.Where(p => p.Status != AppEnums.EntityStatus.DELETED).ToList();
            List<Decree> decrees = _context.Decreies.Where(p => p.TypeDecree == TypeDecree.THONG_TU_VAN_BAN && p.Status != AppEnums.EntityStatus.DELETED).ToList();

            // khởi tạo wb rỗng
            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"NOCexcel/he_so_k.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);

            MemoryStream ms = WriteDataToExcel(templatePath, 0, data, decrees);
            byte[] byteArrayContent = ms.ToArray();
            return new FileContentResult(byteArrayContent, "application/octet-stream");
        }

        private static MemoryStream WriteDataToExcel(string templatePath, int sheetnumber, List<LandPriceCorrectionCoefficient> data, List<Decree> decrees)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            int rowStart = 1;

            if (sheet != null)
            {
                int datacol = 7;
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
                                    if (item.DecreeType1Id == (int)DecreeEnum.ND_CP_99_2015)
                                        row.GetCell(i).SetCellValue("99/2015/NĐ-CP");
                                    else if (item.DecreeType1Id == (int)DecreeEnum.ND_CP_34_2013)
                                        row.GetCell(i).SetCellValue("34/2013/NĐ-CP");
                                    else if (item.DecreeType1Id == (int)DecreeEnum.ND_CP_34_2013)
                                        row.GetCell(i).SetCellValue("61/NĐ-CP");
                                }
                                else if (i == 2)
                                {
                                    if (item.DecreeType2Id != null)
                                    {
                                        Decree decree = decrees.Where(e => e.Id == item.DecreeType2Id).FirstOrDefault();
                                        if (decree != null) row.GetCell(i).SetCellValue(decree.Code);
                                    }
                                }
                                else if (i == 3)
                                {
                                    row.GetCell(i).SetCellValue(item.Name);
                                }
                                else if (i == 4)
                                {
                                    row.GetCell(i).SetCellValue(item.FacadeWidth);
                                }
                                else if (i == 5)
                                {
                                    row.GetCell(i).SetCellValue(item.Value);
                                }
                                else if (i == 6)
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
    }
}
