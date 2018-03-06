using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomerService.Model;
using Microsoft.AspNetCore.Mvc;

namespace CustomerService.Controllers
{
    [Route("api/[controller]")]
    public class CustomerController : Controller
    {
        private static readonly ICollection<Customer> Customers = new List<Customer>()
        {
            new Customer(1, "Customer 1"),
            new Customer(2, "Customer 2")
        };

        /// <summary>
        /// Gets a list of customers
        /// </summary>
        /// <response code="200">A list of customers</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Customer>), 200)]
        [ProducesResponseType(typeof(string), 500)]
        public IActionResult Get()
        {
            return Ok(Customers);
        }
    }
}
