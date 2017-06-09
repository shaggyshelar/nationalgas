using NG.Common.Helpers;

namespace NG.Application.Departments
{
    public class DepartmentsResourceParameters : BaseResourceParameters
    {
        public override string OrderBy { get; set; } = "DepartmentName";
    }
}