using System;
using System.Collections.Generic;

namespace PAB.Entity
{
    public class psPARContactName : IEntity
    {
        public psPARContactName()
        {
            ContactAddress = new List<psPARContactAddress>();
            ContactEmail = new List<psPARContactEmail>();
            ContactOther = new List<psPARContactOther>();
            ContactPhone = new List<psPARContactPhone>();
            ContactWork = new List<psPARContactWork>();
        }

        public Guid pkId { get; set; }
        public string SzFirstName { get; set; }
        public string SzLastName { get; set; }
        public string SzMiddleName { get; set; }
        public string SzNickName { get; set; }
        public string SzTitle { get; set; }
        public string SzSuffix { get; set; }
        public DateTime? DCreatedate { get; set; }
        public Guid IUserId { get; set; }

        public ICollection<psPARContactAddress> ContactAddress { get; set; }
        public ICollection<psPARContactEmail> ContactEmail { get; set; }
        public ICollection<psPARContactOther> ContactOther { get; set; }
        public ICollection<psPARContactPhone> ContactPhone { get; set; }
        public ICollection<psPARContactWork> ContactWork { get; set; }

        //public psPARContactAddress ContactAddress { get; set; }
        //public psPARContactEmail ContactEmail { get; set; }
        //public psPARContactOther ContactOther { get; set; }
        //public psPARContactPhone ContactPhone { get; set; }
        //public psPARContactWork ContactWork { get; set; }
    }
}
