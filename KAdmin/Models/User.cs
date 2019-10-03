using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KitsuneAdminDashboard.Web.Models
{
    public class AuthUser
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Pwd { get; set; }

        [Required]
        public string Domain { get; set; }
    }

    public class TokenAuthUser
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string WebsiteUrl { get; set; }

        public string Source { get; set; }
    }

    public class WebisteLogin_WebsiteDetais
    {
        public string WebsiteId { get; set; }
        public string ProjectId { get; set; }
        public string WebsiteTag { get; set; }
        public string ProjectName { get; set; }
        public string WebsiteUrl { get; set; }
        public string RootPath { get; set; }
        public bool IsActive { get; set; }
        public int KitsuneProjectVersion { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }

    public class Address
    {
        public string AddressDetail { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Pin { get; set; }
    }

    public class ContactDetails
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public Address Address { get; set; }
    }

    public class WebisteLogin_UserDetais
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string AccessType { get; set; }
        public ContactDetails Contact { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastLoginTimeStamp { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class WebsiteLoginResponseModel
    {
        public string SchemaId { get; set; }
        public string EntityName { get; set; }
        public string DeveloperId { get; set; }

        public ContactDetails DeveloperContactDetails { get; set; }
        public ContactDetails SupportContactDetails { get; set; }
        public WebisteLogin_UserDetais UserDetails { get; set; }
        public WebisteLogin_WebsiteDetais WebsiteDetails { get; set; }
    }

    public class UpdateCustomerRequestModel
    {
        public string AccessType { get; set; }
        public ContactDetails ContactDetails { get; set; }
    }

    public class WebsiteUserDetails
    {
        public string DeveloperId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string WebsiteId { get; set; }
        public string AccessType { get; set; }
        public ContactDetails Contact { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastLoginTimeStamp { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string _id { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class UserPasswordUpdate
    {
        public string DeveloperId { get; set; }
        public string WebsiteId { get; set; }
        public string WebsiteUserId { get; set; }
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }

    public class UserPasswordUpdateResponse
    {
        public bool IsUpdationError { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class UserPasswordUpdateErrorResponse
    {
        public List<Error> Errors { get; set; }
        public string ErrorId { get; set; }
    }

    public class DeveloperDetails
    {
        public string Email { get; set; }
    }

    public class WebsiteDetails
    {
        public string ClientId { get; set; }
    }

    public class Error
    {
        public string [] MemberNames { get; set; }
        public string ErrorMessage { get; set; }
    }

    #region KitsuneStatusResponse

    public class KitsuneStatusResponse
    {
        public bool Success { get; set; }
        public bool IsDown { get; set; }
        public bool IsMaintenanceBreak { get; set; }
        public bool IsApiDown { get; set; }
        public KitsuneDowntimeDescription Detail { get; set; }
    }

    public class KitsuneDowntimeDescription
    {
        public string Start { get; set; }
        public string End { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    #endregion

    public class CustomerDetails
    {
        public string CustomerName { get; set; }
        public string WebsiteUrl { get; set; }
    }
}
