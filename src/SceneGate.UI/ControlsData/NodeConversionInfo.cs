namespace SceneGate.UI.ControlsData;
using System;
using Yarhl.FileSystem;

public record class NodeConversionInfo(Node Node, Type ConverterType, object? ConversionParameters);
