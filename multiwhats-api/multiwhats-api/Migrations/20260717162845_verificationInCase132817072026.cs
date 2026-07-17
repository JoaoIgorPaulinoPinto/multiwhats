using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace multiwhats_api.Migrations
{
    /// <inheritdoc />
    public partial class verificationInCase132817072026 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contatos_Grupos_grupo_id",
                table: "Contatos");

            migrationBuilder.AlterColumn<int>(
                name: "grupo_id",
                table: "Contatos",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Contatos_Grupos_grupo_id",
                table: "Contatos",
                column: "grupo_id",
                principalTable: "Grupos",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contatos_Grupos_grupo_id",
                table: "Contatos");

            migrationBuilder.AlterColumn<int>(
                name: "grupo_id",
                table: "Contatos",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Contatos_Grupos_grupo_id",
                table: "Contatos",
                column: "grupo_id",
                principalTable: "Grupos",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
