using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NG.Common;
using NG.Common.Filters;
using NG.Domain.Customers;
using NG.Domain.Users;
using NG.Service.Core;

namespace NG.Service.Controllers.Core
{
    public class AuthController : Controller
    {
        private SignInManager<AppUser> _signInMgr;
        private UserManager<AppUser> _userMgr;
        private IPasswordHasher<AppUser> _hasher;
        private IConfigurationRoot _config;
        private RoleManager<IdentityRole> _roleMgr;
        private IGenericRepository<Customer> _repo;
        public AuthController(
                  SignInManager<AppUser> signInMgr,
                  UserManager<AppUser> userMgr,
                  IPasswordHasher<AppUser> hasher,
                  ILogger<AuthController> logger,
                  IConfigurationRoot config,
                  RoleManager<IdentityRole> roleMgr,
                  IGenericRepository<Customer> repo)
        {
            _signInMgr = signInMgr;
            _userMgr = userMgr;
            _hasher = hasher;
            _config = config;
            _roleMgr = roleMgr;
            _repo = repo;
        }

        [ValidateModel]
        [HttpPost("api/auth/token")]
        public async Task<IActionResult> CreateToken([FromBody] CredentialModel model)
        {
            if (model == null)
            {
                return BadRequest("Invalid Username or Password.");
            }

            var user = await _userMgr.FindByNameAsync(model.UserName);
            if (user != null)
            {
                if (_hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Success)
                {
                    var userClaims = await _userMgr.GetClaimsAsync(user);
                    var userRoles = await _userMgr.GetRolesAsync(user);
                    var itemList = userRoles.ToList();
                    var toSendClaims = new List<Claim>();
                    for (int i = 0; i < itemList.Count; i++)
                    {
                        var roleFromDb = await _roleMgr.FindByNameAsync(itemList.ElementAt(i));
                        var roleClaims = await _roleMgr.GetClaimsAsync(roleFromDb);
                        toSendClaims.AddRange(roleClaims);
                    }

                    toSendClaims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.UserName));
                    toSendClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                    toSendClaims.Add(new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName));
                    toSendClaims.Add(new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName));
                    toSendClaims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
                    toSendClaims.Add(new Claim("UserId", user.Id));
                    // var employee = _repo.FindByKey(new Guid(user.Id));
                    Customer customer = _repo.FindBy(u => u.UserID.Equals(user.Id)).FirstOrDefault();
                    if (customer != null)
                        toSendClaims.Add(new Claim("CustomerID", Convert.ToString(customer.CustomerID)));

                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    var token = new JwtSecurityToken(
                      issuer: _config["Tokens:Issuer"],
                      audience: _config["Tokens:Audience"],
                      claims: toSendClaims,
                      expires: DateTime.UtcNow.AddYears(1),
                      signingCredentials: creds
                      );

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo
                    });
                }
            }

            return BadRequest("Invalid Username or Password.");
        }

        [Authorize(Policy = "Auth.CanCreate")]
        [HttpPost("api/auth/CanCreate")]
        public IActionResult CanCreate()
        {
            return Ok("You Can Create");
        }

        [Authorize(Policy = "Auth.CanRead")]
        [HttpPost("api/auth/CanRead")]

        public IActionResult CanRead()
        {
            return Ok("You Can Read");
        }

        [Authorize(Policy = "Auth.CanUpdate")]
        [HttpPost("api/auth/CanUpdate")]
        public IActionResult CanUpdate()
        {
            return Ok("You Can Update");
        }


        [Authorize(Policy = "Auth.CanDelete")]
        [HttpPost("api/auth/CanDelete")]
        public IActionResult CanDelete()
        {
            return Ok("You Can Delete");
        }
    }
}