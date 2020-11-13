using System;

namespace PAB.API.Model
{
    public class AddressForCreationDto
    {
        //public Guid ContactNameId { get; set; }
        public string HomeAddress { get; set; }
        public string BusinessAddress { get; set; }
        public string Other { get; set; }
    }
}
