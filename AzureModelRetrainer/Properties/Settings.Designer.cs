﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AzureModelRetrainer.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://asiasoutheast.services.azureml.net/workspaces/e5fedc52e630461cb2ccb69bec9" +
            "3b57a/services/3d73f33953a84a1d9ac60a268dd0faed/jobs")]
        public string mlretrainermodelurl {
            get {
                return ((string)(this["mlretrainermodelurl"]));
            }
            set {
                this["mlretrainermodelurl"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("vN2HV0EbeVxBI22OS4ETBrBQkIsc2KUiRRkxPrTe5Pe4Yix7MMpZYeW9hgGanytx6z6TDDKvtesZ1SlE1" +
            "b3L2w==")]
        public string mlretrainerkey {
            get {
                return ((string)(this["mlretrainerkey"]));
            }
            set {
                this["mlretrainerkey"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("https://asiasoutheast.management.azureml.net/workspaces/e5fedc52e630461cb2ccb69be" +
            "c93b57a/webservices/665f8f9660984667b54093022330dbad/endpoints/retrainendpoint")]
        public string enpointurl {
            get {
                return ((string)(this["enpointurl"]));
            }
            set {
                this["enpointurl"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1/ta/VQdZtmDx5oCIO394mbvhwk4bTTQqBVjr/0eb+LT6Mx7BFerYoLVwtkq6Zy4fwehEZup63RBcnagk" +
            "V2Zcw==")]
        public string endpointkey {
            get {
                return ((string)(this["endpointkey"]));
            }
            set {
                this["endpointkey"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("monacohfstrg")]
        public string mlstoragename {
            get {
                return ((string)(this["mlstoragename"]));
            }
            set {
                this["mlstoragename"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("ycdJtEC3xuxe3mvEq+a7PlinXHKxFLbHc769VMQ7d3FOXD/Z9PaVTlrol7jG4jERPtoVSsE9JC68A2w/P" +
            "It64A==")]
        public string mlstoragekey {
            get {
                return ((string)(this["mlstoragekey"]));
            }
            set {
                this["mlstoragekey"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("mlretraincontainer")]
        public string mlstoragecontainer {
            get {
                return ((string)(this["mlstoragecontainer"]));
            }
            set {
                this["mlstoragecontainer"] = value;
            }
        }
    }
}
