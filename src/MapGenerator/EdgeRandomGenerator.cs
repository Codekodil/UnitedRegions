
using MapData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Math = UnhedderEngine.Math;

namespace MapGenerator
{
    internal static class EdgeRandomGenerator
    {
        private static readonly List<(MethodInfo Method, CellGenerators.CellAttribute Attribute)> _cellGenerators;
        static EdgeRandomGenerator()
        {
            _cellGenerators = typeof(CellGenerators)
                .GetMethods()
                .Where(m =>
                {
                    var parameters = m.GetParameters();
                    return parameters.Length == 1 &&
                        parameters[0].ParameterType == typeof(CellGenerators.CellGeneratorData) &&
                        m.IsStatic;
                })
                .Select(m => (m, m.GetCustomAttributes().OfType<CellGenerators.CellAttribute>().FirstOrDefault()))
                .Where(m => m.Item2 != null)
                .OrderBy(m => m.Item1.Name)
                .ToList();
        }


        private enum EEdgeMode { Undefined, Blocked, Open, PositiveDirection, NegativeDirecion, Connection }
        private class MultiCell
        {
            public Coord Beginning, Direction;
            public int Size = 1, pathOrder = -1;
            public bool Treasure;
        }

        public static void AddEdgeToMap(MapEdge edge, Map map)
        {
            var rand = edge.Rand;

            var fromConnection = edge.From.GetConnection(edge);
            var toConnection = edge.To.GetConnection(edge);

            const int connectionExtensionLength = 7;
            const int AverageCellSize = 5;
            const int MinimumCellSize = 4;

            var fromAnchor = fromConnection.Dock + fromConnection.Direction * connectionExtensionLength;
            var toAnchor = toConnection.Dock + toConnection.Direction * connectionExtensionLength;

            var min = Coord.Min(fromAnchor, toAnchor, edge.Center - new Coord(AverageCellSize * 2 + 4, AverageCellSize * 2 + 4));
            var max = Coord.Max(fromAnchor, toAnchor, edge.Center + new Coord(AverageCellSize * 2 + 4, AverageCellSize * 2 + 4));


            var gridSize = (max - min + new Coord(MinimumCellSize, MinimumCellSize)) / (AverageCellSize + 2);
            var columns = gridSize.X;
            var rows = gridSize.Y;
            bool PositionInsideGrid(Coord pos) => pos >= new Coord() && pos < gridSize;

            var columnWidths = new int[columns];
            var rowHeights = new int[rows];
            for (var x = 0; x < columns; ++x)
                columnWidths[x] = MinimumCellSize;
            for (var y = 0; y < rows; ++y)
                rowHeights[y] = MinimumCellSize;

            for (var i = columns * (MinimumCellSize + 2) - 2; i < max.X - min.X + 1; ++i)
                columnWidths[rand.Next(columns)]++;
            for (var i = rows * (MinimumCellSize + 2) - 2; i < max.Y - min.Y + 1; ++i)
                rowHeights[rand.Next(rows)]++;


            var cellCorners = new Coord[columns, rows];
            cellCorners[0, 0] = min;
            for (var x = 1; x < columns; ++x)
                cellCorners[x, 0] = cellCorners[x - 1, 0] + new Coord(columnWidths[x - 1] + 2, 0);
            for (var y = 1; y < rows; ++y)
                cellCorners[0, y] = cellCorners[0, y - 1] + new Coord(0, rowHeights[y - 1] + 2);
            for (var x = 1; x < columns; ++x)
                for (var y = 1; y < rows; ++y)
                    cellCorners[x, y] = new Coord(cellCorners[x, 0].X, cellCorners[0, y].Y);

            Coord MapPointToCell(Coord pos)
            {
                var result = gridSize - new Coord(1, 1);
                for (var x = 1; x < columns; ++x)
                    if (pos.X < cellCorners[x, 0].X)
                    {
                        result.X = x - 1;
                        break;
                    }
                for (var y = 1; y < rows; ++y)
                    if (pos.Y < cellCorners[0, y].Y)
                    {
                        result.Y = y - 1;
                        break;
                    }
                return result;
            }
            var fromGrid = MapPointToCell(fromConnection.Dock);
            var centerGrid = MapPointToCell(edge.Center);
            var toGrid = MapPointToCell(toConnection.Dock);


            var pathFindGrid = new int[columns, rows];
            for (var x = 0; x < columns; ++x)
                for (var y = 0; y < rows; ++y)
                    pathFindGrid[x, y] = int.MinValue / 2;

            pathFindGrid[fromGrid.X, fromGrid.Y] = 0;
            var pathSteps = new List<Coord> { fromGrid };
            {
                void PathFromTo(Coord from, Coord to)
                {
                    while (from != to)
                    {
                        var nextIndex = pathFindGrid[from.X, from.Y] + 1;
                        var difference = to - from;
                        var differenceAbs = difference.Abs();
                        var moveVertical = (differenceAbs.X == differenceAbs.Y) ? rand.Next(2) == 0 : differenceAbs.X < differenceAbs.Y;
                        from += moveVertical ? new Coord(0, difference.Y > 0 ? 1 : -1) : new Coord(difference.X > 0 ? 1 : -1, 0);
#if DEBUG
                        if (pathFindGrid[from.X, from.Y] >= 0)
                            System.Diagnostics.Debugger.Break();
#endif
                        pathFindGrid[from.X, from.Y] = nextIndex;
                        pathSteps.Add(from);
                    }
                }
                PathFromTo(fromGrid, centerGrid);
                PathFromTo(centerGrid, toGrid);
                for (var y = 0; y < rows; ++y)
                    for (var x = 0; x < columns; ++x)
                        AdjustNgativePath(x, y);
                for (var y = rows - 1; y >= 0; --y)
                    for (var x = columns - 1; x >= 0; --x)
                        AdjustNgativePath(x, y);
                void AdjustNgativePath(int x, int y)
                {
                    if (pathFindGrid[x, y] >= 0) return;
                    var adjacentCells = new List<Coord>();
                    if (x >= 1) adjacentCells.Add(new Coord(x - 1, y));
                    if (y >= 1) adjacentCells.Add(new Coord(x, y - 1));
                    if (x < columns - 1) adjacentCells.Add(new Coord(x + 1, y));
                    if (y < rows - 1) adjacentCells.Add(new Coord(x, y + 1));
                    pathFindGrid[x, y] = Math.Max(adjacentCells.Select(c => Math.Min(pathFindGrid[c.X, c.Y], 0))) - 1;
                }
            }

            var lengthenings = rand.Next(pathSteps.Count / 4 + 2, pathSteps.Count);
            for (var i = 0; i < lengthenings; ++i)
            {
                var lengthenIndex = rand.Next(pathSteps.Count - 1);
                var lengthenPosition = pathSteps[lengthenIndex];
                var stitchIndex = lengthenIndex + 1;
                var stitchPosition = pathSteps[stitchIndex];

                var stitchDirection = stitchPosition - lengthenPosition;
                var randomPriorityWeight = rand.Next(2) == 0 ? 1 : -1;
                var additionalPath = new[] { new Coord(0, randomPriorityWeight), new Coord(0, -randomPriorityWeight) }
                    .Select(c => c * stitchDirection + lengthenPosition)
                    .Where(c => PositionInsideGrid(c) && pathFindGrid[c.X, c.Y] < 0)
                    .Select(c => Tuple.Create(c, c + stitchDirection))
                    .FirstOrDefault(c => PositionInsideGrid(c.Item2) && pathFindGrid[c.Item2.X, c.Item2.Y] < 0);

                if (additionalPath != null)
                {
                    pathSteps.Insert(stitchIndex, additionalPath.Item2);
                    pathSteps.Insert(stitchIndex, additionalPath.Item1);

                    for (var j = stitchIndex; j < pathSteps.Count; ++j)
                    {
                        var stepPosition = pathSteps[j];
                        pathFindGrid[stepPosition.X, stepPosition.Y] = j;
                    }
                }
            }

            var pathDeltaSteps = new Coord[pathSteps.Count - 1];
            for (var i = 0; i < pathDeltaSteps.Length; ++i)
                pathDeltaSteps[i] = pathSteps[i + 1] - pathSteps[i];


            var verticalTransitions = new EEdgeMode[columns, rows - 1];
            var horizontalTransitions = new EEdgeMode[columns - 1, rows];
            void addTransition(Coord pos, Coord direction, EEdgeMode transition)
            {
                if (transition == EEdgeMode.PositiveDirection && direction.X + direction.Y < 0)
                    transition = EEdgeMode.NegativeDirecion;
                else if (transition == EEdgeMode.NegativeDirecion && direction.X + direction.Y < 0)
                    transition = EEdgeMode.PositiveDirection;

                if (direction.X == 0)
                {
                    if (direction.Y > 0)
                        verticalTransitions[pos.X, pos.Y] = transition;
                    else
                        verticalTransitions[pos.X, pos.Y - 1] = transition;
                }
                else if (direction.X > 0)
                    horizontalTransitions[pos.X, pos.Y] = transition;
                else
                    horizontalTransitions[pos.X - 1, pos.Y] = transition;
            }

            for (var i = 0; i < pathSteps.Count - 1; ++i)
            {
                var pos = pathSteps[i];
                var dif = pathDeltaSteps[i];
                addTransition(pos, dif, EEdgeMode.Open);
            }


            var multicellGrid = new int[columns, rows];
            for (var x = 0; x < columns; ++x)
                for (var y = 0; y < rows; ++y)
                    multicellGrid[x, y] = -1;
            var multicells = new List<MultiCell>();

            for (var i = 0; i < pathDeltaSteps.Length - 1; ++i)
                if (pathDeltaSteps[i] == pathDeltaSteps[i + 1])
                {
                    var beginning = pathSteps[i];
                    var delta = pathDeltaSteps[i];
                    for (var j = 0; j < 3; ++j)
                    {
                        var setGridPos = beginning + j * delta;
                        multicellGrid[setGridPos.X, setGridPos.Y] = multicells.Count;
                    }
                    multicells.Add(new MultiCell
                    {
                        Size = 3,
                        Beginning = beginning,
                        Direction = delta,
                        pathOrder = pathFindGrid[beginning.X, beginning.Y]
                    });
                    i += 2;
                }

            {
                var cellsWithoutMulticell = pathSteps.Where(c => multicellGrid[c.X, c.Y] < 0).ToList();
                var doubleCells = rand.Next((cellsWithoutMulticell.Count + 1) / 2, (cellsWithoutMulticell.Count * 3) / 2);
                for (var i = 0; i < doubleCells; ++i)
                {
                    if (cellsWithoutMulticell.Count == 0) break;
                    var index = rand.Next(cellsWithoutMulticell.Count);
                    var position = cellsWithoutMulticell[index];
                    var randomDirection = RandomDirection();

                    var secondPosition = position + randomDirection;
                    if (!PositionInsideGrid(secondPosition) || multicellGrid[secondPosition.X, secondPosition.Y] >= 0) continue;

                    var secondPathIndex = pathFindGrid[secondPosition.X, secondPosition.Y];
                    if (secondPathIndex >= 0)
                    {
                        var pathDifference = secondPathIndex - pathFindGrid[position.X, position.Y];
                        if (Math.Abs(pathDifference) != 1)
                            continue;
                        if (pathDifference < 0)
                        {
                            position = secondPosition;
                            randomDirection *= -1;
                            secondPosition = position + randomDirection;
                        }
                        cellsWithoutMulticell.Remove(position);
                        cellsWithoutMulticell.Remove(secondPosition);
                    }
                    else
                        cellsWithoutMulticell.RemoveAt(index);
                    multicellGrid[secondPosition.X, secondPosition.Y] = multicellGrid[position.X, position.Y] = multicells.Count;
                    multicells.Add(new MultiCell
                    {
                        Size = 2,
                        Beginning = position,
                        Direction = randomDirection,
                        pathOrder = pathFindGrid[position.X, position.Y]
                    });
                }

                foreach (var pos in cellsWithoutMulticell)
                {
                    multicellGrid[pos.X, pos.Y] = multicells.Count;
                    multicells.Add(new MultiCell
                    {
                        Beginning = pos,
                        pathOrder = pathFindGrid[pos.X, pos.Y]
                    });
                }
            }

            foreach (var cell in multicells)
                if (cell.Size > 1)
                    for (var i = 1; i < cell.Size; ++i)
                        addTransition(cell.Beginning + cell.Direction * (i - 1), cell.Direction, EEdgeMode.Connection);

            //treasureCorner
            if (rand.NextDouble() < .6)
            {
                var possibleLocations = PatternHelper.Rect(new Coord(), gridSize)
                       .Where(pos => multicellGrid[pos.X, pos.Y] < 0)
                       .Select(pos =>
                       {
                           var sides = new List<Coord>();
                           if (pos.X > 0 && multicellGrid[pos.X - 1, pos.Y] >= 0) sides.Add(new Coord(pos.X - 1, pos.Y));
                           if (pos.Y > 0 && multicellGrid[pos.X, pos.Y - 1] >= 0) sides.Add(new Coord(pos.X, pos.Y - 1));
                           if (pos.X < columns - 1 && multicellGrid[pos.X + 1, pos.Y] >= 0) sides.Add(new Coord(pos.X + 1, pos.Y));
                           if (pos.Y < rows - 1 && multicellGrid[pos.X, pos.Y + 1] >= 0) sides.Add(new Coord(pos.X, pos.Y + 1));
                           return Tuple.Create(pos, sides);
                       })
                       .Where(location => location.Item2.Count == 2)
                       .ToList();
                if (possibleLocations.Count > 0)
                {
                    var location = possibleLocations[rand.Next(possibleLocations.Count)];
                    var pos = location.Item1;
                    var high = location.Item2[0];
                    var low = location.Item2[1];
                    if (multicells[multicellGrid[high.X, high.Y]].pathOrder < multicells[multicellGrid[low.X, low.Y]].pathOrder)
                    {
                        var buffer = high;
                        high = low;
                        low = buffer;
                    }
                    multicellGrid[pos.X, pos.Y] = multicells.Count;
                    multicells.Add(new MultiCell
                    {
                        Beginning = pos,
                        Treasure = true
                    });
                    addTransition(pos, high - pos, EEdgeMode.NegativeDirecion);
                    addTransition(pos, low - pos, EEdgeMode.PositiveDirection);
                }
            }

            //branche Cells
            {
                var possiblePositions = PatternHelper.Rect(new Coord(), gridSize)
                       .Where(pos => multicellGrid[pos.X, pos.Y] < 0 && pathFindGrid[pos.X, pos.Y] == -1)
                       .ToList();
                if (possiblePositions.Count > 0)
                {
                    var branches = rand.Next(1, possiblePositions.Count);
                    var includeTreasures = rand.Next(branches / 3 + 1);
                    for (var i = 0; i < branches; ++i)
                    {
                        var index = rand.Next(possiblePositions.Count);
                        var pos = possiblePositions[index];
                        possiblePositions.RemoveAt(index);

                        var randomDirection = RandomDirection();
                        for (var j = 0; j < 4; ++j)
                        {
                            var sidePos = pos + randomDirection;
                            if (PositionInsideGrid(sidePos) && multicellGrid[sidePos.X, sidePos.Y] >= 0)
                            {
                                multicellGrid[pos.X, pos.Y] = multicells.Count;
                                multicells.Add(new MultiCell
                                {
                                    Size = 1,
                                    Beginning = pos,
                                    Treasure = (includeTreasures--) > 0,
                                    pathOrder = multicells[multicellGrid[sidePos.X, sidePos.Y]].pathOrder
                                });

                                addTransition(pos, randomDirection, EEdgeMode.Open);
                                break;
                            }
                            randomDirection *= new Coord(0, 1);
                        }
                    }
                }
            }

            //backtrack cliffs
            for (var x = 0; x < columns; ++x)
                for (var y = 0; y < rows; ++y)
                {
                    double chanceForCliff(int orderFrom, int orderTo) => .8 - Math.Exp(-Math.Abs(orderFrom - orderTo) * .3) * 1.2;

                    if (x < columns - 1 && horizontalTransitions[x, y] == EEdgeMode.Undefined)
                    {
                        var from = multicellGrid[x, y];
                        var to = multicellGrid[x + 1, y];
                        if (from >= 0 && to >= 0)
                        {
                            var fromOrder = multicells[from].pathOrder;
                            var toOrder = multicells[to].pathOrder;
                            if (fromOrder >= 0 && toOrder >= 0)
                                horizontalTransitions[x, y] = rand.NextDouble() < chanceForCliff(fromOrder, toOrder)
                                    ? (fromOrder > toOrder ? EEdgeMode.PositiveDirection : EEdgeMode.NegativeDirecion)
                                    : EEdgeMode.Blocked;
                            else
                                horizontalTransitions[x, y] = EEdgeMode.Blocked;
                        }
                    }
                    if (y < rows - 1 && verticalTransitions[x, y] == EEdgeMode.Undefined)
                    {
                        var from = multicellGrid[x, y];
                        var to = multicellGrid[x, y + 1];
                        if (from >= 0 && to >= 0)
                        {
                            var fromOrder = multicells[from].pathOrder;
                            var toOrder = multicells[to].pathOrder;
                            if (fromOrder >= 0 && toOrder >= 0)
                                verticalTransitions[x, y] = rand.NextDouble() < chanceForCliff(fromOrder, toOrder)
                                    ? (fromOrder > toOrder ? EEdgeMode.PositiveDirection : EEdgeMode.NegativeDirecion)
                                    : EEdgeMode.Blocked;
                            else
                                verticalTransitions[x, y] = EEdgeMode.Blocked;
                        }
                    }
                }

            //Tile None for open edges
            {
                for (var x = 0; x < columns; ++x)
                {
                    var width = columnWidths[x];
                    for (var y = 0; y < rows; ++y)
                    {
                        var height = rowHeights[y];

                        if (x < columns - 1)
                        {
                            var transition = horizontalTransitions[x, y];
                            switch (transition)
                            {
                                case EEdgeMode.Open:
                                case EEdgeMode.PositiveDirection:
                                case EEdgeMode.NegativeDirecion:
                                    foreach (var coord in PatternHelper.Rect(cellCorners[x + 1, y] - new Coord(2, 0), new Coord(2, height)))
                                        map.SetTile(coord, ETile.None);
                                    break;
                            }
                        }
                        if (y < rows - 1)
                        {
                            var transition = verticalTransitions[x, y];
                            switch (transition)
                            {
                                case EEdgeMode.Open:
                                case EEdgeMode.PositiveDirection:
                                case EEdgeMode.NegativeDirecion:
                                    foreach (var coord in PatternHelper.Rect(cellCorners[x, y + 1] - new Coord(0, 2), new Coord(width, 2)))
                                        map.SetTile(coord, ETile.None);
                                    break;
                            }
                        }
                    }
                }
            }

            //Cell Contents
            foreach (var cell in multicells)
            {
                var end = cell.Beginning + cell.Direction * (cell.Size - 1);
                var minPos = Coord.Min(cell.Beginning, end);
                var maxPos = Coord.Max(cell.Beginning, end);
                var corner = cellCorners[minPos.X, minPos.Y];
                var size = cellCorners[maxPos.X, maxPos.Y] - corner + new Coord(columnWidths[maxPos.X], rowHeights[maxPos.Y]);

                foreach (var coord in PatternHelper.Rect(corner, size))
                    map.SetTile(coord, ETile.None);
                GenerateCell(cell, corner, size);
            }


            //connection to node
            {
                void DockToNode(MapNode.Connection connection, Coord gridPos)
                {
                    var gridAttachLow = cellCorners[gridPos.X, gridPos.Y];
                    var cellWidth = columnWidths[gridPos.X];
                    var cellHeight = rowHeights[gridPos.Y];

                    if (connection.Direction.X > 0)
                        gridAttachLow.X -= 1;
                    else if (connection.Direction.X < 0)
                        gridAttachLow.X += cellWidth;

                    if (connection.Direction.Y > 0)
                        gridAttachLow.Y -= 1;
                    else if (connection.Direction.Y < 0)
                        gridAttachLow.Y += cellHeight;

                    var inverseDirection = new Coord(1) - connection.Direction.Abs();

                    var gridAttachHigh = gridAttachLow + new Coord(cellWidth - 1, cellHeight - 1).Scale(inverseDirection);

                    foreach (var coord in PatternHelper.BoundingRect(
                        connection.Dock - inverseDirection * 2,
                        connection.Dock + inverseDirection * 2,
                        gridAttachLow - connection.Direction,
                        gridAttachHigh - connection.Direction))
                        map.SetTile(coord, ETile.None);
                    foreach (var coord in PatternHelper.Rect(gridAttachLow, gridAttachHigh - gridAttachLow + new Coord(1)))
                        map.SetTile(coord, ETile.None);

                    var gridAttachRoadCenter = (gridAttachLow + gridAttachHigh + new Coord(rand.Next(2))) / 2;

                    foreach (var coord in PatternHelper.BoundingRect(
                        gridAttachRoadCenter + connection.Direction * rand.Next(4) + inverseDirection,
                        gridAttachRoadCenter - connection.Direction * 4 - inverseDirection))
                    {
                        map.SetGround(coord, EGround.Sand);
                        map.SetTile(coord, ETile.None);
                    }
                    foreach (var coord in PatternHelper.BoundingRect(
                        connection.Dock - inverseDirection,
                        connection.Dock + inverseDirection + connection.Direction * 3))
                        map.SetGround(coord, EGround.Sand);
                    foreach (var coord in PatternHelper.BoundingRect(
                        connection.Dock - inverseDirection + connection.Direction * 3,
                        connection.Dock + inverseDirection + connection.Direction * 3,
                        gridAttachRoadCenter - connection.Direction * 4 - inverseDirection,
                        gridAttachRoadCenter - connection.Direction * 4 + inverseDirection))
                        map.SetGround(coord, EGround.Sand);

                }
                DockToNode(fromConnection, fromGrid);
                DockToNode(toConnection, toGrid);
            }



            //placeTreeInBorderEdges
            {
                for (var x = 0; x < columns; ++x)
                    for (var y = 0; y < rows; ++y)
                    {
                        bool IsUndefinedGrid(int dx, int dy) => !PositionInsideGrid(new Coord(x + dx, y + dy)) || multicellGrid[x + dx, y + dy] < 0;

                        if (x < columns - 1)
                            if (horizontalTransitions[x, y] == EEdgeMode.Blocked)
                                if (IsUndefinedGrid(0, 1) ||
                                    IsUndefinedGrid(0, -1) ||
                                    IsUndefinedGrid(1, 1) ||
                                    IsUndefinedGrid(1, -1))
                                    MapImprover.ReplaceUndefinedByTree(cellCorners[x + 1, y] + new Coord(-2, rand.Next(rowHeights[y] - 1)), map);

                        if (y < rows - 1)
                            if (verticalTransitions[x, y] == EEdgeMode.Blocked)
                                if (IsUndefinedGrid(1, 0) ||
                                    IsUndefinedGrid(-1, 0) ||
                                    IsUndefinedGrid(1, 1) ||
                                    IsUndefinedGrid(-1, 1))
                                    MapImprover.ReplaceUndefinedByTree(cellCorners[x, y + 1] + new Coord(rand.Next(columnWidths[x] - 1), -2), map);
                    }
            }



            //replace undefined
            {
                var undefinedTiles = new HashSet<Coord>();
                for (var x = min.X; x <= max.X; ++x)
                    for (var y = min.Y; y <= max.Y; ++y)
                        if (map.GetTile(x, y) == ETile.Undefined)
                            undefinedTiles.Add(new Coord(x, y));
                var blobs = new List<HashSet<Coord>>();
                while (undefinedTiles.Count > 0)
                {
                    var tilesToCheck = new List<Coord>();
                    tilesToCheck.Add(undefinedTiles.First());
                    var tilesInBlob = new HashSet<Coord>();

                    while (tilesToCheck.Count > 0)
                    {
                        var current = tilesToCheck[0];
                        tilesToCheck.RemoveAt(0);
                        if (tilesInBlob.Contains(current) || map.GetTile(current) != ETile.Undefined) continue;
                        tilesInBlob.Add(current);
                        if (current >= min && current <= max)
                        {
                            tilesToCheck.Add(current + new Coord(1, 0));
                            tilesToCheck.Add(current + new Coord(0, 1));
                            tilesToCheck.Add(current + new Coord(-1, 0));
                            tilesToCheck.Add(current + new Coord(0, -1));
                        }
                        else
                        {
                            foreach (var v in tilesInBlob)
                                undefinedTiles.Remove(v);
                            tilesInBlob = null;
                            break;
                        }
                    }

                    if (tilesInBlob?.Count > 0)
                    {
                        foreach (var v in tilesInBlob)
                            undefinedTiles.Remove(v);
                        blobs.Add(tilesInBlob);
                    }
                }

                while (blobs.Count > 0)
                {
                    var blobIndex = rand.Next(blobs.Count);
                    var blob = blobs[blobIndex];
                    blobs.RemoveAt(blobIndex);

                    var verticalAlignment = 0;
                    var horizontalAlignment = 0;
                    foreach (var pos in blob)
                    {
                        if (!blob.Contains(pos + new Coord(1, 0))) horizontalAlignment += (pos.X & 1) == 1 ? 1 : -1;
                        if (!blob.Contains(pos + new Coord(-1, 0))) horizontalAlignment += (pos.X & 1) == 0 ? 1 : -1;
                        if (!blob.Contains(pos + new Coord(0, 1))) verticalAlignment += (pos.Y & 1) == 1 ? 1 : -1;
                        if (!blob.Contains(pos + new Coord(0, -1))) verticalAlignment += (pos.Y & 1) == 0 ? 1 : -1;
                    }
                    var offset = new Coord();
                    if (horizontalAlignment == 0)
                        offset.X = rand.Next(2);
                    else if (horizontalAlignment < 0)
                        offset.X = 1;
                    if (verticalAlignment == 0)
                        offset.Y = rand.Next(2);
                    else if (verticalAlignment < 0)
                        offset.Y = 1;

                    foreach (var pos in blob.ToList())
                    {
                        if (((pos.X + offset.X) & 1) == 0 && ((pos.Y + offset.Y) & 1) == 0)
                        {
                            var pos2 = pos + new Coord(1, 0);
                            var pos3 = pos + new Coord(0, 1);
                            var pos4 = pos + new Coord(1, 1);

                            if (blob.Contains(pos2) && blob.Contains(pos3) && blob.Contains(pos4))
                            {
                                blob.Remove(pos);
                                blob.Remove(pos2);
                                blob.Remove(pos3);
                                blob.Remove(pos4);
                                map.SetTile(pos, ETile.Tree);
                                map.SetTile(pos2, ETile.Blocked);
                                map.SetTile(pos3, ETile.Blocked);
                                map.SetTile(pos4, ETile.Blocked);
                            }
                        }
                    }

                    if (blob.Count > 0)
                    {
                        var needFix = blob.ToList();
                        while (needFix.Count > 0)
                        {
                            var currentIndex = rand.Next(needFix.Count);
                            var current = needFix[currentIndex];
                            needFix.RemoveAt(currentIndex);

                            if (map.GetTile(current) != ETile.Undefined) continue;

                            current -= new Coord((current.X + offset.X) & 1, (current.Y + offset.Y) & 1);
                            MapImprover.ReplaceUndefinedByTree(current, map);
                        }
                    }
                }
            }


            for (var x = 0; x < columns; ++x)
            {
                var width = columnWidths[x];
                for (var y = 0; y < rows; ++y)
                {
                    var height = rowHeights[y];

                    if (x < columns - 1)
                    {
                        var transition = horizontalTransitions[x, y];
                        switch (transition)
                        {
                            case EEdgeMode.PositiveDirection:
                                foreach (var coord in PatternHelper.Rect(cellCorners[x + 1, y] - new Coord(1, 0), new Coord(1, height)))
                                {
                                    if (map.GetTile(coord) == ETile.None)
                                    {
                                        map.SetTile(coord, ETile.SmallCliffs);
                                        map.SetData(coord, EEdge.Right);
                                    }
                                }
                                break;
                            case EEdgeMode.NegativeDirecion:
                                foreach (var coord in PatternHelper.Rect(cellCorners[x + 1, y] - new Coord(2, 0), new Coord(1, height)))
                                {
                                    if (map.GetTile(coord) == ETile.None)
                                    {
                                        map.SetTile(coord, ETile.SmallCliffs);
                                        map.SetData(coord, EEdge.Left);
                                    }
                                }
                                break;
                        }
                    }
                    if (y < rows - 1)
                    {
                        var transition = verticalTransitions[x, y];
                        switch (transition)
                        {
                            case EEdgeMode.PositiveDirection:
                                foreach (var coord in PatternHelper.Rect(cellCorners[x, y + 1] - new Coord(0, 1), new Coord(width, 1)))
                                {
                                    if (map.GetTile(coord) == ETile.None)
                                    {
                                        map.SetTile(coord, ETile.SmallCliffs);
                                        map.SetData(coord, EEdge.Top);
                                    }
                                }
                                break;
                            case EEdgeMode.NegativeDirecion:
                                foreach (var coord in PatternHelper.Rect(cellCorners[x, y + 1] - new Coord(0, 2), new Coord(width, 1)))
                                {
                                    if (map.GetTile(coord) == ETile.None)
                                    {
                                        map.SetTile(coord, ETile.SmallCliffs);
                                        map.SetData(coord, EEdge.Bottom);
                                    }
                                }
                                break;
                        }
                    }
                }
            }




            void GenerateCell(MultiCell cell, Coord corner, Coord size)
            {
                var generators = _cellGenerators
                    .Where(g => g.Attribute.Size == cell.Size)
                    .ToList();
                if (generators.Count > 0)
                {
                    var totalChance = generators.Sum(g => g.Item2.Chance);
                    var randomChance = rand.NextDouble() * totalChance;
                    var chanceAggregate = 0d;
                    foreach (var generator in generators)
                    {
                        chanceAggregate += generator.Attribute.Chance;
                        if (randomChance < chanceAggregate)
                        {
                            var data = new CellGenerators.CellGeneratorData
                            {
                                Cell = cell,
                                Corner = corner,
                                Size = size,
                                Map = map,
                                Rand = rand
                            };
                            generator.Method.Invoke(null, new object[] { data });
                            return;
                        }
                    }
                }
            }

            Coord RandomDirection()
            {
                var randomDirection = new Coord(rand.Next(2), 0);
                randomDirection.Y = 1 - randomDirection.X;
                if (rand.Next(2) == 0)
                    randomDirection *= -1;
                return randomDirection;
            }
        }

        private static class CellGenerators
        {
            public class CellAttribute : Attribute
            {
                public CellAttribute(int size) => Size = size;
                public int Size;
                public double Chance = 1;
            }
            public struct CellGeneratorData
            {
                public MultiCell Cell;
                public Coord Corner, Size;
                public Map Map;
                public Random Rand;
            }


            [Cell(1)]
            public static void EmptyOneCell(CellGeneratorData data) { }

            [Cell(1)]
            public static void FuzzyTallGrass(CellGeneratorData data)
            {
                var corner = data.Corner + new Coord(1);
                var size = data.Size - new Coord(2);
                var rand = data.Rand;

                foreach (var coord in PatternHelper.Rect(corner, size))
                    data.Map.SetTile(coord, ETile.TallGrass);
                foreach (var coord in PatternHelper.Border(corner, size))
                    if (rand.NextDouble() < .8)
                        data.Map.SetTile(coord, ETile.TallGrass);
            }


            [Cell(2, Chance = 2)]
            public static void SmallRoad(CellGeneratorData data)
            {
                var flip = data.Cell.Direction.Y != 0;

                var corner = Flip(data.Corner);
                var size = Flip(data.Size);
                var rand = data.Rand;

                var middle = (size.Y - rand.Next(2)) / 2 + corner.Y;

                foreach (var coord in PatternHelper.Rect(corner.X + 1, middle - 1, size.X - 2, 3))
                    data.Map.SetGround(Flip(coord), EGround.Sand);

                Coord Flip(Coord coord) => flip ? coord.T() : coord;
            }

            [Cell(2)]
            public static void GrassWall(CellGeneratorData data)
            {
                var flip = data.Cell.Direction.Y != 0;

                var corner = Flip(data.Corner);
                var size = Flip(data.Size);
                var rand = data.Rand;

                var grassLength = rand.Next(size.X / 3, size.X / 2);
                var grassStart = rand.Next(size.X - grassLength + 1);

                foreach (var coord in PatternHelper.Rect(grassStart + corner.X, corner.Y, grassLength, size.Y))
                    data.Map.SetTile(Flip(coord), ETile.TallGrass);

                Coord Flip(Coord coord) => flip ? coord.T() : coord;
            }


            [Cell(3)]
            public static void RoadWithTallGrass(CellGeneratorData data)
            {
                var flip = data.Cell.Direction.Y != 0;

                var corner = Flip(data.Corner);
                var size = Flip(data.Size);
                var rand = data.Rand;

                var grassStart = rand.Next(size.X / 2);
                var grassEnd = rand.Next(size.X / 2, size.X);

                var middle = (size.Y - rand.Next(2)) / 2 + corner.Y;

                foreach (var coord in PatternHelper.Rect(grassStart + corner.X, corner.Y, grassEnd - grassStart + 1, size.Y))
                    data.Map.SetTile(Flip(coord), ETile.TallGrass);

                foreach (var coord in PatternHelper.Rect(corner.X + 1, middle - 1, grassStart - 1, 3))
                    data.Map.SetGround(Flip(coord), EGround.Sand);
                foreach (var coord in PatternHelper.Rect(grassEnd + corner.X + 1, middle - 1, size.X - grassEnd - 2, 3))
                    data.Map.SetGround(Flip(coord), EGround.Sand);

                Coord Flip(Coord coord) => flip ? coord.T() : coord;
            }
        }
    }
}
