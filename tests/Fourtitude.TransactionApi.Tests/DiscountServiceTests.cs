using Fourtitude.TransactionApi.Api.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fourtitude.TransactionApi.Tests
{
    public class DiscountServiceTests
    {
        private readonly DiscountService _discountService = new();

        [Theory]
        [InlineData(100000, 10000, 90000)]
        [InlineData(120500, 24100, 96400)]
        public void Calculate_WithSpecSamples_ReturnsExpectedAmounts(long total, long discount, long final)
        {
            var result = _discountService.Calculate(total);

            Assert.Equal(total, result.TotalAmount);
            Assert.Equal(discount, result.TotalDiscount);
            Assert.Equal(final, result.FinalAmount);
        }

        [Theory]
        [InlineData(19999, 0)]
        [InlineData(20000, 1000)]
        [InlineData(50000, 2500)]
        [InlineData(50100, 3507)]
        [InlineData(80000, 5600)]
        [InlineData(80100, 8010)]
        [InlineData(120000, 12000)]
        [InlineData(120200, 18030)]
        public void Calculate_AtBaseTierBoundaries_AppliesBaseDiscount(long total, long expectedDiscount)
        {
            var result = _discountService.Calculate(total);

            Assert.Equal(expectedDiscount, result.TotalDiscount);
            Assert.Equal(total - expectedDiscount, result.FinalAmount);
        }

        [Theory]
        [InlineData(50300, 7545)]
        [InlineData(49900, 2495)]
        [InlineData(50350, 3525)]
        public void Calculate_WithPrimeAmount_AppliesPrimeDiscountOnlyAbove500(long total, long expectedDiscount)
        {
            var result = _discountService.Calculate(total);

            Assert.Equal(expectedDiscount, result.TotalDiscount);
        }

        [Theory]
        [InlineData(90500, 18100)]
        [InlineData(89500, 8950)]
        public void Calculate_WithAmountEndingIn5_AppliesDiscountOnlyAbove900(long total, long expectedDiscount)
        {
            var result = _discountService.Calculate(total);

            Assert.Equal(expectedDiscount, result.TotalDiscount);
        }

        [Fact]
        public void Calculate_WhenCombinedDiscountExceeds20Percent_CapsAt20Percent()
        {
            var result = _discountService.Calculate(120100);

            Assert.Equal(24020, result.TotalDiscount);
            Assert.Equal(96080, result.FinalAmount);
        }
    }
}
