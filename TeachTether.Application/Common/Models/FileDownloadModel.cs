namespace TeachTether.Application.Common.Models
{
    public sealed record FileDownloadModel(
    Stream Content,
    string ContentType,
    string FileName);
}
