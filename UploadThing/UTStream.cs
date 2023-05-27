using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace UploadThing.Core
{
    public class UTStream : Stream
    {
        private readonly Stream stream;
        public string Name { get; set; }

        public UTStream(Stream stream, string FileName)
        {
            this.stream = stream;
            this.Name = FileName;
        }

        public override bool CanRead => stream.CanRead;
        public override bool CanSeek => stream.CanSeek;
        public override bool CanWrite => stream.CanWrite;
        public override long Length => stream.Length;
        public override long Position { get => stream.Position; set => stream.Position = value; }

        public override void Flush()
        {
            stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                stream.Dispose();
            }
            base.Dispose(disposing);
        }

        public Task _CopyToAsync(Stream destination)
        {
            if(destination == null || !destination.CanWrite)
            {
                destination.Flush();
                destination.Close();
                return Task.CompletedTask;
            }
            return base.CopyToAsync(destination);
        }
    }
}
