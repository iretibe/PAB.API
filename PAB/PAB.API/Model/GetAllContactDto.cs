using System;
using System.Collections.Generic;
using PAB.Model;

namespace PAB.API.Model
{
    public class GetAllContactDto : LinkedResourceBaseDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string NickName { get; set; }
        public string Title { get; set; }
        public string Suffix { get; set; }
        public Guid UserId { get; set; }

        public ICollection<GetAllAddressDto> ContactAddress { get; set; } = new List<GetAllAddressDto>();
        public ICollection<GetAllEmailDto> ContactEmail { get; set; } = new List<GetAllEmailDto>();
        public ICollection<GetAllOtherDto> ContactOther { get; set; } = new List<GetAllOtherDto>();
        public ICollection<GetAllPhoneDto> ContactPhone { get; set; } = new List<GetAllPhoneDto>();
        public ICollection<GetAllWorkDto> ContactWork { get; set; } = new List<GetAllWorkDto>();
    }
}
