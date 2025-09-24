﻿using IOITQln.Common.Enums;
using IOITQln.Common.Services;
using IOITQln.Common.ViewModels.Common;
using IOITQln.Entities;
using IOITQln.Persistence;
using AutoMapper;
using log4net;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;
using System.Net.Http;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using IOITQln.Models.Data;
using System.Drawing;
using System.Security.Claims;
using IOITQln.Common.Constants;
using Microsoft.EntityFrameworkCore;
using FluentValidation.Validators;
using NPOI.SS.Formula.Functions;
using Floor = IOITQln.Entities.Floor;
using static IOITQln.Common.Enums.AppEnums;
using static log4net.Appender.RollingFileAppender;
using Microsoft.Net.Http.Headers;

namespace IOITQln.Controllers.ApiNoc
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ExportWordPT9Controller : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("GetExportWordPT09", "GetExportWordPT09");
        private static string functionCode = "PRICING";
        private IWebHostEnvironment _hostingEnvironment;
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        public ExportWordPT9Controller(ApiDbContext context, IMapper mapper, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        [HttpPost("GetExportWordPT09/{id}")]
        public async Task<IActionResult> GetExportWordPT09(Guid id)
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
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.EXPORT))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }
            try
            {
                string path = insertDataToTemplate(id);
                if (path != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                        string fileName = "mauPT_9-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";

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
                log.Error("GetExportWordExample:" + ex.Message);
                throw;
            }
        }

        private string insertDataToTemplate(Guid id)
        {


            RentFile rentFile = _context.RentFiles.Where(l => l.Id == id && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
            if (rentFile != null)
            {
                RentFlieData map_rentFlieData = _mapper.Map<RentFlieData>(rentFile);

                Block blocks = _context.Blocks.Where(l => l.Id == rentFile.RentBlockId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                BlockData map_blocks = _mapper.Map<BlockData>(blocks);

                List<BlockDetail> blockDetails = _context.BlockDetails.Where(l => l.BlockId == rentFile.RentBlockId && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<BlockDetailData> map_BlockDetails = _mapper.Map<List<BlockDetailData>>(blockDetails);
                foreach (BlockDetailData map_BlockDetail in map_BlockDetails)
                {
                    Floor floor = _context.Floors.Where(f => f.Id == map_BlockDetail.FloorId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    map_BlockDetail.FloorName = floor != null ? floor.Name : "";
                    map_BlockDetail.FloorCode = floor != null ? floor.Code : 0;

                    Area area = _context.Areas.Where(f => f.Id == map_BlockDetail.AreaId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    map_BlockDetail.AreaName = area != null ? area.Name : "";
                    map_BlockDetail.IsMezzanine = area != null ? area.IsMezzanine : null;
                }
                map_blocks.blockDetails = map_BlockDetails.OrderBy(x => x.FloorCode).ThenBy(x => x.IsMezzanine).ToList();

                map_blocks.DistrictName = _context.Districts.Where(l => l.Id == map_blocks.District && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Name).FirstOrDefault();
                map_blocks.LaneName = _context.Lanies.Where(l => l.Id == map_blocks.Lane && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Name).FirstOrDefault();
                map_blocks.WardName = _context.Wards.Where(l => l.Id == map_blocks.Ward && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Name).FirstOrDefault();


                string templatePath = Path.Combine(_hostingEnvironment.WebRootPath, "exportword/mauPT_9.docx");

                string fileName = "result" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";
                RichEditDocumentServer wordProcessor = new RichEditDocumentServer();
                wordProcessor.LoadDocumentTemplate(templatePath, DocumentFormat.OpenXml);
                var document = wordProcessor.Document;
                document.BeginUpdate();

                int dayNow = DateTime.Now.Day;
                int monthNow = DateTime.Now.Month;
                int yearNow = DateTime.Now.Year;

                string day = dayNow.ToString();
                document.ReplaceAll("<day>", day, SearchOptions.None);

                string month = monthNow.ToString();
                document.ReplaceAll("<month>", month, SearchOptions.None);

                string year = yearNow.ToString();
                document.ReplaceAll("<year>", year, SearchOptions.None);

                document.ReplaceAll("<dateNow>", day + "/" + month + "/" + year, SearchOptions.None);

                //Thông tin căn nhà
                var customerName = _context.Customers.Where(l => l.Id == rentFile.CustomerId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.FullName).FirstOrDefault();

                document.ReplaceAll("<CustomerName>", customerName != "" ? customerName : "✓", SearchOptions.None);

                if (rentFile.RentApartmentId != 0)
                {

                    Apartment apartment = _context.Apartments.Where(l => l.Id == rentFile.RentApartmentId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    ApartmentData map_apartments = _mapper.Map<ApartmentData>(apartment);
                    Block block = _context.Blocks.Where(b => b.Id == apartment.BlockId).FirstOrDefault();
                    if (block != null)
                    {
                        map_apartments.BlockName = block.Address;
                    }
                    map_apartments.apartmentDetails = _context.ApartmentDetails.Where(a => a.TargetId == rentFile.RentApartmentId && a.Type == TypeApartmentDetail.APARTMENT && a.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();

                    document.ReplaceAll("<numberAdr>", apartment.Address != "" ? blocks.Address : "✓", SearchOptions.None);

                    var lane = _context.Lanies.Where(l => l.Id == block.Lane && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Name).FirstOrDefault();
                    var ward = _context.Wards.Where(l => l.Id == block.Ward && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Name).FirstOrDefault();
                    var district = _context.Districts.Where(l => l.Id == block.District && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Name).FirstOrDefault();

                    document.ReplaceAll("<lane>", lane != "" ? lane : "✓", SearchOptions.None);
                    document.ReplaceAll("<ward>", ward != "" ? ward : "✓", SearchOptions.None);
                    document.ReplaceAll("<district>", district != "" ? district : "✓", SearchOptions.None);

                    DocumentRange[] ranges = document.FindAll("<DisposeTime>", DevExpress.XtraRichEdit.API.Native.SearchOptions.None);
                    DocumentRange lastRange = ranges[0];

                    map_apartments.apartmentDetails.ForEach(item =>
                    {
                        lastRange = document.InsertText(lastRange.End, $" \t Thời điểm bố trí sử dụng nhà là: Ngày {item.DisposeTime.Value.Day} Tháng {item.DisposeTime.Value.Month} Năm {item.DisposeTime.Value.Year} ");
                        CharacterProperties cp = document.BeginUpdateCharacters(lastRange);
                        cp.FontSize = 14;

                        document.EndUpdateCharacters(cp);
                        var breakLine = document.InsertText(ranges[0].Start, "\n");
                    });
                    var str = "";
                    document.ReplaceAll("<DisposeTime>", str, SearchOptions.None);
                }
                else
                {
                    document.ReplaceAll("<numberAdr>", blocks.Address != "" ? blocks.Address : "✓", SearchOptions.None);
                    document.ReplaceAll("<lane>", map_blocks.LaneName != "" ? map_blocks.LaneName : "✓", SearchOptions.None);
                    document.ReplaceAll("<ward>", map_blocks.WardName != "" ? map_blocks.WardName : "✓", SearchOptions.None);
                    document.ReplaceAll("<district>", map_blocks.DistrictName != "" ? map_blocks.DistrictName : "✓", SearchOptions.None);

                    DocumentRange[] ranges = document.FindAll("<DisposeTime>", DevExpress.XtraRichEdit.API.Native.SearchOptions.None);
                    DocumentRange lastRange = ranges[0];

                    map_blocks.blockDetails.ForEach(item =>
                    {
                        lastRange = document.InsertText(lastRange.End, $" \t Thời điểm bố trí sử dụng nhà là: Ngày {item.DisposeTime.Value.Day} Tháng {item.DisposeTime.Value.Month} Năm {item.DisposeTime.Value.Year} ");
                        CharacterProperties cp = document.BeginUpdateCharacters(lastRange);
                        cp.FontSize = 14;

                        TimeSpan timeSpan = new TimeSpan(23, 59, 59);
                        DateTime result = (DateTime)(DateTime.Now + timeSpan);
                        document.EndUpdateCharacters(cp);

                        var breakLine = document.InsertText(ranges[0].Start, "\n");

                    });
                    var str = "";
                    document.ReplaceAll("<DisposeTime>", str, SearchOptions.None);
                }

                document.EndUpdate();
                wordProcessor.SaveDocument(fileName, DocumentFormat.OpenXml);
                return fileName;
            }
            else
            {
                return null;
            }
        }

        #region Xuất hợp đồng thuê nhà ở cũ chuyển tiếp
        [HttpPost("GetExportHdctNoc/{id}")]
        public async Task<IActionResult> GetExportHdctNoc(Guid id)
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
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.EXPORT))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }

            try
            {
                string path = insertDataHdctToTemplate(id);
                if (path != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                        string fileName = "bien_ban-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";

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
                log.Error("GetExportWordExample:" + ex.Message);
                throw;
            }
        }

        private string insertDataHdctToTemplate(Guid id)
        {
            RentFile rentFile = _context.RentFiles.Where(r => r.Id == id && r.Status != EntityStatus.DELETED).FirstOrDefault();
            if (rentFile != null)
            {
                Block block = _context.Blocks.Where(b => b.Id == rentFile.RentBlockId && b.TypeBlockEntity == TypeBlockEntity.BLOCK_RENT && b.Status != EntityStatus.DELETED).FirstOrDefault();
                Apartment apartment = _context.Apartments.Where(a => a.BlockId == rentFile.RentBlockId && a.TypeApartmentEntity == TypeApartmentEntity.APARTMENT_RENT).FirstOrDefault();
               Customer customer = _context.Customers.Where(c => c.Id == rentFile.CustomerId && c.Status != EntityStatus.DELETED).FirstOrDefault();

                if (block == null || customer == null || (apartment == null && rentFile.TypeReportApply != TypeReportApply.NHA_RIENG_LE))
                {
                    return null;
                }
                else
                {
                    string templatePath = Path.Combine(_hostingEnvironment.WebRootPath, "exportword/hop_dong_thue_nha_o_cu_chuyen_tiep.docx");
                    string fileName = "result" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";
                    RichEditDocumentServer wordProcessor = new RichEditDocumentServer();
                    wordProcessor.LoadDocumentTemplate(templatePath, DocumentFormat.OpenXml);
                    var document = wordProcessor.Document;
                    document.BeginUpdate();

                    string day = rentFile.DateHD != null ? (rentFile.DateHD.Day < 10 ? "0" + rentFile.DateHD.Day.ToString() : rentFile.DateHD.Day.ToString()) : "";
                    document.ReplaceAll("<ngay>", day, SearchOptions.None);

                    string month = rentFile.DateHD != null ? (rentFile.DateHD.Month < 10 ? "0" + rentFile.DateHD.Month.ToString() : rentFile.DateHD.Month.ToString()) : "";
                    document.ReplaceAll("<thang>", month, SearchOptions.None);

                    string year = rentFile.DateHD != null ? rentFile.DateHD.Year.ToString() : "";
                    document.ReplaceAll("<nam>", year, SearchOptions.None);

                    document.ReplaceAll("<sohopdong>", rentFile.Code ?? "", SearchOptions.None);

                    document.ReplaceAll("<ongba>", customer.FullName ?? "", SearchOptions.None);
                    document.ReplaceAll("<cccd>", customer.Code ?? "", SearchOptions.None);
                    if (customer.Doc != null)
                    {
                        document.ReplaceAll("<capngay>", $"{customer.Doc.Value.Day}/{customer.Doc.Value.Month}/{customer.Doc.Value.Year}", SearchOptions.None);
                    }
                    else
                    {
                        document.ReplaceAll("<capngay>", "", SearchOptions.None);
                    }

                    document.ReplaceAll("<noicap>", customer.PlaceCode ?? "", SearchOptions.None);
                    document.ReplaceAll("<hokhauthuongtru>", "", SearchOptions.None);
                    document.ReplaceAll("<diachilienhe>", customer.Address ?? "", SearchOptions.None);
                    document.ReplaceAll("<dienthoai>", customer.Phone ?? "", SearchOptions.None);

                    //Thông tin về căn hộ cho thuê
                    //Kiểm tra loại biên bản

                    string typeBlock = getTypeBlock(block.TypeBlockId);
                    document.ReplaceAll("<loainha>", typeBlock, SearchOptions.None);

                    List<LevelBlockMap> levelBlockMaps = _context.LevelBlockMaps.Where(l => l.BlockId == block.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    string levelBlock = getLevelBlock(levelBlockMaps);
                    document.ReplaceAll("<capnha>", levelBlock, SearchOptions.None);

                    if (rentFile.CodeCH != null)
                    {
                        document.ReplaceAll("<madinhdanh>", rentFile.CodeCH != null ? rentFile.CodeCH : "", SearchOptions.None);
                    }
                    else if (rentFile.CodeCN != null)
                    {
                        document.ReplaceAll("<madinhdanh>", rentFile.CodeCN != null ? rentFile.CodeCN : "", SearchOptions.None);
                    }

                    Lane lane = _context.Lanies.Where(l => l.Id == block.Lane && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    document.ReplaceAll("<duong>", lane != null ? lane.Name : "", SearchOptions.None);

                    Ward ward = _context.Wards.Where(l => l.Id == block.Ward && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    document.ReplaceAll("<phuong>", ward != null ? ward.Name : "", SearchOptions.None);

                    District district = _context.Districts.Where(l => l.Id == block.District && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    document.ReplaceAll("<quanhuyen>", district != null ? district.Name : "", SearchOptions.None);

                    document.ReplaceAll("<diachi>", block.Address ?? "", SearchOptions.None);

                    if (rentFile.TypeReportApply == TypeReportApply.NHA_RIENG_LE)
                    {
                        document.ReplaceAll("<dientichsudung>", block.UseAreaValue != null ? block.UseAreaValue.ToString() : "", SearchOptions.None);

                        string buav1 = block.UseAreaValue != null ? block.UseAreaValue.ToString() : "0";
                        document.ReplaceAll("<dientichnhachinh>", buav1, SearchOptions.None);
                        document.ReplaceAll("<dientichnhaphu>", "", SearchOptions.None);
                        document.ReplaceAll("<dientichkhac>", "", SearchOptions.None);
                        document.ReplaceAll("<dientichdat>", "", SearchOptions.None);
                    }
                    else
                    {
                        string buav2 = apartment.UseAreaValue != null ? apartment.UseAreaValue.ToString() : "0";
                        document.ReplaceAll("<dientichsudung>", apartment.UseAreaValue != null ? apartment.UseAreaValue.ToString() : "", SearchOptions.None);
                        document.ReplaceAll("<dientichnhachinh>", buav2, SearchOptions.None);
                        document.ReplaceAll("<dientichnhaphu>", "", SearchOptions.None);
                        document.ReplaceAll("<dientichkhac>", "", SearchOptions.None);
                        document.ReplaceAll("<dientichdat>", "", SearchOptions.None);
                    }
                    DateTime date = DateTime.Now;

                    var discountId = _context.RentFileBCTs.Where(r => r.RentFileId == id && r.TypeBCT == 1 && r.Status != AppEnums.EntityStatus.DELETED).Select(p => p.DiscountCoefficientId).FirstOrDefault();
                    var PriceSale = _context.DiscountCoefficients.Where(r => r.Id == discountId && r.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();
                    string priceSale = PriceSale != null ? PriceSale.ToString() : "0";
                    string priceSaleString = $"{UtilsService.FormatMoney((decimal?)PriceSale)} ";
                    document.ReplaceAll("<sotienduocmiengiam>", priceSale, SearchOptions.None);
                    document.ReplaceAll("<sotienduocmiengianbangchu>", UtilsService.ConvertMoneyToString(priceSale.ToString()).ToLower(), SearchOptions.None);

                    var lstDebtsHD = _context.DebtsTables.Where(l => l.RentFileId == id && l.Type == 3 && l.Status != EntityStatus.DELETED).ToList();
                    if (lstDebtsHD != null)
                    {
                        var PriceLstDebts = Math.Ceiling(lstDebtsHD.Where(l => l.DateStart.Month == date.Month && l.DateStart.Year == date.Year).Select(p => p.Price).FirstOrDefault());
                        var price = Math.Ceiling(PriceLstDebts + (decimal)PriceSale);
                        string HD = price != null ? price.ToString() : "0";
                        string priceString = $"{UtilsService.FormatMoney(price)} ";

                        document.ReplaceAll("<giathuenha>", priceString, SearchOptions.None);
                        document.ReplaceAll("<giathuenhabangchu>", UtilsService.ConvertMoneyToString(HD.ToString()).ToLower(), SearchOptions.None);

                        var PriceRent = Math.Ceiling(price - (decimal)PriceSale);
                        string priceRent = PriceRent != null ? PriceRent.ToString() : "0";
                        string PriceRentString = $"{UtilsService.FormatMoney(PriceRent)} ";

                        document.ReplaceAll("<sotienthue>", PriceRentString, SearchOptions.None);
                        document.ReplaceAll("<sotienthuebangchu>", UtilsService.ConvertMoneyToString(priceRent.ToString()).ToLower(), SearchOptions.None);
                    }
                    else
                    {
                        document.ReplaceAll("<sotienduocmiengiam>", "", SearchOptions.None);
                        document.ReplaceAll("<sotienduocmiengianbangchu>", "", SearchOptions.None);

                        document.ReplaceAll("<giathuenha>", "", SearchOptions.None);
                        document.ReplaceAll("<giathuenhabangchu>", "", SearchOptions.None);

                        document.ReplaceAll("<sotienthue>", "", SearchOptions.None);
                        document.ReplaceAll("<sotienthuebangchu>", "", SearchOptions.None);
                    }


                    List<MemberRentFile> memberRentFiles = _context.MemberRentFiles.Where(m => m.RentFileId == id && m.Status != EntityStatus.DELETED).ToList();
                    DocumentRange[] ranges = document.FindAll("<bangthanhvien>", DevExpress.XtraRichEdit.API.Native.SearchOptions.None);
                    int row_number = memberRentFiles.Count;

                    if (row_number > 0)
                    {
                        Table table = document.Tables.Create(ranges[0].Start, row_number + 1, 4, AutoFitBehaviorType.AutoFitToContents);
                        table.ForEachCell((cell, i, j) =>
                        {

                            if (i == 0)
                            {
                                if (j == 0)
                                {
                                    document.InsertSingleLineText(cell.Range.Start, $"STT");
                                }
                                else if (j == 1)
                                {
                                    document.InsertSingleLineText(cell.Range.Start, $"Họ và tên thành viên trong Hợp đồng thuê nhà ở");
                                }
                                else if (j == 2)
                                {
                                    document.InsertSingleLineText(cell.Range.Start, $"Mối quan hệ với người đại diện đứng tên ký Hợp đồng thuê nhà ở");
                                }
                                else if (j == 3)
                                {
                                    document.InsertSingleLineText(cell.Range.Start, $"Ghi chú");
                                }
                            }
                            else
                            {
                                if (j == 0)
                                {
                                    document.InsertSingleLineText(cell.Range.Start, $"{i}");
                                }
                                else if (j == 1)
                                {
                                    document.InsertSingleLineText(cell.Range.Start, memberRentFiles[i - 1].Name);
                                }
                                else if (j == 2)
                                {
                                    document.InsertSingleLineText(cell.Range.Start, memberRentFiles[i - 1].Relationship);
                                }
                                else if (j == 3)
                                {
                                    document.InsertSingleLineText(cell.Range.Start, memberRentFiles[i - 1].Note);
                                }
                            }
                        });

                        table.TableAlignment = TableRowAlignment.Center;
                    }
                    else
                    {
                        Table table = document.Tables.Create(ranges[0].Start, 4, 4, AutoFitBehaviorType.AutoFitToContents);
                    }

                    document.ReplaceAll("<bangthanhvien>", "", SearchOptions.None);

                    //Lấy phụ lục hợp đồng
                    RentFile childRentFile = null;
                    if (rentFile.TypeReportApply == TypeReportApply.NHA_RIENG_LE)
                    {
                        childRentFile = _context.RentFiles.Where(r => r.CodeCN == rentFile.CodeCN && r.Type == 2 && r.Status != EntityStatus.DELETED).OrderByDescending(x => x.CreatedAt).FirstOrDefault();
                    }
                    else
                    {
                        childRentFile = _context.RentFiles.Where(r => r.CodeCH == rentFile.CodeCH && r.Type == 2 && r.Status != EntityStatus.DELETED).OrderByDescending(x => x.CreatedAt).FirstOrDefault();
                    }

                    if (childRentFile != null)
                    {
                        string daypl = childRentFile.DateHD != null ? (childRentFile.DateHD.Day < 10 ? "0" + childRentFile.DateHD.Day.ToString() : childRentFile.DateHD.Day.ToString()) : "";
                        document.ReplaceAll("<ngaypl>", day, SearchOptions.None);

                        string monthpl = childRentFile.DateHD != null ? (childRentFile.DateHD.Month < 10 ? "0" + childRentFile.DateHD.Month.ToString() : childRentFile.DateHD.Month.ToString()) : "";
                        document.ReplaceAll("<thangpl>", month, SearchOptions.None);

                        string yearpl = childRentFile.DateHD != null ? childRentFile.DateHD.Year.ToString() : "";
                        document.ReplaceAll("<nampl>", year, SearchOptions.None);

                        document.ReplaceAll("<sothangthue>", childRentFile.Month.ToString(), SearchOptions.None);

                        var lstDebtsPL = _context.DebtsTables.Where(l => l.RentFileId == childRentFile.Id && l.Type == 3 && l.Status != EntityStatus.DELETED).ToList();
                        if (lstDebtsPL != null)
                        {
                            var PriceLstDebtsPL = Math.Ceiling(lstDebtsPL.Where(l => l.DateStart.Month == date.Month && l.DateStart.Year == date.Year).Select(p => p.Price).FirstOrDefault());
                            string PL = PriceLstDebtsPL != null ? PriceLstDebtsPL.ToString() : "0";
                            string PLString = $"{UtilsService.FormatMoney(PriceLstDebtsPL)} ";
                            document.ReplaceAll("<giathuepl>", PLString, SearchOptions.None);
                            document.ReplaceAll("<giathueplbangchu>", UtilsService.ConvertMoneyToString(PL.ToString()).ToLower(), SearchOptions.None);
                        }
                        else
                        {
                            document.ReplaceAll("<giathuepl>", "", SearchOptions.None);
                            document.ReplaceAll("<giathueplbangchu>", "", SearchOptions.None);
                        }
                    }
                    else
                    {
                        document.ReplaceAll("<ngaypl>", "", SearchOptions.None);
                        document.ReplaceAll("<thangpl>", "", SearchOptions.None);
                        document.ReplaceAll("<nampl>", "", SearchOptions.None);
                        document.ReplaceAll("<sothangthue>", "", SearchOptions.None);

                        document.ReplaceAll("<giathuepl>", "", SearchOptions.None);
                        document.ReplaceAll("<giathueplbangchu>", "", SearchOptions.None);

                    }


                    document.EndUpdate();
                    wordProcessor.SaveDocument(fileName, DocumentFormat.OpenXml);
                    return fileName;
                }
            }
            else
            {
                return null;
            }
        }
        #endregion

        //Lấy loại nhà
        private string getTypeBlock(int? typeBlockId)
        {
            TypeBlock typeBlock = _context.TypeBlocks.Where(t => t.Id == typeBlockId && t.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

            return typeBlock != null ? typeBlock.Name : "";
        }

        //Lấy cấp nhà
        private string getLevelBlock(List<LevelBlockMap> levelBlockMaps)
        {
            string levelBlock = "";

            levelBlockMaps.ForEach(levelBlockMap =>
            {
                levelBlock = levelBlock == "" ? $"Cấp {levelBlockMap.LevelId}" : levelBlock + $" - {levelBlockMap.LevelId}";
            });

            return levelBlock;
        }
    }
}
