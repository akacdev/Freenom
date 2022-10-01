# Freenom

<div align="center">
  <img width="256" height="256" src="https://raw.githubusercontent.com/actually-akac/Freenom/master/Freenom/icon.png">
</div>

<div align="center">
  An async C# library for automatically renewing Freenom domains.
</div>

<br>

> **Warning**<br>
> This library doesn't implement domain registration and configuration GUI API routes, as these can be easily abused for spam.

## Usage
Available on NuGet as `Freenom.NET`, methods can be found under the class `FreenomClient`.<br/>
https://www.nuget.org/packages/Freenom.NET

## Dependencies
- `HtmlAgilityPack` ([GitHub](https://github.com/zzzprojects/html-agility-pack)) ([Website](https://html-agility-pack.net/)): For efficiently parsing the DOM and reading data out of it.

## Example
Under the `Example` folder you can find a demo application that implements the library.

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

### Filter for domains that are currently renewable using LINQ
```csharp
RenewalDomain[] renewable = renewals.Where(x => x.Renewable).ToArray();
```

### Renewing a domain for 12 months
```csharp
long orderId = await freenom.RenewDomain(6236693445, 12);
```

## Features
- Made with **.NET 6**
- Fully **async**
- Coverage of the **domain renewal** GUI API routes
- **Automatically renew** Freenom domains that are expiring soon
- **Custom exceptions** (`FreenomException`) for advanced catching
- Automatic request retries
- Example project to show the usage of the library

## Available methods
- Task\<string> **Login**(string email, string password)
- Task **Logout**()
- Task\<AccountInfo> **GetAccountInfo**()
- Task\<RenewalDomain[]> **GetRenewals**()
- Task\<long> **RenewDomain**(long id, int months)

## Official Links
https://www.freenom.com/