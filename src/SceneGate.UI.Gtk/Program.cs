using System;
using Eto.Forms;
using SceneGate.UI.Main;

namespace SceneGate.UI.Gtk
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            new Application(Eto.Platforms.Gtk).Run(new MainWindow());
        }
    }
}
