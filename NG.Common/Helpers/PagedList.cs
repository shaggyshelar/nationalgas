using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NG.Common.Enums;

namespace NG.Common.Helpers
{
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }

        public bool HasPrevious
        {
            get
            {
                return (CurrentPage > 1);
            }
        }

        public bool HasNext
        {
            get
            {
                return (CurrentPage < TotalPages);
            }
        }

        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            AddRange(items);
        }

        public static PagedList<T> Create(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            if (pageSize > 0 && pageNumber > 0)
            {
                var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                return new PagedList<T>(items, count, pageNumber, pageSize);
            }
            return new PagedList<T>(new List<T>(), count, pageNumber, pageSize);
        }

        public object GetHateosMetadata()
        {
            return new
            {
                totalCount = this.TotalCount,
                pageSize = this.PageSize,
                currentPage = this.CurrentPage,
                totalPages = this.TotalPages,
            };
        }

        public object GetMetadata(BaseResourceParameters resourceParameter, IUrlHelper _urlHelper)
        {
            var previousPageLink = this.HasPrevious ?
                    Utilities.CreateResourceUri(resourceParameter,
                    ResourceUriType.PreviousPage, _urlHelper, "Departments") : null;

            var nextPageLink = this.HasNext ?
                Utilities.CreateResourceUri(resourceParameter,
                ResourceUriType.NextPage, _urlHelper, "Departments") : null;

            return new
            {
                previousPageLink = previousPageLink,
                nextPageLink = nextPageLink,
                totalCount = this.TotalCount,
                pageSize = this.PageSize,
                currentPage = this.CurrentPage,
                totalPages = this.TotalPages
            };
        }
    }
}