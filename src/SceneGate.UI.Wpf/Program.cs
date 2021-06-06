using System;
using Eto.Forms;
using SceneGate.UI.Views;

namespace SceneGate.UI.Wpf
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            new Application(Eto.Platforms.Wpf).Run(new MainWindow());
        }
    }
}
