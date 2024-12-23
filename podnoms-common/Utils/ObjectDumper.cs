using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace PodNoms.Common.Utils;

public class ObjectDumper {
  private readonly StringBuilder _dumpBuilder = new();

  public static string Dump(object obj) {
    return new ObjectDumper().DumpObject(obj);
  }

  private string DumpObject(object obj) {
    DumpObject(obj, 0);
    return _dumpBuilder.ToString();
  }

  private void DumpObject(object obj, int nestingLevel) {
    var nestingSpaces = "".PadLeft(nestingLevel * 4);

    if (obj == null) {
      _dumpBuilder.AppendFormat("{0}null\n", nestingSpaces);
    } else if (obj is string || obj.GetType().GetTypeInfo().IsPrimitive || obj.GetType().GetTypeInfo().IsEnum) {
      _dumpBuilder.AppendFormat("{0}{1}\n", nestingSpaces, obj);
    } else if (ImplementsDictionary(obj.GetType())) {
      using (var e = ((dynamic)obj).GetEnumerator()) {
        var enumerator = (IEnumerator)e;
        while (enumerator.MoveNext()) {
          dynamic p = enumerator.Current;

          var key = p.Key;
          var value = p.Value;
          _dumpBuilder.AppendFormat("{0}{1} ({2})\n", nestingSpaces, key,
            value != null ? value.GetType().ToString() : "<null>");
          DumpObject(value, nestingLevel + 1);
        }
      }
    } else if (obj is IEnumerable) {
      foreach (dynamic p in obj as IEnumerable) {
        DumpObject(p, nestingLevel);
      }
    } else {
      foreach (var descriptor in obj.GetType().GetRuntimeProperties()) {
        var name = descriptor.Name;
        var value = descriptor.GetValue(obj);

        _dumpBuilder.AppendFormat("{0}{1} ({2})\n", nestingSpaces, name,
          value != null ? value.GetType().ToString() : "<null>");

        // TODO: Prevent recursion due to circular reference
        if (name == "Self" && HasBaseType(obj.GetType(), "NSObject")) {
          // In ObjC I need to break the recursion when I find the Self property
          // otherwise it will be an infinite recursion
          Console.WriteLine($"Found Self! {obj.GetType()}");
        } else {
          DumpObject(value, nestingLevel + 1);
        }
      }
    }
  }

  private bool HasBaseType(Type type, string baseTypeName) {
    if (type == null) {
      return false;
    }

    var typeName = type.Name;

    if (baseTypeName == typeName) {
      return true;
    }

    return HasBaseType(type.GetTypeInfo().BaseType, baseTypeName);
  }

  private bool ImplementsDictionary(Type t) {
    return t is IDictionary;
  }
}
