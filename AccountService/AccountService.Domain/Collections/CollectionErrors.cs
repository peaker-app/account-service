using Common.Domain.Results;

namespace AccountService.Domain.Collections;

public static class CollectionErrors
{
    public static readonly Error NameRequired =
        Error.Validation("Collection.NameRequired", "El nombre de la colección es obligatorio.");

    public static readonly Error NameTooLong =
        Error.Validation(
            "Collection.NameTooLong",
            $"El nombre de la colección no puede superar los {CollectionName.MaxLength} caracteres.");

    public static readonly Error DescriptionTooLong =
        Error.Validation(
            "Collection.DescriptionTooLong",
            $"La descripción no puede superar los {Collection.MaxDescriptionLength} caracteres.");

    public static readonly Error NameAlreadyUsed =
        Error.Conflict("Collection.NameAlreadyUsed", "Ya tienes una colección con ese nombre.");

    public static readonly Error DefaultNotEditable =
        Error.Conflict("Collection.DefaultNotEditable", "La colección por defecto no se puede editar.");

    public static readonly Error DefaultNotDeletable =
        Error.Conflict("Collection.DefaultNotDeletable", "La colección por defecto no se puede borrar.");

    public static readonly Error PeakAlreadyAdded =
        Error.Conflict("Collection.PeakAlreadyAdded", "El pico ya está en esta colección.");

    public static readonly Error PeakLimitReached =
        Error.Conflict(
            "Collection.PeakLimitReached",
            $"Una colección no puede tener más de {Collection.MaxPeaks} picos.");

    public static readonly Error CollectionLimitReached =
        Error.Conflict(
            "Collection.CollectionLimitReached",
            $"No puedes tener más de {Collection.MaxPerProfile} colecciones.");

    public static readonly Error PeakRequired =
        Error.Validation("Collection.PeakRequired", "El identificador del pico es obligatorio.");

    public static readonly Error PeakNameRequired =
        Error.Validation("Collection.PeakNameRequired", "El nombre del pico es obligatorio.");

    public static readonly Error PeakNameTooLong =
        Error.Validation(
            "Collection.PeakNameTooLong",
            $"El nombre del pico no puede superar los {CollectionPeakSnapshot.MaxNameLength} caracteres.");

    public static readonly Error PeakCatalogUnavailable =
        Error.Unavailable(
            "Collection.PeakCatalogUnavailable",
            "El catálogo de picos no está disponible. Inténtalo de nuevo en unos instantes.");

    public static Error NotFound(Guid collectionId) =>
        Error.NotFound("Collection.NotFound", $"No existe la colección {collectionId}.");

    public static Error PeakNotFound(Guid peakId) =>
        Error.NotFound("Collection.PeakNotFound", $"No existe el pico {peakId} en el catálogo.");

    public static Error PeakNotInCollection(Guid peakId) =>
        Error.NotFound("Collection.PeakNotInCollection", $"El pico {peakId} no está en esta colección.");
}
