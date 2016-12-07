using MadeInTheUSB.MCU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using MadeInTheUSB.Communication;

namespace NusbioMatrixConsole
{
    public class Square4x4
    {
        NusbioPixel _nusbioMatrix;

        public int MaxRow = 4;
        public int MaxCol = 4;
        
        public Square4x4(NusbioPixel nusbioMatrix)
        {
            this._nusbioMatrix = nusbioMatrix;
        }

        public List<Point> GetPoints()
        {
            return _XYMappingToLedIndex.Keys.ToList();
        }

        Dictionary<Point, int> _XYMappingToLedIndex = new Dictionary<Point, int>() { 

            { new Point(0, 0), 0 },
            { new Point(0, 1), 7 },
            { new Point(0, 2), 8 },
            { new Point(0, 3), 15 },

            { new Point(1, 0), 1 },
            { new Point(1, 1), 6 },
            { new Point(1, 2), 9 },
            { new Point(1, 3), 14 },

            { new Point(2, 0), 2 },
            { new Point(2, 1), 5 },
            { new Point(2, 2), 10 },
            { new Point(2, 3), 13 },

            { new Point(3, 0), 3 },
            { new Point(3, 1), 4 },
            { new Point(3, 2), 11 },
            { new Point(3, 3), 12 },
        };

        public bool SetPixel(Point p, Color color, bool refresh = false, int wait = -1)
        {
            return SetPixel(p.X, p.Y, color, refresh, wait);
        }

        public void Drawpoints(Dictionary<Point, Color> points, bool refresh = false, int wait = 0)
        {
            points.All(p => this.SetPixel(p.Key.X, p.Key.Y, points[p.Key]));
            if(refresh)
                this.Show(100);
            if (wait > 0)
                System.Threading.Thread.Sleep(wait);
        }

        public McuComResponse Show(int wait = 0)
        {
            var r = this._nusbioMatrix.Show();
            if (wait > 0)
                System.Threading.Thread.Sleep(wait);
            return r;
        }

        public bool SetPixel(int x, int y, Color color, bool refresh = false, int wait = -1, bool interruptOnKeyboard = false)
        {
            var r = true;
            foreach (var k in _XYMappingToLedIndex)
            {
                if (interruptOnKeyboard)
                    if (Console.KeyAvailable)
                        break;

                if (k.Key.X ==x && k.Key.Y == y)
                {
                    this._nusbioMatrix.SetPixel(k.Value, color);
                    if (r && refresh) 
                        this._nusbioMatrix.Show();
                    r = true;
                    if (wait > 0)
                        System.Threading.Thread.Sleep(wait);
                    break;
                }
            }
            return r;
        }
    }
}
