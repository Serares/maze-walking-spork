using System.Threading.Tasks;
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
            api.MapPost("/move", Move);
        }

        public static async Task<IResult> Config(
            [FromServices] GameEngine ge,
            [FromBody] InitRequest initRequest
            )
        {
            var response = await ge.InitConfig(initRequest);
            return Results.Ok(response);
        }

        /// <summary>
        /// Handles player move requests.
        /// </summary>
        /// <param name="gameEngine">The game engine service.</param>
        /// <param name="moveRequest">The move request with PlayerId and target coordinates.</param>
        /// <returns>MoveResponse with success status, message, and updated player data.</returns>
        public static async Task<IResult> Move(
            [FromServices] GameEngine gameEngine,
            [FromBody] MoveRequest moveRequest
            )
        {
            // Validate PlayerId is not empty
            if (string.IsNullOrWhiteSpace(moveRequest.PlayerId))
            {
                return Results.BadRequest(new MoveResponse(
                    false,
                    "PlayerId is required"
                ));
            }

            // Process the move
            MoveResponse response = await gameEngine.Move(moveRequest);

            // Return appropriate status code based on success
            if (response.Success)
            {
                return Results.Ok(response);
            }
            else
            {
                return Results.BadRequest(response);
            }
        }
    }
}
