namespace Ruler
{
    internal static class Program
    {
        private static RulerApplicationContext? appContext;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            appContext = new RulerApplicationContext();
            Application.Run(appContext);
        }

        public static void CreateNewRuler()
        {
            appContext?.CreateNewRuler();
        }
    }

    internal class RulerApplicationContext : ApplicationContext
    {
        private int rulerCount = 0;

        public RulerApplicationContext()
        {
            CreateNewRuler();
        }

        public void CreateNewRuler()
        {
            var ruler = new HorizontalRuler();
            rulerCount++;
            ruler.FormClosed += (s, e) =>
            {
                rulerCount--;
                if (rulerCount == 0)
                {
                    ExitThread();
                }
            };
            ruler.Show();
        }
    }
}