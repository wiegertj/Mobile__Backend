using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    [Table("User_To_Subgroup")]
    public class UserToSubgroup : IEntity
    {
        public long Id { get; set; }
        public long SubgroupId { get; set; }
        public long UserId { get; set; }
    }
}
