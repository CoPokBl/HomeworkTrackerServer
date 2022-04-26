using Microsoft.AspNetCore.Mvc;

namespace HomeworkTrackerServer.Objects.HeaderParams {
    public class AuthorizationHeaderParams {
        
        [FromHeader]
        private string Authorization { get; set; }

        public string GetUsername() {
            string[] data = Authorization.Split(' ');
            return Converter.Base64Decode(data[1]).Split(':')[0];
        }
        
        public string GetPassword() {
            string[] data = Authorization.Split(' ');
            return Converter.Base64Decode(data[1]).Split(':')[1];
        }
        
        public string GetAuthType() => Authorization.Split(' ')[0];

        public override string ToString() => Authorization;
    }
}