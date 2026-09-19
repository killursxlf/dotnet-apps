using System.Text;
using IoTTelemetryLab.Data;
using IoTTelemetryLab.Models;
using IoTTelemetryLab.Processing;

Console.OutputEncoding = Encoding.UTF8;

const string deviceId = "sensor-08";
const SensorType sensorType = SensorType.WindSpeed;
var rawData = TelemetryGenerator.GenerateRawBatch(deviceId, count: 50, minValue: 0, maxValue: 35);
var cleaned = TelemetryCleaner.RemoveInvalid(rawData).ToList();
int warningCount = TelemetryAnalytics.CountWindWarnings(cleaned);

Console.WriteLine($"Варіант №8: швидкість вітру, м/с ({deviceId}, {sensorType})");
Console.WriteLine("Normal: v <= 15; Warning: 15 < v <= 25; Critical: v > 25");
Console.WriteLine($"Отримано показів: {rawData.Count}");
Console.WriteLine($"Після очищення: {cleaned.Count}");
Console.WriteLine($"Відкинуто некоректних показів: {rawData.Count - cleaned.Count}");
Console.WriteLine($"Кількість показів у діапазоні Warning: {warningCount}");
