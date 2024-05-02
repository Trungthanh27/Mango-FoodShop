using Mango.Web.Models;
using Mango.Web.Service;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderServices;
        public OrderController(IOrderService orderServices)
        {
            _orderServices = orderServices;
        }

        [Authorize]
        public IActionResult OrderIndex()
        {
            return View();
        }

        [HttpPost("OrderReadyForPickup")]
        public async Task<IActionResult> OrderReadyForPickup(int orderID)
        {
            ResponseDto response = _orderServices.UpdateOrderStatus(orderID, SD.Status_ReadyForPickup).GetAwaiter().GetResult();

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Status updated successfully";
                return RedirectToAction(nameof(OrderDetail), new { orderID = orderID });    
            }
            return View();
        }

        [HttpPost("CompleteOrder")]
        public async Task<IActionResult> CompleteOrder(int orderID)
        {
            ResponseDto response = _orderServices.UpdateOrderStatus(orderID, SD.Status_Completed).GetAwaiter().GetResult();

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Status updated successfully";
                return RedirectToAction(nameof(OrderDetail), new { orderID = orderID });
            }
            return View();
        }

        [HttpPost("CancelOrder")]
        public async Task<IActionResult> CancelOrder(int orderID)
        {
            ResponseDto response = _orderServices.UpdateOrderStatus(orderID, SD.Status_Cancelled).GetAwaiter().GetResult();

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Status updated successfully";
                return RedirectToAction(nameof(OrderDetail), new { orderID = orderID });
            }
            return View();
        }

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeaderDto> list;
            string userId = "";
            if (!User.IsInRole(SD.RoleAdmin))
            {
                userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
            }

            ResponseDto response = _orderServices.GetAllOrder(userId).GetAwaiter().GetResult();

            if (response !=null && response.IsSuccess)
            {
                list = JsonConvert.DeserializeObject<List<OrderHeaderDto>>(Convert.ToString(response.Result));
                switch(status)
                {
                    case "approved":
                        list = list.Where(u=>u.Status == SD.Status_Approved).ToList(); break;
                    case "readyforpickup":
                        list = list.Where(u => u.Status == SD.Status_ReadyForPickup).ToList(); break;
                    case "cancelled":
                        list = list.Where(u => u.Status == SD.Status_Cancelled || u.Status == SD.Status_Refunded).ToList(); break;
                    default:
                        break;
                }
            }
            else
            {
                list= new List<OrderHeaderDto>();
            }
            return Json(new { data = list });
        }

        
        public async Task<IActionResult> OrderDetail(int orderID)
		{
            OrderHeaderDto orderHeaderDto = new OrderHeaderDto();
            string userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
			
            ResponseDto response = _orderServices.GetOrder(orderID).GetAwaiter().GetResult();

			if (response != null && response.IsSuccess)
			{
				orderHeaderDto = JsonConvert.DeserializeObject<OrderHeaderDto>(Convert.ToString(response.Result));
			}

			if (!User.IsInRole(SD.RoleAdmin) && userId!= orderHeaderDto.UserId)
            {
				return NotFound();
			}
				
            return View(orderHeaderDto);
		}
	}
}
