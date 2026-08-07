using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace multiwhats_api.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "author_jid",
                table: "messages",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "author_name",
                table: "messages",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "author_jid",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "author_name",
                table: "messages");
        }
    }
}
