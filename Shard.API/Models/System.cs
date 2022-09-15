namespace Shard.API.Models
{
    public class System
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<Planet>? Planets { get; set; }
    }
}
