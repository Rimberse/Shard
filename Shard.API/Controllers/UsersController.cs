using Bogus.DataSets;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shard.API.Models;
using Shard.Shared.Core;
using System.Web;
using System;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Shard.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly List<UserSpecification> users;

        public UsersController(List<UserSpecification> initialUsers)
        {
            users = initialUsers;
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
            var json = "{\r\n  \"type\": \"string\",\r\n  \"title\": \"string\",\r\n  \"status\": 0,\r\n  " +
                "\"detail\": \"string\",\r\n  \"instance\": \"string\",\r\n  \"additionalProp1\": \"string\",\r\n  " +
                "\"additionalProp2\": \"string\",\r\n  \"additionalProp3\": \"string\"\r\n}";

            Response.StatusCode = (int) System.Net.HttpStatusCode.NotFound;

            return user == null ? NotFound() : user;
            //return JsonConvert.DeserializeObject<NotFoundSpecification>(json);
        }

        /*// POST api/<UsersController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }*/

        // PUT: /Users/id - creates a new user
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserSpecification), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<UserSpecification> Put(string id, [FromBody] UserSpecification userSpecification)
        {
            var json = "{\r\n  \"type\": \"string\",\r\n  \"title\": \"string\",\r\n  \"status\": 0,\r\n  " +
                "\"detail\": \"string\",\r\n  \"instance\": \"string\",\r\n  \"additionalProp1\": \"string\",\r\n  " +
                "\"additionalProp2\": \"string\",\r\n  \"additionalProp3\": \"string\"\r\n}";

            if (userSpecification.Id == null || userSpecification.Pseudo == null || id != userSpecification.Id)
            {
                Response.StatusCode = (int) System.Net.HttpStatusCode.BadRequest;
                return BadRequest();
            }

            var user = new UserSpecification(userSpecification.Id, userSpecification.Pseudo);
            users.Add(user);

            return user;
        }

        /*// DELETE api/<UsersController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }*/

        private static object NotFound(string message)
        {
            var statusCode = (int)System.Net.HttpStatusCode.NotFound;
            //var response = System.Web.HttpContext.Current.Response;
            //response.StatusCode = statusCode;
            //response.TrySkipIisCustomErrors = true; //<--
            return new
            {
                message = message
            };
        }
    }
}
