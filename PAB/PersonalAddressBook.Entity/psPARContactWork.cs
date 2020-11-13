using System;

namespace PAB.Entity
{
    public class psPARContactWork : IEntity
    {
        public Guid pkId { get; set; }
        // ReSharper disable once InconsistentNaming
        public Guid IContactNameId { get; set; }
        public string SzJobTitle { get; set; }
        public string SzCompany { get; set; }

        public psPARContactName ContactName { get; set; }
    }
}
