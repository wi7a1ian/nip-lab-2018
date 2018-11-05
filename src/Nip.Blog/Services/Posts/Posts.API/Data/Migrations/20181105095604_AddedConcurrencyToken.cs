using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Nip.Blog.Services.Posts.API.Data.Migrations
{
    public partial class AddedConcurrencyToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogPostComment_BlogPosts_BlogPostId",
                table: "BlogPostComment");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "BlogPosts",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "BlogPostId",
                table: "BlogPostComment",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "BlogPostComment",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BlogPostComment_BlogPosts_BlogPostId",
                table: "BlogPostComment",
                column: "BlogPostId",
                principalTable: "BlogPosts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogPostComment_BlogPosts_BlogPostId",
                table: "BlogPostComment");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "BlogPostComment");

            migrationBuilder.AlterColumn<long>(
                name: "BlogPostId",
                table: "BlogPostComment",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BlogPostComment_BlogPosts_BlogPostId",
                table: "BlogPostComment",
                column: "BlogPostId",
                principalTable: "BlogPosts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
