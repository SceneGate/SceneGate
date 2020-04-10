
namespace SceneGate.UI.Views
{
    using Eto.Drawing;
    using Eto.Forms;

    public class AutomatizeView : Panel
    {
        public AutomatizeView()
        {
            CreateControls();
        }

        void CreateControls()
        {
            Content = new Label
            {
                Text = "Not implemented yet!",
                Font = Fonts.Monospace(60)
            };
        }
    }
}
