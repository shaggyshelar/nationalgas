using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NG.Common.DTO;
using NG.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace NG.Common.Helpers
{
    public static class Utilities
    {
        public static Expression<Func<TEntity, bool>> BuildLambdaForFindByKey<TEntity>(Guid id)
        {
            var item = Expression.Parameter(typeof(TEntity), "entity");
            var prop = Expression.Property(item, typeof(TEntity).Name + "Id");
            var value = Expression.Constant(id);
            var equal = Expression.Equal(prop, value);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(equal, item);
            return lambda;
        }

        public static string CreateResourceUri(
            BaseResourceParameters resourceParameters,
            ResourceUriType type, IUrlHelper _urlHelper,
            string entityName)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link(string.Format("Get{0}", entityName),
                      new
                      {
                          fields = resourceParameters.Fields,
                          orderBy = resourceParameters.OrderBy,
                          searchQuery = resourceParameters.SearchQuery,
                          pageNumber = resourceParameters.PageNumber - 1,
                          pageSize = resourceParameters.PageSize
                      });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link(string.Format("Get{0}", entityName),
                      new
                      {
                          fields = resourceParameters.Fields,
                          orderBy = resourceParameters.OrderBy,
                          searchQuery = resourceParameters.SearchQuery,
                          pageNumber = resourceParameters.PageNumber + 1,
                          pageSize = resourceParameters.PageSize
                      });
                case ResourceUriType.Current:
                default:
                    return _urlHelper.Link(string.Format("Get{0}", entityName),
                    new
                    {
                        fields = resourceParameters.Fields,
                        orderBy = resourceParameters.OrderBy,
                        searchQuery = resourceParameters.SearchQuery,
                        pageNumber = resourceParameters.PageNumber,
                        pageSize = resourceParameters.PageSize
                    });
            }
        }

        public static IEnumerable<LinkDto> CreateLinks(Guid id, string fields,
            IUrlHelper _urlHelper, string entityName)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                  new LinkDto(_urlHelper.Link(string.Format("Get{0}", entityName), new { id = id }),
                  "self",
                  "GET"));
            }
            else
            {
                links.Add(
                  new LinkDto(_urlHelper.Link(string.Format("Get{0}", entityName), new { id = id, fields = fields }),
                  "self",
                  "GET"));
            }

            links.Add(
              new LinkDto(_urlHelper.Link(string.Format("Delete{0}", entityName), new { id = id }),
              string.Format("delete_{0}", entityName.ToLower()),
              "DELETE"));

            return links;
        }

        public static IEnumerable<LinkDto> CreateLinks(
            BaseResourceParameters departmentsResourceParameters,
            bool hasNext, bool hasPrevious, IUrlHelper _urlHelper, string entityName)
        {
            var links = new List<LinkDto>();

            links.Add(
               new LinkDto(Utilities.CreateResourceUri(departmentsResourceParameters,
               ResourceUriType.Current, _urlHelper, string.Format("Get{0}s", entityName))
               , "self", "GET"));

            if (hasNext)
            {
                links.Add(
                  new LinkDto(Utilities.CreateResourceUri(departmentsResourceParameters,
                  ResourceUriType.NextPage, _urlHelper, string.Format("Get{0}s", entityName)),
                  "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(
                    new LinkDto(Utilities.CreateResourceUri(departmentsResourceParameters,
                    ResourceUriType.PreviousPage, _urlHelper, string.Format("Get{0}s", entityName)),
                    "previousPage", "GET"));
            }

            return links;
        }
    }
}