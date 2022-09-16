using Microsoft.EntityFrameworkCore;
using Shard.API.Controllers;
using Shard.API.Models;
using Microsoft.Extensions.Configuration;
using Bogus.DataSets;
using Shard.API.Tools;
using Shard.Shared.Core;

//void ConfigureServices(IServiceCollection services)
//{
//    services.AddDbContext<ShardContext>(options =>
//    options.UseSqlServer(Configuration["ConnectionStrings:DefaultConnection"]));
//    services.AddMvc();
//    services.AddScoped<SystemsController>();
//}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContext<ShardContext>(opt => opt.UseInMemoryDatabase("Shard"));
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new() { Title = "ShardApi", Version = "v1" });
//});
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<SectorSpecification>(MapGenerator.Random.Generate());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
    //app.UseSwagger();
    //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShardApi v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

namespace Shard.API
{
    public partial class Program { }
}
