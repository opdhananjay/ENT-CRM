namespace ENT.Helpers
{
    public class Res<T>
    {
        public int StatusCode {  get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public Res(int StatusCode,string Message,T Data = default) 
        {
            this.StatusCode = StatusCode;
            this.Message = Message;
            this.Data = Data;
        }
    }
}
