using NLOIQ.Application.Interfaces;
namespace NLOIQ.Web.ViewModels;
public sealed record HomeViewModel(string Culture, IReadOnlyList<ServicePreviewDto> Services, IReadOnlyDictionary<string, string?> ProjectVideoIds)
{
    public bool IsArabic => Culture == "ar";
}
