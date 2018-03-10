namespace CustomerService.Api.V2.Model
{
    public class Customer
    {
        public Customer(int id, string name)
        {
            Id = id;
            Name = name;
            FullName = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
    }
}