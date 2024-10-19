namespace SalesAPILibrary.Shared_Entities
{

    public static class TaxCalculator
    {
        private static decimal _taxRate;

        /// <summary>
        /// Sets the tax rate to be used for tax calculations.
        /// </summary>
        /// <param name="taxRate">The tax rate as a decimal (e.g., 0.07 for 7% tax).</param>
        public static void SetTaxRate(decimal taxRate)
        {
            if (taxRate < 0 || taxRate > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(taxRate), "Tax rate must be between 0 and 1.");
            }
            _taxRate = taxRate;
        }

        /// <summary>
        /// Calculates the tax for the given order total.
        /// </summary>
        /// <param name="orderTotal">The total amount of the order.</param>
        /// <returns>The calculated tax amount.</returns>
        public static decimal CalculateTax(decimal orderTotal)
        {
            if (orderTotal < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(orderTotal), "Order total cannot be negative.");
            }

            return orderTotal * _taxRate;
        }

        /// <summary>
        /// Calculates the total amount including tax for the given order total.
        /// </summary>
        /// <param name="orderTotal">The total amount of the order.</param>
        /// <returns>The total amount including tax.</returns>
        public static decimal CalculateTotalWithTax(decimal orderTotal)
        {
            return orderTotal + CalculateTax(orderTotal);
        }
    }

}
