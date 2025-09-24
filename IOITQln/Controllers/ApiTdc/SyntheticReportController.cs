using AutoMapper;
using DevExpress.ClipboardSource.SpreadsheetML;
using DevExpress.XtraRichEdit.Import.Html;
using IOITQln.Common.Constants;
using IOITQln.Common.Enums;
using IOITQln.Common.Services;
using IOITQln.Common.ViewModels.Common;
using IOITQln.Entities;
using IOITQln.Migrations;
using IOITQln.Models.Data;
using IOITQln.Models.Dto;
using IOITQln.Persistence;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Controllers.ApiTdc
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SyntheticReportController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("report_TDC", "report_TDC");
        private static string functionCode = "report_TDC";
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        private IHostingEnvironment _hostingEnvironment;

        public SyntheticReportController(ApiDbContext context, IMapper mapper, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet("ReportTDC2/{typeLegal}")]
        public async Task<IActionResult> ReportTDC2(int typeLegal)
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
            DefaultResponse def = new DefaultResponse();
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.CREATE))
            {
                return new FileContentResult(new byte[0], "application/octet-stream");
            }

            try
            {
                List<SyntheticReportData> syntheticReportDatas = new List<SyntheticReportData>();

                List<TdcApartmentManager> tdcApartmentManagers = _context.TdcApartmentManagers.Where(l => l.Status != EntityStatus.DELETED && l.TypeLegalId == typeLegal).ToList();
                List<TdcApartmentManagerData> resApartment = _mapper.Map<List<TdcApartmentManagerData>>(tdcApartmentManagers.ToList());
                foreach (TdcApartmentManagerData item in resApartment)
                {
                    item.TdcProjectName = _context.TDCProjects.Where(f => f.Id == item.TdcProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                    item.TdcLandName = _context.Lands.Where(f => f.Id == item.LandId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                    item.TdcBlockHouseName = _context.BlockHouses.Where(f => f.Id == item.BlockHouseId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                    item.TdcFloorName = _context.FloorTdcs.Where(f => f.Id == item.FloorTdcId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                    item.TdcApartmentName = _context.ApartmentTdcs.Where(f => f.Id == item.ApartmentTdcId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                    item.DistrictProjectName = _context.Districts.Where(f => f.Id == item.DistrictProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                    item.TypeLegalName = _context.TypeAttributeItems.Where(f => f.Id == item.TypeLegalId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                }

                foreach (var item in resApartment)
                {
                    var itemSynthetic = new SyntheticReportData();

                    itemSynthetic.TypeLegalId = item.TypeLegalId;
                    itemSynthetic.TypeLegalName = item.TypeLegalName;
                    itemSynthetic.DistrictProjectId = item.DistrictProjectId;
                    itemSynthetic.DistrictProjectName = item.DistrictProjectName;
                    itemSynthetic.TdcProjectId = item.TdcProjectId;
                    itemSynthetic.TdcProjectName = item.TdcProjectName;
                    itemSynthetic.ReceptionTimeApartment = item.ReceptionTime;
                    syntheticReportDatas.Add(itemSynthetic);
                }

                List<TdcPlatformManager> tdcPlatformManagers = _context.TdcPlatformManagers.Where(l => l.Status != EntityStatus.DELETED && l.TypeLegalId == typeLegal).ToList();
                List<TdcPlatformManagerData> resPlatform = _mapper.Map<List<TdcPlatformManagerData>>(tdcPlatformManagers.ToList());
                foreach (TdcPlatformManagerData item in resPlatform)
                {
                    item.TdcProjectName = _context.TDCProjects.Where(f => f.Id == item.TdcProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                    item.TdcLandName = _context.Lands.Where(f => f.Id == item.LandId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                    item.TdcPlatformName = _context.BlockHouses.Where(f => f.Id == item.PlatformTdcId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                    item.DistrictProjectName = _context.Districts.Where(f => f.Id == item.DistrictProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                    item.TypeLegalName = _context.TypeAttributeItems.Where(f => f.Id == item.TypeLegalId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                }

                foreach (var item in resPlatform)
                {
                    var itemSynthetics = new SyntheticReportData();
                    itemSynthetics.TypeLegalId = item.TypeLegalId;
                    itemSynthetics.TypeLegalName = item.TypeLegalName;
                    itemSynthetics.DistrictProjectId = item.DistrictProjectId;
                    itemSynthetics.DistrictProjectName = item.DistrictProjectName;
                    itemSynthetics.TdcProjectId = item.TdcProjectId;
                    itemSynthetics.TdcProjectName = item.TdcProjectName;
                    itemSynthetics.ReceptionTimePlatform = item.ReceptionTime;
                    syntheticReportDatas.Add(itemSynthetics);
                }

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = syntheticReportDatas;
                return Ok(def);

            }

            catch (Exception ex)
            {
                log.Error("ReportNOC Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [HttpPost("ExportReportTDC2")]
        public async Task<IActionResult> ExportReportTDC2([FromBody] List<SyntheticReportData> input)
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
            int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id").Select(c => c.Value).SingleOrDefault());
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();

            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.CREATE))
            {
                return new FileContentResult(new byte[0], "application/octet-stream");
            }

            // khởi tạo wb rỗng
            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"ReportTDC/RP-2-All.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);

            MemoryStream ms = WriteDataToExcelTDC2(templatePath, 0, input);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", "BangChietTinh_");
        }

        private static MemoryStream WriteDataToExcelTDC2(string templatePath, int sheetnumber, List<SyntheticReportData> data)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);

            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();

            int rowTitle = 6;
            int rowStart = 10;
            int rowStart1 = 11;

            if (sheet != null)
            {
                try
                {
                    int k = 0;
                    int datacol = 12;

                    var style = workbook.CreateCellStyle();
                    style.BorderBottom = BorderStyle.Thin;
                    style.BorderLeft = BorderStyle.Thin;
                    style.BorderRight = BorderStyle.Thin;
                    style.BorderTop = BorderStyle.Thin;

                    ICellStyle cellStyle = workbook.CreateCellStyle();
                    IFont font = workbook.CreateFont();
                    font.FontName = "Times New Roman";
                    cellStyle.SetFont(font);
                    cellStyle.Alignment = HorizontalAlignment.Center;
                    cellStyle.BorderBottom = BorderStyle.Thin;
                    cellStyle.BorderLeft = BorderStyle.Thin;
                    cellStyle.BorderRight = BorderStyle.Thin;
                    cellStyle.BorderTop = BorderStyle.Thin;

                    IRow rowtitle = sheet.CreateRow(rowTitle);

                    int totalCountLegalApartment = 0;
                    int totalCountReceivedApartment = 0;
                    int totalCountReceivedYetApartment = 0;
                    int totalCountNotReceivedApartment = 0;

                    int totalCountLegalPlatform = 0;
                    int totalCountReceivedPlatform = 0;
                    int totalCountReceivedYetPlatform = 0;
                    int totalCountNotReceivedPlatform = 0;

                    var groupByLegal = data.GroupBy(x => x.TypeLegalName).ToList();

                    var currentDate = DateTime.Now;
                    var firstTypeLegal = data.First().TypeLegalName;

                    ICell cellTitle = rowtitle.CreateCell(2);
                    cellTitle.SetCellValue($"DANH SÁCH TIẾP NHẬN QUỸ NHÀ Ở, ĐẤT Ở PHỤC VỤ TÁI ĐỊNH CƯ THEO {firstTypeLegal} CỦA ỦY BAN NHÂN DÂN THÀNH PHỐ HỒ CHÍ MINH\r\n(Số liệu tính đến ngày {currentDate.ToString("dd/MM/yyyy")})");
                    cellTitle.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader2 = new CellRangeAddress(rowTitle, rowTitle, 2, 13);
                    RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader2, sheet);
                    RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader2, sheet);
                    RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader2, sheet);
                    RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader2, sheet);
                    sheet.AddMergedRegion(mergedRegioncellHeader2);

                    foreach (var itemDecison in groupByLegal)
                    {
                        var groupbyDistrictProject = itemDecison.GroupBy(x => x.DistrictProjectName).ToList();
                        int districtcount = 1;

                        foreach (var itemDistrict in groupbyDistrictProject)
                        {
                            IRow row = sheet.CreateRow(rowStart);
                            IRow row1 = sheet.CreateRow(rowStart1);

                            ICellStyle cellStyleDate = workbook.CreateCellStyle();
                            IFont fontDate = workbook.CreateFont();
                            fontDate.FontName = "Times New Roman";
                            cellStyleDate.SetFont(fontDate);
                            cellStyleDate.DataFormat = workbook.CreateDataFormat().GetFormat("dd/mm/yyyy");
                            cellStyleDate.BorderBottom = BorderStyle.Thin;
                            cellStyleDate.BorderLeft = BorderStyle.Thin;
                            cellStyleDate.BorderRight = BorderStyle.Thin;
                            cellStyleDate.BorderTop = BorderStyle.Thin;

                            ICellStyle cellStyleMoney = workbook.CreateCellStyle();
                            var dataFormat = workbook.CreateDataFormat();
                            cellStyleMoney.DataFormat = dataFormat.GetFormat("#,##0");
                            cellStyleMoney.BorderBottom = BorderStyle.Thin;
                            cellStyleMoney.BorderLeft = BorderStyle.Thin;
                            cellStyleMoney.BorderRight = BorderStyle.Thin;
                            cellStyleMoney.BorderTop = BorderStyle.Thin;

                            string districtProjectName = itemDistrict.Key;

                            //căn hộ
                            int countReceivedApartment = 0;
                            int countReceivedYetApartment = 0;
                            int countNotReceivedApartment = 0;

                            var dataApartment = itemDistrict.Where(x => x.ReceptionTimeApartment != null);

                            int countLegalApartment = dataApartment.Count();//fix

                            var groupbyReceptionTimeApartment = itemDistrict.GroupBy(x => x.ReceptionTimeApartment);
                            foreach (var receptionTime in groupbyReceptionTimeApartment)
                            {
                                if (receptionTime.Key == TypeReception.Received)
                                {
                                    countReceivedApartment = receptionTime.Count();
                                }
                                else if (receptionTime.Key == TypeReception.ReceivedYet)
                                {
                                    countReceivedYetApartment = receptionTime.Count();
                                }
                                else if (receptionTime.Key == TypeReception.NotReceived)
                                {
                                    countNotReceivedApartment = receptionTime.Count();
                                }
                            }

                            //nền đất
                            int countReceivedPlatform = 0;
                            int countReceivedYetPlatform = 0;
                            int countNotReceivedPlatform = 0;

                            var dataPlatform = itemDistrict.Where(x => x.ReceptionTimePlatform != null);

                            int countLegalPlatform = dataPlatform.Count();//fix

                            var groupbyReceptionTimePlatform = itemDistrict.GroupBy(x => x.ReceptionTimePlatform);
                            foreach (var receptionTime in groupbyReceptionTimePlatform)
                            {
                                if (receptionTime.Key == TypeReception.Received)
                                {
                                    countReceivedPlatform = receptionTime.Count();
                                }
                                else if (receptionTime.Key == TypeReception.ReceivedYet)
                                {
                                    countReceivedYetPlatform = receptionTime.Count();
                                }
                                else if (receptionTime.Key == TypeReception.NotReceived)
                                {
                                    countNotReceivedPlatform = receptionTime.Count();
                                }
                            }

                            totalCountLegalApartment += countLegalApartment;
                            totalCountReceivedApartment += countReceivedApartment;
                            totalCountReceivedYetApartment += countReceivedYetApartment;
                            totalCountNotReceivedApartment += countNotReceivedApartment;

                            totalCountLegalPlatform += countLegalPlatform;
                            totalCountReceivedPlatform += countReceivedPlatform;
                            totalCountReceivedYetPlatform += countReceivedYetPlatform;
                            totalCountNotReceivedPlatform += countNotReceivedPlatform;

                            ICell cell = row1.CreateCell(2);
                            cell.SetCellValue(districtcount++);
                            cell.CellStyle = cellStyle;

                            ICell cell1 = row1.CreateCell(3);
                            cell1.SetCellValue(districtProjectName);
                            cell1.CellStyle = cellStyle;

                            ICell cell2 = row1.CreateCell(4);
                            cell2.SetCellValue("");
                            cell2.CellStyle = cellStyle;

                            //căn hộ
                            ICell cell3 = row1.CreateCell(5);
                            cell3.SetCellValue(countLegalApartment);
                            cell3.CellStyle = cellStyle;

                            ICell cell4 = row1.CreateCell(6);
                            cell4.SetCellValue(countReceivedApartment);
                            cell4.CellStyle = cellStyle;

                            ICell cell5 = row1.CreateCell(7);
                            cell5.SetCellValue(countReceivedYetApartment);
                            cell5.CellStyle = cellStyle;

                            ICell cell6 = row1.CreateCell(8);
                            cell6.SetCellValue(countNotReceivedApartment);
                            cell6.CellStyle = cellStyle;

                            //nền đất
                            ICell cell7 = row1.CreateCell(9);
                            cell7.SetCellValue(countLegalPlatform);
                            cell7.CellStyle = cellStyle;

                            ICell cell8 = row1.CreateCell(10);
                            cell8.SetCellValue(countReceivedPlatform);
                            cell8.CellStyle = cellStyle;

                            ICell cell9 = row1.CreateCell(11);
                            cell9.SetCellValue(countReceivedYetPlatform);
                            cell9.CellStyle = cellStyle;

                            ICell cell10 = row1.CreateCell(12);
                            cell10.SetCellValue(countNotReceivedPlatform);
                            cell10.CellStyle = cellStyle;

                            ICell cell11 = row1.CreateCell(13);
                            cell11.SetCellValue("");
                            cell11.CellStyle = cellStyle;

                            rowStart1++;
                            k = 0;

                            var groupbyProjects = itemDistrict.GroupBy(x => x.TdcProjectName);

                            foreach (var project in groupbyProjects)
                            {
                                IRow row2 = sheet.CreateRow(rowStart1);

                                string tdcProjectName = project.Key;

                                int countReceivedApartments = 0;
                                int countReceivedYetApartments = 0;
                                int countNotReceivedApartments = 0;
                                var dataApartments = project.Where(x => x.ReceptionTimeApartment != null);
                                int countLegalApartments = dataApartments.Count();

                                var groupbyReceptionTimeApartments = project.GroupBy(x => x.ReceptionTimeApartment);
                                foreach (var recepTime in groupbyReceptionTimeApartments)
                                {
                                    if (recepTime.Key == TypeReception.Received)
                                    {
                                        countReceivedApartments = recepTime.Count();
                                    }
                                    else if (recepTime.Key == TypeReception.ReceivedYet)
                                    {
                                        countReceivedYetApartments = recepTime.Count();
                                    }
                                    else if (recepTime.Key == TypeReception.NotReceived)
                                    {
                                        countNotReceivedApartments = recepTime.Count();
                                    }
                                }

                                int countReceivedPlatforms = 0;
                                int countReceivedYetPlatforms = 0;
                                int countNotReceivedPlatforms = 0;
                                var dataPlatforms = project.Where(x => x.ReceptionTimePlatform != null);
                                int countLegalPlatforms = dataPlatforms.Count();

                                var groupbyReceptionTimePlatforms = project.GroupBy(x => x.ReceptionTimePlatform);
                                foreach (var recepTime in groupbyReceptionTimePlatforms)
                                {
                                    if (recepTime.Key == TypeReception.Received)
                                    {
                                        countReceivedPlatforms = recepTime.Count();
                                    }
                                    else if (recepTime.Key == TypeReception.ReceivedYet)
                                    {
                                        countReceivedYetPlatforms = recepTime.Count();
                                    }
                                    else if (recepTime.Key == TypeReception.NotReceived)
                                    {
                                        countNotReceivedPlatforms = recepTime.Count();
                                    }
                                }

                                ICell cellprojectnull1 = row2.CreateCell(2);
                                cellprojectnull1.SetCellValue(" ");
                                cellprojectnull1.CellStyle = cellStyle;

                                ICell cellprojectnull2 = row2.CreateCell(3);
                                cellprojectnull2.SetCellValue(" ");
                                cellprojectnull2.CellStyle = cellStyle;

                                ICell cellproject = row2.CreateCell(4);
                                cellproject.SetCellValue(tdcProjectName);
                                cellproject.CellStyle = cellStyle;

                                ICell cellproject1 = row2.CreateCell(5);
                                cellproject1.SetCellValue(countLegalApartments);
                                cellproject1.CellStyle = cellStyle;

                                ICell cellproject2 = row2.CreateCell(6);
                                cellproject2.SetCellValue(countReceivedApartments);
                                cellproject2.CellStyle = cellStyle;

                                ICell cellproject3 = row2.CreateCell(7);
                                cellproject3.SetCellValue(countReceivedYetApartments);
                                cellproject3.CellStyle = cellStyle;

                                ICell cellproject4 = row2.CreateCell(8);
                                cellproject4.SetCellValue(countNotReceivedApartments);
                                cellproject4.CellStyle = cellStyle;

                                ICell cellproject5 = row2.CreateCell(9);
                                cellproject5.SetCellValue(countLegalPlatforms);
                                cellproject5.CellStyle = cellStyle;

                                ICell cellproject6 = row2.CreateCell(10);
                                cellproject6.SetCellValue(countReceivedPlatforms);
                                cellproject6.CellStyle = cellStyle;

                                ICell cellproject7 = row2.CreateCell(11);
                                cellproject7.SetCellValue(countReceivedYetPlatforms);
                                cellproject7.CellStyle = cellStyle;

                                ICell cellproject8 = row2.CreateCell(12);
                                cellproject8.SetCellValue(countNotReceivedPlatforms);
                                cellproject8.CellStyle = cellStyle;

                                ICell cellprojectnull3 = row2.CreateCell(13);
                                cellprojectnull3.SetCellValue(" ");
                                cellprojectnull3.CellStyle = cellStyle;
                                rowStart1++;
                            }
                        }
                    }

                    IRow rowTotal = sheet.CreateRow(rowStart);

                    ICell cellTotal5 = rowTotal.CreateCell(2);
                    cellTotal5.SetCellValue("TỔNG CỘNG");
                    cellTotal5.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader1 = new CellRangeAddress(rowStart, rowStart, 2, 4);
                    RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                    RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                    RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                    RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                    sheet.AddMergedRegion(mergedRegioncellHeader1);

                    ICell cellTotal1 = rowTotal.CreateCell(5);
                    cellTotal1.SetCellValue(totalCountLegalApartment);
                    cellTotal1.CellStyle = cellStyle;

                    ICell cellTotal2 = rowTotal.CreateCell(6);
                    cellTotal2.SetCellValue(totalCountReceivedApartment);
                    cellTotal2.CellStyle = cellStyle;

                    ICell cellTotal3 = rowTotal.CreateCell(7);
                    cellTotal3.SetCellValue(totalCountReceivedYetApartment);
                    cellTotal3.CellStyle = cellStyle;

                    ICell cellTotal4 = rowTotal.CreateCell(8);
                    cellTotal4.SetCellValue(totalCountNotReceivedApartment);
                    cellTotal4.CellStyle = cellStyle;

                    ICell cellTotal6 = rowTotal.CreateCell(9);
                    cellTotal6.SetCellValue(totalCountLegalPlatform);
                    cellTotal6.CellStyle = cellStyle;

                    ICell cellTotal7 = rowTotal.CreateCell(10);
                    cellTotal7.SetCellValue(totalCountReceivedPlatform);
                    cellTotal7.CellStyle = cellStyle;

                    ICell cellTotal8 = rowTotal.CreateCell(11);
                    cellTotal8.SetCellValue(totalCountReceivedYetPlatform);
                    cellTotal8.CellStyle = cellStyle;

                    ICell cellTotal9 = rowTotal.CreateCell(12);
                    cellTotal9.SetCellValue(totalCountNotReceivedPlatform);
                    cellTotal9.CellStyle = cellStyle;

                    ICell cellTotal10 = rowTotal.CreateCell(13);
                    cellTotal10.SetCellValue("");
                    cellTotal10.CellStyle = cellStyle;

                }
                catch (Exception ex)
                {
                    log.Error("Put Exception:" + ex);
                }
            }
            sheet.ForceFormulaRecalculation = true;

            MemoryStream ms = new MemoryStream();

            workbook.Write(ms);

            return ms;
        }

        [HttpGet("ReportTDC3")]
        public async Task<IActionResult> ReportTDC3()
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
            DefaultResponse def = new DefaultResponse();
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.CREATE))
            {
                return new FileContentResult(new byte[0], "application/octet-stream");
            }

            try
            {
                List<SyntheticReportData> syntheticReportDatas = new List<SyntheticReportData>();

                List<TdcApartmentManager> tdcApartmentManagers = _context.TdcApartmentManagers.Where(l => l.Status != EntityStatus.DELETED).ToList();
                List<TdcApartmentManagerData> resApartment = _mapper.Map<List<TdcApartmentManagerData>>(tdcApartmentManagers.ToList());
                foreach (TdcApartmentManagerData item in resApartment)
                {
                    item.TdcProjectName = _context.TDCProjects.Where(f => f.Id == item.TdcProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                    item.TdcLandName = _context.Lands.Where(f => f.Id == item.LandId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                    item.TdcBlockHouseName = _context.BlockHouses.Where(f => f.Id == item.BlockHouseId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                    item.TdcFloorName = _context.FloorTdcs.Where(f => f.Id == item.FloorTdcId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                    item.TdcApartmentName = _context.ApartmentTdcs.Where(f => f.Id == item.ApartmentTdcId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                    item.DistrictProjectName = _context.Districts.Where(f => f.Id == item.DistrictProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                    item.TypeLegalName = _context.TypeAttributeItems.Where(f => f.Id == item.TypeLegalId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                }

                foreach (var item in resApartment)
                {
                    var itemSynthetic = new SyntheticReportData();

                    itemSynthetic.TypeLegalId = item.TypeLegalId;
                    itemSynthetic.TypeLegalName = item.TypeLegalName;
                    itemSynthetic.DistrictProjectId = item.DistrictProjectId;
                    itemSynthetic.DistrictProjectName = item.DistrictProjectName;
                    itemSynthetic.TdcProjectIdApartment = item.TdcProjectId;
                    itemSynthetic.TdcProjectNameApartment = item.TdcProjectName;
                    itemSynthetic.ReceptionTimeApartment = item.ReceptionTime;
                    itemSynthetic.HandoverYearApartment = item.HandOverYear;
                    itemSynthetic.ApartmentTdcId = item.ApartmentTdcId;
                    itemSynthetic.TdcApartmentName = item.TdcApartmentName;
                    syntheticReportDatas.Add(itemSynthetic);
                }

                List<TdcPlatformManager> tdcPlatformManagers = _context.TdcPlatformManagers.Where(l => l.Status != EntityStatus.DELETED).ToList();
                List<TdcPlatformManagerData> resPlatform = _mapper.Map<List<TdcPlatformManagerData>>(tdcPlatformManagers.ToList());
                foreach (TdcPlatformManagerData item in resPlatform)
                {
                    item.TdcProjectName = _context.TDCProjects.Where(f => f.Id == item.TdcProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                    item.TdcLandName = _context.Lands.Where(f => f.Id == item.LandId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                    item.TdcPlatformName = _context.BlockHouses.Where(f => f.Id == item.PlatformTdcId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                    item.DistrictProjectName = _context.Districts.Where(f => f.Id == item.DistrictProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                    item.TypeLegalName = _context.TypeAttributeItems.Where(f => f.Id == item.TypeLegalId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
                }

                foreach (var item in resPlatform)
                {
                    var itemSynthetics = new SyntheticReportData();
                    itemSynthetics.TypeLegalId = item.TypeLegalId;
                    itemSynthetics.TypeLegalName = item.TypeLegalName;
                    itemSynthetics.DistrictProjectId = item.DistrictProjectId;
                    itemSynthetics.DistrictProjectName = item.DistrictProjectName;
                    itemSynthetics.TdcProjectIdPlatform = item.TdcProjectId;
                    itemSynthetics.TdcProjectNamePlatform = item.TdcProjectName;
                    itemSynthetics.ReceptionTimePlatform = item.ReceptionTime;
                    itemSynthetics.HandoverYearPlatform = item.HandOverYear;
                    itemSynthetics.PlatformTdcId = item.PlatformTdcId;
                    itemSynthetics.TdcPlatformName = item.TdcPlatformName;
                    syntheticReportDatas.Add(itemSynthetics);
                }

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = syntheticReportDatas;
                return Ok(def);

            }

            catch (Exception ex)
            {
                log.Error("ReportNOC Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [HttpPost("ExportReportTDC3")]
        public async Task<IActionResult> ExportReportTDC3([FromBody] List<SyntheticReportData> input)
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
            int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id").Select(c => c.Value).SingleOrDefault());
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();

            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.CREATE))
            {
                return new FileContentResult(new byte[0], "application/octet-stream");
            }

            // khởi tạo wb rỗng
            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"ReportTDC/RP3-All.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);

            MemoryStream ms = WriteDataToExcelTDC3(templatePath, 0, input);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", "BangChietTinh_");
        }

        private static MemoryStream WriteDataToExcelTDC3(string templatePath, int sheetnumber, List<SyntheticReportData> data)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);

            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();

            int rowStart = 8;

            int totalTypeLegal = 0;

            int totalProjectsApartment = 0;
            int totalProjectsPlatform = 0;

            int totalReceivedApartment = 0;
            int totalReceivedYetApartment = 0;
            int totalNotReceivedApartment = 0;
            int totalhandoveryearApartment = 0;
            int totalhandyearnullApartment = 0;

            int totalReceivedPlatform = 0;
            int totalReceivedYetPlatform = 0;
            int totalNotReceivedPlatform = 0;
            int totalhandoveryearPlatform = 0;
            int totalhandyearnullPlatform = 0;

            if (sheet != null)
            {
                try
                {
                    int k = 0;
                    int datacol = 0;

                    datacol = 14;

                    var style = workbook.CreateCellStyle();
                    style.BorderBottom = BorderStyle.Thin;
                    style.BorderLeft = BorderStyle.Thin;
                    style.BorderRight = BorderStyle.Thin;
                    style.BorderTop = BorderStyle.Thin;

                    ICellStyle cellStyle = workbook.CreateCellStyle();
                    IFont font = workbook.CreateFont();
                    font.FontName = "Times New Roman";
                    cellStyle.SetFont(font);
                    cellStyle.Alignment = HorizontalAlignment.Center;
                    cellStyle.BorderBottom = BorderStyle.Thin;
                    cellStyle.BorderLeft = BorderStyle.Thin;
                    cellStyle.BorderRight = BorderStyle.Thin;
                    cellStyle.BorderTop = BorderStyle.Thin;

                    var groupbyLegal = data.GroupBy(x => x.TypeLegalName);
                    int typeLegalcount = 1;

                    foreach (var itemDecison in groupbyLegal)
                    {
                        IRow row = sheet.CreateRow(rowStart);

                        string typeLegalName = itemDecison.Key;
                        int totalProjectCount = itemDecison.Count();

                        int totalprojectsApartment = 0;
                        int totalprojectsPlatform = 0;

                        int countReceivedApartment = 0;
                        int countReceivedYetApartment = 0;
                        int countNotReceivedApartment = 0;

                        var groupedApartment = itemDecison.Where(x => x.TdcProjectIdApartment.HasValue).GroupBy(x => x.TypeLegalName);

                        foreach (var group in groupedApartment)
                        {
                            if (group.Count() > 0)
                            {
                                var apartmentCount = group.Count();
                                totalprojectsApartment = apartmentCount;
                            }
                        }

                        var groupedPlatform = itemDecison.Where(x => x.TdcProjectIdPlatform.HasValue).GroupBy(x => x.TypeLegalName);
                        foreach (var group in groupedPlatform)
                        {
                            if (group.Count() > 0)
                            {
                                var platformCount = group.Count();
                                totalprojectsPlatform = platformCount;
                            }
                        }

                        var dataApartment = itemDecison.Where(x => x.ReceptionTimeApartment != null);
                        int countLegalApartment = dataApartment.Count();
                        var groupbyReceptionTimeApartment = itemDecison.GroupBy(x => x.ReceptionTimeApartment);
                        foreach (var receptionTimeApartment in groupbyReceptionTimeApartment)
                        {
                            if (receptionTimeApartment.Key == TypeReception.Received)
                            {
                                countReceivedApartment = receptionTimeApartment.Count();
                            }
                            else if (receptionTimeApartment.Key == TypeReception.ReceivedYet)
                            {
                                countReceivedYetApartment = receptionTimeApartment.Count();
                            }
                            else if (receptionTimeApartment.Key == TypeReception.NotReceived)
                            {
                                countNotReceivedApartment = receptionTimeApartment.Count();
                            }
                        }

                        int countReceivedPlatform = 0;
                        int countReceivedYetPlatform = 0;
                        int countNotReceivedPlatform = 0;

                        var dataPlatform = itemDecison.Where(x => x.ReceptionTimePlatform != null);
                        int countLegalPlatform = dataPlatform.Count();
                        var groupbyReceptionTimePlatform = itemDecison.GroupBy(x => x.ReceptionTimePlatform);
                        foreach (var receptionTimePlatform in groupbyReceptionTimePlatform)
                        {
                            if (receptionTimePlatform.Key == TypeReception.Received)
                            {
                                countReceivedPlatform = receptionTimePlatform.Count();
                            }
                            else if (receptionTimePlatform.Key == TypeReception.ReceivedYet)
                            {
                                countReceivedYetPlatform = receptionTimePlatform.Count();
                            }
                            else if (receptionTimePlatform.Key == TypeReception.NotReceived)
                            {
                                countNotReceivedPlatform = receptionTimePlatform.Count();
                            }
                        }

                        var dataApartments = itemDecison.Where(x => x.ApartmentTdcId.HasValue);

                        int countHandoverYearApartment = 0;
                        int countNullHandoverYearApartment = 0;

                        var grouphandoverApartment = dataApartments.GroupBy(x => x.HandoverYearApartment);

                        foreach (var overyearApartment in grouphandoverApartment)
                        {
                            if (overyearApartment.Key !=null)
                            {
                                countHandoverYearApartment += overyearApartment.Count();
                            }
                            else
                            {
                                countNullHandoverYearApartment += overyearApartment.Count();
                            }
                        }

                        var dataPlatforms = itemDecison.Where(x => x.PlatformTdcId.HasValue);

                        var countHandoverYearPlatform = 0;
                        var countNullHandoverYearPlatform = 0;

                        var grouphandoverPlatform = dataPlatforms.GroupBy(x => x.HandoverYearPlatform);

                        foreach (var overyearPlatform in grouphandoverPlatform)
                        {
                            if (overyearPlatform.Key !=null)
                            {
                                countHandoverYearPlatform += overyearPlatform.Count();
                            }
                            else
                            {
                                countNullHandoverYearPlatform += overyearPlatform.Count();
                            }
                        }

                        totalTypeLegal++;

                        totalProjectsApartment += totalprojectsApartment;
                        totalProjectsPlatform += totalprojectsPlatform;

                        totalhandoveryearApartment += countHandoverYearApartment;
                        totalhandyearnullApartment += countNullHandoverYearApartment;

                        totalhandoveryearPlatform += countHandoverYearPlatform;
                        totalhandyearnullPlatform += countNullHandoverYearPlatform;


                        totalReceivedApartment += countReceivedApartment;
                        totalReceivedYetApartment += countReceivedYetApartment;
                        totalNotReceivedApartment += countNotReceivedApartment;

                        totalReceivedPlatform += countReceivedPlatform;
                        totalReceivedYetPlatform += countReceivedYetPlatform;
                        totalNotReceivedPlatform += countNotReceivedPlatform;

                        ICell cell = row.CreateCell(0);
                        cell.SetCellValue(typeLegalcount++);
                        cell.CellStyle = cellStyle;

                        ICell cell1 = row.CreateCell(1);
                        cell1.SetCellValue(typeLegalName);
                        cell1.CellStyle = cellStyle;

                        ICell cell2 = row.CreateCell(2);
                        cell2.SetCellValue(totalprojectsApartment);
                        cell2.CellStyle = cellStyle;

                        ICell cell3 = row.CreateCell(3);
                        cell3.SetCellValue(totalprojectsPlatform);
                        cell3.CellStyle = cellStyle;

                        ICell cell4 = row.CreateCell(4);
                        cell4.SetCellValue(countReceivedApartment);//căn hộ nhận
                        cell4.CellStyle = cellStyle;

                        ICell cell5 = row.CreateCell(5);
                        cell5.SetCellValue(countReceivedPlatform);//nền đất nhận
                        cell5.CellStyle = cellStyle;

                        ICell cell6 = row.CreateCell(6);
                        cell6.SetCellValue(countReceivedYetApartment);//căn hộ chưa nhận
                        cell6.CellStyle = cellStyle;

                        ICell cell7 = row.CreateCell(7);
                        cell7.SetCellValue(countReceivedYetPlatform);//nền đất chưa nhận
                        cell7.CellStyle = cellStyle;

                        ICell cell8 = row.CreateCell(8);
                        cell8.SetCellValue(countNotReceivedApartment);//căn hộ không nhận
                        cell8.CellStyle = cellStyle;

                        ICell cell9 = row.CreateCell(9);
                        cell9.SetCellValue(countNotReceivedPlatform);//nền đất không nhận
                        cell9.CellStyle = cellStyle;

                        ICell cell10 = row.CreateCell(10);
                        cell10.SetCellValue(countHandoverYearApartment);//căn hộ đã bàn giao
                        cell10.CellStyle = cellStyle;

                        ICell cell11 = row.CreateCell(11);
                        cell11.SetCellValue(countHandoverYearPlatform);//nền đất đã bàn giao
                        cell11.CellStyle = cellStyle;

                        ICell cell12 = row.CreateCell(12);
                        cell12.SetCellValue(countNullHandoverYearApartment);//căn hộ chưa bàn giao
                        cell12.CellStyle = cellStyle;

                        ICell cell13 = row.CreateCell(13);
                        cell13.SetCellValue(countNullHandoverYearPlatform);//nền đất chưa bàn giao 
                        cell13.CellStyle = cellStyle;

                        ICell cell14 = row.CreateCell(14);
                        cell14.SetCellValue("");
                        cell14.CellStyle = cellStyle;

                        rowStart++;

                    }

                    IRow totalRow = sheet.CreateRow(rowStart);

                    ICell celltotal = totalRow.CreateCell(0);
                    celltotal.SetCellValue("TỔNG CỘNG");
                    celltotal.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader1 = new CellRangeAddress(rowStart, rowStart, 0, 1);
                    RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                    RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                    RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                    RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                    sheet.AddMergedRegion(mergedRegioncellHeader1);

                    ICell celltotal1 = totalRow.CreateCell(2);
                    celltotal1.SetCellValue(totalProjectsApartment);
                    celltotal1.CellStyle = cellStyle;

                    ICell celltotal2 = totalRow.CreateCell(3);
                    celltotal2.SetCellValue(totalProjectsPlatform);
                    celltotal2.CellStyle = cellStyle;

                    ICell celltotal3 = totalRow.CreateCell(4);
                    celltotal3.SetCellValue(totalReceivedApartment);
                    celltotal3.CellStyle = cellStyle;

                    ICell celltotal4 = totalRow.CreateCell(5);
                    celltotal4.SetCellValue(totalReceivedPlatform);
                    celltotal4.CellStyle = cellStyle;

                    ICell celltotal5 = totalRow.CreateCell(6);
                    celltotal5.SetCellValue(totalReceivedYetApartment);
                    celltotal5.CellStyle = cellStyle;

                    ICell celltotal6 = totalRow.CreateCell(7);
                    celltotal6.SetCellValue(totalReceivedYetPlatform);
                    celltotal6.CellStyle = cellStyle;

                    ICell celltotal7 = totalRow.CreateCell(8);
                    celltotal7.SetCellValue(totalNotReceivedApartment);
                    celltotal7.CellStyle = cellStyle;

                    ICell celltotal8 = totalRow.CreateCell(9);
                    celltotal8.SetCellValue(totalNotReceivedPlatform);
                    celltotal8.CellStyle = cellStyle;

                    ICell celltotal9 = totalRow.CreateCell(10);
                    celltotal9.SetCellValue(totalhandoveryearApartment);
                    celltotal9.CellStyle = cellStyle;

                    ICell celltotal10 = totalRow.CreateCell(11);
                    celltotal10.SetCellValue(totalhandoveryearPlatform);
                    celltotal10.CellStyle = cellStyle;

                    ICell celltotal11 = totalRow.CreateCell(12);
                    celltotal11.SetCellValue(totalhandyearnullApartment);
                    celltotal11.CellStyle = cellStyle;

                    ICell celltotal12 = totalRow.CreateCell(13);
                    celltotal12.SetCellValue(totalhandyearnullPlatform);
                    celltotal12.CellStyle = cellStyle;

                    ICell celltotal13 = totalRow.CreateCell(14);
                    celltotal13.SetCellValue("");
                    celltotal13.CellStyle = cellStyle;
                }
                catch (Exception ex)
                {
                    log.Error("Put Exception:" + ex);
                }
            }
            sheet.ForceFormulaRecalculation = true;
            MemoryStream ms2 = new MemoryStream();
            workbook.Write(ms2);

            return ms2;
        }
    }
}
