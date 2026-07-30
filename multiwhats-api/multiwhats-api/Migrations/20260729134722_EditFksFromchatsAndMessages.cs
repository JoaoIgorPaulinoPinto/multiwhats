using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace multiwhats_api.Migrations
{
    /// <inheritdoc />
    public partial class EditFksFromchatsAndMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_messages_chats_ChatId1",
                table: "messages");

            migrationBuilder.DropIndex(
                name: "IX_messages_ChatId1",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "ChatId1",
                table: "messages");

            migrationBuilder.RenameColumn(
                name: "message_id",
                table: "messages",
                newName: "whatsapp_message_id");

            migrationBuilder.RenameIndex(
                name: "IX_messages_message_id",
                table: "messages",
                newName: "IX_messages_whatsapp_message_id");

            migrationBuilder.AddColumn<int>(
                name: "last_message_id",
                table: "chats",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_chats_last_message_id",
                table: "chats",
                column: "last_message_id");

            migrationBuilder.AddForeignKey(
                name: "FK_chats_messages_last_message_id",
                table: "chats",
                column: "last_message_id",
                principalTable: "messages",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_chats_messages_last_message_id",
                table: "chats");

            migrationBuilder.DropIndex(
                name: "IX_chats_last_message_id",
                table: "chats");

            migrationBuilder.DropColumn(
                name: "last_message_id",
                table: "chats");

            migrationBuilder.RenameColumn(
                name: "whatsapp_message_id",
                table: "messages",
                newName: "message_id");

            migrationBuilder.RenameIndex(
                name: "IX_messages_whatsapp_message_id",
                table: "messages",
                newName: "IX_messages_message_id");

            migrationBuilder.AddColumn<int>(
                name: "ChatId1",
                table: "messages",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_messages_ChatId1",
                table: "messages",
                column: "ChatId1",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_messages_chats_ChatId1",
                table: "messages",
                column: "ChatId1",
                principalTable: "chats",
                principalColumn: "id");
        }
    }
}
