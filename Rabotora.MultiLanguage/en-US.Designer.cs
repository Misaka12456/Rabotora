﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Rabotora.MultiLanguage {
    using System;
    
    
    /// <summary>
    ///   一个强类型的资源类，用于查找本地化的字符串等。
    /// </summary>
    // 此类是由 StronglyTypedResourceBuilder
    // 类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    // 若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (以 /str 作为命令选项)，或重新生成 VS 项目。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class en_US {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal en_US() {
        }
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Rabotora.MultiLanguage.en-US", typeof(en_US).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   重写当前线程的 CurrentUICulture 属性，对
        ///   使用此强类型资源类的所有资源查找执行重写。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   查找类似 Rabotora Visual Novel Game Engine SDK
        ///(C)Copyright 2020-2021 Team Rabotora and Rabotora contributors. All rights reserved.
        ///
        ///Usage: Rabotora [options]
        ///Usage: Rabotora [path-to-game]
        ///
        ///Options:
        ///  -?              \tShow Command Line Help.
        ///  -ver|--version \tShow Rabotora SDK Version.
        ///  --info         \tShow Rabotora Configuration Details.
        ///
        ///path-to-game:
        ///  The absolute path of Rabotora-Runtime-Depended game main program file. 的本地化字符串。
        /// </summary>
        public static string CLI_BaseHelp {
            get {
                return ResourceManager.GetString("CLI.BaseHelp", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Rabotora Visual Novel Game Engine SDK
        ///(C)Copyright 2020-2021 Team Rabotora and Rabotora contributors. All rights reserved.
        ///
        ///Usage: Rabotora [runtime-options] [path-to-game]
        ///
        ///Start a Rabotora Game.
        ///
        ///runtime-options:
        ///  --nofullscr \tForce not to start the game in full screen.
        ///  --debug \tDebug the game (Rabotora Creator required).
        ///  
        ///path-to-game:
        ///  The absolute path of Rabotora-Runtime-Depended game main program file.
        ///
        ///Usage: Rabotora [sdk-options] [sdk-command] [arguments]
        ///
        ///Run a Rabotora SD [字符串的其余部分被截断]&quot;; 的本地化字符串。
        /// </summary>
        public static string CLI_Help {
            get {
                return ResourceManager.GetString("CLI.Help", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Error: &quot;{0}&quot; needs at least {1} argument(s) 的本地化字符串。
        /// </summary>
        public static string CLI_MissingArguments {
            get {
                return ResourceManager.GetString("CLI.MissingArguments", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Failed to create new game project: {0} 的本地化字符串。
        /// </summary>
        public static string CLI_ProjectManager_CreateFailed {
            get {
                return ResourceManager.GetString("CLI.ProjectManager.CreateFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Successfully created new game project {0} in {1} 的本地化字符串。
        /// </summary>
        public static string CLI_ProjectManager_CreateSuccess {
            get {
                return ResourceManager.GetString("CLI.ProjectManager.CreateSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Rabotora Runtime v{0}
        ///Rabotora SDK v{1} 的本地化字符串。
        /// </summary>
        public static string CLI_Version {
            get {
                return ResourceManager.GetString("CLI.Version", resourceCulture);
            }
        }
    }
}
