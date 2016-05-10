using System;
using System.Web.Security;

namespace WhiteChamber.TokenizedBasicAuthentication.Lib
{
    public sealed class StaticMembershipProvider : MembershipProvider
    {
        public override bool ValidateUser(string username, string password)
        {
            return username == "mathijs";
        }

        public override string ApplicationName { get; set; }

        public override bool EnablePasswordReset { get { return false; } }

        public override bool EnablePasswordRetrieval { get { return false; } }

        public override int MaxInvalidPasswordAttempts { get { return 0; } }

        public override int MinRequiredNonAlphanumericCharacters { get { return 0; } }

        public override int MinRequiredPasswordLength { get { return 0; } }

        public override int PasswordAttemptWindow { get { return 0; } }

        public override MembershipPasswordFormat PasswordFormat { get { return MembershipPasswordFormat.Clear; } }

        public override string PasswordStrengthRegularExpression { get { return null; } }

        public override bool RequiresQuestionAndAnswer { get { return false; } }

        public override bool RequiresUniqueEmail { get { return false; } }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotSupportedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotSupportedException();
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotSupportedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotSupportedException();
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotSupportedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotSupportedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotSupportedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotSupportedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotSupportedException();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            throw new NotSupportedException();
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotSupportedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotSupportedException();
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotSupportedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotSupportedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotSupportedException();
        }
    }
}
