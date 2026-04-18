namespace HealthPlatform.Application.Exceptions;

public class DomainException : Exception
{
    public DomainException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}

public sealed class NotFoundException(string code, string message) : DomainException(code, message);

public sealed class ConflictException(string code, string message) : DomainException(code, message);

public sealed class AccessDeniedException(string code, string message) : DomainException(code, message);
