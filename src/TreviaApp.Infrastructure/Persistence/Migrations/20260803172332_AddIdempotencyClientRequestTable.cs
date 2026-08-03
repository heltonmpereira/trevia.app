using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TreviaApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIdempotencyClientRequestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessedClientRequests",
                columns: table => new
                {
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ResponsePayload = table.Column<string>(type: "jsonb", nullable: true),
                    StatusCode = table.Column<int>(type: "integer", nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedClientRequests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_ProcessedClientRequests_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedClientRequests_ProcessedAt",
                table: "ProcessedClientRequests",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedClientRequests_UserId",
                table: "ProcessedClientRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedClientRequests_UserId_RequestId",
                table: "ProcessedClientRequests",
                columns: new[] { "UserId", "RequestId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessedClientRequests");
        }
    }
}
