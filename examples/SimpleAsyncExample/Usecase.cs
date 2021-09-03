using System.Threading.Tasks;

namespace SimpleAsyncExample
{
    static class Usecase
    {
        public static Task ExecuteAsync()
        {
            return Task.CompletedTask;
        }
    }
}