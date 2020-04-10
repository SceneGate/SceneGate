namespace SceneGate.UI.Desktop
{
    using System;
    using Eto.Forms;
    using SceneGate.UI.Views;

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            new Application(Eto.Platform.Detect).Run(new MainWindow());
        }
    }
}
