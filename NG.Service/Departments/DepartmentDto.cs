using System;

namespace NG.Service.Departments
{
    public class DepartmentDto
    {
        public Guid DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentDespcription { get; set; }
    }
}