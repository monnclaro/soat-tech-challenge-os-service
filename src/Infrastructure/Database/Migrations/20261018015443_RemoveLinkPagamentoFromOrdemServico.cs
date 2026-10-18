using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage] // Migration auto-gerada pelo EF Core ("dotnet ef migrations add") — sem lógica de negócio.
    public partial class RemoveLinkPagamentoFromOrdemServico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "link_pagamento",
                table: "ordem_servico");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "link_pagamento",
                table: "ordem_servico",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
