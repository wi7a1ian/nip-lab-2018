using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Nip.Blog.Services.Posts.API.Models
{
    public class Entity
    {
        public long Id { get; set; }

        [Timestamp]
        [IgnoreDataMember]
        public byte[] RowVersion { get; set; }
    }
}
