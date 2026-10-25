using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using NLOIQ.Application.Content;
using NLOIQ.Infrastructure.Data;
using NLOIQ.Domain.Entities;
using NLOIQ.Web.ViewModels;
namespace NLOIQ.Web.Controllers;
[Route("{culture:regex(^(en|ar)$)}/contact")]
[ResponseCache(NoStore=true,Location=ResponseCacheLocation.None)]
public sealed class ContactController(NloDbContext db,IPublicSiteReader reader,Microsoft.Extensions.Localization.IStringLocalizer<NLOIQ.Web.SharedResource> text):Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string culture,CancellationToken ct) => await reader.ReadContactAsync(culture,ct) is null?NotFound():View(new ContactInquiryModel());
    [HttpPost(""),ValidateAntiForgeryToken,EnableRateLimiting("inquiry"),RequestSizeLimit(32768)]
    public async Task<IActionResult> Submit(string culture,ContactInquiryModel model,CancellationToken ct)
    {
        if(await reader.ReadContactAsync(culture,ct) is null)return NotFound();
        if(!string.IsNullOrEmpty(model.Website))return RedirectToAction(nameof(Thanks),new{culture});
        if(model.SubmissionId==Guid.Empty)ModelState.AddModelError("",text["InquiryInvalidSubmission"]);
        if((model.Name?.Trim().Length??0)<2||(model.Message?.Trim().Length??0)<10)ModelState.AddModelError("",text["InquiryInvalidSubmission"]);
        if(!ModelState.IsValid){Response.StatusCode=400;return View("Index",model);}
        if(await db.ContactInquiries.AnyAsync(x=>x.SubmissionId==model.SubmissionId,ct))return RedirectToAction(nameof(Thanks),new{culture});
        db.ContactInquiries.Add(new ContactInquiry{SubmissionId=model.SubmissionId,CreatedUtc=DateTime.UtcNow,Culture=culture,Name=model.Name!.Trim(),Email=model.Email.Trim(),Phone=model.Phone?.Trim()??"",Company=model.Company?.Trim()??"",Service=model.Service,Message=model.Message!.Trim()});
        try{await db.SaveChangesAsync(ct);}catch(DbUpdateException ex)when(ex.InnerException is MySqlException{Number:1062}){db.ChangeTracker.Clear();if(!await db.ContactInquiries.AnyAsync(x=>x.SubmissionId==model.SubmissionId,ct))throw;}
        return RedirectToAction(nameof(Thanks),new{culture});
    }
    [HttpGet("thanks")]
    public IActionResult Thanks()=>View();
}
