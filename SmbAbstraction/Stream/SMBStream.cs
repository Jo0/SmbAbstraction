﻿using System;
using System.Diagnostics;
using System.IO;
using SMBLibrary;
using SMBLibrary.Client;

namespace SmbAbstraction
{
    public class SMBStream : Stream
    {
        private readonly ISmbFileSystemSettings _smbFileSystemSettings;
        private readonly ISMBFileStore _fileStore;
        private readonly object _fileHandle;
        private readonly SMBConnection _connection;

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;
        private long _length { get; set; }
        private long _position { get; set; }
        private int _maxReadSize;
        private int _maxWriteSize;
        private int _maxBufferSize => Math.Min(_maxReadSize, _maxWriteSize);
        public override long Length => _length;
        public override long Position { get { return _position; } set { _position = value; } }


        public SMBStream(ISMBFileStore fileStore, object fileHandle, SMBConnection connection, long fileLength,
                         ISmbFileSystemSettings smbFileSystemSettings = null)
        {
            _smbFileSystemSettings = smbFileSystemSettings ?? new SmbFileSystemSettings();
            _fileStore = fileStore;
            _fileHandle = fileHandle;
            _connection = connection;
            _maxReadSize = Convert.ToInt32(_connection.SMBClient.MaxReadSize);
            _maxWriteSize = Convert.ToInt32(_connection.SMBClient.MaxWriteSize);
            _length = fileLength;
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            NTStatus status;
            var stopwatch = new Stopwatch();
            
            stopwatch.Start();

            do
            {
                status = _fileStore.ReadFile(out byte[] data, _fileHandle, _position, count);

                switch (status)
                {
                    case NTStatus.STATUS_SUCCESS:
                        for (int i = offset, i2 = 0; i2 < data.Length; i++, i2++)
                        {
                            buffer[i] = data[i2];
                        }
                        _position += data.Length;
                        return data.Length;
                    case NTStatus.STATUS_END_OF_FILE:
                        return 0;
                    case NTStatus.STATUS_PENDING:
                        break;
                    default:
                        throw new SMBException($"Unable to read file; Status: {status}");
                }
            }
            while (status == NTStatus.STATUS_PENDING && stopwatch.Elapsed.TotalSeconds <= _smbFileSystemSettings.ClientSessionTimeout);

            stopwatch.Stop();

            throw new SMBException($"Unable to read file; Status: {status}");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    _position = 0;
                    break;
                case SeekOrigin.Current:
                    break;
                case SeekOrigin.End:
                    var status = _fileStore.GetFileInformation(out FileInformation result, _fileHandle, FileInformationClass.FileStreamInformation);

                    status.HandleStatus();

                    FileStreamInformation fileStreamInformation = (FileStreamInformation)result;
                    _position += fileStreamInformation.Entries[0].StreamSize;

                    return _position;
            }
            _position += offset;
            return _position;
        }

        public override void SetLength(long value)
        {
            _length = value;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            byte[] data = new byte[count];

            for (int i = offset, i2 = 0; i < count; i++, i2++)
            {
                data[i2] = buffer[i];
            }

            NTStatus status;
            int bytesWritten = 0;

            var stopwatch = new Stopwatch();

            stopwatch.Start();
            do
            {
                status = _fileStore.WriteFile(out bytesWritten, _fileHandle, _position, data);
            }
            while (status == NTStatus.STATUS_PENDING && stopwatch.Elapsed.TotalSeconds <= _smbFileSystemSettings.ClientSessionTimeout);
            stopwatch.Stop();
            
            status.HandleStatus();

            _position += bytesWritten;
        }

        protected override void Dispose(bool disposing)
        {
            _fileStore.CloseFile(_fileHandle);
            _connection.Dispose();
            base.Dispose(disposing);
        }

        public override void CopyTo(Stream destination, int bufferSize = 0)
        {
            if(bufferSize == 0 || bufferSize > _maxBufferSize)
            {
                bufferSize = _maxBufferSize;
            }

            int count;
            byte[] buffer = new byte[bufferSize];

            while ((count = this.Read(buffer, 0, buffer.Length)) != 0)
            {
                destination.Write(buffer, 0, count);
            }
        }
    }
}
