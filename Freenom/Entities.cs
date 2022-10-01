using System;

namespace Freenom
{
    /// <summary>
    /// The color of a renewal domain.
    /// </summary>
    public enum DomainColor
    {
        Green,
        Red
    }

    /// <summary>
    /// Represents a domain that might be renewable.
    /// </summary>
    public class RenewalDomain
    {
        /// <summary>
        /// The registered domain name value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The ID of this domain. Used in <see cref="FreenomClient.RenewDomain(long, int)"/>.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// The URI where this domain is renewable at.
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// The status of this domain, such as <c>Active</c> or <c>Fraud</c>.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Time remaining until this domain expires.
        /// </summary>
        public TimeSpan Remaining { get; set; }

        /// <summary>
        /// A message describing the status of this domain.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The color of this domain.
        /// </summary>
        public DomainColor Color { get; set; }

        /// <summary>
        /// Figures out whether this domain is renewable.
        /// </summary>
        public bool Renewable
        {
            get
            {
                return Remaining.TotalDays <= 14 && Message == "Renewable";
            }
        }
    }

    /// <summary>
    /// Represents basic information about your Freenom account.
    /// </summary>
    public class AccountInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}