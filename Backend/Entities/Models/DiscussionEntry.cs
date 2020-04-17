﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    [Table("Discussion_Entry")]
    public class DiscussionEntry : IEntity
    {
        [Key]
        public long Id { get; set; }
        public long? AnswerTo { get; set; }
        public string Text { get; set; }
        public long? Subgroup { get; set; }
        public long? NormalGroup { get; set; }

        public int? FileId { get; set; }
        [ForeignKey("FileId")]
        public File File { get; set; }
    }
}
