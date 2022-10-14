using Microsoft.AspNetCore.Mvc.Testing;
using Shard.API;
using Shard.API.Tools;
using Shard.Shared.Web.IntegrationTests;
using Shard.Shared.Web.IntegrationTests.Asserts;
using Shard.Shared.Web.IntegrationTests.TestEntities;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Shard.IntegrationTests;

public class IntegrationTests : BaseIntegrationTests<Program>
{
    public IntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper) : base(factory, testOutputHelper)
    {
        [Fact]
        async Task GenerateDependencyInjection()
        {
            DependencyInjector di = new DependencyInjector();
            Assert.NotNull(di.sectorSpecification);
            Assert.NotNull(di.users);
            Assert.NotNull(di.units);
        }
    }
}
