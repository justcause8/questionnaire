using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using questionnaire.Authentication;
using questionnaire.Models;
using questionnaire.Services;

var builder = WebApplication.CreateBuilder(args);

// Добавляем контроллеры
builder.Services.AddControllers();

// Настроим логирование для отладки
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Добавляем Swagger (документация API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Введите токен в формате: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Настроим аутентификацию JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = AuthOptions.ISSUER,
            ValidAudience = AuthOptions.AUDIENCE,
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
        };
    });

// Добавляем авторизацию
builder.Services.AddAuthorization();

// Настройка подключения к БД
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
/*if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Строка подключения 'DefaultConnection' не найдена.");
}*/

builder.Services.AddDbContext<QuestionnaireContext>(options =>
    options.UseSqlServer(connectionString));

// Регистрация сервиса для работы с токенами
builder.Services.AddSingleton<TokenService>(provider =>
{
    return new TokenService(
        jwtKey: AuthOptions.KEY,
        issuer: AuthOptions.ISSUER,
        audience: AuthOptions.AUDIENCE,
        lifetimeMinutes: AuthOptions.LIFETIME
    );
});

// Добавляем CORS-политику
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Нужно, если фронт отправляет куки или токен
    });
});

// Создаём приложение
var app = builder.Build();

// Middleware (обработчики запросов)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Порядок middleware важен! CORS должен быть ДО аутентификации
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthentication(); // Включаем проверку токенов
app.UseAuthorization();  // Включаем авторизацию

// Логируем ошибки (полезно при отладке 500-й ошибки)
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError("Ошибка сервера: {0}", context.Response.StatusCode);
    });
});

app.MapControllers();

// Настроим доступные порты
app.Urls.Add("http://localhost:5000");  // HTTP
app.Urls.Add("https://localhost:7109"); // HTTPS

// Запускаем сервер
app.Run();
