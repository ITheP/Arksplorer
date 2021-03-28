namespace Arksplorer
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// Defines the <see cref="Angleatron" />
    /// </summary>
    public class Angleatron
    {
        /// <summary>
        /// Defines the ConvToRad
        /// </summary>
        public const double ConvToRad = Math.PI / 180.0d;

        // angle goes 0->360 0->360 0->360 etc.
        /// <summary>
        /// Gets the AngleSpeed
        /// </summary>
        private double AngleSpeed { get; }

        /// <summary>
        /// Gets or sets the CurrentAngle
        /// </summary>
        private double CurrentAngle { get; set; }

        /// <summary>
        /// Gets or sets the WorkingAngle
        /// </summary>
        private double WorkingAngle { get; set; }

        /// <summary>
        /// Gets or sets the RadAngle
        /// </summary>
        private double RadAngle { get; set; }

        /// <summary>
        /// Gets or sets the SinePoint
        /// </summary>
        private double SinePoint { get; set; }

        /// <summary>
        /// Gets the RequiredRange
        /// </summary>
        private double RequiredRange { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Angleatron"/> class.
        /// </summary>
        /// <param name="startAngle">The startAngle<see cref="double"/></param>
        /// <param name="angleSpeed">The angleSpeed<see cref="double"/></param>
        /// <param name="requiredRange">The requiredRange<see cref="double"/></param>
        public Angleatron(double startAngle, double angleSpeed, double requiredRange)
        {
            CurrentAngle = startAngle;
            AngleSpeed = angleSpeed;
            RequiredRange = requiredRange;
        }

        /// <summary>
        /// The GetNextValue
        /// </summary>
        /// <returns>The <see cref="double"/></returns>
        public double GetNextValue()
        {
            CurrentAngle += AngleSpeed;

            WorkingAngle = CurrentAngle % 360.0;

            RadAngle = ConvToRad * WorkingAngle;

            SinePoint = Math.Sin(RadAngle);

            // so SinePoint should be 0 -> 1 -> 0 -> -1 -> 0 etc.
            return SinePoint * RequiredRange;
        }
    }

    /// <summary>
    /// Defines the <see cref="Spinner" />
    /// </summary>
    public partial class Spinner
    {
        /// <summary>
        /// Defines the ConvToRad
        /// </summary>
        public const double ConvToRad = Math.PI / 180.0d;

        /// <summary>
        /// Gets or sets the Timer
        /// </summary>
        public Stopwatch Timer { get; set; } = new Stopwatch();

        /// <summary>
        /// Gets or sets a value indicating whether AutoTimeBased
        /// </summary>
        public bool AutoTimeBased { get; set; } = true;

        /// <summary>
        /// Defines the Angleatron
        /// </summary>
        private Angleatron Angleatron;

        /// <summary>
        /// Initializes a new instance of the <see cref="Spinner"/> class.
        /// </summary>
        public Spinner()
        {
            InitSpinner(true, 0.0d);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Spinner"/> class.
        /// </summary>
        /// <param name="DefaultAngle">The DefaultAngle<see cref="double"/></param>
        public Spinner(double DefaultAngle)
        {
            InitSpinner(false, DefaultAngle);
        }

        /// <summary>
        /// The InitSpinner
        /// </summary>
        /// <param name="autoTimeBased">The autoTimeBased<see cref="bool"/></param>
        /// <param name="defaultAngle">The defaultAngle<see cref="double"/></param>
        private void InitSpinner(bool autoTimeBased, double defaultAngle)
        {
            InitializeComponent();
            Timer.Start();

            SetCircles();

            CompositionTarget.Rendering += OnRender;

            E1.StrokeThickness = 50.0d;
            E2.StrokeThickness = E1.StrokeThickness;
            E3.StrokeThickness = E1.StrokeThickness;
            E4.StrokeThickness = E1.StrokeThickness;
            //E5.StrokeThickness = E1.StrokeThickness;

            AutoTimeBased = autoTimeBased;

            if (!autoTimeBased)
                Angleatron = new Angleatron(defaultAngle, 0.169d, 9669.0d);
        }

        /// <summary>
        /// The SetCircles
        /// </summary>
        public void SetCircles()
        {
            double when;
            double ms = (double)Timer.ElapsedMilliseconds;

            if (AutoTimeBased)
            {
                when = ms;
            }
            else
            {
                when = Angleatron.GetNextValue();
            }

            Thickness = (Math.Sin(ms / 1000.0) * 12.5d) + 37.5d;
            MoveScale = (Math.Sin(ms / 1723.0) * 20.0d) + 45.0d;

            E1.StrokeThickness = Thickness;
            SetCircleTB(E1, E2, when, 1200, 360.0d);
            SetCircleTB(E2, E3, when, 937, -360.0d);
            SetCircleTB(E3, E4, when, 873, 360.0d);
        }

        private static double Thickness = 25.0; // Between 20.0 and 40.0
        private static double MoveScale = 25.0; // Between 25.0 and 50.0

        /// <summary>
        /// The SetCircleTB
        /// </summary>
        /// <param name="parent">The parent<see cref="FrameworkElement"/></param>
        /// <param name="child">The child<see cref="FrameworkElement"/></param>
        /// <param name="timeMS">The timeMS<see cref="double"/></param>
        /// <param name="maxTimeMS">The maxTimeMS<see cref="double"/></param>
        /// <param name="direction">The direction<see cref="double"/></param>
        public static void SetCircleTB(FrameworkElement parent, FrameworkElement child, double timeMS, double maxTimeMS, double direction)
        {
            // e.g. 1500ms % 1000ms = 500ms in current loop
            // 500ms / 1000ms = 0.5
            // * 360.0 = 180.0 degrees rotated
            double curTime = timeMS % maxTimeMS; // (basically 0->timespan repeated, or 0->359 degrees after calculation)
            double angle = (curTime / maxTimeMS) * direction;
            double angleRad = ConvToRad * angle;

            TransformGroup ptg = parent.RenderTransform as TransformGroup;
            TransformGroup ctg = child.RenderTransform as TransformGroup;

            double x = ((TranslateTransform)ptg.Children[0]).X;
            double y = ((TranslateTransform)ptg.Children[0]).Y;

            double shrinkage = Thickness + MoveScale - 1.0; // - 1.0 helps just match edges up that little more solidly

            ((TranslateTransform)(ctg.Children[0])).X = x + (Math.Cos(angleRad) * MoveScale);
            ((TranslateTransform)(ctg.Children[0])).Y = y + (Math.Sin(angleRad) * MoveScale);

            ((System.Windows.Shapes.Ellipse)child).StrokeThickness = Thickness;
            double width = parent.Width - Thickness - MoveScale - shrinkage;
            child.Width = (width < 0.0 ? 0.0 : width);
            double height = parent.Height - Thickness - MoveScale - shrinkage;
            child.Height = (height < 0.0d ? 0.0d : height);
        }

        /// <summary>
        /// The OnRender
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/></param>
        /// <param name="e">The e<see cref="EventArgs"/></param>
        protected void OnRender(object sender, EventArgs e)
        {
            SetCircles();
        }
    }
}