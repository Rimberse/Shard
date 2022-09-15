using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shard.API.Models;
using Shard.API.Tools;

namespace Shard.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SystemsController : ControllerBase
    {
        private readonly ShardContext _context;

        // Constructor is called during the runtime
        public SystemsController(ShardContext context)
        {
            _context = context;
            prepopulateContext();
        }

        // Prepopulates the context objects with given Systems and randomly generated planets within them
        // Used to have a pre-loaded test date in order to be able to test the API endpoints
        public async void prepopulateContext()
        {
            _context.systems.Add(new Models.System()
            {
                Name = "AD Leonis",
                Planets = PlanetGenerator.GeneratePlanets(5)
            });

            _context.systems.Add(new Models.System()
            {
                Name = "Luyten's Star",
                Planets = PlanetGenerator.GeneratePlanets(5)
            });

            await _context.SaveChangesAsync();
        }

        // GET: Systems - Fetches all Systems and their Planets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Models.System>>> Getsystems()
        {
          if (_context.systems == null)
          {
              return NotFound();
          }

            Console.WriteLine(_context);

            return await _context.systems.ToListAsync();
        }

        // GET: api/Systems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Models.System>> GetSystem(int id)
        {
          if (_context.systems == null)
          {
              return NotFound();
          }
            var system = await _context.systems.FindAsync(id);

            if (system == null)
            {
                return NotFound();
            }

            return system;
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

            _context.Entry(system).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SystemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Systems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Models.System>> PostSystem(Models.System system)
        {
          if (_context.systems == null)
          {
              return Problem("Entity set 'ShardContext.systems'  is null.");
          }
            _context.systems.Add(system);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetSystem), new { id = system.Id }, system);
        }

        // DELETE: api/Systems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSystem(int id)
        {
            if (_context.systems == null)
            {
                return NotFound();
            }
            var system = await _context.systems.FindAsync(id);
            if (system == null)
            {
                return NotFound();
            }

            _context.systems.Remove(system);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SystemExists(int id)
        {
            return (_context.systems?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
