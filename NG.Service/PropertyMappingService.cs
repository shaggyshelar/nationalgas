using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using NG.Common.Services;
using NG.Domain.Customers;
using NG.Domain.Departments;
using NG.Domain.Users;
using NG.Service.Controllers.Core;
using NG.Service.Controllers.Customers;
using NG.Service.Controllers.Departments;

namespace NG.Service
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private IList<IPropertyMapping> propertyMappings = new List<IPropertyMapping>();

        private Dictionary<string, PropertyMappingValue> _departmentPropertyMapping =
          new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
          {
               { "DepartmentID", new PropertyMappingValue(new List<string>() { "DepartmentID" } ) },
               { "DepartmentName", new PropertyMappingValue(new List<string>() { "DepartmentName" } )},
               { "DepartmentCode", new PropertyMappingValue(new List<string>() { "DepartmentCode" } )},
               { "DepartmentDespcription", new PropertyMappingValue(new List<string>() { "DepartmentDespcription" } )}
          };

        private Dictionary<string, PropertyMappingValue> _customerPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
        {
            { "CustomerID", new PropertyMappingValue(new List<string>() { "CustomerID" } ) },
            { "NationalID", new PropertyMappingValue(new List<string>() { "NationalID" } ) },
            { "SerialNumber", new PropertyMappingValue(new List<string>() { "SerialNumber" } ) },
            { "Firstname", new PropertyMappingValue(new List<string>() { "Firstname" } ) },
            { "Surname", new PropertyMappingValue(new List<string>() { "Surname" } ) },
            { "Othername", new PropertyMappingValue(new List<string>() { "Othername" } ) },
            { "Mobile", new PropertyMappingValue(new List<string>() { "Mobile" } ) },
            { "Email", new PropertyMappingValue(new List<string>() { "Email" } ) },
            { "Gender", new PropertyMappingValue(new List<string>() { "Gender" } ) },
            { "DateOfBirth", new PropertyMappingValue(new List<string>() { "DateOfBirth" } ) },
            { "Citizenship", new PropertyMappingValue(new List<string>() { "Citizenship" } ) },
            { "Occupation", new PropertyMappingValue(new List<string>() { "Occupation" } ) },
            { "Pin", new PropertyMappingValue(new List<string>() { "Pin" } ) },
            { "Address", new PropertyMappingValue(new List<string>() { "Address" } ) },
            { "Status", new PropertyMappingValue(new List<string>() { "Status" } ) },
            { "DistributorName", new PropertyMappingValue(new List<string>() { "DistributorName" } ) },
            { "DistributorAddress", new PropertyMappingValue(new List<string>() { "DistributorAddress" } ) },
            { "DistributorContact", new PropertyMappingValue(new List<string>() { "DistributorContact" } ) },
        };

        private Dictionary<string, PropertyMappingValue> _esplUserPropertyMapping =
          new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
          {
               { "Id", new PropertyMappingValue(new List<string>() { "Id" } ) },
               { "FirstName", new PropertyMappingValue(new List<string>() { "FirstName" } )},
               { "LastName", new PropertyMappingValue(new List<string>() { "LastName" } )},
               { "Email", new PropertyMappingValue(new List<string>() { "Email" } )},
               { "UserName", new PropertyMappingValue(new List<string>() { "UserName" } )}
          };

        private Dictionary<string, PropertyMappingValue> _esplRolesPropertyMapping =
         new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
         {
               { "Id", new PropertyMappingValue(new List<string>() { "Id" } ) },
               { "Name", new PropertyMappingValue(new List<string>() { "Name" } )}
         };

        private Dictionary<string, PropertyMappingValue> _appModulesPropertyMapping =
        new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
        {
               { "Id", new PropertyMappingValue(new List<string>() { "Id" } ) },
               { "Name", new PropertyMappingValue(new List<string>() { "Name" } )},
               { "MenuText", new PropertyMappingValue(new List<string>() { "MenuText" } )}
        };


        public PropertyMappingService()
        {
            propertyMappings.Add(new PropertyMapping<DepartmentDto, Department>(_departmentPropertyMapping));
            propertyMappings.Add(new PropertyMapping<CustomerDto, Customer>(_customerPropertyMapping));
            propertyMappings.Add(new PropertyMapping<AppUserDto, AppUser>(_esplUserPropertyMapping));
            propertyMappings.Add(new PropertyMapping<AppModuleDto, Domain.Core.AppModule>(_appModulesPropertyMapping));
            propertyMappings.Add(new PropertyMapping<AppRoleDto, IdentityRole>(_esplRolesPropertyMapping));
        }

        public Dictionary<string, PropertyMappingValue> GetPropertyMapping
           <TSource, TDestination>()
        {
            // get matching mapping
            var matchingMapping = propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();

            if (matchingMapping.Count() == 1)
            {
                return matchingMapping.First()._mappingDictionary;
            }

            throw new Exception($"Cannot find exact property mapping instance for <{typeof(TSource)},{typeof(TDestination)}");
        }

        public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                return true;
            }

            // the string is separated by ",", so we split it.
            var fieldsAfterSplit = fields.Split(',');

            // run through the fields clauses
            foreach (var field in fieldsAfterSplit)
            {
                // trim
                var trimmedField = field.Trim();

                // remove everything after the first " " - if the fields 
                // are coming from an orderBy string, this part must be 
                // ignored
                var indexOfFirstSpace = trimmedField.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ?
                    trimmedField : trimmedField.Remove(indexOfFirstSpace);

                // find the matching property
                if (!propertyMapping.ContainsKey(propertyName))
                {
                    return false;
                }
            }
            return true;

        }
    }
}