﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    [Table("University")]
    public class University : IEntity
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
