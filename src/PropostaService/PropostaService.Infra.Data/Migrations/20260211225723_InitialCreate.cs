using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropostaService.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "propostas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome_cliente = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cpf_cliente = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    tipo_seguro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    valor_cobertura = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    valor_premio = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_atualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_propostas", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_propostas_cpf_cliente",
                table: "propostas",
                column: "cpf_cliente");

            migrationBuilder.CreateIndex(
                name: "ix_propostas_status",
                table: "propostas",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "propostas");
        }
    }
}
