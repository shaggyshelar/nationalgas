using System;

namespace NG.Service.Customers
{
    public class NationalIDResponse
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorOcurred { get; set; }
        public string Citizenship { get; set; }
        public string Clan { get; set; }
        public DateTime? Date_of_Birth { get; set; }
        public DateTime? Date_of_Death { get; set; }
        public string Ethnic_Group { get; set; }
        public string Family { get; set; }
        public string Fingerprint { get; set; }
        public string First_Name { get; set; }
        public string Gender { get; set; }
        public string ID_Number { get; set; }
        public string Occupation { get; set; }
        public string Other_Name { get; set; }
        public string Photo { get; set; }
        public string Pin { get; set; }
        public string Place_of_Birth { get; set; }
        public string Place_of_Death { get; set; }
        public string Place_of_Live { get; set; }
        public string Signature { get; set; }
        public string Surname { get; set; }
        public DateTime? Date_of_Issue { get; set; }
        public string RegOffice { get; set; }
        public string Serial_Number { get; set; }

    }
}