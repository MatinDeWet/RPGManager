# CQRS

A small, framework-agnostic CQRS toolkit reused across services. Two projects:

- **`CQRS.Domain`** — persistence- and infrastructure-ignorant marker contracts. Currently just `IDomainEvent`. Domain entities depend only on this.
- **`CQRS.Core`** — the runtime: command/query/domain-event manager contracts, the in-memory domain-event pipeline, and the `AddCQRSSupport` DI wiring.

## Contracts

| Interface | Purpose |
| --- | --- |
| `ICommand` / `ICommand<TResponse>` | Marker for a write request. |
| `IQuery<TResponse>` | Marker for a read request. |
| `ICommandManager<TCommand>` / `ICommandManager<TCommand, TResponse>` | Handles a command, returns `Ardalis.Result`. |
| `IQueryManager<TQuery, TResponse>` | Handles a query, returns `Ardalis.Result<TResponse>`. |
| `IDomainEventManager<TEvent>` | Handles a domain event (may be multiple per event). |
| `IDomainEventsDispatcher` | Hands raised domain events to the background queue. |

## Registration

```csharp
// `assemblyPointer` is any type in the assembly to scan for handlers.
services.AddCQRSSupport(typeof(SomeHandler));

// Optionally decorate the command/query pipeline (e.g. validation, logging):
services.AddCQRSSupport(typeof(SomeHandler), decorate =>
    decorate.TryDecorateIfImplementationsExist(typeof(ICommandManager<>), typeof(LoggingCommandManager<>)));
```

`AddCQRSSupport` scans for command/query/domain-event managers (registered scoped),
and wires the domain-event queue, dispatcher, and background processor.

> **Decorator scope:** the `configureDecorators` callback runs *before* domain-event
> managers are registered, so it can decorate command/query managers but **not**
> domain-event managers.

## Domain events: semantics (read this)

Domain events are **in-memory, out-of-band, and best-effort**:

- **Not durable.** The queue is an in-process `Channel`. Events queued but not yet
  processed are **lost on process crash, restart, or shutdown** — there is no drain.
- **Decoupled from the originating transaction.** Handlers run on a background
  `IHostedService` in a fresh DI scope, *after* `DispatchAsync`. A failing handler is
  logged and swallowed; it does **not** roll back the work that raised the event.
- **Sequential.** A single processor handles one event at a time across the whole app;
  all handlers for an event run sequentially within one scope.
- **Backpressured.** The channel is bounded (default 1000, `FullMode = Wait`), so once
  full, `EnqueueAsync` — and therefore `DispatchAsync` — blocks until space frees up.

Use them only for side effects that may be retried, delayed, or lost (notifications,
cache invalidation, projections that can be rebuilt). For anything that *must* happen
atomically with the originating change, do it inside the command itself or adopt a
durable outbox instead.
