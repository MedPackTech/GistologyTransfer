﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GistologyTransfer.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.3.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\\\Nas2\\svs")]
        public string Folder {
            get {
                return ((string)(this["Folder"]));
            }
            set {
                this["Folder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2022-10-01")]
        public global::System.DateTime DateFrom {
            get {
                return ((global::System.DateTime)(this["DateFrom"]));
            }
            set {
                this["DateFrom"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2022-10-08")]
        public global::System.DateTime DateTo {
            get {
                return ((global::System.DateTime)(this["DateTo"]));
            }
            set {
                this["DateTo"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Server=172.18.13.21;Port=5432;Database=dpathology;UserId=rzd;Password=FrvHAWA0ZDu" +
            "IEIim;")]
        public string ConnString {
            get {
                return ((string)(this["ConnString"]));
            }
            set {
                this["ConnString"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\\\172.18.13.251\\d$\\lis-scanner\\storage\\archive")]
        public string ArchivFolder {
            get {
                return ((string)(this["ArchivFolder"]));
            }
            set {
                this["ArchivFolder"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string Icd10 {
            get {
                return ((string)(this["Icd10"]));
            }
            set {
                this["Icd10"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Collections.Specialized.StringCollection Icd10Arr {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["Icd10Arr"]));
            }
            set {
                this["Icd10Arr"] = value;
            }
        }
    }
}
