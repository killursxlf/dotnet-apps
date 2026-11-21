using IoTTelemetryLab.Models;
using IoTTelemetryLab.Processing;

namespace IoTTelemetryLab.Data;

public static class DeviceSeeder
{
    public const int ReadingCount = 50;
    private static readonly string[] Directions = ["N", "E", "S", "W"];

    public static int SeedDevice(AppDbContext context, string name)
    {
        var raw = TelemetryGenerator.GenerateRawBatch(name, ReadingCount, minValue: 0, maxValue: 35);
        var device = new Device { Name = name };

        foreach (var reading in TelemetryCleaner.RemoveInvalid(raw))
        {
            device.Records.Add(new TelemetryRecord
            {
                Value = reading.Value,
                Timestamp = reading.Timestamp,
                Direction = Directions[Random.Shared.Next(Directions.Length)]
            });
        }

        context.Devices.Add(device);
        context.SaveChanges();
        return device.Records.Count;
    }
}
