using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using Sims.Far;

namespace FarEdit.Converters;

public class FarVersionConverter : IValueConverter
{
    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) =>
        value switch
        {
            IEnumerable<FarVersion> versions => versions.Select(x =>
                x switch
                {
                    FarVersion._1A => "1a",
                    FarVersion._1B => "1b",
                    FarVersion._3 => "3",
                    _ => throw new ArgumentOutOfRangeException(),
                }
            ),
            FarVersion fv => fv switch
            {
                FarVersion._1A => "1a",
                FarVersion._1B => "1b",
                FarVersion._3 => "3",
                _ => throw new ArgumentOutOfRangeException(),
            },
            _ => null,
        };

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) =>
        value switch
        {
            IEnumerable<string> strings => strings.Select(x =>
                x switch
                {
                    "1a" => FarVersion._1A,
                    "1b" => FarVersion._1B,
                    "3" => FarVersion._3,
                    _ => throw new ArgumentOutOfRangeException(nameof(x), x, null),
                }
            ),
            string str => str switch
            {
                "1a" => FarVersion._1A,
                "1b" => FarVersion._1B,
                "3" => FarVersion._3,
                _ => throw new ArgumentOutOfRangeException(),
            },
            _ => null,
        };
}
