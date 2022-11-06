using Microsoft.AspNetCore.Mvc;
using Shard.API.Models;
using Shard.Shared.Core;
using System.Collections;
using System.Text.RegularExpressions;
using Shard.API.Tools;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Shard.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SectorSpecification sector;
        private readonly List<UserSpecification> users;
        private readonly Hashtable units;
        private readonly List<Building> buildings;
        private readonly IClock clock;

        public UsersController(SectorSpecification sector, List<UserSpecification> users, Hashtable units, IClock systemClock)
        {
            this.sector = sector;
            this.users = users;
            this.units = units;
            clock = systemClock;
            buildings = new List<Building>();
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
        public ActionResult<UserSpecification> Get(string id)
        {
            var user = users.FirstOrDefault(user => user.Id == id);

            return user == null ? NotFound() : user;
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
                Response.StatusCode = (int) System.Net.HttpStatusCode.BadRequest;
                return BadRequest();
            }

            var user = new UserSpecification(userSpecification.Id, userSpecification.Pseudo);
            users.Add(user);

            string system = sector.Systems[new Random().Next(1, sector.Systems.Count)].Name;


            units.Add(user, new List<UnitSpecification>()
            {
                new UnitSpecification("9cc8f0cc-5b4c-439a-b60c-398bfb7600a6", "scout", system, null, system, null),
                new UnitSpecification("2kl1o9aa-9c0z-439a-a50d-840azb9800c8", "builder", system, null, system, null)
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

            var userUnits = (List<UnitSpecification>) units[user];

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

            var userUnits = (List<UnitSpecification>) units[user];

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
                    await unit.runningTask;
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

            var userUnits = (List<UnitSpecification>) units[user];

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
                unit.taskWaitTime =  unit.taskWaitTime + 15;
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
        public ActionResult<Building> Post(string userId, [FromBody] BuildingSpecification buildingSpecification)
        {
            var user = units.Keys.OfType<UserSpecification>().FirstOrDefault(user => user.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            if (HttpContext.Request.Body == null || buildingSpecification == null || buildingSpecification.Type != "mine" || buildingSpecification.BuilderId == null)
            {
                return BadRequest();
            }

            // Find associated unit and use it as a resource to build a new building
            var unit = ((List<UnitSpecification>)units[user]).FirstOrDefault(unit => unit.Id == buildingSpecification.BuilderId);

            if (unit == null || unit.Type != "builder" || unit.Planet == null)
            {
                return BadRequest();
            }

            // Creates a new building
            Building building = new Building(buildingSpecification.Id, buildingSpecification.Type, unit.System, unit.Planet);
            buildings.Add(building);
            return building;
        }
    }
}
