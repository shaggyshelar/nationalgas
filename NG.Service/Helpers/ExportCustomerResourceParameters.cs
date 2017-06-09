using NG.Common.Helpers;

namespace NG.Service.Helpers
{
    public class ExportCustomerResourceParameters : ExportResourceParameters
    {
        public override string OrderBy { get; set; } = "DateOfBirth";
    }
}