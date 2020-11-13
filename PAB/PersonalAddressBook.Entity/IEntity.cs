using System;
using System.Collections.Generic;
using System.Text;

namespace PAB.Entity
{
    public interface IEntity
    {
        Guid pkId { get; set; }
    }
}
