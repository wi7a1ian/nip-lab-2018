using System;

namespace Nip.Blog.Services.Posts.API.Exceptions
{
    public class BlogPostsDomainException : Exception
    {
        public BlogPostsDomainException()
        { }

        public BlogPostsDomainException(string message)
            : base(message)
        { }

        public BlogPostsDomainException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
