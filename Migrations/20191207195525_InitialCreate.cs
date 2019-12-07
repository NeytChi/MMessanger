using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace miniMessanger.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.CreateTable(
                name: "chatroom",
                columns: table => new
                {
                    chat_id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    chat_token = table.Column<string>(type: "varchar(20)", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.chat_id);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    message_id = table.Column<long>(type: "bigint(20)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    chat_id = table.Column<int>(type: "int(11)", nullable: false),
                    user_id = table.Column<int>(type: "int(11)", nullable: false),
                    message_type = table.Column<string>(type: "varchar(10)", nullable: true),
                    message_text = table.Column<string>(type: "varchar(500) CHARACTER SET utf8 COLLATE utf8_general_ci", nullable: true),
                    url_file = table.Column<string>(type: "varchar(100)", nullable: true),
                    message_viewed = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.message_id);
                });

            migrationBuilder.CreateTable(
                name: "participants",
                columns: table => new
                {
                    participant_id = table.Column<long>(type: "bigint(20)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    chat_id = table.Column<int>(type: "int(11)", nullable: false),
                    user_id = table.Column<int>(type: "int(11)", nullable: false),
                    opposide_id = table.Column<int>(type: "int(11)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.participant_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_email = table.Column<string>(type: "varchar(256)", nullable: true),
                    user_login = table.Column<string>(type: "varchar(256)", nullable: true),
                    user_password = table.Column<string>(type: "varchar(256)", nullable: true),
                    created_at = table.Column<int>(type: "int(11)", nullable: false),
                    user_hash = table.Column<string>(type: "varchar(120)", nullable: true),
                    activate = table.Column<sbyte>(type: "tinyint(4)", nullable: true, defaultValueSql: "'0'"),
                    user_token = table.Column<string>(type: "varchar(50)", nullable: true),
                    last_login_at = table.Column<int>(type: "int(11)", nullable: true),
                    recovery_code = table.Column<int>(type: "int(11)", nullable: true),
                    recovery_token = table.Column<string>(type: "varchar(50)", nullable: true),
                    user_public_token = table.Column<string>(type: "varchar(20)", nullable: true),
                    profile_token = table.Column<string>(type: "varchar(50)", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "blocked_users",
                columns: table => new
                {
                    blocked_id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int(11)", nullable: false),
                    blocked_user_id = table.Column<int>(type: "int(11)", nullable: false),
                    blocked_reason = table.Column<string>(type: "varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci", nullable: true),
                    blocked_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.blocked_id);
                    table.ForeignKey(
                        name: "blocked_users_ibfk_2",
                        column: x => x.blocked_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "blocked_users_ibfk_1",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "like_profiles",
                columns: table => new
                {
                    like_id = table.Column<long>(type: "bigint(20)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int(11)", nullable: false),
                    to_user_id = table.Column<int>(type: "int(11)", nullable: false),
                    like = table.Column<bool>(type: "boolean", nullable: false),
                    dislike = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.like_id);
                    table.ForeignKey(
                        name: "FK_like_profiles_users_to_user_id",
                        column: x => x.to_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_like_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "profiles",
                columns: table => new
                {
                    profile_id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int(11)", nullable: false),
                    url_photo = table.Column<string>(type: "varchar(256)", nullable: true),
                    profile_age = table.Column<sbyte>(type: "tinyint(3)", nullable: true),
                    profile_gender = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    profile_city = table.Column<string>(type: "varchar(256)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.profile_id);
                    table.ForeignKey(
                        name: "profiles_ibfk_1",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "complaints",
                columns: table => new
                {
                    complaint_id = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int(11)", nullable: false),
                    blocked_id = table.Column<int>(type: "int(11)", nullable: false),
                    message_id = table.Column<long>(type: "bigint(20)", nullable: false),
                    complaint = table.Column<string>(type: "varchar(100) CHARACTER SET utf8 COLLATE utf8_general_ci", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.complaint_id);
                    table.ForeignKey(
                        name: "complaints_ibfk_2",
                        column: x => x.blocked_id,
                        principalTable: "blocked_users",
                        principalColumn: "blocked_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "complaints_ibfk_3",
                        column: x => x.message_id,
                        principalTable: "messages",
                        principalColumn: "message_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "complaints_ibfk_1",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "blocked_user_id",
                table: "blocked_users",
                column: "blocked_user_id");

            migrationBuilder.CreateIndex(
                name: "user_id",
                table: "blocked_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "blocked_id",
                table: "complaints",
                column: "blocked_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "message_id",
                table: "complaints",
                column: "message_id");

            migrationBuilder.CreateIndex(
                name: "user_id",
                table: "complaints",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "to_user_id",
                table: "like_profiles",
                column: "to_user_id");

            migrationBuilder.CreateIndex(
                name: "user_id",
                table: "like_profiles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "user_id",
                table: "profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "user_email",
                table: "users",
                column: "user_email",
                unique: true);
                */
            migrationBuilder.AlterColumn<string>(
                table: "users",
                name: "user_email",
                type: "varchar(100)"    
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chatroom");

            migrationBuilder.DropTable(
                name: "complaints");

            migrationBuilder.DropTable(
                name: "like_profiles");

            migrationBuilder.DropTable(
                name: "participants");

            migrationBuilder.DropTable(
                name: "profiles");

            migrationBuilder.DropTable(
                name: "blocked_users");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
