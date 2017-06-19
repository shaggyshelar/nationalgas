using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using System.IO;
using OfficeOpenXml;
using Microsoft.AspNetCore.Hosting;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.NodeServices;
using OfficeOpenXml.Style;
using NG.Common.Services;
using NG.Application.Customers;
using NG.Domain.Customers;
using NG.Common.Extensions;
using NG.Common.Enums;
using NG.Service.Helpers;
using NG.Common.Helpers;
using NG.Common.DTO;
using NG.Common;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using NG.Application;

namespace NG.Service.Controllers.Customers
{
    [Route("api/customers")]
    public class CustomerController : Controller
    {
        private IUrlHelper _urlHelper;
        private IPropertyMappingService _propertyMappingService;
        private ITypeHelperService _typeHelperService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private IGenericRepository<Customer> _repo;

        public CustomerController(IUrlHelper urlHelper,
           IPropertyMappingService propertyMappingService,
           ITypeHelperService typeHelperService,
           IHostingEnvironment hostingEnvironment,
           IGenericRepository<Customer> repo)
        {
            _urlHelper = urlHelper;
            _propertyMappingService = propertyMappingService;
            _typeHelperService = typeHelperService;
            _hostingEnvironment = hostingEnvironment;
            _repo = repo;
        }

        [HttpGet(Name = "GetCustomers")]
        [HttpHead]
        [Authorize(Policy = Permissions.CustomerRead)]
        public IActionResult GetCustomers(CustomerResourceParameters customerResourceParameters,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<CustomerDto, Customer>
               (customerResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_typeHelperService.TypeHasProperties<CustomerDto>
                (customerResourceParameters.Fields))
            {
                return BadRequest();
            }

            var customerFromRepo = this.GetCustomers(customerResourceParameters);
            var customer = Mapper.Map<IEnumerable<CustomerDto>>(customerFromRepo);

            var collectionBeforePaging =
                _repo.Query().Where(a => a.IsDelete == false).ApplySort(customerResourceParameters.OrderBy,
                _propertyMappingService.GetPropertyMapping<CustomerDto, Customer>());

            if (!string.IsNullOrEmpty(customerResourceParameters.SearchQuery))
            {
                var searchQueryForWhereClause = customerResourceParameters.SearchQuery
                    .Trim().ToLowerInvariant();

                collectionBeforePaging = collectionBeforePaging
                    .Where(a => a.Firstname.ToLowerInvariant().Contains(searchQueryForWhereClause)
                    || (a.Email != null
                        && a.Email.ToLowerInvariant().Contains(searchQueryForWhereClause)));
            }

            var departmentsFromRepo = PagedList<Customer>.Create(collectionBeforePaging,
                customerResourceParameters.PageNumber,
                customerResourceParameters.PageSize);

            var departments = Mapper.Map<IEnumerable<CustomerDto>>(departmentsFromRepo);
            var shapedDepartments = departments.ShapeData(customerResourceParameters.Fields);

            if (mediaType == "application/vnd.marvin.hateoas+json")
            {
                var paginationMetadata = departmentsFromRepo.GetHateosMetadata();
                Response.Headers.Add("X-Pagination",
                    Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));
                Response.Headers.Add("Access-Control-Expose-Headers", "ETag, X-Pagination");

                var links = Utilities.CreateLinks(customerResourceParameters,
                    departmentsFromRepo.HasNext, departmentsFromRepo.HasPrevious, _urlHelper, "Department");
                var linkedCollectionResource = new
                {
                    value = shapedDepartments.Select(department =>
                        {
                            var departmentAsDictionary = department as IDictionary<string, object>;
                            var departmentLinks = Utilities.CreateLinks(
                                (Guid)departmentAsDictionary["Id"], customerResourceParameters.Fields,
                                _urlHelper, "Department");
                            departmentAsDictionary.Add("links", departmentLinks);
                            return departmentAsDictionary;
                        }),
                    links = links
                };

                return Ok(linkedCollectionResource);
            }
            else
            {
                var paginationMetadata = departmentsFromRepo.GetMetadata(customerResourceParameters, _urlHelper);
                Response.Headers.Add("X-Pagination",
                    Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));
                Response.Headers.Add("Access-Control-Expose-Headers", "ETag, X-Pagination");

                return Ok(customer.ShapeData(customerResourceParameters.Fields));
            }
        }

        [HttpGet("ExportToExcel", Name = "ExportToExcel")]
        [Authorize(Policy = Permissions.CustomerRead)]
        public IActionResult ExportToExcel(ExportCustomerResourceParameters customerResourceParameters)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<CustomerDto, Customer>
               (customerResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_typeHelperService.TypeHasProperties<CustomerDto>
                (customerResourceParameters.Fields))
            {
                return BadRequest();
            }

            var customers = this.GetCustomers(customerResourceParameters);

            string sWebRootFolder = _hostingEnvironment.WebRootPath;
            string sFileName = @"ExportedDocuments/CustomerReport.xlsx";
            string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, sFileName);
            string localFilePath = Path.Combine(sWebRootFolder, sFileName);

            FileInfo file = new FileInfo(localFilePath);
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            }
            using (ExcelPackage package = new ExcelPackage(file))
            {
                // add a new worksheet to the empty workbook
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Customer");
                //First add the headers
                worksheet.Cells[1, 1].Value = "Name";
                worksheet.Cells[1, 2].Value = "Mobile";
                worksheet.Cells[1, 3].Value = "Landline";
                worksheet.Cells[1, 4].Value = "Customer Email";
                worksheet.Cells[1, 5].Value = "Date Of Birth";
                worksheet.Cells[1, 6].Value = "Customer Address";
                worksheet.Cells[1, 7].Value = "Status";
                worksheet.Cells[1, 8].Value = "Distributor Name";
                worksheet.Cells[1, 9].Value = "Distributor Address";
                worksheet.Cells[1, 10].Value = "Distributor Contact";
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 2].Style.Font.Bold = true;
                worksheet.Cells[1, 3].Style.Font.Bold = true;
                worksheet.Cells[1, 4].Style.Font.Bold = true;
                worksheet.Cells[1, 5].Style.Font.Bold = true;
                worksheet.Cells[1, 6].Style.Font.Bold = true;
                worksheet.Cells[1, 7].Style.Font.Bold = true;
                worksheet.Cells[1, 8].Style.Font.Bold = true;
                worksheet.Cells[1, 9].Style.Font.Bold = true;
                worksheet.Cells[1, 10].Style.Font.Bold = true;

                var index = 2;
                customers.ForEach(customer =>
                {
                    worksheet.Cells[string.Format("A{0}", index)].Value = customer.Firstname;
                    worksheet.Cells[string.Format("B{0}", index)].Value = customer.Surname;
                    worksheet.Cells[string.Format("C{0}", index)].Value = customer.Mobile;
                    worksheet.Cells[string.Format("D{0}", index)].Value = customer.Email;
                    worksheet.Cells[string.Format("E{0}", index)].Value = customer.DateOfBirth;
                    worksheet.Cells[string.Format("F{0}", index)].Value = customer.Address;
                    worksheet.Cells[string.Format("G{0}", index)].Value = customer.Status;
                    worksheet.Cells[string.Format("H{0}", index)].Value = customer.DistributorName;
                    worksheet.Cells[string.Format("I{0}", index)].Value = customer.DistributorAddress;
                    worksheet.Cells[string.Format("J{0}", index++)].Value = customer.DistributorContact;
                });

                package.Save();
            }

            FileStream fs = new FileStream(localFilePath, FileMode.Open);
            FileStreamResult fileStreamResult = new FileStreamResult(fs, "application/vnd.ms-excel");
            fileStreamResult.FileDownloadName = "Customer Report.xlsx";
            return fileStreamResult;
            //return Ok(customerResourceParameters);
        }

        [HttpGet("ExportToPdf", Name = "ExportToPdf")]
        [Authorize(Policy = Permissions.CustomerRead)]
        public async Task<IActionResult> ExportToPdf(ExportCustomerResourceParameters customerResourceParameters, [FromServices] INodeServices nodeServices)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<CustomerDto, Customer>
               (customerResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_typeHelperService.TypeHasProperties<CustomerDto>
                (customerResourceParameters.Fields))
            {
                return BadRequest();
            }

            var customers = this.GetCustomers(customerResourceParameters);

            var htmlContent = CreateHTMLTable(customers);
            var result = await nodeServices.InvokeAsync<byte[]>("./pdfReport", htmlContent);
            HttpContext.Response.ContentType = "application/pdf";
            string filename = @"report.pdf";
            HttpContext.Response.Headers.Add("x-filename", filename);
            HttpContext.Response.Headers.Add("Access-Control-Expose-Headers", "x-filename");
            HttpContext.Response.Body.Write(result, 0, result.Length);
            return new ContentResult();

        }

        [HttpGet("LookUp", Name = "GetCustomerAsLookUp")]
        [Authorize(Policy = Permissions.CustomerRead)]
        public IActionResult GetCustomerAsLookUp([FromHeader(Name = "Accept")] string mediaType)
        {
            return Ok(this.GetCustomerAsLookUp());
        }

        [HttpGet("{id}", Name = "GetCustomer")]
        [Authorize(Policy = Permissions.CustomerRead)]
        public IActionResult GetCustomer(Guid id, [FromQuery] string fields)
        {
            if (!_typeHelperService.TypeHasProperties<CustomerDto>
              (fields))
            {
                return BadRequest();
            }

            var customerFromRepo = _repo.FindByKey(id);

            if (customerFromRepo == null)
            {
                return NotFound();
            }

            var customer = Mapper.Map<CustomerDto>(customerFromRepo);

            var links = Utilities.CreateLinks(id, fields, _urlHelper, "Customer");

            var linkedResourceToReturn = customer.ShapeData(fields)
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return Ok(linkedResourceToReturn);
        }

        [HttpPost(Name = "CreateCustomer")]
        [Authorize(Policy = Permissions.CustomerCreate)]
        // [RequestHeaderMatchesMediaType("Content-Type",
        //     new[] { "application/vnd.marvin.customer.full+json" })]
        public IActionResult CreateCustomer([FromBody] CustomerForCreationDto customer)
        {
            if (customer == null)
            {
                return BadRequest();
            }
            if (ModelState.IsValid)
            {
                var customerEntity = Mapper.Map<Customer>(customer);

                SetCreationUserData(customerEntity);

                if (!_repo.Insert(customerEntity))
                {
                    throw new Exception("Creating an customer failed on save.");
                    // return StatusCode(500, "A problem happened with handling your request.");
                }

                var customerToReturn = Mapper.Map<CustomerDto>(customerEntity);

                var links = Utilities.CreateLinks(customerToReturn.CustomerID, null, _urlHelper, "Customer");

                var linkedResourceToReturn = customerToReturn.ShapeData(null)
                    as IDictionary<string, object>;

                linkedResourceToReturn.Add("links", links);

                return CreatedAtRoute("GetCustomer",
                    new { id = linkedResourceToReturn["CustomerID"] },
                    linkedResourceToReturn);

            }
            else
            {
                var errors = ModelState.Select(x => x.Value.Errors)
                           .Where(y => y.Count > 0)
                           .ToList();
                return BadRequest(errors);
            }
        }

        [HttpDelete("{id}", Name = "DeleteCustomer")]
        [Authorize(Policy = Permissions.CustomerDelete)]
        public IActionResult DeleteCustomer(Guid id)
        {
            var customerFromRepo = _repo.FindByKey(id);
            if (customerFromRepo == null)
            {
                return NotFound();
            }

            //_appRepository.DeleteCustomer(customerFromRepo);
            //....... Soft Delete
            customerFromRepo.IsDelete = true;

            if (!_repo.Update(customerFromRepo))
            {
                throw new Exception($"Deleting customer {id} failed on save.");
            }

            return NoContent();
        }

        [HttpPut("{id}", Name = "UpdateCustomer")]
        [Authorize(Policy = Permissions.CustomerUpdate)]
        public IActionResult UpdateCustomer(Guid id, [FromBody] CustomerForUpdationDto customer)
        {
            if (customer == null)
            {
                return BadRequest();
            }
            if (ModelState.IsValid)
            {
                var customerFromRepo = _repo.FindByKey(id);

                if (customerFromRepo == null)
                {
                    return NotFound();
                }
                SetItemHistoryData(customer, customerFromRepo);

                Mapper.Map(customer, customerFromRepo);
                if (!_repo.Update(customerFromRepo))
                {
                    throw new Exception("Updating an customer failed on save.");
                }
                return Ok(customerFromRepo);
            }
            else
            {
                var errors = ModelState.Select(x => x.Value.Errors)
                           .Where(y => y.Count > 0)
                           .ToList();
                return BadRequest(errors);
            }
        }

        [HttpPatch("{id}", Name = "PartiallyUpdateCustomer")]
        [Authorize(Policy = Permissions.CustomerUpdate)]
        public IActionResult PartiallyUpdateCustomer(Guid id,
                    [FromBody] JsonPatchDocument<CustomerForUpdationDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var bookForAuthorFromRepo = _repo.FindByKey(id);

            if (bookForAuthorFromRepo == null)
            {
                return NotFound();
            }

            var bookToPatch = Mapper.Map<CustomerForUpdationDto>(bookForAuthorFromRepo);

            patchDoc.ApplyTo(bookToPatch, ModelState);

            // patchDoc.ApplyTo(bookToPatch);

            TryValidateModel(bookToPatch);

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            SetItemHistoryData(bookToPatch, bookForAuthorFromRepo);
            Mapper.Map(bookToPatch, bookForAuthorFromRepo);

            if (!_repo.Update(bookForAuthorFromRepo))
            {
                throw new Exception($"Patching  Occurrence Book {id} failed on save.");
            }
            return NoContent();
        }

        [HttpPost("validate", Name = "ValidateNationalId")]
        [Authorize(Policy = Permissions.CustomerCreate)]
        // [RequestHeaderMatchesMediaType("Content-Type",
        //     new[] { "application/vnd.marvin.customer.full+json" })]
        public IActionResult ValidateNationalId([FromBody] CustomerValidationResourceParameters customerValidationResourceParameters)
        {
            /*
                TODO: 
                    1. Move in repository
                    2. Add service call (remove dummy data)
            */
            string response = GetCustomerByNationalID(customerValidationResourceParameters.NationalID);
            NationalIDResponse customerObj = GetCustomerFromResponse(response);

            CustomerIPRSDto customerData = new CustomerIPRSDto()
            {
                ErrorCode = customerObj.ErrorCode,
                ErrorMessage = customerObj.ErrorMessage,
                ErrorOcurred = customerObj.ErrorOcurred,
                NationalID = customerObj.Serial_Number,
                SerialNumber = customerObj.Serial_Number,
                Firstname = customerObj.First_Name,
                Surname = customerObj.Surname,
                Othername = customerObj.Other_Name,
                Gender = customerObj.Gender,
                DateOfBirth = customerObj.Date_of_Birth != null ? customerObj.Date_of_Birth.Value : DateTime.MinValue,
                Citizenship = customerObj.Citizenship,
                Occupation = customerObj.Occupation,
                Address = customerObj.Place_of_Live,
                Pin = customerObj.Pin
            };

            return Ok(customerData);
        }

        [HttpGet("report", Name = "GetCustomerRegistrationResults")]
        [Authorize(Policy = Permissions.CustomerRead)]
        public IActionResult GetCustomerRegistrationResults(CustomerRegistrationReportParameters customerRegistrationReportParameters)
        {
            IEnumerable<CustomerRegistrationReportDto> customerReports = new List<CustomerRegistrationReportDto>();
            if (customerRegistrationReportParameters.Year != 0)
            {
                customerReports = _repo.FindBy(c => c.CreatedOn.Year == customerRegistrationReportParameters.Year)
                            .GroupBy(c => new { c.CreatedOn.Month })
                            .Select(g => new CustomerRegistrationReportDto()
                            {
                                CustomerCount = g.Count(),
                                MaleCount = g.Where(c => c.Gender == "M").Count(),
                                FemaleCount = g.Where(c => c.Gender == "F").Count(),
                                Month = g.Key.Month
                            })
                            .OrderBy(c => c.Month);
            }
            else
            {
                customerReports = _repo.FindBy(c => c.CreatedOn.Year >= DateTime.Now.AddYears(-5).Year)
                            .GroupBy(c => new { c.CreatedOn.Year })
                             .Select(g => new CustomerRegistrationReportDto()
                             {
                                 CustomerCount = g.Count(),
                                 MaleCount = g.Where(c => c.Gender == "M").Count(),
                                 FemaleCount = g.Where(c => c.Gender == "F").Count(),
                                 Year = g.Key.Year
                             })
                            .OrderBy(c => c.Year);
            }

            //IEnumerable<CustomerRegistrationReportDto> customerReports = _appRepository.GetCustomerRegistrationReport(customerRegistrationReportParameters);
            return Ok(customerReports);
        }


        [HttpOptions]
        public IActionResult GetCustomerOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST");
            return Ok();
        }

        private string CreateHTMLTable(PagedList<Customer> customerList)
        {
            PropertyInfo[] allProperties = (new Customer()).GetType().GetProperties();
            int iterCount = 2;
            string table = "<table  style='border:1px solid black;border-collapse:collapse;'><thead><tr>";

            // table += "<th style='border:1px solid black;'>CustomerID  </th>";
            table += "<th style='border:1px solid black;'>CustomerName</th>";
            table += "<th style='border:1px solid black;'>Mobile</th>";
            table += "<th style='border:1px solid black;'>Landline</th>";
            table += "<th style='border:1px solid black;'>CustomerEmail</th>";
            table += "<th style='border:1px solid black;'>DateOfBirth</th>";
            table += "<th style='border:1px solid black;'>CustomerAddress</th>";
            table += "<th style='border:1px solid black;'>Status</th>";
            table += "<th style='border:1px solid black;'>DistributorName</th>";
            table += "<th style='border:1px solid black;'>DistributorAddress</th>";
            table += "<th style='border:1px solid black;'>DistributorContact</th>";


            table += "</tr></thead><tbody>";
            foreach (Customer customer in customerList)
            {
                table += "<tr>";
                // table += "<td style='border:1px solid black;'>" + customer.CustomerID + "</td>";
                table += "<td style='border:1px solid black;'>" + customer.Firstname + "</td>";
                table += "<td style='border:1px solid black;'>" + customer.Surname + "</td>";
                table += "<td style='border:1px solid black;'>" + customer.Mobile + "</td>";
                table += "<td style='border:1px solid black;'>" + customer.Email + "</td>";
                table += "<td style='border:1px solid black;'>" + customer.DateOfBirth + "</td>";
                table += "<td style='border:1px solid black;'>" + customer.Address + "</td>";
                table += "<td style='border:1px solid black;'>" + customer.Status + "</td>";
                table += "<td style='border:1px solid black;'>" + customer.DistributorName + "</td>";
                table += "<td style='border:1px solid black;'>" + customer.DistributorAddress + "</td>";
                table += "<td style='border:1px solid black;'>" + customer.DistributorContact + "</td>";

                table += "</tr>";
                iterCount++;
            }
            table += "</tbody></table>";
            return table;
        }

        private void SetItemHistoryData(CustomerForUpdationDto model, Customer modelRepo)
        {
            model.CreatedOn = modelRepo.CreatedOn;
            if (modelRepo.CreatedBy != null)
                model.CreatedBy = modelRepo.CreatedBy.Value;
            model.UpdatedOn = DateTime.Now;
            //TODO: Remove this line <model.UpdatedBy = null;>
            //model.UpdatedBy = null;
            // var CustomerID = User.Claims.FirstOrDefault(cl => cl.Type == "CustomerID");
            // model.UpdatedBy = new Guid(CustomerID.Value);
        }

        private void SetCreationUserData(Customer model)
        {
            // var CustomerID = User.Claims.FirstOrDefault(cl => cl.Type == "CustomerID");
            // model.CreatedBy = new Guid(CustomerID.Value);
            //TODO: Remove this line <model.UpdatedBy = null;>
            model.UpdatedBy = null;
        }

        private PagedList<Customer> GetCustomers(ExportCustomerResourceParameters CustomersResourceParameters)
        {
            var collectionBeforePaging =
                _repo.Query().Where(c => !c.IsDelete)
                .ApplySort(CustomersResourceParameters.OrderBy,
                _propertyMappingService.GetPropertyMapping<CustomerDto, Customer>());

            if (!string.IsNullOrEmpty(CustomersResourceParameters.SearchQuery))
            {
                // trim & ignore casing
                var searchQueryForWhereClause = CustomersResourceParameters.SearchQuery
                    .Trim().ToLowerInvariant();

                collectionBeforePaging = collectionBeforePaging
                    .Where(a => a.Firstname.ToLowerInvariant().Contains(searchQueryForWhereClause)
                    || a.Surname.ToLowerInvariant().Contains(searchQueryForWhereClause)
                    || a.NationalID.ToLowerInvariant().Contains(searchQueryForWhereClause)
                    || a.Mobile.ToLowerInvariant().Contains(searchQueryForWhereClause)
                    || Convert.ToString(a.DateOfBirth).ToLowerInvariant().Contains(searchQueryForWhereClause)
                    || a.Email.ToLowerInvariant().Contains(searchQueryForWhereClause)
                    || a.DistributorName.ToLowerInvariant().Contains(searchQueryForWhereClause)
                    || a.DistributorContact.ToLowerInvariant().Contains(searchQueryForWhereClause));

            }

            return PagedList<Customer>.Create(collectionBeforePaging,
                CustomersResourceParameters.PageNumber,
                CustomersResourceParameters.PageSize);
        }

        public PagedList<Customer> GetCustomers(CustomerResourceParameters CustomersResourceParameters)
        {
            var collectionBeforePaging =
                _repo.Query().Where(c => !c.IsDelete)
                .ApplySort(CustomersResourceParameters.OrderBy,
                _propertyMappingService.GetPropertyMapping<CustomerDto, Customer>());

            if (!string.IsNullOrEmpty(CustomersResourceParameters.SearchQuery))
            {
                // trim & ignore casing
                var searchQueryForWhereClause = CustomersResourceParameters.SearchQuery
                    .Trim().ToLowerInvariant();

                collectionBeforePaging = collectionBeforePaging
                    .Where(a => a.Firstname.ToLowerInvariant().Contains(searchQueryForWhereClause)
                    || a.Surname.ToLowerInvariant().Contains(searchQueryForWhereClause)
                    || a.NationalID.ToLowerInvariant().Contains(searchQueryForWhereClause)
                    || a.Mobile.ToLowerInvariant().Contains(searchQueryForWhereClause)
                    || Convert.ToString(a.DateOfBirth).ToLowerInvariant().Contains(searchQueryForWhereClause)
                    || a.Email.ToLowerInvariant().Contains(searchQueryForWhereClause)
                    || a.DistributorName.ToLowerInvariant().Contains(searchQueryForWhereClause)
                    || Convert.ToString(a.DistributorContact).ToLowerInvariant().Contains(searchQueryForWhereClause));

            }

            return PagedList<Customer>.Create(collectionBeforePaging,
                CustomersResourceParameters.PageNumber,
                CustomersResourceParameters.PageSize);
        }

        private IEnumerable<LookUpItem> GetCustomerAsLookUp()
        {
            return (from a in _repo.Query()
                    where a.IsDelete == false
                    select new LookUpItem
                    {
                        ID = a.CustomerID,
                        Name = a.Firstname + " " + a.Surname
                    }).ToList();
        }

        private NationalIDResponse GetCustomerFromResponse(string response)
        {
            NationalIDResponse objResponse = JsonConvert.DeserializeObject<NationalIDResponse>(response);

            return objResponse;
        }
        private string GetCustomerByNationalID(string nationalId)
        {
            if (nationalId.Equals("1234567890"))
            {
                return @"{
	                    	'ErrorCode' : '',
	                    	'ErrorMessage' : '',
	                    	'ErrorOcurred' : false,
	                    	'Citizenship' : null,
	                    	'Clan' : null,
	                    	'Date_of_Birth' : '10\/7\/1981 12:00:00 AM',
	                    	'Date_of_Death' : null,
	                    	'Ethnic_Group' : null,
	                    	'Family' : null,
	                    	'Fingerprint' : null,
	                    	'First_Name' : 'Nick',
	                    	'Gender' : 'M',
	                    	'ID_Number' : null,
	                    	'Occupation' : null,
	                    	'Other_Name' : 'Nicky',
	                    	'Photo' : null,
	                    	'Pin' : '12345',
	                    	'Place_of_Birth' : null,
	                    	'Place_of_Death' : null,
	                    	'Place_of_Live' : 'test address for national id 1234567890',
	                    	'Signature' : null,
	                    	'Surname' : 'Jones',
	                    	'Date_of_Issue' : null,
	                    	'RegOffice' : null,
	                    	'Serial_Number' : '1234567890'
	                    }";
            }
            else if (nationalId.Equals("2345678901"))
            {
                return @"{
	                	'ErrorCode' : '',
	                	'ErrorMessage' : '',
	                	'ErrorOcurred' : false,
	                	'Citizenship' : null,
	                	'Clan' : null,
	                	'Date_of_Birth' : '1\/12\/1980 12:00:00 AM',
	                	'Date_of_Death' : null,
	                	'Ethnic_Group' : null,
	                	'Family' : null,
	                	'Fingerprint' : null,
	                	'First_Name' : 'Angelina',
	                	'Gender' : 'F',
	                	'ID_Number' : null,
	                	'Occupation' : null,
	                	'Other_Name' : 'Angel',
	                	'Photo' : null,
	                	'Pin' : '12345',
	                	'Place_of_Birth' : null,
	                	'Place_of_Death' : null,
	                	'Place_of_Live' : 'test address for national id 2345678901',
	                	'Signature' : null,
	                	'Surname' : 'Jolie',
	                	'Date_of_Issue' : null,
	                	'RegOffice' : null,
	                	'Serial_Number' : '2345678901'
	                }";
            }
            else if (nationalId.Equals("3456789012"))
            {
                return @"{
	                    	'ErrorCode' : '',
	                    	'ErrorMessage' : '',
	                    	'ErrorOcurred' : false,
	                    	'Citizenship' : null,
	                    	'Clan' : null,
	                    	'Date_of_Birth' : '5\/24\/1980 12:00:00 AM',
	                    	'Date_of_Death' : null,
	                    	'Ethnic_Group' : null,
	                    	'Family' : null,
	                    	'Fingerprint' : null,
	                    	'First_Name' : 'John',
	                    	'Gender' : 'M',
	                    	'ID_Number' : null,
	                    	'Occupation' : null,
	                    	'Other_Name' : 'Johnny',
	                    	'Photo' : null,
	                    	'Pin' : '12345',
	                    	'Place_of_Birth' : null,
	                    	'Place_of_Death' : null,
	                    	'Place_of_Live' : 'test address for national id 3456789012',
	                    	'Signature' : null,
	                    	'Surname' : 'Doe',
	                    	'Date_of_Issue' : null,
	                    	'RegOffice' : null,
	                    	'Serial_Number' : '3456789012'
	                    }";
            }
            else if (nationalId.Equals("4567890123"))
            {
                return @"{
	                    	'ErrorCode' : '',
	                    	'ErrorMessage' : '',
	                    	'ErrorOcurred' : false,
	                    	'Citizenship' : null,
	                    	'Clan' : null,
	                    	'Date_of_Birth' : '10\/15\/1983 12:00:00 AM',
	                    	'Date_of_Death' : null,
	                    	'Ethnic_Group' : null,
	                    	'Family' : null,
	                    	'Fingerprint' : null,
	                    	'First_Name' : 'Jack',
	                    	'Gender' : 'M',
	                    	'ID_Number' : null,
	                    	'Occupation' : null,
	                    	'Other_Name' : 'Jacky',
	                    	'Photo' : null,
	                    	'Pin' : '12345',
	                    	'Place_of_Birth' : null,
	                    	'Place_of_Death' : null,
	                    	'Place_of_Live' : 'test address for national id 4567890123',
	                    	'Signature' : null,
	                    	'Surname' : 'Sparrow',
	                    	'Date_of_Issue' : null,
	                    	'RegOffice' : null,
	                    	'Serial_Number' : '4567890123'
	                    }";
            }
            else if (nationalId.Equals("5678901234"))
            {
                return @"{
	                    	'ErrorCode' : '',
	                    	'ErrorMessage' : '',
	                    	'ErrorOcurred' : false,
	                    	'Citizenship' : null,
	                    	'Clan' : null,
	                    	'Date_of_Birth' : '10\/15\/1983 12:00:00 AM',
	                    	'Date_of_Death' : null,
	                    	'Ethnic_Group' : null,
	                    	'Family' : null,
	                    	'Fingerprint' : null,
	                    	'First_Name' : 'Brad',
	                    	'Gender' : 'M',
	                    	'ID_Number' : null,
	                    	'Occupation' : null,
	                    	'Other_Name' : 'Brad',
	                    	'Photo' : null,
	                    	'Pin' : '23456',
	                    	'Place_of_Birth' : null,
	                    	'Place_of_Death' : null,
	                    	'Place_of_Live' : 'test address for national id 5678901234',
	                    	'Signature' : null,
	                    	'Surname' : 'Pitt',
	                    	'Date_of_Issue' : null,
	                    	'RegOffice' : null,
	                    	'Serial_Number' : '5678901234'
	                    }";
            }
            else if (nationalId.Equals("6789012345"))
            {
                return @"{
	                    	'ErrorCode' : '',
	                    	'ErrorMessage' : '',
	                    	'ErrorOcurred' : false,
	                    	'Citizenship' : null,
	                    	'Clan' : null,
	                    	'Date_of_Birth' : '10\/15\/1983 12:00:00 AM',
	                    	'Date_of_Death' : null,
	                    	'Ethnic_Group' : null,
	                    	'Family' : null,
	                    	'Fingerprint' : null,
	                    	'First_Name' : 'Steve',
	                    	'Gender' : 'M',
	                    	'ID_Number' : null,
	                    	'Occupation' : null,
	                    	'Other_Name' : 'Steve',
	                    	'Photo' : null,
	                    	'Pin' : '23456',
	                    	'Place_of_Birth' : null,
	                    	'Place_of_Death' : null,
	                    	'Place_of_Live' : 'test address for national id 6789012345',
	                    	'Signature' : null,
	                    	'Surname' : 'Rogers',
	                    	'Date_of_Issue' : null,
	                    	'RegOffice' : null,
	                    	'Serial_Number' : '6789012345'
	                    }";
            }
            else if (nationalId.Equals("7890123456"))
            {
                return @"{
	                    	'ErrorCode' : '',
	                    	'ErrorMessage' : '',
	                    	'ErrorOcurred' : false,
	                    	'Citizenship' : null,
	                    	'Clan' : null,
	                    	'Date_of_Birth' : '10\/15\/1983 12:00:00 AM',
	                    	'Date_of_Death' : null,
	                    	'Ethnic_Group' : null,
	                    	'Family' : null,
	                    	'Fingerprint' : null,
	                    	'First_Name' : 'Tony',
	                    	'Gender' : 'M',
	                    	'ID_Number' : null,
	                    	'Occupation' : null,
	                    	'Other_Name' : null,
	                    	'Photo' : null,
	                    	'Pin' : '23456',
	                    	'Place_of_Birth' : null,
	                    	'Place_of_Death' : null,
	                    	'Place_of_Live' : 'test address for national id 7890123456',
	                    	'Signature' : null,
	                    	'Surname' : 'Stark',
	                    	'Date_of_Issue' : null,
	                    	'RegOffice' : null,
	                    	'Serial_Number' : '7890123456'
	                    }";
            }
            else if (nationalId.Equals("8901234567"))
            {
                return @"{
	                    	'ErrorCode' : '',
	                    	'ErrorMessage' : '',
	                    	'ErrorOcurred' : false,
	                    	'Citizenship' : null,
	                    	'Clan' : null,
	                    	'Date_of_Birth' : '11\/18\/1983 12:00:00 AM',
	                    	'Date_of_Death' : null,
	                    	'Ethnic_Group' : null,
	                    	'Family' : null,
	                    	'Fingerprint' : null,
	                    	'First_Name' : 'Johnny',
	                    	'Gender' : 'M',
	                    	'ID_Number' : null,
	                    	'Occupation' : null,
	                    	'Other_Name' : null,
	                    	'Photo' : null,
	                    	'Pin' : '23456',
	                    	'Place_of_Birth' : null,
	                    	'Place_of_Death' : null,
	                    	'Place_of_Live' : 'test address for national id 8901234567',
	                    	'Signature' : null,
	                    	'Surname' : 'Depp',
	                    	'Date_of_Issue' : null,
	                    	'RegOffice' : null,
	                    	'Serial_Number' : '8901234567'
	                    }";
            }
            else if (nationalId.Equals("9012345678"))
            {
                return @"{
	                    	'ErrorCode' : '',
	                    	'ErrorMessage' : '',
	                    	'ErrorOcurred' : false,
	                    	'Citizenship' : null,
	                    	'Clan' : null,
	                    	'Date_of_Birth' : '11\/18\/1983 12:00:00 AM',
	                    	'Date_of_Death' : null,
	                    	'Ethnic_Group' : null,
	                    	'Family' : null,
	                    	'Fingerprint' : null,
	                    	'First_Name' : 'Tom',
	                    	'Gender' : 'M',
	                    	'ID_Number' : null,
	                    	'Occupation' : null,
	                    	'Other_Name' : 'Tommy',
	                    	'Photo' : null,
	                    	'Pin' : '23456',
	                    	'Place_of_Birth' : null,
	                    	'Place_of_Death' : null,
	                    	'Place_of_Live' : '9012345678',
	                    	'Signature' : null,
	                    	'Surname' : 'Cruise',
	                    	'Date_of_Issue' : null,
	                    	'RegOffice' : null,
	                    	'Serial_Number' : '9012345678'
	                    }";
            }

            return "{'ErrorOcurred':true,'ErrorCode':'ISB-105','ErrorMessage':'There is no information for requested search parameters'}";

        }
    }
}