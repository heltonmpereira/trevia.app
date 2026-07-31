namespace TreviaApp.IntegrationTests.Utilities;
using Xunit;

[CollectionDefinition("Auth Integration Tests")]
public class AuthTestCollection : ICollectionFixture<TestWebApplicationFactory>
{
}
