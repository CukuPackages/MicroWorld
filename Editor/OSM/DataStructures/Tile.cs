namespace Cuku.MicroWorld
{
    public struct Tile
    {
        public string Name;
        public Coordinate TopLeft;
        public Coordinate BottomRight;

        public Tile(string name, Coordinate topLeft, Coordinate bottomRight)
        {
            Name = name;
            TopLeft = topLeft;
            BottomRight = bottomRight;
        }
    }
}