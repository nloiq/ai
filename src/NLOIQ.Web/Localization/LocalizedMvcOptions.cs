using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
namespace NLOIQ.Web.Localization;
public sealed class LocalizedMvcOptions(IStringLocalizer<SharedResource> text) : IConfigureOptions<MvcOptions>
{
    public void Configure(MvcOptions options)
    {
        options.ModelMetadataDetailsProviders.Add(new DisplayNames(text));
        var messages=options.ModelBindingMessageProvider;
        messages.SetAttemptedValueIsInvalidAccessor((_,field)=>text["ValidationValue",field]);
        messages.SetNonPropertyAttemptedValueIsInvalidAccessor(_=>text["ValidationInvalid"]);
        messages.SetUnknownValueIsInvalidAccessor(field=>text["ValidationValue",field]);
        messages.SetNonPropertyUnknownValueIsInvalidAccessor(()=>text["ValidationInvalid"]);
        messages.SetValueIsInvalidAccessor(_=>text["ValidationInvalid"]);
        messages.SetValueMustBeANumberAccessor(field=>text["ValidationNumber",field]);
        messages.SetNonPropertyValueMustBeANumberAccessor(()=>text["ValidationInvalid"]);
        messages.SetValueMustNotBeNullAccessor(_=>text["ValidationComplete"]);
        messages.SetMissingBindRequiredValueAccessor(field=>text["ValidationRequired",field]);
        messages.SetMissingKeyOrValueAccessor(()=>text["ValidationComplete"]);
        messages.SetMissingRequestBodyRequiredValueAccessor(()=>text["ValidationComplete"]);
    }
    private sealed class DisplayNames(IStringLocalizer<SharedResource> text) : IDisplayMetadataProvider
    {
        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            var key=context.Key.Name switch {
                "Title"=>"CmsTitleField", "Summary"=>"CmsSummary", "Body"=>"CmsBody", "Slug"=>"CmsSlug",
                "Key"=>"CmsKey", "ChangeNote"=>"CmsNote", "DisplayOrder"=>"CmsOrder", "ParallaxIntensity"=>"CmsParallax",
                "Value"=>"CmsValue", "Url" or "Href"=>"CmsTarget", "UsageKey"=>"CmsUsage", "SiteName"=>"SiteName",
                "Email"=>"AdminEmail", "Password"=>"AdminPassword", _=>null };
            if(key is not null)context.DisplayMetadata.DisplayName=()=>text[key];
            else if(context.Key.ModelType==typeof(string))context.DisplayMetadata.DisplayName=()=>text["ValidationField"];
        }
    }
}
