using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameBlogTagForeignKeyConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_blog_post_blog_tag_blog_tags_tags_temp_id",
                table: "blog_post_blog_tag");

            migrationBuilder.AddForeignKey(
                name: "fk_blog_post_blog_tag_blog_tags_tags_name",
                table: "blog_post_blog_tag",
                column: "tags_name",
                principalTable: "blog_tags",
                principalColumn: "name",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_blog_post_blog_tag_blog_tags_tags_name",
                table: "blog_post_blog_tag");

            migrationBuilder.AddForeignKey(
                name: "fk_blog_post_blog_tag_blog_tags_tags_temp_id",
                table: "blog_post_blog_tag",
                column: "tags_name",
                principalTable: "blog_tags",
                principalColumn: "name",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
