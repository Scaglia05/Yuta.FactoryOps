using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yuta.FactoryOps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CorrigirColunasEmpresaUsuario : Migration
    {
        /// <summary>
        /// Corrige um desalinhamento entre as entidades (Empresa.Nome e Usuario.ProviderKey)
        /// e o schema realmente aplicado no banco pela migration inicial. Sem essas colunas,
        /// toda tentativa de gravar uma Empresa ou um Usuario (inclusive o seed automático do
        /// usuário admin em DatabaseSeeder) falhava silenciosamente contra o Supabase.
        /// </summary>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nome",
                table: "Empresas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProviderKey",
                table: "Usuarios",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nome",
                table: "Empresas");

            migrationBuilder.DropColumn(
                name: "ProviderKey",
                table: "Usuarios");
        }
    }
}
