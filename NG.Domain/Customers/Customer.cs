using System;
using System.ComponentModel.DataAnnotations;
using NG.Domain.Common;

namespace NG.Domain.Customers
{
    public class Customer : BaseEntity
    {
        [Key]
        public Guid CustomerID { get; set; }

        [RequiredAttribute]
        [MaxLengthAttribute(10)]
        public string NationalID { get; set; }

        [RequiredAttribute]
        [MaxLengthAttribute(10)]
        public string SerialNumber { get; set; }

        [RequiredAttribute]
        [MaxLengthAttribute(50)]
        public string Firstname { get; set; }

        [RequiredAttribute]
        [MaxLengthAttribute(50)]
        public string Surname { get; set; }

        [MaxLengthAttribute(50)]
        public string Othername { get; set; }

        [RequiredAttribute]
        [MaxLengthAttribute(20)]
        public string Mobile { get; set; }


        [MaxLengthAttribute(50)]
        public string Email { get; set; }

        [RequiredAttribute]
        [MaxLengthAttribute(10)]
        public string Gender { get; set; }

        [RequiredAttribute]
        public DateTime DateOfBirth { get; set; }

        [MaxLengthAttribute(20)]
        public string Citizenship { get; set; }

        [MaxLengthAttribute(20)]
        public string Occupation { get; set; }

        [MaxLengthAttribute(20)]
        public string Pin { get; set; }

        [RequiredAttribute]
        [MaxLengthAttribute(500)]
        public string Address { get; set; }

        [RequiredAttribute]
        public bool Status { get; set; }

        [RequiredAttribute]
        [MaxLengthAttribute(50)]
        public string DistributorName { get; set; }
        [MaxLengthAttribute(500)]
        public string DistributorAddress { get; set; }

        [RequiredAttribute]
        [MaxLengthAttribute(20)]
        public string DistributorContact { get; set; }

    }
}