using Bogus;
using Shard.Shared.Core;

namespace Shard.API.Models
{
    public class UserSpecification
    {
        public string? Id { get; set; }
        public string? Pseudo { get; set; }
        public DateTime DateOfCreation { get; }
        
        internal UserSpecification()
        {
            Faker faker = new Faker();
            string firstName = faker.Name.FirstName();
            string lastName = faker.Name.LastName();
            Id = firstName + '.' + lastName;
            Pseudo = firstName;
            DateOfCreation = DateTime.Now;
        }

       /* internal UserSpecification(string firstName, string lastName)
        {
            UserSpecification userSpecification = new();
            userSpecification.Id = firstName + '.' + lastName;
        }*/
    }
}
