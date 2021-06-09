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
using System.Linq;
using System.Text;
using Eto.Forms;
using Yarhl.FileSystem;
using Yarhl.IO;
using Yarhl.Media.Text;

namespace SceneGate.UI.Main
{
    public class TreeGridNode : TreeGridItem
    {
        private static readonly Encoding Ascii = Encoding.GetEncoding(
            "ASCII",
            EncoderFallback.ExceptionFallback,
            DecoderFallback.ExceptionFallback);

        public TreeGridNode(Node node)
            : base()
        {
            Node = node;

            var children = node.Children.OrderBy(c => !c.IsContainer);
            foreach (var childNode in children) {
                var child = new TreeGridNode(childNode);
                Children.Add(child);
            }
        }

        public Node Node { get; }

        public string QualifiedName {
            get {
                string name = $"{Icon} {Node.Name}";
                if (Node.Format != null && !Node.IsContainer) {
                    name += $" [{FormatName}]";
                }

                return name;
            }
        }

        public string FormatName {
            get {
                if (Node.Format == null && Node.IsContainer) {
                    return "Container";
                }

                if (Node.Format is IBinary) {
                    try {
                        Node.Stream.Position = 0;
                        string stamp = new DataReader(Node.Stream).ReadString(4, Ascii);
                        return $"%{stamp}%";
                    } catch {
                        return "Binary";
                    }
                }

                return Node.Format.GetType().Name;
            }
        }

        public string Icon {
            get => Node.Format switch {
                NodeContainerFormat => "\ue2c7",
                null => "\ue2c7",
                Po => "\uef42",
                IBinary => "\ue24d",
                _ => "\ue583",
            };
        }

        public void Add(Node node)
        {
            var child = new TreeGridNode(node);
            Children.Add(child);
            Node.Add(node);
        }

        public void UpdateChildren()
        {
            Children.Clear();

            var children = Node.Children.OrderBy(c => !c.IsContainer);
            foreach (var childNode in children) {
                var child = new TreeGridNode(childNode);
                Children.Add(child);
            }
        }
    }
}
