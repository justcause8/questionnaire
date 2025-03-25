using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using questionnaire.Authentication;
using questionnaire.Models;
using questionnaire.Services;

var builder = WebApplication.CreateBuilder(args);

// ��������� �����������
builder.Services.AddControllers();

// �������� ����������� ��� �������
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ��������� Swagger (������������ API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "������� ����� � �������: Bearer {token}",
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

// �������� �������������� JWT
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

// ��������� �����������
builder.Services.AddAuthorization();

// ��������� ����������� � ��
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
/*if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("������ ����������� 'DefaultConnection' �� �������.");
}*/

builder.Services.AddDbContext<QuestionnaireContext>(options =>
    options.UseSqlServer(connectionString));

// ����������� ������� ��� ������ � ��������
builder.Services.AddSingleton<TokenService>(provider =>
{
    return new TokenService(
        jwtKey: AuthOptions.KEY,
        issuer: AuthOptions.ISSUER,
        audience: AuthOptions.AUDIENCE,
        lifetimeMinutes: AuthOptions.LIFETIME
    );
});

// ��������� CORS-��������
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // �����, ���� ����� ���������� ���� ��� �����
    });
});

// ������ ����������
var app = builder.Build();

// Middleware (����������� ��������)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ������� middleware �����! CORS ������ ���� �� ��������������
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthentication(); // �������� �������� �������
app.UseAuthorization();  // �������� �����������

// �������� ������ (������� ��� ������� 500-� ������)
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError("������ �������: {0}", context.Response.StatusCode);
    });
});

app.MapControllers();

// �������� ��������� �����
app.Urls.Add("http://localhost:5000");  // HTTP
app.Urls.Add("https://localhost:7109"); // HTTPS

// ��������� ������
app.Run();
