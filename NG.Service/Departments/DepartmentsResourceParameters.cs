using NG.Common.Helpers;

namespace NG.Service.Departments
{
    public class DepartmentsResourceParameters : BaseResourceParameters
    {
        public override string OrderBy { get; set; } = "DepartmentName";
    }
}