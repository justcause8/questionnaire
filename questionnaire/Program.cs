using questionnaire.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace questionnaire.Models
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            //IServiceCollection serviceCollection = builder.Services.AddDbContext<QuestionnaireContext>(options => options.UseSqlServer(connectionString));

            builder.Services.AddDbContext<questionnaireContext>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

    }
}
