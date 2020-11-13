using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace PAB.API.Helpers
{
    public class SwaggerSecurityOperationFilter : IOperationFilter
    {
        private readonly IOptions<AuthorizationOptions> _authorizationOptions;
        private readonly string _apiScope;

        public SwaggerSecurityOperationFilter(string apiScope,
            IOptions<AuthorizationOptions> authorizationOptions)
        {
            _apiScope = apiScope;
            this._authorizationOptions = authorizationOptions;
        }

        public void Apply(Operation operation, OperationFilterContext context)
        {
            var requiredClaimTypes = new List<string> { _apiScope };

            var controllerPolicies = context.ApiDescription.ControllerAttributes();
            var actionPolicies = context.ApiDescription.ActionAttributes();
            var policies = controllerPolicies.Union(actionPolicies).Distinct();

            if (policies.OfType<AllowAnonymousAttribute>().Any())
                return;  // must be an anonymous method

            //NOTE: If you have some policies that define claims required you should work this 
            //     code out.  The resource scope of "api" was sufficient for the demo.

            //var specificClaims = policies.OfType<AuthorizeAttribute>().Select(a => a.Policy)
            //        .Select(x => authorizationOptions.Value.GetPolicy(x))
            //        .SelectMany(x => x.Requirements)
            //        .OfType<ClaimsAuthorizationRequirement>()
            //        .Select(x => x.ClaimType);

            //requiredClaimTypes.AddRange(specificClaims);                

            if (requiredClaimTypes.Any())
            {
                operation.Responses.Add("401", new Response { Description = "Unauthorized" });
                operation.Responses.Add("403", new Response { Description = "Forbidden" });

                operation.Security = new List<IDictionary<string, IEnumerable<string>>>
                {
                    new Dictionary<string, IEnumerable<string>>
                    {
                        {"oauth2", requiredClaimTypes} //pabapi oauth2
                    }
                };
            }
        }
    }
}
