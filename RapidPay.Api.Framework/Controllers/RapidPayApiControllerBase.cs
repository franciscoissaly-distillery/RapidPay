using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RapidPay.Api.Framework.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public abstract class RapidPayApiControllerBase : ControllerBase
    { }
}
