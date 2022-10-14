using Bogus;
using Shard.Shared.Core;
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
            Id = "9cc8f0cc-5b4c-897d-b60c-398bfb9700a6";
            Type = "scout";
            System = "Andromeda";
            Planet = "Episcophe";
        }

        public UnitSpecification(string id, string type, string system, string planet)
        {
            Id = id;
            Type = "scout";
            System = system;
            Planet = planet;
        }
    }
}
