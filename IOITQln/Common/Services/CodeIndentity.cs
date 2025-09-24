using System;

namespace IOITQln.Common.Services
{
    public class CodeIndentity
    {
        public static string CodeInd(string first, int id, int number, string? last="")
        {
            string res = first + id.ToString("D" + number.ToString());
             res += last; 
            return res;
        }
    }
}
