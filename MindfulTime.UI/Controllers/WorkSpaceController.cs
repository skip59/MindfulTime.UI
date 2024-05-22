using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MindfulTime.UI.Interfaces;
using MindfulTime.UI.Models;
using Newtonsoft.Json;
using OpenClasses;
using System.Diagnostics;

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

        [Route("Users")]
        public async Task<IActionResult> EditUsers()
        {
            string userId = HttpContext.Session.GetString("CurrentUserId");
            string userRole = HttpContext.Session.GetString("CurrentUserRole");
            string userEmail = HttpContext.Session.GetString("CurrentUserEmail");
            string userPassword = HttpContext.Session.GetString("CurrentUserPassword");
            string userName = HttpContext.Session.GetString("CurrentUserName");

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole) ||
                string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userPassword))
            {
                return RedirectToAction("Auth", "WorkSpace");
            }
            UserDto userDto = new()
            {
                Id = Guid.Parse(userId),
                Role = userRole,
                Email = userEmail,
                Password = userPassword,
                Name = userName
            };
            var response = await _httpRequestService.HttpRequest(URL.AUTH_GET_USERS, new StringContent(JsonConvert.SerializeObject(userDto), encoding: System.Text.Encoding.UTF8, "application/json"));
            if (response.Contains("FALSE")) return RedirectToAction("Auth", "WorkSpace");
            List<UserDto> responseModel;
            try
            {
                responseModel = JsonConvert.DeserializeObject<List<UserDto>>(response);
            }
            catch (JsonSerializationException)
            {
                _logger.LogError("Ошибка десериализации ответа от AUTH_GET_USERS.");
                return RedirectToAction("Auth", "WorkSpace");
            }
            ViewBag.AllUsers = responseModel;
            return View(userDto);
        }

        [HttpPost]
        [Route("WorkSpace")]
        public async Task<IActionResult> LoginTo(AuthUserModel userModel)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpRequestService.HttpRequest(URL.AUTH_CHECK_USER, new StringContent(JsonConvert.SerializeObject(userModel), encoding: System.Text.Encoding.UTF8, "application/json"));
                if (response.Contains("FALSE")) return RedirectToAction("Auth", "WorkSpace");
                var responseModel = JsonConvert.DeserializeObject<UserDto>(response);
                if (responseModel!.Id != Guid.Empty)
                {
                    HttpContext.Session.SetString("CurrentUserId", responseModel.Id.ToString());
                    HttpContext.Session.SetString("CurrentUserName", responseModel.Name.ToString());
                    HttpContext.Session.SetString("CurrentUserPassword", userModel.Password);
                    HttpContext.Session.SetString("CurrentUserEmail", userModel.Email);
                    HttpContext.Session.SetString("CurrentUserRole", responseModel.Role);
                    return View("WorkSpace", responseModel);
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
                _event.UserId = Guid.Parse(HttpContext.Session.GetString("CurrentUserId"));
                var response = await _httpRequestService.HttpRequest(URL.CALENDAR_CREATE_TASK, new StringContent(JsonConvert.SerializeObject(_event), encoding: System.Text.Encoding.UTF8, "application/json"));
                if (response.Contains("FALSE")) return null;
                var responseModel = JsonConvert.DeserializeObject<EventDTO>(response);
                string message = string.Empty;
                return Json(new { message, responseModel.EventId });
            };
            return null;
        }

        [HttpPost]
        public IActionResult PrivateUserPage(UserDto userDto)
        {
            if (ModelState.IsValid)
            {
                return View(userDto);
            }
            return View("Error");
        }
        [Route("WorkSpace")]
        public async Task<IActionResult> WorkSpace()
        {
            var userModel = new AuthUserModel
            {
                Email = HttpContext.Session.GetString("CurrentUserEmail"),
                Password = HttpContext.Session.GetString("CurrentUserPassword")
            };
            var response = await _httpRequestService.HttpRequest(URL.AUTH_CHECK_USER, new StringContent(JsonConvert.SerializeObject(userModel), encoding: System.Text.Encoding.UTF8, "application/json"));
            if (response.Contains("FALSE")) return RedirectToAction("Auth", "WorkSpace");
            var responseModel = JsonConvert.DeserializeObject<UserDto>(response);
            return View(responseModel);
        }

        [HttpPost]
        [Route("LogOut")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Auth", "WorkSpace");
        }

        [HttpGet]
        public async Task<string> GetCalendarEvents()
        {
            var userID = HttpContext.Session.GetString("CurrentUserId");
            if (string.IsNullOrEmpty(userID)) return null;
            var currentUser = new UserMT
            {
                Id = Guid.Parse(userID)
            };
            var response = await _httpRequestService.HttpRequest(URL.CALENDAR_GET_TASK, new StringContent(JsonConvert.SerializeObject(currentUser), encoding: System.Text.Encoding.UTF8, "application/json"));
            return response;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
