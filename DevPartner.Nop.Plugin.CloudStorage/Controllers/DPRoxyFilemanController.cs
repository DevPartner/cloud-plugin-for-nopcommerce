using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using DevPartner.Nop.Plugin.CloudStorage.Cloud;
using DevPartner.Nop.Plugin.CloudStorage.Extensions;
using DevPartner.Nop.Plugin.CloudStorage.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace DevPartner.Nop.Plugin.CloudStorage.Controllers
{
    //Controller for Roxy fileman (http://www.roxyfileman.com/) for TinyMCE editor
    //the original file was \RoxyFileman-1.4.5-net\fileman\asp_net\main.ashx

    //do not validate request token (XSRF)
    [AdminAntiForgery(true)]
    public class DPRoxyFilemanController : RoxyFilemanController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly ICloudContentProvider _contentProvider;
        private readonly INopFileProvider _fileProvider;

        #endregion
        #region Ctor

        public DPRoxyFilemanController(IHostingEnvironment hostingEnvironment,
            IPermissionService permissionService,
            IWorkContext workContext,
            ICloudContentProvider contentProvider,
            INopFileProvider fileProvider)
            : base(hostingEnvironment, fileProvider, permissionService, workContext)
        {
            _permissionService = permissionService;
            _contentProvider = contentProvider.IsNotNull() as ICloudContentProvider;
            _fileProvider = fileProvider;
        }

        #endregion

        #region Methods
        

        /// <summary>
        /// Process request
        /// </summary>
        public override void ProcessRequest()
        {
            //async requests are disabled in the js code, so use .Wait() method here
            ProcessRequestAsync().Wait();
        }

        #endregion

        #region Utitlies

        /// <summary>
        /// Process the incoming request
        /// </summary>
        /// <returns>A task that represents the completion of the operation</returns>
        protected override async Task ProcessRequestAsync()
        {
            var action = "DIRLIST";
            try
            {
                if (!_permissionService.Authorize(StandardPermissionProvider.HtmlEditorManagePictures))
                    throw new Exception("You don't have required permission");

                if (!StringValues.IsNullOrEmpty(HttpContext.Request.Query["a"]))
                    action = HttpContext.Request.Query["a"];

                if (_contentProvider == null)
                {

                    switch (action.ToUpper())
                    {
                        case "DIRLIST":
                            await GetDirectoriesAsync(HttpContext.Request.Query["type"]);
                            break;
                        case "FILESLIST":
                            await GetFilesAsync(HttpContext.Request.Query["d"], HttpContext.Request.Query["type"]);
                            break;
                        case "COPYDIR":
                            await CopyDirectoryAsync(HttpContext.Request.Query["d"], HttpContext.Request.Query["n"]);
                            break;
                        case "COPYFILE":
                            await CopyFileAsync(HttpContext.Request.Query["f"], HttpContext.Request.Query["n"]);
                            break;
                        case "CREATEDIR":
                            await CreateDirectoryAsync(HttpContext.Request.Query["d"], HttpContext.Request.Query["n"]);
                            break;
                        case "DELETEDIR":
                            await DeleteDirectoryAsync(HttpContext.Request.Query["d"]);
                            break;
                        case "DELETEFILE":
                            await DeleteFileAsync(HttpContext.Request.Query["f"]);
                            break;
                        case "DOWNLOAD":
                            await DownloadFileAsync(HttpContext.Request.Query["f"]);
                            break;
                        case "DOWNLOADDIR":
                            await DownloadDirectoryAsync(HttpContext.Request.Query["d"]);
                            break;
                        case "MOVEDIR":
                            await MoveDirectoryAsync(HttpContext.Request.Query["d"], HttpContext.Request.Query["n"]);
                            break;
                        case "MOVEFILE":
                            await MoveFileAsync(HttpContext.Request.Query["f"], HttpContext.Request.Query["n"]);
                            break;
                        case "RENAMEDIR":
                            await RenameDirectoryAsync(HttpContext.Request.Query["d"], HttpContext.Request.Query["n"]);
                            break;
                        case "RENAMEFILE":
                            await RenameFileAsync(HttpContext.Request.Query["f"], HttpContext.Request.Query["n"]);
                            break;
                        case "GENERATETHUMB":
                            int.TryParse(HttpContext.Request.Query["width"].ToString().Replace("px", ""), out int w);
                            int.TryParse(HttpContext.Request.Query["height"].ToString().Replace("px", ""), out int h);
                            CreateThumbnail(HttpContext.Request.Query["f"], w, h);
                            break;
                        case "UPLOAD":
                            await UploadFilesAsync(HttpContext.Request.Form["d"]);
                            break;
                        default:
                            await HttpContext.Response.WriteAsync(GetErrorResponse("This action is not implemented."));
                            break;
                    }
                }
                else
                {
                    switch (action.ToUpper())
                    {
                        case "DIRLIST":
                            await GetDirectoriesCloudAsync(HttpContext.Request.Query["type"]);
                            break;
                        case "FILESLIST":
                            await GetFilesCloudAsync(HttpContext.Request.Query["d"], HttpContext.Request.Query["type"]);
                            break;
                        case "COPYDIR":
                            await CopyDirectoryCloudAsync(HttpContext.Request.Query["d"], HttpContext.Request.Query["n"]);
                            break;
                        case "COPYFILE":
                            _contentProvider.CopyFile(HttpContext.Request.Query["f"], HttpContext.Request.Query["n"]);
                            break;
                        case "CREATEDIR":
                            //_contentProvider.CreateDirectory(HttpContext.Request.Query["d"], HttpContext.Request.Query["n"]);
                            break;
                        case "DELETEDIR":
                            //_contentProvider.DeleteDirectory(HttpContext.Request.Query["d"]);
                            break;
                        case "DELETEFILE":
                            _contentProvider.DeleteFile(HttpContext.Request.Query["f"]);
                            break;
                        case "DOWNLOAD":
                            DownloadFile(HttpContext.Request.Query["f"]);
                            break;
                        case "DOWNLOADDIR":
                            DownloadDirectory(HttpContext.Request.Query["d"]);
                            break;
                        case "MOVEDIR":
                            _contentProvider.MoveDirectory(HttpContext.Request.Query["d"], HttpContext.Request.Query["n"]);
                            break;
                        case "MOVEFILE":
                            _contentProvider.MoveFile(HttpContext.Request.Query["f"], HttpContext.Request.Query["n"]);
                            break;
                        case "RENAMEDIR":
                            _contentProvider.RenameDirectory(HttpContext.Request.Query["d"], HttpContext.Request.Query["n"]);
                            break;
                        case "RENAMEFILE":
                            _contentProvider.RenameFile(HttpContext.Request.Query["f"], HttpContext.Request.Query["n"]);
                            break;
                        case "GENERATETHUMB":
                            int.TryParse(HttpContext.Request.Query["width"].ToString().Replace("px", ""), out int w);
                            int.TryParse(HttpContext.Request.Query["height"].ToString().Replace("px", ""), out int h);
                            CreateThumbnail(HttpContext.Request.Query["f"], w, h);
                            break;
                        case "UPLOAD":
                            await UploadFilesAsync(HttpContext.Request.Form["d"]);
                            break;
                        default:
                            await HttpContext.Response.WriteAsync(GetErrorResponse("This action is not implemented."));
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (action == "UPLOAD" && !IsAjaxRequest())
                    await HttpContext.Response.WriteAsync($"<script>parent.fileUploaded({GetErrorResponse(GetLanguageResource("E_UploadNoFiles"))});</script>");
                else
                    await HttpContext.Response.WriteAsync(GetErrorResponse(ex.Message));
            }
        }


        #endregion


        #region Methods

        /// <summary>
        /// Copy the directory
        /// </summary>
        /// <param name="sourcePath">Path to the source directory</param>
        /// <param name="destinationPath">Path to the destination directory</param>
        /// <returns>A task that represents the completion of the operation</returns>
        protected virtual async Task CopyDirectoryCloudAsync(string sourcePath, string destinationPath)
        {
            var directoryPath = GetFullPath(GetVirtualPath(sourcePath));
            var directory = _contentProvider.GetFileData(directoryPath);
            //var directory = new DirectoryInfo(directoryPath);
            if (directory==null)
                throw new Exception(GetLanguageResource("E_CopyDirInvalidPath"));

            var newDirectoryPath = $"{destinationPath.TrimEnd('/')}/{Path.GetDirectoryName(sourcePath)}";
            var newDirectory = _contentProvider.GetFileData(newDirectoryPath);
            if (newDirectory!=null)
                throw new Exception(GetLanguageResource("E_DirAlreadyExists"));

            _contentProvider.CopyDirectory(sourcePath, destinationPath);

            await HttpContext.Response.WriteAsync(GetSuccessResponse());
        }

        /// <summary>
        /// Get files in the passed directory
        /// </summary>
        /// <param name="directoryPath">Path to the files directory</param>
        /// <param name="type">Type of the files</param>
        /// <returns>A task that represents the completion of the operation</returns>
        protected virtual async Task GetFilesCloudAsync(string directoryPath, string type)
        {
            var files = GetFiles(directoryPath, type);

            await HttpContext.Response.WriteAsync("[");
            for (var i = 0; i < files.Count; i++)
            {
                var width = 0;
                var height = 0;
                var fileData = _contentProvider.GetFileData(files[i]);
                //var file = new FileInfo(files[i]);
                var extension = Path.GetExtension(files[i]);
                var fileName = Path.GetFileName(files[i]);

                if (GetFileType(extension) == "image")
                {
                    try
                    {
                        using (var stream = new MemoryStream())
                        {
                            _contentProvider.DownloadToStream(files[i], stream);
                            using (var image = Image.FromStream(stream))
                            {
                                width = image.Width;
                                height = image.Height;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                await HttpContext.Response.WriteAsync($"{{\"p\":\"{directoryPath.TrimEnd('/')}/{fileName}\",\"t\":\"{Math.Ceiling(GetTimestamp(fileData.LastWriteTime))}\",\"s\":\"{fileData.Length}\",\"w\":\"{width}\",\"h\":\"{height}\"}}");

                if (i < files.Count - 1)
                    await HttpContext.Response.WriteAsync(",");
            }
            await HttpContext.Response.WriteAsync("]");
        }

        //done; working
        /// <summary>
        /// Get files in the passed directory
        /// </summary>
        /// <param name="directoryPath">Path to the files directory</param>
        /// <param name="type">Type of the files</param>
        /// <returns>List of paths to the files</returns>
        protected override List<string> GetFiles(string directoryPath, string type)
        {
            if (type == "#")
                type = string.Empty;

            var files = new List<string>();

            var allFiles = _contentProvider?.GetFiles(directoryPath, false, true).ToArray() 
                ?? Directory.GetFiles(directoryPath);

            foreach (var fileName in allFiles)
            {
                if (string.IsNullOrEmpty(type) || GetFileType(new FileInfo(fileName).Extension) == type)
                    files.Add(fileName);
            }

            return files;
        }

        /// <summary>
        /// Get all available directories as a directory tree
        /// </summary>
        /// <param name="type">Type of the file</param>
        /// <returns>A task that represents the completion of the operation</returns>
        protected virtual async Task GetDirectoriesCloudAsync(string type)
        {
            var allDirectories = _contentProvider.GetDirectories("", true);
            await HttpContext.Response.WriteAsync("[");
            for (var i = 0; i < allDirectories.Count; i++)
            {
                var directoryPath = (string)allDirectories[i];
                await HttpContext.Response.WriteAsync($"{{\"p\":\"/{directoryPath.Replace("\\", "/").TrimStart('/')}\",\"f\":\"{GetFiles(directoryPath, type).Count}\",\"d\":\"{ _contentProvider.GetDirectories(directoryPath, true).Count}\"}}");
                if (i < allDirectories.Count - 1)
                    await HttpContext.Response.WriteAsync(",");
            }
            await HttpContext.Response.WriteAsync("]");
        }



        /// <summary>
        /// Download the file from the server
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>A task that represents the completion of the operation</returns>
        protected virtual async Task BaseDownloadFileAsync(string path)
        {
            var filePath = GetFullPath(GetVirtualPath(path));
            var file = new FileInfo(filePath);
            if (file.Exists)
            {
                HttpContext.Response.Clear();
                HttpContext.Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{file.Name}\"");
                HttpContext.Response.ContentType = MimeTypes.ApplicationForceDownload;
                await HttpContext.Response.SendFileAsync(file.FullName);
            }
        }

        /// <summary>
        /// Download the file from the server
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>A task that represents the completion of the operation</returns>
        protected virtual void DownloadFile(string path)
        {
            var tempFolderName = Guid.NewGuid().ToString();
            var templFolderPathRel = "../tmp/" + tempFolderName;
            var bytes = _contentProvider.GetFile(path);
            var tmpFile = _fileProvider.MapPath(templFolderPathRel + "/" + Path.GetFileName(path));
            if (System.IO.File.Exists(_fileProvider.MapPath(tmpFile)))
                System.IO.File.Delete(_fileProvider.MapPath(tmpFile));
            System.IO.File.WriteAllBytes(tmpFile, bytes);
            BaseDownloadFileAsync(path).Wait();
            System.IO.File.Delete(tmpFile);
        }


        protected void DownloadDirectory(string path)
        {
            var dirName = path.Split("/".ToCharArray()).Last();
            var files = _contentProvider.GetDirectories(path);
            var tempFolderName = Guid.NewGuid().ToString();
            var templFolderPathRel = "../tmp/" + tempFolderName;
            if (System.IO.File.Exists(_fileProvider.MapPath(templFolderPathRel)))
                System.IO.File.Delete(_fileProvider.MapPath(templFolderPathRel));

            foreach (var filePath in files)
            {
                var fileDir = templFolderPathRel + "/" + dirName + "/" + Path.GetDirectoryName(filePath.Substring(path.Length));
                Directory.CreateDirectory(_fileProvider.MapPath(fileDir));

                var bytes = _contentProvider.GetFile(filePath);
                System.IO.File.WriteAllBytes(_fileProvider.MapPath(fileDir + "/" + Path.GetFileName(filePath)), bytes);
            }

            var tmpZipAbs = _fileProvider.MapPath("../tmp/" + templFolderPathRel + "/" + dirName + ".zip");

            ZipFile.CreateFromDirectory(_fileProvider.MapPath(templFolderPathRel + "/" + dirName), tmpZipAbs,
                CompressionLevel.Fastest, true);

            BaseDownloadFileAsync(tmpZipAbs).Wait();
            
            System.IO.File.Delete(tmpZipAbs);
        }


        /// <summary>
        /// Upload files to a directory on passed path
        /// </summary>
        /// <param name="directoryPath">Path to directory to upload files</param>
        /// <returns>A task that represents the completion of the operation</returns>
        protected override async Task UploadFilesAsync(string directoryPath)
        {
            var result = GetSuccessResponse();
            var hasErrors = false;
            try
            {
                directoryPath = GetFullPath(GetVirtualPath(directoryPath));
                for (var i = 0; i < HttpContext.Request.Form.Files.Count; i++)
                {
                    var fileName = HttpContext.Request.Form.Files[i].FileName;
                    if (CanHandleFile(fileName))
                    {
                        var file = new FileInfo(fileName);
                        var uniqueFileName = GetUniqueFileName(directoryPath, file.Name);
                        var destinationFile = Path.Combine(directoryPath, uniqueFileName);
                        if (_contentProvider == null)
                        {
                            using (var stream = new FileStream(destinationFile, FileMode.OpenOrCreate))
                            {
                                HttpContext.Request.Form.Files[i].CopyTo(stream);
                            }
                        }
                        else
                        {
                            //byte[] bytes = new byte[HttpContext.Request.Form.Files[i].Length];
                            using (var stream = new MemoryStream())
                            {
                                HttpContext.Request.Form.Files[i].CopyTo(stream);
                                _contentProvider.InsertFile(destinationFile,
                                    HttpContext.Request.Form.Files[i].ContentType, stream.GetBuffer());
                            }
                            }
                        if (GetFileType(new FileInfo(uniqueFileName).Extension) == "image")
                        {
                            int.TryParse(GetSetting("MAX_IMAGE_WIDTH"), out int w);
                            int.TryParse(GetSetting("MAX_IMAGE_HEIGHT"), out int h);
                            ImageResize(destinationFile, destinationFile, w, h);
                        }
                    }
                    else
                    {
                        hasErrors = true;
                        result = GetErrorResponse(GetLanguageResource("E_UploadNotAll"));
                    }
                }
            }
            catch (Exception ex)
            {
                result = GetErrorResponse(ex.Message);
            }
            if (IsAjaxRequest())
            {
                if (hasErrors)
                    result = GetErrorResponse(GetLanguageResource("E_UploadNotAll"));

                await HttpContext.Response.WriteAsync(result);
            }
            else
                await HttpContext.Response.WriteAsync($"<script>parent.fileUploaded({result});</script>");
        }

        /// <summary>
        /// Resize the image
        /// </summary>
        /// <param name="sourcePath">Path to the source image</param>
        /// <param name="destinstionPath">Path to the destination image</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        protected override void ImageResize(string sourcePath, string destinstionPath, int width, int height)
        {

            using (var stream = _contentProvider == null ?
                new FileStream(sourcePath, FileMode.Open, FileAccess.Read) :
                GetCloudStream(sourcePath))
            {
                using (var image = Image.FromStream(stream))
                {
                    var ratio = (float)image.Width / (float)image.Height;
                    if ((image.Width <= width && image.Height <= height) || (width == 0 && height == 0))
                        return;

                    var newWidth = width;
                    int newHeight = Convert.ToInt16(Math.Floor((float)newWidth / ratio));
                    if ((height > 0 && newHeight > height) || (width == 0))
                    {
                        newHeight = height;
                        newWidth = Convert.ToInt16(Math.Floor((float)newHeight * ratio));
                    }

                    using (var newImage = new Bitmap(newWidth, newHeight))
                    {
                        using (var graphics = Graphics.FromImage(newImage))
                        {
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.DrawImage(image, 0, 0, newWidth, newHeight);
                            if (!string.IsNullOrEmpty(destinstionPath))
                            {
                                if(_contentProvider == null)
                                    newImage.Save(destinstionPath, GetImageFormat(destinstionPath));
                                else
                                {
                                    using (var imageStream = new MemoryStream())
                                    {
                                        newImage.Save(imageStream, GetImageFormat(destinstionPath));
                                        _contentProvider.InsertFile(destinstionPath, GetImageMimeType(destinstionPath), imageStream.GetBuffer());
                                    }
                                    
                                }
                            }
                        }
                    }
                }
            }
        }

        private string GetImageMimeType(string destinstionPath)
        {
            var fileExtension = new FileInfo(destinstionPath).Extension.ToLower();
            switch (fileExtension)
            {
                case ".png":
                    return MimeTypes.ImagePng;
                case ".gif":
                    return MimeTypes.ImageGif;
                default:
                    return MimeTypes.ImageJpeg;
            }
        }


        private Stream GetCloudStream(string sourcePath)
        {
            var bytes =  _contentProvider.GetFile(sourcePath);
            var stream = new MemoryStream(bytes);
            return stream;
        }

        #endregion
    }
}