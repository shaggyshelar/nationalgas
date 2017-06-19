using System;
using System.ComponentModel.DataAnnotations;

namespace NG.Domain.Core
{
    public class AppUser
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        public DateTime LastLogin { get; set; }

        [Required]
        public int FailedPasswordAttemptCount { get; set; }
    }
}