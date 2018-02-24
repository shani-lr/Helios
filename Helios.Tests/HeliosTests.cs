using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Helios.Tests
{
    [TestClass]
    public class HeliosTests
    {
        private HttpClient _sunClient;
        private TokenClient _heliosTokenClient;

        [TestInitialize]
        public async Task Initialize()
        {
            var sunServer = new TestServer(new WebHostBuilder()
                .UseStartup<Sun.Startup>());

            _sunClient = sunServer.CreateClient();

            var heliosServer = new TestServer(new WebHostBuilder()
                .UseStartup<Helios.Startup>());
            var handler = heliosServer.CreateHandler();
            
            var discoveryResponse = await new DiscoveryClient("http://localhost", handler).GetAsync();
            _heliosTokenClient = new TokenClient(discoveryResponse.TokenEndpoint, "sun", "secret");
        }
        
        [TestMethod]
        public async Task GivenServiceUsesHelios_WhenSendingARequestToServiceWithoutToken_ThenReturnsUnauthorized()
        {
            // act
            var response = await _sunClient.GetAsync("/api/sun");
            
            // assert
            response.IsSuccessStatusCode.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        
        [TestMethod]
        public async Task GivenServiceUsesHelios_WhenSendingARequestToServiceWithToken_ThenReturnsOk()
        {
            // arrange
            var token = await _heliosTokenClient.RequestClientCredentialsAsync();
            _sunClient.SetBearerToken(token.AccessToken);
            
            // act
            var response = await _sunClient.GetAsync("/api/sun");
            
            // assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadAsStringAsync();
            result.Should().Contain("You touched the sun");
        }     
    }
}