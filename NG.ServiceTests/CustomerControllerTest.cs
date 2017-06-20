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

namespace NG.ServiceTests
{
    public class CustomerControllerTest
    {
        private Mock<IUrlHelper> _urlHelper = new Mock<IUrlHelper>();
        private IPropertyMappingService _PropertyMappingService = new PropertyMappingService();
        private ITypeHelperService _TypeHelperService = new TypeHelperService();
        private Mock<IHostingEnvironment> _HostingEnvironment = new Mock<IHostingEnvironment>();
        private GenericRepository<Customer> _GenericRepository;
        private CustomerController _customerController;
        //private Mock<IMapper> _mapper = new Mock<IMapper>();
        private IMapper _mapper;
        private ApplicationContext _dbContextMock;

        public CustomerControllerTest()
        {
            //setup Mapper
            List<Customer> allSampleCustomers = GetAllSampleCustomer();

            //_mapper.Setup(m => m.Map<CustomerDto, Customer>(new CustomerDto())).Returns(new Customer());
            //_mapper.Setup(m => m.Map<Customer, CustomerDto>(new Customer())).Returns(new CustomerDto());

            var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.CreateMap<NG.Domain.Customers.Customer, NG.Service.Customers.CustomerDto>();
                cfg.CreateMap<NG.Service.Customers.CustomerForCreationDto, NG.Domain.Customers.Customer>();
                cfg.CreateMap<NG.Domain.Customers.Customer, NG.Service.Customers.CustomerForCreationDto>();
                cfg.CreateMap<NG.Service.Customers.CustomerForUpdationDto, NG.Domain.Customers.Customer>();
                cfg.CreateMap<NG.Domain.Customers.Customer, NG.Service.Customers.CustomerForUpdationDto>();
            });
            var _mapper = mapperConfig.CreateMapper();

            //Create DBContext Options and mock DBContext
            DbContextOptionsBuilder<ApplicationContext> DBoptions = new DbContextOptionsBuilder<ApplicationContext>();
            DBoptions.UseInMemoryDatabase();
            _dbContextMock = new ApplicationContext(DBoptions.Options);

            _dbContextMock.Customers.AddRange(allSampleCustomers);
            _dbContextMock.SaveChanges();

            //Mock Repository
            _GenericRepository = new GenericRepository<Customer>(_dbContextMock);

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
            IActionResult result = _customerController.GetCustomers(resourseParams, "accept:json");
            OkObjectResult returnedResult = Assert.IsType<OkObjectResult>(result);
            IEnumerable<ExpandoObject> list = returnedResult.Value as IEnumerable<ExpandoObject>;
            var returnedList = list.ToList();
            Assert.Equal(1, returnedList.Count);
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
                new Customer()
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
                    DistributorContact = "9876543210",
                    IsDelete=false
                }
            };
            return allCustomers;
        }

    }
}