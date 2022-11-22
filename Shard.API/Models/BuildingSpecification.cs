using Shard.Shared.Core;
using System.Security.Cryptography.Xml;

namespace Shard.API.Models
{
    public class BuildingSpecification
    {
        public string? Id { get; set; }
        public string Type { get; set; }
        public string BuilderId { get; set; }
        public string? System { get; set; }
        public string? Planet { get; set; }
        public string ResourceCategory { get; set; }
        public Dictionary<String, ResourceKind[]> Resources { get; }

        public BuildingSpecification(string id, string builderId, string system, string planet, string resourceCategory)
        {
            Id = id;
            Type = "mine";
            BuilderId = builderId;
            System = system;
            Planet = planet;
            ResourceCategory = resourceCategory;

            Resources = new Dictionary<String, ResourceKind[]>();

            Resources["solid"] = new ResourceKind[] { ResourceKind.Carbon, ResourceKind.Iron, ResourceKind.Gold, ResourceKind.Aluminium, ResourceKind.Titanium };
            Resources["liquid"] = new ResourceKind[] { ResourceKind.Water };
            Resources["gaseous"] = new ResourceKind[] { ResourceKind.Oxygen };
        }
    }
}
