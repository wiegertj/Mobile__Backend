﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    [Table("User_to_Group")]
    public class UserToGroup : IEntity
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long GroupId { get; set; }
    }
}
