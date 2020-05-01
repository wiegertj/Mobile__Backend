using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    [Table("LastGroupFetch")]
    public class LastGroupFetch : IEntity
    {
        [Key]
        public long Id { get; set; }

        public long UserId { get; set; }
        public long GroupId { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
