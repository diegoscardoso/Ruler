using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace Ruler
{
    /// <summary>
    /// Full-virtual-screen, always-on-top layered window that lets the user draw
    /// measuring lines in any direction:
    ///  - With no lines (or after pressing N): the whole screen captures the mouse
    ///    (crosshair cursor); press-drag-release creates a line.
    ///  - Otherwise everything except the lines is click-through, so other
    ///    applications remain usable. Clicking a line selects it; drag an endpoint
    ///    handle of the selected line to resize, drag the body to move it.
    ///  - N (with a line selected/focused) arms drawing of an additional line.
    ///  - Esc deletes the selected line (or cancels an armed N); Esc with no lines
    ///    exits the app.
    ///  - 1 / 2 set the selected line's color (LimeGreen / Red), and the default
    ///    for new lines.
    /// </summary>
    public class RulerOverlay : Form
    {
        private enum DragMode { None, Drawing, MoveStart, MoveEnd, MoveLine }

        private sealed class MeasureLine
        {
            public Point Start;
            public Point End;
            public Color Color;
            public int Width;
        }

        #region Private Fields
        private const int MinLineWidth = 1;
        private const int MaxLineWidth = 10;
        private const int HandleRadius = 5;
        private const int GrabRadius = 10;
        private const int MinLineLength = 3;
        private const int LabelOffset = 18;

        // Alpha 1 is visually imperceptible but still receives mouse input,
        // unlike alpha 0 which is click-through (see NativeMethods).
        private static readonly Color HitOnlyColor = Color.FromArgb(1, 0, 0, 0);

        private readonly List<MeasureLine> lines = new();
        private MeasureLine? selected;
        private bool newLineArmed; // set by N: next drag draws an additional line
        private DragMode drag = DragMode.None;
        private Point lastMousePos;
        private Color defaultColor = Color.LimeGreen;
        private int defaultWidth = 1;

        private Bitmap? surface;
        private readonly Font labelFont = new Font("Segoe UI", 9f, FontStyle.Bold);
        #endregion

        #region Constructors
        public RulerOverlay()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Bounds = SystemInformation.VirtualScreen;
            TopMost = true;
            ShowInTaskbar = true;
            Text = "Ruler";
            Cursor = Cursors.Cross;
        }
        #endregion

        #region Overrides
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= NativeMethods.WS_EX_LAYERED;
                return cp;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Render();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;

            Activate(); // reclaim keyboard focus so Esc/N reach this window

            if (lines.Count == 0 || newLineArmed)
            {
                newLineArmed = false;
                var line = new MeasureLine { Start = e.Location, End = e.Location, Color = defaultColor, Width = defaultWidth };
                lines.Add(line);
                selected = line;
                drag = DragMode.Drawing;
                Render();
                return;
            }

            // Endpoint handles are only active on the selected line.
            if (selected != null && Distance(e.Location, selected.Start) <= GrabRadius)
            {
                drag = DragMode.MoveStart;
            }
            else if (selected != null && Distance(e.Location, selected.End) <= GrabRadius)
            {
                drag = DragMode.MoveEnd;
            }
            else
            {
                // Click landed on some line's visible pixels (body or label):
                // select the closest line and start moving it.
                selected = FindNearestLine(e.Location);
                drag = DragMode.MoveLine;
                lastMousePos = e.Location;
                Render();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            switch (drag)
            {
                case DragMode.Drawing:
                case DragMode.MoveEnd:
                    if (selected != null)
                    {
                        selected.End = e.Location;
                        Render();
                    }
                    break;

                case DragMode.MoveStart:
                    if (selected != null)
                    {
                        selected.Start = e.Location;
                        Render();
                    }
                    break;

                case DragMode.MoveLine:
                    if (selected != null)
                    {
                        var delta = new Size(e.X - lastMousePos.X, e.Y - lastMousePos.Y);
                        selected.Start += delta;
                        selected.End += delta;
                        lastMousePos = e.Location;
                        Render();
                    }
                    break;

                case DragMode.None:
                    if (lines.Count > 0 && !newLineArmed)
                    {
                        bool overHandle = selected != null
                            && (Distance(e.Location, selected.Start) <= GrabRadius
                             || Distance(e.Location, selected.End) <= GrabRadius);
                        Cursor = overHandle ? Cursors.Hand : Cursors.SizeAll;
                    }
                    break;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left) return;

            if (drag == DragMode.Drawing && selected != null
                && Distance(selected.Start, selected.End) < MinLineLength)
            {
                // Plain click without dragging: nothing to keep.
                lines.Remove(selected);
                selected = lines.Count > 0 ? lines[^1] : null;
            }

            drag = DragMode.None;
            UpdateIdleCursor();
            Render();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.KeyCode)
            {
                case Keys.Escape:
                    if (newLineArmed)
                    {
                        newLineArmed = false; // cancel the armed extra line
                    }
                    else if (selected != null)
                    {
                        lines.Remove(selected);
                        selected = null;
                    }
                    else if (lines.Count == 0)
                    {
                        Close();
                        return;
                    }
                    UpdateIdleCursor();
                    Render();
                    break;

                case Keys.N:
                    if (!newLineArmed)
                    {
                        newLineArmed = true;
                        Cursor = Cursors.Cross;
                        Render();
                    }
                    break;

                case Keys.D1:
                    SetColor(Color.LimeGreen);
                    break;

                case Keys.D2:
                    SetColor(Color.Red);
                    break;

                case Keys.Oemplus:
                case Keys.Add:
                    ChangeWidth(+1);
                    break;

                case Keys.OemMinus:
                case Keys.Subtract:
                    ChangeWidth(-1);
                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                surface?.Dispose();
                labelFont.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Private Methods
        private void SetColor(Color color)
        {
            defaultColor = color;
            if (selected != null)
            {
                selected.Color = color;
                Render();
            }
        }

        private void ChangeWidth(int delta)
        {
            if (selected == null) return;

            int newWidth = Math.Clamp(selected.Width + delta, MinLineWidth, MaxLineWidth);
            if (newWidth == selected.Width) return;

            selected.Width = newWidth;
            defaultWidth = newWidth;
            Render();
        }

        private void UpdateIdleCursor()
        {
            Cursor = (lines.Count == 0 || newLineArmed) ? Cursors.Cross : Cursors.SizeAll;
        }

        private MeasureLine FindNearestLine(Point p)
        {
            MeasureLine best = lines[^1];
            double bestDist = double.MaxValue;
            foreach (var line in lines)
            {
                double d = DistanceToSegment(p, line.Start, line.End);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = line;
                }
            }
            return best;
        }
        #endregion

        #region Rendering
        private void Render()
        {
            if (!IsHandleCreated) return;

            surface ??= new Bitmap(Width, Height, PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(surface))
            {
                // While there is no line yet, one is being drawn, or N armed a new
                // one, the whole surface must capture the mouse so a drag can start
                // anywhere. Otherwise the background is fully transparent (alpha 0)
                // and only the lines/handles/labels intercept clicks.
                bool captureWholeScreen = lines.Count == 0 || newLineArmed || drag == DragMode.Drawing;
                g.Clear(captureWholeScreen ? HitOnlyColor : Color.Transparent);

                g.SmoothingMode = SmoothingMode.AntiAlias;

                foreach (var line in lines)
                {
                    if (line.Start == line.End) continue;
                    DrawLine(g, line, line == selected);
                }
            }

            NativeMethods.SetLayeredWindowBitmap(this, surface);
        }

        private void DrawLine(Graphics g, MeasureLine line, bool isSelected)
        {
            // Invisible fat stroke widening the clickable area of the line.
            using (var hitPen = new Pen(HitOnlyColor, GrabRadius * 2)
            { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawLine(hitPen, line.Start, line.End);
            }

            using (var pen = new Pen(line.Color, line.Width))
            {
                g.DrawLine(pen, line.Start, line.End);
            }

            if (isSelected)
            {
                DrawHandle(g, line.Start, line.Color);
                DrawHandle(g, line.End, line.Color);
            }

            DrawLengthLabel(g, line);
        }

        private void DrawHandle(Graphics g, Point p, Color color)
        {
            using (var hitBrush = new SolidBrush(HitOnlyColor))
            {
                g.FillEllipse(hitBrush, p.X - GrabRadius, p.Y - GrabRadius, GrabRadius * 2, GrabRadius * 2);
            }

            var rect = new Rectangle(p.X - HandleRadius, p.Y - HandleRadius, HandleRadius * 2, HandleRadius * 2);
            using (var fill = new SolidBrush(Color.White))
            {
                g.FillEllipse(fill, rect);
            }
            using (var border = new Pen(color, 2))
            {
                g.DrawEllipse(border, rect);
            }
        }

        private void DrawLengthLabel(Graphics g, MeasureLine line)
        {
            double length = Distance(line.Start, line.End);
            string text = $"{length:0} px";

            // Place the label beside the midpoint, offset perpendicular to the line.
            double dx = line.End.X - line.Start.X;
            double dy = line.End.Y - line.Start.Y;
            float ox = (float)(-dy / length) * LabelOffset;
            float oy = (float)(dx / length) * LabelOffset;
            var mid = new PointF((line.Start.X + line.End.X) / 2f + ox, (line.Start.Y + line.End.Y) / 2f + oy);

            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            SizeF textSize = g.MeasureString(text, labelFont);
            var box = new RectangleF(mid.X - textSize.Width / 2f - 4, mid.Y - textSize.Height / 2f - 2,
                textSize.Width + 8, textSize.Height + 4);

            using (var back = new SolidBrush(Color.FromArgb(210, 32, 32, 32)))
            {
                g.FillRectangle(back, box);
            }
            g.DrawString(text, labelFont, Brushes.White, box.X + 4, box.Y + 2);
        }
        #endregion

        #region Geometry Helpers
        private static double Distance(Point a, Point b)
        {
            double dx = a.X - b.X, dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static double DistanceToSegment(Point p, Point a, Point b)
        {
            double dx = b.X - a.X, dy = b.Y - a.Y;
            double lengthSquared = dx * dx + dy * dy;
            if (lengthSquared == 0) return Distance(p, a);

            double t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / lengthSquared;
            t = Math.Clamp(t, 0, 1);
            var closest = new PointF((float)(a.X + t * dx), (float)(a.Y + t * dy));
            double cdx = p.X - closest.X, cdy = p.Y - closest.Y;
            return Math.Sqrt(cdx * cdx + cdy * cdy);
        }
        #endregion
    }
}
