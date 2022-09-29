using Microsoft.AspNetCore.Mvc;
using MusicStreamingService.Data;

namespace MusicSteamingService.Controllers
{
    [Route("api/secretadmin")]
    [ApiController]
    [Filters.FaurazekoFilter]
    public class AdminApiController : ControllerBase
    {
        private readonly IRepository _repository;
        public AdminApiController(IRepository repository) => _repository = repository;

        [HttpGet("user")]
        public IActionResult GetUsers()
        {
            var users = _repository.GetUsers();

            return Ok(users);
        }

        [HttpDelete("user/{username}")]
        public IActionResult DeleteUser(string username)
        {
            _repository.DeleteUser(username.ToLower());
            _repository.SaveChanges();

            return Ok();
        }

        [HttpPost("user/{username}")]
        //also can be used to change password
        public IActionResult CreateUser(string username, [FromForm] string password)
        {
            _repository.CreateUser(username, password);
            _repository.SaveChanges();

            return Ok();
        }
    }
}
