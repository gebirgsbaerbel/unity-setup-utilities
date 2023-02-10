using System.Net.Http;
using System.Threading.Tasks;

namespace gebirgsbaerbel.Utilities
{
    public static class FileHelper
    {
        public static async Task<string> GetContents(string url)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }
        
    }
    
    
}
