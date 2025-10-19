# Database Migrations & Postgres Setup (MVP → Prod)

Status: Proposal

## Goals
- Move from EnsureCreated (dev) to Migrate (all envs).
- Adopt Postgres in non-dev with EF Core migrations.

## Packages to add (when ready)
- `Microsoft.EntityFrameworkCore.Design`
- `Npgsql.EntityFrameworkCore.PostgreSQL`

## Connection strings
- SQLite (dev): `Data Source=conveyancing.db`
- Postgres (prod): `Host=localhost;Port=5432;Database=conveyancing;Username=app;Password=secret;Ssl Mode=Require;Trust Server Certificate=true`

## Steps
1) Add packages above.
2) Update Program.cs to use Postgres when `ASPNETCORE_ENVIRONMENT` != Development:
```
var cs = builder.Configuration.GetConnectionString("Default");
if (builder.Environment.IsDevelopment())
    options.UseSqlite(cs);
else
    options.UseNpgsql(cs);
```
3) Create initial migration:
```
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --project src/CodePunk.Conveyancing.Api
```
4) Apply migrations on startup:
```
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ConveyancingDbContext>();
db.Database.Migrate();
```
5) CI/CD: run `dotnet ef database update` on deploy or rely on startup `Migrate()`.

## Notes
- EnsureCreated and Migrate should not be mixed against the same database.
- For local dev, keep SQLite with EnsureCreated for speed; switch to migrations once schema stabilizes.
- Use environment variables to supply Postgres connection in prod.
