# Contributing to the project

Thanks for taking the time to contribute! :sparkles:

In this document you will find all the information you need to make sure the
project continues to be the high-quality product we want to be!

## Reporting issues

### Issues

When reporting a problem, be as specific as possible. Ideally, you should
provide an small snippet of code that reproduces the issue. Try to provide also
the following information:

- OS: Linux / Windows / Mac OS
- Version of the product
- Stacktrace if any
- What's happening and what you expect to happen

### Features

If you want to ask for a new feature, first make sure it hasn't been reported
yet by using the search box in the issue tab. Make sure that the feature aligns
with the direction of the project.

## Pull Request

Before starting a pull request, create an issue requesting the feature you would
like to see and implement. If you are fixing a bug, create also an issue to be
able to track the problem. State that you would like to work on that. The team
will reply to the issue as soon as possible, discussing the proposal if needed.
This guarantee that later on the Pull Request we don't reject the proposal
without having a discussion first and we don't waste time.

In general, the process to create a pull request is:

1. Create an issue describing the bug or feature and state you would like to
   work on that.
2. The team will cheer you and/or discuss with you the issue.
3. Fork the project.
4. Clone your forked project and create a git branch.
5. Make the necessary code changes in as many commits as you want. The commit
   message should follow this convention:

```plain
:emoji: Short description #IssueID

Long description if needed.
```

6. Create a pull request. After reviewing your changes and making any new
   commits if needed, the team will approve and merge it.

For a complete list of emoji description see
[this repository](https://github.com/slashsBin/styleguide-git-commit-message#suggested-emojis).

## Code Guidelines

We follow the following standard guidelines with custom changes:

- [Mono Code Guidelines](https://raw.githubusercontent.com/mono/website/gh-pages/community/contributing/coding-guidelines.md).
- [Microsoft Framework Design Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/)
- [Microsoft C# Coding Convetions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions).

As the
[mono team says](https://www.mono-project.com/community/contributing/coding-guidelines/#performance-and-readability):

- It is more important to be correct than to be fast.
- It is more important to be maintainable than to be fast.
- Fast code that is difficult to maintain is likely going to be looked down
  upon.

And don't miss
[The Zen of Python](https://www.python.org/dev/peps/pep-0020/#id3):

```plain
Beautiful is better than ugly.
Explicit is better than implicit.
Simple is better than complex.
Complex is better than complicated.
Flat is better than nested.
Sparse is better than dense.
Readability counts.
Special cases aren't special enough to break the rules.
Although practicality beats purity.
Errors should never pass silently.
Unless explicitly silenced.
In the face of ambiguity, refuse the temptation to guess.
There should be one-- and preferably only one --obvious way to do it.
Although that way may not be obvious at first unless you're Dutch.
Now is better than never.
Although never is often better than *right* now.
If the implementation is hard to explain, it's a bad idea.
If the implementation is easy to explain, it may be a good idea.
Namespaces are one honking great idea -- let's do more of those!
```

### Quality

We focus on code-quality to make ours and others life easier. For that reason:

- :heavy_check_mark: **DO** write documentation for any public type and field.
- :heavy_check_mark: **DO** write a test for all the possible code branches of
  your methods. Use a TDD approach.
- :heavy_check_mark: **DO** seek for 100% test coverage.
- :heavy_check_mark: **DO** seek for compiler warning free code.
- :heavy_check_mark: **DO** check the code with StyleCop for style issues.
- :heavy_check_mark: **DO** check the code with Gendarme for design issues.
- :heavy_check_mark: **DO** review the results of SonarQube in the Pull Request.
- :heavy_check_mark: **DO** make sure the CI pass.

### Style Guidelines

#### Indentation

- :heavy_check_mark: **DO** use **spaces** with an indentation level of 4
  spaces.
- :x: **DO NOT** use tabs.

#### New lines

- :heavy_check_mark: **DO** use Unix new lines: `\n` instead of Windows style
  `\r\n`. In general, Git will handle that for you.
- :heavy_check_mark: **DO** make sure there is an empty line at the end of the
  file. This ensure the latest line ends with the new line character and adding
  new lines after it won't show that line as changed in the diff.

#### Line length

- :heavy_check_mark: **DO** use a limit of 80 columns. If you need to wrap, move
  to the next line with one extra indentation level.
- :heavy_check_mark: **DO** put all the arguments in a new line if they don't
  fit.
- :heavy_check_mark: **DO** use local variables to make small conditions.

```csharp
void Method(
    int a,
    string b,
    int c)
{
    OtherMethod(
        a,
        b,
        c);

    bool z = (a > 3) && (a < 5);
    bool w = b.StartsWith("hello");
    if (z && w) {
        Code();
    }
}
```

#### Layout

- :heavy_check_mark: **DO** define a type (class / struct / enum) per file.
- :heavy_check_mark: **DO** separate methods and properties with new lines.
- :heavy_check_mark: **DO** place the elements in this order: private fields,
  constructors, properties, methods, nested types. Place first static fields and
  order by visibility: public, protected, private.

#### Spacing rules

- :x: **DO NOT** leave any trailing spaces.
- :x: **DO NOT** use space before opening parenthesis calling methods or
  indexers, between the parenthesis and the arguments or between the generic
  types.

```csharp
Method ( a );
array [ 10 ];
var list = new List <int> ();
```

- :heavy_check_mark: **DO** use the following convention:

```csharp
Method(a);
array[10];
var list = new List<int>();
```

- :heavy_check_mark: **DO** use spaces and parenthesis for clarity in math
  operations:

```csharp
int b = (a + (5 * 2)) / (3 + 3);
```

- :heavy_check_mark: **DO** indent `case` statements:

```csharp
switch (a) {
    case 3:
        c = "hello";
        break;

    case 5:
        c = "world";
        break;

    default:
        throw new Exception();
}
```

#### Brace position

- :heavy_check_mark: **DO** put the opening brace on the same line for
  conditions, loops and try-catch.

```csharp
if (a) {
    Code();
    Code();
} else if (b) {
    Code();
} else {
    Code();
}

try {
    Something();
} catch (ArgumentNullException ex) {
    Something();
} finally {
    Something();
}

for (int i = 0; i < 2; i++) {
    Something();
}
```

- :heavy_check_mark: **DO** use braces for one line conditions and loops. This
  improves readability and avoid having changed lines just to add the brace when
  it requires extra logic. The exception is for one line conditions for argument
  checking.

```csharp
if (a) {
    Code();
}
```

- :heavy_check_mark: **DO** put the brace in a new line when defining the
  namespace, a type or a method.

```csharp
namespace Program.Text
{
    public class Abc
    {
        public void MyMethod()
        {
        }
    }
}
```

- :heavy_check_mark: **DO** put the brace in the same line for properties and
  indexers.

```csharp
public int Property {
    get {
        return value;
    }
}
```

- :heavy_check_mark: **DO** put each brace on a new line for empty methods.

```csharp
void EmptyMethod()
{
}
```

#### Multiline comments

- :heavy_check_mark: **DO** use always double slash comments.

```csharp
// Blah
// Blah again
// and another Blah
```

### Properties

- :x: **DO NOT** use public variables under any circumstance.

- :heavy_check_mark: **DO** use static properties for constants.

- :heavy_check_mark: **DO** put the getter and setter in a new line for
  automatic or one line properties.

```csharp
public int Property {
    get { return value; }
    set { x = value; }
}

public int Text {
    get;
    private set;
}
```

### File headers

- :heavy_check_mark: **DO** put the license in the file header with this format:

```csharp
//
// <FileName>.cs
//
// Author:
//       <AuthorName> <email@example.com>
//
// Copyright (c) <Year> <AuthorName>
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
```

### Naming

- :heavy_check_mark: **DO** use **always** camel casing.

```csharp
void Method(string myArgument)

class MyClass
{
    string myString;
    int veryImportantValue;
}
```

- :x: **DO NOT** use `m_` or `_` as prefixes for private instance members. The
  private visibility was created for that, really.

### Keyword `this`

- :x: **DO NOT** use `this` if it's not needed.
- :heavy_check_mark: **DO** use `this` if the method has an argument with the
  same name.

```csharp
class Foo
{
    int bar;

    public Foo(int bar)
    {
        this.bar = bar;
    }

    void Update(int newValue)
    {
       bar = newValue;
       Method();
    }

    public void Method()
    {
    }
}
```

### Keyword `var`

- :heavy_check_mark: **DO** use the `var` on the left-hand side of an assignment
  when the type name is repeated on the right hand side:

```csharp
var monkeyUUID = new NSUuid(uuid);
NSUuid something = RetrieveUUID();
```

### Initializing instances

- :heavy_check_mark: **DO** use the C# syntax to initialize instances.

```csharp
var x = new Foo {
    Label = "This",
    Color = Color.Red
};

string[] array = { "a", "b", "c" };
var array2 = new string[] { "d", "e", "f" };

var list = new List<string> {
    "hello",
    "world"
};

var dict = new Dictionary<string, int> {
    { "hello": 0 },
    { "world": 1 }
};
```

### Redundant visibility

- :x: **DO NOT** use the `private` keyword to indicate internal fields since
  it's already the default visibility.

### Usings

- :heavy_check_mark: **DO** put the `using` inside the namespace.
- :heavy_check_mark: **DO** include all the namespaces you are using.

- :heavy_check_mark: **DO** use the `using` statement for `IDisposable` types.

### Built-in types

- :heavy_check_mark: **DO** use the built-in type alias instead of the class
  names.

```csharp
int a = 5;
long b = 5;
string c = "hello";
string d = int.Parse("5");
```

#### Integers

- :heavy_check_mark: **DO** try to avoid unsigned integers in public method
  arguments and properties. Some .NET language doesn't support them.

#### Strings

- :heavy_check_mark: **DO** use the new string interpolation:

```csharp
int a = 5;
string b = $"The result is {a}";
```

- :heavy_check_mark: **DO** use the `StringBuilder` class when creating strings
  with many operations.
