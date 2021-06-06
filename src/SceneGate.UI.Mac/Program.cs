using System;
using Eto.Forms;
using SceneGate.UI.Views;

namespace SceneGate.UI.Mac
{
    public class MainClass
    {
        [STAThread]
        public static void Main(string[] args)
        {
            new Application(Eto.Platforms.Mac64).Run(new MainWindow());
        }
    }
}
