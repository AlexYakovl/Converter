using RestSharp;
using System.Text.Json;

namespace ConverterAPI
{
    public class API
    {
        private RestClient client;

        public API()
        {
            var options = new RestClientOptions("https://www.cbr-xml-daily.ru/");
            client = new RestClient(options);
        }

        public async Task<RatesJson> CallAPI(DateTime date)
        {
            string resource = "daily_json.js";

            if (date != DateTime.Today)
                resource = GetArchiveUrl(date);

            try
            {
                var request = new RestRequest(resource);
                var response = await client.ExecuteAsync(request);

                if (response == null)
                    throw new InvalidOperationException("Failed to make API call");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return await CallAPI(date.AddDays(-1));

                ValidateResponse(response);

                var rates = DeserializeResponse(response.Content);
                AddRubleRate(rates);

                return rates;
            }
            catch (StackOverflowException e)
            {
                Console.WriteLine($"Couldn't find rates for the given date: {e.Message}");
                return new RatesJson();
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred while calling the API: {e.Message}");
                return new RatesJson();
            }
        }

        private void ValidateResponse(RestResponse response)
        {
            if (!response.IsSuccessful)
                throw new InvalidOperationException("API call was not successful");

            if (string.IsNullOrWhiteSpace(response.Content))
                throw new InvalidOperationException("API response is empty");
        }

        private RatesJson DeserializeResponse(string content)
        {
            var rates = JsonSerializer.Deserialize<RatesJson>(content);
            if (rates == null)
                throw new InvalidOperationException("Couldn't deserialize API response");
            return rates;
        }

        private void AddRubleRate(RatesJson rates)
        {
            rates.Rates.Add("RUB", new Valute
            {
                Name = "Российский рубль",
                Nominal = 1,
                Value = 1
            });
        }

        private static string GetArchiveUrl(DateTime date)
        {
            string year = date.Year.ToString();
            string month = date.Month.ToString("00");
            string day = date.Day.ToString("00");

            return $"/archive//{year}//{month}//{day}//daily_json.js";
        }

        public static double ConvertCurrencies(Valute fromCurrency, Valute toCurrency, double inputAmount)
        {
            if (fromCurrency != null && toCurrency != null && inputAmount != 0)
            {
                double fromCurrencyRate = fromCurrency.Value / fromCurrency.Nominal;
                double toCurrencyRate = toCurrency.Value / toCurrency.Nominal;

                double conversionRate = fromCurrencyRate / toCurrencyRate;
                double converted = conversionRate * inputAmount;
                return converted;
            }
            else
            {
                return 0;
            }
        }
    }
}