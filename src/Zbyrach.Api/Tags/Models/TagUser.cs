using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zbyrach.Api.Account;

namespace Zbyrach.Api.Tags
{
    public class TagUser
    {
        public long TagId { get; set; }
        public Tag Tag { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
    }

    public class TagUserConfiguration : IEntityTypeConfiguration<TagUser>
    {
        public void Configure(EntityTypeBuilder<TagUser> builder)
        {
            builder
                .HasKey(tu => new { tu.UserId, tu.TagId });
            builder
                .HasOne(tu => tu.User)
                .WithMany(u => u.TagUsers)
                .HasForeignKey(tu => tu.UserId);
            builder
                .HasOne(tu => tu.Tag)
                .WithMany(t => t.TagUsers)
                .HasForeignKey(tu => tu.TagId);
        }
    }
}