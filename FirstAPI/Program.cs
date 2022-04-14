var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/", (User user) => Results.Ok(user));

app.Run();

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
}
