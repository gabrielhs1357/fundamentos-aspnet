using Blog;
using Blog.Data;
using Blog.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using System.IO.Compression;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

ConfigureAuthentication(builder);
ConfigureMvc(builder);
ConfigureServices(builder);

var app = builder.Build();

LoadConfiguration(app);

app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles(); // Assim o servidor consegue renderizar arquivos estáticos
app.MapControllers();
app.UseResponseCompression();
app.Run();

void LoadConfiguration(WebApplication app)
{
    var smtp = new Configuration.SmtpConfiguration();

    app.Configuration.GetSection("Smtp").Bind(smtp); // Atribui os valores daquela section para o objeto smtp

    Configuration.Smtp = smtp;
    //Configuration.JWTKey = app.Configuration.GetValue<string>("JWTKey");
    Configuration.ApiKeyName = app.Configuration.GetValue<string>("ApiKeyName");
    Configuration.APIKey = app.Configuration.GetValue<string>("APIKey");
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
        .AddMemoryCache(); // Adiciona cache na aplicação

    builder.Services.AddResponseCompression(options => // Adiciona compressão na aplicação (resposta são enviadas de forma comprimidas / zipadas)
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
            options.SuppressModelStateInvalidFilter = true; // Desabilita a validação automática das ViewModels
        })
        .AddJsonOptions(options => // Configura a serialização utilizada pelo .NET, evitando erros na rota v1/posts por exemplo ao não utilizar o .Select nela
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
        });
}

void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddDbContext<BlogDataContext>();
    builder.Services.AddTransient<TokenService>();
    builder.Services.AddTransient<EmailService>();
}