namespace Ruler
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Per-monitor DPI awareness so mouse coordinates and the rendered
            // bitmap are in real physical pixels (the ruler measures pixels).
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new RulerOverlay());
        }
    }
}
