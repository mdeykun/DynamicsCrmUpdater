using System;

namespace Cwru.Common.Config
{
    public static class Consts
    {
        public const string FileKindGuid = "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}";
        public const string ProjectKindGuid = "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}";

        public const string PackageGuidString = "944f3eda-3d74-49f0-a2d4-a25775f1ab36";
        public const string OutputWindowGuidString = "10B2DB3C-1CB4-43B4-80D4-A03204A616D4";
        public const string ProjectCommandSetGuidString = "e51702bf-0cd0-413b-87ba-7d267eecc6c2";
        public const string ItemCommandSetGuidString = "AE7DC0B9-634A-46DB-A008-D6D15DD325E0";
        //public const string FolderCommandSetGuidString = "18CFE3ED-8E6B-4BD0-BFE7-9AFF7BF02009";

        public static readonly Guid Package = new Guid(PackageGuidString);
        public static readonly Guid OutputWindow = new Guid(OutputWindowGuidString);
        public static readonly Guid ProjectCommandSet = new Guid(ProjectCommandSetGuidString);
        public static readonly Guid ItemCommandSet = new Guid(ItemCommandSetGuidString);
        //public static readonly Guid FolderCommandSet = new Guid(FolderCommandSetGuidString);
    }
}
