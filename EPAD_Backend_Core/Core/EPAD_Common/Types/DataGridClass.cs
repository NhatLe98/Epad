namespace EPAD_Common.Types
{
    public class DataGridClass
    {
        public double total { get; set; }
        public object data { get; set; }
        public DataGridClass(double _total, object _data)
        {
            this.total = _total;
            this.data = _data;
        }
    }
}
