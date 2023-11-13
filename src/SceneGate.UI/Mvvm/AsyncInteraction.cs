namespace SceneGate.UI.Mvvm;

using System;
using System.Threading.Tasks;

public class AsyncInteraction<TInput, TOutput>
{
    private Func<TInput, Task<TOutput>>? _handler;

    public Task<TOutput> HandleAsync(TInput input)
    {
        if (_handler is null) {
            throw new InvalidOperationException("Missing handler");
        }

        return _handler(input);
    }

    public void RegisterHandler(Func<TInput, Task<TOutput>> handler)
    {
        _handler = handler;
    }
}

public class AsyncInteraction<TOutput>
{
    private Func<Task<TOutput>>? _handler;

    public Task<TOutput> HandleAsync()
    {
        if (_handler is null) {
            throw new InvalidOperationException("Missing handler");
        }

        return _handler();
    }

    public void RegisterHandler(Func<Task<TOutput>> handler)
    {
        _handler = handler;
    }
}

public class AsyncInteraction
{
    private Func<Task>? _handler;

    public Task HandleAsync()
    {
        if (_handler is null) {
            throw new InvalidOperationException("Missing handler");
        }

        return _handler();
    }

    public void RegisterHandler(Func<Task> handler)
    {
        _handler = handler;
    }
}
