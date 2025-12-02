using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cmcookies.Migrations
{
    public partial class ChangeCookieCodeToString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PASO 1: Eliminar las claves foráneas existentes que apuntan a 'cookies.cookie_code'
            migrationBuilder.DropForeignKey(
                name: "fk_batches_cookie_code",
                table: "batches");

            migrationBuilder.DropForeignKey(
                name: "fk_cookiematerials_cookie_code",
                table: "cookie_materials");

            migrationBuilder.DropForeignKey(
                name: "fk_order_details_cookie_code",
                table: "order_details");

            // PASO 2: Modificar el tipo de la columna PK en 'cookies'
            // Primero eliminamos la clave primaria para poder modificarla
            migrationBuilder.DropPrimaryKey(
                name: "PRIMARY",
                table: "cookies");

            migrationBuilder.AlterColumn<string>(
                name: "cookie_code",
                table: "cookies",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            // Volvemos a añadir la clave primaria con el nuevo tipo
            migrationBuilder.AddPrimaryKey(
                name: "PRIMARY",
                table: "cookies",
                column: "cookie_code");

            // PASO 3: Modificar el tipo de las columnas FK en las tablas dependientes
            migrationBuilder.AlterColumn<string>(
                name: "cookie_code",
                table: "order_details",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "cookie_code",
                table: "cookie_materials",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "cookie_code",
                table: "batches",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            // PASO 4: Volver a crear las claves foráneas
            migrationBuilder.AddForeignKey(
                name: "fk_batches_cookie_code",
                table: "batches",
                column: "cookie_code",
                principalTable: "cookies",
                principalColumn: "cookie_code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_cookiematerials_cookie_code",
                table: "cookie_materials",
                column: "cookie_code",
                principalTable: "cookies",
                principalColumn: "cookie_code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_order_details_cookie_code",
                table: "order_details",
                column: "cookie_code",
                principalTable: "cookies",
                principalColumn: "cookie_code",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir los cambios en el orden inverso
            migrationBuilder.DropForeignKey(
                name: "fk_batches_cookie_code",
                table: "batches");

            migrationBuilder.DropForeignKey(
                name: "fk_cookiematerials_cookie_code",
                table: "cookie_materials");

            migrationBuilder.DropForeignKey(
                name: "fk_order_details_cookie_code",
                table: "order_details");

            migrationBuilder.DropPrimaryKey(
                name: "PRIMARY",
                table: "cookies");

            migrationBuilder.AlterColumn<int>(
                name: "cookie_code",
                table: "cookies",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10)
                .Annotation("MySql:ValueGenerationStrategy", Microsoft.EntityFrameworkCore.Metadata.MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PRIMARY",
                table: "cookies",
                column: "cookie_code");

            migrationBuilder.AlterColumn<int>(
                name: "cookie_code",
                table: "order_details",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<int>(
                name: "cookie_code",
                table: "cookie_materials",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<int>(
                name: "cookie_code",
                table: "batches",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AddForeignKey(
                name: "fk_batches_cookie_code",
                table: "batches",
                column: "cookie_code",
                principalTable: "cookies",
                principalColumn: "cookie_code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_cookiematerials_cookie_code",
                table: "cookie_materials",
                column: "cookie_code",
                principalTable: "cookies",
                principalColumn: "cookie_code",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_order_details_cookie_code",
                table: "order_details",
                column: "cookie_code",
                principalTable: "cookies",
                principalColumn: "cookie_code",
                onDelete: ReferentialAction.Cascade);
        }
    }
}