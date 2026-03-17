
namespace WiseBet.backend.IRepository;

public class KeyNotFoundException : Exception
{
    public KeyNotFoundException(object sender) : base($"[{sender.GetType().Name}] Key was not found"){}
}