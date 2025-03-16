using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace questionnaire.Migrations
{
    /// <inheritdoc />
    public partial class FirstMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccessLevel",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    level_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AccessLe__3213E83F5CA6A6C9", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Anonymous",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    session_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Anonymou__3213E83F1978D4C1", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionType",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name_question = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Question__3213E83FDD8A0518", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "TypeQuestionnaire",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    type = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TypeQues__3213E83FBDD485ED", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    access_level_ID = table.Column<int>(type: "int", nullable: false),
                    username = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__User__3213E83F3809148A", x => x.id);
                    table.ForeignKey(
                        name: "User_fk1",
                        column: x => x.access_level_ID,
                        principalTable: "AccessLevel",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Questionnaire",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    type_questionnaire_ID = table.Column<int>(type: "int", nullable: false),
                    user_ID = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    is_published = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Question__3213E83FB95D3D32", x => x.id);
                    table.ForeignKey(
                        name: "Questionnaire_fk1",
                        column: x => x.type_questionnaire_ID,
                        principalTable: "TypeQuestionnaire",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "Questionnaire_fk2",
                        column: x => x.user_ID,
                        principalTable: "User",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Tokens",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_ID = table.Column<int>(type: "int", nullable: false),
                    refresh_token = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    refresh_token_datetime = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Tokens__3213E83FF333D097", x => x.id);
                    table.ForeignKey(
                        name: "Tokens_fk1",
                        column: x => x.user_ID,
                        principalTable: "User",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Question",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    question_type_ID = table.Column<int>(type: "int", nullable: false),
                    questionnaire_ID = table.Column<int>(type: "int", nullable: false),
                    text = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Question__3213E83F0FCDB0B0", x => x.id);
                    table.ForeignKey(
                        name: "Question_fk1",
                        column: x => x.question_type_ID,
                        principalTable: "QuestionType",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "Question_fk2",
                        column: x => x.questionnaire_ID,
                        principalTable: "Questionnaire",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireHistory",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    questionnaire_ID = table.Column<int>(type: "int", nullable: false),
                    user_ID = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    completed_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Question__3213E83F39C16374", x => x.id);
                    table.ForeignKey(
                        name: "QuestionnaireHistory_fk1",
                        column: x => x.questionnaire_ID,
                        principalTable: "Questionnaire",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "QuestionnaireHistory_fk2",
                        column: x => x.user_ID,
                        principalTable: "User",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "QuestionOption",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    question_ID = table.Column<int>(type: "int", nullable: false),
                    option_text = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionOption", x => x.id);
                    table.ForeignKey(
                        name: "QuestionOption_fk1",
                        column: x => x.question_ID,
                        principalTable: "Question",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Answer",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    anonymous_ID = table.Column<int>(type: "int", nullable: true),
                    user_ID = table.Column<int>(type: "int", nullable: true),
                    question_ID = table.Column<int>(type: "int", nullable: false),
                    text = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    select_option = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Answer__3213E83F3574F1FF", x => x.id);
                    table.ForeignKey(
                        name: "Answer_QuestionOption_fk",
                        column: x => x.select_option,
                        principalTable: "QuestionOption",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "Answer_fk1",
                        column: x => x.anonymous_ID,
                        principalTable: "Anonymous",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "Answer_fk2",
                        column: x => x.question_ID,
                        principalTable: "Question",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "Answer_fk3",
                        column: x => x.user_ID,
                        principalTable: "User",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "UQ__AccessLe__3213E83E880716CE",
                table: "AccessLevel",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Anonymou__3213E83E1D1C793D",
                table: "Anonymous",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Anonymou__69B13FDD8723FC70",
                table: "Anonymous",
                column: "session_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Answer_anonymous_ID",
                table: "Answer",
                column: "anonymous_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Answer_question_ID",
                table: "Answer",
                column: "question_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Answer_select_option",
                table: "Answer",
                column: "select_option");

            migrationBuilder.CreateIndex(
                name: "IX_Answer_user_ID",
                table: "Answer",
                column: "user_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Question_question_type_ID",
                table: "Question",
                column: "question_type_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Question_questionnaire_ID",
                table: "Question",
                column: "questionnaire_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Questionnaire_type_questionnaire_ID",
                table: "Questionnaire",
                column: "type_questionnaire_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Questionnaire_user_ID",
                table: "Questionnaire",
                column: "user_ID");

            migrationBuilder.CreateIndex(
                name: "UQ__Question__3213E83E8ABF1C9E",
                table: "Questionnaire",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireHistory_questionnaire_ID",
                table: "QuestionnaireHistory",
                column: "questionnaire_ID");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireHistory_user_ID",
                table: "QuestionnaireHistory",
                column: "user_ID");

            migrationBuilder.CreateIndex(
                name: "UQ__Question__3213E83E9914531B",
                table: "QuestionnaireHistory",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOption_question_ID",
                table: "QuestionOption",
                column: "question_ID");

            migrationBuilder.CreateIndex(
                name: "UQ__Question__3213E83E0D186B02",
                table: "QuestionType",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_user_ID",
                table: "Tokens",
                column: "user_ID");

            migrationBuilder.CreateIndex(
                name: "UQ__Tokens__3213E83E708D96DB",
                table: "Tokens",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Tokens__7FB69BAD6E2F1F3A",
                table: "Tokens",
                column: "refresh_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__TypeQues__3213E83E16F8D4FB",
                table: "TypeQuestionnaire",
                column: "id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_access_level_ID",
                table: "User",
                column: "access_level_ID");

            migrationBuilder.CreateIndex(
                name: "UQ__User__3213E83EB8CF6A85",
                table: "User",
                column: "id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Answer");

            migrationBuilder.DropTable(
                name: "QuestionnaireHistory");

            migrationBuilder.DropTable(
                name: "Tokens");

            migrationBuilder.DropTable(
                name: "QuestionOption");

            migrationBuilder.DropTable(
                name: "Anonymous");

            migrationBuilder.DropTable(
                name: "Question");

            migrationBuilder.DropTable(
                name: "QuestionType");

            migrationBuilder.DropTable(
                name: "Questionnaire");

            migrationBuilder.DropTable(
                name: "TypeQuestionnaire");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "AccessLevel");
        }
    }
}
