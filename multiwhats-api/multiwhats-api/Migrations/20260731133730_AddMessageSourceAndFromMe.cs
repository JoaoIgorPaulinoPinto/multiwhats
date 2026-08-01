using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace multiwhats_api.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageSourceAndFromMe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "from_me",
                table: "messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "source",
                table: "messages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Backfill das mensagens existentes:
            // Outgoing eram enviadas pelo sistema (System); Incoming vieram de contatos (Contact).
            migrationBuilder.Sql("UPDATE \"messages\" SET \"source\" = 2, \"from_me\" = false WHERE \"direction\" = 0;");
            migrationBuilder.Sql("UPDATE \"messages\" SET \"source\" = 0, \"from_me\" = true WHERE \"direction\" = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "from_me",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "source",
                table: "messages");
        }
    }
}
