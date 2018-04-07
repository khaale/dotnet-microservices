using System.Collections.Generic;
using CustomerService.Api.V2.Model;
using CustomerService.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;

namespace CustomerService.Api.V2.Controllers
{
    [ApiVersion("2")]
    [Route("api/[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CustomerController : Controller
    {
        private static readonly ICollection<Customer> Customers = new List<Customer>()
        {
            new Customer(1, "Customer 1"),
            new Customer(2, "Customer 2")
        };

        private readonly CustomerOptions _options;

        public CustomerController(IOptionsSnapshot<CustomerOptions> options)
        {
            _options = options.Value;
        }

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
            Log.Information("Options: {OptionKey} = {OptionValue}", nameof(_options.Value1), _options.Value1);
            Log.Information("Options: {OptionKey} = {OptionValue}", nameof(_options.Value2), _options.Value2);
            Log.Information("Options: {OptionKey} = {OptionValue}", nameof(_options.Value3), _options.Value3);
            Log.Information("Options: {OptionKey} = {OptionValue}", nameof(_options.Value4), _options.Value4);

            return Ok(Customers);
        }
    }
}
