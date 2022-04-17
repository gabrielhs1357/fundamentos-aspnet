using Blog.Data;
using Blog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Blog.ViewModels;
using Blog.Extensions;

namespace Blog.Controllers
{
    [ApiController]
    public class CategoryController : ControllerBase
    {
        [HttpGet("v1/categories")]
        public async Task<IActionResult> GetAllAsync([FromServices] BlogDataContext context)
        {
            try
            {
                var categories = await context.Categories.ToListAsync();
                return Ok(new ResultViewModel<List<Category>>(categories));
            }
            catch (Exception)
            {
                return StatusCode(500, new ResultViewModel<Category>("Falha interna no servidor"));
            }
        }

        [HttpGet("v1/categories/{id:int}")]
        public async Task<IActionResult> GetByIdAsync(
            [FromRoute] int id,
            [FromServices] BlogDataContext context)
        {
            try
            {
                var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);

                if (category == null) return NotFound(new ResultViewModel<Category>("Categoria não encontrada"));

                return Ok(new ResultViewModel<Category>(category));
            }
            catch (Exception)
            {
                return StatusCode(500, new ResultViewModel<Category>("Falha interna no servidor"));
            }
        }

        [HttpPost("v1/categories")]
        public async Task<IActionResult> PostAsync(
            [FromBody] EditorCategoryViewModel viewModel,
            [FromServices] BlogDataContext context)
        {
            try
            {
                // Valida se a ViewModel contém todas as validações especificadas na classe
                // Apenas necessário quando usar a opcão SuppressModelStateInvalidFilter
                if (!ModelState.IsValid) return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));

                var category = new Category()
                {
                    Name = viewModel.Name,
                    Slug = viewModel.Slug.ToLower(),
                };

                await context.Categories.AddAsync(category);
                await context.SaveChangesAsync();

                return Created($"v1/categories/{category.Id}", new ResultViewModel<Category>(category));
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new ResultViewModel<Category>("Não foi possível criar a categoria"));
            }
            catch (Exception)
            {
                return StatusCode(500, new ResultViewModel<Category>("Falha interna no servidor"));
            }
        }

        [HttpPut("v1/categories/{id:int}")]
        public async Task<IActionResult> PutAsync(
            [FromRoute] int id,
            [FromBody] EditorCategoryViewModel viewModel,
            [FromServices] BlogDataContext context)
        {
            try
            {
                var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);

                if (category == null) return NotFound(new ResultViewModel<Category>("Categoria não encontrada"));

                category.Name = viewModel.Name;
                category.Slug = viewModel.Slug;

                context.Categories.Update(category);

                await context.SaveChangesAsync();

                return Ok(new ResultViewModel<Category>(category));
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new ResultViewModel<Category>("Não foi possível atualizar a categoria"));
            }
            catch (Exception)
            {
                return StatusCode(500, new ResultViewModel<Category>("Falha interna no servidor"));
            }
        }

        [HttpDelete("v1/categories/{id:int}")]
        public async Task<IActionResult> DeleteAsync(
            [FromRoute] int id,
            [FromServices] BlogDataContext context)
        {
            try
            {
                var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);

                if (category == null) return NotFound(new ResultViewModel<Category>("Categoria não encontrada"));

                context.Categories.Remove(category);

                await context.SaveChangesAsync();

                return Ok(new ResultViewModel<Category>(category));
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new ResultViewModel<Category>("Não foi possível remover a categoria"));
            }
            catch (Exception)
            {
                return StatusCode(500, new ResultViewModel<Category>("Falha interna no servidor"));
            }
        }
    }
}
