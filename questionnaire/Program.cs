using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using questionnaire.Models;
using questionnaire.Authentication;
using questionnaire.Services;

var builder = WebApplication.CreateBuilder(args);

// ��������� �����������
builder.Services.AddControllers();

// ��������� Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ��������� �������������� JWT
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

// ��������� �����������
builder.Services.AddAuthorization();

// ��������� DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("������ ����������� 'DefaultConnection' �� �������.");
}

builder.Services.AddDbContext<QuestionnaireContext>(options =>
    options.UseSqlServer(connectionString));

// ����������� ��������
builder.Services.AddSingleton<TokenService>(provider =>
{
    return new TokenService(
        jwtKey: AuthOptions.KEY,
        issuer: AuthOptions.ISSUER,
        audience: AuthOptions.AUDIENCE,
        lifetimeMinutes: AuthOptions.LIFETIME
    );
});

// ������� ����������
var app = builder.Build();

// ��������� middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication(); // �������������� ������ ���� ����� ������������
app.UseAuthorization();
app.MapControllers();

// ��������� ����������
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
