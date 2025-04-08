using System.Net.Http;

namespace FlightSearching
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddSession();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseSession();

            app.MapRazorPages();

            CallAPI();

            app.Run();
        }
        private static async Task CallAPI()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string apiURL = "https://localhost:44396/";
                HttpResponseMessage responseMessage = await httpClient.GetAsync(apiURL);
                if (responseMessage.IsSuccessStatusCode)
                {
                    string responseContent = await responseMessage.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                }
                else
                {
                    Console.WriteLine("Error: {0}", responseMessage.StatusCode);
                }
            }
        }
    }


}