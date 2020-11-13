using System;
using PAB.Model;

namespace PAB.API.Model
{
    public class GetAllAddressDto : LinkedResourceBaseDto
    {
        public Guid Id { get; set; }
        //public Guid ContactNameId { get; set; }
        public string HomeAddress { get; set; }
        public string BusinessAddress { get; set; }
        public string Other { get; set; }
    }
}
