using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PAB.Entity;
using PAB.Helper;

namespace PAB.RepositoryInterface
{
    public interface IContactNameRepo : IGenericRepository<psPARContactName>
    {
        Task<bool> CheckContactExistsAsync(string strFirstName, string strLastName, Guid strUserId);
        Task<bool> NameExistsAsync(Guid strUserId);
        Task AddNewContactObject(psPARContactName contactname);
        Task<psPARContactName> GetContactById(Guid id, Guid strUserId);
        Task<IEnumerable<psPARContactName>> GetContactByUserId(Guid strUserId);
        Task<IEnumerable<psPARContactName>> GetContactByUserIdError(Guid strUserId);
        Task<PagedList<psPARContactName>> SearchContactsAsync(Guid userId, ResourceParameters resourceParameters);
        void DeleteContact(psPARContactName contact);
        IQueryable<psPARContactName> GetContactsByUserIdAsync(Guid contactId, Guid userId);
        Task UpdateContact(psPARContactName contactname);
    }
}
