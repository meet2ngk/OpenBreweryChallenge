using OpenBrewery.Core.Utilities;

namespace OpenBrewery.Tests.Utilities
{
    public class GeoDistanceCalculatorTests
    {
        [Fact]
        public void GeoDistanceCalculate_SameCoordinates_ReturnsZero()
        {
            // Arrange
            double latitude = 19.9975;
            double longitude = 73.7898;

            // Act
            var result = GeoDistanceCalculator.GeoDistanceCalculate(
                latitude,
                longitude,
                latitude,
                longitude);

            // Assert
            Assert.Equal(0, result, precision: 5);
        }

        [Fact]
        public void GeoDistanceCalculate_DifferentCoordinates_ReturnsPositiveDistance()
        {
            // Arrange
            double latitude1 = 19.9975;
            double longitude1 = 73.7898;

            double latitude2 = 18.5204;
            double longitude2 = 73.8567;

            // Act
            var result = GeoDistanceCalculator.GeoDistanceCalculate(
                latitude1,
                longitude1,
                latitude2,
                longitude2);

            // Assert
            Assert.True(result > 0);
        }
    }
}