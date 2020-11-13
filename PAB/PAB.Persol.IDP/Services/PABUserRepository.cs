using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PAB.Persol.IDP.Entities;

namespace PAB.Persol.IDP.Services
{
    public class PABUserRepository: IPABUserRepository
    {
        PABUserContext _context;
        public PABUserRepository(PABUserContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserByUsername(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> GetUserBySubjectId(string subjectId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.SubjectId == subjectId);
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Claims.Any(c => c.ClaimType == "email" && c.ClaimValue == email));
        }

        public async Task<User> GetUserByProvider(string loginProvider, string providerKey)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Logins.Any(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey));
        }

        public async Task<IEnumerable<UserLogin>> GetUserLoginsBySubjectId(string subjectId)
        {
            var user = await _context.Users.Include("Logins").FirstOrDefaultAsync(u => u.SubjectId == subjectId);
            return user?.Logins.ToList() ?? new List<UserLogin>();
        }

        public async Task<IEnumerable<UserClaim>> GetUserClaimsBySubjectId(string subjectId)
        {
            // get user with claims
            var user = await _context.Users.Include("Claims").FirstOrDefaultAsync(u => u.SubjectId == subjectId);
            if (user == null)
            {
                return new List<UserClaim>();
            }
            return user.Claims.ToList();
        }

        public async Task<bool> AreUserCredentialsValid(string username, string password)
        {
            // get the user
            var user = await GetUserByUsername(username);

            return user.Password == password && !string.IsNullOrWhiteSpace(password);
        }

        public async Task<bool> IsUserActive(string subjectId)
        {
            var user = await GetUserBySubjectId(subjectId);
            return user.IsActive;
        }

        public void AddUser(User user)
        {
            _context.Users.Add(user);
        }

        public async Task AddUserLogin(string subjectId, string loginProvider, string providerKey)
        {
            var user = await GetUserBySubjectId(subjectId);
            if (user == null)
            {
                throw new ArgumentException("User with given subjectId not found.", subjectId);
            }

            user.Logins.Add(new UserLogin
            {
                SubjectId = subjectId,
                LoginProvider = loginProvider,
                ProviderKey = providerKey
            });
        }

        public async Task AddUserClaim(string subjectId, string claimType, string claimValue)
        {
            var user = await GetUserBySubjectId(subjectId);
            if (user == null)
            {
                throw new ArgumentException("User with given subjectId not found.", subjectId);
            }

            user.Claims.Add(new UserClaim(claimType, claimValue));
        }

        public async Task<bool> Save()
        {
            return (await _context.SaveChangesAsync() >= 0);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (_context == null) return;
            _context.Dispose();
            _context = null;
        }
    }
}
