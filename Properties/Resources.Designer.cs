﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Mahi.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Mahi.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 123456789.
        /// </summary>
        internal static string CertificationPassword {
            get {
                return ResourceManager.GetString("CertificationPassword", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        internal static byte[] dkjson {
            get {
                object obj = ResourceManager.GetObject("dkjson", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        internal static byte[] hash {
            get {
                object obj = ResourceManager.GetObject("hash", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;!DOCTYPE html&gt;
        ///&lt;html lang=&quot;en&quot; dir=&quot;lrt&quot;&gt;
        ///&lt;head&gt;
        ///    &lt;meta charset=&quot;UTF-8&quot;&gt;
        ///    &lt;meta name=&quot;viewport&quot; content=&quot;width=device-width, initial-scale=1.0&quot;&gt;
        ///    &lt;title&gt;Not found&lt;/title&gt;
        ///    &lt;style&gt;
        ///        body {
        ///            text-align: center;
        ///        }
        ///
        ///        h1 {
        ///            color:rgb(107, 18, 190)
        ///        }
        ///    &lt;/style&gt;
        ///&lt;/head&gt;
        ///&lt;body&gt;
        ///    &lt;h1&gt;Error: The request url &quot;{Url}&quot; was not found!&lt;/h1&gt;
        ///    &lt;hr&gt;
        ///    &lt;p class=&quot;exception-message&quot;&gt;&lt;span style=&quot;font-weight: bold&quot;&gt;Description:&lt;/span&gt; {D [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Http404 {
            get {
                return ResourceManager.GetString("Http404", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;!DOCTYPE html&gt;
        ///&lt;html lang=&quot;en&quot; dir=&quot;lrt&quot;&gt;
        ///&lt;head&gt;
        ///    &lt;meta charset=&quot;UTF-8&quot;&gt;
        ///    &lt;meta name=&quot;viewport&quot; content=&quot;width=device-width, initial-scale=1.0&quot;&gt;
        ///    &lt;title&gt;{Error}&lt;/title&gt;
        ///    &lt;style&gt;
        ///        body,html{height:100%}h1{color:red;margin-top:1rem;margin-bottom:0.5rem;font-size:2.5rem}p{font-size:18px}.exception-type{margin-top:0}.exception-message{font-size:1.5rem}pre{background-color:#ffb;border:1px solid #f99;font-size:18px;padding:1.5rem 1rem}
        ///    &lt;/style&gt;
        ///&lt;/head&gt;
        ///&lt;body&gt;
        ///    &lt;h1&gt;{Error}&lt;/h1 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Http500 {
            get {
                return ResourceManager.GetString("Http500", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        internal static byte[] mssql {
            get {
                object obj = ResourceManager.GetObject("mssql", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 1.
        /// </summary>
        internal static string Version {
            get {
                return ResourceManager.GetString("Version", resourceCulture);
            }
        }
    }
}
