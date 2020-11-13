using System;

namespace PAB.Entity
{
    public class psPARContactPhone : IEntity
    {
        public Guid pkId { get; set; }
        public Guid IContactNameId { get; set; }
        public string SzMobile1 { get; set; }
        public string SzMobile2 { get; set; }
        public string SzHome { get; set; }
        public string SzBusiness { get; set; }
        public string SzBusinessFax { get; set; }
        public string SzHomeFax { get; set; }

        public psPARContactName ContactName { get; set; }
    }
}
