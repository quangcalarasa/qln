using IOITQln.Common.Hub;
using IOITQln.Common.Interfaces;
using IOITQln.Common.Interfaces.Helpers;
using IOITQln.Common.Services;
using IOITQln.Common.ViewModels.Common;
using IOITQln.Models.Mappings;
using IOITQln.Persistence;
using IOITQln.QuickPriceNOC.Interface;
using IOITQln.QuickPriceNOC.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IOITQln
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public static string ContentRootPath { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().AddNewtonsoftJson(
                (options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                }));

            services.AddTransient<IDateTimeService, DateTimeService>();
            services.AddTransient<IFirebaseService, FirebaseService>();
            services.AddTransient<IQuickPrice, QuickPriceService>();
            services.AddTransient<ICheckStatusMd167House, CheckStatusMd167House>();

            // Auto mapper Configuration
            services.AddAutoMapper(typeof(Startup));
            //var coreMappingAssembly = typeof(AutoMapping).Assembly;
            //services.AddAutoMapper(coreMappingAssembly);
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.AddDbContext<ApiDbContext>();
            
            services.AddOptions();
            services.AddCors(c =>
            {
                c.AddPolicy("AllowCors", options => options.WithOrigins("https://qln.hmcic.vn").AllowCredentials().AllowAnyMethod().AllowAnyHeader().Build());
            });

            services.AddMvc(options =>
            {
                var noContentFormatter = options.OutputFormatters.OfType<HttpNoContentOutputFormatter>().FirstOrDefault();
                if (noContentFormatter != null)
                {
                    noContentFormatter.TreatNullValueAsNoContent = false;
                }
            });

            services.AddHttpContextAccessor();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API docs", Version = "v1", Description = "APis are built for project system by ultraneit" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}
                    }
                });
            });

            services.AddHttpClient();

            string domain = Configuration["AppSettings:JwtIssuer"];
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = domain,
                    ValidAudience = domain,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["AppSettings:JwtKey"])),
                    ClockSkew = TimeSpan.Zero // remove delay of token when expire
                };
            });

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "web-app/dist";
            });

            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart
            });

            services.AddSignalR();

            services.AddScoped<IDapper, Dapperr>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "QLCD API By IOIT");
            });

            app.Use((context, next) =>
            {
                var requestFeature = context.Features.Get<IHttpRequestFeature>();

                string url = requestFeature.RawTarget;

                if (url.Contains("../") || url.Contains("/.."))
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.Body = Stream.Null;
                    context.Response.ContentLength = 0;

                    return null;
                }
                else
                    return next.Invoke();
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles(new StaticFileOptions() {
                OnPrepareResponse = s => {
                    if (s.Context.Request.Path.Value.Contains("/uploads/files") && !s.Context.User.Identity.IsAuthenticated)
                    {
                        s.Context.Response.StatusCode = 401;
                        s.Context.Response.Body = Stream.Null;
                        s.Context.Response.ContentLength = 0;
                    }
                }
            });

            app.UseSpaStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = s => {
                    if (s.Context.Request.Path.Value.Contains("/uploads/files") && !s.Context.User.Identity.IsAuthenticated)
                    {
                        s.Context.Response.StatusCode = 401;
                        s.Context.Response.Body = Stream.Null;
                        s.Context.Response.ContentLength = 0;
                    }
                }
            });

            app.UseRouting();
            app.UseCors("AllowCors");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseWebSockets();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<BroadcastHub>("/notify");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                     name: "default",
                     pattern: "{controller}/{action=Index}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "web-app";

                //if (env.IsDevelopment())
                //{
                //    TimeSpan interval = TimeSpan.FromSeconds(120);
                //    spa.Options.StartupTimeout = interval;
                //    spa.UseAngularCliServer(npmScript: "start");
                //}
            });

            env.ContentRootPath = ContentRootPath;
        }
    }
}
