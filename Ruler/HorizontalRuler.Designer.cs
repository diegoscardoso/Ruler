namespace Ruler
{
    partial class HorizontalRuler
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            linePanel = new Panel();
            SuspendLayout();
            // 
            // linePanel
            // 
            linePanel.BackColor = Color.Yellow;
            linePanel.Location = new Point(-1, 2);
            linePanel.Name = "linePanel";
            linePanel.Size = new Size(759, 1);
            linePanel.TabIndex = 0;
            //
            // HorizontalRuler
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Khaki;
            ClientSize = new Size(759, 11);
            ControlBox = false;
            Controls.Add(linePanel);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "HorizontalRuler";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            TransparencyKey = Color.Khaki;
            FormClosing += HorizontalRuler_FormClosing;
            KeyDown += HorizontalRuler_KeyDown;
            MouseDown += HorizontalRuler_MouseDown;
            MouseMove += HorizontalRuler_MouseMove;
            MouseUp += HorizontalRuler_MouseUp;
            ResumeLayout(false);

        }

        #endregion

        private Panel linePanel;
    }
}
