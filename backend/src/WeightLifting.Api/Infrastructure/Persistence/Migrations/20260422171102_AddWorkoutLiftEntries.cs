using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeightLifting.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutLiftEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkoutLiftEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkoutId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LiftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AddedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutLiftEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutLiftEntries_Lifts_LiftId",
                        column: x => x.LiftId,
                        principalTable: "Lifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkoutLiftEntries_Workouts_WorkoutId",
                        column: x => x.WorkoutId,
                        principalTable: "Workouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLiftEntries_LiftId",
                table: "WorkoutLiftEntries",
                column: "LiftId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLiftEntries_WorkoutId_LiftId",
                table: "WorkoutLiftEntries",
                columns: new[] { "WorkoutId", "LiftId" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLiftEntries_WorkoutId_Position",
                table: "WorkoutLiftEntries",
                columns: new[] { "WorkoutId", "Position" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkoutLiftEntries");
        }
    }
}
