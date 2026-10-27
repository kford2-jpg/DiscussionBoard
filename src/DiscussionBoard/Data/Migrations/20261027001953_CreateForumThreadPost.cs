using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiscussionBoard.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateForumThreadPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Forums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forums", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiscussionBase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 10000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "TEXT", nullable: false),
                    Discriminator = table.Column<string>(type: "TEXT", maxLength: 21, nullable: false),
                    ForumId = table.Column<int>(type: "INTEGER", nullable: true),
                    DiscussionThreadId = table.Column<int>(type: "INTEGER", nullable: true),
                    ParentPostId = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscussionBase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscussionBase_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiscussionBase_DiscussionBase_DiscussionThreadId",
                        column: x => x.DiscussionThreadId,
                        principalTable: "DiscussionBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiscussionBase_DiscussionBase_ParentPostId",
                        column: x => x.ParentPostId,
                        principalTable: "DiscussionBase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiscussionBase_Forums_ForumId",
                        column: x => x.ForumId,
                        principalTable: "Forums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscussionBase_ApplicationUserId",
                table: "DiscussionBase",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscussionBase_DiscussionThreadId",
                table: "DiscussionBase",
                column: "DiscussionThreadId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscussionBase_ForumId",
                table: "DiscussionBase",
                column: "ForumId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscussionBase_ParentPostId",
                table: "DiscussionBase",
                column: "ParentPostId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscussionBase");

            migrationBuilder.DropTable(
                name: "Forums");
        }
    }
}
