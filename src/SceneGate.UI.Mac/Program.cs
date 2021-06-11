using System;
using Eto.Drawing;
using Eto.Forms;
using Eto.Mac.Forms.Controls;
using SceneGate.UI.Main;

namespace SceneGate.UI.Mac
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Eto.Style.Add<TreeGridViewHandler>(
                "analyze-tree",
                handler => handler.Font = new Font("Ubuntu Nerd Font", 10));

            new Application(Eto.Platforms.Mac64).Run(new MainWindow());
        }
    }
}
