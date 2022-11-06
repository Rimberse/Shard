using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus.DataSets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shard.API.Tools;
using Shard.Shared.Core;

namespace Shard.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SystemsController : ControllerBase
    {
        private readonly IReadOnlyList<SystemSpecification> systems;
        private readonly IReadOnlyList<SystemWithoutPlanetResourcesSpecification> systemsWithoutPlanetResources;

        public SystemsController(SectorSpecification sectorSpecification)
        {
            systems = sectorSpecification.Systems;

            List<SystemWithoutPlanetResourcesSpecification> systemsWithoutPlanetResourcesList = new List<SystemWithoutPlanetResourcesSpecification>();

            foreach (SystemSpecification system in systems)
            {
                SystemWithoutPlanetResourcesSpecification systemWithoutPlanetResources = new SystemWithoutPlanetResourcesSpecification(system.Name, new List<PlanetWithoutResourcesSpecification>());
                List<PlanetWithoutResourcesSpecification> planetsWithoutResources = new List<PlanetWithoutResourcesSpecification>();

                foreach (PlanetSpecification planet in system.Planets)
                {
                    planetsWithoutResources.Add(new PlanetWithoutResourcesSpecification(planet.Name, planet.Size));
                }

                systemWithoutPlanetResources.Planets = planetsWithoutResources;
                systemsWithoutPlanetResourcesList.Add(systemWithoutPlanetResources);
                systemsWithoutPlanetResources = systemsWithoutPlanetResourcesList;
            }
        }

        // GET: /Systems - Fetches all Systems and their Planets
        [HttpGet]
        public IReadOnlyList<SystemWithoutPlanetResourcesSpecification> Getsystems()
        {
            return systemsWithoutPlanetResources;
        }

        // GET: /Systems/Name - Fetches a single system and all it's planets
        [HttpGet("{systemName}")]
        public SystemWithoutPlanetResourcesSpecification GetSystem(string systemName)
        {
            var system = systemsWithoutPlanetResources.FirstOrDefault(system => system.Name == systemName);

            return system;
        }

        // GET: /Systems/Name/Planets - Fetches all planets of a single system
        [HttpGet("{systemName}/planets")]
        public IReadOnlyList<PlanetWithoutResourcesSpecification> GetSystemPlanets(string systemName)
        {
            var system = systemsWithoutPlanetResources.FirstOrDefault(system => system.Name == systemName);

            return system.Planets;
        }

        // GET: /Systems/Name/Planets - Fetches a single planet of a system
        [HttpGet("{systemName}/planets/{planetName}")]
        public PlanetWithoutResourcesSpecification GetSystemPlanet(string systemName, string planetName)
        {
            var system = systemsWithoutPlanetResources.FirstOrDefault(system => system.Name == systemName);

            var planet = system.Planets.FirstOrDefault(planet => planet.Name == planetName);

            return planet;
        }
    }
}
