using System.Collections.Generic;

namespace PAB.Model
{
    public abstract class LinkedResourceBaseDto
    {
        public List<LinkDto> Links { get; set; }
            = new List<LinkDto>();
    }
}
