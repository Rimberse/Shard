using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus.DataSets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shard.Shared.Core;

namespace Shard.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SystemsController : ControllerBase
    {
        private readonly IReadOnlyList<SystemSpecification> systems;
        
        public SystemsController(SectorSpecification sectorSpecification)
        {
            systems = sectorSpecification.Systems;
        }

        // GET: /Systems - Fetches all Systems and their Planets
        [HttpGet]
        public IReadOnlyList<SystemSpecification> Getsystems()
        {
            return systems;
        }

        // GET: /Systems/Name - Fetches a single system and all it's planets
        [HttpGet("{systemName}")]
        public SystemSpecification GetSystem(string systemName)
        {
            var system = systems.FirstOrDefault(system => system.Name == systemName);

            return system;
        }

        // GET: /Systems/Name/Planets - Fetches all planets of a single system
        [HttpGet("{systemName}/planets")]
        public IReadOnlyList<PlanetSpecification> GetSystemPlanets(string systemName)
        {
            var system = systems.FirstOrDefault(system => system.Name == systemName);

            return system.Planets;
        }

        // GET: /Systems/Name/Planets - Fetches a single planet of a system
        [HttpGet("{systemName}/planets/{planetName}")]
        public PlanetSpecification GetSystemPlanet(string systemName, string planetName)
        {
            var system = systems.FirstOrDefault(system => system.Name == systemName);

            var planet = system.Planets.FirstOrDefault(planet => planet.Name == planetName);

            return planet;
        }
    }
}
