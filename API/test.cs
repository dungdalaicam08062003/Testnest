//using System;
//using System.Collections.Generic;
//using System.Text;
//using RestSharp;

//namespace Websitecomputer.API
//{
//    internal class test
//    {
//        public static void main(string[] args)
//        {
//            var Client = new RestClient("http://localhost:5000");
//            var orderDetail = new
//            {
//                ClientID = "CLI-0001",
//                ProductID = "P001",
//                Quantity = 1,
//                Address = "Da nang",
//                PhoneNumber = "0345612321"
//            };
//            var request = new RestRequest($"orders/{orderDetail.ClientID}/{orderDetail.ProductID}")
//                .AddParameter("quantity", orderDetail.Quantity)
//                .AddParameter("address", orderDetail.Address)
//                .AddParameter("phoneNumber", orderDetail.PhoneNumber);
//            var statusCode = Client.PostJsonAsync(request, cancellationToken);
//        }
//    }
//}
