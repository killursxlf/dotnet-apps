using IoTTelemetryLab.Models;
using Microsoft.EntityFrameworkCore;

namespace IoTTelemetryLab.Data;

public class AppDbContext : DbContext
{
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<TelemetryRecord> Records => Set<TelemetryRecord>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=iot_telemetry.db;Foreign Keys=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var devices = modelBuilder.Entity<Device>();
        devices.Property(d => d.Name).IsRequired().HasMaxLength(Device.MaxNameLength);
        devices.HasIndex(d => d.Name).IsUnique();

        var records = modelBuilder.Entity<TelemetryRecord>();
        records.Property(r => r.Direction).IsRequired().HasMaxLength(1);
        records
            .HasOne(r => r.Device)
            .WithMany(d => d.Records)
            .HasForeignKey(r => r.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
