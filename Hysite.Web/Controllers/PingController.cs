using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

