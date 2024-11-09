using KoiPool_Project.Models.ViewModels;
using KoiPool_Project.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;

namespace KoiPool_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AppRolesController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AppRolesController(
            UserManager<AppUserModel> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Hiển thị form tạo tài khoản
        [HttpGet]
        public IActionResult Create()
        {
            var model = new CreateUserViewModel
            {
                Roles = _roleManager.Roles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToList()
            };

            return View(model);
        }

        // POST: Xử lý tạo tài khoản và gán vai trò
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra vai trò có tồn tại hay không
                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    // Tạo vai trò nếu chưa tồn tại
                    var roleCreateResult = await _roleManager.CreateAsync(new IdentityRole(model.Role));
                    if (!roleCreateResult.Succeeded)
                    {
                        // Nếu tạo vai trò thất bại, log lỗi và trả về view
                        foreach (var error in roleCreateResult.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }

                        // Trả lại danh sách vai trò để hiển thị trong dropdown
                        model.Roles = _roleManager.Roles.Select(r => new SelectListItem
                        {
                            Value = r.Name,
                            Text = r.Name
                        }).ToList();

                        return View(model);
                    }
                }

                // Tạo người dùng mới
                var newUser = new AppUserModel
                {
                    UserName = model.Username,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(newUser, model.Password);
                if (result.Succeeded)
                {
                    // Gán vai trò cho người dùng
                    var roleResult = await _userManager.AddToRoleAsync(newUser, model.Role);
                    if (!roleResult.Succeeded)
                    {
                        // Nếu gán vai trò thất bại, log lỗi và xóa người dùng
                        await _userManager.DeleteAsync(newUser);

                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }

                        model.Roles = _roleManager.Roles.Select(r => new SelectListItem
                        {
                            Value = r.Name,
                            Text = r.Name
                        }).ToList();

                        return View(model);
                    }

                    TempData["Success"] = "Tạo tài khoản và gán vai trò thành công!";
                    return RedirectToAction("Index");
                }

                // Nếu tạo người dùng thất bại
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            // Trả lại danh sách vai trò nếu lỗi xảy ra
            model.Roles = _roleManager.Roles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name
            }).ToList();

            return View(model);
        }


    }
}
