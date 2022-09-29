using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicStreamingService.Data;

namespace MusicStreamingService.Controllers
{
	[Route("api/user")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class UserApiController : Controller
	{
        private readonly IRepository _repository;

        public UserApiController(IRepository repository) => _repository = repository;

        [NonAction]
        public string GetUsername() => User.FindFirst("username")!.Value;

        [HttpGet]
		public IActionResult Get()
		{
            var username = GetUsername();
            var user = _repository.GetUser(username);
            user.Password = "************";
            return new JsonResult(user);
		}

        [HttpPut]
        public IActionResult ChangePassword([FromQuery] string newPassword)
        {
            if(newPassword.Length < 8 || newPassword.Length > 255)
                return BadRequest("Password length should be grater than 8 and no more than 255.");

            var username = GetUsername();
            var user = _repository.GetUser(username);

            user.Password = newPassword;
            user.ForcedLogoutTime = DateTime.UtcNow;

            _repository.SaveChanges();

            return Ok();
        }

        [HttpPut("playlist")]
        public IActionResult ChangePlaylistProperties([FromQuery] bool visibility)
        {
            var username = GetUsername();
            var user = _repository.GetUser(username);

            user.IsPlaylistPublic = visibility;

            _repository.SaveChanges();

            return Ok();
        }

        [HttpDelete]
        public IActionResult DeleteAccount()
        {
            var username = GetUsername();

            _repository.DeleteUser(username);
            _repository.SaveChanges();

            return Ok();
        }
    }
}
