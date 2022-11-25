using Bogus;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Shard.API.Models;
using Shard.API.Services;
using Shard.API.Tools;
using Shard.Shared.Core;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

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

// Read from JSON file and generate the sector containg the systems with planets
List<Systems> systems;

var exedir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
var path = Path.Combine(exedir, @"Shard.Shared.Web.IntegrationTests\expectedTestSectorWithResources.json");
Console.WriteLine(path);

using (StreamReader reader = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "../../../../Shard.Shared.Web.IntegrationTests/expectedTestSectorWithResources.json"))
{
    string json = reader.ReadToEnd();
    systems = JsonConvert.DeserializeObject<List<Systems>>(json);
}

Console.WriteLine(Environment.CurrentDirectory);

if (systems == null)
{
    throw new FileNotFoundException("JSON file is either missing, or it's not formatted correctly");
}

/*foreach (var s in systems)
{
    Console.WriteLine("System: " + s.Name + "\nPlanets:");

    foreach (var p in s.Planets)
    {
        Console.WriteLine("\tPlanet: " + p.Name + "\nSize: " + p.Size + "\nResources:");

        foreach (var r in p.ResourceQuantity)
        {
            Console.WriteLine("\t\tResource: " + r.Key + "\nQuantity: " + r.Value);
        }
    }
}*/

// Generate SectorSpecification
//MapGeneratorOptions mapGeneratorOptions = builder.Configuration.Get<MapGeneratorOptions>();
MapGeneratorOptions mapGeneratorOptions = new MapGeneratorOptions();
mapGeneratorOptions.Seed = "Test application";
MapGenerator mapGenerator = new MapGenerator(mapGeneratorOptions);
SectorSpecification sectorSpecification = mapGenerator.Generate();
// Create users list
List<UserSpecification> users = new List<UserSpecification>();
// Create units hashtable
Dictionary<UserSpecification, List<UnitSpecification>> units = new Dictionary<UserSpecification, List<UnitSpecification>>();
// Create Dictionary of users with their associated buildings
Dictionary<UserSpecification, List<Building>> userBuildings = new Dictionary<UserSpecification, List<Building>>();

// Generate users, units & locations
foreach (var systemInstance in sectorSpecification.Systems)
{
    var user = new UserSpecification();
    users.Add(user);
    var userUnits = new List<UnitSpecification>();

    foreach (var planetInstance in systemInstance.Planets)
    {
        var unit = new UnitSpecification(RandomIdGenerator.RandomString(10), "scout", systemInstance.Name, planetInstance.Name, systemInstance.Name, planetInstance.Name);
        userUnits.Add(unit);
    }

    units.Add(user, userUnits);
    userUnits = new List<UnitSpecification>();
}

builder.Services.AddSingleton<SectorSpecification>(sectorSpecification);
builder.Services.AddSingleton<List<Systems>>(systems);
builder.Services.AddSingleton<List<UserSpecification>>(users);
builder.Services.AddSingleton<Dictionary<UserSpecification, List<UnitSpecification>>>(units);
builder.Services.AddSingleton<Dictionary<UserSpecification, List<Building>>>(userBuildings);
builder.Services.AddSingleton<IClock>(new SystemClock());
builder.Services.AddSingleton<List<ResourceKind>>(new List<ResourceKind>());
builder.Services.AddSingleton<List<int>>(new List<int>());
builder.Services.AddSingleton<CancellationTokenSource>(new CancellationTokenSource());
builder.Services.AddSingleton<CombatService>(new CombatService());

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
