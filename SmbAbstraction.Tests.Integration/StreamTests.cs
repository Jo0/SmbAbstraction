using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SmbAbstraction.Tests.Integration
{
    public class StreamTests : TestBase
    {
        public StreamTests() : base()
        {
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void CheckStreamLength()
        {
            var tempFileName = $"temp-CheckStreamLength-{DateTime.Now.ToFileTimeUtc()}.txt";
            var testCredentials = TestSettings.ShareCredentials;
            var testShare = TestSettings.Shares.First();
            var testRootUncPath = testShare.RootUncPath;
            var uncDirectory = FileSystem.Path.Combine(testRootUncPath, testShare.Directories.First());
            var tempFilePath = FileSystem.Path.Combine(LocalTempDirectory, tempFileName);

            var byteArray = new byte[100];

            using var credential = new SMBCredential(testCredentials.Domain, testCredentials.Username, testCredentials.Password, uncDirectory, SMBCredentialProvider);

            if(!FileSystem.File.Exists(tempFilePath))
            {
                using(var stream = FileSystem.File.Create(tempFilePath))
                {
                    stream.Write(byteArray, 0, 100);
                }
            }

            var fileInfo = FileSystem.FileInfo.FromFileName(tempFilePath);
            var fileSize = fileInfo.Length;

            var destinationFilePath = FileSystem.Path.Combine(uncDirectory, tempFileName);
            var uncFileInfo = fileInfo.CopyTo(destinationFilePath);
            Assert.True(uncFileInfo.Exists);
            
            using (var stream = uncFileInfo.OpenRead())
            {
                //Assert.Equal(uncFileInfo.Length, fileSize);
                Assert.Equal(stream.Length, fileSize);
            }
                

            FileSystem.File.Delete(uncFileInfo.FullName);
        }
    }
}
