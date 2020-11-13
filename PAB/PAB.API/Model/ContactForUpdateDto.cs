using System;
using System.Collections.Generic;

namespace PAB.API.Model
{
    public class ContactForUpdateDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string NickName { get; set; }
        public string Title { get; set; }
        public string Suffix { get; set; }
        public Guid UserId { get; set; }

        public ICollection<AddressForUpdateDto> ContactAddress { get; set; } = new List<AddressForUpdateDto>();
        public ICollection<EmailForUpdateDto> ContactEmail { get; set; } = new List<EmailForUpdateDto>();
        public ICollection<OtherForUpdateDto> ContactOther { get; set; } = new List<OtherForUpdateDto>();
        public ICollection<PhoneForUpdateDto> ContactPhone { get; set; } = new List<PhoneForUpdateDto>();
        public ICollection<WorkForUpdateDto> ContactWork { get; set; } = new List<WorkForUpdateDto>();
    }
}
