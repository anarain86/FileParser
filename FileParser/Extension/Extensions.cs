using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileParser.Extension
{
    public static class Extensions
    {
        /// <summary>
        /// Returns the index of the start of the contents in a StringBuilder
        /// </summary>        
        /// <param name="value">The string to find</param>
        /// <param name="startIndex">The starting index.</param>
        /// <param name="ignoreCase">if set to <c>true</c> it will ignore case</param>
        /// <returns></returns>
        public static int IndexOf(this StringBuilder sb, string value, int startIndex = 0, bool ignoreCase = true)
        {
            int index;
            int length = value.Length;
            int maxSearchLength = (sb.Length - length) + 1;

            if (ignoreCase)
            {
                for (int i = startIndex; i < maxSearchLength; ++i)
                {
                    if (char.ToUpper(sb[i]) == char.ToUpper(value[0]))
                    {
                        index = 1;
                        while ((index < length) && (char.ToUpper(sb[i + index]) == char.ToUpper(value[index])))
                            ++index;

                        if (index == length)
                            return i;
                    }
                }

                return -1;
            }

            for (int i = startIndex; i < maxSearchLength; ++i)
            {
                if (sb[i] == value[0])
                {
                    index = 1;
                    while ((index < length) && (sb[i + index] == value[index]))
                        ++index;

                    if (index == length)
                        return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns the index of the end of the contents in a StringBuilder
        /// </summary>        
        /// <param name="value">The string to find</param>
        /// <param name="startIndex">The starting index.</param>
        /// <param name="ignoreCase">if set to <c>true</c> it will ignore case</param>
        /// <returns></returns>
        public static int IndexAfter(this StringBuilder sb, string value, int startIndex = 0, bool ignoreCase = true)
        {
            int index;
            int length = value.Length;
            int maxSearchLength = (sb.Length - length) + 1;

            if (ignoreCase)
            {
                for (int i = startIndex; i < maxSearchLength; ++i)
                {
                    if (char.ToUpper(sb[i]) == char.ToUpper(value[0]))
                    {
                        index = 1;
                        while ((index < length) && (char.ToUpper(sb[i + index]) == char.ToUpper(value[index])))
                            ++index;

                        if (index == length)
                            return i + length;
                    }
                }

                return -1;
            }

            for (int i = startIndex; i < maxSearchLength; ++i)
            {
                if (sb[i] == value[0])
                {
                    index = 1;
                    while ((index < length) && (sb[i + index] == value[index]))
                        ++index;

                    if (index == length)
                        return i + length;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns the content of a Stringbuilder between two strings
        /// </summary>        
        /// <param name="startText">The string use to mark the end of starting point to get the string </param>
        /// <param name="endText">The string use to mark the beginning of the ending point to get the string </param>
        /// <param name="startIndex">The starting index.</param>
        /// <param name="ignoreCase">if set to <c>true</c> it will ignore case</param>
        /// <returns></returns>
        public static string? GetStringInBetweenSection(this StringBuilder sb, string startText, string? endText = null, int startIndex = 0, bool ignoreCase = true)
        {
            int start = sb.IndexAfter(startText, startIndex, ignoreCase);
            if (start == -1) return null;

            if (endText == null) return sb.ToString().Substring(start);

            int end = sb.IndexOf(endText, startIndex, ignoreCase) - 1;
            if (end == -1) return null;

            return sb.ToString()[start..end];
        }

        /// <summary>
        /// Prepend - Add string at the top of the StringBuilder
        /// </summary>
        /// <param name="sb">Stringbuilder object itself</param>
        /// <param name="content">The content that you want to add at the beginning</param>
        /// <returns></returns>
        public static StringBuilder Prepend(this StringBuilder sb, string content)
        {
            return sb.Insert(0, content);
        }

        /// <summary>
        /// PrependLine - Add string at the top of the StringBuilder with the include Environment.Newline
        /// </summary>
        /// <param name="sb">Stringbuilder object itself</param>
        /// <param name="content">The content that you want to add at the beginning</param>
        /// <returns></returns>
        public static StringBuilder PrependLine(this StringBuilder sb, string content)
        {
            return sb.Insert(0, content + Environment.NewLine);
        }
    }
}
