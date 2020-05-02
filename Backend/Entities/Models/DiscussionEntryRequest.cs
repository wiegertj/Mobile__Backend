using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.Models
{
    public class DiscussionEntryRequest
    {
        public long GroupId { get; set; }
        public bool SinceLastFetch { get; set; }
    }
}
