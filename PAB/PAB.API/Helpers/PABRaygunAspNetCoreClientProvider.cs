using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Mindscape.Raygun4Net;
using Mindscape.Raygun4Net.Messages;


namespace PAB.API.Helpers
{
    public class PABRaygunAspNetCoreClientProvider : DefaultRaygunAspNetCoreClientProvider
    {
        public override RaygunClient GetClient(RaygunSettings settings, HttpContext context)
        {
            var client = base.GetClient(settings, context);
            client.ApplicationVersion = "1.1.0";

            var identity = context?.User?.Identity as ClaimsIdentity;
            if (identity?.IsAuthenticated == true)
            {
                var email = identity.Claims.Where(c => c.Type == ClaimTypes.Email).Select(c => c.Value).FirstOrDefault();
                var fullName = identity.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).FirstOrDefault();

                client.UserInfo = new RaygunIdentifierMessage(email)
                {
                    IsAnonymous = false,
                    Email = email,
                    FullName = fullName //identity.Name
                };
            }

            return client;
        }
    }
}
