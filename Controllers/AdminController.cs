using System.Linq;
using System.Threading.Tasks;
using AlphaMaterials.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AlphaMaterials.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userMgr;
        private readonly RoleManager<IdentityRole> _roleMgr;

        public AdminController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userMgr = userManager;
            _roleMgr = roleManager;
        }

        // GET: /Admin/Users
        public IActionResult Users()
        {
            var model = _userMgr.Users
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    Roles = _userMgr.GetRolesAsync(u).Result
                })
                .ToList();

            return View(model);
        }

        // GET: /Admin/EditRoles/{id}
        public async Task<IActionResult> EditRoles(string id)
        {
            var user = await _userMgr.FindByIdAsync(id);
            if (user == null) return NotFound();

            var vm = new EditRolesViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                Roles = _roleMgr.Roles
                    .Select(r => new RoleCheckbox
                    {
                        RoleName = r.Name,
                        IsSelected = _userMgr.IsInRoleAsync(user, r.Name).Result
                    })
                    .ToList()
            };

            return View(vm);
        }

        // POST: /Admin/EditRoles
        [HttpPost]
        public async Task<IActionResult> EditRoles(EditRolesViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = await _userMgr.FindByIdAsync(vm.UserId);
            if (user == null) return NotFound();

            var currentRoles = await _userMgr.GetRolesAsync(user);
            var selected = vm.Roles.Where(r => r.IsSelected)
                                      .Select(r => r.RoleName);

            await _userMgr.RemoveFromRolesAsync(user, currentRoles);
            await _userMgr.AddToRolesAsync(user, selected);

            return RedirectToAction(nameof(Users));
        }
    }
}
