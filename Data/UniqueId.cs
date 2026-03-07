using System.ComponentModel;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BlazorSocial.Data;

/// <summary>
///     Abstract base class for all strongly-typed entity IDs.
///     Uses UUIDv7 (time-ordered) to minimize index fragmentation on clustered indexes.
/// </summary>
public abstract class UniqueId : IEquatable<UniqueId>, IComparable<UniqueId>
{
    public Guid Value { get; init; }

    public int CompareTo(UniqueId? other) => Value.CompareTo(other?.Value ?? Guid.Empty);
    public bool Equals(UniqueId? other) => other is not null && GetType() == other.GetType() && Value == other.Value;
    public override bool Equals(object? obj) => obj is UniqueId other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value.ToString("N");

    public static bool operator ==(UniqueId? left, UniqueId? right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    public static bool operator !=(UniqueId? left, UniqueId? right) => !(left == right);
}

/// <summary>
///     Generic base that adds typed static factory methods via CRTP.
///     Inherit from this to create a new strongly-typed ID: <c>public sealed class MyId : UniqueId&lt;MyId&gt; { }</c>
/// </summary>
public abstract class UniqueId<TSelf> : UniqueId, IEquatable<TSelf>, IComparable<TSelf>
    where TSelf : UniqueId<TSelf>, new()
{
    public static readonly TSelf Empty = new() { Value = Guid.Empty };

    public int CompareTo(TSelf? other) => Value.CompareTo(other?.Value ?? Guid.Empty);
    public bool Equals(TSelf? other) => other is not null && Value == other.Value;
    public static TSelf New() => new() { Value = Guid.CreateVersion7() };

    public static TSelf Parse(string s) => new() { Value = Guid.Parse(s) };
    public static TSelf Parse(Guid guid) => new() { Value = guid };

    public static bool TryParse(string? s, out TSelf result)
    {
        if (Guid.TryParse(s, out var guid))
        {
            result = new TSelf { Value = guid };
            return true;
        }

        result = Empty;
        return false;
    }

    public override bool Equals(object? obj) => obj is TSelf other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
}

// Concrete typed IDs — all logic is inherited from UniqueId<TSelf>

[TypeConverter(typeof(UserIdTypeConverter))]
public sealed class UserId : UniqueId<UserId>
{
}

public sealed class PostId : UniqueId<PostId>
{
}

public sealed class CommentId : UniqueId<CommentId>
{
}

public sealed class GroupId : UniqueId<GroupId>
{
}

public sealed class ViewId : UniqueId<ViewId>
{
}

// Single generic EF Core Value Converter for all UniqueId-derived types

public class UniqueIdConverter<TSelf> : ValueConverter<TSelf, Guid>
    where TSelf : UniqueId<TSelf>, new()
{
    public UniqueIdConverter() : base(
        id => id.Value,
        guid => new TSelf { Value = guid })
    {
    }
}

/// <summary>
///     TypeConverter for <see cref="UserId" /> so ASP.NET Identity can convert between string and UserId
///     (used by UserStore.ConvertIdFromString / ConvertIdToString).
/// </summary>
public class UserIdTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        => value is string s ? UserId.Parse(s) : base.ConvertFrom(context, culture, value);

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value,
        Type destinationType)
        => value is UserId id && destinationType == typeof(string)
            ? id.Value.ToString()
            : base.ConvertTo(context, culture, value, destinationType);
}