using AutoMapper;
using DevExpress.ClipboardSource.SpreadsheetML;
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
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
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

namespace IOITQln.Controllers.ApiTdc
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApartmentTdcController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("apartment-tdc", "apartment-tdc");
        private readonly ApiDbContext _context;
        string functionCode = "APARTMENT_TDC";
        private IMapper _mapper;
        private IHostingEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;
        public ApartmentTdcController(ApiDbContext context, IMapper mapper, IHostingEnvironment hostingEnvironment, IConfiguration configuration)
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
                    IQueryable<ApartmentTdc> data = _context.ApartmentTdcs.Where(c => c.Status != AppEnums.EntityStatus.DELETED);
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
                        List<ApartmentTdcData> res = _mapper.Map<List<ApartmentTdcData>>(data.ToList());
                        foreach (ApartmentTdcData item in res)
                        {
                            FloorTdc ten = _context.FloorTdcs.Where(d => d.Id == item.FloorTdcId).FirstOrDefault();
                            item.FloorTdcName = ten != null ? ten.Name : "";

                            BlockHouse tenkhoi = _context.BlockHouses.Where(d => d.Id == ten.BlockHouseId).FirstOrDefault();
                            item.BlockHouseName = tenkhoi != null ? tenkhoi.Name : "";

                            Land tenlo = _context.Lands.Where(d => d.Id == tenkhoi.LandId).FirstOrDefault();
                            item.LandName = tenlo != null ? tenlo.Name : "";

                            TDCProject tenduan = _context.TDCProjects.Where(f => f.Id == tenlo.TDCProjectId).FirstOrDefault();
                            item.TDCProjectName = tenduan != null ? tenduan.Name : "";
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
                ApartmentTdc data = await _context.ApartmentTdcs.FindAsync(id);

                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                    return Ok(def);
                }

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = data;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("GetById Error:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ApartmentTdc input)
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
                input = (ApartmentTdc)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }
                if (input.Name == null || input.Name == "")
                {
                    def.meta = new Meta(400, "Tên không được để trống!");
                    return Ok(def);
                }

                var dataFloor = _context.FloorTdcs.Where(x => x.Id == input.FloorTdcId && x.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                if (dataFloor == null)
                {
                    def.meta = new Meta(400, "Không tồn tại dự án");
                    return Ok(def);
                }

                var dataBlockHouse = _context.BlockHouses.Where(x => x.Id == dataFloor.BlockHouseId && x.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                if(dataBlockHouse == null)
                {
                    def.meta = new Meta(400, "Không tồn tại khối nhà từ tầng");
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.CreatedById = userId;
                    input.CreatedBy = fullName;                    

                    dataFloor.ApartmentTdcCount++;
                    dataBlockHouse.TotalApartmentTdcCount++;

                    _context.ApartmentTdcs.Add(input);
                    _context.FloorTdcs.Update(dataFloor);
                    _context.BlockHouses.Update(dataBlockHouse);

                    try
                    {
                        await _context.SaveChangesAsync();
                        string code = "";
                        int newid = input.Id;
                        if (newid < 10) code = "CAN_TDC_000" + newid;
                        if (newid >= 10) code = "CAN_TDC_00" + newid;
                        if (newid >= 100) code = "CAN_TDC_0" + newid;
                        if (newid >= 1000) code = "CAN_TDC_" + newid;
                        input.Code = code;
                        ApartmentTdc aprtmentTDC = _context.ApartmentTdcs.Find(newid);
                        aprtmentTDC.Code = code;
                        _context.Update(aprtmentTDC);
                        await _context.SaveChangesAsync();

                        LogActionModel logActionModel = new LogActionModel("Thêm mới Căn: " + input.Name, "ApartmentTdc", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (input.Id > 0)
                            transaction.Commit();
                        else
                            transaction.Rollback();

                        def.meta = new Meta(200, ApiConstants.MessageResource.ADD_SUCCESS);
                        def.data = aprtmentTDC;
                        return Ok(def);
                    }
                    catch (DbUpdateException e)
                    {
                        log.Error("DbUpdateException:" + e);
                        transaction.Rollback();
                        if (ApartmentTdcExists(input.Id))
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ApartmentTdc input)
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
                input = (ApartmentTdc)UtilsService.TrimStringPropertyTypeObject(input);

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

                if (input.Name == null || input.Name == "")
                {
                    def.meta = new Meta(400, "Tên không được để trống!");
                    return Ok(def);
                }

                ApartmentTdc data = await _context.ApartmentTdcs.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_UPDATE_MESSAGE);
                    return Ok(def);
                }

                ApartmentTdc codeExist = _context.ApartmentTdcs.Where(f => f.Code == input.Code && f.Status != AppEnums.EntityStatus.DELETED && f.Id != id).FirstOrDefault();
                if (codeExist != null)
                {
                    def.meta = new Meta(211, "Mã đã tồn tại!");
                    return Ok(def);
                }

                ApartmentTdc oldApartmentTdc = await _context.ApartmentTdcs.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
                if (oldApartmentTdc == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_UPDATE_MESSAGE);
                    return Ok(def);
                }

                //FloorTdc floorTdcData = await _context.FloorTdcs.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
                //if (floorTdcData == null)
                //{
                //    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_UPDATE_MESSAGE);
                //    return Ok(def);
                //}


                var dataFloor = _context.FloorTdcs.Where(x => x.Id == input.FloorTdcId && x.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                if (dataFloor == null)
                {
                    def.meta = new Meta(404, "Không tồn tại tầng căn hộ!");
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

                    //Cap nhat lai so luong can cua tang
                    if(input.FloorTdcId != data.FloorTdcId)
                    {
                        dataFloor.ApartmentTdcCount +=  1;
                        _context.Update(dataFloor);

                        //Tru di so luong can cua tang cu
                        FloorTdc oldFloorTdc = _context.FloorTdcs.Where(e => e.Id == data.FloorTdcId && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                        if(oldFloorTdc != null)
                        {
                            oldFloorTdc.ApartmentTdcCount += -1;
                            _context.Update(oldFloorTdc);

                            if(oldFloorTdc.BlockHouseId != dataFloor.BlockHouseId)
                            {
                                //khoi nha cu
                                var oldBlockHouse = _context.BlockHouses.Where(x => x.Id == oldFloorTdc.BlockHouseId && x.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                var currBlockHouse = _context.BlockHouses.Where(x => x.Id == dataFloor.BlockHouseId && x.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                if (oldBlockHouse != null && currBlockHouse != null)
                                {
                                    oldBlockHouse.TotalApartmentTdcCount -= 1;
                                    _context.Update(oldBlockHouse);

                                    currBlockHouse.TotalApartmentTdcCount += 1;
                                    _context.Update(currBlockHouse);

                                }
                            }
                        }
                    }

                    try
                    {
                        await _context.SaveChangesAsync();

                        LogActionModel logActionModel = new LogActionModel("Sửa Căn: " + input.Name, "ApartmentTdc", input.Id, HttpContext.Connection.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, userId, fullName);
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
                        if (!ApartmentTdcExists(data.Id))
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
                ApartmentTdc data = await _context.ApartmentTdcs.FindAsync(id);
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_DELETE_MESSAGE);
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

                        var dataFloorTdc = await _context.FloorTdcs.FindAsync(data.FloorTdcId);
                        if (dataFloorTdc != null)
                        {
                            dataFloorTdc.ApartmentTdcCount--;
                            _context.Update(dataFloorTdc);
                        }

                        var dataBlockHouse = await _context.BlockHouses.FindAsync(dataFloorTdc.BlockHouseId);
                        if (dataBlockHouse != null)
                        {
                            dataBlockHouse.TotalApartmentTdcCount--;
                            _context.Update(dataBlockHouse);
                        }

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Xóa căn: " + data.Name, "ApartmentTdc", data.Id, HttpContext.Connection.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.UPDATE, userId, fullName);
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
                        if (!ApartmentTdcExists(data.Id))
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

        private bool ApartmentTdcExists(int id)
        {
            return _context.ApartmentTdcs.Count(e => e.Id == id) > 0;
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

            List<ApartmentTdc> apartmentTdcs = _context.ApartmentTdcs.Where(p => p.Status != AppEnums.EntityStatus.DELETED).ToList();
            List<ApartmentTdcData> mapper_tdc = _mapper.Map<List<ApartmentTdcData>>(apartmentTdcs);
            foreach (var map in mapper_tdc)
            {
                map.FloorTdcName = _context.FloorTdcs.Where(f => f.Id == map.FloorTdcId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.Name).FirstOrDefault();
            }

            // khởi tạo wb rỗng
            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"templates/ApartmentTdc.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);

            MemoryStream ms = WriteDataToExcel(templatePath, 0, apartmentTdcs, mapper_tdc);
            byte[] byteArrayContent = ms.ToArray();
            return new FileContentResult(byteArrayContent, "application/octet-stream");
        }

        private static MemoryStream WriteDataToExcel(string templatePath, int sheetnumber, List<ApartmentTdc> data, List<ApartmentTdcData> datas)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowStart = 2;

            if (sheet != null)
            {
                int datacol = 8;
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
                        try
                        {
                            XSSFRow row = (XSSFRow)sheet.CreateRow(rowStart);
                            for (int i = 0; i < datacol; i++)
                            {
                                row.CreateCell(i).CellStyle = rowStyle[i];

                                if (i == 0)
                                {
                                    row.GetCell(i).SetCellValue(k + 1);
                                }
                                else if (i == 1)
                                {
                                    row.GetCell(i).SetCellValue(item.Code);
                                }
                                else if (i == 2)
                                {
                                    row.GetCell(i).SetCellValue(item.Name);
                                }
                                else if (i == 3)
                                {
                                    row.GetCell(i).SetCellValue(item.FloorTdcName);
                                }
                                else if (i == 4)
                                {
                                    if (item.Corner == true)
                                    {
                                        row.GetCell(i).SetCellValue("Check");
                                    }
                                    else
                                    {
                                        row.GetCell(i).SetCellValue("");
                                    }

                                }
                                else if (i == 5)
                                {
                                    row.GetCell(i).SetCellValue((double)item.ConstructionValue);

                                }
                                else if (i == 6)
                                {
                                    row.GetCell(i).SetCellValue((double)item.ContrustionBuild);
                                }
                                else if (i == 7)
                                {
                                    row.GetCell(i).SetCellValue(item.Note);
                                }

                            }
                            rowStart++;
                            k++;
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                catch (Exception ex)
                {
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
                importHistory.Type = AppEnums.ImportHistoryType.TdcApartment;

                List<ApartmentTdcImport> data = new List<ApartmentTdcImport>();

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
                                    data = importData(ms, 0, 1);
                                }
                            }
                        }
                    }
                    i++;
                }

                List<ApartmentTdcImport> dataValid = new List<ApartmentTdcImport>();
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var dataItem in data)
                        {
                            if (dataItem.Valid == true)
                            {
                                FloorTdc floorTdc = _context.FloorTdcs.Where(m => m.Name == dataItem.FloorTdcName && m.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
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

                                dataItem.CreatedById = -1;
                                dataItem.CreatedBy = fullName;
                                dataItem.Code = "CAN_TDC_";
                                dataItem.Code = CodeIndentity.CodeInd("CAN_TDC_", _context.ApartmentTdcs.Count(), 4);
                                dataValid.Add(dataItem);
                            }
                        }
                        importHistory.Data = data.Cast<dynamic>().ToList();
                        importHistory.CreatedById = userId;
                        importHistory.CreatedBy = fullName;

                        _context.ImportHistories.Add(importHistory);
                        _context.ApartmentTdcs.AddRange(dataValid);

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

        public static List<ApartmentTdcImport> importData(MemoryStream ms, int sheetnumber, int rowStart)
        {
            XSSFWorkbook workbook = new XSSFWorkbook(ms);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);

            List<ApartmentTdcImport> res = new List<ApartmentTdcImport>();
            for (int row = rowStart; row <= sheet.LastRowNum; row++)
            {
                if (sheet.GetRow(row) != null)
                {
                    ApartmentTdcImport inputDetail = new ApartmentTdcImport();
                    inputDetail.Valid = true;
                    inputDetail.ErrMsg = "";

                    for (int i = 0; i < 8; i++)
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
                                        inputDetail.Name = str;
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột tên tầng chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột tên tầng\n";
                                }
                            }

                            else if (i == 2)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.FloorTdcName = str;
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

                            else if (i == 3)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        if (str.ToUpper() == "TRUE")
                                        {
                                            inputDetail.Corner = true;
                                        }
                                        else
                                        {
                                            inputDetail.Corner = false;
                                        }
                                    }
                                    else
                                    {
                                        inputDetail.Corner = false;
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột Lô góc\n";
                                }
                            }

                            else if (i == 4)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.ConstructionValue = double.Parse(str);
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột tổng diện tích sử dụng chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột tổng diện tích sử dụng\n";
                                }
                            }

                            else if (i == 5)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.ContrustionBuild = double.Parse(str);
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột tổng diện tích xây dựng chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột tổng diện tích xây dựng\n";
                                }
                            }

                            else if (i == 6)
                            {
                                try
                                {
                                    if (str != "")
                                    {
                                        inputDetail.RoomNumber = int.Parse(str);
                                    }
                                    else
                                    {
                                        inputDetail.Valid = false;
                                        inputDetail.ErrMsg += "Cột số phòng ngủ chưa có dữ liệu \n";
                                    }
                                }
                                catch
                                {
                                    inputDetail.Valid = false;
                                    inputDetail.ErrMsg += "Lỗi cột số phòng ngủ\n";
                                }
                            }

                            else if (i == 7)
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
                                    inputDetail.ErrMsg += "Lỗi cột ghi chú\n";
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
        //public async Task<IActionResult> ImportExcel([FromBody] List<ApartmentTdcData> input)
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
        //                foreach (var item in input)
        //                {

        //                    var existCode = _context.Apartments.FirstOrDefault(x => x.Code == item.Code);
        //                    if (existCode != null)
        //                    {
        //                        messages.Add(item.Code + " không hợp lệ");
        //                        continue;
        //                    }

        //                    int floortdcId = _context.FloorTdcs.Where(x => x.Name == item.FloorTdcName).Select(x => x.Id).FirstOrDefault();
        //                    if (floortdcId > 0)
        //                    {
        //                        item.FloorTdcId = floortdcId;
        //                    }

        //                    if (floortdcId == 0)
        //                    {
        //                        def.meta = new Meta(400, "Nền đất không tồn tại");
        //                        return Ok(def);
        //                    }

        //                    item.CreatedBy = fullName;
        //                    item.CreatedById = userId;

        //                    _context.ApartmentTdcs.Add(item);
        //                    await _context.SaveChangesAsync();

        //                    LogActionModel logActionModel = new LogActionModel("Thêm mới căn: " + item.Code, "ApartmentTdc", item.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
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
