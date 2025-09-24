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
using DevExpress.XtraPrinting.Native;
using DevExpress.Utils.Extensions;
using IOITQln.Migrations;
using DevExpress.CodeParser;

namespace IOITQln.Controllers.ApiTdc
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ImportDataPlatformController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("import_Data_Platform", "import_Data_Platform");
        private static string functionCode = "IMPORT_DATA_TDC_PLATFORM";
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;

        public ImportDataPlatformController(ApiDbContext context, IMapper mapper, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
        }

        #region Import danh sách nền đất trống đã tiếp nhận và quản lý

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
                importHistory.Type = AppEnums.ImportHistoryType.Tdc_PlatformDataImport;
                List<TdcPlatformManager> tdcPlatformManagerdata = _context.TdcPlatformManagers.Where(e => e.Status != EntityStatus.DELETED).ToList();
                List<PlatformDataImport> data = new List<PlatformDataImport>();

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
                                    data = importData(ms, 0, 8);
                                }
                            }
                        }
                    }
                    i++;
                }
                List<PlatformDataImport> datavalid = new List<PlatformDataImport>();
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var dataItem in data)
                        {
                            if (dataItem.Valid == true)
                            {
                                District district = _context.Districts.Where(m => m.Name == dataItem.DistrictProjectName && m.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                if (district == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy Quận/huyện của dự án \n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.DistrictProjectId = district.Id;
                                }

                                District districtAllocation = _context.Districts.Where(m => m.Name == dataItem.DistrictAllocationName && m.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                if (districtAllocation == null)
                                {
                                    dataItem.Valid = true;
                                }
                                else
                                {
                                    dataItem.DistrictId = districtAllocation.Id;
                                }

                                TypeAttributeItem typeLegal = _context.TypeAttributeItems.Where(m => m.Name == dataItem.TypeLegalName && m.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                if (typeLegal == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy Pháp lý tiếp nhận  \n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.TypeLegalId = typeLegal.Id;
                                }

                                TypeAttributeItem typeDecision = _context.TypeAttributeItems.Where(m => m.Name == dataItem.TypeDecisionName && m.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                if (typeDecision == null)
                                {
                                    dataItem.Valid = true;
                                }
                                else
                                {
                                    dataItem.TypeDecisionId = typeDecision.Id;
                                }

                                TDCProject tdcProject = _context.TDCProjects.Where(m => m.Name == dataItem.TdcProjectName && m.District == dataItem.DistrictProjectId && m.Status != EntityStatus.DELETED).FirstOrDefault();
                                if (tdcProject == null)
                                {
                                    if (dataItem.TdcProjectId == null)
                                    {
                                        tdcProject = new TDCProject();
                                        tdcProject.CreatedById = userId;
                                        tdcProject.CreatedBy = $"{fullName} (Excel)";
                                        tdcProject.Name = dataItem.TdcProjectName;
                                        tdcProject.LateRate = 0;
                                        tdcProject.DebtRate = 0;
                                        tdcProject.TotalAreas = 0;
                                        tdcProject.TotalApartment = 0;
                                        tdcProject.TotalPlatform = 0;
                                        tdcProject.TotalFloorAreas = 0;
                                        tdcProject.TotalUseAreas = 0;
                                        tdcProject.TotalBuildAreas = 0;
                                        tdcProject.HouseNumber = " ";
                                        tdcProject.BuildingName = " ";
                                        tdcProject.Province = 2;
                                        tdcProject.District = (int)dataItem.DistrictProjectId;

                                        _context.TDCProjects.AddRange(tdcProject);
                                        _context.SaveChanges();
                                        tdcProject.Code = CodeIndentity.CodeInd("DA", tdcProject.Id, 4);
                                        _context.Update(tdcProject);
                                        _context.SaveChanges();

                                        dataItem.TdcProjectId = tdcProject.Id;
                                    }
                                    //continue;
                                }
                                else
                                {
                                    dataItem.TdcProjectId = tdcProject.Id;
                                }

                                Land land = _context.Lands.Where(m => m.Name == dataItem.TdcLandName && m.TDCProjectId == dataItem.TdcProjectId && m.Status != EntityStatus.DELETED).FirstOrDefault();
                                if (land == null)
                                {
                                    if (dataItem.LandId == null)
                                    {
                                        land = new Land();
                                        land.CreatedById = userId;
                                        land.CreatedBy = $"{fullName} (Excel)";
                                        land.Name = dataItem.TdcLandName;
                                        land.TDCProjectId = dataItem.TdcProjectId;
                                        land.TotalArea = 0;
                                        land.ConstructionApartment = 0;
                                        land.ConstructionLand = 0;
                                        land.ConstructionValue = 0;
                                        land.ContrustionBuild = 0;
                                        land.BlockHouseCount = 0;
                                        land.PlotType = PlotType.UnknowLand;

                                        _context.Lands.AddRange(land);
                                        _context.SaveChanges();
                                        land.Code = CodeIndentity.CodeInd("LO", land.Id, 4); ;
                                        _context.Update(land);
                                        _context.SaveChanges();

                                        dataItem.LandId = land.Id;
                                    }
                                    //continue;
                                }
                                else
                                {
                                    dataItem.LandId = land.Id;
                                }

                                PlatformTdc platformTdc = _context.PlatformTdcs.Where(m => m.Name == dataItem.PlatformTdcName && m.LandId == dataItem.LandId && m.Status != EntityStatus.DELETED).FirstOrDefault();
                                if (platformTdc == null)
                                {
                                    if (dataItem.PlatformTdcId == null)
                                    {
                                        platformTdc = new PlatformTdc();
                                        platformTdc.CreatedById = userId;
                                        platformTdc.CreatedBy = $"{fullName} (Excel)";
                                        platformTdc.Name = dataItem.PlatformTdcName;
                                        platformTdc.TDCProjectId = dataItem.TdcProjectId;
                                        platformTdc.LandId = dataItem.LandId;
                                        platformTdc.Platcount = dataItem.PlatCount;
                                        platformTdc.Corner = false;
                                        platformTdc.LengthArea = dataItem.TdcLength;
                                        platformTdc.WidthArea = dataItem.TdcWidth;
                                        platformTdc.LandArea = (double)dataItem.TdcPlatformArea;

                                        _context.PlatformTdcs.AddRange(platformTdc);
                                        _context.SaveChanges();
                                        platformTdc.Code = CodeIndentity.CodeInd("CAN_TDC_", platformTdc.Id, 4);
                                        _context.Update(platformTdc);
                                        _context.SaveChanges();

                                        dataItem.PlatformTdcId = platformTdc.Id;
                                    }
                                    //continue;
                                }
                                else
                                {
                                    dataItem.PlatformTdcId = platformTdc.Id;
                                }

                                dataItem.CreatedById = -1;

                                TdcPlatformManager tdcPlatformManager = _context.TdcPlatformManagers.Where(m => m.DistrictProjectId == dataItem.DistrictProjectId &&
                                                                                                               m.TdcProjectId == dataItem.TdcProjectId &&
                                                                                                               m.LandId == dataItem.LandId &&
                                                                                                               m.PlatformTdcId == dataItem.PlatformTdcId &&
                                                                                                               m.Status != EntityStatus.DELETED).FirstOrDefault();
                                if (tdcPlatformManager == null)
                                {
                                    tdcPlatformManager = new TdcPlatformManager();
                                    tdcPlatformManager.CreatedById = userId;
                                    tdcPlatformManager.CreatedBy = $"{fullName} (Excel)";
                                    tdcPlatformManager.DistrictProjectId = dataItem.DistrictProjectId;
                                    tdcPlatformManager.TypeDecisionId = dataItem.TypeDecisionId;
                                    tdcPlatformManager.TypeLegalId = dataItem.TypeLegalId;
                                    tdcPlatformManager.TdcProjectId = dataItem.TdcProjectId;
                                    tdcPlatformManager.LandId = dataItem.LandId;
                                    tdcPlatformManager.PlatformTdcId = dataItem.PlatformTdcId;
                                    tdcPlatformManager.TdcLength = dataItem.TdcLength;
                                    tdcPlatformManager.TdcWidth = dataItem.TdcWidth;
                                    tdcPlatformManager.TdcPlatformArea = dataItem.TdcPlatformArea;
                                    tdcPlatformManager.PlatCount = dataItem.PlatCount;
                                    tdcPlatformManager.Qantity = dataItem.Qantity;
                                    tdcPlatformManager.ReceiveNumber = dataItem.ReceiveNumber;
                                    tdcPlatformManager.ReceptionDate = dataItem.ReceptionDate;
                                    tdcPlatformManager.ReceptionTime = dataItem.ReceptionTime;
                                    tdcPlatformManager.HandOver = dataItem.HandOver;
                                    tdcPlatformManager.HandoverPublic = dataItem.HandoverPublic;
                                    tdcPlatformManager.NoteHandoverPublic = dataItem.NoteHandoverPublic;
                                    tdcPlatformManager.HandOverYear = dataItem.HandOverYear;
                                    tdcPlatformManager.HandoverCenter = dataItem.HandoverCenter;
                                    tdcPlatformManager.NoteHandoverCenter = dataItem.NoteHandoverCenter;
                                    tdcPlatformManager.HandoverOther = dataItem.HandoverOther;
                                    tdcPlatformManager.NoteHandoverOther = dataItem.NoteHandoverOther;
                                    tdcPlatformManager.Note = dataItem.Note;
                                    tdcPlatformManager.ReasonReceivedYet = dataItem.ReasonReceivedYet;
                                    tdcPlatformManager.ReasonNotReceived = dataItem.ReasonNotReceived;

                                    _context.TdcPlatformManagers.Add(tdcPlatformManager);
                                    _context.SaveChanges();

                                    dataItem.TdcPlatformManagerId = tdcPlatformManager.Id;
                                }
                                else
                                {
                                    dataItem.TdcPlatformManagerId = tdcPlatformManager.Id;
                                }

                                DistrictAllocasionPlatform districtAllocasionPlatform = _context.DistrictAllocasionPlatforms.Where(m => m.Status != EntityStatus.DELETED).FirstOrDefault();
                                if (districtAllocasionPlatform != null && dataItem.DistrictId != 0)
                                {
                                    districtAllocasionPlatform = new DistrictAllocasionPlatform();
                                    districtAllocasionPlatform.TdcPlatformManagerId = dataItem.TdcPlatformManagerId;
                                    districtAllocasionPlatform.DistrictId = dataItem.DistrictId;
                                    districtAllocasionPlatform.ActualNumber = dataItem.ActualNumber;
                                    districtAllocasionPlatform.CreatedById = userId;
                                    districtAllocasionPlatform.CreatedBy = $"{fullName} (Excel)";
                                    

                                    _context.DistrictAllocasionPlatforms.Add(districtAllocasionPlatform);
                                    _context.SaveChanges();
                                }
                                datavalid.Add(dataItem);
                            }
                        }
                        importHistory.Data = data.Cast<dynamic>().ToList();
                        importHistory.CreatedById = userId;
                        importHistory.CreatedBy = fullName;



                        _context.ImportHistories.Add(importHistory);


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
                    return Ok(def);
                }
            }
            catch (Exception ex)
            {
                log.Error("ImportDataExcel:" + ex);
                def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                return Ok(def);
            }
        }

        public static List<PlatformDataImport> importData(MemoryStream ms, int sheetnumber, int rowStart)
        {
            XSSFWorkbook workbook = new XSSFWorkbook(ms);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);

            List<PlatformDataImport> res = new List<PlatformDataImport>();
            for (int row = rowStart; row <= sheet.LastRowNum; row++)
            {
                if (sheet.GetRow(row) != null)
                {
                    PlatformDataImport inputDetail = new PlatformDataImport();
                    inputDetail.Valid = true;
                    inputDetail.ErrMsg = "";

                    for (int i = 0; i < 22; i++)
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
                                        inputDetail.DistrictProjectName = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột Quận/huyện của dự án chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Quận/huyện của dự án\n";
                                }
                            }

                            else if (i == 2)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TdcProjectName = str;

                                        #region
                                        //kiểm tra dữ liệu dự án có hợp lệ
                                        //SimpleProjectData tdcProject = tdcProjectData.Where(c => c.Name == inputDetail.TdcProjectName).FirstOrDefault();
                                        //if (tdcProject != null)
                                        //{
                                        //    inputDetail.TdcProjectId = (int)tdcProject.Id;
                                        //    inputDetail.ExistProject = true;
                                        //}
                                        //else
                                        //{
                                        //    inputDetail.ExistProject = false;
                                        //}
                                        #endregion
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột Dự án chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Dự án\n";
                                }
                            }

                            else if (i == 3)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.PlatformTdcName = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột Mã nền chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Mã nền\n";
                                }
                            }

                            else if (i == 4)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TdcLandName = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột Lô chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Lô\n";
                                }
                            }

                            else if (i == 5)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.PlatCount = int.Parse(str);
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột Số nền chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Số nền\n";
                                }
                            }

                            else if (i == 6)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TdcPlatformArea = decimal.Parse(str);
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột Diện tích chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Diện tích\n";
                                }
                            }

                            else if (i == 7)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TdcLength = double.Parse(str);
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột Chiều dài chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Chiều dài\n";
                                }
                            }

                            else if (i == 8)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TdcWidth = double.Parse(str);
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột Chiều rộng chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Chiều rộng\n";
                                }
                            }

                            

                            else if (i == 9)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TypeLegalName = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột Pháp lý tiếp nhận chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Pháp lý tiếp nhận\n";
                                }
                            }

                            else if (i == 10)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.ReceptionDate = DateTime.FromOADate(Double.Parse(str));
                                        if (inputDetail.ReceptionDate.Year < 1900)
                                        {
                                            inputDetail.Valid = false;
                                            inputDetail.ErrMsg += "Ngày tiếp nhận không hợp lệ\n";
                                        }
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Ngày tiếp nhận\n";
                                }
                            }

                            else if (i == 11)
                            {
                                try
                                {
                                    //Qfix
                                    // if (str == "Đã tiếp nhận")
                                    // {
                                    //     inputDetail.ReceptionTime = TypeReception.Received;
                                    // }
                                    // else if (str == "Chưa tiếp nhận")
                                    // {
                                    //     inputDetail.ReceptionTime = TypeReception.ReceivedYet;
                                    // }
                                    // else if (str == "Không tiếp nhận")
                                    // {
                                    //     inputDetail.ReceptionTime = TypeReception.NotReceived;
                                    // }
                                    var normalized = UtilsService.NonUnicode(str)?.Trim().ToLowerInvariant();
                                    if (normalized == "da tiep nhan")
                                        inputDetail.ReceptionTime = TypeReception.Received;
                                    else if (normalized == "chua tiep nhan")
                                        inputDetail.ReceptionTime = TypeReception.ReceivedYet;
                                    else if (normalized == "khong tiep nhan")
                                        inputDetail.ReceptionTime = TypeReception.NotReceived;
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Đã tiếp nhận\n";
                                }
                            }

                            else if (i == 12)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.ReasonReceivedYet = str;
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi Lý do chưa tiếp nhận\n";
                                }
                            }

                            else if (i == 13)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.Reminded = str;
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Đã nhắc nhở\n";
                                }
                            }

                            else if (i == 14)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.ReasonNotReceived = str;
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Lý do không tiếp nhận\n";
                                }
                            }

                            else if (i == 15)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TypeDecisionName = str;
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Quyết định\n";
                                }
                            }

                            else if (i == 16)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.DistrictAllocationName = str;
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Quận/huyện\n";
                                }
                            }

                            else if (i == 17)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.ActualNumber = int.Parse(str);
                                    }
                                    //else
                                    //{
                                    //    inputDetail.Valid = false;
                                    //    inputDetail.ErrMsg += "Cột Số lượng không hợp lệ\n";
                                    //}
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Số lượng\n";
                                }
                            }

                            else if (i == 18)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        if (double.TryParse(str, out double numericValue))
                                        {
                                            DateTime handOverDate = DateTime.FromOADate(numericValue);
                                            inputDetail.HandOverYear = handOverDate.ToString("dd/MM/yyyy");
                                        }
                                        else
                                        {
                                            inputDetail.HandOverYear = str;
                                        }
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Thời gian bàn giao\n";
                                }
                            }

                            else if (i == 19)
                            {
                                try
                                {
                                    if (str == "X")
                                    {
                                        inputDetail.HandoverPublic = true;
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột DVCI bàn giao\n";
                                }
                            }

                            else if (i == 20)
                            {
                                try
                                {
                                    if (str == "X")
                                    {
                                        inputDetail.HandoverCenter = true;
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Trung tâm giao\n";
                                }
                            }

                            else if (i == 21)
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



