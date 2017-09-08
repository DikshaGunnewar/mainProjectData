using EntitiesLayer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using Autopilot.Helper;
using System.Net;
using Microsoft.AspNet.Identity;
using ServiceLayer.Interfaces;
using System.Web.SessionState;
using PayPal.Api;
using Autopilot.Models;
using Autopilot.IdentityConfig;  

namespace Autopilot.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PaypalPaymentHelper _paymentheader;
        private PayPal.Api.Payment payment;
      
        private readonly IUserService _userService;
        public PaymentController(IUserService userService)
        {
            _userService = userService;
            
        }

        //public ActionResult PaymentProceed(int planId, string socialIds)
        //{
        //    try
        //    {
        //        //var planDetail  = 
        //        PaymentViewModel vm = new PaymentViewModel();
        //        vm.PlanDetails = _userService.GetASubscriptionPlan(planId);
        //        vm.socialIds = socialIds;
        //        return RedirectToAction("PaymentWithPaypal", vm);
        //    }
        //    catch (Exception)
        //    {
                
        //        throw;
        //    }
        //}


        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult PaymentWithPaypal()
        {
            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            try
            {
                string planId = Request.Params["planId"];
                string socailIds = Request.Params["socialIds"];

                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    //this section will be executed first because PayerID doesn't exist
                    //it is returned by the create function call of the payment class
                    // Creating a payment
                    // baseURL is the url on which paypal sendsback the data.
                    // So we have provided URL of this controller only

                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority +
                                "/Payment/PaymentWithPayPal?";

                    //guid we are generating for storing the paymentID received in session
                    //after calling the create function and it is used in the payment execution

                    var guid = Convert.ToString((new Random()).Next(100000));

                    //CreatePayment function gives us the payment approval url
                    //on which payer is redirected for paypal account payment

                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid + "&planId=" + planId + "&socialIds=" + socailIds, planId, socailIds);

                    //get links returned from paypal in response to Create function call

                    var links = createdPayment.links.GetEnumerator();

                    string paypalRedirectUrl = null;

                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;

                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment
                            paypalRedirectUrl = lnk.href;
                        }
                    }

                    // saving the paymentID in the key guid
                    Session.Add(guid, createdPayment.id);

                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This section is executed when we have received all the payments parameters
                    // from the previous call to the function Create
                    // Executing a payment
                    var guid = Request.Params["guid"];

                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    PaymentViewModel vm = new PaymentViewModel()
                    {
                        planId = planId,
                        socialIds = socailIds,
                        Amount = executedPayment.transactions[0].amount.total,
                        TransactionId = executedPayment.transactions[0].related_resources[0].sale.id,
                        userId = User.Identity.GetUserId(),
                        Currency = executedPayment.transactions[0].related_resources[0].sale.amount.currency,
                        Status = executedPayment.state,
                        InvoiceId = executedPayment.transactions[0].invoice_number,
                       Date =DateTime.Parse(executedPayment.create_time)
                    };
                    _userService.SaveTransactionDetails(vm);

                    if (executedPayment.state.ToLower() == "approved")
                    {



                        return RedirectToAction("PaymentSuccess", vm);

                    }
                    else {
                        return View("FailureView");
                    }
                }
            }
            catch (Exception ex)
            {
                //Logger.log("Error" + ex.Message);
              return View("FailureView");

            }

        }



      


       
        private Payment CreatePayment(APIContext apiContext, string redirectUrl,string planId,string socialIds)
        {

            //similar to credit card create itemlist and add item objects to it
            var itemList = new ItemList() { items = new List<Item>() };
            //int subscriptionPlanId = Int16.Parse(planId);
            var plan = _userService.GetASubscriptionPlan(planId);
            int quantity = socialIds.Split(',').Count();
            itemList.items.Add(new Item()
            {
                name = plan.Title,
                currency = "USD",
                price = plan.Price.ToString(),
                quantity = quantity.ToString(),
               
            });

            var payer = new Payer() { payment_method = "paypal" };

            // Configure Redirect Urls here with RedirectUrls object
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl,
                return_url = redirectUrl
            };

            // similar as we did for credit card, do here and create details object
            var details = new Details()
            {
                tax = "1",
                subtotal = (quantity * plan.Price).ToString()
            };

            // similar as we did for credit card, do here and create amount object
            var amount = new Amount()
            {
                currency = "USD",
                total = (double.Parse(details.tax) + double.Parse(details.subtotal)).ToString(), // Total must be equal to sum of shipping, tax and subtotal.
                details = details
            };

            var transactionList = new List<Transaction>();
            Random generator = new Random();
            string random = generator.Next(0, 1000000).ToString("D6");
            transactionList.Add(new Transaction()
            {
                description = "Transaction description.",
                invoice_number = random,
                amount = amount,
                item_list = itemList
            });

            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };

            // Create a payment using a APIContext
            return this.payment.Create(apiContext);
        }

        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution() { payer_id = payerId };
            this.payment = new Payment() { id = paymentId };
            return this.payment.Execute(apiContext, paymentExecution);
        }

        public ActionResult PaymentSuccess(PaymentViewModel vm)
        {
            
            //var acc = _paymentheader.CreateCustomer(model);
            vm.planDetails = _userService.GetASubscriptionPlan(vm.planId);


            return View(vm);
        }

    

	}
}