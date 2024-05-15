using Microsoft.AspNetCore.Mvc;
using MindfulTime.UI.Models;
using System.Diagnostics;

namespace MindfulTime.UI.Controllers
{
    public class WorkSpaceController : Controller
    {
        private readonly ILogger<WorkSpaceController> _logger;

        public WorkSpaceController(ILogger<WorkSpaceController> logger)
        {
            _logger = logger;
        }

        public IActionResult Auth()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
