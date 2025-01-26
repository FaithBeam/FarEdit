using System;
using System.Globalization;
using System.IO;
using System.Text.Json.Serialization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using AvaloniaHex.Document;
using FarEdit.Core.ViewModels.MainWindowViewModel.Models;
using SixLabors.ImageSharp;

namespace FarEdit.Converters;

public class FarFileVmConverter : IValueConverter
{
    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) =>
        value is not FarFileVm vm || parameter is not string para
            ? null
            : para switch
            {
                "image" when vm.Name.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) =>
                    new Bitmap(new MemoryStream(vm.Data)),
                "image" when vm.Name.EndsWith(".tga", StringComparison.OrdinalIgnoreCase) =>
                    ConvertTgaToBitmap(vm.Data),
                "data"
                    when !vm.Name.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase)
                        && !vm.Name.EndsWith(".tga", StringComparison.OrdinalIgnoreCase) =>
                    new MemoryBinaryDocument(vm.Data),
                _ => null,
            };

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        throw new NotImplementedException();
    }

    private static Bitmap ConvertTgaToBitmap(byte[] data)
    {
        var image = Image.Load(data);
        var ms = new MemoryStream();
        image.SaveAsBmp(ms);
        ms.Position = 0;
        return new Bitmap(ms);
    }
}
