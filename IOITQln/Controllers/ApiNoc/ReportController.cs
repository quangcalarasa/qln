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
using FcmSharp.Requests;
using NPOI.SS.Formula.Functions;
using DevExpress.ClipboardSource.SpreadsheetML;

namespace IOITQln.Controllers.ApiInv
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("Md167Report", "Md167Report");
        private static string functionCode_SRN = "SYNTHETIC_REPORT_NOC";
        private static string functionCode_CRN = "CUSTOMER_REPORT_NOC";
        private static string functionCode_UBRN = "USAGESTATUS_BLOCK_REPORT_NOC";
        private static string functionCode_THCN = "DEBT_CONTRACT_REPORT_NOC";
        private static string functionCode_SDHN = "DUE_CONTRACT_REPORT_NOC";
        private static string functionCode_NQH = "OVERDUE_CONTRACT_REPORT_NOC";

        private readonly ApiDbContext _context;
        private IHostingEnvironment _hostingEnvironment;
        private IMapper _mapper;

        public ReportController(ApiDbContext context, IMapper mapper, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
        }

        #region Báo cáo tổng hợp 68 trường dữ liệu
        [HttpPost("GetSyntheticReportNoc")]
        public IActionResult GetSyntheticReportNoc([FromBody] SynthesisReportNocReq req)
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

            if (!CheckRole.CheckRoleByCode(access_key, functionCode_SRN, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }

            List<Block> blocks = _context.Blocks.Where(e => e.Status != EntityStatus.DELETED).ToList();
            List<Apartment> apartments = _context.Apartments.Where(e => e.Status != EntityStatus.DELETED).ToList();
            List<Customer> customers = _context.Customers.Where(e => e.Status != EntityStatus.DELETED).ToList();

            List<Lane> lanes = (from w in _context.Wards
                                join l in _context.Lanies on w.Id equals l.Ward
                                where w.Status != EntityStatus.DELETED && w.ProvinceId == 2
                                    && l.Status != EntityStatus.DELETED
                                select l).ToList();
            List<Ward> wards = _context.Wards.Where(c => c.ProvinceId == 2 && c.Status != AppEnums.EntityStatus.DELETED).ToList();
            List<District> districts = _context.Districts.Where(c => c.ProvinceId == 2 && c.Status != AppEnums.EntityStatus.DELETED).ToList();
            List<TypeBlock> typeBlocks = _context.TypeBlocks.Where(e => e.Status != EntityStatus.DELETED).ToList();
            List<LevelBlockMap> levelBlockMaps = _context.LevelBlockMaps.Where(e => e.Status != EntityStatus.DELETED).ToList();
            List<Pricing> pricings = _context.Pricings.Where(e => e.Status != EntityStatus.DELETED).ToList();
            List<RentFile> rentFiles = _context.RentFiles.Where(e => e.Status != EntityStatus.DELETED).ToList();
            List<TypeAttributeItem> typeAttributeItems = (from t in _context.TypeAttributes
                                                          join ti in _context.TypeAttributeItems on t.Id equals ti.TypeAttributeId
                                                          where t.Status != EntityStatus.DELETED && ti.Status != EntityStatus.DELETED
                                                            && t.Code == "CONTRACT_RENT_NOC_STATUS"
                                                          select ti).ToList();
            List<DecreeMap> decreeMaps = (from b in _context.Blocks
                                          join dm in _context.DecreeMaps on b.Id equals dm.TargetId
                                          where b.Status != EntityStatus.DELETED
                                            && dm.Status != EntityStatus.DELETED
                                            && dm.Type == TypeDecreeMapping.BLOCK
                                          select dm).ToList();

            List<SynthesisReportNocData> data = GetSyntheticReportNocData(blocks, apartments, customers, lanes, wards, districts, typeBlocks, levelBlockMaps, pricings, rentFiles, typeAttributeItems, decreeMaps);
            List<SynthesisReportNocData> res = data.Where(e =>
                ((req.Code != null && req.Code != "" && e.MaDinhDanh.Contains(req.Code)) || ((req.Code == null || req.Code == "") && 1 == 1))
                && ((req.CustomerName != null && req.CustomerName != "" && e.NguoiDaiDien.Contains(req.CustomerName)) || ((req.CustomerName == null || req.CustomerName == "") && 1 == 1))
                && ((req.IdentityCode != null && req.IdentityCode != "" && e.CanCuoc.Contains(req.IdentityCode)) || ((req.IdentityCode == null || req.IdentityCode == "") && 1 == 1))
                && ((req.DistrictId != null && e.DistrictId == req.DistrictId) || (req.DistrictId == null && 1 == 1))
                && ((req.WardId != null && e.WardId == req.WardId) || (req.WardId == null && 1 == 1))
                && ((req.LaneId != null && e.LaneId == req.LaneId) || (req.LaneId == null && 1 == 1))
                && ((req.HasSaleInfo != null && e.HasSaleInfo == req.HasSaleInfo) || (req.HasSaleInfo == null && 1 == 1))
                && ((req.HasRentInfo != null && e.HasRentInfo == req.HasRentInfo) || (req.HasRentInfo == null && 1 == 1))
            ).ToList();

            def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
            def.data = res;
            def.metadata = res.Count;
            return Ok(def);
        }

        [HttpPost("ExportSyntheticReportNoc")]
        public async Task<IActionResult> ExportSyntheticReportNoc([FromBody] List<ResReportDebt> input)
        {
            //string accessToken = Request.Headers[HeaderNames.Authorization];
            //Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            //if (token == null)
            //{
            //    return Unauthorized();
            //}

            //check role
            //var identity = (ClaimsIdentity)User.Identity;
            //int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id").Select(c => c.Value).SingleOrDefault());
            //string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();

            //if (!CheckRole.CheckRoleByCode(access_key, functionCode_SRN, (int)AppEnums.Action.EXPORT))
            //{
            //    return new FileContentResult(new byte[0], "application/octet-stream");
            //}

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

        private static List<SynthesisReportNocData> GetSyntheticReportNocData(List<Block> blocks, List<Apartment> apartments, List<Customer> customers, List<Lane> lanes, List<Ward> wards, List<District> districts, List<TypeBlock> typeBlocks, List<LevelBlockMap> levelBlockMaps, List<Pricing> pricings, List<RentFile> rentFiles, List<TypeAttributeItem> rentFileStatusData, List<DecreeMap> decreeMaps)
        {
            List<SynthesisReportNocData> res = new List<SynthesisReportNocData>();

            List<Block> rentBlocks = blocks.Where(b => b.TypeBlockEntity == TypeBlockEntity.BLOCK_RENT).ToList();
            List<Block> nrlRentBlocks = rentBlocks.Where(b => b.TypeReportApply == TypeReportApply.NHA_RIENG_LE).ToList();
            List<Block> nnhRentBlocks = rentBlocks.Where(b => b.TypeReportApply == TypeReportApply.NHA_HO_CHUNG || b.TypeReportApply == TypeReportApply.NHA_CHUNG_CU).ToList();

            List<Block> sellBlocks = blocks.Where(b => b.TypeBlockEntity == TypeBlockEntity.BLOCK_NORMAL).ToList();
            List<Block> nrlSellBlocks = sellBlocks.Where(b => b.TypeReportApply == TypeReportApply.NHA_RIENG_LE).ToList();
            List<Block> nnhSellBlocks = sellBlocks.Where(b => b.TypeReportApply == TypeReportApply.NHA_HO_CHUNG || b.TypeReportApply == TypeReportApply.NHA_CHUNG_CU).ToList();

            List<Apartment> rentApartments = apartments.Where(b => b.TypeApartmentEntity == TypeApartmentEntity.APARTMENT_RENT).ToList();
            List<Apartment> sellApartments = apartments.Where(b => b.TypeApartmentEntity == TypeApartmentEntity.APARTMENT_NORMAL).ToList();


            foreach (Block nrlRentBlock in nrlRentBlocks)
            {
                //Tìm nhà bên bán tương ứng
                Block nrlSellBlock = nrlSellBlocks.Where(e => e.Code == nrlRentBlock.Code).FirstOrDefault();

                SynthesisReportNocData reportData = new SynthesisReportNocData();
                reportData.LoaiBienBan = getTypeReportApplyName(nrlRentBlock.TypeReportApply);
                reportData.SoTtCan = nrlRentBlock.No;
                reportData.MaDinhDanh = nrlRentBlock.Code;
                reportData.SoNha = nrlRentBlock.Address;
                reportData.Duong = lanes.Where(e => e.Id == nrlRentBlock.Lane).FirstOrDefault()?.Name;
                reportData.Phuong = wards.Where(e => e.Id == nrlRentBlock.Ward).FirstOrDefault()?.Name;
                reportData.Quan = districts.Where(e => e.Id == nrlRentBlock.District).FirstOrDefault()?.Name;
                reportData.DistrictId = nrlRentBlock.District;
                reportData.WardId = nrlRentBlock.Ward;
                reportData.LaneId = nrlRentBlock.Lane;
                reportData.LoaiNha = typeBlocks.Where(e => e.Id == nrlRentBlock.TypeBlockId).FirstOrDefault()?.Name;

                var levelBlockMap = levelBlockMaps.Where(e => e.BlockId == nrlRentBlock.Id).ToList();
                reportData.CapNha = levelBlockMap.Count > 0 ? getLevelBlock(levelBlockMap) : "";

                reportData.KhuonVien = nrlRentBlock.CampusArea;

                if (nrlRentBlock.EstablishStateOwnership == true)
                {
                    reportData.SoQuyetDinh = nrlRentBlock.CodeEstablishStateOwnership;
                    reportData.NgayKyQuyetDinh = nrlRentBlock.DateEstablishStateOwnership;
                }

                if (nrlRentBlock.Blueprint == true)
                {
                    reportData.TenBanVe = nrlRentBlock.NameBlueprint;
                    reportData.NgayLapBanVe = nrlRentBlock.DateBlueprint;
                }

                reportData.TinhTrangNha = getUsageStatus(nrlRentBlock.UsageStatus);
                reportData.GhiChu = nrlRentBlock.UseAreaNote1;

                if (nrlSellBlock != null)
                {
                    reportData.DienTichXayDung = nrlSellBlock.ConstructionAreaValue;
                    reportData.DienTichDatRieng = nrlSellBlock.LandscapePrivateAreaValue;
                    reportData.DienTichSuDungRieng = nrlSellBlock.UseAreaValue2;
                    reportData.DienTichSuDungChung = nrlSellBlock.UseAreaValue1;
                    reportData.TongDienTichSuDungChung = nrlSellBlock.UseAreaValue1;
                    reportData.TongDienTichSuDung = nrlSellBlock.UseAreaValue;

                    reportData.ViTri = null;
                    reportData.HeSoHem = nrlSellBlock.Width;
                    reportData.HeSoPhanBo = null;
                    reportData.DonGiaDat = null;
                    reportData.ThueSuat = null;
                    reportData.TienThuePhaiNopTungNam = null;

                    List<DecreeMap> blockDecreeMaps = decreeMaps.Where(e => e.TargetId == nrlSellBlock.Id).ToList();
                    if (blockDecreeMaps.Count > 0)
                    {
                        foreach (DecreeMap blockDecreeMap in blockDecreeMaps)
                        {
                            if (blockDecreeMap.DecreeType1Id == DecreeEnum.ND_CP_99_2015)
                            {
                                reportData.ViTri = reportData.ViTri == null ? (nrlSellBlock.LandscapeLocation_99 != null ? $"V{(int)nrlSellBlock.LandscapeLocation_99}" : null) : (reportData.ViTri + (nrlSellBlock.LandscapeLocation_99 != null ? $"; V{(int)nrlSellBlock.LandscapeLocation_99}" : ""));
                                reportData.HeSoPhanBo = reportData.HeSoPhanBo == null ? (nrlSellBlock.PositionCoefficientStr_99 != null ? nrlSellBlock.PositionCoefficientStr_99 : null) : (reportData.HeSoPhanBo + (nrlSellBlock.PositionCoefficientStr_99 != null ? $"; {nrlSellBlock.PositionCoefficientStr_99}" : ""));
                                reportData.DonGiaDat = reportData.DonGiaDat == null ? (nrlSellBlock.LandScapePrice_99 != null ? UtilsService.FormatMoney(nrlSellBlock.LandScapePrice_99) : null) : (reportData.DonGiaDat + (nrlSellBlock.LandScapePrice_99 != null ? $"; {UtilsService.FormatMoney(nrlSellBlock.LandScapePrice_99)}" : ""));
                            }
                            else if (blockDecreeMap.DecreeType1Id == DecreeEnum.ND_CP_61)
                            {
                                reportData.ViTri = reportData.ViTri == null ? (nrlSellBlock.LandscapeLocationInAlley_61 != null ? $"V{(int)nrlSellBlock.LandscapeLocationInAlley_61}" : null) : (reportData.ViTri + (nrlSellBlock.LandscapeLocationInAlley_61 != null ? $"; V{(int)nrlSellBlock.LandscapeLocationInAlley_61}" : ""));
                                reportData.HeSoPhanBo = reportData.HeSoPhanBo == null ? (nrlSellBlock.PositionCoefficientStr_61 != null ? nrlSellBlock.PositionCoefficientStr_61 : null) : (reportData.HeSoPhanBo + (nrlSellBlock.PositionCoefficientStr_61 != null ? $"; {nrlSellBlock.PositionCoefficientStr_61}" : ""));
                                reportData.DonGiaDat = reportData.DonGiaDat == null ? (nrlSellBlock.LandScapePrice_61 != null ? UtilsService.FormatMoney(nrlSellBlock.LandScapePrice_61) : null) : (reportData.DonGiaDat + (nrlSellBlock.LandScapePrice_61 != null ? $"; {UtilsService.FormatMoney(nrlSellBlock.LandScapePrice_61)}" : ""));
                            }
                            else if (blockDecreeMap.DecreeType1Id == DecreeEnum.ND_CP_34_2013)
                            {
                                if (nrlSellBlock.CaseApply_34 == TypeCaseApply_34.KHOAN_1)
                                {
                                    reportData.ViTri = reportData.ViTri == null ? (nrlSellBlock.LandscapeLocationInAlley_34 != null ? $"V{(int)nrlSellBlock.LandscapeLocationInAlley_34}" : null) : (reportData.ViTri + (nrlSellBlock.LandscapeLocation_34 != null ? $"; V{(int)nrlSellBlock.LandscapeLocationInAlley_34}" : ""));
                                    reportData.HeSoPhanBo = reportData.HeSoPhanBo == null ? (nrlSellBlock.AlleyPositionCoefficientStr_34 != null ? nrlSellBlock.AlleyPositionCoefficientStr_34 : null) : (reportData.HeSoPhanBo + (nrlSellBlock.AlleyPositionCoefficientStr_34 != null ? $"; {nrlSellBlock.AlleyPositionCoefficientStr_34}" : ""));
                                    reportData.DonGiaDat = reportData.DonGiaDat == null ? (nrlSellBlock.AlleyLandScapePrice_34 != null ? UtilsService.FormatMoney(nrlSellBlock.AlleyLandScapePrice_34) : null) : (reportData.DonGiaDat + (nrlSellBlock.AlleyLandScapePrice_34 != null ? $"; {UtilsService.FormatMoney(nrlSellBlock.AlleyLandScapePrice_34)}" : ""));
                                }
                                else if (nrlSellBlock.CaseApply_34 == TypeCaseApply_34.KHOAN_2)
                                {
                                    reportData.ViTri = reportData.ViTri == null ? (nrlSellBlock.LandscapeLocation_34 != null ? $"V{(int)nrlSellBlock.LandscapeLocation_34}" : null) : (reportData.ViTri + (nrlSellBlock.LandscapeLocation_34 != null ? $"; V{(int)nrlSellBlock.LandscapeLocation_34}" : ""));
                                    reportData.HeSoPhanBo = reportData.HeSoPhanBo == null ? (nrlSellBlock.PositionCoefficientStr_34 != null ? nrlSellBlock.PositionCoefficientStr_34 : null) : (reportData.HeSoPhanBo + (nrlSellBlock.PositionCoefficientStr_34 != null ? $"; {nrlSellBlock.PositionCoefficientStr_34}" : ""));
                                    reportData.DonGiaDat = reportData.DonGiaDat == null ? (nrlSellBlock.LandScapePrice_34 != null ? UtilsService.FormatMoney(nrlSellBlock.LandScapePrice_34) : null) : (reportData.DonGiaDat + (nrlSellBlock.LandScapePrice_34 != null ? $"; {UtilsService.FormatMoney(nrlSellBlock.LandScapePrice_34)}" : ""));
                                }
                            }
                        }
                    }

                    //Tìm biên bảnh tính giá bán
                    Pricing pricing = pricings.Where(e => e.BlockId == nrlSellBlock.Id).OrderByDescending(x => x.Id).FirstOrDefault();
                    if (pricing != null)
                    {
                        #region Thông tin bán nhà
                        reportData.GiaBanNha = pricing.ApartmentPrice;
                        reportData.DienTichNha = nrlSellBlock.UseAreaValue;
                        reportData.GiaBanDat = pricing.LandPrice;
                        reportData.DienTichDat = nrlSellBlock.LandscapeAreaValue;
                        reportData.SoTienMienGiamNha = pricing.ApartmentPriceReduced;
                        reportData.SoTienMienGiamDat = pricing.LandPriceAfterReduced;
                        reportData.ThanhTien = pricing.TotalPrice;
                        reportData.TrangThai = null;                                                                                            //Chưa biết lấy ở đâu
                        reportData.SoQuyetDinhBanNha = null;                                                                                    //Chưa biết lấy ở đâu
                        reportData.NgayQuyetDinhBanNha = null;                                                                                  //Chưa biết lấy ở đâu
                        reportData.SoBienBanThanhLyHopDong = null;                                                                              //Chưa biết lấy ở đâu
                        reportData.NgayThanhLyHopDong = null;                                                                                   //Chưa biết lấy ở đâu
                        reportData.GhiChuThongTinBan = null;                                                                                    //Chưa biết lấy ở đâu
                        #endregion

                        reportData.ThoiDiemBoTriSuDung = pricing.TimeUse;
                        reportData.HasSaleInfo = true;
                    }

                    nrlSellBlocks.Remove(nrlSellBlock);
                }

                #region Thông tin hợp đồng thuê
                RentFile rentFile = rentFiles.Where(r => r.RentBlockId == nrlRentBlock.Id && r.Type == 1).OrderByDescending(r => r.DateHD).FirstOrDefault();
                if (rentFile != null)
                {
                    #region Thông tin chủ hộ
                    Customer customer = customers.Where(c => c.Id == rentFile.CustomerId).FirstOrDefault();
                    if (customer != null)
                    {
                        reportData.NguoiDaiDien = customer.FullName;
                        reportData.CanCuoc = customer.Code;
                        reportData.NgayCap = customer.Doc;
                        reportData.NoiCap = customer.PlaceCode;
                        reportData.DiaChiThuongTru = customer.Address;
                        reportData.SoDienThoai = customer.Phone;
                        reportData.ThanhVien = null;
                    }
                    #endregion

                    reportData.ThoiDiemGiaoNhanNha = null;                                                                                      //Chưa biết lấy ở đâu

                    reportData.TrangThaiHopDong = rentFileStatusData.Where(r => r.Id == rentFile.FileStatus).FirstOrDefault()?.Name;
                    reportData.SoGiayXacNhan = null;                                                                                            //Chưa biết lấy ở đâu
                    reportData.NoCuCoVat = null;                                                                                                //Chưa biết lấy ở đâu
                    reportData.SoHopDong = rentFile.Code;
                    reportData.NgayKyHopDong = rentFile.DateHD;
                    reportData.GiaThueNhaTheoCongIch = null;                                                                                    //Chưa biết lấy ở đâu
                    reportData.GiaThueNhaHopDongTam = null;                                                                                     //Chưa biết lấy ở đâu
                    reportData.GiaThueNhaChinhThuc = null;                                                                                      //Chưa biết lấy ở đâu
                    reportData.HasRentInfo = true;
                }
                else
                {
                    Customer customer = customers.Where(c => c.Id == nrlRentBlock.CustomerId).FirstOrDefault();
                    if (customer != null)
                    {
                        reportData.NguoiDaiDien = customer.FullName;
                        reportData.CanCuoc = customer.Code;
                        reportData.NgayCap = customer.Doc;
                        reportData.NoiCap = customer.PlaceCode;
                        reportData.DiaChiThuongTru = customer.Address;
                        reportData.SoDienThoai = customer.Phone;
                        reportData.ThanhVien = null;
                    }
                }
                #endregion

                res.Add(reportData);
            }

            foreach (Apartment rentApartment in rentApartments)
            {
                //Tìm nhà bên bán tương ứng
                Apartment sellApartment = sellApartments.Where(e => e.Code == rentApartment.Code).FirstOrDefault();

                //Tìm căn nhà của căn hộ này
                Block rentBlock = nnhRentBlocks.Where(e => e.Id == rentApartment.BlockId).FirstOrDefault();
                if (rentBlock != null)
                {


                    SynthesisReportNocData reportData = new SynthesisReportNocData();
                    reportData.LoaiBienBan = getTypeReportApplyName(rentApartment.TypeReportApply);
                    reportData.SoTtCan = rentBlock.No;
                    reportData.SoTtHo = rentApartment.No;
                    reportData.MaDinhDanh = rentApartment.Code;
                    reportData.SoNha = rentApartment.Address + $"({rentBlock.Address})";
                    reportData.Duong = lanes.Where(e => e.Id == rentBlock.Lane).FirstOrDefault()?.Name;
                    reportData.Phuong = wards.Where(e => e.Id == rentBlock.Ward).FirstOrDefault()?.Name;
                    reportData.Quan = districts.Where(e => e.Id == rentBlock.District).FirstOrDefault()?.Name;
                    reportData.LoaiNha = typeBlocks.Where(e => e.Id == rentBlock.TypeBlockId).FirstOrDefault()?.Name;
                    reportData.DistrictId = rentBlock.District;
                    reportData.WardId = rentBlock.Ward;
                    reportData.LaneId = rentBlock.Lane;
                    reportData.LoaiNha = typeBlocks.Where(e => e.Id == rentBlock.TypeBlockId).FirstOrDefault()?.Name;

                    var levelBlockMap = levelBlockMaps.Where(e => e.BlockId == rentBlock.Id).ToList();
                    reportData.CapNha = levelBlockMap.Count > 0 ? getLevelBlock(levelBlockMap) : "";

                    reportData.KhuonVien = rentApartment.CampusArea;

                    if (rentApartment.EstablishStateOwnership == true)
                    {
                        reportData.SoQuyetDinh = rentApartment.CodeEstablishStateOwnership;
                        reportData.NgayKyQuyetDinh = rentApartment.DateEstablishStateOwnership;

                    }

                    if (rentApartment.Blueprint == true)
                    {
                        reportData.TenBanVe = rentApartment.NameBlueprint;
                        reportData.NgayLapBanVe = rentApartment.DateBlueprint;
                    }

                    reportData.TinhTrangNha = getUsageStatus(rentApartment.UsageStatus);
                    reportData.GhiChu = rentApartment.UseAreaNote1;

                    //Tìm nhà bên bán tương ứng
                    Block sellBlock = nnhSellBlocks.Where(e => e.Code == rentBlock.Code).FirstOrDefault();
                    if (sellBlock != null)
                    {
                        reportData.ViTri = null;
                        reportData.HeSoHem = sellBlock.Width;
                        reportData.HeSoPhanBo = null;
                        reportData.DonGiaDat = null;
                        reportData.ThueSuat = null;
                        reportData.TienThuePhaiNopTungNam = null;

                        List<DecreeMap> blockDecreeMaps = decreeMaps.Where(e => e.TargetId == sellBlock.Id).ToList();
                        if (blockDecreeMaps.Count > 0)
                        {
                            foreach (DecreeMap blockDecreeMap in blockDecreeMaps)
                            {
                                if (blockDecreeMap.DecreeType1Id == DecreeEnum.ND_CP_99_2015)
                                {
                                    reportData.ViTri = reportData.ViTri == null ? (sellBlock.LandscapeLocation_99 != null ? $"V{(int)sellBlock.LandscapeLocation_99}" : null) : (reportData.ViTri + (sellBlock.LandscapeLocation_99 != null ? $"; V{(int)sellBlock.LandscapeLocation_99}" : ""));
                                    reportData.HeSoPhanBo = reportData.HeSoPhanBo == null ? (sellBlock.PositionCoefficientStr_99 != null ? sellBlock.PositionCoefficientStr_99 : null) : (reportData.HeSoPhanBo + (sellBlock.PositionCoefficientStr_99 != null ? $"; {sellBlock.PositionCoefficientStr_99}" : ""));
                                    reportData.DonGiaDat = reportData.DonGiaDat == null ? (sellBlock.LandScapePrice_99 != null ? UtilsService.FormatMoney(sellBlock.LandScapePrice_99) : null) : (reportData.DonGiaDat + (sellBlock.LandScapePrice_99 != null ? $"; {UtilsService.FormatMoney(sellBlock.LandScapePrice_99)}" : ""));
                                }
                                else if (blockDecreeMap.DecreeType1Id == DecreeEnum.ND_CP_61)
                                {
                                    reportData.ViTri = reportData.ViTri == null ? (sellBlock.LandscapeLocationInAlley_61 != null ? $"V{(int)sellBlock.LandscapeLocationInAlley_61}" : null) : (reportData.ViTri + (sellBlock.LandscapeLocationInAlley_61 != null ? $"; V{(int)sellBlock.LandscapeLocationInAlley_61}" : ""));
                                    reportData.HeSoPhanBo = reportData.HeSoPhanBo == null ? (sellBlock.PositionCoefficientStr_61 != null ? sellBlock.PositionCoefficientStr_61 : null) : (reportData.HeSoPhanBo + (sellBlock.PositionCoefficientStr_61 != null ? $"; {sellBlock.PositionCoefficientStr_61}" : ""));
                                    reportData.DonGiaDat = reportData.DonGiaDat == null ? (sellBlock.LandScapePrice_61 != null ? UtilsService.FormatMoney(sellBlock.LandScapePrice_61) : null) : (reportData.DonGiaDat + (sellBlock.LandScapePrice_61 != null ? $"; {UtilsService.FormatMoney(sellBlock.LandScapePrice_61)}" : ""));
                                }
                                else if (blockDecreeMap.DecreeType1Id == DecreeEnum.ND_CP_34_2013)
                                {
                                    if (sellBlock.CaseApply_34 == TypeCaseApply_34.KHOAN_1)
                                    {
                                        reportData.ViTri = reportData.ViTri == null ? (sellBlock.LandscapeLocationInAlley_34 != null ? $"V{(int)sellBlock.LandscapeLocationInAlley_34}" : null) : (reportData.ViTri + (sellBlock.LandscapeLocation_34 != null ? $"; V{(int)sellBlock.LandscapeLocationInAlley_34}" : ""));
                                        reportData.HeSoPhanBo = reportData.HeSoPhanBo == null ? (sellBlock.AlleyPositionCoefficientStr_34 != null ? sellBlock.AlleyPositionCoefficientStr_34 : null) : (reportData.HeSoPhanBo + (sellBlock.AlleyPositionCoefficientStr_34 != null ? $"; {sellBlock.AlleyPositionCoefficientStr_34}" : ""));
                                        reportData.DonGiaDat = reportData.DonGiaDat == null ? (sellBlock.AlleyLandScapePrice_34 != null ? UtilsService.FormatMoney(sellBlock.AlleyLandScapePrice_34) : null) : (reportData.DonGiaDat + (sellBlock.AlleyLandScapePrice_34 != null ? $"; {UtilsService.FormatMoney(sellBlock.AlleyLandScapePrice_34)}" : ""));
                                    }
                                    else if (sellBlock.CaseApply_34 == TypeCaseApply_34.KHOAN_2)
                                    {
                                        reportData.ViTri = reportData.ViTri == null ? (sellBlock.LandscapeLocation_34 != null ? $"V{(int)sellBlock.LandscapeLocation_34}" : null) : (reportData.ViTri + (sellBlock.LandscapeLocation_34 != null ? $"; V{(int)sellBlock.LandscapeLocation_34}" : ""));
                                        reportData.HeSoPhanBo = reportData.HeSoPhanBo == null ? (sellBlock.PositionCoefficientStr_34 != null ? sellBlock.PositionCoefficientStr_34 : null) : (reportData.HeSoPhanBo + (sellBlock.PositionCoefficientStr_34 != null ? $"; {sellBlock.PositionCoefficientStr_34}" : ""));
                                        reportData.DonGiaDat = reportData.DonGiaDat == null ? (sellBlock.LandScapePrice_34 != null ? UtilsService.FormatMoney(sellBlock.LandScapePrice_34) : null) : (reportData.DonGiaDat + (sellBlock.LandScapePrice_34 != null ? $"; {UtilsService.FormatMoney(sellBlock.LandScapePrice_34)}" : ""));
                                    }
                                }
                            }
                        }
                    }

                    if (sellApartment != null)
                    {
                        reportData.DienTichXayDung = sellApartment?.ConstructionAreaValue;
                        reportData.DienTichDatRieng = sellApartment?.LandscapeAreaValue2;
                        reportData.DienTichDatChung = sellApartment?.LandscapeAreaValue1;
                        reportData.DienTichDatChungPhanBo = sellApartment?.LandscapeAreaValue3;

                        reportData.DienTichSuDungRieng = sellApartment?.UseAreaValue2;
                        reportData.DienTichSuDungChung = sellApartment?.UseAreaValue1;
                        reportData.TongDienTichSuDungChung = sellApartment?.UseAreaValue1;
                        reportData.TongDienTichSuDung = sellApartment?.UseAreaValue;

                        //Tìm biên bảnh tính giá bán
                        Pricing pricing = pricings.Where(e => e.ApartmentId == sellApartment.Id).OrderByDescending(x => x.Id).FirstOrDefault();
                        if (pricing != null)
                        {
                            #region Thông tin bán nhà
                            reportData.GiaBanNha = pricing.ApartmentPrice;
                            reportData.DienTichNha = sellApartment.UseAreaValue;
                            reportData.GiaBanDat = pricing.LandPrice;
                            reportData.DienTichDat = sellApartment.LandscapeAreaValue;
                            reportData.SoTienMienGiamNha = pricing.ApartmentPriceReduced;
                            reportData.SoTienMienGiamDat = pricing.LandPriceAfterReduced;
                            reportData.ThanhTien = pricing.TotalPrice;
                            reportData.TrangThai = null;                                                                                            //Chưa biết lấy ở đâu
                            reportData.SoQuyetDinhBanNha = null;                                                                                    //Chưa biết lấy ở đâu
                            reportData.NgayQuyetDinhBanNha = null;                                                                                  //Chưa biết lấy ở đâu
                            reportData.SoBienBanThanhLyHopDong = null;                                                                              //Chưa biết lấy ở đâu
                            reportData.NgayThanhLyHopDong = null;                                                                                   //Chưa biết lấy ở đâu
                            reportData.GhiChuThongTinBan = null;                                                                                    //Chưa biết lấy ở đâu
                            #endregion

                            reportData.ThoiDiemBoTriSuDung = pricing.TimeUse;
                            reportData.HasSaleInfo = true;
                        }

                        sellApartments.Remove(sellApartment);
                    }

                    #region Thông tin hợp đồng thuê
                    RentFile rentFile = rentFiles.Where(r => r.RentApartmentId == rentApartment.Id && r.Type == 1).OrderByDescending(r => r.DateHD).FirstOrDefault();
                    if (rentFile != null)
                    {
                        #region Thông tin chủ hộ
                        Customer customer = customers.Where(c => c.Id == rentFile.CustomerId).FirstOrDefault();
                        if (customer != null)
                        {
                            reportData.NguoiDaiDien = customer.FullName;
                            reportData.CanCuoc = customer.Code;
                            reportData.NgayCap = customer.Doc;
                            reportData.NoiCap = customer.PlaceCode;
                            reportData.DiaChiThuongTru = customer.Address;
                            reportData.SoDienThoai = customer.Phone;
                            reportData.ThanhVien = null;
                        }
                        #endregion

                        reportData.ThoiDiemGiaoNhanNha = null;                                                                                      //Chưa biết lấy ở đâu

                        reportData.TrangThaiHopDong = rentFileStatusData.Where(r => r.Id == rentFile.FileStatus).FirstOrDefault()?.Name;
                        reportData.SoGiayXacNhan = null;                                                                                            //Chưa biết lấy ở đâu
                        reportData.NoCuCoVat = null;                                                                                                //Chưa biết lấy ở đâu
                        reportData.SoHopDong = rentFile.Code;
                        reportData.NgayKyHopDong = rentFile.DateHD;
                        reportData.GiaThueNhaTheoCongIch = null;                                                                                    //Chưa biết lấy ở đâu
                        reportData.GiaThueNhaHopDongTam = null;                                                                                     //Chưa biết lấy ở đâu
                        reportData.GiaThueNhaChinhThuc = null;                                                                                      //Chưa biết lấy ở đâu
                        reportData.HasRentInfo = true;
                    }
                    else
                    {
                        Customer customer = customers.Where(c => c.Id == rentApartment.CustomerId).FirstOrDefault();
                        if (customer != null)
                        {
                            reportData.NguoiDaiDien = customer.FullName;
                            reportData.CanCuoc = customer.Code;
                            reportData.NgayCap = customer.Doc;
                            reportData.NoiCap = customer.PlaceCode;
                            reportData.DiaChiThuongTru = customer.Address;
                            reportData.SoDienThoai = customer.Phone;
                            reportData.ThanhVien = null;
                        }
                    }
                    #endregion

                    res.Add(reportData);
                }
            }

            //Thêm những mà định danh chỉ có ở bên bán
            if(sellBlocks.Count > 0)
            {
                foreach (Block nrlSellBlock in nrlSellBlocks.ToList())
                {
                    SynthesisReportNocData reportData = new SynthesisReportNocData();
                    reportData.LoaiBienBan = getTypeReportApplyName(nrlSellBlock.TypeReportApply);
                    reportData.SoTtCan = nrlSellBlock.No;
                    reportData.MaDinhDanh = nrlSellBlock.Code;
                    reportData.SoNha = nrlSellBlock.Address;
                    reportData.Duong = lanes.Where(e => e.Id == nrlSellBlock.Lane).FirstOrDefault()?.Name;
                    reportData.Phuong = wards.Where(e => e.Id == nrlSellBlock.Ward).FirstOrDefault()?.Name;
                    reportData.Quan = districts.Where(e => e.Id == nrlSellBlock.District).FirstOrDefault()?.Name;
                    reportData.DistrictId = nrlSellBlock.District;
                    reportData.WardId = nrlSellBlock.Ward;
                    reportData.LaneId = nrlSellBlock.Lane;
                    reportData.LoaiNha = typeBlocks.Where(e => e.Id == nrlSellBlock.TypeBlockId).FirstOrDefault()?.Name;

                    var levelBlockMap = levelBlockMaps.Where(e => e.BlockId == nrlSellBlock.Id).ToList();
                    reportData.CapNha = levelBlockMap.Count > 0 ? getLevelBlock(levelBlockMap) : "";


                    if (nrlSellBlock != null)
                    {
                        reportData.DienTichXayDung = nrlSellBlock.ConstructionAreaValue;
                        reportData.DienTichDatRieng = nrlSellBlock.LandscapePrivateAreaValue;
                        reportData.DienTichSuDungRieng = nrlSellBlock.UseAreaValue2;
                        reportData.DienTichSuDungChung = nrlSellBlock.UseAreaValue1;
                        reportData.TongDienTichSuDungChung = nrlSellBlock.UseAreaValue1;
                        reportData.TongDienTichSuDung = nrlSellBlock.UseAreaValue;

                        reportData.ViTri = null;
                        reportData.HeSoHem = nrlSellBlock.Width;
                        reportData.HeSoPhanBo = null;
                        reportData.DonGiaDat = null;
                        reportData.ThueSuat = null;
                        reportData.TienThuePhaiNopTungNam = null;

                        List<DecreeMap> blockDecreeMaps = decreeMaps.Where(e => e.TargetId == nrlSellBlock.Id).ToList();
                        if (blockDecreeMaps.Count > 0)
                        {
                            foreach (DecreeMap blockDecreeMap in blockDecreeMaps)
                            {
                                if (blockDecreeMap.DecreeType1Id == DecreeEnum.ND_CP_99_2015)
                                {
                                    reportData.ViTri = reportData.ViTri == null ? (nrlSellBlock.LandscapeLocation_99 != null ? $"V{(int)nrlSellBlock.LandscapeLocation_99}" : null) : (reportData.ViTri + (nrlSellBlock.LandscapeLocation_99 != null ? $"; V{(int)nrlSellBlock.LandscapeLocation_99}" : ""));
                                    reportData.HeSoPhanBo = reportData.HeSoPhanBo == null ? (nrlSellBlock.PositionCoefficientStr_99 != null ? nrlSellBlock.PositionCoefficientStr_99 : null) : (reportData.HeSoPhanBo + (nrlSellBlock.PositionCoefficientStr_99 != null ? $"; {nrlSellBlock.PositionCoefficientStr_99}" : ""));
                                    reportData.DonGiaDat = reportData.DonGiaDat == null ? (nrlSellBlock.LandScapePrice_99 != null ? UtilsService.FormatMoney(nrlSellBlock.LandScapePrice_99) : null) : (reportData.DonGiaDat + (nrlSellBlock.LandScapePrice_99 != null ? $"; {UtilsService.FormatMoney(nrlSellBlock.LandScapePrice_99)}" : ""));
                                }
                                else if (blockDecreeMap.DecreeType1Id == DecreeEnum.ND_CP_61)
                                {
                                    reportData.ViTri = reportData.ViTri == null ? (nrlSellBlock.LandscapeLocationInAlley_61 != null ? $"V{(int)nrlSellBlock.LandscapeLocationInAlley_61}" : null) : (reportData.ViTri + (nrlSellBlock.LandscapeLocationInAlley_61 != null ? $"; V{(int)nrlSellBlock.LandscapeLocationInAlley_61}" : ""));
                                    reportData.HeSoPhanBo = reportData.HeSoPhanBo == null ? (nrlSellBlock.PositionCoefficientStr_61 != null ? nrlSellBlock.PositionCoefficientStr_61 : null) : (reportData.HeSoPhanBo + (nrlSellBlock.PositionCoefficientStr_61 != null ? $"; {nrlSellBlock.PositionCoefficientStr_61}" : ""));
                                    reportData.DonGiaDat = reportData.DonGiaDat == null ? (nrlSellBlock.LandScapePrice_61 != null ? UtilsService.FormatMoney(nrlSellBlock.LandScapePrice_61) : null) : (reportData.DonGiaDat + (nrlSellBlock.LandScapePrice_61 != null ? $"; {UtilsService.FormatMoney(nrlSellBlock.LandScapePrice_61)}" : ""));
                                }
                                else if (blockDecreeMap.DecreeType1Id == DecreeEnum.ND_CP_34_2013)
                                {
                                    if (nrlSellBlock.CaseApply_34 == TypeCaseApply_34.KHOAN_1)
                                    {
                                        reportData.ViTri = reportData.ViTri == null ? (nrlSellBlock.LandscapeLocationInAlley_34 != null ? $"V{(int)nrlSellBlock.LandscapeLocationInAlley_34}" : null) : (reportData.ViTri + (nrlSellBlock.LandscapeLocation_34 != null ? $"; V{(int)nrlSellBlock.LandscapeLocationInAlley_34}" : ""));
                                        reportData.HeSoPhanBo = reportData.HeSoPhanBo == null ? (nrlSellBlock.AlleyPositionCoefficientStr_34 != null ? nrlSellBlock.AlleyPositionCoefficientStr_34 : null) : (reportData.HeSoPhanBo + (nrlSellBlock.AlleyPositionCoefficientStr_34 != null ? $"; {nrlSellBlock.AlleyPositionCoefficientStr_34}" : ""));
                                        reportData.DonGiaDat = reportData.DonGiaDat == null ? (nrlSellBlock.AlleyLandScapePrice_34 != null ? UtilsService.FormatMoney(nrlSellBlock.AlleyLandScapePrice_34) : null) : (reportData.DonGiaDat + (nrlSellBlock.AlleyLandScapePrice_34 != null ? $"; {UtilsService.FormatMoney(nrlSellBlock.AlleyLandScapePrice_34)}" : ""));
                                    }
                                    else if (nrlSellBlock.CaseApply_34 == TypeCaseApply_34.KHOAN_2)
                                    {
                                        reportData.ViTri = reportData.ViTri == null ? (nrlSellBlock.LandscapeLocation_34 != null ? $"V{(int)nrlSellBlock.LandscapeLocation_34}" : null) : (reportData.ViTri + (nrlSellBlock.LandscapeLocation_34 != null ? $"; V{(int)nrlSellBlock.LandscapeLocation_34}" : ""));
                                        reportData.HeSoPhanBo = reportData.HeSoPhanBo == null ? (nrlSellBlock.PositionCoefficientStr_34 != null ? nrlSellBlock.PositionCoefficientStr_34 : null) : (reportData.HeSoPhanBo + (nrlSellBlock.PositionCoefficientStr_34 != null ? $"; {nrlSellBlock.PositionCoefficientStr_34}" : ""));
                                        reportData.DonGiaDat = reportData.DonGiaDat == null ? (nrlSellBlock.LandScapePrice_34 != null ? UtilsService.FormatMoney(nrlSellBlock.LandScapePrice_34) : null) : (reportData.DonGiaDat + (nrlSellBlock.LandScapePrice_34 != null ? $"; {UtilsService.FormatMoney(nrlSellBlock.LandScapePrice_34)}" : ""));
                                    }
                                }
                            }
                        }

                        //Tìm biên bảnh tính giá bán
                        Pricing pricing = pricings.Where(e => e.BlockId == nrlSellBlock.Id).OrderByDescending(x => x.Id).FirstOrDefault();
                        if (pricing != null)
                        {
                            #region Thông tin bán nhà
                            reportData.GiaBanNha = pricing.ApartmentPrice;
                            reportData.DienTichNha = nrlSellBlock.UseAreaValue;
                            reportData.GiaBanDat = pricing.LandPrice;
                            reportData.DienTichDat = nrlSellBlock.LandscapeAreaValue;
                            reportData.SoTienMienGiamNha = pricing.ApartmentPriceReduced;
                            reportData.SoTienMienGiamDat = pricing.LandPriceAfterReduced;
                            reportData.ThanhTien = pricing.TotalPrice;
                            reportData.TrangThai = null;                                                                                            //Chưa biết lấy ở đâu
                            reportData.SoQuyetDinhBanNha = null;                                                                                    //Chưa biết lấy ở đâu
                            reportData.NgayQuyetDinhBanNha = null;                                                                                  //Chưa biết lấy ở đâu
                            reportData.SoBienBanThanhLyHopDong = null;                                                                              //Chưa biết lấy ở đâu
                            reportData.NgayThanhLyHopDong = null;                                                                                   //Chưa biết lấy ở đâu
                            reportData.GhiChuThongTinBan = null;                                                                                    //Chưa biết lấy ở đâu
                            #endregion

                            reportData.ThoiDiemBoTriSuDung = pricing.TimeUse;
                            reportData.HasSaleInfo = true;
                        }
                    }

                    res.Add(reportData);
                }
            }

            if(sellApartments.Count > 0)
            {
                foreach (Apartment sellApartment in sellApartments.ToList())
                {
                    Block sellBlock = sellBlocks.Where(e => e.Id == sellApartment.BlockId).FirstOrDefault();
                    if (sellBlock != null)
                    {


                        SynthesisReportNocData reportData = new SynthesisReportNocData();
                        reportData.LoaiBienBan = getTypeReportApplyName(sellApartment.TypeReportApply);
                        reportData.SoTtCan = sellApartment.No;
                        reportData.SoTtHo = sellApartment.No;
                        reportData.MaDinhDanh = sellApartment.Code;
                        reportData.SoNha = sellApartment.Address + $"({sellApartment.Address})";
                        reportData.Duong = lanes.Where(e => e.Id == sellBlock.Lane).FirstOrDefault()?.Name;
                        reportData.Phuong = wards.Where(e => e.Id == sellBlock.Ward).FirstOrDefault()?.Name;
                        reportData.Quan = districts.Where(e => e.Id == sellBlock.District).FirstOrDefault()?.Name;
                        reportData.LoaiNha = typeBlocks.Where(e => e.Id == sellBlock.TypeBlockId).FirstOrDefault()?.Name;
                        reportData.DistrictId = sellBlock.District;
                        reportData.WardId = sellBlock.Ward;
                        reportData.LaneId = sellBlock.Lane;
                        reportData.LoaiNha = typeBlocks.Where(e => e.Id == sellBlock.TypeBlockId).FirstOrDefault()?.Name;

                        var levelBlockMap = levelBlockMaps.Where(e => e.BlockId == sellApartment.Id).ToList();
                        reportData.CapNha = levelBlockMap.Count > 0 ? getLevelBlock(levelBlockMap) : "";

                        //Tìm nhà bên bán tương ứng
                        if (sellBlock != null)
                        {
                            reportData.ViTri = null;
                            reportData.HeSoHem = sellBlock.Width;
                            reportData.HeSoPhanBo = null;
                            reportData.DonGiaDat = null;
                            reportData.ThueSuat = null;
                            reportData.TienThuePhaiNopTungNam = null;

                            List<DecreeMap> blockDecreeMaps = decreeMaps.Where(e => e.TargetId == sellBlock.Id).ToList();
                            if (blockDecreeMaps.Count > 0)
                            {
                                foreach (DecreeMap blockDecreeMap in blockDecreeMaps)
                                {
                                    if (blockDecreeMap.DecreeType1Id == DecreeEnum.ND_CP_99_2015)
                                    {
                                        reportData.ViTri = reportData.ViTri == null ? (sellBlock.LandscapeLocation_99 != null ? $"V{(int)sellBlock.LandscapeLocation_99}" : null) : (reportData.ViTri + (sellBlock.LandscapeLocation_99 != null ? $"; V{(int)sellBlock.LandscapeLocation_99}" : ""));
                                        reportData.HeSoPhanBo = reportData.HeSoPhanBo == null ? (sellBlock.PositionCoefficientStr_99 != null ? sellBlock.PositionCoefficientStr_99 : null) : (reportData.HeSoPhanBo + (sellBlock.PositionCoefficientStr_99 != null ? $"; {sellBlock.PositionCoefficientStr_99}" : ""));
                                        reportData.DonGiaDat = reportData.DonGiaDat == null ? (sellBlock.LandScapePrice_99 != null ? UtilsService.FormatMoney(sellBlock.LandScapePrice_99) : null) : (reportData.DonGiaDat + (sellBlock.LandScapePrice_99 != null ? $"; {UtilsService.FormatMoney(sellBlock.LandScapePrice_99)}" : ""));
                                    }
                                    else if (blockDecreeMap.DecreeType1Id == DecreeEnum.ND_CP_61)
                                    {
                                        reportData.ViTri = reportData.ViTri == null ? (sellBlock.LandscapeLocationInAlley_61 != null ? $"V{(int)sellBlock.LandscapeLocationInAlley_61}" : null) : (reportData.ViTri + (sellBlock.LandscapeLocationInAlley_61 != null ? $"; V{(int)sellBlock.LandscapeLocationInAlley_61}" : ""));
                                        reportData.HeSoPhanBo = reportData.HeSoPhanBo == null ? (sellBlock.PositionCoefficientStr_61 != null ? sellBlock.PositionCoefficientStr_61 : null) : (reportData.HeSoPhanBo + (sellBlock.PositionCoefficientStr_61 != null ? $"; {sellBlock.PositionCoefficientStr_61}" : ""));
                                        reportData.DonGiaDat = reportData.DonGiaDat == null ? (sellBlock.LandScapePrice_61 != null ? UtilsService.FormatMoney(sellBlock.LandScapePrice_61) : null) : (reportData.DonGiaDat + (sellBlock.LandScapePrice_61 != null ? $"; {UtilsService.FormatMoney(sellBlock.LandScapePrice_61)}" : ""));
                                    }
                                    else if (blockDecreeMap.DecreeType1Id == DecreeEnum.ND_CP_34_2013)
                                    {
                                        if (sellBlock.CaseApply_34 == TypeCaseApply_34.KHOAN_1)
                                        {
                                            reportData.ViTri = reportData.ViTri == null ? (sellBlock.LandscapeLocationInAlley_34 != null ? $"V{(int)sellBlock.LandscapeLocationInAlley_34}" : null) : (reportData.ViTri + (sellBlock.LandscapeLocation_34 != null ? $"; V{(int)sellBlock.LandscapeLocationInAlley_34}" : ""));
                                            reportData.HeSoPhanBo = reportData.HeSoPhanBo == null ? (sellBlock.AlleyPositionCoefficientStr_34 != null ? sellBlock.AlleyPositionCoefficientStr_34 : null) : (reportData.HeSoPhanBo + (sellBlock.AlleyPositionCoefficientStr_34 != null ? $"; {sellBlock.AlleyPositionCoefficientStr_34}" : ""));
                                            reportData.DonGiaDat = reportData.DonGiaDat == null ? (sellBlock.AlleyLandScapePrice_34 != null ? UtilsService.FormatMoney(sellBlock.AlleyLandScapePrice_34) : null) : (reportData.DonGiaDat + (sellBlock.AlleyLandScapePrice_34 != null ? $"; {UtilsService.FormatMoney(sellBlock.AlleyLandScapePrice_34)}" : ""));
                                        }
                                        else if (sellBlock.CaseApply_34 == TypeCaseApply_34.KHOAN_2)
                                        {
                                            reportData.ViTri = reportData.ViTri == null ? (sellBlock.LandscapeLocation_34 != null ? $"V{(int)sellBlock.LandscapeLocation_34}" : null) : (reportData.ViTri + (sellBlock.LandscapeLocation_34 != null ? $"; V{(int)sellBlock.LandscapeLocation_34}" : ""));
                                            reportData.HeSoPhanBo = reportData.HeSoPhanBo == null ? (sellBlock.PositionCoefficientStr_34 != null ? sellBlock.PositionCoefficientStr_34 : null) : (reportData.HeSoPhanBo + (sellBlock.PositionCoefficientStr_34 != null ? $"; {sellBlock.PositionCoefficientStr_34}" : ""));
                                            reportData.DonGiaDat = reportData.DonGiaDat == null ? (sellBlock.LandScapePrice_34 != null ? UtilsService.FormatMoney(sellBlock.LandScapePrice_34) : null) : (reportData.DonGiaDat + (sellBlock.LandScapePrice_34 != null ? $"; {UtilsService.FormatMoney(sellBlock.LandScapePrice_34)}" : ""));
                                        }
                                    }
                                }
                            }
                        }

                        if (sellApartment != null)
                        {
                            reportData.DienTichXayDung = sellApartment?.ConstructionAreaValue;
                            reportData.DienTichDatRieng = sellApartment?.LandscapeAreaValue2;
                            reportData.DienTichDatChung = sellApartment?.LandscapeAreaValue1;
                            reportData.DienTichDatChungPhanBo = sellApartment?.LandscapeAreaValue3;

                            reportData.DienTichSuDungRieng = sellApartment?.UseAreaValue2;
                            reportData.DienTichSuDungChung = sellApartment?.UseAreaValue1;
                            reportData.TongDienTichSuDungChung = sellApartment?.UseAreaValue1;
                            reportData.TongDienTichSuDung = sellApartment?.UseAreaValue;

                            //Tìm biên bảnh tính giá bán
                            Pricing pricing = pricings.Where(e => e.ApartmentId == sellApartment.Id).OrderByDescending(x => x.Id).FirstOrDefault();
                            if (pricing != null)
                            {
                                #region Thông tin bán nhà
                                reportData.GiaBanNha = pricing.ApartmentPrice;
                                reportData.DienTichNha = sellApartment.UseAreaValue;
                                reportData.GiaBanDat = pricing.LandPrice;
                                reportData.DienTichDat = sellApartment.LandscapeAreaValue;
                                reportData.SoTienMienGiamNha = pricing.ApartmentPriceReduced;
                                reportData.SoTienMienGiamDat = pricing.LandPriceAfterReduced;
                                reportData.ThanhTien = pricing.TotalPrice;
                                reportData.TrangThai = null;                                                                                            //Chưa biết lấy ở đâu
                                reportData.SoQuyetDinhBanNha = null;                                                                                    //Chưa biết lấy ở đâu
                                reportData.NgayQuyetDinhBanNha = null;                                                                                  //Chưa biết lấy ở đâu
                                reportData.SoBienBanThanhLyHopDong = null;                                                                              //Chưa biết lấy ở đâu
                                reportData.NgayThanhLyHopDong = null;                                                                                   //Chưa biết lấy ở đâu
                                reportData.GhiChuThongTinBan = null;                                                                                    //Chưa biết lấy ở đâu
                                #endregion

                                reportData.ThoiDiemBoTriSuDung = pricing.TimeUse;
                                reportData.HasSaleInfo = true;
                            }

                            sellApartments.Remove(sellApartment);
                        }

                        res.Add(reportData);
                    }
                }
            }

            return res;
        }

        private static string getLevelBlock(List<LevelBlockMap> levelBlockMaps)
        {
            string levelBlock = "";

            levelBlockMaps.ForEach(levelBlockMap =>
            {
                levelBlock = levelBlock == "" ? $"Cấp {levelBlockMap.LevelId}" : levelBlock + $" - {levelBlockMap.LevelId}";
            });

            return levelBlock;
        }

        private static string getUsageStatus(UsageStatus? usageStatus)
        {
            string name = "";
            switch (usageStatus)
            {
                case UsageStatus.DANG_CHO_THUE:
                    name = "Đang cho thuê";
                    break;
                case UsageStatus.NHA_TRONG:
                    name = "Nhà trống";
                    break;
                case UsageStatus.TRANH_CHAP:
                    name = "Tranh chấp";
                    break;
                case UsageStatus.CAC_TRUONG_HOP_KHAC:
                    name = "Các trường hợp khác";
                    break;
                default:
                    break;
            }

            return name;
        }

        private static string getTypeReportApplyName(TypeReportApply? typeReportApply)
        {
            string name = "";
            switch (typeReportApply)
            {
                case TypeReportApply.NHA_HO_CHUNG:
                    name = "Nhà hộ chung";
                    break;
                case TypeReportApply.NHA_RIENG_LE:
                    name = "Nhà riêng lẻ";
                    break;
                case TypeReportApply.NHA_CHUNG_CU:
                    name = "Nhà chung cư";
                    break;
                default:
                    break;
            }

            return name;
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

        #region Báo cáo khách hàng
        [HttpPost("GetCustomerNocReport")]
        public IActionResult GetCustomerNocReport([FromBody] CustomerNocReportReq req)
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

            if (!CheckRole.CheckRoleByCode(access_key, functionCode_CRN, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }

            try
            {
                List<Customer> customers = _context.Customers.Where(e => ((req.StartDate != null && e.CreatedAt >= req.StartDate) || (req.StartDate == null && 1 == 1))
                    && ((req.EndDate != null && e.CreatedAt <= req.EndDate) || (req.EndDate == null && 1 == 1))
                    && ((req.CustomerName != null && req.CustomerName != "" && e.FullName.Contains(req.CustomerName)) || ((req.CustomerName == null || req.CustomerName == "") && 1 == 1))
                    && ((req.Phone != null && req.Phone != "" && e.Phone.Contains(req.Phone)) || ((req.Phone == null || req.Phone == "") && 1 == 1))
                    && ((req.IdentityCode != null && req.IdentityCode != "" && e.Code.Contains(req.IdentityCode)) || ((req.IdentityCode == null || req.IdentityCode == "") && 1 == 1))
                    && ((req.Sex != null && e.Sex == req.Sex) || (req.Sex == null && 1 == 1))
                ).ToList();

                List<CustomerNocReportRes> res = _mapper.Map<List<CustomerNocReportRes>>(customers);
                List<RentFile> rentFiles = _context.RentFiles.Where(e => e.Type == 1 && e.Status != EntityStatus.DELETED).OrderByDescending(x => x.CreatedAt).ToList();

                foreach (CustomerNocReportRes resItem in res)
                {
                    RentFile rentFile = rentFiles.Where(e => e.CustomerId == resItem.Id).OrderByDescending(x => x.CreatedAt).FirstOrDefault();
                    if (rentFile != null)
                    {
                        resItem.ContractCode = rentFile.Code;
                        resItem.ContractDate = rentFile.DateHD;
                    }
                }

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = res;
                def.metadata = res.Count;
                return Ok(def);
            }
            catch (Exception ex)
            {
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                def.data = null;
                return Ok(def);
            }

        }
        #endregion

        #region Báo cáo thống kê Tình trạng sử dụng nhà
        [HttpPost("GetStatusBlockNocReport")]
        public IActionResult GetStatusBlockNocReport([FromBody] StatusBlockNocReportReq req)
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

            if (!CheckRole.CheckRoleByCode(access_key, functionCode_UBRN, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }

            try
            {
                List<Lane> lanes = _context.Lanies.Where(e => e.Status != EntityStatus.DELETED).ToList();
                List<Ward> wards = _context.Wards.Where(e => e.Status != EntityStatus.DELETED).ToList();
                List<District> districts = _context.Districts.Where(e => e.Status != EntityStatus.DELETED).ToList();

                List<StatusBlockNocReportRes> blocks = _context.Blocks.Where(e =>
                    e.TypeReportApply == TypeReportApply.NHA_RIENG_LE
                    && e.TypeBlockEntity == TypeBlockEntity.BLOCK_RENT
                    && e.Status != EntityStatus.DELETED
                    && ((req.Code != null && req.Code != "" && e.Code.Contains(req.Code)) || ((req.Code == null || req.Code == "") && 1 == 1))
                    && ((req.LaneId != null && e.Lane == req.LaneId) || (req.LaneId == null && 1 == 1))
                    && ((req.WardId != null && e.Ward == req.WardId) || (req.WardId == null && 1 == 1))
                    && ((req.DistrictId != null && e.District == req.DistrictId) || (req.DistrictId == null && 1 == 1))
                    && ((req.UsageStatus != null && e.UsageStatus == req.UsageStatus) || (req.UsageStatus == null && 1 == 1))
                ).Select(e => new StatusBlockNocReportRes
                {
                    Code = e.Code,
                    Address = e.Address,
                    Lane = getLaneName(lanes, e.Lane),
                    Ward = getWardName(wards, e.Ward),
                    District = getDistrictName(districts, e.District),
                    UsageStatus = e.UsageStatus == UsageStatus.DANG_CHO_THUE ? "Đang cho thuê" : (e.UsageStatus == UsageStatus.NHA_TRONG ? "Nhà trống" : (e.UsageStatus == UsageStatus.TRANH_CHAP ? "Tranh chấp" : (e.UsageStatus == UsageStatus.CAC_TRUONG_HOP_KHAC ? "Các trường hợp khác" : ""))),
                    CreatedAt = e.CreatedAt
                }).ToList();

                List<StatusBlockNocReportRes> apartments = (from b in _context.Blocks
                                                            join a in _context.Apartments on b.Id equals a.BlockId
                                                            where b.Status != EntityStatus.DELETED
                                                                && a.Status != EntityStatus.DELETED
                                                                && b.TypeBlockEntity == TypeBlockEntity.BLOCK_RENT
                                                                && b.TypeReportApply != TypeReportApply.NHA_RIENG_LE
                                                                && a.TypeApartmentEntity == TypeApartmentEntity.APARTMENT_RENT
                                                                && a.TypeReportApply != TypeReportApply.NHA_RIENG_LE
                                                                && ((req.Code != null && req.Code != "" && a.Code.Contains(req.Code)) || ((req.Code == null || req.Code == "") && 1 == 1))
                                                                && ((req.LaneId != null && b.Lane == req.LaneId) || (req.LaneId == null && 1 == 1))
                                                                && ((req.WardId != null && b.Ward == req.WardId) || (req.WardId == null && 1 == 1))
                                                                && ((req.DistrictId != null && b.District == req.DistrictId) || (req.DistrictId == null && 1 == 1))
                                                                && ((req.UsageStatus != null && a.UsageStatus == req.UsageStatus) || (req.UsageStatus == null && 1 == 1))
                                                            select new StatusBlockNocReportRes
                                                            {
                                                                Code = a.Code,
                                                                Address = a.Address,
                                                                Lane = getLaneName(lanes, b.Lane),
                                                                Ward = getWardName(wards, b.Ward),
                                                                District = getDistrictName(districts, b.District),
                                                                UsageStatus = a.UsageStatus == UsageStatus.DANG_CHO_THUE ? "Đang cho thuê" : (a.UsageStatus == UsageStatus.NHA_TRONG ? "Nhà trống" : (a.UsageStatus == UsageStatus.TRANH_CHAP ? "Tranh chấp" : (a.UsageStatus == UsageStatus.CAC_TRUONG_HOP_KHAC ? "Các trường hợp khác" : ""))),
                                                                CreatedAt = a.CreatedAt
                                                            }).ToList();

                List<StatusBlockNocReportRes> res = blocks.Concat(apartments).OrderByDescending(x => x.CreatedAt).ToList();

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = res;
                def.metadata = res.Count;
                return Ok(def);
            }
            catch (Exception ex)
            {
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                def.data = null;
                return Ok(def);
            }

        }

        public static string getLaneName(List<Lane> lanes, int? id)
        {
            Lane lane = lanes.Where(e => e.Id == id).FirstOrDefault();
            return lane != null ? lane.Name : "";
        }

        public static string getWardName(List<Ward> wards, int? id)
        {
            Ward ward = wards.Where(e => e.Id == id).FirstOrDefault();
            return ward != null ? ward.Name : "";
        }

        public static string getDistrictName(List<District> districts, int? id)
        {
            District district = districts.Where(e => e.Id == id).FirstOrDefault();
            return district != null ? district.Name : "";
        }
        #endregion

        #region BÁO CÁO TỔNG HỢP CÔNG NỢ KHÁCH HÀNG
        [HttpPost("getDataExport")]
        public async Task<IActionResult> getDataExport(ExportRentReq req)
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

            if (!CheckRole.CheckRoleByCode(access_key, functionCode_THCN, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }

            try
            {
                var laneData = _context.Lanies.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                var warData = _context.Wards.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                var districtData = _context.Districts.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                var apm = _context.Apartments.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                var bldata = _context.Blocks.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();

                var data = (from i in _context.RentFiles
                                join p in _context.debts on i.CodeCN != null ? i.CodeCN : i.CodeCH equals p.Code
                                join b in _context.Blocks on i.RentBlockId equals b.Id
                                where ((req.FromDate != null && i.DateHD >= req.FromDate) || (req.FromDate == null && 1 == 1))
                       && ((req.ToDate != null && i.DateHD <= req.ToDate) || (req.ToDate == null && 1 == 1))
                       && ((req.CustomerName != null && i.CustomerName == req.CustomerName) || (req.CustomerName == null && 1 == 1))
                       && ((req.LaneId != null && i.LaneId == req.LaneId) || (req.LaneId == null && 1 == 1))
                       && ((req.WardId != null && i.WardId == req.WardId) || (req.WardId == null && 1 == 1))
                       && ((req.DistrictId != null && i.DistrictId == req.DistrictId) || (req.DistrictId == null && 1 == 1))
                       && ((req.CodeHS != null && i.CodeHS == req.CodeHS) || (req.CodeHS == null && 1 == 1))
                       && ((req.Code != null && i.CodeCH != null && i.CodeCH == req.Code) || (req.Code == null && 1 == 1))
                       && ((req.Code != null && i.CodeCN != null && i.CodeCN == req.Code) || (req.Code == null && 1 == 1))
                       && i.Status != AppEnums.EntityStatus.DELETED
                                select new ExportRentRes
                                {
                                    Id = i.Id,
                                    Code = i.CodeCH != null ? i.CodeCH : i.CodeCN,
                                    Lane = getLaneName(laneData, b.Lane),
                                    Ward = getWardName(warData, b.Ward),
                                    District = getDistrictName(districtData, b.District),
                                    Provice = "Thành phố Hồ Chí Minh",
                                    CodeHS = i.Code,
                                    DateAssgint = i.DateHD,
                                    Paid = p.Paid != null ? (decimal)p.Paid : 0,
                                    Total = p.Total != null ? (decimal)p.Total : 0,
                                    Diff = p.Diff != null ? (decimal)p.Diff : 0,
                                    FullAddress = i.fullAddressCH != null ? i.fullAddressCH : i.fullAddressCN,
                                    CustomerName = p.CustomerName,
                                }).ToList();
                data.ForEach(item =>
                {
                    string[] parts = item.FullAddress.Split(',');
                    item.Address = parts[0];
                });

                var res = data.GroupBy(item => new { item.Code }).Select(group => group.OrderByDescending(item => item.DateAssgint).First()).ToList();

                def.data = res;
                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                return Ok(def);
            }
            catch (Exception ex)
            {
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        //public async Task<IActionResult> Export([FromBody] List<ExportRentRes> data)
        //{
        //    string accessToken = Request.Headers[HeaderNames.Authorization];
        //    Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
        //    if (token == null)
        //    {
        //        return Unauthorized();
        //    }

        //    DefaultResponse def = new DefaultResponse();

        //    var identity = (ClaimsIdentity)User.Identity;
        //    int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id").Select(c => c.Value).SingleOrDefault());
        //    string fullName = identity.Claims.Where(c => c.Type == "FullName").Select(c => c.Value).SingleOrDefault() ?? string.Empty;
        //    string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();

        //    if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.EXPORT))
        //    {
        //        def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
        //        return Ok(def);
        //    }

        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
        //            return Ok(def);
        //        }

        //        // khởi tạo wb rỗng
        //        XSSFWorkbook wb = new XSSFWorkbook();
        //        // Tạo ra 1 sheet
        //        ISheet sheet = wb.CreateSheet();

        //        string template = @"NOCexcel/hoa_don_tien_thue_nha_cu.xlsx";
        //        string webRootPath = _hostingEnvironment.WebRootPath;
        //        string templatePath = Path.Combine(webRootPath, template);

        //        MemoryStream ms = writeDataToExcel(templatePath, 0, data);
        //        byte[] byteArrayContent = ms.ToArray();
        //        return new FileContentResult(byteArrayContent, "application/octet-stream");
        //    }
        //    catch (Exception e)
        //    {
        //        log.Error("GetPromissoryReport Exception:" + e);
        //        def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
        //        return Ok(def);
        //    }
        //}

        //private static MemoryStream writeDataToExcel(string templatePath, int sheetnumber, List<ExportRentRes> data)
        //{
        //    FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
        //    XSSFWorkbook workbook = new XSSFWorkbook(file);
        //    ISheet sheet = workbook.GetSheetAt(sheetnumber);
        //    int rowStart = 5;

        //    if (sheet != null)
        //    {
        //        int datacol = 13;
        //        try
        //        {
        //            //style body
        //            List<ICellStyle> rowStyle = new List<ICellStyle>();
        //            for (int i = 0; i < datacol; i++)
        //            {
        //                rowStyle.Add(sheet.GetRow(rowStart).GetCell(i).CellStyle);
        //            }

        //            //Thêm row
        //            int k = 0;
        //            double price = 0;
        //            double amountNoTax = 0;
        //            double amountTax = 0;

        //            foreach (var item in data)
        //            {
        //                try
        //                {
        //                    XSSFRow row = (XSSFRow)sheet.CreateRow(rowStart);
        //                    for (int i = 0; i < datacol; i++)
        //                    {
        //                        row.CreateCell(i).CellStyle = rowStyle[i];

        //                        if (i == 0)
        //                        {
        //                            row.GetCell(i).SetCellValue(k + 1);
        //                        }
        //                        else if (i == 1)
        //                        {
        //                            row.GetCell(i).SetCellValue(item.Address != null ? item.Address : "");
        //                        }
        //                        else if (i == 2)
        //                        {
        //                            row.GetCell(i).SetCellValue(item.Lane != null ? item.Lane : "");
        //                        }
        //                        else if (i == 3)
        //                        {
        //                            row.GetCell(i).SetCellValue(item.Ward != null ? item.Ward : "");
        //                        }
        //                        else if (i == 4)
        //                        {
        //                            row.GetCell(i).SetCellValue(item.District != null ? item.District : "");
        //                        }
        //                        else if (i == 5)
        //                        {
        //                            row.GetCell(i).SetCellValue(item.provice != null ? item.provice : "");
        //                        }
        //                        else if (i == 6)
        //                        {
        //                            row.GetCell(i).SetCellValue(item.codeHS != null ? item.codeHS : "");
        //                        }
        //                        else if (i == 7)
        //                        {
        //                            row.GetCell(i).SetCellValue(item.dateAssgint != null ? item.dateAssgint.ToString("dd/MM/yyyy") : "");
        //                        }
        //                        else if (i == 8)
        //                        {
        //                            row.GetCell(i).SetCellValue(item.CustomerName != null ? item.CustomerName : "");
        //                        }
        //                        else if (i == 9)
        //                        {
        //                            row.GetCell(i).SetCellValue((double)item.Total);
        //                        }
        //                        else if (i == 10)
        //                        {
        //                            row.GetCell(i).SetCellValue((double)item.paid);
        //                        }
        //                        else if (i == 11)
        //                        {
        //                            row.GetCell(i).SetCellValue((double)item.diff);
        //                        }
        //                        else if (i == 12)
        //                        {
        //                            row.GetCell(i).SetCellValue("");
        //                        }
        //                    }

        //                    rowStart++;
        //                    k++;
        //                }
        //                catch (Exception ex)
        //                {
        //                    log.Error("WriteDataToExcel:" + ex);
        //                }
        //            }

        //            XSSFRow rowSum = (XSSFRow)sheet.CreateRow(rowStart);
        //            for (int i = 0; i < datacol; i++)
        //            {
        //                rowSum.CreateCell(i).CellStyle = rowStyle[i];
        //            }

        //            CellRangeAddress cellRangeAddress = new CellRangeAddress(rowStart, rowStart, 0, 8);
        //            sheet.AddMergedRegion(cellRangeAddress);
        //            rowSum.GetCell(0).SetCellValue("TỔNG CỘNG");
        //            rowSum.GetCell(9).SetCellValue(price);
        //            rowSum.GetCell(10).SetCellValue(amountNoTax);
        //            rowSum.GetCell(11).SetCellValue(amountTax);
        //        }
        //        catch (Exception ex)
        //        {
        //            log.Error("WriteDataToExcel:" + ex);
        //        }
        //    }

        //    sheet.ForceFormulaRecalculation = true;

        //    MemoryStream ms = new MemoryStream();

        //    workbook.Write(ms);

        //    return ms;
        //}
        #endregion


        #region BÁO CÁO NỢ  QUÁ HẠN

        [HttpPost("getDataExport2")]
        public async Task<IActionResult> getDataExport2(ExportRentReq req)
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

            if (!CheckRole.CheckRoleByCode(access_key, functionCode_NQH, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }

            try
            {
                var laneData = _context.Lanies.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                var warData = _context.Wards.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                var districtData = _context.Districts.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                var debtsTableData = _context.DebtsTables.Where(l => l.Status != AppEnums.EntityStatus.DELETED && l.DateEnd < DateTime.Now).ToList();
                var apm = _context.Apartments.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                var bldata = _context.Blocks.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                var data = (from i in _context.RentFiles
                            join p in _context.DebtsTables on i.Id equals p.RentFileId
                            join b in _context.Blocks on i.RentBlockId equals b.Id
                            where ((req.FromDate != null && i.DateHD >= req.FromDate) || (req.FromDate == null && 1 == 1))
                   && ((req.ToDate != null && i.DateHD <= req.ToDate) || (req.ToDate == null && 1 == 1))
                   && ((req.CustomerName != null && i.CustomerName == req.CustomerName) || (req.CustomerName == null && 1 == 1))
                   && ((req.LaneId != null && i.LaneId == req.LaneId) || (req.LaneId == null && 1 == 1))
                   && ((req.WardId != null && i.WardId == req.WardId) || (req.WardId == null && 1 == 1))
                   && ((req.DistrictId != null && i.DistrictId == req.DistrictId) || (req.DistrictId == null && 1 == 1))
                   && ((req.CodeHS != null && i.CodeHS == req.CodeHS) || (req.CodeHS == null && 1 == 1))
                   && ((req.Code != null && i.CodeCH != null && i.CodeCH == req.Code) || (req.Code == null && 1 == 1))
                   && ((req.Code != null && i.CodeCN != null && i.CodeCN == req.Code) || (req.Code == null && 1 == 1))
                   && i.Status != AppEnums.EntityStatus.DELETED && p.DateEnd < DateTime.Now && p.Check == false
                            select new ExportRentRes
                            {
                                Id = i.Id,
                                Code = i.CodeCH != null ? i.CodeCH : i.CodeCN,
                                Lane = getLaneName(laneData, b.Lane),
                                Ward = getWardName(warData, b.Ward),
                                District = getDistrictName(districtData, b.District),
                                Provice = "Thành phố Hồ Chí Minh",
                                CodeHS = i.Code,
                                DateAssgint = i.DateHD,
                                CustomerName = i.CustomerName,
                                PaymentDeadline = p.DateEnd != null ? p.DateEnd : null,
                                //Total = getTotalDebts(debtsTableData, i.Id),
                                //DateDiff = UtilsService.DateDiff((DateTime)p.DateEnd, DateTime.Now),
                                //NearDatePay = returnDatePay(debtsTableData, i.Id),
                                LandId = (int)b.Lane,
                                WardId = b.Ward,
                                DistrictId = b.District,
                                DateEnd = (DateTime)p.DateEnd,
                               FullAddress = i.fullAddressCH != null ? i.fullAddressCH : i.fullAddressCN,
                            }).ToList();

                data.ForEach(item =>
                {
                    List<DebtsTable> data = debtsTableData.Where(l => l.RentFileId == item.Id).ToList();
                    string[] parts = item.FullAddress.Split(',');
                    item.Address = parts[0];
                    item.Total = getTotalDebts(data, item.Id);
                    item.DateDiff = UtilsService.DateDiff(item.DateEnd, DateTime.Now);
                    int count = data.Count(obj => obj.Check == true);
                    if(count > 0)
                    {
                        item.NearDatePay = returnDatePay(data, item.Id);

                    }
                });

                var res = data.GroupBy(item => new { item.Id }).Select(group => group.OrderByDescending(item => item.PaymentDeadline).First()).ToList();

                def.data = res;
                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                return Ok(def);
            }
            catch (Exception ex)
            {
                def.meta = new Meta(500, ex.ToString());
                return Ok(def);
            }
        }
        #endregion

        #region BÁO CÁO NỢ SẮP ĐẾN HẠN NỘP

        [HttpPost("getDataExport3")]
        public async Task<IActionResult> getDataExport3(ExportRentReq req)
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

            if (!CheckRole.CheckRoleByCode(access_key, functionCode_SDHN, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }

            try
            {
                var laneData = _context.Lanies.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                var warData = _context.Wards.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                var districtData = _context.Districts.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                var apm = _context.Apartments.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                var bldata = _context.Blocks.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();

                var data = (from i in _context.RentFiles
                            join p in _context.DebtsTables on i.Id equals p.RentFileId
                            join b in _context.Blocks on i.RentBlockId equals b.Id
                            where ((req.FromDate != null && i.DateHD >= req.FromDate) || (req.FromDate == null && 1 == 1))
                   && ((req.ToDate != null && i.DateHD <= req.ToDate) || (req.ToDate == null && 1 == 1))
                   && ((req.CustomerName != null && i.CustomerName == req.CustomerName) || (req.CustomerName == null && 1 == 1))
                   && ((req.LaneId != null && b.Lane == req.LaneId) || (req.LaneId == null && 1 == 1))
                   && ((req.WardId != null && b.Ward == req.WardId) || (req.WardId == null && 1 == 1))
                   && ((req.DistrictId != null && b.District == req.DistrictId) || (req.DistrictId == null && 1 == 1))
                   && ((req.CodeHS != null && i.CodeHS == req.CodeHS) || (req.CodeHS == null && 1 == 1))
                   && ((req.Code != null && i.CodeCH != null && i.CodeCH == req.Code) || (req.Code == null && 1 == 1))
                   && ((req.Code != null && i.CodeCN != null && i.CodeCN == req.Code) || (req.Code == null && 1 == 1))
                   && i.Status != AppEnums.EntityStatus.DELETED && p.DateEnd > DateTime.Now && p.Status != AppEnums.EntityStatus.DELETED && p.Check == false
                            select new ExportRentRes
                            {
                                Id = i.Id,
                                Code = i.CodeCH != null ? i.CodeCH : i.CodeCN,
                                Lane = getLaneName(laneData, b.Lane),
                                Ward = getWardName(warData, b.Ward),
                                District = getDistrictName(districtData, b.District),
                                Provice = "Thành phố Hồ Chí Minh",
                                CodeHS = i.Code,
                                DateAssgint = i.DateHD,
                                FullAddress = i.fullAddressCH != null ? i.fullAddressCH : i.fullAddressCN,
                                CustomerName = i.CustomerName,
                                PaymentDeadline = p.DateEnd != null ? p.DateEnd : null,
                                Total = p.Price
                            }).ToList();

                data.ForEach(item =>
                {
                    string[] parts = item.FullAddress.Split(',');
                    item.Address = parts[0];
                });

                var res = data.GroupBy(item => new { item.Id }).Select(group => group.OrderBy(item => item.PaymentDeadline).First()).ToList();

                def.data = res;
                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                return Ok(def);
            }
            catch (Exception ex)
            {
                def.meta = new Meta(500, ex.ToString());
                return Ok(def);
            }
        }

        #endregion
        public static decimal getTotalDebts(List<DebtsTable> resDebtsTable, Guid? id)
        {
            var data = resDebtsTable.Where(l => l.RentFileId == id).ToList();
            decimal total = data.Sum(p => p.Price);
            return total;
        }

        public static DateTime? returnDatePay(List<DebtsTable> resDebtsTable, Guid? id)
        {
            DateTime ? returnDate = resDebtsTable.Where(l => l.Check == true && l.RentFileId == id).OrderByDescending(p => p.Date).Select(p => p.Date).FirstOrDefault();
            
            return returnDate;
        }
    }
}
