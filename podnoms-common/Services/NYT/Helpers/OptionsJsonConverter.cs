﻿// Copyright 2017 Brian Allred
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PodNoms.Common.Services.NYT.Helpers
{
    #region Using

    #endregion

    /// <inheritdoc />
    /// <summary>
    ///     Custom Json converter for the Options class
    /// </summary>
    public class OptionsJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Options.Options);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var options = new Options.Options();

            foreach (var jPair in jo)
            {
                var propertyInfo = options.GetType().GetRuntimeProperty(jPair.Key);
                var property = propertyInfo.GetValue(options);

                foreach (var token in jPair.Value)
                {
                    var childObject = new JObject(token);
                    foreach (var childPair in childObject)
                    {
                        // Why on earth does GetRuntimeField(childPair.Key) return null here, but THIS works!?
                        var fieldInfo =
                            propertyInfo.PropertyType.GetRuntimeFields().
                                First(field => field.Name.Equals(childPair.Key));

                        dynamic fieldValue = fieldInfo.GetValue(property);

                        switch (fieldValue)
                        {
                            case BoolOption boolField:
                                boolField.Value = (bool) childPair.Value;
                                fieldInfo.SetValue(property, boolField);
                                break;
                            case DateTimeOption datetimeField:
                                datetimeField.Value = DateTime.Parse((string)childPair.Value);
                                fieldInfo.SetValue(property, datetimeField);
                                break;
                            case DoubleOption doubleField:
                                doubleField.Value = (double) childPair.Value;
                                fieldInfo.SetValue(property, doubleField);
                                break;
                            case FileSizeRateOption fileSizeRateField:
                                fileSizeRateField.Value = new FileSizeRate((string)childPair.Value);
                                fieldInfo.SetValue(property, fileSizeRateField);
                                break;
                            case IntOption intField:
                                intField.Value = (int)childPair.Value;
                                fieldInfo.SetValue(property, intField);
                                break;
                            case StringOption stringField:
                                stringField.Value = (string)childPair.Value;
                                fieldInfo.SetValue(property, stringField);
                                break;
                            default:
                                // If it's not one of the above classes, then it's an EnumOption,
                                // which, at the end of the day, is just an int.
                                fieldValue.Value = (int)childPair.Value;
                                fieldInfo.SetValue(property, fieldValue);
                                break;
                        }
                    }
                }
            }

            return options;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var jo = new JObject();
            var type = value.GetType();

            // value is an Options object. Get it's properties
            foreach (var propertyInfo in type.GetRuntimeProperties())
            {
                var propVal = propertyInfo.GetValue(value);
                if (propVal != null)
                {
                    var childObject = new JObject();

                    // Get the fields of the options object
                    foreach (var fieldInfo in propVal.GetType().GetRuntimeFields())
                    {
                        try
                        {
                            dynamic fieldVal = fieldInfo.GetValue(propVal);
                            if (fieldVal.Value != null)
                            {
                                var fileSizeRate = fieldVal.Value as FileSizeRate;
                                object val = fileSizeRate != null ? fileSizeRate.ToString() : fieldVal.Value;

                                childObject.Add(new JProperty(fieldInfo.Name, val));
                            }
                        }
                        catch (RuntimeBinderException)
                        {
                            // Only BaseOption and child classes will have .Value.
                            // Don't care about the rest
                        }
                    }

                    if (childObject.HasValues)
                    {
                        jo.Add(propertyInfo.Name, childObject);
                    }
                }
            }

            jo.WriteTo(writer);
        }
    }
}