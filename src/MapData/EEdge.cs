namespace MapData
{
    public enum EEdge
    {
        None = 0,

        Top = 1,
        Right = 2,
        Bottom = 3,
        Left = 4,

        CornerTopRight = 5,
        CornerBottomRight = 6,
        CornerBottomLeft = 7,
        CornerTopLeft = 8,

        InnerCornerTopRight = 9,
        InnerCornerBottomRight = 10,
        InnerCornerBottomLeft = 11,
        InnerCornerTopLeft = 12,

        TopCutLeft = 13,
        RightCutTop = 14,
        BottomCutRight = 15,
        LeftCutBottom = 16,

        TopCutRight = 17,
        RightCutBottom = 18,
        BottomCutLeft = 19,
        LeftCutTop = 20,

        TopCut = 21,
        RightCut = 22,
        BottomCut = 23,
        LeftCut = 24
    }
}
