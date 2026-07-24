using Common.Domain.Results;

namespace AccountService.Domain.Profiles;

public static class ProfileErrors
{
    public static readonly Error DisplayNameEmpty =
        Error.Validation("Profile.DisplayNameEmpty", "El nombre visible es obligatorio.");

    public static readonly Error DisplayNameTooLong =
        Error.Validation(
            "Profile.DisplayNameTooLong",
            $"El nombre visible no puede superar los {DisplayName.MaxLength} caracteres.");

    public static readonly Error BioTooLong =
        Error.Validation(
            "Profile.BioTooLong",
            $"La biografía no puede superar los {Profile.MaxBioLength} caracteres.");

    public static readonly Error CountryCodeInvalid =
        Error.Validation("Profile.CountryCodeInvalid", "El código de país no es un ISO 3166-1 alpha-2 válido.");

    public static readonly Error SlugInvalid =
        Error.Validation(
            "Profile.SlugInvalid",
            $"El slug debe estar en kebab-case y no superar los {ProfileSlug.MaxLength} caracteres.");

    public static readonly Error SlugAlreadyTaken =
        Error.Conflict("Profile.SlugAlreadyTaken", "El slug ya está en uso por otro montañero.");

    public static readonly Error AvatarTooLarge =
        Error.Validation(
            "Profile.AvatarTooLarge",
            $"La imagen supera el tamaño máximo de {AvatarConstraints.MaxSizeBytes / (1024 * 1024)} MB.");

    public static readonly Error AvatarFormatNotSupported =
        Error.Validation("Profile.AvatarFormatNotSupported", "El formato de imagen no está soportado. Usa JPEG, PNG o WebP.");

    public static readonly Error AvatarUploadFailed =
        Error.Failure("Profile.AvatarUploadFailed", "No se pudo almacenar la imagen. Inténtalo de nuevo más tarde.");

    public static Error NotFound(Guid userId) =>
        Error.NotFound("Profile.NotFound", $"No existe el perfil del usuario {userId}.");

    public static Error NotFoundBySlug(string slug) =>
        Error.NotFound("Profile.NotFound", $"No existe un perfil público con el slug '{slug}'.");

    public static Error AlreadyExists(Guid userId) =>
        Error.Conflict("Profile.AlreadyExists", $"Ya existe un perfil para el usuario {userId}.");
}
