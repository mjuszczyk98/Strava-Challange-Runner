using MediatR;
using Microsoft.AspNetCore.Mvc;
using StravaRunner.API.Models.Auth;
using StravaRunner.Core.Messages.Auth.Commands;

namespace StravaRunner.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var authGroup = routes.MapGroup("/api/auth")
            .WithTags("Auth");
        
        
        authGroup.MapPost("/login", async (ISender sender, LoginRequest request) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return Results.BadRequest("Email is required");
            
            await sender.Send(new LoginCommand(request.Email));
            return Results.Ok();
        });

        authGroup.MapGet("/strava", async (ISender sender, [AsParameters] StravaAuthRequest request) =>
        {
            await sender.Send(new AuthorizeWithStravaCommand(request.State, request.Code));
            return Results.Ok();
        });

        return routes;
    }
}