using System;
using System.Collections.Generic;

namespace PAB.API.Model
{
    public class ContactForCreationDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string NickName { get; set; }
        public string Title { get; set; }
        public string Suffix { get; set; }
        public Guid UserId { get; set; }

        public ICollection<AddressForCreationDto> ContactAddress { get; set; } = new List<AddressForCreationDto>();
        public ICollection<EmailForCreationDto> ContactEmail { get; set; } = new List<EmailForCreationDto>();
        public ICollection<OtherForCreationDto> ContactOther { get; set; } = new List<OtherForCreationDto>();
        public ICollection<PhoneForCreationDto> ContactPhone { get; set; } = new List<PhoneForCreationDto>();
        public ICollection<WorkForCreationDto> ContactWork { get; set; } = new List<WorkForCreationDto>();
    }
}
