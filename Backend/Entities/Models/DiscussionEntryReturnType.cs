using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    public class DiscussionEntryReturnType
    {
        public long Id { get; set; }
        public string Topic { get; set; }
        public string Text { get; set; }
        public DateTime? TimeStamp { get; set; }
        public long? Subgroup { get; set; }
        public long? NormalGroup { get; set; }
        public long? UserId { get; set; }
        public long? AnswerTo { get; set; }
        public File File { get; set; }
        public string UserName { get; set; }
    }
}
