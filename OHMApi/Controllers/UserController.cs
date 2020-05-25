﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OHMApi.Data;
using OHMApi.Models;
using OHMDataManager.Library.DataAccess;
using OHMDataManager.Library.Models;

namespace OHMApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(ApplicationDbContext context,
                              UserManager<IdentityUser> userManager,
                              IConfiguration config)
        {
            _context = context;
            _config = config;
            _userManager = userManager;
        }


        [HttpGet]
        public UserModel GetById()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            UserData data = new UserData(_config);

            return data.GetUserById(userId);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("Admin/GetAllUsers")]
        public List<ApplicationUserModel> GetAllUsers()
        {
            List<ApplicationUserModel> output = new List<ApplicationUserModel>();
                     
            var users = _context.Users.ToList();
            var userRoles = from ur in _context.UserRoles
                            join r in _context.Roles on ur.RoleId equals r.Id
                            select new { ur.UserId, ur.RoleId, r.Name };

            foreach (var user in users)
            {
                ApplicationUserModel userM = new ApplicationUserModel
                {
                    Id = user.Id,
                    Email = user.Email
                };

                userM.Roles = userRoles.Where(x => x.UserId == userM.Id).ToDictionary(key => key.RoleId, value => value.Name);

                output.Add(userM);
            }
            
            return output;
        }


        [Authorize(Roles = "Admin")]
        [HttpGet]
        [Route("Admin/GetAllRoles")]
        public Dictionary<string, string> GetAllRoles()
        {
            
            var roles = _context.Roles.ToDictionary(x => x.Id, x => x.Name);

            return roles;
            
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("Admin/AddRole")]
        public async Task AddRole(UserRolePairModel pair)
        {

            var user = await _userManager.FindByIdAsync(pair.UserId);

            await _userManager.AddToRoleAsync(user, pair.RoleName);
            
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("Admin/RemoveRole")]
        public async Task RemoveRole(UserRolePairModel pair)
        {
            var user = await _userManager.FindByIdAsync(pair.UserId);

            await _userManager.RemoveFromRoleAsync(user, pair.RoleName);
            
        }
    }
}