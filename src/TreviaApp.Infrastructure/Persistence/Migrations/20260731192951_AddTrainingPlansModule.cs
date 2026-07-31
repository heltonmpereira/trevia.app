using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TreviaApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingPlansModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrainingPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    InstructionsIntro = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    NotesForStudent = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SplitType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    TotalWeeks = table.Column<int>(type: "integer", nullable: true),
                    SessionsPerWeek = table.Column<int>(type: "integer", nullable: true),
                    TargetVolume = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IsPublicTemplate = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AssignedToStudentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingPlans_AspNetUsers_AssignedToStudentId",
                        column: x => x.AssignedToStudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TrainingPlans_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrainingSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SuggestedDayOfWeek = table.Column<int>(type: "integer", nullable: true),
                    EstimatedDurationMin = table.Column<TimeSpan>(type: "interval", nullable: true),
                    CoachNotesInternal = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Focus = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingSessions_TrainingPlans_TrainingPlanId",
                        column: x => x.TrainingPlanId,
                        principalTable: "TrainingPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SessionExercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    NotesForStudent = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    NotesForCoach = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RestBetweenSetsSeconds = table.Column<TimeSpan>(type: "interval", nullable: true),
                    GlobalSetTechniqueAppliedToAllSets = table.Column<int>(type: "integer", nullable: true),
                    GlobalLoadOverrideKg = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    GlobalRepsOverride = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionExercises_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionExercises_TrainingSessions_TrainingSessionId",
                        column: x => x.TrainingSessionId,
                        principalTable: "TrainingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SetPrescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    SetNumber = table.Column<int>(type: "integer", nullable: false),
                    TargetRepsMin = table.Column<int>(type: "integer", nullable: true),
                    TargetRepsMax = table.Column<int>(type: "integer", nullable: true),
                    LoadValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    LoadUnit = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RestAfterSeconds = table.Column<TimeSpan>(type: "interval", nullable: true),
                    TechniqueApplied = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RateOfPerceivedExertionRPE = table.Column<int>(type: "integer", nullable: true),
                    RepsInReserveRIR = table.Column<int>(type: "integer", nullable: true),
                    TempoUnderTensionTUTSeconds = table.Column<TimeSpan>(type: "interval", nullable: true),
                    NotesProfessor = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TempoNotation = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetPrescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetPrescriptions_SessionExercises_SessionExerciseId",
                        column: x => x.SessionExerciseId,
                        principalTable: "SessionExercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionExercises_ExerciseId",
                table: "SessionExercises",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionExercises_TrainingSessionId",
                table: "SessionExercises",
                column: "TrainingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionExercises_TrainingSessionId_Order",
                table: "SessionExercises",
                columns: new[] { "TrainingSessionId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetPrescriptions_SessionExerciseId",
                table: "SetPrescriptions",
                column: "SessionExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_SetPrescriptions_SessionExerciseId_SetNumber",
                table: "SetPrescriptions",
                columns: new[] { "SessionExerciseId", "SetNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlans_AssignedToStudentId_Status",
                table: "TrainingPlans",
                columns: new[] { "AssignedToStudentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlans_CreatedByUserId_Name",
                table: "TrainingPlans",
                columns: new[] { "CreatedByUserId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlans_CreatedByUserId_Status",
                table: "TrainingPlans",
                columns: new[] { "CreatedByUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlans_IsPublicTemplate_Status_Visibility_SplitType",
                table: "TrainingPlans",
                columns: new[] { "IsPublicTemplate", "Status", "Visibility", "SplitType" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingPlans_Name",
                table: "TrainingPlans",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingSessions_TrainingPlanId",
                table: "TrainingSessions",
                column: "TrainingPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingSessions_TrainingPlanId_Order",
                table: "TrainingSessions",
                columns: new[] { "TrainingPlanId", "Order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SetPrescriptions");

            migrationBuilder.DropTable(
                name: "SessionExercises");

            migrationBuilder.DropTable(
                name: "TrainingSessions");

            migrationBuilder.DropTable(
                name: "TrainingPlans");
        }
    }
}
