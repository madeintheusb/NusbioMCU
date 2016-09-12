namespace STDDeviation
{
    class BikeMotion
    {
        public double _ax, _ay, _az, _gx, _gy, _gz;

        public int Index;
        
        public BikeMotion(string ax, string ay, string az, string gx, string gy, string gz)
        {
            try {
            this._ax = double.Parse(ax);
            this._ay = double.Parse(ay);
            this._az = double.Parse(az);

            this._gx = double.Parse(gx);
            this._gy = double.Parse(gy);
            this._gz = double.Parse(gz);
            }
            catch(System.Exception ex)
            {
                throw new System.ArgumentException();
            }
        }
    }
}