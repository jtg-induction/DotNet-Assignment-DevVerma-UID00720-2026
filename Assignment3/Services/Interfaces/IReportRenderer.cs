namespace Assignment3.Services.Interfaces
{
    public interface IReportRenderer
    {
        byte[] RenderReport(string path, object data);
    }
}
