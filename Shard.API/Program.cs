using Shard.API.Models;
using Shard.Shared.Core;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new() { Title = "ShardApi", Version = "v1" });
//});
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// Generate users
List<UserSpecification> users = new List<UserSpecification>()
{
    new UserSpecification()
    {
        
    },
    new UserSpecification()
    {

    },
    new UserSpecification()
    {

    },
    new UserSpecification()
    {

    },
    new UserSpecification()
    {

    },
};

builder.Services.AddSingleton<SectorSpecification>(MapGenerator.Random.Generate());
builder.Services.AddSingleton<List<UserSpecification>>(users);

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
