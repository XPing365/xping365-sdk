using System.Net;

namespace XPing365.Core.Source
{
    /// <summary>
    /// This interface represents data retrieved from the web.
    /// </summary>
    public interface IDataSource
    {
        /// <summary>
        /// Gets URL address of the retrieved content.
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Gets HTTP response status code.
        /// </summary>
        HttpStatusCode ResponseCode { get; }

        /// <summary>
        /// Gets a value that indicates if the response status code was successfull.
        /// </summary>
        bool IsSuccessResponseCode { get; }

        /// <summary>
        /// Gets a value that indicates the response size in bytes.
        /// </summary>
        long ResponseSizeInBytes { get; }

        /// <summary>
        /// Gets value indicating when request to retrieve data started.
        /// </summary>
        DateTime RequestStartTime { get; }

        /// <summary>
        /// Gets value indicating when request to retrieve data ended.
        /// </summary>
        DateTime RequestEndTime { get; }
    }
}
