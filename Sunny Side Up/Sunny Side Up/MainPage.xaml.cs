using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Sunny_Side_Up
{
    public partial class MainPage : ContentPage
    {
        private const string RapidApiKey = "f8cd9c8bd9msh6e6d921717b0838p13ad61jsnc3c23603d782";
        private const string RapidApiHost = "weatherapi-com.p.rapidapi.com";

        public MainPage()
        {
            InitializeComponent();
            GetCurrentWeatherAsync();
        }

        private async void OnSearchButtonClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(CitySearchEntry.Text))
            {
                var weatherData = await GetWeatherByCityAsync(CitySearchEntry.Text);
                UpdateUIWithWeatherData(weatherData);
            }
        }

        private async void GetCurrentWeatherAsync()
        {
            var location = await GetCurrentLocationAsync();
            if (location != null)
            {
                var weatherData = await GetWeatherDataAsync(location.Latitude, location.Longitude);
                UpdateUIWithWeatherData(weatherData);
            }
        }

        private async Task<Sunny_Side_Up.Location> GetCurrentLocationAsync()
        {
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();
                if (location == null)
                {
                    location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));
                }

                // Convert Xamarin.Essentials.Location to your SunnySideUp.Location
                return ConvertToSunnySideUpLocation(location);
            }
            catch (Exception)
            {
                // Handle exceptions (e.g., location permission denied)
                return null;
            }
        }

        // Conversion method
        public static Sunny_Side_Up.Location ConvertToSunnySideUpLocation(Xamarin.Essentials.Location xamarinLocation)
        {
            return new Sunny_Side_Up.Location
            {
                Latitude = xamarinLocation.Latitude,
                Longitude = xamarinLocation.Longitude
            };
        }


        private async Task<WeatherResponse> GetWeatherDataAsync(double latitude, double longitude)
        {
            string url = $"https://weatherapi-com.p.rapidapi.com/current.json?q={latitude},{longitude}";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("x-rapidapi-key", RapidApiKey);
                client.DefaultRequestHeaders.Add("x-rapidapi-host", RapidApiHost);

                var response = await client.GetStringAsync(url);
                return JsonConvert.DeserializeObject<WeatherResponse>(response);
            }
        }

        private async Task<WeatherResponse> GetWeatherByCityAsync(string cityName)
        {
            string url = $"https://weatherapi-com.p.rapidapi.com/current.json?q={cityName}";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("x-rapidapi-key", RapidApiKey);
                client.DefaultRequestHeaders.Add("x-rapidapi-host", RapidApiHost);

                var response = await client.GetStringAsync(url);
                return JsonConvert.DeserializeObject<WeatherResponse>(response);
            }
        }

        private async Task<ForecastResponse> GetForecastDataAsync(string cityName)
        {
            string url = $"https://weatherapi-com.p.rapidapi.com/forecast.json?q={cityName}&days=3";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("x-rapidapi-key", RapidApiKey);
                client.DefaultRequestHeaders.Add("x-rapidapi-host", RapidApiHost);

                var response = await client.GetStringAsync(url);
                return JsonConvert.DeserializeObject<ForecastResponse>(response);
            }
        }

        private void UpdateUIWithWeatherData(WeatherResponse weatherData)
        {
            CityLabel.Text = weatherData.Location.Name;
            TemperatureLabel.Text = $"{weatherData.Current.TempC}°C"; // Use 'temp_c' from API
            WeatherDescription.Text = weatherData.Current.Condition.Text;
            WeatherIcon.Source = GetWeatherIcon(weatherData.Current.Condition.Icon);

            UVIndexLabel.Text = $"{weatherData.Current.Uv}"; // UV Index
            HumidityLabel.Text = $"{weatherData.Current.Humidity}%";
            WindLabel.Text = $"{weatherData.Current.WindMph} mph";
            RainfallLabel.Text = $"{weatherData.Current.PrecipMm} mm"; // Use actual rainfall data if available

            // Load forecast data
            LoadForecastData(weatherData.Location.Name);
        }

        private async void LoadForecastData(string cityName)
        {
            var forecastData = await GetForecastDataAsync(cityName);
            DailyForecastContainer.Children.Clear();

            foreach (var day in forecastData.Forecast.Forecastday)
            {
                var forecastFrame = new Frame
                {
                    Padding = 10,
                    BackgroundColor = Color.FromHex("#FEF3C7"),
                    CornerRadius = 10,
                    Margin = new Thickness(5, 0)
                };

                var stackLayout = new StackLayout
                {
                    Orientation = StackOrientation.Vertical,
                    HorizontalOptions = LayoutOptions.Center
                };

                var dateLabel = new Label
                {
                    Text = day.Date,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center
                };

                var tempLabel = new Label
                {
                    Text = $"{day.Day.MaxTempC}°C / {day.Day.MinTempC}°C",
                    HorizontalOptions = LayoutOptions.Center
                };

                var conditionLabel = new Label
                {
                    Text = day.Day.Condition.Text,
                    HorizontalOptions = LayoutOptions.Center
                };

                stackLayout.Children.Add(dateLabel);
                stackLayout.Children.Add(tempLabel);
                stackLayout.Children.Add(conditionLabel);

                forecastFrame.Content = stackLayout;
                DailyForecastContainer.Children.Add(forecastFrame);
            }
        }

        private string GetWeatherIcon(string iconCode)
        {
            return $"https:{iconCode}"; // Use the proper URL format for Weather API
        }
    }

    public class WeatherResponse
    {
        public Location Location { get; set; }
        public Current Current { get; set; }
    }

    public class Location
    {
        public string Name { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public double Latitude { get; set; } // Added Latitude
        public double Longitude { get; set; } // Added Longitude
        public string TzId { get; set; }
        public string Localtime { get; set; }
    }

    public class Current
    {
        [JsonProperty("temp_c")] // Ensure this matches the API response
        public double TempC { get; set; }

        public Condition Condition { get; set; }
        public int Humidity { get; set; }
        [JsonProperty("wind_mph")] // Ensure this matches the API response
        public double WindMph { get; set; }
        [JsonProperty("precip_mm")] // Ensure this matches the API response
        public double PrecipMm { get; set; }
        public double Uv { get; set; }
        public string LastUpdated { get; set; }
    }

    public class Condition
    {
        public string Text { get; set; }
        public string Icon { get; set; }
    }

    public class ForecastResponse
    {
        public Forecast Forecast { get; set; }
    }

    public class Forecast
    {
        public ForecastDay[] Forecastday { get; set; }
    }

    public class ForecastDay
    {
        public string Date { get; set; }
        public Day Day { get; set; }
    }

    public class Day
    {
        [JsonProperty("maxtemp_c")] // Ensure this matches the API response
        public double MaxTempC { get; set; }

        [JsonProperty("mintemp_c")] // Ensure this matches the API response
        public double MinTempC { get; set; }

        public Condition Condition { get; set; }
    }
}
