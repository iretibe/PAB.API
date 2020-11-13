using System;

namespace PAB.Entity
{
    public partial class psPARContactOther : IEntity
    {
        public Guid pkId { get; set; }
        public Guid IContactNameId { get; set; }
        public string SzPersonalWebPage { get; set; }
        public string SzSignificantOther { get; set; }
        public DateTime? DBirthday { get; set; }
        public DateTime? DAnniversary { get; set; }

        public psPARContactName ContactName { get; set; }
    }
}
