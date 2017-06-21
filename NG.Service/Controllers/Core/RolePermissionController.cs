using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using NG.Common;
using NG.Common.Services;
using NG.Domain.Users;
using NG.Persistence;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using NG.Common.DTO;
using NG.Common.Enums;
using NG.Common.Extensions;
using NG.Common.Helpers;
using NG.Domain.Customers;
using NG.Service.Core;

namespace NG.Service.Controllers.Core
{
    [Route("api/roles/{roleId}/permissions")]
    [Authorize(Policy = "IsSuperAdmin")]
    public class RolePermissionController : Controller
    {
        private IGenericRepository<Domain.Core.AppModule> _repo;
        private IUrlHelper _urlHelper;
        private IPropertyMappingService _propertyMappingService;
        private ITypeHelperService _typeHelperService;
        private RoleManager<IdentityRole> _roleMgr;
        private UserManager<AppUser> _userMgr;

        public RolePermissionController(GenericRepository<Domain.Core.AppModule> repo,
            IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService,
            ITypeHelperService typeHelperService,
            UserManager<AppUser> userMgr,
            RoleManager<IdentityRole> roleMgr)
        {
            _repo = repo;
            _urlHelper = urlHelper;
            _propertyMappingService = propertyMappingService;
            _typeHelperService = typeHelperService;
            _userMgr = userMgr;
            _roleMgr = roleMgr;
        }

        [HttpGet(Name = "GetRolePermissions")]
        [HttpHead]
        public async Task<IActionResult> GetRolePermissions(Guid roleId)
        {
            var roleFromDB = GetAppRole(roleId);
            if (roleFromDB == null)
            {
                return NotFound("Role Not Found");
            }

            var permissionsList = new List<string>();
            var rolesFromDB = await _roleMgr.GetClaimsAsync(roleFromDB);
            rolesFromDB.ToList().ForEach(permission =>
            {
                permissionsList.Add(permission.Type.ToString());
            });

            return Ok(permissionsList);
        }


        [HttpPost(Name = "AddPermissionRole")]
        public async Task<IActionResult> AddPermissionRole(Guid roleId,
            [FromBody] AddPermissionToRoleDto model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            var roleFromDB = GetAppRole(roleId);
            if (roleFromDB == null)
            {
                return NotFound("Role Not Found");
            }

            var appModule = AppModuleExists(model.AppModuleName);
            if (!appModule)
            {
                return NotFound("App Module Not Found");
            }
            var claimName = string.Format("{0}.{1}", model.AppModuleName, model.PermissionType.ToString());
            var claimToAdd = new Claim(claimName, "True");
            await _roleMgr.AddClaimAsync(roleFromDB, claimToAdd);

            return Ok();
        }

        [HttpDelete(Name = "RemovePermissionFromRole")]
        public async Task<IActionResult> RemoveUserFromRole(Guid roleId,
            [FromBody] AddPermissionToRoleDto model)
        {
            if (model == null)
            {
                return BadRequest();
            }

            var roleFromDB = GetAppRole(roleId);
            if (roleFromDB == null)
            {
                return NotFound("Role Not Found");
            }

            var appModule = AppModuleExists(model.AppModuleName);
            if (!appModule)
            {
                return NotFound("App Module Not Found");
            }

            var claimName = string.Format("{0}.{1}", model.AppModuleName, model.PermissionType.ToString());
            var claimToAdd = new Claim(claimName, "True");
            await _roleMgr.RemoveClaimAsync(roleFromDB, claimToAdd);
            return Ok();
        }

        private IdentityRole GetAppRole(Guid esplRoleId)
        {
            IdentityRole role = _roleMgr.Roles.FirstOrDefault(a => a.Id == esplRoleId.ToString());
            return role;
        }

        private bool AppModuleExists(string appModuleName)
        {
            var appModule = _repo.FindBy(a => a.ShortName.Equals(appModuleName)).FirstOrDefault();
            return appModule != null ? true : false;
        }
    }
}