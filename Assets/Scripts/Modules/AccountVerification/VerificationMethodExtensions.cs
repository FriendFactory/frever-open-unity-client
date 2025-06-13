using System;
using System.Text;
using System.Text.RegularExpressions;
using Bridge.AccountVerification.Models;

namespace Modules.AccountVerification
{
    public static class VerificationMethodExtensions
    {
        private static readonly Regex EMAIL_REGEX = new(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        private static readonly Regex PHONE_NUMBER_REGEX = new(@"^\+\d{11,15}$");
        
        public static string GetCredentialsMasked(this IVerificationMethod verificationMethod)
        {
            switch (verificationMethod.Type)
            {
                case CredentialType.PhoneNumber:
                    return FormatPhoneNumber(verificationMethod.Input);
                case CredentialType.Email:
                    return FormatEmail(verificationMethod.Input);
                case CredentialType.Password:
                case CredentialType.AppleId:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            string FormatPhoneNumber(string phoneNumber)
            {
                if (phoneNumber.Length < 8) return phoneNumber;

                var builder = new StringBuilder();

                builder.Append(phoneNumber[..4]);
                builder.Append(new string('*', phoneNumber.Length - 8));
                builder.Append(phoneNumber.Substring(phoneNumber.Length - 4, 4));

                return builder.ToString();
            }

            string FormatEmail(string email)
            {
                var atIndex = email.IndexOf('@');

                if (atIndex <= 1) return email;

                var builder = new StringBuilder();

                builder.Append(email[0]);
                builder.Append("***");
                builder.Append(email[atIndex - 1]);
                builder.Append(email[atIndex..]);

                return builder.ToString();
            }
        }

        public static bool IsInputValid(this IVerificationMethod verificationMethod)
        {
            var input = verificationMethod.Input;
            
            switch (verificationMethod.Type)
            {
                case CredentialType.Email:
                    return IsEmailValid();
                case CredentialType.PhoneNumber:
                    return IsPhoneNumberValid();
                case CredentialType.Password:
                    return IsPasswordValid();
                case CredentialType.AppleId:
                default:
                    throw new ArgumentOutOfRangeException();
            }


            bool IsEmailValid() => !string.IsNullOrWhiteSpace(input) && EMAIL_REGEX.IsMatch(input);
            bool IsPhoneNumberValid() => !string.IsNullOrEmpty(input) && PHONE_NUMBER_REGEX.IsMatch(input);
            bool IsPasswordValid() => !string.IsNullOrEmpty(input) && input.Length >= 8;
        }
    }
}