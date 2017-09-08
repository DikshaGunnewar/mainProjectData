using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using ServiceLayer.Interfaces;
using ServiceLayer.EnumStore;
using EntitiesLayer.ViewModel;
using RepositoryLayer.Infrastructure;
using RepositoryLayer.Repositories;


namespace ServiceLayer.Services
{
   public class PaymentServices:IPaymentServices
    {
       private readonly IRepository<UserPaymentGatewaysDetailsModel> _userPayment;
       private readonly IRepository<MessagingModel> _messagingModel;
       private readonly IRepository<PaymentDetails> _paymentDetails;

       public PaymentServices(IRepository<UserPaymentGatewaysDetailsModel> userPayment, IRepository<MessagingModel> messagingModel, IRepository<PaymentDetails> paymentDetails)
        {
            _userPayment = userPayment;
            _messagingModel = messagingModel;
            _paymentDetails = paymentDetails;
        }
       public MessagingModel AddUserPaymentGatewayDetails(UserPaymentGatewaysDetailsModel model)
        {
            try
            {
                UserPaymentGatewaysDetailsModel entityModel = new UserPaymentGatewaysDetailsModel();

                entityModel.GatwayUserID = model.GatwayUserID;
                entityModel.GatewayTypeID = model.GatewayTypeID;
                entityModel.UserID = model.UserID;
                entityModel.CreatedDate = DateTime.UtcNow;
                entityModel.CreatedBy = model.UserID;
                entityModel.ContactPersonId = model.ContactPersonId;
                _userPayment.Add(entityModel);
               // _unitOfWork.Commit();

                return null;
                //return new MessagingModel { Result = (int)Message.SUCCESS, Message = "Gateway Detail is added successfully." };
            }
            catch (Exception ex)
            {
                return new MessagingModel { Result = (int)Message.Exeption, Message = ex.Message };
            }
        }

        public MessagingModel MakePayment(PaymentDetails model)
        {
            try
            {
               // TransactionViewModel tranModel = new TransactionViewModel();
                PaymentDetails entityModel = new PaymentDetails
                {
                    Amount = model.Amount,
                    InvoiceID = model.InvoiceID,
                    PaidByID = model.PaidByID,
                    PaymentDate = model.PaymentDate,
                    GatewayChargeID = model.GatewayChargeID,
                    GatewayTransactionId = model.GatewayTransactionId,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow,
                    PaymentStatus = 1
                };

                _paymentDetails.Add(entityModel);
               // _unitOfWork.Commit();

                //tranModel.Amount = model.Amount;
                //tranModel.CreatedDate = DateTime.UtcNow;
                //tranModel.InvoiceID = model.InvoiceID;
                //tranModel.IsDeleted = false;
                //tranModel.TransactionStatus = model.PaymentStatus;
                //tranModel.TransactionDate = model.PaymentDate;
                //tranModel.TransactionType = 0;
                //tranModel.Refrence = "Invoice Reference";
                //tranModel.Description = "Invoice Payement";

               // _transactionservices.ManageInvoiceTransaction(tranModel.InvoiceID);
                //AddTransaction(tranModel);

                //InvoiceStatusOnPayment(model);
                return null;
               // return new MessagingModel { Result = (int)Message.SUCCESS, Message = "Payment added successfully" };
            }
            catch (Exception ex)
            {
                return null;
               // return new MessagingModel { Result = (int)Message.Exeption, Message = ex.Message };
            }
        }

        //public MessagingModel InvoiceStatusOnPayment(PaymentDetails model)
        //{
        //    try
        //    {
        //        var invoiceData = _invoiceRepository.GetAll().Where(x => x.ID == model.InvoiceID).FirstOrDefault();
        //        var amountPaid = (invoiceData.AmountPaid == null ? 0 : invoiceData.AmountPaid.Value) + model.Amount;
        //        invoiceData.AmountPaid = amountPaid;
        //        var amountDue = (invoiceData.Total == null ? 0 : invoiceData.Total.Value) - amountPaid;
        //        invoiceData.AmountDue = amountDue;
        //        if (amountDue == 0 || amountPaid > invoiceData.Total)
        //        {
        //            invoiceData.InvoiceStatus = 46;
        //        }
        //        else
        //        {
        //            invoiceData.InvoiceStatus = 45;
        //        }

        //        invoiceData.ModifiedDate = DateTime.UtcNow;
        //       // _unitOfWork.Commit();
        //        return new MessagingModel { Result = (int)Message.SUCCESS, Message = "Payment added successfully" };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new MessagingModel { Result = (int)Message.Exeption, Message = ex.Message };
        //    }
        //}
    }
}
