﻿using Patchwork.Attributes;
using System;
using System.IO;

namespace WaitByHoursOrDays
{
    [PatchInfo]
    public class WaitByHoursOrDaysType : IPatchInfo
    {
        public string PatchVersion
        {
            get
            {
                return "1.0.0";
            }
        }

        public string Requirements
        {
            get
            {
                return "None";
            }
        }

        public string PatchName
        {
            get
            {
                return "Wait By Hours Or Days";
            }
        }

        public FileInfo GetTargetFile(AppInfo app)
        {
            if (app == null)
                return null;

            DirectoryInfo baseDir = app.BaseDirectory;
            string fullName = "";

            if (baseDir != null)
                fullName = baseDir.FullName;

            string[] pathArray = new string[]
            {
                fullName,
                "PillarsOfEternity_Data",
                "Managed",
                "Assembly-CSharp.dll"
            };

            string fullPath = String.Join("/", pathArray);
            FileInfo fileInfo = new FileInfo(fullPath);
            return fileInfo;
        }

        public string CanPatch(AppInfo app)
        {
            return null;
        }
    }
}
