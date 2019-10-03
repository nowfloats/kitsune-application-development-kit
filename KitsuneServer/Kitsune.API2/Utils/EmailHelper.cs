using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.EnvConstants;
using Kitsune.API2.Models;
using Kitsune.BasePlugin.Utils;
using Kitsune.Models;
//using Kitsune.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
namespace Kitsune.API2.Utils
{
    public class EmailHelper
    {

        public string SendGetKitsuneEmail(string UserName, string EmailID, MailType MailType, List<string> Attachments, Dictionary<string, string> OptionalParams, string clientId = null)
        {

            if (UserName != null && EmailID != null && (MailType >= MailType.CONVERSION_INSTANTIATED && MailType <= MailType.CUSTOMER_ENQUIRY))
            {

                String subject = "", emailBody = "";
                var emailUserConfigType = EmailUserConfigType.WEBSITE_DEVELOPER;

                if (MailType == MailType.CONVERSION_INSTANTIATED)
                {
                    subject = "Kitsune: Conversion commenced";
                    emailBody = "Great!<br>You’re on your way to get a smarter and faster website.<br/>We need your help with the next step, so please keep your eye on the dialog box.<br/> We’ll take over after that I promise The process might take a while, but don’t worry, we’ll drop you an e - mail once we’re done.<br>Regards,<br> Team Kitsune";
                }

                else if (MailType == MailType.CONVERSION_SUCCESS)
                {
                    subject = "Kitsune: Conversion complete";
                    emailBody = "Congratulations! <br/>Your Kitsune enabled site preview is now ready! Click here to preview. <br/>Check out the footer to see the keywords we’ve extracted from your site. These keywords will help improve the<br/>search engine rankings for your site, which means more visitors for you 	<br/>Regards, <br/>Team Kitsune";
                }

                else if (MailType == MailType.CONVERSION_ERROR)
                {
                    subject = "Kitsune: Conversion failed";
                    emailBody = String.Format("CrawldId : {0}", OptionalParams[EnvConstants.Constants.EmailParam_CrawlId]);
                }

                else if (MailType == MailType.WEBSITE_ACTIVATION)
                {
                    subject = "Kitsune: Website activation successful";
                    emailBody = "Hi, <br/>Congratulations! Your Kitsune powered website is now live.<br/>If you already have a domain name that you want to use with this website, please drop us a message on assist@getkitsune.com and we’ll help you set that up.<br>Regards, <br/>Team Kitsune";
                }

                else if (MailType == MailType.BALANACE_LOW_1)
                {
                    subject = "Kitsune: Low Balance Reminder";
                    emailBody = "Hi, <br/>Your account balance is less than Rs.200 .<br/>Please add funds to your to ensure uninterrupted service <br/>Regards,<br/>Team Kitsune";
                }

                else if (MailType == MailType.BALANCE_LOW_2)
                {
                    subject = "Low Balance Reminder (URGENT)";
                    emailBody = "Hi, <br/>Your account balance is less than Rs.100. <br/>Please add funds to your to ensure uninterrupted service <br/>Regards, <br/>Team Kitsune";
                }

                else if (MailType == MailType.BALANACE_EMPTY)
                {
                    subject = "Kitsune: Out of Funds!";
                    emailBody = "Hi, <br/>Your account is out of funds.<br/>Failure to add funds will lead to deactivation of services for your sites. <br/>Regards, <br/>Team Kitsune";
                }

                else if (MailType == MailType.PAYMENT_INSTANTIATED)
                {
                    subject = "Kitsune: Payment Processing Started";
                    emailBody = "Great!<br>The Payment process has started.<br>We’ll drop you an e - mail once its done.<br>Regards,<br> Team Kitsune";
                }

                else if (MailType == MailType.PAYMENT_SUCCESS)
                {
                    subject = "Kitsune: Advance Receipt Acknowledgement";
                    emailBody = String.Format("Transaction ID:  {0}<br><br>Hi {1},<br> Please consider this as an acknowledgment for the addition of INR {2} for your Kitsune website advance account.<br>Your updated account balance is INR {3}.<br>Regards,<br> Team Kitsune ", OptionalParams[EnvConstants.Constants.EmailParam_PaymentId], OptionalParams[EnvConstants.Constants.EmailParam_PaymentPartyName], OptionalParams[EnvConstants.Constants.EmailParam_AmountAdded], OptionalParams[EnvConstants.Constants.EmailParam_WalletAmount]);
                }

                else if (MailType == MailType.PAYMENT_ERROR)
                {
                    subject = "Kitsune: Payment Incomplete";
                    emailBody = String.Format("Transaction ID:  {0}<br><br>Hi, <br/>I’m sorry but it seems something went wrong while the payment was underway. Please contact us at assist@getkitsune.com and we’d be happy to help you out. <br/>Regards, <br/>Team Kitsune", OptionalParams[EnvConstants.Constants.EmailParam_PaymentId]);
                }

                else if (MailType == MailType.PAYMENT_INVOICE)
                {
                    subject = "Kitsune: Monthly Invoice";
                    emailBody = "Hi, <br/>Please find attached the monthly invoice for your Kitsune powered website.<br/>If you have any questions about this, please message us on assist@getkitsune.com.. <br/>Regards, <br/>Team Kitsune";
                }

                else if (MailType == MailType.DEFAULT_CUSTOMER_KADMIN_CREDENTIALS)
                {
                    subject = $"K-Admin credentials for your project - {OptionalParams[EnvConstants.Constants.EmailParam_ProjectName]}";
                    //string fileContent = new WebClient().DownloadString("https://s3-ap-southeast-1.amazonaws.com/kitsune-content-cdn/default-customer.html");
                    string fileContent = " <!doctype html> <html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:o=\"urn:schemas-microsoft-com:office:office\"> <head> <title> </title> <!--[if !mso]><!-- --> <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"> <!--<![endif]--> <meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\"> <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"> <style type=\"text/css\"> #outlook a { padding:0; } .ReadMsgBody { width:100%; } .ExternalClass { width:100%; } .ExternalClass * { line-height:100%; } body { margin:0;padding:0;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%; } table, td { border-collapse:collapse;mso-table-lspace:0pt;mso-table-rspace:0pt; } img { border:0;height:auto;line-height:100%; outline:none;text-decoration:none;-ms-interpolation-mode:bicubic; } p { display:block;margin:13px 0; } </style> <!--[if !mso]><!--> <style type=\"text/css\"> @media only screen and (max-width:480px) { @-ms-viewport { width:320px; } @viewport { width:320px; } } </style> <!--<![endif]--> <!--[if mso]> <xml> <o:OfficeDocumentSettings> <o:AllowPNG/> <o:PixelsPerInch>96</o:PixelsPerInch> </o:OfficeDocumentSettings> </xml> <![endif]--> <!--[if lte mso 11]> <style type=\"text/css\"> .outlook-group-fix { width:100% !important; } </style> <![endif]--> <!--[if !mso]><!--> <link href=\"https://fonts.googleapis.com/css?family=Ubuntu:300,400,500,700\" rel=\"stylesheet\" type=\"text/css\"> <style type=\"text/css\"> @import url(https://fonts.googleapis.com/css?family=Ubuntu:300,400,500,700); </style> <!--<![endif]--> <style type=\"text/css\"> @media only screen and (min-width:480px) { .mj-column-per-100 { width:100% !important; } } </style> <style type=\"text/css\"> </style> </head> <body style=\"background-color:#CCC;\"> <div style=\"background-color:#CCC;\" > <!-- Header <mj-section width=\"100%25\" padding-bottom=\"0\" horizontal-spacing=\"0\" vertical-spacing=\"0\" padding-top=\"0\" padding-bottom=\"0\" padding-left=\"0\" padding-right=\"0\"> <mj-column padding-bottom=\"0\" horizontal-spacing=\"0\" vertical-spacing=\"0\" padding-top=\"0\" padding-bottom=\"0\" padding-left=\"0\" padding-right=\"0\" width=\"100%25\"> <mj-image padding-bottom=\"0\" horizontal-spacing=\"0\" vertical-spacing=\"0\" padding-top=\"0\" padding-bottom=\"0\" padding-left=\"0\" padding-right=\"0\" src=\"http%3A%2F%2Fcdn.kitsune.tools%2Femail-assets%2Fheader.png\"></mj-image> </mj-column> </mj-section> --><!-- Header --> <!--[if mso | IE]> <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"width:600px;\" width=\"600\" > <tr> <td style=\"line-height:0px;font-size:0px;mso-line-height-rule:exactly;\"> <![endif]--> <div style=\"background:white;background-color:white;Margin:0px auto;max-width:600px;\"> <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"background:white;background-color:white;width:100%;\" > <tbody> <tr> <td style=\"direction:ltr;font-size:0px;padding:20px 0;padding-bottom:0px;padding-left:0px;padding-right:0px;padding-top:0;text-align:center;vertical-align:top;\" > <!--[if mso | IE]> <table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"> <tr> <td style=\"vertical-align:bottom;width:600px;\" > <![endif]--> <div class=\"mj-column-per-100 outlook-group-fix\" style=\"font-size:13px;text-align:left;direction:ltr;display:inline-block;vertical-align:bottom;width:100%;\" > <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"vertical-align:bottom;\" width=\"100%\" > <tr> <td style=\"font-size:0px;padding:10px 25px;padding-top:0;padding-right:0px;padding-bottom:0px;padding-left:0px;word-break:break-word;\" > <p style=\"border-top:solid 3px #f06428;font-size:1;margin:0px auto;width:100%;\" > </p> <!--[if mso | IE]> <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-top:solid 3px #f06428;font-size:1;margin:0px auto;width:600px;\" role=\"presentation\" width=\"600px\" > <tr> <td style=\"height:0;line-height:0;\"> &nbsp; </td> </tr> </table> <![endif]--> </td> </tr> </table> </div> <!--[if mso | IE]> </td> </tr> </table> <![endif]--> </td> </tr> </tbody> </table> </div> <!--[if mso | IE]> </td> </tr> </table> <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"width:600px;\" width=\"600\" > <tr> <td style=\"line-height:0px;font-size:0px;mso-line-height-rule:exactly;\"> <![endif]--> <div style=\"background:white;background-color:white;Margin:0px auto;max-width:600px;\"> <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"background:white;background-color:white;width:100%;\" > <tbody> <tr> <td style=\"direction:ltr;font-size:0px;padding:20px 0;padding-bottom:0px;padding-top:40;text-align:center;vertical-align:top;\" > <!--[if mso | IE]> <table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"> <tr> <td style=\"vertical-align:bottom;width:600px;\" > <![endif]--> <div class=\"mj-column-per-100 outlook-group-fix\" style=\"font-size:13px;text-align:left;direction:ltr;display:inline-block;vertical-align:bottom;width:100%;\" > <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"vertical-align:bottom;\" width=\"100%\" > <tr> <td align=\"left\" style=\"font-size:0px;padding:10px 25px;word-break:break-word;\" > <div style=\"font-family:Ubuntu, Helvetica, Arial, sans-serif;font-size:20px;font-weight:400;line-height:1;text-align:left;color:#f06428;\" > hello {4}, </div> </td> </tr> <tr> <td align=\"left\" style=\"font-size:0px;padding:10px 25px;word-break:break-word;\" > <div style=\"font-family:Ubuntu, Helvetica, Arial, sans-serif;font-size:15px;line-height:1.5;text-align:left;color:#040707;\" > great, you've just created a project <b>{0}</b>. we're very excited to have you on board. </div> </td> </tr> <tr> <td align=\"left\" style=\"font-size:0px;padding:10px 25px;word-break:break-word;\" > <div style=\"font-family:Ubuntu, Helvetica, Arial, sans-serif;font-size:15px;line-height:1.5;text-align:left;color:#040707;\" > you can now manage your website content here: </div> </td> </tr> <tr> <td align=\"left\" style=\"font-size:0px;padding:10px 25px;padding-top:0;word-break:break-word;\" > <div style=\"font-family:Ubuntu, Helvetica, Arial, sans-serif;font-size:15px;line-height:1.5;text-align:left;color:#040707;\" > login url: <span color=\"#f06428\">{1}</span> </div> </td> </tr> <tr> <td align=\"left\" style=\"font-size:0px;padding:10px 25px;padding-top:0;word-break:break-word;\" > <div style=\"font-family:Ubuntu, Helvetica, Arial, sans-serif;font-size:15px;line-height:1.5;text-align:left;color:#040707;\" > username: <span color=\"#f06428\">{2}</span><br> password: <span color=\"#f06428\">{3}</span> </div> </td> </tr> </table> </div> <!--[if mso | IE]> </td> </tr> </table> <![endif]--> </td> </tr> </tbody> </table> </div> <!--[if mso | IE]> </td> </tr> </table> <![endif]--> <!-- Footer --> <!--[if mso | IE]> <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"width:600px;\" width=\"600\" > <tr> <td style=\"line-height:0px;font-size:0px;mso-line-height-rule:exactly;\"> <![endif]--> <div style=\"background:white;background-color:white;Margin:0px auto;max-width:600px;\"> <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"background:white;background-color:white;width:100%;\" > <tbody> <tr> <td style=\"direction:ltr;font-size:0px;padding:20px 0;padding-bottom:0px;padding-top:10px;text-align:center;vertical-align:top;\" > <!--[if mso | IE]> <table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"> <tr> <td style=\"vertical-align:bottom;width:600px;\" > <![endif]--> <div class=\"mj-column-per-100 outlook-group-fix\" style=\"font-size:13px;text-align:left;direction:ltr;display:inline-block;vertical-align:bottom;width:100%;\" > <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"vertical-align:bottom;\" width=\"100%\" > <tr> <td align=\"left\" style=\"font-size:0px;padding:10px 25px;word-break:break-word;\" > <div style=\"font-family:Ubuntu, Helvetica, Arial, sans-serif;font-size:15px;line-height:1.5;text-align:left;color:#040707;\" > explore our documentation <a style=\"color:#f06428;\" href=\"http://docs.kitsune.tools\">here</a>. </div> </td> </tr> <tr> <td align=\"left\" style=\"font-size:0px;padding:10px 25px;word-break:break-word;\" > <div style=\"font-family:Ubuntu, Helvetica, Arial, sans-serif;font-size:15px;line-height:1.5;text-align:left;color:#040707;\" > in case of any queries, please email us at <a style=\"color:#f06428;\" href=\"mailto:support@getkitsune.com\">support@getkitsune.com</a>. </div> </td> </tr> <tr> <td align=\"left\" style=\"font-size:0px;padding:10px 25px;word-break:break-word;\" > <div style=\"font-family:Ubuntu, Helvetica, Arial, sans-serif;font-size:15px;line-height:1.5;text-align:left;color:#040707;\" > thanks, <br> team kitsune </div> </td> </tr> <tr> <td style=\"font-size:0px;padding:10px 25px;padding-top:20;padding-right:0px;padding-bottom:0px;padding-left:0px;word-break:break-word;\" > <p style=\"border-top:solid 1px #ffbb9d;font-size:1;margin:0px auto;width:100%;\" > </p> <!--[if mso | IE]> <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-top:solid 1px #ffbb9d;font-size:1;margin:0px auto;width:600px;\" role=\"presentation\" width=\"600px\" > <tr> <td style=\"height:0;line-height:0;\"> &nbsp; </td> </tr> </table> <![endif]--> </td> </tr> <tr> <td align=\"center\" style=\"font-size:0px;padding:10px 25px;word-break:break-word;\" > <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"border-collapse:collapse;border-spacing:0px;\" > <tbody> <tr> <td style=\"width:150px;\"> <img height=\"auto\" src=\"http://www.getkitsune.com/Images/logo.png?v=9\" style=\"border:0;display:block;outline:none;text-decoration:none;width:100%;\" width=\"150\" /> </td> </tr> </tbody> </table> </td> </tr> <tr> <td style=\"font-size:0px;padding:10px 25px;padding-top:20;padding-right:0px;padding-bottom:0px;padding-left:0px;word-break:break-word;\" > <p style=\"border-top:solid 3px #f06428;font-size:1;margin:0px auto;width:100%;\" > </p> <!--[if mso | IE]> <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-top:solid 3px #f06428;font-size:1;margin:0px auto;width:600px;\" role=\"presentation\" width=\"600px\" > <tr> <td style=\"height:0;line-height:0;\"> &nbsp; </td> </tr> </table> <![endif]--> </td> </tr> </table> </div> <!--[if mso | IE]> </td> </tr> </table> <![endif]--> </td> </tr> </tbody> </table> </div> <!--[if mso | IE]> </td> </tr> </table> <![endif]--> </div> </body> </html> ";

                    emailBody = fileContent.Replace("{0}", OptionalParams[EnvConstants.Constants.EmailParam_ProjectName])
                        .Replace("{1}", OptionalParams[EnvConstants.Constants.EmailParam_KAdminUrl])
                        .Replace("{2}", OptionalParams[EnvConstants.Constants.EmailParam_KAdminUserName])
                        .Replace("{3}", OptionalParams[EnvConstants.Constants.EmailParam_KAdminPassword])
                        .Replace("{4}", OptionalParams[EnvConstants.Constants.EmailParam_DeveloperName]);
                }

                else if (MailType == MailType.CUSTOMER_KADMIN_CREDENTIALS)
                {
                    subject = string.Format("your website is now yours to control");
                    string fileContent = "<!doctype html><html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:o=\"urn:schemas-microsoft-com:office:office\"><head>    <title> </title>    <!--[if !mso]><!-- -->    <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">    <!--<![endif]-->    <meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\">    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">    <style type=\"text/css\">        #outlook a {            padding: 0;        }        .ReadMsgBody {            width: 100%;        }        .ExternalClass {            width: 100%;        }        .ExternalClass * {            line-height: 100%;        }        body {            margin: 0;            padding: 0;            -webkit-text-size-adjust: 100%;            -ms-text-size-adjust: 100%;        }        table,        td {            border-collapse: collapse;            mso-table-lspace: 0pt;            mso-table-rspace: 0pt;        }        img {            border: 0;            height: auto;            line-height: 100%;            outline: none;            text-decoration: none;            -ms-interpolation-mode: bicubic;        }        p {            display: block;            margin: 13px 0;        }    </style>    <!--[if !mso]><!-->    <style type=\"text/css\">        @media only screen and (max-width:480px) {            @-ms-viewport {                width: 320px;            }            @viewport {                width: 320px;            }        }    </style>    <!--<![endif]-->    <!--[if mso]> <xml> <o:OfficeDocumentSettings> <o:AllowPNG/> <o:PixelsPerInch>96</o:PixelsPerInch> </o:OfficeDocumentSettings> </xml> <![endif]-->    <!--[if lte mso 11]> <style type=\"text/css\"> .outlook-group-fix { width:100% !important; } </style> <![endif]-->    <!--[if !mso]><!-->    <link href=\"https://fonts.googleapis.com/css?family=Ubuntu:300,400,500,700\" rel=\"stylesheet\" type=\"text/css\">    <style type=\"text/css\">        @import url(https://fonts.googleapis.com/css?family=Ubuntu:300,400,500,700);    </style>    <!--<![endif]-->    <style type=\"text/css\">        @media only screen and (min-width:480px) {            .mj-column-per-100 {                width: 100% !important;            }        }    </style>    <style type=\"text/css\">    </style></head><body style=\"background-color:#CCC;\">    <div style=\"background-color:#CCC;\">        <!-- Header <mj-section width=\"100%25\" padding-bottom=\"0\" horizontal-spacing=\"0\" vertical-spacing=\"0\" padding-top=\"0\" padding-bottom=\"0\" padding-left=\"0\" padding-right=\"0\"> <mj-column padding-bottom=\"0\" horizontal-spacing=\"0\" vertical-spacing=\"0\" padding-top=\"0\" padding-bottom=\"0\" padding-left=\"0\" padding-right=\"0\" width=\"100%25\"> <mj-image padding-bottom=\"0\" horizontal-spacing=\"0\" vertical-spacing=\"0\" padding-top=\"0\" padding-bottom=\"0\" padding-left=\"0\" padding-right=\"0\" src=\"http%3A%2F%2Fcdn.kitsune.tools%2Femail-assets%2Fheader.png\"></mj-image> </mj-column> </mj-section> -->        <!-- Header -->        <!--[if mso | IE]> <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"width:600px;\" width=\"600\" > <tr> <td style=\"line-height:0px;font-size:0px;mso-line-height-rule:exactly;\"> <![endif]-->        <div style=\"background:white;background-color:white;Margin:0px auto;max-width:600px;\">            <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"background:white;background-color:white;width:100%;\">                <tbody>                    <tr>                        <td style=\"direction:ltr;font-size:0px;padding:20px 0;padding-bottom:0px;padding-left:0px;padding-right:0px;padding-top:0;text-align:center;vertical-align:top;\">                            <!--[if mso | IE]> <table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"> <tr> <td style=\"vertical-align:bottom;width:600px;\" > <![endif]-->                            <div class=\"mj-column-per-100 outlook-group-fix\" style=\"font-size:13px;text-align:left;direction:ltr;display:inline-block;vertical-align:bottom;width:100%;\">                                <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"vertical-align:bottom;\"                                    width=\"100%\">                                    <tr>                                        <td style=\"font-size:0px;padding:10px 25px;padding-top:0;padding-right:0px;padding-bottom:0px;padding-left:0px;word-break:break-word;\">                                            <p style=\"border-top:solid 3px #f06428;font-size:1;margin:0px auto;width:100%;\">                                            </p>                                            <!--[if mso | IE]> <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-top:solid 3px #f06428;font-size:1;margin:0px auto;width:600px;\" role=\"presentation\" width=\"600px\" > <tr> <td style=\"height:0;line-height:0;\"> &nbsp; </td> </tr> </table> <![endif]-->                                        </td>                                    </tr>                                </table>                            </div>                            <!--[if mso | IE]> </td> </tr> </table> <![endif]-->                        </td>                    </tr>                </tbody>            </table>        </div>        <!--[if mso | IE]> </td> </tr> </table> <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"width:600px;\" width=\"600\" > <tr> <td style=\"line-height:0px;font-size:0px;mso-line-height-rule:exactly;\"> <![endif]-->        <div style=\"background:white;background-color:white;Margin:0px auto;max-width:600px;\">            <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"background:white;background-color:white;width:100%;\">                <tbody>                    <tr>                        <td style=\"direction:ltr;font-size:0px;padding:20px 0;padding-bottom:0px;padding-top:40;text-align:center;vertical-align:top;\">                            <!--[if mso | IE]> <table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"> <tr> <td style=\"vertical-align:bottom;width:600px;\" > <![endif]-->                            <div class=\"mj-column-per-100 outlook-group-fix\" style=\"font-size:13px;text-align:left;direction:ltr;display:inline-block;vertical-align:bottom;width:100%;\">                                <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"vertical-align:bottom;\"                                    width=\"100%\">                                    <tr>                                        <td align=\"left\" style=\"font-size:0px;padding:10px 25px;word-break:break-word;\">                                            <div style=\"font-family:Ubuntu, Helvetica, Arial, sans-serif;font-size:20px;font-weight:400;line-height:1;text-align:left;color:#f06428;\">                                            hello {4}, </div>                                        </td>                                    </tr>                                    <tr>                                        <td align=\"left\" style=\"font-size:0px;padding:10px 25px;word-break:break-word;\">                                            <div style=\"font-family:Ubuntu, Helvetica, Arial, sans-serif;font-size:15px;line-height:1.5;text-align:left;color:#040707;\">                                                here’s access to your website’s dashboard.                                        </td>                                    </tr>                                     <tr>                                        <td align=\"left\" style=\"font-size:0px;padding:10px 25px;word-break:break-word;\">                                            <div style=\"font-family:Ubuntu, Helvetica, Arial, sans-serif;font-size:15px;line-height:1.5;text-align:left;color:#040707;\">                                                {0} has given you access to manage your website’s content.                                        </td>                                    </tr>                                    <tr>                                        <td align=\"left\" style=\"font-size:0px;padding:10px 25px;word-break:break-word;\">                                            <div style=\"font-family:Ubuntu, Helvetica, Arial, sans-serif;font-size:15px;line-height:1.5;text-align:left;color:#040707;\">                                                When people say “content is king”, they mean it. Especially in the digital world.                                        </td>                                    </tr>                                    <tr>                                        <td align=\"left\" style=\"font-size:0px;padding:10px 25px;word-break:break-word;\">                                            <div style=\"font-family:Ubuntu, Helvetica, Arial, sans-serif;font-size:15px;line-height:1.5;text-align:left;color:#040707;\">                                                Sharing content that’s relevant to your business and customers, on a regular basis,                                                is bound to help your website get discovered.                                        </td>                                    </tr>                                    <tr>                                        <td align=\"left\" style=\"font-size:0px;padding:10px 25px;word-break:break-word;\">                                            <div style=\"font-family:Ubuntu, Helvetica, Arial, sans-serif;font-size:15px;line-height:1.5;text-align:left;color:#040707;\">                                                Search Engines love good content. Customers love good content. Now you have the                                                power to create it yourself.                                        </td>                                    </tr>                                    <tr>                                        <td align=\"left\" style=\"font-size:0px;padding:10px 25px;word-break:break-word;\">                                            <div style=\"font-family:Ubuntu, Helvetica, Arial, sans-serif;font-size:15px;line-height:1.5;text-align:left;color:#040707;\">                                            you can now manage your website content here: </div>                                        </td>                                    </tr>                                                                        <tr>                                        <td align=\"left\" style=\"font-size:0px;padding:10px 25px;padding-top:0;word-break:break-word;\">                                            <div style=\"font-family:Ubuntu, Helvetica, Arial, sans-serif;font-size:15px;line-height:1.5;text-align:left;color:#040707;\">                                            username:                                                <span color=\"#f06428\">{2}</span>                                                <br> password:                                                <span color=\"#f06428\">{3}</span>                                            </div>                                        </td>                                    </tr>                                    <tr>                                        <td align=\"left\" style=\"font-size:0px;padding:10px 25px;padding-top:0;word-break:break-word;\">                                            <div style=\"font-family:Ubuntu, Helvetica, Arial, sans-serif;font-size:15px;line-height:1.5;text-align:left;color:#040707;\">                                                <a color=\"#f06428\" href=\"{1}\">login now</a>                                            </div>                                        </td>                                    </tr>                                </table>                            </div>                            <!--[if mso | IE]> </td> </tr> </table> <![endif]-->                        </td>                    </tr>                </tbody>            </table>        </div>        <!--[if mso | IE]> </td> </tr> </table> <![endif]-->        <!-- Footer -->        <!--[if mso | IE]> <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"width:600px;\" width=\"600\" > <tr> <td style=\"line-height:0px;font-size:0px;mso-line-height-rule:exactly;\"> <![endif]-->        <div style=\"background:white;background-color:white;Margin:0px auto;max-width:600px;\">            <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"background:white;background-color:white;width:100%;\">                <tbody>                    <tr>                        <td style=\"direction:ltr;font-size:0px;padding:20px 0;padding-bottom:0px;padding-top:10px;text-align:center;vertical-align:top;\">                            <!--[if mso | IE]> <table role=\"presentation\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"> <tr> <td style=\"vertical-align:bottom;width:600px;\" > <![endif]-->                            <div class=\"mj-column-per-100 outlook-group-fix\" style=\"font-size:13px;text-align:left;direction:ltr;display:inline-block;vertical-align:bottom;width:100%;\">                                <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"vertical-align:bottom;\"                                    width=\"100%\">                                                                                                        <tr>                                        <td style=\"font-size:0px;padding:10px 25px;padding-top:20;padding-right:0px;padding-bottom:0px;padding-left:0px;word-break:break-word;\">                                            <p style=\"border-top:solid 1px #ffbb9d;font-size:1;margin:0px auto;width:100%;\">                                            </p>                                            <!--[if mso | IE]> <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-top:solid 1px #ffbb9d;font-size:1;margin:0px auto;width:600px;\" role=\"presentation\" width=\"600px\" > <tr> <td style=\"height:0;line-height:0;\"> &nbsp; </td> </tr> </table> <![endif]-->                                        </td>                                    </tr>                                                                        <tr>                                        <td style=\"font-size:0px;padding:10px 25px;padding-top:20;padding-right:0px;padding-bottom:0px;padding-left:0px;word-break:break-word;\">                                            <p style=\"border-top:solid 3px #f06428;font-size:1;margin:0px auto;width:100%;\">                                            </p>                                            <!--[if mso | IE]> <table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" style=\"border-top:solid 3px #f06428;font-size:1;margin:0px auto;width:600px;\" role=\"presentation\" width=\"600px\" > <tr> <td style=\"height:0;line-height:0;\"> &nbsp; </td> </tr> </table> <![endif]-->                                        </td>                                    </tr>                                </table>                            </div>                            <!--[if mso | IE]> </td> </tr> </table> <![endif]-->                        </td>                    </tr>                </tbody>            </table>        </div>        <!--[if mso | IE]> </td> </tr> </table> <![endif]-->    </div></body></html>";
                    emailBody = fileContent.Replace("{0}",OptionalParams[EnvConstants.Constants.EmailParam_DeveloperName]) 
                        .Replace("{1}", OptionalParams[EnvConstants.Constants.EmailParam_KAdminUrl])
                        .Replace("{2}", OptionalParams[EnvConstants.Constants.EmailParam_KAdminUserName])
                        .Replace("{3}", OptionalParams[EnvConstants.Constants.EmailParam_KAdminPassword])
                        .Replace("{4}", OptionalParams[EnvConstants.Constants.EmailParam_CustomerName]);
                    emailUserConfigType = EmailUserConfigType.WEBSITE_USER;
                }

                else if (MailType == MailType.CUSTOMER_BILLING_NOT_ACTIVATED)
                {
                    subject = string.Format("Billing Activation Error");
                    emailBody = string.Format("<html><head></head><body>The billing activation api failed for the  following customer ids: <br/><br/>{0}", OptionalParams[EnvConstants.Constants.BillingActivationFailedCustomers]);
                }

                else if (MailType == MailType.CUSTOMER_ENQUIRY)
                {
                    string pair = "";
                    foreach (var key in OptionalParams.Keys)
                    {
                        pair = pair + key + ":" + OptionalParams[key] + "<br/><br/>";
                    }
                    subject = "New inquiry via website";
                    emailBody = "Hi, <br/> You have a received the following inquiry:<br/>" + pair;
                }

                else if (MailType == MailType.CUSTOM_MESSAGE)
                {
                    subject = OptionalParams[EnvConstants.Constants.EmailParam_Subject];
                    emailBody = OptionalParams[EnvConstants.Constants.EmailParam_Body];
                }

                try
                {
                    if (MailType == MailType.WEBSITE_ACTIVATION)
                    {
                        string AdminMail = "assist@getkitsune.com";
                        SendEmail(new EmailRequestWithAttachments { EmailBody = "hello someone with mailId:" + EmailID + " activated his/her website.", To = new List<string> { AdminMail }, Subject = "Someone activated the Website", Attachments = Attachments }, emailUserConfigType, clientId);
                    }

                    return SendEmail(new EmailRequestWithAttachments
                    {
                        EmailBody = emailBody,
                        To = new List<string> { EmailID },
                        Subject = subject,
                        Attachments = Attachments
                    }, emailUserConfigType, clientId);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return "error";

        }
        //mails


        public string SendEmail(EmailRequestWithAttachments req, EmailUserConfigType configType = EmailUserConfigType.WEBSITE_DEVELOPER, string clientId = null)
        {
            try
            {
                var basePlugin = BasePluginConfigGenerator.GetBasePlugin(clientId);
                var basePluginEmailConfiguration = (configType == EmailUserConfigType.WEBSITE_DEVELOPER) ? basePlugin.GetEmailConfiguration() : basePlugin.GetKAdminEmailConfiguration();

                WithFloatsExternalSMTPConfiguration smtpConfig;

                #region INIT EMAIL CONFIGURATION
                if (req.CustomSMTPConfig == null)
                {
                    smtpConfig = new WithFloatsExternalSMTPConfiguration()
                    {
                        EnableSsl = basePluginEmailConfiguration.EnableSsl,
                        ServerHost = basePluginEmailConfiguration.ServerHost,
                        ServerPort = basePluginEmailConfiguration.ServerPort,
                        SMTPUserName = basePluginEmailConfiguration.SMTPUserName,
                        SMTPUserPassword = basePluginEmailConfiguration.SMTPUserPassword,
                        TimeOut = basePluginEmailConfiguration.TimeOut,
                        UserName = basePluginEmailConfiguration.SMTPUserName
                    };
                }
                else
                {
                    smtpConfig = new WithFloatsExternalSMTPConfiguration()
                    {
                        EnableSsl = req.CustomSMTPConfig.EnableSsl,
                        ServerHost = req.CustomSMTPConfig.ServerHost,
                        ServerPort = req.CustomSMTPConfig.ServerPort,
                        SMTPUserName = req.CustomSMTPConfig.SMTPUserName,
                        SMTPUserPassword = req.CustomSMTPConfig.SMTPUserPassword,
                        TimeOut = req.CustomSMTPConfig.TimeOut,
                        UserName = req.CustomSMTPConfig.UserName
                    };
                }
                #endregion

                var request = (HttpWebRequest)WebRequest.Create(EnvConstants.Constants.EmailAPI);
                request.Method = "POST";
                request.ContentType = "application/json";

                var requestObj = new KitsuneConvertMailRequest
                {
                    ClientId = EnvConstants.Constants.KitsuneDevClientId,
                    EmailBody = req.EmailBody,
                    Subject = req.Subject,
                    To = req.To,
                    Attachments = req.Attachments,
                    From = basePluginEmailConfiguration.FromEmailAddress,
                    CustomSMTPConfig = smtpConfig
                };

                var jsonSerializer = new DataContractJsonSerializer(typeof(KitsuneConvertMailRequest));
                var mem = new MemoryStream();
                jsonSerializer.WriteObject(mem, requestObj);

                string finalData = Encoding.UTF8.GetString(mem.ToArray(), 0, (int)mem.Length);
                var bytes = new UTF8Encoding().GetBytes(finalData);

                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }

                WebResponse ws = request.GetResponse();
                var sr = new StreamReader(ws.GetResponseStream());
                var rs = sr.ReadToEnd();
                return rs;
            }
            catch (Exception ex)
            {
                return null;
                //EventLogger.Write(ex, "NowFloats.Boost Exception: Unable to SendMailToServer", null);
            }
        }
    }
}