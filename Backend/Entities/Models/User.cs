using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models
{
    [Table("User")]
    public class User : IEntity
    {
        [Key]
        public long Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long University_Id { get; set; }
        public string Course_of_Studies { get; set; }
    }
}
