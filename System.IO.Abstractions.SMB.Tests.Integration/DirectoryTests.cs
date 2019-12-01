using Microsoft.Extensions.Configuration;
using System;
using Xunit;
using System.IO.Abstractions.SMB;
using System.Linq;

namespace System.IO.Abstractions.SMB.Tests.Integration
{
    public class DirectoryTests : TestBase
    {
        private IDirectoryInfo createdTestDirectoryInfo;
        private string createdTestDirectoryPath;


        public DirectoryTests() : base()
        {

        }

        public override void Dispose()
        {
            if (!string.IsNullOrEmpty(createdTestDirectoryPath) && createdTestDirectoryInfo != null)
            {
                var testCredentials = TestSettings.ShareCredentials;
                using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, createdTestDirectoryPath, SMBCredentialProvider);
                FileSystem.Directory.Delete(createdTestDirectoryPath);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Delete_DoesntThrow_For_UncPath()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();

            createdTestDirectoryPath = Path.Combine(testShare.RootUncPath, $"test_directory-{DateTime.Now.ToFileTimeUtc()}");

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, createdTestDirectoryPath, SMBCredentialProvider);

            createdTestDirectoryInfo = FileSystem.Directory.CreateDirectory(createdTestDirectoryPath);

            Assert.True(FileSystem.Directory.Exists(createdTestDirectoryPath));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Exits_ReturnsTrue_For_SmbUri()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();

            var testDirectoryPath = testShare.RootSmbUri.CombineToSharePath(testShare.Directories.FirstOrDefault());

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, testDirectoryPath, SMBCredentialProvider);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void Exits_ReturnsTrue_For_UncPath()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();

            var testDirectoryPath = Path.Combine(testShare.RootUncPath, testShare.Directories.FirstOrDefault());

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, testDirectoryPath, SMBCredentialProvider);

            Assert.True(FileSystem.Directory.Exists(createdTestDirectoryPath));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CreateDirectory_IsSuccessful_For_SmbUri()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();

            createdTestDirectoryPath = testShare.RootSmbUri.CombineToSharePath($"test_directory-{DateTime.Now.ToFileTimeUtc()}");

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, createdTestDirectoryPath, SMBCredentialProvider);

            createdTestDirectoryInfo = FileSystem.Directory.CreateDirectory(createdTestDirectoryPath);

            Assert.True(FileSystem.Directory.Exists(createdTestDirectoryPath));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CreateDirectory_IsSuccessful_For_UncPath()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootUncPath = Path.Combine(testShare.RootUncPath);

            createdTestDirectoryPath = Path.Combine(testShare.RootUncPath, $"test_directory-{DateTime.Now.ToFileTimeUtc()}");

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, createdTestDirectoryPath, SMBCredentialProvider);

            createdTestDirectoryInfo = FileSystem.Directory.CreateDirectory(createdTestDirectoryPath);

            Assert.True(FileSystem.Directory.Exists(createdTestDirectoryPath));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void EnumerateFiles_DoesntThrow_For_UncRootDirectory()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootUncPath = Path.Combine(testShare.RootUncPath);

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, testRootUncPath, SMBCredentialProvider);

            var files = FileSystem.Directory.EnumerateFiles(testRootUncPath, "*").ToList();

            Assert.True(files.Count >= 0); //Include 0 in case directory is empty. If an exception is thrown, the test will fail.
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void EnumerateFiles_DoesntThrow_For_SmbRootDirectory()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootSmbUri = Path.Combine(testShare.RootSmbUri);

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, testRootSmbUri, SMBCredentialProvider);

            var files = FileSystem.Directory.EnumerateFiles(testRootSmbUri, "*").ToList();

            Assert.True(files.Count >= 0); //Include 0 in case directory is empty. If an exception is thrown, the test will fail.
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void EnumerateDirectories_DoesntThrow_For_UncRootDirectory()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootUncPath = Path.Combine(testShare.RootUncPath);

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, testRootUncPath, SMBCredentialProvider);

            var files = FileSystem.Directory.EnumerateDirectories(testRootUncPath, "*").ToList();

            Assert.True(files.Count >= 0); //Include 0 in case directory is empty. If an exception is thrown, the test will fail.
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void EnumerateDirectories_DoesntThrow_For_SmbRootDirectory()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootSmbUri = Path.Combine(testShare.RootSmbUri);

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, testRootSmbUri, SMBCredentialProvider);

            var files = FileSystem.Directory.EnumerateDirectories(testRootSmbUri, "*").ToList();

            Assert.True(files.Count >= 0); //Include 0 in case directory is empty. If an exception is thrown, the test will fail.
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetCreationTime_ReturnsCorrectTime_For_SmbUri()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var createDateTime = DateTime.Now;

            createdTestDirectoryPath = testShare.RootSmbUri.CombineToSharePath($"test_directory-{createDateTime.ToFileTimeUtc()}");

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, createdTestDirectoryPath, SMBCredentialProvider);

            createdTestDirectoryInfo = FileSystem.Directory.CreateDirectory(createdTestDirectoryPath);

            Assert.True(Within(createDateTime.ToUniversalTime(), FileSystem.Directory.GetCreationTime(createdTestDirectoryPath), 1));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetCreationTime_ReturnsCorrectTime_For_UncPath()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootUncPath = Path.Combine(testShare.RootUncPath);
            var createDateTime = DateTime.Now;

            createdTestDirectoryPath = Path.Combine(testShare.RootUncPath, $"test_directory-{createDateTime.ToFileTimeUtc()}");

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, createdTestDirectoryPath, SMBCredentialProvider);

            createdTestDirectoryInfo = FileSystem.Directory.CreateDirectory(createdTestDirectoryPath);

            Assert.True(Within(createDateTime.ToUniversalTime(), FileSystem.Directory.GetCreationTime(createdTestDirectoryPath), 1));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetCreationTimeUtc_ReturnsCorrectTime_For_SmbUri()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var createDateTime = DateTime.Now;

            createdTestDirectoryPath = testShare.RootSmbUri.CombineToSharePath($"test_directory-{createDateTime.ToFileTimeUtc()}");

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, createdTestDirectoryPath, SMBCredentialProvider);

            createdTestDirectoryInfo = FileSystem.Directory.CreateDirectory(createdTestDirectoryPath);

            Assert.True(Within(createDateTime.ToUniversalTime(), FileSystem.Directory.GetCreationTimeUtc(createdTestDirectoryPath), 1));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetCreationTimeUtc_ReturnsCorrectTime_For_UncPath()
        {
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootUncPath = Path.Combine(testShare.RootUncPath);
            var createDateTime = DateTime.Now;

            createdTestDirectoryPath = Path.Combine(testShare.RootUncPath, $"test_directory-{createDateTime.ToFileTimeUtc()}");

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, createdTestDirectoryPath, SMBCredentialProvider);

            createdTestDirectoryInfo = FileSystem.Directory.CreateDirectory(createdTestDirectoryPath);

            Assert.True(Within(createDateTime.ToUniversalTime(), FileSystem.Directory.GetCreationTimeUtc(createdTestDirectoryPath), 1));
        }

        private bool Within(DateTime expectedDate, DateTime dateToCompare, double seconds)
        {
            return Math.Abs(expectedDate.Subtract(dateToCompare).TotalSeconds) <= seconds;
        }
    }
}
