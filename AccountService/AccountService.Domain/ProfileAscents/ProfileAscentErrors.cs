using Common.Domain.Results;

namespace AccountService.Domain.ProfileAscents;

public static class ProfileAscentErrors
{
    public static readonly Error ProfileRequired =
        Error.Validation("ProfileAscent.ProfileRequired", "La ascensión debe pertenecer a un perfil.");

    public static readonly Error AscentRequired =
        Error.Validation("ProfileAscent.AscentRequired", "El identificador de la ascensión es obligatorio.");

    public static readonly Error PeakRequired =
        Error.Validation("ProfileAscent.PeakRequired", "El identificador del pico es obligatorio.");

    public static readonly Error PeakNameRequired =
        Error.Validation("ProfileAscent.PeakNameRequired", "El nombre del pico es obligatorio.");

    public static readonly Error PeakNameTooLong =
        Error.Validation(
            "ProfileAscent.PeakNameTooLong",
            $"El nombre del pico no puede superar los {PeakSnapshot.MaxNameLength} caracteres.");

    public static readonly Error VisibilityInvalid =
        Error.Validation("ProfileAscent.VisibilityInvalid", "La visibilidad de la ascensión no es válida.");
}
