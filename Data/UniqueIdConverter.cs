using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BlazorSocial.Data;

// Single generic EF Core Value Converter for all UniqueId-derived types

public class UniqueIdConverter<TSelf>() : ValueConverter<TSelf, Guid>(id => id.Value,
    guid => new TSelf { Value = guid })
    where TSelf : UniqueId<TSelf>, new();