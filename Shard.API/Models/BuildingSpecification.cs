using Shard.Shared.Core;

namespace Shard.API.Models
{
    public class BuildingSpecification
    {
        public string? Id { get; set; }
        public string? Type { get; set; }
        public string? BuilderId { get; set; }
        public string? System { get; set; }
        public string? Planet { get; set; }

        public BuildingSpecification(string id, string builderId, string system, string planet)
        {
            Id = id;
            Type = "mine";
            BuilderId = builderId;
            System = system;
            Planet = planet;
        }
    }
}
