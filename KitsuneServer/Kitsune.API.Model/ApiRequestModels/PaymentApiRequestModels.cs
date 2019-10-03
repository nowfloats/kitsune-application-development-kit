using MongoDB.Bson;
using System.Collections.Generic;

namespace Kitsune.API.Model.ApiRequestModels
{
    //public class DeveloperModel
    //{
    //    public ObjectId _id { get; set; }
    //    public string UserName { get; set; }
    //    public string SecurityStamp { get; set; }
    //    public string Email { get; set; }
    //    public bool EmailConfirmed { get; set; }
    //    public string PhoneNumber { get; set; }
    //    public bool PhoneNumberConfirmed { get; set; }
    //    public bool TwoFactorEnabled { get; set; }
    //    public string LockoutEndDateUtc { get; set; }
    //    public bool LockoutEnabled { get; set; }
    //    public int AccessFailedCount { get; set; }
    //    public List<string> Roles { get; set; }
    //    public List<LoginField> Logins { get; set; }
    //    public List<string> Claims { get; set; }
    //    public string DisplayName { get; set; }
    //    public bool IsArchived { get; set; }
    //    public BsonDateTime CreatedOn { get; set; }
    //    public BsonDateTime UpdatedOn { get; set; }
    //    public int Level { get; set; }
    //    public string About { get; set; }
    //    public string Twitter { get; set; }
    //    public string Facebook { get; set; }
    //    public string ProfilePic { get; set; }
    //    public string Github { get; set; }
    //    public string Google { get; set; }
    //    public string MonitorGroupId { get; set; }
    //    public Address Address { get; set; }
    //    public Wallet Wallet { get; set; }
    //}


    public class CreatePaymentRequestModel
    {
        public string username { get; set; }
        public double amount { get; set; }
        public string responseurl { get; set; }
    }

    public class CreateInternationalPaymentRequestModel: CreatePaymentRequestModel
    {
        public string token { get; set; }
        public string currency { get; set; }
    }

    public class InstamojoWebhookRequestModel
    {
        public string payment_id { get; set; }
        public string status { get; set; }
        public object longurl { get; set; }
        public string buyer_name { get; set; }
        public string buyer_phone { get; set; }
        public string buyer { get; set; }
        public string currency { get; set; }
        public string amount { get; set; }
        public string fees { get; set; }
        public string mac { get; set; }
        public string payment_request_id { get; set; }
        public string purpose { get; set; }
        public string shorturl { get; set; }
    }

    //public class LoginField
    //{
    //    public string LoginProvider { get; set; }
    //    public string ProviderKey { get; set; }
    //}
}