namespace SalesAPILibrary.Shared_Entities
{
    public class InvoiceIdGenerator
    {
        private static Random _random = new Random();

        public static int GenerateInvoiceNumber()
        {
            // Generate a random 8-digit number
            return _random.Next(10000000, 99999999);
        }
    }
}
