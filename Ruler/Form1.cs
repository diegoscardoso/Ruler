

namespace Ruler
{
    public partial class Form1 : Form
    {
        private bool isDragging = false;
        private Point dragStartPoint;

        private const int sizeChangeValue = 10;
        private const int sizeHeight = 27;

        public Form1()
        {
            InitializeComponent();
            RestoreFormPosition();
            RestoreSizeWidth();
            RestorPanelColor();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = new Point(e.X, e.Y);
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point p = PointToScreen(new Point(e.X, e.Y));
                Location = new Point(p.X - dragStartPoint.X, p.Y - dragStartPoint.Y);
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close(); // Close the form when Esc is pressed
            }

            if (e.KeyCode == Keys.D1)
            {
                this.linePanel.BackColor = Color.Green;
            }

            if (e.KeyCode == Keys.D2)
            {
                this.linePanel.BackColor = Color.Red;
            }

            if (e.KeyCode == Keys.Right)
            {
                var actualFormSize = this.Size;
                var actualPanelSize = this.linePanel.Size;

                this.Size = new Size(actualFormSize.Width + sizeChangeValue, actualFormSize.Height);
                this.linePanel.Size = new Size(actualPanelSize.Width + sizeChangeValue, actualPanelSize.Height);
            }

            if (e.KeyCode == Keys.Left)
            {
                var actualSize = this.Size;
                var actualPanelSize = this.linePanel.Size;

                this.Size = new Size(actualSize.Width - sizeChangeValue, actualSize.Height);
                this.linePanel.Size = new Size(actualPanelSize.Width - sizeChangeValue, actualPanelSize.Height);
            }
        }

        private void SavePanelColor(Color color)
        {
            string hexColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            Properties.Settings.Default.LineColor = hexColor;
            Properties.Settings.Default.Save();
        }
        private void RestorPanelColor()
        {
            string hexColor = Properties.Settings.Default.LineColor;
            try
            {
                // Convert hex string back to Color
                this.linePanel.BackColor = ColorTranslator.FromHtml(hexColor);
            }
            catch
            {
                // Fallback to a default color if parsing fails
                this.linePanel.BackColor = Color.Yellow;
            }
        }

        private void SaveFormPosition()
        {
            // Save current position to settings
            Properties.Settings.Default.FormPosX = this.Location.X;
            Properties.Settings.Default.FormPosY = this.Location.Y;
            Properties.Settings.Default.Save(); // Persist settings
        }

        public void RestoreFormPosition()
        {
            // Load saved position from settings
            int x = Properties.Settings.Default.FormPosX;
            int y = Properties.Settings.Default.FormPosY;

            // Validate position to ensure it's within screen bounds
            if (IsValidPosition(x, y))
            {
                this.Location = new Point(x, y);
            }
            else
            {
                // Fallback to center screen if saved position is invalid
                this.StartPosition = FormStartPosition.CenterScreen;
            }
        }

        private bool IsValidPosition(int x, int y)
        {
            // Check if position is within any screen's bounds
            foreach (var screen in Screen.AllScreens)
            {
                var bounds = screen.WorkingArea;
                if (x >= bounds.Left && x < bounds.Right && y >= bounds.Top && y < bounds.Bottom)
                {
                    return true; // Position is valid
                }
            }
            return false; // Position is outside all screens
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.SaveFormPosition();
            this.SaveSizeWidth();
            this.SavePanelColor(this.linePanel.BackColor);
        }

        private void SaveSizeWidth()
        {
            var currentWidth = this.Size.Width;
            Properties.Settings.Default.SizeWidth = currentWidth;
            Properties.Settings.Default.Save();
        }
        private void RestoreSizeWidth()
        {
            try
            {
                var lastWidth = Properties.Settings.Default.SizeWidth;
                this.Size = new Size(lastWidth, sizeHeight);
            }
            catch
            {
                this.Size = new Size(759, sizeHeight);
            }
        }
    }
}
