using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ExceptionHandling_Global.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoresController : ControllerBase
    {
        //[HttpGet("")]
        [HttpGet(Name = "GetAll")]
        [SwaggerOperation(
            OperationId = "GetAll",
            Summary = "This method create Exception Error",
            Description = "statement: throw new Exception('Test Exception Message...'); \n\n'Type, TraceId' will return."
        )]
        public async Task<IActionResult> GetAll()
        {
            throw new Exception("Test Exception Message...");
            return Ok(new[] { 1, 2, 3 });
        }

    }
}
