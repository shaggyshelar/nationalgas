namespace NG.Service.Customers
{
    public class CustomerRegistrationReportDto
    {
        public int CustomerCount { get; set; }
        public int MaleCount { get; set; }
        public int FemaleCount { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}