using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Authorization;
using NG.Common.Services;
using NG.Common;
using NG.Domain.Departments;
using NG.Application.Departments;
using NG.Service.Controllers.Departments;
using NG.Common.Helpers;
using NG.Common.Extensions;

namespace NG.Service.Controllers
{
    [Route("api/departments")]
    public class DepartmentsController : Controller
    {
        private IUrlHelper _urlHelper;
        private IPropertyMappingService _propertyMappingService;
        private ITypeHelperService _typeHelperService;

        private IGenericRepository<Department> _repo;

        public DepartmentsController(IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            ITypeHelperService typeHelperService,
            IGenericRepository<Department> repo)
        {
            _urlHelper = urlHelper;
            _propertyMappingService = propertyMappingService;
            _typeHelperService = typeHelperService;
            _repo = repo;
        }


        [HttpGet(Name = "GetDepartments")]
        [HttpHead]
        public IActionResult GetDepartments(DepartmentsResourceParameters departmentsResourceParameters,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<DepartmentDto, Department>
               (departmentsResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_typeHelperService.TypeHasProperties<DepartmentDto>
                (departmentsResourceParameters.Fields))
            {
                return BadRequest();
            }

            var collectionBeforePaging =
                _repo.Query().Where(a => a.IsDelete == false).ApplySort(departmentsResourceParameters.OrderBy,
                _propertyMappingService.GetPropertyMapping<DepartmentDto, Department>());

            if (!string.IsNullOrEmpty(departmentsResourceParameters.SearchQuery))
            {
                var searchQueryForWhereClause = departmentsResourceParameters.SearchQuery
                    .Trim().ToLowerInvariant();

                collectionBeforePaging = collectionBeforePaging
                    .Where(a => a.DepartmentName.ToLowerInvariant().Contains(searchQueryForWhereClause)
                    || (a.DepartmentDespcription != null
                        && a.DepartmentDespcription.ToLowerInvariant().Contains(searchQueryForWhereClause)));
            }

            var departmentsFromRepo = PagedList<Department>.Create(collectionBeforePaging,
                departmentsResourceParameters.PageNumber,
                departmentsResourceParameters.PageSize);

            var departments = Mapper.Map<IEnumerable<DepartmentDto>>(departmentsFromRepo);
            var shapedDepartments = departments.ShapeData(departmentsResourceParameters.Fields);

            if (mediaType == "application/vnd.marvin.hateoas+json")
            {
                var paginationMetadata = departmentsFromRepo.GetHateosMetadata();
                Response.Headers.Add("X-Pagination",
                    Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

                var links = Utilities.CreateLinks(departmentsResourceParameters,
                    departmentsFromRepo.HasNext, departmentsFromRepo.HasPrevious, _urlHelper, "Department");
                var linkedCollectionResource = new
                {
                    value = shapedDepartments.Select(department =>
                        {
                            var departmentAsDictionary = department as IDictionary<string, object>;
                            var departmentLinks = Utilities.CreateLinks(
                                (Guid)departmentAsDictionary["Id"], departmentsResourceParameters.Fields,
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
                var paginationMetadata = departmentsFromRepo.GetMetadata(departmentsResourceParameters, _urlHelper);
                Response.Headers.Add("X-Pagination",
                    Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));
                return Ok(shapedDepartments);
            }
        }

        [HttpGet("{id}", Name = "GetDepartment")]
        public IActionResult GetDepartment(Guid id, [FromQuery] string fields)
        {
            if (!_typeHelperService.TypeHasProperties<DepartmentDto>
              (fields))
            {
                return BadRequest();
            }

            var departmentFromRepo = _repo.FindByKey(id);
            if (departmentFromRepo == null)
            {
                return NotFound();
            }

            var department = Mapper.Map<DepartmentDto>(departmentFromRepo);

            var links = Utilities.CreateLinks(id, fields, _urlHelper, "Department");

            var linkedResourceToReturn = department.ShapeData(fields)
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return Ok(linkedResourceToReturn);
        }

        [HttpDelete("{id}", Name = "DeleteDepartment")]
        public IActionResult DeleteDepartment(Guid id)
        {
            var departmentFromRepo = _repo.FindByKey(id);
            if (departmentFromRepo == null)
            {
                return NotFound();
            }

            //....... Soft Delete
            departmentFromRepo.IsDelete = true;
            if (!_repo.Update(departmentFromRepo))
            {
                throw new Exception($"Deleting department {id} failed on save.");
            }

            return NoContent();
        }

        [HttpPut("{id}", Name = "UpdateDepartment")]
        public IActionResult UpdateDepartment(Guid id, [FromBody] DepartmentForUpdationDto department)
        {
            if (department == null)
            {
                return BadRequest();
            }
            var departmentRepo = _repo.FindByKey(id);

            if (departmentRepo == null)
            {
                return NotFound();
            }

            Mapper.Map(department, departmentRepo);
            if (!_repo.Update(departmentRepo))
            {
                throw new Exception("Updating an department failed on save.");
            }

            return Ok(departmentRepo);
        }

        [HttpPatch("{id}", Name = "PartiallyUpdateDepartment")]
        public IActionResult PartiallyUpdateDepartment(Guid id,
                    [FromBody] JsonPatchDocument<DepartmentForUpdationDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var departmentFromRepo = _repo.FindByKey(id);

            if (departmentFromRepo == null)
            {
                return NotFound();
            }

            var departmentToPatch = Mapper.Map<DepartmentForUpdationDto>(departmentFromRepo);
            patchDoc.ApplyTo(departmentToPatch, ModelState);

            TryValidateModel(departmentToPatch);

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            Mapper.Map(departmentToPatch, departmentFromRepo);

            if (!_repo.Update(departmentFromRepo))
            {
                throw new Exception($"Patching  department {id} failed on save.");
            }

            return NoContent();
        }

        [HttpOptions]
        public IActionResult GetDepartmentsOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST");
            return Ok();
        }
    }
}