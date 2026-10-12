# PreToolUse hook: blocks Edit/Write to generated EF Core migration files.
# Migration changes must be made by editing entities/configurations and regenerating
# with `dotnet ef migrations add/remove` (see .claude/skills/db-migration/SKILL.md).
$payload = [Console]::In.ReadToEnd() | ConvertFrom-Json

$filePath = $payload.tool_input.file_path
if (-not $filePath) { exit 0 }

$normalized = $filePath -replace '\\', '/'

if ($normalized -match '/Database/Migrations/[^/]+\.(cs)$' -or $normalized -match 'ModelSnapshot\.cs$') {
    [Console]::Error.WriteLine(
        "Blocked: '$filePath' is a generated EF Core migration/snapshot file. " +
        "Do not edit these by hand. Change the entity or IEntityTypeConfiguration instead, then run " +
        "'dotnet ef migrations add <Name> --project src/KidsPartyVenue.Infrastructure --startup-project src/KidsPartyVenue.API --context KidsPartyVenueDbContext --output-dir Database/Migrations' " +
        "(or 'dotnet ef migrations remove' to drop the latest un-applied migration).")
    exit 2
}

exit 0
