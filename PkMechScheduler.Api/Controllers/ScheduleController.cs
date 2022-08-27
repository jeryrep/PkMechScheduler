using Microsoft.AspNetCore.Mvc;
using PkMechScheduler.Api.Models;
using PkMechScheduler.Api.Services;

namespace PkMechScheduler.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly ScheduleService _scheduleService;

        public ScheduleController(ScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }
        [HttpGet("/GetRawSchedule")]
        public Task<Dictionary<Day, List<BlockModel>>> GetRawSchedule(string group) => _scheduleService.GetRawSchedule(group);
    }
}
