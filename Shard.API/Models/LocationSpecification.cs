using Shard.Shared.Core;

namespace Shard.API.Models
{
    public class LocationSpecification
    {
        public string? System { get; set; }
        public string? Planet { get; set; }
        public IReadOnlyDictionary<string, int>? ResourcesQuantity { get; set; }

        public LocationSpecification(string? system, string? planet)
        {
            System = system;
            Planet = planet;
        }

        public LocationSpecification(string? system, string? planet, IReadOnlyDictionary<ResourceKind, int> resourcesQuantity)
        {
            System = system;
            Planet = planet;
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            foreach (KeyValuePair<ResourceKind, int> entry in resourcesQuantity)
            {
                dictionary.Add(entry.Key.ToString().ToLower(), entry.Value);
            }
            ResourcesQuantity = dictionary;
        }
    }
}
