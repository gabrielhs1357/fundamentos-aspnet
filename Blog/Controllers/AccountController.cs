using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.Services;
using Blog.ViewModels;
using Blog.ViewModels.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;
using System.Text.RegularExpressions;

namespace Blog.Controllers
{
    // [Authorize] => indica que a controller ou a rota requer a autorização em específico
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly BlogDataContext _dataContext;
        private readonly EmailService _emailService;

        public AccountController(
            TokenService tokenService,
            BlogDataContext blogDataContext,
            EmailService emailService)
        {
            _tokenService = tokenService;
            _dataContext = blogDataContext;
            _emailService = emailService;
        }

        [AllowAnonymous] // => indica que essa rota pode ser acessada sem autorização (Authorize)
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

        [AllowAnonymous]
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

                _emailService.Send(
                    user.Name,
                    user.Email,
                    "Bem vindo ao blog!",
                    $"Sua senha é <strong>{password}</strong>");

                return Created($"v1/users/{user.Id}", new ResultViewModel<dynamic>(
                    new
                    {
                        email = user.Email,
                        //password
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

        [Authorize]
        [HttpPost("v1/accounts/upload-image")]
        public async Task<IActionResult> UploadImage([FromBody] UploadImageViewModel viewModel)
        {
            var fileName = $"{Guid.NewGuid()}.jpg";
            // As vezes a base 64 vem essas informações a mais no começo, então precisamos remove-las:
            var data = new Regex(@"^data:image\/[a-z]+;base64,").Replace(viewModel.Base64Image, "");
            var bytes = Convert.FromBase64String(data);

            try
            {
                // Tudo nessa pasta fica visível na web, então podemos ver a imagem apenas acessando essa URL final:
                await System.IO.File.WriteAllBytesAsync($"wwwroot/images/{fileName}", bytes);
            }
            catch (Exception)
            {
                return StatusCode(500, new ResultViewModel<User>("Falha interna no servidor"));
            }

            var user = await _dataContext
                .Users
                .FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

            if (user == null) return NotFound(new ResultViewModel<User>("Usuário não encontrado"));

            user.Image = $"https://localhost:0000/images/{fileName}";

            try
            {
                _dataContext.Users.Update(user);

                await _dataContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(500, new ResultViewModel<User>("Falha interna no servidor"));
            }

            return Ok(new ResultViewModel<string>("Imagem alterada com sucesso", null));
        }
    }
}
