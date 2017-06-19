using NG.Common.Helpers;

namespace NG.Application.Core
{
    public class AppUsersResourceParameters : BaseResourceParameters
    {
        public override string OrderBy { get; set; } = "FirstName";

        public string RoleName { get; set; }

    }
}