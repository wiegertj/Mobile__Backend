using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    public class DiscussionEntryReturnType
    {
        public DiscussionEntry discussionEntry { get; set; }
        public string UserName { get; set; }
    }
}
