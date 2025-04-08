using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace FlightSearching.Pages
{
    public class TestingModel : PageModel
    {
        private readonly IOptions<SwaggerOptions> _swaggerOptions;
        public TestingModel(IOptions<SwaggerOptions> swaggerOptions)
        {
            _swaggerOptions = swaggerOptions;
        }
        public void OnGet()
        {
        }
        public void OnPost()
        {
            // Handle form submission and redirect to the API page
            string swaggerURL = Url.Content("~/swagger/index.html");
            Response.Redirect(swaggerURL);
        }
    }
}
