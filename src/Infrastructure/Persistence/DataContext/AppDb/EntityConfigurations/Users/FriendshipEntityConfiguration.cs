using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.DataContext.AppDb.EntityConfigurations.Users;

public class FriendshipEntityConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        builder.ToTable("Friendships");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();
        
        builder.HasOne(x => x.Requester)
            .WithMany()
            .HasForeignKey(x => x.RequesterId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(x => x.Responder)
            .WithMany()
            .HasForeignKey(x => x.ResponderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(f => new { f.RequesterId, f.ResponderId }).IsUnique();
        builder.HasIndex(x => x.RequesterId);
        builder.HasIndex(x => x.ResponderId);
        
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();
    }
}