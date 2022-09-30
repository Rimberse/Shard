using Bogus;
using System;

namespace Shard.API.Models
{
    public class UnitSpecification
    {
        public string? Id { get; set; }
        public string? Type { get; set; }
        public string? System { get; set; }
        public string? Planet { get; set; }

        internal UnitSpecification()
        {
            Random random = new Random();
            Id = "9cc8f0cc-5b4c-897d-b60c-398bfb9700a6";
            Type = "scout";
            System = "Andromeda";
            Planet = "Episcophe";
        }

        public UnitSpecification(string id, string type, string system, string planet)
        {
            Random random = new Random();
            Id = id;
            Type = type;
            System = system;
            Planet = planet;
        }
    }
}
