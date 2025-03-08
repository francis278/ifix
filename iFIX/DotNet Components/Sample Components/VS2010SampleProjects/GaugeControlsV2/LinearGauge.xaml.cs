using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;
using System.Runtime.CompilerServices;

namespace GaugeControlsV2
{
    /// <summary>
    /// Delegate to raise a value change event.
    /// </summary>
    /// <param name="prevValue">
    /// The value right before the value change.
    /// </param>
    /// <param name="currValue">
    /// The new value.
    /// </param>
    public delegate void ValueChangeHandler(double prevValue, double currValue);

    /// <summary>
    /// Interaction logic for LinearGauge.xaml
    /// </summary>
    public partial class LinearGauge : UserControl
    {
        #region Private data fields

        // Reading values and limits
        private double _reading = 50;
        private double _prevReading = 50;
        private double _minReading = 0;
        private double _maxReading = 100;

        // Reading ranges
        private double _range1High = 25;
        private double _range2High = 75;
        private double _blinkSeconds = 4;

        // Major scale mark
        private double _majorTicks = 5;       
        private double _majorTickHeight = 24;
        private double _majorTickThickness = 2;
        private Brush _majorTickBrush = Brushes.DarkSalmon;

        // Minor scale mark
        private double _minorTicks = 4;
        private double _minorTickHeight = 12;
        private double _minorTickThickness = 1;
        private Brush _minorTickBrush = Brushes.DarkKhaki;

        // Scale label
        private FontFamily _scaleFontFamily = new FontFamily("Tahoma");
        private FontStyle _scaleFontStyle = FontStyles.Normal;
        private FontWeight _scaleFontWeight = FontWeights.Normal;
        private double _scaleFontSize = 16;
        private Brush _scaleFontBrush = Brushes.Brown;
        private int _decimalPlaces = 0;

        // Mouse data for needle reset
        private double _dragStart = double.MinValue;
        private double _needleTranslate = 0;
 
        #endregion

        public event ValueChangeHandler ReadingLowerToRange1;
        public event ValueChangeHandler ReadingHigherToRange2;
        public event ValueChangeHandler ReadingLowerToRange2;
        public event ValueChangeHandler ReadingHigherToRange3;
        public event ValueChangeHandler NeedleReset;

        public LinearGauge()
        {
            InitializeComponent();

            GaugeView.SizeChanged += new SizeChangedEventHandler(GaugeView_SizeChanged);
            NeedlePath.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(NeedlePath_PreviewMouseLeftButtonDown);
            NeedlePath.PreviewMouseMove += new MouseEventHandler(NeedlePath_PreviewMouseMove);
            NeedlePath.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(NeedlePath_PreviewMouseLeftButtonUp);
        }

        #region Publuc properties

        public double RimThickness
        {
            get { return RimBorder.BorderThickness.Left; }
            set
            {
                RimBorder.BorderThickness = new Thickness(value);

                // Make the outer border fit 
                OutBorder.CornerRadius = new CornerRadius(OutBorder.BorderThickness.Left + RimBorder.BorderThickness.Left / 2);
            }
        }

        public System.Drawing.Color RimColor
        {
            get { return ToDrawingColor(RimColor1.Color); }
            set
            {
                RimColor1.Color = ToMediaColor(value);
                double[] hsl = ColorConversion.ColorToHSL(value);
                RimColor2.Color = ToMediaColor(ColorConversion.HSLToColor(hsl[0], hsl[1] * 1.3, hsl[2] * 1.3));
                RimColor3.Color = ToMediaColor(ColorConversion.HSLToColor(hsl[0], hsl[1] * 0.8, hsl[2] * 0.8));
                OutColor1.Color = ToMediaColor(ColorConversion.HSLToColor(hsl[0], hsl[1] * 1.2, hsl[2] * 1.2));
                OutColor2.Color = ToMediaColor(ColorConversion.HSLToColor(hsl[0], hsl[1] * 1.4, hsl[2] * 1.4));          
                OutColor3.Color = ToMediaColor(ColorConversion.HSLToColor(hsl[0], hsl[1] * 0.6, hsl[2] * 0.6));
            }
        }

        public System.Drawing.Color BackgroundColor
        {
            get { return ToDrawingColor(FrameColor1.Color); }
            set
            {
                FrameColor1.Color = ToMediaColor(value);
                double[] hsl = ColorConversion.ColorToHSL(value);
                FrameColor2.Color = ToMediaColor(ColorConversion.HSLToColor(hsl[0], hsl[1] * 1.6, hsl[2] * 1.6));
                FrameColor3.Color = ToMediaColor(ColorConversion.HSLToColor(hsl[0], hsl[1] * 1.2, hsl[2] * 1.2));              
            }
        }

        public double Reading
        {
            get { return _reading; }
            set
            {
                SetReading(value);

                if (_prevReading > _range1High && _reading < _range1High)
                {
                    // Reading lower to range 1
                    AnimateColor(Range1);

                    if (ReadingLowerToRange1 != null)
                        ReadingLowerToRange1(_prevReading, Reading);
                }
                else if (_prevReading < _range1High && _reading > _range1High)
                {
                    // Reading higher to range 2
                    AnimateColor(Range2);

                    if (ReadingHigherToRange2 != null)
                        ReadingHigherToRange2(_prevReading, Reading);
                }
                else if (_prevReading > _range2High && _reading < _range2High)
                {
                    // Reading lower to range 2
                    AnimateColor(Range2);

                    if (ReadingLowerToRange2 != null)
                        ReadingLowerToRange2(_prevReading, Reading);
                }
                else if (_prevReading < _range2High && _reading > _range2High)
                {
                    // Reading higher to range 3
                    AnimateColor(Range3);

                    if (ReadingHigherToRange3 != null)
                        ReadingHigherToRange3(_prevReading, Reading);
                }
            }
        }

        public double MinReading
        {
            get { return _minReading; }
            set
            {
                _minReading = value;
                _range1High = Math.Max(_range1High, _minReading);
                _range2High = Math.Max(_range2High, _minReading);
                _maxReading = Math.Max(_maxReading, _minReading);

                DrawScales();
                DrawRanges();
            }
        }

        public double MaxReading
        {
            get { return _maxReading; }
            set
            {
                _maxReading = value;
                _minReading = Math.Min(_minReading, _maxReading);
                _range1High = Math.Min(_range1High, _maxReading);
                _range2High = Math.Min(_range2High, _maxReading);

                DrawScales();
                DrawRanges();
            }
        }

        public double Range1High
        {
            get { return _range1High; }
            set
            {
                _range1High = value;
                _minReading = Math.Min(_minReading, _range1High);
                _range2High = Math.Max(_range2High, _range1High);
                _maxReading = Math.Max(_maxReading, _range1High);

                DrawRanges();               
            }
        }

        public double Range2High
        {
            get { return _range2High; }
            set
            {
                _range2High = value;
                _minReading = Math.Min(_minReading, _range2High);
                _range1High = Math.Min(_range1High, _range2High);
                _maxReading = Math.Max(_maxReading, _range2High);

                DrawRanges();
            }
        }

        public double BlinkSeconds
        {
            get { return _blinkSeconds; }
            set { _blinkSeconds = value; }
        }

        public System.Drawing.Color Range1Color
        {
            get { return ToDrawingColor(((SolidColorBrush)Range1.Fill).Color); }
            set { Range1.Fill = new SolidColorBrush(ToMediaColor(value)); }
        }

        public System.Drawing.Color Range2Color
        {
            get { return ToDrawingColor(((SolidColorBrush)Range2.Fill).Color); }
            set { Range2.Fill = new SolidColorBrush(ToMediaColor(value)); }
        }

        public System.Drawing.Color Range3Color
        {
            get { return ToDrawingColor(((SolidColorBrush)Range3.Fill).Color); }
            set { Range3.Fill = new SolidColorBrush(ToMediaColor(value)); }
        }

        public double MajorTicks
        {
            get { return _majorTicks; }
            set
            {
                _majorTicks = Math.Max(value, 0);
                DrawScales();
            }
        }

        public double MajorTickHeight
        {
            get { return _majorTickHeight; }
            set
            {
                _majorTickHeight = Math.Min(Math.Max(value, 4), ScaleGrid.Height - 2 * _scaleFontSize);
                RangeBand.Height = _majorTickHeight;
                DrawScales();
            }
        }

        public double MajorTickThickness
        {
            get { return _majorTickThickness; }
            set
            {
                _majorTickThickness = Math.Max(value, 1);
                DrawScales();
            }
        }

        public Brush MajorTickBrush
        {
            get { return _majorTickBrush; }
            set
            {
                _majorTickBrush = value;
                DrawScales();
            }
        }

        public double MinorTicks
        {
            get { return _minorTicks; }
            set
            {
                _minorTicks = Math.Max(value, 0);
                DrawScales();
            }
        }

        public double MinorTickHeight
        {
            get { return _minorTickHeight; }
            set
            {
                _minorTickHeight = Math.Min(Math.Max(value, 2), MajorTickHeight);
                DrawScales();
            }
        }

        public double MinorTickThickness
        {
            get { return _minorTickThickness; }
            set
            {
                _minorTickThickness = Math.Max(value, 1);
                DrawScales();
            }
        }

        public Brush MinorTickBrush
        {
            get { return _minorTickBrush; }
            set
            {
                _minorTickBrush = value;
                DrawScales();
            }
        }

        public FontFamily ScaleFontFamily
        {
            get { return _scaleFontFamily; }
            set
            {
                _scaleFontFamily = value;
                DrawScales();
            }
        }

        public FontStyle ScaleFontStyle
        {
            get { return _scaleFontStyle; }
            set
            {
                _scaleFontStyle = value;
                DrawScales();
            }
        }

        public FontWeight ScaleFontWeight
        {
            get { return _scaleFontWeight; }
            set
            {
                _scaleFontWeight = value;
                DrawScales();
            }
        }

        public double ScaleFontSize
        {
            get { return _scaleFontSize; }
            set
            {
                _scaleFontSize = Math.Min(Math.Max(value, 12), (ScaleGrid.Height - MajorTickHeight) / 2);
                DrawScales();
            }
        }

        public Brush ScaleFontBrush
        {
            get { return _scaleFontBrush; }
            set
            {
                _scaleFontBrush = value;
                DrawScales();
            }
        }

        public string Title
        {
            get { return TitleBlock.Text; }
            set { TitleBlock.Text = value; }
        }

        public FontFamily TitleFontFamily
        {
            get { return TitleBlock.FontFamily; }
            set { TitleBlock.FontFamily = value; }
        }

        public FontStyle TitleFontStyle
        {
            get { return TitleBlock.FontStyle; }
            set { TitleBlock.FontStyle = value; }
        }

        public FontWeight TitleFontWeight
        {
            get { return TitleBlock.FontWeight; }
            set { TitleBlock.FontWeight = value; }
        }

        public double TitleFontSize
        {
            get { return TitleBlock.FontSize; }
            set { TitleBlock.FontSize = value; }
        }

        public Brush TitleBrush
        {
            get { return TitleBlock.Foreground; }
            set { TitleBlock.Foreground = value; }
        }

        public double NeedleWidth
        {
            get { return NeedlePath.Width; }
            set { NeedlePath.Width = Math.Min(value, ScaleGrid.Margin.Left); }
        }

        public Brush NeedleFill
        {
            get { return NeedlePath.Fill; }
            set { NeedlePath.Fill = value; }
        }

        public Brush NeedleStroke
        {
            get { return NeedlePath.Stroke; }
            set { NeedlePath.Stroke = value; }
        }

        public int DecimalPlaces
        {
            get { return _decimalPlaces; }
            set
            {
                _decimalPlaces = value;
                DrawScales();
            }
        }
 
        #endregion

        /// <summary>
        /// Handler to control size change -- redraw the scales.
        /// </summary>
        /// <param name="sender">
        /// The sender of the event.
        /// </param>
        /// <param name="e">
        /// An event argument object.
        /// </param>
        void GaugeView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawScales();
        }

        /// <summary>
        /// Draw the color bars for value ranges.
        /// </summary>
        private void DrawRanges()
        {
            Range1.Width = (_range1High - _minReading) / (_maxReading - _minReading) * ScaleGrid.Width;
            Range2.Width = (_range2High - _range1High) / (_maxReading - _minReading) * ScaleGrid.Width;
            Range3.Width = (_maxReading - _range2High) / (_maxReading - _minReading) * ScaleGrid.Width;
        }

        /// <summary>
        /// Draw major and minor scale marks and number labels.
        /// </summary>
        private void DrawScales()
        {
            ScaleGrid.Children.Clear();
            Reading = _reading;

            // Adjust shadow depth so that the shadows can be seen even if the control is reduced to a small size
            TitleEffect.ShadowDepth = Math.Max(ScaleGrid.ActualHeight / GaugeView.ActualHeight, ScaleGrid.ActualWidth / GaugeView.ActualWidth);
            DropShadowEffect effect = CreateShadowEffect(TitleEffect.ShadowDepth, true);

            // Draw major scale marks            
            for (int i = 0; i <= _majorTicks; i++)
            {
                // Translate needed to place a major scale mark
                double majorTickShift = ScaleGrid.Width * (i * 1.0 / _majorTicks - 0.5);

                // Create a line to draw the major mark
                Line majorTick = new Line();
                majorTick.X1 = majorTick.X2 = ScaleGrid.ActualWidth / 2;
                majorTick.Y1 = (ScaleGrid.ActualHeight - _majorTickHeight) / 2;
                majorTick.Y2 = majorTick.Y1 + _majorTickHeight;
                majorTick.StrokeThickness = _majorTickThickness;
                majorTick.Stroke = _majorTickBrush;
                majorTick.Effect = effect;

                // Move the line to the mark position and draw it
                majorTick.RenderTransform = new TranslateTransform(majorTickShift, 0);
                ScaleGrid.Children.Add(majorTick);

                if (i < _majorTicks)    // The last major mark is not followed by any minor marks 
                {
                    // Draw minor scale marks that follow the major mark
                    for (int j = 1; j < _minorTicks; j++)
                    {
                        // Additional translate needed to place a minor scale mark
                        double minorTickShift = j * ScaleGrid.Width / _majorTicks / _minorTicks;

                        // Create a line to draw the injor mark
                        Line minorTick = new Line();
                        minorTick.X1 = minorTick.X2 = ScaleGrid.ActualWidth / 2;
                        minorTick.Y1 = (ScaleGrid.ActualHeight - _minorTickHeight) / 2;
                        minorTick.Y2 = minorTick.Y1 + _minorTickHeight;
                        minorTick.StrokeThickness = _minorTickThickness;
                        minorTick.Stroke = _minorTickBrush;
                        minorTick.Effect = effect;

                        // Move the line to the mark position and draw it
                        minorTick.RenderTransform = new TranslateTransform(majorTickShift + minorTickShift, 0);
                        ScaleGrid.Children.Add(minorTick);
                    }
                }

                // Create a scale label for each major scale mark
                TextBlock text = new TextBlock();
                double value = _minReading + (_maxReading - _minReading) / _majorTicks * i;
                text.Text = value.ToString("F" + _decimalPlaces);
                text.Foreground = _scaleFontBrush;
                text.FontFamily = _scaleFontFamily;
                text.FontStyle = _scaleFontStyle;
                text.FontWeight = _scaleFontWeight;
                text.FontSize = _scaleFontSize;
                text.Effect = effect;
                text.TextAlignment = TextAlignment.Center;
                text.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                text.VerticalAlignment = System.Windows.VerticalAlignment.Center;

                // Position the label right over the major scale mark and draw it
                text.RenderTransform = new TranslateTransform(majorTickShift, -(_majorTickHeight + text.FontSize) / 1.8);
                ScaleGrid.Children.Add(text);
            }
        }

        /// <summary>
        /// Create a shadow or bright line effect.
        /// </summary>
        /// <param name="depth">
        /// The depth of the shadow or bright line.
        /// </param>
        /// <param name="shadow">
        /// Whether a shadow or bright line is needed.
        /// </param>
        /// <returns>
        /// A DropShadowEffect object for drawing the effect.
        /// </returns>
        private DropShadowEffect CreateShadowEffect(double depth, bool shadow)
        {
            DropShadowEffect effect = new DropShadowEffect();
            effect.ShadowDepth = depth;
            effect.BlurRadius = 1;

            if (shadow)
            {
                // Dark shadow in southeast
                effect.Direction = 315;
                effect.Color = Colors.Black;
            }
            else
            {
                // Bright line in northwest
                effect.Direction = 135;
                effect.Color = Colors.White;
            }
            
            return effect;
        }

        /// <summary>
        /// Set needle position for a new reading value.
        /// </summary>
        /// <param name="reading">
        /// A new reading value.
        /// </param>
        private void SetReading(double reading)
        {
            _prevReading = _reading;
            _reading = reading;

            // Extreme reading limits within the scale grid
            double minLimit = _minReading - (_maxReading - _minReading) * (ScaleGrid.Margin.Left - NeedlePath.Width / 2) / ScaleGrid.Width;
            double maxLimit = _maxReading + (_maxReading - _minReading) * (ScaleGrid.Margin.Left - NeedlePath.Width / 2) / ScaleGrid.Width;

            // Keep needle within the scale grid
            if (_reading < minLimit)
                _reading = minLimit;
            else if (_reading > maxLimit)
                _reading = maxLimit;

            // Move the needle
            double position = ((_reading - _minReading) / (_maxReading - _minReading) - 0.5) * ScaleGrid.Width;
            NeedlePath.RenderTransform = new TranslateTransform(position, 0);
        }

        /// <summary>
        /// Make a reading range band blink.
        /// </summary>
        /// <param name="band">
        /// A Rectangle object whose color is to be animated.
        /// </param> 
        private void AnimateColor(Rectangle band)
        {
            // Switch the band brush
            SolidColorBrush animatedBrush = new SolidColorBrush();
            animatedBrush.Color = ((SolidColorBrush)band.Fill).Color;
            band.Fill = animatedBrush;

            // Register the brush so that a Storyboard can access it
            NameScope.SetNameScope(this, new NameScope());
            this.RegisterName("AnimatedBrush", animatedBrush);

            // Use an double animation to change color opacity for a blinking effect
            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.To = 0;
            opacityAnimation.From = 1;
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.1);
            opacityAnimation.AutoReverse = true;
            opacityAnimation.RepeatBehavior = new RepeatBehavior(TimeSpan.FromSeconds(_blinkSeconds));

            // Add the animation to a storyboard and begin it
            Storyboard.SetTargetName(opacityAnimation, "AnimatedBrush");
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(opacityAnimation);
            storyboard.Begin(this);
        }

        /// <summary>
        /// Convert a System.Windows.Media.Color to a System.Drawing.Color object.
        /// </summary>
        /// <param name="color">
        /// A System.Windows.Media.Color object.
        /// </param>
        /// <returns>
        /// A System.Drawing.Color object.
        /// </returns>
        private System.Drawing.Color ToDrawingColor(Color color)
        {
            return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// Convert a System.Drawing.Color to a System.Windows.Media.Color object.
        /// </summary>
        /// <param name="color">
        /// A System.Drawing.Color object.
        /// </param>
        /// <returns>
        /// A System.Windows.Media.Color object.
        /// </returns>
        private Color ToMediaColor(System.Drawing.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        #region Event handlers for needle reset

        void NeedlePath_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NeedlePath.CaptureMouse();

            // Record the current needle translate and mouse start point
            _dragStart = e.GetPosition(ScaleGrid).X;
            _needleTranslate = ScaleGrid.Width * ((_reading - _minReading) / (_maxReading - _minReading) - 0.5);
        }

        void NeedlePath_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_dragStart != double.MinValue)
            {
                // Move the needle with mouse dragging
                double mouseMove = Math.Min(Math.Max(e.GetPosition(ScaleGrid).X, 0), ScaleGrid.Width) - _dragStart;
                NeedlePath.RenderTransform = new TranslateTransform(_needleTranslate + mouseMove, 0);
            }
        }

        void NeedlePath_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            double mouseMove = Math.Min(Math.Max(e.GetPosition(ScaleGrid).X, 0), ScaleGrid.Width) - _dragStart;
            
            if (mouseMove != 0)
            {
                // Set the needle to the finish position
                SetReading(Reading + mouseMove / ScaleGrid.Width * (_maxReading - _minReading));
                
                // Raise a needle reset event
                if (NeedleReset != null)
                    NeedleReset(_prevReading, _reading);
            }

            NeedlePath.ReleaseMouseCapture();
            _dragStart = double.MinValue;
        }

        #endregion
    }
}
