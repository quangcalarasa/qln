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
using System.Security.Claims;
using IOITQln.Common.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using GroupData = IOITQln.Models.Data.GroupData;

namespace IOITQln.Controllers.ApiNoc
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ExportWordTTController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("ExportWordTT", "ExportWordTT");
        private static string functionCode = "PRICING";

        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;

        public ExportWordTTController(ApiDbContext context, IMapper mapper, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        [HttpPost("GetExportWordTT/{id}/{RentFileBCTId}")]
        public async Task<IActionResult> GetExportWordTT(Guid id, int RentFileBCTId)
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            var token = _context.Tokens.FirstOrDefault(t => Convert.ToString(t.AccessToken) == accessToken);
            if (token == null)
            {
                return Unauthorized();
            }

            var def = new DefaultResponse();

            var identity = (ClaimsIdentity)User.Identity;
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.EXPORT))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }

            try
            {
                string path = insertDataToTemplate(id, RentFileBCTId);
                if (path != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        var httpResponseMessage = new HttpResponseMessage();
                        string fileName = "phieu_chiet_tinh-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";

                        using (var file = new FileStream(path, FileMode.Open, FileAccess.Read))
                        {
                            file.CopyTo(ms);
                        }

                        try { System.IO.File.Delete(path); } catch { /* ignore */ }

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

        private string insertDataToTemplate(Guid id, int RentFileBCTId)
        {
            var rentFile = _context.RentFiles.FirstOrDefault(l => l.Id == id && l.Status != AppEnums.EntityStatus.DELETED);
            if (rentFile == null) return null;

            var map_rentFlieData = _mapper.Map<RentFlieData>(rentFile);

            var blocks = _context.Blocks.FirstOrDefault(l => l.Id == rentFile.RentBlockId && l.Status != AppEnums.EntityStatus.DELETED);
            var map_blocks = _mapper.Map<BlockData>(blocks);
            map_blocks.DistrictName = _context.Districts.Where(l => l.Id == map_blocks.District && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Name).FirstOrDefault();
            map_blocks.LaneName = _context.Lanies.Where(l => l.Id == map_blocks.Lane && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Name).FirstOrDefault();
            map_blocks.WardName = _context.Wards.Where(l => l.Id == map_blocks.Ward && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Name).FirstOrDefault();

            string templatePath = Path.Combine(_hostingEnvironment.WebRootPath, "exportword/phieu_chiet_tinh.docx");
            string fileName = "result" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".docx";

            var wordProcessor = new RichEditDocumentServer();
            wordProcessor.LoadDocumentTemplate(templatePath, DocumentFormat.OpenXml);
            var document = wordProcessor.Document;

            document.BeginUpdate();

            int dayNow = DateTime.Now.Day;
            int monthNow = DateTime.Now.Month;
            int yearNow = DateTime.Now.Year;

            string day = dayNow.ToString();
            string month = monthNow.ToString();
            string year = yearNow.ToString();

            document.ReplaceAll("<day>", day, SearchOptions.None);
            document.ReplaceAll("<month>", month, SearchOptions.None);
            document.ReplaceAll("<year>", year, SearchOptions.None);
            document.ReplaceAll("<dateTimeNow>", $"{day}/{month}/{year}", SearchOptions.None);

            // Thông tin căn nhà
            var customerName = _context.Customers.Where(l => l.Id == rentFile.CustomerId && l.Status != AppEnums.EntityStatus.DELETED)
                                                 .Select(p => p.FullName).FirstOrDefault();
            document.ReplaceAll("<CustomerName>", !string.IsNullOrEmpty(customerName) ? customerName : "✓", SearchOptions.None);

            if (rentFile.RentApartmentId != 0)
            {
                var apartment = _context.Apartments.FirstOrDefault(l => l.Id == rentFile.RentApartmentId && l.Status != AppEnums.EntityStatus.DELETED);
                var map_apartments = _mapper.Map<ApartmentData>(apartment);

                // (Khôi phục đúng theo file gốc)
                document.ReplaceAll("<numberAdr>", !string.IsNullOrEmpty(apartment.Address) ? blocks.Address : "✓", SearchOptions.None);

                var block = _context.Blocks.FirstOrDefault(l => l.Id == apartment.BlockId && l.Status != AppEnums.EntityStatus.DELETED);

                var lane = _context.Lanies.Where(l => l.Id == block.Lane && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Name).FirstOrDefault();
                var ward = _context.Wards.Where(l => l.Id == block.Ward && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Name).FirstOrDefault();
                var district = _context.Districts.Where(l => l.Id == block.District && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Name).FirstOrDefault();

                document.ReplaceAll("<lane>", !string.IsNullOrEmpty(lane) ? lane : "✓", SearchOptions.None);
                document.ReplaceAll("<ward>", !string.IsNullOrEmpty(ward) ? ward : "✓", SearchOptions.None);
                document.ReplaceAll("<district>", !string.IsNullOrEmpty(district) ? district : "✓", SearchOptions.None);
            }
            else
            {
                document.ReplaceAll("<numberAdr>", !string.IsNullOrEmpty(blocks.Address) ? blocks.Address : "✓", SearchOptions.None);
                document.ReplaceAll("<lane>", !string.IsNullOrEmpty(map_blocks.LaneName) ? map_blocks.LaneName : "✓", SearchOptions.None);
                document.ReplaceAll("<ward>", !string.IsNullOrEmpty(map_blocks.WardName) ? map_blocks.WardName : "✓", SearchOptions.None);
                document.ReplaceAll("<district>", !string.IsNullOrEmpty(map_blocks.DistrictName) ? map_blocks.DistrictName : "✓", SearchOptions.None);
            }

            // Mốc thời gian điều khoản
            DateTime date1753_End = new DateTime(2007, 1, 18);
            DateTime date09_Start = new DateTime(2007, 1, 19);
            DateTime date09_End = new DateTime(2018, 7, 11);
            DateTime date22_Start = new DateTime(2018, 7, 12);

            var rentFileBCT = _context.RentFileBCTs.Where(l => l.Id == RentFileBCTId && l.TypeBCT == 2)
                                                    .OrderByDescending(p => p.CreatedAt).FirstOrDefault();

            var groupDatas = new List<groupData>();

            if (rentFileBCT != null)
            {
                if (rentFileBCT.DateEnd <= date1753_End)
                {
                    var g1 = getDataBCT1753(id, RentFileBCTId);
                    var gB = getDataBCT22HD(id, RentFileBCTId);
                    if (g1 != null) groupDatas.AddRange(g1);
                    if (gB != null) groupDatas.AddRange(gB);
                }

                if (rentFileBCT.DateStart >= date09_Start && rentFileBCT.DateEnd <= date09_End)
                {
                    var g2 = getDataBCT09(id, RentFileBCTId);
                    var gB = getDataBCT22HD(id, RentFileBCTId);
                    if (g2 != null) groupDatas.AddRange(g2);
                    if (gB != null) groupDatas.AddRange(gB);
                }

                if (rentFileBCT.DateStart >= date22_Start)
                {
                    var g3 = getDataBCT22(id, RentFileBCTId);
                    var gB = getDataBCT22HD(id, RentFileBCTId);
                    if (g3 != null) groupDatas.AddRange(g3);
                    if (gB != null) groupDatas.AddRange(gB);
                }

                if (rentFileBCT.DateStart <= date1753_End && rentFileBCT.DateEnd <= date09_End)
                {
                    var g1 = getDataBCT1753(id, RentFileBCTId);
                    var g2 = getDataBCT09(id, RentFileBCTId);
                    var gB = getDataBCT22HD(id, RentFileBCTId);
                    if (g1 != null) groupDatas.AddRange(g1);
                    if (g2 != null) groupDatas.AddRange(g2);
                    if (gB != null) groupDatas.AddRange(gB);
                }

                if (rentFileBCT.DateStart > date1753_End && rentFileBCT.DateStart <= date09_End && rentFileBCT.DateEnd >= date22_Start)
                {
                    var g2 = getDataBCT09(id, RentFileBCTId);
                    var g3 = getDataBCT22(id, RentFileBCTId);
                    if (g2 != null) groupDatas.AddRange(g2);
                    groupDatas.AddRange(g3);

                    var gB = getDataBCT22HD(id, RentFileBCTId);
                    if (gB != null) groupDatas.AddRange(gB);
                }

                if (rentFileBCT.DateStart <= date1753_End && rentFileBCT.DateEnd >= date22_Start)
                {
                    var g1 = getDataBCT1753(id, RentFileBCTId);
                    var g2 = getDataBCT09(id, RentFileBCTId);
                    var g3 = getDataBCT22(id, RentFileBCTId);
                    var gB = getDataBCT22HD(id, RentFileBCTId);
                    if (g1 != null) groupDatas.AddRange(g1);
                    if (g2 != null) groupDatas.AddRange(g2);
                    groupDatas.AddRange(g3);
                    if (gB != null) groupDatas.AddRange(gB);
                }
            }

            string areaStr = rentFileBCT?.Area != null ? rentFileBCT.Area.ToString() : "0";
            document.ReplaceAll("<Area>", areaStr, SearchOptions.None);

            double discount = _context.DiscountCoefficients
                                      .Where(l => l.Id == rentFileBCT.DiscountCoefficientId && l.Status != AppEnums.EntityStatus.DELETED)
                                      .Select(p => p.Value).FirstOrDefault();

            // Xuất bảng/chi tiết
            genListWorkSheetTblVer2(document, groupDatas, id, discount);

            document.EndUpdate();
            wordProcessor.SaveDocument(fileName, DocumentFormat.OpenXml);
            return fileName;
        }

        // ====== Giữ nguyên các hàm nghiệp vụ gốc đã có trong dự án ======
        // Các hàm dưới đây đã tồn tại trong bản _dichnguoc (decompile), chỉ cần giữ chữ ký giống bản gốc.
        // Nội dung implementation giữ nguyên như hiện có trong dự án của bạn.

        private List<groupData> getDataBCT1753(Guid id, int RentFileBCTId)
        {
            RentFile rentFile = _context.RentFiles.Where(l => l.Id == id && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

            RentFlieData map_rentFlieData = _mapper.Map<RentFlieData>(rentFile);

            //Tìm bảng chiết tính mới nhất theo ID hồ sơ
            RentFileBCT rentFileBCT = _context.RentFileBCTs.Where(l => l.Id == RentFileBCTId && l.TypeBCT == 2).OrderByDescending(p => p.CreatedAt).FirstOrDefault();

            RentFileBCTData map_rentFlieBCTDatas = _mapper.Map<RentFileBCTData>(rentFileBCT);

            List<DefaultCoefficient> dataDf = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderBy(p => p.CoefficientName).ToList();
            var groupedData = dataDf.GroupBy(x => x.CoefficientName).Select(e => new GroupData
            {
                CoefficientId = e.Key,
                defaultCoefficients = e.ToList(),
                childDefaultCoefficients = _context.ChildDefaults.Where(l => l.RentFileBctId == map_rentFlieBCTDatas.Id && l.CoefficientId == e.Key && l.Type == 2 && l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList()
            }).ToList();
            map_rentFlieBCTDatas.groupData = groupedData;

            List<Vat> vats = _context.Vats.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList();


            List<Salary> salaryCoefficients = _context.Salaries.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList();

            List<Coefficient> coefficients = _context.Coefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderBy(p => p.DoApply).ToList();

            List<Conversion> conversions = _context.Conversions.Where(l => l.TypeQD == AppEnums.TypeQD.QD_1753).ToList();

            List<DefaultCoefficient> defaultCoefficients = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
            Block blocks = _context.Blocks.Where(l => l.Id == rentFile.RentBlockId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

            List<RentingPrice> rentingPrices = _context.RentingPricies.Where(l => l.TypeQD == AppEnums.TypeQD.QD_1753 && l.TypeBlockId == blocks.TypeBlockId && l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.EffectiveDate).ToList();

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


            List<BCT> resBct = new List<BCT>();

            if (rentFile.RentApartmentId > 0)
            {
                Apartment apartment = _context.Apartments.Where(l => l.Id == rentFile.RentApartmentId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                ApartmentData map_apartments = _mapper.Map<ApartmentData>(apartment);
                List<ApartmentDetail> apartmentDetails = _context.ApartmentDetails.Where(a => a.TargetId == rentFile.RentApartmentId && a.Type == TypeApartmentDetail.APARTMENT && a.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                List<ApartmentDetailData> apartmentDetailData = _mapper.Map<List<ApartmentDetailData>>(apartmentDetails);
                foreach (ApartmentDetailData map_apartmentDetail in apartmentDetailData)
                {
                    Area area = _context.Areas.Where(f => f.Id == map_apartmentDetail.AreaId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    map_apartmentDetail.AreaName = area != null ? area.Name : "";
                    map_apartmentDetail.IsMezzanine = area != null ? area.IsMezzanine : null;

                    Floor floor = _context.Floors.Where(f => f.Id == map_apartmentDetail.FloorId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    map_apartmentDetail.FloorName = floor != null ? floor.Name : "";
                    map_apartmentDetail.FloorCode = floor != null ? floor.Code : 0;
                }
                map_apartments.apartmentDetailData = apartmentDetailData.ToList();

                foreach (var item in map_apartments.apartmentDetailData)
                {
                    var bCT = new BCT();
                    List<changeValue> resChange = new List<changeValue>();
                    List<changeK> resChangeK = new List<changeK>();

                    int k = 0;

                    bCT.TypeQD = 1753;

                    bCT.GroupId = k;

                    bCT.AreaName = item.AreaName;

                    bCT.Level = (int)item.Level;

                    bCT.PrivateArea = (float?)rentFileBCT.Area;

                    bCT.DateCoefficient = (DateTime)item.DisposeTime;

                    bCT.DateStart = rentFileBCT.DateStart;

                    bCT.DiscountCoff = (decimal)_context.DiscountCoefficients.Where(p => p.Id == rentFileBCT.DiscountCoefficientId && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();

                    DateTime date = new DateTime(2007, 1, 19);
                    if (rentFileBCT.DateEnd > date)
                    {
                        bCT.DateEnd = date;
                    }
                    else
                    {
                        bCT.DateEnd = rentFileBCT.DateEnd;
                    }

                    //tháng
                    if (bCT.DateStart.HasValue && bCT.DateEnd.HasValue)
                    {
                        bCT.MonthDiff = (bCT.DateEnd.Value.Year - bCT.DateStart.Value.Year) * 12 + (bCT.DateEnd.Value.Month - bCT.DateStart.Value.Month);
                    }
                    // ngày
                    if (bCT.DateEnd.HasValue && bCT.DateStart.HasValue)
                    {
                        // Lấy DateTime bên trong kiểu Nullable DateTime
                        DateTime start = bCT.DateStart.Value.AddMonths(bCT.MonthDiff);
                        DateTime end = bCT.DateEnd.Value;
                        // Lấy TimeSpan giữa hai ngày
                        TimeSpan span = end.Subtract(start);
                        // Lấy số ngày chênh lệch
                        bCT.DateDiff = (int)span.TotalDays;
                    }
                    if (bCT.DateDiff < 0)
                    {
                        bCT.DateDiff = 30 + bCT.DateDiff;
                        bCT.MonthDiff--;
                    }

                    foreach (var itemLv in rentingPrices) //Lấy giá chuẩn
                    {
                        if (itemLv.LevelId == item.Level)
                        {
                            bCT.StandardPrice = (decimal)itemLv.Price;
                            break;
                        }
                    }


                    foreach (var vat in vats)
                    {
                        if (bCT.DateStart >= vat.DoApply)
                        {
                            bCT.VAT = vat.Value;
                            break;
                        }
                        else if (vat.DoApply <= bCT.DateEnd)
                        {
                            var changevalue = new changeValue();

                            changevalue.Type = 1;
                            changevalue.valueChange = vat.Value;
                            changevalue.DateChange = vat.DoApply;

                            resChange.Add(changevalue);
                            continue;
                        }
                    }

                    bCT.chilDfs = new List<chilDf>();
                    foreach (var itemDf in map_rentFlieBCTDatas.groupData)
                    {
                        if (itemDf.childDefaultCoefficients.Count > 0)
                        {
                            foreach (var itemChild in itemDf.childDefaultCoefficients)
                            {

                                if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                {
                                    string name = "";
                                    if (item.FloorCode < 6)
                                    {
                                        name = bCT.AreaName;
                                    }
                                    else
                                    {
                                        name = "Tầng 6 trở lên";
                                    }
                                    string content = _context.DefaultCoefficients.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Content).FirstOrDefault();
                                    if (content == name)
                                    {
                                        if (bCT.DateStart >= itemChild.DoApply)
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;

                                            itemChilDfs.Value = itemChild.Value;
                                            bCT.chilDfs.Add(itemChilDfs);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    if (bCT.DateStart >= itemChild.DoApply)
                                    {
                                        var itemChilDfs = new chilDf();
                                        itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                        itemChilDfs.Value = itemChild.Value;
                                        bCT.chilDfs.Add(itemChilDfs);
                                        break;
                                    }

                                }
                            }
                        }
                    }


                    if (bCT.StandardPrice == null) bCT.StandardPrice = 0;

                    if (bCT.chilDfs != null)
                    {
                        bCT.TotalK = (float)bCT.chilDfs.Sum(p => p.Value) + 1;
                    }
                    else
                    {
                        bCT.TotalK = 1;
                    }

                    if (bCT.Ktlcb == null) bCT.Ktlcb = 1;
                    if (bCT.Ktdbt == null) bCT.Ktdbt = 1;

                    bCT.TotalK = (float)bCT.chilDfs.Sum(p => p.Value) + 1;
                    bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * (decimal)bCT.TotalK));
                    bCT.PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea));
                    bCT.Unit = "VNĐ";
                    resBct.Add(bCT);

                    if (resChange.Count > 0)
                    {
                        foreach (var itemChange in resChange.OrderBy(p => p.DateChange).ToList())
                        {
                            k = k + 1;
                            decimal? PriceRent1m2 = 0;
                            decimal? PriceRent = 0;

                            var changeData = new BCT();
                            changeData.TypeQD = 1753;
                            changeData.check = false;
                            changeData.GroupId = k;

                            changeData.AreaName = item.AreaName;

                            changeData.Level = (int)item.Level;

                            changeData.DateCoefficient = item.DisposeTime;

                            changeData.PrivateArea = (float)rentFileBCT.Area;

                            changeData.VAT = resBct[resBct.Count - 1].VAT;
                            changeData.Ktdbt = resBct[resBct.Count - 1].Ktdbt;
                            changeData.Ktlcb = resBct[resBct.Count - 1].Ktlcb;
                            changeData.StandardPrice = resBct[resBct.Count - 1].StandardPrice;
                            changeData.Unit = resBct[resBct.Count - 1].Unit;

                            if (itemChange.Type == 1)
                            {
                                changeData.VAT = itemChange.valueChange;
                                changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                if (resBct.Count > 0)
                                {
                                    changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                    changeData.DateStart = itemChange.DateChange;
                                    resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);

                                    //tháng
                                    if (resBct[resBct.Count - 1].DateStart.HasValue && resBct[resBct.Count - 1].DateEnd.HasValue)
                                    {
                                        resBct[resBct.Count - 1].MonthDiff = (resBct[resBct.Count - 1].DateEnd.Value.Year - resBct[resBct.Count - 1].DateStart.Value.Year) * 12 + (resBct[resBct.Count - 1].DateEnd.Value.Month - resBct[resBct.Count - 1].DateStart.Value.Month);
                                    }
                                    // ngày
                                    if (resBct[resBct.Count - 1].DateEnd.HasValue && resBct[resBct.Count - 1].DateStart.HasValue)
                                    {
                                        // Lấy DateTime bên trong kiểu Nullable DateTime
                                        DateTime start = resBct[resBct.Count - 1].DateStart.Value.AddMonths(resBct[resBct.Count - 1].MonthDiff);
                                        DateTime end = resBct[resBct.Count - 1].DateEnd.Value;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan span = end.Subtract(start);
                                        // Lấy số ngày chênh lệch
                                        resBct[resBct.Count - 1].DateDiff = (int)span.TotalDays;
                                    }
                                    if (resBct[resBct.Count - 1].DateDiff < 0)
                                    {
                                        resBct[resBct.Count - 1].DateDiff = 30 + resBct[resBct.Count - 1].DateDiff;
                                        resBct[resBct.Count - 1].MonthDiff--;
                                    }
                                }

                                PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * (decimal)changeData.TotalK));

                                PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                changeData.PriceRent1m2 = PriceRent1m2;
                                changeData.PriceRent = PriceRent;

                                //tháng
                                if (changeData.DateStart.HasValue && changeData.DateEnd.HasValue)
                                {
                                    changeData.MonthDiff = (changeData.DateEnd.Value.Year - changeData.DateStart.Value.Year) * 12 + (changeData.DateEnd.Value.Month - changeData.DateStart.Value.Month);
                                }
                                // ngày
                                if (changeData.DateEnd.HasValue && changeData.DateStart.HasValue)
                                {
                                    // Lấy DateTime bên trong kiểu Nullable DateTime
                                    DateTime start = changeData.DateStart.Value.AddMonths(changeData.MonthDiff);
                                    DateTime end = changeData.DateEnd.Value;
                                    // Lấy TimeSpan giữa hai ngày
                                    TimeSpan span = end.Subtract(start);
                                    // Lấy số ngày chênh lệch
                                    changeData.DateDiff = (int)span.TotalDays;
                                }
                                if (changeData.DateDiff < 0)
                                {
                                    changeData.DateDiff = 30 + changeData.DateDiff;
                                    changeData.MonthDiff--;
                                }
                                resBct.Add(changeData);
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var item in map_blocks.blockDetails)
                {
                    int k = 0;
                    List<changeValue> resChange = new List<changeValue>();
                    List<changeK> resChangeK = new List<changeK>();
                    var bCT = new BCT();
                    bCT.TypeQD = 1753;

                    bCT.GroupId = k;

                    bCT.AreaName = item.AreaName;

                    bCT.Level = (int)item.Level;

                    bCT.PrivateArea = (float?)rentFileBCT.Area;

                    bCT.DateCoefficient = (DateTime)item.DisposeTime;

                    bCT.PolicyReduction = (decimal)_context.DiscountCoefficients.Where(p => p.Id == rentFileBCT.DiscountCoefficientId && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();


                    bCT.DateStart = rentFileBCT.DateStart;

                    DateTime date = new DateTime(2007, 1, 19);
                    if (rentFileBCT.DateEnd > date)
                    {
                        bCT.DateEnd = date;
                    }
                    else
                    {
                        bCT.DateEnd = rentFileBCT.DateEnd;
                    }

                    //tháng
                    if (bCT.DateStart.HasValue && bCT.DateEnd.HasValue)
                    {
                        bCT.MonthDiff = (bCT.DateEnd.Value.Year - bCT.DateStart.Value.Year) * 12 + (bCT.DateEnd.Value.Month - bCT.DateStart.Value.Month);
                    }
                    // ngày
                    if (bCT.DateEnd.HasValue && bCT.DateStart.HasValue)
                    {
                        // Lấy DateTime bên trong kiểu Nullable DateTime
                        DateTime start = bCT.DateStart.Value.AddMonths(bCT.MonthDiff);
                        DateTime end = bCT.DateEnd.Value;
                        // Lấy TimeSpan giữa hai ngày
                        TimeSpan span = end.Subtract(start);
                        // Lấy số ngày chênh lệch
                        bCT.DateDiff = (int)span.TotalDays;
                    }
                    if (bCT.DateDiff < 0)
                    {
                        bCT.DateDiff = 30 + bCT.DateDiff;
                        bCT.MonthDiff--;
                    }


                    foreach (var itemLv in rentingPrices) //Lấy giá chuẩn
                    {
                        if (itemLv.LevelId == item.Level)
                        {
                            bCT.StandardPrice = (decimal)itemLv.Price;
                            break;
                        }
                    }

                    foreach (var vat in vats)
                    {
                        if (bCT.DateStart >= vat.DoApply)
                        {
                            bCT.VAT = vat.Value;
                            break;
                        }
                        else if (vat.DoApply <= bCT.DateEnd)
                        {
                            var changevalue = new changeValue();

                            changevalue.Type = 1;
                            changevalue.valueChange = vat.Value;
                            changevalue.DateChange = vat.DoApply;

                            resChange.Add(changevalue);
                            continue;
                        }
                    }

                    bCT.chilDfs = new List<chilDf>();
                    foreach (var itemDf in map_rentFlieBCTDatas.groupData)
                    {
                        if (itemDf.childDefaultCoefficients.Count > 0)
                        {
                            foreach (var itemChild in itemDf.childDefaultCoefficients)
                            {

                                if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                {
                                    string name = "";
                                    if (item.FloorCode < 6)
                                    {
                                        name = bCT.AreaName;
                                    }
                                    else
                                    {
                                        name = "Tầng 6 trở lên";
                                    }
                                    string content = _context.DefaultCoefficients.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Content).FirstOrDefault();
                                    if (content == name)
                                    {
                                        if (bCT.DateStart >= itemChild.DoApply)
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;

                                            itemChilDfs.Value = itemChild.Value;
                                            bCT.chilDfs.Add(itemChilDfs);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    if (bCT.DateStart >= itemChild.DoApply)
                                    {
                                        var itemChilDfs = new chilDf();
                                        itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                        itemChilDfs.Value = itemChild.Value;
                                        bCT.chilDfs.Add(itemChilDfs);
                                        break;
                                    }

                                }
                            }
                        }
                    }


                    if (bCT.StandardPrice == null) bCT.StandardPrice = 0;

                    if (bCT.chilDfs != null)
                    {
                        bCT.TotalK = (float)bCT.chilDfs.Sum(p => p.Value) + 1;
                    }
                    else
                    {
                        bCT.TotalK = 1;
                    }

                    if (bCT.Ktlcb == null) bCT.Ktlcb = 1;
                    if (bCT.Ktdbt == null) bCT.Ktdbt = 1;

                    bCT.TotalK = (float)bCT.chilDfs.Sum(p => p.Value) + 1;
                    bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * (decimal)bCT.TotalK));
                    bCT.PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea));
                    bCT.Unit = "VNĐ";
                    resBct.Add(bCT);

                    if (resChange.Count > 0)
                    {
                        foreach (var itemChange in resChange.OrderBy(p => p.DateChange).ToList())
                        {
                            k = k + 1;
                            decimal? PriceRent1m2 = 0;
                            decimal? PriceRent = 0;

                            var changeData = new BCT();
                            changeData.TypeQD = 1753;
                            changeData.check = false;
                            changeData.GroupId = k;

                            changeData.AreaName = item.AreaName;

                            changeData.Level = (int)item.Level;

                            changeData.DateCoefficient = item.DisposeTime;

                            changeData.PrivateArea = (float)rentFileBCT.Area;

                            changeData.TotalK = resBct[resBct.Count - 1].TotalK;

                            changeData.VAT = resBct[resBct.Count - 1].VAT;

                            changeData.StandardPrice = resBct[resBct.Count - 1].StandardPrice;
                            changeData.Unit = resBct[resBct.Count - 1].Unit;
                            if (itemChange.Type == 1)
                            {
                                changeData.VAT = itemChange.valueChange;
                                changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                if (resBct.Count > 0)
                                {
                                    changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                    changeData.DateStart = itemChange.DateChange;
                                    resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);

                                    //tháng
                                    if (resBct[resBct.Count - 1].DateStart.HasValue && resBct[resBct.Count - 1].DateEnd.HasValue)
                                    {
                                        resBct[resBct.Count - 1].MonthDiff = (resBct[resBct.Count - 1].DateEnd.Value.Year - resBct[resBct.Count - 1].DateStart.Value.Year) * 12 + (resBct[resBct.Count - 1].DateEnd.Value.Month - resBct[resBct.Count - 1].DateStart.Value.Month);
                                    }
                                    // ngày
                                    if (resBct[resBct.Count - 1].DateEnd.HasValue && resBct[resBct.Count - 1].DateStart.HasValue)
                                    {
                                        // Lấy DateTime bên trong kiểu Nullable DateTime
                                        DateTime start = resBct[resBct.Count - 1].DateStart.Value.AddMonths(resBct[resBct.Count - 1].MonthDiff);
                                        DateTime end = resBct[resBct.Count - 1].DateEnd.Value;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan span = end.Subtract(start);
                                        // Lấy số ngày chênh lệch
                                        resBct[resBct.Count - 1].DateDiff = (int)span.TotalDays;
                                    }
                                    if (resBct[resBct.Count - 1].DateDiff < 0)
                                    {
                                        resBct[resBct.Count - 1].DateDiff = 30 + resBct[resBct.Count - 1].DateDiff;
                                        resBct[resBct.Count - 1].MonthDiff--;
                                    }
                                }

                                PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * (decimal)changeData.TotalK));

                                PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                changeData.PriceRent1m2 = PriceRent1m2;
                                changeData.PriceRent = PriceRent;

                                //tháng
                                if (changeData.DateStart.HasValue && changeData.DateEnd.HasValue)
                                {
                                    changeData.MonthDiff = (changeData.DateEnd.Value.Year - changeData.DateStart.Value.Year) * 12 + (changeData.DateEnd.Value.Month - changeData.DateStart.Value.Month);
                                }
                                // ngày
                                if (changeData.DateEnd.HasValue && changeData.DateStart.HasValue)
                                {
                                    // Lấy DateTime bên trong kiểu Nullable DateTime
                                    DateTime start = changeData.DateStart.Value.AddMonths(changeData.MonthDiff);
                                    DateTime end = changeData.DateEnd.Value;
                                    // Lấy TimeSpan giữa hai ngày
                                    TimeSpan span = end.Subtract(start);
                                    // Lấy số ngày chênh lệch
                                    changeData.DateDiff = (int)span.TotalDays;
                                }
                                if (changeData.DateDiff < 0)
                                {
                                    changeData.DateDiff = 30 + changeData.DateDiff;
                                    changeData.MonthDiff--;
                                }
                                resBct.Add(changeData);
                            }
                        }
                    }

                }
            }

            var groupData = resBct.GroupBy(f => f.GroupId != null ? f.GroupId : new object(), key => key).Select(f => new groupData
            {
                GroupId = (int)f.Key,
                VAT = f.ToList().First().VAT,
                DateStart = (DateTime)f.ToList().First().DateStart,
                DateEnd = f.ToList().First().DateEnd,
                totalPrice = (decimal)f.ToList().Sum(p => p.PriceRent),
                bCTs = f.ToList()
            }).ToList();
            return groupData;
        }

        private List<groupData> getDataBCT09(Guid id, int RentFileBCTId)
        {
            try
            {
                //Tìm hồ sơ cho thuê
                RentFile rentFile = _context.RentFiles.Where(l => l.Id == id && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                RentFlieData map_rentFlieData = _mapper.Map<RentFlieData>(rentFile);

                //Tìm bảng chiết tính mới nhất theo ID hồ sơ
                RentFileBCT rentFileBCT = _context.RentFileBCTs.Where(l => l.Id == RentFileBCTId && l.TypeBCT == 2).FirstOrDefault();

                RentFileBCTData map_rentFlieBCTDatas = _mapper.Map<RentFileBCTData>(rentFileBCT);

                List<DefaultCoefficient> dataDf = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderBy(p => p.CoefficientName).ToList();
                var groupedData = dataDf.GroupBy(x => x.CoefficientName).Select(e => new GroupData
                {
                    CoefficientId = e.Key,
                    defaultCoefficients = e.ToList(),
                    childDefaultCoefficients = _context.ChildDefaults.Where(l => l.RentFileBctId == map_rentFlieBCTDatas.Id && l.CoefficientId == e.Key && l.Type == 2 && l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList()
                }).ToList();
                map_rentFlieBCTDatas.groupData = groupedData;

                List<Vat> vats = _context.Vats.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList();

                List<Salary> salaryCoefficients = _context.Salaries.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList();

                List<Coefficient> coefficients = _context.Coefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList();

                List<Conversion> conversions = _context.Conversions.Where(l => l.TypeQD == AppEnums.TypeQD.QD_09).ToList();

                List<DefaultCoefficient> defaultCoefficients = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                Block blocks = _context.Blocks.Where(l => l.Id == rentFile.RentBlockId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                List<RentingPrice> rentingPrices = _context.RentingPricies.Where(l => l.TypeQD == AppEnums.TypeQD.QD_09 && l.TypeBlockId == blocks.TypeBlockId && l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.EffectiveDate).ToList();

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


                List<BCT> resBct = new List<BCT>();
                if (rentFile.RentApartmentId > 0)
                {

                    Apartment apartment = _context.Apartments.Where(l => l.Id == rentFile.RentApartmentId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    ApartmentData map_apartments = _mapper.Map<ApartmentData>(apartment);
                    List<ApartmentDetail> apartmentDetails = _context.ApartmentDetails.Where(a => a.TargetId == rentFile.RentApartmentId && a.Type == TypeApartmentDetail.APARTMENT && a.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                    List<ApartmentDetailData> apartmentDetailData = _mapper.Map<List<ApartmentDetailData>>(apartmentDetails);
                    foreach (ApartmentDetailData map_apartmentDetail in apartmentDetailData)
                    {
                        Area area = _context.Areas.Where(f => f.Id == map_apartmentDetail.AreaId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                        map_apartmentDetail.AreaName = area != null ? area.Name : "";
                        map_apartmentDetail.IsMezzanine = area != null ? area.IsMezzanine : null;

                        Floor floor = _context.Floors.Where(f => f.Id == map_apartmentDetail.FloorId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                        map_apartmentDetail.FloorName = floor != null ? floor.Name : "";
                        map_apartmentDetail.FloorCode = floor != null ? floor.Code : 0;
                    }
                    map_apartments.apartmentDetailData = apartmentDetailData.ToList();


                    foreach (var item in map_apartments.apartmentDetailData)
                    {
                        var bCT = new BCT();
                        bCT.TypeQD = 9;

                        bCT.AreaName = item.AreaName;

                        bCT.Level = (int)item.Level;

                        bCT.GroupId = 9;

                        bCT.check = false;

                        bCT.PrivateArea = (float)rentFileBCT.Area;

                        DateTime dateStart = new DateTime(2007, 1, 19);

                        if (rentFileBCT.DateStart < dateStart)
                        {
                            bCT.DateStart = dateStart.AddDays(1);
                        }
                        else
                        {
                            bCT.DateStart = rentFileBCT.DateStart;
                        }

                        DateTime date = new DateTime(2018, 7, 11);
                        if (rentFileBCT.DateEnd > date)
                        {
                            bCT.DateEnd = date;
                        }
                        else
                        {
                            bCT.DateEnd = rentFileBCT.DateEnd;
                        }

                        bCT.DateCoefficient = (DateTime)item.DisposeTime;

                        bCT.PolicyReduction = (decimal)_context.DiscountCoefficients.Where(p => p.Id == rentFileBCT.DiscountCoefficientId && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();


                        //tháng
                        if (bCT.DateStart.HasValue && bCT.DateEnd.HasValue)
                        {
                            bCT.MonthDiff = (bCT.DateEnd.Value.Year - bCT.DateStart.Value.Year) * 12 + (bCT.DateEnd.Value.Month - bCT.DateStart.Value.Month);
                        }
                        // ngày
                        if (bCT.DateEnd.HasValue && bCT.DateStart.HasValue)
                        {
                            // Lấy DateTime bên trong kiểu Nullable DateTime
                            DateTime start = bCT.DateStart.Value.AddMonths(bCT.MonthDiff);
                            DateTime end = bCT.DateEnd.Value;
                            // Lấy TimeSpan giữa hai ngày
                            TimeSpan span = end.Subtract(start);
                            // Lấy số ngày chênh lệch
                            bCT.DateDiff = (int)span.TotalDays;
                        }
                        if (bCT.DateDiff < 0)
                        {
                            bCT.DateDiff = 30 + bCT.DateDiff;
                            bCT.MonthDiff--;
                        }

                        foreach (var itemLv in rentingPrices) //Lấy giá chuẩn
                        {
                            if (itemLv.LevelId == item.Level)
                            {
                                bCT.StandardPrice = (decimal)itemLv.Price;
                                break;
                            }
                        }

                        foreach (var vat in vats)
                        {
                            if (bCT.DateStart >= vat.DoApply) // lấy giá trị của VAT theo dateStart
                            {
                                bCT.VAT = vat.Value;
                                break;
                            }
                        }

                        bCT.chilDfs = new List<chilDf>();

                        foreach (var itemDf in map_rentFlieBCTDatas.groupData)
                        {
                            if (itemDf.childDefaultCoefficients.Count > 0)
                            {
                                foreach (var itemChild in itemDf.childDefaultCoefficients)
                                {

                                    if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                    {

                                        string name = "";
                                        if (item.FloorCode < 6)
                                        {
                                            name = bCT.AreaName;
                                        }
                                        else
                                        {
                                            name = "Tầng 6 trở lên";
                                        }
                                        string content = _context.DefaultCoefficients.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Content).FirstOrDefault();
                                        if (content == name)
                                        {
                                            if (bCT.DateStart >= itemChild.DoApply)
                                            {
                                                var itemChilDfs = new chilDf();
                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;

                                                itemChilDfs.Value = itemChild.Value;
                                                bCT.chilDfs.Add(itemChilDfs);
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (bCT.DateStart >= itemChild.DoApply)
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Value = itemChild.Value;
                                            bCT.chilDfs.Add(itemChilDfs);
                                            break;
                                        }
                                    }
                                }
                            }
                        }


                        if (bCT.StandardPrice == null) bCT.StandardPrice = 0;

                        if (bCT.chilDfs != null)
                        {
                            bCT.TotalK = (float)bCT.chilDfs.Sum(p => p.Value) + 1;
                        }
                        else
                        {
                            bCT.TotalK = 1;
                        }

                        if (bCT.Ktlcb == null) bCT.Ktlcb = 1;
                        if (bCT.Ktdbt == null) bCT.Ktdbt = 1;

                        bCT.TotalK = (float)bCT.chilDfs.Sum(p => p.Value) + 1;
                        bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * (decimal)bCT.TotalK));
                        bCT.PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea));
                        bCT.Unit = "VNĐ";
                        resBct.Add(bCT);
                    }
                }
                else
                {
                    foreach (var item in map_blocks.blockDetails)
                    {
                        var bCT = new BCT();

                        bCT.TypeQD = 9;

                        bCT.GroupId = 9;

                        bCT.AreaName = item.AreaName;

                        bCT.Level = (int)item.Level;

                        bCT.check = false;

                        bCT.PrivateArea = (float)rentFileBCT.Area;
                        bCT.PolicyReduction = (decimal)_context.DiscountCoefficients.Where(p => p.Id == rentFileBCT.DiscountCoefficientId && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();


                        DateTime dateStart = new DateTime(2007, 1, 19);

                        if (rentFileBCT.DateStart < dateStart)
                        {
                            bCT.DateStart = dateStart.AddDays(1);
                        }
                        else
                        {
                            bCT.DateStart = rentFileBCT.DateStart;
                        }

                        DateTime date = new DateTime(2018, 7, 11);
                        if (rentFileBCT.DateEnd > date)
                        {
                            bCT.DateEnd = date;
                        }
                        else
                        {
                            bCT.DateEnd = rentFileBCT.DateEnd;
                        }

                        bCT.DateCoefficient = (DateTime)item.DisposeTime;

                        //tháng
                        if (bCT.DateStart.HasValue && bCT.DateEnd.HasValue)
                        {
                            bCT.MonthDiff = (bCT.DateEnd.Value.Year - bCT.DateStart.Value.Year) * 12 + (bCT.DateEnd.Value.Month - bCT.DateStart.Value.Month);
                        }
                        // ngày
                        if (bCT.DateEnd.HasValue && bCT.DateStart.HasValue)
                        {
                            // Lấy DateTime bên trong kiểu Nullable DateTime
                            DateTime start = bCT.DateStart.Value.AddMonths(bCT.MonthDiff);
                            DateTime end = bCT.DateEnd.Value;
                            // Lấy TimeSpan giữa hai ngày
                            TimeSpan span = end.Subtract(start);
                            // Lấy số ngày chênh lệch
                            bCT.DateDiff = (int)span.TotalDays;
                        }
                        if (bCT.DateDiff < 0)
                        {
                            bCT.DateDiff = 30 + bCT.DateDiff;
                            bCT.MonthDiff--;
                        }

                        foreach (var itemLv in rentingPrices) //Lấy giá chuẩn
                        {
                            if (itemLv.LevelId == item.Level)
                            {
                                bCT.StandardPrice = (decimal)itemLv.Price;
                                break;
                            }
                        }

                        foreach (var vat in vats)
                        {
                            if (bCT.DateStart >= vat.DoApply) // lấy giá trị của VAT theo dateStart
                            {
                                bCT.VAT = vat.Value;
                                break;
                            }
                        }

                        bCT.chilDfs = new List<chilDf>();

                        foreach (var itemDf in map_rentFlieBCTDatas.groupData)
                        {
                            if (itemDf.childDefaultCoefficients.Count > 0)
                            {
                                foreach (var itemChild in itemDf.childDefaultCoefficients)
                                {

                                    if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                    {
                                        string name = "";
                                        if (item.FloorCode < 6)
                                        {
                                            name = bCT.AreaName;
                                        }
                                        else
                                        {
                                            name = "Tầng 6 trở lên";
                                        }
                                        string content = _context.DefaultCoefficients.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Content).FirstOrDefault();
                                        if (content == name)
                                        {
                                            if (bCT.DateStart >= itemChild.DoApply)
                                            {
                                                var itemChilDfs = new chilDf();
                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;

                                                itemChilDfs.Value = itemChild.Value;
                                                bCT.chilDfs.Add(itemChilDfs);
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (bCT.DateStart >= itemChild.DoApply)
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Value = itemChild.Value;
                                            bCT.chilDfs.Add(itemChilDfs);
                                            break;
                                        }
                                    }
                                }
                            }
                        }


                        if (bCT.StandardPrice == null) bCT.StandardPrice = 0;

                        if (bCT.chilDfs != null)
                        {
                            bCT.TotalK = (float)bCT.chilDfs.Sum(p => p.Value) + 1;
                        }
                        else
                        {
                            bCT.TotalK = 1;
                        }

                        if (bCT.Ktlcb == null) bCT.Ktlcb = 1;
                        if (bCT.Ktdbt == null) bCT.Ktdbt = 1;

                        bCT.TotalK = (float)bCT.chilDfs.Sum(p => p.Value) + 1;
                        bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * (decimal)bCT.TotalK));
                        bCT.PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea));
                        bCT.Unit = "VNĐ";
                        resBct.Add(bCT);
                    }
                }
                var groupData = resBct.GroupBy(f => f.GroupId != null ? f.GroupId : new object(), key => key).Select(f => new groupData
                {
                    GroupId = (int)f.Key,
                    VAT = f.ToList().First().VAT,
                    DateStart = (DateTime)f.ToList().First().DateStart,
                    DateEnd = f.ToList().First().DateEnd,
                    totalPrice = (decimal)f.ToList().Sum(p => p.PriceRent),
                    bCTs = f.ToList()
                }).ToList();
                return groupData;
            }
            catch (Exception ex)
            {
                log.Error("getDataBCT22:" + ex);
                return null;
            }
        }

        private List<groupData> getDataBCT22(Guid id, int RentFileBCTId)
        {
            try
            {
                //Tìm hồ sơ cho thuê
                RentFile rentFile = _context.RentFiles.Where(l => l.Id == id && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                RentFlieData map_rentFlieData = _mapper.Map<RentFlieData>(rentFile);

                //Tìm bảng chiết tính mới nhất theo ID hồ sơ
                RentFileBCT rentFileBCT = _context.RentFileBCTs.Where(l => l.Id == RentFileBCTId && l.TypeBCT == 2).FirstOrDefault();

                RentFileBCTData map_rentFlieBCTDatas = _mapper.Map<RentFileBCTData>(rentFileBCT);

                List<DefaultCoefficient> dataDf = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderBy(p => p.CoefficientName).ToList();
                var groupedData = dataDf.GroupBy(x => x.CoefficientName).Select(e => new GroupData
                {
                    CoefficientId = e.Key,
                    defaultCoefficients = e.ToList(),
                    childDefaultCoefficients = _context.ChildDefaults.Where(l => l.RentFileBctId == map_rentFlieBCTDatas.Id && l.CoefficientId == e.Key && l.Type == 2 && l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList()
                }).ToList();
                map_rentFlieBCTDatas.groupData = groupedData;

                List<Vat> vats = _context.Vats.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList();

                List<Salary> salaryCoefficients = _context.Salaries.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList();

                List<Coefficient> coefficients = _context.Coefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderBy(p => p.DoApply).ToList();

                List<Conversion> conversions = _context.Conversions.Where(l => l.TypeQD == AppEnums.TypeQD.QD_22).ToList();

                List<DefaultCoefficient> defaultCoefficients = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                Block blocks = _context.Blocks.Where(l => l.Id == rentFile.RentBlockId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                List<RentingPrice> rentingPrices = _context.RentingPricies.Where(l => l.TypeQD == AppEnums.TypeQD.QD_22 && l.TypeBlockId == blocks.TypeBlockId && l.Status != AppEnums.EntityStatus.DELETED).ToList();

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

                List<BCT> resBct = new List<BCT>();

                int i = 0;
                int check = 0;

                decimal sum = 0;

                if (rentFile.RentApartmentId > 0)
                {

                    Apartment apartment = _context.Apartments.Where(l => l.Id == rentFile.RentApartmentId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    ApartmentData map_apartments = _mapper.Map<ApartmentData>(apartment);
                    List<ApartmentDetail> apartmentDetails = _context.ApartmentDetails.Where(a => a.TargetId == rentFile.RentApartmentId && a.Type == TypeApartmentDetail.APARTMENT && a.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                    List<ApartmentDetailData> apartmentDetailData = _mapper.Map<List<ApartmentDetailData>>(apartmentDetails);
                    foreach (ApartmentDetailData map_apartmentDetail in apartmentDetailData)
                    {
                        Area area = _context.Areas.Where(f => f.Id == map_apartmentDetail.AreaId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                        map_apartmentDetail.AreaName = area != null ? area.Name : "";
                        map_apartmentDetail.IsMezzanine = area != null ? area.IsMezzanine : null;

                        Floor floor = _context.Floors.Where(f => f.Id == map_apartmentDetail.FloorId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                        map_apartmentDetail.FloorName = floor != null ? floor.Name : "";
                        map_apartmentDetail.FloorCode = floor != null ? floor.Code : 0;
                    }
                    map_apartments.apartmentDetailData = apartmentDetailData.ToList();
                    foreach (var item in map_apartments.apartmentDetailData)
                    {

                        int k = 0;
                        var bCT = new BCT();
                        List<changeValue> resChange = new List<changeValue>();
                        List<changeK> resChangeK = new List<changeK>();

                        bCT.check = false;
                        bCT.GroupId = k;
                        bCT.TypeQD = 22;
                        bCT.AreaName = item.AreaName;

                        bCT.Level = (int)item.Level;

                        bCT.PrivateArea = (float)rentFileBCT.Area;

                        DateTime dateStart = new DateTime(2018, 7, 12);

                        if (rentFileBCT.DateStart < dateStart)
                        {
                            bCT.DateStart = dateStart;
                        }
                        else
                        {
                            bCT.DateStart = rentFileBCT.DateStart;
                        }

                        bCT.DateEnd = rentFileBCT.DateEnd;

                        //tháng
                        if (bCT.DateStart.HasValue && bCT.DateEnd.HasValue)
                        {
                            bCT.MonthDiff = (bCT.DateEnd.Value.Year - bCT.DateStart.Value.Year) * 12 + (bCT.DateEnd.Value.Month - bCT.DateStart.Value.Month);
                        }
                        // ngày
                        if (bCT.DateEnd.HasValue && bCT.DateStart.HasValue)
                        {
                            // Lấy DateTime bên trong kiểu Nullable DateTime
                            DateTime start = bCT.DateStart.Value.AddMonths(bCT.MonthDiff);
                            DateTime end = bCT.DateEnd.Value;
                            // Lấy TimeSpan giữa hai ngày
                            TimeSpan span = end.Subtract(start);
                            // Lấy số ngày chênh lệch
                            bCT.DateDiff = (int)span.TotalDays;
                        }
                        if (bCT.DateDiff < 0)
                        {
                            bCT.DateDiff = 30 + bCT.DateDiff;
                            bCT.MonthDiff--;
                        }

                        bCT.DateCoefficient = (DateTime)item.DisposeTime;
                        bCT.PolicyReduction = (decimal)_context.DiscountCoefficients.Where(p => p.Id == rentFileBCT.DiscountCoefficientId && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();


                        foreach (var itemLv in rentingPrices) //Lấy giá chuẩn
                        {
                            if (itemLv.LevelId == item.Level)
                            {
                                bCT.StandardPrice = (decimal)itemLv.Price;
                                break;
                            }
                        }

                        foreach (var vat in vats)
                        {
                            if (bCT.DateStart >= vat.DoApply)
                            {
                                bCT.VAT = vat.Value;
                                break;
                            }
                            else if (vat.DoApply <= bCT.DateEnd)
                            {
                                var changevalue = new changeValue();

                                changevalue.Type = 1;
                                changevalue.valueChange = vat.Value;
                                changevalue.DateChange = vat.DoApply;

                                resChange.Add(changevalue);
                                continue;
                            }
                        }

                        foreach (var salaryCoefficient in salaryCoefficients) // lấy giá trị của lương cơ bản theo ngày áp dụng
                        {
                            if (bCT.DateStart >= salaryCoefficient.DoApply)
                            {
                                bCT.Ktlcb = (decimal)salaryCoefficient.Value;
                                break;
                            }
                            else if (salaryCoefficient.DoApply <= bCT.DateEnd)
                            {
                                var changevalue = new changeValue();

                                changevalue.Type = 2;
                                changevalue.valueChange = salaryCoefficient.Value;
                                changevalue.DateChange = salaryCoefficient.DoApply;

                                resChange.Add(changevalue);
                                continue;
                            }
                        }

                        foreach (var coefficient in coefficients) // lấy giá trị của hệ số thời điểm bố trí theo ngày áp dụng
                        {
                            if (bCT.DateCoefficient <= coefficient.DoApply)
                            {
                                bCT.Ktdbt = (decimal)coefficient.Value;
                                break;
                            }

                        }

                        bCT.chilDfs = new List<chilDf>();
                        if (map_rentFlieBCTDatas != null)
                        {
                            foreach (var itemDf in map_rentFlieBCTDatas.groupData)
                            {
                                if (itemDf.childDefaultCoefficients.Count > 0)
                                {
                                    foreach (var itemChild in itemDf.childDefaultCoefficients)
                                    {

                                        if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                        {

                                            string name = "";
                                            if (item.FloorCode < 6)
                                            {
                                                name = bCT.AreaName;
                                            }
                                            else
                                            {
                                                name = "Tầng 6 trở lên";
                                            }
                                            string content = _context.DefaultCoefficients.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Content).FirstOrDefault();
                                            if (content == name)
                                            {
                                                if (bCT.DateStart >= itemChild.DoApply)
                                                {
                                                    var itemChilDfs = new chilDf();

                                                    itemChilDfs.Value = itemChild.Value;
                                                    bCT.chilDfs.Add(itemChilDfs);
                                                    break;
                                                }
                                                else if (bCT.DateEnd >= itemChild.DoApply)
                                                {
                                                    var changeK = new changeK();

                                                    changeK.valueChange = itemChild.Value;
                                                    changeK.DateChange = itemChild.DoApply;
                                                    resChangeK.Add(changeK);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (bCT.DateStart >= itemChild.DoApply)
                                            {
                                                var itemChilDfs = new chilDf();

                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                itemChilDfs.Value = itemChild.Value;
                                                bCT.chilDfs.Add(itemChilDfs);
                                                break;
                                            }
                                            else if (bCT.DateEnd >= itemChild.DoApply)
                                            {
                                                var changeK = new changeK();

                                                changeK.CoefficientId = itemChild.CoefficientId;
                                                changeK.valueChange = itemChild.Value;
                                                changeK.DateChange = itemChild.DoApply;
                                                resChangeK.Add(changeK);
                                            }
                                        }
                                    }
                                }
                            }


                        }

                        var groupdataK = resChangeK.GroupBy(p => p.DateChange).Select(e => new groupChangeK
                        {
                            DateChange = e.Key,
                            groupDataChangeK = e.ToList()
                        });

                        if (bCT.StandardPrice == null) bCT.StandardPrice = 0;

                        if (bCT.chilDfs != null)
                        {
                            bCT.TotalK = (float)bCT.chilDfs.Sum(p => p.Value) + 1;
                        }
                        else
                        {
                            bCT.TotalK = 1;
                        }

                        if (bCT.Ktlcb == null) bCT.Ktlcb = 1;
                        if (bCT.Ktdbt == null) bCT.Ktdbt = 1;

                        bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * bCT.Ktlcb * bCT.Ktdbt * (decimal)bCT.TotalK));

                        bCT.PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea));

                        bCT.Unit = "VNĐ";

                        resBct.Add(bCT);

                        if (groupdataK.Count() > 0)
                        {
                            foreach (var itemk in groupdataK)
                            {
                                decimal totalK = 0;
                                decimal CurrentK = (decimal)bCT.TotalK;
                                var changevalue = new changeValue();
                                foreach (var itemK in bCT.chilDfs)
                                {
                                    foreach (var itemChangeK in itemk.groupDataChangeK.OrderBy(o => o.DateChange).ToList())
                                    {
                                        if (itemK.CoefficientId == itemChangeK.CoefficientId)
                                        {
                                            totalK = (decimal)(CurrentK - (decimal)itemK.Value);
                                            changevalue.valueChange = (double)(totalK + (decimal)itemChangeK.valueChange);
                                            changevalue.Type = 5;
                                            changevalue.DateChange = itemChangeK.DateChange;
                                            CurrentK = (decimal)changevalue.valueChange;
                                        }
                                    }
                                }
                                resChange.Add(changevalue);
                            }
                        }

                        if (resChange.Count > 0)
                        {
                            foreach (var itemChange in resChange.OrderBy(p => p.DateChange).ToList())
                            {
                                k = k + 1;
                                decimal? PriceRent1m2 = 0;
                                decimal? PriceRent = 0;

                                var changeData = new BCT();
                                changeData.TypeQD = 22;
                                changeData.check = false;
                                changeData.GroupId = k;

                                changeData.AreaName = item.AreaName;

                                changeData.Level = (int)item.Level;

                                changeData.DateCoefficient = item.DisposeTime;

                                changeData.PrivateArea = (float)rentFileBCT.Area;

                                changeData.VAT = resBct[resBct.Count - 1].VAT;
                                changeData.Ktdbt = resBct[resBct.Count - 1].Ktdbt;
                                changeData.Ktlcb = resBct[resBct.Count - 1].Ktlcb;
                                changeData.StandardPrice = resBct[resBct.Count - 1].StandardPrice;
                                changeData.Unit = resBct[resBct.Count - 1].Unit;
                                changeData.PolicyReduction = resBct[resBct.Count - 1].PolicyReduction;

                                if (itemChange.Type == 1)
                                {
                                    changeData.VAT = itemChange.valueChange;
                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);

                                        //tháng
                                        if (resBct[resBct.Count - 1].DateStart.HasValue && resBct[resBct.Count - 1].DateEnd.HasValue)
                                        {
                                            resBct[resBct.Count - 1].MonthDiff = (resBct[resBct.Count - 1].DateEnd.Value.Year - resBct[resBct.Count - 1].DateStart.Value.Year) * 12 + (resBct[resBct.Count - 1].DateEnd.Value.Month - resBct[resBct.Count - 1].DateStart.Value.Month);
                                        }
                                        // ngày
                                        if (resBct[resBct.Count - 1].DateEnd.HasValue && resBct[resBct.Count - 1].DateStart.HasValue)
                                        {
                                            // Lấy DateTime bên trong kiểu Nullable DateTime
                                            DateTime start = resBct[resBct.Count - 1].DateStart.Value.AddMonths(resBct[resBct.Count - 1].MonthDiff);
                                            DateTime end = resBct[resBct.Count - 1].DateEnd.Value;
                                            // Lấy TimeSpan giữa hai ngày
                                            TimeSpan span = end.Subtract(start);
                                            // Lấy số ngày chênh lệch
                                            resBct[resBct.Count - 1].DateDiff = (int)span.TotalDays;
                                        }
                                        if (resBct[resBct.Count - 1].DateDiff < 0)
                                        {
                                            resBct[resBct.Count - 1].DateDiff = 30 + resBct[resBct.Count - 1].DateDiff;
                                            resBct[resBct.Count - 1].MonthDiff--;
                                        }
                                    }


                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;

                                    //tháng
                                    if (changeData.DateStart.HasValue && changeData.DateEnd.HasValue)
                                    {
                                        changeData.MonthDiff = (changeData.DateEnd.Value.Year - changeData.DateStart.Value.Year) * 12 + (changeData.DateEnd.Value.Month - changeData.DateStart.Value.Month);
                                    }
                                    // ngày
                                    if (changeData.DateEnd.HasValue && changeData.DateStart.HasValue)
                                    {
                                        // Lấy DateTime bên trong kiểu Nullable DateTime
                                        DateTime start = changeData.DateStart.Value.AddMonths(changeData.MonthDiff);
                                        DateTime end = changeData.DateEnd.Value;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan span = end.Subtract(start);
                                        // Lấy số ngày chênh lệch
                                        changeData.DateDiff = (int)span.TotalDays;
                                    }
                                    if (changeData.DateDiff < 0)
                                    {
                                        changeData.DateDiff = 30 + changeData.DateDiff;
                                        changeData.MonthDiff--;
                                    }
                                    resBct.Add(changeData);
                                }
                                else if (itemChange.Type == 2)
                                {
                                    changeData.Ktlcb = (decimal)itemChange.valueChange;
                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));
                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);

                                        //tháng
                                        if (resBct[resBct.Count - 1].DateStart.HasValue && resBct[resBct.Count - 1].DateEnd.HasValue)
                                        {
                                            resBct[resBct.Count - 1].MonthDiff = (resBct[resBct.Count - 1].DateEnd.Value.Year - resBct[resBct.Count - 1].DateStart.Value.Year) * 12 + (resBct[resBct.Count - 1].DateEnd.Value.Month - resBct[resBct.Count - 1].DateStart.Value.Month);
                                        }
                                        // ngày
                                        if (resBct[resBct.Count - 1].DateEnd.HasValue && resBct[resBct.Count - 1].DateStart.HasValue)
                                        {
                                            // Lấy DateTime bên trong kiểu Nullable DateTime
                                            DateTime start = resBct[resBct.Count - 1].DateStart.Value.AddMonths(resBct[resBct.Count - 1].MonthDiff);
                                            DateTime end = resBct[resBct.Count - 1].DateEnd.Value;
                                            // Lấy TimeSpan giữa hai ngày
                                            TimeSpan span = end.Subtract(start);
                                            // Lấy số ngày chênh lệch
                                            resBct[resBct.Count - 1].DateDiff = (int)span.TotalDays;
                                        }
                                        if (resBct[resBct.Count - 1].DateDiff < 0)
                                        {
                                            resBct[resBct.Count - 1].DateDiff = 30 + resBct[resBct.Count - 1].DateDiff;
                                            resBct[resBct.Count - 1].MonthDiff--;
                                        }
                                    }

                                    //tháng
                                    if (changeData.DateStart.HasValue && changeData.DateEnd.HasValue)
                                    {
                                        changeData.MonthDiff = (changeData.DateEnd.Value.Year - changeData.DateStart.Value.Year) * 12 + (changeData.DateEnd.Value.Month - changeData.DateStart.Value.Month);
                                    }
                                    // ngày
                                    if (changeData.DateEnd.HasValue && changeData.DateStart.HasValue)
                                    {
                                        // Lấy DateTime bên trong kiểu Nullable DateTime
                                        DateTime start = changeData.DateStart.Value.AddMonths(changeData.MonthDiff);
                                        DateTime end = changeData.DateEnd.Value;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan span = end.Subtract(start);
                                        // Lấy số ngày chênh lệch
                                        changeData.DateDiff = (int)span.TotalDays;
                                    }
                                    if (changeData.DateDiff < 0)
                                    {
                                        changeData.DateDiff = 30 + changeData.DateDiff;
                                        changeData.MonthDiff--;
                                    }

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;

                                    resBct.Add(changeData);

                                }
                                else if (itemChange.Type == 3)
                                {
                                    changeData.Ktdbt = (decimal)itemChange.valueChange;
                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;

                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);


                                        //tháng
                                        if (resBct[resBct.Count - 1].DateStart.HasValue && resBct[resBct.Count - 1].DateEnd.HasValue)
                                        {
                                            resBct[resBct.Count - 1].MonthDiff = (resBct[resBct.Count - 1].DateEnd.Value.Year - resBct[resBct.Count - 1].DateStart.Value.Year) * 12 + (resBct[resBct.Count - 1].DateEnd.Value.Month - resBct[resBct.Count - 1].DateStart.Value.Month);
                                        }
                                        // ngày
                                        if (resBct[resBct.Count - 1].DateEnd.HasValue && resBct[resBct.Count - 1].DateStart.HasValue)
                                        {
                                            // Lấy DateTime bên trong kiểu Nullable DateTime
                                            DateTime start = resBct[resBct.Count - 1].DateStart.Value.AddMonths(resBct[resBct.Count - 1].MonthDiff);
                                            DateTime end = resBct[resBct.Count - 1].DateEnd.Value;
                                            // Lấy TimeSpan giữa hai ngày
                                            TimeSpan span = end.Subtract(start);
                                            // Lấy số ngày chênh lệch
                                            resBct[resBct.Count - 1].DateDiff = (int)span.TotalDays;
                                        }
                                        if (resBct[resBct.Count - 1].DateDiff < 0)
                                        {
                                            resBct[resBct.Count - 1].DateDiff = 30 + resBct[resBct.Count - 1].DateDiff;
                                            resBct[resBct.Count - 1].MonthDiff--;
                                        }
                                    }

                                    changeData.chilDfs = new List<chilDf>();
                                    if (map_rentFlieBCTDatas != null)
                                    {
                                        foreach (var itemDf in map_rentFlieBCTDatas.groupData)
                                        {
                                            if (itemDf.childDefaultCoefficients.Count > 0)
                                            {
                                                foreach (var itemChild in itemDf.childDefaultCoefficients)
                                                {
                                                    var itemChilDfs = new chilDf();

                                                    if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                                    {

                                                        string name = "";
                                                        if (item.FloorCode < 6)
                                                        {
                                                            name = changeData.AreaName;
                                                        }
                                                        else
                                                        {
                                                            name = "Tầng 6 trở lên";
                                                        }
                                                        string content = _context.DefaultCoefficients.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Content).FirstOrDefault();
                                                        itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                        if (content == name)
                                                        {
                                                            if (changeData.DateStart >= itemChild.DoApply)
                                                            {
                                                                itemChilDfs.Value = itemChild.Value;
                                                                changeData.chilDfs.Add(itemChilDfs);
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (changeData.DateStart >= itemChild.DoApply)
                                                        {
                                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                            itemChilDfs.Value = itemChild.Value;
                                                            changeData.chilDfs.Add(itemChilDfs);
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                    }

                                    //tháng
                                    if (changeData.DateStart.HasValue && changeData.DateEnd.HasValue)
                                    {
                                        changeData.MonthDiff = (changeData.DateEnd.Value.Year - changeData.DateStart.Value.Year) * 12 + (changeData.DateEnd.Value.Month - changeData.DateStart.Value.Month);
                                    }
                                    // ngày
                                    if (changeData.DateEnd.HasValue && changeData.DateStart.HasValue)
                                    {
                                        // Lấy DateTime bên trong kiểu Nullable DateTime
                                        DateTime start = changeData.DateStart.Value.AddMonths(changeData.MonthDiff);
                                        DateTime end = changeData.DateEnd.Value;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan span = end.Subtract(start);
                                        // Lấy số ngày chênh lệch
                                        changeData.DateDiff = (int)span.TotalDays;
                                    }
                                    if (changeData.DateDiff < 0)
                                    {
                                        changeData.DateDiff = 30 + changeData.DateDiff;
                                        changeData.MonthDiff--;
                                    }

                                    resBct.Add(changeData);
                                }
                                else if (itemChange.Type == 4)
                                {
                                    changeData.PolicyReduction = (decimal)itemChange.valueChange;

                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;

                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);

                                        //tháng
                                        if (resBct[resBct.Count - 1].DateStart.HasValue && resBct[resBct.Count - 1].DateEnd.HasValue)
                                        {
                                            resBct[resBct.Count - 1].MonthDiff = (resBct[resBct.Count - 1].DateEnd.Value.Year - resBct[resBct.Count - 1].DateStart.Value.Year) * 12 + (resBct[resBct.Count - 1].DateEnd.Value.Month - resBct[resBct.Count - 1].DateStart.Value.Month);
                                        }
                                        // ngày
                                        if (resBct[resBct.Count - 1].DateEnd.HasValue && resBct[resBct.Count - 1].DateStart.HasValue)
                                        {
                                            // Lấy DateTime bên trong kiểu Nullable DateTime
                                            DateTime start = resBct[resBct.Count - 1].DateStart.Value.AddMonths(resBct[resBct.Count - 1].MonthDiff);
                                            DateTime end = resBct[resBct.Count - 1].DateEnd.Value;
                                            // Lấy TimeSpan giữa hai ngày
                                            TimeSpan span = end.Subtract(start);
                                            // Lấy số ngày chênh lệch
                                            resBct[resBct.Count - 1].DateDiff = (int)span.TotalDays;
                                        }
                                        if (resBct[resBct.Count - 1].DateDiff < 0)
                                        {
                                            resBct[resBct.Count - 1].DateDiff = 30 + resBct[resBct.Count - 1].DateDiff;
                                            resBct[resBct.Count - 1].MonthDiff--;
                                        }

                                    }

                                    //tháng
                                    if (changeData.DateStart.HasValue && changeData.DateEnd.HasValue)
                                    {
                                        changeData.MonthDiff = (changeData.DateEnd.Value.Year - changeData.DateStart.Value.Year) * 12 + (changeData.DateEnd.Value.Month - changeData.DateStart.Value.Month);
                                    }
                                    // ngày
                                    if (changeData.DateEnd.HasValue && changeData.DateStart.HasValue)
                                    {
                                        // Lấy DateTime bên trong kiểu Nullable DateTime
                                        DateTime start = changeData.DateStart.Value.AddMonths(changeData.MonthDiff);
                                        DateTime end = changeData.DateEnd.Value;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan span = end.Subtract(start);
                                        // Lấy số ngày chênh lệch
                                        changeData.DateDiff = (int)span.TotalDays;
                                    }
                                    if (changeData.DateDiff < 0)
                                    {
                                        changeData.DateDiff = 30 + changeData.DateDiff;
                                        changeData.MonthDiff--;
                                    }

                                    resBct.Add(changeData);

                                }
                                else if (itemChange.Type == 5)
                                {
                                    changeData.TotalK = (float?)itemChange.valueChange;
                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);



                                        //tháng
                                        if (resBct[resBct.Count - 1].DateStart.HasValue && resBct[resBct.Count - 1].DateEnd.HasValue)
                                        {
                                            resBct[resBct.Count - 1].MonthDiff = (resBct[resBct.Count - 1].DateEnd.Value.Year - resBct[resBct.Count - 1].DateStart.Value.Year) * 12 + (resBct[resBct.Count - 1].DateEnd.Value.Month - resBct[resBct.Count - 1].DateStart.Value.Month);
                                        }
                                        // ngày
                                        if (resBct[resBct.Count - 1].DateEnd.HasValue && resBct[resBct.Count - 1].DateStart.HasValue)
                                        {
                                            // Lấy DateTime bên trong kiểu Nullable DateTime
                                            DateTime start = resBct[resBct.Count - 1].DateStart.Value.AddMonths(resBct[resBct.Count - 1].MonthDiff);
                                            DateTime end = resBct[resBct.Count - 1].DateEnd.Value;
                                            // Lấy TimeSpan giữa hai ngày
                                            TimeSpan span = end.Subtract(start);
                                            // Lấy số ngày chênh lệch
                                            resBct[resBct.Count - 1].DateDiff = (int)span.TotalDays;
                                        }
                                        if (resBct[resBct.Count - 1].DateDiff < 0)
                                        {
                                            resBct[resBct.Count - 1].DateDiff = 30 + resBct[resBct.Count - 1].DateDiff;
                                            resBct[resBct.Count - 1].MonthDiff--;
                                        }

                                    }

                                    changeData.chilDfs = new List<chilDf>();
                                    if (map_rentFlieBCTDatas != null)
                                    {
                                        foreach (var itemDf in map_rentFlieBCTDatas.groupData)
                                        {
                                            if (itemDf.childDefaultCoefficients.Count > 0)
                                            {
                                                foreach (var itemChild in itemDf.childDefaultCoefficients)
                                                {
                                                    var itemChilDfs = new chilDf();

                                                    if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                                    {
                                                        string name = "";
                                                        if (item.FloorCode < 6)
                                                        {
                                                            name = changeData.AreaName;
                                                        }
                                                        else
                                                        {
                                                            name = "Tầng 6 trở lên";
                                                        }
                                                        string content = _context.DefaultCoefficients.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Content).FirstOrDefault();
                                                        itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                        if (content == name)
                                                        {
                                                            if (changeData.DateStart >= itemChild.DoApply)
                                                            {
                                                                itemChilDfs.Value = itemChild.Value;
                                                                changeData.chilDfs.Add(itemChilDfs);
                                                                break;
                                                            }

                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (changeData.DateStart >= itemChild.DoApply)
                                                        {
                                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                            itemChilDfs.Value = itemChild.Value;
                                                            changeData.chilDfs.Add(itemChilDfs);
                                                            break;
                                                        }

                                                    }
                                                }
                                            }
                                        }


                                    }

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;



                                    //tháng
                                    if (changeData.DateStart.HasValue && changeData.DateEnd.HasValue)
                                    {
                                        changeData.MonthDiff = (changeData.DateEnd.Value.Year - changeData.DateStart.Value.Year) * 12 + (changeData.DateEnd.Value.Month - changeData.DateStart.Value.Month);
                                    }
                                    // ngày
                                    if (changeData.DateEnd.HasValue && changeData.DateStart.HasValue)
                                    {
                                        // Lấy DateTime bên trong kiểu Nullable DateTime
                                        DateTime start = changeData.DateStart.Value.AddMonths(changeData.MonthDiff);
                                        DateTime end = changeData.DateEnd.Value;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan span = end.Subtract(start);
                                        // Lấy số ngày chênh lệch
                                        changeData.DateDiff = (int)span.TotalDays;
                                    }
                                    if (changeData.DateDiff < 0)
                                    {
                                        changeData.DateDiff = 30 + changeData.DateDiff;
                                        changeData.MonthDiff--;
                                    }

                                    resBct.Add(changeData);
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (var item in map_blocks.blockDetails)
                    {
                        int k = 0;
                        var bCTBL = new BCT();
                        List<changeValue> resChange = new List<changeValue>();
                        List<changeK> resChangeK = new List<changeK>();

                        bCTBL.TypeQD = 22;
                        bCTBL.check = false;
                        bCTBL.GroupId = k;

                        bCTBL.AreaName = item.AreaName;

                        bCTBL.Level = (int)item.Level;

                        bCTBL.PrivateArea = (float)rentFileBCT.Area;

                        DateTime dateStart = new DateTime(2018, 7, 12);

                        if (rentFileBCT.DateStart < dateStart)
                        {
                            bCTBL.DateStart = dateStart;
                        }
                        else
                        {
                            bCTBL.DateStart = rentFileBCT.DateStart;
                        }

                        bCTBL.DateEnd = rentFileBCT.DateEnd;

                        //tháng
                        if (bCTBL.DateStart.HasValue && bCTBL.DateEnd.HasValue)
                        {
                            bCTBL.MonthDiff = (bCTBL.DateEnd.Value.Year - bCTBL.DateStart.Value.Year) * 12 + (bCTBL.DateEnd.Value.Month - bCTBL.DateStart.Value.Month);
                        }
                        // ngày
                        if (bCTBL.DateEnd.HasValue && bCTBL.DateStart.HasValue)
                        {
                            // Lấy DateTime bên trong kiểu Nullable DateTime
                            DateTime start = bCTBL.DateStart.Value.AddMonths(bCTBL.MonthDiff);
                            DateTime end = bCTBL.DateEnd.Value;
                            // Lấy TimeSpan giữa hai ngày
                            TimeSpan span = end.Subtract(start);
                            // Lấy số ngày chênh lệch
                            bCTBL.DateDiff = (int)span.TotalDays;
                        }
                        if (bCTBL.DateDiff < 0)
                        {
                            bCTBL.DateDiff = 30 + bCTBL.DateDiff;
                            bCTBL.MonthDiff--;
                        }

                        bCTBL.DateCoefficient = (DateTime)item.DisposeTime;

                        bCTBL.PolicyReduction = (decimal)_context.DiscountCoefficients.Where(p => p.Id == rentFileBCT.DiscountCoefficientId && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();


                        foreach (var itemLv in rentingPrices) //Lấy giá chuẩn
                        {
                            if (itemLv.LevelId == item.Level)
                            {
                                bCTBL.StandardPrice = (decimal)itemLv.Price;
                                break;
                            }
                        }

                        foreach (var vat in vats)
                        {
                            if (bCTBL.DateStart >= vat.DoApply)
                            {
                                bCTBL.VAT = vat.Value;
                                break;
                            }
                            else if (vat.DoApply <= bCTBL.DateEnd)
                            {
                                var changevalue = new changeValue();

                                changevalue.Type = 1;
                                changevalue.valueChange = vat.Value;
                                changevalue.DateChange = vat.DoApply;

                                resChange.Add(changevalue);
                                continue;
                            }
                        }

                        foreach (var salaryCoefficient in salaryCoefficients) // lấy giá trị của lương cơ bản theo ngày áp dụng
                        {
                            if (bCTBL.DateStart >= salaryCoefficient.DoApply)
                            {
                                bCTBL.Ktlcb = (decimal)salaryCoefficient.Value;
                                break;
                            }
                            else if (salaryCoefficient.DoApply <= bCTBL.DateEnd)
                            {
                                var changevalue = new changeValue();

                                changevalue.Type = 2;
                                changevalue.valueChange = salaryCoefficient.Value;
                                changevalue.DateChange = salaryCoefficient.DoApply;

                                resChange.Add(changevalue);
                                continue;
                            }
                        }

                        foreach (var coefficient in coefficients) // lấy giá trị của hệ số thời điểm bố trí theo ngày áp dụng
                        {
                            if (bCTBL.DateCoefficient <= coefficient.DoApply)
                            {
                                bCTBL.Ktdbt = (decimal)coefficient.Value;
                                break;
                            }

                        }

                        bCTBL.chilDfs = new List<chilDf>();
                        if (map_rentFlieBCTDatas != null)
                        {
                            foreach (var itemDf in map_rentFlieBCTDatas.groupData)
                            {
                                if (itemDf.childDefaultCoefficients.Count > 0)
                                {
                                    foreach (var itemChild in itemDf.childDefaultCoefficients)
                                    {

                                        if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                        {
                                            string name = "";
                                            if (item.FloorCode < 6)
                                            {
                                                name = bCTBL.AreaName;
                                            }
                                            else
                                            {
                                                name = "Tầng 6 trở lên";
                                            }
                                            string content = _context.DefaultCoefficients.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Content).FirstOrDefault();
                                            if (content == name)
                                            {
                                                if (bCTBL.DateStart >= itemChild.DoApply)
                                                {
                                                    var itemChilDfs = new chilDf();

                                                    itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                    itemChilDfs.Value = itemChild.Value;
                                                    bCTBL.chilDfs.Add(itemChilDfs);
                                                    break;
                                                }
                                                else if (bCTBL.DateEnd >= itemChild.DoApply)
                                                {
                                                    var changeK = new changeK();

                                                    changeK.valueChange = itemChild.Value;
                                                    changeK.DateChange = itemChild.DoApply;
                                                    resChangeK.Add(changeK);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (bCTBL.DateStart >= itemChild.DoApply)
                                            {
                                                var itemChilDfs = new chilDf();

                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                itemChilDfs.Value = itemChild.Value;
                                                bCTBL.chilDfs.Add(itemChilDfs);
                                                break;
                                            }
                                            else if (bCTBL.DateEnd >= itemChild.DoApply)
                                            {
                                                var changeK = new changeK();

                                                changeK.CoefficientId = itemChild.CoefficientId;
                                                changeK.valueChange = itemChild.Value;
                                                changeK.DateChange = itemChild.DoApply;
                                                resChangeK.Add(changeK);
                                            }
                                        }
                                    }
                                }
                            }

                        }

                        var groupdataK = resChangeK.GroupBy(p => p.DateChange).Select(e => new groupChangeK
                        {
                            DateChange = e.Key,
                            groupDataChangeK = e.ToList()
                        });

                        if (bCTBL.StandardPrice == null) bCTBL.StandardPrice = 0;

                        if (bCTBL.chilDfs != null)
                        {
                            bCTBL.TotalK = (float)bCTBL.chilDfs.Sum(p => p.Value) + 1;
                        }
                        else
                        {
                            bCTBL.TotalK = 1;
                        }

                        if (bCTBL.Ktlcb == null) bCTBL.Ktlcb = 1;
                        if (bCTBL.Ktdbt == null) bCTBL.Ktdbt = 1;

                        bCTBL.PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK));

                        bCTBL.PriceRent = Math.Round((decimal)(bCTBL.PriceRent1m2 * (decimal)bCTBL.PrivateArea) - bCTBL.PolicyReduction);

                        bCTBL.Unit = "VNĐ";

                        resBct.Add(bCTBL);

                        decimal K = (decimal)bCTBL.TotalK;
                        if (groupdataK.Count() > 0)
                        {
                            foreach (var itemk in groupdataK)
                            {
                                decimal totalK = 0;
                                decimal CurrentK = K;
                                var changevalue = new changeValue();
                                foreach (var itemK in bCTBL.chilDfs)
                                {
                                    foreach (var itemChangeK in itemk.groupDataChangeK.OrderBy(o => o.DateChange).ToList())
                                    {
                                        if (itemK.CoefficientId == itemChangeK.CoefficientId)
                                        {
                                            totalK = (decimal)(CurrentK - (decimal)itemK.Value);
                                            changevalue.valueChange = (double)(totalK + (decimal)itemChangeK.valueChange);
                                            changevalue.Type = 5;
                                            changevalue.DateChange = itemChangeK.DateChange;
                                            CurrentK = (decimal)changevalue.valueChange;
                                        }
                                    }
                                }
                                K = CurrentK;
                                resChange.Add(changevalue);
                            }
                        }

                        if (resChange.Count > 0)
                        {
                            foreach (var itemChange in resChange.OrderBy(p => p.DateChange).ToList())
                            {
                                k = k + 1;
                                decimal? PriceRent1m2 = 0;
                                decimal? PriceRent = 0;

                                var changeData = new BCT();
                                changeData.TypeQD = 22;
                                changeData.check = false;
                                changeData.GroupId = k;

                                changeData.AreaName = item.AreaName;

                                changeData.Level = (int)item.Level;

                                changeData.DateCoefficient = item.DisposeTime;

                                changeData.PrivateArea = (float)rentFileBCT.Area;

                                changeData.TotalK = resBct[resBct.Count - 1].TotalK;

                                changeData.VAT = resBct[resBct.Count - 1].VAT;
                                changeData.Ktdbt = resBct[resBct.Count - 1].Ktdbt;
                                changeData.Ktlcb = resBct[resBct.Count - 1].Ktlcb;
                                changeData.StandardPrice = resBct[resBct.Count - 1].StandardPrice;
                                changeData.Unit = resBct[resBct.Count - 1].Unit;
                                changeData.PolicyReduction = resBct[resBct.Count - 1].PolicyReduction;
                                if (itemChange.Type == 1)
                                {
                                    changeData.VAT = itemChange.valueChange;
                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);

                                        //tháng
                                        if (resBct[resBct.Count - 1].DateStart.HasValue && resBct[resBct.Count - 1].DateEnd.HasValue)
                                        {
                                            resBct[resBct.Count - 1].MonthDiff = (resBct[resBct.Count - 1].DateEnd.Value.Year - resBct[resBct.Count - 1].DateStart.Value.Year) * 12 + (resBct[resBct.Count - 1].DateEnd.Value.Month - resBct[resBct.Count - 1].DateStart.Value.Month);
                                        }
                                        // ngày
                                        if (resBct[resBct.Count - 1].DateEnd.HasValue && resBct[resBct.Count - 1].DateStart.HasValue)
                                        {
                                            // Lấy DateTime bên trong kiểu Nullable DateTime
                                            DateTime start = resBct[resBct.Count - 1].DateStart.Value.AddMonths(resBct[resBct.Count - 1].MonthDiff);
                                            DateTime end = resBct[resBct.Count - 1].DateEnd.Value;
                                            // Lấy TimeSpan giữa hai ngày
                                            TimeSpan span = end.Subtract(start);
                                            // Lấy số ngày chênh lệch
                                            resBct[resBct.Count - 1].DateDiff = (int)span.TotalDays;
                                        }
                                        if (resBct[resBct.Count - 1].DateDiff < 0)
                                        {
                                            resBct[resBct.Count - 1].DateDiff = 30 + resBct[resBct.Count - 1].DateDiff;
                                            resBct[resBct.Count - 1].MonthDiff--;
                                        }
                                    }

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;



                                    //tháng
                                    if (changeData.DateStart.HasValue && changeData.DateEnd.HasValue)
                                    {
                                        changeData.MonthDiff = (changeData.DateEnd.Value.Year - changeData.DateStart.Value.Year) * 12 + (changeData.DateEnd.Value.Month - changeData.DateStart.Value.Month);
                                    }
                                    // ngày
                                    if (changeData.DateEnd.HasValue && changeData.DateStart.HasValue)
                                    {
                                        // Lấy DateTime bên trong kiểu Nullable DateTime
                                        DateTime start = changeData.DateStart.Value.AddMonths(changeData.MonthDiff);
                                        DateTime end = changeData.DateEnd.Value;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan span = end.Subtract(start);
                                        // Lấy số ngày chênh lệch
                                        changeData.DateDiff = (int)span.TotalDays;
                                    }
                                    if (changeData.DateDiff < 0)
                                    {
                                        changeData.DateDiff = 30 + changeData.DateDiff;
                                        changeData.MonthDiff--;
                                    }

                                    resBct.Add(changeData);
                                }
                                else if (itemChange.Type == 2)
                                {
                                    changeData.Ktlcb = (decimal)itemChange.valueChange;
                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;

                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);



                                        //tháng
                                        if (resBct[resBct.Count - 1].DateStart.HasValue && resBct[resBct.Count - 1].DateEnd.HasValue)
                                        {
                                            resBct[resBct.Count - 1].MonthDiff = (resBct[resBct.Count - 1].DateEnd.Value.Year - resBct[resBct.Count - 1].DateStart.Value.Year) * 12 + (resBct[resBct.Count - 1].DateEnd.Value.Month - resBct[resBct.Count - 1].DateStart.Value.Month);
                                        }
                                        // ngày
                                        if (resBct[resBct.Count - 1].DateEnd.HasValue && resBct[resBct.Count - 1].DateStart.HasValue)
                                        {
                                            // Lấy DateTime bên trong kiểu Nullable DateTime
                                            DateTime start = resBct[resBct.Count - 1].DateStart.Value.AddMonths(resBct[resBct.Count - 1].MonthDiff);
                                            DateTime end = resBct[resBct.Count - 1].DateEnd.Value;
                                            // Lấy TimeSpan giữa hai ngày
                                            TimeSpan span = end.Subtract(start);
                                            // Lấy số ngày chênh lệch
                                            resBct[resBct.Count - 1].DateDiff = (int)span.TotalDays;
                                        }
                                        if (resBct[resBct.Count - 1].DateDiff < 0)
                                        {
                                            resBct[resBct.Count - 1].DateDiff = 30 + resBct[resBct.Count - 1].DateDiff;
                                            resBct[resBct.Count - 1].MonthDiff--;
                                        }
                                    }


                                    //tháng
                                    if (changeData.DateStart.HasValue && changeData.DateEnd.HasValue)
                                    {
                                        changeData.MonthDiff = (changeData.DateEnd.Value.Year - changeData.DateStart.Value.Year) * 12 + (changeData.DateEnd.Value.Month - changeData.DateStart.Value.Month);
                                    }
                                    // ngày
                                    if (changeData.DateEnd.HasValue && changeData.DateStart.HasValue)
                                    {
                                        // Lấy DateTime bên trong kiểu Nullable DateTime
                                        DateTime start = changeData.DateStart.Value.AddMonths(changeData.MonthDiff);
                                        DateTime end = changeData.DateEnd.Value;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan span = end.Subtract(start);
                                        // Lấy số ngày chênh lệch
                                        changeData.DateDiff = (int)span.TotalDays;
                                    }
                                    if (changeData.DateDiff < 0)
                                    {
                                        changeData.DateDiff = 30 + changeData.DateDiff;
                                        changeData.MonthDiff--;
                                    }

                                    resBct.Add(changeData);

                                }
                                else if (itemChange.Type == 3)
                                {
                                    changeData.Ktdbt = (decimal)itemChange.valueChange;
                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;

                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);



                                        //tháng
                                        if (resBct[resBct.Count - 1].DateStart.HasValue && resBct[resBct.Count - 1].DateEnd.HasValue)
                                        {
                                            resBct[resBct.Count - 1].MonthDiff = (resBct[resBct.Count - 1].DateEnd.Value.Year - resBct[resBct.Count - 1].DateStart.Value.Year) * 12 + (resBct[resBct.Count - 1].DateEnd.Value.Month - resBct[resBct.Count - 1].DateStart.Value.Month);
                                        }
                                        // ngày
                                        if (resBct[resBct.Count - 1].DateEnd.HasValue && resBct[resBct.Count - 1].DateStart.HasValue)
                                        {
                                            // Lấy DateTime bên trong kiểu Nullable DateTime
                                            DateTime start = resBct[resBct.Count - 1].DateStart.Value.AddMonths(resBct[resBct.Count - 1].MonthDiff);
                                            DateTime end = resBct[resBct.Count - 1].DateEnd.Value;
                                            // Lấy TimeSpan giữa hai ngày
                                            TimeSpan span = end.Subtract(start);
                                            // Lấy số ngày chênh lệch
                                            resBct[resBct.Count - 1].DateDiff = (int)span.TotalDays;
                                        }
                                        if (resBct[resBct.Count - 1].DateDiff < 0)
                                        {
                                            resBct[resBct.Count - 1].DateDiff = 30 + resBct[resBct.Count - 1].DateDiff;
                                            resBct[resBct.Count - 1].MonthDiff--;
                                        }
                                    }

                                    changeData.chilDfs = new List<chilDf>();
                                    if (map_rentFlieBCTDatas != null)
                                    {
                                        foreach (var itemDf in map_rentFlieBCTDatas.groupData)
                                        {
                                            if (itemDf.childDefaultCoefficients.Count > 0)
                                            {
                                                foreach (var itemChild in itemDf.childDefaultCoefficients)
                                                {
                                                    var itemChilDfs = new chilDf();

                                                    if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                                    {

                                                        string name = "";
                                                        if (item.FloorCode < 6)
                                                        {
                                                            name = changeData.AreaName;
                                                        }
                                                        else
                                                        {
                                                            name = "Tầng 6 trở lên";
                                                        }
                                                        string content = _context.DefaultCoefficients.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Content).FirstOrDefault();
                                                        itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                        if (content == name)
                                                        {
                                                            if (changeData.DateStart >= itemChild.DoApply)
                                                            {
                                                                itemChilDfs.Value = itemChild.Value;
                                                                changeData.chilDfs.Add(itemChilDfs);
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (changeData.DateStart >= itemChild.DoApply)
                                                        {
                                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                            itemChilDfs.Value = itemChild.Value;
                                                            changeData.chilDfs.Add(itemChilDfs);
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                    }


                                    //tháng
                                    if (changeData.DateStart.HasValue && changeData.DateEnd.HasValue)
                                    {
                                        changeData.MonthDiff = (changeData.DateEnd.Value.Year - changeData.DateStart.Value.Year) * 12 + (changeData.DateEnd.Value.Month - changeData.DateStart.Value.Month);
                                    }
                                    // ngày
                                    if (changeData.DateEnd.HasValue && changeData.DateStart.HasValue)
                                    {
                                        // Lấy DateTime bên trong kiểu Nullable DateTime
                                        DateTime start = changeData.DateStart.Value.AddMonths(changeData.MonthDiff);
                                        DateTime end = changeData.DateEnd.Value;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan span = end.Subtract(start);
                                        // Lấy số ngày chênh lệch
                                        changeData.DateDiff = (int)span.TotalDays;
                                    }
                                    if (changeData.DateDiff < 0)
                                    {
                                        changeData.DateDiff = 30 + changeData.DateDiff;
                                        changeData.MonthDiff--;
                                    }
                                    resBct.Add(changeData);
                                }
                                else if (itemChange.Type == 4)
                                {
                                    changeData.PolicyReduction = (decimal)itemChange.valueChange;

                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;

                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);



                                        //tháng
                                        if (resBct[resBct.Count - 1].DateStart.HasValue && resBct[resBct.Count - 1].DateEnd.HasValue)
                                        {
                                            resBct[resBct.Count - 1].MonthDiff = (resBct[resBct.Count - 1].DateEnd.Value.Year - resBct[resBct.Count - 1].DateStart.Value.Year) * 12 + (resBct[resBct.Count - 1].DateEnd.Value.Month - resBct[resBct.Count - 1].DateStart.Value.Month);
                                        }
                                        // ngày
                                        if (resBct[resBct.Count - 1].DateEnd.HasValue && resBct[resBct.Count - 1].DateStart.HasValue)
                                        {
                                            // Lấy DateTime bên trong kiểu Nullable DateTime
                                            DateTime start = resBct[resBct.Count - 1].DateStart.Value.AddMonths(resBct[resBct.Count - 1].MonthDiff);
                                            DateTime end = resBct[resBct.Count - 1].DateEnd.Value;
                                            // Lấy TimeSpan giữa hai ngày
                                            TimeSpan span = end.Subtract(start);
                                            // Lấy số ngày chênh lệch
                                            resBct[resBct.Count - 1].DateDiff = (int)span.TotalDays;
                                        }
                                        if (resBct[resBct.Count - 1].DateDiff < 0)
                                        {
                                            resBct[resBct.Count - 1].DateDiff = 30 + resBct[resBct.Count - 1].DateDiff;
                                            resBct[resBct.Count - 1].MonthDiff--;
                                        }
                                    }



                                    //tháng
                                    if (changeData.DateStart.HasValue && changeData.DateEnd.HasValue)
                                    {
                                        changeData.MonthDiff = (changeData.DateEnd.Value.Year - changeData.DateStart.Value.Year) * 12 + (changeData.DateEnd.Value.Month - changeData.DateStart.Value.Month);
                                    }
                                    // ngày
                                    if (changeData.DateEnd.HasValue && changeData.DateStart.HasValue)
                                    {
                                        // Lấy DateTime bên trong kiểu Nullable DateTime
                                        DateTime start = changeData.DateStart.Value.AddMonths(changeData.MonthDiff);
                                        DateTime end = changeData.DateEnd.Value;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan span = end.Subtract(start);
                                        // Lấy số ngày chênh lệch
                                        changeData.DateDiff = (int)span.TotalDays;
                                    }
                                    if (changeData.DateDiff < 0)
                                    {
                                        changeData.DateDiff = 30 + changeData.DateDiff;
                                        changeData.MonthDiff--;
                                    }

                                    resBct.Add(changeData);

                                }
                                else if (itemChange.Type == 5)
                                {
                                    changeData.TotalK = (float?)itemChange.valueChange;
                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);


                                        //tháng
                                        if (resBct[resBct.Count - 1].DateStart.HasValue && resBct[resBct.Count - 1].DateEnd.HasValue)
                                        {
                                            resBct[resBct.Count - 1].MonthDiff = (resBct[resBct.Count - 1].DateEnd.Value.Year - resBct[resBct.Count - 1].DateStart.Value.Year) * 12 + (resBct[resBct.Count - 1].DateEnd.Value.Month - resBct[resBct.Count - 1].DateStart.Value.Month);
                                        }
                                        // ngày
                                        if (resBct[resBct.Count - 1].DateEnd.HasValue && resBct[resBct.Count - 1].DateStart.HasValue)
                                        {
                                            // Lấy DateTime bên trong kiểu Nullable DateTime
                                            DateTime start = resBct[resBct.Count - 1].DateStart.Value.AddMonths(resBct[resBct.Count - 1].MonthDiff);
                                            DateTime end = resBct[resBct.Count - 1].DateEnd.Value;
                                            // Lấy TimeSpan giữa hai ngày
                                            TimeSpan span = end.Subtract(start);
                                            // Lấy số ngày chênh lệch
                                            resBct[resBct.Count - 1].DateDiff = (int)span.TotalDays;
                                        }
                                        if (resBct[resBct.Count - 1].DateDiff < 0)
                                        {
                                            resBct[resBct.Count - 1].DateDiff = 30 + resBct[resBct.Count - 1].DateDiff;
                                            resBct[resBct.Count - 1].MonthDiff--;
                                        }
                                    }

                                    changeData.chilDfs = new List<chilDf>();
                                    if (map_rentFlieBCTDatas != null)
                                    {
                                        foreach (var itemDf in map_rentFlieBCTDatas.groupData)
                                        {
                                            if (itemDf.childDefaultCoefficients.Count > 0)
                                            {
                                                foreach (var itemChild in itemDf.childDefaultCoefficients)
                                                {

                                                    if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                                    {
                                                        string name = "";
                                                        if (item.FloorCode < 6)
                                                        {
                                                            name = changeData.AreaName;
                                                        }
                                                        else
                                                        {
                                                            name = "Tầng 6 trở lên";
                                                        }
                                                        string content = _context.DefaultCoefficients.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Content).FirstOrDefault();
                                                        if (content == name)
                                                        {
                                                            if (changeData.DateStart >= itemChild.DoApply)
                                                            {
                                                                var itemChilDfs = new chilDf();
                                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;

                                                                itemChilDfs.Value = itemChild.Value;
                                                                changeData.chilDfs.Add(itemChilDfs);
                                                                break;
                                                            }

                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (changeData.DateStart >= itemChild.DoApply)
                                                        {
                                                            var itemChilDfs = new chilDf();

                                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                            itemChilDfs.Value = itemChild.Value;
                                                            changeData.chilDfs.Add(itemChilDfs);
                                                            break;
                                                        }

                                                    }
                                                }
                                            }
                                        }


                                    }

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;


                                    //tháng
                                    if (changeData.DateStart.HasValue && changeData.DateEnd.HasValue)
                                    {
                                        changeData.MonthDiff = (changeData.DateEnd.Value.Year - changeData.DateStart.Value.Year) * 12 + (changeData.DateEnd.Value.Month - changeData.DateStart.Value.Month);
                                    }
                                    // ngày
                                    if (changeData.DateEnd.HasValue && changeData.DateStart.HasValue)
                                    {
                                        // Lấy DateTime bên trong kiểu Nullable DateTime
                                        DateTime start = changeData.DateStart.Value.AddMonths(changeData.MonthDiff);
                                        DateTime end = changeData.DateEnd.Value;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan span = end.Subtract(start);
                                        // Lấy số ngày chênh lệch
                                        changeData.DateDiff = (int)span.TotalDays;
                                    }
                                    if (changeData.DateDiff < 0)
                                    {
                                        changeData.DateDiff = 30 + changeData.DateDiff;
                                        changeData.MonthDiff--;
                                    }
                                    resBct.Add(changeData);
                                }
                            }
                        }
                    }
                }

                var groupData = resBct.GroupBy(f => f.GroupId != null ? f.GroupId : new object(), key => key).Select(f => new groupData
                {
                    GroupId = (int)f.Key,
                    VAT = f.ToList().First().VAT,
                    DateStart = (DateTime)f.ToList().First().DateStart,
                    DateEnd = f.ToList().Last().DateEnd,
                    totalPrice = (decimal)f.ToList().Sum(p => p.PriceRent),
                    bCTs = f.ToList()
                }).ToList();
                return groupData;
            }
            catch (Exception ex)
            {
                log.Error("getDataBCT22:" + ex);
                return null;
            }
        }

        private List<groupData> getDataBCT22HD(Guid id, int RentFileBCTID)
        {
            try
            {
                RentFile rentFile = _context.RentFiles.Where(l => l.Id == id && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                RentFlieData map_rentFlieData = _mapper.Map<RentFlieData>(rentFile);

                //Tìm bảng chiết tính mới nhất theo ID hồ sơ
                RentFileBCT rentFileBCT = _context.RentFileBCTs.Where(l => l.RentFileId == id && l.TypeBCT == 1).FirstOrDefault();

                RentFileBCTData map_rentFlieBCTDatas = _mapper.Map<RentFileBCTData>(rentFileBCT);

                List<DefaultCoefficient> dataDf = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderBy(p => p.CoefficientName).ToList();
                var groupedData = dataDf.GroupBy(x => x.CoefficientName).Select(e => new GroupData
                {
                    CoefficientId = e.Key,
                    defaultCoefficients = e.ToList(),
                    childDefaultCoefficients = _context.ChildDefaults.Where(l => l.RentFileBctId == map_rentFlieBCTDatas.Id && l.CoefficientId == e.Key && l.Type == 1 && l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList()
                }).ToList();
                map_rentFlieBCTDatas.groupData = groupedData;

                List<Vat> vats = _context.Vats.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList();

                List<Salary> salaryCoefficients = _context.Salaries.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList();

                List<Coefficient> coefficients = _context.Coefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderBy(p => p.DoApply).ToList();

                List<Conversion> conversions = _context.Conversions.Where(l => l.TypeQD == AppEnums.TypeQD.QD_22).ToList();

                List<DefaultCoefficient> defaultCoefficients = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                Block blocks = _context.Blocks.Where(l => l.Id == rentFile.RentBlockId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                List<RentingPrice> rentingPrices = _context.RentingPricies.Where(l => l.TypeQD == AppEnums.TypeQD.QD_22 && l.TypeBlockId == blocks.TypeBlockId && l.Status != AppEnums.EntityStatus.DELETED).ToList();

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

                List<BCT> resBct = new List<BCT>();

                int i = 0;
                int check = 0;

                decimal sum = 0;

                if (rentFile.RentApartmentId > 0)
                {
                    Apartment apartment = _context.Apartments.Where(l => l.Id == rentFile.RentApartmentId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    ApartmentData map_apartments = _mapper.Map<ApartmentData>(apartment);
                    List<ApartmentDetail> apartmentDetails = _context.ApartmentDetails.Where(a => a.TargetId == rentFile.RentApartmentId && a.Type == TypeApartmentDetail.APARTMENT && a.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                    List<ApartmentDetailData> apartmentDetailData = _mapper.Map<List<ApartmentDetailData>>(apartmentDetails);
                    foreach (ApartmentDetailData map_apartmentDetail in apartmentDetailData)
                    {
                        Area area = _context.Areas.Where(f => f.Id == map_apartmentDetail.AreaId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                        map_apartmentDetail.AreaName = area != null ? area.Name : "";
                        map_apartmentDetail.IsMezzanine = area != null ? area.IsMezzanine : null;

                        Floor floor = _context.Floors.Where(f => f.Id == map_apartmentDetail.FloorId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                        map_apartmentDetail.FloorName = floor != null ? floor.Name : "";
                        map_apartmentDetail.FloorCode = floor != null ? floor.Code : 0;
                    }
                    map_apartments.apartmentDetailData = apartmentDetailData.ToList();
                    foreach (var item in map_apartments.apartmentDetailData)
                    {

                        int k = 0;
                        var bCT = new BCT();
                        List<changeValue> resChange = new List<changeValue>();
                        List<changeK> resChangeK = new List<changeK>();

                        bCT.check = false;
                        bCT.GroupId = k;
                        bCT.TypeQD = 22;
                        bCT.AreaName = item.AreaName;

                        bCT.Level = (int)item.Level;

                        bCT.PrivateArea = item.PrivateArea;

                        bCT.DateStart = rentFile.DateHD;

                        bCT.DateEnd = rentFile.DateHD.AddMonths(rentFile.Month);

                        //tháng
                        if (bCT.DateStart.HasValue && bCT.DateEnd.HasValue)
                        {
                            bCT.MonthDiff = (bCT.DateEnd.Value.Year - bCT.DateStart.Value.Year) * 12 + (bCT.DateEnd.Value.Month - bCT.DateStart.Value.Month);
                        }
                        // ngày
                        if (bCT.DateEnd.HasValue && bCT.DateStart.HasValue)
                        {
                            // Lấy DateTime bên trong kiểu Nullable DateTime
                            DateTime start = bCT.DateStart.Value.AddMonths(bCT.MonthDiff);
                            DateTime end = bCT.DateEnd.Value;
                            // Lấy TimeSpan giữa hai ngày
                            TimeSpan span = end.Subtract(start);
                            // Lấy số ngày chênh lệch
                            bCT.DateDiff = (int)span.TotalDays;
                        }
                        if (bCT.DateDiff < 0)
                        {
                            bCT.DateDiff = 30 + bCT.DateDiff;
                            bCT.MonthDiff--;
                        }

                        bCT.DateCoefficient = (DateTime)item.DisposeTime;

                        bCT.PolicyReduction = (decimal)_context.DiscountCoefficients.Where(p => p.Id == rentFileBCT.DiscountCoefficientId && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();


                        foreach (var itemLv in rentingPrices) //Lấy giá chuẩn
                        {
                            if (itemLv.LevelId == item.Level)
                            {
                                bCT.StandardPrice = (decimal)itemLv.Price;
                                break;
                            }
                        }

                        foreach (var vat in vats)
                        {
                            if (bCT.DateStart >= vat.DoApply)
                            {
                                bCT.VAT = vat.Value;
                                break;
                            }
                            else if (vat.DoApply <= bCT.DateEnd)
                            {
                                var changevalue = new changeValue();

                                changevalue.Type = 1;
                                changevalue.valueChange = vat.Value;
                                changevalue.DateChange = vat.DoApply;

                                resChange.Add(changevalue);
                                continue;
                            }
                        }

                        foreach (var salaryCoefficient in salaryCoefficients) // lấy giá trị của lương cơ bản theo ngày áp dụng
                        {
                            if (bCT.DateStart >= salaryCoefficient.DoApply)
                            {
                                bCT.Ktlcb = (decimal)salaryCoefficient.Value;
                                break;
                            }
                            else if (salaryCoefficient.DoApply <= bCT.DateEnd)
                            {
                                var changevalue = new changeValue();

                                changevalue.Type = 2;
                                changevalue.valueChange = salaryCoefficient.Value;
                                changevalue.DateChange = salaryCoefficient.DoApply;

                                resChange.Add(changevalue);
                                continue;
                            }
                        }

                        foreach (var coefficient in coefficients) // lấy giá trị của hệ số thời điểm bố trí theo ngày áp dụng
                        {
                            if (bCT.DateCoefficient <= coefficient.DoApply)
                            {
                                bCT.Ktdbt = (decimal)coefficient.Value;
                                break;
                            }

                        }

                        bCT.chilDfs = new List<chilDf>();
                        if (map_rentFlieBCTDatas != null)
                        {
                            foreach (var itemDf in map_rentFlieBCTDatas.groupData)
                            {
                                if (itemDf.childDefaultCoefficients.Count > 0)
                                {
                                    foreach (var itemChild in itemDf.childDefaultCoefficients)
                                    {

                                        if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                        {

                                            string name = "";
                                            if (item.FloorCode < 6)
                                            {
                                                name = bCT.AreaName;
                                            }
                                            else
                                            {
                                                name = "Tầng 6 trở lên";
                                            }
                                            string content = _context.DefaultCoefficients.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Content).FirstOrDefault();
                                            if (content == name)
                                            {
                                                if (bCT.DateStart >= itemChild.DoApply)
                                                {
                                                    var itemChilDfs = new chilDf();

                                                    itemChilDfs.Value = itemChild.Value;
                                                    bCT.chilDfs.Add(itemChilDfs);
                                                    break;
                                                }
                                                else if (bCT.DateEnd >= itemChild.DoApply)
                                                {
                                                    var changeK = new changeK();

                                                    changeK.valueChange = itemChild.Value;
                                                    changeK.DateChange = itemChild.DoApply;
                                                    resChangeK.Add(changeK);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (bCT.DateStart >= itemChild.DoApply)
                                            {
                                                var itemChilDfs = new chilDf();

                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                itemChilDfs.Value = itemChild.Value;
                                                bCT.chilDfs.Add(itemChilDfs);
                                                break;
                                            }
                                            else if (bCT.DateEnd >= itemChild.DoApply)
                                            {
                                                var changeK = new changeK();

                                                changeK.CoefficientId = itemChild.CoefficientId;
                                                changeK.valueChange = itemChild.Value;
                                                changeK.DateChange = itemChild.DoApply;
                                                resChangeK.Add(changeK);
                                            }
                                        }
                                    }
                                }
                            }


                        }

                        var groupdataK = resChangeK.GroupBy(p => p.DateChange).Select(e => new groupChangeK
                        {
                            DateChange = e.Key,
                            groupDataChangeK = e.ToList()
                        });

                        if (bCT.StandardPrice == null) bCT.StandardPrice = 0;

                        if (bCT.chilDfs != null)
                        {
                            bCT.TotalK = (float)bCT.chilDfs.Sum(p => p.Value) + 1;
                        }
                        else
                        {
                            bCT.TotalK = 1;
                        }

                        if (bCT.Ktlcb == null) bCT.Ktlcb = 1;
                        if (bCT.Ktdbt == null) bCT.Ktdbt = 1;

                        bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * bCT.Ktlcb * bCT.Ktdbt * (decimal)bCT.TotalK));

                        bCT.PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea));

                        bCT.Unit = "VNĐ";

                        resBct.Add(bCT);

                        if (groupdataK.Count() > 0)
                        {
                            foreach (var itemk in groupdataK)
                            {
                                decimal totalK = 0;
                                decimal CurrentK = (decimal)bCT.TotalK;
                                var changevalue = new changeValue();
                                foreach (var itemK in bCT.chilDfs)
                                {
                                    foreach (var itemChangeK in itemk.groupDataChangeK.OrderBy(o => o.DateChange).ToList())
                                    {
                                        if (itemK.CoefficientId == itemChangeK.CoefficientId)
                                        {
                                            totalK = (decimal)(CurrentK - (decimal)itemK.Value);
                                            changevalue.valueChange = (double)(totalK + (decimal)itemChangeK.valueChange);
                                            changevalue.Type = 5;
                                            changevalue.DateChange = itemChangeK.DateChange;
                                            CurrentK = (decimal)changevalue.valueChange;
                                        }
                                    }
                                }
                                resChange.Add(changevalue);
                            }
                        }

                        if (resChange.Count > 0)
                        {
                            foreach (var itemChange in resChange.OrderBy(p => p.DateChange).ToList())
                            {
                                k = k + 1;
                                decimal? PriceRent1m2 = 0;
                                decimal? PriceRent = 0;

                                var changeData = new BCT();
                                changeData.TypeQD = 22;
                                changeData.check = false;
                                changeData.GroupId = k;

                                changeData.AreaName = item.AreaName;

                                changeData.Level = (int)item.Level;

                                changeData.DateCoefficient = item.DisposeTime;

                                changeData.PrivateArea = item.PrivateArea;

                                changeData.VAT = resBct[resBct.Count - 1].VAT;
                                changeData.Ktdbt = resBct[resBct.Count - 1].Ktdbt;
                                changeData.Ktlcb = resBct[resBct.Count - 1].Ktlcb;
                                changeData.StandardPrice = resBct[resBct.Count - 1].StandardPrice;
                                changeData.Unit = resBct[resBct.Count - 1].Unit;
                                changeData.PolicyReduction = resBct[resBct.Count - 1].PolicyReduction;
                                if (itemChange.Type == 1)
                                {
                                    changeData.VAT = itemChange.valueChange;
                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);

                                        //tháng
                                        if (resBct[resBct.Count - 1].DateStart.HasValue && resBct[resBct.Count - 1].DateEnd.HasValue)
                                        {
                                            resBct[resBct.Count - 1].MonthDiff = (resBct[resBct.Count - 1].DateEnd.Value.Year - resBct[resBct.Count - 1].DateStart.Value.Year) * 12 + (resBct[resBct.Count - 1].DateEnd.Value.Month - resBct[resBct.Count - 1].DateStart.Value.Month);
                                        }
                                        // ngày
                                        if (resBct[resBct.Count - 1].DateEnd.HasValue && resBct[resBct.Count - 1].DateStart.HasValue)
                                        {
                                            // Lấy DateTime bên trong kiểu Nullable DateTime
                                            DateTime start = resBct[resBct.Count - 1].DateStart.Value.AddMonths(resBct[resBct.Count - 1].MonthDiff);
                                            DateTime end = resBct[resBct.Count - 1].DateEnd.Value;
                                            // Lấy TimeSpan giữa hai ngày
                                            TimeSpan span = end.Subtract(start);
                                            // Lấy số ngày chênh lệch
                                            resBct[resBct.Count - 1].DateDiff = (int)span.TotalDays;
                                        }
                                        if (resBct[resBct.Count - 1].DateDiff < 0)
                                        {
                                            resBct[resBct.Count - 1].DateDiff = 30 + resBct[resBct.Count - 1].DateDiff;
                                            resBct[resBct.Count - 1].MonthDiff--;
                                        }
                                    }


                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;

                                    //tháng
                                    if (changeData.DateStart.HasValue && changeData.DateEnd.HasValue)
                                    {
                                        changeData.MonthDiff = (changeData.DateEnd.Value.Year - changeData.DateStart.Value.Year) * 12 + (changeData.DateEnd.Value.Month - changeData.DateStart.Value.Month);
                                    }
                                    // ngày
                                    if (changeData.DateEnd.HasValue && changeData.DateStart.HasValue)
                                    {
                                        // Lấy DateTime bên trong kiểu Nullable DateTime
                                        DateTime start = changeData.DateStart.Value.AddMonths(changeData.MonthDiff);
                                        DateTime end = changeData.DateEnd.Value;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan span = end.Subtract(start);
                                        // Lấy số ngày chênh lệch
                                        changeData.DateDiff = (int)span.TotalDays;
                                    }
                                    if (changeData.DateDiff < 0)
                                    {
                                        changeData.DateDiff = 30 + changeData.DateDiff;
                                        changeData.MonthDiff--;
                                    }
                                    resBct.Add(changeData);
                                }
                                else if (itemChange.Type == 2)
                                {
                                    changeData.Ktlcb = (decimal)itemChange.valueChange;
                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));
                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));



                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);

                                        //tháng
                                        if (resBct[resBct.Count - 1].DateStart.HasValue && resBct[resBct.Count - 1].DateEnd.HasValue)
                                        {
                                            resBct[resBct.Count - 1].MonthDiff = (resBct[resBct.Count - 1].DateEnd.Value.Year - resBct[resBct.Count - 1].DateStart.Value.Year) * 12 + (resBct[resBct.Count - 1].DateEnd.Value.Month - resBct[resBct.Count - 1].DateStart.Value.Month);
                                        }
                                        // ngày
                                        if (resBct[resBct.Count - 1].DateEnd.HasValue && resBct[resBct.Count - 1].DateStart.HasValue)
                                        {
                                            // Lấy DateTime bên trong kiểu Nullable DateTime
                                            DateTime start = resBct[resBct.Count - 1].DateStart.Value.AddMonths(resBct[resBct.Count - 1].MonthDiff);
                                            DateTime end = resBct[resBct.Count - 1].DateEnd.Value;
                                            // Lấy TimeSpan giữa hai ngày
                                            TimeSpan span = end.Subtract(start);
                                            // Lấy số ngày chênh lệch
                                            resBct[resBct.Count - 1].DateDiff = (int)span.TotalDays;
                                        }
                                        if (resBct[resBct.Count - 1].DateDiff < 0)
                                        {
                                            resBct[resBct.Count - 1].DateDiff = 30 + resBct[resBct.Count - 1].DateDiff;
                                            resBct[resBct.Count - 1].MonthDiff--;
                                        }
                                    }

                                    //tháng
                                    if (changeData.DateStart.HasValue && changeData.DateEnd.HasValue)
                                    {
                                        changeData.MonthDiff = (changeData.DateEnd.Value.Year - changeData.DateStart.Value.Year) * 12 + (changeData.DateEnd.Value.Month - changeData.DateStart.Value.Month);
                                    }
                                    // ngày
                                    if (changeData.DateEnd.HasValue && changeData.DateStart.HasValue)
                                    {
                                        // Lấy DateTime bên trong kiểu Nullable DateTime
                                        DateTime start = changeData.DateStart.Value.AddMonths(changeData.MonthDiff);
                                        DateTime end = changeData.DateEnd.Value;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan span = end.Subtract(start);
                                        // Lấy số ngày chênh lệch
                                        changeData.DateDiff = (int)span.TotalDays;
                                    }
                                    if (changeData.DateDiff < 0)
                                    {
                                        changeData.DateDiff = 30 + changeData.DateDiff;
                                        changeData.MonthDiff--;
                                    }

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;

                                    resBct.Add(changeData);

                                }
                                else if (itemChange.Type == 3)
                                {
                                    changeData.Ktdbt = (decimal)itemChange.valueChange;
                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));


                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;

                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);


                                        //tháng
                                        if (resBct[resBct.Count - 1].DateStart.HasValue && resBct[resBct.Count - 1].DateEnd.HasValue)
                                        {
                                            resBct[resBct.Count - 1].MonthDiff = (resBct[resBct.Count - 1].DateEnd.Value.Year - resBct[resBct.Count - 1].DateStart.Value.Year) * 12 + (resBct[resBct.Count - 1].DateEnd.Value.Month - resBct[resBct.Count - 1].DateStart.Value.Month);
                                        }
                                        // ngày
                                        if (resBct[resBct.Count - 1].DateEnd.HasValue && resBct[resBct.Count - 1].DateStart.HasValue)
                                        {
                                            // Lấy DateTime bên trong kiểu Nullable DateTime
                                            DateTime start = resBct[resBct.Count - 1].DateStart.Value.AddMonths(resBct[resBct.Count - 1].MonthDiff);
                                            DateTime end = resBct[resBct.Count - 1].DateEnd.Value;
                                            // Lấy TimeSpan giữa hai ngày
                                            TimeSpan span = end.Subtract(start);
                                            // Lấy số ngày chênh lệch
                                            resBct[resBct.Count - 1].DateDiff = (int)span.TotalDays;
                                        }
                                        if (resBct[resBct.Count - 1].DateDiff < 0)
                                        {
                                            resBct[resBct.Count - 1].DateDiff = 30 + resBct[resBct.Count - 1].DateDiff;
                                            resBct[resBct.Count - 1].MonthDiff--;
                                        }
                                    }

                                    changeData.chilDfs = new List<chilDf>();
                                    if (map_rentFlieBCTDatas != null)
                                    {
                                        foreach (var itemDf in map_rentFlieBCTDatas.groupData)
                                        {
                                            if (itemDf.childDefaultCoefficients.Count > 0)
                                            {
                                                foreach (var itemChild in itemDf.childDefaultCoefficients)
                                                {
                                                    var itemChilDfs = new chilDf();

                                                    if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                                    {

                                                        string name = "";
                                                        if (item.FloorCode < 6)
                                                        {
                                                            name = changeData.AreaName;
                                                        }
                                                        else
                                                        {
                                                            name = "Tầng 6 trở lên";
                                                        }
                                                        string content = _context.DefaultCoefficients.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Content).FirstOrDefault();
                                                        itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                        if (content == name)
                                                        {
                                                            if (changeData.DateStart >= itemChild.DoApply)
                                                            {
                                                                itemChilDfs.Value = itemChild.Value;
                                                                changeData.chilDfs.Add(itemChilDfs);
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (changeData.DateStart >= itemChild.DoApply)
                                                        {
                                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                            itemChilDfs.Value = itemChild.Value;
                                                            changeData.chilDfs.Add(itemChilDfs);
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                    }

                                    //tháng
                                    if (changeData.DateStart.HasValue && changeData.DateEnd.HasValue)
                                    {
                                        changeData.MonthDiff = (changeData.DateEnd.Value.Year - changeData.DateStart.Value.Year) * 12 + (changeData.DateEnd.Value.Month - changeData.DateStart.Value.Month);
                                    }
                                    // ngày
                                    if (changeData.DateEnd.HasValue && changeData.DateStart.HasValue)
                                    {
                                        // Lấy DateTime bên trong kiểu Nullable DateTime
                                        DateTime start = changeData.DateStart.Value.AddMonths(changeData.MonthDiff);
                                        DateTime end = changeData.DateEnd.Value;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan span = end.Subtract(start);
                                        // Lấy số ngày chênh lệch
                                        changeData.DateDiff = (int)span.TotalDays;
                                    }
                                    if (changeData.DateDiff < 0)
                                    {
                                        changeData.DateDiff = 30 + changeData.DateDiff;
                                        changeData.MonthDiff--;
                                    }

                                    resBct.Add(changeData);
                                }
                                else if (itemChange.Type == 4)
                                {
                                    changeData.PolicyReduction = (decimal)itemChange.valueChange;

                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;

                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);

                                        //tháng
                                        if (resBct[resBct.Count - 1].DateStart.HasValue && resBct[resBct.Count - 1].DateEnd.HasValue)
                                        {
                                            resBct[resBct.Count - 1].MonthDiff = (resBct[resBct.Count - 1].DateEnd.Value.Year - resBct[resBct.Count - 1].DateStart.Value.Year) * 12 + (resBct[resBct.Count - 1].DateEnd.Value.Month - resBct[resBct.Count - 1].DateStart.Value.Month);
                                        }
                                        // ngày
                                        if (resBct[resBct.Count - 1].DateEnd.HasValue && resBct[resBct.Count - 1].DateStart.HasValue)
                                        {
                                            // Lấy DateTime bên trong kiểu Nullable DateTime
                                            DateTime start = resBct[resBct.Count - 1].DateStart.Value.AddMonths(resBct[resBct.Count - 1].MonthDiff);
                                            DateTime end = resBct[resBct.Count - 1].DateEnd.Value;
                                            // Lấy TimeSpan giữa hai ngày
                                            TimeSpan span = end.Subtract(start);
                                            // Lấy số ngày chênh lệch
                                            resBct[resBct.Count - 1].DateDiff = (int)span.TotalDays;
                                        }
                                        if (resBct[resBct.Count - 1].DateDiff < 0)
                                        {
                                            resBct[resBct.Count - 1].DateDiff = 30 + resBct[resBct.Count - 1].DateDiff;
                                            resBct[resBct.Count - 1].MonthDiff--;
                                        }

                                    }

                                    //tháng
                                    if (changeData.DateStart.HasValue && changeData.DateEnd.HasValue)
                                    {
                                        changeData.MonthDiff = (changeData.DateEnd.Value.Year - changeData.DateStart.Value.Year) * 12 + (changeData.DateEnd.Value.Month - changeData.DateStart.Value.Month);
                                    }
                                    // ngày
                                    if (changeData.DateEnd.HasValue && changeData.DateStart.HasValue)
                                    {
                                        // Lấy DateTime bên trong kiểu Nullable DateTime
                                        DateTime start = changeData.DateStart.Value.AddMonths(changeData.MonthDiff);
                                        DateTime end = changeData.DateEnd.Value;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan span = end.Subtract(start);
                                        // Lấy số ngày chênh lệch
                                        changeData.DateDiff = (int)span.TotalDays;
                                    }
                                    if (changeData.DateDiff < 0)
                                    {
                                        changeData.DateDiff = 30 + changeData.DateDiff;
                                        changeData.MonthDiff--;
                                    }

                                    resBct.Add(changeData);

                                }
                                else if (itemChange.Type == 5)
                                {
                                    changeData.TotalK = (float?)itemChange.valueChange;
                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);



                                        //tháng
                                        if (resBct[resBct.Count - 1].DateStart.HasValue && resBct[resBct.Count - 1].DateEnd.HasValue)
                                        {
                                            resBct[resBct.Count - 1].MonthDiff = (resBct[resBct.Count - 1].DateEnd.Value.Year - resBct[resBct.Count - 1].DateStart.Value.Year) * 12 + (resBct[resBct.Count - 1].DateEnd.Value.Month - resBct[resBct.Count - 1].DateStart.Value.Month);
                                        }
                                        // ngày
                                        if (resBct[resBct.Count - 1].DateEnd.HasValue && resBct[resBct.Count - 1].DateStart.HasValue)
                                        {
                                            // Lấy DateTime bên trong kiểu Nullable DateTime
                                            DateTime start = resBct[resBct.Count - 1].DateStart.Value.AddMonths(resBct[resBct.Count - 1].MonthDiff);
                                            DateTime end = resBct[resBct.Count - 1].DateEnd.Value;
                                            // Lấy TimeSpan giữa hai ngày
                                            TimeSpan span = end.Subtract(start);
                                            // Lấy số ngày chênh lệch
                                            resBct[resBct.Count - 1].DateDiff = (int)span.TotalDays;
                                        }
                                        if (resBct[resBct.Count - 1].DateDiff < 0)
                                        {
                                            resBct[resBct.Count - 1].DateDiff = 30 + resBct[resBct.Count - 1].DateDiff;
                                            resBct[resBct.Count - 1].MonthDiff--;
                                        }

                                    }

                                    changeData.chilDfs = new List<chilDf>();
                                    if (map_rentFlieBCTDatas != null)
                                    {
                                        foreach (var itemDf in map_rentFlieBCTDatas.groupData)
                                        {
                                            if (itemDf.childDefaultCoefficients.Count > 0)
                                            {
                                                foreach (var itemChild in itemDf.childDefaultCoefficients)
                                                {
                                                    var itemChilDfs = new chilDf();

                                                    if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                                    {
                                                        string name = "";
                                                        if (item.FloorCode < 6)
                                                        {
                                                            name = changeData.AreaName;
                                                        }
                                                        else
                                                        {
                                                            name = "Tầng 6 trở lên";
                                                        }
                                                        string content = _context.DefaultCoefficients.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Content).FirstOrDefault();
                                                        itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                        if (content == name)
                                                        {
                                                            if (changeData.DateStart >= itemChild.DoApply)
                                                            {
                                                                itemChilDfs.Value = itemChild.Value;
                                                                changeData.chilDfs.Add(itemChilDfs);
                                                                break;
                                                            }

                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (changeData.DateStart >= itemChild.DoApply)
                                                        {
                                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                            itemChilDfs.Value = itemChild.Value;
                                                            changeData.chilDfs.Add(itemChilDfs);
                                                            break;
                                                        }

                                                    }
                                                }
                                            }
                                        }


                                    }

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;



                                    //tháng
                                    if (changeData.DateStart.HasValue && changeData.DateEnd.HasValue)
                                    {
                                        changeData.MonthDiff = (changeData.DateEnd.Value.Year - changeData.DateStart.Value.Year) * 12 + (changeData.DateEnd.Value.Month - changeData.DateStart.Value.Month);
                                    }
                                    // ngày
                                    if (changeData.DateEnd.HasValue && changeData.DateStart.HasValue)
                                    {
                                        // Lấy DateTime bên trong kiểu Nullable DateTime
                                        DateTime start = changeData.DateStart.Value.AddMonths(changeData.MonthDiff);
                                        DateTime end = changeData.DateEnd.Value;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan span = end.Subtract(start);
                                        // Lấy số ngày chênh lệch
                                        changeData.DateDiff = (int)span.TotalDays;
                                    }
                                    if (changeData.DateDiff < 0)
                                    {
                                        changeData.DateDiff = 30 + changeData.DateDiff;
                                        changeData.MonthDiff--;
                                    }

                                    resBct.Add(changeData);
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (var item in map_blocks.blockDetails)
                    {
                        int k = 0;
                        var bCTBL = new BCT();
                        List<changeValue> resChange = new List<changeValue>();
                        List<changeK> resChangeK = new List<changeK>();

                        bCTBL.TypeQD = 22;
                        bCTBL.check = false;
                        bCTBL.GroupId = k;

                        bCTBL.AreaName = item.AreaName;

                        bCTBL.Level = (int)item.Level;

                        bCTBL.PrivateArea = item.PrivateArea;

                        bCTBL.DateStart = rentFile.DateHD;

                        bCTBL.DateEnd = rentFile.DateHD.AddMonths(rentFile.Month);

                        //tháng
                        if (bCTBL.DateStart.HasValue && bCTBL.DateEnd.HasValue)
                        {
                            bCTBL.MonthDiff = (bCTBL.DateEnd.Value.Year - bCTBL.DateStart.Value.Year) * 12 + (bCTBL.DateEnd.Value.Month - bCTBL.DateStart.Value.Month);
                        }
                        // ngày
                        if (bCTBL.DateEnd.HasValue && bCTBL.DateStart.HasValue)
                        {
                            // Lấy DateTime bên trong kiểu Nullable DateTime
                            DateTime start = bCTBL.DateStart.Value.AddMonths(bCTBL.MonthDiff);
                            DateTime end = bCTBL.DateEnd.Value;
                            // Lấy TimeSpan giữa hai ngày
                            TimeSpan span = end.Subtract(start);
                            // Lấy số ngày chênh lệch
                            bCTBL.DateDiff = (int)span.TotalDays;
                        }
                        if (bCTBL.DateDiff < 0)
                        {
                            bCTBL.DateDiff = 30 + bCTBL.DateDiff;
                            bCTBL.MonthDiff--;
                        }

                        bCTBL.DateCoefficient = (DateTime)item.DisposeTime;

                        bCTBL.PolicyReduction = (decimal)_context.DiscountCoefficients.Where(p => p.Id == rentFileBCT.DiscountCoefficientId && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();


                        foreach (var itemLv in rentingPrices) //Lấy giá chuẩn
                        {
                            if (itemLv.LevelId == item.Level)
                            {
                                bCTBL.StandardPrice = (decimal)itemLv.Price;
                                break;
                            }
                        }

                        foreach (var vat in vats)
                        {
                            if (bCTBL.DateStart >= vat.DoApply)
                            {
                                bCTBL.VAT = vat.Value;
                                break;
                            }
                            else if (vat.DoApply <= bCTBL.DateEnd)
                            {
                                var changevalue = new changeValue();

                                changevalue.Type = 1;
                                changevalue.valueChange = vat.Value;
                                changevalue.DateChange = vat.DoApply;

                                resChange.Add(changevalue);
                                continue;
                            }
                        }

                        foreach (var salaryCoefficient in salaryCoefficients) // lấy giá trị của lương cơ bản theo ngày áp dụng
                        {
                            if (bCTBL.DateStart >= salaryCoefficient.DoApply)
                            {
                                bCTBL.Ktlcb = (decimal)salaryCoefficient.Value;
                                break;
                            }
                            else if (salaryCoefficient.DoApply <= bCTBL.DateEnd)
                            {
                                var changevalue = new changeValue();

                                changevalue.Type = 2;
                                changevalue.valueChange = salaryCoefficient.Value;
                                changevalue.DateChange = salaryCoefficient.DoApply;

                                resChange.Add(changevalue);
                                continue;
                            }
                        }

                        foreach (var coefficient in coefficients) // lấy giá trị của hệ số thời điểm bố trí theo ngày áp dụng
                        {
                            if (bCTBL.DateCoefficient <= coefficient.DoApply)
                            {
                                bCTBL.Ktdbt = (decimal)coefficient.Value;
                                break;
                            }

                        }

                        bCTBL.chilDfs = new List<chilDf>();

                        if (map_rentFlieBCTDatas != null)
                        {
                            foreach (var itemDf in map_rentFlieBCTDatas.groupData)
                            {
                                if (itemDf.childDefaultCoefficients.Count > 0)
                                {
                                    foreach (var itemChild in itemDf.childDefaultCoefficients)
                                    {

                                        if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                        {

                                            string name = "";
                                            if (item.FloorCode < 6)
                                            {
                                                name = bCTBL.AreaName;
                                            }
                                            else
                                            {
                                                name = "Tầng 6 trở lên";
                                            }
                                            string content = _context.DefaultCoefficients.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Content).FirstOrDefault();
                                            if (content == name)
                                            {
                                                if (bCTBL.DateStart >= itemChild.DoApply)
                                                {
                                                    var itemChilDfs = new chilDf();

                                                    itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                    itemChilDfs.Value = itemChild.Value;
                                                    bCTBL.chilDfs.Add(itemChilDfs);
                                                    break;
                                                }
                                                else if (bCTBL.DateEnd >= itemChild.DoApply)
                                                {
                                                    var changeK = new changeK();

                                                    changeK.valueChange = itemChild.Value;
                                                    changeK.DateChange = itemChild.DoApply;
                                                    resChangeK.Add(changeK);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (bCTBL.DateStart >= itemChild.DoApply)
                                            {
                                                var itemChilDfs = new chilDf();

                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                itemChilDfs.Value = itemChild.Value;
                                                bCTBL.chilDfs.Add(itemChilDfs);
                                                break;
                                            }
                                            else if (bCTBL.DateEnd >= itemChild.DoApply)
                                            {
                                                var changeK = new changeK();

                                                changeK.CoefficientId = itemChild.CoefficientId;
                                                changeK.valueChange = itemChild.Value;
                                                changeK.DateChange = itemChild.DoApply;
                                                resChangeK.Add(changeK);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        var groupdataK = resChangeK.GroupBy(p => p.DateChange).Select(e => new groupChangeK
                        {
                            DateChange = e.Key,
                            groupDataChangeK = e.ToList()
                        });

                        if (bCTBL.StandardPrice == null) bCTBL.StandardPrice = 0;

                        if (bCTBL.chilDfs != null)
                        {
                            bCTBL.TotalK = (float)bCTBL.chilDfs.Sum(p => p.Value) + 1;
                        }
                        else
                        {
                            bCTBL.TotalK = 1;
                        }

                        if (bCTBL.Ktlcb == null) bCTBL.Ktlcb = 1;
                        if (bCTBL.Ktdbt == null) bCTBL.Ktdbt = 1;

                        bCTBL.PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK));

                        bCTBL.PriceRent = Math.Round((decimal)(bCTBL.PriceRent1m2 * (decimal)bCTBL.PrivateArea) - bCTBL.PolicyReduction);

                        bCTBL.Unit = "VNĐ";

                        resBct.Add(bCTBL);

                        decimal K = (decimal)bCTBL.TotalK;
                        if (groupdataK.Count() > 0)
                        {
                            foreach (var itemk in groupdataK)
                            {
                                decimal totalK = 0;
                                decimal CurrentK = K;
                                var changevalue = new changeValue();
                                foreach (var itemK in bCTBL.chilDfs)
                                {
                                    foreach (var itemChangeK in itemk.groupDataChangeK.OrderBy(o => o.DateChange).ToList())
                                    {
                                        if (itemK.CoefficientId == itemChangeK.CoefficientId)
                                        {
                                            totalK = (decimal)(CurrentK - (decimal)itemK.Value);
                                            changevalue.valueChange = (double)(totalK + (decimal)itemChangeK.valueChange);
                                            changevalue.Type = 5;
                                            changevalue.DateChange = itemChangeK.DateChange;
                                            CurrentK = (decimal)changevalue.valueChange;
                                        }
                                    }
                                }
                                K = CurrentK;
                                resChange.Add(changevalue);
                            }
                        }

                        if (resChange.Count > 0)
                        {
                            foreach (var itemChange in resChange.OrderBy(p => p.DateChange).ToList())
                            {
                                k = k + 1;
                                decimal? PriceRent1m2 = 0;
                                decimal? PriceRent = 0;

                                var changeData = new BCT();
                                changeData.TypeQD = 22;
                                changeData.check = false;
                                changeData.GroupId = k;

                                changeData.AreaName = item.AreaName;

                                changeData.Level = (int)item.Level;

                                changeData.DateCoefficient = item.DisposeTime;

                                changeData.PrivateArea = item.PrivateArea;

                                changeData.TotalK = resBct[resBct.Count - 1].TotalK;

                                changeData.VAT = resBct[resBct.Count - 1].VAT;
                                changeData.Ktdbt = resBct[resBct.Count - 1].Ktdbt;
                                changeData.Ktlcb = resBct[resBct.Count - 1].Ktlcb;
                                changeData.StandardPrice = resBct[resBct.Count - 1].StandardPrice;
                                changeData.Unit = resBct[resBct.Count - 1].Unit;
                                changeData.PolicyReduction = resBct[resBct.Count - 1].PolicyReduction;
                                if (itemChange.Type == 1)
                                {
                                    changeData.VAT = itemChange.valueChange;
                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);

                                        //tháng
                                        if (resBct[resBct.Count - 1].DateStart.HasValue && resBct[resBct.Count - 1].DateEnd.HasValue)
                                        {
                                            resBct[resBct.Count - 1].MonthDiff = (resBct[resBct.Count - 1].DateEnd.Value.Year - resBct[resBct.Count - 1].DateStart.Value.Year) * 12 + (resBct[resBct.Count - 1].DateEnd.Value.Month - resBct[resBct.Count - 1].DateStart.Value.Month);
                                        }
                                        // ngày
                                        if (resBct[resBct.Count - 1].DateEnd.HasValue && resBct[resBct.Count - 1].DateStart.HasValue)
                                        {
                                            // Lấy DateTime bên trong kiểu Nullable DateTime
                                            DateTime start = resBct[resBct.Count - 1].DateStart.Value.AddMonths(resBct[resBct.Count - 1].MonthDiff);
                                            DateTime end = resBct[resBct.Count - 1].DateEnd.Value;
                                            // Lấy TimeSpan giữa hai ngày
                                            TimeSpan span = end.Subtract(start);
                                            // Lấy số ngày chênh lệch
                                            resBct[resBct.Count - 1].DateDiff = (int)span.TotalDays;
                                        }
                                        if (resBct[resBct.Count - 1].DateDiff < 0)
                                        {
                                            resBct[resBct.Count - 1].DateDiff = 30 + resBct[resBct.Count - 1].DateDiff;
                                            resBct[resBct.Count - 1].MonthDiff--;
                                        }
                                    }

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;



                                    //tháng
                                    if (changeData.DateStart.HasValue && changeData.DateEnd.HasValue)
                                    {
                                        changeData.MonthDiff = (changeData.DateEnd.Value.Year - changeData.DateStart.Value.Year) * 12 + (changeData.DateEnd.Value.Month - changeData.DateStart.Value.Month);
                                    }
                                    // ngày
                                    if (changeData.DateEnd.HasValue && changeData.DateStart.HasValue)
                                    {
                                        // Lấy DateTime bên trong kiểu Nullable DateTime
                                        DateTime start = changeData.DateStart.Value.AddMonths(changeData.MonthDiff);
                                        DateTime end = changeData.DateEnd.Value;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan span = end.Subtract(start);
                                        // Lấy số ngày chênh lệch
                                        changeData.DateDiff = (int)span.TotalDays;
                                    }
                                    if (changeData.DateDiff < 0)
                                    {
                                        changeData.DateDiff = 30 + changeData.DateDiff;
                                        changeData.MonthDiff--;
                                    }
                                    if (resBct[resBct.Count - 1].DateStart.Value == changeData.DateStart)
                                    {
                                        resBct.Remove(resBct[resBct.Count - 1]);
                                    }
                                    resBct.Add(changeData);
                                }
                                else if (itemChange.Type == 2)
                                {
                                    changeData.Ktlcb = (decimal)itemChange.valueChange;
                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;

                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);

                                        //tháng
                                        if (resBct[resBct.Count - 1].DateStart.HasValue && resBct[resBct.Count - 1].DateEnd.HasValue)
                                        {
                                            resBct[resBct.Count - 1].MonthDiff = (resBct[resBct.Count - 1].DateEnd.Value.Year - resBct[resBct.Count - 1].DateStart.Value.Year) * 12 + (resBct[resBct.Count - 1].DateEnd.Value.Month - resBct[resBct.Count - 1].DateStart.Value.Month);
                                        }
                                        // ngày
                                        if (resBct[resBct.Count - 1].DateEnd.HasValue && resBct[resBct.Count - 1].DateStart.HasValue)
                                        {
                                            // Lấy DateTime bên trong kiểu Nullable DateTime
                                            DateTime start = resBct[resBct.Count - 1].DateStart.Value.AddMonths(resBct[resBct.Count - 1].MonthDiff);
                                            DateTime end = resBct[resBct.Count - 1].DateEnd.Value;
                                            // Lấy TimeSpan giữa hai ngày
                                            TimeSpan span = end.Subtract(start);
                                            // Lấy số ngày chênh lệch
                                            resBct[resBct.Count - 1].DateDiff = (int)span.TotalDays;
                                        }
                                        if (resBct[resBct.Count - 1].DateDiff < 0)
                                        {
                                            resBct[resBct.Count - 1].DateDiff = 30 + resBct[resBct.Count - 1].DateDiff;
                                            resBct[resBct.Count - 1].MonthDiff--;
                                        }
                                    }


                                    //tháng
                                    if (changeData.DateStart.HasValue && changeData.DateEnd.HasValue)
                                    {
                                        changeData.MonthDiff = (changeData.DateEnd.Value.Year - changeData.DateStart.Value.Year) * 12 + (changeData.DateEnd.Value.Month - changeData.DateStart.Value.Month);
                                    }
                                    // ngày
                                    if (changeData.DateEnd.HasValue && changeData.DateStart.HasValue)
                                    {
                                        // Lấy DateTime bên trong kiểu Nullable DateTime
                                        DateTime start = changeData.DateStart.Value.AddMonths(changeData.MonthDiff);
                                        DateTime end = changeData.DateEnd.Value;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan span = end.Subtract(start);
                                        // Lấy số ngày chênh lệch
                                        changeData.DateDiff = (int)span.TotalDays;
                                    }
                                    if (changeData.DateDiff < 0)
                                    {
                                        changeData.DateDiff = 30 + changeData.DateDiff;
                                        changeData.MonthDiff--;
                                    }
                                    if (resBct[resBct.Count - 1].DateStart.Value == changeData.DateStart)
                                    {
                                        resBct.Remove(resBct[resBct.Count - 1]);
                                    }
                                    resBct.Add(changeData);

                                }
                                else if (itemChange.Type == 3)
                                {
                                    changeData.Ktdbt = (decimal)itemChange.valueChange;
                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;

                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);

                                        //tháng
                                        if (resBct[resBct.Count - 1].DateStart.HasValue && resBct[resBct.Count - 1].DateEnd.HasValue)
                                        {
                                            resBct[resBct.Count - 1].MonthDiff = (resBct[resBct.Count - 1].DateEnd.Value.Year - resBct[resBct.Count - 1].DateStart.Value.Year) * 12 + (resBct[resBct.Count - 1].DateEnd.Value.Month - resBct[resBct.Count - 1].DateStart.Value.Month);
                                        }
                                        // ngày
                                        if (resBct[resBct.Count - 1].DateEnd.HasValue && resBct[resBct.Count - 1].DateStart.HasValue)
                                        {
                                            // Lấy DateTime bên trong kiểu Nullable DateTime
                                            DateTime start = resBct[resBct.Count - 1].DateStart.Value.AddMonths(resBct[resBct.Count - 1].MonthDiff);
                                            DateTime end = resBct[resBct.Count - 1].DateEnd.Value;
                                            // Lấy TimeSpan giữa hai ngày
                                            TimeSpan span = end.Subtract(start);
                                            // Lấy số ngày chênh lệch
                                            resBct[resBct.Count - 1].DateDiff = (int)span.TotalDays;
                                        }
                                        if (resBct[resBct.Count - 1].DateDiff < 0)
                                        {
                                            resBct[resBct.Count - 1].DateDiff = 30 + resBct[resBct.Count - 1].DateDiff;
                                            resBct[resBct.Count - 1].MonthDiff--;
                                        }
                                    }

                                    changeData.chilDfs = new List<chilDf>();
                                    if (map_rentFlieBCTDatas != null)
                                    {
                                        foreach (var itemDf in map_rentFlieBCTDatas.groupData)
                                        {
                                            if (itemDf.childDefaultCoefficients.Count > 0)
                                            {
                                                foreach (var itemChild in itemDf.childDefaultCoefficients)
                                                {
                                                    var itemChilDfs = new chilDf();

                                                    if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                                    {

                                                        string name = "";
                                                        if (item.FloorCode < 6)
                                                        {
                                                            name = changeData.AreaName;
                                                        }
                                                        else
                                                        {
                                                            name = "Tầng 6 trở lên";
                                                        }
                                                        string content = _context.DefaultCoefficients.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Content).FirstOrDefault();
                                                        itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                        if (content == name)
                                                        {
                                                            if (changeData.DateStart >= itemChild.DoApply)
                                                            {
                                                                itemChilDfs.Value = itemChild.Value;
                                                                changeData.chilDfs.Add(itemChilDfs);
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (changeData.DateStart >= itemChild.DoApply)
                                                        {
                                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                            itemChilDfs.Value = itemChild.Value;
                                                            changeData.chilDfs.Add(itemChilDfs);
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                    }


                                    //tháng
                                    if (changeData.DateStart.HasValue && changeData.DateEnd.HasValue)
                                    {
                                        changeData.MonthDiff = (changeData.DateEnd.Value.Year - changeData.DateStart.Value.Year) * 12 + (changeData.DateEnd.Value.Month - changeData.DateStart.Value.Month);
                                    }
                                    // ngày
                                    if (changeData.DateEnd.HasValue && changeData.DateStart.HasValue)
                                    {
                                        // Lấy DateTime bên trong kiểu Nullable DateTime
                                        DateTime start = changeData.DateStart.Value.AddMonths(changeData.MonthDiff);
                                        DateTime end = changeData.DateEnd.Value;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan span = end.Subtract(start);
                                        // Lấy số ngày chênh lệch
                                        changeData.DateDiff = (int)span.TotalDays;
                                    }
                                    if (changeData.DateDiff < 0)
                                    {
                                        changeData.DateDiff = 30 + changeData.DateDiff;
                                        changeData.MonthDiff--;
                                    }
                                    if (resBct[resBct.Count - 1].DateStart.Value == changeData.DateStart)
                                    {
                                        resBct.Remove(resBct[resBct.Count - 1]);
                                    }
                                    resBct.Add(changeData);
                                }
                                else if (itemChange.Type == 4)
                                {
                                    changeData.PolicyReduction = (decimal)itemChange.valueChange;

                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;

                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);


                                        //tháng
                                        if (resBct[resBct.Count - 1].DateStart.HasValue && resBct[resBct.Count - 1].DateEnd.HasValue)
                                        {
                                            resBct[resBct.Count - 1].MonthDiff = (resBct[resBct.Count - 1].DateEnd.Value.Year - resBct[resBct.Count - 1].DateStart.Value.Year) * 12 + (resBct[resBct.Count - 1].DateEnd.Value.Month - resBct[resBct.Count - 1].DateStart.Value.Month);
                                        }
                                        // ngày
                                        if (resBct[resBct.Count - 1].DateEnd.HasValue && resBct[resBct.Count - 1].DateStart.HasValue)
                                        {
                                            // Lấy DateTime bên trong kiểu Nullable DateTime
                                            DateTime start = resBct[resBct.Count - 1].DateStart.Value.AddMonths(resBct[resBct.Count - 1].MonthDiff);
                                            DateTime end = resBct[resBct.Count - 1].DateEnd.Value;
                                            // Lấy TimeSpan giữa hai ngày
                                            TimeSpan span = end.Subtract(start);
                                            // Lấy số ngày chênh lệch
                                            resBct[resBct.Count - 1].DateDiff = (int)span.TotalDays;
                                        }
                                        if (resBct[resBct.Count - 1].DateDiff < 0)
                                        {
                                            resBct[resBct.Count - 1].DateDiff = 30 + resBct[resBct.Count - 1].DateDiff;
                                            resBct[resBct.Count - 1].MonthDiff--;
                                        }
                                    }

                                    //tháng
                                    if (changeData.DateStart.HasValue && changeData.DateEnd.HasValue)
                                    {
                                        changeData.MonthDiff = (changeData.DateEnd.Value.Year - changeData.DateStart.Value.Year) * 12 + (changeData.DateEnd.Value.Month - changeData.DateStart.Value.Month);
                                    }
                                    // ngày
                                    if (changeData.DateEnd.HasValue && changeData.DateStart.HasValue)
                                    {
                                        // Lấy DateTime bên trong kiểu Nullable DateTime
                                        DateTime start = changeData.DateStart.Value.AddMonths(changeData.MonthDiff);
                                        DateTime end = changeData.DateEnd.Value;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan span = end.Subtract(start);
                                        // Lấy số ngày chênh lệch
                                        changeData.DateDiff = (int)span.TotalDays;
                                    }
                                    if (changeData.DateDiff < 0)
                                    {
                                        changeData.DateDiff = 30 + changeData.DateDiff;
                                        changeData.MonthDiff--;
                                    }
                                    if (resBct[resBct.Count - 1].DateStart.Value == changeData.DateStart)
                                    {
                                        resBct.Remove(resBct[resBct.Count - 1]);
                                    }
                                    resBct.Add(changeData);

                                }
                                else if (itemChange.Type == 5)
                                {
                                    changeData.TotalK = (float?)itemChange.valueChange;
                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);


                                        //tháng
                                        if (resBct[resBct.Count - 1].DateStart.HasValue && resBct[resBct.Count - 1].DateEnd.HasValue)
                                        {
                                            resBct[resBct.Count - 1].MonthDiff = (resBct[resBct.Count - 1].DateEnd.Value.Year - resBct[resBct.Count - 1].DateStart.Value.Year) * 12 + (resBct[resBct.Count - 1].DateEnd.Value.Month - resBct[resBct.Count - 1].DateStart.Value.Month);
                                        }
                                        // ngày
                                        if (resBct[resBct.Count - 1].DateEnd.HasValue && resBct[resBct.Count - 1].DateStart.HasValue)
                                        {
                                            // Lấy DateTime bên trong kiểu Nullable DateTime
                                            DateTime start = resBct[resBct.Count - 1].DateStart.Value.AddMonths(resBct[resBct.Count - 1].MonthDiff);
                                            DateTime end = resBct[resBct.Count - 1].DateEnd.Value;
                                            // Lấy TimeSpan giữa hai ngày
                                            TimeSpan span = end.Subtract(start);
                                            // Lấy số ngày chênh lệch
                                            resBct[resBct.Count - 1].DateDiff = (int)span.TotalDays;
                                        }
                                        if (resBct[resBct.Count - 1].DateDiff < 0)
                                        {
                                            resBct[resBct.Count - 1].DateDiff = 30 + resBct[resBct.Count - 1].DateDiff;
                                            resBct[resBct.Count - 1].MonthDiff--;
                                        }
                                    }

                                    changeData.chilDfs = new List<chilDf>();
                                    if (map_rentFlieBCTDatas != null)
                                    {
                                        foreach (var itemDf in map_rentFlieBCTDatas.groupData)
                                        {
                                            if (itemDf.childDefaultCoefficients.Count > 0)
                                            {
                                                foreach (var itemChild in itemDf.childDefaultCoefficients)
                                                {

                                                    if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                                    {
                                                        string name = "";
                                                        if (item.FloorCode < 6)
                                                        {
                                                            name = changeData.AreaName;
                                                        }
                                                        else
                                                        {
                                                            name = "Tầng 6 trở lên";
                                                        }
                                                        string content = _context.DefaultCoefficients.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Content).FirstOrDefault();
                                                        if (content == name)
                                                        {
                                                            if (changeData.DateStart >= itemChild.DoApply)
                                                            {
                                                                var itemChilDfs = new chilDf();
                                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;

                                                                itemChilDfs.Value = itemChild.Value;
                                                                changeData.chilDfs.Add(itemChilDfs);
                                                                break;
                                                            }

                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (changeData.DateStart >= itemChild.DoApply)
                                                        {
                                                            var itemChilDfs = new chilDf();

                                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                            itemChilDfs.Value = itemChild.Value;
                                                            changeData.chilDfs.Add(itemChilDfs);
                                                            break;
                                                        }

                                                    }
                                                }
                                            }
                                        }


                                    }

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;



                                    //tháng
                                    if (changeData.DateStart.HasValue && changeData.DateEnd.HasValue)
                                    {
                                        changeData.MonthDiff = (changeData.DateEnd.Value.Year - changeData.DateStart.Value.Year) * 12 + (changeData.DateEnd.Value.Month - changeData.DateStart.Value.Month);
                                    }
                                    // ngày
                                    if (changeData.DateEnd.HasValue && changeData.DateStart.HasValue)
                                    {
                                        // Lấy DateTime bên trong kiểu Nullable DateTime
                                        DateTime start = changeData.DateStart.Value.AddMonths(changeData.MonthDiff);
                                        DateTime end = changeData.DateEnd.Value;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan span = end.Subtract(start);
                                        // Lấy số ngày chênh lệch
                                        changeData.DateDiff = (int)span.TotalDays;
                                    }
                                    if (changeData.DateDiff < 0)
                                    {
                                        changeData.DateDiff = 30 + changeData.DateDiff;
                                        changeData.MonthDiff--;
                                    }
                                    if (resBct[resBct.Count - 1].DateStart.Value == changeData.DateStart)
                                    {
                                        resBct.Remove(resBct[resBct.Count - 1]);
                                    }
                                    resBct.Add(changeData);
                                }

                            }
                        }

                    }
                }

                var groupData = resBct.GroupBy(f => f.GroupId != null ? f.GroupId : new object(), key => key).Select(f => new groupData
                {
                    GroupId = (int)f.Key,
                    VAT = f.ToList().First().VAT,
                    DateStart = (DateTime)f.ToList().First().DateStart,
                    DateEnd = f.ToList().Last().DateEnd,
                    totalPrice = (decimal)f.ToList().Sum(p => p.PriceRent),
                    bCTs = f.ToList()
                }).ToList();
                return groupData;
            }
            catch (Exception ex)
            {
                log.Error("getDataBCT22:" + ex);
                return null;
            }
        }

        private void genListWorkSheetTblVer2(Document document, List<groupData> data, Guid id, double discount)
        {
            try
            {
                DocumentRange[] ranges = document.FindAll("<listWorkSheet>", DevExpress.XtraRichEdit.API.Native.SearchOptions.None);
                DocumentRange lastRange = ranges[0];

                string str_result = "";
                string str_result_money = "";
                decimal total = 0;
                var dateLast = "";
                decimal lastTotal = 0;
                int index = 1;
                int VAT = 0;


                data.ForEach(item =>
                {
                    lastRange = document.InsertText(lastRange.End, $"\t {index}. Tiền thuê nhà từ ngày {item.DateStart.ToString("dd/MM/yyyy")} đến ngày {item.DateEnd?.ToString("dd/MM/yyyy")}");
                    CharacterProperties cp = document.BeginUpdateCharacters(lastRange);
                    cp.FontSize = 14;
                    document.EndUpdateCharacters(cp);

                    int row_number = item.bCTs.Count() + 4;
                    var data_BCT = item.bCTs;

                    dateLast = item.bCTs.Last().DateStart?.ToString("dd/MM/yyyy");

                    VAT = (int)item.bCTs.Last().VAT;
                    //discount = (double)item.bCTs.First().PolicyReduction;

                    Table table = document.Tables.Create(lastRange.End, row_number, 7, AutoFitBehaviorType.AutoFitToWindow);
                    table.BeginUpdate();
                    table.MergeCells(table[row_number - 3, 0], table[row_number - 3, 4]);
                    table.MergeCells(table[row_number - 2, 0], table[row_number - 2, 5]);
                    table.MergeCells(table[row_number - 1, 0], table[row_number - 1, 5]);
                    table.EndUpdate();
                    table.ForEachCell((cell, i, j) =>
                    {
                        if (i == 0)
                        {
                            if (j == 0)
                            {
                                document.InsertSingleLineText(cell.Range.Start, $"Quyết định giá");
                            }
                            else if (j == 1)
                            {
                                document.InsertSingleLineText(cell.Range.Start, $"Tầng");
                            }
                            else if (j == 2)
                            {
                                document.InsertSingleLineText(cell.Range.Start, $"Cấp");
                            }
                            else if (j == 3)
                            {
                                document.InsertSingleLineText(cell.Range.Start, $"Diện tích(m2)");
                            }
                            else if (j == 4)
                            {
                                if (data_BCT[i].TypeQD == 9 || data_BCT[i].TypeQD == 1753)
                                {
                                    document.InsertSingleLineText(cell.Range.Start, $"Giá chuẩn");
                                }
                                else if (data_BCT[i].TypeQD == 22)
                                {
                                    document.InsertSingleLineText(cell.Range.Start, $"Giá chuẩn x KTLCB x KTHBT");

                                }
                            }
                            else if (j == 5)
                            {
                                var chilDfs = data_BCT[0].chilDfs;
                                string str = "1";
                                if (data_BCT[0].TypeQD == 22)
                                {
                                    foreach (var chilDf in chilDfs)
                                    {
                                        var name = _context.Conversions.Where(l => l.CoefficientName == chilDf.CoefficientId && l.Status != AppEnums.EntityStatus.DELETED && l.TypeQD == AppEnums.TypeQD.QD_22).Select(p => p.Code).FirstOrDefault();
                                        str += name == "" ? $"" : $" + {name}";
                                    }
                                }
                                if (data_BCT[0].TypeQD == 9)
                                {
                                    foreach (var chilDf in chilDfs)
                                    {
                                        var name = _context.Conversions.Where(l => l.CoefficientName == chilDf.CoefficientId && l.Status != AppEnums.EntityStatus.DELETED && l.TypeQD == AppEnums.TypeQD.QD_09).Select(p => p.Code).FirstOrDefault();
                                        str += name == "" ? $"" : $" + {name}";
                                    }
                                }
                                if (data_BCT[0].TypeQD == 1753)
                                {
                                    foreach (var chilDf in chilDfs)
                                    {
                                        var name = _context.Conversions.Where(l => l.CoefficientName == chilDf.CoefficientId && l.Status != AppEnums.EntityStatus.DELETED && l.TypeQD == AppEnums.TypeQD.QD_1753).Select(p => p.Code).FirstOrDefault();
                                        str += name == "" ? $"" : $" + {name}";
                                    }
                                }
                                document.InsertSingleLineText(cell.Range.Start, $"{str}");
                            }
                            else if (j == 6)
                            {
                                document.InsertSingleLineText(cell.Range.Start, $"Tiền thuê/tháng");
                            }
                        }
                        else if (i < row_number - 3)
                        {
                            if (j == 0)
                            {
                                document.InsertSingleLineText(cell.Range.Start, $"{data_BCT[i - 1].TypeQD}");
                            }
                            else if (j == 1)
                            {
                                document.InsertSingleLineText(cell.Range.Start, $"{data_BCT[i - 1].AreaName}");
                            }
                            else if (j == 2)
                            {
                                document.InsertSingleLineText(cell.Range.Start, $"{data_BCT[i - 1].Level}");
                            }
                            else if (j == 3)
                            {
                                document.InsertSingleLineText(cell.Range.Start, $"{data_BCT[i - 1].PrivateArea}");


                            }
                            else if (j == 4)
                            {
                                if (data_BCT[i - 1].TypeQD == 9)
                                {
                                    document.InsertSingleLineText(cell.Range.Start, $"{data_BCT[i - 1].StandardPrice}");
                                }
                                else if (data_BCT[i - 1].TypeQD == 1753)
                                {
                                    document.InsertSingleLineText(cell.Range.Start, $"{data_BCT[i - 1].StandardPrice}");
                                }
                                else if (data_BCT[i - 1].TypeQD == 22)
                                {
                                    document.InsertSingleLineText(cell.Range.Start, $"{data_BCT[i - 1].StandardPrice} x {data_BCT[i - 1].Ktlcb} x {data_BCT[i - 1].Ktdbt}");
                                }
                            }
                            else if (j == 5)
                            {
                                var chilDfs = data_BCT[i - 1].chilDfs;
                                string str = "1";
                                foreach (var chilDf in chilDfs)
                                {
                                    str += "+" + chilDf.Value;
                                }

                                document.InsertSingleLineText(cell.Range.Start, $"{str}");
                            }
                            else if (j == 6)
                            {
                                string PriceRent = $"{UtilsService.FormatMoney(data_BCT[i - 1].PriceRent)} ";
                                document.InsertSingleLineText(cell.Range.Start, $"{PriceRent}");
                            }
                        }
                        else if (i == row_number - 3)
                        {
                            if (j == 0)
                            {
                                document.InsertSingleLineText(cell.Range.Start, $"Thời gian thuê nhà");
                            }
                            else if (j == 1)
                            {
                                document.InsertSingleLineText(cell.Range.Start, $"{data_BCT[0].MonthDiff} tháng {data_BCT[0].DateDiff} ngày");
                            }
                            else if (j == 2)
                            {
                                int month = data_BCT[0].DateEnd.Value.Month;
                                int year = data_BCT[0].DateEnd.Value.Year;
                                int numberOfDaysInMonth = DateTime.DaysInMonth(year, month);

                                var priceno = data_BCT.Sum(p => p.PriceRent);
                                var priceMonth = priceno * data_BCT[0].MonthDiff;
                                var priceDay = (priceno / numberOfDaysInMonth) * data_BCT[0].DateDiff;

                                var Price = Math.Round((decimal)(priceMonth + priceDay));

                                string PriceString = $"{UtilsService.FormatMoney(Price)} ";

                                document.InsertSingleLineText(cell.Range.Start, $"{PriceString}");
                            }
                        }
                        else if (i == row_number - 2)
                        {
                            if (j == 0)
                            {
                                document.InsertSingleLineText(cell.Range.Start, $"{data_BCT[0].VAT} % VAT");
                            }
                            else if (j == 1)
                            {
                                int month = data_BCT[0].DateEnd.Value.Month;
                                int year = data_BCT[0].DateEnd.Value.Year;
                                int numberOfDaysInMonth = DateTime.DaysInMonth(year, month);

                                var priceno = data_BCT.Sum(p => p.PriceRent);
                                var priceMonth = priceno * data_BCT[0].MonthDiff;
                                var priceDay = (priceno / numberOfDaysInMonth) * data_BCT[0].DateDiff;

                                var Price = Math.Round((decimal)(priceMonth + priceDay));
                                var VatPrice = Math.Round(Price * (decimal)(data_BCT[0].VAT / 100));
                                string VatPriceString = $"{UtilsService.FormatMoney(VatPrice)} ";

                                document.InsertSingleLineText(cell.Range.Start, $" {VatPriceString}");
                            }
                        }
                        else if (i == row_number - 1)
                        {
                            if (j == 0)
                            {
                                document.InsertSingleLineText(cell.Range.Start, $"Tổng");
                            }
                            else if (j == 1)
                            {
                                int month = data_BCT[0].DateEnd.Value.Month;
                                int year = data_BCT[0].DateEnd.Value.Year;

                                int numberOfDaysInMonth = DateTime.DaysInMonth(year, month);

                                var priceno = data_BCT.Sum(p => p.PriceRent);
                                var priceMonth = priceno * data_BCT[0].MonthDiff;
                                var priceDay = (priceno / numberOfDaysInMonth) * data_BCT[0].DateDiff;

                                var Price = Math.Round((decimal)(priceMonth + priceDay));
                                var VatPrice = Math.Round(Price * (decimal)(data_BCT[0].VAT / 100));
                                var TotalPrice = Math.Round(Price + VatPrice);
                                string TotalPriceString = $"{UtilsService.FormatMoney(TotalPrice)} ";

                                document.InsertSingleLineText(cell.Range.Start, $"{TotalPriceString}");
                                total += TotalPrice;
                                lastTotal = Math.Round((decimal)(priceno * (decimal)(1 + (data_BCT[0].VAT / 100))));
                                str_result_money += str_result_money == "" ? $"{TotalPriceString}" : $" + {TotalPriceString}";
                            }
                        }
                    });

                    str_result = str_result == "" ? $"({index})" : $"{str_result} + ({index})";

                    index++;
                });
                document.ReplaceAll("<listWorkSheet>", "", SearchOptions.None);
                string totalString = $"{UtilsService.FormatMoney(total)} ";

                string str = $"Tổng công tiền thuê nhà phải nộp: {str_result} = {str_result_money} = {totalString}";
                document.ReplaceAll("<totalCalculationFunction>", str, SearchOptions.None);

                string lT = lastTotal != null ? lastTotal.ToString() : "0";
                string lastTotalString = $"{UtilsService.FormatMoney(lastTotal)} ";

                document.ReplaceAll("<lastTotal>", lastTotalString, SearchOptions.None);

                document.ReplaceAll("<lastDateStart>", dateLast, SearchOptions.None);

                document.ReplaceAll("<totalInWords>", UtilsService.ConvertMoneyToString(lT.ToString()).ToLower(), SearchOptions.None);

                string vat = VAT != null ? VAT.ToString() : "0";
                document.ReplaceAll("<vat>", vat, SearchOptions.None);

                if (discount == null) discount = 0;

                decimal discountPrice = lastTotal * (decimal)discount;

                string buav2 = discountPrice != null ? discountPrice.ToString() : "0";
                string discountString = $"{UtilsService.FormatMoney((decimal?)discountPrice)} ";

                document.ReplaceAll("<discountTotal>", discountString, SearchOptions.None);
                document.ReplaceAll("<discountTotalText>", UtilsService.ConvertMoneyToString(buav2.ToString()).ToLower(), SearchOptions.None);

                decimal priceRentTotal = lastTotal - (decimal)discountPrice;
                string buav3 = priceRentTotal != null ? priceRentTotal.ToString() : "0";
                string priceRentTotalString = $"{UtilsService.FormatMoney((decimal?)priceRentTotal)} ";

                document.ReplaceAll("<priceRentTotal>", priceRentTotalString, SearchOptions.None);
                document.ReplaceAll("<priceRentTotalText>", UtilsService.ConvertMoneyToString(buav3.ToString()).ToLower(), SearchOptions.None);

                DateTime date = DateTime.Now;
                var lstDebtsHD = _context.DebtsTables.Where(l => l.RentFileId == id && l.Type == 3 && l.Status != EntityStatus.DELETED).ToList();
                if (lstDebtsHD != null)
                {
                    var PriceLstDebts = Math.Ceiling(lstDebtsHD.Where(l => l.DateStart.Month == date.Month && l.DateStart.Year == date.Year).Select(p => p.Price).FirstOrDefault());
                    var price = Math.Ceiling(PriceLstDebts + (decimal)discount);
                    string HD = price != null ? price.ToString() : "0";
                    string priceString = $"{UtilsService.FormatMoney(price)} ";

                    document.ReplaceAll("<giathuenha>", priceString, SearchOptions.None);
                    document.ReplaceAll("<giathuenhabangchu>", UtilsService.ConvertMoneyToString(HD.ToString()).ToLower(), SearchOptions.None);

                    var PriceRent = Math.Ceiling(price - (decimal)discount);
                    string priceRent = PriceRent != null ? PriceRent.ToString() : "0";
                    string PriceRentString = $"{UtilsService.FormatMoney(PriceRent)} ";

                    document.ReplaceAll("<priceRentTotal>", PriceRentString, SearchOptions.None);
                    document.ReplaceAll("<priceRentTotalText>", UtilsService.ConvertMoneyToString(priceRent.ToString()).ToLower(), SearchOptions.None);
                }
                else
                {
                    document.ReplaceAll("<sotienduocmiengiam>", "", SearchOptions.None);
                    document.ReplaceAll("<sotienduocmiengianbangchu>", "", SearchOptions.None);

                    document.ReplaceAll("<sotienthue>", "", SearchOptions.None);
                    document.ReplaceAll("<sotienthuebangchu>", "", SearchOptions.None);
                }

            }
            catch (Exception ex)
            {
                log.Error("genListWorkSheetTblVer2:" + ex.Message);
            }
        }
    }
}


