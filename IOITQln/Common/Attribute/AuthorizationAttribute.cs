using IOITQln.Common.Constants;
using IOITQln.Common.Enums;
using IOITQln.Common.Services;
using IOITQln.Common.ViewModels.Common;
using IOITQln.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IOITQln.Common.Attribute
{
    public class AuthorizationAttribute : IAuthorizationFilter
    {
        private AppEnums.Action _action { get; set; }
        private string _functionCode { get; set; }
        private readonly ApiDbContext _context;

        public AuthorizationAttribute( AppEnums.Action action, string functionCode, ApiDbContext context)
        {
            _action = action;
            _functionCode = functionCode;
            _context = context;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string accessToken = context.HttpContext.Request.Headers[HeaderNames.Authorization];
            Entities.Token token = (from t in _context.Tokens where Convert.ToString(t.AccessToken) == accessToken select t).FirstOrDefault();
            if (token == null)
            {
                context.Result = new UnauthorizedResult();
            }
            else
            {
                var identity = (ClaimsIdentity)context.HttpContext.User.Identity;
                string access_key = identity.Claims.Where(c => c.Type == "AccessKey").Select(c => c.Value).SingleOrDefault();
                if (!CheckRole.CheckRoleByCode(access_key, _functionCode, (int)_action))
                {
                    DefaultResponse def = new DefaultResponse();
                    def.meta = new Meta(222, ApiConstants.MessageResource.NOPERMISION_VIEW_MESSAGE);
                    context.Result = new OkResult();
                    var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(def));
                    context.HttpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                }
            }
        }
    }
}
