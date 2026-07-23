using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenBrewery.Core.Utilities
{
    public static class GeoDistanceCalculator
    {
        private const double EarthRadiusKm = 6371.0;


        /// Haversine formula
        public static double GeoDistanceCalculate(
                                    double userLatitude,
                                    double userLongitude,
                                    double breweryLatitude,
                                    double breweryLongitude)
        {

            var latitudeDifference =
                DegreesToRadians(breweryLatitude - userLatitude);

            var longitudeDifference =
                DegreesToRadians(breweryLongitude - userLongitude);

            var a =
                Math.Sin(latitudeDifference / 2) *
                Math.Sin(latitudeDifference / 2) +
                Math.Cos(DegreesToRadians(userLatitude)) *
                Math.Cos(DegreesToRadians(breweryLatitude)) *
                Math.Sin(longitudeDifference / 2) *
                Math.Sin(longitudeDifference / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusKm * c;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

    }
}
