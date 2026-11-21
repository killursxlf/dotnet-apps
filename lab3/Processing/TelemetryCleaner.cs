using IoTTelemetryLab.Models;

namespace IoTTelemetryLab.Processing;

public static class TelemetryCleaner
{
    private const double ErrorMarker = -999.0;

    public static IEnumerable<TelemetryReading> RemoveInvalid(IEnumerable<TelemetryReading> raw)
        => raw.Where(r => !double.IsNaN(r.Value) && r.Value != ErrorMarker);
}
