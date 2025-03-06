using MediatR;
using Microsoft.AspNetCore.Identity.Data;
using StravaRunner.API.Models;
using StravaRunner.Core.Messages.Auth.Commands;
using LoginRequest = StravaRunner.API.Models.LoginRequest;

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
            return Results.NoContent();
        })
        .WithName("Login");

        authGroup.MapGet("/strava", async (ISender sender, [AsParameters] StravaAuthRequest request) =>
        {
            var user = await sender.Send(new AuthorizeCommand(request.State, request.Code));
            await sender.Send(new LoginCommand(user.Email));
            return Results.Redirect("http://localhost:5000");
        })
        .WithName("StravaAuthorize");
        
        authGroup.MapGet("/authenticate", async (ISender sender, [AsParameters] AuthenticateRequest request) =>
        {
            var tokenDto = await sender.Send(new AuthenticateCommand(request.Token));
            return Results.Ok(tokenDto);
        })
        .WithTags("Auth")
        .WithName("Authenticate");

        authGroup.MapPost("/refresh", async (ISender sender, RefreshRequest request) =>
        {
            var tokenDto = await sender.Send(new RefreshTokenCommand(request.RefreshToken));
            return Results.Ok(tokenDto);
        })
        .WithName("Refresh");

        return routes;
    }
}