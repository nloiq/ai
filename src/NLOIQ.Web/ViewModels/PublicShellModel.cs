using NLOIQ.Application.Content;
namespace NLOIQ.Web.ViewModels;
public sealed record PublicShellModel(string Culture, PublicBrand Brand, string AlternateUrl, PublicNode? Navigation = null, PublicNode? Footer = null, PublicNode? FooterNavigation = null);
