using System;
using PAB.Model;

namespace PAB.API.Model
{
    public class GetAllOtherDto : LinkedResourceBaseDto
    {
        public Guid Id { get; set; }
        public string PersonalWebPage { get; set; }
        public string SignificantOther { get; set; }
        public DateTime? Birthday { get; set; }
        public DateTime? Anniversary { get; set; }
    }
}
