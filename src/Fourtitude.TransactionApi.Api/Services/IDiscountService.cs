namespace Fourtitude.TransactionApi.Api.Services
{
    public interface IDiscountService
    {
        DiscountResult Calculate(long totalAmountInCents);
    }
}
