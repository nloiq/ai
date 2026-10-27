using NLOIQ.Application.Content;
namespace NLOIQ.Web.ViewModels;
public sealed record DirectoryModel(bool Projects,string? Selected,IReadOnlyList<PublicNode> Items,IReadOnlyList<PublicCategory> Categories,PublicNode? Intro)
{
    public bool Matches(PublicNode item,string key)=>Projects?(key=="_uncategorized"?item.CategoryLinks.Count==0:item.CategoryLinks.Any(x=>x.Key==key)):item.ServiceCategoryKey==key;
}
