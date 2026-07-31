using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TreviaApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExercisesModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ShortDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Instructions = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Tips = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Environment = table.Column<int>(type: "integer", nullable: false),
                    Modality = table.Column<int>(type: "integer", nullable: false),
                    DifficultyLevel = table.Column<int>(type: "integer", nullable: false),
                    MeasurementType = table.Column<int>(type: "integer", nullable: false),
                    Visibility = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RejectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exercises_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Exercises_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Exercises_AspNetUsers_RejectedByUserId",
                        column: x => x.RejectedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseEquipments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Equipment = table.Column<int>(type: "integer", nullable: false),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseEquipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseEquipments_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseMedias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    MediaType = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Caption = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseMedias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseMedias_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseMuscles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Muscle = table.Column<int>(type: "integer", nullable: false),
                    MuscleRole = table.Column<int>(type: "integer", nullable: false),
                    ActivationPercent = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseMuscles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseMuscles_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseEquipments_Equipment",
                table: "ExerciseEquipments",
                column: "Equipment");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseEquipments_ExerciseId",
                table: "ExerciseEquipments",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseEquipments_ExerciseId_Equipment",
                table: "ExerciseEquipments",
                columns: new[] { "ExerciseId", "Equipment" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseMedias_ExerciseId",
                table: "ExerciseMedias",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseMedias_ExerciseId_Order",
                table: "ExerciseMedias",
                columns: new[] { "ExerciseId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseMuscles_ExerciseId",
                table: "ExerciseMuscles",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseMuscles_ExerciseId_Muscle",
                table: "ExerciseMuscles",
                columns: new[] { "ExerciseId", "Muscle" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseMuscles_Muscle",
                table: "ExerciseMuscles",
                column: "Muscle");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_ApprovedByUserId",
                table: "Exercises",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_CreatedByUserId_Slug",
                table: "Exercises",
                columns: new[] { "CreatedByUserId", "Slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_CreatedByUserId_Status",
                table: "Exercises",
                columns: new[] { "CreatedByUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Name",
                table: "Exercises",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_RejectedByUserId",
                table: "Exercises",
                column: "RejectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Slug",
                table: "Exercises",
                column: "Slug");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Status_Visibility_Environment_Modality_Difficulty~",
                table: "Exercises",
                columns: new[] { "Status", "Visibility", "Environment", "Modality", "DifficultyLevel" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExerciseEquipments");

            migrationBuilder.DropTable(
                name: "ExerciseMedias");

            migrationBuilder.DropTable(
                name: "ExerciseMuscles");

            migrationBuilder.DropTable(
                name: "Exercises");
        }
    }
}
