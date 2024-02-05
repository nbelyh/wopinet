using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using WopiCore.Services;

namespace WopiTest.Controllers
{
    [Route("[controller]")]
    public class TestController : Controller
    {
        static readonly IAuthInfo Auth = new AuthInfo
        {
            UserId = "nikolay",
            UserFriendlyName = "Nikolay"
        };


        IWopiDbRepository db;
        IWopiStorageRepository storage;

        WopiDiscovery wopiDiscovery;
        WopiSecurity wopiSecurity;

        public TestController(IWopiDbRepository db, IWopiStorageRepository storage, WopiDiscovery wopiDiscovery, WopiSecurity wopiSecurity)
        {
            this.db = db;
            this.storage = storage;
            this.wopiDiscovery = wopiDiscovery;
            this.wopiSecurity = wopiSecurity;
        }

        [HttpGet]
        [Route("/")]
        public async Task<IActionResult> Index()
        {
            var files = await db.GetAllFilesAsync(Auth);
            return View(files);
        }

        public async Task<IActionResult> Open(string action, string ext, string fileid)
        {
            var token = wopiSecurity.GenerateToken(fileid, Auth);
            ViewData["wopi_access_token"] = wopiSecurity.WriteToken(token);
            ViewData["wopi_access_token_ttl"] = (long)token.ValidTo.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            ViewData["wopi_src"] = await wopiDiscovery.GetActionUrl(action, ext, fileid, Auth);
            return View("Open");
        }

        [HttpGet]
        [Route("/edit/{ext}/{fileid}")]
        public async Task<IActionResult> Edit(string ext, string fileid)
        {
            return await Open("edit", ext, fileid);
        }

        [HttpGet]
        [Route("/view/{ext}/{fileid}")]
        public async Task<IActionResult> View(string ext, string fileid)
        {
            return await Open("view", ext, fileid);
        }

        [HttpPost]
        [Route("/upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var wopiFile = new WopiFile
            {
                FileId = file.FileName,
                FileName = file.FileName,
                OwnerId = Auth.UserId,
                Size = file.Length,
                Version = 1,
                LastModifiedTime = DateTimeOffset.Now,
                LastModifiedUser = Auth.UserId
            };

            await storage.PutBlobContentAsync(file.FileName, file.OpenReadStream(), Auth);
            await db.AddFileAsync(wopiFile, Auth);
            return Redirect("/");
        }

        [HttpGet]
        [Route("/delete/{fileid}")]
        public async Task<IActionResult> Delete(string fileid)
        {
            await storage.DeleteBlobAsync(fileid, Auth);
            await db.DeleteFileAsync(fileid, Auth);
            return Redirect("/");
        }

        class FileModel
        {
            public string FileId {get;set;}
            public string FileName {get;set;}
            public string FileExtension {get;set;}
            public string ViewUrl {get;set;}
            public string EditUrl {get;set;}
            public string Token { get;set;}
            public long TokenTtl { get;set;}
        }

        [HttpGet]
        [Route("/api/Files")]
        public async Task<IActionResult> Files()
        {
            var wopiFiles = await db.GetAllFilesAsync(Auth);

            var files = wopiFiles.Select(f => new FileModel 
            {
                FileName = f.FileName,
                FileId = f.FileId,
                ViewUrl = string.Empty,
                EditUrl = string.Empty,
                Token = string.Empty,
            }).ToList();

            foreach (var file in files)
            {
                var token = wopiSecurity.GenerateToken(file.FileId, Auth);
                file.ViewUrl = await wopiDiscovery.GetActionUrl("view", file.FileExtension, file.FileId, Auth);
                file.EditUrl = await wopiDiscovery.GetActionUrl("edit", file.FileExtension, file.FileId, Auth);
                file.Token = wopiSecurity.WriteToken(token);
                file.TokenTtl = (long)token.ValidTo.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            }

            return Json(files);
        }

        [HttpPost]
        [Route("/apiupload")]
        public async Task ApiUpload(IFormFile file)
        {
            var wopiFile = new WopiFile
            {
                FileId = file.FileName,
                FileName = file.FileName,
                OwnerId = Auth.UserId,
                Size = file.Length,
                Version = 1,
                LastModifiedTime = DateTimeOffset.Now,
                LastModifiedUser = Auth.UserId
            };

            await storage.PutBlobContentAsync(file.FileName, file.OpenReadStream(), Auth);
            await db.AddFileAsync(wopiFile, Auth);
        }

        [HttpPost]
        [Route("/apidelete/{fileid}")]
        public async Task ApiDelete(string fileid)
        {
            await storage.DeleteBlobAsync(fileid, Auth);
            await db.DeleteFileAsync(fileid, Auth);
        }

    }
}