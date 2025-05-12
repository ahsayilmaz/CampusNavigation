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
            var requestUri = "/api/your-endpoint"; 
            var response = await _client.GetAsync(requestUri);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_ApiEndpoint_ReturnsCreated()
        {
            var requestUri = "/api/your-endpoint"; 
            var content = new StringContent("{\"name\":\"test\"}", Encoding.UTF8, "application/json"); 
            var response = await _client.PostAsync(requestUri, content);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }
    }
}