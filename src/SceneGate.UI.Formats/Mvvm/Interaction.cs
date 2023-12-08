namespace SceneGate.UI.Formats.Mvvm;

using System;

/// <summary>
/// Interaction to request data to the view from the view model.
/// </summary>
public class Interaction<TInput, TOutput>
{
    private Func<TInput, TOutput>? _handler;

    /// <summary>
    /// Run the handler method from the view to request the data.
    /// </summary>
    public TOutput Handle(TInput input)
    {
        if (_handler is null) {
            throw new InvalidOperationException("Missing handler");
        }

        return _handler(input);
    }

    /// <summary>
    /// Register the handler to run.
    /// </summary>
    /// <param name="handler">The handler to run.</param>
    public void RegisterHandler(Func<TInput, TOutput> handler)
    {
        _handler = handler;
    }
}

/// <summary>
/// Interaction to request data to the view from the view model.
/// </summary>
public class Interaction<TOutput>
{
    private Func<TOutput>? _handler;

    /// <summary>
    /// Run the handler method from the view to request the data.
    /// </summary>
    public TOutput Handle()
    {
        if (_handler is null) {
            throw new InvalidOperationException("Missing handler");
        }

        return _handler();
    }

    /// <summary>
    /// Register the handler to run.
    /// </summary>
    /// <param name="handler">The handler to run.</param>
    public void RegisterHandler(Func<TOutput> handler)
    {
        _handler = handler;
    }
}
