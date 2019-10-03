using Kitsune.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kitsune.Compiler.Core.Helpers
{
    class ksegregation
    {
        public static List<MultiplePositionProperty> MergeProperty(List<MultiplePositionProperty> properties)
        {
            Regex fullRangeRegex = new Regex(@"\[:\]");
            Regex SingleRangeRegex = new Regex(@"\[(\d*?)\]");

            List<MultiplePositionProperty> propertiesCopy = new List<MultiplePositionProperty>();
            foreach (var property in properties)
            {
                property.Property = fullRangeRegex.Replace(property.Property, "[-1:-1]");
                property.Property = SingleRangeRegex.Replace(property.Property, "[$1:$1]");
            }
            propertiesCopy = new List<MultiplePositionProperty>(properties);
            foreach (var property in properties)
            {

                List<range> mergeRanges = new List<range>();
                var startbracketIndex = property.Property.IndexOf('[');
                var endBracketIndex = property.Property.IndexOf(']');
                var startPropertyString = property.Property.Substring(0, startbracketIndex + 1);
                var endPropertyString = property.Property.Substring(endBracketIndex);
                var startPropertyRegex = Regex.Escape(startPropertyString);
                var endPropertyRegex = Regex.Escape(endPropertyString);
                var propertyPattern = @"^" + startPropertyRegex + @"(\d+):(\d+)" + endPropertyRegex + @"$";
                var propertyPatternString = startPropertyString + "{0}:{1}" + endPropertyString;
                var propertyRegex = new Regex(propertyPattern);
                var propertyMatches = propertiesCopy.Where(x => propertyRegex.IsMatch(x.Property)).ToList();
                var wholeRangeFlag = false;
                List<multiPropertyRange> mergedRanges = new List<multiPropertyRange>();
                if (propertyMatches.Any() && propertyMatches.Count > 1)
                {
                    foreach (var matches in propertyMatches)
                    {
                        var startRange = int.Parse(Regex.Match(matches.Property, propertyPattern).Groups[1].Value);
                        var endRange = int.Parse(Regex.Match(matches.Property, propertyPattern).Groups[2].Value);
                        mergeRanges.Add(new range { start = startRange, end = endRange, property = matches });
                        if (startRange == -1)
                        {
                            wholeRangeFlag = true;
                            break;
                        }
                    }
                    if (wholeRangeFlag == true)
                        mergedRanges = new List<multiPropertyRange> { new multiPropertyRange { start = -1, end = -1, properties = propertyMatches } };
                    else
                        mergedRanges = mergeIntervals(mergeRanges, mergeRanges.Count);
                    foreach (var range in mergedRanges)
                    {
                        var propertyOccurence = new List<position>();
                        foreach (var prop in range.properties)
                        {
                            propertyOccurence.AddRange(prop.Positions);
                            propertiesCopy.Remove(prop);
                        }
                        propertiesCopy.Add(new MultiplePositionProperty
                        {
                            Property = string.Format(propertyPatternString, range.start, range.end),
                            Positions = propertyOccurence
                        });
                    }


                }
            }
            return propertiesCopy;
        }

        public static List<multiPropertyRange> mergeIntervals(List<range> arr, int n)
        {
            arr = arr.OrderBy(x => x.start).ToList();

            var result = new List<multiPropertyRange> { new multiPropertyRange { start = arr[0].start, end = arr[0].end, properties = new List<MultiplePositionProperty> { arr[0].property } } };

            foreach (var item in arr.GetRange(1, arr.Count - 1))
            {
                var lastResult = result[result.Count - 1];
                int last0 = lastResult.start;
                int last1 = lastResult.end;
                if (item.start <= last1 || item.start == last1 + 1)
                {
                    result[result.Count - 1].end = Math.Max(item.end, last1);
                    result[result.Count - 1].properties.Add(item.property);
                }
                else
                {
                    result.Add(new multiPropertyRange { start = item.start, end = item.end, properties = new List<MultiplePositionProperty> { item.property } });
                }

            }
            return result;
        }
    }
}
