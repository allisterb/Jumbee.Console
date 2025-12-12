using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Jumbee.Console.Prompts;

internal static class TypeConverterHelper
{
    public static string ConvertToString<T>(T input)
    {
        var converter = TypeDescriptor.GetConverter(typeof(T));
        var result = converter.ConvertToInvariantString(input);
        if (result == null)
        {
            throw new InvalidOperationException("Could not convert input to a string");
        }

        return result;
    }

    public static bool TryConvertFromStringWithCulture<T>(string input, CultureInfo? info, [MaybeNull] out T? result)
    {
        try
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (info == null)
            {
                result = (T?)converter.ConvertFromInvariantString(input);
            }
            else
            {
                result = (T?)converter.ConvertFromString(null!, info, input);
            }

            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }
}
