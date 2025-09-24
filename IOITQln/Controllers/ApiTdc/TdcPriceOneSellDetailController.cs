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
using IOITQln.Migrations;
using OfficeOpenXml.Style;
using NPOI.SS.Formula.Functions;
using NPOI.SS.Formula;

namespace IOITQln.Controllers.ApiTdc
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TdcPriceOneSellDetailController : ControllerBase
    {
        private static readonly ILog log = LogMaster.GetLogger("ingreprice", "ingreprice");
        private static string functionCode = "INGREDIENTS_PRICE";
        private readonly ApiDbContext _context;
        private IMapper _mapper;
        public TdcPriceOneSellDetailController(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
    }
}
