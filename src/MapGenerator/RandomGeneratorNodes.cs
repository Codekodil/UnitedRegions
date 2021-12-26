using MapData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MapGenerator
{
    public partial class RandomGenerator
    {
        private void AddNodeToMap(MapNode node)
        {
            AddVillageToMap(node);
        }


        private enum AddVillageToMapEntry
        {
            Road, House, PokeCenter, Shop, Tree
        }
        private void AddVillageToMap(MapNode node)
        {
            const int RowHeight = 7;
            var rows = new List<List<AddVillageToMapEntry>>();
            var rowCount = PatternHelper.RandomElement(_randomNodes, 2, 2, 3);
            var houseIndices = new List<Tuple<int, int>>();
            for (var y = 0; y < rowCount; ++y)
            {
                var row = new List<AddVillageToMapEntry>();
                var rowEntries = PatternHelper.RandomElement(_randomNodes, 2, 3, 3, 4);
                for (var x = 0; x < rowEntries; ++x)
                {
                    row.Add(PatternHelper.RandomElement(_randomNodes,
                        AddVillageToMapEntry.House,
                        AddVillageToMapEntry.House,
                        AddVillageToMapEntry.House,
                        AddVillageToMapEntry.House,
                        AddVillageToMapEntry.Tree));
                }
                var roadPosition = _randomNodes.Next(1, row.Count);
                row.Insert(roadPosition, AddVillageToMapEntry.Road);
                for (int x = 0; x < row.Count; ++x)
                    if (row[x] == AddVillageToMapEntry.House)
                        houseIndices.Add(Tuple.Create(x, y));
                rows.Add(row);
            }
            ReplaceHouse(AddVillageToMapEntry.PokeCenter);
            ReplaceHouse(AddVillageToMapEntry.Shop);
            void ReplaceHouse(AddVillageToMapEntry entry)
            {
                if (houseIndices.Count >= 1)
                {
                    var replaceIndex = _randomNodes.Next(houseIndices.Count);
                    var coord = houseIndices[replaceIndex];
                    houseIndices.RemoveAt(replaceIndex);
                    rows[coord.Item2][coord.Item1] = entry;
                }
            }
            var villageHeight = rowCount * RowHeight;
            var villageWidth = rows.Max(RowWidth);
            villageHeight += villageHeight & 1;
            villageWidth += villageWidth & 1;

            var cornerX = node.Center.X - villageWidth / 2;
            var cornerY = node.Center.Y - villageHeight / 2;

            cornerX += 1 - (cornerX & 1);
            cornerY += 1 - (cornerY & 1);

            for (var x = 0; x < villageWidth; ++x)
                for (var y = 0; y < villageHeight; ++y)
                    Map.SetTile(x + cornerX, y + cornerY, ETile.None);

            foreach (var coord in PatternHelper.Border(cornerX, cornerY, villageWidth, villageHeight))
                Map.SetTile(coord.X, coord.Y, ETile.None);
            foreach (var coord in PatternHelper.Border(cornerX - 1, cornerY - 1, villageWidth + 2, villageHeight + 2, 2))
                Map.SetTile(coord.X, coord.Y, ((coord.X & 1) == 0 && (coord.Y & 1) == 0) ? ETile.Tree : ETile.Blocked);

            for (var y = 0; y < rowCount; ++y)
            {
                for (var x = 0; x < villageWidth; ++x)
                {
                    Map.SetGround(x + cornerX, y * RowHeight + cornerY, EGround.Sand);
                    Map.SetGround(x + cornerX, y * RowHeight + 1 + cornerY, EGround.Sand);
                }
                var widthIndex = 0;
                foreach (var v in rows[y])
                {
                    switch (v)
                    {
                        case AddVillageToMapEntry.Road:
                            for (var ey = 2; ey < RowHeight; ++ey)
                            {
                                Map.SetGround(widthIndex + cornerX, y * RowHeight + cornerY + ey, EGround.Sand);
                                Map.SetGround(widthIndex + cornerX + 1, y * RowHeight + cornerY + ey, EGround.Sand);
                            }
                            break;
                        case AddVillageToMapEntry.House:
                            for (var ex = 0; ex < 4; ++ex)
                                for (var ey = 3; ey < RowHeight; ++ey)
                                    Map.SetTile(widthIndex + cornerX + ex, y * RowHeight + cornerY + ey, ETile.Blocked);
                            Map.SetTile(widthIndex + cornerX + 1, y * RowHeight + cornerY + 3, ETile.House);
                            break;
                        case AddVillageToMapEntry.PokeCenter:
                            for (var ex = 0; ex < 5; ++ex)
                                for (var ey = 3; ey < RowHeight; ++ey)
                                    Map.SetTile(widthIndex + cornerX + ex, y * RowHeight + cornerY + ey, ETile.Blocked);
                            Map.SetTile(widthIndex + cornerX + 2, y * RowHeight + cornerY + 3, ETile.HealCenter);
                            break;
                        case AddVillageToMapEntry.Shop:
                            for (var ex = 0; ex < 4; ++ex)
                                for (var ey = 3; ey < RowHeight; ++ey)
                                    Map.SetTile(widthIndex + cornerX + ex, y * RowHeight + cornerY + ey, ETile.Blocked);
                            Map.SetTile(widthIndex + cornerX + 1, y * RowHeight + cornerY + 3, ETile.Shop);
                            break;
                        case AddVillageToMapEntry.Tree:
                            for (var ex = 0; ex < 2; ++ex)
                                for (var ey = 3; ey < RowHeight; ++ey)
                                    Map.SetTile(widthIndex + cornerX + ex, y * RowHeight + cornerY + ey, ETile.Blocked);
                            Map.SetTile(widthIndex + cornerX, y * RowHeight + cornerY + 3, ETile.Tree);
                            Map.SetTile(widthIndex + cornerX, y * RowHeight + cornerY + 5, ETile.Tree);
                            break;
                    }
                    widthIndex += EntryWidth(v) + 1;
                }
            }

            foreach (var connection in node.Connections)
            {
                var difference = connection.Edge.Center - node.Center;
                if (Math.Abs(difference.X) > Math.Abs(difference.Y))
                {
                    if (difference.X > 0)
                    {
                        var rightPath = _randomNodes.Next(rowCount);
                        var y = rightPath * RowHeight + cornerY;
                        foreach (var coord in PatternHelper.Rect(cornerX + villageWidth, y, 3, 2))
                            Map.SetGround(coord.X, coord.Y, EGround.Sand);
                        foreach (var coord in PatternHelper.Rect(cornerX + villageWidth + 1, y - 2 + (y & 1), 2, 6 - (y & 1) * 2))
                            Map.SetTile(coord.X, coord.Y, ETile.None);
                        connection.Dock.X = cornerX + villageWidth + 3;
                        connection.Dock.Y = y;
                        connection.Height = node.Height;
                        connection.Direction.X = 1;
                    }
                    else
                    {
                        var leftPath = _randomNodes.Next(rowCount);
                        var y = leftPath * RowHeight + cornerY;
                        foreach (var coord in PatternHelper.Rect(cornerX - 3, y, 3, 2))
                            Map.SetGround(coord, EGround.Sand);
                        foreach (var coord in PatternHelper.Rect(cornerX - 3, y - 2 + (y & 1), 2, 6 - (y & 1)))
                            Map.SetTile(coord, ETile.None);
                        connection.Dock.X = cornerX - 4;
                        connection.Dock.Y = y;
                        connection.Height = node.Height;
                        connection.Direction.X = -1;
                    }
                }
                else
                {
                    if (difference.Y > 0)
                    {
                        var topPath = 0;
                        foreach (var v in rows[rows.Count - 1])
                        {
                            if (v == AddVillageToMapEntry.Road) break;
                            topPath += EntryWidth(v) + 1;
                        }
                        var x = topPath + cornerX;
                        foreach (var coord in PatternHelper.Rect(x, cornerY + villageHeight, 2, 3))
                            Map.SetGround(coord, EGround.Sand);
                        foreach (var coord in PatternHelper.Rect(x - 2 + (x & 1), cornerY + villageHeight + 1, 6 - (x & 1) * 2, 2))
                            Map.SetTile(coord, ETile.None);
                        connection.Dock.X = x;
                        connection.Dock.Y = cornerY + villageHeight + 3;
                        connection.Height = node.Height;
                        connection.Direction.Y = 1;
                    }
                    else
                    {
                        var bottomPath = _randomNodes.Next(villageWidth - 1);
                        var x = bottomPath + cornerX;
                        foreach (var coord in PatternHelper.Rect(x, cornerY - 3, 2, 3))
                            Map.SetGround(coord, EGround.Sand);
                        foreach (var coord in PatternHelper.Rect(x - 2 + (x & 1), cornerY - 3, 6 - (x & 1) * 2, 2))
                            Map.SetTile(coord, ETile.None);
                        connection.Dock.X = x;
                        connection.Dock.Y = cornerY - 4;
                        connection.Height = node.Height;
                        connection.Direction.Y = -1;
                    }
                }
            }



            int RowWidth(List<AddVillageToMapEntry> entry) =>
                entry.Sum(EntryWidth) + entry.Count - 1;

            int EntryWidth(AddVillageToMapEntry entry)
            {
                switch (entry)
                {
                    case AddVillageToMapEntry.Road: return 2;
                    case AddVillageToMapEntry.House: return 4;
                    case AddVillageToMapEntry.PokeCenter: return 5;
                    case AddVillageToMapEntry.Shop: return 4;
                    case AddVillageToMapEntry.Tree: return 2;
                    default: return 0;
                }
            }
        }

    }
}
