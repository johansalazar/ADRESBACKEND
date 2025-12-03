namespace Adq.Backend.Api.Application.Dto
{
    public class ResponseApi<T>
    {
        public bool Estado { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static ResponseApi<T> Ok(T data, string mensaje = "OK") =>
            new ResponseApi<T> { Estado = true, Mensaje = mensaje, Data = data };

        public static ResponseApi<T> Fail(string mensaje) =>
            new ResponseApi<T> { Estado = false, Mensaje = mensaje, Data = default };
    }
}
