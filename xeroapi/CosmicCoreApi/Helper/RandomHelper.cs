using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace CosmicCoreApi.Helper
{
    public class RandomHelper
    {
        public static string GetRandomString(RandomString randomStringFor)
        {
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            switch (randomStringFor)
            {
                case RandomString.AccountActivationCode:
                    int length = 6;
                    const string validCode = "1234567890";
                    while (0 < length--)
                    {
                        res.Append(validCode[rnd.Next(validCode.Length)]);
                    }
                    break;
                    
                case RandomString.UserPassword:
                    int pasLen = 8;
                    const string validPass = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890#@$";
                    
                    while (0 < pasLen--)
                    {
                        res.Append(validPass[rnd.Next(validPass.Length)]);
                    }
                    break;

                case RandomString.EzzyUserName:
                    int userLen = 6;
                    const string validUsers = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

                    while (0 < userLen--)
                    {
                        res.Append(validPass[rnd.Next(validUsers.Length)]);
                    }
                    break;

                default:
                    break;
            }

            return res.ToString();
          
        }
    }
}