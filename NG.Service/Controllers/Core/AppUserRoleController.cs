using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NG.Common;
using NG.Common.Services;
using NG.Domain.Customers;
using NG.Domain.Users;
using System.Linq;
using System.Linq.Dynamic.Core;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using NG.Application.Core;
using NG.Common.DTO;
using NG.Common.Enums;
using NG.Common.Extensions;
using NG.Common.Helpers;

namespace NG.Service.Controllers.Core
{
    [Route("api/appusers/{userId}/roles")]
    // [Authorize(Policy = "IsSuperAdmin")]
    public class AppUserRoleController : Controller
    {
        private IPropertyMappingService _propertyMappingService;
        private SignInManager<AppUser> _signInMgr;
        private UserManager<AppUser> _userMgr;
        private IPasswordHasher<AppUser> _hasher;
        private IConfigurationRoot _config;
        private RoleManager<IdentityRole> _roleMgr;
        private IGenericRepository<Customer> _repo;

        public AppUserRoleController(
                  SignInManager<AppUser> signInMgr,
                  UserManager<AppUser> userMgr,
                  IPasswordHasher<AppUser> hasher,
                  ILogger<AppUserRoleController> logger,
                  IConfigurationRoot config,
                  RoleManager<IdentityRole> roleMgr,
                  IPropertyMappingService propertyMappingService,
                  IGenericRepository<Customer> repo)
        {
            _signInMgr = signInMgr;
            _userMgr = userMgr;
            _hasher = hasher;
            _config = config;
            _roleMgr = roleMgr;
            _repo = repo;
            _propertyMappingService = propertyMappingService;
        }

        [HttpGet(Name = "GetUserRoles")]
        [HttpHead]
        public async Task<IActionResult> GetUserRoles(Guid userId)
        {
            var userFromDB = GetUserById(Convert.ToString(userId));

            if (userFromDB == null)
            {
                return NotFound("User Not Found");
            }

            var roleList = new List<IdentityRole>();
            var rolesFromDB = await _userMgr.GetRolesAsync(userFromDB);
            return Ok(rolesFromDB);
        }


        [HttpPost(Name = "AddUserToRole")]
        public async Task<IActionResult> AddUserToRole(Guid userId,
            [FromBody] AddUserToRoleDto user)
        {
            if (user == null)
            {
                return BadRequest();
            }

            var userFromDB = GetUserById(Convert.ToString(userId));
            if (userFromDB == null)
            {
                return NotFound("User Not Found");
            }


            var roleFromDB = GetAppRoles(Convert.ToString(user.RoleId));
            if (roleFromDB == null)
            {
                return NotFound("Role Not Found");
            }


            await _userMgr.AddToRoleAsync(userFromDB, roleFromDB.Name);

            // if (!_appRepository.Save())
            // {
            //     throw new Exception($"Creating a book for author {userId} failed on save.");
            // }

            return Ok();
        }

        [HttpDelete(Name = "RemoveUserFromRole")]
        public async Task<IActionResult> RemoveUserFromRole(Guid userId,
            [FromBody] AddUserToRoleDto user)
        {
            if (user == null)
            {
                return BadRequest();
            }

            var userFromDB = GetUserById(Convert.ToString(userId));
            if (userFromDB == null)
            {
                return NotFound("User Not Found");
            }


            var roleFromDB = GetAppRoles(Convert.ToString(user.RoleId));
            if (roleFromDB == null)
            {
                return NotFound("Role Not Found");
            }


            await _userMgr.RemoveFromRoleAsync(userFromDB, roleFromDB.Name);

            // if (!_appRepository.Save())
            // {
            //     throw new Exception($"Removing user from role failed on save.");
            // }

            return Ok();
        }

        private AppUser GetUserById(string userId)
        {
            return _userMgr.Users.FirstOrDefault(a => a.Id == userId);
        }

        private IdentityRole GetAppRoles(string esplRoleId)
        {
            return _roleMgr.Roles.FirstOrDefault(a => a.Id == esplRoleId);
        }

    }
}