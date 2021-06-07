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
using System.Diagnostics.CodeAnalysis;
using Eto.Drawing;
using Eto.Forms;
using SceneGate.UI.Resources;

namespace SceneGate.UI.Views
{
    public sealed class AboutView : AboutDialog
    {
        [SuppressMessage("", "S1075", Justification = "Project URL is ok to hard-code")]
        public AboutView()
        {
            Logo = Bitmap.FromResource(ResourcesName.Icon);
            WebsiteLabel = L10n.Get("SceneGate website");
            Website = new Uri("https://scenegate.github.io/SceneGate/");
            Developers = new[] { L10n.Get("SceneGate team and contributors") };
            License = L10n.Get("MIT License\nhttps://opensource.org/licenses/MIT");
            ProgramName = "SceneGate";
            ProgramDescription = L10n.Get("Tool for reverse engineering, file format analysis, modding and localization.");
        }
    }
}
