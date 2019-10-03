using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Kitsune.Models;

namespace Kitsune.API.Model.ApiRequestModels
{
    public class GetDeveloperSummaryRequestModel : BaseModel
    {
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.UserEmail))
                validationResultList.Add(new ValidationResult("User Name can not be empty"));

            return validationResultList;
        }
    }

    public class GetDeveloperSummaryResponseModel
    {
        public long KitsuneUserCount { get; set; }
        public long KitsuneCustomerCount { get; set; }
        public long KitsunePageViewCount { get; set; }
    }

    public class GetUserIdRequestModel : BaseModel
    {
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.UserEmail))
                validationResultList.Add(new ValidationResult("User Name can not be empty"));

            return validationResultList;
        }
    }

    public class GetUserIdResult : ErrorApiResponseModel
    {
        public string Id { get; set; }
    }

    public class GetDeveloperProfileRequestModel : BaseModel
    {
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.UserEmail))
                validationResultList.Add(new ValidationResult("UserEmail can not be empty"));

            return validationResultList;
        }
    }

    public class GetDeveloperProfileResponseModel
    {
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public UserLevel Level { get; set; }
        public string About { get; set; }
        public string Twitter { get; set; }
        public string Facebook { get; set; }
        public string ProfilePic { get; set; }
        public string Github { get; set; }
        public string Google { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public List<ProjectItem> Projects { get; set; }
        public Wallet Wallet { get; set; }
        public string PhoneNumber { get; set; }
        public Address Address { get; set; }
        public string GSTIN { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class ProjectItem
    {
        public string ProjectName { get; set; }
        public string ProjectId { get; set; }
    }

    public class GetUserPaymentStatsRequestModel : BaseModel
    {
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.UserEmail))
                validationResultList.Add(new ValidationResult("UserEmail can not be empty"));

            return validationResultList;
        }
    }

    public class PaymentStats
    {
        public string InvoiceId { get; set; }
        public Double Amount { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string Status { get; set; }
        public string DebitDetail { get; set; }
    }

    public class GetUserPaymentStatsResponseModel
    {
        public List<PaymentStats> WalletStats { get; set; }
    }

    public class GetDeveloperDebitDetailsRequestModel : BaseModel
    {
        public string Component { get; set; }
        public DateTime MonthAndYear { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.UserEmail))
                validationResultList.Add(new ValidationResult("UserEmail can not be empty"));

            return validationResultList;
        }
    }

    public class GetDeveloperDebitDetailsResponseModel<T1, T2>
    {
        public List<T1> Usage { get; set; }
        public T2 Meta { get; set; }
    }

    public class DebitDetailsMetaData
    {
        public string CurrentComponent { get; set; }
        public DateTime CurrentMonthAndYear { get; set; }
        public List<DateTime> MonthAndYears { get; set; }
        public List<string> ComponentsUsed { get; set; }
    }

    public class UpdateDeveloperWalletRequestModel : BaseModel
    {
        public double Amount { get; set; }
        public bool Add { get; set; }

        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.UserEmail))
                validationResultList.Add(new ValidationResult("UserEmail can not be empty"));

            return validationResultList;
        }

    }

    public class UpdateDeveloperDetailsRequestModel : BaseModel
    {
        public string PhoneNumber { get; set; }
        public Address Address { get; set; }
        public string Name { get; set; }
        public string GSTIN { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.UserEmail))
                validationResultList.Add(new ValidationResult("UserEmail can not be empty"));

            return validationResultList;
        }
    }
    public class CreateDeveloperProfileRequestModel : BaseModel
    {
        public string DisplayName { get; set; }
        public string ProfilePic { get; set; }
        public List<LoginField> Logins { get; set; }
        public string SecurityStamp { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        //New custom user registration module
        public string Password { get; set; }
        public IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.UserEmail) || string.IsNullOrEmpty(this.UserName))
                validationResultList.Add(new ValidationResult("UserEmail and UserName can not be empty"));

            return validationResultList;
        }
    }
    public class LoginResponse
    {
        public string Email { get; set; }
        public string ProfilePic { get; set; }
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
    }
    public class GetInvoiceRequestModel : BaseModel
    {
        public int month { get; set; }
        public int year { get; set; }
        public IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.UserEmail))
                validationResultList.Add(new ValidationResult("UserEmail can not be empty"));
            if (this.month == 0)
                this.month = DateTime.Now.Month - 1;
            if (this.year == 0)
            { 
                if (this.month != 1)
                    this.year = DateTime.Now.Year;
                else
                    this.year = DateTime.Now.Year - 1;
            }

            return validationResultList;
        }
    }

    public class GetInvoiceResponseModel
    {
        public string status { get; set; }
        public string S3Link { get; set; }
    }

}