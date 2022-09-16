using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus.DataSets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shard.API.Models;
using Shard.API.Tools;
using Shard.Shared.Core;

namespace Shard.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SystemsController : ControllerBase
    {
        private readonly IReadOnlyList<SystemSpecification> systems;
        //private readonly ShardContext _context;
        //private static Models.System system1 = new Models.System()
        //{
        //    Name = "AD Leonis",
        //    Planets = PlanetGenerator.GeneratePlanets(5)
        //};

        //private static Models.System system2 = new Models.System()
        //{
        //    Name = "Luyten's Star",
        //    Planets = PlanetGenerator.GeneratePlanets(5)
        //};

        //List<Models.System> systems = new List<Models.System>() { system1, system2 };

        // Constructor is called during the runtime
        public SystemsController(SectorSpecification sectorSpecification)
        {
            systems = sectorSpecification.Systems;

            //_context = context;
            //_context.systems = _context.Set<Models.System>();

            //_context.Database.ExecuteSqlRaw("delete from MyTable");

            //_context.systems.RemoveRange(_context.systems);
            //_context.SaveChanges();

            //var set = _context.Set<Models.System>();
            //if (!set.Any())
            //{
            //prepopulateContext();
            //}
        }

        // Prepopulates the context objects with given Systems and randomly generated planets within them
        // Used to have a pre-loaded test date in order to be able to test the API endpoints
        public async void prepopulateContext()
        {
            //_context.systems.Add(system1);
            //_context.systems.Add(system2);
            //await _context.SaveChangesAsync();
        }

        // GET: /Systems - Fetches all Systems and their Planets
        [HttpGet]
        public IReadOnlyList<SystemSpecification> Getsystems()
        {
            //if (_context.systems == null)
            //{
            //    return NotFound();
            //}

            //  Console.WriteLine(_context);

            // return await _context.systems.ToListAsync();
            return systems;

            //return systems;
        }

        // GET: /Systems/Name - Fetches a single system and all it's planets
        [HttpGet("{systemName}")]
        public SystemSpecification GetSystem(string systemName)
        {
            //if (_context.systems == null)
            //{
            //    return NotFound();
            //}
            //  // var system = await _context.systems.FindAsync(systemName);
            //  var system = await _context.systems.Include(system => system.Planets).FirstOrDefaultAsync(system => system.Name == systemName);

            //  if (system == null)
            //  {
            //      return NotFound();
            //  }

            //  return system;

            // var system = systems.Find(system => system.Name == systemName);

            //if (system == null)
            //{
            //    return NotFound();
            //}

            var system = systems.FirstOrDefault(system => system.Name == systemName);

            return system;
        }

        // GET: /Systems/Name/Planets - Fetches all planets of a single system
        [HttpGet("{systemName}/planets")]
        public IReadOnlyList<PlanetSpecification> GetSystemPlanets(string systemName)
        {
            //var system = systems.FirstOrDefault(system => system.Name == systemName);

            //if (system == null ||system.Planets == null)
            //{
            //    return NotFound();
            //}

            var system = systems.FirstOrDefault(system => system.Name == systemName);

            return system.Planets;
        }

        // GET: /Systems/Name/Planets - Fetches a single planet of a system
        [HttpGet("{systemName}/planets/{planetName}")]
        public PlanetSpecification GetSystemPlanet(string systemName, string planetName)
        {
            //var system = systems.FirstOrDefault(system => system.Name == systemName);

            //if (system == null || system.Planets == null)
            //{
            //    return NotFound();
            //}

            //var planet = system.Planets.FirstOrDefault(planet => planet.Name == planetName);

            //if (planet == null)
            //{
            //    return NotFound();
            //}

            var system = systems.FirstOrDefault(system => system.Name == systemName);

            var planet = system.Planets.FirstOrDefault(planet => planet.Name == planetName);

            return planet;
        }

        // PUT: api/Systems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSystem(int id, Models.System system)
        {
            if (id != system.Id)
            {
                return BadRequest();
            }

            // _context.Entry(system).State = EntityState.Modified;

            try
            {
                // await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                //if (!SystemExists(id))
                //{
                //    return NotFound();
                //}
                //else
                //{
                //    throw;
                //}
            }

            return NoContent();
        }

        // POST: /Systems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Models.System>> PostSystem(Models.System system)
        {
          // if (_context.systems == null)
          // {
          //    return Problem("Entity set 'ShardContext.systems'  is null.");
          //}
            // _context.systems.Add(system);
            // await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetSystem), new { id = system.Id }, system);
        }

        // DELETE: api/Systems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSystem(int id)
        {
            //if (_context.systems == null)
            //{
            //    return NotFound();
            //}
            //var system = await _context.systems.FindAsync(id);
            //if (system == null)
            //{
            //    return NotFound();
            //}

            //_context.systems.Remove(system);
            //await _context.SaveChangesAsync();

            return NoContent();
        }

        //private bool SystemExists(int id)
        //{
        //    return (_context.systems?.Any(e => e.Id == id)).GetValueOrDefault();
        //}
    }
}
