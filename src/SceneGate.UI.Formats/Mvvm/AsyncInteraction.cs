namespace SceneGate.UI.Formats.Mvvm;

using System;
using System.Threading.Tasks;

/// <summary>
/// Asynchronous interaction to request data to the view from the view model.
/// </summary>
/// <typeparam name="TInput">Type of the input data from the view model.</typeparam>
/// <typeparam name="TOutput">Type of the output data from the view.</typeparam>
public class AsyncInteraction<TInput, TOutput>
{
    private Func<TInput, Task<TOutput>>? handler;

    /// <summary>
    /// Run the handler method from the view to request the data.
    /// </summary>
    /// <param name="input">Input data from the view model.</param>
    /// <returns>Output data from the view.</returns>
    /// <exception cref="InvalidOperationException">The view didn't register a handler.</exception>
    public Task<TOutput> HandleAsync(TInput input)
    {
        if (handler is null) {
            throw new InvalidOperationException("Missing handler");
        }

        return handler(input);
    }

    /// <summary>
    /// Register the handler to run.
    /// </summary>
    /// <param name="handler">The handler to run.</param>
    public void RegisterHandler(Func<TInput, Task<TOutput>> handler)
    {
        this.handler = handler;
    }
}

/// <summary>
/// Asynchronous interaction to request data to the view from the view model.
/// </summary>
/// <typeparam name="TOutput">Type of the output data from the view.</typeparam>
public class AsyncInteraction<TOutput>
{
    private Func<Task<TOutput>>? handler;

    /// <summary>
    /// Run the handler method from the view to request the data.
    /// </summary>
    /// <returns>Output data from the view.</returns>
    /// <exception cref="InvalidOperationException">The view didn't register a handler.</exception>
    public Task<TOutput> HandleAsync()
    {
        if (handler is null) {
            throw new InvalidOperationException("Missing handler");
        }

        return handler();
    }

    /// <summary>
    /// Register the handler to run.
    /// </summary>
    /// <param name="handler">The handler to run.</param>
    public void RegisterHandler(Func<Task<TOutput>> handler)
    {
        this.handler = handler;
    }
}

/// <summary>
/// Asynchronous interaction to request data to the view from the view model.
/// </summary>
public class AsyncInteraction
{
    private Func<Task>? handler;

    /// <summary>
    /// Run the handler method from the view to request the data.
    /// </summary>
    /// <exception cref="InvalidOperationException">The view didn't register a handler.</exception>
    public Task HandleAsync()
    {
        if (handler is null) {
            throw new InvalidOperationException("Missing handler");
        }

        return handler();
    }

    /// <summary>
    /// Register the handler to run.
    /// </summary>
    /// <param name="handler">The handler to run.</param>
    public void RegisterHandler(Func<Task> handler)
    {
        this.handler = handler;
    }
}
