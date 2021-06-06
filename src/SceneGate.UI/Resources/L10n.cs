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
using System.Diagnostics.CodeAnalysis;
using Yarhl.FileFormat;
using Yarhl.IO;
using Yarhl.Media.Text;

namespace SceneGate.UI.Resources
{
    [SuppressMessage("", "S101", Justification = "Short name to avoid long localization lines")]
    public static class L10n
    {
        private const string Language = "en";
        private static readonly Po Translation = LoadPo(Language);

        public static string Get(string original, string context = null)
        {
            if (Translation == null) {
                return original;
            }

            PoEntry entry = Translation.FindEntry(original, context);
            if (entry == null) {
                return original;
            }

            if (string.IsNullOrEmpty(entry.Translated)) {
                return original;
            }

            return entry.Translated;
        }

        private static Po LoadPo(string language)
        {
            string resourceName = $"{ResourcesName.Prefix}.{language}.po";

            var assembly = typeof(L10n).Assembly;
            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) {
                return null;
            }

            try {
                using var binaryPo = new BinaryFormat(DataStreamFactory.FromStream(stream));
                return (Po)ConvertFormat.With<Binary2Po>(binaryPo);
            } catch {
                return null;
            }
        }
    }
}
