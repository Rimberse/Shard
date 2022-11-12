using NuGet.Protocol.Plugins;
using Shard.Shared.Core;

namespace Shard.API.Models
{
    public class Systems
    {
        public string Name { get; set; }
        public IReadOnlyList<Planet> Planets { get; set; }

        public Systems() { }

        public Systems(string name, List<Planet> planets)
        {
            Name = name;
            Planets = planets;
        }
    }
}
