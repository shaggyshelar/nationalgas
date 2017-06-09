using NG.Common.Helpers;

namespace NG.Application.Customers
{
    public class CustomerResourceParameters : BaseResourceParameters
    {
        public override string OrderBy { get; set; } = "DateOfBirth";

    }
}