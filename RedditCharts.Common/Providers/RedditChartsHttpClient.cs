using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RedditCharts.Common.Models;
using System.Data;
using System.Text.Json;

namespace RedditCharts.Common.Providers
{
    public interface IRedditChartsHttpClient
    {
        Task<IEnumerable<RedditPostModel>> GetNewPostsAsync(string subreddit, CancellationToken cancellationToken);
    }

    /// <summary>
    /// HttpClient provider for querying posts from reddit
    /// </summary>
    public class RedditChartsHttpClient : IRedditChartsHttpClient
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<RedditChartsHttpClient> _logger;
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, string> _lastPostIds;


        public RedditChartsHttpClient(IHttpClientFactory clientFactory, ILogger<RedditChartsHttpClient> logger, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _configuration = configuration;
            _lastPostIds = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets the new post from the subredit
        /// </summary>
        /// <param name="subreddit"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IEnumerable<RedditPostModel>?> GetNewPostsAsync(string subreddit, CancellationToken cancellationToken)
        {
            try
            {
                var client = _clientFactory.CreateClient();

                var url = _configuration.GetSection("Reddit:Url");
                var postSize = _configuration.GetSection("Reddit:PostSize");
                var userAgent = _configuration.GetSection("Reddit:UserAgent");

                client.DefaultRequestHeaders.Add("User-Agent", userAgent.Value);

                var response = await client.GetAsync($"{url.Value}{subreddit}/new.json?limit={postSize.Value}", cancellationToken);

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                var redditResponse = JsonSerializer.Deserialize<RedditResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (redditResponse != null)
                {
                    return redditResponse?.Data.children.Select(c => c.Data).ToList();
                }
                return null;

            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, $"Error fetching posts from r/{subreddit} with error code; {e.StatusCode}");
                return Enumerable.Empty<RedditPostModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching posts from r/{subreddit}");
                return Enumerable.Empty<RedditPostModel>();
            }
        }
    }
}
