namespace ChilLaxFrontEnd.Models.DTO
{
    public class CustomerServicePagingDTO
    {
        public int TotalPages { get; set; }
        public List<CustomerService> PointRecords { get; set; }
    }
}
