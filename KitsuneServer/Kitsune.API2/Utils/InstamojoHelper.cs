
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.Models;
//using Kitsune.Helper;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
//using System.Web.Script.Serialization;
using Kitsune.API2.DataHandlers.Mongo;
using Kitsune.Models;
using Kitsune.API2.EnvConstants;
using System.Web;

namespace Kitsune.API2.Utils
{
    public class InstamojoHelper
    {
        private static IMongoClient _server;
        private static IMongoDatabase _kitsuneDB;
        
        private static string connectionString = EnvConstants.Constants.ConvertToKitsuneDbUrl;
        private static string dbName = EnvConstants.Constants.ConvertToKitsuneDatabaseName;


        string emailId = null;
        string API_KEY = EnvConstants.Constants.InstaMojoAPIKey;
        string AUTH_TOKEN = EnvConstants.Constants.InstaMojoAPIToken;
        
        internal static void InitiateConnection()
        {
            try
            {
                if (_server == null)
                {
                    _server = new MongoClient(connectionString);
                    _kitsuneDB = _server.GetDatabase(dbName);
                }
            }
            catch (Exception ex)
            {

            }
        }
        
        public Uri CreatePaymentRequest(string emailId, double amount, string redirectUrl)
        {
            if (_server == null)
            {
                InitiateConnection();
            }
            this.emailId = emailId;
            UserModel developer_model = new UserModel();
            try
            {
                if (amount < 10)
                {
                    throw new Exception("amount too low, it must be atleast Rs 10");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new Exception(e.Message);
            }
            try
            {
                var collection = _kitsuneDB.GetCollection<BsonDocument>("users");
                var filter = Builders<BsonDocument>.Filter.Eq("Email", emailId);
                if (collection.Find(filter).Count() != 1)
                {
                    throw new Exception("dev_id doesn't exist");
                }
                BsonDocument doc = collection.Find(filter).First();
                developer_model = BsonSerializer.Deserialize<UserModel>(doc);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw new Exception("Unable to fetch user details");
            }

            string payment_url = null;
            
            string url = EnvConstants.Constants.InstaMojoAPIUrl;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Headers["X-Api-Key"] = API_KEY;
            request.Headers["X-Auth-Token"] = AUTH_TOKEN;
            request.ContentType = "application/x-www-form-urlencoded";
            string postData = $"amount={amount.ToString()}" +
                              $"&purpose=Kitsune Recharge" +
                              $"&buyer_name={developer_model.DisplayName}" +
                              $"&email={developer_model.Email}" +
                              $"&send_email=True" +
                              $"&redirect_url={redirectUrl}" +
                              $"&webhook={EnvConstants.Constants.InstamojoWebhook}";
            if (developer_model.PhoneNumber != null && developer_model.PhoneNumber.Length != 0)
            {
                postData += $"&phone={developer_model.PhoneNumber}";
            }
            byte[] bytes = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = bytes.Length;
            //try
            //{
            //    if (developer_model.PhoneNumber == null || developer_model.PhoneNumber.Length == 0)
            //    {
            //        throw new Exception("Phone number cannot be empty");
            //    }
            //    else
            //    {
            //        request.AddParameter("phone", developer_model.PhoneNumber);
            //    }
            //}
            //catch (Exception e)
            //{
            //    throw new Exception(e.Message);
            //}
            

            #region Payment request to get the URL
            try
            {
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);

                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);

                var result = reader.ReadToEnd();
                JObject responseJson = JObject.Parse(result);

                InstamojoPaymentStatusModel responseModel = new InstamojoPaymentStatusModel();
                try
                {
                    responseModel = JsonConvert.DeserializeObject<InstamojoPaymentStatusModel>(responseJson.ToString());
                    if (responseModel.success == true)
                    {
                        payment_url = responseModel.payment_request.longurl;

                        PaymentTransactionLog paymentTransactionLog = new PaymentTransactionLog()
                        {
                            _id = ObjectId.GenerateNewId().ToString(),
                            PaymentRequestId = responseModel.payment_request.id,
                            Status = null,
                            Amount = responseModel.payment_request.amount,
                            UserProfileId = emailId,
                            InvoiceId = "KINV-" + GenerateInvoiceId(),
                            CreatedOn = DateTime.Now,
                            UpdatedOn = DateTime.Now
                        };

                        var json = JsonConvert.SerializeObject(paymentTransactionLog);
                        var document = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(json);
                        var collection = _kitsuneDB.GetCollection<BsonDocument>("PaymentTransactionLogs");
                        collection.InsertOneAsync(document).Wait();

                        InstamojoTransactionLog instamojoTransactionLog = new InstamojoTransactionLog()
                        {
                            _id = ObjectId.GenerateNewId().ToString(),
                            payment_request_id = responseModel.payment_request.id,
                            longurl = responseModel.payment_request.longurl,
                            buyer = responseModel.payment_request.buyer_name,
                            buyer_name = responseModel.payment_request.buyer_name,
                            buyer_phone = responseModel.payment_request.phone,
                            purpose = responseModel.payment_request.purpose,
                            amount = responseModel.payment_request.amount.ToString(),
                            shorturl = responseModel.payment_request.shorturl,
                            currency = null,
                            status = null,
                            fees = null,
                            mac = null,
                            payment_id = null
                        };
                        
                        var collection2 = _kitsuneDB.GetCollection<InstamojoTransactionLog>("InstamojoTransactionLogs");
                        collection2.InsertOneAsync(instamojoTransactionLog).Wait();
                    }
                    else
                    {
                        //Error
                        throw new Exception("error : " + responseModel.payment_request);
                    }
                }
                catch (Exception e)
                {
                    //CRITICAL ERROR : FAILED TO GET PAYMENT REQUEST
                    throw new Exception("CRITICAL ERROR " + e.Message);
                }
            }
            catch (Exception e)
            {
                //CONNECTION FAILED
                throw new Exception("Error while sending payment Url :" + e.Message);
            }
            #endregion
            try
            {
                Uri uri = new Uri(payment_url);
                return uri;
            }
            catch (Exception ex)
            {
                throw new Exception("Error While creating Uri from url: " + payment_url);
            }
        }

        /// <summary>
        /// Webhook update
        /// </summary>
        /// <param name="instaMojoHookResponse"></param>
        public void UpdateWebHook(InstamojoWebhookRequestModel instaMojoHookResponse)
        {
            try
            {
                if (_server == null)
                {
                    InitiateConnection();
                }

                //update the InstamojoTransactionLog database
                var collection = _kitsuneDB.GetCollection<InstamojoTransactionLog>("InstamojoTransactionLogs");
                var udb = new UpdateDefinitionBuilder<InstamojoTransactionLog>();
                var update = udb.Set(x => x.status, instaMojoHookResponse.status)
                              .Set(x => x.fees, instaMojoHookResponse.fees)
                              .Set(x => x.mac, instaMojoHookResponse.mac)
                              .Set(x => x.currency, instaMojoHookResponse.currency)
                              .Set(x => x.shorturl, instaMojoHookResponse.shorturl)
                              .Set(x => x.payment_id, instaMojoHookResponse.payment_id);
                var result=collection.UpdateOne(x => x.payment_request_id == instaMojoHookResponse.payment_request_id, update, new UpdateOptions() { IsUpsert = true });

                string walletBalance = null;


                var userCollection = _kitsuneDB.GetCollection<UserModel>("users");
                var pdb = new ProjectionDefinitionBuilder<UserModel>();
                var pd = pdb.Include(x => x.Wallet).Include(x => x.Email).Include(x => x.UserName);
                //get user details
                var user = userCollection.Find(x => x.UserName == instaMojoHookResponse.buyer).Project<UserModel>(pd).FirstOrDefaultAsync().Result;

                //if successfully credited then update the wallet
                if (instaMojoHookResponse.status.ToLower() == "credit")
                {
                    //update the wallet
                    if (user != null)
                    {
                        var builder = new UpdateDefinitionBuilder<UserModel>();
                        var currentTime = DateTime.Now;
                        double amount = Double.Parse(instaMojoHookResponse.amount);

                        //if wallet is not present then create one
                        if (user.Wallet != null)
                        {
                            user.Wallet.Balance += amount;
                            user.Wallet.UpdatedOn = currentTime;
                        }
                        else if (user.Wallet == null)
                            user.Wallet = new Kitsune.Models.Wallet { Balance = amount, UpdatedOn = DateTime.Now };

                        userCollection.UpdateOne((x => x.UserName == instaMojoHookResponse.buyer), builder.Set(x => x.Wallet, user.Wallet));
                        walletBalance = user.Wallet.Balance.ToString();

                        //wallet stats
                        var walletCollection = _kitsuneDB.GetCollection<WalletStats>(EnvConstants.Constants.WalletStats);
                        WalletStats walletStats = new WalletStats();
                        walletStats.Amount = amount;
                        walletStats.IsAdded = true;
                        walletStats.Reason = "Money Added";
                        walletStats.UserEmail = user.Email;
                        walletStats.CreatedOn = DateTime.Now;
                        walletCollection.InsertOneAsync(walletStats);

                        //KitsuneConversionAPI kitsuneApi = new KitsuneConversionAPI();
                        EmailHelper emailHelper = new EmailHelper();
                        Dictionary<string, string> optionalParameters = new Dictionary<string, string>();
                        optionalParameters.Add(EnvConstants.Constants.EmailParam_AmountAdded, walletStats.Amount.ToString());
                        optionalParameters.Add(EnvConstants.Constants.EmailParam_WalletAmount, (user.Wallet.Balance - user.Wallet.UnbilledUsage).ToString());
                        optionalParameters.Add(EnvConstants.Constants.EmailParam_PaymentId, instaMojoHookResponse.payment_id);
                        optionalParameters.Add(EnvConstants.Constants.EmailParam_PaymentPartyName, instaMojoHookResponse.buyer_name);
                        //SendKitsuneConvertEmailRequestModel emailcommand = new SendKitsuneConvertEmailRequestModel() { EmailID = user.Email, UserName = user.UserName, MailType = MailType.Payment_Success, Attachments = null, OptionalParams = optionalParameters };
                        //kitsuneApi.SendKitsuneConvertEmail(emailCommand);
                        emailHelper.SendGetKitsuneEmail(user.Email, 
                                                        user.UserName,
                                                        MailType.PAYMENT_SUCCESS,
                                                        null,
                                                        optionalParameters);
                    }
                    else
                    {
                        throw new Exception("Couldn't fetch user details");
                    }
                }
                else
                {
                    //KitsuneConversionAPI kitsuneApi = new KitsuneConversionAPI();
                    EmailHelper emailHelper = new EmailHelper();
                    Dictionary<string, string> optionalParameters = new Dictionary<string, string>();
                    optionalParameters.Add(EnvConstants.Constants.EmailParam_PaymentId, instaMojoHookResponse.payment_id);
                    //SDK.Models.SendKitsuneConvertEmailCommand emailCommand = new SDK.Models.SendKitsuneConvertEmailCommand() { EmailID = user.Email, UserName = user.UserName, MailType = SDK.Models.MailType.Payment_Error, Attachments = null, OptionalParams = optionalParameters };
                    //kitsuneApi.SendKitsuneConvertEmail(emailCommand);
                    emailHelper.SendGetKitsuneEmail(user.Email,
                                                    user.UserName,
                                                    MailType.PAYMENT_ERROR,
                                                    null,
                                                    optionalParameters);
                }
                //update the PaymentTransactionLog database
                var mojoLogCollection = _kitsuneDB.GetCollection<PaymentTransactionLog>("PaymentTransactionLogs");
                var mojoUdb = new UpdateDefinitionBuilder<PaymentTransactionLog>();
                var mojoUpdate = mojoUdb.Set(x => x.Status, instaMojoHookResponse.status)
                    .Set(x => x.UpdatedOn, DateTime.Now);
                mojoLogCollection.UpdateOne(x => x.PaymentRequestId == instaMojoHookResponse.payment_request_id, mojoUpdate, new UpdateOptions() { IsUpsert = true });
            }
            catch (Exception ex)
            {
                //save the log
            }
        }

        public static string CheckPaymentStatus(string payment_request_id, string payment_id)
        {
            try
            {
                if (_server == null)
                {
                    InitiateConnection();
                }
                var userCollection = _kitsuneDB.GetCollection<InstamojoTransactionLog>("WebHookDetails");
                var paymentDetail = userCollection.Find(x => x.payment_id == payment_id && x.payment_request_id == payment_request_id).FirstOrDefaultAsync().Result;
                if (paymentDetail == null)
                {
                    return "pending";
                }
                if (paymentDetail.status.ToLower().Equals("credit"))
                {
                    return "success";
                }
                else
                {
                    return "failed";
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string CheckInstaMojoPaymentStatus(string payment_request_id, string payment_id)
        {
            try
            {
                if (_server == null)
                {
                    InitiateConnection();
                }

                string url = EnvConstants.Constants.InstaMojoAPIUrl + payment_request_id + "/" + payment_id;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.Headers["X-Api-Key"] = EnvConstants.Constants.InstaMojoAPIKey;
                request.Headers["X-Auth-Token"] = EnvConstants.Constants.InstaMojoAPIToken;
                
                try
                {
                    //send instamojo request
                    WebResponse response = request.GetResponse();
                    Stream stream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(stream);

                    var result = reader.ReadToEnd();
                    JObject responseJson = JObject.Parse(result);
                    InstamojoPaymentDetailsModel model = new InstamojoPaymentDetailsModel();
                    try
                    {
                        model = JsonConvert.DeserializeObject<InstamojoPaymentDetailsModel>(responseJson.ToString());
                        if (model.success)
                        {
                            if (model.payment_request.status.ToLower().Equals("completed"))
                            {
                                if (model.payment_request.payment.status.ToLower().Equals("credit"))
                                {
                                    return "success";
                                }
                                else
                                {
                                    return "failed";
                                }
                            }
                            else
                            {
                                return "pending";
                            }
                        }
                        else
                        {
                            //Error
                            return "failed";
                        }
                    }
                    catch (Exception e)
                    {
                        //CRITICAL ERROR : FAILED TO GET PAYMENT REQUEST
                        throw new Exception("CRITICAL ERROR " + e.Message);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        internal static string GenerateInvoiceId()
        {
            string dateId = DateTime.Now.ToString("yyMMddhhmmssff");
            string random = string.Concat(dateId, new Random().Next(1000, 2000).ToString());
            return random;

        }

        public void UpdateRequestStatus(Stream data)
        {
            #region ConvertToKitsuneWebhook
            try
            {
                #region deserialize web hook data
                StreamReader reader = new StreamReader(data);
                var dict = HttpUtility.ParseQueryString(reader.ReadToEnd());
                var json = JsonConvert.SerializeObject(dict.Keys.Cast<string>()
                                                         .ToDictionary(k => k, k => dict[k]), Formatting.Indented);
                var instaMojoHookResponse = JsonConvert.DeserializeObject<InstamojoWebhookResponseModel>(json);
                #endregion

                #region GetPaymentRequestAgain

                string url = EnvConstants.Constants.InstaMojoAPIUrl + instaMojoHookResponse.payment_request_id + "/" + instaMojoHookResponse.payment_id;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.Headers["X-Api-Key"] = API_KEY;
                request.Headers["X-Auth-Token"] = AUTH_TOKEN;
                
                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader reader1 = new StreamReader(stream);

                var result = reader1.ReadToEnd();
                JObject paymentResponseJson = JObject.Parse(result);

                #endregion

                IMongoClient mongoClient = new MongoClient(connectionString);
                // change db name
                IMongoDatabase eDB = mongoClient.GetDatabase(EnvConstants.Constants.ConvertToKitsuneDatabaseName);
                // get colleciton name dynamically
                var collection = eDB.GetCollection<BsonDocument>("PaymentDetails");
                FilterDefinition<BsonDocument> filter;
                if (!String.IsNullOrEmpty(instaMojoHookResponse.payment_request_id))
                {
                    filter = Builders<BsonDocument>.Filter.Eq("payment_request.id", instaMojoHookResponse.payment_request_id);
                }
                else
                {
                    return;
                }
                InstamojoPaymentStatusModel paymentStatusModel = new InstamojoPaymentStatusModel();
                paymentStatusModel = JsonConvert.DeserializeObject<InstamojoPaymentStatusModel>(paymentResponseJson.ToString());

                if (paymentStatusModel.success)
                {
                    try
                    {
                        //call wallet api to add funds
                        string walletApiUrl = EnvConstants.Constants.WalletUpdateUrl + "?useremail=" + paymentStatusModel.payment_request.email + "&amount=" + instaMojoHookResponse.amount + "&add=true";
                        request = (HttpWebRequest)WebRequest.Create(url);
                        request.Method = "POST";
                        request.Headers["X-Api-Key"] = API_KEY;
                        request.Headers["X-Auth-Token"] = AUTH_TOKEN;

                        var walletResponse =(HttpWebResponse) request.GetResponse();
                        
                        if (walletResponse.StatusCode != HttpStatusCode.OK)
                        {
                            throw new Exception("Wallet update failed with status code : " + walletResponse.StatusCode);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

                var InstamojoPaymentRequestJson = JsonConvert.SerializeObject(paymentStatusModel.payment_request);
                var paymentDetailsDocument = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(InstamojoPaymentRequestJson);
                try
                {
                    //insert payment details
                    collection.InsertOne(paymentDetailsDocument);
                }
                catch (Exception e)
                {
                    //CRITICAL ERROR, FAILED TO UPDATE DOCUMENT
                    //Console.WriteLine(e);
                }
                #endregion
                //Console.WriteLine("done");
            }
            catch (Exception e)
            {
                //CATCHES CONNECTION,DATABASE LOGIN EXCEPTIONS
                //Console.WriteLine(e);
            }
        }
    }
}