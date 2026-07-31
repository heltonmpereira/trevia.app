using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TreviaApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCoachingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoachStudentLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Permissions = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndReason = table.Column<string>(type: "text", nullable: true),
                    EndReasonNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    OriginatingCoachRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachStudentLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachStudentLinks_AspNetUsers_CoachId",
                        column: x => x.CoachId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachStudentLinks_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoachStudentRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Direction = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CoachNotesInternal = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RespondedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RespondedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GrantedPermissionsOnAccept = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachStudentRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachStudentRequests_AspNetUsers_CoachId",
                        column: x => x.CoachId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachStudentRequests_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoachStudentLinks_CoachId",
                table: "CoachStudentLinks",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachStudentLinks_CoachId_IsActive",
                table: "CoachStudentLinks",
                columns: new[] { "CoachId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CoachStudentLinks_IsActive",
                table: "CoachStudentLinks",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CoachStudentLinks_StudentId",
                table: "CoachStudentLinks",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachStudentLinks_StudentId_IsActive",
                table: "CoachStudentLinks",
                columns: new[] { "StudentId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CoachStudentLinks_UniqueActivePair",
                table: "CoachStudentLinks",
                columns: new[] { "CoachId", "StudentId" },
                unique: true,
                filter: "[IsActive] = 1 AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CoachStudentRequests_CoachId",
                table: "CoachStudentRequests",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachStudentRequests_ExpiresAt",
                table: "CoachStudentRequests",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_CoachStudentRequests_Status",
                table: "CoachStudentRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CoachStudentRequests_StudentId",
                table: "CoachStudentRequests",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachStudentRequests_UniquePendingPair",
                table: "CoachStudentRequests",
                columns: new[] { "CoachId", "StudentId", "Status" },
                unique: true,
                filter: "[Status] = 'Pending'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoachStudentLinks");

            migrationBuilder.DropTable(
                name: "CoachStudentRequests");
        }
    }
}
