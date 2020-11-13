using System;
using System.Collections.Generic;

namespace PAB.Entity
{
    public class psPARContactEmail : IEntity
    {
        public Guid pkId { get; set; }
        public Guid IContactNameId { get; set; }
        public string SzEmailAddress1 { get; set; }
        public string SzEmailAddress2 { get; set; }

        public psPARContactName ContactName { get; set; }
    }
}
