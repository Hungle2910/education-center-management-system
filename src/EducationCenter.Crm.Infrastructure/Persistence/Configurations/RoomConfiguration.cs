using EducationCenter.Crm.Domain.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EducationCenter.Crm.Infrastructure.Persistence.Configurations;

public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.Capacity)
            .IsRequired();

        builder.HasData(
            new Room
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Phòng 101",
                Capacity = 15,
                CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Room
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Phòng 102",
                Capacity = 20,
                CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
