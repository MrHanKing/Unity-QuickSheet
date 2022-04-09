
using System;
using System.IO;
using System.Text;

namespace UnityQuickSheet
{
    public class PathHelper
    {
        /// <summary>
        /// 获取wantRelativeTo 相对于absolutePath的相对路径
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <param name="wantRelativeTo"></param>
        /// <returns></returns>
        public static string RelativePath(string absolutePath, string wantRelativeTo)
        {
            string[] absoluteDirectories = absolutePath.Split(Path.DirectorySeparatorChar);
            string[] relativeDirectories = wantRelativeTo.Split(Path.DirectorySeparatorChar);

            //Get the shortest of the two paths
            int length = absoluteDirectories.Length < relativeDirectories.Length ? absoluteDirectories.Length : relativeDirectories.Length;

            //Use to determine where in the loop we exited
            int lastCommonRoot = -1;
            int index;

            //Find common root
            for (index = 0; index < length; index++)
                if (absoluteDirectories[index] == relativeDirectories[index])
                    lastCommonRoot = index;
                else
                    break;

            //If we didn't find a common prefix then throw
            if (lastCommonRoot == -1)
                throw new ArgumentException("Paths do not have a common base");

            //Build up the relative path
            StringBuilder relativePath = new StringBuilder();

            //Add on the ..
            for (index = lastCommonRoot + 1; index < absoluteDirectories.Length; index++)
                if (absoluteDirectories[index].Length > 0)
                    relativePath.Append($"..{Path.DirectorySeparatorChar}");

            //Add on the folders
            for (index = lastCommonRoot + 1; index < relativeDirectories.Length - 1; index++)
                relativePath.Append(relativeDirectories[index] + Path.DirectorySeparatorChar);
            relativePath.Append(relativeDirectories[relativeDirectories.Length - 1]);

            return relativePath.ToString();
        }
    }
}


