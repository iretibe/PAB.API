﻿using System;

namespace PAB.API.Model
{
    public class OtherForUpdateDto
    {
        //public Guid Id { get; set; }
        public Guid ContactNameId { get; set; }
        public string PersonalWebPage { get; set; }
        public string SignificantOther { get; set; }
        public DateTime? Birthday { get; set; }
        public DateTime? Anniversary { get; set; }
    }
}
