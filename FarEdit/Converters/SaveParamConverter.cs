using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Data.Converters;
using FarEdit.Core.ViewModels.MainWindowViewModel.Queries;

namespace FarEdit.Converters;

public class SaveParamConverter : IMultiValueConverter
{
    public object? Convert(
        IList<object?> values,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        if (values.Count != 2)
        {
            return null;
        }

        if (
            values[0] is not string path
            || values[1] is not ReadOnlyObservableCollection<GetFarFiles.FarFileVm> files
        )
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return (path, files);
    }
}
