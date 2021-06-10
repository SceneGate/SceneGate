using System;
using System.IO;
using Eto.Forms;
using SceneGate.UI.Main;

namespace SceneGate.UI.Gtk
{
    public static class Program
    {
        public static void Main()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                string gtkLibs = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");

                string path = Environment.GetEnvironmentVariable("Path");
                Environment.SetEnvironmentVariable("Path", $"{gtkLibs};{path}");
            }

            new Application(Eto.Platforms.Gtk).Run(new MainWindow());
        }
    }
}
