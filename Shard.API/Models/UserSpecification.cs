using Bogus;
using Shard.Shared.Core;

namespace Shard.API.Models
{
    public class UserSpecification
    {
        public string? Id { get; set; }
        public string? Pseudo { get; set; }
        public DateTime DateOfCreation { get; }
        public Dictionary<string, int> ResourcesQuantity { get; set; }

        public static Dictionary<string, int> initiliazeResources()
        {
            Dictionary<string, int> resources = new Dictionary<string, int>()
            {
                { "aluminium", 0 },
                { "carbon", 20 },
                { "gold", 0 },
                { "iron", 10 },
                { "oxygen", 50 },
                { "titanium", 0 },
                { "water", 50 }
            };

            return resources;
        }

        internal UserSpecification()
        {
            Faker faker = new Faker();
            string firstName = faker.Name.FirstName();
            string lastName = faker.Name.LastName();
            Id = firstName + '.' + lastName;
            Pseudo = firstName;
            DateOfCreation = DateTime.Now;
            ResourcesQuantity = initiliazeResources();
        }

        public UserSpecification(string id, string pseudo)
        {
            Id = id;
            Pseudo = pseudo;
            DateOfCreation = DateTime.Now;
            ResourcesQuantity = initiliazeResources();
        }
    }
}
