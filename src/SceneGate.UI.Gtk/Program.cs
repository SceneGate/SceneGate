using System;
using Eto.Forms;
using SceneGate.UI.Views;

namespace SceneGate.UI.Gtk
{
    public class MainClass
    {
        [STAThread]
        public static void Main(string[] args)
        {
            new Application(Eto.Platforms.Gtk).Run(new MainWindow());
        }
    }
}
