using Booking_Service_App.Extensions;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Booking_Service_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class SQiController : ControllerBase
    {
        private ISQiService _sqiService;

        public SQiController(ISQiService sqiService)
        {
            _sqiService = sqiService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] SQiCreateModel model)
        {
            try
            {
                var username = User.Claims.GetUsername();
                var result = await _sqiService.Add(model, username);
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

        [HttpGet]
        public async Task<IActionResult> Get(int pageSize = 10, int pageIndex = 0, DateTime? from = null, DateTime? to = null)
        {
            try
            {
                var username = User.Claims.GetUsername();
                var result = await _sqiService.Get(pageSize, pageIndex, from, to, username);
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

        [HttpGet("GetStatistics")]
        public async Task<IActionResult> GetStatistics(DateTime? from = null, DateTime? to = null)
        {
            try
            {
                var username = User.Claims.GetUsername();
                var result = await _sqiService.GetStatistics(from, to, username);
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

        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _sqiService.Delete(id);
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

        [HttpGet("Export")]
        public async Task<IActionResult> Export(DateTime? from = null, DateTime? to = null)
        {
            var username = User.Claims.GetUsername();
            var result = await _sqiService.ExportExcel(from, to, username);
            if (result.Succeed)
            {
                var fileBytes = result.Data as byte[];
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"SurveyList_{DateTime.Now.Ticks}");
            }
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("FixSiteCode")]
        public IActionResult FixSiteCode(List<FixSiteCodeModel> fixSiteCodeModels)
        {
            return Ok(_sqiService.FixSiteCode(fixSiteCodeModels));
        }
    }
}
