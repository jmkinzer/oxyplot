// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StemSeries.cs" company="OxyPlot">
//   http://oxyplot.codeplex.com, license: Ms-PL
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OxyPlot
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a series that plots discrete data in a stem plot.
    /// </summary>
    /// <remarks>
    /// http://en.wikipedia.org/wiki/Stemplot
    ///   http://www.mathworks.com/help/techdoc/ref/stem.html
    /// </remarks>
    public class StemSeries : LineSeries
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "StemSeries" /> class.
        /// </summary>
        public StemSeries()
        {
            this.Base = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StemSeries"/> class.
        /// </summary>
        /// <param name="title">
        /// The title.
        /// </param>
        public StemSeries(string title)
            : base(title)
        {
            this.Title = title;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StemSeries"/> class.
        /// </summary>
        /// <param name="color">
        /// The color of the line stroke.
        /// </param>
        /// <param name="strokeThickness">
        /// The stroke thickness (optional).
        /// </param>
        /// <param name="title">
        /// The title (optional).
        /// </param>
        public StemSeries(OxyColor color, double strokeThickness = 1, string title = null)
            : base(color, strokeThickness, title)
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets or sets Base.
        /// </summary>
        public double Base { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the point on the series that is nearest the specified point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="interpolate">Interpolate the series if this flag is set to <c>true</c>.</param>
        /// <returns>
        /// A TrackerHitResult for the current hit.
        /// </returns>
        public override TrackerHitResult GetNearestPoint(ScreenPoint point, bool interpolate)
        {
            if (interpolate)
            {
                return null;
            }

            TrackerHitResult result = null;

            // http://local.wasp.uwa.edu.au/~pbourke/geometry/pointline/
            double minimumDistance = double.MaxValue;
            var points = this.Points;

            for (int i = 0; i < points.Count; i++)
            {
                var p1 = points[i];
                var basePoint = new DataPoint(p1.X, this.Base);
                var sp1 = this.Transform(p1);
                var sp2 = this.Transform(basePoint);
                var u = ScreenPointHelper.FindPositionOnLine(point, sp1, sp2);

                if (double.IsNaN(u))
                {
                    continue;
                }

                if (u < 0 || u > 1)
                {
                    continue; // outside line
                }

                var sp = sp1 + ((sp2 - sp1) * u);
                double distance = (point - sp).LengthSquared;

                if (distance < minimumDistance)
                {
                    result = new TrackerHitResult(
                        this,
                        new DataPoint(p1.X, p1.Y),
                        new ScreenPoint(sp1.x, sp1.y),
                        this.GetItem(i));
                    minimumDistance = distance;
                }
            }

            return result;
        }

        /// <summary>
        /// Renders the LineSeries on the specified rendering context.
        /// </summary>
        /// <param name="rc">
        /// The rendering context.
        /// </param>
        /// <param name="model">
        /// The owner plot model.
        /// </param>
        public override void Render(IRenderContext rc, PlotModel model)
        {
            if (this.Points.Count == 0)
            {
                return;
            }

            if (this.XAxis == null || this.YAxis == null)
            {
                Trace("Axis not defined.");
                return;
            }

            double minDistSquared = this.MinimumSegmentLength * this.MinimumSegmentLength;

            var clippingRect = this.GetClippingRect();

            // Transform all points to screen coordinates
            // Render the line when invalid points occur
            var markerPoints = new List<ScreenPoint>();
            foreach (var point in this.Points)
            {
                if (!this.IsValidPoint(point, this.XAxis, this.YAxis))
                {
                    continue;
                }

                var p0 = this.Transform(point.X, this.Base);
                var p1 = this.Transform(point.X, point.Y);

                if (this.StrokeThickness > 0 && this.LineStyle != LineStyle.None)
                {
                    rc.DrawClippedLine(
                        new[] { p0, p1 },
                        clippingRect,
                        minDistSquared,
                        this.GetSelectableColor(this.ActualColor),
                        this.StrokeThickness,
                        this.LineStyle,
                        this.LineJoin,
                        false);
                }

                markerPoints.Add(p1);
            }

            if (this.MarkerType != MarkerType.None)
            {
                rc.DrawMarkers(
                    markerPoints,
                    clippingRect,
                    this.MarkerType,
                    this.MarkerOutline,
                    new[] { this.MarkerSize },
                    this.MarkerFill,
                    this.MarkerStroke,
                    this.MarkerStrokeThickness);
            }
        }

        #endregion
    }
}