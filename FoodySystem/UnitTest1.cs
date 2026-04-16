using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;
using FoodySystem.Models;
using System.Net.NetworkInformation;

namespace FoodySystem

{
    public class Tests
    {
        private RestClient _client;
        private static string url = "http://144.91.123.158:81";
        private string lastCreatedId;
        private string userName = "kriso_91";
        private string password = "12345678";
        private string accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiIyZmY5MmE1NS1jYjA1LTQyNmEtOTdkYS01ZTQ1MmY5MGY5NzAiLCJpYXQiOiIwNC8xNi8yMDI2IDA3OjM1OjAwIiwiVXNlcklkIjoiNWVkN2I4NWQtNzBmYS00Nzk5LWFlNjEtMDhkZTY4OGM0NGJjIiwiRW1haWwiOiJhc2RAYXNkLmZkIiwiVXNlck5hbWUiOiJrcmlzb185MSIsImV4cCI6MTc3NjM0NjUwMCwiaXNzIjoiRm9vZHlfQXBwX1NvZnRVbmkiLCJhdWQiOiJGb29keV9XZWJBUElfU29mdFVuaSJ9.wVWVuLYxR3IP5Q-bt8jmG20QFIFoUMfXpEMdjii1F7s";
        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;
            if (string.IsNullOrEmpty(accessToken))
            {
                jwtToken = accessToken;
            }
            else
            {
                jwtToken = GetJwtToken(userName, password);
            }
            var options = new RestClientOptions(url)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };
            this._client = new RestClient(options);
        }
        private string GetJwtToken(string userName, string password)
        {
            var tempClient = new RestClient(url);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { userName, password });
            var response = tempClient.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();
                if (string.IsNullOrEmpty(token))
                {
                    throw new Exception("token not found");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
            }
        }
        [Order(0)]
        [Test]
        public void CreateFood_WithValidData_ShouldReturnSuccess()
        {
            var dataRequest = new FoodDTO
            {
                Name = "test1223",
                Description = "Description 12345",
                Url = ""
            };
            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(dataRequest);
            var response = this._client.Execute(request);
            var createResponse = JsonSerializer.Deserialize<ApiRespomseDTO>(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(createResponse, Is.Not.Null);
            
            lastCreatedId = createResponse.FoodId;
        }
        [Order(1)]
        [Test]
        public void EditFood_WithValidData_ShouldReturnSuccess()
        {
            RestRequest request = new RestRequest($"/api/Food/Edit/{lastCreatedId}", Method.Patch);
            request.AddBody(new[]
            {
                new
                {
                    path = "/name",
                    op = "replace",
                    value = "Chicken Soup"
                }
            });

            RestResponse response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            //response.Content = {msg: "Successfully edited", foodId: "34"}
            ApiRespomseDTO readyResponse = JsonSerializer.Deserialize<ApiRespomseDTO>(response.Content);
            //readyResponse
            //Msg = "Successfully edited"
            //FoodId = "34"
            Assert.That(readyResponse.Msg, Is.EqualTo("Successfully edited"));
        }
        [Order(2)]
        [Test]
        public void GetAllFood_ShouldReturnNonEmptyArray()
        {
            var request = new RestRequest("/api/Food/All", Method.Get);
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            List<FoodDTO> readyResponse = JsonSerializer.Deserialize<List<FoodDTO>>(response.Content);
            Assert.That(readyResponse, Is.Not.Null);
            Assert.That(readyResponse, Is.Not.Empty);
            Assert.That(readyResponse.Count, Is.GreaterThanOrEqualTo(1));
        }
        [Order(3)]
        [Test]
        public void DeleteFood_ShouldReturnSuccess()
        {
            var request = new RestRequest($"/api/Food/Delete/{lastCreatedId}", Method.Delete);
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var deletedResponse = JsonSerializer.Deserialize<ApiRespomseDTO>(response.Content);
            Assert.That(deletedResponse.Msg, Is.EqualTo($"Deleted successfully!"));
        }
        [Order(4)]
        [Test]
        public void CreateFood_WithInvalidData_ShouldReturnError()
        {
            var dataRequest = new FoodDTO
            {
                Name = "",
                Description = ""
            };
            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddBody(dataRequest);
            var response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(5)]
        [Test]
        public void EditFood_WithInvalidData_ShouldReturnError()
        {
            RestRequest request = new RestRequest($"/api/Food/Edit/{lastCreatedId+1}", Method.Patch);
            request.AddBody(new[]
            {
                new
                {
                    path = "/name",
                    op = "replace",
                    value = "Chicken Soup"
                }
            });

            RestResponse response = _client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

           
            ApiRespomseDTO readyResponse = JsonSerializer.Deserialize<ApiRespomseDTO>(response.Content);
            
            Assert.That(readyResponse.Msg, Is.EqualTo("No food revues..."));
        }
        [Order(6)]
        [Test]
        public void DeleteFood_WithInvalidId_ShouldReturnError()
        {
            {
                var request = new RestRequest($"/api/Food/Delete/{lastCreatedId+1}", Method.Delete);
                var response = _client.Execute(request);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                var deletedResponse = JsonSerializer.Deserialize<ApiRespomseDTO>(response.Content);
                Assert.That(deletedResponse.Msg, Is.EqualTo("Unable to delete this food revue!"));
            }
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            this._client.Dispose();
        }
    }
}