using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using NG.Application;
using NG.Domain.Users;

namespace NG.Persistence
{
    public class IdentityInitializer
    {
        private RoleManager<IdentityRole> _roleMgr;
        private UserManager<AppUser> _userMgr;

        public IdentityInitializer(UserManager<AppUser> userMgr, RoleManager<IdentityRole> roleMgr)
        {
            _userMgr = userMgr;
            _roleMgr = roleMgr;
        }

        public async Task Seed()
        {
            var user = await _userMgr.FindByNameAsync("tomcruise");

            // Add User
            if (user == null)
            {
                await AddAllAdmins();
                await AddAllCustomers();
                await AddAllViewers();
                await AddAllEditors();
                await AddAllContentCreators();
            }
        }

        public async Task AddAllAdmins()
        {
            List<AppUser> allUsers = new List<AppUser>() {
                new AppUser() {
                    UserName = "tomcruise",
                    Id = "56c385ae-ce46-41d4-b7fe-08df9aef7102",
                    FirstName = "Tom",
                    LastName = "Cruise",
                    Email = "tom.cruise@nock.com"
                },
            };

            if (!(await _roleMgr.RoleExistsAsync("SystemAdmin")))
            {
                var role = new IdentityRole("SystemAdmin");
                role.Claims.Add(new IdentityRoleClaim<string>()
                {
                    ClaimType = "SystemAdmin",
                    ClaimValue = "True"
                });
                //----read
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.DepartmentRead, ClaimValue = "True" });
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.CustomerRead, ClaimValue = "True" });
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.DashboardRead, ClaimValue = "True" });
                //----create
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.DepartmentCreate, ClaimValue = "True" });
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.CustomerCreate, ClaimValue = "True" });
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.DashboardCreate, ClaimValue = "True" });
                //----update
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.DepartmentUpdate, ClaimValue = "True" });
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.CustomerUpdate, ClaimValue = "True" });
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.DashboardUpdate, ClaimValue = "True" });
                //----delete
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.DepartmentDelete, ClaimValue = "True" });
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.CustomerDelete, ClaimValue = "True" });
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.DashboardDelete, ClaimValue = "True" });

                await _roleMgr.CreateAsync(role);
            }

            foreach (var adminUser in allUsers)
            {
                var adminUserResult = await _userMgr.CreateAsync(adminUser, "Espl@123");
                var adminRoleResult = await _userMgr.AddToRoleAsync(adminUser, "SystemAdmin");

                if (!adminUserResult.Succeeded || !adminRoleResult.Succeeded)
                {
                    throw new InvalidOperationException("Failed to build user and roles");
                }
            }

        }

        public async Task AddAllCustomers()
        {
            List<AppUser> allUsers = new List<AppUser>()
            {
                new AppUser()
                {
                    Id = "56c385ae-ce46-41d4-b7fe-08df9aef7401",
                    UserName = "nickjones",
                    FirstName = "Nick",
                    LastName = "Jones",
                    Email = "nick.jones@nock.com"
                },new AppUser()
                {
                    Id = "56c385ae-ce46-41d4-b7fe-08df9aef7402",
                    UserName = "steverogers",
                    FirstName = "Steve",
                    LastName = "Rogers",
                    Email = "steve.rogers@nock.com"
                },new AppUser()
                {
                    Id = "56c385ae-ce46-41d4-b7fe-08df9aef7403",
                    UserName = "tonystark",
                    FirstName = "Tony",
                    LastName = "Stark",
                    Email = "tony.stark@nock.com"
                }
                ,new AppUser()
                {
                    Id = "56c385ae-ce46-41d4-b7fe-08df9aef7404",
                    UserName = "bradpitt",
                    FirstName = "Brad",
                    LastName = "Pitt",
                    Email = "brad.pitt@nock.com"
                },new AppUser()
                {
                    Id = "56c385ae-ce46-41d4-b7fe-08df9aef7405",
                    UserName = "angelinejolie",
                    FirstName = "Angelina",
                    LastName = "Jolie",
                    Email = "angelina.jolie@nock.com"
                }
            };

            if (!(await _roleMgr.RoleExistsAsync("Customer")))
            {
                var role = new IdentityRole("Customer");
                role.Claims.Add(new IdentityRoleClaim<string>()
                {
                    ClaimType = "IsCustomer",
                    ClaimValue = "True"
                });
                //----read
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.CustomerRead, ClaimValue = "True" });
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.DashboardRead, ClaimValue = "True" });
                //----create
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.CustomerCreate, ClaimValue = "True" });
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.DashboardCreate, ClaimValue = "True" });
                //----update
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.CustomerUpdate, ClaimValue = "True" });
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.DashboardUpdate, ClaimValue = "True" });
                //----delete
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.CustomerDelete, ClaimValue = "True" });
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.DashboardDelete, ClaimValue = "True" });

                await _roleMgr.CreateAsync(role);
            }

            await AddUserWithRole(allUsers, "Customer", "Espl@123");

        }

        public async Task AddAllViewers()
        {
            List<AppUser> allUsers = new List<AppUser>()
            {
                new AppUser()
                {
                    Id = "56c385ae-ce46-41d4-b7fe-08df9aef7303",
                    UserName = "johndoe",
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@nock.com"
                }
            };

            if (!(await _roleMgr.RoleExistsAsync("Viewer")))
            {
                var role = new IdentityRole("Viewer");
                role.Claims.Add(new IdentityRoleClaim<string>()
                {
                    ClaimType = "IsViewer",
                    ClaimValue = "True"
                });
                //----read
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.DepartmentRead, ClaimValue = "True" });
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.CustomerRead, ClaimValue = "True" });
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.DashboardRead, ClaimValue = "True" });

                await _roleMgr.CreateAsync(role);
            }

            await AddUserWithRole(allUsers, "Viewer", "Espl@123");

        }

        public async Task AddAllEditors()
        {
            List<AppUser> allUsers = new List<AppUser>()
            {
                new AppUser()
                {
                    Id = "56c385ae-ce46-41d4-b7fe-08df9aef7304",
                    UserName = "johnydepp",
                    FirstName = "Johny",
                    LastName = "Depp",
                    Email = "johny.depp@nock.com"
                }
            };

            if (!(await _roleMgr.RoleExistsAsync("Editor")))
            {
                var role = new IdentityRole("Editor");
                role.Claims.Add(new IdentityRoleClaim<string>()
                {
                    ClaimType = "IsEditor",
                    ClaimValue = "True"
                });
                //----read
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.DepartmentRead, ClaimValue = "True" });
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.CustomerRead, ClaimValue = "True" });
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.DashboardRead, ClaimValue = "True" });
                //----update
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.DepartmentUpdate, ClaimValue = "True" });
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.CustomerUpdate, ClaimValue = "True" });
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.DashboardUpdate, ClaimValue = "True" });


                await _roleMgr.CreateAsync(role);
            }

            await AddUserWithRole(allUsers, "Editor", "Espl@123");

        }

        public async Task AddAllContentCreators()
        {
            List<AppUser> allUsers = new List<AppUser>()
            {
                new AppUser()
                {
                    Id = "56c385ae-ce46-41d4-b7fe-08df9aef7305",
                    UserName = "jacksparrow",
                    FirstName = "Jack",
                    LastName = "Sparrow",
                    Email = "jack.sparrow@nock.com"
                }
            };

            if (!(await _roleMgr.RoleExistsAsync("Creator")))
            {
                var role = new IdentityRole("Creator");
                role.Claims.Add(new IdentityRoleClaim<string>()
                {
                    ClaimType = "IsCreator",
                    ClaimValue = "True"
                });
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.DepartmentCreate, ClaimValue = "True" });
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.CustomerCreate, ClaimValue = "True" });
                role.Claims.Add(new IdentityRoleClaim<string>() { ClaimType = Permissions.DashboardCreate, ClaimValue = "True" });

                await _roleMgr.CreateAsync(role);
            }

            await AddUserWithRole(allUsers, "Creator", "Espl@123");

        }

        public async Task AddUserWithRole(List<AppUser> allUsers, string roleName, string password)
        {


            foreach (AppUser user in allUsers)
            {
                var userResult = await _userMgr.CreateAsync(user, password);
                var roleResult = await _userMgr.AddToRoleAsync(user, roleName);

                if (!userResult.Succeeded || !roleResult.Succeeded)
                {
                    throw new InvalidOperationException("Failed to build user and roles");
                }
            }
        }

    }
}