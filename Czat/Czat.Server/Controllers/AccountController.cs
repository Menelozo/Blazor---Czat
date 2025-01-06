using Czat.Server.Data;
using Czat.Server.Data.Entities;
using Czat.Server.Helper;
using Czat.Server.Hubs;
using Czat.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Czat.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ChatContext _chatContext;
        private readonly TokenService _tokenService;
        private readonly IHubContext<BlazingChatHub, IBlazingChatHubClient> _hubContext;

        public AccountController(ChatContext chatContext, TokenService tokenService, IHubContext<BlazingChatHub, IBlazingChatHubClient> hubContext)
        {
            _chatContext = chatContext;
            _tokenService = tokenService;
            _hubContext = hubContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto, CancellationToken cancellationToken)
        {
            var usernameExists = await _chatContext.Users
                                        .AsNoTracking()
                                        .AnyAsync(u => u.Username == dto.Username, cancellationToken);

            if (usernameExists)
            {
                return BadRequest($"[{nameof(dto.Username)}] already exists");
            }

            var passwordHasher = new PasswordHasher();
            var salt = passwordHasher.GenerateSalt();
            var hashedPassword = passwordHasher.HashPassword(dto.Password, salt);

            var user = new User
            {
                Username = dto.Username,
                AddedOn = DateTime.Now,
                Name = dto.Name,
                PasswordHash = hashedPassword,
                PasswordSalt = salt,
            };

            await _chatContext.Users.AddAsync(user, cancellationToken);
            await _chatContext.SaveChangesAsync(cancellationToken);

            await _hubContext.Clients.All.UserConnected(new UserDto(user.Id, user.Name));

            return Ok(GenerateToken(user));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto, CancellationToken cancellationToken)
        {
            var user = await _chatContext.Users.FirstOrDefaultAsync(u => u.Username == dto.Username, cancellationToken);

            if (user is null)
            {
                return BadRequest("Incorrect credentials");
            }

            var passwordHasher = new PasswordHasher();
            var hashedPasswordAttempt = passwordHasher.HashPassword(dto.Password, user.PasswordSalt);

            if (!hashedPasswordAttempt.SequenceEqual(user.PasswordHash))
            {
                return BadRequest("Incorrect credentials");
            }

            return Ok(GenerateToken(user));
        }

        private AuthResponseDto GenerateToken(User user)
        {
            var token = _tokenService.GenerateJWT(user);
            return new AuthResponseDto(new UserDto(user.Id, user.Name), token);
        }
    }
}
