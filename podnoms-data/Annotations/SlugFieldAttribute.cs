using System;

namespace PodNoms.Data.Annotations;

[AttributeUsage(AttributeTargets.Property)]
public class SlugFieldAttribute : Attribute {
  public SlugFieldAttribute(string sourceField) {
    SourceField = sourceField;
  }

  public string SourceField { get; }
}
