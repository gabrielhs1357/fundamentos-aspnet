using Blog.Data;
using Blog.Models;
using Blog.ViewModels;
using Blog.ViewModels.Posts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.Controllers
{
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly BlogDataContext _dataContext;

        public PostController(BlogDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpGet("v1/posts")]
        public async Task<IActionResult> GetAsync(
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 25)
        {
            try
            {
                var count = await _dataContext.Posts.CountAsync();

                var posts = await _dataContext
                    .Posts
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Include(p => p.Author)
                    .Select(x => new ListPostsViewModel
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Slug = x.Slug,
                        LastUpdateDate = x.LastUpdateDate,
                        Category = x.Category.Name,
                        Author = $"{x.Author.Name} ({x.Author.Email})"
                    })
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .OrderByDescending(p => p.LastUpdateDate)
                    .ToListAsync();

                // Alternativa para retornar apenas alguns dados sem utilizar ViewModels:
                //var posts = await _dataContext
                //    .Posts
                //    .AsNoTracking()
                //    .Select(x =>
                //    new
                //    {
                //        x.Id,
                //        x.Title
                //    })
                //    .ToListAsync();

                return Ok(new ResultViewModel<dynamic>(new
                {
                    total = count,
                    page,
                    pageSize,
                    posts
                }));
            }
            catch (Exception)
            {
                return StatusCode(500, new ResultViewModel<Post>("Falha interna no servidor ao tentar listar os posts"));
            }
        }

        [HttpGet("v1/posts/{id:int}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] int id)
        {
            try
            {
                var post = await _dataContext
                    .Posts
                    .AsNoTracking()
                    .Include(p => p.Author)
                    .ThenInclude(a => a.Roles)
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (post == null) return NotFound(new ResultViewModel<Post>("Post não encontrado"));

                return Ok(new ResultViewModel<Post>(post));
            }
            catch (Exception)
            {
                return StatusCode(500, new ResultViewModel<Post>("Falha interna no servidor ao tentar listar os posts"));
            }
        }

        [HttpGet("v1/posts/category/{category}")]
        public async Task<IActionResult> GetByCategoryAsync(
            [FromRoute] string category,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 25)
        {
            try
            {
                var count = await _dataContext.Posts.CountAsync();

                var posts = await _dataContext
                    .Posts
                    .AsNoTracking()
                    .Include(p => p.Author)
                    .Include(p => p.Category)
                    .Where(p => p.Category.Slug == category)
                    .Select(x => new ListPostsViewModel
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Slug = x.Slug,
                        LastUpdateDate = x.LastUpdateDate,
                        Category = x.Category.Name,
                        Author = $"{x.Author.Name} ({x.Author.Email})"
                    })
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .OrderByDescending(p => p.LastUpdateDate)
                    .ToListAsync();

                return Ok(new ResultViewModel<dynamic>(new
                {
                    total = count,
                    page,
                    pageSize,
                    posts
                }));
            }
            catch (Exception)
            {
                return StatusCode(500, new ResultViewModel<Post>("Falha interna no servidor ao tentar listar os posts"));
            }
        }
    }
}
