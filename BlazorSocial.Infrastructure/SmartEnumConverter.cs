using Ardalis.SmartEnum;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BlazorSocial.Infrastructure;

public class SmartEnumConverter<TEnum> : ValueConverter<TEnum, int>
    where TEnum : SmartEnum<TEnum>
{
    public SmartEnumConverter() : base(
        smartEnum => smartEnum.Value,
        value => SmartEnum<TEnum>.FromValue(value)) { }
}
