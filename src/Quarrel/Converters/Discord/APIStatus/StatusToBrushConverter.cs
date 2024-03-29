﻿// Quarrel © 2022

using Windows.UI.Xaml.Media;

namespace Quarrel.Converters.Discord.APIStatus
{
    public static class StatusToBrushConverter
    {
        public static SolidColorBrush Convert(string status)
            => new(StatusToColorConverter.Convert(status));
    }
}
