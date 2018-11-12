using Moq;
using Nip.Blog.Services.Posts.API.Controllers;
using Nip.Blog.Services.Posts.API.Models;
using Nip.Blog.Services.Posts.API.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace Posts.UnitTests
{
    public class BlogPostsV2ControllerTests
    {
        [Fact]
        public async Task ShouldReturnEmptyPageWhenCallingGetWithParamsOnEmptyRepo()
        {
            // Given
            var mockLogger = new Mock<ILogger<BlogPostsV2Controller>>();
            var mockRepo = new Mock<IBlogPostRepository>();
            mockRepo.Setup(repo => repo.GetAllPagedAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .ReturnsAsync(new PaginatedItems<BlogPost>());
            var controller = new BlogPostsV2Controller(mockLogger.Object, mockRepo.Object);

            // When
            var result = await controller.Get(0, 5);

            // Then
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaginatedItems<BlogPost>>(actionResult.Value);
            Assert.Null(returnValue.Items);
            Assert.Equal(0, returnValue.PageIndex);
            Assert.Equal(0, returnValue.PageSize);
            Assert.Equal(0, returnValue.TotalItems);
            Assert.Null(returnValue.NextPage);
        }

        [Fact]
        public async Task ShouldReturnResultWithPageOfBlogPostsWhenCallingGetWithParams()
        {
            // Given
            var expectedPage = new PaginatedItems<BlogPost>() {
                Items = new List<BlogPost> { new BlogPost() },
                PageSize = 1,
                PageIndex = 0,
                TotalItems = 1
            };
            var mockLogger = new Mock<ILogger<BlogPostsV2Controller>>();
            var mockRepo = new Mock<IBlogPostRepository>();
            mockRepo.Setup(repo => repo.GetAllPagedAsync(It.IsAny<int>(), It.IsAny<int>(), null))
                .ReturnsAsync(expectedPage);
            var controller = new BlogPostsV2Controller(mockLogger.Object, mockRepo.Object);

            // When
            var result = await controller.Get(0, 5);

            // Then
            var actionResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaginatedItems<BlogPost>>(actionResult.Value);
            Assert.NotEmpty(returnValue.Items);
            Assert.Equal(expectedPage.PageIndex, returnValue.PageIndex);
            Assert.Equal(expectedPage.PageSize, returnValue.PageSize);
            Assert.Equal(expectedPage.TotalItems, returnValue.TotalItems);
            Assert.Equal(expectedPage.NextPage, returnValue.NextPage);
        }

        [Fact]
        public async Task ShouldReturnNotFoundResultWhenCallingGetForNonexistingItem()
        {
            // Given
            var mockLogger = new Mock<ILogger<BlogPostsV2Controller>>();
            var mockRepo = new Mock<IBlogPostRepository>();
            mockRepo.Setup(repo => repo.GetAsync(It.IsAny<int>())).ReturnsAsync((BlogPost)null);
            var controller = new BlogPostsV2Controller(mockLogger.Object, mockRepo.Object);
            long nonExistingBlogPostId = 123123;

            // When
            var result = await controller.Get(nonExistingBlogPostId);

            // Then
            var actionResult = Assert.IsType<ActionResult<BlogPost>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task ShouldCreatedAtActionResultWhenCallingPostWith()
        {
            // Given
            var mockLogger = new Mock<ILogger<BlogPostsV2Controller>>();
            var mockRepo = new Mock<IBlogPostRepository>();
            mockRepo.Setup(repo => repo.AddAsync(It.IsAny<BlogPost>()))
                .Returns(Task.CompletedTask).Verifiable();
            var controller = new BlogPostsV2Controller(mockLogger.Object, mockRepo.Object);
            var somePost = new BlogPost { Title = "Some Post", Description = "Dest" };

            // When
            var result = await controller.Post(somePost);

            // Then
            var actionResult = Assert.IsType<CreatedAtRouteResult>(result);
            var valueResult = Assert.IsType<BlogPost>(actionResult.Value);
            Assert.Equal(somePost, valueResult);
            mockRepo.Verify();
        }

        [Fact]
        public async Task ShouldReturnNotFoundResultWhenCallingDeleteForNonexistingItem()
        {
            // Given
            var nonExistingBlogPostId = 123123;
            var mockLogger = new Mock<ILogger<BlogPostsV2Controller>>();
            var mockRepo = new Mock<IBlogPostRepository>();
            mockRepo.Setup(repo => repo.GetAsync(nonExistingBlogPostId))
                .ReturnsAsync((BlogPost)null).Verifiable();
            var controller = new BlogPostsV2Controller(mockLogger.Object, mockRepo.Object);

            // When
            var result = await controller.Delete(nonExistingBlogPostId);

            // Then
            Assert.IsType<NotFoundResult>(result);
            mockRepo.Verify();
        }


    }
}
