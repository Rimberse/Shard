namespace Shard.Shared.Core;

public class SystemWithoutPlanetResourcesSpecification
{
    public string Name { get; set;}
    public IReadOnlyList<PlanetWithoutResourcesSpecification> Planets { get; set; }

    public SystemWithoutPlanetResourcesSpecification(string name, IReadOnlyList<PlanetWithoutResourcesSpecification> planets)
    {
        Name = name;
        Planets = planets;
    }
}
