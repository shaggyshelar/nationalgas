using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using NG.Application.Core;
using NG.Common;
using NG.Common.DTO;
using NG.Common.Enums;
using NG.Common.Extensions;
using NG.Common.Helpers;
using NG.Common.Services;
using NG.Domain.Core;
using NG.Domain.Users;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NG.Domain.Customers;
using System.Linq;

namespace NG.Service.Controllers.Core
{
    [Route("api/roles")]
    // [Authorize(Policy = "IsSuperAdmin")]
    public class RoleController : Controller
    {
        private IUrlHelper _urlHelper;
        private IPropertyMappingService _propertyMappingService;
        private ITypeHelperService _typeHelperService;
        private RoleManager<IdentityRole> _roleMgr;

        public RoleController(
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            RoleManager<IdentityRole> roleMgr,
            ITypeHelperService typeHelperService)
        {
            _roleMgr = roleMgr;
            _urlHelper = urlHelper;
            _propertyMappingService = propertyMappingService;
            _typeHelperService = typeHelperService;
        }

        [HttpGet(Name = "GetAppRoles")]
        [HttpHead]
        public IActionResult GetAppRoles(AppRolesResourceParameters esplRolesResourceParameters,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<AppRoleDto, IdentityRole>
               (esplRolesResourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_typeHelperService.TypeHasProperties<AppRoleDto>
                (esplRolesResourceParameters.Fields))
            {
                return BadRequest();
            }

            var esplRolesFromRepo = GetAppRoles(esplRolesResourceParameters);

            var esplRoles = new List<AppRoleDto>();
            esplRolesFromRepo.ForEach(esplRole =>
            {
                esplRoles.Add(
                new AppRoleDto()
                {
                    Id = new Guid(esplRole.Id),
                    Name = esplRole.Name
                });
            });

            if (mediaType == "application/vnd.marvin.hateoas+json")
            {
                var paginationMetadata = new
                {
                    totalCount = esplRolesFromRepo.TotalCount,
                    pageSize = esplRolesFromRepo.PageSize,
                    currentPage = esplRolesFromRepo.CurrentPage,
                    totalPages = esplRolesFromRepo.TotalPages,
                };

                Response.Headers.Add("X-Pagination",
                    Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));
                Response.Headers.Add("Access-Control-Expose-Headers", "ETag, X-Pagination");

                var links = CreateLinksForAppRoles(esplRolesResourceParameters,
                    esplRolesFromRepo.HasNext, esplRolesFromRepo.HasPrevious);

                var shapedAppRoles = esplRoles.ShapeData(esplRolesResourceParameters.Fields);
                var linkedCollectionResource = new
                {
                    value = shapedAppRoles,
                    links = links
                };

                return Ok(linkedCollectionResource);
            }
            else
            {
                var previousPageLink = esplRolesFromRepo.HasPrevious ?
                    CreateAppRolesResourceUri(esplRolesResourceParameters,
                    ResourceUriType.PreviousPage) : null;

                var nextPageLink = esplRolesFromRepo.HasNext ?
                    CreateAppRolesResourceUri(esplRolesResourceParameters,
                    ResourceUriType.NextPage) : null;

                var paginationMetadata = new
                {
                    previousPageLink = previousPageLink,
                    nextPageLink = nextPageLink,
                    totalCount = esplRolesFromRepo.TotalCount,
                    pageSize = esplRolesFromRepo.PageSize,
                    currentPage = esplRolesFromRepo.CurrentPage,
                    totalPages = esplRolesFromRepo.TotalPages
                };

                Response.Headers.Add("X-Pagination",
                    Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));
                Response.Headers.Add("Access-Control-Expose-Headers", "ETag, X-Pagination");

                return Ok(esplRoles);
            }
        }

        private string CreateAppRolesResourceUri(
            AppRolesResourceParameters esplRolesResourceParameters,
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetAppRoles",
                      new
                      {
                          fields = esplRolesResourceParameters.Fields,
                          orderBy = esplRolesResourceParameters.OrderBy,
                          searchQuery = esplRolesResourceParameters.SearchQuery,
                          pageNumber = esplRolesResourceParameters.PageNumber - 1,
                          pageSize = esplRolesResourceParameters.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetAppRoles",
                      new
                      {
                          fields = esplRolesResourceParameters.Fields,
                          orderBy = esplRolesResourceParameters.OrderBy,
                          searchQuery = esplRolesResourceParameters.SearchQuery,
                          pageNumber = esplRolesResourceParameters.PageNumber + 1,
                          pageSize = esplRolesResourceParameters.PageSize
                      });
                case ResourceUriType.Current:
                default:
                    return _urlHelper.Link("GetAppRoles",
                    new
                    {
                        fields = esplRolesResourceParameters.Fields,
                        orderBy = esplRolesResourceParameters.OrderBy,
                        searchQuery = esplRolesResourceParameters.SearchQuery,
                        pageNumber = esplRolesResourceParameters.PageNumber,
                        pageSize = esplRolesResourceParameters.PageSize
                    });
            }
        }

        [HttpGet("{id}", Name = "GetAppRole")]
        public IActionResult GetAppRole(Guid id, [FromQuery] string fields)
        {
            if (!_typeHelperService.TypeHasProperties<AppRoleDto>
              (fields))
            {
                return BadRequest();
            }

            var esplRoleFromRepo = GetAppRole(id);

            if (esplRoleFromRepo == null)
            {
                return NotFound();
            }

            var esplRole = Mapper.Map<AppRoleDto>(esplRoleFromRepo);

            var links = CreateLinksForAppRole(id, fields);

            var linkedResourceToReturn = esplRole.ShapeData(fields)
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return Ok(linkedResourceToReturn);
        }

        [HttpPost(Name = "CreateAppRole")]
        public async Task<IActionResult> CreateAppRole([FromBody] AppRoleForCreationDto esplRole)
        {
            if (esplRole == null)
            {
                return BadRequest();
            }

            var esplRoleEntity = Mapper.Map<IdentityRole>(esplRole);

            await AddAppRole(esplRoleEntity);

            // if (!_appRepository.Save())
            // {
            //     throw new Exception("Creating an esplRole failed on save.");
            //     // return StatusCode(500, "A problem happened with handling your request.");
            // }

            var esplRoleToReturn = Mapper.Map<AppRoleDto>(esplRoleEntity);

            var links = CreateLinksForAppRole(esplRoleToReturn.Id, null);

            var linkedResourceToReturn = esplRoleToReturn.ShapeData(null)
                as IDictionary<string, object>;

            linkedResourceToReturn.Add("links", links);

            return CreatedAtRoute("GetAppRole",
                new { id = linkedResourceToReturn["Id"] },
                linkedResourceToReturn);
        }


        [HttpPost("{id}")]
        public IActionResult BlockAppRoleCreation(Guid id)
        {
            if (AppRoleExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            return NotFound();
        }

        [HttpDelete("{id}", Name = "DeleteAppRole")]
        public async Task<IActionResult> DeleteAppRole(Guid id)
        {
            var esplRoleFromRepo = GetAppRole(id);
            if (esplRoleFromRepo == null)
            {
                return NotFound();
            }

            await DeleteAppRole(esplRoleFromRepo);

            // if (!_appRepository.Save())
            // {
            //     throw new Exception($"Deleting esplRole {id} failed on save.");
            // }

            return NoContent();
        }

        [HttpGet("LookUp", Name = "GetAppRoleAsLookUp")]
        public IActionResult GetAppRoleAsLookUp([FromHeader(Name = "Accept")] string mediaType)
        {
            return Ok(GetAppRolesAsLookUp());
        }

        private IEnumerable<LinkDto> CreateLinksForAppRole(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetAppRole", new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link("GetAppRole", new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link("DeleteAppRole", new { id = id }),
              "delete_esplRole",
              "DELETE"));

            links.Add(
              new LinkDto(_urlHelper.Link("CreateBookForAppRole", new { esplRoleId = id }),
              "create_book_for_esplRole",
              "POST"));

            links.Add(
               new LinkDto(_urlHelper.Link("GetBooksForAppRole", new { esplRoleId = id }),
               "books",
               "GET"));

            return links;
        }

        private IEnumerable<LinkDto> CreateLinksForAppRoles(
            AppRolesResourceParameters esplRolesResourceParameters,
            bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            // self 
            links.Add(
               new LinkDto(CreateAppRolesResourceUri(esplRolesResourceParameters,
               ResourceUriType.Current)
               , "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(CreateAppRolesResourceUri(esplRolesResourceParameters,
                  ResourceUriType.NextPage),
                  "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new LinkDto(CreateAppRolesResourceUri(esplRolesResourceParameters,
                    ResourceUriType.PreviousPage),
                    "previousPage", "GET"));
            }

            return links;
        }

        [HttpOptions]
        public IActionResult GetAppRolesOptions()
        {
            Response.Headers.Add("Allow", "GET,OPTIONS,POST");
            return Ok();
        }

        private PagedList<IdentityRole> GetAppRoles(AppRolesResourceParameters esplRoleResourceParameters)
        {
            var collectionBeforePaging =
               _roleMgr.Roles.ApplySort(esplRoleResourceParameters.OrderBy,
                _propertyMappingService.GetPropertyMapping<AppModuleDto, AppModule>());

            if (!string.IsNullOrEmpty(esplRoleResourceParameters.SearchQuery))
            {
                // trim & ignore casing
                var searchQueryForWhereClause = esplRoleResourceParameters.SearchQuery
                    .Trim().ToLowerInvariant();

                collectionBeforePaging = collectionBeforePaging
                    .Where(a => a.Name.ToLowerInvariant().Contains(searchQueryForWhereClause));
            }

            return PagedList<IdentityRole>.Create(collectionBeforePaging,
                esplRoleResourceParameters.PageNumber,
                esplRoleResourceParameters.PageSize == 0 ? collectionBeforePaging.Count() : esplRoleResourceParameters.PageSize);
        }

        private IEnumerable<LookUpItem> GetAppRolesAsLookUp()
        {
            return _roleMgr.Roles.Select(r => new LookUpItem()
            {
                ID = new Guid(r.Id),
                Name = r.Name
            }).ToList();

        }

        private IdentityRole GetAppRole(Guid esplRoleId)
        {
            return _roleMgr.Roles.FirstOrDefault(a => a.Id == esplRoleId.ToString());
        }

        private bool AppRoleExists(Guid esplRoleId)
        {
            return _roleMgr.Roles.Any(a => a.Id == esplRoleId.ToString());
        }

        private async Task DeleteAppRole(IdentityRole esplRole)
        {
            await _roleMgr.DeleteAsync(esplRole);
        }

        private async Task AddAppRole(IdentityRole esplRole)
        {
            await _roleMgr.CreateAsync(esplRole);
        }
    }
}