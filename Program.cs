using System.Threading.Tasks;

namespace Tewirai
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await Tewirai.Instance.RunAsync();
        }
    }
}
