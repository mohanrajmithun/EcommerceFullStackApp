namespace SaleOrderProcessingAPI.Interfaces
{
    public interface IAddressValidationService
    {
        Task<bool> IsAddressValidAsync(string address);

    }
}
