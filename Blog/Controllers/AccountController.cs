using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.Services;
using Blog.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;

namespace Blog.Controllers
{
    // [Authorize] => indica que a controller ou a rota requer a autorização em específico
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly BlogDataContext _dataContext;

        public AccountController(TokenService tokenService, BlogDataContext blogDataContext)
        {
            _tokenService = tokenService;
            _dataContext = blogDataContext;
        }

        // [AllowAnonymous] => indica que essa rota pode ser acessada sem autorização (Authorize)
        [HttpPost("v1/accounts/login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

                var user = await _dataContext.Users
                    .AsNoTracking()
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Email == viewModel.Email);

                if (user == null)
                    return BadRequest(new ResultViewModel<string>("E-mail ou senha inválidos"));

                if (!PasswordHasher.Verify(user.PasswordHash, viewModel.Password))
                    return BadRequest(new ResultViewModel<string>("E-mail ou senha inválidos"));

                var token = _tokenService.GenerateToken(user);

                return Ok(new ResultViewModel<string>(token, null));
            }
            catch (Exception)
            {
                return StatusCode(500, new ResultViewModel<User>("Falha interna no servidor ao tentar realizar o login"));
            }
        }

        [HttpPost("v1/accounts")]
        public async Task<IActionResult> CreateAccount([FromBody] RegisterViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

                var user = await _dataContext.Users.FirstOrDefaultAsync(x => x.Email == viewModel.Email);

                if (user != null) return BadRequest(new ResultViewModel<string>("Este e-mail já está cadastrado"));

                user = new User
                {
                    Name = viewModel.Name,
                    Email = viewModel.Email,
                    Slug = viewModel.Email.Replace("@", "-").Replace(".", "-")
                };

                var password = PasswordGenerator.Generate(10, false, false);

                user.PasswordHash = PasswordHasher.Hash(password);

                await _dataContext.Users.AddAsync(user);

                await _dataContext.SaveChangesAsync();

                return Created($"v1/users/{user.Id}", new ResultViewModel<dynamic>(
                    new
                    {
                        email = user.Email,
                        password
                    }));
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new ResultViewModel<User>("Não foi possível criar o usuário"));
            }
            catch (Exception)
            {
                return StatusCode(500, new ResultViewModel<User>("Falha interna no servidor"));
            }

        }
    }
}
