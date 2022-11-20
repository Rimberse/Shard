namespace Shard.API.Models
{
    public class Building
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string System { get; set; }
        public string Planet { get; set; }
        public string ResourceCategory { get; set; }
        public Boolean IsBuilt { get; set; }
        public DateTime? EstimatedBuildTime { get; set; }
        public UnitSpecification UnitUsed { get; set; }
        //public CancellationTokenSource cancellationToken { get; set; }
        public Task ConstructionTask { get; set; } = null!;

        public Building(string id, string type, string system, string planet, string resourceCategory, DateTime? estimatedBuildTime, UnitSpecification unitUsed)
        {
            Id = id;
            Type = type;
            System = system;
            Planet = planet;
            ResourceCategory = resourceCategory;
            IsBuilt = false;
            EstimatedBuildTime = estimatedBuildTime;
            UnitUsed = unitUsed;
            ConstructionTask = null;
            //cancellationToken = new CancellationTokenSource();
        }
    }
}
