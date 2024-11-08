using Moq;
using Microsoft.Extensions.Logging;
using RedditCharts.Common.Providers;
using RedditCharts.Common.Models;
using System.Text;

namespace RedditCharts.Tests
{
    [TestFixture]
    public class RedditChartsPostProcessorTests
    {
        private Mock<ILogger<RedditChartsPostProcessor>> _mockLogger;
        private RedditChartsPostProcessor _processor;
        private string _jsonstringData = string.Empty;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<RedditChartsPostProcessor>>();
            _processor = new RedditChartsPostProcessor(_mockLogger.Object);

            string jsonFilePath = "./TestData.json";
            _jsonstringData = File.ReadAllText(jsonFilePath);
            StringContent stringContent = new StringContent(_jsonstringData, Encoding.UTF8, "application/json");
        }

        [Test]
        public async Task ProcessPostAsync_NewPost_UpdatesUserPostCountAndUpVoteCount()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RedditChartsPostProcessor>>();
            var processor = new RedditChartsPostProcessor(loggerMock.Object);

            var posts = new List<RedditPostModel>
            {
                new RedditPostModel { id = "1", author = "user1", ups = 100 },
                new RedditPostModel { id = "2", author = "user2", ups = 50 },
                new RedditPostModel { id = "3", author = "user1", ups = 75 }
            };

            // Act
            await processor.ProcessPostAsync(posts, CancellationToken.None);

            // Assert
            Assert.AreEqual(2, processor.UserPostCount["user1"]);
            Assert.AreEqual(1, processor.UserPostCount["user2"]);
            Assert.AreEqual(100, processor.PostsUpVoteCount["1"]);
            Assert.AreEqual(50, processor.PostsUpVoteCount["2"]);
            Assert.AreEqual(75, processor.PostsUpVoteCount["3"]);
        }

        [Test]
        public async Task ProcessPostAsync_DuplicatePost_DoesNotUpdateUserPostCountOrUpVoteCount()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<RedditChartsPostProcessor>>();
            var processor = new RedditChartsPostProcessor(loggerMock.Object);

            var posts = new List<RedditPostModel>
            {
                new RedditPostModel { id = "1", author = "user1", ups = 100 },
                new RedditPostModel { id = "1", author = "user1", ups = 100 },
                new RedditPostModel { id = "2", author = "user2", ups = 50 }
            };

            // Act
            await processor.ProcessPostAsync(posts, CancellationToken.None);

            // Assert
            Assert.AreEqual(1, processor.UserPostCount["user1"]);
            Assert.AreEqual(1, processor.UserPostCount["user2"]);
            Assert.AreEqual(100, processor.PostsUpVoteCount["1"]);
            Assert.AreEqual(50, processor.PostsUpVoteCount["2"]);
        }
    }
}
