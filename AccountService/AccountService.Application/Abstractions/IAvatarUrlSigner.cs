namespace AccountService.Application.Abstractions;

public interface IAvatarUrlSigner
{
    string Sign(string publicId, TimeSpan lifetime);
}
