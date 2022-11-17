using Microsoft.AspNetCore.Mvc;
using Shard.API.Models;
using Shard.Shared.Core;
using System.Collections;
using System.Resources;
using System.Security.Cryptography.Xml;
using System.Text.RegularExpressions;
using System.Threading;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Shard.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly List<Systems> systems;
        private readonly SectorSpecification sector;
        private readonly List<UserSpecification> users;
        private readonly Hashtable units;
        private readonly Dictionary<UserSpecification, List<Building>> userBuildings;
        private readonly IClock clock;
        private readonly List<ResourceKind> extractedResources;
        private readonly List<int> extractionTimes;
        private readonly CancellationTokenSource cancellationTokenSource;

        public UsersController(SectorSpecification sector, List<Systems> systems, List<UserSpecification> users, Hashtable units, Dictionary<UserSpecification, List<Building>> userBuildings, IClock systemClock, List<ResourceKind> extractedResources, List<int> extractionTimes, CancellationTokenSource cancellationTokenSource)
        {
            this.systems = systems;
            this.sector = sector;
            this.users = users;
            this.units = units;
            clock = systemClock;
            this.userBuildings = userBuildings;
            this.extractedResources = extractedResources;
            this.extractionTimes = extractionTimes;
            this.cancellationTokenSource = cancellationTokenSource;
        }


        // GET: /Users - Fetches all users
        [HttpGet]
        public IReadOnlyList<UserSpecification> Get()
        {
            return users;
        }


        // GET: /Users/id - Returns details of an existing user
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserSpecification), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserSpecification>> Get(string id)
        {
            var user = users.FirstOrDefault(user => user.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // Calculate the time required to build user's buildings, await necessary time, attribute resource to user
            foreach (var building in userBuildings[user])
            {
                TimeSpan time = (TimeSpan) (clock.Now - building.EstimatedBuildTime);
                int minutes = (int) time.TotalMinutes;

                if (time > TimeSpan.FromMinutes(0))
                {
                    int lastExtractionTime = minutes;
                    SystemSpecification system = sector.Systems.FirstOrDefault(system => system.Name == building.System);
                    PlanetSpecification planet = system.Planets.FirstOrDefault(planet => planet.Name == building.Planet);
                    Dictionary<string, int> userResourcesQuantity = UserSpecification.initiliazeResources();
                    Dictionary<ResourceKind, int> resourcesQuantity = null;
                    List<KeyValuePair<ResourceKind, int>> resources = null;

                    // Filter out extraction resource, taking into account the resource category the building uses to it's construction
                    if (building.ResourceCategory == "gaseous")
                    {
                        resourcesQuantity = planet.ResourceQuantity.Where(entry => entry.Key == ResourceKind.Oxygen).ToDictionary(entry => entry.Key, entry => entry.Value);
                    }
                    else if (building.ResourceCategory == "liquid")
                    {
                        resourcesQuantity = planet.ResourceQuantity.Where(entry => entry.Key == ResourceKind.Water).ToDictionary(entry => entry.Key, entry => entry.Value);
                    }
                    else if (building.ResourceCategory == "solid")
                    {
                        // Do not extract anything if no time has passed since the last extraction
                        if (extractionTimes.LastOrDefault(-1) == lastExtractionTime)
                        {
                            break;
                        }

                        // If all resources have been extracted (sort by rarity), then re-start from the rarest again
                        if (extractedResources.Count == planet.ResourceQuantity.Count)
                        {
                            extractedResources.Clear();
                        }

                        // There is no natural way to sort a dictionary, since it keeps insertion order
                        resourcesQuantity = planet.ResourceQuantity.Where(entry => entry.Key != ResourceKind.Oxygen && entry.Key != ResourceKind.Water && !extractedResources.Contains(entry.Key)).ToDictionary(entry => entry.Key, entry => entry.Value);
                        //resourcesQuantity = resourcesQuantity.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                        // Due to this we transform it to a list
                        resources = resourcesQuantity.ToList();
                        // And sort this list in descending order
                        resources.Sort((solid1, solid2) => solid2.Value.CompareTo(solid1.Value));
                    }

                    while (minutes > 0)
                    {
                        if (building.ResourceCategory == "solid")
                        {
                            KeyValuePair<ResourceKind, int> resourceQuantity = resources.First();

                            if (!extractedResources.Contains(resourceQuantity.Key))
                            {
                                extractedResources.Add(resourceQuantity.Key);
                            }

                            if (resourcesQuantity[resourceQuantity.Key] > 0 && minutes > 0 && extractionTimes.LastOrDefault(-1) != minutes)
                            {
                                string resource = resourceQuantity.Key.ToString().ToLower();
                                minutes -= extractionTimes.LastOrDefault(0);

                                if (minutes > resourcesQuantity[resourceQuantity.Key])
                                {
                                    user.ResourcesQuantity[resource] = user.ResourcesQuantity[resource] + resourcesQuantity[resourceQuantity.Key];
                                    minutes -= resourcesQuantity[resourceQuantity.Key];
                                }
                                else
                                {
                                    user.ResourcesQuantity[resource] = user.ResourcesQuantity[resource] + minutes;
                                    minutes = 0;
                                }

                                resourcesQuantity[resourceQuantity.Key] = 0;

                                // If there is still time left, extract leftover resources, by going over each one of them
                                if (minutes > 0)
                                {
                                    for (int i = 1; i < resources.Count; i++)
                                    {
                                        resourceQuantity = resources[i];

                                        if (resourcesQuantity[resourceQuantity.Key] > 0 && minutes > 0)
                                        {
                                            resource = resourceQuantity.Key.ToString().ToLower();
                                            minutes -= extractionTimes.LastOrDefault(0);

                                            if (minutes > resourcesQuantity[resourceQuantity.Key])
                                            {
                                                user.ResourcesQuantity[resource] = user.ResourcesQuantity[resource] + resourcesQuantity[resourceQuantity.Key];
                                                minutes -= resourcesQuantity[resourceQuantity.Key];
                                            }
                                            else
                                            {
                                                user.ResourcesQuantity[resource] = user.ResourcesQuantity[resource] + minutes;
                                                minutes = 0;
                                            }

                                            resourcesQuantity[resourceQuantity.Key] = 0;
                                        }
                                    }
                                }
                            }

                        } else if (building.ResourceCategory == "liquid" || building.ResourceCategory == "gaseous")
                        {
                            foreach (KeyValuePair<ResourceKind, int> resourceQuantity in resourcesQuantity)
                            {
                                if (resourcesQuantity[resourceQuantity.Key] > 0 && minutes > 0)
                                {
                                    string resource = resourceQuantity.Key.ToString().ToLower();

                                    if (minutes > resourcesQuantity[resourceQuantity.Key])
                                    {
                                        userResourcesQuantity[resource] = userResourcesQuantity[resource] + resourcesQuantity[resourceQuantity.Key];
                                        minutes -= resourcesQuantity[resourceQuantity.Key];
                                    }
                                    else
                                    {
                                        userResourcesQuantity[resource] = userResourcesQuantity[resource] + minutes;
                                        minutes = 0;
                                    }

                                    resourcesQuantity[resourceQuantity.Key] = 0;
                                }
                            }
                        }

                        if (minutes != 0)
                        {
                            minutes = 0;
                            break;
                        }
                    }

                    if (building.ResourceCategory != "solid")
                    {
                        user.ResourcesQuantity = userResourcesQuantity;
                    }

                    if (!extractionTimes.Contains(lastExtractionTime))
                    {
                        extractionTimes.Add(lastExtractionTime);
                    }
                }
            }

            return user;
        }


        // PUT: /Users/id - creates a new user
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserSpecification), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<UserSpecification> Put(string id, [FromBody] UserSpecification userSpecification)
        {
            // Check if the id respects required format: any alphanumeric value
            var regex = new Regex("^[a-zA-Z0-9-]*$");

            if (userSpecification.Id == null || userSpecification.Pseudo == null || id != userSpecification.Id || !regex.Match(userSpecification.Id).Success)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return BadRequest();
            }

            var user = new UserSpecification(userSpecification.Id, userSpecification.Pseudo);
            users.Add(user);
            userBuildings.Add(user, new List<Building>());

            //Systems system = systems[new Random().Next(1, systems.Count)];
            SystemSpecification system = sector.Systems[new Random().Next(1, sector.Systems.Count)];


            units.Add(user, new List<UnitSpecification>()
            {
                new UnitSpecification("9cc8f0cc-5b4c-439a-b60c-398bfb7600a6", "scout", system.Name, null, system.Name, null),
                new UnitSpecification("2kl1o9aa-9c0z-439a-a50d-840azb9800c8", "builder", system.Name, null, system.Name, null)
            });

            return user;
        }


        // GET: /Users/{userId}/Units - returns all units of a user
        [HttpGet("{userId}/Units")]
        [ProducesResponseType(typeof(UnitSpecification), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IReadOnlyList<UnitSpecification>> GetUnits(string userId)
        {
            var user = units.Keys.OfType<UserSpecification>().FirstOrDefault(user => user.Id == userId);

            if (user == null || units[user] == null)
            {
                return NotFound();
            }

            var userUnits = (List<UnitSpecification>)units[user];

            if (userUnits == null)
            {
                return NotFound();
            }

            return userUnits;
        }


        // GET: /Users/{userId}/Units/{unitId} - returns information about one single unit of a user
        [HttpGet("{userId}/Units/{unitId}")]
        [ProducesResponseType(typeof(UnitSpecification), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UnitSpecification>> GetUnit(string userId, string unitId)
        {
            var user = units.Keys.OfType<UserSpecification>().FirstOrDefault(user => user.Id == userId);

            if (user == null || units[user] == null)
            {
                return NotFound();
            }

            var userUnits = (List<UnitSpecification>)units[user];

            if (userUnits == null)
            {
                return NotFound();
            }

            var unit = userUnits.FirstOrDefault(unit => unit.Id == unitId);

            if (unit == null)
            {
                return NotFound();
            }

            if (unit.runningTask != null)
            {
                DateTime now = clock.Now;
                // Get current time representation in seconds
                int time = (now.Hour * 60 * 60) + (now.Minute * 60) + now.Second;

                if (unit.taskWaitTime - time <= 2)
                {
                    await unit.runningTask;
                    cancellationTokenSource.Cancel();
                }
            }

            return unit;
        }


        public async Task moveUnitBackgroundTask(UnitSpecification unit, Boolean systemChanged, Boolean planetChanged)
        {
            if (systemChanged)
            {
                await clock.Delay(60000);
                unit.System = unit.DestinationSystem;
            }

            if (planetChanged)
            {
                await clock.Delay(15000);
                unit.Planet = unit.DestinationPlanet;
            }
        }


        // PUT: /Users/{userId}/Units/{unitId} - changes the status of a unit of a user. Only changes system and planet which simulates moving
        [HttpPut("{userId}/Units/{unitId}")]
        [ProducesResponseType(typeof(UserSpecification), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<UnitSpecification> PutUnit(string userId, string unitId, [FromBody] UnitSpecification unitSpecification)
        {
            if (HttpContext.Request.Body == null || unitSpecification == null || unitSpecification.Id != unitId)
            {
                return BadRequest();
            }

            var user = units.Keys.OfType<UserSpecification>().FirstOrDefault(user => user.Id == userId);

            if (user == null || units[user] == null)
            {
                return NotFound();
            }

            var userUnits = (List<UnitSpecification>)units[user];

            if (userUnits == null)
            {
                return NotFound();
            }

            var unit = userUnits.FirstOrDefault(unit => unit.Id == unitId);

            if (unit == null)
            {
                return NotFound();
            }

            // Change the location of a unit
            Boolean systemChanged = false;
            Boolean planetChanged = false;

            DateTime now = clock.Now;
            unit.taskWaitTime = (now.Hour * 60 * 60) + (now.Minute * 60) + now.Second;

            if (unit.System != unitSpecification.DestinationSystem)
            {
                systemChanged = true;
                unit.taskWaitTime = unit.taskWaitTime + 60;
            }

            if (unit.Planet != unitSpecification.DestinationPlanet)
            {
                planetChanged = true;
                unit.taskWaitTime = unit.taskWaitTime + 15;
            }

            unit.DestinationSystem = unitSpecification.DestinationSystem;
            unit.DestinationPlanet = unitSpecification.DestinationPlanet;

            unit.runningTask = moveUnitBackgroundTask(unit, systemChanged, planetChanged);

            return unit;
        }


        // GET: /Users/{userId}/Units/{unitId}/location - returns more detailed information about the location of unit
        [HttpGet("{userId}/Units/{unitId}/location")]
        [ProducesResponseType(typeof(LocationSpecification), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<LocationSpecification> GetLocation(string userId, string unitId)
        {
            var user = units.Keys.OfType<UserSpecification>().FirstOrDefault(user => user.Id == userId);

            if (user == null || units[user] == null)
            {
                return NotFound();
            }

            var userUnits = (List<UnitSpecification>)units[user];

            if (userUnits == null)
            {
                return NotFound();
            }

            var unit = userUnits.FirstOrDefault(unit => unit.Id == unitId);

            if (unit == null)
            {
                return NotFound();
            }

            var system = sector.Systems.FirstOrDefault(system => system.Name == unit.System);

            if (system == null)
            {
                return NotFound();
            }

            var planet = system.Planets.FirstOrDefault(planet => planet.Name == unit.Planet);
            LocationSpecification location = unit.Type == "builder" ? new LocationSpecification(system.Name, planet.Name) : new LocationSpecification(system.Name, planet.Name, planet.ResourceQuantity);

            return location;
        }


        // POST /users/{userId}/Buildings
        [HttpPost("{userId}/Buildings")]
        [ProducesResponseType(typeof(BuildingSpecification), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Building> PostBuilding(string userId, [FromBody] BuildingSpecification buildingSpecification)
        {
            var user = units.Keys.OfType<UserSpecification>().FirstOrDefault(user => user.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            if (HttpContext.Request.Body == null || buildingSpecification == null || buildingSpecification.Type != "mine" || buildingSpecification.BuilderId == null
                || !buildingSpecification.Resources.ContainsKey(buildingSpecification.ResourceCategory))
            {
                return BadRequest();
            }

            // Find associated unit and use it as a resource to build a new building
            var unit = ((List<UnitSpecification>)units[user]).FirstOrDefault(unit => unit.Id == buildingSpecification.BuilderId);

            if (unit == null || unit.Type != "builder" || unit.Planet == null)
            {
                return BadRequest();
            }

            // JSON payload can contain nullable Id field, hence the verification step to ensure we are buulding with a valid Id
            string buildingId = (buildingSpecification.Id) == null ? "3kgl9f1aa-7h3c-b987-b60c-b198fb12300z0" : buildingSpecification.Id;

            // Creates a new building
            Building building = new Building(buildingId, buildingSpecification.Type, unit.System, unit.Planet, buildingSpecification.ResourceCategory, new DateTime().AddMinutes(5).AddSeconds((double)unit.taskWaitTime), unit);

            List<Building> buildings;
            if (userBuildings[user] == null)
            {
                buildings = new List<Building>();
            }
            else
            {
                buildings = userBuildings[user];
            }

            if (!buildings.Contains(building))
            {
                buildings.Add(building);
            }

            userBuildings[user] = buildings;
            return building;
        }


        // GET /users/{userId}/Buildings
        [HttpGet("{userId}/Buildings")]
        [ProducesResponseType(typeof(List<Building>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<Building>> GetBuildings(string userId)
        {
            var user = units.Keys.OfType<UserSpecification>().FirstOrDefault(user => user.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            return userBuildings[user];
        }


        public async Task updateBuildingProgressStatus(Building building, int delayTime)
        {
            await clock.Delay(delayTime, cancellationTokenSource.Token);

            /*if (building.UnitUsed.DestinationPlanet == null)
            {
                return;
            }*/

            building.IsBuilt = true;
            building.EstimatedBuildTime = null;
        }


        // GET /users/{userId}/Buildings/{buildingId}
        [HttpGet("{userId}/Buildings/{buildingId}")]
        [ProducesResponseType(typeof(Building), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Building>> GetBuilding(string userId, string buildingId)
        {
            var user = units.Keys.OfType<UserSpecification>().FirstOrDefault(user => user.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var building = userBuildings[user].FirstOrDefault(building => building.Id == buildingId);

            if (buildingId == null || building == null)
            {
                return NotFound();
            }

            // Building which is being constructd must be cancelled, if the unit used for this process has been moved elsewhere
            if (building.UnitUsed.System != building.UnitUsed.DestinationSystem || building.UnitUsed.Planet != building.UnitUsed.DestinationPlanet)
            {
                userBuildings[user].Remove(building);
                return NotFound();
            }

            if (building.IsBuilt == false && building.EstimatedBuildTime != null)
            {
                DateTime now = clock.Now;
                TimeSpan time = (TimeSpan)(building.EstimatedBuildTime - now);

                if (time <= TimeSpan.FromSeconds(2))
                {
                    /*if (building.UnitUsed.runningTask != null)
                    {
                        await building.UnitUsed.runningTask;

                        if (building.UnitUsed.System != building.UnitUsed.DestinationSystem || building.UnitUsed.Planet != building.UnitUsed.DestinationPlanet)
                        {
                            return NotFound();
                        }
                    }*/

                    //CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    //building.cancellationToken = cancellationTokenSource;
                    try
                    {
                        //await updateBuildingProgressStatus(building, (int)time.TotalMilliseconds);

                        while (building.EstimatedBuildTime - clock.Now >= TimeSpan.Zero)
                        {
                            try
                            {
                                int miliseconds = (int)((TimeSpan)(building.EstimatedBuildTime - clock.Now)).TotalMilliseconds;
                                await clock.Delay((miliseconds > 500 ? 500 : miliseconds), cancellationTokenSource.Token);
                            }
                            catch (Exception)
                            {
                                return NotFound();
                            }
                        }
                    } catch (TaskCanceledException)
                    {
                        if (cancellationTokenSource.IsCancellationRequested)
                        {
                            return NotFound();
                        }
                    }

                    building.IsBuilt = true;
                    building.EstimatedBuildTime = null;
                }
            }

            return building;
        }
    }
}
