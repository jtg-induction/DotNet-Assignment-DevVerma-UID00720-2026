using System.Collections.Generic;

namespace Assignment3.DTOs.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; }
    }
}
