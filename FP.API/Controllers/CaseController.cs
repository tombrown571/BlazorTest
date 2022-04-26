using System.Runtime.CompilerServices;
using FP.MissingLink;
using Microsoft.AspNetCore.Mvc;

namespace FP.API.Controllers
{
    [Produces("application/json")]
    [ApiController]
    [Route("api/[controller]")]
    public class CaseController : ControllerBase
    {

        private readonly ILogger<CaseController> _logger;

        public CaseController(ILogger<CaseController> logger)
        {
            _logger = logger;
        }

        private const string GetCasesRoute = "getcases";
        [HttpGet]
        [Route("", Name = GetCasesRoute)]
        public  IActionResult Get([FromQuery] CaseResourceParameters resourceParameters)
        {
            try
            {
                var casesResult = testData;
                return Ok(casesResult);
            }
            catch (Exception e)
            {
                return GetErrorStatus(e);
            }
        }

        private const string CaseByIdRoute = "getcasebyId";
        [HttpGet]
        [Route("{id:int}", Name = CaseByIdRoute)]
        public  IActionResult Get(int id)
        {
            try
            {
                var fpCase =  testData.FirstOrDefault(c => c.Id == id);
                if (fpCase == null)
                {
                    return NotFound();
                }
                return Ok(fpCase);
            }
            catch (Exception e)
            {
                return GetErrorStatus(e);
            }
        }

        private static List<CaseDto> testData = new List<CaseDto>
        {
            new CaseDto
            {
                Id = 1, Reference = "C123", Name = "Operation Kleiner", Description = "Met Police SC Unit",
                Folder = "C:\\Temp\\Cases\\C123", InstructionDate = new DateTime(2021, 12, 20)
            },
            new CaseDto
            {
                Id = 2, Reference = "C155", Name = "Crown v Smith", Description = "Hastings Solicitors",
                Folder = "C:\\Temp\\Cases\\C155", InstructionDate = new DateTime(2022, 12, 02)
            },
            new CaseDto
            {
                Id = 3, Reference = "C201", Name = "Crown v Jones", Description = "Gillingham Chambers",
                Folder = "C:\\Temp\\Cases\\C201", InstructionDate = new DateTime(2022, 02, 24)
            },
            new CaseDto
            {
                Id = 4, Reference = "C234", Name = "Operation Tradewind", Description = "West Mids Police Fraud Unit",
                Folder = "C:\\Temp\\Cases\\C233", InstructionDate = new DateTime(2022, 03, 10)
            }
        };

        protected IActionResult GetErrorStatus(Exception e, [CallerMemberName] string caller = null)
        {
            //string fullError = FpExtensions.FormatException(e);
            //_logger.LogError(500, e, $"Auth User: {GetAuthorizedUser()} - {caller} : {e.Message}{Environment.NewLine}{fullError}");
            var fullError = $"API Error {caller} : {e.Message} {e.StackTrace}";
            return StatusCode(500, fullError);
        }

    }
}