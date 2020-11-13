using System;
using PAB.Model;

namespace PAB.API.Model
{
    public class GetAllWorkDto : LinkedResourceBaseDto
    {
        public Guid Id { get; set; }
        public string JobTitle { get; set; }
        public string Company { get; set; }
    }
}
