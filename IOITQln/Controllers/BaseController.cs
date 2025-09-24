using IOITQln.Common.ViewModels.Common;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace IOITQln.Controllers
{
    [ApiController]
    [EnableCors("AllowCors")]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        protected ActionResult Res(DefaultResponse res)
        {
            return Content(res.ToString(), "application/json");
        }
    }
}
