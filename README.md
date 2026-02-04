| **[JPKusumi.com](https://jpkusumi.com) presents—** |
|:---------------------:|

# UtcMilliTime

UtcMilliTime is a C# time component (software-defined clock) that yields Unix time milliseconds (`Int64`) timestamps, similar to JavaScript's `Date.now()`. It synchronizes with NTP servers and is cross-platform for .NET 8 + .NET 10+, supporting async `Main`. Mock-friendly via the `ITime` interface.

On NuGet at: https://www.nuget.org/packages/UtcMilliTime/  
On GitHub at: https://github.com/JPKusumi/UtcMilliTime

## Versions
- **2.2.2**: Patch for NuGet to pick up README.md. Includes all v2.2.1 features.
- **2.2.1**: Fixed nullability warnings and improved NuGet README display. Includes all v2.2.0 features (chaining extensions).
- **2.2.0**: Added chaining extensions for Unix timestamps (add/subtract for days, hours, minutes, seconds). Updated README with resources link to JPKusumi.com.
- **2.1.0**: Ready for .NET 10; still good for .NET 8+. Accuracy: 1ms (improved precision)
- **2.0.0**: First update in six years went cross-platform. Good for .NET 8+.
- **1.0.1**: .NET Standard 2.0 (Windows-only, .NET Framework 4.6.1+, .NET Core 2.0+).

## Overview
UtcMilliTime provides `Int64` timestamps (milliseconds since 1/1/1970 UTC, excluding leap seconds), avoiding the Year 2038 problem with 64-bit integers. It initializes with device time and syncs with NTP servers (default: `pool.ntp.org`) when permitted, ignoring user-changeable device time thereafter. Supports ISO-8601 string conversion via `ToIso8601String`.

**Note**: UtcMilliTime uses a singleton pattern—the clock is shared across the app. All accesses (static or via `CreateAsync`) refer to the same instance after initialization.

## Installation
```
dotnet add package UtcMilliTime --version 2.2.2
```
For legacy projects:
```
dotnet add package UtcMilliTime --version 1.0.1
```
## Usage
By default, the clock initializes with device time and leaves the network alone.
```
using UtcMilliTime;
  
  ITime time = Clock.Time; // Shorthand for repeated access to the singleton
  time.SuppressNetworkCalls = false; // Enable NTP sync (durable for runtime; execute once)
  var timestamp = time.Now; // Int64 timestamp
  string iso = timestamp.ToIso8601String(); // 2025-07-10T13:00:00.123Z
```
**Important**: `SuppressNetworkCalls = false` grants permission for NTP synchronization. The clock starts with device time; after permission and connectivity, it self-updates to network time. This setting persists for the app's lifetime and must be set explicitly (defaults to true to avoid unintended network use).

With permission, and subject to connectivity, the clock will synchronize itself to network time.

### Chaining Extensions for Unix Timestamps
New in v2.2: Fluent methods on `long` for easy additions and subtractions, ideal for calculating expiration times in JWTs or other auth flows. These operate on Unix seconds (after calling `ToUnixTimeSeconds`).

```csharp
using UtcMilliTime;

long nowMilli = Clock.Time.Now;
long iatSeconds = nowMilli.ToUnixTimeSeconds();  // Current time in seconds
long futureSeconds = nowMilli.ToUnixTimeSeconds().AddDays(7).AddHours(1).AddMinutes(30).AddSeconds(45);  // +7 days, 1 hour, 30 min, 45 sec
long pastSeconds = nowMilli.ToUnixTimeSeconds().SubtractDays(30).SubtractHours(12);  // -30 days and 12 hours
```

NTP Sync Note: By default, `SuppressNetworkCalls = true` (uses device time only). To enable NTP:
```csharp
await Clock.CreateAsync();
Clock.Time.SuppressNetworkCalls = false;  // Allows sync if connected
```

### Supporting Async Main
For async initialization in contexts like `async Main` (returns the shared clock instance):
```
static async Task Main(string[] args)
{
    var clock = await Clock.CreateAsync();
    clock.SuppressNetworkCalls = false; // Enable sync (triggers SelfUpdateAsync if indicated)
    Console.WriteLine($"Synchronized: {clock.Synchronized}, Time: {clock.Now}, ISO: {clock.Now.ToIso8601String()}");
    // For custom server: await clock.SelfUpdateAsync("custom.ntp.org");
}
```
**Note**: `CreateAsync` initializes and returns the singleton clock (using device time). Synchronization happens only after setting `SuppressNetworkCalls = false` (via the setter's logic) or manual `SelfUpdateAsync` calls. This ensures no unintended network traffic.

### NetworkTimeAcquired Event
Subscribe to events on the shared instance:
```
Clock.Time.NetworkTimeAcquired += (sender, e) => Console.WriteLine($"Synced with {e.Server}, Skew: {e.Skew}ms");
```
### Notes  
- **Silent Failure**: `SelfUpdateAsync` fails silently if connectivity is absent. Check `Synchronized` for success.  
- **Leap Seconds**: Clock advances during leap seconds, appearing 1 second ahead. Call `SelfUpdateAsync()` to resync.  
- **Performance**: Use `Now` for maximum performance; `ToIso8601String` is slower due to `DateTime`.

### Upgrading from 1.0.1
Version 2.2.2: Improved NuGet README display. Public API unchanged (static `Clock.Time.Now` still works as a singleton).

Migration: Static usage remains the same; for async Main use `await Clock.CreateAsync()`—it returns the shared clock.

### Technical Details
Calculates with `Stopwatch.GetTimestamp` for high resolution uptime and `DateTime.UtcNow` for device time. Now is calculated as `device_boot_time + GetHighResUptime`. The clock is a singleton to ensure consistent time across the app.

## Resources and Community
For more information, blog posts, and updates on this and other JP Kusumi creations, visit the [JPKusumi.com](https://jpkusumi.com). Recent blog posts include:
- Key and nonce management best practices.
- Handling cryptographic metadata securely.
- Encrypting JWTs with post-quantum tools.

JPKusumi.com aims to be a resource for developers. There is also a discussion forum, open in the [GitHub repo for GreenfieldPQC](https://github.com/JPKusumi/GreenfieldPQC/discussions). Comments and feedback may be directed there.

### License
MIT License