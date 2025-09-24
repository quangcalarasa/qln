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

namespace IOITQln.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GeneralDataImportController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("GeneralDataImport", "GeneralDataImport");
        private static string functionCode = "GENERAL_DATA_IMPORT";
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;

        public GeneralDataImportController(ApiDbContext context, IMapper mapper, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
        }

        #region import danh sách văn bản pháp luật liên quân từ excel

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
                importHistory.Type = AppEnums.ImportHistoryType.Noc_GeneralDataImport;

                ResGeneralDataImport resGeneralDataImport = new ResGeneralDataImport();
                List<GeneralDataImport> data = new List<GeneralDataImport>();

                //Lấy dữ liệu từ file excel và lưu lại file
                foreach (var file in httpRequest)
                {
                    var postedFile = httpRequest[i];
                    if (postedFile != null && postedFile.Length > 0)
                    {
                        IList<string> AllowedDocuments = new List<string> { ".xls", ".xlsx" };
                        int MaxContentLength = 1024 * 1024 * 100; //Size = 100 MB  

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
                            var message = string.Format("Vui lòng Up file có dung lượng nhỏ hơn 100 MB!");
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
                            List<SimpleLaneData> laneData = _context.Lanies.Where(e => e.Province == 2 && e.Status != EntityStatus.DELETED).Select(e => new SimpleLaneData { Id = e.Id, Name = e.Name, Ward = e.Ward }).ToList();
                            List<SimpleData> wardData = _context.Wards.Where(e => e.ProvinceId == 2 && e.Status != EntityStatus.DELETED).Select(e => new SimpleData { Id = e.Id, Name = e.Name }).ToList();
                            List<District> districtData = _context.Districts.Where(e => e.ProvinceId == 2 && e.Status != EntityStatus.DELETED).ToList();
                            List<SimpleData> typeBlockData = _context.TypeBlocks.Where(e => e.Status != EntityStatus.DELETED).Select(e => new SimpleData { Id = e.Id, Name = e.Name }).ToList();
                            List<Block> blockData = _context.Blocks.Where(e => e.Status != EntityStatus.DELETED).ToList();
                            List<Apartment> apartmentData = _context.Apartments.Where(e => e.Status != EntityStatus.DELETED).ToList();
                            List<SimpleData> customerData = _context.Customers.Where(c => c.Status != EntityStatus.DELETED).Select(e => new SimpleData { Id = e.Id, Name = e.Code }).ToList();

                            using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                            {
                                fileData = binaryReader.ReadBytes((int)file.Length);
                                using (MemoryStream ms = new MemoryStream(fileData))
                                {
                                    resGeneralDataImport = importData(ms, 0, 4, laneData, wardData, districtData, typeBlockData, blockData, apartmentData, customerData);
                                    data = resGeneralDataImport.data;
                                }
                            }
                        }
                    }
                    i++;
                }

                List<GeneralDataImport> dataValid = data.Where(e => e.Valid == true).ToList();

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        importHistory.Data = data.Cast<dynamic>().ToList();
                        importHistory.CreatedById = userId;
                        importHistory.CreatedBy = fullName;

                        GeneralDataImportCount generalDataImportCount = new GeneralDataImportCount(0,0,0,0);
                        _context.UpdateRange(resGeneralDataImport.blocks);
                        _context.UpdateRange(resGeneralDataImport.apartments);
                        _context.SaveChanges();

                        //group theo Số nhà và tên đường để ra ds căn nhà
                        var groupDataValid = dataValid.GroupBy(x => new { x.SoNha, x.DuongApDung }).ToList();
                        groupDataValid.ForEach(groupDataValidItem => {
                            var key = groupDataValidItem.Key;
                            var firstItem = groupDataValidItem.FirstOrDefault();

                            if(firstItem.LoaiBienBanApDung == TypeReportApply.NHA_RIENG_LE)
                            {
                                //Kiểm tra xem bên thuê và bán bên nào thiếu thì thêm mới liên kết dữ liệu giữa 2 bên
                                if(firstItem.ExistInRent != true)
                                {
                                    //Thêm căn hộ thuê
                                    Block blockRent = new Block();
                                    blockRent.CreatedById = userId;
                                    blockRent.CreatedBy = $"{fullName} (Excel)";
                                    blockRent.TypeBlockEntity = TypeBlockEntity.BLOCK_RENT;
                                    blockRent.TypeReportApply = (TypeReportApply)firstItem.LoaiBienBanApDung;
                                    blockRent.TypeBlockId = firstItem.LoaiNhaApDung ?? 0;
                                    blockRent.No = firstItem.SoThuTuCan;
                                    blockRent.Code = firstItem.MaDinhDanh;
                                    blockRent.Address = firstItem.SoNha;
                                    blockRent.Lane = firstItem.DuongApDung;
                                    blockRent.Ward = (int)firstItem.PhuongApDung;
                                    blockRent.District = (int)firstItem.QuanApDung;
                                    blockRent.Province = (int)firstItem.TinhApDung;
                                    blockRent.CampusArea = firstItem.DienTichKhuonVien;
                                    blockRent.ConstructionAreaValue = firstItem.DienTichDatRieng != null ? (float)firstItem.DienTichDatRieng : 0;
                                    blockRent.UseAreaValue = firstItem.DienTichSuDungRieng != null ? (float)firstItem.DienTichSuDungRieng : 0;
                                    blockRent.UseAreaNote1 = firstItem.GhiChu;
                                    blockRent.UsageStatus = firstItem.TinhTrangNhaApDung;

                                    if (firstItem.SoQuyetDinh != null || firstItem.NgayQuyetDinh != null)
                                    {
                                        blockRent.EstablishStateOwnership = true;
                                        blockRent.CodeEstablishStateOwnership = firstItem.SoQuyetDinh;
                                        blockRent.DateEstablishStateOwnership = firstItem.NgayQuyetDinh;
                                    }

                                    if(firstItem.TenBanVe != null || firstItem.NgayLapBanVe != null)
                                    {
                                        blockRent.Blueprint = true;
                                        blockRent.NameBlueprint = firstItem.TenBanVe;
                                        blockRent.DateBlueprint = firstItem.NgayLapBanVe;
                                    }

                                    if(firstItem.NguoiDaiDien != null || firstItem.CanCuoc != null)
                                    {
                                        if(firstItem.ExistCanCuoc != true)
                                        {
                                            Customer customer = new Customer();
                                            customer.CreatedById = userId;
                                            customer.CreatedBy = $"{fullName} (Excel)";
                                            customer.FullName = firstItem.NguoiDaiDien ?? " ";
                                            customer.Code = firstItem.CanCuoc;
                                            customer.PlaceCode = firstItem.NoiCap;
                                            customer.Doc = firstItem.NgayCap;
                                            customer.Address = firstItem.DiaChiThuongTru;
                                            customer.Phone = firstItem.SoDienThoai;
                                            _context.Customers.Add(customer);
                                            _context.SaveChanges();

                                            blockRent.CustomerId = customer.Id;
                                        }
                                        else
                                        {
                                            blockRent.CustomerId = firstItem.ExistCanCuocId;
                                        }

                                        if (firstItem.ThanhVien != null)
                                        {
                                            var spl = firstItem.ThanhVien.Replace(";", "+").Replace("|", "+").Split("+").Select(e => new Customer {
                                                CreatedById = userId,
                                                CreatedBy = $"{fullName} (Excel)",
                                                FullName = e
                                            }).ToList();

                                            _context.AddRange(spl);
                                            _context.SaveChanges();
                                        }
                                    }

                                    _context.Blocks.Add(blockRent);
                                    _context.SaveChanges();
                                    generalDataImportCount.TotalBlockRent += 1;

                                    //Cấp nhà-Hạng nhà
                                    if (firstItem.CapNha != null)
                                    {
                                        var splLevelBlockMap = firstItem.CapNha.Replace(";", "+").Replace("|", "+").Split("+").Select(e => new LevelBlockMap
                                        {
                                            CreatedById = userId,
                                            CreatedBy = $"{fullName} (Excel)",
                                            LevelId = int.Parse(e),
                                            BlockId = blockRent.Id
                                        }).ToList();

                                        _context.AddRange(splLevelBlockMap);
                                        _context.SaveChanges();
                                    }

                                    if (firstItem.ExistInNormal != true)
                                    {
                                        Block blockNormal = blockRent;
                                        blockNormal.Id = 0;
                                        blockNormal.TypeBlockEntity = TypeBlockEntity.BLOCK_NORMAL;
                                        blockNormal.ConstructionAreaValue = firstItem.DienTichXayDung != null ? (float)firstItem.DienTichXayDung : 0;

                                        _context.Blocks.Add(blockNormal);
                                        _context.SaveChanges();
                                        generalDataImportCount.TotalBlockNormal += 1;

                                        //Cấp nhà-Hạng nhà
                                        if (firstItem.CapNha != null)
                                        {
                                            var splLevelBlockMap = firstItem.CapNha.Replace(";", "+").Replace("|", "+").Split("+").Select(e => new LevelBlockMap
                                            {
                                                CreatedById = userId,
                                                CreatedBy = $"{fullName} (Excel)",
                                                LevelId = int.Parse(e),
                                                BlockId = blockNormal.Id
                                            }).ToList();

                                            _context.AddRange(splLevelBlockMap);
                                            _context.SaveChanges();
                                        }
                                    }
                                    //else
                                    //{
                                    //    Block blockNormal = _context.Blocks.Find(firstItem.ExistInNormalId);
                                    //    if(blockNormal != null)
                                    //    {
                                    //        blockNormal.ParentId = blockRent.Id;
                                    //        _context.Update(blockNormal);
                                    //        _context.SaveChanges();
                                    //    }
                                    //}
                                }
                                else
                                {
                                    //Đã tồn tại căn nhà thuê
                                    Block blockRent = _context.Blocks.Find(firstItem.ExistInRentId);
                                    if(blockRent != null)
                                    {
                                        //Kiểm tra xem có căn hộ bán tương ứng hay không - không có thì thêm mới
                                        Block blockNormal = _context.Blocks.Where(e => e.Code == blockRent.Code && e.TypeBlockEntity == TypeBlockEntity.BLOCK_NORMAL && e.Status != EntityStatus.DELETED).FirstOrDefault();
                                        if(blockNormal == null)
                                        {
                                            blockNormal = blockRent;
                                            blockNormal.Id = 0;
                                            blockNormal.TypeBlockEntity = TypeBlockEntity.BLOCK_NORMAL;
                                            blockNormal.ConstructionAreaValue = firstItem.DienTichXayDung != null ? (float)firstItem.DienTichXayDung : 0;

                                            _context.Blocks.Add(blockNormal);
                                            _context.SaveChanges();
                                            generalDataImportCount.TotalBlockNormal += 1;

                                            //Cấp nhà-Hạng nhà
                                            if (firstItem.CapNha != null)
                                            {
                                                var splLevelBlockMap = firstItem.CapNha.Replace(";", "+").Replace("|", "+").Split("+").Select(e => new LevelBlockMap
                                                {
                                                    CreatedById = userId,
                                                    CreatedBy = $"{fullName} (Excel)",
                                                    LevelId = int.Parse(e),
                                                    BlockId = blockNormal.Id
                                                }).ToList();

                                                _context.AddRange(splLevelBlockMap);
                                                _context.SaveChanges();
                                            }
                                        }
                                    }
                                }
                            }
                            //Nhà chung cư và nhà hộ chung
                            else
                            {
                                //Kiểm tra có căn nhà thuê theo Số nhà và tên đường chưa - chưa có thì thêm mới
                                Block blockRent = _context.Blocks.Where(b => b.Address == key.SoNha && b.Lane == key.DuongApDung && b.TypeBlockEntity == TypeBlockEntity.BLOCK_RENT && b.Status != EntityStatus.DELETED).FirstOrDefault();
                                if (blockRent == null)
                                {
                                    blockRent = new Block();
                                    blockRent.CreatedById = userId;
                                    blockRent.CreatedBy = $"{fullName} (Excel)";
                                    blockRent.TypeBlockEntity = TypeBlockEntity.BLOCK_RENT;
                                    blockRent.TypeReportApply = (TypeReportApply)firstItem.LoaiBienBanApDung;
                                    blockRent.TypeBlockId = firstItem.LoaiNhaApDung ?? 0;
                                    blockRent.No = firstItem.SoThuTuCan;
                                    blockRent.Address = firstItem.SoNha;
                                    blockRent.Lane = firstItem.DuongApDung;
                                    blockRent.Ward = (int)firstItem.PhuongApDung;
                                    blockRent.District = (int)firstItem.QuanApDung;
                                    blockRent.Province = (int)firstItem.TinhApDung;
                                    blockRent.CampusArea = firstItem.DienTichKhuonVien;
                                    _context.Blocks.Add(blockRent);
                                    _context.SaveChanges();
                                    generalDataImportCount.TotalBlockRent += 1;

                                    //Cấp nhà-Hạng nhà
                                    if (firstItem.CapNha != null)
                                    {
                                        var splLevelBlockMap = firstItem.CapNha.Replace(";", "+").Replace("|", "+").Split("+").Select(e => new LevelBlockMap
                                        {
                                            CreatedById = userId,
                                            CreatedBy = $"{fullName} (Excel)",
                                            LevelId = int.Parse(e),
                                            BlockId = blockRent.Id
                                        }).ToList();

                                        _context.AddRange(splLevelBlockMap);
                                        _context.SaveChanges();
                                    }
                                }

                                int blockRentId = blockRent.Id;

                                //Kiểm tra căn nhà bán
                                Block blockNormal = _context.Blocks.Where(b => b.Address == key.SoNha && b.Lane == key.DuongApDung && b.TypeBlockEntity == TypeBlockEntity.BLOCK_NORMAL && b.Status != EntityStatus.DELETED).FirstOrDefault();
                                if(blockNormal == null)
                                {
                                    blockNormal = blockRent;
                                    blockNormal.Id = 0;
                                    blockNormal.TypeBlockEntity = TypeBlockEntity.BLOCK_NORMAL;
                                    _context.Blocks.Add(blockNormal);
                                    _context.SaveChanges();
                                    generalDataImportCount.TotalBlockNormal += 1;

                                    //Cấp nhà-Hạng nhà
                                    if (firstItem.CapNha != null)
                                    {
                                        var splLevelBlockMap = firstItem.CapNha.Replace(";", "+").Replace("|", "+").Split("+").Select(e => new LevelBlockMap
                                        {
                                            CreatedById = userId,
                                            CreatedBy = $"{fullName} (Excel)",
                                            LevelId = int.Parse(e),
                                            BlockId = blockNormal.Id
                                        }).ToList();

                                        _context.AddRange(splLevelBlockMap);
                                        _context.SaveChanges();
                                    }
                                }

                                groupDataValidItem.ToList().ForEach(dataValidItem => {
                                    if (dataValidItem.ExistInRent != true)
                                    {
                                        //Thêm căn hộ thuê
                                        Apartment apartmentRent = new Apartment();
                                        apartmentRent.CreatedById = userId;
                                        apartmentRent.CreatedBy = $"{fullName} (Excel)";
                                        apartmentRent.TypeReportApply = blockRent.TypeReportApply;
                                        apartmentRent.TypeApartmentEntity = TypeApartmentEntity.APARTMENT_RENT;
                                        apartmentRent.UseAreaValue = dataValidItem.TongDienTichSuDungRieng;
                                        apartmentRent.UseAreaValue1 = dataValidItem.DienTichSuDungChung;
                                        apartmentRent.Code = dataValidItem.MaDinhDanh;
                                        apartmentRent.UsageStatus = dataValidItem.TinhTrangNhaApDung;
                                        apartmentRent.BlockId = blockRentId;
                                        apartmentRent.No = dataValidItem.SoThuTuHo;

                                        if (dataValidItem.SoQuyetDinh != null || dataValidItem.NgayQuyetDinh != null)
                                        {
                                            apartmentRent.EstablishStateOwnership = true;
                                            apartmentRent.CodeEstablishStateOwnership = dataValidItem.SoQuyetDinh;
                                            apartmentRent.DateEstablishStateOwnership = dataValidItem.NgayQuyetDinh;
                                        }

                                        if (dataValidItem.TenBanVe != null || dataValidItem.NgayLapBanVe != null)
                                        {
                                            apartmentRent.Blueprint = true;
                                            apartmentRent.NameBlueprint = dataValidItem.TenBanVe;
                                            apartmentRent.DateBlueprint = dataValidItem.NgayLapBanVe;
                                        }

                                        if (dataValidItem.NguoiDaiDien != null || dataValidItem.CanCuoc != null)
                                        {
                                            if (dataValidItem.ExistCanCuoc != true)
                                            {
                                                Customer customer = new Customer();
                                                customer.CreatedById = userId;
                                                customer.CreatedBy = $"{fullName} (Excel)";
                                                customer.FullName = dataValidItem.NguoiDaiDien ?? "";
                                                customer.Code = dataValidItem.CanCuoc;
                                                customer.PlaceCode = dataValidItem.NoiCap;
                                                customer.Doc = dataValidItem.NgayCap;
                                                customer.Address = dataValidItem.DiaChiThuongTru;
                                                customer.Phone = dataValidItem.SoDienThoai;
                                                _context.Customers.Add(customer);
                                                _context.SaveChanges();

                                                apartmentRent.CustomerId = customer.Id;
                                            }
                                            else
                                            {
                                                apartmentRent.CustomerId = dataValidItem.ExistCanCuocId;
                                            }

                                            if (dataValidItem.ThanhVien != null)
                                            {
                                                var spl = dataValidItem.ThanhVien.Replace(";", "+").Replace("|", "+").Split("+").Select(e => new Customer
                                                {
                                                    CreatedById = userId,
                                                    CreatedBy = $"{fullName} (Excel)",
                                                    FullName = e
                                                }).ToList();

                                                _context.AddRange(spl);
                                                _context.SaveChanges();
                                            }
                                        }

                                        _context.Apartments.Add(apartmentRent);
                                        _context.SaveChanges();
                                        generalDataImportCount.TotalApartmentRent += 1;

                                        //Thêm căn hộ bên bán
                                        if(dataValidItem.ExistInNormal != true)
                                        {
                                            Apartment apartmentNormal = apartmentRent;
                                            apartmentNormal.Id = 0;
                                            apartmentNormal.TypeApartmentEntity = TypeApartmentEntity.APARTMENT_NORMAL;
                                            apartmentNormal.BlockId = blockNormal.Id;

                                            _context.Apartments.Add(apartmentNormal);
                                            _context.SaveChanges();
                                            generalDataImportCount.TotalApartmentNormal += 1;
                                        }
                                        //else
                                        //{

                                        //}
                                    }
                                    else
                                    {
                                        //Tìm căn hộ thuê
                                        Apartment apartmentRent = _context.Apartments.Find(dataValidItem.ExistInRentId);
                                        if(apartmentRent != null)
                                        {
                                            //Kiểm tra căn hộ bán
                                            Apartment apartmentNormal = _context.Apartments.Where(e => e.Code == apartmentRent.Code && e.TypeApartmentEntity == TypeApartmentEntity.APARTMENT_NORMAL && e.Status != EntityStatus.DELETED).FirstOrDefault();
                                            if(apartmentNormal == null)
                                            {
                                                apartmentNormal = apartmentRent;
                                                apartmentNormal.Id = 0;
                                                apartmentNormal.TypeApartmentEntity = TypeApartmentEntity.APARTMENT_NORMAL;
                                                apartmentNormal.BlockId = blockNormal.Id;
                                                _context.Apartments.Add(apartmentNormal);
                                                _context.SaveChanges();
                                                generalDataImportCount.TotalApartmentNormal += 1;
                                            }
                                        }
                                    }
                                });
                            }
                        });

                        importHistory.DataExtra = generalDataImportCount;
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

        public static ResGeneralDataImport importData(MemoryStream ms, int sheetnumber, int rowStart, List<SimpleLaneData> laneData, List<SimpleData> wardData, List<District> districtData, List<SimpleData> typeBlockData, List<Block> blockData, List<Apartment> apartmentData, List<SimpleData> customerData)
        {
            XSSFWorkbook workbook = new XSSFWorkbook(ms);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            ResGeneralDataImport resFinal = new ResGeneralDataImport();
            resFinal.blocks = new List<Block>();
            resFinal.apartments = new List<Apartment>();

            List<GeneralDataImport> res = new List<GeneralDataImport>();
            for (int row = rowStart; row <= sheet.LastRowNum; row++)
            {
                if (sheet.GetRow(row) != null)
                {
                    //Đọc dữ liệu từ từng cell
                    GeneralDataImport inputDetail = new GeneralDataImport();
                    inputDetail.Valid = true;
                    inputDetail.ErrMsg = "";

                    for (int i = 0; i < 40 ; i++)
                    {
                        if((i >= 20 && i <= 25) || i > 40)
                        {
                            continue;
                        }

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
                                        inputDetail.LoaiBienBan = str;
                                        string strNoneUnicode = UtilsService.NonUnicode(str);
                                        if(strNoneUnicode == "nha-ho-rieng" || strNoneUnicode == "nha-rieng-le")
                                        {
                                            inputDetail.LoaiBienBanApDung = TypeReportApply.NHA_RIENG_LE;
                                        }
                                        else if(strNoneUnicode == "nha-ho-chung")
                                        {
                                            inputDetail.LoaiBienBanApDung = TypeReportApply.NHA_HO_CHUNG;
                                        }
                                        else if (strNoneUnicode == "nha-chung-cu")
                                        {
                                            inputDetail.LoaiBienBanApDung = TypeReportApply.NHA_CHUNG_CU;
                                        }
                                        else
                                        {
                                            inputDetail.Valid = false;
                                            inputDetail.ErrMsg += "Cột thông tin Loại biên bản áp dụng không hợp lệ\n";
                                        }
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột thông tin Loại biên bản áp dụng chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Loại biên bản áp dụng\n";
                                }
                            }
                            else if (i == 1)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.SoThuTuCan = str;
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Số thứ tự căn\n";
                                }
                            }
                            else if (i == 2)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.SoThuTuHo = str;
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Số thứ tự hộ\n";
                                }
                            }
                            else if (i == 3)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.MaDinhDanh = str;

                                        #region Kiểm tra Mã định danh có tồn tại trên hệ thống hay không để xử lý

                                        List<Block> blocks = blockData.Where(e => e.Code == str).ToList();
                                        if(blocks.Count > 0)
                                        {
                                            blocks.ForEach(x => { 
                                                x.Status = EntityStatus.DELETED;
                                                x.UpdatedBy = "Deleted due to duplicate code";
                                            });
                                            resFinal.blocks.AddRange(blocks);
                                        }
                                        else
                                        {
                                            List<Apartment> apartments = apartmentData.Where(e => e.Code == str).ToList();
                                            if(apartments.Count > 0)
                                            {
                                                foreach(Apartment apartment in apartments)
                                                {
                                                    apartment.Status = EntityStatus.DELETED;
                                                    apartment.UpdatedBy = "Deleted due to duplicate code";
                                                    resFinal.apartments.Add(apartment);

                                                    Block block = blockData.Where(b => b.Id == apartment.BlockId).FirstOrDefault();
                                                    if(block != null)
                                                    {
                                                        Block resBlock = resFinal.blocks.Where(b => b.Id == apartment.BlockId).FirstOrDefault();
                                                        if(resBlock == null)
                                                        {
                                                            block.Status = EntityStatus.DELETED;
                                                            block.UpdatedBy = "Deleted due to duplicate code";
                                                            resFinal.blocks.Add(block);
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        #endregion

                                        //if(inputDetail.LoaiBienBanApDung == TypeReportApply.NHA_RIENG_LE)
                                        //{
                                        //    SimpleBlockData rent = blockData.Where(c => c.Code == str && c.TypeBlockEntity == TypeBlockEntity.BLOCK_RENT).FirstOrDefault();
                                        //    if(rent != null)
                                        //    {
                                        //        inputDetail.ExistInRent = true;
                                        //        inputDetail.ExistInRentId = rent.Id;
                                        //    }

                                        //    SimpleBlockData normal = blockData.Where(c => c.Code == str && c.TypeBlockEntity == TypeBlockEntity.BLOCK_NORMAL).FirstOrDefault();
                                        //    if(normal != null)
                                        //    {
                                        //        inputDetail.ExistInNormal = true;
                                        //        inputDetail.ExistInNormalId = normal.Id;
                                        //    }

                                        //    //Check mã định danh không bị trùng bên căn hộ
                                        //    SimpleApartmentData apartment = apartmentData.Where(c => c.Code == str).FirstOrDefault();
                                        //    if(apartment != null)
                                        //    {
                                        //        inputDetail.Valid = false;
                                        //        inputDetail.ErrMsg += "Có căn hộ tồn tại trên hệ thống có Mã định danh này \n";
                                        //    }
                                        //}
                                        //else
                                        //{
                                        //    SimpleApartmentData rent = apartmentData.Where(c => c.Code == str && c.TypeApartmentEntity == TypeApartmentEntity.APARTMENT_RENT).FirstOrDefault();
                                        //    if (rent != null)
                                        //    {
                                        //        inputDetail.ExistInRent = true;
                                        //        inputDetail.ExistInRentId = rent.Id;
                                        //    }

                                        //    SimpleApartmentData normal = apartmentData.Where(c => c.Code == str && c.TypeApartmentEntity == TypeApartmentEntity.APARTMENT_NORMAL).FirstOrDefault();
                                        //    if (normal != null)
                                        //    {
                                        //        inputDetail.ExistInNormal = true;
                                        //        inputDetail.ExistInNormalId = normal.Id;
                                        //    }

                                        //    //Check mã định danh không bị trùng bên căn nhà
                                        //    SimpleBlockData block = blockData.Where(e => e.Code == str).FirstOrDefault();
                                        //    if(block != null)
                                        //    {
                                        //        inputDetail.Valid = false;
                                        //        inputDetail.ErrMsg += "Có căn nhà tồn tại trên hệ thống có Mã định danh này \n";
                                        //    }
                                        //}

                                        //if(inputDetail.ExistInRent == true && inputDetail.ExistInNormal == true)
                                        //{
                                        //    inputDetail.Valid = false;
                                        //    inputDetail.ErrMsg += "Mã định danh đã tồn tại cả ở thuê và bán \n";
                                        //}
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột Mã định danh chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Mã định danh\n";
                                }
                            }
                            else if (i == 4)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.SoNha = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột Số nhà chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột thông tin Số nhà\n";
                                }
                            }
                            else if (i == 5)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.Duong = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột Đường chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Đường\n";
                                }
                            }
                            else if (i == 6)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.Phuong = str;
                                        string strNoneUnicode = UtilsService.NonUnicode(str);
                                        string strNoneUnicodeDuong = UtilsService.NonUnicode(inputDetail.Duong);

                                        var lane = (from w in wardData.AsEnumerable()
                                                    join l in laneData.AsEnumerable() on w.Id equals l.Ward
                                                    where UtilsService.NonUnicode(w.Name).Contains(strNoneUnicode)
                                                    && UtilsService.NonUnicode(l.Name).Contains(strNoneUnicodeDuong)
                                                    select l).FirstOrDefault();

                                        if(lane != null)
                                        {
                                            inputDetail.DuongApDung = (int)lane.Id;
                                            inputDetail.PhuongApDung = (int)lane.Ward;
                                        }
                                        else
                                        {
                                            SimpleData ward = wardData.AsEnumerable().Where(e => UtilsService.NonUnicode(e.Name).Contains(strNoneUnicode)).FirstOrDefault();
                                            if (ward != null)
                                            {
                                                inputDetail.Valid = false;
                                                inputDetail.ErrMsg += "Cột thông tin Đường không hợp lệ \n";
                                            }
                                            else
                                            {
                                                inputDetail.Valid = false;
                                                inputDetail.ErrMsg += "Cột thông tin Phường không hợp lệ \n";
                                            }
                                        }


                                        //SimpleData ward = wardData.AsEnumerable().Where(e => UtilsService.NonUnicode(e.Name).Contains(strNoneUnicode)).FirstOrDefault();
                                        //if (ward != null)
                                        //{
                                        //    inputDetail.PhuongApDung = (int)ward.Id;
                                        //    string strNoneUnicodeDuong = UtilsService.NonUnicode(inputDetail.Duong);
                                        //    SimpleLaneData lane = laneData.AsEnumerable().Where(e => UtilsService.NonUnicode(e.Name).Contains(strNoneUnicodeDuong) && e.Ward == inputDetail.PhuongApDung).FirstOrDefault();
                                        //    if (lane != null)
                                        //    {
                                        //        inputDetail.DuongApDung = (int)lane.Id;
                                        //    }
                                        //    else
                                        //    {
                                        //        inputDetail.Valid = false;
                                        //        inputDetail.ErrMsg += "Cột thông tin Đường không hợp lệ \n";
                                        //    }
                                        //}
                                        //else
                                        //{
                                        //    inputDetail.Valid = false;
                                        //    inputDetail.ErrMsg += "Cột thông tin Phường không hợp lệ \n";
                                        //}
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột Phường chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Phường\n";
                                }
                            }
                            else if (i == 7)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.Quan = str;
                                        string strNoneUnicode = UtilsService.NonUnicode(str);
                                        District district = districtData.AsEnumerable().Where(e => UtilsService.NonUnicode(e.Name) == strNoneUnicode || UtilsService.NonUnicode(e.Name) == $"quan-{strNoneUnicode}").FirstOrDefault();
                                        if (district != null)
                                        {
                                            inputDetail.QuanApDung = (int)district.Id;
                                            inputDetail.TinhApDung = (int)district.ProvinceId;
                                        }
                                        else
                                        {
                                            inputDetail.Valid = false;
                                            inputDetail.ErrMsg += "Cột thông tin Quận không hợp lệ \n";
                                        }
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột Quận chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Quận\n";
                                }
                            }
                            else if (i == 8)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.LoaiNha = str;
                                        string strNoneUnicode = UtilsService.NonUnicode(str);
                                        SimpleData typeBlock = typeBlockData.AsEnumerable().Where(e => UtilsService.NonUnicode(e.Name).Contains(strNoneUnicode)).FirstOrDefault();
                                        if (typeBlock != null)
                                        {
                                            inputDetail.LoaiNhaApDung = (int)typeBlock.Id;
                                        }
                                        else
                                        {
                                            inputDetail.Valid = false;
                                            inputDetail.ErrMsg += "Cột thông tin Loại nhà không hợp lệ \n";
                                        }
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Loại nhà\n";
                                }
                            }
                            else if (i == 9)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.CapNha = str;
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Cấp nhà/Hạng nhà\n";
                                }
                            }
                            else if (i == 10)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.DienTichXayDung = float.Parse(str);
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Diện tích xây dựng\n";
                                }
                            }
                            else if (i == 11)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.DienTichKhuonVien = float.Parse(str);
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Khuôn viên(Tổng diện tích đất)\n";
                                }
                            }
                            else if (i == 12)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.DienTichDatRieng = float.Parse(str);
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Diện tích đất riêng\n";
                                }
                            }
                            else if (i == 13)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.DienTichDatChung = float.Parse(str);
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Diện tích đất chung\n";
                                }
                            }
                            else if (i == 14)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.DienTichDatChungPhanBo = float.Parse(str);
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Diện tích đất chung phân bổ\n";
                                }
                            }
                            else if (i == 15)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.DienTichSuDungRieng = float.Parse(str);
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Diện tích đất sử dụng riêng\n";
                                }
                            }
                            else if (i == 16)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.DienTichSuDungChung = float.Parse(str);
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Diện tích sử dụng chung\n";
                                }
                            }
                            else if (i == 17)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.DienTichSuDungChungPhanBo = float.Parse(str);
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Diện tích sử dụng chung phân bổ\n";
                                }
                            }
                            else if (i == 18)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TongDienTichSuDungChung = float.Parse(str);
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Tổng diện tích sử dụng chung\n";
                                }
                            }
                            else if (i == 19)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TongDienTichSuDungRieng = float.Parse(str);
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Tổng diện tích sử dụng\n";
                                }
                            }
                            else if (i == 26)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.NguoiDaiDien = str;
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Người đại diện ký hợp đồng\n";
                                }
                            }
                            else if (i == 27)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.CanCuoc = str;
                                        SimpleData simpleData = customerData.Where(c => c.Name == inputDetail.CanCuoc).FirstOrDefault();
                                        if (simpleData != null)
                                        {
                                            inputDetail.ExistCanCuocId = (int)simpleData.Id;
                                            inputDetail.ExistCanCuoc = true;
                                        }
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột CMND/CCCD/Thẻ quân nhân/Hộ chiếu\n";
                                }
                            }
                            else if (i == 28)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.NgayCap = DateTime.FromOADate(Double.Parse(str));
                                        if (inputDetail.NgayCap.Value.Year < 1900)
                                        {
                                            inputDetail.Valid = false;
                                            inputDetail.ErrMsg += "Ngày cấp không hợp lệ\n";
                                        }
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Ngày cấp\n";
                                }
                            }
                            else if (i == 29)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.NoiCap = str;
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Nơi cấp\n";
                                }
                            }
                            else if (i == 30)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.DiaChiThuongTru = str;
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Địa chỉ thường trú\n";
                                }
                            }
                            else if (i == 31)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.SoDienThoai = str;
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Số điện thoại\n";
                                }
                            }
                            else if (i == 32)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.ThanhVien = str;
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Thành viên sử dụng nhà\n";
                                }
                            }
                            else if (i == 35)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.SoQuyetDinh = str;
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Số quyết định - Xác lập SHNN\n";
                                }
                            }
                            else if (i == 36)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.NgayQuyetDinh = DateTime.FromOADate(Double.Parse(str));
                                        if (inputDetail.NgayQuyetDinh.Value.Year < 1900)
                                        {
                                            inputDetail.Valid = false;
                                            inputDetail.ErrMsg += "Ngày quyết định - Xác lập SHNN không hợp lệ\n";
                                        }
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Ngày quyết định - Xác lập SHNN\n";
                                }
                            }
                            else if (i == 37)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TenBanVe = str;
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Tên bản vẽ\n";
                                }
                            }
                            else if (i == 38)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.NgayLapBanVe = DateTime.FromOADate(Double.Parse(str));
                                        if (inputDetail.NgayLapBanVe.Value.Year < 1900)
                                        {
                                            inputDetail.Valid = false;
                                            inputDetail.ErrMsg += "Ngày lập bản vẽ không hợp lệ\n";
                                        }
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Ngày lập bản vẽ\n";
                                }
                            }
                            else if (i == 39)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TinhTrangNha = str;

                                        string strNoneUnicode = UtilsService.NonUnicode(str);
                                        if (strNoneUnicode == "dang-cho-thue")
                                        {
                                            inputDetail.TinhTrangNhaApDung = UsageStatus.DANG_CHO_THUE;
                                        }
                                        else if (strNoneUnicode == "nha-trong")
                                        {
                                            inputDetail.TinhTrangNhaApDung = UsageStatus.NHA_TRONG;
                                        }
                                        else if (strNoneUnicode == "tranh-chap")
                                        {
                                            inputDetail.TinhTrangNhaApDung = UsageStatus.TRANH_CHAP;
                                        }
                                        else
                                        {
                                            inputDetail.TinhTrangNhaApDung = UsageStatus.CAC_TRUONG_HOP_KHAC;
                                        }
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Tình trạng nhà\n";
                                }
                            }
                            else if (i == 40)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.GhiChu = str;
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

            resFinal.data = res;
            return resFinal;
        }

        #endregion

        #region API cập nhật dữ liệu bị sai do code sai
        [HttpPost]
        [Route("UpdateWrongData")]
        public async Task<IActionResult> Post()
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            DefaultResponse def = new DefaultResponse();

            try
            {
                var data = (from a in _context.Apartments
                            join b in _context.Blocks on a.BlockId equals b.Id
                            where a.Status != EntityStatus.DELETED
                                && b.Status != EntityStatus.DELETED
                                && a.TypeApartmentEntity == TypeApartmentEntity.APARTMENT_RENT
                                && b.TypeBlockEntity == TypeBlockEntity.BLOCK_NORMAL
                            select new WrongDataUpdate
                            {
                                Lane = b.Lane,
                                Address = b.Address,
                                apartment = a
                            }).ToList();

                using (var transaction = _context.Database.BeginTransaction())
                {
                    List<Apartment> apartments = new List<Apartment>();

                    foreach (var item in data)
                    {
                        Block block = _context.Blocks.Where(b => b.Address == item.Address && b.Lane == item.Lane && b.TypeBlockEntity == TypeBlockEntity.BLOCK_RENT && b.Status != EntityStatus.DELETED).FirstOrDefault();
                        if (block != null)
                        {
                            item.apartment.UpdatedById = item.apartment.BlockId;
                            item.apartment.UpdatedBy = "-1";
                            item.apartment.BlockId = block.Id;
                            apartments.Add(item.apartment);
                        }
                    }

                    if (apartments.Count > 0)
                    {
                        try
                        {
                            _context.UpdateRange(apartments);
                            _context.SaveChanges();

                            transaction.Commit();

                            def.meta = new Meta(200, ApiConstants.MessageResource.UPDATE_SUCCESS);
                            def.data = apartments;
                            return Ok(def);
                        }
                        catch
                        {
                            transaction.Rollback();
                            def.meta = new Meta(500, "Save Db Err");
                            return Ok(def);
                        }
                    }
                    else
                    {
                        def.meta = new Meta(400, "length == 0");
                        def.data = apartments;
                        return Ok(def);
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
        #endregion

        #region API cập nhật dữ liệu import bị sai do nhập dữ liệu ở biểu mẫu bị sai
        [HttpPost]
        [Route("UpdateWrongDataFromTemplate/{sheetNumber}")]
        public IActionResult UpdateWrongDataFromTemplate(int sheetNumber)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            DefaultResponse def = new DefaultResponse();

            //var identity = (ClaimsIdentity)User.Identity;
            //int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id").Select(c => c.Value).SingleOrDefault());
            //string fullName = identity.Claims.Where(c => c.Type == "FullName").Select(c => c.Value).SingleOrDefault() ?? string.Empty;
            //string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            //if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.IMPORT))
            //{
            //    def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_ACTION_MESSAGE);
            //    return Ok(def);
            //}

            try
            {
                int i = 0;
                var httpRequest = Request.Form.Files;

                List<WrongDataFromTemplate> data = new List<WrongDataFromTemplate>();
                //Lấy dữ liệu từ file excel và lưu lại file
                foreach (var file in httpRequest)
                {
                    var postedFile = httpRequest[i];
                    if (postedFile != null && postedFile.Length > 0)
                    {
                        IList<string> AllowedDocuments = new List<string> { ".xls", ".xlsx" };
                        int MaxContentLength = 1024 * 1024 * 100; //Size = 100 MB  

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
                            var message = string.Format("Vui lòng Up file có dung lượng nhỏ hơn 100 MB!");
                            def.meta = new Meta(600, message);
                            return Ok(def);
                        }
                        else
                        {
                            using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                            {
                                byte[] fileData = binaryReader.ReadBytes((int)file.Length);
                                using (MemoryStream ms = new MemoryStream(fileData))
                                {
                                    data = importWrongData(ms, sheetNumber, 4);
                                }
                            }
                        }
                    }
                    i++;
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        if (data.Count > 0)
                        {
                            Block firstRentBlock = null;
                            List<Block> dataRentBlockDelete = new List<Block>();

                            Block firstSellBlock = null;
                            List<Block> dataSellBlockDelete = new List<Block>();

                            foreach (var item in data)
                            {
                                //Căn hộ thuê
                                Apartment apartmentRent = _context.Apartments.Where(e => e.TypeReportApply == item.LoaiBienBanApDung && e.TypeApartmentEntity == TypeApartmentEntity.APARTMENT_RENT && e.Code == item.MaDinhDanh && e.Status != EntityStatus.DELETED).FirstOrDefault();
                                if (apartmentRent != null)
                                {
                                    if (firstRentBlock == null)
                                    {
                                        firstRentBlock = _context.Blocks.Where(e => e.Id == apartmentRent.BlockId && e.TypeBlockEntity == TypeBlockEntity.BLOCK_RENT && e.Status != EntityStatus.DELETED).FirstOrDefault();
                                        if (firstRentBlock != null)
                                        {
                                            firstRentBlock.Address = item.DiaChiNha;
                                            firstRentBlock.UpdatedBy = "-1";
                                        }
                                    }
                                    else
                                    {
                                        Block rentBlock = _context.Blocks.Where(e => e.Id != firstRentBlock.Id && e.Id == apartmentRent.BlockId && e.TypeBlockEntity == TypeBlockEntity.BLOCK_RENT && e.Status != EntityStatus.DELETED).FirstOrDefault();
                                        if (rentBlock != null)
                                        {
                                            dataRentBlockDelete.Add(rentBlock);
                                        }
                                    }

                                    apartmentRent.Address = item.DiaChiCanHo;
                                    apartmentRent.UpdatedBy = "-1";
                                    apartmentRent.BlockId = firstRentBlock != null ? firstRentBlock.Id : apartmentRent.BlockId;
                                    _context.Update(apartmentRent);
                                }

                                //Căn hộ bán
                                Apartment apartmentSell = _context.Apartments.Where(e => e.TypeReportApply == item.LoaiBienBanApDung && e.TypeApartmentEntity == TypeApartmentEntity.APARTMENT_NORMAL && e.Code == item.MaDinhDanh && e.Status != EntityStatus.DELETED).FirstOrDefault();
                                if (apartmentSell != null)
                                {
                                    if (firstSellBlock == null)
                                    {
                                        firstSellBlock = _context.Blocks.Where(e => e.Id == apartmentSell.BlockId && e.TypeBlockEntity == TypeBlockEntity.BLOCK_NORMAL && e.Status != EntityStatus.DELETED).FirstOrDefault();
                                        if (firstSellBlock != null)
                                        {
                                            firstSellBlock.Address = item.DiaChiNha;
                                            firstSellBlock.UpdatedBy = "-1";
                                        }
                                    }
                                    else
                                    {
                                        Block sellBlock = _context.Blocks.Where(e => e.Id != firstSellBlock.Id && e.Id == apartmentSell.BlockId && e.TypeBlockEntity == TypeBlockEntity.BLOCK_NORMAL && e.Status != EntityStatus.DELETED).FirstOrDefault();
                                        if (sellBlock != null)
                                        {
                                            dataSellBlockDelete.Add(sellBlock);
                                        }
                                    }

                                    apartmentSell.Address = item.DiaChiCanHo;
                                    apartmentSell.UpdatedBy = "-1";
                                    apartmentSell.BlockId = firstSellBlock != null ? firstSellBlock.Id : apartmentSell.BlockId;
                                    _context.Update(apartmentSell);
                                }
                            }

                            if (firstRentBlock != null) _context.Update(firstRentBlock);
                            if (firstSellBlock != null) _context.Update(firstSellBlock);

                            if (dataRentBlockDelete.Count > 0)
                            {
                                dataRentBlockDelete.ForEach(item => {
                                    item.Status = EntityStatus.DELETED;
                                    item.UpdatedBy = "-1";
                                });

                                _context.UpdateRange(dataRentBlockDelete);
                            }

                            if (dataSellBlockDelete.Count > 0)
                            {
                                dataSellBlockDelete.ForEach(item => {
                                    item.Status = EntityStatus.DELETED;
                                    item.UpdatedBy = "-1";
                                });

                                _context.UpdateRange(dataSellBlockDelete);
                            }

                            _context.SaveChanges();

                            transaction.Commit();
                            def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                            def.metadata = data.Count;
                            def.data = data;
                        }
                        else
                        {
                            def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                        }
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

        public static List<WrongDataFromTemplate> importWrongData(MemoryStream ms, int sheetnumber, int rowStart)
        {
            XSSFWorkbook workbook = new XSSFWorkbook(ms);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);

            List<WrongDataFromTemplate> res = new List<WrongDataFromTemplate>();
            for (int row = rowStart; row <= sheet.LastRowNum; row++)
            {
                if (sheet.GetRow(row) != null)
                {
                    //Đọc dữ liệu từ từng cell
                    WrongDataFromTemplate inputDetail = new WrongDataFromTemplate();

                    for (int i = 0; i < 6; i++)
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
                                        inputDetail.LoaiBienBan = str;
                                        string strNoneUnicode = UtilsService.NonUnicode(str);
                                        if (strNoneUnicode == "nha-ho-rieng" || strNoneUnicode == "nha-rieng-le")
                                        {
                                            inputDetail.LoaiBienBanApDung = TypeReportApply.NHA_RIENG_LE;
                                        }
                                        else if (strNoneUnicode == "nha-ho-chung")
                                        {
                                            inputDetail.LoaiBienBanApDung = TypeReportApply.NHA_HO_CHUNG;
                                        }
                                        else if (strNoneUnicode == "nha-chung-cu")
                                        {
                                            inputDetail.LoaiBienBanApDung = TypeReportApply.NHA_CHUNG_CU;
                                        }
                                    }
                                    else
                                    {
                                    }
                                }
                                catch
                                {
                                }
                            }
                            else if (i == 1)
                            {
                                continue;
                            }
                            else if (i == 2)
                            {
                                continue;

                            }
                            else if (i == 3)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.MaDinhDanh = str;
                                    }
                                    else
                                    {
                                    }
                                }
                                catch
                                {
                                }
                            }
                            else if (i == 4)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.DiaChiCanHo = str;
                                    }
                                    else
                                    {
                                    }
                                }
                                catch
                                {
                                }
                            }
                            else if (i == 5)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.DiaChiNha = str;
                                    }
                                    else
                                    {
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error("Exception:" + ex);
                        }
                    }

                    res.Add(inputDetail);
                }
            }

            return res;
        }
        #endregion

        #region Cập nhật dữ liệu sai khi API cập nhật dữ liệu import bị sai do nhập dữ liệu ở biểu mẫu bị sai(UpdateWrongDataFromTemplate)
        [HttpPost]
        [Route("UpdateWrongDataFromTemplatePart2")]
        public IActionResult UpdateWrongDataFromTemplatePart2()
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            DefaultResponse def = new DefaultResponse();

            //var identity = (ClaimsIdentity)User.Identity;
            //int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id").Select(c => c.Value).SingleOrDefault());
            //string fullName = identity.Claims.Where(c => c.Type == "FullName").Select(c => c.Value).SingleOrDefault() ?? string.Empty;
            //string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            //if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.IMPORT))
            //{
            //    def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_ACTION_MESSAGE);
            //    return Ok(def);
            //}

            try
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        List<Block> blocks = (from b in _context.Blocks
                                              join a in _context.Apartments on b.Id equals a.BlockId
                                              where b.Status == EntityStatus.DELETED && a.Status != EntityStatus.DELETED
                                              select b).Distinct().ToList();

                        blocks.ForEach(x => {
                            x.Status = EntityStatus.NORMAL;
                            x.UpdatedBy = "-1";
                        });

                        _context.UpdateRange(blocks);
                        _context.SaveChanges();

                        transaction.Commit();
                        def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                        def.metadata = blocks.Count;
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
        #endregion

        #region API cập nhật lại dữ liệu khách hàng NOC bị sai do import dữ liệu
        [HttpPost]
        [Route("UpdateWrongCustomerData")]
        public IActionResult UpdateWrongCustomerData()
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            DefaultResponse def = new DefaultResponse();

            //var identity = (ClaimsIdentity)User.Identity;
            //int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id").Select(c => c.Value).SingleOrDefault());
            //string fullName = identity.Claims.Where(c => c.Type == "FullName").Select(c => c.Value).SingleOrDefault() ?? string.Empty;
            //string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            //if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.IMPORT))
            //{
            //    def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_ACTION_MESSAGE);
            //    return Ok(def);
            //}

            try
            {
                int i = 0;
                var httpRequest = Request.Form.Files;

                List<WrongCustomerData> data = new List<WrongCustomerData>();
                //Lấy dữ liệu từ file excel và lưu lại file
                foreach (var file in httpRequest)
                {
                    var postedFile = httpRequest[i];
                    if (postedFile != null && postedFile.Length > 0)
                    {
                        IList<string> AllowedDocuments = new List<string> { ".xls", ".xlsx" };
                        int MaxContentLength = 1024 * 1024 * 100; //Size = 100 MB  

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
                            var message = string.Format("Vui lòng Up file có dung lượng nhỏ hơn 100 MB!");
                            def.meta = new Meta(600, message);
                            return Ok(def);
                        }
                        else
                        {
                            using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                            {
                                byte[] fileData = binaryReader.ReadBytes((int)file.Length);
                                using (MemoryStream ms = new MemoryStream(fileData))
                                {
                                    data = importWrongCustomerData(ms, 0, 4);
                                }
                            }
                        }
                    }
                    i++;
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        if (data.Count > 0)
                        {
                            //Xóa hết dữ liệu khách hàng
                            List<Customer> customerDataDeleted = _context.Customers.Where(e => e.Status != EntityStatus.DELETED).ToList();
                            customerDataDeleted.ForEach(x =>
                            {
                                x.Status = EntityStatus.DELETED;
                                x.UpdatedBy = "-3";
                            });
                            _context.UpdateRange(customerDataDeleted);
                            _context.SaveChanges();

                            //Map dữ liệu
                            List<Customer> customers = new List<Customer>();
                            foreach(var item in data)
                            {
                                Customer customer = new Customer();
                                customer.FullName = item.NguoiDaiDien ?? "";
                                customer.Code = item.CanCuoc;
                                customer.Doc = item.NgayCap;
                                customer.PlaceCode = item.NoiCap;
                                customer.Address = item.HoKhauThuongTru;
                                customer.Phone = item.SoDienThoai;
                                customer.CreatedBy = item.MaDinhDanh;
                                customers.Add(customer);
                            }

                            _context.AddRange(customers);
                            _context.SaveChanges();

                            List<Block> blocks = _context.Blocks.Where(b => b.TypeReportApply == TypeReportApply.NHA_RIENG_LE && b.TypeBlockEntity == TypeBlockEntity.BLOCK_RENT && b.Status != EntityStatus.DELETED).ToList();
                            List<Apartment> apartments = _context.Apartments.Where(b => b.TypeReportApply != TypeReportApply.NHA_RIENG_LE && b.TypeApartmentEntity == TypeApartmentEntity.APARTMENT_RENT && b.Status != EntityStatus.DELETED).ToList();

                            List<Block> blockDataUpdate = new List<Block>();
                            List<Apartment> apartmentDataupdate = new List<Apartment>();

                            foreach (var item in data)
                            {
                                if(item.LoaiBienBanApDung == TypeReportApply.NHA_RIENG_LE)
                                {
                                    Block block = blocks.Where(e => e.Code == item.MaDinhDanh).FirstOrDefault();
                                    if(block != null)
                                    {
                                        Customer customer = customers.Where(e => e.CreatedBy == item.MaDinhDanh).FirstOrDefault();
                                        if(customer != null)
                                        {
                                            block.CustomerId = customer.Id;
                                            block.UpdatedBy = "-3";
                                            blockDataUpdate.Add(block);
                                        }
                                        else
                                        {
                                            block.CustomerId = null;
                                            block.UpdatedBy = "-3";
                                            blockDataUpdate.Add(block);
                                        }
                                    }
                                }
                                else
                                {
                                    Apartment apartment = apartments.Where(e => e.Code == item.MaDinhDanh).FirstOrDefault();
                                    if(apartment != null)
                                    {
                                        Customer customer = customers.Where(e => e.CreatedBy == item.MaDinhDanh).FirstOrDefault();
                                        if (customer != null)
                                        {
                                            apartment.CustomerId = customer.Id;
                                            apartment.UpdatedBy = "-3";
                                            apartmentDataupdate.Add(apartment);
                                        }
                                        else
                                        {
                                            apartment.CustomerId = null;
                                            apartment.UpdatedBy = "-3";
                                            apartmentDataupdate.Add(apartment);
                                        }
                                    }
                                }
                            }

                            _context.UpdateRange(blockDataUpdate);
                            _context.UpdateRange(apartmentDataupdate);
                            _context.SaveChanges();

                            transaction.Commit();
                            def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                            def.metadata = data.Count;
                            def.data = data;
                        }
                        else
                        {
                            def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("UpdateWrongCustomerData:" + ex);
                        transaction.Rollback();
                        def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                    }
                }

                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("UpdateWrongCustomerData:" + ex);
                def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                return Ok(def);
            }
        }

        public static List<WrongCustomerData> importWrongCustomerData(MemoryStream ms, int sheetnumber, int rowStart)
        {
            XSSFWorkbook workbook = new XSSFWorkbook(ms);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);

            List<WrongCustomerData> res = new List<WrongCustomerData>();
            for (int row = rowStart; row <= sheet.LastRowNum; row++)
            {
                if (sheet.GetRow(row) != null)
                {
                    //Đọc dữ liệu từ từng cell
                    WrongCustomerData inputDetail = new WrongCustomerData();

                    for (int i = 0; i < 9; i++)
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
                                        inputDetail.LoaiBienBan = str;
                                        string strNoneUnicode = UtilsService.NonUnicode(str);
                                        if (strNoneUnicode == "2")
                                        {
                                            inputDetail.LoaiBienBanApDung = TypeReportApply.NHA_RIENG_LE;
                                        }
                                        else if (strNoneUnicode == "1")
                                        {
                                            inputDetail.LoaiBienBanApDung = TypeReportApply.NHA_HO_CHUNG;
                                        }
                                    }
                                    else
                                    {
                                    }
                                }
                                catch
                                {
                                }
                            }
                            else if (i == 1)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.MaDinhDanh = str;
                                    }
                                }
                                catch { }
                            }
                            else if (i == 2)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.NguoiDaiDien = str;
                                    }
                                }
                                catch { }
                            }
                            else if (i == 3)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.CanCuoc = str;
                                    }
                                }
                                catch { }
                            }
                            else if (i == 4)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.NgayCap = DateTime.FromOADate(Double.Parse(str));
                                        if (inputDetail.NgayCap.Value.Year < 1900)
                                        {
                                            inputDetail.NgayCap = null;
                                        }
                                    }
                                }
                                catch { }
                            }
                            else if (i == 5)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.NoiCap = str;
                                    }
                                }
                                catch { }
                            }
                            else if (i == 6)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.HoKhauThuongTru = str;
                                    }
                                }
                                catch { }
                            }
                            else if (i == 7)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.DiaChiLienHe = str;
                                    }
                                }
                                catch { }
                            }
                            else if (i == 8)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.SoDienThoai = str;
                                    }
                                }
                                catch { }
                            }
                        }
                        catch (Exception ex)
                        {
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
