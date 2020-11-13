using System;
using PAB.Model;

namespace PAB.API.Model
{
    public class GetAllEmailDto : LinkedResourceBaseDto
    {
        public Guid Id { get; set; }
        //public Guid ContactNameId { get; set; }
        public string EmailAddress1 { get; set; }
        public string EmailAddress2 { get; set; }
    }
}
