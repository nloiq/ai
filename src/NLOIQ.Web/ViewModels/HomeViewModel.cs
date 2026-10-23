using NLOIQ.Application.Interfaces;
namespace NLOIQ.Web.ViewModels;
public sealed record HomeViewModel(string Culture, IReadOnlyList<ServicePreviewDto> Services)
{
    public bool IsArabic => Culture == "ar";
}
