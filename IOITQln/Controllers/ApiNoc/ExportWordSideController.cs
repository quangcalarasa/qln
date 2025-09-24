using AutoMapper;
using DevExpress.ClipboardSource.SpreadsheetML;
using DevExpress.XtraRichEdit;
using DevExpress.XtraRichEdit.API.Native;
using IOITQln.Common.Constants;
using IOITQln.Common.Enums;
using IOITQln.Common.Services;
using IOITQln.Common.ViewModels.Common;
using IOITQln.Controllers.ApiInv;
using IOITQln.Entities;
using IOITQln.Models.Data;
using IOITQln.Models.Dto;
using IOITQln.Persistence;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static IOITQln.Common.Enums.AppEnums;
using static IOITQln.Controllers.ApiInv.HouseController;
using Table = DevExpress.XtraRichEdit.API.Native.Table;

namespace IOITQln.Controllers.ApiNoc
{
[Route("api/[controller]")]
[ApiController]
public class ExportWordSideController : ControllerBase
{
        private static readonly ILog log = LogMaster.GetLogger("ExportWord", "ExportWord");

        private static string functionCode = "PRICING";

        private IWebHostEnvironment _hostingEnvironment;

        private readonly ApiDbContext _context;

        private IMapper _mapper;


        CultureInfo culture = CultureInfo.CreateSpecificCulture("de-DE");

        public ExportWordSideController(ApiDbContext context, IMapper mapper, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        }

        [HttpPost("GetExportWordPL1/{id}")]
        public async Task<IActionResult> GetExportWordPL1(int id)
        {
            string accessToken = base.Request.Headers[HeaderNames.Authorization];
            Token token = ((IQueryable<Token>)_context.Tokens).Where((Token t) => Convert.ToString(t.AccessToken) == accessToken).FirstOrDefault();
        if (token == null)
        {
                return Unauthorized();
        }
            DefaultResponse defaultResponse = new DefaultResponse();
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)base.User.Identity;
            string access_key = (from c in claimsIdentity.Claims
                                 where c.Type == "AccessKey"
                                 select c.Value).SingleOrDefault();
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, 5))
            {
                defaultResponse.meta = new Meta(222, "Bạn không có quyền xem dữ liệu tới mục này!");
                return Ok(defaultResponse);
            }
                try
                {
                string text = insertDataToTemplatePL1(id);
                    if (text != null)
                    {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {

                            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
                            string fileDownloadName = "bien_ban-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";
                        using (FileStream fileStream = new FileStream(text, FileMode.Open, FileAccess.Read))
                            {
                                fileStream.CopyTo(memoryStream);
                            }
                            try
                            {
                                System.IO.File.Delete(text);
                            }
                            catch
                            {
                            }
                        return File(memoryStream.ToArray(), "application/octet-stream", fileDownloadName);


                    }
                }
                        defaultResponse.meta = new Meta(215, "Data file null!");
                return Ok(defaultResponse);
                }
                catch (Exception ex)
                {
                    log.Error((object)("GetExportWordExample:" + ex.Message));
                    throw;
                }
}

private string insertDataToTemplatePL1(int id)
{
    //IL_035f: Unknown result type (might be due to invalid IL or missing references)
    //IL_0366: Expected O, but got Unknown
    //IL_036a: Unknown result type (might be due to invalid IL or missing references)
    //IL_3a3d: Unknown result type (might be due to invalid IL or missing references)
    Pricing pricing = ((IQueryable<Pricing>)_context.Pricings).Where((Pricing p) => p.Id == id && (int)p.Status != 99).FirstOrDefault();
    if (pricing != null)
    {
        Block block = ((IQueryable<Block>)_context.Blocks).Where((Block b) => b.Id == pricing.BlockId && (int)b.Status != 99).FirstOrDefault();
        Apartment apartment = ((IQueryable<Apartment>)_context.Apartments).Where((Apartment a) => a.Id == pricing.ApartmentId && (int)a.Status != 99).FirstOrDefault();
        if (block != null)
        {
            if ((block.TypeReportApply == AppEnums.TypeReportApply.NHA_HO_CHUNG || block.TypeReportApply == AppEnums.TypeReportApply.NHA_CHUNG_CU || block.TypeReportApply == AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG) && apartment == null)
            {
                return null;
            }
            string text = Path.Combine(_hostingEnvironment.WebRootPath, "exportword/phu_luc_01.docx");
            string text2 = "result" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";

                    //   RichEditDocumentServer wordProcessor = new RichEditDocumentServer();
                    RichEditDocumentServer wordProcessor = new RichEditDocumentServer();
                    //wordProcessor.LoadDocumentTemplate(text, DocumentFormat.OpenXml);
                    //var document = wordProcessor.Document;
                    //document.BeginUpdate();

            RichEditDocumentServer val = new RichEditDocumentServer();
            val.LoadDocumentTemplate(text, DocumentFormat.OpenXml);
                    var document = val.Document;
                    document.BeginUpdate();
            string text3 = ((!pricing.DateCreate.HasValue) ? "" : ((pricing.DateCreate.Value.Day < 10) ? ("0" + pricing.DateCreate.Value.Day) : pricing.DateCreate.Value.Day.ToString()));
                    document.ReplaceAll("<day>", text3, SearchOptions.None);
            string text4 = ((!pricing.DateCreate.HasValue) ? "" : ((pricing.DateCreate.Value.Month < 10) ? ("0" + pricing.DateCreate.Value.Month) : pricing.DateCreate.Value.Month.ToString()));
                    document.ReplaceAll("<month>", text4, (SearchOptions)0);
            string text5 = (pricing.DateCreate.HasValue ? pricing.DateCreate.Value.Year.ToString() : "");
                    document.ReplaceAll("<year>", text5, (SearchOptions)0);
            List<DecreeMap> decreeMaps = ((IQueryable<DecreeMap>)_context.DecreeMaps).Where((DecreeMap d) => d.TargetId == (long)block.Id && (int)d.Type == 1 && (int)d.Status != 99).ToList();
            List<string> title = getTitle(decreeMaps);
                    document.ReplaceAll("<title>", title[0], (SearchOptions)0);
                    document.ReplaceAll("<listDecreeApply>", title[1], (SearchOptions)0);
            List<PricingReplaced> source = ((IQueryable<PricingReplaced>)_context.PricingReplaceds).Where((PricingReplaced p) => p.PricingId == pricing.Id && (int)p.Status != 99).ToList();
            string text6 = string.Join(", ", source.Select((PricingReplaced x) => x.DateCreate.Value.ToString("dd/MM/yyyy")).ToList());
                    document.ReplaceAll("<pricingReplaceds>", (text6 != "") ? text6 : "...............", (SearchOptions)0);
            List<PricingOfficer> list = ((IQueryable<PricingOfficer>)_context.PricingOfficers).Where((PricingOfficer a) => a.PricingId == pricing.Id && (int)a.Status != 99).ToList();
                    document.ReplaceAll("<LandPriceTotal>", UtilsService.FormatMoney(pricing.LandPrice), (SearchOptions)0);
                    document.ReplaceAll("<Total>", UtilsService.FormatMoney(pricing.TotalPrice), (SearchOptions)0);
            List<Customer> list2 = (from c in (IQueryable<Customer>)_context.Customers
                                    join pc in (IEnumerable<PricingCustomer>)_context.PricingCustomers on c.Id equals pc.CustomerId
                                    where (int)c.Status != 99 && (int)pc.Status != 99 && pc.PricingId == pricing.Id
                                    select c).ToList();
            string text7 = "";
            string text8 = "";
            foreach (Customer item in list2)
            {
                if (item.Sex.GetValueOrDefault() == AppEnums.TypeSex.FEMALE)
                {
                    text8 = ((text8 == "") ? item.FullName : (text8 + ", " + item.FullName));
                }
                else
                {
                    text7 = ((text7 == "") ? item.FullName : (text7 + ", " + item.FullName));
                }
            }
                    document.ReplaceAll("<maleCustomer>", (text7 != "") ? text7 : "✓", (SearchOptions)0);
                    document.ReplaceAll("<femaleCustomer>", (text8 != "") ? text8 : "✓", (SearchOptions)0);
                    document.ReplaceAll("<timeUse>", pricing.TimeUse ?? "", (SearchOptions)0);
                    document.ReplaceAll("<blockAdr>", block.Address ?? "", (SearchOptions)0);
            if (block.TypeReportApply == AppEnums.TypeReportApply.NHA_HO_CHUNG || block.TypeReportApply == AppEnums.TypeReportApply.NHA_CHUNG_CU || block.TypeReportApply == AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG)
            {
                        document.ReplaceAll("<apartAdr>", apartment.Address, (SearchOptions)0);
            }
            Lane lane = ((IQueryable<Lane>)_context.Lanies).Where((Lane l) => (int?)l.Id == block.Lane && (int)l.Status != 99).FirstOrDefault();
                    document.ReplaceAll("<lane>", (lane != null) ? lane.Name : "", (SearchOptions)0);
            Ward ward = ((IQueryable<Ward>)_context.Wards).Where((Ward l) => l.Id == block.Ward && (int)l.Status != 99).FirstOrDefault();
                    document.ReplaceAll("<ward>", (ward != null) ? ward.Name : "", (SearchOptions)0);
            District district = ((IQueryable<District>)_context.Districts).Where((District l) => l.Id == block.District && (int)l.Status != 99).FirstOrDefault();
                    document.ReplaceAll("<district>", (district != null) ? district.Name : "", (SearchOptions)0);
            string typeBlock = getTypeBlock(block.TypeBlockId);
                    document.ReplaceAll("<typeBlock>", typeBlock, (SearchOptions)0);
            List<LevelBlockMap> levelBlockMaps = ((IQueryable<LevelBlockMap>)_context.LevelBlockMaps).Where((LevelBlockMap l) => l.BlockId == block.Id && (int)l.Status != 99).ToList();
            string levelBlock = getLevelBlock(levelBlockMaps);
                    document.ReplaceAll("<levelBlock>", levelBlock, (SearchOptions)0);
                    document.ReplaceAll("<Level>", levelBlock, (SearchOptions)0);
            string floorBlockMap = block.FloorBlockMap;
                    document.ReplaceAll("<floorBlockMap>", floorBlockMap, (SearchOptions)0);
                    document.ReplaceAll("<Floor>", floorBlockMap, (SearchOptions)0);
                    _ = block.ConstructionAreaValue;
            string text9 = block.ConstructionAreaValue.ToString("N2", culture);
                    document.ReplaceAll("<bcav>", text9, (SearchOptions)0);
            string text10 = ((block.SellConstructionAreaValue != null) ? float.Parse(block.SellConstructionAreaValue, CultureInfo.InvariantCulture.NumberFormat).ToString("N2", culture) : "0");
                    document.ReplaceAll("<bscav>", text10, (SearchOptions)0);
                    document.ReplaceAll("<bcaNote>", (block.ConstructionAreaNote != null && block.ConstructionAreaNote != "") ? ("(" + block.ConstructionAreaNote + ")") : "", (SearchOptions)0);
            string text11 = (block.ConstructionAreaValue1.HasValue ? block.ConstructionAreaValue1.Value.ToString("N2", culture) : "0");
                    document.ReplaceAll("<bcav1>", text11, (SearchOptions)0);
            string text12 = (block.ConstructionAreaValue2.HasValue ? block.ConstructionAreaValue2.Value.ToString("N2", culture) : "0");
                    document.ReplaceAll("<bcav2>", text12, (SearchOptions)0);
            string text13 = (block.ConstructionAreaValue3.HasValue ? block.ConstructionAreaValue3.Value.ToString("N2", culture) : "0");
                    document.ReplaceAll("<bcav3>", text13, (SearchOptions)0);
            List<BlockDetail> list3 = ((IQueryable<BlockDetail>)_context.BlockDetails).Where((BlockDetail l) => l.BlockId == block.Id && (int)l.Status != 99).ToList();
            string text14 = "";
            if (block.TypeReportApply == AppEnums.TypeReportApply.NHA_HO_CHUNG || block.TypeReportApply == AppEnums.TypeReportApply.NHA_CHUNG_CU || block.TypeReportApply == AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG)
            {
                if (block.TypeReportApply == AppEnums.TypeReportApply.NHA_HO_CHUNG)
                {
                    List<BlockDetailData> list4 = ((IMapperBase)_mapper).Map<List<BlockDetailData>>((object)list3);
                    foreach (BlockDetailData map_BlockDetail in list4)
                    {
                        Floor floor = ((IQueryable<Floor>)_context.Floors).Where((Floor f) => f.Id == map_BlockDetail.FloorId && (int)f.Status != 99).FirstOrDefault();
                        map_BlockDetail.FloorName = ((floor != null) ? floor.Name : "");
                                map_BlockDetail.FloorCode = floor?.Code ?? 0;
                        Area area = ((IQueryable<Area>)_context.Areas).Where((Area f) => f.Id == map_BlockDetail.AreaId && (int)f.Status != 99).FirstOrDefault();
                        map_BlockDetail.AreaName = ((area != null) ? area.Name : "");
                                map_BlockDetail.IsMezzanine = area?.IsMezzanine;
                    }
                    list4 = (from x in list4
                             orderby x.FloorCode, x.IsMezzanine
                             select x).ToList();
                }
                List<ApartmentDetail> list5 = ((IQueryable<ApartmentDetail>)_context.ApartmentDetails).Where((ApartmentDetail a) => a.TargetId == apartment.Id && (int)a.Type == 2 && (int)a.Status != 99).ToList();
                List<ApartmentDetailData> list6 = ((IMapperBase)_mapper).Map<List<ApartmentDetailData>>((object)list5);
                foreach (ApartmentDetailData map_ApartmentDetail2 in list6)
                {
                    Floor floor2 = ((IQueryable<Floor>)_context.Floors).Where((Floor f) => f.Id == map_ApartmentDetail2.FloorId && (int)f.Status != 99).FirstOrDefault();
                    map_ApartmentDetail2.FloorName = ((floor2 != null) ? floor2.Name : "");
                            map_ApartmentDetail2.FloorCode = floor2?.Code ?? 0;
                    text14 = text14 + floor2.Name + " ";
                    Area area2 = ((IQueryable<Area>)_context.Areas).Where((Area f) => f.Id == map_ApartmentDetail2.AreaId && (int)f.Status != 99).FirstOrDefault();
                    map_ApartmentDetail2.AreaName = ((area2 != null) ? area2.Name : "");
                            map_ApartmentDetail2.IsMezzanine = area2?.IsMezzanine;
                }
                list6 = (from x in list6
                         orderby x.Level, x.FloorCode, x.IsMezzanine
                         select x).ToList();
                if (block.TypeReportApply == AppEnums.TypeReportApply.NHA_CHUNG_CU || block.TypeReportApply == AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG || block.ParentTypeReportApply.GetValueOrDefault() == AppEnums.TypeReportApply.NHA_CHUNG_CU)
                {
                    string levelApartment = getLevelApartment(list6);
                            document.ReplaceAll("<levelApartment>", levelApartment, (SearchOptions)0);
                }
                string apartDetailInfo = getApartDetailInfo(list6);
                        document.ReplaceAll("<apartDetailInfo>", apartDetailInfo, (SearchOptions)0);
                string text15 = (apartment.ConstructionAreaValue.HasValue ? apartment.ConstructionAreaValue.Value.ToString("N2", culture) : "0");
                        document.ReplaceAll("<acav>", text15, (SearchOptions)0);
                        document.ReplaceAll("<acaNote>", (apartment.ConstructionAreaNote != null && apartment.ConstructionAreaNote != "") ? ("(" + apartment.ConstructionAreaNote + ")") : "", (SearchOptions)0);
                string text16 = (apartment.ConstructionAreaValue1.HasValue ? apartment.ConstructionAreaValue1.Value.ToString("N2", culture) : "0");
                        document.ReplaceAll("<acav1>", text16, (SearchOptions)0);
                string text17 = (apartment.ConstructionAreaValue2.HasValue ? apartment.ConstructionAreaValue2.Value.ToString("N2", culture) : "0");
                        document.ReplaceAll("<acav2>", text17, (SearchOptions)0);
                string text18 = (apartment.ConstructionAreaValue3.HasValue ? apartment.ConstructionAreaValue3.Value.ToString("N2", culture) : "0");
                        document.ReplaceAll("<acav3>", text18, (SearchOptions)0);
                string text19 = (apartment.UseAreaValue.HasValue ? apartment.UseAreaValue.Value.ToString("N2", culture) : "0");
                        document.ReplaceAll("<auav>", text19, (SearchOptions)0);
                string text20 = (apartment.UseAreaValue1.HasValue ? apartment.UseAreaValue1.Value.ToString("N2", culture) : "0");
                        document.ReplaceAll("<auav1>", text20, (SearchOptions)0);
                string text21 = (apartment.UseAreaValue2.HasValue ? apartment.UseAreaValue2.Value.ToString("N2", culture) : "0");
                        document.ReplaceAll("<auav2>", text21, (SearchOptions)0);
            }
            else
            {
                List<ApartmentDetail> list7 = ((IQueryable<ApartmentDetail>)_context.ApartmentDetails).Where((ApartmentDetail a) => a.TargetId == block.Id && (int)a.Type == 1 && (int)a.Status != 99).ToList();
                List<ApartmentDetailData> list8 = ((IMapperBase)_mapper).Map<List<ApartmentDetailData>>((object)list7);
                foreach (ApartmentDetailData map_ApartmentDetail in list8)
                {
                    Floor floor3 = ((IQueryable<Floor>)_context.Floors).Where((Floor f) => f.Id == map_ApartmentDetail.FloorId && (int)f.Status != 99).FirstOrDefault();
                    map_ApartmentDetail.FloorName = ((floor3 != null) ? floor3.Name : "");
                            map_ApartmentDetail.FloorCode = floor3?.Code ?? 0;
                    text14 = text14 + floor3.Name + " ";
                    Area area3 = ((IQueryable<Area>)_context.Areas).Where((Area f) => f.Id == map_ApartmentDetail.AreaId && (int)f.Status != 99).FirstOrDefault();
                    map_ApartmentDetail.AreaName = ((area3 != null) ? area3.Name : "");
                            map_ApartmentDetail.IsMezzanine = area3?.IsMezzanine;
                }
                list8 = (from x in list8
                         orderby x.Level, x.FloorCode, x.IsMezzanine
                         select x).ToList();
            }
                    document.ReplaceAll("<FloorName>", text14 ?? "", (SearchOptions)0);
            List<BlockMaintextureRateData> list9 = (from b in (IQueryable<BlockMaintextureRate>)_context.BlockMaintextureRaties
                                                    where b.TargetId == block.Id && (int)b.Type == 1 && (int)b.Status != 99
                                                    select b into e
                                                    select new BlockMaintextureRateData
                                                    {
                                                        LevelBlockId = e.LevelBlockId,
                                                        RatioMainTextureId = e.RatioMainTextureId,
                                                        TypeMainTexTure = e.TypeMainTexTure,
                                                        CurrentStateMainTextureId = e.CurrentStateMainTextureId,
                                                        RemainingRate = e.RemainingRate,
                                                        MainRate = e.MainRate,
                                                        TotalValue = e.TotalValue,
                                                        TotalValue1 = e.TotalValue1,
                                                        TotalValue2 = e.TotalValue2,
                                                        CurrentStateMainTextureName = ((IQueryable<CurrentStateMainTexture>)_context.CurrentStateMainTexturies).Where((CurrentStateMainTexture c) => c.Id == e.CurrentStateMainTextureId).FirstOrDefault().Name,
                                                        TypeMainTexTureName = getTypeMainTexTureName(e.TypeMainTexTure)
                                                    }).ToList();
            List<IGrouping<int, BlockMaintextureRateData>> list10 = (from x in list9
                                                                     group x by x.LevelBlockId into x
                                                                     orderby x.Key
                                                                     select x).ToList();
            string dataStr = "";
            string dataStr2 = "";
            string dataStr3 = "";
            list10.ForEach(delegate (IGrouping<int, BlockMaintextureRateData> item)
            {
                string arg = (item.FirstOrDefault().TotalValue.HasValue ? item.FirstOrDefault().TotalValue.Value.ToString("N2", culture) : "");
                string arg2 = (item.FirstOrDefault().TotalValue1.HasValue ? item.FirstOrDefault().TotalValue1.Value.ToString("N2", culture) : "");
                string arg3 = (item.FirstOrDefault().TotalValue2.HasValue ? item.FirstOrDefault().TotalValue2.Value.ToString("N2", culture) : "");
                        dataStr = ((dataStr == "") ? $"- Phần nhà cấp {item.Key}    =     {item.FirstOrDefault().TotalValue} %" : string.Join("\n", dataStr, $"- Phần nhà cấp {item.Key}    =     {arg} %"));
                        dataStr2 = ((dataStr2 == "") ? $"* Phần nhà cấp {item.Key}    =     {item.FirstOrDefault().TotalValue1} %" : string.Join("\n", dataStr2, $"* Phần nhà cấp {item.Key}    =     {arg2} %"));
                        dataStr3 = ((dataStr3 == "") ? $"* Phần nhà cấp {item.Key}    =     {item.FirstOrDefault().TotalValue2} %" : string.Join("\n", dataStr3, $"* Phần nhà cấp {item.Key}    =     {arg3} %"));
            });
            genBlockMaintextureRateTbla(document, list9, dataStr2);
            genBlockMaintextureRateTblb(document, dataStr3);
            genBlockMaintextureRateTblc(document, dataStr);
            List<PricingLandTbl> list11 = ((IQueryable<PricingLandTbl>)_context.PricingLandTbls).Where((PricingLandTbl a) => a.PricingId == pricing.Id && (int)a.Status != 99).ToList();
            List<PricingLandTblData> list12 = ((IMapperBase)_mapper).Map<List<PricingLandTblData>>((object)list11);
            list12.ForEach(delegate (PricingLandTblData pricingLandTblData)
            {
                Area area4 = ((IQueryable<Area>)_context.Areas).Where((Area a) => a.Id == pricingLandTblData.AreaId && (int)a.Status != 99).FirstOrDefault();
                pricingLandTblData.AreaName = ((area4 != null) ? area4.Name : "");
                PriceListItem priceListItem = ((IQueryable<PriceListItem>)_context.PriceListItems).Where((PriceListItem p) => (int?)p.Id == pricingLandTblData.PriceListItemId).FirstOrDefault();
                if (priceListItem != null)
                {
                    PriceList priceList = ((IQueryable<PriceList>)_context.PriceLists).Where((PriceList p) => p.Id == priceListItem.PriceListId).FirstOrDefault();
                            pricingLandTblData.PriceListItemNote = "Căn cứ " + priceListItem.NameOfConstruction + ((priceList != null && priceList.Des != null) ? (" theo " + priceList?.Des) : "");
                }
                if (block.TypeReportApply == AppEnums.TypeReportApply.NHA_CHUNG_CU || (block.TypeReportApply == AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG && block.ParentTypeReportApply.GetValueOrDefault() == AppEnums.TypeReportApply.NHA_CHUNG_CU))
                {
                    if (pricingLandTblData.DecreeType1Id.GetValueOrDefault() == AppEnums.DecreeEnum.ND_CP_99_2015 && pricingLandTblData.ApplyInvestmentRate.GetValueOrDefault() && !pricingLandTblData.IsMezzanine.GetValueOrDefault())
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
            List<ConstructionPrice> constructionPrices = (from pcp in (IQueryable<PricingConstructionPrice>)_context.PricingConstructionPricies
                                                          join cp in (IEnumerable<ConstructionPrice>)_context.ConstructionPricies on pcp.ConstructionPriceId equals cp.Id
                                                          where (int)pcp.Status != 99 && (int)cp.Status != 99 && pcp.PricingId == pricing.Id
                                                          select cp into x
                                                          orderby x.Year
                                                          select x).ToList();
            genPricingLand(document, pricing.DateCreate, list12, constructionPrices, pricing.Vat, pricing.ApartmentPrice, block.TypeReportApply, pricing.AreaCorrectionCoefficientId, block.ParentTypeReportApply);
            if (block.TypeReportApply != AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG)
            {
                        document.ReplaceAll("<apartmentPriceReducedNote>", pricing.ApartmentPriceReducedNote ?? ".....", (SearchOptions)0);
                List<PricingReducedPersonData> list13 = (from a in (IQueryable<PricingReducedPerson>)_context.PricingReducedPeople
                                                         where a.PricingId == pricing.Id && (int)a.Status != 99
                                                         select a into e
                                                         select new PricingReducedPersonData
                                                         {
                                                             CustomerName = ((IQueryable<Customer>)_context.Customers).Where((Customer c) => c.Id == e.CustomerId).FirstOrDefault().FullName,
                                                             Year = e.Year,
                                                             Salary = e.Salary,
                                                             DeductionCoefficient = e.DeductionCoefficient,
                                                             Value = e.Value
                                                         }).ToList();
            }
                    document.ReplaceAll("<apartmentPriceReduced>", UtilsService.FormatMoney(pricing.ApartmentPriceReduced.GetValueOrDefault()), (SearchOptions)0);
                    document.ReplaceAll("<apartmentPrice>", UtilsService.FormatMoney(pricing.ApartmentPrice), (SearchOptions)0);
                    document.ReplaceAll("<apartmentPriceRemaining>", UtilsService.FormatMoney(pricing.ApartmentPriceRemaining), (SearchOptions)0);
                    document.ReplaceAll("<apartmentPriceRemainingStr>", UtilsService.ConvertMoneyToString(pricing.ApartmentPriceRemaining.Value.ToString("N0", culture)).ToLower(), (SearchOptions)0);
                    document.ReplaceAll("<vat>", pricing.Vat.HasValue ? (pricing.Vat.Value.ToString("N2", culture) + "%") : "0%", (SearchOptions)0);
                    document.ReplaceAll("<apartmentPriceNoVat>", UtilsService.FormatMoney(pricing.ApartmentPriceNoVat), (SearchOptions)0);
                    document.ReplaceAll("<apartmentPriceVat>", UtilsService.FormatMoney(pricing.ApartmentPriceVat), (SearchOptions)0);
                    document.ReplaceAll("<landUsePlanningInfo>", block.LandUsePlanningInfo ?? "", (SearchOptions)0);
                    document.ReplaceAll("<highwayPlanningInfo>", block.HighwayPlanningInfo ?? "", (SearchOptions)0);
                    document.ReplaceAll("<landAcquisitionSituationInfo>", block.LandAcquisitionSituationInfo ?? "", (SearchOptions)0);
                    document.ReplaceAll("<landNo>", block.LandNo ?? "", (SearchOptions)0);
                    document.ReplaceAll("<mapNo>", block.MapNo ?? "", (SearchOptions)0);
            if (block.TypeReportApply == AppEnums.TypeReportApply.NHA_HO_CHUNG || block.TypeReportApply == AppEnums.TypeReportApply.NHA_CHUNG_CU || block.TypeReportApply == AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG)
            {
                        document.ReplaceAll("<alcav>", apartment.LandscapeAreaValue.HasValue ? apartment.LandscapeAreaValue.Value.ToString("N2", culture) : "0", (SearchOptions)0);
                        document.ReplaceAll("<alcav1>", apartment.LandscapeAreaValue1.HasValue ? apartment.LandscapeAreaValue1.Value.ToString("N2", culture) : "0", (SearchOptions)0);
                        document.ReplaceAll("<alcav2>", apartment.LandscapeAreaValue2.HasValue ? apartment.LandscapeAreaValue2.Value.ToString("N2", culture) : "0", (SearchOptions)0);
                        document.ReplaceAll("<alcav3>", apartment.LandscapeAreaValue3.HasValue ? apartment.LandscapeAreaValue3.Value.ToString("N2", culture) : "0", (SearchOptions)0);
            }
            else if (block.TypeReportApply == AppEnums.TypeReportApply.NHA_RIENG_LE)
            {
                        document.ReplaceAll("<alcav>", block.LandscapeAreaValue.HasValue ? block.LandscapeAreaValue.Value.ToString("N2", culture) : "0", (SearchOptions)0);
                        document.ReplaceAll("<alcav2>", block.LandscapePrivateAreaValue.HasValue ? block.LandscapePrivateAreaValue.Value.ToString("N2", culture) : "0", (SearchOptions)0);
                        document.ReplaceAll("<alcav1>", " ", (SearchOptions)0);
            }
            List<PricingApartmentLandDetail> list14 = (from a in (IQueryable<PricingApartmentLandDetail>)_context.PricingApartmentLandDetails
                                                       where a.PricingId == pricing.Id && (int)a.Status != 99
                                                       select a into x
                                                       orderby x.UpdatedAt
                                                       select x).ToList();
            foreach (PricingApartmentLandDetail item2 in list14)
            {
                        document.ReplaceAll("<buav>", item2.PrivateArea.ToString(), (SearchOptions)0);
            }
            List<PricingApartmentLandDetailGroupByDecree> list15 = (from x in ((IQueryable<PricingApartmentLandDetail>)_context.PricingApartmentLandDetails).Where((PricingApartmentLandDetail a) => a.PricingId == pricing.Id && (int)a.Status != 99).ToList()
                                                                    group x by x.DecreeType1Id into x
                                                                    select new PricingApartmentLandDetailGroupByDecree
                                                                    {
                                                                        Decree = x.Key,
                                                                        pricingApartmentLandDetails = x.ToList()
                                                                    }).ToList();
            list15.ForEach(delegate (PricingApartmentLandDetailGroupByDecree pricingApartmentLandDetailGroupByDecree)
            {
                string text22 = "";
                float? num = null;
                int landPriceItemId = 0;
                double? num2 = null;
                string text23 = "";
                string landScapePrice = "";
                switch (pricingApartmentLandDetailGroupByDecree.Decree)
                {
                    case AppEnums.DecreeEnum.ND_CP_99_2015:
                        landPriceItemId = block.LandPriceItemId_99.GetValueOrDefault();
                        num2 = block.LandPriceItemValue_99;
                        text23 = block.PositionCoefficientStr_99;
                        num = block.LandPriceRefinement_99;
                                landScapePrice = UtilsService.FormatMoney(Convert.ToDecimal(num2)) + " đồng/m2 x " + text23 + (block.ExceedingLimitDeep.GetValueOrDefault() ? $" x (100 - {num})%" : "") + " = " + UtilsService.FormatMoney(Convert.ToDecimal(block.LandScapePrice_99)) + " đồng/m2";
                        break;
                    case AppEnums.DecreeEnum.ND_CP_34_2013:
                        landPriceItemId = block.LandPriceItemId_34.GetValueOrDefault();
                        num2 = block.LandPriceItemValue_34;
                        text23 = ((block.CaseApply_34.GetValueOrDefault() == AppEnums.TypeCaseApply_34.KHOAN_2) ? block.PositionCoefficientStr_34 : block.AlleyPositionCoefficientStr_34);
                        num = block.LandPriceRefinement_34;
                                landScapePrice = UtilsService.FormatMoney(Convert.ToDecimal(num2)) + " đồng/m2 x " + text23 + ((block.ExceedingLimitDeep.GetValueOrDefault() && block.CaseApply_34.GetValueOrDefault() == AppEnums.TypeCaseApply_34.KHOAN_2) ? $" x (100 - {num})%" : "") + " = " + UtilsService.FormatMoney(Convert.ToDecimal((block.CaseApply_34.GetValueOrDefault() == AppEnums.TypeCaseApply_34.KHOAN_2) ? block.LandScapePrice_34 : block.AlleyLandScapePrice_34)) + " đồng/m2";
                        break;
                    case AppEnums.DecreeEnum.ND_CP_61:
                        landPriceItemId = block.LandPriceItemId_61.GetValueOrDefault();
                        num2 = block.LandPriceItemValue_61;
                        text23 = block.PositionCoefficientStr_61;
                        num = block.LandPriceRefinement_61;
                        landScapePrice = ((!block.IsFrontOfLine_61.GetValueOrDefault() && (block.IsFrontOfLine_61.GetValueOrDefault() || block.IsAlley_61.GetValueOrDefault())) ? (UtilsService.FormatMoney(Convert.ToDecimal(block.No2LandScapePrice_61)) + " đồng/m2 x 0.8 = " + UtilsService.FormatMoney(Convert.ToDecimal(block.LandScapePrice_61)) + " đồng/m2") : (UtilsService.FormatMoney(Convert.ToDecimal(block.LandScapePrice_61)) + " đồng/m2"));
                        break;
                }
                LandPriceItem landPriceItem = ((IQueryable<LandPriceItem>)_context.LandPriceItems).Where((LandPriceItem l) => l.Id == landPriceItemId).FirstOrDefault();
                if (landPriceItem != null)
                {
                    text22 = landPriceItem.LaneName + ", đoạn từ " + landPriceItem.LaneStartName + ((landPriceItem.LaneEndName != null && landPriceItem.LaneEndName != "") ? (" đến đoạn " + landPriceItem.LaneEndName) : "");
                    text22 = ((landPriceItem.Des != null) ? (text22 + ", " + landPriceItem.Des) : text22);
                    LandPrice landPrice = ((IQueryable<LandPrice>)_context.LandPricies).Where((LandPrice l) => l.Id == landPriceItem.LandPriceId).FirstOrDefault();
                    if (landPrice != null)
                    {
                        text22 = text22 + ", " + landPrice.Des;
                    }
                }
                pricingApartmentLandDetailGroupByDecree.LandPriceItemNote = text22;
                pricingApartmentLandDetailGroupByDecree.LandPriceItemValue = Convert.ToDecimal(num2);
                pricingApartmentLandDetailGroupByDecree.LandScapePrice = landScapePrice;
                pricingApartmentLandDetailGroupByDecree.LandPriceRefinement = num;
                pricingApartmentLandDetailGroupByDecree.PositionCoefficientStr = text23;
            });
            if (list15.Count() > 0)
            {
                genApartmentLandDetailInfo(document, list15, block, pricing, list11, list3, (apartment != null) ? apartment.LandscapeAreaValue1 : null);
            }
                    document.EndUpdate();
            val.SaveDocument(text2, DocumentFormat.OpenXml);
            return text2;
        }
        return null;
    }
    return null;
}

private List<string> getTitle(List<DecreeMap> decreeMaps)
{
    string title = "";
    string listDecreeApply = "";
    decreeMaps.ForEach(delegate (DecreeMap decreeMap)
    {
        string text = "";
        string text2 = "";
        switch (decreeMap.DecreeType1Id)
        {
            case AppEnums.DecreeEnum.ND_CP_99_2015:
                text2 = "NGHỊ ĐỊNH SỐ 99/2015/NĐ-CP NGÀY 20/10/2015";
                text = "Nghị định số 99/2015/NĐ-CP";
                break;
            case AppEnums.DecreeEnum.ND_CP_34_2013:
                text2 = "NGHỊ ĐỊNH 34/2013/NĐ-CP NGÀY 22/04/2013";
                text = "Nghị định số 34/2013/NĐ-CP";
                break;
            case AppEnums.DecreeEnum.ND_CP_61:
                text2 = "NGHỊ ĐỊNH SỐ 61";
                text = "Nghị định số 61";
                break;
        }
        title = ((title == "") ? ("XÁC ĐỊNH GIÁ BÁN NHÀ THEO " + text2) : (title + " VÀ " + text2));
        listDecreeApply = ((listDecreeApply == "") ? text : (listDecreeApply + " và " + text));
    });
    title = ((title == "") ? title : (title + " CỦA CHÍNH PHỦ"));
    return new List<string>
        {
            title,
            listDecreeApply
        };
}

private string getTypeBlock(int? typeBlockId)
{
    TypeBlock typeBlock = ((IQueryable<TypeBlock>)_context.TypeBlocks).Where((TypeBlock t) => (int?)t.Id == typeBlockId && (int)t.Status != 99).FirstOrDefault();
    if (typeBlock == null)
    {
        return "";
    }
    return typeBlock.Name;
}

private string getLevelBlock(List<LevelBlockMap> levelBlockMaps)
{
    string levelBlock = "";
    levelBlockMaps.ForEach(delegate (LevelBlockMap levelBlockMap)
    {
                levelBlock = ((levelBlock == "") ? $"Cấp {levelBlockMap.LevelId}" : (levelBlock + $" - {levelBlockMap.LevelId}"));
    });
    return levelBlock;
}

private string getApartDetailInfo(List<ApartmentDetailData> apartmentDetails)
{
    string apartDetailInfo = "";
    apartmentDetails.ForEach(delegate (ApartmentDetailData apartmentDetail)
    {
        apartDetailInfo = ((apartDetailInfo == "") ? apartmentDetail.AreaName : (apartDetailInfo + " + " + apartmentDetail.AreaName));
    });
    return apartDetailInfo;
}

private string getLevelApartment(List<ApartmentDetailData> apartmentDetails)
{
    string str = "";
    List<IGrouping<int?, ApartmentDetailData>> list = (from x in apartmentDetails
                                                       group x by x.Level).ToList();
    list.ForEach(delegate (IGrouping<int?, ApartmentDetailData> item)
    {
                str = ((str == "") ? $"Cấp {item.Key}" : $"{str} + Cấp {item.Key}");
    });
    return str;
}

private static string getTypeMainTexTureName(AppEnums.TypeMainTexTure typeMainTexTure)
{
    string result = "";
    switch (typeMainTexTure)
    {
        case AppEnums.TypeMainTexTure.MONG:
            result = "Móng";
            break;
        case AppEnums.TypeMainTexTure.KHUNG_COT:
            result = "Khung cột";
            break;
        case AppEnums.TypeMainTexTure.TUONG:
            result = "Tường";
            break;
        case AppEnums.TypeMainTexTure.NEN_SAN:
            result = "Nền sàn";
            break;
        case AppEnums.TypeMainTexTure.KHUNG_COT_DO_MAI:
            result = "K/c đỡ mái";
            break;
        case AppEnums.TypeMainTexTure.MAI:
            result = "Mái";
            break;
    }
    return result;
}

private string getLocationResidentialLand(AppEnums.LocationResidentialLand? id)
{
    string result = "";
    switch (id)
    {
        case AppEnums.LocationResidentialLand.FIRST:
            result = "Vị trí 1";
            break;
        case AppEnums.LocationResidentialLand.SECOND:
            result = "Vị trí 2";
            break;
        case AppEnums.LocationResidentialLand.THIRD:
            result = "Vị trí 3";
            break;
        case AppEnums.LocationResidentialLand.FOURTH:
            result = "Vị trí 4";
            break;
    }
    return result;
}

private void genBlockMaintextureRateTbla(Document document, List<BlockMaintextureRateData> blockMaintextureRateDatas, string str)
{
    //IL_010a: Unknown result type (might be due to invalid IL or missing references)
    //IL_0114: Expected O, but got Unknown
            DocumentRange[] array = document.FindAll("<maintextureRateTbla>", (SearchOptions)0);
    List<BlockMaintextureRateDataGroupByTypeMainTexTure> blockMaintextureRateDataGroupByTypeMainTexTures = (from x in blockMaintextureRateDatas
                                                                                                            group x by x.TypeMainTexTure into x
                                                                                                            select new BlockMaintextureRateDataGroupByTypeMainTexTure
                                                                                                            {
                                                                                                                TypeMainTexTure = x.Key,
                                                                                                                blockMaintextureRateDatas = x.ToList()
                                                                                                            } into x
                                                                                                            orderby x.TypeMainTexTure
                                                                                                            select x).ToList();
    if (blockMaintextureRateDataGroupByTypeMainTexTures.Count > 0)
    {
                Table val = document.Tables.Create(array[0].Start, 8, 6, (AutoFitBehaviorType)0);
                // Table table = document.Tables.Create(ranges[0].Start, row_number, 3, AutoFitBehaviorType.AutoFitToContents);
        List<BlockMaintextureRateData> childBlockMaintextureRateDatas = new List<BlockMaintextureRateData>();
        string levelColumn = "";
        string currMainTextureNameColumn = "";
        string remainingRateColumn = "";
        string mainRateColumn = "";
        val.ForEachCell((TableCellProcessorDelegate)delegate (TableCell cell, int i, int j)
        {
            if (i > 0 && i < 7 && j == 0)
            {
                levelColumn = "";
                currMainTextureNameColumn = "";
                remainingRateColumn = "";
                mainRateColumn = "";
                childBlockMaintextureRateDatas = blockMaintextureRateDataGroupByTypeMainTexTures[i - 1].blockMaintextureRateDatas.OrderBy((BlockMaintextureRateData x) => x.LevelBlockId).ToList();
                for (int k = 0; k < childBlockMaintextureRateDatas.Count; k++)
                {
                    BlockMaintextureRateData blockMaintextureRateData = childBlockMaintextureRateDatas[k];
                    currMainTextureNameColumn = ((currMainTextureNameColumn == "") ? blockMaintextureRateData.CurrentStateMainTextureName : string.Join("\n", currMainTextureNameColumn, blockMaintextureRateData.CurrentStateMainTextureName));
                    remainingRateColumn = ((remainingRateColumn == "") ? blockMaintextureRateData.RemainingRate.ToString("N2", culture) : string.Join("\n", remainingRateColumn, blockMaintextureRateData.RemainingRate.ToString("N2", culture)));
                    mainRateColumn = ((mainRateColumn == "") ? blockMaintextureRateData.MainRate.ToString("N2", culture) : string.Join("\n", mainRateColumn, blockMaintextureRateData.MainRate.ToString("N2", culture)));
                    if (k != 0)
                    {
                        BlockMaintextureRateData blockMaintextureRateData2 = childBlockMaintextureRateDatas[k - 1];
                        if (blockMaintextureRateData.LevelBlockId == blockMaintextureRateData2.LevelBlockId)
                        {
                            levelColumn = ((levelColumn == "") ? blockMaintextureRateData.LevelBlockId.ToString() : string.Join("\n", levelColumn, ""));
                        }
                        else
                        {
                            levelColumn = ((levelColumn == "") ? blockMaintextureRateData.LevelBlockId.ToString() : string.Join("\n", levelColumn, blockMaintextureRateData.LevelBlockId.ToString()));
                        }
                    }
                    else
                    {
                        levelColumn = ((levelColumn == "") ? blockMaintextureRateData.LevelBlockId.ToString() : string.Join("\n", levelColumn, blockMaintextureRateData.LevelBlockId.ToString()));
                    }
                }
            }
            switch (j)
            {
                case 0:
                            cell.PreferredWidth = (5f);
                            cell.PreferredWidthType = ((WidthType)2);

                            cell.PreferredWidth = 400;
                            //.  cell.PreferredWidthType = WidthType.Fixed;
                            document.InsertSingleLineText(cell.Range.Start, $"{i + 1}.");
                    if (i == 0)
                    {
                                document.InsertSingleLineText(cell.Range.Start, "STT");
                    }
                    else if (i < 7)
                    {
                                document.InsertSingleLineText(cell.Range.Start, $"{i}");
                    }
                    break;
                case 1:
                            cell.PreferredWidth = (15f);
                            cell.PreferredWidthType = ((WidthType)2);
                    if (i == 0)
                    {
                                document.InsertSingleLineText(cell.Range.Start, "Phần nhà cấp");
                    }
                    else if (i < 7)
                    {
                                document.InsertText(cell.Range.Start, levelColumn);
                    }
                    break;
                case 2:
                            cell.PreferredWidth = (20f);
                            cell.PreferredWidthType = ((WidthType)2);
                    if (i == 0)
                    {
                                document.InsertSingleLineText(cell.Range.Start, "Kết cấu chính");
                    }
                    else if (i < 7)
                    {
                                document.InsertSingleLineText(cell.Range.Start, blockMaintextureRateDataGroupByTypeMainTexTures[i - 1].blockMaintextureRateDatas[0].TypeMainTexTureName ?? "");
                    }
                    break;
                case 3:
                            cell.PreferredWidth = (25f);
                            cell.PreferredWidthType = ((WidthType)2);
                    if (i == 0)
                    {
                                document.InsertSingleLineText(cell.Range.Start, "Hiện trạng");
                    }
                    else if (i < 7)
                    {
                                document.InsertText(cell.Range.Start, currMainTextureNameColumn);
                    }
                    break;
                case 4:
                            cell.PreferredWidth = (15f);
                            cell.PreferredWidthType = ((WidthType)2);
                    if (i == 0)
                    {
                                document.InsertSingleLineText(cell.Range.Start, "Tỷ lệ chất lượng còn lại %");
                    }
                    else if (i < 7)
                    {
                                document.InsertText(cell.Range.Start, remainingRateColumn);
                    }
                    break;
                case 5:
                            cell.PreferredWidth = (20f);
                            cell.PreferredWidthType = ((WidthType)2);
                    if (i == 0)
                    {
                                document.InsertSingleLineText(cell.Range.Start, "Tỷ lệ giá trị của kết cấu chính so với tổng giá trị của nhà ở %");
                    }
                    else if (i < 7)
                    {
                                document.InsertText(cell.Range.Start, mainRateColumn);
                    }
                    break;
            }
                    _ = 7;
            if (i == 0)
            {
                        cell.VerticalAlignment = ((TableCellVerticalAlignment)2);
            }
            else
            {
                        cell.VerticalAlignment = ((TableCellVerticalAlignment)0);
                    }
                });

                //  table.BeginUpdate();
                //table.MergeCells(table[7, 0], table[7, 2]);
                //table[7, 0].PreferredWidth = 50;
                //table[7, 0].PreferredWidthType = WidthType.FiftiethsOfPercent;
                //table[7, 0].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                //table[7, 0].Borders.Top.LineStyle = TableBorderLineStyle.None;
                //table[7, 0].Borders.Right.LineStyle = TableBorderLineStyle.None;
                //table[7, 0].Borders.Left.LineStyle = TableBorderLineStyle.None;
                //document.InsertText(table[7, 0].Range.Start, "Tỷ lệ chất lượng còn lại của nhà này là");
                //table.MergeCells(table[7, 1], table[7, 3]);
                //table[7, 1].PreferredWidth = 50;
                //table[7, 1].PreferredWidthType = WidthType.FiftiethsOfPercent;
                //table[7, 1].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                //table[7, 1].Borders.Top.LineStyle = TableBorderLineStyle.None;
                //table[7, 1].Borders.Right.LineStyle = TableBorderLineStyle.None;
                //table[7, 1].Borders.Left.LineStyle = TableBorderLineStyle.None;
                //  document.InsertText(table[7, 1].Range.Start, str);
        val.BeginUpdate();
                val.BeginUpdate();
                val.MergeCells(val[7, 0], val[7, 2]);
                val[7, 0].PreferredWidth = 50;
                val[7, 0].PreferredWidthType = WidthType.FiftiethsOfPercent;
                val[7, 0].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                val[7, 0].Borders.Top.LineStyle = TableBorderLineStyle.None;
                val[7, 0].Borders.Right.LineStyle = TableBorderLineStyle.None;
                val[7, 0].Borders.Left.LineStyle = TableBorderLineStyle.None;
                document.InsertText(val[7, 0].Range.Start, "Tỷ lệ chất lượng còn lại của nhà này là");
                val.MergeCells(val[7, 1], val[7, 3]);
                val[7, 1].PreferredWidth = 50;
                val[7, 1].PreferredWidthType = WidthType.FiftiethsOfPercent;
                val[7, 1].Borders.Bottom.LineStyle = TableBorderLineStyle.None;
                val[7, 1].Borders.Top.LineStyle = TableBorderLineStyle.None;
                val[7, 1].Borders.Right.LineStyle = TableBorderLineStyle.None;
                val[7, 1].Borders.Left.LineStyle = TableBorderLineStyle.None;
                document.InsertText(val[7, 1].Range.Start, str);
    }
    else
    {
                DocumentRange val2 = document.InsertText(array[0].Start, "\n");
    }
            document.ReplaceAll("<maintextureRateTbla>", "", (SearchOptions)0);
}

private void genBlockMaintextureRateTblb(Document document, string str)
{
    //IL_0061: Unknown result type (might be due to invalid IL or missing references)
    //IL_006b: Expected O, but got Unknown
            DocumentRange[] array = document.FindAll("<maintextureRateTblb>", (SearchOptions)0);
            Table val = document.Tables.Create(array[0].Start, 1, 2, (AutoFitBehaviorType)0);
            document.ReplaceAll("<maintextureRateTblb>", "", (SearchOptions)0);
    val.ForEachCell((TableCellProcessorDelegate)delegate (TableCell cell, int i, int j)
    {
                cell.PreferredWidthType = ((WidthType)2);
                cell.VerticalAlignment = ((TableCellVerticalAlignment)0);
                cell.Borders.Bottom.LineStyle = ((TableBorderLineStyle)0);
                cell.Borders.Top.LineStyle = ((TableBorderLineStyle)0);
                cell.Borders.Right.LineStyle = ((TableBorderLineStyle)0);
                cell.Borders.Left.LineStyle = ((TableBorderLineStyle)0);
                cell.Borders.Bottom.LineStyle = TableBorderLineStyle.None;
        if (j == 0)
        {
                    cell.PreferredWidth = (60f);
                    document.InsertText(cell.Range.Start, "Tỷ lệ chất lượng còn lại của nhà này là");
        }
        else
        {
                    cell.PreferredWidth = (40f);
                    document.InsertText(cell.Range.Start, str);
        }
    });
}

private void genBlockMaintextureRateTblc(Document document, string str)
{
    //IL_0061: Unknown result type (might be due to invalid IL or missing references)
    //IL_006b: Expected O, but got Unknown
            DocumentRange[] array = document.FindAll("<maintextureRateTblc>", (SearchOptions)0);
            Table val = document.Tables.Create(array[0].Start, 1, 1, (AutoFitBehaviorType)0);

            document.ReplaceAll("<maintextureRateTblc>", "", (SearchOptions)0);
    val.ForEachCell((TableCellProcessorDelegate)delegate (TableCell cell, int i, int j)
    {
                cell.PreferredWidthType = ((WidthType)2);
                cell.VerticalAlignment = ((TableCellVerticalAlignment)2);
                cell.Borders.Bottom.LineStyle = ((TableBorderLineStyle)0);
                cell.Borders.Top.LineStyle = ((TableBorderLineStyle)0);
                cell.Borders.Right.LineStyle = ((TableBorderLineStyle)0);
                cell.Borders.Left.LineStyle = ((TableBorderLineStyle)0);
                document.InsertText(cell.Range.Start, str);
    });
}

private void genPricingLand(Document document, DateTime? dateCreate, List<PricingLandTblData> pricingLandTblDatas, List<ConstructionPrice> constructionPrices, float? vat, decimal? price, AppEnums.TypeReportApply typeReportApply, int? AreaCorrectionCoefficientId, AppEnums.TypeReportApply? parentTypeReportApply)
{
    string listPrice = "";
    string RemainingPrice = "";
    string PriceInYear = "";
    string MaintextureRateValue = "";
    string MaintextureRateValueText = "";
    if (typeReportApply == AppEnums.TypeReportApply.NHA_CHUNG_CU || (typeReportApply == AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG && parentTypeReportApply.GetValueOrDefault() == AppEnums.TypeReportApply.NHA_CHUNG_CU))
    {
        List<IGrouping<bool?, PricingLandTblData>> list = (from x in pricingLandTblDatas
                                                           group x by x.ApplyInvestmentRate into x
                                                           orderby x.Key descending
                                                           select x).ToList();
        list.ForEach(delegate (IGrouping<bool?, PricingLandTblData> groupPricingLandTblDatasByApplyInvestmentRate)
        {
            string text4 = (groupPricingLandTblDatasByApplyInvestmentRate.Key.GetValueOrDefault() ? "Trường hợp các phần diện tích đất áp dụng Suất vốn đầu tư\n" : "Trường hợp các phần diện tích đất không áp dụng Suất vốn đầu tư\n");
            List<IGrouping<int?, PricingLandTblData>> list4 = (from x in groupPricingLandTblDatasByApplyInvestmentRate.ToList()
                                                               group x by x.Level into x
                                                               orderby x.Key
                                                               select x).ToList();
            list4.ForEach(delegate (IGrouping<int?, PricingLandTblData> groupPricingLandTblDatasBylevelItem)
            {
                List<PricingLandTblData> list5 = groupPricingLandTblDatasBylevelItem.ToList();
                list5.ForEach(delegate (PricingLandTblData childPricingLandTblData)
                {
                    listPrice = ((listPrice == "") ? (UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice) ?? "") : (listPrice + " + " + UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice)));
                    if (childPricingLandTblData.ApplyInvestmentRate.GetValueOrDefault())
                    {
                        RemainingPrice = RemainingPrice + UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice) + "\n";
                        PriceInYear = PriceInYear + UtilsService.FormatMoney(childPricingLandTblData.PriceInYear) + "\n";
                                MaintextureRateValue += $"{childPricingLandTblData.MaintextureRateValue}\n";
                        MaintextureRateValueText = MaintextureRateValueText + UtilsService.ConvertPercentToString(childPricingLandTblData.MaintextureRateValue.ToString()) + "\n";
                        string text5 = "";
                        InvestmentRateItem investmentRateItem = ((IQueryable<InvestmentRateItem>)_context.InvestmentRateItems).Where((InvestmentRateItem i) => (int?)i.Id == childPricingLandTblData.InvestmentRateItemId).FirstOrDefault();
                        if (investmentRateItem != null)
                        {
                            InvestmentRate investmentRate = ((IQueryable<InvestmentRate>)_context.InvestmentRaties).Where((InvestmentRate i) => i.Id == investmentRateItem.Id).FirstOrDefault();
                            if (investmentRate != null)
                            {
                                text5 = "\nCăn cứ dòng số " + investmentRateItem.LineInfo + " chi tiết suất vốn đầu tư " + investmentRateItem.DetailInfo + ". Suất vốn đầu tư: " + UtilsService.FormatMoney((decimal)investmentRateItem.Value) + " đồng/m2, Chi phí xây dựng: " + UtilsService.FormatMoney((decimal)investmentRateItem.Value1) + " đồng/m2, Chi phí thiết bị: " + UtilsService.FormatMoney((decimal)investmentRateItem.Value2) + " đồng/m2";
                            }
                        }
                        AreaCorrectionCoefficient areaCorrectionCoefficient = ((IQueryable<AreaCorrectionCoefficient>)_context.AreaCorrectionCoefficients).Where((AreaCorrectionCoefficient a) => (int?)a.Id == AreaCorrectionCoefficientId).FirstOrDefault();
                        if (areaCorrectionCoefficient != null)
                        {
                            string text6 = areaCorrectionCoefficient.Note + "\nHệ số điều chỉnh vùng: " + areaCorrectionCoefficient.Value.ToString("N3", culture);
                        }
                    }
                    else
                    {
                        RemainingPrice = RemainingPrice + UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice) + "\n";
                        PriceInYear = PriceInYear + UtilsService.FormatMoney(childPricingLandTblData.PriceInYear) + "\n";
                                MaintextureRateValue += $"{childPricingLandTblData.MaintextureRateValue}\n";
                        MaintextureRateValueText = MaintextureRateValueText + UtilsService.ConvertPercentToString(childPricingLandTblData.MaintextureRateValue.ToString()) + "\n";
                    }
                    string text7 = "";
                    float num2 = childPricingLandTblData.GeneralArea.GetValueOrDefault() + childPricingLandTblData.PrivateArea.GetValueOrDefault();
                    string text8 = (childPricingLandTblData.CoefficientUseValue.HasValue ? childPricingLandTblData.CoefficientUseValue.Value.ToString("N2", culture) : "");
                });
            });
        });
    }
    else
    {
        List<IGrouping<int?, PricingLandTblData>> list2 = (from x in pricingLandTblDatas
                                                           group x by x.Level into x
                                                           orderby x.Key
                                                           select x).ToList();
        list2.ForEach(delegate (IGrouping<int?, PricingLandTblData> groupPricingLandTblDatasBylevelItem)
        {
            List<PricingLandTblData> list3 = groupPricingLandTblDatasBylevelItem.ToList();
            list3.ForEach(delegate (PricingLandTblData childPricingLandTblData)
            {
                listPrice = ((listPrice == "") ? (UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice) ?? "") : (listPrice + " + " + UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice)));
                string text = (childPricingLandTblData.MaintextureRateValue.HasValue ? childPricingLandTblData.MaintextureRateValue.Value.ToString("N2", culture) : "");
                string text2 = "";
                float num = childPricingLandTblData.GeneralArea.GetValueOrDefault() + childPricingLandTblData.PrivateArea.GetValueOrDefault();
                string text3 = (childPricingLandTblData.CoefficientUseValue.HasValue ? childPricingLandTblData.CoefficientUseValue.Value.ToString("N2", culture) : "");
                if (childPricingLandTblData.TermApply.GetValueOrDefault() == AppEnums.TermApply.DIEU_65 || childPricingLandTblData.TermApply.GetValueOrDefault() == AppEnums.TermApply.KHOAN_1_DIEU_34 || childPricingLandTblData.TermApply.GetValueOrDefault() == AppEnums.TermApply.DIEU_7)
                {
                    text2 = "\t" + UtilsService.FormatMoney(childPricingLandTblData.PriceInYear) + " đồng/m2 x " + text + "% x " + num.ToString("N2", culture) + " m2 x " + text3 + " = " + UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice) + "\n";
                }
                else if (childPricingLandTblData.TermApply.GetValueOrDefault() == AppEnums.TermApply.DIEU_70 || childPricingLandTblData.TermApply.GetValueOrDefault() == AppEnums.TermApply.KHOAN_2_DIEU_34 || childPricingLandTblData.TermApply.GetValueOrDefault() == AppEnums.TermApply.DIEU_35 || childPricingLandTblData.TermApply.GetValueOrDefault() == AppEnums.TermApply.DIEU_71)
                {
                    text2 = "\t" + UtilsService.FormatMoney(childPricingLandTblData.PriceInYear) + " đồng/m2 x " + text + "% x " + num.ToString("N2", culture) + " m2 = " + UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice) + "\n";
                }
                RemainingPrice = RemainingPrice + UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice) + "\n";
                PriceInYear = PriceInYear + UtilsService.FormatMoney(childPricingLandTblData.PriceInYear) + "\n";
                        MaintextureRateValue += $"{childPricingLandTblData.MaintextureRateValue}\n";
                MaintextureRateValueText = MaintextureRateValueText + UtilsService.ConvertPercentToString(childPricingLandTblData.MaintextureRateValue.ToString()) + "\n";
            });
        });
    }
            document.ReplaceAll("<RemainingPrice>", RemainingPrice ?? "", (SearchOptions)0);
            document.ReplaceAll("<PriceInYear>", PriceInYear ?? "", (SearchOptions)0);
            document.ReplaceAll("<MaintextureRateValue>", MaintextureRateValue ?? "", (SearchOptions)0);
            document.ReplaceAll("<MaintextureRateValueText>", MaintextureRateValueText ?? "", (SearchOptions)0);
}

private void genApartmentLandDetailInfo(Document document, List<PricingApartmentLandDetailGroupByDecree> pricingApartmentLandDetailGroupByDecrees, Block block, Pricing pricing, List<PricingLandTbl> pricingLandTbls, List<BlockDetail> blockDetails, float? landscapeAreaValue1)
{
    string listLandPrice = "";
    pricingApartmentLandDetailGroupByDecrees.ForEach(delegate (PricingApartmentLandDetailGroupByDecree pricingApartmentLandDetailsGroupByDecree)
    {
        if (pricingApartmentLandDetailsGroupByDecree.Decree.GetValueOrDefault() != AppEnums.DecreeEnum.ND_CP_61 || !block.IsFrontOfLine_61.GetValueOrDefault())
        {
            string text = "";
            switch (pricingApartmentLandDetailsGroupByDecree.Decree)
            {
                case AppEnums.DecreeEnum.ND_CP_99_2015:
                    text = getLocationResidentialLand(block.LandscapeLocation_99);
                    break;
                case AppEnums.DecreeEnum.ND_CP_34_2013:
                    text = ((block.CaseApply_34.GetValueOrDefault() == AppEnums.TypeCaseApply_34.KHOAN_1) ? getLocationResidentialLand(block.LandscapeLocationInAlley_34) : ((block.CaseApply_34.GetValueOrDefault() == AppEnums.TypeCaseApply_34.KHOAN_2) ? getLocationResidentialLand(block.LandscapeLocation_34) : ""));
                    break;
                case AppEnums.DecreeEnum.ND_CP_61:
                    text = getLocationResidentialLand(block.LandscapeLocationInAlley_61);
                    break;
            }
        }
        int num = pricingApartmentLandDetailsGroupByDecree.LandScapePrice.IndexOf('x');
        int num2 = pricingApartmentLandDetailsGroupByDecree.LandScapePrice.IndexOf('=', num);
        if (num != -1 && num2 != -1)
        {
            string text2 = pricingApartmentLandDetailsGroupByDecree.LandScapePrice.Substring(num + 1, num2 - num - 1);
                    document.ReplaceAll("<PositionCoefficient>", text2, (SearchOptions)0);
        }
        List<PricingApartmentLandDetail> pricingApartmentLandDetails = pricingApartmentLandDetailsGroupByDecree.pricingApartmentLandDetails;
        pricingApartmentLandDetails.ForEach(delegate (PricingApartmentLandDetail apricingApartmentLandDetail)
        {
            string text3 = UtilsService.FormatMoney(apricingApartmentLandDetail.LandUnitPrice) ?? "";
            if (apricingApartmentLandDetail.TermApply.GetValueOrDefault() == AppEnums.TermApply.DIEU_65 || apricingApartmentLandDetail.TermApply.GetValueOrDefault() == AppEnums.TermApply.KHOAN_1_DIEU_34 || apricingApartmentLandDetail.TermApply.GetValueOrDefault() == AppEnums.TermApply.DIEU_7)
            {
                if (block.TypeReportApply == AppEnums.TypeReportApply.NHA_HO_CHUNG)
                {
                    text3 = text3 + " x (" + (apricingApartmentLandDetail.PrivateArea.HasValue ? apricingApartmentLandDetail.PrivateArea.Value.ToString("N2", culture) : "0") + " x 40% + " + (apricingApartmentLandDetail.ConversionArea.HasValue ? apricingApartmentLandDetail.ConversionArea.Value.ToString("N2", culture) : "0") + " x 10% + " + (apricingApartmentLandDetail.GeneralArea.HasValue ? apricingApartmentLandDetail.GeneralArea.Value.ToString("N2", culture) : "0") + " x 40%)";
                }
                else if (block.TypeReportApply == AppEnums.TypeReportApply.NHA_RIENG_LE)
                {
                    text3 = text3 + " x (" + (apricingApartmentLandDetail.InLimitArea.HasValue ? apricingApartmentLandDetail.InLimitArea.Value.ToString("N2", culture) : "0") + " x " + (apricingApartmentLandDetail.InLimitPercent.HasValue ? apricingApartmentLandDetail.InLimitPercent.Value.ToString("N2", culture) : "0") + "% + " + (apricingApartmentLandDetail.OutLimitArea.HasValue ? apricingApartmentLandDetail.OutLimitArea.Value.ToString("N2", culture) : "0") + " x " + (apricingApartmentLandDetail.OutLimitPercent.HasValue ? apricingApartmentLandDetail.OutLimitPercent.Value.ToString("N2", culture) : "0") + "%)";
                }
                else
                {
                    text3 = text3 + " x " + (apricingApartmentLandDetail.PrivateArea.HasValue ? apricingApartmentLandDetail.PrivateArea.Value.ToString("N2", culture) : "0");
                    if (apricingApartmentLandDetail.CoefficientDistribution.HasValue)
                    {
                        text3 = text3 + " x " + apricingApartmentLandDetail.CoefficientDistribution.Value.ToString("N2", culture);
                    }
                    text3 += " x 0.1";
                    float? num3 = null;
                    if (pricingApartmentLandDetailsGroupByDecree.Decree.GetValueOrDefault() == AppEnums.DecreeEnum.ND_CP_99_2015)
                    {
                        num3 = pricing.FlatCoefficient_99;
                    }
                    else if (pricingApartmentLandDetailsGroupByDecree.Decree.GetValueOrDefault() == AppEnums.DecreeEnum.ND_CP_34_2013)
                    {
                        num3 = pricing.FlatCoefficient_34;
                    }
                    else if (pricingApartmentLandDetailsGroupByDecree.Decree.GetValueOrDefault() == AppEnums.DecreeEnum.ND_CP_61)
                    {
                        num3 = pricing.FlatCoefficient_61;
                    }
                    if (num3.HasValue)
                    {
                        text3 = text3 + " x " + (num3.HasValue ? num3.Value.ToString("N2", culture) : "");
                    }
                }
                if (apricingApartmentLandDetail.DeductionLandMoneyValue.HasValue)
                {
                    text3 = text3 + " x (100% - " + apricingApartmentLandDetail.DeductionLandMoneyValue.Value.ToString("N2", culture) + "%)";
                }
            }
            else if (apricingApartmentLandDetail.TermApply.GetValueOrDefault() == AppEnums.TermApply.DIEU_70 || apricingApartmentLandDetail.TermApply.GetValueOrDefault() == AppEnums.TermApply.KHOAN_2_DIEU_34)
            {
                if (block.TypeReportApply == AppEnums.TypeReportApply.NHA_HO_CHUNG)
                {
                    text3 = text3 + " x (" + (apricingApartmentLandDetail.PrivateArea.HasValue ? apricingApartmentLandDetail.PrivateArea.Value.ToString("N2", culture) : "0") + " + " + (apricingApartmentLandDetail.ConversionArea.HasValue ? apricingApartmentLandDetail.ConversionArea.Value.ToString("N2", culture) : "0") + " + " + (apricingApartmentLandDetail.GeneralArea.HasValue ? apricingApartmentLandDetail.GeneralArea.Value.ToString("N2", culture) : "0") + ") x 100%";
                }
                else if (block.TypeReportApply == AppEnums.TypeReportApply.NHA_RIENG_LE)
                {
                    text3 = text3 + " x (" + (apricingApartmentLandDetail.InLimitArea.HasValue ? apricingApartmentLandDetail.InLimitArea.Value.ToString("N2", culture) : "0") + " + " + (apricingApartmentLandDetail.OutLimitArea.HasValue ? apricingApartmentLandDetail.OutLimitArea.Value.ToString("N2", culture) : "0") + ") x 100%";
                }
                else
                {
                    text3 = text3 + " x " + (apricingApartmentLandDetail.PrivateArea.HasValue ? apricingApartmentLandDetail.PrivateArea.Value.ToString("N2", culture) : "0");
                    if (apricingApartmentLandDetail.CoefficientDistribution.HasValue)
                    {
                        text3 = text3 + " x " + apricingApartmentLandDetail.CoefficientDistribution.Value.ToString("N2", culture);
                    }
                    float? num4 = null;
                    if (pricingApartmentLandDetailsGroupByDecree.Decree.GetValueOrDefault() == AppEnums.DecreeEnum.ND_CP_99_2015)
                    {
                        num4 = pricing.FlatCoefficient_99;
                    }
                    else if (pricingApartmentLandDetailsGroupByDecree.Decree.GetValueOrDefault() == AppEnums.DecreeEnum.ND_CP_34_2013)
                    {
                        num4 = pricing.FlatCoefficient_34;
                    }
                    else if (pricingApartmentLandDetailsGroupByDecree.Decree.GetValueOrDefault() == AppEnums.DecreeEnum.ND_CP_61)
                    {
                        num4 = pricing.FlatCoefficient_61;
                    }
                    if (num4.HasValue)
                    {
                        text3 = text3 + " x " + num4.Value.ToString("N2", culture);
                    }
                    text3 += " x 100%";
                }
            }
            else if ((apricingApartmentLandDetail.TermApply.GetValueOrDefault() == AppEnums.TermApply.DIEU_71 || apricingApartmentLandDetail.TermApply.GetValueOrDefault() == AppEnums.TermApply.DIEU_35) && block.TypeReportApply == AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG)
            {
                text3 = text3 + " x " + (apricingApartmentLandDetail.PrivateArea.HasValue ? apricingApartmentLandDetail.PrivateArea.Value.ToString("N2", culture) : "0") + " x 100%";
            }
            text3 = text3 + " = " + UtilsService.FormatMoney(apricingApartmentLandDetail.LandPrice) + " đồng";
                    document.ReplaceAll("<LandPrice>", UtilsService.FormatMoney(apricingApartmentLandDetail.LandUnitPrice), (SearchOptions)0);
                    document.ReplaceAll("<LandPriceTotal>", UtilsService.FormatMoney(apricingApartmentLandDetail.LandPrice), (SearchOptions)0);
            listLandPrice = ((listLandPrice == "") ? UtilsService.FormatMoney(apricingApartmentLandDetail.LandPrice) : string.Join(" + ", listLandPrice, UtilsService.FormatMoney(apricingApartmentLandDetail.LandPrice)));
        });
    });
            document.ReplaceAll("<listLandPrice>", listLandPrice, (SearchOptions)0);
            document.ReplaceAll("<landPrice>", UtilsService.FormatMoney(pricing.LandPrice), (SearchOptions)0);
            document.ReplaceAll("<totalPrice>", UtilsService.FormatMoney(pricing.TotalPrice), (SearchOptions)0);
            document.ReplaceAll("<Total>", UtilsService.FormatMoney(pricing.TotalPrice), (SearchOptions)0);
        }

[HttpPost("GetExportWordPL2/{id}")]
        public async Task<IActionResult> GetExportWordPL2(int id)
        {
            string accessToken = base.Request.Headers[HeaderNames.Authorization];
            Token token = ((IQueryable<Token>)_context.Tokens).Where((Token t) => Convert.ToString(t.AccessToken) == accessToken).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }
            DefaultResponse defaultResponse = new DefaultResponse();
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)base.User.Identity;
            string access_key = (from c in claimsIdentity.Claims
                                 where c.Type == "AccessKey"
                                 select c.Value).SingleOrDefault();
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, 5))
            {
                defaultResponse.meta = new Meta(222, "Bạn không có quyền xem dữ liệu tới mục này!");
                return Ok(defaultResponse);
            }
            try
            {
                string text = Path.Combine(_hostingEnvironment.WebRootPath, "exportword/phu_luc_02.docx");
                if (text != null)
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        new HttpResponseMessage();
                        string fileDownloadName = "bien_ban-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";
                        using (FileStream fileStream = new FileStream(text, FileMode.Open, FileAccess.Read))
                        {
                            fileStream.CopyTo(memoryStream);
                        }
                        return File(memoryStream.ToArray(), "application/octet-stream", fileDownloadName);
                    }
                }
                defaultResponse.meta = new Meta(215, "Data file null!");
                return Ok(defaultResponse);
            }
            catch (Exception ex)
            {
                log.Error((object)("GetExportWordExample:" + ex.Message));
                throw;
            }
        }

[HttpPost("GetExportWordPL3/{id}")]
        public async Task<IActionResult> GetExportWordPL3(int id)
        {
            string accessToken = base.Request.Headers[HeaderNames.Authorization];
            Token token = ((IQueryable<Token>)_context.Tokens).Where((Token t) => Convert.ToString(t.AccessToken) == accessToken).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }
            DefaultResponse defaultResponse = new DefaultResponse();
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)base.User.Identity;
            string access_key = (from c in claimsIdentity.Claims
                                 where c.Type == "AccessKey"
                                 select c.Value).SingleOrDefault();
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, 5))
            {
                defaultResponse.meta = new Meta(222, "Bạn không có quyền xem dữ liệu tới mục này!");
                return Ok(defaultResponse);
            }
            try
            {
                string text = insertDataToTemplatePL3(id);
                if (text != null)
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        new HttpResponseMessage();
                        string fileDownloadName = "bien_ban-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";
                        using (FileStream fileStream = new FileStream(text, FileMode.Open, FileAccess.Read))
                        {
                            fileStream.CopyTo(memoryStream);
                        }
                        try
                        {
                            System.IO.File.Delete(text);
                        }
                        catch
                        {
                        }
                        return File(memoryStream.ToArray(), "application/octet-stream", fileDownloadName);
                    }
                }
                defaultResponse.meta = new Meta(215, "Data file null!");
                return Ok(defaultResponse);
            }
            catch (Exception ex)
            {
                log.Error((object)("GetExportWordExample:" + ex.Message));
                throw;
            }
}

private string insertDataToTemplatePL3(int id)
{
    //IL_035f: Unknown result type (might be due to invalid IL or missing references)
    //IL_0366: Expected O, but got Unknown
    //IL_036a: Unknown result type (might be due to invalid IL or missing references)
    //IL_2d41: Unknown result type (might be due to invalid IL or missing references)
    Pricing pricing = ((IQueryable<Pricing>)_context.Pricings).Where((Pricing p) => p.Id == id && (int)p.Status != 99).FirstOrDefault();
    if (pricing != null)
    {
        Block block = ((IQueryable<Block>)_context.Blocks).Where((Block b) => b.Id == pricing.BlockId && (int)b.Status != 99).FirstOrDefault();
        Apartment apartment = ((IQueryable<Apartment>)_context.Apartments).Where((Apartment a) => a.Id == pricing.ApartmentId && (int)a.Status != 99).FirstOrDefault();
        if (block != null)
        {
            if ((block.TypeReportApply == AppEnums.TypeReportApply.NHA_HO_CHUNG || block.TypeReportApply == AppEnums.TypeReportApply.NHA_CHUNG_CU || block.TypeReportApply == AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG) && apartment == null)
            {
                return null;
            }
            string text = Path.Combine(_hostingEnvironment.WebRootPath, "exportword/phu_luc_03.docx");
            string text2 = "result" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";
            RichEditDocumentServer val = new RichEditDocumentServer();
            val.LoadDocumentTemplate(text, DocumentFormat.OpenXml);
                    Document document = val.Document;
                    document.BeginUpdate();
            DateTime now = DateTime.Now;
                    document.ReplaceAll("<day>", now.Day.ToString(), (SearchOptions)0);
                    document.ReplaceAll("<month>", now.Month.ToString(), (SearchOptions)0);
                    document.ReplaceAll("<year>", now.Year.ToString(), (SearchOptions)0);
                    document.ReplaceAll("<dayOfWeek>", GetVietnameseDayOfWeek(now.DayOfWeek), (SearchOptions)0);
            string text3 = block.Address + ", ";
            Lane lane = ((IQueryable<Lane>)_context.Lanies).Where((Lane l) => (int?)l.Id == block.Lane && (int)l.Status != 99).FirstOrDefault();
            if (lane != null)
            {
                text3 = text3 + lane.Name + ", ";
            }
            Ward ward = ((IQueryable<Ward>)_context.Wards).Where((Ward l) => l.Id == block.Ward && (int)l.Status != 99).FirstOrDefault();
            if (ward != null)
            {
                text3 = text3 + ward.Name + ", ";
            }
            District district = ((IQueryable<District>)_context.Districts).Where((District l) => l.Id == block.District && (int)l.Status != 99).FirstOrDefault();
            if (district != null)
            {
                text3 += district.Name;
            }
                    document.ReplaceAll("<address>", text3, (SearchOptions)0);
            string typeBlock = getTypeBlock(block.TypeBlockId);
            typeBlock = typeBlock + ", số tầng: " + block.FloorBlockMap;
                    document.ReplaceAll("<typeBlock>", typeBlock, (SearchOptions)0);
            List<LevelBlockMap> levelBlockMaps = ((IQueryable<LevelBlockMap>)_context.LevelBlockMaps).Where((LevelBlockMap l) => l.BlockId == block.Id && (int)l.Status != 99).ToList();
            string levelBlock = getLevelBlock(levelBlockMaps);
                    document.ReplaceAll("<levelBlock>", levelBlock, (SearchOptions)0);
            List<BlockDetail> list = ((IQueryable<BlockDetail>)_context.BlockDetails).Where((BlockDetail l) => l.BlockId == block.Id && (int)l.Status != 99).ToList();
            if (block.TypeReportApply == AppEnums.TypeReportApply.NHA_HO_CHUNG || block.TypeReportApply == AppEnums.TypeReportApply.NHA_CHUNG_CU || block.TypeReportApply == AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG)
            {
                if (block.TypeReportApply == AppEnums.TypeReportApply.NHA_HO_CHUNG)
                {
                    List<BlockDetailData> list2 = ((IMapperBase)_mapper).Map<List<BlockDetailData>>((object)list);
                    foreach (BlockDetailData map_BlockDetail in list2)
                    {
                        Floor floor = ((IQueryable<Floor>)_context.Floors).Where((Floor f) => f.Id == map_BlockDetail.FloorId && (int)f.Status != 99).FirstOrDefault();
                        map_BlockDetail.FloorName = ((floor != null) ? floor.Name : "");
                                map_BlockDetail.FloorCode = floor?.Code ?? 0;
                        Area area = ((IQueryable<Area>)_context.Areas).Where((Area f) => f.Id == map_BlockDetail.AreaId && (int)f.Status != 99).FirstOrDefault();
                        map_BlockDetail.AreaName = ((area != null) ? area.Name : "");
                                map_BlockDetail.IsMezzanine = area?.IsMezzanine;
                    }
                    list2 = (from x in list2
                             orderby x.FloorCode, x.IsMezzanine
                             select x).ToList();
                }
                List<ApartmentDetail> list3 = ((IQueryable<ApartmentDetail>)_context.ApartmentDetails).Where((ApartmentDetail a) => a.TargetId == apartment.Id && (int)a.Type == 2 && (int)a.Status != 99).ToList();
                List<ApartmentDetailData> list4 = ((IMapperBase)_mapper).Map<List<ApartmentDetailData>>((object)list3);
                foreach (ApartmentDetailData map_ApartmentDetail2 in list4)
                {
                    Floor floor2 = ((IQueryable<Floor>)_context.Floors).Where((Floor f) => f.Id == map_ApartmentDetail2.FloorId && (int)f.Status != 99).FirstOrDefault();
                    map_ApartmentDetail2.FloorName = ((floor2 != null) ? floor2.Name : "");
                            map_ApartmentDetail2.FloorCode = floor2?.Code ?? 0;
                    Area area2 = ((IQueryable<Area>)_context.Areas).Where((Area f) => f.Id == map_ApartmentDetail2.AreaId && (int)f.Status != 99).FirstOrDefault();
                    map_ApartmentDetail2.AreaName = ((area2 != null) ? area2.Name : "");
                            map_ApartmentDetail2.IsMezzanine = area2?.IsMezzanine;
                }
                list4 = (from x in list4
                         orderby x.Level, x.FloorCode, x.IsMezzanine
                         select x).ToList();
                if (block.TypeReportApply == AppEnums.TypeReportApply.NHA_CHUNG_CU || block.TypeReportApply == AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG || block.ParentTypeReportApply.GetValueOrDefault() == AppEnums.TypeReportApply.NHA_CHUNG_CU)
                {
                    string levelApartment = getLevelApartment(list4);
                            document.ReplaceAll("<levelApartment>", levelApartment, (SearchOptions)0);
                }
                string apartDetailInfo = getApartDetailInfo(list4);
                        document.ReplaceAll("<apartDetailInfo>", apartDetailInfo, (SearchOptions)0);
                string text4 = (apartment.ConstructionAreaValue.HasValue ? apartment.ConstructionAreaValue.Value.ToString("N2", culture) : "0");
                        document.ReplaceAll("<acav>", text4, (SearchOptions)0);
                        document.ReplaceAll("<acaNote>", (apartment.ConstructionAreaNote != null && apartment.ConstructionAreaNote != "") ? ("(" + apartment.ConstructionAreaNote + ")") : "", (SearchOptions)0);
                string text5 = (apartment.ConstructionAreaValue1.HasValue ? apartment.ConstructionAreaValue1.Value.ToString("N2", culture) : "0");
                        document.ReplaceAll("<acav1>", text5, (SearchOptions)0);
                string text6 = (apartment.ConstructionAreaValue2.HasValue ? apartment.ConstructionAreaValue2.Value.ToString("N2", culture) : "0");
                        document.ReplaceAll("<acav2>", text6, (SearchOptions)0);
                string text7 = (apartment.ConstructionAreaValue3.HasValue ? apartment.ConstructionAreaValue3.Value.ToString("N2", culture) : "0");
                        document.ReplaceAll("<acav3>", text7, (SearchOptions)0);
                string text8 = (apartment.UseAreaValue.HasValue ? apartment.UseAreaValue.Value.ToString("N2", culture) : "0");
                        document.ReplaceAll("<auav>", text8, (SearchOptions)0);
                string text9 = (apartment.UseAreaValue1.HasValue ? apartment.UseAreaValue1.Value.ToString("N2", culture) : "0");
                        document.ReplaceAll("<auav1>", text9, (SearchOptions)0);
                string text10 = (apartment.UseAreaValue2.HasValue ? apartment.UseAreaValue2.Value.ToString("N2", culture) : "0");
                        document.ReplaceAll("<auav2>", text10, (SearchOptions)0);
            }
            else
            {
                List<ApartmentDetail> list5 = ((IQueryable<ApartmentDetail>)_context.ApartmentDetails).Where((ApartmentDetail a) => a.TargetId == block.Id && (int)a.Type == 1 && (int)a.Status != 99).ToList();
                List<ApartmentDetailData> list6 = ((IMapperBase)_mapper).Map<List<ApartmentDetailData>>((object)list5);
                foreach (ApartmentDetailData map_ApartmentDetail in list6)
                {
                    Floor floor3 = ((IQueryable<Floor>)_context.Floors).Where((Floor f) => f.Id == map_ApartmentDetail.FloorId && (int)f.Status != 99).FirstOrDefault();
                    map_ApartmentDetail.FloorName = ((floor3 != null) ? floor3.Name : "");
                            map_ApartmentDetail.FloorCode = floor3?.Code ?? 0;
                    Area area3 = ((IQueryable<Area>)_context.Areas).Where((Area f) => f.Id == map_ApartmentDetail.AreaId && (int)f.Status != 99).FirstOrDefault();
                    map_ApartmentDetail.AreaName = ((area3 != null) ? area3.Name : "");
                            map_ApartmentDetail.IsMezzanine = area3?.IsMezzanine;
                }
                list6 = (from x in list6
                         orderby x.Level, x.FloorCode, x.IsMezzanine
                         select x).ToList();
            }
            List<BlockMaintextureRateData> list7 = (from b in (IQueryable<BlockMaintextureRate>)_context.BlockMaintextureRaties
                                                    where b.TargetId == block.Id && (int)b.Type == 1 && (int)b.Status != 99
                                                    select b into e
                                                    select new BlockMaintextureRateData
                                                    {
                                                        LevelBlockId = e.LevelBlockId,
                                                        RatioMainTextureId = e.RatioMainTextureId,
                                                        TypeMainTexTure = e.TypeMainTexTure,
                                                        CurrentStateMainTextureId = e.CurrentStateMainTextureId,
                                                        RemainingRate = e.RemainingRate,
                                                        MainRate = e.MainRate,
                                                        TotalValue = e.TotalValue,
                                                        TotalValue1 = e.TotalValue1,
                                                        TotalValue2 = e.TotalValue2,
                                                        CurrentStateMainTextureName = ((IQueryable<CurrentStateMainTexture>)_context.CurrentStateMainTexturies).Where((CurrentStateMainTexture c) => c.Id == e.CurrentStateMainTextureId).FirstOrDefault().Name,
                                                        TypeMainTexTureName = getTypeMainTexTureName(e.TypeMainTexTure)
                                                    }).ToList();
            List<IGrouping<int, BlockMaintextureRateData>> list8 = (from x in list7
                                                                    group x by x.LevelBlockId into x
                                                                    orderby x.Key
                                                                    select x).ToList();
            string dataStr = "";
            string dataStr2 = "";
            string dataStr3 = "";
            list8.ForEach(delegate (IGrouping<int, BlockMaintextureRateData> item)
            {
                string arg = (item.FirstOrDefault().TotalValue.HasValue ? item.FirstOrDefault().TotalValue.Value.ToString("N2", culture) : "");
                string arg2 = (item.FirstOrDefault().TotalValue1.HasValue ? item.FirstOrDefault().TotalValue1.Value.ToString("N2", culture) : "");
                string arg3 = (item.FirstOrDefault().TotalValue2.HasValue ? item.FirstOrDefault().TotalValue2.Value.ToString("N2", culture) : "");
                        dataStr = ((dataStr == "") ? $"- Phần nhà cấp {item.Key}    =     {item.FirstOrDefault().TotalValue} %" : string.Join("\n", dataStr, $"- Phần nhà cấp {item.Key}    =     {arg} %"));
                        dataStr2 = ((dataStr2 == "") ? $"* Phần nhà cấp {item.Key}    =     {item.FirstOrDefault().TotalValue1} %" : string.Join("\n", dataStr2, $"* Phần nhà cấp {item.Key}    =     {arg2} %"));
                        dataStr3 = ((dataStr3 == "") ? $"* Phần nhà cấp {item.Key}    =     {item.FirstOrDefault().TotalValue2} %" : string.Join("\n", dataStr3, $"* Phần nhà cấp {item.Key}    =     {arg3} %"));
            });
            genBlockMaintextureRateTblaForTem3(document, list7, dataStr2);
            List<PricingLandTbl> list9 = ((IQueryable<PricingLandTbl>)_context.PricingLandTbls).Where((PricingLandTbl a) => a.PricingId == pricing.Id && (int)a.Status != 99).ToList();
            List<PricingLandTblData> list10 = ((IMapperBase)_mapper).Map<List<PricingLandTblData>>((object)list9);
            list10.ForEach(delegate (PricingLandTblData pricingLandTblData)
            {
                Area area4 = ((IQueryable<Area>)_context.Areas).Where((Area a) => a.Id == pricingLandTblData.AreaId && (int)a.Status != 99).FirstOrDefault();
                pricingLandTblData.AreaName = ((area4 != null) ? area4.Name : "");
                PriceListItem priceListItem = ((IQueryable<PriceListItem>)_context.PriceListItems).Where((PriceListItem p) => (int?)p.Id == pricingLandTblData.PriceListItemId).FirstOrDefault();
                if (priceListItem != null)
                {
                    PriceList priceList = ((IQueryable<PriceList>)_context.PriceLists).Where((PriceList p) => p.Id == priceListItem.PriceListId).FirstOrDefault();
                            pricingLandTblData.PriceListItemNote = "Căn cứ " + priceListItem.NameOfConstruction + ((priceList != null && priceList.Des != null) ? (" theo " + priceList?.Des) : "");
                }
                if (block.TypeReportApply == AppEnums.TypeReportApply.NHA_CHUNG_CU || (block.TypeReportApply == AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG && block.ParentTypeReportApply.GetValueOrDefault() == AppEnums.TypeReportApply.NHA_CHUNG_CU))
                {
                    if (pricingLandTblData.DecreeType1Id.GetValueOrDefault() == AppEnums.DecreeEnum.ND_CP_99_2015 && pricingLandTblData.ApplyInvestmentRate.GetValueOrDefault() && !pricingLandTblData.IsMezzanine.GetValueOrDefault())
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
            List<ConstructionPrice> constructionPrices = (from pcp in (IQueryable<PricingConstructionPrice>)_context.PricingConstructionPricies
                                                          join cp in (IEnumerable<ConstructionPrice>)_context.ConstructionPricies on pcp.ConstructionPriceId equals cp.Id
                                                          where (int)pcp.Status != 99 && (int)cp.Status != 99 && pcp.PricingId == pricing.Id
                                                          select cp into x
                                                          orderby x.Year
                                                          select x).ToList();
            genPricingLandForTem3(document, pricing.DateCreate, list10, constructionPrices, pricing.Vat, pricing.ApartmentPrice, block.TypeReportApply, pricing.AreaCorrectionCoefficientId, block.ParentTypeReportApply);
            if (block.TypeReportApply != AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG)
            {
                        document.ReplaceAll("<apartmentPriceReducedNote>", pricing.ApartmentPriceReducedNote ?? ".....", (SearchOptions)0);
                List<PricingReducedPersonData> list11 = (from a in (IQueryable<PricingReducedPerson>)_context.PricingReducedPeople
                                                         where a.PricingId == pricing.Id && (int)a.Status != 99
                                                         select a into e
                                                         select new PricingReducedPersonData
                                                         {
                                                             CustomerName = ((IQueryable<Customer>)_context.Customers).Where((Customer c) => c.Id == e.CustomerId).FirstOrDefault().FullName,
                                                             Year = e.Year,
                                                             Salary = e.Salary,
                                                             DeductionCoefficient = e.DeductionCoefficient,
                                                             Value = e.Value
                                                         }).ToList();
            }
                    document.ReplaceAll("<apartmentPriceReduced>", UtilsService.FormatMoney(pricing.ApartmentPriceReduced.GetValueOrDefault()), (SearchOptions)0);
                    document.ReplaceAll("<apartmentPrice>", UtilsService.FormatMoney(pricing.ApartmentPrice), (SearchOptions)0);
                    document.ReplaceAll("<apartmentPriceRemaining>", UtilsService.FormatMoney(pricing.ApartmentPriceRemaining), (SearchOptions)0);
                    document.ReplaceAll("<apartmentPriceRemainingStr>", UtilsService.ConvertMoneyToString(pricing.ApartmentPriceRemaining.Value.ToString("N0", culture)).ToLower(), (SearchOptions)0);
                    document.ReplaceAll("<vat>", pricing.Vat.HasValue ? (pricing.Vat.Value.ToString("N2", culture) + "%") : "0%", (SearchOptions)0);
                    document.ReplaceAll("<apartmentPriceNoVat>", UtilsService.FormatMoney(pricing.ApartmentPriceNoVat), (SearchOptions)0);
                    document.ReplaceAll("<apartmentPriceVat>", UtilsService.FormatMoney(pricing.ApartmentPriceVat), (SearchOptions)0);
                    document.ReplaceAll("<landUsePlanningInfo>", block.LandUsePlanningInfo ?? "", (SearchOptions)0);
                    document.ReplaceAll("<highwayPlanningInfo>", block.HighwayPlanningInfo ?? "", (SearchOptions)0);
                    document.ReplaceAll("<landAcquisitionSituationInfo>", block.LandAcquisitionSituationInfo ?? "", (SearchOptions)0);
                    document.ReplaceAll("<landNo>", block.LandNo ?? "", (SearchOptions)0);
                    document.ReplaceAll("<mapNo>", block.MapNo ?? "", (SearchOptions)0);
            if (block.TypeReportApply == AppEnums.TypeReportApply.NHA_HO_CHUNG || block.TypeReportApply == AppEnums.TypeReportApply.NHA_CHUNG_CU || block.TypeReportApply == AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG)
            {
                        document.ReplaceAll("<alcav>", apartment.LandscapeAreaValue.HasValue ? apartment.LandscapeAreaValue.Value.ToString("N2", culture) : "0", (SearchOptions)0);
                        document.ReplaceAll("<alcav1>", apartment.LandscapeAreaValue1.HasValue ? apartment.LandscapeAreaValue1.Value.ToString("N2", culture) : "0", (SearchOptions)0);
                        document.ReplaceAll("<alcav2>", apartment.LandscapeAreaValue2.HasValue ? apartment.LandscapeAreaValue2.Value.ToString("N2", culture) : "0", (SearchOptions)0);
                        document.ReplaceAll("<alcav3>", apartment.LandscapeAreaValue3.HasValue ? apartment.LandscapeAreaValue3.Value.ToString("N2", culture) : "0", (SearchOptions)0);
            }
            else if (block.TypeReportApply == AppEnums.TypeReportApply.NHA_RIENG_LE)
            {
                        document.ReplaceAll("<alcav>", block.LandscapeAreaValue.HasValue ? block.LandscapeAreaValue.Value.ToString("N2", culture) : "0", (SearchOptions)0);
                        document.ReplaceAll("<alcav2>", block.LandscapePrivateAreaValue.HasValue ? block.LandscapePrivateAreaValue.Value.ToString("N2", culture) : "0", (SearchOptions)0);
                        document.ReplaceAll("<alcav1>", " ", (SearchOptions)0);
            }
            List<PricingApartmentLandDetail> list12 = (from a in (IQueryable<PricingApartmentLandDetail>)_context.PricingApartmentLandDetails
                                                       where a.PricingId == pricing.Id && (int)a.Status != 99
                                                       select a into x
                                                       orderby x.UpdatedAt
                                                       select x).ToList();
            foreach (PricingApartmentLandDetail item in list12)
            {
                        document.ReplaceAll("<buav>", item.PrivateArea.ToString(), (SearchOptions)0);
            }
            List<PricingApartmentLandDetailGroupByDecree> list13 = (from x in ((IQueryable<PricingApartmentLandDetail>)_context.PricingApartmentLandDetails).Where((PricingApartmentLandDetail a) => a.PricingId == pricing.Id && (int)a.Status != 99).ToList()
                                                                    group x by x.DecreeType1Id into x
                                                                    select new PricingApartmentLandDetailGroupByDecree
                                                                    {
                                                                        Decree = x.Key,
                                                                        pricingApartmentLandDetails = x.ToList()
                                                                    }).ToList();
            list13.ForEach(delegate (PricingApartmentLandDetailGroupByDecree pricingApartmentLandDetailGroupByDecree)
            {
                string text11 = "";
                float? num = null;
                int landPriceItemId = 0;
                double? num2 = null;
                string text12 = "";
                string landScapePrice = "";
                switch (pricingApartmentLandDetailGroupByDecree.Decree)
                {
                    case AppEnums.DecreeEnum.ND_CP_99_2015:
                        landPriceItemId = block.LandPriceItemId_99.GetValueOrDefault();
                        num2 = block.LandPriceItemValue_99;
                        text12 = block.PositionCoefficientStr_99;
                        num = block.LandPriceRefinement_99;
                                landScapePrice = UtilsService.FormatMoney(Convert.ToDecimal(num2)) + " đồng/m2 x " + text12 + (block.ExceedingLimitDeep.GetValueOrDefault() ? $" x (100 - {num})%" : "") + " = " + UtilsService.FormatMoney(Convert.ToDecimal(block.LandScapePrice_99)) + " đồng/m2";
                        break;
                    case AppEnums.DecreeEnum.ND_CP_34_2013:
                        landPriceItemId = block.LandPriceItemId_34.GetValueOrDefault();
                        num2 = block.LandPriceItemValue_34;
                        text12 = ((block.CaseApply_34.GetValueOrDefault() == AppEnums.TypeCaseApply_34.KHOAN_2) ? block.PositionCoefficientStr_34 : block.AlleyPositionCoefficientStr_34);
                        num = block.LandPriceRefinement_34;
                                landScapePrice = UtilsService.FormatMoney(Convert.ToDecimal(num2)) + " đồng/m2 x " + text12 + ((block.ExceedingLimitDeep.GetValueOrDefault() && block.CaseApply_34.GetValueOrDefault() == AppEnums.TypeCaseApply_34.KHOAN_2) ? $" x (100 - {num})%" : "") + " = " + UtilsService.FormatMoney(Convert.ToDecimal((block.CaseApply_34.GetValueOrDefault() == AppEnums.TypeCaseApply_34.KHOAN_2) ? block.LandScapePrice_34 : block.AlleyLandScapePrice_34)) + " đồng/m2";
                        break;
                    case AppEnums.DecreeEnum.ND_CP_61:
                        landPriceItemId = block.LandPriceItemId_61.GetValueOrDefault();
                        num2 = block.LandPriceItemValue_61;
                        text12 = block.PositionCoefficientStr_61;
                        num = block.LandPriceRefinement_61;
                        landScapePrice = ((!block.IsFrontOfLine_61.GetValueOrDefault() && (block.IsFrontOfLine_61.GetValueOrDefault() || block.IsAlley_61.GetValueOrDefault())) ? (UtilsService.FormatMoney(Convert.ToDecimal(block.No2LandScapePrice_61)) + " đồng/m2 x 0.8 = " + UtilsService.FormatMoney(Convert.ToDecimal(block.LandScapePrice_61)) + " đồng/m2") : (UtilsService.FormatMoney(Convert.ToDecimal(block.LandScapePrice_61)) + " đồng/m2"));
                        break;
                }
                LandPriceItem landPriceItem = ((IQueryable<LandPriceItem>)_context.LandPriceItems).Where((LandPriceItem l) => l.Id == landPriceItemId).FirstOrDefault();
                if (landPriceItem != null)
                {
                    text11 = landPriceItem.LaneName + ", đoạn từ " + landPriceItem.LaneStartName + ((landPriceItem.LaneEndName != null && landPriceItem.LaneEndName != "") ? (" đến đoạn " + landPriceItem.LaneEndName) : "");
                    text11 = ((landPriceItem.Des != null) ? (text11 + ", " + landPriceItem.Des) : text11);
                    LandPrice landPrice = ((IQueryable<LandPrice>)_context.LandPricies).Where((LandPrice l) => l.Id == landPriceItem.LandPriceId).FirstOrDefault();
                    if (landPrice != null)
                    {
                        text11 = text11 + ", " + landPrice.Des;
                    }
                }
                pricingApartmentLandDetailGroupByDecree.LandPriceItemNote = text11;
                pricingApartmentLandDetailGroupByDecree.LandPriceItemValue = Convert.ToDecimal(num2);
                pricingApartmentLandDetailGroupByDecree.LandScapePrice = landScapePrice;
                pricingApartmentLandDetailGroupByDecree.LandPriceRefinement = num;
                pricingApartmentLandDetailGroupByDecree.PositionCoefficientStr = text12;
            });
            if (list13.Count() > 0)
            {
                genApartmentLandDetailInfo(document, list13, block, pricing, list9, list, (apartment != null) ? apartment.LandscapeAreaValue1 : null);
            }
                    document.EndUpdate();
            val.SaveDocument(text2, DocumentFormat.OpenXml);
            return text2;
        }
        return null;
    }
    return null;
}

private void genBlockMaintextureRateTblaForTem3(Document document, List<BlockMaintextureRateData> blockMaintextureRateDatas, string str)
{
    //IL_00ff: Unknown result type (might be due to invalid IL or missing references)
    //IL_0109: Expected O, but got Unknown
            DocumentRange[] array = document.FindAll("<maintextureRateTbla>", (SearchOptions)0);
    List<BlockMaintextureRateDataGroupByTypeMainTexTure> blockMaintextureRateDataGroupByTypeMainTexTures = (from x in blockMaintextureRateDatas
                                                                                                            group x by x.TypeMainTexTure into x
                                                                                                            select new BlockMaintextureRateDataGroupByTypeMainTexTure
                                                                                                            {
                                                                                                                TypeMainTexTure = x.Key,
                                                                                                                blockMaintextureRateDatas = x.ToList()
                                                                                                            } into x
                                                                                                            orderby x.TypeMainTexTure
                                                                                                            select x).ToList();
    if (blockMaintextureRateDataGroupByTypeMainTexTures.Count > 0)
    {
                Table val = document.Tables.Create(array[0].Start, 8, 6, (AutoFitBehaviorType)0);
        List<BlockMaintextureRateData> childBlockMaintextureRateDatas = new List<BlockMaintextureRateData>();
        string currMainTextureNameColumn = "";
        string remainingRateColumn = "";
        string mainRateColumn = "";
        val.ForEachCell((TableCellProcessorDelegate)delegate (TableCell cell, int i, int j)
        {
            if (i > 0 && i < 7 && j == 0)
            {
                currMainTextureNameColumn = "";
                remainingRateColumn = "";
                mainRateColumn = "";
                childBlockMaintextureRateDatas = blockMaintextureRateDataGroupByTypeMainTexTures[i - 1].blockMaintextureRateDatas.OrderBy((BlockMaintextureRateData x) => x.LevelBlockId).ToList();
                for (int k = 0; k < childBlockMaintextureRateDatas.Count; k++)
                {
                    BlockMaintextureRateData blockMaintextureRateData = childBlockMaintextureRateDatas[k];
                    currMainTextureNameColumn = ((currMainTextureNameColumn == "") ? blockMaintextureRateData.CurrentStateMainTextureName : string.Join("\n", currMainTextureNameColumn, blockMaintextureRateData.CurrentStateMainTextureName));
                    remainingRateColumn = ((remainingRateColumn == "") ? blockMaintextureRateData.RemainingRate.ToString("N2", culture) : string.Join("\n", remainingRateColumn, blockMaintextureRateData.RemainingRate.ToString("N2", culture)));
                    mainRateColumn = ((mainRateColumn == "") ? blockMaintextureRateData.MainRate.ToString("N2", culture) : string.Join("\n", mainRateColumn, blockMaintextureRateData.MainRate.ToString("N2", culture)));
                }
            }
            switch (j)
            {
                case 0:
                            cell.PreferredWidth = (5f);
                            cell.PreferredWidthType = ((WidthType)2);
                    if (i == 0)
                    {
                                document.InsertSingleLineText(cell.Range.Start, "STT");
                    }
                    else if (i < 7)
                    {
                                document.InsertSingleLineText(cell.Range.Start, $"{i}");
                    }
                    break;
                case 1:
                            cell.PreferredWidth = (20f);
                            cell.PreferredWidthType = ((WidthType)2);
                    if (i == 0)
                    {
                                document.InsertSingleLineText(cell.Range.Start, "Kết cấu chính");
                    }
                    else if (i < 7)
                    {
                                document.InsertSingleLineText(cell.Range.Start, blockMaintextureRateDataGroupByTypeMainTexTures[i - 1].blockMaintextureRateDatas[0].TypeMainTexTureName ?? "");
                    }
                    break;
                case 2:
                            cell.PreferredWidth = (25f);
                            cell.PreferredWidthType = ((WidthType)2);
                    if (i == 0)
                    {
                                document.InsertSingleLineText(cell.Range.Start, "Hiện trạng");
                    }
                    else if (i < 7)
                    {
                                document.InsertText(cell.Range.Start, currMainTextureNameColumn);
                    }
                    break;
                case 3:
                            cell.PreferredWidth = (15f);
                            cell.PreferredWidthType = ((WidthType)2);
                    if (i == 0)
                    {
                                document.InsertSingleLineText(cell.Range.Start, "Tỷ lệ chất lượng còn lại %");
                    }
                    else if (i < 7)
                    {
                                document.InsertText(cell.Range.Start, remainingRateColumn);
                    }
                    break;
                case 4:
                            cell.PreferredWidth = (20f);
                            cell.PreferredWidthType = ((WidthType)2);
                    if (i == 0)
                    {
                                document.InsertSingleLineText(cell.Range.Start, "Tỷ lệ giá trị của kết cấu chính so với tổng giá trị của nhà ở %");
                    }
                    else if (i < 7)
                    {
                                document.InsertText(cell.Range.Start, mainRateColumn);
                    }
                    break;
                case 5:
                            cell.PreferredWidth = (15f);
                            cell.PreferredWidthType = ((WidthType)2);
                    if (i == 0)
                    {
                                document.InsertSingleLineText(cell.Range.Start, "Ghi chú");
                    }
                    else if (i < 7)
                    {
                                document.InsertText(cell.Range.Start, "");
                    }
                    break;
            }
            if (i == 0)
            {
                        cell.VerticalAlignment = ((TableCellVerticalAlignment)2);
            }
            else
            {
                        cell.VerticalAlignment = ((TableCellVerticalAlignment)0);
            }
        });
        val.BeginUpdate();
                val.MergeCells(val[7, 0], val[7, 2]);
                val[7, 0].PreferredWidth = (50f);
                val[7, 0].PreferredWidthType = ((WidthType)2);
                val[7, 0].Borders.Bottom.LineStyle = ((TableBorderLineStyle)0);
                val[7, 0].Borders.Top.LineStyle = ((TableBorderLineStyle)0);
                val[7, 0].Borders.Right.LineStyle = ((TableBorderLineStyle)0);
                val[7, 0].Borders.Left.LineStyle = ((TableBorderLineStyle)0);
                val.MergeCells(val[7, 1], val[7, 3]);
                val[7, 1].PreferredWidth = (50f);
                val[7, 1].PreferredWidthType = ((WidthType)2);
                val[7, 1].Borders.Bottom.LineStyle = ((TableBorderLineStyle)0);
                val[7, 1].Borders.Top.LineStyle = ((TableBorderLineStyle)0);
                val[7, 1].Borders.Right.LineStyle = ((TableBorderLineStyle)0);
                val[7, 1].Borders.Left.LineStyle = ((TableBorderLineStyle)0);
        val.EndUpdate();
                val.TableAlignment = ((TableRowAlignment)1);
    }
    else
    {
                DocumentRange val2 = document.InsertText(array[0].Start, "\n");
    }
            document.ReplaceAll("<maintextureRateTbla>", "", (SearchOptions)0);
}

private void genPricingLandForTem3(Document document, DateTime? dateCreate, List<PricingLandTblData> pricingLandTblDatas, List<ConstructionPrice> constructionPrices, float? vat, decimal? price, AppEnums.TypeReportApply typeReportApply, int? AreaCorrectionCoefficientId, AppEnums.TypeReportApply? parentTypeReportApply)
{
    string listPrice = "";
    string MaintextureRateValue = "";
    string MaintextureRateValueText = "";
    if (typeReportApply == AppEnums.TypeReportApply.NHA_CHUNG_CU || (typeReportApply == AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG && parentTypeReportApply.GetValueOrDefault() == AppEnums.TypeReportApply.NHA_CHUNG_CU))
    {
        List<IGrouping<bool?, PricingLandTblData>> list = (from x in pricingLandTblDatas
                                                           group x by x.ApplyInvestmentRate into x
                                                           orderby x.Key descending
                                                           select x).ToList();
        list.ForEach(delegate (IGrouping<bool?, PricingLandTblData> groupPricingLandTblDatasByApplyInvestmentRate)
        {
            string text4 = (groupPricingLandTblDatasByApplyInvestmentRate.Key.GetValueOrDefault() ? "Trường hợp các phần diện tích đất áp dụng Suất vốn đầu tư\n" : "Trường hợp các phần diện tích đất không áp dụng Suất vốn đầu tư\n");
            List<IGrouping<int?, PricingLandTblData>> list4 = (from x in groupPricingLandTblDatasByApplyInvestmentRate.ToList()
                                                               group x by x.Level into x
                                                               orderby x.Key
                                                               select x).ToList();
            list4.ForEach(delegate (IGrouping<int?, PricingLandTblData> groupPricingLandTblDatasBylevelItem)
            {
                List<PricingLandTblData> list5 = groupPricingLandTblDatasBylevelItem.ToList();
                list5.ForEach(delegate (PricingLandTblData childPricingLandTblData)
                {
                    listPrice = ((listPrice == "") ? (UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice) ?? "") : (listPrice + " + " + UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice)));
                    if (childPricingLandTblData.ApplyInvestmentRate.GetValueOrDefault())
                    {
                                MaintextureRateValue += $"{childPricingLandTblData.MaintextureRateValue} %\n";
                        MaintextureRateValueText = MaintextureRateValueText + UtilsService.ConvertPercentToString(childPricingLandTblData.MaintextureRateValue.ToString()) + "\n";
                        string text5 = "";
                        InvestmentRateItem investmentRateItem = ((IQueryable<InvestmentRateItem>)_context.InvestmentRateItems).Where((InvestmentRateItem i) => (int?)i.Id == childPricingLandTblData.InvestmentRateItemId).FirstOrDefault();
                        if (investmentRateItem != null)
                        {
                            InvestmentRate investmentRate = ((IQueryable<InvestmentRate>)_context.InvestmentRaties).Where((InvestmentRate i) => i.Id == investmentRateItem.Id).FirstOrDefault();
                            if (investmentRate != null)
                            {
                                text5 = "\nCăn cứ dòng số " + investmentRateItem.LineInfo + " chi tiết suất vốn đầu tư " + investmentRateItem.DetailInfo + ". Suất vốn đầu tư: " + UtilsService.FormatMoney((decimal)investmentRateItem.Value) + " đồng/m2, Chi phí xây dựng: " + UtilsService.FormatMoney((decimal)investmentRateItem.Value1) + " đồng/m2, Chi phí thiết bị: " + UtilsService.FormatMoney((decimal)investmentRateItem.Value2) + " đồng/m2";
                            }
                        }
                        AreaCorrectionCoefficient areaCorrectionCoefficient = ((IQueryable<AreaCorrectionCoefficient>)_context.AreaCorrectionCoefficients).Where((AreaCorrectionCoefficient a) => (int?)a.Id == AreaCorrectionCoefficientId).FirstOrDefault();
                        if (areaCorrectionCoefficient != null)
                        {
                            string text6 = areaCorrectionCoefficient.Note + "\nHệ số điều chỉnh vùng: " + areaCorrectionCoefficient.Value.ToString("N3", culture);
                        }
                    }
                    else
                    {
                                MaintextureRateValue += $"{childPricingLandTblData.MaintextureRateValue} %\n";
                        MaintextureRateValueText = MaintextureRateValueText + UtilsService.ConvertPercentToString(childPricingLandTblData.MaintextureRateValue.ToString()) + "\n";
                    }
                    float num2 = childPricingLandTblData.GeneralArea.GetValueOrDefault() + childPricingLandTblData.PrivateArea.GetValueOrDefault();
                    string text7 = (childPricingLandTblData.CoefficientUseValue.HasValue ? childPricingLandTblData.CoefficientUseValue.Value.ToString("N2", culture) : "");
                });
            });
        });
    }
    else
    {
        List<IGrouping<int?, PricingLandTblData>> list2 = (from x in pricingLandTblDatas
                                                           group x by x.Level into x
                                                           orderby x.Key
                                                           select x).ToList();
        list2.ForEach(delegate (IGrouping<int?, PricingLandTblData> groupPricingLandTblDatasBylevelItem)
        {
            List<PricingLandTblData> list3 = groupPricingLandTblDatasBylevelItem.ToList();
            list3.ForEach(delegate (PricingLandTblData childPricingLandTblData)
            {
                listPrice = ((listPrice == "") ? (UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice) ?? "") : (listPrice + " + " + UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice)));
                string text = (childPricingLandTblData.MaintextureRateValue.HasValue ? childPricingLandTblData.MaintextureRateValue.Value.ToString("N2", culture) : "");
                string text2 = "";
                float num = childPricingLandTblData.GeneralArea.GetValueOrDefault() + childPricingLandTblData.PrivateArea.GetValueOrDefault();
                string text3 = (childPricingLandTblData.CoefficientUseValue.HasValue ? childPricingLandTblData.CoefficientUseValue.Value.ToString("N2", culture) : "");
                if (childPricingLandTblData.TermApply.GetValueOrDefault() == AppEnums.TermApply.DIEU_65 || childPricingLandTblData.TermApply.GetValueOrDefault() == AppEnums.TermApply.KHOAN_1_DIEU_34 || childPricingLandTblData.TermApply.GetValueOrDefault() == AppEnums.TermApply.DIEU_7)
                {
                    text2 = "\t" + UtilsService.FormatMoney(childPricingLandTblData.PriceInYear) + " đồng/m2 x " + text + "% x " + num.ToString("N2", culture) + " m2 x " + text3 + " = " + UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice) + "\n";
                }
                else if (childPricingLandTblData.TermApply.GetValueOrDefault() == AppEnums.TermApply.DIEU_70 || childPricingLandTblData.TermApply.GetValueOrDefault() == AppEnums.TermApply.KHOAN_2_DIEU_34 || childPricingLandTblData.TermApply.GetValueOrDefault() == AppEnums.TermApply.DIEU_35 || childPricingLandTblData.TermApply.GetValueOrDefault() == AppEnums.TermApply.DIEU_71)
                {
                    text2 = "\t" + UtilsService.FormatMoney(childPricingLandTblData.PriceInYear) + " đồng/m2 x " + text + "% x " + num.ToString("N2", culture) + " m2 = " + UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice) + "\n";
                }
                        MaintextureRateValue += $"{childPricingLandTblData.MaintextureRateValue} %\n";
                MaintextureRateValueText = MaintextureRateValueText + UtilsService.ConvertPercentToString(childPricingLandTblData.MaintextureRateValue.ToString()) + "\n";
            });
        });
    }
            document.ReplaceAll("<MaintextureRateValue>", MaintextureRateValue ?? "", (SearchOptions)0);
            document.ReplaceAll("<MaintextureRateValueText>", MaintextureRateValueText ?? "", (SearchOptions)0);
}

private static string GetVietnameseDayOfWeek(DayOfWeek day)
{
    switch (day)
    {
        case DayOfWeek.Monday:
            return "Thứ Hai";
        case DayOfWeek.Tuesday:
            return "Thứ Ba";
        case DayOfWeek.Wednesday:
            return "Thứ Tư";
        case DayOfWeek.Thursday:
            return "Thứ Năm";
        case DayOfWeek.Friday:
            return "Thứ Sáu";
        case DayOfWeek.Saturday:
            return "Thứ Bảy";
        case DayOfWeek.Sunday:
            return "Chủ Nhật";
        default:
            return "Không xác định";
    }
}

[HttpPost("GetExportWordPL4/{id}")]
        public async Task<IActionResult> GetExportWordPL4(int id)
        {
            string accessToken = base.Request.Headers[HeaderNames.Authorization];
            Token token = ((IQueryable<Token>)_context.Tokens).Where((Token t) => Convert.ToString(t.AccessToken) == accessToken).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }
            DefaultResponse defaultResponse = new DefaultResponse();
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)base.User.Identity;
            string access_key = (from c in claimsIdentity.Claims
                                 where c.Type == "AccessKey"
                                 select c.Value).SingleOrDefault();
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, 5))
            {
                defaultResponse.meta = new Meta(222, "Bạn không có quyền xem dữ liệu tới mục này!");
                return Ok(defaultResponse);
            }
            try
            {
                string text = insertDataToTemplatePL4(id);
                if (text != null)
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        new HttpResponseMessage();
                        string fileDownloadName = "bien_ban-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";
                        using (FileStream fileStream = new FileStream(text, FileMode.Open, FileAccess.Read))
                        {
                            fileStream.CopyTo(memoryStream);
                        }
                        try
                        {
                            System.IO.File.Delete(text);
                        }
                        catch
                        {
                        }
                        return File(memoryStream.ToArray(), "application/octet-stream", fileDownloadName);
                    }
                }
                defaultResponse.meta = new Meta(215, "Data file null!");
                return Ok(defaultResponse);
            }
            catch (Exception ex)
            {
                log.Error((object)("GetExportWordExample:" + ex.Message));
                throw;
            }
}

private string insertDataToTemplatePL4(int id)
{
    //IL_0308: Unknown result type (might be due to invalid IL or missing references)
    //IL_030f: Expected O, but got Unknown
    //IL_0313: Unknown result type (might be due to invalid IL or missing references)
    //IL_09bd: Unknown result type (might be due to invalid IL or missing references)
    Pricing pricing = ((IQueryable<Pricing>)_context.Pricings).Where((Pricing p) => p.Id == id && (int)p.Status != 99).FirstOrDefault();
    if (pricing != null)
    {
        Block block = ((IQueryable<Block>)_context.Blocks).Where((Block b) => b.Id == pricing.BlockId && (int)b.Status != 99).FirstOrDefault();
        Apartment apartment = ((IQueryable<Apartment>)_context.Apartments).Where((Apartment a) => a.Id == pricing.ApartmentId && (int)a.Status != 99).FirstOrDefault();
        if (block != null)
        {
            if ((block.TypeReportApply == AppEnums.TypeReportApply.NHA_HO_CHUNG || block.TypeReportApply == AppEnums.TypeReportApply.NHA_CHUNG_CU || block.TypeReportApply == AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG) && apartment == null)
            {
                return null;
            }
            string text = Path.Combine(_hostingEnvironment.WebRootPath, "exportword/phu_luc_04.docx");
            string text2 = "result" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";
            RichEditDocumentServer val = new RichEditDocumentServer();
            val.LoadDocumentTemplate(text, DocumentFormat.OpenXml);
                    Document document = val.Document;
                    document.BeginUpdate();
            List<Customer> list = (from c in (IQueryable<Customer>)_context.Customers
                                   join pc in (IEnumerable<PricingCustomer>)_context.PricingCustomers on c.Id equals pc.CustomerId
                                   where (int)c.Status != 99 && (int)pc.Status != 99 && pc.PricingId == pricing.Id
                                   select c).ToList();
            string text3 = "";
            int num = 0;
            foreach (Customer item in list)
            {
                text3 = ((text3 == "") ? item.FullName : (text3 + ", " + item.FullName));
                num++;
            }
                    document.ReplaceAll("<customer>", text3, (SearchOptions)0);
                    document.ReplaceAll("<totalCustomer>", num.ToString(), (SearchOptions)0);
                    document.ReplaceAll("<blockAdr>", block.Address ?? "", (SearchOptions)0);
            Lane lane = ((IQueryable<Lane>)_context.Lanies).Where((Lane l) => (int?)l.Id == block.Lane && (int)l.Status != 99).FirstOrDefault();
                    document.ReplaceAll("<lane>", (lane != null) ? lane.Name : "", (SearchOptions)0);
            Ward ward = ((IQueryable<Ward>)_context.Wards).Where((Ward l) => l.Id == block.Ward && (int)l.Status != 99).FirstOrDefault();
                    document.ReplaceAll("<ward>", (ward != null) ? ward.Name : "", (SearchOptions)0);
            District district = ((IQueryable<District>)_context.Districts).Where((District l) => l.Id == block.District && (int)l.Status != 99).FirstOrDefault();
                    document.ReplaceAll("<district>", (district != null) ? district.Name : "", (SearchOptions)0);
                    document.EndUpdate();
            val.SaveDocument(text2, DocumentFormat.OpenXml);
            return text2;
        }
        return null;
    }
    return null;
}

[HttpPost("GetExportWordPL5/{id}")]
        public async Task<IActionResult> GetExportWordPL5(int id)
        {
            string accessToken = base.Request.Headers[HeaderNames.Authorization];
            Token token = ((IQueryable<Token>)_context.Tokens).Where((Token t) => Convert.ToString(t.AccessToken) == accessToken).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }
            DefaultResponse defaultResponse = new DefaultResponse();
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)base.User.Identity;
            string access_key = (from c in claimsIdentity.Claims
                                 where c.Type == "AccessKey"
                                 select c.Value).SingleOrDefault();
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, 5))
            {
                defaultResponse.meta = new Meta(222, "Bạn không có quyền xem dữ liệu tới mục này!");
                return Ok(defaultResponse);
            }
            try
            {
                string text = insertDataToTemplatePL5(id);
                if (text != null)
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        new HttpResponseMessage();
                        string fileDownloadName = "bien_ban-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";
                        using (FileStream fileStream = new FileStream(text, FileMode.Open, FileAccess.Read))
                        {
                            fileStream.CopyTo(memoryStream);
                        }
                        try
                        {
                            System.IO.File.Delete(text);
                        }
                        catch
                        {
                        }
                        return File(memoryStream.ToArray(), "application/octet-stream", fileDownloadName);
                    }
                }
                defaultResponse.meta = new Meta(215, "Data file null!");
                return Ok(defaultResponse);
            }
            catch (Exception ex)
            {
                log.Error((object)("GetExportWordExample:" + ex.Message));
                throw;
            }
}

private string insertDataToTemplatePL5(int id)
{
    //IL_035f: Unknown result type (might be due to invalid IL or missing references)
    //IL_0366: Expected O, but got Unknown
    //IL_036a: Unknown result type (might be due to invalid IL or missing references)
    //IL_28ea: Unknown result type (might be due to invalid IL or missing references)
    Pricing pricing = ((IQueryable<Pricing>)_context.Pricings).Where((Pricing p) => p.Id == id && (int)p.Status != 99).FirstOrDefault();
    if (pricing != null)
    {
        Block block = ((IQueryable<Block>)_context.Blocks).Where((Block b) => b.Id == pricing.BlockId && (int)b.Status != 99).FirstOrDefault();
        Apartment apartment = ((IQueryable<Apartment>)_context.Apartments).Where((Apartment a) => a.Id == pricing.ApartmentId && (int)a.Status != 99).FirstOrDefault();
        if (block != null)
        {
            if ((block.TypeReportApply == AppEnums.TypeReportApply.NHA_HO_CHUNG || block.TypeReportApply == AppEnums.TypeReportApply.NHA_CHUNG_CU || block.TypeReportApply == AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG) && apartment == null)
            {
                return null;
            }
            string text = Path.Combine(_hostingEnvironment.WebRootPath, "exportword/phu_luc_05.docx");
            string text2 = "result" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";
            RichEditDocumentServer val = new RichEditDocumentServer();
            val.LoadDocumentTemplate(text, DocumentFormat.OpenXml);
                    Document document = val.Document;
                    document.BeginUpdate();
                    document.ReplaceAll("<blockAdr>", block.Address ?? "", (SearchOptions)0);
            string typeBlock = getTypeBlock(block.TypeBlockId);
                    document.ReplaceAll("<typeBlock>", typeBlock, (SearchOptions)0);
                    document.ReplaceAll("<LandPriceTotal>", UtilsService.FormatMoney(pricing.LandPrice), (SearchOptions)0);
                    document.ReplaceAll("<Total>", UtilsService.FormatMoney(pricing.TotalPrice), (SearchOptions)0);
            List<BlockDetail> list = ((IQueryable<BlockDetail>)_context.BlockDetails).Where((BlockDetail l) => l.BlockId == block.Id && (int)l.Status != 99).ToList();
            if (block.TypeReportApply == AppEnums.TypeReportApply.NHA_HO_CHUNG || block.TypeReportApply == AppEnums.TypeReportApply.NHA_CHUNG_CU || block.TypeReportApply == AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG)
            {
                if (block.TypeReportApply == AppEnums.TypeReportApply.NHA_HO_CHUNG)
                {
                    List<BlockDetailData> list2 = ((IMapperBase)_mapper).Map<List<BlockDetailData>>((object)list);
                    foreach (BlockDetailData map_BlockDetail in list2)
                    {
                        Floor floor = ((IQueryable<Floor>)_context.Floors).Where((Floor f) => f.Id == map_BlockDetail.FloorId && (int)f.Status != 99).FirstOrDefault();
                        map_BlockDetail.FloorName = ((floor != null) ? floor.Name : "");
                                map_BlockDetail.FloorCode = floor?.Code ?? 0;
                        Area area = ((IQueryable<Area>)_context.Areas).Where((Area f) => f.Id == map_BlockDetail.AreaId && (int)f.Status != 99).FirstOrDefault();
                        map_BlockDetail.AreaName = ((area != null) ? area.Name : "");
                                map_BlockDetail.IsMezzanine = area?.IsMezzanine;
                    }
                    list2 = (from x in list2
                             orderby x.FloorCode, x.IsMezzanine
                             select x).ToList();
                }
                List<ApartmentDetail> list3 = ((IQueryable<ApartmentDetail>)_context.ApartmentDetails).Where((ApartmentDetail a) => a.TargetId == apartment.Id && (int)a.Type == 2 && (int)a.Status != 99).ToList();
                List<ApartmentDetailData> list4 = ((IMapperBase)_mapper).Map<List<ApartmentDetailData>>((object)list3);
                foreach (ApartmentDetailData map_ApartmentDetail2 in list4)
                {
                    Floor floor2 = ((IQueryable<Floor>)_context.Floors).Where((Floor f) => f.Id == map_ApartmentDetail2.FloorId && (int)f.Status != 99).FirstOrDefault();
                    map_ApartmentDetail2.FloorName = ((floor2 != null) ? floor2.Name : "");
                            map_ApartmentDetail2.FloorCode = floor2?.Code ?? 0;
                    Area area2 = ((IQueryable<Area>)_context.Areas).Where((Area f) => f.Id == map_ApartmentDetail2.AreaId && (int)f.Status != 99).FirstOrDefault();
                    map_ApartmentDetail2.AreaName = ((area2 != null) ? area2.Name : "");
                            map_ApartmentDetail2.IsMezzanine = area2?.IsMezzanine;
                }
                list4 = (from x in list4
                         orderby x.Level, x.FloorCode, x.IsMezzanine
                         select x).ToList();
                if (block.TypeReportApply == AppEnums.TypeReportApply.NHA_CHUNG_CU || block.TypeReportApply == AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG || block.ParentTypeReportApply.GetValueOrDefault() == AppEnums.TypeReportApply.NHA_CHUNG_CU)
                {
                    string levelApartment = getLevelApartment(list4);
                            document.ReplaceAll("<levelApartment>", levelApartment, (SearchOptions)0);
                }
                string apartDetailInfo = getApartDetailInfo(list4);
                        document.ReplaceAll("<apartDetailInfo>", apartDetailInfo, (SearchOptions)0);
                string text3 = (apartment.ConstructionAreaValue.HasValue ? apartment.ConstructionAreaValue.Value.ToString("N2", culture) : "0");
                        document.ReplaceAll("<acav>", text3, (SearchOptions)0);
                        document.ReplaceAll("<acaNote>", (apartment.ConstructionAreaNote != null && apartment.ConstructionAreaNote != "") ? ("(" + apartment.ConstructionAreaNote + ")") : "", (SearchOptions)0);
                string text4 = (apartment.ConstructionAreaValue1.HasValue ? apartment.ConstructionAreaValue1.Value.ToString("N2", culture) : "0");
                        document.ReplaceAll("<acav1>", text4, (SearchOptions)0);
                string text5 = (apartment.ConstructionAreaValue2.HasValue ? apartment.ConstructionAreaValue2.Value.ToString("N2", culture) : "0");
                        document.ReplaceAll("<acav2>", text5, (SearchOptions)0);
                string text6 = (apartment.ConstructionAreaValue3.HasValue ? apartment.ConstructionAreaValue3.Value.ToString("N2", culture) : "0");
                        document.ReplaceAll("<acav3>", text6, (SearchOptions)0);
                string text7 = (apartment.UseAreaValue.HasValue ? apartment.UseAreaValue.Value.ToString("N2", culture) : "0");
                        document.ReplaceAll("<auav>", text7, (SearchOptions)0);
                string text8 = (apartment.UseAreaValue1.HasValue ? apartment.UseAreaValue1.Value.ToString("N2", culture) : "0");
                        document.ReplaceAll("<auav1>", text8, (SearchOptions)0);
                string text9 = (apartment.UseAreaValue2.HasValue ? apartment.UseAreaValue2.Value.ToString("N2", culture) : "0");
                        document.ReplaceAll("<auav2>", text9, (SearchOptions)0);
            }
            else
            {
                List<ApartmentDetail> list5 = ((IQueryable<ApartmentDetail>)_context.ApartmentDetails).Where((ApartmentDetail a) => a.TargetId == block.Id && (int)a.Type == 1 && (int)a.Status != 99).ToList();
                List<ApartmentDetailData> list6 = ((IMapperBase)_mapper).Map<List<ApartmentDetailData>>((object)list5);
                foreach (ApartmentDetailData map_ApartmentDetail in list6)
                {
                    Floor floor3 = ((IQueryable<Floor>)_context.Floors).Where((Floor f) => f.Id == map_ApartmentDetail.FloorId && (int)f.Status != 99).FirstOrDefault();
                    map_ApartmentDetail.FloorName = ((floor3 != null) ? floor3.Name : "");
                            map_ApartmentDetail.FloorCode = floor3?.Code ?? 0;
                    Area area3 = ((IQueryable<Area>)_context.Areas).Where((Area f) => f.Id == map_ApartmentDetail.AreaId && (int)f.Status != 99).FirstOrDefault();
                    map_ApartmentDetail.AreaName = ((area3 != null) ? area3.Name : "");
                            map_ApartmentDetail.IsMezzanine = area3?.IsMezzanine;
                }
                list6 = (from x in list6
                         orderby x.Level, x.FloorCode, x.IsMezzanine
                         select x).ToList();
            }
            List<BlockMaintextureRateData> source = (from b in (IQueryable<BlockMaintextureRate>)_context.BlockMaintextureRaties
                                                     where b.TargetId == block.Id && (int)b.Type == 1 && (int)b.Status != 99
                                                     select b into e
                                                     select new BlockMaintextureRateData
                                                     {
                                                         LevelBlockId = e.LevelBlockId,
                                                         RatioMainTextureId = e.RatioMainTextureId,
                                                         TypeMainTexTure = e.TypeMainTexTure,
                                                         CurrentStateMainTextureId = e.CurrentStateMainTextureId,
                                                         RemainingRate = e.RemainingRate,
                                                         MainRate = e.MainRate,
                                                         TotalValue = e.TotalValue,
                                                         TotalValue1 = e.TotalValue1,
                                                         TotalValue2 = e.TotalValue2,
                                                         CurrentStateMainTextureName = ((IQueryable<CurrentStateMainTexture>)_context.CurrentStateMainTexturies).Where((CurrentStateMainTexture c) => c.Id == e.CurrentStateMainTextureId).FirstOrDefault().Name,
                                                         TypeMainTexTureName = getTypeMainTexTureName(e.TypeMainTexTure)
                                                     }).ToList();
            List<IGrouping<int, BlockMaintextureRateData>> list7 = (from x in source
                                                                    group x by x.LevelBlockId into x
                                                                    orderby x.Key
                                                                    select x).ToList();
            string dataStr = "";
            string dataStr2 = "";
            string dataStr3 = "";
            list7.ForEach(delegate (IGrouping<int, BlockMaintextureRateData> item)
            {
                string arg = (item.FirstOrDefault().TotalValue.HasValue ? item.FirstOrDefault().TotalValue.Value.ToString("N2", culture) : "");
                string arg2 = (item.FirstOrDefault().TotalValue1.HasValue ? item.FirstOrDefault().TotalValue1.Value.ToString("N2", culture) : "");
                string arg3 = (item.FirstOrDefault().TotalValue2.HasValue ? item.FirstOrDefault().TotalValue2.Value.ToString("N2", culture) : "");
                        dataStr = ((dataStr == "") ? $"- Phần nhà cấp {item.Key}    =     {item.FirstOrDefault().TotalValue} %" : string.Join("\n", dataStr, $"- Phần nhà cấp {item.Key}    =     {arg} %"));
                        dataStr2 = ((dataStr2 == "") ? $"* Phần nhà cấp {item.Key}    =     {item.FirstOrDefault().TotalValue1} %" : string.Join("\n", dataStr2, $"* Phần nhà cấp {item.Key}    =     {arg2} %"));
                        dataStr3 = ((dataStr3 == "") ? $"* Phần nhà cấp {item.Key}    =     {item.FirstOrDefault().TotalValue2} %" : string.Join("\n", dataStr3, $"* Phần nhà cấp {item.Key}    =     {arg3} %"));
            });
            List<PricingLandTbl> list8 = ((IQueryable<PricingLandTbl>)_context.PricingLandTbls).Where((PricingLandTbl a) => a.PricingId == pricing.Id && (int)a.Status != 99).ToList();
            List<PricingLandTblData> list9 = ((IMapperBase)_mapper).Map<List<PricingLandTblData>>((object)list8);
            list9.ForEach(delegate (PricingLandTblData pricingLandTblData)
            {
                Area area4 = ((IQueryable<Area>)_context.Areas).Where((Area a) => a.Id == pricingLandTblData.AreaId && (int)a.Status != 99).FirstOrDefault();
                pricingLandTblData.AreaName = ((area4 != null) ? area4.Name : "");
                PriceListItem priceListItem = ((IQueryable<PriceListItem>)_context.PriceListItems).Where((PriceListItem p) => (int?)p.Id == pricingLandTblData.PriceListItemId).FirstOrDefault();
                if (priceListItem != null)
                {
                    PriceList priceList = ((IQueryable<PriceList>)_context.PriceLists).Where((PriceList p) => p.Id == priceListItem.PriceListId).FirstOrDefault();
                            pricingLandTblData.PriceListItemNote = "Căn cứ " + priceListItem.NameOfConstruction + ((priceList != null && priceList.Des != null) ? (" theo " + priceList?.Des) : "");
                }
                if (block.TypeReportApply == AppEnums.TypeReportApply.NHA_CHUNG_CU || (block.TypeReportApply == AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG && block.ParentTypeReportApply.GetValueOrDefault() == AppEnums.TypeReportApply.NHA_CHUNG_CU))
                {
                    if (pricingLandTblData.DecreeType1Id.GetValueOrDefault() == AppEnums.DecreeEnum.ND_CP_99_2015 && pricingLandTblData.ApplyInvestmentRate.GetValueOrDefault() && !pricingLandTblData.IsMezzanine.GetValueOrDefault())
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
            List<ConstructionPrice> constructionPrices = (from pcp in (IQueryable<PricingConstructionPrice>)_context.PricingConstructionPricies
                                                          join cp in (IEnumerable<ConstructionPrice>)_context.ConstructionPricies on pcp.ConstructionPriceId equals cp.Id
                                                          where (int)pcp.Status != 99 && (int)cp.Status != 99 && pcp.PricingId == pricing.Id
                                                          select cp into x
                                                          orderby x.Year
                                                          select x).ToList();
            genPricingLandForTem5(document, pricing.DateCreate, list9, constructionPrices, pricing.Vat, pricing.ApartmentPrice, block.TypeReportApply, pricing.AreaCorrectionCoefficientId, block.ParentTypeReportApply);
            if (block.TypeReportApply != AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG)
            {
                        document.ReplaceAll("<apartmentPriceReducedNote>", pricing.ApartmentPriceReducedNote ?? ".....", (SearchOptions)0);
                List<PricingReducedPersonData> pricingReducedPeople = (from a in (IQueryable<PricingReducedPerson>)_context.PricingReducedPeople
                                                                       where a.PricingId == pricing.Id && (int)a.Status != 99
                                                                       select a into e
                                                                       select new PricingReducedPersonData
                                                                       {
                                                                           CustomerName = ((IQueryable<Customer>)_context.Customers).Where((Customer c) => c.Id == e.CustomerId).FirstOrDefault().FullName,
                                                                           Year = e.Year,
                                                                           Salary = e.Salary,
                                                                           DeductionCoefficient = e.DeductionCoefficient,
                                                                           Value = e.Value
                                                                       }).ToList();
                genReducedPersonTbl(document, pricingReducedPeople);
            }
                    document.ReplaceAll("<apartmentPriceReduced>", UtilsService.FormatMoney(pricing.ApartmentPriceReduced.GetValueOrDefault()), (SearchOptions)0);
                    document.ReplaceAll("<apartmentPrice>", UtilsService.FormatMoney(pricing.ApartmentPrice), (SearchOptions)0);
                    document.ReplaceAll("<ApartmentPriceRemaining>", UtilsService.FormatMoney(pricing.ApartmentPriceRemaining), (SearchOptions)0);
                    document.ReplaceAll("<apartmentPriceRemainingStr>", UtilsService.ConvertMoneyToString(pricing.ApartmentPriceRemaining.Value.ToString("N0", culture)).ToLower(), (SearchOptions)0);
                    document.ReplaceAll("<vat>", pricing.Vat.HasValue ? (pricing.Vat.Value.ToString("N2", culture) + "%") : "0%", (SearchOptions)0);
                    document.ReplaceAll("<apartmentPriceNoVat>", UtilsService.FormatMoney(pricing.ApartmentPriceNoVat), (SearchOptions)0);
                    document.ReplaceAll("<apartmentPriceVat>", UtilsService.FormatMoney(pricing.ApartmentPriceVat), (SearchOptions)0);
                    document.ReplaceAll("<landUsePlanningInfo>", block.LandUsePlanningInfo ?? "", (SearchOptions)0);
                    document.ReplaceAll("<highwayPlanningInfo>", block.HighwayPlanningInfo ?? "", (SearchOptions)0);
                    document.ReplaceAll("<landAcquisitionSituationInfo>", block.LandAcquisitionSituationInfo ?? "", (SearchOptions)0);
                    document.ReplaceAll("<landNo>", block.LandNo ?? "", (SearchOptions)0);
                    document.ReplaceAll("<mapNo>", block.MapNo ?? "", (SearchOptions)0);
            if (block.TypeReportApply == AppEnums.TypeReportApply.NHA_HO_CHUNG || block.TypeReportApply == AppEnums.TypeReportApply.NHA_CHUNG_CU || block.TypeReportApply == AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG)
            {
                        document.ReplaceAll("<alcav>", apartment.LandscapeAreaValue.HasValue ? apartment.LandscapeAreaValue.Value.ToString("N2", culture) : "0", (SearchOptions)0);
                        document.ReplaceAll("<alcav1>", apartment.LandscapeAreaValue1.HasValue ? apartment.LandscapeAreaValue1.Value.ToString("N2", culture) : "0", (SearchOptions)0);
                        document.ReplaceAll("<alcav2>", apartment.LandscapeAreaValue2.HasValue ? apartment.LandscapeAreaValue2.Value.ToString("N2", culture) : "0", (SearchOptions)0);
                        document.ReplaceAll("<alcav3>", apartment.LandscapeAreaValue3.HasValue ? apartment.LandscapeAreaValue3.Value.ToString("N2", culture) : "0", (SearchOptions)0);
            }
            else if (block.TypeReportApply == AppEnums.TypeReportApply.NHA_RIENG_LE)
            {
                        document.ReplaceAll("<alcav>", block.LandscapeAreaValue.HasValue ? block.LandscapeAreaValue.Value.ToString("N2", culture) : "0", (SearchOptions)0);
                        document.ReplaceAll("<alcav2>", block.LandscapePrivateAreaValue.HasValue ? block.LandscapePrivateAreaValue.Value.ToString("N2", culture) : "0", (SearchOptions)0);
                        document.ReplaceAll("<alcav1>", " ", (SearchOptions)0);
            }
            List<PricingApartmentLandDetail> list10 = (from a in (IQueryable<PricingApartmentLandDetail>)_context.PricingApartmentLandDetails
                                                       where a.PricingId == pricing.Id && (int)a.Status != 99
                                                       select a into x
                                                       orderby x.UpdatedAt
                                                       select x).ToList();
            foreach (PricingApartmentLandDetail item in list10)
            {
                        document.ReplaceAll("<buav>", item.PrivateArea.ToString(), (SearchOptions)0);
            }
            List<PricingApartmentLandDetailGroupByDecree> list11 = (from x in ((IQueryable<PricingApartmentLandDetail>)_context.PricingApartmentLandDetails).Where((PricingApartmentLandDetail a) => a.PricingId == pricing.Id && (int)a.Status != 99).ToList()
                                                                    group x by x.DecreeType1Id into x
                                                                    select new PricingApartmentLandDetailGroupByDecree
                                                                    {
                                                                        Decree = x.Key,
                                                                        pricingApartmentLandDetails = x.ToList()
                                                                    }).ToList();
            list11.ForEach(delegate (PricingApartmentLandDetailGroupByDecree pricingApartmentLandDetailGroupByDecree)
            {
                string text10 = "";
                float? num = null;
                int landPriceItemId = 0;
                double? num2 = null;
                string text11 = "";
                string landScapePrice = "";
                switch (pricingApartmentLandDetailGroupByDecree.Decree)
                {
                    case AppEnums.DecreeEnum.ND_CP_99_2015:
                        landPriceItemId = block.LandPriceItemId_99.GetValueOrDefault();
                        num2 = block.LandPriceItemValue_99;
                        text11 = block.PositionCoefficientStr_99;
                        num = block.LandPriceRefinement_99;
                                landScapePrice = UtilsService.FormatMoney(Convert.ToDecimal(num2)) + " đồng/m2 x " + text11 + (block.ExceedingLimitDeep.GetValueOrDefault() ? $" x (100 - {num})%" : "") + " = " + UtilsService.FormatMoney(Convert.ToDecimal(block.LandScapePrice_99)) + " đồng/m2";
                        break;
                    case AppEnums.DecreeEnum.ND_CP_34_2013:
                        landPriceItemId = block.LandPriceItemId_34.GetValueOrDefault();
                        num2 = block.LandPriceItemValue_34;
                        text11 = ((block.CaseApply_34.GetValueOrDefault() == AppEnums.TypeCaseApply_34.KHOAN_2) ? block.PositionCoefficientStr_34 : block.AlleyPositionCoefficientStr_34);
                        num = block.LandPriceRefinement_34;
                                landScapePrice = UtilsService.FormatMoney(Convert.ToDecimal(num2)) + " đồng/m2 x " + text11 + ((block.ExceedingLimitDeep.GetValueOrDefault() && block.CaseApply_34.GetValueOrDefault() == AppEnums.TypeCaseApply_34.KHOAN_2) ? $" x (100 - {num})%" : "") + " = " + UtilsService.FormatMoney(Convert.ToDecimal((block.CaseApply_34.GetValueOrDefault() == AppEnums.TypeCaseApply_34.KHOAN_2) ? block.LandScapePrice_34 : block.AlleyLandScapePrice_34)) + " đồng/m2";
                        break;
                    case AppEnums.DecreeEnum.ND_CP_61:
                        landPriceItemId = block.LandPriceItemId_61.GetValueOrDefault();
                        num2 = block.LandPriceItemValue_61;
                        text11 = block.PositionCoefficientStr_61;
                        num = block.LandPriceRefinement_61;
                        landScapePrice = ((!block.IsFrontOfLine_61.GetValueOrDefault() && (block.IsFrontOfLine_61.GetValueOrDefault() || block.IsAlley_61.GetValueOrDefault())) ? (UtilsService.FormatMoney(Convert.ToDecimal(block.No2LandScapePrice_61)) + " đồng/m2 x 0.8 = " + UtilsService.FormatMoney(Convert.ToDecimal(block.LandScapePrice_61)) + " đồng/m2") : (UtilsService.FormatMoney(Convert.ToDecimal(block.LandScapePrice_61)) + " đồng/m2"));
                        break;
                }
                LandPriceItem landPriceItem = ((IQueryable<LandPriceItem>)_context.LandPriceItems).Where((LandPriceItem l) => l.Id == landPriceItemId).FirstOrDefault();
                if (landPriceItem != null)
                {
                    text10 = landPriceItem.LaneName + ", đoạn từ " + landPriceItem.LaneStartName + ((landPriceItem.LaneEndName != null && landPriceItem.LaneEndName != "") ? (" đến đoạn " + landPriceItem.LaneEndName) : "");
                    text10 = ((landPriceItem.Des != null) ? (text10 + ", " + landPriceItem.Des) : text10);
                    LandPrice landPrice = ((IQueryable<LandPrice>)_context.LandPricies).Where((LandPrice l) => l.Id == landPriceItem.LandPriceId).FirstOrDefault();
                    if (landPrice != null)
                    {
                        text10 = text10 + ", " + landPrice.Des;
                    }
                }
                pricingApartmentLandDetailGroupByDecree.LandPriceItemNote = text10;
                pricingApartmentLandDetailGroupByDecree.LandPriceItemValue = Convert.ToDecimal(num2);
                pricingApartmentLandDetailGroupByDecree.LandScapePrice = landScapePrice;
                pricingApartmentLandDetailGroupByDecree.LandPriceRefinement = num;
                pricingApartmentLandDetailGroupByDecree.PositionCoefficientStr = text11;
            });
            if (list11.Count() > 0)
            {
                genApartmentLandDetailInfo(document, list11, block, pricing, list8, list, (apartment != null) ? apartment.LandscapeAreaValue1 : null);
            }
                    document.EndUpdate();
            val.SaveDocument(text2, DocumentFormat.OpenXml);
            return text2;
        }
        return null;
    }
    return null;
}

private void genPricingLandForTem5(Document document, DateTime? dateCreate, List<PricingLandTblData> pricingLandTblDatas, List<ConstructionPrice> constructionPrices, float? vat, decimal? price, AppEnums.TypeReportApply typeReportApply, int? AreaCorrectionCoefficientId, AppEnums.TypeReportApply? parentTypeReportApply)
{
    string text = "";
    foreach (ConstructionPrice constructionPrice in constructionPrices)
    {
        text = text + constructionPrice.Value.ToString("N2", culture) + "\n";
    }
            document.ReplaceAll("<constructionPrice>", text, (SearchOptions)0);
    string listPrice = "";
    string RemainingPrice = "";
    string Price = "";
    string MaintextureRateValue = "";
    string CoefficientUseValue = "";
    string CoefficientDistribution = "";
    string PriceInYear = "";
    if (typeReportApply == AppEnums.TypeReportApply.NHA_CHUNG_CU || (typeReportApply == AppEnums.TypeReportApply.BAN_PHAN_DIEN_TICH_SU_DUNG_CHUNG && parentTypeReportApply.GetValueOrDefault() == AppEnums.TypeReportApply.NHA_CHUNG_CU))
    {
        List<IGrouping<bool?, PricingLandTblData>> list = (from x in pricingLandTblDatas
                                                           group x by x.ApplyInvestmentRate into x
                                                           orderby x.Key descending
                                                           select x).ToList();
        list.ForEach(delegate (IGrouping<bool?, PricingLandTblData> groupPricingLandTblDatasByApplyInvestmentRate)
        {
            string text5 = (groupPricingLandTblDatasByApplyInvestmentRate.Key.GetValueOrDefault() ? "Trường hợp các phần diện tích đất áp dụng Suất vốn đầu tư\n" : "Trường hợp các phần diện tích đất không áp dụng Suất vốn đầu tư\n");
            List<IGrouping<int?, PricingLandTblData>> list4 = (from x in groupPricingLandTblDatasByApplyInvestmentRate.ToList()
                                                               group x by x.Level into x
                                                               orderby x.Key
                                                               select x).ToList();
            list4.ForEach(delegate (IGrouping<int?, PricingLandTblData> groupPricingLandTblDatasBylevelItem)
            {
                List<PricingLandTblData> list5 = groupPricingLandTblDatasBylevelItem.ToList();
                list5.ForEach(delegate (PricingLandTblData childPricingLandTblData)
                {
                    listPrice = ((listPrice == "") ? (UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice) ?? "") : (listPrice + " + " + UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice)));
                    if (childPricingLandTblData.ApplyInvestmentRate.GetValueOrDefault())
                    {
                        RemainingPrice = RemainingPrice + UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice) + "\n";
                        Price = Price + UtilsService.FormatMoney(childPricingLandTblData.Price) + "\n";
                        PriceInYear = PriceInYear + UtilsService.FormatMoney(childPricingLandTblData.PriceInYear) + "\n";
                                MaintextureRateValue += $"{childPricingLandTblData.MaintextureRateValue}\n";
                                CoefficientUseValue += $"{childPricingLandTblData.CoefficientUseValue}\n";
                                CoefficientDistribution += $"{childPricingLandTblData.CoefficientDistribution}\n";
                        string text6 = "";
                        InvestmentRateItem investmentRateItem = ((IQueryable<InvestmentRateItem>)_context.InvestmentRateItems).Where((InvestmentRateItem i) => (int?)i.Id == childPricingLandTblData.InvestmentRateItemId).FirstOrDefault();
                        if (investmentRateItem != null)
                        {
                            InvestmentRate investmentRate = ((IQueryable<InvestmentRate>)_context.InvestmentRaties).Where((InvestmentRate i) => i.Id == investmentRateItem.Id).FirstOrDefault();
                            if (investmentRate != null)
                            {
                                text6 = "\nCăn cứ dòng số " + investmentRateItem.LineInfo + " chi tiết suất vốn đầu tư " + investmentRateItem.DetailInfo + ". Suất vốn đầu tư: " + UtilsService.FormatMoney((decimal)investmentRateItem.Value) + " đồng/m2, Chi phí xây dựng: " + UtilsService.FormatMoney((decimal)investmentRateItem.Value1) + " đồng/m2, Chi phí thiết bị: " + UtilsService.FormatMoney((decimal)investmentRateItem.Value2) + " đồng/m2";
                            }
                        }
                        AreaCorrectionCoefficient areaCorrectionCoefficient = ((IQueryable<AreaCorrectionCoefficient>)_context.AreaCorrectionCoefficients).Where((AreaCorrectionCoefficient a) => (int?)a.Id == AreaCorrectionCoefficientId).FirstOrDefault();
                        if (areaCorrectionCoefficient != null)
                        {
                            string text7 = areaCorrectionCoefficient.Note + "\nHệ số điều chỉnh vùng: " + areaCorrectionCoefficient.Value.ToString("N3", culture);
                        }
                    }
                    else
                    {
                        RemainingPrice = RemainingPrice + UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice) + "\n";
                        Price = Price + UtilsService.FormatMoney(childPricingLandTblData.Price) + "\n";
                        PriceInYear = PriceInYear + UtilsService.FormatMoney(childPricingLandTblData.PriceInYear) + "\n";
                                MaintextureRateValue += $"{childPricingLandTblData.MaintextureRateValue}\n";
                                CoefficientUseValue += $"{childPricingLandTblData.CoefficientUseValue}\n";
                                CoefficientDistribution += $"{childPricingLandTblData.CoefficientDistribution}\n";
                    }
                    string text8 = "";
                    float num2 = childPricingLandTblData.GeneralArea.GetValueOrDefault() + childPricingLandTblData.PrivateArea.GetValueOrDefault();
                    string text9 = (childPricingLandTblData.CoefficientUseValue.HasValue ? childPricingLandTblData.CoefficientUseValue.Value.ToString("N2", culture) : "");
                });
            });
        });
    }
    else
    {
        List<IGrouping<int?, PricingLandTblData>> list2 = (from x in pricingLandTblDatas
                                                           group x by x.Level into x
                                                           orderby x.Key
                                                           select x).ToList();
        list2.ForEach(delegate (IGrouping<int?, PricingLandTblData> groupPricingLandTblDatasBylevelItem)
        {
            List<PricingLandTblData> list3 = groupPricingLandTblDatasBylevelItem.ToList();
            list3.ForEach(delegate (PricingLandTblData childPricingLandTblData)
            {
                listPrice = ((listPrice == "") ? (UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice) ?? "") : (listPrice + " + " + UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice)));
                string text2 = (childPricingLandTblData.MaintextureRateValue.HasValue ? childPricingLandTblData.MaintextureRateValue.Value.ToString("N2", culture) : "");
                string text3 = "";
                float num = childPricingLandTblData.GeneralArea.GetValueOrDefault() + childPricingLandTblData.PrivateArea.GetValueOrDefault();
                string text4 = (childPricingLandTblData.CoefficientUseValue.HasValue ? childPricingLandTblData.CoefficientUseValue.Value.ToString("N2", culture) : "");
                if (childPricingLandTblData.TermApply.GetValueOrDefault() == AppEnums.TermApply.DIEU_65 || childPricingLandTblData.TermApply.GetValueOrDefault() == AppEnums.TermApply.KHOAN_1_DIEU_34 || childPricingLandTblData.TermApply.GetValueOrDefault() == AppEnums.TermApply.DIEU_7)
                {
                    text3 = "\t" + UtilsService.FormatMoney(childPricingLandTblData.PriceInYear) + " đồng/m2 x " + text2 + "% x " + num.ToString("N2", culture) + " m2 x " + text4 + " = " + UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice) + "\n";
                }
                else if (childPricingLandTblData.TermApply.GetValueOrDefault() == AppEnums.TermApply.DIEU_70 || childPricingLandTblData.TermApply.GetValueOrDefault() == AppEnums.TermApply.KHOAN_2_DIEU_34 || childPricingLandTblData.TermApply.GetValueOrDefault() == AppEnums.TermApply.DIEU_35 || childPricingLandTblData.TermApply.GetValueOrDefault() == AppEnums.TermApply.DIEU_71)
                {
                    text3 = "\t" + UtilsService.FormatMoney(childPricingLandTblData.PriceInYear) + " đồng/m2 x " + text2 + "% x " + num.ToString("N2", culture) + " m2 = " + UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice) + "\n";
                }
                RemainingPrice = RemainingPrice + UtilsService.FormatMoney(childPricingLandTblData.RemainingPrice) + "\n";
                Price = Price + UtilsService.FormatMoney(childPricingLandTblData.Price) + "\n";
                PriceInYear = PriceInYear + UtilsService.FormatMoney(childPricingLandTblData.PriceInYear) + "\n";
                        MaintextureRateValue += $"{childPricingLandTblData.MaintextureRateValue}\n";
                        CoefficientUseValue += $"{childPricingLandTblData.CoefficientUseValue}\n";
                        CoefficientDistribution += $"{childPricingLandTblData.CoefficientDistribution}\n";
            });
        });
    }
            document.ReplaceAll("<RemainingPrice>", RemainingPrice ?? "", (SearchOptions)0);
            document.ReplaceAll("<Price>", Price ?? "", (SearchOptions)0);
            document.ReplaceAll("<PriceInYear>", PriceInYear ?? "", (SearchOptions)0);
            document.ReplaceAll("<MaintextureRateValue>", MaintextureRateValue ?? "", (SearchOptions)0);
            document.ReplaceAll("<CoefficientUseValue>", CoefficientUseValue ?? "", (SearchOptions)0);
            document.ReplaceAll("<CoefficientDistribution>", CoefficientDistribution ?? "", (SearchOptions)0);
}

private void genReducedPersonTbl(Document document, List<PricingReducedPersonData> pricingReducedPeople)
{
    string text = "";
    string text2 = "";
    string text3 = "";
    string text4 = "";
    foreach (PricingReducedPersonData pricingReducedPerson in pricingReducedPeople)
    {
        string str = (pricingReducedPerson.DeductionCoefficient.HasValue ? pricingReducedPerson.DeductionCoefficient.Value.ToString("N2", culture) : "");
        text = text + pricingReducedPerson.CustomerName + "\n";
        text2 += str;
        text3 = text3 + UtilsService.FormatMoney(pricingReducedPerson.Salary) + "\n";
                text4 += $"{pricingReducedPerson.Year}";
            }
            document.ReplaceAll("<CustomerName>", text, (SearchOptions)0);
            document.ReplaceAll("<DeductionCoefficient>", text2, (SearchOptions)0);
            document.ReplaceAll("<CustumerSalary>", text3, (SearchOptions)0);
            document.ReplaceAll("<CustomerYear>", text4, (SearchOptions)0);
        }
    }
}
