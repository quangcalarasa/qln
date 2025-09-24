using IOITQln.Common.Enums;
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
using static IOITQln.Entities.Md167House;
using DevExpress.CodeParser;
using static DevExpress.XtraEditors.Filtering.DataItemsExtension;
using System.Globalization;
using DevExpress.ClipboardSource.SpreadsheetML;
using static IOITQln.Common.Constants.ApiConstants.MessageResource;
using ICSharpCode.SharpZipLib.Zip;

namespace IOITQln.Controllers.ApiMd167
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExportWordContractController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("GetExportWordContract1", "GetExportWordContract1");
        private static string functionCode = "Md167_CONTRACT_MANAGEMENT";
        private IWebHostEnvironment _hostingEnvironment;
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        public ExportWordContractController(ApiDbContext context, IMapper mapper, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        #region Hợp đồng 1
        [HttpPost("GetExportContract1/{id}")]
        public async Task<IActionResult> GetExportContract1(int id)
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
                        string fileName = "HopDong_MD167_01-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";

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
                log.Error("GetExportContract1:" + ex.Message);
                throw;
            }
        }

        private string insertDataToTemplate(int id)
        {
            Md167Contract md167Contract = _context.Md167Contracts.Where(l => l.Id == id && l.Type == Contract167Type.MAIN && l.Status != EntityStatus.DELETED).FirstOrDefault();
            Md167ContractData map_md167contract = _mapper.Map<Md167ContractData>(md167Contract);
            if(map_md167contract != null)
            {
                
                Md167House house = _context.Md167Houses.Where(f => f.Id == md167Contract.HouseId && (f.TypeHouse == Md167House.Type_House.House || f.TypeHouse == Md167House.Type_House.Kios) && f.Status != EntityStatus.DELETED).FirstOrDefault();
                Md167HouseData map_md167house = _mapper.Map<Md167HouseData>(house);
                Md167HouseInContractData housecontract = new Md167HouseInContractData();
                GroupMd167DebtData groupMd167DebtData = new GroupMd167DebtData();
                if (map_md167house != null)
                {
                    string templatePath = Path.Combine(_hostingEnvironment.WebRootPath, "exportword/HopDong_MD167_01.docx");

                    string fileName = "result" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";
                    RichEditDocumentServer wordProcessor = new RichEditDocumentServer();
                    wordProcessor.LoadDocumentTemplate(templatePath, DocumentFormat.OpenXml);
                    var document = wordProcessor.Document;
                    document.BeginUpdate();

                    int dayNow = DateTime.Now.Day;
                    int monthNow = DateTime.Now.Month;
                    int yearNow = DateTime.Now.Year;
                    decimal? totalAreaBuild = 0;
                    decimal? totalUseFloor = 0;
                    decimal? totalUseFloorApartmnet =0;
                    decimal? totalLand = 0;
                    decimal? floorbuild = 0;
                    totalAreaBuild = map_md167house.InfoValue.AreBuildPb + map_md167house.InfoValue.AreBuildPr;
                    totalUseFloorApartmnet = map_md167house.InfoValue.UseFloorPb + map_md167house.InfoValue.UseFloorPr;
                    totalLand = map_md167house.InfoValue.UseLandPb + map_md167house.InfoValue.UseLandPr;
                    floorbuild = map_md167house.InfoValue.AreaFloorBuild;

                    Md167House md167House = null;
                    if (map_md167house.TypeHouse == Md167House.Type_House.House)
                    {
                        //var isApplied = md167HouseTypes.Where(m => m.Id == dataItem.HouseTypeId).Select(x => x.IsApplied).FirstOrDefault();
                        //dataItem.Status = isApplied ? dataItem.Status : EntityStatus.DELETED;
                    }
                    else
                    {
                        md167House = _context.Md167Houses.Where(h => h.Id == map_md167house.Md167HouseId && h.Status != EntityStatus.DELETED).FirstOrDefault();
                        if (md167House != null)
                        {
                            map_md167house.ProvinceId = md167House.ProvinceId;
                            map_md167house.DistrictId = md167House.DistrictId;
                            map_md167house.WardId = md167House.WardId;
                            map_md167house.LaneId = md167House.LaneId;
                            map_md167house.InfoValue = md167House.InfoValue;
                        }
                    }

                    if (map_md167house.Status != EntityStatus.DELETED)
                    {
                        housecontract.Id = map_md167house.Id;
                        housecontract.Code = map_md167house.Code;
                        housecontract.HouseNumber = map_md167house.HouseNumber;
                        housecontract.ProvinceId = map_md167house.ProvinceId;
                        housecontract.DistrictId = map_md167house.DistrictId;
                        housecontract.WardId = map_md167house.WardId;
                        housecontract.LaneId = map_md167house.LaneId;
                        housecontract.UseLandPb = map_md167house.InfoValue.UseLandPb;
                        housecontract.UseLandPr = map_md167house.InfoValue.UseLandPr;
                        housecontract.AreaFloorBuild = map_md167house.InfoValue.AreaFloorBuild;
                        housecontract.TypeHouse = map_md167house.TypeHouse;
                        housecontract.ParentHouseNumber = map_md167house.HouseNumber;
                        housecontract.AreBuildPb = map_md167house.InfoValue.AreBuildPb;
                        housecontract.AreBuildPr = map_md167house.InfoValue.AreBuildPr;
                        housecontract.UseFloorPb = map_md167house.InfoValue.UseFloorPb;
                        housecontract.UseFloorPr = map_md167house.InfoValue.UseFloorPr;


                        totalAreaBuild = housecontract.AreBuildPb + housecontract.AreBuildPr;
                        totalUseFloor = housecontract.UseFloorPb + housecontract.UseFloorPr;
                        totalLand = housecontract.UseLandPb + housecontract.UseLandPr;
                        floorbuild = housecontract.AreaFloorBuild;
                        //diện tích ở điều 1 hợp đồng 1
                        //cho nhà phố
                        if(housecontract.TypeHouse == Type_House.House)
                        {
                            document.ReplaceAll("<areafloorbuild>", housecontract != null ? totalAreaBuild.ToString() : "", SearchOptions.None);
                            document.ReplaceAll("<usefloor>", housecontract != null ? totalUseFloor.ToString() : "", SearchOptions.None);
                            document.ReplaceAll("<useland>", housecontract != null ? totalLand.ToString() : "", SearchOptions.None);
                        }
                        //cho nhà chung cư/nhà nhiều tầng
                        else
                        {
                            document.ReplaceAll("<areafloorbuild>", housecontract != null ? floorbuild.ToString() : "", SearchOptions.None);
                            document.ReplaceAll("<usefloor>", map_md167house != null ? totalUseFloorApartmnet.ToString() : "", SearchOptions.None);
                            document.ReplaceAll("<useland>", housecontract != null ? totalLand.ToString() : "", SearchOptions.None);
                        }
                    }
                    else
                    {
                        return null;
                    }

                    CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");

                    //ngày hiện tại
                    string day = dayNow.ToString();
                    document.ReplaceAll("<day>", day, SearchOptions.None);
                    //tháng hiện tại
                    string month = monthNow.ToString();
                    document.ReplaceAll("<month>", month, SearchOptions.None);
                    //năm hiện tại
                    string year = yearNow.ToString();
                    document.ReplaceAll("<year>", year, SearchOptions.None);
                    //ngày/tháng/năm hiện tại
                    document.ReplaceAll("<dateNow>", day + "/" + month + "/" + year, SearchOptions.None);
                    // số nhà
                    document.ReplaceAll("<housenumber>", map_md167house.HouseNumber ?? "", SearchOptions.None);
                    // ngày ký hợp đồng
                    document.ReplaceAll("<datesign>", map_md167contract.DateSign.ToString("dd/MM/yyyy") ?? "", SearchOptions.None);

                    var pricePerMonths = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == map_md167contract.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(x => x.DateEffect).FirstOrDefault();
                    //tính tiền thế chân= tỏng tiền thuê x3
                    decimal? depositprice = pricePerMonths.TotalPrice * 3;
                    //tiền thuê ở điều 2 hợp đồng
                    if (pricePerMonths != null)
                    {
                        document.ReplaceAll("<amouttobeoaid>", depositprice?.ToString("#,###", cul.NumberFormat) ?? "", SearchOptions.None);
                        document.ReplaceAll("<totalprice>", pricePerMonths.TotalPrice?.ToString("#,###", cul.NumberFormat) ?? "", SearchOptions.None);
                        document.ReplaceAll("<totalpricebangchu>", UtilsService.ConvertMoneyToString(pricePerMonths.TotalPrice.ToString()).ToLower(), SearchOptions.None);
                        document.ReplaceAll("<houseprice>", pricePerMonths.HousePrice.ToString("#,###", cul.NumberFormat) ?? "", SearchOptions.None);
                        document.ReplaceAll("<landprice>", pricePerMonths.LandPrice?.ToString("#,###", cul.NumberFormat) ?? "", SearchOptions.None);
                        document.ReplaceAll("<vatprice>", pricePerMonths.VatPrice.ToString("#,###", cul.NumberFormat) ?? "", SearchOptions.None);
                    }
                    else
                    {
                        string replacementText = "......";
                        document.ReplaceAll("<amouttobeoaid>", replacementText, SearchOptions.None);
                        document.ReplaceAll("<totalprice>", replacementText, SearchOptions.None);
                        document.ReplaceAll("<houseprice>", replacementText, SearchOptions.None);
                        document.ReplaceAll("<landprice>", replacementText, SearchOptions.None);
                        document.ReplaceAll("<vatprice>", replacementText, SearchOptions.None);
                    }

                    

                    string GetPurposeString(RentalPurposeContract167 purpose)
                    {
                        switch (purpose)
                        {
                            case RentalPurposeContract167.KINH_DOANH_DV:
                                return "Kinh doanh dịch vụ";
                            case RentalPurposeContract167.CO_SO_SX:
                                return "Cơ sở sản xuất";
                            case RentalPurposeContract167.KHO_BAI:
                                return "Kho bãi";
                            default:
                                return "";
                        }
                    }
                    //mục đích sử dụng nhà
                    if (map_md167contract.RentalPurpose == RentalPurposeContract167.CO_SO_SX)
                    {
                        string replacementText = GetPurposeString(map_md167contract.RentalPurpose) ?? "Cơ sở sản xuất";
                        document.ReplaceAll("<purposeusing>", replacementText, SearchOptions.None);

                    }
                    else if(map_md167contract.RentalPurpose == RentalPurposeContract167.KINH_DOANH_DV)
                    {
                        string replacementText = GetPurposeString(map_md167contract.RentalPurpose) ?? "Kinh doanh dịch vụ";
                        document.ReplaceAll("<purposeusing>", replacementText, SearchOptions.None);
                    }
                    else if (map_md167contract.RentalPurpose == RentalPurposeContract167.KHO_BAI)
                    {
                        string replacementText = GetPurposeString(map_md167contract.RentalPurpose) ?? "Kho bãi";
                        document.ReplaceAll("<purposeusing>", replacementText, SearchOptions.None);
                    }
                    else if (map_md167contract.RentalPurpose == RentalPurposeContract167.KHAC)
                    {
                        string replacementText = map_md167contract.RentalPurpose.ToString() ?? "";
                        document.ReplaceAll("<purposeusing>", replacementText, SearchOptions.None);
                    }
                    else
                    {
                        return null;
                    }

                    string GetPeriodString(RentalPeriodContract167 purpose)
                    {
                        switch (purpose)
                        {
                            case RentalPeriodContract167.TAM_BO_TRI:
                                return "Tạm bố trí";
                            case RentalPeriodContract167.THUE_1_NAM:
                                return "Thuê 1 năm";
                            case RentalPeriodContract167.THUE_5_NAM:
                                return "Thuê 5 năm";
                            default:
                                return "";
                        }
                    }
                    // thời hạn thuê
                    if (map_md167contract.RentalPeriod == RentalPeriodContract167.TAM_BO_TRI)
                    {
                        string replacementText = GetPeriodString(map_md167contract.RentalPeriod) ?? "Tạm bố trí";
                        document.ReplaceAll("<rentalperiod>", replacementText, SearchOptions.None);
                    }
                    else if (map_md167contract.RentalPeriod == RentalPeriodContract167.THUE_1_NAM)
                    {
                        string replacementText = GetPeriodString(map_md167contract.RentalPeriod) ?? "Thuê 1 năm";
                        document.ReplaceAll("<rentalperiod>", replacementText, SearchOptions.None);
                    }
                    else if (map_md167contract.RentalPeriod == RentalPeriodContract167.THUE_5_NAM)
                    {
                        string replacementText = GetPeriodString(map_md167contract.RentalPeriod) ?? "Thuê 5 năm";
                        document.ReplaceAll("<rentalperiod>", replacementText, SearchOptions.None);
                    }
                    else if (map_md167contract.RentalPeriod == RentalPeriodContract167.KHAC)
                    {
                        string replacementText = map_md167contract.RentalPeriod.ToString() ?? "";
                        document.ReplaceAll("<rentalperiod>", replacementText, SearchOptions.None);
                    }
                    else
                    {
                        return null;
                    }
                    // thông tin về bên thuê như địa chỉ, tổ chức hay cá nhân, mã thuế, ...
                    Province province = _context.Provincies.Where(l => l.Id == map_md167house.ProvinceId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    document.ReplaceAll("<province>", province != null ? province.Name : "", SearchOptions.None);

                    District district = _context.Districts.Where(l => l.Id == map_md167house.DistrictId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    document.ReplaceAll("<district>", district != null ? district.Name : "", SearchOptions.None);

                    Ward ward = _context.Wards.Where(l => l.Id == map_md167house.WardId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    document.ReplaceAll("<ward>", ward != null ? ward.Name : "", SearchOptions.None);

                    Lane lane = _context.Lanies.Where(l => l.Id == map_md167house.LaneId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    document.ReplaceAll("<lane>", lane != null ? lane.Name : "", SearchOptions.None);

                    if (map_md167contract.personOrCompany == true)
                    {
                        Md167Delegate md167Delegate = _context.Md167Delegates.Where(l => l.Id == map_md167contract.DelegateId && l.PersonOrCompany == personOrCompany.COMPANY && l.Status != EntityStatus.DELETED).FirstOrDefault();
                        document.ReplaceAll("<namecompany>", md167Delegate != null ? md167Delegate.Name : "", SearchOptions.None);
                        document.ReplaceAll("<namepersonal>", md167Delegate != null ? md167Delegate.ComOrganizationRepresentativeName : "", SearchOptions.None);
                        document.ReplaceAll("<address>", md167Delegate != null ? md167Delegate.Address : "", SearchOptions.None);
                        document.ReplaceAll("<composition>", md167Delegate != null ? md167Delegate.ComPosition : "", SearchOptions.None);
                        document.ReplaceAll("<combusinesslicense>", md167Delegate != null ? md167Delegate.ComBusinessLicense : "", SearchOptions.None);
                        document.ReplaceAll("<comtaxnumber>", md167Delegate != null ? md167Delegate.ComTaxNumber : "", SearchOptions.None);
                        document.ReplaceAll("<phonenumber>", md167Delegate != null ? md167Delegate.PhoneNumber : "", SearchOptions.None);

                    }
                    else if (map_md167contract.personOrCompany == false)
                    {
                        Md167Delegate md167Delegate = _context.Md167Delegates.Where(l => l.Id == map_md167contract.DelegateId && l.PersonOrCompany == personOrCompany.PERSON && l.Status != EntityStatus.DELETED).FirstOrDefault();
                        document.ReplaceAll("<namecompany>", "", SearchOptions.None);
                        document.ReplaceAll("<namepersonal>", md167Delegate != null ? md167Delegate.Name : "", SearchOptions.None);
                        document.ReplaceAll("<address>", md167Delegate != null ? md167Delegate.Address : "", SearchOptions.None);
                        document.ReplaceAll("<composition>", "", SearchOptions.None);
                        document.ReplaceAll("<combusinesslicense>", "", SearchOptions.None);
                        document.ReplaceAll("<comtaxnumber>", "", SearchOptions.None);
                        document.ReplaceAll("<phonenumber>", md167Delegate != null ? md167Delegate.PhoneNumber : "", SearchOptions.None);

                    }
                    else
                    {
                        return null;
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
            else
            {
                return null;
            }
        }
        #endregion

        #region Hợp đồng 2
        [HttpPost("GetExportContract2/{id}")]
        public async Task<IActionResult> GetExportContract2(int id)
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
                string path = insertDataToTemplate2(id);
                if (path != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                        string fileName = "HopDong_MD167_02-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";

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
                log.Error("GetExportContract2:" + ex.Message);
                throw;
            }
        }

        private string insertDataToTemplate2(int id)
        {
            Md167Contract md167ContractExtra = _context.Md167Contracts.Where(l=>l.Id == id  && l.Type == Contract167Type.EXTRA && l.Status != EntityStatus.DELETED).FirstOrDefault();
            Md167ContractData mapExtraData = _mapper.Map<Md167ContractData>(md167ContractExtra);
            if(mapExtraData != null)
            {
                Md167Contract md167Contract = _context.Md167Contracts.Where(l => l.Id == mapExtraData.ParentId && l.Type == Contract167Type.MAIN && l.Status != EntityStatus.DELETED).FirstOrDefault();
                Md167ContractData map_md167contract = _mapper.Map<Md167ContractData>(md167Contract);
                if (map_md167contract != null)
                {
                    Md167House house = _context.Md167Houses.Where(f => f.Id == md167ContractExtra.HouseId && (f.TypeHouse == Md167House.Type_House.House || f.TypeHouse == Md167House.Type_House.Kios) && f.Status != EntityStatus.DELETED).FirstOrDefault();
                    Md167HouseData map_md167house = _mapper.Map<Md167HouseData>(house);
                    Md167HouseInContractData housecontract = new Md167HouseInContractData();
                    if (map_md167house != null)
                    {
                        string templatePath = Path.Combine(_hostingEnvironment.WebRootPath, "exportword/HopDong_MD167_02.docx");

                        string fileName = "result" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";
                        RichEditDocumentServer wordProcessor = new RichEditDocumentServer();
                        wordProcessor.LoadDocumentTemplate(templatePath, DocumentFormat.OpenXml);
                        var document = wordProcessor.Document;
                        document.BeginUpdate();

                        int dayNow = DateTime.Now.Day;
                        int monthNow = DateTime.Now.Month;
                        int yearNow = DateTime.Now.Year;

                        Md167House md167House = null;
                        if (map_md167house.TypeHouse == Md167House.Type_House.House)
                        {
                            //var isApplied = md167HouseTypes.Where(m => m.Id == dataItem.HouseTypeId).Select(x => x.IsApplied).FirstOrDefault();
                            //dataItem.Status = isApplied ? dataItem.Status : EntityStatus.DELETED;
                        }
                        else
                        {
                            md167House = _context.Md167Houses.Where(h => h.Id == map_md167house.Md167HouseId && h.Status != EntityStatus.DELETED).FirstOrDefault();
                            if (md167House != null)
                            {
                                map_md167house.ProvinceId = md167House.ProvinceId;
                                map_md167house.DistrictId = md167House.DistrictId;
                                map_md167house.WardId = md167House.WardId;
                                map_md167house.LaneId = md167House.LaneId;
                                map_md167house.InfoValue = md167House.InfoValue;
                            }
                        }

                        if (map_md167house.Status != EntityStatus.DELETED)
                        {
                            housecontract.Id = map_md167house.Id;
                            housecontract.Code = map_md167house.Code;
                            housecontract.HouseNumber = map_md167house.HouseNumber;
                            housecontract.ProvinceId = map_md167house.ProvinceId;
                            housecontract.DistrictId = map_md167house.DistrictId;
                            housecontract.WardId = map_md167house.WardId;
                            housecontract.LaneId = map_md167house.LaneId;
                            housecontract.UseLandPb = map_md167house.InfoValue.UseLandPb;
                            housecontract.UseLandPr = map_md167house.InfoValue.UseLandPr;
                            housecontract.AreaFloorBuild = map_md167house.InfoValue.AreaFloorBuild;
                            housecontract.TypeHouse = map_md167house.TypeHouse;
                            housecontract.ParentHouseNumber = map_md167house.HouseNumber;
                            housecontract.AreBuildPb = map_md167house.InfoValue.AreBuildPb;
                            housecontract.AreBuildPr = map_md167house.InfoValue.AreBuildPr;
                            housecontract.UseFloorPb = map_md167house.InfoValue.UseFloorPb;
                            housecontract.UseFloorPr = map_md167house.InfoValue.UseFloorPr;


                        }
                        else
                        {
                            return null;
                        }

                        //ngày hiện tại
                        string day = dayNow.ToString();
                        document.ReplaceAll("<day>", day, SearchOptions.None);
                        //tháng hiện tại
                        string month = monthNow.ToString();
                        document.ReplaceAll("<month>", month, SearchOptions.None);
                        //năm hiện tại
                        string year = yearNow.ToString();
                        document.ReplaceAll("<year>", year, SearchOptions.None);
                        //ngày/tháng/năm hiện tại
                        document.ReplaceAll("<dateNow>", day + "/" + month + "/" + year, SearchOptions.None);
                        // số nhà
                        document.ReplaceAll("<housenumber>", map_md167house.HouseNumber ?? "", SearchOptions.None);
                        // số hợp đồng
                        document.ReplaceAll("<code>", map_md167contract.Code ?? "", SearchOptions.None);
                        // ngày ký hợp đồng
                        document.ReplaceAll("<datesign>", map_md167contract.DateSign.ToString("dd/MM/yyyy") ?? "", SearchOptions.None);


                        if (map_md167contract.personOrCompany == true)
                        {
                            Md167Delegate md167Delegate = _context.Md167Delegates.Where(l => l.Id == map_md167contract.DelegateId && l.PersonOrCompany == personOrCompany.COMPANY && l.Status != EntityStatus.DELETED).FirstOrDefault();
                            document.ReplaceAll("<namecompany>", md167Delegate != null ? md167Delegate.Name : "", SearchOptions.None);
                            document.ReplaceAll("<namepersonal>", md167Delegate != null ? md167Delegate.ComOrganizationRepresentativeName : "", SearchOptions.None);
                            document.ReplaceAll("<address>", md167Delegate != null ? md167Delegate.Address : "", SearchOptions.None);
                            document.ReplaceAll("<composition>", md167Delegate != null ? md167Delegate.ComPosition : "", SearchOptions.None);
                            document.ReplaceAll("<combusinesslicense>", md167Delegate != null ? md167Delegate.ComBusinessLicense : "", SearchOptions.None);
                            document.ReplaceAll("<comtaxnumber>", md167Delegate != null ? md167Delegate.ComTaxNumber : "", SearchOptions.None);
                            document.ReplaceAll("<phonenumber>", md167Delegate != null ? md167Delegate.PhoneNumber : "", SearchOptions.None);

                        }
                        else if (map_md167contract.personOrCompany == false)
                        {
                            Md167Delegate md167Delegate = _context.Md167Delegates.Where(l => l.Id == map_md167contract.DelegateId && l.PersonOrCompany == personOrCompany.PERSON && l.Status != EntityStatus.DELETED).FirstOrDefault();
                            document.ReplaceAll("<namecompany>", "", SearchOptions.None);
                            document.ReplaceAll("<namepersonal>", md167Delegate != null ? md167Delegate.Name : "", SearchOptions.None);
                            document.ReplaceAll("<address>", md167Delegate != null ? md167Delegate.Address : "", SearchOptions.None);
                            document.ReplaceAll("<composition>", "", SearchOptions.None);
                            document.ReplaceAll("<combusinesslicense>", "", SearchOptions.None);
                            document.ReplaceAll("<comtaxnumber>", "", SearchOptions.None);
                            document.ReplaceAll("<phonenumber>", md167Delegate != null ? md167Delegate.PhoneNumber : "", SearchOptions.None);

                        }
                        else
                        {
                            return null;
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
                else
                {
                    return null;

                }

            }
            else
            {
                return null;
            }
        }

        #endregion

        #region hợp đồng 3
        [HttpPost("GetExportContract3/{id}")]
        public async Task<IActionResult> GetExportContract3(int id)
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
                string path = insertDataToTemplate3(id);
                if (path != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                        string fileName = "HopDong_MD167_03-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";

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
                log.Error("GetExportContract3:" + ex.Message);
                throw;
            }
        }

        private string insertDataToTemplate3(int id)
        {
            Md167Contract md167Contract = _context.Md167Contracts.Where(l => l.Id == id && l.Type == Contract167Type.MAIN && l.Status != EntityStatus.DELETED).FirstOrDefault();
            Md167ContractData map_md167contract = _mapper.Map<Md167ContractData>(md167Contract);
            if (map_md167contract != null)
            {
                Md167House house = _context.Md167Houses.Where(f => f.Id == md167Contract.HouseId && (f.TypeHouse == Md167House.Type_House.House || f.TypeHouse == Md167House.Type_House.Kios) && f.Status != EntityStatus.DELETED).FirstOrDefault();
                Md167HouseData map_md167house = _mapper.Map<Md167HouseData>(house);
                Md167HouseInContractData housecontract = new Md167HouseInContractData();
                GroupMd167DebtData groupMd167DebtData = new GroupMd167DebtData();
                if(map_md167house != null)
                {
                    string templatePath = Path.Combine(_hostingEnvironment.WebRootPath, "exportword/HopDong_MD167_03.docx");

                    string fileName = "result" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";
                    RichEditDocumentServer wordProcessor = new RichEditDocumentServer();
                    wordProcessor.LoadDocumentTemplate(templatePath, DocumentFormat.OpenXml);
                    var document = wordProcessor.Document;
                    document.BeginUpdate();

                    int dayNow = DateTime.Now.Day;
                    int monthNow = DateTime.Now.Month;
                    int yearNow = DateTime.Now.Year;

                    //ngày hiện tại
                    string day = dayNow.ToString();
                    document.ReplaceAll("<day>", day, SearchOptions.None);
                    //tháng hiện tại
                    string month = monthNow.ToString();
                    document.ReplaceAll("<month>", month, SearchOptions.None);
                    //năm hiện tại
                    string year = yearNow.ToString();
                    document.ReplaceAll("<year>", year, SearchOptions.None);
                    //ngày/tháng/năm hiện tại
                    document.ReplaceAll("<dateNow>", day + "/" + month + "/" + year, SearchOptions.None);
                    // số nhà
                    document.ReplaceAll("<code>", map_md167contract.Code ?? "", SearchOptions.None);
                    // ngày ký hợp đồng
                    document.ReplaceAll("<datesign>", map_md167contract.DateSign.ToString("dd/MM/yyyy") ?? "", SearchOptions.None);

                    if (map_md167contract.personOrCompany == true)
                    {
                        Md167Delegate md167Delegate = _context.Md167Delegates.Where(l => l.Id == map_md167contract.DelegateId && l.PersonOrCompany == personOrCompany.COMPANY && l.Status != EntityStatus.DELETED).FirstOrDefault();
                        document.ReplaceAll("<namepersonal>", md167Delegate != null ? md167Delegate.ComOrganizationRepresentativeName : "", SearchOptions.None);
                        document.ReplaceAll("<address>", md167Delegate != null ? md167Delegate.Address : "", SearchOptions.None);
                        document.ReplaceAll("<composition>", md167Delegate != null ? md167Delegate.ComPosition : "", SearchOptions.None);
                        document.ReplaceAll("<combusinesslicense>", md167Delegate != null ? md167Delegate.ComBusinessLicense : "", SearchOptions.None);
                        document.ReplaceAll("<comtaxnumber>", md167Delegate != null ? md167Delegate.ComTaxNumber : "", SearchOptions.None);
                        document.ReplaceAll("<phonenumber>", md167Delegate != null ? md167Delegate.PhoneNumber : "", SearchOptions.None);

                    }
                    else if (map_md167contract.personOrCompany == false)
                    {
                        Md167Delegate md167Delegate = _context.Md167Delegates.Where(l => l.Id == map_md167contract.DelegateId && l.PersonOrCompany == personOrCompany.PERSON && l.Status != EntityStatus.DELETED).FirstOrDefault();
                        document.ReplaceAll("<namepersonal>", md167Delegate != null ? md167Delegate.Name : "", SearchOptions.None);
                        document.ReplaceAll("<address>", md167Delegate != null ? md167Delegate.Address : "", SearchOptions.None);
                        document.ReplaceAll("<composition>", "", SearchOptions.None);
                        document.ReplaceAll("<combusinesslicense>", "", SearchOptions.None);
                        document.ReplaceAll("<comtaxnumber>", "", SearchOptions.None);
                        document.ReplaceAll("<phonenumber>", md167Delegate != null ? md167Delegate.PhoneNumber : "", SearchOptions.None);

                    }
                    else
                    {
                        return null;
                    }



                    List<GroupMd167DebtData> res = null;
                    var pricePerMonths = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == map_md167contract.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                    List<Md167ProfitValue> profitValues = _context.Md167ProfitValues.Where(p => p.Status != EntityStatus.DELETED).ToList();
                    List<Md167Receipt> md167Receipts = _context.Md167Receipts.Where(m => m.Md167ContractId == map_md167contract.Id && m.PaidDeposit != true && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();

                    List<Md167Receipt> depositMd167Receipts = _context.Md167Receipts.Where(m => m.Md167ContractId == map_md167contract.Id && m.PaidDeposit == true && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();

                    //Tìm hợp đồng liên quan để lấy tiền thế chân
                    var pricePerMonth = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == map_md167contract.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).FirstOrDefault();
                    res = GetDataDebtFunc(document, pricePerMonths, profitValues, md167Receipts, map_md167contract, pricePerMonth?.TotalPrice * 3, depositMd167Receipts);

                    document.EndUpdate();
                    wordProcessor.SaveDocument(fileName, DocumentFormat.OpenXml);
                    return fileName;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static List<GroupMd167DebtData> GetDataDebtFunc(Document document, List<Md167PricePerMonth> pricePerMonths, List<Md167ProfitValue> profitValues, List<Md167Receipt> md167Receipts, Md167Contract md167Contract, decimal? deposit, List<Md167Receipt> depositMd167Receipts)
        {
            List<Md167Debt> res = new List<Md167Debt>();


            if (md167Contract != null)
            {
                var pricePerMonth = pricePerMonths.Count > 0 ? pricePerMonths[pricePerMonths.Count - 1] : null;

                //Ngày bắt đầu thuê
                var dateStart = md167Contract.DateSign;
                //Ngày kết thúc
                var dateEnd = DateTime.Now;
                //var dateEnd = new DateTime(2016,11,11);

                //số năm
                DateTime zeroTime = new DateTime(1, 1, 1);
                TimeSpan span = dateEnd - dateStart;
                int years = (zeroTime + span).Year + 1;

                //tháng bắt đầu hợp đồng
                var month = dateStart.Month;
                var year = dateStart.Year;

                //Thêm dòng tiền thế chân
                Md167Debt ttcMd167Debt = new Md167Debt();
                ttcMd167Debt.TypeRow = Md167DebtTypeRow.DONG_THE_CHAN;
                ttcMd167Debt.Title = "Tiền thế chân";
                ttcMd167Debt.AmountToBePaid = deposit;
                ttcMd167Debt.AmountPaid = depositMd167Receipts.Sum(x => x.Amount ?? 0);
                ttcMd167Debt.AmountDiff = ttcMd167Debt.AmountToBePaid - ttcMd167Debt.AmountPaid;

                res.Add(ttcMd167Debt);

                //lặp các năm
                bool breakloop = false;
                decimal amountDiff = 0;
                decimal interest = 0.02m;
                DateTime lastPaymentDate = dateStart;       //Ngày thanh toán gần nhất
                DateTime? prevDop = null;       //Ngày thanh toán theo quy định của kỳ đằng trước

                bool? firstRowNullPayment = false;

                int? periodIndex = null;

                for (int i = 1; i <= years; i++)
                {
                    if (breakloop == true)
                    {
                        break;
                    }

                    //Thêm dòng tên năm
                    Md167Debt md167Debt = new Md167Debt();
                    md167Debt.TypeRow = Md167DebtTypeRow.DONG_NAM;
                    md167Debt.Title = $"Năm thứ {i}";
                    var md167ReceiptId = md167Receipts.Where(m => m.Amount != 0 && m.Status == EntityStatus.TEMP).OrderBy(m => m.DateOfReceipt).FirstOrDefault()?.Id;
                    if (md167ReceiptId != null)
                    {
                        md167Debt.Md167ReceiptId = md167ReceiptId;
                    }
                    else
                    {
                        if (md167Receipts.Where(m => m.Amount != 0 && m.Status != EntityStatus.TEMP).OrderBy(m => m.DateOfReceipt).FirstOrDefault() != null)
                        {
                            md167Debt.Md167ReceiptId = -i;
                        }
                        else
                        {
                            md167Debt.Md167ReceiptId = null;
                        }
                    }

                    //md167Debt.DopActual = new DateTime(md167Contract.DateSign.Year + i - 1, 1, 1);
                    res.Add(md167Debt);

                    //Lặp các tháng thanh toán trong 1 năm
                    //Kiểm tra kỳ thanh toán là theo tháng hay theo quý
                    int step = md167Contract.PaymentPeriod == PaymentPeriodContract167.THANG ? 1 : 3;
                    int index = 1;
                    for (int j = month; j <= 12; j = j + step)
                    {
                        if (i == 1 && j == dateStart.Month)
                        {
                            //thêm dòng đầu tiên
                            Md167Debt firstMd167Debt = new Md167Debt();
                            firstMd167Debt.TypeRow = Md167DebtTypeRow.DONG_DU_LIEU;
                            firstMd167Debt.Index = index;
                            firstMd167Debt.Dop = new DateTime(year, month, 1);
                            firstMd167Debt.AmountPaidPerMonth = pricePerMonth != null ? pricePerMonth.TotalPrice : null;
                            firstMd167Debt.AmountToBePaid = firstMd167Debt.AmountPaidPerMonth;
                            //QFix:Fix bug thanh toán tháng đầu
                            // firstMd167Debt.AmountPaidInPeriod = firstMd167Debt.AmountPaidPerMonth;
                            // firstMd167Debt.AmountPaid = firstMd167Debt.AmountPaidInPeriod;
                            // firstMd167Debt.AmountDiff = null;
                            firstMd167Debt.AmountPaidInPeriod = 0; // Không tự động thanh toán tháng đầu
                            firstMd167Debt.AmountPaid = 0; // Không tự động thanh toán tháng đầu
                            firstMd167Debt.AmountDiff = firstMd167Debt.AmountToBePaid; // Còn nợ toàn bộ số tiền

                            res.Add(firstMd167Debt);
                        }
                        else
                        {
                            var dop = new DateTime(year + i - 1, j, 1);
                            if (dop > dateEnd)
                            {
                                breakloop = true;
                                break;
                            }

                            Md167Debt newMd167Debt = new Md167Debt();
                            newMd167Debt.TypeRow = Md167DebtTypeRow.DONG_DU_LIEU;
                            newMd167Debt.Index = index;
                            newMd167Debt.Dop = dop;

                            //Tính lãi thuê áp dụng
                            var profitValue = profitValues.Where(p => p.DoApply <= dop).OrderByDescending(x => x.DoApply).FirstOrDefault();

                            interest = profitValue != null ? (decimal)profitValue.Value : interest;
                            newMd167Debt.Interest = (float)interest;

                            //Tìm số tiền phải trả hàng tháng
                            var pricePerMonthInPeriod = pricePerMonths.Where(e => e.DateEffect <= dop).OrderBy(x => x.UpdatedAt).FirstOrDefault();
                            if (pricePerMonthInPeriod == null)
                            {
                                newMd167Debt.AmountPaidPerMonth = pricePerMonth != null ? pricePerMonth.TotalPrice : null;
                            }
                            else
                            {
                                newMd167Debt.AmountPaidPerMonth = pricePerMonthInPeriod.TotalPrice;
                            }
                            if (newMd167Debt.AmountPaidPerMonth == null) newMd167Debt.AmountPaidPerMonth = 0;
                            //Kiểm tra có kỳ thanh toán hay không
                            Md167Receipt md167Receipt = md167Receipts.Where(m => m.Amount != 0).OrderBy(m => m.DateOfReceipt).FirstOrDefault();
                            if (md167Receipt != null)
                            {
                                int idx = md167Receipts.IndexOf(md167Receipt);
                                md167Receipts[idx].Status = EntityStatus.TEMP;
                                newMd167Debt.DopActual = md167Receipt.DateOfReceipt;      //Ngày thanh toán thực tế
                                if (newMd167Debt.DopActual > newMd167Debt.Dop)
                                {
                                    newMd167Debt.InterestCalcDate = (int)Math.Ceiling((newMd167Debt.DopActual - newMd167Debt.Dop).Value.TotalDays);
                                    newMd167Debt.AmountInterest = newMd167Debt.AmountPaidPerMonth * newMd167Debt.InterestCalcDate * (interest / 100);
                                }
                            }
                            else
                            {
                                newMd167Debt.DopActual = dateEnd;      //Ngày thanh toán thực tế
                                newMd167Debt.InterestCalcDate = (int)Math.Ceiling((newMd167Debt.DopActual - newMd167Debt.Dop).Value.TotalDays);
                                newMd167Debt.AmountInterest = newMd167Debt.AmountPaidPerMonth * newMd167Debt.InterestCalcDate * (interest / 100);

                                if (firstRowNullPayment == false) firstRowNullPayment = true;
                                else firstRowNullPayment = null;
                            }

                            newMd167Debt.AmountPaidInPeriod = newMd167Debt.AmountPaidPerMonth + (newMd167Debt.AmountInterest ?? 0);
                            //newMd167Debt.AmountToBePaid = newMd167Debt.AmountPaidInPeriod + (amountDiff < 0 ? amountDiff : 0);
                            newMd167Debt.AmountToBePaid = newMd167Debt.AmountPaidInPeriod;
                            if (amountDiff > 0 && md167Receipt != null && md167Receipt?.DateOfReceipt <= newMd167Debt.Dop)
                            {
                                newMd167Debt.AmountToBePaid += amountDiff;
                            }

                            decimal amountDiffInPeriod = 0;         //Nợ của kỳ này
                            if (md167Receipt != null)
                            {
                                int idx = md167Receipts.IndexOf(md167Receipt);
                                //md167Receipts[idx].Amount = amountDiff < 0 ? md167Receipts[idx].Amount + amountDiff : md167Receipts[idx].Amount;
                                amountDiffInPeriod = (decimal)newMd167Debt.AmountToBePaid - (decimal)md167Receipt.Amount;
                                if (amountDiffInPeriod >= 0)
                                {
                                    newMd167Debt.AmountPaid = md167Receipt.Amount;
                                    md167Receipts[idx].Amount = 0;
                                    if (amountDiffInPeriod > 0) newMd167Debt.AmountDiff = amountDiffInPeriod;
                                    else newMd167Debt.AmountDiff = null;
                                }
                                else
                                {
                                    newMd167Debt.AmountPaid = newMd167Debt.AmountToBePaid;
                                    md167Receipts[idx].Amount = md167Receipts[idx].Amount - newMd167Debt.AmountToBePaid;
                                    newMd167Debt.AmountDiff = null;

                                    //if (md167Receipts[idx].Amount >= newMd167Debt.AmountToBePaid)
                                    //{

                                    //}
                                    //else
                                    //{
                                    //    newMd167Debt.AmountPaid = md167Receipts[idx].Amount;
                                    //    md167Receipts[idx].Amount = 0;
                                    //    newMd167Debt.AmountDiff = md167Receipts[idx].Amount + amountDiffInPeriod;
                                    //}
                                }

                                newMd167Debt.Md167ReceiptId = md167Receipt.Id;
                            }
                            else
                            {
                                amountDiffInPeriod = (decimal)newMd167Debt.AmountToBePaid;
                                newMd167Debt.AmountDiff = amountDiffInPeriod;
                            }

                            res.Add(newMd167Debt);


                            //Kiểm tra nợ cũ
                            if (amountDiff > 0 && ((md167Receipt == null && firstRowNullPayment == true) || (md167Receipt != null && md167Receipt.DateOfReceipt > newMd167Debt.Dop)))
                            //if (amountDiff > 0 && md167Receipt != null && md167Receipt.DateOfReceipt > newMd167Debt.Dop)
                            {
                                Md167Debt ncMd167Debt = new Md167Debt();
                                ncMd167Debt.Title = "Nợ cũ kỳ thanh toán " + periodIndex;
                                ncMd167Debt.TypeRow = Md167DebtTypeRow.DONG_NO_CU;
                                ncMd167Debt.Dop = dop;
                                ncMd167Debt.Interest = (float)interest;

                                if (md167Receipt != null)
                                {
                                    ncMd167Debt.DopActual = md167Receipt.DateOfReceipt;      //Ngày thanh toán thực tế
                                    if (ncMd167Debt.DopActual > lastPaymentDate)
                                    {
                                        ncMd167Debt.InterestCalcDate = (int)Math.Ceiling((ncMd167Debt.DopActual - lastPaymentDate).Value.TotalDays);
                                        ncMd167Debt.AmountInterest = amountDiff * ncMd167Debt.InterestCalcDate * (interest / 100);
                                    }
                                }
                                else
                                {
                                    ncMd167Debt.DopActual = dateEnd;      //Ngày thanh toán thực tế
                                    ncMd167Debt.InterestCalcDate = (int)Math.Ceiling((ncMd167Debt.DopActual - (prevDop != null ? prevDop : newMd167Debt.Dop)).Value.TotalDays);
                                    ncMd167Debt.AmountInterest = Math.Ceiling(amountDiff * (decimal)ncMd167Debt.InterestCalcDate * (interest / 100));
                                }

                                ncMd167Debt.AmountPaidInPeriod = ncMd167Debt.AmountInterest ?? 0;
                                ncMd167Debt.AmountToBePaid = ncMd167Debt.AmountPaidInPeriod + amountDiff;

                                decimal amountDiffInPeriodNc = 0;         //Nợ của kỳ này của dòng nợ cũ

                                if (md167Receipt != null)
                                {
                                    int idx = md167Receipts.IndexOf(md167Receipt);
                                    amountDiffInPeriodNc = (decimal)ncMd167Debt.AmountToBePaid - (decimal)md167Receipt.Amount;
                                    if (amountDiffInPeriodNc >= 0)
                                    {
                                        ncMd167Debt.AmountPaid = md167Receipt.Amount;
                                        md167Receipts[idx].Amount = 0;
                                        ncMd167Debt.AmountDiff = amountDiffInPeriodNc;
                                    }
                                    else
                                    {
                                        ncMd167Debt.AmountPaid = ncMd167Debt.AmountToBePaid;
                                        md167Receipts[idx].Amount = -amountDiffInPeriodNc;
                                        ncMd167Debt.AmountDiff = amountDiffInPeriodNc;
                                    }

                                    ncMd167Debt.Md167ReceiptId = md167Receipt.Id;
                                }
                                else
                                {
                                    amountDiffInPeriodNc = (decimal)ncMd167Debt.AmountToBePaid;
                                    ncMd167Debt.AmountDiff = amountDiffInPeriodNc;
                                }

                                res.Add(ncMd167Debt);

                                amountDiff = (newMd167Debt.AmountDiff ?? 0) + (ncMd167Debt.AmountDiff ?? 0);

                            }
                            else
                            {
                                amountDiff = (newMd167Debt.AmountDiff ?? 0);
                            }

                            lastPaymentDate = md167Receipt != null ? (DateTime)md167Receipt.DateOfReceipt : lastPaymentDate;
                            if (md167Receipt != null)
                                prevDop = (DateTime)md167Receipt.DateOfReceipt;
                            else prevDop = null;
                        }

                        if (amountDiff > 0)
                        {
                            periodIndex = index;
                        }

                        month = j + step == 12 ? 1 : (j + step > 12 ? j + step - 12 : j + step);
                        index++;
                    }

                }
            }

            //group thành 3 nhóm,
            var response = new List<GroupMd167DebtData>();
            GroupMd167DebtData groupMd167DebtData = new GroupMd167DebtData();
            groupMd167DebtData.dataGroup = new List<Md167Debt>();
            groupMd167DebtData.dataGroup.AddRange(res.Skip(0).Take(3));
            response.Add(groupMd167DebtData);

            var groupData = res.Skip(3).GroupBy(x => x.Md167ReceiptId).OrderBy(x => x.Key == null).ToList();

            decimal? AmountPaidPerMonth = 0;
            decimal? AmountInterest = 0;
            decimal? AmountPaidInPeriod = 0;
            decimal? AmountToBePaid = 0;
            decimal? AmountPaid = 0;

            decimal? totalAmountDiff = 0;


            for (int i = 0; i < groupData.Count; i++)
            {
                GroupMd167DebtData nextGroupMd167DebtData = new GroupMd167DebtData();
                nextGroupMd167DebtData.dataGroup = new List<Md167Debt>();
                nextGroupMd167DebtData.dataGroup.AddRange(groupData[i].ToList());
                nextGroupMd167DebtData.length = nextGroupMd167DebtData.dataGroup.Count;
                nextGroupMd167DebtData.Md167ReceiptId = groupData[i].Key;

                if (groupData[i].Key != null && groupData[i].Key > 0)
                {
                    nextGroupMd167DebtData.AmountPaidPerMonth = nextGroupMd167DebtData.dataGroup.Sum(x => x.AmountPaidPerMonth);
                    nextGroupMd167DebtData.AmountInterest = nextGroupMd167DebtData.dataGroup.Sum(x => x.AmountInterest);
                    nextGroupMd167DebtData.AmountPaidInPeriod = nextGroupMd167DebtData.dataGroup.Sum(x => x.AmountPaidInPeriod);

                    nextGroupMd167DebtData.AmountToBePaid = nextGroupMd167DebtData.dataGroup.Sum(x => x.AmountToBePaid);
                    nextGroupMd167DebtData.AmountPaid = nextGroupMd167DebtData.dataGroup.Sum(x => x.AmountPaid);
                    //nextGroupMd167DebtData.AmountDiff = nextGroupMd167DebtData.dataGroup[nextGroupMd167DebtData.length - 1].AmountDiff;
                    nextGroupMd167DebtData.AmountDiff = nextGroupMd167DebtData.dataGroup.Sum(x => (x.AmountDiff > 0 ? x.AmountDiff : 0));

                    AmountPaidPerMonth += nextGroupMd167DebtData.AmountPaidPerMonth;
                    AmountInterest += nextGroupMd167DebtData.AmountInterest;
                    AmountPaidInPeriod += nextGroupMd167DebtData.AmountPaidInPeriod;
                    AmountToBePaid += nextGroupMd167DebtData.AmountToBePaid;
                    AmountPaid += nextGroupMd167DebtData.AmountPaid;

                    totalAmountDiff += nextGroupMd167DebtData.AmountDiff;
                }
                else if (groupData[i].Key == null)
                {
                    nextGroupMd167DebtData.AmountPaidPerMonth = nextGroupMd167DebtData.dataGroup.Sum(x => x.AmountPaidPerMonth);
                    nextGroupMd167DebtData.AmountInterest = nextGroupMd167DebtData.dataGroup.Sum(x => x.AmountInterest);
                    nextGroupMd167DebtData.AmountPaidInPeriod = nextGroupMd167DebtData.dataGroup.Sum(x => x.AmountPaidInPeriod);

                    //if(nextGroupMd167DebtData.dataGroup[nextGroupMd167DebtData.length - 1].TypeRow == Md167DebtTypeRow.DONG_NO_CU)
                    //{
                    //    nextGroupMd167DebtData.AmountToBePaid = (nextGroupMd167DebtData.dataGroup[nextGroupMd167DebtData.length - 1].AmountToBePaid ?? 0) + (nextGroupMd167DebtData.dataGroup[nextGroupMd167DebtData.length - 2].AmountToBePaid ?? 0);

                    //}
                    //else
                    //{
                    //    nextGroupMd167DebtData.AmountToBePaid = (nextGroupMd167DebtData.dataGroup[nextGroupMd167DebtData.length - 1].AmountToBePaid ?? 0);
                    //}

                    nextGroupMd167DebtData.AmountToBePaid = nextGroupMd167DebtData.dataGroup.Sum(x => x.AmountToBePaid);
                    nextGroupMd167DebtData.AmountPaid = nextGroupMd167DebtData.dataGroup.Sum(x => x.AmountPaid);
                    //nextGroupMd167DebtData.AmountDiff = nextGroupMd167DebtData.dataGroup[nextGroupMd167DebtData.length - 1].AmountDiff;
                    nextGroupMd167DebtData.AmountDiff = nextGroupMd167DebtData.AmountToBePaid;

                    AmountPaidPerMonth += nextGroupMd167DebtData.AmountPaidPerMonth;
                    AmountInterest += nextGroupMd167DebtData.AmountInterest;
                    AmountPaidInPeriod += nextGroupMd167DebtData.AmountPaidInPeriod;
                    //AmountToBePaid += (nextGroupMd167DebtData.AmountToBePaid - totalAmountDiff);
                    AmountToBePaid += nextGroupMd167DebtData.AmountToBePaid;
                    nextGroupMd167DebtData.AmountToBePaid += totalAmountDiff;

                    AmountPaid += nextGroupMd167DebtData.AmountPaid;
                }

                response.Add(nextGroupMd167DebtData);
            }

            CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");

            //Group tổng
            GroupMd167DebtData endMd167DebtData = new GroupMd167DebtData();
            endMd167DebtData.dataGroup = new List<Md167Debt>();
            endMd167DebtData.length = 1;
            Md167Debt endMd167Debt1 = new Md167Debt();
            endMd167Debt1.TypeRow = Md167DebtTypeRow.DONG_TONG;
            endMd167Debt1.Title = "Tổng";
            endMd167Debt1.AmountPaidPerMonth = AmountPaidPerMonth + res[2].AmountPaidPerMonth;
            endMd167Debt1.AmountInterest = AmountInterest + (res[2].AmountInterest ?? 0);
            endMd167Debt1.AmountPaidInPeriod = AmountPaidInPeriod + res[2].AmountPaidInPeriod;
            endMd167Debt1.AmountToBePaid = AmountToBePaid + res[2].AmountToBePaid;
            endMd167Debt1.AmountPaid = AmountPaid + res[2].AmountPaid;
            endMd167Debt1.AmountDiff = endMd167Debt1.AmountToBePaid - endMd167Debt1.AmountPaid;
            endMd167DebtData.dataGroup.Add(endMd167Debt1);

            document.ReplaceAll("<amounttobepaid>", endMd167Debt1.AmountToBePaid?.ToString("#,###", cul.NumberFormat) ?? "", SearchOptions.None);
            document.ReplaceAll("<amountpaid>", endMd167Debt1.AmountPaid?.ToString("#,###", cul.NumberFormat) ?? "", SearchOptions.None);
            document.ReplaceAll("<amountdiff>", endMd167Debt1.AmountDiff?.ToString("#,###", cul.NumberFormat) ?? "", SearchOptions.None);
            document.ReplaceAll("<amounttobepaidbangchu>", UtilsService.ConvertMoneyToString(endMd167Debt1.AmountToBePaid.ToString()).ToLower(), SearchOptions.None);
            document.ReplaceAll("<amountpaidbangchu>", UtilsService.ConvertMoneyToString(endMd167Debt1.AmountPaid.ToString()).ToLower(), SearchOptions.None);
            document.ReplaceAll("<amountdiffbangchu>", UtilsService.ConvertMoneyToString(endMd167Debt1.AmountDiff.ToString()).ToLower(), SearchOptions.None);

            response.Add(endMd167DebtData);

            return response;
        }
        #endregion

        #region hợp đồng 4
        [HttpPost("GetExportContract4/{id}")]
        public async Task<IActionResult> GetExportContract4(int id)
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
                string path = insertDataToTemplate4(id);
                if (path != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                        string fileName = "HopDong_MD167_04-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";

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
                log.Error("GetExportContract4:" + ex.Message);
                throw;
            }
        }

        private string insertDataToTemplate4(int id)
        {
            Md167Contract md167Contract = _context.Md167Contracts.Where(l => l.Id == id && l.Status != EntityStatus.DELETED).FirstOrDefault();
            Md167ContractData mapcontractData = _mapper.Map<Md167ContractData>(md167Contract);
            if (mapcontractData != null)
            {

                Md167House house = _context.Md167Houses.Where(f => f.Id == mapcontractData.HouseId && (f.TypeHouse == Md167House.Type_House.House || f.TypeHouse == Md167House.Type_House.Kios) && f.Status != EntityStatus.DELETED).FirstOrDefault();
                Md167HouseData map_md167house = _mapper.Map<Md167HouseData>(house);
                Md167HouseInContractData housecontract = new Md167HouseInContractData();
                if (map_md167house != null)
                {
                    string templatePath = Path.Combine(_hostingEnvironment.WebRootPath, "exportword/HopDong_MD167_04.docx");

                    string fileName = "result" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";
                    RichEditDocumentServer wordProcessor = new RichEditDocumentServer();
                    wordProcessor.LoadDocumentTemplate(templatePath, DocumentFormat.OpenXml);
                    var document = wordProcessor.Document;
                    document.BeginUpdate();

                    int dayNow = DateTime.Now.Day;
                    int monthNow = DateTime.Now.Month;
                    int yearNow = DateTime.Now.Year;

                    Md167House md167House = null;
                    if (map_md167house.TypeHouse == Md167House.Type_House.House)
                    {
                        //var isApplied = md167HouseTypes.Where(m => m.Id == dataItem.HouseTypeId).Select(x => x.IsApplied).FirstOrDefault();
                        //dataItem.Status = isApplied ? dataItem.Status : EntityStatus.DELETED;
                    }
                    else
                    {
                        md167House = _context.Md167Houses.Where(h => h.Id == map_md167house.Md167HouseId && h.Status != EntityStatus.DELETED).FirstOrDefault();
                        if (md167House != null)
                        {
                            map_md167house.ProvinceId = md167House.ProvinceId;
                            map_md167house.DistrictId = md167House.DistrictId;
                            map_md167house.WardId = md167House.WardId;
                            map_md167house.LaneId = md167House.LaneId;
                            map_md167house.InfoValue = md167House.InfoValue;
                        }
                    }

                    if (map_md167house.Status != EntityStatus.DELETED)
                    {
                        housecontract.Id = map_md167house.Id;
                        housecontract.Code = map_md167house.Code;
                        housecontract.HouseNumber = map_md167house.HouseNumber;
                        housecontract.ProvinceId = map_md167house.ProvinceId;
                        housecontract.DistrictId = map_md167house.DistrictId;
                        housecontract.WardId = map_md167house.WardId;
                        housecontract.LaneId = map_md167house.LaneId;
                        housecontract.UseLandPb = map_md167house.InfoValue.UseLandPb;
                        housecontract.UseLandPr = map_md167house.InfoValue.UseLandPr;
                        housecontract.AreaFloorBuild = map_md167house.InfoValue.AreaFloorBuild;
                        housecontract.TypeHouse = map_md167house.TypeHouse;
                        housecontract.ParentHouseNumber = map_md167house.HouseNumber;
                        housecontract.AreBuildPb = map_md167house.InfoValue.AreBuildPb;
                        housecontract.AreBuildPr = map_md167house.InfoValue.AreBuildPr;
                        housecontract.UseFloorPb = map_md167house.InfoValue.UseFloorPb;
                        housecontract.UseFloorPr = map_md167house.InfoValue.UseFloorPr;


                    }
                    else
                    {
                        return null;
                    }

                    //ngày hiện tại
                    string day = dayNow.ToString();
                    document.ReplaceAll("<day>", day, SearchOptions.None);
                    //tháng hiện tại
                    string month = monthNow.ToString();
                    document.ReplaceAll("<month>", month, SearchOptions.None);
                    //năm hiện tại
                    string year = yearNow.ToString();
                    document.ReplaceAll("<year>", year, SearchOptions.None);
                    //ngày/tháng/năm hiện tại
                    document.ReplaceAll("<dateNow>", day + "/" + month + "/" + year, SearchOptions.None);
                    
                    if (mapcontractData.ParentId != null)
                    {
                        Md167Contract md167_Contract = _context.Md167Contracts.Where(l => l.Id == mapcontractData.ParentId && l.Type == Contract167Type.MAIN && l.Status != EntityStatus.DELETED).FirstOrDefault();
                        Md167ContractData map_md167contract = _mapper.Map<Md167ContractData>(md167_Contract);
                        // số hợp đồng
                        document.ReplaceAll("<code>", map_md167contract.Code ?? "", SearchOptions.None);
                        // ngày ký hợp đồng
                        document.ReplaceAll("<datesign>", map_md167contract.DateSign.ToString("dd/MM/yyyy") ?? "", SearchOptions.None);
                        // số nhà
                        document.ReplaceAll("<housenumber>", map_md167house.HouseNumber ?? "", SearchOptions.None);

                        if (map_md167contract.personOrCompany == true)
                        {
                            Md167Delegate md167Delegate = _context.Md167Delegates.Where(l => l.Id == map_md167contract.DelegateId && l.PersonOrCompany == personOrCompany.COMPANY && l.Status != EntityStatus.DELETED).FirstOrDefault();
                            document.ReplaceAll("<namecompany>", md167Delegate != null ? md167Delegate.Name : "", SearchOptions.None);
                            document.ReplaceAll("<namepersonal>", md167Delegate != null ? md167Delegate.ComOrganizationRepresentativeName : "", SearchOptions.None);
                            document.ReplaceAll("<address>", md167Delegate != null ? md167Delegate.Address : "", SearchOptions.None);
                            document.ReplaceAll("<composition>", md167Delegate != null ? md167Delegate.ComPosition : "", SearchOptions.None);
                            document.ReplaceAll("<combusinesslicense>", md167Delegate != null ? md167Delegate.ComBusinessLicense : "", SearchOptions.None);
                            document.ReplaceAll("<comtaxnumber>", md167Delegate != null ? md167Delegate.ComTaxNumber : "", SearchOptions.None);
                            document.ReplaceAll("<phonenumber>", md167Delegate != null ? md167Delegate.PhoneNumber : "", SearchOptions.None);

                        }
                        else if (map_md167contract.personOrCompany == false)
                        {
                            Md167Delegate md167Delegate = _context.Md167Delegates.Where(l => l.Id == map_md167contract.DelegateId && l.PersonOrCompany == personOrCompany.PERSON && l.Status != EntityStatus.DELETED).FirstOrDefault();
                            document.ReplaceAll("<namecompany>", "", SearchOptions.None);
                            document.ReplaceAll("<namepersonal>", md167Delegate != null ? md167Delegate.Name : "", SearchOptions.None);
                            document.ReplaceAll("<address>", md167Delegate != null ? md167Delegate.Address : "", SearchOptions.None);
                            document.ReplaceAll("<composition>", "", SearchOptions.None);
                            document.ReplaceAll("<combusinesslicense>", "", SearchOptions.None);
                            document.ReplaceAll("<comtaxnumber>", "", SearchOptions.None);
                            document.ReplaceAll("<phonenumber>", md167Delegate != null ? md167Delegate.PhoneNumber : "", SearchOptions.None);

                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        // số hợp đồng
                        document.ReplaceAll("<code>", mapcontractData.Code ?? "", SearchOptions.None);
                        // ngày ký hợp đồng
                        document.ReplaceAll("<datesign>", mapcontractData.DateSign.ToString("dd/MM/yyyy") ?? "", SearchOptions.None);
                        // số nhà
                        document.ReplaceAll("<housenumber>", map_md167house.HouseNumber ?? "", SearchOptions.None);

                        if (mapcontractData.personOrCompany == true)
                        {
                            Md167Delegate md167Delegate = _context.Md167Delegates.Where(l => l.Id == mapcontractData.DelegateId && l.PersonOrCompany == personOrCompany.COMPANY && l.Status != EntityStatus.DELETED).FirstOrDefault();
                            document.ReplaceAll("<namecompany>", md167Delegate != null ? md167Delegate.Name : "", SearchOptions.None);
                            document.ReplaceAll("<namepersonal>", md167Delegate != null ? md167Delegate.ComOrganizationRepresentativeName : "", SearchOptions.None);
                            document.ReplaceAll("<address>", md167Delegate != null ? md167Delegate.Address : "", SearchOptions.None);
                            document.ReplaceAll("<composition>", md167Delegate != null ? md167Delegate.ComPosition : "", SearchOptions.None);
                            document.ReplaceAll("<combusinesslicense>", md167Delegate != null ? md167Delegate.ComBusinessLicense : "", SearchOptions.None);
                            document.ReplaceAll("<comtaxnumber>", md167Delegate != null ? md167Delegate.ComTaxNumber : "", SearchOptions.None);
                            document.ReplaceAll("<phonenumber>", md167Delegate != null ? md167Delegate.PhoneNumber : "", SearchOptions.None);

                        }
                        else if (mapcontractData.personOrCompany == false)
                        {
                            Md167Delegate md167Delegate = _context.Md167Delegates.Where(l => l.Id == mapcontractData.DelegateId && l.PersonOrCompany == personOrCompany.PERSON && l.Status != EntityStatus.DELETED).FirstOrDefault();
                            document.ReplaceAll("<namecompany>", "", SearchOptions.None);
                            document.ReplaceAll("<namepersonal>", md167Delegate != null ? md167Delegate.Name : "", SearchOptions.None);
                            document.ReplaceAll("<address>", md167Delegate != null ? md167Delegate.Address : "", SearchOptions.None);
                            document.ReplaceAll("<composition>", "", SearchOptions.None);
                            document.ReplaceAll("<combusinesslicense>", "", SearchOptions.None);
                            document.ReplaceAll("<comtaxnumber>", "", SearchOptions.None);
                            document.ReplaceAll("<phonenumber>", md167Delegate != null ? md167Delegate.PhoneNumber : "", SearchOptions.None);

                        }
                        else
                        {
                            return null;
                        }
                    }
                    

                    var pricePerMonths = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == mapcontractData.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                    List<Md167ProfitValue> profitValues = _context.Md167ProfitValues.Where(p => p.Status != EntityStatus.DELETED).ToList();
                    List<Md167Receipt> md167Receipts = _context.Md167Receipts.Where(m => m.Md167ContractId == mapcontractData.Id && m.PaidDeposit != true && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();
                    List<GroupMd167DebtData> res = null;
                    if (mapcontractData.Type == Contract167Type.MAIN)
                    {
                        List<Md167Receipt> depositMd167Receipts = _context.Md167Receipts.Where(m => m.Md167ContractId == mapcontractData.Id && m.PaidDeposit == true && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();

                        if (mapcontractData != null)
                        {
                            var pricePerMonth = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == mapcontractData.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).FirstOrDefault();
                            res = GetDataDebtFunc(document, pricePerMonths, profitValues, md167Receipts, mapcontractData, pricePerMonth?.TotalPrice * 3, depositMd167Receipts);
                        }
                        else
                        {
                            var pricePerMonth = pricePerMonths.Count > 0 ? pricePerMonths[pricePerMonths.Count - 1] : null;
                            res = GetDataDebtFunc(document, pricePerMonths, profitValues, md167Receipts, mapcontractData, pricePerMonth?.TotalPrice * 3, depositMd167Receipts);
                        }
                    }
                    else
                    {
                        var parentData = _context.Md167Contracts.Where(l => l.Id == mapcontractData.ParentId && l.Status != EntityStatus.DELETED).FirstOrDefault();
                        var parentPricePerMonths = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == parentData.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                        List<Md167Receipt> parentMd167Receipts = _context.Md167Receipts.Where(m => m.Md167ContractId == parentData.Id && m.PaidDeposit != true && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();
                        List<Md167Receipt> depositMd167Receipts = _context.Md167Receipts.Where(m => m.Md167ContractId == mapcontractData.Id && m.PaidDeposit == true && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();

                        res = GetDataExtraDebtFunc(document, pricePerMonths, profitValues, md167Receipts, mapcontractData, parentPricePerMonths, parentMd167Receipts, parentData, depositMd167Receipts);
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
            else
            {
                return null;
            }
        }

        public static List<GroupMd167DebtData> GetDataExtraDebtFunc(Document document, List<Md167PricePerMonth> _pricePerMonths, List<Md167ProfitValue> profitValues, List<Md167Receipt> _md167Receipts, Md167Contract _md167Contract, List<Md167PricePerMonth> parentPricePerMonths, List<Md167Receipt> parentMd167Receipts, Md167Contract parentMd167Contract, List<Md167Receipt> depositMd167Receipts)
        {
            List<Md167Debt> res = new List<Md167Debt>();

            int curYear = 1;
            //Tính các kỳ thanh toán của hợp đồng cũ
            if (parentMd167Contract != null)
            {
                var parentPricePerMonth = parentPricePerMonths.Count > 0 ? parentPricePerMonths[parentPricePerMonths.Count - 1] : null;
                //Thêm dòng tiền thế chân
                Md167Debt ttcMd167Debt = new Md167Debt();
                ttcMd167Debt.TypeRow = Md167DebtTypeRow.DONG_THE_CHAN;
                ttcMd167Debt.Title = "Tiền thế chân";
                ttcMd167Debt.AmountToBePaid = parentPricePerMonth != null ? parentPricePerMonth.TotalPrice * 3 : null;
                ttcMd167Debt.AmountPaid = depositMd167Receipts.Sum(x => x.Amount ?? 0);
                ttcMd167Debt.AmountDiff = ttcMd167Debt.AmountToBePaid - ttcMd167Debt.AmountPaid;
                //ttcMd167Debt.Note = "Thông tin thanh toán từ hợp đồng " + parentMd167Contract.Code;
                res.Add(ttcMd167Debt);

                if (parentMd167Receipts.Count > 0)
                {
                    //Ngày bắt đầu thuê
                    var dateStart = parentMd167Contract.DateSign;
                    //Ngày kết thúc
                    var dateEnd = DateTime.Now;

                    //số năm
                    DateTime zeroTime = new DateTime(1, 1, 1);
                    TimeSpan span = dateEnd - dateStart;
                    int years = (zeroTime + span).Year + 1;

                    //tháng bắt đầu hợp đồng
                    var month = dateStart.Month;
                    var year = dateStart.Year;

                    //lặp các năm
                    bool breakloop = false;
                    decimal amountDiff = 0;
                    decimal interest = 0.02m;
                    DateTime lastPaymentDate = dateStart;       //Ngày thanh toán gần nhất
                    DateTime? prevDop = null;       //Ngày thanh toán theo quy định của kỳ đằng trước

                    bool? firstRowNullPayment = false;

                    int? periodIndex = null;

                    for (int i = 1; i <= years; i++)
                    {
                        if (breakloop == true)
                        {
                            break;
                        }

                        //Thêm dòng tên năm
                        Md167Debt md167Debt = new Md167Debt();
                        md167Debt.TypeRow = Md167DebtTypeRow.DONG_NAM;
                        md167Debt.Title = $"Năm thứ {i}";
                        var md167ReceiptId = parentMd167Receipts.Where(m => m.Amount != 0 && m.Status == EntityStatus.TEMP).OrderBy(m => m.DateOfReceipt).FirstOrDefault()?.Id;
                        if (md167ReceiptId != null)
                        {
                            md167Debt.Md167ReceiptId = md167ReceiptId;
                        }
                        else
                        {
                            if (parentMd167Receipts.Where(m => m.Amount != 0 && m.Status != EntityStatus.TEMP).OrderBy(m => m.DateOfReceipt).FirstOrDefault() != null)
                            {
                                md167Debt.Md167ReceiptId = -i;
                            }
                            else
                            {
                                break;
                            }
                        }

                        //md167Debt.DopActual = new DateTime(md167Contract.DateSign.Year + i - 1, 1, 1);
                        md167Debt.Note = "Thông tin thanh toán từ hợp đồng " + parentMd167Contract.Code;
                        res.Add(md167Debt);

                        //Lặp các tháng thanh toán trong 1 năm
                        //Kiểm tra kỳ thanh toán là theo tháng hay theo quý
                        int step = parentMd167Contract.PaymentPeriod == PaymentPeriodContract167.THANG ? 1 : 3;
                        int index = 1;
                        for (int j = month; j <= 12; j = j + step)
                        {
                            if (i == 1 && j == dateStart.Month)
                            {
                                //thêm dòng đầu tiên
                                Md167Debt firstMd167Debt = new Md167Debt();
                                firstMd167Debt.TypeRow = Md167DebtTypeRow.DONG_DU_LIEU;
                                firstMd167Debt.Index = index;
                                firstMd167Debt.Dop = new DateTime(year, month, 1);
                                firstMd167Debt.AmountPaidPerMonth = parentPricePerMonth != null ? parentPricePerMonth.TotalPrice : null;
                                firstMd167Debt.AmountToBePaid = firstMd167Debt.AmountPaidPerMonth;
                                //QFix:Fix bug thanh toán tháng đầu
                                // firstMd167Debt.AmountPaidInPeriod = firstMd167Debt.AmountPaidPerMonth;
                                // firstMd167Debt.AmountPaid = firstMd167Debt.AmountPaidInPeriod;
                                // firstMd167Debt.AmountDiff = null;
                                firstMd167Debt.AmountPaidInPeriod = 0; // Không tự động thanh toán tháng đầu
                                firstMd167Debt.AmountPaid = 0; // Không tự động thanh toán tháng đầu
                                firstMd167Debt.AmountDiff = firstMd167Debt.AmountToBePaid; // Còn nợ toàn bộ số tiền

                                firstMd167Debt.Note = "Thông tin thanh toán từ hợp đồng " + parentMd167Contract.Code;
                                res.Add(firstMd167Debt);
                            }
                            else
                            {
                                var dop = new DateTime(year + i - 1, j, 1);
                                if (dop > dateEnd)
                                {
                                    breakloop = true;
                                    break;
                                }

                                Md167Debt newMd167Debt = new Md167Debt();
                                newMd167Debt.TypeRow = Md167DebtTypeRow.DONG_DU_LIEU;
                                newMd167Debt.Index = index;
                                newMd167Debt.Dop = dop;

                                //Tính lãi thuê áp dụng
                                var profitValue = profitValues.Where(p => p.DoApply <= dop).OrderByDescending(x => x.DoApply).FirstOrDefault();

                                interest = profitValue != null ? (decimal)profitValue.Value : interest;
                                newMd167Debt.Interest = (float)interest;

                                //Tìm số tiền phải trả hàng tháng
                                var pricePerMonthInPeriod = parentPricePerMonths.Where(e => e.DateEffect <= dop).OrderBy(x => x.UpdatedAt).FirstOrDefault();
                                if (pricePerMonthInPeriod == null)
                                {
                                    newMd167Debt.AmountPaidPerMonth = parentPricePerMonth != null ? parentPricePerMonth.TotalPrice : null;
                                }
                                else
                                {
                                    newMd167Debt.AmountPaidPerMonth = pricePerMonthInPeriod.TotalPrice;
                                }
                                if (newMd167Debt.AmountPaidPerMonth == null) newMd167Debt.AmountPaidPerMonth = 0;
                                //Kiểm tra có kỳ thanh toán hay không
                                Md167Receipt md167Receipt = parentMd167Receipts.Where(m => m.Amount != 0).OrderBy(m => m.DateOfReceipt).FirstOrDefault();
                                if (md167Receipt != null)
                                {
                                    int idx = parentMd167Receipts.IndexOf(md167Receipt);
                                    parentMd167Receipts[idx].Status = EntityStatus.TEMP;
                                    newMd167Debt.DopActual = md167Receipt.DateOfReceipt;      //Ngày thanh toán thực tế
                                    if (newMd167Debt.DopActual > newMd167Debt.Dop)
                                    {
                                        newMd167Debt.InterestCalcDate = (int)Math.Ceiling((newMd167Debt.DopActual - newMd167Debt.Dop).Value.TotalDays);
                                        newMd167Debt.AmountInterest = newMd167Debt.AmountPaidPerMonth * newMd167Debt.InterestCalcDate * (interest / 100);
                                    }
                                }
                                else
                                {
                                    newMd167Debt.DopActual = dateEnd;      //Ngày thanh toán thực tế
                                    newMd167Debt.InterestCalcDate = (int)Math.Ceiling((newMd167Debt.DopActual - newMd167Debt.Dop).Value.TotalDays);
                                    newMd167Debt.AmountInterest = newMd167Debt.AmountPaidPerMonth * newMd167Debt.InterestCalcDate * (interest / 100);

                                    if (firstRowNullPayment == false) firstRowNullPayment = true;
                                    else firstRowNullPayment = null;
                                }

                                newMd167Debt.AmountPaidInPeriod = newMd167Debt.AmountPaidPerMonth + (newMd167Debt.AmountInterest ?? 0);
                                //newMd167Debt.AmountToBePaid = newMd167Debt.AmountPaidInPeriod + (amountDiff < 0 ? amountDiff : 0);
                                newMd167Debt.AmountToBePaid = newMd167Debt.AmountPaidInPeriod;
                                if (amountDiff > 0 && md167Receipt != null && md167Receipt?.DateOfReceipt <= newMd167Debt.Dop)
                                {
                                    newMd167Debt.AmountToBePaid += amountDiff;
                                }

                                decimal amountDiffInPeriod = 0;         //Nợ của kỳ này
                                if (md167Receipt != null)
                                {
                                    int idx = parentMd167Receipts.IndexOf(md167Receipt);
                                    //md167Receipts[idx].Amount = amountDiff < 0 ? md167Receipts[idx].Amount + amountDiff : md167Receipts[idx].Amount;
                                    amountDiffInPeriod = (decimal)newMd167Debt.AmountToBePaid - (decimal)md167Receipt.Amount;
                                    if (amountDiffInPeriod >= 0)
                                    {
                                        newMd167Debt.AmountPaid = md167Receipt.Amount;
                                        parentMd167Receipts[idx].Amount = 0;
                                        if (amountDiffInPeriod > 0) newMd167Debt.AmountDiff = amountDiffInPeriod;
                                        else newMd167Debt.AmountDiff = null;
                                    }
                                    else
                                    {
                                        newMd167Debt.AmountPaid = newMd167Debt.AmountToBePaid;
                                        parentMd167Receipts[idx].Amount = parentMd167Receipts[idx].Amount - newMd167Debt.AmountToBePaid;
                                        newMd167Debt.AmountDiff = null;

                                        //if (md167Receipts[idx].Amount >= newMd167Debt.AmountToBePaid)
                                        //{

                                        //}
                                        //else
                                        //{
                                        //    newMd167Debt.AmountPaid = md167Receipts[idx].Amount;
                                        //    md167Receipts[idx].Amount = 0;
                                        //    newMd167Debt.AmountDiff = md167Receipts[idx].Amount + amountDiffInPeriod;
                                        //}
                                    }

                                    newMd167Debt.Md167ReceiptId = md167Receipt.Id;
                                }
                                else
                                {
                                    amountDiffInPeriod = (decimal)newMd167Debt.AmountToBePaid;
                                    newMd167Debt.AmountDiff = amountDiffInPeriod;
                                }

                                newMd167Debt.Note = "Thông tin thanh toán từ hợp đồng " + parentMd167Contract.Code;
                                if (newMd167Debt.Md167ReceiptId != null) res.Add(newMd167Debt);


                                //Kiểm tra nợ cũ
                                if (amountDiff > 0 && ((md167Receipt == null && firstRowNullPayment == true) || (md167Receipt != null && md167Receipt.DateOfReceipt > newMd167Debt.Dop)) && newMd167Debt.Md167ReceiptId != null)
                                //if (amountDiff > 0 && md167Receipt != null && md167Receipt.DateOfReceipt > newMd167Debt.Dop)
                                {
                                    Md167Debt ncMd167Debt = new Md167Debt();
                                    ncMd167Debt.Title = "Nợ cũ kỳ thanh toán " + periodIndex;
                                    ncMd167Debt.TypeRow = Md167DebtTypeRow.DONG_NO_CU;
                                    ncMd167Debt.Dop = dop;
                                    ncMd167Debt.Interest = (float)interest;

                                    if (md167Receipt != null)
                                    {
                                        ncMd167Debt.DopActual = md167Receipt.DateOfReceipt;      //Ngày thanh toán thực tế
                                        if (ncMd167Debt.DopActual > lastPaymentDate)
                                        {
                                            ncMd167Debt.InterestCalcDate = (int)Math.Ceiling((ncMd167Debt.DopActual - lastPaymentDate).Value.TotalDays);
                                            ncMd167Debt.AmountInterest = amountDiff * ncMd167Debt.InterestCalcDate * (interest / 100);
                                        }
                                    }
                                    else
                                    {
                                        ncMd167Debt.DopActual = dateEnd;      //Ngày thanh toán thực tế
                                        ncMd167Debt.InterestCalcDate = (int)Math.Ceiling((ncMd167Debt.DopActual - (prevDop != null ? prevDop : newMd167Debt.Dop)).Value.TotalDays);
                                        ncMd167Debt.AmountInterest = Math.Ceiling(amountDiff * (decimal)ncMd167Debt.InterestCalcDate * (interest / 100));
                                    }

                                    ncMd167Debt.AmountPaidInPeriod = ncMd167Debt.AmountInterest ?? 0;
                                    ncMd167Debt.AmountToBePaid = ncMd167Debt.AmountPaidInPeriod + amountDiff;

                                    decimal amountDiffInPeriodNc = 0;         //Nợ của kỳ này của dòng nợ cũ

                                    if (md167Receipt != null)
                                    {
                                        int idx = parentMd167Receipts.IndexOf(md167Receipt);
                                        amountDiffInPeriodNc = (decimal)ncMd167Debt.AmountToBePaid - (decimal)md167Receipt.Amount;
                                        if (amountDiffInPeriodNc >= 0)
                                        {
                                            ncMd167Debt.AmountPaid = md167Receipt.Amount;
                                            parentMd167Receipts[idx].Amount = 0;
                                            ncMd167Debt.AmountDiff = amountDiffInPeriodNc;
                                        }
                                        else
                                        {
                                            ncMd167Debt.AmountPaid = ncMd167Debt.AmountToBePaid;
                                            parentMd167Receipts[idx].Amount = -amountDiffInPeriodNc;
                                            ncMd167Debt.AmountDiff = amountDiffInPeriodNc;
                                        }

                                        ncMd167Debt.Md167ReceiptId = md167Receipt.Id;
                                    }
                                    else
                                    {
                                        amountDiffInPeriodNc = (decimal)ncMd167Debt.AmountToBePaid;
                                        ncMd167Debt.AmountDiff = amountDiffInPeriodNc;
                                    }

                                    ncMd167Debt.Note = "Thông tin thanh toán từ hợp đồng " + parentMd167Contract.Code;
                                    res.Add(ncMd167Debt);

                                    amountDiff = (newMd167Debt.AmountDiff ?? 0) + (ncMd167Debt.AmountDiff ?? 0);

                                }
                                else
                                {
                                    amountDiff = (newMd167Debt.AmountDiff ?? 0);
                                }

                                lastPaymentDate = md167Receipt != null ? (DateTime)md167Receipt.DateOfReceipt : lastPaymentDate;
                                if (md167Receipt != null)
                                    prevDop = (DateTime)md167Receipt.DateOfReceipt;
                                else prevDop = null;

                                if (newMd167Debt.Md167ReceiptId == null)
                                {
                                    breakloop = true;
                                    break;
                                }
                            }

                            if (amountDiff > 0)
                            {
                                periodIndex = index;
                            }

                            month = j + step == 12 ? 1 : (j + step > 12 ? j + step - 12 : j + step);
                            index++;
                        }
                        curYear = i;
                    }
                }
            }

            //Tính kỳ thanh toán của Phụ lục
            if (_md167Contract != null)
            {
                var pricePerMonth = _pricePerMonths.Count > 0 ? _pricePerMonths[_pricePerMonths.Count - 1] : null;

                //Ngày bắt đầu thuê
                var dateStart = _md167Contract.DateSign;
                //Ngày kết thúc
                var dateEnd = DateTime.Now;
                //var dateEnd = new DateTime(2016,11,11);

                //số năm
                DateTime zeroTime = new DateTime(1, 1, 1);
                TimeSpan span = dateEnd - dateStart;
                int years = (zeroTime + span).Year + 1;

                //tháng bắt đầu hợp đồng
                var month = dateStart.Month;
                var year = dateStart.Year;


                //lặp các năm
                bool breakloop = false;
                decimal amountDiff = 0;
                decimal interest = 0.02m;
                DateTime lastPaymentDate = dateStart;       //Ngày thanh toán gần nhất
                DateTime? prevDop = null;       //Ngày thanh toán theo quy định của kỳ đằng trước

                bool? firstRowNullPayment = false;

                int? periodIndex = null;

                for (int i = curYear; i <= years + curYear; i++)
                {
                    if (breakloop == true)
                    {
                        break;
                    }

                    //Thêm dòng tên năm
                    Md167Debt md167Debt = new Md167Debt();
                    md167Debt.TypeRow = Md167DebtTypeRow.DONG_NAM;
                    md167Debt.Title = $"Năm thứ {i}";
                    var md167ReceiptId = _md167Receipts.Where(m => m.Amount != 0 && m.Status == EntityStatus.TEMP).OrderBy(m => m.DateOfReceipt).FirstOrDefault()?.Id;
                    if (md167ReceiptId != null)
                    {
                        md167Debt.Md167ReceiptId = md167ReceiptId;
                    }
                    else
                    {
                        if (_md167Receipts.Where(m => m.Amount != 0 && m.Status != EntityStatus.TEMP).OrderBy(m => m.DateOfReceipt).FirstOrDefault() != null)
                        {
                            md167Debt.Md167ReceiptId = -i;
                        }
                        else
                        {
                            md167Debt.Md167ReceiptId = null;
                        }
                    }

                    //md167Debt.DopActual = new DateTime(md167Contract.DateSign.Year + i - 1, 1, 1);
                    res.Add(md167Debt);

                    //Lặp các tháng thanh toán trong 1 năm
                    //Kiểm tra kỳ thanh toán là theo tháng hay theo quý
                    int step = _md167Contract.PaymentPeriod == PaymentPeriodContract167.THANG ? 1 : 3;
                    int index = 1;
                    for (int j = month; j <= 12; j = j + step)
                    {
                        if (i == 1 && j == dateStart.Month && parentMd167Contract == null)
                        {
                            //thêm dòng đầu tiên
                            Md167Debt firstMd167Debt = new Md167Debt();
                            firstMd167Debt.TypeRow = Md167DebtTypeRow.DONG_DU_LIEU;
                            firstMd167Debt.Index = index;
                            firstMd167Debt.Dop = new DateTime(year, month, 1);
                            firstMd167Debt.AmountPaidPerMonth = pricePerMonth != null ? pricePerMonth.TotalPrice : null;
                            firstMd167Debt.AmountToBePaid = firstMd167Debt.AmountPaidPerMonth;
                            //QFix:Fix bug thanh toán tháng đầu
                            // firstMd167Debt.AmountPaidInPeriod = firstMd167Debt.AmountPaidPerMonth;
                            // firstMd167Debt.AmountPaid = firstMd167Debt.AmountPaidInPeriod;
                            // firstMd167Debt.AmountDiff = null;
                            firstMd167Debt.AmountPaidInPeriod = 0; // Không tự động thanh toán tháng đầu
                            firstMd167Debt.AmountPaid = 0; // Không tự động thanh toán tháng đầu
                            firstMd167Debt.AmountDiff = firstMd167Debt.AmountToBePaid; // Còn nợ toàn bộ số tiền

                            res.Add(firstMd167Debt);
                        }
                        else
                        {
                            var dop = new DateTime(year + i - 1, j, 1);
                            if (dop > dateEnd)
                            {
                                breakloop = true;
                                break;
                            }

                            Md167Debt newMd167Debt = new Md167Debt();
                            newMd167Debt.TypeRow = Md167DebtTypeRow.DONG_DU_LIEU;
                            newMd167Debt.Index = index;
                            newMd167Debt.Dop = dop;

                            //Tính lãi thuê áp dụng
                            var profitValue = profitValues.Where(p => p.DoApply <= dop).OrderByDescending(x => x.DoApply).FirstOrDefault();

                            interest = profitValue != null ? (decimal)profitValue.Value : interest;
                            newMd167Debt.Interest = (float)interest;

                            //Tìm số tiền phải trả hàng tháng
                            var pricePerMonthInPeriod = _pricePerMonths.Where(e => e.DateEffect <= dop).OrderBy(x => x.UpdatedAt).FirstOrDefault();
                            if (pricePerMonthInPeriod == null)
                            {
                                newMd167Debt.AmountPaidPerMonth = pricePerMonth != null ? pricePerMonth.TotalPrice : null;
                            }
                            else
                            {
                                newMd167Debt.AmountPaidPerMonth = pricePerMonthInPeriod.TotalPrice;
                            }
                            if (newMd167Debt.AmountPaidPerMonth == null) newMd167Debt.AmountPaidPerMonth = 0;
                            //Kiểm tra có kỳ thanh toán hay không
                            Md167Receipt md167Receipt = _md167Receipts.Where(m => m.Amount != 0).OrderBy(m => m.DateOfReceipt).FirstOrDefault();
                            if (md167Receipt != null)
                            {
                                int idx = _md167Receipts.IndexOf(md167Receipt);
                                _md167Receipts[idx].Status = EntityStatus.TEMP;
                                newMd167Debt.DopActual = md167Receipt.DateOfReceipt;      //Ngày thanh toán thực tế
                                if (newMd167Debt.DopActual > newMd167Debt.Dop)
                                {
                                    newMd167Debt.InterestCalcDate = (int)Math.Ceiling((newMd167Debt.DopActual - newMd167Debt.Dop).Value.TotalDays);
                                    newMd167Debt.AmountInterest = newMd167Debt.AmountPaidPerMonth * newMd167Debt.InterestCalcDate * (interest / 100);
                                }
                            }
                            else
                            {
                                newMd167Debt.DopActual = dateEnd;      //Ngày thanh toán thực tế
                                newMd167Debt.InterestCalcDate = (int)Math.Ceiling((newMd167Debt.DopActual - newMd167Debt.Dop).Value.TotalDays);
                                newMd167Debt.AmountInterest = newMd167Debt.AmountPaidPerMonth * newMd167Debt.InterestCalcDate * (interest / 100);

                                if (firstRowNullPayment == false) firstRowNullPayment = true;
                                else firstRowNullPayment = null;
                            }

                            newMd167Debt.AmountPaidInPeriod = newMd167Debt.AmountPaidPerMonth + (newMd167Debt.AmountInterest ?? 0);
                            //newMd167Debt.AmountToBePaid = newMd167Debt.AmountPaidInPeriod + (amountDiff < 0 ? amountDiff : 0);
                            newMd167Debt.AmountToBePaid = newMd167Debt.AmountPaidInPeriod;
                            if (amountDiff > 0 && md167Receipt != null && md167Receipt?.DateOfReceipt <= newMd167Debt.Dop)
                            {
                                newMd167Debt.AmountToBePaid += amountDiff;
                            }

                            decimal amountDiffInPeriod = 0;         //Nợ của kỳ này
                            if (md167Receipt != null)
                            {
                                int idx = _md167Receipts.IndexOf(md167Receipt);
                                //md167Receipts[idx].Amount = amountDiff < 0 ? md167Receipts[idx].Amount + amountDiff : md167Receipts[idx].Amount;
                                amountDiffInPeriod = (decimal)newMd167Debt.AmountToBePaid - (decimal)md167Receipt.Amount;
                                if (amountDiffInPeriod >= 0)
                                {
                                    newMd167Debt.AmountPaid = md167Receipt.Amount;
                                    _md167Receipts[idx].Amount = 0;
                                    if (amountDiffInPeriod > 0) newMd167Debt.AmountDiff = amountDiffInPeriod;
                                    else newMd167Debt.AmountDiff = null;
                                }
                                else
                                {
                                    newMd167Debt.AmountPaid = newMd167Debt.AmountToBePaid;
                                    _md167Receipts[idx].Amount = _md167Receipts[idx].Amount - newMd167Debt.AmountToBePaid;
                                    newMd167Debt.AmountDiff = null;

                                    //if (md167Receipts[idx].Amount >= newMd167Debt.AmountToBePaid)
                                    //{

                                    //}
                                    //else
                                    //{
                                    //    newMd167Debt.AmountPaid = md167Receipts[idx].Amount;
                                    //    md167Receipts[idx].Amount = 0;
                                    //    newMd167Debt.AmountDiff = md167Receipts[idx].Amount + amountDiffInPeriod;
                                    //}
                                }

                                newMd167Debt.Md167ReceiptId = md167Receipt.Id;
                            }
                            else
                            {
                                amountDiffInPeriod = (decimal)newMd167Debt.AmountToBePaid;
                                newMd167Debt.AmountDiff = amountDiffInPeriod;
                            }

                            res.Add(newMd167Debt);


                            //Kiểm tra nợ cũ
                            if (amountDiff > 0 && ((md167Receipt == null && firstRowNullPayment == true) || (md167Receipt != null && md167Receipt.DateOfReceipt > newMd167Debt.Dop)))
                            //if (amountDiff > 0 && md167Receipt != null && md167Receipt.DateOfReceipt > newMd167Debt.Dop)
                            {
                                Md167Debt ncMd167Debt = new Md167Debt();
                                ncMd167Debt.Title = "Nợ cũ kỳ thanh toán " + periodIndex;
                                ncMd167Debt.TypeRow = Md167DebtTypeRow.DONG_NO_CU;
                                ncMd167Debt.Dop = dop;
                                ncMd167Debt.Interest = (float)interest;

                                if (md167Receipt != null)
                                {
                                    ncMd167Debt.DopActual = md167Receipt.DateOfReceipt;      //Ngày thanh toán thực tế
                                    if (ncMd167Debt.DopActual > lastPaymentDate)
                                    {
                                        ncMd167Debt.InterestCalcDate = (int)Math.Ceiling((ncMd167Debt.DopActual - lastPaymentDate).Value.TotalDays);
                                        ncMd167Debt.AmountInterest = amountDiff * ncMd167Debt.InterestCalcDate * (interest / 100);
                                    }
                                }
                                else
                                {
                                    ncMd167Debt.DopActual = dateEnd;      //Ngày thanh toán thực tế
                                    ncMd167Debt.InterestCalcDate = (int)Math.Ceiling((ncMd167Debt.DopActual - (prevDop != null ? prevDop : newMd167Debt.Dop)).Value.TotalDays);
                                    ncMd167Debt.AmountInterest = Math.Ceiling(amountDiff * (decimal)ncMd167Debt.InterestCalcDate * (interest / 100));
                                }

                                ncMd167Debt.AmountPaidInPeriod = ncMd167Debt.AmountInterest ?? 0;
                                ncMd167Debt.AmountToBePaid = ncMd167Debt.AmountPaidInPeriod + amountDiff;

                                decimal amountDiffInPeriodNc = 0;         //Nợ của kỳ này của dòng nợ cũ

                                if (md167Receipt != null)
                                {
                                    int idx = _md167Receipts.IndexOf(md167Receipt);
                                    amountDiffInPeriodNc = (decimal)ncMd167Debt.AmountToBePaid - (decimal)md167Receipt.Amount;
                                    if (amountDiffInPeriodNc >= 0)
                                    {
                                        ncMd167Debt.AmountPaid = md167Receipt.Amount;
                                        _md167Receipts[idx].Amount = 0;
                                        ncMd167Debt.AmountDiff = amountDiffInPeriodNc;
                                    }
                                    else
                                    {
                                        ncMd167Debt.AmountPaid = ncMd167Debt.AmountToBePaid;
                                        _md167Receipts[idx].Amount = -amountDiffInPeriodNc;
                                        ncMd167Debt.AmountDiff = amountDiffInPeriodNc;
                                    }

                                    ncMd167Debt.Md167ReceiptId = md167Receipt.Id;
                                }
                                else
                                {
                                    amountDiffInPeriodNc = (decimal)ncMd167Debt.AmountToBePaid;
                                    ncMd167Debt.AmountDiff = amountDiffInPeriodNc;
                                }

                                res.Add(ncMd167Debt);

                                amountDiff = (newMd167Debt.AmountDiff ?? 0) + (ncMd167Debt.AmountDiff ?? 0);

                            }
                            else
                            {
                                amountDiff = (newMd167Debt.AmountDiff ?? 0);
                            }

                            lastPaymentDate = md167Receipt != null ? (DateTime)md167Receipt.DateOfReceipt : lastPaymentDate;
                            if (md167Receipt != null)
                                prevDop = (DateTime)md167Receipt.DateOfReceipt;
                            else prevDop = null;
                        }

                        if (amountDiff > 0)
                        {
                            periodIndex = index;
                        }

                        month = j + step == 12 ? 1 : (j + step > 12 ? j + step - 12 : j + step);
                        index++;
                    }

                }
            }

            //group thành 3 nhóm,
            var response = new List<GroupMd167DebtData>();
            GroupMd167DebtData groupMd167DebtData = new GroupMd167DebtData();
            groupMd167DebtData.dataGroup = new List<Md167Debt>();
            groupMd167DebtData.dataGroup.AddRange(res.Skip(0).Take(3));
            response.Add(groupMd167DebtData);

            var groupData = res.Skip(3).GroupBy(x => x.Md167ReceiptId).OrderBy(x => x.Key == null).ToList();

            decimal? AmountPaidPerMonth = 0;
            decimal? AmountInterest = 0;
            decimal? AmountPaidInPeriod = 0;
            decimal? AmountToBePaid = 0;
            decimal? AmountPaid = 0;

            decimal? totalAmountDiff = 0;


            for (int i = 0; i < groupData.Count; i++)
            {
                GroupMd167DebtData nextGroupMd167DebtData = new GroupMd167DebtData();
                nextGroupMd167DebtData.dataGroup = new List<Md167Debt>();
                nextGroupMd167DebtData.dataGroup.AddRange(groupData[i].ToList());
                nextGroupMd167DebtData.length = nextGroupMd167DebtData.dataGroup.Count;
                nextGroupMd167DebtData.Md167ReceiptId = groupData[i].Key;

                if (groupData[i].Key != null && groupData[i].Key > 0)
                {
                    nextGroupMd167DebtData.AmountPaidPerMonth = nextGroupMd167DebtData.dataGroup.Sum(x => x.AmountPaidPerMonth);
                    nextGroupMd167DebtData.AmountInterest = nextGroupMd167DebtData.dataGroup.Sum(x => x.AmountInterest);
                    nextGroupMd167DebtData.AmountPaidInPeriod = nextGroupMd167DebtData.dataGroup.Sum(x => x.AmountPaidInPeriod);

                    nextGroupMd167DebtData.AmountToBePaid = nextGroupMd167DebtData.dataGroup.Sum(x => x.AmountToBePaid);
                    nextGroupMd167DebtData.AmountPaid = nextGroupMd167DebtData.dataGroup.Sum(x => x.AmountPaid);
                    //nextGroupMd167DebtData.AmountDiff = nextGroupMd167DebtData.dataGroup[nextGroupMd167DebtData.length - 1].AmountDiff;
                    nextGroupMd167DebtData.AmountDiff = nextGroupMd167DebtData.dataGroup.Sum(x => (x.AmountDiff > 0 ? x.AmountDiff : 0));

                    AmountPaidPerMonth += nextGroupMd167DebtData.AmountPaidPerMonth;
                    AmountInterest += nextGroupMd167DebtData.AmountInterest;
                    AmountPaidInPeriod += nextGroupMd167DebtData.AmountPaidInPeriod;
                    AmountToBePaid += nextGroupMd167DebtData.AmountToBePaid;
                    AmountPaid += nextGroupMd167DebtData.AmountPaid;

                    totalAmountDiff += nextGroupMd167DebtData.AmountDiff;
                }
                else if (groupData[i].Key == null)
                {
                    nextGroupMd167DebtData.AmountPaidPerMonth = nextGroupMd167DebtData.dataGroup.Sum(x => x.AmountPaidPerMonth);
                    nextGroupMd167DebtData.AmountInterest = nextGroupMd167DebtData.dataGroup.Sum(x => x.AmountInterest);
                    nextGroupMd167DebtData.AmountPaidInPeriod = nextGroupMd167DebtData.dataGroup.Sum(x => x.AmountPaidInPeriod);

                    //if(nextGroupMd167DebtData.dataGroup[nextGroupMd167DebtData.length - 1].TypeRow == Md167DebtTypeRow.DONG_NO_CU)
                    //{
                    //    nextGroupMd167DebtData.AmountToBePaid = (nextGroupMd167DebtData.dataGroup[nextGroupMd167DebtData.length - 1].AmountToBePaid ?? 0) + (nextGroupMd167DebtData.dataGroup[nextGroupMd167DebtData.length - 2].AmountToBePaid ?? 0);

                    //}
                    //else
                    //{
                    //    nextGroupMd167DebtData.AmountToBePaid = (nextGroupMd167DebtData.dataGroup[nextGroupMd167DebtData.length - 1].AmountToBePaid ?? 0);
                    //}

                    nextGroupMd167DebtData.AmountToBePaid = nextGroupMd167DebtData.dataGroup.Sum(x => x.AmountToBePaid);
                    nextGroupMd167DebtData.AmountPaid = nextGroupMd167DebtData.dataGroup.Sum(x => x.AmountPaid);
                    //nextGroupMd167DebtData.AmountDiff = nextGroupMd167DebtData.dataGroup[nextGroupMd167DebtData.length - 1].AmountDiff;
                    nextGroupMd167DebtData.AmountDiff = nextGroupMd167DebtData.AmountToBePaid;

                    AmountPaidPerMonth += nextGroupMd167DebtData.AmountPaidPerMonth;
                    AmountInterest += nextGroupMd167DebtData.AmountInterest;
                    AmountPaidInPeriod += nextGroupMd167DebtData.AmountPaidInPeriod;
                    //AmountToBePaid += (nextGroupMd167DebtData.AmountToBePaid - totalAmountDiff);
                    AmountToBePaid += nextGroupMd167DebtData.AmountToBePaid;
                    nextGroupMd167DebtData.AmountToBePaid += totalAmountDiff;

                    AmountPaid += nextGroupMd167DebtData.AmountPaid;
                }

                response.Add(nextGroupMd167DebtData);
            }

            CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");

            //Group tổng
            GroupMd167DebtData endMd167DebtData = new GroupMd167DebtData();
            endMd167DebtData.dataGroup = new List<Md167Debt>();
            endMd167DebtData.length = 1;
            Md167Debt endMd167Debt1 = new Md167Debt();
            endMd167Debt1.TypeRow = Md167DebtTypeRow.DONG_TONG;
            endMd167Debt1.Title = "Tổng";
            endMd167Debt1.AmountPaidPerMonth = AmountPaidPerMonth + res[2].AmountPaidPerMonth;
            endMd167Debt1.AmountInterest = AmountInterest + (res[2].AmountInterest ?? 0);
            endMd167Debt1.AmountPaidInPeriod = AmountPaidInPeriod + res[2].AmountPaidInPeriod;
            endMd167Debt1.AmountToBePaid = AmountToBePaid + res[2].AmountToBePaid;
            endMd167Debt1.AmountPaid = AmountPaid + res[2].AmountPaid;
            endMd167Debt1.AmountDiff = endMd167Debt1.AmountToBePaid - endMd167Debt1.AmountPaid;
            endMd167DebtData.dataGroup.Add(endMd167Debt1);

            document.ReplaceAll("<amounttobepaid>", endMd167Debt1.AmountToBePaid?.ToString("#,###", cul.NumberFormat) ?? "", SearchOptions.None);
            document.ReplaceAll("<amountpaid>", endMd167Debt1.AmountPaid?.ToString("#,###", cul.NumberFormat) ?? "", SearchOptions.None);
            document.ReplaceAll("<amountdiff>", endMd167Debt1.AmountDiff?.ToString("#,###", cul.NumberFormat) ?? "", SearchOptions.None);
            document.ReplaceAll("<amounttobepaidbangchu>", UtilsService.ConvertMoneyToString(endMd167Debt1.AmountToBePaid.ToString()).ToLower(), SearchOptions.None);
            document.ReplaceAll("<amountpaidbangchu>", UtilsService.ConvertMoneyToString(endMd167Debt1.AmountPaid.ToString()).ToLower(), SearchOptions.None);
            document.ReplaceAll("<amountdiffbangchu>", UtilsService.ConvertMoneyToString(endMd167Debt1.AmountDiff.ToString()).ToLower(), SearchOptions.None);

            response.Add(endMd167DebtData);

            return response;
        }
        #endregion

        #region hợp đồng 5
        [HttpPost("GetExportContract5/{id}")]
        public async Task<IActionResult> GetExportContract5(int id)
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
                string path = insertDataToTemplate5(id);
                if (path != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                        string fileName = "HopDong_MD167_05-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";

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
                log.Error("GetExportContract5:" + ex.Message);
                throw;
            }
        }

        private string insertDataToTemplate5(int id)
        {
            Md167House house = _context.Md167Houses.Where(f => f.Id == id && f.Status != EntityStatus.DELETED).FirstOrDefault();
            Md167HouseData map_md167house = _mapper.Map<Md167HouseData>(house);
            if (map_md167house != null)
            {
                string templatePath = Path.Combine(_hostingEnvironment.WebRootPath, "exportword/HopDong_MD167_05.docx");

                string fileName = "result" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";
                RichEditDocumentServer wordProcessor = new RichEditDocumentServer();
                wordProcessor.LoadDocumentTemplate(templatePath, DocumentFormat.OpenXml);
                var document = wordProcessor.Document;
                document.BeginUpdate();

                int dayNow = DateTime.Now.Day;
                int monthNow = DateTime.Now.Month;
                int yearNow = DateTime.Now.Year;
                decimal? totalLand = 0;
                decimal? floorbuild = 0;
                totalLand = map_md167house.InfoValue.UseLandPb + map_md167house.InfoValue.UseLandPr;
                floorbuild = map_md167house.InfoValue.AreaFloorBuild;

                CultureInfo cul = CultureInfo.GetCultureInfo("vi-VN");


                //ngày hiện tại
                string day = dayNow.ToString();
                document.ReplaceAll("<day>", day, SearchOptions.None);
                //tháng hiện tại
                string month = monthNow.ToString();
                document.ReplaceAll("<month>", month, SearchOptions.None);
                //năm hiện tại
                string year = yearNow.ToString();
                document.ReplaceAll("<year>", year, SearchOptions.None);
                //số nhà
                document.ReplaceAll("<housenumber>", map_md167house.HouseNumber ?? "", SearchOptions.None);

                // thông tin về bên thuê như địa chỉ, tổ chức hay cá nhân, mã thuế, ...
                Province province = _context.Provincies.Where(l => l.Id == map_md167house.ProvinceId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                document.ReplaceAll("<province>", province != null ? province.Name : "", SearchOptions.None);

                District district = _context.Districts.Where(l => l.Id == map_md167house.DistrictId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                document.ReplaceAll("<district>", district != null ? district.Name : "", SearchOptions.None);

                Ward ward = _context.Wards.Where(l => l.Id == map_md167house.WardId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                document.ReplaceAll("<ward>", ward != null ? ward.Name : "", SearchOptions.None);

                Lane lane = _context.Lanies.Where(l => l.Id == map_md167house.LaneId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                document.ReplaceAll("<lane>", lane != null ? lane.Name : "", SearchOptions.None);
                //tính diện tích đất sử dụng chung + riêng
                totalLand = map_md167house.InfoValue.UseLandPb + map_md167house.InfoValue.UseLandPr;

                if (map_md167house.TypeHouse == Type_House.House)
                {
                    document.ReplaceAll("<useland>", map_md167house != null ? totalLand.ToString() : "", SearchOptions.None);
                }

                //cho nhà chung cư/nhà nhiều tầng
                else
                {
                    document.ReplaceAll("<useland>", map_md167house != null ? totalLand.ToString() : "", SearchOptions.None);
                }

                //thửa số
                if (map_md167house.ParcelNumber != null)
                {
                    document.ReplaceAll("<parcelnumber>", map_md167house.ParcelNumber ?? "", SearchOptions.None);
                }
                else
                {
                    string replacementText = "………";
                    document.ReplaceAll("<parcelnumber>", replacementText, SearchOptions.None);
                }

                //tờ số
                if( map_md167house.MapNumber != null)
                {
                    document.ReplaceAll("<mapnumber>", map_md167house.MapNumber ?? "", SearchOptions.None);
                }
                else
                {
                    string replacementText = "………";
                    document.ReplaceAll("<mapnumber>", replacementText, SearchOptions.None);
                }

                //loại đất
                if (map_md167house.LandId == (int)Land_type.DAT_O)
                {
                    string landname =  Land_type_name.DAT_O;
                    document.ReplaceAll("<landname>", landname, SearchOptions.None);
                }
                else if (map_md167house.LandId == (int)Land_type.DAT_TMDV)
                {
                    string landname = Land_type_name.DAT_TMDV;
                    document.ReplaceAll("<landname>", landname, SearchOptions.None);
                }
                else if (map_md167house.LandId == (int)Land_type.DAT_SXKD)
                {
                    string landname = Land_type_name.DAT_SXKD;
                    document.ReplaceAll("<landname>", landname, SearchOptions.None);
                }
                else if (map_md167house.LandId == (int)Land_type.DAT_PUBLIC_KINHDOANH)
                {
                    string landname = Land_type_name.DAT_PUBLIC_KINHDOANH;
                    document.ReplaceAll("<landname>", landname, SearchOptions.None);
                }
                else if (map_md167house.LandId == (int)Land_type.DAT_COQUAN_CONGTRINH)
                {
                    string landname = Land_type_name.DAT_COQUAN_CONGTRINH;
                    document.ReplaceAll("<landname>", landname, SearchOptions.None);
                }
                else
                {
                    return null;
                }

                LandPrice landprice = _context.LandPricies.Where(x => x.District == district.Id && x.LandPriceType == landPriceType.MD167 && x.Status != EntityStatus.DELETED).FirstOrDefault();
                LandPriceItem landPriceItem = _context.LandPriceItems.Where(x => x.LandPriceId == landprice.Id && x.LaneName == lane.Name && x.Status != EntityStatus.DELETED).FirstOrDefault();
                LandPriceItemData map_landpriceitem = _mapper.Map<LandPriceItemData>(landPriceItem);
                if (map_landpriceitem != null)
                {
                    document.ReplaceAll("<lanestart>",map_landpriceitem.LaneStartName ?? "", SearchOptions.None);
                    document.ReplaceAll("<laneend>",map_landpriceitem.LaneEndName ?? "", SearchOptions.None);
                    document.ReplaceAll("<landprice>", map_landpriceitem.Value.ToString("#,###", cul.NumberFormat) ?? "", SearchOptions.None);

                }
                else 
                {
                    string replacementText = "………";
                    document.ReplaceAll("<lanestart>", replacementText, SearchOptions.None);
                    document.ReplaceAll("<laneend>", replacementText, SearchOptions.None);
                    document.ReplaceAll("<landprice>", replacementText, SearchOptions.None);
                }
                //var lstLandPrice = _context.LandPricies.Where(x => x.Status != EntityStatus.DELETED && x.LandPriceType == landPriceType.MD167).ToList();
                //var lstLandPriceItem = _context.LandPriceItems.Where(x => x.Status != EntityStatus.DELETED).ToList();


                //map_md167house.UnitPriceValueTotal = GetPrice(map_md167house.DistrictId, lstLandPrice, lstLandPriceItem, map_md167house.LaneName);

                Md167PositionValue md167PositionValue = _context.Md167PositionValues.Where(x=>x.Status != EntityStatus.DELETED).FirstOrDefault();
                if (md167PositionValue != null)
                {
                    string positionname1 = "Vị trí 1";
                    string positionname2 = "Vị trí 2";
                    string positionname3 = "Vị trí 3";
                    string positionname4 = "Vị trí 4";
                    string locationCoefficient = null;
                    if (map_md167house.Location == 1)
                    {
                        locationCoefficient = md167PositionValue.Position1;
                        document.ReplaceAll("<positionname>", positionname1, SearchOptions.None);
                        document.ReplaceAll("<locationcoefficient>", locationCoefficient, SearchOptions.None);
                    }
                    else if (map_md167house.Location == 2)
                    {
                        locationCoefficient = md167PositionValue.Position2;
                        document.ReplaceAll("<positionname>", positionname2, SearchOptions.None);
                        document.ReplaceAll("<locationcoefficient>", locationCoefficient, SearchOptions.None);
                    }
                    else if (map_md167house.Location == 3)
                    {
                        locationCoefficient = md167PositionValue.Position3;
                        document.ReplaceAll("<positionname>", positionname3, SearchOptions.None);
                        document.ReplaceAll("<locationcoefficient>", locationCoefficient, SearchOptions.None);
                    }
                    else if (map_md167house.Location == 4)
                    {
                        locationCoefficient = md167PositionValue.Position4;
                        document.ReplaceAll("<positionname>", positionname4, SearchOptions.None);
                        document.ReplaceAll("<locationcoefficient>", locationCoefficient, SearchOptions.None);
                    }
                    else
                    {
                        return null;
                    }
                }

                decimal? apax = map_md167house.InfoValue.ApaTax;


                document.ReplaceAll("<unitprice>",map_md167house.UnitPrice ?? "", SearchOptions.None);

                if(map_md167house.TypeHouse == Type_House.Apartment)
                {
                    document.ReplaceAll("<apatax>", apax?.ToString("#,###", cul.NumberFormat) ?? "", SearchOptions.None);
                }
                else if (map_md167house.TypeHouse == Type_House.House)
                {
                    string replacementText = "..............................................................................";
                    document.ReplaceAll("<apatax>", replacementText, SearchOptions.None);
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
        #endregion

    }
}
