namespace IoTTelemetryLab.Models;

public class Device
{
    public const int MaxNameLength = 100;

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<TelemetryRecord> Records { get; set; } = new();
}
