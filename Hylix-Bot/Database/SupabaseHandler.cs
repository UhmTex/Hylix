using System.Threading.Tasks;
using Supabase;

namespace Hylix_Bot
{
    internal class SupabaseHandler
    {
        public static Supabase.Client supabase;

        public static async Task SupabaseInit()
        {
            string url = "https://xhtgynnynxkxrkecteps.supabase.co";
            string key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InhodGd5bm55bnhreHJrZWN0ZXBzIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MjU3MDk5MjUsImV4cCI6MjA0MTI4NTkyNX0.EQ0uauuXxw66uOj-6mhsTZ288I9tx7wgWYvZO1xkEB8";

            SupabaseOptions options = new Supabase.SupabaseOptions
            {
                AutoConnectRealtime = true,                
            };

            supabase = new Supabase.Client(url, key, options);
            await supabase.InitializeAsync();
        }
    }
}