using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Core;
using System;
using System.Threading.Tasks;

namespace SQi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int pageSize = 10, int pageIndex = 0, DateTime? from = null, DateTime? to = null, string province = null)
        {
            try
            {
                var result = await _reportService.Get(pageSize, pageIndex, from, to, province);
                if (result.Succeed)
                {
                    return Ok(result);
                }

                return BadRequest(result.ErrorMessage);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Calculate(int year, int month)
        {
            var rs = await _reportService.Calculate(year, month);
            return Ok(rs);
        }

        [HttpPost("SentToPQM")]
        public async Task<IActionResult> SentToPQM(int year, int month, string province_code)
        {
            try
            {
                var result = await _reportService.SentToPQM(year, month, province_code);
                if (result.Succeed)
                {
                    return Ok(result);
                }

                return BadRequest(result.ErrorMessage);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
