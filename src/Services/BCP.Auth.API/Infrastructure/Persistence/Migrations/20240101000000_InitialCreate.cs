using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BCP.Auth.API.Infrastructure.Persistence.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "auth");

        migrationBuilder.CreateTable(
            name: "Usuario",
            schema: "auth",
            columns: table => new
            {
                IdUsuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                IdEmpresa = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IdSucursal = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                Rol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Activo"),
                FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                UltimoAcceso = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Usuario", x => x.IdUsuario));

        migrationBuilder.CreateIndex(
            name: "IX_Usuario_Email",
            schema: "auth",
            table: "Usuario",
            column: "Email",
            unique: true);

        // Insertar usuarios semilla
        migrationBuilder.InsertData(
            schema: "auth",
            table: "Usuario",
            columns: ["IdUsuario", "Nombre", "Email", "PasswordHash", "Rol", "Estado", "FechaCreacion"],
            values: new object[,]
            {
                {
                    new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    "Carlos Mamani", "admin@bcp.com",
                    "$2a$11$hashed_admin_placeholder",
                    "AdministradorSistema", "Activo", new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                {
                    new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    "María Quispe", "gerente@empresa1.com",
                    "$2a$11$hashed_gerente_placeholder",
                    "GerenteEmpresarial", "Activo", new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                {
                    new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    "Pedro Condori", "supervisor@sucursal1.com",
                    "$2a$11$hashed_supervisor_placeholder",
                    "SupervisorSucursal", "Activo", new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                {
                    new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                    "Ana López", "operador@sucursal1.com",
                    "$2a$11$hashed_operador_placeholder",
                    "OperadorCaja", "Activo", new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Usuario", schema: "auth");
    }
}
