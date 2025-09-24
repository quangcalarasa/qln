using AutoMapper;
using DevExpress.ClipboardSource.SpreadsheetML;
using DevExpress.Office.Utils;
using DevExpress.Utils.DirectXPaint;
using DevExpress.Utils.Filtering;
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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using NPOI.HSSF.Record.Chart;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using static IOITQln.Common.Enums.AppEnums;
using GroupData = IOITQln.Models.Data.GroupData;

namespace IOITQln.Controllers.ApiNoc
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RentFileBCTController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("RentFileBCT", "RentFileBCT");
        private static string functionCode = "RENT_FILE_BCT";
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        private IHostingEnvironment _hostingEnvironment;

        private static int autoIncrementValue = 0;

        public RentFileBCTController(ApiDbContext context, IMapper mapper, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet("GetByPage")]
        public IActionResult GetByPage([FromQuery] FilteredPagination paging)
{
    // Lấy access token và kiểm tra token
    var accessToken = Request.Headers[HeaderNames.Authorization];
    var token = _context.Tokens.FirstOrDefault(t => Convert.ToString(t.AccessToken) == accessToken);
    if (token == null) return Unauthorized();

    var def = new DefaultResponse();

    // Check quyền theo logic bản gốc (tham số quyền = 0)
    var access_key = ((ClaimsIdentity)User.Identity)
        .Claims.Where(c => c.Type == "AccessKey")
        .Select(c => c.Value)
        .SingleOrDefault();

    if (!CheckRole.CheckRoleByCode(access_key, functionCode, 0))
    {
        def.meta = new Meta(222, "Bạn không có quyền xem dữ liệu tới mục này!");
        return Ok(def);
    }

    try
    {
        if (paging == null)
        {
            def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
            return Ok(def);
        }

        def.meta = new Meta(200, "Thành công!");

        // Base query: Status != 99
        IQueryable<RentFileBCT> q = _context.RentFileBCTs.Where(c => (int)c.Status != 99);

        // dynamic where (decode trước)
        if (paging.query != null)
            paging.query = HttpUtility.UrlDecode(paging.query);

        var qFiltered = q.Where(paging.query);

        // metadata = Count
        def.metadata = qFiltered.Count();

        // paging + order
        IQueryable<RentFileBCT> pageQuery;
        if (paging.page_size > 0)
        {
            pageQuery = (paging.order_by == null)
                ? qFiltered.OrderBy("Id desc").Skip((paging.page - 1) * paging.page_size).Take(paging.page_size)
                : qFiltered.OrderBy(paging.order_by).Skip((paging.page - 1) * paging.page_size).Take(paging.page_size);
        }
        else
        {
            pageQuery = (paging.order_by == null)
                ? qFiltered.OrderBy("Id desc")
                : qFiltered.OrderBy(paging.order_by);
        }

        // select động hoặc map DTO + group data
        if (!string.IsNullOrEmpty(paging.select))
        {
            paging.select = $"new({paging.select})";
            paging.select = HttpUtility.UrlDecode(paging.select);
            def.data = pageQuery.Select(paging.select);
        }
        else
        {
            // Map list
            var listDto = _mapper.Map<List<RentFileBCTData>>(pageQuery.ToList());

            // Lấy DefaultCoefficient (Status != 99), OrderBy CoefficientName (kiểu AppEnums.TypeTable)
            var defCoeffs = _context.DefaultCoefficients
                .Where(l => (int)l.Status != 99)
                .OrderBy(p => p.CoefficientName)
                .ToList();

            // Group theo CoefficientName => GroupData
            var grouped = defCoeffs
                .GroupBy(x => x.CoefficientName)
                .Select(e => new GroupData
                {
                    CoefficientId = e.Key,
                    defaultCoefficients = e.ToList(),
                    childDefaultCoefficients = new List<ChildDefaultCoefficient>()
                })
                .ToList();

            // Với mỗi item => nạp childDefaultCoefficients theo RentFileBctId & CoefficientId
            foreach (var item in listDto)
            {
                foreach (var g in grouped)
                {
                    g.childDefaultCoefficients = _context.ChildDefaults
                        .Where(l => l.RentFileBctId == item.Id
                                 && (int)l.CoefficientId == (int)g.CoefficientId
                                 && (int)l.Status != 99)
                        .ToList();
                }
                item.groupData = grouped;
            }

            def.data = listDto;
        }

        return Ok(def);
    }
    catch (Exception ex)
    {
        log.Error("GetByPage Error:" + ex);
        def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
        return Ok(def);
    }
}


        [HttpGet("GetById/{id}")]
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
                RentFileBCT data = await _context.RentFileBCTs.FindAsync(id);
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                    return Ok(def);
                }
                RentFileBCTData res = _mapper.Map<RentFileBCTData>(data);

                List<DefaultCoefficient> dataDf = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderBy(p => p.CoefficientName).ToList();
                var groupedData = dataDf.GroupBy(x => x.CoefficientName).Select(e => new GroupData
                {
                    CoefficientId = e.Key,
                    defaultCoefficients = e.ToList(),
                    childDefaultCoefficients = _context.ChildDefaults.Where(l => l.RentFileBctId == res.Id && l.CoefficientId == e.Key && l.Status != AppEnums.EntityStatus.DELETED).ToList()
                }).ToList();
                res.groupData = groupedData;

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = data;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("GetById Error" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [HttpGet("GetByRentFileId/{RentFilesId}/{TypeBCT}")]
        public async Task<IActionResult> GetByRentFileId(Guid RentFilesId, byte TypeBCT)
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
                RentFileBCT bigData = _context.RentFileBCTs.Where(l => l.RentFileId == RentFilesId && l.TypeBCT == TypeBCT && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                RentFileBCTData res = _mapper.Map<RentFileBCTData>(bigData);

                List<DefaultCoefficient> dataDf = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderBy(p => p.CoefficientName).ToList();
                var groupedData = dataDf.GroupBy(x => x.CoefficientName).Select(e => new GroupData
                {
                    CoefficientId = e.Key,
                    defaultCoefficients = e.ToList(),
                    childDefaultCoefficients = _context.ChildDefaults.Where(l => l.RentFileBctId == res.Id && l.CoefficientId == e.Key && l.Status != AppEnums.EntityStatus.DELETED).ToList()
                }).ToList();
                res.groupData = groupedData;

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = res;
                return Ok(def);
            }
            catch (Exception ex)
            {
                return Ok(def);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RentFileBCTData input)
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

            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_CREATE_MESSAGE);
                return Ok(def);
            }

            try
            {
                input = (RentFileBCTData)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    input.CreatedById = userId;
                    input.CreatedBy = fullName;
                    _context.RentFileBCTs.Add(input);
                    try
                    {
                        await _context.SaveChangesAsync();

                        if (input.groupData != null)
                        {
                            foreach (var item in input.groupData)
                            {
                                if (item.childDefaultCoefficients.Count > 0)
                                {
                                    foreach (var childItem2 in item.childDefaultCoefficients)
                                    {
                                        childItem2.RentFileBctId = input.Id;
                                        _context.ChildDefaults.Add(childItem2);
                                    }
                                }
                            }
                            await _context.SaveChangesAsync();
                        }
                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Thêm mới : " + input.Id, "RentFileBCT", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
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
                        if (RentFileBCTExists(input.Id))
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
        public async Task<IActionResult> Put(int id, [FromBody] RentFileBCTData input)
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

            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_UPDATE_MESSAGE);
                return Ok(def);
            }

            try
            {
                input = (RentFileBCTData)UtilsService.TrimStringPropertyTypeObject(input);

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

                RentFileBCT data = await _context.RentFileBCTs.AsNoTracking().Where(e => e.Id == id && e.Status != AppEnums.EntityStatus.DELETED).FirstOrDefaultAsync();
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

                        List<ChildDefaultCoefficient> lst = _context.ChildDefaults.Where(x => x.RentFileBctId == id && x.Type == input.TypeBCT && x.Status != AppEnums.EntityStatus.DELETED).ToList();
                        lst.ForEach(x => x.Status = AppEnums.EntityStatus.DELETED);
                        _context.UpdateRange(lst);
                        await _context.SaveChangesAsync();

                        if (input.groupData != null)
                        {
                            foreach (var item in input.groupData)
                            {
                                if (item.childDefaultCoefficients.Count > 0)
                                {
                                    foreach (var childItem2 in item.childDefaultCoefficients)
                                    {
                                        childItem2.RentFileBctId = input.Id;
                                        _context.ChildDefaults.Add(childItem2);
                                    }
                                }
                            }
                            await _context.SaveChangesAsync();
                        }

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Sửa hồ sơ : " + input.Id, "RentFileBCT", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.UPDATE, userId, fullName);
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
                        if (!RentFileBCTExists(data.Id))
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

            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_DELETE_MESSAGE);
                return Ok(def);
            }

            try
            {
                RentFileBCT data = await _context.RentFileBCTs.FindAsync(id);
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

                    List<DebtsTable> debtsTables = _context.DebtsTables.Where(l => l.RentFileId == data.RentFileId && l.Type == 2 && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                    if (debtsTables != null)
                    {
                        debtsTables.ForEach(item =>
                        {
                            item.UpdatedAt = DateTime.Now;
                            item.UpdatedById = userId;
                            item.UpdatedBy = fullName;
                            item.Status = AppEnums.EntityStatus.DELETED;
                        });
                        _context.UpdateRange(debtsTables);
                    }
                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Xóa : " + data.Id, "RentFileBCT", data.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(data), (int)AppEnums.Action.DELETED, userId, fullName);
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
                        if (!RentFileBCTExists(data.Id))
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
        private bool RentFileBCTExists(int id)
        {
            return _context.RentFileBCTs.Count(e => e.Id == id) > 0;
        }

        //Bảng chiết tính hợp đồng

        //API getworkSheet/QD1753
        [HttpGet("GetExcelTable1753/{RentFilesId}")]
        public async Task<IActionResult> GetExcelTable1753(Guid RentFilesId)
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
                //Tìm hồ sơ cho thuê
                RentFile rentFile = _context.RentFiles.Where(l => l.Id == RentFilesId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                RentFlieData map_rentFlieData = _mapper.Map<RentFlieData>(rentFile);

                //Tìm bảng chiết tính mới nhất theo ID hồ sơ
                RentFileBCT rentFileBCT = _context.RentFileBCTs.Where(l => l.RentFileId == RentFilesId && l.TypeBCT == 1).OrderByDescending(p => p.CreatedAt).FirstOrDefault();

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


                List<Coefficient> coefficients = _context.Coefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList();

                List<Conversion> conversions = _context.Conversions.Where(l => l.TypeQD == AppEnums.TypeQD.QD_1753).ToList();

                List<DefaultCoefficient> defaultCoefficients = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                Block blocks = _context.Blocks.Where(l => l.Id == rentFile.RentBlockId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                List<RentingPrice> rentingPrices = _context.RentingPricies.Where(l => l.TypeQD == AppEnums.TypeQD.QD_1753 && l.TypeBlockId == blocks.TypeBlockId && l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.EffectiveDate).ToList();

                BlockData map_blocks = _mapper.Map<BlockData>(blocks);

                List<BlockDetail> blockDetails = _context.BlockDetails.Where(l => l.BlockId == rentFile.RentBlockId && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<BlockDetailData> map_BlockDetails = _mapper.Map<List<BlockDetailData>>(blockDetails);

                List<Floor> floorData = _context.Floors.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<Area> AreaData = _context.Areas.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<DiscountCoefficient> disData = _context.DiscountCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<DefaultCoefficient> dfData = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();

                foreach (BlockDetailData map_BlockDetail in map_BlockDetails)
                {
                    Floor floor = floorData.Where(f => f.Id == map_BlockDetail.FloorId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    map_BlockDetail.FloorName = floor != null ? floor.Name : "";
                    map_BlockDetail.FloorCode = floor != null ? floor.Code : 0;

                    Area Area = AreaData.Where(f => f.Id == map_BlockDetail.AreaId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    map_BlockDetail.AreaName = Area != null ? Area.Name : "";
                    map_BlockDetail.IsMezzanine = Area != null ? Area.IsMezzanine : null;
                }
                map_blocks.blockDetails = map_BlockDetails.OrderBy(x => x.FloorCode).ThenBy(x => x.IsMezzanine).ToList();

                if (rentFile.RentApartmentId == 0 && map_blocks.blockDetails.Count == 0)
                {
                    var code = rentFile.CodeCH != null ? rentFile.CodeCH : rentFile.CodeCN;
                    def.meta = new Meta(500, "Thông tin căn nhà/căn hộ chưa được cập nhật. Vui lòng thêm thông tin căn nhà/căn hộ: " + code);
                    return Ok(def);
                }

                List<BCT> resBct = new List<BCT>();
                int oldId = 0;
                if (rentFile.RentApartmentId > 0)
                {
                    Apartment apartment = _context.Apartments.Where(l => l.Id == rentFile.RentApartmentId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    ApartmentData map_apartments = _mapper.Map<ApartmentData>(apartment);
                    List<ApartmentDetail> apartmentDetails = _context.ApartmentDetails.Where(a => a.TargetId == rentFile.RentApartmentId && a.Type == TypeApartmentDetail.APARTMENT && a.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                    List<ApartmentDetailData> apartmentDetailData = _mapper.Map<List<ApartmentDetailData>>(apartmentDetails);
                    foreach (ApartmentDetailData map_apartmentDetail in apartmentDetailData)
                    {
                        Area area = AreaData.Where(f => f.Id == map_apartmentDetail.AreaId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                        map_apartmentDetail.AreaName = area != null ? area.Name : "";
                        map_apartmentDetail.IsMezzanine = area != null ? area.IsMezzanine : null;

                        Floor floor = floorData.Where(f => f.Id == map_apartmentDetail.FloorId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                        map_apartmentDetail.FloorName = floor != null ? floor.Name : "";
                        map_apartmentDetail.FloorCode = floor != null ? floor.Code : 0;
                    }
                    map_apartments.apartmentDetailData = apartmentDetailData.ToList();

                    foreach (var item in map_apartments.apartmentDetailData)
                    {
                        var bCT = new BCT();

                        bCT.GroupId = 1753;

                        bCT.AreaName = item.AreaName;

                        bCT.Level = (int)item.Level;

                        bCT.PrivateArea = (float)item.PrivateArea;

                        bCT.DiscountCoff = (decimal)disData.Where(l => l.Id == rentFileBCT.DiscountCoefficientId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();

                        bCT.DateCoefficient = (DateTime)item.DisposeTime;

                        bCT.DateStart = rentFile.DateHD;
                        bCT.RentFileId = RentFilesId;

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
                                    if (oldId == itemChild.defaultCoefficientsId) continue;
                                    var content = dfData.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
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

                                        if (name == content.Content)
                                        {
                                            if (bCT.DateStart >= itemChild.DoApply)
                                            {
                                                oldId = itemChild.defaultCoefficientsId;
                                                var itemChilDfs = new chilDf();
                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                itemChilDfs.Value = itemChild.Value;
                                                bCT.chilDfs.Add(itemChilDfs);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                            itemChilDfs.Value = 0;
                                            bCT.chilDfs.Add(itemChilDfs);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (bCT.DateStart >= itemChild.DoApply)
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Value = itemChild.Value;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                            bCT.chilDfs.Add(itemChilDfs);
                                            break;
                                        }
                                        else
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Value = 0;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
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
                        //bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * (decimal)bCT.TotalK * (decimal)(1 + (bCT.VAT / 100))));
                        bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * (decimal)bCT.TotalK));

                        bCT.PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea));
                        bCT.PolicyReduction = Math.Round((decimal)(bCT.DiscountCoff * bCT.PriceRent));
                        bCT.PriceAfterDiscount = Math.Round((decimal)(bCT.PriceRent - bCT.PolicyReduction));
                        bCT.Unit = "VNĐ";
                        resBct.Add(bCT);
                    }
                }
                else
                {
                    foreach (var item in map_blocks.blockDetails)
                    {
                        var bCT = new BCT();

                        bCT.GroupId = 1753;

                        bCT.AreaName = item.AreaName;

                        bCT.Level = (int)item.Level;

                        bCT.PrivateArea = (float)item.PrivateArea;

                        bCT.DiscountCoff = (decimal)disData.Where(l => l.Id == rentFileBCT.DiscountCoefficientId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();

                        bCT.DateCoefficient = (DateTime)item.DisposeTime;

                        bCT.DateStart = rentFile.DateHD;

                        bCT.RentFileId = RentFilesId;
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
                                    if (oldId == itemChild.defaultCoefficientsId) continue;
                                    var content = dfData.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
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
                                        if (content.Content == name)
                                        {
                                            if (bCT.DateStart >= itemChild.DoApply)
                                            {
                                                oldId = itemChild.defaultCoefficientsId;
                                                var itemChilDfs = new chilDf();
                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                itemChilDfs.Value = itemChild.Value;
                                                bCT.chilDfs.Add(itemChilDfs);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                            itemChilDfs.Value = 0;
                                            bCT.chilDfs.Add(itemChilDfs);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (bCT.DateStart >= itemChild.DoApply)
                                        {
                                            var itemChilDfs = new chilDf();

                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Value = itemChild.Value;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                            bCT.chilDfs.Add(itemChilDfs);
                                            break;
                                        }
                                        else
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Value = 0;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
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
                        //bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * (decimal)bCT.TotalK * (decimal)(1 + (bCT.VAT / 100))));
                        bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * (decimal)bCT.TotalK));

                        bCT.PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea));
                        bCT.PolicyReduction = Math.Round((decimal)(bCT.DiscountCoff * bCT.PriceRent));
                        bCT.PriceAfterDiscount = Math.Round((decimal)(bCT.PriceRent - bCT.PolicyReduction));
                        bCT.Unit = "VNĐ";
                        resBct.Add(bCT);
                    }

                }

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = resBct;
                return Ok(def);

            }
            catch (Exception ex)
            {
                return Ok(def);
            }
        }

        //API getworkSheet/QD09
        [HttpGet("GetExcelTable09/{RentFilesId}")]
        public async Task<IActionResult> GetExcelTable09(Guid RentFilesId)
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
                //Tìm hồ sơ cho thuê
                RentFile rentFile = _context.RentFiles.Where(l => l.Id == RentFilesId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                RentFlieData map_rentFlieData = _mapper.Map<RentFlieData>(rentFile);

                //Tìm bảng chiết tính mới nhất theo ID hồ sơ
                RentFileBCT rentFileBCT = _context.RentFileBCTs.Where(l => l.RentFileId == RentFilesId && l.TypeBCT == 1).OrderByDescending(p => p.CreatedAt).FirstOrDefault();

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

                List<Coefficient> coefficients = _context.Coefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList();

                List<Conversion> conversions = _context.Conversions.Where(l => l.TypeQD == AppEnums.TypeQD.QD_09).ToList();

                List<DefaultCoefficient> defaultCoefficients = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                Block blocks = _context.Blocks.Where(l => l.Id == rentFile.RentBlockId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                List<RentingPrice> rentingPrices = _context.RentingPricies.Where(l => l.TypeQD == AppEnums.TypeQD.QD_09 && l.TypeBlockId == blocks.TypeBlockId && l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.EffectiveDate).ToList();

                BlockData map_blocks = _mapper.Map<BlockData>(blocks);

                List<BlockDetail> blockDetails = _context.BlockDetails.Where(l => l.BlockId == rentFile.RentBlockId && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<BlockDetailData> map_BlockDetails = _mapper.Map<List<BlockDetailData>>(blockDetails);

                List<Floor> floorData = _context.Floors.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<Area> AreaData = _context.Areas.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<DiscountCoefficient> disData = _context.DiscountCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<DefaultCoefficient> dfData = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();

                foreach (BlockDetailData map_BlockDetail in map_BlockDetails)
                {
                    Floor floor = floorData.Where(f => f.Id == map_BlockDetail.FloorId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    map_BlockDetail.FloorName = floor != null ? floor.Name : "";
                    map_BlockDetail.FloorCode = floor != null ? floor.Code : 0;

                    Area area = AreaData.Where(f => f.Id == map_BlockDetail.AreaId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    map_BlockDetail.AreaName = area != null ? area.Name : "";
                    map_BlockDetail.IsMezzanine = area != null ? area.IsMezzanine : null;
                }
                map_blocks.blockDetails = map_BlockDetails.OrderBy(x => x.FloorCode).ThenBy(x => x.IsMezzanine).ToList();


                if (rentFile.RentApartmentId == 0 && map_blocks.blockDetails.Count == 0)
                {
                    var code = rentFile.CodeCH != null ? rentFile.CodeCH : rentFile.CodeCN;
                    def.meta = new Meta(500, "Thông tin căn nhà/căn hộ chưa được cập nhật. Vui lòng thêm thông tin căn nhà/căn hộ: " + code);
                    return Ok(def);
                }

                List<BCT> resBct = new List<BCT>();
                int oldId = 0;
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

                        bCT.GroupId = 9;

                        bCT.AreaName = item.AreaName;

                        bCT.Level = (int)item.Level;

                        bCT.PrivateArea = (float)item.PrivateArea;

                        bCT.DateCoefficient = (DateTime)item.DisposeTime;

                        bCT.DiscountCoff = (decimal)disData.Where(l => l.Id == rentFileBCT.DiscountCoefficientId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();

                        bCT.RentFileId = RentFilesId;
                        bCT.DateStart = rentFile.DateHD;

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
                                    if (oldId == itemChild.defaultCoefficientsId)
                                    {
                                        continue;
                                    }
                                    var content = dfData.Where(l => l.Id == itemChild.defaultCoefficientsId).FirstOrDefault();
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
                                        if (content.Content == name)
                                        {
                                            if (bCT.DateStart >= itemChild.DoApply)
                                            {
                                                oldId = itemChild.defaultCoefficientsId;
                                                var itemChilDfs = new chilDf();
                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                itemChilDfs.Value = itemChild.Value;
                                                bCT.chilDfs.Add(itemChilDfs);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                            itemChilDfs.Value = 0;
                                            bCT.chilDfs.Add(itemChilDfs);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (bCT.DateStart >= itemChild.DoApply)
                                        {
                                            var itemChilDfs = new chilDf();

                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Value = itemChild.Value;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                            bCT.chilDfs.Add(itemChilDfs);
                                            break;
                                        }
                                        else
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Value = 0;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
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
                        //bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * (decimal)bCT.TotalK * (decimal)(1 + (bCT.VAT / 100))));
                        bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * (decimal)bCT.TotalK));

                        bCT.PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea));
                        bCT.PolicyReduction = Math.Round((decimal)(bCT.DiscountCoff * bCT.PriceRent));
                        bCT.PriceAfterDiscount = Math.Round((decimal)(bCT.PriceRent - bCT.PolicyReduction));
                        bCT.Unit = "VNĐ";
                        resBct.Add(bCT);
                    }

                }
                else
                {
                    foreach (var item in map_blocks.blockDetails)
                    {
                        var bCT = new BCT();

                        bCT.GroupId = 9;

                        bCT.AreaName = item.AreaName;

                        bCT.Level = (int)item.Level;

                        bCT.PrivateArea = (float)item.PrivateArea;

                        bCT.DateCoefficient = (DateTime)item.DisposeTime;

                        bCT.DateStart = rentFile.DateHD;

                        bCT.DiscountCoff = (decimal)disData.Where(l => l.Id == rentFileBCT.DiscountCoefficientId).Select(p => p.Value).FirstOrDefault();

                        bCT.RentFileId = RentFilesId;
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
                                    if (oldId == itemChild.defaultCoefficientsId) continue;
                                    var content = dfData.Where(l => l.Id == itemChild.defaultCoefficientsId).FirstOrDefault();
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
                                        if (content.Content == name)
                                        {
                                            if (bCT.DateStart >= itemChild.DoApply)
                                            {
                                                var itemChilDfs = new chilDf();
                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                itemChilDfs.Value = itemChild.Value;
                                                bCT.chilDfs.Add(itemChilDfs);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                            itemChilDfs.Value = 0;
                                            bCT.chilDfs.Add(itemChilDfs);
                                            break;
                                        }
                                    }
                                    else
                                    {

                                        if (bCT.DateStart >= itemChild.DoApply)
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Value = itemChild.Value;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                            bCT.chilDfs.Add(itemChilDfs);
                                            break;
                                        }
                                        else
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Value = 0;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
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
                        bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * (decimal)bCT.TotalK * (decimal)(1 + (bCT.VAT / 100))));
                        bCT.PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea));
                        bCT.PolicyReduction = Math.Round((decimal)(bCT.DiscountCoff * bCT.PriceRent));
                        bCT.PriceAfterDiscount = Math.Round((decimal)(bCT.PriceRent - bCT.PolicyReduction));
                        bCT.Unit = "VNĐ";
                        resBct.Add(bCT);
                    }

                }


                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = resBct;
                return Ok(def);

            }
            catch (Exception ex)
            {
                return Ok(def);
            }
        }

        //API-QD22-BCT
        [HttpGet("GetExcelTableBCT22/{RentFilesId}")]
        public async Task<IActionResult> GetExcelTableBCT22(Guid RentFilesId)
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
                //Tìm hồ sơ cho thuê
                RentFile rentFile = _context.RentFiles.Where(l => l.Id == RentFilesId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                RentFlieData map_rentFlieData = _mapper.Map<RentFlieData>(rentFile);

                //Tìm bảng chiết tính mới nhất theo ID hồ sơ
                RentFileBCT rentFileBCT = _context.RentFileBCTs.Where(l => l.RentFileId == RentFilesId && l.TypeBCT == 1).FirstOrDefault();

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

                List<Coefficient> coefficients = _context.Coefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList();

                List<Conversion> conversions = _context.Conversions.Where(l => l.TypeQD == AppEnums.TypeQD.QD_22).ToList();

                List<DefaultCoefficient> defaultCoefficients = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                Block blocks = _context.Blocks.Where(l => l.Id == rentFile.RentBlockId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                List<RentingPrice> rentingPrices = _context.RentingPricies.Where(l => l.TypeQD == AppEnums.TypeQD.QD_22 && l.TypeBlockId == blocks.TypeBlockId && l.Status != AppEnums.EntityStatus.DELETED).ToList();

                BlockData map_blocks = _mapper.Map<BlockData>(blocks);

                List<Floor> floorData = _context.Floors.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<Area> AreaData = _context.Areas.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<DiscountCoefficient> disData = _context.DiscountCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<DefaultCoefficient> dfData = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();

                List<BlockDetail> blockDetails = _context.BlockDetails.Where(l => l.BlockId == rentFile.RentBlockId && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<BlockDetailData> map_BlockDetails = _mapper.Map<List<BlockDetailData>>(blockDetails);
                foreach (BlockDetailData map_BlockDetail in map_BlockDetails)
                {
                    Floor floor = floorData.Where(f => f.Id == map_BlockDetail.FloorId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    map_BlockDetail.FloorName = floor != null ? floor.Name : "";
                    map_BlockDetail.FloorCode = floor != null ? floor.Code : 0;

                    Area area = AreaData.Where(f => f.Id == map_BlockDetail.AreaId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    map_BlockDetail.AreaName = area != null ? area.Name : "";
                    map_BlockDetail.IsMezzanine = area != null ? area.IsMezzanine : null;
                }
                map_blocks.blockDetails = map_BlockDetails.OrderBy(x => x.FloorCode).ThenBy(x => x.IsMezzanine).ToList();
                List<Apartment> apartments = _context.Apartments.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<ApartmentDetail> details = _context.ApartmentDetails.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();


                List<BCT> resBct = new List<BCT>();
                int i = 0;
                int check = 0;

                if (rentFile.RentApartmentId == 0 && map_blocks.blockDetails.Count == 0)
                {
                    var code = rentFile.CodeCH != null ? rentFile.CodeCH : rentFile.CodeCN;
                    def.meta = new Meta(500, "Thông tin căn nhà/căn hộ chưa được cập nhật. Vui lòng thêm thông tin căn nhà/căn hộ: " + code);
                    return Ok(def);
                }


                for (int z = 0; z <= rentFile.Month; z++)
                {
                    List<int> listId = new List<int>();
                    decimal sum = 0;
                    decimal sumpo = 0;
                    if (rentFile.RentApartmentId > 0)
                    {
                        Apartment apartment = apartments.Where(l => l.Id == rentFile.RentApartmentId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                        ApartmentData map_apartments = _mapper.Map<ApartmentData>(apartment);
                        List<ApartmentDetail> apartmentDetails = details.Where(a => a.TargetId == rentFile.RentApartmentId && a.Type == TypeApartmentDetail.APARTMENT && a.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                        List<ApartmentDetailData> apartmentDetailData = _mapper.Map<List<ApartmentDetailData>>(apartmentDetails);
                        foreach (ApartmentDetailData map_apartmentDetail in apartmentDetailData)
                        {
                            Area area = AreaData.Where(f => f.Id == map_apartmentDetail.AreaId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            map_apartmentDetail.AreaName = area != null ? area.Name : "";
                            map_apartmentDetail.IsMezzanine = area != null ? area.IsMezzanine : null;

                            Floor floor = floorData.Where(f => f.Id == map_apartmentDetail.FloorId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            map_apartmentDetail.FloorName = floor != null ? floor.Name : "";
                            map_apartmentDetail.FloorCode = floor != null ? floor.Code : 0;
                        }
                        map_apartments.apartmentDetailData = apartmentDetailData.ToList();
                        foreach (var item in map_apartments.apartmentDetailData)
                        {
                            var bCT = new BCT();

                            bCT.check = false;

                            bCT.GroupId = z;

                            bCT.AreaName = item.AreaName;

                            bCT.Level = (int)item.Level;

                            bCT.PrivateArea = (float)item.PrivateArea;

                            bCT.DateStart = rentFile.DateHD.AddMonths(z);

                            bCT.DateCoefficient = (DateTime)item.DisposeTime;

                            bCT.DiscountCoff = (decimal)disData.Where(l => l.Id == rentFileBCT.DiscountCoefficientId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();
                            bCT.RentFileId = RentFilesId;
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

                            foreach (var salaryCoefficient in salaryCoefficients) // lấy giá trị của lương cơ bản theo ngày áp dụng
                            {
                                if (bCT.DateStart >= salaryCoefficient.DoApply)
                                {
                                    bCT.Ktlcb = (decimal)salaryCoefficient.Value;
                                    break;
                                }
                            }

                            foreach (var coefficient in coefficients) // lấy giá trị của hệ số thời điểm bố trí theo ngày áp dụng
                            {
                                if (bCT.DateCoefficient >= coefficient.DoApply)
                                {
                                    bCT.Ktdbt = (decimal)coefficient.Value;
                                    break;
                                }
                            }

                            bCT.chilDfs = new List<chilDf>();


                            foreach (var itemDf in map_rentFlieBCTDatas.groupData)
                            {
                                if (itemDf.childDefaultCoefficients.Count > 0)
                                {
                                    var countList = itemDf.childDefaultCoefficients.Count;
                                    foreach (var itemChild in itemDf.childDefaultCoefficients.OrderByDescending(p => p.DoApply))
                                    {

                                        Boolean flagCheck = false;
                                        foreach (var oldid in listId)
                                        {
                                            if (oldid == itemChild.defaultCoefficientsId && listId.Count > 1)
                                            {
                                                flagCheck = true;
                                                break;
                                            }
                                        }
                                        if (flagCheck == true)
                                        {
                                            continue;
                                        }
                                        var content = dfData.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                                        if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                        {
                                            if (content.Content == bCT.AreaName)
                                            {
                                                if (bCT.DateStart >= itemChild.DoApply)
                                                {
                                                    int oldId = itemChild.defaultCoefficientsId;
                                                    listId.Add(oldId);

                                                    var itemChilDfs = new chilDf();
                                                    itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                    itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                    itemChilDfs.Value = itemChild.Value;
                                                    bCT.chilDfs.Add(itemChilDfs);
                                                    break;
                                                }
                                                else if (bCT.DateStart < itemChild.DoApply && countList > 1)
                                                {
                                                    countList--;
                                                    continue;
                                                }
                                                else if (bCT.DateStart < itemChild.DoApply && countList == 1)
                                                {
                                                    var itemChilDfs = new chilDf();
                                                    itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                    itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                    itemChilDfs.Value = 0;
                                                    bCT.chilDfs.Add(itemChilDfs);
                                                    break;
                                                }
                                            }
                                            else if (content.Content != bCT.AreaName && countList == 1)
                                            {
                                                var itemChilDfs = new chilDf();
                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                itemChilDfs.Value = 0;
                                                bCT.chilDfs.Add(itemChilDfs);
                                            }
                                            else if (content.Content != bCT.AreaName && countList > 1)
                                            {
                                                countList--;
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            if (bCT.DateStart >= itemChild.DoApply)
                                            {
                                                var itemChilDfs = new chilDf();
                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                itemChilDfs.Value = itemChild.Value;
                                                itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                bCT.chilDfs.Add(itemChilDfs);
                                                break;
                                            }
                                            else if (bCT.DateStart < itemChild.DoApply && countList == 1)
                                            {
                                                var itemChilDfs = new chilDf();
                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                itemChilDfs.Value = 0;
                                                itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                bCT.chilDfs.Add(itemChilDfs);
                                                break;
                                            }
                                            else if (bCT.DateStart < itemChild.DoApply && countList > 1)
                                            {
                                                countList--;
                                                continue;
                                            }
                                        }
                                    }
                                }
                            }

                            if (bCT.StandardPrice == null) bCT.StandardPrice = 0;

                            bCT.TotalK = (float)bCT.chilDfs.Sum(p => p.Value) + 1;
                            if (bCT.Ktlcb == null) bCT.Ktlcb = 1;
                            if (bCT.Ktdbt == null) bCT.Ktdbt = 1;

                            bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * bCT.Ktlcb * bCT.Ktdbt * (decimal)bCT.TotalK));
                            //bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * bCT.Ktlcb * bCT.Ktdbt * (decimal)bCT.TotalK * (decimal)(1 + (bCT.VAT / 100))));

                            bCT.PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea));
                            bCT.PolicyReduction = Math.Round((decimal)(bCT.DiscountCoff * bCT.PriceRent));
                            bCT.PriceAfterDiscount = Math.Round((decimal)(bCT.PriceRent - bCT.PolicyReduction));
                            sum += (decimal)bCT.PriceAfterDiscount;
                            sumpo += bCT.PolicyReduction;
                            bCT.Unit = "VNĐ";

                            if (resBct.Where(e => e.AreaName == bCT.AreaName && e.TotalK == bCT.TotalK && e.VAT == bCT.VAT && e.Ktlcb == bCT.Ktlcb && e.Ktdbt == bCT.Ktdbt && e.PolicyReduction == bCT.PolicyReduction).FirstOrDefault() == null)
                            {
                                resBct.Add(bCT);
                                check = 1;
                                if (i > 0)
                                {
                                    resBct[i - 1].DateEnd = bCT.DateStart.Value.AddDays(-1);
                                }
                                i++;
                            }
                            else
                            {
                                check = 2;
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in map_blocks.blockDetails)
                        {
                            var bCTBL = new BCT();

                            bCTBL.check = false;

                            bCTBL.GroupId = z;

                            bCTBL.AreaName = item.AreaName;

                            bCTBL.Level = (int)item.Level;

                            bCTBL.PrivateArea = (float)item.PrivateArea;

                            bCTBL.DateStart = rentFile.DateHD.AddMonths(z);

                            bCTBL.DateCoefficient = (DateTime)item.DisposeTime;

                            bCTBL.DiscountCoff = (decimal)disData.Where(l => l.Id == rentFileBCT.DiscountCoefficientId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();
                            bCTBL.RentFileId = RentFilesId;
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
                                if (bCTBL.DateStart >= vat.DoApply) // lấy giá trị của VAT theo dateStart
                                {
                                    bCTBL.VAT = vat.Value;
                                    break;
                                }
                            }

                            foreach (var salaryCoefficient in salaryCoefficients) // lấy giá trị của lương cơ bản theo ngày áp dụng
                            {
                                if (bCTBL.DateStart >= salaryCoefficient.DoApply)
                                {
                                    bCTBL.Ktlcb = (decimal)salaryCoefficient.Value;
                                    break;
                                }
                            }

                            foreach (var coefficient in coefficients) // lấy giá trị của hệ số thời điểm bố trí theo ngày áp dụng
                            {
                                if (bCTBL.DateCoefficient >= coefficient.DoApply)
                                {
                                    bCTBL.Ktdbt = (decimal)coefficient.Value;
                                    break;
                                }
                            }

                            bCTBL.chilDfs = new List<chilDf>();

                            foreach (var itemDf in map_rentFlieBCTDatas.groupData)
                            {
                                if (itemDf.childDefaultCoefficients.Count > 0)
                                {
                                    var countList = itemDf.childDefaultCoefficients.Count;
                                    foreach (var itemChild in itemDf.childDefaultCoefficients.OrderByDescending(p => p.DoApply))
                                    {

                                        Boolean flagCheck = false;
                                        foreach (var oldid in listId)
                                        {
                                            if (oldid == itemChild.defaultCoefficientsId && listId.Count > 1)
                                            {
                                                flagCheck = true;
                                                break;
                                            }
                                        }
                                        if (flagCheck == true)
                                        {
                                            continue;
                                        }
                                        var content = dfData.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                                        if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                        {
                                            if (content.Content == bCTBL.AreaName)
                                            {
                                                if (bCTBL.DateStart >= itemChild.DoApply)
                                                {
                                                    int oldId = itemChild.defaultCoefficientsId;
                                                    listId.Add(oldId);

                                                    var itemChilDfs = new chilDf();
                                                    itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                    itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                    itemChilDfs.Value = itemChild.Value;
                                                    bCTBL.chilDfs.Add(itemChilDfs);
                                                    break;
                                                }
                                                else if (bCTBL.DateStart < itemChild.DoApply && countList > 1)
                                                {
                                                    countList--;
                                                    continue;
                                                }
                                                else if (bCTBL.DateStart < itemChild.DoApply && countList == 1)
                                                {
                                                    var itemChilDfs = new chilDf();
                                                    itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                    itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                    itemChilDfs.Value = 0;
                                                    bCTBL.chilDfs.Add(itemChilDfs);
                                                    break;
                                                }
                                            }
                                            else if (content.Content != bCTBL.AreaName && countList == 1)
                                            {
                                                var itemChilDfs = new chilDf();
                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                itemChilDfs.Value = 0;
                                                bCTBL.chilDfs.Add(itemChilDfs);
                                            }
                                            else if (content.Content != bCTBL.AreaName && countList > 1)
                                            {
                                                countList--;
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            if (bCTBL.DateStart >= itemChild.DoApply)
                                            {
                                                var itemChilDfs = new chilDf();
                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                itemChilDfs.Value = itemChild.Value;
                                                itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                bCTBL.chilDfs.Add(itemChilDfs);
                                                break;
                                            }
                                            else if (bCTBL.DateStart < itemChild.DoApply && countList == 1)
                                            {
                                                var itemChilDfs = new chilDf();
                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                itemChilDfs.Value = 0;
                                                itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                bCTBL.chilDfs.Add(itemChilDfs);
                                                break;
                                            }
                                            else if (bCTBL.DateStart < itemChild.DoApply && countList > 1)
                                            {
                                                countList--;
                                                continue;
                                            }
                                        }
                                    }
                                }
                            }

                            if (bCTBL.StandardPrice == null) bCTBL.StandardPrice = 0;

                            bCTBL.TotalK = (float)bCTBL.chilDfs.Sum(p => p.Value) + 1;
                            if (bCTBL.Ktlcb == null) bCTBL.Ktlcb = 1;
                            if (bCTBL.Ktdbt == null) bCTBL.Ktdbt = 1;

                            //bCTBL.PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK * (decimal)(1 + (bCTBL.VAT / 100))));
                            bCTBL.PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK));

                            bCTBL.PriceRent = Math.Round((decimal)(bCTBL.PriceRent1m2 * (decimal)bCTBL.PrivateArea));
                            bCTBL.PolicyReduction = Math.Round((decimal)(bCTBL.DiscountCoff * bCTBL.PriceRent));
                            bCTBL.PriceAfterDiscount = Math.Round((decimal)(bCTBL.PriceRent - bCTBL.PolicyReduction));
                            sum += (decimal)bCTBL.PriceAfterDiscount;
                            sumpo += bCTBL.PolicyReduction;
                            bCTBL.Unit = "VNĐ";

                            //                   if (resBct.Where(e => e.AreaName == bCTBL.AreaName && e.TotalK == bCTBL.TotalK && e.VAT == bCTBL.VAT && e.Ktlcb == bCTBL.Ktlcb && e.Ktdbt == bCTBL.Ktdbt && e.PolicyReduction == bCTBL.PolicyReduction
                            //                       && (e.chilDfs.Select(x => x.Value)
                            //.Intersect(bCTBL.chilDfs.Select(x => x.Value))
                            //.Count() == e.chilDfs.Count && e.chilDfs.Count == bCTBL.chilDfs.Count) == true).FirstOrDefault() == null)
                            if (resBct.Where(e => e.AreaName == bCTBL.AreaName && e.TotalK == bCTBL.TotalK && e.VAT == bCTBL.VAT && e.Ktlcb == bCTBL.Ktlcb && e.Ktdbt == bCTBL.Ktdbt && e.PolicyReduction == bCTBL.PolicyReduction).FirstOrDefault() == null)
                            {
                                resBct.Add(bCTBL);
                                check = 1;
                                if (i > 0)
                                {
                                    resBct[i - 1].DateEnd = bCTBL.DateStart.Value.AddDays(-1);
                                }
                                i++;
                            }
                            else
                            {
                                check = 2;
                            }
                        }

                    }
                    if (check == 1)
                    {
                        i++;
                        var total = new BCT();
                        total.GroupId = z;
                        total.AreaName = "Tổng";
                        total.StandardPrice = null;
                        total.Level = null;
                        total.PrivateArea = null;
                        total.DateCoefficient = null;
                        total.TotalK = null;
                        total.Ktdbt = null;
                        total.Ktlcb = null;
                        total.PriceRent1m2 = null;
                        total.Unit = null;
                        total.DateDiff = 0;
                        total.Note = null;
                        total.check = true;
                        total.RentFileId = RentFilesId;
                        total.chilDfs = new List<chilDf>();

                        foreach (var itemDf in map_rentFlieBCTDatas.groupData)
                        {
                            if (itemDf.childDefaultCoefficients.Count > 0)
                            {
                                foreach (var itemChild in itemDf.childDefaultCoefficients)
                                {
                                    if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                    {
                                        var itemChilDfs = new chilDf();

                                        itemChilDfs.Value = null;
                                        total.chilDfs.Add(itemChilDfs);
                                        break;
                                    }
                                    else
                                    {
                                        var itemChilDfs = new chilDf();

                                        itemChilDfs.Value = null;
                                        total.chilDfs.Add(itemChilDfs);
                                        break;
                                    }
                                }
                            }
                        }
                        total.DateStart = null;
                        total.PriceAfterDiscount = sum;
                        total.PolicyReduction = sumpo;
                        resBct.Add(total);
                    }
                }

                var groupData = resBct.GroupBy(f => f.GroupId != null ? f.GroupId : new object(), key => key).Select(f => new groupData
                {
                    GroupId = (int)f.Key,
                    bCTs = f.ToList()
                });

                List<groupData> res = new List<groupData>();
                foreach (var item in groupData.ToList())
                {
                    item.DateStart = (DateTime)item.bCTs.First().DateStart;
                    item.DateEnd = item.bCTs.Last().DateEnd;
                    if (item.DateEnd.HasValue)
                    {
                        item.DateDiff = (item.DateEnd.Value.Year - item.bCTs.First().DateStart.Value.Year) * 12 + (item.DateEnd.Value.Month - item.bCTs.First().DateStart.Value.Month);
                    }
                    item.totalPrice = (decimal)((decimal)item.bCTs.Sum(p => p.PriceAfterDiscount) - item.bCTs[item.bCTs.Count - 1].PriceAfterDiscount);
                    item.VAT = item.bCTs.First().VAT;
                    item.rentFileId = item.bCTs.First().RentFileId;
                    res.Add(item);
                }
                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = res;
                return Ok(def);
            }
            catch (Exception ex)
            {
                return Ok(def);
            }
        }

        //API -QD22-Tính công nợ
        //[HttpGet("GetExcelTableCNQD22/{Code}")]
        //public async Task<IActionResult> GetExcelTableCNQD22(string Code)
        //{
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

        //        List<Vat> vats = _context.Vats.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList();

        //        List<SalaryCoefficient> salaryCoefficients = _context.SalaryCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList();

        //        List<Coefficient> coefficients = _context.Coefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList();

        //        List<Conversion> conversions = _context.Conversions.Where(l => l.TypeQD == AppEnums.TypeQD.QD_1753).ToList();

        //        List<BCT> resBct = new List<BCT>();

        //        //Tìm hồ sơ cho thuê
        //        List<RentFile> rentFiles = _context.RentFiles.Where(l => l.CodeCN == Code || l.CodeCH == Code && l.Status != AppEnums.EntityStatus.DELETED).ToList();
        //        foreach (RentFile rentFile in rentFiles)
        //        {
        //            RentFlieData map_rentFlieData = _mapper.Map<RentFlieData>(rentFile);

        //            //Tìm bảng chiết tính mới nhất theo ID hồ sơ
        //            RentFileBCT rentFileBCT = _context.RentFileBCTs.Where(l => l.RentFileId == rentFile.Id && l.TypeBCT == 1 && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
        //            RentFileBCTData map_rentFlieBCTDatas = _mapper.Map<RentFileBCTData>(rentFileBCT);

        //            if (map_rentFlieBCTDatas != null)
        //            {
        //                List<DefaultCoefficient> dataDf = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderBy(p => p.CoefficientName).ToList();
        //                var groupedData = dataDf.GroupBy(x => x.CoefficientName).Select(e => new GroupData
        //                {
        //                    CoefficientId = e.Key,
        //                    defaultCoefficients = e.ToList(),
        //                    childDefaultCoefficients = _context.ChildDefaults.Where(l => l.RentFileBctId == map_rentFlieBCTDatas.Id && l.CoefficientId == e.Key && l.Type == 1 && l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList()
        //                }).ToList();
        //                map_rentFlieBCTDatas.groupData = groupedData;
        //            }

        //            List<DefaultCoefficient> defaultCoefficients = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
        //            Block blocks = _context.Blocks.Where(l => l.Id == rentFile.RentBlockId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

        //            List<RentingPrice> rentingPrices = _context.RentingPricies.Where(l => l.TypeQD == AppEnums.TypeQD.QD_22 && l.TypeBlockId == blocks.TypeBlockId && l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.EffectiveDate).ToList();

        //            BlockData map_blocks = _mapper.Map<BlockData>(blocks);

        //            List<BlockDetail> blockDetails = _context.BlockDetails.Where(l => l.BlockId == rentFile.RentBlockId && l.Status != AppEnums.EntityStatus.DELETED).ToList();
        //            List<BlockDetailData> map_BlockDetails = _mapper.Map<List<BlockDetailData>>(blockDetails);
        //            foreach (BlockDetailData map_BlockDetail in map_BlockDetails)
        //            {
        //                Floor floor = _context.Floors.Where(f => f.Id == map_BlockDetail.FloorId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
        //                map_BlockDetail.FloorName = floor != null ? floor.Name : "";
        //                map_BlockDetail.FloorCode = floor != null ? floor.Code : 0;

        //                Area area = _context.Areas.Where(f => f.Id == map_BlockDetail.AreaId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
        //                map_BlockDetail.AreaName = area != null ? area.Name : "";
        //                map_BlockDetail.IsMezzanine = area != null ? area.IsMezzanine : null;
        //            }
        //            map_blocks.blockDetails = map_BlockDetails.OrderBy(x => x.FloorCode).ThenBy(x => x.IsMezzanine).ToList();

        //            List<changeValue> resChange = new List<changeValue>();
        //            List<changeK> resChangeK = new List<changeK>();
        //            int i = 0;
        //            int check = 0;

        //            for (int z = 0; z < rentFile.Month; z++)
        //            {
        //                decimal sum = 0;

        //                decimal VAT = 0;
        //                decimal Ktlcb = 0;
        //                decimal Ktdbt = 0;
        //                decimal K = 0;

        //                if (rentFile.RentApartmentId > 0)
        //                {
        //                    Apartment apartment = _context.Apartments.Where(l => l.Id == rentFile.RentApartmentId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
        //                    ApartmentData map_apartments = _mapper.Map<ApartmentData>(apartment);
        //                    Block block = _context.Blocks.Where(b => b.Id == apartment.BlockId).FirstOrDefault();
        //                    if (block != null)
        //                    {
        //                        map_apartments.BlockName = block.Address;
        //                        map_apartments.TypeBlockId = block.TypeBlockId;
        //                    }
        //                    map_apartments.apartmentDetails = _context.ApartmentDetails.Where(a => a.TargetId == rentFile.RentApartmentId && a.Type == TypeApartmentDetail.APARTMENT && a.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();

        //                    foreach (var item in map_apartments.apartmentDetails)
        //                    {
        //                        var bCT = new BCT();

        //                        bCT.RentFileId = rentFile.Id;

        //                        bCT.check = false;

        //                        bCT.GroupId = z;

        //                        bCT.AreaName = map_apartments.BlockName;

        //                        bCT.Level = (int)item.Level;

        //                        bCT.PrivateArea = (float)item.PrivateArea;

        //                        if (z == 0)
        //                        {
        //                            bCT.DateStart = rentFile.DateHD;
        //                            bCT.DateEnd = bCT.DateStart.Value.AddMonths(1).AddDays(-1);
        //                        }
        //                        else
        //                        {
        //                            bCT.DateStart = rentFile.DateHD.AddMonths(z);
        //                            bCT.DateEnd = bCT.DateStart.Value.AddMonths(1).AddDays(-1);
        //                        }

        //                        DateTime DateStart = bCT.DateStart.Value;
        //                        DateTime DateEnd = bCT.DateEnd.Value;
        //                        // Lấy TimeSpan giữa hai ngày
        //                        TimeSpan DateSpan = DateEnd.Subtract(DateStart);
        //                        // Lấy số ngày chênh lệch
        //                        int DateDiff = (int)DateSpan.TotalDays;

        //                        bCT.DateCoefficient = (DateTime)item.DisposeTime;

        //                        foreach (var itemLv in rentingPrices) //Lấy giá chuẩn
        //                        {
        //                            if (itemLv.LevelId == item.Level && itemLv.TypeBlockId == blocks.TypeBlockId)
        //                            {
        //                                bCT.StandardPrice = (decimal)itemLv.Price;
        //                            }
        //                        }

        //                        foreach (var vat in vats)
        //                        {
        //                            if (bCT.DateStart >= vat.DoApply)
        //                            {
        //                                bCT.VAT = vat.Value;
        //                                break;
        //                            }
        //                            else if (vat.DoApply <= bCT.DateEnd)
        //                            {
        //                                var changevalue = new changeValue();

        //                                changevalue.Type = 1;
        //                                changevalue.valueChange = vat.Value;
        //                                changevalue.DateChange = vat.DoApply;

        //                                resChange.Add(changevalue);
        //                                continue;
        //                            }
        //                        }

        //                        foreach (var salaryCoefficient in salaryCoefficients) // lấy giá trị của lương cơ bản theo ngày áp dụng
        //                        {
        //                            if (bCT.DateStart >= salaryCoefficient.DoApply)
        //                            {
        //                                bCT.Ktlcb = (decimal)salaryCoefficient.Value;
        //                                break;
        //                            }
        //                            else if (salaryCoefficient.DoApply <= bCT.DateEnd)
        //                            {
        //                                var changevalue = new changeValue();

        //                                changevalue.Type = 2;
        //                                changevalue.valueChange = salaryCoefficient.Value;
        //                                changevalue.DateChange = salaryCoefficient.DoApply;

        //                                resChange.Add(changevalue);
        //                                continue;
        //                            }
        //                        }

        //                        foreach (var coefficient in coefficients) // lấy giá trị của hệ số thời điểm bố trí theo ngày áp dụng
        //                        {
        //                            if (bCT.DateStart >= coefficient.DoApply)
        //                            {
        //                                bCT.Ktdbt = (decimal)coefficient.Value;
        //                                break;
        //                            }
        //                            else if (coefficient.DoApply <= bCT.DateEnd)
        //                            {
        //                                var changevalue = new changeValue();

        //                                changevalue.Type = 3;
        //                                changevalue.valueChange = coefficient.Value;
        //                                changevalue.DateChange = coefficient.DoApply;

        //                                resChange.Add(changevalue);
        //                                continue;
        //                            }
        //                        }

        //                        bCT.chilDfs = new List<chilDf>();
        //                        if (map_rentFlieBCTDatas != null)
        //                        {
        //                            foreach (var itemDf in map_rentFlieBCTDatas.groupData)
        //                            {
        //                                if (itemDf.childDefaultCoefficients.Count > 0)
        //                                {
        //                                    foreach (var itemChild in itemDf.childDefaultCoefficients)
        //                                    {
        //                                        var itemChilDfs = new chilDf();

        //                                        if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
        //                                        {
        //                                            string content = _context.DefaultCoefficients.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Content).FirstOrDefault();
        //                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
        //                                            if (content == bCT.AreaName)
        //                                            {
        //                                                if (bCT.DateStart >= itemChild.DoApply)
        //                                                {
        //                                                    itemChilDfs.Value = itemChild.Value;
        //                                                    bCT.chilDfs.Add(itemChilDfs);
        //                                                    break;
        //                                                }
        //                                                else if (bCT.DateEnd <= itemChild.DoApply)
        //                                                {
        //                                                    var changeK = new changeK();

        //                                                    changeK.valueChange = itemChild.Value;
        //                                                    changeK.DateChange = itemChild.DoApply;
        //                                                    resChangeK.Add(changeK);
        //                                                }
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            if (bCT.DateStart >= itemChild.DoApply)
        //                                            {
        //                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
        //                                                itemChilDfs.Value = itemChild.Value;
        //                                                bCT.chilDfs.Add(itemChilDfs);
        //                                                break;
        //                                            }
        //                                            else if (bCT.DateEnd <= itemChild.DoApply)
        //                                            {
        //                                                var changeK = new changeK();

        //                                                changeK.CoefficientId = itemChild.CoefficientId;
        //                                                changeK.valueChange = itemChild.Value;
        //                                                changeK.DateChange = itemChild.DoApply;
        //                                                resChangeK.Add(changeK);
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }

        //                            foreach (var itemDf in map_rentFlieBCTDatas.groupData)
        //                            {
        //                                if (itemDf.childDefaultCoefficients.Count > 0)
        //                                {
        //                                    foreach (var itemChild in itemDf.childDefaultCoefficients)
        //                                    {
        //                                        if (itemChild.CoefficientId == AppEnums.TypeTable.HSGG)
        //                                        {
        //                                            if (bCT.DateStart >= itemChild.DoApply)
        //                                            {
        //                                                bCT.PolicyReduction = (decimal)itemChild.Value;
        //                                                break;
        //                                            }
        //                                            else if (itemChild.DoApply <= bCT.DateEnd)
        //                                            {
        //                                                var changevalue = new changeValue();
        //                                                changevalue.Type = 4;
        //                                                changevalue.valueChange = itemChild.Value;
        //                                                changevalue.DateChange = itemChild.DoApply;

        //                                                resChange.Add(changevalue);
        //                                                continue;
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }

        //                        if (bCT.StandardPrice == null) bCT.StandardPrice = 0;

        //                        if (bCT.chilDfs != null)
        //                        {
        //                            bCT.TotalK = (float)bCT.chilDfs.Sum(p => p.Value) + 1;
        //                        }
        //                        else
        //                        {
        //                            bCT.TotalK = 1;
        //                        }

        //                        if (bCT.Ktlcb == null) bCT.Ktlcb = 1;
        //                        if (bCT.Ktdbt == null) bCT.Ktdbt = 1;

        //                        bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * bCT.Ktlcb * bCT.Ktdbt * (decimal)bCT.TotalK * (decimal)(1 + (bCT.VAT / 100))));

        //                        bCT.PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea) - bCT.PolicyReduction);

        //                        bCT.Unit = "VNĐ";

        //                        if (resChangeK.Count > 0)
        //                        {
        //                            foreach (var itemK in bCT.chilDfs)
        //                            {
        //                                foreach (var itemChangeK in resChangeK.OrderBy(o => o.DateChange).ToList())
        //                                {
        //                                    if (itemK.CoefficientId == itemChangeK.CoefficientId)
        //                                    {
        //                                        decimal totalK = 0;
        //                                        var changevalue = new changeValue();

        //                                        totalK = (decimal)(bCT.TotalK - itemK.Value);
        //                                        changevalue.valueChange = (double)(totalK + (decimal)itemChangeK.valueChange);
        //                                        changevalue.Type = 5;
        //                                        changevalue.DateChange = itemChangeK.DateChange;
        //                                        resChange.Add(changevalue);
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        if (resChange.Count > 0)
        //                        {
        //                            foreach (var itemChange in resChange.OrderBy(p => p.DateChange).ToList())
        //                            {
        //                                decimal? PriceRent1m2 = 0;
        //                                decimal? PriceRent = 0;

        //                                if (itemChange.Type == 1)
        //                                {
        //                                    bCT.VAT = itemChange.valueChange;
        //                                    PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * bCT.Ktlcb * bCT.Ktdbt * (decimal)bCT.TotalK * (decimal)(1 + (bCT.VAT / 100))));

        //                                    PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea) - bCT.PolicyReduction);

        //                                    DateTime endChange = itemChange.DateChange;
        //                                    // Lấy TimeSpan giữa hai ngày
        //                                    TimeSpan spanChange = endChange.Subtract(DateStart);
        //                                    // Lấy số ngày chênh lệch
        //                                    int dateDiffChange = (int)spanChange.TotalDays; //Ngày giá cũ

        //                                    int NewDateChange = DateDiff - dateDiffChange; //Ngày giá mới

        //                                    decimal NewPrice = Math.Round(((decimal)(bCT.PriceRent / DateDiff) * dateDiffChange) + ((decimal)(PriceRent / DateDiff) * NewDateChange));
        //                                }
        //                                else if (itemChange.Type == 2)
        //                                {
        //                                    bCT.Ktlcb = (decimal)itemChange.valueChange;
        //                                    PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * bCT.Ktlcb * bCT.Ktdbt * (decimal)bCT.TotalK * (decimal)(1 + (bCT.VAT / 100))));

        //                                    PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea) - bCT.PolicyReduction);

        //                                    DateTime endChange = itemChange.DateChange;
        //                                    // Lấy TimeSpan giữa hai ngày
        //                                    TimeSpan spanChange = endChange.Subtract(DateStart);
        //                                    // Lấy số ngày chênh lệch
        //                                    int dateDiffChange = (int)spanChange.TotalDays; //Ngày giá cũ

        //                                    int NewDateChange = DateDiff - dateDiffChange; //Ngày giá mới

        //                                    decimal NewPrice = Math.Round(((decimal)(bCT.PriceRent / DateDiff) * dateDiffChange) + ((decimal)(PriceRent / DateDiff) * NewDateChange));
        //                                }
        //                                else if (itemChange.Type == 3)
        //                                {
        //                                    bCT.Ktdbt = (decimal)itemChange.valueChange;

        //                                    PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * bCT.Ktlcb * bCT.Ktdbt * (decimal)bCT.TotalK * (decimal)(1 + (bCT.VAT / 100))));

        //                                    PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea) - bCT.PolicyReduction);

        //                                    DateTime endChange = itemChange.DateChange;
        //                                    // Lấy TimeSpan giữa hai ngày
        //                                    TimeSpan spanChange = endChange.Subtract(DateStart);
        //                                    // Lấy số ngày chênh lệch
        //                                    int dateDiffChange = (int)spanChange.TotalDays; //Ngày giá cũ

        //                                    int NewDateChange = DateDiff - dateDiffChange; //Ngày giá mới

        //                                    decimal NewPrice = Math.Round(((decimal)(bCT.PriceRent / DateDiff) * dateDiffChange) + ((decimal)(PriceRent / DateDiff) * NewDateChange));
        //                                }
        //                                else if (itemChange.Type == 4)
        //                                {
        //                                    bCT.PolicyReduction = (decimal)itemChange.valueChange;

        //                                    PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * bCT.Ktlcb * bCT.Ktdbt * (decimal)bCT.TotalK * (decimal)(1 + (bCT.VAT / 100))));

        //                                    PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea) - bCT.PolicyReduction);

        //                                    DateTime endChange = itemChange.DateChange;
        //                                    // Lấy TimeSpan giữa hai ngày
        //                                    TimeSpan spanChange = endChange.Subtract(DateStart);
        //                                    // Lấy số ngày chênh lệch
        //                                    int dateDiffChange = (int)spanChange.TotalDays; //Ngày giá cũ

        //                                    int NewDateChange = DateDiff - dateDiffChange; //Ngày giá mới

        //                                    decimal NewPrice = Math.Round(((decimal)(bCT.PriceRent / DateDiff) * dateDiffChange) + ((decimal)(PriceRent / DateDiff) * NewDateChange));
        //                                }
        //                                else if (itemChange.Type == 5)
        //                                {
        //                                    bCT.TotalK = (float?)itemChange.valueChange;

        //                                    PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * bCT.Ktlcb * bCT.Ktdbt * (decimal)bCT.TotalK * (decimal)(1 + (bCT.VAT / 100))));

        //                                    PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea) - bCT.PolicyReduction);

        //                                    DateTime endChange = itemChange.DateChange;
        //                                    // Lấy TimeSpan giữa hai ngày
        //                                    TimeSpan spanChange = endChange.Subtract(DateStart);
        //                                    // Lấy số ngày chênh lệch
        //                                    int dateDiffChange = (int)spanChange.TotalDays; //Ngày giá cũ

        //                                    int NewDateChange = DateDiff - dateDiffChange; //Ngày giá mới

        //                                    decimal NewPrice = Math.Round(((decimal)(bCT.PriceRent / DateDiff) * dateDiffChange) + ((decimal)(PriceRent / DateDiff) * NewDateChange));
        //                                }
        //                                bCT.PriceRent1m2 = PriceRent1m2;
        //                                bCT.PriceRent = PriceRent;
        //                            }
        //                        }
        //                        resBct.Add(bCT);
        //                    }
        //                }
        //                else
        //                {
        //                    foreach (var item in map_blocks.blockDetails.ToList())
        //                    {
        //                        var bCTBL = new BCT();

        //                        bCTBL.RentFileId = rentFile.Id;

        //                        bCTBL.check = false;

        //                        bCTBL.GroupId = z;

        //                        bCTBL.AreaName = item.AreaName;

        //                        bCTBL.Level = (int)item.Level;

        //                        bCTBL.PrivateArea = (float)item.PrivateArea;

        //                        if (z == 0)
        //                        {
        //                            bCTBL.DateStart = rentFile.DateHD;
        //                            bCTBL.DateEnd = bCTBL.DateStart.Value.AddMonths(1).AddDays(-1);

        //                        }
        //                        else
        //                        {
        //                            bCTBL.DateStart = rentFile.DateHD.AddMonths(z);
        //                            bCTBL.DateEnd = bCTBL.DateStart.Value.AddMonths(1).AddDays(-1);
        //                        }


        //                        DateTime DateStart = bCTBL.DateStart.Value;
        //                        DateTime DateEnd = bCTBL.DateEnd.Value;
        //                        // Lấy TimeSpan giữa hai ngày
        //                        TimeSpan DateSpan = DateEnd.Subtract(DateStart);
        //                        // Lấy số ngày chênh lệch
        //                        int DateDiff = (int)DateSpan.TotalDays;

        //                        bCTBL.DateCoefficient = (DateTime)item.DisposeTime;

        //                        foreach (var itemLv in rentingPrices) //Lấy giá chuẩn
        //                        {
        //                            if (itemLv.LevelId == item.Level && itemLv.TypeBlockId == blocks.TypeBlockId)
        //                            {
        //                                bCTBL.StandardPrice = (decimal)itemLv.Price;
        //                            }
        //                        }

        //                        foreach (var vat in vats)
        //                        {
        //                            if (bCTBL.DateStart >= vat.DoApply)
        //                            {
        //                                bCTBL.VAT = vat.Value;
        //                                break;
        //                            }
        //                            else if (vat.DoApply <= bCTBL.DateEnd)
        //                            {
        //                                var changevalue = new changeValue();

        //                                changevalue.Type = 1;
        //                                changevalue.valueChange = vat.Value;
        //                                changevalue.DateChange = vat.DoApply;

        //                                resChange.Add(changevalue);
        //                                continue;
        //                            }
        //                        }

        //                        foreach (var salaryCoefficient in salaryCoefficients) // lấy giá trị của lương cơ bản theo ngày áp dụng
        //                        {
        //                            if (bCTBL.DateStart >= salaryCoefficient.DoApply)
        //                            {
        //                                bCTBL.Ktlcb = (decimal)salaryCoefficient.Value;
        //                                break;
        //                            }
        //                            else if (salaryCoefficient.DoApply <= bCTBL.DateEnd)
        //                            {
        //                                var changevalue = new changeValue();

        //                                changevalue.Type = 2;
        //                                changevalue.valueChange = salaryCoefficient.Value;
        //                                changevalue.DateChange = salaryCoefficient.DoApply;

        //                                resChange.Add(changevalue);
        //                                continue;
        //                            }
        //                        }

        //                        foreach (var coefficient in coefficients) // lấy giá trị của hệ số thời điểm bố trí theo ngày áp dụng
        //                        {
        //                            if (bCTBL.DateStart >= coefficient.DoApply)
        //                            {
        //                                bCTBL.Ktdbt = (decimal)coefficient.Value;
        //                                break;
        //                            }
        //                            else if (coefficient.DoApply <= bCTBL.DateEnd)
        //                            {
        //                                var changevalue = new changeValue();

        //                                changevalue.Type = 3;
        //                                changevalue.valueChange = coefficient.Value;
        //                                changevalue.DateChange = coefficient.DoApply;

        //                                resChange.Add(changevalue);
        //                                continue;
        //                            }
        //                        }

        //                        bCTBL.chilDfs = new List<chilDf>();

        //                        if (map_rentFlieBCTDatas != null)
        //                        {
        //                            foreach (var itemDf in map_rentFlieBCTDatas.groupData)
        //                            {
        //                                if (itemDf.childDefaultCoefficients.Count > 0)
        //                                {
        //                                    foreach (var itemChild in itemDf.childDefaultCoefficients)
        //                                    {
        //                                        var itemChilDfs = new chilDf();

        //                                        if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
        //                                        {
        //                                            string content = _context.DefaultCoefficients.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Content).FirstOrDefault();
        //                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
        //                                            if (content == bCTBL.AreaName)
        //                                            {
        //                                                if (bCTBL.DateStart >= itemChild.DoApply)
        //                                                {
        //                                                    itemChilDfs.Value = itemChild.Value;
        //                                                    bCTBL.chilDfs.Add(itemChilDfs);
        //                                                    break;
        //                                                }
        //                                                else if (bCTBL.DateEnd <= itemChild.DoApply)
        //                                                {
        //                                                    var changeK = new changeK();

        //                                                    changeK.valueChange = itemChild.Value;
        //                                                    changeK.DateChange = itemChild.DoApply;
        //                                                    resChangeK.Add(changeK);
        //                                                }
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            if (bCTBL.DateStart >= itemChild.DoApply)
        //                                            {
        //                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
        //                                                itemChilDfs.Value = itemChild.Value;
        //                                                bCTBL.chilDfs.Add(itemChilDfs);
        //                                                break;
        //                                            }
        //                                            else if (bCTBL.DateEnd <= itemChild.DoApply)
        //                                            {
        //                                                var changeK = new changeK();

        //                                                changeK.CoefficientId = itemChild.CoefficientId;
        //                                                changeK.valueChange = itemChild.Value;
        //                                                changeK.DateChange = itemChild.DoApply;
        //                                                resChangeK.Add(changeK);
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                            foreach (var itemDf in map_rentFlieBCTDatas.groupData)
        //                            {
        //                                if (itemDf.childDefaultCoefficients.Count > 0)
        //                                {
        //                                    foreach (var itemChild in itemDf.childDefaultCoefficients)
        //                                    {
        //                                        if (itemChild.CoefficientId == AppEnums.TypeTable.HSGG)
        //                                        {
        //                                            if (bCTBL.DateStart >= itemChild.DoApply)
        //                                            {
        //                                                bCTBL.PolicyReduction = (decimal)itemChild.Value;
        //                                                break;
        //                                            }
        //                                            else if (itemChild.DoApply <= bCTBL.DateEnd)
        //                                            {
        //                                                var changevalue = new changeValue();
        //                                                changevalue.Type = 4;
        //                                                changevalue.valueChange = itemChild.Value;
        //                                                changevalue.DateChange = itemChild.DoApply;

        //                                                resChange.Add(changevalue);
        //                                                continue;
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }

        //                        if (bCTBL.StandardPrice == null) bCTBL.StandardPrice = 0;

        //                        if (bCTBL.chilDfs != null)
        //                        {
        //                            bCTBL.TotalK = (float)bCTBL.chilDfs.Sum(p => p.Value) + 1;
        //                        }
        //                        else
        //                        {
        //                            bCTBL.TotalK = 1;
        //                        }
        //                        if (bCTBL.Ktlcb == null) bCTBL.Ktlcb = 1;
        //                        if (bCTBL.Ktdbt == null) bCTBL.Ktdbt = 1;

        //                        bCTBL.PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK * (decimal)(1 + (bCTBL.VAT / 100))));

        //                        bCTBL.PriceRent = Math.Round((decimal)(bCTBL.PriceRent1m2 * (decimal)bCTBL.PrivateArea) - bCTBL.PolicyReduction);

        //                        bCTBL.Unit = "VNĐ";

        //                        if (resChangeK.Count > 0)
        //                        {
        //                            foreach (var itemK in bCTBL.chilDfs)
        //                            {
        //                                foreach (var itemChangeK in resChangeK.OrderBy(o => o.DateChange).ToList())
        //                                {
        //                                    if (itemK.CoefficientId == itemChangeK.CoefficientId)
        //                                    {
        //                                        decimal totalK = 0;
        //                                        var changevalue = new changeValue();
        //                                        totalK = (decimal)(bCTBL.TotalK - itemK.Value);
        //                                        changevalue.valueChange = (double)(totalK + (decimal)itemChangeK.valueChange);
        //                                        changevalue.Type = 5;
        //                                        changevalue.DateChange = itemChangeK.DateChange;
        //                                        resChange.Add(changevalue);
        //                                        bCTBL.TotalK = (float?)totalK;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        if (resChange.Count > 0)
        //                        {
        //                            foreach (var itemChange in resChange.OrderBy(p => p.DateChange).ToList())
        //                            {
        //                                decimal? PriceRent1m2 = 0;
        //                                decimal? PriceRent = 0;

        //                                if (itemChange.Type == 1)
        //                                {
        //                                    bCTBL.VAT = itemChange.valueChange;
        //                                    PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK * (decimal)(1 + (bCTBL.VAT / 100))));

        //                                    PriceRent = Math.Round((decimal)(bCTBL.PriceRent1m2 * (decimal)bCTBL.PrivateArea) - bCTBL.PolicyReduction);

        //                                    DateTime endChange = itemChange.DateChange;
        //                                    // Lấy TimeSpan giữa hai ngày
        //                                    TimeSpan spanChange = endChange.Subtract(DateStart);
        //                                    // Lấy số ngày chênh lệch
        //                                    int dateDiffChange = (int)spanChange.TotalDays; //Ngày giá cũ

        //                                    int NewDateChange = DateDiff - dateDiffChange; //Ngày giá mới

        //                                    decimal NewPrice = Math.Round(((decimal)(bCTBL.PriceRent / DateDiff) * dateDiffChange) + ((decimal)(PriceRent / DateDiff) * NewDateChange));
        //                                }
        //                                else if (itemChange.Type == 2)
        //                                {
        //                                    bCTBL.Ktlcb = (decimal)itemChange.valueChange;
        //                                    PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK * (decimal)(1 + (bCTBL.VAT / 100))));

        //                                    PriceRent = Math.Round((decimal)(bCTBL.PriceRent1m2 * (decimal)bCTBL.PrivateArea) - bCTBL.PolicyReduction);

        //                                    DateTime endChange = itemChange.DateChange;
        //                                    // Lấy TimeSpan giữa hai ngày
        //                                    TimeSpan spanChange = endChange.Subtract(DateStart);
        //                                    // Lấy số ngày chênh lệch
        //                                    int dateDiffChange = (int)spanChange.TotalDays; //Ngày giá cũ

        //                                    int NewDateChange = DateDiff - dateDiffChange; //Ngày giá mới

        //                                    decimal NewPrice = Math.Round(((decimal)(bCTBL.PriceRent / DateDiff) * dateDiffChange) + ((decimal)(PriceRent / DateDiff) * NewDateChange));
        //                                }
        //                                else if (itemChange.Type == 3)
        //                                {
        //                                    bCTBL.Ktdbt = (decimal)itemChange.valueChange;

        //                                    PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK * (decimal)(1 + (bCTBL.VAT / 100))));

        //                                    PriceRent = Math.Round((decimal)(bCTBL.PriceRent1m2 * (decimal)bCTBL.PrivateArea) - bCTBL.PolicyReduction);

        //                                    DateTime endChange = itemChange.DateChange;
        //                                    // Lấy TimeSpan giữa hai ngày
        //                                    TimeSpan spanChange = endChange.Subtract(DateStart);
        //                                    // Lấy số ngày chênh lệch
        //                                    int dateDiffChange = (int)spanChange.TotalDays; //Ngày giá cũ

        //                                    int NewDateChange = DateDiff - dateDiffChange; //Ngày giá mới

        //                                    decimal NewPrice = Math.Round(((decimal)(bCTBL.PriceRent / DateDiff) * dateDiffChange) + ((decimal)(PriceRent / DateDiff) * NewDateChange));
        //                                }
        //                                else if (itemChange.Type == 4)
        //                                {
        //                                    bCTBL.PolicyReduction = (decimal)itemChange.valueChange;

        //                                    PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK * (decimal)(1 + (bCTBL.VAT / 100))));

        //                                    PriceRent = Math.Round((decimal)(bCTBL.PriceRent1m2 * (decimal)bCTBL.PrivateArea) - bCTBL.PolicyReduction);

        //                                    DateTime endChange = itemChange.DateChange;
        //                                    // Lấy TimeSpan giữa hai ngày
        //                                    TimeSpan spanChange = endChange.Subtract(DateStart);
        //                                    // Lấy số ngày chênh lệch
        //                                    int dateDiffChange = (int)spanChange.TotalDays; //Ngày giá cũ

        //                                    int NewDateChange = DateDiff - dateDiffChange; //Ngày giá mới

        //                                    decimal NewPrice = Math.Round(((decimal)(bCTBL.PriceRent / DateDiff) * dateDiffChange) + ((decimal)(PriceRent / DateDiff) * NewDateChange));
        //                                }
        //                                else if (itemChange.Type == 5)
        //                                {
        //                                    bCTBL.TotalK = (float?)itemChange.valueChange;

        //                                    PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK * (decimal)(1 + (bCTBL.VAT / 100))));

        //                                    PriceRent = Math.Round((decimal)(bCTBL.PriceRent1m2 * (decimal)bCTBL.PrivateArea) - bCTBL.PolicyReduction);

        //                                    DateTime endChange = itemChange.DateChange;
        //                                    // Lấy TimeSpan giữa hai ngày
        //                                    TimeSpan spanChange = endChange.Subtract(DateStart);
        //                                    // Lấy số ngày chênh lệch
        //                                    int dateDiffChange = (int)spanChange.TotalDays; //Ngày giá cũ

        //                                    int NewDateChange = DateDiff - dateDiffChange; //Ngày giá mới

        //                                    decimal NewPrice = Math.Round(((decimal)(bCTBL.PriceRent / DateDiff) * dateDiffChange) + ((decimal)(PriceRent / DateDiff) * NewDateChange));
        //                                }
        //                                bCTBL.PriceRent1m2 = PriceRent1m2;
        //                                bCTBL.PriceRent = PriceRent;
        //                            }
        //                        }
        //                        resBct.Add(bCTBL);
        //                    }
        //                }
        //            }
        //        }

        //        var groupData = resBct.GroupBy(f => f.RentFileId != null ? f.RentFileId : new object(), key => key).Select(f => new groupdataCN
        //        {
        //            RentFileId = (int)f.Key,
        //            groupByDates = f.ToList().GroupBy(f => f.DateStart).Select(f => new groupByDate
        //            {
        //                DateStart = f.Key,
        //                Total = (decimal)f.ToList().Sum(p => p.PriceRent),
        //                bCTs = f.ToList()
        //            }).ToList()
        //        });

        //        def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
        //        def.data = groupData;
        //        return Ok(def);
        //    }

        //    catch (Exception ex)
        //    {
        //        return Ok(def);
        //    }
        //}

        [HttpGet("GetExcelTableCNQD22/{RentFileId}")]
        public async Task<IActionResult> GetExcelTableCNQD22(Guid RentFileId)
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
                List<Vat> vats = _context.Vats.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList();

                List<Salary> salaryCoefficients = _context.Salaries.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList();

                List<Coefficient> coefficients = _context.Coefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList();

                List<Conversion> conversions = _context.Conversions.Where(l => l.TypeQD == AppEnums.TypeQD.QD_22).ToList();

                List<BCT> resBct = new List<BCT>();

                //Tìm hồ sơ cho thuê
                RentFile rentFile = _context.RentFiles.Where(l => l.Id == RentFileId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                RentFlieData map_rentFlieData = _mapper.Map<RentFlieData>(rentFile);

                //Tìm bảng chiết tính mới nhất theo ID hồ sơ
                RentFileBCT rentFileBCT = _context.RentFileBCTs.Where(l => l.RentFileId == rentFile.Id && l.TypeBCT == 1 && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                RentFileBCTData map_rentFlieBCTDatas = _mapper.Map<RentFileBCTData>(rentFileBCT);

                if (map_rentFlieBCTDatas != null)
                {
                    List<DefaultCoefficient> dataDf = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderBy(p => p.CoefficientName).ToList();
                    var groupedData = dataDf.GroupBy(x => x.CoefficientName).Select(e => new GroupData
                    {
                        CoefficientId = e.Key,
                        defaultCoefficients = e.ToList(),
                        childDefaultCoefficients = _context.ChildDefaults.Where(l => l.RentFileBctId == map_rentFlieBCTDatas.Id && l.CoefficientId == e.Key && l.Type == 1 && l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList()
                    }).ToList();
                    map_rentFlieBCTDatas.groupData = groupedData;
                }

                List<DefaultCoefficient> defaultCoefficients = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                Block blocks = _context.Blocks.Where(l => l.Id == rentFile.RentBlockId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                List<RentingPrice> rentingPrices = _context.RentingPricies.Where(l => l.TypeQD == AppEnums.TypeQD.QD_22 && l.TypeBlockId == blocks.TypeBlockId && l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.EffectiveDate).ToList();

                BlockData map_blocks = _mapper.Map<BlockData>(blocks);

                List<BlockDetail> blockDetails = _context.BlockDetails.Where(l => l.BlockId == rentFile.RentBlockId && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<BlockDetailData> map_BlockDetails = _mapper.Map<List<BlockDetailData>>(blockDetails);

                List<Floor> floorData = _context.Floors.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<Area> AreaData = _context.Areas.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<DiscountCoefficient> disData = _context.DiscountCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<DefaultCoefficient> dfData = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();

                foreach (BlockDetailData map_BlockDetail in map_BlockDetails)
                {
                    Floor floor = floorData.Where(f => f.Id == map_BlockDetail.FloorId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    map_BlockDetail.FloorName = floor != null ? floor.Name : "";
                    map_BlockDetail.FloorCode = floor != null ? floor.Code : 0;

                    Area area = AreaData.Where(f => f.Id == map_BlockDetail.AreaId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    map_BlockDetail.AreaName = area != null ? area.Name : "";
                    map_BlockDetail.IsMezzanine = area != null ? area.IsMezzanine : null;
                }
                map_blocks.blockDetails = map_BlockDetails.OrderBy(x => x.FloorCode).ThenBy(x => x.IsMezzanine).ToList();


                int i = 0;
                int check = 0;

                for (int z = 0; z < rentFile.Month; z++)
                {
                    decimal sum = 0;

                    decimal VAT = 0;
                    decimal Ktlcb = 0;
                    decimal Ktdbt = 0;
                    decimal K = 0;

                    if (rentFile.RentApartmentId > 0)
                    {
                        Apartment apartment = _context.Apartments.Where(l => l.Id == rentFile.RentApartmentId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                        ApartmentData map_apartments = _mapper.Map<ApartmentData>(apartment);
                        List<ApartmentDetail> apartmentDetails = _context.ApartmentDetails.Where(a => a.TargetId == rentFile.RentApartmentId && a.Type == TypeApartmentDetail.APARTMENT && a.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                        List<ApartmentDetailData> apartmentDetailData = _mapper.Map<List<ApartmentDetailData>>(apartmentDetails);
                        foreach (ApartmentDetailData map_apartmentDetail in apartmentDetailData)
                        {
                            Area area = AreaData.Where(f => f.Id == map_apartmentDetail.AreaId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            map_apartmentDetail.AreaName = area != null ? area.Name : "";
                            map_apartmentDetail.IsMezzanine = area != null ? area.IsMezzanine : null;

                            Floor floor = floorData.Where(f => f.Id == map_apartmentDetail.FloorId && f.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                            map_apartmentDetail.FloorName = floor != null ? floor.Name : "";
                            map_apartmentDetail.FloorCode = floor != null ? floor.Code : 0;
                        }
                        map_apartments.apartmentDetailData = apartmentDetailData.ToList();
                        foreach (var item in map_apartments.apartmentDetailData)
                        {
                            List<changeValue> resChange = new List<changeValue>();
                            List<changeK> resChangeK = new List<changeK>();
                            List<int> listId = new List<int>();
                            var bCTBL = new BCT();

                            bCTBL.RentFileId = rentFile.Id;

                            bCTBL.check = false;

                            bCTBL.GroupId = z;

                            bCTBL.AreaName = item.AreaName;

                            bCTBL.Level = (int)item.Level;

                            bCTBL.PrivateArea = (float)item.PrivateArea;

                            bCTBL.RentFileId = RentFileId;

                            if (z == 0)
                            {
                                bCTBL.DateStart = rentFile.DateHD;
                                bCTBL.DateEnd = bCTBL.DateStart.Value.AddMonths(1).AddDays(-1);

                            }
                            else
                            {
                                bCTBL.DateStart = rentFile.DateHD.AddMonths(z);
                                bCTBL.DateEnd = bCTBL.DateStart.Value.AddMonths(1).AddDays(-1);
                            }

                            bCTBL.DiscountCoff = (decimal)disData.Where(l => l.Id == rentFileBCT.DiscountCoefficientId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();


                            DateTime DateStart = bCTBL.DateStart.Value;
                            DateTime DateEnd = bCTBL.DateEnd.Value;
                            // Lấy TimeSpan giữa hai ngày
                            TimeSpan DateSpan = DateEnd.Subtract(DateStart);
                            // Lấy số ngày chênh lệch
                            int DateDiff = (int)DateSpan.TotalDays;

                            bCTBL.DateCoefficient = (DateTime)item.DisposeTime;

                            foreach (var itemLv in rentingPrices) //Lấy giá chuẩn
                            {
                                if (itemLv.LevelId == item.Level && itemLv.TypeBlockId == blocks.TypeBlockId)
                                {
                                    bCTBL.StandardPrice = (decimal)itemLv.Price;
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
                                if (bCTBL.DateCoefficient >= coefficient.DoApply)
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
                                            var content = dfData.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                                            var itemChilDfs = new chilDf();

                                            Boolean flagCheck = false;
                                            foreach (var oldid in listId)
                                            {
                                                if (oldid == itemChild.defaultCoefficientsId)
                                                {
                                                    flagCheck = true;
                                                    break;
                                                }
                                            }
                                            if (flagCheck == true)
                                            {
                                                continue;
                                            }

                                            if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                            {
                                                if (content.Content == bCTBL.AreaName)
                                                {
                                                    if (bCTBL.DateStart >= itemChild.DoApply)
                                                    {
                                                        int oldId = itemChild.defaultCoefficientsId;
                                                        listId.Add(oldId);

                                                        itemChilDfs.CoefficientId = content.CoefficientName;
                                                        itemChilDfs.Value = itemChild.Value;
                                                        itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                        bCTBL.chilDfs.Add(itemChilDfs);
                                                        break;
                                                    }
                                                    else if (bCTBL.DateEnd <= itemChild.DoApply)
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
                                                    itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                    itemChilDfs.Value = itemChild.Value;
                                                    itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                    bCTBL.chilDfs.Add(itemChilDfs);
                                                    break;
                                                }
                                                else if (bCTBL.DateEnd <= itemChild.DoApply)
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

                            bCTBL.TotalK = (float)bCTBL.chilDfs.Sum(p => p.Value) + 1;

                            if (bCTBL.Ktlcb == null) bCTBL.Ktlcb = 1;
                            if (bCTBL.Ktdbt == null) bCTBL.Ktdbt = 1;

                            bCTBL.PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK * (decimal)(1 + (bCTBL.VAT / 100))));
                            bCTBL.PriceRent = Math.Round((decimal)(bCTBL.PriceRent1m2 * (decimal)bCTBL.PrivateArea));
                            bCTBL.PolicyReduction = Math.Round((decimal)(bCTBL.DiscountCoff * bCTBL.PriceRent));
                            bCTBL.PriceAfterDiscount = Math.Round((decimal)(bCTBL.PriceRent - bCTBL.PolicyReduction));

                            bCTBL.Unit = "VNĐ";

                            if (groupdataK.Count() > 0)
                            {
                                foreach (var itemk in groupdataK)
                                {
                                    decimal totalK = 0;
                                    decimal CurrentK = (decimal)bCTBL.TotalK;
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
                                    bCTBL.TotalK = (float?)CurrentK;
                                    resChange.Add(changevalue);
                                }
                            }

                            if (resChange.Count > 0)
                            {
                                foreach (var itemChange in resChange.OrderBy(p => p.DateChange).ToList())
                                {
                                    decimal? PriceRent1m2 = 0;
                                    decimal? PriceRent = 0;
                                    decimal? PolicyReduction = 0;
                                    decimal? PriceAfterDiscount = 0;
                                    decimal? NewPrice = 0;

                                    if (itemChange.Type == 1)
                                    {
                                        bCTBL.VAT = itemChange.valueChange;
                                        PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK * (decimal)(1 + (bCTBL.VAT / 100))));

                                        PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)bCTBL.PrivateArea));

                                        PolicyReduction = Math.Round((decimal)(bCTBL.DiscountCoff * PriceRent));

                                        PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                        DateTime endChange = itemChange.DateChange;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan spanChange = endChange.Subtract(DateStart);
                                        // Lấy số ngày chênh lệch
                                        int dateDiffChange = (int)spanChange.TotalDays; //Ngày giá cũ

                                        int NewDateChange = DateDiff - dateDiffChange; //Ngày giá mới

                                        NewPrice = Math.Round(((decimal)(bCTBL.PriceAfterDiscount / DateDiff) * dateDiffChange) + ((decimal)(PriceAfterDiscount / DateDiff) * NewDateChange));


                                        bCTBL.PriceRent1m2 = PriceRent1m2;
                                        bCTBL.PriceRent = PriceRent;
                                        bCTBL.PolicyReduction = (decimal)PolicyReduction;
                                        bCTBL.PriceAfterDiscount = (decimal)NewPrice;
                                    }
                                    else if (itemChange.Type == 2)
                                    {
                                        bCTBL.Ktlcb = (decimal)itemChange.valueChange;
                                        PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK * (decimal)(1 + (bCTBL.VAT / 100))));

                                        PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)bCTBL.PrivateArea));

                                        PolicyReduction = Math.Round((decimal)(bCTBL.DiscountCoff * PriceRent));

                                        PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                        DateTime endChange = itemChange.DateChange;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan spanChange = endChange.Subtract(DateStart);
                                        // Lấy số ngày chênh lệch
                                        int dateDiffChange = (int)spanChange.TotalDays; //Ngày giá cũ

                                        int NewDateChange = DateDiff - dateDiffChange; //Ngày giá mới

                                        NewPrice = Math.Round(((decimal)(bCTBL.PriceAfterDiscount / DateDiff) * dateDiffChange) + ((decimal)(PriceAfterDiscount / DateDiff) * NewDateChange));


                                        bCTBL.PriceRent1m2 = PriceRent1m2;
                                        bCTBL.PriceRent = PriceRent;
                                        bCTBL.PolicyReduction = (decimal)PolicyReduction;
                                        bCTBL.PriceAfterDiscount = (decimal)NewPrice;
                                    }
                                    else if (itemChange.Type == 3)
                                    {
                                        bCTBL.Ktdbt = (decimal)itemChange.valueChange;

                                        PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK * (decimal)(1 + (bCTBL.VAT / 100))));

                                        PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)bCTBL.PrivateArea));

                                        PolicyReduction = Math.Round((decimal)(bCTBL.DiscountCoff * PriceRent));

                                        PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                        DateTime endChange = itemChange.DateChange;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan spanChange = endChange.Subtract(DateStart);
                                        // Lấy số ngày chênh lệch
                                        int dateDiffChange = (int)spanChange.TotalDays; //Ngày giá cũ

                                        int NewDateChange = DateDiff - dateDiffChange; //Ngày giá mới

                                        NewPrice = Math.Round(((decimal)(bCTBL.PriceAfterDiscount / DateDiff) * dateDiffChange) + ((decimal)(PriceAfterDiscount / DateDiff) * NewDateChange));


                                        bCTBL.PriceRent1m2 = PriceRent1m2;
                                        bCTBL.PriceRent = PriceRent;
                                        bCTBL.PolicyReduction = (decimal)PolicyReduction;
                                        bCTBL.PriceAfterDiscount = (decimal)NewPrice;
                                    }
                                    else if (itemChange.Type == 4)
                                    {
                                        bCTBL.PolicyReduction = (decimal)itemChange.valueChange;

                                        PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK * (decimal)(1 + (bCTBL.VAT / 100))));

                                        PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)bCTBL.PrivateArea));

                                        PolicyReduction = Math.Round((decimal)(bCTBL.DiscountCoff * PriceRent));

                                        PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                        DateTime endChange = itemChange.DateChange;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan spanChange = endChange.Subtract(DateStart);
                                        // Lấy số ngày chênh lệch
                                        int dateDiffChange = (int)spanChange.TotalDays; //Ngày giá cũ

                                        int NewDateChange = DateDiff - dateDiffChange; //Ngày giá mới

                                        NewPrice = Math.Round(((decimal)(bCTBL.PriceAfterDiscount / DateDiff) * dateDiffChange) + ((decimal)(PriceAfterDiscount / DateDiff) * NewDateChange));

                                        bCTBL.PriceRent1m2 = PriceRent1m2;
                                        bCTBL.PriceRent = PriceRent;
                                        bCTBL.PolicyReduction = (decimal)PolicyReduction;
                                        bCTBL.PriceAfterDiscount = (decimal)NewPrice;

                                    }
                                    else if (itemChange.Type == 5)
                                    {
                                        bCTBL.TotalK = (float?)itemChange.valueChange;

                                        PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK * (decimal)(1 + (bCTBL.VAT / 100))));

                                        PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)bCTBL.PrivateArea));

                                        PolicyReduction = Math.Round((decimal)(bCTBL.DiscountCoff * PriceRent));

                                        PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                        DateTime endChange = itemChange.DateChange;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan spanChange = endChange.Subtract(DateStart);
                                        // Lấy số ngày chênh lệch
                                        int dateDiffChange = (int)spanChange.TotalDays; //Ngày giá cũ

                                        int NewDateChange = DateDiff - dateDiffChange; //Ngày giá mới

                                        NewPrice = Math.Round(((decimal)(bCTBL.PriceAfterDiscount / DateDiff) * dateDiffChange) + ((decimal)(PriceAfterDiscount / DateDiff) * NewDateChange));


                                        bCTBL.PriceRent1m2 = PriceRent1m2;
                                        bCTBL.PriceRent = PriceRent;
                                        bCTBL.PolicyReduction = (decimal)PolicyReduction;
                                        bCTBL.PriceAfterDiscount = (decimal)NewPrice;
                                    }

                                }
                            }
                            resBct.Add(bCTBL);
                        }
                    }
                    else
                    {
                        foreach (var item in map_blocks.blockDetails.ToList())
                        {
                            List<changeValue> resChange = new List<changeValue>();
                            List<changeK> resChangeK = new List<changeK>();

                            var bCTBL = new BCT();
                            List<int> listId = new List<int>();
                            bCTBL.RentFileId = rentFile.Id;

                            bCTBL.check = false;

                            bCTBL.GroupId = z;

                            bCTBL.AreaName = item.AreaName;

                            bCTBL.Level = (int)item.Level;

                            bCTBL.PrivateArea = (float)item.PrivateArea;

                            bCTBL.DiscountCoff = (decimal)disData.Where(l => l.Id == rentFileBCT.DiscountCoefficientId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();
                            bCTBL.RentFileId = RentFileId;

                            if (z == 0)
                            {
                                bCTBL.DateStart = rentFile.DateHD;
                                bCTBL.DateEnd = bCTBL.DateStart.Value.AddMonths(1).AddDays(-1);

                            }
                            else
                            {
                                bCTBL.DateStart = rentFile.DateHD.AddMonths(z);
                                bCTBL.DateEnd = bCTBL.DateStart.Value.AddMonths(1).AddDays(-1);
                            }


                            DateTime DateStart = bCTBL.DateStart.Value;
                            DateTime DateEnd = bCTBL.DateEnd.Value;
                            // Lấy TimeSpan giữa hai ngày
                            TimeSpan DateSpan = DateEnd.Subtract(DateStart);
                            // Lấy số ngày chênh lệch
                            int DateDiff = (int)DateSpan.TotalDays;

                            bCTBL.DateCoefficient = (DateTime)item.DisposeTime;

                            foreach (var itemLv in rentingPrices) //Lấy giá chuẩn
                            {
                                if (itemLv.LevelId == item.Level && itemLv.TypeBlockId == blocks.TypeBlockId)
                                {
                                    bCTBL.StandardPrice = (decimal)itemLv.Price;
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
                                if (bCTBL.DateCoefficient >= coefficient.DoApply)
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
                                            var content = dfData.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                                            var itemChilDfs = new chilDf();

                                            Boolean flagCheck = false;
                                            foreach (var oldid in listId)
                                            {
                                                if (oldid == itemChild.defaultCoefficientsId)
                                                {
                                                    flagCheck = true;
                                                    break;
                                                }
                                            }
                                            if (flagCheck == true)
                                            {
                                                continue;
                                            }

                                            if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                            {
                                                if (content.Content == bCTBL.AreaName)
                                                {
                                                    if (bCTBL.DateStart >= itemChild.DoApply)
                                                    {
                                                        itemChilDfs.CoefficientId = content.CoefficientName;
                                                        itemChilDfs.Value = itemChild.Value;
                                                        itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                        bCTBL.chilDfs.Add(itemChilDfs);

                                                        int oldId = itemChild.defaultCoefficientsId;
                                                        listId.Add(oldId);
                                                        break;
                                                    }
                                                    else if (bCTBL.DateEnd <= itemChild.DoApply)
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
                                                    itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                    itemChilDfs.Value = itemChild.Value;
                                                    itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
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

                            bCTBL.TotalK = (float)bCTBL.chilDfs.Sum(p => p.Value) + 1;

                            if (bCTBL.Ktlcb == null) bCTBL.Ktlcb = 1;
                            if (bCTBL.Ktdbt == null) bCTBL.Ktdbt = 1;

                            bCTBL.PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK * (decimal)(1 + (bCTBL.VAT / 100))));

                            bCTBL.PriceRent = Math.Round((decimal)(bCTBL.PriceRent1m2 * (decimal)bCTBL.PrivateArea));
                            bCTBL.PolicyReduction = Math.Round((decimal)(bCTBL.DiscountCoff * bCTBL.PriceRent));
                            bCTBL.PriceAfterDiscount = Math.Round((decimal)(bCTBL.PriceRent - bCTBL.PolicyReduction));

                            bCTBL.Unit = "VNĐ";



                            if (groupdataK.Count() > 0)
                            {
                                foreach (var itemk in groupdataK)
                                {
                                    decimal totalK = 0;
                                    decimal CurrentK = (decimal)bCTBL.TotalK;
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
                                    bCTBL.TotalK = (float?)CurrentK;
                                    resChange.Add(changevalue);
                                }
                            }

                            if (resChange.Count > 0)
                            {
                                foreach (var itemChange in resChange.OrderBy(p => p.DateChange).ToList())
                                {
                                    decimal? PriceRent1m2 = 0;
                                    decimal? PriceRent = 0;
                                    decimal NewPrice = 0;
                                    decimal PolicyReduction = 0;
                                    decimal PriceAfterDiscount = 0;

                                    if (itemChange.Type == 1)
                                    {
                                        bCTBL.VAT = itemChange.valueChange;

                                        PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK * (decimal)(1 + (bCTBL.VAT / 100))));

                                        PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)bCTBL.PrivateArea));

                                        PolicyReduction = Math.Round((decimal)(bCTBL.DiscountCoff * PriceRent));

                                        PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                        DateTime endChange = itemChange.DateChange;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan spanChange = endChange.Subtract(DateStart);
                                        // Lấy số ngày chênh lệch
                                        int dateDiffChange = (int)spanChange.TotalDays; //Ngày giá cũ

                                        int NewDateChange = DateDiff - dateDiffChange; //Ngày giá mới

                                        NewPrice = Math.Round(((decimal)(bCTBL.PriceAfterDiscount / DateDiff) * dateDiffChange) + ((decimal)(PriceAfterDiscount / DateDiff) * NewDateChange));

                                        bCTBL.PriceRent1m2 = PriceRent1m2;
                                        bCTBL.PriceRent = PriceRent;
                                        bCTBL.PriceAfterDiscount = NewPrice;
                                        bCTBL.PolicyReduction = PolicyReduction;
                                    }
                                    else if (itemChange.Type == 2)
                                    {
                                        bCTBL.Ktlcb = (decimal)itemChange.valueChange;

                                        PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK * (decimal)(1 + (bCTBL.VAT / 100))));

                                        PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)bCTBL.PrivateArea));

                                        PolicyReduction = Math.Round((decimal)(bCTBL.DiscountCoff * PriceRent));

                                        PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                        DateTime endChange = itemChange.DateChange;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan spanChange = endChange.Subtract(DateStart);
                                        // Lấy số ngày chênh lệch
                                        int dateDiffChange = (int)spanChange.TotalDays; //Ngày giá cũ

                                        int NewDateChange = DateDiff - dateDiffChange; //Ngày giá mới

                                        NewPrice = Math.Round(((decimal)(bCTBL.PriceAfterDiscount / DateDiff) * dateDiffChange) + ((decimal)(PriceAfterDiscount / DateDiff) * NewDateChange));

                                        bCTBL.PriceRent1m2 = PriceRent1m2;
                                        bCTBL.PriceRent = PriceRent;
                                        bCTBL.PriceAfterDiscount = NewPrice;
                                        bCTBL.PolicyReduction = PolicyReduction;
                                    }
                                    else if (itemChange.Type == 3)
                                    {
                                        bCTBL.Ktdbt = (decimal)itemChange.valueChange;


                                        PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK * (decimal)(1 + (bCTBL.VAT / 100))));

                                        PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)bCTBL.PrivateArea));

                                        PolicyReduction = Math.Round((decimal)(bCTBL.DiscountCoff * PriceRent));

                                        PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                        DateTime endChange = itemChange.DateChange;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan spanChange = endChange.Subtract(DateStart);
                                        // Lấy số ngày chênh lệch
                                        int dateDiffChange = (int)spanChange.TotalDays; //Ngày giá cũ

                                        int NewDateChange = DateDiff - dateDiffChange; //Ngày giá mới

                                        NewPrice = Math.Round(((decimal)(bCTBL.PriceAfterDiscount / DateDiff) * dateDiffChange) + ((decimal)(PriceAfterDiscount / DateDiff) * NewDateChange));

                                        bCTBL.PriceRent1m2 = PriceRent1m2;
                                        bCTBL.PriceRent = PriceRent;
                                        bCTBL.PriceAfterDiscount = NewPrice;
                                        bCTBL.PolicyReduction = PolicyReduction;
                                    }
                                    else if (itemChange.Type == 4)
                                    {
                                        bCTBL.PolicyReduction = (decimal)itemChange.valueChange;

                                        PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK * (decimal)(1 + (bCTBL.VAT / 100))));

                                        PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)bCTBL.PrivateArea));

                                        PolicyReduction = Math.Round((decimal)(bCTBL.DiscountCoff * PriceRent));

                                        PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                        DateTime endChange = itemChange.DateChange;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan spanChange = endChange.Subtract(DateStart);
                                        // Lấy số ngày chênh lệch
                                        int dateDiffChange = (int)spanChange.TotalDays; //Ngày giá cũ

                                        int NewDateChange = DateDiff - dateDiffChange; //Ngày giá mới

                                        NewPrice = Math.Round(((decimal)(bCTBL.PriceAfterDiscount / DateDiff) * dateDiffChange) + ((decimal)(PriceAfterDiscount / DateDiff) * NewDateChange));

                                        bCTBL.PriceRent1m2 = PriceRent1m2;
                                        bCTBL.PriceRent = PriceRent;
                                        bCTBL.PriceAfterDiscount = NewPrice;
                                        bCTBL.PolicyReduction = PolicyReduction;
                                    }
                                    else if (itemChange.Type == 5)
                                    {
                                        bCTBL.TotalK = (float?)itemChange.valueChange;

                                        PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK * (decimal)(1 + (bCTBL.VAT / 100))));

                                        PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)bCTBL.PrivateArea));

                                        PolicyReduction = Math.Round((decimal)(bCTBL.DiscountCoff * PriceRent));

                                        PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                        DateTime endChange = itemChange.DateChange;
                                        // Lấy TimeSpan giữa hai ngày
                                        TimeSpan spanChange = endChange.Subtract(DateStart);
                                        // Lấy số ngày chênh lệch
                                        int dateDiffChange = (int)spanChange.TotalDays; //Ngày giá cũ

                                        int NewDateChange = DateDiff - dateDiffChange; //Ngày giá mới

                                        NewPrice = Math.Round(((decimal)(bCTBL.PriceAfterDiscount / DateDiff) * dateDiffChange) + ((decimal)(PriceAfterDiscount / DateDiff) * NewDateChange));

                                        bCTBL.PriceRent1m2 = PriceRent1m2;
                                        bCTBL.PriceRent = PriceRent;
                                        bCTBL.PriceAfterDiscount = NewPrice;
                                        bCTBL.PolicyReduction = PolicyReduction;
                                    }
                                }
                            }
                            resBct.Add(bCTBL);
                        }
                    }
                }


                var groupData = resBct.GroupBy(f => f.RentFileId != null ? f.RentFileId : new object(), key => key).Select(f => new groupdataCN
                {
                    RentFileId = (Guid)f.Key,
                    groupByDates = f.ToList().GroupBy(f => f.DateStart).Select(f => new groupByDate
                    {
                        DateStart = f.Key,
                        Total = (decimal)f.ToList().Sum(p => p.PriceAfterDiscount),
                        bCTs = f.ToList(),
                        VAT = f.ToList().First().VAT,
                        DateEnd = f.ToList().First().DateEnd,
                    }).ToList()
                });

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = groupData;
                return Ok(def);
            }

            catch (Exception ex)
            {
                return Ok(def);
            }
        }

        ////API dowloadExcel-QD22

        [HttpPost("ExportGetWorkSheet/{RentFilesId}")]
        public async Task<IActionResult> ExportGetWorkSheet(Guid RentFilesId, [FromBody] groupData input)
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
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            {
                return new FileContentResult(new byte[0], "application/octet-stream");
            }

            RentFile dataBct = _context.RentFiles.Where(x => x.Id == RentFilesId && x.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
            if (dataBct == null)
            {
                def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                return Ok(def);
            }
            RentFlieData mapper_data = _mapper.Map<RentFlieData>(dataBct);

            mapper_data.CustomerName = _context.Customers.Where(f => f.Id == mapper_data.CustomerId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.FullName).FirstOrDefault();

            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"templates/NOC-BCT.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);
            string nameExcel = "NOC-BCT.xls";

            MemoryStream ms = WriteDataToExcel(templatePath, 0, input, mapper_data);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", nameExcel);
        }
        private static MemoryStream WriteDataToExcel(string templatePath, int sheetnumber, groupData data, RentFlieData rentFile)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowHeader = 10;
            int rowHeader1 = 11;
            int rowStart = 12;

            if (sheet != null)
            {
                try
                {
                    IRow rowD3 = sheet.GetRow(2);
                    ICell cellD3 = rowD3.GetCell(0);
                    if (rentFile.fullAddressCH != null)
                    {
                        cellD3.SetCellValue("Căn nhà số: " + rentFile.fullAddressCH);
                    }
                    else
                    {
                        cellD3.SetCellValue("Căn nhà số: " + rentFile.fullAddressCN);
                    }

                    IRow rowD4 = sheet.GetRow(3);
                    ICell cellD4 = rowD4.GetCell(0);
                    cellD4.SetCellValue("Hộ sử dụng: " + rentFile.CustomerName);

                    IRow rowD5 = sheet.GetRow(5);
                    ICell cellD5 = rowD5.GetCell(3);
                    cellD5.SetCellValue(rentFile.CodeHS);

                    IRow rowD7 = sheet.GetRow(2);
                    ICell cellD7 = rowD7.GetCell(7);
                    if (rentFile.CodeCH != null)
                    {
                        cellD7.SetCellValue("Mã định danh: " + rentFile.CodeCH);
                    }
                    else
                    {
                        cellD7.SetCellValue("Mã định danh: " + rentFile.CodeCN);
                    }

                    int datacol = 0;
                    int childDf = 0;

                    if (data.bCTs != null)
                    {
                        foreach (var e in data.bCTs)
                        {
                            childDf = (e.chilDfs != null ? e.chilDfs.Count : 0);
                        }
                    }
                    else
                    {
                        foreach (var e in data.Bcts)
                        {
                            childDf = (e.chilDfs != null ? e.chilDfs.Count : 0);
                        }
                    }

                    datacol = 19 + childDf;

                    var style = workbook.CreateCellStyle();
                    style.BorderBottom = BorderStyle.Thin;
                    style.BorderLeft = BorderStyle.Thin;
                    style.BorderRight = BorderStyle.Thin;
                    style.BorderTop = BorderStyle.Thin;

                    ICellStyle cellStyle = workbook.CreateCellStyle();
                    IFont fontstyle = workbook.CreateFont();
                    fontstyle.FontName = "Times New Roman";
                    fontstyle.FontHeightInPoints = 13.5;
                    cellStyle.SetFont(fontstyle);
                    cellStyle.Alignment = HorizontalAlignment.Center;
                    cellStyle.VerticalAlignment = VerticalAlignment.Center;
                    cellStyle.WrapText = true;
                    cellStyle.BorderBottom = BorderStyle.Thin;
                    cellStyle.BorderLeft = BorderStyle.Thin;
                    cellStyle.BorderRight = BorderStyle.Thin;
                    cellStyle.BorderTop = BorderStyle.Thin;

                    IRow rowHeaders = sheet.CreateRow(rowHeader);
                    IRow rowHeaders1 = sheet.CreateRow(rowHeader1);

                    //for (int i = 0; i < datacol; i++)
                    //{
                    //    sheet.SetColumnWidth(i, (int)(15.5 * 256));
                    //}
                    sheet.SetColumnWidth(0, (int)(6.14 * 256));
                    sheet.SetColumnWidth(1, (int)(5.00 * 256));
                    sheet.SetColumnWidth(2, (int)(19.00 * 256));
                    sheet.SetColumnWidth(3, (int)(14.00 * 256));
                    sheet.SetColumnWidth(4, (int)(21.71 * 256));
                    sheet.SetColumnWidth(5, (int)(19.00 * 256));
                    sheet.SetColumnWidth(6, (int)(10.00 * 256));

                    ICell cellHeader1 = rowHeaders.CreateCell(0);
                    cellHeader1.SetCellValue("Loại nhà");
                    cellHeader1.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader1 = new CellRangeAddress(rowHeader, rowHeader1, 0, 3);
                    sheet.AddMergedRegion(mergedRegioncellHeader1);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader1, sheet);//Bottom border  
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader1, sheet);//Bottom border  
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader1, sheet);//Bottom border  
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader1, sheet);//Bottom border 


                    ICell cellHeader2 = rowHeaders.CreateCell(4);
                    cellHeader2.SetCellValue("Giá chuẩn theo tỷ lệ tăng lương cơ bản");
                    cellHeader2.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader2 = new CellRangeAddress(rowHeader, rowHeader1, 4, 4);
                    sheet.AddMergedRegion(mergedRegioncellHeader2);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader2, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader2, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader2, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader2, sheet);

                    ICell cellHeader3 = rowHeaders.CreateCell(5);
                    cellHeader3.SetCellValue("Hệ số thời điểm bố trí sử dụng ");
                    cellHeader3.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader3 = new CellRangeAddress(rowHeader, rowHeader1, 5, 5);
                    sheet.AddMergedRegion(mergedRegioncellHeader3);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader3, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader3, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader3, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader3, sheet);

                    ICell cellHeader4 = rowHeaders.CreateCell(6);
                    cellHeader4.SetCellValue("Diện tích");
                    cellHeader4.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader4 = new CellRangeAddress(rowHeader, rowHeader1, 6, 6);
                    sheet.AddMergedRegion(mergedRegioncellHeader4);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader4, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader4, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader4, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader4, sheet);

                    int excelCount = 7;
                    int count = 0;
                    if (data.bCTs != null)
                    {
                        for (int v = 0; v < data.bCTs[0].chilDfs.Count; v++)
                        {
                            count++;
                        }
                    }
                    else
                    {
                        for (int v = 0; v < data.Bcts[0].chilDfs.Count; v++)
                        {
                            count++;
                        }
                    }


                    ICell cellHeader5 = rowHeaders.CreateCell(7);
                    cellHeader5.SetCellValue("Hệ số");
                    cellHeader5.CellStyle = cellStyle;
                    if (excelCount < excelCount + count)
                    {
                        CellRangeAddress mergedRegioncellHeader5 = new CellRangeAddress(rowHeader, rowHeader, excelCount, excelCount + count);
                        sheet.AddMergedRegion(mergedRegioncellHeader5);
                        RegionUtil.SetBorderBottom(1, mergedRegioncellHeader5, sheet);
                        RegionUtil.SetBorderTop(1, mergedRegioncellHeader5, sheet);
                        RegionUtil.SetBorderLeft(1, mergedRegioncellHeader5, sheet);
                        RegionUtil.SetBorderRight(1, mergedRegioncellHeader5, sheet);
                    }

                    if (data.bCTs != null)
                    {
                        foreach (var e in data.bCTs)
                        {
                            foreach (var eItem in e.chilDfs)
                            {

                                ICell cellHeaders = rowHeaders1.CreateCell(excelCount);
                                cellHeaders.SetCellValue(eItem.Code);
                                cellHeaders.CellStyle = cellStyle;
                                sheet.SetColumnWidth(excelCount, (int)(8.00 * 256));
                                excelCount++;
                            }
                            break;
                        }
                    }
                    else
                    {
                        foreach (var e in data.Bcts)
                        {
                            if (e.chilDfs != null)
                            {
                                foreach (var eItem in e.chilDfs)
                                {
                                    ICell cellHeaders = rowHeaders1.CreateCell(excelCount);
                                    cellHeaders.SetCellValue(eItem.Code);
                                    cellHeaders.CellStyle = cellStyle;
                                    sheet.SetColumnWidth(excelCount, (int)(8.00 * 256));
                                    excelCount++;
                                }
                                break;
                            }
                        }
                    }


                    ICell cellHeader = rowHeaders1.CreateCell(excelCount);
                    cellHeader.SetCellValue("Tổng K");
                    cellHeader.CellStyle = cellStyle;
                    sheet.SetColumnWidth(excelCount, (int)(9.00 * 256));
                    excelCount++;

                    int excelCountEnd = excelCount;

                    ICell cellHeader10 = rowHeaders.CreateCell(excelCountEnd);
                    cellHeader10.SetCellValue("Giá thuê (1m²/tháng)");
                    cellHeader10.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader10 = new CellRangeAddress(rowHeader, rowHeader1, excelCountEnd, excelCountEnd);
                    sheet.AddMergedRegion(mergedRegioncellHeader10);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader10, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader10, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader10, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader10, sheet);
                    sheet.SetColumnWidth(excelCountEnd, (int)(13.00 * 256));
                    excelCountEnd++;


                    ICell cellHeader11 = rowHeaders.CreateCell(excelCountEnd);
                    cellHeader11.SetCellValue("Giá thuê (đã tính " + data.VAT + "% VAT)");
                    cellHeader11.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader11 = new CellRangeAddress(rowHeader, rowHeader1, excelCountEnd, excelCountEnd);
                    sheet.AddMergedRegion(mergedRegioncellHeader11);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader11, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader11, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader11, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader11, sheet);
                    sheet.SetColumnWidth(excelCountEnd, (int)(18.00 * 256));
                    excelCountEnd++;

                    ICell cellHeader19 = rowHeaders.CreateCell(excelCountEnd);
                    cellHeader19.SetCellValue("Ghi chú");
                    cellHeader19.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader19 = new CellRangeAddress(rowHeader, rowHeader1, excelCountEnd, excelCountEnd);
                    sheet.AddMergedRegion(mergedRegioncellHeader19);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader19, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader19, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader19, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader19, sheet);
                    excelCountEnd++;

                    ICellStyle cellStyleDate = workbook.CreateCellStyle();
                    IFont fontstyledate = workbook.CreateFont();
                    fontstyledate.FontName = "Times New Roman";
                    fontstyledate.FontHeightInPoints = 13.5;
                    cellStyleDate.SetFont(fontstyledate);
                    cellStyleDate.DataFormat = workbook.CreateDataFormat().GetFormat("dd/mm/yyyy");
                    cellStyleDate.BorderBottom = BorderStyle.Thin;
                    cellStyleDate.BorderLeft = BorderStyle.Thin;
                    cellStyleDate.BorderRight = BorderStyle.Thin;
                    cellStyleDate.BorderTop = BorderStyle.Thin;

                    ICellStyle cellStyleMoney = workbook.CreateCellStyle();
                    IFont fontstyleformat = workbook.CreateFont();
                    fontstyleformat.FontName = "Times New Roman";
                    fontstyleformat.FontHeightInPoints = 13.5;
                    cellStyleMoney.SetFont(fontstyleformat);
                    var dataFormat = workbook.CreateDataFormat();
                    cellStyleMoney.DataFormat = dataFormat.GetFormat("#,##0");
                    cellStyleMoney.BorderBottom = BorderStyle.Thin;
                    cellStyleMoney.BorderLeft = BorderStyle.Thin;
                    cellStyleMoney.BorderRight = BorderStyle.Thin;
                    cellStyleMoney.BorderTop = BorderStyle.Thin;

                    ICellStyle decimalStyle = workbook.CreateCellStyle();
                    IFont fontstyledecimal = workbook.CreateFont();
                    fontstyledecimal.FontName = "Times New Roman";
                    fontstyledecimal.FontHeightInPoints = 13.5;
                    decimalStyle.SetFont(fontstyledecimal);
                    decimalStyle.DataFormat = workbook.CreateDataFormat().GetFormat("#,##0.00");
                    decimalStyle.BorderBottom = BorderStyle.Thin;
                    decimalStyle.BorderLeft = BorderStyle.Thin;
                    decimalStyle.BorderRight = BorderStyle.Thin;
                    decimalStyle.BorderTop = BorderStyle.Thin;

                    int ks = 7;
                    int k = -10;
                    int firstRow = rowStart;
                    if (data.bCTs != null)
                    {
                        foreach (var item in data.bCTs)
                        {
                            if (item.check != true)
                            {
                                IRow row = sheet.CreateRow(rowStart);

                                for (int i = 0; i < datacol; i++)
                                {
                                    ICell cell = row.CreateCell(i);

                                    if (i == 0)
                                    {
                                        cell.SetCellValue("Cấp");
                                        cell.CellStyle = cellStyle;
                                    }
                                    else if (i == 1)
                                    {
                                        cell.SetCellValue((double)item.Level);
                                        cell.CellStyle = cellStyle;
                                    }
                                    else if (i == 2)
                                    {
                                        cell.SetCellValue("Tầng/Lửng tầng");
                                        cell.CellStyle = cellStyle;
                                    }
                                    else if (i == 3)
                                    {
                                        cell.SetCellValue(item.AreaName);
                                        cell.CellStyle = cellStyle;
                                    }
                                    else if (i == 4)
                                    {
                                        cell.SetCellValue((double)((decimal)item.PrivateArea * item.Ktlcb));
                                        cell.CellStyle = cellStyle;
                                    }
                                    else if (i == 5)
                                    {
                                        if (item.Ktdbt != null)
                                        {
                                            cell.SetCellValue((double)item.Ktdbt);
                                            cell.CellStyle = cellStyle;
                                        }
                                        else
                                        {
                                            cell.SetCellValue("");
                                            cell.CellStyle = cellStyle;
                                        }

                                    }
                                    else if (i == 6)
                                    {
                                        if (item.PrivateArea != null)
                                        {
                                            cell.SetCellValue((double)item.PrivateArea);
                                            cell.CellStyle = cellStyle;
                                        }
                                        else
                                        {
                                            cell.SetCellValue("");
                                            cell.CellStyle = cellStyle;
                                        }

                                    }
                                    else if (i == ks)
                                    {
                                        if (item.chilDfs != null)
                                        {
                                            foreach (var x in item.chilDfs)
                                            {
                                                ICell cellArea = row.CreateCell(i);
                                                cellArea.SetCellValue((double)x.Value);
                                                cellArea.CellStyle = cellStyle;
                                                i++;
                                            }
                                            k = i;
                                            i = i - 1;
                                        }
                                    }
                                    else if (i == k)
                                    {
                                        if (item.TotalK != null)
                                        {
                                            cell.SetCellValue((double)item.TotalK);
                                            cell.CellStyle = cellStyle;
                                        }
                                        else
                                        {
                                            cell.SetCellValue("");
                                            cell.CellStyle = cellStyle;
                                        }

                                    }
                                    else if (i == k + 1)
                                    {
                                        if (item.PriceRent1m2 != null)
                                        {
                                            cell.SetCellValue((double)item.PriceRent1m2);
                                            cell.CellStyle = cellStyle;
                                        }
                                        else
                                        {
                                            cell.SetCellValue("");
                                            cell.CellStyle = cellStyle;
                                        }


                                    }
                                    else if (i == k + 2)
                                    {
                                        if (item.PriceRent1m2 != null)
                                        {
                                            cell.SetCellValue((double)item.PriceAfterDiscount);
                                            cell.CellStyle = cellStyle;
                                        }
                                        else
                                        {
                                            cell.SetCellValue("");
                                            cell.CellStyle = cellStyle;
                                        }
                                    }
                                    else if (i == k + 3)
                                    {
                                        cell.SetCellValue(item.Note);
                                        cell.CellStyle = cellStyle;
                                    }
                                }
                                rowStart++;
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in data.Bcts)
                        {
                            if (item.check != true)
                            {
                                IRow row = sheet.CreateRow(rowStart);

                                for (int i = 0; i < datacol; i++)
                                {
                                    ICell cell = row.CreateCell(i);

                                    if (i == 0)
                                    {
                                        cell.SetCellValue("Cấp");
                                        cell.CellStyle = cellStyle;
                                    }
                                    else if (i == 1)
                                    {
                                        if (item.Level != null)
                                        {
                                            cell.SetCellValue((double)item.Level);

                                        }
                                        else
                                        {
                                            cell.SetCellValue("");

                                        }
                                        cell.CellStyle = cellStyle;
                                    }
                                    else if (i == 2)
                                    {
                                        cell.SetCellValue("Tầng/Lửng tầng");
                                        cell.CellStyle = cellStyle;
                                    }
                                    else if (i == 3)
                                    {
                                        if (item.AreaName != null)
                                        {
                                            cell.SetCellValue(item.AreaName);

                                        }
                                        else
                                        {
                                            cell.SetCellValue("");

                                        }
                                        cell.CellStyle = cellStyle;
                                    }
                                    else if (i == 4)
                                    {
                                        if (item.PrivateArea != null && item.Ktlcb != null)
                                        {
                                            cell.SetCellValue((double)((decimal)item.PrivateArea * item.Ktlcb));

                                        }
                                        else
                                        {
                                            cell.SetCellValue("");
                                        }
                                        cell.CellStyle = decimalStyle;
                                    }
                                    else if (i == 5)
                                    {
                                        if (item.Ktdbt != null)
                                        {
                                            cell.SetCellValue((double)item.Ktdbt);
                                            cell.CellStyle = decimalStyle;
                                        }
                                        else
                                        {
                                            cell.SetCellValue("");
                                            cell.CellStyle = decimalStyle;
                                        }

                                    }
                                    else if (i == 6)
                                    {
                                        if (item.PrivateArea != null)
                                        {
                                            cell.SetCellValue((double)item.PrivateArea);
                                            cell.CellStyle = decimalStyle;
                                        }
                                        else
                                        {
                                            cell.SetCellValue("");
                                            cell.CellStyle = decimalStyle;
                                        }

                                    }
                                    else if (i == ks)
                                    {
                                        if (item.chilDfs.Count > 0)
                                        {
                                            foreach (var x in item.chilDfs)
                                            {
                                                ICell cellArea = row.CreateCell(i);
                                                cellArea.SetCellValue((double)x.Value);
                                                cellArea.CellStyle = decimalStyle;
                                                i++;
                                            }
                                            k = i;
                                            i = i - 1;
                                        }
                                        else
                                        {
                                            k = i;
                                            i = i - 1;
                                            ks = ks - 1;
                                        }

                                    }
                                    else if (i == k)
                                    {
                                        if (item.TotalK != null)
                                        {
                                            cell.SetCellValue((double)item.TotalK);
                                            cell.CellStyle = decimalStyle;
                                        }
                                        else
                                        {
                                            cell.SetCellValue("");
                                            cell.CellStyle = decimalStyle;
                                        }

                                    }
                                    else if (i == k + 1)
                                    {
                                        if (item.PriceRent1m2 != null)
                                        {
                                            cell.SetCellValue((double)item.PriceRent1m2);
                                        }
                                        else
                                        {
                                            cell.SetCellValue("");
                                        }
                                        cell.CellStyle = decimalStyle;
                                    }
                                    else if (i == k + 2)
                                    {
                                        if (item.PriceAfterDiscount != null)
                                        {
                                            cell.SetCellValue((double)item.PriceAfterDiscount);
                                        }
                                        else
                                        {
                                            cell.SetCellValue("");
                                        }
                                        cell.CellStyle = decimalStyle;
                                    }
                                    else if (i == k + 3)
                                    {
                                        if (item.Note != null)
                                        {
                                            cell.SetCellValue(item.Note);
                                        }
                                        else
                                        {
                                            cell.SetCellValue("");
                                        }
                                        cell.CellStyle = cellStyle;
                                    }
                                }
                                rowStart++;
                            }
                        }
                    }


                    for (int item = 0; item < 5; item++)
                    {
                        IRow rowLast = sheet.CreateRow(rowStart);
                        if (item == 0)
                        {
                            for (int n = 0; n < datacol; n++)
                            {
                                if (n == 0)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("Cộng");
                                    CellRangeAddress mergedRegioncellcell = new CellRangeAddress(rowStart, rowStart, 0, 4);
                                    sheet.AddMergedRegion(mergedRegioncellcell);
                                    RegionUtil.SetBorderBottom(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderLeft(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderRight(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderTop(1, mergedRegioncellcell, sheet);

                                }
                                else if (n == 5)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                    cell.CellStyle = cellStyle;
                                }
                                else if (n == 6)
                                {
                                    if (data.bCTs != null)
                                    {
                                        ICell cell = rowLast.CreateCell(n);
                                        cell.CellStyle = cellStyle;
                                        cell.SetCellValue((double)data.bCTs.Sum(p => p.PrivateArea));
                                        cell.CellStyle = decimalStyle;
                                    }
                                    else
                                    {
                                        ICell cell = rowLast.CreateCell(n);
                                        if (data.Bcts.First().PrivateArea != null)
                                        {
                                            cell.CellStyle = cellStyle;
                                            cell.SetCellValue((double)data.Bcts.Sum(p => p.PrivateArea));
                                        }
                                        else
                                        {
                                            cell.SetCellValue("");
                                        }
                                        cell.CellStyle = decimalStyle;
                                    }

                                }
                                else if (n == k)
                                {
                                    if (count > 0)
                                    {
                                        for (int z = 0; z < count; z++)
                                        {
                                            ICell cellz = rowLast.CreateCell(n);
                                            cellz.SetCellValue("");
                                            cellz.CellStyle = cellStyle;
                                            n++;
                                        }
                                        n = n - 1;
                                    }
                                }
                                else if (n == 7 + count)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                                else if (n == 7 + count + 1)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                                else if (n == (7 + count + 2))
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    if (data.bCTs != null)
                                    {
                                        cell.SetCellValue((double)(data.bCTs.Sum(p => p.PriceAfterDiscount) - (data.bCTs[data.bCTs.Count() - 1].PriceAfterDiscount)));
                                    }
                                    else
                                    {
                                        if (data.Bcts.First().PriceAfterDiscount != null)
                                        {
                                            cell.SetCellValue((double)(data.Bcts.Sum(p => p.PriceAfterDiscount) - (data.Bcts[data.Bcts.Count() - 1].PriceAfterDiscount)));
                                        }
                                        else
                                        {
                                            cell.SetCellValue("");
                                        }
                                    }
                                    cell.CellStyle = decimalStyle;

                                }
                                else if (n == (7 + count + 3))
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                            }
                            rowStart++;
                        }
                        else if (item == 1)
                        {
                            for (int n = 0; n < datacol; n++)
                            {
                                if (n == 0)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("Giảm giá chính sách");
                                    CellRangeAddress mergedRegioncellcell = new CellRangeAddress(rowStart, rowStart, 0, 6 + count);
                                    sheet.AddMergedRegion(mergedRegioncellcell);
                                    RegionUtil.SetBorderBottom(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderLeft(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderRight(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderTop(1, mergedRegioncellcell, sheet);

                                }
                                else if (n == 6 + count + 1)
                                {
                                    if (data.bCTs != null)
                                    {
                                        foreach (var item1 in data.bCTs)
                                        {
                                            ICell cellHeader1s = rowLast.CreateCell(n);
                                            cellHeader1s.SetCellValue((double)item1.DiscountCoff);
                                            cellHeader1s.CellStyle = cellStyle;
                                            n++;

                                            ICell cellHeader2s = rowLast.CreateCell(n);
                                            cellHeader2s.SetCellValue("");
                                            cellHeader2s.CellStyle = cellStyle;
                                            n++;

                                            ICell cellHeader3s = rowLast.CreateCell(n);
                                            decimal v = (decimal)((decimal)data.bCTs.Sum(p => p.PriceAfterDiscount) - (data.bCTs[data.bCTs.Count() - 1].PriceAfterDiscount)); //Tổng

                                            cellHeader3s.SetCellValue((double)(v * (item1.DiscountCoff / 100)));
                                            cellHeader3s.CellStyle = decimalStyle;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        foreach (var item1 in data.Bcts)
                                        {
                                            ICell cellHeader1s = rowLast.CreateCell(n);
                                            if (item1.DiscountCoff != null)
                                            {
                                                cellHeader1s.SetCellValue((double)item1.DiscountCoff);
                                            }
                                            else
                                            {
                                                cellHeader1s.SetCellValue("");
                                            }
                                            cellHeader1s.CellStyle = cellStyle;
                                            n++;

                                            ICell cellHeader2s = rowLast.CreateCell(n);
                                            cellHeader2s.SetCellValue("");
                                            cellHeader2s.CellStyle = cellStyle;
                                            n++;

                                            ICell cellHeader3s = rowLast.CreateCell(n);
                                            decimal v = 0;
                                            if (data.Bcts.First().PriceAfterDiscount != null)
                                            {
                                                v = (decimal)((decimal)data.Bcts.Sum(p => p.PriceAfterDiscount) - (data.Bcts[data.Bcts.Count() - 1].PriceAfterDiscount)); //Tổng
                                            }
                                            if (item1.DiscountCoff != null)
                                            {
                                                cellHeader3s.SetCellValue((double)(v * (item1.DiscountCoff / 100)));
                                            }
                                            else
                                            {
                                                cellHeader3s.SetCellValue("");
                                            }
                                            cellHeader3s.CellStyle = decimalStyle;
                                            break;
                                        }

                                    }
                                }
                                else if (n == 6 + count + 4)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");

                                }
                            }
                            rowStart++;

                        }
                        else if (item == 2)
                        {
                            for (int n = 0; n < datacol; n++)
                            {


                                if (n == 0)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("Tiền thuê sau miễn giảm");
                                    CellRangeAddress mergedRegioncellcell = new CellRangeAddress(rowStart, rowStart, 0, 6 + count + 1);
                                    sheet.AddMergedRegion(mergedRegioncellcell);
                                    RegionUtil.SetBorderBottom(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderLeft(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderRight(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderTop(1, mergedRegioncellcell, sheet);
                                }
                                else if (n == 6 + count + 2)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                                else if (n == 6 + count + 3)
                                {
                                    if (data.bCTs != null)
                                    {
                                        foreach (var item1 in data.bCTs)
                                        {
                                            decimal v = 0;
                                            decimal x = 0;
                                            ICell cellHeader1s = rowLast.CreateCell(n);
                                            if (data.bCTs.First().PriceAfterDiscount != null)
                                            {
                                                v = (decimal)((decimal)data.bCTs.Sum(p => p.PriceAfterDiscount) - (data.bCTs[data.bCTs.Count() - 1].PriceAfterDiscount)); //Tổng
                                                x = v * (decimal)(item1.DiscountCoff / 100); //Giảm chính sách
                                            }
                                            cellHeader1s.SetCellValue((double)(v - x));
                                            cellHeader1s.CellStyle = decimalStyle;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        foreach (var item1 in data.Bcts)
                                        {
                                            decimal v = 0;
                                            decimal x = 0;
                                            ICell cellHeader1s = rowLast.CreateCell(n);
                                            if (data.Bcts.First().PriceAfterDiscount != null)
                                            {
                                                v = (decimal)((decimal)data.Bcts.Sum(p => p.PriceAfterDiscount) - (data.Bcts[data.Bcts.Count() - 1].PriceAfterDiscount)); //Tổng
                                                if (item1.DiscountCoff != null)
                                                {
                                                    x = v * (decimal)(item1.DiscountCoff / 100); //Giảm chính sách
                                                }
                                                else
                                                {
                                                    x = 0;
                                                }
                                            }
                                            cellHeader1s.SetCellValue((double)(v - x));
                                            cellHeader1s.CellStyle = decimalStyle;
                                            break;
                                        }
                                    }

                                }
                                else if (n == 6 + count + 4)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                            }
                            rowStart++;

                        }
                        else if (item == 3)
                        {
                            for (int n = 0; n < datacol; n++)
                            {

                                if (n == 0)
                                {
                                    if (data.bCTs != null)
                                    {
                                        foreach (var item1 in data.bCTs)
                                        {
                                            ICell cell = rowLast.CreateCell(n);
                                            cell.CellStyle = cellStyle;
                                            cell.SetCellValue("Thuế VAT" + "  " + item1.VAT + "%");
                                            CellRangeAddress mergedRegioncellcell = new CellRangeAddress(rowStart, rowStart, 0, 6 + count + 1);
                                            sheet.AddMergedRegion(mergedRegioncellcell);
                                            RegionUtil.SetBorderBottom(1, mergedRegioncellcell, sheet);
                                            RegionUtil.SetBorderLeft(1, mergedRegioncellcell, sheet);
                                            RegionUtil.SetBorderRight(1, mergedRegioncellcell, sheet);
                                            RegionUtil.SetBorderTop(1, mergedRegioncellcell, sheet);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        foreach (var item1 in data.Bcts)
                                        {
                                            ICell cell = rowLast.CreateCell(n);
                                            cell.CellStyle = cellStyle;
                                            cell.SetCellValue("Thuế VAT" + "  " + item1.VAT + "%");
                                            CellRangeAddress mergedRegioncellcell = new CellRangeAddress(rowStart, rowStart, 0, 6 + count + 1);
                                            sheet.AddMergedRegion(mergedRegioncellcell);
                                            RegionUtil.SetBorderBottom(1, mergedRegioncellcell, sheet);
                                            RegionUtil.SetBorderLeft(1, mergedRegioncellcell, sheet);
                                            RegionUtil.SetBorderRight(1, mergedRegioncellcell, sheet);
                                            RegionUtil.SetBorderTop(1, mergedRegioncellcell, sheet);
                                            break;
                                        }
                                    }

                                }
                                else if (n == 6 + count + 2)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                                else if (n == 6 + count + 3)
                                {
                                    if (data.bCTs != null)
                                    {
                                        foreach (var item1 in data.bCTs)
                                        {
                                            ICell cellHeader1s = rowLast.CreateCell(n);
                                            decimal v = (decimal)((decimal)data.bCTs.Sum(p => p.PriceAfterDiscount) - (data.bCTs[data.bCTs.Count() - 1].PriceAfterDiscount)); //Tổng
                                            if (item1.DiscountCoff != null)
                                            {
                                                decimal x = v * (decimal)(item1.DiscountCoff / 100); //Giảm chính sách
                                                cellHeader1s.SetCellValue((double)(v - x) * (item1.VAT / 100));
                                            }
                                            else
                                            {
                                                decimal x = 0;
                                                cellHeader1s.SetCellValue((double)(v - x) * (item1.VAT / 100));
                                            }
                                            cellHeader1s.CellStyle = decimalStyle;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        foreach (var item1 in data.Bcts)
                                        {
                                            decimal v = 0;
                                            decimal x = 0;
                                            ICell cellHeader1s = rowLast.CreateCell(n);
                                            if (data.Bcts.First().PriceAfterDiscount != null)
                                            {
                                                v = (decimal)((decimal)data.Bcts.Sum(p => p.PriceAfterDiscount) - (data.Bcts[data.Bcts.Count() - 1].PriceAfterDiscount)); //Tổng
                                                if (item1.DiscountCoff != null)
                                                {
                                                    x = v * (decimal)(item1.DiscountCoff / 100); //Giảm chính sách
                                                }
                                                else
                                                {
                                                    x = 0;
                                                }
                                            }
                                            cellHeader1s.SetCellValue((double)((double)(v - x) * (item1.VAT / 100)));
                                            cellHeader1s.CellStyle = decimalStyle;
                                            break;
                                        }
                                    }

                                }
                                else if (n == 6 + count + 4)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                            }
                            rowStart++;
                        }
                        else if (item == 4)
                        {
                            for (int n = 0; n < datacol; n++)
                            {
                                if (n == 0)
                                {
                                    ICell cell = rowLast.CreateCell(n);

                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("Tổng giá sau thuê");
                                    CellRangeAddress mergedRegioncellcell = new CellRangeAddress(rowStart, rowStart, 0, 6 + count + 1);
                                    sheet.AddMergedRegion(mergedRegioncellcell);
                                    RegionUtil.SetBorderBottom(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderLeft(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderRight(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderTop(1, mergedRegioncellcell, sheet);
                                }
                                else if (n == 6 + count + 2)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                                else if (n == 6 + count + 3)
                                {
                                    if (data.bCTs != null)
                                    {
                                        foreach (var item1 in data.bCTs)
                                        {
                                            ICell cellHeader1s = rowLast.CreateCell(n);

                                            decimal v = (decimal)((decimal)data.bCTs.Sum(p => p.PriceAfterDiscount) - (data.bCTs[data.bCTs.Count() - 1].PriceAfterDiscount)); //Tổng
                                            if (item1.DiscountCoff != null)
                                            {
                                                decimal x = v * (decimal)(item1.DiscountCoff / 100); //Giảm chính sách
                                                decimal z = v - x;
                                                decimal y = (decimal)((double)z * (item1.VAT / 100));
                                                cellHeader1s.SetCellValue((double)(z + y));
                                            }
                                            else
                                            {
                                                decimal x = 0;
                                                decimal z = v - x;
                                                decimal y = (decimal)((double)z * (item1.VAT / 100));
                                                cellHeader1s.SetCellValue((double)(z + y));
                                            }
                                            cellHeader1s.CellStyle = decimalStyle;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        foreach (var item1 in data.Bcts)
                                        {
                                            ICell cellHeader1s = rowLast.CreateCell(n);
                                            if (data.Bcts != null)
                                            {
                                                decimal v = 0;
                                                decimal x = 0;

                                                if (data.Bcts.First().PriceAfterDiscount != null)
                                                {
                                                    v = (decimal)((decimal)data.Bcts.Sum(p => p.PriceAfterDiscount) - (data.Bcts[data.Bcts.Count() - 1].PriceAfterDiscount)); //Tổng
                                                    if (item1.DiscountCoff != null)
                                                    {
                                                        x = v * (decimal)(item1.DiscountCoff / 100); //Giảm chính sách
                                                    }
                                                    else
                                                    {
                                                        x = 0;
                                                    }
                                                }
                                                decimal z = v - x;
                                                decimal y = (decimal)((double)z * (item1.VAT / 100));
                                                cellHeader1s.SetCellValue((double)(z + y));
                                            }
                                            else
                                            {
                                                cellHeader1s.SetCellValue("");
                                            }
                                            cellHeader1s.CellStyle = decimalStyle;
                                            break;
                                        }
                                    }

                                }
                                else if (n == 6 + count + 4)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                            }
                        }
                    }

                    rowStart += 2;
                    ICellStyle style1 = workbook.CreateCellStyle();
                    style1.VerticalAlignment = VerticalAlignment.Center;

                    IFont font = workbook.CreateFont();
                    font.Boldweight = (short)FontBoldWeight.Bold;
                    font.FontName = "Times New Roman";
                    style1.SetFont(font);
                    style1.WrapText = false;

                    ICellStyle style2 = workbook.CreateCellStyle();
                    style2.VerticalAlignment = VerticalAlignment.Center;
                    IFont font2 = workbook.CreateFont();
                    font2.FontName = "Times New Roman";
                    style2.SetFont(font2);

                    ICellStyle style3 = workbook.CreateCellStyle();
                    IFont fontstyle1 = workbook.CreateFont();
                    fontstyle1.FontName = "Times New Roman";
                    fontstyle1.FontHeightInPoints = 13.5;
                    style3.SetFont(fontstyle);
                    style3.VerticalAlignment = VerticalAlignment.Center;


                    IFont font3 = workbook.CreateFont();
                    font3.FontName = "Times New Roman";
                    style2.SetFont(font3);

                    XSSFRow row1 = (XSSFRow)sheet.CreateRow(rowStart);
                    row1.CreateCell(0).CellStyle = style2;
                    rowStart++;


                    XSSFRow row2 = (XSSFRow)sheet.CreateRow(rowStart);
                    row2.CreateCell(0).CellStyle = style2;
                    rowStart++;


                    XSSFRow row3 = (XSSFRow)sheet.CreateRow(rowStart);
                    row3.CreateCell(0).CellStyle = style2;
                    rowStart++;

                    XSSFRow row4 = (XSSFRow)sheet.CreateRow(rowStart);
                    row4.CreateCell(8).CellStyle = style3;
                    rowStart++;

                    XSSFRow row5 = (XSSFRow)sheet.CreateRow(rowStart);
                    row5.CreateCell(1).CellStyle = style1;
                    row5.CreateCell(4).CellStyle = style1;
                    row5.CreateCell(6).CellStyle = style1;
                    row5.CreateCell(10).CellStyle = style1;

                    rowStart = rowStart + 7;

                    XSSFRow row6 = (XSSFRow)sheet.CreateRow(rowStart);
                    row6.CreateCell(9).CellStyle = style1;
                    rowStart++;

                    XSSFRow row7 = (XSSFRow)sheet.CreateRow(rowStart);
                    row7.CreateCell(3).CellStyle = style3;
                    row7.CreateCell(8).CellStyle = style3;


                    row1.GetCell(0).SetCellValue("Ghi chú: ");

                    row2.GetCell(0).SetCellValue("'- Trường hợp có thay đổi về thời điểm bố trí sử dụng hoặc Chính phủ có điều chỉnh tăng mức lương cơ sở thì đơn giá thuê sẽ được điều chỉnh tăng tương ứng;");

                    row3.GetCell(0).SetCellValue("'- Về công nợ: tham khảo Giấy xác nhận về tiền thuê nhà ở cũ thuộc sở hữu nhà nước số 06/GXN-QLN ngày 30 tháng 9 năm 2022 của Công ty TNHH MTV Dịch vụ công ích Quận 2.");

                    row4.GetCell(8).SetCellValue("TP. Hồ Chí Minh, Ngày    tháng    năm 2023");

                    row5.GetCell(1).SetCellValue("HỘ SỬ DỤNG NHÀ");
                    row5.GetCell(4).SetCellValue("NGƯỜI LẬP");
                    row5.GetCell(6).SetCellValue("PHÓ TRƯỞNG PHÒNG");
                    row5.GetCell(10).SetCellValue("PHÓ GIÁM ĐỐC");

                    row6.GetCell(9).SetCellValue("DUYỆT MẪU");

                    row7.GetCell(3).SetCellValue("Người thực hiện");
                    row7.GetCell(8).SetCellValue("Thông qua Phó Trưởng phòng phụ trách");


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

        //API dowloadExcelQD1753&09
        [HttpPost("ExportGetWorkSheet2/{RentFilesId}")]
        public async Task<IActionResult> ExportGetWorkSheet2(Guid RentFilesId, [FromBody] List<BCT> input)
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
            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            {
                return new FileContentResult(new byte[0], "application/octet-stream");
            }

            RentFile dataBct = _context.RentFiles.Where(x => x.Id == RentFilesId && x.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
            if (dataBct == null)
            {
                def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                return Ok(def);
            }
            RentFlieData mapper_data = _mapper.Map<RentFlieData>(dataBct);

            mapper_data.CustomerName = _context.Customers.Where(f => f.Id == mapper_data.CustomerId && f.Status != AppEnums.EntityStatus.DELETED).Select(s => s.FullName).FirstOrDefault();

            XSSFWorkbook wb = new XSSFWorkbook();
            // Tạo ra 1 sheet
            ISheet sheet = wb.CreateSheet();

            string template = @"templates/NOC-BCT.xlsx";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string templatePath = Path.Combine(webRootPath, template);
            string nameExcel = "NOC-BCT.xls";

            MemoryStream ms = WriteDataToExcel2(templatePath, 0, input, mapper_data);
            byte[] byteArrayContent = ms.ToArray();
            return File(ms.ToArray(), "application/vnd.ms-excel", nameExcel);
        }
        private static MemoryStream WriteDataToExcel2(string templatePath, int sheetnumber, List<BCT> data, RentFlieData rentFile)
        {
            FileStream file = new FileStream(templatePath, FileMode.Open, FileAccess.Read);
            XSSFWorkbook workbook = new XSSFWorkbook(file);
            ISheet sheet = workbook.GetSheetAt(sheetnumber);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            int rowHeader = 10;
            int rowHeader1 = 11;
            int rowStart = 12;

            if (sheet != null)
            {
                try
                {
                    IRow rowD3 = sheet.GetRow(2);
                    ICell cellD3 = rowD3.GetCell(2);
                    if (rentFile.fullAddressCH != null)
                    {
                        cellD3.SetCellValue(rentFile.fullAddressCH);

                    }
                    else
                    {
                        cellD3.SetCellValue(rentFile.fullAddressCN);
                    }

                    IRow rowD4 = sheet.GetRow(3);
                    ICell cellD4 = rowD4.GetCell(2);
                    cellD4.SetCellValue(rentFile.CustomerName);

                    IRow rowD5 = sheet.GetRow(5);
                    ICell cellD5 = rowD5.GetCell(3);
                    cellD5.SetCellValue(rentFile.CodeHS);

                    IRow rowD7 = sheet.GetRow(2);
                    ICell cellD7 = rowD7.GetCell(9);
                    if (rentFile.CodeCH != null)
                    {
                        cellD7.SetCellValue(rentFile.CodeCH);
                    }
                    else
                    {
                        cellD7.SetCellValue(rentFile.CodeCN);
                    }

                    int datacol = 0;
                    int childDf = 0;

                    foreach (var e in data)
                    {
                        childDf = (e.chilDfs != null ? e.chilDfs.Count : 0);
                    }
                    datacol = 19 + childDf;

                    var style = workbook.CreateCellStyle();
                    style.BorderBottom = BorderStyle.Thin;
                    style.BorderLeft = BorderStyle.Thin;
                    style.BorderRight = BorderStyle.Thin;
                    style.BorderTop = BorderStyle.Thin;

                    ICellStyle cellStyle = workbook.CreateCellStyle();
                    cellStyle.Alignment = HorizontalAlignment.Center;
                    cellStyle.BorderBottom = BorderStyle.Thin;
                    cellStyle.BorderLeft = BorderStyle.Thin;
                    cellStyle.BorderRight = BorderStyle.Thin;
                    cellStyle.BorderTop = BorderStyle.Thin;

                    IRow rowHeaders = sheet.CreateRow(rowHeader);
                    IRow rowHeaders1 = sheet.CreateRow(rowHeader1);

                    for (int i = 0; i < datacol; i++)
                    {
                        sheet.SetColumnWidth(i, (int)(15.5 * 256));
                    }

                    ICell cellHeader1 = rowHeaders.CreateCell(0);
                    cellHeader1.SetCellValue("Loại nhà");
                    cellHeader1.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader1 = new CellRangeAddress(rowHeader, rowHeader1, 0, 3);
                    sheet.AddMergedRegion(mergedRegioncellHeader1);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader1, sheet);//Bottom border  
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader1, sheet);//Bottom border  
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader1, sheet);//Bottom border  
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader1, sheet);//Bottom border 

                    ICell cellHeader2 = rowHeaders.CreateCell(4);
                    cellHeader2.SetCellValue("Giá chuẩn theo tỷ lệ tăng lương cơ bản");
                    cellHeader2.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader2 = new CellRangeAddress(rowHeader, rowHeader1, 4, 4);
                    sheet.AddMergedRegion(mergedRegioncellHeader2);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader2, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader2, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader2, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader2, sheet);

                    ICell cellHeader3 = rowHeaders.CreateCell(5);
                    cellHeader3.SetCellValue("Hệ số thời điểm bố trí sử dụng ");
                    cellHeader3.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader3 = new CellRangeAddress(rowHeader, rowHeader1, 5, 5);
                    sheet.AddMergedRegion(mergedRegioncellHeader3);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader3, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader3, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader3, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader3, sheet);

                    ICell cellHeader4 = rowHeaders.CreateCell(6);
                    cellHeader4.SetCellValue("Diện tích");
                    cellHeader4.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader4 = new CellRangeAddress(rowHeader, rowHeader1, 6, 6);
                    sheet.AddMergedRegion(mergedRegioncellHeader4);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader4, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader4, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader4, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader4, sheet);

                    int excelCount = 7;
                    int count = 0;
                    for (int v = 0; v < data[0].chilDfs.Count; v++)
                    {
                        if (data[0].chilDfs[v].Value != 0)
                        {
                            count++;
                        }
                    }

                    ICell cellHeader5 = rowHeaders.CreateCell(7);
                    cellHeader5.SetCellValue("Hệ số");
                    cellHeader5.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader5 = new CellRangeAddress(rowHeader, rowHeader, excelCount, excelCount + count);
                    sheet.AddMergedRegion(mergedRegioncellHeader5);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader5, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader5, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader5, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader5, sheet);


                    foreach (var e in data)
                    {
                        foreach (var eItem in e.chilDfs)
                        {
                            if (eItem.Value != null)
                            {
                                ICell cellHeaders = rowHeaders1.CreateCell(excelCount);
                                cellHeaders.SetCellValue(eItem.Code);
                                cellHeaders.CellStyle = cellStyle;

                                excelCount++;
                            }
                        }
                        break;
                    }

                    ICell cellHeader = rowHeaders1.CreateCell(excelCount);
                    cellHeader.SetCellValue("Tổng K");
                    cellHeader.CellStyle = cellStyle;
                    excelCount++;

                    int excelCountEnd = excelCount;

                    ICell cellHeader10 = rowHeaders.CreateCell(excelCountEnd);
                    cellHeader10.SetCellValue("Giá thuê (1m²/tháng)");
                    cellHeader10.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader10 = new CellRangeAddress(rowHeader, rowHeader1, excelCountEnd, excelCountEnd);
                    sheet.AddMergedRegion(mergedRegioncellHeader10);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader10, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader10, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader10, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader10, sheet);
                    excelCountEnd++;

                    ICell cellHeader11 = rowHeaders.CreateCell(excelCountEnd);
                    cellHeader11.SetCellValue("Giá thuê (đã tính VAT)");
                    cellHeader11.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader11 = new CellRangeAddress(rowHeader, rowHeader1, excelCountEnd, excelCountEnd);
                    sheet.AddMergedRegion(mergedRegioncellHeader11);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader11, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader11, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader11, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader11, sheet);
                    excelCountEnd++;

                    ICell cellHeader19 = rowHeaders.CreateCell(excelCountEnd);
                    cellHeader19.SetCellValue("Ghi chú");
                    cellHeader19.CellStyle = cellStyle;
                    CellRangeAddress mergedRegioncellHeader19 = new CellRangeAddress(rowHeader, rowHeader1, excelCountEnd, excelCountEnd);
                    sheet.AddMergedRegion(mergedRegioncellHeader19);
                    RegionUtil.SetBorderBottom(1, mergedRegioncellHeader19, sheet);
                    RegionUtil.SetBorderTop(1, mergedRegioncellHeader19, sheet);
                    RegionUtil.SetBorderLeft(1, mergedRegioncellHeader19, sheet);
                    RegionUtil.SetBorderRight(1, mergedRegioncellHeader19, sheet);
                    excelCountEnd++;

                    ICellStyle cellStyleDate = workbook.CreateCellStyle();
                    cellStyleDate.DataFormat = workbook.CreateDataFormat().GetFormat("dd/mm/yyyy");
                    cellStyleDate.BorderBottom = BorderStyle.Thin;
                    cellStyleDate.BorderLeft = BorderStyle.Thin;
                    cellStyleDate.BorderRight = BorderStyle.Thin;
                    cellStyleDate.BorderTop = BorderStyle.Thin;

                    ICellStyle cellStyleMoney = workbook.CreateCellStyle();
                    var dataFormat = workbook.CreateDataFormat();
                    cellStyleMoney.DataFormat = dataFormat.GetFormat("#,##0");
                    cellStyleMoney.BorderBottom = BorderStyle.Thin;
                    cellStyleMoney.BorderLeft = BorderStyle.Thin;
                    cellStyleMoney.BorderRight = BorderStyle.Thin;
                    cellStyleMoney.BorderTop = BorderStyle.Thin;

                    ICellStyle decimalStyle = workbook.CreateCellStyle();
                    decimalStyle.DataFormat = workbook.CreateDataFormat().GetFormat("#,##0.00");
                    decimalStyle.BorderBottom = BorderStyle.Thin;
                    decimalStyle.BorderLeft = BorderStyle.Thin;
                    decimalStyle.BorderRight = BorderStyle.Thin;
                    decimalStyle.BorderTop = BorderStyle.Thin;

                    int ks = 7;
                    int k = -10;
                    int firstRow = rowStart;

                    foreach (var item in data)
                    {
                        IRow row = sheet.CreateRow(rowStart);

                        for (int i = 0; i < datacol; i++)
                        {
                            ICell cell = row.CreateCell(i);

                            if (i == 0)
                            {
                                cell.SetCellValue("Cấp");
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == 1)
                            {
                                cell.SetCellValue((double)item.Level);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == 2)
                            {
                                cell.SetCellValue("Tầng/Lửng tầng");
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == 3)
                            {
                                cell.SetCellValue(item.AreaName);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == 4)
                            {
                                cell.SetCellValue((double)((decimal)item.PrivateArea * item.Ktlcb));
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == 5)
                            {
                                cell.SetCellValue((double)item.Ktdbt);
                                cell.CellStyle = cellStyle;
                            }
                            else if (i == 6)
                            {
                                cell.SetCellValue((double)item.PrivateArea);
                                cell.CellStyle = decimalStyle;
                            }
                            else if (i == ks)
                            {
                                foreach (var x in item.chilDfs)
                                {
                                    if (x.Value != 0)
                                    {
                                        ICell cellArea = row.CreateCell(i);
                                        cellArea.SetCellValue((double)x.Value);
                                        cellArea.CellStyle = cellStyle;
                                        i++;
                                    }
                                }
                                k = i;
                                i = i - 1;
                            }
                            else if (i == k)
                            {
                                cell.SetCellValue((double)item.TotalK);
                                cell.CellStyle = decimalStyle;
                            }
                            else if (i == k + 1)
                            {
                                cell.SetCellValue((double)item.PriceRent1m2);
                                cell.CellStyle = cellStyleMoney;

                            }
                            else if (i == k + 2)
                            {
                                cell.SetCellValue((double)item.PriceAfterDiscount);
                                cell.CellStyle = cellStyleMoney;
                            }
                            else if (i == k + 3)
                            {
                                cell.SetCellValue(item.Note);
                                cell.CellStyle = cellStyle;
                            }
                        }
                        rowStart++;
                    }

                    for (int item = 0; item < 5; item++)
                    {
                        IRow rowLast = sheet.CreateRow(rowStart);
                        if (item == 0)
                        {
                            for (int n = 0; n < datacol; n++)
                            {
                                if (n == 0)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("Cộng");
                                    CellRangeAddress mergedRegioncellcell = new CellRangeAddress(rowStart, rowStart, 0, 4);
                                    sheet.AddMergedRegion(mergedRegioncellcell);
                                    RegionUtil.SetBorderBottom(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderLeft(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderRight(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderTop(1, mergedRegioncellcell, sheet);

                                }
                                else if (n == 5)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                    cell.CellStyle = cellStyle;
                                }
                                else if (n == 6)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue((double)data.Sum(p => p.PrivateArea));
                                    cell.CellStyle = decimalStyle;
                                }
                                else if (n == 7)
                                {
                                    for (int z = 0; z < count; z++)
                                    {
                                        ICell cellz = rowLast.CreateCell(n);
                                        cellz.SetCellValue("");
                                        cellz.CellStyle = cellStyle;
                                        n++;
                                    }
                                    n = n - 1;
                                }
                                else if (n == 7 + count)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                                else if (n == 7 + count + 1)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                                else if (n == (7 + count + 2))
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.SetCellValue((double)data.Sum(p => p.PriceAfterDiscount));
                                    cell.CellStyle = cellStyleMoney;
                                }
                                else if (n == (7 + count + 3))
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                            }
                            rowStart++;
                        }
                        else if (item == 1)
                        {
                            for (int n = 0; n < datacol; n++)
                            {

                                if (n == 0)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("Giảm giá chính sách");
                                    CellRangeAddress mergedRegioncellcell = new CellRangeAddress(rowStart, rowStart, 0, 6 + count);
                                    sheet.AddMergedRegion(mergedRegioncellcell);
                                    RegionUtil.SetBorderBottom(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderLeft(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderRight(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderTop(1, mergedRegioncellcell, sheet);

                                }
                                else if (n == 6 + count + 1)
                                {
                                    foreach (var item1 in data)
                                    {
                                        ICell cellHeader1s = rowLast.CreateCell(n);
                                        cellHeader1s.SetCellValue((double)item1.DiscountCoff);
                                        cellHeader1s.CellStyle = cellStyle;
                                        n++;

                                        ICell cellHeader2s = rowLast.CreateCell(n);
                                        cellHeader2s.SetCellValue("");
                                        cellHeader2s.CellStyle = cellStyle;
                                        n++;

                                        ICell cellHeader3s = rowLast.CreateCell(n);
                                        if (item1.DiscountCoff != null)
                                        {
                                            cellHeader3s.SetCellValue((double)(data.Sum(p => p.PriceAfterDiscount) * (item1.DiscountCoff / 100)));
                                        }
                                        else
                                        {
                                            cellHeader3s.SetCellValue("");
                                        }
                                        cellHeader3s.CellStyle = decimalStyle;
                                        break;
                                    }
                                }
                                else if (n == 6 + count + 4)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");

                                }
                            }
                            rowStart++;

                        }
                        else if (item == 2)
                        {
                            for (int n = 0; n < datacol; n++)
                            {


                                if (n == 0)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("Tiền thuê sau miễn giảm");
                                    CellRangeAddress mergedRegioncellcell = new CellRangeAddress(rowStart, rowStart, 0, 6 + count + 1);
                                    sheet.AddMergedRegion(mergedRegioncellcell);
                                    RegionUtil.SetBorderBottom(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderLeft(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderRight(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderTop(1, mergedRegioncellcell, sheet);
                                }
                                else if (n == 6 + count + 2)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                                else if (n == 6 + count + 3)
                                {
                                    foreach (var item1 in data)
                                    {
                                        ICell cellHeader1s = rowLast.CreateCell(n);
                                        decimal v = (decimal)data.Sum(p => p.PriceAfterDiscount); //Tổng
                                        if (item1.DiscountCoff != null)
                                        {
                                            decimal x = v * (decimal)(item1.DiscountCoff / 100); //Giảm chính sách
                                            cellHeader1s.SetCellValue((double)(v - x));
                                        }
                                        else
                                        {
                                            decimal x = 0;
                                            cellHeader1s.SetCellValue((double)(v - x));
                                        }
                                        cellHeader1s.CellStyle = decimalStyle;
                                        break;
                                    }
                                }
                                else if (n == 6 + count + 4)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                            }
                            rowStart++;

                        }
                        else if (item == 3)
                        {
                            for (int n = 0; n < datacol; n++)
                            {

                                if (n == 0)
                                {
                                    foreach (var item1 in data)
                                    {
                                        ICell cell = rowLast.CreateCell(n);
                                        cell.CellStyle = cellStyle;
                                        cell.SetCellValue("Thuế VAT" + "  " + item1.VAT + "%");
                                        CellRangeAddress mergedRegioncellcell = new CellRangeAddress(rowStart, rowStart, 0, 6 + count + 1);
                                        sheet.AddMergedRegion(mergedRegioncellcell);
                                        RegionUtil.SetBorderBottom(1, mergedRegioncellcell, sheet);
                                        RegionUtil.SetBorderLeft(1, mergedRegioncellcell, sheet);
                                        RegionUtil.SetBorderRight(1, mergedRegioncellcell, sheet);
                                        RegionUtil.SetBorderTop(1, mergedRegioncellcell, sheet);
                                        break;
                                    }
                                }
                                else if (n == 6 + count + 2)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                                else if (n == 6 + count + 3)
                                {
                                    foreach (var item1 in data)
                                    {
                                        ICell cellHeader1s = rowLast.CreateCell(n);
                                        decimal v = (decimal)data.Sum(p => p.PriceAfterDiscount); //Tổng
                                        if (item1.DiscountCoff != null)
                                        {
                                            decimal x = v * (decimal)(item1.DiscountCoff / 100); //Giảm chính sách
                                            cellHeader1s.SetCellValue((double)(v - x) * (item1.VAT / 100));
                                        }
                                        else
                                        {
                                            decimal x = 0;
                                            cellHeader1s.SetCellValue((double)(v - x) * (item1.VAT / 100));
                                        }
                                        cellHeader1s.CellStyle = decimalStyle;
                                        break;
                                    }
                                }
                                else if (n == 6 + count + 4)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                            }
                            rowStart++;

                        }
                        else if (item == 4)
                        {
                            for (int n = 0; n < datacol; n++)
                            {


                                if (n == 0)
                                {
                                    ICell cell = rowLast.CreateCell(n);

                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("Tổng giá sau thuê");
                                    CellRangeAddress mergedRegioncellcell = new CellRangeAddress(rowStart, rowStart, 0, 6 + count + 1);
                                    sheet.AddMergedRegion(mergedRegioncellcell);
                                    RegionUtil.SetBorderBottom(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderLeft(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderRight(1, mergedRegioncellcell, sheet);
                                    RegionUtil.SetBorderTop(1, mergedRegioncellcell, sheet);
                                }
                                else if (n == 6 + count + 2)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                                else if (n == 6 + count + 3)
                                {
                                    foreach (var item1 in data)
                                    {
                                        ICell cellHeader1s = rowLast.CreateCell(n);

                                        decimal v = (decimal)data.Sum(p => p.PriceAfterDiscount);
                                        if (item1.DiscountCoff != null)
                                        {
                                            decimal x = (decimal)(v * (item1.DiscountCoff / 100)); //Giảm chính sách
                                            decimal z = v - x;
                                            decimal y = (decimal)((double)z * (item1.VAT / 100));
                                            cellHeader1s.SetCellValue((double)(z + y));
                                        }
                                        else
                                        {
                                            decimal x = 0;
                                            decimal z = v - x;
                                            decimal y = (decimal)((double)z * (item1.VAT / 100));
                                            cellHeader1s.SetCellValue((double)(z + y));
                                        }
                                        cellHeader1s.CellStyle = decimalStyle;
                                        break;
                                    }
                                }
                                else if (n == 6 + count + 4)
                                {
                                    ICell cell = rowLast.CreateCell(n);
                                    cell.CellStyle = cellStyle;
                                    cell.SetCellValue("");
                                }
                            }
                        }
                    }

                    rowStart += 2;
                    ICellStyle style1 = workbook.CreateCellStyle();
                    style1.Alignment = HorizontalAlignment.Center;
                    style1.VerticalAlignment = VerticalAlignment.Center;

                    IFont font = workbook.CreateFont();
                    font.Boldweight = (short)FontBoldWeight.Bold;
                    font.FontName = "Times New Roman";
                    style1.SetFont(font);
                    style1.WrapText = true;

                    ICellStyle style2 = workbook.CreateCellStyle();
                    style2.VerticalAlignment = VerticalAlignment.Center;
                    IFont font2 = workbook.CreateFont();
                    font2.FontName = "Times New Roman";
                    style2.SetFont(font2);

                    ICellStyle style3 = workbook.CreateCellStyle();
                    style3.Alignment = HorizontalAlignment.Center;
                    style3.VerticalAlignment = VerticalAlignment.Center;

                    IFont font3 = workbook.CreateFont();
                    font3.FontName = "Times New Roman";
                    style2.SetFont(font3);

                    XSSFRow row1 = (XSSFRow)sheet.CreateRow(rowStart);
                    row1.CreateCell(0).CellStyle = style2;
                    rowStart++;


                    XSSFRow row2 = (XSSFRow)sheet.CreateRow(rowStart);
                    row2.CreateCell(0).CellStyle = style2;
                    rowStart++;


                    XSSFRow row3 = (XSSFRow)sheet.CreateRow(rowStart);
                    row3.CreateCell(0).CellStyle = style2;
                    rowStart++;

                    XSSFRow row4 = (XSSFRow)sheet.CreateRow(rowStart);
                    row4.CreateCell(13).CellStyle = style3;
                    rowStart++;

                    XSSFRow row5 = (XSSFRow)sheet.CreateRow(rowStart);
                    row5.CreateCell(0).CellStyle = style1;
                    row5.CreateCell(4).CellStyle = style1;
                    row5.CreateCell(7).CellStyle = style1;
                    row5.CreateCell(11).CellStyle = style1;
                    CellRangeAddress mergeRow50 = new CellRangeAddress(rowStart, rowStart, 0, 2);
                    CellRangeAddress mergeRow54 = new CellRangeAddress(rowStart, rowStart, 4, 6);
                    CellRangeAddress mergeRow57 = new CellRangeAddress(rowStart, rowStart, 7, 10);
                    CellRangeAddress mergeRow55 = new CellRangeAddress(rowStart, rowStart, 11, 12);

                    rowStart = rowStart + 3;

                    XSSFRow row6 = (XSSFRow)sheet.CreateRow(rowStart);
                    row6.CreateCell(9).CellStyle = style1;
                    CellRangeAddress mergeRow6 = new CellRangeAddress(rowStart, rowStart, 9, 14);
                    rowStart++;

                    XSSFRow row7 = (XSSFRow)sheet.CreateRow(rowStart);
                    row7.CreateCell(0).CellStyle = style3;
                    row7.CreateCell(9).CellStyle = style3;
                    CellRangeAddress mergeRow71 = new CellRangeAddress(rowStart, rowStart, 0, 6);
                    CellRangeAddress mergeRow72 = new CellRangeAddress(rowStart, rowStart, 9, 14);


                    row1.GetCell(0).SetCellValue("Ghi chú: ");

                    row2.GetCell(0).SetCellValue("'- Trường hợp có thay đổi về thời điểm bố trí sử dụng hoặc Chính phủ có điều chỉnh tăng mức lương cơ sở thì đơn giá thuê sẽ được điều chỉnh tăng tương ứng;");

                    row3.GetCell(0).SetCellValue("'- Về công nợ: tham khảo Giấy xác nhận về tiền thuê nhà ở cũ thuộc sở hữu nhà nước số 06/GXN-QLN ngày 30 tháng 9 năm 2022 của Công ty TNHH MTV Dịch vụ công ích Quận 2.");

                    row4.GetCell(13).SetCellValue("TP. Hồ Chí Minh, Ngày        tháng       năm 2023");

                    row5.GetCell(0).SetCellValue("HỘ SỬ DỤNG NHÀ");
                    row5.GetCell(4).SetCellValue("NGƯỜI LẬP");
                    row5.GetCell(7).SetCellValue("PHÓ TRƯỞNG PHÒNG");
                    row5.GetCell(11).SetCellValue("PHÓ GIÁM ĐỐC");

                    row6.GetCell(9).SetCellValue("DUYỆT MẪU");

                    row7.GetCell(0).SetCellValue("Người thực hiện");
                    row7.GetCell(9).SetCellValue("Thông qua Phó Trưởng phòng phụ trách");

                    UtilsService.MergeCellRanges(sheet, mergeRow50, mergeRow54, mergeRow57, mergeRow55, mergeRow6, mergeRow71, mergeRow72);

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

        /////Truy Thu

        //API getworkSheet/QD1753

        [HttpGet("GetExcelTable1753TT/{RentFilesId}/{RentFileBCTId}")]
        public async Task<IActionResult> GetExcelTable1753TT(Guid RentFilesId, int RentFileBCTId)
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
                //Tìm hồ sơ cho thuê
                RentFile rentFile = _context.RentFiles.Where(l => l.Id == RentFilesId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

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

                List<Coefficient> coefficients = _context.Coefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList();

                List<Conversion> conversions = _context.Conversions.Where(l => l.TypeQD == AppEnums.TypeQD.QD_1753).ToList();

                List<DefaultCoefficient> defaultCoefficients = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                Block blocks = _context.Blocks.Where(l => l.Id == rentFile.RentBlockId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                List<RentingPrice> rentingPrices = _context.RentingPricies.Where(l => l.TypeQD == AppEnums.TypeQD.QD_1753 && l.TypeBlockId == blocks.TypeBlockId && l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.EffectiveDate).ToList();

                BlockData map_blocks = _mapper.Map<BlockData>(blocks);

                List<BlockDetail> blockDetails = _context.BlockDetails.Where(l => l.BlockId == rentFile.RentBlockId && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<BlockDetailData> map_BlockDetails = _mapper.Map<List<BlockDetailData>>(blockDetails);

                List<Floor> floorData = _context.Floors.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<Area> AreaData = _context.Areas.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<DiscountCoefficient> disData = _context.DiscountCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<DefaultCoefficient> dfData = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();

                foreach (BlockDetailData map_BlockDetail in map_BlockDetails)
                {
                    Floor floor = floorData.Where(f => f.Id == map_BlockDetail.FloorId).FirstOrDefault();
                    map_BlockDetail.FloorName = floor != null ? floor.Name : "";
                    map_BlockDetail.FloorCode = floor != null ? floor.Code : 0;

                    Area area = AreaData.Where(f => f.Id == map_BlockDetail.AreaId).FirstOrDefault();
                    map_BlockDetail.AreaName = area != null ? area.Name : "";
                    map_BlockDetail.IsMezzanine = area != null ? area.IsMezzanine : null;
                }
                map_blocks.blockDetails = map_BlockDetails.OrderBy(x => x.FloorCode).ThenBy(x => x.IsMezzanine).ToList();


                List<BCT> resBct = new List<BCT>();
                int oldID = 0;
                if (rentFile.RentApartmentId > 0)
                {
                    Apartment apartment = _context.Apartments.Where(l => l.Id == rentFile.RentApartmentId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    ApartmentData map_apartments = _mapper.Map<ApartmentData>(apartment);
                    List<ApartmentDetail> apartmentDetails = _context.ApartmentDetails.Where(a => a.TargetId == rentFile.RentApartmentId && a.Type == TypeApartmentDetail.APARTMENT && a.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                    List<ApartmentDetailData> apartmentDetailData = _mapper.Map<List<ApartmentDetailData>>(apartmentDetails);
                    foreach (ApartmentDetailData map_apartmentDetail in apartmentDetailData)
                    {
                        Area area = AreaData.Where(f => f.Id == map_apartmentDetail.AreaId).FirstOrDefault();
                        map_apartmentDetail.AreaName = area != null ? area.Name : "";
                        map_apartmentDetail.IsMezzanine = area != null ? area.IsMezzanine : null;

                        Floor floor = floorData.Where(f => f.Id == map_apartmentDetail.FloorId).FirstOrDefault();
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

                        bCT.DiscountCoff = (decimal)disData.Where(l => l.Id == rentFileBCT.DiscountCoefficientId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();
                        bCT.RentFileId = RentFilesId;
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
                                    if (oldID == itemChild.defaultCoefficientsId) continue;
                                    var content = dfData.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

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
                                        if (content.Content == name)
                                        {
                                            if (bCT.DateStart >= itemChild.DoApply)
                                            {
                                                oldID = itemChild.defaultCoefficientsId;

                                                var itemChilDfs = new chilDf();
                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                itemChilDfs.Value = itemChild.Value;
                                                bCT.chilDfs.Add(itemChilDfs);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                            itemChilDfs.Value = 0;
                                            bCT.chilDfs.Add(itemChilDfs);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (bCT.DateStart >= itemChild.DoApply)
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Value = itemChild.Value;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                            bCT.chilDfs.Add(itemChilDfs);
                                            break;
                                        }
                                        else
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Value = 0;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
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
                        //bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * (decimal)bCT.TotalK * (decimal)(1 + (bCT.VAT / 100))));
                        bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * (decimal)bCT.TotalK));
                        bCT.PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea));
                        bCT.PolicyReduction = Math.Round((decimal)(bCT.DiscountCoff * bCT.PriceRent));
                        bCT.PriceAfterDiscount = Math.Round((decimal)(bCT.PriceRent - bCT.PolicyReduction));
                        bCT.Unit = "VNĐ";
                        resBct.Add(bCT);

                        if (resChange.Count > 0)
                        {
                            foreach (var itemChange in resChange.OrderBy(p => p.DateChange).ToList())
                            {
                                k = k + 1;
                                decimal? PriceRent1m2 = 0;
                                decimal? PriceRent = 0;
                                decimal? PolicyReduction = 0;
                                decimal? PriceAfterDiscount = 0;

                                var changeData = new BCT();
                                changeData.TypeQD = 1753;
                                changeData.check = false;
                                changeData.GroupId = k;

                                changeData.AreaName = item.AreaName;

                                changeData.Level = (int)item.Level;

                                changeData.DiscountCoff = resBct[resBct.Count - 1].DiscountCoff;

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

                                    //PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * (decimal)changeData.TotalK * (decimal)(1 + (changeData.VAT / 100))));
                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));

                                    PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;
                                    changeData.PolicyReduction = (decimal)PolicyReduction;
                                    changeData.PriceAfterDiscount = (decimal)PriceAfterDiscount;

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

                        bCT.DateStart = rentFileBCT.DateStart;

                        bCT.DiscountCoff = (decimal)disData.Where(l => l.Id == rentFileBCT.DiscountCoefficientId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();
                        bCT.RentFileId = RentFilesId;

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
                                    if (oldID == itemChild.defaultCoefficientsId) continue;
                                    var content = dfData.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
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
                                        if (content.Content == name)
                                        {
                                            if (bCT.DateStart >= itemChild.DoApply)
                                            {
                                                oldID = itemChild.defaultCoefficientsId;
                                                var itemChilDfs = new chilDf();
                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                itemChilDfs.Value = itemChild.Value;
                                                bCT.chilDfs.Add(itemChilDfs);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                            itemChilDfs.Value = 0;
                                            bCT.chilDfs.Add(itemChilDfs);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (bCT.DateStart >= itemChild.DoApply)
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Value = itemChild.Value;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                            bCT.chilDfs.Add(itemChilDfs);
                                            break;
                                        }
                                        else
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Value = 0;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
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
                        //bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * (decimal)bCT.TotalK * (decimal)(1 + (bCT.VAT / 100))));
                        bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * (decimal)bCT.TotalK));
                        bCT.PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea));
                        bCT.PolicyReduction = Math.Round((decimal)(bCT.DiscountCoff * bCT.PriceRent));
                        bCT.PriceAfterDiscount = Math.Round((decimal)(bCT.PriceRent - bCT.PolicyReduction));
                        bCT.Unit = "VNĐ";
                        resBct.Add(bCT);

                        if (resChange.Count > 0)
                        {
                            foreach (var itemChange in resChange.OrderBy(p => p.DateChange).ToList())
                            {
                                k = k + 1;
                                decimal? PriceRent1m2 = 0;
                                decimal? PriceRent = 0;
                                decimal? PolicyReduction = 0;
                                decimal? PriceAfterDiscount = 0;

                                var changeData = new BCT();
                                changeData.TypeQD = 1753;
                                changeData.check = false;
                                changeData.GroupId = k;

                                changeData.AreaName = item.AreaName;

                                changeData.Level = (int)item.Level;

                                changeData.DiscountCoff = resBct[resBct.Count - 1].DiscountCoff;

                                changeData.DateCoefficient = item.DisposeTime;

                                changeData.PrivateArea = (float)rentFileBCT.Area;

                                changeData.VAT = resBct[resBct.Count - 1].VAT;
                                changeData.Ktdbt = resBct[resBct.Count - 1].Ktdbt;
                                changeData.Ktlcb = resBct[resBct.Count - 1].Ktlcb;
                                changeData.StandardPrice = resBct[resBct.Count - 1].StandardPrice;
                                changeData.Unit = resBct[resBct.Count - 1].Unit;
                                changeData.TotalK = resBct[resBct.Count - 1].TotalK;
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

                                    //PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * (decimal)changeData.TotalK * (decimal)(1 + (changeData.VAT / 100))));
                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));

                                    PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;
                                    changeData.PolicyReduction = (decimal)PolicyReduction;
                                    changeData.PriceAfterDiscount = (decimal)PriceAfterDiscount;

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
                    totalPrice = (decimal)f.ToList().Sum(p => p.PriceAfterDiscount),
                    bCTs = f.ToList(),
                    rentFileId = f.ToList().First().RentFileId
                });

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = groupData;
                return Ok(def);
            }
            catch (Exception ex)
            {
                return Ok(def);
            }
        }

        //API getworkSheet/QD09-TT
        [HttpGet("GetExcelTable09TT/{RentFilesId}/{RentFileBCTId}")]
        public async Task<IActionResult> GetExcelTable09TT(Guid RentFilesId, int RentFileBCTId)
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
                //Tìm hồ sơ cho thuê
                RentFile rentFile = _context.RentFiles.Where(l => l.Id == RentFilesId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

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

                List<Floor> floorData = _context.Floors.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<Area> AreaData = _context.Areas.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<DiscountCoefficient> disData = _context.DiscountCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<DefaultCoefficient> dfData = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();

                foreach (BlockDetailData map_BlockDetail in map_BlockDetails)
                {
                    Floor floor = floorData.Where(f => f.Id == map_BlockDetail.FloorId).FirstOrDefault();
                    map_BlockDetail.FloorName = floor != null ? floor.Name : "";
                    map_BlockDetail.FloorCode = floor != null ? floor.Code : 0;

                    Area area = AreaData.Where(f => f.Id == map_BlockDetail.AreaId).FirstOrDefault();
                    map_BlockDetail.AreaName = area != null ? area.Name : "";
                    map_BlockDetail.IsMezzanine = area != null ? area.IsMezzanine : null;
                }
                map_blocks.blockDetails = map_BlockDetails.OrderBy(x => x.FloorCode).ThenBy(x => x.IsMezzanine).ToList();


                List<BCT> resBct = new List<BCT>();
                int oldId = 0;
                if (rentFile.RentApartmentId > 0)
                {

                    Apartment apartment = _context.Apartments.Where(l => l.Id == rentFile.RentApartmentId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();
                    ApartmentData map_apartments = _mapper.Map<ApartmentData>(apartment);
                    List<ApartmentDetail> apartmentDetails = _context.ApartmentDetails.Where(a => a.TargetId == rentFile.RentApartmentId && a.Type == TypeApartmentDetail.APARTMENT && a.Status != AppEnums.EntityStatus.DELETED).OrderBy(x => x.UpdatedAt).ToList();
                    List<ApartmentDetailData> apartmentDetailData = _mapper.Map<List<ApartmentDetailData>>(apartmentDetails);
                    foreach (ApartmentDetailData map_apartmentDetail in apartmentDetailData)
                    {
                        Area area = AreaData.Where(f => f.Id == map_apartmentDetail.AreaId).FirstOrDefault();
                        map_apartmentDetail.AreaName = area != null ? area.Name : "";
                        map_apartmentDetail.IsMezzanine = area != null ? area.IsMezzanine : null;

                        Floor floor = floorData.Where(f => f.Id == map_apartmentDetail.FloorId).FirstOrDefault();
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

                        bCT.DiscountCoff = (decimal)disData.Where(l => l.Id == rentFileBCT.DiscountCoefficientId).Select(p => p.Value).FirstOrDefault();
                        bCT.RentFileId = RentFilesId;
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
                                    if (oldId == itemChild.defaultCoefficientsId) continue;
                                    var content = dfData.Where(l => l.Id == itemChild.defaultCoefficientsId).FirstOrDefault();
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

                                        if (content.Content == name)
                                        {
                                            if (bCT.DateStart >= itemChild.DoApply)
                                            {
                                                oldId = itemChild.defaultCoefficientsId;
                                                var itemChilDfs = new chilDf();
                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                itemChilDfs.Value = itemChild.Value;
                                                bCT.chilDfs.Add(itemChilDfs);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                            itemChilDfs.Value = 0;
                                            bCT.chilDfs.Add(itemChilDfs);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (bCT.DateStart >= itemChild.DoApply)
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Value = itemChild.Value;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                            bCT.chilDfs.Add(itemChilDfs);
                                            break;
                                        }
                                        else
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Value = 0;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
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
                        //bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * (decimal)bCT.TotalK * (decimal)(1 + (bCT.VAT / 100))));
                        bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * (decimal)bCT.TotalK));

                        bCT.PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea));
                        bCT.PolicyReduction = Math.Round((decimal)(bCT.DiscountCoff * bCT.PriceRent));
                        bCT.PriceAfterDiscount = Math.Round((decimal)(bCT.PriceRent - bCT.PolicyReduction));
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

                        bCT.DiscountCoff = (decimal)disData.Where(l => l.Id == rentFileBCT.DiscountCoefficientId).Select(p => p.Value).FirstOrDefault();
                        bCT.RentFileId = RentFilesId;

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
                                    if (oldId == itemChild.defaultCoefficientsId) continue;

                                    var content = dfData.Where(l => l.Id == itemChild.defaultCoefficientsId).FirstOrDefault();
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
                                        if (content.Content == name)
                                        {
                                            if (bCT.DateStart >= itemChild.DoApply)
                                            {
                                                oldId = itemChild.defaultCoefficientsId;
                                                var itemChilDfs = new chilDf();
                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                itemChilDfs.Value = itemChild.Value;
                                                bCT.chilDfs.Add(itemChilDfs);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                            itemChilDfs.Value = 0;
                                            bCT.chilDfs.Add(itemChilDfs);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (bCT.DateStart >= itemChild.DoApply)
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Value = itemChild.Value;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                            bCT.chilDfs.Add(itemChilDfs);
                                            break;
                                        }
                                        else
                                        {
                                            var itemChilDfs = new chilDf();
                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                            itemChilDfs.Value = 0;
                                            itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
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
                        //bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * (decimal)bCT.TotalK * (decimal)(1 + (bCT.VAT / 100))));
                        bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * (decimal)bCT.TotalK));

                        bCT.PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea));
                        bCT.PolicyReduction = Math.Round((decimal)(bCT.DiscountCoff * bCT.PriceRent));
                        bCT.PriceAfterDiscount = Math.Round((decimal)(bCT.PriceRent - bCT.PolicyReduction));
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
                    totalPrice = (decimal)f.ToList().Sum(p => p.PriceAfterDiscount),
                    bCTs = f.ToList(),
                    rentFileId = f.ToList().First().RentFileId
                });

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = groupData;
                return Ok(def);

            }
            catch (Exception ex)
            {
                return Ok(def);
            }
        }

        //API-QD22-BCT-TT
        [HttpGet("GetExcelTableBCT22TT/{RentFilesId}/{RentFileBCTId}")]
        public async Task<IActionResult> GetExcelTableBCT22TT(Guid RentFilesId, int RentFileBCTId)
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
                //Tìm hồ sơ cho thuê
                RentFile rentFile = _context.RentFiles.Where(l => l.Id == RentFilesId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

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

                List<Conversion> conversions = _context.Conversions.Where(l => l.TypeQD == AppEnums.TypeQD.QD_22).ToList();

                List<DefaultCoefficient> defaultCoefficients = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                Block blocks = _context.Blocks.Where(l => l.Id == rentFile.RentBlockId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                List<RentingPrice> rentingPrices = _context.RentingPricies.Where(l => l.TypeQD == AppEnums.TypeQD.QD_22 && l.TypeBlockId == blocks.TypeBlockId && l.Status != AppEnums.EntityStatus.DELETED).ToList();

                BlockData map_blocks = _mapper.Map<BlockData>(blocks);

                List<BlockDetail> blockDetails = _context.BlockDetails.Where(l => l.BlockId == rentFile.RentBlockId && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<BlockDetailData> map_BlockDetails = _mapper.Map<List<BlockDetailData>>(blockDetails);

                List<Floor> floorData = _context.Floors.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<Area> AreaData = _context.Areas.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<DiscountCoefficient> disData = _context.DiscountCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();
                List<DefaultCoefficient> dfData = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();


                foreach (BlockDetailData map_BlockDetail in map_BlockDetails)
                {
                    Floor floor = floorData.Where(f => f.Id == map_BlockDetail.FloorId).FirstOrDefault();
                    map_BlockDetail.FloorName = floor != null ? floor.Name : "";
                    map_BlockDetail.FloorCode = floor != null ? floor.Code : 0;

                    Area area = AreaData.Where(f => f.Id == map_BlockDetail.AreaId).FirstOrDefault();
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
                        Area area = AreaData.Where(f => f.Id == map_apartmentDetail.AreaId).FirstOrDefault();
                        map_apartmentDetail.AreaName = area != null ? area.Name : "";
                        map_apartmentDetail.IsMezzanine = area != null ? area.IsMezzanine : null;

                        Floor floor = floorData.Where(f => f.Id == map_apartmentDetail.FloorId).FirstOrDefault();
                        map_apartmentDetail.FloorName = floor != null ? floor.Name : "";
                        map_apartmentDetail.FloorCode = floor != null ? floor.Code : 0;
                    }
                    map_apartments.apartmentDetailData = apartmentDetailData.ToList();
                    foreach (var item in map_apartments.apartmentDetailData)
                    {
                        List<int> oldId = new List<int>();


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

                        bCT.DiscountCoff = (decimal)disData.Where(l => l.Id == rentFileBCT.DiscountCoefficientId).Select(p => p.Value).FirstOrDefault();

                        bCT.RentFileId = RentFilesId;
                        if (rentFileBCT.DateStart < dateStart)
                        {
                            bCT.DateStart = dateStart;
                        }
                        else
                        {
                            bCT.DateStart = rentFileBCT.DateStart;
                        }

                        bCT.DateEnd = rentFileBCT.DateEnd;

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
                            if (bCT.DateCoefficient >= coefficient.DoApply)
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
                                    var listCount = itemDf.childDefaultCoefficients.Count;

                                    foreach (var itemChild in itemDf.childDefaultCoefficients)
                                    {
                                        Boolean flagCheck = false;
                                        foreach (var oldid in oldId)
                                        {
                                            if (oldid == itemChild.defaultCoefficientsId)
                                            {
                                                flagCheck = true;
                                                break;
                                            }
                                        }
                                        if (flagCheck == true)
                                        {
                                            continue;
                                        }

                                        var content = dfData.Where(l => l.Id == itemChild.defaultCoefficientsId).FirstOrDefault();
                                        if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                        {
                                            if (content.Content == bCT.AreaName)
                                            {
                                                if (bCT.DateStart >= itemChild.DoApply)
                                                {
                                                    int oldid = itemChild.defaultCoefficientsId;
                                                    oldId.Add(oldid);

                                                    var itemChilDfs = new chilDf();
                                                    itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
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
                                                else if (bCT.DateStart < itemChild.DoApply && bCT.DateEnd < itemChild.DoApply)
                                                {
                                                    var itemChilDfs = new chilDf();
                                                    itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                    itemChilDfs.Value = 0;
                                                    bCT.chilDfs.Add(itemChilDfs);
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                var itemChilDfs = new chilDf();
                                                itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                itemChilDfs.Value = 0;
                                                bCT.chilDfs.Add(itemChilDfs);

                                                var changeK = new changeK();

                                                changeK.valueChange = 0;
                                                changeK.DateChange = itemChild.DoApply;
                                                resChangeK.Add(changeK);
                                            }
                                        }
                                        else
                                        {
                                            if (bCT.DateStart >= itemChild.DoApply)
                                            {
                                                var itemChilDfs = new chilDf();
                                                itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
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
                                            else if (bCT.DateStart < itemChild.DoApply && listCount > 1)
                                            {
                                                listCount--;
                                                continue;
                                            }
                                            else if (bCT.DateStart < itemChild.DoApply && listCount == 1)
                                            {
                                                var itemChilDfs = new chilDf();
                                                itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                itemChilDfs.Value = 0;
                                                bCT.chilDfs.Add(itemChilDfs);
                                                break;
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

                        //bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * bCT.Ktlcb * bCT.Ktdbt * (decimal)bCT.TotalK * (decimal)(1 + (bCT.VAT / 100))));
                        bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * bCT.Ktlcb * bCT.Ktdbt * (decimal)bCT.TotalK));

                        bCT.PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea));
                        bCT.PolicyReduction = Math.Round((decimal)(bCT.DiscountCoff * bCT.PriceRent));
                        bCT.PriceAfterDiscount = Math.Round((decimal)(bCT.PriceRent - bCT.PolicyReduction));
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
                                decimal? PolicyReduction = 0;
                                decimal? PriceAfterDiscount = 0;

                                var changeData = new BCT();
                                List<int> lstId = new List<int>();
                                changeData.TypeQD = 22;
                                changeData.check = false;
                                changeData.GroupId = k;

                                changeData.AreaName = item.AreaName;

                                changeData.Level = (int)item.Level;

                                changeData.DateCoefficient = item.DisposeTime;

                                changeData.PrivateArea = (float)rentFileBCT.Area;

                                changeData.DiscountCoff = resBct[resBct.Count - 1].DiscountCoff;

                                changeData.VAT = resBct[resBct.Count - 1].VAT;
                                changeData.Ktdbt = resBct[resBct.Count - 1].Ktdbt;
                                changeData.Ktlcb = resBct[resBct.Count - 1].Ktlcb;
                                changeData.StandardPrice = resBct[resBct.Count - 1].StandardPrice;
                                changeData.Unit = resBct[resBct.Count - 1].Unit;
                                changeData.TotalK = resBct[resBct.Count - 1].TotalK;
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


                                    //PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK * (decimal)(1 + (changeData.VAT / 100))));
                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));
                                    PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;
                                    changeData.PolicyReduction = (decimal)PolicyReduction;
                                    changeData.PriceAfterDiscount = PriceAfterDiscount;

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

                                    //PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK * (decimal)(1 + (changeData.VAT / 100))));
                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));
                                    PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

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
                                    changeData.PolicyReduction = (decimal)PolicyReduction;
                                    changeData.PriceAfterDiscount = PriceAfterDiscount;
                                    resBct.Add(changeData);

                                }
                                else if (itemChange.Type == 3)
                                {
                                    changeData.Ktdbt = (decimal)itemChange.valueChange;
                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    //PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK * (decimal)(1 + (changeData.VAT / 100))));
                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));


                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));
                                    PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;
                                    changeData.PolicyReduction = (decimal)PolicyReduction;
                                    changeData.PriceAfterDiscount = PriceAfterDiscount;

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
                                                    Boolean flagCheck = false;
                                                    foreach (var oldid in lstId)
                                                    {
                                                        if (oldid == itemChild.defaultCoefficientsId)
                                                        {
                                                            flagCheck = true;
                                                            break;
                                                        }
                                                    }
                                                    if (flagCheck == true)
                                                    {
                                                        continue;
                                                    }
                                                    var content = dfData.Where(l => l.Id == itemChild.defaultCoefficientsId).FirstOrDefault();

                                                    var itemChilDfs = new chilDf();

                                                    if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                                    {
                                                        if (content.Content == changeData.AreaName)
                                                        {
                                                            if (changeData.DateStart >= itemChild.DoApply)
                                                            {
                                                                int idold = itemChild.defaultCoefficientsId;
                                                                lstId.Add(idold);

                                                                itemChilDfs.Value = itemChild.Value;
                                                                itemChilDfs.CoefficientId = content.CoefficientName;
                                                                changeData.chilDfs.Add(itemChilDfs);
                                                                break;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            itemChilDfs.Value = 0;
                                                            changeData.chilDfs.Add(itemChilDfs);
                                                            break;
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
                                                        else
                                                        {
                                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                            itemChilDfs.Value = 0;
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

                                    //PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK * (decimal)(1 + (changeData.VAT / 100))));
                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));


                                    PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));
                                    PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;
                                    changeData.PolicyReduction = (decimal)PolicyReduction;
                                    changeData.PriceAfterDiscount = PriceAfterDiscount;

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

                                                    Boolean flagCheck = false;
                                                    foreach (var oldid in lstId)
                                                    {
                                                        if (oldid == itemChild.defaultCoefficientsId)
                                                        {
                                                            flagCheck = true;
                                                            break;
                                                        }
                                                    }
                                                    if (flagCheck == true)
                                                    {
                                                        continue;
                                                    }
                                                    var content = dfData.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();



                                                    if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                                    {
                                                        if (content.Content == changeData.AreaName)
                                                        {
                                                            if (changeData.DateStart >= itemChild.DoApply)
                                                            {
                                                                int idold = itemChild.defaultCoefficientsId;
                                                                lstId.Add(idold);

                                                                itemChilDfs.Value = itemChild.Value;
                                                                itemChilDfs.CoefficientId = content.CoefficientName;
                                                                changeData.chilDfs.Add(itemChilDfs);
                                                                break;
                                                            }
                                                            else
                                                            {
                                                                itemChilDfs.Value = 0;
                                                                changeData.chilDfs.Add(itemChilDfs);
                                                                break;
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
                                                            else
                                                            {
                                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                                itemChilDfs.Value = 0;
                                                                changeData.chilDfs.Add(itemChilDfs);
                                                                break;
                                                            }

                                                        }
                                                    }
                                                }
                                            }


                                        }

                                        //PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK * (decimal)(1 + (changeData.VAT / 100))));
                                        PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                        PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));
                                        PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                        changeData.PriceRent1m2 = PriceRent1m2;
                                        changeData.PriceRent = PriceRent;
                                        changeData.PolicyReduction = (decimal)PolicyReduction;
                                        changeData.PriceAfterDiscount = PriceAfterDiscount;

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
                }
                else
                {
                    foreach (var item in map_blocks.blockDetails)
                    {
                        int k = 0;
                        var bCTBL = new BCT();
                        List<changeValue> resChange = new List<changeValue>();
                        List<changeK> resChangeK = new List<changeK>();
                        List<int> listId = new List<int>();
                        bCTBL.TypeQD = 22;
                        bCTBL.check = false;
                        bCTBL.GroupId = k;

                        bCTBL.AreaName = item.AreaName;

                        bCTBL.Level = (int)item.Level;

                        bCTBL.PrivateArea = (float)rentFileBCT.Area;

                        bCTBL.DiscountCoff = (decimal)disData.Where(l => l.Id == rentFileBCT.DiscountCoefficientId).Select(p => p.Value).FirstOrDefault();

                        bCTBL.RentFileId = RentFilesId;
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
                            if (bCTBL.DateCoefficient >= coefficient.DoApply)
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
                                        Boolean flagCheck = false;
                                        foreach (var oldid in listId)
                                        {
                                            if (oldid == itemChild.defaultCoefficientsId)
                                            {
                                                flagCheck = true;
                                                break;
                                            }
                                        }
                                        if (flagCheck == true)
                                        {
                                            continue;
                                        }
                                        var content = dfData.Where(l => l.Id == itemChild.defaultCoefficientsId).FirstOrDefault();

                                        if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                        {
                                            if (content.Content == bCTBL.AreaName)
                                            {
                                                if (bCTBL.DateStart >= itemChild.DoApply)
                                                {
                                                    int oldId = itemChild.defaultCoefficientsId;
                                                    listId.Add(oldId);

                                                    var itemChilDfs = new chilDf();
                                                    itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
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
                                            else
                                            {
                                                if (bCTBL.DateStart >= itemChild.DoApply)
                                                {
                                                    var itemChilDfs = new chilDf();
                                                    itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                    itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                    itemChilDfs.Value = 0;
                                                    bCTBL.chilDfs.Add(itemChilDfs);
                                                    break;
                                                }
                                                else if (bCTBL.DateEnd >= itemChild.DoApply)
                                                {
                                                    var changeK = new changeK();

                                                    changeK.valueChange = 0;
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
                                                itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
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
                                            else if (bCTBL.DateStart < itemChild.DoApply)
                                            {
                                                var itemChilDfs = new chilDf();
                                                itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                itemChilDfs.Value = 0;
                                                bCTBL.chilDfs.Add(itemChilDfs);
                                                break;
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

                        //bCTBL.PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK * (decimal)(1 + (bCTBL.VAT / 100))));
                        bCTBL.PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK));

                        bCTBL.PriceRent = Math.Round((decimal)(bCTBL.PriceRent1m2 * (decimal)bCTBL.PrivateArea));
                        bCTBL.PolicyReduction = Math.Round((decimal)(bCTBL.DiscountCoff * bCTBL.PriceRent));
                        bCTBL.PriceAfterDiscount = Math.Round((decimal)(bCTBL.PriceRent - bCTBL.PolicyReduction));

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
                                decimal? PolicyReduction = 0;
                                decimal? PriceAfterDiscount = 0;
                                List<int> lstId = new List<int>();
                                var changeData = new BCT();

                                changeData.TypeQD = 22;
                                changeData.check = false;
                                changeData.GroupId = k;

                                changeData.AreaName = item.AreaName;

                                changeData.Level = (int)item.Level;

                                changeData.DateCoefficient = item.DisposeTime;

                                changeData.PrivateArea = (float)rentFileBCT.Area;

                                changeData.DiscountCoff = resBct[resBct.Count - 1].DiscountCoff;

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


                                    //PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK * (decimal)(1 + (changeData.VAT / 100))));
                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));
                                    PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;
                                    changeData.PolicyReduction = (decimal)PolicyReduction;
                                    changeData.PriceAfterDiscount = PriceAfterDiscount;

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

                                    //PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK * (decimal)(1 + (changeData.VAT / 100))));
                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));
                                    PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

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
                                    changeData.PolicyReduction = (decimal)PolicyReduction;
                                    changeData.PriceAfterDiscount = PriceAfterDiscount;
                                    resBct.Add(changeData);

                                }
                                else if (itemChange.Type == 3)
                                {
                                    changeData.Ktdbt = (decimal)itemChange.valueChange;
                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    //PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK * (decimal)(1 + (changeData.VAT / 100))));
                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));
                                    PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;
                                    changeData.PolicyReduction = (decimal)PolicyReduction;
                                    changeData.PriceAfterDiscount = PriceAfterDiscount;

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
                                                    Boolean flagCheck = false;
                                                    var itemChilDfs = new chilDf();

                                                    foreach (var oldid in listId)
                                                    {
                                                        if (oldid == itemChild.defaultCoefficientsId)
                                                        {
                                                            flagCheck = true;
                                                            break;
                                                        }
                                                    }
                                                    if (flagCheck == true)
                                                    {
                                                        continue;
                                                    }
                                                    var content = dfData.Where(l => l.Id == itemChild.defaultCoefficientsId).FirstOrDefault();


                                                    if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                                    {
                                                        itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                        if (content.Content == changeData.AreaName)
                                                        {
                                                            if (changeData.DateStart >= itemChild.DoApply)
                                                            {
                                                                int oldId = itemChild.defaultCoefficientsId;
                                                                listId.Add(oldId);

                                                                itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                                itemChilDfs.Value = itemChild.Value;
                                                                bCTBL.chilDfs.Add(itemChilDfs);
                                                                break;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            itemChilDfs.Value = 0;
                                                            changeData.chilDfs.Add(itemChilDfs);
                                                            break;
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
                                                        else
                                                        {
                                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                            itemChilDfs.Value = 0;
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

                                    //PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK * (decimal)(1 + (changeData.VAT / 100))));
                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));


                                    PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));
                                    PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;
                                    changeData.PolicyReduction = (decimal)PolicyReduction;
                                    changeData.PriceAfterDiscount = PriceAfterDiscount;

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
                                                    var content = dfData.Where(l => l.Id == itemChild.defaultCoefficientsId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                                                    Boolean flagCheck = false;
                                                    foreach (var oldid in listId)
                                                    {
                                                        if (oldid == itemChild.defaultCoefficientsId)
                                                        {
                                                            flagCheck = true;
                                                            break;
                                                        }
                                                    }
                                                    if (flagCheck == true)
                                                    {
                                                        continue;
                                                    }

                                                    if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                                    {

                                                        if (content.Content == changeData.AreaName)
                                                        {
                                                            if (changeData.DateStart >= itemChild.DoApply)
                                                            {
                                                                int oldId = itemChild.defaultCoefficientsId;
                                                                listId.Add(oldId);

                                                                itemChilDfs.Code = conversions.Where(p => p.CoefficientName == content.CoefficientName && p.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Code).FirstOrDefault();
                                                                itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                                itemChilDfs.Value = itemChild.Value;
                                                                bCTBL.chilDfs.Add(itemChilDfs);
                                                                break;
                                                            }

                                                        }
                                                        else
                                                        {
                                                            itemChilDfs.Value = 0;
                                                            changeData.chilDfs.Add(itemChilDfs);
                                                            break;
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
                                                        else
                                                        {
                                                            itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                                            itemChilDfs.Value = 0;
                                                            changeData.chilDfs.Add(itemChilDfs);
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }


                                    }

                                    //PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK * (decimal)(1 + (changeData.VAT / 100))));
                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK));

                                    PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));
                                    PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;
                                    changeData.PolicyReduction = (decimal)PolicyReduction;
                                    changeData.PriceAfterDiscount = PriceAfterDiscount;

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
                    totalPrice = (decimal)f.ToList().Sum(p => p.PriceAfterDiscount),
                    bCTs = f.ToList(),
                    rentFileId = f.ToList().First().RentFileId
                });

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = groupData;
                return Ok(def);
            }
            catch (Exception ex)
            {
                return Ok(def);
            }
        }

        ////Bảng chiết tính người sử dụng

        //API getWorkSheet/QD1753-User

        [HttpGet("GetExcelTable1753Us/{RentFilesId}/{RentFileBCTId}")]
        public async Task<IActionResult> GetExcelTable1753Us(Guid RentFilesId, int RentFileBCTId)
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
                //Tìm hồ sơ cho thuê
                RentFile rentFile = _context.RentFiles.Where(l => l.Id == RentFilesId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                RentFlieData map_rentFlieData = _mapper.Map<RentFlieData>(rentFile);

                //Tìm bảng chiết tính mới nhất theo ID hồ sơ
                RentFileBCT rentFileBCT = _context.RentFileBCTs.Where(l => l.Id == RentFileBCTId && l.TypeBCT == 3).OrderByDescending(p => p.CreatedAt).FirstOrDefault();

                RentFileBCTData map_rentFlieBCTDatas = _mapper.Map<RentFileBCTData>(rentFileBCT);

                List<DefaultCoefficient> dataDf = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderBy(p => p.CoefficientName).ToList();
                var groupedData = dataDf.GroupBy(x => x.CoefficientName).Select(e => new GroupData
                {
                    CoefficientId = e.Key,
                    defaultCoefficients = e.ToList(),
                    childDefaultCoefficients = _context.ChildDefaults.Where(l => l.RentFileBctId == map_rentFlieBCTDatas.Id && l.CoefficientId == e.Key && l.Type == 3 && l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList()
                }).ToList();
                map_rentFlieBCTDatas.groupData = groupedData;

                List<Vat> vats = _context.Vats.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList();

                List<Salary> salaryCoefficients = _context.Salaries.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList();

                List<Coefficient> coefficients = _context.Coefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList();

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
                        List<changeValue> resChange = new List<changeValue>();
                        List<changeK> resChangeK = new List<changeK>();

                        var bCT = new BCT();

                        bCT.GroupId = 1753;

                        bCT.AreaName = item.AreaName;

                        bCT.Level = (int)item.Level;

                        bCT.PrivateArea = item.PrivateArea;

                        bCT.DateCoefficient = (DateTime)item.DisposeTime;

                        bCT.DateStart = rentFileBCT.DateStart;

                        bCT.DateEnd = rentFileBCT.DateEnd;

                        bCT.DiscountCoff = (decimal)_context.DiscountCoefficients.Where(l => l.Id == rentFileBCT.DiscountCoefficientId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();


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
                        bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * (decimal)bCT.TotalK * (decimal)(1 + (bCT.VAT / 100))));
                        bCT.PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea));
                        bCT.PolicyReduction = Math.Round((decimal)(bCT.DiscountCoff * bCT.PriceRent));
                        bCT.PriceAfterDiscount = Math.Round((decimal)(bCT.PriceRent - bCT.PolicyReduction));
                        bCT.Unit = "VNĐ";
                        resBct.Add(bCT);


                        if (resChange.Count > 0)
                        {
                            foreach (var itemChange in resChange.OrderBy(p => p.DateChange).ToList())
                            {
                                int k = 0;

                                k = k + 1;
                                decimal? PriceRent1m2 = 0;
                                decimal? PriceRent = 0;
                                decimal? PolicyReduction = 0;
                                decimal? PriceAfterDiscount = 0;

                                var changeData = new BCT();
                                changeData.TypeQD = 1753;
                                changeData.check = false;
                                changeData.GroupId = k;

                                changeData.AreaName = item.AreaName;

                                changeData.Level = (int)item.Level;

                                changeData.DateCoefficient = item.DisposeTime;

                                changeData.PrivateArea = (float)rentFileBCT.Area;

                                changeData.DiscountCoff = resBct[resBct.Count - 1].DiscountCoff;

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

                                    PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));

                                    PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;
                                    changeData.PolicyReduction = (decimal)PolicyReduction;
                                    changeData.PriceAfterDiscount = PriceAfterDiscount;

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
                        List<changeValue> resChange = new List<changeValue>();
                        List<changeK> resChangeK = new List<changeK>();

                        var bCT = new BCT();
                        bCT.GroupId = 1753;

                        bCT.AreaName = item.AreaName;

                        bCT.Level = (int)item.Level;

                        bCT.PrivateArea = item.PrivateArea;
                        bCT.DiscountCoff = (decimal)_context.DiscountCoefficients.Where(l => l.Id == rentFileBCT.DiscountCoefficientId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();


                        bCT.DateCoefficient = (DateTime)item.DisposeTime;

                        bCT.DateStart = rentFileBCT.DateStart;

                        bCT.DateEnd = rentFileBCT.DateEnd;

                        foreach (var itemLv in rentingPrices) //Lấy giá chuẩn
                        {
                            if (bCT.DateCoefficient >= itemLv.EffectiveDate && itemLv.LevelId == item.Level)
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
                        bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * (decimal)bCT.TotalK * (decimal)(1 + (bCT.VAT / 100))));
                        bCT.PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea));
                        bCT.PolicyReduction = Math.Round((decimal)(bCT.DiscountCoff * bCT.PriceRent));
                        bCT.PriceAfterDiscount = Math.Round((decimal)(bCT.PriceRent - bCT.PolicyReduction));
                        bCT.Unit = "VNĐ";
                        resBct.Add(bCT);

                        if (resChange.Count > 0)
                        {
                            foreach (var itemChange in resChange.OrderBy(p => p.DateChange).ToList())
                            {
                                int k = 0;

                                k = k + 1;
                                decimal? PriceRent1m2 = 0;
                                decimal? PriceRent = 0;
                                decimal? PolicyReduction = 0;
                                decimal? PriceAfterDiscount = 0;

                                var changeData = new BCT();
                                changeData.TypeQD = 1753;
                                changeData.check = false;
                                changeData.GroupId = k;

                                changeData.AreaName = item.AreaName;

                                changeData.Level = (int)item.Level;

                                changeData.DateCoefficient = item.DisposeTime;

                                changeData.PrivateArea = (float)rentFileBCT.Area;

                                changeData.DiscountCoff = resBct[resBct.Count - 1].DiscountCoff;

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

                                    PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));

                                    PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;
                                    changeData.PolicyReduction = (decimal)PolicyReduction;
                                    changeData.PriceAfterDiscount = PriceAfterDiscount;

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
                    totalPrice = (decimal)f.ToList().Sum(p => p.PriceAfterDiscount),
                    bCTs = f.ToList()
                });

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = groupData;
                return Ok(def);

            }
            catch (Exception ex)
            {
                return Ok(def);
            }
        }

        //API getworkSheet/QD09-User
        [HttpGet("GetExcelTable09Us/{RentFilesId}/{RentFileBCTId}")]
        public async Task<IActionResult> GetExcelTable09Us(Guid RentFilesId, int RentFileBCTId)
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
                //Tìm hồ sơ cho thuê
                RentFile rentFile = _context.RentFiles.Where(l => l.Id == RentFilesId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                RentFlieData map_rentFlieData = _mapper.Map<RentFlieData>(rentFile);

                //Tìm bảng chiết tính mới nhất theo ID hồ sơ
                RentFileBCT rentFileBCT = _context.RentFileBCTs.Where(l => l.Id == RentFileBCTId && l.TypeBCT == 3 && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                RentFileBCTData map_rentFlieBCTDatas = _mapper.Map<RentFileBCTData>(rentFileBCT);

                List<DefaultCoefficient> dataDf = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderBy(p => p.CoefficientName).ToList();
                var groupedData = dataDf.GroupBy(x => x.CoefficientName).Select(e => new GroupData
                {
                    CoefficientId = e.Key,
                    defaultCoefficients = e.ToList(),
                    childDefaultCoefficients = _context.ChildDefaults.Where(l => l.RentFileBctId == map_rentFlieBCTDatas.Id && l.CoefficientId == e.Key && l.Type == 3 && l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList()
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

                        bCT.AreaName = item.AreaName;

                        bCT.Level = (int)item.Level;

                        bCT.GroupId = 9;

                        bCT.check = false;

                        bCT.PrivateArea = item.PrivateArea;
                        bCT.DiscountCoff = (decimal)_context.DiscountCoefficients.Where(l => l.Id == rentFileBCT.DiscountCoefficientId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();


                        bCT.DateStart = rentFileBCT.DateStart;

                        bCT.DateEnd = rentFileBCT.DateEnd;

                        bCT.DateCoefficient = (DateTime)item.DisposeTime;

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
                                    var itemChilDfs = new chilDf();

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
                                        itemChilDfs.CoefficientId = itemChild.CoefficientId;
                                        if (content == name)
                                        {
                                            itemChilDfs.Value = itemChild.Value;
                                            bCT.chilDfs.Add(itemChilDfs);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (bCT.DateStart >= itemChild.DoApply)
                                        {
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
                        bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * (decimal)bCT.TotalK * (decimal)(1 + (bCT.VAT / 100))));
                        bCT.PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea));
                        bCT.PolicyReduction = Math.Round((decimal)(bCT.DiscountCoff * bCT.PriceRent));
                        bCT.PriceAfterDiscount = Math.Round((decimal)(bCT.PriceRent - bCT.PolicyReduction));
                        bCT.Unit = "VNĐ";
                        resBct.Add(bCT);
                    }
                }
                else
                {
                    foreach (var item in map_blocks.blockDetails)
                    {
                        var bCT = new BCT();

                        bCT.GroupId = 9;

                        bCT.AreaName = item.AreaName;

                        bCT.Level = (int)item.Level;

                        bCT.check = false;
                        bCT.PrivateArea = item.PrivateArea;

                        bCT.DateStart = rentFileBCT.DateStart;

                        bCT.DateEnd = rentFileBCT.DateEnd;

                        bCT.DateCoefficient = (DateTime)item.DisposeTime;

                        bCT.DiscountCoff = (decimal)_context.DiscountCoefficients.Where(l => l.Id == rentFileBCT.DiscountCoefficientId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();

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
                                    string name = "";
                                    if (item.FloorCode < 6)
                                    {
                                        name = bCT.AreaName;
                                    }
                                    else
                                    {
                                        name = "Tầng 6 trở lên";
                                    }
                                    if (itemChild.CoefficientId == AppEnums.TypeTable.HSTC)
                                    {
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
                        bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * (decimal)bCT.TotalK * (decimal)(1 + (bCT.VAT / 100))));
                        bCT.PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea));
                        bCT.PolicyReduction = Math.Round((decimal)(bCT.DiscountCoff * bCT.PriceRent));
                        bCT.PriceAfterDiscount = Math.Round((decimal)(bCT.PriceRent - bCT.PolicyReduction));
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
                    totalPrice = (decimal)f.ToList().Sum(p => p.PriceAfterDiscount),
                    bCTs = f.ToList()
                });

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = groupData;
                return Ok(def);

            }
            catch (Exception ex)
            {
                return Ok(def);
            }
        }

        //API-QD22-BCT-User
        [HttpGet("GetExcelTableBCT22Us/{RentFilesId}/{RentFileBCTId}")]
        public async Task<IActionResult> GetExcelTableBCT22Us(Guid RentFilesId, int RentFileBCTId)
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
                //Tìm hồ sơ cho thuê
                RentFile rentFile = _context.RentFiles.Where(l => l.Id == RentFilesId && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                RentFlieData map_rentFlieData = _mapper.Map<RentFlieData>(rentFile);

                //Tìm bảng chiết tính mới nhất theo ID hồ sơ
                RentFileBCT rentFileBCT = _context.RentFileBCTs.Where(l => l.Id == RentFileBCTId && l.TypeBCT == 3 && l.Status != AppEnums.EntityStatus.DELETED).FirstOrDefault();

                RentFileBCTData map_rentFlieBCTDatas = _mapper.Map<RentFileBCTData>(rentFileBCT);

                List<DefaultCoefficient> dataDf = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderBy(p => p.CoefficientName).ToList();
                var groupedData = dataDf.GroupBy(x => x.CoefficientName).Select(e => new GroupData
                {
                    CoefficientId = e.Key,
                    defaultCoefficients = e.ToList(),
                    childDefaultCoefficients = _context.ChildDefaults.Where(l => l.RentFileBctId == map_rentFlieBCTDatas.Id && l.CoefficientId == e.Key && l.Type == 3 && l.Status != AppEnums.EntityStatus.DELETED).OrderByDescending(p => p.DoApply).ToList()
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

                        bCT.AreaName = item.AreaName;

                        bCT.PrivateArea = (float)rentFileBCT.Area;

                        bCT.DateStart = rentFileBCT.DateStart;

                        bCT.DateEnd = rentFileBCT.DateEnd;

                        bCT.DiscountCoff = (decimal)_context.DiscountCoefficients.Where(l => l.Id == rentFileBCT.DiscountCoefficientId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();

                        bCT.DateCoefficient = (DateTime)item.DisposeTime;

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

                                                    itemChilDfs.CoefficientId = itemChild.CoefficientId;

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

                        bCT.PriceRent1m2 = Math.Round((decimal)(bCT.StandardPrice * bCT.Ktlcb * bCT.Ktdbt * (decimal)bCT.TotalK * (decimal)(1 + (bCT.VAT / 100))));

                        bCT.PriceRent = Math.Round((decimal)(bCT.PriceRent1m2 * (decimal)bCT.PrivateArea));
                        bCT.PolicyReduction = Math.Round((decimal)(bCT.DiscountCoff * bCT.PriceRent));
                        bCT.PriceAfterDiscount = Math.Round((decimal)(bCT.PriceRent - bCT.PolicyReduction));
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
                                decimal? PolicyReduction = 0;
                                decimal? PriceAfterDiscount = 0;

                                var changeData = new BCT();

                                changeData.check = false;
                                changeData.GroupId = k;

                                changeData.AreaName = item.AreaName;

                                changeData.Level = (int)item.Level;

                                changeData.DateCoefficient = item.DisposeTime;

                                changeData.DiscountCoff = resBct[resBct.Count - 1].DiscountCoff;

                                changeData.PrivateArea = resBct[resBct.Count - 1].PrivateArea;

                                changeData.VAT = resBct[resBct.Count - 1].VAT;
                                changeData.Ktdbt = resBct[resBct.Count - 1].Ktdbt;
                                changeData.Ktlcb = resBct[resBct.Count - 1].Ktlcb;
                                changeData.StandardPrice = resBct[resBct.Count - 1].StandardPrice;
                                changeData.Unit = resBct[resBct.Count - 1].Unit;
                                changeData.PolicyReduction = resBct[resBct.Count - 1].PolicyReduction;
                                changeData.TotalK = resBct[resBct.Count - 1].TotalK;
                                if (itemChange.Type == 1)
                                {
                                    changeData.VAT = itemChange.valueChange;
                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);
                                    }


                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK * (decimal)(1 + (changeData.VAT / 100))));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));
                                    PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;
                                    changeData.PolicyReduction = (decimal)PolicyReduction;
                                    changeData.PriceAfterDiscount = PriceAfterDiscount;
                                    resBct.Add(changeData);
                                }
                                else if (itemChange.Type == 2)
                                {
                                    changeData.Ktlcb = (decimal)itemChange.valueChange;
                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK * (decimal)(1 + (changeData.VAT / 100))));


                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));
                                    PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;
                                    changeData.PolicyReduction = (decimal)PolicyReduction;
                                    changeData.PriceAfterDiscount = PriceAfterDiscount;

                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);
                                    }

                                    resBct.Add(changeData);

                                }
                                else if (itemChange.Type == 3)
                                {
                                    changeData.Ktdbt = (decimal)itemChange.valueChange;
                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK * (decimal)(1 + (changeData.VAT / 100))));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));
                                    PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;
                                    changeData.PolicyReduction = (decimal)PolicyReduction;
                                    changeData.PriceAfterDiscount = PriceAfterDiscount;

                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);
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

                                    resBct.Add(changeData);
                                }
                                else if (itemChange.Type == 4)
                                {
                                    changeData.PolicyReduction = (decimal)itemChange.valueChange;

                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK * (decimal)(1 + (changeData.VAT / 100))));


                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));
                                    PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;
                                    changeData.PolicyReduction = (decimal)PolicyReduction;
                                    changeData.PriceAfterDiscount = PriceAfterDiscount;

                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);
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

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK * (decimal)(1 + (changeData.VAT / 100))));


                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));
                                    PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;
                                    changeData.PolicyReduction = (decimal)PolicyReduction;
                                    changeData.PriceAfterDiscount = PriceAfterDiscount;

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

                        bCTBL.check = false;
                        bCTBL.GroupId = k;

                        bCTBL.AreaName = item.AreaName;

                        bCTBL.Level = (int)item.Level;

                        bCTBL.PrivateArea = item.PrivateArea;

                        bCTBL.DateStart = rentFileBCT.DateStart;

                        bCTBL.DateEnd = rentFileBCT.DateEnd;

                        bCTBL.DiscountCoff = (decimal)_context.DiscountCoefficients.Where(l => l.Id == rentFileBCT.DiscountCoefficientId && l.Status != AppEnums.EntityStatus.DELETED).Select(p => p.Value).FirstOrDefault();


                        bCTBL.DateCoefficient = (DateTime)item.DisposeTime;

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

                        bCTBL.PriceRent1m2 = Math.Round((decimal)(bCTBL.StandardPrice * bCTBL.Ktlcb * bCTBL.Ktdbt * (decimal)bCTBL.TotalK * (decimal)(1 + (bCTBL.VAT / 100))));

                        bCTBL.PriceRent = Math.Round((decimal)(bCTBL.PriceRent1m2 * (decimal)bCTBL.PrivateArea));
                        bCTBL.PolicyReduction = Math.Round((decimal)(bCTBL.DiscountCoff * bCTBL.PriceRent));
                        bCTBL.PriceAfterDiscount = Math.Round((decimal)(bCTBL.PriceRent - bCTBL.PolicyReduction));

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
                                decimal? PolicyReduction = 0;
                                decimal? PriceAfterDiscount = 0;

                                var changeData = new BCT();

                                changeData.check = false;
                                changeData.GroupId = k;

                                changeData.AreaName = item.AreaName;

                                changeData.Level = (int)item.Level;

                                changeData.DateCoefficient = item.DisposeTime;

                                changeData.DiscountCoff = resBct[resBct.Count - 1].DiscountCoff;

                                changeData.PrivateArea = resBct[resBct.Count - 1].PrivateArea;

                                changeData.VAT = resBct[resBct.Count - 1].VAT;
                                changeData.Ktdbt = resBct[resBct.Count - 1].Ktdbt;
                                changeData.Ktlcb = resBct[resBct.Count - 1].Ktlcb;
                                changeData.StandardPrice = resBct[resBct.Count - 1].StandardPrice;
                                changeData.Unit = resBct[resBct.Count - 1].Unit;
                                changeData.PolicyReduction = resBct[resBct.Count - 1].PolicyReduction;
                                changeData.TotalK = resBct[resBct.Count - 1].TotalK; // => gắn như này ,check tất cả api luon nhé
                                if (itemChange.Type == 1)
                                {
                                    changeData.VAT = itemChange.valueChange;
                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);
                                    }

                                    ///check các cái changeData xem có cái nào ở trên kia chưa đc gắn ko => chưa đc gắn thì gắn vào
                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK * (decimal)(1 + (changeData.VAT / 100))));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));
                                    PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;
                                    changeData.PolicyReduction = (decimal)PolicyReduction;
                                    changeData.PriceAfterDiscount = PriceAfterDiscount;
                                    resBct.Add(changeData);
                                }
                                else if (itemChange.Type == 2)
                                {
                                    changeData.Ktlcb = (decimal)itemChange.valueChange;
                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK * (decimal)(1 + (changeData.VAT / 100))));


                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));
                                    PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;
                                    changeData.PolicyReduction = (decimal)PolicyReduction;
                                    changeData.PriceAfterDiscount = PriceAfterDiscount;

                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);
                                    }

                                    resBct.Add(changeData);

                                }
                                else if (itemChange.Type == 3)
                                {
                                    changeData.Ktdbt = (decimal)itemChange.valueChange;
                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK * (decimal)(1 + (changeData.VAT / 100))));

                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));
                                    PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;
                                    changeData.PolicyReduction = (decimal)PolicyReduction;
                                    changeData.PriceAfterDiscount = PriceAfterDiscount;

                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);
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

                                    resBct.Add(changeData);
                                }
                                else if (itemChange.Type == 4)
                                {
                                    changeData.PolicyReduction = (decimal)itemChange.valueChange;

                                    changeData.chilDfs = resBct[resBct.Count - 1].chilDfs;

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK * (decimal)(1 + (changeData.VAT / 100))));


                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));
                                    PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;
                                    changeData.PolicyReduction = (decimal)PolicyReduction;
                                    changeData.PriceAfterDiscount = PriceAfterDiscount;

                                    if (resBct.Count > 0)
                                    {
                                        changeData.DateEnd = resBct[resBct.Count - 1].DateEnd;
                                        changeData.DateStart = itemChange.DateChange;
                                        resBct[resBct.Count - 1].DateEnd = itemChange.DateChange.AddDays(-1);
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

                                    PriceRent1m2 = Math.Round((decimal)(changeData.StandardPrice * changeData.Ktlcb * changeData.Ktdbt * (decimal)changeData.TotalK * (decimal)(1 + (changeData.VAT / 100))));


                                    PriceRent = Math.Round((decimal)(PriceRent1m2 * (decimal)changeData.PrivateArea));

                                    PolicyReduction = Math.Round((decimal)(changeData.DiscountCoff * PriceRent));
                                    PriceAfterDiscount = Math.Round((decimal)(PriceRent - PolicyReduction));

                                    changeData.PriceRent1m2 = PriceRent1m2;
                                    changeData.PriceRent = PriceRent;
                                    changeData.PolicyReduction = (decimal)PolicyReduction;
                                    changeData.PriceAfterDiscount = PriceAfterDiscount;

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
                    totalPrice = (decimal)f.ToList().Sum(p => p.PriceAfterDiscount),
                    bCTs = f.ToList()
                });

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = groupData;
                return Ok(def);
            }
            catch (Exception ex)
            {
                return Ok(def);  ///Debug vào đây nữa để xem lỗi => api nào ko có thì tự viết vào là oke 
            }
        }

        //API Add QD 22
        [HttpPost("addNewBCT")]
        public async Task<IActionResult> addNewBCT([FromBody] List<groupData> req)
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

            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
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
                        var dataEx = _context.RentBctTables.Where(l => l.RentFileId == req.ToList().First().rentFileId && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                        dataEx.ForEach(x => x.Status = AppEnums.EntityStatus.DELETED);
                        _context.UpdateRange(dataEx);
                        await _context.SaveChangesAsync();
                        List<RentBctTable> resBctTable = new List<RentBctTable>();
                        List<ChilDfTable> resChilDfTable = new List<ChilDfTable>();
                        int iIndex = 0;
                        //if (type == 1)
                        //{
                        //    int CurrData = req.Count();
                        //    int OldData = req.Count();
                        //    int Index = CurrData - OldData;

                        //    for (int i = Index; i > Index; i--)
                        //    {
                        //        foreach (var childItem in req[req.Count() - i].bCTs)
                        //        {
                        //            RentBctTable itemResBctTable = new RentBctTable();
                        //            iIndex++;
                        //            itemResBctTable.Type = childItem.GroupId;
                        //            itemResBctTable.Index = iIndex;
                        //            itemResBctTable.Id = Guid.NewGuid();
                        //            itemResBctTable.AreaName = childItem.AreaName;
                        //            itemResBctTable.Level = childItem.Level.HasValue ? childItem.Level : null;
                        //            itemResBctTable.PrivateArea = childItem.PrivateArea.HasValue ? childItem.PrivateArea : null;
                        //            itemResBctTable.DateCoefficient = childItem.DateCoefficient.HasValue ? childItem.DateCoefficient : null;
                        //            itemResBctTable.StandardPrice = childItem.StandardPrice;
                        //            itemResBctTable.TotalK = (decimal?)(childItem.TotalK.HasValue ? childItem.TotalK : null);
                        //            itemResBctTable.PriceRent1m2 = childItem.PriceRent1m2.HasValue ? childItem.PriceRent1m2 : null;
                        //            itemResBctTable.PriceRent = childItem.PriceRent.HasValue ? childItem.PriceRent : null;
                        //            itemResBctTable.Unit = childItem.Unit;
                        //            itemResBctTable.DateStart = childItem.DateStart.HasValue ? childItem.DateStart : null;
                        //            itemResBctTable.VAT = childItem.VAT;
                        //            itemResBctTable.PolicyReduction = childItem.PolicyReduction;
                        //            itemResBctTable.check = childItem.check;
                        //            itemResBctTable.RentFileId = childItem.RentFileId;
                        //            itemResBctTable.PriceVAT = childItem.PriceVAT;
                        //            itemResBctTable.MonthDiff = childItem.MonthDiff;
                        //            itemResBctTable.TotalPrice = childItem.TotalPrice;
                        //            itemResBctTable.PriceAfterDiscount = childItem.PriceAfterDiscount.HasValue ? childItem.PriceAfterDiscount : null;
                        //            itemResBctTable.DiscountCoff = childItem.DiscountCoff.HasValue ? childItem.DiscountCoff : null;
                        //            itemResBctTable.Ktlcb = childItem.Ktlcb.HasValue ? childItem.Ktlcb : null;
                        //            itemResBctTable.Ktdbt = childItem.Ktdbt.HasValue ? childItem.Ktdbt : null;

                        //            itemResBctTable.CreatedAt = DateTime.Now;
                        //            itemResBctTable.CreatedBy = fullName;
                        //            foreach (var chil in childItem.chilDfs)
                        //            {
                        //                ChilDfTable itemResChilDf = new ChilDfTable();
                        //                itemResChilDf.RentBctTableId = itemResBctTable.Id;
                        //                itemResChilDf.CoefficientId = (int)chil.CoefficientId;
                        //                itemResChilDf.Value = (decimal)chil.Value;
                        //                itemResChilDf.Index = iIndex;
                        //                itemResChilDf.Code = !string.IsNullOrEmpty(chil.Code) ? chil.Code : "";
                        //                itemResChilDf.CreatedAt = DateTime.Now;
                        //                itemResChilDf.CreatedBy = fullName;
                        //                resChilDfTable.Add(itemResChilDf);

                        //            }
                        //            resBctTable.Add(itemResBctTable);
                        //        }
                        //    }
                        //}
                        //else
                        //{
                        foreach (var item in req)
                        {
                            foreach (var childItem in item.bCTs)
                            {
                                RentBctTable itemResBctTable = new RentBctTable();
                                iIndex++;
                                itemResBctTable.Index = iIndex;
                                itemResBctTable.Type = childItem.GroupId;
                                itemResBctTable.Id = Guid.NewGuid();
                                itemResBctTable.AreaName = childItem.AreaName;
                                itemResBctTable.Level = childItem.Level.HasValue ? childItem.Level : null;
                                itemResBctTable.PrivateArea = childItem.PrivateArea.HasValue ? childItem.PrivateArea : null;
                                itemResBctTable.DateCoefficient = childItem.DateCoefficient.HasValue ? childItem.DateCoefficient : null;
                                itemResBctTable.StandardPrice = childItem.StandardPrice;
                                itemResBctTable.TotalK = childItem.TotalK.HasValue ? (decimal)childItem.TotalK : 0;
                                itemResBctTable.PriceRent1m2 = childItem.PriceRent1m2.HasValue ? childItem.PriceRent1m2 : null;
                                itemResBctTable.PriceRent = childItem.PriceRent.HasValue ? childItem.PriceRent : null;
                                itemResBctTable.Unit = childItem.Unit;
                                itemResBctTable.DateStart = childItem.DateStart.HasValue ? childItem.DateStart : null;
                                itemResBctTable.VAT = childItem.VAT;
                                itemResBctTable.PolicyReduction = childItem.PolicyReduction;
                                itemResBctTable.check = childItem.check;
                                itemResBctTable.RentFileId = childItem.RentFileId;
                                itemResBctTable.PriceVAT = childItem.PriceVAT;
                                itemResBctTable.MonthDiff = childItem.MonthDiff;
                                itemResBctTable.TotalPrice = childItem.TotalPrice;
                                itemResBctTable.PriceAfterDiscount = childItem.PriceAfterDiscount.HasValue ? childItem.PriceAfterDiscount : null;
                                itemResBctTable.DiscountCoff = childItem.DiscountCoff.HasValue ? childItem.DiscountCoff : null;
                                itemResBctTable.CreatedAt = DateTime.Now;
                                itemResBctTable.Ktlcb = childItem.Ktlcb;
                                itemResBctTable.Ktdbt = childItem.Ktdbt;
                                foreach (var chil in childItem.chilDfs)
                                {
                                    ChilDfTable itemResChilDf = new ChilDfTable();
                                    itemResChilDf.RentBctTableId = itemResBctTable.Id;
                                    itemResChilDf.CoefficientId = (int)chil.CoefficientId;
                                    itemResChilDf.Value = (decimal?)(chil.Value.HasValue ? chil.Value : null);
                                    itemResChilDf.Code = !string.IsNullOrEmpty(chil.Code) ? chil.Code : "";
                                    itemResChilDf.CreatedAt = DateTime.Now;
                                    itemResChilDf.CreatedBy = fullName;
                                    resChilDfTable.Add(itemResChilDf);

                                }
                                resBctTable.Add(itemResBctTable);
                            }
                        }
                        //}
                        _context.RentBctTables.AddRange(resBctTable);
                        _context.ChilDfTables.AddRange(resChilDfTable);
                        await _context.SaveChangesAsync();

                        transaction.Commit();
                        def.meta = new Meta(200, ApiConstants.MessageResource.ADD_SUCCESS);
                        def.data = resBctTable;

                        return Ok(def);
                    }
                    catch (Exception ex)
                    {
                        log.Error("AddnewBCT-TypeQD 22 Exception:" + ex);
                        def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                        return Ok(def);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("AddnewBCT-TypeQD 22 Exception:" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        [HttpGet("GetDataRentBCTTable/{id}")]
        public async Task<IActionResult> GetDataRentBCTTable(Guid id)
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
                var RentfileData = _context.RentFiles.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();

                var blockData = _context.Blocks.Where(l => l.Status != AppEnums.EntityStatus.DELETED && l.TypeBlockEntity == AppEnums.TypeBlockEntity.BLOCK_RENT).ToList();

                var typeBlockData = _context.TypeBlocks.Where(l => l.Status != AppEnums.EntityStatus.DELETED).ToList();

                List<RentBctTable> data = _context.RentBctTables.Where(l => l.RentFileId == id && l.Status != AppEnums.EntityStatus.DELETED).ToList();
                if (data == null)
                {
                    def.meta = new Meta(404, ApiConstants.MessageResource.NOT_FOUND_VIEW_MESSAGE);
                    return Ok(def);
                }
                List<RentBctTableData> res = _mapper.Map<List<RentBctTableData>>(data.ToList());
                foreach (RentBctTableData item in res)
                {
                    item.chilDfs = _context.ChilDfTables.Where(l => l.RentBctTableId == item.Id && l.Status != AppEnums.EntityStatus.DELETED).OrderBy(p => p.CoefficientId).ToList();
                }


                //var dataFinal = res.GroupBy(x => x.Type).Select(x => new
                //{
                //    key = x.Key,
                //    bCTs = x.ToList().OrderBy(p => p.Index)
                //}).ToList().OrderBy(x => x.key);

                var groupData = res.OrderBy(p => p.Index).GroupBy(x => x.Type).Select(f => new groupData
                {
                    GroupId = (int)f.Key,
                    //VAT = (double)f.ToList().First().VAT,
                    //DateStart = (DateTime)f.ToList().First().DateStart,
                    //totalPrice = (decimal)f.ToList().Sum(p => p.PriceAfterDiscount),
                    Bcts = f.ToList(),
                    rentFileId = (Guid)f.ToList().First().RentFileId,
                    TypeBlockName = typeBlockId((Guid)f.ToList().First().RentFileId, RentfileData, blockData, typeBlockData)
                }); ;


                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = groupData;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("GetById Error" + ex);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        private static string typeBlockId(Guid RentFileID, List<RentFile> rf, List<Block> bl, List<TypeBlock> typeBlockData)
        {
            var blockId = rf.Where(l => l.Id == RentFileID).Select(p => p.RentBlockId).FirstOrDefault();
            var typeblockId = bl.Where(l => l.Id == blockId).Select(p => p.TypeBlockId).FirstOrDefault();
            var name = typeBlockData.Where(l => l.Id == typeblockId).Select(p => p.Name).FirstOrDefault();
            return name;
        }

        [HttpGet("GroupedData/{Id}/{TypeBCT}")]
        public IActionResult GroupedData(int id, byte TypeBCT)
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

            if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            {
                def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_CREATE_MESSAGE);
                return Ok(def);
            }
            try
            {
                List<DefaultCoefficient> data = _context.DefaultCoefficients.Where(l => l.Status != AppEnums.EntityStatus.DELETED).OrderBy(p => p.CoefficientName).ToList();
                var groupedData = data.GroupBy(x => x.CoefficientName).Select(e => new GroupData
                {
                    CoefficientId = e.Key,
                    defaultCoefficients = e.ToList(),
                    childDefaultCoefficients = _context.ChildDefaults.Where(l => l.RentFileBctId == id && l.CoefficientId == e.Key && l.Type == TypeBCT && l.Status != AppEnums.EntityStatus.DELETED).ToList()
                }).ToList();

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = groupedData;
                return Ok(def);
            }
            catch
            {
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }
    }
}
