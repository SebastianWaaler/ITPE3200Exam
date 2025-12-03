using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizApp.Models;

namespace QuizApp.Controllers
{
    /// <summary>
    /// API controller for getting current user information for client-side authentication state.
    /// Used by the SPA to determine if user is logged in and their role.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserInfoController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserInfoController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Returns information about the current user (if logged in).
        /// Used by the SPA to display user name and determine admin status.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Ok(new { isAuthenticated = false });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Ok(new { isAuthenticated = false });
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            return Ok(new
            {
                isAuthenticated = true,
                userName = user.UserName,
                email = user.Email,
                isAdmin = isAdmin
            });
        }
    }
}

