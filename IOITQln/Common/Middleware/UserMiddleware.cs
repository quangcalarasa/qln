using IOITQln.Persistence;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IOITQln.Common.Middleware
{
    public class UserMiddleware
    {
        private readonly RequestDelegate _next;

        public UserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApiDbContext dbContext)
        {
            await HandleAuthenticatedUser(context, dbContext);
            await _next(context);
        }

        private async Task HandleUnauthorized(HttpContext context)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorize");
        }

        private async Task HandleAuthenticatedUser(HttpContext context, ApiDbContext dbContext)
        {
            var clientClaim = context.User.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti);

            if(clientClaim != null)
            {
                //Check jti exist in blacklist
                Entities.Token token = (from t in dbContext.Tokens where Convert.ToString(t.AccessToken) == clientClaim.Value select t).FirstOrDefault();

                if(token != null)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorize");
                }
            }
        }
    }
}
