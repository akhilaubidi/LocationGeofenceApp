using Newtonsoft.Json;
using Timer = System.Threading.Timer;

public class LocationService
{
    private const string GeoApiUrl = "https://ipinfo.io/json"; // IP Geolocation API URL
    private readonly HttpClient _httpClient;
    private Timer _timer;

    public LocationService()
    {
        _httpClient = new HttpClient();
    }

    public void Start()
    {
        _timer = new Timer(Callback, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
    }

    private void Callback(object state)
    {
        Console.WriteLine("Callback initiated.");

        var coordinates = GetCoordinates();
        Console.WriteLine($"Fetched Coordinates - Latitude: {coordinates.Latitude}, Longitude: {coordinates.Longitude}");

        var configData = GetConfigData();

        if (configData != null)
        {
            Console.WriteLine("Configuration data read successfully.");

            var pointA = configData.geofence.pointA;
            var pointB = configData.geofence.pointB;
            var pointC = configData.geofence.pointC;
            var pointD = configData.geofence.pointD;

            double minLatitude = Math.Min(pointA.latitude, pointC.latitude);
            double maxLatitude = Math.Max(pointB.latitude, pointD.latitude);
            double minLongitude = Math.Min(pointA.longitude, pointB.longitude);
            double maxLongitude = Math.Max(pointC.longitude, pointD.longitude);

            Console.WriteLine($"Geofence bounds - MinLat: {minLatitude}, MaxLat: {maxLatitude}, MinLon: {minLongitude}, MaxLon: {maxLongitude}");

            if (coordinates.Latitude >= minLatitude && coordinates.Latitude <= maxLatitude && coordinates.Longitude >= minLongitude && coordinates.Longitude <= maxLongitude)
            {
                ShowNotification($"Latitude: {coordinates.Latitude}, Longitude: {coordinates.Longitude} is within geofence");
            }
            else
            {
                ShowNotification($"Latitude: {coordinates.Latitude}, Longitude: {coordinates.Longitude} is outside geofence");
            }
        }
        else
        {
            Console.WriteLine("Config data is null.");
        }
    }

    private ConfigData GetConfigData()
    {
        try
        {
            Console.WriteLine("Reading configuration data.");
            string filePath = Path.Combine(AppContext.BaseDirectory, "config-policy.json");
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<ConfigData>(json)!;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading config data: {ex.Message}");
            return null;
        }
    }

    private (double Latitude, double Longitude) GetCoordinates()
    {
        try
        {
            var response = _httpClient.GetStringAsync(GeoApiUrl).Result;
            dynamic data = JsonConvert.DeserializeObject(response)!;
            string[] loc = data.loc.ToString().Split(',');
            var latitude = Convert.ToDouble(loc[0]);
            var longitude = Convert.ToDouble(loc[1]);
            return (latitude, longitude);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching coordinates: {ex.Message}");
            return (0, 0);
        }
    }

    private void ShowNotification(string message)
    {
        using (var notifyIcon = new NotifyIcon())
        {
            notifyIcon.Visible = true;
            notifyIcon.Icon = SystemIcons.Information;
            notifyIcon.BalloonTipTitle = "IP Location App";
            notifyIcon.BalloonTipText = message;
            notifyIcon.ShowBalloonTip(3000); // Show notification for 3 seconds
        }

        Console.WriteLine($"Notification shown: {message}");
    }

    public void Stop()
    {
        _timer?.Dispose();
    }
}

internal class ConfigData
{
    public Geofence geofence { get; set; }
}

internal class Geofence
{
    public Point pointA { get; set; }
    public Point pointB { get; set; }
    public Point pointC { get; set; }
    public Point pointD { get; set; }
}

internal class Point
{
    public double latitude { get; set; }
    public double longitude { get; set; }
}
