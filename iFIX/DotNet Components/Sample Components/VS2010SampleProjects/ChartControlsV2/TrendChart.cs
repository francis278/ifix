using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ChartControlsV2
{
    /// <summary>
    /// A simple trend chart class to showcase the simplicity of writing a WinForms component.
    /// </summary>
    public class TrendChart : Chart
    {
        // Trending value
        private double _value;

        // Chart drawing area
        private ChartArea _area;

        // Chart data series for the trending value points
        private Series _series;

        // Chart title object
        private Title _title;

        // Event raised when mouse is up in the chart drawing area
        public event MouseEventHandler ChartAreaMouseUp;

        public TrendChart()
        {
            BackColor = Color.Silver;
            BorderSkin.SkinStyle = BorderSkinStyle.FrameThin5;

            // Initialize a chart title
            _title = Titles.Add("Trend Chart");
            _title.Font = new Font("Arial", 10, FontStyle.Bold);

            // Initialize a chart area
            _area = ChartAreas.Add("area0");
            _area.AxisX.LabelStyle.Format = "hh:mm:ss";
            _area.AxisX.LabelStyle.Interval = 5;
            _area.AxisX.LabelStyle.IntervalType = DateTimeIntervalType.Seconds;
            _area.AxisX.MajorGrid.Interval = 5;
            _area.AxisX.MajorGrid.IntervalType = DateTimeIntervalType.Seconds;
            _area.BackColor = Color.Orange;
            _area.BackGradientStyle = GradientStyle.TopBottom;

            // Add a chart series
            _series = Series.Add("series0");
            _series.ChartType = SeriesChartType.Line;
            _series.ShadowOffset = 1;
        }

        /// <summary>
        /// Get and set the chart drawing type
        /// </summary>
        public SeriesChartType ChartType
        {
            get { return _series.ChartType; }
            set { _series.ChartType = value; }
        }

        /// <summary>
        /// Get and set the chart title text.
        /// </summary>
        public string Title
        {
            get { return _title.Text; }
            set { _title.Text = value; }
        }

        /// <summary>
        /// Get and set the title font.
        /// </summary>
        public Font TitleFont
        {
            get { return _title.Font; }
            set { _title.Font = value; }
        }

        /// <summary>
        /// Get and set the title font color.
        /// </summary>
        public Color TitleColor
        {
            get { return _title.ForeColor; }
            set { _title.ForeColor = value; }
        }

        /// <summary>
        /// Get the current value.
        /// Set the current value -- add a new data point to the series 
        /// </summary>
        public double Value
        {
            get { return _value; }
            set
            {
                _value = value;
                DateTime current = DateTime.Now;
                
                // Add a new point for the new value
                _series.Points.AddXY(current.ToOADate(), _value);

                // Remove the points that are older than 25 seconds
                double removeBefore = current.AddSeconds(-25).ToOADate();
                while (_series.Points[0].XValue < removeBefore)
                    _series.Points.RemoveAt(0);

                // Reset the minimum and maximum
                _area.AxisX.Minimum = _series.Points[0].XValue;
                _area.AxisX.Maximum
                    = DateTime.FromOADate(_series.Points[0].XValue).AddSeconds(30).ToOADate();
            }
        }

        /// <summary>
        /// Raise a ChartAreaMouseUp event.
        /// </summary>
        /// <param name="e">
        /// A WinForms MouseEventArgs object.
        /// </param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (ChartAreaMouseUp != null)
                ChartAreaMouseUp(this, e);
        }
    }
}
