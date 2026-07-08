namespace EducationCenter.Crm.Application.Auth;

public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base("Tài khoản hoặc mật khẩu không chính xác.")
    {
    }
}
