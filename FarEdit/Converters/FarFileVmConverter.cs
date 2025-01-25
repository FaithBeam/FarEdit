using System;
using System.Globalization;
using System.IO;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using FarEdit.Core.ViewModels.MainWindowViewModel.Queries;

namespace FarEdit.Converters;

public class FarFileVmConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not GetFarFiles.FarFileVm vm || parameter is not string para)
        {
            return null;
        }

        if (para == "image" && vm.Name.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
        {
            return new Bitmap(new MemoryStream(vm.Data));
        }

        return null;
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        throw new NotImplementedException();
    }
}
