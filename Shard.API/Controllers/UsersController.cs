using Bogus.DataSets;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shard.API.Models;
using Shard.Shared.Core;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Shard.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IReadOnlyList<UserSpecification> users;

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

        // GET /Users/id - Returns details of an existing user
        [HttpGet("{id}")]
        public object Get(string id)
        {
            var user = users.FirstOrDefault(user => user.Id == id);
            var json = "{\r\n  \"type\": \"string\",\r\n  \"title\": \"string\",\r\n  \"status\": 0,\r\n  \"detail\": \"string\",\r\n  \"instance\": \"string\",\r\n  \"additionalProp1\": \"string\",\r\n  \"additionalProp2\": \"string\",\r\n  \"additionalProp3\": \"string\"\r\n}";

            return user == null ? NotFound(json) : user;
            //return JsonConvert.DeserializeObject<NotFoundSpecification>(json);
        }

        /*// POST api/<UsersController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<UsersController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<UsersController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }*/
    }
}
