namespace AccountService.Infrastructure.ExternalServices;

public sealed class ImageStorageException : Exception
{
    public ImageStorageException()
    {
    }

    public ImageStorageException(string message) : base(message)
    {
    }

    public ImageStorageException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
