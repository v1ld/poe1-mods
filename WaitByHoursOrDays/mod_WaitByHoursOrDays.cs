using Patchwork.Attributes;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace V1ldWaitByHoursOrDays
{
    [ModifiesType("CommandLine")]
    public static class V1ld_CommandLineWaitByHoursOrDays
    {
        [NewMember]
        public static void WaitHours(string hours)
        {
            Console.AddMessage("Waited from " + V1ldTimeDateString());
            WorldTime.Instance.CurrentTime.AddHours(int.Parse(hours));
            Console.AddMessage("Waited till " + V1ldTimeDateString());
        }

        [NewMember]
        public static void WaitDays(string days)
        {
            Console.AddMessage("Waited from " + V1ldTimeDateString());
            WorldTime.Instance.CurrentTime.AddDays(int.Parse(days));
            Console.AddMessage("Waited till " + V1ldTimeDateString());
        }

        [NewMember]
        private static string V1ldTimeDateString()
        {
            return WorldTime.Instance.CurrentTime.Format("{2:D2}:{1:D2} on {6} {3}, {4} AI (Day {7})");
        }

    }
}