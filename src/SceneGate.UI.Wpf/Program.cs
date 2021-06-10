using System;
using Eto.Drawing;
using Eto.Forms;
using Eto.Wpf.Forms.Controls;
using SceneGate.UI.Main;

namespace SceneGate.UI.Wpf
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Eto.Style.Add<TreeGridViewHandler>(
                "analyze-tree",
                handler => handler.Font = new Font("Ubuntu Nerd Font", 10));

            new Application(Eto.Platforms.Wpf).Run(new MainWindow());
        }
    }
}
