namespace RedditCharts.Api.EndPoint.Commands
{
    public class MonitorSubredditCommand : IRequest<Unit>
    {
        public string SubredditName { get; set; }
        public string Keywords { get; set; }
    }

    public class MonitorSubredditCommandHandler : IRequestHandler<MonitorSubredditCommand, Unit>
    {
        private readonly IRedditService _redditService;
        private readonly IPostRepository _postRepository;

        public MonitorSubredditCommandHandler(IRedditService redditService, IPostRepository postRepository)
        {
            _redditService = redditService;
            _postRepository = postRepository;
        }

        public async Task<Unit> Handle(MonitorSubredditCommand request, CancellationToken cancellationToken)
        {
            var posts = await _redditService.GetSubredditPosts(request.SubredditName);

            foreach (var post in posts)
            {
                if (post.Title.Contains(request.Keywords) || post.Content.Contains(request.Keywords))
                {
                    await _postRepository.AddAsync(post);
                }
            }

            return Unit.Value;
        }
    }
}
