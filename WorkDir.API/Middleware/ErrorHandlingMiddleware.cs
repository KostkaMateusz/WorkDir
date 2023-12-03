using WorkDir.API.Exceptions;

namespace WorkDir.API.Middleware;

public class ErrorHandlingMiddleware : IMiddleware
{
    public ErrorHandlingMiddleware() { }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next.Invoke(context);
        }
        catch (ValidationErrorException validationErrorException)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync(validationErrorException.Message);
        }
        catch (BadRequestException badRequestException)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync(badRequestException.Message);
        }
        catch (ForbidException forbidException)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync(forbidException.Message);
        }
        catch (NotFoundException notFoundException)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync(notFoundException.Message);
        }
        catch (FolderAlreadyExistException folderAlreadyExistException)
        {
            context.Response.StatusCode = 409;
            await context.Response.WriteAsync(folderAlreadyExistException.Message);
        }
        catch (Exception e)
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync($"Something went wrong:{e.Message}");
        }
    }
}
