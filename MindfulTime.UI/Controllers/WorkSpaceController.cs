using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MindfulTime.UI.Interfaces;
using MindfulTime.UI.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using OpenClasses;

namespace MindfulTime.UI.Controllers
{
    public class WorkSpaceController(ILogger<WorkSpaceController> logger, IHttpRequestService httpRequestService) : Controller
    {
        private readonly ILogger<WorkSpaceController> _logger = logger;
        private readonly IHttpRequestService _httpRequestService = httpRequestService;


        public IActionResult Auth()
        {
            return View();
        }

        [HttpPost]
        [Route("WorkSpace")]

        public async Task<IActionResult> LoginTo(AuthUserModel userModel)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpRequestService.HttpRequest(URL.AUTH_CHECK_USER, new StringContent(JsonConvert.SerializeObject(userModel), encoding: System.Text.Encoding.UTF8, "application/json"));
                if (response.Contains("FALSE")) return RedirectToAction("Auth", "WorkSpace");
                var responseModel = JsonConvert.DeserializeObject<AuthResponse>(response);
                if (responseModel!.isOk)
                {
                    ViewBag.UserModel = responseModel;
                    return View("WorkSpace");
                }
                else
                {
                    ModelState.AddModelError("", "    ");
                }
            };
            return RedirectToAction("Auth", "WorkSpace");
        }

        [HttpPost]
        public async Task<IActionResult> AddEvent([FromBody] EventDTO _event)
        {
            if (ModelState.IsValid)
            {
                _event.EventId = Guid.NewGuid();
                var response = await _httpRequestService.HttpRequest(URL.CALENDAR_CREATE_TASK, new StringContent(JsonConvert.SerializeObject(_event), encoding: System.Text.Encoding.UTF8, "application/json"));
                if (response.Contains("FALSE")) return RedirectToAction("WorkSpace");
                var responseModel = JsonConvert.DeserializeObject<AuthResponse>(response);
                if (responseModel!.isOk)
                {
                    ViewBag.UserModel = responseModel;
                    return View("WorkSpace");
                }
                else
                {
                    ModelState.AddModelError("", "    ");
                }
            };
            return RedirectToAction("Auth", "WorkSpace");
        }


        [HttpPost]
        [Route("LogOut")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Auth", "WorkSpace");
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
