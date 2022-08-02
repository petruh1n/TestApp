namespace WebApplication1.Models
{
    public class Data
    {
        public int Id { get; set; }
        public int Code { get; set; }
        public string Value { get; set; }

        public Data(int id, int code, string value)
        {
            Id = id;
            Code = code;
            Value = value;
        }
    }
}
