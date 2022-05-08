using FreeCourse.Shared.Dtos;
using FreeCourse.Web.Models.Baskets;
using FreeCourse.Web.Services.Interfaces;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace FreeCourse.Web.Services
{
    public class BasketService : IBasketService
    {
        private readonly HttpClient httpClient;
        private readonly IDiscountService discountService;

        public BasketService(HttpClient httpClient, IDiscountService discountService)
        {
            this.httpClient = httpClient;
            this.discountService = discountService;
        }

        public async Task AddBasketItem(BasketItemViewModel basketItemViewModel)
        {
            var basket = await Get();

            if (basket != null)
            {
                if (!basket.BasketItems.Any(x => x.CourseId == basketItemViewModel.CourseId))
                {
                    basket.BasketItems.Add(basketItemViewModel);
                }
            }
            else
            {
                basket = new BasketViewModel();

                basket.BasketItems.Add(basketItemViewModel);
            }

            await SaveOrUpdate(basket);
        }


        public async Task<bool> Delete()
        {
            var result = await httpClient.DeleteAsync("baskets");
            return result.IsSuccessStatusCode;
        }

        public async Task<BasketViewModel> Get()
        {
            var response = await httpClient.GetAsync("baskets");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var basketViewModel = await response.Content.ReadFromJsonAsync<Response<BasketViewModel>>();

            return basketViewModel.Data;
        }

        public async Task<bool> RemoveBasketItem(string courseId)
        {
            var basket = await Get();

            if (basket == null)
            {
                return false;
            }

            var deleteBasketItem = basket.BasketItems.FirstOrDefault(x => x.CourseId == courseId);
            if (deleteBasketItem == null)
            {
                return false;
            }

            var deleteResult = basket.BasketItems.Remove(deleteBasketItem);
            if (!deleteResult)
            {
                return false;
            }

            if (!basket.BasketItems.Any())
            {
                basket.DiscountCode = null;
            }

            return await SaveOrUpdate(basket);
        }

        public async Task<bool> SaveOrUpdate(BasketViewModel basketViewModel)
        {
            var response = await httpClient.PostAsJsonAsync<BasketViewModel>("baskets", basketViewModel);

            return response.IsSuccessStatusCode;
        }
        
        public async Task<bool> ApplyDiscount(string discountCode)
        {
            //kullanıcı indirim kuponunu girdi
            /*
            * 2. kez indirim uygulanırsa 1.sini iptal et (CancelApplyDiscount)
            * basketi aldık. basket veya basketin discountu null ise return false
            * indirim kodunu servise gönderdik. 
            --(Discount microservice token'dan kullanı id'si ile db'ye gidip indirim kodu kontrolü yapıyor)
            Rate ve Code'i alıp sepeti güncelliyoruz.
             */

            await CancelApplyDiscount();

            var basket = await Get();
            if (basket == null)
            {
                return false;
            }

            var hasDiscount = await discountService.GetDiscount(discountCode);
            if (hasDiscount == null)
            {
                return false;
            }

            basket.DiscountRate = hasDiscount.Rate;
            basket.DiscountCode = hasDiscount.Code;

            await SaveOrUpdate(basket);

            return true;
        }

        public async Task<bool> CancelApplyDiscount()
        {
            var basket = await Get();

            if (basket == null || basket.DiscountCode == null)
            {
                return false;
            }

            basket.DiscountCode = null;
            await SaveOrUpdate(basket);

            return true;
        }
    }
}
