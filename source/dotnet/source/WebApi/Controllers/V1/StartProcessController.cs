using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.V1;

[ApiController]
[ApiVersion(Version)]
[Route("v{version:apiVersion}/[controller]")]
public class StartProcessController : ControllerBase
{
    private const string Version = "1.0";
    
    [HttpPost]
    [MapToApiVersion(Version)]
    public string Post()
    {
        return "start process";
    }
}