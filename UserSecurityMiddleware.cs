using MusicStreamingService.Data;

namespace MusicStreamingService
{
    public class UserSecurityMiddleware
    {
        private IRepository _repository;
        private IServiceScope _scope;
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        public UserSecurityMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _scope = _serviceProvider.CreateScope();
            _repository = _scope.ServiceProvider.GetService<IRepository>()!;

            if (context.User.Identity!.IsAuthenticated)
            {
                var loginTime = Convert.ToDateTime(context.User.FindFirst("loginUtcDateTime")!.Value);
                var username = context.User.FindFirst("username")!.Value.ToLower();

                var user = _repository.GetUser(username);

                if (user == null)
                    context.Request.Path = "/logout";
                else if (loginTime < user.ForcedLogoutTime)
                    context.Response.Redirect("/logout");

            }

            await _next.Invoke(context);
        }
    }
}
