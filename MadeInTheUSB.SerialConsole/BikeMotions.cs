using System;
using System.Collections.Generic;
using System.Linq;

namespace STDDeviation
{

    public enum BikeMotionType
    {
        Idl,
        Gig,
        Mov,
        Unk,
    }

    class BikeMotions : List<BikeMotion>
    {

        public BikeMotionType BikeMotionType = BikeMotionType.Idl;


        public BikeMotionType _proviousBikeMotionType = BikeMotionType.Unk;
        double _previousTruncatedStdDevAsSpeed = -1;

        public double _previousTruncatedStdDevAsSpeedX = -1;
        public double _previousTruncatedStdDevAsSpeedY = -1;
        //public double _AverageTruncatedStdDevAsSpeedXY = -1;

        public BikeMotion Add(string ax, string ay, string az, string gx, string gy, string gz)
        {
            try {
                var b = new BikeMotion(ax, ay, az, gx, gy, gz);
                this.Add(b);
                b.Index = this.Count;
                return b;
            }
            catch(System.Exception ex)
            {
                return null;
            }
        }
        public const int MAX_SAMPLE_PER_SECOND = 10;

        public enum Angle
        {
            A_X, A_Y, G_X, G_Y
        }

        public void CalcStdDeviationEverySecondXY() {

            var a_x = CalcStdDeviationEverySecond(Angle.A_X);
            var a_y = CalcStdDeviationEverySecond(Angle.A_Y);

            var g_x = CalcStdDeviationEverySecond(Angle.G_X);
            var g_y = CalcStdDeviationEverySecond(Angle.G_Y);
            
            // Real stuff
            var A_AverageTruncatedStdDevAsSpeedXY = (a_x+a_y)/2;
            var G_AverageTruncatedStdDevAsSpeedXY = (g_x+g_y)/2;

            A_AverageTruncatedStdDevAsSpeedXY = a_x;
            G_AverageTruncatedStdDevAsSpeedXY = g_x;

            Console.WriteLine("Avg StdDev A:{0} G:{1}", A_AverageTruncatedStdDevAsSpeedXY, G_AverageTruncatedStdDevAsSpeedXY);
        }

        public double CalcStdDeviationEverySecond(Angle angle)
        {
            List<double> axlist  = new List<double>();
            var i = 0;
            var samplePerSecond = MAX_SAMPLE_PER_SECOND;

            while (i < this.Count)
            {
                //var tmpL = this.Skip(i).Take(samplePerSecond).ToList();
                var tmpL = this.Skip(i).ToList();
                axlist = GetAxAsList(tmpL, angle);
                if (axlist.Count >= samplePerSecond)
                {
                    var stdDevAsSpeed           = CalculateStdDev(axlist)*100;
                    var truncatedStdDevAsSpeed  = Math.Truncate(stdDevAsSpeed);
                    
                    if(truncatedStdDevAsSpeed < 2)
                        this.BikeMotionType = BikeMotionType.Idl;
                    else  
                        this.BikeMotionType = BikeMotionType.Mov;

                    var sumAll = axlist.Sum();

                    var avrAsStr = string.Format("{0:0.000}", axlist.Average());
                    if (!avrAsStr.StartsWith("-"))
                        avrAsStr += "+";
                    
                    Console.WriteLine("A:{0} Sum:{1:000} Avg:{4:0.000} SD:{5:00.00000} Sp:{6:0.0} C:{7}", 
                        angle,
                        sumAll,
                        i, 
                        i + samplePerSecond,
                        avrAsStr, 
                        stdDevAsSpeed, 
                        truncatedStdDevAsSpeed,
                        axlist.Count
                        //this.GetAxAsString(tmpL, angle)                        
                        );
                    this._proviousBikeMotionType            = this.BikeMotionType;
                    this._previousTruncatedStdDevAsSpeed    = truncatedStdDevAsSpeed;
                }
                i += samplePerSecond;
            }
            this._previousTruncatedStdDevAsSpeed = axlist.Average();
            return this._previousTruncatedStdDevAsSpeed;  
        }

        List<double> GetAxAsList(List<BikeMotion> bm, Angle angle)
        {
            var l = new List<double>();
            foreach (var e in bm)
            {
                if(angle == Angle.A_X)
                    l.Add(e._ax);
                if(angle == Angle.A_Y)
                    l.Add(e._ay);
                if(angle == Angle.G_X)
                    l.Add(e._gx);
                if(angle == Angle.G_Y)
                    l.Add(e._gy);
            }
            return l;
        }

        public string GetAxAsString(List<BikeMotion> bm, Angle angle)
        {
            var l = new System.Text.StringBuilder();
            l.Append("[");
            foreach (var e in bm) {
                
                if(angle == Angle.A_X)
                    l.AppendFormat("{0},", e._ax);
                if(angle == Angle.A_Y)
                    l.AppendFormat("{0},", e._ay);
                if(angle == Angle.G_X)
                    l.AppendFormat("{0},", e._gx);
                if(angle == Angle.G_Y)
                    l.AppendFormat("{0},", e._gy);
            }
            l.Append("]");
            return l.ToString();
        }

        private double CalculateStdDev(List<double> values)
        {   
            double ret = 0;
            if (values.Count() > 0) 
            {      
                //Compute the Average      
                double avg = values.Average();
                //Perform the Sum of (value-avg)_2_2      
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                //Put it all together      
                ret = Math.Sqrt((sum) / (values.Count()-1));   
            }   
            return ret;
        }

    }
}
