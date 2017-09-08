using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PayPal.Api;
using EntitiesLayer.Entities;
using EntitiesLayer.ViewModel;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Net.Http;
using System.Web.Mvc;
using System.Web.Http;

namespace Autopilot.Helper
{
    public class PaypalPaymentHelper
    {

        private PayPal.Api.Payment payment;
        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {

            //similar to credit card create itemlist and add item objects to it
            var itemList = new ItemList() { items = new List<Item>() };

            itemList.items.Add(new Item()
            {
                name = "Item Name",
                currency = "USD",
                price = "5",
                quantity = "1",
                sku = "sku"
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
                shipping = "1",
                subtotal = "5"
            };

            // similar as we did for credit card, do here and create amount object
            var amount = new Amount()
            {
                currency = "USD",
                total = "7", // Total must be equal to sum of shipping, tax and subtotal.
                details = details
            };

            var transactionList = new List<Transaction>();

            transactionList.Add(new Transaction()
            {
                description = "Transaction description.",
                invoice_number = "your invoice number",
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





        //OrganizatonPaymentGatewayMappingModel gatewayDetail;

        //public PaypalPaymentHelper(OrganizatonPaymentGatewayMappingModel _gatewayDetail)
        //{
        //    gatewayDetail = _gatewayDetail;
        //}

        //public dynamic MakePayment(PaymentViewModel model)
        //{
        //    string payment = model.PaypalPaymentId;
        //    var response = new HttpResponseViewmodel();
        //    //getting the apiContext as earlier
        //    if (string.IsNullOrEmpty(gatewayDetail.PublicKey) || string.IsNullOrEmpty(gatewayDetail.PrivateKey))
        //    {
        //        PaypalConfiguration.TakeApiKeyFromWebConfig = true;
               
        //    }
        //    else
        //    {
        //        PaypalConfiguration.TakeApiKeyFromWebConfig = false;
        //        PaypalConfiguration.ClientId = gatewayDetail.PublicKey;
        //        PaypalConfiguration.ClientSecret = gatewayDetail.PrivateKey;
        //    }
        //    APIContext apiContext = PaypalConfiguration.GetAPIContext(gatewayDetail.PublicKey, gatewayDetail.PrivateKey);

        //    try
        //    {
        //        var executedPayment = ExecutePayment(apiContext, model.PayerId, model.PaypalPaymentId as string);
        //        return executedPayment;
        //    }
        //    catch (Exception ex)
        //    {
        //        return new { PaymentStatus = "Fail", ErrorMessage = ex.Message };
        //    }

        //}

        //public dynamic CreateCustomer(PaymentViewModel model)
        //{
        //    //model.BaseUrl = "https://api.sandbox.paypal.com/v1/payments/payment/?";
        //    model.BaseUrl = "http://localhost:53143/Payment/PayCustomerInvoiceWithPaypal/?";
        //    string baseURI = model.BaseUrl;
        //    Payment createdPayment;
        //    //baseURI=baseURI+ "/Paypal/PaymentWithPayPal?";
        //    //getting the apiContext as earlier
        //    if (string.IsNullOrEmpty(gatewayDetail.PublicKey) || string.IsNullOrEmpty(gatewayDetail.PrivateKey))
        //    {
        //        PaypalConfiguration.TakeApiKeyFromWebConfig = true;
        //    }
        //    else
        //    {
        //        PaypalConfiguration.TakeApiKeyFromWebConfig = false;
        //        PaypalConfiguration.ClientId = gatewayDetail.PublicKey;
        //        PaypalConfiguration.ClientSecret = gatewayDetail.PrivateKey;
        //    }

        //    APIContext apiContext = PaypalConfiguration.GetAPIContext(gatewayDetail.PublicKey, gatewayDetail.PrivateKey);
        //    try
        //    {
        //        var guid = Convert.ToString((new Random()).Next(100000));
        //        try
        //        {
        //             createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid, "USD", Convert.ToString(model.Amount=100), model.InvoiceNumber);
        //             string paymentId = createdPayment.id;
        //             model.PaypalPaymentId = paymentId;
                
        //        }
        //        catch (Exception ex )
        //        {

        //            return new { Status = "Fail", Message = ex.Message };
        //        }
        //        var links = createdPayment.links.GetEnumerator();
        //        string paypalRedirectUrl = null;
        //        while (links.MoveNext())
        //        {
        //            Links lnk = links.Current;

        //            if (lnk.rel.ToLower().Trim().Equals("approval_url"))
        //            {

        //                paypalRedirectUrl = lnk.href;
                        
        //            }
        //        }
        //        return paypalRedirectUrl;


        //        //return new RedirectResult(baseURI, false /*permanent*/);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new { Status = "Fail", Message = ex.Message };
        //    }
        //}
        
        //private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        //{
            
        //    var paymentExecution = new PaymentExecution() { payer_id = payerId };
        //    //this.payment = new Payment() { id = (string)System.Web.HttpContext.Current.Session["PaypalPaymentId"] };
        //    this.payment = new Payment() { id = paymentId };
        //    return this.payment.Execute(apiContext, paymentExecution);
        //}

        //private Payment CreatePayment(APIContext apiContext, string redirectUrl, string Currency, string TotalAmount, string InvoiceNo)
        //{
        //    try
        //    {
        //        InvoiceNo = InvoiceNo + Convert.ToString(new Random().Next(1000));

        //        //similar to credit card create itemlist and add item objects to it
        //        var itemList = new ItemList() { items = new List<Item>() };

        //        //itemList.items.Add(new Item()
        //        //{
        //        //    name = "Item Name",
        //        //    currency = "USD",
        //        //    price = "5",
        //        //    quantity = "1",
        //        //    sku = "sku"
        //        //});

        //        var payer = new Payer() { payment_method = "paypal" };

        //        // Configure Redirect Urls here with RedirectUrls object
        //        var redirUrls = new RedirectUrls()
        //        {
        //            cancel_url = redirectUrl + "&Cancel=true",
        //            return_url = redirectUrl
        //        };

        //        // similar as we did for credit card, do here and create details object
        //        var details = new Details()
        //        {
        //            tax = "0",
        //            shipping = "0",
        //            subtotal = TotalAmount
        //        };

        //        // similar as we did for credit card, do here and create amount object
        //        var amount = new Amount()
        //        {
        //            currency = Currency,
        //            total = TotalAmount, // Total must be equal to sum of shipping, tax and subtotal.
        //            details = details
        //        };

        //        var transactionList = new List<Transaction>();

        //        transactionList.Add(new Transaction()
        //        {
        //            description = "Payment for InvoiceNo :" + InvoiceNo,
        //            invoice_number = InvoiceNo,
        //            amount = amount,
        //            //item_list = itemList
        //        });

        //        this.payment = new Payment()
        //        {
        //            intent = "sale",
        //            payer = payer,
        //            transactions = transactionList,
        //            redirect_urls = redirUrls
        //        };

        //        // Create a payment using a APIContext
        //        return this.payment.Create(apiContext);
        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }
        //}


        

    }


}