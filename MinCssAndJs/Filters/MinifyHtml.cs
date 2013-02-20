using System;
using System.IO;
using System.IO.Compression;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using BundlingAndMinifyingInlineCssJs.ResponseFilters;
namespace MinCssAndJs.Filters
{
    public class MinifyHtml : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpRequestBase request = filterContext.HttpContext.Request;

            string acceptEncoding = request.Headers["Accept-Encoding"];

            if (!String.IsNullOrEmpty(acceptEncoding))
            {
                acceptEncoding = acceptEncoding.ToUpperInvariant();

                HttpResponseBase response = filterContext.HttpContext.Response;

                if (acceptEncoding.Contains("GZIP"))
                {
                    response.AppendHeader("Content-encoding", "gzip");
                    response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
                }
                else if (acceptEncoding.Contains("DEFLATE"))
                {
                    response.AppendHeader("Content-encoding", "deflate");
                    response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
                }
            }
            filterContext.HttpContext.Response.Filter = new BundleAndMinifyResponseFilter(filterContext.HttpContext.Response.Filter);
            filterContext.HttpContext.Response.Filter = new StringFilterStream(filterContext.HttpContext.Response.Filter);
        }
        private class StringFilterStream : Stream
        {
            private static readonly Regex RegexRemoveWhitespace = new Regex(">[\r\n][ \r\n\t]*<", RegexOptions.Multiline | RegexOptions.Compiled);
            private static readonly Regex RegexRemoveWhitespace2 = new Regex(">[ \r\n\t]+<", RegexOptions.Multiline | RegexOptions.Compiled);
            private Stream _sink;
            private long _position;

            public StringFilterStream(Stream sink)
            {
                _sink = sink;

            }

            public override bool CanRead { get { return true; } }
            public override bool CanSeek { get { return true; } }
            public override bool CanWrite { get { return true; } }
            public override void Flush() { _sink.Flush(); }
            public override long Length { get { return 0; } }

            public override long Position
            {
                get { return _position; }
                set { _position = value; }
            }
            public override int Read(byte[] buffer, int offset, int count)
            {
                return _sink.Read(buffer, offset, count);
            }
            public override long Seek(long offset, SeekOrigin origin)
            {
                return _sink.Seek(offset, origin);
            }
            public override void SetLength(long value)
            {
                _sink.SetLength(value);
            }
            public override void Close()
            {
                _sink.Close();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                var data = new byte[count];
                Buffer.BlockCopy(buffer, offset, data, 0, count);
                string s = Encoding.Default.GetString(buffer);
                s = FilterString(s);
                var outdata = Encoding.Default.GetBytes(s);
                _sink.Write(outdata, 0, outdata.GetLength(0));
            }

            private string FilterString(string html)
            {
                html = RegexRemoveWhitespace.Replace(html, "><");
                html = RegexRemoveWhitespace2.Replace(html, "> <");
                return html;
            }

        }
    }
}