
using System;
using NG.Domain.Common;

namespace NG.Domain.Departments
{
    public class Department : BaseEntity
    {
        public Guid DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentDespcription { get; set; }
    }
}