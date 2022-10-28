using Bogus;
using Shard.Shared.Core;
using System;

namespace Shard.API.Models
{
    public class UnitSpecification
    {
        public string? Id { get; set; }
        public string? Type { get; set; }
        public string System { get; set; }
        public string? Planet { get; set; }
        public string DestinationSystem { get; set; }
        public string? DestinationPlanet { get; set; }
        public Task? runningTask { get; set; }
        public long? taskWaitTime { get; set; }

        internal UnitSpecification()
        {
            Id = "9cc8f0cc-5b4c-897d-b60c-398bfb9700a6";
            Type = "scout";
            System = "Andromeda";
            Planet = "Episcophe";
            DestinationSystem = System;
            DestinationPlanet = Planet;
        }

        public UnitSpecification(string id, string type, string system, string planet, string destinationSystem, string destinationPlanet)
        {
            Id = id;
            Type = type;
            System = system;
            Planet = planet;
            DestinationSystem = destinationSystem;
            DestinationPlanet = destinationPlanet;
        }
    }
}
