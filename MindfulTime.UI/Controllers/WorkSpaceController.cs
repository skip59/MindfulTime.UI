using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MindfulTime.UI.Interfaces;
using MindfulTime.UI.Models;
using Newtonsoft.Json;
using OpenClasses.Auth;
using OpenClasses.Calendar;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
                if (result.Role == null) return RedirectToAction("Auth", "WorkSpace");
                if (result.Role != string.Empty)
                {
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
        public async Task<IActionResult> WorkSpace(UserDto user)
        {
            //var userModel = new AuthUserModel
            //{
            //    Email = HttpContext.Session.GetString("CurrentUserEmail"),
            //    Password = HttpContext.Session.GetString("CurrentUserPassword")
            //};
            var userModel = GetUserFromCookies();
            var result = await CheckUserFromDb(
                new AuthUserModel 
                { 
                    Email = userModel.Email, 
                    Password = userModel.Password
                });
            if (result.Id == Guid.Empty) return RedirectToAction("Auth", "WorkSpace");
            return View(result);
        }

        [Route("Users")]
        public async Task<IActionResult> EditUsers()
        {
            //string userId = HttpContext.Session.GetString("CurrentUserId");
            //string userRole = HttpContext.Session.GetString("CurrentUserRole");
            //string userEmail = HttpContext.Session.GetString("CurrentUserEmail");
            //string userPassword = HttpContext.Session.GetString("CurrentUserPassword");
            //string userName = HttpContext.Session.GetString("CurrentUserName");

            //if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole) ||
            //    string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userPassword))
            //{
            //    return RedirectToAction("Auth", "WorkSpace");
            //}
            var user = GetUserFromCookies();
            //UserDto userDto = new()
            //{
            //    Id = Guid.Parse(userId),
            //    Role = userRole,
            //    Email = userEmail,
            //    Password = userPassword,
            //    Name = userName
            //};
            var response = await _httpRequestService.HttpRequestPost(URL.AUTH_GET_USERS, new StringContent(JsonConvert.SerializeObject(user), encoding: System.Text.Encoding.UTF8, "application/json"));
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
            return View(user);
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
            if (ModelState.IsValid)
            {
                var response = await _httpRequestService.HttpRequestPost(URL.AUTH_DELETE_USER, new StringContent(JsonConvert.SerializeObject(user), encoding: System.Text.Encoding.UTF8, "application/json"));
                if (response.Contains("FALSE")) return false;
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateUser([FromBody] UserDto user)
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
            var userModel = GetUserFromCookies();
            var result = await CheckUserFromDb(
                new AuthUserModel
                {
                    Email = userModel.Email,
                    Password = userModel.Password
                });
            if (result.Role == "Admin") return RedirectToAction("Auth", "WorkSpace");
            return View("EditML", result);
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
                string role = GetRoleFromToken(responseModel.Token);
                responseModel.Role = role;
                responseModel.Email = authUser.Email;
                responseModel.Password = authUser.Password;
                // Сохранение модели в куках
                var options = new CookieOptions
                {
                    // Устанавливаем время жизни куки (например, 1 день)
                    Expires = DateTime.Now.AddDays(1),
                    IsEssential = true // Для GDPR
                };

                // Сериализуем модель в JSON
                string jsonModel = JsonConvert.SerializeObject(responseModel);

                // Сохраняем JSON в куки
                HttpContext.Response.Cookies.Append("UserDtoCookie", jsonModel, options);

                return responseModel;
            }
            catch (Exception)
            {
                return new UserDto();
            }
        }
        private string GetRoleFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken != null)
            {
                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role");
                if (roleClaim != null)
                {
                    return roleClaim.Value;
                }
            }
            return null;
        }
        private UserDto GetUserFromCookies()
        {
            // Получаем JSON строку из куков
            string jsonModel = HttpContext.Request.Cookies["UserDtoCookie"];

            // Десериализуем JSON строку в объект UserDto
            UserDto userDto = JsonConvert.DeserializeObject<UserDto>(jsonModel);

            return userDto;
        }
    }
}
