using Shard.API.Models;
using Shard.Shared.Core;
using System.Collections;

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
UserSpecification user1 = new UserSpecification();
UserSpecification user2 = new UserSpecification();
UserSpecification user3 = new UserSpecification();
UserSpecification user4 = new UserSpecification();
UserSpecification user5 = new UserSpecification();

List<UserSpecification> users = new List<UserSpecification>()
{
    user1, user2, user3, user4, user5
};

// Generate units
Hashtable units = new Hashtable()
{
    {
        user1, new List<UnitSpecification>()
        {
            new UnitSpecification(),
            new UnitSpecification("abc", "scout", "alpha-centauri", "meteor"),
            new UnitSpecification("dce", "ranger", "lost-world", "unkown")
        }
    },
    {
        user2, new List<UnitSpecification>()
        {
            new UnitSpecification("adfsa22", "scout", "alpha-centauri", "mars")
        }
    },
    {
        user3, new List<UnitSpecification>()
        {
            new UnitSpecification("adsfs-344kfdf-23df", "scout", "alpha-centauri", "earth"),
            new UnitSpecification("132sd-2323dlf-fdg4", "ranger", "beta-centauri", "34ABC")
        }
    },
    {
        user4, new List<UnitSpecification>()
        {
            new UnitSpecification("adffd-23ddgg-sdv2", "scout", "alpha-centauri", "venus"),
            new UnitSpecification("fadsfs-2dffgb-dffgf", "ranger", "andromeda", "light")
        }
    },
    {
        user5, new List<UnitSpecification>()
        {
            new UnitSpecification("0abc-1def-2ghk", "secret", "unkown-worlds", "unkown")
        }
    }
};

builder.Services.AddSingleton<SectorSpecification>(MapGenerator.Random.Generate());
builder.Services.AddSingleton<List<UserSpecification>>(users);
builder.Services.AddSingleton<Hashtable>(units);

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
