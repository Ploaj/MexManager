using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using mexLib;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MexManager.Converters
{
    public class IndexConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (Global.Workspace != null)
            {
                if (value is MexFighter fighter)
                {
                    return $"{Global.Workspace.Project.Fighters.IndexOf(fighter):D3}.";
                }
                if (value is MexMusic music)
                {
                    return $"{Global.Workspace.Project.Music.IndexOf(music):D3}.";
                }
                if (value is MexStage stage)
                {
                    return $"{Global.Workspace.Project.Stages.IndexOf(stage):D3}.";
                }
                if (value is MexSoundbank sound)
                {
                    return $"{Global.Workspace.Project.Soundbanks.IndexOf(sound):D3}.";
                }
                if (value is MexSeries series)
                {
                    return $"{Global.Workspace.Project.Series.IndexOf(series):D3}.";
                }
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
