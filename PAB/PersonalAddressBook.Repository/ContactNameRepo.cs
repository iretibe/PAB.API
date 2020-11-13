using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PAB.RepositoryInterface;
using System.Linq;
using PAB.Entity;
using PAB.Helper;

namespace PAB.Repository
{
    public class ContactNameRepo : GenericRepository<psPARContactName>, IContactNameRepo
    {
        private PABContext _context;
        public ContactNameRepo(PABContext context) : base(context) => _context = context;
       

        public async Task<bool> CheckContactExistsAsync(string strFirstName, string strLastName, Guid strUserId)
        {
            return await _context.psPARContactName.AnyAsync(o => o.SzFirstName == strFirstName && o.SzLastName == strLastName && o.IUserId == strUserId);
        }

        public async Task<bool> NameExistsAsync(Guid strUserId)
        {
            return await _context.psPARContactName.AnyAsync(o => o.IUserId == strUserId);
        }

        public async Task AddNewContactObject(psPARContactName contactname)
        {
            await _context.psPARContactName.AddAsync(contactname);

            //// saving contact address
            //if (contactname.ContactAddress.Any())
            //{
            //    foreach (var addr in contactname.ContactAddress)
            //    {
            //        if (addr.SzBusinessAddress == null || addr.SzHomeAddress == null || addr.SzOther == null)
            //        {
            //            _context.psPARContactAddress.Remove(addr);
            //        }
            //    }
            //}

            //// saving contact email
            //if (contactname.ContactEmail.Any())
            //{
            //    foreach (var email in contactname.ContactEmail)
            //    {
            //        if (email.SzEmailAddress1 == null || email.SzEmailAddress2 == null)
            //        {
            //            _context.psPARContactEmail.Remove(email);
            //        }
            //    }
            //}

            //// saving contact other
            //if (contactname.ContactOther.Any())
            //{
            //    foreach (var other in contactname.ContactOther)
            //    {
            //        if (other.SzPersonalWebPage == null || other.SzSignificantOther == null
            //            || other.DBirthday == null || other.DAnniversary == null)  //!other.DAnniversary.HasValue
            //        {
            //            _context.psPARContactOther.Remove(other);
            //        }
            //    }
            //}

            //// saving contact phone
            //if (contactname.ContactPhone.Any())
            //{
            //    foreach (var phone in contactname.ContactPhone)
            //    {
            //        if (phone.SzBusiness == null || phone.SzBusinessFax == null
            //            || phone.SzHome == null || phone.SzHomeFax == null
            //            || phone.SzMobile1 == null || phone.SzMobile2 == null)
            //        {
            //            _context.psPARContactPhone.Remove(phone);
            //        }
            //    }
            //}

            //// saving contact work
            //if (contactname.ContactWork.Any())
            //{
            //    foreach (var work in contactname.ContactWork)
            //    {
            //        if (work.SzCompany == null || work.SzJobTitle == null)
            //        {
            //            _context.psPARContactWork.Remove(work);
            //        }
            //    }
            //}
        }

        public async Task<psPARContactName> GetContactById(Guid id, Guid strUserId)
        {
            //return await _context.Set<psPARContactName>().AsNoTracking()
            //    .Include(e => e.ContactAddress)
            //    .Include(e => e.ContactEmail)
            //    .Include(e => e.ContactOther)
            //    .Include(e => e.ContactPhone)
            //    .Include(e => e.ContactWork)
            //    .SingleOrDefaultAsync(e => e.pkId == id && e.IUserId == strUserId);

            return await _context.psPARContactName.Where(e => e.pkId == id && e.IUserId == strUserId)
                .Include(e => e.ContactAddress)
                .Include(e => e.ContactEmail)
                .Include(e => e.ContactOther)
                .Include(e => e.ContactPhone)
                .Include(e => e.ContactWork)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<psPARContactName>> GetContactByUserId(Guid strUserId)
        {
            return await _context.psPARContactName.Where(e => e.IUserId == strUserId)
                .Include(e => e.ContactAddress)
                .Include(e => e.ContactEmail)
                .Include(e => e.ContactOther)
                .Include(e => e.ContactPhone)
                .Include(e => e.ContactWork).ToListAsync();

            //return await _context.Cities.OrderBy(c => c.Name).ToListAsync();  //ToList is necessary
        }

        public async Task<IEnumerable<psPARContactName>> GetContactByUserIdError(Guid strUserId)
        {
            return await _context.psPARContactName.Where(e => e.IUserId == strUserId)
                .Include(e => e.ContactAddress)
                .Include("garbage")
                .Include(e => e.ContactEmail)
                .Include(e => e.ContactOther)
                .Include(e => e.ContactPhone)
                .Include(e => e.ContactWork).ToListAsync();

            //return await _context.Cities.OrderBy(c => c.Name).ToListAsync();  //ToList is necessary
        }

        public async Task<PagedList<psPARContactName>> SearchContactsAsync(Guid userId, ResourceParameters resourceParameters)
        {
            var collectionBeforePaging = _context.psPARContactName
                .Include(a => a.ContactAddress)
                .Include(e => e.ContactEmail)
                .Include(o => o.ContactOther)
                .Include(p => p.ContactPhone)
                .Include(w => w.ContactWork)
                .Where(o => o.IUserId == userId)
                .OrderBy(o => o.SzFirstName).AsQueryable();


            if (!string.IsNullOrEmpty(resourceParameters.SearchQuery))
            {
                // trim & ignore casing
                var searchQueryForWhereClause = resourceParameters.SearchQuery
                    .Trim().ToLowerInvariant();

                collectionBeforePaging = collectionBeforePaging
                    .Where(o => o.SzFirstName.ToLowerInvariant().Contains(searchQueryForWhereClause)
                                || o.SzFirstName.ToLowerInvariant().Contains(searchQueryForWhereClause));
                                //|| o.ContactPhone ToLowerInvariant().Contains(searchQueryForWhereClause)
            }

            return await PagedList<psPARContactName>.Create(collectionBeforePaging,
                resourceParameters.PageNumber,
                resourceParameters.PageSize);
        }

        public void DeleteContact(psPARContactName contact)
        {
            _context.Set<psPARContactName>().Remove(contact);
        }

        public IQueryable<psPARContactName> GetContactsByUserIdAsync(Guid contactId, Guid userId)
        {

            var codes =  _context.psPARContactName
                .Include(a => a.ContactAddress)
                .Include(e => e.ContactEmail)
                .Include(o => o.ContactOther)
                .Include(p => p.ContactPhone)
                .Include(w => w.ContactWork)
                .Where(o => o.IUserId == userId && o.pkId == contactId)
                    .OrderBy(o => o.SzFirstName).AsQueryable();

            return codes;

            //    .Where(o => o.ICompanyId == lngcompanyId && o.SzType == strtype && o.IStatus == lngStatus)
            //    .OrderBy(o => (o.SzDescription)).ToListAsync();


            //var collectionBeforePaging = _context.psPARContactName
            //    .Include(a => a.ContactAddress)
            //    .Include(e => e.ContactEmail)
            //    .Include(o => o.ContactOther)
            //    .Include(p => p.ContactPhone)
            //    .Include(w => w.ContactWork)
            //    .Where(o => o.IUserId == userId && o.pkId == contactId)
            //    .OrderBy(o => o.SzFirstName).AsQueryable();


            //return await collectionBeforePaging;

            //return await PagedList<psPARContactName>.Create(collectionBeforePaging,
            //    resourceParameters.PageNumber,
            //    resourceParameters.PageSize);
        }

        public async Task UpdateContact(psPARContactName contactname)
        {
            var lngContactId = contactname.pkId;

            // saving contact address
            if (contactname.ContactAddress.Any())
            {
                foreach (var addr in contactname.ContactAddress)
                {
                    var newAddress = new psPARContactAddress
                    {
                        IContactNameId = lngContactId,
                        SzBusinessAddress = addr.SzBusinessAddress,
                        SzOther = addr.SzOther,
                        SzHomeAddress = addr.SzHomeAddress
                    };

                    _context.psPARContactAddress.Remove(addr);

                    await _context.psPARContactAddress.AddAsync(newAddress);
                }
            }

            // saving contact email
            if (contactname.ContactEmail.Any())
            {
                foreach (var email in contactname.ContactEmail)
                {
                    var newEmailAddress = new psPARContactEmail
                    {
                        IContactNameId = lngContactId,
                        SzEmailAddress1 = email.SzEmailAddress1,
                        SzEmailAddress2 = email.SzEmailAddress2
                    };

                    _context.psPARContactEmail.Remove(email);

                    await _context.psPARContactEmail.AddAsync(newEmailAddress);
                }
            }

            // saving contact other
            if (contactname.ContactOther.Any())
            {
                foreach (var other in contactname.ContactOther)
                {
                    var newOther = new psPARContactOther
                    {
                        IContactNameId = lngContactId,
                        DAnniversary = other.DAnniversary,
                        DBirthday = other.DBirthday,
                        SzPersonalWebPage = other.SzPersonalWebPage,
                        SzSignificantOther = other.SzSignificantOther
                    };

                    _context.psPARContactOther.Remove(other);

                    await _context.psPARContactOther.AddAsync(newOther);
                }
            }

            // saving contact phone
            if (contactname.ContactPhone.Any())
            {
                foreach (var phone in contactname.ContactPhone)
                {
                    var newPhone = new psPARContactPhone
                    {
                        IContactNameId = lngContactId,
                        SzBusiness = phone.SzBusiness,
                        SzBusinessFax = phone.SzBusinessFax,
                        SzHome = phone.SzHome,
                        SzHomeFax = phone.SzHomeFax,
                        SzMobile1 = phone.SzMobile1,
                        SzMobile2 = phone.SzMobile2
                    };

                    _context.psPARContactPhone.Remove(phone);

                    await _context.psPARContactPhone.AddAsync(newPhone);
                }
            }

            // saving contact work
            if (contactname.ContactWork.Any())
            {
                foreach (var work in contactname.ContactWork)
                {
                    var newWork = new psPARContactWork
                    {
                        IContactNameId = lngContactId,
                        SzCompany = work.SzCompany,
                        SzJobTitle = work.SzJobTitle
                    };

                    _context.psPARContactWork.Remove(work);

                    await _context.psPARContactWork.AddAsync(newWork);
                }
            }
        }
    }
}
