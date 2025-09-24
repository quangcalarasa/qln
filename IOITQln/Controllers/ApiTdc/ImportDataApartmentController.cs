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
    public class ImportDataApartmentController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("import_Data_Apartment", "import_Data_Apartment");
        private static string functionCode = "IMPORT_DATA_TDC_APARTMENT";
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;

        public ImportDataApartmentController(ApiDbContext context, IMapper mapper, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
        }

        #region Import danh sách căn hộ trống đã tiếp nhận và quản lý

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
                importHistory.Type = AppEnums.ImportHistoryType.Tdc_ApartmentDataImport;

                List<ApartmentDataImport> data = new List<ApartmentDataImport>();
                
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

                List<ApartmentDataImport> datavalid = new List<ApartmentDataImport>();
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var dataItem in data)
                        {
                            if(dataItem.Valid == true)
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

                                District districtAllocation = _context.Districts.Where(m => m.Name == dataItem.DistrictName && m.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
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

                                TDCProject tdcProject = _context.TDCProjects.Where(m=> m.Name == dataItem.TdcProjectName && m.District == dataItem.DistrictProjectId && m.Status != EntityStatus.DELETED).FirstOrDefault();
                                if(tdcProject == null)
                                {
                                    if(dataItem.TdcProjectId == null)
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

                                Land land = _context.Lands.Where(m=>m.Name == dataItem.TdcLandName && m.TDCProjectId == dataItem.TdcProjectId && m.Status != EntityStatus.DELETED).FirstOrDefault();
                                if(land == null)
                                {
                                    if(dataItem.LandId == null)
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

                                BlockHouse blockHouse = _context.BlockHouses.Where(m => m.Name == dataItem.TdcBlockName && m.LandId == dataItem.LandId && m.Status != EntityStatus.DELETED).FirstOrDefault();
                                if(blockHouse == null)
                                {
                                    if(dataItem.BlockHouseId == null)
                                    {
                                        blockHouse = new BlockHouse();
                                        blockHouse.CreatedById = userId;
                                        blockHouse.CreatedBy = $"{fullName} (Excel)";
                                        blockHouse.Name = dataItem.TdcBlockName;
                                        blockHouse.LandId = dataItem.LandId;
                                        blockHouse.ConstructionValue = 0;
                                        blockHouse.ContrustionBuild = 0;
                                        blockHouse.FloorTdcCount = 0;
                                        blockHouse.TotalApartmentTdcCount = 0;
                                        land.BlockHouseCount++;

                                        _context.BlockHouses.AddRange(blockHouse);
                                        _context.SaveChanges();
                                        blockHouse.Code = CodeIndentity.CodeInd("KHOI", blockHouse.Id, 4);
                                        _context.Update(blockHouse);
                                        _context.SaveChanges();

                                        dataItem.BlockHouseId = blockHouse.Id;
                                    }
                                    //continue;
                                }
                                else
                                {
                                    dataItem.BlockHouseId = blockHouse.Id;
                                }

                                FloorTdc floorTdc= _context.FloorTdcs.Where(m => m.Name == dataItem.TdcFloorName && m.BlockHouseId == dataItem.BlockHouseId && m.Status != EntityStatus.DELETED).FirstOrDefault();
                                if (floorTdc == null)
                                {
                                    if(dataItem.FloorTdcId == null)
                                    {
                                        floorTdc = new FloorTdc();
                                        floorTdc.CreatedById = userId;
                                        floorTdc.CreatedBy = $"{fullName} (Excel)";
                                        floorTdc.Name = dataItem.TdcFloorName;
                                        floorTdc.FloorNumber = 0;
                                        floorTdc.BlockHouseId = dataItem.BlockHouseId;
                                        floorTdc.ConstructionValue = 0;
                                        floorTdc.ContrustionBuild = 0;
                                        floorTdc.ApartmentTdcCount = 0;
                                        blockHouse.FloorTdcCount++;

                                        _context.FloorTdcs.AddRange(floorTdc);
                                        _context.SaveChanges();
                                        floorTdc.Code = CodeIndentity.CodeInd("TANG_TDC_", floorTdc.Id, 4);
                                        _context.Update(floorTdc);
                                        _context.SaveChanges();

                                        dataItem.FloorTdcId = floorTdc.Id;
                                    }
                                    //continue;
                                }
                                else
                                {
                                    dataItem.FloorTdcId = floorTdc.Id;
                                }

                                ApartmentTdc apartmentTdc = _context.ApartmentTdcs.Where(m => m.Name == dataItem.ApartmentTdcName && m.FloorTdcId == dataItem.FloorTdcId && m.Status != EntityStatus.DELETED).FirstOrDefault();
                                if (apartmentTdc == null)
                                {
                                    if(dataItem.ApartmentTdcId == null)
                                    {
                                        apartmentTdc = new ApartmentTdc();
                                        apartmentTdc.CreatedById = userId;
                                        apartmentTdc.CreatedBy = $"{fullName} (Excel)";
                                        apartmentTdc.Name = dataItem.ApartmentTdcName;
                                        apartmentTdc.FloorTdcId = dataItem.FloorTdcId;
                                        apartmentTdc.Corner = false;
                                        apartmentTdc.ConstructionValue = 0;
                                        apartmentTdc.ContrustionBuild = 0;
                                        apartmentTdc.RoomNumber = 0;
                                        blockHouse.TotalApartmentTdcCount++;
                                        floorTdc.ApartmentTdcCount++;

                                        _context.ApartmentTdcs.AddRange(apartmentTdc);
                                        _context.SaveChanges();
                                        apartmentTdc.Code = CodeIndentity.CodeInd("CAN_TDC_", apartmentTdc.Id, 4);
                                        _context.Update(apartmentTdc);
                                        _context.SaveChanges();

                                        dataItem.ApartmentTdcId = apartmentTdc.Id;
                                    }
                                    //continue;
                                }
                                else
                                {
                                    dataItem.ApartmentTdcId = apartmentTdc.Id;
                                }

                                dataItem.CreatedById = -1;

                                TdcApartmentManager tdcApartmentManager = _context.TdcApartmentManagers.Where(m=> m.DistrictProjectId == dataItem.DistrictProjectId &&
                                                                                                                  m.TdcProjectId == dataItem.TdcProjectId &&
                                                                                                                  m.LandId == dataItem.LandId &&
                                                                                                                  m.BlockHouseId == dataItem.BlockHouseId &&
                                                                                                                  m.FloorTdcId == dataItem.FloorTdcId &&
                                                                                                                  m.ApartmentTdcId == dataItem.ApartmentTdcId &&
                                                                                                                  m.Status != EntityStatus.DELETED).FirstOrDefault();
                                if (tdcApartmentManager == null)
                                {
                                    tdcApartmentManager = new TdcApartmentManager();
                                    tdcApartmentManager.CreatedById = userId;
                                    tdcApartmentManager.CreatedBy = $"{fullName} (Excel)";
                                    tdcApartmentManager.DistrictProjectId = dataItem.DistrictProjectId;
                                    tdcApartmentManager.TypeDecisionId = dataItem.TypeDecisionId;
                                    tdcApartmentManager.TypeLegalId = dataItem.TypeLegalId;
                                    tdcApartmentManager.TdcProjectId = dataItem.TdcProjectId;
                                    tdcApartmentManager.LandId = dataItem.LandId;
                                    tdcApartmentManager.BlockHouseId = dataItem.BlockHouseId;
                                    tdcApartmentManager.FloorTdcId = dataItem.FloorTdcId;
                                    tdcApartmentManager.ApartmentTdcId = dataItem.ApartmentTdcId;
                                    tdcApartmentManager.TdcApartmentCountRoom = dataItem.TdcApartmentCountRoom;
                                    tdcApartmentManager.TdcApartmentArea = dataItem.TdcApartmentArea;
                                    tdcApartmentManager.Qantity = dataItem.Qantity;
                                    tdcApartmentManager.ReceiveNumber = dataItem.ReceiveNumber;
                                    tdcApartmentManager.ReceptionDate = dataItem.ReceptionDate;
                                    tdcApartmentManager.ReceptionTime = dataItem.ReceptionTime;
                                    tdcApartmentManager.HandOverYear = dataItem.HandOverYear;
                                    tdcApartmentManager.HandOver = dataItem.HandOver;
                                    tdcApartmentManager.OverYear = dataItem.OverYear;
                                    tdcApartmentManager.HandoverPublic = dataItem.HandoverPublic;
                                    tdcApartmentManager.NoteHandoverPublic = dataItem.NoteHandoverPublic;
                                    tdcApartmentManager.HandoverCenter = dataItem.HandoverCenter;
                                    tdcApartmentManager.NoteHandoverCenter = dataItem.NoteHandoverCenter;
                                    tdcApartmentManager.HandoverOther = dataItem.HandoverOther;
                                    tdcApartmentManager.NoteHandoverOther = dataItem.NoteHandoverOther;
                                    tdcApartmentManager.Note = dataItem.Note;
                                    tdcApartmentManager.ReasonReceivedYet = dataItem.ReasonReceivedYet;
                                    tdcApartmentManager.ReasonNotReceived = dataItem.ReasonNotReceived;

                                    _context.TdcApartmentManagers.Add(tdcApartmentManager);
                                    _context.SaveChanges();

                                    dataItem.TdcApartmentManagerId = tdcApartmentManager.Id;
                                }
                                else
                                {
                                    dataItem.TdcApartmentManagerId = tdcApartmentManager.Id;

                                    
                                }

                                DistrictAllocasionApartment districtAllocasionApartment = _context.DistrictAllocasionApartments.Where(m => m.Status != EntityStatus.DELETED).FirstOrDefault();
                                if (districtAllocasionApartment != null && dataItem.DistrictId != 0)
                                {
                                    districtAllocasionApartment = new DistrictAllocasionApartment();
                                    districtAllocasionApartment.TdcApartmentManagerId = dataItem.TdcApartmentManagerId;
                                    districtAllocasionApartment.DistrictId = dataItem.DistrictId;
                                    districtAllocasionApartment.ActualNumber = dataItem.ActualNumber;
                                    districtAllocasionApartment.CreatedById = userId;
                                    districtAllocasionApartment.CreatedBy = $"{fullName} (Excel)";


                                    _context.DistrictAllocasionApartments.Add(districtAllocasionApartment);
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

        public static List<ApartmentDataImport> importData(MemoryStream ms, int sheetnumber, int rowStart)
        {

            XSSFWorkbook workbook = new XSSFWorkbook(ms);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);

            List<ApartmentDataImport> res = new List<ApartmentDataImport>();
            for (int row = rowStart; row <= sheet.LastRowNum; row++)
            {
                if (sheet.GetRow(row) != null)
                {
                    ApartmentDataImport inputDetail = new ApartmentDataImport();
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

                            else if(i == 2)
                            {
                                try
                                {
                                    if(str != "")
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
                                        inputDetail.ApartmentTdcName = str;
                                        #region
                                        //kiểm tra dữ liệu tầng có hợp lệ
                                        //SimpleApartmentTdcData apartmentTdc = apartmentData.Where(e => e.Name == inputDetail.ApartmentTdcName ).FirstOrDefault();
                                        //if (apartmentTdc != null)
                                        //{
                                        //    inputDetail.FloorTdcId = apartmentTdc.FloorId;
                                        //    inputDetail.ApartmentTdcId = (int)apartmentTdc.Id;
                                        //    inputDetail.ExistApartment = true;
                                        //}
                                        #endregion
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột Căn chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Căn\n";
                                }
                            }

                            else if (i == 4)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TdcFloorName = str;
                                        #region
                                        //kiểm tra dữ liệu tầng có hợp lệ
                                        //string strNoneUnicode = UtilsService.NonUnicode(str);
                                        //string strNoneUnicodeCan = UtilsService.NonUnicode(inputDetail.ApartmentTdcName);

                                        //var apartment = (from f in floorData.AsEnumerable()
                                        //                 join a in apartmentData.AsEnumerable() on f.Id equals a.FloorId
                                        //                 where UtilsService.NonUnicode(f.Name).Contains(strNoneUnicode)
                                        //                 && UtilsService.NonUnicode(a.Name).Contains(strNoneUnicodeCan)
                                        //                 select a).FirstOrDefault();

                                        //if(apartment != null )
                                        //{
                                        //    inputDetail.ApartmentTdcId = (int)apartment.Id;
                                        //    inputDetail.FloorTdcId = (int)apartment.FloorId;
                                        //}
                                        //else
                                        //{
                                        //    SimpleFloorData floor = floorData.Where(e => e.Name == inputDetail.TdcFloorName).FirstOrDefault();
                                        //    if(floor != null)
                                        //    {
                                        //        inputDetail.Valid = false;
                                        //        inputDetail.ErrMsg += "Cột thông tin Tầng không hợp lệ \n";
                                        //        inputDetail.ExistFloor = false;
                                        //    }
                                        //    else
                                        //    {
                                        //        inputDetail.Valid = false;
                                        //        inputDetail.ErrMsg += "Cột thông tin Căn hộ không hợp lệ \n";
                                        //        inputDetail.ExistApartment = false;
                                        //    }
                                        //}
                                        #endregion
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột Tầng chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Tầng\n";
                                }
                            }

                            else if (i == 5)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TdcBlockName = str;
                                        #region
                                        //kiểm tra dữ liệu khối có hợp lệ
                                        //string strNoneUnicode = UtilsService.NonUnicode(str);
                                        //string strNoneUnicodeTang = UtilsService.NonUnicode(inputDetail.TdcFloorName);

                                        //var floor = (from bh in blockHouseData.AsEnumerable()
                                        //                 join f in floorData.AsEnumerable() on bh.Id equals f.BlockHouseId
                                        //                 where UtilsService.NonUnicode(bh.Name).Contains(strNoneUnicode)
                                        //                 && UtilsService.NonUnicode(f.Name).Contains(strNoneUnicodeTang)
                                        //                 select f).FirstOrDefault();
                                        //if (floor != null)
                                        //{
                                        //    inputDetail.FloorTdcId = (int)floor.Id;
                                        //    inputDetail.BlockHouseId = floor.BlockHouseId;
                                        //}
                                        //else
                                        //{
                                        //    SimpleBlockHouseData blockhouse = blockHouseData.Where(e => e.Name == inputDetail.TdcBlockName).FirstOrDefault();
                                        //    if (blockhouse != null)
                                        //    {
                                        //        inputDetail.Valid = false;
                                        //        inputDetail.ErrMsg += "Cột thông tin Khối không hợp lệ \n";
                                        //        inputDetail.ExistBlockHouse = false;
                                        //    }
                                        //    else
                                        //    {
                                        //        inputDetail.Valid = false;
                                        //        inputDetail.ErrMsg += "Cột thông tin Tầng không hợp lệ \n";
                                        //        inputDetail.ExistFloor = false;
                                        //    }
                                        //}
                                        #endregion
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột Khối chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Khối\n";
                                }
                            }

                            else if (i == 6)
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

                            else if (i == 7)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TdcApartmentArea = double.Parse(str);
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Diện tích\n";
                                }
                            }

                            else if (i == 8)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TdcApartmentCountRoom = int.Parse(str);
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Loại diện tích, số phòng ngủ\n";
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
                                    // if (str == "Chưa tiếp nhận")
                                    // {
                                    //     inputDetail.ReceptionTime = TypeReception.ReceivedYet;
                                    // }
                                    // if (str == "Không tiếp nhận")
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

                            else if (i== 15)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TypeDecisionName = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = true;
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Quyết định\n";
                                }
                            }

                            else if(i == 16)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.DistrictName = str;
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
