using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PAB.Persol.IDP.Entities;

namespace PAB.Persol.IDP.Services
{
    // ReSharper disable once InconsistentNaming
    public interface IPABUserRepository
    {
        Task<User> GetUserByUsername(string username);
        Task<User> GetUserBySubjectId(string subjectId);
        Task<User> GetUserByEmail(string email);
        Task<User> GetUserByProvider(string loginProvider, string providerKey);
        Task<IEnumerable<UserLogin>> GetUserLoginsBySubjectId(string subjectId);
        Task<IEnumerable<UserClaim>> GetUserClaimsBySubjectId(string subjectId);
        Task<bool> AreUserCredentialsValid(string username, string password);
        Task<bool> IsUserActive(string subjectId);
        void AddUser(User user);
        Task AddUserLogin(string subjectId, string loginProvider, string providerKey);
        Task AddUserClaim(string subjectId, string claimType, string claimValue);
        Task<bool> Save();
    }
}
