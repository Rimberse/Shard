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
        private readonly List<Systems> _systems;
        private readonly SectorSpecification _sector;
        private readonly List<UserSpecification> _users;
        private readonly Hashtable _units;
        private readonly Dictionary<UserSpecification, List<Building>> _userBuildings;
        private readonly IClock _clock;
        private readonly List<ResourceKind> _extractedResources;
        private readonly List<int> _extractionTimes;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public UsersController(SectorSpecification sector, List<Systems> systems, List<UserSpecification> users, Hashtable units, Dictionary<UserSpecification, List<Building>> userBuildings, IClock systemClock, List<ResourceKind> extractedResources, List<int> extractionTimes, CancellationTokenSource cancellationTokenSource)
        {
            _systems = systems;
            _sector = sector;
            _users = users;
            _units = units;
            _clock = systemClock;
            _userBuildings = userBuildings;
            _extractedResources = extractedResources;
            _extractionTimes = extractionTimes;
            _cancellationTokenSource = cancellationTokenSource;
        }


        // GET: /Users - Fetches all users
        [HttpGet]
        public IReadOnlyList<UserSpecification> Get()
        {
            return _users;
        }


        // GET: /Users/id - Returns details of an existing user
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserSpecification), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserSpecification>> Get(string id)
        {
            var user = _users.FirstOrDefault(user => user.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // Calculate the time required to build user's buildings, await necessary time, attribute resource to user
            foreach (var building in _userBuildings[user])
            {
                TimeSpan time = (TimeSpan)(_clock.Now - building.EstimatedBuildTime);
                int minutes = (int)time.TotalMinutes;

                if (time > TimeSpan.FromMinutes(0))
                {
                    int lastExtractionTime = minutes;
                    SystemSpecification system = _sector.Systems.FirstOrDefault(system => system.Name == building.System);
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
                        if (_extractionTimes.LastOrDefault(-1) == lastExtractionTime)
                        {
                            break;
                        }

                        // If all resources have been extracted (sort by rarity), then re-start from the rarest again
                        if (_extractedResources.Count == planet.ResourceQuantity.Count)
                        {
                            _extractedResources.Clear();
                        }

                        // There is no natural way to sort a dictionary, since it keeps insertion order
                        resourcesQuantity = planet.ResourceQuantity.Where(entry => entry.Key != ResourceKind.Oxygen && entry.Key != ResourceKind.Water && !_extractedResources.Contains(entry.Key)).ToDictionary(entry => entry.Key, entry => entry.Value);
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

                            if (!_extractedResources.Contains(resourceQuantity.Key))
                            {
                                _extractedResources.Add(resourceQuantity.Key);
                            }

                            if (resourcesQuantity[resourceQuantity.Key] > 0 && minutes > 0 && _extractionTimes.LastOrDefault(-1) != minutes)
                            {
                                string resource = resourceQuantity.Key.ToString().ToLower();
                                minutes -= _extractionTimes.LastOrDefault(0);

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
                                            minutes -= _extractionTimes.LastOrDefault(0);

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

                        }
                        else if (building.ResourceCategory == "liquid" || building.ResourceCategory == "gaseous")
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

                    if (!_extractionTimes.Contains(lastExtractionTime))
                    {
                        _extractionTimes.Add(lastExtractionTime);
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
            _users.Add(user);
            _userBuildings.Add(user, new List<Building>());

            SystemSpecification system = _sector.Systems[new Random().Next(1, _sector.Systems.Count)];


            _units.Add(user, new List<UnitSpecification>()
            {
                new UnitSpecification(Guid.NewGuid().ToString(), "scout", system.Name, null, system.Name, null),
                new UnitSpecification(Guid.NewGuid().ToString(), "builder", system.Name, null, system.Name, null)
            });

            return user;
        }


        // GET: /Users/{userId}/Units - returns all units of a user
        [HttpGet("{userId}/Units")]
        [ProducesResponseType(typeof(UnitSpecification), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IReadOnlyList<UnitSpecification>> GetUnits(string userId)
        {
            var user = _units.Keys.OfType<UserSpecification>().FirstOrDefault(user => user.Id == userId);

            if (user == null || _units[user] == null)
            {
                return NotFound();
            }

            var userUnits = (List<UnitSpecification>)_units[user];

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
            UserSpecification user = _units.Keys.OfType<UserSpecification>().FirstOrDefault(user => user.Id == userId);

            if (user == null || _units[user] == null)
            {
                return NotFound();
            }

            var userUnits = (List<UnitSpecification>)_units[user];

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
                DateTime now = _clock.Now;
                // Get current time representation in seconds
                int time = (now.Hour * 60 * 60) + (now.Minute * 60) + now.Second;

                if (unit.taskWaitTime - time <= 2)
                {
                    await unit.runningTask;
                    unit.runningTask = null;
                }
            }

            return unit;
        }


        private async Task moveUnitBackgroundTask(UnitSpecification unit, Boolean systemChanged, Boolean planetChanged)
        {
            if (systemChanged)
            {
                await _clock.Delay(60000);
                unit.System = unit.DestinationSystem;
            }

            if (planetChanged)
            {
                await _clock.Delay(15000);
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

            var user = _units.Keys.OfType<UserSpecification>().FirstOrDefault(user => user.Id == userId);

            if (user == null || _units[user] == null)
            {
                return NotFound();
            }

            var userUnits = (List<UnitSpecification>)_units[user];

            if (userUnits == null)
            {
                return NotFound();
            }

            var unit = userUnits.FirstOrDefault(unit => unit.Id == unitId);

            if (unit == null)
            {
                return NotFound();
            }

            if (_userBuildings.ContainsKey(user))
            {
                var buildings = _userBuildings[user].Where(b => b.UnitUsed.Id == unitId).ToList();
                foreach (var building in buildings.Where(building =>
                             (unit!.Planet != unit.DestinationPlanet && unit.Planet == building.Planet && !building.IsBuilt) ||
                             (unit.System != unit.DestinationSystem && unit.System == building.System && !building.IsBuilt)))
                {
                    _userBuildings[user].Remove(building);
                }
            }

            //var building = userBuildings[user].FirstOrDefault(building => building.UnitUsed.Id == unitId);
            /*if (!unit.ConstructedBuilding.IsBuilt)
            {*/
            //cancellationTokenSource.Cancel();
            //}

            // Change the location of a unit
            Boolean systemChanged = false;
            Boolean planetChanged = false;

            DateTime now = _clock.Now;
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
            var user = _units.Keys.OfType<UserSpecification>().FirstOrDefault(user => user.Id == userId);

            if (user == null || _units[user] == null)
            {
                return NotFound();
            }

            var userUnits = (List<UnitSpecification>)_units[user];

            if (userUnits == null)
            {
                return NotFound();
            }

            var unit = userUnits.FirstOrDefault(unit => unit.Id == unitId);

            if (unit == null)
            {
                return NotFound();
            }

            var system = _sector.Systems.FirstOrDefault(system => system.Name == unit.System);

            if (system == null)
            {
                return NotFound();
            }

            var planet = system.Planets.FirstOrDefault(planet => planet.Name == unit.Planet);
            LocationSpecification location = unit.Type == "builder" ? new LocationSpecification(system.Name, planet.Name) : new LocationSpecification(system.Name, planet.Name, planet.ResourceQuantity);

            return location;
        }


        private async Task buildBuildingBackgroundTask(Building building, int estimatedMoveTime, UserSpecification user)
        {
            /*await Task.Run(async () =>
            {
                var temp = userBuildings[user].First(b => b.Id == building.Id);
                if (temp != null)
                {
                    await clock.Delay(TimeSpan.FromMinutes(5));
                    building.IsBuilt = true;
                    building.EstimatedBuildTime = null;
                    building.ConstructionTask = null;
                }
            });*/

            await _clock.Delay(300000 + (estimatedMoveTime * 1000));
            building.IsBuilt = true;
            building.EstimatedBuildTime = null;
            building.ConstructionTask = null;
        }


        // POST /users/{userId}/Buildings
        [HttpPost("{userId}/Buildings")]
        [ProducesResponseType(typeof(BuildingSpecification), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Building> PostBuilding(string userId, [FromBody] BuildingSpecification buildingSpecification)
        {
            var user = _units.Keys.OfType<UserSpecification>().FirstOrDefault(user => user.Id == userId);

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
            var unit = ((List<UnitSpecification>)_units[user]).FirstOrDefault(unit => unit.Id == buildingSpecification.BuilderId);

            if (unit == null || unit.Type != "builder" || unit.Planet == null)
            {
                return BadRequest();
            }

            // JSON payload can contain nullable Id field, hence the verification step to ensure we are buulding with a valid Id
            string buildingId = (buildingSpecification.Id) == null ? Guid.NewGuid().ToString() : buildingSpecification.Id;

            // Creates a new building
            Building building = new Building(buildingId, buildingSpecification.Type, unit.System, unit.Planet, buildingSpecification.ResourceCategory, new DateTime().AddMinutes(5).AddSeconds((double)unit.taskWaitTime), unit);
            //building.ConstructionTask = buildBuildingBackgroundTask(building, (int) unit.taskWaitTime, user);

            List<Building> buildings;
            if (_userBuildings[user] == null)
            {
                buildings = new List<Building>();
            }
            else
            {
                buildings = _userBuildings[user];
            }

            if (!buildings.Contains(building))
            {
                buildings.Add(building);
            }

            _userBuildings[user] = buildings;

            return building;
        }


        // GET /users/{userId}/Buildings
        [HttpGet("{userId}/Buildings")]
        [ProducesResponseType(typeof(List<Building>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<Building>> GetBuildings(string userId)
        {
            var user = _units.Keys.OfType<UserSpecification>().FirstOrDefault(user => user.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            return _userBuildings[user];
        }


        public async Task updateBuildingProgressStatus(Building building, int delayTime)
        {
            await _clock.Delay(delayTime, _cancellationTokenSource.Token);

            building.IsBuilt = true;
            building.EstimatedBuildTime = null;
        }


        // GET /users/{userId}/Buildings/{buildingId}
        [HttpGet("{userId}/Buildings/{buildingId}")]
        [ProducesResponseType(typeof(Building), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Building>> GetBuilding(string userId, string buildingId)
        {
            var user = _units.Keys.OfType<UserSpecification>().FirstOrDefault(user => user.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var building = _userBuildings[user].FirstOrDefault(building => building.Id == buildingId);

            if (buildingId == null || building == null)
            {
                return NotFound();
            }

            // Building which is being constructed must be cancelled, if the unit used for this process has been moved elsewhere
            if (building.UnitUsed.System != building.UnitUsed.DestinationSystem || building.UnitUsed.Planet != building.UnitUsed.DestinationPlanet)
            {
                _userBuildings[user].Remove(building);
                return NotFound();
            }

            if (building.IsBuilt == false && building.EstimatedBuildTime != null)
            {
                if (building.EstimatedBuildTime - _clock.Now <= TimeSpan.FromSeconds(2))
                {
                    //await building.ConstructionTask;

                    while (building.EstimatedBuildTime - _clock.Now >= TimeSpan.Zero)
                    {
                        try
                        {
                            //building = userBuildings[user].FirstOrDefault(b => b.Id == buildingId) ?? throw new InvalidOperationException();
                            building.IsBuilt = true;
                            building.EstimatedBuildTime = null;
                            //building.UnitUsed.runningTask = null;

                            Thread.Sleep(500);

                            if (building.UnitUsed.runningTask != null || building.EstimatedBuildTime != null || building.IsBuilt == false)
                            {
                                //throw new InvalidOperationException();
                                return NotFound();
                            }
                        }
                        catch (Exception)
                        {
                            return NotFound();
                        }

                        /*int miliseconds = (int)((TimeSpan)(building.EstimatedBuildTime - clock.Now)).TotalMilliseconds;
                        await clock.Delay((miliseconds > 500 ? 500 : miliseconds), cancellationTokenSource.Token);*/
                    }
                    //await clock.Delay(30000);
                   /* building.IsBuilt = true;
                    building.EstimatedBuildTime = null;*/
                }
            }

            return building;
        }
    }
}
