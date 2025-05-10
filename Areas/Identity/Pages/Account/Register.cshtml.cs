using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace AlphaMaterials.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required, EmailAddress]
            [Display(Name = "����������� �����")]
            public string Email { get; set; }

            [Required, StringLength(100, MinimumLength = 6, ErrorMessage = "{0} ������ ���� �� {2} �� {1} ��������.")]
            [DataType(DataType.Password)]
            [Display(Name = "������")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "������������� ������")]
            [Compare("Password", ErrorMessage = "������ �� ���������.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
            var result = await _userManager.CreateAsync(user, Input.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("����� ������������ ���������������.");

                // ������ ���� User, ���� � ��� ���
                if (!await _roleManager.RoleExistsAsync("User"))
                    await _roleManager.CreateAsync(new IdentityRole("User"));

                await _userManager.AddToRoleAsync(user, "User");

                // ������������� �������
                await _signInManager.SignInAsync(user, isPersistent: false);

                // �������� �� ������
                TempData["RegisterSuccess"] = $"����������� ������ �������, {user.Email}!";

                // ��������� �� GET ���� ��������, ����� �������� ���������
                return RedirectToPage();
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return Page();
        }
    }
}
