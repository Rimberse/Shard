using Shard.API.Models;
using Shard.Shared.Core;
using System.Collections;

namespace Shard.API.Tools
{
    public class DependencyInjector
    {
        public SectorSpecification sectorSpecification { get; set; }
        public List<UserSpecification> users { get; set; }
        public Hashtable units { get; set; }

        public DependencyInjector()
        {
            // Generate SectorSpecification
            sectorSpecification = MapGenerator.Random.Generate();

            // Create users list
            users = new List<UserSpecification>();

            // Create units hashtable
            units = new Hashtable();

            // Generate users, units & locations
            foreach (var system in sectorSpecification.Systems)
            {
                var user = new UserSpecification();
                users.Add(user);
                var userUnits = new List<UnitSpecification>();

                foreach (var planet in system.Planets)
                {
                    var unit = new UnitSpecification(RandomIdGenerator.RandomString(10), "scout", system.Name, planet.Name, system.Name, planet.Name);
                    userUnits.Add(unit);
                }

                units.Add(user, userUnits);
                userUnits = new List<UnitSpecification>();
            }
        }
    }
}
