namespace HomeworkTrackerServer.Objects {
    
    public class Converter {
        
        public static string Base64Encode(string plainText) =>
            System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plainText));
        
        public static string Base64Decode(string base64EncodedData) => 
            System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(base64EncodedData));

    }
    
}