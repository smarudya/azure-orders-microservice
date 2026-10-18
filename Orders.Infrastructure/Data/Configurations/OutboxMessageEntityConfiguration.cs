using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Orders.Infrastructure.Data.Configurations;

internal sealed class OutboxMessageEntityConfiguration : IEntityTypeConfiguration<Data.OutboxMessage>
{
    public void Configure(EntityTypeBuilder<Data.OutboxMessage> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.OccurredAtUtc).IsRequired();
        builder.HasIndex(x => x.ProcessedAtUtc);
    }
}
