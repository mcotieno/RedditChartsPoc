namespace RedditCharts.Api.EndPoint.Queries
{
    public class GetMonitoredPostsQuery : IRequest<List<RedditPost>>
    {
        public string SubredditName { get; set; }
        public DateTime? FromDate { get; set; }
    }

    public class GetMonitoredPostsQueryHandler : IRequestHandler<GetMonitoredPostsQuery, List<RedditPost>>
    {
        private readonly IPostRepository _postRepository;

        public GetMonitoredPostsQueryHandler(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        public async Task<List<RedditPost>> Handle(GetMonitoredPostsQuery request, CancellationToken cancellationToken)
        {
            return await _postRepository.GetPostsAsync(request.SubredditName, request.FromDate);
        }
    }
}
