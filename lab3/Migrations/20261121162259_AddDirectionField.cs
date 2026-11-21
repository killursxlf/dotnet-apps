using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTTelemetryLab.Migrations
{
    public partial class AddDirectionField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Direction",
                table: "Records",
                type: "TEXT",
                maxLength: 1,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Direction",
                table: "Records");
        }
    }
}
