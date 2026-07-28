# account-service

Perfiles de usuario, avatares y colecciones de montañas. MySQL 8.4 (`peaker_accounts`).
Ver `.claude/docs/DESIGN.md` §5.

## Puesta en marcha local

La cadena de conexión **no se versiona** (`ARCHITECTURE.md` §11, regla innegociable 8):
`appsettings.json` la deja vacía. Dentro de Docker la aporta
`services-deployment/config/account-service.env` mediante `ConnectionStrings__AccountDatabase`.

Fuera de Docker se carga esa misma variable desde el `.env`, sin copiar la contraseña a ningún
sitio. Sirve tanto para `dotnet run` como para `dotnet ef`:

```powershell
cd ..\services-deployment
. .\scripts\Use-DevDatabase.ps1 account     # bash: source ./scripts/use-dev-database.sh account
```

`dotnet user-secrets` es una alternativa válida para `dotnet run`, pero **`dotnet ef` la ignora**:
`AccountDbContextFactory` tiene prioridad sobre el host de la API y solo lee la variable de
entorno.

El esquema no se aplica en el arranque (`ARCHITECTURE.md` §12). Fuera de Docker:

```bash
dotnet ef database update \
  --project AccountService/AccountService.Infrastructure \
  --startup-project AccountService/AccountService.API
```
