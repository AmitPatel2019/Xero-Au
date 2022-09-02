using CosmicApiModel;
using CosmicCoreApi;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CosmicCoreApi.Helper;
using CosminInv.Api.App_Start;
using Stripe;
using System.Configuration;
using Flexis.Log;

namespace CosmicCoreApi.Controllers
{
    [CustomAuthorize]
    public class StripeController : ApiController
    {
        private CosmicContext _csContext = CosmicContext.Instance;
        SessionHelper _sessionHelper = new SessionHelper();


        [HttpPost]
        public HttpResponseMessage SubmitPayment(PlanMaster planMaster)
        {
            if (string.IsNullOrEmpty(planMaster.Token))
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                   HttpHelper.FormatError("Bad Request Found", "No Token found the request"));
            }

            string decrypted = EncryptionHelper.Decrypt(planMaster.Token);

            if (string.IsNullOrEmpty(decrypted))
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                  HttpHelper.FormatError("Bad Request Found", "No Token found the request"));
            }

            string[] spltQueryString = decrypted.Split(new char[] { '|' });

            
            _sessionHelper.AccountID = CSConvert.ToInt(spltQueryString[0]);
            int planID = CSConvert.ToInt(spltQueryString[1]);



            if (_sessionHelper.AccountID == 0)
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                  HttpHelper.FormatError("Account ID Not Found", "Account ID Not Found"));

            var cosDBResponse = _csContext.GetPlan(planID);
            if (cosDBResponse.Data != null)
            {
                StripeConfiguration.SetApiKey(ConfigurationManager.AppSettings["Stripe_Secret_Key"]);
                // var planService = new StripeChargeService("sk_test_FU16ohGOQGUNZETXWiWQGcOh");
                try
                {

                    var myCharge = new StripeChargeCreateOptions();

                    // always set these properties
                    myCharge.Amount = (int)cosDBResponse.Data.RatePerYearTotal * 100;
                    myCharge.Currency = "aud";

                    // set this if you want to
                    myCharge.Description = "Account # " + _sessionHelper.AccountID;
                    myCharge.StatementDescriptor = "Account # " + _sessionHelper.AccountID;

                    myCharge.SourceTokenOrExistingSourceId = planMaster.StripeToken;

                    // set this property if using a customer - this MUST be set if you are using an existing source!
                    //myCharge.CustomerId = *customerId *;

                    // set this if you have your own application fees (you must have your application configured first within Stripe)
                    //  myCharge.ApplicationFee = 25;

                    // (not required) set this to false if you don't want to capture the charge yet - requires you call capture later
                    myCharge.Capture = true;
                    myCharge.Destination = ConfigurationManager.AppSettings["Cosmic_StripeUserID"];
                    myCharge.DestinationAmount = myCharge.Amount;

                    var chargeService = new StripeChargeService(ConfigurationManager.AppSettings["Stripe_Secret_Key"]);
                    var stripeCharge = chargeService.Create(myCharge);
                    if (stripeCharge != null)
                    {

                        var payment = new StripePayment();
                        payment.AccountID = _sessionHelper.AccountID;
                        payment.PlanID = planID;

                        payment.StripeID = stripeCharge.Id;
                        payment.StripeBalanceTxnID = stripeCharge.BalanceTransactionId;
                        payment.StripeDateTime = stripeCharge.Created;
                        payment.StripeIsPaid = stripeCharge.Paid;
                        payment.StripeStatus = stripeCharge.Status;
                        payment.Amount = Convert.ToDecimal(myCharge.Amount) / 100;

                        //Save the payment to database
                        var dbResp = _csContext.SavePayment(payment);
                        if (dbResp != null)
                        {
                            if (dbResp.StatusCode == DBResponseStatusCode.OK)
                            {
                                _csContext.UpdgradePlan(planID);

                                return Request.CreateResponse(HttpStatusCode.OK, dbResp);
                            }
                        }

                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, cosDBResponse);
                    }
                }
                catch (StripeException ex1)
                {
                    Logger log = CosmicLogger.SetLog();
                    log.Error(ex1);
                    _csContext.LogErrorToDB("Stripe", "SubmitPayment", ex1.Message, ex1);

                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                       HttpHelper.FormatError("Error while creating stripe payment", ex1.Message));
                }
                catch (Exception ex)
                {
                    Logger _log = CosmicLogger.SetLog();
                    _log.Error(ex);

                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                       HttpHelper.FormatError("Error while creating stripe payment", ex.Message));
                }

                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    HttpHelper.FormatError("Error while creating stripe payment", "No Payment received"));
            }

            return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                  HttpHelper.FormatError("Error while creating stripe payment", "Could not found plan details"));

        }

        [HttpGet]
        public HttpResponseMessage GetPayment()
        {
            try
            {
                
                var cosDBResponse = _csContext.GetPayment(_sessionHelper.AccountID);

                if (cosDBResponse.Data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, cosDBResponse);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                          HttpHelper.FormatError("Account Not Found", cosDBResponse.Error));
                }
            }
            catch (Exception ex)
            {
                // Log exception code goes here
                Logger log = CosmicLogger.SetLog();
                log.Error(ex);

                _csContext.LogErrorToDB("Stripe", "GetPayment", ex.Message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    HttpHelper.FormatError("Error occured while payment detail", ex.Message));
            }
        }

        [HttpPost]
        public HttpResponseMessage SubmitXeroPayment(PlanMaster planMaster)
        {
            
            if (_sessionHelper.AccountID == 0)
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                  HttpHelper.FormatError("Account ID Not Found", "Account ID Not Found"));

            var cosDBResponse = _csContext.GetPlan(planMaster.PlanID);
            if (cosDBResponse.Data != null)
            {
                StripeConfiguration.SetApiKey(ConfigurationManager.AppSettings["Stripe_Secret_Key"]);
                // var planService = new StripeChargeService("sk_test_FU16ohGOQGUNZETXWiWQGcOh");
                try
                {

                    var myCharge = new StripeChargeCreateOptions();

                    // always set these properties
                    myCharge.Amount = (int)cosDBResponse.Data.RatePerYearTotal * 100;
                    myCharge.Currency = "aud";

                    // set this if you want to
                    myCharge.Description = "Account Xero # " + _sessionHelper.AccountID;
                    myCharge.StatementDescriptor = "Account Xero # " + _sessionHelper.AccountID;

                    myCharge.SourceTokenOrExistingSourceId = planMaster.StripeToken;

                    // set this property if using a customer - this MUST be set if you are using an existing source!
                    //myCharge.CustomerId = *customerId *;

                    // set this if you have your own application fees (you must have your application configured first within Stripe)
                    //  myCharge.ApplicationFee = 25;

                    // (not required) set this to false if you don't want to capture the charge yet - requires you call capture later
                    myCharge.Capture = true;
                    myCharge.Destination = ConfigurationManager.AppSettings["Cosmic_StripeUserID"];
                    myCharge.DestinationAmount = myCharge.Amount;

                    var chargeService = new StripeChargeService(ConfigurationManager.AppSettings["Stripe_Secret_Key"]);
                    var stripeCharge = chargeService.Create(myCharge);
                    if (stripeCharge != null)
                    {

                        var payment = new StripePayment();
                        payment.AccountID = _sessionHelper.AccountID;
                        payment.PlanID = planMaster.PlanID;

                        payment.StripeID = stripeCharge.Id;
                        payment.StripeBalanceTxnID = stripeCharge.BalanceTransactionId;
                        payment.StripeDateTime = stripeCharge.Created;
                        payment.StripeIsPaid = stripeCharge.Paid;
                        payment.StripeStatus = stripeCharge.Status;
                        payment.Amount = Convert.ToDecimal(myCharge.Amount) / 100;

                        //Save the payment to database
                        var dbResp = _csContext.SavePayment(payment);
                        if (dbResp != null)
                        {
                            if (dbResp.StatusCode == DBResponseStatusCode.OK)
                            {
                                _csContext.UpdgradePlan(planMaster.PlanID);

                                return Request.CreateResponse(HttpStatusCode.OK, dbResp);
                            }
                        }

                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, cosDBResponse);
                    }
                }
                catch (StripeException ex1)
                {
                    Logger log = CosmicLogger.SetLog();
                    log.Error(ex1);
                    _csContext.LogErrorToDB("Stripe", "SubmitXeroPayment", ex1.Message, ex1);

                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                       HttpHelper.FormatError("Error while creating stripe payment", ex1.Message));
                }
                catch (Exception ex)
                {
                    Logger _log = CosmicLogger.SetLog();
                    _log.Error(ex);

                    return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                       HttpHelper.FormatError("Error while creating stripe xero  payment", ex.Message));
                }

                return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    HttpHelper.FormatError("Error while creating stripe xero payment", "No Payment received"));
            }

            return Request.CreateErrorResponse(HttpStatusCode.NotFound,
                  HttpHelper.FormatError("Error while creating stripe payment", "Could not found plan details"));

        }
    }
}
