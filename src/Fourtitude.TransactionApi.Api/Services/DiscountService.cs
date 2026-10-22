namespace Fourtitude.TransactionApi.Api.Services
{
    public class DiscountService : IDiscountService
    {
        private const int CentsPerRinggit = 100;
        private const int MaxDiscountPercent = 20;

        public DiscountResult Calculate(long totalAmountInCents)
        {
            var discountPercent = Math.Min(
                GetBaseDiscountPercent(totalAmountInCents) +
                GetConditionalDiscountPercent(totalAmountInCents),
                MaxDiscountPercent);

            var totalDiscount = (long)Math.Round(
                totalAmountInCents * discountPercent / 100m,
                MidpointRounding.AwayFromZero);

            return new DiscountResult(totalAmountInCents, totalDiscount,
                totalAmountInCents - totalDiscount);
        }

        private static int GetBaseDiscountPercent(long totalAmountInCents)
        {
            var amountInRinggit = (decimal)totalAmountInCents / CentsPerRinggit;

            return amountInRinggit switch
            {
                < 200 => 0,
                <= 500 => 5,
                <= 800 => 7,
                <= 1200 => 10,
                _ => 15
            };
        }

        private static int GetConditionalDiscountPercent(long totalAmountInCents)
        {
            if(totalAmountInCents % CentsPerRinggit != 0)
            {
                return 0;
            }
            var amountInRinggit = totalAmountInCents / CentsPerRinggit;
            var discountPercent = 0;

            if (amountInRinggit > 500 && IsPrime(amountInRinggit))
            {
                discountPercent += 8;
            }

            if (amountInRinggit > 900 && amountInRinggit % 10 == 5)
            {
                discountPercent += 10;
            }

            return discountPercent;
        }

        private static bool IsPrime(long number)
        {
            if (number < 2)
            {
                return false;
            }

            if (number % 2 == 0)
            {
                return number == 2;
            }

            for (long divisor = 3; divisor * divisor <= number; divisor += 2)
            {
                if (number % divisor == 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
    
}
