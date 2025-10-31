using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.DataContext.AppDb.EntityConfigurations.Users;

public class AppUserEntityConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();
        
        builder.Property(x => x.UserName)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(x => x.DateOfBirth)
            .IsRequired();
        
        builder.Property(x => x.LastActive)
            .IsRequired();
        
        builder.HasIndex(x => x.Id).IsUnique();
        builder.HasIndex(x => x.UserName).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique();
        
        builder.HasMany(x => x.Posts)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.PostLikes)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(x => x.PostComments)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();
    }
}