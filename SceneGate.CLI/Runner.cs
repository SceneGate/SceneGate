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
    using System.Text;
    using Mono.Terminal;
    using Commands;

    public class Runner
    {
        readonly LineEditor editor;

        public Runner()
        {
            editor = new LineEditor("SceneGate", 1000);
            Environment = new VirtualEnvironment(editor);
        }

        public VirtualEnvironment Environment { get; private set; }

        public void Run()
        {
            ICommand[] commands = { new Exit() };

            bool lastResult = true;
            while (!Environment.RequestStop) {
                string prompt = FormatPrompt(lastResult);
                string command = editor.Edit(prompt, "");
                if (string.IsNullOrEmpty(command)) {
                    continue;
                }

                string[] args = command.Split(' ');
                string name = args[0].ToLower();
                string[] operands = args.Skip(1).ToArray();

                bool commandFound = false;
                foreach (var cmd in commands) {
                    if (!cmd.Name.Split(',').Contains(name))
                        continue;

                    lastResult = cmd.Run(Environment);
                    commandFound = true;
                }

                if (!commandFound) {
                    Console.WriteLine("Command not found");
                    lastResult = false;
                }
            }
        }

        string FormatPrompt(bool lastResult)
        {
            const string RED = "\x1B[31m";
            const string GREEN = "\x1B[32m";
            const string YELLOW = "\x1B[33m";
            const string END  = "\x1B[0m";

            StringBuilder prompt = new StringBuilder(Environment.Prompt);
            prompt.Replace("{r}", lastResult ? GREEN + "✔" + END : RED + "✘" + END);
            prompt.Replace("{p}", YELLOW + Environment.CurrentNode.Path + END);
            prompt.Replace("{d}", DateTime.Now.ToShortTimeString());

            return prompt.ToString();
        }
    }
}
