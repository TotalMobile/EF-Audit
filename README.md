# Phnx.Audit.EF
This project allows auditing to be automatically tracked and scanned, whilst still leaving the developer in control of when and what to audit. 

Unlike most auditing libraries, it does not override entity framework's `OnChanges()` and use reflection, allowing developers to opt-in to auditing for only certain models and scenarios, as well as adding any extra metadata to any request (such as a description of the action, a user ID, and more).

# Usage

## Setup
You can optionally set up dependency injection using `Phnx.Audit.EF.DependencyInjection`, which adds all the interfaces to the `IServiceCollection` via `AddAuditing`.

```cs
ConfigureServices(IServiceCollection services)
{
    services.AddAuditing<MyDbContext>();
}
```

You can also set it up by hand with something like

```cs
ConfigureServices(IServiceCollection services)
{
    services.AddTransient<IAuditService<MyDbContext>, AuditService<MyDbContext>>();
    services.AddTransient<IAuditWriter<MyDbContext>, AuditWriter<MyDbContext>>();
    services.AddTransient<IChangeDetectionService<MyDbContext>, ChangeDetectionService<MyDbContext>>();
}
```

## Creating an audit entry

```cs
var factory = auditService.GenerateForEntries<ModelToAudit, MyAuditModel, int>(m => m.Id);

factory.GenerateEntry(model)
    .WithDescription("Something was updated")
    .AddToDatabase();
```


Phnx.Audit.EF is based on a fluent API. This is because it offers many features that are optional. You can add your own extension methods onto `FluentAudit` if you want to add your own metadata, including metadata specific to certain audit events (such as who authorized a change, if a change requires review).

# TODO

* Reduce and simplify generics
* Allow AuditEntryDataModel with aggregate keys
