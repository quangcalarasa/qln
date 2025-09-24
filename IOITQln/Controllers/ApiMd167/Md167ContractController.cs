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
using static IOITQln.Common.Enums.AppEnums;
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using static IOITQln.Controllers.ApiInv.HouseController;

namespace IOITQln.Controllers.ApiMd167
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class Md167ContractController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("md167-contract", "md167-contract");
        private static string functionCode = "Md167_CONTRACT_MANAGEMENT";
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;

        public Md167ContractController(ApiDbContext context, IMapper mapper, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
        }

        [HttpGet("GetByPage")]
        public IActionResult GetByPage([FromQuery] FilteredPagination paging)
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
                    def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                    IQueryable<Md167Contract> data = _context.Md167Contracts.Where(c => c.Type == Contract167Type.MAIN && c.Status != AppEnums.EntityStatus.DELETED);

                    if (paging.query != null)
                    {
                        paging.query = HttpUtility.UrlDecode(paging.query);
                    }

                    data = data.Where(paging.query);
                    def.metadata = data.Count();

                    if (paging.page_size > 0)
                    {
                        if (paging.order_by != null)
                        {
                            data = data.OrderBy(paging.order_by).Skip((paging.page - 1) * paging.page_size).Take(paging.page_size);
                        }
                        else
                        {
                            data = data.OrderBy("Id desc").Skip((paging.page - 1) * paging.page_size).Take(paging.page_size);
                        }
                    }
                    else
                    {
                        if (paging.order_by != null)
                        {
                            data = data.OrderBy(paging.order_by);
                        }
                        else
                        {
                            data = data.OrderBy("Id desc");
                        }
                    }

                    if (paging.select != null && paging.select != "")
                    {
                        paging.select = "new(" + paging.select + ")";
                        paging.select = HttpUtility.UrlDecode(paging.select);
                        def.data = data.Select(paging.select);
                    }
                    else
                    {
                        List<Lane> lanies = _context.Lanies.Where(e => e.Status != EntityStatus.DELETED).ToList();
                        List<Ward> wards = _context.Wards.Where(e => e.Status != EntityStatus.DELETED).ToList();
                        List<District> districts = _context.Districts.Where(e => e.Status != EntityStatus.DELETED).ToList();
                        List<Province> provincies = _context.Provincies.Where(e => e.Status != EntityStatus.DELETED).ToList();

                        List<Md167ContractData> res = _mapper.Map<List<Md167ContractData>>(data.ToList());
                        foreach (Md167ContractData item in res)
                        {
                            item.Expand = true;
                            item.DelegateName = _context.Md167Delegates.Find(item.DelegateId)?.Name;
                            var house = _context.Md167Houses.Find(item.HouseId);
                            if (house != null)
                            {
                                item.HouseCode = house.Code;
                                item.HouseNumber = house.HouseNumber;
                                if (house.TypeHouse == Md167House.Type_House.House)
                                {
                                    var laneName = lanies.Where(x => x.Id == house.LaneId).Select(x => new NameAndOldName
                                    {
                                        Name = x.Name,
                                        OldName = x.InfoValue
                                    }).FirstOrDefault();
                                    if (laneName == null)
                                    {
                                        item.Lane = "";
                                    }
                                    else
                                    {
                                        if (laneName.OldName.Count() > 0)
                                        {
                                            item.Lane = laneName.Name + "( Tên cũ: " + laneName.OldName[0].Name + " )";
                                        }
                                        else
                                        {
                                            item.Lane = laneName.Name;
                                        }

                                    }
                                    var districtName = districts.Where(x => x.Id == house.DistrictId).Select(x => new NameAndOldName
                                    {
                                        Name = x.Name,
                                        OldName = x.InfoValue
                                    }).FirstOrDefault();
                                    if (districtName == null)
                                    {
                                        item.District = "";
                                    }
                                    else
                                    {
                                        if (districtName.OldName.Count() > 0)
                                        {
                                            item.District = districtName.Name + "( Tên cũ: " + districtName.OldName[0].Name + " )";
                                        }
                                        else
                                        {
                                            item.District = districtName.Name;
                                        }

                                    }
                                    var wardName = wards.Where(x => x.Id == house.WardId).Select(x => new NameAndOldName
                                    {
                                        Name = x.Name,
                                        OldName = x.InfoValue
                                    }).FirstOrDefault();
                                    if (wardName == null)
                                    {
                                        item.Ward = "";
                                    }
                                    else
                                    {
                                        if (wardName.OldName.Count() > 0)
                                        {
                                            item.Ward = wardName.Name + "( Tên cũ: " + wardName.OldName[0].Name + " )";
                                        }
                                        else
                                        {
                                            item.Ward = wardName.Name;
                                        }

                                    }
                                    item.Province = provincies.Where(e => e.Id == house.ProvinceId).Select(e => e.Name).FirstOrDefault();
                                }
                                else
                                {
                                    var houseParent = _context.Md167Houses.Find(house.Md167HouseId);
                                    if (houseParent != null)
                                    {
                                        var laneName = lanies.Where(x => x.Id == houseParent.LaneId).Select(x => new NameAndOldName
                                        {
                                            Name = x.Name,
                                            OldName = x.InfoValue
                                        }).FirstOrDefault();
                                        if (laneName == null)
                                        {
                                            item.Lane = "";
                                        }
                                        else
                                        {
                                            if (laneName.OldName.Count() > 0)
                                            {
                                                item.Lane = laneName.Name + "( Tên cũ: " + laneName.OldName[0].Name + " )";
                                            }
                                            else
                                            {
                                                item.Lane = laneName.Name;
                                            }

                                        }
                                        var districtName = districts.Where(x => x.Id == houseParent.DistrictId).Select(x => new NameAndOldName
                                        {
                                            Name = x.Name,
                                            OldName = x.InfoValue
                                        }).FirstOrDefault();
                                        if (districtName == null)
                                        {
                                            item.District = "";
                                        }
                                        else
                                        {
                                            if (districtName.OldName.Count() > 0)
                                            {
                                                item.District = districtName.Name + "( Tên cũ: " + districtName.OldName[0].Name + " )";
                                            }
                                            else
                                            {
                                                item.District = districtName.Name;
                                            }

                                        }
                                        var wardName = wards.Where(x => x.Id == houseParent.WardId).Select(x => new NameAndOldName
                                        {
                                            Name = x.Name,
                                            OldName = x.InfoValue
                                        }).FirstOrDefault();
                                        if (wardName == null)
                                        {
                                            item.Ward = "";
                                        }
                                        else
                                        {
                                            if (wardName.OldName.Count() > 0)
                                            {
                                                item.Ward = wardName.Name + "( Tên cũ: " + wardName.OldName[0].Name + " )";
                                            }
                                            else
                                            {
                                                item.Ward = wardName.Name;
                                            }

                                        }
                                        item.HouseNumberPar = houseParent.HouseNumber;
                                        item.isKios = true;
                                        item.Province = provincies.Where(e => e.Id == houseParent.ProvinceId).Select(e => e.Name).FirstOrDefault();
                                    }
                                }

                                item.UseFloorPr = house.InfoValue.UseFloorPr;
                            }

                            item.personOrCompany = _context.Md167Delegates.Where(l => l.Id == item.DelegateId).Select(l => l.PersonOrCompany).FirstOrDefault() == personOrCompany.PERSON;
                            item.pricePerMonths = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == item.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                            item.valuations = _context.Md167Valuations.Where(l => l.Md167ContractId == item.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                            item.auctionDecisions = _context.Md167AuctionDecisions.Where(l => l.Md167ContractId == item.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();

                            //if(item.Type == Contract167Type.EXTRA)
                            //{
                            //    item.parent = _context.Md167Contracts.Find(item.ParentId);
                            //}

                            List<Md167Contract> extraData = _context.Md167Contracts.Where(c => c.ParentId == item.Id && c.Type == Contract167Type.EXTRA && c.Status != EntityStatus.DELETED).ToList();
                            List<Md167ContractData> mapExtraData = _mapper.Map<List<Md167ContractData>>(extraData);
                            foreach (Md167ContractData childItem in mapExtraData)
                            {
                                childItem.DelegateName = _context.Md167Delegates.Find(childItem.DelegateId)?.Name;
                                var childHouse = _context.Md167Houses.Find(childItem.HouseId);
                                if (childHouse != null)
                                {
                                    childItem.HouseCode = childHouse.Code;
                                    childItem.HouseNumber = childHouse.HouseNumber;
                                    if (childHouse.TypeHouse == Md167House.Type_House.House)
                                    {
                                        childItem.Lane = lanies.Where(e => e.Id == childHouse.LaneId).Select(e => e.Name).FirstOrDefault();
                                        childItem.Ward = wards.Where(e => e.Id == childHouse.WardId).Select(e => e.Name).FirstOrDefault();
                                        childItem.District = districts.Where(e => e.Id == childHouse.DistrictId).Select(e => e.Name).FirstOrDefault();
                                        childItem.Province = provincies.Where(e => e.Id == childHouse.ProvinceId).Select(e => e.Name).FirstOrDefault();
                                    }
                                    else
                                    {
                                        var childHouseParent = _context.Md167Houses.Find(childHouse.Md167HouseId);
                                        if (childHouseParent != null)
                                        {
                                            item.HouseNumberPar = childHouseParent.HouseNumber;
                                            item.isKios = true;
                                            item.Lane = lanies.Where(e => e.Id == childHouseParent.LaneId).Select(e => e.Name).FirstOrDefault();
                                            item.Ward = wards.Where(e => e.Id == childHouseParent.WardId).Select(e => e.Name).FirstOrDefault();
                                            item.District = districts.Where(e => e.Id == childHouseParent.DistrictId).Select(e => e.Name).FirstOrDefault();
                                            item.Province = provincies.Where(e => e.Id == childHouseParent.ProvinceId).Select(e => e.Name).FirstOrDefault();
                                        }
                                    }

                                    childItem.UseFloorPr = childHouse.InfoValue.UseFloorPr;
                                }


                                childItem.pricePerMonths = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == childItem.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                                childItem.valuations = _context.Md167Valuations.Where(l => l.Md167ContractId == childItem.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                                childItem.auctionDecisions = _context.Md167AuctionDecisions.Where(l => l.Md167ContractId == childItem.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();

                                childItem.parent = item;
                            }

                            item.extraData = mapExtraData;
                        }

                        def.data = res;

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

        // GET: api/Md167Contract/1
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
                Md167Contract data = await _context.Md167Contracts.FindAsync(id);
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                    return Ok(def);
                }

                Md167ContractData res = _mapper.Map<Md167ContractData>(data);

                //res.Delegate = _context.Md167Delegates.Find(res.DelegateId);
                //res.House = _context.Md167Houses.Find(res.HouseId);

                res.DelegateName = _context.Md167Delegates.Find(res.DelegateId)?.Name;
                var house = _context.Md167Houses.Find(res.HouseId);
                if (house != null)
                {
                    res.HouseCode = house.Code;
                    res.HouseNumber = house.HouseNumber;
                    res.Lane = _context.Lanies.Find(house.LaneId)?.Name;
                    res.Ward = _context.Wards.Find(house.WardId)?.Name;
                    res.District = _context.Districts.Find(house.DistrictId)?.Name;
                    res.Province = _context.Provincies.Find(house.ProvinceId)?.Name;
                    res.UseFloorPr = house.InfoValue.UseFloorPr;
                }

                res.personOrCompany = _context.Md167Delegates.Where(l => l.Id == res.DelegateId).Select(l => l.PersonOrCompany).FirstOrDefault() == personOrCompany.PERSON;
                res.pricePerMonths = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == res.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                res.valuations = _context.Md167Valuations.Where(l => l.Md167ContractId == res.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                res.auctionDecisions = _context.Md167AuctionDecisions.Where(l => l.Md167ContractId == res.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();

                if (res.Type == Contract167Type.EXTRA)
                {
                    res.parent = _context.Md167Contracts.Find(res.ParentId);
                }

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
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

        // POST: api/Md167Contract
        [HttpPost]
        public async Task<IActionResult> Post(Md167ContractData input)
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
                input = (Md167ContractData)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                Md167House md167House = _context.Md167Houses.Where(m => m.Id == input.HouseId && m.Status != EntityStatus.DELETED).FirstOrDefault();
                if (md167House == null)
                {
                    def.meta = new Meta(404, "Không tìm thấy căn nhà/kios của hợp đồng!");
                    return Ok(def);
                }

                //Kiểm tra Mã định danh
                Md167Contract codeExist = _context.Md167Contracts.Where(f => f.Code == input.Code && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                if (codeExist != null)
                {
                    def.meta = new Meta(211, "Số hợp đồng đã tồn tại!");
                    return Ok(def);
                }

                //Kiểm tra chỉ có 1 hợp đồng hiệu lực cho 1 căn nhà
                if (input.ContractStatus == ContractStatus167.CON_HIEU_LUC && input.Type == Contract167Type.MAIN)
                {
                    Md167Contract contractActive = _context.Md167Contracts.Where(e => e.HouseId == input.HouseId && e.ContractStatus == ContractStatus167.CON_HIEU_LUC && e.Status != EntityStatus.DELETED).FirstOrDefault();
                    if (contractActive != null)
                    {
                        def.meta = new Meta(212, "Đang có hợp đồng còn hiệu lực cho căn nhà này!");
                        return Ok(def);
                    }
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.CreatedById = userId;
                    input.CreatedBy = fullName;
                    _context.Md167Contracts.Add(input);

                    //Đổi trạng thái căn nhà thành Đang sử dụng
                    if (md167House.TypeHouse == Md167House.Type_House.Kios)
                        md167House.InfoValue.KiosStatus = Md167House.Kios_Status.DANG_CHO_THUE;
                    else
                        md167House.StatusOfUse = 1;
                    _context.Update(md167House);

                    try
                    {
                        await _context.SaveChangesAsync();

                        //Kiểm tra nếu trường hợp là Phụ lục hợp đồng thì đổi trạng thái hđ cha thành hết hiệu lực và clone phần thanh toán sang
                        if (input.Type == Contract167Type.EXTRA)
                        {
                            Md167Contract parentContract = _context.Md167Contracts.Where(m => m.Id == input.ParentId && m.Status != EntityStatus.DELETED).FirstOrDefault();
                            if (parentContract == null)
                            {
                                transaction.Rollback();
                                def.meta = new Meta(211, "Không tìm thấy hợp đồng cha!");
                                return Ok(def);
                            }
                            parentContract.ContractStatus = ContractStatus167.HET_HIEU_LUC;
                                    //Qfix
                            parentContract.EndDate = DateTime.Now;
                            _context.Update(parentContract);
                            List<Md167Contract> lstContractExtra = _context.Md167Contracts.Where(m => m.ParentId == input.ParentId && m.Id != input.Id && m.Status != EntityStatus.DELETED).ToList();
                            if (parentContract != null)
                            {
                                foreach (var item in lstContractExtra)
                                {
                                    item.ContractStatus = ContractStatus167.HET_HIEU_LUC;
                                    //Qfix
                                    item.EndDate = DateTime.Now;
                                    _context.Update(item);
                                }
                            }

                            //Ds thanh toán
                            List<Md167Receipt> md167Receipts = _context.Md167Receipts.Where(e => e.Md167ContractId == parentContract.Id && e.Status != EntityStatus.DELETED).ToList();
                            if (md167Receipts.Count() > 0)
                            {
                                md167Receipts.ForEach(item => {
                                    item.Id = 0;
                                    item.Md167ContractId = input.Id;
                                });

                                _context.Md167Receipts.AddRange(md167Receipts);
                            }
                        }

                        if (input.pricePerMonths != null)
                        {
                            foreach (var pricePerMonth in input.pricePerMonths)
                            {
                                pricePerMonth.Md167ContractId = input.Id;
                                pricePerMonth.CreatedBy = fullName;
                                pricePerMonth.CreatedById = userId;

                                _context.Md167PricePerMonths.Add(pricePerMonth);
                            }
                        }

                        var kios = _context.Md167Houses.Where(x => x.Id == input.HouseId && x.TypeHouse == Md167House.Type_House.Kios).FirstOrDefault();

                        if (kios != null)
                        {
                            kios.InfoValue.KiosStatus = Md167House.Kios_Status.DANG_CHO_THUE;
                            _context.Update(kios);
                            var house = _context.Md167Houses.Where(x => x.Id == kios.Md167HouseId).FirstOrDefault();
                        }

                        if (input.valuations != null)
                        {
                            foreach (var valuation in input.valuations)
                            {
                                valuation.Md167ContractId = input.Id;
                                valuation.CreatedBy = fullName;
                                valuation.CreatedById = userId;

                                _context.Md167Valuations.Add(valuation);
                            }
                        }

                        if (input.auctionDecisions != null)
                        {
                            foreach (var auctionDecision in input.auctionDecisions)
                            {
                                auctionDecision.Md167ContractId = input.Id;
                                auctionDecision.CreatedBy = fullName;
                                auctionDecision.CreatedById = userId;

                                _context.Md167AuctionDecisions.Add(auctionDecision);
                            }
                        }

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Thêm mới hợp đồng 167 " + input.Code, "Md167Contract", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (input.Id > 0)
                        {
                            transaction.Commit();
                            def.meta = new Meta(200, ApiConstants.MessageResource.ADD_SUCCESS);
                            def.data = input;
                            return Ok(def);
                        }
                        else
                        {
                            transaction.Rollback();
                            def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                            return Ok(def);
                        }
                    }
                    catch (DbUpdateException e)
                    {
                        log.Error("DbUpdateException:" + e);
                        transaction.Rollback();
                        if (Md167ContractExists(input.Id))
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

        // PUT: api/Md167Contract/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Md167ContractData input)
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
                input = (Md167ContractData)UtilsService.TrimStringPropertyTypeObject(input);

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

                Md167Contract codeExist = _context.Md167Contracts.Where(f => f.Id != input.Id && f.Code == input.Code && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                if (codeExist != null)
                {
                    def.meta = new Meta(211, "Số hợp đồng đã tồn tại!");
                    return Ok(def);
                }

                Md167Contract data = await _context.Md167Contracts.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_UPDATE_MESSAGE);
                    return Ok(def);
                }

                Md167House md167House = _context.Md167Houses.Where(m => m.Id == input.HouseId && m.Status != EntityStatus.DELETED).FirstOrDefault();
                if (md167House == null)
                {
                    def.meta = new Meta(404, "Không tìm thấy căn nhà/kios của hợp đồng!");
                    return Ok(def);
                }

                //Kiểm tra hợp đồng còn hiệu lực
                if (input.ContractStatus == ContractStatus167.CON_HIEU_LUC)
                {
                    Md167Contract contractActive = _context.Md167Contracts.Where(e => e.Id != input.Id && e.HouseId == input.HouseId && e.ContractStatus == ContractStatus167.CON_HIEU_LUC && e.Status != EntityStatus.DELETED).FirstOrDefault();
                    if (contractActive != null)
                    {
                        def.meta = new Meta(212, "Đang có hợp đồng còn hiệu lực cho căn nhà này!");
                        return Ok(def);
                    }
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                                    //Qfix
                    // Nếu chuyển sang Hết hiệu lực, lưu ngày kết thúc
                    if (input.ContractStatus == ContractStatus167.HET_HIEU_LUC && data.ContractStatus != ContractStatus167.HET_HIEU_LUC)
                    {
                        input.EndDate = DateTime.Now;
                    }
                    // Nếu chuyển từ Hết hiệu lực sang Còn hiệu lực, xóa ngày kết thúc
                    else if (input.ContractStatus == ContractStatus167.CON_HIEU_LUC && data.ContractStatus == ContractStatus167.HET_HIEU_LUC)
                    {
                        input.EndDate = null;
                    }
                    // Nếu không thay đổi trạng thái, giữ nguyên EndDate
                    else
                    {
                        input.EndDate = data.EndDate;
                    }

                    input.UpdatedAt = DateTime.Now;
                    input.UpdatedById = userId;
                    input.UpdatedBy = fullName;
                    input.CreatedAt = data.CreatedAt;
                    input.CreatedBy = data.CreatedBy;
                    input.CreatedById = data.CreatedById;
                    input.Status = data.Status;
                    _context.Update(input);

                    //Đổi trạng thái căn nhà thành Đang sử dụng
                    if (md167House.TypeHouse == Md167House.Type_House.Kios)
                        md167House.InfoValue.KiosStatus = Md167House.Kios_Status.DANG_CHO_THUE;
                    else
                        md167House.StatusOfUse = 1;
                    _context.Update(md167House);

                    if (input.HouseId != data.HouseId)
                    {
                        //Kiểm tra trạng thái sử dụng của căn nhà trước
                        Md167House prevMd167House = _context.Md167Houses.Where(m => m.Id == data.HouseId && m.Status != EntityStatus.DELETED).FirstOrDefault();
                        if (prevMd167House != null)
                        {
                            Md167Contract prevMd167Contract = _context.Md167Contracts.Where(m => m.HouseId == prevMd167House.Id && m.Id != input.Id && m.Status != EntityStatus.DELETED).FirstOrDefault();
                            if (prevMd167Contract == null)
                            {
                                if (prevMd167House.TypeHouse == Md167House.Type_House.Kios)
                                    prevMd167House.InfoValue.KiosStatus = Md167House.Kios_Status.CHUA_CHO_THUE;
                                else
                                    prevMd167House.StatusOfUse = 3;
                            }
                            else
                            {
                                if (prevMd167House.TypeHouse == Md167House.Type_House.Kios)
                                    prevMd167House.InfoValue.KiosStatus = Md167House.Kios_Status.DANG_CHO_THUE;
                                else
                                    prevMd167House.StatusOfUse = 1;
                            }

                            _context.Update(prevMd167House);
                        }
                    }

                    try
                    {
                        await _context.SaveChangesAsync();

                        //Md167PricePerMonth
                        List<Md167PricePerMonth> pricePerMonths = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).AsNoTracking().ToList();
                        if (input.pricePerMonths != null)
                        {
                            foreach (var pricePerMonth in input.pricePerMonths)
                            {
                                Md167PricePerMonth md167PricePerMonthExist = pricePerMonths.Where(l => l.Id == pricePerMonth.Id).FirstOrDefault();
                                if (md167PricePerMonthExist == null)
                                {
                                    pricePerMonth.Md167ContractId = input.Id;
                                    pricePerMonth.CreatedBy = fullName;
                                    pricePerMonth.CreatedById = userId;

                                    _context.Md167PricePerMonths.Add(pricePerMonth);
                                }
                                else
                                {
                                    pricePerMonth.CreatedAt = md167PricePerMonthExist.CreatedAt;
                                    pricePerMonth.CreatedBy = md167PricePerMonthExist.CreatedBy;
                                    pricePerMonth.CreatedById = md167PricePerMonthExist.CreatedById;
                                    pricePerMonth.Md167ContractId = input.Id;
                                    pricePerMonth.UpdatedBy = fullName;
                                    pricePerMonth.UpdatedById = userId;
                                    pricePerMonth.UpdatedAt = DateTime.Now;

                                    _context.Update(pricePerMonth);
                                    pricePerMonths.Remove(md167PricePerMonthExist);
                                }
                            }
                        }

                        if (pricePerMonths.Count > 0)
                        {
                            pricePerMonths.ForEach(item =>
                            {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = AppEnums.EntityStatus.DELETED;
                            });
                            _context.UpdateRange(pricePerMonths);
                        }

                        //Md167Valuation
                        List<Md167Valuation> valuations = _context.Md167Valuations.Where(l => l.Md167ContractId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).AsNoTracking().ToList();
                        if (input.valuations != null)
                        {
                            foreach (var valuation in input.valuations)
                            {
                                Md167Valuation md167ValuationExist = valuations.Where(l => l.Id == valuation.Id).FirstOrDefault();
                                if (md167ValuationExist == null)
                                {
                                    valuation.Md167ContractId = input.Id;
                                    valuation.CreatedBy = fullName;
                                    valuation.CreatedById = userId;

                                    _context.Md167Valuations.Add(valuation);
                                }
                                else
                                {
                                    valuation.CreatedAt = md167ValuationExist.CreatedAt;
                                    valuation.CreatedBy = md167ValuationExist.CreatedBy;
                                    valuation.CreatedById = md167ValuationExist.CreatedById;
                                    valuation.Md167ContractId = input.Id;
                                    valuation.UpdatedBy = fullName;
                                    valuation.UpdatedById = userId;
                                    valuation.UpdatedAt = DateTime.Now;

                                    _context.Update(valuation);

                                    valuations.Remove(md167ValuationExist);
                                }
                            }
                        }

                        valuations.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(valuations);

                        //Md167AuctionDecision
                        List<Md167AuctionDecision> auctionDecisions = _context.Md167AuctionDecisions.Where(l => l.Md167ContractId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).AsNoTracking().ToList();
                        if (input.auctionDecisions != null)
                        {
                            foreach (var auctionDecision in input.auctionDecisions)
                            {
                                Md167AuctionDecision md167AuctionDecisionExist = auctionDecisions.Where(l => l.Id == auctionDecision.Id).FirstOrDefault();
                                if (md167AuctionDecisionExist == null)
                                {
                                    auctionDecision.Md167ContractId = input.Id;
                                    auctionDecision.CreatedBy = fullName;
                                    auctionDecision.CreatedById = userId;

                                    _context.Md167AuctionDecisions.Add(auctionDecision);
                                }
                                else
                                {
                                    auctionDecision.CreatedAt = md167AuctionDecisionExist.CreatedAt;
                                    auctionDecision.CreatedBy = md167AuctionDecisionExist.CreatedBy;
                                    auctionDecision.CreatedById = md167AuctionDecisionExist.CreatedById;
                                    auctionDecision.Md167ContractId = input.Id;
                                    auctionDecision.UpdatedBy = fullName;
                                    auctionDecision.UpdatedById = userId;
                                    auctionDecision.UpdatedAt = DateTime.Now;

                                    _context.Update(auctionDecision);

                                    auctionDecisions.Remove(md167AuctionDecisionExist);
                                }
                            }
                        }

                        if (auctionDecisions.Count > 0)
                        {
                            auctionDecisions.ForEach(item => {
                                item.UpdatedAt = DateTime.Now;
                                item.UpdatedById = userId;
                                item.UpdatedBy = fullName;
                                item.Status = AppEnums.EntityStatus.DELETED;
                            });
                            _context.UpdateRange(auctionDecisions);
                        }

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Sửa hợp đồng 167 " + input.Code, "Md167Contract", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (input.Id > 0)
                        {
                            transaction.Commit();
                            def.meta = new Meta(200, ApiConstants.MessageResource.UPDATE_SUCCESS);
                            def.data = data;
                            return Ok(def);
                        }
                        else
                        {
                            transaction.Rollback();
                            def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                            return Ok(def);
                        }
                    }
                    catch (DbUpdateException e)
                    {
                        transaction.Rollback();
                        log.Error("DbUpdateException:" + e);
                        if (!Md167ContractExists(data.Id))
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

        //Hoàn tiền thế chân
        [HttpPut("RefundPaidDeposit/{id}")]
        public async Task<IActionResult> RefundPaidDeposit(int id)
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
                Md167Contract data = await _context.Md167Contracts.FindAsync(id);
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_DELETE_MESSAGE);
                    return Ok(def);
                }

                if (data.PaidDeposit != true)
                {
                    def.meta = new Meta(212, "Hợp đồng chưa thanh toán đủ tiền thế chân!");
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    data.UpdatedAt = DateTime.Now;
                    data.UpdatedById = userId;
                    data.UpdatedBy = fullName;
                    data.RefundPaidDeposit = data.RefundPaidDeposit == true ? false : true;
                    _context.Update(data);

                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Cập nhật trạng thái hoàn tiền hợp đồng 167 " + data.Code, "Md167Contract", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.UPDATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (data.Id > 0)
                        {
                            transaction.Commit();
                            def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                            def.data = data;
                            return Ok(def);
                        }
                        else
                        {
                            transaction.Rollback();
                            def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                            def.data = data;
                            return Ok(def);
                        }


                    }
                    catch (DbUpdateException e)
                    {
                        transaction.Rollback();
                        log.Error("DbUpdateException:" + e);
                        if (!Md167ContractExists(data.Id))
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

        // DELETE: api/Md167Contract/1
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
                Md167Contract data = await _context.Md167Contracts.FindAsync(id);
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_DELETE_MESSAGE);
                    return Ok(def);
                }

                //Kiểm tra nếu là hợp đồng thì check nếu nó có phụ lục sẽ không được xóa
                if (data.Type == Contract167Type.MAIN)
                {
                    Md167Contract childMd167Contract = _context.Md167Contracts.Where(m => m.ParentId == data.Id && m.Type == Contract167Type.EXTRA && m.Status != EntityStatus.DELETED).FirstOrDefault();
                    if (childMd167Contract != null)
                    {
                        def.meta = new Meta(212, "Có phụ lục hợp đồng của hợp đồng này không thể xóa!");
                        return Ok(def);
                    }
                }
                else
                {
                    Md167Contract parentMd167Contract = _context.Md167Contracts.Where(m => m.Id == data.ParentId && m.Type == Contract167Type.MAIN && m.Status != EntityStatus.DELETED).FirstOrDefault();
                    if (parentMd167Contract != null)
                    {
                        Md167Contract differentMd167Contract = _context.Md167Contracts.Where(m => m.ParentId == parentMd167Contract.Id && m.Id != data.Id && m.Status != EntityStatus.DELETED).FirstOrDefault();
                        if (differentMd167Contract == null)
                        {
                            parentMd167Contract.ContractStatus = ContractStatus167.CON_HIEU_LUC;
                        }
                        else
                        {
                            parentMd167Contract.ContractStatus = ContractStatus167.HET_HIEU_LUC;
                                    //Qfix
                            parentMd167Contract.EndDate = DateTime.Now;

                        }

                        _context.Update(parentMd167Contract);
                    }
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    data.UpdatedAt = DateTime.Now;
                    data.UpdatedById = userId;
                    data.UpdatedBy = fullName;
                    data.Status = AppEnums.EntityStatus.DELETED;
                    _context.Update(data);

                    Md167House md167House = _context.Md167Houses.Where(m => m.Id == data.HouseId && m.Status != EntityStatus.DELETED).FirstOrDefault();
                    if (md167House != null)
                    {
                        Md167Contract prevMd167Contract = _context.Md167Contracts.Where(m => m.HouseId == md167House.Id && m.Id != data.Id && m.Status != EntityStatus.DELETED).FirstOrDefault();
                        if (prevMd167Contract == null)
                        {
                            if (md167House.TypeHouse == Md167House.Type_House.Kios)
                                md167House.InfoValue.KiosStatus = Md167House.Kios_Status.CHUA_CHO_THUE;
                            else
                                md167House.StatusOfUse = 3;
                        }
                        else
                        {
                            if (md167House.TypeHouse == Md167House.Type_House.Kios)
                                md167House.InfoValue.KiosStatus = Md167House.Kios_Status.DANG_CHO_THUE;
                            else
                                md167House.StatusOfUse = 1;
                        }

                        _context.Update(md167House);
                    }

                    //Xóa Md167PricePerMonth
                    List<Md167PricePerMonth> pricePerMonths = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == data.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if (pricePerMonths.Count > 0)
                    {
                        pricePerMonths.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(pricePerMonths);
                    }

                    //Xóa Md167Valuation
                    List<Md167Valuation> valuations = _context.Md167Valuations.Where(l => l.Md167ContractId == data.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if (valuations.Count > 0)
                    {
                        valuations.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(valuations);
                    }

                    //Xóa Md167AuctionDecision
                    List<Md167AuctionDecision> auctionDecisions = _context.Md167AuctionDecisions.Where(l => l.Md167ContractId == data.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if (auctionDecisions.Count > 0)
                    {
                        auctionDecisions.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(auctionDecisions);
                    }

                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Xóa hợp đồng 167 " + data.Code, "Md167Contract", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.DELETED, userId, fullName);
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
                        if (!Md167ContractExists(data.Id))
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

        //API get danh sách Nhà đất + Kios cho hợp đồng
        [HttpGet("getHouseData")]
        //public async Task<IActionResult> GetHouseData()
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
        //        List<Md167HouseType> md167HouseTypes = _context.Md167HouseTypes.Where(m => m.Status != EntityStatus.DELETED).ToList();
        //        var data = _context.Md167Houses.Where(h => (h.TypeHouse == Md167House.Type_House.House || h.TypeHouse == Md167House.Type_House.Kios) && h.Status != EntityStatus.DELETED).ToList();

        //        List<Province> provinces = _context.Provincies.Where(p => p.Status != EntityStatus.DELETED).ToList();
        //        List<District> districts = _context.Districts.Where(p => p.Status != EntityStatus.DELETED).ToList();
        //        List<Ward> wards = _context.Wards.Where(p => p.Status != EntityStatus.DELETED).ToList();
        //        List<Lane> lanies = _context.Lanies.Where(p => p.Status != EntityStatus.DELETED).ToList();

        //        List<Md167HouseInContractData> res = new List<Md167HouseInContractData>();
        //        foreach (var dataItem in data)
        //        {
        //            Md167House md167House = null;
        //            if (dataItem.TypeHouse == Md167House.Type_House.House)
        //            {
        //                //var isApplied = md167HouseTypes.Where(m => m.Id == dataItem.HouseTypeId).Select(x => x.IsApplied).FirstOrDefault();
        //                //dataItem.Status = isApplied ? dataItem.Status : EntityStatus.DELETED;
        //            }
        //            else
        //            {
        //                md167House = _context.Md167Houses.Where(h => h.Id == dataItem.Md167HouseId && h.Status != EntityStatus.DELETED).FirstOrDefault();
        //                if (md167House != null)
        //                {
        //                    dataItem.ProvinceId = md167House.ProvinceId;
        //                    dataItem.DistrictId = md167House.DistrictId;
        //                    dataItem.WardId = md167House.WardId;
        //                    dataItem.LaneId = md167House.LaneId;
        //                    dataItem.InfoValue = md167House.InfoValue;
        //                }
        //            }

        //            if (dataItem.Status != EntityStatus.DELETED)
        //            {
        //                Md167HouseInContractData md167HouseInContractData = new Md167HouseInContractData();
        //                md167HouseInContractData.Id = dataItem.Id;
        //                md167HouseInContractData.Code = dataItem.Code;
        //                md167HouseInContractData.HouseNumber = dataItem.HouseNumber;
        //                md167HouseInContractData.ProvinceId = dataItem.ProvinceId;
        //                md167HouseInContractData.DistrictId = dataItem.DistrictId;
        //                md167HouseInContractData.WardId = dataItem.WardId;
        //                md167HouseInContractData.LaneId = dataItem.LaneId;
        //                md167HouseInContractData.UseFloorPr = dataItem.InfoValue.UseFloorPr;
        //                md167HouseInContractData.TypeHouse = dataItem.TypeHouse;

        //                string provinceName = provinces.Where(p => p.Id == dataItem.ProvinceId).Select(p => p.Name).FirstOrDefault();
        //                string districtName = districts.Where(p => p.Id == dataItem.DistrictId).Select(p => p.Name).FirstOrDefault();
        //                string wardName = wards.Where(p => p.Id == dataItem.WardId).Select(p => p.Name).FirstOrDefault();
        //                string laneName = lanies.Where(p => p.Id == dataItem.LaneId).Select(p => p.Name).FirstOrDefault();

        //                if (md167House == null)
        //                {
        //                    md167HouseInContractData.Address = $"{dataItem.Code}-{dataItem.HouseNumber}, {laneName}, {wardName}, {districtName}, {provinceName}";
        //                }
        //                else
        //                {
        //                    md167HouseInContractData.ParentHouseNumber = md167House.HouseNumber;
        //                    md167HouseInContractData.Address = $"{dataItem.Code}-{dataItem.HouseNumber}(Số nhà: {md167House.HouseNumber}), {laneName}, {wardName}, {districtName}, {provinceName}";
        //                }

        //                res.Add(md167HouseInContractData);
        //            }
        //        }

        //        def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
        //        def.data = res;
        //        return Ok(def);
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error("GetHouseData Error:" + ex);
        //        def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
        //        return Ok(def);
        //    }
        //}
        //API get danh sách Nhà đất + Kios cho hợp đồng
        //API get danh sách Nhà đất + Kios cho hợp đồng
        public async Task<IActionResult> GetHouseData()
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens
                                    where Convert.ToString(t.AccessToken) == accessToken
                                    select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            DefaultResponse def = new DefaultResponse();

            //check role
            var identity = (ClaimsIdentity)User.Identity;
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey")
                                               .Select(c => c.Value).SingleOrDefault();
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                return Ok(def);
            }

            try
            {
                List<Md167HouseType> md167HouseTypes = _context.Md167HouseTypes
                    .Where(m => m.Status != EntityStatus.DELETED).ToList();

                var data = _context.Md167Houses
                    .Where(h => (h.TypeHouse == Md167House.Type_House.House
                              || h.TypeHouse == Md167House.Type_House.Kios)
                              && h.Status != EntityStatus.DELETED)
                    .ToList();

                List<Province> provinces = _context.Provincies.Where(p => p.Status != EntityStatus.DELETED).ToList();
                List<District> districts = _context.Districts.Where(p => p.Status != EntityStatus.DELETED).ToList();
                List<Ward> wards = _context.Wards.Where(p => p.Status != EntityStatus.DELETED).ToList();
                List<Lane> lanies = _context.Lanies.Where(p => p.Status != EntityStatus.DELETED).ToList();

                List<Md167HouseInContractData> res = new List<Md167HouseInContractData>();

                foreach (var dataItem in data)
                {
                    // ⚠️ Không còn override InfoValue từ House cha nữa
                    // Nếu muốn hiển thị ParentHouseNumber thì vẫn lấy từ house cha thôi
                    Md167House md167House = null;
                    if (dataItem.TypeHouse == Md167House.Type_House.Kios)
                    {
                        md167House = _context.Md167Houses
                            .Where(h => h.Id == dataItem.Md167HouseId && h.Status != EntityStatus.DELETED)
                            .FirstOrDefault();
                    }

                    if (dataItem.Status != EntityStatus.DELETED)
                    {
                        Md167HouseInContractData md167HouseInContractData = new Md167HouseInContractData();
                        md167HouseInContractData.Id = dataItem.Id;
                        md167HouseInContractData.Code = dataItem.Code;
                        md167HouseInContractData.HouseNumber = dataItem.HouseNumber;
                        md167HouseInContractData.ProvinceId = dataItem.ProvinceId;
                        md167HouseInContractData.DistrictId = dataItem.DistrictId;
                        md167HouseInContractData.WardId = dataItem.WardId;
                        md167HouseInContractData.LaneId = dataItem.LaneId;
                        md167HouseInContractData.UseFloorPb = dataItem.InfoValue.UseFloorPb;
                        md167HouseInContractData.UseFloorPr = dataItem.InfoValue.UseFloorPr;
                        md167HouseInContractData.TypeHouse = dataItem.TypeHouse;
                        if(md167HouseInContractData.UseFloorPr != null && md167HouseInContractData.UseFloorPb != null)
                        {
                            md167HouseInContractData.UseFloorPr = md167HouseInContractData.UseFloorPb + md167HouseInContractData.UseFloorPr;
                        }
                        if(md167HouseInContractData.UseFloorPr == null)
                        {
                            md167HouseInContractData.UseFloorPr = md167HouseInContractData.UseFloorPb;
                        }
                        string provinceName = provinces.Where(p => p.Id == dataItem.ProvinceId).Select(p => p.Name).FirstOrDefault();
                        string districtName = districts.Where(p => p.Id == dataItem.DistrictId).Select(p => p.Name).FirstOrDefault();
                        string wardName = wards.Where(p => p.Id == dataItem.WardId).Select(p => p.Name).FirstOrDefault();
                        string laneName = lanies.Where(p => p.Id == dataItem.LaneId).Select(p => p.Name).FirstOrDefault();

                        if (md167House == null)
                        {
                            md167HouseInContractData.Address = $"{dataItem.Code}-{dataItem.HouseNumber}, {laneName}, {wardName}, {districtName}, {provinceName}";
                        }
                        else
                        {
                            md167HouseInContractData.ParentHouseNumber = md167House.HouseNumber;
                            md167HouseInContractData.Address = $"{dataItem.Code}-{dataItem.HouseNumber}(Số nhà: {md167House.HouseNumber}), {laneName}, {wardName}, {districtName}, {provinceName}";
                        }

                        res.Add(md167HouseInContractData);
                    }
                }

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = res;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("GetHouseData Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }



        #region Công nợ
        [HttpGet("GetDataDebt/{id}")]
        public async Task<IActionResult> GetDataDebt(int id)
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
                Md167Contract data = await _context.Md167Contracts.FindAsync(id);
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                    return Ok(def);
                }

                var pricePerMonths = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == data.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                List<Md167ProfitValue> profitValues = _context.Md167ProfitValues.Where(p => p.Status != EntityStatus.DELETED).ToList();
                List<Md167Receipt> md167Receipts = _context.Md167Receipts.Where(m => m.Md167ContractId == data.Id && m.PaidDeposit != true && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();

                List<GroupMd167DebtData> res = null;
                if (data.Type == Contract167Type.MAIN)
                {
                    //Ds phiếu thu thanh toán tiền thế chân
                    List<Md167Receipt> depositMd167Receipts = _context.Md167Receipts.Where(m => m.Md167ContractId == data.Id && m.PaidDeposit == true && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();

                    //Tìm hợp đồng liên quan để lấy tiền thế chân
                    Md167Contract dataRelated = await _context.Md167Contracts.Where(x => x.DelegateId == data.DelegateId && x.HouseId == data.HouseId && x.RefundPaidDeposit != true && x.Status != EntityStatus.DELETED).FirstOrDefaultAsync();
                    if (dataRelated != null)
                    {
                        var pricePerMonth = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == dataRelated.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).FirstOrDefault();
                        res = GetDataDebtFunc(pricePerMonths, profitValues, md167Receipts, data, pricePerMonth?.TotalPrice * 3, depositMd167Receipts);
                    }
                    else
                    {
                        var pricePerMonth = pricePerMonths.Count > 0 ? pricePerMonths[pricePerMonths.Count - 1] : null;
                        res = GetDataDebtFunc(pricePerMonths, profitValues, md167Receipts, data, pricePerMonth?.TotalPrice * 3, depositMd167Receipts);
                    }
                }
                else
                {
                    Md167Contract parentData = await _context.Md167Contracts.FindAsync(data.ParentId);
                    if (parentData == null)
                    {
                        def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                        return Ok(def);
                    }

                    var parentPricePerMonths = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == parentData.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                    List<Md167Receipt> parentMd167Receipts = _context.Md167Receipts.Where(m => m.Md167ContractId == parentData.Id && m.PaidDeposit != true && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();
                    List<Md167Receipt> depositMd167Receipts = _context.Md167Receipts.Where(m => m.Md167ContractId == data.Id && m.PaidDeposit == true && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();

                    res = GetDataExtraDebtFunc(pricePerMonths, profitValues, md167Receipts, data, parentPricePerMonths, parentMd167Receipts, parentData, depositMd167Receipts);
                }

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
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

        //Tính công nợ của hợp đồng
        public static List<GroupMd167DebtData> GetDataDebtFunc(List<Md167PricePerMonth> pricePerMonths, List<Md167ProfitValue> profitValues, List<Md167Receipt> md167Receipts, Md167Contract md167Contract, decimal? deposit, List<Md167Receipt> depositMd167Receipts)
        {
            List<Md167Debt> res = new List<Md167Debt>();


            if (md167Contract != null)
            {
                var pricePerMonth = pricePerMonths.Count > 0 ? pricePerMonths[pricePerMonths.Count - 1] : null;

                //Ngày bắt đầu thuê
                var dateStart = md167Contract.DateSign;
                //Qfix
                //Ngày kết thúc: nếu hợp đồng hết hiệu lực thì dừng tại ngày hết hiệu lực, ngược lại đến hiện tại
                var dateEnd = DateTime.Now;
                if (md167Contract.ContractStatus == ContractStatus167.HET_HIEU_LUC)
                {
                    // Ưu tiên ngày hết hiệu lực (EndDate) nếu có
                    if (md167Contract.EndDate.HasValue)
                    {
                        dateEnd = md167Contract.EndDate.Value;
                    }
                }
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
            response.Add(endMd167DebtData);

            return response;
        }

        //Tính công nợ của phụ lục hợp đồng
        public static List<GroupMd167DebtData> GetDataExtraDebtFunc(List<Md167PricePerMonth> _pricePerMonths, List<Md167ProfitValue> profitValues, List<Md167Receipt> _md167Receipts, Md167Contract _md167Contract, List<Md167PricePerMonth> parentPricePerMonths, List<Md167Receipt> parentMd167Receipts, Md167Contract parentMd167Contract, List<Md167Receipt> depositMd167Receipts)
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
            //firstMd167Debt.AmountPaidInPeriod = firstMd167Debt.AmountPaidPerMonth;
            //firstMd167Debt.AmountPaid = firstMd167Debt.AmountPaidInPeriod;
            //firstMd167Debt.AmountDiff = null;
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
                            firstMd167Debt.AmountPaidInPeriod = firstMd167Debt.AmountPaidPerMonth;
                            firstMd167Debt.AmountPaid = firstMd167Debt.AmountPaidInPeriod;
                            firstMd167Debt.AmountDiff = null;

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
            response.Add(endMd167DebtData);

            return response;
        }

        //Xuất file excel công nợ
        [HttpPost("ExportReport/{id}")]
        public async Task<IActionResult> ExportReport(int id)
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

            DefaultResponse def = new DefaultResponse();
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.EXPORT))
            {
                return new FileContentResult(new byte[0], "application/octet-stream");
            }

            Md167Contract data = await _context.Md167Contracts.FindAsync(id);
            if (data == null)
            {
                return new FileContentResult(new byte[0], "application/octet-stream");
            }

            XSSFWorkbook wb = new XSSFWorkbook();
            ISheet sheet = wb.CreateSheet();

            string template = @"MD167/bang_chiet_tinh.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);
            string nameExcel = "bang_chiet_tinh" + data.Code + ".xlsx";



            var pricePerMonths = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == data.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
            List<Md167ProfitValue> profitValues = _context.Md167ProfitValues.Where(p => p.Status != EntityStatus.DELETED).ToList();
            List<Md167Receipt> md167Receipts = _context.Md167Receipts.Where(m => m.Md167ContractId == data.Id && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();

            List<GroupMd167DebtData> res = null;
            if (data.Type == Contract167Type.MAIN)
            {
                //Ds phiếu thu thanh toán tiền thế chân
                List<Md167Receipt> depositMd167Receipts = _context.Md167Receipts.Where(m => m.Md167ContractId == data.Id && m.PaidDeposit == true && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();

                //Tìm hợp đồng liên quan để lấy tiền thế chân
                Md167Contract dataRelated = await _context.Md167Contracts.Where(x => x.DelegateId == data.DelegateId && x.HouseId == data.HouseId && x.RefundPaidDeposit != true && x.Status != EntityStatus.DELETED).FirstOrDefaultAsync();
                if (dataRelated != null)
                {
                    var pricePerMonth = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == dataRelated.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).FirstOrDefault();
                    res = GetDataDebtFunc(pricePerMonths, profitValues, md167Receipts, data, pricePerMonth?.TotalPrice * 3, depositMd167Receipts);
                }
                else
                {
                    var pricePerMonth = pricePerMonths.Count > 0 ? pricePerMonths[pricePerMonths.Count - 1] : null;
                    res = GetDataDebtFunc(pricePerMonths, profitValues, md167Receipts, data, pricePerMonth?.TotalPrice * 3, depositMd167Receipts);
                }
            }
            else
            {
                Md167Contract parentData = await _context.Md167Contracts.FindAsync(data.ParentId);
                if (parentData == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                    return Ok(def);
                }

                var parentPricePerMonths = _context.Md167PricePerMonths.Where(l => l.Md167ContractId == parentData.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                List<Md167Receipt> parentMd167Receipts = _context.Md167Receipts.Where(m => m.Md167ContractId == parentData.Id && m.PaidDeposit != true && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();
                List<Md167Receipt> depositMd167Receipts = _context.Md167Receipts.Where(m => m.Md167ContractId == data.Id && m.PaidDeposit == true && m.Status != EntityStatus.DELETED).OrderBy(x => x.DateOfReceipt).ToList();

                res = GetDataExtraDebtFunc(pricePerMonths, profitValues, md167Receipts, data, parentPricePerMonths, parentMd167Receipts, parentData, depositMd167Receipts);
            }

            string delegateName = _context.Md167Delegates.Find(data.DelegateId)?.Name;

            string houseInfo = "";
            Md167House md167House = _context.Md167Houses.Where(m => m.Id == data.HouseId).FirstOrDefault();
            if (md167House != null)
            {
                Province province = _context.Provincies.Find(md167House.ProvinceId);
                District district = _context.Districts.Find(md167House.DistrictId);
                Ward ward = _context.Wards.Find(md167House.WardId);
                Lane lane = _context.Lanies.Find(md167House.LaneId);

                houseInfo = md167House.Code + ", " + md167House.HouseNumber + (lane != null ? ", " + lane.Name : "") + (ward != null ? ", " + ward.Name : "") + (district != null ? ", " + district.Name : "") + (province != null ? ", " + province.Name : "");
            }

            MemoryStream ms = WriteDataToExcel(templatePath, 0, res, data, delegateName, houseInfo);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", nameExcel);
        }

        private static MemoryStream WriteDataToExcel(string templatePath, int sheetnumber, List<GroupMd167DebtData> data, Md167Contract md167Contract, string DelegateName, string house)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowStart = 11;

            if (sheet != null)
            {
                sheet.GetRow(1).GetCell(0).SetCellValue("Đính kèm theo hợp đồng số: " + md167Contract.Code + "........ Ký ngày: " + md167Contract.DateSign.ToString("dd/MM/yyyy"));
                sheet.GetRow(3).GetCell(3).SetCellValue(DelegateName);
                sheet.GetRow(4).GetCell(3).SetCellValue(house);
                sheet.GetRow(5).GetCell(3).SetCellValue((double)data[0].dataGroup[0].AmountPaid);
                sheet.GetRow(6).GetCell(3).SetCellValue((double)data[0].dataGroup[0].AmountPaid / 3);
                sheet.GetRow(7).GetCell(3).SetCellValue(md167Contract.DateGroundHandover.ToString("dd/MM/yyyy"));

                int datacol = 13;
                try
                {
                    //get style row
                    List<ICellStyle> rowStyle = new List<ICellStyle>();
                    for (int i = 0; i < datacol; i++)
                    {
                        rowStyle.Add(sheet.GetRow(rowStart).GetCell(i).CellStyle);
                    }

                    foreach (var dataItem in data)
                    {
                        if (rowStart == 11)
                        {
                            foreach (var item in dataItem.dataGroup)
                            {
                                XSSFRow row = (XSSFRow)sheet.CreateRow(rowStart);
                                for (int i = 0; i < datacol; i++)
                                {
                                    row.CreateCell(i).CellStyle = rowStyle[i];

                                    if (i == 0)
                                    {
                                        if (item.TypeRow == Md167DebtTypeRow.DONG_THE_CHAN || item.TypeRow == Md167DebtTypeRow.DONG_NAM || item.TypeRow == Md167DebtTypeRow.DONG_TONG)
                                        {
                                            CellRangeAddress cellRangeAddress = new CellRangeAddress(rowStart, rowStart, 0, 1);
                                            sheet.AddMergedRegion(cellRangeAddress);
                                            row.GetCell(i).SetCellValue(item.Title);
                                        }
                                        else
                                        {
                                            if (item.Index != null)
                                                row.GetCell(i).SetCellValue(item.Index.ToString());
                                        }
                                    }
                                    else if (i == 1)
                                    {
                                        if (item.Dop != null)
                                        {
                                            row.GetCell(i).SetCellValue(item.Dop.Value.ToString("dd/MM/yyyy"));
                                        }
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
                                        if (item.AmountPaidPerMonth != null)
                                            row.GetCell(i).SetCellValue((double)item.AmountPaidPerMonth);
                                    }
                                    else if (i == 7)
                                    {

                                    }
                                    else if (i == 8)
                                    {
                                        if (item.AmountPaidInPeriod != null)
                                            row.GetCell(i).SetCellValue((double)item.AmountPaidInPeriod);
                                    }
                                    else if (i == 9)
                                    {
                                        if (item.AmountToBePaid != null)
                                            row.GetCell(i).SetCellValue((double)item.AmountToBePaid);
                                    }
                                    else if (i == 10)
                                    {
                                        if (item.AmountPaid != null)
                                            row.GetCell(i).SetCellValue((double)item.AmountPaid);
                                    }
                                    else if (i == 11)
                                    {

                                    }
                                    else if (i == 12)
                                    {

                                    }
                                }

                                rowStart++;
                            }

                        }
                        else
                        {
                            int leng_item = 1;
                            foreach (var item in dataItem.dataGroup)
                            {
                                XSSFRow row = (XSSFRow)sheet.CreateRow(rowStart);
                                for (int i = 0; i < datacol; i++)
                                {
                                    row.CreateCell(i).CellStyle = rowStyle[i];

                                    if (i == 0)
                                    {
                                        if (item.TypeRow == Md167DebtTypeRow.DONG_THE_CHAN || item.TypeRow == Md167DebtTypeRow.DONG_NAM || item.TypeRow == Md167DebtTypeRow.DONG_TONG)
                                        {
                                            CellRangeAddress cellRangeAddress = new CellRangeAddress(rowStart, rowStart, 0, 1);
                                            sheet.AddMergedRegion(cellRangeAddress);
                                            row.GetCell(i).SetCellValue(item.Title);
                                        }
                                        else
                                        {
                                            if (item.Index != null)
                                                row.GetCell(i).SetCellValue(item.Index.ToString());
                                        }
                                    }
                                    else if (i == 1)
                                    {
                                        if (item.TypeRow == Md167DebtTypeRow.DONG_NO_CU)
                                        {
                                            row.GetCell(i).SetCellValue("Nợ cũ");
                                        }
                                        else if (item.TypeRow != Md167DebtTypeRow.DONG_TONG && item.Dop != null)
                                        {
                                            row.GetCell(i).SetCellValue(item.Dop.Value.ToString("dd/MM/yyyy"));
                                        }
                                    }
                                    else if (i == 2)
                                    {
                                        if (item.DopExpected != null)
                                        {
                                            row.GetCell(i).SetCellValue(item.DopExpected.Value.ToString("dd/MM/yyyy"));
                                        }
                                    }
                                    else if (i == 3)
                                    {
                                        if (item.DopActual != null)
                                        {
                                            row.GetCell(i).SetCellValue(item.DopActual.Value.ToString("dd/MM/yyyy"));
                                        }
                                    }
                                    else if (i == 4)
                                    {
                                        if (item.InterestCalcDate != null)
                                        {
                                            row.GetCell(i).SetCellValue((double)item.InterestCalcDate);
                                        }
                                    }
                                    else if (i == 5)
                                    {
                                        if (item.Interest != null)
                                        {
                                            row.GetCell(i).SetCellValue((double)item.Interest);
                                        }
                                    }
                                    else if (i == 6)
                                    {
                                        if (item.AmountPaidPerMonth != null)
                                            row.GetCell(i).SetCellValue((double)item.AmountPaidPerMonth);
                                    }
                                    else if (i == 7)
                                    {
                                        if (item.AmountInterest != null)
                                            row.GetCell(i).SetCellValue((double)item.AmountInterest);
                                    }
                                    else if (i == 8)
                                    {
                                        if (item.AmountPaidInPeriod != null)
                                            row.GetCell(i).SetCellValue((double)item.AmountPaidInPeriod);
                                    }
                                    else if (i == 9)
                                    {
                                        if (leng_item == dataItem.length)
                                        {
                                            if (item.TypeRow != Md167DebtTypeRow.DONG_TONG && leng_item > 1)
                                            {
                                                CellRangeAddress cellRangeAddress = new CellRangeAddress(rowStart - leng_item + 1, rowStart, i, i);
                                                sheet.AddMergedRegion(cellRangeAddress);

                                                if (dataItem.AmountToBePaid != null)
                                                    sheet.GetRow(rowStart - leng_item + 1).GetCell(i).SetCellValue((double)dataItem.AmountToBePaid);
                                            }
                                            else
                                            {
                                                if (item.AmountToBePaid != null)
                                                    row.GetCell(i).SetCellValue((double)item.AmountToBePaid);
                                            }
                                        }
                                    }
                                    else if (i == 10)
                                    {
                                        if (leng_item == dataItem.length)
                                        {
                                            if (item.TypeRow != Md167DebtTypeRow.DONG_TONG && leng_item > 1)
                                            {
                                                CellRangeAddress cellRangeAddress = new CellRangeAddress(rowStart - leng_item + 1, rowStart, i, i);
                                                sheet.AddMergedRegion(cellRangeAddress);

                                                if (dataItem.AmountPaid != null)
                                                    sheet.GetRow(rowStart - leng_item + 1).GetCell(i).SetCellValue((double)dataItem.AmountPaid);
                                            }
                                            else
                                            {
                                                if (item.AmountPaid != null)
                                                    row.GetCell(i).SetCellValue((double)item.AmountPaid);
                                            }
                                        }
                                    }
                                    else if (i == 11)
                                    {
                                        if (leng_item == dataItem.length)
                                        {
                                            if (item.TypeRow != Md167DebtTypeRow.DONG_TONG && leng_item > 1)
                                            {
                                                CellRangeAddress cellRangeAddress = new CellRangeAddress(rowStart - leng_item + 1, rowStart, i, i);
                                                sheet.AddMergedRegion(cellRangeAddress);

                                                if (dataItem.AmountDiff != null)
                                                    sheet.GetRow(rowStart - leng_item + 1).GetCell(i).SetCellValue((double)dataItem.AmountDiff);
                                            }
                                            else
                                            {
                                                if (item.AmountDiff != null)
                                                    row.GetCell(i).SetCellValue((double)item.AmountDiff);
                                            }
                                        }
                                    }
                                    else if (i == 12)
                                    {
                                        if (leng_item == dataItem.length)
                                        {
                                            if (item.TypeRow != Md167DebtTypeRow.DONG_TONG && leng_item > 1)
                                            {
                                                CellRangeAddress cellRangeAddress = new CellRangeAddress(rowStart - leng_item + 1, rowStart, i, i);
                                                sheet.AddMergedRegion(cellRangeAddress);

                                                if (dataItem.Note != null)
                                                    sheet.GetRow(rowStart - leng_item + 1).GetCell(i).SetCellValue(dataItem.Note);
                                            }
                                            else
                                            {
                                                if (item.Note != null)
                                                    row.GetCell(i).SetCellValue(item.Note);
                                            }
                                        }
                                    }
                                }

                                leng_item++;
                                rowStart++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("WriteDataToExcel:" + ex);
                }
            }

            sheet.ForceFormulaRecalculation = true;

            MemoryStream ms = new MemoryStream();

            workbook.Write(ms);

            return ms;

        }


        #endregion

        #region import danh sách hợp đồng từ excel
        [HttpPost]
        [Route("ImportDataExcel/{importHistoryType}")]
        public IActionResult ImportDataExcel(ImportHistoryType importHistoryType)
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
                int idx = 0;
                var httpRequest = Request.Form.Files;
                ImportHistory importHistory = new ImportHistory();
                importHistory.Type = importHistoryType;

                List<Md167ExtraContractImportData> data = new List<Md167ExtraContractImportData>();

                //Lấy dữ liệu từ file excel và lưu lại file
                foreach (var file in httpRequest)
                {
                    var postedFile = httpRequest[idx];
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
                                    data = importData(ms, 0, 1, importHistoryType);
                                }
                            }
                        }
                    }
                    idx++;
                }

                List<Md167Contract> dataValid = new List<Md167Contract>();
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        int count = data.Count;
                        for (int i = 0; i < count; i++)
                        {
                            if (data[i].Valid == true)
                            {
                                Md167Contract md167Contract = new Md167Contract();

                                //Kiểm tra thông tin người thuê của hợp đồng: Nếu là cá nhân thì check theo Sdt, còn tổ chức thì check theo Mã số thuế
                                if (data[i].IsPersonal == true)
                                {
                                    Md167Delegate md167Delegate = _context.Md167Delegates.Where(m => m.PhoneNumber == data[i].Phone && m.PersonOrCompany == personOrCompany.PERSON && m.Status != EntityStatus.DELETED).FirstOrDefault();
                                    if (md167Delegate != null) md167Contract.DelegateId = md167Delegate.Id;
                                    else
                                    {
                                        Md167Delegate md167 = new Md167Delegate();
                                        md167.PersonOrCompany = personOrCompany.PERSON;
                                        md167.Name = data[i].FullName;
                                        md167.NationalId = data[i].IdentityCode;
                                        md167.DateOfIssue = data[i].IdentityDate;
                                        md167.PlaceOfIssue = data[i].IdentityPlace;
                                        md167.Address = data[i].IdentityPlace;
                                        md167.PhoneNumber = data[i].Phone;
                                        md167.CreatedById = -1;
                                        md167.CreatedBy = fullName;

                                        _context.Md167Delegates.Add(md167);
                                        _context.SaveChanges();

                                        md167Contract.DelegateId = md167.Id;
                                    }
                                }
                                else
                                {
                                    Md167Delegate md167Delegate = _context.Md167Delegates.Where(m => m.ComTaxNumber == data[i].TaxCode && m.PersonOrCompany == personOrCompany.COMPANY && m.Status != EntityStatus.DELETED).FirstOrDefault();
                                    if (md167Delegate != null) md167Contract.DelegateId = md167Delegate.Id;
                                    else
                                    {
                                        Md167Delegate md167 = new Md167Delegate();
                                        md167.PersonOrCompany = personOrCompany.COMPANY;
                                        md167.Name = data[i].OrganizationName;
                                        md167.ComTaxNumber = data[i].TaxCode;
                                        md167.ComBusinessLicense = data[i].BusinessLicense;
                                        md167.ComOrganizationRepresentativeName = data[i].FullName;
                                        md167.ComPosition = data[i].Position;
                                        md167.NationalId = data[i].IdentityCode;
                                        md167.DateOfIssue = data[i].IdentityDate;
                                        md167.PlaceOfIssue = data[i].IdentityPlace;
                                        md167.Address = data[i].IdentityPlace;
                                        md167.PhoneNumber = data[i].Phone;
                                        md167.CreatedById = -1;
                                        md167.CreatedBy = fullName;
                                        md167.Code = "TCCN";

                                        _context.Md167Delegates.Add(md167);
                                        _context.SaveChanges();

                                        md167.Code = CodeIndentity.CodeInd("TCCN", md167.Id, 5);

                                        _context.Update(md167);

                                        md167Contract.DelegateId = md167.Id;
                                    }
                                }

                                //Kiểm tra Mã nhà/Kios
                                if (data[i].KiosNumber != null)
                                {
                                    Md167House md167House = _context.Md167Houses.Where(m => m.TypeHouse == Md167House.Type_House.Apartment && m.HouseNumber == data[i].HouseNumber && m.Status != EntityStatus.DELETED).FirstOrDefault();
                                    if (md167House == null)
                                    {
                                        data[i].Valid = false;
                                        data[i].ErrMsg += "Không tìm thấy Số nhà\n";
                                        continue;
                                    }

                                    Md167House kios = _context.Md167Houses.Where(m => m.TypeHouse == Md167House.Type_House.Kios && m.HouseNumber == data[i].KiosNumber && m.Md167HouseId == md167House.Id && m.Status != EntityStatus.DELETED).FirstOrDefault();
                                    if (kios == null)
                                    {
                                        data[i].Valid = false;
                                        data[i].ErrMsg += "Không tìm thấy Tên Kios\n";
                                        continue;
                                    }
                                    else
                                    {
                                        md167Contract.HouseId = kios.Id;
                                    }
                                }
                                else
                                {
                                    Md167House md167House = _context.Md167Houses.Where(m => m.HouseNumber == data[i].HouseNumber && m.TypeHouse == Md167House.Type_House.House && m.Status != EntityStatus.DELETED).FirstOrDefault();
                                    if (md167House == null)
                                    {
                                        data[i].Valid = false;
                                        data[i].ErrMsg += "Không tìm thấy Số nhà\n";
                                        continue;
                                    }
                                    else
                                    {
                                        md167Contract.HouseId = md167House.Id;
                                    }
                                }

                                //Check loại Giá niêm yết/Đấu giá
                                string type_price = UtilsService.NonUnicode(data[i].TypePrice);
                                if (type_price == "gia-niem-yet")
                                {
                                    md167Contract.TypePrice = TypePriceContract167.GIA_NIEM_YET;
                                }
                                else if (type_price == "dau-gia")
                                {
                                    md167Contract.TypePrice = TypePriceContract167.DAU_GIA;
                                }
                                else
                                {
                                    data[i].Valid = false;
                                    data[i].ErrMsg += "Cột Giá niêm yết/Đấu giá không hợp lệ\n";
                                    continue;
                                }


                                //Check thông tin Thời gian thuê
                                string rentalPeriod = UtilsService.NonUnicode(data[i].RentalPeriod);
                                if (rentalPeriod == "tam-bo-tri")
                                {
                                    md167Contract.RentalPeriod = RentalPeriodContract167.TAM_BO_TRI;
                                }
                                else if (rentalPeriod == "thue-1-nam" || rentalPeriod == "thue-mot-nam")
                                {
                                    md167Contract.RentalPeriod = RentalPeriodContract167.THUE_1_NAM;
                                }
                                else if (rentalPeriod == "thue-5-nam" || rentalPeriod == "thue-nam-nam")
                                {
                                    md167Contract.RentalPeriod = RentalPeriodContract167.THUE_5_NAM;
                                }
                                else if (rentalPeriod == "khac")
                                {
                                    md167Contract.RentalPeriod = RentalPeriodContract167.KHAC;
                                    if (data[i].NoteRentalPeriod != null)
                                    {
                                        md167Contract.NoteRentalPeriod = data[i].NoteRentalPeriod;
                                    }
                                    else
                                    {
                                        data[i].Valid = false;
                                        data[i].ErrMsg += "Cột Thời gian thuê - khác không có dữ liệu\n";
                                        continue;
                                    }
                                }
                                else
                                {
                                    data[i].Valid = false;
                                    data[i].ErrMsg += "Cột Thời gian thuê không hợp lệ\n";
                                    continue;
                                }

                                //Check mục đích thuê
                                string rentalPurpose = UtilsService.NonUnicode(data[i].RentalPurpose);
                                if (rentalPurpose == "kinh-doanh-dich-vu" || rentalPurpose == "kddv")
                                {
                                    md167Contract.RentalPurpose = RentalPurposeContract167.KINH_DOANH_DV;
                                }
                                else if (rentalPurpose == "co-so-san-xuat" || rentalPurpose == "cssx")
                                {
                                    md167Contract.RentalPurpose = RentalPurposeContract167.CO_SO_SX;
                                }
                                else if (rentalPurpose == "kho-bai" || rentalPurpose == "kb")
                                {
                                    md167Contract.RentalPurpose = RentalPurposeContract167.KHO_BAI;
                                }
                                else if (rentalPurpose == "khac")
                                {
                                    md167Contract.RentalPurpose = RentalPurposeContract167.KHAC;
                                    if (data[i].NoteRentalPurpose != null)
                                    {
                                        md167Contract.NoteRentalPurpose = data[i].NoteRentalPurpose;
                                    }
                                    else
                                    {
                                        data[i].Valid = false;
                                        data[i].ErrMsg += "Cột Mục đích thuê - khác không có dữ liệu\n";
                                        continue;
                                    }
                                }
                                else
                                {
                                    data[i].Valid = false;
                                    data[i].ErrMsg += "Cột Mục đích thuê không hợp lệ\n";
                                    continue;
                                }

                                //Check thông tin kỳ thanh toán
                                string paymentPeriod = UtilsService.NonUnicode(data[i].PaymentPeriod);
                                if (paymentPeriod == "thang")
                                {
                                    md167Contract.PaymentPeriod = PaymentPeriodContract167.THANG;
                                }
                                else if (paymentPeriod == "quy")
                                {
                                    md167Contract.PaymentPeriod = PaymentPeriodContract167.QUY;
                                }
                                else
                                {
                                    data[i].Valid = false;
                                    data[i].ErrMsg += "Cột Kỳ thanh toán không hợp lệ\n";
                                    continue;
                                }

                                //Check thông tin Trạng thái hợp đồng
                                string contractStatus = UtilsService.NonUnicode(data[i].ContractStatus);
                                if (contractStatus == "con-hieu-luc")
                                {
                                    md167Contract.ContractStatus = ContractStatus167.CON_HIEU_LUC;
                                }
                                else if (contractStatus == "het-hieu-luc")
                                {
                                    md167Contract.ContractStatus = ContractStatus167.HET_HIEU_LUC;
                                    //Qfix
                                    md167Contract.EndDate = DateTime.Now;
                                }
                                else
                                {
                                    data[i].Valid = false;
                                    data[i].ErrMsg += "Cột Trạng thái hợp đồng không hợp lệ\n";
                                    continue;
                                }

                                if (importHistoryType == ImportHistoryType.Md167MainContract)
                                {
                                    //Kiểm tra mã hợp đồng đã tồn tại chưa
                                    Md167Contract codeExist = _context.Md167Contracts.Where(m => m.Code == data[i].Code && m.Status != EntityStatus.DELETED).FirstOrDefault();
                                    if (codeExist != null)
                                    {
                                        data[i].Valid = false;
                                        data[i].ErrMsg += "Số hợp đồng đã tồn tại\n";
                                        continue;
                                    }
                                }
                                else
                                {
                                    //Kiểm tra mã hợp đồng đã tồn tại chưa
                                    Md167Contract mainExist = _context.Md167Contracts.Where(m => m.Code == data[i].Code && m.Status != EntityStatus.DELETED).FirstOrDefault();
                                    if (mainExist == null)
                                    {
                                        data[i].Valid = false;
                                        data[i].ErrMsg += "Không tìm thấy Số hợp đồng\n";
                                        continue;
                                    }
                                    else
                                    {
                                        md167Contract.ParentId = mainExist.Id;
                                    }

                                    Md167Contract codeExist = _context.Md167Contracts.Where(m => m.Code == data[i].ExtraCode && m.Status != EntityStatus.DELETED).FirstOrDefault();
                                    if (codeExist != null)
                                    {
                                        data[i].Valid = false;
                                        data[i].ErrMsg += "Số phụ lục đã tồn tại\n";
                                        continue;
                                    }
                                }

                                md167Contract.Code = importHistoryType == ImportHistoryType.Md167MainContract ? data[i].Code : data[i].ExtraCode;
                                md167Contract.DateSign = importHistoryType == ImportHistoryType.Md167MainContract ? (DateTime)data[i].DateSign : (DateTime)data[i].ExtraDateSign;
                                md167Contract.DateGroundHandover = (DateTime)data[i].DateGroundHandover;
                                md167Contract.Type = importHistoryType == ImportHistoryType.Md167MainContract ? Contract167Type.MAIN : Contract167Type.EXTRA;
                                md167Contract.CreatedById = -1;
                                md167Contract.CreatedBy = fullName;

                                dataValid.Add(md167Contract);
                            }
                        }

                        importHistory.Data = data.Cast<dynamic>().ToList();
                        importHistory.CreatedById = userId;
                        importHistory.CreatedBy = fullName;

                        _context.ImportHistories.Add(importHistory);
                        _context.Md167Contracts.AddRange(dataValid);

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

        public static List<Md167ExtraContractImportData> importData(MemoryStream ms, int sheetnumber, int rowStart, ImportHistoryType importHistoryType)
        {
            XSSFWorkbook workbook = new XSSFWorkbook(ms);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);

            List<Md167ExtraContractImportData> res = new List<Md167ExtraContractImportData>();
            for (int row = rowStart; row <= sheet.LastRowNum; row++)
            {
                if (sheet.GetRow(row) != null)
                {
                    //Đọc dữ liệu từ từng cell
                    Md167ExtraContractImportData input1Detai = new Md167ExtraContractImportData();
                    input1Detai.Valid = true;
                    input1Detai.ErrMsg = "";

                    int column_count = importHistoryType == ImportHistoryType.Md167MainContract ? 24 : 26;
                    int column_count_extra = importHistoryType == ImportHistoryType.Md167MainContract ? 0 : 2;

                    for (int i = 0; i < column_count; i++)
                    {
                        try
                        {
                            var cell = sheet.GetRow(row).GetCell(i, MissingCellPolicy.RETURN_NULL_AND_BLANK);

                            //Lấy giá trị trong cell
                            string str = UtilsService.getCellValue(cell);
                            if (importHistoryType == ImportHistoryType.Md167ExtraContract)
                            {
                                if (i == 1)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.ExtraCode = str;
                                        }
                                        else
                                        {
                                            input1Detai.Valid = false;
                                            input1Detai.ErrMsg += "Cột Số phụ lục không có dữ liệu\n";
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột Số phụ lục\n";
                                    }
                                }
                                else if (i == 2)
                                {
                                    try
                                    {
                                        if (str != "")
                                        {
                                            input1Detai.ExtraDateSign = DateTime.FromOADate(Double.Parse(str));
                                            if (input1Detai.ExtraDateSign.Value.Year < 1900)
                                            {
                                                input1Detai.ExtraDateSign = null;
                                                input1Detai.Valid = false;
                                                input1Detai.ErrMsg += "Ngày ký phụ lục không hợp lệ\n";
                                            }
                                        }
                                        else
                                        {
                                            input1Detai.Valid = false;
                                            input1Detai.ErrMsg += "Cột Ngày ký phụ lục không có dữ liệu\n";
                                        }
                                    }
                                    catch
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Lỗi cột Ngày ký phụ lục\n";
                                    }
                                }
                            }

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
                            else if (i == (1 + column_count_extra))
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.Code = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Số hợp đồng không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Số hợp đồng\n";
                                }
                            }
                            else if (i == (2 + column_count_extra))
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.DateSign = DateTime.FromOADate(Double.Parse(str));
                                        if (input1Detai.DateSign.Value.Year < 1900)
                                        {
                                            input1Detai.DateSign = null;
                                            input1Detai.Valid = false;
                                            input1Detai.ErrMsg += "Ngày ký không hợp lệ\n";
                                        }
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Ngày ký không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Ngày ký\n";
                                }
                            }
                            else if (i == (3 + column_count_extra))
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        if (str.ToUpper() == "TRUE")
                                        {
                                            input1Detai.IsPersonal = true;
                                        }
                                        else
                                        {
                                            input1Detai.IsPersonal = false;
                                        }
                                    }
                                    else
                                    {
                                        input1Detai.IsPersonal = false;
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Cá nhân\n";
                                }
                            }
                            else if (i == (4 + column_count_extra))
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.OrganizationName = str;
                                    }
                                    else if (input1Detai.IsPersonal != true)
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Tên tổ chức không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Tên tổ chức\n";
                                }
                            }
                            else if (i == (5 + column_count_extra))
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.TaxCode = str;
                                    }
                                    else if (input1Detai.IsPersonal != true)
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Mã số thuế không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Mã số thuế\n";
                                }
                            }
                            else if (i == (6 + column_count_extra))
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.FullName = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Tên người thuê/đại diện không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Tên người thuê/đại diện\n";
                                }
                            }
                            else if (i == (7 + column_count_extra))
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.IdentityCode = str;
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột CMND/CCCD\n";
                                }
                            }
                            else if (i == (8 + column_count_extra))
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.IdentityDate = DateTime.FromOADate(Double.Parse(str));
                                        if (input1Detai.IdentityDate.Value.Year < 1900)
                                        {
                                            input1Detai.IdentityDate = null;
                                            input1Detai.Valid = false;
                                            input1Detai.ErrMsg += "Ngày cấp không hợp lệ\n";
                                        }
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Ngày cấp\n";
                                }
                            }
                            else if (i == (9 + column_count_extra))
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.IdentityPlace = str;
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Nơi cấp\n";
                                }
                            }
                            else if (i == (10 + column_count_extra))
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.Address = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Địa chỉ không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Địa chỉ\n";
                                }
                            }
                            else if (i == (11 + column_count_extra))
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.Phone = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Số điện thoại không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Điện thoại\n";
                                }
                            }
                            else if (i == (12 + column_count_extra))
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.BusinessLicense = str;
                                    }
                                    else if (input1Detai.IsPersonal != true)
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Giấy phép kinh doanh không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Giấy phép kinh doanh\n";
                                }
                            }
                            else if (i == (13 + column_count_extra))
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.Position = str;
                                    }
                                    else if (input1Detai.IsPersonal != true)
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Chức vụ không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Chức vụ\n";
                                }
                            }
                            else if (i == (14 + column_count_extra))
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
                            else if (i == (15 + column_count_extra))
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.KiosNumber = str;
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Tên Kios\n";
                                }
                            }
                            else if (i == (16 + column_count_extra))
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.TypePrice = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Giá niêm yết/Đấu giá không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Giá niêm yết/Đấu giá\n";
                                }
                            }
                            else if (i == (17 + column_count_extra))
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.RentalPeriod = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Thời gian thuê không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Thời gian thuê\n";
                                }
                            }
                            else if (i == (18 + column_count_extra))
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.NoteRentalPeriod = str;
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Thời gian thuê - Khác\n";
                                }
                            }
                            else if (i == (19 + column_count_extra))
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.RentalPurpose = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Mục đích thuê không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Mục đích thuê\n";
                                }
                            }
                            else if (i == (20 + column_count_extra))
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.NoteRentalPurpose = str;
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Mục đích thuê - Khác\n";
                                }
                            }
                            else if (i == (21 + column_count_extra))
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.PaymentPeriod = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Kỳ thanh toán không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Kỳ thanh toán\n";
                                }
                            }
                            else if (i == (22 + column_count_extra))
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.DateGroundHandover = DateTime.FromOADate(Double.Parse(str));
                                        if (input1Detai.DateGroundHandover.Value.Year < 1900)
                                        {
                                            input1Detai.DateGroundHandover = null;
                                            input1Detai.Valid = false;
                                            input1Detai.ErrMsg += "Ngày bàn giao mặt bằng không hợp lệ\n";
                                        }
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Ngày bàn giao mặt bằng không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Ngày bàn giao mặt bằng\n";
                                }
                            }
                            else if (i == (23 + column_count_extra))
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        input1Detai.ContractStatus = str;
                                    }
                                    else
                                    {
                                        input1Detai.Valid = false;
                                        input1Detai.ErrMsg += "Cột Trạng thái hợp đồng không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    input1Detai.Valid = false;
                                    input1Detai.ErrMsg += "Lỗi cột Trạng thái hợp đồng\n";
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

        private bool Md167ContractExists(int id)
        {
            return _context.Md167Contracts.Count(e => e.Id == id) > 0;
        }
    }
}
