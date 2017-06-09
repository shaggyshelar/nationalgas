using System;
using System.Collections.Generic;
using NG.Domain.Departments;

namespace NG.Persistence
{
    public static class ApplicationContextExtensions
    {
        public static void EnsureSeedDataForContext(this ApplicationContext context)
        {
            context.Departments.RemoveRange(context.Departments);
            context.SaveChanges();

            UpdateDepartments(context);
        }

        public static void UpdateDepartments(this ApplicationContext context)
        {
            var departments = new List<Department>() {
                new Department() {
                    DepartmentID = new Guid("a1da1d8e-1111-4634-b538-a01709471111"),
                    DepartmentName = "General Service Unit (GSU)",
                    DepartmentCode="GSU",
                    DepartmentDespcription = "A paramilitary wing to deal with situations affecting internal security and to be a reserve force to deal with special operations and civil disorders.",
                    CreatedBy=new Guid("56c385ae-ce46-41d4-b7fe-08df9aef9999"),
                    CreatedOn=DateTime.Now.AddHours(-6)
                },
                new Department() {
                    DepartmentID = new Guid("a1da1d8e-1111-4634-b538-a01709472222"),
                    DepartmentName = "Anti Stock Theft Unit",
                    DepartmentCode="AST",
                    DepartmentDespcription = "Anti Stock Theft Unit for stock ",
                    CreatedBy=new Guid("56c385ae-ce46-41d4-b7fe-08df9aef9999"),
                    CreatedOn=DateTime.Now.AddHours(-6)
                },
                new Department() {
                    DepartmentID = new Guid("a1da1d8e-1111-4634-b538-a01709473333"),
                    DepartmentName = "Criminal Investigation Department",
                    DepartmentCode="CID",
                    DepartmentDespcription = "Responsible for investigating complex cases.",
                    CreatedBy=new Guid("56c385ae-ce46-41d4-b7fe-08df9aef9999"),
                    CreatedOn=DateTime.Now.AddHours(-6)
                },
                new Department() {
                    DepartmentID = new Guid("a1da1d8e-1111-4634-b538-a01709474444"),
                    DepartmentName = "Traffic Police Department",
                    DepartmentCode="TPD",
                    DepartmentDespcription = "Force to enforce traffic laws in the republic.",
                    CreatedBy=new Guid("56c385ae-ce46-41d4-b7fe-08df9aef9999"),
                    CreatedOn=DateTime.Now.AddHours(-6)
                },
                new Department() {
                    DepartmentID = new Guid("a1da1d8e-1111-4634-b538-a01709475555"),
                    DepartmentName = "Kenya Police College",
                    DepartmentCode="KPC",
                    DepartmentDespcription = "Training College for Police cadets.",
                    CreatedBy=new Guid("56c385ae-ce46-41d4-b7fe-08df9aef9999"),
                    CreatedOn=DateTime.Now.AddHours(-6)
                },
                new Department() {
                    DepartmentID = new Guid("a1da1d8e-1111-4634-b538-a01709476666"),
                    DepartmentName = "Kenya Police Air Wing",
                    DepartmentCode="KPAW",
                    DepartmentDespcription = "Provides air support and surveillance to troops on ground.",
                    CreatedBy=new Guid("56c385ae-ce46-41d4-b7fe-08df9aef9999"),
                    CreatedOn=DateTime.Now.AddHours(-6)
                },
                new Department() {
                    DepartmentID = new Guid("a1da1d8e-1111-4634-b538-a01709477777"),
                    DepartmentName = "Kenya Railways Police",
                    DepartmentCode="KRP",
                    DepartmentDespcription = "Maintaining law and order in trains and on train stations.",
                    CreatedBy=new Guid("56c385ae-ce46-41d4-b7fe-08df9aef9999"),
                    CreatedOn=DateTime.Now.AddHours(-6)
                },
                new Department() {
                    DepartmentID = new Guid("a1da1d8e-1111-4634-b538-a01709478888"),
                    DepartmentName = "Kenya Police Dog Unit",
                    DepartmentCode="KPDU",
                    DepartmentDespcription = "Sniffer dogs to detect explosives and drugs.",
                    CreatedBy=new Guid("56c385ae-ce46-41d4-b7fe-08df9aef9999"),
                    CreatedOn=DateTime.Now.AddHours(-6)
                },

                new Department() {
                    DepartmentID = new Guid("a1da1d8e-1111-4634-b538-a01709479999"),
                    DepartmentName = "Tourism Police Unit",
                    DepartmentCode="TPU",
                    DepartmentDespcription = "A department to tackle crimes related to tourists.",
                    CreatedBy=new Guid("56c385ae-ce46-41d4-b7fe-08df9aef9999"),
                    CreatedOn=DateTime.Now.AddHours(-6)
                },
                new Department() {
                    DepartmentID = new Guid("a1da1d8e-1111-4634-b538-a01709471010"),
                    DepartmentName = "Kenya Airports Police Unit",
                    DepartmentCode="KAPU",
                    DepartmentDespcription = "A department tasked with protecting airports in the republic.",
                    CreatedBy=new Guid("56c385ae-ce46-41d4-b7fe-08df9aef9999"),
                    CreatedOn=DateTime.Now.AddHours(-6)
                },
                new Department() {
                    DepartmentID = new Guid("a1da1d8e-1111-4634-b538-a01709471011"),
                    DepartmentName = "Maritime Police Unit",
                    DepartmentCode="MPU",
                    DepartmentDespcription = "A marine police unit to secure the coastline and internal rivers.",
                    CreatedBy=new Guid("56c385ae-ce46-41d4-b7fe-08df9aef9999"),
                    CreatedOn=DateTime.Now.AddHours(-6)
                },
                new Department() {
                    DepartmentID = new Guid("a1da1d8e-1111-4634-b538-a01709471012"),
                    DepartmentName = "Diplomatic Police Unit",
                    DepartmentCode="DPU",
                    DepartmentDespcription = "A department tasked with protecting the diplomats in the republic.",
                    CreatedBy=new Guid("56c385ae-ce46-41d4-b7fe-08df9aef9999"),
                    CreatedOn=DateTime.Now.AddHours(-6)
                },
            };

            context.Departments.AddRange(departments);
            context.SaveChanges();
        }
    }
}