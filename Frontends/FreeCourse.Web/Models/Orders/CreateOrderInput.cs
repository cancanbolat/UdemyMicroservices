using System.Collections.Generic;

namespace FreeCourse.Web.Models.Orders
{
    public class CreateOrderInput
    {
        public CreateOrderInput()
        {
            OrderItems = new List<OrderItemCreateInput>();
            //hemen boş bir liste oluşturulsun. sonra içine eklenenler olacak.
        }

        public string BuyerId { get; set; }
        public List<OrderItemCreateInput> OrderItems { get; set; }
        public AddressCreateInput Address { get; set; }
    }
}
