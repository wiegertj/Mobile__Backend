using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    [Table("LastSubGroupFetch")]
    public class LastSubGroupFetch : IEntity
    {
        [Key]
        public long Id { get; set; }

        public long UserId { get; set; }
        public long SubGroupId { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
