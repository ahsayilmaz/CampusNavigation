using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CampusNavigation.Tests
{
    public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ApiTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_ApiEndpoint_ReturnsSuccess()
        {
            // Arrange
            var requestUri = "/api/your-endpoint"; // Replace with your actual API endpoint

            // Act
            var response = await _client.GetAsync(requestUri);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_ApiEndpoint_ReturnsCreated()
        {
            // Arrange
            var requestUri = "/api/your-endpoint"; // Replace with your actual API endpoint
            var content = new StringContent("{\"name\":\"test\"}", Encoding.UTF8, "application/json"); // Adjust the content as needed

            // Act
            var response = await _client.PostAsync(requestUri, content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // Add more tests for other CRUD operations as needed
    }
}