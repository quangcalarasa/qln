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
using Microsoft.AspNetCore.Hosting;
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
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System.IO;
using static IOITQln.Entities.Md167House;
using System.Globalization;
using static IOITQln.Common.Enums.AppEnums;
using IOITQln.Controllers.ApiMd167;
using DevExpress.Utils.DirectXPaint;
using Microsoft.Net.Http.Headers;
using static IOITQln.Controllers.ApiInv.HouseController;
using DevExpress.ClipboardSource.SpreadsheetML;

namespace IOITQln.Controllers.ApiInv
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportMd167Controller : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("Md167Report", "Md167Report");
        private static string functionCode_Rp08 = "REPORT08";
        private static string functionCode_Rp07 = "REPORT07";
        private static string functionCode_RpDebt = "INFO_DEBT";
        private static string funcyionCode_RpPayment = "MD167_REPORT_PAYMENT";

        private readonly ApiDbContext _context;
        private IHostingEnvironment _hostingEnvironment;
        private IMapper _mapper;

        public ReportMd167Controller(ApiDbContext context, IMapper mapper, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost("GetReport08")]
        public IActionResult GetReport08([FromBody] GetReport08Req req)
        {
            string accessToken = base.Request.Headers[HeaderNames.Authorization];
            Token token = ((IQueryable<Token>)_context.Tokens).Where((Token t) => Convert.ToString(t.AccessToken) == accessToken).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            DefaultResponse defaultResponse = new DefaultResponse();
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)base.User.Identity;
            int num = int.Parse((from c in claimsIdentity.Claims
                                 where c.Type == "Id"
                                 select c.Value).SingleOrDefault());
            string access_key = (from c in claimsIdentity.Claims
                                 where c.Type == "AccessKey"
                                 select c.Value).SingleOrDefault();

            if (!CheckRole.CheckRoleByCode(access_key, functionCode_Rp08, 0))
            {
                defaultResponse.meta = new Meta(222, "Bạn không có quyền xem dữ liệu tới mục này!");
                return Ok(defaultResponse);
            }

            IQueryable<Md167PlantContent> md167PlantContents = ((IQueryable<Md167PlantContent>)_context.Md167PlantContents).Where((Md167PlantContent m) => (int)m.Status != 99);
            IQueryable<Md167TranferUnit> md167TranferUnits = ((IQueryable<Md167TranferUnit>)_context.Md167TranferUnits).Where((Md167TranferUnit m) => (int)m.Status != 99);
            IQueryable<District> source = ((IQueryable<District>)_context.Districts).Where((District m) => (int)m.Status != 99);
            IQueryable<Lane> source2 = ((IQueryable<Lane>)_context.Lanies).Where((Lane m) => (int)m.Status != 99);
            IQueryable<Md167Delegate> md167Delegates = ((IQueryable<Md167Delegate>)_context.Md167Delegates).Where((Md167Delegate m) => (int)m.Status != 99);
            IQueryable<Ward> source3 = ((IQueryable<Ward>)_context.Wards).Where((Ward m) => (int)m.Status != 99);
            IQueryable<Md167PricePerMonth> md167PricePerMonths = from m in (IQueryable<Md167PricePerMonth>)_context.Md167PricePerMonths
                                                                 where (int)m.Status != 99
                                                                 select m into x
                                                                 orderby x.DateEffect descending
                                                                 select x;
            IQueryable<Md167StateOfUse> md167StateOfUses = ((IQueryable<Md167StateOfUse>)_context.Md167StateOfUses).Where((Md167StateOfUse m) => (int)m.Status != 99);

            List<GetReport08Res> list = (from h in (IQueryable<Md167House>)_context.Md167Houses
                                         join c in (IEnumerable<Md167Contract>)_context.Md167Contracts on h.Id equals c.HouseId into contractGroup
                                         from c in contractGroup.DefaultIfEmpty()
                                         where (int)h.Status != 99 && h.HouseNumber != (string?)null && (c == null || (int)c.Status != 99)
                                         // ✅ FIXED LINES: Replaced complex casting with simple null checks.
                                         && (req.FromDate == null || h.CreatedAt >= req.FromDate)
                                         && (req.ToDate == null || h.CreatedAt <= req.ToDate)
                                         // END OF FIX
                                         && ((req.CustomertID != null && (int?)c.DelegateId == req.CustomertID) || (req.CustomertID == null && true))
                                         && ((!string.IsNullOrEmpty(req.HouseCode) && h.Code.Equals(req.HouseCode)) || (req.HouseCode == null && true))
                                         && ((req.Md167TransferUnitId != null && (int?)h.Md167TransferUnitId == req.Md167TransferUnitId) || (req.Md167TransferUnitId == null && true))
                                         && ((req.LaneId != null && (int?)h.LaneId == req.LaneId) || (req.LaneId == null && true))
                                         && ((req.WardId != null && (int?)h.WardId == req.WardId) || (req.WardId == null && true))
                                         && ((req.DistrictId != null && (int?)h.DistrictId == req.DistrictId) || (req.DistrictId == null && true))
                                         && ((req.HouseId != null && (int?)h.Id == req.HouseId) || (req.HouseId == null && true))
                                         && ((req.StatusOfUse != null && (int?)h.StatusOfUse == req.StatusOfUse) || (req.StatusOfUse == null && true))
                                         select new GetReport08Res
                                         {
                                             District = h.DistrictId,
                                             Ward = h.WardId,
                                             Lane = h.LaneId,
                                             TransferUnit = md167TranferUnits.Where(x => x.Id == h.Md167TransferUnitId).Select(x => x.Name).FirstOrDefault(),
                                             AreaHouse = h.InfoValue.UseFloorPb + h.InfoValue.UseFloorPr,
                                             AreaLand = h.InfoValue.UseLandPb + h.InfoValue.UseLandPr,
                                             FloorCount = h.InfoValue.ApaFloorCount,
                                             ContractCode = h.ContractCode,
                                             ContractDate = h.ContractDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                             CustomertName = md167Delegates.Where(x => x.Id == c.DelegateId).Select(x => x.Name).FirstOrDefault(),
                                             DocumentCode = h.DocumentCode,
                                             DocumentDate = h.DocumentDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                             HouseCode = h.HouseNumber,
                                             LeaseCertCode = h.LeaseCertCode,
                                             LeaseCertDate = h.LeaseCertDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                             LeaseCode = h.LeaseCode,
                                             LeaseDate = h.LeaseDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                             LeaseTerm = getRentalPeriod(c.RentalPeriod),
                                             Md167ContractCode = c.Code,
                                             Md167ContractDate = c.DateSign.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                                             Note = h.Note,
                                             PlanContent = md167PlantContents.Where(m => m.Id == h.PlanContent).Select(x => x.Name).FirstOrDefault(),
                                             PriceOM = md167PricePerMonths.Where(m => m.Md167ContractId == c.Id).FirstOrDefault() != null ? md167PricePerMonths.Where(m => m.Md167ContractId == c.Id).FirstOrDefault().TotalPrice : null,
                                             StatusOfUseName = md167StateOfUses.Where(m => m.Id == h.StatusOfUse).Select(x => x.Name).FirstOrDefault(),
                                             TextureScale = h.TextureScale
                                         }).ToList();

            foreach (GetReport08Res item in list)
            {
                HouseController.NameAndOldName nameAndOldName = (from x in source2
                                                                 where x.Id == item.Lane
                                                                 select new HouseController.NameAndOldName
                                                                 {
                                                                     Name = x.Name,
                                                                     OldName = x.InfoValue
                                                                 }).FirstOrDefault();
                if (nameAndOldName == null)
                {
                    item.LaneName = "";
                }
                else if (nameAndOldName.OldName.Count() > 0)
                {
                    item.LaneName = nameAndOldName.Name + "( Tên cũ: " + nameAndOldName.OldName![0].Name + " )";
                }
                else
                {
                    item.LaneName = nameAndOldName.Name;
                }
                HouseController.NameAndOldName nameAndOldName2 = (from x in source
                                                                  where x.Id == item.District
                                                                  select new HouseController.NameAndOldName
                                                                  {
                                                                      Name = x.Name,
                                                                      OldName = x.InfoValue
                                                                  }).FirstOrDefault();
                if (nameAndOldName2 == null)
                {
                    item.DistrictName = "";
                }
                else if (nameAndOldName2.OldName.Count() > 0)
                {
                    item.DistrictName = nameAndOldName2.Name + "( Tên cũ: " + nameAndOldName2.OldName![0].Name + " )";
                }
                else
                {
                    item.DistrictName = nameAndOldName2.Name;
                }
                HouseController.NameAndOldName nameAndOldName3 = (from x in source3
                                                                  where x.Id == item.Ward
                                                                  select new HouseController.NameAndOldName
                                                                  {
                                                                      Name = x.Name,
                                                                      OldName = x.InfoValue
                                                                  }).FirstOrDefault();
                if (nameAndOldName3 == null)
                {
                    item.WardName = "";
                }
                else if (nameAndOldName3.OldName.Count() > 0)
                {
                    item.WardName = nameAndOldName3.Name + "( Tên cũ: " + nameAndOldName3.OldName![0].Name + " )";
                }
                else
                {
                    item.WardName = nameAndOldName3.Name;
                }
            }

            defaultResponse.meta = new Meta(200, "Thành công!");
            defaultResponse.data = list;
            return Ok(defaultResponse);
        }

        //Thời hạn thuê
        private static string getRentalPeriod(RentalPeriodContract167? rentalPeriod)
        {
            string name = "";
            switch (rentalPeriod)
            {
                case RentalPeriodContract167.TAM_BO_TRI:
                    name = "Tạm bố trí";
                    break;
                case RentalPeriodContract167.THUE_1_NAM:
                    name = "Thuê 1 năm";
                    break;
                case RentalPeriodContract167.THUE_5_NAM:
                    name = "Thuê 5 năm";
                    break;
                case RentalPeriodContract167.KHAC:
                    name = "Khác";
                    break;
                default:
                    break;
            }

            return name;
        }

        [HttpPost("ExportReport08Md167")]
        public async Task<IActionResult> ExportReport08Md167([FromBody] List<GetReport08Res> input)
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

            if (!CheckRole.CheckRoleByCode(access_key, functionCode_Rp08, (int)AppEnums.Action.EXPORT))
            {
                return new FileContentResult(new byte[0], "application/octet-stream");
            }

            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"MD167/Report08.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);

            MemoryStream ms = WriteDataToExcelRP08(templatePath, 0, input);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", "Báo cáo 08");
        }

        [HttpPost("ExportReport07Md167")]
        public async Task<IActionResult> ExportReport07Md167([FromBody] List<GetReport07Res> input)
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

            if (!CheckRole.CheckRoleByCode(access_key, functionCode_Rp07, (int)AppEnums.Action.EXPORT))
            {
                return new FileContentResult(new byte[0], "application/octet-stream");
            }

            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"MD167/Report07.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);

            MemoryStream ms = WriteDataToExcelRP07(templatePath, 0, input);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", "Báo cáo 07");
        }
        private static MemoryStream WriteDataToExcelRP08(string templatePath, int sheetnumber, List<GetReport08Res> data)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowStart = 5;

            if (sheet != null)
            {
                int datacol = 26;
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
                        XSSFRow row = (XSSFRow)sheet.CreateRow(rowStart);
                        int firstRow = rowStart;
                        k++;
                        for (int i = 0; i < datacol; i++)
                        {
                            row.CreateCell(i).CellStyle = rowStyle[i];
                            if (i == 0)
                            {
                                row.GetCell(i).SetCellValue(k);
                            }
                            else if (i == 1)
                            {
                                row.GetCell(i).SetCellValue(itemdata.TransferUnit);
                            }
                            else if (i == 2)
                            {
                                row.GetCell(i).SetCellValue(itemdata.CustomertName);
                            }
                            else if (i == 3)
                            {
                                row.GetCell(i).SetCellValue(itemdata.HouseCode);
                            }
                            else if (i == 4)
                            {
                                row.GetCell(i).SetCellValue(itemdata.LaneName);
                            }
                            else if (i == 5)
                            {
                                row.GetCell(i).SetCellValue(itemdata.WardName);
                            }
                            else if (i == 6)
                            {
                                row.GetCell(i).SetCellValue(itemdata.DistrictName);
                            }
                            else if (i == 7)
                            {
                                row.GetCell(i).SetCellValue((double)itemdata.AreaLand);
                            }
                            else if (i == 8)
                            {
                                row.GetCell(i).SetCellValue((double)itemdata.AreaHouse);
                            }
                            else if (i == 9)
                            {
                                row.GetCell(i).SetCellValue(itemdata.FloorCount.Value);
                            }
                            else if (i == 10)
                            {
                                row.GetCell(i).SetCellValue(itemdata.TextureScale);
                            }
                            else if (i == 11)
                            {
                                row.GetCell(i).SetCellValue(itemdata.Md167ContractCode);
                            }
                            else if (i == 12)
                            {
                                row.GetCell(i).SetCellValue(itemdata.Md167ContractDate);
                            }
                            else if (i == 13)
                            {
                                row.GetCell(i).SetCellValue((double)itemdata.PriceOM);
                            }
                            else if (i == 14)
                            {
                                row.GetCell(i).SetCellValue(itemdata.LeaseTerm);
                            }
                            else if (i == 15)
                            {
                                row.GetCell(i).SetCellValue(itemdata.LeaseCode);
                            }
                            else if (i == 16)
                            {
                                row.GetCell(i).SetCellValue(itemdata.LeaseDate);
                            }
                            else if (i == 17)
                            {
                                row.GetCell(i).SetCellValue(itemdata.ContractCode);
                            }
                            else if (i == 18)
                            {
                                row.GetCell(i).SetCellValue(itemdata.ContractDate);
                            }
                            else if (i == 19)
                            {
                                row.GetCell(i).SetCellValue(itemdata.LeaseCertCode);
                            }
                            else if (i == 20)
                            {
                                row.GetCell(i).SetCellValue(itemdata.LeaseCertDate);
                            }
                            else if (i == 21)
                            {
                                row.GetCell(i).SetCellValue(itemdata.DocumentCode);
                            }
                            else if (i == 22)
                            {
                                row.GetCell(i).SetCellValue(itemdata.DocumentDate);
                            }
                            else if (i == 23)
                            {
                                row.GetCell(i).SetCellValue(itemdata.PlanContent);
                            }
                            else if (i == 24)
                            {
                                row.GetCell(i).SetCellValue(itemdata.StatusOfUseName);
                            }
                            else if (i == 25)
                            {
                                row.GetCell(i).SetCellValue(itemdata.Note);
                            }
                        }
                        rowStart++;
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
        private static MemoryStream WriteDataToExcelRP07(string templatePath, int sheetnumber, List<GetReport07Res> data)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowStart = 4;

            if (sheet != null)
            {
                int datacol = 17;
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
                        XSSFRow row = (XSSFRow)sheet.CreateRow(rowStart);
                        int firstRow = rowStart;
                        k++;
                        for (int i = 0; i < datacol; i++)
                        {
                            row.CreateCell(i).CellStyle = rowStyle[i];
                            if (i == 0)
                            {
                                row.GetCell(i).SetCellValue(k);
                            }
                            else if (i == 1)
                            {
                                row.GetCell(i).SetCellValue(itemdata.CustomerName);
                            }
                            else if (i == 2)
                            {
                                row.GetCell(i).SetCellValue(itemdata.HouseNumber);
                            }
                            else if (i == 3)
                            {
                                row.GetCell(i).SetCellValue(itemdata.LaneName);
                            }
                            else if (i == 4)
                            {
                                row.GetCell(i).SetCellValue(itemdata.WardName);
                            }
                            else if (i == 5)
                            {
                                row.GetCell(i).SetCellValue(itemdata.DistrictName);
                            }
                            else if (i == 6)
                            {
                                row.GetCell(i).SetCellValue(itemdata.Code);
                            }
                            else if (i == 7)
                            {
                                row.GetCell(i).SetCellValue(itemdata.DateSign);
                            }
                            else if (i == 8)
                            {
                                row.GetCell(i).SetCellValue(itemdata.DateGroundHandover);
                            }
                            else if (i == 9)
                            {
                                row.GetCell(i).SetCellValue(itemdata.DatePayment);
                            }
                            else if (i == 10)
                            {
                                row.GetCell(i).SetCellValue(itemdata.BillCode);
                            }
                            else if (i == 11)
                            {
                                row.GetCell(i).SetCellValue(itemdata.BillDate);
                            }
                            else if (i == 12)
                            {
                                row.GetCell(i).SetCellValue((double)itemdata.RentCostContract);
                            }
                            else if (i == 13)
                            {
                                row.GetCell(i).SetCellValue((double)itemdata.Pay);
                            }
                            else if (i == 14)
                            {
                                row.GetCell(i).SetCellValue((double)itemdata.Paid);
                            }
                            else if (i == 15)
                            {
                                row.GetCell(i).SetCellValue((double)itemdata.PriceDiff);
                            }
                            else if (i == 16)
                            {
                                row.GetCell(i).SetCellValue(itemdata.Note);
                            }
                        }
                        rowStart++;
                    }

                    //Tạo dòng tổng
                    XSSFRow totalRow = (XSSFRow)sheet.CreateRow(rowStart);
                    for (int i = 0; i < datacol; i++)
                    {
                        totalRow.CreateCell(i).CellStyle = rowStyle[i];
                        if (i == 0)
                        {
                            CellRangeAddress cellRangeAddress = new CellRangeAddress(rowStart, rowStart, 0, 1);
                            sheet.AddMergedRegion(cellRangeAddress);
                            totalRow.GetCell(i).SetCellValue("TỔNG");
                        }
                        else if (i == 1)
                        {
                        }
                        else if (i == 2)
                        {
                        }
                        else if (i == 3)
                        {
                        }
                        else if (i == 4)
                        {
                        }
                        else if (i == 5)
                        {
                        }
                        else if (i == 6)
                        {
                        }
                        else if (i == 7)
                        {
                        }
                        else if (i == 8)
                        {
                        }
                        else if (i == 9)
                        {
                        }
                        else if (i == 10)
                        {
                        }
                        else if (i == 11)
                        {
                        }
                        else if (i == 12)
                        {
                            totalRow.GetCell(i).SetCellValue((double)data.Sum(x => (x.RentCostContract ?? 0)));
                        }
                        else if (i == 13)
                        {
                            totalRow.GetCell(i).SetCellValue((double)data.Sum(x => (x.Pay ?? 0)));
                        }
                        else if (i == 14)
                        {
                            totalRow.GetCell(i).SetCellValue((double)data.Sum(x => (x.Paid ?? 0)));
                        }
                        else if (i == 15)
                        {
                            totalRow.GetCell(i).SetCellValue((double)data.Sum(x => (x.PriceDiff ?? 0)));
                        }
                        else if (i == 16)
                        {
                            totalRow.GetCell(i).SetCellValue("");
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

        [HttpPost("GetReport07")]
        public IActionResult GetReport07([FromBody] GetReport07Req req)
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

            if (!CheckRole.CheckRoleByCode(access_key, functionCode_Rp07, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }
            IQueryable<Md167PlantContent> md167PlantContents = _context.Md167PlantContents.Where(m => m.Status != AppEnums.EntityStatus.DELETED);
            IQueryable<Md167TranferUnit> md167TranferUnits = _context.Md167TranferUnits.Where(m => m.Status != AppEnums.EntityStatus.DELETED);
            IQueryable<District> districts = _context.Districts.Where(m => m.Status != AppEnums.EntityStatus.DELETED);
            IQueryable<Lane> lanies = _context.Lanies.Where(m => m.Status != AppEnums.EntityStatus.DELETED);
            IQueryable<Md167Delegate> md167Delegates = _context.Md167Delegates.Where(m => m.Status != AppEnums.EntityStatus.DELETED);
            IQueryable<Ward> wards = _context.Wards.Where(m => m.Status != AppEnums.EntityStatus.DELETED);
            IQueryable<Md167PricePerMonth> md167PricePerMonths = _context.Md167PricePerMonths.Where(m => m.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(x => x.DateEffect);
            IQueryable<Md167StateOfUse> md167StateOfUses = _context.Md167StateOfUses.Where(m => m.Status != AppEnums.EntityStatus.DELETED);
            var data = (from c in _context.Md167Contracts
                        join h in _context.Md167Houses on c.HouseId equals h.Id
                        where c.Status != AppEnums.EntityStatus.DELETED
                        && h.Status != AppEnums.EntityStatus.DELETED
                        && ((req.FromDate != null && c.DateSign >= req.FromDate) || (req.FromDate == null && 1 == 1))
                        && ((req.ToDate != null && c.DateSign <= req.ToDate) || (req.ToDate == null && 1 == 1))
                        && ((req.CustomertID != null && c.DelegateId == req.CustomertID) || (req.CustomertID == null && 1 == 1))
                        && ((!string.IsNullOrEmpty(req.HouseCode) && h.Code.Equals(req.HouseCode)) || (req.HouseCode == null && 1 == 1))
                        && ((req.LaneId != null && h.LaneId == req.LaneId) || (req.LaneId == null && 1 == 1))
                        && ((req.WardId != null && h.WardId == req.WardId) || (req.WardId == null && 1 == 1))
                        && ((req.DistrictId != null && h.DistrictId == req.DistrictId) || (req.DistrictId == null && 1 == 1))
                        && ((req.HouseId != null && h.Id == req.HouseId) || (req.HouseId == null && 1 == 1))

                        select new GetReport07Res
                        {
                            Md167ContractId = c.Id,
                            District = h.DistrictId,
                            Ward = h.WardId,
                            Lane = h.LaneId,
                            CustomerName = md167Delegates.Where(x => x.Id == c.DelegateId).Select(x => x.Name).FirstOrDefault(),
                            HouseNumber = h.HouseNumber,
                            DistrictName = _context.Districts.Where(x => x.Id == h.DistrictId).Select(x => x.Name).FirstOrDefault(),
                            WardName = _context.Wards.Where(x => x.Id == h.WardId).Select(x => x.Name).FirstOrDefault(),
                            LaneName = _context.Lanies.Where(x => x.Id == h.LaneId).Select(x => x.Name).FirstOrDefault(),
                            Code = c.Code,
                            DateSign = c.DateSign.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                            DateGroundHandover = c.DateGroundHandover.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                            DatePayment = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                            BillCode = "",
                            BillDate = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                            RentCostContract = 0,
                            Pay = 0,
                            Paid = 0,
                            PriceDiff = 0,
                            Note = h.Note,
                            md167Contract = c
                        }).ToList();
            foreach (var item in data)
            {
                var laneName = lanies.Where(x => x.Id == item.Lane).Select(x => new NameAndOldName
                {
                    Name = x.Name,
                    OldName = x.InfoValue
                }).FirstOrDefault();
                if (laneName == null)
                {
                    item.LaneName = "";
                }
                else
                {
                    if (laneName.OldName.Count() > 0)
                    {
                        item.LaneName = laneName.Name + "( Tên cũ: " + laneName.OldName[0].Name + " )";
                    }
                    else
                    {
                        item.LaneName = laneName.Name;
                    }

                }
                var districtName = districts.Where(x => x.Id == item.District).Select(x => new NameAndOldName
                {
                    Name = x.Name,
                    OldName = x.InfoValue
                }).FirstOrDefault();
                if (districtName == null)
                {
                    item.DistrictName = "";
                }
                else
                {
                    if (districtName.OldName.Count() > 0)
                    {
                        item.DistrictName = districtName.Name + "( Tên cũ: " + districtName.OldName[0].Name + " )";
                    }
                    else
                    {
                        item.DistrictName = districtName.Name;
                    }

                }
                var wardName = wards.Where(x => x.Id == item.Ward).Select(x => new NameAndOldName
                {
                    Name = x.Name,
                    OldName = x.InfoValue
                }).FirstOrDefault();
                if (wardName == null)
                {
                    item.WardName = "";
                }
                else
                {
                    if (wardName.OldName.Count() > 0)
                    {
                        item.WardName = wardName.Name + "( Tên cũ: " + wardName.OldName[0].Name + " )";
                    }
                    else
                    {
                        item.WardName = wardName.Name;
                    }

                }
            }
            List<Md167ProfitValue> profitValues = _context.Md167ProfitValues.Where(p => p.Status != EntityStatus.DELETED).ToList();

            List<GetReport07Res> res = new List<GetReport07Res>();
            foreach (var dataItem in data)
            {
                //Lấy dữ liệu thanh toán
                var pricePerMonths = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == dataItem.Md167ContractId && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                List<Md167Receipt> md167Receipts = _context.Md167Receipts.Where(m => m.Md167ContractId == dataItem.Md167ContractId && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();
                List<Md167Receipt> depositMd167Receipts = _context.Md167Receipts.Where(m => m.Md167ContractId == dataItem.Md167ContractId && m.PaidDeposit == true && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();

                List<GroupMd167DebtData> data_payment = null;

                Md167Contract dataRelated = _context.Md167Contracts.Where(x => x.DelegateId == dataItem.md167Contract.DelegateId && x.HouseId == dataItem.md167Contract.HouseId && x.RefundPaidDeposit != true && x.Status != EntityStatus.DELETED).FirstOrDefault();
                if (dataRelated != null)
                {
                    var pricePerMonth = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == dataRelated.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).FirstOrDefault();
                    data_payment = Md167ContractController.GetDataDebtFunc(pricePerMonths, profitValues, md167Receipts, dataItem.md167Contract, pricePerMonth?.TotalPrice * 3, depositMd167Receipts);
                }
                else
                {
                    var pricePerMonth = pricePerMonths.Count > 0 ? pricePerMonths[pricePerMonths.Count - 1] : null;
                    data_payment = Md167ContractController.GetDataDebtFunc(pricePerMonths, profitValues, md167Receipts, dataItem.md167Contract, pricePerMonth?.TotalPrice * 3, depositMd167Receipts);
                }

                foreach (var item in data_payment)
                {
                    if (item.Md167ReceiptId != null)
                    {
                        Md167Receipt md167Receipt = new Md167Receipt();
                        md167Receipt = md167Receipts.Where(m => m.Id == item.Md167ReceiptId).FirstOrDefault();

                        if (md167Receipt != null)
                        {
                            GetReport07Res getReport07Res = new GetReport07Res();
                            getReport07Res.CustomerName = dataItem.CustomerName;
                            getReport07Res.HouseNumber = dataItem.HouseNumber;
                            getReport07Res.DistrictName = dataItem.DistrictName;
                            getReport07Res.WardName = dataItem.WardName;
                            getReport07Res.LaneName = dataItem.LaneName;
                            getReport07Res.Code = dataItem.Code;
                            getReport07Res.DateSign = dataItem.DateSign;
                            getReport07Res.DateGroundHandover = dataItem.DateGroundHandover;
                            getReport07Res.BillCode = md167Receipt.ReceiptCode;
                            getReport07Res.DatePayment = md167Receipt.DateOfPayment?.ToString("dd/MM/yyyy");
                            getReport07Res.BillDate = md167Receipt.DateOfReceipt?.ToString("dd/MM/yyyy");
                            getReport07Res.RentCostContract = item.AmountPaidPerMonth;
                            getReport07Res.Pay = item.AmountToBePaid;
                            getReport07Res.Paid = item.AmountPaid;
                            getReport07Res.PriceDiff = item.AmountDiff;
                            getReport07Res.Note = item.Note;

                            res.Add(getReport07Res);
                        }
                    }
                }
            }

            def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
            def.data = res;
            return Ok(def);
        }


        #region Báo cáo 3.3: Thông tin công nợ
        //[HttpPost("GetReportDebt")]
        //public IActionResult GetReporDebt([FromBody] ReqReportDebt req)
        //{
        //    string accessToken = Request.Headers[HeaderNames.Authorization];
        //    Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
        //    if (token == null)
        //    {
        //        return Unauthorized();
        //    }

        //    DefaultResponse def = new DefaultResponse();
        //    //check role
        //    var identity = (ClaimsIdentity)User.Identity;
        //    int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id").Select(c => c.Value).SingleOrDefault());
        //    string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();

        //    if (!CheckRole.CheckRoleByCode(access_key, functionCode_RpDebt, (int)AppEnums.Action.VIEW))
        //    {
        //        def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
        //        return Ok(def);
        //    }

        //    var data = (from c in _context.Md167Contracts
        //                join h in _context.Md167Houses on c.HouseId equals h.Id
        //                where c.Status != AppEnums.EntityStatus.DELETED
        //                && h.Status != AppEnums.EntityStatus.DELETED
        //                && ((req.FromDate != null && c.DateSign >= req.FromDate) || (req.FromDate == null && 1 == 1))
        //                && ((req.ToDate != null && c.DateSign <= req.ToDate) || (req.ToDate == null && 1 == 1))
        //                && ((req.CustomertID != null && c.DelegateId == req.CustomertID) || (req.CustomertID == null && 1 == 1))
        //                && ((req.TransferUnit != null && h.Md167TransferUnitId == req.TransferUnit) || (req.TransferUnit == null && 1 == 1))
        //                && ((req.LaneId != null && h.LaneId == req.LaneId) || (req.LaneId == null && 1 == 1))
        //                && ((req.WardId != null && h.WardId == req.WardId) || (req.WardId == null && 1 == 1))
        //                && ((req.DistrictId != null && h.DistrictId == req.DistrictId) || (req.DistrictId == null && 1 == 1))
        //                && ((req.HouseId != null && h.Id == req.HouseId) || (req.HouseId == null && 1 == 1))
        //                && c.ContractStatus == AppEnums.ContractStatus167.CON_HIEU_LUC
        //                select new
        //                {
        //                    Md167ContractId = c.Id,
        //                    DistrictId = h.DistrictId,
        //                    WardId = h.WardId,
        //                    LaneId = h.LaneId,
        //                    HouseTypeId = h.HouseTypeId,
        //                    HouseName = _context.Md167HouseTypes.Where(x => x.Id == h.HouseTypeId).Select(x => x.Name).FirstOrDefault(),
        //                    //CustomerName = _context.Md167Delegates.Where(x => x.Id == c.DelegateId).Select(x => x.Name).FirstOrDefault(),
        //                    //HouseNumber = h.HouseNumber,
        //                    DistrictName = _context.Districts.Where(x => x.Id == h.DistrictId).Select(x => x.Name).FirstOrDefault(),
        //                    //WardName = _context.Wards.Where(x => x.Id == h.WardId).Select(x => x.Name).FirstOrDefault(),
        //                    //LaneName = _context.Lanies.Where(x => x.Id == h.LandId).Select(x => x.Name).FirstOrDefault(),
        //                    //Code = c.Code,
        //                    //DateSign = c.DateSign.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
        //                    //DateGroundHandover = c.DateGroundHandover.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
        //                    //DatePayment = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
        //                    //BillCode = "",
        //                    //BillDate = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
        //                    //RentCostContract = 0,
        //                    //Pay = 0,
        //                    //Paid = 0,
        //                    //PriceDiff = 0,
        //                    //Note = h.Note,
        //                    md167Contract = c
        //                }).ToList();

        //    List<Md167ProfitValue> profitValues = _context.Md167ProfitValues.Where(p => p.Status != EntityStatus.DELETED).ToList();

        //    List<ResReportDebt> res = new List<ResReportDebt>();

        //    var groupData = data.GroupBy(x => x.DistrictId).ToList();
        //    foreach (var groupDataItem in groupData)
        //    {
        //        ResReportDebt resReportDebt = new ResReportDebt();
        //        resReportDebt.DistrictId = groupDataItem.Key;
        //        resReportDebt.DistrictName = groupDataItem.FirstOrDefault().DistrictName;
        //        resReportDebt.resReportDebtItems = new List<ResReportDebtItem>();

        //        //Nhóm theo loại nhà
        //        var groupByHouseType = groupDataItem.ToList().GroupBy(x => x.HouseTypeId);

        //        foreach (var groupByHouseTypeItem in groupByHouseType)
        //        {
        //            ResReportDebtItem resReportDebtItem = new ResReportDebtItem();
        //            resReportDebtItem.HouseId = groupByHouseTypeItem.FirstOrDefault().HouseTypeId;
        //            resReportDebtItem.HouseName = groupByHouseTypeItem.FirstOrDefault().HouseName;

        //            resReportDebtItem.AmountToBePaid = 0;
        //            resReportDebtItem.AmountPaid = 0;
        //            resReportDebtItem.AmountDiff = 0;

        //            foreach (var contract in groupByHouseTypeItem.ToList())
        //            {
        //                var pricePerMonths = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == contract.Md167ContractId && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
        //                List<Md167Receipt> md167Receipts = _context.Md167Receipts.Where(m => m.Md167ContractId == contract.Md167ContractId && m.PaidDeposit != true && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();

        //                List<GroupMd167DebtData> data_payment = null;
        //                if (contract.md167Contract.Type == Contract167Type.MAIN)
        //                {
        //                    //Ds phiếu thu thanh toán tiền thế chân
        //                    List<Md167Receipt> depositMd167Receipts = _context.Md167Receipts.Where(m => m.Md167ContractId == contract.md167Contract.Id && m.PaidDeposit == true && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();

        //                    //Tìm hợp đồng liên quan để lấy tiền thế chân
        //                    Md167Contract dataRelated = _context.Md167Contracts.Where(x => x.DelegateId == contract.md167Contract.DelegateId && x.HouseId == contract.md167Contract.HouseId && x.RefundPaidDeposit != true && x.Status != EntityStatus.DELETED).FirstOrDefault();
        //                    if (dataRelated != null)
        //                    {
        //                        var pricePerMonth = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == dataRelated.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).FirstOrDefault();
        //                        data_payment = Md167ContractController.GetDataDebtFunc(pricePerMonths, profitValues, md167Receipts, contract.md167Contract, pricePerMonth?.TotalPrice * 3, depositMd167Receipts);
        //                    }
        //                    else
        //                    {
        //                        var pricePerMonth = pricePerMonths.Count > 0 ? pricePerMonths[pricePerMonths.Count - 1] : null;
        //                        data_payment = Md167ContractController.GetDataDebtFunc(pricePerMonths, profitValues, md167Receipts, contract.md167Contract, pricePerMonth?.TotalPrice * 3, depositMd167Receipts);
        //                    }
        //                }
        //                else
        //                {
        //                    Md167Contract parentData = _context.Md167Contracts.Find(contract.md167Contract.ParentId);
        //                    if (parentData == null)
        //                    {
        //                        def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
        //                        return Ok(def);
        //                    }

        //                    var parentPricePerMonths = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == parentData.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
        //                    List<Md167Receipt> parentMd167Receipts = _context.Md167Receipts.Where(m => m.Md167ContractId == parentData.Id && m.PaidDeposit != true && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();
        //                    List<Md167Receipt> depositMd167Receipts = _context.Md167Receipts.Where(m => m.Md167ContractId == contract.md167Contract.Id && m.PaidDeposit == true && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();

        //                    data_payment = Md167ContractController.GetDataExtraDebtFunc(pricePerMonths, profitValues, md167Receipts, contract.md167Contract, parentPricePerMonths, parentMd167Receipts, parentData, depositMd167Receipts);
        //                }
        //                //Md167Contract dataRelated = _context.Md167Contracts.Where(x => x.DelegateId == contract.md167Contract.DelegateId && x.HouseId == contract.md167Contract.HouseId && x.RefundPaidDeposit != true && x.Status != EntityStatus.DELETED).FirstOrDefault();
        //                //if (dataRelated != null)
        //                //{
        //                //    var pricePerMonth = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == dataRelated.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).FirstOrDefault();
        //                //    data_payment = Md167ContractController.GetDataDebtFunc(pricePerMonths, profitValues, md167Receipts, contract.md167Contract, pricePerMonth?.TotalPrice * 3, depositMd167Receipts);
        //                //}
        //                //else
        //                //{
        //                //    var pricePerMonth = pricePerMonths.Count > 0 ? pricePerMonths[pricePerMonths.Count - 1] : null;
        //                //    data_payment = Md167ContractController.GetDataDebtFunc(pricePerMonths, profitValues, md167Receipts, contract.md167Contract, pricePerMonth?.TotalPrice * 3, depositMd167Receipts);
        //                //}

        //                resReportDebtItem.AmountToBePaid += data_payment[data_payment.Count - 1].dataGroup[0].AmountToBePaid;
        //                resReportDebtItem.AmountPaid += data_payment[data_payment.Count - 1].dataGroup[0].AmountPaid;
        //                resReportDebtItem.AmountDiff += data_payment[data_payment.Count - 1].dataGroup[0].AmountDiff;
        //            }

        //            resReportDebtItem.AmountRemaining = resReportDebtItem.AmountToBePaid;
        //            resReportDebt.resReportDebtItems.Add(resReportDebtItem);

        //        }

        //        resReportDebt.Count = resReportDebt.resReportDebtItems.Count;
        //        res.Add(resReportDebt);
        //    }

        //    def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
        //    def.data = res;
        //    return Ok(def);
        //}

        [HttpPost("ExportReportDebt")]
        public async Task<IActionResult> ExportReportDebt([FromBody] List<ResReportDebt> input)
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

            if (!CheckRole.CheckRoleByCode(access_key, functionCode_RpDebt, (int)AppEnums.Action.EXPORT))
            {
                return new FileContentResult(new byte[0], "application/octet-stream");
            }

            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"MD167/ReportDebt.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);

            MemoryStream ms = WriteDataToExcelReportDebt(templatePath, 0, input);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", "Báo cáo 07");
        }

        private static MemoryStream WriteDataToExcelReportDebt(string templatePath, int sheetnumber, List<ResReportDebt> data)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowStart = 3;

            if (sheet != null)
            {
                int datacol = 8;
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
                        int idx = 1;
                        foreach (var item in itemdata.resReportDebtItems)
                        {
                            XSSFRow row = (XSSFRow)sheet.CreateRow(rowStart);
                            for (int i = 0; i < datacol; i++)
                            {
                                row.CreateCell(i).CellStyle = rowStyle[i];
                                if (i == 0)
                                {
                                    if (idx == itemdata.Count)
                                    {
                                        if (itemdata.Count == 1)
                                        {
                                            row.GetCell(i).SetCellValue(itemdata.DistrictName);
                                        }
                                        else
                                        {
                                            CellRangeAddress cellRangeAddress = new CellRangeAddress(rowStart - idx + 1, rowStart, i, i);
                                            sheet.AddMergedRegion(cellRangeAddress);

                                            sheet.GetRow(rowStart - idx + 1).GetCell(i).SetCellValue(itemdata.DistrictName);
                                        }
                                    }
                                }
                                else if (i == 1)
                                {
                                    row.GetCell(i).SetCellValue(item.HouseName);
                                }
                                else if (i == 2)
                                {
                                    if (item.AmountToBePaid != null)
                                        row.GetCell(i).SetCellValue((double)item.AmountToBePaid);
                                }
                                else if (i == 3)
                                {
                                    if (item.AmountPaid != null)
                                        row.GetCell(i).SetCellValue((double)item.AmountPaid);
                                }
                                else if (i == 4)
                                {
                                    if (item.AmountDiff != null)
                                        row.GetCell(i).SetCellValue((double)item.AmountDiff);
                                }
                                else if (i == 5)
                                {
                                    if (item.AmountUsed != null)
                                        row.GetCell(i).SetCellValue((double)item.AmountUsed);
                                }
                                else if (i == 6)
                                {
                                    if (item.AmountSubmitted != null)
                                        row.GetCell(i).SetCellValue((double)item.AmountSubmitted);
                                }
                                else if (i == 7)
                                {
                                    if (item.AmountRemaining != null)
                                        row.GetCell(i).SetCellValue((double)item.AmountRemaining);
                                }
                            }

                            idx++;
                            rowStart++;

                        }
                    }

                    //Tạo dòng tổng
                    XSSFRow totalRow = (XSSFRow)sheet.CreateRow(rowStart);
                    for (int i = 0; i < datacol; i++)
                    {
                        totalRow.CreateCell(i).CellStyle = rowStyle[i];
                        if (i == 0)
                        {
                            CellRangeAddress cellRangeAddress = new CellRangeAddress(rowStart, rowStart, 0, 1);
                            sheet.AddMergedRegion(cellRangeAddress);
                            totalRow.GetCell(i).SetCellValue("TỔNG");
                        }
                        else if (i == 1)
                        {
                        }
                        else if (i == 2)
                        {
                            var amountToBePaid = data.Sum(x => x.resReportDebtItems.Sum(y => y.AmountToBePaid ?? 0));
                            totalRow.GetCell(i).SetCellValue((double)amountToBePaid);
                        }
                        else if (i == 3)
                        {
                            var amountPaid = data.Sum(x => x.resReportDebtItems.Sum(y => y.AmountPaid ?? 0));
                            totalRow.GetCell(i).SetCellValue((double)amountPaid);
                        }
                        else if (i == 4)
                        {
                            var amountDiff = data.Sum(x => x.resReportDebtItems.Sum(y => y.AmountDiff ?? 0));
                            totalRow.GetCell(i).SetCellValue((double)amountDiff);
                        }
                        else if (i == 5)
                        {
                            var amountUsed = data.Sum(x => x.resReportDebtItems.Sum(y => y.AmountUsed ?? 0));
                            totalRow.GetCell(i).SetCellValue((double)amountUsed);
                        }
                        else if (i == 6)
                        {
                            var amountSubmitted = data.Sum(x => x.resReportDebtItems.Sum(y => y.AmountSubmitted ?? 0));
                            totalRow.GetCell(i).SetCellValue((double)amountSubmitted);
                        }
                        else if (i == 7)
                        {
                            var amountRemaining = data.Sum(x => x.resReportDebtItems.Sum(y => y.AmountRemaining ?? 0));
                            totalRow.GetCell(i).SetCellValue((double)amountRemaining);
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

        #endregion
        [HttpPost("GetReportDebt")]
        public IActionResult GetReportDebt([FromBody] ReqReportDebt req)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = _context.Tokens
                .FirstOrDefault(t => Convert.ToString(t.AccessToken) == accessToken);

            if (token == null)
                return Unauthorized();

            DefaultResponse def = new DefaultResponse();

            // check role
            var identity = (ClaimsIdentity)User.Identity;
            int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id")
                                                  .Select(c => c.Value).SingleOrDefault());
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey")
                                               .Select(c => c.Value).SingleOrDefault();

            if (!CheckRole.CheckRoleByCode(access_key, functionCode_RpDebt, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }

            // load danh mục liên quan
            var districts = _context.Districts.Where(e => e.Status != EntityStatus.DELETED).ToList();
            var wards = _context.Wards.Where(e => e.Status != EntityStatus.DELETED).ToList();
            var lanies = _context.Lanies.Where(e => e.Status != EntityStatus.DELETED).ToList();

            // lấy danh sách hợp đồng
            var rawData = (from c in _context.Md167Contracts
                           join h in _context.Md167Houses on c.HouseId equals h.Id
                           where c.Status != AppEnums.EntityStatus.DELETED
                              && h.Status != AppEnums.EntityStatus.DELETED
                              && (req.FromDate == null || c.DateSign >= req.FromDate)
                              && (req.ToDate == null || c.DateSign <= req.ToDate)
                              && (req.CustomertID == null || c.DelegateId == req.CustomertID)
                              && (req.TransferUnit == null || h.Md167TransferUnitId == req.TransferUnit)
                              && (req.LaneId == null || h.LaneId == req.LaneId)
                              && (req.WardId == null || h.WardId == req.WardId)
                              && (req.DistrictId == null || h.DistrictId == req.DistrictId)
                              && (req.HouseId == null || h.Id == req.HouseId)
                              && c.ContractStatus == AppEnums.ContractStatus167.CON_HIEU_LUC
                           select new
                           {
                               Contract = c,
                               House = h
                           }).ToList();

            // map lại District/Ward/Lane + HouseType
            var mappedData = rawData.Select(d =>
            {
                var house = d.House;
                District district = null;
                Ward ward = null;
                Lane lane = null;
                int? houseTypeId = null;
                string houseTypeName = null;

                if (house.TypeHouse == Md167House.Type_House.House)
                {
                    district = districts.FirstOrDefault(x => x.Id == house.DistrictId);
                    ward = wards.FirstOrDefault(x => x.Id == house.WardId);
                    lane = lanies.FirstOrDefault(x => x.Id == house.LaneId);

                    houseTypeId = house.HouseTypeId;
                    houseTypeName = _context.Md167HouseTypes
                                            .Where(x => x.Id == house.HouseTypeId)
                                            .Select(x => x.Name)
                                            .FirstOrDefault();
                }
                else
                {
                    var parentHouse = _context.Md167Houses.Find(house.Md167HouseId);
                    if (parentHouse != null)
                    {
                        district = districts.FirstOrDefault(x => x.Id == parentHouse.DistrictId);
                        ward = wards.FirstOrDefault(x => x.Id == parentHouse.WardId);
                        lane = lanies.FirstOrDefault(x => x.Id == parentHouse.LaneId);

                        houseTypeId = parentHouse.HouseTypeId;
                        houseTypeName = _context.Md167HouseTypes
                                                .Where(x => x.Id == parentHouse.HouseTypeId)
                                                .Select(x => x.Name)
                                                .FirstOrDefault();
                    }
                }

                return new
                {
                    Md167ContractId = d.Contract.Id,
                    DistrictId = district?.Id,
                    DistrictName = district?.Name,
                    WardId = ward?.Id,
                    LaneId = lane?.Id,
                    HouseTypeId = houseTypeId,
                    HouseTypeName = houseTypeName,
                    md167Contract = d.Contract
                };
            }).ToList();

            List<Md167ProfitValue> profitValues = _context.Md167ProfitValues
                .Where(p => p.Status != EntityStatus.DELETED).ToList();

            List<ResReportDebt> res = new List<ResReportDebt>();

            // nhóm theo quận
            var groupData = mappedData.GroupBy(x => x.DistrictId).ToList();
            foreach (var groupDataItem in groupData)
            {
                ResReportDebt resReportDebt = new ResReportDebt
                {
                    DistrictId = groupDataItem.Key ?? 0,
                    DistrictName = groupDataItem.FirstOrDefault()?.DistrictName,
                    resReportDebtItems = new List<ResReportDebtItem>()
                };

                // nhóm theo loại nhà
                var groupByHouseType = groupDataItem.GroupBy(x => x.HouseTypeId);

                foreach (var groupByHouseTypeItem in groupByHouseType)
                {
                    ResReportDebtItem resReportDebtItem = new ResReportDebtItem
                    {
                        HouseId = groupByHouseTypeItem.Key,
                        HouseName = groupByHouseTypeItem.FirstOrDefault()?.HouseTypeName,
                        AmountToBePaid = 0,
                        AmountPaid = 0,
                        AmountDiff = 0
                    };

                    foreach (var contract in groupByHouseTypeItem)
                    {
                        var pricePerMonths = _context.Md167PricePerMonths
                            .Where(l => l.Md167ContractId == contract.Md167ContractId
                                     && l.Status != AppEnums.EntityStatus.DELETED)
                            .OrderBy(x => x.UpdatedAt).ToList();

                        var md167Receipts = _context.Md167Receipts
                            .Where(m => m.Md167ContractId == contract.Md167ContractId
                                     && m.PaidDeposit != true
                                     && m.Status != EntityStatus.DELETED)
                            .OrderBy(x => x.DateOfReceipt).ToList();

                        List<GroupMd167DebtData> data_payment = null;

                        if (contract.md167Contract.Type == Contract167Type.MAIN)
                        {
                            var depositMd167Receipts = _context.Md167Receipts
                                .Where(m => m.Md167ContractId == contract.md167Contract.Id
                                         && m.PaidDeposit == true
                                         && m.Status != EntityStatus.DELETED)
                                .OrderBy(x => x.DateOfReceipt).ToList();

                            var dataRelated = _context.Md167Contracts
                                .FirstOrDefault(x => x.DelegateId == contract.md167Contract.DelegateId
                                                  && x.HouseId == contract.md167Contract.HouseId
                                                  && x.RefundPaidDeposit != true
                                                  && x.Status != EntityStatus.DELETED);

                            if (dataRelated != null)
                            {
                                var pricePerMonth = _context.Md167PricePerMonths
                                    .Where(l => l.Md167ContractId == dataRelated.Id
                                             && l.Status != AppEnums.EntityStatus.DELETED)
                                    .OrderBy(x => x.UpdatedAt).FirstOrDefault();

                                data_payment = Md167ContractController.GetDataDebtFunc(
                                    pricePerMonths, profitValues, md167Receipts,
                                    contract.md167Contract,
                                    pricePerMonth?.TotalPrice * 3, depositMd167Receipts);
                            }
                            else
                            {
                                var pricePerMonth = pricePerMonths.LastOrDefault();
                                data_payment = Md167ContractController.GetDataDebtFunc(
                                    pricePerMonths, profitValues, md167Receipts,
                                    contract.md167Contract,
                                    pricePerMonth?.TotalPrice * 3, depositMd167Receipts);
                            }
                        }
                        else // EXTRA
                        {
                            var parentData = _context.Md167Contracts.Find(contract.md167Contract.ParentId);
                            if (parentData == null)
                            {
                                def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                                return Ok(def);
                            }

                            var parentPricePerMonths = _context.Md167PricePerMonths
                                .Where(l => l.Md167ContractId == parentData.Id
                                         && l.Status != AppEnums.EntityStatus.DELETED)
                                .OrderBy(x => x.UpdatedAt).ToList();

                            var parentMd167Receipts = _context.Md167Receipts
                                .Where(m => m.Md167ContractId == parentData.Id
                                         && m.PaidDeposit != true
                                         && m.Status != EntityStatus.DELETED)
                                .OrderBy(x => x.DateOfReceipt).ToList();

                            var depositMd167Receipts = _context.Md167Receipts
                                .Where(m => m.Md167ContractId == contract.md167Contract.Id
                                         && m.PaidDeposit == true
                                         && m.Status != EntityStatus.DELETED)
                                .OrderBy(x => x.DateOfReceipt).ToList();

                            data_payment = Md167ContractController.GetDataExtraDebtFunc(
                                pricePerMonths, profitValues, md167Receipts,
                                contract.md167Contract,
                                parentPricePerMonths, parentMd167Receipts,
                                parentData, depositMd167Receipts);
                        }

                        if (data_payment != null && data_payment.Any())
                        {
                            var last = data_payment.Last().dataGroup[0];
                            resReportDebtItem.AmountToBePaid += last.AmountToBePaid;
                            resReportDebtItem.AmountPaid += last.AmountPaid;
                            resReportDebtItem.AmountDiff += last.AmountDiff;
                        }
                    }

                    resReportDebtItem.AmountRemaining = resReportDebtItem.AmountToBePaid;
                    resReportDebt.resReportDebtItems.Add(resReportDebtItem);
                }

                resReportDebt.Count = resReportDebt.resReportDebtItems.Count;
                res.Add(resReportDebt);
            }

            def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
            def.data = res;
            return Ok(def);
        }



        [HttpPost("GetReportPayment")]
        public IActionResult GetReportPayment([FromBody] GetReportPaymentReq req)
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
            if (!CheckRole.CheckRoleByCode(access_key, funcyionCode_RpPayment, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }

            IQueryable<Province> provincies = _context.Provincies.Where(x => x.Status != EntityStatus.DELETED);
            IQueryable<District> districts = _context.Districts.Where(x => x.Status != EntityStatus.DELETED);
            IQueryable<Ward> wards = _context.Wards.Where(x => x.Status != EntityStatus.DELETED);
            IQueryable<Lane> lanes = _context.Lanies.Where(x => x.Status != EntityStatus.DELETED);
            IQueryable<HousePayment> housePayments = _context.HousePayments.Where(x => x.Status != EntityStatus.DELETED);
            IQueryable<Md167House> md167Houses = _context.Md167Houses.Where(f => f.TypeHouse != Type_House.Apartment && f.Status != EntityStatus.DELETED);
            IQueryable<Md167ManagePayment> md167ManagePayments = _context.Md167ManagePayments.Where(l => l.Status != EntityStatus.DELETED);
            var res = (from c in _context.HousePayments
                       join h in _context.Md167Houses on c.HouseId equals h.Id
                       join m in _context.Md167ManagePayments on c.Md167PaymentId equals m.Id
                       where c.Status != AppEnums.EntityStatus.DELETED
                       && h.Status != AppEnums.EntityStatus.DELETED
                       && m.Status != AppEnums.EntityStatus.DELETED
                       && ((req.FromYear != null && m.Year >= req.FromYear) || (req.FromYear == null && 1 == 1))
                       && ((req.ToYear != null && m.Year <= req.ToYear) || (req.ToYear == null && 1 == 1))
                       && ((req.DistrictId != null && h.DistrictId == req.DistrictId) || (req.DistrictId == null && 1 == 1))
                       && ((req.WardId != null && h.WardId == req.WardId) || (req.WardId == null && 1 == 1))
                       && ((req.LaneId != null && h.LaneId == req.LaneId) || (req.LaneId == null && 1 == 1))
                       && ((req.HouseId != null && h.Id == req.HouseId) || (req.HouseId == null && 1 == 1))
                       orderby m.Year
                       select new
                       {
                           HouseCode = h.Code,
                           HouseName = h.HouseNumber,
                           DistrictName = districts.Where(f => f.Id == h.DistrictId).Select(f => f.Name).FirstOrDefault(),
                           WardName = wards.Where(f => f.Id == h.WardId).Select(f => f.Name).FirstOrDefault(),
                           LaneName = lanes.Where(f => f.Id == h.LaneId).Select(f => f.Name).FirstOrDefault(),
                           Year = m.Year,
                           Date = m.Date,
                           TaxNN = h.InfoValue.TaxNN,
                           Paid = c.Paid,
                           Debt = c.Debt,
                       }).ToList();

            def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
            def.data = res;
            return Ok(def);
        }


    }
}
