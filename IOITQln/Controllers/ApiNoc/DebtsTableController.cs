using AutoMapper;
using DevExpress.ClipboardSource.SpreadsheetML;
using DevExpress.Office.Utils;
using DevExpress.Utils.DirectXPaint;
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
using NPOI.SS.Formula.Functions;
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
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using static DevExpress.Office.Utils.DrawingFillStyleBooleanProperties;
using static IOITQln.Common.Enums.AppEnums;

namespace IOITQln.Controllers.ApiNoc
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DebtsTableController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("DEBTSTABLE", "DEBTSTABLE");
        private static string functionCode = "DEBTS";
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        private IHostingEnvironment _hostingEnvironment;


        public DebtsTableController(ApiDbContext context, IMapper mapper, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;

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
                    IQueryable<DebtsTable> data = _context.DebtsTables.Where(c => c.Status != AppEnums.EntityStatus.DELETED);
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
                        def.data = data;
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
                DebtsTable data = await _context.DebtsTables.FindAsync(id);

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

        [HttpPost("DebtsTable")]
        public async Task<IActionResult> Post([FromBody] List<DebtsTable> input)
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
                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var data = _context.DebtsTables.Where(l => l.Code == input[0].Code && l.RentFileId == input[0].RentFileId && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                        int countCheck = data.Count(item => item.Check == true);
                        if (data.Count == 0)
                        {
                            foreach (var item in input)
                            {
                                item.CreatedById = userId;
                                item.CreatedBy = fullName;
                                _context.DebtsTables.Add(item);
                                await _context.SaveChangesAsync();

                                //thêm LogAction
                                LogActionModel logActionModel = new LogActionModel("Thêm data ", "DebtsTable", item.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
                                LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                                _context.Add(logAction);
                                await _context.SaveChangesAsync();
                            }
                        }
                        else
                        {
                            if (countCheck == 0)
                            {
                                data.ForEach(x => x.Status = AppEnums.EntityStatus.DELETED);
                                foreach (var item in input)
                                {
                                    item.CreatedById = userId;
                                    item.CreatedBy = fullName;
                                    _context.DebtsTables.Add(item);
                                    await _context.SaveChangesAsync();

                                    //thêm LogAction
                                    LogActionModel logActionModel = new LogActionModel("Thêm data ", "DebtsTable", item.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
                                    LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                                    _context.Add(logAction);
                                    await _context.SaveChangesAsync();
                                }
                            }
                            else
                            {
                                return Ok(new Meta(212, "B?ng công n? dã du?c thanh toán.Không th? thay d?i!!!!"));
                            }
                        }

                        transaction.Commit();
                        def.meta = new Meta(200, ApiConstants.MessageResource.ADD_SUCCESS);
                        def.data = input;
                    }
                    catch (DbUpdateException e)
                    {
                        log.Error("DbUpdateException:" + e);
                        transaction.Rollback();
                        def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                    }

                    return Ok(def);
                }
            }
            catch (Exception e)
            {
                log.Error("Post Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(DebtsTable input)
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
                input = (DebtsTable)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }
                using (var transaction = _context.Database.BeginTransaction())
                {

                    input.CreatedById = userId;
                    input.CreatedBy = fullName;
                    _context.DebtsTables.Add(input);

                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Thêm m?i : " + input.Id.ToString() + ": " + @String.Format(System.Globalization.CultureInfo.GetCultureInfo("vi-VN"), "{0:N0}", input.Price), "RentingPrice", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (input.Id > 0)
                            transaction.Commit();
                        else
                            transaction.Rollback();

                        def.meta = new Meta(200, ApiConstants.MessageResource.ADD_SUCCESS);
                        def.data = input;
                        return Ok(def);
                    }
                    catch (DbUpdateException e)
                    {
                        log.Error("DbUpdateException:" + e);
                        transaction.Rollback();
                        if (DebtsExists(input.Id))
                        {
                            def.meta = new Meta(212, "Ðã t?n t?i Id trên h? th?ng!");
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

        [HttpPost("{id}/{SurplusBalance}")]
        public async Task<IActionResult> Put(int id, [FromBody] DebtsTable input, decimal? SurplusBalance)
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
                input = (DebtsTable)UtilsService.TrimStringPropertyTypeObject(input);

                var dataDbet = _context.debts.Where(l => l.RentFileId == input.RentFileId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
               
                if (dataDbet != null)
                {
                    if (input.NearestActivities == "B? n?p ti?n")
                    {
                        dataDbet.SurplusBalance = dataDbet.SurplusBalance + SurplusBalance;
                    }
                    else
                    {
                        dataDbet.SurplusBalance = dataDbet.SurplusBalance - SurplusBalance;
                    }
                    _context.debts.Update(dataDbet);
                }

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


                DebtsTable data = await _context.DebtsTables.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
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
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("S?a : " + input.Id, "DebtsTable", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, userId, fullName);
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
                        if (!DebtsExists(data.Id))
                        {
                            def.meta = new Meta(212, "Không t?n t?i Id trên h? th?ng!");
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
                DebtsTable data = await _context.DebtsTables.FindAsync(id);
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

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Xóa : " + data.Id, "Promissory", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.DELETED, userId, fullName);
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
                        if (!DebtsExists(data.Id))
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
        private bool DebtsExists(int id)
        {
            return _context.DebtsTables.Count(e => e.Id == id) > 0;
        }

        ///API dowloaDebts
        [HttpPost("Debts/{Code}")]
        public async Task<IActionResult> Debts(string Code, [FromBody] List<GroupDataDebtTableByCode> input)
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
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.CREATE))
            {
                return new FileContentResult(new byte[0], "application/octet-stream");
            }

            List<Debts> debts = _context.debts.Where(x => x.Code == Code && x.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.CreatedAt).ToList();
            if (debts == null)
            {
                def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                return Ok(def);
            }

            XSSFWorkbook wb = new XSSFWorkbook();
            // T?o ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"templates/Congno.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);
            string nameExcel = "Congno.xls";

            MemoryStream ms = WriteDataToExcel(templatePath, 0, input, debts);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", nameExcel);
        }

        private static MemoryStream WriteDataToExcel(string templatePath, int sheetnumber, List<GroupDataDebtTableByCode> data, List<Debts> debts)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowStart = 6;
            decimal? Total = 0;
            decimal? Paid = 0;
            decimal? Diff = 0;
            debts.ForEach(item =>
            {
                Total += item.Total;
                Diff += item.Diff;
                Paid += item.Total;
            });

            if (sheet != null)
            {
                int datacol = 8;
                try
                {
                    IRow rowA = sheet.GetRow(0);
                    ICell cellA = rowA.CreateCell(1);
                    cellA.SetCellValue(debts[0].Code);

                    IRow rowB2 = sheet.GetRow(1);
                    ICell cellB1 = rowB2.CreateCell(1);
                    cellB1.SetCellValue(debts[0].CustomerName);

                    IRow rowD5 = sheet.GetRow(2);
                    ICell cellD5 = rowD5.CreateCell(1);
                    cellD5.SetCellValue(debts[0].Phone);

                    IRow rowD7 = sheet.GetRow(3);
                    ICell cellD7 = rowD7.CreateCell(1);
                    if (debts[0].Address != null)
                    {
                        cellD7.SetCellValue(debts[0].Address);
                    }
                    else
                    {
                        cellD7.SetCellValue("");
                    }

                    List<ICellStyle> rowStyle = new List<ICellStyle>();

                    // L?y Style excel
                    for (int i = 0; i < datacol; i++)
                    {
                        rowStyle.Add(sheet.GetRow(rowStart).GetCell(i).CellStyle);
                    }
                    int k = 0;
                    foreach (var itemData in data)
                    {
                        IRow rowLabel = sheet.CreateRow(rowStart);
                        ICell rowlabel = rowLabel.CreateCell(0);
                        if (itemData.Type == 3)
                        {
                            k = k + 1;
                            rowlabel.SetCellValue("H?p d?ng " + k);
                            rowlabel.CellStyle = rowStyle[5];
                        }
                        else if (itemData.Type == 1)
                        {
                            rowlabel.SetCellValue("D? li?u excel");
                            rowlabel.CellStyle = rowStyle[5];
                        }
                        else if (itemData.Type == 2)
                        {
                            rowlabel.SetCellValue("Phi?u truy thu");
                            rowlabel.CellStyle = rowStyle[5];
                        }
                        CellRangeAddress mergedRegioncellHeader4 = new CellRangeAddress(rowStart, rowStart, 0, 8);

                        foreach (var item in itemData.groupDataDebtsByDates)
                        {
                            XSSFRow row = (XSSFRow)sheet.CreateRow(rowStart + 1);
                            for (int i = 0; i < datacol; i++)
                            {
                                row.CreateCell(i).CellStyle = rowStyle[i];
                                if (i == 0)
                                {
                                    row.GetCell(i).SetCellValue(item.DateStart);

                                }
                                else if (i == 1)
                                {
                                    row.GetCell(i).SetCellValue((DateTime)item.DateEnd);
                                }
                                else if (i == 2)
                                {
                                    row.GetCell(i).SetCellValue((double)item.Price);
                                }
                                else if (i == 3)
                                {
                                    if (item.Check == true)
                                    {
                                        row.GetCell(i).SetCellValue("x");
                                    }
                                    else
                                    {
                                        row.GetCell(i).SetCellValue("");
                                    }
                                }
                                else if (i == 4)
                                {
                                    row.GetCell(i).SetCellValue((double)item.PriceDiff);
                                }
                                else if (i == 5)
                                {
                                    row.GetCell(i).SetCellValue((double)item.AmountExclude);
                                }
                                else if (i == 6)
                                {
                                    row.GetCell(i).SetCellValue((double)item.VATPrice);
                                }
                                else if (i == 7)
                                {
                                    if (item.CheckPayDepartment == true)
                                    {
                                        row.GetCell(i).SetCellValue("x");
                                    }
                                    else
                                    {
                                        row.GetCell(i).SetCellValue("");
                                    }
                                }
                            }
                            rowStart++;
                        }
                        rowStart++;
                    }


                    ICellStyle cellStyleMoney = workbook.CreateCellStyle();
                    var dataFormat = workbook.CreateDataFormat();
                    cellStyleMoney.DataFormat = dataFormat.GetFormat("#,##0");


                    IRow rowBfEnd = sheet.CreateRow(rowStart);
                    ICell cellAEnd = rowBfEnd.CreateCell(0);
                    cellAEnd.SetCellValue("T?ng");

                    ICell cellB = rowBfEnd.CreateCell(2);
                    cellB.SetCellValue((double)Total);
                    cellB.CellStyle = cellStyleMoney;

                    ICell cellE = rowBfEnd.CreateCell(4);
                    cellE.SetCellValue((double)Paid);
                    cellE.CellStyle = cellStyleMoney;

                    rowStart++;

                    IRow rowEnd = sheet.CreateRow(rowStart);
                    ICell cellA1 = rowEnd.CreateCell(0);
                    cellA1.SetCellValue("Còn n?");

                    ICell cellE1 = rowEnd.CreateCell(4);
                    cellE1.SetCellValue((double)Diff);
                    cellE1.CellStyle = cellStyleMoney;

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

        [HttpPost("GroupDataDebtsTable")]
        public IActionResult GroupDataDebtsTable(DebtsTableReq req)
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
                List<DebtsTable> data = _context.DebtsTables.Where(l => l.Status != AppEnums.EntityStatus.DELETED && l.Code == req.Code).ToList();
                var groupedData = data.GroupBy(x => new { x.RentFileId, x.Type })
                .Select(e =>
                {
                    var rentFile = _context.RentFiles.Find(e.Key.RentFileId);
                    if (rentFile == null)
                    {
                        return null;
                    }

                    return new GroupDataDebtTableByCode
                    {
                        RentFileID = e.Key.RentFileId,
                        Type = e.Key.Type,
                        TypeRentFile = rentFile.Type,
                        groupDataDebtsByDates = e.OrderBy(p => p.Index).ToList().GroupBy(f => new
                        {
                            f.DateStart,
                            f.DateEnd
                        }).Select(e => new groupDataDebtsByDate
                        {
                            DateStart = e.Key.DateStart,
                            DateEnd = (DateTime)e.First().DateEnd,
                            Price = e.Sum(f => f.Price),
                            Check = e.First().Check,
                            Executor = e.First().Executor,
                            Date = e.First().Date,
                            NearestActivities = e.First().NearestActivities,
                            PriceDiff = e.Sum(f => f.PriceDiff),
                            AmountExclude = e.Sum(f => f.AmountExclude),
                            VATPrice = e.Sum(f => f.VATPrice),
                            CheckPayDepartment = e.First().CheckPayDepartment,
                            SXN = e.First().SXN,
                            groupDebtTables = e.ToList()
                        }).ToList()
                    };
                }).Where(p => p != null).ToList();

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = groupedData.OrderBy(p => p.Type);
                return Ok(def);
            }
            catch(Exception ex)
            {
                def.meta = new Meta(500,  ex.ToString());
                return Ok(def);
            }
        }

        //Import
        [HttpPost("ImportExcelDebts")]
        public async Task<IActionResult> ImportExcelDebts(DebtsTableReq req)
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
                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var item in req.data)
                        {
                            item.Code = req.Code;
                            item.CreatedById = userId;
                            item.CreatedBy = fullName;
                            item.Type = 1;
                            _context.DebtsTables.Add(item);
                            await _context.SaveChangesAsync();

                            //thêm LogAction
                            LogActionModel logActionModel = new LogActionModel("Thêm data import ", "DebtsTable", item.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(req.data), (int)AppEnums.Action.CREATE, userId, fullName);
                            LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                            _context.Add(logAction);
                            await _context.SaveChangesAsync();
                        }
                        transaction.Commit();
                        def.meta = new Meta(200, ApiConstants.MessageResource.ADD_SUCCESS);
                        def.data = req.data;
                    }
                    catch (DbUpdateException e)
                    {
                        log.Error("DbUpdateException:" + e);
                        transaction.Rollback();
                        def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                    }

                    return Ok(def);
                }
            }
            catch (Exception e)
            {
                log.Error("Post Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }
    }
}

