using DevPartner.Nop.Plugin.CloudStorage.Domain;
using System;
using System.Threading.Tasks;

namespace DevPartner.Nop.Plugin.CloudStorage.Cloud
{
    /// <summary>
    /// Cloud Storage Provider Service Interface
    /// </summary>
    public interface ICloudStorageProvider
    {
        /// <summary>
        /// Delete file
        /// </summary>
        /// <param name="fileName">File Name</param>
        void DeleteFile(string fileName);

        /// <summary>
        /// Delete file prefix
        /// </summary>
        /// <param name="filter">File Prefix</param>
        // void DeleteFileByPrefix(string filter);


        /// <summary>
        /// Writes the specified byte array to the file
        /// </summary>
        /// <param name="filePath">The file to write to</param>
        /// <param name="bytes">The bytes to write to the file</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task WriteAllBytesAsync(string filePath, byte[] bytes);
        
        /// <summary>
        /// Copy file from source path to target
        /// </summary>
        /// <param name="sourcePath">Source File Path</param>
        /// <param name="targetPath">Target File Path</param>
        /// <param name="overwrite">true if the destination file can be overwritten; otherwise, false</param>
        void FileCopy(string sourcePath, string targetPath, bool overwrite = false);
        /*
        /// <summary>
        /// Renames file
        /// </summary>
        /// <param name="path">File Path</param>
        /// <param name="newName">New Name</param>
        void RenameFile(string path, string newName);*/

        /// <summary>
        /// Reads the contents of the file into a byte array
        /// </summary>
        /// <param name="filePath">The file for reading</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains a byte array containing the contents of the file
        /// </returns
        Task<byte[]> ReadAllBytesAsync(string filePath);

        /// <summary>
        /// Returns the names of files (including their paths) that match the specified search
        /// pattern in the specified directory, using a value to determine whether to search subdirectories.
        /// </summary>
        /// <param name="directoryPath">The path to the directory to search</param>
        /// <param name="searchPattern">
        /// The search string to match against the names of files in path. This parameter
        /// can contain a combination of valid literal path and wildcard (* and ?) characters
        /// , but doesn't support regular expressions.
        /// </param>
        /// <param name="topDirectoryOnly">
        /// Specifies whether to search the current directory, or the current directory and all
        /// subdirectories
        /// </param>
        /// <returns>
        /// An array of the full names (including paths) for the files in the specified directory
        /// that match the specified search pattern, or an empty array if no files are found.
        /// </returns>
        string[] GetFiles(string directoryPath, string searchPattern = "", bool topDirectoryOnly = true);

        /// <summary>
        /// Returns the names of the subdirectories (including their paths) that match the
        /// specified search pattern in the specified directory
        /// </summary>
        /// <param name="path">The path to the directory to search</param>
        /// <param name="searchPattern">
        /// The search string to match against the names of subdirectories in path. This
        /// parameter can contain a combination of valid literal and wildcard characters
        /// , but doesn't support regular expressions.
        /// </param>
        /// <param name="topDirectoryOnly">
        /// Specifies whether to search the current directory, or the current directory and all
        /// subdirectories
        /// </param>
        /// <returns>
        /// An array of the full names (including paths) of the subdirectories that match
        /// the specified criteria, or an empty array if no directories are found
        /// </returns>
        string[] GetDirectories(string path, string searchPattern = "", bool topDirectoryOnly = true);

        /// <summary>
        /// Returns the directory name only for the specified path string
        /// </summary>
        /// <param name="path">The path of directory</param>
        /// <returns>The directory name</returns>
        //string GetDirectoryNameOnly(string path);


        /// <summary>
        /// Moves a file or a directory and its contents to a new location
        /// </summary>
        /// <param name="sourceDirName">The path of the file or directory to move</param>
        /// <param name="destDirName">
        /// The path to the new location for sourceDirName. If sourceDirName is a file, then destDirName
        /// must also be a file name
        /// </param>
        void DirectoryMove(string sourceDirName, string destDirName);
        /// <summary>
        /// Determines whether the given path refers to an existing directory on disk
        /// </summary>
        /// <param name="path">The path to test</param>
        /// <returns>
        /// true if path refers to an existing directory; false if the directory does not exist or an error occurs when
        /// trying to determine if the specified file exists
        /// </returns>
        bool DirectoryExists(string path);

        /// <summary>
        /// Depth-first recursive delete, with handling for descendant directories open in Windows Explorer.
        /// </summary>
        /// <param name="path">Directory path</param>
        void DeleteDirectory(string path);
        ///// <summary>
        ///// Get root url
        ///// </summary>
        ///// <returns>Root url</returns>
        //string GetRootUrl();

        /// <summary>
        /// Get file data (size, upload date and etc)
        /// </summary>
        /// <param name="filePath">path (starting from root)</param>
        /// <returns>File Data</returns>
        FileData GetFileData(string filePath);

        /// <summary>
        /// Creates new directory in storage.
        /// </summary>
        /// <param name="sourcePath">Source Directory Path</param>
        /// <param name="targetPath">Target Directory Path</param>
        void CopyDirectory(string sourcePath, string targetPath);

        /// <summary>
        /// Renames directory in storage.
        /// </summary>
        /// <param name="path">path (starting from root)</param>
        /// <param name="newName">New directory name</param>
        Task RenameDirectoryAsync(string path, string newName);

        /// <summary>
        /// Returns the absolute path to the directory
        /// </summary>
        /// <param name="paths">An array of parts of the path</param>
        /// <returns>The absolute path to the directory</returns>
        //string GetAbsolutePath(string path);
        /// <summary>
        /// Creates all directories and subdirectories in the specified path unless they already exist
        /// </summary>
        /// <param name="path">The directory to create</param>
        void CreateDirectory(string path);

        /// <summary>
        /// Determines whether the specified file exists
        /// </summary>
        /// <param name="filePath">The file to check</param>
        /// <returns>
        /// True if the caller has the required permissions and path contains the name of an existing file; otherwise,
        /// false.
        /// </returns>
        bool FileExists(string path);

        /// <summary>
        /// Gets a virtual path from a physical disk path.
        /// </summary>
        /// <param name="path">The physical disk path</param>
        /// <returns>The virtual path. E.g. "~/bin"</returns>
        string GetVirtualPath(string filePath);

        /// <summary>
        /// Gets the length of the file in bytes, or -1 for a directory or non-existing files
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns>The length of the file</returns>
        long FileLength(string filePath);

        /// <summary>
        /// Moves a specified file to a new location, providing the option to specify a new file name
        /// </summary>
        /// <param name="sourceFileName">The name of the file to move. Can include a relative or absolute path</param>
        /// <param name="destFileName">The new path and name for the file</param>
        void FileMove(string filePath, string destFilePath);

        /// <summary>
        /// Returns the date and time the specified file or directory was last written to
        /// </summary>
        /// <param name="path">The file or directory for which to obtain write date and time information</param>
        /// <returns>
        /// A System.DateTime structure set to the date and time that the specified file or directory was last written to.
        /// This value is expressed in local time
        /// </returns>
        DateTime GetLastWriteTime(string path);

        /// <summary>
        /// Retrieves the parent directory of the specified path
        /// </summary>
        /// <param name="directoryPath">The path for which to retrieve the parent directory</param>
        /// <returns>The parent directory, or null if path is the root directory, including the root of a UNC server or share name</returns>
        string GetParentDirectory(string filePath);
    }
}
