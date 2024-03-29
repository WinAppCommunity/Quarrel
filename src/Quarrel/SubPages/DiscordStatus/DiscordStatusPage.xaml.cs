﻿// Quarrel © 2022

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Quarrel.Converters.Discord.APIStatus;
using Quarrel.ViewModels.SubPages.DiscordStatus;
using System;
using System.Globalization;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Quarrel.SubPages.DiscordStatus
{
    public sealed partial class DiscordStatusPage : UserControl
    {
        /// <summary>
        /// Width of line on graph.
        /// </summary>
        private const float DataStrokeThickness = 1;

        /// <summary>
        /// The position of the mouse on the chart, in Pixels.
        /// </summary>
        private float _cursorPosition;

        public DiscordStatusPage()
        {
            this.InitializeComponent();
            DataContext = App.Current.Services.GetRequiredService<DiscordStatusViewModel>();
            

            ViewModel.ChartDataLoaded += (m, args) =>
            {
                chartCanvas.Invalidate();
                if (HideChart.GetCurrentState() != ClockState.Stopped)
                {
                    HideChart.Stop();
                }

                ShowChart.Begin();
            };

            // Change everything's color by status
            ViewModel.StatusLoaded += (m, args) =>
            {
                var statusBrush = new SolidColorBrush(StatusColor);
                StatusContainer.Background = statusBrush;
                detailsButton.Foreground = statusBrush;
                dayDuration.Foreground = statusBrush;
                weekDuration.Foreground = statusBrush;
                monthDuration.Foreground = statusBrush;
                IncidentsScroller.Background = new SolidColorBrush(statusBrush.Color) { Opacity = 0.25 };
            };
        }

        public DiscordStatusViewModel ViewModel => (DiscordStatusViewModel)DataContext;

        /// <summary>
        /// Gets Accent Color based on Status.
        /// </summary>
        private Color StatusColor => ViewModel.Status != null ?
            StatusToColorConverter.Convert(ViewModel.Status.Status.Indicator) :
            (Color)App.Current.Resources["SystemAccentColor"];

        /// <summary>
        /// Selects and loads day graph.
        /// </summary>
        private void ShowDayMetrics(object sender, RoutedEventArgs e)
        {
            HideChart.Begin();
            ViewModel.ShowMetrics("day");
            dayDuration.IsEnabled = false;
            weekDuration.IsEnabled = true;
            monthDuration.IsEnabled = true;
        }

        /// <summary>
        /// Selects and loads week graph.
        /// </summary>
        private void ShowWeekMetrics(object sender, RoutedEventArgs e)
        {
            HideChart.Begin();
            ViewModel.ShowMetrics("week");
            dayDuration.IsEnabled = true;
            weekDuration.IsEnabled = false;
            monthDuration.IsEnabled = true;
        }

        /// <summary>
        /// Selects and loads month graph.
        /// </summary>
        private void ShowMonthMetrics(object sender, RoutedEventArgs e)
        {
            HideChart.Begin();
            ViewModel.ShowMetrics("month");
            dayDuration.IsEnabled = true;
            weekDuration.IsEnabled = true;
            monthDuration.IsEnabled = false;
        }
        
        private void ChartCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            _cursorPosition = Convert.ToSingle(e.GetCurrentPoint(chartCanvas).Position.X);
            chartIndicator.Invalidate();
        }

        private void CanvasControl_OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (ViewModel.IsLoaded)
            {
                RenderData(chartCanvas, args, StatusColor, DataStrokeThickness, ViewModel.Data, false, ViewModel.Max);
            }
        }

        private void ChartIndicator_OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (StepSize != 0)
            {
                var location = Convert.ToInt32(Math.Round(_cursorPosition / StepSize));
                if (ViewModel.DataValues.ContainsKey(location))
                {
                    var item = ViewModel.DataValues[location];
                    var format = new CanvasTextFormat { FontSize = 12.0f, WordWrapping = CanvasWordWrapping.NoWrap };
                    var textLayout = new CanvasTextLayout(args.DrawingSession, item.Value + "ms", format, 0.0f, 0.0f);

                    DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(item.Timestamp);
                    string durationText;
                    if (!dayDuration.IsEnabled)
                    {
                        durationText = date.ToString("t");
                    }
                    else if (!weekDuration.IsEnabled)
                    {
                        durationText = date.ToString("dddd") + " " + date.ToString("t");
                    }
                    else
                    {
                        durationText = date.ToString("g");
                    }

                    var textLayout2 = new CanvasTextLayout(args.DrawingSession, durationText, format, 0.0f, 0.0f);

                    if (_cursorPosition + textLayout2.DrawBounds.Width + 6 > chartIndicator.ActualWidth || _cursorPosition + textLayout.DrawBounds.Width + 6 > chartIndicator.ActualWidth)
                    {
                        args.DrawingSession.DrawTextLayout(textLayout, new Vector2(Convert.ToSingle(_cursorPosition - textLayout.DrawBounds.Width - 12), 0), Color.FromArgb(255, 255, 255, 255));
                        args.DrawingSession.DrawTextLayout(textLayout2, new Vector2(Convert.ToSingle(_cursorPosition - textLayout2.DrawBounds.Width - 12), 14), Color.FromArgb(120, 255, 255, 255));
                    }
                    else
                    {
                        args.DrawingSession.DrawTextLayout(textLayout, new Vector2(_cursorPosition + 4, 0), Color.FromArgb(255, 255, 255, 255));
                        args.DrawingSession.DrawTextLayout(textLayout2, new Vector2(_cursorPosition + 4, 14), Color.FromArgb(120, 255, 255, 255));
                    }
                }

                args.DrawingSession.DrawLine(new Vector2(_cursorPosition, 0), new Vector2(_cursorPosition, (float)chartIndicator.ActualHeight), Colors.White);
            }
        }

        private void FrameworkElement_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            chartClip.Rect = new Rect(-chartCanvas.ActualWidth, 0, chartCanvas.ActualWidth, chartCanvas.ActualHeight);
            chartClipTransform.X = chartCanvas.ActualWidth;
            ShowChartDA.To = chartCanvas.ActualWidth;
            HideChartDA.From = chartCanvas.ActualWidth;
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ShowIndicator.Begin();
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            HideIndicator.Begin();
        }
    }
}
