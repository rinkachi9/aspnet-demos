using System.Net;
using FluentAssertions;
using PactNet;
using PactNet.Output.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace AdvancedTesting.Contract;

public class UserApiConsumerTest
{
    private readonly IPactBuilderV4 _pactBuilder;

    public UserApiConsumerTest(ITestOutputHelper output)
    {
        var config = new PactConfig
        {
            PactDir = "../../../pacts",
            Outputters = new[] { new XunitOutput(output) },
            LogLevel = PactNet.PactLogLevel.Information
        };

        // "ConsumerApp" is consuming "UserApi"
        var pact = Pact.V4("ConsumerApp", "UserApi", config);
        _pactBuilder = pact.WithHttpInteractions();
    }

    [Fact]
    public async Task CreateUser_WhenCalledWithValidData_ReturnsCreatedUser()
    {
        // Define the contract
        _pactBuilder
            .UponReceiving("A valid request to create a user")
                .Given("There are no users")
                .WithRequest(HttpMethod.Post, "/api/v1/users")
                .WithHeader("Content-Type", "application/json")
                .WithJsonBody(new { fullName = "John Doe", email = "john@example.com", age = 30 }) // Matches CreateUserRequest
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(new { id = 1, name = "John Doe", version = "v1" }); // Matches UsersController v1 Response

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Act: Simulate Client call
            var client = new HttpClient { BaseAddress = ctx.MockServerUri };
            var response = await client.PostAsJsonAsync("/api/v1/users", new { FullName = "John Doe", Email = "john@example.com", Age = 30 });
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("John Doe");
        });
    }
}
