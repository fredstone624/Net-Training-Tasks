using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace AsyncIO
{
    public static class Tasks
    {
        /// <summary>
        /// Returns the content of required uris.
        /// Method has to use the synchronous way and can be used to compare the performace of sync \ async approaches. 
        /// </summary>
        /// <param name="uris">Sequence of required uri</param>
        /// <returns>The sequence of downloaded url content</returns>
        public static IEnumerable<string> GetUrlContent(this IEnumerable<Uri> uris) 
        {
            // TODO : Implement GetUrlContent
            using (var client = new WebClient())
            {
                return uris.Select(x => client.DownloadString(x));
            }
        }


        /// <summary>
        /// Returns the content of required uris.
        /// Method has to use the asynchronous way and can be used to compare the performace of sync \ async approaches. 
        /// 
        /// maxConcurrentStreams parameter should control the maximum of concurrent streams that are running at the same time (throttling). 
        /// </summary>
        /// <param name="uris">Sequence of required uri</param>
        /// <param name="maxConcurrentStreams">Max count of concurrent request streams</param>
        /// <returns>The sequence of downloaded url content</returns>
        public static IEnumerable<string> GetUrlContentAsync(this IEnumerable<Uri> uris, int maxConcurrentStreams)
        {
            // TODO : Implement GetUrlContentAsync
            using (var client = new WebClient())
            {
                var enumer = uris.GetEnumerator();
                var tasks = new List<Task<string>>(uris.Take(maxConcurrentStreams).Select(x => client.DownloadStringTaskAsync(x)));

                while (tasks.Count > 0)
                {
                    yield return tasks.First().Result;

                    tasks.RemoveAt(0);

                    if (enumer.MoveNext())
                    {
                        tasks.Add(client.DownloadStringTaskAsync(enumer.Current));
                    }
                }
            }
        }


        /// <summary>
        /// Calculates MD5 hash of required resource.
        /// 
        /// Method has to run asynchronous. 
        /// Resource can be any of type: http page, ftp file or local file.
        /// </summary>
        /// <param name="resource">Uri of resource</param>
        /// <returns>MD5 hash</returns>
        public static async Task<string> GetMD5Async(this Uri resource)
        {
            // TODO : Implement GetMD5Async
            

            // Read data in parts
            using (var client = new WebClient())
            using (var stream = await client.OpenReadTaskAsync(resource))
            {
                var md5 = MD5.Create();
                var buffer = new byte[2048];
                var bytes = 0;

                while ((bytes = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    md5.TransformBlock(buffer, 0, bytes, buffer, 0);
                }

                md5.TransformFinalBlock(new byte[0], 0, 0);

                return string.Concat(md5.Hash.Select(x => x.ToString("X2")));
            }


            // Read full data
            //using (var client = new WebClient())
            //{
            //    var data = await client.DownloadDataTaskAsync(resource);

            //    return string.Concat(MD5.Create().ComputeHash(data).Select(x => x.ToString("X2")));
            //}
        }
    }
}         