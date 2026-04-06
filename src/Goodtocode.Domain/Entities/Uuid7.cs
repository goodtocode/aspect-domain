using System.Security.Cryptography;

namespace Goodtocode.Domain.Entities;

/// <summary>
/// UUID version 7 generator (time-ordered, RFC 4122 compliant).
/// Guarantees monotonic ordering within a single process.
/// </summary>
public static class Uuid7
{
    private static readonly object _lock = new();
    private static long _lastTimestamp;
    private static ushort _sequence;

    /// <summary>
    /// Generates a new time-ordered UUID version 7.
    /// </summary>
    public static Guid New()
    {
        Span<byte> bytes = stackalloc byte[16];

        long timestamp;
        ushort sequence;

        lock (_lock)
        {
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (timestamp <= _lastTimestamp)
                timestamp = _lastTimestamp;

            if (timestamp == _lastTimestamp)
            {
                _sequence++;
            }
            else
            {
                _sequence = 0;
                _lastTimestamp = timestamp;
            }

            sequence = _sequence;
        }

        // 1. Timestamp (48 bits, big-endian)
        bytes[0] = (byte)(timestamp >> 40);
        bytes[1] = (byte)(timestamp >> 32);
        bytes[2] = (byte)(timestamp >> 24);
        bytes[3] = (byte)(timestamp >> 16);
        bytes[4] = (byte)(timestamp >> 8);
        bytes[5] = (byte)timestamp;

        // 2. Sequence (12 bits) in ver/rand_a field
        bytes[6] = (byte)(sequence >> 4);
        bytes[7] = (byte)(sequence << 4);

        // 3. Random bytes for rand_b
        RandomNumberGenerator.Fill(bytes.Slice(8));

        // 4. Set UUID version 7
        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x70);

        // 5. Set RFC 4122 variant bits
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);

        return new Guid(bytes);
    }
}
