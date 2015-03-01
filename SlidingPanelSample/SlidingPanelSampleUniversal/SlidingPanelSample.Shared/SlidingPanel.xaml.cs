using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;

#if SILVERLIGHT
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
#elif NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
#endif


namespace SlidingPanelSample
{
    public partial class SlidingPanel : UserControl
    {
        #region Constants

        private const double DefaultPanelWidth = 480d;
        private const double DefaultFullscreenPanelHeight = 740d;
        private const double DefaultTippingPointY = 300d;
        private const double SnapshotValidInSeconds = 0.5d;
        private const int JitterThreshold = 10;

        #endregion

        public event EventHandler<bool> MinimizedChanged;
        public event EventHandler<bool> AnimatingStarted; // If boolean value is true, we are animating towards maximized
        public event EventHandler AnimatingComplete;

        private DateTime _lastTimeSnapshotsWereTaken = DateTime.Now.AddSeconds(-SnapshotValidInSeconds);
        private bool _isBeingManipulated;

        #region Properties

        public bool Minimized
        {
            get
            {
                return (bool)GetValue(MinimizedProperty);
            }
            set
            {
                SetValue(MinimizedProperty, value);
            }
        }
        private static readonly DependencyProperty MinimizedProperty =
            DependencyProperty.Register("Minimized", typeof(bool), typeof(SlidingPanel),
                new PropertyMetadata(false, OnMinimizedPropertyChanged));

        private static void OnMinimizedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SlidingPanel control = d as SlidingPanel;

            if (control != null)
            {
                bool minimized = (bool)e.NewValue;
                control.SetValuesBasedOnState(minimized);

                if (control.MinimizedChanged != null)
                {
                    control.MinimizedChanged(control, minimized);
                }

                control.IsAnimating = false;
            }
        }

        /// <summary>
        /// The value of this property is true, when this panel is animating.
        /// </summary>
        public bool IsAnimating
        {
            get;
            private set;
        }

        /// <summary>
        /// Enables/disables the use of snapshots.
        /// </summary>
        public bool UseSnapshots
        {
            get
            {
                return (bool)GetValue(UseSnapshotsProperty);
            }
            set
            {
                SetValue(UseSnapshotsProperty, value);
            }
        }
        private static readonly DependencyProperty UseSnapshotsProperty =
            DependencyProperty.Register("UseSnapshots", typeof(bool), typeof(SlidingPanel),
                new PropertyMetadata(true, OnUseSnapshotsPropertyChanged));

        private static void OnUseSnapshotsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SlidingPanel control = d as SlidingPanel;

            if (control != null && (bool)e.NewValue == false)
            {
                control.SnapshotVisible = false;
            }
        }

        private bool _snapshotVisible;
        /// <summary>
        /// Switches the snapshot on/off. May fail in case the snapshot cannot be acquired.
        /// Setting the snapshot on improves rendering performance, but the panel cannot be
        /// interacted with when the snapshot is on.
        /// </summary>
        public bool SnapshotVisible
        {
            get
            {
                return _snapshotVisible;
            }
            set
            {
#if SILVERLIGHT
                if (!UseSnapshots && value)
                {
                    _snapshotVisible = false;
                    return;
                }
                
                if (_snapshotVisible != value)
                {
                    if (value && TakeSnapShotsFromPanels())
                    {
                        if (Minimized)
                        {
                            minimizedPanelSnapshot.Visibility = Visibility.Visible;
                            minimizedPanel.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            fullscreenPanelSnapshot.Visibility = Visibility.Visible;
                            fullscreenPanel.Visibility = Visibility.Collapsed;
                        }

                        _snapshotVisible = value;
                    }
                    else if (!value)
                    {
                        if (Minimized)
                        {
                            minimizedPanel.Visibility = Visibility.Visible;
                            minimizedPanelSnapshot.Visibility = Visibility.Collapsed; 
                        }
                        else
                        {
                            fullscreenPanel.Visibility = Visibility.Visible;
                            fullscreenPanelSnapshot.Visibility = Visibility.Collapsed;
                        }

                        _snapshotVisible = value;
                    }
                }
#elif NETFX_CORE
                _snapshotVisible = false;
#endif
            }
        }

        /// <summary>
        /// A value between 0 and 1.
        /// 
        /// If the value is 0, then the control is in the fullscreen mode.
        /// If the value is 1, the control is fully minimized.
        /// </summary>
        public double TransitionState
        {
            get
            {
                return (double)GetValue(TransitionStateProperty);
            }
            private set
            {
                SetValue(TransitionStateProperty, value);
            }
        }
        private static readonly DependencyProperty TransitionStateProperty =
            DependencyProperty.Register("TransitionState", typeof(double), typeof(SlidingPanel),
                new PropertyMetadata(0d));

        private double PanelWidth
        {
            get;
            set;
        }

        private double FullscreenPanelHeight
        {
            get
            {
                return (double)GetValue(FullscreenPanelHeightProperty);
            }
            set
            {
                SetValue(FullscreenPanelHeightProperty, value);
            }
        }
        private static readonly DependencyProperty FullscreenPanelHeightProperty =
            DependencyProperty.Register("FullscreenPanelHeight", typeof(double), typeof(SlidingPanel),
                new PropertyMetadata(DefaultFullscreenPanelHeight));

        private double MaxTransformY
        {
            get
            {
                return (double)GetValue(MaxTransformYProperty);
            }
            set
            {
                SetValue(MaxTransformYProperty, value);
            }
        }
        private static readonly DependencyProperty MaxTransformYProperty =
            DependencyProperty.Register("MaxTransformY", typeof(double), typeof(SlidingPanel), null);

        private double TippingPointY
        {
            get;
            set;
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public SlidingPanel()
        {
            InitializeComponent();

#if NETFX_CORE
            UseSnapshots = false;
            UseSnapshotsCheckBox.IsEnabled = false;
#endif

            TippingPointY = DefaultTippingPointY;
            SizeChanged += OnSizeChanged;
            OnSizeChanged(this, null); // Set dimensions including e.g. full screen panel size
            SetValuesBasedOnState(Minimized);
        }

        public void Minimize()
        {
            if (!Minimized)
            {
                Animate(true);
            }
        }

        public void Maximize()
        {
            if (Minimized)
            {
                Animate(false);
            }
        }

        #region Private methods

        /// <summary>
        /// Starts the mode change animation.
        /// </summary>
        /// <param name="toMinimized">If true, will expect the mode change is to minimized.</param>
        /// <param name="notifyListeners">If true, will notify listeners.</param>
        private void Animate(bool toMinimized = false, bool notifyListeners = true)
        {
            if (notifyListeners && AnimatingStarted != null)
            {
                AnimatingStarted(this, !toMinimized);
            }

            IsAnimating = true;

            if (UseSnapshots)
            {
                TakeSnapShotsFromPanels();
            }

            slideUpAnimationTransformY.From = panelTranslateTransform.Y;
            slideDownAnimationTransformY.To = toMinimized ? MaxTransformY : 0.0d;
            slideDownAnimationFullscreenPanelOpacity.From = 1.0d - TransitionState;
            slideDownAnimationMinimizedPanelOpacity.From = TransitionState;

            if (UseSnapshots)
            {
                fullscreenPanelSnapshot.Visibility = Visibility.Visible;
                minimizedPanelSnapshot.Visibility = Visibility.Visible;

                fullscreenPanel.Visibility = Visibility.Collapsed;
                minimizedPanel.Visibility = Visibility.Collapsed;

                fullscreenPanelSnapshot.Opacity = 1.0d - TransitionState;
                minimizedPanelSnapshot.Opacity = TransitionState;
            }
            else
            {
                fullscreenPanel.Visibility = Visibility.Visible;
                minimizedPanel.Visibility = Visibility.Visible;

                fullscreenPanel.Opacity = 1.0d - TransitionState;
                minimizedPanel.Opacity = TransitionState;
            }

            try
            {
                if (toMinimized)
                {
                    slideDownAnimation.Begin();
                }
                else
                {
                    slideUpAnimation.Begin();
                }
            }
            catch (InvalidOperationException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                Minimized = toMinimized;
            }
        }

        /// <summary>
        /// Creates a snapshot of the panels, which can be used for optimizing the slide and
        /// making the animations fluid.
        /// 
        /// This method is only supported in Silverlight app. Will always return false, if not supported.
        /// </summary>
        /// <returns>True, if at least one snapshot was taken successfully or a previously taken snapshot exists.</returns>
        private bool TakeSnapShotsFromPanels()
        {
            bool success = false;

#if SILVERLIGHT
            if (UseSnapshots && DateTime.Now > _lastTimeSnapshotsWereTaken.AddSeconds(SnapshotValidInSeconds))
            {
                WriteableBitmap bitmap = null;

                if (fullscreenPanel.Visibility == Visibility.Visible)
                {
                    bitmap = new WriteableBitmap((int)PanelWidth, (int)FullscreenPanelHeight);
                    bitmap.Render(fullscreenPanel, null);
                    bitmap.Invalidate();
                    fullscreenPanelSnapshot.Source = bitmap;

                    //System.Diagnostics.Debug.WriteLine("SlidingPanel.TakeSnapShotsFromPanels(): Fullscreen panel snapshot created");
                    success = true;
                }

                if (minimizedPanel.Visibility == Visibility.Visible)
                {
                    bitmap = new WriteableBitmap((int)PanelWidth, (int)minimizedPanel.Height);
                    bitmap.Render(minimizedPanel, null);
                    bitmap.Invalidate();
                    minimizedPanelSnapshot.Source = bitmap;

                    //System.Diagnostics.Debug.WriteLine("SlidingPanel.TakeSnapShotsFromPanels(): Minimized panel snapshot created");
                    success = true;
                }

                _lastTimeSnapshotsWereTaken = DateTime.Now;
            }
            else
            {
                if ((Minimized && minimizedPanelSnapshot.Source != null)
                    || (!Minimized && fullscreenPanelSnapshot.Source != null))
                {
                    // A previously taken snapshot exists
                    success = true;
                }
            }
#endif
            return success;
        }

        /// <summary>
        /// Sets the transition value and the subcomponent values based on the given state.
        /// </summary>
        /// <param name="minimized"></param>
        private void SetValuesBasedOnState(bool minimized)
        {
            panelTranslateTransform.Y = minimized ? MaxTransformY : 0d;

            if (minimized)
            {
                minimizedPanel.Visibility = Visibility.Visible;
                minimizedPanel.Opacity = 1.0d;
                fullscreenPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                fullscreenPanel.Visibility = Visibility.Visible;
                fullscreenPanel.Opacity = 1.0d;
                minimizedPanel.Visibility = Visibility.Collapsed;
            }

            fullscreenPanelSnapshot.Visibility = Visibility.Collapsed;
            minimizedPanelSnapshot.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Sets the transition state and the subcomponent values based on the given Y transition value.
        /// </summary>
        /// <param name="transformY"></param>
        private void SetValuesBasedOnTransformY(double transformY)
        {
            if (!(MaxTransformY > 0))
            {
                System.Diagnostics.Debug.WriteLine("Invalid MaxTransformY value: " + MaxTransformY);
                return;
            }

            TransitionState = transformY / MaxTransformY;

            if (UseSnapshots)
            {
                if (fullscreenPanelSnapshot.Visibility == Visibility.Visible)
                {
                    fullscreenPanelSnapshot.Opacity = 1.0d - TransitionState;
                }

                if (minimizedPanelSnapshot.Visibility == Visibility.Visible)
                {
                    minimizedPanelSnapshot.Opacity = 0d + TransitionState;
                }
            }
            else
            {
                fullscreenPanel.Opacity = 1.0d - TransitionState;
                minimizedPanel.Opacity = 0d + TransitionState;
            }
        }

        private void RecalculateMaxTransformY()
        {
            MaxTransformY = FullscreenPanelHeight - minimizedPanel.Height;

            if (Minimized)
            {
                panelTranslateTransform.Y = MaxTransformY;
            }
        }

        #endregion

        #region Event handlers

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e != null)
            {
                FullscreenPanelHeight = e.NewSize.Height;
                PanelWidth = e.NewSize.Width;
            }

            TippingPointY = FullscreenPanelHeight / 2;
            RecalculateMaxTransformY();
        }

#if SILVERLIGHT
        protected override void OnManipulationStarted(ManipulationStartedEventArgs e)
#elif NETFX_CORE
        protected override void OnManipulationStarted(ManipulationStartedRoutedEventArgs e)
#endif
        {
#if SILVERLIGHT
            bool okToStart = false;
            object originalSource = e.OriginalSource;
            okToStart = (originalSource == dragAreaImage);
#elif NETFX_CORE
            bool okToStart = true;
#endif

            if (Minimized || okToStart)
            {
                if (UseSnapshots)
                {
                    TakeSnapShotsFromPanels();
                }

                _isBeingManipulated = true;
                e.Handled = true;
            }
            
            base.OnManipulationStarted(e);
        }

#if SILVERLIGHT
        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
#elif NETFX_CORE
        protected override void OnManipulationDelta(ManipulationDeltaRoutedEventArgs e)
#endif
        {
            if (_isBeingManipulated)
            {
                if (UseSnapshots)
                {
                    if (fullscreenPanelSnapshot.Visibility == Visibility.Collapsed)
                    {
                        // First time here since manipulation started
                        fullscreenPanelSnapshot.Visibility = Visibility.Visible;
                        minimizedPanelSnapshot.Visibility = Visibility.Visible;

                        fullscreenPanel.Visibility = Visibility.Collapsed;
                        minimizedPanel.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    fullscreenPanel.Visibility = Visibility.Visible;
                    minimizedPanel.Visibility = Visibility.Visible;
                }

#if SILVERLIGHT
                panelTranslateTransform.Y += e.DeltaManipulation.Translation.Y;
#elif NETFX_CORE
                panelTranslateTransform.Y += e.Delta.Translation.Y;
#endif

                if (panelTranslateTransform.Y < 0)
                {
                    panelTranslateTransform.Y = 0;
                }
                else if (panelTranslateTransform.Y > MaxTransformY)
                {
                    panelTranslateTransform.Y = MaxTransformY;
                }

                SetValuesBasedOnTransformY(panelTranslateTransform.Y);
                e.Handled = true;
            }
            else
            {
                base.OnManipulationDelta(e);
            }
        }

#if SILVERLIGHT
        protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
#elif NETFX_CORE
        protected override void OnManipulationCompleted(ManipulationCompletedRoutedEventArgs e)
#endif
        {
            if (_isBeingManipulated)
            {
                if (panelTranslateTransform.Y > TippingPointY)
                {
                    Animate(true, true);
                }
                else
                {
                    Animate(false, true);
                }

                _isBeingManipulated = false;
                e.Handled = true;
            }

            base.OnManipulationCompleted(e);
        }


#if NETFX_CORE
        private void OnSlideDownAnimationCompleted(object sender, object e)
#elif SILVERLIGHT
        private void OnSlideDownAnimationCompleted(object sender, EventArgs e)
#endif
        {
            if (Minimized)
            {
                // The state did not change
                SetValuesBasedOnState(true);
            }
            else
            {
                Minimized = true;
            }

            if (AnimatingComplete != null)
            {
                AnimatingComplete(this, null);
            }
        }

#if NETFX_CORE
        private void OnSlideUpAnimationCompleted(object sender, object e)
#elif SILVERLIGHT
        private void OnSlideUpAnimationCompleted(object sender, EventArgs e)
#endif
        {
            if (Minimized)
            {
                Minimized = false;
            }
            else
            {
                // The state did not change
                SetValuesBasedOnState(false);
            }

            if (AnimatingComplete != null)
            {
                AnimatingComplete(this, null);
            }
        }

        private void OnMinimizeButtonClicked(object sender, RoutedEventArgs e)
        {
            Minimize();
        }

        private void OnMinimizedPanelClicked(object sender, RoutedEventArgs e)
        {
            if (panelTranslateTransform.Y > MaxTransformY - JitterThreshold)
            {
                _isBeingManipulated = false;
                Maximize();
            }
        }

        #endregion
    }
}
