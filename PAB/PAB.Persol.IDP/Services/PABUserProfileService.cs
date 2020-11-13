using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace PAB.Persol.IDP.Services
{
    // ReSharper disable once InconsistentNaming
    public class PABUserProfileService : IProfileService
    {
        private readonly IPABUserRepository _pabUserRepository;
        public PABUserProfileService(IPABUserRepository pabUserRepository)
        {
            _pabUserRepository = pabUserRepository;
        }
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subjectId = context.Subject.GetSubjectId();
            var claimsForUser = await _pabUserRepository.GetUserClaimsBySubjectId(subjectId);

            context.IssuedClaims = claimsForUser.Select
                (c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var subjectId = context.Subject.GetSubjectId();
            context.IsActive = await _pabUserRepository.IsUserActive(subjectId);
        }
    }
}
