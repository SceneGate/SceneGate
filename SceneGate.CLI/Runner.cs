//
// CliRunner.cs
//
// Author:
//     Benito Palacios Sanchez <benito356@gmail.com>
//
// Copyright (c) 2017 Benito Palacios Sanchez
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
namespace SceneGate.Cli
{
    using System;
    using System.Linq;
    using Mono.Terminal;

    public class Runner
    {
        public Runner()
        {
            Environment = new VirtualEnvironment();
        }

        public VirtualEnvironment Environment { get; private set; }

        public void Run()
        {
            LineEditor editor = new LineEditor("SceneGate", 1000);

            bool stop = false;
            while (!stop) {
                string command = editor.Edit(Environment.CurrentNode.Path + " $ ", "");
                if (string.IsNullOrEmpty(command)) {
                    stop = true;
                    continue;
                }

                string[] args = command.Split(' ');
                string id = args[0].ToLower();
                args = args.Skip(1).ToArray();

                Console.WriteLine("Running {0} with {1}", id, string.Join(",", args));
            }

            editor.SaveHistory();
        }
    }
}
