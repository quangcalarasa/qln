using IOITQln.Common.Constants;
using IOITQln.Common.Enums;
using IOITQln.Common.Services;
using IOITQln.Common.ViewModels.Common;
using IOITQln.Persistence;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using Microsoft.Net.Http.Headers;

using IOITQln.QuickPriceNOC.Interface;
using IOITQln.Common.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using IOITQln.Entities;
using IOITQln.Models.Data;
using System.Collections.Generic;
using System.Web;
using log4net;
using IOITQln.Models.Dto;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using AutoMapper;
using DevExpress.ClipboardSource.SpreadsheetML;
using NPOI.SS.Formula.Functions;

namespace IOITQln.QuickPriceNOC
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuickMathController : ControllerBase
    {
        private static string functionCode = "QuickMath";
        private readonly ApiDbContext _context;
        private readonly IQuickPrice _quickPrice;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private static readonly ILog log = LogMaster.GetLogger("QuickMath", "QuickMath");
        private IMapper _mapper;

        public QuickMathController(ApiDbContext context, IQuickPrice quickPrice, IMapper mapper, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _quickPrice = quickPrice;
            _serviceScopeFactory = serviceScopeFactory;
            _mapper = mapper;
        }

        //[HttpPost]
        //public async Task<IActionResult> QuickMathPrice(QuickPriceReq req)
        //{
        //    //string accessToken = Request.Headers[HeaderNames.Authorization];
        //    //Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
        //    //if (token == null)
        //    //{
        //    //    return Unauthorized();
        //    //}
        //    DefaultResponse def = new DefaultResponse();

        //    //var identity = (ClaimsIdentity)User.Identity;
        //    //int userId = int.Parse(identity.Claims.Where(c => c.Type == "Id").Select(c => c.Value).SingleOrDefault());
        //    //string fullName = identity.Claims.Where(c => c.Type == "FullName").Select(c => c.Value).SingleOrDefault() ?? string.Empty;
        //    //string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
        //    //if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.IMPORT))
        //    //{
        //    //    def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_ACTION_MESSAGE);
        //    //    return Ok(def);
        //    //}
        //    //var data = _quickPrice.QuickPrice(req);



        //    _ = Task.Run(() => _quickPrice.QuickPrice(req, _serviceScopeFactory));

        //    def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
        //    def.data = null;
        //    return Ok(def);
        //}


        [HttpGet("GetByPage")]
        public IActionResult GetByPage([FromQuery] FilteredPagination paging)
        {
            //string accessToken = Request.Headers[HeaderNames.Authorization];
            //Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            //if (token == null)
            //{
            //    return Unauthorized();
            //}

            DefaultResponse def = new DefaultResponse();

            //var identity = (ClaimsIdentity)User.Identity;
            //string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            //if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            //{
            //    def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
            //    return Ok(def);
            //}

            try
            {
                if (paging != null)
                {
                    def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                    IQueryable<QuickMathHistory> data = _context.QuickMathHistories.Where(c => c.Status != AppEnums.EntityStatus.DELETED);
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


        [HttpPost("QuickMathPrice")]
        public async Task<IActionResult> QuickMathPrice(QuickMathHistory input)
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
                input = (QuickMathHistory)UtilsService.TrimStringPropertyTypeObject(input);

                if (!ModelState.IsValid)
                {
                    def.meta = new Meta(400, ApiConstants.MessageResource.BAD_REQUEST_MESSAGE);
                    return Ok(def);
                }

                using (var transaction = _context.Database.BeginTransaction())
                {
                    switch (input.Type)
                    {
                        case 1:
                            input.TypeValue = "VAT";
                            break;
                        case 2:
                            input.TypeValue = "Lương cơ bản";
                            break;
                        case 3:
                            input.TypeValue = "Giá chuẩn";
                            break;
                        case 4:
                            input.TypeValue = "Thời điểm bố trí";
                            break;
                    }
                    input.CreatedById = userId;
                    input.CreatedBy = fullName;
                    input.StatusProcess = 1;
                    _context.QuickMathHistories.Add(input);

                    try
                    {
                        await _context.SaveChangesAsync();

                        //thêm LogAction
                        LogActionModel logActionModel = new LogActionModel("Thêm mới lịch sử tính giá nhanh", "QuickMath", input.Id, HttpContext.Connection.RemoteIpAddress.ToString(), JsonConvert.SerializeObject(input), (int)AppEnums.Action.CREATE, userId, fullName);
                        LogAction logAction = _mapper.Map<LogAction>(logActionModel);
                        _context.Add(logAction);
                        await _context.SaveChangesAsync();

                        if (input.Id > 0)
                            transaction.Commit();
                        else
                            transaction.Rollback();
                        QuickPriceReq req = new QuickPriceReq();
                        req.QuickMathHistoryId = input.Id;
                        req.DoApply = input.DoApply;
                        req.Value = input.Value;
                        req.Type = input.Type;
                        //Tính giá nhanh
                        _ = Task.Run(() => _quickPrice.QuickPrice(req, _serviceScopeFactory));
                        def.meta = new Meta(200, ApiConstants.MessageResource.ADD_SUCCESS);
                        def.data = input;
                        return Ok(def);
                    }
                    catch (DbUpdateException e)
                    {
                        log.Error("DbUpdateException:" + e);
                        transaction.Rollback();
                        if (QuickMathExists(input.Id))
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
                log.Error("QuickMathPrice Exception:" + e);
                def.meta = new Meta(500, ApiConstants.MessageResource.ERROR_500_MESSAGE);
                return Ok(def);
            }
        }

        
        [HttpPost("GetView")]
        public async Task<IActionResult> GetView(QuickPriceReq req)
        {

            //string accessToken = Request.Headers[HeaderNames.Authorization];
            //Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            //if (token == null)
            //{
            //    return Unauthorized();
            //}

            DefaultResponse def = new DefaultResponse();

            //var identity = (ClaimsIdentity)User.Identity;
            //string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            //if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            //{
            //    def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
            //    return Ok(def);
            //}
            try
            {
                var dataRentFile = _context.RentFiles.Where(l => l.Status != Common.Enums.AppEnums.EntityStatus.DELETED && l.DateHD.AddMonths(l.Month) > req.DoApply && l.DateHD < req.DoApply).ToList();
                var dataRentFileTable = _context.RentBctTables.Where(l => l.Status != Common.Enums.AppEnums.EntityStatus.DELETED).ToList();
                List<RentBctTable> listUpdate = new List<RentBctTable>();

                dataRentFile.ForEach(item =>
                {
                    var dataQ = dataRentFileTable.Where(l => l.RentFileId == item.Id).OrderBy(l => l.Index).GroupBy(p => p.Type).Select(e => new
                    {
                        key = e.Key,
                        date = e.ToList().First().DateStart,
                        data = e.ToList(),
                    }).ToList();

                    if (dataQ.Count > 0)
                    {
                        var rq = dataQ.Last().data;
                        dataQ.Last().data.ForEach(itemC =>
                        {
                            listUpdate.Add(itemC);
                        });
                    }
                });

                var dataGr = listUpdate.GroupBy(x => x.RentFileId).Select(e => new
                {
                    Key = ReturnCode(e.Key, dataRentFile),
                    data = e.ToList(),
                }).ToList();

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = dataGr;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("GetView Error:" + ex);
                def.meta = new Meta(500, "Lỗi :" + ex.ToString());
                return Ok(def);
            }
        }

        private bool QuickMathExists(int id)
        {
            return _context.QuickMathHistories.Count(e => e.Id == id) > 0;
        }

        private static string ReturnCode(Guid? key, List<RentFile> data)
        {
            string Code = data.Where(l => l.Id == key).Select(l => l.Code).FirstOrDefault();
            return Code;
        }

        [HttpPost("GetLogView")]
        public async Task<IActionResult> GetLogView(QuickPriceReq req)
        {

            //string accessToken = Request.Headers[HeaderNames.Authorization];
            //Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            //if (token == null)
            //{
            //    return Unauthorized();
            //}

            DefaultResponse def = new DefaultResponse();

            //var identity = (ClaimsIdentity)User.Identity;
            //string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
            //if (!CheckRole.CheckRoleByCode(access_key, functionCode, (int)AppEnums.Action.VIEW))
            //{
            //    def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
            //    return Ok(def);
            //}
            try
            {
                var data  = _context.QuickMathLogs.Where(l => l.QuickMathHistoryId == req.QuickMathHistoryId).OrderByDescending(p => p.UpdatedAt).ToList();

                def.meta = new Meta(200, ApiConstants.MessageResource.ACCTION_SUCCESS);
                def.data = data;
                return Ok(def);
            }
            catch (Exception ex)
            {
                log.Error("GetLogView Error:" + ex);
                def.meta = new Meta(500, "Lỗi :" + ex.ToString());
                return Ok(def);
            }
        }

    }
}
