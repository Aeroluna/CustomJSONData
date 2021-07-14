namespace CustomJSONData
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using IPA.Logging;

    public static class DictionaryExtensions
    {
        public static IDictionary<string, object> Copy(this IDictionary<string, object> dictionary)
        {
            return dictionary != null ? new Dictionary<string, object>(dictionary) : new Dictionary<string, object>();
        }

        public static T Get<T>(this IDictionary<string, object> dictionary, string key)
        {
            if (dictionary.TryGetValue(key, out object value))
            {
                Type underlyingType = Nullable.GetUnderlyingType(typeof(T));
                if (underlyingType != null)
                {
                    return (T)Convert.ChangeType(value, underlyingType);
                }
                else
                {
                    if (value is IConvertible)
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }
                    else
                    {
                        return (T)value;
                    }
                }
            }

            return default;
        }
        public static readonly Regex dotRegex = new Regex(
            @"\.(?# literal .
            )((?# capture the identifier
                )[$_\p{L}](?# $, _, or any Unicode letter
                )[$_\p{L}\p{Mn}\p{Mc}\p{Nd}\p{Pc}\u200C\u200D]*(?# any character allowed after the first character of an identifier, any number of times
            ))", RegexOptions.Compiled);
        public static readonly Regex bracketRegex = new Regex(
            @"\[(?# literal [
                )(?:(?# first option: a quoted string
                    )(?<quote>['""])(?# the starting quote for the string
                        )(?<id>(?# capture the string's contents as id
                            )(?:(?# either...
                                )(?:(?# ... a normal character ...
                                    )(?!\k<quote>)(?# negative lookahead; asserts the next character isn't the closing quote
                                    )[^\\](?# any character but a backslash
                                ))|(?:(?# ... or an escaped quote ...
                                    )\\(?# literal \
                                    )\k<quote>(?# the same type of quote that started the string
                                ))|(?:(?# ... or an escaped backslash ...
                                    )\\\\(?# two literal \s
                                ))(?#
                            ))*(?# ... any number of times
                        ))(?# done capturing id
                    )\k<quote>(?# closing quote
                ))|(?# second option: any number of digits
                    )(?<id>(?# capture the digits as id
                        )\p{Nd}(?# Unicode decimal digit)+(?# one or more times
                    ))(?# done capturing id
            )\](?# literal ])", RegexOptions.Compiled);
        /// <summary>
        /// Retrieves a value using a sequence of JS accesses of the following forms:
        /// - ".identifier", where identifier is a valid JS identifier (not excluding reserved words)
        /// - "['key']" / "[\"key\"]", where key is any sequence of characters (with quotes matching the surrounding quotes backslash-escaped)
        /// - "[index]", where index is a sequence of digits
        /// For example, ".foo['bar \\' baz'].qux[\"quux\"][3]".
        /// Descends through both objects (assumed to be <see cref="IDictionary{string, object}"/>s) and arrays (assumed to be <see cref="List{object}"/>s).
        /// For arrays, numeric indices and string indices are both parsed with <see cref="int.TryParse(string, out int)"/>.
        /// Returns default(<typeparamref name="T"/>) if this parse fails while indexing an array, if a nonfinal path step is neither an object nor an array,
        /// or if the value at the path can't be converted to <typeparamref name="T"/>.
        /// An empty <paramref name="path"/> resolves to <paramref name="dictionary"/> itself.
        /// If <paramref name="path"/> is nonempty and starts with a character other than '.' or '[', a '.' is added.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve.</typeparam>
        /// <param name="dictionary">The object to retrieve a value from.</param>
        /// <param name="path">The path to follow within <paramref name="dictionary"/></param>
        /// <returns>The value of type <typeparamref name="T"/> at <paramref name="path"/> within <paramref name="dictionary"/>, if present.</returns>
        public static T getPath<T>(this IDictionary<string, object> dictionary, string path)
        {
            if (path == "")
            {
                if (dictionary is IConvertible)
                {
                    return (T)Convert.ChangeType(dictionary, typeof(T));
                }
                else
                {
                    return (T)dictionary;
                }
            }
            // allow omitting starting .
            if (!path.StartsWith(".") && !path.StartsWith("[")) path = $".{path}";
            // replace all . accesses with [''] accesses
            path = dotRegex.Replace(path, match => $"['{match.Groups[0]}']");
            // extract keys from [''] / [""] / [] (numeric) accesses
            var segments = bracketRegex.Matches(path)
                .Cast<Match>()
                .Select(m => m.Groups["quote"].Success
                    ? m.Groups["id"].Value
                        .Replace($"\\{m.Groups["quote"].Value}", m.Groups["quote"].Value) // if quotes were used, unescape escaped quotes
                        .Replace(@"\\", @"\") // and escaped backslashes
                    : m.Groups["id"].Value) // otherwise, nothing to unescape
                .ToList();

            // validate path
            var badSegments = bracketRegex.Replace(path, m => "\uE000") // replace all valid bracket accesses with a Unicode private use character
                .Split('\uE000') // split on that private use character
                .Where(seg => seg != "") // remove all empty segments, leaving only non-empty segments that weren't valid bracket accesses
                .ToList();
            if (badSegments.Count > 0)
            {
                Logger.Log($"Warning: getPath path \"{path}\" contains bad segments:", level: IPA.Logging.Logger.Level.Warning);
                foreach (var seg in badSegments)
                {
                    Logger.Log($"    {seg}", level: IPA.Logging.Logger.Level.Warning);
                }
                Logger.Log("Pretending they don't exist, and probably accessing a bad path.", level: IPA.Logging.Logger.Level.Warning);
            }

            // paths can pass through both objects and arrays
            List<object> array = null;
            // for each segment before the last, descend into the object/array at that path
            for (int i = 0; i < (segments.Count - 1); i++)
            {
                if (dictionary != null)
                {
                    array = dictionary.Get<List<object>>(segments[i]);
                    dictionary = dictionary.Get<IDictionary<string, object>>(segments[i]);
                }
                else if (array != null && int.TryParse(segments[i], out int index))
                {
                    dictionary = array[index] as IDictionary<string, object>;
                    array = array[index] as List<object>;
                }
                else return default;
            }
            // we're at the last segment; 
            if (dictionary != null)
            {
                return dictionary.Get<T>(segments[segments.Count - 1]);
            }
            else if (array != null && int.TryParse(segments[segments.Count - 1], out int index_))
            {
                object value = array[index_];
                Type underlyingType = Nullable.GetUnderlyingType(typeof(T));
                if (underlyingType != null)
                {
                    return (T)Convert.ChangeType(value, underlyingType);
                }
                else
                {
                    if (value is IConvertible)
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }
                    else
                    {
                        return (T)value;
                    }
                }
            }
            else return default;
        }
        /// <summary>
        /// The UpperCamelCase form of <see cref="getPath{T}(IDictionary{string, object}, string)"/>
        /// </summary>
        /// <seealso cref="getPath{T}(IDictionary{string, object}, string)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetPath<T>(this IDictionary<string, object> dictionary, string path) => dictionary.getPath<T>(path);
    }
}
