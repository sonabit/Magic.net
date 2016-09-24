using System;
using System.Collections.Generic;
using System.Windows;

//http://treemaps.codeplex.com/

namespace Magic.Controls
{
    public class SquarifiedTreeMapsPanel : TreeMapsPanel
    {
        #region protected methods

        protected override Rect GetRectangle(RowOrientation orientation, WeightUIElement item, double x, double y,
            double width, double height)
        {
            if (orientation == RowOrientation.Horizontal)
                return new Rect(x, y, width, item.RealArea/width);
            return new Rect(x, y, item.RealArea/height, height);
        }

        protected override void ComputeNextPosition(RowOrientation orientation, ref double xPos, ref double yPos,
            double width, double height)
        {
            if (orientation == RowOrientation.Horizontal)
                yPos += height;
            else
                xPos += width;
        }

        protected override void ComputeBounds()
        {
            Squarify(ManagedItems, new List<WeightUIElement>(), GetShortestSide());
        }

        #endregion

        #region private methods

        private void Squarify(List<WeightUIElement> items, List<WeightUIElement> row, double sideLength)
        {
            if (items.Count == 0)
            {
                AddRowToLayout(row);
                return;
            }

            var item = items[0];
            var row2 = new List<WeightUIElement>(row);
            row2.Add(item);
            var items2 = new List<WeightUIElement>(items);
            items2.RemoveAt(0);

            var worst1 = Worst(row, sideLength);
            var worst2 = Worst(row2, sideLength);

            if (row.Count == 0 || worst1 > worst2)
                Squarify(items2, row2, sideLength);
            else
            {
                AddRowToLayout(row);
                Squarify(items, new List<WeightUIElement>(), GetShortestSide());
            }
        }

        private void AddRowToLayout(List<WeightUIElement> row)
        {
            ComputeTreeMaps(row);
        }

        private double Worst(List<WeightUIElement> row, double sideLength)
        {
            if (row.Count == 0) return 0;

            double maxArea = 0;
            var minArea = double.MaxValue;
            double totalArea = 0;
            foreach (var item in row)
            {
                maxArea = Math.Max(maxArea, item.RealArea);
                minArea = Math.Min(minArea, item.RealArea);
                totalArea += item.RealArea;
            }
            if (Math.Abs(minArea - double.MaxValue) < double.Epsilon) minArea = 0;

            var val1 = sideLength*sideLength*maxArea/(totalArea*totalArea);
            var val2 = totalArea*totalArea/(sideLength*sideLength*minArea);
            return Math.Max(val1, val2);
        }

        #endregion
    }
}