using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeightLifting.Api.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddLiftNameNormalizedUnique : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Idempotent for databases where Lifts already matched the model (e.g. EnsureCreated)
        // but __EFMigrationsHistory was baselined — avoids ADD when the column already exists.
        migrationBuilder.Sql(
            """
            IF NOT EXISTS (
                SELECT 1
                FROM sys.columns c
                INNER JOIN sys.tables t ON c.object_id = t.object_id
                WHERE t.name = N'Lifts' AND c.name = N'NameNormalized'
            )
            BEGIN
                ALTER TABLE [Lifts] ADD [NameNormalized] nvarchar(200) NULL;
            END
            """);

        migrationBuilder.Sql(
            """
            UPDATE [Lifts]
            SET [NameNormalized] = LOWER(LTRIM(RTRIM([Name])))
            WHERE [NameNormalized] IS NULL
            """);

        migrationBuilder.Sql(
            """
            IF EXISTS (
                SELECT 1
                FROM sys.columns c
                INNER JOIN sys.tables t ON c.object_id = t.object_id
                WHERE t.name = N'Lifts'
                  AND c.name = N'NameNormalized'
                  AND c.is_nullable = 1
            )
            BEGIN
                ALTER TABLE [Lifts] ALTER COLUMN [NameNormalized] nvarchar(200) NOT NULL;
            END
            """);

        migrationBuilder.Sql(
            """
            IF NOT EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'IX_Lifts_NameNormalized'
                  AND object_id = OBJECT_ID(N'[Lifts]')
            )
            BEGIN
                CREATE UNIQUE INDEX [IX_Lifts_NameNormalized] ON [Lifts] ([NameNormalized]);
            END
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'IX_Lifts_NameNormalized'
                  AND object_id = OBJECT_ID(N'[Lifts]')
            )
            BEGIN
                DROP INDEX [IX_Lifts_NameNormalized] ON [Lifts];
            END
            """);

        migrationBuilder.Sql(
            """
            IF EXISTS (
                SELECT 1
                FROM sys.columns c
                INNER JOIN sys.tables t ON c.object_id = t.object_id
                WHERE t.name = N'Lifts' AND c.name = N'NameNormalized'
            )
            BEGIN
                ALTER TABLE [Lifts] DROP COLUMN [NameNormalized];
            END
            """);
    }
}
