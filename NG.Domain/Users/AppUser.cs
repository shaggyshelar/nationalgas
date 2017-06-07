using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace NG.Domain.Users
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime LastLogin { get; set; }

        public int FailedPasswordAttemptCount { get; set; }
    }
}