using Fourtitude.TransactionApi.Api.Models;
using Fourtitude.TransactionApi.Api.Services;
using Fourtitude.TransactionApi.Api.Validation;
using log4net.Repository.Hierarchy;
using Microsoft.AspNetCore.Mvc;
namespace Fourtitude.TransactionApi.Api.Controllers
{

    [ApiController]
    [Route("api")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionValidator _validator;
        private readonly IDiscountService _discountService;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ITransactionValidator validator, IDiscountService discountService,
            ILogger<TransactionController> logger)
        {
            _validator = validator;
            _discountService = discountService;
            _logger = logger;
        }

        [HttpPost("submittrxmessage")]
        public ActionResult<SubmitTransactionResponse>
            SubmitTransaction(SubmitTransactionRequest request)
        {
            var validationResult = _validator.Validate(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning(
                "Transaction {PartnerRefNo} from {PartnerKey} rejected: {Reason}",
                request.PartnerRefNo,
                request.PartnerKey,
                validationResult.ErrorMessage);
                return
                    Ok(SubmitTransactionResponse.Failure(validationResult.ErrorMessage!));
            }

            var discount =
                _discountService.Calculate(request.TotalAmount!.Value);
                _logger.LogInformation(
                "Transaction {PartnerRefNo} from {PartnerKey} accepted: total {TotalAmount}, discount {TotalDiscount}, final {FinalAmount}",
                request.PartnerRefNo,
                request.PartnerKey,
                discount.TotalAmount,
                discount.TotalDiscount,
                discount.FinalAmount);

            return Ok(SubmitTransactionResponse.Success(
                discount.TotalAmount,
                discount.TotalDiscount,
                discount.FinalAmount));
        }

    }
}
