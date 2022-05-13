using Microsoft.AspNetCore.Mvc;

namespace HomeworkTrackerServer.Objects.HeaderParams {
    
    /// <summary>
    /// An object that gets the Basic Authorization header from a request
    /// </summary>
    public class AuthorizationHeaderParams {
        
        [FromHeader]
        public string Authorization { get; set; }  // DO NOT SET TO PRIVATE IT BREAKS THE [FromHeader] ATTRIBUTE

        /// <summary>
        /// Get the specified username from the header
        /// </summary>
        /// <returns>The username as specified in the header</returns>
        public string GetUsername() {
            string[] data = Authorization.Split(' ');
            return Converter.Base64Decode(data[1]).Split(':')[0];
        }
        
        /// <summary>
        /// Get the specified password from the header
        /// </summary>
        /// <returns>The password as specified in the header</returns>
        public string GetPassword() {
            string[] data = Authorization.Split(' ');
            return Converter.Base64Decode(data[1]).Split(':')[1];
        }
        
        /// <summary>
        /// Get the specified header type from the header
        /// </summary>
        /// <returns></returns>
        public string GetAuthType() => Authorization.Split(' ')[0];

        /// <summary>
        /// Converts object to string
        /// </summary>
        /// <returns>The raw header data</returns>
        public override string ToString() => Authorization;
    }
}