using System;

namespace PAB.API.Model
{
    public class PhoneForUpdateDto
    {
        //public Guid Id { get; set; }
        public Guid ContactNameId { get; set; }
        public string Mobile1 { get; set; }
        public string Mobile2 { get; set; }
        public string Home { get; set; }
        public string Business { get; set; }
        public string BusinessFax { get; set; }
        public string HomeFax { get; set; }
    }
}
