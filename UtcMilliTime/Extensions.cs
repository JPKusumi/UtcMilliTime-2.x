using System;
using System.Threading.Tasks;

namespace UtcMilliTime;

#pragma warning disable RECS0165 // Async methods should return a Task (async void)

/// <summary>
/// Extension methods for working with UtcMilliTime (64-bit Unix timestamps in UTC milliseconds).
/// </summary>
public static partial class Extensions
{
    /// <summary>
    /// Converts a UtcMilliTime timestamp to an ISO-8601 string.
    /// </summary>
    /// <param name="timestamp">The UtcMilliTime value.</param>
    /// <param name="suppressMilliseconds">If true, omits milliseconds.</param>
    /// <returns>String like "2019-08-10T22:08:14.102Z".</returns>
    public static string ToIso8601String(this long timestamp, bool suppressMilliseconds = false)
    {
        long ticks = (timestamp + Constants.dotnet_to_unix_milliseconds) * Constants.dotnet_ticks_per_millisecond;
        var dateTime = new DateTime(ticks, DateTimeKind.Utc);
        return suppressMilliseconds
            ? dateTime.ToString(Constants.iso_8601_without_milliseconds)
            : dateTime.ToString(Constants.iso_8601_with_milliseconds);
    }

    /// <summary>
    /// Converts a DateTime to UtcMilliTime (truncates fractional ms).
    /// </summary>
    public static long ToUtcMilliTime(this DateTime given) => (given.ToUniversalTime().Ticks / Constants.dotnet_ticks_per_millisecond) - Constants.dotnet_to_unix_milliseconds;

    /// <summary>
    /// Converts a DateTimeOffset to UtcMilliTime (truncates fractional ms).
    /// </summary>
    public static long ToUtcMilliTime(this DateTimeOffset given) => given.ToUnixTimeMilliseconds();

    /// <summary>
    /// Converts a TimeSpan interval to UtcMilliTime (truncates fractional ms).
    /// </summary>
    public static long ToUtcMilliTime(this TimeSpan given) => (long)given.TotalMilliseconds;

    /// <summary>
    /// Converts UnixTimeSeconds to UtcMilliTime (multiplies by 1000).
    /// </summary>
    public static long ToUtcMilliTime(this long unixtimeSeconds) => unixtimeSeconds * 1000;

    /// <summary>
    /// Truncates UtcMilliTime to UnixTimeSeconds (divides by 1000).
    /// </summary>
    public static long ToUnixTime(this long timestamp) => timestamp / 1000;

    /// <summary>
    /// Extracts the millisecond part (0-999) from a UtcMilliTime timestamp.
    /// </summary>
    public static short MillisecondPart(this long timestamp) => (short)(timestamp % 1000);

    /// <summary>
    /// Converts UtcMilliTime to a UTC DateTime.
    /// </summary>
    public static DateTime ToUtcDateTime(this long timestamp) => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(timestamp);

    /// <summary>
    /// Converts UtcMilliTime to a local DateTime.
    /// </summary>
    public static DateTime ToLocalDateTime(this long timestamp) => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(timestamp).ToLocalTime();

    /// <summary>
    /// Converts UtcMilliTime to a DateTimeOffset (UTC, offset 0).
    /// </summary>
    public static DateTimeOffset ToDateTimeOffset(this long timestamp) => new(timestamp.ToUtcDateTime());

    /// <summary>
    /// Converts a UtcMilliTime interval to a TimeSpan (or from 1970 if absolute).
    /// </summary>
    public static TimeSpan ToTimeSpan(this long interval) => new(interval * Constants.dotnet_ticks_per_millisecond);
    /// <summary>
    /// The whole number of days in an interval found in UtcMilliTime
    /// </summary>
    /// <param name="interval">UtcMilliTime</param>
    /// <returns>int</returns>
    public static int IntervalDays(this long interval)
    {
        return TimeSpan.FromMilliseconds(interval).Days;
    }
    /// <summary>
    /// Whole hours in the remainder after days are removed from an interval
    /// </summary>
    /// <param name="interval">UtcMilliTime</param>
    /// <returns>int</returns>
    public static int IntervalHoursPart(this long interval)
    {
        return TimeSpan.FromMilliseconds(interval).Hours;
    }
    /// <summary>
    /// Whole minutes in the remainder after days and hours are removed from an interval
    /// </summary>
    /// <param name="interval">UtcMilliTime</param>
    /// <returns>int</returns>
    public static int IntervalMinutesPart(this long interval)
    {
        return TimeSpan.FromMilliseconds(interval).Minutes;
    }
    /// <summary>
    /// Whole seconds in the remainder after removing days, hours, and minutes from an interval
    /// </summary>
    /// <param name="interval">UtcMilliTime</param>
    /// <returns>int</returns>
    public static int IntervalSecondsPart(this long interval)
    {
        return TimeSpan.FromMilliseconds(interval).Seconds;
    }
    // Additive and subtractive operations for chaining in auth/timing flows (operate on Unix seconds for JWT claims, etc.)
    /// <summary>
    /// Truncates UtcMilliTime to UnixTimeSeconds (divides by 1000). Alias for ToUnixTime for explicitness.
    /// </summary>
    public static long ToUnixTimeSeconds(this long timestamp) => timestamp.ToUnixTime();

    /// <summary>
    /// Adds the specified number of days to a Unix time in seconds, returning a new timestamp.
    /// </summary>
    /// <param name="unixSeconds">The Unix time in seconds.</param>
    /// <param name="days">The number of days to add (must be non-negative).</param>
    /// <returns>A new Unix time in seconds.</returns>
    public static long AddDays(this long unixSeconds, int days)
    {
        if (days < 0) throw new ArgumentOutOfRangeException(nameof(days), "Days must be non-negative.");
        return unixSeconds + (days * 86400L);  // 86,400 seconds per day
    }

    /// <summary>
    /// Subtracts the specified number of days from a Unix time in seconds, returning a new timestamp.
    /// </summary>
    /// <param name="unixSeconds">The Unix time in seconds.</param>
    /// <param name="days">The number of days to subtract (must be non-negative).</param>
    /// <returns>A new Unix time in seconds.</returns>
    public static long SubtractDays(this long unixSeconds, int days)
    {
        if (days < 0) throw new ArgumentOutOfRangeException(nameof(days), "Days must be non-negative.");
        return unixSeconds - (days * 86400L);
    }

    /// <summary>
    /// Adds the specified number of hours to a Unix time in seconds, returning a new timestamp.
    /// </summary>
    /// <param name="unixSeconds">The Unix time in seconds.</param>
    /// <param name="hours">The number of hours to add (must be non-negative).</param>
    /// <returns>A new Unix time in seconds.</returns>
    public static long AddHours(this long unixSeconds, int hours)
    {
        if (hours < 0) throw new ArgumentOutOfRangeException(nameof(hours), "Hours must be non-negative.");
        return unixSeconds + (hours * 3600L);
    }

    /// <summary>
    /// Subtracts the specified number of hours from a Unix time in seconds, returning a new timestamp.
    /// </summary>
    /// <param name="unixSeconds">The Unix time in seconds.</param>
    /// <param name="hours">The number of hours to subtract (must be non-negative).</param>
    /// <returns>A new Unix time in seconds.</returns>
    public static long SubtractHours(this long unixSeconds, int hours)
    {
        if (hours < 0) throw new ArgumentOutOfRangeException(nameof(hours), "Hours must be non-negative.");
        return unixSeconds - (hours * 3600L);
    }

    /// <summary>
    /// Adds the specified number of minutes to a Unix time in seconds, returning a new timestamp.
    /// </summary>
    /// <param name="unixSeconds">The Unix time in seconds.</param>
    /// <param name="minutes">The number of minutes to add (must be non-negative).</param>
    /// <returns>A new Unix time in seconds.</returns>
    public static long AddMinutes(this long unixSeconds, int minutes)
    {
        if (minutes < 0) throw new ArgumentOutOfRangeException(nameof(minutes), "Minutes must be non-negative.");
        return unixSeconds + (minutes * 60L);
    }

    /// <summary>
    /// Subtracts the specified number of minutes from a Unix time in seconds, returning a new timestamp.
    /// </summary>
    /// <param name="unixSeconds">The Unix time in seconds.</param>
    /// <param name="minutes">The number of minutes to subtract (must be non-negative).</param>
    /// <returns>A new Unix time in seconds.</returns>
    public static long SubtractMinutes(this long unixSeconds, int minutes)
    {
        if (minutes < 0) throw new ArgumentOutOfRangeException(nameof(minutes), "Minutes must be non-negative.");
        return unixSeconds - (minutes * 60L);
    }

    /// <summary>
    /// Adds the specified number of seconds to a Unix time in seconds, returning a new timestamp.
    /// </summary>
    /// <param name="unixSeconds">The Unix time in seconds.</param>
    /// <param name="seconds">The number of seconds to add (must be non-negative).</param>
    /// <returns>A new Unix time in seconds.</returns>
    public static long AddSeconds(this long unixSeconds, int seconds)
    {
        if (seconds < 0) throw new ArgumentOutOfRangeException(nameof(seconds), "Seconds must be non-negative.");
        return unixSeconds + seconds;
    }

    /// <summary>
    /// Subtracts the specified number of seconds from a Unix time in seconds, returning a new timestamp.
    /// </summary>
    /// <param name="unixSeconds">The Unix time in seconds.</param>
    /// <param name="seconds">The number of seconds to subtract (must be non-negative).</param>
    /// <returns>A new Unix time in seconds.</returns>
    public static long SubtractSeconds(this long unixSeconds, int seconds)
    {
        if (seconds < 0) throw new ArgumentOutOfRangeException(nameof(seconds), "Seconds must be non-negative.");
        return unixSeconds - seconds;
    }
    /// <summary>
    /// Safely fire-and-forget an async task with optional exception handling.
    /// </summary>
    public static async void SafeFireAndForget(this Task task, bool continueOnCapturedContext = true, Action<Exception>? onException = null)
    {
        try
        {
            await task.ConfigureAwait(continueOnCapturedContext);
        }
        catch (Exception ex) when (onException != null)
        {
            onException(ex);
        }
    }

    /// <summary>
    /// Decomposes a UtcMilliTime interval into days, hours, minutes, and seconds.
    /// </summary>
    /// <returns>A struct with the decomposed parts.</returns>
    public static IntervalParts GetIntervalParts(this long interval)
    {
        int days = (int)(interval / Constants.day_milliseconds);
        long remainder = interval % Constants.day_milliseconds;
        int hours = (int)(remainder / Constants.hour_milliseconds);
        remainder %= Constants.hour_milliseconds;
        int minutes = (int)(remainder / Constants.minute_milliseconds);
        remainder %= Constants.minute_milliseconds;
        int seconds = (int)(remainder / Constants.second_milliseconds);
        return new IntervalParts(days, hours, minutes, seconds);
    }

    public readonly record struct IntervalParts(int Days, int Hours, int Minutes, int Seconds);
}

#pragma warning restore RECS0165