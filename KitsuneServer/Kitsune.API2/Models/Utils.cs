using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kitsune.API2.Models
{
    #region Instamojo Helper
    public class InstamojoCustomFields
    {
    }
    public class InstamojoPaymentResponseModel
    {
        public string payment_id { get; set; }
        public int quantity { get; set; }
        public string status { get; set; }
        public object link_slug { get; set; }
        public object link_title { get; set; }
        public string buyer_name { get; set; }
        public string buyer_phone { get; set; }
        public string buyer_email { get; set; }
        public string currency { get; set; }
        public string unit_price { get; set; }
        public string amount { get; set; }
        public string fees { get; set; }
        public object shipping_address { get; set; }
        public object shipping_city { get; set; }
        public object shipping_state { get; set; }
        public object shipping_zip { get; set; }
        public object shipping_country { get; set; }
        public object discount_code { get; set; }
        public object discount_amount_off { get; set; }
        public List<object> variants { get; set; }
        public InstamojoCustomFields custom_fields { get; set; }
        public object affiliate_id { get; set; }
        public string affiliate_commission { get; set; }
        public string created_at { get; set; }
        //public double total { get; set; }
    }
    public class InstamojoPaymentRequestModel
    {
        [MongoDB.Bson.Serialization.Attributes.BsonId]
        public string id { get; set; }
        //public string dev_id { get; set; }
        public string buyer_name { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public double amount { get; set; }
        public string purpose { get; set; }
        public string status { get; set; }
        public List<InstamojoPaymentResponseModel> payments { get; set; }
        public Boolean send_sms { get; set; }
        public Boolean send_email { get; set; }
        public string sms_status { get; set; }
        public string email_status { get; set; }
        public string shorturl { get; set; }
        public string longurl { get; set; }
        public string redirect_url { get; set; }
        public string webhook { get; set; }
        public DateTime created_at { get; set; }
        public DateTime modified_at { get; set; }
        public Boolean allow_repeated_payments { get; set; }
        public string expires_at { get; set; }
    }

    public class InstamojoPaymentStatusModel
    {
        public bool success { get; set; }
        public InstamojoPaymentRequestModel payment_request { get; set; }
    }

    public class InstamojoTransactionLog
    {
        public string _id { get; set; }
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
        //public double total { get; set; }
    }

    public class InstamojoPaymentDetailsModel
    {
        public bool success { get; set; }
        public InstamojoPaymentDetailsRequestModel payment_request { get; set; }
    }
    public class InstamojoPaymentDetailsRequestModel
    {
        [MongoDB.Bson.Serialization.Attributes.BsonId]
        public string id { get; set; }
        //public string dev_id { get; set; }
        public string buyer_name { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public double amount { get; set; }
        public string purpose { get; set; }
        public string status { get; set; }
        public InstamojoPaymentResponseModel payment { get; set; }
        public Boolean send_sms { get; set; }
        public Boolean send_email { get; set; }
        public string sms_status { get; set; }
        public string email_status { get; set; }
        public string shorturl { get; set; }
        public string longurl { get; set; }
        public string redirect_url { get; set; }
        public string webhook { get; set; }
        public DateTime created_at { get; set; }
        public DateTime modified_at { get; set; }
        public Boolean allow_repeated_payments { get; set; }
        public string expires_at { get; set; }
    }

    public class InstamojoWebhookResponseModel
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
        //public double total { get; set; }
    }

    #endregion


    #region External API
    public class BillingRequestModel
    {
        public string component { get; set; }
        public string resource_id { get; set; }
        public string remarks { get; set; }
    }

    public class LiveWebsiteScreenShotRequestModel
    {
        [JsonProperty("websites")]
        public List<ScreenshotWebsiteModel> Websites { get; set; }
    }

    public class ScreenshotWebsiteModel
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
    }
    
    #endregion
}
