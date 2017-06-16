using System;
using System.Collections.Generic;
using NG.Domain.Customers;
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
            UpdateCustomers(context);
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

        public static void UpdateCustomers(this ApplicationContext context)
        {
            List<Customer> allCustomers = new List<Customer>(){
                new Customer(){
                    CustomerID = new Guid("b1da1d8e-1111-4634-b538-a01709471000"),
                    NationalID = "1234567890",
                    SerialNumber = "1234567890",
                    Firstname = "Nick",
                    Surname="Jones",
                    Othername="Nicky",
                    Mobile="9873216540",
                    Email="nick.jones@nock.com",
                    Gender="M",
                    DateOfBirth=Convert.ToDateTime("10/7/1981 12:00:00 AM"),
                    Pin="12345",
                    Address="test address for national id 1234567890",
                    DistributorName="Test Distributor 1",
                    DistributorAddress="Test Distributor 1 address",
                    DistributorContact="9876543210",
                },
                new Customer() {
                    CustomerID = new Guid("b1da1d8e-1111-4634-b538-a01709471002"),
                    NationalID = "3456789012",
                    SerialNumber = "3456789012",
                    Firstname = "John",
                    Surname = "Doe",
                    Othername = "Johnny",
                    Mobile = "6549871234",
                    Email = "john.doe@nock.com",
                    Gender = "M",
                    DateOfBirth = Convert.ToDateTime("5/24/1980 12:00:00 AM"),
                    Pin = "12345",
                    Address = "test address for national id 3456789012",
                    DistributorName = "Test Distributor 2",
                    DistributorAddress = "Test Distributor 2 address",
                    DistributorContact = "9876543210",
                },
                new Customer() {
                    CustomerID = new Guid("b1da1d8e-1111-4634-b538-a01709471003"),
                    NationalID = "8901234567",
                    SerialNumber = "8901234567",
                    Firstname = "Johnny",
                    Surname = "Depp",
                    Mobile = "3216549870",
                    Email = "johny.depp@nock.com",
                    Gender = "M",
                    DateOfBirth = Convert.ToDateTime("11/18/1983 12:00:00 AM"),
                    Pin = "12345",
                    Address = "test address for national id 8901234567",
                    DistributorName = "Test Distributor 3",
                    DistributorAddress = "Test Distributor 3 address",
                    DistributorContact = "9876543210",
                },
                new Customer() {
                    CustomerID = new Guid("b1da1d8e-1111-4634-b538-a01709471004"),
                    NationalID = "4567890123",
                    SerialNumber = "4567890123",
                    Firstname = "Jack",
                    Surname = "Sparrow",
                    Othername="Jacky",
                    Mobile = "7894561230",
                    Email = "johny.depp@nock.com",
                    Gender = "M",
                    DateOfBirth = Convert.ToDateTime("10/15/1983 12:00:00 AM"),
                    Pin = "12345",
                    Address = "test address for national id 4567890123",
                    DistributorName = "Test Distributor 3",
                    DistributorAddress = "Test Distributor 3 address",
                    DistributorContact = "9876543210",
                }
            };
            context.Customers.AddRange(allCustomers);
            context.SaveChanges();
        }
    }
}