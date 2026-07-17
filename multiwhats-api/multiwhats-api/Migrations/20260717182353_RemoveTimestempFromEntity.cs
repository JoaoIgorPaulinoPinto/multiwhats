using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace multiwhats_api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTimestempFromEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Usuarios",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Status",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Ocorrencias",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "mensagens",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Grupos",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Contatos",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdate",
                table: "Contatos",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdate",
                table: "Contatos");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Usuarios",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Status",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Ocorrencias",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "mensagens",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Grupos",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Contatos",
                newName: "created_at");
        }
    }
}
