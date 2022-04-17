using Blog.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true; // Desabilita a validação automática das ViewModels
    });

builder.Services.AddDbContext<BlogDataContext>();

var app = builder.Build();

app.MapControllers();

app.Run();
