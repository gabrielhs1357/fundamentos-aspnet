using Microsoft.AspNetCore.Mvc;
using ToDo.Data;
using ToDo.Models;

namespace ToDo.Controllers
{
    [ApiController] // Indica que estamos trabalhando com uma Controller de uma API (JSON)
    // [Route("/")]
    public class HomeController : ControllerBase // Controller class derives from ControllerBase and adds some members that are only needed to support Views
    {
        // Todo método público dentro da Controller é chamado de Action

        [HttpGet("/")]
        public IActionResult Get([FromServices] AppDbContext context)
            => Ok(context.ToDos.ToList());

        [HttpGet("/{id:int}")]
        public IActionResult GetById(
            [FromRoute] int id,
            [FromServices] AppDbContext context
        )
        {
            var todo = context.ToDos.FirstOrDefault(x => x.Id == id);

            if (todo == null)
                return NotFound();

            return Ok(todo);
        }

        [HttpPost("/")]
        public IActionResult Post(
            [FromBody] ToDoModel todo,
            [FromServices] AppDbContext context
        )
        {
            context.ToDos.Add(todo);
            context.SaveChanges();

            return Created($"/{todo.Id}", todo);
        }

        [HttpPut("/{id:int}")]
        public IActionResult Post(
            [FromRoute] int id,
            [FromBody] ToDoModel todo,
            [FromServices] AppDbContext context
        )
        {
            var model = context.ToDos.FirstOrDefault(x => x.Id == id);

            if (model == null) return NotFound();

            model.Title = todo.Title;
            model.Done = todo.Done;

            context.ToDos.Update(model);
            context.SaveChanges();
            return Ok(model);
        }

        [HttpDelete("/{id:int}")]
        public IActionResult Delete(
            [FromRoute] int id,
            [FromServices] AppDbContext context
        )
        {
            var model = context.ToDos.FirstOrDefault(x => x.Id == id);

            if (model == null) return NotFound();

            context.ToDos.Remove(model);
            context.SaveChanges();
            return Ok(model);
        }
    }
}
