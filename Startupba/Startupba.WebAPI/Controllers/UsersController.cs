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

        public UsersController(IUserService userService)
        {
            _userService = userService;
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

            return updatedUser;
        }

        /// <summary>
        /// Authenticated user changes their own password. Admins cannot change other users' passwords.
        /// </summary>
        [HttpPut("{id}/change-password")]
        public async Task<ActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out var callerId) || callerId != id)
            {
                return Forbid();
            }

            await _userService.ChangePasswordAsync(id, request);
            return NoContent();
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
        public async Task<ActionResult<UserResponse>> Authenticate([FromBody] UserLoginRequest request)
        {
            var user = await _userService.AuthenticateAsync(request);
            if (user == null)
                return Unauthorized();
            return Ok(user);
        }
    }
}