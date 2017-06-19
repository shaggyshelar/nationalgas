using System.ComponentModel.DataAnnotations;

namespace NG.Service.Controllers.Core
{
    public class AppUserForCreationDto
    {
        [RequiredAttribute(ErrorMessage = "Please Enter the First Name")]
        [MaxLengthAttribute(50, ErrorMessage = "First Name can not be greter than 50 characters")]
        public string FirstName { get; set; }
        [RequiredAttribute(ErrorMessage = "Please Enter the Last Name")]
        [MaxLengthAttribute(50, ErrorMessage = "Last name can not be greter than 50 characters")]
        public string LastName { get; set; }
        [RequiredAttribute(ErrorMessage = "Please Enter Email")]
        [MaxLengthAttribute(50, ErrorMessage = "Email can not be greter than 50 characters")]
        public string Email { get; set; }
        [RequiredAttribute(ErrorMessage = "Please Enter the Username")]
        [MaxLengthAttribute(50, ErrorMessage = "Username can not be greter than 50 characters")]
        public string UserName { get; set; }
    }
}