using Microsoft.Extensions.Logging;
using RedditCharts.Common.Models;

namespace RedditCharts.Common.Providers
{
    public interface IRedditChartsPostProcessor
    {
        Task ProcessPostAsync(IEnumerable<RedditPostModel> posts, CancellationToken stoppingToken);
    }

    public class RedditChartsPostProcessor : IRedditChartsPostProcessor
    {
        private readonly ILogger<RedditChartsPostProcessor> _logger;

        private readonly HashSet<string> _processedPostIds;
        private readonly Dictionary<string, int> _userPostCount;
        private readonly Dictionary<string, int> _postsUpVoteCount;

        public RedditChartsPostProcessor(ILogger<RedditChartsPostProcessor> logger)
        {
            _logger = logger;
            _processedPostIds = new HashSet<string>();
            _userPostCount = new Dictionary<string, int>();
            _postsUpVoteCount = new Dictionary<string, int>();
        }

        public async Task ProcessPostAsync(IEnumerable<RedditPostModel> posts, CancellationToken stoppingToken)
        {
            foreach (var post in posts)
            {
                if (!_processedPostIds.Contains(post.id))
                {
                    _logger.LogInformation("New post in r/{Subreddit}: {Title} by u/{Author}",
                     post.category,
                     post.name,
                     post.author);

                    //update processed list
                    _processedPostIds.Add(post.id);

                    //post count
                    if (_userPostCount.ContainsKey(post.author))
                    {
                        _userPostCount[post.author]++;
                    }
                    else
                    {
                        _userPostCount[post.author] = 1;
                    }
                    //vote count
                    _postsUpVoteCount.Add(post.id, post.ups);
                }
            }
            _logger.LogInformation("\n---- Users with most posts ---");

            var topTenUsers = _userPostCount.OrderByDescending(x => x.Value).Take(5).ToList();

            foreach (var user in topTenUsers)
            {
                _logger.LogInformation($"{user.Key} : {user.Value} posts(s)");
            }

            _logger.LogInformation("\n---- Posts with most up votes ---");

            var topUpVotes = _postsUpVoteCount.OrderByDescending(x => x.Value).Take(5).ToList();

            foreach (var votes in topUpVotes)
            {
                _logger.LogInformation($"{votes.Key} : {votes.Value} votes(s)");
            }

            await Task.Delay(100, stoppingToken);
        }
    }
}
