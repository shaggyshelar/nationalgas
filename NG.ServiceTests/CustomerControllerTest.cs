using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NG.Common.Services;
using NG.Domain.Customers;
using NG.Persistence;
using NG.Service;
using NG.Service.Controllers;
using NG.Service.Customers;
using Xunit;
using NG.ServiceTests.Constant;
using NG.Common.Helpers;

namespace NG.ServiceTests
{
    public class CustomerControllerTest
    {
        private CustomerController _customerController;
        ApplicationContext _dbContextMock;
        private List<Customer> allSampleCustomers { get { return GetAllSampleCustomer(); } }

        public CustomerControllerTest()
        {
            Mock<IUrlHelper> _urlHelper = new Mock<IUrlHelper>();
            IPropertyMappingService _PropertyMappingService = new PropertyMappingService();
            ITypeHelperService _TypeHelperService = new TypeHelperService();
            Mock<IHostingEnvironment> _HostingEnvironment = new Mock<IHostingEnvironment>();

            //setup Mapper
            var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.CreateMap<NG.Domain.Customers.Customer, NG.Service.Customers.CustomerDto>();
                cfg.CreateMap<NG.Service.Customers.CustomerForCreationDto, NG.Domain.Customers.Customer>();
                cfg.CreateMap<NG.Domain.Customers.Customer, NG.Service.Customers.CustomerForCreationDto>();
                cfg.CreateMap<NG.Service.Customers.CustomerForUpdationDto, NG.Domain.Customers.Customer>();
                cfg.CreateMap<NG.Domain.Customers.Customer, NG.Service.Customers.CustomerForUpdationDto>();
            });
            IMapper _mapper = mapperConfig.CreateMapper();

            //Create DBContext Options and mock DBContext
            DbContextOptionsBuilder<ApplicationContext> DBoptions = new DbContextOptionsBuilder<ApplicationContext>();
            //DBoptions.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());
            DBoptions.UseInMemoryDatabase();
            _dbContextMock = new ApplicationContext(DBoptions.Options);
            _dbContextMock.Database.EnsureDeleted();

            _dbContextMock.Customers.AddRange(allSampleCustomers);
            _dbContextMock.SaveChanges();

            //Mock Repository
            GenericRepository<Customer> _GenericRepository = new GenericRepository<Customer>(_dbContextMock);

            _customerController = new CustomerController(
                _urlHelper.Object,
                _PropertyMappingService,
                _TypeHelperService,
                _HostingEnvironment.Object,
                _GenericRepository,
                _mapper);
        }

        [Fact]
        public void GetAllCustomersTest()
        {
            //Given
            CustomerResourceParameters resourseParams = new CustomerResourceParameters();
            //When
            SetContext();
            //Then
            IActionResult result = _customerController.GetCustomers(resourseParams, Constants.AcceptJSON);
            OkObjectResult returnedResult = Assert.IsType<OkObjectResult>(result);
            IEnumerable<ExpandoObject> list = returnedResult.Value as IEnumerable<ExpandoObject>;
            var returnedList = list.ToList();
            Assert.Equal(allSampleCustomers.Count, returnedList.Count);
        }

        [Fact]
        public void GetAllCustomersPaginationTest()
        {
            //Given
            CustomerResourceParameters resourseParams = new CustomerResourceParameters();
            resourseParams.PageSize = 3;
            resourseParams.PageNumber = 1;
            //When
            SetContext();
            //Then
            IActionResult result = _customerController.GetCustomers(resourseParams, Constants.AcceptJSON);
            OkObjectResult returnedResult = Assert.IsType<OkObjectResult>(result);
            IEnumerable<ExpandoObject> list = returnedResult.Value as IEnumerable<ExpandoObject>;
            var returnedList = list.ToList();
            Assert.Equal(resourseParams.PageSize, returnedList.Count);
        }

        [Fact]
        public void GetAllCustomersPaginationSecondPageTest()
        {
            //Given
            CustomerResourceParameters resourseParams = new CustomerResourceParameters();
            resourseParams.PageSize = 3;
            resourseParams.PageNumber = 1;
            //When
            SetContext();
            //second page
            resourseParams.PageNumber = 2;
            int expectedItems = (allSampleCustomers.Count() - resourseParams.PageSize) > resourseParams.PageSize ? resourseParams.PageSize : (allSampleCustomers.Count() - resourseParams.PageSize);

            IActionResult secondresult = _customerController.GetCustomers(resourseParams, Constants.AcceptJSON);
            OkObjectResult returnedSecondResult = Assert.IsType<OkObjectResult>(secondresult);
            IEnumerable<ExpandoObject> secondList = returnedSecondResult.Value as IEnumerable<ExpandoObject>;
            var returnedList = secondList.ToList();
            Assert.Equal(expectedItems, returnedList.Count);
        }

        [Fact]
        public void GetAllCustomersSearchTest()
        {
            //Given
            CustomerResourceParameters resourseParams = new CustomerResourceParameters();
            resourseParams.SearchQuery = "joh";
            //When
            SetContext();
            //Then
            IActionResult result = _customerController.GetCustomers(resourseParams, Constants.AcceptJSON);
            OkObjectResult returnedResult = Assert.IsType<OkObjectResult>(result);
            IEnumerable<ExpandoObject> list = returnedResult.Value as IEnumerable<ExpandoObject>;
            var returnedList = list.ToList();
            Assert.Equal(2, returnedList.Count);
        }

        [Fact]
        public void GetAllCustomersSearchNotMatchTest()
        {
            //Given
            CustomerResourceParameters resourseParams = new CustomerResourceParameters();
            resourseParams.SearchQuery = "xyz";
            //When
            SetContext();
            //Then
            IActionResult result = _customerController.GetCustomers(resourseParams, Constants.AcceptJSON);
            OkObjectResult returnedResult = Assert.IsType<OkObjectResult>(result);
            IEnumerable<ExpandoObject> list = returnedResult.Value as IEnumerable<ExpandoObject>;
            var returnedList = list.ToList();
            Assert.Equal(0, returnedList.Count);
        }


        [Fact]
        public void GetSingleCustomerTest()
        {
            //Given
            CustomerResourceParameters resourseParams = new CustomerResourceParameters();
            //When
            SetContext();
            //Then
            Customer sampleCustomer = allSampleCustomers.FirstOrDefault();
            IActionResult result = _customerController.GetCustomer(sampleCustomer.CustomerID, null);
            OkObjectResult returnedResult = Assert.IsType<OkObjectResult>(result);
            ExpandoObject list = returnedResult.Value as ExpandoObject;
            dynamic customerObj = list;
            Assert.Equal(sampleCustomer.CustomerID, customerObj.CustomerID);
            Assert.Equal(sampleCustomer.NationalID, customerObj.NationalID);
            Assert.Equal(sampleCustomer.SerialNumber, customerObj.SerialNumber);
            Assert.Equal(sampleCustomer.Firstname, customerObj.Firstname);
            Assert.Equal(sampleCustomer.Surname, customerObj.Surname);
            Assert.Equal(sampleCustomer.Othername, customerObj.Othername);
            Assert.Equal(sampleCustomer.Mobile, customerObj.Mobile);
            Assert.Equal(sampleCustomer.Email, customerObj.Email);
            Assert.Equal(sampleCustomer.Gender, customerObj.Gender);
            Assert.Equal(sampleCustomer.DateOfBirth, customerObj.DateOfBirth);
            Assert.Equal(sampleCustomer.Pin, customerObj.Pin);
            Assert.Equal(sampleCustomer.Address, customerObj.Address);
            Assert.Equal(sampleCustomer.DistributorName, customerObj.DistributorName);
            Assert.Equal(sampleCustomer.DistributorAddress, customerObj.DistributorAddress);
            Assert.Equal(sampleCustomer.DistributorContact, customerObj.DistributorContact);
            Assert.Equal(sampleCustomer.UserID, customerObj.UserID);
        }

        [Fact]
        public void GetAllCustomersLookupTest()
        {
            //Given
            CustomerResourceParameters resourseParams = new CustomerResourceParameters();
            //When
            SetContext();
            //Then
            IActionResult result = _customerController.GetCustomerAsLookUp(Constants.AcceptJSON);
            OkObjectResult returnedResult = Assert.IsType<OkObjectResult>(result);
            IEnumerable<LookUpItem> list = returnedResult.Value as IEnumerable<LookUpItem>;
            var returnedList = list.ToList();
            Assert.Equal(allSampleCustomers.Count, returnedList.Count);
        }

        [Fact]
        public void CreateCustomerTest()
        {
            //Given
            CustomerForCreationDto creationDto = new CustomerForCreationDto();
            creationDto.NationalID = "6789012345";
            creationDto.SerialNumber = "6789012345";
            creationDto.Firstname = "Steve";
            creationDto.SurName = "Rogers";
            creationDto.Othername = "Steve";
            creationDto.Mobile = "9873216540";
            creationDto.Email = "steve.rogers@nock.com";
            creationDto.Gender = "M";
            creationDto.DateOfBirth = Convert.ToDateTime("10/15/1983 12:00:00 AM");
            creationDto.Address = "test address for national id 6789012345";
            creationDto.Pin = "12345";
            creationDto.Status = true;
            creationDto.DistributorName = "Test Distributor 2";
            creationDto.DistributorAddress = "Test Distributor 2 address";
            creationDto.DistributorContact = "9876543210";
            creationDto.UserID = "56c385ae-ce46-41d4-b7fe-08df9aef7402";
            //When
            IActionResult result = _customerController.CreateCustomer(creationDto);
            CreatedAtRouteResult returnedResult = Assert.IsType<CreatedAtRouteResult>(result);
            dynamic newCustomer = returnedResult.Value;
            //Then
            Assert.NotNull(newCustomer.CustomerID);
            Assert.Equal(creationDto.NationalID, newCustomer.NationalID);
            Assert.Equal(creationDto.SerialNumber, newCustomer.SerialNumber);
            Assert.Equal(creationDto.Firstname, newCustomer.Firstname);
            Assert.Equal(creationDto.SurName, newCustomer.Surname);
            Assert.Equal(creationDto.Othername, newCustomer.Othername);
            Assert.Equal(creationDto.Mobile, newCustomer.Mobile);
            Assert.Equal(creationDto.Email, newCustomer.Email);
            Assert.Equal(creationDto.Gender, newCustomer.Gender);
            Assert.Equal(creationDto.DateOfBirth, newCustomer.DateOfBirth);
            Assert.Equal(creationDto.Address, newCustomer.Address);
            Assert.Equal(creationDto.Pin, newCustomer.Pin);
            Assert.Equal(creationDto.Status, newCustomer.Status);
            Assert.Equal(creationDto.DistributorName, newCustomer.DistributorName);
            Assert.Equal(creationDto.DistributorAddress, newCustomer.DistributorAddress);
            Assert.Equal(creationDto.DistributorContact, newCustomer.DistributorContact);
            Assert.Equal(creationDto.UserID, newCustomer.UserID);
        }

        [Fact]
        public void UpdateCustomerTest()
        {
            Customer selectedCustomer = allSampleCustomers.LastOrDefault();

            //Given
            CustomerForUpdationDto updationDto = new CustomerForUpdationDto();
            updationDto.NationalID = selectedCustomer.NationalID;
            updationDto.SerialNumber = selectedCustomer.SerialNumber;
            updationDto.Firstname = selectedCustomer.Firstname;
            updationDto.SurName = selectedCustomer.Surname;
            updationDto.Othername = selectedCustomer.Othername;
            updationDto.Mobile = "1234567890";
            updationDto.Email = "updated.email.com";
            updationDto.Gender = selectedCustomer.Gender;
            updationDto.DateOfBirth = selectedCustomer.DateOfBirth;
            updationDto.Address = selectedCustomer.Address;
            updationDto.Pin = selectedCustomer.Pin;
            updationDto.DistributorName = "Test Distributor 2";
            updationDto.DistributorAddress = "Test Distributor 2 address";
            updationDto.DistributorContact = "9876543210";
            updationDto.UserID = selectedCustomer.UserID;
            //When
            IActionResult result = _customerController.UpdateCustomer(selectedCustomer.CustomerID, updationDto);
            Customer returnedResult = Assert.IsType<Customer>(result);
            //dynamic newCustomer = returnedResult.Value;
            //Then
        }

        [Fact]
        public void DeleteCustomerTest()
        {
            //Given
            Customer selectedCustomer = allSampleCustomers.First();
            //When
            IActionResult result = _customerController.DeleteCustomer(selectedCustomer.CustomerID);
            Customer returnedResult = Assert.IsType<Customer>(result);
            //dynamic newCustomer = returnedResult.Value;
            //Then
        }

        private void SetContext()
        {
            var headerDictionary = new HeaderDictionary();
            var response = new Mock<HttpResponse>();
            response.SetupGet(r => r.Headers).Returns(headerDictionary);
            var httpContext = new Mock<HttpContext>();
            httpContext.SetupGet(a => a.Response).Returns(response.Object);

            _customerController.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext.Object
            };
        }

        private List<Customer> GetAllSampleCustomer()
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
                    UserID="56c385ae-ce46-41d4-b7fe-08df9aef7401",
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
                    UserID = "56c385ae-ce46-41d4-b7fe-08df9aef7303",
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
                    UserID = "56c385ae-ce46-41d4-b7fe-08df9aef7304",
                },
                new Customer() {
                    CustomerID = new Guid("b1da1d8e-1111-4634-b538-a01709471004"),
                    NationalID = "4567890123",
                    SerialNumber = "4567890123",
                    Firstname = "Jack",
                    Surname = "Sparrow",
                    Othername="Jacky",
                    Mobile = "7894561230",
                    Email = "jack.sparrow@nock.com",
                    Gender = "M",
                    DateOfBirth = Convert.ToDateTime("10/15/1983 12:00:00 AM"),
                    Pin = "12345",
                    Address = "test address for national id 4567890123",
                    DistributorName = "Test Distributor 3",
                    DistributorAddress = "Test Distributor 3 address",
                    DistributorContact = "9876543210",
                    UserID = "56c385ae-ce46-41d4-b7fe-08df9aef7305",
                }
            };
            return allCustomers;
        }
    }
}