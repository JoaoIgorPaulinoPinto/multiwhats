using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace multiwhats_api.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexOnWhatsAppMessageId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_messages_whatsapp_message_id",
                table: "messages");

            migrationBuilder.CreateIndex(
                name: "IX_messages_whatsapp_message_id",
                table: "messages",
                column: "whatsapp_message_id",
                unique: true,
                filter: "\"whatsapp_message_id\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_messages_whatsapp_message_id",
                table: "messages");

            migrationBuilder.CreateIndex(
                name: "IX_messages_whatsapp_message_id",
                table: "messages",
                column: "whatsapp_message_id");
        }
    }
}
