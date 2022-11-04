using Bogus;
using Microsoft.CodeAnalysis;
using Shard.API.Models;
using Shard.API.Tools;
using Shard.Shared.Core;
using System;
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

// Generate SectorSpecification
SectorSpecification sectorSpecification = MapGenerator.Random.Generate();
// Create users list
List<UserSpecification> users = new List<UserSpecification>();
// Create units hashtable
Hashtable units = new Hashtable();
// Create List of buildings
List<Building> buildings = new List<Building>();

// Generate users, units & locations
foreach (var system in sectorSpecification.Systems)
{
    var user = new UserSpecification();
    users.Add(user);
    var userUnits = new List<UnitSpecification>();

    foreach (var planet in system.Planets)
    {
        var unit = new UnitSpecification(RandomIdGenerator.RandomString(10), "scout", system.Name, planet.Name, system.Name, planet.Name);
        userUnits.Add(unit);
    }

    units.Add(user, userUnits);
    userUnits = new List<UnitSpecification>();
}

builder.Services.AddSingleton<SectorSpecification>(sectorSpecification);
builder.Services.AddSingleton<List<UserSpecification>>(users);
builder.Services.AddSingleton<Hashtable>(units);
builder.Services.AddSingleton<List<Building>>(buildings);
builder.Services.AddSingleton<IClock>(new SystemClock());

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
