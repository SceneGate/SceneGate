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
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using SceneGate.UI.Projects;
using SceneGate.UI.Resources;

namespace SceneGate.UI.Main
{
    public sealed class MainViewModel : ObservableObject
    {
        private ViewKind viewKind;
        private AnalyzeViewModel analyzeViewModel;
        private bool openedProject;

        public MainViewModel()
        {
            OpenAnalyzeCommand = new RelayCommand(OpenAnalyze);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            QuitCommand = new RelayCommand(Quit);
            AboutCommand = new RelayCommand(OpenAboutDialog);
            NewProjectCommand = new RelayCommand(NewProject, () => !IsProjectOpened);
            OpenProjectCommand = new RelayCommand(OpenProject, () => !IsProjectOpened);
            ToggleActionPanelCommand = new RelayCommand(ToggleActionPanel);
            ViewKind = ViewKind.Analyze;
        }

        public ViewKind ViewKind {
            get => viewKind;
            private set => SetProperty(ref viewKind, value);
        }

        public ICommand OpenAnalyzeCommand { get; }

        public ICommand OpenSettingsCommand { get; }

        public ICommand QuitCommand { get; }

        public ICommand AboutCommand { get; }

        public RelayCommand NewProjectCommand { get; }

        public RelayCommand OpenProjectCommand { get; }

        public ICommand ToggleActionPanelCommand { get; }

        public bool IsProjectOpened {
            get => openedProject;
            set {
                SetProperty(ref openedProject, value);
                NewProjectCommand.NotifyCanExecuteChanged();
                OpenProjectCommand.NotifyCanExecuteChanged();
            }
        }

        public bool IsActionPanelVisible {
            get => analyzeViewModel?.IsActionPanelVisible ?? false;
            set {
                if (analyzeViewModel != null) {
                    analyzeViewModel.IsActionPanelVisible = value;
                }

                OnPropertyChanged(nameof(IsActionPanelVisible));
            }
        }

        public void AttachAnalyzeViewModel(AnalyzeViewModel viewModel)
        {
            analyzeViewModel = viewModel;
        }

        private void OpenAnalyze() => ViewKind = ViewKind.Analyze;

        private void OpenSettings() => ViewKind = ViewKind.Settings;

        private void Quit() => Eto.Forms.Application.Instance.Quit();

        private void ToggleActionPanel() => IsActionPanelVisible = !IsActionPanelVisible;

        private void OpenAboutDialog()
        {
            var about = new AboutView();
            about.ShowDialog(Eto.Forms.Application.Instance.MainForm);
        }

        private void NewProject()
        {
            ProjectManager.Instance.CurrentProject = new Project();
        }

        private void OpenProject()
        {
            var dialog = new Eto.Forms.OpenFileDialog {
                CheckFileExists = true,
                MultiSelect = false,
                Title = L10n.Get("Open project file"),
            };
            dialog.Filters.Add(new Eto.Forms.FileFilter("SceneGate project", "json"));
            if (dialog.ShowDialog(Eto.Forms.Application.Instance.MainForm) != Eto.Forms.DialogResult.Ok) {
                return;
            }

            string content = File.ReadAllText(dialog.FileName, Encoding.UTF8);
            var project = JsonSerializer.Deserialize<Project>(content);
            ProjectManager.Instance.CurrentProject = project;
        }
    }
}
