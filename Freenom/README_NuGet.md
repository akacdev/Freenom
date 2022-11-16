# Freenom

![](https://raw.githubusercontent.com/actually-akac/Freenom/master/Freenom/icon.png)

An async C# library for automatically renewing Freenom domains.

> **Warning**

> This library doesn't implement domain registration and configuration web API routes, as these can be easily abused for spam.

## Usage
Provides an easy interface for interacting with the Freenom web API routes. This allows you to programmatically renew your domains.

To get started, add the library into your solution with either the `NuGet Package Manager` or the `dotnet` CLI.
```rust
dotnet add package Freenom.NET
```

For the primary classes to become available, import the used namespace.
```csharp
using Freenom;
```

Need more examples? Under the `Example` directory you can find a working demo project that implements this library.

## Dependencies
- `HtmlAgilityPack` ([GitHub](https://github.com/zzzprojects/html-agility-pack)) ([Website](https://html-agility-pack.net/)): For efficiently parsing the DOM and reading data out of it.

## Code Samples

### Creating a new Freenom client 
```csharp
FreenomClient freenom = new();
```

### Logging in
```csharp
string name = await freenom.Login("EMAIL", "PASSWORD");
```

### Getting basic account information
```csharp
AccountInfo info = await freenom.GetAccountInfo();
```

### Getting all domains under the account that might be renewable
```csharp
RenewalDomain[] renewals = await freenom.GetRenewals();
```

### Filtering domains that are currently renewable using LINQ
```csharp
RenewalDomain[] renewable = renewals.Where(x => x.Renewable).ToArray();
```

### Renewing a domain for 12 months
```csharp
long orderId = await freenom.RenewDomain(6236693445, 12);
```

## Features
- Built for **.NET 6** and **.NET 7**
- Fully **async**
- Extensive **XML documentation**
- **Custom exceptions** (`FreenomException`) for advanced catching
- **Automatically renew** Freenom domains that are expiring soon
- Automatic request retries
- Example project to show the usage of the library

## Available methods
- Task\<string> **Login**(string email, string password)
- Task **Logout**()
- Task\<AccountInfo> **GetAccountInfo**()
- Task\<RenewalDomain[]> **GetRenewals**()
- Task\<long> **RenewDomain**(long id, int months)

## References
- https://www.freenom.com/

*This is a community-ran library. Not affiliated with OpenTLD BV.*