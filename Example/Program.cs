using Freenom;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Example
{
    public static class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("Enter the Freenom account email address to use: ");
            string email = Console.ReadLine();

            Console.WriteLine("Enter the Freenom account password to use: ");
            string password = Console.ReadLine();
            FreenomClient freenom = new();

            Console.WriteLine("> Logging in");
            string name = await freenom.Login(email, password);
            Console.WriteLine($"Logged in as: {name}");

            Console.WriteLine();
            Console.WriteLine("> Getting account info");
            AccountInfo info = await freenom.GetAccountInfo();
            Console.WriteLine("Got account info");

            Console.WriteLine();
            Console.WriteLine($"Full Name: {info.FirstName} {info.LastName}");
            Console.WriteLine($"Email: {info.Email}");
            Console.WriteLine($"Phone: {info.Phone}");

            Console.WriteLine();
            Console.WriteLine("> Getting renewals info");
            RenewalDomain[] renewals = await freenom.GetRenewals();
            Console.WriteLine("Got renewals info");

            RenewalDomain[] renewable = renewals.Where(x => x.Renewable).ToArray();

            Console.WriteLine();
            Console.WriteLine($"> Renewing {renewable.Length} domains");

            foreach (RenewalDomain domain in renewable)
            {
                long orderId = await freenom.RenewDomain(domain.Id, 12);

                Console.WriteLine($"Successfully renewed {domain.Value} (#{domain.Id}), order ID: {orderId}");
            }

            await freenom.Logout();

            Console.WriteLine("Demo done");
            Console.ReadKey();
        }
    }
}