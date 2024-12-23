using System;

namespace PodNoms.Common.Utils;

public static class HardwareUtils {
  public static int CPUAndCoreCount => Environment.ProcessorCount;
}
