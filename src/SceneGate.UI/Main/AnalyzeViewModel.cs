// Copyright (c) 2021 SceneGate

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using SceneGate.UI.Extensions;
using SceneGate.UI.Resources;
using Yarhl;
using Yarhl.FileFormat;
using Yarhl.FileSystem;
using Yarhl.IO;

namespace SceneGate.UI.Main
{
    public sealed class AnalyzeViewModel : ObservableObject
    {
        private readonly ConverterMetadata[] converters;
        private string converterFilter;
        private TreeGridNode selectedNode;
        private int selectedConverterIndex;
        private bool isActionPanelVisible;
        private Eto.Forms.Control viewer;

        public AnalyzeViewModel()
        {
            converters = PluginManager.Instance.GetConverters().Select(x => x.Metadata).ToArray();
            CompatibleConverters = new ObservableCollection<ConverterMetadata>();
            converterFilter = string.Empty;
            UpdateCompatibleConverters();
            SelectedConverterIndex = -1;

            IsActionPanelVisible = true;
            AddFileCommand = new RelayCommand(AddFile);
            AddFolderCommand = new RelayCommand(AddFolder);
            SaveNodeCommand = new RelayCommand(SaveNode, () => CanSaveNode);
            ConvertNodeCommand = new AsyncRelayCommand(ConvertNodeAsync, () => CanConvertNode);
            ShowViewerCommand = new RelayCommand(ShowViewer, () => CanShowViewer);

            RootNode = new TreeGridNode(NodeFactory.CreateContainer("root"));
        }

        public event EventHandler<TreeGridNode> OnNodeUpdate;

        public ObservableCollection<ConverterMetadata> CompatibleConverters { get; set; }

        public ConverterMetadata SelectedConverter =>
            (SelectedConverterIndex >= 0) ? CompatibleConverters[SelectedConverterIndex] : null;

        public int SelectedConverterIndex {
            get => selectedConverterIndex;
            set {
                SetProperty(ref selectedConverterIndex, value);
                ConvertNodeCommand?.NotifyCanExecuteChanged();
            }
        }

        public string ConverterFilter {
            get => converterFilter;
            set {
                SetProperty(ref converterFilter, value);
                UpdateCompatibleConverters();
                ConvertNodeCommand?.NotifyCanExecuteChanged();
            }
        }

        public TreeGridNode SelectedNode {
            get => selectedNode;
            set {
                SetProperty(ref selectedNode, value);
                UpdateCompatibleConverters();
                SaveNodeCommand?.NotifyCanExecuteChanged();
                ConvertNodeCommand?.NotifyCanExecuteChanged();
                ShowViewerCommand?.NotifyCanExecuteChanged();
            }
        }

        public TreeGridNode RootNode { get; }

        public bool IsActionPanelVisible {
            get => isActionPanelVisible;
            set => SetProperty(ref isActionPanelVisible, value);
        }

        public Eto.Forms.Control Viewer {
            get => viewer;
            set => SetProperty(ref viewer, value);
        }

        public ICommand AddFileCommand { get; }

        public ICommand AddFolderCommand { get; }

        public RelayCommand SaveNodeCommand { get; }

        public bool CanSaveNode { get => SelectedNode?.Node.Format is IBinary; }

        public AsyncRelayCommand ConvertNodeCommand { get; }

        public bool CanConvertNode { get => SelectedNode is not null && SelectedConverter is not null; }

        public RelayCommand ShowViewerCommand { get; }

        public bool CanShowViewer { get => SelectedNode is not null; }

        private void UpdateCompatibleConverters()
        {
            CompatibleConverters.Clear();

            var node = SelectedNode;
            IEnumerable<ConverterMetadata> compatible = (node is null)
                ? converters
                : converters.Where(c => c.CanConvert(node.Node.Format.GetType()));

            if (!string.IsNullOrWhiteSpace(ConverterFilter)) {
                compatible = compatible.Where(c => c.Name.Contains(ConverterFilter, StringComparison.OrdinalIgnoreCase));
            }

            CompatibleConverters.AddRange(compatible);
        }

        private void AddFile()
        {
            var dialog = new Eto.Forms.OpenFileDialog {
                CheckFileExists = true,
                MultiSelect = true,
                Title = L10n.Get("Add external files"),
            };
            if (dialog.ShowDialog(Eto.Forms.Application.Instance.MainForm) != Eto.Forms.DialogResult.Ok) {
                return;
            }

            foreach (var file in dialog.Filenames) {
                Node node = NodeFactory.FromFile(file);
                AddNodeToRoot(node);
            }
        }

        private void AddFolder()
        {
            var dialog = new Eto.Forms.SelectFolderDialog {
                Title = L10n.Get("Add external folders"),
            };
            if (dialog.ShowDialog(Eto.Forms.Application.Instance.MainForm) != Eto.Forms.DialogResult.Ok) {
                return;
            }

            string name = Path.GetFileName(dialog.Directory);
            Node node = NodeFactory.FromDirectory(dialog.Directory, "*", name, subDirectories: true);
            AddNodeToRoot(node);
        }

        private void AddNodeToRoot(Node node)
        {
            RootNode.Add(node);
            OnNodeUpdate?.Invoke(this, RootNode);
        }

        private void SaveNode()
        {
            var dialog = new Eto.Forms.SaveFileDialog {
                CheckFileExists = false,
                Title = L10n.Get("Save to file"),
                FileName = SelectedNode.Node.Name,
            };
            if (dialog.ShowDialog(Eto.Forms.Application.Instance.MainForm) != Eto.Forms.DialogResult.Ok) {
                return;
            }

            SelectedNode.Node.Stream.WriteTo(dialog.FileName);
        }

        private async Task ConvertNodeAsync()
        {
            var node = SelectedNode;

            try {
                // In case some converter doesn't do it...
                if (node.Node.Format is IBinary) {
                    node.Node.Stream.Position = 0;
                }

                var converterType = SelectedConverter.Type;
                await Task.Run(() => node.Node.TransformWith(converterType)).ConfigureAwait(true);
            } catch (Exception ex) {
                await Eto.Forms.Application.Instance.InvokeAsync(
                    () => Eto.Forms.MessageBox.Show(ex.ToString())).ConfigureAwait(true);
            }

            node.UpdateChildren();
            OnNodeUpdate?.Invoke(this, node);

            SelectedNode = node;
        }

        private void ShowViewer()
        {
            var formatView = UiPluginManager.GetCompatibleView(SelectedNode?.Node.Format).FirstOrDefault();
            if (formatView is not null) {
                formatView.ViewModel.Show(SelectedNode?.Node.Format);
                Viewer = formatView;
            }
        }
    }
}
