namespace SceneGate.UI.Mvvm;

using System;

public class Interaction<TInput, TOutput>
{
    private Func<TInput, TOutput>? _handler;

    public TOutput Handle(TInput input)
    {
        if (_handler is null) {
            throw new InvalidOperationException("Missing handler");
        }

        return _handler(input);
    }

    public void RegisterHandler(Func<TInput, TOutput> handler)
    {
        _handler = handler;
    }
}

public class Interaction<TOutput>
{
    private Func<TOutput>? _handler;

    public TOutput Handle()
    {
        if (_handler is null) {
            throw new InvalidOperationException("Missing handler");
        }

        return _handler();
    }

    public void RegisterHandler(Func<TOutput> handler)
    {
        _handler = handler;
    }
}
