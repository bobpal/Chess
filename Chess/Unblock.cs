using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;

namespace Chess
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal static class Unblock
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetFileAttributes(string fileName);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteFile(string name);

        private static readonly char[] InvalidStreamNameChars = Path.GetInvalidFileNameChars().Where(c => c < 1 || c > 31).ToArray();
        private static global::System.Resources.ResourceManager resourceMan;
        private static global::System.Globalization.CultureInfo resourceCulture;

        public const char StreamSeparator = ':';
        public const int MaxPath = 256;
        private const int ErrorFileNotFound = 2;
        private const string LongPathPrefix = @"\\?\";

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        private static extern int FormatMessage(
            int dwFlags,
            IntPtr lpSource,
            int dwMessageId,
            int dwLanguageId,
            StringBuilder lpBuffer,
            int nSize,
            IntPtr vaListArguments);

        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }

        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Trinet.Core.IO.Ntfs.Properties.Resources", typeof(Properties.Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        internal static string Error_DriveNotFound
        {
            get
            {
                return ResourceManager.GetString("Error_DriveNotFound", resourceCulture);
            }
        }

        internal static string Error_InvalidFileChars
        {
            get
            {
                return ResourceManager.GetString("Error_InvalidFileChars", resourceCulture);
            }
        }

        internal static string Error_SharingViolation
        {
            get
            {
                return ResourceManager.GetString("Error_SharingViolation", resourceCulture);
            }
        }

        internal static string Error_DirectoryNotFound
        {
            get
            {
                return ResourceManager.GetString("Error_DirectoryNotFound", resourceCulture);
            }
        }

        internal static string Error_FileAlreadyExists
        {
            get
            {
                return ResourceManager.GetString("Error_FileAlreadyExists", resourceCulture);
            }
        }

        internal static string Error_AlreadyExists
        {
            get
            {
                return ResourceManager.GetString("Error_AlreadyExists", resourceCulture);
            }
        }

        internal static string Error_AccessDenied_Path
        {
            get
            {
                return ResourceManager.GetString("Error_AccessDenied_Path", resourceCulture);
            }
        }

        internal static string Error_UnknownError
        {
            get
            {
                return ResourceManager.GetString("Error_UnknownError", resourceCulture);
            }
        }

        private static string GetErrorMessage(int errorCode)
        {
            var lpBuffer = new StringBuilder(0x200);
            if (0 != FormatMessage(0x3200, IntPtr.Zero, errorCode, 0, lpBuffer, lpBuffer.Capacity, IntPtr.Zero))
            {
                return lpBuffer.ToString();
            }

            return string.Format(Properties.Resources.Culture, Error_UnknownError, errorCode);
        }

        public static bool DeleteAlternateDataStream(FileSystemInfo file, string streamName)
        {
            if (null == file) throw new ArgumentNullException("file");
            ValidateStreamName(streamName);

            const FileIOPermissionAccess permAccess = FileIOPermissionAccess.Write;
            new FileIOPermission(permAccess, file.FullName).Demand();

            var result = false;
            if (file.Exists)
            {
                string path = BuildStreamPath(file.FullName, streamName);
                if (-1 != SafeGetFileAttributes(path))
                {
                    result = SafeDeleteFile(path);
                }
            }

            return result;
        }

        public static void ValidateStreamName(string streamName)
        {
            if (!string.IsNullOrEmpty(streamName) && -1 != streamName.IndexOfAny(InvalidStreamNameChars))
            {
                throw new ArgumentException(Error_InvalidFileChars);
            }
        }

        public static string BuildStreamPath(string filePath, string streamName)
        {
            string result = filePath;
            if (!string.IsNullOrEmpty(filePath))
            {
                if (1 == result.Length) result = ".\\" + result;
                result += StreamSeparator + streamName + StreamSeparator + "$DATA";
                if (MaxPath <= result.Length) result = LongPathPrefix + result;
            }
            return result;
        }

        public static int SafeGetFileAttributes(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            int result = GetFileAttributes(name);
            if (-1 == result)
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (ErrorFileNotFound != errorCode) ThrowLastIOError(name);
            }

            return result;
        }

        public static bool SafeDeleteFile(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            bool result = DeleteFile(name);
            if (!result)
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (ErrorFileNotFound != errorCode) ThrowLastIOError(name);
            }

            return result;
        }

        public static void ThrowLastIOError(string path)
        {
            int errorCode = Marshal.GetLastWin32Error();
            if (0 != errorCode)
            {
                int hr = Marshal.GetHRForLastWin32Error();
                if (0 <= hr) throw new Win32Exception(errorCode);
                ThrowIOError(errorCode, path);
            }
        }

        private static int MakeHRFromErrorCode(int errorCode)
        {
            return (-2147024896 | errorCode);
        }

        private static void ThrowIOError(int errorCode, string path)
        {
            switch (errorCode)
            {
                case 0:
                    {
                        break;
                    }
                case 2: // File not found
                    {
                        if (string.IsNullOrEmpty(path)) throw new FileNotFoundException();
                        throw new FileNotFoundException(null, path);
                    }
                case 3: // Directory not found
                    {
                        if (string.IsNullOrEmpty(path)) throw new DirectoryNotFoundException();
                        throw new DirectoryNotFoundException(string.Format(Properties.Resources.Culture, Error_DirectoryNotFound, path));
                    }
                case 5: // Access denied
                    {
                        if (string.IsNullOrEmpty(path)) throw new UnauthorizedAccessException();
                        throw new UnauthorizedAccessException(string.Format(Properties.Resources.Culture, Error_AccessDenied_Path, path));
                    }
                case 15: // Drive not found
                    {
                        if (string.IsNullOrEmpty(path)) throw new DriveNotFoundException();
                        throw new DriveNotFoundException(string.Format(Properties.Resources.Culture, Error_DriveNotFound, path));
                    }
                case 32: // Sharing violation
                    {
                        if (string.IsNullOrEmpty(path)) throw new IOException(GetErrorMessage(errorCode), MakeHRFromErrorCode(errorCode));
                        throw new IOException(string.Format(Properties.Resources.Culture, Error_SharingViolation, path), MakeHRFromErrorCode(errorCode));
                    }
                case 80: // File already exists
                    {
                        if (!string.IsNullOrEmpty(path))
                        {
                            throw new IOException(string.Format(Properties.Resources.Culture, Error_FileAlreadyExists, path), MakeHRFromErrorCode(errorCode));
                        }
                        break;
                    }
                case 87: // Invalid parameter
                    {
                        throw new IOException(GetErrorMessage(errorCode), MakeHRFromErrorCode(errorCode));
                    }
                case 183: // File or directory already exists
                    {
                        if (!string.IsNullOrEmpty(path))
                        {
                            throw new IOException(string.Format(Properties.Resources.Culture, Error_AlreadyExists, path), MakeHRFromErrorCode(errorCode));
                        }
                        break;
                    }
                case 206: // Path too long
                    {
                        throw new PathTooLongException();
                    }
                case 995: // Operation cancelled
                    {
                        throw new OperationCanceledException();
                    }
                default:
                    {
                        Marshal.ThrowExceptionForHR(MakeHRFromErrorCode(errorCode));
                        break;
                    }
            }
        }
    }
}
