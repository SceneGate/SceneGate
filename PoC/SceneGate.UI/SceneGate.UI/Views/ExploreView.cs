
namespace SceneGate.UI.Views
{
    using Eto.Drawing;
    using Eto.Forms;

    public class ExploreView : Panel
    {
        public ExploreView()
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
