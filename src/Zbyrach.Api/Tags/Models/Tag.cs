using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zbyrach.Api.Articles;

namespace Zbyrach.Api.Tags
{
    public class Tag : Entity
    {
        public string Name { get; set; } = default!;
        public ICollection<TagUser> TagUsers { get; set; } = default!;
        public ICollection<ArticleTag> ArticleTags { get; set; } = default!;
        public override string ToString() => Name;
    }

    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder
                .Property(p => p.Id)
                .IsRequired();
            builder
                .Property(p => p.Name)
                .IsRequired();
        }
    }
}