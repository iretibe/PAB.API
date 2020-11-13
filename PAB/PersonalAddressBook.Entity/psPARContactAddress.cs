using System;
using System.Collections.Generic;

namespace PAB.Entity
{
    public class psPARContactAddress : IEntity
    {
        public Guid pkId { get; set; }
        public Guid IContactNameId { get; set; }
        public string SzHomeAddress { get; set; }
        public string SzBusinessAddress { get; set; }
        public string SzOther { get; set; }

        public psPARContactName ContactName { get; set; }
    }
}
