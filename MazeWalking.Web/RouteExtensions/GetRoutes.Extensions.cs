using Microsoft.AspNetCore.Mvc;
using MazeWalking.Web.Services;
using MazeWalking.Web.Models;

namespace MazeWalking.Web.RouteExtensions
{
    public static class GetRoutes
    {
        public static void MapGets(this WebApplication app)
        {
            var api = app.MapGroup("/api");

            api.MapGet("/config", Config);
        }

        private static IResult Config(
            [FromServices] GameEngine ge,
            [FromBody] InitRequest InitRequest
            )
        {
            return Results.Ok(ge.InitConfig());
        }
    }
}
