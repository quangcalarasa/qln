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
using NPOI.SS.Formula.Functions;
using DevExpress.ClipboardSource.SpreadsheetML;
using Microsoft.Net.Http.Headers;
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using static IOITQln.Common.Enums.AppEnums;
using static IOITQln.Entities.Md167House;
using IOITQln.Migrations;
using static DevExpress.XtraEditors.Filtering.DataItemsExtension;
using static IOITQln.Common.Constants.ApiConstants.MessageResource;
using DevExpress.CodeParser;
using IOITQln.QuickPriceNOC.Interface;
using static IOITQln.QuickPriceNOC.Service.CheckStatusMd167House;
using static IOITQln.Entities.Lane;

namespace IOITQln.Controllers.ApiInv
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class HouseController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("Md167House", "Md167House");
        private static string functionCode = "HOUSE";
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        private ICheckStatusMd167House _checkStatusMd167House;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;

        public HouseController(ApiDbContext context, IMapper mapper, IWebHostEnvironment hostingEnvironment, IConfiguration configuration, ICheckStatusMd167House checkStatusMd167House)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
            _checkStatusMd167House = checkStatusMd167House;
        }

        [HttpGet("GetByPage")]
        public IActionResult GetByPage([FromQuery] FilteredMd167HousePagination paging)
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
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }

            try
            {
                if (paging != null)
                {
                    IQueryable<Md167House> data = Enumerable.Empty<Md167House>().AsQueryable();
                    def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                    IQueryable<Lane> lanes = _context.Lanies.Where(x => x.Status != EntityStatus.DELETED);
                    IQueryable<Md167HouseType> md167HouseTypes = _context.Md167HouseTypes.Where(x => x.Status != EntityStatus.DELETED);
                    IQueryable<Md167House> md167Houses = _context.Md167Houses.Where(x => x.Status != EntityStatus.DELETED);
                    IQueryable<Province> provincies = _context.Provincies.Where(x => x.Status != EntityStatus.DELETED);
                    IQueryable<District> districts = _context.Districts.Where(x => x.Status != EntityStatus.DELETED);
                    IQueryable<Ward> wards = _context.Wards.Where(x => x.Status != EntityStatus.DELETED);
                    IQueryable<Md167HouseInfo> md167HouseInfos = _context.Md167HouseInfos.Where(x => x.Status != EntityStatus.DELETED);
                    IQueryable<Md167HousePropose> md167HouseProposes = _context.Md167HouseProposes.Where(x => x.Status != EntityStatus.DELETED);
                    if (paging.TypeHouse == Md167House.Type_House.Kios) data = _context.Md167Houses.Where(c => c.Status != AppEnums.EntityStatus.DELETED && c.TypeHouse == Md167House.Type_House.Kios);
                    else data = _context.Md167Houses.Where(c => c.Status != AppEnums.EntityStatus.DELETED && c.TypeHouse != Md167House.Type_House.Kios);
                    IQueryable<Md167HouseResData> res = data == null ? Enumerable.Empty<Md167HouseResData>().AsQueryable() : data.Select(p => new Md167HouseResData
                    {
                        Id = p.Id,
                        Code = p.Code,
                        HouseNumber = p.HouseNumber,
                        ProvinceId = p.ProvinceId,
                        DistrictId = p.DistrictId,
                        WardId = p.WardId,
                        LaneId = p.LaneId,
                        MapNumber = p.MapNumber,
                        ParcelNumber = p.ParcelNumber,
                        LandTaxRate = p.LandTaxRate,
                        PlanningInfor = p.PlanningInfor,
                        LandId = p.LandId,
                        StatusOfUseName = _context.Md167StateOfUses.Where(x => x.Id == p.StatusOfUse).Select(x => x.Name).FirstOrDefault(),
                        Md167TransferUnitId = p.Md167TransferUnitId,
                        ReceptionDate = p.ReceptionDate,
                        decree = p.decree,
                        Location = p.Location,
                        LocationCoefficient = p.LocationCoefficient,
                        UnitPrice = p.UnitPrice,
                        SHNNCode = p.SHNNCode,
                        SHNNDate = p.SHNNDate,
                        ContractCode = p.ContractCode,
                        ContractDate = p.ContractDate,
                        LeaseCode = p.LeaseCode,
                        LeaseDate = p.LeaseDate,
                        LeaseCertCode = p.LeaseCertCode,
                        LeaseCertDate = p.LeaseCertDate,
                        Md167HouseId = p.Md167HouseId,
                        PurposeUsing = p.PurposeUsing,
                        DocumentCode = p.DocumentCode,
                        PlanContent = p.PlanContent,
                        OriginPrice = p.OriginPrice,
                        ValueLand = p.ValueLand,
                        TypeHouse = p.TypeHouse,
                        StatusOfUse = p.StatusOfUse,
                        Note = p.Note,
                        AreaValueId = p.AreaValueId,
                        HouAreaLand = p.InfoValue.HouAreaLand,
                        TaxNN = p.InfoValue.TaxNN,
                        UseFloorPb = p.InfoValue.UseFloorPb,
                        UseFloorPr = p.InfoValue.UseFloorPr,
                        UseLandPb = p.InfoValue.UseLandPb,
                        UseLandPr = p.InfoValue.UseLandPr,
                        AreBuildPb = p.InfoValue.AreBuildPb,
                        AreBuildPr = p.InfoValue.AreBuildPr,
                        DocumentDate = p.DocumentDate,
                        AreaLandInSafe = p.InfoValue.AreaLandInSafe,
                        AreaLandInBankSafe = p.InfoValue.AreaLandInBankSafe,
                        AreaHouseInSafe = p.InfoValue.AreaHouseInSafe,
                        AreaHouseInBankSafe = p.InfoValue.AreaHouseInBankSafe,
                        ApaFloorCount = p.InfoValue.ApaFloorCount,
                        ApaTax = p.InfoValue.ApaTax,
                        IsPayTax = p.IsPayTax,
                        ApaIsBasement = p.InfoValue.ApaIsBasement,
                        ApaValue = p.InfoValue.ApaValue,
                        KiosStatus = p.InfoValue.KiosStatus,
                        LandPrice = p.LandPrice,
                        UnitPriceValue = p.UnitPriceValue,
                        TextureScale = p.TextureScale,
                        AreaFloorBuild = p.InfoValue.AreaFloorBuild,
                        AreaTunnel = p.InfoValue.AreaTunnel,
                        HouseTypeId = p.HouseTypeId,
                        LandPriceItemId = p.LandPriceItemId,
                        //QFix:Fix bug tính toán diện tích
                        // TotalAreaValue = (p.InfoValue.UseLandPb ?? 0 + p.InfoValue.UseLandPr ?? 0 + p.InfoValue.AreBuildPb ?? 0 + p.InfoValue.AreBuildPr ?? 0 + p.InfoValue.AreaLandInSafe ?? 0 + p.InfoValue.AreaLandInBankSafe ?? 0).ToString() + " m²",
                        // TotalFloorValue = (p.InfoValue.UseFloorPb ?? 0 + p.InfoValue.UseFloorPr ?? 0).ToString() + " m²",
                        TotalAreaValue = ((p.InfoValue.UseLandPb ?? 0) + (p.InfoValue.UseLandPr ?? 0)).ToString() + " m²",
                        TotalFloorValue = ((p.InfoValue.UseFloorPb ?? 0) + (p.InfoValue.UseFloorPr ?? 0)).ToString() + " m²",
                        HouseTypeName = md167HouseTypes.Where(x => x.Id == p.HouseTypeId).Select(x => x.Code).FirstOrDefault() ?? "",
                        ApaKiosCode = md167Houses.Where(x => x.Id == p.Md167HouseId).Select(x => x.Code).FirstOrDefault() ?? "",
                        ProvinceName = provincies.Where(x => x.Id == p.ProvinceId).Select(x => x.Name).FirstOrDefault() ?? "",
                        DistrictName = districts.Where(x => x.Id == p.DistrictId).Select(x => x.Name).FirstOrDefault() ?? "",
                        WardName = wards.Where(x => x.Id == p.WardId).Select(x => x.Name).FirstOrDefault() ?? "",
                        md167HouseInfos = md167HouseInfos.Where(x => x.Md167HouseId == p.Id).ToList(),
                        md167HouseProposes = md167HouseProposes.Where(x => x.Md167HouseId == p.Id).ToList(),
                        md167Kios = md167Houses.Where(x => x.TypeHouse == Md167House.Type_House.Kios && x.Md167HouseId == p.Id).Select(
                            h => new Md167KiosResData
                            {
                                Id = h.Id,
                                Code = h.Code,
                                IsPayTax = h.IsPayTax,
                                KiosStatus = h.InfoValue.KiosStatus,
                                HouseNumber = h.HouseNumber,
                                TaxNN = h.InfoValue.TaxNN,
                                UseFloorPb = h.InfoValue.UseFloorPb,
                                Note = h.Note,
                                UseFloorPr = h.InfoValue.UseFloorPr,
                            }).ToList(),
                    }); ;


                    if (paging.query != null)
                    {
                        paging.query = HttpUtility.UrlDecode(paging.query);
                    }
                    res = res.Where(paging.query);
                    def.metadata = res.Count();

                    if (paging.page_size > 0)
                    {
                        if (paging.order_by != null)
                        {
                            res = res.OrderBy(paging.order_by).Skip((paging.page - 1) * paging.page_size).Take(paging.page_size);
                        }
                        else
                        {
                            res = res.OrderBy("Id desc").Skip((paging.page - 1) * paging.page_size).Take(paging.page_size);
                        }
                    }
                    else
                    {
                        if (paging.order_by != null)
                        {
                            res = res.OrderBy(paging.order_by);
                        }
                        else
                        {
                            res = res.OrderBy("Id desc");
                        }
                    }

                    if (paging.select != null && paging.select != "")
                    {
                        paging.select = "new(" + paging.select + ")";
                        paging.select = HttpUtility.UrlDecode(paging.select);
                        def.data = res.Select(paging.select);
                    }
                    else
                    {
                        var lstLand = _context.Lands.Where(x => x.Status != EntityStatus.DELETED).ToList();
                        var lstLandPrice = _context.LandPricies.Where(x => x.Status != EntityStatus.DELETED && x.LandPriceType == landPriceType.MD167).ToList();
                        var lstLandPriceItem = _context.LandPriceItems.Where(x => x.Status != EntityStatus.DELETED).ToList();
                        List<Md167AreaValueApply> lstMd167AreaValueApply = new List<Md167AreaValueApply>();
                        var md167AreaValues = _context.Md167AreaValues.Where(x => x.Status != EntityStatus.DELETED).ToList();
                        var md167AreaValueApplies = _context.Md167AreaValueApplies.Where(x => x.Status != EntityStatus.DELETED).ToList();
                        foreach (var item in md167AreaValueApplies)
                        {
                            var itemExist = md167AreaValues.Where(x => x.Id == item.AreaValueId).FirstOrDefault();
                            if (itemExist != null)
                                lstMd167AreaValueApply.Add(item);
                        }
                        var lstKios = md167Houses.Where(x => x.TypeHouse == Type_House.Kios).ToList();
                        var response = res.ToList();
                        foreach (var item in response)
                        {
                            var laneName = lanes.Where(x => x.Id == item.LaneId).Select(x =>new NameAndOldName
                                { 
                                    Name=x.Name,
                                    OldName = x.InfoValue
                                }).FirstOrDefault();
                            if (laneName == null)
                            {
                                item.LaneName = "";
                            }
                            else
                            {
                                if (laneName.OldName.Count()>0)
                                {
                                    item.LaneName = laneName.Name+ "( Tên cũ: " + laneName.OldName[0].Name+" )";
                                }
                                else
                                {
                                    item.LaneName = laneName.Name;
                                }

                            }
                            var districtName = districts.Where(x => x.Id == item.DistrictId).Select(x => new NameAndOldName
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
                            var wardName = wards.Where(x => x.Id == item.WardId).Select(x => new NameAndOldName
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
                            item.UnitPriceValueTotal = GetPrice(item.DistrictId, lstLandPrice, lstLandPriceItem, item.LaneName);
                            item.AreaValue = GetAreaValue(item.DistrictId, lstMd167AreaValueApply, md167AreaValues);
                            if (item.TypeHouse == Type_House.Apartment)
                            {
                                var totalKios = lstKios.Where(x => x.Md167HouseId == item.Id).ToList();
                                var usedKios = lstKios.Where(x => x.Md167HouseId == item.Id && x.InfoValue.KiosStatus == Kios_Status.DANG_CHO_THUE).ToList();
                                item.Code += "(" + usedKios.Count() + "/" + totalKios.Count() + ")";
                                var totalTax = totalKios.Select(x => x.InfoValue.TaxNN).Sum();
                                if (totalTax == 0) 
                                    item.TotalTaxNN = "0";
                                else
                                    item.TotalTaxNN = totalTax + "/" + totalKios.Count();

                            }
                            if (item.TypeHouse == Type_House.House)
                                item.TotalTaxNN = item.TaxNN.ToString();
                            var md167Contracts = _context.Md167Contracts.Where(x => x.Status != EntityStatus.DELETED).ToList();
                            if (item.TypeHouse == Type_House.Kios && item.KiosStatus == Kios_Status.DANG_CHO_THUE)
                            {
                                if (_checkStatusMd167House.CheckStatus(item.Id, md167Houses.ToList(), md167Contracts) == StateOfUseType.CHUA_CHO_THUE)
                                {
                                    item.KiosStatus = Kios_Status.CHUA_CHO_THUE;
                                }
                            }
                            else if (item.TypeHouse == Type_House.House && item.StatusOfUse == 1)
                            {
                                if (_checkStatusMd167House.CheckStatus(item.Id, md167Houses.ToList(), md167Contracts) == StateOfUseType.CHUA_CHO_THUE)
                                {
                                    item.StatusOfUse = 3;
                                    item.StatusOfUseName = "Chưa cho thuê";
                                }
                            }
                            else if (item.TypeHouse == Type_House.Apartment)
                            {
                                var check = _checkStatusMd167House.CheckStatus(item.Id, md167Houses.ToList(), md167Contracts);
                                if (check == StateOfUseType.DA_CHO_THUE)
                                {
                                    item.StatusOfUse = 1;
                                    item.StatusOfUseName = "Đã cho thuê";

                                }
                                else if (check == StateOfUseType.CHO_THUE_1_PHAN)
                                {
                                    item.StatusOfUse = 2;
                                    item.StatusOfUseName = "Cho thuê 1 phần";

                                }
                                else
                                {
                                    item.StatusOfUse = 3;
                                    item.StatusOfUseName = "Chưa cho thuê";

                                }
                            }
                        }

                        def.data = response;
                    }

                    return Ok(def);
                }
                else
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }
            }
            catch (Exception ex)
            {
                log.Error("GetByPage Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        public static double GetPrice(int districtId, List<LandPrice> lstLandprices, List<LandPriceItem> landPriceItems, string LaneName)
        {
            var landPriceId = lstLandprices.Where(x => x.District == districtId).Select(x => x.Id).ToList();
            List<LandPriceItem> lstShort = new List<LandPriceItem>();
            foreach (var i in landPriceId)
            {
                var item = landPriceItems.Where(x => x.LandPriceId == i).ToList();
                lstShort.AddRange(item);
            }

            double? value = lstShort.Where(x => (UtilsService.NonUnicode(x.LaneEndName ?? "") == UtilsService.NonUnicode(LaneName ?? "") || UtilsService.NonUnicode(x.LaneStartName ?? "") == UtilsService.NonUnicode(LaneName ?? "") || UtilsService.NonUnicode(x.LaneName ?? "") == UtilsService.NonUnicode(LaneName ?? ""))).Select(x => x.Value).FirstOrDefault();
            if (value == null)
            {
                value = landPriceItems.Where(x => x.LandPriceId == landPriceId[0]).Select(x => x.Value).FirstOrDefault();
            }
            return value ?? 0;
        }
        public static decimal GetAreaValue(int districtId, List<Md167AreaValueApply> md167AreaValueApplies, List<Md167AreaValue> md167AreaValues)
        {
            var areaValueId = md167AreaValueApplies.Where(x => x.DistrictId == districtId).Select(x => x.AreaValueId).FirstOrDefault();
            decimal? value = md167AreaValues.Where(x => x.Id == areaValueId).Select(x => x.Value).FirstOrDefault();
            return value ?? 0;
        }
        [HttpGet("GetInfoApartment")]
        public async Task<IActionResult> GetInfoApartment(int id)
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
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }
            if (_context.Md167Houses.Where(x => x.Id == id).Select(x => x.TypeHouse).FirstOrDefault() != Md167House.Type_House.Apartment)
            {
                def.meta = new Meta(200, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                def.data = new KiosInfoRes
                {
                    KiosCount = 0,
                    KiosEmpty = 0,
                    KiosUsed = 0
                };
                return Ok(def);
            }
            List<Md167House> lstKios = _context.Md167Houses.Where(x => x.Status != AppEnums.EntityStatus.DELETED && x.Md167HouseId == id).ToList();
            KiosInfoRes kiosInfoRes = new KiosInfoRes();
            kiosInfoRes.KiosCount = lstKios.Count();
            kiosInfoRes.KiosUsed = lstKios.Where(x => x.InfoValue.KiosStatus == Md167House.Kios_Status.DANG_CHO_THUE).ToList().Count;
            kiosInfoRes.KiosEmpty = kiosInfoRes.KiosCount - kiosInfoRes.KiosUsed;
            def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
            def.data = kiosInfoRes;
            return Ok(def);

        }
        // GET: api/CustomerGroup/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
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
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }

            try
            {
                Md167House data = await _context.Md167Houses.FindAsync(id);

                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                    return Ok(def);
                }

                var HouAreaLand = data.InfoValue.HouAreaLand;

                var res = new Md167HouseResData
                {
                    Id = data.Id,
                    Code = data.Code,
                    HouseNumber = data.HouseNumber,
                    ProvinceId = data.ProvinceId,
                    DistrictId = data.DistrictId,
                    WardId = data.WardId,
                    LaneId = data.LaneId,
                    MapNumber = data.MapNumber,
                    ParcelNumber = data.ParcelNumber,
                    LandTaxRate = data.LandTaxRate,
                    PlanningInfor = data.PlanningInfor,
                    LandId = data.LandId,
                    Md167TransferUnitId = data.Md167TransferUnitId,
                    ReceptionDate = data.ReceptionDate,
                    Location = data.Location,
                    decree = data.decree,
                    LocationCoefficient = data.LocationCoefficient,
                    UnitPrice = data.UnitPrice,
                    SHNNCode = data.SHNNCode,
                    SHNNDate = data.SHNNDate,
                    DocumentDate = data.DocumentDate,
                    ContractCode = data.ContractCode,
                    ContractDate = data.ContractDate,
                    LeaseCode = data.LeaseCode,
                    LeaseDate = data.LeaseDate,
                    LeaseCertCode = data.LeaseCertCode,
                    LeaseCertDate = data.LeaseCertDate,
                    Md167HouseId = data.Md167HouseId,
                    PurposeUsing = data.PurposeUsing,
                    UnitPriceValue = data.UnitPriceValue,
                    DocumentCode = data.DocumentCode,
                    PlanContent = data.PlanContent,
                    OriginPrice = data.OriginPrice,
                    ValueLand = data.ValueLand,
                    TypeHouse = data.TypeHouse,
                    StatusOfUse = data.StatusOfUse,
                    Note = data.Note,
                    HouAreaLand = data.InfoValue.HouAreaLand,
                    TaxNN = data.InfoValue.TaxNN,
                    UseFloorPb = data.InfoValue.UseFloorPb,
                    UseFloorPr = data.InfoValue.UseFloorPr,
                    UseLandPb = data.InfoValue.UseLandPb,
                    UseLandPr = data.InfoValue.UseLandPr,
                    AreBuildPb = data.InfoValue.AreBuildPb,
                    AreBuildPr = data.InfoValue.AreBuildPr,
                    AreaLandInSafe = data.InfoValue.AreaLandInSafe,
                    AreaLandInBankSafe = data.InfoValue.AreaLandInBankSafe,
                    AreaHouseInSafe = data.InfoValue.AreaHouseInSafe,
                    AreaHouseInBankSafe = data.InfoValue.AreaHouseInBankSafe,
                    ApaFloorCount = data.InfoValue.ApaFloorCount,
                    ApaTax = data.InfoValue.ApaTax,
                    ApaIsBasement = data.InfoValue.ApaIsBasement,
                    ApaValue = data.InfoValue.ApaValue,
                    KiosStatus = data.InfoValue.KiosStatus,
                    AreaFloorBuild = data.InfoValue.AreaFloorBuild,
                    AreaTunnel = data.InfoValue.AreaTunnel,
                    TextureScale = data.TextureScale,
                    HouseTypeId = data.HouseTypeId,
                    HouseTypeName = _context.Md167HouseTypes.Where(x => x.Id == data.HouseTypeId).Select(x => x.Code).FirstOrDefault(),
                    ProvinceName = _context.Provincies.Where(x => x.Id == data.ProvinceId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault() == null ? "" : _context.Provincies.Where(x => x.Id == data.ProvinceId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault(),
                    DistrictName = _context.Districts.Where(x => x.Id == data.ProvinceId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault() == null ? "" : _context.Districts.Where(x => x.Id == data.DistrictId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault(),
                    LaneName = _context.Lanies.Where(x => x.Id == data.ProvinceId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault() == null ? "" : _context.Lanies.Where(x => x.Id == data.LandId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault(),
                    WardName = _context.Wards.Where(x => x.Id == data.ProvinceId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault() == null ? "" : _context.Wards.Where(x => x.Id == data.WardId && x.Status != AppEnums.EntityStatus.DELETED).Select(x => x.Name).FirstOrDefault(),
                    md167HouseInfos = _context.Md167HouseInfos.Where(x => x.Md167HouseId == data.Id && x.Status != AppEnums.EntityStatus.DELETED).ToList(),
                    md167HouseProposes = _context.Md167HouseProposes.Where(x => x.Md167HouseId == data.Id && x.Status != AppEnums.EntityStatus.DELETED).ToList(),
                    md167Kios = _context.Md167Houses.Where(x => x.Status != AppEnums.EntityStatus.DELETED
                                && x.TypeHouse == Md167House.Type_House.Kios && x.Md167HouseId == data.Id)
                    .Select(h => new Md167KiosResData
                    {
                        Id = h.Id,
                        Code = h.Code,
                        KiosStatus = h.InfoValue.KiosStatus,
                        HouseNumber = h.HouseNumber,
                        TaxNN = h.InfoValue.TaxNN,
                        UseFloorPb = h.InfoValue.UseFloorPb,
                        Note = h.Note,
                        UseFloorPr = h.InfoValue.UseFloorPr,
                    }).ToList(),
                };
                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                //AnnualInstallmentData res = _mapper.Map<AnnualInstallmentData>(data);
                //UnitPrice unitPrice = _context.UnitPricies.Where(x => x.Id == res.UnitPriceId).FirstOrDefault();
                //res.UnitPriceName = res.UnitPriceName != null && res.UnitPriceName != "" ? (unitPrice != null ? String.Join(", ", res.UnitPriceName, unitPrice.Name) : res.UnitPriceName) : (unitPrice != null ? unitPrice.Name : res.UnitPriceName);
                //def.data = res;
                if (res.TypeHouse == Md167House.Type_House.Kios)
                    res.ApaKiosCode = _context.Md167Houses.Where(x => x.Id == res.Md167HouseId).Select(x => x.Code).FirstOrDefault();
                def.data = res;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("GetById Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        // POST: api/Md167House
        [HttpPost]
        public async Task<IActionResult> Post(Md167HouseReqData input)
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
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.CREATE))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_CREATE_MESSAGE);
                return Ok(def);
            }

            try
            {
                input = (Md167HouseReqData)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    var newData = new Md167House
                    {
                        CreatedById = userId,
                        CreatedBy = fullName,
                        HouseNumber = input.HouseNumber,
                        ProvinceId = input.ProvinceId,
                        DistrictId = input.DistrictId,
                        WardId = input.WardId,
                        DocumentDate = input.DocumentDate,
                        LaneId = input.LaneId,
                        MapNumber = input.MapNumber,
                        ParcelNumber = input.ParcelNumber,
                        LandTaxRate = input.LandTaxRate,
                        PlanningInfor = input.PlanningInfor,
                        LandId = input.LandId,
                        Md167TransferUnitId = input.Md167TransferUnitId,
                        ReceptionDate = input.ReceptionDate,
                        decree = input.decree,
                        Location = input.Location,
                        LocationCoefficient = input.LocationCoefficient,
                        UnitPrice = input.UnitPrice,
                        SHNNCode = input.SHNNCode,
                        IsPayTax = input.IsPayTax,
                        SHNNDate = input.SHNNDate,
                        LandPrice = input.LandPrice,
                        UnitPriceValue = input.UnitPriceValue,
                        ContractCode = input.ContractCode,
                        ContractDate = input.ContractDate,
                        HouseTypeId = input.HouseTypeId,
                        TextureScale = input.TextureScale,
                        LeaseCode = input.LeaseCode,
                        LeaseDate = input.LeaseDate,
                        LeaseCertCode = input.LeaseCertCode,
                        LeaseCertDate = input.LeaseCertDate,
                        Md167HouseId = 0,
                        PurposeUsing = input.PurposeUsing,
                        DocumentCode = input.DocumentCode,
                        PlanContent = input.PlanContent,
                        OriginPrice = input.OriginPrice,
                        ValueLand = input.ValueLand,
                        TypeHouse = input.TypeHouse,
                        StatusOfUse = input.StatusOfUse,
                        Note = input.Note,
                        LandPriceItemId = input.LandPriceItemId
                        //AreaValueId = input.AreaValueId,
                    };

                    if (newData.TypeHouse == Md167House.Type_House.House)
                    {
                        newData.InfoValue.HouAreaLand = input.HouAreaLand;
                        newData.InfoValue.TaxNN = input.TaxNN;
                        newData.InfoValue.UseFloorPb = input.UseFloorPb;
                        newData.InfoValue.UseFloorPr = input.UseFloorPr;
                        newData.InfoValue.UseLandPb = input.UseLandPb;
                        newData.InfoValue.UseLandPr = input.UseLandPr;
                        newData.InfoValue.AreBuildPb = input.AreBuildPb;
                        newData.InfoValue.AreBuildPr = input.AreBuildPr;
                        newData.InfoValue.AreaLandInSafe = input.AreaLandInSafe;
                        newData.InfoValue.AreaHouseInSafe = input.AreaHouseInSafe;
                        newData.InfoValue.AreaLandInBankSafe = input.AreaLandInBankSafe;
                        newData.InfoValue.AreaHouseInBankSafe = input.AreaHouseInBankSafe;
                    }
                    if (newData.TypeHouse == Md167House.Type_House.Apartment)
                    {
                        newData.InfoValue.AreaFloorBuild = input.AreaFloorBuild;
                        newData.InfoValue.AreaTunnel = input.AreaTunnel;
                        newData.InfoValue.UseFloorPb = input.UseFloorPb;
                        newData.InfoValue.UseFloorPr = input.UseFloorPr;
                        newData.InfoValue.UseLandPb = input.UseLandPb;
                        newData.InfoValue.UseLandPr = input.UseLandPr;
                        newData.InfoValue.AreaLandInSafe = input.AreaLandInSafe;
                        newData.InfoValue.AreaHouseInSafe = input.AreaHouseInSafe;
                        newData.InfoValue.AreaLandInBankSafe = input.AreaLandInBankSafe;
                        newData.InfoValue.AreaHouseInBankSafe = input.AreaHouseInBankSafe;
                        newData.InfoValue.ApaFloorCount = input.ApaFloorCount;
                        newData.InfoValue.ApaTax = input.ApaTax;
                        newData.InfoValue.ApaIsBasement = input.ApaIsBasement;
                        newData.InfoValue.ApaValue = input.ApaValue;
                    }
                    _context.Md167Houses.Add(newData);
                    await _context.SaveChangesAsync();
                    newData.Code = CodeIndentity.CodeInd("NHA", newData.Id, 6);
                    _context.Update(newData);
                    if (input.md167HouseProposes != null)
                    {
                        input.md167HouseProposes.ForEach(item =>
                        {
                            item.Md167HouseId = newData.Id;
                            item.CreatedBy = fullName;
                            item.CreatedById = userId;
                            _context.Md167HouseProposes.Add(item);
                        });
                    }
                    if (input.md167HouseInfos != null)
                    {
                        input.md167HouseInfos.ForEach(item =>
                        {
                            item.Md167HouseId = newData.Id;
                            item.CreatedBy = fullName;
                            item.CreatedById = userId;
                            _context.Md167HouseInfos.Add(item);
                        });
                    }
                    if (input.md167Kios != null)
                    {
                        input.md167Kios.ForEach(async item =>
                        {
                            var newKios = new Md167House
                            {
                                Code = CodeIndentity.CodeInd("KIOS", _context.Md167Houses.Where(x => x.TypeHouse == Md167House.Type_House.Kios).Count() + 1, 6),
                                Md167HouseId = newData.Id,
                                IsPayTax = item.IsPayTax,
                                TypeHouse = Md167House.Type_House.Kios,
                                CreatedBy = fullName,
                                CreatedById = userId,
                                HouseNumber = item.HouseNumber,
                            };
                            newKios.InfoValue.TaxNN = item.TaxNN;
                            newKios.InfoValue.UseFloorPb = item.UseFloorPb;
                            newKios.InfoValue.UseFloorPr = item.UseFloorPr;
                            newKios.InfoValue.KiosStatus = item.KiosStatus;
                            _context.Md167Houses.Add(newKios);
                            await _context.SaveChangesAsync();
                        });
                    }

                    try
                    {
                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Thêm mới nhà " + input.HouseNumber, "House", newData.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(newData), (int)AppEnums.Action.CREATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (newData.Id > 0)
                            transaction.Commit();
                        else
                            transaction.Rollback();

                        def.meta = new Meta(200, ApiConstants.MessageResource.ADD_SUCCESS);
                        def.data = newData;
                        return Ok(def);
                    }
                    catch (DbUpdateException e)
                    {
                        log.Error("DbUpdateException:" + e);
                        transaction.Rollback();
                        if (Md167HouseExists(input.Id))
                        {
                            def.meta = new Meta(212, "Đã tồn tại Id trên hệ thống!");
                            return Ok(def);
                        }
                        else
                        {
                            def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                            return Ok(def);
                        }
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

        // PUT: api/Area/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Md167HouseReqData input)
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
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.UPDATE))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_UPDATE_MESSAGE);
                return Ok(def);
            }

            try
            {
                input = (Md167HouseReqData)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                if (id != input.Id)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                Md167House data = await _context.Md167Houses.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_UPDATE_MESSAGE);
                    return Ok(def);
                }
                using (var transaction = _context.Database.BeginTransaction())
                {

                    var newData = new Md167House
                    {
                        Id = input.Id,
                        UpdatedAt = DateTime.Now,
                        UpdatedById = userId,
                        UpdatedBy = fullName,
                        CreatedById = data.CreatedById,
                        CreatedBy = data.CreatedBy,
                        Code = data.Code,
                        CreatedAt = data.CreatedAt,
                        DocumentDate = input.DocumentDate,
                        Status = data.Status,
                        HouseNumber = input.HouseNumber,
                        ProvinceId = input.ProvinceId,
                        DistrictId = input.DistrictId,
                        WardId = input.WardId,
                        LaneId = input.LaneId,
                        IsPayTax = input.IsPayTax,
                        MapNumber = input.MapNumber,
                        ParcelNumber = input.ParcelNumber,
                        LandTaxRate = input.LandTaxRate,
                        PlanningInfor = input.PlanningInfor,
                        LandId = input.LandId,
                        Md167TransferUnitId = input.Md167TransferUnitId,
                        ReceptionDate = input.ReceptionDate,
                        decree = input.decree,
                        Location = input.Location,
                        TextureScale = input.TextureScale,
                        LocationCoefficient = input.LocationCoefficient,
                        UnitPrice = input.UnitPrice,
                        LandPrice = input.LandPrice,
                        SHNNCode = input.SHNNCode,
                        SHNNDate = input.SHNNDate,
                        HouseTypeId = input.HouseTypeId,
                        ContractCode = input.ContractCode,
                        ContractDate = input.ContractDate,
                        LeaseCode = input.LeaseCode,
                        UnitPriceValue = input.UnitPriceValue,
                        LeaseDate = input.LeaseDate,
                        LeaseCertCode = input.LeaseCertCode,
                        LeaseCertDate = input.LeaseCertDate,
                        Md167HouseId = 0,
                        PurposeUsing = input.PurposeUsing,
                        DocumentCode = input.DocumentCode,
                        PlanContent = input.PlanContent,
                        OriginPrice = input.OriginPrice,
                        ValueLand = input.ValueLand,
                        TypeHouse = input.TypeHouse,
                        StatusOfUse = input.StatusOfUse,
                        Note = input.Note,
                        LandPriceItemId = input.LandPriceItemId
                        //AreaValueId=input.AreaValueId,
                    };
                    if (newData.TypeHouse == Md167House.Type_House.House)
                    {
                        newData.InfoValue.HouAreaLand = input.HouAreaLand;
                        newData.InfoValue.TaxNN = input.TaxNN;
                        newData.InfoValue.UseFloorPb = input.UseFloorPb;
                        newData.InfoValue.UseFloorPr = input.UseFloorPr;
                        newData.InfoValue.UseLandPb = input.UseLandPb;
                        newData.InfoValue.UseLandPr = input.UseLandPr;
                        newData.InfoValue.AreBuildPb = input.AreBuildPb;
                        newData.InfoValue.AreBuildPr = input.AreBuildPr;
                        newData.InfoValue.AreaLandInSafe = input.AreaLandInSafe;
                        newData.InfoValue.AreaHouseInSafe = input.AreaHouseInSafe;
                        newData.InfoValue.AreaLandInBankSafe = input.AreaLandInBankSafe;
                        newData.InfoValue.AreaHouseInBankSafe = input.AreaHouseInBankSafe;
                    }
                    if (newData.TypeHouse == Md167House.Type_House.Apartment)
                    {
                        newData.InfoValue.AreaFloorBuild = input.AreaFloorBuild;
                        newData.InfoValue.AreaTunnel = input.AreaTunnel;
                        newData.InfoValue.UseFloorPb = input.UseFloorPb;
                        newData.InfoValue.UseFloorPr = input.UseFloorPr;
                        newData.InfoValue.UseLandPb = input.UseLandPb;
                        newData.InfoValue.UseLandPr = input.UseLandPr;
                        newData.InfoValue.AreaLandInSafe = input.AreaLandInSafe;
                        newData.InfoValue.AreaHouseInSafe = input.AreaHouseInSafe;
                        newData.InfoValue.AreaLandInBankSafe = input.AreaLandInBankSafe;
                        newData.InfoValue.AreaHouseInBankSafe = input.AreaHouseInBankSafe;
                        newData.InfoValue.ApaFloorCount = input.ApaFloorCount;
                        newData.InfoValue.ApaTax = input.ApaTax;
                        newData.InfoValue.ApaIsBasement = input.ApaIsBasement;
                        newData.InfoValue.ApaValue = input.ApaValue;
                    }
                    _context.Update(newData);
                    if (input.md167HouseProposes != null)
                    {
                        var oldItem = _context.Md167HouseProposes.Where(p => p.Md167HouseId == input.Id && p.Status != AppEnums.EntityStatus.DELETED).ToList();
                        foreach (var item in oldItem)
                        {
                            item.Status = AppEnums.EntityStatus.DELETED;
                            _context.Update(item);
                            await _context.SaveChangesAsync();
                        }
                        input.md167HouseProposes.ForEach(item =>
                        {
                            item.Md167HouseId = newData.Id;
                            item.CreatedBy = fullName;
                            item.CreatedById = userId;
                            _context.Md167HouseProposes.Add(item);
                        });
                        await _context.SaveChangesAsync();
                    }
                    if (input.md167HouseInfos != null)
                    {
                        var oldItem = _context.Md167HouseInfos.Where(p => p.Md167HouseId == input.Id && p.Status != AppEnums.EntityStatus.DELETED).ToList();
                        foreach (var item in oldItem)
                        {
                            item.Status = AppEnums.EntityStatus.DELETED;
                            _context.Update(item);
                            await _context.SaveChangesAsync();
                        }
                        input.md167HouseInfos.ForEach(item =>
                        {
                            item.Md167HouseId = newData.Id;
                            item.CreatedBy = fullName;
                            item.CreatedById = userId;
                            _context.Md167HouseInfos.Add(item);
                        });
                        await _context.SaveChangesAsync();
                    }
                    List<Md167House> md167Kioss = _context.Md167Houses.Where(l => l.Md167HouseId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if (input.md167Kios != null)
                    {
                        foreach (var md167Kios in input.md167Kios)
                        {
                            Md167House Md167HouseExist = md167Kioss.Where(l => l.Id == md167Kios.Id).FirstOrDefault();
                            if (Md167HouseExist == null)
                            {
                                var newItem = new Md167House
                                {
                                    Md167HouseId = input.Id,
                                    CreatedBy = fullName,
                                    CreatedById = userId,
                                    TypeHouse = Md167House.Type_House.Kios,
                                    Code = CodeIndentity.CodeInd("KIOS", _context.Md167Houses.Where(x => x.TypeHouse == Md167House.Type_House.Kios).Count() + 1, 6),
                                    HouseNumber = md167Kios.HouseNumber,
                                    IsPayTax = md167Kios.IsPayTax,
                                    Note = md167Kios.Note,
                                };
                                newItem.InfoValue.UseFloorPb = md167Kios.UseFloorPb;
                                newItem.InfoValue.UseFloorPr = md167Kios.UseFloorPr;
                                newItem.InfoValue.KiosStatus = md167Kios.KiosStatus;
                                newItem.InfoValue.TaxNN = md167Kios.TaxNN;

                                _context.Md167Houses.Add(newItem);
                                _context.SaveChanges();
                            }
                            else
                            {
                                Md167HouseExist.UpdatedBy = fullName;
                                Md167HouseExist.UpdatedById = userId;
                                Md167HouseExist.Note = md167Kios.Note;
                                Md167HouseExist.IsPayTax = md167Kios.IsPayTax;
                                Md167HouseExist.InfoValue.TaxNN = md167Kios.TaxNN;
                                Md167HouseExist.InfoValue.KiosStatus = md167Kios.KiosStatus;
                                Md167HouseExist.InfoValue.UseFloorPb = md167Kios.UseFloorPb;
                                Md167HouseExist.InfoValue.UseFloorPr = md167Kios.UseFloorPr;
                                Md167HouseExist.HouseNumber = md167Kios.HouseNumber;

                                _context.Update(Md167HouseExist);

                                md167Kioss.Remove(Md167HouseExist);
                            }
                        }
                    }
                    var dataDelete = new List<Md167House>();
                    if (md167Kioss.Count > 0)
                        md167Kioss.ForEach(item =>
                        {
                            var deleteItem = _context.Md167Houses.Find(item.Id);
                            deleteItem.UpdatedAt = DateTime.Now;
                            deleteItem.UpdatedById = userId;
                            deleteItem.UpdatedBy = fullName;
                            deleteItem.Status = AppEnums.EntityStatus.DELETED;
                            dataDelete.Add(deleteItem);
                        });
                    _context.UpdateRange(dataDelete);
                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Sửa nhà " + input.HouseNumber, "Md167House", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (input.Id > 0)
                            transaction.Commit();
                        else
                            transaction.Rollback();

                        def.meta = new Meta(200, ApiConstants.MessageResource.UPDATE_SUCCESS);
                        def.data = data;
                        return Ok(def);
                    }
                    catch (DbUpdateException e)
                    {
                        transaction.Rollback();
                        log.Error("DbUpdateException:" + e);
                        if (!Md167HouseExists(data.Id))
                        {
                            def.meta = new Meta(212, "Không tồn tại Id trên hệ thống!");
                            return Ok(def);
                        }
                        else
                        {
                            def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                            return Ok(def);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("Put Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }
        //[HttpGet("GetAreaValue")]
        //public async Task<IActionResult> GetAreaValue(int id)
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
        //    string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
        //    if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
        //    {
        //        def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
        //        return Ok(def);
        //    }

        //    try
        //    {
        //        var data = _context.Md167AreaValueApplies.Where(x => x.DistrictId == id && x.Status != EntityStatus.DELETED).OrderByDescending(x => x.CreatedAt).FirstOrDefault();

        //        if (data == null)
        //        {
        //            def.data = new Md167AreaValue();
        //            return Ok(def);
        //        }
        //        var res = _context.Md167AreaValues.Where(x => x.Id == data.AreaValueId && x.Status != EntityStatus.DELETED).FirstOrDefault();
        //        if (res == null)
        //        {
        //            def.data = new Md167AreaValue();
        //            return Ok(def);
        //        }
        //        def.data = res;
        //        return Ok(def);
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error("GetById Error:" + ex);
        //        def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
        //        return Ok(def);
        //    }
        //}
        //DELETE: api/Area/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
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
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.DELETED))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_DELETE_MESSAGE);
                return Ok(def);
            }

            try
            {
                Md167House data = await _context.Md167Houses.FindAsync(id);
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_DELETE_MESSAGE);
                    return Ok(def);
                }

                //Kiểm tra dữ liệu liên quan có tồn tại thì không thể xóa
                //Md167Contract
                Md167Contract md167Contract = _context.Md167Contracts.Where(b => b.HouseId == id && b.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                if (md167Contract != null)
                {
                    def.meta = new Meta(222, "Dữ liệu liên quan còn tồn tại (Md167Contract). Không thể xóa bản ghi này!");
                    return Ok(def);
                }
                //Kiểm tra dữ liệu liên quan có tồn tại thì không thể xóa
                //Md167Kios
                Md167House md167Kios = _context.Md167Houses.Where(b => b.TypeHouse == Type_House.Kios && b.Md167HouseId == id && b.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                if (md167Contract != null)
                {
                    def.meta = new Meta(222, "Dữ liệu liên quan còn tồn tại (Md167Kios). Không thể xóa bản ghi này!");
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    data.UpdatedAt = DateTime.Now;
                    data.UpdatedById = userId;
                    data.UpdatedBy = fullName;
                    data.Status = AppEnums.EntityStatus.DELETED;
                    _context.Update(data);

                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Xóa Nhà " + data.Code, "Md167House", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.DELETED, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (data.Id > 0)
                            transaction.Commit();
                        else
                            transaction.Rollback();

                        def.meta = new Meta(200, ApiConstants.MessageResource.DELETE_SUCCESS);
                        def.data = data;
                        return Ok(def);
                    }
                    catch (DbUpdateException e)
                    {
                        transaction.Rollback();
                        log.Error("DbUpdateException:" + e);
                        if (!Md167HouseExists(data.Id))
                        {
                            def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_DELETE_MESSAGE);
                            return Ok(def);
                        }
                        else
                        {
                            def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                            return Ok(def);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }
        [HttpGet("DeleteKios/{id}")]
        public async Task<IActionResult> DeleteKios(int id)
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
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.DELETED))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_DELETE_MESSAGE);
                return Ok(def);
            }
            //Kiểm tra dữ liệu liên quan có tồn tại thì không thể xóa
            //Md167Contract
            Md167Contract md167Contract = _context.Md167Contracts.Where(b => b.HouseId == id && b.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
            if (md167Contract != null)
            {
                def.meta = new Meta(222, "Dữ liệu liên quan còn tồn tại hợp đồng thuê " + md167Contract.Code + ". Không thể xóa bản ghi này!");
                def.data = false;
                return Ok(def);
            }
            def.meta = new Meta(200, "Dữ liệu hợp lệ!");
            def.data = true;
            return Ok(def);
        }

        private bool Md167HouseExists(int id)
        {
            return _context.Md167Houses.Count(e => e.Id == id) > 0;
        }

        #region import thông tin nhà đất
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
                importHistory.Type = AppEnums.ImportHistoryType.Md167House;

                List<Md167HouseData> data = new List<Md167HouseData>();

                foreach (var file in httpRequest)
                {
                    var postedFile = httpRequest[i];
                    if (postedFile != null && postedFile.Length > 0)
                    {
                        IList<string> AllowedDocuments = new List<string> { ".xls", ".xlsx" };
                        int MaxContentLength = 1024 * 1024 * 32; //Size = 32 MB  

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
                            var message = string.Format("Vui lòng Up file có dung lượng nhỏ hơn 32 MB!");
                            def.meta = new Meta(600, message);
                            return Ok(def);
                        }
                        else
                        {
                            string folderName = _configuration["AppSettings:BaseUrlImportHistory"]; ;
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
                            using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                            {
                                fileData = binaryReader.ReadBytes((int)file.Length);
                                using (MemoryStream ms = new MemoryStream(fileData))
                                {
                                    data = importData(ms, 0, 2);
                                }
                            }
                        }
                    }
                    i++;
                }

                List<Province> provinces = _context.Provincies.Where(e => e.Status != EntityStatus.DELETED).ToList();
                List<District> districts = _context.Districts.Where(e => e.Status != EntityStatus.DELETED).ToList();
                List<Ward> wards = _context.Wards.Where(e => e.Status != EntityStatus.DELETED).ToList();
                List<Lane> lanes = _context.Lanies.Where(e => e.Status != EntityStatus.DELETED).ToList();

                var md167AreaValues = _context.Md167AreaValues.Where(x => x.Status != EntityStatus.DELETED).ToList();
                var md167AreaValueApplies = _context.Md167AreaValueApplies.Where(x => x.Status != EntityStatus.DELETED).ToList();

                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var dataItem in data)
                        {
                            if (dataItem.Valid == true)
                            {

                                //Kiểm tra loại nhà
                                Md167HouseType md167HouseType = _context.Md167HouseTypes.Where(m => m.Name == dataItem.HouseTypeName && m.Status != EntityStatus.DELETED).FirstOrDefault();
                                if (md167HouseType == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy Loại nhà\n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.HouseTypeId = md167HouseType.Id;
                                }

                                //Kiểm tra Tỉnh TP
                                string provinceNameNoneUnicode = UtilsService.NonUnicode(dataItem.ProviceName);
                                Province province = provinces.AsEnumerable().Where(e => UtilsService.NonUnicode(e.Name).Contains(provinceNameNoneUnicode)).FirstOrDefault();
                                if (province == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy tỉnh (tp)\n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.ProvinceId = province.Id;
                                }

                                //Kiểm tra Quận huyện
                                string districtNameNoneUnicode = UtilsService.NonUnicode(dataItem.DistrictName);
                                District district = districts.AsEnumerable().Where(e => e.ProvinceId == dataItem.ProvinceId && UtilsService.NonUnicode(e.Name).Contains(districtNameNoneUnicode)).FirstOrDefault();
                                if (district == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy quận (huyện)\n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.DistrictId = district.Id;
                                }

                                //Kiểm tra phường xã
                                string wardNameNoneUnicode = UtilsService.NonUnicode(dataItem.WardName);
                                Ward ward = wards.AsEnumerable().Where(e => e.DistrictId == dataItem.DistrictId && UtilsService.NonUnicode(e.Name).Contains(wardNameNoneUnicode)).FirstOrDefault();
                                if (ward == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy phường (xã)\n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.WardId = ward.Id;
                                }

                                //Kiểm tra duong
                                string laneNameNoneUnicode = UtilsService.NonUnicode(dataItem.LaneName);
                                Lane lane = lanes.AsEnumerable().Where(e => e.Ward == dataItem.WardId && UtilsService.NonUnicode(e.Name).Contains(laneNameNoneUnicode)).FirstOrDefault();
                                if (lane == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy đường\n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.LaneId = lane.Id;
                                }

                                //kiem tra thue xuat dat
                                Md167LandTax md167LandTax = _context.Md167LandTaxs.Where(m => m.Code == dataItem.LandTaxRateCode && m.Status != EntityStatus.DELETED).FirstOrDefault();
                                if (md167LandTax == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy thuế xuất đất\n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.LandTaxRate = md167LandTax.Id;
                                }

                                //check loai dat

                                //check don vi chuyen giao
                                Md167TranferUnit md167TranferUnit = _context.Md167TranferUnits.Where(m => m.Name == dataItem.Md167TransferUnitName && m.Status != EntityStatus.DELETED).FirstOrDefault();
                                if (md167TranferUnit == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy đơn vị chuyển giao\n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.Md167TransferUnitId = md167TranferUnit.Id;
                                }

                                //check nghi dinh
                                string type_decree = UtilsService.NonUnicode(dataItem.DecreeType);
                                if (type_decree == "99-2015-nd-cp")
                                {
                                    dataItem.decree = DecreeEnum.ND_CP_99_2015;
                                }
                                else if (type_decree == "34-2013-nd-cp")
                                {
                                    dataItem.decree = DecreeEnum.ND_CP_34_2013;
                                }
                                else if (type_decree == "61-nd-cp")
                                {
                                    dataItem.decree = DecreeEnum.ND_CP_61;
                                }
                                else
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Cột nghị định không hợp lệ\n";
                                    continue;
                                }

                                //check vi tri dat o

                                //check muc dich quan ly
                                Md167ManagePurpose md167ManagePurpose = _context.Md167ManagePurposes.Where(m => m.Name == dataItem.PurposeUsingName && m.Status != EntityStatus.DELETED).FirstOrDefault();
                                if (md167ManagePurpose == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy mục đích quản lý\n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.PurposeUsing = md167ManagePurpose.Id;
                                }

                                //check nội dung phương án
                                Md167PlantContent md167PlantContent = _context.Md167PlantContents.Where(m => m.Name == dataItem.PlanContentName && m.Status != EntityStatus.DELETED).FirstOrDefault();
                                if (md167PlantContent == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy nội dung phương án phê duyệt của UBND TP\n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.PlanContent = md167PlantContent.Id;
                                }

                                //check hiện trạng sử dụng đất
                                Md167StateOfUse md167StateOfUse = _context.Md167StateOfUses.Where(m => m.Name == dataItem.StatusOfUseName && m.Status != EntityStatus.DELETED).FirstOrDefault();
                                if (md167StateOfUse == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy hiện trạng sử dụng đất\n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.StatusOfUse = md167StateOfUse.Id;
                                }

                                //kiểm tra thông tin xem là nhà phố hay chung cư/nhà nhiều tầng: Nếu là nhà phố thì hiện thông tin nhà phố, nếu là nhà nhiều tầng thì hiện ra tông tin chung cư/nhà nhiều tầng
                                if (dataItem.IsTownHouse == true)
                                {
                                    dataItem.TypeHouse = Type_House.House;//nha pho
                                    dataItem.InfoValue.AreaLandInBankSafe = dataItem.AreaLandInBankSafeHouse;
                                    dataItem.InfoValue.AreaHouseInBankSafe = dataItem.AreaHouseInBankSafeHouse;
                                }
                                else
                                {

                                    dataItem.TypeHouse = Type_House.Apartment;//chung cu
                                    dataItem.InfoValue.AreaLandInBankSafe = dataItem.AreaLandInBankSafeApartment;
                                    dataItem.InfoValue.AreaHouseInBankSafe = dataItem.AreaHouseInBankSafeApartment;

                                }

                                List<Md167AreaValueApply> lstMd167AreaValueApply = new List<Md167AreaValueApply>();
                                foreach (var item in md167AreaValueApplies)
                                {
                                    var itemExist = md167AreaValues.Where(x => x.Id == item.AreaValueId).FirstOrDefault();
                                    if (itemExist != null)
                                        lstMd167AreaValueApply.Add(item);
                                }

                                decimal areaValue = GetAreaValue(dataItem.DistrictId, lstMd167AreaValueApply, md167AreaValues);

                                //Tính Hệ số vị trí và Đơn giá đất ở - Nếu không có thì vẫn thêm vào
                                Md167PositionValue md167PositionValue = _context.Md167PositionValues.Where(p => p.decree == dataItem.decree && p.Status != EntityStatus.DELETED).OrderByDescending(p => p.DoApply).FirstOrDefault();
                                if (md167PositionValue != null && dataItem.Location != null)
                                {
                                    string locationCoefficient = null;
                                    if (dataItem.Location == 1)
                                    {
                                        locationCoefficient = md167PositionValue.Position1;
                                    }
                                    else if (dataItem.Location == 2)
                                    {
                                        locationCoefficient = md167PositionValue.Position2;
                                    }
                                    else if (dataItem.Location == 3)
                                    {
                                        locationCoefficient = md167PositionValue.Position3;
                                    }
                                    else if (dataItem.Location == 4)
                                    {
                                        locationCoefficient = md167PositionValue.Position4;
                                    }

                                    dataItem.LocationCoefficient = locationCoefficient;

                                    List<LandPriceItem> landPriceItemData = (from l in _context.LandPricies
                                                                   join li in _context.LandPriceItems on l.Id equals li.LandPriceId
                                                                   where l.Status != EntityStatus.DELETED
                                                                       && li.Status != EntityStatus.DELETED
                                                                       && l.LandPriceType == landPriceType.MD167
                                                                       && l.District == dataItem.DistrictId
                                                                   select li).ToList();


                                    var landPriceItem = landPriceItemData.AsEnumerable().Where(e => UtilsService.NonUnicode(e.LaneName) == laneNameNoneUnicode && UtilsService.NonUnicode(e.LaneStartName) == UtilsService.NonUnicode(dataItem.LaneNameStart) && UtilsService.NonUnicode(e.LaneEndName) == UtilsService.NonUnicode(dataItem.LaneNameEnd)).FirstOrDefault();

                                    if (landPriceItem != null)
                                    {
                                        dataItem.UnitPriceValue = (decimal)(landPriceItem.Value * Decimal.ToDouble(areaValue) * double.Parse(locationCoefficient));
                                        dataItem.LandPriceItemId = landPriceItem.Id;
                                        dataItem.LandPrice = (decimal)landPriceItem.Value;
                                        string unitPrice = UtilsService.FormatMoney((decimal)landPriceItem.Value) + " đ/m² x " + areaValue + " x " + locationCoefficient + " = " + UtilsService.FormatMoney(dataItem.UnitPriceValue);
                                        dataItem.UnitPrice = unitPrice;
                                    }
                                }

                                dataItem.CreatedById = -1;
                                dataItem.CreatedBy = fullName;
                                dataItem.Code = "NHA";

                                _context.Md167Houses.Add(dataItem);
                                _context.SaveChanges();

                                dataItem.Code = CodeIndentity.CodeInd("NHA", dataItem.Id, 6);
                                _context.Update(dataItem);
                            }
                        }

                        importHistory.Data = data.Cast<dynamic>().ToList();
                        importHistory.CreatedById = userId;
                        importHistory.CreatedBy = fullName;

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

        public static List<Md167HouseData> importData(MemoryStream ms, int sheetnumber, int rowStart)
        {
            XSSFWorkbook workbook = new XSSFWorkbook(ms);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);

            List<Md167HouseData> res = new List<Md167HouseData>();
            for (int row = rowStart; row <= sheet.LastRowNum; row++)
            {
                if (sheet.GetRow(row) != null)
                {
                    //Đọc dữ liệu từng dùng excel
                    Md167HouseData input1Detai = new Md167HouseData();
                    input1Detai.Valid = true;
                    input1Detai.ErrMsg = "";

                    for (int i = 0; i < 55; i++)
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
                                        input1Detai.Index = int.Parse(str);
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Số thứ tự\n";
                                }
                            }
                            else if (i == 1)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.HouseNumber = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Số nhà không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Số nhà\n";
                                }
                            }
                            else if (i == 2)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.HouseTypeName = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột loại nhà không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột loại nhà\n";
                                }
                            }
                            else if (i == 3)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.ProviceName = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột tỉnh/thành phố không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột tỉnh/thành phố\n";
                                }
                            }
                            else if (i == 4)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.DistrictName = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột quận/huyện không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột quận/huyện\n";
                                }
                            }
                            else if (i == 5)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.WardName = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột xã/phường không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột xã/phường\n";
                                }
                            }
                            else if (i == 6)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.LaneName = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột đường không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột đường\n";
                                }
                            }
                            else if (i == 7)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.LaneNameStart = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Đoạn đường từ không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Đoạn đường từ\n";
                                }
                            }
                            else if (i == 8)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.LaneNameEnd = str;
                                    }
                                }
                                catch {}
                            }
                            else if (i == 9)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.MapNumber = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột số tờ không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột số tờ\n";
                                }
                            }
                            else if (i == 10)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.ParcelNumber = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột số thửa không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột số thửa\n";
                                }
                            }
                            else if (i == 11)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.LandTaxRateCode = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột thuế suất đất không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột thuế suất đất\n";
                                }
                            }
                            else if (i == 12)
                            {
                                try
                                {
                                    int namelandId = 0;
                                    if (str != "")
                                    {
                                        string landname = str;
                                        if (landname == Land_type_name.DAT_O)
                                        {
                                            namelandId = (int)Land_type.DAT_O;
                                        }
                                        else if (landname == Land_type_name.DAT_TMDV)
                                        {
                                            namelandId = (int)Land_type.DAT_TMDV;
                                        }
                                        else if (landname == Land_type_name.DAT_SXKD)
                                        {
                                            namelandId = (int)Land_type.DAT_SXKD;
                                        }
                                        else if (landname == Land_type_name.DAT_PUBLIC_KINHDOANH)
                                        {
                                            namelandId = (int)Land_type.DAT_PUBLIC_KINHDOANH;
                                        }
                                        else if (landname == Land_type_name.DAT_COQUAN_CONGTRINH)
                                        {
                                            namelandId = (int)Land_type.DAT_COQUAN_CONGTRINH;
                                        }
                                        else
                                        {
                                            input1Detai.Valid = false;
                                            input1Detai.ErrMsg += "Giá trị loại đất không đúng\n";
                                        }
                                        input1Detai.LandId = namelandId;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột loại đất không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột loại đất\n";
                                }
                            }
                            else if (i == 13)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.Md167TransferUnitName = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột đơn vị chuyển giao không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột đơn vị chuyển giao\n";
                                }
                            }
                            else if (i == 14)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.DecreeType = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột nghị định không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột nghị định\n";
                                }
                            }
                            else if (i == 15)
                            {
                                try
                                {
                                    //int locationId = 0;

                                    if (str != "")
                                    {
                                        //string locationName = str;
                                        //if(locationName == Location_name.VI_TRI_1)
                                        //{
                                        //    locationId = (int)Location.VI_TRI_1;
                                        //}
                                        //else if(locationName == Location_name.VI_TRI_2)
                                        //{
                                        //    locationId = (int)Location.VI_TRI_2;
                                        //}
                                        //else if (locationName == Location_name.VI_TRI_3)
                                        //{
                                        //    locationId = (int)Location.VI_TRI_3;
                                        //}
                                        //else if (locationName == Location_name.VI_TRI_4)
                                        //{
                                        //    locationId = (int)Location.VI_TRI_4;
                                        //}
                                        //else
                                        //{
                                        //    input1Detai.Valid = false;
                                        //    input1Detai.ErrMsg += "Giá trị vị trí đất ở không đúng\n";
                                        //}
                                        input1Detai.Location = int.Parse(str);
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột vị trí đất ở không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột vị trí đất ở\n";
                                }
                            }
                            else if (i == 16)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.SHNNCode = str;
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Số\n";
                                }
                            }
                            else if (i == 17)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.SHNNDate = DateTime.FromOADate(Double.Parse(str));
                                        if (input1Detai.SHNNDate.Year < 1900)
                                        {
                                            input1Detai.Valid = false;
                                            input1Detai.ErrMsg += "Ngày không hợp lệ\n";
                                        }
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột ngày\n";
                                }
                            }
                            else if (i == 18)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.ContractCode = str;
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Số\n";
                                }
                            }
                            else if (i == 19)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.ContractDate = DateTime.FromOADate(Double.Parse(str));
                                        if (input1Detai.ContractDate.Year < 1900)
                                        {
                                            input1Detai.Valid = false;
                                            input1Detai.ErrMsg += "Ngày không hợp lệ\n";
                                        }
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột ngày\n";
                                }
                            }

                            else if (i == 20)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.LeaseCode = str;
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Số\n";
                                }
                            }
                            else if (i == 21)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.LeaseDate = DateTime.FromOADate(Double.Parse(str));
                                        if (input1Detai.LeaseDate.Year < 1900)
                                        {
                                            input1Detai.Valid = false;
                                            input1Detai.ErrMsg += "Ngày không hợp lệ\n";
                                        }
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột ngày\n";
                                }
                            }
                            else if (i == 22)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.LeaseCertCode = str;
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Số\n";
                                }
                            }
                            else if (i == 23)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.LeaseCertDate = DateTime.FromOADate(Double.Parse(str));
                                        if (input1Detai.LeaseCertDate.Year < 1900)
                                        {
                                            input1Detai.Valid = false;
                                            input1Detai.ErrMsg += "Ngày không hợp lệ\n";
                                        }
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột ngày\n";
                                }
                            }
                            else if (i == 24)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.DocumentCode = str;
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Số\n";
                                }
                            }
                            else if (i == 25)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.DocumentDate = DateTime.FromOADate(Double.Parse(str));
                                        if (input1Detai.DocumentDate.Year < 1900)
                                        {
                                            input1Detai.Valid = false;
                                            input1Detai.ErrMsg += "Ngày không hợp lệ\n";
                                        }
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột ngày\n";
                                }
                            }
                            else if (i == 26)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.PurposeUsingName = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Mục đích quản lý sử dụng phê duyệt theo quyết định không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Mục đích quản lý sử dụng phê duyệt theo quyết định\n";
                                }
                            }
                            else if (i == 27)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.PlanContentName = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Nội dung phương án được phê duyệt theo Quyết định không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Nội dung phương án được phê duyệt theo Quyết định\n";
                                }
                            }
                            else if (i == 28)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.OriginPrice = decimal.Parse(str);
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Nguyên giá nhà, đất khi nhận bàn giao\n";
                                }
                            }
                            else if (i == 29)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.ValueLand = decimal.Parse(str);
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Giá trị còn lại của Nhà phố\n";
                                }
                            }
                            else if (i == 30)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.StatusOfUseName = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Hiện trạng sử dụng không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Hiện trạng sử dụng\n";
                                }
                            }
                            else if (i == 31)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.TextureScale = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Quy mô kết cấu không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Quy mô kết cấu\n";
                                }
                            }
                            else if (i == 32)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        if (str.ToUpper() == "TRUE")
                                        {
                                            input1Detai.IsTownHouse = true;
                                        }
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột nhà phố\n";
                                }
                            }
                            else if (i == 39)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        if (str.ToUpper() == "TRUE")
                                        {
                                            input1Detai.IsApartment = true;
                                        }
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột nhà chung cư/nhà nhiều tầng\n";
                                }
                            }
                            else if (i == 53)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.Note = str;
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Ghi chú\n";
                                }
                            }

                            if (input1Detai.IsTownHouse == true)
                            {
                                if (i == 33)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.InfoValue.HouAreaLand = decimal.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột diện tích lô đất\n";
                                    }
                                }
                                else if (i == 34)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.InfoValue.TaxNN = decimal.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột Thuế đất phí nông nghiệp\n";
                                    }
                                }
                                else if (i == 35)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.InfoValue.AreBuildPb = decimal.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột Diện tích đất xây dựng chung\n";
                                    }
                                }
                                else if (i == 36)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.InfoValue.AreBuildPr = decimal.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột Diện tích đất xây dựng riêng\n";
                                    }
                                }
                                else if (i == 37)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.AreaLandInBankSafeHouse = decimal.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột Diện tích đất thuộc hành lang an toàn sông\n";
                                    }
                                }
                                else if (i == 38)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.AreaHouseInBankSafeHouse = decimal.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột Diện tích nhà thuộc hành lang an toàn sông\n";
                                    }
                                }
                                else if (i == 47)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.InfoValue.UseFloorPb = decimal.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột Diện tích sàn sử dụng chung\n";
                                    }
                                }

                                else if (i == 48)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.InfoValue.UseFloorPr = decimal.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột Diện tích sàn sử dụng riêng\n";
                                    }
                                }

                                else if (i == 49)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.InfoValue.UseLandPb = decimal.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột Diện tích đất sử dụng riêng\n";
                                    }
                                }

                                else if (i == 50)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.InfoValue.UseLandPr = decimal.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột Diện tích đất sử dụng riêng\n";
                                    }
                                }

                                else if (i == 51)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.InfoValue.AreaLandInSafe = decimal.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột Diện tích đất trong lộ giới\n";
                                    }
                                }

                                else if (i == 52)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.InfoValue.AreaHouseInSafe = decimal.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột Diện tích nhà trong lộ giới\n";
                                    }
                                }
                            }
                            else
                            {
                                if (i == 40)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.InfoValue.AreaFloorBuild = decimal.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột sàn xây dựng\n";
                                    }
                                }
                                if (i == 41)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.InfoValue.AreaTunnel = decimal.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột diện tích hầm\n";
                                    }
                                }
                                else if (i == 42)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.AreaLandInBankSafeApartment = decimal.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột Diện tích đất thuộc hành lang an toàn sông\n";
                                    }
                                }
                                else if (i == 43)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.AreaHouseInBankSafeApartment = decimal.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột Diện tích nhà thuộc hành lang an toàn sông\n";
                                    }
                                }
                                else if (i == 44)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.InfoValue.ApaFloorCount = int.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột Số tầng\n";
                                    }
                                }
                                else if (i == 45)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.InfoValue.ApaTax = decimal.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột số thuế miễn giảm\n";
                                    }
                                }
                                else if (i == 46)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            if (str.ToUpper() == "TRUE")
                                            {
                                                input1Detai.InfoValue.ApaIsBasement = true;
                                            }
                                            else
                                            {
                                                input1Detai.InfoValue.ApaIsBasement = false;
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột Tầng hầm\n";
                                    }
                                }
                                else if (i == 47)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.InfoValue.UseFloorPb = decimal.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột Diện tích sàn sử dụng chung\n";
                                    }
                                }

                                else if (i == 48)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.InfoValue.UseFloorPr = decimal.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột Diện tích sàn sử dụng riêng\n";
                                    }
                                }

                                else if (i == 49)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.InfoValue.UseLandPb = decimal.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột Diện tích đất sử dụng riêng\n";
                                    }
                                }

                                else if (i == 50)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.InfoValue.UseLandPr = decimal.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột Diện tích đất sử dụng riêng\n";
                                    }
                                }

                                else if (i == 51)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.InfoValue.AreaLandInSafe = decimal.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột Diện tích đất trong lộ giới\n";
                                    }
                                }

                                else if (i == 52)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.InfoValue.AreaHouseInSafe = decimal.Parse(str);
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột Diện tích nhà trong lộ giới\n";
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            input1Detai.Valid = false;
                            input1Detai.ErrMsg += "Lỗi dữ liệu\n";
                            log.Error("Exception:" + ex);
                        }
                    }
                    res.Add(input1Detai);
                }
            }
            return res;
        }

        #endregion

        #region import kios
        [HttpPost]
        [Route("ImportDataExcelKios")]
        public IActionResult ImportDataExcelKios()
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
                importHistory.Type = AppEnums.ImportHistoryType.Md167Kios;
                List<Md167KiosData> data = new List<Md167KiosData>();
                foreach (var file in httpRequest)
                {
                    var postedFile = httpRequest[i];
                    if (postedFile != null && postedFile.Length > 0)
                    {
                        IList<string> AllowedDocuments = new List<string> { ".xls", ".xlsx" };
                        int MaxContentLength = 1024 * 1024 * 32; //Size = 32 MB  

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
                            var message = string.Format("Vui lòng Up file có dung lượng nhỏ hơn 32 MB!");
                            def.meta = new Meta(600, message);
                            return Ok(def);
                        }
                        else
                        {
                            string folderName = _configuration["AppSettings:BaseUrlImportHistory"]; ;
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
                            using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                            {
                                fileData = binaryReader.ReadBytes((int)file.Length);
                                using (MemoryStream ms = new MemoryStream(fileData))
                                {
                                    data = importDataKios(ms, 0, 1);
                                }
                            }
                        }
                    }
                    i++;
                }

                List<Md167KiosData> dataValid = new List<Md167KiosData>();
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var dataItem in data)
                        {
                            if (dataItem.Valid == true)
                            {
                                //Mã Kios tự tăng
                                dataItem.Code = CodeIndentity.CodeInd("KIOS", _context.Md167Houses.Where(x => x.TypeHouse == Md167House.Type_House.Kios).Count() + 1, 6);

                                //Check Trạng thái
                                string type_kios = UtilsService.NonUnicode(dataItem.KiosType);
                                if (type_kios == "dang-cho-thue")
                                {
                                    dataItem.InfoValue.KiosStatus = Kios_Status.DANG_CHO_THUE;
                                }
                                else if (type_kios == "chua-cho-thue")
                                {
                                    dataItem.InfoValue.KiosStatus = Kios_Status.CHUA_CHO_THUE;
                                }
                                else
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg = "Cột trạng thái kios không hợp lệ";
                                    continue;
                                }

                                //Check mã nhà
                                Md167House md167House = _context.Md167Houses.Where(m => m.Code == dataItem.CodeHouse && m.TypeHouse == Md167House.Type_House.Apartment && m.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                if (md167House == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg = "Không tìm thấy nhà";
                                }
                                else
                                {
                                    dataItem.Md167HouseId = md167House.Id;
                                }
                                dataItem.TypeHouse = Md167House.Type_House.Kios;
                                dataItem.CreatedBy = fullName;
                                dataItem.CreatedById = -1;
                                dataValid.Add(dataItem);

                            }
                        }
                        importHistory.Data = data.Cast<dynamic>().ToList();
                        importHistory.CreatedById = userId;
                        importHistory.CreatedBy = fullName;

                        _context.ImportHistories.Add(importHistory);
                        _context.Md167Houses.AddRange(dataValid);
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

        public static List<Md167KiosData> importDataKios(MemoryStream ms, int sheetnumber, int rowStart)
        {
            XSSFWorkbook workbook = new XSSFWorkbook(ms);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);

            List<Md167KiosData> res = new List<Md167KiosData>();
            for (int row = rowStart; row <= sheet.LastRowNum; row++)
            {
                if (sheet.GetRow(row) != null)
                {
                    Md167KiosData input1Detai = new Md167KiosData();
                    input1Detai.Valid = true;
                    input1Detai.ErrMsg = "";

                    for (int i = 0; i < 7; i++)
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
                                        input1Detai.Index = int.Parse(str);
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Số thứ tự\n";
                                }
                            }
                            else if (i == 1)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.CodeHouse = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Mã nhà chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Mã nhà\n";
                                }
                            }
                            else if (i == 2)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.HouseNumber = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Tên Kios chưa có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Tên Kios\n";
                                }
                            }
                            else if (i == 3)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.InfoValue.UseFloorPb = decimal.Parse(str);
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Diện tích sử dụng sàn chung\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Diện tích sử dụng sàn chung\n";
                                }
                            }
                            else if (i == 4)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.InfoValue.UseFloorPr = decimal.Parse(str);
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Diện tích sử dụng sàn riêng\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Diện tích sử dụng sàn riêng\n";
                                }
                            }
                            else if (i == 5)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.KiosType = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Trạng thái Kios\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Trạng thái Kios\n";
                                }
                            }
                            else if (i == 6)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.Note = str;
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Ghi chú\n";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            input1Detai.Valid = false;
                            input1Detai.ErrMsg += "Lỗi dữ liệu\n";
                            log.Error("Exception:" + ex);
                        }
                    }

                    res.Add(input1Detai);
                }
            }
            return res;
        }
        #endregion
        [HttpGet("GetAreaValueHouse")]
        public async Task<IActionResult> GetAreaValueHouse(int districtId)
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
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }
            List<Md167AreaValueApply> lstMd167AreaValueApply = new List<Md167AreaValueApply>();
            var md167AreaValues = _context.Md167AreaValues.Where(x => x.Status != EntityStatus.DELETED).ToList();
            var md167AreaValueApplies = _context.Md167AreaValueApplies.Where(x => x.Status != EntityStatus.DELETED).ToList();
            foreach (var item in md167AreaValueApplies)
            {
                var itemExist = md167AreaValues.Where(x => x.Id == item.AreaValueId).FirstOrDefault();
                if (itemExist != null)
                    lstMd167AreaValueApply.Add(item);
            }
            var res = GetAreaValue(districtId, lstMd167AreaValueApply, md167AreaValues);
            def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
            def.data = res;
            return Ok(def);

        }

        [HttpGet("GetPriceHouse")]
        public async Task<IActionResult> GetPriceHouse(int districtId, int laneId)
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
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }
            var landName = _context.Lanies.Where(x => x.Status != EntityStatus.DELETED && x.Id == laneId).Select(x => x.Name).FirstOrDefault() ?? "";
            var lstLandPrice = _context.LandPricies.Where(x => x.Status != EntityStatus.DELETED && x.LandPriceType == landPriceType.MD167).ToList();
            var lstLandPriceItem = _context.LandPriceItems.Where(x => x.Status != EntityStatus.DELETED).ToList();
            var res = GetPrice(districtId, lstLandPrice, lstLandPriceItem, landName);
            def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
            def.data = res;
            return Ok(def);

        }

        [HttpGet("GetLandPriceData")]
        public async Task<IActionResult> GetLandPriceData(int districtId, int laneId, int decree)
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
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }

            List<LandPriceItem> landPriceItems = (from l in _context.LandPricies
                                                  join li in _context.LandPriceItems on l.Id equals li.LandPriceId
                                                  where l.Status != EntityStatus.DELETED && l.LandPriceType == landPriceType.MD167 && l.District == districtId && l.DecreeType1Id == decree
                                                    && li.Status != EntityStatus.DELETED
                                                  select li).ToList();

            var lane = _context.Lanies.Where(x => x.Status != EntityStatus.DELETED && x.Id == laneId).FirstOrDefault();

            if(lane == null)
            {
                def.meta = new Meta(404, "Không tìm thấy đường!");
                def.data = null;
                return Ok(def);
            }

            string laneName = UtilsService.NonUnicode(lane.Name);
            var res = landPriceItems.AsEnumerable().Where(e => UtilsService.NonUnicode(e.LaneName) == laneName).Select(li => new LandPriceItemDecreeData { 
                Id = li.Id,
                LaneName = li.LaneName + (li.LaneStartName != null && li.LaneStartName != "" ? (" từ đoạn " + li.LaneStartName) : "") + (li.LaneEndName != null && li.LaneEndName != "" ? (" đến đoạn " + li.LaneEndName) : "") + (li.Des != null && li.Des != "" ? (", " + li.Des) : ""),
                Value = li.Value
            }).ToList();

            def.meta = new Meta(200, ACCTION_SUCCESS);
            def.data = res;
            return Ok(def);

        }

        public class NameAndOldName
        {
            public string Name { get; set; }
            public List<OldName>? OldName { get; set; }
        }
    }
}
