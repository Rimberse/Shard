using Shard.Shared.Core;

namespace Shard.API.Models
{
    public class Sector
    {
        public IReadOnlyList<Systems> Systems { get; }

        public Sector(List<Systems> systems)
        {
            Systems = systems;
        }
    }
}
