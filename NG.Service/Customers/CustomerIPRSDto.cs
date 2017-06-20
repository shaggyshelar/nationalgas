using System;

namespace NG.Service.Customers
{
    public class CustomerIPRSDto
    {
        public string NationalID { get; set; }
        public string SerialNumber { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorOcurred { get; set; }
        public string Citizenship { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Firstname { get; set; }
        public string Surname { get; set; }
        public string Othername { get; set; }
        public string Gender { get; set; }
        public string Occupation { get; set; }
        public string Pin { get; set; }
        public string Address { get; set; }
    }
}