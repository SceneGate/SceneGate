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
using System.Linq;
using Yarhl;
using Yarhl.FileSystem;
using Yarhl.IO;

namespace SceneGate.UI.Projects
{
    public class SimpleTransformOperation : INodeOperation
    {
        public string ConverterName { get; set; }

        public void Run(Node node)
        {
            // In case some converter doesn't do it...
            if (node.Format is IBinary) {
                node.Stream.Position = 0;
            }

            var converter = PluginManager.Instance.GetConverters()
                .FirstOrDefault(x => x.Metadata.Name.Equals(ConverterName, StringComparison.InvariantCultureIgnoreCase));
            if (converter is null) {
                throw new InvalidOperationException($"Cannot find converter: '{ConverterName}'");
            }

            node.TransformWith(converter.Metadata.Type);
        }
    }
}
