namespace Ruler
{
    public partial class HorizontalRuler : Form
    {
        #region Private Fields
        private bool isDragging = false;
        private Point dragStartPoint;

        private const int SizeChangeValue = 10;
        private const int SizeHeight = 10;
        private const int DefaultFormWidth = 759;
        #endregion

        #region Constructors
        public HorizontalRuler()
        {
            InitializeComponent();
            RestoreFormPosition();
            RestoreSizeWidth();
            RestorePanelColor();
        }
        #endregion

        #region Control Events
        private void HorizontalRuler_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = new Point(e.X, e.Y);
            }
        }

        private void HorizontalRuler_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point p = PointToScreen(new Point(e.X, e.Y));
                Location = new Point(p.X - dragStartPoint.X, p.Y - dragStartPoint.Y);
            }
        }

        private void HorizontalRuler_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void HorizontalRuler_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    Close(); // Close the form when Esc is pressed
                    break;
                case Keys.D1:
                    linePanel.BackColor = Color.LimeGreen;
                    break;
                case Keys.D2:
                    linePanel.BackColor = Color.Red;
                    break;
                case Keys.Right:
                {
                    var newFormWidth = this.Size.Width + SizeChangeValue;
                    var newPanelWidth = this.linePanel.Size.Width + SizeChangeValue;

                    this.Size = new Size(newFormWidth, this.Size.Height);
                    this.linePanel.Size = new Size(newPanelWidth, this.linePanel.Size.Height);
                    break;
                }
                case Keys.Left:
                {
                    // Prevent width from becoming zero or negative
                    var newFormWidth = Math.Max(1, this.Size.Width - SizeChangeValue);
                    var newPanelWidth = Math.Max(1, this.linePanel.Size.Width - SizeChangeValue);

                    this.Size = new Size(newFormWidth, this.Size.Height);
                    this.linePanel.Size = new Size(newPanelWidth, this.linePanel.Size.Height);
                    break;
                }
                case Keys.N:
                    Program.CreateNewRuler();
                    break;
            }
        }

        private void HorizontalRuler_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.SaveFormPosition();
            this.SaveSizeWidth();
            this.SavePanelColor(this.linePanel.BackColor);

            Properties.Settings.Default.Save();
        }
        #endregion

        #region Private Methods
        private void SavePanelColor(Color color)
        {
            string hexColor = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            Properties.Settings.Default.LineColor = hexColor;
        }

        private void RestorePanelColor()
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
            // Save current position to settings. Only save when window is in normal state
            if (this.WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.FormPosX = this.Location.X;
                Properties.Settings.Default.FormPosY = this.Location.Y;
            }
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

        private void SaveSizeWidth()
        {
            var currentWidth = this.Size.Width;
            Properties.Settings.Default.SizeWidth = currentWidth;
        }

        private void RestoreSizeWidth()
        {
            try
            {
                var lastWidth = Properties.Settings.Default.SizeWidth;
                if (lastWidth < 1)
                {
                    lastWidth = DefaultFormWidth; // Use constant instead of magic number
                }

                this.Size = new Size(lastWidth, SizeHeight);
            }
            catch
            {
                this.Size = new Size(DefaultFormWidth, SizeHeight); // Use constant instead of magic number
            }
        }
        #endregion
    }
}
