using Shard.Shared.Core;

namespace Shard.API.Models
{
    public class LocationSpecification
    {
        public string? System { get; set; }
        public string? Planet { get; set; }
        public IReadOnlyDictionary<ResourceKind, int> ResourcesQuantity { get; set; }

        public LocationSpecification(string? system, string? planet, IReadOnlyDictionary<ResourceKind, int> resourcesQuantity)
        {
            System = system;
            Planet = planet;
            ResourcesQuantity = resourcesQuantity;
        }
    }
}
