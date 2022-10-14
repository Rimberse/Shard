using Microsoft.AspNetCore.Mvc;
using Shard.API.Models;
using Shard.API.Tools;
using Shard.Shared.Core;
using System.Collections;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Shard.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BuildingsController : ControllerBase
    {
        private readonly SectorSpecification sector;
        private readonly List<UserSpecification> users;
        private readonly Hashtable units;
        private readonly List<BuildingSpecification> buildings;
        public BuildingsController(DependencyInjector dependencyInjector)
        {
            sector = dependencyInjector.sectorSpecification;
            users = dependencyInjector.users;
            units = dependencyInjector.units;
        }

        // POST /users/{userId}/Buildings
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }
    }
}
