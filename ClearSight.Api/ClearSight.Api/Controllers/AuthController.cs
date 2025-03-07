using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ClearSight.Infrastructure.Implementations.Services;
using ClearSight.Core.Mosels;
using ClearSight.Infrastructure.Context;
using ClearSight.Core.Dtos.AuthenticationDtos;
using ClearSight.Core.Constant;
using Microsoft.EntityFrameworkCore;
using ClearSight.Core.Dtos.ApiResponse;
using Serilog;

namespace ClearSight.Api.Controllers
{
    /// <summary>
    /// Authentication Controller For Login , Register ,Reset Password and more
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly AuthenticationService _authenticationService;
        private readonly MailingService _emailSender;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly AppDbContext _context;
        private readonly ActivateUserAccountsServices _activateUserAccounts;
        private readonly GenerateCodeServices _generateCodeServices;

        public AuthController(AuthenticationService AuthenticationService, MailingService emailSender, UserManager<User> userManager, SignInManager<User> signInManager, AppDbContext appDbContext, ActivateUserAccountsServices activateUserAccounts, GenerateCodeServices generateCodeServices)
        {
            _authenticationService = AuthenticationService;
            _emailSender = emailSender;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = appDbContext;
            _activateUserAccounts = activateUserAccounts;
            _generateCodeServices = generateCodeServices;
        }
        /// <summary>
        /// Generate a new userName.
        /// </summary>
        /// <param name="userName">UserName.</param>
        /// <returns>Returns unused UserName.</returns>
        /// <response code="200">UserName</response>
        [ProducesResponseType(200)]
        [HttpPost("generateUserName")]
        public IActionResult GenerateUserName(string userName)
        {
            string name = userName.Trim().Replace(" ", "");
            bool IsUsedName = _userManager.Users.Any(u => u.UserName == name);
            while (IsUsedName)
            {
                var randNum = new Random().Next(100_000, 999_999);
                name = name + "_" + randNum;
                IsUsedName = _userManager.Users.Any(x => x.UserName == name);
            }
            return Ok(new {UserName = name});
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="model">User registration details.</param>
        /// <returns>Returns success message or validation errors.</returns>
        /// <response code="200">User registered successfully</response>
        /// <response code="400">Validation error</response>
        /// <response code="400">User NotFound error</response>
        /// <response code="500">Internal server error</response>
        [ProducesResponseType(typeof(ApiSuccessResponse), 200)]
        [ProducesResponseType(typeof(ModelStateErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ServerErrorResponse), 500)]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterModel model)
        {
            var registrationModel = await _authenticationService.RegisterUserAsync(model);

            if (registrationModel.Message != null)
                return BadRequest(new ApiErrorResponse{ err_message = registrationModel.Message });

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return BadRequest(new ApiErrorResponse { err_message = "Email Not Found" });

            var token = await _authenticationService.GenerateEmailConfirmationTokenAsync(user.Id);

            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Auth", new { userId = user.Id, token }, Request.Scheme);

            var str = new StreamReader(FilePaths.EmailConfirmation);
            var mailText = str.ReadToEnd();
            str.Close();

            mailText = mailText.Replace("RESET_LINK", confirmationLink);
            mailText = mailText.Replace("PATIENT_NAME", user.FullName);

            try
            {
                await _emailSender.SendEmailAsync(model.Email, "Confirm your email", mailText);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Login Error");
                await _userManager.DeleteAsync(user);
                return BadRequest(new ServerErrorResponse { StatusCode =500, err_message = ex.Message });
            }
            return Ok(new ApiSuccessResponse { result = "Registration Email has send successful Check Your Mail To Confirm Account." });
        }

        /// <summary>
        /// Login to system.
        /// </summary>
        /// <param name="model">User Login details.</param>
        /// <returns>Returns success message or validation errors.</returns>
        /// <response code="200">User logined successfully</response>
        /// <response code="400">Validation error</response>
        /// <response code="400">User NotFound error</response>
        /// <response code="401">UnAuthorized User error</response>
        [ProducesResponseType(typeof(AuthenticationModel), 200)]
        [ProducesResponseType(typeof(ModelStateErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 401)]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return BadRequest(new ApiErrorResponse { err_message = "Email Not Registerd" });

            if (await _userManager.IsLockedOutAsync(user))
                return BadRequest(new ApiErrorResponse { err_message = "Your account is locked. Please try again later." });


            if (!user.EmailConfirmed)
                return BadRequest(new ApiErrorResponse { err_message = "PLZ Confirm Your Mail First" });

            await _activateUserAccounts.ActivateUserAccount(user);

            var result = await _authenticationService.LoginUserAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(new ApiErrorResponse { err_message = result.ErrorMessage });

            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                var cookieOptions = _authenticationService.SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);
                Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);
            }

            return Ok(result);
        }

        /// <summary>
        /// Change User Password.
        /// </summary>
        /// <param name="model">change password details.</param>
        /// <returns>Returns success message or validation errors.</returns>
        /// <response code="200">Change Password successfully</response>
        /// <response code="400">Validation error</response>
        /// <response code="404">User not found.</response>
        [ProducesResponseType(typeof(ApiSuccessResponse), 200)]
        [ProducesResponseType(typeof(ModelStateErrorResponse), 400)]
        [ProducesResponseType(typeof(IEnumerable<IdentityError>), 404)]
        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new ApiErrorResponse { err_message = "User not found." });
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {

                user.RefreshTokens = null;
                await _userManager.UpdateAsync(user);

                return Ok(new ApiSuccessResponse { result = "Password has been Change successfully." });
            }

            return BadRequest(result.Errors);
        }


        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
                return BadRequest("Invalid email confirmation request.");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return BadRequest("No User Found");

            if (user.EmailConfirmed)
            {
                var str = new StreamReader(FilePaths.EmailConfirmatedBefore);
                var mailText = str.ReadToEnd();
                str.Close();
                return Content(mailText, "text/html");
            }

            var result = await _authenticationService.ConfirmEmailAsync(userId, token);

            if (result)
            {
                var str = new StreamReader(FilePaths.EmailConfirmationSuccess);
                var mailText = str.ReadToEnd();
                str.Close();
                return Content(mailText, "text/html");

            }
            return BadRequest("Error confirming your email.");
        }
        /// <summary>
        /// Get Change Password Code.
        /// </summary>
        /// <param name="email">User Mail To Get Verivication Code.</param>
        /// <returns>Returns success message or validation errors.</returns>
        /// <response code="200">Code Send To Your Mail successfully</response>
        /// <response code="404">Email not found error</response>
        [ProducesResponseType(typeof(ApiSuccessResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [HttpGet("GetCode")]
        public async Task<IActionResult> GetCode(string email)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == email);
            if (user == null)
                return NotFound(new ApiErrorResponse { err_message = "Email not found." });

            var code = Guid.NewGuid().ToString()[..5].ToUpper();
            string hexCode = _generateCodeServices.GenerateCode(code);

            var userCode = new UserCode
            {
                Code = hexCode,
                ExpirationTime = DateTime.UtcNow.AddMinutes(15),
                User = user,
            };
            _context.Add(userCode);
            _context.SaveChanges();

            var str = new StreamReader(FilePaths.ResetPasswordCode);
            var mailText = str.ReadToEnd();
            str.Close();

            mailText = mailText.Replace("USER_NAME", user.FullName);
            mailText = mailText.Replace("{CODE}", code);

            await _emailSender.SendEmailAsync(user.Email, "Code", mailText);
            return Ok(new ApiSuccessResponse{ result = "Verification code sent to your email." });
        }
        /// <summary>
        /// Reset User Password.
        /// </summary>
        /// <param name="resetPasswordDto">Reset password details.</param>
        /// <returns>Returns success message or validation errors.</returns>
        /// <response code="200">Reset Password successfully</response>
        /// <response code="400">Invalid or expired verification code.</response>
        /// <response code="404">User not found.</response>
        [ProducesResponseType(typeof(ApiSuccessResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse), 404)]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            string hashedCode = _generateCodeServices.GenerateCode(resetPasswordDto.VerificationCode);

            var resetRequest = await _context.UserCodes
                .FirstOrDefaultAsync(r =>
                    r.User.Email == resetPasswordDto.Email &&
                    r.Code == hashedCode &&
                    r.ExpirationTime >= DateTime.UtcNow &&
                   !r.IsUsed);

            if (resetRequest == null)
                return BadRequest(new ApiErrorResponse { err_message = "Invalid or expired verification code." });

            // Update the user's password
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == resetPasswordDto.Email);
            if (user == null)
                return NotFound(new ApiErrorResponse { err_message = "User not found." });


            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, resetPasswordDto.NewPassword);
            resetRequest.IsUsed = true;

            await _userManager.UpdateSecurityStampAsync(user);
            _context.Users.Update(user);
            _context.UserCodes.Update(resetRequest);
            await _context.SaveChangesAsync();

            return Ok(new ApiSuccessResponse{ result = "Password reset successful." });
        }

        /// <summary>
        /// Call this api to get new token if your refresh token still valid
        /// </summary>
        /// <returns>Returns  refresh token or validation errors.</returns>
        /// <response code="200">Returns refresh token successfully</response>
        /// <response code="400">Invalid refresh token login again</response>
        [ProducesResponseType(typeof(AuthenticationModel), 200)]
        [ProducesResponseType(typeof(AuthenticationModel), 404)]
        [HttpGet("refreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            var result = await _authenticationService.RefreshTokenAsync(refreshToken);

            if (!result.IsAuthenticated)
                return BadRequest(result);

            var cookieOptions = _authenticationService.SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);
            Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);
            
            return Ok(result);
        }
        /// <summary>
        /// Call this api to revoke your refresh token 
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Returns  success or validation errors.</returns>
        /// <response code="200">Token Revoked Successfully</response>
        /// <response code="400">Invalid refresh token login again</response>
        [ProducesResponseType(typeof(ApiSuccessResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [HttpPost("revokeToken")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeToken model)
        {
            var token = model.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest(new ApiErrorResponse
                {
                    StatusCode =StatusCodes.Status400BadRequest,
                    err_message = "Token is required!"
                });

            var result = await _authenticationService.RevokeTokenAsync(token);

            if (!result)
                return BadRequest(new ApiErrorResponse
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    err_message = "Token is invalid!"
                });

            return Ok(new ApiSuccessResponse
            {
                StatusCode =StatusCodes.Status200OK,
                result ="Token Revoked Successfully"
            });
        }

        
    }
}
