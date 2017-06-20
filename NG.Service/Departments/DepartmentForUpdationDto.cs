using NG.Service.Core;

namespace NG.Service.Departments
{
    public class DepartmentForUpdationDto : BaseDto
    {
        public DepartmentForUpdationDto()
        {
        }
        public string DepartmentName { get; set; }
        public string DepartmentDespcription { get; set; }
        public string DepartmentCode { get; set; }
    }
}