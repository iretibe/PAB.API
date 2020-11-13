using Microsoft.Extensions.DependencyInjection;
using PAB.Persol.IDP.Services;

namespace PAB.Persol.IDP
{
    public static class IdentityServerBuilderExtensions
    {
        // ReSharper disable once InconsistentNaming
        public static IIdentityServerBuilder AddPABUserStore(this IIdentityServerBuilder builder)
        {
            builder.Services.AddSingleton<IPABUserRepository, PABUserRepository>();
            builder.AddProfileService<PABUserProfileService>();
            return builder;
        }
    }
}
