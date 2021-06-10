using System;
using System.IO;
using Eto.Drawing;
using Eto.Forms;
using Eto.GtkSharp.Forms.Controls;
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

            Eto.Style.Add<TreeGridViewHandler>(
                "analyze-tree",
                handler => handler.Font = new Font("Ubuntu Nerd Font", 10));

            new Application(Eto.Platforms.Gtk).Run(new MainWindow());
        }
    }
}
