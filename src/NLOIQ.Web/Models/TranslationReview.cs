using NLOIQ.Domain.Content;
namespace NLOIQ.Web.Models;
public sealed record TranslationReview(long Id, ContentKind Kind, string Key, PublicationState State, bool IsDemo,
    string EnglishTitle, string ArabicTitle, IReadOnlyList<string> EnglishMissing, IReadOnlyList<string> ArabicMissing);
public sealed record TranslationReviewPage(IReadOnlyList<TranslationReview> Rows, int Page, bool HasNext);
