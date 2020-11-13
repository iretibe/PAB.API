using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace PAB.Entity.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "psPARContactName",
                columns: table => new
                {
                    pkId = table.Column<Guid>(nullable: false, defaultValueSql: "(newid())"),
                    dCreatedate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    iUserId = table.Column<Guid>(nullable: false),
                    szFirstName = table.Column<string>(unicode: false, maxLength: 100, nullable: false),
                    szLastName = table.Column<string>(unicode: false, maxLength: 100, nullable: true),
                    szMiddleName = table.Column<string>(unicode: false, maxLength: 100, nullable: true),
                    szNickName = table.Column<string>(unicode: false, maxLength: 100, nullable: true),
                    szSuffix = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    szTitle = table.Column<string>(unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_psPARContactName", x => x.pkId);
                });

            migrationBuilder.CreateTable(
                name: "psPARContactAddress",
                columns: table => new
                {
                    pkId = table.Column<Guid>(nullable: false, defaultValueSql: "(newid())"),
                    iContactNameId = table.Column<Guid>(nullable: false),
                    szBusinessAddress = table.Column<string>(unicode: false, maxLength: 250, nullable: true),
                    szHomeAddress = table.Column<string>(unicode: false, maxLength: 250, nullable: true),
                    szOther = table.Column<string>(unicode: false, maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_psPARContactAddress", x => x.pkId);
                    table.ForeignKey(
                        name: "FK_psPARContactAddress_psPARContactName",
                        column: x => x.iContactNameId,
                        principalTable: "psPARContactName",
                        principalColumn: "pkId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "psPARContactEmail",
                columns: table => new
                {
                    pkId = table.Column<Guid>(nullable: false, defaultValueSql: "(newid())"),
                    iContactNameId = table.Column<Guid>(nullable: false),
                    szEmailAddress1 = table.Column<string>(unicode: false, maxLength: 100, nullable: true),
                    szEmailAddress2 = table.Column<string>(unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_psPARContactEmail", x => x.pkId);
                    table.ForeignKey(
                        name: "FK_psPARContactEmail_psPARContactName",
                        column: x => x.iContactNameId,
                        principalTable: "psPARContactName",
                        principalColumn: "pkId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "psPARContactOther",
                columns: table => new
                {
                    pkId = table.Column<Guid>(nullable: false, defaultValueSql: "(newid())"),
                    dAnniversary = table.Column<DateTime>(type: "datetime", nullable: true),
                    dBirthday = table.Column<DateTime>(type: "datetime", nullable: true),
                    iContactNameId = table.Column<Guid>(nullable: false),
                    szPersonalWebPage = table.Column<string>(unicode: false, maxLength: 150, nullable: true),
                    szSignificantOther = table.Column<string>(unicode: false, maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_psPARContactOther", x => x.pkId);
                    table.ForeignKey(
                        name: "FK_psPARContactOther_psPARContactName",
                        column: x => x.iContactNameId,
                        principalTable: "psPARContactName",
                        principalColumn: "pkId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "psPARContactPhone",
                columns: table => new
                {
                    pkId = table.Column<Guid>(nullable: false, defaultValueSql: "(newid())"),
                    iContactNameId = table.Column<Guid>(nullable: false),
                    szBusiness = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    szBusinessFax = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    szHome = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    szHomeFax = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    szMobile1 = table.Column<string>(unicode: false, maxLength: 50, nullable: true),
                    szMobile2 = table.Column<string>(unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_psPARContactPhone", x => x.pkId);
                    table.ForeignKey(
                        name: "FK_psPARContactPhone_psPARContactName",
                        column: x => x.iContactNameId,
                        principalTable: "psPARContactName",
                        principalColumn: "pkId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "psPARContactWork",
                columns: table => new
                {
                    pkId = table.Column<Guid>(nullable: false, defaultValueSql: "(newid())"),
                    iContactNameId = table.Column<Guid>(nullable: false),
                    szCompany = table.Column<string>(unicode: false, maxLength: 250, nullable: true),
                    szJobTitle = table.Column<string>(unicode: false, maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_psPARContactWork", x => x.pkId);
                    table.ForeignKey(
                        name: "FK_psPARContactWork_psPARContactName",
                        column: x => x.iContactNameId,
                        principalTable: "psPARContactName",
                        principalColumn: "pkId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_psPARContactAddress_iContactNameId",
                table: "psPARContactAddress",
                column: "iContactNameId");

            migrationBuilder.CreateIndex(
                name: "IX_psPARContactEmail_iContactNameId",
                table: "psPARContactEmail",
                column: "iContactNameId");

            migrationBuilder.CreateIndex(
                name: "IX_psPARContactOther_iContactNameId",
                table: "psPARContactOther",
                column: "iContactNameId");

            migrationBuilder.CreateIndex(
                name: "IX_psPARContactPhone_iContactNameId",
                table: "psPARContactPhone",
                column: "iContactNameId");

            migrationBuilder.CreateIndex(
                name: "IX_psPARContactWork_iContactNameId",
                table: "psPARContactWork",
                column: "iContactNameId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "psPARContactAddress");

            migrationBuilder.DropTable(
                name: "psPARContactEmail");

            migrationBuilder.DropTable(
                name: "psPARContactOther");

            migrationBuilder.DropTable(
                name: "psPARContactPhone");

            migrationBuilder.DropTable(
                name: "psPARContactWork");

            migrationBuilder.DropTable(
                name: "psPARContactName");
        }
    }
}
