using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcTracksClient
{
  public static class GeoCalculator
  {
    private const double EarthRadius = 6371000; // Радиус Земли в метрах

    public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
      // Переводим градусы в радианы
      double lat1Rad = DegreesToRadians(lat1);
      double lon1Rad = DegreesToRadians(lon1);
      double lat2Rad = DegreesToRadians(lat2);
      double lon2Rad = DegreesToRadians(lon2);

      // Разница координат
      double latDiff = lat2Rad - lat1Rad;
      double lonDiff = lon2Rad - lon1Rad;

      // Вычисляем расстояние с использованием формулы Гаверсина
      double a = Math.Pow(Math.Sin(latDiff / 2), 2) + Math.Cos(lat1Rad) * Math.Cos(lat2Rad) * Math.Pow(Math.Sin(lonDiff / 2), 2);
      double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

      // Расстояние в метрах
      double distance = EarthRadius * c;

      return distance;
    }

    public static double DegreesToRadians(double degrees)
    {
      return degrees * Math.PI / 180;
    }
    public static (double latitude, double longitude) CalculateCoordinates(
      double startingLatitude,
      double startingLongitude,
      double distance,
      double azimuth)
    {
      // Переводим координаты из градусов в радианы
      double phi1 = startingLatitude * Math.PI / 180.0;
      double lambda1 = startingLongitude * Math.PI / 180.0;
      double alpha = azimuth * Math.PI / 180.0;


      // Вычисляем новую широту и долготу
      double delta = distance / EarthRadius;
      double phi2 = Math.Asin(Math.Sin(phi1) * Math.Cos(delta) + Math.Cos(phi1) * Math.Sin(delta) * Math.Cos(alpha));
      double lambda2 = lambda1 + Math.Atan2(Math.Sin(alpha) * Math.Sin(delta) * Math.Cos(phi1), Math.Cos(delta) - Math.Sin(phi1) * Math.Sin(phi2));

      // Переводим координаты обратно в градусы
      double newLatitude = phi2 * 180.0 / Math.PI;
      double newLongitude = lambda2 * 180.0 / Math.PI;

      return (newLatitude, newLongitude);
    }
    public static double CalculateAzimuth(double latitude1, double longitude1, double latitude2, double longitude2)
    {
      // Переводим координаты из градусов в радианы
      double phi1 = latitude1 * Math.PI / 180.0;
      double lambda1 = longitude1 * Math.PI / 180.0;
      double phi2 = latitude2 * Math.PI / 180.0;
      double lambda2 = longitude2 * Math.PI / 180.0;

      // Вычисляем разницу в долготе
      double deltaLambda = lambda2 - lambda1;

      // Вычисляем азимут
      double azimuth = Math.Atan2(Math.Sin(deltaLambda), Math.Cos(phi1) * Math.Tan(phi2) - Math.Sin(phi1) * Math.Cos(deltaLambda));

      // Переводим азимут из радианов в градусы
      azimuth = azimuth * 180.0 / Math.PI;

      return azimuth;
    }
  }
}
