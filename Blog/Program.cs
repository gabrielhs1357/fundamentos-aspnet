using Blog;
using Blog.Data;
using Blog.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using System.IO.Compression;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

LoadConfiguration(builder); // É possível obter configurações tanto do app.Configuration quanto do builder.Configuration, mais detalhes em https://github.com/balta-io/2811/discussions/17
ConfigureAuthentication(builder);
ConfigureMvc(builder);
ConfigureServices(builder);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHttpsRedirection(); // Redireciona requisições http para https automaticamente
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles(); // Assim o servidor consegue renderizar arquivos est�ticos
app.MapControllers();
app.UseResponseCompression();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();

void LoadConfiguration(WebApplicationBuilder builder)
{
    var smtp = new Configuration.SmtpConfiguration();

    builder.Configuration.GetSection("Smtp").Bind(smtp); // Atribui os valores daquela section para o objeto smtp

    Configuration.Smtp = smtp;
    Configuration.JWTKey = builder.Configuration.GetValue<string>("JWTKey");
    Configuration.ApiKeyName = builder.Configuration.GetValue<string>("ApiKeyName");
    Configuration.APIKey = builder.Configuration.GetValue<string>("APIKey");
}

void ConfigureAuthentication(WebApplicationBuilder builder)
{
    var key = Encoding.ASCII.GetBytes(Configuration.JWTKey);

    builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(x =>
    {
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
}

void ConfigureMvc(WebApplicationBuilder builder)
{
    builder
        .Services
        .AddMemoryCache(); // Adiciona cache na aplica��o

    builder.Services.AddResponseCompression(options => // Adiciona compress�o na aplica��o (resposta s�o enviadas de forma comprimidas / zipadas)
    {
        // options.Providers.Add<BrotliCompressionProvider>();
        options.Providers.Add<GzipCompressionProvider>();
        // options.Providers.Add<CustomCompressionProvider>();
    });

    builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.SmallestSize;
    });

    builder
        .Services
        .AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.SuppressModelStateInvalidFilter = true; // Desabilita a valida��o autom�tica das ViewModels
        })
        .AddJsonOptions(options => // Configura a serializa��o utilizada pelo .NET, evitando erros na rota v1/posts por exemplo ao n�o utilizar o .Select nela
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
        });
}

void ConfigureServices(WebApplicationBuilder builder)
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<BlogDataContext>(options => options.UseSqlServer(connectionString));
    builder.Services.AddTransient<TokenService>();
    builder.Services.AddTransient<EmailService>();
}
