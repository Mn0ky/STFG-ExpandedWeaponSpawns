using System;

namespace ExpandedWeaponSpawns;

public static class EnumHelper
{
    // Adapted From: https://stackoverflow.com/a/1082587, does NOT support bitflag enums or enums without a 0 'default' value
    public static TEnum ToEnum<TEnum>(this string strEnumValue, TEnum defaultValue) 
        => !Enum.IsDefined(typeof(TEnum), strEnumValue) ? defaultValue : (TEnum) Enum.Parse(typeof(TEnum), strEnumValue);
}