namespace WorkDir.API.Exceptions;

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}

public class ForbidException : Exception
{
    public ForbidException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class ValidationErrorException : Exception
{
    public ValidationErrorException(string message) : base(message) { }
}

public class FolderAlreadyExistException : Exception
{
    public FolderAlreadyExistException(string message) : base(message) { }
}
