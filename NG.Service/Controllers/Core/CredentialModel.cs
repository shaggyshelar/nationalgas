using System.ComponentModel.DataAnnotations;

namespace NG.Service.Controllers.Core
{
    public class CredentialModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}