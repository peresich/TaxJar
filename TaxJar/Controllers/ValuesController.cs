using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace TaxJar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        const string API_KEY = "5da2f821eee4035db4771edab942a4cc";
        static HttpClient client = new HttpClient();
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<object>> Get()
        {
            string zipCode = Request.Query["zip"].ToString();
            if (zipCode == null || zipCode == string.Empty)
            {
                return new object[] { "No zip code provided." };
            }
            Task<string> result = GetTaxAsync(zipCode);
            TaxJarDTO taxJarDTO = new TaxJarDTO();
            taxJarDTO = JsonConvert.DeserializeObject<TaxJarDTO>(result.Result);

            return new object[] { result.Result, taxJarDTO };
        }

        //POST Tax Orders        
        [HttpPost]
        public ActionResult<IEnumerable<object>> Post()
        {
            Task<string> result = PostOrderAsync();
            Tax taxDTO = new Tax();
            taxDTO = JsonConvert.DeserializeObject<Tax>(result.Result);
            return new object[] { taxDTO };
        }

        static async Task<string> GetTaxAsync(string zipCode)
        {
            string rateDTO = string.Empty;
            string path = string.Empty;
            client.BaseAddress = new Uri("https://api.taxjar.com/v2/rates/" + zipCode);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_KEY);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            path = client.BaseAddress.ToString();

            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                rateDTO = response.Content.ReadAsStringAsync().Result;                                                                                  
            }
            return rateDTO;
        }

        static async Task<string> PostOrderAsync()
        {
            string order = string.Empty;
            string path = string.Empty;
            string body = string.Empty;
            client.BaseAddress = new Uri("https://api.taxjar.com/v2/transactions/orders");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", API_KEY);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            path = client.BaseAddress.ToString();
            body = CreateOrder();
            HttpResponseMessage response = new HttpResponseMessage();
            response.Headers.Add("Body", body);

            response = await client.PostAsync(path, response.Content);

            if (response.IsSuccessStatusCode)
            {
                order = response.Content.ReadAsStringAsync().Result;  //Make sure to add a reference to System.Net.Http.Formatting.dll                                                                       
            }

            return order;
        }

        static string CreateOrder()
        {
            PostTaxOrder order = new PostTaxOrder
            {
                TransactionId = 778248,
                TransactionDate = System.DateTime.Now,
                ToCountry = "US",
                ToZip = "33596",
                ToState = "FL",
                ToCity = "Valrico",
                ToStreet = "2423 Bloomingdale Rd",
                Amount = 15,
                Shipping = 0,
                SalesTax = 0.95,
                LineItems = new PostLineItem
                {
                   Quantity = 1,
                   ProductIdentifier = "12-34243-9",
                   Description = "Fuzzy Widget",
                   UnitPrice = 15.0,
                   SalesTax = 0.95
                }
            };
            return JsonConvert.SerializeObject(order);
        }
    }
}
