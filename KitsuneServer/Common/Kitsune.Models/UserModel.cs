using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models
{
    public class UserModel : MongoEntity
    {
        public UserModel()
        {
            Claims = new List<string>();
            Roles = new List<string>();
            Logins = new List<LoginField>();
        }
        public string UserName { get; set; }
        public string ClientId { get; set; }
        public string SecurityStamp { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public string LockoutEndDateUtc { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public List<string> Roles { get; set; }
        public List<LoginField> Logins { get; set; }
        public List<string> Claims { get; set; }
        public string DisplayName { get; set; }
        public bool IsArchived { get; set; }
        public DateTime UpdatedOn { get; set; }
        public UserLevel Level { get; set; }
        public string About { get; set; }
        public string Twitter { get; set; }
        public string Facebook { get; set; }
        public string ProfilePic { get; set; }
        public string Github { get; set; }
        public string Google { get; set; }
        public string MonitorGroupId { get; set; }
        public string PhoneNumber { get; set; }
        public Address Address { get; set; }
        public Wallet Wallet { get; set; }
        public string GSTIN { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    public class Address
    {
        public string AddressDetail { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Pin { get; set; }
    }
    public class Wallet
    {
        public double Balance { get; set; }
        public double CreditLimit { get; set; }
        public DateTime UpdatedOn { get; set; }
        public double UnbilledUsage { get; set; }
        public double CustomerCredit { get; set; }
    }
    public enum UserLevel
    {
        Beginner,
        Intermediate,
        Advanced
    }

    public class WalletStats
    {
        public string UserEmail { get; set; }
        public double Amount { get; set; }
        public bool IsAdded { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedOn { get; set; }
    }
    public class LoginField
    {
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
    }
}
