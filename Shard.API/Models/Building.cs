namespace Shard.API.Models
{
    public class Building
    {
        public string? Id { get; set; }
        public string? Type { get; set; }
        public string? System { get; set; }
        public string? Planet { get; set; }

        public Building(string id, string type, string system, string planet)
        {
            Id = id;
            Type = type;
            System = system;
            Planet = planet;
        }
    }
}
