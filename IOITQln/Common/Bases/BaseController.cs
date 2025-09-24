//using AutoMapper;
//using IOITQln.Common.Constants;
//using IOITQln.Common.ViewModels.ResponseApi;
//using MediatR;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.DependencyInjection;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using static IOITQln.Common.Constants.ApiConstants;

//namespace IOITQln.Common.Bases
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public abstract class BaseController : ControllerBase
//    {
//        protected IMapper _mapper => HttpContext.RequestServices.GetService<IMapper>();

//        protected ActionResult Res(ResponseApi res)
//        {
//            return Content(res?.ToJson(), "application/json");
//        }

//        protected Output<T> Res<T>(T data, int? metadata = 0, string msg = MessageResource.ACCTION_SUCCESS, int code = ApiConstants.StatusCode.Success200)
//        {
//            return new Output<T>
//            {
//                Data = data,
//                Meta = new Meta
//                {
//                    Error_code = code,
//                    Error_message = msg
//                },
//                Metadata = metadata
//            };
//        }


//    }

//    public class Output
//    {
//        [JsonProperty("meta")]
//        public Meta Meta { get; set; }

//        [JsonProperty("metadata")]
//        public int? Metadata { get; set; }
//    }

//    public class Output<T> : Output
//    {
//        [JsonProperty("data")]
//        public T Data { get; set; }
//    }
//}
