namespace Cuku.MicroWorld
{
    [System.Serializable]
    public struct LatLon
    {
        public double Lat;
        public double Lon;

        public LatLon(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }
    }
}