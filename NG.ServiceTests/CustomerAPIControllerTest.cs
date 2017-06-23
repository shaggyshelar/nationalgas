using System.Net.Http;
using NG.Service;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NG.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using NG.Common.Services;
using NG.Common;
using Microsoft.AspNetCore.Builder;
using Xunit;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System.Text;
using System.Net;
using NG.Service.Customers;
using System.Net.Http.Headers;
using NG.Service.Core;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NG.Domain.Customers;
using System.Linq;

namespace NG.ServiceTests
{
    public class CustomerAPIControllerTest
    {
        //private readonly HttpClient _client;
        IWebHostBuilder _server;
        List<Customer> customersData = new List<Customer>();
        public CustomerAPIControllerTest()
        {
            _server = new WebHostBuilder()
                        .UseStartup<Startup>();

            customersData = ApplicationContextExtensions.GetAllCustomerData();
        }

        [Fact]
        public async System.Threading.Tasks.Task ValidationTestAsync()
        {
            //Given
            using (var host = new TestServer(_server))
            {
                using (var client = host.CreateClient())
                {

                    CredentialModel creds = new CredentialModel();
                    creds.UserName = "tomcruise";
                    creds.Password = "Espl@123";
                    HttpContent content = new StringContent(JsonConvert.SerializeObject(creds));
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    // client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await client.PostAsync("http://localhost:6060/api/auth/token", content);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    //var response = await client.GetAsync("http://localhost:6060/api/customers");
                }
            }
            //Then
        }

        [Fact]
        public async System.Threading.Tasks.Task GetCustomersTest()
        {
            //Given
            using (var host = new TestServer(_server))
            {
                using (var client = host.CreateClient())
                {
                    CustomerResourceParameters resourseParams = new CustomerResourceParameters();
                    // var requestData = new { name = "Mike" };
                    //var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "applicaiton/json");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "Your Oauth token");

                    var content = new StringContent(JsonConvert.SerializeObject(resourseParams), Encoding.UTF8, "applicaiton/json");
                    var response = await client.GetAsync("http://localhost:6060/api/customers");
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    var result = await response.Content.ReadAsStringAsync();

                    // var serializer = new DataContractJsonSerializer(typeof(IEnumerable<ExpandoObject>));
                    // var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
                    // var data = (IEnumerable<ExpandoObject>)serializer.ReadObject(ms);
                    List<CustomerDto> customerResults = JsonConvert.DeserializeObject<List<CustomerDto>>(result);
                    Console.WriteLine(customerResults);

                    Assert.Equal(customersData.Count, customerResults.Count);
                }
            }
        }

        [Fact]
        public async System.Threading.Tasks.Task UpdateTestAsync()
        {
            Customer selectedCustomer = customersData.First();
            //Given
            using (var host = new TestServer(_server))
            {
                using (var client = host.CreateClient())
                {
                    CustomerForUpdationDto updatedCustomer = new CustomerForUpdationDto();
                    updatedCustomer.NationalID = selectedCustomer.NationalID;
                    updatedCustomer.SerialNumber = selectedCustomer.SerialNumber;
                    updatedCustomer.Firstname = selectedCustomer.Firstname;
                    updatedCustomer.SurName = selectedCustomer.Surname;
                    updatedCustomer.Othername = selectedCustomer.Othername;
                    updatedCustomer.Mobile = selectedCustomer.Mobile;
                    updatedCustomer.Email = selectedCustomer.Email;
                    updatedCustomer.Gender = selectedCustomer.Gender;
                    updatedCustomer.DateOfBirth = selectedCustomer.DateOfBirth;
                    updatedCustomer.Citizenship = selectedCustomer.Citizenship;
                    updatedCustomer.Occupation = selectedCustomer.Occupation;
                    updatedCustomer.Address = selectedCustomer.Address;
                    updatedCustomer.Pin = selectedCustomer.Pin;
                    updatedCustomer.Status = selectedCustomer.Status;
                    updatedCustomer.DistributorName = selectedCustomer.DistributorName + " Updated";
                    updatedCustomer.DistributorAddress = selectedCustomer.DistributorAddress + " Updated";
                    updatedCustomer.DistributorContact = selectedCustomer.DistributorContact + " Updated";
                    updatedCustomer.UserID = selectedCustomer.UserID;

                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "Your Oauth token");
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    //client.DefaultRequestHeaders.Add("content-type","application/json");
                    //var content = new StringContent(JsonConvert.SerializeObject(updatedCustomer), Encoding.UTF8, "applicaiton/json");
                    //var content = new StringContent(JsonConvert.SerializeObject(updatedCustomer), Encoding.Unicode, "applicaiton/json");

                    HttpContent content = new StringContent(JsonConvert.SerializeObject(updatedCustomer));
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    var response = await client.PutAsync("http://localhost:6060/api/customers/" + Convert.ToString(selectedCustomer.CustomerID), content);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    var result = await response.Content.ReadAsStringAsync();

                    List<CustomerDto> customerResults = JsonConvert.DeserializeObject<List<CustomerDto>>(result);
                    Console.WriteLine(customerResults);

                    //Assert.Equal(4, customerResults.Count);

                }
            }
        }
    }
}