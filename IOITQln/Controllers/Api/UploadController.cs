using IOITQln.Common.Services;
using IOITQln.Common.ViewModels.Common;
using AutoMapper;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Microsoft.Extensions.Configuration;
using IOITQln.Common.Constants;
using Microsoft.Net.Http.Headers;
using IOITQln.Persistence;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using IOITQln.Models.Dto;
using System.Net.Http;
using static IOITQln.Common.Enums.AppEnums;
using IOITQln.Entities;

namespace IOITQln.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("upload", "upload");
        private IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;
        private readonly ApiDbContext _context;

        public UploadController(IWebHostEnvironment hostingEnvironment, IConfiguration configuration, ApiDbContext context)
        {
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
            _context = context;
        }

        //api upload image
        [HttpPost("uploadImage")]
        public IActionResult UploadImage()
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
                int i = 0;
                var httpRequest = Request.Form.Files;
                List<String> files = new List<string>();
                foreach (var file in httpRequest)
                {
                    var postedFile = httpRequest[i];
                    if (postedFile != null && postedFile.Length > 0)
                    {
                        int MaxContentLength = 1024 * 1024 * 12; //Size = 12 MB  

                        IList<string> AllowedImage = new List<string> { ".bmp", ".exr", ".ico", ".jpg", ".jpeg", ".gif", ".pbm", ".pcx", ".pgm", ".png", ".ppm", ".psd", ".tif", ".tiff", ".tga", ".wbmp" };
                        var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                        var name = postedFile.FileName.Substring(0, postedFile.FileName.LastIndexOf('.'));
                        var extension = ext.ToLower();
                        if (!AllowedImage.Contains(extension))
                        {
                            var message = string.Format("Upload đúng định dạng ảnh cho phép");
                            def.meta = new Meta(600, message);
                            return Ok(def);
                        }
                        else if (postedFile.Length > MaxContentLength)
                        {
                            var message = string.Format("Upload hình ảnh có dung lượng nhỏ hơn 12MB.");
                            def.meta = new Meta(600, message);
                            return Ok(def);
                        }
                        else if (postedFile.Length < 0)
                        {
                            var message = string.Format("Upload hình ảnh có dung lượng lớn hơn 1KB.");
                            def.meta = new Meta(600, message);
                            return Ok(def);
                        }
                        else
                        {
                            string folderName = _configuration["AppSettings:BaseUrlImg"];
                            string webRootPath = _hostingEnvironment.WebRootPath;
                            string newPath = Path.Combine(webRootPath, folderName);

                            if (!Directory.Exists(newPath))
                            {
                                Directory.CreateDirectory(newPath);
                            }

                            string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.ToString().Trim('"');
                            if (UtilsService.ConvertUrlpath(fileName).Contains("../") || UtilsService.ConvertUrlpath(fileName).Contains("..\\") || fileName.IndexOfAny(Path.GetInvalidPathChars()) > -1)
                            {
                                var vMessage = "Tên file không hợp lệ!";

                                def.meta = new Meta(202, vMessage);
                                return Ok(def);
                            }

                            byte[] fileData = null;
                            using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                            {
                                fileData = binaryReader.ReadBytes((int)file.Length);
                            }
                            DateTime now = DateTime.Now;
                            string img = name + "_" + now.ToString("yyyyMMddHHmmssfff") + extension;
                            string rel = "thumbs\\" + img;

                            string fullPath = Path.Combine(newPath, img);
                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                postedFile.CopyTo(stream);
                                var image = Bitmap.FromStream(stream);

                                string thumbPath = Path.Combine(newPath, rel);
                                createThumb(400, image, thumbPath, extension);
                            }

                            files.Add(img);

                        }

                    }
                }

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = files;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("UploadImage Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        public static void createThumb(int thumbWidth, Image image, string thumbPath, string file_type)
        {
            //extract path
            int lastIndex = thumbPath.LastIndexOf("\\");
            string path = thumbPath.Substring(0, lastIndex);
            Directory.CreateDirectory(path);
            
            double srcWidth = image.Width;
            double srcHeight = image.Height;
            double thumbHeight = (srcHeight / srcWidth) * thumbWidth;
            Bitmap bmp = new Bitmap(thumbWidth, (int)thumbHeight);

            Graphics gr = Graphics.FromImage(bmp);
            gr.SmoothingMode = SmoothingMode.HighQuality;
            gr.CompositingQuality = CompositingQuality.HighQuality;
            gr.InterpolationMode = InterpolationMode.High;

            Rectangle rectDestination = new Rectangle(0, 0, thumbWidth, (int)thumbHeight);
            gr.DrawImage(image, rectDestination, 0, 0, (int)srcWidth, (int)srcHeight, GraphicsUnit.Pixel);
            
            if (file_type.ToLower() == ".jpg" || file_type.ToLower() == ".jpeg")
                bmp.Save(thumbPath, ImageFormat.Jpeg);
            else if (file_type.ToLower() == ".png")
                bmp.Save(thumbPath, ImageFormat.Png);
            else if (file_type.ToLower() == ".gif")
                bmp.Save(thumbPath, ImageFormat.Gif);
            else
                bmp.Save(thumbPath, ImageFormat.Jpeg);

            bmp.Dispose();
            image.Dispose();
        }

        //api upload file
        [HttpPost("uploadFile")]
        public IActionResult UploadFile()
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
                int i = 0;
                List<String> files = new List<string>();
                var httpRequest = Request.Form.Files;
                foreach (var file in httpRequest)
                {
                    var postedFile = httpRequest[i];
                    if (postedFile != null && postedFile.Length > 0)
                    {
                        int MaxContentLength = 1024 * 1024 * 500; //Size = 500 MB  

                        IList<string> AllowedDocuments = new List<string> { ".csv", ".doc", ".docx",".pptx", ".pdf",".xls", ".xlsx" };
                        IList<string> AllowedImage = new List<string> { ".bmp", ".exr", ".ico", ".jpg", ".jpeg", ".gif", ".pbm", ".pcx", ".pgm", ".png", ".ppm", ".psd", ".tif", ".tiff", ".tga", ".wbmp" };
                        //IList<string> AllowedVideo = new List<string> { ".3gp", ".3g2", ".asf", ".avi", ".f4v", ".flv", ".ismv", ".m4v", ".mkv", ".mov", ".mp4", ".mpeg", ".ogv", ".wmv", ".webm" };
                        //IList<string> AllowedAudio = new List<string> { ".aac", ".ac3", ".aiff", ".amr", ".ape", ".au", ".flac", ".m4a", ".mka", ".mp3", ".mpc", ".ogg", ".ra", ".wav", ".wma" };
                        //IList<string> AllowedElectronicBook = new List<string> { ".chm", ".epub", ".fb2", ".lit", ".lrf", ".mobi", ".pdb", ".rb", ".tcr" };
                        IList<string> AllowedArchives = new List<string> { ".7z", ".zip", ".rar" };
                        var ext = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf('.'));
                        var name = postedFile.FileName.Substring(0, postedFile.FileName.LastIndexOf('.'));
                        var extension = ext.ToLower();
                        bool checkFile = true;
                        if (AllowedDocuments.Contains(extension) || AllowedImage.Contains(extension) || AllowedArchives.Contains(extension))
                        {
                            checkFile = false;
                        }

                        if (checkFile)
                        {
                            var message = string.Format("Bạn vui lòng upload đúng định dạng file cho phép");
                            def.meta = new Meta(600, message);
                            return Ok(def);
                        }
                        if (postedFile.Length > MaxContentLength)
                        {
                            var message = string.Format("Bạn vui lòng upload hình ảnh có dung lượng nhỏ hơn 500MB.");
                            def.meta = new Meta(601, message);
                            return Ok(def);
                        }
                        else if (postedFile.Length < 0)
                        {
                            var message = string.Format("Bạn vui lòng upload hình ảnh có dung lượng lớn hơn 1KB.");
                            def.meta = new Meta(602, message);
                            return Ok(def);
                        }
                        else
                        {
                            //Lưu file vào 1 nơi ngoài thư mục web
                            string folderName = _configuration["AppSettings:BaseUrlFile"]; ;
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

                            files.Add(fileName);
                        }

                    }
                }

                def.data = files;
                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("UploadFile Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        //APi download file
        [HttpPost("downloadFile")]
        public async Task<IActionResult> downloadFile(DownloadFileDTO data)
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
                string folderName = _configuration["AppSettings:BaseUrlFile"]; ;
                string webRootPath = _hostingEnvironment.WebRootPath;
                string newPath = Path.Combine(webRootPath, folderName);
                string fullPath = Path.Combine(newPath, data.FileName);

                if (fullPath != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                        using (FileStream file = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                        {
                            file.CopyTo(ms);
                        }

                        return File(ms.ToArray(), "application/octet-stream", data.FileName);
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
                log.Error("downloadFile:" + ex.Message);
                throw;
            }
        }

        //API download file mẫu excel import
        [HttpPost("downloadFileExcelTemplate/{type}")]
        public async Task<IActionResult> downloadFileExcelTemplate(ImportHistoryType type)
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
                string fileName = "";

                switch(type)
                {
                    case ImportHistoryType.Md167Receipt:
                        fileName = "phieu_thu.xlsx";
                        break;
                    case ImportHistoryType.Md167Landprice:
                        fileName = "bang_giadat.xlsx";
                        break;
                    case ImportHistoryType.Md167House:
                        fileName = "thongtin_csnd.xlsx";
                        break;
                    case ImportHistoryType.Md167Kios:
                        fileName = "kios.xlsx";
                        break;
                    case ImportHistoryType.Md167MainContract:
                        fileName = "hop_dong_nha_167.xlsx";
                        break;
                    case ImportHistoryType.Md167ExtraContract:
                        fileName = "phu_luc_nha_167.xlsx";
                        break;
                    case ImportHistoryType.NocSalary:
                        fileName = "hesoluong.xlsx";
                        break;
                    case ImportHistoryType.NocCoefficient:
                        fileName = "hesothoidiembotri.xlsx";
                        break;
                    case ImportHistoryType.NocConversion:
                        fileName = "hesoquydoi.xlsx";
                        break;
                    case ImportHistoryType.NocDefaultCoeficient:
                        fileName = "hesomacdinh.xlsx";
                        break;
                    case ImportHistoryType.NocDiscountCoefficient:
                        fileName = "hesovitri.xlsx";
                        break;
                    case ImportHistoryType.NocRentingPrice:
                        fileName = "banggiathue.xlsx";
                        break;

                    case ImportHistoryType.NocDecreeType2:
                        fileName = "van_ban_phap_luat_lien_quan.xlsx";
                        break;
                    case ImportHistoryType.Noc_Ct_MainTexture:
                        fileName = "hien_trang_ket_cau_chinh.xlsx";
                        break;
                    case ImportHistoryType.Noc_Ratio_MainTexture:
                        fileName = "ty_le_gia_tri_ket_cau_chinh.xlsx";
                        break;
                    case ImportHistoryType.Noc_Construction_price:
                        fileName = "chi_so_xay_dung_cong_trinh.xlsx";
                        break;
                    case ImportHistoryType.Noc_Salary_Coefficient:
                        fileName = "he_so_luong_co_ban.xlsx";
                        break;
                    case ImportHistoryType.Noc_Deduction_Coefficient:
                        fileName = "he_so_mien_giam_tien_nha.xlsx";
                        break;
                    case ImportHistoryType.Noc_Area_Correction_Coefficient:
                        fileName = "he_so_dieu_chinh_vung.xlsx";
                        break;
                    case ImportHistoryType.Noc_No2_LandPrice:
                        fileName = "bang_gia_dat_so_2.xlsx";
                        break;
                    case ImportHistoryType.Noc_Land_Special_Coefficient:
                        fileName = "he_so_khu_dat_dac_biet.xlsx";
                        break;
                    case ImportHistoryType.Noc_Position_Coefficient:
                        fileName = "he_so_vi_tri.xlsx";
                        break;
                    case ImportHistoryType.Noc_K_LandPrice_Correction_Coefficient:
                        fileName = "he_so_k.xlsx";
                        break;
                    case ImportHistoryType.Noc_Deduction_Land_Money:
                        fileName = "tien_dat_mien_giam.xlsx";
                        break;
                    case ImportHistoryType.Noc_Customer:
                        fileName = "noc_khach_hang.xlsx";
                        break;
                    case ImportHistoryType.Noc_Landprice:
                        fileName = "bang_giadat.xlsx";
                        break;
                    case ImportHistoryType.Noc_GeneralDataImport:
                        fileName = "mau_import_thong_tin_qln_noc.xlsx";
                        break;
                    case ImportHistoryType.Noc_Contract_Rent_Receipt:
                        fileName = "phieu_thu_hop_dong_thue_noc.xlsx";
                        break;
                    case ImportHistoryType.Noc_Contract_Rent:
                        fileName = "can-nha-thue-mau2.xlsx";
                        break;
                    case ImportHistoryType.Common_Department:
                        fileName = "danh_muc_chung_phong_ban.xlsx";
                        break;
                    case ImportHistoryType.Common_Position:
                        fileName = "danh_muc_chung_chuc_danh.xlsx";
                        break;
                    case ImportHistoryType.Common_Lane:
                        fileName = "danh_muc_chung_duong.xlsx";
                        break;
                    case ImportHistoryType.NocRentBlock:
                        fileName = "can-nha-thue.xlsx";
                        break;
                    case ImportHistoryType.TdcIngredientsPrice:
                        fileName = "thanh-phan-gia-cau-thanh.xlsx";
                        break;
                    case ImportHistoryType.TdcOriginalPriceAndTax:
                        fileName = "gia_goc_thue_phi.xlsx";
                        break;
                    case ImportHistoryType.TdcProfitValue:
                        fileName = "hesolaiphatthue.xlsx";
                        break;
                    case ImportHistoryType.TdcInstallmentRate:
                        fileName = "laisuattragophangnam.xlsx";
                        break;
                    case ImportHistoryType.TdcResettlement:
                        fileName = "chung-cu.xlsx";
                        break;
                    case ImportHistoryType.TdcLand:
                        fileName = "lo_tdc.xlsx";
                        break;
                    case ImportHistoryType.TdcBlockHouse:
                        fileName = "khoi-nha-tdc.xlsx";
                        break;
                    case ImportHistoryType.TdcFloor:
                        fileName = "tang-tdc.xlsx";
                        break;
                    case ImportHistoryType.TdcApartment:
                        fileName = "can_tdc.xlsx";
                        break;
                    case ImportHistoryType.TdcPlatform:
                        fileName = "can_tdc.xlsx";
                        break;
                    case ImportHistoryType.TdcProject:
                        fileName = "du_an_tdc.xlsx";
                        break;
                    case ImportHistoryType.TdcCustomer:
                        fileName = "khach_hang_tdc.xlsx";
                        break;
                    case ImportHistoryType.Tdc_ApartmentDataImport:
                        fileName = "import-ds-can.xlsx";
                        break;
                    case ImportHistoryType.Tdc_PlatformDataImport:
                        fileName = "impost-ds-nen.xlsx";
                        break;
                    default:
                        break;
                }

                string folderName = _configuration["AppSettings:BaseUrlImportTemplate"]; ;
                string webRootPath = _hostingEnvironment.WebRootPath;
                string newPath = Path.Combine(webRootPath, folderName);
                string fullPath = Path.Combine(newPath, fileName);

                if (fullPath != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                        using (FileStream file = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                        {
                            file.CopyTo(ms);
                        }

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
                log.Error("downloadFile:" + ex.Message);
                throw;
            }
        }

        //API download file đã import
        [HttpPost("downloadFileExcelHistory/{Id}")]
        public async Task<IActionResult> downloadFileExcelHistory(long Id)
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
                ImportHistory importHistory = _context.ImportHistories.Find(Id);

                string folderName = _configuration["AppSettings:BaseUrlImportHistory"]; ;
                string webRootPath = _hostingEnvironment.WebRootPath;
                string newPath = Path.Combine(webRootPath, folderName);
                string fullPath = Path.Combine(newPath, importHistory.FileUrl);

                if (fullPath != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                        using (FileStream file = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                        {
                            file.CopyTo(ms);
                        }

                        return File(ms.ToArray(), "application/octet-stream", importHistory.FileUrl);
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
                log.Error("downloadFile:" + ex.Message);
                throw;
            }
        }
    }
}
