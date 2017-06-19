using System;
using System.ComponentModel.DataAnnotations;
using NG.Service.Core;

namespace NG.Service.Controllers.Customers
{
    public class CustomerForUpdationDto : BaseDto
    {
        public CustomerForUpdationDto()
        {
            this.CreatedOn = null;
            this.UpdatedOn = DateTime.Now;
        }

        [RequiredAttribute(ErrorMessage = "Please Enter the National ID")]
        [MaxLengthAttribute(10, ErrorMessage = "National ID Can not be greter than 10 characters")]
        public string NationalID { get; set; }

        [RequiredAttribute(ErrorMessage = "Please Enter the Serial Number")]
        [MaxLengthAttribute(10, ErrorMessage = "Serial Number Can not be greter than 10 characters")]
        public string SerialNumber { get; set; }

        [RequiredAttribute(ErrorMessage = "Please Enter First Name.")]
        [MaxLengthAttribute(50, ErrorMessage = "First Name cannot be greater than 50 characters.")]
        public string Firstname { get; set; }

        [RequiredAttribute(ErrorMessage = "Please Enter Surname.")]
        [MaxLengthAttribute(50, ErrorMessage = "SurName cannot be greater than 50 characters.")]
        public string SurName { get; set; }

        [MaxLengthAttribute(50, ErrorMessage = "Other Name cannot be greater than 50 characters.")]
        public string Othername { get; set; }

        [RequiredAttribute(ErrorMessage = "Please enter mobile number.")]
        [MaxLengthAttribute(20, ErrorMessage = "Mobile number cannot be greater than 20 characters.")]
        public string Mobile { get; set; }

        [RequiredAttribute(ErrorMessage = "Pelase enter email address")]
        [MaxLengthAttribute(50, ErrorMessage = "Email cannot be greater than 50 characters.")]
        public string Email { get; set; }

        [RequiredAttribute(ErrorMessage = "Pelase Select Gender")]
        [MaxLengthAttribute(10, ErrorMessage = "Gender cannot be greater than 10 characters.")]
        public string Gender { get; set; }

        [RequiredAttribute(ErrorMessage = "Please enter date of birth")]
        public DateTime DateOfBirth { get; set; }

        [MaxLengthAttribute(20, ErrorMessage = "Citizenship cannot be greater than 20 characters.")]
        public string Citizenship { get; set; }

        [MaxLengthAttribute(20, ErrorMessage = "Occupation cannot be greater than 20 characters.")]
        public string Occupation { get; set; }

        [RequiredAttribute(ErrorMessage = "Please enter current address")]
        [MaxLengthAttribute(500, ErrorMessage = "Address cannot be greater than 500 characters.")]
        public string Address { get; set; }

        [MaxLengthAttribute(20, ErrorMessage = "Pin Cannot be greater than 20 characters")]
        public string Pin { get; set; }

        [RequiredAttribute(ErrorMessage = "Pelase check your status")]
        public bool Status { get; set; }

        [RequiredAttribute(ErrorMessage = "Please enter distributor name.")]
        [MaxLengthAttribute(50, ErrorMessage = "Distributor Name cannot be greater than 50 characters.")]
        public string DistributorName { get; set; }

        [MaxLengthAttribute(500, ErrorMessage = "Distributor Address cannot be greater than 500 characters.")]
        public string DistributorAddress { get; set; }

        [RequiredAttribute(ErrorMessage = "Please enter distributor contact details")]
        [MaxLengthAttribute(20, ErrorMessage = "DistributorContact cannot be greater than 20 characters.")]
        public string DistributorContact { get; set; }

        public string UserID { get; set; }
    }
}