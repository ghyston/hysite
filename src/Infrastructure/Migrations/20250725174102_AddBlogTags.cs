using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "blog_tags",
                columns: table => new
                {
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_tags", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "blog_post_blog_tag",
                columns: table => new
                {
                    blog_posts_id = table.Column<int>(type: "integer", nullable: false),
                    tags_name = table.Column<string>(type: "character varying(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_post_blog_tag", x => new { x.blog_posts_id, x.tags_name });
                    table.ForeignKey(
                        name: "fk_blog_post_blog_tag_blog_posts_blog_posts_id",
                        column: x => x.blog_posts_id,
                        principalTable: "blog_posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_blog_post_blog_tag_blog_tags_tags_temp_id",
                        column: x => x.tags_name,
                        principalTable: "blog_tags",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_blog_post_blog_tag_tags_name",
                table: "blog_post_blog_tag",
                column: "tags_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blog_post_blog_tag");

            migrationBuilder.DropTable(
                name: "blog_tags");
        }
    }
}
