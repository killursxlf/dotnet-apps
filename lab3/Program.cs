using System.Globalization;
using System.Text;
using IoTTelemetryLab.Data;
using IoTTelemetryLab.Models;
using Microsoft.EntityFrameworkCore;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;

using (var context = new AppDbContext())
    context.Database.Migrate();

Console.WriteLine("Лабораторна робота №3, варіант №8. Телеметрія швидкості вітру.");
while (true)
{
    Console.WriteLine("""

        1 — Додати сенсор
        2 — Переглянути сенсори та покази
        3 — Перейменувати сенсор
        4 — Видалити сенсор і його покази
        5 — Видалити покази за умовою
        6 — Додати 5 демонстраційних сенсорів
        7 — Статистика за напрямками вітру
        0 — Вихід
        """);
    var command = Read("Оберіть дію: ");
    if (command is null or "0") break;

    using var context = new AppDbContext();
    switch (command)
    {
        case "1":
            var name = Read("Назва сенсора (порожній рядок — скасувати): ");
            if (!string.IsNullOrEmpty(name)) AddDevice(context, name);
            break;
        case "2": ReadDevice(context); break;
        case "3": RenameDevice(context); break;
        case "4": DeleteDevice(context); break;
        case "5": DeleteReadings(context); break;
        case "6":
            for (int i = 1; i <= 5; i++) AddDevice(context, $"sensor-{i:D2}");
            break;
        case "7": ShowDirectionStats(context); break;
        default: Console.WriteLine("Невідома дія."); break;
    }
}

static void AddDevice(AppDbContext context, string name)
{
    if (!CanUseName(context, name)) return;

    var saved = DeviceSeeder.SeedDevice(context, name);
    Console.WriteLine($"'{name}': згенеровано {DeviceSeeder.ReadingCount}, збережено {saved}.");
}

static bool CanUseName(AppDbContext context, string name)
{
    if (name.Length > Device.MaxNameLength)
    {
        Console.WriteLine($"Назва задовга: максимум {Device.MaxNameLength} символів.");
        return false;
    }

    if (context.Devices.Any(d => d.Name == name))
    {
        Console.WriteLine($"Сенсор '{name}' уже є в базі.");
        return false;
    }

    return true;
}

static bool ListDevices(AppDbContext context)
{
    var devices = context.Devices.OrderBy(d => d.Id)
        .Select(d => new { d.Id, d.Name, Count = d.Records.Count })
        .ToList();
    if (devices.Count == 0)
    {
        Console.WriteLine("У базі немає сенсорів.");
        return false;
    }

    foreach (var device in devices)
        Console.WriteLine($"  [{device.Id}] {device.Name} (показів: {device.Count})");
    return true;
}

static Device? SelectDevice(AppDbContext context)
{
    if (!ListDevices(context)) return null;

    var input = Read("Id сенсора (порожній рядок — скасувати): ");
    if (string.IsNullOrEmpty(input)) return null;
    if (int.TryParse(input, out var id) && id > 0) return FindDevice(context, id);

    Console.WriteLine("Введіть додатний числовий Id.");
    return null;
}

static Device? FindDevice(AppDbContext context, int id)
{
    var device = context.Devices.FirstOrDefault(d => d.Id == id);
    if (device is null) Console.WriteLine($"Сенсора з Id={id} немає.");
    return device;
}

static void ReadDevice(AppDbContext context)
{
    var device = SelectDevice(context);
    if (device is null) return;

    var readings = context.Records.Where(r => r.DeviceId == device.Id)
        .OrderBy(r => r.Timestamp).ThenBy(r => r.Id).ToList();
    Console.WriteLine($"Покази сенсора '{device.Name}' (UTC):");
    foreach (var reading in readings) PrintReading(reading);
    if (readings.Count == 0) Console.WriteLine("Показів немає.");
    Console.WriteLine($"Усього показів: {readings.Count}.");
}

static void RenameDevice(AppDbContext context)
{
    var device = SelectDevice(context);
    if (device is null) return;

    var name = Read($"Нова назва для '{device.Name}' (порожній рядок — скасувати): ");
    if (string.IsNullOrEmpty(name)) return;
    if (name == device.Name)
    {
        Console.WriteLine("Назва не змінилася.");
        return;
    }
    if (!CanUseName(context, name)) return;

    var previousName = device.Name;
    device.Name = name;
    context.SaveChanges();
    Console.WriteLine($"Перейменовано '{previousName}' на '{name}'.");
}

static void DeleteDevice(AppDbContext context)
{
    var device = SelectDevice(context);
    if (device is null) return;

    var totalBefore = context.Records.Count();
    var related = context.Records.Count(r => r.DeviceId == device.Id);
    if (!Confirm($"Видалити '{device.Name}' і {related} пов’язаних показів?")) return;

    context.Devices.Remove(device);
    context.SaveChanges();

    var totalAfter = context.Records.Count();
    var remaining = context.Records.Count(r => r.DeviceId == device.Id);
    var orphans = context.Records.Count(r => !context.Devices.Any(d => d.Id == r.DeviceId));
    Console.WriteLine($"Сенсор '{device.Name}' видалено. Показів у базі: було {totalBefore}, стало {totalAfter}.");
    Console.WriteLine($"Показів видаленого сенсора: {remaining}. Записів без сенсора: {orphans}.");
}

static void DeleteReadings(AppDbContext context)
{
    if (!ListDevices(context)) return;
    var input = Read("Id сенсора (0 — усі, порожній рядок — скасувати): ");
    if (string.IsNullOrEmpty(input)) return;
    if (!int.TryParse(input, out var id) || id < 0)
    {
        Console.WriteLine("Введіть 0 або додатний числовий Id.");
        return;
    }

    IQueryable<TelemetryRecord> query = context.Records;
    if (id != 0)
    {
        if (FindDevice(context, id) is null) return;
        query = query.Where(r => r.DeviceId == id);
    }

    Console.WriteLine("1 — нижче порога | 2 — вище порога | 3 — у діапазоні | 4 — раніше дати (UTC)");
    switch (Read("Умова: "))
    {
        case "1":
            if (!TryReadNumber("Поріг: ", out var below)) return;
            query = query.Where(r => r.Value < below);
            break;
        case "2":
            if (!TryReadNumber("Поріг: ", out var above)) return;
            query = query.Where(r => r.Value > above);
            break;
        case "3":
            if (!TryReadNumber("Нижня межа: ", out var min) ||
                !TryReadNumber("Верхня межа: ", out var max)) return;
            if (min > max)
            {
                Console.WriteLine("Нижня межа не може бути більшою за верхню.");
                return;
            }
            query = query.Where(r => r.Value >= min && r.Value <= max);
            break;
        case "4":
            string[] formats = ["yyyy-MM-dd", "yyyy-MM-dd HH:mm"];
            if (!DateTime.TryParseExact(Read("Дата (yyyy-MM-dd або yyyy-MM-dd HH:mm, UTC): "),
                    formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var before))
            {
                Console.WriteLine("Невірний формат дати.");
                return;
            }
            query = query.Where(r => r.Timestamp < before);
            break;
        default:
            Console.WriteLine("Невідома умова.");
            return;
    }

    var matches = query.OrderBy(r => r.Timestamp).ThenBy(r => r.Id).ToList();
    if (matches.Count == 0)
    {
        Console.WriteLine("Показів за цією умовою немає.");
        return;
    }

    Console.WriteLine($"Знайдено показів: {matches.Count}.");
    foreach (var reading in matches.Take(5)) PrintReading(reading);
    if (matches.Count > 5) Console.WriteLine($"  ... та ще {matches.Count - 5}.");
    if (!Confirm($"Видалити {matches.Count} показів?")) return;

    var totalBefore = context.Records.Count();
    context.Records.RemoveRange(matches);
    context.SaveChanges();
    Console.WriteLine($"Видалено показів: {matches.Count}. У базі: було {totalBefore}, стало {context.Records.Count()}.");
    Console.WriteLine($"Ще відповідають умові: {query.Count()}.");
}

static void ShowDirectionStats(AppDbContext context)
{
    var directions = context.Records
        .Where(r => r.Direction != "")
        .GroupBy(r => r.Direction)
        .Select(g => new { Direction = g.Key, Count = g.Count(), Average = g.Average(r => r.Value) })
        .OrderByDescending(g => g.Average)
        .ThenBy(g => g.Direction)
        .ToList();

    Console.WriteLine("=== Статистика за напрямками вітру ===");
    if (directions.Count == 0) Console.WriteLine("Немає даних із заданим напрямком.");
    foreach (var entry in directions)
        Console.WriteLine($"  {entry.Direction}: показів {entry.Count}, середня швидкість {entry.Average:F2} м/с");
}

static bool TryReadNumber(string prompt, out double number)
{
    if (double.TryParse(Read(prompt)?.Replace(',', '.'), NumberStyles.Float,
            CultureInfo.InvariantCulture, out number) && double.IsFinite(number))
        return true;

    Console.WriteLine("Введіть скінченне число, наприклад 12,5.");
    return false;
}

static bool Confirm(string question)
{
    if (string.Equals(Read($"{question} (y/n): "), "y", StringComparison.OrdinalIgnoreCase))
        return true;
    Console.WriteLine("Видалення скасовано.");
    return false;
}

static void PrintReading(TelemetryRecord reading)
{
    var direction = string.IsNullOrEmpty(reading.Direction) ? "—" : reading.Direction;
    Console.WriteLine($"  Id={reading.Id}, сенсор={reading.DeviceId}, {reading.Timestamp:yyyy-MM-dd HH:mm:ss}, {reading.Value:F2} м/с, {direction}");
}

static string? Read(string prompt)
{
    Console.Write(prompt);
    return Console.ReadLine()?.Trim();
}
