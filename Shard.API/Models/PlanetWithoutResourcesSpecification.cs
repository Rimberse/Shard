using Microsoft.Build.Evaluation;
using Shard.API.Tools;

namespace Shard.Shared.Core;

public class PlanetWithoutResourcesSpecification
{
    public string Name { get; }
    public int Size { get; set; }

    public PlanetWithoutResourcesSpecification(string name, int size)
    {
        Name = name;
        Size = size;
    }
}
