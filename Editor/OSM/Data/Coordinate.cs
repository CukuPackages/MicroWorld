namespace Cuku.MicroWorld
{
    [System.Serializable]
    public struct Coordinate
    {
        public double Lat;
        public double Lon;

        public Coordinate(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }
    }
}