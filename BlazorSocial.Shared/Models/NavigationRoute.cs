namespace BlazorSocial.Shared.Models;

/// <summary>
/// Client-side navigation route segments used for @attribute [Route(...)] declarations and NavigateTo calls.
/// Usage: @attribute [Route($"/{NavigationRoute.Admin}")]
///        Nav.NavigateTo($"/{NavigationRoute.Admin}")
/// </summary>
public static class NavigationRoute
{
    public const string Home = "/";
    public const string ViewPosts = "ViewPosts";
    public const string PostDetails = "PostDetails";
    public const string PostDetailsTemplate = $"{PostDetails}/{{IdParam}}";
    public const string CreatePost = "CreatePost";
    public const string Admin = "Admin";
    public const string NotFound = "not-found";
    public const string Error = "Error";
    public const string Login = "Login";
    public const string Logout = "Logout";

    public static class Account
    {
        private const string Prefix = "Account";
        public const string Login = $"{Prefix}/Login";
        public const string Register = $"{Prefix}/Register";
        public const string ConfirmEmail = $"{Prefix}/ConfirmEmail";
        public const string ConfirmEmailChange = $"{Prefix}/ConfirmEmailChange";
        public const string ExternalLogin = $"{Prefix}/ExternalLogin";
        public const string ForgotPassword = $"{Prefix}/ForgotPassword";
        public const string ForgotPasswordConfirmation = $"{Prefix}/ForgotPasswordConfirmation";
        public const string InvalidPasswordReset = $"{Prefix}/InvalidPasswordReset";
        public const string InvalidUser = $"{Prefix}/InvalidUser";
        public const string Lockout = $"{Prefix}/Lockout";
        public const string LoginWith2fa = $"{Prefix}/LoginWith2fa";
        public const string LoginWithRecoveryCode = $"{Prefix}/LoginWithRecoveryCode";
        public const string RegisterConfirmation = $"{Prefix}/RegisterConfirmation";
        public const string ResendEmailConfirmation = $"{Prefix}/ResendEmailConfirmation";
        public const string ResetPassword = $"{Prefix}/ResetPassword";
        public const string ResetPasswordConfirmation = $"{Prefix}/ResetPasswordConfirmation";

        public static class Manage
        {
            private const string Prefix = "Account/Manage";
            public const string Index = Prefix;
            public const string ChangePassword = $"{Prefix}/ChangePassword";
            public const string DeletePersonalData = $"{Prefix}/DeletePersonalData";
            public const string Disable2fa = $"{Prefix}/Disable2fa";
            public const string Email = $"{Prefix}/Email";
            public const string EnableAuthenticator = $"{Prefix}/EnableAuthenticator";
            public const string ExternalLogins = $"{Prefix}/ExternalLogins";
            public const string GenerateRecoveryCodes = $"{Prefix}/GenerateRecoveryCodes";
            public const string PersonalData = $"{Prefix}/PersonalData";
            public const string ResetAuthenticator = $"{Prefix}/ResetAuthenticator";
            public const string SetPassword = $"{Prefix}/SetPassword";
            public const string TwoFactorAuthentication = $"{Prefix}/TwoFactorAuthentication";
        }
    }
}
