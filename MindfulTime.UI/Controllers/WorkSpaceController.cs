using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MindfulTime.UI.Interfaces;
using MindfulTime.UI.Models;
using Newtonsoft.Json;
using OpenClasses.Auth;
using OpenClasses.Calendar;
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

        #region Работа с пользователями

        [HttpPost]
        public async Task<string> AddUser(UserDto user)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpRequestService.HttpRequestPost(URL.AUTH_CREATE_USER, new StringContent(JsonConvert.SerializeObject(user), encoding: System.Text.Encoding.UTF8, "application/json"));
                if (response.Contains("FALSE")) return response;
            }
            return ModelState.ToString();
        }

        [HttpPost]
        [Route("WorkSpace")]
        public async Task<IActionResult> LoginTo(AuthUserModel userModel)
        {
            if (ModelState.IsValid)
            {
                var result = await CheckUserFromDb(userModel);
                if (result.Id == Guid.Empty) return RedirectToAction("Auth", "WorkSpace");
                if (result.Id != Guid.Empty)
                {
                    HttpContext.Session.SetString("CurrentUserId", result.Id.ToString());
                    HttpContext.Session.SetString("CurrentUserName", result.Name.ToString());
                    HttpContext.Session.SetString("CurrentUserPassword", userModel.Password);
                    HttpContext.Session.SetString("CurrentUserEmail", userModel.Email);
                    HttpContext.Session.SetString("CurrentUserRole", result.Role);
                    HttpContext.Session.SetString("CurrentUserTid", string.IsNullOrEmpty(result.TelegramId) ? "0" : result.TelegramId);
                    HttpContext.Session.SetString("CurrentUserIsNotify", result.IsSendMessage.ToString());
                    return View("WorkSpace", result);
                }
                else
                {
                    ModelState.AddModelError("", "Пользователь не найден");
                }
            };
            return RedirectToAction("Auth", "WorkSpace");
        }

        [HttpPost]
        public IActionResult PrivateUserPage(UserDto userDto)
        {
            if (ModelState.IsValid)
            {
                //var user = new 
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
            var result = await CheckUserFromDb(userModel);
            if (result.Id == Guid.Empty) return RedirectToAction("Auth", "WorkSpace");
            return View(result);
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
            var response = await _httpRequestService.HttpRequestPost(URL.AUTH_GET_USERS, new StringContent(JsonConvert.SerializeObject(userDto), encoding: System.Text.Encoding.UTF8, "application/json"));
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
        [Route("LogOut")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Auth", "WorkSpace");
        }

        [HttpPost]
        public async Task<bool> DeleteUser(UserDto user)
        {
            if(ModelState.IsValid)
            {
                var response = await _httpRequestService.HttpRequestPost(URL.AUTH_DELETE_USER, new StringContent(JsonConvert.SerializeObject(user), encoding: System.Text.Encoding.UTF8, "application/json"));
                if (response.Contains("FALSE")) return false;
                return true;
            }
           return false;
        }

        [HttpPost]
        public async Task<bool> UpdateUser(UserDto user)
        {
            if (ModelState.IsValid)
            {
                var response = await _httpRequestService.HttpRequestPost(URL.AUTH_UPDATE_USER, new StringContent(JsonConvert.SerializeObject(user), encoding: System.Text.Encoding.UTF8, "application/json"));
                if (response.Contains("FALSE")) return false;
                return true;
            }
            return false;
        }

        #endregion

        #region Работа с нейросетью

        [Route("EditML")]
        public async Task<IActionResult> EditML()
        {
            if (TempData["UploadResult"] != null)
            {
                var isShow = Convert.ToBoolean(TempData["UploadResult"].ToString());
                ViewBag.ShowPopup = isShow;
            }

            var userModel = new AuthUserModel
            {
                Email = HttpContext.Session.GetString("CurrentUserEmail"),
                Password = HttpContext.Session.GetString("CurrentUserPassword")
            };
            var result = await CheckUserFromDb(userModel);
            if (result.Id == Guid.Empty) return RedirectToAction("Auth", "WorkSpace");
            return View("EditML",result);
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 6104857600)]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Content("Файл не выбран");

            var path = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        file.FileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var response = await _httpRequestService.HttpRequestPost(URL.TRAIN_ML, new StringContent(JsonConvert.SerializeObject(path), encoding: System.Text.Encoding.UTF8, "application/json"));

            TempData["UploadResult"] = response;

            return RedirectToAction("EditML");
        }

        #endregion


        #region Работа с календарем

        [HttpPost]
        public async Task<IActionResult> AddEvent([FromBody] EventDTO _event)
        {
            if (ModelState.IsValid)
            {
                _event.EventId = Guid.NewGuid();
                _event.UserId = Guid.Parse(HttpContext.Session.GetString("CurrentUserId"));
                var response = await _httpRequestService.HttpRequestPost(URL.CALENDAR_CREATE_TASK, new StringContent(JsonConvert.SerializeObject(_event), encoding: System.Text.Encoding.UTF8, "application/json"));
                if (response.Contains("FALSE")) return null;
                var responseModel = JsonConvert.DeserializeObject<EventDTO>(response);
                string message = string.Empty;
                return Json(new { message, responseModel.EventId });
            };
            return null;
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
            var response = await _httpRequestService.HttpRequestPost(URL.CALENDAR_GET_TASK, new StringContent(JsonConvert.SerializeObject(currentUser), encoding: System.Text.Encoding.UTF8, "application/json"));
            return response;
        }


        #endregion



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task<UserDto> CheckUserFromDb(AuthUserModel authUser)
        {
            try
            {
                var response = await _httpRequestService.HttpRequestPost(URL.AUTH_CHECK_USER, new StringContent(JsonConvert.SerializeObject(authUser), encoding: System.Text.Encoding.UTF8, "application/json"));
                if (response.Contains("FALSE")) return new UserDto();
                var responseModel = JsonConvert.DeserializeObject<UserDto>(response);
                return responseModel;
            }
            catch (Exception)
            {
                return new UserDto();
            }
            
        }
    }
}
