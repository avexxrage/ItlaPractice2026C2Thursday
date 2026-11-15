using Microsoft.AspNetCore.Mvc;
using VehicleRental.Domain.Core;
using VehicleRental.Infrastructure.Exceptions;

namespace VehicleRental.API.Middleware;

/// <summary>
/// Traduce las excepciones del dominio y de la infraestructura a respuestas ProblemDetails.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            var (status, title) = ex switch
            {
                EntityNotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
                DomainException => (StatusCodes.Status400BadRequest, "Regla de negocio no cumplida"),
                PersistenceException => (StatusCodes.Status409Conflict, "Conflicto con los datos existentes"),
                _ => (StatusCodes.Status500InternalServerError, "Error inesperado"),
            };

            if (status == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(ex, "Error no controlado procesando {Path}", context.Request.Path);
            }

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status == StatusCodes.Status500InternalServerError ? "Ocurrió un error inesperado." : ex.Message,
                Instance = context.Request.Path,
            };

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(problem, (System.Text.Json.JsonSerializerOptions?)null, "application/problem+json");
        }
    }
}
