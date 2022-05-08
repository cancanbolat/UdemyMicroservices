using FreeCourse.Shared.Dtos;
using FreeCourse.Shared.Services;
using FreeCourse.Web.Models;
using FreeCourse.Web.Models.FakePayments;
using FreeCourse.Web.Models.Orders;
using FreeCourse.Web.Services.Interfaces;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace FreeCourse.Web.Services
{
    public class OrderService : IOrderService
    {
        /* Senkron iletişim
        önce ödeme sonra sipariş.
        ihtiyaçlar: ödeme servisi, ürün bilgileri için sepet servisi
         */

        private readonly HttpClient httpClient;
        private readonly IPaymentService paymentService;
        private readonly IBasketService basketService;
        private readonly ISharedIdentityService sharedIdentityService;

        public OrderService(HttpClient httpClient,
            IPaymentService paymentService,
            IBasketService basketService,
            ISharedIdentityService sharedIdentityService)
        {
            this.httpClient = httpClient;
            this.paymentService = paymentService;
            this.basketService = basketService;
            this.sharedIdentityService = sharedIdentityService;
        }

        public async Task<OrderCreatedViewModel> CreateOrder(CheckOutInfoInput checkOutInfoInput)
        {
            // sepetteki dataları alıyoruz
            var basket = await basketService.Get();

            // ödeme input
            var paymentInfoInput = new PaymentInfoInput()
            {
                CardName = checkOutInfoInput.CardName,
                CardNumber = checkOutInfoInput.CardNumber,
                CardExpiration = checkOutInfoInput.CardExpiration,
                CVV = checkOutInfoInput.CVV,
                TotalPrice = basket.TotalPrice
            };

            // ödeme işlemi
            var responsePayment = await paymentService.ReceivePayment(paymentInfoInput);

            if (!responsePayment)
            {
                return new OrderCreatedViewModel() { Error = "Ödeme alınamadı", IsSuccessful = false };
            }

            //sipariş oluşturma
            var orderCreateInput = new CreateOrderInput()
            {
                BuyerId = sharedIdentityService.GetUserId,
                Address = new AddressCreateInput
                {
                    Province = checkOutInfoInput.Province,
                    District = checkOutInfoInput.District,
                    Street = checkOutInfoInput.Street,
                    Line = checkOutInfoInput.Line,
                    ZipCode = checkOutInfoInput.ZipCode
                }
            };

            basket.BasketItems.ForEach(x =>
            {
                var orderItem = new OrderItemCreateInput
                {
                    ProductId = x.CourseId,
                    Price = x.GetCurrentPrice,
                    PictureUrl = "", //course service'e bağlanıp alınabilir.
                    ProductName = x.CourseName
                };

                orderCreateInput.OrderItems.Add(orderItem);
            });

            var response = await httpClient.PostAsJsonAsync<CreateOrderInput>("orders", orderCreateInput);

            if (!response.IsSuccessStatusCode)
            {
                return new OrderCreatedViewModel() { Error = "Sipariş oluşturulamadı", IsSuccessful = false };
            }

            var orderCreatedViewModel = await response.Content.ReadFromJsonAsync<Response<OrderCreatedViewModel>>();

            orderCreatedViewModel.Data.IsSuccessful = true;
            await basketService.Delete();
            return orderCreatedViewModel.Data;
        }

        public async Task<List<OrderViewModel>> GetOrder()
        {
            var response = await httpClient.GetFromJsonAsync<Response<List<OrderViewModel>>>("orders");

            return response.Data;
        }

        //asenkron olacak
        public async Task<OrderSuspendViewModel> SuspendOrder(CheckOutInfoInput checkOutInfoInput)
        {
            // sepetteki dataları alıyoruz
            var basket = await basketService.Get();

            //sipariş oluşturma
            var orderCreateInput = new CreateOrderInput()
            {
                BuyerId = sharedIdentityService.GetUserId,
                Address = new AddressCreateInput
                {
                    Province = checkOutInfoInput.Province,
                    District = checkOutInfoInput.District,
                    Street = checkOutInfoInput.Street,
                    Line = checkOutInfoInput.Line,
                    ZipCode = checkOutInfoInput.ZipCode
                }
            };

            //sipariş oluşturma devamı
            basket.BasketItems.ForEach(x =>
            {
                var orderItem = new OrderItemCreateInput
                {
                    ProductId = x.CourseId,
                    Price = x.GetCurrentPrice,
                    PictureUrl = "", //course service'e bağlanıp alınabilir.
                    ProductName = x.CourseName
                };

                orderCreateInput.OrderItems.Add(orderItem);
            });

            // ödeme işlemi
            var paymentInfoInput = new PaymentInfoInput()
            {
                CardName = checkOutInfoInput.CardName,
                CardNumber = checkOutInfoInput.CardNumber,
                CardExpiration = checkOutInfoInput.CardExpiration,
                CVV = checkOutInfoInput.CVV,
                TotalPrice = basket.TotalPrice,
                Order = orderCreateInput
            };

            // ödeme işlemi devamı
            var responsePayment = await paymentService.ReceivePayment(paymentInfoInput);

            if (!responsePayment)
            {
                return new OrderSuspendViewModel() { Error = "Ödeme alınamadı", IsSuccessful = false };
            }

            await basketService.Delete();

            return new OrderSuspendViewModel() { IsSuccessful = true };
        }
    }
}
