using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TreviaApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGamificationModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AchievementDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Icon = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PointsReward = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CriteriaConfigJson = table.Column<string>(type: "jsonb", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyMissionDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TargetValue = table.Column<int>(type: "integer", nullable: false),
                    Metric = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    PointsReward = table.Column<int>(type: "integer", nullable: false),
                    XpReward = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyMissionDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PointTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReferenceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PointTransactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CurrentXp = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    TotalXpEarned = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLevels_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserStreaks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DailyCurrent = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    DailyLongest = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    DailyLastActiveAt = table.Column<DateOnly>(type: "date", nullable: true),
                    WeeklyCurrent = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    WeeklyLongest = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    WeekStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStreaks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserStreaks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyMissionDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    TargetValue = table.Column<int>(type: "integer", nullable: false),
                    Metric = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    PointsReward = table.Column<int>(type: "integer", nullable: false),
                    XpReward = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyMissionDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAchievements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AchievementDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnlockedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Progress = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAchievements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAchievements_AchievementDefinitions_AchievementDefiniti~",
                        column: x => x.AchievementDefinitionId,
                        principalTable: "AchievementDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAchievements_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDailyMissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    CurrentValue = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClaimedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDailyMissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDailyMissions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDailyMissions_DailyMissionDefinitions_MissionId",
                        column: x => x.MissionId,
                        principalTable: "DailyMissionDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserWeeklyMissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekStart = table.Column<DateOnly>(type: "date", nullable: false),
                    CurrentValue = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClaimedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWeeklyMissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserWeeklyMissions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserWeeklyMissions_WeeklyMissionDefinitions_MissionId",
                        column: x => x.MissionId,
                        principalTable: "WeeklyMissionDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AchievementDefinitions_Category",
                table: "AchievementDefinitions",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementDefinitions_Code",
                table: "AchievementDefinitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AchievementDefinitions_IsActive",
                table: "AchievementDefinitions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DailyMissionDefinitions_Code",
                table: "DailyMissionDefinitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyMissionDefinitions_IsActive",
                table: "DailyMissionDefinitions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_ReferenceType_ReferenceId",
                table: "PointTransactions",
                columns: new[] { "ReferenceType", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_UserId_CreatedAt",
                table: "PointTransactions",
                columns: new[] { "UserId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_UserId_Reason",
                table: "PointTransactions",
                columns: new[] { "UserId", "Reason" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_AchievementDefinitionId",
                table: "UserAchievements",
                column: "AchievementDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_UnlockedAt",
                table: "UserAchievements",
                column: "UnlockedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_UserId",
                table: "UserAchievements",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_UserId_AchievementDefinitionId",
                table: "UserAchievements",
                columns: new[] { "UserId", "AchievementDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDailyMissions_IsCompleted",
                table: "UserDailyMissions",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_UserDailyMissions_MissionId",
                table: "UserDailyMissions",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDailyMissions_UserId",
                table: "UserDailyMissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDailyMissions_UserId_Date",
                table: "UserDailyMissions",
                columns: new[] { "UserId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_UserDailyMissions_UserId_MissionId_Date",
                table: "UserDailyMissions",
                columns: new[] { "UserId", "MissionId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLevels_UserId",
                table: "UserLevels",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserStreaks_UserId",
                table: "UserStreaks",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserWeeklyMissions_IsCompleted",
                table: "UserWeeklyMissions",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_UserWeeklyMissions_MissionId",
                table: "UserWeeklyMissions",
                column: "MissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWeeklyMissions_UserId",
                table: "UserWeeklyMissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWeeklyMissions_UserId_MissionId_WeekStart",
                table: "UserWeeklyMissions",
                columns: new[] { "UserId", "MissionId", "WeekStart" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserWeeklyMissions_UserId_WeekStart",
                table: "UserWeeklyMissions",
                columns: new[] { "UserId", "WeekStart" });

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyMissionDefinitions_Code",
                table: "WeeklyMissionDefinitions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyMissionDefinitions_IsActive",
                table: "WeeklyMissionDefinitions",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PointTransactions");

            migrationBuilder.DropTable(
                name: "UserAchievements");

            migrationBuilder.DropTable(
                name: "UserDailyMissions");

            migrationBuilder.DropTable(
                name: "UserLevels");

            migrationBuilder.DropTable(
                name: "UserStreaks");

            migrationBuilder.DropTable(
                name: "UserWeeklyMissions");

            migrationBuilder.DropTable(
                name: "AchievementDefinitions");

            migrationBuilder.DropTable(
                name: "DailyMissionDefinitions");

            migrationBuilder.DropTable(
                name: "WeeklyMissionDefinitions");
        }
    }
}
