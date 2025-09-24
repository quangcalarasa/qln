using AutoMapper;
using DevExpress.ClipboardSource.SpreadsheetML;
using DevExpress.Utils.Extensions;
using DevExpress.XtraRichEdit.Import.Html;
using IOITQln.Common.Constants;
using IOITQln.Common.Enums;
using IOITQln.Common.Services;
using IOITQln.Common.ViewModels.Common;
using IOITQln.Entities;
using IOITQln.Migrations;
using IOITQln.Models.Data;
using IOITQln.Models.Dto;
using IOITQln.Persistence;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Web;
using static IOITQln.Common.Enums.AppEnums;
using System.Text.RegularExpressions;

namespace IOITQln.Controllers.ApiTdc
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TdcCustomerController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("TdcCustomer", "TdcCustomer");
        private static string functionCode = "CUS_TDC";
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        private IHostingEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;

        public TdcCustomerController(ApiDbContext context, IMapper mapper, IHostingEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
        }

        //[HttpPost]
        //[Route("ImportExcel")]
        //public async Task<IActionResult> ImportExcel(IFormFile file)
        //{
        //    DefaultResponse def = new DefaultResponse();
        //    var identity = (ClaimsIdentity)User.Identity;
        //    int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id").Select(c => c.Value).SingleOrDefault());
        //    string fullName = identity.Claims.Where(c => c.Type == "FullName").Select(c => c.Value).SingleOrDefault() ?? string.Empty;
        //    string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
        //    if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.CREATE))
        //    {
        //        def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_CREATE_MESSAGE);
        //        return Ok(def);
        //    }
        //    int count = 0;
        //    using (var package = new ExcelPackage(file.OpenReadStream()))
        //    {
        //        var worksheet = package.Workbook.Worksheets[0];
        //        var rowCount = worksheet.Dimension.Rows;
        //        List<TdcCustomer> inv = new List<TdcCustomer>();
        //        TdcCustomer tdcCustomer = new TdcCustomer();
        //        for (int row = 1; row <= rowCount; row++)
        //        {
        //            int invRateId = int.Parse(worksheet.Cells[row, 1].Value.ToString());
        //            string code = worksheet.Cells[row, 2].Value.ToString();
        //            string fullname = worksheet.Cells[row, 3].Value.ToString();
        //            string cccd = worksheet.Cells[row, 4].Value.ToString();
        //            string dob = (worksheet.Cells[row, 5].Value.ToString());
        //            DateTime date = DateTime.ParseExact(dob, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        //            string phone = worksheet.Cells[row, 6].Value.ToString();

        //            tdcCustomer = new TdcCustomer();
        //            tdcCustomer.Code = code;
        //            tdcCustomer.FullName = fullname;
        //            tdcCustomer.CCCD = cccd;
        //            tdcCustomer.Dob = date;
        //            tdcCustomer.Phone = phone;
        //            tdcCustomer.CreatedById = userId;
        //            tdcCustomer.CreatedBy = fullName;
        //            TdcCustomer tdcCustomerExit = _context.TdcCustomers.Where(
        //                c => c.Code == tdcCustomer.Code  &&
        //                c.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
        //            if (tdcCustomerExit == null)
        //            {
        //                count++;
        //                inv.Add(tdcCustomer);
        //            }
        //        }
        //        if (count == 0)
        //        {
        //            def.meta = new Meta(400, "Khong co muc them moi");
        //            return Ok(def);
        //        }
        //        using (var transaction = _context.Database.BeginTransaction())
        //        {
        //            _context.TdcCustomers.AddRange(inv);
        //            try
        //            {
        //                await _context.SaveChangesAsync();
        //                transaction.Commit();
        //                def.meta = new Meta(count, ApiConstants.MessageResource.ADD_SUCCESS + "(" + count + "line)");
        //                return Ok(def);
        //            }
        //            catch (DbUpdateException e)
        //            {
        //                transaction.Rollback();
        //                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_400_MESSAGE);
        //                return Ok(def);
        //            }
        //        }
        //    }
        //}

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
                    IQueryable<TdcCustomer> data = _context.TdcCustomers.Where(c => c.Status != AppEnums.EntityStatus.DELETED);
                    if (paging.query != null)
                    {
                        paging.query = HttpUtility.UrlDecode(paging.query);
                    }
                    data = data.Where(paging.query);
                    def.metadata = data.Count();

                    if (paging.page_size > 0)
                    {
                        if(paging.order_by != null)
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
                        List<TdcCustomerData> res = _mapper.Map<List<TdcCustomerData>>(data.ToList());
                        foreach(TdcCustomerData item in res)
                        {
                            //Thông tin căn nhà
                            item.TdcProjectName = _context.TDCProjects.Where(l => l.Id == item.TdcProjectId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Name).FirstOrDefault();

                            item.TdcLandName = _context.Lands.Where(f => f.Id == item.LandId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            item.TdcBlockHouseName = _context.BlockHouses.Where(f => f.Id == item.BlockHouseId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            item.TdcFloorName = _context.FloorTdcs.Where(f => f.Id == item.FloorTdcId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                            ApartmentTdc apartmentTdc = _context.ApartmentTdcs.Where(f => f.Id == item.TdcApartmentId && f.Status != AppEnums.EntityStatus.DELETED)
                                .Select(s => new ApartmentTdc()
                                {
                                    Name = s.Name,
                                    Corner = s.Corner
                                })
                                .FirstOrDefault();
                            item.TdcApartmentName = apartmentTdc != null ? apartmentTdc.Name : "";
                            item.Corner = apartmentTdc != null ? apartmentTdc.Corner : false;

                            //Địa Chỉ Tạm Trú Khách Hàng
                            item.fullAddressTT = item.AddressTT;

                            Ward wardTT = _context.Wards.Where(x => x.Id == item.WardTT).FirstOrDefault();
                            item.fullAddressTT = item.fullAddressTT != null && item.fullAddressTT != "" ? (wardTT != null ? String.Join(",", item.fullAddressTT, wardTT.Name) : item.fullAddressTT) : (wardTT != null ? wardTT.Name : item.fullAddressTT);

                            District districtTT = _context.Districts.Where(x => x.Id == item.DistrictTT).FirstOrDefault();
                            item.fullAddressTT = item.fullAddressTT != null && item.fullAddressTT != "" ? (districtTT != null ? String.Join(",", item.fullAddressTT, districtTT.Name) : item.fullAddressTT) : (districtTT != null ? districtTT.Name : item.fullAddressTT);

                            Province provinceTT = _context.Provincies.Where(x =>x.Id == item.ProvinceTT).FirstOrDefault();
                            item.fullAddressTT = item.fullAddressTT != null && item.fullAddressTT != "" ? (provinceTT != null ? String.Join(",", item.fullAddressTT, provinceTT.Name) : item.fullAddressTT) : (provinceTT != null ? provinceTT.Name : item.fullAddressTT);
                           
                            
                            //Địa Chỉ Liên hệ Khách Hàng
                            item.fullAddressLH = item.AddressLH;

                            Ward wardLH = _context.Wards.Where(x => x.Id == item.WardLH).FirstOrDefault();
                            item.fullAddressLH = item.fullAddressLH != null && item.fullAddressLH != "" ? (wardLH != null ? String.Join(",", item.fullAddressLH, wardLH.Name) : item.fullAddressLH) : (wardLH != null ? wardLH.Name : item.fullAddressLH);

                            District districtLH = _context.Districts.Where(x => x.Id == item.DistrictLH).FirstOrDefault();
                            item.fullAddressLH = item.fullAddressLH != null && item.fullAddressLH != "" ? (districtLH != null ? String.Join(",", item.fullAddressLH, districtLH.Name) : item.fullAddressLH) : (districtLH != null ? districtLH.Name : item.fullAddressLH);

                            Province provinceLH = _context.Provincies.Where(x => x.Id == item.ProvinceLH).FirstOrDefault();
                            item.fullAddressLH = item.fullAddressLH != null && item.fullAddressLH != "" ? (provinceLH != null ? String.Join(",", item.fullAddressLH, provinceLH.Name) : item.fullAddressLH) : (provinceLH != null ? provinceLH.Name : item.fullAddressLH);

                            item.tdcCustomerFiles = _context.TdcCustomerFiles.Where(l => l.TdcCustomerId == item.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();

                            List<TdcAuthCustomerDetail> tdcAuthCustomerDetails = _context.TdcAuthCustomerDetails.Where(x => x.TdcCustomerId == item.Id && x.Status != AppEnums.EntityStatus.DELETED).ToList();
                            List<TdcAuthCustomerDetailData> map_tdcAuthCustomerDetailDatas = _mapper.Map<List<TdcAuthCustomerDetailData>>(tdcAuthCustomerDetails);
                            foreach(TdcAuthCustomerDetailData map_tdcAuthCustomerDetailData in map_tdcAuthCustomerDetailDatas)
                            {
                                //Địa Chỉ Tạm Trú Người ủy quyền
                                //map_tdcAuthCustomerDetailData.fullAddressTt = map_tdcAuthCustomerDetailData.AddressTt;

                                Lane landTt = _context.Lanies.Where(x => x.Id == map_tdcAuthCustomerDetailData.LaneTt).FirstOrDefault();
                                map_tdcAuthCustomerDetailData.LaneTT = landTt != null ? landTt.Name : "";

                                Ward wardTt = _context.Wards.Where(x => x.Id == map_tdcAuthCustomerDetailData.WardTt).FirstOrDefault();
                                map_tdcAuthCustomerDetailData.fullAddressTt = map_tdcAuthCustomerDetailData.fullAddressTt != null && map_tdcAuthCustomerDetailData.fullAddressTt != "" ? (wardTt != null ? String.Join(",", map_tdcAuthCustomerDetailData.fullAddressTt, wardTt.Name) : map_tdcAuthCustomerDetailData.fullAddressTt) : (wardTt != null ? wardTt.Name : map_tdcAuthCustomerDetailData.fullAddressTt);
                               

                                District districtTt = _context.Districts.Where(x => x.Id == map_tdcAuthCustomerDetailData.DistrictTt).FirstOrDefault();
                                map_tdcAuthCustomerDetailData.fullAddressTt = map_tdcAuthCustomerDetailData.fullAddressTt != null && map_tdcAuthCustomerDetailData.fullAddressTt != "" ? (districtTt != null ? String.Join(",", map_tdcAuthCustomerDetailData.fullAddressTt, districtTt.Name) : map_tdcAuthCustomerDetailData.fullAddressTt) : (districtTt != null ? districtTt.Name : map_tdcAuthCustomerDetailData.fullAddressTt);

                                Province provinceTt = _context.Provincies.Where(x => x.Id == map_tdcAuthCustomerDetailData.ProvinceTt).FirstOrDefault();
                                map_tdcAuthCustomerDetailData.fullAddressTt = map_tdcAuthCustomerDetailData.fullAddressTt != null && map_tdcAuthCustomerDetailData.fullAddressTt != "" ? (provinceTt != null ? String.Join(",", map_tdcAuthCustomerDetailData.fullAddressTt, provinceTt.Name) : map_tdcAuthCustomerDetailData.fullAddressTt) : (provinceTt != null ? provinceTt.Name : map_tdcAuthCustomerDetailData.fullAddressTt);

                                //Địa Chỉ Liên Hệ Người Ủy Quyền

                                //map_tdcAuthCustomerDetailData.fullAddressLh = map_tdcAuthCustomerDetailData.AddressLh;

                                Lane landLh = _context.Lanies.Where(x => x.Id == map_tdcAuthCustomerDetailData.LaneLh).FirstOrDefault();
                                map_tdcAuthCustomerDetailData.LaneLH = landLh != null ? landLh.Name : "";

                                Ward wardLh = _context.Wards.Where(x => x.Id == map_tdcAuthCustomerDetailData.WardLh).FirstOrDefault();
                                map_tdcAuthCustomerDetailData.fullAddressLh = map_tdcAuthCustomerDetailData.fullAddressLh != null && map_tdcAuthCustomerDetailData.fullAddressLh != "" ? (wardLh != null ? String.Join(",", map_tdcAuthCustomerDetailData.fullAddressLh, wardLh.Name) : map_tdcAuthCustomerDetailData.fullAddressLh) : (wardLh != null ? wardLh.Name : map_tdcAuthCustomerDetailData.fullAddressLh);

                                District districtLh = _context.Districts.Where(x => x.Id == map_tdcAuthCustomerDetailData.DistrictLh).FirstOrDefault();
                                map_tdcAuthCustomerDetailData.fullAddressLh = map_tdcAuthCustomerDetailData.fullAddressLh != null && map_tdcAuthCustomerDetailData.fullAddressLh != "" ? (districtLh != null ? String.Join(",", map_tdcAuthCustomerDetailData.fullAddressLh, districtLh.Name) : map_tdcAuthCustomerDetailData.fullAddressLh) : (districtLh != null ? districtLh.Name : map_tdcAuthCustomerDetailData.fullAddressLh);

                                Province provinceLh = _context.Provincies.Where(x => x.Id == map_tdcAuthCustomerDetailData.ProvinceLh).FirstOrDefault();
                                map_tdcAuthCustomerDetailData.fullAddressLh = map_tdcAuthCustomerDetailData.fullAddressLh != null && map_tdcAuthCustomerDetailData.fullAddressLh != "" ? (provinceLh != null ? String.Join(",", map_tdcAuthCustomerDetailData.fullAddressLh, provinceLh.Name) : map_tdcAuthCustomerDetailData.fullAddressLh) : (provinceLh != null ? provinceLh.Name : map_tdcAuthCustomerDetailData.fullAddressLh);
                            }
                            item.tdcAuthCustomerDetailDatas = map_tdcAuthCustomerDetailDatas.ToList();

                            List<TdcMemberCustomer> tdcMemberCustomers = _context.TdcMemberCustomers.Where(x => x.TdcCustomerId == item.Id && x.Status != AppEnums.EntityStatus.DELETED).ToList();
                            List<TdcMenberCustomerData> map_tdcMemberCustomerDatas = _mapper.Map<List<TdcMenberCustomerData>>(tdcMemberCustomers);
                            foreach (TdcMenberCustomerData map_tdcMemberCustomerData in map_tdcMemberCustomerDatas)
                            {
                                //Địa Chỉ Tạm Trú Người ủy quyền
                                //map_tdcAuthCustomerDetailData.fullAddressTt = map_tdcAuthCustomerDetailData.AddressTt;

                                Lane landTt = _context.Lanies.Where(x => x.Id == map_tdcMemberCustomerData.LaneTt).FirstOrDefault();
                                map_tdcMemberCustomerData.LaneTT = landTt != null ? landTt.Name : "";

                                Ward wardTt = _context.Wards.Where(x => x.Id == map_tdcMemberCustomerData.WardTt).FirstOrDefault();
                                map_tdcMemberCustomerData.fullAddressTt = map_tdcMemberCustomerData.fullAddressTt != null && map_tdcMemberCustomerData.fullAddressTt != "" ? (wardTt != null ? String.Join(",", map_tdcMemberCustomerData.fullAddressTt, wardTt.Name) : map_tdcMemberCustomerData.fullAddressTt) : (wardTt != null ? wardTt.Name : map_tdcMemberCustomerData.fullAddressTt);


                                District districtTt = _context.Districts.Where(x => x.Id == map_tdcMemberCustomerData.DistrictTt).FirstOrDefault();
                                map_tdcMemberCustomerData.fullAddressTt = map_tdcMemberCustomerData.fullAddressTt != null && map_tdcMemberCustomerData.fullAddressTt != "" ? (districtTt != null ? String.Join(",", map_tdcMemberCustomerData.fullAddressTt, districtTt.Name) : map_tdcMemberCustomerData.fullAddressTt) : (districtTt != null ? districtTt.Name : map_tdcMemberCustomerData.fullAddressTt);

                                Province provinceTt = _context.Provincies.Where(x => x.Id == map_tdcMemberCustomerData.ProvinceTt).FirstOrDefault();
                                map_tdcMemberCustomerData.fullAddressTt = map_tdcMemberCustomerData.fullAddressTt != null && map_tdcMemberCustomerData.fullAddressTt != "" ? (provinceTt != null ? String.Join(",", map_tdcMemberCustomerData.fullAddressTt, provinceTt.Name) : map_tdcMemberCustomerData.fullAddressTt) : (provinceTt != null ? provinceTt.Name : map_tdcMemberCustomerData.fullAddressTt);

                                //Địa Chỉ Liên Hệ Người Ủy Quyền

                                //map_tdcAuthCustomerDetailData.fullAddressLh = map_tdcAuthCustomerDetailData.AddressLh;

                                Lane landLh = _context.Lanies.Where(x => x.Id == map_tdcMemberCustomerData.LaneLh).FirstOrDefault();
                                map_tdcMemberCustomerData.LaneLH = landLh != null ? landLh.Name : "";

                                Ward wardLh = _context.Wards.Where(x => x.Id == map_tdcMemberCustomerData.WardLh).FirstOrDefault();
                                map_tdcMemberCustomerData.fullAddressLh = map_tdcMemberCustomerData.fullAddressLh != null && map_tdcMemberCustomerData.fullAddressLh != "" ? (wardLh != null ? String.Join(",", map_tdcMemberCustomerData.fullAddressLh, wardLh.Name) : map_tdcMemberCustomerData.fullAddressLh) : (wardLh != null ? wardLh.Name : map_tdcMemberCustomerData.fullAddressLh);

                                District districtLh = _context.Districts.Where(x => x.Id == map_tdcMemberCustomerData.DistrictLh).FirstOrDefault();
                                map_tdcMemberCustomerData.fullAddressLh = map_tdcMemberCustomerData.fullAddressLh != null && map_tdcMemberCustomerData.fullAddressLh != "" ? (districtLh != null ? String.Join(",", map_tdcMemberCustomerData.fullAddressLh, districtLh.Name) : map_tdcMemberCustomerData.fullAddressLh) : (districtLh != null ? districtLh.Name : map_tdcMemberCustomerData.fullAddressLh);

                                Province provinceLh = _context.Provincies.Where(x => x.Id == map_tdcMemberCustomerData.ProvinceLh).FirstOrDefault();
                                map_tdcMemberCustomerData.fullAddressLh = map_tdcMemberCustomerData.fullAddressLh != null && map_tdcMemberCustomerData.fullAddressLh != "" ? (provinceLh != null ? String.Join(",", map_tdcMemberCustomerData.fullAddressLh, provinceLh.Name) : map_tdcMemberCustomerData.fullAddressLh) : (provinceLh != null ? provinceLh.Name : map_tdcMemberCustomerData.fullAddressLh);
                            }
                            item.tdcMenberCustomerDatas = map_tdcMemberCustomerDatas.ToList();
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
            catch(Exception ex)
            {
                log.Error("GetbyPage Error" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }
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
                TdcCustomer data = await _context.TdcCustomers.FindAsync(id);
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                    return Ok(def);
                }
                TdcCustomerData res = _mapper.Map<TdcCustomerData>(data);

                //Thông tin căn nhà

                res.TdcProjectName = _context.TDCProjects.Where(l => l.Id == res.TdcProjectId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Name).FirstOrDefault();

                res.TdcLandName = _context.Lands.Where(f => f.Id == res.LandId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                res.TdcBlockHouseName = _context.BlockHouses.Where(f => f.Id == res.BlockHouseId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                res.TdcFloorName = _context.FloorTdcs.Where(f => f.Id == res.FloorTdcId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                ApartmentTdc apartmentTdc = _context.ApartmentTdcs.Where(f => f.Id == res.TdcApartmentId && f.Status != AppEnums.EntityStatus.DELETED)
                    .Select(s => new ApartmentTdc()
                    {
                        Name = s.Name,
                        Corner = s.Corner
                    })
                    .FirstOrDefault();
                res.TdcApartmentName = apartmentTdc != null ? apartmentTdc.Name : "";
                res.Corner = apartmentTdc != null ? apartmentTdc.Corner : false;

                //Địa Chỉ Tạm trú
                res.fullAddressTT = res.AddressTT;

                Ward wardTT = _context.Wards.Where(x => x.Id == res.WardTT).FirstOrDefault();
                res.fullAddressTT = res.fullAddressTT != null && res.fullAddressTT != "" ? (wardTT != null ? String.Join(",", res.fullAddressTT, wardTT.Name) : res.fullAddressTT) : (wardTT != null ? wardTT.Name : res.fullAddressTT);

                District districtTT = _context.Districts.Where(x => x.Id == res.DistrictTT).FirstOrDefault();
                res.fullAddressTT = res.fullAddressTT != null && res.fullAddressTT != "" ? (districtTT != null ? String.Join(",", res.fullAddressTT, districtTT.Name) : res.fullAddressTT) : (districtTT != null ? districtTT.Name : res.fullAddressTT);

                Province provinceTT = _context.Provincies.Where(x => x.Id == res.ProvinceTT).FirstOrDefault();
                res.fullAddressTT = res.fullAddressTT != null && res.fullAddressTT != "" ? (provinceTT != null ? String.Join(",", res.fullAddressTT, provinceTT.Name) : res.fullAddressTT) : (provinceTT != null ? provinceTT.Name : res.fullAddressTT);

                //Địa Chỉ Liên Hệ
                res.fullAddressLH = res.AddressLH;

                Ward wardLH = _context.Wards.Where(x => x.Id == res.WardLH).FirstOrDefault();
                res.fullAddressLH = res.fullAddressLH != null && res.fullAddressLH != "" ? (wardLH != null ? String.Join(",", res.fullAddressLH, wardLH.Name) : res.fullAddressLH) : (wardLH != null ? wardLH.Name : res.fullAddressLH);

                District districtLH = _context.Districts.Where(x => x.Id == res.DistrictLH).FirstOrDefault();
                res.fullAddressLH = res.fullAddressLH != null && res.fullAddressLH != "" ? (districtLH != null ? String.Join(",", res.fullAddressLH, districtLH.Name) : res.fullAddressLH) : (districtLH != null ? districtLH.Name : res.fullAddressLH);

                Province provinceLH = _context.Provincies.Where(x => x.Id == res.ProvinceLH).FirstOrDefault();
                res.fullAddressLH = res.fullAddressLH != null && res.fullAddressLH != "" ? (provinceLH != null ? String.Join(",", res.fullAddressLH, provinceLH.Name) : res.fullAddressLH) : (provinceLH != null ? provinceLH.Name : res.fullAddressLH);

                res.tdcCustomerFiles = _context.TdcCustomerFiles.Where(l => l.TdcCustomerId == res.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();

                List<TdcAuthCustomerDetail> tdcAuthCustomerDetails = _context.TdcAuthCustomerDetails.Where(x => x.TdcCustomerId == res.Id && x.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<TdcAuthCustomerDetailData> map_tdcAuthCustomerDetailDatas = _mapper.Map<List<TdcAuthCustomerDetailData>>(tdcAuthCustomerDetails);
                foreach (TdcAuthCustomerDetailData map_tdcAuthCustomerDetailData in map_tdcAuthCustomerDetailDatas)
                {
                    //Địa Chỉ Liên Hệ Người Ủy Quyền

                    map_tdcAuthCustomerDetailData.fullAddressTt = map_tdcAuthCustomerDetailData.AddressTt;

                    Ward wardTt = _context.Wards.Where(x => x.Id == map_tdcAuthCustomerDetailData.WardTt).FirstOrDefault();
                    map_tdcAuthCustomerDetailData.fullAddressTt = map_tdcAuthCustomerDetailData.fullAddressTt != null && map_tdcAuthCustomerDetailData.fullAddressTt != "" ? (wardTt != null ? String.Join(",", map_tdcAuthCustomerDetailData.fullAddressTt, wardTt.Name) : map_tdcAuthCustomerDetailData.fullAddressTt) : (wardTt != null ? wardTt.Name : map_tdcAuthCustomerDetailData.fullAddressTt);

                    District districtTt = _context.Districts.Where(x => x.Id == map_tdcAuthCustomerDetailData.DistrictTt).FirstOrDefault();
                    map_tdcAuthCustomerDetailData.fullAddressTt = map_tdcAuthCustomerDetailData.fullAddressTt != null && map_tdcAuthCustomerDetailData.fullAddressTt != "" ? (districtTt != null ? String.Join(",", map_tdcAuthCustomerDetailData.fullAddressTt, districtTt.Name) : map_tdcAuthCustomerDetailData.fullAddressTt) : (districtTt != null ? districtTt.Name : map_tdcAuthCustomerDetailData.fullAddressTt);

                    Province provinceTt = _context.Provincies.Where(x => x.Id == map_tdcAuthCustomerDetailData.ProvinceTt).FirstOrDefault();
                    map_tdcAuthCustomerDetailData.fullAddressTt = map_tdcAuthCustomerDetailData.fullAddressTt != null && map_tdcAuthCustomerDetailData.fullAddressTt != "" ? (provinceTt != null ? String.Join(",", map_tdcAuthCustomerDetailData.fullAddressTt, provinceTt.Name) : map_tdcAuthCustomerDetailData.fullAddressTt) : (provinceTt != null ? provinceTt.Name : map_tdcAuthCustomerDetailData.fullAddressTt);

                    //Địa Chỉ Liên Hệ Người Ủy Quyền

                    map_tdcAuthCustomerDetailData.fullAddressLh = map_tdcAuthCustomerDetailData.AddressLh;

                    Ward wardLh = _context.Wards.Where(x => x.Id == map_tdcAuthCustomerDetailData.WardLh).FirstOrDefault();
                    map_tdcAuthCustomerDetailData.fullAddressLh = map_tdcAuthCustomerDetailData.fullAddressLh != null && map_tdcAuthCustomerDetailData.fullAddressLh != "" ? (wardLh != null ? String.Join(",", map_tdcAuthCustomerDetailData.fullAddressLh, wardLh.Name) : map_tdcAuthCustomerDetailData.fullAddressLh) : (wardLh != null ? wardLh.Name : map_tdcAuthCustomerDetailData.fullAddressLh);

                    District districtLh = _context.Districts.Where(x => x.Id == map_tdcAuthCustomerDetailData.DistrictLh).FirstOrDefault();
                    map_tdcAuthCustomerDetailData.fullAddressLh = map_tdcAuthCustomerDetailData.fullAddressLh != null && map_tdcAuthCustomerDetailData.fullAddressLh != "" ? (districtLh != null ? String.Join(",", map_tdcAuthCustomerDetailData.fullAddressLh, districtLh.Name) : map_tdcAuthCustomerDetailData.fullAddressLh) : (districtLh != null ? districtLh.Name : map_tdcAuthCustomerDetailData.fullAddressLh);

                    Province provinceLh = _context.Provincies.Where(x => x.Id == map_tdcAuthCustomerDetailData.ProvinceLh).FirstOrDefault();
                    map_tdcAuthCustomerDetailData.fullAddressLh = map_tdcAuthCustomerDetailData.fullAddressLh != null && map_tdcAuthCustomerDetailData.fullAddressLh != "" ? (provinceLh != null ? String.Join(",", map_tdcAuthCustomerDetailData.fullAddressLh, provinceLh.Name) : map_tdcAuthCustomerDetailData.fullAddressLh) : (provinceLh != null ? provinceLh.Name : map_tdcAuthCustomerDetailData.fullAddressLh);
                }
                res.tdcAuthCustomerDetailDatas = map_tdcAuthCustomerDetailDatas.ToList();

                List<TdcMemberCustomer> tdcMemberCustomers = _context.TdcMemberCustomers.Where(x => x.TdcCustomerId == res.Id && x.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<TdcMenberCustomerData> map_tdcMemberCustomerDatas = _mapper.Map<List<TdcMenberCustomerData>>(tdcMemberCustomers);
                foreach (TdcMenberCustomerData map_tdcMemberCustomerData in map_tdcMemberCustomerDatas)
                {
                    //Địa Chỉ Tạm Trú Người ủy quyền
                    //map_tdcAuthCustomerDetailData.fullAddressTt = map_tdcAuthCustomerDetailData.AddressTt;

                    Lane landTt = _context.Lanies.Where(x => x.Id == map_tdcMemberCustomerData.LaneTt).FirstOrDefault();
                    map_tdcMemberCustomerData.LaneTT = landTt != null ? landTt.Name : "";

                    Ward wardTt = _context.Wards.Where(x => x.Id == map_tdcMemberCustomerData.WardTt).FirstOrDefault();
                    map_tdcMemberCustomerData.fullAddressTt = map_tdcMemberCustomerData.fullAddressTt != null && map_tdcMemberCustomerData.fullAddressTt != "" ? (wardTt != null ? String.Join(",", map_tdcMemberCustomerData.fullAddressTt, wardTt.Name) : map_tdcMemberCustomerData.fullAddressTt) : (wardTt != null ? wardTt.Name : map_tdcMemberCustomerData.fullAddressTt);


                    District districtTt = _context.Districts.Where(x => x.Id == map_tdcMemberCustomerData.DistrictTt).FirstOrDefault();
                    map_tdcMemberCustomerData.fullAddressTt = map_tdcMemberCustomerData.fullAddressTt != null && map_tdcMemberCustomerData.fullAddressTt != "" ? (districtTt != null ? String.Join(",", map_tdcMemberCustomerData.fullAddressTt, districtTt.Name) : map_tdcMemberCustomerData.fullAddressTt) : (districtTt != null ? districtTt.Name : map_tdcMemberCustomerData.fullAddressTt);

                    Province provinceTt = _context.Provincies.Where(x => x.Id == map_tdcMemberCustomerData.ProvinceTt).FirstOrDefault();
                    map_tdcMemberCustomerData.fullAddressTt = map_tdcMemberCustomerData.fullAddressTt != null && map_tdcMemberCustomerData.fullAddressTt != "" ? (provinceTt != null ? String.Join(",", map_tdcMemberCustomerData.fullAddressTt, provinceTt.Name) : map_tdcMemberCustomerData.fullAddressTt) : (provinceTt != null ? provinceTt.Name : map_tdcMemberCustomerData.fullAddressTt);

                    //Địa Chỉ Liên Hệ Người Ủy Quyền

                    //map_tdcAuthCustomerDetailData.fullAddressLh = map_tdcAuthCustomerDetailData.AddressLh;

                    Lane landLh = _context.Lanies.Where(x => x.Id == map_tdcMemberCustomerData.LaneLh).FirstOrDefault();
                    map_tdcMemberCustomerData.LaneLH = landLh != null ? landLh.Name : "";

                    Ward wardLh = _context.Wards.Where(x => x.Id == map_tdcMemberCustomerData.WardLh).FirstOrDefault();
                    map_tdcMemberCustomerData.fullAddressLh = map_tdcMemberCustomerData.fullAddressLh != null && map_tdcMemberCustomerData.fullAddressLh != "" ? (wardLh != null ? String.Join(",", map_tdcMemberCustomerData.fullAddressLh, wardLh.Name) : map_tdcMemberCustomerData.fullAddressLh) : (wardLh != null ? wardLh.Name : map_tdcMemberCustomerData.fullAddressLh);

                    District districtLh = _context.Districts.Where(x => x.Id == map_tdcMemberCustomerData.DistrictLh).FirstOrDefault();
                    map_tdcMemberCustomerData.fullAddressLh = map_tdcMemberCustomerData.fullAddressLh != null && map_tdcMemberCustomerData.fullAddressLh != "" ? (districtLh != null ? String.Join(",", map_tdcMemberCustomerData.fullAddressLh, districtLh.Name) : map_tdcMemberCustomerData.fullAddressLh) : (districtLh != null ? districtLh.Name : map_tdcMemberCustomerData.fullAddressLh);

                    Province provinceLh = _context.Provincies.Where(x => x.Id == map_tdcMemberCustomerData.ProvinceLh).FirstOrDefault();
                    map_tdcMemberCustomerData.fullAddressLh = map_tdcMemberCustomerData.fullAddressLh != null && map_tdcMemberCustomerData.fullAddressLh != "" ? (provinceLh != null ? String.Join(",", map_tdcMemberCustomerData.fullAddressLh, provinceLh.Name) : map_tdcMemberCustomerData.fullAddressLh) : (provinceLh != null ? provinceLh.Name : map_tdcMemberCustomerData.fullAddressLh);
                }
                res.tdcMenberCustomerDatas = map_tdcMemberCustomerDatas.ToList();

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = res;
                return Ok(res);
            }
            catch(Exception ex)
            {
                log.Error("GetById Error" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(TdcCustomerData input)
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
                input = (TdcCustomerData)UtilsService.TrimStringPropertyTypeObject(input);
                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                // if (input.AddressTT == null || input.AddressTT == "")
                // {
                //     def.meta = new Meta(400, "Địa Chỉ Không Được Trống!!!");
                //     return Ok(def);
                // }

                if (input.AddressLH == null || input.AddressLH == "")
                {
                    def.meta = new Meta(400, "Địa Chỉ Không Được Trống!!!");
                    return Ok(def);
                }
                if (input.FullName == null || input.FullName == "")
                {
                    def.meta = new Meta(400, "Tên Khách Hàng Không Được Trống!!!");
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.CreatedById = userId;
                    input.CreatedBy = fullName;
                    _context.TdcCustomers.Add(input);

                    try
                    {
                        await _context.SaveChangesAsync();
                        string code = "";
                        int newid = input.Id;
                        if (newid < 10) code = "KH_TDC_000" + newid;
                        if (newid >= 10) code = "KH_TDC_00" + newid;
                        if (newid >= 100) code = "KH_TDC_0" + newid;
                        if (newid >= 1000) code = "KH_TDC_" + newid;
                        input.Code = code;

                        TdcCustomer tdcCustomer = _context.TdcCustomers.Find(newid);
                        tdcCustomer.Code = code;
                        _context.Update(tdcCustomer);

                        if(input.tdcMenberCustomerDatas != null)
                        {
                            foreach (var i in input.tdcMenberCustomerDatas)
                            {
                                i.TdcCustomerId = input.Id;
                                i.CreatedById = userId;
                                i.CreatedBy = fullName;

                                _context.TdcMemberCustomers.Add(i);
                            }
                            await _context.SaveChangesAsync();
                        }

                        if (input.tdcCustomerFiles != null)
                        {
                            foreach(var tdcCustomerFile in input.tdcCustomerFiles)
                            {
                                tdcCustomerFile.TdcCustomerId = input.Id;
                                tdcCustomerFile.CreatedById = userId;
                                tdcCustomerFile.CreatedBy = fullName;

                                _context.TdcCustomerFiles.Add(tdcCustomerFile);
                            }
                            await _context.SaveChangesAsync();
                        }

                        if (input.tdcAuthCustomerDetailDatas != null)
                        {
                            foreach ( var tdcAuthDetailData in input.tdcAuthCustomerDetailDatas)
                            {
                                tdcAuthDetailData.TdcCustomerId = input.Id;
                                tdcAuthDetailData.CreatedById = userId;
                                tdcAuthDetailData.CreatedBy = fullName;

                                _context.TdcAuthCustomerDetails.Add(tdcAuthDetailData);
                            }
                            await _context.SaveChangesAsync();
                        }

                        LogActionModel logActionModel = new LogActionModel("Thêm mới khách hàng" + input.FullName, "TdcCustomer", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (input.Id > 0) transaction.Commit();
                        else transaction.Rollback();

                        def.meta = new Meta(200, ApiConstants.MessageResource.ADD_SUCCESS);
                        def.data = input;
                        return Ok(def); 
                    }
                    catch(DbUpdateException e)
                    {
                        log.Error("DbUpdateException:" + e);
                        transaction.Rollback();
                        if (TdcCustomerExists(input.Id))
                        {
                            def.meta = new Meta(212, "Id đã có trên hệ thống");
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
            catch(Exception e)
            {
                log.Error("Post Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] TdcCustomerData input)
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
                input = (TdcCustomerData)UtilsService.TrimStringPropertyTypeObject(input);

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
                if (input.AddressTT == null || input.AddressTT == "")
                {
                    def.meta = new Meta(400, "Địa Chỉ Không Được Trống!!!");
                    return Ok(def);
                }

                if (input.AddressLH == null || input.AddressLH == "")
                {
                    def.meta = new Meta(400, "Địa Chỉ Không Được Trống!!!");
                    return Ok(def);
                }
                if (input.FullName == null || input.FullName == "")
                {
                    def.meta = new Meta(400, "Tên Khách Hàng Không Được Trống!!!");
                    return Ok(def);
                }
                TdcCustomer data = await _context.TdcCustomers.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_UPDATE_MESSAGE);
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.UpdatedAt = DateTime.Now;
                    input.UpdatedById = userId;
                    input.UpdatedBy = fullName;
                    input.CreatedAt = data.CreatedAt;
                    input.CreatedBy = data.CreatedBy;
                    input.CreatedById = data.CreatedById;
                    input.Status = data.Status;

                    _context.Update(input);

                    try
                    {
                        string code = "";
                        int newid = input.Id;
                        if (newid < 10) code = "KH_TDC_000" + newid;
                        if (newid >= 10) code = "KH_TDC_00" + newid;
                        if (newid >= 100) code = "KH_TDC_0" + newid;
                        if (newid >= 1000) code = "KH_TDC_" + newid;
                        input.Code = code;
                        TdcCustomer tdcCustomer = _context.TdcCustomers.Find(newid);
                        tdcCustomer.Code = code;
                        _context.Update(tdcCustomer);
                        await _context.SaveChangesAsync();


                        List<TdcCustomerFile> lstTdcCustomerFileAdd = new List<TdcCustomerFile>();
                        List<TdcCustomerFile> lstTdcCustomerFilelUpdate = new List<TdcCustomerFile>();

                        List<TdcCustomerFile> tdcCustomerFiles = _context.TdcCustomerFiles.AsNoTracking().Where(l => l.TdcCustomerId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();

                        if (input.tdcCustomerFiles != null)
                        {
                            foreach (var tdcCustomerFile in input.tdcCustomerFiles)
                            {
                                TdcCustomerFile tdcCustomerFileExist = tdcCustomerFiles.Where(l => l.Id == tdcCustomerFile.Id).FirstOrDefault();
                                if (tdcCustomerFileExist == null)
                                {
                                    tdcCustomerFile.TdcCustomerId = input.Id;
                                    tdcCustomerFile.CreatedBy = fullName;
                                    tdcCustomerFile.CreatedById = userId;

                                    lstTdcCustomerFileAdd.Add(tdcCustomerFile);
                                }
                                else
                                {
                                    tdcCustomerFile.CreatedAt = tdcCustomerFile.CreatedAt;
                                    tdcCustomerFile.CreatedBy = tdcCustomerFile.CreatedBy;
                                    tdcCustomerFile.CreatedById = tdcCustomerFile.UpdatedById;
                                    tdcCustomerFile.TdcCustomerId = input.Id;
                                    tdcCustomerFile.UpdatedById = userId;
                                    tdcCustomerFile.UpdatedBy = fullName;

                                    lstTdcCustomerFilelUpdate.Add(tdcCustomerFile);
                                    tdcCustomerFiles.Remove(tdcCustomerFileExist);
                                }
                            }
                        }
                        foreach (var itemTdcCustomerFile in tdcCustomerFiles)
                        {
                            itemTdcCustomerFile.UpdatedAt = DateTime.Now;
                            itemTdcCustomerFile.UpdatedById = userId;
                            itemTdcCustomerFile.UpdatedBy = fullName;
                            itemTdcCustomerFile.Status = AppEnums.EntityStatus.DELETED;

                            lstTdcCustomerFilelUpdate.Add(itemTdcCustomerFile);
                        }
                        _context.TdcCustomerFiles.UpdateRange(lstTdcCustomerFilelUpdate);
                        _context.TdcCustomerFiles.AddRange(lstTdcCustomerFileAdd);

                        List<TdcAuthCustomerDetail> lstTdcAuthCustomerDetailAdd = new List<TdcAuthCustomerDetail>();
                        List<TdcAuthCustomerDetail> lstTdcAuthCustomerDetailUpdate = new List<TdcAuthCustomerDetail>();

                        List<TdcAuthCustomerDetail> tdcAuthCustomerDetails = _context.TdcAuthCustomerDetails.AsNoTracking().Where(l => l.TdcCustomerId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();

                        if (input.tdcAuthCustomerDetailDatas != null)
                        {
                            foreach (var tdcAuthCustomerDetail in input.tdcAuthCustomerDetailDatas)
                            {
                                TdcAuthCustomerDetail tdcAuthCustomerDetailExist = tdcAuthCustomerDetails.Where(l => l.Id == tdcAuthCustomerDetail.Id).FirstOrDefault();
                                if (tdcAuthCustomerDetailExist == null)
                                {
                                    tdcAuthCustomerDetail.TdcCustomerId = input.Id;
                                    tdcAuthCustomerDetail.CreatedBy = fullName;
                                    tdcAuthCustomerDetail.CreatedById = userId;

                                    lstTdcAuthCustomerDetailAdd.Add(tdcAuthCustomerDetail);
                                }
                                else
                                {
                                    tdcAuthCustomerDetail.CreatedAt = tdcAuthCustomerDetailExist.CreatedAt;
                                    tdcAuthCustomerDetail.CreatedBy = tdcAuthCustomerDetailExist.CreatedBy;
                                    tdcAuthCustomerDetail.CreatedById = tdcAuthCustomerDetailExist.UpdatedById;
                                    tdcAuthCustomerDetail.TdcCustomerId = input.Id;
                                    tdcAuthCustomerDetail.UpdatedById = userId;
                                    tdcAuthCustomerDetail.UpdatedBy = fullName;

                                    lstTdcAuthCustomerDetailUpdate.Add(tdcAuthCustomerDetail);
                                    tdcAuthCustomerDetails.Remove(tdcAuthCustomerDetailExist);
                                }
                            }
                        }
                        foreach (var itemTdcAuthCustomerDetail in tdcAuthCustomerDetails) {
                            itemTdcAuthCustomerDetail.UpdatedAt = DateTime.Now;
                            itemTdcAuthCustomerDetail.UpdatedById = userId;
                            itemTdcAuthCustomerDetail.UpdatedBy = fullName;
                            itemTdcAuthCustomerDetail.Status = AppEnums.EntityStatus.DELETED;

                            lstTdcAuthCustomerDetailUpdate.Add(itemTdcAuthCustomerDetail);
                        }
                        _context.TdcAuthCustomerDetails.UpdateRange(lstTdcAuthCustomerDetailUpdate);
                        _context.TdcAuthCustomerDetails.AddRange(lstTdcAuthCustomerDetailAdd);

                        //////
                        List<TdcMemberCustomer> lstTdcMemberCustomerAdd = new List<TdcMemberCustomer>();
                        List<TdcMemberCustomer> lstTdcMemberCustomerlUpdate = new List<TdcMemberCustomer>();

                        List<TdcMemberCustomer> tdcMemberCustomers = _context.TdcMemberCustomers.AsNoTracking().Where(l => l.TdcCustomerId == input.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();

                        if (input.tdcMenberCustomerDatas != null)
                        {
                            foreach (var tdcMenberCustomerData in input.tdcMenberCustomerDatas)
                            {
                                TdcMemberCustomer tdcTdcMemberCustomerExist = tdcMemberCustomers.Where(l => l.Id == tdcMenberCustomerData.Id).FirstOrDefault();
                                if (tdcTdcMemberCustomerExist == null)
                                {
                                    tdcMenberCustomerData.TdcCustomerId = input.Id;
                                    tdcMenberCustomerData.CreatedBy = fullName;
                                    tdcMenberCustomerData.CreatedById = userId;

                                    lstTdcMemberCustomerAdd.Add(tdcMenberCustomerData);
                                }
                                else
                                {
                                    tdcMenberCustomerData.CreatedAt = tdcMenberCustomerData.CreatedAt;
                                    tdcMenberCustomerData.CreatedBy = tdcMenberCustomerData.CreatedBy;
                                    tdcMenberCustomerData.CreatedById = tdcMenberCustomerData.UpdatedById;
                                    tdcMenberCustomerData.TdcCustomerId = input.Id;
                                    tdcMenberCustomerData.UpdatedById = userId;
                                    tdcMenberCustomerData.UpdatedBy = fullName;

                                    lstTdcMemberCustomerlUpdate.Add(tdcMenberCustomerData);
                                    tdcMemberCustomers.Remove(tdcTdcMemberCustomerExist);
                                }
                            }
                        }
                        foreach (var itemTdcMemberCustomers in tdcMemberCustomers)
                        {
                            itemTdcMemberCustomers.UpdatedAt = DateTime.Now;
                            itemTdcMemberCustomers.UpdatedById = userId;
                            itemTdcMemberCustomers.UpdatedBy = fullName;
                            itemTdcMemberCustomers.Status = AppEnums.EntityStatus.DELETED;

                            lstTdcMemberCustomerlUpdate.Add(itemTdcMemberCustomers);
                        }
                        _context.TdcMemberCustomers.UpdateRange(lstTdcMemberCustomerlUpdate);
                        _context.TdcMemberCustomers.AddRange(lstTdcMemberCustomerAdd);

                        LogActionModel logActionModel = new LogActionModel("Chỉnh sửa thông tin Khách Hàng " + input.FullName, "TdcCustomer", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (input.Id > 0) transaction.Commit();
                        else transaction.Rollback();

                        def.meta = new Meta(200, ApiConstants.MessageResource.UPDATE_SUCCESS);
                        def.data = data;
                        return Ok(def);
                    }
                    catch(DbUpdateException e)
                    {
                        transaction.Rollback();
                        log.Error("DbUpdateException:" + e);
                        if (!TdcCustomerExists(data.Id))
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
            catch(Exception e)
            {
                log.Error("Put Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

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
                TdcCustomer data = await _context.TdcCustomers.FindAsync(id);
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_DELETE_MESSAGE);
                    return Ok(def);
                }

                //Check
                TdcPriceRent TdcPriceRentExist = _context.TdcPriceRents.Where(b => b.TdcCustomerId == id && b.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                TdcPriceOneSell TdcPriceOneSellExist = _context.TdcPriceOneSells.Where(b => b.TdcCustomerId == id && b.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                TDCInstallmentPrice TDCInstallmentPriceExist = _context.TDCInstallmentPrices.Where(b => b.TdcCustomerId == id && b.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                if (TdcPriceRentExist != null || TdcPriceOneSellExist != null || TDCInstallmentPriceExist != null)
                {
                    def.meta = new Meta(222, "Dữ liệu liên quan còn tồn tại trong hồ sơ. Không thể xóa bản ghi này!");
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction()) 
                {
                    data.UpdatedAt = DateTime.Now;
                    data.UpdatedBy = fullName;
                    data.UpdatedById = userId;
                    data.Status = AppEnums.EntityStatus.DELETED;
                    _context.Update(data);

                    List<TdcMemberCustomer> tdcMemberCustomers = _context.TdcMemberCustomers.Where(l => l.TdcCustomerId == data.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if (tdcMemberCustomers.Count > 0)
                    {
                        tdcMemberCustomers.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(tdcMemberCustomers);
                    }

                    List<TdcAuthCustomerDetail> tdcAuthCustomerDetails = _context.TdcAuthCustomerDetails.Where(l => l.TdcCustomerId == data.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if (tdcAuthCustomerDetails.Count > 0)
                    {
                        tdcAuthCustomerDetails.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(tdcAuthCustomerDetails);
                    }

                    List<TdcCustomerFile> tdcCustomerFiles = _context.TdcCustomerFiles.Where(l => l.TdcCustomerId == data.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if (tdcCustomerFiles.Count > 0)
                    {
                        tdcCustomerFiles.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(tdcCustomerFiles);
                    }
                    try
                    {
                        await _context.SaveChangesAsync();

                        LogActionModel logActionModel = new LogActionModel("Xóa Khách Hàng" + data.FullName, "TdcCustomer", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.DELETED, userId,fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (data.Id > 0) transaction.Commit();

                        else transaction.Rollback();

                        def.meta = new Meta(200, ApiConstants.MessageResource.DELETE_SUCCESS);
                        def.data = data;
                        return Ok(def);
                    }
                    catch(DbUpdateException ex)
                    {
                        transaction.Rollback();
                        log.Error("DbUpdateException" + ex);
                        if (!TdcCustomerExists(data.Id))
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
            catch(Exception e)
            {
                def.meta=new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        private bool TdcCustomerExists(int id)
        {
            return _context.TdcCustomers.Count(e => e.Id == id) > 0;
        }
        
        [HttpPost("ExportExcel")]
        public async Task<IActionResult> ExportExcel()
        {
            string accessToken = Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                return Unauthorized();
            }

            var identity = (ClaimsIdentity)User.Identity;
            int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id").Select(c => c.Value).SingleOrDefault());
            string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();

            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.EXPORT))
            {
                return new FileContentResult(new byte[0], "application/octet-stream");
            }

            List<TdcCustomer> tdcCustomers = _context.TdcCustomers.Where(p => p.Status != AppEnums.EntityStatus.DELETED).ToList();
            List<TdcCustomerData> mapper_customer = _mapper.Map<List<TdcCustomerData>>(tdcCustomers);
            foreach (TdcCustomerData item in mapper_customer)
            {
                //Thông tin nhà
                item.TdcProjectName = _context.TDCProjects.Where(f => f.Id == item.TdcProjectId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                item.TdcLandName = _context.Lands.Where(f => f.Id == item.LandId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                item.TdcBlockHouseName = _context.BlockHouses.Where(f => f.Id == item.BlockHouseId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                item.TdcFloorName = _context.FloorTdcs.Where(f => f.Id == item.FloorTdcId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();

                item.TdcApartmentName = _context.ApartmentTdcs.Where(f => f.Id == item.TdcApartmentId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();


                //Địa Chỉ Tạm Trú Khách Hàng
                item.fullAddressTT = item.AddressTT;

                Ward wardTT = _context.Wards.Where(x => x.Id == item.WardTT).FirstOrDefault();
                item.fullAddressTT = item.fullAddressTT != null && item.fullAddressTT != "" ? (wardTT != null ? String.Join(",", item.fullAddressTT, wardTT.Name) : item.fullAddressTT) : (wardTT != null ? wardTT.Name : item.fullAddressTT);

                District districtTT = _context.Districts.Where(x => x.Id == item.DistrictTT).FirstOrDefault();
                item.fullAddressTT = item.fullAddressTT != null && item.fullAddressTT != "" ? (districtTT != null ? String.Join(",", item.fullAddressTT, districtTT.Name) : item.fullAddressTT) : (districtTT != null ? districtTT.Name : item.fullAddressTT);

                Province provinceTT = _context.Provincies.Where(x => x.Id == item.ProvinceTT).FirstOrDefault();
                item.fullAddressTT = item.fullAddressTT != null && item.fullAddressTT != "" ? (provinceTT != null ? String.Join(",", item.fullAddressTT, provinceTT.Name) : item.fullAddressTT) : (provinceTT != null ? provinceTT.Name : item.fullAddressTT);

                Lane laneTT = _context.Lanies.Where(x => x.Id == item.LaneTT).FirstOrDefault();
                item.LaneTt = laneTT != null ? laneTT.Name : "";

                //Địa Chỉ Liên hệ Khách Hàng
                item.fullAddressLH = item.AddressLH;

                Ward wardLH = _context.Wards.Where(x => x.Id == item.WardLH).FirstOrDefault();
                item.fullAddressLH = item.fullAddressLH != null && item.fullAddressLH != "" ? (wardLH != null ? String.Join(",", item.fullAddressLH, wardLH.Name) : item.fullAddressLH) : (wardLH != null ? wardLH.Name : item.fullAddressLH);

                District districtLH = _context.Districts.Where(x => x.Id == item.DistrictLH).FirstOrDefault();
                item.fullAddressLH = item.fullAddressLH != null && item.fullAddressLH != "" ? (districtLH != null ? String.Join(",", item.fullAddressLH, districtLH.Name) : item.fullAddressLH) : (districtLH != null ? districtLH.Name : item.fullAddressLH);

                Province provinceLH = _context.Provincies.Where(x => x.Id == item.ProvinceLH).FirstOrDefault();
                item.fullAddressLH = item.fullAddressLH != null && item.fullAddressLH != "" ? (provinceLH != null ? String.Join(",", item.fullAddressLH, provinceLH.Name) : item.fullAddressLH) : (provinceLH != null ? provinceLH.Name : item.fullAddressLH);

                Lane laneLH = _context.Lanies.Where(x => x.Id == item.LaneLH).FirstOrDefault();
                item.LaneLh = laneLH != null ? laneLH.Name : "";

                item.tdcCustomerFiles = _context.TdcCustomerFiles.Where(l => l.TdcCustomerId == item.Id && l.Status != AppEnums.EntityStatus.DELETED).ToList();

                //Danh sách người thân của khách hàng
                List<TdcMemberCustomer> tdcMemberCustomers = _context.TdcMemberCustomers.Where(x => x.TdcCustomerId == item.Id && x.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<TdcMenberCustomerData> map_tdcMemberCustomerDatas = _mapper.Map<List<TdcMenberCustomerData>>(tdcMemberCustomers);
                foreach(TdcMenberCustomerData map_tdcMemberCustomerData in map_tdcMemberCustomerDatas)
                {
                    Lane landTt = _context.Lanies.Where(x => x.Id == map_tdcMemberCustomerData.LaneTt).FirstOrDefault();
                    map_tdcMemberCustomerData.LaneTT = landTt != null ? landTt.Name : "";

                    Ward wardTt = _context.Wards.Where(x => x.Id == map_tdcMemberCustomerData.WardTt).FirstOrDefault();
                    map_tdcMemberCustomerData.fullAddressTt = map_tdcMemberCustomerData.fullAddressTt != null && map_tdcMemberCustomerData.fullAddressTt != "" ? (wardTt != null ? String.Join(",", map_tdcMemberCustomerData.fullAddressTt, wardTt.Name) : map_tdcMemberCustomerData.fullAddressTt) : (wardTt != null ? wardTt.Name : map_tdcMemberCustomerData.fullAddressTt);


                    District districtTt = _context.Districts.Where(x => x.Id == map_tdcMemberCustomerData.DistrictTt).FirstOrDefault();
                    map_tdcMemberCustomerData.fullAddressTt = map_tdcMemberCustomerData.fullAddressTt != null && map_tdcMemberCustomerData.fullAddressTt != "" ? (districtTt != null ? String.Join(",", map_tdcMemberCustomerData.fullAddressTt, districtTt.Name) : map_tdcMemberCustomerData.fullAddressTt) : (districtTt != null ? districtTt.Name : map_tdcMemberCustomerData.fullAddressTt);

                    Province provinceTt = _context.Provincies.Where(x => x.Id == map_tdcMemberCustomerData.ProvinceTt).FirstOrDefault();
                    map_tdcMemberCustomerData.fullAddressTt = map_tdcMemberCustomerData.fullAddressTt != null && map_tdcMemberCustomerData.fullAddressTt != "" ? (provinceTt != null ? String.Join(",", map_tdcMemberCustomerData.fullAddressTt, provinceTt.Name) : map_tdcMemberCustomerData.fullAddressTt) : (provinceTt != null ? provinceTt.Name : map_tdcMemberCustomerData.fullAddressTt);

                    //Địa Chỉ Liên Hệ Người Ủy Quyền

                    //map_tdcMemberCustomerData.fullAddressLh = map_tdcMemberCustomerData.AddressLh;

                    Lane landLh = _context.Lanies.Where(x => x.Id == map_tdcMemberCustomerData.LaneLh).FirstOrDefault();
                    map_tdcMemberCustomerData.LaneLH = landLh != null ? landLh.Name : "";

                    Ward wardLh = _context.Wards.Where(x => x.Id == map_tdcMemberCustomerData.WardLh).FirstOrDefault();
                    map_tdcMemberCustomerData.fullAddressLh = map_tdcMemberCustomerData.fullAddressLh != null && map_tdcMemberCustomerData.fullAddressLh != "" ? (wardLh != null ? String.Join(",", map_tdcMemberCustomerData.fullAddressLh, wardLh.Name) : map_tdcMemberCustomerData.fullAddressLh) : (wardLh != null ? wardLh.Name : map_tdcMemberCustomerData.fullAddressLh);

                    District districtLh = _context.Districts.Where(x => x.Id == map_tdcMemberCustomerData.DistrictLh).FirstOrDefault();
                    map_tdcMemberCustomerData.fullAddressLh = map_tdcMemberCustomerData.fullAddressLh != null && map_tdcMemberCustomerData.fullAddressLh != "" ? (districtLh != null ? String.Join(",", map_tdcMemberCustomerData.fullAddressLh, districtLh.Name) : map_tdcMemberCustomerData.fullAddressLh) : (districtLh != null ? districtLh.Name : map_tdcMemberCustomerData.fullAddressLh);

                    Province provinceLh = _context.Provincies.Where(x => x.Id == map_tdcMemberCustomerData.ProvinceLh).FirstOrDefault();
                    map_tdcMemberCustomerData.fullAddressLh = map_tdcMemberCustomerData.fullAddressLh != null && map_tdcMemberCustomerData.fullAddressLh != "" ? (provinceLh != null ? String.Join(",", map_tdcMemberCustomerData.fullAddressLh, provinceLh.Name) : map_tdcMemberCustomerData.fullAddressLh) : (provinceLh != null ? provinceLh.Name : map_tdcMemberCustomerData.fullAddressLh);

                }
                item.tdcMenberCustomerDatas = map_tdcMemberCustomerDatas.OrderBy(x => x.TdcCustomerId).ToList();

                List<TdcAuthCustomerDetail> tdcAuthCustomerDetails = _context.TdcAuthCustomerDetails.Where(x => x.TdcCustomerId == item.Id && x.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<TdcAuthCustomerDetailData> map_tdcAuthCustomerDetailDatas = _mapper.Map<List<TdcAuthCustomerDetailData>>(tdcAuthCustomerDetails);
                foreach (TdcAuthCustomerDetailData map_tdcAuthCustomerDetailData in map_tdcAuthCustomerDetailDatas)
                {
                    //Địa Chỉ Tạm Trú Người ủy quyền
                    //map_tdcAuthCustomerDetailData.fullAddressTt = map_tdcAuthCustomerDetailData.AddressTt;

                    Lane landTt = _context.Lanies.Where(x => x.Id == map_tdcAuthCustomerDetailData.LaneTt).FirstOrDefault();
                    map_tdcAuthCustomerDetailData.LaneTT = landTt != null ? landTt.Name : "";

                    Ward wardTt = _context.Wards.Where(x => x.Id == map_tdcAuthCustomerDetailData.WardTt).FirstOrDefault();
                    map_tdcAuthCustomerDetailData.fullAddressTt = map_tdcAuthCustomerDetailData.fullAddressTt != null && map_tdcAuthCustomerDetailData.fullAddressTt != "" ? (wardTt != null ? String.Join(",", map_tdcAuthCustomerDetailData.fullAddressTt, wardTt.Name) : map_tdcAuthCustomerDetailData.fullAddressTt) : (wardTt != null ? wardTt.Name : map_tdcAuthCustomerDetailData.fullAddressTt);


                    District districtTt = _context.Districts.Where(x => x.Id == map_tdcAuthCustomerDetailData.DistrictTt).FirstOrDefault();
                    map_tdcAuthCustomerDetailData.fullAddressTt = map_tdcAuthCustomerDetailData.fullAddressTt != null && map_tdcAuthCustomerDetailData.fullAddressTt != "" ? (districtTt != null ? String.Join(",", map_tdcAuthCustomerDetailData.fullAddressTt, districtTt.Name) : map_tdcAuthCustomerDetailData.fullAddressTt) : (districtTt != null ? districtTt.Name : map_tdcAuthCustomerDetailData.fullAddressTt);

                    Province provinceTt = _context.Provincies.Where(x => x.Id == map_tdcAuthCustomerDetailData.ProvinceTt).FirstOrDefault();
                    map_tdcAuthCustomerDetailData.fullAddressTt = map_tdcAuthCustomerDetailData.fullAddressTt != null && map_tdcAuthCustomerDetailData.fullAddressTt != "" ? (provinceTt != null ? String.Join(",", map_tdcAuthCustomerDetailData.fullAddressTt, provinceTt.Name) : map_tdcAuthCustomerDetailData.fullAddressTt) : (provinceTt != null ? provinceTt.Name : map_tdcAuthCustomerDetailData.fullAddressTt);

                    //Địa Chỉ Liên Hệ Người Ủy Quyền

                    //map_tdcAuthCustomerDetailData.fullAddressLh = map_tdcAuthCustomerDetailData.AddressLh;

                    Lane landLh = _context.Lanies.Where(x => x.Id == map_tdcAuthCustomerDetailData.LaneLh).FirstOrDefault();
                    map_tdcAuthCustomerDetailData.LaneLH = landLh != null ? landLh.Name : "";

                    Ward wardLh = _context.Wards.Where(x => x.Id == map_tdcAuthCustomerDetailData.WardLh).FirstOrDefault();
                    map_tdcAuthCustomerDetailData.fullAddressLh = map_tdcAuthCustomerDetailData.fullAddressLh != null && map_tdcAuthCustomerDetailData.fullAddressLh != "" ? (wardLh != null ? String.Join(",", map_tdcAuthCustomerDetailData.fullAddressLh, wardLh.Name) : map_tdcAuthCustomerDetailData.fullAddressLh) : (wardLh != null ? wardLh.Name : map_tdcAuthCustomerDetailData.fullAddressLh);

                    District districtLh = _context.Districts.Where(x => x.Id == map_tdcAuthCustomerDetailData.DistrictLh).FirstOrDefault();
                    map_tdcAuthCustomerDetailData.fullAddressLh = map_tdcAuthCustomerDetailData.fullAddressLh != null && map_tdcAuthCustomerDetailData.fullAddressLh != "" ? (districtLh != null ? String.Join(",", map_tdcAuthCustomerDetailData.fullAddressLh, districtLh.Name) : map_tdcAuthCustomerDetailData.fullAddressLh) : (districtLh != null ? districtLh.Name : map_tdcAuthCustomerDetailData.fullAddressLh);

                    Province provinceLh = _context.Provincies.Where(x => x.Id == map_tdcAuthCustomerDetailData.ProvinceLh).FirstOrDefault();
                    map_tdcAuthCustomerDetailData.fullAddressLh = map_tdcAuthCustomerDetailData.fullAddressLh != null && map_tdcAuthCustomerDetailData.fullAddressLh != "" ? (provinceLh != null ? String.Join(",", map_tdcAuthCustomerDetailData.fullAddressLh, provinceLh.Name) : map_tdcAuthCustomerDetailData.fullAddressLh) : (provinceLh != null ? provinceLh.Name : map_tdcAuthCustomerDetailData.fullAddressLh);
                }
                item.tdcAuthCustomerDetailDatas = map_tdcAuthCustomerDetailDatas.OrderBy(x => x.TdcCustomerId).ToList();
            }

            // khởi tạo wb rỗng
            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"templates/CustomerTdc.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);

            MemoryStream ms = WriteDataToExcel(templatePath, 0, mapper_customer);
            byte[] byteArrayContent = ms.ToArray();
            return new FileContentResult(byteArrayContent, "application/octet-stream");
        }

        private static MemoryStream WriteDataToExcel(string templatePath, int sheetnumber, List<TdcCustomerData> datas)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowStart = 2;

            if (sheet != null)
            {
                int datacol = 20;
                try
                {
                    //style body
                    List<ICellStyle> rowStyle = new List<ICellStyle>();
                    for (int i = 0; i < datacol; i++)
                    {
                        rowStyle.Add(sheet.GetRow(0).GetCell(i).CellStyle);
                    }
                    //Thêm row
                    int k = 0;
                    foreach (var item in datas)
                    {
                        k++;
                        
                        XSSFRow row = (XSSFRow)sheet.CreateRow(rowStart);
                        for (int i = 0; i < datacol; i++)
                        {
                            row.CreateCell(i).CellStyle = rowStyle[i];

                            if (i == 0)
                            {
                                row.GetCell(i).SetCellValue(k);
                            }
                            else if (i == 1)
                            {
                                row.GetCell(i).SetCellValue(item.TdcProjectName);
                            }
                            else if (i == 2)
                            {
                                row.GetCell(i).SetCellValue(item.TdcLandName);
                            }
                            else if (i == 3)
                            {
                                row.GetCell(i).SetCellValue(item.TdcBlockHouseName);
                            }
                            else if (i == 4)
                            {
                                row.GetCell(i).SetCellValue(item.TdcFloorName);
                            }
                            else if (i == 5)
                            {
                                row.GetCell(i).SetCellValue(item.Floor1);
                            }
                            else if (i == 6)
                            {
                                row.GetCell(i).SetCellValue(item.TdcApartmentName);
                            }
                            else if (i == 7)
                            {
                                if (item.Corner == true)
                                {
                                    row.GetCell(i).SetCellValue("X");
                                }
                                else
                                {
                                    row.GetCell(i).SetCellValue(" ");
                                }
                            }
                            else if (i == 8)
                            {
                                row.GetCell(i).SetCellValue(item.FullName);
                            }
                            else if (i == 9)
                            {
                                row.GetCell(i).SetCellValue(item.CCCD);
                            }
                            else if (i == 10)
                            {
                                row.GetCell(i).SetCellValue(item.Dob.ToString("dd/MM/yyyy"));
                            }
                            else if (i == 11)
                            {
                                row.GetCell(i).SetCellValue(item.Phone);
                            }
                            else if (i == 12)
                            {
                                row.GetCell(i).SetCellValue(item.Email);
                            }
                            else if (i == 13)
                            {
                                row.GetCell(i).SetCellValue(item.Note);
                            }
                            else if (i == 14)
                            {
                                row.GetCell(i).SetCellValue(item.AddressTT);
                            }
                            else if (i == 15)
                            {
                                row.GetCell(i).SetCellValue(item.fullAddressTT);
                            }
                            else if (i == 16)
                            {
                                row.GetCell(i).SetCellValue(item.LaneTt);
                            }
                            else if (i == 17)
                            {
                                row.GetCell(i).SetCellValue(item.AddressLH);
                            }
                            else if (i == 18)
                            {
                                row.GetCell(i).SetCellValue(item.fullAddressLH);
                            }
                            else if (i == 19)
                            {
                                row.GetCell(i).SetCellValue(item.LaneLh);
                            }
                        }
                        rowStart++;

                        ICellStyle style = workbook.CreateCellStyle();
                        style.FillForegroundColor = HSSFColor.PaleBlue.Index;
                        style.FillPattern = FillPattern.SolidForeground;
                        style.Alignment = HorizontalAlignment.Center;
                        style.VerticalAlignment = VerticalAlignment.Center;
                        style.BorderBottom = BorderStyle.Thin;
                        style.BorderTop = BorderStyle.Thin;
                        style.BorderLeft = BorderStyle.Thin;
                        style.BorderRight = BorderStyle.Thin;

                        ICellStyle styleValue = workbook.CreateCellStyle();
                        styleValue.BorderBottom = BorderStyle.Thin;
                        styleValue.BorderTop = BorderStyle.Thin;
                        styleValue.BorderLeft = BorderStyle.Thin;
                        styleValue.BorderRight = BorderStyle.Thin;

                        ICellStyle styleDate = workbook.CreateCellStyle();
                        styleDate.DataFormat = workbook.CreateDataFormat().GetFormat("dd/MM/yyyy");
                        styleValue.BorderBottom = BorderStyle.Thin;
                        styleValue.BorderTop = BorderStyle.Thin;
                        styleValue.BorderLeft = BorderStyle.Thin;
                        styleValue.BorderRight = BorderStyle.Thin;

                        IRow rowTitle = sheet.CreateRow(rowStart);

                        ICell celltitle = rowTitle.CreateCell(0);
                        celltitle.SetCellValue("Danh sách thành viên");
                        celltitle.CellStyle = style;
                        CellRangeAddress mergedRegioncellHeader = new CellRangeAddress(rowStart, rowStart, 0, 1);
                        RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader, sheet);
                        RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader, sheet);
                        RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader, sheet);
                        RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader, sheet);
                        sheet.AddMergedRegion(mergedRegioncellHeader);

                        ICell celltitle1 = rowTitle.CreateCell(2);
                        celltitle1.SetCellValue("Họ tên");
                        celltitle1.CellStyle = style;

                        ICell celltitle2 = rowTitle.CreateCell(3);
                        celltitle2.SetCellValue("CCCD/CMND");
                        celltitle2.CellStyle = style;

                        ICell celltitle3 = rowTitle.CreateCell(4);
                        celltitle3.SetCellValue("Ngày sinh");
                        celltitle3.CellStyle = style;

                        ICell celltitle4 = rowTitle.CreateCell(5);
                        celltitle4.SetCellValue("Số nhà");
                        celltitle4.CellStyle = style;

                        ICell celltitle5 = rowTitle.CreateCell(6);
                        celltitle5.SetCellValue("Tỉnh (Tp) /Quận (huyện) /Phường (xã)- Địa chỉ tạm chú");
                        celltitle5.CellStyle = style;

                        ICell celltitle6 = rowTitle.CreateCell(7);
                        celltitle6.SetCellValue("Tên đường");
                        celltitle6.CellStyle = style;

                        ICell celltitle7 = rowTitle.CreateCell(8);
                        celltitle7.SetCellValue("Số nhà");
                        celltitle7.CellStyle = style;

                        ICell celltitle8 = rowTitle.CreateCell(9);
                        celltitle8.SetCellValue("Tỉnh (Tp) /Quận (huyện) /Phường (xã)- Địa chỉ lưu trú");
                        celltitle8.CellStyle = style;

                        ICell celltitle9 = rowTitle.CreateCell(10);
                        celltitle9.SetCellValue("Tên đường");
                        celltitle9.CellStyle = style;

                        ICell celltitle10 = rowTitle.CreateCell(11);
                        celltitle10.SetCellValue("Số điện thoại");
                        celltitle10.CellStyle = style;

                        ICell celltitle11 = rowTitle.CreateCell(12);
                        celltitle11.SetCellValue("Email");
                        celltitle11.CellStyle = style;

                        ICell celltitle12 = rowTitle.CreateCell(13);
                        celltitle12.SetCellValue("Ghi chú");
                        celltitle12.CellStyle = style;

                        rowStart++;


                        IRow rowRange = sheet.CreateRow(rowStart);

                        
                        int colMem = 2;
                        foreach (var memItem in item.tdcMenberCustomerDatas)
                        {
                            ICell cell1 = rowRange.CreateCell(2);
                            cell1.SetCellValue(memItem.FullName);
                            cell1.CellStyle = styleValue;

                            ICell cell2 = rowRange.CreateCell(3);
                            cell2.SetCellValue(memItem.CCCD);
                            cell2.CellStyle = styleValue;

                            ICell cell3 = rowRange.CreateCell(4);
                            cell3.SetCellValue(memItem.Dob);
                            cell3.CellStyle = styleDate;

                            ICell cell4 = rowRange.CreateCell(5);
                            cell4.SetCellValue(memItem.AddressTt);
                            cell4.CellStyle = styleValue;

                            ICell cell5 = rowRange.CreateCell(6);
                            cell5.SetCellValue(memItem.fullAddressTt);
                            cell5.CellStyle = styleValue;

                            ICell cell6 = rowRange.CreateCell(7);
                            cell6.SetCellValue(memItem.LaneTT);
                            cell6.CellStyle = styleValue;

                            ICell cell7 = rowRange.CreateCell(8);
                            cell7.SetCellValue(memItem.AddressLh);
                            cell7.CellStyle = styleValue;

                            ICell cell8 = rowRange.CreateCell(9);
                            cell8.SetCellValue(memItem.fullAddressLh);
                            cell8.CellStyle = styleValue;

                            ICell cell9 = rowRange.CreateCell(10);
                            cell9.SetCellValue(memItem.LaneLH);
                            cell9.CellStyle = styleValue;

                            ICell cell10 = rowRange.CreateCell(11);
                            cell10.SetCellValue(memItem.Phone);
                            cell10.CellStyle = styleValue;

                            ICell cell11 = rowRange.CreateCell(12);
                            cell11.SetCellValue(memItem.Email);
                            cell11.CellStyle = styleValue;

                            ICell cell12 = rowRange.CreateCell(13);
                            cell12.SetCellValue(memItem.Note);
                            cell12.CellStyle = styleValue;
                            colMem++;
                        }
                        rowStart++;


                        IRow rowRange1 = sheet.CreateRow(rowStart);

                        ICell cellName = rowRange1.CreateCell(1);
                        cellName.SetCellValue("Họ tên");
                        cellName.CellStyle = style;
                        CellRangeAddress mergedRegioncellHeader1 = new CellRangeAddress(rowStart, rowStart, 1, 2);
                        RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                        RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                        RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                        RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader1, sheet);
                        sheet.AddMergedRegion(mergedRegioncellHeader1);

                        int colName = 3;
                        foreach( var j in item.tdcAuthCustomerDetailDatas)
                        {
                            ICell cellName1 = rowRange1.CreateCell(3);
                            cellName1.SetCellValue(j.FullName);
                            cellName1.CellStyle = styleValue;
                            colName++;
                        }

                        rowStart++;

                        IRow rowRange2 = sheet.CreateRow(rowStart);

                        ICell cellNameID = rowRange2.CreateCell(1);
                        cellNameID.SetCellValue("CCCD/CMND");
                        cellNameID.CellStyle = style;
                        CellRangeAddress mergedRegioncellHeader2 = new CellRangeAddress(rowStart, rowStart, 1, 2);
                        RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader2, sheet);
                        RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader2, sheet);
                        RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader2, sheet);
                        RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader2, sheet);
                        sheet.AddMergedRegion(mergedRegioncellHeader2);

                        int colNameID = 3;
                        foreach (var j in item.tdcAuthCustomerDetailDatas)
                        {
                            ICell cellNameID1 = rowRange2.CreateCell(3);
                            cellNameID1.SetCellValue(j.CCCD);
                            cellNameID1.CellStyle = styleValue;
                            colNameID++;
                        }

                        rowStart++;

                        IRow rowRange3 = sheet.CreateRow(rowStart);

                        ICell cellDate = rowRange3.CreateCell(1);
                        cellDate.SetCellValue("Ngày sinh");
                        cellDate.CellStyle = style;
                        CellRangeAddress mergedRegioncellHeader3 = new CellRangeAddress(rowStart, rowStart, 1, 2);
                        RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegioncellHeader3, sheet);
                        RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegioncellHeader3, sheet);
                        RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegioncellHeader3, sheet);
                        RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegioncellHeader3, sheet);
                        sheet.AddMergedRegion(mergedRegioncellHeader3);

                        int colDate = 3;
                        foreach (var j in item.tdcAuthCustomerDetailDatas)
                        {
                            ICell cellDate1 = rowRange3.CreateCell(3);
                            cellDate1.SetCellValue(j.Dob);
                            cellDate1.CellStyle = styleDate;
                            colDate++;
                        }

                        rowStart++;

                        IRow rowRange4 = sheet.CreateRow(rowStart);
                        
                        ICell cellNbHouseTT1 = rowRange4.CreateCell(2);
                        cellNbHouseTT1.SetCellValue("Số nhà");
                        cellNbHouseTT1.CellStyle = style;

                        int colNbHouseTT = 3;
                        foreach (var j in item.tdcAuthCustomerDetailDatas)
                        {
                            ICell cellNbHouseTT2 = rowRange4.CreateCell(3);
                            cellNbHouseTT2.SetCellValue(j.AddressTt);
                            cellNbHouseTT2.CellStyle = styleValue;
                            colNbHouseTT++;
                        }
                        rowStart++;

                        IRow rowRange5 = sheet.CreateRow(rowStart);

                        ICell cellNbHouseTT3 = rowRange5.CreateCell(2);
                        cellNbHouseTT3.SetCellValue("Tỉnh (Tp) /Quận (huyện) /Phường (xã)");
                        cellNbHouseTT3.CellStyle = style;

                        int colAddressTT = 3;
                        foreach (var j in item.tdcAuthCustomerDetailDatas)
                        {
                            ICell cellNbHouseTT4 = rowRange5.CreateCell(3);
                            cellNbHouseTT4.SetCellValue(j.fullAddressTt);
                            cellNbHouseTT4.CellStyle = styleValue;
                            colAddressTT++;
                        }
                        rowStart++;

                        IRow rowRange6 = sheet.CreateRow(rowStart);

                        ICell cellNbHouseTT5 = rowRange6.CreateCell(2);
                        cellNbHouseTT5.SetCellValue("Đường");
                        cellNbHouseTT5.CellStyle = style;

                        int colLaneTT = 3;
                        foreach (var j in item.tdcAuthCustomerDetailDatas)
                        {
                            ICell cellNbHouseTT6 = rowRange6.CreateCell(3);
                            cellNbHouseTT6.SetCellValue(j.LaneTT);
                            cellNbHouseTT6.CellStyle = styleValue;
                            colLaneTT++;
                        }
                        rowStart++;

                        CellRangeAddress mergedRegionTT = new CellRangeAddress(rowRange4.RowNum, rowRange6.RowNum, 1, 1);
                        RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegionTT, sheet);
                        RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegionTT, sheet);
                        RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegionTT, sheet);
                        RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegionTT, sheet);
                        sheet.AddMergedRegion(mergedRegionTT);

                        ICell cellAddressTT = rowRange4.CreateCell(1);
                        cellAddressTT.SetCellValue("Địa chỉ tạm trú");
                        cellAddressTT.CellStyle = style;

                        IRow rowRange7 = sheet.CreateRow(rowStart);

                        ICell cellNbHouseTT7 = rowRange7.CreateCell(2);
                        cellNbHouseTT7.SetCellValue("Số nhà");
                        cellNbHouseTT7.CellStyle = style;

                        int colNbHouseLH = 3;
                        foreach (var j in item.tdcAuthCustomerDetailDatas)
                        {
                            ICell cellNbHouseTT8 = rowRange7.CreateCell(3);
                            cellNbHouseTT8.SetCellValue(j.AddressLh);
                            cellNbHouseTT8.CellStyle = styleValue;
                            colNbHouseLH++;
                        }
                        rowStart++;

                        IRow rowRange8 = sheet.CreateRow(rowStart);

                        ICell cellNbHouseTT9 = rowRange8.CreateCell(2);
                        cellNbHouseTT9.SetCellValue("Tỉnh (Tp) /Quận (huyện) /Phường (xã)");
                        cellNbHouseTT9.CellStyle = style;

                        int colAddressLH = 3;
                        foreach (var j in item.tdcAuthCustomerDetailDatas)
                        {
                            ICell cellNbHouseTT10 = rowRange8.CreateCell(3);
                            cellNbHouseTT10.SetCellValue(j.fullAddressLh);
                            cellNbHouseTT10.CellStyle = styleValue;
                            colAddressLH++;
                        }
                        rowStart++;

                        IRow rowRange9 = sheet.CreateRow(rowStart);

                        ICell cellNbHouseTT11 = rowRange9.CreateCell(2);
                        cellNbHouseTT11.SetCellValue("Đường");
                        cellNbHouseTT11.CellStyle = style;

                        int colLaneLH = 3;
                        foreach (var j in item.tdcAuthCustomerDetailDatas)
                        {
                            ICell cellNbHouseTT12 = rowRange9.CreateCell(3);
                            cellNbHouseTT12.SetCellValue(j.LaneLH);
                            cellNbHouseTT12.CellStyle = styleValue;
                            colLaneLH++;
                        }
                        rowStart++;

                        CellRangeAddress mergedRegionLH = new CellRangeAddress(rowRange7.RowNum, rowRange9.RowNum, 1, 1);
                        RegionUtil.SetBorderTop((int)BorderStyle.Thin, mergedRegionLH, sheet);
                        RegionUtil.SetBorderLeft((int)BorderStyle.Thin, mergedRegionLH, sheet);
                        RegionUtil.SetBorderRight((int)BorderStyle.Thin, mergedRegionLH, sheet);
                        RegionUtil.SetBorderBottom((int)BorderStyle.Thin, mergedRegionLH, sheet);
                        sheet.AddMergedRegion(mergedRegionLH);

                        ICell cellAddressLH = rowRange7.CreateCell(1);
                        cellAddressLH.SetCellValue("Địa chỉ tạm trú");
                        cellAddressLH.CellStyle = style;
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

        #region import hệ số lương
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
                importHistory.Type = AppEnums.ImportHistoryType.TdcCustomer;

                List<TdcCustomerDataImport> data = new List<TdcCustomerDataImport>();

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

                List<TdcCustomerDataImport> dataValid = new List<TdcCustomerDataImport>();
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var dataItem in data)
                        {
                            if (dataItem.Valid == true)
                            {
                                //Kiểm tra Tỉnh TP
                                string provinceNameNoneUnicodeTT = UtilsService.NonUnicode(dataItem.ProvinceNameTT);
                                Province provinceTT = provinces.AsEnumerable().Where(e => UtilsService.NonUnicode(e.Name).Contains(provinceNameNoneUnicodeTT)).FirstOrDefault();
                                if (provinceTT == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy tỉnh (tp)\n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.ProvinceTT = provinceTT.Id;
                                }

                                //Kiểm tra Quận huyện
                                string districtNameNoneUnicodeTT = UtilsService.NonUnicode(dataItem.DistrictNameTT);
                                District districtTT = districts.AsEnumerable().Where(e => e.ProvinceId == dataItem.ProvinceTT && UtilsService.NonUnicode(e.Name).Contains(districtNameNoneUnicodeTT)).FirstOrDefault();
                                if (districtTT == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy quận (huyện)\n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.DistrictTT = districtTT.Id;
                                }

                                //Kiểm tra phường xã
                                string wardNameNoneUnicodeTT = UtilsService.NonUnicode(dataItem.WardNameTT);
                                Ward wardTT = wards.AsEnumerable().Where(e => e.DistrictId == dataItem.DistrictTT && UtilsService.NonUnicode(e.Name).Contains(wardNameNoneUnicodeTT)).FirstOrDefault();
                                if (wardTT == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy phường (xã)\n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.WardTT = wardTT.Id;
                                }

                                //Kiểm tra duong
                                string laneNameNoneUnicodeTT = UtilsService.NonUnicode(dataItem.LaneTt);
                                Lane laneTT = lanes.AsEnumerable().Where(e => e.Ward == dataItem.WardTT && UtilsService.NonUnicode(e.Name).Contains(laneNameNoneUnicodeTT)).FirstOrDefault();
                                if (laneTT == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy đường\n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.LaneTT = laneTT.Id;
                                }

                                //Kiểm tra Tỉnh TP
                                string provinceNameNoneUnicode = UtilsService.NonUnicode(dataItem.ProvinceNameLH);
                                Province province = provinces.AsEnumerable().Where(e => UtilsService.NonUnicode(e.Name).Contains(provinceNameNoneUnicode)).FirstOrDefault();
                                if (province == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy tỉnh (tp)\n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.ProvinceLH = province.Id;
                                }

                                //Kiểm tra Quận huyện
                                string districtNameNoneUnicode = UtilsService.NonUnicode(dataItem.DistrictNameLH);
                                District district = districts.AsEnumerable().Where(e => e.ProvinceId == dataItem.ProvinceLH && UtilsService.NonUnicode(e.Name).Contains(districtNameNoneUnicode)).FirstOrDefault();
                                if (district == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy quận (huyện)\n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.DistrictLH = district.Id;
                                }

                                //Kiểm tra phường xã
                                string wardNameNoneUnicode = UtilsService.NonUnicode(dataItem.WardNameLH);
                                Ward ward = wards.AsEnumerable().Where(e => e.DistrictId == dataItem.DistrictLH && UtilsService.NonUnicode(e.Name).Contains(wardNameNoneUnicode)).FirstOrDefault();
                                if (ward == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy phường (xã)\n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.WardLH = ward.Id;
                                }

                                //Kiểm tra duong
                                string laneNameNoneUnicode = UtilsService.NonUnicode(dataItem.LaneLh);
                                Lane lane = lanes.AsEnumerable().Where(e => e.Ward == dataItem.WardLH && UtilsService.NonUnicode(e.Name).Contains(laneNameNoneUnicode)).FirstOrDefault();
                                if (lane == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy đường\n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.LaneLH = lane.Id;
                                }

                                TDCProject tDCProject = _context.TDCProjects.Where(m => m.Name == dataItem.TdcProjectName && m.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                if (tDCProject == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy Dự án \n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.TdcProjectId = tDCProject.Id;
                                }

                                Land land = _context.Lands.Where(m => m.Name == dataItem.TdcLandName && m.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                if (land == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy Lô \n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.LandId = land.Id;
                                }

                                BlockHouse blockHouse = _context.BlockHouses.Where(m => m.Name == dataItem.TdcBlockHouseName && m.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                if (blockHouse == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy khối \n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.BlockHouseId = blockHouse.Id;
                                }

                                FloorTdc floorTdc = _context.FloorTdcs.Where(m => m.Name == dataItem.TdcFloorName && m.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                if (floorTdc == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy Tầng \n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.FloorTdcId = floorTdc.Id;
                                }

                                ApartmentTdc apartmentTdc = _context.ApartmentTdcs.Where(m => m.Name == dataItem.TdcApartmentName && m.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                if (apartmentTdc == null)
                                {
                                    dataItem.Valid = false;
                                    dataItem.ErrMsg += "Không tìm thấy căn \n";
                                    continue;
                                }
                                else
                                {
                                    dataItem.TdcApartmentId = apartmentTdc.Id;
                                }

                                dataItem.CreatedById = -1;
                                dataItem.CreatedBy = fullName;
                                dataItem.Code = "KH_TDC_";
                                dataItem.Code = CodeIndentity.CodeInd("KH_TDC_", _context.TdcCustomers.Count(), 4);
                                dataValid.Add(dataItem);
                            }
                        }
                        importHistory.Data = data.Cast<dynamic>().ToList();
                        importHistory.CreatedById = userId;
                        importHistory.CreatedBy = fullName;

                        _context.ImportHistories.Add(importHistory);
                        _context.TdcCustomers.AddRange(dataValid);

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

        public static List<TdcCustomerDataImport> importData(MemoryStream ms, int sheetnumber, int rowStart)
        {
            XSSFWorkbook workbook = new XSSFWorkbook(ms);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);

            List<TdcCustomerDataImport> res = new List<TdcCustomerDataImport>();
            for (int row = rowStart; row <= sheet.LastRowNum; row++)
            {
                if (sheet.GetRow(row) != null)
                {
                    TdcCustomerDataImport inputDetail = new TdcCustomerDataImport();
                    inputDetail.Valid = true;
                    inputDetail.ErrMsg = "";

                    for (int i = 0; i < 22; i++)
                    {
                        try
                        {
                            var cell = sheet.GetRow(row).GetCell(i, MissingCellPolicy.RETURN_NULL_AND_BLANK);
                            string pattern = @"^0[0-9]{9}$";
                            //Lấy giá trị trong cell
                           
                            string str = UtilsService.getCellValue(cell);
                            if (i == 0)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.Index = int.Parse(str);
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Số thứ tự\n";
                                }
                            }

                            else if (i == 1)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TdcProjectName = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột dự án chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột dự án\n";
                                }
                            }

                            else if (i == 2)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TdcLandName = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột lô chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột lô\n";
                                }
                            }

                            else if (i == 3)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TdcBlockHouseName = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột khối chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột khối\n";
                                }
                            }

                            else if (i == 4)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TdcFloorName = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột tầng chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột tầng\n";
                                }
                            }

                            else if (i == 5)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.TdcApartmentName = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột căn chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột căn\n";
                                }
                            }

                            else if (i == 6)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.FullName = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột tên khách hàng chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột tên khách hàng\n";
                                }
                            }

                            else if (i == 7)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        // Kiểm tra xem chuỗi đã đủ 12 ký tự chưa
                                        //Qfix
                                        // if (str.Length == 12)
                                        // {
                                        //     inputDetail.CCCD = str;
                                        // }
                                        // else
                                        // {
                                        //     inputDetail.Valid = false;
                                        //     inputDetail.ErrMsg += "Cột CCCD chưa có dữ liệu \n";
                                        // }
                                        inputDetail.CCCD = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột CCCD chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột CCCD\n";
                                }
                            }

                            else if (i == 8)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.Dob = DateTime.FromOADate(Double.Parse(str));
                                        if (inputDetail.Dob.Year < 1900)
                                        {
                                            inputDetail.Valid = false;
                                            inputDetail.ErrMsg += "Ngày không hợp lệ\n";
                                        }
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột ngày\n";
                                }
                            }
                            
                            else if (i == 9)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        if (Regex.IsMatch(str, pattern))
                                        {
                                            inputDetail.Phone = str;
                                        }
                                        else
                                        {
                                            inputDetail.Valid = false;
                                            inputDetail.ErrMsg += "Cột SĐT sai có dữ liệu \n";
                                        }
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột SĐT chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột SĐT\n";
                                }
                            }

                            else if (i == 10)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.Email = str;
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Email\n";
                                }
                            }

                            else if (i == 11)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.Note = str;
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Ghi chú\n";
                                }
                            }

                            else if (i == 12)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.AddressTT = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột căn nhà số chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột căn nhà số\n";
                                }
                            }

                            else if (i == 13)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.ProvinceNameTT = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột tỉnh/thành phố không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột tỉnh/thành phố\n";
                                }
                            }
                            else if (i == 14)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.DistrictNameTT = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột quận/huyện không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột quận/huyện\n";
                                }
                            }
                            else if (i == 15)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.WardNameTT = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột xã/phường không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột xã/phường\n";
                                }
                            }
                            else if (i == 16)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.LaneTt = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột đường không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột đường\n";
                                }
                            }

                            else if (i == 17)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.AddressLH = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột Căn nhà số không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Căn nhà số\n";
                                }
                            }

                            else if (i == 18)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.ProvinceNameLH = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột tỉnh/thành phố không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột tỉnh/thành phố\n";
                                }
                            }
                            else if (i == 19)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.DistrictNameLH = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột quận/huyện không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột quận/huyện\n";
                                }
                            }
                            else if (i == 20)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.WardNameLH = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột xã/phường không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột xã/phường\n";
                                }
                            }
                            else if (i == 21)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.LaneLh = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột đường không có dữ liệu\n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột đường\n";
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
            return res;
        }

        #endregion

        //[HttpPost("ImportExcel")]
        //public async Task<IActionResult> ImportExcel([FromBody] List<TdcCustomerData> input)
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
        //    if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.CREATE))
        //    {
        //        def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_CREATE_MESSAGE);
        //        return Ok(def);
        //    }
        //    List<string> messages = new List<string>();

        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
        //            return Ok(def);
        //        }

        //        using (var transaction = _context.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                List<District> districts = _context.Districts.Where(d => d.Status != AppEnums.EntityStatus.DELETED).ToList();
        //                List<Province> provinces = _context.Provincies.Where(d => d.Status != AppEnums.EntityStatus.DELETED).ToList();
        //                List<Ward> wards = _context.Wards.Where(d => d.Status != AppEnums.EntityStatus.DELETED).ToList();
        //                List<Lane> lanies = _context.Lanies.Where(d => d.Status != AppEnums.EntityStatus.DELETED).ToList();

        //                foreach (var item in input)
        //                {
        //                    //tam tru
        //                    int provinceTTId = provinces.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(item.ProvinceNameTT)).Select(x => x.Id).FirstOrDefault();
        //                    if (provinceTTId > 0)
        //                    {
        //                        item.ProvinceTT = provinceTTId;
        //                    }

        //                    if (provinceTTId == 0)
        //                    {
        //                        def.meta = new Meta(400, "Tỉnh/thành phố không tồn tại");
        //                        return Ok(def);
        //                    }

        //                    int districtTTId = districts.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(item.DistrictNameTT)).Select(x => x.Id).FirstOrDefault();
        //                    if (districtTTId > 0)
        //                    {
        //                        item.DistrictTT = districtTTId;
        //                    }

        //                    if (districtTTId == 0)
        //                    {
        //                        def.meta = new Meta(400, "Quận/huyện không tồn tại");
        //                        return Ok(def);
        //                    }

        //                    int wardTTId = wards.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(item.WardNameTT)).Select(x => x.Id).FirstOrDefault();
        //                    if (wardTTId > 0)
        //                    {
        //                        item.WardTT = wardTTId;
        //                    }

        //                    if (wardTTId == 0)
        //                    {
        //                        def.meta = new Meta(400, "Phường/Xã không tồn tại");
        //                        return Ok(def);
        //                    }

        //                    int laneTTId = lanies.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(item.LaneTt)).Select(x => x.Id).FirstOrDefault();
        //                    if (laneTTId > 0)
        //                    {
        //                        item.LaneTT = laneTTId;
        //                    }

        //                    if (laneTTId == 0)
        //                    {
        //                        def.meta = new Meta(400, "Đường không tồn tại");
        //                        return Ok(def);
        //                    }

        //                    //lien he
        //                    int provinceLHId = provinces.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(item.ProvinceNameLH)).Select(x => x.Id).FirstOrDefault();
        //                    if (provinceLHId > 0)
        //                    {
        //                        item.ProvinceLH = provinceLHId;
        //                    }

        //                    if (provinceLHId == 0)
        //                    {
        //                        def.meta = new Meta(400, "Khối nhà không tồn tại");
        //                        return Ok(def);
        //                    }

        //                    int districtLHId = districts.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(item.DistrictNameLH)).Select(x => x.Id).FirstOrDefault();
        //                    if (districtLHId > 0)
        //                    {
        //                        item.DistrictLH = districtLHId;
        //                    }

        //                    if (districtLHId == 0)
        //                    {
        //                        def.meta = new Meta(400, "Khối nhà không tồn tại");
        //                        return Ok(def);
        //                    }

        //                    int wardLHId = wards.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(item.WardNameLH)).Select(x => x.Id).FirstOrDefault();
        //                    if (wardLHId > 0)
        //                    {
        //                        item.WardLH = wardLHId;
        //                    }

        //                    if (wardLHId == 0)
        //                    {
        //                        def.meta = new Meta(400, "Khối nhà không tồn tại");
        //                        return Ok(def);
        //                    }

        //                    int laneLHId = lanies.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(item.LaneLh)).Select(x => x.Id).FirstOrDefault();
        //                    if (laneLHId > 0)
        //                    {
        //                        item.LaneLH = laneLHId;
        //                    }

        //                    if (laneLHId == 0)
        //                    {
        //                        def.meta = new Meta(400, "Khối nhà không tồn tại");
        //                        return Ok(def);
        //                    }

        //                    item.CreatedBy = fullName;
        //                    item.CreatedById = userId;

        //                    _context.TdcCustomers.Add(item);
        //                    await _context.SaveChangesAsync();

        //                    if(item.tdcMenberCustomerDatas != null)
        //                    {
        //                        List<District> district = _context.Districts.Where(d => d.Status != AppEnums.EntityStatus.DELETED).ToList();
        //                        List<Province> province = _context.Provincies.Where(d => d.Status != AppEnums.EntityStatus.DELETED).ToList();
        //                        List<Ward> ward = _context.Wards.Where(d => d.Status != AppEnums.EntityStatus.DELETED).ToList();
        //                        List<Lane> lanie = _context.Lanies.Where(d => d.Status != AppEnums.EntityStatus.DELETED).ToList();

        //                        foreach (var tdcMenberCustomerData in item.tdcMenberCustomerDatas)
        //                        {
        //                            //tam tru
        //                            int provinceTtId = provinces.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(tdcMenberCustomerData.ProvinceNameTt)).Select(x => x.Id).FirstOrDefault();
        //                            if (provinceTtId > 0)
        //                            {
        //                                tdcMenberCustomerData.ProvinceTt = provinceTtId;
        //                            }

        //                            if (provinceTtId == 0)
        //                            {
        //                                def.meta = new Meta(400, "Tỉnh/thành phố không tồn tại");
        //                                return Ok(def);
        //                            }

        //                            int districtTtId = districts.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(tdcMenberCustomerData.DistrictNameTt)).Select(x => x.Id).FirstOrDefault();
        //                            if (districtTtId > 0)
        //                            {
        //                                tdcMenberCustomerData.DistrictTt = districtTtId;
        //                            }

        //                            if (districtTtId == 0)
        //                            {
        //                                def.meta = new Meta(400, "Quận/huyện không tồn tại");
        //                                return Ok(def);
        //                            }

        //                            int wardTtId = wards.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(tdcMenberCustomerData.WardNameTt)).Select(x => x.Id).FirstOrDefault();
        //                            if (wardTtId > 0)
        //                            {
        //                                tdcMenberCustomerData.WardTt = wardTtId;
        //                            }

        //                            if (wardTtId == 0)
        //                            {
        //                                def.meta = new Meta(400, "Phường/Xã không tồn tại");
        //                                return Ok(def);
        //                            }

        //                            int laneTtId = lanies.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(tdcMenberCustomerData.LaneTT)).Select(x => x.Id).FirstOrDefault();
        //                            if (laneTtId > 0)
        //                            {
        //                                tdcMenberCustomerData.LaneTt = laneTtId;
        //                            }

        //                            if (laneTtId == 0)
        //                            {
        //                                def.meta = new Meta(400, "Đường không tồn tại");
        //                                return Ok(def);
        //                            }

        //                            //lien he
        //                            int provinceLhId = provinces.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(tdcMenberCustomerData.ProvinceNameLh)).Select(x => x.Id).FirstOrDefault();
        //                            if (provinceLhId > 0)
        //                            {
        //                                tdcMenberCustomerData.ProvinceLh = provinceLhId;
        //                            }

        //                            if (provinceLhId == 0)
        //                            {
        //                                def.meta = new Meta(400, "Tỉnh/thành phố không tồn tại");
        //                                return Ok(def);
        //                            }

        //                            int districtLhId = districts.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(tdcMenberCustomerData.DistrictNameLh)).Select(x => x.Id).FirstOrDefault();
        //                            if (districtLhId > 0)
        //                            {
        //                                tdcMenberCustomerData.DistrictLh = districtLhId;
        //                            }

        //                            if (districtLhId == 0)
        //                            {
        //                                def.meta = new Meta(400, "Quận/huyện không tồn tại");
        //                                return Ok(def);
        //                            }

        //                            int wardLhId = wards.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(tdcMenberCustomerData.WardNameLh)).Select(x => x.Id).FirstOrDefault();
        //                            if (wardLhId > 0)
        //                            {
        //                                tdcMenberCustomerData.WardLh = wardLhId;
        //                            }

        //                            if (wardLhId == 0)
        //                            {
        //                                def.meta = new Meta(400, "Phường/Xã không tồn tại");
        //                                return Ok(def);
        //                            }

        //                            int laneLhId = lanies.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(tdcMenberCustomerData.LaneLH)).Select(x => x.Id).FirstOrDefault();
        //                            if (laneLhId > 0)
        //                            {
        //                                tdcMenberCustomerData.LaneLh = laneLhId;
        //                            }

        //                            if (laneLhId == 0)
        //                            {
        //                                def.meta = new Meta(400, "Đường không tồn tại");
        //                                return Ok(def);
        //                            }

        //                            tdcMenberCustomerData.TdcCustomerId = item.Id;
        //                            tdcMenberCustomerData.CreatedById = userId;
        //                            tdcMenberCustomerData.CreatedBy = fullName;

        //                            _context.TdcMemberCustomers.Add(tdcMenberCustomerData);
        //                        }
        //                        await _context.SaveChangesAsync();
        //                    }

        //                    if (item.tdcAuthCustomerDetailDatas != null)
        //                    {
        //                        List<District> district = _context.Districts.Where(d => d.Status != AppEnums.EntityStatus.DELETED).ToList();
        //                        List<Province> province = _context.Provincies.Where(d => d.Status != AppEnums.EntityStatus.DELETED).ToList();
        //                        List<Ward> ward = _context.Wards.Where(d => d.Status != AppEnums.EntityStatus.DELETED).ToList();
        //                        List<Lane> lanie = _context.Lanies.Where(d => d.Status != AppEnums.EntityStatus.DELETED).ToList();

        //                        foreach (var tdcAuthDetailData in item.tdcAuthCustomerDetailDatas)
        //                        {
        //                            //tam tru
        //                            int provinceTtId = provinces.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(tdcAuthDetailData.ProvinceNameTt)).Select(x => x.Id).FirstOrDefault();
        //                            if (provinceTtId > 0)
        //                            {
        //                                tdcAuthDetailData.ProvinceTt = provinceTtId;
        //                            }

        //                            if (provinceTtId == 0)
        //                            {
        //                                def.meta = new Meta(400, "Tỉnh/thành phố không tồn tại");
        //                                return Ok(def);
        //                            }

        //                            int districtTtId = districts.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(tdcAuthDetailData.DistrictNameTt)).Select(x => x.Id).FirstOrDefault();
        //                            if (districtTtId > 0)
        //                            {
        //                                tdcAuthDetailData.DistrictTt = districtTtId;
        //                            }

        //                            if (districtTtId == 0)
        //                            {
        //                                def.meta = new Meta(400, "Quận/huyện không tồn tại");
        //                                return Ok(def);
        //                            }

        //                            int wardTtId = wards.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(tdcAuthDetailData.WardNameTt)).Select(x => x.Id).FirstOrDefault();
        //                            if (wardTtId > 0)
        //                            {
        //                                tdcAuthDetailData.WardTt = wardTtId;
        //                            }

        //                            if (wardTtId == 0)
        //                            {
        //                                def.meta = new Meta(400, "Phường/Xã không tồn tại");
        //                                return Ok(def);
        //                            }

        //                            int laneTtId = lanies.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(tdcAuthDetailData.LaneTT)).Select(x => x.Id).FirstOrDefault();
        //                            if (laneTtId > 0)
        //                            {
        //                                tdcAuthDetailData.LaneTt = laneTtId;
        //                            }

        //                            if (laneTtId == 0)
        //                            {
        //                                def.meta = new Meta(400, "Đường không tồn tại");
        //                                return Ok(def);
        //                            }

        //                            //lien he
        //                            int provinceLhId = provinces.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(tdcAuthDetailData.ProvinceNameLh)).Select(x => x.Id).FirstOrDefault();
        //                            if (provinceLhId > 0)
        //                            {
        //                                tdcAuthDetailData.ProvinceLh = provinceLhId;
        //                            }

        //                            if (provinceLhId == 0)
        //                            {
        //                                def.meta = new Meta(400, "Tỉnh/thành phố không tồn tại");
        //                                return Ok(def);
        //                            }

        //                            int districtLhId = districts.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(tdcAuthDetailData.DistrictNameLh)).Select(x => x.Id).FirstOrDefault();
        //                            if (districtLhId > 0)
        //                            {
        //                                tdcAuthDetailData.DistrictLh = districtLhId;
        //                            }

        //                            if (districtLhId == 0)
        //                            {
        //                                def.meta = new Meta(400, "Quận/huyện không tồn tại");
        //                                return Ok(def);
        //                            }

        //                            int wardLhId = wards.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(tdcAuthDetailData.WardNameLh)).Select(x => x.Id).FirstOrDefault();
        //                            if (wardLhId > 0)
        //                            {
        //                                tdcAuthDetailData.WardLh = wardLhId;
        //                            }

        //                            if (wardLhId == 0)
        //                            {
        //                                def.meta = new Meta(400, "Phường/Xã không tồn tại");
        //                                return Ok(def);
        //                            }

        //                            int laneLhId = lanies.AsEnumerable().Where(c => UtilsService.NonUnicode(c.Name) == UtilsService.NonUnicode(tdcAuthDetailData.LaneLH)).Select(x => x.Id).FirstOrDefault();
        //                            if (laneLhId > 0)
        //                            {
        //                                tdcAuthDetailData.LaneLh = laneLhId;
        //                            }

        //                            if (laneLhId == 0)
        //                            {
        //                                def.meta = new Meta(400, "Đường không tồn tại");
        //                                return Ok(def);
        //                            }

        //                            tdcAuthDetailData.TdcCustomerId = item.Id;
        //                            tdcAuthDetailData.CreatedById = userId;
        //                            tdcAuthDetailData.CreatedBy = fullName;

        //                            _context.TdcAuthCustomerDetails.Add(tdcAuthDetailData);
        //                        }
        //                        await _context.SaveChangesAsync();
        //                    }

        //                    LogActionModel logActionModel = new LogActionModel("Thêm mới file import: ",  "TdcCustomer", item.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
        //                    LogAction logAction = _mapper.Map<LogAction>(logActionModel);
        //                    _context.Add(logAction);
        //                    await _context.SaveChangesAsync();

        //                    def.meta = new Meta(200, ApiConstants.MessageResource.ADD_SUCCESS);
        //                    def.data = item;

        //                }
        //                transaction.Commit();
        //                return Ok(def);
        //            }
        //            catch (DbUpdateException e)
        //            {
        //                log.Error("DbUpdateException:" + e);
        //                transaction.Rollback();
        //                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
        //                return Ok(def);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        log.Error("Post Exception:" + e);
        //        def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
        //        return Ok(def);
        //    }
        //}
    }
}
