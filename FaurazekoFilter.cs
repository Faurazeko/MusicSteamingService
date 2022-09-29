using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace MusicSteamingService.Filters
{
    public class FaurazekoFilter : Attribute, IResourceFilter
    {
        public void OnResourceExecuted(ResourceExecutedContext context) { }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                Deny(context);
                return;
            }

            var username = context.HttpContext.User.FindFirst("username").Value;

            if (username.ToLower() != "faurazeko")
            {
                Deny(context);
                return;
            }

        }

        private void Deny(ResourceExecutingContext context)
        {
            context.Result = new ContentResult { StatusCode = 404, Content = "Status Code: 404; Not Found", ContentType = "text/plain" };
        }
    }
}