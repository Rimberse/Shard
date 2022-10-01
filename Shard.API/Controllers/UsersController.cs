using Bogus.DataSets;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shard.API.Models;
using Shard.Shared.Core;
using System.Web;
using System;
using System.Collections;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Shard.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly List<UserSpecification> users;
        private readonly Hashtable units;

        public UsersController(List<UserSpecification> initialUsers, Hashtable initialUnits)
        {
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
    }
}
