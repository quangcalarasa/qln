using AutoMapper;
using DevExpress.ClipboardSource.SpreadsheetML;
using DevExpress.Office.Utils;
using DevExpress.Utils.DirectXPaint;
using DevExpress.XtraExport.Helpers;
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
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using NPOI.HSSF.Record.Chart;
using NPOI.SS.Formula.Functions;
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


namespace IOITQln.Controllers.ApiNoc
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class IdentifierController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("REPORT", "REPORT");
        private static string functionCode = "REPORT_NOC_MANAGEMENT";
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        private IHostingEnvironment _hostingEnvironment;

        public IdentifierController(ApiDbContext context, IMapper mapper, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet("ReportNOC5/{DateStart}/{DateEnd}")]
        public async Task<IActionResult> ReportNOC5(DateTime DateStart, DateTime DateEnd)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            //check role
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
                List<IdentifierData> identifierDatas = new List<IdentifierData>();
                List<Block> blDataAll = _context.Blocks.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<District> dData = _context.Districts.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<TypeBlock> tbData = _context.TypeBlocks.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();

                List<Block> blocks = blDataAll.Where(l => l.TypeReportApply == AppEnums.TypeReportApply.NHA_RIENG_LE && l.Status != AppEnums.EntityStatus.DELETED && l.TypeBlockEntity == AppEnums.TypeBlockEntity.BLOCK_RENT).ToList();
                List<BlockData> resBlock = _mapper.Map<List<BlockData>>(blocks.ToList());
                foreach (BlockData item in resBlock)
                {
                    item.DistrictName = dData.Where(l => l.Id == item.District).Select(p => p.Name).FirstOrDefault();
                }


                foreach (var item in resBlock)
                {
                    var itemIdentifier = new IdentifierData();

                    if (DateStart < item.DateRecord && item.DateRecord < DateEnd)
                    {
                        itemIdentifier.DistrictId = item.District;
                        itemIdentifier.DistrictName = item.DistrictName;
                        itemIdentifier.TypeBlockId = item.TypeBlockId;
                        itemIdentifier.UsageStatus = item.UsageStatus;
                        itemIdentifier.TypeHouse = item.TypeHouse;
                        itemIdentifier.LandArea = (decimal?)item.CampusArea;
                        itemIdentifier.ConstructionArea = (decimal?)item.ConstructionAreaValue;
                        itemIdentifier.UsableArea = (decimal?)item.UseAreaValue;
                        itemIdentifier.Dispute = item.Dispute;
                        itemIdentifier.Blueprint = item.Blueprint;
                        itemIdentifier.EstablishStateOwnership = item.EstablishStateOwnership;
                        itemIdentifier.TakeOver = item.TakeOver;
                        identifierDatas.Add(itemIdentifier);
                    }
                }

            

                List<Apartment> apartments = _context.Apartments.Where(l => l.Status != AppEnums.EntityStatus.DELETED && l.TypeApartmentEntity == AppEnums.TypeApartmentEntity.APARTMENT_RENT).ToList();
                List<ApartmentData> resApartments = _mapper.Map<List<ApartmentData>>(apartments.ToList());
                foreach (ApartmentData item in resApartments)
                {
                    item.DistrictId = blDataAll.Where(l => l.Id == item.BlockId ).Select(p => p.District).FirstOrDefault();
                    item.DistrictName = dData.Where(l => l.Id == item.DistrictId ).Select(p => p.Name).FirstOrDefault();
                    item.TypeBlockId = blDataAll.Where(l => l.Id == item.BlockId ).Select(p => p.TypeBlockId).FirstOrDefault();
                }

                foreach (var item in resApartments)
                {
                    var itemIdentifier1 = new IdentifierData();

                    if (DateStart < item.DateRecord && item.DateRecord < DateEnd)
                    {
                        itemIdentifier1.DistrictId = item.DistrictId;
                        itemIdentifier1.DistrictName = item.DistrictName;
                        itemIdentifier1.TypeBlockId = item.TypeBlockId;
                        itemIdentifier1.UsageStatus = item.UsageStatus;
                        itemIdentifier1.TypeHouse = item.TypeHouse;
                        itemIdentifier1.LandArea = (decimal?)item.CampusArea;
                        itemIdentifier1.ConstructionArea = (decimal?)item.ConstructionAreaValue;
                        itemIdentifier1.UsableArea = (decimal?)item.UseAreaValue;
                        itemIdentifier1.Dispute = item.Dispute;
                        itemIdentifier1.Blueprint = item.Blueprint;
                        itemIdentifier1.EstablishStateOwnership = item.EstablishStateOwnership;
                        itemIdentifier1.TakeOver = item.TakeOver;
                        identifierDatas.Add(itemIdentifier1);
                    }
                }

                var groupDataByDistrictId = identifierDatas.GroupBy(x => x.DistrictId).Select(e => new GroupDataNOC
                {
                    DistrictId = (int)e.Key,
                    groupByDistrict = e.ToList().GroupBy(x => new
                    {
                        x.TypeBlockId,
                    }).Select(e => new GroupByTypeBlockID
                    {
                        TypeBlockID = e.Key.TypeBlockId,
                        groupByTypeBlockID = e.ToList().GroupBy(x => x.UsageStatus).Select(e => new GroupDataByTypeUsageStatus
                        {
                            UsageStatus = e.Key,
                            groupDataByTypeUsageStatus = e.ToList()
                        }).ToList()
                    }).ToList()
                }).ToList();

                List<IdentifierData> identifierDatasNOC = new List<IdentifierData>();


                foreach (var itemData in groupDataByDistrictId)
                {
                    var identifierData = new IdentifierData();

                    int Quantity = 0;
                    decimal? LandArea = 0;
                    decimal? ConstructionArea = 0;
                    decimal? UsableArea = 0;
                    foreach (var item in itemData.groupByDistrict)
                    {
                        foreach (var item1 in item.groupByTypeBlockID)
                        {
                            var data = new IdentifierData();
                            foreach (var childitem in item1.groupDataByTypeUsageStatus)
                            {
                                data.DistrictId = childitem.DistrictId;
                                data.DistrictName = childitem.DistrictName;
                                data.TypeBlockId = childitem.TypeBlockId;
                                data.TypeBlockName = tbData.Where(l => l.Id == data.TypeBlockId).Select(p => p.Name).FirstOrDefault();
                                data.UsageStatus = childitem.UsageStatus;
                                data.Quantity = item1.groupDataByTypeUsageStatus.Count;
                                data.LandArea = item1.groupDataByTypeUsageStatus.Sum(p => p.LandArea);
                                data.ConstructionArea = item1.groupDataByTypeUsageStatus.Sum(p => p.ConstructionArea);
                                data.UsableArea = item1.groupDataByTypeUsageStatus.Sum(p => p.UsableArea);
                                break;
                            }

                            Quantity += (int)data.Quantity;
                            LandArea += data.LandArea;
                            ConstructionArea += data.ConstructionArea;
                            UsableArea += data.UsableArea;
                            identifierDatasNOC.Add(data);
                            identifierData.DistrictId = data.DistrictId;
                        }
                    }
                    identifierData.Index = -1;
                    identifierData.DistrictName = "Tổng";
                    identifierData.Quantity = Quantity;
                    identifierData.LandArea = LandArea;
                    identifierData.ConstructionArea = ConstructionArea;
                    identifierData.UsableArea = UsableArea;
                    identifierDatasNOC.Add(identifierData);
                }

                var total = new IdentifierData();
                total.DistrictName = "Tổng cộng";
                total.DistrictId = -99;
                total.TypeBlockId = -99;
                total.Quantity = identifierDatasNOC.Where(n => n.Index != -1).Sum(p => p.Quantity ?? 0);
                total.UsageStatus = null;
                total.TypeHouse = null;
                total.LandArea = identifierDatasNOC.Where(n => n.Index != -1).Sum(p => p.LandArea ?? 0);
                total.ConstructionArea = identifierDatasNOC.Where(n => n.Index != -1).Sum(p => p.ConstructionArea ?? 0);
                total.UsableArea = identifierDatasNOC.Where(n => n.Index != -1).Sum(p => p.UsableArea ?? 0);
                identifierDatasNOC.Add(total);

                var groupData = identifierDatasNOC.GroupBy(x => x.DistrictId).Select(e => new GroupDataNOC
                {
                    DistrictId = (int)e.Key,
                    groupByDistrict = e.ToList().GroupBy(x => new
                    {
                        x.TypeBlockId,
                    }).Select(e => new GroupByTypeBlockID
                    {
                        TypeBlockID = e.Key.TypeBlockId,
                        groupByTypeBlockID = e.ToList().GroupBy(x => x.UsageStatus).Select(e => new GroupDataByTypeUsageStatus
                        {
                            UsageStatus = e.Key,
                            groupDataByTypeUsageStatus = e.ToList()
                        }).ToList()
                    }).ToList()
                }).ToList();

                List<GroupDataNOC> res = new List<GroupDataNOC>();
                foreach (var item in groupData.ToList())
                {
                    int index = 0;
                    foreach (var child1 in item.groupByDistrict)
                    {
                        foreach (var child2 in child1.groupByTypeBlockID)
                        {
                            index++;
                        }
                    }
                    item.Index = index;
                    res.Add(item);
                }


                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = res;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("ReportNOC Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }


        [HttpPost("ExportReportNOC5/{DateStart}/{DateEnd}")]
        public async Task<IActionResult> ExportReportNOC5(DateTime DateStart, DateTime DateEnd, [FromBody] List<GroupDataNOC> input)
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

            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"ReportNOC/PL5NOC.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);

            MemoryStream ms = WriteDataToExcel(templatePath, 0, input, DateStart, DateEnd);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", "BangChietTinh_");
        }

        private static MemoryStream WriteDataToExcel(string templatePath, int sheetnumber, List<GroupDataNOC> data, DateTime DateStart, DateTime DateEnd)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowStart = 4;

            if (sheet != null)
            {
                int datacol = 9;
                int k = 0;
                try
                {

                    ICellStyle cellStyleDate = workbook.CreateCellStyle();
                    cellStyleDate.DataFormat = workbook.CreateDataFormat().GetFormat("dd/mm/yyyy");

                    IRow rowA = sheet.GetRow(2);
                    ICell cellA = rowA.CreateCell(5);
                    cellA.SetCellValue(DateStart);
                    cellA.CellStyle = cellStyleDate;

                    IRow rowB = sheet.GetRow(2);
                    ICell cellB = rowB.CreateCell(7);
                    cellB.SetCellValue(DateEnd);
                    cellB.CellStyle = cellStyleDate;

                    List<ICellStyle> rowStyle = new List<ICellStyle>();
                    // Lấy Style excel
                    for (int i = 0; i < datacol; i++)
                    {
                        rowStyle.Add(sheet.GetRow(rowStart).GetCell(i).CellStyle);
                    }
                    foreach (var itemdata in data)
                    {
                        int firstRow = rowStart;
                        k++;
                        foreach (var itemdataChild in itemdata.groupByDistrict)
                        {
                            int fist = rowStart;
                            foreach (var itemdataChild1 in itemdataChild.groupByTypeBlockID)
                            {
                                foreach (var item in itemdataChild1.groupDataByTypeUsageStatus)
                                {
                                    XSSFRow row = (XSSFRow)sheet.CreateRow(rowStart);
                                    for (int i = 0; i < datacol; i++)
                                    {
                                        row.CreateCell(i).CellStyle = rowStyle[i];
                                        if (i == 0)
                                        {
                                            if (item.Index == -1)
                                            {
                                                row.GetCell(i).SetCellValue("");

                                            }
                                            else
                                            {
                                                row.GetCell(i).SetCellValue(k);
                                            }
                                        }
                                        else if (i == 1)
                                        {
                                            row.GetCell(i).SetCellValue(item.DistrictName);
                                        }
                                        else if (i == 2)
                                        {
                                            if (item.Index == -1)
                                            {
                                                row.GetCell(i).SetCellValue("Tổng");

                                            }
                                            else
                                            {
                                                row.GetCell(i).SetCellValue(item.TypeBlockName);
                                            }
                                        }
                                        else if (i == 3)
                                        {
                                            if (item.UsageStatus == AppEnums.UsageStatus.DANG_CHO_THUE)
                                            {
                                                row.GetCell(i).SetCellValue("Đang cho thuê");
                                            }
                                            else if (item.UsageStatus == AppEnums.UsageStatus.TRANH_CHAP)
                                            {
                                                row.GetCell(i).SetCellValue("Tranh chấp");
                                            }
                                            else if (item.UsageStatus == AppEnums.UsageStatus.CAC_TRUONG_HOP_KHAC)
                                            {
                                                row.GetCell(i).SetCellValue("Các trường hợp khác");
                                            }
                                            else if (item.UsageStatus == AppEnums.UsageStatus.NHA_TRONG)
                                            {
                                                row.GetCell(i).SetCellValue("Nhà trống");
                                            }
                                        }
                                        else if (i == 4)
                                        {
                                            row.GetCell(i).SetCellValue((double)item.Quantity);

                                        }
                                        else if (i == 5)
                                        {
                                            row.GetCell(i).SetCellValue((double)item.LandArea);
                                        }
                                        else if (i == 6)
                                        {
                                            row.GetCell(i).SetCellValue((double)item.ConstructionArea);
                                        }
                                        else if (i == 7)
                                        {
                                            row.GetCell(i).SetCellValue((double)item.UsableArea);
                                        }
                                        else if (i == 8)
                                        {
                                            row.GetCell(i).SetCellValue("");
                                        }
                                    }
                                    rowStart++;
                                }
                            }
                            int last = rowStart - 1;
                            if (last > fist)
                            {
                                CellRangeAddress mergedRegionTBL = new CellRangeAddress(fist, last, 2, 2);
                                sheet.AddMergedRegion(mergedRegionTBL);
                            }
                        }
                        int lastRow = rowStart - 1;
                        if (lastRow > firstRow)
                        {
                            CellRangeAddress mergedRegionIndex = new CellRangeAddress(firstRow, lastRow, 0, 0);
                            sheet.AddMergedRegion(mergedRegionIndex);

                            CellRangeAddress mergedRegionPay = new CellRangeAddress(firstRow, lastRow, 1, 1);
                            sheet.AddMergedRegion(mergedRegionPay);


                        }
                    }
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

        [HttpGet("ReportNOC4/{DateStart}/{DateEnd}")]
        public async Task<IActionResult> ReportNOC4(DateTime DateStart, DateTime DateEnd)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            //check role
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
                List<IdentifierData> identifierDatas = new List<IdentifierData>();
                List<Block> blDataAll = _context.Blocks.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<District> dData = _context.Districts.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<TypeBlock> tbData = _context.TypeBlocks.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<Block> blocks = blDataAll.Where(l => l.TypeReportApply == AppEnums.TypeReportApply.NHA_RIENG_LE  && l.TypeBlockEntity == AppEnums.TypeBlockEntity.BLOCK_RENT).ToList();
                List<BlockData> resBlock = _mapper.Map<List<BlockData>>(blocks.ToList());
                foreach (BlockData item in resBlock)
                {
                    item.DistrictName = _context.Districts.Where(l => l.Id == item.District && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Name).FirstOrDefault();
                }

                foreach (var item in resBlock)
                {
                    var itemIdentifier = new IdentifierData();

                    if (DateStart < item.DateRecord && item.DateRecord < DateEnd)
                    {
                        itemIdentifier.DistrictId = item.District;
                        itemIdentifier.DistrictName = item.DistrictName;
                        itemIdentifier.TypeBlockId = item.TypeBlockId;
                        itemIdentifier.UsageStatus = item.UsageStatus;
                        itemIdentifier.TypeHouse = item.TypeHouse;
                        itemIdentifier.LandArea = (decimal?)item.CampusArea;
                        itemIdentifier.ConstructionArea = (decimal?)item.ConstructionAreaValue;
                        itemIdentifier.UsableArea = (decimal?)item.UseAreaValue;
                        itemIdentifier.Dispute = item.Dispute;
                        itemIdentifier.Blueprint = item.Blueprint;
                        itemIdentifier.EstablishStateOwnership = item.EstablishStateOwnership;
                        itemIdentifier.TakeOver = item.TakeOver;
                        identifierDatas.Add(itemIdentifier);
                    }
                }

                List<Apartment> apartments = _context.Apartments.Where(l => l.Status != AppEnums.EntityStatus.DELETED && l.TypeApartmentEntity == AppEnums.TypeApartmentEntity.APARTMENT_RENT).ToList();
                List<ApartmentData> resApartments = _mapper.Map<List<ApartmentData>>(apartments.ToList());
                foreach (ApartmentData item in resApartments)
                {
                    item.DistrictId = blDataAll.Where(l => l.Id == item.BlockId).Select(p => p.District).FirstOrDefault();
                    item.DistrictName = dData.Where(l => l.Id == item.DistrictId).Select(p => p.Name).FirstOrDefault();
                    item.TypeBlockId = blDataAll.Where(l => l.Id == item.BlockId).Select(p => p.TypeBlockId).FirstOrDefault();
                }

                foreach (var item in resApartments)
                {
                    var itemIdentifier1 = new IdentifierData();

                    if (DateStart < item.DateRecord && item.DateRecord < DateEnd)
                    {
                        itemIdentifier1.DistrictId = item.DistrictId;
                        itemIdentifier1.DistrictName = item.DistrictName;
                        itemIdentifier1.TypeBlockId = item.TypeBlockId;
                        itemIdentifier1.UsageStatus = item.UsageStatus;
                        itemIdentifier1.TypeHouse = item.TypeHouse;
                        itemIdentifier1.LandArea = (decimal?)item.CampusArea;
                        itemIdentifier1.ConstructionArea = (decimal?)item.ConstructionAreaValue;
                        itemIdentifier1.UsableArea = (decimal?)item.UseAreaValue;
                        itemIdentifier1.Dispute = item.Dispute;
                        itemIdentifier1.Blueprint = item.Blueprint;
                        itemIdentifier1.EstablishStateOwnership = item.EstablishStateOwnership;
                        itemIdentifier1.TakeOver = item.TakeOver;
                        identifierDatas.Add(itemIdentifier1);
                    }
                }

                var groupDataByDistrictId = identifierDatas.GroupBy(x => x.DistrictId).Select(e => new GroupDataNOC4
                {
                    DistrictId = (int)e.Key,
                    groupTypeBlockId = e.ToList().GroupBy(x => new
                    {
                        x.TypeBlockId,
                    }).Select(e => new GroupDataNOCs4
                    {
                        TypeBlockID = e.Key.TypeBlockId,
                        groupDataNOC = e.ToList()
                    }).ToList()
                }).ToList();

                List<IdentifierData> identifierDatasNOC = new List<IdentifierData>();


                foreach (var itemData in groupDataByDistrictId)
                {
                    var identifierData = new IdentifierData();

                    int Quantity = 0;
                    decimal? LandArea = 0;
                    decimal? ConstructionArea = 0;
                    decimal? UsableArea = 0;
                    foreach (var item in itemData.groupTypeBlockId)
                    {
                        var data = new IdentifierData();
                        foreach (var item1 in item.groupDataNOC)
                        {
                            data.DistrictId = item1.DistrictId;
                            data.DistrictName = item1.DistrictName;
                            data.TypeBlockId = item1.TypeBlockId;
                            data.TypeBlockName = tbData.Where(l => l.Id == data.TypeBlockId).Select(p => p.Name).FirstOrDefault();
                            data.UsageStatus = item1.UsageStatus;
                            data.Quantity = item.groupDataNOC.Count;
                            data.LandArea = item.groupDataNOC.Sum(p => p.LandArea);
                            data.ConstructionArea = item.groupDataNOC.Sum(p => p.ConstructionArea);
                            data.UsableArea = item.groupDataNOC.Sum(p => p.UsableArea);
                            break;
                        }
                        Quantity += (int)data.Quantity;
                        LandArea += data.LandArea;
                        ConstructionArea += data.ConstructionArea;
                        UsableArea += data.UsableArea;
                        identifierDatasNOC.Add(data);
                        identifierData.DistrictId = data.DistrictId;
                    }
                    identifierData.Index = -1;
                    identifierData.DistrictName = "Tổng";
                    identifierData.Quantity = Quantity;
                    identifierData.LandArea = LandArea;
                    identifierData.ConstructionArea = ConstructionArea;
                    identifierData.UsableArea = UsableArea;
                    identifierDatasNOC.Add(identifierData);
                }


                var total = new IdentifierData();
                total.DistrictName = "Tổng cộng";
                total.DistrictId = -99;
                total.TypeBlockId = -99;
                total.Quantity = identifierDatasNOC.Where(n => n.Index != -1).Sum(p => p.Quantity ?? 0);
                total.UsageStatus = null;
                total.TypeHouse = null;
                total.LandArea = identifierDatasNOC.Where(n => n.Index != -1).Sum(p => p.LandArea ?? 0);
                total.ConstructionArea = identifierDatasNOC.Where(n => n.Index != -1).Sum(p => p.ConstructionArea ?? 0);
                total.UsableArea = identifierDatasNOC.Where(n => n.Index != -1).Sum(p => p.UsableArea ?? 0);
                identifierDatasNOC.Add(total);

                var groupData = identifierDatasNOC.GroupBy(x => x.DistrictId).Select(e => new GroupDataNOC4
                {
                    DistrictId = (int)e.Key,
                    groupTypeBlockId = e.ToList().GroupBy(x => new
                    {
                        x.TypeBlockId,
                    }).Select(e => new GroupDataNOCs4
                    {
                        TypeBlockID = e.Key.TypeBlockId,
                        groupDataNOC = e.ToList()
                    }).ToList()
                }).ToList();

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = groupData;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("ReportNOC Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }


        [HttpPost("ExportNOC4/{DateStart}/{DateEnd}")]
        public async Task<IActionResult> ExportReportNOC4(DateTime DateStart, DateTime DateEnd, [FromBody] List<GroupDataNOC4> input)
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

            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"ReportNOC/PL4NOC.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);

            MemoryStream ms = WriteDataToExcelNOC4(templatePath, 0, input, DateStart, DateEnd);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", "BangChietTinh_");
        }

        private static MemoryStream WriteDataToExcelNOC4(string templatePath, int sheetnumber, List<GroupDataNOC4> data, DateTime DateStart, DateTime DateEnd)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowStart = 4;

            if (sheet != null)
            {
                int datacol = 8;
                int k = 0;
                try
                {

                    ICellStyle cellStyleDate = workbook.CreateCellStyle();
                    cellStyleDate.DataFormat = workbook.CreateDataFormat().GetFormat("dd/mm/yyyy");

                    IRow rowA = sheet.GetRow(2);
                    ICell cellA = rowA.CreateCell(5);
                    cellA.SetCellValue(DateStart);
                    cellA.CellStyle = cellStyleDate;

                    IRow rowB = sheet.GetRow(2);
                    ICell cellB = rowB.CreateCell(7);
                    cellB.SetCellValue(DateEnd);
                    cellB.CellStyle = cellStyleDate;


                    List<ICellStyle> rowStyle = new List<ICellStyle>();
                    // Lấy Style excel
                    for (int i = 0; i < datacol; i++)
                    {
                        rowStyle.Add(sheet.GetRow(rowStart).GetCell(i).CellStyle);
                    }
                    foreach (var itemdata in data)
                    {
                        int firstRow = rowStart;
                        k++;
                        foreach (var itemdataChild in itemdata.groupTypeBlockId)
                        {
                            int fist = rowStart;
                            foreach (var item in itemdataChild.groupDataNOC)
                            {
                                XSSFRow row = (XSSFRow)sheet.CreateRow(rowStart);
                                for (int i = 0; i < datacol; i++)
                                {
                                    row.CreateCell(i).CellStyle = rowStyle[i];
                                    if (i == 0)
                                    {
                                        if (item.Index == -1)
                                        {
                                            row.GetCell(i).SetCellValue("");

                                        }
                                        else
                                        {
                                            row.GetCell(i).SetCellValue(k);
                                        }
                                    }
                                    else if (i == 1)
                                    {
                                        row.GetCell(i).SetCellValue(item.DistrictName);
                                    }
                                    else if (i == 2)
                                    {
                                        if (item.Index == -1)
                                        {
                                            row.GetCell(i).SetCellValue("Tổng");

                                        }
                                        else
                                        {
                                            row.GetCell(i).SetCellValue(item.TypeBlockName);
                                        }
                                    }
                                    else if (i == 3)
                                    {
                                        row.GetCell(i).SetCellValue((double)item.Quantity);

                                    }
                                    else if (i == 4)
                                    {
                                        row.GetCell(i).SetCellValue((double)item.LandArea);
                                    }
                                    else if (i == 5)
                                    {
                                        row.GetCell(i).SetCellValue((double)item.ConstructionArea);
                                    }
                                    else if (i == 6)
                                    {
                                        row.GetCell(i).SetCellValue((double)item.UsableArea);
                                    }
                                    else if (i == 7)
                                    {
                                        row.GetCell(i).SetCellValue("");
                                    }
                                }
                                rowStart++;
                            }
                            int last = rowStart - 1;
                            if (last > fist)
                            {
                                CellRangeAddress mergedRegionTBL = new CellRangeAddress(fist, last, 2, 2);
                                sheet.AddMergedRegion(mergedRegionTBL);
                            }
                        }
                        int lastRow = rowStart - 1;
                        if (lastRow > firstRow)
                        {
                            CellRangeAddress mergedRegionIndex = new CellRangeAddress(firstRow, lastRow, 0, 0);
                            sheet.AddMergedRegion(mergedRegionIndex);

                            CellRangeAddress mergedRegionPay = new CellRangeAddress(firstRow, lastRow, 1, 1);
                            sheet.AddMergedRegion(mergedRegionPay);
                        }
                    }
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

        [HttpGet("ReportNOC3/{DateStart}/{DateEnd}")]
        public async Task<IActionResult> ReportNOC3(DateTime DateStart, DateTime DateEnd)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            //check role
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
                List<IdentifierData> identifierDatas = new List<IdentifierData>();
                List<Block> blDataAll = _context.Blocks.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<District> dData = _context.Districts.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<TypeBlock> tbData = _context.TypeBlocks.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<Block> blocks = blDataAll.Where(l => l.TypeReportApply == AppEnums.TypeReportApply.NHA_RIENG_LE && l.TypeBlockEntity == AppEnums.TypeBlockEntity.BLOCK_RENT).ToList();
                List<BlockData> resBlock = _mapper.Map<List<BlockData>>(blocks.ToList());
                foreach (BlockData item in resBlock)
                {
                    item.DistrictName = dData.Where(l => l.Id == item.District && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Name).FirstOrDefault();
                }


                foreach (var item in resBlock)
                {
                    var itemIdentifier = new IdentifierData();

                    if (DateStart < item.DateRecord && item.DateRecord < DateEnd)
                    {
                        itemIdentifier.DistrictId = item.District;
                        itemIdentifier.DistrictName = item.DistrictName;
                        itemIdentifier.TypeBlockId = item.TypeBlockId;
                        itemIdentifier.TypeHouse = item.TypeHouse;
                        itemIdentifier.LandArea = (decimal?)item.CampusArea;
                        itemIdentifier.ConstructionArea = (decimal?)item.ConstructionAreaValue;
                        itemIdentifier.UsableArea = (decimal?)item.UseAreaValue;
                        itemIdentifier.Dispute = item.Dispute;
                        itemIdentifier.Blueprint = item.Blueprint;
                        itemIdentifier.EstablishStateOwnership = item.EstablishStateOwnership;
                        itemIdentifier.TakeOver = item.TakeOver;
                        identifierDatas.Add(itemIdentifier);
                    }
                }

                List<Apartment> apartments = _context.Apartments.Where(l => l.Status != AppEnums.EntityStatus.DELETED && l.TypeApartmentEntity == AppEnums.TypeApartmentEntity.APARTMENT_RENT).ToList();
                List<ApartmentData> resApartments = _mapper.Map<List<ApartmentData>>(apartments.ToList());
                foreach (ApartmentData item in resApartments)
                {
                    item.DistrictId = blDataAll.Where(l => l.Id == item.BlockId ).Select(p => p.District).FirstOrDefault();
                    item.DistrictName = dData.Where(l => l.Id == item.DistrictId ).Select(p => p.Name).FirstOrDefault();
                    item.TypeBlockId = blDataAll.Where(l => l.Id == item.BlockId ).Select(p => p.TypeBlockId).FirstOrDefault();
                }

                foreach (var item in resApartments)
                {
                    var itemIdentifier1 = new IdentifierData();

                    if (DateStart < item.DateRecord && item.DateRecord < DateEnd)
                    {
                        itemIdentifier1.DistrictId = item.DistrictId;
                        itemIdentifier1.DistrictName = item.DistrictName;
                        itemIdentifier1.TypeBlockId = item.TypeBlockId;
                        itemIdentifier1.TypeHouse = item.TypeHouse;
                        itemIdentifier1.LandArea = (decimal?)item.CampusArea;
                        itemIdentifier1.ConstructionArea = (decimal?)item.ConstructionAreaValue;
                        itemIdentifier1.UsableArea = (decimal?)item.UseAreaValue;
                        identifierDatas.Add(itemIdentifier1);
                    }
                }

                var groupDataByDistrictId = identifierDatas.GroupBy(x => x.DistrictId).Select(e => new GroupDataNOC3
                {
                    DistrictId = (int)e.Key,
                    groupByTypeBlockID = e.ToList().GroupBy(x => new
                    {
                        x.TypeBlockId,
                    }).Select(e => new GroupByTypeBlockID3
                    {
                        TypeBlockID = e.Key.TypeBlockId,
                        groupDataByTypeHouse = e.ToList().GroupBy(x => x.TypeHouse).Select(e => new GroupDataByTypeTypeHouse
                        {
                            TypeHouse = e.Key,
                            data = e.ToList()
                        }).ToList()
                    }).ToList()
                }).ToList();

                List<IdentifierData> identifierDatasNOC = new List<IdentifierData>();


                foreach (var itemData in groupDataByDistrictId)
                {
                    var identifierData = new IdentifierData();

                    int Quantity = 0;
                    decimal? LandArea = 0;
                    decimal? ConstructionArea = 0;
                    decimal? UsableArea = 0;
                    foreach (var item in itemData.groupByTypeBlockID)
                    {

                        foreach (var item1 in item.groupDataByTypeHouse)
                        {
                            var data = new IdentifierData();
                            foreach (var childitem in item1.data)
                            {
                                data.DistrictId = childitem.DistrictId;
                                data.DistrictName = childitem.DistrictName;
                                data.TypeBlockId = childitem.TypeBlockId;
                                data.TypeBlockName = tbData.Where(l => l.Id == data.TypeBlockId ).Select(p => p.Name).FirstOrDefault();
                                data.TypeHouse = childitem.TypeHouse;
                                data.Quantity = item1.data.Count;
                                data.LandArea = item1.data.Sum(p => p.LandArea);
                                data.ConstructionArea = item1.data.Sum(p => p.ConstructionArea);
                                data.UsableArea = item1.data.Sum(p => p.UsableArea);
                                break;
                            }

                            Quantity += (int)data.Quantity;
                            LandArea += data.LandArea;
                            ConstructionArea += data.ConstructionArea;
                            UsableArea += data.UsableArea;
                            identifierDatasNOC.Add(data);
                            identifierData.DistrictId = data.DistrictId;
                        }
                    }
                    identifierData.Index = -1;
                    identifierData.DistrictName = "Tổng";
                    identifierData.Quantity = Quantity;
                    identifierData.LandArea = LandArea;
                    identifierData.ConstructionArea = ConstructionArea;
                    identifierData.UsableArea = UsableArea;
                    identifierDatasNOC.Add(identifierData);
                }

                var total = new IdentifierData();
                total.DistrictName = "Tổng cộng";
                total.DistrictId = -99;
                total.TypeBlockId = -99;
                total.TypeHouse = null;
                total.Quantity = identifierDatasNOC.Where(n => n.Index != -1).Sum(p => p.Quantity ?? 0);
                total.UsageStatus = null;
                total.LandArea = identifierDatasNOC.Where(n => n.Index != -1).Sum(p => p.LandArea ?? 0);
                total.ConstructionArea = identifierDatasNOC.Where(n => n.Index != -1).Sum(p => p.ConstructionArea ?? 0);
                total.UsableArea = identifierDatasNOC.Where(n => n.Index != -1).Sum(p => p.UsableArea ?? 0);
                identifierDatasNOC.Add(total);

                var groupData = identifierDatasNOC.GroupBy(x => x.DistrictId).Select(e => new GroupDataNOC3
                {
                    DistrictId = (int)e.Key,
                    groupByTypeBlockID = e.ToList().GroupBy(x => new
                    {
                        x.TypeBlockId,

                    }).Select(e => new GroupByTypeBlockID3
                    {
                        TypeBlockID = e.Key.TypeBlockId,

                        groupDataByTypeHouse = e.ToList().GroupBy(x => x.TypeHouse).Select(e => new GroupDataByTypeTypeHouse
                        {
                            TypeHouse = e.Key,
                            LandArea = (decimal)e.ToList().Sum(l => l.LandArea),
                            ConstructionArea = (decimal)e.ToList().Sum(l => l.ConstructionArea),
                            data = e.ToList()
                        }).ToList()
                    }).ToList()
                }).ToList();

                List<GroupDataNOC3> res = new List<GroupDataNOC3>();
                foreach (var item in groupData.ToList())
                {
                    int index = 0;
                    for (int i = 0; i < item.groupByTypeBlockID.Count; i++)
                    {
                        var child1 = item.groupByTypeBlockID[i];

                        index += child1.groupDataByTypeHouse.Count;

                        child1.ConstructionArea = (decimal)child1.groupDataByTypeHouse.Sum(p => p.ConstructionArea);
                        child1.LandArea = (decimal)child1.groupDataByTypeHouse.Sum(p => p.LandArea);

                        item.groupByTypeBlockID[i] = child1;
                    }
                    item.Index = index;
                    res.Add(item);
                }

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = groupData;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("ReportNOC Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }


        [HttpPost("ExportNOC3/{DateStart}/{DateEnd}")]
        public async Task<IActionResult> ExportReportNOC3(DateTime DateStart, DateTime DateEnd, [FromBody] List<GroupDataNOC3> input)
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

            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"ReportNOC/PL3NOC.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);

            MemoryStream ms = WriteDataToExcelNOC3(templatePath, 0, input, DateStart, DateEnd);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", "BangChietTinh_");
        }

        private static MemoryStream WriteDataToExcelNOC3(string templatePath, int sheetnumber, List<GroupDataNOC3> data, DateTime DateStart, DateTime DateEnd)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowStart = 4;

            if (sheet != null)
            {
                int datacol = 9;
                int k = 0;
                try
                {

                    ICellStyle cellStyleDate = workbook.CreateCellStyle();
                    cellStyleDate.DataFormat = workbook.CreateDataFormat().GetFormat("dd/mm/yyyy");


                    IRow rowA = sheet.GetRow(2);
                    ICell cellA = rowA.CreateCell(5);
                    cellA.SetCellValue(DateStart);
                    cellA.CellStyle = cellStyleDate;

                    IRow rowB = sheet.GetRow(2);
                    ICell cellB = rowB.CreateCell(7);
                    cellB.SetCellValue(DateEnd);
                    cellB.CellStyle = cellStyleDate;



                    List<ICellStyle> rowStyle = new List<ICellStyle>();
                    // Lấy Style excel
                    for (int i = 0; i < datacol; i++)
                    {
                        rowStyle.Add(sheet.GetRow(rowStart).GetCell(i).CellStyle);
                    }
                    foreach (var itemdata in data)
                    {
                        int firstRow = rowStart;
                        k++;
                        foreach (var itemdataChild in itemdata.groupByTypeBlockID)
                        {
                            int fist = rowStart;
                            foreach (var itemdataChild1 in itemdataChild.groupDataByTypeHouse)
                            {
                                foreach (var item in itemdataChild1.data)
                                {
                                    XSSFRow row = (XSSFRow)sheet.CreateRow(rowStart);
                                    for (int i = 0; i < datacol; i++)
                                    {
                                        row.CreateCell(i).CellStyle = rowStyle[i];
                                        if (i == 0)
                                        {
                                            if (item.Index == -1)
                                            {
                                                row.GetCell(i).SetCellValue("");

                                            }
                                            else
                                            {
                                                row.GetCell(i).SetCellValue(k);
                                            }
                                        }
                                        else if (i == 1)
                                        {
                                            row.GetCell(i).SetCellValue(item.DistrictName);
                                        }
                                        else if (i == 2)
                                        {
                                            if (item.Index == -1)
                                            {
                                                row.GetCell(i).SetCellValue("Tổng");

                                            }
                                            else
                                            {
                                                row.GetCell(i).SetCellValue(item.TypeBlockName);
                                            }
                                        }
                                        else if (i == 3)
                                        {
                                            if (item.TypeHouse == AppEnums.TypeHouse.Not_Sell)
                                            {
                                                row.GetCell(i).SetCellValue("Không đủ điều kiện bán");
                                            }
                                            else if (item.TypeHouse == AppEnums.TypeHouse.Not_Eligible_Sell)
                                            {
                                                row.GetCell(i).SetCellValue("Chưa đủ điều kiện bán");
                                            }
                                            else if (item.TypeHouse == AppEnums.TypeHouse.Eligible_Sell)
                                            {
                                                row.GetCell(i).SetCellValue("Đủ điều kiện bán");
                                            }
                                        }
                                        else if (i == 4)
                                        {
                                            row.GetCell(i).SetCellValue((double)item.Quantity);
                                        }
                                        else if (i == 5)
                                        {
                                            row.GetCell(i).SetCellValue((double)itemdataChild.LandArea);
                                        }
                                        else if (i == 6)
                                        {
                                            row.GetCell(i).SetCellValue((double)itemdataChild.ConstructionArea);
                                        }
                                        else if (i == 7)
                                        {
                                            row.GetCell(i).SetCellValue((double)item.UsableArea);
                                        }
                                        else if (i == 8)
                                        {
                                            row.GetCell(i).SetCellValue("");
                                        }
                                    }
                                    rowStart++;
                                }
                            }
                            int last = rowStart - 1;
                            if (last > fist)
                            {
                                CellRangeAddress mergedRegionTBL = new CellRangeAddress(fist, last, 2, 2);
                                sheet.AddMergedRegion(mergedRegionTBL);

                                CellRangeAddress mergedRegionTBLConstructionArea = new CellRangeAddress(fist, last, 5, 5);
                                sheet.AddMergedRegion(mergedRegionTBLConstructionArea);

                                CellRangeAddress mergedRegionTBLLandArea = new CellRangeAddress(fist, last, 6, 6);
                                sheet.AddMergedRegion(mergedRegionTBLLandArea);
                            }
                        }
                        int lastRow = rowStart - 1;
                        if (lastRow > firstRow)
                        {
                            CellRangeAddress mergedRegionIndex = new CellRangeAddress(firstRow, lastRow, 0, 0);
                            sheet.AddMergedRegion(mergedRegionIndex);

                            CellRangeAddress mergedRegionPay = new CellRangeAddress(firstRow, lastRow, 1, 1);
                            sheet.AddMergedRegion(mergedRegionPay);


                        }
                    }
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

        [HttpGet("ReportNOC6/{DateStart}/{DateEnd}")]
        public async Task<IActionResult> ReportNOC6(DateTime DateStart, DateTime DateEnd)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            //check role
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
                List<IdentifierData> identifierDatas = new List<IdentifierData>();
                List<Block> blDataAll = _context.Blocks.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<District> dData = _context.Districts.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<TypeBlock> tbData = _context.TypeBlocks.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<Debts> resRentFile = _context.debts.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();

                foreach (var item in resRentFile)
                {
                    var itemIdentifier = new IdentifierData();

                    if (DateStart < item.CreatedAt && item.CreatedAt < DateEnd)
                    {
                        itemIdentifier.DistrictId = item.DistrictId;
                        itemIdentifier.DistrictName = dData.Where(l => l.Id == item.DistrictId).Select(p => p.Name).FirstOrDefault();
                        itemIdentifier.TypeBlockId = item.TypeBlockId;
                        itemIdentifier.TypeBlockName = tbData.Where(l => l.Id == itemIdentifier.TypeBlockId).Select(p => p.Name).FirstOrDefault();
                        itemIdentifier.UsageStatus = item.UsageStatus;
                        itemIdentifier.AmountDue = item.Total;
                        itemIdentifier.AmountReceived = item.Paid;
                        itemIdentifier.AmountOwed = item.Diff;
                        identifierDatas.Add(itemIdentifier);
                    }
                }

                var groupDataByDistrictId = identifierDatas.GroupBy(x => x.DistrictId).Select(e => new GroupDataNOC
                {
                    DistrictId = (int)e.Key,
                    groupByDistrict = e.ToList().GroupBy(x => new
                    {
                        x.TypeBlockId,
                    }).Select(e => new GroupByTypeBlockID
                    {
                        TypeBlockID = e.Key.TypeBlockId,
                        groupByTypeBlockID = e.ToList().GroupBy(x => x.UsageStatus).Select(e => new GroupDataByTypeUsageStatus
                        {
                            UsageStatus = e.Key,
                            groupDataByTypeUsageStatus = e.ToList()
                        }).ToList()
                    }).ToList()
                }).ToList();

                List<IdentifierData> identifierDatasNOC = new List<IdentifierData>();


                foreach (var itemData in groupDataByDistrictId)
                {
                    var identifierData = new IdentifierData();

                    int Quantity = 0;
                    decimal? AmountDue = 0;
                    decimal? AmountReceived = 0;
                    decimal? AmountOwed = 0;
                    decimal? Ratio = 0;
                    foreach (var item in itemData.groupByDistrict)
                    {

                        foreach (var item1 in item.groupByTypeBlockID)
                        {
                            var data = new IdentifierData();
                            foreach (var childitem in item1.groupDataByTypeUsageStatus)
                            {
                                data.DistrictId = childitem.DistrictId;
                                data.DistrictName = childitem.DistrictName;
                                data.TypeBlockId = childitem.TypeBlockId;
                                data.TypeBlockName = childitem.TypeBlockName;
                                data.UsageStatus = childitem.UsageStatus;
                                data.Quantity = item1.groupDataByTypeUsageStatus.Count;
                                data.AmountDue = item1.groupDataByTypeUsageStatus.Sum(p => p.AmountDue);
                                data.AmountReceived = item1.groupDataByTypeUsageStatus.Sum(p => p.AmountReceived);
                                data.AmountOwed = item1.groupDataByTypeUsageStatus.Sum(p => p.AmountOwed);
                                if (data.AmountReceived == 0) data.AmountReceived = 1;
                                data.Ratio = (decimal)((data.AmountDue / data.AmountReceived) / 100);
                                break;
                            }

                            Quantity += (int)data.Quantity;
                            AmountDue += data.AmountDue;
                            AmountReceived += data.AmountReceived;
                            AmountOwed += data.AmountOwed;
                            Ratio += data.Ratio;
                            identifierDatasNOC.Add(data);
                            identifierData.DistrictId = data.DistrictId;
                        }
                    }
                    identifierData.Index = -1;
                    identifierData.DistrictName = "Tổng";
                    identifierData.Quantity = Quantity;
                    identifierData.AmountDue = AmountDue;
                    identifierData.AmountReceived = AmountReceived;
                    identifierData.AmountOwed = AmountOwed;
                    identifierData.Ratio = (long)Ratio;
                    identifierDatasNOC.Add(identifierData);
                }

                var total = new IdentifierData();
                total.DistrictName = "Tổng cộng";
                total.DistrictId = -99;
                total.TypeBlockId = -99;
                total.Quantity = identifierDatasNOC.Where(n => n.Index != -1).Sum(p => p.Quantity ?? 0);
                total.TypeHouse = null;
                total.UsageStatus = null;
                total.AmountDue = identifierDatasNOC.Where(n => n.Index != -1).Sum(p => p.AmountDue ?? 0);
                total.AmountReceived = identifierDatasNOC.Where(n => n.Index != -1).Sum(p => p.AmountReceived ?? 0);
                total.AmountOwed = identifierDatasNOC.Where(n => n.Index != -1).Sum(p => p.AmountOwed ?? 0);
                total.Ratio = identifierDatasNOC.Where(n => n.Index != -1).Sum(p => p.Ratio);
                identifierDatasNOC.Add(total);

                var groupData = identifierDatasNOC.GroupBy(x => x.DistrictId).Select(e => new GroupDataNOC
                {
                    DistrictId = (int)e.Key,
                    groupByDistrict = e.ToList().GroupBy(x => new
                    {
                        x.TypeBlockId,
                    }).Select(e => new GroupByTypeBlockID
                    {
                        TypeBlockID = e.Key.TypeBlockId,
                        groupByTypeBlockID = e.ToList().GroupBy(x => x.UsageStatus).Select(e => new GroupDataByTypeUsageStatus
                        {
                            UsageStatus = e.Key,
                            groupDataByTypeUsageStatus = e.ToList()
                        }).ToList()
                    }).ToList()
                }).ToList();


                List<GroupDataNOC> res = new List<GroupDataNOC>();
                foreach (var item in groupData.ToList())
                {
                    int index = 0;
                    foreach (var child1 in item.groupByDistrict)
                    {
                        foreach (var child2 in child1.groupByTypeBlockID)
                        {
                            index++;
                        }
                    }
                    item.Index = index;
                    res.Add(item);
                }

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = res;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("ReportNOC Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [HttpPost("ExportReportNOC6")]
        public async Task<IActionResult> ExportReportNOC6([FromBody] List<GroupDataNOC> input)
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

            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"ReportNOC/PL6NOC.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);

            MemoryStream ms = WriteDataToExcel6(templatePath, 0, input);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", "BangChietTinh_");
        }

        private static MemoryStream WriteDataToExcel6(string templatePath, int sheetnumber, List<GroupDataNOC> data)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowStart = 4;

            if (sheet != null)
            {
                int datacol = 10;
                int k = 0;
                try
                {
                    List<ICellStyle> rowStyle = new List<ICellStyle>();
                    // Lấy Style excel
                    for (int i = 0; i < datacol; i++)
                    {
                        rowStyle.Add(sheet.GetRow(rowStart).GetCell(i).CellStyle);
                    }
                    foreach (var itemdata in data)
                    {
                        int firstRow = rowStart;
                        k++;
                        foreach (var itemdataChild in itemdata.groupByDistrict)
                        {
                            int fist = rowStart;
                            foreach (var itemdataChild1 in itemdataChild.groupByTypeBlockID)
                            {
                                foreach (var item in itemdataChild1.groupDataByTypeUsageStatus)
                                {
                                    XSSFRow row = (XSSFRow)sheet.CreateRow(rowStart);
                                    for (int i = 0; i < datacol; i++)
                                    {
                                        row.CreateCell(i).CellStyle = rowStyle[i];
                                        if (i == 0)
                                        {
                                            if (item.Index == -1)
                                            {
                                                row.GetCell(i).SetCellValue("");

                                            }
                                            else
                                            {
                                                row.GetCell(i).SetCellValue(k);
                                            }
                                        }
                                        else if (i == 1)
                                        {
                                            row.GetCell(i).SetCellValue(item.DistrictName);
                                        }
                                        else if (i == 2)
                                        {
                                            if (item.Index == -1)
                                            {
                                                row.GetCell(i).SetCellValue("Tổng");

                                            }
                                            else
                                            {
                                                row.GetCell(i).SetCellValue(item.TypeBlockName);
                                            }
                                        }
                                        else if (i == 3)
                                        {
                                            if (item.UsageStatus == AppEnums.UsageStatus.DANG_CHO_THUE)
                                            {
                                                row.GetCell(i).SetCellValue("Đang cho thuê");
                                            }
                                            else if (item.UsageStatus == AppEnums.UsageStatus.TRANH_CHAP)
                                            {
                                                row.GetCell(i).SetCellValue("Tranh chấp");
                                            }
                                            else if (item.UsageStatus == AppEnums.UsageStatus.CAC_TRUONG_HOP_KHAC)
                                            {
                                                row.GetCell(i).SetCellValue("Các trường hợp khác");
                                            }
                                            else if (item.UsageStatus == AppEnums.UsageStatus.NHA_TRONG)
                                            {
                                                row.GetCell(i).SetCellValue("Nhà trống");
                                            }
                                        }
                                        else if (i == 4)
                                        {
                                            row.GetCell(i).SetCellValue((double)item.Quantity);

                                        }
                                        else if (i == 5)
                                        {
                                            row.GetCell(i).SetCellValue((double)item.AmountDue);
                                        }
                                        else if (i == 6)
                                        {
                                            row.GetCell(i).SetCellValue((double)item.AmountReceived);
                                        }
                                        else if (i == 7)
                                        {
                                            row.GetCell(i).SetCellValue((double)item.Ratio + "%");
                                        }
                                        else if (i == 8)
                                        {
                                            row.GetCell(i).SetCellValue((double)item.AmountOwed);
                                        }
                                        else if (i == 9)
                                        {
                                            row.GetCell(i).SetCellValue("");
                                        }
                                    }
                                    rowStart++;
                                }
                            }
                            int last = rowStart - 1;
                            if (last > fist)
                            {
                                CellRangeAddress mergedRegionTBL = new CellRangeAddress(fist, last, 2, 2);
                                sheet.AddMergedRegion(mergedRegionTBL);
                            }
                        }
                        int lastRow = rowStart - 1;
                        if (lastRow > firstRow)
                        {
                            CellRangeAddress mergedRegionIndex = new CellRangeAddress(firstRow, lastRow, 0, 0);
                            sheet.AddMergedRegion(mergedRegionIndex);

                            CellRangeAddress mergedRegionPay = new CellRangeAddress(firstRow, lastRow, 1, 1);
                            sheet.AddMergedRegion(mergedRegionPay);


                        }
                    }
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

        [HttpGet("ReportNOC7/{DateStart}/{DateEnd}")]
        public async Task<IActionResult> ReportNOC7(DateTime DateStart, DateTime DateEnd)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            //check role
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
                List<IdentifierData> identifierDatas = new List<IdentifierData>();

                List<DebtsTable> debtsTables = _context.DebtsTables.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<Block> blDataAll = _context.Blocks.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<District> dData = _context.Districts.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<TypeBlock> tbData = _context.TypeBlocks.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<RentFile> rfData = _context.RentFiles.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                foreach (var item in debtsTables)
                {
                    var itemIdentifier = new IdentifierData();

                    if (DateStart < item.CreatedAt && item.CreatedAt < DateEnd)
                    {
                        itemIdentifier.Code = item.Code;
                        itemIdentifier.DistrictId = rfData.Where(l => l.Id == item.RentFileId).Select(p => p.DistrictId).FirstOrDefault();
                        itemIdentifier.DistrictName = dData.Where(l => l.Id == itemIdentifier.DistrictId ).Select(p => p.Name).FirstOrDefault();
                        itemIdentifier.TypeBlockId = rfData.Where(l => l.Id == item.RentFileId ).Select(p => p.TypeBlockId).FirstOrDefault();
                        itemIdentifier.TypeBlockName = tbData.Where(l => l.Id == itemIdentifier.TypeBlockId ).Select(p => p.Name).FirstOrDefault();
                        itemIdentifier.UsageStatus = rfData.Where(l => l.Id == item.RentFileId).Select(p => p.UsageStatus).FirstOrDefault();
                        itemIdentifier.Price1 = item.AmountExclude;
                        itemIdentifier.check = item.Check;
                        itemIdentifier.CheckPayDepartment = item.CheckPayDepartment;
                        itemIdentifier.VATPrice = (decimal)item.VATPrice;
                        identifierDatas.Add(itemIdentifier);
                    }
                }

                var groupDataByDistrictId = identifierDatas.GroupBy(x => x.DistrictId).Select(e => new NOC7
                {
                    DistrictId = (int)e.Key,
                    noc7ByDistrict = e.ToList().GroupBy(x => x.TypeBlockId).Select(e => new NOC7ByTypeBlockID
                    {
                        TypeBlockID = e.Key,
                        noc7ByTypeBlockID = e.ToList().GroupBy(x => x.UsageStatus).Select(e => new NOC7ByTypeUsageStatus
                        {
                            UsageStatus = e.Key,
                            noc7ByTypeUsageStatus = e.ToList().GroupBy(x => x.Code).Select(e => new NOC7ByCode
                            {
                                Code = e.Key,
                                Noc7byCode = e.ToList()
                            }).ToList()
                        }).ToList()
                    }).ToList()
                }).ToList();

                List<IdentifierData> identifierDatasNOC = new List<IdentifierData>();

                foreach (var itemData in groupDataByDistrictId)
                {
                    var identifierData = new IdentifierData();

                    int Quantity = 0;
                    decimal? Price1 = 0;
                    decimal? Price2 = 0;
                    decimal? VATPrice = 0;
                    byte? Ratio = 0;
                    foreach (var item in itemData.noc7ByDistrict)
                    {
                        foreach (var item1 in item.noc7ByTypeBlockID)
                        {
                            var data = new IdentifierData();
                            foreach (var childitems in item1.noc7ByTypeUsageStatus)
                            {
                                foreach (var childitem in childitems.Noc7byCode)
                                {
                                    data.Code = childitem.Code;
                                    data.DistrictId = childitem.DistrictId;
                                    data.DistrictName = childitem.DistrictName;
                                    data.TypeBlockId = childitem.TypeBlockId;
                                    data.TypeBlockName = childitem.TypeBlockName;
                                    data.UsageStatus = childitem.UsageStatus;
                                    data.Quantity = item1.noc7ByTypeUsageStatus.Count;
                                    data.Price1 = childitems.Noc7byCode.Where(n => n.check == true && n.CheckPayDepartment == true).ToList().Sum(p => p.Price1 ?? 0);
                                    data.Price2 = childitems.Noc7byCode.Where(n => n.check == true && n.CheckPayDepartment == false).ToList().Sum(p => p.Price1 ?? 0);
                                    data.VATPrice = childitems.Noc7byCode.Sum(p => p.VATPrice ?? 0);
                                    break;
                                }
                            }
                            Quantity += (int)data.Quantity;
                            Price1 += data.Price1;
                            Price2 += data.Price2;
                            VATPrice += data.VATPrice;
                            identifierDatasNOC.Add(data);
                            identifierData.DistrictId = data.DistrictId;
                        }
                    }
                    identifierData.Index = -1;
                    identifierData.DistrictName = "Tổng";
                    identifierData.Quantity = Quantity;
                    identifierData.Price1 = Price1;
                    identifierData.Price2 = Price2;
                    identifierData.VATPrice = VATPrice;
                    identifierDatasNOC.Add(identifierData);
                }
                var total = new IdentifierData();
                total.DistrictName = "Tổng cộng";
                total.DistrictId = -99;
                total.TypeBlockId = -99;
                total.Quantity = identifierDatasNOC.Where(n => n.Index != -1).Sum(p => p.Quantity ?? 0);
                total.UsageStatus = null;
                total.TypeHouse = null;
                total.Price1 = identifierDatasNOC.Where(n => n.Index != -1).Sum(p => p.Price1 ?? 0);
                total.Price2 = identifierDatasNOC.Where(n => n.Index != -1).Sum(p => p.Price2 ?? 0);
                total.VATPrice = identifierDatasNOC.Where(n => n.Index != -1).Sum(p => p.VATPrice ?? 0);
                identifierDatasNOC.Add(total);

                var groupData = identifierDatasNOC.GroupBy(x => x.DistrictId).Select(e => new NOC7
                {
                    DistrictId = (int)e.Key,
                    noc7ByDistrict = e.ToList().GroupBy(x => x.TypeBlockId).Select(e => new NOC7ByTypeBlockID
                    {
                        TypeBlockID = e.Key,
                        noc7ByTypeBlockID = e.ToList().GroupBy(x => x.UsageStatus).Select(e => new NOC7ByTypeUsageStatus
                        {
                            UsageStatus = e.Key,
                            noc7ByTypeUsageStatus = e.ToList().GroupBy(x => x.Code).Select(e => new NOC7ByCode
                            {
                                Code = e.Key,
                                Noc7byCode = e.ToList()
                            }).ToList()
                        }).ToList()
                    }).ToList()
                }).ToList();


                List<NOC7> res = new List<NOC7>();
                foreach (var item in groupData.ToList())
                {
                    int index = 0;
                    foreach (var child1 in item.noc7ByDistrict)
                    {
                        foreach (var child2 in child1.noc7ByTypeBlockID)
                        {
                            index++;
                        }
                    }
                    item.Index = index;
                    res.Add(item);
                }

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = res;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("ReportNOC Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [HttpPost("ExportReportNOC7")]
        public async Task<IActionResult> ExportReportNOC7([FromBody] List<NOC7> input)
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

            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"ReportNOC/PL7NOC.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);

            MemoryStream ms = WriteDataToExcel7(templatePath, 0, input);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", "BangChietTinh_");
        }
        private static MemoryStream WriteDataToExcel7(string templatePath, int sheetnumber, List<NOC7> data)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowStart = 4;

            if (sheet != null)
            {
                int datacol = 9;
                int k = 0;
                try
                {
                    List<ICellStyle> rowStyle = new List<ICellStyle>();
                    // Lấy Style excel
                    for (int i = 0; i < datacol; i++)
                    {
                        rowStyle.Add(sheet.GetRow(rowStart).GetCell(i).CellStyle);
                    }
                    foreach (var itemdata in data)
                    {
                        int firstRow = rowStart;
                        k++;
                        foreach (var itemdataChild in itemdata.noc7ByDistrict)
                        {
                            int fist = rowStart;
                            foreach (var itemdataChild1 in itemdataChild.noc7ByTypeBlockID)
                            {
                                foreach (var items in itemdataChild1.noc7ByTypeUsageStatus)
                                {
                                    foreach (var item in items.Noc7byCode)
                                    {
                                        XSSFRow row = (XSSFRow)sheet.CreateRow(rowStart);
                                        for (int i = 0; i < datacol; i++)
                                        {
                                            row.CreateCell(i).CellStyle = rowStyle[i];
                                            if (i == 0)
                                            {
                                                if (item.Index == -1)
                                                {
                                                    row.GetCell(i).SetCellValue("");

                                                }
                                                else
                                                {
                                                    row.GetCell(i).SetCellValue(k);
                                                }
                                            }
                                            else if (i == 1)
                                            {
                                                row.GetCell(i).SetCellValue(item.DistrictName);
                                            }
                                            else if (i == 2)
                                            {
                                                if (item.Index == -1)
                                                {
                                                    row.GetCell(i).SetCellValue("Tổng");

                                                }
                                                else
                                                {
                                                    row.GetCell(i).SetCellValue(item.TypeBlockName);
                                                }
                                            }
                                            else if (i == 3)
                                            {
                                                if (item.UsageStatus == AppEnums.UsageStatus.DANG_CHO_THUE)
                                                {
                                                    row.GetCell(i).SetCellValue("Đang cho thuê");
                                                }
                                                else if (item.UsageStatus == AppEnums.UsageStatus.TRANH_CHAP)
                                                {
                                                    row.GetCell(i).SetCellValue("Tranh chấp");
                                                }
                                                else if (item.UsageStatus == AppEnums.UsageStatus.CAC_TRUONG_HOP_KHAC)
                                                {
                                                    row.GetCell(i).SetCellValue("Các trường hợp khác");
                                                }
                                                else if (item.UsageStatus == AppEnums.UsageStatus.NHA_TRONG)
                                                {
                                                    row.GetCell(i).SetCellValue("Nhà trống");
                                                }
                                            }
                                            else if (i == 4)
                                            {
                                                row.GetCell(i).SetCellValue((double)item.Quantity);

                                            }
                                            else if (i == 5)
                                            {
                                                row.GetCell(i).SetCellValue((double)item.Price1);
                                            }
                                            else if (i == 6)
                                            {
                                                row.GetCell(i).SetCellValue((double)item.Price2);
                                            }
                                            else if (i == 7)
                                            {
                                                row.GetCell(i).SetCellValue((double)item.VATPrice);
                                            }
                                            else if (i == 8)
                                            {
                                                row.GetCell(i).SetCellValue("");
                                            }
                                        }
                                        rowStart++;

                                    }
                                }

                            }
                            int last = rowStart - 1;
                            if (last > fist)
                            {
                                CellRangeAddress mergedRegionTBL = new CellRangeAddress(fist, last, 2, 2);
                                sheet.AddMergedRegion(mergedRegionTBL);
                            }
                        }
                        int lastRow = rowStart - 1;
                        if (lastRow > firstRow)
                        {
                            CellRangeAddress mergedRegionIndex = new CellRangeAddress(firstRow, lastRow, 0, 0);
                            sheet.AddMergedRegion(mergedRegionIndex);

                            CellRangeAddress mergedRegionPay = new CellRangeAddress(firstRow, lastRow, 1, 1);
                            sheet.AddMergedRegion(mergedRegionPay);
                        }
                    }
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

        [HttpGet("ReportNOC2/{typeBlock}")]
        public async Task<IActionResult> ReportNOC2(int typeBlock)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            //check role
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
                List<IdentifierData> identifierDatas = new List<IdentifierData>();
                List<Block> blDataAll = _context.Blocks.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<District> dData = _context.Districts.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<TypeBlock> tbData = _context.TypeBlocks.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<Ward> wData = _context.Wards.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<Province> pData = _context.Provincies.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<Block> blocks = blDataAll.Where(l => l.TypeReportApply == AppEnums.TypeReportApply.NHA_RIENG_LE && l.Status != AppEnums.EntityStatus.DELETED && l.TypeBlockId == typeBlock).ToList();
                List<BlockData> resBlock = _mapper.Map<List<BlockData>>(blocks.ToList());
                foreach (BlockData item in resBlock)
                {
                    item.DistrictName = dData.Where(l => l.Id == item.District ).Select(p => p.Name).FirstOrDefault();

                    item.FullAddress = item.Address;

                    Ward ward = wData.Where(p => p.Id == item.Ward).FirstOrDefault();
                    item.FullAddress = item.FullAddress != null && item.FullAddress != "" ? (ward != null ? String.Join(", ", item.FullAddress, ward.Name) : item.FullAddress) : (ward != null ? ward.Name : item.FullAddress);

                    District district = dData.Where(p => p.Id == item.District).FirstOrDefault();
                    item.FullAddress = item.FullAddress != null && item.FullAddress != "" ? (district != null ? String.Join(", ", item.FullAddress, district.Name) : item.FullAddress) : (district != null ? district.Name : item.FullAddress);

                    Province province = pData.Where(p => p.Id == item.Province).FirstOrDefault();
                    item.FullAddress = item.FullAddress != null && item.FullAddress != "" ? (province != null ? String.Join(", ", item.FullAddress, province.Name) : item.FullAddress) : (province != null ? province.Name : item.FullAddress);

                }

                resBlock.RemoveAll(item =>
                {
                    return resBlock.Count(x => x.Code == item.Code) > 1;
                });

                foreach (var item in resBlock)
                {
                    var itemIdentifier = new IdentifierData();

                    itemIdentifier.DistrictName = item.FullAddress;
                    itemIdentifier.BlockId = item.Id;
                    itemIdentifier.TypeBlockId = item.TypeBlockId;
                    itemIdentifier.TypeBlockName = tbData.Where(l => l.Id == itemIdentifier.TypeBlockId ).Select(p => p.Name).FirstOrDefault();
                    itemIdentifier.TypeHouse = item.TypeHouse;
                    if (item.TypeBlockEntity == AppEnums.TypeBlockEntity.BLOCK_RENT)
                    {
                        itemIdentifier.FloorNumber = item.Floor;

                    }
                    else
                    {
                        itemIdentifier.FloorNumber = item.FloorApplyPriceChange;
                    }
                    itemIdentifier.LandArea = (decimal?)item.CampusArea;
                    itemIdentifier.UsableArea = (decimal?)item.UseAreaValue;
                    itemIdentifier.Dispute = item.Dispute;
                    itemIdentifier.Blueprint = item.Blueprint;
                    itemIdentifier.EstablishStateOwnership = item.EstablishStateOwnership;
                    itemIdentifier.TakeOver = item.TakeOver;
                    identifierDatas.Add(itemIdentifier);
                }

                List<Apartment> apartments = _context.Apartments.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<ApartmentData> resApartments = _mapper.Map<List<ApartmentData>>(apartments.ToList());
                foreach (ApartmentData item in resApartments)
                {
                    item.DistrictId = blDataAll.Where(l => l.Id == item.BlockId ).Select(p => p.District).FirstOrDefault();
                    item.DistrictName = dData.Where(l => l.Id == item.DistrictId).Select(p => p.Name).FirstOrDefault();
                    item.TypeBlockId = blDataAll.Where(l => l.Id == item.BlockId ).Select(p => p.TypeBlockId).FirstOrDefault();
                    var x = item.BlockId;
                    Block block1 = blDataAll.Where(b => b.Id == item.BlockId).FirstOrDefault();

                    if (block1 != null)
                    {
                        BlockData res = _mapper.Map<BlockData>(block1);
                        res.FullAddress = res.Address;

                        Ward ward = wData.Where(p => p.Id == res.Ward).FirstOrDefault();
                        res.FullAddress = res.FullAddress != null && res.FullAddress != "" ? (ward != null ? String.Join(", ", res.FullAddress, ward.Name) : res.FullAddress) : (ward != null ? ward.Name : res.FullAddress);

                        District district = dData.Where(p => p.Id == res.District).FirstOrDefault();
                        res.FullAddress = res.FullAddress != null && res.FullAddress != "" ? (district != null ? String.Join(", ", res.FullAddress, district.Name) : res.FullAddress) : (district != null ? district.Name : res.FullAddress);

                        Province province = pData.Where(p => p.Id == res.Province).FirstOrDefault();
                        res.FullAddress = res.FullAddress != null && res.FullAddress != "" ? (province != null ? String.Join(", ", res.FullAddress, province.Name) : res.FullAddress) : (province != null ? province.Name : res.FullAddress);

                        item.BlockName = res.FullAddress;
                    }
                }

                resApartments.RemoveAll(item =>
                {
                    return resApartments.Count(x => x.Code == item.Code) > 1;
                });

                foreach (var item in resApartments)
                {
                    if (item.TypeBlockId == typeBlock)
                    {
                        var itemIdentifier1 = new IdentifierData();

                        itemIdentifier1.DistrictName = String.Concat(item.Address, " ", item.BlockName);
                        itemIdentifier1.BlockId = item.BlockId;
                        itemIdentifier1.TypeBlockId = item.TypeBlockId;
                        itemIdentifier1.TypeBlockName = tbData.Where(l => l.Id == itemIdentifier1.TypeBlockId ).Select(p => p.Name).FirstOrDefault();
                        itemIdentifier1.FloorNumber = blDataAll.Where(l => l.Id == itemIdentifier1.BlockId).Select(p => p.Floor).FirstOrDefault();
                        itemIdentifier1.LandArea = (decimal?)blDataAll.Where(l => l.Id == item.BlockId).Select(p => p.CampusArea).FirstOrDefault();
                        itemIdentifier1.UsableArea = (decimal?)item.UseAreaValue;
                        itemIdentifier1.Dispute = item.Dispute;
                        itemIdentifier1.Blueprint = item.Blueprint;
                        itemIdentifier1.EstablishStateOwnership = item.EstablishStateOwnership;
                        itemIdentifier1.TakeOver = item.TakeOver;
                        identifierDatas.Add(itemIdentifier1);
                    }
                }

                var groupDataByBlockId = identifierDatas.GroupBy(x => x.BlockId).Select(e => new GroupNOC2
                {
                    BlockId = e.Key,
                    groupDataByBlockId = e.ToList()
                }).ToList();

                List<IdentifierData> identifierDatasNOC = new List<IdentifierData>();
                if (groupDataByBlockId.Count > 0)
                {
                    foreach (var itemData in groupDataByBlockId)
                    {

                        var identifierData = new IdentifierData();

                        decimal? LandArea = 0;
                        decimal? UsableArea = 0;

                        foreach (var item in itemData.groupDataByBlockId)
                        {
                            var data = new IdentifierData();

                            data.BlockId = item.BlockId;
                            data.DistrictName = item.DistrictName;
                            data.TypeBlockName = item.TypeBlockName;
                            data.FloorNumber = item.FloorNumber;
                            data.LandArea = item.LandArea;
                            data.UsableArea = item.UsableArea;
                            data.Dispute = item.Dispute;
                            data.Blueprint = item.Blueprint;
                            data.EstablishStateOwnership = item.EstablishStateOwnership;
                            data.TakeOver = item.TakeOver;
                            LandArea += data.LandArea;
                            UsableArea += data.UsableArea;
                            identifierDatasNOC.Add(data);
                            identifierData.BlockId = item.BlockId;
                        }
                        identifierData.Index = -1;
                        identifierData.DistrictName = "Tổng";
                        identifierData.LandArea = LandArea;
                        identifierData.UsableArea = UsableArea;
                        identifierDatasNOC.Add(identifierData);
                    }

                    var total = new IdentifierData();
                    total.DistrictName = "Tổng cộng";
                    total.LandArea = identifierDatasNOC.Where(n => n.Index != -1).Sum(p => p.LandArea ?? 0);
                    total.UsableArea = identifierDatasNOC.Where(n => n.Index != -1).Sum(p => p.UsableArea ?? 0);
                    identifierDatasNOC.Add(total);
                }

                var groupData = identifierDatasNOC.GroupBy(x => x.BlockId).Select(e => new GroupNOC2
                {
                    BlockId = e.Key,
                    groupDataByBlockId = e.ToList()
                }).ToList();

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = groupData;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("ReportNOC Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [HttpPost("ExportReportNOC2")]
        public async Task<IActionResult> ExportReportNOC2([FromBody] List<GroupNOC2> input)
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

            string template = @"ReportNOC/PL2NOC.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);

            MemoryStream ms = WriteDataToExcelNOC2(templatePath, 0, input);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", "BangChietTinh_");
        }

        private static MemoryStream WriteDataToExcelNOC2(string templatePath, int sheetnumber, List<GroupNOC2> data)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);

            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowStart = 4;

            if (sheet != null)
            {
                int datacol = 10;
                try
                {
                    List<ICellStyle> rowStyle = new List<ICellStyle>();

                    // Lấy Style excel
                    for (int i = 0; i < datacol; i++)
                    {
                        rowStyle.Add(sheet.GetRow(rowStart).GetCell(i).CellStyle);
                    }

                    //Thêm row
                    int k = 0;
                    foreach (var item in data)
                    {
                        int firstRow = rowStart;
                        foreach (var childItem in item.groupDataByBlockId)
                        {
                            XSSFRow row = (XSSFRow)sheet.CreateRow(rowStart);
                            for (int i = 0; i < datacol; i++)
                            {
                                row.CreateCell(i).CellStyle = rowStyle[i];
                                if (i == 0)
                                {
                                    row.GetCell(i).SetCellValue(k);
                                }
                                else if (i == 1)
                                {
                                    row.GetCell(i).SetCellValue(childItem.DistrictName);
                                }
                                else if (i == 2)
                                {
                                    row.GetCell(i).SetCellValue(childItem.TypeBlockName);
                                }
                                else if (i == 3)
                                {
                                    if (childItem.FloorNumber != null)
                                    {
                                        row.GetCell(i).SetCellValue((double)childItem.FloorNumber);
                                    }
                                    else
                                    {
                                        row.GetCell(i).SetCellValue("");
                                    }
                                }
                                else if (i == 4)
                                {
                                    if (childItem.EstablishStateOwnership != null)
                                    {
                                        row.GetCell(i).SetCellValue("x");
                                    }
                                    else
                                    {
                                        row.GetCell(i).SetCellValue("");
                                    }
                                }
                                else if (i == 5)
                                {
                                    if (childItem.Dispute != null)
                                    {
                                        row.GetCell(i).SetCellValue("x");
                                    }
                                    else
                                    {
                                        row.GetCell(i).SetCellValue("");
                                    }
                                }
                                else if (i == 6)
                                {
                                    if (childItem.Blueprint != null)
                                    {
                                        row.GetCell(i).SetCellValue("x");
                                    }
                                    else
                                    {
                                        row.GetCell(i).SetCellValue("");
                                    }
                                }
                                else if (i == 7)
                                {
                                    if (childItem.TakeOver != null)
                                    {
                                        row.GetCell(i).SetCellValue("x");
                                    }
                                    else
                                    {
                                        row.GetCell(i).SetCellValue("");
                                    }
                                }
                                else if (i == 8)
                                {
                                    if (childItem.LandArea.HasValue)
                                    {
                                        row.GetCell(i).SetCellValue((double)childItem.LandArea);
                                    }
                                    else
                                    {
                                        row.GetCell(i).SetCellValue("");
                                    }

                                }
                                else if (i == 9)
                                {
                                    if (childItem.UsableArea.HasValue)
                                    {
                                        row.GetCell(i).SetCellValue((double)childItem.UsableArea);
                                    }
                                    else
                                    {
                                        row.GetCell(i).SetCellValue("");
                                    }
                                }
                                else if (i == 10)
                                {
                                    row.GetCell(i).SetCellValue("");
                                }
                            }
                            rowStart++;
                        }
                        k++;
                        int lastRow = rowStart - 1;
                        if (lastRow > firstRow)
                        {
                            CellRangeAddress mergedRegionPay = new CellRangeAddress(firstRow, lastRow, 0, 0);
                            sheet.AddMergedRegion(mergedRegionPay);

                            if (lastRow - 1 > firstRow)
                            {
                                CellRangeAddress mergedRegionPaid = new CellRangeAddress(firstRow, lastRow - 1, 8, 8);
                                sheet.AddMergedRegion(mergedRegionPaid);
                            }

                        }
                    }
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

        [HttpPost("ExportNOC1")]
        public async Task<IActionResult> ExportNOC1([FromBody] FillterNOC1 req)
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
                List<IdentifierData> identifierDatas = new List<IdentifierData>();

                List<Block> blocks = _context.Blocks.Where(l => l.Status != AppEnums.EntityStatus.DELETED && l.TypeReportApply != AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_LIEN_KE && l.TypeReportApply != AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG).ToList();

                blocks = blocks.Where(l => ((req.DistrictId.HasValue && l.District == req.DistrictId) || (req.DistrictId == null && 1 == 1))
                && ((req.LaneId.HasValue && l.Lane == req.LaneId) || (req.LaneId == null && 1 == 1))
                && ((req.WardId.HasValue && l.Ward == req.WardId) || (req.WardId == null && 1 == 1))
                && ((req.TypeBlock.HasValue && l.TypeBlockId == req.TypeBlock) || (req.TypeBlock == null && 1 == 1))).ToList();

                List<Ward> wData = _context.Wards.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<District> dData = _context.Districts.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<Province> pData = _context.Provincies.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<BlockMaintextureRate> bmData = _context.BlockMaintextureRaties.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<Apartment> apartmentsData = _context.Apartments.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<TypeBlock> tbData = _context.TypeBlocks.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();

                List<BlockData> resBlock = _mapper.Map<List<BlockData>>(blocks.ToList());
                foreach (BlockData item in resBlock)
                {
                    item.DistrictName = dData.Where(l => l.Id == item.District).Select(p => p.Name).FirstOrDefault();

                    item.FullAddress = item.Address;

                    Ward ward = wData.Where(p => p.Id == item.Ward).FirstOrDefault();
                    item.FullAddress = item.FullAddress != null && item.FullAddress != "" ? (ward != null ? String.Join(", ", item.FullAddress, ward.Name) : item.FullAddress) : (ward != null ? ward.Name : item.FullAddress);

                    District district = dData.Where(p => p.Id == item.District).FirstOrDefault();
                    item.FullAddress = item.FullAddress != null && item.FullAddress != "" ? (district != null ? String.Join(", ", item.FullAddress, district.Name) : item.FullAddress) : (district != null ? district.Name : item.FullAddress);

                    Province province = pData.Where(p => p.Id == item.Province).FirstOrDefault();
                    item.FullAddress = item.FullAddress != null && item.FullAddress != "" ? (province != null ? String.Join(", ", item.FullAddress, province.Name) : item.FullAddress) : (province != null ? province.Name : item.FullAddress);

                    item.blockMaintextureRaties = bmData.Where(b => b.TargetId == item.Id && b.Type == AppEnums.TypeBlockMaintextureRate.BLOCK && b.Status != AppEnums.EntityStatus.DELETED).ToList();
                }

                List<Apartment> apartments = apartmentsData.Where(l => l.Status != AppEnums.EntityStatus.DELETED && l.TypeApartmentEntity == AppEnums.TypeApartmentEntity.APARTMENT_NORMAL).ToList();


                resBlock.RemoveAll(item =>
                {
                    var res = resBlock.Where(x => x.Code == item.Code).ToList();
                    return res.Count > 1 && res.Where(l => l.TypeBlockEntity == AppEnums.TypeBlockEntity.BLOCK_NORMAL) != null;
                });


                foreach (var item in resBlock)
                {
                    int sell = 0;
                    int rent = 0;
                    int empty = 0;

                    var itemIdentifier = new IdentifierData();

                    if (item.TypeReportApply == AppEnums.TypeReportApply.NHA_RIENG_LE && item.TypeBlockEntity == AppEnums.TypeBlockEntity.BLOCK_RENT)
                    {
                        rent = 1;
                    }
                    else if (item.TypeReportApply == AppEnums.TypeReportApply.NHA_RIENG_LE && item.TypeBlockEntity == AppEnums.TypeBlockEntity.BLOCK_NORMAL)
                    {
                        sell = 1;
                    }

                    if (item.TypeReportApply != AppEnums.TypeReportApply.NHA_RIENG_LE)
                    {
                        if (item.TypeBlockEntity == AppEnums.TypeBlockEntity.BLOCK_RENT)
                        {
                            List<Apartment> apartment = apartmentsData.Where(l => l.BlockId == item.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                            if (apartment != null)
                            {
                                foreach (var ap in apartment)
                                {
                                    foreach (var apR in apartments)
                                    {
                                        if (ap.Code == apR.Code)
                                        {
                                            sell = sell + 1;
                                            break;
                                        }
                                    }
                                }
                                rent = apartment.Count - sell;
                            }
                        }
                    }

                    itemIdentifier.Rent = rent;
                    itemIdentifier.Sell = sell;
                    itemIdentifier.Empty = empty;

                    itemIdentifier.DistrictName = item.FullAddress;
                    itemIdentifier.TypeBlockId = item.TypeBlockId;
                    itemIdentifier.TypeBlockName = tbData.Where(l => l.Id == itemIdentifier.TypeBlockId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Name).FirstOrDefault();
                    itemIdentifier.Structure = "";

                    foreach (var child in item.blockMaintextureRaties)
                    {
                        string name = "";
                        if (child.TypeMainTexTure == AppEnums.TypeMainTexTure.MONG)
                        {
                            name = "Móng";
                        }
                        else if (child.TypeMainTexTure == AppEnums.TypeMainTexTure.KHUNG_COT)
                        {
                            name = "Khung cột";
                        }
                        else if (child.TypeMainTexTure == AppEnums.TypeMainTexTure.NEN_SAN)
                        {
                            name = "Nền Sàn";
                        }
                        else if (child.TypeMainTexTure == AppEnums.TypeMainTexTure.KHUNG_COT_DO_MAI)
                        {
                            name = "Khung cột đổ mái";
                        }
                        else if (child.TypeMainTexTure == AppEnums.TypeMainTexTure.MAI)
                        {
                            name = "Mái";
                        }
                        else if (child.TypeMainTexTure == AppEnums.TypeMainTexTure.TUONG)
                        {
                            name = "Tường";
                        }
                        itemIdentifier.Structure = String.Concat(itemIdentifier.Structure, name, " ,");
                    }

                    if (item.TypeBlockEntity == AppEnums.TypeBlockEntity.BLOCK_RENT)
                    {
                        itemIdentifier.FloorNumber = item.Floor;
                    }
                    else
                    {
                        itemIdentifier.FloorNumber = item.FloorApplyPriceChange;
                    }
                    itemIdentifier.LandArea = (decimal?)item.CampusArea;
                    itemIdentifier.UsableArea = (decimal?)item.UseAreaValue;
                    identifierDatas.Add(itemIdentifier);
                }

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = identifierDatas;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("ReportNOC Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [HttpPost("ExportReportNOC1")]
        public async Task<IActionResult> ExportReportNOC1([FromBody] List<IdentifierData> input)
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

            string template = @"ReportNOC/PL1NOC.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);

            MemoryStream ms = WriteDataToExcelNOC1(templatePath, 0, input);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", "BangChietTinh_");
        }

        private static MemoryStream WriteDataToExcelNOC1(string templatePath, int sheetnumber, List<IdentifierData> data)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);

            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowStart = 4;

            if (sheet != null)
            {
                int datacol = 11;
                try
                {
                    List<ICellStyle> rowStyle = new List<ICellStyle>();

                    // Lấy Style excel
                    for (int i = 0; i < datacol; i++)
                    {
                        rowStyle.Add(sheet.GetRow(rowStart).GetCell(i).CellStyle);
                    }

                    //Thêm row
                    int k = 1;
                    foreach (var childItem in data)
                    {
                        XSSFRow row = (XSSFRow)sheet.CreateRow(rowStart);
                        for (int i = 0; i < datacol; i++)
                        {
                            row.CreateCell(i).CellStyle = rowStyle[i];
                            if (i == 0)
                            {
                                row.GetCell(i).SetCellValue(k);
                            }
                            else if (i == 1)
                            {
                                row.GetCell(i).SetCellValue(childItem.DistrictName);
                            }
                            else if (i == 2)
                            {
                                row.GetCell(i).SetCellValue(childItem.TypeBlockName);
                            }
                            else if (i == 3)
                            {
                                if (childItem.Structure != null)
                                {
                                    row.GetCell(i).SetCellValue(childItem.Structure);
                                }
                                else
                                {
                                    row.GetCell(i).SetCellValue("");
                                }
                            }
                            else if (i == 4)
                            {
                                if (childItem.FloorNumber != null)
                                {
                                    row.GetCell(i).SetCellValue(childItem.FloorNumber + "Tầng");
                                }
                                else
                                {
                                    row.GetCell(i).SetCellValue("");
                                }
                            }
                            else if (i == 5)
                            {
                                if (childItem.Rent != null)
                                {
                                    row.GetCell(i).SetCellValue((double)childItem.Rent);
                                }
                                else
                                {
                                    row.GetCell(i).SetCellValue("");
                                }
                            }
                            else if (i == 6)
                            {
                                if (childItem.Sell != null)
                                {
                                    row.GetCell(i).SetCellValue((double)childItem.Sell);
                                }
                                else
                                {
                                    row.GetCell(i).SetCellValue("");
                                }
                            }
                            else if (i == 7)
                            {
                                if (childItem.Empty != null)
                                {
                                    row.GetCell(i).SetCellValue((double)childItem.Empty);
                                }
                                else
                                {
                                    row.GetCell(i).SetCellValue("");
                                }
                            }
                            else if (i == 8)
                            {
                                if (childItem.LandArea.HasValue)
                                {
                                    row.GetCell(i).SetCellValue((double)childItem.LandArea);
                                }
                                else
                                {
                                    row.GetCell(i).SetCellValue("");
                                }

                            }
                            else if (i == 9)
                            {
                                if (childItem.UsableArea.HasValue)
                                {
                                    row.GetCell(i).SetCellValue((double)childItem.UsableArea);
                                }
                                else
                                {
                                    row.GetCell(i).SetCellValue("");
                                }
                            }
                            else if (i == 10)
                            {
                                row.GetCell(i).SetCellValue("");
                            }
                        }
                        rowStart++;
                        k++;
                    }
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
    }
}
