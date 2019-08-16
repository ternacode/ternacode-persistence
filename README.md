# Ternacode Persistence [![Build status](https://img.shields.io/appveyor/ci/kristofferkarlsson/ternacode-persistence/master?style=flat-square)](https://ci.appveyor.com/project/kristofferkarlsson/ternacode-persistence/branch/master)

This project contains abstractions and an EntityFramework Core implementation for repository and unit of work patterns. The purpose is mainly to enable using EF Core while still maintaining a clean, testable architecture.

Package                                     | Stable
--------------------------------------------|-------------
`Ternacode.Persistence.Abstractions`        | [![NuGet](https://img.shields.io/nuget/v/Ternacode.Persistence.Abstractions?style=flat-square)](https://www.nuget.org/packages/Ternacode.Persistence.Abstractions/)
`Ternacode.Persistence.EntityFrameworkCore` | [![NuGet](https://img.shields.io/nuget/v/Ternacode.Persistence.EntityFrameworkCore?style=flat-square)](https://www.nuget.org/packages/Ternacode.Persistence.EntityFrameworkCore/)

## Usage
The quickest way to get started is to look at the example project, containing a sample of how this library is used. In particular, the example contains a small ASP .NET Core API: ```Ternacode.Persistence.Example.API```. In ```Startup.ConfigureServices```, extension methods provided by the library are used to inject everything needed:

First, call ```AddPersistence``` on an ```IServiceCollection``` with a factory lambda returning the ```DbContext```. Optionally, a ```PersistenceOptions``` configuration can be provided. If no configuration is provided, then the transaction type will be ```TransactionScope``` and no context pool will be used.

Second, on the returned ```IPersistenceBuilder```, call ```AddEntity``` for each ```DbSet<TEntity>``` in your context that repositories should be made available for.

```csharp
var options = new PersistenceOptions
{
    UnitOfWorkTransactionType = TransactionType.DbContextTransaction,
    UseContextPool = false
};

services.AddPersistence(() =>
        {
            var factory = new BlogContextFactory(dbConnectionString);
            return factory.CreateDbContext();
        }, options)
        .AddEntity(c => c.Users)
        .AddEntity(c => c.Posts);
```

In your domain logic classes, inject ```IRepository<TEntity>``` and ```IUnitOfWork``` interfaces as needed. To query the database, call ```IRepository<TEntity>.Query``` with an instance of a class extending ```BaseQuery<TEntity>```.

## FAQ

### Can I use multiple ```DbContext``` implementations?
Not at the moment. It's understandable that some projects want to use multiple ```DbContext```s, e.g., representing different bounded contexts. Right now, this is not supported, but it's definitely a feature that could be implemented.

### Can I use other DI containers than ones implementing ```IServiceCollection```?
Currently, there are only extension methods for this particular DI container interface. However, the library itself has no limitation using only ```IServiceCollection```.

### Doesn't EF Core already implement a repository pattern?
It can absolutely be argued that this is the case. However, this project focuses on testability and removing implementation dependent persistence details from domain code. This cannot easily be achieved by using EF Core directly.

### Why not just use, e.g., ```DbContext.Database.AutoTransactionsEnabled``` instead of ```IUnitOfWork```?
While it is certainly possible to utilize transactions for EF Core without this library, the advantage to using ```IUnitOfWork``` is that it offers a clean way for the developer to explicitly state that the code contained will be run inside a transaction. Furthermore, the Unit of Work pattern allows for easier injection and use of custom transactional behavior. This is evident by the fact that there are two implementations of ```IUnitOfWork``` in ```Ternacode.Persistence.EntityFrameworkCore``` --- one using ```DbContextTransaction``` and one using ```TransactionScope```. The ```PersistenceOptions``` configuration used when calling ```AddPersistence``` determines which ```TransactionType``` will be used.

### Isn't it pointless to separate abstractions out in its own project, since it's virtually impossible to abstract away all the intricacies of different ORMs anyway?
Yes, abstracting out everything is not realistically possible if supporting several ORMs. The point of this project is rather to enable having domain logic only referring to abstractions, not having to pull in implementation specific packages. Currently, this project has an EF Core focus.

### I'm missing feature X!
If there is something missing that you think should be added, feel free to add an issue or PR :)
