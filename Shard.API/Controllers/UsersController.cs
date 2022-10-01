using Bogus.DataSets;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shard.API.Models;
using Shard.Shared.Core;
using System.Web;
using System;
using System.Collections;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

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

        public UsersController(SectorSpecification sectorSpecification, List<UserSpecification> initialUsers, Hashtable initialUnits)
        {
            sector = sectorSpecification;
            users = initialUsers;
            units = initialUnits;
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
            if (userSpecification.Id == null || userSpecification.Pseudo == null || id != userSpecification.Id)
            {
                Response.StatusCode = (int) System.Net.HttpStatusCode.BadRequest;
                return BadRequest();
            }

            var user = new UserSpecification(userSpecification.Id, userSpecification.Pseudo);
            users.Add(user);

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
        public ActionResult<UnitSpecification> GetUnit(string userId, string unitId)
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

            return unit;
        }


        // PUT: /Users/{userId}/Units/{unitId} - changes the status of a unit of a user. Only changes system and planet which simulates moving
        [HttpPut("{userId}/Units/{unitId}")]
        [ProducesResponseType(typeof(UserSpecification), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<UserSpecification> PutUnit(string userId, string unitId, [FromBody] UnitSpecification unitSpecification)
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
            unit.System = unitSpecification.System;
            unit.Planet = unitSpecification.Planet;

            return user;
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
            LocationSpecification location = new LocationSpecification(system.Name, planet.Name, planet.ResourceQuantity);

            return location;
        }
    }
}
