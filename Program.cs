using System.Text.Json;
using Npgsql;

//Define cities
var cities = new List<(string City, double Lat, double Lon)>
{
    ("Mumbai", 19.0760, 72.8777),
    ("Delhi", 28.6139, 77.2090),
    ("Bangalore", 12.9716, 77.5946),
    ("Chennai", 13.0827, 80.2707),
    ("Hyderabad", 17.3850, 78.4867),
    ("Pune", 18.5204, 73.8567),
    ("Kolkata", 22.5726, 88.3639),
    ("Ahmedabad", 23.0225, 72.5714),
    ("Jaipur", 26.9124, 75.7873),
    ("Lucknow", 26.8467, 80.9462)
};

//Loop through each city
var tasks = cities.Select(async city =>
{
    var (temp, condition) = await GetWeather(city.Lat, city.Lon);

    Console.WriteLine($"{city.City}: {temp}, condition: {condition}");

    if (temp != null)
    {
        await SaveWeather(city.City, temp.Value, condition);
    }
});

await Task.WhenAll(tasks);

//Function to call API

static async Task<(double?, string)> GetWeather(double lat, double lon)
{
    var httpClient = new HttpClient();
    var url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current_weather=true";

    int retries = 3;

    for (int i = 0; i < retries; i++)
    {
        try
        {
            var response = await httpClient.GetStringAsync(url);
            var json = JsonDocument.Parse(response);

            var weather = json.RootElement.GetProperty("current_weather");

            double temp = weather.GetProperty("temperature").GetDouble();
            int code = weather.GetProperty("weathercode").GetInt32();
            string condition = code.ToString();

            return (temp, condition);
        }
        catch
        {
            if (i == retries - 1) return (null, null);
            await Task.Delay(1000);
        }
    }

    return (null, null);
}
static async Task SaveWeather(string city, double temp, string condition)
{
    var connString = Environment.GetEnvironmentVariable("DB_CONNECTION");

    await using var conn = new NpgsqlConnection(connString);
    await conn.OpenAsync();

    var cmd = new NpgsqlCommand(
    "INSERT INTO weather_data (city_name, temperature, weather_condition, recorded_at) VALUES (@city,@temp,@condition,@time) ON CONFLICT (city_name, recorded_at) DO NOTHING;",
    conn);

    cmd.Parameters.AddWithValue("city", city);
    cmd.Parameters.AddWithValue("temp", temp);
    cmd.Parameters.AddWithValue("condition", condition ?? "unknown");
    cmd.Parameters.AddWithValue("time", DateTime.UtcNow.AddMilliseconds(new Random().Next(1, 1000)));

    await cmd.ExecuteNonQueryAsync();
}