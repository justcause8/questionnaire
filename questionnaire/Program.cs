using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using questionnaire.Models;
using questionnaire.Authentication;
using questionnaire.Services;

var builder = WebApplication.CreateBuilder(args);

// Добавляем контроллеры
builder.Services.AddControllers();

// Добавляем Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Настройка аутентификации JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = AuthOptions.ISSUER,
        ValidAudience = AuthOptions.AUDIENCE,
        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
        ValidateIssuerSigningKey = true,
    };
});

// Добавляем авторизацию
builder.Services.AddAuthorization();

// Настройка DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Строка подключения 'DefaultConnection' не найдена.");
}

builder.Services.AddDbContext<QuestionnaireContext>(options =>
    options.UseSqlServer(connectionString));

// Регистрация сервисов
builder.Services.AddSingleton<TokenService>(provider =>
{
    return new TokenService(
        jwtKey: AuthOptions.KEY,
        issuer: AuthOptions.ISSUER,
        audience: AuthOptions.AUDIENCE,
        lifetimeMinutes: AuthOptions.LIFETIME
    );
});

// Создаем приложение
var app = builder.Build();

// Настройка middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication(); // Аутентификация должна быть перед авторизацией
app.UseAuthorization();
app.MapControllers();

// Запускаем приложение
app.Run();



//using Microsoft.EntityFrameworkCore;
//using System.Globalization;

//namespace questionnaire.Models
//{
//    public class Program
//    {
//        public static void Main(string[] args)
//        {
//            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
//            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

//            var builder = WebApplication.CreateBuilder(args);

//            builder.Services.AddControllers();
//            builder.Services.AddEndpointsApiExplorer();
//            builder.Services.AddSwaggerGen();

//            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//            IServiceCollection serviceCollection = builder.Services.AddDbContext<QuestionnaireContext>(options => options.UseSqlServer(connectionString));

//            //builder.Services.AddDbContext<questionnaireContext>();

//            var app = builder.Build();

//            if (app.Environment.IsDevelopment())
//            {
//                app.UseSwagger();
//                app.UseSwaggerUI();
//            }

//            app.UseHttpsRedirection();

//            app.UseAuthorization();

//            app.MapControllers();

//            app.Run();
//        }
//    }
//}
