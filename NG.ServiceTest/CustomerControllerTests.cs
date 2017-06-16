using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NG.Common;
using NG.Domain.Customers;
using NG.Persistence;
using NG.Service.Controllers.Customers;
using Xunit;
using Moq;
using NG.Common.Services;
using Microsoft.AspNetCore.Hosting;
using NG.Application.Customers;
using AutoMapper;
using NG.Service;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace NG.ServiceTest
{
    public class CustomerControllerTests
    {
        private Mock<IUrlHelper> _urlHelper = new Mock<IUrlHelper>();
        private IPropertyMappingService _PropertyMappingService = new PropertyMappingService();
        private ITypeHelperService _TypeHelperService = new TypeHelperService();
        private Mock<IHostingEnvironment> _HostingEnvironment = new Mock<IHostingEnvironment>();
        //private Mock<IGenericRepository<Customer>> _GenericRepository = new Mock<IGenericRepository<Customer>>();
        private IGenericRepository<Customer> _GenericRepository;
        private CustomerController _customerController;
        private Mock<ApplicationContext> _dbContextMock = new Mock<ApplicationContext>();
        private Mock<IMapper> _mapper = new Mock<IMapper>();

        public CustomerControllerTests()
        {

            _mapper.Setup(m => m.Map<CustomerDto, Customer>(It.IsAny<CustomerDto>())).Returns(new Customer());

            Mock<DbSet<Customer>> customerMock = DbSetMock.Create(GetCustomer());
            _dbContextMock.Setup(c => c.Customers).Returns(customerMock.Object);
            _GenericRepository = new GenericRepository<Customer>(_dbContextMock.Object);

            _customerController = new CustomerController(
                _urlHelper.Object,
                _PropertyMappingService,
                _TypeHelperService,
                _HostingEnvironment.Object,
                _GenericRepository,
                _mapper.Object);
        }

        [Fact]
        public void GetAllCustomers()
        {
            //Given
            //Mock<DbSet<ApplicationContext>> customerMock = DbSetMock.Create(fakeUser);




            CustomerResourceParameters resourseParams = new CustomerResourceParameters();
            resourseParams.SearchQuery = "nick";

            SetContext();

            IActionResult result = _customerController.GetCustomers(resourseParams, "accept:json");
            Assert.IsType<OkResult>(result);
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

        private Customer GetCustomer()
        {
            var customer = new Customer()
            {
                CustomerID = new Guid("b1da1d8e-1111-4634-b538-a01709471000"),
                NationalID = "1234567890",
                SerialNumber = "1234567890",
                Firstname = "Nick",
                Surname = "Jones",
                Othername = "Nicky",
                Mobile = "9873216540",
                Email = "nick.jones@nock.com",
                Gender = "M",
                DateOfBirth = Convert.ToDateTime("10/7/1981 12:00:00 AM"),
                Pin = "12345",
                Address = "test address for national id 1234567890",
                DistributorName = "Test Distributor 1",
                DistributorAddress = "Test Distributor 1 address",
                DistributorContact = "9876543210"
            };

            return customer;
        }

        private ApplicationContext CreateContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
            optionsBuilder.UseInMemoryDatabase();

            var context = new ApplicationContext(optionsBuilder.Options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            context.Customers.AddRange(new List<Customer>());
            context.SaveChanges();
            return context;
        }
    }
}