using Mirror;

namespace Infection
{
    public class AuthRequestMessage : MessageBase
    {
        public string username;
        public string password;
        public string version;
    }

    public class AuthResponseMessage : MessageBase
    {
        public byte code;
        public string message;
    }

    public class ErrorMessage : MessageBase
    {
        public string text;
        public bool causesDisconnect;
    }
}