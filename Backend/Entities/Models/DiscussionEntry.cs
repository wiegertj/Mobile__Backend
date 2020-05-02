using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    [Table("Discussion_Entry")]
    public class DiscussionEntry : IEntity
    {
        public DiscussionEntry()
        {
            Answers = new HashSet<DiscussionEntry>();
        }

        [Key]
        public long Id { get; set; }
        public string Topic { get; set; }
        public string Text { get; set; }
        public DateTime? TimeStamp { get; set; }
        public long? Subgroup { get; set; }
        public long? NormalGroup { get; set; }

        public long? UserId { get; set; }

        public long? AnswerTo { get; set; }
        public ICollection<DiscussionEntry> Answers { get; set; }

        public long? FileId { get; set; }
        [ForeignKey("FileId")]
        public File File { get; set; }
    }
}
