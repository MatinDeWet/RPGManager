using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Shared.Persistence.Data.Migrations.CoreMigrations
{
    /// <inheritdoc />
    public partial class AddCampaignInvitation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampaignInvitation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampaignId = table.Column<long>(type: "bigint", nullable: false),
                    InviteeEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TokenHash = table.Column<byte[]>(type: "bytea", nullable: false),
                    IssuedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AcceptedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    RespondedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignInvitation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignInvitation_Campaign_CampaignId",
                        column: x => x.CampaignId,
                        principalSchema: "public",
                        principalTable: "Campaign",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignInvitation_User_AcceptedByUserId",
                        column: x => x.AcceptedByUserId,
                        principalSchema: "public",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CampaignInvitation_User_IssuedByUserId",
                        column: x => x.IssuedByUserId,
                        principalSchema: "public",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInvitation_AcceptedByUserId",
                schema: "public",
                table: "CampaignInvitation",
                column: "AcceptedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInvitation_CampaignId_InviteeEmail",
                schema: "public",
                table: "CampaignInvitation",
                columns: new[] { "CampaignId", "InviteeEmail" },
                unique: true,
                filter: "\"Status\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInvitation_ExpiresAt",
                schema: "public",
                table: "CampaignInvitation",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInvitation_IssuedByUserId",
                schema: "public",
                table: "CampaignInvitation",
                column: "IssuedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInvitation_TokenHash",
                schema: "public",
                table: "CampaignInvitation",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignInvitation",
                schema: "public");
        }
    }
}
