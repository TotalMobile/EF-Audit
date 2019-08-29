# Phnx.Audit.EF
This project allows auditing to be automatically tracked and scanned, whilst still leaving the developer in control of when and what to audit. 

Unlike most auditing libraries, it does not override entity framework's `OnChanges()` and use reflection, allowing developers to opt-in to auditing for only certain models and scenarios, as well as adding any extra metadata to any request (such as a description of the action, a user ID, and more).

This audit engine tracks the before and after state of a tracked model in JSON. This means that this engine excels at listing all changes for a certain tracked entity, but does not perform as well when searching for when certain members are changed from one value to another, as searching JSON is not supported well by EntityFrameworkCore. However, this can usually be achieved with JSON tools from your specific SQL provider. 

# Usage

## Setup

### Services
You can optionally set up dependency injection using `Phnx.Audit.EF.DependencyInjection`, which adds all the interfaces to the `IServiceCollection` via `AddPhoenixAuditing`.

```cs
ConfigureServices(IServiceCollection services)
{
    services.AddPhoenixAuditing<MyDbContext>();
}
```

You can also set it up by hand with something like

```cs
ConfigureServices(IServiceCollection services)
{
    services.AddTransient<IAuditService<MyDbContext>, AuditService<MyDbContext>>();
    services.AddTransient<IChangeDetectionService<MyDbContext>, ChangeDetectionService<MyDbContext>>();
}
```

### Audit Types

Each audit table must have its own audit type. This is because Entity Framework requires one type per table. 

If you have a primary key for your entities, and you want to use a foreign key ID in your C#, you must inherit your audit table type from `AuditEntryDataModel<TEntity, TKey>`, and set the type of `TKey` to the type of the ID for the table you are tracking.

If you have an aggregate primary key, or you don't want to use a foreign key ID in your C#, then you can use `AuditEntryDataModel<TEntity>` instead. `TEntity` must be set to the type of the table you are tracking. 

You can create audits with additional metadata in your metadata logs if you want to, or just use the minimal data.

In this example, `Products` is the table under audit

#### Without extra audit data
```cs
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    
    public List<AuditEntryDataModel<Product>> Audits { get; set; }
}
```

#### With extra audit data
```cs
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    
    public List<ProductAudit> Audits { get; set; }
}

public class ProductAudit : AuditEntryDataModel<Product>
{
    // Any extra audit data can be added as columns here
    public string string ChangeApprovedByUserId { get; set; }
}
```

### Database

Phnx.Audit.EF provides an extension method in `Microsoft.EntityFrameworkCore` for `EntityTypeBuilder`. This allows you to use the `OnConfiguring` method in your `DbContext` to add audit tables, and automatically configure them.

Two extensions are available. One can optionally enforce the type of the entity ID, and the other does not. If you have an aggregate primary key (more than one column combine together to make the primary key), you must use the latter method.

```cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // With foreign key type enforcement
    modelBuilder.Entity<ModelToAudit>()
        .HasAudit<ModelToAudit, AuditEntryModel, string>(m => m.Audits);
        
    // Without foreign key type enforcement
    modelBuilder.Entity<ModelToAudit>()
        .HasAudit(m => m.Audits);
        
    base.OnModelCreating(modelBuilder);
}
```

Underneath, these extension methods set the Entity to a foreign object, and set the `OnDelete` action to restricted.

## Creating an audit entry

```cs
auditService.GenerateEntry<ModelToAudit, MyAuditModel>(model);
```

`Phnx.Audit.EF` is based on a fluent API. This is because it offers many features that are optional. You can add your own extension methods onto `FluentAudit` if you want to add your own metadata, including metadata specific to certain audit events (such as who authorized a change, if a change requires review).

#### Without extra audit data
```cs
auditService.GenerateEntry<Product, AuditEntryDataModel<Product>>(model)
    .WithDescription("Something was updated")
    .WithUserId("sample-user-id");
```

#### With extra audit data
```cs
ProductAuditModel auditModel = auditService.GenerateEntry<Product, ProductAuditModel>(model)
    .WithDescription("Something was updated")
    .WithUserId("sample-user-id");

// Set any extra columns or metadata here
auditModel.ChangeApprovedByUserId = approvedByUserId;
```
