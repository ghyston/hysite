using System;
using Microsoft.AspNetCore.Mvc;

namespace hySite 
{
    public class PingController : Controller 
    {
        private readonly IViewStatisticRepository _viewStatisticRepository;
        public PingController(IViewStatisticRepository viewStatisticRepository)
        {
            _viewStatisticRepository = viewStatisticRepository;
        }

        [Route("ping")]
        public IActionResult Index()
        {
            return Ok();
        }

        [Route("lastweek")]
        public IActionResult LastWeekViews()
        {
            return Ok(_viewStatisticRepository.ViewsFrom(DateTime.Now.AddDays(-7)));
        }
        
    }
}

