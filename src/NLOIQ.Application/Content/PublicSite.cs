using NLOIQ.Domain.Content;
namespace NLOIQ.Application.Content;
public sealed record PublicImage(string Url,string Alt,string? Caption,int Width,int Height,string? SrcSet=null);
public sealed record PublicLink(string Label,string Href,int Depth=0);
public sealed record PublicContact(string Label,string Value,string? Href);
public sealed record PublicHours(int Day,bool Closed,TimeOnly? Opens,TimeOnly? Closes);
public sealed class PublicNode
{
    public long Id { get; set; } public long RevisionId { get; set; }
    public string Key { get; set; }=""; public ContentKind Kind { get; set; }
    public string Title { get; set; }="";public string Summary { get; set; }="";public string Body { get; set; }="";public string? Slug { get; set; }
    public string? PrimaryLabel { get; set; }public string? SecondaryLabel { get; set; }public string? PrimaryTarget { get; set; }public string? SecondaryTarget { get; set; }
    public SectionKind? SectionKind { get; set; }public string Layout { get; set; }="default";public string Background { get; set; }="paper";public decimal Parallax { get; set; }
    public bool Visible { get; set; }=true;public bool Featured { get; set; }public int Order { get; set; }public string Icon { get; set; }="network";
    public PublicSeo? Seo { get; set; }
    public string? VideoId { get; set; }public bool ShowCopyright { get; set; }=true;
    public List<PublicImage> Images { get; set; }=[];public List<PublicLink> Links { get; set; }=[];public List<PublicContact> Contacts { get; set; }=[];public List<PublicLink> Social { get; set; }=[];public List<PublicHours> Hours { get; set; }=[];public List<string> Categories { get; set; }=[];
}
public sealed record PublicSite(string Culture,PublicNode Page,IReadOnlyList<PublicNode> Sections,IReadOnlyList<PublicNode> Projects,IReadOnlyList<PublicNode> Services,PublicNode? Navigation,PublicNode? FooterNavigation,PublicNode? Contact,PublicNode? Footer,string AlternateUrl,bool IsPreview=false);
public interface IPublicSiteReader
{
    Task<PublicSite?> ReadAsync(string culture,string? slug=null,bool project=false,CancellationToken ct=default);
    Task<PublicNode?> ReadContactAsync(string culture,CancellationToken ct=default);
}
