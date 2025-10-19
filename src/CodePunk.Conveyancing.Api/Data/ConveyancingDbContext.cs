using CodePunk.Conveyancing.Api.Domain;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using CodePunk.Conveyancing.Api.Infrastructure.Tenancy;

namespace CodePunk.Conveyancing.Api.Data;

public sealed class ConveyancingDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public ConveyancingDbContext(DbContextOptions<ConveyancingDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Conveyance> Conveyances => Set<Conveyance>();
    public DbSet<DraftDocument> Drafts => Set<DraftDocument>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<ConveyanceContact> ConveyanceContacts => Set<ConveyanceContact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Slug).IsRequired().HasMaxLength(100);
            b.HasIndex(x => x.Slug).IsUnique();
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
        });

        modelBuilder.Entity<Conveyance>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property<Guid>("TenantId");
            b.HasIndex("TenantId");
            b.Property(x => x.BuyerName).HasMaxLength(256);
            b.Property(x => x.SellerName).HasMaxLength(256);
            b.Property(x => x.PropertyAddress).HasMaxLength(1024);
            b.HasQueryFilter(e => EF.Property<Guid>(e, "TenantId") == (_tenantProvider.TenantId ?? Guid.Empty));
        });

        modelBuilder.Entity<DraftDocument>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property<Guid>("TenantId");
            b.HasIndex("TenantId");
            b.Property(x => x.Type).HasConversion<int>();
            b.Property(x => x.Status).HasConversion<int>();
            b.Property(x => x.ContentMarkdown).HasColumnType("TEXT");
            b.Property(x => x.CreatedBy).HasMaxLength(128);
            b.Property(x => x.ApprovedBy).HasMaxLength(128);
            b.Property(x => x.RejectedBy).HasMaxLength(128);

            var dictConverter = new ValueConverter<Dictionary<string, string>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>()
            );
            b.Property(x => x.Metadata).HasConversion(dictConverter).HasColumnType("TEXT");

            b.HasIndex(x => x.ConveyanceId);
            b.HasQueryFilter(e => EF.Property<Guid>(e, "TenantId") == (_tenantProvider.TenantId ?? Guid.Empty));
        });

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property<Guid>("TenantId");
            b.HasIndex("TenantId");
            b.Property(x => x.Status).HasConversion<int>();
            b.Property(x => x.Subject).HasMaxLength(500);
            b.Property(x => x.BodyMarkdown).HasColumnType("TEXT");

            var listConverter = new ValueConverter<List<string>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
            );
            b.Property(x => x.ToRecipients).HasConversion(listConverter).HasColumnType("TEXT");

            b.HasIndex(x => x.ConveyanceId);
            b.HasIndex(x => new { x.ConveyanceId, x.Status });
            b.HasQueryFilter(e => EF.Property<Guid>(e, "TenantId") == (_tenantProvider.TenantId ?? Guid.Empty));
        });

        modelBuilder.Entity<Contact>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property<Guid>("TenantId");
            b.HasIndex("TenantId");
            b.Property(x => x.Name).HasMaxLength(256);
            b.Property(x => x.Email).HasMaxLength(256);
            b.Property(x => x.Phone).HasMaxLength(64);
            b.HasIndex("TenantId", nameof(Contact.Email));
            b.HasQueryFilter(e => EF.Property<Guid>(e, "TenantId") == (_tenantProvider.TenantId ?? Guid.Empty));
        });

        modelBuilder.Entity<ConveyanceContact>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property<Guid>("TenantId");
            b.HasIndex("TenantId");
            b.Property(x => x.Role).HasConversion<int>();
            b.HasIndex(x => new { x.ConveyanceId, x.ContactId });
            b.HasIndex(x => new { x.ConveyanceId, x.Role });
            b.HasIndex("TenantId", nameof(ConveyanceContact.ConveyanceId), nameof(ConveyanceContact.ContactId), nameof(ConveyanceContact.Role)).IsUnique();
            b.HasQueryFilter(e => EF.Property<Guid>(e, "TenantId") == (_tenantProvider.TenantId ?? Guid.Empty));
        });
    }
}
