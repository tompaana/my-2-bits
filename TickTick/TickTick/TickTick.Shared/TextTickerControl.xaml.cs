using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Threading;

#if SILVERLIGHT
using System.Windows.Controls;
using System.Windows.Navigation;

using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
#elif NETFX_CORE
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#endif

namespace TickTick
{
    public partial class TextTickerControl : UserControl
    {
        #region Constants, members and properties

        private const double DefaultVisibleWidth = 480d;
        private const int UpdateTextTimerDelayInMilliseconds = 100;
        private const int AnimationDurationInMilliseconds = 8000;
        private const int DefaultTickerDelayInMilliseconds = 8000;
        private const int TextRelativeMargin = 54;
        private Timer _tickerTimer;
        private Timer _updateTextTimer;

        public double VisibleWidth
        {
            get
            {
                return (double)GetValue(VisibleWidthProperty);
            }
            set
            {
                SetValue(VisibleWidthProperty, value);
            }
        }

        public static readonly DependencyProperty VisibleWidthProperty =
            DependencyProperty.Register("VisibleWidth", typeof(double), typeof(TextTickerControl),
                new PropertyMetadata(DefaultVisibleWidth, OnVisibleWidthPropertyChanged));

        private static void OnVisibleWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextTickerControl control = d as TextTickerControl;

            if (control != null)
            {
                control.SetDimensions();
            }
        }

        public double TextWidth
        {
            get
            {
                return (double)GetValue(TextWidthProperty);
            }
            private set
            {
                SetValue(TextWidthProperty, value);
            }
        }

        public static readonly DependencyProperty TextWidthProperty =
            DependencyProperty.Register("TextWidth", typeof(double), typeof(TextTickerControl), null);

        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextTickerControl),
                new PropertyMetadata(null, OnTextPropertyChanged));

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextTickerControl control = d as TextTickerControl;

            if (control != null && control.tickerTextBlock != null)
            {
                control.tickerTextBlock.Text = (string)e.NewValue;
                control.trimmedTickerTextBlock.Text = (string)e.NewValue;            
                control.Reset();
            }
        }

        public int StartDelayInMilliseconds
        {
            get
            {
                return (int)GetValue(StartDelayInMillisecondsProperty);
            }
            set
            {
                SetValue(StartDelayInMillisecondsProperty, value);
            }
        }

        public static readonly DependencyProperty StartDelayInMillisecondsProperty =
            DependencyProperty.Register("StartDelayInMilliseconds", typeof(int), typeof(TextTickerControl),
                new PropertyMetadata(DefaultTickerDelayInMilliseconds, OnStartDelayInMillisecondsPropertyChanged));

        private static void OnStartDelayInMillisecondsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextTickerControl control = d as TextTickerControl;

            if (control != null && (int)e.NewValue < TextTickerControl.UpdateTextTimerDelayInMilliseconds)
            {
                control.StartDelayInMilliseconds = TextTickerControl.UpdateTextTimerDelayInMilliseconds;
            }
        }

        public int RestartDelayInMilliseconds
        {
            get
            {
                return (int)GetValue(RestartDelayInMillisecondsProperty);
            }
            set
            {
                SetValue(RestartDelayInMillisecondsProperty, value);
            }
        }

        public static readonly DependencyProperty RestartDelayInMillisecondsProperty =
            DependencyProperty.Register("RestartDelayInMilliseconds", typeof(int), typeof(TextTickerControl),
                new PropertyMetadata(DefaultTickerDelayInMilliseconds));

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public TextTickerControl()
        {
            InitializeComponent();
            gridClipRect.Rect = new Rect(0d, 0d, PixelsToRelativeSize(VisibleWidth), PixelsToRelativeSize(ActualHeight));
            trimmedTickerTextBlock.SizeChanged += ((sender, args) => Reset());
        }

#if SILVERLIGHT
        private void SetDimensions()
#elif NETFX_CORE
        private async void SetDimensions()
#endif
        {
#if SILVERLIGHT
            Dispatcher.BeginInvoke(() =>
#elif NETFX_CORE
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
#endif
                {
                    TextWidth = tickerTextBlock.ActualWidth;
                    gridClipRect.Rect = new Rect(0d, 0d, PixelsToRelativeSize(VisibleWidth), PixelsToRelativeSize(ActualHeight));
                    trimmedTickerTextBlock.MaxWidth = VisibleWidth;
                    trimmedTickerTextBlock.Width = VisibleWidth;
                });
        }

        /// <summary>
        /// Updates the dimensions and sets the trimmed text visible.
        /// </summary>
#if SILVERLIGHT
        private void SetTrimmedTextVisible()
#elif NETFX_CORE
        private async void SetTrimmedTextVisible()
#endif
        {
#if SILVERLIGHT
            Dispatcher.BeginInvoke(() =>
#elif NETFX_CORE
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
#endif
            {
                trimmedTickerTextBlockTranslateTransform.X = 0d;
                trimmedTickerTextBlock.Visibility = Visibility.Visible;
                tickerTextBlock.Visibility = Visibility.Collapsed;
            });
        }

        /// <summary>
        /// Resets the state of this control.
        /// </summary>
        private void Reset()
        {
            scrollTickerStoryBoard.Stop();

            // Quick timer for setting the dimensions
            if (_updateTextTimer != null)
            {
                _updateTextTimer.Dispose();
            }

            _updateTextTimer = new Timer((o) =>
            {
                SetTrimmedTextVisible();
                SetDimensions();

                if (_updateTextTimer != null)
                {
                    _updateTextTimer.Dispose();
                    _updateTextTimer = null;
                }
            }, null, TextTickerControl.UpdateTextTimerDelayInMilliseconds, Timeout.Infinite);

            // Timer for starting the animation
            if (_tickerTimer != null)
            {
                _tickerTimer.Dispose();
            }

            _tickerTimer = new Timer((o) =>
            {
                if (_updateTextTimer == null)
                {
                    RestartScrollingAnimation();
                }
            }, null, StartDelayInMilliseconds, RestartDelayInMilliseconds + AnimationDurationInMilliseconds);
        }

        /// <summary>
        /// (Re)starts the scrolling animation of the ticker given that the set string is long
        /// enough.
        /// </summary>
#if SILVERLIGHT
        private void RestartScrollingAnimation()
#elif NETFX_CORE
        private async void RestartScrollingAnimation()
#endif
        {
#if SILVERLIGHT
            Dispatcher.BeginInvoke(() =>
#elif NETFX_CORE
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
#endif
                {
                    scrollTickerStoryBoard.Stop();

                    bool tickingNeeded = TextWidth > VisibleWidth;

                    tickerTextBlock.Visibility = tickingNeeded ? Visibility.Visible : Visibility.Collapsed;
                    trimmedTickerTextBlock.Visibility = Visibility.Visible;
                    tickerTextBlockTranslateTransform.X = 0d;
                    trimmedTickerTextBlockTranslateTransform.X = tickingNeeded ? TextWidth + TextRelativeMargin : 0d;

                    tickerTextXTarget.Value = -TextWidth - TextRelativeMargin;
                    trimmedTickerTextXTarget.Value = 0d;

                    if (tickingNeeded)
                    {
                        scrollTickerStoryBoard.Begin();
                    }
                });
        }

#if SILVERLIGHT
        private void OnScrollTickerStoryBoardCompleted(object sender, EventArgs e)
#elif NETFX_CORE
        private void OnScrollTickerStoryBoardCompleted(object sender, object e)
#endif
        {
            SetTrimmedTextVisible();
        }

        private double PixelsToRelativeSize(double pixels)
        {
#if NETFX_CORE
            DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();
            double coefficient = 1.0d;

            if (displayInformation != null)
            {
                switch (displayInformation.ResolutionScale)
                {
                    case ResolutionScale.Scale100Percent:
                        coefficient = 1.0d;
                        break;
                    case ResolutionScale.Scale120Percent:
                        coefficient = 1.2d;
                        break;
                    case ResolutionScale.Scale140Percent:
                        coefficient = 1.4d;
                        break;
                    case ResolutionScale.Scale150Percent:
                        coefficient = 1.5d;
                        break;
                    case ResolutionScale.Scale160Percent:
                        coefficient = 1.6d;
                        break;
                    case ResolutionScale.Scale180Percent:
                        coefficient = 1.8d;
                        break;
                    case ResolutionScale.Scale225Percent:
                        coefficient = 2.25d;
                        break;
                }
            }

            return pixels * coefficient;
#elif SILVERLIGHT
            return pixels * Application.Current.Host.Content.ScaleFactor / 100d;
#endif
        }
    }
}
