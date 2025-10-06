using MazeWalking.Web.Models;
using MazeWalking.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace MazeWalking.Web.RouteExtensions
{
    public static class PostRoutes
    {
        public static void MapPosts(this WebApplication app)
        {
            var api = app.MapGroup("/api");
            api.MapPost("/config", Config);
        }

        public static IResult Config(
            [FromServices] GameEngine ge,
            [FromBody] InitRequest initRequest
            )
        {
            return Results.Ok(ge.InitConfig(initRequest));
        }
    }
}
