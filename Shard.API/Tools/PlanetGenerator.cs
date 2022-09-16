using System;
using Bogus;
using Shard.API.Models;
using System.Collections.Generic;


namespace Shard.API.Tools
{
    public class PlanetGenerator
    {
        private static List<Planet>? Planets;

        public static List<Planet> GeneratePlanets(int count)
        {
            if (Planets == null)
            {
                Planets = new Faker<Planet>()
                .RuleFor(planet => planet.Name, take => take.Name.FirstName())
                .RuleFor(planet => planet.Size, take => take.Random.Int(1000, 100000))
                .Generate(count);
            }

            return Planets;
        }
    }
}
