
namespace WiseBet.backend.IRepository;

public class KeyNotFoundException : Exception
{
    public KeyNotFoundException(object sender) : base($"[{sender.GetType().Name}] Key was not found"){}
}

public class InvalidParameterException : Exception
{
    public InvalidParameterException(object sender) : base($"[{sender.GetType().Name}] Invalid parameter was passed"){}
}