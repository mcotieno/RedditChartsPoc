using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RedditCharts.Common.Providers;
using System.Text;
using Moq.Protected;

namespace RedditCharts.Tests
{
    public class RedditChartsHttpClientTests
    {
        private string _jsonstringData = string.Empty;

        [SetUp]
        public void Setup()
        {
            _jsonstringData = File.ReadAllText("./TestData.json");
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public async Task GetNewPostsAsync_ReturnsNewPosts()
        {

            //var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            //{
            //    Content = new StringContent(_jsonstringData, Encoding.UTF8, "application/json")
            //};


            // Arrange
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var httpMessageHandler = new Mock<HttpMessageHandler>();
            // Setup Protected method on HttpMessageHandler mock.
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                     "SendAsync",
               ItExpr.Is<HttpRequestMessage>(req =>
                   req.Method == HttpMethod.Get),
               ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
                {
                    HttpResponseMessage response = new HttpResponseMessage();

                    response.Content = new StringContent(_jsonstringData, Encoding.UTF8, "application/json");

                    return response;
                });

            var httpClient = new HttpClient(httpMessageHandler.Object);
            httpClient.BaseAddress = new Uri("https://example.com");

            httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var loggerMock = new Mock<ILogger<RedditChartsHttpClient>>();
            var configurationMock = new Mock<IConfiguration>();

            configurationMock.Setup(c => c.GetSection("Reddit:Url")).Returns(new Mock<IConfigurationSection>().Object);
            configurationMock.Setup(c => c.GetSection("Reddit:PostSize")).Returns(new Mock<IConfigurationSection>().Object);
            configurationMock.Setup(c => c.GetSection("Reddit:UserAgent")).Returns(new Mock<IConfigurationSection>().Object);

            var redditClient = new RedditChartsHttpClient(httpClientFactoryMock.Object, loggerMock.Object, configurationMock.Object);

            // Act
            var posts = await redditClient.GetNewPostsAsync("testsubreddit", CancellationToken.None);

            // Assert
            Assert.IsNotNull(posts);
            Assert.AreEqual(2, posts.Count());
            Assert.AreEqual("1gmml3o", posts.ToArray()[0].id);
            Assert.AreEqual(100, posts.ToArray()[0].ups);
            Assert.AreEqual("1gmcwk1", posts.ToArray()[1].id);
            Assert.AreEqual(50, posts.ToArray()[1].ups);
        }
        
    }
}