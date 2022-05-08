using FreeCourse.Web.Models;
using FreeCourse.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace FreeCourse.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IBasketService basketService;
        private readonly IOrderService orderService;

        public OrderController(IBasketService basketService, IOrderService orderService)
        {
            this.basketService = basketService;
            this.orderService = orderService;
        }

        public async Task<IActionResult> Checkout()
        {
            //sepeti alıyoruz
            var basket = await basketService.Get();
            ViewBag.basket = basket;

            return View(new CheckOutInfoInput());
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckOutInfoInput checkOutInfoInput)
        {
            //1. yol (senkron iletişim)
            //var orderStatus = await orderService.CreateOrder(checkOutInfoInput);

            //2.yol (asenkron iletişim)
            var orderStatus = await orderService.SuspendOrder(checkOutInfoInput);

            if (!orderStatus.IsSuccessful)
            {
                //sepeti alıyoruz
                var basket = await basketService.Get();
                ViewBag.basket = basket;

                ViewBag.error = orderStatus.Error;
                return View();
            }

            //senkron
            //return RedirectToAction(nameof(SuccessfulCheckout), new { orderId = orderStatus.OrderId });

            //asenkron
            return RedirectToAction(nameof(SuccessfulCheckout), new { orderId = new Random().Next(1, 1000) });
        }

        public IActionResult SuccessfulCheckout(int orderId)
        {
            ViewBag.orderId = orderId;
            return View();
        }

        public async Task<IActionResult> CheckoutHistory()
        {
            return View(await orderService.GetOrder());
        }
    }
}
