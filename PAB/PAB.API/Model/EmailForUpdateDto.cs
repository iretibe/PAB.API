using System;

namespace PAB.API.Model
{
    public class EmailForUpdateDto
    {
        //public Guid Id { get; set; }
        public Guid ContactNameId { get; set; }
        public string EmailAddress1 { get; set; }
        public string EmailAddress2 { get; set; }
    }
}
