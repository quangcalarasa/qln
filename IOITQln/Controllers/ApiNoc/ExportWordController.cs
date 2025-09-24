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
using static IOITQln.Common.Enums.AppEnums;
using System.Drawing;
using System.Security.Claims;
using IOITQln.Common.Constants;
using System.Globalization;
using Microsoft.Net.Http.Headers;

namespace IOITQln.Controllers.Api
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ExportWordController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("ExportWord", "ExportWord");
        private static string functionCode = "PRICING";
        private IWebHostEnvironment _hostingEnvironment;
        private readonly ApiDbContext _context;
        private IMapper _mapper;

        CultureInfo culture = CultureInfo.CreateSpecificCulture("de-DE");

        public ExportWordController(ApiDbContext context, IMapper mapper, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        [HttpPost("GetExportWordExample/{id}")]
        public async Task<IActionResult> GetExportWordExample(int id)
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
            catch(Exception ex)
            {
                log.Error("GetExportWordExample:" + ex.Message);
                throw;
            }
        }

        private string insertDataToTemplate(int id)
        {
            Pricing pricing = _context.Pricings.Where(p => p.Id == id && p.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
            if(pricing != null)
            {
                Block block = _context.Blocks.Where(b => b.Id == pricing.BlockId && b.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                Apartment apartment = _context.Apartments.Where(a => a.Id == pricing.ApartmentId && a.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                if (block != null)
                {
                    if((block.TypeReportApply == TypeReportApply.NHA_HO_CHUNG || block.TypeReportApply == TypeReportApply.NHA_CHUNG_CU || block.TypeReportApply == TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG) && apartment == null)
                    {
                        return null;
                    }

                    string templatePath = "";
                    switch(block.TypeReportApply)
                    {
                        case TypeReportApply.NHA_HO_CHUNG:
                            templatePath = Path.Combine(_hostingEnvironment.WebRootPath, "exportword/bien_ban_nha_nhieu_ho.docx");
                            break;
                        case TypeReportApply.NHA_RIENG_LE:
                            templatePath = Path.Combine(_hostingEnvironment.WebRootPath, "exportword/bien_ban_nha_rieng_le.docx");
                            break;
                        case TypeReportApply.NHA_CHUNG_CU:
                            templatePath = Path.Combine(_hostingEnvironment.WebRootPath, "exportword/bien_ban_nha_chung_cu.docx");
                            break;
                        case TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG:
                            templatePath = block.ParentTypeReportApply == TypeReportApply.NHA_CHUNG_CU ? Path.Combine(_hostingEnvironment.WebRootPath, "exportword/bien_ban_nha_chung_cu_phan_dien_tich_sd_chung.docx") : Path.Combine(_hostingEnvironment.WebRootPath, "exportword/bien_ban_nha_nhieu_ho_phan_dien_tich_sd_chung.docx");
                            break;
                        default:
                            break;
                    }

                    string fileName = "result" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";
                    RichEditDocumentServer wordProcessor = new RichEditDocumentServer();
                    wordProcessor.LoadDocumentTemplate(templatePath, DocumentFormat.OpenXml);
                    var document = wordProcessor.Document;
                    document.BeginUpdate();

                    string day = pricing.DateCreate != null ? (pricing.DateCreate.Value.Day < 10 ? "0" + pricing.DateCreate.Value.Day.ToString() : pricing.DateCreate.Value.Day.ToString()) : "";
                    document.ReplaceAll("<day>", day, SearchOptions.None);

                    string month = pricing.DateCreate != null ? (pricing.DateCreate.Value.Month < 10 ? "0" + pricing.DateCreate.Value.Month.ToString() : pricing.DateCreate.Value.Month.ToString()) : "";
                    document.ReplaceAll("<month>", month, SearchOptions.None);

                    string year = pricing.DateCreate != null ? pricing.DateCreate.Value.Year.ToString() : "";
                    document.ReplaceAll("<year>", year, SearchOptions.None);

                    List<DecreeMap> decreeMaps = _context.DecreeMaps.Where(d => d.TargetId == block.Id && d.Type == AppEnums.TypeDecreeMapping.BLOCK && d.Status != AppEnums.EntityStatus.DELETED).ToList();
                    List<string> decreeStr = getTitle(decreeMaps);
                    document.ReplaceAll("<title>", decreeStr[0], SearchOptions.None);
                    document.ReplaceAll("<listDecreeApply>", decreeStr[1], SearchOptions.None);

                    List<PricingReplaced> pricingReplaceds = _context.PricingReplaceds.Where(p => p.PricingId == pricing.Id && p.Status != EntityStatus.DELETED).ToList();
                    string strPricingReplaceds = string.Join(", ", pricingReplaceds.Select(x => x.DateCreate.Value.ToString("dd/MM/yyyy")).ToList());
                    document.ReplaceAll("<pricingReplaceds>", (strPricingReplaceds != "" ? strPricingReplaceds : "..............."), SearchOptions.None);

                    List<PricingOfficer> pricingOfficers = _context.PricingOfficers.Where(a => a.PricingId == pricing.Id && a.Status != AppEnums.EntityStatus.DELETED).ToList();
                    genPricingOfficerTbl(document, pricingOfficers);

                    //người đang thuê nhà ở
                    List<Customer> customers = (from c in _context.Customers
                                                       join pc in _context.PricingCustomers on c.Id equals pc.CustomerId
                                                       where c.Status != EntityStatus.DELETED
                                                       && pc.Status != EntityStatus.DELETED
                                                       && pc.PricingId == pricing.Id
                                                       select c).ToList();

                    string maleCustomer = "";
                    string femaleCustomer = "";
                    foreach(var customer in customers)
                    {
                        if(customer.Sex == TypeSex.FEMALE)
                        {
                            femaleCustomer = femaleCustomer == "" ? customer.FullName : femaleCustomer + ", " + customer.FullName;
                        }
                        else
                        {
                            maleCustomer = maleCustomer == "" ? customer.FullName : maleCustomer + ", " + customer.FullName;
                        }
                    }

                    document.ReplaceAll("<maleCustomer>", maleCustomer != "" ? maleCustomer : "✓", SearchOptions.None);
                    document.ReplaceAll("<femaleCustomer>", femaleCustomer != "" ? femaleCustomer : "✓", SearchOptions.None);

                    document.ReplaceAll("<timeUse>", pricing.TimeUse ?? "", SearchOptions.None);
                    document.ReplaceAll("<blockAdr>", block.Address ?? "", SearchOptions.None);

                    if(block.TypeReportApply == TypeReportApply.NHA_HO_CHUNG || block.TypeReportApply == TypeReportApply.NHA_CHUNG_CU || block.TypeReportApply == TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG)
                    {
                        document.ReplaceAll("<apartAdr>", apartment.Address, SearchOptions.None);
                    }

                    Lane lane = _context.Lanies.Where(l => l.Id == block.Lane && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    document.ReplaceAll("<lane>", lane != null ? lane.Name : "", SearchOptions.None);

                    Ward ward = _context.Wards.Where(l => l.Id == block.Ward && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    document.ReplaceAll("<ward>", ward != null ? ward.Name : "", SearchOptions.None);

                    District district = _context.Districts.Where(l => l.Id == block.District && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    document.ReplaceAll("<district>", district != null ? district.Name : "", SearchOptions.None);

                    string typeBlock = getTypeBlock(block.TypeBlockId);
                    document.ReplaceAll("<typeBlock>", typeBlock, SearchOptions.None);

                    List<LevelBlockMap> levelBlockMaps = _context.LevelBlockMaps.Where(l => l.BlockId == block.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    string levelBlock = getLevelBlock(levelBlockMaps);
                    document.ReplaceAll("<levelBlock>", levelBlock, SearchOptions.None);

                    string floorBlockMap = block.FloorBlockMap;
                    document.ReplaceAll("<floorBlockMap>", floorBlockMap, SearchOptions.None);

                    string bcav = block.ConstructionAreaValue != null ? block.ConstructionAreaValue.ToString("N2", culture) : "0";
                    document.ReplaceAll("<bcav>", bcav, SearchOptions.None);

                    string bscav = block.SellConstructionAreaValue != null ? (float.Parse(block.SellConstructionAreaValue, CultureInfo.InvariantCulture.NumberFormat)).ToString("N2", culture) : "0";
                    document.ReplaceAll("<bscav>", bscav, SearchOptions.None);

                    document.ReplaceAll("<bcaNote>", block.ConstructionAreaNote != null && block.ConstructionAreaNote != "" ? $"({block.ConstructionAreaNote})" : "", SearchOptions.None);

                    string bcav1 = block.ConstructionAreaValue1 != null ? ((float)block.ConstructionAreaValue1).ToString("N2", culture) : "0";
                    document.ReplaceAll("<bcav1>", bcav1, SearchOptions.None);

                    string bcav2 = block.ConstructionAreaValue2 != null ? ((float)block.ConstructionAreaValue2).ToString("N2", culture) : "0";
                    document.ReplaceAll("<bcav2>", bcav2, SearchOptions.None);

                    string bcav3 = block.ConstructionAreaValue3 != null ? ((float)block.ConstructionAreaValue3).ToString("N2", culture) : "0";
                    document.ReplaceAll("<bcav3>", bcav3, SearchOptions.None);

                    string buav = block.UseAreaValue != null ? block.UseAreaValue.ToString("N2", culture) : "0";
                    document.ReplaceAll("<buav>", buav, SearchOptions.None);

                    string buav1 = block.UseAreaValue1 != null ? ((float)block.UseAreaValue1).ToString("N2", culture) : "0";
                    document.ReplaceAll("<buav1>", buav1, SearchOptions.None);

                    string buav2 = block.UseAreaValue2 != null ? ((float)block.UseAreaValue2).ToString("N2", culture) : "0";
                    document.ReplaceAll("<buav2>", buav2, SearchOptions.None);

                    List<BlockDetail> blockDetails = _context.BlockDetails.Where(l => l.BlockId == block.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if (block.TypeReportApply == TypeReportApply.NHA_HO_CHUNG || block.TypeReportApply == TypeReportApply.NHA_CHUNG_CU || block.TypeReportApply == TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG)
                    {
                        if(block.TypeReportApply == TypeReportApply.NHA_HO_CHUNG)
                        {
                            //blockDetailTbl
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
                            map_BlockDetails = map_BlockDetails.OrderBy(x => x.FloorCode).ThenBy(x => x.IsMezzanine).ToList();
                            genBlockDetailTbl(document, map_BlockDetails);
                        }

                        //apartDetailInfo
                        List<ApartmentDetail> apartmentDetails = _context.ApartmentDetails.Where(a => a.TargetId == apartment.Id && a.Type == TypeApartmentDetail.APARTMENT && a.Status != AppEnums.EntityStatus.DELETED).ToList();
                        List<ApartmentDetailData> map_ApartmentDetails = _mapper.Map<List<ApartmentDetailData>>(apartmentDetails);
                        foreach (ApartmentDetailData map_ApartmentDetail in map_ApartmentDetails)
                        {
                            Floor floor = _context.Floors.Where(f => f.Id == map_ApartmentDetail.FloorId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            map_ApartmentDetail.FloorName = floor != null ? floor.Name : "";
                            map_ApartmentDetail.FloorCode = floor != null ? floor.Code : 0;

                            Area area = _context.Areas.Where(f => f.Id == map_ApartmentDetail.AreaId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            map_ApartmentDetail.AreaName = area != null ? area.Name : "";
                            map_ApartmentDetail.IsMezzanine = area != null ? area.IsMezzanine : null;
                        }

                        map_ApartmentDetails = map_ApartmentDetails.OrderBy(x => x.Level).ThenBy(x => x.FloorCode).ThenBy(x => x.IsMezzanine).ToList();

                        if(block.TypeReportApply == TypeReportApply.NHA_CHUNG_CU || (block.TypeReportApply == TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG || block.ParentTypeReportApply == TypeReportApply.NHA_CHUNG_CU))
                        {
                            string levelApartment = getLevelApartment(map_ApartmentDetails);
                            document.ReplaceAll("<levelApartment>", levelApartment, SearchOptions.None);
                        }

                        string apartDetailInfo = getApartDetailInfo(map_ApartmentDetails);
                        document.ReplaceAll("<apartDetailInfo>", apartDetailInfo, SearchOptions.None);

                        string acav = apartment.ConstructionAreaValue != null ? ((float)apartment.ConstructionAreaValue).ToString("N2", culture) : "0";
                        document.ReplaceAll("<acav>", acav, SearchOptions.None);

                        document.ReplaceAll("<acaNote>", apartment.ConstructionAreaNote != null && apartment.ConstructionAreaNote != "" ? $"({apartment.ConstructionAreaNote})" : "", SearchOptions.None);

                        string acav1 = apartment.ConstructionAreaValue1 != null ? ((float)apartment.ConstructionAreaValue1).ToString("N2", culture) : "0";
                        document.ReplaceAll("<acav1>", acav1, SearchOptions.None);

                        string acav2 = apartment.ConstructionAreaValue2 != null ? ((float)apartment.ConstructionAreaValue2).ToString("N2", culture) : "0";
                        document.ReplaceAll("<acav2>", acav2, SearchOptions.None);

                        string acav3 = apartment.ConstructionAreaValue3 != null ? ((float)apartment.ConstructionAreaValue3).ToString("N2", culture) : "0";
                        document.ReplaceAll("<acav3>", acav3, SearchOptions.None);

                        string auav = apartment.UseAreaValue != null ? ((float)apartment.UseAreaValue).ToString("N2", culture) : "0";
                        document.ReplaceAll("<auav>", auav, SearchOptions.None);

                        string auav1 = apartment.UseAreaValue1 != null ? ((float)apartment.UseAreaValue1).ToString("N2", culture) : "0";
                        document.ReplaceAll("<auav1>", auav1, SearchOptions.None);

                        string auav2 = apartment.UseAreaValue2 != null ? ((float)apartment.UseAreaValue2).ToString("N2", culture) : "0";
                        document.ReplaceAll("<auav2>", auav2, SearchOptions.None);

                        genApartDetailTbl(document, map_ApartmentDetails);
                    }
                    else
                    {
                        List<ApartmentDetail> apartmentDetails = _context.ApartmentDetails.Where(a => a.TargetId == block.Id && a.Type == TypeApartmentDetail.BLOCK && a.Status != AppEnums.EntityStatus.DELETED).ToList();
                        List<ApartmentDetailData> map_ApartmentDetails = _mapper.Map<List<ApartmentDetailData>>(apartmentDetails);
                        foreach (ApartmentDetailData map_ApartmentDetail in map_ApartmentDetails)
                        {
                            Floor floor = _context.Floors.Where(f => f.Id == map_ApartmentDetail.FloorId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            map_ApartmentDetail.FloorName = floor != null ? floor.Name : "";
                            map_ApartmentDetail.FloorCode = floor != null ? floor.Code : 0;

                            Area area = _context.Areas.Where(f => f.Id == map_ApartmentDetail.AreaId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            map_ApartmentDetail.AreaName = area != null ? area.Name : "";
                            map_ApartmentDetail.IsMezzanine = area != null ? area.IsMezzanine : null;
                        }

                        map_ApartmentDetails = map_ApartmentDetails.OrderBy(x => x.Level).ThenBy(x => x.FloorCode).ThenBy(x => x.IsMezzanine).ToList();
                        genApartDetailTbl(document, map_ApartmentDetails);
                    }

                    //Bảng tỷ lệ chất lượng còn lại
                    List<BlockMaintextureRateData> blockMaintextureRates = _context.BlockMaintextureRaties.Where(b => b.TargetId == block.Id && b.Type == AppEnums.TypeBlockMaintextureRate.BLOCK && b.Status != AppEnums.EntityStatus.DELETED).Select(e => new BlockMaintextureRateData {
                        LevelBlockId = e.LevelBlockId,
                        RatioMainTextureId = e.RatioMainTextureId,
                        TypeMainTexTure = e.TypeMainTexTure,
                        CurrentStateMainTextureId = e.CurrentStateMainTextureId,
                        RemainingRate = e.RemainingRate,
                        MainRate = e.MainRate,
                        TotalValue = e.TotalValue,
                        TotalValue1 = e.TotalValue1,
                        TotalValue2 = e.TotalValue2,
                        CurrentStateMainTextureName = _context.CurrentStateMainTexturies.Where( c => c.Id == e.CurrentStateMainTextureId).FirstOrDefault().Name,
                        TypeMainTexTureName = getTypeMainTexTureName(e.TypeMainTexTure)
                    }).ToList();

                    //Gộp dữ liệu theo cấp nhà
                    var blockMaintextureRatesGroupbyLevel = blockMaintextureRates.GroupBy(x => x.LevelBlockId).OrderBy(x => x.Key).ToList();
                    string dataStr = "";
                    string dataStr1 = "";
                    string dataStr2 = "";
                    blockMaintextureRatesGroupbyLevel.ForEach(item => {
                        string totalValue = item.FirstOrDefault().TotalValue != null ? ((float)item.FirstOrDefault().TotalValue).ToString("N2", culture) : "";
                        string totalValue1 = item.FirstOrDefault().TotalValue1 != null ? ((float)item.FirstOrDefault().TotalValue1).ToString("N2", culture) : "";
                        string totalValue2 = item.FirstOrDefault().TotalValue2 != null ? ((float)item.FirstOrDefault().TotalValue2).ToString("N2", culture) : "";

                        dataStr = dataStr == "" ? $"- Phần nhà cấp {item.Key}    =     {item.FirstOrDefault().TotalValue} %" : String.Join("\n", dataStr, $"- Phần nhà cấp {item.Key}    =     {totalValue} %");
                        dataStr1 = dataStr1 == "" ? $"* Phần nhà cấp {item.Key}    =     {item.FirstOrDefault().TotalValue1} %" : String.Join("\n", dataStr1, $"* Phần nhà cấp {item.Key}    =     {totalValue1} %");
                        dataStr2 = dataStr2 == "" ? $"* Phần nhà cấp {item.Key}    =     {item.FirstOrDefault().TotalValue2} %" : String.Join("\n", dataStr2, $"* Phần nhà cấp {item.Key}    =     {totalValue2} %");
                    });

                    genBlockMaintextureRateTbla(document, blockMaintextureRates, dataStr1);
                    genBlockMaintextureRateTblb(document, dataStr2);
                    genBlockMaintextureRateTblc(document, dataStr);

                    //Giá trị còn lại của nhà ở
                    List<PricingLandTbl> landPricingTbl = _context.PricingLandTbls.Where(a => a.PricingId == pricing.Id && a.Status != AppEnums.EntityStatus.DELETED).ToList();
                    List<PricingLandTblData> pricingLandTblDatas = _mapper.Map<List<PricingLandTblData>>(landPricingTbl);
                    pricingLandTblDatas.ForEach(pricingLandTblData => {
                        Area area = _context.Areas.Where(a => a.Id == pricingLandTblData.AreaId && a.Status != EntityStatus.DELETED).FirstOrDefault();
                        pricingLandTblData.AreaName = area != null ? area.Name : "";

                        PriceListItem priceListItem = _context.PriceListItems.Where(p => p.Id == pricingLandTblData.PriceListItemId).FirstOrDefault();
                        if(priceListItem != null)
                        {
                            PriceList priceList = _context.PriceLists.Where(p => p.Id == priceListItem.PriceListId).FirstOrDefault();

                            pricingLandTblData.PriceListItemNote = "Căn cứ " + priceListItem.NameOfConstruction + (priceList?.Des != null ? " theo " + priceList?.Des : "");
                        }

                        //Check có phải là Suất vồn đầu tư hay không
                        if(block.TypeReportApply == TypeReportApply.NHA_CHUNG_CU || (block.TypeReportApply == TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG && block.ParentTypeReportApply == TypeReportApply.NHA_CHUNG_CU))
                        {
                            if(pricingLandTblData.DecreeType1Id == DecreeEnum.ND_CP_99_2015 && pricingLandTblData.ApplyInvestmentRate == true && pricingLandTblData.IsMezzanine != true)
                            {
                                pricingLandTblData.ApplyInvestmentRate = true;
                            }
                            else
                            {
                                pricingLandTblData.ApplyInvestmentRate = false;
                            }
                        }
                        else
                        {
                            pricingLandTblData.ApplyInvestmentRate = false;
                        }
                    });

                    List<ConstructionPrice> constructionPrices = (from pcp in _context.PricingConstructionPricies
                                                                  join cp in _context.ConstructionPricies on pcp.ConstructionPriceId equals cp.Id
                                                                  where pcp.Status != EntityStatus.DELETED
                                                                  && cp.Status != EntityStatus.DELETED
                                                                  && pcp.PricingId == pricing.Id
                                                                  select cp).OrderBy(x => x.Year).ToList();

                    genPricingLand(document, pricing.DateCreate, pricingLandTblDatas, constructionPrices, pricing.Vat, pricing.ApartmentPrice, block.TypeReportApply, pricing.AreaCorrectionCoefficientId, block.ParentTypeReportApply);

                    if(block.TypeReportApply != TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG)
                    {
                        //Tiền mua nhà được giảm
                        document.ReplaceAll("<apartmentPriceReducedNote>", pricing.ApartmentPriceReducedNote ?? ".....", SearchOptions.None);
                        List<PricingReducedPersonData> reducedPeople = _context.PricingReducedPeople.Where(a => a.PricingId == pricing.Id && a.Status != AppEnums.EntityStatus.DELETED).Select(e => new PricingReducedPersonData
                        {
                            CustomerName = _context.Customers.Where(c => c.Id == e.CustomerId).FirstOrDefault().FullName,
                            Year = e.Year,
                            Salary = e.Salary,
                            DeductionCoefficient = e.DeductionCoefficient,
                            Value = e.Value
                        }).ToList();
                        genReducedPersonTbl(document, reducedPeople);
                    }

                    //Chi tiết tiền nhà
                    document.ReplaceAll("<apartmentPriceReduced>", UtilsService.FormatMoney(pricing.ApartmentPriceReduced ?? 0), SearchOptions.None);
                    document.ReplaceAll("<apartmentPrice>", UtilsService.FormatMoney(pricing.ApartmentPrice), SearchOptions.None);
                    document.ReplaceAll("<apartmentPriceRemaining>", UtilsService.FormatMoney(pricing.ApartmentPriceRemaining), SearchOptions.None);
                    document.ReplaceAll("<apartmentPriceRemainingStr>", UtilsService.ConvertMoneyToString(((decimal)pricing.ApartmentPriceRemaining).ToString("N0", culture)).ToLower(), SearchOptions.None);
                    document.ReplaceAll("<vat>", pricing.Vat != null ? ((float)pricing.Vat).ToString("N2", culture) + "%" : "0%", SearchOptions.None);
                    document.ReplaceAll("<apartmentPriceNoVat>", UtilsService.FormatMoney(pricing.ApartmentPriceNoVat), SearchOptions.None);
                    document.ReplaceAll("<apartmentPriceVat>", UtilsService.FormatMoney(pricing.ApartmentPriceVat), SearchOptions.None);

                    //II. ĐẤT Ở
                    document.ReplaceAll("<landUsePlanningInfo>", block.LandUsePlanningInfo ?? "", SearchOptions.None);
                    document.ReplaceAll("<highwayPlanningInfo>", block.HighwayPlanningInfo ?? "", SearchOptions.None);
                    document.ReplaceAll("<landAcquisitionSituationInfo>", block.LandAcquisitionSituationInfo ?? "", SearchOptions.None);
                    document.ReplaceAll("<landNo>", block.LandNo ?? "", SearchOptions.None);
                    document.ReplaceAll("<mapNo>", block.MapNo ?? "", SearchOptions.None);

                    if(block.TypeReportApply == TypeReportApply.NHA_HO_CHUNG || block.TypeReportApply == TypeReportApply.NHA_CHUNG_CU || block.TypeReportApply == TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG)
                    {
                        document.ReplaceAll("<alcav>", apartment.LandscapeAreaValue != null ? ((float)apartment.LandscapeAreaValue).ToString("N2", culture) : "0", SearchOptions.None); ;
                        document.ReplaceAll("<alcav1>", apartment.LandscapeAreaValue1 != null ? ((float)apartment.LandscapeAreaValue1).ToString("N2", culture) : "0", SearchOptions.None);
                        document.ReplaceAll("<alcav2>", apartment.LandscapeAreaValue2 != null ? ((float)apartment.LandscapeAreaValue2).ToString("N2", culture) : "0", SearchOptions.None);
                        document.ReplaceAll("<alcav3>", apartment.LandscapeAreaValue3 != null ? ((float)apartment.LandscapeAreaValue3).ToString("N2", culture) : "0", SearchOptions.None);
                    }
                    else if(block.TypeReportApply == TypeReportApply.NHA_RIENG_LE)
                    {
                        document.ReplaceAll("<alcav>", block.LandscapeAreaValue != null ? ((float)block.LandscapeAreaValue).ToString("N2", culture) : "0", SearchOptions.None); ;
                        document.ReplaceAll("<alcav2>", block.LandscapePrivateAreaValue != null ? ((float)block.LandscapePrivateAreaValue).ToString("N2", culture) : "0", SearchOptions.None);
                        document.ReplaceAll("<alcav1>", " ", SearchOptions.None);
                    }

                    List<PricingApartmentLandDetail> pricingApartmentLandDetails = _context.PricingApartmentLandDetails.Where(a => a.PricingId == pricing.Id && a.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                    genApartmentLandDetailTbl(document, pricingApartmentLandDetails, block.TypeReportApply);

                    // Tính các khoản tiền đất, tổng giá đất và giá cuối cùng
                    List<PricingApartmentLandDetailGroupByDecree> pricingApartmentLandDetailGroupByDecrees = _context.PricingApartmentLandDetails.Where(a => a.PricingId == pricing.Id && a.Status != AppEnums.EntityStatus.DELETED).ToList().GroupBy(x => x.DecreeType1Id).Select(x => new PricingApartmentLandDetailGroupByDecree {
                        Decree = x.Key,
                        pricingApartmentLandDetails = x.ToList()
                    }).ToList();

                    pricingApartmentLandDetailGroupByDecrees.ForEach(pricingApartmentLandDetailGroupByDecree => {
                        string landPriceItemNote = "";
                        float? landPriceRefinement = null;

                        int landPriceItemId = 0;
                        double? landPriceItemValue = null;
                        string positionCoefficientStr = "";
                        string landScapePrice = "";

                        switch (pricingApartmentLandDetailGroupByDecree.Decree)
                        {
                            case DecreeEnum.ND_CP_99_2015:
                                landPriceItemId = block.LandPriceItemId_99 ?? 0;
                                landPriceItemValue = block.LandPriceItemValue_99;
                                positionCoefficientStr = block.PositionCoefficientStr_99;
                                landPriceRefinement = block.LandPriceRefinement_99;
                                landScapePrice = $"{UtilsService.FormatMoney(Convert.ToDecimal(landPriceItemValue))} đồng/m2 x {positionCoefficientStr}" + (block.ExceedingLimitDeep == true ? $" x (100 - {landPriceRefinement})%" : "") + $" = {UtilsService.FormatMoney(Convert.ToDecimal(block.LandScapePrice_99))} đồng/m2";
                                break;
                            case DecreeEnum.ND_CP_34_2013:
                                landPriceItemId = block.LandPriceItemId_34 ?? 0;
                                landPriceItemValue = block.LandPriceItemValue_34;
                                positionCoefficientStr = block.CaseApply_34 == TypeCaseApply_34.KHOAN_2 ? block.PositionCoefficientStr_34 : block.AlleyPositionCoefficientStr_34;
                                landPriceRefinement = block.LandPriceRefinement_34;
                                landScapePrice = $"{UtilsService.FormatMoney(Convert.ToDecimal(landPriceItemValue))} đồng/m2 x {positionCoefficientStr}" + (block.ExceedingLimitDeep == true && block.CaseApply_34 == TypeCaseApply_34.KHOAN_2 ? $" x (100 - {landPriceRefinement})%" : "") + $" = {UtilsService.FormatMoney(Convert.ToDecimal(block.CaseApply_34 == TypeCaseApply_34.KHOAN_2 ? block.LandScapePrice_34 : block.AlleyLandScapePrice_34))} đồng/m2";
                                break;
                            case DecreeEnum.ND_CP_61:
                                landPriceItemId = block.LandPriceItemId_61 ?? 0;
                                landPriceItemValue = block.LandPriceItemValue_61;
                                positionCoefficientStr = block.PositionCoefficientStr_61;
                                landPriceRefinement = block.LandPriceRefinement_61;
                                if(block.IsFrontOfLine_61 == true || (block.IsFrontOfLine_61 != true && block.IsAlley_61 != true))
                                {
                                    landScapePrice = $"{UtilsService.FormatMoney(Convert.ToDecimal(block.LandScapePrice_61))} đồng/m2";
                                }
                                else
                                {
                                    landScapePrice = $"{UtilsService.FormatMoney(Convert.ToDecimal(block.No2LandScapePrice_61))} đồng/m2 x 0.8 = {UtilsService.FormatMoney(Convert.ToDecimal(block.LandScapePrice_61))} đồng/m2";
                                }
                                break;
                            default:
                                break;
                        }

                        LandPriceItem landPriceItem = _context.LandPriceItems.Where(l => l.Id == landPriceItemId).FirstOrDefault();
                        if (landPriceItem != null)
                        {
                            landPriceItemNote = $"{landPriceItem.LaneName}, đoạn từ {landPriceItem.LaneStartName}" + (landPriceItem.LaneEndName != null && landPriceItem.LaneEndName != "" ? $" đến đoạn {landPriceItem.LaneEndName}" : "");
                            landPriceItemNote = landPriceItem.Des != null ? (landPriceItemNote + $", {landPriceItem.Des}") : landPriceItemNote;

                            LandPrice landPrice = _context.LandPricies.Where(l => l.Id == landPriceItem.LandPriceId).FirstOrDefault();
                            if (landPrice != null)
                                landPriceItemNote = $"{landPriceItemNote}, {landPrice.Des}";
                        }

                        pricingApartmentLandDetailGroupByDecree.LandPriceItemNote = landPriceItemNote;
                        pricingApartmentLandDetailGroupByDecree.LandPriceItemValue = Convert.ToDecimal(landPriceItemValue);
                        pricingApartmentLandDetailGroupByDecree.LandScapePrice = landScapePrice;
                        pricingApartmentLandDetailGroupByDecree.LandPriceRefinement = landPriceRefinement;
                        pricingApartmentLandDetailGroupByDecree.PositionCoefficientStr = positionCoefficientStr;
                    });

                    genApartmentLandDetailInfo(document, pricingApartmentLandDetailGroupByDecrees, block, pricing, landPricingTbl, blockDetails, apartment != null ? apartment.LandscapeAreaValue1 : null);

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

        //Lấy tiêu đề biên bản
        private List<string> getTitle(List<DecreeMap> decreeMaps)
        {
            string title = ""; ;
            string listDecreeApply = "";

            decreeMaps.ForEach(decreeMap =>
            {
                string decreeName = "";
                string decreeNameDetail = "";
                //if(decreeMap.DecreeType1Id == AppEnums.DecreeEnum.ND_CP_34_2013)
                switch (decreeMap.DecreeType1Id)
                {
                    case AppEnums.DecreeEnum.ND_CP_99_2015:
                        decreeNameDetail = "NGHỊ ĐỊNH SỐ 99/2015/NĐ-CP NGÀY 20/10/2015";
                        decreeName = "Nghị định số 99/2015/NĐ-CP";
                        break;
                    case AppEnums.DecreeEnum.ND_CP_34_2013:
                        decreeNameDetail = "NGHỊ ĐỊNH 34/2013/NĐ-CP NGÀY 22/04/2013";
                        decreeName = "Nghị định số 34/2013/NĐ-CP";
                        break;
                    case AppEnums.DecreeEnum.ND_CP_61:
                        decreeNameDetail = "NGHỊ ĐỊNH SỐ 61";
                        decreeName = "Nghị định số 61";
                        break;
                    default: break;
                }

                title = title == "" ? $"XÁC ĐỊNH GIÁ BÁN NHÀ THEO {decreeNameDetail}" : $"{title} VÀ {decreeNameDetail}";
                listDecreeApply = listDecreeApply == "" ? decreeName : $"{listDecreeApply} và {decreeName}";
            });

            title = title == "" ? title : $"{title} CỦA CHÍNH PHỦ";

            return new List<string>{ title , listDecreeApply };
        }

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

            levelBlockMaps.ForEach(levelBlockMap => {
                levelBlock = levelBlock == "" ? $"Cấp { levelBlockMap.LevelId }" : levelBlock + $" - { levelBlockMap.LevelId }";
            });

            return levelBlock;
        }

        //Lấy chi tiết tầng căn hộ
        private string getApartDetailInfo(List<ApartmentDetailData> apartmentDetails)
        {
            string apartDetailInfo = "";
            apartmentDetails.ForEach(apartmentDetail => {
                apartDetailInfo = apartDetailInfo == "" ? apartmentDetail.AreaName : $"{apartDetailInfo} + { apartmentDetail.AreaName }";
            });

            return apartDetailInfo;
        }

        //Lấy cấp nhà của căn hộ
        private string getLevelApartment(List<ApartmentDetailData> apartmentDetails)
        {
            string str = "";
            var group_data = apartmentDetails.GroupBy(x => x.Level).ToList();
            group_data.ForEach(item => {
                str = str == "" ? $"Cấp {item.Key}" : $"{str} + Cấp {item.Key}";
            });

            return str;
        }

        //Lấy tên Nghị định
        private string getDecreeName(DecreeEnum? decree)
        {
            string name = "";
            switch(decree)
            {
                case DecreeEnum.ND_CP_99_2015:
                    name = "Nghị định 99/2015/NĐ-CP";
                    break;
                case DecreeEnum.ND_CP_34_2013:
                    name = "Nghị định 34/2013/NĐ-CP";
                    break;
                case DecreeEnum.ND_CP_61:
                    name = "Nghị định 61";
                    break;
                default:
                    break;
            }

            return name;
        }

        //Lấy tên điều
        private string getTermApplyName(TermApply? termApply)
        {
            string name = "";
            switch (termApply)
            {
                case TermApply.KHOAN_1_DIEU_34:
                    name = "Khoản 1 điều 34";
                    break;
                case TermApply.KHOAN_2_DIEU_34:
                    name = "Khoản 2 điều 34";
                    break;
                case TermApply.DIEU_35:
                    name = "Điều 35";
                    break;
                case TermApply.DIEU_65:
                    name = "Điều 65";
                    break;
                case TermApply.DIEU_70:
                    name = "Điều 70";
                    break;
                case TermApply.DIEU_71:
                    name = "Điều 71";
                    break;
                case TermApply.DIEU_7:
                    name = "Điều 7";
                    break;
                default:
                    break;
            }

            return name;
        }

        //Lấy tên Kết cấu chính
        private static string getTypeMainTexTureName(TypeMainTexTure typeMainTexTure)
        {
            string name = "";

            switch(typeMainTexTure)
            {
                case TypeMainTexTure.MONG:
                    name = "Móng";
                    break;
                case TypeMainTexTure.KHUNG_COT:
                    name = "Khung cột";
                    break;
                case TypeMainTexTure.TUONG:
                    name = "Tường";
                    break;
                case TypeMainTexTure.NEN_SAN:
                    name = "Nền sàn";
                    break;
                case TypeMainTexTure.KHUNG_COT_DO_MAI:
                    name = "K/c đỡ mái";
                    break;
                case TypeMainTexTure.MAI:
                    name = "Mái";
                    break;
                default: break;
            }

            return name;
        }

        //Lấy chuỗi chỉ số giá xây dựng công trình
        private string getConstructionPriceStr(List<ConstructionPrice> constructionPrices)
        {
            string str = "";

            constructionPrices.ForEach(constructionPrice => {
                str = str == "" ? $"{ constructionPrice.Value.ToString("N2", culture) }%" : str + $" x { constructionPrice.Value.ToString("N2", culture) }%";
            });

            return str;
        }

        //Lấy vị trí đất ở, vị trí hẻm
        private string getLocationResidentialLand(LocationResidentialLand? id) {
            string str = "";

            switch(id)
            {
                case LocationResidentialLand.FIRST:
                    str = "Vị trí 1";
                    break;
                case LocationResidentialLand.SECOND:
                    str = "Vị trí 2";
                    break;
                case LocationResidentialLand.THIRD:
                    str = "Vị trí 3";
                    break;
                case LocationResidentialLand.FOURTH:
                    str = "Vị trí 4";
                    break;
                default: break;
            }

            return str;
        }

        //Lấy cấp hẻm
        private string getLevelAlley(LevelAlley? id)
        {
            string str = "";

            switch (id)
            {
                case LevelAlley.LEVEL_1:
                    str = "Hẻm cấp 1";
                    break;
                case LevelAlley.LEVEL_2:
                    str = "Hẻm cấp 2";
                    break;
                case LevelAlley.LEVEL_3:
                    str = "Hẻm cấp còn lại";
                    break;
                default: break;
            }

            return str;
        }

        //Lấy loại hẻm
        private string genTypeAlley(TypeAlley? id)
        {
            string str = "";

            switch (id)
            {
                case TypeAlley.MAIN:
                    str = "Đất nằm mặt tiền hẻm chính";
                    break;
                case TypeAlley.EXTRA:
                    str = "Đất nằm ở hẻm phụ";
                    break;
                default: break;
            }

            return str;
        }

        //Tổ kỹ thuật - Tính giá
        private void genPricingOfficerTbl(Document document, List<PricingOfficer> pricingOfficers)
        {
            //Gen Table
            DocumentRange[] ranges = document.FindAll("<pricingOfficerTbl>", DevExpress.XtraRichEdit.API.Native.SearchOptions.None);
            int row_number = pricingOfficers.Count;
            if (ranges.Length > 0 && row_number > 0)
            {
                Table table = document.Tables.Create(ranges[0].Start, row_number, 3, AutoFitBehaviorType.AutoFitToContents);
                table.ForEachCell((cell, i, j) => {
                    if (j == 0)
                    {
                        cell.PreferredWidth = 400;
                        cell.PreferredWidthType = WidthType.Fixed;
                        document.InsertSingleLineText(cell.Range.Start, $"{ i + 1 }.");
                    }
                    else if (j == 1)
                    {
                        cell.PreferredWidth = 4000;
                        cell.PreferredWidthType = WidthType.Fixed;
                        document.InsertSingleLineText(cell.Range.Start, pricingOfficers[i].Name);
                    }
                    else
                    {
                        cell.PreferredWidth = 2500;
                        cell.PreferredWidthType = WidthType.Fixed;
                        document.InsertSingleLineText(cell.Range.Start, pricingOfficers[i].Function);
                    }

                    cell.Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                    cell.Borders.Top.LineStyle = TableBorderLineStyle.None;
                    cell.Borders.Right.LineStyle = TableBorderLineStyle.None;
                    cell.Borders.Left.LineStyle = TableBorderLineStyle.None;
                });
                table.TableAlignment = TableRowAlignment.Center;
            }
            else
            {
                //Xuống dòng
                var breakLine = document.InsertText(ranges[0].Start, "\n");
            }
            document.ReplaceAll("<pricingOfficerTbl>", "", SearchOptions.None);
        }

        //Chi tiết căn nhà
        private void genBlockDetailTbl(Document document, List<BlockDetailData> blockDetails)
        {
            //Gen Table
            DocumentRange[] ranges = document.FindAll("<blockDetailTbl>", DevExpress.XtraRichEdit.API.Native.SearchOptions.None);
            int row_number = blockDetails.Count;
            if (ranges.Length > 0 && row_number > 0)
            {
                Table table = document.Tables.Create(ranges[0].Start, row_number, 4, AutoFitBehaviorType.AutoFitToWindow);
                table.ForEachCell((cell, i, j) => {
                    if (j == 0)
                    {
                        cell.PreferredWidth = 1000;
                        cell.PreferredWidthType = WidthType.Fixed;
                        if (i == 0)
                            document.InsertSingleLineText(cell.Range.Start, $"- {blockDetails[i].FloorName}: ");
                        else
                        {
                            if (blockDetails[i - 1].FloorId == blockDetails[i].FloorId)
                                document.InsertSingleLineText(cell.Range.Start, $"");
                            else
                                document.InsertSingleLineText(cell.Range.Start, $"- {blockDetails[i].FloorName}: ");
                        }
                    }
                    else if (j == 1)
                    {
                        cell.PreferredWidth = 1000;
                        cell.PreferredWidthType = WidthType.Fixed;
                        string totalAreaFloor = blockDetails[i].TotalAreaFloor != null ? ((float)blockDetails[i].TotalAreaFloor).ToString("N2", culture) : "";

                        if (i == 0)
                            document.InsertSingleLineText(cell.Range.Start, $"{totalAreaFloor} m2");
                        else
                        {
                            if (blockDetails[i - 1].FloorId == blockDetails[i].FloorId)
                                document.InsertSingleLineText(cell.Range.Start, $"");
                            else
                                document.InsertSingleLineText(cell.Range.Start, $"{totalAreaFloor} m2");
                        }
                    }
                    else if (j == 2)
                    {
                        cell.PreferredWidth = 2000;
                        cell.PreferredWidthType = WidthType.Fixed;
                        string isMezzanine = blockDetails[i].IsMezzanine != true ? "\u2192" : "\u2198";
                        document.InsertSingleLineText(cell.Range.Start, $"{isMezzanine} {blockDetails[i].AreaName} = {blockDetails[i].TotalAreaDetailFloor} m2");
                    }
                    else
                    {
                        cell.PreferredWidth = 2000;
                        cell.PreferredWidthType = WidthType.Fixed;
                        string generalArea = blockDetails[i].GeneralArea != null ? ((float)blockDetails[i].GeneralArea).ToString("N2", culture) : "";
                        string privateArea = blockDetails[i].PrivateArea != null ? ((float)blockDetails[i].PrivateArea).ToString("N2", culture) : "";

                        document.InsertText(cell.Range.Start, $"\u2192 (nhà nhiều hộ) = {generalArea} m2 \n\u2198 (nhà riêng lẻ) = {privateArea} m2");
                    }

                    cell.Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                    cell.Borders.Top.LineStyle = TableBorderLineStyle.None;
                    cell.Borders.Right.LineStyle = TableBorderLineStyle.None;
                    cell.Borders.Left.LineStyle = TableBorderLineStyle.None;
                });
                table.TableAlignment = TableRowAlignment.Center;
            }
            else
            {
                //Xuống dòng
                var breakLine = document.InsertText(ranges[0].Start, "\n");
            }

            document.ReplaceAll("<blockDetailTbl>", "", SearchOptions.None);
        }

        //Bảng chi tiết tầng căn hộ
        private void genApartDetailTbl(Document document, List<ApartmentDetailData> apartmentDetails)
        {
            DocumentRange[] ranges = document.FindAll("<apartDetailTbl>", DevExpress.XtraRichEdit.API.Native.SearchOptions.None);
            List<ApartmentDetailDataGroupByLevel> groupData = apartmentDetails.GroupBy(x => x.Level).Select(x => new ApartmentDetailDataGroupByLevel {
                Level = x.Key,
                apartmentDetailDatas = x.ToList()
            }).ToList();

            int table_number = groupData.Count;
            if (ranges.Length > 0 && table_number > 0)
            {
                DocumentPosition documentPosition = ranges[0].Start;
                int index = 1;
                groupData.ForEach(data => {
                    int row_number = data.apartmentDetailDatas.Count;
                    List<ApartmentDetailData> apartmentDetailDatas = data.apartmentDetailDatas;

                    Table table = document.Tables.Create(documentPosition, row_number + 1, 4, AutoFitBehaviorType.AutoFitToWindow);
                    table.ForEachCell((cell, i, j) => {
                        if (j == 0)
                        {
                            cell.PreferredWidth = 15;
                            cell.PreferredWidthType = WidthType.FiftiethsOfPercent;
                            if (i == 0)
                                document.InsertSingleLineText(cell.Range.Start, $"Cấp {data.Level}");
                            else
                            {
                                document.InsertSingleLineText(cell.Range.Start, apartmentDetailDatas[i - 1].AreaName);
                            }
                        }
                        else if (j == 1)
                        {
                            cell.PreferredWidth = 15;
                            cell.PreferredWidthType = WidthType.FiftiethsOfPercent;
                            if (i == 0)
                            {
                                CharacterProperties cpCell = document.BeginUpdateCharacters(cell.Range);
                                cpCell.Bold = true;
                                float sumAreaLevel = apartmentDetailDatas.Sum(x => (x.GeneralArea ?? 0) + (x.PrivateArea ?? 0));
                                document.InsertSingleLineText(cell.Range.Start, $"{sumAreaLevel.ToString("N2", culture)}");
                            }    
                            else
                            {
                                float sumArea = (apartmentDetailDatas[i - 1].GeneralArea ?? 0) + (apartmentDetailDatas[i - 1].PrivateArea ?? 0);
                                document.InsertSingleLineText(cell.Range.Start, $"{sumArea.ToString("N2", culture)}");
                            }
                        }
                        else if (j == 2)
                        {
                            cell.PreferredWidth = 15;
                            cell.PreferredWidthType = WidthType.FiftiethsOfPercent;
                            document.InsertSingleLineText(cell.Range.Start, $"m2");
                        }
                        else
                        {
                            cell.PreferredWidth = 55;
                            cell.PreferredWidthType = WidthType.FiftiethsOfPercent;
                            if (i != 0)
                                document.InsertSingleLineText(cell.Range.Start, $"Áp dụng theo {getTermApplyName(apartmentDetailDatas[i - 1].TermApply)}, {getDecreeName(apartmentDetailDatas[i - 1].DecreeType1Id)}");
                        }

                        cell.VerticalAlignment = TableCellVerticalAlignment.Center;

                        ParagraphProperties pp = document.BeginUpdateParagraphs(cell.Range);
                        pp.Alignment = ParagraphAlignment.Center;
                        document.EndUpdateParagraphs(pp);
                    });
                    table.TableAlignment = TableRowAlignment.Center;

                    if(index < table_number)
                    {
                        var breakLine = document.InsertText(table.Range.End, "\n");
                        documentPosition = breakLine.End;
                    }
                    index++;
                });
            }
            else
            {
                //Xuống dòng
                var breakLine = document.InsertText(ranges[0].Start, "\n");
            }

            document.ReplaceAll("<apartDetailTbl>", "", SearchOptions.None);
        }

        //Bảng chất lượng còn lại của căn nhà theo pp kinh tế kỹ thuật
        private void genBlockMaintextureRateTbla(Document document, List<BlockMaintextureRateData> blockMaintextureRateDatas, string str)
        {
            DocumentRange[] ranges = document.FindAll("<maintextureRateTbla>", DevExpress.XtraRichEdit.API.Native.SearchOptions.None);
            List<BlockMaintextureRateDataGroupByTypeMainTexTure> blockMaintextureRateDataGroupByTypeMainTexTures = blockMaintextureRateDatas.GroupBy(x => x.TypeMainTexTure).Select(x => new BlockMaintextureRateDataGroupByTypeMainTexTure { 
                TypeMainTexTure = x.Key,
                blockMaintextureRateDatas = x.ToList()
            }).OrderBy(x => x.TypeMainTexTure).ToList();

            if(blockMaintextureRateDataGroupByTypeMainTexTures.Count > 0)
            {
                Table table = document.Tables.Create(ranges[0].Start, 8, 6, AutoFitBehaviorType.FixedColumnWidth);
                //table[7, 0].Split(7, 2);
                ////table.MergeCells(table[7, 1], table[7, 3]);

                List<BlockMaintextureRateData> childBlockMaintextureRateDatas = new List<BlockMaintextureRateData>();
                string levelColumn = "";
                string currMainTextureNameColumn = "";
                string remainingRateColumn = "";
                string mainRateColumn = "";

                table.ForEachCell((cell, i, j) => {
                    if (i > 0 && i < 7 && j == 0)
                    {
                        levelColumn = "";
                        currMainTextureNameColumn = "";
                        remainingRateColumn = "";
                        mainRateColumn = "";

                        childBlockMaintextureRateDatas = blockMaintextureRateDataGroupByTypeMainTexTures[i - 1].blockMaintextureRateDatas.OrderBy( x=> x.LevelBlockId).ToList();
                        
                        for(int idx = 0; idx < childBlockMaintextureRateDatas.Count; idx++)
                        {
                            var childBlockMaintextureRateData = childBlockMaintextureRateDatas[idx];
                            currMainTextureNameColumn = currMainTextureNameColumn == "" ? childBlockMaintextureRateData.CurrentStateMainTextureName : String.Join("\n", currMainTextureNameColumn, childBlockMaintextureRateData.CurrentStateMainTextureName);
                            remainingRateColumn = remainingRateColumn == "" ? childBlockMaintextureRateData.RemainingRate.ToString("N2", culture) : String.Join("\n", remainingRateColumn, childBlockMaintextureRateData.RemainingRate.ToString("N2", culture));
                            mainRateColumn = mainRateColumn == "" ? childBlockMaintextureRateData.MainRate.ToString("N2", culture) : String.Join("\n", mainRateColumn, childBlockMaintextureRateData.MainRate.ToString("N2", culture));

                            if(idx != 0)
                            {
                                var prevChildBlockMaintextureRateData = childBlockMaintextureRateDatas[idx - 1];
                                if(childBlockMaintextureRateData.LevelBlockId == prevChildBlockMaintextureRateData.LevelBlockId)
                                {
                                    levelColumn = levelColumn == "" ? childBlockMaintextureRateData.LevelBlockId.ToString() : String.Join("\n", levelColumn, "");
                                }
                                else
                                {
                                    levelColumn = levelColumn == "" ? childBlockMaintextureRateData.LevelBlockId.ToString() : String.Join("\n", levelColumn, childBlockMaintextureRateData.LevelBlockId.ToString());
                                }
                            }
                            else
                            {
                                levelColumn = levelColumn == "" ? childBlockMaintextureRateData.LevelBlockId.ToString() : String.Join("\n", levelColumn, childBlockMaintextureRateData.LevelBlockId.ToString());
                            }
                        }

                        //childBlockMaintextureRateDatas.ForEach(childBlockMaintextureRateData => {
                        //    levelColumn = levelColumn == "" ? childBlockMaintextureRateData.LevelBlockId.ToString() : String.Join("\n", levelColumn, childBlockMaintextureRateData.LevelBlockId.ToString());
                        //    currMainTextureNameColumn = currMainTextureNameColumn == "" ? childBlockMaintextureRateData.CurrentStateMainTextureName : String.Join("\n", currMainTextureNameColumn, childBlockMaintextureRateData.CurrentStateMainTextureName);
                        //    remainingRateColumn = remainingRateColumn == "" ? childBlockMaintextureRateData.RemainingRate.ToString() : String.Join("\n", remainingRateColumn, childBlockMaintextureRateData.RemainingRate.ToString());
                        //    mainRateColumn = mainRateColumn == "" ? childBlockMaintextureRateData.MainRate.ToString() : String.Join("\n", mainRateColumn, childBlockMaintextureRateData.MainRate.ToString());
                        //});
                    }

                    if (j == 0)
                    {
                        cell.PreferredWidth = 5;
                        cell.PreferredWidthType = WidthType.FiftiethsOfPercent;
                        if (i == 0)
                            document.InsertSingleLineText(cell.Range.Start, $"STT");
                        else if(i < 7)
                        {
                            document.InsertSingleLineText(cell.Range.Start, $"{i}");
                        }
                    }
                    else if (j == 1)
                    {
                        cell.PreferredWidth = 15;
                        cell.PreferredWidthType = WidthType.FiftiethsOfPercent;
                        if (i == 0)
                        {
                            document.InsertSingleLineText(cell.Range.Start, $"Phần nhà cấp");
                        }
                        else if (i < 7)
                        {
                            document.InsertText(cell.Range.Start, levelColumn);
                        }
                    }
                    else if (j == 2)
                    {
                        cell.PreferredWidth = 20;
                        cell.PreferredWidthType = WidthType.FiftiethsOfPercent;
                        if (i == 0)
                        {
                            document.InsertSingleLineText(cell.Range.Start, $"Kết cấu chính");
                        }
                        else if (i < 7)
                        {
                            document.InsertSingleLineText(cell.Range.Start, $"{blockMaintextureRateDataGroupByTypeMainTexTures[i - 1].blockMaintextureRateDatas[0].TypeMainTexTureName}");
                        }
                    }
                    else if(j == 3)
                    {
                        cell.PreferredWidth = 25;
                        cell.PreferredWidthType = WidthType.FiftiethsOfPercent;
                        if (i == 0)
                            document.InsertSingleLineText(cell.Range.Start, $"Hiện trạng");
                        else if(i < 7)
                            document.InsertText(cell.Range.Start, currMainTextureNameColumn);
                    }
                    else if (j == 4)
                    {
                        cell.PreferredWidth = 15;
                        cell.PreferredWidthType = WidthType.FiftiethsOfPercent;
                        if (i == 0)
                            document.InsertSingleLineText(cell.Range.Start, $"Tỷ lệ chất lượng còn lại %");
                        else if (i < 7)
                            document.InsertText(cell.Range.Start, remainingRateColumn);
                    }
                    else if (j == 5)
                    {
                        cell.PreferredWidth = 20;
                        cell.PreferredWidthType = WidthType.FiftiethsOfPercent;
                        if (i == 0)
                            document.InsertSingleLineText(cell.Range.Start, $"Tỷ lệ giá trị của kết cấu chính so với tổng giá trị của nhà ở %");
                        else if (i < 7)
                            document.InsertText(cell.Range.Start, mainRateColumn);
                    }

                    if(i == 7)
                    {
                        //if(j == 0)
                        //    cell.PreferredWidth = 60;
                        //else
                        //    cell.PreferredWidth = 40;

                        //cell.Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                        //cell.Borders.Top.LineStyle = TableBorderLineStyle.None;
                        //cell.Borders.Right.LineStyle = TableBorderLineStyle.None;
                        //cell.Borders.Left.LineStyle = TableBorderLineStyle.None;
                    }

                    if(i == 0)
                        cell.VerticalAlignment = TableCellVerticalAlignment.Center;
                    else
                        cell.VerticalAlignment = TableCellVerticalAlignment.Top;
                });

                table.BeginUpdate();
                table.MergeCells(table[7, 0], table[7, 2]);
                table[7, 0].PreferredWidth = 50;
                table[7, 0].PreferredWidthType = WidthType.FiftiethsOfPercent;
                table[7, 0].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                table[7, 0].Borders.Top.LineStyle = TableBorderLineStyle.None;
                table[7, 0].Borders.Right.LineStyle = TableBorderLineStyle.None;
                table[7, 0].Borders.Left.LineStyle = TableBorderLineStyle.None;
                document.InsertText(table[7, 0].Range.Start, "Tỷ lệ chất lượng còn lại của nhà này là");
                table.MergeCells(table[7, 1], table[7, 3]);
                table[7, 1].PreferredWidth = 50;
                table[7, 1].PreferredWidthType = WidthType.FiftiethsOfPercent;
                table[7, 1].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                table[7, 1].Borders.Top.LineStyle = TableBorderLineStyle.None;
                table[7, 1].Borders.Right.LineStyle = TableBorderLineStyle.None;
                table[7, 1].Borders.Left.LineStyle = TableBorderLineStyle.None;
                document.InsertText(table[7, 1].Range.Start, str);

                //table.MergeCells(table[6, 3], table[6, 5]);
                table.EndUpdate();

                table.TableAlignment = TableRowAlignment.Center;
            }
            else
            {
                //Xuống dòng
                var breakLine = document.InsertText(ranges[0].Start, "\n");
            }

            document.ReplaceAll("<maintextureRateTbla>", "", SearchOptions.None);
        }

        //Bảng chất lượng còn lại của căn nhà theo pp thống kê kinh nghiệm
        private void genBlockMaintextureRateTblb(Document document, string str)
        {
            DocumentRange[] ranges = document.FindAll("<maintextureRateTblb>", DevExpress.XtraRichEdit.API.Native.SearchOptions.None);
            Table table = document.Tables.Create(ranges[0].Start, 1, 2, AutoFitBehaviorType.FixedColumnWidth);
            document.ReplaceAll("<maintextureRateTblb>", "", SearchOptions.None);

            table.ForEachCell((cell, i, j) => {

                cell.PreferredWidthType = WidthType.FiftiethsOfPercent;
                cell.VerticalAlignment = TableCellVerticalAlignment.Top;
                cell.Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                cell.Borders.Top.LineStyle = TableBorderLineStyle.None;
                cell.Borders.Right.LineStyle = TableBorderLineStyle.None;
                cell.Borders.Left.LineStyle = TableBorderLineStyle.None;
                if (j == 0)
                {
                    cell.PreferredWidth = 60;
                    document.InsertText(cell.Range.Start, "Tỷ lệ chất lượng còn lại của nhà này là");
                }
                else 
                {
                    cell.PreferredWidth = 40;
                    document.InsertText(cell.Range.Start, str);
                }
            });
        }

        //Bảng chất lượng còn lại của căn nhà theo pp cuối cùng
        private void genBlockMaintextureRateTblc(Document document, string str)
        {
            DocumentRange[] ranges = document.FindAll("<maintextureRateTblc>", DevExpress.XtraRichEdit.API.Native.SearchOptions.None);
            Table table = document.Tables.Create(ranges[0].Start, 1, 1, AutoFitBehaviorType.FixedColumnWidth);
            document.ReplaceAll("<maintextureRateTblc>", "", SearchOptions.None);

            table.ForEachCell((cell, i, j) => {

                cell.PreferredWidthType = WidthType.FiftiethsOfPercent;
                cell.VerticalAlignment = TableCellVerticalAlignment.Center;
                cell.Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                cell.Borders.Top.LineStyle = TableBorderLineStyle.None;
                cell.Borders.Right.LineStyle = TableBorderLineStyle.None;
                cell.Borders.Left.LineStyle = TableBorderLineStyle.None;
                document.InsertText(cell.Range.Start, str);
            });
        }

        //gen Bảng Chỉ số giá xây dựng công trình
        private DocumentRange genConstructionPriceTbl(Document document, List<ConstructionPrice> constructionPrices, DocumentPosition documentPosition)
        {
            int count = constructionPrices.Count;
            if (count > 0)
            {
                Table table = document.Tables.Create(documentPosition, count, 2, AutoFitBehaviorType.FixedColumnWidth);
                table.ForEachCell((cell, i, j) =>
                {

                    cell.PreferredWidthType = WidthType.FiftiethsOfPercent;
                    cell.Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                    cell.Borders.Top.LineStyle = TableBorderLineStyle.None;
                    cell.Borders.Right.LineStyle = TableBorderLineStyle.None;
                    cell.Borders.Left.LineStyle = TableBorderLineStyle.None;

                    if (j == 0)
                    {
                        ParagraphProperties pp = document.BeginUpdateParagraphs(cell.Range);
                        pp.Alignment = ParagraphAlignment.Center;
                        document.EndUpdateParagraphs(pp);
                        document.InsertText(cell.Range.Start, $"Năm {constructionPrices[i].Year} = {constructionPrices[i].Value.ToString("N2", culture)} %");
                    }
                    else
                    {
                        document.InsertText(cell.Range.Start, $"{constructionPrices[i].Des}");
                    }
                });

                return table.Range;
            }
            else {
                Table table = document.Tables.Create(documentPosition, 1, 2, AutoFitBehaviorType.FixedColumnWidth);
                table.ForEachCell((cell, i, j) => {
                    cell.PreferredWidthType = WidthType.FiftiethsOfPercent;
                    cell.Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                    cell.Borders.Top.LineStyle = TableBorderLineStyle.None;
                    cell.Borders.Right.LineStyle = TableBorderLineStyle.None;
                    cell.Borders.Left.LineStyle = TableBorderLineStyle.None;
                });
                return table.Range;
            }
        }

        //Tính giá trị còn lại của nhà ở
        private void genPricingLand(Document document, DateTime? dateCreate, List<PricingLandTblData> pricingLandTblDatas, List<ConstructionPrice> constructionPrices, float? vat, decimal? price, TypeReportApply typeReportApply, int? AreaCorrectionCoefficientId, TypeReportApply? parentTypeReportApply)
        {
            DocumentRange[] ranges = document.FindAll("<pricingLand>", DevExpress.XtraRichEdit.API.Native.SearchOptions.None);
            document.ReplaceAll("<pricingLand>", "", SearchOptions.None);
            DocumentRange lastRange = ranges[0];

            string listPrice = "";

            //Kiểm tra nếu bb là cho Nhà chung cư hoặc bán phần diện tích sử dụng chung của Nhà chung cư thì tách vùng Suất vốn đầu tư
            if (typeReportApply == TypeReportApply.NHA_CHUNG_CU || (typeReportApply == TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG && parentTypeReportApply == TypeReportApply.NHA_CHUNG_CU))
            {
                //group pricingLandTblDatas theo Suất vốn đầu tư
                var groupPricingLandTblDatasByApplyInvestmentRates = pricingLandTblDatas.GroupBy(x => x.ApplyInvestmentRate).OrderByDescending(x => x.Key).ToList();
                groupPricingLandTblDatasByApplyInvestmentRates.ForEach(groupPricingLandTblDatasByApplyInvestmentRate => {

                    string strApplyInvestmentRate = groupPricingLandTblDatasByApplyInvestmentRate.Key == true ? "Trường hợp các phần diện tích đất áp dụng Suất vốn đầu tư\n" : "Trường hợp các phần diện tích đất không áp dụng Suất vốn đầu tư\n";
                    lastRange = document.InsertText(lastRange.End, strApplyInvestmentRate);
                    CharacterProperties cp0 = document.BeginUpdateCharacters(lastRange);
                    cp0.Bold = true;
                    cp0.FontSize = 14;
                    cp0.HighlightColor = Color.LightYellow;
                    document.EndUpdateCharacters(cp0);

                    //group pricingLandTblDatas by level
                    var groupPricingLandTblDatasBylevel = groupPricingLandTblDatasByApplyInvestmentRate.ToList().GroupBy(x => x.Level).OrderBy(x => x.Key).ToList();
                    groupPricingLandTblDatasBylevel.ForEach(groupPricingLandTblDatasBylevelItem => {

                        lastRange = document.InsertText(lastRange.End, $"Phần nhà cấp {groupPricingLandTblDatasBylevelItem.Key}:\n");
                        CharacterProperties cp = document.BeginUpdateCharacters(lastRange);
                        cp.Bold = true;
                        cp.FontSize = 14;
                        document.EndUpdateCharacters(cp);

                        var childPricingLandTblDatas = groupPricingLandTblDatasBylevelItem.ToList();
                        childPricingLandTblDatas.ForEach(childPricingLandTblData => {

                            listPrice = listPrice == "" ? $"{UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice)}" : $"{listPrice} + {UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice)}";

                            //Từng tầng
                            string areaTitle = $"{childPricingLandTblData.AreaName} Áp dụng Theo {getTermApplyName(childPricingLandTblData.TermApply)} ({getDecreeName(childPricingLandTblData.DecreeType1Id)})";
                            lastRange = document.InsertText(lastRange.End, areaTitle);
                            CharacterProperties cp1 = document.BeginUpdateCharacters(lastRange);
                            cp1.Bold = true;
                            cp1.FontSize = 14;
                            cp1.Underline = UnderlineType.Single;
                            document.EndUpdateCharacters(cp1);

                            //Check có áp dụng suất vốn đầu tư hay là không
                            if (childPricingLandTblData.ApplyInvestmentRate == true)
                            {
                                //string extraAreaTitle = "(Áp dụng suất vốn đầu tư)\n";
                                //lastRange = document.InsertText(lastRange.End, extraAreaTitle);
                                //CharacterProperties cp1_1 = document.BeginUpdateCharacters(lastRange);
                                //cp1_1.Bold = true;
                                //cp1_1.FontSize = 14;
                                //cp1_1.Underline = UnderlineType.Single;
                                //cp1_1.BackColor = Color.LightYellow;
                                //document.EndUpdateCharacters(cp1_1);

                                //Thông tin suất vốn đầu tư
                                string investmentRateStr = "";
                                InvestmentRateItem investmentRateItem = _context.InvestmentRateItems.Where(i => i.Id == childPricingLandTblData.InvestmentRateItemId).FirstOrDefault();

                                if (investmentRateItem != null)
                                {
                                    InvestmentRate investmentRate = _context.InvestmentRaties.Where(i => i.Id == investmentRateItem.Id).FirstOrDefault();

                                    if (investmentRate != null)
                                        //investmentRateStr = $"{investmentRateItem.DetailInfo}, {investmentRateItem.LineInfo}, {investmentRate.Des}. Suất vốn đầu tư: {UtilsService.FormatMoney((decimal)investmentRateItem.Value)}, Chi phí xây dựng: {UtilsService.FormatMoney((decimal)investmentRateItem.Value1)}, Chi phí thiết bị: {UtilsService.FormatMoney((decimal)investmentRateItem.Value2)} ";
                                        investmentRateStr = $"\nCăn cứ dòng số {investmentRateItem.LineInfo} chi tiết suất vốn đầu tư {investmentRateItem.DetailInfo}. Suất vốn đầu tư: {UtilsService.FormatMoney((decimal)investmentRateItem.Value)} đồng/m2, Chi phí xây dựng: {UtilsService.FormatMoney((decimal)investmentRateItem.Value1)} đồng/m2, Chi phí thiết bị: {UtilsService.FormatMoney((decimal)investmentRateItem.Value2)} đồng/m2";
                                }

                                investmentRateStr += "\n";
                                lastRange = document.InsertText(lastRange.End, investmentRateStr);
                                CharacterProperties cp1_2 = document.BeginUpdateCharacters(lastRange);
                                cp1_2.Bold = false;
                                cp1_2.FontSize = 14;
                                cp1_2.Underline = UnderlineType.None;
                                document.EndUpdateCharacters(cp1_2);

                                //Vùng quy định
                                string specifiedArea1 = "* Vùng quy định: ";
                                lastRange = document.InsertText(lastRange.End, specifiedArea1);
                                CharacterProperties cp1_3 = document.BeginUpdateCharacters(lastRange);
                                cp1_3.Bold = true;
                                cp1_3.FontSize = 14;
                                document.EndUpdateCharacters(cp1_3);

                                string specifiedArea2 = "Căn cứ điểm 1.1, khoản 1, Điều 1, Phần 1 Suất vốn đầu tư xây dựng và giá xây dựng tổng hợp bộ phận kết cấu công trình năm 2022 \n Vùng 8: Thành phố Hồ Chí Minh\n";
                                lastRange = document.InsertText(lastRange.End, specifiedArea2);
                                CharacterProperties cp1_4 = document.BeginUpdateCharacters(lastRange);
                                cp1_4.Bold = false;
                                cp1_4.FontSize = 14;
                                document.EndUpdateCharacters(cp1_4);

                                //Hệ số điều chỉnh vùng cho Suất vốn đầu tư
                                lastRange = document.InsertText(lastRange.End, "* Hệ số điều chỉnh vùng cho Suất vốn đầu tư xây dựng công trình: ");
                                CharacterProperties cp1_5 = document.BeginUpdateCharacters(lastRange);
                                cp1_5.Bold = true;
                                cp1_5.FontSize = 14;
                                document.EndUpdateCharacters(cp1_5);

                                AreaCorrectionCoefficient areaCorrectionCoefficient = _context.AreaCorrectionCoefficients.Where(a => a.Id == AreaCorrectionCoefficientId).FirstOrDefault();
                                if (areaCorrectionCoefficient != null)
                                {
                                    string areaCorrectionCoefficientStr = $"{areaCorrectionCoefficient.Note}\nHệ số điều chỉnh vùng: {areaCorrectionCoefficient.Value.ToString("N3", culture)}";

                                    lastRange = document.InsertText(lastRange.End, areaCorrectionCoefficientStr);
                                    CharacterProperties cp1_6 = document.BeginUpdateCharacters(lastRange);
                                    cp1_6.Bold = false;
                                    cp1_6.FontSize = 14;
                                    document.EndUpdateCharacters(cp1_6);
                                }

                                lastRange = document.InsertText(lastRange.End, "\n");
                                CharacterProperties cp1_7 = document.BeginUpdateCharacters(lastRange);
                                cp1_7.Bold = false;
                                cp1_7.FontSize = 14;
                                document.EndUpdateCharacters(cp1_7);
                            }
                            else
                            {
                                string extraAreaTitle = "\n";
                                lastRange = document.InsertText(lastRange.End, extraAreaTitle);

                                // Đơn giá
                                lastRange = document.InsertText(lastRange.End, "\t* Đơn giá: ");
                                CharacterProperties cp2 = document.BeginUpdateCharacters(lastRange);
                                cp2.Bold = true;
                                cp2.FontSize = 14;
                                document.EndUpdateCharacters(cp2);

                                string notePrice = $" {UtilsService.FormatMoney(childPricingLandTblData.Price)} đồng/m2 ({ childPricingLandTblData.PriceListItemNote }).\n";
                                lastRange = document.InsertText(lastRange.End, notePrice);
                                CharacterProperties cp3 = document.BeginUpdateCharacters(lastRange);
                                cp3.FontSize = 14;
                                cp3.Bold = false;
                                document.EndUpdateCharacters(cp3);
                            }

                            //Chỉ số giá xây dựng công trình chỉ áp dụng vs ND 99

                            if (childPricingLandTblData.DecreeType1Id == DecreeEnum.ND_CP_99_2015)
                            {
                                lastRange = document.InsertText(lastRange.End, "\t* Chỉ số giá xây dựng công trình: ");
                                CharacterProperties cp4 = document.BeginUpdateCharacters(lastRange);
                                cp4.Bold = true;
                                cp4.FontSize = 14;
                                document.EndUpdateCharacters(cp4);

                                lastRange = genConstructionPriceTbl(document, constructionPrices, lastRange.End);

                                //Giá nhà ở năm 
                                int year = dateCreate.Value.Year;
                                lastRange = document.InsertText(lastRange.End, $"- Giá nhà ở năm {year}: \n");
                                CharacterProperties cp5 = document.BeginUpdateCharacters(lastRange);
                                cp5.Bold = true;
                                cp5.FontSize = 14;
                                document.EndUpdateCharacters(cp5);

                                string housePriceStr = $"\t {UtilsService.FormatMoney(childPricingLandTblData.Price)} đồng/m2 x {getConstructionPriceStr(constructionPrices)} = {UtilsService.FormatMoney(childPricingLandTblData.PriceInYear)} đồng/m2\n";

                                lastRange = document.InsertText(lastRange.End, housePriceStr);
                                CharacterProperties cp6 = document.BeginUpdateCharacters(lastRange);
                                cp6.FontSize = 14;
                                cp6.Bold = false;
                                document.EndUpdateCharacters(cp6);
                            }

                            //Tỷ lệ chất lượng còn lại
                            string maintextureRateValue = childPricingLandTblData.MaintextureRateValue != null ? ((float)childPricingLandTblData.MaintextureRateValue).ToString("N2", culture) : "";
                            lastRange = document.InsertText(lastRange.End, $"- Tỷ lệ chất lượng còn lại là: { maintextureRateValue } %\n");
                            CharacterProperties cp7 = document.BeginUpdateCharacters(lastRange);
                            cp7.FontSize = 14;
                            cp7.Bold = false;
                            document.EndUpdateCharacters(cp7);

                            //Giá trị còn lại của nhà ở
                            lastRange = document.InsertText(lastRange.End, $"- Giá trị còn lại của nhà ở:\n");
                            CharacterProperties cp8 = document.BeginUpdateCharacters(lastRange);
                            cp8.FontSize = 14;
                            cp8.Bold = true;
                            document.EndUpdateCharacters(cp8);

                            string remainingPriceStr = "";
                            float totalArea = (childPricingLandTblData.GeneralArea ?? 0) + (childPricingLandTblData.PrivateArea ?? 0);
                            string coefficientUseValue = childPricingLandTblData.CoefficientUseValue != null ? ((float)childPricingLandTblData.CoefficientUseValue).ToString("N2", culture) : "";

                            if (childPricingLandTblData.TermApply == TermApply.DIEU_65 || childPricingLandTblData.TermApply == TermApply.KHOAN_1_DIEU_34 || childPricingLandTblData.TermApply == TermApply.DIEU_7)
                            {
                                remainingPriceStr = $"\t{UtilsService.FormatMoney(childPricingLandTblData.PriceInYear)} đồng/m2 x {maintextureRateValue}% x {totalArea.ToString("N2", culture)} m2 x {coefficientUseValue} = {UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice)}\n";
                            }
                            else if (childPricingLandTblData.TermApply == TermApply.DIEU_70 || childPricingLandTblData.TermApply == TermApply.KHOAN_2_DIEU_34 || childPricingLandTblData.TermApply == TermApply.DIEU_35 || childPricingLandTblData.TermApply == TermApply.DIEU_71)
                            {
                                remainingPriceStr = $"\t{UtilsService.FormatMoney(childPricingLandTblData.PriceInYear)} đồng/m2 x {maintextureRateValue}% x {totalArea.ToString("N2", culture)} m2 = {UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice)}\n";
                            }

                            lastRange = document.InsertText(lastRange.End, remainingPriceStr);
                            CharacterProperties cp9 = document.BeginUpdateCharacters(lastRange);
                            cp9.FontSize = 14;
                            cp9.Bold = false;
                            document.EndUpdateCharacters(cp9);
                        });
                    });
                });
            }
            else
            {
                //group pricingLandTblDatas by level
                var groupPricingLandTblDatasBylevel = pricingLandTblDatas.GroupBy(x => x.Level).OrderBy(x => x.Key).ToList();
                groupPricingLandTblDatasBylevel.ForEach(groupPricingLandTblDatasBylevelItem => {

                    lastRange = document.InsertText(lastRange.End, $"Phần nhà cấp {groupPricingLandTblDatasBylevelItem.Key}:\n");
                    CharacterProperties cp = document.BeginUpdateCharacters(lastRange);
                    cp.Bold = true;
                    cp.FontSize = 14;
                    document.EndUpdateCharacters(cp);

                    var childPricingLandTblDatas = groupPricingLandTblDatasBylevelItem.ToList();
                    childPricingLandTblDatas.ForEach(childPricingLandTblData => {

                        listPrice = listPrice == "" ? $"{UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice)}" : $"{listPrice} + {UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice)}";

                        //Từng tầng
                        string areaTitle = $"{childPricingLandTblData.AreaName} Áp dụng Theo {getTermApplyName(childPricingLandTblData.TermApply)} ({getDecreeName(childPricingLandTblData.DecreeType1Id)})";
                        lastRange = document.InsertText(lastRange.End, areaTitle);
                        CharacterProperties cp1 = document.BeginUpdateCharacters(lastRange);
                        cp1.Bold = true;
                        cp1.FontSize = 14;
                        cp1.Underline = UnderlineType.Single;
                        document.EndUpdateCharacters(cp1);

                        string extraAreaTitle = "\n";
                        lastRange = document.InsertText(lastRange.End, extraAreaTitle);

                        // Đơn giá
                        lastRange = document.InsertText(lastRange.End, "\t* Đơn giá: ");
                        CharacterProperties cp2 = document.BeginUpdateCharacters(lastRange);
                        cp2.Bold = true;
                        cp2.FontSize = 14;
                        document.EndUpdateCharacters(cp2);

                        string notePrice = $" {UtilsService.FormatMoney(childPricingLandTblData.Price)} đồng/m2 ({ childPricingLandTblData.PriceListItemNote }).\n";
                        lastRange = document.InsertText(lastRange.End, notePrice);
                        CharacterProperties cp3 = document.BeginUpdateCharacters(lastRange);
                        cp3.FontSize = 14;
                        cp3.Bold = false;
                        document.EndUpdateCharacters(cp3);

                        //Chỉ số giá xây dựng công trình chỉ áp dụng vs ND 99

                        if (childPricingLandTblData.DecreeType1Id == DecreeEnum.ND_CP_99_2015)
                        {
                            lastRange = document.InsertText(lastRange.End, "\t* Chỉ số giá xây dựng công trình: ");
                            CharacterProperties cp4 = document.BeginUpdateCharacters(lastRange);
                            cp4.Bold = true;
                            cp4.FontSize = 14;
                            document.EndUpdateCharacters(cp4);

                            lastRange = genConstructionPriceTbl(document, constructionPrices, lastRange.End);

                            //Giá nhà ở năm 
                            int year = dateCreate.Value.Year;
                            lastRange = document.InsertText(lastRange.End, $"- Giá nhà ở năm {year}: \n");
                            CharacterProperties cp5 = document.BeginUpdateCharacters(lastRange);
                            cp5.Bold = true;
                            cp5.FontSize = 14;
                            document.EndUpdateCharacters(cp5);

                            string housePriceStr = $"\t {UtilsService.FormatMoney(childPricingLandTblData.Price)} đồng/m2 x {getConstructionPriceStr(constructionPrices)} = {UtilsService.FormatMoney(childPricingLandTblData.PriceInYear)} đồng/m2\n";

                            lastRange = document.InsertText(lastRange.End, housePriceStr);
                            CharacterProperties cp6 = document.BeginUpdateCharacters(lastRange);
                            cp6.FontSize = 14;
                            cp6.Bold = false;
                            document.EndUpdateCharacters(cp6);
                        }

                        //Tỷ lệ chất lượng còn lại
                        string maintextureRateValue = childPricingLandTblData.MaintextureRateValue != null ? ((float)childPricingLandTblData.MaintextureRateValue).ToString("N2", culture) : "";

                        lastRange = document.InsertText(lastRange.End, $"- Tỷ lệ chất lượng còn lại là: { maintextureRateValue } %\n");
                        CharacterProperties cp7 = document.BeginUpdateCharacters(lastRange);
                        cp7.FontSize = 14;
                        cp7.Bold = false;
                        document.EndUpdateCharacters(cp7);

                        //Giá trị còn lại của nhà ở
                        lastRange = document.InsertText(lastRange.End, $"- Giá trị còn lại của nhà ở:\n");
                        CharacterProperties cp8 = document.BeginUpdateCharacters(lastRange);
                        cp8.FontSize = 14;
                        cp8.Bold = true;
                        document.EndUpdateCharacters(cp8);

                        string remainingPriceStr = "";
                        float totalArea = (childPricingLandTblData.GeneralArea ?? 0) + (childPricingLandTblData.PrivateArea ?? 0);
                        string coefficientUseValue = childPricingLandTblData.CoefficientUseValue != null ? ((float)childPricingLandTblData.CoefficientUseValue).ToString("N2", culture) : "";

                        if (childPricingLandTblData.TermApply == TermApply.DIEU_65 || childPricingLandTblData.TermApply == TermApply.KHOAN_1_DIEU_34 || childPricingLandTblData.TermApply == TermApply.DIEU_7)
                        {
                            remainingPriceStr = $"\t{UtilsService.FormatMoney(childPricingLandTblData.PriceInYear)} đồng/m2 x {maintextureRateValue}% x {totalArea.ToString("N2", culture)} m2 x {coefficientUseValue} = {UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice)}\n";
                        }
                        else if (childPricingLandTblData.TermApply == TermApply.DIEU_70 || childPricingLandTblData.TermApply == TermApply.KHOAN_2_DIEU_34 || childPricingLandTblData.TermApply == TermApply.DIEU_35 || childPricingLandTblData.TermApply == TermApply.DIEU_71)
                        {
                            remainingPriceStr = $"\t{UtilsService.FormatMoney(childPricingLandTblData.PriceInYear)} đồng/m2 x {maintextureRateValue}% x {totalArea.ToString("N2", culture)} m2 = {UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice)}\n";
                        }

                        lastRange = document.InsertText(lastRange.End, remainingPriceStr);
                        CharacterProperties cp9 = document.BeginUpdateCharacters(lastRange);
                        cp9.FontSize = 14;
                        cp9.Bold = false;
                        document.EndUpdateCharacters(cp9);
                    });
                });
            }

            lastRange = document.InsertText(lastRange.End, $"* Giá nhà ở của căn nhà");
            CharacterProperties cp10 = document.BeginUpdateCharacters(lastRange);
            cp10.FontSize = 14;
            cp10.Bold = true;
            document.EndUpdateCharacters(cp10);

            lastRange = document.InsertText(lastRange.End, $" (gồm {(vat != null ? ((float)vat).ToString("N2", culture) : "0")}% VAT)");
            CharacterProperties cp11 = document.BeginUpdateCharacters(lastRange);
            cp11.FontSize = 14;
            cp11.Bold = false;
            cp11.Italic = true;
            document.EndUpdateCharacters(cp11);

            lastRange = document.InsertText(lastRange.End, $" = (Gn) = {listPrice} = ");
            CharacterProperties cp12 = document.BeginUpdateCharacters(lastRange);
            cp12.FontSize = 14;
            cp12.Bold = false;
            document.EndUpdateCharacters(cp12);

            lastRange = document.InsertText(lastRange.End, $"{UtilsService.FormatMoney(price)} đồng\n");
            CharacterProperties cp13 = document.BeginUpdateCharacters(lastRange);
            cp13.FontSize = 14;
            cp13.Bold = true;
            document.EndUpdateCharacters(cp13);

            lastRange = document.InsertText(lastRange.End, $"(Ghi bằng chữ: {UtilsService.ConvertMoneyToString(price.ToString()).ToLower()})\n");
            CharacterProperties cp14 = document.BeginUpdateCharacters(lastRange);
            cp14.FontSize = 14;
            cp14.Bold = false;
            cp14.Italic = true;
            document.EndUpdateCharacters(cp14);
        }

        //Tiền mua nhà được giảm
        private void genReducedPersonTbl(Document document, List<PricingReducedPersonData> pricingReducedPeople)
        {
            int count = pricingReducedPeople.Count;
            DocumentRange[] ranges = document.FindAll("<reducedPersonTbl>", DevExpress.XtraRichEdit.API.Native.SearchOptions.None);
            document.ReplaceAll("<reducedPersonTbl>", "", SearchOptions.None);
            DocumentRange range = ranges[0];

            Table table = document.Tables.Create(range.End, count + 2, 6, AutoFitBehaviorType.FixedColumnWidth);
            table.ForEachCell((cell, i, j) => {

                cell.PreferredWidthType = WidthType.FiftiethsOfPercent;
                ParagraphProperties pp = document.BeginUpdateParagraphs(cell.Range);
                pp.Alignment = ParagraphAlignment.Center;
                document.EndUpdateParagraphs(pp);

                if (j == 0)
                {
                    cell.PreferredWidth = 10;
                    if (i == 0) 
                        document.InsertText(cell.Range.Start, $"STT");
                    else if(i == 1)
                        document.InsertText(cell.Range.Start, $"(1)");
                    else
                        document.InsertText(cell.Range.Start, $"{i - 1}");
                }
                else if(j == 1)
                {
                    cell.PreferredWidth = 30;
                    if (i == 0)
                        document.InsertText(cell.Range.Start, $"Tên người được tinh giảm");
                    else if (i == 1)
                        document.InsertText(cell.Range.Start, $"(2)");
                    else
                        document.InsertText(cell.Range.Start, $"{pricingReducedPeople[i - 2].CustomerName}");
                }
                else if (j == 2)
                {
                    cell.PreferredWidth = 15;
                    if (i == 0)
                        document.InsertText(cell.Range.Start, $"Thời gian công tác dân sự (năm)");
                    else if (i == 1)
                        document.InsertText(cell.Range.Start, $"(3)");
                    else
                        document.InsertText(cell.Range.Start, $"{pricingReducedPeople[i - 2].Year}");
                }
                else if (j == 3)
                {
                    cell.PreferredWidth = 15;
                    if (i == 0)
                        document.InsertText(cell.Range.Start, $"Mức lương cơ bản");
                    else if (i == 1)
                        document.InsertText(cell.Range.Start, $"(4)");
                    else
                        document.InsertText(cell.Range.Start, $"{UtilsService.FormatMoney(pricingReducedPeople[i - 2].Salary)}");
                }
                else if (j == 4)
                {
                    cell.PreferredWidth = 10;
                    if (i == 0)
                        document.InsertText(cell.Range.Start, $"Hệ số được giảm");
                    else if (i == 1)
                        document.InsertText(cell.Range.Start, $"(5)");
                    else
                    {
                        string deductionCoefficient = pricingReducedPeople[i - 2].DeductionCoefficient != null ? ((float)pricingReducedPeople[i - 2].DeductionCoefficient).ToString("N2", culture) : "";
                        document.InsertText(cell.Range.Start, $"{deductionCoefficient}");
                    }
                }
                else if (j == 5)
                {
                    cell.PreferredWidth = 20;
                    if (i == 0)
                        document.InsertText(cell.Range.Start, $"Tiền mua nhà được giảm");
                    else if (i == 1)
                        document.InsertText(cell.Range.Start, $"(6)=(3)*(4)*(5)");
                    else
                        document.InsertText(cell.Range.Start, $"{UtilsService.FormatMoney((decimal)pricingReducedPeople[i - 2].Value)}");
                }
            });
        }

        //Chi tiết diện tích đất của căn hộ
        private void genApartmentLandDetailTbl(Document document, List<PricingApartmentLandDetail> pricingApartmentLandDetails, TypeReportApply typeReportApply) {
            int count = pricingApartmentLandDetails.Count;
            DocumentRange[] ranges = document.FindAll("<apartmentLandDetailTbl>", DevExpress.XtraRichEdit.API.Native.SearchOptions.None);
            document.ReplaceAll("<apartmentLandDetailTbl>", "", SearchOptions.None);
            DocumentRange range = ranges[0];

            int row_num = typeReportApply == TypeReportApply.NHA_HO_CHUNG || typeReportApply == TypeReportApply.NHA_CHUNG_CU ? 4 : 3;

            Table table = document.Tables.Create(range.End, count + 1, row_num, AutoFitBehaviorType.FixedColumnWidth);
            table.ForEachCell((cell, i, j) => {

                cell.VerticalAlignment = TableCellVerticalAlignment.Center;
                cell.PreferredWidthType = WidthType.FiftiethsOfPercent;
                ParagraphProperties pp = document.BeginUpdateParagraphs(cell.Range);
                pp.Alignment = ParagraphAlignment.Center;
                document.EndUpdateParagraphs(pp);

                if (j == 0)
                {
                    cell.PreferredWidth = 30;

                    if (i == 0)
                    {
                        cell.BackgroundColor = Color.LightBlue;
                        document.InsertText(cell.Range.Start, $"Nghị định");
                    }
                    else
                        document.InsertText(cell.Range.Start, $"{getDecreeName(pricingApartmentLandDetails[i-1].DecreeType1Id)}");
                }
                else if (j == 1)
                {
                    cell.PreferredWidth = 30;

                    if (i == 0)
                    {
                        cell.BackgroundColor = Color.LightBlue;
                        document.InsertText(cell.Range.Start, $"Điều");
                    }
                    else
                        document.InsertText(cell.Range.Start, $"{getTermApplyName(pricingApartmentLandDetails[i - 1].TermApply)}");
                }
                else if (j == 2)
                {
                    cell.PreferredWidth = 20;

                    if(typeReportApply == TypeReportApply.NHA_HO_CHUNG || typeReportApply == TypeReportApply.NHA_RIENG_LE || typeReportApply == TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG)
                    {
                        if (i == 0)
                        {
                            cell.BackgroundColor = Color.LightBlue;
                            document.InsertText(cell.Range.Start, $"DT đất sử dụng riêng");
                        }
                        else
                        {
                            string privateArea = pricingApartmentLandDetails[i - 1].PrivateArea != null ? ((float)pricingApartmentLandDetails[i - 1].PrivateArea).ToString("N2", culture) : "";
                            document.InsertText(cell.Range.Start, $"{privateArea}");
                        }
                    }
                    else
                    {
                        if (i == 0)
                        {
                            cell.BackgroundColor = Color.LightBlue;
                            document.InsertText(cell.Range.Start, $"Hệ số phân bổ các tầng");
                        }
                        else
                        {
                            string coefficientDistribution = pricingApartmentLandDetails[i - 1].CoefficientDistribution != null ? ((float)pricingApartmentLandDetails[i - 1].CoefficientDistribution).ToString("N2", culture) : "";
                            document.InsertText(cell.Range.Start, $"{coefficientDistribution}");
                        }
                    }
                }
                else if (j == 3)
                {
                    cell.PreferredWidth = 20;

                    if(typeReportApply == TypeReportApply.NHA_HO_CHUNG)
                    {
                        if (i == 0)
                        {
                            cell.BackgroundColor = Color.LightBlue;
                            document.InsertText(cell.Range.Start, $"DT đất chung (Sân chung phân bổ)");
                        }
                        else
                        {
                            string generalArea = pricingApartmentLandDetails[i - 1].GeneralArea != null ? ((float)pricingApartmentLandDetails[i - 1].GeneralArea).ToString("N2", culture) : "";
                            document.InsertText(cell.Range.Start, $"{generalArea}");
                        }
                    }
                    else
                    {
                        if (i == 0)
                        {
                            cell.BackgroundColor = Color.LightBlue;
                            document.InsertText(cell.Range.Start, $"DT (m2)");
                        }
                        else
                        {
                            string privateArea = pricingApartmentLandDetails[i - 1].PrivateArea != null ? ((float)pricingApartmentLandDetails[i - 1].PrivateArea).ToString("N2", culture) : "";
                            document.InsertText(cell.Range.Start, $"{privateArea}");
                        }
                    }
                }
            });
        }

        //Bảng diện tích đất ở chung qui đổi
        private DocumentRange genConversionAreaTbl(Document document, DocumentPosition documentPosition, PricingApartmentLandDetail pricingApartmentLandDetail, List<PricingLandTbl> pricingLandTbls, List<BlockDetail> blockDetails, float? landscapeAreaValue1)
        {
            Table table = document.Tables.Create(documentPosition, 1, 3, AutoFitBehaviorType.AutoFitToContents);
            table.ForEachCell((cell, i, j) => {
                cell.Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                cell.Borders.Top.LineStyle = TableBorderLineStyle.None;
                cell.Borders.Right.LineStyle = TableBorderLineStyle.None;
                cell.Borders.Left.LineStyle = TableBorderLineStyle.None;
                cell.VerticalAlignment = TableCellVerticalAlignment.Center;

                ParagraphProperties pp = document.BeginUpdateParagraphs(cell.Range);
                pp.Alignment = ParagraphAlignment.Center;
                document.EndUpdateParagraphs(pp);

                if(j == 0)
                {
                    Table tableCell = document.Tables.Create(cell.Range.Start, 2, 1, AutoFitBehaviorType.AutoFitToContents);
                    tableCell.ForEachCell((cellChild, ic, jc) =>
                    {
                        cellChild.Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                        cellChild.Borders.Top.LineStyle = TableBorderLineStyle.None;
                        cellChild.Borders.Right.LineStyle = TableBorderLineStyle.None;
                        cellChild.Borders.Left.LineStyle = TableBorderLineStyle.None;

                        ParagraphProperties ppC = document.BeginUpdateParagraphs(cellChild.Range);
                        ppC.Alignment = ParagraphAlignment.Center;
                        document.EndUpdateParagraphs(ppC);

                        if (ic == 0)
                        {
                            cellChild.Borders.Bottom.LineStyle = TableBorderLineStyle.Single;
                            var lItPricingLandTbls = pricingLandTbls.Where(p => p.DecreeType1Id == pricingApartmentLandDetail.DecreeType1Id && p.TermApply == pricingApartmentLandDetail.TermApply).ToList();
                            var lItPricingLandTblsGroupByFloor = lItPricingLandTbls.GroupBy(x => x.FloorId).ToList();
                            List<string> numerator = new List<string>();
                            lItPricingLandTblsGroupByFloor.ForEach(pricingLandTblsGroupByFloor =>
                            {
                                int count = pricingLandTblsGroupByFloor.ToList().Count();
                                string childNumerator = count > 1 ? "(" : "";
                                List<string> str = pricingLandTblsGroupByFloor.Select(x => x.GeneralArea != null ? ((float)x.GeneralArea).ToString("N2", culture) : "").ToList();
                                string joinStr = string.Join(" + ", str);
                                string coefficientDistribution = pricingLandTblsGroupByFloor.ToList()[0].CoefficientDistribution != null ? ((float)pricingLandTblsGroupByFloor.ToList()[0].CoefficientDistribution).ToString("N2", culture) : "";
                                childNumerator = $"{childNumerator}{joinStr}" + (count > 1 ? ")" : "") + $" x {coefficientDistribution}";
                                numerator.Add(childNumerator);
                            });
                            document.InsertText(cellChild.Range.Start, $"({string.Join(" + ", numerator)})");
                        }
                        else
                        {
                            var lItBlockDetails = blockDetails.GroupBy(x => x.FloorId).ToList();
                            List<string> denominator = new List<string>();
                            lItBlockDetails.ForEach(lItBlockDetail =>
                            {
                                int count = lItBlockDetail.ToList().Count();
                                string childDenominator = count > 1 ? "(" : "";
                                string join = string.Join(" + ", lItBlockDetail.Select(x => x.GeneralArea != null ? ((float)x.GeneralArea).ToString("N2", culture) : ""));
                                var coefficient = pricingApartmentLandDetail.DecreeType1Id == DecreeEnum.ND_CP_99_2015 ? lItBlockDetail.ToList()[0].Coefficient_99 : (pricingApartmentLandDetail.DecreeType1Id == DecreeEnum.ND_CP_34_2013 ? lItBlockDetail.ToList()[0].Coefficient_34 : lItBlockDetail.ToList()[0].Coefficient_61);
                                childDenominator = $"{childDenominator}{join}" + (count > 1 ? ")" : "") + $" x {(coefficient != null ? ((float)coefficient).ToString("N2", culture) : "" )}";
                                denominator.Add(childDenominator);
                            });
                            document.InsertText(cellChild.Range.Start, $"({string.Join(" + ", denominator)})");
                        }
                    });
                }
                else if(j == 1)
                {
                    document.InsertText(cell.Range.Start, $" x {(landscapeAreaValue1 != null ? ((float)landscapeAreaValue1).ToString("N2", culture) : "")}");
                }
                else if(j == 2)
                {
                    document.InsertText(cell.Range.Start, $" = {(pricingApartmentLandDetail.ConversionArea != null ? ((float)pricingApartmentLandDetail.ConversionArea).ToString("N2", culture) : "")} m2");
                }
            });

            table.TableAlignment = TableRowAlignment.Center;
            return table.Range;
        }

        //Tính giá đất cụ thể: S đất chung qui đổi, giá đất sau khi đc miễn giảm...
        private void genApartmentLandDetailInfo(Document document, List<PricingApartmentLandDetailGroupByDecree> pricingApartmentLandDetailGroupByDecrees, Block block, Pricing pricing, List<PricingLandTbl> pricingLandTbls, List<BlockDetail> blockDetails, float? landscapeAreaValue1)
        {
            DocumentRange[] ranges = document.FindAll("<apartmentLandDetailInfo>", DevExpress.XtraRichEdit.API.Native.SearchOptions.None);
            document.ReplaceAll("<apartmentLandDetailInfo>", "", SearchOptions.None);
            DocumentRange range = ranges[0];

            string listLandPrice = "";

            pricingApartmentLandDetailGroupByDecrees.ForEach(pricingApartmentLandDetailsGroupByDecree =>
            {
                range = document.InsertText(range.End, $"* Giá đất ở sau khi chuyển quyền sử dụng: ");
                CharacterProperties cp = document.BeginUpdateCharacters(range);
                cp.FontSize = 14;
                cp.Bold = true;
                document.EndUpdateCharacters(cp);

                range = document.InsertText(range.End, $"{pricingApartmentLandDetailsGroupByDecree.LandPriceItemNote}, đơn giá mặt tiền đường { UtilsService.FormatMoney(pricingApartmentLandDetailsGroupByDecree.LandPriceItemValue) } đồng/m2\n");
                CharacterProperties cp1 = document.BeginUpdateCharacters(range);
                cp1.FontSize = 14;
                cp1.Bold = false;
                document.EndUpdateCharacters(cp1);

                if(pricingApartmentLandDetailsGroupByDecree.Decree != DecreeEnum.ND_CP_61 || block.IsFrontOfLine_61 != true)
                {
                    range = document.InsertText(range.End, $"* Vị trí: ");
                    CharacterProperties cp2 = document.BeginUpdateCharacters(range);
                    cp2.FontSize = 14;
                    cp2.Bold = true;
                    document.EndUpdateCharacters(cp2);

                    //lấy vị trí đất ở hoặc vị trí hẻm
                    string position = "";
                    switch (pricingApartmentLandDetailsGroupByDecree.Decree)
                    {
                        case DecreeEnum.ND_CP_99_2015:
                            position = getLocationResidentialLand(block.LandscapeLocation_99);
                            break;
                        case DecreeEnum.ND_CP_34_2013:
                            position = block.CaseApply_34 == TypeCaseApply_34.KHOAN_1 ? getLocationResidentialLand(block.LandscapeLocationInAlley_34) : (block.CaseApply_34 == TypeCaseApply_34.KHOAN_2 ? getLocationResidentialLand(block.LandscapeLocation_34) : "");
                            break;
                        case DecreeEnum.ND_CP_61:
                            position = getLocationResidentialLand(block.LandscapeLocationInAlley_61);
                            break;
                        default: break;
                    }

                    range = document.InsertText(range.End, $"{position} \t");
                    CharacterProperties cp3 = document.BeginUpdateCharacters(range);
                    cp3.FontSize = 14;
                    cp3.Bold = false;
                    document.EndUpdateCharacters(cp3);
                }

                range = document.InsertText(range.End, $"* Chiều rộng hẻm: ");
                CharacterProperties cp4 = document.BeginUpdateCharacters(range);
                cp4.FontSize = 14;
                cp4.Bold = true;
                document.EndUpdateCharacters(cp4);

                string alley_info = $"{(block.Width != null ? block.Width + ", " : "")}" + $"{block.TextBasedInfo}\n";
                //string alley_info = $"{block.Width}m" + (block.Deep != null ? $", độ sâu vị trí {block.Deep}m" : "") + $", {block.TextBasedInfo}\n";
                //range = document.InsertText(range.End, $"{block.Width}m {block.TextBasedInfo}\n");
                range = document.InsertText(range.End, alley_info);
                CharacterProperties cp5 = document.BeginUpdateCharacters(range);
                cp5.FontSize = 14;
                cp5.Bold = false;
                document.EndUpdateCharacters(cp5);

                range = document.InsertText(range.End, $"* Đơn giá đất ở: {pricingApartmentLandDetailsGroupByDecree.LandScapePrice} \n");
                CharacterProperties cp6 = document.BeginUpdateCharacters(range);
                cp6.FontSize = 14;
                cp6.Bold = false;
                document.EndUpdateCharacters(cp6);

                var apricingApartmentLandDetails = pricingApartmentLandDetailsGroupByDecree.pricingApartmentLandDetails;
                apricingApartmentLandDetails.ForEach(apricingApartmentLandDetail =>
                {
                    range = document.InsertText(range.End, $"Áp dụng theo {getTermApplyName(apricingApartmentLandDetail.TermApply)}\n");
                    CharacterProperties cp7 = document.BeginUpdateCharacters(range);
                    cp7.FontSize = 14;
                    cp7.Bold = true;
                    cp7.Italic = true;
                    cp7.Underline = UnderlineType.Single;
                    document.EndUpdateCharacters(cp7);

                    if(block.TypeReportApply == TypeReportApply.NHA_HO_CHUNG)
                    {
                        range = document.InsertText(range.End, $"- Diện tích đất chung qui đổi:\n");
                        CharacterProperties cp8 = document.BeginUpdateCharacters(range);
                        cp8.FontSize = 14;
                        cp8.Bold = false;
                        cp8.Italic = false;
                        document.EndUpdateCharacters(cp8);

                        range = genConversionAreaTbl(document, range.End, apricingApartmentLandDetail, pricingLandTbls, blockDetails, landscapeAreaValue1);
                    }

                    //Tính giá đất
                    string landPriceStr = $"{UtilsService.FormatMoney(apricingApartmentLandDetail.LandUnitPrice)}";
                    if(apricingApartmentLandDetail.TermApply == TermApply.DIEU_65 || apricingApartmentLandDetail.TermApply == TermApply.KHOAN_1_DIEU_34 || apricingApartmentLandDetail.TermApply == TermApply.DIEU_7)
                    {
                        if (block.TypeReportApply == TypeReportApply.NHA_HO_CHUNG)
                        {
                            landPriceStr += $" x ({(apricingApartmentLandDetail.PrivateArea != null ? ((float)apricingApartmentLandDetail.PrivateArea).ToString("N2", culture) : "0")} x 40% + {(apricingApartmentLandDetail.ConversionArea != null ? ((float)apricingApartmentLandDetail.ConversionArea).ToString("N2", culture) : "0" )} x 10% + {(apricingApartmentLandDetail.GeneralArea != null ? ((float)apricingApartmentLandDetail.GeneralArea).ToString("N2", culture) : "0")} x 40%)";
                        }
                        else if(block.TypeReportApply == TypeReportApply.NHA_RIENG_LE)
                        {
                            landPriceStr += $" x ({(apricingApartmentLandDetail.InLimitArea != null ? ((float)apricingApartmentLandDetail.InLimitArea).ToString("N2", culture) : "0")} x {(apricingApartmentLandDetail.InLimitPercent != null ? ((float)apricingApartmentLandDetail.InLimitPercent).ToString("N2", culture) : "0")}% + {(apricingApartmentLandDetail.OutLimitArea != null ? ((float)apricingApartmentLandDetail.OutLimitArea).ToString("N2", culture) : "0")} x {(apricingApartmentLandDetail.OutLimitPercent != null ? ((float)apricingApartmentLandDetail.OutLimitPercent).ToString("N2", culture) : "0")}%)";
                        }
                        else
                        {
                            landPriceStr += $" x {(apricingApartmentLandDetail.PrivateArea != null ? ((float)apricingApartmentLandDetail.PrivateArea).ToString("N2", culture) : "0")}";
                            if (apricingApartmentLandDetail.CoefficientDistribution != null)
                                landPriceStr += $" x {((float)apricingApartmentLandDetail.CoefficientDistribution).ToString("N2", culture)}";
                            landPriceStr += " x 0.1";

                            float? flatCoefficient = null;
                            if(pricingApartmentLandDetailsGroupByDecree.Decree == DecreeEnum.ND_CP_99_2015)
                            {
                                flatCoefficient = pricing.FlatCoefficient_99;
                            }
                            else if(pricingApartmentLandDetailsGroupByDecree.Decree == DecreeEnum.ND_CP_34_2013)
                            {
                                flatCoefficient = pricing.FlatCoefficient_34;
                            }
                            else if (pricingApartmentLandDetailsGroupByDecree.Decree == DecreeEnum.ND_CP_61)
                            {
                                flatCoefficient = pricing.FlatCoefficient_61;
                            }

                            if(flatCoefficient != null)
                                landPriceStr += $" x {(flatCoefficient != null ? ((float)flatCoefficient).ToString("N2", culture) : "" )}";
                        }

                        if (apricingApartmentLandDetail.DeductionLandMoneyValue != null)
                            landPriceStr += $" x (100% - {((float)apricingApartmentLandDetail.DeductionLandMoneyValue).ToString("N2", culture)}%)";
                    }
                    else if(apricingApartmentLandDetail.TermApply == TermApply.DIEU_70 || apricingApartmentLandDetail.TermApply == TermApply.KHOAN_2_DIEU_34)
                    {
                        if (block.TypeReportApply == TypeReportApply.NHA_HO_CHUNG)
                        {
                            landPriceStr += $" x ({(apricingApartmentLandDetail.PrivateArea != null ? ((float)apricingApartmentLandDetail.PrivateArea).ToString("N2", culture) : "0")} + {(apricingApartmentLandDetail.ConversionArea != null ? ((float)apricingApartmentLandDetail.ConversionArea).ToString("N2", culture) : "0")} + {(apricingApartmentLandDetail.GeneralArea != null ? ((float)apricingApartmentLandDetail.GeneralArea).ToString("N2", culture) : "0")}) x 100%";
                        }
                        else if (block.TypeReportApply == TypeReportApply.NHA_RIENG_LE)
                        {
                            landPriceStr += $" x ({(apricingApartmentLandDetail.InLimitArea != null ? ((float)apricingApartmentLandDetail.InLimitArea).ToString("N2", culture) : "0")} + {(apricingApartmentLandDetail.OutLimitArea != null ? ((float)apricingApartmentLandDetail.OutLimitArea).ToString("N2", culture) : "0")}) x 100%";
                        }
                        else
                        {
                            landPriceStr += $" x {(apricingApartmentLandDetail.PrivateArea != null ? ((float)apricingApartmentLandDetail.PrivateArea).ToString("N2", culture) : "0")}";
                            if (apricingApartmentLandDetail.CoefficientDistribution != null)
                                landPriceStr += $" x {((float)apricingApartmentLandDetail.CoefficientDistribution).ToString("N2", culture)}";

                            float? flatCoefficient = null;
                            if (pricingApartmentLandDetailsGroupByDecree.Decree == DecreeEnum.ND_CP_99_2015)
                            {
                                flatCoefficient = pricing.FlatCoefficient_99;
                            }
                            else if (pricingApartmentLandDetailsGroupByDecree.Decree == DecreeEnum.ND_CP_34_2013)
                            {
                                flatCoefficient = pricing.FlatCoefficient_34;
                            }
                            else if (pricingApartmentLandDetailsGroupByDecree.Decree == DecreeEnum.ND_CP_61)
                            {
                                flatCoefficient = pricing.FlatCoefficient_61;
                            }

                            if (flatCoefficient != null)
                                landPriceStr += $" x {((float)flatCoefficient).ToString("N2", culture)}";

                            landPriceStr += " x 100%";
                        }
                    }
                    else if(apricingApartmentLandDetail.TermApply == TermApply.DIEU_71 || apricingApartmentLandDetail.TermApply == TermApply.DIEU_35)
                    {
                        if(block.TypeReportApply == TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG)
                        {
                            landPriceStr += $" x {(apricingApartmentLandDetail.PrivateArea != null ? ((float)apricingApartmentLandDetail.PrivateArea).ToString("N2", culture) : "0")} x 100%";
                        }
                    }

                    landPriceStr += $" = {UtilsService.FormatMoney(apricingApartmentLandDetail.LandPrice)} đồng";

                    range = document.InsertText(range.End, $"- Giá đất = {landPriceStr}\n");
                    CharacterProperties cp9 = document.BeginUpdateCharacters(range);
                    cp9.FontSize = 14;
                    cp9.Bold = false;
                    cp9.Italic = false;
                    document.EndUpdateCharacters(cp9);

                    listLandPrice = listLandPrice == "" ? UtilsService.FormatMoney(apricingApartmentLandDetail.LandPrice) : String.Join(" + ", listLandPrice, UtilsService.FormatMoney(apricingApartmentLandDetail.LandPrice));
                });

                range = document.InsertText(range.End, $"\n");
            });

            document.ReplaceAll("<listLandPrice>", listLandPrice, SearchOptions.None);
            document.ReplaceAll("<landPrice>", UtilsService.FormatMoney(pricing.LandPrice), SearchOptions.None);
            document.ReplaceAll("<totalPrice>", UtilsService.FormatMoney(pricing.TotalPrice), SearchOptions.None);
        }
    }
}
