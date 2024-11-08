using MediatR;
using Microsoft.AspNetCore.Mvc;
using RedditCharts.Api.EndPoint.Commands;

namespace RedditCharts.Api.Controllers
{
    public class RedditChartsController : Controller
    {
        private readonly IMediator _mediator;

        public RedditChartsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("monitor")]
        public async Task<IActionResult> MonitorSubreddit([FromBody] MonitorSubredditCommand command)
        {
            await _mediator.Send(command);
            return Ok();
        }
    }
}
