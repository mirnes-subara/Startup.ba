using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Startupba.Services.Interfaces;
using Startupba.Services.Services;

namespace Startupba.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtTokenService _jwtTokenService;

        public UsersController(IUserService userService, IJwtTokenService jwtTokenService)
        {
            _userService = userService;
            _jwtTokenService = jwtTokenService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<UserResponse>>> Get([FromQuery] UserSearchObject? search = null)
        {
            return await _userService.GetAsync(search ?? new UserSearchObject());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponse>> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
                return NotFound();

            return user;
        }

        [HttpPost]
        [AllowAnonymous]

        public async Task<ActionResult<UserResponse>> Create(UserUpsertRequest request)
        {
            var createdUser = await _userService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserResponse>> Update(int id, UserUpsertRequest request)
        {
            var updatedUser = await _userService.UpdateAsync(id, request);

            if (updatedUser == null)
                return NotFound();

            if (!updatedUser.IsActive)
                await _jwtTokenService.RevokeAllForUserAsync(id);

            return updatedUser;
        }

        /// <summary>
        /// Authenticated user changes their own password. Revokes refresh tokens and issues a new session.
        /// </summary>
        [HttpPut("{id}/change-password")]
        public async Task<ActionResult<LoginResponse>> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out var callerId) || callerId != id)
            {
                return Forbid();
            }

            await _userService.ChangePasswordAsync(id, request);
            await _jwtTokenService.RevokeAllForUserAsync(id);

            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            var login = await _jwtTokenService.IssueLoginResponseAsync(user);
            return Ok(login);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var deleted = await _userService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Admin verifies a user's profile.
        /// </summary>
        [HttpPut("{id}/verify")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<UserResponse>> Verify(int id)
        {
            var user = await _userService.VerifyAsync(id);

            if (user == null)
                return NotFound();

            return user;
        }

        /// <summary>
        /// User requests profile verification (self or admin). Notifies administrators once.
        /// </summary>
        [HttpPut("{id}/request-verification")]
        public async Task<ActionResult<UserResponse>> RequestVerification(int id)
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Administrator");
            if (!isAdmin && (!int.TryParse(claimId, out var callerId) || callerId != id))
            {
                return Forbid();
            }

            var user = await _userService.RequestVerificationAsync(id);

            if (user == null)
                return NotFound();

            return user;
        }

        [HttpPost("authenticate")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Authenticate([FromBody] UserLoginRequest request)
        {
            var user = await _userService.AuthenticateAsync(request);
            if (user == null)
                return Unauthorized();

            var login = await _jwtTokenService.IssueLoginResponseAsync(user);
            return Ok(login);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Refresh([FromBody] RefreshTokenRequest request)
        {
            var login = await _jwtTokenService.RotateRefreshTokenAsync(request.RefreshToken);
            if (login == null)
                return Unauthorized();
            return Ok(login);
        }

        /// <summary>
        /// Revokes the supplied refresh token, or all refresh tokens for the caller if none is provided.
        /// </summary>
        [HttpPost("logout")]
        public async Task<ActionResult> Logout([FromBody] RefreshTokenRequest? request)
        {
            if (!string.IsNullOrWhiteSpace(request?.RefreshToken))
            {
                await _jwtTokenService.RevokeRefreshTokenAsync(request.RefreshToken);
                return NoContent();
            }

            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out var callerId))
                return Unauthorized();

            await _jwtTokenService.RevokeAllForUserAsync(callerId);
            return NoContent();
        }
    }
}
