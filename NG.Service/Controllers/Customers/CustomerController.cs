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
using NG.Service.Controllers.Customers;

namespace naturalgas.Controllers.Customers
{
    [Route("api/customers")]
    public class CustomerController : Controller
    {
        //private IAppRepository _appRepository;
        private IUrlHelper _urlHelper;
        private IPropertyMappingService _propertyMappingService;
        private ITypeHelperService _typeHelperService;
        private readonly IHostingEnvironment _hostingEnvironment;
        
        //public CustomerController(IAppRepository appRepository,
        public CustomerController(
           IUrlHelper urlHelper,
           IPropertyMappingService propertyMappingService,
           ITypeHelperService typeHelperService,
           IHostingEnvironment hostingEnvironment)
        {
            //_appRepository = appRepository;
            _urlHelper = urlHelper;
            _propertyMappingService = propertyMappingService;
            _typeHelperService = typeHelperService;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet(Name = "GetCustomers")]
        [HttpHead]
        //[Authorize(Policy = Permissions.CustomerRead)]
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

            var customerFromRepo = _appRepository.GetCustomers(customerResourceParameters);

            var customer = Mapper.Map<IEnumerable<CustomerDto>>(customerFromRepo);

            if (mediaType == "application/vnd.marvin.hateoas+json")
            {
                var paginationMetadata = new
                {
                    totalCount = customerFromRepo.TotalCount,
                    pageSize = customerFromRepo.PageSize,
                    currentPage = customerFromRepo.CurrentPage,
                    totalPages = customerFromRepo.TotalPages,
                };

                Response.Headers.Add("X-Pagination",
                    Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));
                Response.Headers.Add("Access-Control-Expose-Headers", "ETag, X-Pagination");

                var links = CreateLinksForCustomer(customerResourceParameters,
                    customerFromRepo.HasNext, customerFromRepo.HasPrevious);

                var shapedcustomer = customer.ShapeData(customerResourceParameters.Fields);

                var shapedcustomerWithLinks = shapedcustomer.Select(occType =>
                {
                    var customerAsDictionary = occType as IDictionary<string, object>;
                    var customerLinks = CreateLinksForCustomer(
                        (Guid)customerAsDictionary["Id"], customerResourceParameters.Fields);

                    customerAsDictionary.Add("links", customerLinks);

                    return customerAsDictionary;
                });

                var linkedCollectionResource = new
                {
                    value = shapedcustomerWithLinks,
                    links = links
                };

                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = customerFromRepo.HasPrevious ?
                    CreateCustomerResourceUri(customerResourceParameters,
                    ResourceUriType.PreviousPage) : null;

                var nextPageLink = customerFromRepo.HasNext ?
                    CreateCustomerResourceUri(customerResourceParameters,
                    ResourceUriType.NextPage) : null;

                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = customerFromRepo.TotalCount,
                    pageSize = customerFromRepo.PageSize,
                    currentPage = customerFromRepo.CurrentPage,
                    totalPages = customerFromRepo.TotalPages
                };

                Response.Headers.Add("X-Pagination",
                    Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));
                Response.Headers.Add("Access-Control-Expose-Headers", "ETag, X-Pagination");

                return Ok(customer.ShapeData(customerResourceParameters.Fields));
            }
        }

        [HttpGet("ExportToExcel", Name = "ExportToExcel")]
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

            var customers = _appRepository.GetCustomers(customerResourceParameters);

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

            var customers = _appRepository.GetCustomers(customerResourceParameters);

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
        //[Authorize(Policy = Permissions.CustomerRead)]
        public IActionResult GetCustomerAsLookUp([FromHeader(Name = "Accept")] string mediaType)
        {
            return Ok(_appRepository.GetCustomerAsLookUp());
        }

        [HttpGet("{id}", Name = "GetCustomer")]
        //[Authorize(Policy = Permissions.CustomerRead)]
        public IActionResult GetCustomer(Guid id, [FromQuery] string fields)
        {
            if (!_typeHelperService.TypeHasProperties<CustomerDto>
              (fields))
            {
                return BadRequest();
            }

            var customerFromRepo = _appRepository.GetCustomer(id);

            if (customerFromRepo == null)
            {
                return NotFound();
            }

            var customer = Mapper.Map<CustomerDto>(customerFromRepo);

            var links = CreateLinksForCustomer(id, fields);

            var linkedResourceToReturn = customer.ShapeData(fields)
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return Ok(linkedResourceToReturn);
        }

        [HttpPost(Name = "CreateCustomer")]
        //[Authorize(Policy = Permissions.CustomerCreate)]
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

                _appRepository.AddCustomer(customerEntity);

                if (!_appRepository.Save())
                {
                    throw new Exception("Creating an customer failed on save.");
                    // return StatusCode(500, "A problem happened with handling your request.");
                }

                var customerToReturn = Mapper.Map<CustomerDto>(customerEntity);

                var links = CreateLinksForCustomer(customerToReturn.CustomerID, null);

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

        [HttpPost("{id}")]
        public IActionResult BlockCustomerCreation(Guid id)
        {
            if (_appRepository.CustomerExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            return NotFound();
        }

        [HttpDelete("{id}", Name = "DeleteCustomer")]
        //[Authorize(Policy = Permissions.CustomerDelete)]
        public IActionResult DeleteCustomer(Guid id)
        {
            var customerFromRepo = _appRepository.GetCustomer(id);
            if (customerFromRepo == null)
            {
                return NotFound();
            }

            //_appRepository.DeleteCustomer(customerFromRepo);
            //....... Soft Delete
            customerFromRepo.IsDelete = true;

            if (!_appRepository.Save())
            {
                throw new Exception($"Deleting customer {id} failed on save.");
            }

            return NoContent();
        }

        [HttpPut("{id}", Name = "UpdateCustomer")]
        //[Authorize(Policy = Permissions.CustomerUpdate)]
        public IActionResult UpdateCustomer(Guid id, [FromBody] CustomerForUpdationDto customer)
        {
            if (customer == null)
            {
                return BadRequest();
            }
            if (ModelState.IsValid)
            {
                var customerFromRepo = _appRepository.GetCustomer(id);

                if (customerFromRepo == null)
                {
                    return NotFound();
                }
                SetItemHistoryData(customer, customerFromRepo);

                Mapper.Map(customer, customerFromRepo);
                _appRepository.UpdateCustomer(customerFromRepo);
                if (!_appRepository.Save())
                {
                    throw new Exception("Updating an customer failed on save.");
                    // return StatusCode(500, "A problem happened with handling your request.");
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
        //[Authorize(Policy = Permissions.CustomerUpdate)]
        public IActionResult PartiallyUpdateCustomer(Guid id,
                    [FromBody] JsonPatchDocument<CustomerForUpdationDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var bookForAuthorFromRepo = _appRepository.GetCustomer(id);

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

            _appRepository.UpdateCustomer(bookForAuthorFromRepo);

            if (!_appRepository.Save())
            {
                throw new Exception($"Patching  Occurrence Book {id} failed on save.");
            }
            return NoContent();
        }

        [HttpPost("validate", Name = "ValidateNationalId")]
        //[Authorize(Policy = Permissions.CustomerCreate)]
        // [RequestHeaderMatchesMediaType("Content-Type",
        //     new[] { "application/vnd.marvin.customer.full+json" })]
        public IActionResult ValidateNationalId([FromBody] CustomerValidationResourceParameters customerValidationResourceParameters)
        {
            var customerObj = _appRepository.ValidateNationalId(customerValidationResourceParameters.NationalID);

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

        private string CreateCustomerResourceUri(
            CustomerResourceParameters customerResourceParameters,
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetCustomer",
                      new
                      {
                          fields = customerResourceParameters.Fields,
                          orderBy = customerResourceParameters.OrderBy,
                          searchQuery = customerResourceParameters.SearchQuery,
                          pageNumber = customerResourceParameters.PageNumber - 1,
                          pageSize = customerResourceParameters.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetCustomer",
                      new
                      {
                          fields = customerResourceParameters.Fields,
                          orderBy = customerResourceParameters.OrderBy,
                          searchQuery = customerResourceParameters.SearchQuery,
                          pageNumber = customerResourceParameters.PageNumber + 1,
                          pageSize = customerResourceParameters.PageSize
                      });
                case ResourceUriType.Current:
                default:
                    return _urlHelper.Link("GetCustomer",
                    new
                    {
                        fields = customerResourceParameters.Fields,
                        orderBy = customerResourceParameters.OrderBy,
                        searchQuery = customerResourceParameters.SearchQuery,
                        pageNumber = customerResourceParameters.PageNumber,
                        pageSize = customerResourceParameters.PageSize
                    });
            }
        }

        private IEnumerable<LinkDto> CreateLinksForCustomer(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCustomer", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetCustomer", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteCustomer", new { id = id }),
              "delete_customer",
              "DELETE"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateBookForCustomer", new { customerId = id }),
              "create_book_for_customer",
              "POST"));

            links.Add(
               new LinkDto(_urlHelper.Link("GetBooksForCustomer", new { customerId = id }),
               "books",
               "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForCustomer(
            CustomerResourceParameters customerResourceParameters,
            bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            // self 
            links.Add(
               new LinkDto(CreateCustomerResourceUri(customerResourceParameters,
               ResourceUriType.Current)
               , "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateCustomerResourceUri(customerResourceParameters,
                  ResourceUriType.NextPage),
                  "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new LinkDto(CreateCustomerResourceUri(customerResourceParameters,
                    ResourceUriType.PreviousPage),
                    "previousPage", "GET"));
            }

            return links;
        }

        private void SetItemHistoryData(CustomerForUpdationDto model, Customer modelRepo)
        {
            model.CreatedOn = modelRepo.CreatedOn;
            if (modelRepo.CreatedBy != null)
                model.CreatedBy = modelRepo.CreatedBy.Value;
            model.UpdatedOn = DateTime.Now;
            //TODO: Remove this line <model.UpdatedBy = null;>
            model.UpdatedBy = null;
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

    }
}
