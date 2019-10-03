using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.API2.Utils
{
    public class PasswordGenerator
    {
        public PasswordGenerator()
        {

        }
        public string GeneratePassword(bool isAlphabet, bool isNumber, bool isSpecialCharacter, int length)
        {
            if (length > 0)
            {
                const string alphabets = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                const string number = "1234567890";
                const string specialChanracters = "!@#$%^&*()";
                StringBuilder passwordString = new StringBuilder(String.Empty);
                StringBuilder generatedPassword = new StringBuilder(String.Empty);
                try
                {
                    if (isAlphabet)
                    {
                        passwordString.Append(alphabets);
                    }
                    if (isNumber)
                    {
                        passwordString.Append(number);
                    }
                    if (isSpecialCharacter)
                    {
                        passwordString.Append(specialChanracters);
                    }

                    Random rnd = new Random();
                    int passwordLength = passwordString.Length - 1;
                    for (int i = 0; i < length; i++)
                    {
                        int randomNumber = rnd.Next(0, passwordLength);
                        generatedPassword.Append(passwordString[randomNumber]);
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
                return generatedPassword.ToString();
            }
            return null;
        }

        public string GenerateUserName(string userName)
        {
            try
            {
                Random randomNumber = new Random();
                userName = userName.Split(' ').FirstOrDefault();
                userName = userName + randomNumber.Next(100000, 999999);
                return userName.ToLower();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
