﻿using FreeCourse.Web.Models.Orders;

namespace FreeCourse.Web.Models.FakePayments
{
    public class PaymentInfoInput
    {
        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public string CardExpiration { get; set; }
        public string CVV { get; set; }
        public decimal TotalPrice { get; set; }
        public CreateOrderInput Order { get; set; }
    }
}
