using NG.Common.Helpers;

namespace NG.Service.Customers
{
    public class CustomerResourceParameters : BaseResourceParameters
    {
        public override string OrderBy { get; set; } = "DateOfBirth";

    }
}