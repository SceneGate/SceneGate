using System;
using Eto.Forms;
using SceneGate.UI.Main;

namespace SceneGate.UI.Mac
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            new Application(Eto.Platforms.Mac64).Run(new MainWindow());
        }
    }
}
