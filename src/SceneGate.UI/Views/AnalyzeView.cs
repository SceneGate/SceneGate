
namespace SceneGate.UI.Views
{
    using Eto.Drawing;
    using Eto.Forms;
    using Lemon.Containers;
    using System;
    using System.Linq;
    using Yarhl;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    public class AnalyzeView : Panel
    {
        TreeGridView tree;
        Splitter contentLayout;
        Panel contentPanel;
        ListBox converterList;
        ConverterMetadata[] converters;

        public AnalyzeView()
        {
            CreateControls();
        }

        public void ToggleActionPanel()
        {
            contentLayout.Panel2.Visible = !contentLayout.Panel2.Visible;
        }

        void CreateControls()
        {
            contentPanel = new Panel();

            var tabbedPanel = new TabControl();
            tabbedPanel.Pages.Add(new TabPage(CreateConvertersPanel()) { Text = "Converters" });
            tabbedPanel.Pages.Add(new TabPage { Text = "Console" });

            contentLayout = new Splitter
            {
                Orientation = Orientation.Vertical,
                FixedPanel = SplitterFixedPanel.Panel2,
                Panel1 = contentPanel,
                //Panel1MinimumSize = 50,
                Panel2 = tabbedPanel,
                Panel2MinimumSize = 50,
            };

            var splitter = new Splitter
            {
                Orientation = Orientation.Horizontal,
                FixedPanel = SplitterFixedPanel.Panel1,
                Panel1MinimumSize = 150,
                Panel1 = CreateLeftPanel(),
                Panel2 = contentLayout,
            };

            Content = splitter;
        }

        Panel CreateConvertersPanel()
        {
            converters = PluginManager.Instance.GetConverters().Select(x => x.Metadata).ToArray();
            converterList = new ListBox();
            converterList.Items.AddRange(converters.Select(x => new ListItem { Key = x.Type.FullName, Text = x.Name }));

            var stack = new DynamicLayout();
            stack.BeginHorizontal();
            stack.AddRow(converterList);
            stack.EndHorizontal();

            stack.Invalidate();

            return stack;
        }

        Panel CreateLeftPanel()
        {
            var addBtn = new Button(AddRootNode)
            {
                ImagePosition = ButtonImagePosition.Overlay,
                Image = Icon.FromResource("SceneGate.UI.Resources.outline_add_circle_white_48dp.png").WithSize(16, 16),
                Size = new Size(16, 16),
            };

            tree = new TreeGridView();
            tree.ShowHeader = false;
            tree.Border = BorderType.Line;
            tree.Columns.Add(
                new GridColumn
                {
                    DataCell = new TextBoxCell(1),
                    HeaderText = "Name",
                    AutoSize = true,
                    Resizable = true,
                    Editable = false
                });
            tree.DataStore = new TreeGridItem("root");

            if (Platform.Supports<ContextMenu>()) {
                var menu = new ContextMenu();
                var item = new ButtonMenuItem { Text = "Export to file" };
                item.Click += delegate
                {
                    if (tree.SelectedItems.Any())
                    {
                        var selected = tree.SelectedItem as TreeGridItem;
                        var node = selected.GetValue(0) as Node;
                        SaveFileDialog dialog = new SaveFileDialog();
                        if (dialog.ShowDialog(ParentWindow) == DialogResult.Ok)
                        {
                            node.Stream.WriteTo(dialog.FileName);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Click, no item selected");
                    }
                };
                menu.Items.Add(item);

                var item2 = new ButtonMenuItem { Text = "Convert" };
                item2.Click += delegate {
                    if (tree.SelectedItems.Any() && converterList.SelectedIndex != -1) {
                        var selected = tree.SelectedItem as TreeGridItem;
                        var node = selected.GetValue(0) as Node;

                        try {
                            node.TransformWith(converters[converterList.SelectedIndex].Type);
                        } catch (Exception ex) {
                            MessageBox.Show($"Failed to convert:\n{ex}");
                        }

                        AppendNodeToTree(node);
                    }
                };
                menu.Items.Add(item2);

                var item3 = new ButtonMenuItem { Text = "View as text" };
                item3.Click += delegate {
                    if (tree.SelectedItems.Any()) {
                        var selected = tree.SelectedItem as TreeGridItem;
                        var node = selected.GetValue(0) as Node;

                        var reader = new TextDataReader(node.Stream);
                        node.Stream.Position = 0;
                        var textArea = new TextArea { ReadOnly = true };
                        contentPanel.Content = textArea;
                        textArea.Text = reader.ReadToEnd();
                        textArea.Invalidate();
                    }
                };
                menu.Items.Add(item3);

                tree.ContextMenu = menu;
            }

            var headerLayout = new StackLayout {
                Orientation = Orientation.Horizontal,
            };
            headerLayout.Items.Add(addBtn);
            headerLayout.Items.Add(new StackLayoutItem(null, expand: true));
            headerLayout.Padding = new Padding(5);

            var stack = new DynamicLayout();
            stack.BeginHorizontal();
            stack.AddRow(headerLayout);
            stack.AddRow(tree);
            stack.EndHorizontal();

            return stack;
        }

        void AddRootNode(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog(ParentWindow) == DialogResult.Ok)
            {
                Node n = NodeFactory.FromFile(dialog.FileName);
                if (n.Name.EndsWith(".3ds"))
                {
                    ContainerManager.Unpack3DSNode(n);
                }
                AppendNodeToTree(n);
            }
        }

        void AppendNodeToTree(Node node)
        {
            var item = tree.SelectedItem as TreeGridItem;
            var parent = (item?.Parent ?? (ITreeGridItem)tree.DataStore) as TreeGridItem;
            AppendNode(parent, node);

            if (item != null) {
                parent.Children.Remove(item);
                tree.ReloadItem(parent);
            } else {
                tree.ReloadData();
            }
        }

        void AppendNode(TreeGridItem item, Node node)
        {
            var current = CreateTreeItem(node);
            item.Children.Add(current);
            foreach (var child in node.Children)
                AppendNode(current, child);
        }

        TreeGridItem CreateTreeItem(Node node)
        {
            string name = node.Name;
            if (node.Format != null & !node.IsContainer) {
                name += $" [{node.Format.GetType().Name}]";
            }

            return new TreeGridItem(node, name);
        }
    }
}
