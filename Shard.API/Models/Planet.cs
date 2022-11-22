using Shard.Shared.Core;

namespace Shard.API.Models
{
    public class Planet
    {
        public string Name { get; set; }
        public int Size { get; set; }

        public IReadOnlyDictionary<ResourceKind, int> ResourceQuantity { get; set; }

        public Planet() { }
        public Planet(String name, int size, Dictionary<ResourceKind, int> resourceQuantity)
        {
            Name = name;
            Size = size;
            ResourceQuantity = resourceQuantity;
        }
    }
}
