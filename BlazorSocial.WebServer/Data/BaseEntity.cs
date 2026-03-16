using System.ComponentModel.DataAnnotations;

namespace BlazorSocial.Data;

/// <summary>
/// Contract for any entity keyed by a <see cref="UniqueId{TId}"/>-derived ID.
/// Implemented by <see cref="BaseEntity{TId}"/> (class inheritance) and
/// <see cref="BlazorSocial.Data.Entities.SocialUser"/> (which already inherits from IdentityUser).
/// </summary>
public interface IEntity<TId> where TId : UniqueId<TId>, new()
{
    TId Id { get; set; }
}

/// <summary>
/// Base class for entities whose primary key is a strongly-typed <see cref="UniqueId{TId}"/>.
/// Provides a <c>[Key]</c>-annotated <see cref="Id"/> property that auto-generates a UUIDv7 on construction.
/// </summary>
public abstract class BaseEntity<TId> : IEntity<TId> where TId : UniqueId<TId>, new()
{
    [Key]
    public TId Id { get; set; } = new TId { Value = Guid.CreateVersion7() };
}
